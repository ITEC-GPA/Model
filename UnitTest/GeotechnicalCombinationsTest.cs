using GPC.Model.Design.Combinations;
using GPC.Model.Standards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTest
{
    /// <summary>
    /// Generator of the geotechnical combinations (NTC 2018 §2.5.3; A1 of Tab. 6.2.I: G1 1.3/1.0, G2 1.5/0.8, Q 1.5/0). Expected rows written by
    /// hand from the formulas of the standard, independent of the code: SLE characteristic Gk + Qk1 + Σ ψ0i Qki, frequent Gk + ψ11 Qk1 + Σ ψ2i Qki,
    /// quasi permanent Gk + Σ ψ2i Qki, SLU γG Gk + γQ Qk1 + Σ γQ ψ0i Qki, seismic E + Gk + Σ ψ2i Qki, accidental Ad + Gk + Σ ψ2i Qki.
    /// </summary>
    [TestClass]
    public class GeotechnicalCombinationsTest
    {
        private static readonly GeotechnicalActionFactors A1 = new StandardNTC2018Geotechnics()
            .Combinations(GeotechnicalCheck.RetainingWallSliding, GeotechnicalSituation.PersistentTransient).Single().Actions;
        private static GeotechnicalDesignAction Q(string id, double psi0 = .7, double psi1 = .5, double psi2 = .3, string group = null)
            => new GeotechnicalDesignAction(id, group ?? id, GeotechnicalActionNature.Q, psi0, psi1, psi2);

        // One variable action, two bodies: 2 SLS + 1 quasi permanent + 2³ ULS = 11 rows, numbered 1 to 11 in the order of the bits.
        [TestMethod]
        public void OneVariableActionGivesElevenRowsInTheOrderOfTheBits()
        {
            var rows = GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(A1, new[] { Q("q") }, new[] { "wall", "soil" }));
            Assert.AreEqual(11, rows.Count);
            CollectionAssert.AreEqual(Enumerable.Range(1, 11).ToArray(), rows.Select(r => r.Number).ToArray());
            var states = new[] { GeotechnicalLimitState.Characteristic, GeotechnicalLimitState.Frequent, GeotechnicalLimitState.QuasiPermanent }
                .Concat(Enumerable.Repeat(GeotechnicalLimitState.Ultimate, 8)).ToArray();
            CollectionAssert.AreEqual(states, rows.Select(r => r.State).ToArray());
            CollectionAssert.AreEqual(new[] { 1, .5, .3 }, rows.Take(3).Select(r => r.Factors["q"]).ToArray(), "Qk, ψ1 Qk, ψ2 Qk");
            Assert.AreEqual("q", rows[0].Lead); Assert.AreEqual("q", rows[1].Lead); Assert.IsNull(rows[2].Lead);
            Assert.IsTrue(rows.Take(3).All(r => r.BodyFactors.SequenceEqual(new[] { 1d, 1d })), "bodies at 1 outside the ultimate rows");
            // Bit 0 the wall, bit 1 the soil, bit 2 the presence of q.
            var expected = new[] { (1.0, 1.0, 0.0), (1.3, 1.0, 0.0), (1.0, 1.3, 0.0), (1.3, 1.3, 0.0), (1.0, 1.0, 1.5), (1.3, 1.0, 1.5), (1.0, 1.3, 1.5), (1.3, 1.3, 1.5) };
            var ultimate = rows.Skip(3).ToArray();
            for (int i = 0; i < 8; i++)
            {
                Assert.AreEqual(expected[i].Item1, ultimate[i].BodyFactors[0], "wall " + i); Assert.AreEqual(expected[i].Item2, ultimate[i].BodyFactors[1], "soil " + i);
                Assert.AreEqual(expected[i].Item3, ultimate[i].Factors["q"], "q " + i);
                Assert.AreEqual(expected[i].Item3 > 0 ? "q" : null, ultimate[i].Lead);
            }
        }

        // Two variable groups: for every body pattern one row without Q, one with each alone and two with both (each one leading): 4 · 5 = 20.
        [TestMethod]
        public void EveryPresentVariableGroupLeadsInTurn()
        {
            var rows = GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(A1, new[] { Q("q", .6, .5, .2), Q("q2", .8, .7, .6) }, new[] { "wall", "soil" }));
            var service = rows.Where(r => r.State != GeotechnicalLimitState.Ultimate).ToArray();
            Assert.AreEqual(5, service.Length, "2 characteristic, 2 frequent, 1 quasi permanent");
            Assert.AreEqual(1, service[0].Factors["q"]); Assert.AreEqual(.8, service[0].Factors["q2"], "ψ0 of the accompanying action");
            Assert.AreEqual(.6, service[1].Factors["q"]); Assert.AreEqual(1, service[1].Factors["q2"]);
            Assert.AreEqual(.5, service[2].Factors["q"], "ψ1 of the leading action"); Assert.AreEqual(.6, service[2].Factors["q2"], "ψ2 of the other");
            Assert.AreEqual(.2, service[3].Factors["q"]); Assert.AreEqual(.7, service[3].Factors["q2"]);
            Assert.AreEqual(.2, service[4].Factors["q"]); Assert.AreEqual(.6, service[4].Factors["q2"]);
            var ultimate = rows.Where(r => r.State == GeotechnicalLimitState.Ultimate).ToArray();
            Assert.AreEqual(20, ultimate.Length);
            // Pattern wall 1, soil 1, both present: q leading (1.5, 1.5·0.8) then q2 leading (1.5·0.6, 1.5).
            var both = ultimate.Where(r => r.BodyFactors.SequenceEqual(new[] { 1d, 1d }) && r.Factors["q"] > 0 && r.Factors["q2"] > 0).ToArray();
            Assert.AreEqual(2, both.Length);
            Assert.AreEqual("q", both[0].Lead); Assert.AreEqual(1.5, both[0].Factors["q"]); Assert.AreEqual(1.2, both[0].Factors["q2"], 1e-15);
            Assert.AreEqual("q2", both[1].Lead); Assert.AreEqual(.9, both[1].Factors["q"], 1e-15); Assert.AreEqual(1.5, both[1].Factors["q2"]);
        }

        // Permanent groups: G1 1.0/1.3 and G2 0.8/1.5 of A1, one bit each, after the bodies; a body-less request still enumerates them.
        [TestMethod]
        public void PermanentGroupsTakeTheFactorsOfTheSet()
        {
            var actions = new[] { new GeotechnicalDesignAction("g1", "g1", GeotechnicalActionNature.G1), new GeotechnicalDesignAction("g2", "g2", GeotechnicalActionNature.G2) };
            var rows = GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(A1, actions));
            Assert.AreEqual(3 + 4, rows.Count, "one characteristic and one frequent row without lead, quasi permanent, 2² ULS");
            Assert.IsTrue(rows.Take(3).All(r => r.Factors["g1"] == 1 && r.Factors["g2"] == 1 && r.Lead == null));
            var ultimate = rows.Skip(3).ToArray();
            CollectionAssert.AreEqual(new[] { 1, 1.3, 1, 1.3 }, ultimate.Select(r => r.Factors["g1"]).ToArray());
            CollectionAssert.AreEqual(new[] { .8, .8, 1.5, 1.5 }, ultimate.Select(r => r.Factors["g2"]).ToArray());
            Assert.IsTrue(ultimate.All(r => r.BodyFactors.Count == 0));
            // A caller set that departs from the standard (G2 favourable absent) is used as given.
            var absent = new GeotechnicalActionFactors("A1 G2 favourable absent", A1.PermanentUnfavourable, A1.PermanentFavourable, A1.NonStructuralUnfavourable, 0,
                A1.VariableUnfavourable, A1.VariableFavourable);
            CollectionAssert.AreEqual(new[] { 0, 0, 1.5, 1.5 }, GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(absent, actions))
                .Where(r => r.State == GeotechnicalLimitState.Ultimate).Select(r => r.Factors["g2"]).ToArray());
        }

        // Correlated actions: one group, one presence bit and one lead, each action with its own ψ0 when accompanying.
        [TestMethod]
        public void ActionsOfAGroupArePresentTogether()
        {
            var rows = GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(A1, new[] { Q("a", group: "G"), Q("b", group: "G") }, new[] { "wall" }));
            Assert.AreEqual(2 + 1 + 4, rows.Count);
            Assert.AreEqual("G", rows[0].Lead);
            Assert.IsTrue(rows.Where(r => r.State == GeotechnicalLimitState.Ultimate).All(r => r.Factors["a"] == r.Factors["b"]));
            Assert.ThrowsException<ArgumentException>(() => new GeotechnicalCombinationRequest(A1,
                new[] { Q("a", group: "G"), new GeotechnicalDesignAction("b", "G", GeotechnicalActionNature.G2) }), "one nature per group");
        }

        // Row bodies double every ultimate row, favourable first; seismic and accidental rows keep every body at 1. With one seismic or exceptional
        // row each (variablePresence false: the rule before the census D6, kept for fixed older rules; the default is in the next test).
        [TestMethod]
        public void RowBodiesDoubleTheUltimateRowsOnly()
        {
            var actions = new[] { Q("q"), new GeotechnicalDesignAction("u", "u", GeotechnicalActionNature.A) };
            var rows = GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(A1, actions, new[] { "wall" }, new[] { "valley" }, 2, false, null));
            var ultimate = rows.Where(r => r.State == GeotechnicalLimitState.Ultimate).ToArray();
            Assert.AreEqual(2 * 2 * 2, ultimate.Length);
            for (int i = 0; i < ultimate.Length; i += 2)
            {
                Assert.AreEqual(1, ultimate[i].BodyFactors[1]); Assert.AreEqual(1.3, ultimate[i + 1].BodyFactors[1]);
                Assert.AreEqual(ultimate[i].BodyFactors[0], ultimate[i + 1].BodyFactors[0]); Assert.AreEqual(ultimate[i].Number + 1, ultimate[i + 1].Number);
            }
            var seismic = rows.Where(r => r.State == GeotechnicalLimitState.Seismic).ToArray();
            CollectionAssert.AreEqual(new[] { 0, 1 }, seismic.Select(r => r.SeismicVariant).ToArray());
            Assert.IsTrue(seismic.All(r => r.BodyFactors.SequenceEqual(new[] { 1d, 1d }) && r.Factors["q"] == .3 && r.Factors["u"] == 0), "E + Gk + ψ2 Qk");
            var accidental = rows.Single(r => r.State == GeotechnicalLimitState.Exceptional);
            Assert.AreEqual("u", accidental.AccidentalGroup); Assert.AreEqual(1, accidental.Factors["u"]); Assert.AreEqual(.3, accidental.Factors["q"]);
            Assert.IsTrue(accidental.BodyFactors.SequenceEqual(new[] { 1d, 1d }));
            Assert.IsTrue(rows.Where(r => r.State != GeotechnicalLimitState.Exceptional).All(r => r.Factors["u"] == 0), "accidental actions only in their rows");
            Assert.AreEqual(rows.Count, rows.Last().Number, "one counter over all the rows");
        }

        // Census D6 a (decision of 28/10/2026): ψ2 Qk only where unfavourable in the seismic and exceptional rows (NTC 2018 §2.5.3), so every variant
        // and every A group has one row for every presence pattern of the Q groups: first all present, then q absent, q2 absent, both absent. z has
        // ψ2 = 0: its absence gives the same factors and is merged without a number. Expected rows written by hand.
        [TestMethod]
        public void SeismicAndExceptionalRowsEnumerateThePresenceOfTheVariableGroups()
        {
            var actions = new[] { Q("q", .7, .5, .3), Q("q2", .8, .7, .6), Q("z", 0, 0, 0), new GeotechnicalDesignAction("u", "u", GeotechnicalActionNature.A) };
            var rows = GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(A1, actions, new[] { "wall" }, seismicVariants: 2));
            var expected = new[] { (.3, .6), (0, .6), (.3, 0), (0d, 0d) };
            var seismic = rows.Where(r => r.State == GeotechnicalLimitState.Seismic).ToArray();
            Assert.AreEqual(8, seismic.Length, "2 variants × 4 presence patterns");
            for (int i = 0; i < 8; i++)
            {
                Assert.AreEqual(i / 4, seismic[i].SeismicVariant, "variant " + i);
                Assert.AreEqual(expected[i % 4].Item1, seismic[i].Factors["q"], "q " + i); Assert.AreEqual(expected[i % 4].Item2, seismic[i].Factors["q2"], "q2 " + i);
                Assert.AreEqual(0, seismic[i].Factors["z"]); Assert.AreEqual(0, seismic[i].Factors["u"]);
                Assert.IsTrue(seismic[i].BodyFactors.SequenceEqual(new[] { 1d }) && seismic[i].Lead == null);
            }
            var exceptional = rows.Where(r => r.State == GeotechnicalLimitState.Exceptional).ToArray();
            Assert.AreEqual(4, exceptional.Length, "one A group × 4 presence patterns");
            for (int i = 0; i < 4; i++)
            {
                Assert.AreEqual("u", exceptional[i].AccidentalGroup); Assert.AreEqual(1, exceptional[i].Factors["u"]);
                Assert.AreEqual(expected[i].Item1, exceptional[i].Factors["q"]); Assert.AreEqual(expected[i].Item2, exceptional[i].Factors["q2"]);
            }
            CollectionAssert.AreEqual(Enumerable.Range(1, rows.Count).ToArray(), rows.Select(r => r.Number).ToArray(), "merged rows use no number");
            // The fixed rule: one seismic row for every variant and one exceptional row, every Q at ψ2.
            var single = GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(A1, actions, new[] { "wall" }, null, 2, false, null));
            Assert.AreEqual(2, single.Count(r => r.State == GeotechnicalLimitState.Seismic)); Assert.AreEqual(1, single.Count(r => r.State == GeotechnicalLimitState.Exceptional));
            Assert.IsTrue(single.Where(r => r.State == GeotechnicalLimitState.Seismic || r.State == GeotechnicalLimitState.Exceptional).All(r => r.Factors["q"] == .3 && r.Factors["q2"] == .6));
            Assert.IsTrue(single.Where(r => r.State == GeotechnicalLimitState.Ultimate).Select(r => r.Number)
                .SequenceEqual(rows.Where(r => r.State == GeotechnicalLimitState.Ultimate).Select(r => r.Number)), "the rows before the seismic ones keep their numbers");
        }

        // Census D7 a: the fixed cases of the version 1 walls of ANTHEA (LegacyCombinations) enumerate the wall (slowest), the fill, the presence of
        // the loads and the water (fastest). With the declared order the generator gives them in that order; expected rows by hand from those loops.
        [TestMethod]
        public void TheDeclaredPatternOrderSetsTheBitsOfTheUltimateRows()
        {
            var loads = new[] { "q", "hq", "nq" }.Select(id => new GeotechnicalDesignAction(id, "legacy", GeotechnicalActionNature.Q, 1, .5, .2)).ToArray();
            var rows = GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(A1, loads, new[] { "wall", "fill", "water" }, null, 2, false,
                new[] { "water", "legacy", "fill", "wall" }));
            Assert.AreEqual(3 + 16 + 2, rows.Count);
            CollectionAssert.AreEqual(new[] { 1, .5, .2 }, rows.Take(3).Select(r => r.Factors["hq"]).ToArray(), "characteristic, frequent ψ1, quasi permanent ψ2");
            var ultimate = rows.Where(r => r.State == GeotechnicalLimitState.Ultimate).ToArray(); int n = 0;
            foreach (double wall in new[] { 1, 1.3 }) foreach (double fill in new[] { 1, 1.3 }) foreach (double q in new[] { 0, 1.5 }) foreach (double water in new[] { 1, 1.3 })
            {
                var r = ultimate[n++];
                CollectionAssert.AreEqual(new[] { wall, fill, water }, r.BodyFactors.ToArray(), "bodies of row " + n);
                Assert.IsTrue(loads.All(a => r.Factors[a.Id] == q), "loads of row " + n);
            }
            Assert.IsTrue(rows.Skip(19).All(r => r.State == GeotechnicalLimitState.Seismic && r.Factors["q"] == .2), "one seismic row for every variant, ψ2");
            // The default order puts the bodies first: the same rows in another order.
            var plain = GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(A1, loads, new[] { "wall", "fill", "water" }, null, 2, false, null))
                .Where(r => r.State == GeotechnicalLimitState.Ultimate).ToArray();
            Assert.AreEqual(1.3, plain[1].BodyFactors[0], "default: the wall is the fastest bit");
            CollectionAssert.AreEquivalent(ultimate.Select(Key).ToArray(), plain.Select(Key).ToArray());
            string Key(GeotechnicalCombinationRow r) => string.Join(",", r.BodyFactors) + "/" + r.Factors["q"];
        }

        [TestMethod]
        public void AnInvalidPatternOrderIsRefused()
        {
            var actions = new[] { Q("q"), new GeotechnicalDesignAction("g", "g", GeotechnicalActionNature.G1), new GeotechnicalDesignAction("u", "u", GeotechnicalActionNature.A) };
            GeotechnicalCombinationRequest Request(params string[] order) => new GeotechnicalCombinationRequest(A1, actions, new[] { "wall" }, null, 0, true, order);
            Assert.AreEqual(3, Request("q", "g", "wall").PatternOrder.Count);
            Assert.ThrowsException<ArgumentException>(() => Request("q", "wall"), "missing group");
            Assert.ThrowsException<ArgumentException>(() => Request("q", "g", "wall", "u"), "accidental groups are not in the pattern");
            Assert.ThrowsException<ArgumentException>(() => Request("q", "q", "wall"), "duplicate");
            Assert.ThrowsException<ArgumentException>(() => new GeotechnicalCombinationRequest(A1, actions, new[] { "q" }, null, 0, true, new[] { "q", "g" }), "a body named as a group");
            Assert.AreEqual(0, new GeotechnicalCombinationRequest(A1, actions, new[] { "q" }).PatternOrder.Count, "without an order the names may coincide");
        }

        // Identical rows are merged without using a number: with the seismic set (all factors 1) the body patterns give the same rows.
        [TestMethod]
        public void IdenticalRowsAreMergedWithoutUsingANumber()
        {
            var seismic = new StandardNTC2018Geotechnics().Combinations(GeotechnicalCheck.RetainingWallSliding, GeotechnicalSituation.Seismic).Single().Actions;
            var rows = GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(seismic, new[] { Q("q") }, new[] { "wall", "soil" }));
            var ultimate = rows.Where(r => r.State == GeotechnicalLimitState.Ultimate).ToArray();
            Assert.AreEqual(2, ultimate.Length, "q absent and q present: the 4 body patterns coincide");
            CollectionAssert.AreEqual(new[] { 4, 5 }, ultimate.Select(r => r.Number).ToArray());
            CollectionAssert.AreEqual(new[] { 0, 1d }, ultimate.Select(r => r.Factors["q"]).ToArray());
        }

        [TestMethod]
        public void InvalidRequestsAreRefused()
        {
            Assert.ThrowsException<ArgumentNullException>(() => GeotechnicalCombinations.Generate(null));
            Assert.ThrowsException<ArgumentNullException>(() => new GeotechnicalCombinationRequest(null, new[] { Q("q") }));
            Assert.ThrowsException<ArgumentException>(() => new GeotechnicalCombinationRequest(A1, new[] { Q("q"), Q("q") }), "duplicate id");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Q("q", .5, .6, .3), "ψ1 > ψ0");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Q("q", 1.1, .5, .3), "ψ0 > 1");
            Assert.ThrowsException<ArgumentException>(() => new GeotechnicalDesignAction("q", " ", GeotechnicalActionNature.Q));
            Assert.ThrowsException<ArgumentException>(() => new GeotechnicalCombinationRequest(A1, new[] { Q("q") }, new[] { "" }));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new GeotechnicalCombinationRequest(A1, new[] { Q("q") }, seismicVariants: -1));
            var many = Enumerable.Range(0, 31).Select(i => Q("q" + i)).ToArray();
            Assert.ThrowsException<ArgumentException>(() => GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(A1, many)));
        }
    }
}
