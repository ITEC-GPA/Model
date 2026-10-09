using GPC.Model.Models;
using System.Net;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Geometry;
using GPC.Model.Combinations;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Analysis;
using GPC.Model.Checking.Preparation;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;

namespace UnitTest;

/// <summary>Results chosen and read after the import: the filter resolved on the model, the requests it gives, and the binding that
/// accepts a saved and reopened model with edited properties but not with edited geometry or loads.</summary>
[TestClass]
public class ResultFilterAndBindingTest
{
    // Beams 10 (1-2) and 12 (3-4) "I-A", 11 (2-3) "I-B" along X; plate 20 on 1-2-5-6; group Deck = {11, 20} with child Sub = {12};
    // supports at 1 and 4. Cases G, Q1, Q2, W; SLU = 1.35 G + 1.5 Q1, ENVQ = envelope (Q1, Q2), SLU2 = 1.3 G + 1.5 ENVQ, SRSS of W.
    private static GPC.Model.Models.Model Model()
    {
        var batch = new ImportBatch { Program = "Synthetic", ModelRevision = "r1", AnalysisId = "A", SourceHash = "H" };
        var points = new Dictionary<string, Point3d> { ["1"] = new(0, 0, 0), ["2"] = new(4000, 0, 0), ["3"] = new(8000, 0, 0), ["4"] = new(12000, 0, 0),
            ["5"] = new(4000, 3000, 0), ["6"] = new(0, 3000, 0) };
        foreach (var p in points) batch.Nodes.Add(new NodeRecord { Id = p.Key, GlobalPosition = p.Value, Record = "N" + p.Key });
        void Beam(string id, string i, string j, string section) => batch.Beams.Add(new BeamRecord { Id = id, I = i, J = j, MaterialId = "S", SectionId = section, Record = "B" + id,
            CoordinateSystem = Axes.Beam(points[i], points[j], new Vector3d(0, 1, 0), 0) });
        Beam("10", "1", "2", "A"); Beam("11", "2", "3", "B"); Beam("12", "3", "4", "A");
        batch.Shells.Add(new ShellRecord { Id = "20", Nodes = new[] { "1", "2", "5", "6" }, MaterialId = "C", ThicknessId = "T", Record = "P20",
            CoordinateSystem = new CoordinateSystem(new Point3d(2000, 1500, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0), new Vector3d(0, 0, 1)) });
        batch.Materials.Add(new MaterialRecord { Id = "S", Name = "S355", Kind = MaterialKind.Steel, Grade = "S355", Record = "M-S" });
        batch.Materials.Add(new MaterialRecord { Id = "C", Name = "C30/37", Kind = MaterialKind.Concrete, Record = "M-C" });
        batch.Sections.Add(new SectionRecord { Id = "A", Name = "I-A", Shape = SectionShapeKind.I, Dimensions = new double[] { 300, 150, 7, 10, 150, 10 }, Record = "S-A" });
        batch.Sections.Add(new SectionRecord { Id = "B", Name = "I-B", Shape = SectionShapeKind.I, Dimensions = new double[] { 400, 180, 8, 13, 180, 13 }, Record = "S-B" });
        batch.Thicknesses.Add(new ThicknessRecord { Id = "T", Name = "Slab 200", InPlane = 200, Record = "T" });
        var deck = new GroupRecord { Name = "Deck", Record = "G1" }; var sub = new GroupRecord { Name = "Sub", ParentName = "Deck", Record = "G2" };
        deck.Members.Add(new SourceIdentity("Synthetic", "r1", EntityFamily.Beam, "11")); deck.Members.Add(new SourceIdentity("Synthetic", "r1", EntityFamily.Shell, "20"));
        sub.Members.Add(new SourceIdentity("Synthetic", "r1", EntityFamily.Beam, "12")); batch.Groups.Add(deck); batch.Groups.Add(sub);
        foreach (var n in new[] { "1", "4" }) batch.NodeRestrains.Add(new NodeRestrainRecord { NodeId = n, FixedDofs = Enumerable.Repeat(true, 6).ToArray(), CoordinateSystem = CoordinateSystem.Global, Record = "R" + n });
        foreach (var c in new[] { "G", "Q1", "Q2", "W" }) batch.LoadCases.Add(new LoadCaseRecord { Name = c, Record = "LC " + c });
        CombinationTermRecord Case(string name, double f) => new() { Name = name, IsStaticCase = true, Analysis = "ST", Factor = f };
        batch.Combinations.Add(new CombinationRecord { Id = "C/SLU", Name = "SLU", Kind = CombinationKind.Linear, Record = "CB1", Terms = { Case("G", 1.35), Case("Q1", 1.5) } });
        batch.Combinations.Add(new CombinationRecord { Id = "C/ENVQ", Name = "ENVQ", Kind = CombinationKind.Envelope, Record = "CB2", Terms = { Case("Q1", 1), Case("Q2", 1) } });
        batch.Combinations.Add(new CombinationRecord { Id = "C/SLU2", Name = "SLU2", Kind = CombinationKind.Linear, Record = "CB3",
            Terms = { Case("G", 1.3), new CombinationTermRecord { Name = "C/ENVQ", IsCombination = true, Analysis = "CB", Factor = 1.5 } } });
        batch.Combinations.Add(new CombinationRecord { Id = "C/SRSS", Name = "SRSS", Kind = CombinationKind.Srss, Record = "CB4", Terms = { Case("W", 1) } });
        var report = ModelMapper.Map(batch);
        Assert.IsNotNull(report.Model, string.Join("; ", report.Diagnostics.Select(d => d.Code + ": " + d.Message)));
        return report.Model;
    }
    private static string Ids(IEnumerable<Element> elements) => string.Join(",", elements.Select(e => e.Source.OriginalId));
    private static ResultFilter Filter(string[]? groups = null, bool descendants = true, string[]? properties = null, EntityFamily[]? families = null) => new()
    {
        Elements = new ElementSelection { Groups = groups ?? new string[0], IncludeDescendants = descendants,
            Families = families ?? new[] { EntityFamily.Node, EntityFamily.Beam, EntityFamily.Shell } },
        Properties = properties ?? new string[0]
    };

    [TestMethod]
    public void Filter_GroupsPropertiesAndFamilies_SelectElementsAndTheirNodes()
    {
        var m = Model();
        var all = ResultFilter.All(m);
        Assert.AreEqual("10,11,12", Ids(all.Beams)); Assert.AreEqual("20", Ids(all.Shells)); Assert.AreEqual("1,2,3,4,5,6", Ids(all.Nodes));
        Assert.AreEqual("1,4", Ids(all.Supports)); CollectionAssert.AreEqual(new[] { "G", "Q1", "Q2", "W" }, all.StaticCases.ToArray());

        var deck = Filter(new[] { "Deck" }, descendants: false).Resolve(m);
        Assert.AreEqual("11", Ids(deck.Beams)); Assert.AreEqual("20", Ids(deck.Shells)); Assert.AreEqual("1,2,3,5,6", Ids(deck.Nodes)); Assert.AreEqual("1", Ids(deck.Supports));
        Assert.AreEqual("11,12", Ids(Filter(new[] { "Deck" }).Resolve(m).Beams), "Subgroups included by default.");

        var section = Filter(properties: new[] { "I-A" }).Resolve(m);
        Assert.AreEqual("10,12", Ids(section.Beams)); Assert.AreEqual(0, section.Shells.Count); Assert.AreEqual("1,2,3,4", Ids(section.Nodes));
        var both = Filter(new[] { "Deck" }, properties: new[] { "I-A" }).Resolve(m);
        Assert.AreEqual("12", Ids(both.Beams)); Assert.AreEqual("3,4", Ids(both.Nodes)); Assert.AreEqual("4", Ids(both.Supports));
        Assert.AreEqual("20", Ids(Filter(properties: new[] { "Slab 200" }).Resolve(m).Shells));

        var beamsOnly = Filter(families: new[] { EntityFamily.Beam }).Resolve(m);
        Assert.AreEqual(3, beamsOnly.Beams.Count); Assert.AreEqual(0, beamsOnly.Shells.Count + beamsOnly.Nodes.Count + beamsOnly.Supports.Count);
        var nodesOnly = Filter(families: new[] { EntityFamily.Node }); nodesOnly.Elements.Elements = new[] { new ElementKey { Family = EntityFamily.Node, Id = m.NodesElements.Values.Single(n => n.Source.OriginalId == "4").Id } };
        var plan = nodesOnly.Resolve(m); Assert.AreEqual("4", Ids(plan.Nodes)); Assert.AreEqual("4", Ids(plan.Supports)); Assert.AreEqual(0, plan.Beams.Count);

        Assert.ThrowsException<ArgumentException>(() => Filter(properties: new[] { "HEB 999" }).Resolve(m));
        Assert.ThrowsException<InvalidOperationException>(() => Filter(new[] { "Missing" }).Resolve(m));
    }

    [TestMethod]
    public void Filter_CombinationsReadTheirStaticCasesOnly()
    {
        var m = Model();
        CollectionAssert.AreEqual(new[] { "G", "Q1", "Q2" }, new ResultFilter { Combinations = new[] { "SLU2" } }.Resolve(m).StaticCases.ToArray(), "Nested envelope, not expanded.");
        var mixed = new ResultFilter { StaticCases = new[] { "W" }, Combinations = new[] { "C/SLU" } }.Resolve(m);
        CollectionAssert.AreEqual(new[] { "W", "G", "Q1" }, mixed.StaticCases.ToArray()); CollectionAssert.AreEqual(new[] { "C/SLU" }, mixed.Combinations.ToArray());
        var user = new Combination("User"); user.AddLoadCaseCoefficient(m.LoadCases["Q2"], 1); user.AddLoadCaseCoefficient(m.LoadCases["W"], 0.6); m.Combinations.Add(user);
        CollectionAssert.AreEqual(new[] { "Q2", "W" }, new ResultFilter { Combinations = new[] { "User" } }.Resolve(m).StaticCases.ToArray(), "Model combination added after the import.");
        Assert.ThrowsException<NotSupportedException>(() => new ResultFilter { Combinations = new[] { "SRSS" } }.Resolve(m));
        Assert.ThrowsException<ArgumentException>(() => new ResultFilter { Combinations = new[] { "Missing" } }.Resolve(m));
        Assert.ThrowsException<ArgumentException>(() => new ResultFilter { StaticCases = new[] { "Missing" } }.Resolve(m));
    }

    private static CivilNxResponse Table(string head, params string[] rows) =>
        new("post/table", "{\"T\":{\"FORCE\":\"KN\",\"DIST\":\"M\",\"HEAD\":[" + head + "],\"DATA\":[" + string.Join(",", rows) + "]}}");
    private static readonly CivilNxResponse BeamTable = Table("\"Index\",\"Elem\",\"Load\",\"Part\",\"Axial\",\"Shear-y\",\"Shear-z\",\"Torsion\",\"Moment-y\",\"Moment-z\"",
        "[\"1\",\"3\",\"Q\",\"I[2]\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]", "[\"2\",\"3\",\"Q\",\"J[3]\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]");
    private static GPC.Model.Models.Model Reopened(GPC.Model.Models.Model model)
    {
        using var archive = new MemoryStream(); ModelArchive.Save(model, archive); archive.Position = 0; return ModelArchive.Load(archive);
    }

    [TestMethod]
    public void Binding_AcceptsAReopenedModelWithEditedPropertiesAndCombinations_WithAWarning()
    {
        var imported = CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(CivilNxModelWorkflowTest.Json()), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile()).Model;
        var snapshot = CivilNxModelWorkflowTest.Snapshot(CivilNxModelWorkflowTest.Json());
        var unchanged = Reopened(imported); var report = CivilNxResults.Import(unchanged, snapshot, new[] { BeamTable }, "d0");
        Assert.AreEqual(ImportStatus.Completed, report.Status); Assert.IsFalse(report.Diagnostics.Any(d => d.Code == SourceBinding.PropertiesChangedCode));

        var edited = Reopened(imported);
        edited.AreaElements.Values.First().Assignments.PhysicalThickness = 350;
        var user = new Combination("User"); user.AddLoadCaseCoefficient(edited.LoadCases["Q"], 1.5); edited.Combinations.Add(user);
        report = CivilNxResults.Import(edited, snapshot, new[] { BeamTable }, "d1");
        Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        Assert.AreEqual(DiagnosticSeverity.Warning, report.Diagnostics.Single(d => d.Code == SourceBinding.PropertiesChangedCode).Severity);
        Assert.AreEqual(imported.Analysis.InputFingerprint, edited.Datasets["d1"].InputFingerprint);
        Assert.AreNotEqual(edited.AnalysisFingerprint(), edited.Datasets["d1"].InputFingerprint);
        Assert.IsTrue(edited.BeamElements.Values.SelectMany(b => b.Results).SelectMany(r => r.Results)
            .All(r => r.State.InputFingerprint == imported.Analysis.InputFingerprint));
        Assert.AreEqual(AnalysisCompatibility.RequiresReanalysis, AnalysisCompatibilityValidator.Validate(edited).Status);
        Assert.AreEqual(2, edited.BeamElements.Values.Sum(b => b.Results.Sum(r => r.Results.Count)));

        var moved = Reopened(imported); moved.NodesElements.Values.First().Position.X += 1;
        Assert.AreEqual(ImportStatus.Rejected, CivilNxResults.Import(moved, snapshot, new[] { BeamTable }, "d2").Status, "Geometry changed.");
        var loaded = Reopened(imported);
        loaded.NodesElements.Values.First().Loads.Add(new PointLoad(1, 0, 0, 0, 0, 0, loaded.NodesElements.Values.First().Position, loaded.LoadCases["Q"]));
        Assert.AreEqual(ImportStatus.Rejected, CivilNxResults.Import(loaded, snapshot, new[] { BeamTable }, "d3").Status, "Loads changed.");
    }

    [TestMethod]
    public void EarlierBindingWithoutSnapshotRetainsOriginalFingerprintAndUnknownHistory()
    {
        var snapshot = CivilNxModelWorkflowTest.Snapshot(CivilNxModelWorkflowTest.Json());
        var imported = CivilNxGeometryReader.Import(snapshot, CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile()).Model;
        var original = imported.AnalysisFingerprint();
        // This is the persisted representation preceding independent analysis snapshots.
        var legacy = Reopened(imported.AnalysisArchiveView());
        legacy.AreaElements.Values.First().Assignments.PhysicalThickness = 350;
        var report = CivilNxResults.Import(legacy, snapshot, new[] { BeamTable }, "historical");
        Assert.AreEqual(ImportStatus.Completed, report.Status);
        Assert.AreEqual(original, legacy.Datasets["historical"].InputFingerprint);
        Assert.IsNull(legacy.Analysis); Assert.IsNull(legacy.Datasets["historical"].AnalysisSnapshotId);
        Assert.AreEqual(AnalysisCompatibility.Unknown, AnalysisCompatibilityValidator.Validate(legacy).Status);
    }

    [TestMethod]
    public void Binding_StoredByEarlierImports_StaysExact()
    {
        var model = CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(CivilNxModelWorkflowTest.Json()), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile()).Model;
        model.PreservedSourceData.Single(p => p.Kind == CivilNxGeometryReader.InputBindingKind).RawData = model.AnalysisFingerprint();
        var snapshot = CivilNxModelWorkflowTest.Snapshot(CivilNxModelWorkflowTest.Json());
        var plate = model.AreaElements.Values.First(); var thickness = plate.Assignments.PhysicalThickness;
        plate.Assignments.PhysicalThickness = 350;
        Assert.AreEqual(ImportStatus.Rejected, CivilNxResults.Import(model, snapshot, new[] { BeamTable }, "d").Status);
        plate.Assignments.PhysicalThickness = thickness;
        Assert.AreEqual(ImportStatus.Completed, CivilNxResults.Import(model, snapshot, new[] { BeamTable }, "d").Status);
    }

    private sealed class Recorder : HttpMessageHandler
    {
        public readonly List<string> Bodies = new();
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Bodies.Add(await request.Content!.ReadAsStringAsync(token));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"T\":{\"HEAD\":[],\"DATA\":[]}}") };
        }
    }

    [TestMethod]
    public async Task CivilNxPlan_RequestsOnlyTheFilteredElementsAndCases()
    {
        var model = CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(CivilNxModelWorkflowTest.Json()), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile()).Model;
        var recorder = new Recorder(); using var client = new CivilNxApiClient(new Uri("https://example.invalid/civil"), () => "test-only-key", recorder);
        var filter = Filter(new[] { "Impalcato" }); filter.StaticCases = new[] { "Q" };
        await CivilNxResults.ReadAsync(client, model, filter.Resolve(model));
        Assert.AreEqual(3, recorder.Bodies.Count, "Beams, plates and displacements; no support in the group, so no reactions.");
        StringAssert.Contains(recorder.Bodies[0], "\"TABLE_TYPE\":\"BEAMFORCE\""); StringAssert.Contains(recorder.Bodies[0], "\"KEYS\":[3]");
        StringAssert.Contains(recorder.Bodies[1], "\"TABLE_TYPE\":\"PLATEFORCEUL\""); StringAssert.Contains(recorder.Bodies[1], "\"KEYS\":[10]");
        StringAssert.Contains(recorder.Bodies[2], "\"TABLE_TYPE\":\"DISPLACEMENTG\""); StringAssert.Contains(recorder.Bodies[2], "\"KEYS\":[2,3,4,5]");
        Assert.IsTrue(recorder.Bodies.All(b => b.Contains("\"LOAD_CASE_NAMES\":[\"Q(ST)\"]")));

        recorder.Bodies.Clear(); await CivilNxResults.ReadPlateNodesAsync(client, model, Filter(properties: new[] { "Soletta" }).Resolve(model));
        Assert.AreEqual(1, recorder.Bodies.Count); StringAssert.Contains(recorder.Bodies[0], "\"KEYS\":[10]"); StringAssert.Contains(recorder.Bodies[0], "\"LOAD_CASE_NAMES\":[\"G1(ST)\",\"Q(ST)\"]");
    }
}
