using GPC.Model.Results.Locations;
using GPC.Model.Results.Processing;
using GPC.Model.Results.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>
/// Principal components of the plates in the combinations (request of 12/10/2026, ANTHEA Models module, point 8): for each combined state the
/// local state first, then its principal values; for an envelope the principal values of every simultaneous state, with extremes and governing
/// states. Expected values by hand: the tensor formula on numbers chosen for round results, and on the static samples summed by hand.
/// </summary>
[TestClass]
public class PlatePrincipalsTest
{
    [TestMethod]
    public void Formula_ByHand_AndInvariantUnderRotation()
    {
        PlatePrincipals.Of(4, -2, 4, out double max, out double min); Assert.AreEqual(6, max, 1e-12); Assert.AreEqual(-4, min, 1e-12);
        PlatePrincipals.Of(3, 3, 0, out max, out min); Assert.AreEqual(3, max); Assert.AreEqual(3, min);
        PlatePrincipals.Of(0, 0, -2, out max, out min); Assert.AreEqual(2, max); Assert.AreEqual(-2, min);
        // The same tensor in axes rotated by 30°: xx' = c²xx + s²yy + 2cs xy, yy' = s²xx + c²yy − 2cs xy, xy' = cs(yy − xx) + (c² − s²) xy.
        double c = Math.Cos(Math.PI / 6), s = Math.Sin(Math.PI / 6);
        PlatePrincipals.Of(c * c * 4 + s * s * -2 + 2 * c * s * 4, s * s * 4 + c * c * -2 - 2 * c * s * 4, c * s * (-2 - 4) + (c * c - s * s) * 4, out max, out min);
        Assert.AreEqual(6, max, 1e-12); Assert.AreEqual(-4, min, 1e-12);
        CollectionAssert.AreEqual(new[] { "Nmax", "Nmin", "Mmax", "Mmin" }, PlatePrincipals.Components.ToArray());
    }

    private static double[] V(PointResultPlateForces p) => new[] { p.Forces.Fxx, p.Forces.Fyy, p.Forces.Fxy, p.Forces.Fxz, p.Forces.Fyz, p.Forces.Mxx, p.Forces.Myy, p.Forces.Mxy };
    private static double[] ByHand(double[] v)
    {
        double R(double xx, double yy, double xy) => Math.Sqrt((xx - yy) * (xx - yy) / 4 + xy * xy);
        return new[] { (v[0] + v[1]) / 2 + R(v[0], v[1], v[2]), (v[0] + v[1]) / 2 - R(v[0], v[1], v[2]), (v[5] + v[6]) / 2 + R(v[5], v[6], v[7]), (v[5] + v[6]) / 2 - R(v[5], v[6], v[7]) };
    }

    [TestMethod]
    public void Rebuild_GivesThePrincipalsOfEachSimultaneousState_AndTheSameSamples()
    {
        var m = CombinationRebuildTest.Imported(); var env = m.Combinations["ENV"];
        var alternatives = new[]
        {
            new CombinationAlternative("ENV=Q", new Dictionary<string, double> { ["Q"] = 1 }),
            new CombinationAlternative("ENV=SLU_1", new Dictionary<string, double> { ["G1"] = 1.35, ["Q"] = 1.5 }),
            new CombinationAlternative("ENV=-Q", new Dictionary<string, double> { ["Q"] = -0.5, ["G1"] = 1 })
        };
        string Canonical(CombinationRebuildReport r) => GPC.Model.Core.ModelValues.Fingerprint(r.Samples.Cast<object>());
        var plain = CombinationRebuild.Rebuild(m, env, alternatives, "static", m.AllElements, false);
        Assert.AreEqual(0, plain.PlatePrincipals.Count, "Only on request.");
        var cache = new CombinationRebuildCache { PlatePrincipals = true };
        var report = CombinationRebuild.Rebuild(m, env, alternatives, "static", m.AllElements, false, default, cache);
        Assert.IsFalse(report.IsRejected); Assert.AreEqual(Canonical(plain), Canonical(report), "The samples, their roles and states do not change.");
        var plates = m.AreaElements.Values.Where(a => a.Results.Any()).ToArray();
        Assert.AreEqual(plates.Length, report.PlatePrincipals.Count); Assert.IsTrue(plates.Length > 0);
        foreach (var result in report.PlatePrincipals)
        {
            var g1 = V(ResultQueries.Samples<PointResultPlateForces>(result.Owner, new ResultSelection { Dataset = "static", Case = "G1" }).Single());
            var q = V(ResultQueries.Samples<PointResultPlateForces>(result.Owner, new ResultSelection { Dataset = "static", Case = "Q" }).Single());
            var states = new[] { q, g1.Zip(q, (a, b) => 1.35 * a + 1.5 * b).ToArray(), g1.Zip(q, (a, b) => a - 0.5 * b).ToArray() };
            var principal = states.Select(ByHand).ToArray();
            string[] ids = { "envelope:ENV:ENV=Q", "envelope:ENV:ENV=SLU_1", "envelope:ENV:ENV=-Q" };
            Assert.AreEqual(4, result.Components.Count);
            for (int k = 0; k < 4; k++)
            {
                var e = result.Components[k]; Assert.AreEqual(PlatePrincipals.Components[k], e.Component);
                int low = Enumerable.Range(0, 3).OrderBy(a => principal[a][k]).First(), high = Enumerable.Range(0, 3).OrderByDescending(a => principal[a][k]).First();
                Assert.AreEqual(principal[low][k], e.Minimum, 1e-9 * Math.Max(1, Math.Abs(e.Minimum))); Assert.AreEqual(ids[low], e.MinimumStateId);
                Assert.AreEqual(principal[high][k], e.Maximum, 1e-9 * Math.Max(1, Math.Abs(e.Maximum))); Assert.AreEqual(ids[high], e.MaximumStateId);
                for (int c = 0; c < 8; c++) { Assert.AreEqual(states[high][c], e.MaximumState[c], 1e-9 * Math.Max(1, Math.Abs(states[high][c]))); }
                // Never the principal of the extremes of the local components, which are not simultaneous.
                Assert.IsTrue(principal.Any(p => Math.Abs(p[k] - e.Maximum) <= 1e-9 * Math.Max(1, Math.Abs(e.Maximum))));
            }
        }
        // A linear combination: the principals of the one combined state, minimum = maximum.
        var linear = CombinationRebuild.Rebuild(m, env, new[] { alternatives[1] }, "static", m.AllElements, false, default, cache);
        foreach (var result in linear.PlatePrincipals)
        {
            var g1 = V(ResultQueries.Samples<PointResultPlateForces>(result.Owner, new ResultSelection { Dataset = "static", Case = "G1" }).Single());
            var q = V(ResultQueries.Samples<PointResultPlateForces>(result.Owner, new ResultSelection { Dataset = "static", Case = "Q" }).Single());
            var expected = ByHand(g1.Zip(q, (a, b) => 1.35 * a + 1.5 * b).ToArray());
            for (int k = 0; k < 4; k++)
            {
                Assert.AreEqual(expected[k], result.Components[k].Maximum, 1e-9 * Math.Max(1, Math.Abs(expected[k])));
                Assert.AreEqual(result.Components[k].Minimum, result.Components[k].Maximum); Assert.AreEqual("linear:ENV", result.Components[k].MaximumStateId);
            }
        }
        Assert.AreEqual(linear.PlatePrincipals.Count, plates.Length);
    }
}
