using GPC.Model.Materials;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace ModelObjectTest
{
    [TestClass]
    public class InterlayerMaterialTest
    {
        private static readonly double[] _loadDuration = new double[6] { 3, 40, 60, 100, 900, 700 }; // Non in ordine
        private static readonly double[] _temperatures = new double[5] { 1, 20, 60, 50, 40 }; // Non in ordine

        private static readonly double[][] _shearModules = new double[6][] { new double[5] { 3, 30, 30000, 3000, 300},
                                                                             new double[5] { 4, 40, 40000, 4000, 400},
                                                                             new double[5] { 5, 50, 50000, 5000, 500},
                                                                             new double[5] { 6, 60, 60000, 6000, 600},
                                                                             new double[5] { 7, 70, 70000, 7000, 700},
                                                                             new double[5] { 8, 80, 80000, 8000, 800},
                                                                           };

        private static InterlayerMaterial CreateMaterial()
        {
            InterlayerMaterial interlayerMaterial = new InterlayerMaterial("I1", 1, 0, InterlayerMaterial.InterlayerType.AcusticPVB);

            for (int i = 0; i < 6; i++)
            {
                interlayerMaterial.AddShearModule(_loadDuration[i], _temperatures, _shearModules[i]);
            }
            interlayerMaterial.Sort();

            return interlayerMaterial;
        }

        [TestMethod]
        public void ShearModulus1()
        {
            double result = CreateMaterial().GetShearModule(3, 1);
            double expected = _shearModules[0][0];
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ShearModulus2()
        {
            double result = CreateMaterial().GetShearModule(700, 60);
            double expected = _shearModules[5][2];
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ShearModulusBetweenTwoTemperatures()
        {
            // duration 40: 40 at 20 °C, 400 at 40 °C
            Assert.AreEqual(220, CreateMaterial().GetShearModule(40, 30), 1e-9);
        }

        [TestMethod]
        public void ShearModulusBetweenTwoLoadDurations()
        {
            // at 20 °C: 40 for the duration 40, 50 for the duration 60. Before, the abscissa of the interpolation was the temperature
            // (40 + (50 - 40) / (60 - 40) × (20 - 40) = 30)
            Assert.AreEqual(45, CreateMaterial().GetShearModule(50, 20), 1e-9);

            // between the durations and between the temperatures: 220 (duration 40) and 275 (duration 60) at 30 °C
            Assert.AreEqual(247.5, CreateMaterial().GetShearModule(50, 30), 1e-9);

            // durations 100 and 700 at 60 °C: 60000 and 80000
            Assert.AreEqual(70000, CreateMaterial().GetShearModule(400, 60), 1e-9);
        }

        [TestMethod]
        public void ShearModulusOutOfTheTables()
        {
            var material = CreateMaterial();
            Assert.ThrowsException<IndexOutOfRangeException>(() => material.GetShearModule(1, 20));
            Assert.ThrowsException<IndexOutOfRangeException>(() => material.GetShearModule(1000, 20));
            Assert.ThrowsException<IndexOutOfRangeException>(() => material.GetShearModule(40, 0));
            Assert.ThrowsException<IndexOutOfRangeException>(() => material.GetShearModule(40, 70));

            // one table and one temperature: before, a greater value threw ArgumentOutOfRangeException
            var single = new InterlayerMaterial("I2", 1, 0, InterlayerMaterial.InterlayerType.NormalPVB);
            single.AddShearModule(3, new double[] { 20 }, new double[] { 0.5 });
            Assert.AreEqual(0.5, single.GetShearModule(3, 20));
            Assert.ThrowsException<IndexOutOfRangeException>(() => single.GetShearModule(10, 20));
            Assert.ThrowsException<IndexOutOfRangeException>(() => single.GetShearModule(3, 30));

            var empty = new InterlayerMaterial("I3", 1, 0, InterlayerMaterial.InterlayerType.NormalPVB);
            Assert.ThrowsException<KeyNotFoundException>(() => empty.GetShearModule(3, 20));
        }

        [TestMethod]
        public void GetTemperatures()
        {
            List<double> temperatures = CreateMaterial().GetTemperatures();

            List<double> temperatureExpected = new List<double>(_temperatures);
            temperatureExpected.Sort();

            CollectionAssert.AreEqual(temperatureExpected, temperatures);
        }

        [TestMethod]
        public void GetLoadDurations()
        {
            List<double> loadDurations = CreateMaterial().GetLoadDurations();

            List<double> loadDurationseExpected = new List<double>(_loadDuration);
            loadDurationseExpected.Sort();

            CollectionAssert.AreEqual(loadDurationseExpected, loadDurations);
        }
    }
}
