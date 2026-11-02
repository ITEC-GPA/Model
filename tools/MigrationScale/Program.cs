using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Data.Concrete;
using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Sections.Concrete;

// Identical source and domain binaries for net48 and net8. No Rhino/Straus or result data.
// One unmeasured small graph warm-up; explicit first full-size copy, then five warm copies.
// Full GC outside every timer. Wall time, thread allocations when available, live heap estimate
// delta and process peak working set are distinct measurements, never interchangeable.
internal static class Program
{
    private static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;
    private static readonly Func<long> Allocated = Allocations();
    private static StreamWriter output;
    private static int nodes, shells;

    private static int Main(string[] args)
    {
        if (args.Length < 1) throw new ArgumentException("Usage: MigrationScale <output.csv> [side ...]; defaults 10 30 100.");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[0])));
        using (output = new StreamWriter(new FileStream(args[0], FileMode.CreateNew)))
        {
            output.WriteLine("runtime,nodes,shells,operation,repetition,seconds,threadAllocatedBytes,heapDeltaBytes,peakWorkingSetBytes");
            ModelValues.Copy(Build(3));
            foreach (int side in args.Skip(1).DefaultIfEmpty("10,30,100").SelectMany(s => s.Split(',')).Select(int.Parse))
            {
                if (side < 2 || side > 200) throw new ArgumentOutOfRangeException("side", "Expected 2..200.");
                nodes = side * side; shells = 2 * (side - 1) * (side - 1);
                Model model = null, copy = null, reopened = null;
                byte[] archive = null;
                Measure("build-and-assemble", 0, () => model = Build(side));
                Measure("copy-first", 0, () => copy = ModelValues.Copy(model));
                for (int i = 1; i <= 5; i++)
                {
                    Measure("copy-warm", i, () => copy = ModelValues.Copy(model));
                    CheckIsolation(model, copy);
                    copy = null;
                }
                string fingerprint = null;
                Measure("analysis-fingerprint", 0, () => fingerprint = model.AnalysisFingerprint());
                Measure("save-memory", 0, () => { using (var stream = new MemoryStream()) { ModelArchive.Save(model, stream); archive = stream.ToArray(); } });
                Measure("load-memory", 0, () => { using (var stream = new MemoryStream(archive, false)) reopened = ModelArchive.Load(stream); });
                if (fingerprint != reopened.AnalysisFingerprint()) throw new InvalidOperationException("Archive changed the analysis fingerprint.");
                CheckIsolation(model, reopened);
                Console.WriteLine($"PASS {nodes} nodes, {shells} shells, {archive.Length} archive bytes; copy isolation and archive fingerprint.");
                GC.KeepAlive(model);
            }
        }
        return 0;
    }

    private static Func<long> Allocations()
    {
        var method = typeof(GC).GetMethod("GetAllocatedBytesForCurrentThread", Type.EmptyTypes);
        return method == null ? (Func<long>)(() => -1) : (Func<long>)Delegate.CreateDelegate(typeof(Func<long>), method);
    }

    private static void Measure(string operation, int repetition, Action action)
    {
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        long heap = GC.GetTotalMemory(false), before = Allocated();
        var clock = Stopwatch.StartNew(); action(); clock.Stop();
        long allocations = Allocated();
        output.WriteLine(string.Join(",", RuntimeInformation.FrameworkDescription, nodes, shells, operation, repetition,
            clock.Elapsed.TotalSeconds.ToString("R", Invariant), before < 0 ? -1 : allocations - before,
            GC.GetTotalMemory(false) - heap, Process.GetCurrentProcess().PeakWorkingSet64));
        output.Flush();
        Console.WriteLine($"{nodes}/{shells} {operation} {repetition}: {clock.Elapsed.TotalSeconds.ToString("F4", Invariant)} s");
    }

    private static Model Build(int side)
    {
        var model = new Model("Migration triangular grid");
        var property = new ConcretePlateProperty(ConcreteMaterialEN1992Data.C25_30, 100, 100, "Shell 100 mm");
        model.AddProperty(property);
        for (int j = 0; j < side; j++) for (int i = 0; i < side; i++)
            model.NodesElements.Add(new NodeElement(new Point3d(1000.0 * i, 1000.0 * j, 0), id: 1 + j * side + i));
        int id = 0;
        for (int j = 0; j < side - 1; j++) for (int i = 0; i < side - 1; i++)
        {
            int a = 1 + j * side + i;
            foreach (int[] corners in new[] { new[] { a, a + 1, a + side + 1 }, new[] { a, a + side + 1, a + side } })
            {
                var element = new AreaElement(null, property, id: ++id);
                model.AreaElements.Add(element);
                model.ConnectShell(id, corners);
            }
        }
        return model;
    }

    private static void CheckIsolation(Model source, Model copy)
    {
        if (source.NodesElements.Count != copy.NodesElements.Count || source.AreaElements.Count != copy.AreaElements.Count)
            throw new InvalidOperationException("Copy lost entities.");
        var original = source.NodesElements.First().Value; var node = copy.NodesElements.First().Value;
        var shell = copy.AreaElements.First().Value;
        if (ReferenceEquals(original, node) || !ReferenceEquals(node, shell.Nodes[0]))
            throw new InvalidOperationException("Copy did not detach nodes while retaining its internal connectivity.");
        double x = original.Position.X;
        node.Position = new Point3d(x + 17, node.Position.Y, node.Position.Z);
        if (original.Position.X != x || shell.Nodes[0].Position.X != x + 17)
            throw new InvalidOperationException("Branch mutation leaked or broke shared connectivity.");
    }
}
