using GPC.Model.Standards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Model.Core.Tests;

[TestClass]
public class EmbeddedWallStandardsTests
{
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
