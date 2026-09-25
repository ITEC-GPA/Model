using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using GPC.Model.Maths.GaussIntegrations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// Gauss rules on quadrilaterals and triangles (September 2026: Quad400, Quad12, Tri3 and Tri12 were wrong)
    /// </summary>
    [TestClass]
    public class GaussRulesTest
    {
        private static double Factorial(int n)
        {
            double f = 1;
            for (int i = 2; i <= n; i++)
                f *= i;
            return f;
        }

        [TestMethod]
        public void QuadrangleRulesAreExact()
        {
            var degrees = new Dictionary<QuadrangleGaussPoints.GaussPointNumber, int>
            {
                { QuadrangleGaussPoints.GaussPointNumber.Quad1, 1 },
                { QuadrangleGaussPoints.GaussPointNumber.Quad4, 3 },
                { QuadrangleGaussPoints.GaussPointNumber.Quad8, 5 },
                { QuadrangleGaussPoints.GaussPointNumber.Quad12, 7 },
                { QuadrangleGaussPoints.GaussPointNumber.Quad25, 9 },
                { QuadrangleGaussPoints.GaussPointNumber.Quad49, 13 },
                { QuadrangleGaussPoints.GaussPointNumber.Quad121, 21 },
                { QuadrangleGaussPoints.GaussPointNumber.Quad400, 39 },
            };

            foreach (var item in degrees)
            {
                GaussPoint[] points = QuadrangleGaussPoints.GaussPointNumberAssociation[item.Key];
                Assert.AreEqual((int)item.Key, points.Length);
                Assert.IsTrue(points.All(p => Math.Abs(p.Csi) < 1 && Math.Abs(p.Eta) < 1 && p.Weight > 0), $"{item.Key}: points inside, positive weights");

                // integral of csi^a eta^b over [-1, 1] x [-1, 1]
                for (int a = 0; a <= item.Value; a++)
                {
                    for (int b = 0; a + b <= item.Value; b++)
                    {
                        double exact = a % 2 == 1 || b % 2 == 1 ? 0 : 4.0 / ((a + 1) * (b + 1));
                        double numeric = points.Sum(p => p.Weight * Math.Pow(p.Csi, a) * Math.Pow(p.Eta, b));
                        Assert.AreEqual(exact, numeric, 1e-12, $"{item.Key}: csi^{a} eta^{b}");
                    }
                }
            }
        }

        [TestMethod]
        public void TriangleRulesAreExact()
        {
            var degrees = new Dictionary<TriangleGaussPoints.GaussPointNumber, int>
            {
                { TriangleGaussPoints.GaussPointNumber.Tri1, 1 },
                { TriangleGaussPoints.GaussPointNumber.Tri3, 2 },
                { TriangleGaussPoints.GaussPointNumber.Tri4, 3 },
                { TriangleGaussPoints.GaussPointNumber.Tri6, 4 },
                { TriangleGaussPoints.GaussPointNumber.Tri12, 6 },
                { TriangleGaussPoints.GaussPointNumber.Tri33, 12 },
                { TriangleGaussPoints.GaussPointNumber.Tri48, 15 },
                { TriangleGaussPoints.GaussPointNumber.Tri61, 17 },
                { TriangleGaussPoints.GaussPointNumber.Tri79, 20 },
            };

            foreach (var item in degrees)
            {
                GaussPoint[] points = TriangleGaussPoints.GaussPointNumberAssociation[item.Key];
                Assert.AreEqual((int)item.Key, points.Length);

                // integral of csi^a eta^b over the triangle (0,0) (1,0) (0,1): a! b! / (a + b + 2)!; the weights sum to 1, the area is 1/2
                for (int a = 0; a <= item.Value; a++)
                {
                    for (int b = 0; a + b <= item.Value; b++)
                    {
                        double exact = Factorial(a) * Factorial(b) / Factorial(a + b + 2);
                        double numeric = 0.5 * points.Sum(p => p.Weight * Math.Pow(p.Csi, a) * Math.Pow(p.Eta, b));
                        Assert.AreEqual(exact, numeric, 1e-10 * exact + 1e-15, $"{item.Key}: csi^{a} eta^{b}");
                    }
                }
            }
        }

        [TestMethod]
        public void QuadrilateralAndTrianglesGiveTheSameIntegral()
        {
            // general convex quadrilateral: the integral is compared with the one of its two triangles
            var quad = new[] { new Point3d(1, 2, 0), new Point3d(9, 0, 0), new Point3d(11, 7, 0), new Point3d(2, 6, 0) };
            var functions = new Func<double, double, double>[] { (x, y) => 1, (x, y) => x, (x, y) => x * y, (x, y) => x * x * y - 3 * y * y * y, (x, y) => Math.Pow(x, 4) * y * y };

            foreach (Func<double, double, double> f in functions)
            {
                double onQuad = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(f, quad, QuadrangleGaussPoints.GaussPointNumber.Quad400);
                double onTriangles = GaussIntegration.IntegrationTriangularLinearShapeFunction(f, new[] { quad[0], quad[1], quad[2] }, TriangleGaussPoints.GaussPointNumber.Tri33)
                                   + GaussIntegration.IntegrationTriangularLinearShapeFunction(f, new[] { quad[0], quad[2], quad[3] }, TriangleGaussPoints.GaussPointNumber.Tri33);
                Assert.AreEqual(onTriangles, onQuad, 1e-11 * Math.Abs(onTriangles) + 1e-9);
            }

            double area = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction((x, y) => 1, quad, QuadrangleGaussPoints.GaussPointNumber.Quad400);
            Assert.AreEqual(47.5, area, 1e-11);
        }

        [TestMethod]
        public void IntegralOverQuadAndTriangleMeshes()
        {
            // circle (polygon with 48 sides): area, static moment and moment of inertia of the polygon
            var polygon = new Polygon2d(Enumerable.Range(0, 48).Select(i => new Point2d(100 + 150 * Math.Cos(2 * Math.PI * i / 48), 50 + 150 * Math.Sin(2 * Math.PI * i / 48))).ToArray());
            var shape = new Shape2d(polygon);

            double area = 0, sy = 0, jyy = 0;
            for (int i = 0; i < polygon.Count; i++)
            {
                Point2d p = polygon[i], q = polygon[(i + 1) % polygon.Count];
                double cross = p.X * q.Y - q.X * p.Y;
                area += cross / 2;
                sy += (p.X + q.X) * cross / 6;
                jyy += (p.X * p.X + p.X * q.X + q.X * q.X) * cross / 12;
            }

            foreach (bool recombine in new[] { false, true })
            {
                Assert.IsTrue(DelaunayMesh.Generate(shape, new DelaunayMesh.DelaunayGenerateOptions { MeshSize = 40, Recombine = recombine }, out Mesh mesh, out _));
                Assert.AreEqual(recombine, mesh.Faces.Any(f => f.IsQuad));

                GaussIntegration.GlobalCoordinateGaussPoint[][] points = GaussIntegration.GetGlobalCoordinateGaussPointsLinearShapeFunction(mesh,
                    QuadrangleGaussPoints.GaussPointNumber.Quad400, TriangleGaussPoints.GaussPointNumber.Tri79);

                Assert.AreEqual(area, GaussIntegration.IntegrationLinearShapeFunction((x, y) => 1.0, points), 1e-9 * area);
                Assert.AreEqual(sy, GaussIntegration.IntegrationLinearShapeFunction((x, y) => x, points), 1e-9 * Math.Abs(sy));
                Assert.AreEqual(jyy, GaussIntegration.IntegrationLinearShapeFunction((x, y) => x * x, points), 1e-9 * jyy);
            }
        }
    }
}
