using System.Reflection;
using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Combinations;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Results.Locations;
using GPC.Model.Sections;
using GPC.Model.Structure.Assignments;

namespace UnitTest;

/// <summary>
/// C4 (11/11/2026): more Straus7 behaviour of BridGHe 0.3.0 behind explicit options (user rule: what 0.3.0 accepted, 0.4.0 accepts and writes
/// as 0.3.0 did), today's file and run as the default:
/// <list type="bullet">
/// <item>point 1: the distributed supports, temperature gradients and tapers of a beam with a principal angle, on the native axes 1 and 2 as
/// 0.3.0 wrote them (<see cref="Straus7WriteOptions.SectionAxisDataOnNativeAxes"/>);</item>
/// <item>point 3: the nonlinear increments of a model without stages solved as 0.3.0 solved them (NLA with the staged flag off,
/// <see cref="Straus7ModelWritePlan.RunUnstagedNonlinear"/>), in the order of the combinations (<see cref="Straus7WriteOptions.NonlinearIncrementsInModelOrder"/>).</item>
/// </list>
/// Native cases need GPC_STRAUS7_WRITE and a licence.
/// </summary>
[TestClass]
public class Straus7LegacyParityC4Test
{
    private static Straus7LegacyParityTest.Recorder Record(Model model, Straus7WriteOptions? options)
    {
        var api = DispatchProxy.Create<Straus7LegacyParityTest.IRecordedLegacyWriter, Straus7LegacyParityTest.Recorder>();
        Straus7ModelWritePlan.Prepare(model, null, null, null, options).Write(api, model); return (Straus7LegacyParityTest.Recorder)(object)api;
    }
    private static string Text((string Name, object?[] Args) call) =>
        call.Name + "(" + string.Join(";", call.Args.Select(a => a is double[] d ? string.Join(",", d.Select(v => v.ToString("R"))) : a is int[] i ? string.Join(",", i) : a?.ToString())) + ")";
    private static readonly string[] AxisData = { nameof(IStraus7WriteApi.SetBeamSupport), nameof(IStraus7WriteApi.SetBeamTemperatureGradient), nameof(IStraus7WriteApi.SetBeamTaper) };

    /// <summary>The cantilever of <see cref="Straus7ModelWriteTest.Cantilever"/> (beam along x, section axes V1 = y, V2 = z) on an unequal angle
    /// L 150 × 10 / 100 × 12 (a principal angle) or a rectangle (none), with two distributed supports (along +2 of the section axes, compression
    /// only with a gap; along −1, tension only), a temperature gradient and a taper on axis 1 when <paramref name="axisData"/>.</summary>
    internal static Model Rotated(bool angle, bool axisData = true)
    {
        var model = Straus7ModelWriteTest.Cantilever(); var beam = model.BeamElements.Values.Single();
        beam.BeamProperty = new ElasticBeamProperty(angle ? "L 150x100" : "Rect 200x100", new Material("S355", 210000, .3, 7.85e-9, 1.2e-5),
            angle ? new SectionL(150, 10, 100, 12, "L 150x10/100x12") : new SectionRectangular(200, 100));
        if (!axisData) return model;
        var axes = beam.Assignments.SectionAxes; var fc = model.FreedomCases.Values.Single(); var lc = model.LoadCases.Values.Single();
        var frame = new CoordinateSystem(new Point3d(0, 0, 0), axes.V1, axes.V2);
        beam.Assignments.Physical.Supports.Add(new BeamDistributedSupport { FreedomCase = fc, Stiffness = 3, Gap = .5, Behavior = DistributedSpringBehavior.CompressionOnly, Direction = new Vector3d(0, 1, 0), Axes = frame });
        beam.Assignments.Physical.Supports.Add(new BeamDistributedSupport { FreedomCase = fc, Stiffness = 2, Gap = 0, Behavior = DistributedSpringBehavior.TensionOnly, Direction = new Vector3d(-1, 0, 0), Axes = frame });
        beam.Assignments.Physical.TemperatureGradients.Add(new BeamTemperatureGradient { Case = lc, Axis1 = .01, Axis2 = -.02 });
        beam.Assignments.Physical.Taper = new BeamSectionTaper { ScaleI1 = 1, ScaleJ1 = .8, Alignment1 = TaperAlignment.PositiveSide, ScaleI2 = 1, ScaleJ2 = 1, Alignment2 = TaperAlignment.Symmetric };
        return model;
    }

    [TestMethod]
    public void C4_1_RotatedSectionAxisData_AreRefusedByDefault_AndWrittenAsTheLegacyExporterWithTheOption()
    {
        var rotated = Rotated(true);
        Assert.IsTrue(Math.Abs(((ElasticBeamProperty)rotated.BeamElements.Values.Single().BeamProperty).Section.AngleX1) > .1, "the angle has a principal angle");
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(rotated)).Message, "PrincipalAngleWithTaperGradientOrSupportRequiresExplicitAxisConvention");
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(rotated, null, null, null, new Straus7WriteOptions())).Message, "PrincipalAngleWithTaperGradientOrSupportRequiresExplicitAxisConvention");
        var legacy = Record(rotated, new Straus7WriteOptions { SectionAxisDataOnNativeAxes = true });
        int lc = 1, fc = 1;
        // The calls of 0.3.0 (BeamSupportExport, StrausR3Helper.cs:970-1020 and :1225-1230): +2 → adPlus2 (3), compression only, [k, gap];
        // −1 → adMinus1 (0), tension only: compression flag and the direction flipped (^1) → 1; the gradients along 1 and 2; the taper of axis 1
        // (top: btTop, the GPC positive side) and the unit taper of axis 2 the Converter writes on every tapered beam.
        CollectionAssert.AreEqual(new[] { $"SetBeamTemperatureGradient(1;{lc};0.01;-0.02)", $"SetBeamSupport(1;3;{fc};True;3;0.5)", $"SetBeamSupport(1;1;{fc};True;2;0)",
            "SetBeamTaper(1;1;PositiveSide;1;0.8)", "SetBeamTaper(1;2;Symmetric;1;1)" }, legacy.Calls.Where(c => AxisData.Contains(c.Name)).Select(Text).ToArray());
        // The same calls as a beam without a principal angle (the rectangle) with the same section axes and data: no turn by the principal angle.
        // (Copies of the same model: the same GUIDs, so the same native numbering.)
        var twin = GPC.Model.Core.ModelValues.Copy(rotated); twin.BeamElements.Values.Single().BeamProperty = ((ElasticBeamProperty)Rotated(false, false).BeamElements.Values.Single().BeamProperty);
        CollectionAssert.AreEqual(Record(twin, null).Calls.Where(c => AxisData.Contains(c.Name)).Select(Text).ToArray(), legacy.Calls.Where(c => AxisData.Contains(c.Name)).Select(Text).ToArray());
        // Nothing else changes: every other call is the one of the same angle without the axis data (the beam axes are the principal axes, as before).
        var bare = GPC.Model.Core.ModelValues.Copy(rotated); var physical = bare.BeamElements.Values.Single().Assignments.Physical;
        physical.Supports.Clear(); physical.TemperatureGradients.Clear(); physical.Taper = null;
        CollectionAssert.AreEqual(Record(bare, null).Calls.Select(Text).ToArray(), legacy.Calls.Where(c => !AxisData.Contains(c.Name)).Select(Text).ToArray());
        var axes = legacy.Named(nameof(IStraus7ModelWriteApi.SetBeamAxes)).Single(); var section = rotated.BeamElements.Values.Single().Assignments.SectionAxes;
        Assert.IsTrue(Math.Abs(((double[])axes.Args[1]!)[0] * section.V1.X + ((double[])axes.Args[1]!)[1] * section.V1.Y + ((double[])axes.Args[1]!)[2] * section.V1.Z - 1) > 1e-3, "native axes are the principal axes, not the section axes");
        // A beam without a principal angle is written as before with or without the option.
        var plain = Rotated(false);
        CollectionAssert.AreEqual(Record(plain, null).Calls.Select(Text).ToArray(), Record(plain, new Straus7WriteOptions { SectionAxisDataOnNativeAxes = true }).Calls.Select(Text).ToArray());
    }

    /// <summary>The cantilever with the combinations «Half» (0.5 P), «Full» (P) and «Double» (2 P), added in this order (not the order by name).</summary>
    internal static Model Increments()
    {
        var model = Straus7ModelWriteTest.Cantilever(); var p = model.LoadCases.Values.Single();
        foreach (var (name, factor) in new[] { ("Half", .5), ("Full", 1.0), ("Double", 2.0) })
        { var c = new Combination(name); c.AddLoadCaseCoefficients(new List<LoadCaseBase> { p }, new List<double> { factor }); model.Combinations.Add(c); }
        return model;
    }

    [TestMethod]
    public void C4_3_UnstagedIncrements_FollowTheModelOrderWithTheOption()
    {
        var model = Increments();
        var byName = Record(model, null).Named(nameof(IStraus7ModelWriteApi.AddNonlinearIncrement)).Select(Text).ToArray();
        CollectionAssert.AreEqual(new[] { "AddNonlinearIncrement(Double;1;2)", "AddNonlinearIncrement(Full;1;1)", "AddNonlinearIncrement(Half;1;0.5)" }, byName);
        var legacy = Record(model, new Straus7WriteOptions { NonlinearIncrementsInModelOrder = true });
        CollectionAssert.AreEqual(new[] { "AddNonlinearIncrement(Half;1;0.5)", "AddNonlinearIncrement(Full;1;1)", "AddNonlinearIncrement(Double;1;2)" },
            legacy.Named(nameof(IStraus7ModelWriteApi.AddNonlinearIncrement)).Select(Text).ToArray());
        // Every other call the same, in the same order (the LSA combinations stay by name).
        CollectionAssert.AreEqual(Record(model, null).Calls.Where(c => c.Name != nameof(IStraus7ModelWriteApi.AddNonlinearIncrement)).Select(Text).ToArray(),
            legacy.Calls.Where(c => c.Name != nameof(IStraus7ModelWriteApi.AddNonlinearIncrement)).Select(Text).ToArray());
    }

    [TestMethod]
    public void C4_3_UnstagedRun_IsCheckedBeforeAnyNativeCall()
    {
        string folder = Path.GetTempPath(), output = Path.Combine(folder, "c4-never-" + Guid.NewGuid().ToString("N") + ".st7");
        var options = Straus7NonlinearRunOptions.OfFile();
        // No declared output case.
        var model = Increments(); var plan = Straus7ModelWritePlan.Prepare(model);
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => plan.RunUnstagedNonlinear(model, model.LoadCases.Values.Single().Guid, output, folder, options, "NLA")).Message, "UnstagedRunRequiresDeclaredOutputCase");
        // No phase name; no combinations.
        model = Increments(); model.LoadCases.Add("NLA", new GPC.Model.Stages.StageResultCase("NLA", "NLA")); plan = Straus7ModelWritePlan.Prepare(model);
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => plan.RunUnstagedNonlinear(model, model.LoadCases["NLA"].Guid, output, folder, options, " ")).Message, "UnstagedRunRequiresPhaseName");
        var none = Straus7ModelWriteTest.Cantilever(); none.LoadCases.Add("NLA", new GPC.Model.Stages.StageResultCase("NLA", "NLA")); var nonePlan = Straus7ModelWritePlan.Prepare(none);
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => nonePlan.RunUnstagedNonlinear(none, none.LoadCases["NLA"].Guid, output, folder, options, "NLA")).Message, "UnstagedRunRequiresCombinations");
        Assert.ThrowsException<ArgumentNullException>(() => plan.RunUnstagedNonlinear(model, model.LoadCases["NLA"].Guid, output, folder, null!, "NLA"));
        Assert.IsFalse(File.Exists(output));
    }

    private static double[] Displacement(Model model, NodeElement node, string caseName, string? step) =>
        node.Results.SelectMany(r => r.Results).OfType<NodeResultDisplacement>().Where(l => l.Case.Name == caseName && l.State?.Step == step)
            .Select(l => new[] { l.ResultDisplacement.D1, l.ResultDisplacement.D2, l.ResultDisplacement.D3, l.ResultDisplacement.R1, l.ResultDisplacement.R2, l.ResultDisplacement.R3 }).Single();

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeC4_1_RotatedSectionAxisData_ReadBackAsWritten_AndTheModelSolves()
    {
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        var model = Rotated(true); var plan = Straus7ModelWritePlan.Prepare(model, null, null, null, new Straus7WriteOptions { SectionAxisDataOnNativeAxes = true });
        var run = plan.RunLinearStatic(model, Path.Combine(folder, "rotated.st7"), folder);
        Assert.AreEqual(ImportStatus.Completed, run.ImportResults(model, "rotated").Status);
        var beam = model.BeamElements.Values.Single();
        using (var reader = new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath, folder);
            (double[] Values, bool Compression) Support(int d)
            {
                try { var v = reader.ReadBeamSupport(1, d, 1, out bool compression); return (v, compression); }
                catch (Straus7ApiException) { return (new double[2], false); } // no support on that direction
            }
            var supports = Enumerable.Range(0, 4).Select(Support).ToArray();
            for (int d = 0; d < 4; d++) Console.WriteLine($"C4 native point 1: support direction {d}: {string.Join(",", supports[d].Values.Select(v => v.ToString("R")))} compression only {supports[d].Compression}");
            // 0.3.0: +2 → adPlus2 (3) [3, 0.5] compression only; −1 tension only → adPlus1 (1) [2, 0] compression only; nothing on 0 and 2.
            CollectionAssert.AreEqual(new[] { 3, .5 }, supports[3].Values); Assert.IsTrue(supports[3].Compression);
            CollectionAssert.AreEqual(new[] { 2.0, 0 }, supports[1].Values); Assert.IsTrue(supports[1].Compression);
            Assert.IsTrue(supports[0].Values.All(v => v == 0) && supports[2].Values.All(v => v == 0));
            CollectionAssert.AreEqual(new[] { .01, -.02 }, reader.ReadBeamTemperatureGradient(1, 1));
            CollectionAssert.AreEqual(new[] { 1, .8 }, reader.ReadBeamTaper(1, 1, out int alignment1)); Assert.AreEqual(1, alignment1, "btTop");
            // The unit symmetric taper of axis 2 is not kept by Straus7 (St7GetBeamTaper2: «Data not found», measured on R31 on 11/11): no taper
            // on axis 2, as in the file of 0.3.0, which wrote none.
            string axis2 = null; try { axis2 = string.Join(",", reader.ReadBeamTaper(1, 2, out int alignment2)) + " " + alignment2; } catch (Straus7ApiException) { }
            Assert.IsNull(axis2, "no taper on axis 2: " + axis2);
            // The native axes of the beam are the principal axes of the plan (the section axes turned by the principal angle).
            var axes = reader.ReadElement(Straus7Entity.Beam, 1).InitialAxes; double angle = ((ElasticBeamProperty)beam.BeamProperty).Section.AngleX1, c = Math.Cos(angle), s = Math.Sin(angle);
            var v1 = beam.Assignments.SectionAxes.V1 * c + beam.Assignments.SectionAxes.V2 * s;
            Console.WriteLine($"C4 native point 1: principal angle {angle:R} rad, native axis 1 {axes[0]:R},{axes[1]:R},{axes[2]:R}, expected {v1.X:R},{v1.Y:R},{v1.Z:R}");
            Assert.AreEqual(1, axes[0] * v1.X + axes[1] * v1.Y + axes[2] * v1.Z, 1e-6);
        }
        var tip = Displacement(model, beam.NodeJ, model.LoadCases.Values.Single().Name, null);
        Console.WriteLine("C4 native point 1: the angle with supports, gradient and taper solves; tip " + string.Join(",", tip.Select(v => v.ToString("R"))) + ": " + folder);
        Assert.IsTrue(tip.All(double.IsFinite) && Math.Abs(tip[2]) > 0);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeC4_3_UnstagedNonlinear_SolvesTheIncrementsOfStage0_AsTheLegacySolver1()
    {
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        var model = Increments(); model.LoadCases.Add("NLA", new GPC.Model.Stages.StageResultCase("NLA", "NLA"));
        var plan = Straus7ModelWritePlan.Prepare(model, null, null, null, new Straus7WriteOptions { NonlinearIncrementsInModelOrder = true });
        var run = plan.RunUnstagedNonlinear(model, model.LoadCases["NLA"].Guid, Path.Combine(folder, "unstaged.st7"), folder, Straus7NonlinearRunOptions.OfFile(), "Unstaged");
        using (var reader = new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath, folder);
            Assert.IsFalse(reader.ReadStagedAnalysis()); Assert.AreEqual(0, reader.StageCount);
            Assert.AreEqual(3, reader.ReadStageIncrementCount(0));
            CollectionAssert.AreEqual(new[] { "Half", "Full", "Double" }, Enumerable.Range(1, 3).Select(k => reader.ReadStageIncrementName(0, k)).ToArray());
            int p = Enumerable.Range(1, reader.LoadCaseCount).Single(i => reader.ReadLoadCase(i) == "P");
            CollectionAssert.AreEqual(new[] { .5, 1, 2 }, Enumerable.Range(1, 3).Select(k => reader.ReadStageFactor(0, k, p, false)).ToArray());
            var file = reader.OpenResults(run.ResultPath); Assert.AreEqual(3, file.PrimaryCases);
            CollectionAssert.AreEqual(new[] { "1: Half", "2: Full", "3: Double" }, Enumerable.Range(1, 3).Select(k => reader.ReadResultCase(k).Name).ToArray());
            Assert.IsTrue(Enumerable.Range(1, 3).All(k => reader.ReadResultCase(k).Stage == 0 && reader.ReadResultConverged(k)));
        }
        var report = run.ImportResults(model, "unstaged");
        Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        var states = model.AllElements.SelectMany(e => e.Results).SelectMany(r => r.Results).Select(l => l.Case.Name + "|" + l.State.Phase + "|" + l.State.Step).Distinct().OrderBy(s => s).ToArray();
        CollectionAssert.AreEqual(new[] { "NLA|Unstaged|1: Half", "NLA|Unstaged|2: Full", "NLA|Unstaged|3: Double" }, states);
        // The same increments in one explicit stage with every element active (the run BridGHe made before C4): the same mechanics.
        var staged = Increments(); var group = staged.AddGroup("All"); foreach (var e in staged.BeamElements.Values) e.Groups.Add(group.Name, group);
        var lc = staged.LoadCases.Values.Single(); var fc = staged.FreedomCases.Values.Single();
        staged.Stages.Add(new GPC.Model.Stages.Stage(1, "Stage") { Definition = new GPC.Model.Stages.StageDefinition(1, 0), AnalysisState = new GPC.Model.Stages.StageAnalysisState {
            ActiveGroupGuids = new[] { group.Guid }, ActiveFreedomCaseGuids = new[] { fc.Guid },
            Increments = staged.Combinations.Values.Select(c => new GPC.Model.Stages.StageLoadIncrement { Name = c.Name,
                LoadFactors = c.GetLoadCaseCoefficientsPair().Select(f => new GPC.Model.Stages.StageCaseFactor { CaseGuid = f.Key.Guid, Factor = f.Value }).ToArray() }).ToArray() } });
        staged.Combinations.Clear(); staged.LoadCases.Add("NLA", new GPC.Model.Stages.StageResultCase("NLA", "NLA"));
        var stagedRun = Straus7ModelWritePlan.Prepare(staged).RunStagedNonlinear(staged, staged.LoadCases["NLA"].Guid, Path.Combine(folder, "one-stage.st7"), folder, Straus7NonlinearRunOptions.OfFile());
        Assert.AreEqual(ImportStatus.Completed, stagedRun.ImportResults(staged, "staged").Status);
        double worst = 0;
        foreach (var (name, k) in new[] { ("Half", 1), ("Full", 2), ("Double", 3) })
        {
            var a = Displacement(model, model.BeamElements.Values.Single().NodeJ, "NLA", k + ": " + name);
            var b = Displacement(staged, staged.BeamElements.Values.Single().NodeJ, "NLA", k + ": " + name);
            Console.WriteLine($"C4 native point 3: {name} tip unstaged {string.Join(",", a.Select(v => v.ToString("R")))}; one stage {string.Join(",", b.Select(v => v.ToString("R")))}");
            worst = Math.Max(worst, Math.Abs(a[2] - b[2]) / Math.Abs(b[2]));
        }
        Console.WriteLine($"C4 native point 3: worst relative difference of the tip Uz, unstaged vs one stage: {worst:R}: {folder}");
        Assert.IsTrue(worst < 1e-6, "the same mechanics: " + worst);
    }
}
