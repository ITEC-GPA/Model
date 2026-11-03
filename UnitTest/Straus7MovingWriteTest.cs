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
    [DataTestMethod,DataRow("EN1991 General"),DataRow("BS5400 Centrifugal"),TestCategory("Straus7Native")]
    public void NativeLibraryIsExpandedIntoIndependentDomainInput(string requestedLibrary)
    {
        var root=Environment.GetEnvironmentVariable("GPC_STRAUS7_WRITE");if(string.IsNullOrWhiteSpace(root))Assert.Inconclusive("Set GPC_STRAUS7_WRITE.");
        string folder=Path.Combine(Path.GetFullPath(root!),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        KeyValuePair<string,string[]> library;
        using(var reader=new Straus7NativeApi())library=reader.MovingLibraryCatalog().Single(p=>p.Key.Contains(requestedLibrary));
        var resolved=Straus7MovingLibrary.Resolve(library.Key,library.Value[0],folder);resolved.Template.Validate();
        Assert.IsTrue(resolved.Template.Vehicles.Length>0);Assert.AreEqual(library.Key,resolved.LibraryName);
        var model=MovingLoadContractsTest.Sample();var path=model.MovingLoads.Paths[0];resolved.AttachTo(model,path.Id); // B8: the template brings its R3 expression record
        string fingerprint=ModelValues.Fingerprint(new object[]{resolved.Template}),output=Path.Combine(folder,"expanded-library.st7");
        var plan=Straus7MovingWritePlan.Prepare(model,Cases(model),new Dictionary<Guid,Straus7MovingPathOptions>{{path.Id,resolved.Options}});
        using(var writer=new Straus7NativeWriter(output,folder))
        {
            writer.CreateLoadCase(1,"traffic");plan.Write(writer,model);writer.Save();
        }
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder);var reread=reader.ReadExpandedMovingTemplate(1);
            Assert.AreEqual(fingerprint,ModelValues.Fingerprint(new object[]{reread.Template}));
            Assert.AreEqual(resolved.Options.RelativeTolerance,reread.Options.RelativeTolerance);
        }
        Console.WriteLine("F5 expanded library: "+library.Key+" / "+library.Value[0]+"; "+output);
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeAdvancedMovingLoadsPreserveHeatCentrifugalAndSelectedElements()
    {
        var root=Environment.GetEnvironmentVariable("GPC_STRAUS7_WRITE");if(string.IsNullOrWhiteSpace(root))Assert.Inconclusive("Set GPC_STRAUS7_WRITE.");
        string folder=Path.Combine(Path.GetFullPath(root!),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        string output=Path.Combine(folder,"advanced-moving.st7");
        var model=MovingLoadContractsTest.Sample();MovingLoadContractsTest.AddAdvancedLoads(model);var path=model.MovingLoads.Paths[0];
        var beam=model.BeamElements.Values.Single();beam.BeamProperty=new GPC.Model.ElementProperties.ElasticBeamProperty("beam",new GPC.Model.Materials.Material("steel",210000,.3,7.85e-9,0),new GPC.Model.Sections.SectionRectangular(100,100));
        beam.Assignments.SectionAxes=new GPC.Geometry.CoordinateSystem(beam.StartPoint,new GPC.Geometry.Vector3d(0,1,0),new GPC.Geometry.Vector3d(0,0,1));
        path.Target=MovingPathTarget.Selection;path.TargetElementGuids=new[]{beam.Guid};
        var plan=Straus7ModelWritePlan.Prepare(model);
        using(var writer=new Straus7NativeWriter(output,folder)){plan.Write(writer,model);writer.Save();}
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder);reader.ReadMovingPath(1,out var flags);Assert.AreEqual(1,flags[4]);Assert.AreEqual(1,flags[6]);
            Assert.IsTrue(reader.IsInMovingEntitySet(1,1,1));Assert.IsFalse(reader.IsInMovingEntitySet(0,1,1));
            CollectionAssert.AreEqual(new[]{100d,20,50,100,250},reader.ReadMovingHeatSource(1,1,1));
            // B8: the N-mm GPC law is written in mm and N (before: the text law of the test in m and kN).
            var law=reader.ReadMovingCentrifugal(1);Assert.AreEqual("V^2/(g*R)",law.FactorExpression);Assert.AreEqual("mm",law.LengthUnit);Assert.AreEqual("N",law.ForceUnit);Assert.AreEqual(.2,law.MaximumFactor);
            Assert.AreEqual(MovingCentrifugalFactorKind.SpeedSquaredOverGravityRadius,law.ToDomain(out _).FactorKind);
            reader.ReadMovingPointForce(1,1,1,out var forceFlags);Assert.AreEqual(1,forceFlags[3]);
        }
        Console.WriteLine("F5 advanced moving readback: "+output);
    }
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
