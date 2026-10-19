using GPC.Model.Standards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Model.Core.Tests;

[TestClass]
public class EmbeddedWallStandardsTests
{
    [TestMethod]
    public void NtcHydraulicsUsesDedicatedCriteriaAndReportsUnsupportedSituations()
    {
        var ntc = new StandardNTC2018Geotechnics(); var p = ntc.HydraulicProfile(GeotechnicalSituation.PersistentTransient);
        Assert.AreEqual("IT", p.Country); Assert.AreEqual("2018", p.Edition);
        Assert.AreEqual(1.1, p.UpliftActions.PermanentUnfavourable); Assert.AreEqual(.9, p.UpliftActions.PermanentFavourable);
        Assert.AreEqual(1.5, p.UpliftActions.NonStructuralUnfavourable); Assert.AreEqual(.8, p.UpliftActions.NonStructuralFavourable);
        Assert.AreEqual(1.5, p.UpliftActions.VariableUnfavourable); Assert.AreEqual(0, p.UpliftActions.VariableFavourable);
        Assert.AreEqual(3, p.MeanGradientFactor); Assert.AreEqual(2, p.ExitGradientFactor); Assert.AreEqual(2, p.LoadedExitFactor);
        Assert.AreEqual(1.25, p.UpliftResistanceMaterials.TanFrictionAngle); Assert.AreEqual(1.4, p.UpliftResistanceMaterials.UndrainedShearStrength);
        Assert.IsNull(ntc.HydraulicProfile(GeotechnicalSituation.Seismic)); Assert.IsNull(new StandardEN1997p1().HydraulicProfile(GeotechnicalSituation.PersistentTransient));
    }
    [TestMethod]
    public void NtcEmbeddedWallsUseBothR1CombinationsAndDoNotReuseGravityWallFactors()
    {
        var ntc = new StandardNTC2018Geotechnics();
        var rows = ntc.Combinations(GeotechnicalCheck.EmbeddedWallResistance, GeotechnicalSituation.PersistentTransient);
        CollectionAssert.AreEqual(new[] { "A1+M1+R1", "A2+M2+R1" }, rows.Select(r => r.Name).ToArray());
        Assert.IsTrue(rows.All(r => r.ResistanceFactor == 1));
        Assert.AreEqual(1.25, rows[1].Materials.TanFrictionAngle);
        var gravity = ntc.Combinations(GeotechnicalCheck.RetainingWallPassiveResistance, GeotechnicalSituation.PersistentTransient);
        Assert.AreEqual(1.4, gravity.Single().ResistanceFactor);
        Assert.AreEqual(0, ntc.Combinations(GeotechnicalCheck.EmbeddedWallResistance, GeotechnicalSituation.Seismic).Count,
            "Seismic factors must not silently fall back to retaining-wall coefficients.");
    }
}
