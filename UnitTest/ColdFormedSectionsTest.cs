using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UnitTest
{
    /// <summary>
    /// The cold-formed sections: the outline of the middle line with the bends, against the length of the middle line times the thickness and
    /// the thin-walled theory
    /// </summary>
    [TestClass]
    public class ColdFormedSectionsTest
    {
        private static void Rel(double expected, double actual, double tolerance = 1e-9) =>
            Assert.AreEqual(expected, actual, tolerance * Math.Max(1.0, Math.Abs(expected)), $"expected {expected}, actual {actual}");

        [TestMethod]
        public void LippedChannel()
        {
            // C 200 x 75 x 20 x 2, r = 3: the bends of 90° take rm = r + t / 2 = 4 from each part; area = t (flat parts + arcs of radius rm)
            double t = 2, rm = 4;
            var c = SectionColdFormed.LippedChannel(200, 75, 20, t, 3, "C 200 x 75 x 20 x 2");
            double flats = (198 - 2 * rm) + 2 * (73 - 2 * rm) + 2 * (19 - rm);
            Rel(t * (flats + 4 * Math.PI / 2 * rm), c.Area, 2e-4);
            Assert.AreEqual(5, c.ThinWalls.Length);
            Rel(flats, c.ThinWalls.Sum(w => w.L), 1e-9);
            Rel(200, c.Height, 1e-12);
            Rel(75, c.Width, 1e-12);
            Assert.IsTrue(c.IsSymmetricAlongXLocalAxis);
            Assert.IsFalse(c.IsSymmetricAlongYLocalAxis);
            Rel(100, c.ShearCenter.Y, 1e-9);
            Assert.IsTrue(c.ShearCenter.X < 0, "the shear centre is outside the web");
            Assert.AreEqual(PropertyAvailability.Numerical, c.GetAvailability(SectionProperty.WarpingConstant));

            // a steel section of it
            var steel = new SteelSection(c, SteelMaterialEN1993Data.S355, Section.SectionTypes.Rolled, Section.FormedTypes.ColdFormed);
            Assert.IsTrue(steel.IsColdFormed);
        }

        [TestMethod]
        public void ThinPlainChannelAgainstTheThinWalledTheory()
        {
            // middle lines b' = 99.5, h' = 199: shear centre at 3 b'² / (6 b' + h') from the middle of the web, Iw = b'³ h'² t (3 b' + 2 h') / 12 / (6 b' + h')
            double t = 1, b = 100 - t / 2, h = 200 - t;
            var c = SectionColdFormed.Channel(200, 100, t, 0.5);
            double e = 3 * b * b / (6 * b + h);
            Rel(t / 2 - e, c.ShearCenter.X, 0.02);
            double iw = t * Math.Pow(b, 3) * h * h * (3 * b + 2 * h) / (12 * (6 * b + h));
            Rel(iw, c.Jw, 0.03);
            Rel((h + 2 * b) * Math.Pow(t, 3) / 3, c.Jt, 0.05);
        }

        [TestMethod]
        public void ZedHatAndSigma()
        {
            // zed with equal flanges: the centroid in the middle of the web, principal axes rotated
            var z = SectionColdFormed.LippedZed(200, 70, 70, 20, 2, 3, "Z 200");
            Assert.IsFalse(z.IsSymmetricAlongXLocalAxis || z.IsSymmetricAlongYLocalAxis);
            Rel(70, z.Centroid.X, 1e-9); // the outer face of the bottom lip at X = 0, the middle of the web at 70 - t / 2 + t / 2
            Rel(100, z.Centroid.Y, 1e-9);
            Assert.IsTrue(Math.Abs(z.AngleX1) > 0.05);
            Rel(z.Centroid.X, z.ShearCenter.X, 1e-3);
            Rel(z.Centroid.Y, z.ShearCenter.Y, 1e-3);

            // hat: symmetric about the vertical axis
            var hat = SectionColdFormed.Hat(100, 80, 30, 1.5, 2, "omega");
            Assert.IsTrue(hat.IsSymmetricAlongYLocalAxis);
            Rel(70, hat.ShearCenter.X, 1e-9);

            // sigma: a lipped channel with the web indented; area = t (flat parts + bends of the angles of the middle line)
            double t = 2, rm = 3 + t / 2;
            var sigma = SectionColdFormed.Sigma(240, 62, 20, 15, 60, 30, t, 3, "S 240");
            Assert.IsTrue(sigma.IsSymmetricAlongXLocalAxis);
            double slope = Math.Atan2(15, 30), bends = 4 * Math.PI / 2 + 4 * slope;
            Rel(t * (sigma.ThinWalls.Sum(w => w.L) + bends * rm), sigma.Area, 2e-4);
            Assert.AreEqual(9, sigma.ThinWalls.Length);

            Assert.ThrowsException<ArgumentException>(() => SectionColdFormed.LippedChannel(200, 75, 5, 2, 10));
        }

        [TestMethod]
        public void SerializationKeepsTheOutline()
        {
#pragma warning disable SYSLIB0011
            var c = SectionColdFormed.LippedChannel(200, 75, 20, 2, 3, "C");
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var stream = new System.IO.MemoryStream())
            {
                formatter.Serialize(stream, c);
                stream.Position = 0;
                var copy = (SectionColdFormed)formatter.Deserialize(stream);
                Rel(200, copy.Height, 1e-12);
                Rel(75, copy.Width, 1e-12);
                Rel(c.Area, copy.Area);
                Assert.AreEqual(6, copy.MiddleLine.Count);
            }
#pragma warning restore SYSLIB0011
        }
    }
}
