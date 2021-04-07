using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Materials;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using GPC.Utilities.Serialization;
using GPC.TestUtilities;

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
    }
}
