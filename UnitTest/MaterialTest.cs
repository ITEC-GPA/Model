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
            double strainU = 0.05;

            SteelMaterial steel = new SteelMaterial("nome", E, fy, fu, strainU, SteelMaterial.SteelTypes.Structural, ni, density);

            byte[] bytes = Serialization.SerializeToBytes<SteelMaterial>(steel);
            SteelMaterial steelDeserialized = (SteelMaterial)Serialization.DeserializeFromBytes(bytes);

            Assert.AreEqual(steelDeserialized.Name, steel.Name, "Nome diverso");
            Assert.AreEqual(steelDeserialized.AlfaThermalExpansion, steel.AlfaThermalExpansion, "Alfa expansion diverso");
            Assert.AreEqual(steelDeserialized.Density, steel.Density, "Density diverso");
            Assert.AreEqual(steelDeserialized.E, steel.E, "E diverso");
            Assert.AreEqual(steelDeserialized.StrainU, steel.StrainU, "EpsiolonU diverso");
            Assert.AreEqual(steelDeserialized.Fu, steel.Fu, "Fu diverso");
            Assert.AreEqual(steelDeserialized.Fyk, steel.Fyk, "Fyk diverso");
            Assert.AreEqual(steelDeserialized.Ni, steel.Ni, "Ni diverso");
            Assert.AreEqual(steelDeserialized.Guid, steel.Guid, "Guid diverso");

            Console.WriteLine(steelDeserialized.E + " " + steel.E);
        }

		#region Glass Test

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

		#endregion

		#region EN1922 Concrete Material Test

		[TestMethod]
        [Description("C25/30 StressBlock")]
        public void ConcreteENTest1()
		{
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", 25, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.StressBlock);

            Assert.IsTrue(Math.Abs((concrete.E - 31.0 * 1000) / concrete.E) < 0.5, concrete.E.ToString());
            Assert.IsTrue(Math.Abs(concrete.Fcm + 33.0) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 1.8) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 2.6) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 3.3) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.0035) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.0007) < 0.01);       
            //Assert.IsTrue(Math.Abs(concrete.Fcd - 16.66) < 0.01);
        }

        [TestMethod]
        [Description("C60/75 StressBlock")]
        public void ConcreteENTest2()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", 60, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.StressBlock);

            Assert.IsTrue(Math.Abs((concrete.E - 39.0 * 1000) / concrete.E) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fcm + 68.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 3.1) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 4.4) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 5.7) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.0028835) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.006488) < 0.01);
            //Assert.IsTrue(Math.Abs(concrete.Fcd - 38.0) < 0.01);
        }

        [TestMethod]
        [Description("C25/30 BiLinear")]
        public void ConcreteENTest3()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", 25, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.Bilinear);

            Assert.IsTrue(Math.Abs((concrete.E - 31.0 * 1000) / concrete.E) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fcm + 33.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 1.8) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 2.6) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 3.3) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.0035) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.00175) < 0.01);
            //Assert.IsTrue(Math.Abs(concrete.Fcd - 16.66) < 0.01);
        }

        [TestMethod]
        [Description("C60/75 BiLinear")]
        public void ConcreteENTest4()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", 60, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.Bilinear);

            Assert.IsTrue(Math.Abs((concrete.E - 39.0 * 1000) / concrete.E) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fcm + 68.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 3.1) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 4.4) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 5.7) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.0028835) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.0019) < 0.01);
            //Assert.IsTrue(Math.Abs(concrete.Fcd - 40.0) < 0.01);
        }

        [TestMethod]
        [Description("C25/30 Parabola-Rectangle")]
        public void ConcreteENTest5()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", 25, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.ParabolaRectangle);

            Assert.IsTrue(Math.Abs((concrete.E - 31.0 * 1000) / concrete.E) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fcm + 33.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 1.8) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 2.6) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 3.3) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.0035) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.0020) < 0.01);
            //Assert.IsTrue(Math.Abs(concrete.Fcd - 16.66) < 0.01);
        }

        [TestMethod]
        [Description("C60/75 Parabola-Rectangle")]
        public void ConcreteENTest6()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", 60, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.ParabolaRectangle);

            Assert.IsTrue(Math.Abs((concrete.E - 39.0 * 1000) / concrete.E) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fcm + 68.0) < 0.5);
            Assert.IsTrue(Math.Abs(concrete.Fctk05 - 3.1) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctm - 4.4) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.Fctk95 - 5.7) < 0.1);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.0028835) < 0.01);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.0023) < 0.01);
            //Assert.IsTrue(Math.Abs(concrete.Fcd - 40.0) < 0.01);
        }

        [TestMethod]
        [Description("C60/75 CreepFactor")]
        public void ConcreteENTest7()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", 60, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.ParabolaRectangle);

            double sigmaC1 = 20;
            double epsilonCC1 = concrete.GetEpsilonCCInfiniteTime(sigmaC1, 70, 1000000, 2000, 7);
            double epsilonCS1 = concrete.GetEpsilonCSInfiniteTime(70, 1000000, 2000);
            Assert.IsTrue(Math.Abs(epsilonCC1 / sigmaC1 * concrete.Ec - 1.346) < 0.01);
            Assert.IsTrue(Math.Abs(epsilonCS1 - 30.18 * 1e-5) < 0.01);

            double sigmaC2 = 15;
            double epsilonCC2 = concrete.GetEpsilonCCInfiniteTime(sigmaC2, 80, 1000000, 2000, 28);
            double epsilonCS2 = concrete.GetEpsilonCSInfiniteTime(80, 1000000, 2000);
            Assert.IsTrue(Math.Abs(epsilonCC2 / sigmaC2 * concrete.Ec - 0.981) < 0.01);
            Assert.IsTrue(Math.Abs(epsilonCS2 - 25.63 * 1e-5) < 0.01);
        }

        [TestMethod]
        [Description("C25/30 CreepFactor / shrinkage")]
        public void ConcreteENTest8()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", 25, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.ParabolaRectangle);

            double sigmaC1 = 20;
            double epsilonCC1 = concrete.GetEpsilonCCInfiniteTime(sigmaC1, 70, 1000000, 2000, 7, 0);
            double epsilonCS1 = concrete.GetEpsilonCSInfiniteTime(70, 1000000, 2000);
            Assert.IsTrue(Math.Abs(epsilonCC1 - 4.078) < 0.01);
            Assert.IsTrue(Math.Abs(epsilonCS1 - 30.66 * 1e-5) < 0.01);

            double sigmaC2 = 15;
            double epsilonCC2 = concrete.GetEpsilonCCInfiniteTime(sigmaC2, 80, 1000000, 2000, 28);
            double epsilonCS2 = concrete.GetEpsilonCSInfiniteTime(80, 1000000, 2000);
            Assert.IsTrue(Math.Abs(epsilonCC2 - 2.146) < 0.01);
            Assert.IsTrue(Math.Abs(epsilonCS2 - 23.74 * 1e-5) < 0.01);

            double sigmaC3 = 10;
            double epsilonCC3 = concrete.GetEpsilonCCInfiniteTime(sigmaC3, 70, 1000000, 2000, 7, 20, 7);
            Assert.IsTrue(Math.Abs(epsilonCC3 - 0.000730) < 0.01);
        }

        [TestMethod]
        public void ConcreteENTest9()
        {
            ConcreteMaterialEN1992 concrete1 = new ConcreteMaterialEN1992("", 60, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.ParabolaRectangle);
            ConcreteMaterialEN1992 concrete2 = new ConcreteMaterialEN1992("", 60, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.Bilinear);
            ConcreteMaterialEN1992 concrete3 = new ConcreteMaterialEN1992("", 60, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.StressBlock);

            foreach (var concrete in new[] { concrete1, concrete2, concrete3 })
            {
                var stresses = concrete.StressStrainTableCompression.Stresses;
                var strains = concrete.StressStrainTableCompression.Strains;

                Assert.IsTrue(stresses[0] == 0);
                Assert.IsTrue(stresses[stresses.Length - 2] == - 60.0, stresses[1].ToString());
                Assert.IsTrue(stresses[stresses.Length - 1] == - 60.0, stresses[2].ToString());

                Assert.IsTrue(strains[0] == 0);
                Assert.IsTrue(strains[strains.Length - 2] == concrete.StrainYCompression);
                Assert.IsTrue(strains[strains.Length - 1] == concrete.StrainUCompression);
            }


            foreach (var concrete in new[] { concrete1, concrete2, concrete3 })
            {
                var stresses = concrete.StressStrainTableTension.Stresses;
                var strains = concrete.StressStrainTableTension.Strains;

                Assert.IsTrue(stresses[0] == 0);
                Assert.IsTrue(stresses[stresses.Length - 1] == concrete.Fctk05, stresses[stresses.Length - 1].ToString());

                Assert.IsTrue(strains[0] == 0);
                Assert.IsTrue(strains[strains.Length - 1] == concrete.StrainYTension);
                Assert.IsTrue(strains[strains.Length - 1] == concrete.StrainUTension);
            }

        }

        [TestMethod]
        public void ConcreteENTest10()
        {
            ConcreteMaterialEN1992 concrete = ConcreteMaterialEN1992.C25_30;
            List<double> stresses = new List<double>();

            for (int i = 10; i >= -35; i--)
                stresses.Add(concrete.GetStress(i / 10000.0));

            concrete = ConcreteMaterialEN1992.C30_37;

            for (int i = 10; i >= -35; i--)
                stresses.Add(concrete.GetStress(i / 10000.0));

            concrete = ConcreteMaterialEN1992.C45_55;

            for (int i = 10; i >= -35; i--)
                stresses.Add(concrete.GetStress(i / 10000.0));

            concrete = ConcreteMaterialEN1992.C60_75;

            for (int i = 10; i >= -35; i--)
                stresses.Add(concrete.GetStress(i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i] <= 0.0);
        }

        [TestMethod]
        public void ConcreteENTest11()
        {
            ConcreteMaterialEN1992 concrete = ConcreteMaterialEN1992.C25_30;
            List<double> stresses = new List<double>();

            for (int i = 10; i >= -35; i--)
                stresses.Add(concrete.GetStress(i / 10000.0));
                       
            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i] <= 0.0);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i]);
        }

        [TestMethod]
        public void ConcreteENTest12()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", 25, ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.StressBlock);
            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 10; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 0.0);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        [TestMethod]
        public void ConcreteENTest13()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", -0.01, 0.0, new StressStrainTable(new double[] { 0, -314.76 }, new double[] { 0, -0.01 }),
                new StressStrainTable(new double[] { 0, 0.000 }, new double[] { 0, 0.001 }));
            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 10; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 0.0);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        [TestMethod]
        public void ConcreteENTest14()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", 30, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.StressBlock);
            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 10; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 0.0);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        [TestMethod]
        public void ConcreteENTest15()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", 30, ConcreteMaterialEN1992.CompressionStressStrainDiagrams.NonLinear);
            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 10; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 0.0);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        [TestMethod]
        public void ConcreteENTest16()
        {
            ConcreteMaterialEN1992 concrete = new ConcreteMaterialEN1992("", -1.0, 1.0, new StressStrainTable(new double[] { 0, -1 }, new double[] { 0, -1 }),
                new StressStrainTable(new double[] { 0, 1 }, new double[] { 0, 1 }));
            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 10; i >= -100; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        #endregion

        #region ModelCode 2010 FRC Concrete Material Test

        [TestMethod]
        public void ConcreteFRCTest1()
        {
            ConcreteMaterialModelCode2010 concrete = new ConcreteMaterialModelCode2010("", 30, ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.ParabolaRectangle,
                1.55, 1.80, 0.00195, 0.01, ConcreteMaterialEuropeanCommon.TensionStressStrainDiagrams.Bilinear, ConcreteMaterialEuropeanCommon.ConcreteTypes.FRC, 0, 0, 0, ConcreteMaterialEuropeanCommon.CementType.ClassN);

            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 100; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 1.80);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        [TestMethod]
        public void ConcreteFRCTest2()
        {
            ConcreteMaterialModelCode2010 concrete = new ConcreteMaterialModelCode2010("", 30, ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.ParabolaRectangle,
                1.50, 1.00, 0.00195, 0.01, ConcreteMaterialEuropeanCommon.TensionStressStrainDiagrams.Bilinear, ConcreteMaterialEuropeanCommon.ConcreteTypes.FRC, 0, 0, 0, ConcreteMaterialEuropeanCommon.CementType.ClassN);

            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 100; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 1.80);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        [TestMethod]
        public void ConcreteFRCTest3()
        {
            ConcreteMaterialModelCode2010 concrete = new ConcreteMaterialModelCode2010("", 25, ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.ParabolaRectangle,
                1.55, 1.80, 0.00195, 0.01, ConcreteMaterialEuropeanCommon.TensionStressStrainDiagrams.RigidPlastic, ConcreteMaterialEuropeanCommon.ConcreteTypes.FRC);

            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 100; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 1.80);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        [TestMethod]
        public void ConcreteFRCTest4()
        {
            ConcreteMaterialModelCode2010 concrete = new ConcreteMaterialModelCode2010("", 25, ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.ParabolaRectangle,
                1.55, 1.80, 0.00195, 0.01, ConcreteMaterialEuropeanCommon.TensionStressStrainDiagrams.Bilinear, ConcreteMaterialEuropeanCommon.ConcreteTypes.FRC);

            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 100; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 1.80);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        [TestMethod]
        public void ConcreteFRCTest5()
        {
            ConcreteMaterialModelCode2010 concrete = ConcreteMaterialModelCode2010.C25_30_5;

            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 100; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 1.80);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);

            double fr1 = 2.09;
            double fr3 = 3.04;
            double ffts = concrete.CalculateFFTs(fr1, fr3);
            double fftu = concrete.CalculateFFTu(fr1, fr3);
            double fr1R = concrete.CalculateFR1(ffts, fftu);
            double fr3R = concrete.CalculateFR3(ffts, fftu);

            Assert.IsTrue(Math.Abs(fr1 / fr1R) - 1.0 < 0.01);
            Assert.IsTrue(Math.Abs(fr3 / fr3R) - 1.0 < 0.01);
        }

        [TestMethod]
        public void ConcreteFRCTest6()
        {
            ConcreteMaterialModelCode2010 concrete = ConcreteMaterialModelCode2010.C30_37_15;

            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 100; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 2.56);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);

            double fr1 = 1.09;
            double fr3 = 1.04;
            double ffts = concrete.CalculateFFTs(fr1, fr3);
            double fftu = concrete.CalculateFFTu(fr1, fr3);
            double fr1R = concrete.CalculateFR1(ffts, fftu);
            double fr3R = concrete.CalculateFR3(ffts, fftu);

            Assert.IsTrue(Math.Abs(fr1 / fr1R) - 1.0 < 0.01);
            Assert.IsTrue(Math.Abs(fr3 / fr3R) - 1.0 < 0.01);
        }

        [TestMethod]
        public void ConcreteFRCTest7()
        {
            ConcreteMaterialModelCode2010 concrete = new ConcreteMaterialModelCode2010("", -0.5, -5,
                new StressStrainTable(new double[] { 0, -10, -5, -20 }, new double[] { 0, -2, -4, -6 }),
                new StressStrainTable(new double[] { 0, 5, 1, 2 }, new double[] { 0, 1, 2, 3 }), ConcreteMaterialEuropeanCommon.ConcreteTypes.FRC);

            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 300; i >= -600; i--)
                stresses.Add((concrete.GetStress(i / 100.0), i / 100.0));

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        [TestMethod]
        public void ConcreteFRCTest8()
        {
            ConcreteMaterialModelCode2010 concreteMC = ConcreteMaterialModelCode2010.C45_55_10;
            concreteMC.ConcreteType = ConcreteMaterialEuropeanCommon.ConcreteTypes.Normal;

            ConcreteMaterialEN1992 concreteEN = ConcreteMaterialEN1992.C45_55;

            Assert.IsTrue(Math.Abs(concreteMC.Fck - concreteEN.Fck) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk - concreteEN.Fctk) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctu - concreteEN.Fctu) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.E - concreteEN.E) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk95 - concreteEN.Fctk95) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctm - concreteEN.Fctm) < 0.01);

            concreteMC.ConcreteType = ConcreteMaterialEuropeanCommon.ConcreteTypes.FRC;

            Assert.IsTrue(Math.Abs(concreteMC.Fck - concreteEN.Fck) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk - concreteEN.Fctk) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctu - concreteEN.Fctu) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.E - concreteEN.E) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk95 - concreteEN.Fctk95) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctm - concreteEN.Fctm) < 0.01);
        }

        [TestMethod]
        public void ConcreteFRCTest9()
        {
            ConcreteMaterialModelCode2010 concreteMC = ConcreteMaterialModelCode2010.C30_37_10;
            concreteMC.ConcreteType = ConcreteMaterialEuropeanCommon.ConcreteTypes.Normal;

            ConcreteMaterialEN1992 concreteEN = ConcreteMaterialEN1992.C30_37;

            Assert.IsTrue(Math.Abs(concreteMC.Fck - concreteEN.Fck) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk - concreteEN.Fctk) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctu - concreteEN.Fctu) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.E - concreteEN.E) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk95 - concreteEN.Fctk95) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctm - concreteEN.Fctm) < 0.01);

            concreteMC.ConcreteType = ConcreteMaterialEuropeanCommon.ConcreteTypes.FRC;

            Assert.IsTrue(Math.Abs(concreteMC.Fck - concreteEN.Fck) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk - concreteEN.Fctk) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctu - concreteEN.Fctu) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.E - concreteEN.E) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk95 - concreteEN.Fctk95) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctm - concreteEN.Fctm) < 0.01);
        }

        [TestMethod]
        public void ConcreteFRCTest10()
        {
            ConcreteMaterialModelCode2010 concreteMC = ConcreteMaterialModelCode2010.C30_37_25;
            concreteMC.ConcreteType = ConcreteMaterialEuropeanCommon.ConcreteTypes.Normal;

            ConcreteMaterialEN1992 concreteEN = ConcreteMaterialEN1992.C30_37;

            Assert.IsTrue(Math.Abs(concreteMC.Fck - concreteEN.Fck) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk - concreteEN.Fctk) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctu - concreteEN.Fctu) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.E - concreteEN.E) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk95 - concreteEN.Fctk95) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctm - concreteEN.Fctm) < 0.01);

            concreteMC.ConcreteType = ConcreteMaterialEuropeanCommon.ConcreteTypes.FRC;

            Assert.IsTrue(Math.Abs(concreteMC.Fck - concreteEN.Fck) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk - concreteEN.Fctk) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctu - concreteEN.Fctu) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.E - concreteEN.E) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctk95 - concreteEN.Fctk95) < 0.01);
            Assert.IsTrue(Math.Abs(concreteMC.Fctm - concreteEN.Fctm) < 0.01);
        }

        [TestMethod]
        public void ConcreteFRCTest11()
        {
            ConcreteMaterialModelCode2010 concreteMC = ConcreteMaterialModelCode2010.C30_37_25;
            concreteMC.StressStrainTableCompression.GetMinimumStress();
            concreteMC.StressStrainTableCompression.GetLastStrain();

            ConcreteMaterialModelCode2010 c = new ConcreteMaterialModelCode2010("", -25, ConcreteMaterialEuropeanCommon.CompressionStressStrainDiagrams.ParabolaRectangle,
                -1, -12, -1, -2, ConcreteMaterialEuropeanCommon.TensionStressStrainDiagrams.Bilinear, ConcreteMaterial.ConcreteTypes.FRC);
            c.StressStrainTableCompression.GetMinimumStress();
            c.StressStrainTableCompression.GetLastStrain();
        }

        [TestMethod]
        public void ConcreteFRCTest12()
        {
            ConcreteMaterialModelCode2010 concreteMC = ConcreteMaterialModelCode2010.C30_37_10;
            bool comp = concreteMC.StressStrainTableCompression.IsHardening();
            bool tens = concreteMC.StressStrainTableTension.IsHardening();

            Assert.IsTrue(comp);
            Assert.IsFalse(tens);
        }

        [TestMethod]
        public void ConcreteFRCTest13()
        {
            ConcreteMaterialModelCode2010 concreteMC = ConcreteMaterialModelCode2010.C30_37_25;
            bool comp = concreteMC.StressStrainTableCompression.IsHardening();
            bool tens = concreteMC.StressStrainTableTension.IsHardening();

            Assert.IsTrue(comp);
            Assert.IsFalse(tens);
        }

        [TestMethod]
        public void ConcreteFRCTest14()
        {
            ConcreteMaterialModelCode2010 concreteMaterial = new ConcreteMaterialModelCode2010("", -0.002, 0.0001,
                    new StressStrainTable(new double[] { 0, -50, -50 }, new double[] { 0, -0.02, -0.035 }),
                    new StressStrainTable(new double[] { 0, 5 }, new double[] { 0, 0.001 }), ConcreteMaterial.ConcreteTypes.FRC);

            Assert.IsTrue(concreteMaterial.StrainYPureCompression == -0.02);
        }

        #endregion

        #region ModelCode 2010 Concrete Material Test

        [TestMethod]
        public void ConcreteModelCodeTest1()
        {
            ConcreteMaterialModelCode2010 concrete = ConcreteMaterialModelCode2010.C25_30;

            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 100; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 1.80);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        [TestMethod]
        public void ConcreteModelCodeTest2()
        {
            ConcreteMaterialModelCode2010 concrete = ConcreteMaterialModelCode2010.C25_30;
            concrete.ConcreteType = ConcreteMaterial.ConcreteTypes.Normal;
            concrete.ConcreteType = ConcreteMaterial.ConcreteTypes.FRC;

            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 100; i >= -35; i--)
                stresses.Add((concrete.GetStress(i / 10000.0), i / 10000.0));

            for (int i = 0; i < stresses.Count; i++)
                Assert.IsTrue(stresses[i].Item1 <= 1.80);

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        #endregion

        #region ACI318 Concrete Material Test

        [TestMethod]
        [Description("Fc 4000 Bilinear")]
        public void ConcreteACITest1()
        {
            ConcreteMaterialACI318 concrete = new ConcreteMaterialACI318("fc' 4000 psi", 27.579,
                ConcreteMaterialACI318.CompressionStressStrainDiagrams.Bilinear);

            Assert.IsTrue(Math.Abs((concrete.E - 24855) / concrete.E) < 0.5, concrete.E.ToString());
            Assert.IsTrue(Math.Abs(concrete.Fc + 27.579) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.Fct - 3.270) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.003) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.0011) < 0.001);
        }

        [TestMethod]
        [Description("Fc 4000 StressBlock")]
        public void ConcreteACITest2()
        {
            ConcreteMaterialACI318 concrete = new ConcreteMaterialACI318("fc' 4000 psi", 27.579,
                ConcreteMaterialACI318.CompressionStressStrainDiagrams.StressBlock);

            Assert.IsTrue(Math.Abs((concrete.E - 24855) / concrete.E) < 0.5, concrete.E.ToString());
            Assert.IsTrue(Math.Abs(concrete.Fc + 23.442) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.Fct - 3.270) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.003) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.0011) < 0.001);
        }

        [TestMethod]
        [Description("Fc 3000")]
        public void ConcreteACITest3()
        {
            ConcreteMaterialACI318 concrete = ConcreteMaterialACI318.Fc3000;

            Assert.IsTrue(Math.Abs((concrete.E - 21525.562) / concrete.E) < 0.5, concrete.E.ToString());
            Assert.IsTrue(Math.Abs(concrete.Fc + 20.6843) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.003) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.0011) < 0.001);
        }

        [TestMethod]
        [Description("Fc 4000")]
        public void ConcreteACITest4()
        {
            ConcreteMaterialACI318 concrete = ConcreteMaterialACI318.Fc4000;

            Assert.IsTrue(Math.Abs((concrete.E - 24855) / concrete.E) < 0.5, concrete.E.ToString());
            Assert.IsTrue(Math.Abs(concrete.Fc + 27.579) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.003) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.0011) < 0.001);
        }

        [TestMethod]
        [Description("Fc 5000")]
        public void ConcreteACITest5()
        {
            ConcreteMaterialACI318 concrete = ConcreteMaterialACI318.Fc5000;

            Assert.IsTrue(Math.Abs((concrete.E - 27789.382) / concrete.E) < 0.5, concrete.E.ToString());
            Assert.IsTrue(Math.Abs(concrete.Fc + 34.4738) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.003) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.0011) < 0.001);
        }

        [TestMethod]
        [Description("Fc 6000")]
        public void ConcreteACITest6()
        {
            ConcreteMaterialACI318 concrete = ConcreteMaterialACI318.Fc6000;

            Assert.IsTrue(Math.Abs((concrete.E - 30441.742) / concrete.E) < 0.5, concrete.E.ToString());
            Assert.IsTrue(Math.Abs(concrete.Fc + 41.3685) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainUCompression + 0.003) < 0.001);
            Assert.IsTrue(Math.Abs(concrete.StrainYCompression + 0.0011) < 0.001);
        }

        [TestMethod]
        public void ConcreteACITest8()
        {
            ConcreteMaterialACI318 concrete = ConcreteMaterialACI318.Fc5000;
            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 100; i >= -350; i--)
                stresses.Add((concrete.GetStress(i / 100000.0), i / 100000.0));

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        [TestMethod]
        public void ConcreteACITest9()
        {
            ConcreteMaterialACI318 concrete = new ConcreteMaterialACI318("fc' 4000 psi", 27.579,
                ConcreteMaterialACI318.CompressionStressStrainDiagrams.ParabolaRectangle);
            List<(double, double)> stresses = new List<(double, double)>();

            for (int i = 100; i >= -350; i--)
                stresses.Add((concrete.GetStress(i / 100000.0), i / 100000.0));

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i].Item1);
        }

        #endregion

        #region Steel Test

        [TestMethod]
        public void SteelTest1()
        {
            SteelMaterial steel = SteelMaterial.S275;
            List<double> stresses = new List<double>();

            for (int i = 75; i >= -75; i--)
                stresses.Add(steel.CalculateStress(i / 1000.0));

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i]);
        }

        [TestMethod]
        public void SteelTest2()
        {
            SteelMaterial steel = new SteelMaterial("", 200000, 275, 275*1.15);
            List<double> stresses = new List<double>();

            for (int i = 75; i >= -75; i--)
                stresses.Add(steel.CalculateStress(i / 1000.0));

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i]);
        }

        [TestMethod]
        public void RebarTest1()
        {
            SteelMaterial steel = SteelMaterial.B450C;
            List<double> stresses = new List<double>();

            for (int i = 75; i >= -75; i--)
                stresses.Add(steel.CalculateStress(i / 1000.0));

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i]);
        }

        [TestMethod]
        public void RebarTest3()
        {
            SteelMaterial steel = new SteelMaterial("", 200000, 450, 450);
            List<double> stresses = new List<double>();

            for (int i = 75; i >= -75; i--)
                stresses.Add(steel.CalculateStress(i / 1000.0));

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i]);
        }

        [TestMethod]
        public void RebarTest2()
        {
            SteelMaterial steel = SteelMaterial.B450C;

            double stressYTension = steel.CalculateStress(0.001955);
            double stressYCompression = steel.CalculateStress(-0.001955);

            double stressYTest1 = steel.CalculateStress(0.0008243);
            double stressYTest2 = steel.CalculateStress(0.0001708);

            Assert.IsTrue(Math.Abs(stressYTension - 391) < 1);
            Assert.IsTrue(Math.Abs(stressYCompression + 391) < 1);
            Assert.IsTrue(Math.Abs(stressYTest1 - 164.9) < 1);
            Assert.IsTrue(Math.Abs(stressYTest2 - 34.15) < 1);
        }

        [TestMethod]
        public void RebarTest4()
        {
            SteelMaterial steel = SteelMaterial.B450C;
            StandardNTC2018Concrete standardModelCode2010 = new StandardNTC2018Concrete();
            double strain1 = 0.001955 * 0.5;
            double strain2 = 0.001955;
            double strain3 = 0.001955 * 1.5;
            double strain4 = 0.001955 * 2;
            double strain5 = 0.02;
            double strain6 = 0.04;
            double strain7 = 0.06;
            double strain8 = 0.08;
            double strain9 = 0.1;
            double stress1 = steel.CalculateDesignStressRebar(standardModelCode2010, strain1);
            double stress2 = steel.CalculateDesignStressRebar(standardModelCode2010, strain2);
            double stress3 = steel.CalculateDesignStressRebar(standardModelCode2010, strain3);
            double stress4 = steel.CalculateDesignStressRebar(standardModelCode2010, strain4);
            double stress5 = steel.CalculateDesignStressRebar(standardModelCode2010, strain5);
            double stress6 = steel.CalculateDesignStressRebar(standardModelCode2010, strain6);
            double stress7 = steel.CalculateDesignStressRebar(standardModelCode2010, strain7);
            double stress8 = steel.CalculateDesignStressRebar(standardModelCode2010, strain8);
            double stress9 = steel.CalculateDesignStressRebar(standardModelCode2010, strain9);
            double stressTest = steel.CalculateDesignStressRebar(standardModelCode2010, strain4);

            double expValue = steel.Fyk / 1.15;

            Assert.IsTrue(Math.Abs(stress1 - expValue / 2.0) / stress1 < 0.001);
            Assert.IsTrue(Math.Abs(stress2 - expValue) / stress2 < 0.001);
            Assert.IsTrue(Math.Abs(stress3 - expValue) / stress3 < 0.001);
            Assert.IsTrue(Math.Abs(stress4 - expValue) / stress4 < 0.001);
            Assert.IsTrue(Math.Abs(stress5 - expValue) / stress5 < 0.001);
            Assert.IsTrue(Math.Abs(stress6 - expValue) / stress6 < 0.001);
            Assert.IsTrue(Math.Abs(stress7 - expValue) / stress7 < 0.001);
            Assert.IsTrue(Math.Abs(stress8 - expValue) / stress8 < 0.001);
            Assert.IsTrue(Math.Abs(stress9 - expValue) / stress9 < 0.001);
            Assert.IsTrue(Math.Abs(stressTest - expValue) / stressTest < 0.001);
        }

        [TestMethod]
        public void TendonTest1()
        {
            SteelMaterial tendon = new SteelMaterial("", 195000, 1620, 1800);
            List<double> stresses = new List<double>();

            for (int i = 75; i >= -75; i--)
                stresses.Add(tendon.CalculateStress(i / 1000.0));

            for (int i = 0; i < stresses.Count; i++)
                Console.WriteLine(stresses[i]);
        }

        [TestMethod]
        public void TendonTest2()
        {
            SteelMaterial tendon = SteelMaterial.Y1770C;
            StandardNTC2018Concrete standardModelCode2010 = new StandardNTC2018Concrete();
            double strain1 = 0.0017128174817783004;
            double strain2 = 0.0057628174817783004;
            double strain3 = 0.010628174817783005;
            double strain4 = 0.017928174817783004;
            double epsilonP = 0.0071794871794871795;
            double stress1 = tendon.CalculateDesignStressRebar(standardModelCode2010, strain1, epsilonP);
            double stress2 = tendon.CalculateDesignStressRebar(standardModelCode2010, strain2, epsilonP);
            double stress3 = tendon.CalculateDesignStressRebar(standardModelCode2010, strain3, epsilonP);
            double stress4 = tendon.CalculateDesignStressRebar(standardModelCode2010, strain4, epsilonP);
            double stressTest = tendon.CalculateDesignStressRebar(standardModelCode2010, strain4 + epsilonP);

            double expValue = tendon.Fyk / 1.15;

            Assert.IsTrue(Math.Abs(stress1 - expValue) / stress1 < 0.001);
            Assert.IsTrue(Math.Abs(stress2 - expValue) / stress2 < 0.001);
            Assert.IsTrue(Math.Abs(stress3 - expValue) / stress3 < 0.001);
            Assert.IsTrue(Math.Abs(stress4 - expValue) / stress4 < 0.001);
            Assert.IsTrue(Math.Abs(stressTest - expValue) / stressTest < 0.001);
        }

        #endregion

        #region StressStrainTable Test

        [TestMethod]
        public void StressStrainTableTest1()
        {
            StressStrainTable stressStrainTable = new StressStrainTable(new double[] {}, new double[] {});

            Assert.IsTrue(stressStrainTable.Stresses[0] == 0);
            Assert.IsTrue(stressStrainTable.Strains[0] == 0);
        }

        [TestMethod]
        public void StressStrainTableTest2()
        {
            StressStrainTable stressStrainTable = new StressStrainTable(new double[] { 1, 2 }, new double[] { 1, 2 });

            Assert.IsTrue(stressStrainTable.Stresses[0] == 0);
            Assert.IsTrue(stressStrainTable.Strains[0] == 0);
            Assert.IsTrue(stressStrainTable.Strains.Length == 3);
            Assert.IsTrue(stressStrainTable.Strains.Length == 3);
        }

        [TestMethod]
        public void StressStrainTableTest3()
        {
            StressStrainTable stressStrainTable = new StressStrainTable(new double[] { 0, 2 }, new double[] { 0, 2 });
            stressStrainTable.Insert(1, 1, 1);

            Assert.IsTrue(stressStrainTable.Stresses[1] == 1);
            Assert.IsTrue(stressStrainTable.Strains[1] == 1);
            Assert.IsTrue(stressStrainTable.Strains.Length == 3);
            Assert.IsTrue(stressStrainTable.Strains.Length == 3);
        }

        [TestMethod]
        public void StressStrainTableTest4()
        {
            StressStrainTable stressStrainTable = new StressStrainTable(new double[] { 1, 2 }, new double[] { 1, 2 });
            stressStrainTable.Add(3, 3);

            Assert.IsTrue(stressStrainTable.Stresses[0] == 0);
            Assert.IsTrue(stressStrainTable.Strains[0] == 0);
            Assert.IsTrue(stressStrainTable.Stresses[3] == 3);
            Assert.IsTrue(stressStrainTable.Strains[3] == 3);
        }

        [TestMethod]
        public void StressStrainTableTest5()
        {
            StressStrainTable stressStrainTable = new StressStrainTable(new double[] { 0, 1, 2, 3, 4 }, new double[] { 0, 1, 2, 3, 4 });
            stressStrainTable.Remove(3);

            Assert.IsTrue(stressStrainTable.Stresses[0] == 0);
            Assert.IsTrue(stressStrainTable.Strains[0] == 0);
            Assert.IsTrue(stressStrainTable.Stresses[3] == 4);
            Assert.IsTrue(stressStrainTable.Strains[3] == 4);
            Assert.IsTrue(stressStrainTable.Stresses.Length == 4);
            Assert.IsTrue(stressStrainTable.Strains.Length == 4);
        }

        [TestMethod]
        public void StressStrainTableTest6()
        {
            StressStrainTable stressStrainTable = new StressStrainTable(new double[] { 0, 1, 2, 3, 4 }, new double[] { 0, 1, 2, 3, 4 });
            stressStrainTable.Remove(3);
            stressStrainTable.Remove(3);

            Assert.IsTrue(stressStrainTable.Stresses[0] == 0);
            Assert.IsTrue(stressStrainTable.Strains[0] == 0);
            Assert.IsTrue(stressStrainTable.Stresses[2] == 2);
            Assert.IsTrue(stressStrainTable.Strains[2] == 2);
            Assert.IsTrue(stressStrainTable.Stresses.Length == 3);
            Assert.IsTrue(stressStrainTable.Strains.Length == 3);
        }

        #endregion
    }
}
