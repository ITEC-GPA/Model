using GPC.Model.Design.Combinations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UnitTest
{
    /// <summary>
    /// Seismic variants (NTC 2018 §7.3.5, (7.3.10): 1,00·Ex + 0,30·Ey + 0,30·Ez with the coefficients permuted and every sign). Expected
    /// variants written by hand, independent of the code.
    /// </summary>
    [TestClass]
    public class SeismicCombinationsTest
    {
        private static string Text(SeismicVariant v) => string.Join(" ", v.Factors.Select(f => (f.Value < 0 ? "-" : "+") + Math.Abs(f.Value).ToString("0.0#", System.Globalization.CultureInfo.InvariantCulture) + f.Key));

        [TestMethod]
        public void CasesWithoutDirection_AreAloneWithBothSigns()
        {
            var v = SeismicCombinations.Variants(new[] { new SeismicCase("E1"), new SeismicCase("E2") });
            CollectionAssert.AreEqual(new[] { "+1.0E1", "-1.0E1", "+1.0E2", "-1.0E2" }, v.Select(Text).ToArray());
        }

        [TestMethod]
        public void TwoDirections_EightVariants_EachLeadingOnce()
        {
            var v = SeismicCombinations.Variants(new[] { new SeismicCase("Ex", SeismicDirection.X), new SeismicCase("Ey", SeismicDirection.Y) });
            CollectionAssert.AreEqual(new[]
            {
                "+1.0Ex +0.3Ey", "+1.0Ex -0.3Ey", "-1.0Ex +0.3Ey", "-1.0Ex -0.3Ey",
                "+1.0Ey +0.3Ex", "+1.0Ey -0.3Ex", "-1.0Ey +0.3Ex", "-1.0Ey -0.3Ex",
            }, v.Select(Text).ToArray());
        }

        [TestMethod]
        public void ThreeDirections_TwentyFourVariants_AndCasesWithoutDirectionKeepTheirPlace()
        {
            var v = SeismicCombinations.Variants(new[] { new SeismicCase("Ez", SeismicDirection.Z), new SeismicCase("Eacc"),
                new SeismicCase("Ex", SeismicDirection.X), new SeismicCase("Ey", SeismicDirection.Y) });
            Assert.AreEqual(24 + 2, v.Count);
            CollectionAssert.AreEqual(new[] { "+1.0Ex +0.3Ey +0.3Ez", "+1.0Ex +0.3Ey -0.3Ez" }, v.Take(2).Select(Text).ToArray());
            CollectionAssert.AreEqual(new[] { "+1.0Ez +0.3Ex +0.3Ey", "-1.0Ez -0.3Ex -0.3Ey" }, new[] { v[16], v[23] }.Select(Text).ToArray());
            CollectionAssert.AreEqual(new[] { "+1.0Eacc", "-1.0Eacc" }, v.Skip(24).Select(Text).ToArray());
            // Every direction leads 8 times with factor ±1 and is a companion with ±0.3 in the other 16.
            foreach (var id in new[] { "Ex", "Ey", "Ez" })
            {
                var factors = v.Take(24).Select(x => x.Factors.Single(f => f.Key == id).Value).ToArray();
                Assert.AreEqual(8, factors.Count(f => Math.Abs(f) == 1)); Assert.AreEqual(16, factors.Count(f => Math.Abs(f) == .3));
                Assert.AreEqual(0, factors.Sum(), 1e-12);
            }
        }

        [TestMethod]
        public void CasesOfOneDirection_AreAlternatives()
        {
            var v = SeismicCombinations.Variants(new[] { new SeismicCase("Ex+", SeismicDirection.X), new SeismicCase("Ex-", SeismicDirection.X),
                new SeismicCase("Ey", SeismicDirection.Y) });
            Assert.AreEqual(2 * 2 * 4, v.Count);
            Assert.IsTrue(v.All(x => x.Factors.Count == 2 && x.Factors.Count(f => f.Key.StartsWith("Ex")) == 1));
            CollectionAssert.AreEqual(new[] { "+1.0Ex+ +0.3Ey", "+1.0Ex+ -0.3Ey", "-1.0Ex+ +0.3Ey", "-1.0Ex+ -0.3Ey", "+1.0Ex- +0.3Ey" },
                v.Take(5).Select(Text).ToArray());
        }

        [TestMethod]
        public void OneDirection_IsTheSameAsNoDirection_AndInputsAreChecked()
        {
            CollectionAssert.AreEqual(new[] { "+1.0Ex", "-1.0Ex" }, SeismicCombinations.Variants(new[] { new SeismicCase("Ex", SeismicDirection.X) }).Select(Text).ToArray());
            Assert.ThrowsException<ArgumentException>(() => SeismicCombinations.Variants(new[] { new SeismicCase("E"), new SeismicCase("E", SeismicDirection.X) }));
            Assert.ThrowsException<ArgumentException>(() => new SeismicCase(" "));
            Assert.AreEqual(0, SeismicCombinations.Variants(Array.Empty<SeismicCase>()).Count);
        }
    }
}
