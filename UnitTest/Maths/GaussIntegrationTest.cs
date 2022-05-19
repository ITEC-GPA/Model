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

        public void CommonEqualAssert(double result, double expectedValue)
        {
            if (expectedValue == 0)
            {
                Assert.IsTrue(Math.Abs(result - expectedValue) < 0.01, $"1) calculated value: {result}, expValue: {expectedValue}");
            }
            else
            {
                if (result != 0)
                {
                    Assert.IsTrue(Math.Abs((result - expectedValue) / result) < 0.01, $"1) calculated value: {result}, expValue: {expectedValue}");
                }
                else
                {
                    Assert.IsTrue(Math.Abs(result - expectedValue) < 0.01, $"1) calculated value: {result}, expValue: {expectedValue}");
                }
            }           

        }

        protected void CommonAssertLineLinear(Func<double, double, double> func, Point3d[] poly, double expectedValue)
        {
            double result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, LineGaussPoints.GaussPointNumber.Line3);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, LineGaussPoints.GaussPointNumber.Line4);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, LineGaussPoints.GaussPointNumber.Line6);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, LineGaussPoints.GaussPointNumber.Line9);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, LineGaussPoints.GaussPointNumber.Line16);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, LineGaussPoints.GaussPointNumber.Line32);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationLineLinearShapeFunction(func, poly, LineGaussPoints.GaussPointNumber.Line20);
            CommonEqualAssert(result, expectedValue);
        }


        protected void CommonAssertQuadLinear(Func<double, double, double> func, Point3d[] poly, double expectedValue)
        {
            double result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad4);

            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad8);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad12);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad25);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad49);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad121);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad400);
            CommonEqualAssert(result, expectedValue);
        }


        protected void CommonAssertQuadQuadratic(Func<double, double, double> func, Point3d[] poly, double expectedValue)
        {
            double result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad4);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad8);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad12);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad25);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad49);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad121);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationQuadrilateralQuadraticShapeFunction(func, poly, QuadrangleGaussPoints.GaussPointNumber.Quad400);
            CommonEqualAssert(result, expectedValue);
        }

        protected void CommonAssertTriLinear(Func<double, double, double> func, Point3d[] poly, double expectedValue)
        {
            double result;

            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri4);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri6);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri12);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri33);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri48);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri61);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationTriangularLinearShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri79);
            CommonEqualAssert(result, expectedValue);
        }


        protected void CommonAssertTriQuadratic(Func<double, double, double> func, Point3d[] poly, double expectedValue)
        {
            double result;

            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri4);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri6);
            CommonEqualAssert(result, expectedValue);


            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri12);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri33);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri48);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri61);
            CommonEqualAssert(result, expectedValue);

            result = GaussIntegration.IntegrationTriangularQuadraticShapeFunction(func, poly, TriangleGaussPoints.GaussPointNumber.Tri79);
            CommonEqualAssert(result, expectedValue);
        }


        [TestMethod]
        public void Line2Test1LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(1.0, 1.0, 0), new Point3d(10.0, 1.0, 0) };
            double expValue = 999;
            Func<double, double, double> func = (x, y) => constant * x * x;

            CommonAssertLineLinear(func, poly, expValue);

        }

        [TestMethod]
        public void Line2Test2LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(1.0, 1.0, 0), new Point3d(10.0, 1.0, 0) };
            double expValue = 7499.25;
            Func<double, double, double> func = (x, y) => constant * x * x * x;

            CommonAssertLineLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test1LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(-1.0, -1.0, 0), new Point3d(+1.0, -1.0, 0), new Point3d(+1.0, +1.0, 0), new Point3d(-1.0, +1.0, 0) };
            double expValue = constant * ((1.0 * 1.0 * 1.0) - (-1.0 * -1.0 * -1.0)) / 3.0 * 2.0;
            Func<double, double, double> func = (x, y) => constant * x * x;

            CommonAssertQuadLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test2LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(0.0, 0.0, 0), new Point3d(10, 0.0, 0), new Point3d(10, 10, 0), new Point3d(0.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * x;
            double expValue = 10000;

            CommonAssertQuadLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test3LSF()
        {
            double constant = 3.0;
            double expValue = 4218.75;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * y;

            CommonAssertQuadLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test4LSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(12, 12, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x;
            double expValue = 241.5;

            CommonAssertQuadLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test5LSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(15, 12, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x;
            double expValue = 336;

            CommonAssertQuadLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test6LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * y * x;
            double expValue = 4218.75;

            CommonAssertQuadLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test7LSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => -constant * y * x;
            double expValue = -4218.75;

            CommonAssertQuadLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test8LSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(15, 12, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (x, y) => -x;
            double expValue = -336;

            CommonAssertQuadLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test9LSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(15, 12, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (x, y) => 0;
            double expValue = 0;

            CommonAssertQuadLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Tri3Test1LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 85.5;

            CommonAssertTriLinear(func, vertices, expValue);
        }

        [TestMethod]
        public void Tri3Test2LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 126;

            CommonAssertTriLinear(func, vertices, expValue);
        }

        [TestMethod]
        public void Tri3Test3LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 354.66666;

            CommonAssertTriLinear(func, vertices, expValue);
        }

        [TestMethod]
        public void Tri3Test4LSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) => y;
            double expValue = 466.6666;

            CommonAssertTriLinear(func, vertices, expValue);
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

            CommonAssertQuadQuadratic(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test2QSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(0.0, 0.0, 0), new Point3d(10, 0.0, 0), new Point3d(10, 10, 0), new Point3d(0.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * x;
            double expValue = 10000;

            CommonAssertQuadQuadratic(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test3QSF()
        {
            double constant = 3.0;
            double expValue = 4218.75;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * x * y;

            CommonAssertQuadQuadratic(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test4QSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(12, 12, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x;
            double expValue = 241.5;

            CommonAssertQuadLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test5QSF()
        {
            Point3d[] poly = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(15, 12, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (x, y) => x;
            double expValue = 336;

            CommonAssertQuadQuadratic(func, poly, expValue);
        }

        [TestMethod]
        public void Quad4Test6QSF()
        {
            double constant = 3.0;

            Point3d[] poly = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 5.0, 0), new Point3d(10, 10, 0), new Point3d(5.0, 10, 0) };
            Func<double, double, double> func = (x, y) => constant * y * x;
            double expValue = 4218.75;

            CommonAssertQuadLinear(func, poly, expValue);
        }

        [TestMethod]
        public void Tri3Test1QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 85.5;

            CommonAssertTriQuadratic(func, vertices, expValue);
        }

        [TestMethod]
        public void Tri3Test2QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(8.0, 5.0, 0), new Point3d(13, 7.0, 0), new Point3d(7.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 126;

            CommonAssertTriQuadratic(func, vertices, expValue);
        }

        [TestMethod]
        public void Tri3Test3QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) => x;
            double expValue = 354.66666;

            CommonAssertTriQuadratic(func, vertices, expValue);
        }

        [TestMethod]
        public void Tri3Test4QSF()
        {
            Point3d[] vertices = new Point3d[] { new Point3d(2, 2, 0), new Point3d(12, 8, 0), new Point3d(5, 15, 0) };

            Func<double, double, double> func = (double x, double y) => y;
            double expValue = 466.6666;

            CommonAssertTriQuadratic(func, vertices, expValue);
        }

        #endregion

    }
}