// Size and time of the archive forms (lane L5, review A2) on a tensile net like the one of Arachne: k x k nodes, the perimeter
// restrained, horizontal, vertical and diagonal cables with a force-density goal, two membrane triangles per square, one
// displacement result per node. Usage: CompactArchiveProbe <output.csv> [k ...] [--xml-sidecar] (default k: 32 100).
// For each k: ModelArchive XML (the ANTHEA form) and Compact, ModelValueArchive, the sidecar (version 2) prepare/save/open in both
// graph forms; every reopened model must give the input fingerprint and the same results.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Restraints;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;
using GPC.Model.Results.Storage;
using GPC.Model.Structure.Assignments;
using GPC.Persistence;

var inv = CultureInfo.InvariantCulture;
string output = Path.GetFullPath(args[0]);
bool xmlSidecar = args.Contains("--xml-sidecar");
var sides = args.Skip(1).Where(a => !a.StartsWith("--")).SelectMany(a => a.Split(',')).Select(int.Parse).DefaultIfEmpty(32).ToList();
if (args.Skip(1).All(a => a.StartsWith("--"))) sides.Add(100);
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
using var report = new StreamWriter(new FileStream(output, FileMode.CreateNew));
report.WriteLine("runtime,nodes,beams,triangles,operation,seconds,bytes");
string runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
Grid(3);
foreach (int k in sides)
{
    int n = 0, b = 0, t = 0;
    void Measure(string name, Func<long> action)
    {
        GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
        var watch = Stopwatch.StartNew(); long bytes = action(); watch.Stop();
        report.WriteLine(string.Join(",", runtime, n, b, t, name, watch.Elapsed.TotalSeconds.ToString("F4", inv), bytes)); report.Flush();
        Console.WriteLine($"{n}/{b}/{t} {name}: {watch.Elapsed.TotalSeconds:F3} s {(bytes > 0 ? bytes.ToString("N0", inv) + " B" : "")}");
    }
    Model model = null!;
    Measure("build", () => { model = Grid(k); return 0; });
    n = model.NodesElements.Count; b = model.BeamElements.Count; t = model.AreaElements.Count;
    if (args.Contains("--dump"))
    {
        var node = model.NodesElements.Values.First();
        var stored = typeof(Element).GetField("_storedResults", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(node)!;
        var segments = (Array)stored.GetType().GetProperty("Segments", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(stored)!;
        foreach (var segment in segments)
        {
            var payload = (byte[])segment.GetType().GetProperty("Payload", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(segment)!;
            using var inflate = new System.IO.Compression.DeflateStream(new MemoryStream(payload), System.IO.Compression.CompressionMode.Decompress);
            using var raw = new MemoryStream(); inflate.CopyTo(raw);
            Console.WriteLine($"segment: payload {payload.Length} B, raw {raw.Length} B");
            Console.WriteLine(new string(raw.ToArray().Select(c => c >= 32 && c < 127 ? (char)c : '.').ToArray()));
        }
    }
    string fingerprint = null!, numeric = Numeric(model);
    Measure("analysis-fingerprint", () => { fingerprint = model.AnalysisFingerprint(); return 0; });
    Measure("copy", () => { ModelArchive.Copy(model); return 0; });
    var bare = Grid(k, false);
    foreach (var format in new[] { ModelArchiveFormat.Xml, ModelArchiveFormat.Compact })
    {
        byte[] graph = null!;
        Measure("archive-save no results " + format, () => { using var s = new MemoryStream(); ModelArchive.Save(bare, s, format); graph = s.ToArray(); return graph.Length; });
        Measure("archive-load no results " + format, () => { using var s = new MemoryStream(graph, false); ModelArchive.Load(s); return 0; });
    }
    foreach (var format in new[] { ModelArchiveFormat.Xml, ModelArchiveFormat.Compact })
    {
        byte[] archive = null!; Model reopened = null!;
        Measure("archive-save " + format, () => { using var s = new MemoryStream(); ModelArchive.Save(model, s, format); archive = s.ToArray(); return archive.Length; });
        Measure("archive-load " + format, () => { using var s = new MemoryStream(archive, false); reopened = ModelArchive.Load(s); return 0; });
        Check(reopened, "archive " + format);
    }
    var property = (ElasticBeamProperty)model.BeamProperties.Values.First();
    foreach (var format in new[] { ModelArchiveFormat.Xml, ModelArchiveFormat.Compact })
    {
        byte[] value = null!;
        Measure("value-save " + format, () => { using var s = new MemoryStream(); ModelValueArchive.Save(property, s, format); value = s.ToArray(); return value.Length; });
        using (var s = new MemoryStream(value, false))
            if (GPC.Model.Core.ModelValues.Fingerprint(new object[] { ModelValueArchive.Load<ElasticBeamProperty>(s) }) != GPC.Model.Core.ModelValues.Fingerprint(new object[] { property }))
                throw new Exception("Value archive " + format + " changed the property.");
    }
    foreach (var format in xmlSidecar ? new[] { ModelArchiveFormat.Compact, ModelArchiveFormat.Xml } : new[] { ModelArchiveFormat.Compact })
    {
        string folder = Path.Combine(Path.GetDirectoryName(output)!, "sidecar-" + k + "-" + format + "-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        string document = Path.Combine(folder, "probe.ref"), sidecar = ExternalModelArchive.PathFor(document);
        var store = new ExternalModelArchive { GraphFormat = format }; ExternalModelReference reference = null!;
        Measure("sidecar-prepare " + format, () => { reference = store.Prepare(model); return 0; });
        Measure("sidecar-save " + format, () => { reference = store.Save(sidecar, new[] { reference }, r => BlockArchive.WriteFile(document, s => r[0].Write(s)))[0]; return new FileInfo(sidecar).Length; });
        Measure("sidecar-open " + format, () => { var run = ExternalModelArchive.Open(document, reference); Check(run.Model, "sidecar " + format); return 0; });
        Directory.Delete(folder, true);
    }

    void Check(Model reopened, string what)
    {
        if (reopened.AnalysisFingerprint() != fingerprint) throw new Exception(what + ": fingerprint changed.");
        if (Numeric(reopened) != numeric) throw new Exception(what + ": numbers or results changed.");
    }
}
Console.WriteLine("PASS");

static Model Grid(int k, bool results = true)
{
    var owner = Guid.Parse("7b0c2a55-0000-4000-8000-00000000a000");
    Guid Id(string key) { using var sha = System.Security.Cryptography.SHA256.Create(); var h = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(owner + key)); return new Guid(h.Take(16).ToArray()); }
    var load = new LoadCaseBase(Id("case"), "FF");
    var steel = new GPC.Model.Materials.Material("Steel", 210000, 0.3, 7.849e-9, 1.17e-5);
    double r = 5, area = Math.PI * r * r, j = Math.PI * Math.Pow(r, 4) / 4;
    var outline = new Shape2d(new Polygon2d(Enumerable.Range(0, 64).Select(i => new Point2d(r * Math.Cos(2 * Math.PI * i / 64), r * Math.Sin(2 * Math.PI * i / 64)))));
    var section = new GPC.Model.Sections.Section(outline, area, j, j, 0, j, j, 2 * j, new Point2d(0, 0), 0, j / r, j / r, j / r, j / r, "D10");
    var property = new ElasticBeamProperty("Cable", steel, section);
    var fabric = new MembranePlateProperty("Fabric") { MassPerArea = 1.05e-9 };
    var points = new Point3d[k, k]; var nodes = new NodeElement[k, k]; var elements = new List<Element>();
    for (int y = 0; y < k; y++)
        for (int x = 0; x < k; x++)
        {
            bool edge = x == 0 || y == 0 || x == k - 1 || y == k - 1;
            points[x, y] = new Point3d(1000.0 * x + 0.1 * Math.Sin(y), 1000.0 * y + 0.1 * Math.Cos(x), edge ? 200 * Math.Sin(0.3 * x) : 0.01 * Math.Sin(x + y));
            var node = nodes[x, y] = new NodeElement(Id("node:" + x + ":" + y), points[x, y]);
            elements.Add(node);
            if (!edge) continue;
            node.Assignments.Restrains.Add(new RestrainAssignment { Restrain = new NodeRestrain(node, new List<DofRestrain> {
                new DofRestrain(GeometryRestrain.DOF.DX, true), new DofRestrain(GeometryRestrain.DOF.DY, true), new DofRestrain(GeometryRestrain.DOF.DZ, true) }) });
        }
    void Beam(NodeElement a, NodeElement c, string key)
    {
        var beam = new BeamElement(Id(key), a.Position, c.Position, property);
        beam.ConnectNodes(a, c);
        beam.Assignments.Formulation = BeamFormulation.Cable;
        beam.Attributes.Add(new FormFindingGoalAttribute(new FormFindingGoal { Kind = FormFindingGoalKind.ForceDensity, Target = 1, Weight = 1 }, load));
        elements.Add(beam);
    }
    void Triangle(NodeElement a, NodeElement c, NodeElement d, string key)
    {
        var id = Id(key);
        var shell = new AreaElement(id, null, fabric);
        shell.ConnectNodes(a, c, d);
        elements.Add(shell);
    }
    for (int y = 0; y < k; y++)
        for (int x = 0; x < k; x++)
        {
            if (x > 0) Beam(nodes[x - 1, y], nodes[x, y], "h:" + x + ":" + y);
            if (y > 0) Beam(nodes[x, y - 1], nodes[x, y], "v:" + x + ":" + y);
            if (x > 0 && y > 0)
            {
                Beam(nodes[x - 1, y - 1], nodes[x, y], "d:" + x + ":" + y);
                Triangle(nodes[x - 1, y - 1], nodes[x, y - 1], nodes[x, y], "t1:" + x + ":" + y);
                Triangle(nodes[x - 1, y - 1], nodes[x, y], nodes[x - 1, y], "t2:" + x + ":" + y);
            }
        }
    var model = ModelAssembly.Assemble(elements, owner, "Scale");
    if (!model.LoadCases.ContainsKey(load.Name)) model.LoadCases.Add(load);
    if (!results) return model;
    var cases = model.LoadCases.Values.ToArray();
    string input = model.AnalysisFingerprint();
    foreach (var node in model.NodesElements.Values)
        node.AddResult(new NodeResult(cases.Select(c => (INodeResultLocation)new NodeResultDisplacement(c,
            new ResultDisplacement(node.Position.X * 1e-4, node.Position.Y * 1e-4, Math.Sin(node.Id), 0, 0, 0))
        { State = new ResultState { DatasetId = "form-finding", InputFingerprint = input, Step = c.Name } }).ToList()));
    model.CompactResults();
    return model;
}

static string Numeric(Model model)
{
    using var stream = new MemoryStream();
    using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true))
    {
        foreach (var node in model.NodesElements.Values)
        {
            writer.Write(node.Guid.ToByteArray()); writer.Write(node.Id); writer.Write(node.Position.X); writer.Write(node.Position.Y); writer.Write(node.Position.Z);
            foreach (var sample in node.Results.SelectMany(r => r.Results).Cast<NodeResultDisplacement>())
            {
                writer.Write(((GPC.Model.ModelObject)sample.Case).Guid.ToByteArray()); writer.Write(sample.State.DatasetId); writer.Write(sample.State.Step);
                var d = sample.ResultDisplacement; writer.Write(d.D1); writer.Write(d.D2); writer.Write(d.D3);
            }
        }
        foreach (var beam in model.BeamElements.Values) { writer.Write(beam.Guid.ToByteArray()); writer.Write(beam.Id); writer.Write(beam.NodeI.Id); writer.Write(beam.NodeJ.Id); }
        foreach (var shell in model.AreaElements.Values) { writer.Write(shell.Guid.ToByteArray()); foreach (var node in shell.Nodes) writer.Write(node.Id); }
    }
    return ArchiveBlock.Digest(stream.ToArray());
}
