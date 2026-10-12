using GPC.Model.Materials;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Cement = GPC.Model.Materials.ConcreteMaterialEuropeanCommon.CementTypes;

namespace ModelObjectTest;

[TestClass]
public class ConcreteTimeEffectsTests
{
    private static ConcreteTimeEffects Default(Cement cement = Cement.ClassN) => new(30, cement, 70, 560, 10);
    private static void Near(double expected, double actual, string context) =>
        Assert.AreEqual(expected, actual, Math.Max(1e-12, Math.Abs(expected) * 2e-13), context);

    [TestMethod]
    public void IndependentReferenceAcrossCementsStrengthBranchAndHumidity()
    {
        using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "concrete-time-effects.json")));
        int count = 0;
        foreach (var row in document.RootElement.GetProperty("rows").EnumerateArray())
        {
            double V(string key) => row.GetProperty(key).GetDouble();
            var cement = Enum.Parse<Cement>("Class" + row.GetProperty("cement").GetString());
            var curve = new ConcreteTimeEffects(V("fck"), cement, V("rh"), V("h0"), V("t0"));
            var point = curve.AtAge(V("age"));
            Near(V("adjusted"), curve.AdjustedLoadingAgeDays, "cement age");
            Near(V("betaH"), curve.BetaHDays, "betaH");
            Near(V("phiInfinity"), curve.CreepInfinity, "phi infinity");
            Near(V("e28"), curve.Ecm28Mpa, "E28");
            Near(V("phi"), point.CreepCoefficient, "phi(t)");
            Near(V("e"), point.EcmMpa, "E(t)");
            Near(V("factor"), point.ModulusFactor, "factor E");
            count++;
        }
        Assert.IsTrue(count >= 40, "Full independent reference corpus");
    }

    [TestMethod]
    public void LoadingOriginInfinityAndReferenceAgeAreExact()
    {
        foreach (Cement cement in Enum.GetValues<Cement>())
        {
            var curve = Default(cement);
            Assert.AreEqual(0, curve.AtAge(1).CreepCoefficient);
            Assert.AreEqual(0, curve.AtAge(curve.LoadingAgeDays).CreepCoefficient);
            Assert.AreEqual(curve.CreepInfinity, curve.AtAge(double.PositiveInfinity).CreepCoefficient);
            Assert.AreEqual(1, curve.AtAge(28).ModulusFactor);
            double previous = 0;
            foreach (var day in new[] { 10d, 11, 28, 100, 365, 10000, 1e10 })
            {
                var point = curve.AtAge(day);
                Assert.IsTrue(point.CreepCoefficient >= previous && point.CreepCoefficient <= curve.CreepInfinity);
                Assert.IsTrue(double.IsFinite(point.EcmMpa) && point.EcmMpa > 0);
                previous = point.CreepCoefficient;
            }
        }
    }

    [TestMethod]
    public void ModelModulusAndHistoricalClassNCreepAreReusedConsistently()
    {
        var curve = Default();
        var material = new ConcreteMaterialEN1992("C30", 30, ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.ParabolaRectangle);
        Near(material.GetEcm(material.Fcm, 7), curve.AtAge(7).EcmMpa, "Signed fcm convention");
        // Low stress, N cement, no temperature correction: the historical API returns strain.
        double strain = material.GetEpsilonCCInfiniteTime(-1, 70, 280000, 1000, 10);
        Near(curve.CreepInfinity, strain * material.Ec, "Historical phi-infinity at unit stress");
        Assert.AreEqual(560d, ConcreteTimeEffects.NotionalSize(280000, 1000));
    }

    [TestMethod]
    public void IndependentCalculationsRemainDeterministicUnderParallelUse()
    {
        var curve = Default(Cement.ClassR);
        var expected = curve.AtAge(365);
        Parallel.For(0, 100, _ =>
        {
            Assert.AreEqual(expected.CreepCoefficient, curve.AtAge(365).CreepCoefficient);
            Assert.AreEqual(expected.EcmMpa, curve.AtAge(365).EcmMpa);
        });
    }

    [TestMethod]
    public void HistoricalStrainApiKeepsEveryCapturedBit()
    {
        int count = 0;
        foreach (string row in File.ReadLines(Path.Combine(AppContext.BaseDirectory, "Fixtures", "creep-historical.csv")).Skip(1))
        {
            var cells = row.Split(';');
            double V(int column) => double.Parse(cells[column], System.Globalization.CultureInfo.InvariantCulture);
            var material = new ConcreteMaterialEN1992("Compatibility", V(0), ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.ParabolaRectangle,
                cementType: Enum.Parse<Cement>(cells[1]));
            double strain = material.GetEpsilonCCInfiniteTime(V(4), V(2), 280000, 1000, V(3), V(5), 7);
            Assert.AreEqual(cells[6], BitConverter.DoubleToInt64Bits(strain).ToString("X16"), row);
            count++;
        }
        Assert.AreEqual(648, count);
    }

    [TestMethod]
    public void InvalidInputIsRejectedAtTheDomainBoundary()
    {
        foreach (double bad in new[] { double.NaN, double.NegativeInfinity, double.PositiveInfinity, 0, -1 })
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTimeEffects(bad, Cement.ClassN, 70, 560, 10));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTimeEffects(30, Cement.ClassN, bad, 560, 10));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTimeEffects(30, Cement.ClassN, 70, bad, 10));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTimeEffects(30, Cement.ClassN, 70, 560, bad));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ConcreteTimeEffects.NotionalSize(bad, 1000));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => ConcreteTimeEffects.NotionalSize(1000, bad));
            if (!double.IsPositiveInfinity(bad)) Assert.ThrowsException<ArgumentOutOfRangeException>(() => Default().AtAge(bad));
        }
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTimeEffects(30, (Cement)100, 70, 560, 10));
        foreach (double bad in new[] { 11.99, 90.01 }) Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTimeEffects(bad, Cement.ClassN, 70, 560, 10));
        foreach (double bad in new[] { 39.99, 100.01 }) Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTimeEffects(30, Cement.ClassN, bad, 560, 10));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTimeEffects(30, Cement.ClassN, 70, 560, .49));
    }
}
