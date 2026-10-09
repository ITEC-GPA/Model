using GPC.Examples;
using GPC.Geometry;
using GPC.Model.ElementProperties;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;

namespace UnitTest;

[TestClass]
public class PlateSectionAssignmentTest
{
    internal static ReinforcedConcretePlateSection Section(double thickness=300)
    {
        var shell=MixedModelFactory.Create().AreaElements[1090];
        return new ReinforcedConcretePlateSection(((ConcretePlateProperty)shell.PlateProperty).ConcreteMaterial,
            thickness,240,300,shell.Assignments.Layers,"RC plate");
    }
    internal static CoordinateSystem Axes() => new(new Point3d(0,0,0),new Vector3d(1,0,0),new Vector3d(0,1,0),new Vector3d(0,0,1));
    internal static ShellSectionRegion Box(double x0,double x1) => new(new[]{new Point2d(x0,0),new Point2d(x1,0),new Point2d(x1,100),new Point2d(x0,100)});
    private static ShellAssignments Pair(double secondStart=50)
    {
        var sections=new ShellAssignments(); var property=Section();
        sections.Sections.Add(new ShellSectionAssignment{Id="left",Property=property,Axes=Axes(),Region=Box(0,50)});
        sections.Sections.Add(new ShellSectionAssignment{Id="right",Property=property,Axes=Axes(),Region=Box(secondStart,100)});
        return sections;
    }
    [TestMethod]
    public void ReadingAnEmptyAssignmentListDoesNotChangeLegacyIdentity()
    {
        var assignments=new ShellAssignments(); string before=ModelArchive.Fingerprint(new object[]{assignments});
        Assert.AreEqual(0,assignments.Sections.Count);
        Assert.AreEqual(before,ModelArchive.Fingerprint(new object[]{assignments}));
        Assert.AreEqual(before,ModelArchive.Fingerprint(new object[]{ModelArchive.CopyValue(assignments)}));
    }
    [TestMethod]
    public void SectionOwnsReinforcementAndIndependentThicknesses()
    {
        var section=Section(); Assert.AreEqual(300,section.PhysicalThickness); Assert.AreEqual(240,section.BendingThickness);
        Assert.IsTrue(section.RebarLayers.Count>0);
        var copy=ModelArchive.CopyValue(section);
        Assert.AreEqual(ModelArchive.Fingerprint(new object[]{section}),ModelArchive.Fingerprint(new object[]{copy}));
        copy.RebarLayers[0].Pitch*=2;
        Assert.AreNotEqual(section.RebarLayers[0].Pitch,copy.RebarLayers[0].Pitch);
        Assert.IsFalse(section.Equals(copy));
        var plain=new ConcretePlateProperty(section.ConcreteMaterial,240,300,section.Name){PhysicalThickness=300};
        Assert.IsFalse(plain.Equals(section)); Assert.IsFalse(section.Equals(plain));
    }
    [TestMethod]
    public void RegionsSelectTheSectionAtThePointAndRequireABoundaryChoice()
    {
        var assignments=Pair();
        Assert.AreEqual("left",assignments.SectionAt(new Point3d(25,50,0)).Id);
        Assert.AreEqual("right",assignments.SectionAt(new Point3d(75,50,0)).Id);
        Assert.ThrowsException<InvalidOperationException>(()=>assignments.SectionAt(new Point3d(50,50,0)));
        Assert.AreEqual("left",assignments.SectionAt(new Point3d(50,50,0),"left").Id);
        Assert.AreEqual("right",assignments.SectionAt(new Point3d(50,50,0),"right").Id);
        Assert.ThrowsException<InvalidOperationException>(()=>assignments.SectionAt(new Point3d(25,50,0),"right"));
        Assert.ThrowsException<InvalidOperationException>(()=>assignments.SectionAt(new Point3d(110,50,0)));
    }
    [TestMethod]
    public void OverlappingRegionsAreRejectedEvenWithAnExplicitSelection()
    {
        var assignments=Pair(40);
        Assert.ThrowsException<ArgumentException>(()=>assignments.SectionAt(new Point3d(45,50,0),"left"));
        assignments.Sections[1].Region=Box(0,50);
        Assert.ThrowsException<ArgumentException>(()=>ShellSectionValidation.ValidateAssignments(assignments.Sections));
        assignments.Sections[1].Region=null;
        Assert.ThrowsException<ArgumentException>(()=>ShellSectionValidation.ValidateAssignments(assignments.Sections));
    }
    [TestMethod]
    public void GapsAndDuplicateIdsAreNotSilentlyResolved()
    {
        var assignments=Pair(60);
        Assert.ThrowsException<InvalidOperationException>(()=>assignments.SectionAt(new Point3d(55,50,0)));
        assignments.Sections[1].Id="left";
        Assert.ThrowsException<ArgumentException>(()=>assignments.SectionAt(new Point3d(25,50,0)));
    }
    [TestMethod]
    public void RegionAndReinforcementAxesCanBePlacedInSpace()
    {
        var assignments=new ShellAssignments();
        assignments.Sections.Add(new ShellSectionAssignment{Id="rotated",Property=Section(),Region=Box(0,100),
            Axes=new CoordinateSystem(new Point3d(500,600,700),new Vector3d(0,1,0),new Vector3d(0,0,1),new Vector3d(1,0,0))});
        Assert.AreEqual("rotated",assignments.SectionAt(new Point3d(500,625,775)).Id);
        Assert.ThrowsException<ArgumentException>(()=>assignments.SectionAt(new Point3d(501,625,775)));
    }
    [TestMethod]
    public void ArchivePreservesSharedSectionsWithoutAliasingTheSource()
    {
        var original=Pair(); var copy=ModelArchive.CopyValue(original);
        Assert.AreSame(copy.Sections[0].Property,copy.Sections[1].Property);
        Assert.AreNotSame(original.Sections[0].Property,copy.Sections[0].Property);
        Assert.AreEqual(ModelArchive.Fingerprint(new object[]{original}),ModelArchive.Fingerprint(new object[]{copy}));
        Assert.AreEqual("right",copy.SectionAt(new Point3d(75,50,0)).Id);
        copy.Sections[1].Region=Box(60,100);
        Assert.AreNotEqual(ModelArchive.Fingerprint(new object[]{original}),ModelArchive.Fingerprint(new object[]{copy}));
    }
    [TestMethod]
    public void InvalidGeometryAndOutOfThicknessBarsAreRejected()
    {
        Assert.ThrowsException<ArgumentException>(()=>new ShellSectionRegion(new[]{new Point2d(0,0),new Point2d(1,0),new Point2d(2,0)}));
        Assert.ThrowsException<ArgumentException>(()=>new ShellSectionRegion(new[]{new Point2d(0,0),new Point2d(2,2),new Point2d(0,2),new Point2d(2,0)}));
        Assert.ThrowsException<ArgumentException>(()=>new ShellSectionRegion(new[]{new Point2d(0,0),new Point2d(2,0),new Point2d(1,.5),new Point2d(2,2),new Point2d(0,2)}));
        var section=Section(); section.RebarLayers[0].AxisPositionThroughThickness=150;
        Assert.ThrowsException<ArgumentException>(()=>section.Validate());
    }
    [TestMethod]
    public void PolygonVerticesAreOwnedAndBothWindingsWork()
    {
        var region=Box(0,100); var exposed=region.Vertices; exposed[0].X=999;
        Assert.AreEqual(ShellRegionLocation.Inside,region.Locate(new Point2d(50,50)));
        var reverse=new ShellSectionRegion(region.Vertices.Reverse());
        Assert.AreEqual(ShellRegionLocation.Inside,reverse.Locate(new Point2d(50,50)));
        Assert.AreEqual(ShellRegionLocation.Boundary,reverse.Locate(new Point2d(0,50)));
        Assert.AreEqual(ShellRegionLocation.Outside,reverse.Locate(new Point2d(-1,50)));
    }
}
