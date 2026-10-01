using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest
{
    /// <summary>
    /// The parametric sections against the closed formulas of their shapes
    /// </summary>
    [TestClass]
    public class ParametricSectionsTest
    {
        private static void Rel(double expected, double actual, double tolerance = 1e-9) =>
            Assert.AreEqual(expected, actual, tolerance * Math.Max(1.0, Math.Abs(expected)), $"expected {expected}, actual {actual}");

        [TestMethod]
        public void Ellipses()
        {
            // the polygon of 256 sides is smaller than the ellipse by 1e-4
            double a = 100, b = 50;
            var ellipse = new SectionEllipse(2 * a, 2 * b);
            Rel(Math.PI * a * b, ellipse.Area, 2e-4);
            Rel(Math.PI * a * Math.Pow(b, 3) / 4, ellipse.Jxx, 4e-4);
            Rel(Math.PI * Math.Pow(a, 3) * b / 4, ellipse.Jyy, 4e-4);
            Rel(4 * a * b * b / 3, ellipse.WplX, 4e-4);
            Rel(a, ellipse.Centroid.X);
            Rel(b, ellipse.Centroid.Y);
            Assert.IsTrue(ellipse.IsDoubleSymmetric);

            // exact torsion of the solid ellipse, confirmed by the finite elements on the polygon
            double jt = Math.PI * Math.Pow(a, 3) * Math.Pow(b, 3) / (a * a + b * b);
            Rel(jt, ellipse.Jt);
            Assert.AreEqual(PropertyAvailability.Exact, ellipse.GetAvailability(SectionProperty.TorsionConstant));
            SectionTorsionProperties torsion = ellipse.CalculateTorsionProperties();
            Rel(jt, torsion.TorsionConstant, 1e-3);
            Rel(ellipse.Jw, torsion.WarpingConstant, 1e-2);

            // hollow (EHS): the inner ellipse with the semi-axes a - t and b - t, torsion from the finite elements
            var hollow = new SectionEllipse(2 * a, 2 * b, 10, "EHS 200 x 100 x 10");
            Rel(Math.PI * (a * b - (a - 10) * (b - 10)), hollow.Area, 2e-4);
            Assert.AreEqual(PropertyAvailability.Numerical, hollow.GetAvailability(SectionProperty.TorsionConstant));
            Assert.IsTrue(hollow.Jt > 0);
            Rel(a, hollow.ShearCenter.X);
            Assert.ThrowsException<ArgumentException>(() => new SectionEllipse(200, 100, 50));
        }

        [TestMethod]
        public void Stadiums()
        {
            // 300 x 100: a rectangle 200 x 100 and two half circles of radius 50
            double r = 50, d = 100 + 4 * r / (3 * Math.PI), half = Math.PI * r * r / 2;
            var oval = new SectionStadium(300, 100);
            Rel(200 * 100 + Math.PI * r * r, oval.Area, 2e-4);
            Rel(200 * Math.Pow(100, 3) / 12 + Math.PI * Math.Pow(r, 4) / 4, oval.Jxx, 4e-4);
            double halfCircle = Math.PI * Math.Pow(r, 4) / 8 - half * Math.Pow(4 * r / (3 * Math.PI), 2);
            Rel(100 * Math.Pow(200, 3) / 12 + 2 * (halfCircle + half * d * d), oval.Jyy, 4e-4);
            Assert.IsTrue(oval.IsDoubleSymmetric);

            var vertical = new SectionStadium(100, 300, 10);
            Rel(150, vertical.Centroid.Y, 1e-9);
            Rel(200 * 100 + Math.PI * r * r - (200 * 80 + Math.PI * 40 * 40), vertical.Area, 4e-4);
        }

        [TestMethod]
        public void RegularPolygons()
        {
            // hexagon: exact polygon
            double r = 100;
            var hexagon = new SectionRegularPolygon(6, 2 * r);
            Rel(1.5 * Math.Sqrt(3) * r * r, hexagon.Area);
            Rel(5 * Math.Sqrt(3) / 16 * Math.Pow(r, 4), hexagon.Jxx);
            Rel(5 * Math.Sqrt(3) / 16 * Math.Pow(r, 4), hexagon.Jyy);
            Assert.IsTrue(hexagon.IsDoubleSymmetric);

            // square from the distance across the flats: the torsion of the square, 0.1406 a⁴
            var square = SectionRegularPolygon.FromAcrossFlats(4, 100);
            Rel(10000, square.Area);
            Rel(100, square.Width);
            Rel(0.140577 * 1e8, square.Jt, 1e-3);
            Assert.AreEqual(PropertyAvailability.Numerical, square.GetAvailability(SectionProperty.TorsionConstant));

            // triangle: symmetric only about the vertical axis
            var triangle = new SectionRegularPolygon(3, 200);
            Assert.IsTrue(triangle.IsSymmetricAlongYLocalAxis);
            Assert.IsFalse(triangle.IsSymmetricAlongXLocalAxis);

            // hollow octagon: the inner polygon with the sides at the thickness
            var octagon = SectionRegularPolygon.FromAcrossFlats(8, 300, 10);
            double area = 2 * Math.Sqrt(2) * Math.Pow(300 / 2 / Math.Cos(Math.PI / 8), 2) - 2 * Math.Sqrt(2) * Math.Pow(280 / 2 / Math.Cos(Math.PI / 8), 2);
            Rel(area, octagon.Area);
            Rel(150, octagon.ShearCenter.Y, 1e-9);
        }

        [TestMethod]
        public void TrapezoidsAndTriangles()
        {
            var trapezoid = new SectionTrapezoid(300, 100, 200);
            Rel(40000, trapezoid.Area);
            Rel(200 * (300 + 2 * 100) / (3.0 * (300 + 100)), trapezoid.Centroid.Y);
            Assert.IsTrue(trapezoid.IsSymmetricAlongYLocalAxis);
            Assert.IsFalse(new SectionTrapezoid(300, 100, 200, 50).IsSymmetricAlongYLocalAxis);

            // equilateral triangle: It = √3 a⁴ / 80 from the finite elements, shear centre in the centroid
            double side = 100;
            var triangle = SectionTrapezoid.Triangle(side, side * Math.Sqrt(3) / 2);
            Rel(Math.Sqrt(3) * side * side / 4, triangle.Area);
            Rel(Math.Sqrt(3) * Math.Pow(side, 4) / 80, triangle.Jt, 1e-3);
            Rel(side / 2, triangle.ShearCenter.X, 1e-6);
            Rel(triangle.Centroid.Y, triangle.ShearCenter.Y, 1e-6);
        }

        [TestMethod]
        public void BoxGirders()
        {
            // closed steel box 620 x 1000, plates 20: the rectangle minus the cell
            var box = new SectionBoxGirder(1000, 620, 20, 20, 2, 20, 600, 600, name: "box");
            Rel(620 * 1000 - 580 * 960, box.Area);
            Rel((620 * Math.Pow(1000, 3) - 580 * Math.Pow(960, 3)) / 12, box.Jxx);
            Rel(310, box.Centroid.X);
            Assert.IsTrue(box.IsDoubleSymmetric);
            // the closed cell: Bredt on the middle lines 600 x 980, within the thick-wall effect
            double am = 600 * 980, bredt = 4 * am * am / (2 * 600 / 20.0 + 2 * 980 / 20.0);
            Rel(bredt, box.Jt, 0.03);
            Assert.AreEqual(PropertyAvailability.Numerical, box.GetAvailability(SectionProperty.TorsionConstant));

            // two cells, vertical webs 400, cantilevers 1300 tapered from 250 to 200, haunches 500 x 200 and 300 x 150 in the cells
            var deck = new SectionBoxGirder(2000, 8000, 250, 200, 3, 400, 5000, 5000, 200, 0, 500, 200, 300, 150, "2 cells");
            Assert.AreEqual(2, deck.Cells);
            Rel(5400 * 250 + 2 * 225 * 1300 + 3 * 400 * 1550 + 5400 * 200 + 2 * (2 * 500 * 200 / 2.0 + 2 * 300 * 150 / 2.0), deck.Area);
            Assert.IsTrue(deck.IsSymmetricAlongYLocalAxis);
            Rel(4000, deck.ShearCenter.X, 1e-6);

            // trapezoidal: the webs inclined, the bottom slab narrower
            var trapezoidal = new SectionBoxGirder(2500, 10000, 250, 250, 2, 450, 6000, 4000, 200, 300);
            Rel(5000, trapezoidal.Centroid.X, 1e-9);
            Assert.IsTrue(trapezoidal.Jt > 0);

            Assert.ThrowsException<ArgumentException>(() => new SectionBoxGirder(1000, 620, 20, 20, 1, 20, 600, 600));
            Assert.ThrowsException<ArgumentException>(() => new SectionBoxGirder(1000, 500, 20, 20, 2, 20, 600, 600));
        }

        [TestMethod]
        public void HaunchedIAgainstAASHTOTypeIV()
        {
            // AASHTO Type IV (inches): A = 789, yb = 24.73, I = 260741
            var girder = new SectionHaunchedI(54, 8, 20, 8, 6, 26, 8, 9, "AASHTO IV");
            Rel(789, girder.Area);
            Rel(19515.0 / 789, girder.Centroid.Y);
            Rel(743421.5 - 19515.0 * 19515.0 / 789, girder.Jxx);
            Rel(260741, girder.Jxx, 1e-5);
            Rel(13, girder.Centroid.X);

            // a T: the bottom flange as wide as the web, without thickness
            var tee = new SectionHaunchedI(1000, 200, 1200, 150, 100, 200, 0, 0, "T");
            Rel(1200 * 150 + 2 * 500 * 100 / 2.0 + 200 * 850, tee.Area);
            Assert.ThrowsException<ArgumentException>(() => new SectionHaunchedI(100, 20, 10, 10, 0, 50, 10, 0));
        }

        [TestMethod]
        public void UBeamsHollowCoresAndDecks()
        {
            // U 1600 high, 1200 at the bottom, 2000 at the top, webs 150, lips 300 x 100
            var u = new SectionUBeam(1600, 1200, 2000, 200, 150, 300, 100);
            double shift = 150 * Math.Sqrt(1600 * 1600 + 400 * 400) / 1600;
            double inner = (1300 - 2 * shift + 2000 - 2 * shift) / 2 * 1400;
            Rel((1200 + 2000) / 2.0 * 1600 - inner + 2 * (300 * 100 + 25 * 100 / 2.0), u.Area);
            Assert.IsTrue(u.IsSymmetricAlongYLocalAxis);
            Rel(1300, u.Centroid.X, 1e-9);

            // hollow core 1200 x 200 with 6 circular voids Ø150 at 190
            var slab = new SectionHollowCore(1200, 200, 6, 150, 150, 190);
            double circle = 128 * 75 * 75 * Math.Sin(2 * Math.PI / 256);
            Rel(240000 - 6 * circle, slab.Area);
            Assert.IsTrue(slab.IsDoubleSymmetric);
            Assert.ThrowsException<ArgumentException>(() => new SectionHollowCore(1200, 200, 6, 150, 150, 140));

            // double T 2400 x 600 (slab 50, webs 150 to 100), with haunches 100 x 50
            var tt = new SectionMultiWebDeck(2400, 50, 2, 1200, 150, 100, 550, 100, 50, "TT");
            Rel(2400 * 50 + 2 * (150 + 100) / 2.0 * 550 + 4 * 100 * 50 / 2.0, tt.Area);
            Rel(1200, tt.Centroid.X, 1e-9);
            Assert.AreEqual(2, tt.Webs);
            var t = new SectionMultiWebDeck(1500, 200, 1, 0, 400, 400, 800);
            Rel(1500 * 200 + 400 * 800, t.Area);
            Assert.ThrowsException<ArgumentException>(() => new SectionMultiWebDeck(2400, 50, 2, 100, 150, 100, 550));
        }

        [TestMethod]
        public void SteelSectionsAndSerialization()
        {
            // before, the base class threw NotImplementedException in SetEdgeTypeFromSteelType
            var steel = new SteelSection(new SectionEllipse(200, 100, 8, "EHS"), SteelMaterialEN1993Data.S355);
            Assert.IsTrue(steel.Area > 0);

#pragma warning disable SYSLIB0011
            var polygon = SectionRegularPolygon.FromAcrossFlats(12, 400, 6, "POLE");
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var stream = new System.IO.MemoryStream())
            {
                formatter.Serialize(stream, polygon);
                stream.Position = 0;
                var copy = (SectionRegularPolygon)formatter.Deserialize(stream);
                Assert.AreEqual(12, copy.Sides);
                Rel(polygon.Area, copy.Area);
                Rel(polygon.Jt, copy.Jt, 1e-12);
                Assert.IsTrue(polygon.Equals(copy));
            }
#pragma warning restore SYSLIB0011
        }
    }
}
