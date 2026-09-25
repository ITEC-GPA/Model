using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// Corrections of the calculation of the section properties (September 2026)
    /// </summary>
    [TestClass]
    public class SectionCalculationTest
    {
        private static Point2d Q(double x, double y) => new Point2d(x, y);

        private static Section Generic(params Point2d[] points)
        {
            var section = new Section(new Shape2d(new Polygon2d(points)));
            section.SetMechanicalProperties();
            return section;
        }

        private static Section Generic(Shape2d shape)
        {
            var section = new Section(shape);
            section.SetMechanicalProperties();
            return section;
        }

        private static Polygon2d Rectangle(double x0, double y0, double x1, double y1) => new Polygon2d(new[] { Q(x0, y0), Q(x1, y0), Q(x1, y1), Q(x0, y1) });

        private static Polygon2d Circle(double diameter, int sides) =>
            new Polygon2d(Enumerable.Range(0, sides).Select(i => Q(diameter / 2 * Math.Cos(2 * Math.PI * i / sides), diameter / 2 * Math.Sin(2 * Math.PI * i / sides))).ToArray());

        #region Plastic moduli of a generic shape

        [TestMethod]
        public void PlasticModuliOfARectangle()
        {
            // before, the plastic moduli of a generic shape were the elastic ones (b h^2 / 6 instead of b h^2 / 4)
            double b = 300, h = 500;
            Section section = Generic(Rectangle(0, 0, b, h).ToArray());

            Assert.AreEqual(b * h * h / 4, section.Wpl1, 1e-9 * b * h * h);
            Assert.AreEqual(h * b * b / 4, section.Wpl2, 1e-9 * b * h * h);
            Assert.AreEqual(b * h * h / 4, section.WplX, 1e-9 * b * h * h);
            Assert.AreEqual(h * b * b / 4, section.WplY, 1e-9 * b * h * h);
        }

        [TestMethod]
        public void PlasticModulusOfATSection()
        {
            // flange 200 x 20 on a web 20 x 180 (area 4000 + 3600): the plastic neutral axis is in the flange, 19 from its top (y = 181)
            Section section = Generic(Q(-100, 180), Q(-10, 180), Q(-10, 0), Q(10, 0), Q(10, 180), Q(100, 180), Q(100, 200), Q(-100, 200));
            // above: 200 x 19 at 9.5; below: flange 200 x 1 at 0.5 and web 20 x 180 at 181 - 90
            double expected = 200 * 19 * 9.5 + 200 * 1 * 0.5 + 20 * 180 * (181 - 90);
            Assert.AreEqual(expected, section.WplX, 1e-9 * expected);
            Assert.AreEqual(expected, section.Wpl1, 1e-9 * expected);

            // respect to Y the section is symmetric: flange 2 x (20 x 100^2 / 2) + web 2 x (180 x 10^2 / 2)
            double expectedY = 2 * 20 * 100 * 100 / 2.0 + 2 * 180 * 10 * 10 / 2.0;
            Assert.AreEqual(expectedY, section.WplY, 1e-9 * expectedY);
        }

        [TestMethod]
        public void PlasticModulusOfATubeAndOfAnIsland()
        {
            // tube: (D^3 - d^3) / 6 for the circles (polygons with many sides)
            double D = 400, d = 300;
            Section tube = Generic(new Shape2d(Circle(D, 720), new[] { Circle(d, 720) }));
            Assert.AreEqual((D * D * D - d * d * d) / 6, tube.WplX, 1e-4 * D * D * D);
            Assert.AreEqual(tube.WplX, tube.WplY, 1e-6 * tube.WplX);

            // square 100 with a hole 60 and an island 20 inside it: symmetric, plastic modulus = sum of |y| dA
            var shape = new Shape2d(Rectangle(-50, -50, 50, 50), new[] { Rectangle(-30, -30, 30, 30) }, new[] { new Shape2d(Rectangle(-10, -10, 10, 10)) });
            Section section = Generic(shape);
            // centred square of side a: integral of |y| dA = a^3 / 4
            double expected = Math.Pow(100, 3) / 4 - Math.Pow(60, 3) / 4 + Math.Pow(20, 3) / 4;
            Assert.AreEqual(expected, section.WplX, 1e-9 * expected);
        }

        [TestMethod]
        public void PlasticModulusIsKeptBySerialization()
        {
            Section section = Generic(Q(0, 0), Q(300, 0), Q(300, 500), Q(0, 500));
            var formatter = new BinaryFormatter();
            using (var stream = new MemoryStream())
            {
#pragma warning disable SYSLIB0011
                formatter.Serialize(stream, section);
                stream.Position = 0;
                var copy = (Section)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
                Assert.AreEqual(300 * 500 * 500 / 4.0, copy.Wpl1, 1e-6);
            }
        }

        #endregion

        #region Elastic moduli, principal axes

        [TestMethod]
        public void Wel2MaxIsTheOneOfTheMaximumDistance()
        {
            // L section: the distances respect to the axis 2 are different on the two sides
            Section section = Generic(Q(0, 0), Q(200, 0), Q(200, 20), Q(20, 20), Q(20, 100), Q(0, 100));
            Assert.AreNotEqual(section.Wel2Min, section.Wel2Max, "it returned Wel2Min");
            Assert.AreEqual(Math.Min(section.Wel2Max, section.Wel2Min), section.Wel2);
        }

        [TestMethod]
        public void ElasticModuliRespectToXUseJxx()
        {
            // rectangle wider than high: before, J11 was the moment respect to Y and WelX = J11 / (h / 2)
            double b = 400, h = 100;
            Section section = Generic(Rectangle(0, 0, b, h).ToArray());

            Assert.AreEqual(b * h * h / 6, section.WelX, 1e-9 * b * h * h);
            Assert.AreEqual(h * b * b / 6, section.WelY, 1e-9 * b * b * h);

            // the X axis is principal: it is the axis 1 (the convention of the constructor with J11 and J22)
            Assert.AreEqual(0, section.AngleX1);
            Assert.AreEqual(b * h * h * h / 12, section.J11, 1e-9 * b * h * h * h);
            Assert.AreEqual(section.WelX, section.Wel1, 1e-9 * section.WelX);
        }

        [TestMethod]
        public void RotatedShapeElasticModuliRespectToX()
        {
            // square rotated by 45°: WelX = Jxx / (half diagonal)
            double a = 100, half = a / Math.Sqrt(2);
            Section section = Generic(Q(0, -half), Q(half, 0), Q(0, half), Q(-half, 0));
            double jxx = a * a * a * a / 12;
            Assert.AreEqual(jxx / half, section.WelX, 1e-9 * jxx / half);
            Assert.AreEqual(jxx / half, section.WelY, 1e-9 * jxx / half);
        }

        [TestMethod]
        public void ProductOfInertiaThresholdIsRelative()
        {
            // L 50 x 5 in metres: Jxy is about 1e-8 m^4 (before, |Jxy| < 100000 was set to zero: principal axes not rotated)
            Section section = Generic(Q(0, 0), Q(0.05, 0), Q(0.05, 0.005), Q(0.005, 0.005), Q(0.005, 0.05), Q(0, 0.05));
            Assert.IsTrue(section.Jxy < 0 && Math.Abs(section.Jxy) > 1e-9, $"Jxy {section.Jxy}");
            Assert.AreEqual(Math.PI / 4, Math.Abs(section.AngleX1), 1e-6, "equal legs: principal axes at 45°");
            Assert.AreEqual(section.Jxx + section.Jyy, section.J11 + section.J22, 1e-12);
            Assert.IsTrue(section.J11 > section.Jxx);
        }

        [TestMethod]
        public void ConstructorWithThePrincipalMoments()
        {
            // before, J11, J22 and Jp were zero and Jxx, Jyy were J11, J22 also with rotated principal axes
            double j11 = 5e8, j22 = 2e8, angle = 0.3;
            var section = new Section(12000, j11, j22, 1e6, 0, Q(0, 0), new Point3d(0, 0, 0), angle, "principal");

            Assert.AreEqual(j11, section.J11);
            Assert.AreEqual(j22, section.J22);
            Assert.AreEqual(j11 + j22, section.Jp, 1e-6);
            Assert.AreEqual(j11 + j22, section.Jxx + section.Jyy, 1e-6);

            // the X-Y moments give back the principal ones and the angle
            double c = Math.Cos(angle), s = Math.Sin(angle);
            Assert.AreEqual(j11, section.Jxx * c * c + section.Jyy * s * s - 2 * section.Jxy * s * c, 1e-6);
            Assert.AreEqual(angle, -0.5 * Math.Atan2(2 * section.Jxy, section.Jxx - section.Jyy), 1e-12);

            var notRotated = new Section(12000, j11, j22, 1e6, 0, Q(0, 0), new Point3d(0, 0, 0), 0, "principal");
            Assert.AreEqual(j11, notRotated.Jxx);
            Assert.AreEqual(j22, notRotated.Jyy);
            Assert.AreEqual(0, notRotated.Jxy);
        }

        [TestMethod]
        public void WideSectionsHaveTheAxis1AlongX()
        {
            // RHS lying and wide rectangle: the moduli respect to the axis 1 are computed respect to X (Wel1 = J11 / distance from the
            // bottom): J11 must be Jxx (before, J11 = Jyy, the bigger one, and Wel1 = Jyy / (h / 2))
            var rhs = new SectionRHS(100, 200, 10, 10, 10, 10, "RHS 200x100");
            Assert.AreEqual(rhs.Jxx, rhs.J11);
            Assert.AreEqual(0, rhs.AngleX1);
            Assert.AreEqual(rhs.J11 / 50, rhs.Wel1, 1e-9 * rhs.Wel1);

            var rectangle = new SectionRectangular(100, 400, "400x100");
            Assert.AreEqual(400.0 * 100 * 100 * 100 / 12, rectangle.J11, 1e-6);
            Assert.AreEqual(rectangle.J11 / 50, rectangle.Wel1, 1e-9 * rectangle.Wel1);
        }

        #endregion

        #region Other sections

        [TestMethod]
        public void CircularSectionsModuliRespectToXY()
        {
            // before, not set: zero
            double d = 100;
            var circle = new SectionCircular(d);
            Assert.AreEqual(Math.PI * d * d * d / 32, circle.WelX, 1e-9 * d * d * d);
            Assert.AreEqual(Math.PI * d * d * d / 32, circle.WelY, 1e-9 * d * d * d);
            Assert.AreEqual(d * d * d / 6, circle.WplX, 1e-9 * d * d * d);
            Assert.AreEqual(d * d * d / 6, circle.WplY, 1e-9 * d * d * d);

            var tube = new SectionCHS(100, 5);
            Assert.AreEqual(tube.Wel1, tube.WelX);
            Assert.AreEqual(tube.Wpl1, tube.WplY);
        }

        [TestMethod]
        public void SteelSectionRadiiOfGyration()
        {
            // before, Rxx and Ryy returned Rxy (zero for a symmetric section)
            var shape = new SectionH(300, 7.1, 150, 10.7, 150, 10.7, "IPE300");
            var steel = new SteelSection(shape, SteelMaterialEN1993Data.S355);
            Assert.AreEqual(shape.Rxx, steel.Rxx);
            Assert.AreEqual(shape.Ryy, steel.Ryy);
            Assert.IsTrue(steel.Rxx > 0 && steel.Ryy > 0);

            // Rxy with the sign of Jxy (before, NaN when negative)
            Section l = Generic(Q(0, 0), Q(50, 0), Q(50, 5), Q(5, 5), Q(5, 50), Q(0, 50));
            Assert.IsFalse(double.IsNaN(l.Rxy));
            Assert.AreEqual(Math.Sign(l.Jxy), Math.Sign(l.Rxy));
        }

        [TestMethod]
        public void EqualityOperatorWithNull()
        {
            // before, NullReferenceException when the left operand was null
            Section none = null;
            Section section = Generic(Q(0, 0), Q(1, 0), Q(1, 1));
            Assert.IsFalse(none == section);
            Assert.IsTrue(none != section);
            Assert.IsTrue(none == null);
        }

        [TestMethod]
        public void PropertiesAfterAChangeUseTheNewShape()
        {
            // before, the shape was reset after SetMechanicalProperties: the properties computed on it used the old one
            var changed = new SectionH(300, 7.1, 150, 10.7, 150, 10.7, "IPE300");
            _ = changed.Shape;
            changed.Height = 400;
            var built = new SectionH(400, 7.1, 150, 10.7, 150, 10.7, "IPE400");

            Assert.AreEqual(built.WelXMax, changed.WelXMax, 1e-9 * built.WelXMax);
            Assert.AreEqual(built.WelYMax, changed.WelYMax, 1e-9 * built.WelYMax);
            Assert.AreEqual(built.Shape.GetArea(), changed.Shape.GetArea(), 1e-9);
        }

        #endregion
    }
}
