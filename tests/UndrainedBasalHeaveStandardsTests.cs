using GPC.Model.Standards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Model.Core.Tests;

[TestClass]
public class UndrainedBasalHeaveStandardsTests
{
    [TestMethod]
    public void BasalFailureHasItsOwnIdentityAndBothNtcCombinations()
    {
        var standard = new StandardNTC2018Geotechnics();
        var rows = standard.Combinations(GeotechnicalCheck.UndrainedBasalHeave, GeotechnicalSituation.PersistentTransient);
        CollectionAssert.AreEqual(new[] { "A1+M1+R1", "A2+M2+R1" }, rows.Select(r => r.Name).ToArray());
        Assert.IsTrue(rows.All(r => r.Check == GeotechnicalCheck.UndrainedBasalHeave && r.ResistanceFactor == 1));
        Assert.AreEqual(1.3, rows[0].Actions.PermanentUnfavourable);
        Assert.AreEqual(1.5, rows[0].Actions.VariableUnfavourable);
        Assert.AreEqual(1, rows[0].Materials.UndrainedShearStrength);
        Assert.AreEqual(1, rows[1].Actions.PermanentUnfavourable);
        Assert.AreEqual(1.3, rows[1].Actions.VariableUnfavourable);
        Assert.AreEqual(1.4, rows[1].Materials.UndrainedShearStrength);
        Assert.AreEqual(0, standard.Combinations(GeotechnicalCheck.UndrainedBasalHeave, GeotechnicalSituation.Seismic).Count);
        Assert.AreEqual(0, new StandardEN1997p1().Combinations(GeotechnicalCheck.UndrainedBasalHeave, GeotechnicalSituation.PersistentTransient).Count);
    }
}
