// Save/autosave cost of a sidecar with retained runs (review D2, 3/11/2026): bench <new directory> [nodes=30000] [cases=10] [runs=5]
// One result block per element, so the manifest of every run has one entry per element, as on a BridGHe model of example-50 size.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;
using GPC.Model.Results.Storage;
using GPC.Persistence;

internal static class Bench
{
    internal static void Run(string[] args)
    {
        string folder = Path.GetFullPath(args[1]);
        if (Directory.Exists(folder)) throw new IOException("Bench folder already exists."); Directory.CreateDirectory(folder);
        int nodes = args.Length > 2 ? int.Parse(args[2], CultureInfo.InvariantCulture) : 30000;
        int cases = args.Length > 3 ? int.Parse(args[3], CultureInfo.InvariantCulture) : 10;
        int runs = args.Length > 4 ? int.Parse(args[4], CultureInfo.InvariantCulture) : 5;
        string document = Path.Combine(folder, "bench.gh-reference"), sidecar = ExternalModelArchive.PathFor(document);
        var rows = new List<string> { "metric,value", "nodes," + nodes, "cases," + cases, "runs," + runs };
        void Measure(string name, Action action) { var watch = Stopwatch.StartNew(); action(); rows.Add(name + "," + watch.Elapsed.TotalSeconds.ToString("F4", CultureInfo.InvariantCulture)); }
        void Write(IReadOnlyList<ExternalModelReference> r) => BlockArchive.WriteFile(document, s => { foreach (var item in r) item.Write(s); });

        var store = new ExternalModelArchive(); var live = new List<ExternalModelReference>();
        for (int run = 0; run < runs; run++)
        {
            var model = Model(nodes, cases, run); ExternalModelReference reference = null!;
            Measure("prepare" + run + "Seconds", () => reference = store.Prepare(model));
            live.Insert(0, reference);
        }
        IReadOnlyList<ExternalModelReference> saved = null!;
        Measure("firstSaveSeconds", () => saved = store.Save(sidecar, live, r => Write(r)));
        for (int i = 0; i < 3; i++) Measure("unchangedSave" + i + "Seconds", () => saved = store.Save(sidecar, saved, r => Write(r)));
        // A new session on the saved document: the retained runs are known only by their references.
        ExternalModelArchive session = null!;
        Measure("openSessionSeconds", () => session = new ExternalModelArchive(sidecar, saved));
        for (int i = 0; i < 3; i++) Measure("sessionUnchangedSave" + i + "Seconds", () => saved = session.Save(sidecar, saved, r => Write(r)));
        var extra = Model(nodes, cases, runs); ExternalModelReference added = null!;
        Measure("prepareNewRunSeconds", () => added = session.Prepare(extra));
        Measure("sessionChangedSaveSeconds", () => saved = session.Save(sidecar, new[] { added }.Concat(saved.Take(runs - 1)).ToArray(), r => Write(r)));
        Measure("sessionUnchangedAfterChangeSeconds", () => saved = session.Save(sidecar, saved, r => Write(r)));
        Measure("openRunSeconds", () => session.Open(saved[0]));
        rows.Add("archiveBytes," + new FileInfo(sidecar).Length);
        rows.Add("runtime," + System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription);
        foreach (string row in rows) Console.WriteLine(row);
        File.WriteAllLines(Path.Combine(folder, "bench.csv"), rows);
    }

    private static Model Model(int nodes, int cases, int run)
    {
        var model = ModelAssembly.Assemble(Enumerable.Range(0, nodes).Select(i => new NodeElement(new Point3d(i / 7.0, i / 11.0, run))), Guid.NewGuid());
        var loads = Enumerable.Range(0, cases).Select(i => new LoadCaseBase("case-" + i)).ToArray(); foreach (var load in loads) model.LoadCases.Add(load);
        model.CaptureAnalysis();
        foreach (var node in model.NodesElements.Values)
            node.AddResult(new NodeResult(loads.Select(load => (INodeResultLocation)new NodeResultDisplacement(load,
                new ResultDisplacement(node.Id / 7.0 + run, 1.0 / 3, node.Id * 1e-3, 0, 0, -node.Id / 13.0))
                { State = new ResultState { DatasetId = "bench", InputFingerprint = model.Analysis.InputFingerprint, Step = load.Name } }).ToList()));
        model.CompactResults(); return model;
    }
}
