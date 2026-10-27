using GPC.Model.Materials;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest
{
    /// <summary>
    /// Tension chord (tension stiffening on the bars, BUG-TS-2 option 1). Hand values with Ec = 31000 MPa, fct = 2.6 MPa, Es = 200000 MPa, β = 0.4,
    /// ρeff = 0.02: εcr = 2.6 / 31000 = 8.3871e-5; αe ρ = 200000 / 31000 · 0.02 = 0.129032; σsr = 2.6 · 1.129032 / 0.02 = 146.7742 MPa;
    /// εsr = 7.33871e-4; end of the crack formation (1 − β) εsr = 4.40323e-4.
    /// </summary>
    [TestClass]
    public class ConcreteTensionChordTest
    {
        private static readonly ConcreteTensionChord Law = new ConcreteTensionChord(31000, 2.6, 200000);

        [TestMethod]
        public void ConcreteCarriesTensionOnlyUpToCracking()
        {
            Assert.AreEqual(2.6 / 31000, Law.CrackingStrain, 1e-18);
            // Uncracked state: linear; cracked state: zero; compression: the diagram of the material
            Law.EvaluateConcrete(5e-5, false, out double stress, out double tangent);
            Assert.AreEqual(1.55, stress, 1e-12); Assert.AreEqual(31000, tangent);
            Law.EvaluateConcrete(5e-5, true, out stress, out tangent);
            Assert.AreEqual(0, stress); Assert.AreEqual(0, tangent);
            Law.EvaluateConcrete(1e-4, true, out stress, out tangent);
            Assert.AreEqual(0, stress); Assert.AreEqual(0, tangent);
            Law.EvaluateConcrete(-1e-3, false, out stress, out tangent);
            Assert.AreEqual(0, stress); Assert.AreEqual(0, tangent);
        }

        [TestMethod]
        public void BarsCarryTheStressAtTheCrack()
        {
            Assert.AreEqual(146.7741935483871, Law.CrackingBarStress(.02), 1e-10);
            // Compressed: the strain of the plane; from zero the crack formation (cracked section).
            Assert.AreEqual(-1e-3, Law.EquivalentStrain(-1e-3, .02, out double d)); Assert.AreEqual(1, d);
            Assert.AreEqual(8e-5 / .6, Law.EquivalentStrain(8e-5, .02, out d), 1e-18); Assert.AreEqual(1 / .6, d, 1e-15);
            // Crack formation: εm / (1 − β).
            Assert.AreEqual(2e-4 / .6, Law.EquivalentStrain(2e-4, .02, out d), 1e-18); Assert.AreEqual(1 / .6, d, 1e-15);
            // Stabilised cracking: εm + β εsr.
            Assert.AreEqual(1.2935483870967742e-3, Law.EquivalentStrain(1e-3, .02, out d), 1e-15); Assert.AreEqual(1, d);
            // The two branches meet at (1 − β) εsr.
            double end = .6 * Law.CrackingBarStress(.02) / 200000;
            Assert.AreEqual(Law.EquivalentStrain(end * (1 - 1e-12), .02, out _), Law.EquivalentStrain(end * (1 + 1e-12), .02, out _), 1e-14);
            // A larger ratio gives a smaller shift.
            Assert.IsTrue(Law.EquivalentStrain(1e-3, .05, out _) < Law.EquivalentStrain(1e-3, .02, out _));
        }

        [TestMethod]
        public void InvalidDataAreRefused()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTensionChord(0, 2.6, 200000));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTensionChord(31000, double.NaN, 200000));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTensionChord(31000, 2.6, double.PositiveInfinity));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTensionChord(31000, 2.6, 200000, 1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTensionChord(31000, 2.6, 200000, -.1));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => Law.CrackingBarStress(0));
            Assert.AreEqual(.4, Law.Beta); Assert.AreEqual(.6, new ConcreteTensionChord(31000, 2.6, 200000, .6).Beta);
        }

        [TestMethod]
        public void TheSelectorKeepsItsValues()
        {
            Assert.AreEqual(0, (int)ConcreteMaterial.TensionResponseDiagrams.Default);
            Assert.AreEqual(1, (int)ConcreteMaterial.TensionResponseDiagrams.TensionStiffening);
            Assert.AreEqual(2, (int)ConcreteMaterial.TensionResponseDiagrams.TensionChord);
        }
    }
}
