using System.IO.Compression;
using System.Security.Cryptography;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Core.Identity;
using GPC.Model.Models;
using GPC.Model.Structure.Assignments;

namespace UnitTest;

[TestClass]
public class Straus7PhysicalWriteTest
{
    private static (Model Model,Straus7PhysicalWritePlan Plan) Prepared()
    {
        var model=PhysicalAssignmentsTest.Sample();
        var beam=model.BeamElements.Values.Single();
        beam.Assignments.SectionAxes=new CoordinateSystem(beam.StartPoint,new Vector3d(0,1,0),new Vector3d(0,0,1));
        model.AreaElements.Values.Single().Assignments.Physical.InitialStresses[0].Components.Z=0;
        var map=model.AssemblyMap.ToSolver(model,"Straus7-R3",model.AllElements.Select(e=>new KeyValuePair<Guid,int>(e.Guid,e.Id)));
        return (model,Straus7PhysicalWritePlan.Prepare(model,map,new Dictionary<string,int>{{"physical",1}},new Dictionary<string,int>()));
    }
    [TestMethod] public void PhysicalPlanSeparatesFreedomCasesAndKeepsSamplingOutsideNativeWrites()
    {
        var (model,plan)=Prepared(); var capture=new Capture(); plan.Write(capture,model);
        Assert.AreEqual(9,plan.OperationCount); Assert.AreEqual(9,capture.Operations.Count);
        Assert.AreEqual(11,plan.MinimumOutputStations[1]);
        CollectionAssert.AreEqual(new double[]{1,0,1,1,50,2},capture.Operations["beam-support"]);
        CollectionAssert.AreEqual(new double[]{1,0,1,0,0.1,0},capture.Operations["plate-support"]);
        CollectionAssert.AreEqual(new double[]{1,1,2e-9,0.8,0,0,40},capture.Operations["mass"]);
    }
    [TestMethod] public void StaleInputAndUnsupportedThroughThicknessStressCauseNoWrites()
    {
        var (model,plan)=Prepared(); var capture=new Capture(); model.AreaElements.Values.Single().Assignments.Physical.InitialStresses[0].Components.Z=1;
        Assert.ThrowsException<InvalidOperationException>(()=>plan.Write(capture,model)); Assert.AreEqual(0,capture.Operations.Count);
        var map=model.AssemblyMap.ToSolver(model,"Straus7-R3",model.AllElements.Select(e=>new KeyValuePair<Guid,int>(e.Guid,e.Id)));
        Assert.ThrowsException<NotSupportedException>(()=>Straus7PhysicalWritePlan.Prepare(model,map,new Dictionary<string,int>{{"physical",1}},new Dictionary<string,int>()));
    }
    [TestMethod] public void MissingCaseAndUnsupportedSpringDirectionsAreRejectedBeforeWriting()
    {
        var (m,p)=Prepared(); var map=m.AssemblyMap.ToSolver(m,"Straus7-R3",m.AllElements.Select(e=>new KeyValuePair<Guid,int>(e.Guid,e.Id)));
        Assert.ThrowsException<ArgumentException>(()=>Straus7PhysicalWritePlan.Prepare(m,map,new Dictionary<string,int>(),new Dictionary<string,int>()));
        m.BeamElements.Values.Single().Assignments.Physical.Supports[0].Direction=new Vector3d(1,0,0);
        map=m.AssemblyMap.ToSolver(m,"Straus7-R3",m.AllElements.Select(e=>new KeyValuePair<Guid,int>(e.Guid,e.Id)));
        Assert.ThrowsException<NotSupportedException>(()=>Straus7PhysicalWritePlan.Prepare(m,map,new Dictionary<string,int>{{"physical",1}},new Dictionary<string,int>()));
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativePhysicalWriterPersistsAttributesAndLeavesSourceUntouched()
    {
        var root=Environment.GetEnvironmentVariable("GPC_STRAUS7_WRITE");
        if(string.IsNullOrWhiteSpace(root)) Assert.Inconclusive("Set GPC_STRAUS7_WRITE to a new artifact parent directory.");
        var folder=Path.Combine(Path.GetFullPath(root!),Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        ZipFile.ExtractToDirectory(Path.Combine(AppContext.BaseDirectory,"Fixtures","Straus7","native-stages-r31.zip"),folder);
        var source=Path.Combine(folder,"stages.st7"); var output=Path.Combine(folder,"physical.st7"); var before=SHA256.HashData(File.ReadAllBytes(source));
        var (model,plan)=Prepared(); int node=model.NodesElements.Values.Single(n=>n.Assignments.Physical!=null).Id;
        using(var writer=new Straus7NativeWriter(output,folder,source)) { plan.Write(writer,model); writer.Save(); }
        // A fresh native session opens the saved file. No call-capture result is used as its expected value.
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder);
            Assert.AreEqual(-10,reader.ReadNodeTemperature(node,1,out int temperatureType),1e-12); Assert.AreEqual(2,temperatureType);
            CollectionAssert.AreEqual(new[]{0.01,-0.02},reader.ReadBeamTemperatureGradient(1,1));
            Assert.AreEqual(12000,reader.ReadBeamInitialState(1,1,out int preload),1e-10); Assert.AreEqual(0,preload);
            CollectionAssert.AreEqual(new[]{50d,2},reader.ReadBeamSupport(1,0,1,out bool compression)); Assert.IsTrue(compression);
            CollectionAssert.AreEqual(new[]{0.8,1},reader.ReadBeamTaper(1,1,out int taper)); Assert.AreEqual(1,taper);
            CollectionAssert.AreEqual(new[]{1d,2,0},reader.ReadPlateInitialStress(1,1,out int plateKind)); Assert.AreEqual(0,plateKind);
            CollectionAssert.AreEqual(new[]{2e-9,0.8,0,0,40},reader.ReadPlateNonStructuralMass(1,1,1));
            CollectionAssert.AreEqual(new[]{0.1,0,0,0},reader.ReadPlateSupport(1,2,1,out int[] flags)); CollectionAssert.AreEqual(new[]{0,0},flags);
        }
        CollectionAssert.AreEqual(before,SHA256.HashData(File.ReadAllBytes(source)));
        Console.WriteLine("F5 native physical readback: "+output);
    }
    private sealed class Capture:IStraus7WriteApi
    {
        internal Dictionary<string,double[]> Operations=new();
        public void SetNodeTemperature(int n,int c,NodalTemperatureKind k,double v)=>Operations.Add("temperature",new[]{(double)n,c,(int)k,v});
        public void SetBeamTemperatureGradient(int n,int c,double x,double y)=>Operations.Add("gradient",new[]{(double)n,c,x,y});
        public void SetBeamInitialState(int n,int c,BeamInitialStateKind k,double v)=>Operations.Add("initial",new[]{(double)n,c,(int)k,v});
        public void SetBeamSupport(int n,int d,int c,bool only,double k,double g)=>Operations.Add("beam-support",new[]{(double)n,d,c,only?1:0,k,g});
        public void SetBeamTaper(int n,int a,TaperAlignment mode,double i,double j)=>Operations.Add("taper-"+a,new[]{(double)n,a,(int)mode,i,j});
        public void SetPlateInitialStress(int n,int c,double x,double y,double z)=>Operations.Add("plate-stress",new[]{(double)n,c,x,y,z});
        public void SetPlateNonStructuralMass(int n,int c,double m,double f,double x,double y,double z)=>Operations.Add("mass",new[]{(double)n,c,m,f,x,y,z});
        public void SetPlateSupport(int n,ShellFace f,int c,bool only,double k,double g)=>Operations.Add("plate-support",new[]{(double)n,(int)f,c,only?1:0,k,g});
    }
}
