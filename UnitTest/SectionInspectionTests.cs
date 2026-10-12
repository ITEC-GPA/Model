using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json.Nodes;

namespace UnitTest;

[TestClass]
public class SectionInspectionTests
{
    [TestMethod]
    public void CatalogAndFullImportUseTheSameSectionParser()
    {
        var json = CivilNxModelWorkflowTest.Json();
        var profile = new CivilNxModelProfile();
        var whole = profile.Read(CivilNxModelWorkflowTest.Snapshot(json), CivilNxModelWorkflowTest.Identity(), default);
        var only = profile.ReadSectionCatalog(CivilNxModelWorkflowTest.Snapshot(json.Where(p => p.Key is "UNIT" or "SECT").ToDictionary(p => p.Key, p => p.Value)), CivilNxModelWorkflowTest.Identity());
        Assert.AreEqual(whole.Sections.Count, only.Sections.Count);
        foreach(var source in whole.Sections)
        {
            var a = SectionShapeInspection.Read(source);
            var b = SectionShapeInspection.Read(only.Sections.Single(s => s.Id == source.Id));
            Assert.AreEqual(a.Shape.Area, b.Shape.Area);
            Assert.AreEqual(a.Shape.Jxx, b.Shape.Jxx); Assert.AreEqual(a.Shape.Jyy, b.Shape.Jyy);
            Assert.AreEqual(a.CentroidOffset.X, b.CentroidOffset.X); Assert.AreEqual(a.CentroidOffset.Y, b.CentroidOffset.Y);
            CollectionAssert.AreEqual(a.Shape.GetSectionPoints(), b.Shape.GetSectionPoints());
        }
        Assert.AreEqual(0, only.Nodes.Count);
    }

    [DataTestMethod]
    [DataRow("CT", 0, 1, 0d, .125)]
    [DataRow("LC", 1, 0, .05, 0d)]
    public void UnresolvedCustomOffsetNeverBecomesZero(string point, int horizontal, int vertical, double y, double z)
    {
        var json = CivilNxModelWorkflowTest.Json();
        var table = JsonNode.Parse(json["SECT"])!;
        foreach(var pair in table["SECT"]!.AsObject())
        {
            var before = pair.Value!["SECT_BEFORE"]!;
            before["OFFSET_PT"] = point; before["HORZ_OFFSET_OPT"] = horizontal; before["VERT_OFFSET_OPT"] = vertical;
            before["USERDEF_OFFSET_YI"] = y; before["USERDEF_OFFSET_ZI"] = z;
        }
        json["SECT"] = table.ToJsonString();
        var source = CivilNxModelWorkflowTest.Snapshot(json);
        var batch = new CivilNxModelProfile().ReadSectionCatalog(source, CivilNxModelWorkflowTest.Identity());
        Assert.IsTrue(batch.Sections.All(s => s.Reference.IsUnresolved));
        Assert.IsTrue(batch.Sections.All(s => SectionShapeInspection.Read(s).CentroidOffset == null));
        var imported = CivilNxGeometryReader.Import(source, CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile());
        Assert.IsNotNull(imported.Model);
        Assert.IsTrue(imported.Model.BeamElements.Values.All(b => b.Assignments.SectionCentroidOffset == null));
        Assert.IsTrue(imported.Diagnostics.Any(d => d.Code == "CivilNxUserSectionOffsetUnresolved"));
    }

    [TestMethod]
    public void StandaloneGeometryHasNoDependencyOnRecognizedMaterialStrength()
    {
        var source = new SectionRecord { Id="box", Name="test", Shape=SectionShapeKind.Box, Dimensions=new[]{300d,200d,10d,12d,12d}, Reference=new(){Vertical=SectionVerticalReference.MaximumV2} };
        var result=SectionShapeInspection.Read(source);
        Assert.AreEqual(2*200*12+2*10*(300-24),result.Shape.Area,1e-9);
        Assert.AreEqual(-150,result.CentroidOffset.Y,1e-9);
        Assert.AreEqual(1,result.Shape.GetCurveOutlines().Single().Holes.Count);
        Assert.AreEqual(PropertyAvailability.Exact,result.Shape.GetAvailability(SectionProperty.Area));
    }

    [TestMethod]
    public void NumericOnlySectionDoesNotInventAGeometry()
    {
        var values=new SectionValues{Area=1200,I11=4e6,I22=2e6,Torsion=1e5};
        var result=SectionShapeInspection.Read(new(){Id="values",Values=values,Reference=new(){Vertical=SectionVerticalReference.MaximumV2}});
        Assert.IsNull(result.Shape);Assert.IsNull(result.CentroidOffset);Assert.AreSame(values,result.Source.Values);
    }
    private static SectionRecord Composite(string reference="CT",double haunch=0,int stiffeners=0)
    {
        // User's section dimensions, independently specified in metres; no solver result values reused.
        var json=CivilNxModelWorkflowTest.Json();
        json["SECT"]=System.Text.Json.JsonSerializer.Serialize(new {SECT=new Dictionary<string,object>{["100"]=new {
            SECTTYPE="COMPOSITE",SECT_NAME="Campata · concio 1",SECT_BEFORE=new {SHAPE="I",OFFSET_PT=reference,OFFSET_CENTER=0,
                SECT_I=new {vSIZE=new[]{1.96,.015,.6,.02,.8,.02},STIFFENER=new {STIFF_CNT=new[]{stiffeners,0,0,0}}}},
            SECT_AFTER=new {SLAB=new[]{3.6,.28,haunch}}}}});
        return new CivilNxModelProfile().ReadSectionCatalog(CivilNxModelWorkflowTest.Snapshot(json),CivilNxModelWorkflowTest.Identity()).Sections.Single();
    }

    [TestMethod]
    public void CompositeOutlineKeepsPhysicalPartsAndUnambiguousTopReference()
    {
        var record=Composite();var value=SectionShapeInspection.Read(record);
        Assert.IsTrue(value.HasMultipleMaterials);Assert.AreEqual(2,value.Parts.Count);
        Assert.AreEqual(MaterialKind.Steel,value.Parts[0].Material);Assert.AreEqual(MaterialKind.Concrete,value.Parts[1].Material);
        Assert.AreEqual(2000,value.Parts[0].Shape.Height);Assert.AreEqual(280,value.Parts[1].Shape.Height);
        Assert.AreEqual(2000,value.Parts[1].Y);Assert.AreEqual(1400,value.Parts[0].X);
        Assert.AreEqual(1960*15+600*20+800*20+3600*280,value.Shape.Area,1e-6);
        Assert.AreEqual(0,value.CentroidOffset.X,1e-9);
        Assert.AreEqual(2280,value.Shape.Centroid.Y-value.CentroidOffset.Y,1e-9,"Reference line on top of slab");
        Assert.AreEqual(SectionShapeKind.Unknown,record.Shape,"A physical preview is not an analysis property");
        Assert.AreEqual(2,value.Shape.GetCurveOutlines().Count);
    }

    [DataTestMethod]
    [DataRow(.1,0)][DataRow(0d,2)]
    public void UnknownHaunchOrStiffenersDoNotProduceAFalseCompleteOutline(double haunch,int stiffeners)
    {
        var record=Composite("CT",haunch,stiffeners);Assert.IsNull(record.GeometryComponents);
        Assert.IsNull(SectionShapeInspection.Read(record).Shape);
    }

    [TestMethod]
    public void PhysicalCentroidDoesNotReplaceCompositeElasticCentroid()
    {
        var value=SectionShapeInspection.Read(Composite("CC"));
        Assert.IsNotNull(value.Shape);Assert.IsNull(value.CentroidOffset);
        Assert.IsTrue(value.Diagnostics.Any(d=>d.Code=="CompositeReferenceUnresolved"));
    }

}
