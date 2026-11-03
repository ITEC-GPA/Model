using System.IO.Compression;
using System.Security.Cryptography;
using GPC.Converter.Straus7;
using GPC.Model.Actions.Moving;
using GPC.Model.Core;
using GPC.Model.Models;

namespace UnitTest;

[TestClass]
public class Straus7MovingWriteTest
{
    private static Dictionary<Guid,int> Cases(Model model) => new() {{model.LoadCases.Values.Single().Guid,1}};
    [TestMethod] public void PlanDeduplicatesTemplatesAndSeparatesNativeOptionsFromPhysics()
    {
        var model=MovingLoadContractsTest.Sample(); var second=ModelValues.CopyValue(model.MovingLoads.Paths[0]); second.Id=Guid.NewGuid(); second.Start.Y+=10; second.End.Y+=10;
        model.MovingLoads.Paths.Add(second); var options=new Straus7MovingPathOptions {Colour=123,RelativeTolerance=.02};
        var plan=Straus7MovingWritePlan.Prepare(model,Cases(model),model.MovingLoads.Paths.ToDictionary(p=>p.Id,p=>options));
        options.Colour=321; options.RelativeTolerance=.5;
        var api=new Capture(); Assert.AreEqual(0,plan.Write(api,model).Count);
        Assert.AreEqual(1,plan.TemplateCount); Assert.AreEqual(2,plan.PathCount); Assert.AreEqual(1,api.Templates);
        CollectionAssert.AreEqual(new[]{123,2,0,0,0},api.TemplateFlags);
        CollectionAssert.AreEqual(new[]{.02,3000},api.TemplateValues);
        CollectionAssert.AreEqual(new[]{0d,1000,-500,500,0,0,-2},api.Distributed);
        CollectionAssert.AreEqual(new[]{1,1,0,0,2,20,0},api.PathFlags);
    }
    [TestMethod] public void StalePlansInvalidCaseMapsAndFloatingStaticConversionCannotWrite()
    {
        var model=MovingLoadContractsTest.Sample(); var api=new Capture(); var path=model.MovingLoads.Paths[0];
        var plan=Straus7MovingWritePlan.Prepare(model,Cases(model)); path.Divisions++;
        Assert.ThrowsException<InvalidOperationException>(()=>plan.Write(api,model)); Assert.AreEqual(0,api.Calls);
        Assert.ThrowsException<ArgumentException>(()=>Straus7MovingWritePlan.Prepare(model,new Dictionary<Guid,int>()));
        path.Template.Vehicles[0].PointForces[0].Mobility=MovingPointMobility.Floating;
        var options=new Dictionary<Guid,Straus7MovingPathOptions>{{path.Id,new Straus7MovingPathOptions {ConvertToStaticCases=true}}};
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(()=>Straus7MovingWritePlan.Prepare(model,Cases(model),options)).Message,"WouldOmitFloatingLoads");
        Assert.AreEqual(0,api.Calls);
    }
    [TestMethod] public void StaticConversionReturnsGeneratedCasesOnlyAfterAllPaths()
    {
        var model=MovingLoadContractsTest.Sample(); var path=model.MovingLoads.Paths[0];
        var plan=Straus7MovingWritePlan.Prepare(model,Cases(model),new Dictionary<Guid,Straus7MovingPathOptions>{{path.Id,new Straus7MovingPathOptions {ConvertToStaticCases=true}}});
        var api=new Capture(); var generated=plan.Write(api,model);
        Assert.IsTrue(api.ConvertSelected); Assert.IsTrue(api.PathWrittenBeforeConversion); Assert.AreEqual("position 1",generated[9]);
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeMovingWriterPersistsVehiclePathAndForceUnits()
    {
        var root=Environment.GetEnvironmentVariable("GPC_STRAUS7_WRITE");
        if(string.IsNullOrWhiteSpace(root)) Assert.Inconclusive("Set GPC_STRAUS7_WRITE to a new artifact parent directory.");
        var folder=Path.Combine(Path.GetFullPath(root!),Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        ZipFile.ExtractToDirectory(Path.Combine(AppContext.BaseDirectory,"Fixtures","Straus7","native-stages-r31.zip"),folder);
        var source=Path.Combine(folder,"stages.st7"); var output=Path.Combine(folder,"moving.st7"); var before=SHA256.HashData(File.ReadAllBytes(source));
        var model=MovingLoadContractsTest.Sample(); var plan=Straus7MovingWritePlan.Prepare(model,Cases(model));
        using(var writer=new Straus7NativeWriter(output,folder,source)) { plan.Write(writer,model); writer.Save(); }
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder); Assert.AreEqual(1,reader.MovingTemplateCount);
            CollectionAssert.AreEqual(new[]{0d,0,0,1000,0,0,0,100,0},reader.ReadMovingPath(1,out var flags));
            CollectionAssert.AreEqual(new[]{1,1,0,0,2,20,0},flags);
            CollectionAssert.AreEqual(new[]{.01,3000},reader.ReadMovingTemplate(1,out var templateFlags)); CollectionAssert.AreEqual(new[]{255,2,0,0,0},templateFlags);
            Assert.AreEqual(.8,reader.ReadMovingLaneFactor(1,2));
            CollectionAssert.AreEqual(new[]{10000d,-2},reader.ReadMovingVehicle(1,1,out var vehicleFlags)); CollectionAssert.AreEqual(new[]{0,0},vehicleFlags);
            CollectionAssert.AreEqual(new[]{0d,0,0,0,-100000},reader.ReadMovingPointForce(1,1,1,out var pointFlags)); CollectionAssert.AreEqual(new[]{0,0,1,0},pointFlags);
            CollectionAssert.AreEqual(new[]{0d,1000,-500,500,0,0,-2},reader.ReadMovingDistributedForce(1,1,1,out var distributedFlags)); CollectionAssert.AreEqual(new[]{0,0,0,0},distributedFlags);
        }
        CollectionAssert.AreEqual(before,SHA256.HashData(File.ReadAllBytes(source)));
        Console.WriteLine("F5 native moving readback: "+output);
    }
    private sealed class Capture:IStraus7MovingWriteApi
    {
        internal int Calls,Templates; internal int[] TemplateFlags=Array.Empty<int>(),PathFlags=Array.Empty<int>();
        internal double[] TemplateValues=Array.Empty<double>(),Distributed=Array.Empty<double>(); internal bool ConvertSelected,PathWrittenBeforeConversion;
        public void RequireEmptyMovingLoads() { Calls++; }
        public void CreateMovingTemplate(int id,string name,int[] flags,double[] values) { Calls++; Templates++; TemplateFlags=flags; TemplateValues=values; }
        public void SetMovingLaneFactor(int template,int lane,double factor) { Calls++; }
        public void AddMovingVehicle(int template,int vehicle,string name,int[] flags,double[] values,string exclusiveSet,bool[] lanes) { Calls++; }
        public void AddMovingPointForce(int template,int vehicle,int position,int[] flags,double[] values) { Calls++; }
        public void AddMovingDistributedForce(int template,int vehicle,int position,int[] flags,double[] values) { Calls++; Distributed=values; }
        public void SetMovingPath(int id,int[] flags,double[] coordinates,bool convertToStatic) { Calls++; PathFlags=flags; ConvertSelected=convertToStatic; }
        public IReadOnlyDictionary<int,string> ConvertSelectedMovingPaths() { Calls++; PathWrittenBeforeConversion=PathFlags.Length==7; return new Dictionary<int,string>{{9,"position 1"}}; }
    }
}
