using System.IO.Compression;
using System.Security.Cryptography;
using GPC.Converter.Straus7;
using GPC.Model.Core;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Stages;

namespace UnitTest;

[TestClass]
public class Straus7StageWriteTest
{
    private static Dictionary<Guid,int> Groups(Model m) => m.Groups.Values.Select((g,i) => (g.Guid,i)).ToDictionary(p => p.Guid,p => p.i+1);
    private static Dictionary<Guid,int> Loads(Model m) => m.LoadCases.Values.Select((c,i) => (c.Guid,i)).ToDictionary(p => p.Guid,p => p.i+1);
    private static Dictionary<Guid,int> Freedoms(Model m) => m.FreedomCases.Values.Select((c,i) => (c.Guid,i)).ToDictionary(p => p.Guid,p => p.i+1);
    private static Straus7StageWritePlan Plan(Model m) => Straus7StageWritePlan.Prepare(m,Groups(m),Loads(m),Freedoms(m));

    [TestMethod] public void StagePlanUsesFiveFlagsAndWritesCompleteTotalFactorMatrices()
    {
        var model = StageAnalysisContractsTest.Sample(); var plan = Plan(model); var api = new Capture(); plan.Write(api,model);
        Assert.AreEqual(1,plan.StageCount); Assert.AreEqual(2,plan.IncrementCount);
        CollectionAssert.AreEqual(new[] {1,1,1,1,0},api.Flags[1]); Assert.AreEqual(-1200d,api.Levels[1]);
        Assert.AreEqual(1.35,api.Factors[(1,1,1,false)]); Assert.AreEqual(-.25,api.Factors[(1,1,2,false)]);
        Assert.AreEqual(.5,api.Factors[(1,1,1,true)]);
        Assert.AreEqual(0d,api.Factors[(1,2,1,false)]); Assert.AreEqual(0d,api.Factors[(1,2,2,false)]);
        Assert.AreEqual(0d,api.Factors[(1,2,1,true)]); Assert.IsTrue(api.Cases[(1,1,true)]);
        Assert.AreEqual(86400d,api.Times[(1,2)]);
    }

    [TestMethod] public void PlanRejectsStaleInputAmbiguousMapsAndUnsupportedSchedulesBeforeWriting()
    {
        var model = StageAnalysisContractsTest.Sample(); var api = new Capture(); var plan = Plan(model);
        model.Stages[10].AnalysisState.Increments[0].LoadFactors[0].Factor = 2;
        Assert.ThrowsException<InvalidOperationException>(() => plan.Write(api,model)); Assert.AreEqual(0,api.Calls);
        var map = Loads(model); map[model.LoadCases["live"].Guid] = 1;
        Assert.ThrowsException<ArgumentException>(() => Straus7StageWritePlan.Prepare(model,Groups(model),map,Freedoms(model)));
        model.Stages[10].Definition = new StageDefinition(1,7);
        Assert.ThrowsException<NotSupportedException>(() => Plan(model));
        model.Stages[10].Definition = new StageDefinition(1,0);
        model.StageCombinationsMap[10] = new HashSet<string> {"legacy"};
        Assert.ThrowsException<NotSupportedException>(() => Plan(model)); Assert.AreEqual(0,api.Calls);
        model=StageAnalysisContractsTest.Sample(); plan=Plan(model); var previous=model.Stages[10];
        model.Stages.Remove(10); model.Stages.Add(new Stage(10,previous.Name) {Definition=previous.Definition,AnalysisState=previous.AnalysisState});
        Assert.AreEqual(plan.AnalysisFingerprint,model.AnalysisFingerprint(),"A new stage identity can retain the same physical input fingerprint.");
        Assert.ThrowsException<InvalidOperationException>(()=>plan.Write(api,model)); Assert.AreEqual(0,api.Calls);
    }

    [TestMethod] public void StageOrderUsesSequenceAndPreflightFailureCannotPartiallyWrite()
    {
        var model = StageAnalysisContractsTest.Sample(); var earlier = ModelValues.CopyValue(model.Stages[10].AnalysisState);
        earlier.BirthGeometry = StageBirthGeometry.OriginalCoordinates; earlier.MoveRestrainedNodes = false;
        earlier.RotateIncomingClusters = false; earlier.FluidLevelMm = null;
        model.Stages.Add(new Stage(99,"first") {Definition=new StageDefinition(1,0),AnalysisState=earlier});
        var plan = Plan(model); var api = new Capture(); plan.Write(api,model);
        Assert.AreEqual("first",api.Names[1]); Assert.AreEqual("erection",api.Names[2]);
        Assert.AreEqual(1,plan.StageIds[model.Stages[99].Guid]); Assert.AreEqual(2,plan.StageIds[model.Stages[10].Guid]);
        CollectionAssert.AreEqual(new[] {0,0,0,0,0},api.Flags[1]);
        api = new Capture {RejectInventory=true}; Assert.ThrowsException<InvalidOperationException>(() => plan.Write(api,model)); Assert.AreEqual(1,api.Calls);
        model.Stages[99].Definition = new StageDefinition(2,0); Assert.ThrowsException<ArgumentException>(() => Plan(model));
    }

    [TestMethod] public void PlanDetachesCallerMapsAndDoesNotReuseMutableNativeArrays()
    {
        var model=StageAnalysisContractsTest.Sample(); var groups=Groups(model); var loads=Loads(model); var freedoms=Freedoms(model);
        var plan=Straus7StageWritePlan.Prepare(model,groups,loads,freedoms); groups.Clear(); loads.Clear(); freedoms.Clear();
        var first=new Capture(); plan.Write(first,model); first.Flags[1][0]=77;
        var second=new Capture(); plan.Write(second,model); Assert.AreEqual(1,second.Flags[1][0]);
    }

    [TestMethod,TestCategory("Straus7Native")]
    public void NativeStageWriterPersistsMorphResetActivationAndTotalIncrementFactors()
    {
        var root=Environment.GetEnvironmentVariable("GPC_STRAUS7_WRITE");
        if(string.IsNullOrWhiteSpace(root)) Assert.Inconclusive("Set GPC_STRAUS7_WRITE to a new artifact parent directory.");
        var folder=Path.Combine(Path.GetFullPath(root!),Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        ZipFile.ExtractToDirectory(Path.Combine(AppContext.BaseDirectory,"Fixtures","Straus7","native-elements-r31.zip"),folder);
        var source=Path.Combine(folder,"elements.st7"); var output=Path.Combine(folder,"written-stages.st7"); var hash=SHA256.HashData(File.ReadAllBytes(source));
        int[] nativeGroups;
        using(var writer=new Straus7NativeWriter(output,folder,source))
        {
            var reader=writer.Reader; var model=new Model("native stage readback"); var groups=new Dictionary<Guid,int>();
            for(int i=1;i<=reader.GroupCount;i++) {var g=reader.ReadGroup(i);groups.Add(model.AddGroup(g.Name).Guid,g.Id);}
            nativeGroups=groups.Values.OrderBy(i=>i).ToArray(); var loads=new Dictionary<Guid,int>(); var freedoms=new Dictionary<Guid,int>();
            for(int i=1;i<=reader.LoadCaseCount;i++) {var c=new LoadCaseBase("load "+i);model.AddLoadCase(c);loads.Add(c.Guid,i);}
            for(int i=1;i<=reader.FreedomCaseCount;i++) {var c=new FreedomCase("freedom "+i);model.AddFreedomCase(c);freedoms.Add(c.Guid,i);}
            Assert.IsTrue(loads.Count>=2 && freedoms.Count>=1);
            for(int i=1;i<=3;i++)
            {
                var state=new StageAnalysisState {BirthGeometry=(StageBirthGeometry)(i-1),MoveRestrainedNodes=i==2,RotateIncomingClusters=i==2,
                    FluidLevelMm=i==2?-1200d:null,ActiveGroupGuids=groups.Keys.ToArray(),ActiveFreedomCaseGuids=freedoms.Keys.ToArray(),
                    Increments=new[] {new StageLoadIncrement {Name="load",PseudoTimeSeconds=0,LoadFactors=new[] {
                        new StageCaseFactor {CaseGuid=loads.Single(p=>p.Value==1).Key,Factor=1.35},
                        new StageCaseFactor {CaseGuid=loads.Single(p=>p.Value==2).Key,Factor=-.25}},
                        FreedomFactors=new[] {new StageCaseFactor {CaseGuid=freedoms.Single(p=>p.Value==1).Key,Factor=.5}}},
                        new StageLoadIncrement {Name="unload",PseudoTimeSeconds=86400}}};
                if(i==3)
                {
                    state.ActiveFreedomCaseGuids=Array.Empty<Guid>(); state.ActiveGroupGuids=Array.Empty<Guid>();
                    state.Increments[0].LoadFactors=state.Increments[0].LoadFactors.Take(1).ToArray();
                    foreach(var increment in state.Increments) {increment.FreedomFactors=Array.Empty<StageCaseFactor>();increment.PseudoTimeSeconds=null;}
                }
                model.Stages.Add(new Stage(i,"stage "+i) {Definition=new StageDefinition(i,0),AnalysisState=state});
            }
            var plan=Straus7StageWritePlan.Prepare(model,groups,loads,freedoms);plan.Write(writer,model);writer.Save();
            Assert.ThrowsException<InvalidOperationException>(()=>plan.Write(writer,model)); Assert.AreEqual(3,reader.StageCount);
        }
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder); Assert.IsTrue(reader.ReadStagedAnalysis()); Assert.AreEqual(3,reader.StageCount);
            for(int i=1;i<=3;i++)
            {
                var stage=reader.ReadStage(i,nativeGroups); Assert.AreEqual("stage "+i,stage.Name); CollectionAssert.AreEqual(i==3?Array.Empty<int>():nativeGroups,stage.EnabledGroups);
                CollectionAssert.AreEqual(i==1?new[]{0,0,0,0,0}:i==2?new[]{1,1,1,1,0}:new[]{0,0,0,0,1},stage.Data);
                Assert.IsTrue(reader.ReadStageEnabled(i)); Assert.AreEqual(2,reader.ReadStageIncrementCount(i));
                Assert.AreEqual("unload",reader.ReadStageIncrementName(i,2)); Assert.AreEqual(i!=3,reader.ReadStagePseudoTimeEnabled(i));
                if(i!=3) Assert.AreEqual(86400d,reader.ReadStagePseudoTime(i,2));
                Assert.AreEqual(i!=3,reader.ReadStageCaseEnabled(i,1,true)); Assert.AreEqual(i!=3,reader.ReadStageCaseEnabled(i,2,false));
                Assert.AreEqual(1.35,reader.ReadStageFactor(i,1,1,false)); Assert.AreEqual(i==3?0:-.25,reader.ReadStageFactor(i,1,2,false));
                Assert.AreEqual(i==3?0:.5,reader.ReadStageFactor(i,1,1,true)); Assert.AreEqual(0d,reader.ReadStageFactor(i,2,1,false));
                Assert.AreEqual(0d,reader.ReadStageFactor(i,2,2,false)); Assert.AreEqual(0d,reader.ReadStageFactor(i,2,1,true));
            }
            Assert.AreEqual(-1200d,reader.ReadStageFluidLevel(2));
        }
        CollectionAssert.AreEqual(hash,SHA256.HashData(File.ReadAllBytes(source))); Console.WriteLine("F5 native stages readback: "+output);
    }

    private sealed class Capture:IStraus7StageWriteApi
    {
        internal int Calls; internal bool RejectInventory;
        internal Dictionary<int,int[]> Flags=new(); internal Dictionary<int,string> Names=new(); internal Dictionary<int,double?> Levels=new();
        internal Dictionary<(int,int,bool),bool> Cases=new(); internal Dictionary<(int,int,int,bool),double> Factors=new();
        internal Dictionary<(int,int),double?> Times=new();
        public void RequireStageInputs(int[] g,int[] l,int[] f) {Calls++;if(RejectInventory)throw new InvalidOperationException("inventory");}
        public void SetStagedAnalysis() {Calls++;}
        public void CreateStage(int n,string name,int[] flags,double? level) {Calls++;Flags.Add(n,flags);Names.Add(n,name);Levels.Add(n,level);}
        public void SetStageGroup(int stage,int group,bool active) {Calls++;}
        public void SetStageCase(int s,int c,bool freedom,bool active) {Calls++;Cases.Add((s,c,freedom),active);}
        public void SetStagePseudoTime(int stage,bool enabled) {Calls++;}
        public void AddStageIncrement(int s,int i,string name,double? time) {Calls++;Times.Add((s,i),time);}
        public void SetStageFactor(int s,int i,int c,bool freedom,double factor) {Calls++;Factors.Add((s,i,c,freedom),factor);}
    }
}
