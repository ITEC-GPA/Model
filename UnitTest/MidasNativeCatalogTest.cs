using System.Globalization;
using System.Text.Json;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Converter.MidasCivil;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Results.Locations;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

// Immutable native Civil NX 9.5.5 responses, generated 2026-10-23. See fixture PROVENANCE.txt.
[TestClass]
public class MidasNativeCatalogTest
{
    private static string Folder => Path.Combine(AppContext.BaseDirectory, "Fixtures", "MidasCivil", "catalog-pairs-native-9.5.5");
    private static string Read(string name) => File.ReadAllText(Path.Combine(Folder, name));
    private static AnalysisSource Identity() => new() { Program = "MIDAS Civil NX", SolverVersion = "9.5.5",
        ModelRevision = "catalog-pairs-native-20261023", AnalysisId = "catalog-static" };
    private static CivilNxSnapshot Snapshot(bool computed = true)
    {
        var names = CivilNxModelProfile.RequiredTables.Concat(CivilNxModelProfile.OptionalTables).Concat(new[] { "MATERIAL" });
        if (computed) names = names.Concat(new[] { "SECTIONALL" });
        return new CivilNxSnapshot(names.ToDictionary(n => n, n => new CivilNxResponse(
            n is "SECTIONALL" or "MATERIAL" ? "post/table" : "db/" + n, Read(n + ".json"))));
    }
    private static CivilNxResponse Result(string name) => new("post/table", Read(name + ".json"));
    private static Dictionary<string, string> Row(string file, string idColumn, int id)
    {
        using var json = JsonDocument.Parse(Read(file + ".json"));
        var table = json.RootElement.EnumerateObject().Single().Value;
        var columns = table.GetProperty("HEAD").EnumerateArray().Select(v => v.GetString()!).ToArray();
        var row = table.GetProperty("DATA").EnumerateArray().Single(r => r[Array.IndexOf(columns, idColumn)].GetString() == id.ToString());
        return columns.Select((c, i) => (c, v: row[i].GetString()!)).ToDictionary(v => v.c, v => v.v);
    }
    private static double V(Dictionary<string, string> row, string column) => double.Parse(row[column], CultureInfo.InvariantCulture);
    private static Vector3d Vector(Dictionary<string, string> row, string x, string y, string z) => new(V(row, x), V(row, y), V(row, z));
    private static double Dot(Vector3d a, Vector3d b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    private static void Equal(Vector3d expected, Vector3d actual, double tolerance)
    {
        Assert.AreEqual(expected.X, actual.X, tolerance, "X");
        Assert.AreEqual(expected.Y, actual.Y, tolerance, "Y");
        Assert.AreEqual(expected.Z, actual.Z, tolerance, "Z");
    }
    public static IEnumerable<object[]> Members => Enumerable.Range(1, 48).Select(n => new object[] { n });

    [DataTestMethod]
    [DynamicData(nameof(Members))]
    public void NativeSixActionsAndSupportReactionsMatchGlobalEquilibrium(int id)
    {
        var snapshot = Snapshot();
        var imported = CivilNxGeometryReader.Import(snapshot, Identity(), new CivilNxModelProfile());
        Assert.AreNotEqual(ImportStatus.Rejected, imported.Status);
        Assert.AreEqual(48, imported.Model.BeamElements.Count);
        var results = CivilNxResults.Import(imported.Model, snapshot, new[] { Result("BEAMFORCE"), Result("REACTIONG") }, "catalog-actions");
        Assert.AreEqual(ImportStatus.Completed, results.Status, string.Join(";", results.Diagnostics.Select(d => d.Message)));
        var beam = imported.Model.BeamElements.Values.Single(b => b.Source.OriginalId == id.ToString());
        var axes = beam.Assignments.SectionAxes;
        var force = new Vector3d(-10000, 200, 300); var endMoment = new Vector3d(7000, 4000, -6000);
        Vector3d delta = beam.EndPoint - beam.StartPoint;
        var samples = beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().ToArray();
        Assert.AreEqual(5, samples.Length);
        foreach (var sample in samples)
        {
            var f = sample.ResultBeamForces;
            Equal(force, f.V1 * axes.V1 + f.V2 * axes.V2 + f.N * axes.V3, 1e-5);
            var expected = endMoment + (delta * (1 - sample.ParametricDistance)).CrossProduct(force);
            Equal(expected, f.M1 * axes.V1 + f.M2 * axes.V2 + f.T * axes.V3, 1e-3);
        }
        var reaction = Row("REACTIONG", "Node", 2 * id - 1);
        Equal(-1 * force, Vector(reaction, "FX", "FY", "FZ"), 1e-5);
        Equal(-1 * (endMoment + delta.CrossProduct(force)), Vector(reaction, "MX", "MY", "MZ"), 1e-3);
        var support = imported.Model.NodesElements.Values.Single(n => n.Source.OriginalId == (2 * id - 1).ToString());
        var importedReaction = support.Results.SelectMany(r => r.Results).OfType<NodeResultForces>().Single();
        Equal(-1 * force, new Vector3d(importedReaction.Fx, importedReaction.Fy, importedReaction.Fz), 1e-5);
        Equal(-1 * (endMoment + delta.CrossProduct(force)), new Vector3d(importedReaction.Mx, importedReaction.My, importedReaction.Mz), 1e-3);
    }

    [DataTestMethod]
    [DynamicData(nameof(Members))]
    public void NativeDisplacementsMatchEulerBernoulliAndSaintVenantWithNativeStiffness(int id)
    {
        var snapshot = Snapshot();
        var imported = CivilNxGeometryReader.Import(snapshot, Identity(), new CivilNxModelProfile());
        Assert.AreNotEqual(ImportStatus.Rejected, imported.Status);
        var results = CivilNxResults.Import(imported.Model, snapshot, new[] { Result("DISPLACEMENTG") }, "catalog-displacements");
        Assert.AreEqual(ImportStatus.Completed, results.Status, string.Join(";", results.Diagnostics.Select(d => d.Message)));
        var beam = imported.Model.BeamElements.Values.Single(b => b.Source.OriginalId == id.ToString());
        var a = beam.Assignments.SectionAxes; var p = Row("SECTIONALL", "ID", (id - 1) / 6 + 1);
        double l = beam.Length, e = 210000, g = e / 2.6, iy = V(p, "Iyy"), iz = V(p, "Izz");
        var force = new Vector3d(-10000, 200, 300); var moment = new Vector3d(7000, 4000, -6000);
        double fx = Dot(force, a.V3), fy = Dot(force, a.V1), fz = Dot(force, a.V2);
        double mx = Dot(moment, a.V3), my = Dot(moment, a.V1), mz = Dot(moment, a.V2);
        // No shear deformation, no warping restraint; N, mm, radians. Native J is used, not inferred from Iyy+Izz.
        var expectedU = (fx * l / (e * V(p, "Area"))) * a.V3
            + (fy * l * l * l / (3 * e * iz) + mz * l * l / (2 * e * iz)) * a.V1
            + (fz * l * l * l / (3 * e * iy) - my * l * l / (2 * e * iy)) * a.V2;
        var expectedR = (mx * l / (g * V(p, "Ixx"))) * a.V3
            + (my * l / (e * iy) - fz * l * l / (2 * e * iy)) * a.V1
            + (mz * l / (e * iz) + fy * l * l / (2 * e * iz)) * a.V2;
        var displacement = Row("DISPLACEMENTG", "Node", 2 * id);
        // SECTIONALL rounds to 0.0001; propagate this absolute stiffness precision in the tolerance.
        double relative = 2e-8;
        Equal(expectedU, Vector(displacement, "DX", "DY", "DZ"), relative * Math.Max(1, Math.Sqrt(Dot(expectedU, expectedU))));
        Equal(expectedR, Vector(displacement, "RX", "RY", "RZ"), relative * Math.Max(.001, Math.Sqrt(Dot(expectedR, expectedR))));
        var node = imported.Model.NodesElements.Values.Single(n => n.Source.OriginalId == (2 * id).ToString());
        var d = node.Results.SelectMany(r => r.Results).OfType<NodeResultDisplacement>().Single().ResultDisplacement;
        Equal(Vector(displacement, "DX", "DY", "DZ"), new Vector3d(d.D1, d.D2, d.D3), 1e-12);
        Equal(Vector(displacement, "RX", "RY", "RZ"), new Vector3d(d.R1, d.R2, d.R3), 1e-12);
    }

    [DataTestMethod]
    [DataRow(true)] [DataRow(false)]
    public void NativeCatalogPairsRetainEvidenceWithoutInventingGapOrComponents(bool computed)
    {
        var snapshot = Snapshot(computed); var profile = new CivilNxModelProfile();
        var catalog = profile.ReadSectionCatalog(snapshot, Identity());
        Assert.AreEqual(8, catalog.Sections.Count);
        foreach (var id in new[] { "1", "2" })
        {
            var section = catalog.Sections.Single(s => s.Id == id);
            Assert.IsNull(section.IdenticalPair); Assert.IsNull(section.CatalogDesignation);
            Assert.IsNull(SectionShapeInspection.Read(section).Shape);
            if (computed)
            {
                var native = Row("SECTIONALL", "ID", int.Parse(id));
                Assert.IsNotNull(section.Values);
                Assert.AreEqual(V(native, "Area"), section.Values.Area, 1e-10);
                Assert.AreEqual(V(native, "Iyy"), section.Values.I11, 1e-8);
                Assert.AreEqual(V(native, "Izz"), section.Values.I22, 1e-8);
            }
            else Assert.IsNull(section.Values);
        }
        var imported = CivilNxGeometryReader.Import(snapshot, Identity(), profile);
        Assert.AreEqual(ImportStatus.Partial, imported.Status);
        Assert.AreEqual(2, imported.Diagnostics.Count(d => d.Code == "CivilNxPairCatalogUnresolved"));
        foreach (var beam in imported.Model.BeamElements.Values.Where(b => int.Parse(b.Source.OriginalId) <= 12))
            Assert.IsNull(beam.BeamProperty);
    }

    [DataTestMethod]
    [DataRow(3)] [DataRow(4)] [DataRow(5)] [DataRow(6)] [DataRow(7)] [DataRow(8)]
    public void NativeUserPairsMatchIndependentPlateIntegrationAndBothJoinedLegs(int id)
    {
        var catalog = new CivilNxModelProfile().ReadSectionCatalog(Snapshot(), Identity());
        var shape = SectionShapeInspection.Read(catalog.Sections.Single(s => s.Id == id.ToString())).Shape;
        Assert.IsInstanceOfType(shape, typeof(SectionBuiltUp));
        var pair = (SectionBuiltUp)shape;
        bool channel = id <= 4; double h = channel ? 200 : id <= 6 ? 150 : 100, b = channel ? 75 : id <= 6 ? 100 : 150;
        double gap = id % 2 == 1 ? 10 : 30;
        var expected = IdenticalPairReference.Integrate(channel, h, b, 10, 12, gap, 3);
        var native = Row("SECTIONALL", "ID", id);
        Assert.AreEqual(expected.Area, pair.Area, 1e-7);
        Assert.AreEqual(expected.Ixx, pair.Jxx, 1e-5); Assert.AreEqual(expected.Iyy, pair.Jyy, 1e-5);
        Assert.AreEqual(V(native, "Area"), pair.Area, .00006);
        Assert.AreEqual(V(native, "Iyy"), pair.Jxx, .00006); Assert.AreEqual(V(native, "Izz"), pair.Jyy, .00006);
        Assert.AreSame(pair.Parts[0].Section, pair.Parts[1].Section);
        Assert.AreEqual(h, pair.Height, 1e-8); Assert.AreEqual(2 * b + gap, pair.Width, 1e-8);
    }

    [TestMethod]
    public void NativeMctAndApiRetainTheSameAxesAndUserPairsWhileCatalogPairsRemainUnresolved()
    {
        using var file = File.OpenRead(Path.Combine(Folder, "catalog-pairs-full.mct"));
        var text = new MidasCivilTextReader("native-catalog").Import(file);
        Assert.AreEqual(ImportStatus.Partial, text.Status);
        Assert.IsFalse(text.VerificationEnabled);
        Assert.AreEqual(48, text.Model.BeamElements.Count);
        var api = CivilNxGeometryReader.Import(Snapshot(), Identity(), new CivilNxModelProfile());
        foreach (var beam in text.Model.BeamElements.Values)
        {
            var other = api.Model.BeamElements.Values.Single(b => b.Source.OriginalId == beam.Source.OriginalId);
            Equal(other.Assignments.SectionAxes.V1, beam.Assignments.SectionAxes.V1, 1e-12);
            Equal(other.Assignments.SectionAxes.V2, beam.Assignments.SectionAxes.V2, 1e-12);
            Equal(other.Assignments.SectionAxes.V3, beam.Assignments.SectionAxes.V3, 1e-12);
            if (int.Parse(beam.Source.OriginalId) <= 12) Assert.IsNull(beam.BeamProperty);
            else
            {
                var pair = (SectionBuiltUp)((SteelSection)beam.BeamProperty).SectionShape;
                var expected = (SectionBuiltUp)((SteelSection)other.BeamProperty).SectionShape;
                Assert.AreEqual(expected.Area, pair.Area, 1e-8); Assert.AreEqual(expected.Jxx, pair.Jxx, 1e-5); Assert.AreEqual(expected.Jyy, pair.Jyy, 1e-5);
            }
        }
    }
}
