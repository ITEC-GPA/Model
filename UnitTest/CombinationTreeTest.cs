using GPC.Converter;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Processing;
using GPC.Model.Results.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>
/// Linear combinations of envelopes evaluated without listing the alternatives (ANTHEA Models module, point 8 of 1/11/2026): the extreme of a
/// sum is the sum of the extremes of its terms, the governing alternative the choice of each envelope. Below the limit the same governing
/// alternatives and values as the expansion; above it the extremes by hand.
/// </summary>
[TestClass]
public class CombinationTreeTest
{
    private static CombinationTermRecord Case(string name, double factor) => new() { Name = name, IsStaticCase = true, Analysis = "ST", Factor = factor };
    private static CombinationTermRecord Nested(string id, double factor) => new() { Name = id, IsCombination = true, Analysis = "CB", Factor = factor };
    private static CombinationRecord Definition(string id, CombinationKind kind, params CombinationTermRecord[] terms) =>
        new() { Id = id, Name = id, Kind = kind, Source = "test", Terms = terms.ToList() };
    private static double[] V(PointResultPlateForces p) => new[] { p.Forces.Fxx, p.Forces.Fyy, p.Forces.Fxy, p.Forces.Fxz, p.Forces.Fyz, p.Forces.Mxx, p.Forces.Myy, p.Forces.Mxy };
    private static string[] Roles(ResultLocation s) { string t = s.State.Coverage; int end = t.IndexOf(" over the ", StringComparison.Ordinal); return t[8..end].Split(", "); }

    [TestMethod]
    public void BelowTheLimit_TheSameGoverningAlternativesAndValuesAsTheExpansion()
    {
        var m = CombinationRebuildTest.Imported(); var output = m.Combinations["ENV"];
        var definitions = new[]
        {
            Definition("E1", CombinationKind.Envelope, Case("Q", 1), Case("G1", 1)),
            Definition("E2", CombinationKind.Envelope, Case("Q", -0.5), Case("G1", 1.2)),
            Definition("SUM", CombinationKind.Linear, Nested("E1", 1), Nested("E2", 0.7), Case("G1", 0.3)),
            Definition("NEG", CombinationKind.Linear, Nested("SUM", -1), Nested("E1", 0.5)),
        };
        foreach (string id in new[] { "SUM", "NEG" })
        {
            var expanded = CombinationExpansion.Expand(definitions, id).Select(a => new CombinationAlternative(a.Label, a.Factors)).ToArray();
            var tree = CombinationExpansion.Tree(definitions, id);
            Assert.AreEqual(expanded.Length, (int)tree.AlternativeCount());
            var flat = CombinationRebuild.Rebuild(m, output, expanded, "static", m.AllElements, false, default, null);
            var byTree = CombinationRebuild.Rebuild(m, output, tree, "static", m.AllElements, false, default, null);
            Assert.IsFalse(flat.IsRejected || byTree.IsRejected, string.Join("; ", byTree.Diagnostics.Select(d => d.Message)));
            Assert.AreEqual(flat.Locations, byTree.Locations);
            // Each role at each location: the same governing alternative and the same complete state.
            var flatRoles = flat.Samples.SelectMany(s => Roles(s).Select(r => (Location: s.State.DerivedFrom[0], Role: r, Sample: s))).ToList();
            var treeRoles = byTree.Samples.SelectMany(s => Roles(s).Select(r => (Location: s.State.DerivedFrom[0], Role: r, Sample: s))).ToList();
            Assert.AreEqual(flatRoles.Count, treeRoles.Count);
            foreach (var f in flatRoles)
            {
                var t = treeRoles.Single(x => ResultAlgebra.SameLocation(x.Sample, f.Sample) && x.Role == f.Role && x.Sample.GetType() == f.Sample.GetType());
                Assert.AreEqual(f.Sample.State.ConcomitantStateId, t.Sample.State.ConcomitantStateId, id + " " + f.Role);
                Assert.AreEqual(f.Sample.State.Transformation, t.Sample.State.Transformation, id + " " + f.Role);
                CollectionAssert.AreEqual(ResultAlgebra.Values(f.Sample), ResultAlgebra.Values(t.Sample), id + " " + f.Role + ": the same sums of the static samples.");
            }
            StringAssert.Contains(byTree.Samples[0].State.Coverage, "term by term");
        }
    }

    [TestMethod]
    public void AboveTheLimit_TheExtremesByHand_WithTheGoverningChoices()
    {
        var m = CombinationRebuildTest.Imported(); var output = m.Combinations["ENV"];
        // E = envelope of 70 terms (i/10 × Q, −i/20 × G1 alternately), P = E + E: 4900 alternatives, over the limit of 4096 of the expansion.
        var terms = Enumerable.Range(1, 70).Select(i => i % 2 == 0 ? Case("Q", i / 10.0) : Case("G1", -i / 20.0)).ToArray();
        var definitions = new[] { Definition("E", CombinationKind.Envelope, terms), Definition("P", CombinationKind.Linear, Nested("E", 1), Nested("E", 1)) };
        Assert.ThrowsException<InvalidOperationException>(() => CombinationExpansion.Expand(definitions, "P", 4096));
        var tree = CombinationExpansion.Tree(definitions, "P");
        Assert.AreEqual(4900, (int)tree.AlternativeCount());
        var report = CombinationRebuild.Rebuild(m, output, tree, "static", m.AllElements, false, default, null);
        Assert.IsFalse(report.IsRejected, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        // One result point per plate in the fixture (its centre): the samples of a plate are those of its location.
        foreach (var plate in report.Samples.OfType<PointResultPlateForces>().GroupBy(s => m.AreaElements.Values.Single(a => a.Results.SelectMany(r => r.Results).Contains(s.State.DerivedFrom[0]))))
        {
            var owner = plate.Key;
            var g1 = V(ResultQueries.Samples<PointResultPlateForces>(owner, new ResultSelection { Dataset = "static", Case = "G1" }).Single());
            var q = V(ResultQueries.Samples<PointResultPlateForces>(owner, new ResultSelection { Dataset = "static", Case = "Q" }).Single());
            var single = terms.Select(t => (t.Name == "Q" ? q : g1).Select(v => t.Factor * v).ToArray()).ToArray();
            string[] names = { "Nxx", "Nyy", "Nxy", "Qx", "Qy", "Mxx", "Myy", "Mxy" };
            for (int k = 0; k < 8; k++)
            {
                double max = 2 * single.Max(s => s[k]), min = 2 * single.Min(s => s[k]);
                var high = plate.Single(s => Roles(s).Contains("max " + names[k])); var low = plate.Single(s => Roles(s).Contains("min " + names[k]));
                Assert.AreEqual(max, V(high)[k], 1e-9 * Math.Max(1, Math.Abs(max))); Assert.AreEqual(min, V(low)[k], 1e-9 * Math.Max(1, Math.Abs(min)));
                // The same term of E chosen twice: the label of the first maximiser, then its complete state.
                int first = Enumerable.Range(0, 70).First(i => single[i][k] == single.Max(s => s[k]));
                string term = terms[first].Name + (terms.Take(first).Count(t => t.Name == terms[first].Name) is int n and > 0 ? " #" + (n + 1) : "");
                Assert.AreEqual("envelope:ENV:E=" + term + "; E=" + term, high.State.ConcomitantStateId);
            }
        }
    }
}
