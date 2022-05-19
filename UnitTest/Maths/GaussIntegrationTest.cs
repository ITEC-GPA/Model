using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Maths.GaussIntegrations;
using GPC.Utilities.Fem;
using GPC.Utilities.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MathTest
{
    [TestClass]
    public class GaussIntegrationTest
    {

        #region Linear Shape Function

        [TestMethod]
        public void Line2Test1LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(1.0, 1.0, 0), new Point3d(10.0, 1.0, 0) };
            double expValue = 999;
            Func<double, double, double> func = (x, y) => constant * x * x;

            int nrGaussPoints = 3;
            double result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 9;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 16;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 32;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 20;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Line2Test2LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(1.0, 1.0, 0), new Point3d(10.0, 1.0, 0) };
            double expValue = 7499.25;
            Func<double, double, double> func = (x, y) => constant * x * x * x;

            int nrGaussPoints = 3;
            double result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 9;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 16;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 32;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 20;
            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test1LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(-1.0, -1.0, 0), new Point3d(+1.0, -1.0, 0), new Point3d(+1.0, +1.0, 0), new Point3d(-1.0, +1.0, 0) };
            double expValue = constant * ((1.0 * 1.0 * 1.0) - (-1.0 * -1.0 * -1.0)) / 3.0 * 2.0;
            Func<double, double, double> func = (x, y) => constant * x * x;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test2LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(0.0, 0.0, 0), new Point3d(10, 0.0, 0), new Point3d(10, 10, 0), new Point3d(0.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * x;
            double expValue = 10000;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test3LSF()
        {
            double constant = 3.0;
            double expValue = 4218.75;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * y;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test4LSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(12, 12, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x;
            double expValue = 241.5;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test5LSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(15, 12, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x;
            double expValue = 336;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test6LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * y * x;
            double expValue = 4218.75;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test7LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => -constant * y * x;
            double expValue = -4218.75;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test8LSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(15, 12, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (x, y) => -x;
            double expValue = -336;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test9LSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(15, 12, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (x, y) => 0;
            double expValue = 0;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs(result - expValue) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue)) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue)) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue)) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue)) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue)) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue)) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test1LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 85.5;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test2LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 126;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test3LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 354.66666;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test4LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) => y;
            double expValue = 466.6666;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        #endregion

        #region Quadratic Shape Function

        [TestMethod]
        public void Quad4Test1QSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(-1.0, -1.0, 0), new Point3d(+1.0, -1.0, 0), new Point3d(+1.0, +1.0, 0), new Point3d(-1.0, +1.0, 0) };
            double expValue = constant * ((1.0 * 1.0 * 1.0) - (-1.0 * -1.0 * -1.0)) / 3.0 * 2.0;
            Func<double, double, double> func = (x, y) => constant * x * x;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test2QSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(0.0, 0.0, 0), new Point3d(10, 0.0, 0), new Point3d(10, 10, 0), new Point3d(0.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * x;
            double expValue = 10000;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test3QSF()
        {
            double constant = 3.0;
            double expValue = 4218.75;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * y;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test4QSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(12, 12, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x;
            double expValue = 241.5;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test5QSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(15, 12, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x;
            double expValue = 336;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Quad4Test6QSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * y * x;
            double expValue = 4218.75;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 8;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 25;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 49;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"5) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 121;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 400;
            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"6) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test1QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 85.5;

            int nrGaussPoints = 4;
            double result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 33;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test2QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 126;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test3QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 354.66666;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        [TestMethod]
        public void Tri3Test4QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) => y;
            double expValue = 466.6666;

            int nrGaussPoints = 33;
            double result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 12;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"2) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 6;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"3) calculated value: {result}, expValue: {expValue}");

            nrGaussPoints = 4;
            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, vertices, nrGaussPoints);
            Assert.IsTrue(Math.Abs((result - expValue) / result) < 0.01, $"4) calculated value: {result}, expValue: {expValue}");
        }

        #endregion

    }
}