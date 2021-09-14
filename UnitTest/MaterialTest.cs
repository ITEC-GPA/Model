using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Materials;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using GPC.Utilities.Serialization;
using GPC.TestUtilities;
using GPC.Model.Standards;

namespace ModelObjectTest
{
    [TestClass]
    public class MaterialTest : UnitTestBase
    {
        [TestMethod]
        public void TestMethod1()
        {
            double E = 210000;
            double ni = 0.3;
            double density = 7850;
            double fy = 355;
            double fu = 510;

            SteelMaterial steel = new SteelMaterial("nome", E, ni, fy, fu, density);

            byte[] bytes = Serialization.SerializeToBytes<SteelMaterial>(steel);
            SteelMaterial steelDeserialized = (SteelMaterial)Serialization.DeserializeFromBytes(bytes);

            Assert.AreEqual(steelDeserialized.Name, steel.Name, "Nome diverso");
            Assert.AreEqual(steelDeserialized.AlfaThermalExpansion, steel.AlfaThermalExpansion, "Alfa expansion diverso");
            Assert.AreEqual(steelDeserialized.Density, steel.Density, "Density diverso");
            Assert.AreEqual(steelDeserialized.E, steel.E, "E diverso");
            Assert.AreEqual(steelDeserialized.Epsilon0, steel.Epsilon0, "epsiolon0 diverso");
            Assert.AreEqual(steelDeserialized.Fu, steel.Fu, "Fu diverso");
            Assert.AreEqual(steelDeserialized.Fyk, steel.Fyk, "Fyk diverso");
            Assert.AreEqual(steelDeserialized.Ni, steel.Ni, "Ni diverso");
            Assert.AreEqual(steelDeserialized.Guid, steel.Guid, "Guid diverso");

            Console.WriteLine(steelDeserialized.E + " " + steel.E);
        }


        [TestMethod]
        public void GlassResistance1()
        {
            double baseStress = 23.3;
            double baseEdgeStress = 18.3;
            double probBreakage = 0.004;
            double ncoeff = 16;
            double psiSurf = 0.5;

            GlassMaterialAstm gma = new GlassMaterialAstm("name", 1, 0.4, psiSurf, ncoeff, baseStress, baseEdgeStress, probBreakage, 100, 100);

            Assert.AreEqual(gma.GetGlassResistance(false, 3), baseStress * 0.906 * 1 * psiSurf, 0.01);
            Assert.AreEqual(gma.GetGlassResistance(true, 3), baseEdgeStress * 0.906 * 1 * psiSurf, 0.01);


            Assert.AreEqual(gma.GetGlassResistance(false, 86400), baseStress * 0.906 * 0.526 * psiSurf, 0.01);
            Assert.AreEqual(gma.GetGlassResistance(true, 86400), baseEdgeStress * 0.906 * 0.526 * psiSurf, 0.01);
        }


        [TestMethod]
        public void GlassResistance2()
        {
            double baseStress = 23.3;
            double baseEdgeStress = 18.3;
            double probBreakage = 0.004;
            double ncoeff = 47.5;
            double psiSurf = 0.5;

            GlassMaterialAstm gma = new GlassMaterialAstm("name", 1, 0.4, psiSurf, ncoeff, baseStress, baseEdgeStress, probBreakage, 100, 100);

            Assert.AreEqual(gma.GetGlassResistance(false, 3), baseStress * 0.906 * 1 * psiSurf, 0.01);
            Assert.AreEqual(gma.GetGlassResistance(true, 3), baseEdgeStress * 0.906 * 1 * psiSurf, 0.01);


            Assert.AreEqual(gma.GetGlassResistance(false, 86400), baseStress * 0.906 * 0.806 * psiSurf, 0.01);
            Assert.AreEqual(gma.GetGlassResistance(true, 86400), baseEdgeStress * 0.906 * 0.806 * psiSurf, 0.01);
        }

        [TestMethod]
        [Description("C25/30 StressBlock")]
        public void ConcreteENTest1()
		{
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992(25, ConcreteMaterialEN1992.StressStrainDiagrams.StressBlock);

            Assert.IsTrue(Math.Abs(concrete.E - 31.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fcm - 33.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 1.8) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 2.6) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 3.3) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.EpsilonU - 0.35) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.EpsilonY - 0.07) < 0.01);       
            Assert.IsTrue(Math.Abs(concrete.Fcd - 16.66) < 0.01);
        }

        [TestMethod]
        [Description("C60/75 StressBlock")]
        public void ConcreteENTest2()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992(60, ConcreteMaterialEN1992.StressStrainDiagrams.StressBlock);

            Assert.IsTrue(Math.Abs(concrete.E - 39.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fcm - 68.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 3.1) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 4.4) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 5.7) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.EpsilonU - 0.28835) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.EpsilonY - 0.06488) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.Fcd - 38.0) < 0.01);
        }

        [TestMethod]
        [Description("C25/30 BiLinear")]
        public void ConcreteENTest3()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992(25, ConcreteMaterialEN1992.StressStrainDiagrams.Bilinear);

            Assert.IsTrue(Math.Abs(concrete.E - 31.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fcm - 33.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 1.8) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 2.6) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 3.3) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.EpsilonU - 0.35) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.EpsilonY - 0.175) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.Fcd - 16.66) < 0.01);
        }

        [TestMethod]
        [Description("C60/75 BiLinear")]
        public void ConcreteENTest4()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992(60, ConcreteMaterialEN1992.StressStrainDiagrams.Bilinear);

            Assert.IsTrue(Math.Abs(concrete.E - 39.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fcm - 68.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 3.1) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 4.4) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 5.7) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.EpsilonU - 0.28835) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.EpsilonY - 0.19) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.Fcd - 40.0) < 0.01);
        }

        [TestMethod]
        [Description("C25/30 Parabola-Rectangle")]
        public void ConcreteENTest5()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992(25, ConcreteMaterialEN1992.StressStrainDiagrams.ParabolaRectangle);

            Assert.IsTrue(Math.Abs(concrete.E - 31.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fcm - 33.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 1.8) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 2.6) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 3.3) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.EpsilonU - 0.35) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.EpsilonY - 0.20) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.Fcd - 16.66) < 0.01);
        }

        [TestMethod]
        [Description("C60/75 Parabola-Rectangle")]
        public void ConcreteENTest6()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992(60, ConcreteMaterialEN1992.StressStrainDiagrams.ParabolaRectangle);

            Assert.IsTrue(Math.Abs(concrete.E - 39.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fcm - 68.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 3.1) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 4.4) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 5.7) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.EpsilonU - 0.28835) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.EpsilonY - 0.23) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.Fcd - 40.0) < 0.01);
        }

        [TestMethod]
        [Description("C60/75 CreepFactor")]
        public void ConcreteENTest7()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992(60, ConcreteMaterialEN1992.StressStrainDiagrams.ParabolaRectangle);

            double sigmaC1 = 20;
            double epsilonCC1 = concrete.CalculateEpsilonCCInfiniteTime(sigmaC1, 70, 1000000, 2000, 7);
            double epsilonCS1 = concrete.CalculateEpsilonCSInfiniteTime(70, 1000000, 2000);
            Assert.IsTrue(Math.Abs(epsilonCC1 / sigmaC1 * concrete.Ec - 1.346) < 0.01);
            Assert.IsTrue(Math.Abs(epsilonCS1 - 30.18 * 1e-5) < 0.01);

            double sigmaC2 = 15;
            double epsilonCC2 = concrete.CalculateEpsilonCCInfiniteTime(sigmaC2, 80, 1000000, 2000, 28);
            double epsilonCS2 = concrete.CalculateEpsilonCSInfiniteTime(80, 1000000, 2000);
            Assert.IsTrue(Math.Abs(epsilonCC2 / sigmaC2 * concrete.Ec - 0.981) < 0.01);
            Assert.IsTrue(Math.Abs(epsilonCS2 - 25.63 * 1e-5) < 0.01);
        }

        [TestMethod]
        [Description("C25/30 CreepFactor / shrinkage")]
        public void ConcreteENTest8()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992(25, ConcreteMaterialEN1992.StressStrainDiagrams.ParabolaRectangle);

            double sigmaC1 = 20;
            double epsilonCC1 = concrete.CalculateEpsilonCCInfiniteTime(sigmaC1, 70, 1000000, 2000, 7, 0);
            double epsilonCS1 = concrete.CalculateEpsilonCSInfiniteTime(70, 1000000, 2000);
            Assert.IsTrue(Math.Abs(epsilonCC1 - 4.078) < 0.01);
            Assert.IsTrue(Math.Abs(epsilonCS1 - 30.66 * 1e-5) < 0.01);

            double sigmaC2 = 15;
            double epsilonCC2 = concrete.CalculateEpsilonCCInfiniteTime(sigmaC2, 80, 1000000, 2000, 28);
            double epsilonCS2 = concrete.CalculateEpsilonCSInfiniteTime(80, 1000000, 2000);
            Assert.IsTrue(Math.Abs(epsilonCC2 - 2.146) < 0.01);
            Assert.IsTrue(Math.Abs(epsilonCS2 - 23.74 * 1e-5) < 0.01);

            double sigmaC3 = 10;
            double epsilonCC3 = concrete.CalculateEpsilonCCInfiniteTime(sigmaC3, 70, 1000000, 2000, 7, 20, 7);
            Assert.IsTrue(Math.Abs(epsilonCC3 - 0.730) < 0.01);
        }
    }
}
