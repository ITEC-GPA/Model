using GPC.Model.Models;
using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Analysis;
using GPC.Model.Checking.Preparation;
using GPC.Model.Compatibility;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Results.Queries;
using GPC.Model.Results.State;

namespace UnitTest;

[TestClass]
public partial class Straus7ApiWorkflowTest
{
    private string directory = null!, modelPath = null!, resultPath = null!;
    [TestInitialize] public void Files()
    {
        directory = Path.Combine(Path.GetTempPath(), "gpc-straus7-tests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        modelPath = Path.Combine(directory, "synthetic.st7"); resultPath = Path.Combine(directory, "synthetic.lsa");
        File.WriteAllText(modelPath, "Synthetic bytes for transport-double tests, NOT a native ST7 file.");
        File.WriteAllText(resultPath, "Synthetic result bytes, NOT a native Straus7 result file.");
    }
    [TestCleanup] public void Cleanup() { File.Delete(modelPath); File.Delete(resultPath); Directory.Delete(directory); }
    private Straus7ImportRequest Request() => new() { ModelPath = modelPath, ModelRevision = "fixture-r1", AnalysisId = "fixture-analysis" };
    private Straus7ResultsRequest ResultsRequest() => new() { ModelPath = modelPath, ResultPath = resultPath, CaseNumbers = new[] { 1 },
        BeamNumbers = new[] { 1 }, PlateNumbers = new[] { 1 }, NodeNumbers = new[] { 1 }, MinimumBeamStations = 3 };
    private static void Partial(ImportReport report) => Assert.AreEqual(ImportStatus.Partial, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
    private static readonly IReadOnlyDictionary<int, string> CaseMap = new Dictionary<int, string> { [1] = "Case 1: G" };

    [TestMethod]
    public void Geometry_GroupsIdentityAxesUnitsPropertiesAndLifetime()
    {
        var api = new SyntheticApi(); var report = new Straus7ApiConverter(() => api).Import(Request()); Partial(report);
        var m = report.Model; var beam = m.BeamElements.Values.Single(); var plate = m.AreaElements.Values.Single();
        Assert.AreEqual(4, m.NodesElements.Count); Assert.AreSame(beam.NodeI, plate.Nodes[0]);
        Assert.AreEqual("1", beam.Source.OriginalId); Assert.AreEqual("1", plate.Source.OriginalId);
        Assert.AreEqual(2000, beam.Length); Assert.AreEqual(1, beam.Assignments.SectionAxes.V3.X);
        Assert.AreEqual(1, beam.Assignments.SectionAxes.V1.Y); Assert.AreEqual(1, beam.Assignments.SectionAxes.V2.Z);
        Assert.AreEqual(1000, plate.CoordinateSystem.Origin.X); Assert.AreEqual(500, plate.CoordinateSystem.Origin.Y);
        Assert.AreSame(m.Groups["Group 8: Model"], m.Groups["Group 42: Deck"].Parent);
        Assert.AreEqual(0, m.GetGroupElements("Group 8: Model").Count); Assert.AreEqual(2, m.GetGroupElements("Group 8: Model", true).Count);
        Assert.AreEqual(2, m.PreservedSourceData.Count(p => p.SourceRecord.StartsWith("Property/")));
        Assert.IsTrue(m.PreservedSourceData.Any(p => p.SourceRecord == "Node/1" && p.RawData.Contains("\"UserId\":77")));
        Assert.IsNull(beam.BeamProperty); Assert.IsNull(plate.PlateProperty); Assert.IsFalse(report.VerificationEnabled);
        CollectionAssert.AreEqual(new[] { "open-model", "close-model", "dispose" }, api.Lifecycle);
        using var archive = new MemoryStream(); ModelArchive.Save(m, archive); archive.Position = 0; var copy = ModelArchive.Load(archive);
        Assert.AreSame(copy.BeamElements.Values.Single().NodeI, copy.AreaElements.Values.Single().Nodes[0]);
        Assert.AreEqual(m.AnalysisFingerprint(), copy.AnalysisFingerprint());
    }

    [DataTestMethod]
    [DataRow("beam-type")][DataRow("plate-type")][DataRow("beam3")][DataRow("plate8")][DataRow("missing-node")]
    [DataRow("repeated-node")][DataRow("axis-left-handed")][DataRow("axis-wrong-longitudinal")][DataRow("axis-nonfinite")]
    [DataRow("missing-group")][DataRow("group-cycle")][DataRow("missing-parent")][DataRow("node-mismatch")]
    [DataRow("nan-coordinate")][DataRow("bad-unit")][DataRow("bad-count")]
    [DataRow("axis-scaled")]
    public void InvalidNativeGeometry_RejectsWholeCandidateAndReleasesSession(string fault)
    {
        var api = new SyntheticApi { Fault = fault }; var report = new Straus7ApiConverter(() => api).Import(Request());
        Assert.AreEqual(ImportStatus.Rejected, report.Status, fault); Assert.IsNull(report.Model);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)); Assert.IsTrue(api.Disposed);
        Assert.IsTrue(report.Preserved.Any(p => p.Kind == "Straus7 native source"));
    }

    [TestMethod]
    public void NativeErrorAndMissingDll_AreReports_NotInventedModels()
    {
        var api = new SyntheticApi { Fault = "api-error" }; var failed = new Straus7ApiConverter(() => api).Import(Request());
        Assert.AreEqual(ImportStatus.Rejected, failed.Status); Assert.IsNull(failed.Model); Assert.IsTrue(api.Disposed);
        var d = failed.Diagnostics.Single(); Assert.AreEqual("Straus7ApiError", d.Code); Assert.AreEqual("St7GetNodeXYZ 2", d.Record);
        Assert.IsTrue(d.Message.Contains("12345"));
        var missing = new Straus7ApiConverter(() => throw new DllNotFoundException("fixture missing SDK")).Import(Request());
        Assert.AreEqual("Straus7MissingDependency", missing.Diagnostics.Single().Code);
    }

    [TestMethod]
    public void CancelledRead_DiscardsCandidateAndClosesOwnedSession()
    {
        using var cancellation = new CancellationTokenSource(); var api = new SyntheticApi { OnNode = n => { if (n == 2) cancellation.Cancel(); } };
        var report = new Straus7ApiConverter(() => api).Import(Request(), cancellation.Token);
        Assert.AreEqual(ImportStatus.Cancelled, report.Status); Assert.IsNull(report.Model); Assert.IsTrue(api.Disposed);
        bool opened = false; var alreadyCancelled = new Straus7ApiConverter(() => { opened = true; return new SyntheticApi(); }).Import(Request(), cancellation.Token);
        Assert.AreEqual(ImportStatus.Cancelled, alreadyCancelled.Status); Assert.IsFalse(opened);
    }

    [TestMethod]
    public void Results_AreExplicitNativeTables_WithVersionHashesStatesAndExtraStations()
    {
        var api = new SyntheticApi(); var report = new Straus7ApiConverter(() => api).ReadResults(ResultsRequest());
        Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join(";", report.Diagnostics.Select(d => d.Message)));
        Assert.AreEqual(5, report.Tables.Count); Assert.AreEqual(3, report.Tables.Single(t => t.Entity == Straus7Entity.Beam).Rows);
        Assert.AreEqual("G", report.Cases.Single().Name); Assert.AreEqual("3.1.5-fixture", report.ApiVersion);
        Assert.AreNotEqual(report.ModelHash, report.ResultHash); Assert.AreEqual(0, report.File.ValidationFlags);
        CollectionAssert.AreEqual(new[] { "open-model", "validate-results", "open-results", "close-results", "close-model", "dispose" }, api.Lifecycle);
        var list = new Straus7ApiConverter(() => new SyntheticApi()).ReadResults(new Straus7ResultsRequest { ModelPath = modelPath, ResultPath = resultPath });
        Assert.AreEqual(ImportStatus.Completed, list.Status); Assert.AreEqual(1, list.Cases.Count); Assert.AreEqual(0, list.Tables.Count);
    }

    [TestMethod]
    public void PlannedRead_ReadsOnlyTheFilteredEntities_AndMapsCasesByName()
    {
        var converter = new Straus7ApiConverter(() => new SyntheticApi()); var model = converter.Import(Request()).Model;
        var filter = new ResultFilter { Elements = new ElementSelection { Families = new[] { EntityFamily.Beam } } };
        var native = converter.ReadResults(model, filter.Resolve(model), modelPath, resultPath);
        Assert.AreEqual(ImportStatus.Completed, native.Status, string.Join(";", native.Diagnostics.Select(d => d.Message)));
        Assert.AreEqual("BeamForceGlobal", native.Tables.Single().Quantity);
        var result = Straus7LinearStaticResults.Import(model, native, "planned", isSynthetic: true);
        Assert.AreEqual(ImportStatus.Completed, result.Status, string.Join(";", result.Diagnostics.Select(d => d.Message))); Assert.AreEqual(3, result.ImportedSamples);
        var nodes = converter.ReadResults(model, new ResultFilter { Elements = new ElementSelection { Families = new[] { EntityFamily.Node } } }.Resolve(model), modelPath, resultPath);
        Assert.AreEqual(8, nodes.Tables.Count(t => t.Entity == Straus7Entity.Node)); Assert.AreEqual(8, nodes.Tables.Count);
        var missing = converter.ReadResults(model, filter.Resolve(model), modelPath, Path.Combine(directory, "absent.lsa"));
        Assert.AreEqual(ImportStatus.Rejected, missing.Status); Assert.AreEqual(0, missing.Tables.Count);
    }

    [TestMethod]
    public void CaseMap_UsesTheLoadCaseNumberOfTheNativeName_ThenTheNameAlone()
    {
        var model = new GPC.Model.Models.Model();
        foreach (var name in new[] { Straus7ApiConverter.CaseName(1, "G"), Straus7ApiConverter.CaseName(2, "G"), Straus7ApiConverter.CaseName(3, "Q") })
            model.LoadCases.Add(new GPC.Model.LoadCases.LoadCaseBase(name));
        // R31 names: "<load case>: <name>", result cases numbered over the solved load cases only.
        var map = Straus7LinearStaticResults.CaseMap(model, new[] { new Straus7ResultCase { Number = 1, Name = "2: G" }, new Straus7ResultCase { Number = 2, Name = "3: Q" } });
        Assert.AreEqual("Case 2: G", map[1]); Assert.AreEqual("Case 3: Q", map[2], "Load case 1 left out of the solution.");
        map = Straus7LinearStaticResults.CaseMap(model, new[] { new Straus7ResultCase { Number = 1, Name = "Q" }, new Straus7ResultCase { Number = 2, Name = "G" },
            new Straus7ResultCase { Number = 5, Name = "G" }, new Straus7ResultCase { Number = 6, Name = "W" }, new Straus7ResultCase { Number = 7, Name = "4: G" } });
        Assert.AreEqual("Case 3: Q", map[1]); Assert.AreEqual("Case 2: G", map[2], "Equal names told apart by the number.");
        Assert.IsFalse(map.ContainsKey(5), "Ambiguous name."); Assert.IsFalse(map.ContainsKey(6)); Assert.IsFalse(map.ContainsKey(7), "No load case 4.");
    }

    [DataTestMethod]
    [DataRow("result-mismatch")][DataRow("result-shape")][DataRow("result-nan")][DataRow("result-inactive")]
    [DataRow("result-missing")][DataRow("result-case")][DataRow("result-position")][DataRow("result-order")]
    [DataRow("late-result-error")][DataRow("cleanup-error")]
    public void InvalidResults_AreAtomicAndAlwaysReleaseRuntime(string fault)
    {
        var api = new SyntheticApi { Fault = fault }; var report = new Straus7ApiConverter(() => api).ReadResults(ResultsRequest());
        Assert.AreEqual(ImportStatus.Rejected, report.Status, fault); Assert.AreEqual(0, report.Tables.Count); Assert.AreEqual(0, report.Cases.Count);
        Assert.IsTrue(api.Disposed);
        if (fault == "result-mismatch") Assert.IsFalse(api.Lifecycle.Contains("open-results"));
    }

    /// <summary>A beam force table of the synthetic fixture with extra rows: each (position, values) row of six native values.</summary>
    private static void SetBeamRows(Straus7ResultsReport native, params (double Position, double[] Values)[] rows)
    {
        var table = native.Tables.Single(t => t.Entity == Straus7Entity.Beam);
        table.Rows = rows.Length; table.Positions = rows.Select(r => r.Position).ToArray(); table.Values = rows.SelectMany(r => r.Values).ToArray();
    }
    private static double[] Row(double force, double moment) => new[] { force, moment, force, moment, force, moment };

    [TestMethod]
    public void ResultDependentStationWithinRoundoffOfARegularOrLoadStationIsMergedIntoIt()
    {
        var converter = new Straus7ApiConverter(() => new SyntheticApi()); var imported = converter.Import(Request()); Partial(imported);
        var model = imported.Model; var beam = model.BeamElements.Values.Single(); double length = beam.Length;
        Assert.AreEqual(3, converter.ReadResults(ResultsRequest()).Tables.Single(t => t.Entity == Straus7Entity.Beam).MinimumStations, "requested minimum stations recorded");
        // Snap tolerance max(1e-3 mm, L 1e-6); an extremum at half of it from the end, the load pair at 0.25 -/+ 1e-6 with an extremum 1e-5 mm after its
        // left side (the case of example 50), all with values equal to their fixed station (the moment within V times the distance; native kN, m).
        double tolerance = Math.Max(1e-3, length * 1e-6) / length, load = .25, extremum = load - 1e-6 + 1e-5 / length;
        var native = converter.ReadResults(ResultsRequest());
        SetBeamRows(native, (0, Row(-10, -40)), (load - 1e-6, Row(-10, -40)), (extremum, Row(-10, -40 - 10 * 1e-8)), (load + 1e-6, Row(20, -40)),
            (.5, Row(20, -40)), (1 - tolerance / 2, Row(20, -40)), (1, Row(20, -40)));
        var result = Straus7LinearStaticResults.Import(model, native, "snapped", CaseMap, isSynthetic: true);
        Assert.AreEqual(ImportStatus.Completed, result.Status, string.Join(";", result.Diagnostics.Select(d => d.Message)));
        var stations = Verification.BeamSamples(beam, "snapped", "Case 1: G").Select(x => x.ParametricDistance).ToArray();
        CollectionAssert.AreEqual(new[] { 0, load - 1e-6, load + 1e-6, .5, 1 }, stations, "the regular and load stations keep their location");
        var diagnostic = result.Diagnostics.Single(d => d.Code == Straus7LinearStaticResults.StationSnapCode);
        Assert.AreEqual(GPC.Model.Core.Diagnostics.DiagnosticSeverity.Information, diagnostic.Severity); StringAssert.StartsWith(diagnostic.Message, "2 ");
        // Beyond the tolerance a result-dependent station is a station of its own.
        native = converter.ReadResults(ResultsRequest());
        SetBeamRows(native, (0, Row(-10, -40)), (.5, Row(-10, -40)), (1 - 2 * tolerance, Row(-10, -40)), (1, Row(-10, -40)));
        imported = converter.Import(Request()); model = imported.Model; beam = model.BeamElements.Values.Single();
        result = Straus7LinearStaticResults.Import(model, native, "kept", CaseMap, isSynthetic: true);
        Assert.AreEqual(ImportStatus.Completed, result.Status); Assert.AreEqual(4, Verification.BeamSamples(beam, "kept", "Case 1: G").Count);
        Assert.IsFalse(result.Diagnostics.Any(d => d.Code == Straus7LinearStaticResults.StationSnapCode));
    }

    [TestMethod]
    public void UnderADistributedLoadAnExtremumNextToARegularStationIsMerged()
    {
        // Example 99 with E13: a regular station 0.5 and an extremum 2.75e-9 L after it, the shear varying along the beam (distributed load):
        // the forces differ by the slope times the distance, within the snap check. Two result-dependent stations within the tolerance: one kept.
        var converter = new Straus7ApiConverter(() => new SyntheticApi()); var imported = converter.Import(Request()); Partial(imported);
        var model = imported.Model; var beam = model.BeamElements.Values.Single(); double length = beam.Length / 1000; // native m
        double V(double x) => 10 - 20 * x; // kN, x parametric; slope 20 kN over the length
        double M(double x) => 10 * x * length - 10 * x * x * length;
        double e = 2.75e-9, tolerance = Math.Max(1e-3, beam.Length * 1e-6) / beam.Length;
        var native = converter.ReadResults(ResultsRequest());
        SetBeamRows(native, (0, Row(V(0), M(0))), (.5, Row(V(.5), M(.5))), (.5 + e, Row(V(.5 + e), M(.5 + e))), (.75, Row(V(.75), M(.75))),
            (.75 + tolerance / 4, Row(V(.75 + tolerance / 4), M(.75 + tolerance / 4))), (.75 + tolerance / 2, Row(V(.75 + tolerance / 2), M(.75 + tolerance / 2))), (1, Row(V(1), M(1))));
        var result = Straus7LinearStaticResults.Import(model, native, "slope", CaseMap, isSynthetic: true);
        Assert.AreEqual(ImportStatus.Completed, result.Status, string.Join(";", result.Diagnostics.Select(d => d.Message)));
        var stations = Verification.BeamSamples(beam, "slope", "Case 1: G").Select(x => x.ParametricDistance).ToArray();
        // .75 is not regular for 3 minimum stations: the two extrema after it are result-dependent; the first of the pair stays.
        CollectionAssert.AreEqual(new[] { 0, .5, .75, .75 + tolerance / 2, 1 }, stations);
        StringAssert.StartsWith(result.Diagnostics.Single(d => d.Code == Straus7LinearStaticResults.StationSnapCode).Message, "2 ");
    }

    [TestMethod]
    public void StationsWithinRoundoffWithDifferentValuesAreRejected()
    {
        var converter = new Straus7ApiConverter(() => new SyntheticApi()); var imported = converter.Import(Request()); Partial(imported);
        var model = imported.Model; double length = model.BeamElements.Values.Single().Length, tolerance = Math.Max(1e-3, length * 1e-6) / length;
        var native = converter.ReadResults(ResultsRequest());
        // A force jump within the snap tolerance of the end: not roundoff, a load at the end that needs an explicit side.
        SetBeamRows(native, (0, Row(-10, -40)), (.5, Row(-10, -40)), (1 - tolerance / 2, Row(-10, -40)), (1, Row(90, -40)));
        var result = Straus7LinearStaticResults.Import(model, native, "rejected-snap", CaseMap, isSynthetic: true);
        Assert.AreEqual(ImportStatus.Rejected, result.Status);
        StringAssert.Contains(result.Diagnostics.Single().Message, "Straus7ResultStationSnapValuesDisagree");
    }

    [TestMethod]
    public void StaticNormalization_ConvertsCutBodyVectorsUnitsAndPlateTensor_ThenPersists()
    {
        var converter = new Straus7ApiConverter(() => new SyntheticApi()); var imported = converter.Import(Request()); Partial(imported);
        var native = converter.ReadResults(ResultsRequest()); var model = imported.Model;
        var result = Straus7LinearStaticResults.Import(model, native, "fixture-results", CaseMap, isSynthetic: true);
        Assert.AreEqual(ImportStatus.Completed, result.Status, string.Join(";", result.Diagnostics.Select(d => d.Message))); Assert.AreEqual(6, result.ImportedSamples);
        var beam = model.BeamElements.Values.Single(); var samples = Verification.BeamSamples(beam, "fixture-results", "Case 1: G");
        Assert.AreEqual(3, samples.Count); var s = samples[1];
        Assert.AreEqual(10000, s.ResultBeamForces.N); Assert.AreEqual(30000, s.ResultBeamForces.V1); Assert.AreEqual(50000, s.ResultBeamForces.V2);
        Assert.AreEqual(20000000, s.ResultBeamForces.T); Assert.AreEqual(40000000, s.ResultBeamForces.M1); Assert.AreEqual(60000000, s.ResultBeamForces.M2);
        Assert.AreEqual(1000, s.PhysicalDistance); Assert.AreEqual(1000, s.ResultBeamForces.CoordinateSystem.Origin.X);
        Assert.AreEqual(ActionBody.PositiveSectionFace, s.Body); Assert.IsTrue(s.State.IsSynthetic);
        var plate = model.AreaElements.Values.Single().Results.SelectMany(r => r.Results).OfType<PointResultPlateForces>().Single();
        Assert.AreEqual(11, plate.Forces.Fxx); Assert.AreEqual(22, plate.Forces.Fyy); Assert.AreEqual(33, plate.Forces.Fxy);
        Assert.AreEqual(55, plate.Forces.Fxz); Assert.AreEqual(44, plate.Forces.Fyz);
        Assert.AreEqual(6000, plate.Forces.Mxx); Assert.AreEqual(7000, plate.Forces.Myy); Assert.AreEqual(8000, plate.Forces.Mxy);
        Assert.AreEqual(ResultCoordinateKind.LocalPhysical, plate.CoordinateKind); Assert.AreEqual(0, plate.Location.X);
        Assert.AreEqual(1000, plate.Forces.CoordinateSystem.Origin.X);
        Assert.IsTrue(model.PreservedSourceData.Any(p => p.SourceRecord == "ResultFile/" + native.ResultHash && p.RawData.Contains("NodeReactionGlobal")));
        Assert.AreNotEqual(DataStatus.Ready, Verification.PrepareBeam(model, beam.Id, s, "fixture").Status);
        using var archive = new MemoryStream(); ModelArchive.Save(model, archive); archive.Position = 0; var copy = ModelArchive.Load(archive);
        Assert.AreEqual(6, copy.AllElements.Sum(e => e.Results.Sum(r => r.Results.Count)));
        var node = copy.NodesElements.Values.Single(n => n.Source.OriginalId == "1");
        var select = new ResultSelection { Dataset = "fixture-results", Case = "Case 1: G" };
        var reaction = ResultQueries.NodeForce(node, select, NodalForceKind.SupportReaction, ActionBody.OnNode);
        Assert.AreEqual(1000, reaction.Fx); Assert.AreEqual(6000000, reaction.Mz);
        var displacement = ResultQueries.Samples<NodeResultDisplacement>(node, select).Single().ResultDisplacement;
        Assert.AreEqual(1000, displacement.D1); Assert.AreEqual(6, displacement.R3);
        Assert.AreEqual(model.AnalysisFingerprint(), copy.AnalysisFingerprint());
    }

    [DataTestMethod]
    [DataRow("changed-node")][DataRow("missing-analysis")][DataRow("wrong-source")][DataRow("solver-nonlinear")]
    [DataRow("stage")][DataRow("duplicate-station")][DataRow("case-unmapped")][DataRow("plate-pair-missing")]
    [DataRow("plate-extra-component")][DataRow("wrong-column-count")]
    public void StaticNormalization_RejectsUnsupportedOrUnboundDataWithoutMutatingModel(string fault)
    {
        var converter = new Straus7ApiConverter(() => new SyntheticApi { Fault = fault });
        var geometry = converter.Import(Request()); Partial(geometry); var m = geometry.Model;
        var native = converter.ReadResults(ResultsRequest()); Assert.AreEqual(ImportStatus.Completed, native.Status);
        var map = CaseMap;
        if (fault == "changed-node") m.NodesElements.Values.First().Position.X = 7;
        if (fault == "missing-analysis") m.AnalysisSource.AnalysisId = null;
        if (fault == "wrong-source") m.AnalysisSource.GeometryHash = "wrong";
        if (fault == "case-unmapped") map = new Dictionary<int, string> { [1] = "absent" };
        if (fault == "plate-pair-missing") native.Tables.RemoveAll(t => t.Quantity == "PlateMomentLocalCentroid");
        var before = m.AnalysisFingerprint(); int evidence = m.PreservedSourceData.Count;
        var result = Straus7LinearStaticResults.Import(m, native, "rejected", map, true);
        Assert.AreEqual(ImportStatus.Rejected, result.Status, fault); Assert.AreEqual(0, m.Datasets.Count);
        Assert.AreEqual(0, m.AllElements.Sum(e => e.Results.Count)); Assert.AreEqual(before, m.AnalysisFingerprint()); Assert.AreEqual(evidence, m.PreservedSourceData.Count);
    }

    [TestMethod]
    public void Normalization_CancellationAndDuplicateDataset_DoNotAddResults()
    {
        var converter = new Straus7ApiConverter(() => new SyntheticApi()); var m = converter.Import(Request()).Model; var native = converter.ReadResults(ResultsRequest());
        using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        Assert.AreEqual(ImportStatus.Cancelled, Straus7LinearStaticResults.Import(m, native, "dataset", CaseMap, true, cancellation.Token).Status);
        Assert.AreEqual(0, m.Datasets.Count);
        Assert.AreEqual(ImportStatus.Completed, Straus7LinearStaticResults.Import(m, native, "dataset", CaseMap, true).Status);
        Assert.AreEqual(ImportStatus.Rejected, Straus7LinearStaticResults.Import(m, native, "dataset", CaseMap, true).Status);
        Assert.AreEqual(6, m.AllElements.Sum(e => e.Results.Sum(r => r.Results.Count)));
    }

    [DataTestMethod]
    [DataRow("valid")][DataRow("node-forces-unavailable")][DataRow("node-forces-order")][DataRow("node-forces-missing-ids")]
    [DataRow("unknown-node-quantity")][DataRow("wrong-node-rows")]
    public void ElementNodeForces_AreSeparateOwnedActions_AndBadInputsAreAtomic(string fault)
    {
        var converter = new Straus7ApiConverter(() => new SyntheticApi { Fault = fault });
        var model = converter.Import(Request()).Model; Assert.IsNotNull(model);
        var request = ResultsRequest(); request.IncludeElementNodeForces = true;
        var native = converter.ReadResults(request);
        if (fault == "node-forces-unavailable" || fault == "node-forces-missing-ids")
        { Assert.AreEqual(ImportStatus.Rejected, native.Status); Assert.AreEqual(0, native.Tables.Count); return; }
        Assert.AreEqual(ImportStatus.Completed, native.Status);
        var nodal = native.Tables.First(t => t.Entity == Straus7Entity.Node);
        if (fault == "unknown-node-quantity") nodal.Quantity = "Unknown";
        if (fault == "wrong-node-rows") { nodal.Rows = 2; nodal.Values = nodal.Values.Concat(nodal.Values).ToArray(); }
        var result = Straus7LinearStaticResults.Import(model, native, "with-element-nodes", CaseMap, true);
        if (fault != "valid")
        { Assert.AreEqual(ImportStatus.Rejected, result.Status); Assert.AreEqual(0, model.Datasets.Count); Assert.AreEqual(0, model.AllElements.Sum(e => e.Results.Count)); return; }
        Assert.AreEqual(ImportStatus.Completed, result.Status, string.Join(";", result.Diagnostics.Select(d => d.Message)));
        Assert.AreEqual(12, result.ImportedSamples); // Six pre-existing quantities + two beam and four plate node actions.
        var select = new ResultSelection { Dataset = "with-element-nodes", Case = "Case 1: G" };
        var node = model.NodesElements.Values.Single(n => n.Source.OriginalId == "1");
        var forces = ResultQueries.Samples<NodeResultForces>(node, select);
        Assert.AreEqual(3, forces.Count); Assert.AreEqual(2, forces.Count(f => f.Body == ActionBody.OnElement));
        Assert.AreEqual(1, forces.Count(f => f.Kind == NodalForceKind.SupportReaction));
        using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0; var copy = ModelArchive.Load(stream);
        Assert.AreEqual(8, copy.NodesElements.Values.Sum(n => ResultQueries.Samples<ResultLocation>(n, select).Count));
    }

    // Transport double exists only in tests. Native integration is exercised separately by the executable example.
    private sealed class SyntheticApi : IStraus7ReadApi, IStraus7ElementNodeReadApi
    {
        public string Fault = "";
        public Action<int>? OnNode;
        public List<string> Lifecycle = new();
        public bool Disposed;
        private bool modelOpen, resultsOpen;
        public string Version => "3.1.5-fixture";
        public void OpenModelReadOnly(string path, string scratchPath) { modelOpen = true; Lifecycle.Add("open-model"); }
        public void CloseModel() { if (modelOpen) { modelOpen = false; Lifecycle.Add("close-model"); } }
        public void CloseResults() { if (resultsOpen) { resultsOpen = false; Lifecycle.Add("close-results"); } }
        public void Dispose() { CloseResults(); CloseModel(); Lifecycle.Add("dispose"); Disposed = true; if (Fault == "cleanup-error") throw new InvalidOperationException("fixture cleanup"); }
        public int[] ReadUnits() => new[] { Fault == "bad-unit" ? 99 : 0, 1, 2, 0, 0, 0 };
        public int Count(Straus7Entity entity) => entity switch { Straus7Entity.Node => Fault == "bad-count" ? -1 : 4,
            Straus7Entity.Beam => 1, Straus7Entity.Plate => 1, Straus7Entity.Brick => Fault == "bricks" ? 1 : 0, Straus7Entity.Link => Fault == "links" ? 1 : 0, _ => 0 };
        public Straus7Node ReadNode(int number)
        {
            OnNode?.Invoke(number); if (number == 2 && Fault == "api-error") throw new Straus7ApiException("St7GetNodeXYZ 2", 12345, "fixture native failure");
            var xyz = number switch { 1 => new[] { 0d, 0d, 0d }, 2 => new[] { 2d, 0d, 0d }, 3 => new[] { 2d, 1d, 0d }, _ => new[] { 0d, 1d, 0d } };
            if (Fault == "nan-coordinate") xyz[0] = double.NaN;
            return new Straus7Node { Number = Fault == "node-mismatch" ? number + 1 : number, UserId = 77, Coordinates = xyz };
        }
        public Straus7Element ReadElement(Straus7Entity entity, int number)
        {
            bool beam = entity == Straus7Entity.Beam;
            var e = new Straus7Element { Number = number, UserId = 77, Nodes = beam ? new[] { 1, 2 } : new[] { 1, 2, 3, 4 }, Property = beam ? 7 : 19, GroupId = 42,
                Formulation = beam ? 6 : 4, InitialAxes = beam ? new[] { 0d, 1, 0, 0, 0, 1, 1, 0, 0 } : new[] { 1d, 0, 0, 0, 1, 0, 0, 0, 1 }, Centroid = beam ? new[] { 1d, 0, 0 } : new[] { 1d, .5, 0 } };
            if (Fault == "beam-type" && beam) e.Formulation = 3;
            if (Fault == "plate-type" && !beam) e.Formulation = 1;
            if (Fault == "beam3" && beam) e.Nodes = new[] { 1, 2, 3 };
            if (Fault == "plate8" && !beam) e.Nodes = new[] { 1, 2, 3, 4, 5, 6, 7, 8 };
            if (Fault == "missing-node") e.Nodes[0] = 99;
            if (Fault == "repeated-node") e.Nodes[0] = e.Nodes[1];
            if (Fault == "axis-left-handed") e.InitialAxes[7] = 2;
            if (Fault == "axis-wrong-longitudinal" && beam) e.InitialAxes = new[] { 1d, 0, 0, 0, 1, 0, 0, 0, 1 };
            if (Fault == "axis-nonfinite") e.InitialAxes[0] = double.NaN;
            if (Fault == "axis-scaled") e.InitialAxes = e.InitialAxes.Select(v => v * 2).ToArray();
            if (Fault == "missing-group") e.GroupId = 101;
            return e;
        }
        public Straus7Property ReadProperty(Straus7Entity entity, int number) => new() { Number = number, Name = "Synthetic property", Formulation = entity == Straus7Entity.Beam ? 6 : 4,
            Geometry = new[] { .3, .5 }, Material = new[] { 30000000d, .2 }, MaterialType = 1 };
        public int GroupCount => 2;
        public Straus7Group ReadGroup(int index) => index == 1 ? new() { Id = 8, ParentId = Fault == "group-cycle" ? 42 : -1, Name = "Model" }
            : new() { Id = 42, ParentId = Fault == "missing-parent" ? 99 : 8, Name = "Deck" };
        public int LoadCaseCount => 1;
        public string ReadLoadCase(int number) => "G";
        public int StageCount => Fault == "stage" ? 1 : 0;
        public Straus7ResultFile ValidateResults(string path) { Lifecycle.Add("validate-results"); return new() { ValidationFlags = Fault == "result-mismatch" ? 1 << 10 : 0, Solver = Fault == "solver-nonlinear" ? 3 : 1 }; }
        public Straus7ResultFile OpenResults(string path) { Lifecycle.Add("open-results"); resultsOpen = true; return new() { PrimaryCases = 1 }; }
        // Fault "stage": a case of stage 2 in a model with one stage. A model with stages solved without an active stage (stage 0) is an unstaged
        // linear static solution since Converter 2.8 (MOD-33); before, any stage of the model rejected the results.
        public Straus7ResultCase ReadResultCase(int number) => new() { Number = number, Name = "G", Stage = Fault == "stage" ? 2 : 0 };
        private Straus7ResultTable Faulted(Straus7ResultTable t)
        {
            if (Fault == "result-shape") t.Columns = 99;
            if (Fault == "wrong-column-count") { t.Columns = 3; t.Rows *= 2; if (t.Positions != null) t.Positions = Enumerable.Range(0, t.Rows).Select(i => (double)i / (t.Rows - 1)).ToArray(); }
            if (Fault == "result-nan") t.Values[0] = double.NaN;
            if (Fault == "result-inactive" && t.ElementState != null) t.ElementState[0] = 0;
            if (Fault == "result-missing" && t.ElementState != null) t.ElementState[1] = 0;
            if (Fault == "result-case") t.CaseNumber = 2;
            if (Fault == "result-position" && t.Positions != null) t.Positions[1] = 2;
            if (Fault == "result-order" && t.Positions != null) t.Positions = new[] { 1d, .5, 0 };
            if (Fault == "duplicate-station" && t.Positions != null) t.Positions[1] = 0;
            return t;
        }
        public Straus7ResultTable ReadBeamForces(int number, int resultCase, int minimumStations) => Faulted(new() { Entity = Straus7Entity.Beam, Number = number, CaseNumber = resultCase,
            Quantity = "BeamForceGlobal", Rows = 3, Columns = 6, Positions = new[] { 0d, .5, 1 }, Values = Enumerable.Range(0, 3).SelectMany(_ => new[] { -10d, -20, -30, -40, -50, -60 }).ToArray(), ElementState = new[] { 1, 1, 1 } });
        public Straus7ResultTable ReadPlateResult(int number, int resultCase, bool moments)
        {
            if (moments && Fault == "late-result-error") throw new Straus7ApiException("St7GetPlateResultArray", 4321, "fixture late failure");
            var t = new Straus7ResultTable { Entity = Straus7Entity.Plate, Number = number, CaseNumber = resultCase, Quantity = moments ? "PlateMomentLocalCentroid" : "PlateForceLocalCentroid",
                Rows = 1, Columns = 6, Values = moments ? new[] { 6d, 7, 0, 8, 0, 0 } : new[] { 11d, 22, 0, 33, 44, 55 }, ElementState = new[] { 1, 1, 1 } };
            if (Fault == "plate-extra-component") t.Values[2] = 7;
            return Faulted(t);
        }
        public Straus7ResultTable ReadNodeResult(int number, int resultCase, bool reactions) => Faulted(new() { Entity = Straus7Entity.Node, Number = number, CaseNumber = resultCase,
            Quantity = reactions ? "NodeReactionGlobal" : "NodeDisplacementGlobal", Rows = 1, Columns = 6, Values = new[] { 1d, 2, 3, 4, 5, 6 } });
        public Straus7ResultTable ReadElementNodeForces(Straus7Entity entity, int number, int resultCase)
        {
            if (Fault == "node-forces-unavailable") throw new Straus7ApiException("ElementNodeForce", 118, "Result quantity not stored");
            var nodes = entity == Straus7Entity.Beam ? new[] { 1, 2 } : new[] { 1, 2, 3, 4 };
            if (Fault == "node-forces-order") Array.Reverse(nodes);
            var result = new Straus7ResultTable { Entity = entity, Number = number, CaseNumber = resultCase, Rows = nodes.Length, Columns = 6,
                Quantity = "ElementNodeForceGlobal", NodeNumbers = nodes, Positions = entity == Straus7Entity.Beam ? new[] { 0d, 1d } : null,
                Values = Enumerable.Range(0, nodes.Length).SelectMany(_ => new[] { 1d, 2, 3, 4, 5, 6 }).ToArray(), ElementState = new[] { 1, 1, 1 } };
            if (Fault == "node-forces-missing-ids") result.NodeNumbers = null!;
            return result;
        }
    }
}
