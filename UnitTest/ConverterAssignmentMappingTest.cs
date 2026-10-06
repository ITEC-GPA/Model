using GPC.Converter;
using GPC.Geometry;
using GPC.Model.Loads;
using GPC.Model.Materials;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class ConverterAssignmentMappingTest
{
    // Normalized DTOs as a solver reader produces them: N, mm, t/mm³.
    private static ImportBatch Batch()
    {
        var batch = new ImportBatch { Program = "Synthetic", ModelRevision = "r1", AnalysisId = "A", SourceHash = "H" };
        batch.Nodes.Add(new NodeRecord { Id = "1", GlobalPosition = new Point3d(0, 0, 0), Record = "N1" });
        batch.Nodes.Add(new NodeRecord { Id = "2", GlobalPosition = new Point3d(4000, 0, 0), Record = "N2" });
        batch.Nodes.Add(new NodeRecord { Id = "3", GlobalPosition = new Point3d(4000, 2000, 0), Record = "N3" });
        batch.Nodes.Add(new NodeRecord { Id = "4", GlobalPosition = new Point3d(0, 2000, 0), Record = "N4" });
        var axes = Axes.Beam(new Point3d(0, 0, 0), new Point3d(4000, 0, 0), new Vector3d(0, 1, 0), 0);
        batch.Beams.Add(new BeamRecord { Id = "10", I = "1", J = "2", CoordinateSystem = axes, MaterialId = "S", SectionId = "HEB", Record = "B10" });
        batch.Beams.Add(new BeamRecord { Id = "11", I = "4", J = "3", CoordinateSystem = Axes.Beam(new Point3d(0, 2000, 0), new Point3d(4000, 2000, 0), new Vector3d(0, 1, 0), 0),
            MaterialId = "C", SectionId = "R", Record = "B11" });
        batch.Shells.Add(new ShellRecord { Id = "20", Nodes = new[] { "1", "2", "3", "4" }, MaterialId = "C", ThicknessId = "T", Record = "P20",
            CoordinateSystem = new CoordinateSystem(new Point3d(2000, 1000, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0), new Vector3d(0, 0, 1)) });
        batch.Materials.Add(new MaterialRecord { Id = "S", Name = "Steel S355", Kind = MaterialKind.Steel, Grade = "S355", ElasticModulus = 210000, Record = "M-S" });
        batch.Materials.Add(new MaterialRecord { Id = "C", Name = "C30/37", Kind = MaterialKind.Concrete, ElasticModulus = 33000, Record = "M-C" });
        batch.Sections.Add(new SectionRecord { Id = "HEB", Name = "HEB 300", CatalogDesignation = "HE 300 B", Record = "S-HEB" });
        batch.Sections.Add(new SectionRecord { Id = "R", Name = "Rect 300x500", Shape = SectionShapeKind.SolidRectangle, Dimensions = new double[] { 500, 300 }, Record = "S-R" });
        batch.Thicknesses.Add(new ThicknessRecord { Id = "T", Name = "Slab 200", InPlane = 200, Offset = 100, Record = "T" });
        batch.LoadCases.Add(new LoadCaseRecord { Name = "G", Record = "LC" });
        return batch;
    }

    private static ImportReport Map(ImportBatch batch, MappingOptions? options = null)
    {
        var report = ModelMapper.Map(batch, options: options);
        Assert.AreEqual(ImportStatus.Partial, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Code + ": " + d.Message)));
        return report;
    }

    [TestMethod]
    public void CatalogAndParametricSections_WithGradeMaterials_BecomeSteelAndConcreteProperties()
    {
        var report = Map(Batch()); var m = report.Model;
        var steel = (SteelSection)m.BeamElements.Values.Single(b => b.Source.OriginalId == "10").BeamProperty;
        Assert.AreEqual(355, steel.SteelMaterial.Fyk); Assert.AreEqual("Steel S355", steel.SteelMaterial.Name);
        Assert.IsTrue(steel.IsRolled); Assert.AreEqual("HEB 300", steel.Name); Assert.AreEqual(14910, steel.Area, 30, "Catalog HE 300 B area.");
        var concreteBeam = m.BeamElements.Values.Single(b => b.Source.OriginalId == "11");
        var rc = (ReinforcedConcreteSection)concreteBeam.BeamProperty;
        Assert.AreEqual(30, Math.Abs(((ConcreteMaterialEuropeanCommon)rc.ConcreteMaterial).Fck), "Model stores fck with the compression sign."); Assert.AreEqual(150000, rc.Area, 1e-6);
        Assert.AreSame(rc, concreteBeam.Assignments.Sections.Single().Section);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "ConcreteSectionWithoutReinforcement"));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "MaterialGradeFromName" && d.Record == "M-C"));
        var plate = (ConcretePlateProperty)m.AreaElements.Values.Single().PlateProperty;
        Assert.AreEqual(200, plate.MembraneThickness); Assert.AreEqual(200, plate.BendingThickness); Assert.AreEqual("Slab 200", plate.Name);
        Assert.AreSame(rc.ConcreteMaterial, plate.ConcreteMaterial, "One material instance per source material.");
        Assert.AreEqual(200, m.AreaElements.Values.Single().Assignments.PhysicalThickness); Assert.AreEqual(100, m.AreaElements.Values.Single().Assignments.Offset);
        Assert.AreEqual(2, m.BeamProperties.Count); Assert.AreEqual(1, m.PlateProperties.Count);
        Assert.AreEqual(0, concreteBeam.Assignments.SectionCentroidOffset.X); Assert.AreEqual(0, concreteBeam.Assignments.SectionCentroidOffset.Y);
    }

    [TestMethod]
    public void ArchiveRoundTrip_KeepsPropertiesLoadsAndOffsets()
    {
        var batch = Batch();
        batch.Beams[0].OffsetI = new Vector3d(0, 0, 150); batch.Beams[0].OffsetJ = new Vector3d(0, 0, 150); batch.Beams[0].OffsetAxes = CoordinateSystem.Global;
        batch.BeamLoads.Add(new BeamLoadRecord { BeamId = "10", Case = "G", Start = 0, End = 1, StartValues = new double[] { 0, 0, -10, 0, 0, 0 },
            EndValues = new double[] { 0, 0, -10, 0, 0, 0 }, CoordinateSystem = CoordinateSystem.Global, Record = "BL" });
        var report = Map(batch); using var archive = new MemoryStream();
        ModelArchive.Save(report.Model, archive); archive.Position = 0; var restored = ModelArchive.Load(archive);
        Assert.AreEqual(report.Model.AnalysisFingerprint(), restored.AnalysisFingerprint());
        var beam = restored.BeamElements.Values.Single(b => b.Source.OriginalId == "10");
        Assert.AreEqual(355, ((SteelSection)beam.BeamProperty).SteelMaterial.Fyk); Assert.AreEqual(150, beam.Assignments.OffsetI.Z);
        Assert.AreEqual(-10, beam.Assignments.Loads.Single().StartIntensity.F3);
    }

    [DataTestMethod]
    [DataRow(SectionHorizontalReference.Center, SectionVerticalReference.MaximumV2, 0d, 0d, 0d, -250d)]
    [DataRow(SectionHorizontalReference.MinimumV1, SectionVerticalReference.MinimumV2, 0d, 0d, 150d, 250d)]
    [DataRow(SectionHorizontalReference.Centroid, SectionVerticalReference.Centroid, 10d, -20d, -10d, 20d)]
    [DataRow(SectionHorizontalReference.MaximumV1, SectionVerticalReference.Center, 0d, 50d, -150d, -50d)]
    public void SectionReference_GivesCentroidFromTheReferenceLine(SectionHorizontalReference h, SectionVerticalReference v, double dh, double dv, double x, double y)
    {
        var batch = Batch(); batch.Sections[1].Reference = new SectionReference { Horizontal = h, Vertical = v, HorizontalShift = dh, VerticalShift = dv };
        var offset = Map(batch).Model.BeamElements.Values.Single(b => b.Source.OriginalId == "11").Assignments.SectionCentroidOffset;
        Assert.AreEqual(x, offset.X, 1e-9); Assert.AreEqual(y, offset.Y, 1e-9);
    }

    [TestMethod]
    public void UnresolvedMaterial_LeavesNoProperty_UnlessTheCallerResolvesIt()
    {
        var batch = Batch(); batch.Materials[0].Grade = null; batch.Materials[0].Name = "Acciaio speciale";
        var report = Map(batch);
        Assert.IsNull(report.Model.BeamElements.Values.Single(b => b.Source.OriginalId == "10").BeamProperty);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "MaterialGradeUnresolved" && d.Severity == DiagnosticSeverity.Warning));
        var resolved = Map(Batch().Also(b => { b.Materials[0].Grade = null; b.Materials[0].Name = "Acciaio speciale"; }),
            new MappingOptions { ResolveMaterial = r => r.Id == "S" ? new SteelMaterialEN1993("Acciaio speciale", 200000, 300, 400) : null });
        Assert.AreEqual(300, ((SteelSection)resolved.Model.BeamElements.Values.Single(b => b.Source.OriginalId == "10").BeamProperty).SteelMaterial.Fyk);
    }

    [TestMethod]
    public void SameSectionWithTwoMaterials_GivesTwoNamedProperties()
    {
        var batch = Batch(); batch.Beams[1].SectionId = "HEB"; batch.Beams[1].MaterialId = "S2";
        batch.Materials.Add(new MaterialRecord { Id = "S2", Name = "S275", Kind = MaterialKind.Steel, Grade = "S275", Record = "M-S2" });
        var m = Map(batch).Model;
        CollectionAssert.AreEquivalent(new[] { "HEB 300 / Steel S355", "HEB 300 / S275" }, m.BeamProperties.Values.Select(p => p.Name).ToArray());
    }

    [TestMethod]
    public void BeamLoads_MapToAssignmentsWithExactResultants()
    {
        var batch = Batch();
        batch.BeamLoads.Add(new BeamLoadRecord { BeamId = "10", Case = "G", Start = .25, End = .75, StartValues = new double[] { 0, 0, -2, 0, 0, 0 },
            EndValues = new double[] { 0, 0, -4, 0, 0, 0 }, CoordinateSystem = CoordinateSystem.Global, Record = "BL1" });
        batch.BeamLoads.Add(new BeamLoadRecord { BeamId = "10", Case = "G", Start = .5, End = .5, Values = new double[] { 1000, 0, 0, 0, 0, 0 },
            CoordinateSystem = batch.Beams[0].CoordinateSystem, Record = "BL2" });
        batch.BeamLoads.Add(new BeamLoadRecord { BeamId = "11", Case = "G", Start = 0, End = 1, StartValues = new double[] { 0, 0, -1, 0, 0, 0 },
            EndValues = new double[] { 0, 0, -1, 0, 0, 0 }, CoordinateSystem = CoordinateSystem.Global, ProjectionPlaneNormal = new Vector3d(0, 0, 1), Record = "BL3" });
        var m = Map(batch).Model; var beam = m.BeamElements.Values.Single(b => b.Source.OriginalId == "10");
        var trapezoid = Equilibrium.BeamLoad(beam, beam.Assignments.Loads[0]);
        Assert.AreEqual(-(2 + 4) / 2.0 * 2000, trapezoid.Force.Z, 1e-9);
        // Moment about the load start (x = 1000): the trapezoid centroid is 2000 * (2 + 2 * 4) / (3 * (2 + 4)) further along.
        Assert.AreEqual(1000, trapezoid.Point.X, 1e-9);
        Assert.AreEqual(-(2000 * 10.0 / 18.0) * trapezoid.Force.Z, trapezoid.Moment.Y, 1e-6);
        var point = beam.Assignments.Loads[1].Concentrated;
        Assert.AreEqual(2000, point.Point.X); Assert.AreEqual(1000, Equilibrium.BeamLoad(beam, beam.Assignments.Loads[1]).Force.Y, 1e-9);
        var projected = m.BeamElements.Values.Single(b => b.Source.OriginalId == "11").Assignments.Loads.Single();
        Assert.AreEqual(BeamLoadLengthConvention.ProjectedLength, projected.LengthConvention); Assert.AreEqual("BL3", projected.SourceRecord);
    }

    [TestMethod]
    public void ShellLoads_NormalPressureNeedsDeclaredAxes_TractionUsesItsOwnAxes()
    {
        var batch = Batch();
        batch.ShellLoads.Add(new ShellLoadRecord { ShellId = "20", Case = "G", Normal = true, Pressure = -0.005, Record = "PL1" });
        batch.ShellLoads.Add(new ShellLoadRecord { ShellId = "20", Case = "G", Components = new[] { 0, 0.001, 0 }, CoordinateSystem = CoordinateSystem.Global, Record = "PL2" });
        var shell = Map(batch).Model.AreaElements.Values.Single();
        Assert.AreEqual(-0.005, shell.Loads.Values.OfType<NormalAreaLoad>().Single().Pressure);
        Assert.AreEqual(0.001, shell.Loads.Values.OfType<AreaLoad>().Single().P2);
        var undeclared = Batch(); undeclared.Shells[0].CoordinateSystem = null;
        undeclared.ShellLoads.Add(new ShellLoadRecord { ShellId = "20", Case = "G", Normal = true, Pressure = 1, Record = "PL3" });
        var rejected = ModelMapper.Map(undeclared);
        Assert.AreEqual(ImportStatus.Rejected, rejected.Status); Assert.AreEqual("PL3", rejected.Diagnostics.Single(d => d.Severity == DiagnosticSeverity.Error).Record);
    }

    [DataTestMethod]
    [DataRow("missing-section")]
    [DataRow("duplicate-material")]
    [DataRow("rigid-too-long")]
    [DataRow("bad-dimension")]
    [DataRow("load-unknown-case")]
    [DataRow("load-bad-interval")]
    public void InvalidAssignments_RejectAtomically(string defect)
    {
        var batch = Batch();
        switch (defect)
        {
            case "missing-section": batch.Beams[0].SectionId = "X"; break;
            case "duplicate-material": batch.Materials.Add(new MaterialRecord { Id = "S", Name = "dup", Record = "dup" }); break;
            case "rigid-too-long": batch.Beams[0].RigidLengthI = 2500; batch.Beams[0].RigidLengthJ = 2000; break;
            case "bad-dimension": batch.Sections[1].Dimensions = new double[] { 500, -1 }; break;
            case "load-unknown-case": batch.BeamLoads.Add(new BeamLoadRecord { BeamId = "10", Case = "Q", Values = new double[6], CoordinateSystem = CoordinateSystem.Global, Record = "x" }); break;
            case "load-bad-interval": batch.BeamLoads.Add(new BeamLoadRecord { BeamId = "10", Case = "G", Start = .8, End = .2, StartValues = new double[6], EndValues = new double[6],
                CoordinateSystem = CoordinateSystem.Global, Record = "x" }); break;
        }
        var report = ModelMapper.Map(batch);
        Assert.AreEqual(ImportStatus.Rejected, report.Status, defect); Assert.IsNull(report.Model);
    }
}

internal static class BatchExtensions
{
    public static ImportBatch Also(this ImportBatch batch, Action<ImportBatch> change) { change(batch); return batch; }
}
