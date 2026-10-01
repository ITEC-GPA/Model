using GPC.Geometry;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UnitTest
{
    /// <summary>
    /// The torsion solved with the finite elements (<see cref="Section.CalculateTorsionProperties"/>) against the exact solutions and the
    /// thin-walled theory
    /// </summary>
    [TestClass]
    public class SectionTorsionTest
    {
        private static Section Generic(Point2d[] fill, params Point2d[][] holes)
        {
            var shape = holes.Length == 0
                ? new Shape2d(new Polygon2d(fill))
                : new Shape2d(new Polygon2d(fill), holes.Select(h => new Polygon2d(h)).ToArray());
            var section = new Section(shape, "generic");
            section.SetMechanicalProperties();
            return section;
        }

        private static Point2d[] Rectangle(double x0, double y0, double width, double height) => new[]
        {
            new Point2d(x0, y0), new Point2d(x0 + width, y0), new Point2d(x0 + width, y0 + height), new Point2d(x0, y0 + height),
        };

        private static Point2d[] Ellipse(double a, double b, int count) =>
            Enumerable.Range(0, count).Select(i => new Point2d(a * Math.Cos(2 * Math.PI * i / count), b * Math.Sin(2 * Math.PI * i / count))).ToArray();

        /// <summary>
        /// The torsion constant of the rectangle (series of Saint-Venant): a b³ [1/3 - 64 b / (π⁵ a) Σ tanh(n π a / 2b) / n⁵], n odd, a ≥ b
        /// </summary>
        private static double RectangleTorsion(double a, double b)
        {
            double sum = 0;
            for (int n = 1; n < 200; n += 2)
                sum += Math.Tanh(n * Math.PI * a / (2 * b)) / Math.Pow(n, 5);
            return a * Math.Pow(b, 3) * (1.0 / 3.0 - 64.0 * b / (Math.Pow(Math.PI, 5) * a) * sum);
        }

        [TestMethod]
        public void EquilateralTriangle()
        {
            // exact: the warping function about the centroid is (y³ - 3 x² y) / 2h: It = √3 a⁴ / 80, Iw = √3 a⁶ / 40320, shear centre in the centroid
            double a = 100, h = a * Math.Sqrt(3) / 2;
            var triangle = Generic(new[] { new Point2d(0, 0), new Point2d(a, 0), new Point2d(a / 2, h) });
            SectionTorsionProperties torsion = triangle.CalculateTorsionProperties();

            Assert.IsTrue(torsion.IsSolved, torsion.Error);
            Assert.AreEqual(Math.Sqrt(3) * Math.Pow(a, 4) / 80, torsion.TorsionConstant, 1e-4 * torsion.TorsionConstant);
            Assert.AreEqual(Math.Sqrt(3) * Math.Pow(a, 6) / 40320, torsion.WarpingConstant, 1e-3 * torsion.WarpingConstant);
            Assert.AreEqual(a / 2, torsion.ShearCenter.X, 1e-6 * a);
            Assert.AreEqual(h / 3, torsion.ShearCenter.Y, 1e-6 * a);
        }

        [TestMethod]
        public void Rectangles()
        {
            foreach (var (width, height) in new[] { (200.0, 100.0), (100.0, 100.0), (20.0, 400.0) })
            {
                var rectangle = Generic(Rectangle(0, 0, width, height));
                SectionTorsionProperties torsion = rectangle.CalculateTorsionProperties();

                Assert.IsTrue(torsion.IsSolved, torsion.Error);
                double exact = RectangleTorsion(Math.Max(width, height), Math.Min(width, height));
                Assert.AreEqual(exact, torsion.TorsionConstant, 1e-3 * exact);
                Assert.AreEqual(width / 2, torsion.ShearCenter.X, 1e-6 * width);
                Assert.AreEqual(height / 2, torsion.ShearCenter.Y, 1e-6 * height);
            }
        }

        [TestMethod]
        public void EllipseAndHollowCircle()
        {
            // ellipse: It = π a³ b³ / (a² + b²); the polygon of 720 sides is smaller than the ellipse by about 1.3e-5
            var ellipse = Generic(Ellipse(60, 30, 720));
            SectionTorsionProperties torsion = ellipse.CalculateTorsionProperties();
            double exact = Math.PI * Math.Pow(60, 3) * Math.Pow(30, 3) / (60 * 60 + 30 * 30);
            Assert.AreEqual(exact, torsion.TorsionConstant, 1e-3 * exact);
            Assert.AreEqual(0, torsion.ShearCenter.X, 1e-6 * 60);
            Assert.AreEqual(0, torsion.ShearCenter.Y, 1e-6 * 60);

            // hollow circle: It = Ip = π (D⁴ - d⁴) / 32, no warping
            var tube = Generic(Ellipse(50, 50, 720), Ellipse(40, 40, 720).Reverse().ToArray());
            torsion = tube.CalculateTorsionProperties();
            exact = Math.PI * (Math.Pow(100, 4) - Math.Pow(80, 4)) / 32;
            Assert.AreEqual(exact, torsion.TorsionConstant, 1e-3 * exact);
            Assert.AreEqual(0, torsion.WarpingConstant, 1e-6 * Math.Pow(100, 6));
        }

        [TestMethod]
        public void ThinChannelAndI()
        {
            // thin walls 2 mm, middle lines h = 200 and b = 100: the thin-walled theory is exact within about 1%
            double t = 2, h = 200, b = 100;
            var channel = Generic(new[]
            {
                new Point2d(0, 0), new Point2d(b + t / 2, 0), new Point2d(b + t / 2, t), new Point2d(t, t), new Point2d(t, h), new Point2d(b + t / 2, h),
                new Point2d(b + t / 2, h + t), new Point2d(0, h + t),
            });
            SectionTorsionProperties torsion = channel.CalculateTorsionProperties();
            Assert.IsTrue(torsion.IsSolved, torsion.Error);

            double e = 3 * b * b * t / (6 * b * t + h * t);
            Assert.AreEqual(t / 2 - e, torsion.ShearCenter.X, 0.01 * e);
            Assert.AreEqual(h / 2 + t / 2, torsion.ShearCenter.Y, 1e-6 * h);
            double iw = t * Math.Pow(b, 3) * h * h * (3 * b * t + 2 * h * t) / (12 * (6 * b * t + h * t));
            Assert.AreEqual(iw, torsion.WarpingConstant, 0.02 * iw);
            Assert.AreEqual((h + 2 * b) * Math.Pow(t, 3) / 3, torsion.TorsionConstant, 0.05 * torsion.TorsionConstant);

            // I: Iw = tf b³ h² / 24, shear centre in the centroid
            var beam = Generic(new[]
            {
                new Point2d(0, 0), new Point2d(b, 0), new Point2d(b, t), new Point2d(b / 2 + t / 2, t), new Point2d(b / 2 + t / 2, h),
                new Point2d(b, h), new Point2d(b, h + t), new Point2d(0, h + t), new Point2d(0, h), new Point2d(b / 2 - t / 2, h),
                new Point2d(b / 2 - t / 2, t), new Point2d(0, t),
            });
            torsion = beam.CalculateTorsionProperties();
            iw = t * Math.Pow(b, 3) * h * h / 24;
            Assert.AreEqual(iw, torsion.WarpingConstant, 0.02 * iw);
            Assert.AreEqual(b / 2, torsion.ShearCenter.X, 1e-6 * b);
            Assert.AreEqual(h / 2 + t / 2, torsion.ShearCenter.Y, 1e-6 * h);
        }

        [TestMethod]
        public void SeparateParts()
        {
            // two separate rectangles: the torsion constant is the sum, warping constant and shear centre are not defined
            var shape = new Shape2d(new Polygon2d(Rectangle(0, 0, 300, 300)), new[] { new Polygon2d(Rectangle(50, 50, 200, 200)) },
                new[] { new Shape2d(new Polygon2d(Rectangle(100, 100, 100, 50))) });
            var section = new Section(shape, "parts");
            section.SetMechanicalProperties();
            SectionTorsionProperties torsion = section.CalculateTorsionProperties();

            Assert.IsTrue(torsion.IsSolved, torsion.Error);
            Assert.AreEqual(2, torsion.Parts);
            double core = RectangleTorsion(100, 50);
            var frame = Generic(Rectangle(0, 0, 300, 300), Rectangle(50, 50, 200, 200));
            Assert.AreEqual(frame.CalculateTorsionProperties().TorsionConstant + core, torsion.TorsionConstant, 1e-3 * torsion.TorsionConstant);
            Assert.IsTrue(double.IsNaN(torsion.WarpingConstant));
            Assert.IsTrue(double.IsNaN(torsion.ShearCenter.X));
        }
    }
}
