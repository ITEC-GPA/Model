using System.Globalization;
using System.Net;
using System.Text.Json;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Model.Analysis;
using GPC.Model.Core;
using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;
using GPC.Model.Stages;

namespace UnitTest;

/// <summary>Invented numeric values on the documented protocol; these are not results validated against a real staged solver model.</summary>
[TestClass]
public class CivilNxConstructionStagesTest
{
    private static Dictionary<string, string> Json()
    {
        var json = CivilNxModelWorkflowTest.Json();
        json["STAG"] = JsonSerializer.Serialize(new { STAG = new Dictionary<string, object> {
            ["10"] = new { NAME = "Completamento", DURATION = 10.0, bSV_RSLT = true, bSV_STEP = true, bLOAD_STEP = false, INCRE_STEP = 0,
                ADD_STEP = new[] { 2.0, 5.0 }, DACT_ELEM = new[] { new { GRUP_NAME = "Casseri", REDIST = 50.0 } },
                ACT_BNGR = new[] { new { BNGR_NAME = "Appoggi", POS = "DEFORMED" } }, DACT_BNGR = new[] { "Provvisori" },
                ACT_LOAD = new[] { new { LOAD_NAME = "Peso", DAY = "FIRST" } }, DACT_LOAD = new[] { new { LOAD_NAME = "Peso", DAY = "5" } } },
            ["2"] = new { NAME = "Getto", DURATION = 0.0, bSV_RSLT = true, ACT_ELEM = new[] { new { GRUP_NAME = "Impalcato", AGE = 7.0 } } }
        } });
        json["STCT"] = JsonSerializer.Serialize(new { STCT = new Dictionary<string, object> { ["1"] = new {
            iINC_NLA = 0, iNLA_TYPE = 1, bINC_TDE = true, vEREC = new[] { new { LTYPECC = "Erection", EREC = 0, vLCNAME = new[] { "G1" } } }
        } } });
        return json;
    }
    private static CivilNxStageSelection Selection(CivilNxDisplacementReference reference = CivilNxDisplacementReference.Accumulative) => new(new[] {
        new CivilNxStageStep("Getto", "001(last)"), new CivilNxStageStep("Completamento", "001(first)"), new CivilNxStageStep("Completamento", "002(last)")
    }, new[] { "Summation" }, reference);
    private static (GPC.Model.Models.Model Model, CivilNxSnapshot Snapshot) Import(Dictionary<string, string>? json = null)
    {
        var snapshot = CivilNxModelWorkflowTest.Snapshot(json ?? Json());
        var report = CivilNxGeometryReader.Import(snapshot, CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile());
        Assert.IsNotNull(report.Model, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        return (report.Model, snapshot);
    }
    private static CivilNxResponse Table(string[] headers, IEnumerable<string[]> rows, string force = "KN", string length = "M") =>
        new("post/TABLE", JsonSerializer.Serialize(new { T = new { FORCE = force, DIST = length, HEAD = headers, DATA = rows.ToArray() } }));
    private static CivilNxResponse[] Results(CivilNxStageSelection? selection = null)
    {
        selection ??= Selection();
        string[] Row(CivilNxStageStep step, int i, string id, string? location, params double[] values) =>
            new[] { (i + 1).ToString(), id, "Summation", step.Phase, step.Step }.Concat(location == null ? Array.Empty<string>() : new[] { location })
                .Concat(values.Select(v => ((i + 1) * v).ToString("R", CultureInfo.InvariantCulture))).ToArray();
        var beams = new[] { "Index", "Elem", "Load", "Stage", "Step", "Part", "Axial", "Shear-y", "Shear-z", "Torsion", "Moment-y", "Moment-z" };
        var plates = new[] { "Index", "Elem", "Load", "Stage", "Step", "Node", "Fxx", "Fyy", "Fxy", "Mxx", "Myy", "Mxy", "Vxx", "Vyy" };
        var nodes = new[] { "Index", "Node", "Load", "Stage", "Step", "DX", "DY", "DZ", "RX", "RY", "RZ" };
        var reactions = new[] { "Index", "Node", "Load", "Stage", "Step", "FX", "FY", "FZ", "MX", "MY", "MZ" };
        return new[] {
            Table(beams, selection.Steps.Select((s,i) => Row(s, i, "3", "I[2]", 1, 2, 3, 4, 5, 6))),
            Table(plates, selection.Steps.Select((s,i) => Row(s, i, "10", "Cent", 1, 2, 3, 4, 5, 6, 7, 8))),
            Table(nodes, selection.Steps.Select((s,i) => Row(s, i, "3", null, 0.001, 0.002, -0.003, 0.01, 0.02, 0.03))),
            Table(reactions, selection.Steps.Select((s,i) => Row(s, i, "1", null, 1, 2, 3, 4, 5, 6)))
        };
    }

    [TestMethod]
    public void ScheduleCatalogReusesParserWithoutGeometry()
    {
        var tables = Json().Where(p => p.Key == "STAG" || p.Key == "STCT").ToDictionary(p => p.Key, p => p.Value);
        var batch = new CivilNxModelProfile().ReadConstructionStages(CivilNxModelWorkflowTest.Snapshot(tables), CivilNxModelWorkflowTest.Identity());
        Assert.AreEqual(2, batch.Stages.Count);
        Assert.AreEqual(0, batch.Nodes.Count);
        CollectionAssert.AreEquivalent(new[] { "Summation", "Erection" }, batch.LoadCases.Select(c => c.StageSourceCaseName).ToArray());
        Assert.IsTrue(batch.Diagnostics.Any(d => d.Code == "CivilNxConstructionScheduleImported"));
    }

    [TestMethod]
    public void ScheduleMapsOrderDurationAgesLoadsAndBoundaryPositions()
    {
        var (model, _) = Import(); var first = model.Stages[2]; var second = model.Stages[10];
        Assert.AreEqual(1, first.Definition.Sequence); Assert.AreEqual(0, first.Definition.DurationDays);
        Assert.AreEqual(7, first.Definition.GroupChanges.Single().ElementAgeDays);
        Assert.AreEqual(2, second.Definition.Sequence); Assert.AreEqual(10, second.Definition.DurationDays);
        CollectionAssert.AreEqual(new[] { 2.0, 5.0 }, second.Definition.AdditionalOutputDays.ToArray());
        Assert.AreEqual(0.5, second.Definition.GroupChanges.Single(c => c.Kind == StageGroupKind.Structure).RedistributionRatio);
        Assert.AreEqual(StageBoundaryPosition.Deformed, second.Definition.GroupChanges.Single(c => c.GroupName == "Appoggi").BoundaryPosition);
        Assert.AreEqual(5, second.Definition.GroupChanges.Single(c => c.Kind == StageGroupKind.Load && c.Action == StageGroupAction.Deactivate).Day);
        Assert.AreEqual(2, model.LoadCases.Values.OfType<StageResultCase>().Count());
        Assert.IsTrue(model.LoadCases.ContainsKey("Erection(CS)"));
        Assert.IsFalse(model.NodesElements.Values.Any(n => n.Assignments.Restrains.Count != 0), "Staged supports must not become permanent constraints.");
        CollectionAssert.AreEquivalent(new[] { "G1", "Q" }, ResultFilter.All(model).StaticCases.ToArray());
        Assert.ThrowsException<ArgumentException>(() => ResultFilter.All(model, new[] { "Summation(CS)" }));
        Assert.IsTrue(model.PreservedSourceData.Any(p => p.RawData.Contains("bINC_TDE")));
    }

    [DataTestMethod]
    [DataRow(CivilNxDisplacementReference.Accumulative)] [DataRow(CivilNxDisplacementReference.Current)] [DataRow(CivilNxDisplacementReference.Real)]
    public void PhaseStepsAndDisplacementReferenceSurviveTheResultArchive(CivilNxDisplacementReference reference)
    {
        var (model, snapshot) = Import(); var before = model.AnalysisFingerprint(); var selection = Selection(reference);
        var report = CivilNxResults.ImportStages(model, snapshot, Results(), "staged", selection, isSynthetic: true);
        Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        Assert.AreEqual(before, model.AnalysisFingerprint());
        using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0; model = ModelArchive.Load(stream);
        Assert.AreEqual(before, model.AnalysisFingerprint());
        var all = model.AllElements.SelectMany(e => e.Results.SelectMany(r => r.Results)).ToArray();
        Assert.AreEqual(12, all.Length); Assert.IsTrue(all.All(r => r.Case.Name == "Summation(CS)"));
        Assert.IsTrue(all.All(r => r.State.Semantics == AnalysisSemantics.Unknown && r.State.IsSynthetic));
        var beams = all.OfType<StationResultBeamForces>().ToArray(); Assert.AreEqual(3, beams.Length);
        Assert.AreEqual(1000, beams[0].ResultBeamForces.N); Assert.AreEqual(-5e6, beams[0].ResultBeamForces.M1); Assert.AreEqual(6e6, beams[0].ResultBeamForces.M2);
        Assert.AreEqual(3000, beams[2].ResultBeamForces.N); Assert.AreEqual("002(last)", beams[2].State.Step);
        Assert.IsTrue(beams.All(b => b.State.IsCumulative == null));
        var plates = all.OfType<PointResultPlateForces>().ToArray();
        Assert.AreEqual(1, plates[0].Forces.Fxx); Assert.AreEqual(-4000, plates[0].Forces.Mxx); Assert.AreEqual(-12000, plates[2].Forces.Mxx);
        var displacements = all.OfType<NodeResultDisplacement>().ToArray();
        Assert.AreEqual(1, displacements[0].ResultDisplacement.D1); Assert.AreEqual(-9, displacements[2].ResultDisplacement.D3, 1e-12);
        Assert.AreEqual(0.09, displacements[2].ResultDisplacement.R3, 1e-12);
        Assert.IsTrue(displacements.All(d => d.State.SourceDisplacementReference == reference.ToString()));
        Assert.AreEqual(reference == CivilNxDisplacementReference.Accumulative ? true : (bool?)null, displacements[0].State.IsCumulative);
        Assert.AreEqual(3, displacements.Select(d => d.State.ConcomitantStateId).Distinct().Count());
        Assert.IsTrue(displacements.All(d => d.State.Original.ReaderVersion == CivilNxResults.StageReaderVersion));
        Assert.AreEqual(3000, all.OfType<NodeResultForces>().First().Fz);
    }

    [TestMethod]
    public void StaticTablesCannotAbsorbStageDataAndScopeCollisionsAreRejected()
    {
        var (model, snapshot) = Import();
        Assert.AreEqual(ImportStatus.Rejected, CivilNxResults.Import(model, snapshot, Results(), "wrong").Status);
        var json = Json(); json["STLD"] = json["STLD"].Replace("\"G1\"", "\"Summation(CS)\"");
        Assert.AreEqual(ImportStatus.Rejected, CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(json), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile()).Status);
        Assert.AreEqual(0, model.Datasets.Count);
    }

    [DataTestMethod]
    [DataRow("unknown-phase")] [DataRow("unknown-step")] [DataRow("unknown-case")] [DataRow("missing-column")]
    [DataRow("duplicate")] [DataRow("missing-state")] [DataRow("bad-number")] [DataRow("mixed-units")]
    public void InvalidStageResultsAreRejectedWithoutPartialAttachment(string fault)
    {
        var (model, snapshot) = Import(); var tables = Results().ToList();
        switch (fault)
        {
            case "unknown-phase": tables[0] = new("post/TABLE", tables[0].Json.Replace("Getto", "Unknown")); break;
            case "unknown-step": tables[0] = new("post/TABLE", tables[0].Json.Replace("001(last)", "099(last)")); break;
            case "unknown-case": tables[0] = new("post/TABLE", tables[0].Json.Replace("Summation", "Unknown")); break;
            case "missing-column": tables[0] = new("post/TABLE", tables[0].Json.Replace("\"Stage\"", "\"FutureColumn\"")); break;
            case "duplicate": tables.Add(tables[0]); break;
            case "missing-state": tables = Results(new CivilNxStageSelection(Selection().Steps.Take(1), new[] { "Summation" }, CivilNxDisplacementReference.Accumulative)).ToList(); break;
            case "bad-number": tables[0] = new("post/TABLE", tables[0].Json.Replace("\"5\"", "\"NaN\"")); break;
            case "mixed-units": tables[1] = new("post/TABLE", tables[1].Json.Replace("\"KN\"", "\"N\"")); break;
        }
        var report = CivilNxResults.ImportStages(model, snapshot, tables, "bad", Selection(), true);
        Assert.AreEqual(ImportStatus.Rejected, report.Status, fault); Assert.AreEqual(0, model.Datasets.Count);
        Assert.IsFalse(model.AllElements.Any(e => e.HasResults));
    }

    [TestMethod]
    public void ChangedScheduleAndCancellationRejectResults()
    {
        var (model, snapshot) = Import();
        model.Stages[2].Definition = new StageDefinition(1, 11);
        Assert.AreEqual(ImportStatus.Rejected, CivilNxResults.ImportStages(model, snapshot, Results(), "changed", Selection()).Status);
        (model, snapshot) = Import();
        Assert.AreEqual(ImportStatus.Cancelled, CivilNxResults.ImportStages(model, snapshot, Results(), "cancelled", Selection(), cancellationToken: new CancellationToken(true)).Status);
        Assert.AreEqual(0, model.Datasets.Count);
    }

    [TestMethod]
    public void StageRequestsPreserveLegacyDefaultsAndValidateExplicitReference()
    {
        var request = new CivilNxTableRequest { Table = CivilNxResultTable.DisplacementGlobal, ElementIds = new[] { 3 },
            LoadCases = new[] { "Summation(CS)" }, StageSteps = new[] { "Getto:001(last)" } };
        Assert.IsFalse(JsonDocument.Parse(request.ToJson()).RootElement.GetProperty("Argument").TryGetProperty("DISP_OPT", out _));
        request.DisplacementReference = CivilNxDisplacementReference.Real;
        using var json = JsonDocument.Parse(request.ToJson()); var arg = json.RootElement.GetProperty("Argument");
        Assert.IsTrue(arg.GetProperty("OPT_CS").GetBoolean()); Assert.AreEqual("Real", arg.GetProperty("DISP_OPT").GetString());
        Assert.IsFalse(arg.TryGetProperty("EXPORT_PATH", out _));
        request.StageSteps = Array.Empty<string>(); Assert.ThrowsException<ArgumentException>(() => request.ToJson());
        Assert.ThrowsException<ArgumentException>(() => new CivilNxStageStep("A:B", "001(last)"));
        Assert.ThrowsException<ArgumentException>(() => new CivilNxStageSelection(new[] { new CivilNxStageStep("A", "B"), new CivilNxStageStep("A", "B") }, new[] { "C" }, CivilNxDisplacementReference.Current));
    }

    private sealed class Handler : HttpMessageHandler
    {
        internal readonly List<string> Bodies = new();
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        {
            Assert.AreEqual(HttpMethod.Post, request.Method); StringAssert.EndsWith(request.RequestUri!.AbsolutePath.ToUpperInvariant(), "/POST/TABLE");
            Bodies.Add(await request.Content!.ReadAsStringAsync(token));
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{}") };
        }
    }
    [TestMethod]
    public async Task StageReadUsesOnlyTheRequestedOutputStatesAndGlobalDisplacements()
    {
        var (model, _) = Import(); var handler = new Handler();
        using var client = new CivilNxApiClient(new Uri("https://example.invalid/civil"), () => "test-only-key", handler);
        var result = await CivilNxResults.ReadStagesAsync(client, model, Selection(CivilNxDisplacementReference.Current));
        Assert.AreEqual(12, result.Count); Assert.AreEqual(12, handler.Bodies.Count);
        foreach (var body in handler.Bodies)
        {
            using var json = JsonDocument.Parse(body); var arg = json.RootElement.GetProperty("Argument");
            Assert.AreEqual("Summation(CS)", arg.GetProperty("LOAD_CASE_NAMES")[0].GetString());
            Assert.AreEqual(1, arg.GetProperty("STAGE_STEP").GetArrayLength()); Assert.IsTrue(arg.GetProperty("OPT_CS").GetBoolean());
            if (arg.GetProperty("TABLE_TYPE").GetString() == "DISPLACEMENTG") Assert.AreEqual("Current", arg.GetProperty("DISP_OPT").GetString());
            else Assert.IsFalse(arg.TryGetProperty("DISP_OPT", out _));
        }
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => CivilNxResults.ReadStagesAsync(client, model, Selection(), cancellationToken: new CancellationToken(true)));
    }
}
