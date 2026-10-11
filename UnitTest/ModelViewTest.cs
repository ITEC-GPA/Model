using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Examples;
using GPC.Geometry;
using GPC.Model.Core.Identity;
using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Presentation;
using GPC.Model.Results.Locations;

namespace UnitTest;

/// <summary>
/// Architecture review R6: the model viewer is a client of GPC Model. <see cref="ModelView"/> lists geometry, properties, groups, boundaries,
/// cases and datasets with the identities of the model, and reads the results of a dataset on demand in the GPC conventions applied by the
/// Converter (Civil NX: M1 = −Moment-y, plate Mxx, Myy, Mxy = minus the MIDAS values), without decoding the compact results of the model.
/// </summary>
[TestClass]
public class ModelViewTest
{
    private static CivilNxResponse Table(string head, params string[] rows) =>
        new("post/table", "{\"T\":{\"FORCE\":\"KN\",\"DIST\":\"M\",\"HEAD\":[" + head + "],\"DATA\":[" + string.Join(",", rows) + "]}}");

    /// <summary>The Civil NX fixture of CivilNxModelWorkflowTest with its results: beam 3, plate 11 (centre and node 2), node 3, support 1.</summary>
    private static GPC.Model.Models.Model CivilNx()
    {
        var report = CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(CivilNxModelWorkflowTest.Json()), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile());
        Assert.AreEqual(ImportStatus.Partial, report.Status);
        var m = report.Model;
        var tables = new[]
        {
            Table("\"Index\",\"Elem\",\"Load\",\"Part\",\"Axial\",\"Shear-y\",\"Shear-z\",\"Torsion\",\"Moment-y\",\"Moment-z\"",
                "[\"1\",\"3\",\"Q\",\"I[2]\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]", "[\"2\",\"3\",\"Q\",\"1/4\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]",
                "[\"3\",\"3\",\"Q\",\"J[3]\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]"),
            Table("\"Index\",\"Elem\",\"Load\",\"Node\",\"Fxx\",\"Fyy\",\"Fxy\",\"Fmax\",\"Fmin\",\"Angle\",\"Mxx\",\"Myy\",\"Mxy\",\"Mmax\",\"Mmin\",\"Angle\",\"Vxx\",\"Vyy\"",
                "[\"1\",\"11\",\"Q\",\"Cent\",\"1\",\"2\",\"3\",\"0\",\"0\",\"0\",\"4\",\"5\",\"6\",\"0\",\"0\",\"0\",\"7\",\"8\"]",
                "[\"2\",\"11\",\"Q\",\"2\",\"9\",\"9\",\"9\",\"0\",\"0\",\"0\",\"9\",\"9\",\"9\",\"0\",\"0\",\"0\",\"9\",\"9\"]"),
            Table("\"Index\",\"Node\",\"Load\",\"FX\",\"FY\",\"FZ\",\"MX\",\"MY\",\"MZ\"", "[\"1\",\"1\",\"Q\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]"),
            Table("\"Index\",\"Node\",\"Load\",\"DX\",\"DY\",\"DZ\",\"RX\",\"RY\",\"RZ\"", "[\"1\",\"3\",\"Q\",\"0.001\",\"0\",\"-0.002\",\"0\",\"0.0001\",\"0\"]"),
        };
        var results = CivilNxResults.Import(m, CivilNxModelWorkflowTest.Snapshot(CivilNxModelWorkflowTest.Json()), tables, "dataset");
        Assert.AreEqual(ImportStatus.Completed, results.Status, string.Join("; ", results.Diagnostics.Select(d => d.Message)));
        return m;
    }

    private static ViewElement Element(ModelView view, ViewFamily family, string source) => view.Elements.Single(e => e.Family == family && e.SourceId == source);

    [TestMethod]
    public void GeometryAndIdentitiesComeFromTheModel()
    {
        var m = CivilNx();
        var view = ModelView.Open(m);
        Assert.AreEqual(1, ModelView.Schema);
        Assert.AreEqual(m.AnalysisFingerprint(), view.Revision);
        Assert.AreEqual(m.Guid, view.ModelGuid);
        Assert.AreEqual("N,mm,rad", view.Units);

        Assert.AreEqual(5, view.Nodes.Count);
        var node3 = view.Nodes.Single(n => n.SourceId == "3");
        Assert.AreEqual(new ViewVector(4000, 0, 3000), node3.Position);
        Assert.AreEqual(m.NodesElements[node3.Id].Guid, node3.Guid);

        var girder = Element(view, ViewFamily.Beam, "3"); var model = (BeamElement)m.FindBySource(new SourceIdentity("MIDAS Civil NX", "rev", EntityFamily.Beam, "3"))!;
        Assert.AreEqual(model.Id, girder.Id); Assert.AreEqual(model.Guid, girder.Guid);
        CollectionAssert.AreEqual(new[] { model.NodeI.Id, model.NodeJ.Id }, girder.Nodes.ToArray());
        Assert.AreEqual(1, girder.Axes.V3.X, 1e-12, "Beam V3 from I to J.");
        Assert.AreEqual(-150, girder.CentroidOffset!.Value.Y, 1e-6, "CT: reference line at the top of HE 300 B.");
        var section = view.Properties.Single(p => p.Family == ViewFamily.Beam && p.Name == girder.Property);
        Assert.AreEqual("SteelSection", section.Kind); Assert.AreEqual(300, section.Height!.Value, 1e-9);
        Assert.IsTrue(section.Outline.Count >= 1 && section.Outline[0].Count >= 12, "The H outline, not a rectangle.");

        var plate = Element(view, ViewFamily.Plate, "11");
        Assert.AreEqual(600, plate.Thickness); Assert.AreEqual(-100, plate.Offset);
        Assert.AreEqual(1, plate.Axes.V1.Y, 1e-12, "Element axes rotated by ANGLE 90.");
        Assert.AreEqual(4, plate.Nodes.Count);

        Assert.AreEqual(4, view.Groups.Single(g => g.Name == "Impalcato").Members.Count);
        var support = view.Boundaries.Single(b => b.Kind == "Restraint");
        Assert.AreEqual(view.Nodes.Single(n => n.SourceId == "1").Id, support.Nodes.Single());
        Assert.AreEqual("DX, DY, DZ, RX, RY", support.Dofs);
        CollectionAssert.AreEquivalent(new[] { "G1", "Q" }, view.Cases.Where(c => !c.IsCombination).Select(c => c.Name).ToArray());
        var dataset = view.Datasets.Single();
        Assert.AreEqual("dataset", dataset.Id); Assert.AreEqual("MIDAS Civil NX", dataset.Program); Assert.AreEqual("N,mm,rad", dataset.Units);

        Assert.IsTrue(view.IsCurrent);
        m.NodesElements[node3.Id].Position = new Point3d(4000, 0, 3100);
        Assert.IsFalse(view.IsCurrent, "An edit of the model inputs after the view was opened.");
        Assert.AreEqual(new ViewVector(4000, 0, 3000), node3.Position, "The view keeps the copy it was opened with.");
    }

    [TestMethod]
    public void ResultsAreTheValuesOfTheModelInTheGpcConventions()
    {
        var view = ModelView.Open(CivilNx());
        var results = view.Results("dataset");
        CollectionAssert.AreEqual(new[] { "Q" }, results.Cases.ToArray());
        Assert.AreEqual(0, results.Skipped.Count);

        var girder = Element(view, ViewFamily.Beam, "3");
        var m1 = results.Field("Q", ViewFamily.Beam, "M1")!;
        Assert.AreEqual("N·mm", m1.Unit); Assert.AreEqual("section", m1.Axes); Assert.AreEqual("dataset", m1.Dataset);
        Assert.AreEqual(3, m1.Samples.Count);
        var quarter = m1.Samples.Single(s => s.Station == .25);
        Assert.AreEqual(girder.Id, quarter.Element);
        Assert.AreEqual(-5e6, quarter.Value, "M1 = -Moment-y: 5 kN·m in the MIDAS table.");
        Assert.AreEqual(6e6, results.Field("Q", ViewFamily.Beam, "M2")!.Samples.Single(s => s.Station == .25).Value);
        Assert.AreEqual(1000, results.Field("Q", ViewFamily.Beam, "N")!.Samples.Single(s => s.Station == 0).Value);

        var mxx = results.Field("Q", ViewFamily.Plate, "Mxx")!;
        Assert.AreEqual("N·mm/mm", mxx.Unit); Assert.AreEqual("element", mxx.Axes);
        Assert.AreEqual(-4000, mxx.Samples.Single(s => s.PointKind == ShellResultPointKind.Centroid).Value, "Mxx = -Mxx of MIDAS, 4 kN·m/m.");
        var atNode = mxx.Samples.Single(s => s.PointKind == ShellResultPointKind.ElementNodeExtrapolated);
        Assert.AreEqual(view.Nodes.Single(n => n.SourceId == "2").Id, atNode.Node);
        Assert.AreEqual(-9000, atNode.Value);
        Assert.AreEqual(7, results.Field("Q", ViewFamily.Plate, "Fxz")!.Samples.Single(s => s.PointKind == ShellResultPointKind.Centroid).Value);

        var d1 = results.Field("Q", ViewFamily.Node, "D1")!;
        Assert.AreEqual("mm", d1.Unit); Assert.AreEqual("global", d1.Axes);
        Assert.AreEqual(1, d1.Samples.Single().Value, 1e-12);
        Assert.AreEqual(view.Nodes.Single(n => n.SourceId == "3").Id, d1.Samples.Single().Element);
        Assert.AreEqual(3000, results.Field("Q", ViewFamily.Node, "Fz")!.Samples.Single().Value);

        Assert.IsNull(results.Field("G1", ViewFamily.Beam, "M1"), "No results for G1.");
        Assert.IsNull(results.Field("Q", ViewFamily.Beam, "Mxx"), "Not a beam component.");
        Assert.AreEqual(6 + 8 + 6 + 6, results.Fields.Count);
        Assert.ThrowsException<KeyNotFoundException>(() => view.Results("other"));
    }

    [TestMethod]
    public void TheModelKeepsItsResultsCompact()
    {
        var m = CivilNx();
        using var stream = new MemoryStream(); ModelArchive.Save(m, stream); stream.Position = 0;
        var archived = ModelArchive.Load(stream);
        var view = ModelView.Open(archived);
        var field = view.Results("dataset").Field("Q", ViewFamily.Beam, "M1")!;
        Assert.AreEqual(-5e6, field.Samples.Single(s => s.Station == .25).Value);
        var withResults = archived.AllElements.Where(e => e.ContainsResultsOf("dataset")).ToArray();
        Assert.AreEqual(4, withResults.Length, "Beam 3, plate 11, nodes 1 and 3.");
        Assert.IsFalse(withResults.Any(e => e.ResultsLoaded), "The view decodes the results in its own columns, not in the elements.");
        Assert.AreSame(view.Results("dataset"), view.Results("dataset"), "A dataset is read once.");
        view.Release("dataset");
        Assert.AreEqual(3, view.Results("dataset").Field("Q", ViewFamily.Beam, "M1")!.Samples.Count);
    }

    [TestMethod]
    public void TheSyntheticModelGivesItsSamplesUnchanged()
    {
        var m = MixedModelFactory.Create();
        var view = ModelView.Open(m);
        var release = view.Boundaries.Single(b => b.Kind == "Release");
        Assert.AreEqual(new ViewKey(ViewFamily.Beam, 250), release.Element); Assert.AreEqual("RZ: Released", release.Dofs);
        Assert.AreEqual(40, release.Nodes.Single(), "Released at J.");
        Assert.AreEqual(2, view.Groups.Single(g => g.Name == "Wall").Members.Count);
        Assert.AreEqual(130, view.Boundaries.Single(b => b.Kind == "GroundSpring").Nodes.Single());
        Assert.AreEqual("DX, DY, DZ, RX, RY, RZ", view.Boundaries.Single(b => b.Kind == "Restraint").Dofs);
        var section = view.Properties.Single(p => p.Family == ViewFamily.Beam);
        Assert.AreEqual("ReinforcedConcreteSection", section.Kind); Assert.AreEqual(500, section.Height!.Value, 1e-9); Assert.AreEqual(300, section.Width!.Value, 1e-9);
        Assert.AreEqual(300, view.Elements.First(e => e.Family == ViewFamily.Plate).Thickness);

        var results = view.Results("synthetic-static");
        CollectionAssert.AreEqual(new[] { "P+", "P-" }, results.Cases.ToArray());
        var m1 = results.Field("P+", ViewFamily.Beam, "M1")!;
        var expected = ResultQueriesOf(m, "P+").Select(s => (s.ParametricDistance, s.ResultBeamForces.M1)).ToArray();
        CollectionAssert.AreEqual(expected, m1.Samples.Select(s => (s.Station!.Value, s.Value)).ToArray());
        Assert.AreEqual(-2e6, m1.Samples.First().Value, "Cantilever of 2 m with P = 1 kN at J: M1 = -P L at I.");
        Assert.AreEqual(2, results.Field("P-", ViewFamily.Plate, "Mxx")!.Samples.Count);
        Assert.AreEqual(-80000, results.Field("P-", ViewFamily.Plate, "Mxx")!.Samples[0].Value);
    }

    private static StationResultBeamForces[] ResultQueriesOf(GPC.Model.Models.Model m, string caseName) =>
        GPC.Model.Results.Queries.ResultQueries.BeamSamples(m.BeamElements[250], "synthetic-static", caseName).ToArray();
}
