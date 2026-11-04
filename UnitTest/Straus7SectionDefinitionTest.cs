using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Model.Core;
using GPC.Model.ElementProperties;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Loads;
using GPC.Model.Persistence;
using GPC.Model.Sections;
using GPC.Model.Structure.Assignments;

namespace UnitTest;

[TestClass]
public class Straus7SectionDefinitionTest
{
    private static string Folder()
    {
        string root=Environment.GetEnvironmentVariable("GPC_STRAUS7_WRITE")!;
        if(string.IsNullOrWhiteSpace(root))Assert.Inconclusive("Set GPC_STRAUS7_WRITE to an artifact directory.");
        string folder=Path.Combine(root,"native-section-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);return folder;
    }
    [TestMethod]
    public void SectionSnapshotCodecPreservesBitsAndRejectsInvalidPayloads()
    {
        var original=Straus7SectionDefinition.Standard(3,new[]{20d,10,BitConverter.Int64BitsToDouble(long.MinValue),0,0,0});
        var bytes=original.Save();var copy=Straus7SectionDefinition.Load(bytes);
        Assert.AreEqual(original.Fingerprint(),copy.Fingerprint());Assert.AreEqual(long.MinValue,BitConverter.DoubleToInt64Bits(copy.Dimensions[2]));
        var dimensions=copy.Dimensions;dimensions[0]=500;Assert.AreEqual(20,copy.Dimensions[0]);
        bytes[4]=2;Assert.ThrowsException<InvalidDataException>(()=>Straus7SectionDefinition.Load(bytes));
        Assert.ThrowsException<InvalidDataException>(()=>Straus7SectionDefinition.Load(original.Save().Concat(new byte[]{1}).ToArray()));
        Assert.ThrowsException<ArgumentException>(()=>Straus7SectionDefinition.Standard(3,new[]{double.NaN,1,0,0,0,0}));
    }
    [DataTestMethod,DataRow(false),DataRow(true),TestCategory("Straus7Native")]
    public void ResolvedNativeHollowSectionKeepsItsTaperRuleAndAnalyticalAxialResponse(bool bgl)
    {
        string folder=Folder();
        var input=bgl?Straus7SectionCatalog.Get("BglRectangle").Create(new[]{100d,50,5,5,0,0}):Straus7SectionCatalog.Get("RectangleHollow").Create(new[]{50d,100,5,5});
        var definition=Straus7SectionLibrary.Resolve(input,folder);
        Assert.AreEqual(bgl,definition.IsBgl);Assert.AreEqual(1400,definition.ReferenceProperties[0],1e-8);
        var model=ElasticExportContractsTest.Cantilever();var beam=model.BeamElements.Values.Single();
        beam.BeamProperty=definition.ToBeamProperty("native hollow",new Material("E1000",1000,.3,0,0));
        beam.Assignments.Physical.Taper=new BeamSectionTaper {ScaleJ1=2,ScaleJ2=2,Alignment1=TaperAlignment.Symmetric,Alignment2=TaperAlignment.Symmetric};
        var free=beam.NodeJ;free.Loads.Clear();free.Loads.Add(new PointLoad(1000,0,0,0,0,0,free.Position,model.LoadCases.Values.Single()));
        string fingerprint=model.AnalysisFingerprint();var restored=Straus7SectionDefinition.Load(definition.Save());
        var plan=Straus7ModelWritePlan.Prepare(model,null,new Dictionary<Guid,int>{{beam.Guid,10}},new Dictionary<Guid,Straus7SectionDefinition>{{beam.Guid,restored}});
        var run=plan.RunLinearStatic(model,Path.Combine(folder,"taper.st7"),folder);
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath,folder);var captured=reader.ReadSectionDefinition(1,folder);
            Assert.AreEqual(bgl,captured.IsBgl);CollectionAssert.AreEqual(definition.Dimensions,captured.Dimensions);
            reader.OpenResults(run.ResultPath);
            // Standard: walls stay 5 mm, A(t)=1400+1500*t. BGL scales all dimensions, A(t)=1400*(1+t)^2.
            double compliance=bgl?1d/2800:Math.Log(2900d/1400)/1500;
            double expected=1000*1000d/1000*compliance;
            Assert.AreEqual(expected,reader.ReadNodeResult(free.Id,1,false).Values[0],expected*5e-5);
        }
        Assert.AreEqual(ImportStatus.Completed,run.ImportResults(model,"native section").Status);
        Assert.AreEqual(plan.ConfigurationFingerprint,model.Datasets["native section"].SourceHashes["section-export-configuration"]);
        Assert.IsTrue(model.PreservedSourceData.Any(p=>p.SourceRecord.StartsWith("NativeSectionDefinitions/",StringComparison.Ordinal)));
        using(var stream=new MemoryStream()){ModelArchive.SaveDocument(model,stream);stream.Position=0;Assert.AreEqual(fingerprint,ModelArchive.Load(stream).AnalysisFingerprint());}
        Assert.AreEqual(fingerprint,model.AnalysisFingerprint());
        Assert.AreEqual(0,Directory.GetFiles(folder,"section-*.bxs").Length);
    }
    [DataTestMethod,DataRow(false),DataRow(true),TestCategory("Straus7Native")]
    public void InstalledSectionLibrariesAreExpandedAndRewrittenWithoutLibraryLookup(bool bgl)
    {
        string folder=Folder(),library,item;
        using(var api=new Straus7NativeApi())
        {
            var entry=api.SectionLibraryCatalog(bgl).First(p=>p.Value.Length!=0);library=entry.Key;item=entry.Value[0];
        }
        var section=Straus7SectionLibrary.ResolveLibrary(bgl,library,item,folder);
        var model=ElasticExportContractsTest.Cantilever();var beam=model.BeamElements.Values.Single();beam.BeamProperty=section.ToBeamProperty(item,new Material("E",210000,.3,0,0));
        string path=Path.Combine(folder,"library-expanded.st7");
        var plan=Straus7ModelWritePlan.Prepare(model,null,null,new Dictionary<Guid,Straus7SectionDefinition>{{beam.Guid,Straus7SectionDefinition.Load(section.Save())}});
        using(var writer=new Straus7NativeWriter(path,folder)){plan.Write(writer,model);writer.Save();}
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(path,folder);var read=reader.ReadSectionDefinition(1,folder);
            CollectionAssert.AreEqual(section.Dimensions,read.Dimensions);
            for(int i=0;i<11;i++)Assert.AreEqual(section.ReferenceProperties[i],read.ReferenceProperties[i],1e-8*Math.Max(1,Math.Abs(section.ReferenceProperties[i])));
        }
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void EmbeddedBxsSurvivesRemovalOfTheSourceModelAndRetainsSectionProperties()
    {
        string folder=Folder(),path=Path.Combine(folder,"source.st7");var model=ElasticExportContractsTest.Cantilever();var beam=model.BeamElements.Values.Single();
        beam.BeamProperty=new ElasticBeamProperty("rotated",new Material("E",210000,.3,0,0),new SectionRectangular(20,10,.17));
        using(var writer=new Straus7NativeWriter(path,folder)){Straus7ModelWritePlan.Prepare(model).Write(writer,model);writer.Save();}
        Straus7SectionDefinition definition;
        using(var reader=new Straus7NativeApi()){reader.OpenModelReadOnly(path,folder);definition=reader.ReadSectionDefinition(1,folder);}
        Assert.IsTrue(definition.BxsData.Length>0);File.Delete(path);
        var copy=Straus7SectionDefinition.Load(definition.Save());var expanded=Straus7SectionLibrary.Resolve(copy,folder);
        Assert.AreEqual(11,expanded.Shape);Assert.AreEqual(200,expanded.ReferenceProperties[0],1e-8);
        beam.BeamProperty=copy.ToBeamProperty("restored",new Material("E",210000,.3,0,0));
        var plan=Straus7ModelWritePlan.Prepare(model,null,null,new Dictionary<Guid,Straus7SectionDefinition>{{beam.Guid,copy}});
        var run=plan.RunLinearStatic(model,Path.Combine(folder,"restored.st7"),folder);
        Assert.AreEqual(ImportStatus.Completed,run.ImportResults(model,"BXS").Status);
        Assert.AreEqual(0,Directory.GetFiles(folder,"section-*.bxs").Length);
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void SectionMeshIsADisplayTriangulationOfTheSectionAndLeavesNoFiles()
    {
        string folder=Folder();
        static double Area(Straus7SectionMesh mesh)
        {
            var v=mesh.Vertices;double area=0;
            foreach(var f in mesh.Faces)for(int k=1;k+1<f.Length;k++)
                area+=((v[f[k]][0]-v[f[0]][0])*(v[f[k+1]][1]-v[f[0]][1])-(v[f[k+1]][0]-v[f[0]][0])*(v[f[k]][1]-v[f[0]][1]))/2;
            return Math.Abs(area);
        }
        var rectangle=Straus7SectionLibrary.Mesh(Straus7SectionCatalog.Get("RectangleSolid").Create(new[]{20d,10}),folder);
        Assert.AreEqual(200,Area(rectangle),1e-9);Assert.IsTrue(rectangle.Vertices.All(p=>p.Length==3&&p[2]==0));
        Assert.AreEqual(200,rectangle.Section.ReferenceProperties[0],1e-9);
        var input=Straus7SectionCatalog.Get("CircularHollow").Create(new[]{100d,10});
        var resolved=Straus7SectionLibrary.Resolve(input,folder);
        var tube=Straus7SectionLibrary.Mesh(input,folder,6);
        // Properties read before the display discretisation: those of Resolve.
        CollectionAssert.AreEqual(resolved.ReferenceProperties,tube.Section.ReferenceProperties);
        Assert.IsTrue(Straus7SectionLibrary.Mesh(resolved,folder,6).Faces.Length==tube.Faces.Length);
        // A coarse display polygon: within 3 % of the exact area that stays the analysis value.
        Assert.AreEqual(resolved.ReferenceProperties[0],Area(tube),.03*resolved.ReferenceProperties[0]);
        string library,item;
        using(var api=new Straus7NativeApi()){var entry=api.SectionLibraryCatalog(false).First(p=>p.Value.Length!=0);library=entry.Key;item=entry.Value[0];}
        var fromLibrary=Straus7SectionLibrary.MeshLibrary(false,library,item,folder);
        CollectionAssert.AreEqual(Straus7SectionLibrary.ResolveLibrary(false,library,item,folder).ReferenceProperties,fromLibrary.Section.ReferenceProperties);
        Assert.AreEqual(fromLibrary.Section.ReferenceProperties[0],Area(fromLibrary),.03*fromLibrary.Section.ReferenceProperties[0]);
        Assert.AreEqual(0,Directory.GetFiles(folder).Length);
        Assert.ThrowsException<ArgumentOutOfRangeException>(()=>Straus7SectionLibrary.Mesh(resolved,folder,-1));
    }
}
