using GPC.Model.Materials;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class InterlayerMaterialTest
    {
        private static double[] _loadDuration;
        private static double[] _temperatures;
        private static double[][] _shearModules;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            _loadDuration = new double[6] { 3, 40, 60, 100, 900, 700 }; // Non in ordine
            _temperatures = new double[5] { 1, 20, 60, 50, 40 }; // Non in ordine

            _shearModules = new double[6][] { new double[5] { 3, 30, 30000, 3000, 300},
                                              new double[5] { 4, 40, 40000, 4000, 400},
                                              new double[5] { 5, 50, 50000, 5000, 500},
                                              new double[5] { 6, 60, 60000, 6000, 600},
                                              new double[5] { 7, 70, 70000, 7000, 700},
                                              new double[5] { 8, 80, 80000, 8000, 800},
                                            };
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // Nothing
        }

        [TestCleanup]
        public void CleanUp()
        {
            // Nothing
        }

        [TestMethod]
        public void ShearModulus1()
        {
            InterlayerMaterial interlayerMaterial = new InterlayerMaterial(1, 0, InterlayerMaterial.InterlayerType.AcusticPVB, Guid.NewGuid());

            for (int i = 0; i < 6; i++)
            {
                interlayerMaterial.AddShearModule(_loadDuration[i], _temperatures, _shearModules[i]);
            }
            interlayerMaterial.Sort();

            double result = interlayerMaterial.GetShearModule(3, 1);
            double expected = _shearModules[0][0];
            string message = $"Result: {result}, Expected: {expected}";
            Console.WriteLine(message);
            Assert.IsTrue(result == expected, message);
        }
        
        [TestMethod]
        public void ShearModulus2()
        {
            InterlayerMaterial interlayerMaterial = new InterlayerMaterial(1, 0, InterlayerMaterial.InterlayerType.AcusticPVB, Guid.NewGuid());

            for (int i = 0; i < 6; i++)
            {
                interlayerMaterial.AddShearModule(_loadDuration[i], _temperatures, _shearModules[i]);
            }
            interlayerMaterial.Sort();

            double result = interlayerMaterial.GetShearModule(700, 60);
            double expected = _shearModules[5][2];
            string message = $"Result: {result}, Expected: {expected}";
            Console.WriteLine(message);
            Assert.IsTrue(result == expected, message);
        }

        [TestMethod]
        public void GetTemperatures()
        {
            InterlayerMaterial interlayerMaterial = new InterlayerMaterial(1, 0, InterlayerMaterial.InterlayerType.AcusticPVB, Guid.NewGuid());

            for (int i = 0; i < 6; i++)
            {
                interlayerMaterial.AddShearModule(_loadDuration[i], _temperatures, _shearModules[i]);
            }
            interlayerMaterial.Sort();

            List<double> temperatures = interlayerMaterial.GetTemperatures();

            List<double> temperatureExpected = new List<double>(_temperatures);
            temperatureExpected.Sort();


            for (int i = 0; i < temperatureExpected.Count; i++)
            {
                if (temperatureExpected[i] != temperatures[i])
                    Assert.Fail();
            }
        }

        [TestMethod]
        public void GetLoadDurations()
        {
            InterlayerMaterial interlayerMaterial = new InterlayerMaterial(1, 0, InterlayerMaterial.InterlayerType.AcusticPVB, Guid.NewGuid());

            for (int i = 0; i < 6; i++)
            {
                interlayerMaterial.AddShearModule(_loadDuration[i], _temperatures, _shearModules[i]);
            }
            interlayerMaterial.Sort();

            List<double> loadDurations = interlayerMaterial.GetLoadDurations();

            List<double> loadDurationseExpected = new List<double>(_loadDuration);
            loadDurationseExpected.Sort();


            for (int i = 0; i < loadDurationseExpected.Count; i++)
            {
                if (loadDurationseExpected[i] != loadDurations[i])
                    Assert.Fail();
            }
        }
    }
}