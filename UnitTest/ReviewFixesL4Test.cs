using GPC.Converter.Straus7;
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
