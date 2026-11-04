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
}
