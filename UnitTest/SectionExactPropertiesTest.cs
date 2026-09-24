using GPC.Geometry;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace ModelObjectTest
{
    /// <summary>
    /// Properties of the generic sections integrated exactly on the boundary of the shape (September 2026)
    /// </summary>
    [TestClass]
    public class SectionExactPropertiesTest
    {
        private static Point2d Q(double x, double y) => new Point2d(x, y);

        private static void AssertRelative(double expected, double actual, double relativeTolerance = 1e-9)
        {
            Assert.AreEqual(expected, actual, System.Math.Abs(expected) * relativeTolerance + 1e-9);
        }

        [TestMethod]
        public void GenericLSection()
        {
            // L 150 x 200, thickness 20: horizontal leg 150 x 20, vertical leg 20 x 180
            var shape = new Shape2d(new Polygon2d(new[] { Q(0, 0), Q(150, 0), Q(150, 20), Q(20, 20), Q(20, 200), Q(0, 200) }));
            var section = new Section(shape, "L");
            section.SetMechanicalProperties();

            double a1 = 150.0 * 20.0, x1 = 75.0, y1 = 10.0;
            double a2 = 20.0 * 180.0, x2 = 10.0, y2 = 110.0;
            double area = a1 + a2;
            double xc = (a1 * x1 + a2 * x2) / area;
            double yc = (a1 * y1 + a2 * y2) / area;
            double jxx = 150.0 * 20.0 * 20.0 * 20.0 / 12.0 + a1 * (y1 - yc) * (y1 - yc) + 20.0 * 180.0 * 180.0 * 180.0 / 12.0 + a2 * (y2 - yc) * (y2 - yc);
            double jyy = 20.0 * 150.0 * 150.0 * 150.0 / 12.0 + a1 * (x1 - xc) * (x1 - xc) + 180.0 * 20.0 * 20.0 * 20.0 / 12.0 + a2 * (x2 - xc) * (x2 - xc);
            double jxy = a1 * (x1 - xc) * (y1 - yc) + a2 * (x2 - xc) * (y2 - yc);

            AssertRelative(area, section.Area);
            AssertRelative(xc, section.Centroid.X);
            AssertRelative(yc, section.Centroid.Y);
            AssertRelative(jxx, section.Jxx);
            AssertRelative(jyy, section.Jyy);
            AssertRelative(jxy, section.Jxy);
        }

        [TestMethod]
        public void GenericHollowSectionDoesNotDependOnTheOrientation()
        {
            var fill = new Polygon2d(new[] { Q(0, 0), Q(300, 0), Q(300, 500), Q(0, 500) });
            var holeCounterclockwise = new Polygon2d(new[] { Q(50, 100), Q(250, 100), Q(250, 400), Q(50, 400) });
            var holeClockwise = new Polygon2d(holeCounterclockwise.Points.Reverse().ToArray());

            var section1 = new Section(new Shape2d(fill, new[] { holeCounterclockwise }), "1");
            var section2 = new Section(new Shape2d(new Polygon2d(fill.Points.Reverse().ToArray()), new[] { holeClockwise }), "2");
            section1.SetMechanicalProperties();
            section2.SetMechanicalProperties();

            double area = 300.0 * 500.0 - 200.0 * 300.0;
            double jxx = 300.0 * 500.0 * 500.0 * 500.0 / 12.0 - 200.0 * 300.0 * 300.0 * 300.0 / 12.0;
            double jyy = 500.0 * 300.0 * 300.0 * 300.0 / 12.0 - 300.0 * 200.0 * 200.0 * 200.0 / 12.0;

            foreach (var section in new[] { section1, section2 })
            {
                AssertRelative(area, section.Area);
                AssertRelative(150.0, section.Centroid.X);
                AssertRelative(250.0, section.Centroid.Y);
                AssertRelative(jxx, section.Jxx);
                AssertRelative(jyy, section.Jyy);
                Assert.AreEqual(0.0, section.Jxy);
            }
        }

        [TestMethod]
        public void StaticMomentsOfTheMeshEqualTheExactOnes()
        {
            var shape = new Shape2d(new Polygon2d(new[] { Q(0, 500), Q(0, 600), Q(400, 600), Q(400, 500), Q(250, 500), Q(250, 0), Q(150, 0), Q(150, 500) }));
            var section = new Section(shape, "T");
            section.SetMechanicalProperties();

            SectionHelper.CalculateStaticMoments(section.Mesh, out double sxMesh, out double syMesh);
            SectionHelper.CalculateStaticMoments(shape, out double sx, out double sy);

            AssertRelative(sx, sxMesh, 1e-9);
            AssertRelative(sy, syMesh, 1e-9);
            AssertRelative(section.Area * section.Centroid.Y, sx, 1e-9);
            AssertRelative(section.Area * section.Centroid.X, sy, 1e-9);
        }
    }
}
