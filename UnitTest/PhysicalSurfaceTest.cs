using System.Runtime.Serialization;
using System.Xml.Linq;
using GPC.Examples;
using GPC.Geometry;
using GPC.Model.Checking;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Structure;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using GPC.Model.Data.Steel;

namespace UnitTest;

[TestClass]
public class PhysicalSurfaceTest
{
    internal static PhysicalSurfaceDefinition Surface() => new("wall",new[]{1090,1130},new[]{
        new SurfaceZoneDefinition("left",new[]{1090}),new SurfaceZoneDefinition("right",new[]{1130})},"Wall","explicit mesh association");
    private static GPC.Model.Models.Model Model()
    {
        var model=MixedModelFactory.Create(reinforcementRole:ReinforcementAnalysisRole.ExcludedFromAnalysis);
        model.PhysicalSurfaces.Add("wall",Surface()); return model;
    }
    [TestMethod]
    public void SurfaceContainsFemPlatesAndZonesSelectCompleteElements()
    {
        var model=Model(); var surface=model.PhysicalSurfaces["wall"];
        Assert.AreEqual(2,surface.ResolveElements(model).Count);
        Assert.AreEqual(1090,surface.ResolveElements(model,"left").Single().Id);
        Assert.AreEqual("right",surface.ZoneAt(1130));
        Assert.AreSame(surface,model.SurfaceForElement(1090));
        Assert.AreEqual(0,model.ValidatePhysicalSurfaces().Count);
        Assert.ThrowsException<ArgumentException>(()=>surface.ResolveElements(model,"unknown"));
    }
    [TestMethod]
    public void InvalidAndOverlappingZonesAreRejectedAtDefinitionTime()
    {
        Assert.ThrowsException<ArgumentException>(()=>new PhysicalSurfaceDefinition("x",new[]{1090,1090}));
        Assert.ThrowsException<ArgumentException>(()=>new PhysicalSurfaceDefinition("x",new[]{1090},new[]{new SurfaceZoneDefinition("a",new[]{1130})}));
        Assert.ThrowsException<ArgumentException>(()=>new PhysicalSurfaceDefinition("x",new[]{1090,1130},new[]{new SurfaceZoneDefinition("a",new[]{1090}),new SurfaceZoneDefinition("b",new[]{1090})}));
        var ids=new[]{1090}; var zone=new SurfaceZoneDefinition("a",ids); ids[0]=999;
        Assert.AreEqual(1090,zone.ElementIds[0]);
    }
    [TestMethod]
    public void ZonesAssignOneOwnedSectionPerFemPlateWithoutDuplicatingPropertiesInZones()
    {
        var model=Model(); var shell=model.AreaElements[1090]; var previous=model.AreaElements[1130].PlateProperty;
        var source=ModelArchive.Copy(model); var section=(ReinforcedConcretePlateSection)PlateSections.UpgradeLegacy(source.AreaElements[1090]);
        section.RebarLayers[0].Pitch=200;
        var changed=model.AssignSurfaceSection("wall",section,"left");
        CollectionAssert.AreEqual(new[]{1090},changed.ToArray());
        Assert.AreSame(previous,model.AreaElements[1130].PlateProperty);
        Assert.AreNotSame(section,shell.PlateProperty); Assert.AreEqual(0,shell.Assignments.Layers.Count);
        section.RebarLayers[0].Pitch=400;
        Assert.AreEqual(200,((ReinforcedConcretePlateSection)shell.PlateProperty).RebarLayers[0].Pitch);
        Assert.AreEqual(AnalysisCompatibility.Compatible,AnalysisCompatibilityValidator.Validate(model).Status);
        model.AssignSurfaceSection("wall",section);
        Assert.AreSame(shell.PlateProperty,model.AreaElements[1130].PlateProperty);
    }
    [TestMethod]
    public void SteelAndConcreteUseTheSameSurfaceAssignmentMechanism()
    {
        var model=Model(); var steel=new SteelPlateProperty(SteelMaterialEN1993Data.S355,20,20,"steel"){PhysicalThickness=20};
        model.AssignSurfaceSection("wall",steel,"right");
        Assert.IsInstanceOfType(model.AreaElements[1130].PlateProperty,typeof(SteelPlateProperty));
        Assert.IsInstanceOfType(model.AreaElements[1090].PlateProperty,typeof(ConcretePlateProperty));
        Assert.AreEqual(AnalysisCompatibility.RequiresReanalysis,AnalysisCompatibilityValidator.Validate(model).Status);
    }
    [TestMethod]
    public void SurfaceSelectionUnionsWithGroupsWithoutDuplicatingFemElements()
    {
        var model=Model(); var selection=new ElementSelection {Families=new[]{EntityFamily.Shell},Surfaces=new[]{new SurfaceSelection{SurfaceId="wall",ZoneId="left"}}};
        Assert.AreEqual(1090,selection.Resolve(model).Single().Id);
        var copy=selection.Copy(); selection.Surfaces[0].ZoneId="right";
        Assert.AreEqual("left",copy.Surfaces[0].ZoneId);
        selection.Groups=new[]{"Wall"}; Assert.AreEqual(2,selection.Resolve(model).Count);
        selection.Surfaces[0].SurfaceId="missing";
        Assert.ThrowsException<ArgumentException>(()=>selection.Resolve(model));
    }
    [TestMethod]
    public void MetadataChangesInvalidateVerificationWithoutChangingFemInputs()
    {
        var model=MixedModelFactory.Create(); var analysis=model.AnalysisFingerprint(); var check=model.VerificationFingerprint(null);
        model.PhysicalSurfaces.Add("wall",Surface());
        Assert.AreEqual(analysis,model.AnalysisFingerprint()); Assert.AreNotEqual(check,model.VerificationFingerprint(null));
        Assert.AreEqual(AnalysisCompatibility.Compatible,AnalysisCompatibilityValidator.Validate(model).Status);
    }
    [TestMethod]
    public void VersionedDocumentRetainsSurfacesZonesAndPropertyAliases()
    {
        var model=Model(); var section=(ReinforcedConcretePlateSection)PlateSections.UpgradeLegacy(ModelArchive.Copy(model).AreaElements[1090]);
        model.AssignSurfaceSection("wall",section);
        var copy=ModelArchive.Copy(model);
        Assert.AreEqual(2,copy.PhysicalSurfaces["wall"].ResolveElements(copy).Count);
        Assert.AreEqual("right",copy.PhysicalSurfaces["wall"].ZoneAt(1130));
        Assert.AreSame(copy.AreaElements[1090].PlateProperty,copy.AreaElements[1130].PlateProperty);
        Assert.AreNotSame(model.AreaElements[1090].PlateProperty,copy.AreaElements[1090].PlateProperty);
        Assert.AreEqual(model.VerificationFingerprint(null),copy.VerificationFingerprint(null));
        Assert.AreEqual(AnalysisCompatibility.Compatible,AnalysisCompatibilityValidator.Validate(copy).Status);
    }
    [TestMethod]
    public void MissingPlatesAndDoublePhysicalMembershipBlockPublicationAndAssignment()
    {
        var model=Model(); model.PhysicalSurfaces.Add("other",new PhysicalSurfaceDefinition("other",new[]{1090}));
        Assert.IsTrue(model.ValidatePhysicalSurfaces().Any(d=>d.Code=="PlateInMultiplePhysicalSurfaces"));
        var previous=model.AreaElements[1090].PlateProperty;
        Assert.ThrowsException<ArgumentException>(()=>model.AssignSurfaceSection("wall",PlateSectionTest.Section()));
        Assert.AreSame(previous,model.AreaElements[1090].PlateProperty);
        model.PhysicalSurfaces.Remove("other");
        using var edit=new ModelEditSession(model);
        edit.Draft.PhysicalSurfaces["wall"]=new PhysicalSurfaceDefinition("wall",new[]{999});
        Assert.IsTrue(edit.Validate().Any(d=>d.Code=="MissingSurfacePlateOrConnectivity"));
        Assert.ThrowsException<InvalidOperationException>(()=>edit.Commit());
    }
    [TestMethod]
    public void CornerOnlyContactIsNotAConnectedPhysicalSurface()
    {
        var model=Model();
        model.NodesElements.Add(new NodeElement(new Point3d(5000,0,0),id:500));
        model.NodesElements.Add(new NodeElement(new Point3d(5000,0,2000),id:501));
        model.AreaElements[1130].ConnectNodes(model.NodesElements[10],model.NodesElements[500],model.NodesElements[501]);
        Assert.IsTrue(model.ValidatePhysicalSurfaces().Any(d=>d.Code=="DisconnectedPhysicalSurface"));
    }
    [TestMethod]
    public void AxisValidationIsAtomicAndCannotPartiallyAssignTheSurface()
    {
        var model=Model(); var before=model.AreaElements.Values.Select(e=>e.PlateProperty).ToArray();
        Assert.ThrowsException<ArgumentException>(()=>model.AssignSurfaceSection("wall",PlateSectionTest.Section(),sectionAxes:CoordinateSystem.Global));
        CollectionAssert.AreEqual(before,model.AreaElements.Values.Select(e=>e.PlateProperty).ToArray());
    }
    [TestMethod]
    public void MoreThanTwoPlatesSharingAnEdgeAreNotOnePhysicalSurface()
    {
        var model = Model(); var first = model.AreaElements[1090];
        var third = new AreaElement(null, first.PlateProperty, first.CoordinateSystem, id: 1400);
        third.ConnectNodes(first.Nodes.ToArray()); model.AreaElements.Add(third);
        model.PhysicalSurfaces["wall"] = new PhysicalSurfaceDefinition("wall", new[] { 1090, 1130, 1400 });
        Assert.IsTrue(model.ValidatePhysicalSurfaces().Any(d => d.Code == "NonManifoldPhysicalSurface"));
    }
    [DataTestMethod]
    [DataRow(true)]
    [DataRow(false)]
    public void SurfaceDocumentsRejectDowngradedSchemaOrMissingSurfaceData(bool downgradeSchema)
    {
        using var stream = new MemoryStream(); ModelArchive.Save(Model(), stream); stream.Position = 0;
        var xml = XDocument.Load(stream);
        var document = xml.Root!.Elements().Single();
        Assert.AreEqual("2", document.Elements().Single(e => e.Name.LocalName == "Schema").Value);
        if (downgradeSchema) document.Elements().Single(e => e.Name.LocalName == "Schema").Value = "1";
        else document.Elements().Single(e => e.Name.LocalName == "Surfaces").Remove();
        using var changed = new MemoryStream(); xml.Save(changed); changed.Position = 0;
        Assert.ThrowsException<SerializationException>(() => ModelArchive.Load(changed));
    }
    [TestMethod]
    public void SurfaceEditPublishesAnIndependentModelWithoutRewritingAnalysis()
    {
        var model = MixedModelFactory.Create();
        using var edit = new ModelEditSession(model);
        edit.Draft.PhysicalSurfaces.Add("wall", Surface());
        var published = edit.Commit();
        Assert.AreEqual(0, model.PhysicalSurfaces.Count);
        Assert.AreEqual(2, published.Model.PhysicalSurfaces["wall"].ResolveElements(published.Model).Count);
        Assert.IsFalse(published.AnalysisChanged); Assert.IsTrue(published.VerificationChanged);
        Assert.AreEqual(AnalysisCompatibility.Compatible, published.AnalysisCompatibilityAtCommit.Status);
        edit.Draft.PhysicalSurfaces.Clear(); Assert.AreEqual(1, published.Model.PhysicalSurfaces.Count);
    }
    [TestMethod]
    public void SnapshotsTakenWithSurfacesRemainIndependentAndReadable()
    {
        var model=MixedModelFactory.Create(m=>m.PhysicalSurfaces.Add("wall",Surface()));
        Assert.AreEqual(2,model.Analysis.OpenModel().PhysicalSurfaces["wall"].ResolveElements(model).Count);
        var copy=ModelArchive.Copy(model);
        Assert.AreEqual(AnalysisCompatibility.Compatible,AnalysisCompatibilityValidator.Validate(copy).Status);
    }
}
