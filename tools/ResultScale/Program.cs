// Scale measurement of GPC Model with results (architecture review R1, 10/10/2026). Development tool, outside GPCModel.sln.
//   dotnet run --project tools/ResultScale -c Release -- --midas <dir> --cases 2,10,122 [--out <file.json>]
//   dotnet run --project tools/ResultScale -c Release -- --grid 30 --cases 5,9 [--out <file.json>]
// --midas: Civil NX model tables (UNIT, NODE, ELEM, ... as read from db/) and post/TABLE result responses of one case
//          (plate-results-*.json, beam-results-*.json). The result rows are replayed for the first N static cases and combinations
//          of the model, each with its own scale factor, through CivilNxResults.Import: the production import path, so every
//          sample has the object structure of a real import. Ten cases per dataset, as separate acquisitions would give.
// --grid:  synthetic n x n grid of quadrilateral plates, 4 samples per plate and case; no private data needed.
// --load:  an archive saved with --archive, read in a fresh process (no snapshot validated yet), then copied and prepared.
// Measures import, ModelArchive.Save/Load, ModelValues.Copy, VerificationPreparation.Prepare, PreparedVerification.IsCurrent,
// VerificationSnapshot.Capture, the decoding of all the results of a copy, archive size and peak working set. A failing operation
// is recorded with its error. After each dataset the results are compacted (Model.CompactResults), as a large import should do.
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Checking.Preparation;
using GPC.Model.Checking.Scenarios;
using GPC.Model.Core;
using GPC.Model.Core.Identity;
using GPC.Model.Data.Concrete;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;
using GPC.Model.Results.Storage;
using GPC.Model.Sections.Concrete;

string midas = null, output = null, keep = null, load = null; int grid = 30; int[] caseRuns = { 2, 10 };
for (int i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "--midas": midas = args[++i]; break;
        case "--grid": grid = int.Parse(args[++i], CultureInfo.InvariantCulture); break;
        case "--cases": caseRuns = args[++i].Split(',').Select(c => int.Parse(c, CultureInfo.InvariantCulture)).ToArray(); break;
        case "--out": output = args[++i]; break;
        case "--archive": keep = args[++i]; break; // folder: the saved archive of each run is written there as <cases>.gpcmodel
        case "--load": load = args[++i]; break;
        default: throw new ArgumentException("Opzioni: --midas <dir> | --grid <n>, --cases <n,...>, --out <file.json>, --archive <dir>");
    }
}
var runs = new List<Dictionary<string, object>>();
if (load != null)
{
    var row = new Dictionary<string, object> { ["source"] = load, ["archiveBytes"] = new FileInfo(load).Length };
    GPC.Model.Models.Model loaded = null;
    Console.WriteLine($"\n== {load}: {new FileInfo(load).Length / 1048576.0:N1} MB, lettura a freddo");
    Measure(row, "load", () => { using var s = File.OpenRead(load); loaded = ModelArchive.Load(s); });
    if (loaded != null)
    {
        // Architecture review R6: the viewer opens the model through ModelView and reads the results one dataset at a time.
        GPC.Model.Presentation.ModelView view = null;
        Measure(row, "viewOpen", () => view = GPC.Model.Presentation.ModelView.Open(loaded));
        if (view != null)
        {
            long fields = 0, values = 0, maxBytes = 0; var reading = new Stopwatch();
            foreach (var dataset in view.Datasets)
            {
                long before = GC.GetTotalMemory(true);
                reading.Start(); var results = view.Results(dataset.Id); reading.Stop();
                foreach (var key in results.Fields) values += results.Field(key.Case, key.Family, key.Component).Samples.Count;
                fields += results.Fields.Count;
                maxBytes = Math.Max(maxBytes, GC.GetTotalMemory(true) - before);
                view.Release(dataset.Id);
            }
            row["viewDatasets"] = view.Datasets.Count; row["viewFields"] = fields; row["viewValues"] = values;
            row["viewResultsSeconds"] = Math.Round(reading.Elapsed.TotalSeconds, 3); row["viewLargestDatasetMB"] = Math.Round(maxBytes / 1048576.0, 1);
            Console.WriteLine($"   vista: {view.Nodes.Count} nodi, {view.Elements.Count} elementi, {view.Datasets.Count} dataset, {fields:N0} campi, {values:N0} valori; "
                + $"lettura dei dataset {reading.Elapsed.TotalSeconds:N2} s, dataset piu' grande {maxBytes / 1048576.0:N1} MB");
        }
        Measure(row, "copy", () => ModelValues.Copy(loaded));
        PreparedVerification prepared = null;
        Measure(row, "prepare", () => prepared = VerificationPreparation.Prepare(loaded, VerificationScenario.Create(loaded.Analysis.Id, "misura")));
        if (prepared != null) { Measure(row, "isCurrent", () => { if (!prepared.IsCurrent) throw new InvalidOperationException("NotCurrent"); }); Measure(row, "snapshot", () => VerificationSnapshot.Capture(prepared)); }
    }
    row["peakWorkingSetMB"] = Math.Round(Process.GetCurrentProcess().PeakWorkingSet64 / 1048576.0);
    Console.WriteLine($"   picco memoria {row["peakWorkingSetMB"]:N0} MB");
    runs.Add(row); caseRuns = new int[0];
}
foreach (int cases in caseRuns)
{
    GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
    var row = new Dictionary<string, object> { ["source"] = midas != null ? "midas" : "grid " + grid, ["cases"] = cases };
    var clock = Stopwatch.StartNew();
    var (model, samples) = midas != null ? MidasModel(midas, cases, row) : GridModel(grid, cases);
    row["importSeconds"] = clock.Elapsed.TotalSeconds;
    long compact = model.CompactResults();
    row["samples"] = samples; row["plates"] = model.AreaElements.Count; row["beams"] = model.BeamElements.Count; row["compactBytes"] = compact;
    Console.WriteLine($"\n== {cases} casi: {model.AreaElements.Count} plate, {model.BeamElements.Count} beam, {samples:N0} campioni, import {clock.Elapsed.TotalSeconds:N1} s");
    Console.WriteLine($"   risultati compatti {compact / 1048576.0:N1} MB, {compact / (double)Math.Max(1, samples):N0} byte per campione");
    byte[] archive = null;
    Measure(row, "save", () => { using var s = new MemoryStream(); ModelArchive.Save(model, s); archive = s.ToArray(); });
    if (archive != null)
    {
        row["archiveBytes"] = archive.LongLength;
        Console.WriteLine($"   archivio {archive.LongLength / 1048576.0:N1} MB, {archive.LongLength / (double)Math.Max(1, samples):N0} byte per campione");
        if (keep != null) { Directory.CreateDirectory(keep); File.WriteAllBytes(Path.Combine(keep, cases.ToString(CultureInfo.InvariantCulture) + ".gpcmodel"), archive); }
        Measure(row, "load", () => { using var s = new MemoryStream(archive, false); ModelArchive.Load(s); });
        archive = null;
    }
    Measure(row, "copy", () => ModelValues.Copy(model));
    // Decoding every result of a copy: the cost of a verification that reads all the samples (kept to sizes that fit in memory).
    if (samples <= 300000) Measure(row, "readAll", () => { var copy = ModelValues.Copy(model); if (copy.AllElements.Sum(e => e.Results.Sum(r => (long)r.Results.Count)) != samples) throw new InvalidOperationException("SampleCount"); });
    PreparedVerification prepared = null;
    Measure(row, "prepare", () => prepared = VerificationPreparation.Prepare(model, VerificationScenario.Create(model.Analysis.Id, "misura")));
    if (prepared != null)
    {
        row["compatibility"] = prepared.Compatibility.Status.ToString();
        Measure(row, "isCurrent", () => { if (!prepared.IsCurrent) throw new InvalidOperationException("NotCurrent"); });
        Measure(row, "snapshot", () => VerificationSnapshot.Capture(prepared));
    }
    row["peakWorkingSetMB"] = Math.Round(Process.GetCurrentProcess().PeakWorkingSet64 / 1048576.0);
    Console.WriteLine($"   picco memoria {row["peakWorkingSetMB"]:N0} MB");
    runs.Add(row);
}
if (output != null) File.WriteAllText(output, JsonSerializer.Serialize(runs, new JsonSerializerOptions { WriteIndented = true }));

static void Measure(Dictionary<string, object> row, string name, Action action)
{
    var clock = Stopwatch.StartNew();
    try { action(); row[name + "Seconds"] = Math.Round(clock.Elapsed.TotalSeconds, 3); Console.WriteLine($"   {name,-10} {clock.Elapsed.TotalSeconds,8:N2} s"); }
    catch (Exception ex)
    {
        var message = (ex.InnerException ?? ex).Message.Replace("\n", " ");
        if (message.Length > 200) message = message.Substring(0, 200) + "...";
        row[name + "Error"] = ex.GetType().Name + ": " + message;
        Console.WriteLine($"   {name,-10} {clock.Elapsed.TotalSeconds,8:N2} s  ERRORE {ex.GetType().Name}: {message}");
    }
}

static (GPC.Model.Models.Model, long) MidasModel(string directory, int cases, Dictionary<string, object> row)
{
    var tables = CivilNxModelProfile.RequiredTables.Concat(CivilNxModelProfile.OptionalTables).Distinct()
        .Where(t => File.Exists(Path.Combine(directory, t + ".json")))
        .ToDictionary(t => t, t => new CivilNxResponse("db/" + t, File.ReadAllText(Path.Combine(directory, t + ".json"))));
    var snapshot = new CivilNxSnapshot(tables);
    var identity = new AnalysisSource { Program = CivilNxModelProfile.Program, SolverVersion = "misura R1", ModelRevision = snapshot.Hash.Substring(0, 16), AnalysisId = "misura" };
    // The production geometry import: it also records the source binding that CivilNxResults.Import requires.
    var mapped = CivilNxGeometryReader.Import(snapshot, identity, new CivilNxModelProfile());
    var model = mapped.Model ?? throw new InvalidDataException("Mapping rifiutato: " + string.Join("; ", mapped.Diagnostics.Select(d => d.Code)));
    model.CaptureAnalysis(ReinforcementAnalysisRole.ExcludedFromAnalysis, "Misura di scala R1: armature escluse dall'analisi");
    var names = model.LoadCases.Keys.Concat(model.Combinations.Keys).Take(cases).ToArray();
    if (names.Length < cases) Console.WriteLine($"   il modello ha {names.Length} casi e combinazioni, non {cases}");
    row["caseNames"] = names.Length;
    var sources = Directory.GetFiles(directory, "plate-results-*.json").Concat(Directory.GetFiles(directory, "beam-results-*.json"))
        .Select(f => JsonNode.Parse(File.ReadAllText(f))).ToArray();
    long samples = 0;
    for (int first = 0; first < names.Length; first += 10)
    {
        var responses = new List<CivilNxResponse>();
        for (int k = first; k < Math.Min(names.Length, first + 10); k++)
            foreach (var source in sources) responses.Add(new CivilNxResponse("post/TABLE", Replay(source, names[k], 1 + 0.001 * k)));
        var report = CivilNxResults.Import(model, snapshot, responses, "misura-" + (first / 10 + 1).ToString(CultureInfo.InvariantCulture));
        if (report.Status != ImportStatus.Completed && report.Status != ImportStatus.Partial)
            throw new InvalidDataException("Import rifiutato: " + string.Join("; ", report.Diagnostics.Select(d => d.Code + " " + d.Message)));
        samples += report.ImportedSamples;
        model.CompactResults();
    }
    return (model, samples);
}

// The rows of one post/TABLE response for another case: case name replaced, every value column scaled.
static string Replay(JsonNode source, string caseName, double factor)
{
    var root = source.DeepClone();
    var table = root.AsObject().First().Value;
    var head = table["HEAD"].AsArray().Select(h => h.GetValue<string>()).ToArray();
    var text = new HashSet<string> { "Index", "Elem", "Load", "Node", "Part" };
    int load = Array.IndexOf(head, "Load");
    foreach (var item in table["DATA"].AsArray())
    {
        var cells = item.AsArray();
        cells[load] = caseName;
        for (int c = 0; c < head.Length; c++)
            if (!text.Contains(head[c]))
                cells[c] = (double.Parse(cells[c].GetValue<string>(), CultureInfo.InvariantCulture) * factor).ToString("R", CultureInfo.InvariantCulture);
    }
    return root.ToJsonString();
}

static (GPC.Model.Models.Model, long) GridModel(int side, int cases)
{
    var model = new GPC.Model.Models.Model("griglia");
    int n = side + 1;
    for (int j = 0; j < n; j++) for (int i = 0; i < n; i++)
        model.NodesElements.Add(new NodeElement(new Point3d(i * 1000.0, j * 1000.0, 0), id: 1 + j * n + i) { Source = new SourceIdentity("Grid", "r1", EntityFamily.Node, (1 + j * n + i).ToString(CultureInfo.InvariantCulture)) });
    var property = new ConcretePlateProperty(ConcreteMaterialEN1992Data.C25_30, 300, 300, "Shell 300"); model.AddProperty(property);
    int areaId = 100000;
    for (int j = 0; j < side; j++) for (int i = 0; i < side; i++)
    {
        int a = 1 + j * n + i;
        var area = new AreaElement(null, property, id: ++areaId) { Source = new SourceIdentity("Grid", "r1", EntityFamily.Shell, areaId.ToString(CultureInfo.InvariantCulture)) };
        model.AreaElements.Add(area); model.ConnectShell(area.Id, new[] { a, a + 1, a + 1 + n, a + n });
        area.Assignments.PhysicalThickness = 300;
        area.CoordinateSystem = new CoordinateSystem(new Point3d(i * 1000.0, j * 1000.0, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));
    }
    for (int c = 0; c < cases; c++) model.LoadCases.Add(new LoadCaseBase("LC" + c));
    model.CaptureAnalysis(ReinforcementAnalysisRole.ExcludedFromAnalysis, "Griglia sintetica");
    string fingerprint = model.AnalysisFingerprint();
    model.Datasets.Add("griglia", new AnalysisDataset { Id = "griglia", Program = "Grid", ModelRevision = "r1", AnalysisId = "static", InputFingerprint = fingerprint,
        NormalizedUnits = "N,mm,rad", IsSynthetic = true, Semantics = AnalysisSemantics.LinearStatic });
    var corners = new[] { new Point2d(-1, -1), new Point2d(1, -1), new Point2d(1, 1), new Point2d(-1, 1) };
    foreach (var lc in model.LoadCases.Values)
        foreach (var area in model.AreaElements.Values)
            for (int k = 0; k < 4; k++)
                area.AddResult(new PlateElementResult(new List<IPlateResultLocation> {
                    new PointResultPlateForces(lc, new ResultPlateForces(area.CoordinateSystem, 150 + k, 50, 10, 4, 6, 80000 + k, 20000, 5000), corners[k], "Node")
                    { State = new ResultState { DatasetId = "griglia", ModelRevision = "r1", InputFingerprint = fingerprint, Semantics = AnalysisSemantics.LinearStatic,
                        ConcomitantStateId = lc.Name, Components = Enumerable.Repeat(ComponentAvailability.Available, 8).ToArray(), IsSynthetic = true, IsCombined = false,
                        SourceRecord = "Grid", Coverage = "Exported samples only." },
                      PointKind = ShellResultPointKind.ElementNodeExtrapolated, CoordinateKind = ResultCoordinateKind.Natural } }));
    return (model, (long)cases * model.AreaElements.Count * 4);
}
