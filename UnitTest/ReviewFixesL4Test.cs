using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Actions.Moving;
using GPC.Model.ElementProperties;
using GPC.Model.Materials;
using GPC.Model.Models;

namespace UnitTest;

/// <summary>Review L4 (4/11/2026) of the P1 fixes: the export preflight and the Straus7 runs. The shear areas are in ShearAreaPartsTest,
/// GPC.Archive in ExternalArchiveTest.</summary>
[TestClass]
public class ReviewFixesL4Test
{
    /// <summary>An elastic plate property with a negative thickness, registered in the model and used by no element.</summary>
    private static Model WithInvalidProperty(Model model)
    {
        var deck = new ElasticPlateProperty("invalid deck", new Material("E210", 210000, .3, 7.85e-9, 1.2e-5), 200, 200) { BendingThickness = -1 };
        model.AddProperty(deck); return model;
    }
    private static void RejectedByThePreflight(Action prepare, string writer)
    {
        var error = Assert.ThrowsException<InvalidOperationException>(prepare, writer);
        StringAssert.Contains(error.Message, "InvalidModelForExport", writer); StringAssert.Contains(error.Message, "InvalidElasticProperty", writer);
    }

    [TestMethod]
    public void L4_EveryStraus7WriterRunsTheDomainPreflight()
    {
        // Before: ModelPreflight.RequireExportable had no callers; an invalid property that no element uses went to the export unnoticed and
        // the stage, moving-load and physical plans did not check the domain at all.
        var cantilever = WithInvalidProperty(Straus7ModelWriteTest.Cantilever());
        Assert.IsTrue(cantilever.ValidateDomain().Any(d => d.Code == "InvalidElasticProperty"));
        RejectedByThePreflight(() => Straus7ModelWritePlan.Prepare(cantilever), "model");
        var identity = GPC.Model.Core.Identity.SolverIdentityMap.Create(cantilever, "Straus7-R3",
            cantilever.AllElements.Select((e, i) => new KeyValuePair<Guid, int>(e.Guid, i + 1)));
        RejectedByThePreflight(() => Straus7PhysicalWritePlan.Prepare(cantilever, identity, new Dictionary<string, int>(), new Dictionary<string, int>()), "physical");
        var staged = WithInvalidProperty(StageAnalysisContractsTest.Sample());
        RejectedByThePreflight(() => Straus7StageWritePlan.Prepare(staged, new Dictionary<Guid, int>(), new Dictionary<Guid, int>(), new Dictionary<Guid, int>()), "stages");
        var moving = WithInvalidProperty(MovingLoadContractsTest.Sample());
        RejectedByThePreflight(() => Straus7MovingWritePlan.Prepare(moving, new Dictionary<Guid, int>()), "moving loads");
        // The valid model is exported as before.
        Straus7ModelWritePlan.Prepare(Straus7ModelWriteTest.Cantilever());
    }

    /// <summary>The horizontal reaction of the clamp of the cantilever of Straus7ModelWriteTest in each generated static case of a moving axle
    /// of 1000 N at 20 m/s along a circular path through (0, 0, 0), <paramref name="onTheArc"/> and (1000, 0, 0), with a centrifugal law.</summary>
    private static List<double> HorizontalReactions(MovingCentrifugalLaw law, Point3d onTheArc, MovingLoadAxes axes, string name)
    {
        var model = Straus7ModelWriteTest.Cantilever(); foreach (var node in model.NodesElements.Values) node.Loads.Clear();
        var path = new MovingLoadPath { Id = Guid.NewGuid(), CaseGuid = model.LoadCases.Values.Single().Guid, Divisions = 4, Shape = MovingPathShape.Circular,
            Start = new Point3d(0, 0, 0), End = new Point3d(1000, 0, 0), Reference = onTheArc, Target = MovingPathTarget.Beams,
            Template = new MovingLoadTemplate { Name = "axle", Centrifugal = law, Vehicles = new[] { new MovingVehicle { Name = "P1000", Speed = 20000,
                PointForces = new[] { new MovingPointForce { Force = new Vector3d(0, 0, -1000), Axes = axes, ApplyCentrifugal = true } } } } } };
        model.MovingLoads = new MovingLoadDefinition(); model.MovingLoads.Paths.Add(path); model.MovingLoads.Validate(model);
        var plan = Straus7ModelWritePlan.Prepare(model, new Dictionary<Guid, Straus7MovingPathOptions> { { path.Id, new Straus7MovingPathOptions { ConvertToStaticCases = true } } });
        string folder = Straus7ExporterCorrectionsTest.NativeFolder(), output = Path.Combine(folder, name + ".st7"), result; IReadOnlyDictionary<int, string> generated;
        using (var writer = new Straus7NativeWriter(output, folder)) { generated = plan.Write(writer, model); writer.Save(); result = writer.Solve(); }
        int clamp = model.NodesElements.Values.Single(n => n.Position.X == 0).Id; var horizontal = new List<double>();
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(output, folder); reader.OpenResults(result);
        foreach (int c in generated.Keys)
        {
            var reaction = reader.ReadNodeResult(clamp, c, true).Values;
            Console.WriteLine($"L4 centrifugal {name} case {c}: reaction X {reaction[0]:G6} Y {reaction[1]:G6} Z {reaction[2]:G6} N");
            if (Math.Abs(reaction[2]) > 1e-6) horizontal.Add(Math.Sqrt(reaction[0] * reaction[0] + reaction[1] * reaction[1]));
        }
        return horizontal;
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void L4_NativeCentrifugalForceOfAnNmmLawMatchesTheHandCalculation()
    {
        // Review L4 on the B8 test (it only read back the text): the force R31 applies for a GPC law, now written in mm and N. The third point of
        // the path lies on the arc (R31): 1.25 mm beside the middle of the beam, R = (500² + 1.25²)/(2 × 1.25) = 100000.625 mm. The horizontal
        // reaction of the clamp in each generated static case is the radial force.
        var onTheArc = new Point3d(500, -1.25, 0); double radius = (500d * 500 + 1.25 * 1.25) / (2 * 1.25);
        // Constant law: radial force = OffsetForce + Factor × |vertical force| = 50 N + 0.1 × 1000 N (a law read in kN would give 1000 times more).
        var constant = HorizontalReactions(new MovingCentrifugalLaw { FactorKind = MovingCentrifugalFactorKind.Constant, Factor = .1, OffsetForce = 50 }, onTheArc, MovingLoadAxes.Path, "constant");
        Assert.AreEqual(4, constant.Count, "loaded cases");
        foreach (double force in constant) Assert.AreEqual(150, force, 1e-6, "constant N-mm law");
        // V²/(g R) with V in mm/s, R in mm and g = 9806.65 mm/s² (a length read in m would give 1000 times less, in km 10⁶).
        double expected = 20000d * 20000 / (9806.65 * radius) * 1000;
        var speed = HorizontalReactions(new MovingCentrifugalLaw { FactorKind = MovingCentrifugalFactorKind.SpeedSquaredOverGravityRadius }, onTheArc, MovingLoadAxes.Path, "speed");
        Console.WriteLine($"L4 centrifugal V²/(gR): expected {expected:G8} N with R = {radius:G9} mm; R31 {string.Join(", ", speed.Select(f => f.ToString("G8")))}");
        Assert.AreEqual(4, speed.Count, "loaded cases");
        foreach (double force in speed) Assert.AreEqual(expected, force, 1e-5 * expected, "V²/(g R) N-mm law");
        // Observed on R31 (reported, not a GPC rule): a force in global axes gets no centrifugal force at all, even with ApplyCentrifugal.
        var global = HorizontalReactions(new MovingCentrifugalLaw { FactorKind = MovingCentrifugalFactorKind.Constant, Factor = .1, OffsetForce = 50 }, onTheArc, MovingLoadAxes.Global, "global");
        Console.WriteLine("L4 centrifugal of a force in global axes (R31): " + string.Join(", ", global.Select(f => f.ToString("G6"))) + " N");
    }

    private static Dictionary<string, string> Files(string folder, string stem) => Directory.GetFiles(folder, stem + ".*")
        .ToDictionary(f => Path.GetFileName(f)!, f => { using var sha = System.Security.Cryptography.SHA256.Create(); using var s = File.OpenRead(f); return Convert.ToBase64String(sha.ComputeHash(s)); });

    [TestMethod, TestCategory("Straus7Native")]
    public void L4_NativeRerunOntoAnExistingRunLeavesItsModelAndResultsAsTheyWere()
    {
        // Review L4 finding 8 (a failed re-run onto an existing path would leave a partly overwritten model next to the previous results),
        // checked natively: the existing model is refused before any native call, and a model file removed by hand next to kept results is
        // refused at the solve and removed again; the previous results are never touched and their run still imports them only with its model.
        var model = Straus7ModelWriteTest.Cantilever(); string folder = Straus7ExporterCorrectionsTest.NativeFolder(), path = Path.Combine(folder, "rerun.st7");
        var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, path, folder);
        var before = Files(folder, "rerun"); Assert.IsTrue(before.ContainsKey("rerun.st7") && before.Keys.Any(k => k.EndsWith(".lsa", StringComparison.OrdinalIgnoreCase)), string.Join(", ", before.Keys));
        var other = Straus7ModelWriteTest.Cantilever(); foreach (var node in other.NodesElements.Values) foreach (var load in node.Loads.Values.OfType<GPC.Model.Loads.PointLoad>()) load.F3 *= 2;
        var refused = Assert.ThrowsException<IOException>(() => Straus7ModelWritePlan.Prepare(other).RunLinearStatic(other, path, folder));
        Console.WriteLine("L4 re-run onto an existing model: " + refused.Message);
        CollectionAssert.AreEquivalent(before.ToList(), Files(folder, "rerun").ToList());
        File.Delete(path);
        var results = Files(folder, "rerun"); int created = 0;
        using (var watcher = new FileSystemWatcher(folder, "rerun.st7") { EnableRaisingEvents = true })
        {
            // St7NewFile writes nothing at the output path before St7SaveFile, and Solve refuses the existing results before saving.
            watcher.Created += (_, _) => Interlocked.Increment(ref created);
            refused = Assert.ThrowsException<IOException>(() => Straus7ModelWritePlan.Prepare(other).RunLinearStatic(other, path, folder));
            Thread.Sleep(300);
        }
        Console.WriteLine("L4 re-run next to kept results: " + refused.Message + "; model files created meanwhile: " + created);
        Assert.AreEqual(0, created, "no model file at any moment next to the kept results");
        CollectionAssert.AreEquivalent(results.ToList(), Files(folder, "rerun").ToList(), "no new model file, the results unchanged");
        Assert.AreEqual(GPC.Converter.ImportStatus.Rejected, run.ImportResults(model, "orphan results").Status);
    }
}
