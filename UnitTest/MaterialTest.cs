using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Materials;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
using GPC.Utilities.Serialization;

namespace UnitTest
{
    [TestClass]
    public class MaterialTest
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
    }
}
