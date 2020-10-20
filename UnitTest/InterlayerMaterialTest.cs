using GPC.Model.Materials;
using GPC.Utilities.Maths;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

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
            _loadDuration = new double[6] { 10, 20, 30, 60, 50, 40 }; // Non in ordine

            _temperatures = new double[5] { 100, 200, 500, 400, 300 }; // Non in ordine

            _shearModules = new double[6][] { new double[5] { 1000, 2000, 5000, 4000, 3000},
                                              new double[5] { 1001, 2001, 5001, 4001, 3001},
                                              new double[5] { 1002, 2002, 5002, 4002, 3002},
                                              new double[5] { 1003, 2003, 5003, 4003, 3003},
                                              new double[5] { 1004, 2004, 5004, 4004, 3004},
                                              new double[5] { 1005, 2005, 5005, 4005, 3005},
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
            InterlayerMaterial interlayerMaterial = new InterlayerMaterial(1, 0, Guid.NewGuid());

            for (int i = 0; i < 6; i++)
            {
                interlayerMaterial.AddShearModule(_loadDuration[i], _temperatures, _shearModules[i]);
            }
            interlayerMaterial.Sort();

            double result = interlayerMaterial.GetShearModule(10, 100);
            double expected = _shearModules[0][0];  // 10, 100
            string message = $"Result: {result}, Expected: {expected}";
            Console.WriteLine(message);
            Assert.IsTrue(result == expected, message);
        }

        [TestMethod]
        public void ShearModulus2()
        {
            // Arrange
            InterlayerMaterial interlayerMaterial = new InterlayerMaterial(1, 0, Guid.NewGuid());

            for (int i = 0; i < 6; i++)
            {
                interlayerMaterial.AddShearModule(_loadDuration[i], _temperatures, _shearModules[i]);
            }
            interlayerMaterial.Sort();

            // Act
            double result = interlayerMaterial.GetShearModule(60, 400);
            double expected = _shearModules[3][3]; // 60, 400
            string message = $"Result: {result}, Expected: {expected}";
            Console.WriteLine(message);
            Assert.IsTrue(result == expected, message);
        }

        [TestMethod]
        public void ShearModulus3()
        {
            // Arrange
            InterlayerMaterial interlayerMaterial = new InterlayerMaterial(1, 0, Guid.NewGuid());

            for (int i = 0; i < 6; i++)
            {
                interlayerMaterial.AddShearModule(_loadDuration[i], _temperatures, _shearModules[i]);
            }
            interlayerMaterial.Sort();

            // Act
            double temperature = 500;
            double result = interlayerMaterial.GetShearModule(55, temperature);
            double lowerExp = _shearModules[4][2];  //50, 500
            double greaterExp = _shearModules[3][2]; //60, 500

            double expected = Interpolation.GetLinearInterpolation(_loadDuration[4], _loadDuration[3], lowerExp, greaterExp, temperature);

            // Assert
            string message = $"Result: {result}, Expected: {expected}";
            Console.WriteLine(message);
            Assert.IsTrue(result == expected, message);
        }

        [TestMethod]
        public void ShearModulus4()
        {
            // Arrange
            InterlayerMaterial interlayerMaterial = new InterlayerMaterial(1, 0, Guid.NewGuid());

            for (int i = 0; i < 6; i++)
            {
                interlayerMaterial.AddShearModule(_loadDuration[i], _temperatures, _shearModules[i]);
            }
            interlayerMaterial.Sort();

            // Act
            double temperature = 5000;
            try
            {
                double result = interlayerMaterial.GetShearModule(55, temperature);
            }
            // Assert
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.Fail();
            }
        }

        [TestMethod]
        public void ShearModulus5()
        {
            // Arrange
            InterlayerMaterial interlayerMaterial = new InterlayerMaterial(1, 0, Guid.NewGuid());

            for (int i = 0; i < 6; i++)
            {
                interlayerMaterial.AddShearModule(_loadDuration[i], _temperatures, _shearModules[i]);
            }
            interlayerMaterial.Sort();

            // Act
            double temperature = 0;
            try
            {
                double result = interlayerMaterial.GetShearModule(55, temperature);
            }
            // Assert
            catch (IndexOutOfRangeException e)
            {
                Console.WriteLine(e.Message);
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Assert.Fail();
            }
        }
    }
}