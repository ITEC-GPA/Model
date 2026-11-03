// Cross-process, cross-runtime archive compatibility and unchanged-autosave probe.
// write <new directory> [nodes=1000] [cases=20] | read <directory>
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;
using GPC.Model.Results.Storage;
using GPC.Persistence;

if (args[0] == "bench") { Bench.Run(args); return; }
string mode = args[0], folder = Path.GetFullPath(args[1]);
string document = Path.Combine(folder, "probe.gh-reference"), sidecar = ExternalModelArchive.PathFor(document);
var rows = new List<string> { "metric,value" };
void Measure(string name, Action action) { var watch = Stopwatch.StartNew(); action(); rows.Add(name + "," + watch.Elapsed.TotalSeconds.ToString("R", CultureInfo.InvariantCulture)); }
if (mode == "write")
{
    if (Directory.Exists(folder)) throw new IOException("Capture folder already exists."); Directory.CreateDirectory(folder);
    int nodes = args.Length > 2 ? int.Parse(args[2], CultureInfo.InvariantCulture) : 1000;
    int cases = args.Length > 3 ? int.Parse(args[3], CultureInfo.InvariantCulture) : 20;
    var model = ModelAssembly.Assemble(Enumerable.Range(0, nodes).Select(i => new NodeElement(new Point3d(i / 7.0, i / 11.0, i / 13.0))), Guid.NewGuid());
    var loads = Enumerable.Range(0, cases).Select(i => new LoadCaseBase("case-" + i)).ToArray(); foreach (var load in loads) model.LoadCases.Add(load);
    model.CaptureAnalysis();
    foreach (var node in model.NodesElements.Values)
        foreach (var load in loads)
            node.AddResult(new NodeResult(new List<INodeResultLocation> { new NodeResultDisplacement(load,
                new ResultDisplacement(node.Id / 7.0, BitConverter.Int64BitsToDouble(long.MinValue), double.Epsilon, 1.0 / 3, node.Id * 1e-17, -node.Id / 13.0))
                { State = new ResultState { DatasetId = "probe", InputFingerprint = model.Analysis.InputFingerprint, Step = load.Name } } }));
    Measure("compactSeconds", () => model.CompactResults());
    var map = model.AssemblyMap.ToSolver(model, "synthetic", model.AllElements.Select(e => new KeyValuePair<Guid, int>(e.Guid, e.Id + 10000)));
    var store = new ExternalModelArchive(); ExternalModelReference reference = null;
    Measure("prepareSeconds", () => reference = store.Prepare(model, map));
    Measure("firstSaveSeconds", () => reference = store.Save(sidecar, new[] { reference }, r => BlockArchive.WriteFile(document, s => r[0].Write(s)))[0]);
    File.WriteAllText(Path.Combine(folder, "numeric.sha256"), Numeric(model));
    File.WriteAllText(Path.Combine(folder, "input.sha256"), model.AnalysisFingerprint());
    File.WriteAllText(Path.Combine(folder, "solver-map.sha256"), GPC.Model.Core.ModelValues.Fingerprint(new object[] { map }));
    rows.Add("nodes," + nodes); rows.Add("cases," + cases); rows.Add("samples," + (long)nodes * cases);
}
else if (mode != "read") throw new ArgumentException("write|read");
ExternalModelReference read;
using (var input = File.OpenRead(document)) read = ExternalModelReference.Read(input);
ExternalModelRun run = null;
Measure("reopenSeconds", () => run = ExternalModelArchive.Open(document, read));
if (Numeric(run.Model) != File.ReadAllText(Path.Combine(folder, "numeric.sha256")) || run.Model.AnalysisFingerprint() != File.ReadAllText(Path.Combine(folder, "input.sha256")) ||
    GPC.Model.Core.ModelValues.Fingerprint(new object[] { run.SolverMap }) != File.ReadAllText(Path.Combine(folder, "solver-map.sha256"))) throw new Exception("Cross-runtime content changed.");
if (mode == "write")
{
    var session = new ExternalModelArchive(sidecar, new[] { read });
    string hash = ArchiveBlock.Digest(File.ReadAllBytes(sidecar)); DateTime timestamp = File.GetLastWriteTimeUtc(sidecar);
    for (int i = 0; i < 5; i++) Measure("unchangedSave" + i + "Seconds", () => session.Save(sidecar, new[] { read }, r => BlockArchive.WriteFile(document, s => r[0].Write(s))));
    if (ArchiveBlock.Digest(File.ReadAllBytes(sidecar)) != hash || File.GetLastWriteTimeUtc(sidecar) != timestamp) throw new Exception("Unchanged autosave rewrote the archive.");
}
rows.Add("referenceBytes," + new FileInfo(document).Length); rows.Add("archiveBytes," + new FileInfo(sidecar).Length);
rows.Add("runtime," + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
rows.Add("status,PASS");
foreach (string row in rows) Console.WriteLine(row);
if (mode == "write") File.WriteAllLines(Path.Combine(folder, "metrics.csv"), rows);

static string Numeric(Model model)
{
    using var stream = new MemoryStream();
    using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
        foreach (var node in model.NodesElements.Values)
        {
            writer.Write(node.Guid.ToByteArray()); writer.Write(node.Id);
            writer.Write(node.Position.X); writer.Write(node.Position.Y); writer.Write(node.Position.Z);
            foreach (var sample in node.Results.SelectMany(r => r.Results).Cast<NodeResultDisplacement>())
            {
                writer.Write(((GPC.Model.ModelObject)sample.Case).Guid.ToByteArray()); writer.Write(sample.Case.Name); writer.Write(sample.State.DatasetId); writer.Write(sample.State.Step);
                var d = sample.ResultDisplacement; writer.Write(d.D1); writer.Write(d.D2); writer.Write(d.D3); writer.Write(d.R1); writer.Write(d.R2); writer.Write(d.R3);
            }
        }
    return ArchiveBlock.Digest(stream.ToArray());
}
