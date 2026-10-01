using GPC.Geometry;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest
{
    /// <summary>
    /// The welded sections (parts joined along their sides) and the built-up ones (parts connected at points)
    /// </summary>
    [TestClass]
    public class WeldedSectionsTest
    {
        private static void Rel(double expected, double actual, double tolerance = 1e-9) =>
            Assert.AreEqual(expected, actual, tolerance * Math.Max(1.0, Math.Abs(expected)), $"expected {expected}, actual {actual}");

        private static double RectangleTorsion(double a, double b)
        {
            double sum = 0;
            for (int n = 1; n < 200; n += 2)
                sum += Math.Tanh(n * Math.PI * a / (2 * b)) / Math.Pow(n, 5);
            return a * Math.Pow(b, 3) * (1.0 / 3.0 - 64.0 * b / (Math.Pow(Math.PI, 5) * a) * sum);
        }

        [TestMethod]
        public void CoverPlatedH()
        {
            var beam = new SectionH(300, 11, 300, 19, 300, 19, "HE 300 B sharp");
            var plated = SectionWelded.CoverPlated(beam, 320, 20, name: "HEB + plates");
            Rel(beam.Area + 2 * 320 * 20, plated.Area);
            Rel(170, plated.Centroid.Y);
            Rel(160, plated.Centroid.X);
            Rel(beam.Jxx + 2 * (320 * Math.Pow(20, 3) / 12 + 320 * 20 * 160 * 160), plated.Jxx);
            Assert.IsTrue(plated.IsDoubleSymmetric);
            // torsion of the joined parts: more than the H, the plates welded on the flanges
            Assert.IsTrue(plated.Jt > beam.CalculateTorsionProperties().TorsionConstant + 2 * RectangleTorsion(320, 20));
            Rel(160, plated.ShearCenter.X);
            Rel(170, plated.ShearCenter.Y);
            Assert.AreEqual(PropertyAvailability.Numerical, plated.GetAvailability(SectionProperty.WarpingConstant));
        }

        [TestMethod]
        public void ChannelBoxIsClosed()
        {
            // two channels 200 x 75 welded toe to toe: a closed cell, many times the two open channels (Bredt on the middle lines)
            var channel = new SectionC(200, 8.5, 75, 11.5, 75, 11.5, "C 200 sharp");
            var box = SectionWelded.ChannelBox(channel, "box");
            Rel(2 * channel.Area, box.Area);
            double open = 2 * channel.CalculateTorsionProperties().TorsionConstant;
            double am = (150 - 8.5) * (200 - 11.5), bredt = 4 * am * am / (2 * (150 - 8.5) / 11.5 + 2 * (200 - 11.5) / 8.5);
            Assert.IsTrue(box.Jt > 20 * open, $"{box.Jt} {open}");
            Rel(bredt, box.Jt, 0.1);
            Rel(0, box.ShearCenter.X, 1e-9);
        }

        [TestMethod]
        public void Cruciforms()
        {
            // plates 300 x 300 x 20: the thin-walled torsion Σ b t³ / 3 within the effects of the ends and of the joint
            var plates = SectionWelded.CruciformPlates(300, 300, 20, "+");
            Rel(300 * 20 + 280 * 20, plates.Area);
            Assert.IsTrue(plates.IsDoubleSymmetric);
            Rel((300 + 280) * Math.Pow(20, 3) / 3, plates.Jt, 0.05);
            Rel(0, plates.ShearCenter.X, 1e-9);
            Rel(0, plates.ShearCenter.Y, 1e-9);

            // four angles L 100 x 10 battened with a gap of 10: parts connected at points, warping not available
            var angle = new SectionL(100, 10, 100, 10, "L 100 x 10");
            var star = SectionBuiltUp.Cruciform(angle, 10, "4L");
            Rel(4 * angle.Area, star.Area);
            Assert.IsTrue(star.IsDoubleSymmetric);
            Assert.AreEqual(8, star.ThinWalls.Length);
            Assert.AreEqual(PropertyAvailability.NotAvailable, star.GetAvailability(SectionProperty.WarpingConstant));
            Rel(4 * angle.Jt, star.Jt);
        }

        [TestMethod]
        public void SeparateRegions()
        {
            // two rectangles outside one another: a single shape cannot hold them
            Shape2d Rectangle(double x) => new Shape2d(new Polygon2d(new[] { new Point2d(x, 0), new Point2d(x + 100, 0), new Point2d(x + 100, 200), new Point2d(x, 200) }));
            var regions = SectionBuiltUp.FromRegions(new[] { Rectangle(0), Rectangle(300) }, "two");
            Rel(40000, regions.Area);
            Rel(200, regions.Centroid.X);
            Rel(2 * RectangleTorsion(200, 100), regions.Jt, 1e-3);
            Rel(2 * 100 * Math.Pow(200, 3) / 12, regions.Jxx);
        }
    }
}
