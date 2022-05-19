using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Maths.GaussIntegrations;
using GPC.TestUtilities;
using GPC.Utilities.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PerformanceTest
{
    [TestClass]
    public class PerformanceIntegrationTest : UnitTestBase
    {

        [TestMethod]
        public void IntegrationTest1()
        {

            double constant = 3.0;

            Point3d[] polygon = new Point3d[] { new Point2d(-1.0, -1.0), new Point2d(+1.0, -1.0), new Point2d(+1.0, +1.0), new Point2d(-1.0, +1.0) };

            Func<double, double, double> func = (x, y) => constant * x * x;


            double result = 0;
            Action ac2 = new Action(() =>
            {
                result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, polygon, QuadrangleGaussPoints.GaussPointNumber.Quad400);
            });

            var bb0 = MeasureTime.FunctionExecutionTime(20, ac2, true); ;

            Console.WriteLine(bb0);

        }

        [TestMethod]
        public void IntegrationQuad4Test1LSF()
        {
            int numberOfExecutons = 1000;

            Point3d[] vertices = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(4.0, 10, 0), new Point3d(5.0, 10.0, 0.0) };
            Func<double, double, double> func = (double x, double y) => x * x + y / 2 + x;

            Action actionP79 = new Action(() =>
            {
                GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad400);
            });

            Action actionP61 = new Action(() =>
            {
                GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad121);
            });

            Action actionP48 = new Action(() =>
            {
                GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad49);
            });

            Action actionP33 = new Action(() =>
            {
                GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad25);
            });

            Action actionP12 = new Action(() =>
            {
                GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad12);
            });

            Action actionP6 = new Action(() =>
            {
                GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad8);
            });

            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP79, true, "400 Gauss Points");
            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP61, true, "121 Gauss Points");
            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP48, true, "49 Gauss Points");
            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP33, true, "25 Gauss Points");
            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP12, true, "12 Gauss Points");
            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP6, true, "8 Gauss Points");
        }

        [TestMethod]
        public void IntegrationTri3Test1LSF()
        {
            int numberOfExecutons = 1000;

            Point3d[] vertices = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(4.0, 10, 0) };
            Func<double, double, double> func = (double x, double y) => x * x + y / 2 + x;

            Action actionP79 = new Action(() =>
            {
                GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri79);
            });

            Action actionP61 = new Action(() =>
            {
                GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri61);
            });

            Action actionP48 = new Action(() =>
            {
                GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri48);
            });

            Action actionP33 = new Action(() =>
            {
                GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri33);
            });

            Action actionP12 = new Action(() =>
            {
                GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri12);
            });

            Action actionP6 = new Action(() =>
            {
                GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri6);
            });

            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP79, true, "79 Gauss Points");
            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP61, true, "61 Gauss Points");
            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP48, true, "48 Gauss Points");
            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP33, true, "33 Gauss Points");
            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP12, true, "12 Gauss Points");
            MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP6, true, "6 Gauss Points");
        }



        [TestMethod]
        public void IntegrationQuadFunction()
        {

            Point3d[] vertices = new Point3d[] { new Point3d(-23, -21, 0), new Point3d(-10, -3, 0), new Point3d(6, -17, 0), new Point3d(18, -12, 0) };

            Func<double, double, double> func = (double x, double y) => Math.Pow(Math.Sin(x), 2) * Math.Pow(Math.Cos(y), 3) + 11;


            double value = GaussIntegration.IntegrationQuadrilateral(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad400,
                GPC.Utilities.Fem.LinearShapeFunctionQuad4.NaturalShapeFunction,
                GPC.Utilities.Fem.LinearShapeFunctionQuad4.DNdCsi, GPC.Utilities.Fem.LinearShapeFunctionQuad4.DNdEta, 4);

            Console.WriteLine(value);

            //int numberOfExecutons = 1000;


            //int nrGaussPoints = 79;

            //Action actionP79 = new Action(() =>
            //{
            //    GaussIntegration.IntegrationQuadrilateral(func, vertices, nrGaussPoints);
            //});
        }
    }
}
