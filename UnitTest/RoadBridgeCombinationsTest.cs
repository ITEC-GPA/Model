using GPC.Model.Design.Combinations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace UnitTest
{
    /// <summary>
    /// Road bridges, NTC 2018 §5.1.3.12 (D.M. 17/01/2018, S.O. n. 8 alla G.U. n. 42 del 20/02/2018). Expected values written from Tab. 5.1.V and
    /// Tab. 5.1.VI of the text, and rows from the formulas of §2.5.3, independent of the code.
    /// </summary>
    [TestClass]
    public class RoadBridgeCombinationsTest
    {
        private static double[] Values(RoadBridgeActionFactors s) => new[] { s.StructuralUnfavourable, s.StructuralFavourable, s.NonStructuralUnfavourable,
            s.NonStructuralFavourable, s.TrafficUnfavourable, s.TrafficFavourable, s.VariableUnfavourable, s.VariableFavourable };

        // Tab. 5.1.V: g1 and g3 favourable/unfavourable 0.90/1.10 (EQU), 1.00/1.35 (A1), 1.00/1.00 (A2); g2 0.00/1.50, 0.00/1.50, 0.00/1.30;
        // traffic 0.00/1.35, 0.00/1.35, 0.00/1.15; other variable actions 0.00/1.50, 0.00/1.50, 0.00/1.30.
        [TestMethod]
        public void ActionSetsAreThoseOfTable51V()
        {
            CollectionAssert.AreEqual(new[] { 1.10, .90, 1.50, 0, 1.35, 0, 1.50, 0 }, Values(RoadBridgeCombinations.Equilibrium));
            CollectionAssert.AreEqual(new[] { 1.35, 1.00, 1.50, 0, 1.35, 0, 1.50, 0 }, Values(RoadBridgeCombinations.Structural));
            CollectionAssert.AreEqual(new[] { 1.00, 1.00, 1.30, 0, 1.15, 0, 1.30, 0 }, Values(RoadBridgeCombinations.Geotechnical));
        }

        // Tab. 5.1.VI: Schema 1 tandem 0.75/0.75/0.0; Schemes 1, 5 and 6 distributed 0.40/0.40/0.0; Schemes 3 and 4 concentrated 0.40/0.40/0.0.
        [TestMethod]
        public void TrafficPsiAreThoseOfTable51VI()
        {
            CollectionAssert.AreEqual(new[] { .75, .75, 0 }, RoadBridgeCombinations.TrafficPsi(RoadTrafficPsiGroup.Schema1Tandem));
            CollectionAssert.AreEqual(new[] { .40, .40, 0 }, RoadBridgeCombinations.TrafficPsi(RoadTrafficPsiGroup.Schemes156Distributed));
            CollectionAssert.AreEqual(new[] { .40, .40, 0 }, RoadBridgeCombinations.TrafficPsi(RoadTrafficPsiGroup.Schemes34Concentrated));
        }

        // A deck with G1, G2 and one traffic group, set A1: characteristic G1 + G2 + Qk, frequent G1 + G2 + ψ1 Qk (ψ1 = 0.75 of the tandem),
        // quasi permanent G1 + G2 (ψ2 = 0); ultimate γG1 G1 + γG2 G2 + γQ Qk for every pattern: γG1 1.35 or 1.00, γG2 1.50 or 0, traffic 1.35 or
        // absent: 2 x 2 x 2 = 8 rows, all different.
        [TestMethod]
        public void DeckRowsOfA1()
        {
            var rows = RoadBridgeCombinations.Deck(RoadBridgeCombinations.Structural);
            string Key(GeotechnicalCombinationRow r) => string.Join("/", new[] { "g1", "g2", "traffic" }.Select(id => r.Factors[id].ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)));
            CollectionAssert.AreEqual(new[] { "1/1/1" }, rows.Where(r => r.State == GeotechnicalLimitState.Characteristic).Select(Key).ToArray());
            CollectionAssert.AreEqual(new[] { "1/1/0.75" }, rows.Where(r => r.State == GeotechnicalLimitState.Frequent).Select(Key).ToArray());
            CollectionAssert.AreEqual(new[] { "1/1/0" }, rows.Where(r => r.State == GeotechnicalLimitState.QuasiPermanent).Select(Key).ToArray());
            var expected = (from g1 in new[] { "1", "1.35" } from g2 in new[] { "0", "1.5" } from q in new[] { "0", "1.35" } select g1 + "/" + g2 + "/" + q).ToArray();
            CollectionAssert.AreEquivalent(expected, rows.Where(r => r.State == GeotechnicalLimitState.Ultimate).Select(Key).ToArray());
        }
    }
}
