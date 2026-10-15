using System;
using GPC.Model.Materials;

namespace UnitTest
{
    /// <summary>
    /// Tension stiffening of the concrete for the nonlinear response analyses (user request 16/10/2026: a descending post-cracking branch for all the
    /// concretes, opt-in, never in the resistances). Law of Collins and Mitchell (1991): σ = Ec ε up to εcr = fct / Ec, then fct / (1 + √(500 ε)),
    /// tangent −250 fct / (√(500 ε) (1 + √(500 ε))²). Expected values: independent Python (scratchpad agent-concrete-tension/ref_ts.py) with
    /// EN 1992-1-1 Table 3.1 (fcm = fck + 8, Ecm = 22000 (fcm / 10)^0.3, fctm = 0.30 fck^(2/3) up to C50, 2.12 ln(1 + fcm / 10) beyond) and, for ACI 318,
    /// Ec = 57000 √f'c psi and fcr = 0.33 √f'c MPa (Collins and Mitchell 1991)
    /// </summary>
    [TestClass]
    public class ConcreteTensionStiffeningTest
    {
        private static void AssertLaw(ConcreteTensionStiffening law, double ec, double fct, double crackingStrain, double drop,
            params (double Strain, double Stress, double Tangent)[] rows)
        {
            Assert.AreEqual(ec, law.ElasticModulus, 1e-6 * ec, "Ec");
            Assert.AreEqual(fct, law.TensileStrength, 1e-8, "fct");
            Assert.AreEqual(crackingStrain, law.CrackingStrain, 1e-14, "εcr");
            // uncracked up to εcr included, then the drop of the original formulation fct / (1 + √(500 εcr))
            Assert.AreEqual(fct, law.GetStress(law.CrackingStrain), 1e-8, "σ(εcr)");
            Assert.AreEqual(drop, law.GetStress(law.CrackingStrain * (1 + 1e-12)), 1e-8, "σ(εcr+)");
            foreach (var (strain, stress, tangent) in rows)
            {
                law.Evaluate(strain, out double s, out double t);
                Assert.AreEqual(stress, s, 1e-8, "σ at " + strain);
                Assert.AreEqual(tangent, t, Math.Max(1e-8, Math.Abs(tangent) * 1e-9), "tangent at " + strain);
                Assert.AreEqual(s, law.GetStress(strain), 0.0);
                Assert.AreEqual(t, law.GetTangent(strain), 0.0);
            }
        }

        [TestMethod]
        public void EuropeanConcretesUseEcmAndFctm()
        {
            AssertLaw(new ConcreteMaterialEN1992("C35/45", 35, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle).GetTensionStiffening(),
                34077.146199, 3.209962442, 9.419692667e-05, 2.637555409,
                (-1e-3, 0, 0), (0, 0, 0), (5e-5, 1.703857310, 34077.146199189), (2e-4, 2.438759100, -1464.798422190),
                (1e-3, 1.880352464, -389.433746240), (1e-2, 0.991932946, -34.270440413));
            AssertLaw(new ConcreteMaterialModelCode2010("C25/30", 25, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle).GetTensionStiffening(),
                31475.806210, 2.564963920, 8.149001499e-05, 2.134172768,
                (5e-5, 1.573790311, 31475.806210019), (2e-4, 1.948723455, -1170.466998059), (1e-3, 1.502521077, -311.182303994),
                (1e-2, 0.792617441, -27.384259093));
            // beyond C50: fctm = 2.12 ln(1 + fcm / 10); the diagram in compression does not matter
            AssertLaw(new ConcreteMaterialEN1992("C60/75", 60, ConcreteMaterial.CompressionStressStrainDiagrams.StressBlock).GetTensionStiffening(),
                39099.873708, 4.354742315, 1.113748435e-04, 3.523306110,
                (5e-5, 1.954993685, 39099.873708049), (2e-4, 3.308502090, -1987.194488583), (1e-3, 2.550948988, -528.318833822),
                (1e-2, 1.345689382, -46.492424677));
        }

        [TestMethod]
        public void Aci318ConcreteUsesTheCrackingStressOfCollinsAndMitchell()
        {
            var concrete = new ConcreteMaterialACI318("30", 30, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
            // 0.33 √30 = 1.80748 MPa, not the modulus of rupture fr = 0.62 √f'c (a flexural strength)
            Assert.AreEqual(1.807484440, concrete.GetMeanTensileStrength(), 1e-8);
            AssertLaw(concrete.GetTensionStiffening(), 25923.584960, 1.807484440, 6.972355261e-05, 1.523101466,
                (5e-5, 1.296179248, 25923.584960418), (2e-4, 1.373230748, -824.807269118), (1e-3, 1.058799871, -219.284633211),
                (1e-2, 0.558543409, -19.297200175));
        }

        [TestMethod]
        public void TangentIsTheDerivativeOfTheStress()
        {
            var law = new ConcreteTensionStiffening(30000, 2.9);
            foreach (double strain in new[] { 1.2e-4, 3e-4, 1e-3, 5e-3, 2e-2 })
            {
                double h = strain * 1e-6;
                double numerical = (law.GetStress(strain + h) - law.GetStress(strain - h)) / (2 * h);
                Assert.AreEqual(numerical, law.GetTangent(strain), Math.Abs(numerical) * 1e-6, "ε = " + strain);
                Assert.IsTrue(law.GetTangent(strain) < 0 && law.GetStress(strain) > 0, "descending and positive");
            }
            // monotonic decrease after cracking, never zero
            Assert.IsTrue(law.GetStress(1e-3) > law.GetStress(1e-2) && law.GetStress(1e-2) > law.GetStress(1) && law.GetStress(1) > 0);
        }

        [TestMethod]
        public void InvalidValuesAreRejected()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTensionStiffening(0, 2));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTensionStiffening(30000, -2));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTensionStiffening(double.NaN, 2));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new ConcreteTensionStiffening(30000, double.PositiveInfinity));
            // a strain that is not a number propagates
            Assert.IsTrue(double.IsNaN(new ConcreteTensionStiffening(30000, 2).GetStress(double.NaN)));
        }

        [TestMethod]
        public void DefaultIsTheBehaviourOfTheAnalysis()
        {
            // The option of the analyses keeps today's behaviour when not assigned
            Assert.AreEqual(ConcreteMaterial.TensionResponseDiagrams.Default, default(ConcreteMaterial.TensionResponseDiagrams));
            Assert.AreEqual(1, (int)ConcreteMaterial.TensionResponseDiagrams.TensionStiffening);
            // not a property of the material: building the law leaves the material unchanged
            var concrete = new ConcreteMaterialEN1992("C30/37", 30, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
            var copy = new ConcreteMaterialEN1992("C30/37", 30, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
            concrete.GetTensionStiffening();
            Assert.IsTrue(concrete.Equals(copy));
            Assert.AreEqual(ConcreteMaterial.TensionStressStrainDiagrams.Linear, concrete.TensionStressStrainDiagram);
        }
    }
}
