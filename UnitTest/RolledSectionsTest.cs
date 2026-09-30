using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// The sections of the rolled and cold formed profiles: the analytic properties (thin walls plus corners) against the integration of
    /// the exact outline, an independent calculation of the same geometry
    /// </summary>
    [TestClass]
    public class RolledSectionsTest
    {
        /// <summary>
        /// The properties of the outline of a section: area, centroid and moments of inertia integrated on its boundary
        /// </summary>
        private static (double Area, Point2d Centroid, double Jxx, double Jyy, double Jxy) OfTheOutline(Section section)
        {
            var shape = new Section(section.Shape, "outline");
            shape.SetMechanicalProperties();
            return (shape.Area, shape.Centroid, shape.Jxx, shape.Jyy, shape.Jxy);
        }

        [TestMethod]
        public void RoundedCornersOfTheHollowSections()
        {
            // SHS 60 x 60 x 8 and RHS 80 x 40 x 8 of EN 10210-2 (ro = 1.5 t, ri = t): the thick walls with the largest deviation from the
            // published moments of inertia (1.3%); the analytic corners agree with the outline within the discretisation of the arcs
            foreach (var (h, b, t) in new[] { (60.0, 60.0, 8.0), (80.0, 40.0, 8.0), (400.0, 200.0, 10.0) })
            {
                var rhs = new SectionRHSRoundedCorners(h, b, t, 1.5 * t, t, "RHS");
                var outline = OfTheOutline(rhs);
                double area = 2 * t * (h + b - 2 * t) - (4 - Math.PI) * (2.25 * t * t - t * t);
                Assert.AreEqual(area, rhs.Area, 1e-9 * area);
                Assert.AreEqual(rhs.Area, outline.Area, 1e-4 * rhs.Area);
                Assert.AreEqual(rhs.Jxx, outline.Jxx, 1e-4 * rhs.Jxx);
                Assert.AreEqual(rhs.Jyy, outline.Jyy, 1e-4 * rhs.Jyy);
                Assert.AreEqual(b / 2, rhs.Centroid.X, 1e-9 * b);
                Assert.AreEqual(h / 2, rhs.Centroid.Y, 1e-9 * h);
                Assert.AreEqual(0, rhs.Jxy, 1e-9 * rhs.Jxx);
                Assert.AreEqual(0, rhs.Jw);
            }

            // sharp corners: the rectangle with a rectangular hole, torsion of EN 10210-2 with Rc = 0 = Bredt on the middle lines
            var sharp = new SectionRHSRoundedCorners(200, 100, 10, 0, 0, "sharp");
            Assert.AreEqual(200 * 100 - 180 * 80, sharp.Area, 1e-9);
            Assert.AreEqual((100 * Math.Pow(200, 3) - 80 * Math.Pow(180, 3)) / 12, sharp.Jxx, 1e-6);
            double am = 190 * 90, p = 2 * (190 + 90);
            Assert.AreEqual(Math.Pow(10, 3) * p / 3 + 2 * (2 * am * 10 / p) * am, sharp.Jt, 1e-6 * sharp.Jt);

            Assert.ThrowsException<ArgumentException>(() => new SectionRHSRoundedCorners(100, 100, 10, 60, 10, "radius too big"));
        }

        [TestMethod]
        public void FilletsAndToeRadiiOfTheAngles()
        {
            // L 90 x 90 x 6 of EN 10056-1 (r1 = 11, r2 = r1 / 2) and an unequal angle: the corners agree with the outline
            foreach (var (b, h, t, r1) in new[] { (90.0, 90.0, 6.0, 11.0), (200.0, 100.0, 12.0, 15.0) })
            {
                var angle = new SectionL(b, t, h, t, "L", r1, r1 / 2);
                double sharpArea = angle.Area;
                angle.SetEdgeTypeFromSteelType(Section.SectionTypes.Rolled);
                angle.SetMechanicalProperties();
                Assert.AreEqual(sharpArea + (1 - Math.PI / 4) * (r1 * r1 - 2 * r1 * r1 / 4), angle.Area, 1e-9 * sharpArea);

                // the plastic moduli are the exact ones of the outline with the corners (checked against the sharp ones: bigger with the
                // root fillet, the outline has the fillet)
                var outline = new Section(SectionOutlineOf(angle), "outline");
                outline.SetMechanicalProperties();
                Assert.AreEqual(angle.Area, outline.Area, 1e-4 * angle.Area);
                Assert.AreEqual(angle.Centroid.X, outline.Centroid.X, 1e-4 * b);
                Assert.AreEqual(angle.Centroid.Y, outline.Centroid.Y, 1e-4 * h);
                Assert.AreEqual(angle.Jxx, outline.Jxx, 1e-4 * angle.Jxx);
                Assert.AreEqual(angle.Jyy, outline.Jyy, 1e-4 * angle.Jyy);
                Assert.AreEqual(angle.Jxy, outline.Jxy, 1e-4 * Math.Abs(angle.Jxy));
                Assert.AreEqual(outline.WplX, angle.WplX, 1e-4 * angle.WplX);
                Assert.AreEqual(outline.WplY, angle.WplY, 1e-4 * angle.WplY);
            }

            // the sharp angle is the one of before: the radii are not used without the working of the corners
            var sharp = new SectionL(90, 6, 90, 6, "L", 11, 5.5);
            Assert.AreEqual(90 * 6 + 84 * 6, sharp.Area, 1e-9);
        }

        [TestMethod]
        public void TaperFlangeSections()
        {
            // IPN 300 of EN 10365 (slope 14%, tf at b / 4 from the tips): A 69.0 cm², Iy 9800 cm⁴ in the sales programme
            var ipn = new SectionHTaperFlange(300, 10.8, 125, 16.2, 0.14, 10.8, 6.5, 125 / 4.0, "IPN 300");
            Assert.AreEqual(6900, ipn.Area, 0.005 * 6900);
            Assert.AreEqual(9800e4, ipn.Jxx, 0.005 * 9800e4);
            Assert.AreEqual(ipn.Width / 2, ipn.Centroid.X, 1e-9);
            Assert.AreEqual(150, ipn.Centroid.Y, 1e-6);
            Assert.AreEqual(16.2 - 0.14 * 125 / 4.0, ipn.TipThickness, 1e-12);
            Assert.IsTrue(ipn.RootThickness > ipn.ThicknessTopFlange);
            Assert.IsTrue(ipn.WplX > ipn.WelX && ipn.WplY > ipn.WelY);

            // without slope and toe radius the section of the parallel flanges with fillets
            var parallel = new SectionHTaperFlange(300, 10.8, 125, 16.2, 0, 10.8, 0, 125 / 4.0, "parallel");
            var h = new SectionH(300, 10.8, 125, 16.2, 125, 16.2, "H", 10.8);
            h.SetEdgeTypeFromSteelType(Section.SectionTypes.Rolled);
            h.SetMechanicalProperties();
            Assert.AreEqual(h.Area, parallel.Area, 1e-4 * h.Area);
            Assert.AreEqual(h.Jxx, parallel.Jxx, 1e-4 * h.Jxx);
            Assert.AreEqual(h.Jyy, parallel.Jyy, 1e-4 * h.Jyy);
            Assert.AreEqual(h.WplX, parallel.WplX, 1e-4 * h.WplX);

            // UPN 300 (slope 8%, tf at b / 2): A 58.8 cm², Iy 8030 cm⁴; the channel with parallel flanges and the same radii without slope
            var upn = new SectionCTaperFlange(300, 10, 100, 16, 0.08, 16, 8, 50, "UPN 300");
            Assert.AreEqual(5880, upn.Area, 0.005 * 5880);
            Assert.AreEqual(8030e4, upn.Jxx, 0.005 * 8030e4);
            Assert.AreEqual(0, upn.Jxy, 1e-9 * upn.Jxx);
            var c = new SectionC(300, 10, 100, 16, 100, 16, "C", 16, 8);
            c.SetEdgeTypeFromSteelType(Section.SectionTypes.Rolled);
            c.SetMechanicalProperties();
            var cFlat = new SectionCTaperFlange(300, 10, 100, 16, 0, 16, 8, 50, "flat");
            Assert.AreEqual(c.Area, cFlat.Area, 1e-4 * c.Area);
            Assert.AreEqual(c.Jyy, cFlat.Jyy, 1e-4 * c.Jyy);
            Assert.AreEqual(c.Centroid.X, cFlat.Centroid.X, 1e-4 * c.Centroid.X);
            Assert.AreEqual(c.WplY, cFlat.WplY, 1e-4 * c.WplY);

            Assert.ThrowsException<ArgumentException>(() => new SectionHTaperFlange(300, 10.8, 125, 5, 0.14, 10.8, 6.5, 125 / 4.0, "no tip"));
        }

        [TestMethod]
        public void RolledChannelsDoNotThrowAndHaveTheExactPlasticModuli()
        {
            // UPN 120 and 160 threw an exception in the closed form of Wpl,z (a T with a negative web)
            foreach (var (h, b, tw, tf, r1, r2) in new[] { (120.0, 55.0, 7.0, 9.0, 9.0, 4.5), (160.0, 65.0, 7.5, 10.5, 10.5, 5.5) })
            {
                var c = new SteelSection(new SectionC(h, tw, b, tf, b, tf, "C", r1, r2), SteelMaterialEN1993Data.S275, Section.SectionTypes.Rolled);
                Assert.IsTrue(c.Wpl2 > c.Wel2);
                Assert.IsTrue(c.Wpl1 > c.Wel1);
            }
        }

        [TestMethod]
        public void TeesWithFilletsAndTaperFlange()
        {
            // WT from a W: the two fillets in the area (before, the radius was not used)
            var sharp = new SectionT(300, 200, 10, 16, "T", 18);
            var rolled = new SectionT(300, 200, 10, 16, "T", 18);
            rolled.SetEdgeTypeFromSteelType(Section.SectionTypes.Rolled);
            rolled.SetMechanicalProperties();
            Assert.AreEqual(200 * 16 + 284 * 10, sharp.Area, 1e-9);
            Assert.AreEqual(sharp.Area + 2 * (1 - Math.PI / 4) * 18 * 18, rolled.Area, 1e-9);
            var outline = OfTheOutline(new SectionTTaperFlange(300, 200, 10, 16, 0, 18, 0, 50, "flat"));
            Assert.AreEqual(rolled.Area, outline.Area, 1e-4 * rolled.Area);
            Assert.AreEqual(rolled.Jxx, outline.Jxx, 1e-4 * rolled.Jxx);
            Assert.AreEqual(rolled.Jyy, outline.Jyy, 1e-4 * rolled.Jyy);
            Assert.AreEqual(rolled.Centroid.Y, outline.Centroid.Y, 1e-4 * rolled.Centroid.Y);

            // the taper flange: thinner at the tips, thicker at the web, the same area of the outstands when tf is at their middle
            var taper = new SectionTTaperFlange(300, 200, 10, 16, 1.0 / 6, 0, 0, 47.5, "taper");
            Assert.AreEqual(sharp.Area, taper.Area, 1e-4 * sharp.Area);
            Assert.IsTrue(taper.Jyy < sharp.Jyy);
        }

        [TestMethod]
        public void DoubleAnglesAndChannelsFromTheSingleSections()
        {
            // two angles 100 x 100 x 10 back to back at 10 mm: area and moment about X doubled, about Y moved by the spacing
            var angle = new SectionL(100, 10, 100, 10, "L");
            var twin = SectionBuiltUp.DoubleAngle(angle, 10, "2L");
            Assert.AreEqual(2 * angle.Area, twin.Area, 1e-9);
            Assert.AreEqual(0, twin.Centroid.X, 1e-9);
            Assert.AreEqual(angle.Centroid.Y, twin.Centroid.Y, 1e-9);
            Assert.AreEqual(2 * angle.Jxx, twin.Jxx, 1e-6 * twin.Jxx);
            double d = angle.Centroid.X + 5;
            Assert.AreEqual(2 * (angle.Jyy + angle.Area * d * d), twin.Jyy, 1e-6 * twin.Jyy);
            Assert.AreEqual(0, twin.Jxy, 1e-9 * twin.Jxx);
            Assert.IsTrue(twin.IsSymmetricAlongYLocalAxis);
            Assert.IsFalse(twin.IsSymmetricAlongXLocalAxis);
            Assert.AreEqual(2 * angle.Jt, twin.Jt, 1e-9);
            Assert.IsTrue(double.IsNaN(twin.Jw));
            Assert.AreEqual(210, twin.Width, 1e-9);
            Assert.AreEqual(100, twin.Height, 1e-9);
            // shear centre on the axis of symmetry, at the height of the one of the angles
            Assert.AreEqual(0, twin.ShearCenter.X, 1e-9);
            Assert.AreEqual(angle.ShearCenter.Y, twin.ShearCenter.Y, 1e-9);

            // plastic modulus about the axis of symmetry: the two halves, each with the static moment of an angle about the axis
            double wplY = 2 * angle.Area * d;
            Assert.AreEqual(wplY, twin.WplY, 1e-6 * wplY);
            // elastic modulus about Y: the tips of the horizontal legs at 105 mm
            Assert.AreEqual(twin.Jyy / 105, twin.WelY, 1e-9 * twin.WelY);

            // no spacing: the tee made of two angles is the T of the same dimensions
            var tee = new SectionT(100, 200, 20, 10, "T");
            var joined = SectionBuiltUp.DoubleAngle(new SectionL(100, 10, 100, 10, "L"), 0, "2L");
            // the T has the flange at the top: the double angle with the legs at the bottom is the same T upside down
            Assert.AreEqual(tee.Area, joined.Area, 1e-9);
            Assert.AreEqual(tee.Jxx, joined.Jxx, 1e-9 * tee.Jxx);
            Assert.AreEqual(tee.Jyy, joined.Jyy, 1e-9 * tee.Jyy);
            Assert.AreEqual(tee.WplX, joined.WplX, 1e-6 * tee.WplX);

            // two channels toe to toe without spacing: a box
            var channel = new SectionC(200, 10, 80, 10, 80, 10, "C");
            var box = SectionBuiltUp.DoubleChannel(channel, 0, false, "box");
            var rhs = new SectionRHS(200, 160, 10, 10, 10, 10, "RHS");
            Assert.AreEqual(rhs.Area, box.Area, 1e-9);
            Assert.AreEqual(rhs.Jxx, box.Jxx, 1e-9 * rhs.Jxx);
            Assert.AreEqual(rhs.Jyy, box.Jyy, 1e-9 * rhs.Jyy);
            Assert.AreEqual(rhs.WplX, box.WplX, 1e-6 * rhs.WplX);
            Assert.IsTrue(box.IsDoubleSymmetric);

            Assert.ThrowsException<NotSupportedException>(() => box.GetMesh());
            Assert.ThrowsException<ArgumentException>(() => new SectionBuiltUp(Array.Empty<SectionBuiltUp.Part>(), "empty"));
        }

        private static Shape2d SectionOutlineOf(SectionL angle)
        {
            double b = angle.HorizontalLegLength, h = angle.VerticalLegLength, t = angle.HorizontalLegThickness, r1 = angle.R, r2 = angle.R2;
            var points = new List<Point2d> { new Point2d(0, 0), new Point2d(0, h) };
            Arc(points, t - r2, h - r2, r2, 90, 0);
            Arc(points, t + r1, t + r1, r1, 180, 270);
            Arc(points, b - r2, t - r2, r2, 90, 0);
            points.Add(new Point2d(b, 0));
            return new Shape2d(new Polygon2d(points.ToArray()));
        }

        private static void Arc(List<Point2d> points, double cx, double cy, double r, double from, double to)
        {
            const int segments = 256;
            for (int k = 0; k <= segments; k++)
            {
                double a = (from + (to - from) * k / segments) * Math.PI / 180;
                points.Add(new Point2d(cx + r * Math.Cos(a), cy + r * Math.Sin(a)));
            }
        }
    }
}
