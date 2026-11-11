using System.Reflection;
using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Combinations;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Loads.Assignments;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Restraints;
using GPC.Model.Results.Locations;
using GPC.Model.Sections;
using GPC.Model.Structure.Assignments;

namespace UnitTest;

/// <summary>
/// C2 (11/11/2026): the Straus7 behaviour of BridGHe 0.3.0 behind explicit options of the writer and of the run, today's file as the default
/// (points P26-01â€¦07 of the ChatGPT parity review, user decision of 11/11: Â«come la 0.3.0Â»). Native cases need GPC_STRAUS7_WRITE and a licence.
/// </summary>
[TestClass]
public class Straus7LegacyParityTest
{
    public interface IRecordedLegacyWriter : Straus7ExporterCorrectionsTest.IRecordedWriter, IStraus7PropertyNameWriteApi { }
    public class Recorder : DispatchProxy
    {
        public readonly List<(string Name, object?[] Args)> Calls = new();
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            Calls.Add((method!.Name, (args ?? Array.Empty<object?>()).Select(a => a is Array array ? array.Clone() : a).ToArray()));
            return method.ReturnType == typeof(void) || !method.ReturnType.IsValueType ? null : Activator.CreateInstance(method.ReturnType);
        }
        public (string Name, object?[] Args)[] Named(string name) => Calls.Where(c => c.Name == name).ToArray();
    }
    private static Recorder Record(Model model, Straus7WriteOptions? options)
    {
        var api = DispatchProxy.Create<IRecordedLegacyWriter, Recorder>();
        Straus7ModelWritePlan.Prepare(model, null, null, null, options).Write(api, model); return (Recorder)(object)api;
    }
    private static string Text((string Name, object?[] Args) call) =>
        call.Name + "(" + string.Join(";", call.Args.Select(a => a is double[] d ? string.Join(",", d.Select(v => v.ToString("R"))) : a is int[] i ? string.Join(",", i) : a?.ToString())) + ")";

    /// <summary>Three plates of one property Â«Soletta 250Â», one of Â«Soletta 300Â», on a 4 x 1 strip; each plate with its own plate name.</summary>
    internal static Model PlateStrip() => ModelAssembly.Assemble(PlateStripElements(out _, out _), Guid.NewGuid());
    private static List<Element> PlateStripElements(out LoadCaseBase lc, out FreedomCase freedom)
    {
        var concrete = new Material("C32/40", 33000, .2, 2.5e-9, 1e-5);
        var thin = new ElasticPlateProperty("Soletta 250", concrete, 250, 250); var thick = new ElasticPlateProperty("Soletta 300", new Material("C35/45", 34000, .2, 2.5e-9, 1e-5), 300, 300);
        var nodes = Enumerable.Range(0, 5).SelectMany(i => new[] { new NodeElement(new Point3d(1000 * i, 0, 0)), new NodeElement(new Point3d(1000 * i, 1000, 0)) }).ToArray();
        freedom = new FreedomCase("Supports"); lc = new LoadCaseBase("Pressure");
        foreach (var n in new[] { nodes[0], nodes[1], nodes[8], nodes[9] })
            n.Assignments.Restrains.Add(new RestrainAssignment { FreedomCase = freedom, Restrain = new NodeRestrain(n, Enumerable.Range(0, 6).Select(i => new DofRestrain((GeometryRestrain.DOF)i, true)).ToList()) });
        var plates = new List<Element>();
        for (int i = 0; i < 4; i++)
        {
            var plate = new AreaElement(null, i == 3 ? thick : thin) { Name = "P" + (i + 1) };
            plate.ConnectNodes(new[] { nodes[2 * i], nodes[2 * i + 2], nodes[2 * i + 3], nodes[2 * i + 1] });
            plates.Add(plate);
        }
        nodes[4].Loads.Add(new PointLoad(0, 0, -1000, 0, 0, 0, nodes[4].Position, lc));
        return plates;
    }

    [TestMethod]
    public void DefaultOptionsWriteTheFileOfToday()
    {
        foreach (var model in new[] { PlateStrip(), Straus7ModelWriteTest.Cantilever() })
        {
            var today = Record(model, null).Calls.Select(Text).ToArray();
            CollectionAssert.AreEqual(today, Record(model, new Straus7WriteOptions()).Calls.Select(Text).ToArray());
            Assert.IsFalse(today.Any(c => c.StartsWith("SetBeamSectionName") || c.StartsWith("SetPlateMaterialName")));
        }
    }

    [TestMethod]
    public void P26_01_SectionLabelIsWrittenRightAfterThePropertyAsTheLegacyExporter()
    {
        var model = Straus7ModelWriteTest.Cantilever(); var beam = model.BeamElements.Values.Single();
        var calls = Record(model, new Straus7WriteOptions { BeamSectionLabels = new Dictionary<Guid, string> { [beam.Guid] = "SolidRect" } }).Calls.Select(Text).ToList();
        int created = calls.FindIndex(c => c.StartsWith("CreateBeamProperty")), label = calls.IndexOf("SetBeamSectionName(1;SolidRect)"), geometry = calls.FindIndex(c => c.StartsWith("SetBeamSectionGeometry"));
        Assert.IsTrue(created >= 0 && label == created + 1 && geometry > label, string.Join(" | ", calls));
        // Nothing else changes: the same operations without the label.
        calls.RemoveAt(label); CollectionAssert.AreEqual(Record(model, null).Calls.Select(Text).ToArray(), calls);
        Assert.ThrowsException<ArgumentException>(() => Straus7ModelWritePlan.Prepare(model, null, null, null, new Straus7WriteOptions { BeamSectionLabels = new Dictionary<Guid, string> { [Guid.NewGuid()] = "IBeam" } }));
    }

    [TestMethod]
    public void P26_02_P26_03_OneNativePropertyPerPlateProperty_WithItsMaterialName()
    {
        var model = PlateStrip();
        var today = Record(model, null); var legacy = Record(model, new Straus7WriteOptions { PlateMaterialNames = true, PlatePropertiesByProperty = true });
        var before = today.Named(nameof(IStraus7ModelWriteApi.CreatePlateProperty)); var after = legacy.Named(nameof(IStraus7ModelWriteApi.CreatePlateProperty));
        // Plates are numbered by Id, then GUID: the order of P1…P4 is not fixed, so the checks go by name.
        Assert.AreEqual(4, before.Length); CollectionAssert.AreEquivalent(new object[] { "P1", "P2", "P3", "P4" }, before.Select(c => c.Args[1]).ToArray());
        CollectionAssert.AreEqual(before.Select(c => c.Args[0]).ToArray(), legacy.Named(nameof(IStraus7ModelWriteApi.SetElement)).Where(c => (int)c.Args[0]! == 2).Select(c => c.Args[1]).ToArray());
        Assert.AreEqual(2, after.Length); CollectionAssert.AreEquivalent(new object[] { "Soletta 250", "Soletta 300" }, after.Select(c => c.Args[1]).ToArray());
        CollectionAssert.AreEqual(new object[] { 1, 2 }, after.Select(c => c.Args[0]).ToArray());
        string Data((string Name, object?[] Args) call) => string.Join(";", call.Args.Skip(2).Select(a => a is double[] d ? string.Join(",", d.Select(v => v.ToString("R"))) : a?.ToString()));
        int Shared(string property) => (int)after.Single(c => (string)c.Args[1]! == property).Args[0]!;
        // The same native data as the property of each plate of that property (formulation, thicknesses, material, orthotropy).
        foreach (var own in before)
        {
            string property = (string)own.Args[1]! == "P4" ? "Soletta 300" : "Soletta 250";
            Assert.AreEqual(Data(own), Data(after.Single(c => (string)c.Args[1]! == property)), (string)own.Args[1]!);
            int plate = (int)own.Args[0]!;
            Assert.AreEqual(Shared(property), (int)legacy.Named(nameof(IStraus7ModelWriteApi.SetElement)).Single(c => (int)c.Args[0]! == 2 && (int)c.Args[1]! == plate).Args[2]!);
        }
        CollectionAssert.AreEquivalent(new[] { "SetPlateMaterialName(" + Shared("Soletta 250") + ";C32/40)", "SetPlateMaterialName(" + Shared("Soletta 300") + ";C35/45)" }, legacy.Named(nameof(IStraus7PropertyNameWriteApi.SetPlateMaterialName)).Select(Text).ToArray());
        // Every operation that is not a plate property operation is the same, in the same order (element data on each plate as before).
        string[] Others(Recorder log) => log.Calls.Where(c => c.Name != nameof(IStraus7ModelWriteApi.CreatePlateProperty) && c.Name != nameof(IStraus7PropertyNameWriteApi.SetPlateMaterialName)
            && !(c.Name == nameof(IStraus7ModelWriteApi.SetElement) && (int)c.Args[0]! == 2)).Select(Text).ToArray();
        CollectionAssert.AreEqual(Others(today), Others(legacy));
    }

    /// <summary>Two clamped spans of 1000 mm on x: case A a concentrated force at 0.37 of the first span (off the regular stations), case B a
    /// distributed force on the second span, case C a nodal force at the middle support; combinations AB = 1.35 A + 1.5 B and AC = A + C.
    /// Fixed GUIDs (C5, 11/11): ModelAssembly numbers the elements in GUID order and the Converter numbers them by Id, so with random GUIDs two
    /// instances (P26-04 compares two) had another native numbering, and values differed at bit level from the equation order.</summary>
    internal static Model TwoSpans()
    {
        static Guid Fixed(int k) => new Guid(0x0c2026a0, 0x1111, 0x2026, 0, 0, 0, 0, 0, 0, 0, (byte)k);
        var nodes = new[] { new NodeElement(Fixed(1), new Point3d(0, 0, 0)), new NodeElement(Fixed(2), new Point3d(1000, 0, 0)), new NodeElement(Fixed(3), new Point3d(2000, 0, 0)) };
        var freedom = new FreedomCase("Supports");
        foreach (var n in new[] { nodes[0], nodes[2] })
            n.Assignments.Restrains.Add(new RestrainAssignment { FreedomCase = freedom, Restrain = new NodeRestrain(n, Enumerable.Range(0, 6).Select(i => new DofRestrain((GeometryRestrain.DOF)i, true)).ToList()) });
        var a = new LoadCaseBase("A"); var b = new LoadCaseBase("B"); var c = new LoadCaseBase("C");
        var property = new ElasticBeamProperty("Rect 200x100", new Material("S355", 210000, .3, 7.85e-9, 1.2e-5), new SectionRectangular(200, 100));
        var beams = new List<Element>();
        for (int i = 0; i < 2; i++)
        {
            var beam = new BeamElement(Fixed(11 + i), nodes[i].Position, nodes[i + 1].Position, property); beam.ConnectNodes(nodes[i], nodes[i + 1]);
            beam.Assignments.Formulation = BeamFormulation.StraightTwoNode;
            beam.Assignments.SectionAxes = new CoordinateSystem(nodes[i].Position, new Vector3d(0, 1, 0), new Vector3d(0, 0, 1));
            beam.Assignments.Physical = new BeamPhysicalData { MinimumOutputStations = 5 };
            beams.Add(beam);
        }
        var first = (BeamElement)beams[0]; var second = (BeamElement)beams[1];
        first.Assignments.Loads.Add(new BeamLoadAssignment { Start = .37, End = .37, Concentrated = new PointLoad(0, 0, -1000, 0, 0, 0, new Point3d(370, 0, 0), a) });
        second.Assignments.Loads.Add(new BeamLoadAssignment { Start = 0, End = 1, LengthConvention = BeamLoadLengthConvention.ActualLength,
            StartIntensity = new LineLoad(0, 0, -2, 0, 0, 0, second.Line, b), EndIntensity = new LineLoad(0, 0, -2, 0, 0, 0, second.Line, b) });
        nodes[1].Loads.Add(new PointLoad(0, 0, -500, 0, 0, 0, nodes[1].Position, c));
        var model = ModelAssembly.Assemble(beams, Fixed(20));
        var ab = new Combination("AB"); ab.AddLoadCaseCoefficients(new List<LoadCaseBase> { model.LoadCases["A"], model.LoadCases["B"] }, new List<double> { 1.35, 1.5 }); model.Combinations.Add(ab);
        var ac = new Combination("AC"); ac.AddLoadCaseCoefficients(new List<LoadCaseBase> { model.LoadCases["A"], model.LoadCases["C"] }, new List<double> { 1, 1 }); model.Combinations.Add(ac);
        return model;
    }

    [TestMethod]
    public void P26_04_P26_07_RunOptionsAreCheckedBeforeAnyNativeCall()
    {
        var model = TwoSpans(); var plan = Straus7ModelWritePlan.Prepare(model);
        string output = Path.Combine(Path.GetTempPath(), "c2-never-" + Guid.NewGuid().ToString("N") + ".st7");
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => plan.RunLinearStatic(model, output, Path.GetTempPath(), new Straus7LinearRunOptions { LoadCases = new[] { Guid.NewGuid() } })).Message, "UnknownSolvedLoadCase");
        // AC needs C, which is not solved.
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => plan.RunLinearStatic(model, output, Path.GetTempPath(), new Straus7LinearRunOptions {
            LoadCases = new[] { model.LoadCases["A"].Guid, model.LoadCases["B"].Guid }, Combinations = new[] { model.Combinations["AC"].Guid } })).Message, "ReadCombinationRequiresItsSolvedCases");
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => plan.RunLinearStatic(model, output, Path.GetTempPath(), new Straus7LinearRunOptions { Combinations = new[] { Guid.NewGuid() } })).Message, "UnknownReadCombination");
        Assert.IsFalse(File.Exists(output));
        Assert.AreEqual(2, plan.CombinationIds.Count); Assert.AreEqual(1, plan.CombinationIds[model.Combinations["AB"].Guid]); Assert.AreEqual(2, plan.CombinationIds[model.Combinations["AC"].Guid]);
    }

    private static Dictionary<string, double[]> NodeRows(Model model, string dataset)
    {
        var rows = new Dictionary<string, double[]>();
        foreach (var node in model.NodesElements.Values)
            foreach (var d in node.Results.SelectMany(r => r.Results).OfType<NodeResultDisplacement>().Where(l => l.State?.DatasetId == dataset))
                rows["node " + node.Id + " " + d.Case.Name + (d.State.Step == null ? "" : " " + d.State.Step)] = new[] { d.ResultDisplacement.D1, d.ResultDisplacement.D2, d.ResultDisplacement.D3, d.ResultDisplacement.R1, d.ResultDisplacement.R2, d.ResultDisplacement.R3 };
        return rows;
    }
    private static List<(double Station, double[] Values)> BeamRows(BeamElement beam, string caseName) =>
        beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().Where(l => l.Case.Name == caseName).OrderBy(l => l.ParametricDistance)
            .Select(l => (l.ParametricDistance, new[] { l.ResultBeamForces.N, l.ResultBeamForces.V1, l.ResultBeamForces.V2, l.ResultBeamForces.T, l.ResultBeamForces.M1, l.ResultBeamForces.M2 })).ToList();

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeP26_04_OnlyTheSelectedCasesAreSolved_AndTheirValuesDoNotMove()
    {
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        var all = TwoSpans(); var planAll = Straus7ModelWritePlan.Prepare(all);
        var runAll = planAll.RunLinearStatic(all, Path.Combine(folder, "all.st7"), folder);
        Assert.AreEqual(ImportStatus.Completed, runAll.ImportResults(all, "all").Status);
        var some = TwoSpans(); var plan = Straus7ModelWritePlan.Prepare(some);
        // Selection of C2: A and C only (B disabled). Result case 2 is load case 3 (C).
        var run = plan.RunLinearStatic(some, Path.Combine(folder, "selected.st7"), folder, new Straus7LinearRunOptions { LoadCases = new[] { some.LoadCases["A"].Guid, some.LoadCases["C"].Guid } });
        CollectionAssert.AreEqual(new[] { 1, 3 }, run.SolvedLoadCases.ToArray());
        var report = run.ImportResults(some, "selected");
        Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        using (var reader = new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath, folder);
            Assert.IsTrue(reader.ReadLinearCaseEnabled(1)); Assert.IsFalse(reader.ReadLinearCaseEnabled(2)); Assert.IsTrue(reader.ReadLinearCaseEnabled(3));
            var file = reader.OpenResults(run.ResultPath); Assert.AreEqual(2, file.PrimaryCases);
            Assert.AreEqual("1: A", reader.ReadResultCase(1).Name); Assert.AreEqual("3: C", reader.ReadResultCase(2).Name);
        }
        var expected = NodeRows(all, "all"); var actual = NodeRows(some, "selected");
        Assert.IsFalse(actual.Keys.Any(k => k.EndsWith(" B")), "B is not solved");
        Assert.AreEqual(expected.Count(p => !p.Key.EndsWith(" B")), actual.Count);
        foreach (var pair in actual) CollectionAssert.AreEqual(expected[pair.Key], pair.Value, "bit for bit: " + pair.Key);
        foreach (var beam in some.BeamElements.Values)
            foreach (var name in new[] { "A", "C" })
            {
                var reference = BeamRows(all.BeamElements.Values.Single(b => b.Id == beam.Id), name); var rows = BeamRows(beam, name);
                Assert.AreEqual(reference.Count, rows.Count);
                for (int i = 0; i < rows.Count; i++) { Assert.AreEqual(reference[i].Station, rows[i].Station); CollectionAssert.AreEqual(reference[i].Values, rows[i].Values); }
            }
        Console.WriteLine("P26-04 native: solved 1, 3 of 3; " + actual.Count + " node rows of A and C equal bit for bit to the run of every case: " + folder);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeP26_07_CombinationsReadFromStraus7_ShowTheStationsTheSuperpositionLoses()
    {
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        var model = TwoSpans(); var plan = Straus7ModelWritePlan.Prepare(model);
        var run = plan.RunLinearStatic(model, Path.Combine(folder, "combinations.st7"), folder, new Straus7LinearRunOptions {
            LoadCases = new[] { model.LoadCases["A"].Guid, model.LoadCases["B"].Guid }, Combinations = new[] { model.Combinations["AB"].Guid } });
        // The import checks that opening with the combinations leaves the model and result files unchanged (StrausExportFilesChanged).
        var report = run.ImportResults(model, "combined", false, false);
        Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        var nodes = NodeRows(model, "combined");
        Assert.IsTrue(nodes.Keys.Any(k => k.EndsWith(" AB")) && !nodes.Keys.Any(k => k.EndsWith(" AC") || k.EndsWith(" C")));
        // Every family of the combination is IsCombined, attached to the combination itself.
        var combined = model.AllElements.SelectMany(e => e.Results).SelectMany(r => r.Results).Where(l => l.Case.Name == "AB").ToArray();
        Assert.IsTrue(combined.Length > 0 && combined.All(l => l.State.IsCombined == true && l.Case is Combination));
        // Node displacements: the Straus7 combination equals the superposition of the cases to roundoff.
        double worstNode = 0;
        foreach (var node in model.NodesElements.Values)
        {
            var ab = nodes["node " + node.Id + " AB"]; var a = nodes["node " + node.Id + " A"]; var b = nodes["node " + node.Id + " B"];
            for (int k = 0; k < 6; k++) { double sum = 1.35 * a[k] + 1.5 * b[k]; if (ab[k] != sum) worstNode = Math.Max(worstNode, Math.Abs(ab[k] - sum) / Math.Max(Math.Abs(ab[k]), Math.Abs(sum))); }
        }
        Assert.IsTrue(worstNode < 1e-12, "node displacements: " + worstNode);
        // Beams: the stations of the Straus7 combination against the stations common to A and B (the superposition rule of BridGHe until C2).
        var lines = new List<string>(); int lost = 0;
        foreach (var beam in model.BeamElements.Values.OrderBy(b => b.Id))
        {
            var rowsA = BeamRows(beam, "A"); var rowsB = BeamRows(beam, "B"); var rowsAB = BeamRows(beam, "AB");
            var common = rowsA.Select(r => r.Station).Where(s => rowsB.Any(r => Math.Abs(r.Station - s) < 1e-9)).ToArray();
            var missing = rowsAB.Where(r => !common.Any(s => Math.Abs(s - r.Station) < 1e-9)).ToArray(); lost += missing.Length;
            double worstBeam = 0, scale = rowsAB.SelectMany(r => r.Values).Max(Math.Abs);
            foreach (var s in common)
            {
                var ab = rowsAB.FirstOrDefault(r => Math.Abs(r.Station - s) < 1e-9); if (ab.Values == null) continue;
                var a = rowsA.First(r => Math.Abs(r.Station - s) < 1e-9).Values; var b = rowsB.First(r => Math.Abs(r.Station - s) < 1e-9).Values;
                for (int k = 0; k < 6; k++) worstBeam = Math.Max(worstBeam, Math.Abs(ab.Values[k] - (1.35 * a[k] + 1.5 * b[k])) / scale);
            }
            lines.Add($"beam {beam.Id}: stations A {rowsA.Count}, B {rowsB.Count}, common {common.Length}, Straus7 combination {rowsAB.Count} ({string.Join(" ", rowsAB.Select(r => r.Station.ToString("R")))}); "
                + $"combination stations not common to A and B: {string.Join(" ", missing.Select(r => r.Station.ToString("R") + " M1 " + r.Values[4].ToString("R")))}; worst |combination - superposition| / max at the common stations {worstBeam:R}");
            Assert.IsTrue(worstBeam < 1e-12, lines.Last());
        }
        foreach (var line in lines) Console.WriteLine("P26-07 native: " + line);
        Assert.IsTrue(lost > 0, "the case shows stations of the Straus7 combination that the superposition at the common stations loses");
        Console.WriteLine("P26-07 native: " + folder);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeP26_01_02_03_NamesReadBack_AndTheResultsDoNotMove()
    {
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        Model Model()
        {
            var elements = PlateStripElements(out var lc, out var freedom);
            // Five beams with the dimensional shapes the writer writes: circle, CHS, rectangle, RHS, I (the labels of 0.3.0).
            var sections = new ISectionShape[] { new SectionCircular(200), new SectionCHS(200, 10), new SectionRectangular(400, 200), new SectionRHS(400, 200, 10, 10, 12, 12, "RHS"), new SectionH(400, 8, 180, 14, 200, 12, "I") };
            var nodes = Enumerable.Range(0, 6).Select(i => new NodeElement(new Point3d(1000 * i, 5000, 0))).ToArray();
            nodes[0].Assignments.Restrains.Add(new RestrainAssignment { FreedomCase = freedom, Restrain = new NodeRestrain(nodes[0], Enumerable.Range(0, 6).Select(i => new DofRestrain((GeometryRestrain.DOF)i, true)).ToList()) });
            for (int i = 0; i < sections.Length; i++)
            {
                var beam = new BeamElement(nodes[i].Position, nodes[i + 1].Position, new ElasticBeamProperty("B" + i, new Material("S355", 210000, .3, 7.85e-9, 1.2e-5), sections[i]));
                beam.ConnectNodes(nodes[i], nodes[i + 1]); beam.Assignments.Formulation = BeamFormulation.StraightTwoNode;
                beam.Assignments.SectionAxes = new CoordinateSystem(nodes[i].Position, new Vector3d(0, 1, 0), new Vector3d(0, 0, 1)); elements.Add(beam);
            }
            nodes[5].Loads.Add(new PointLoad(0, 0, -100, 0, 0, 0, nodes[5].Position, lc));
            return ModelAssembly.Assemble(elements, Guid.NewGuid());
        }
        // The labels of 0.3.0 by native section type (bsCircularSolid 1, bsCircularHollow 2, bsSquareSolid 3, bsSquareHollow 4, bsISection 7).
        var labels = new Dictionary<int, string> { [1] = "SolidRound", [2] = "HollowRound", [3] = "SolidRect", [4] = "HollowRect", [7] = "IBeam" };
        string LabelOf(BeamElement b) => ((ElasticBeamProperty)b.BeamProperty).Section switch
        { SectionCircular => labels[1], SectionCHS => labels[2], SectionRectangular => labels[3], SectionRHS => labels[4], SectionH => labels[7], _ => throw new ArgumentException() };
        var results = new List<Dictionary<string, double[]>>();
        var source = Model();
        foreach (bool legacy in new[] { false, true })
        {
            // The same model (same GUIDs, same native numbering) for both files.
            var model = GPC.Model.Core.ModelValues.Copy(source); var beams = model.BeamElements.Values.OrderBy(b => b.Id).ThenBy(b => b.Guid).ToArray();
            var options = legacy ? new Straus7WriteOptions { BeamSectionLabels = beams.ToDictionary(b => b.Guid, LabelOf), PlateMaterialNames = true, PlatePropertiesByProperty = true } : null;
            var plan = Straus7ModelWritePlan.Prepare(model, null, null, null, options);
            var run = plan.RunLinearStatic(model, Path.Combine(folder, legacy ? "legacy.st7" : "today.st7"), folder);
            Assert.AreEqual(ImportStatus.Completed, run.ImportResults(model, "names", false, true).Status);
            results.Add(NodeRows(model, "names"));
            using (var reader = new Straus7NativeApi())
            {
                reader.OpenModelReadOnly(run.ModelPath, folder);
                for (int i = 0; i < beams.Length; i++)
                {
                    var property = reader.ReadProperty(Straus7Entity.Beam, reader.ReadElement(Straus7Entity.Beam, i + 1).Property);
                    Assert.AreEqual(legacy ? labels[property.SectionType] : "", property.SectionName, "beam " + (i + 1));
                    Assert.AreEqual(LabelOf(beams[i]), labels[property.SectionType], "native beam " + (i + 1) + " is the GPC beam " + i + " by Id, then GUID");
                    Console.WriteLine($"P26-01 native {(legacy ? "0.3.0" : "today")}: beam {i + 1} section type {property.SectionType} label '{property.SectionName}'");
                }
                // Native plate i is the i-th plate by Id, then GUID.
                var plates = model.AreaElements.Values.OrderBy(p => p.Id).ThenBy(p => p.Guid).ToArray();
                var plateProperties = Enumerable.Range(1, 4).Select(i => reader.ReadElement(Straus7Entity.Plate, i).Property).ToArray();
                Assert.AreEqual(legacy ? 2 : 4, plateProperties.Distinct().Count());
                for (int i = 0; i < 4; i++)
                {
                    var property = reader.ReadProperty(Straus7Entity.Plate, plateProperties[i]); var gpc = (ElasticPlateProperty)plates[i].PlateProperty;
                    Console.WriteLine($"P26-02/03 native {(legacy ? "0.3.0" : "today")}: plate {i + 1} '{plates[i].Name}' on property {plateProperties[i]} '{property.Name}' material '{property.MaterialName}' thickness {string.Join(",", property.Geometry)} E {property.Material[0]}");
                    // Without a name Straus7 shows its default, "Unknown Material".
                    Assert.AreEqual(legacy ? gpc.Material.Name : "Unknown Material", property.MaterialName);
                    Assert.AreEqual(legacy ? gpc.Name : plates[i].Name, property.Name);
                    Assert.AreEqual(gpc.MembraneThickness, property.Geometry[0]); Assert.AreEqual(gpc.Material.E, property.Material[0]);
                }
            }
        }
        // P26-01/02/03 move no value: every node row equal bit for bit.
        Assert.AreEqual(results[0].Count, results[1].Count);
        foreach (var pair in results[0]) CollectionAssert.AreEqual(pair.Value, results[1][pair.Key], pair.Key);
        Console.WriteLine("P26-01/02/03 native: " + results[0].Count + " node rows equal bit for bit: " + folder);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeP26_06_FileDefaultsAreTheSettingsOfTheLegacyExporter()
    {
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        Model Staged()
        {
            var model = Straus7ModelWriteTest.Cantilever(); var beam = model.BeamElements.Values.Single();
            // A slender cantilever: geometric nonlinearity visible at the tip.
            beam.BeamProperty = new ElasticBeamProperty("soft", new Material("E210", 210, .3, 0, 0), new SectionRectangular(200, 100));
            var group = model.AddGroup("Structure"); beam.Groups.Add(group.Name, group);
            var lc = model.LoadCases.Values.Single(); var fc = model.FreedomCases.Values.Single();
            model.Stages.Add(new GPC.Model.Stages.Stage(1, "Stage") { Definition = new GPC.Model.Stages.StageDefinition(1, 0), AnalysisState = new GPC.Model.Stages.StageAnalysisState {
                ActiveGroupGuids = new[] { group.Guid }, ActiveFreedomCaseGuids = new[] { fc.Guid },
                Increments = new[] { new GPC.Model.Stages.StageLoadIncrement { Name = "Full", LoadFactors = new[] { new GPC.Model.Stages.StageCaseFactor { CaseGuid = lc.Guid, Factor = 1 } },
                    FreedomFactors = new[] { new GPC.Model.Stages.StageCaseFactor { CaseGuid = fc.Guid, Factor = 1 } } } } } });
            model.LoadCases.Add("NLA", new GPC.Model.Stages.StageResultCase("NLA", "NLA"));
            return model;
        }
        var tip = new List<double>(); var settings = new List<Straus7SolverSettings>();
        foreach (var options in new[] { Straus7NonlinearRunOptions.Explicit(), Straus7NonlinearRunOptions.OfFile() })
        {
            var model = Staged(); var plan = Straus7ModelWritePlan.Prepare(model);
            var run = plan.RunStagedNonlinear(model, model.LoadCases["NLA"].Guid, Path.Combine(folder, options.FileDefaults ? "file-defaults.st7" : "explicit.st7"), folder, options);
            Assert.AreEqual(ImportStatus.Completed, run.ImportResults(model, "nla").Status);
            tip.Add(NodeRows(model, "nla").Single(p => p.Key.StartsWith("node " + model.NodesElements.Values.Single(n => n.Position.X == 1000).Id + " ")).Value[2]);
            using (var reader = new Straus7NativeApi()) { reader.OpenModelReadOnly(run.ModelPath, folder); settings.Add(reader.ReadSolverSettings()); }
            Console.WriteLine($"P26-06 native {(options.FileDefaults ? "0.3.0 (file defaults)" : "today (explicit)")}: {settings.Last()}; tip displacement {tip.Last():R} mm");
        }
        Assert.IsFalse(settings[0].NonlinearGeometry); Assert.IsFalse(settings[0].NonlinearMaterial); Assert.AreEqual(1e-8, settings[0].DisplacementTolerance); Assert.AreEqual(100, settings[0].MaximumIterations);
        // The settings of St7NewFile, read on the 0.3.0 files of examples 50 and 99 (F1 traced runs) on 11/11/2026.
        Assert.IsTrue(settings[1].NonlinearGeometry); Assert.IsTrue(settings[1].NonlinearMaterial);
        Assert.AreEqual(1e-4, settings[1].DisplacementTolerance); Assert.AreEqual(1e-3, settings[1].ResidualTolerance); Assert.AreEqual(20, settings[1].MaximumIterations);
        Assert.AreNotEqual(tip[0], tip[1], "geometric nonlinearity changes the tip displacement of the slender cantilever");
        // A linear run with the file defaults leaves the switches on in the saved model, as 0.3.0; today it sets them off.
        foreach (bool file in new[] { false, true })
        {
            var model = Straus7ModelWriteTest.Cantilever(); var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, Path.Combine(folder, file ? "linear-file.st7" : "linear-today.st7"), folder,
                new Straus7LinearRunOptions { FileSolverDefaults = file });
            using (var reader = new Straus7NativeApi()) { reader.OpenModelReadOnly(run.ModelPath, folder); var s = reader.ReadSolverSettings(); Assert.AreEqual(file, s.NonlinearGeometry); Assert.AreEqual(file, s.NonlinearMaterial); Console.WriteLine($"P26-06 native linear {(file ? "0.3.0" : "today")}: {s}"); }
        }
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeP26_05_ErrorTextFallsBackToTheSolverCatalogue()
    {
        Straus7ExporterCorrectionsTest.NativeFolder();
        var text = typeof(Straus7NativeApi).GetMethod("ErrorText", BindingFlags.NonPublic | BindingFlags.Instance)!;
        using (var api = new Straus7NativeApi())
        {
            string apiText = (string)text.Invoke(api, new object[] { 1 })!;  // ERR7_FileAlreadyOpen: API catalogue
            string solverText = (string)text.Invoke(api, new object[] { 1001 })!;  // SE_NoLoadCaseSelected: solver catalogue only
            Console.WriteLine($"P26-05 native: 1 '{apiText}', 1001 '{solverText}'");
            Assert.IsFalse(string.IsNullOrWhiteSpace(apiText)); Assert.IsFalse(string.IsNullOrWhiteSpace(solverText)); Assert.AreNotEqual(apiText, solverText);
        }
    }
}
