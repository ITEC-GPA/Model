using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Standards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// Simple control tests of the sections, with the results computed by hand (September 2026)
    /// </summary>
    [TestClass]
    public class SectionControlTests
    {
        private static void Rel(double expected, double actual, double tolerance = 1e-9, string message = "") =>
            Assert.AreEqual(expected, actual, Math.Abs(expected) * tolerance + 1e-12, message);

        private static Polygon2d Rectangle(double x, double y, double width, double height) =>
            new Polygon2d(new[] { new Point2d(x, y), new Point2d(x + width, y), new Point2d(x + width, y + height), new Point2d(x, y + height) });

        [TestMethod]
        public void RectangleProperties()
        {
            // height 300, width 100: the principal axis 1 is the horizontal one
            var s = new SectionRectangular(300, 100);
            Rel(30000, s.Area);
            Rel(100 * Math.Pow(300, 3) / 12, s.Jxx);
            Rel(300 * Math.Pow(100, 3) / 12, s.Jyy);
            Assert.AreEqual(0, s.Jxy, 1e-3);
            Rel(s.Jxx, s.J11);
            Rel(s.Jyy, s.J22);
            Rel(s.Jxx + s.Jyy, s.Jp);
            Rel(100 * 300 * 300 / 6.0, s.Wel1);
            Rel(300 * 100 * 100 / 6.0, s.Wel2);
            Rel(100 * 300 * 300 / 4.0, s.Wpl1);
            Rel(300 * 100 * 100 / 4.0, s.Wpl2);
            Rel(50, s.Centroid.X);
            Rel(150, s.Centroid.Y);
            Assert.IsTrue(s.IsDoubleSymmetric);
        }

        [TestMethod]
        public void RectangleTorsionConstant()
        {
            // J = a b³ (1/3 - 0.21 b/a (1 - (b/a)⁴ / 12)), a/b = 3: coefficient 0.2634 (Saint-Venant 0.263)
            var s = new SectionRectangular(300, 100);
            double r = 1.0 / 3;
            double alpha = 1.0 / 3 - 0.21 * r * (1 - Math.Pow(r, 4) / 12);
            Rel(300 * Math.Pow(100, 3) * alpha, s.Jt);
            Assert.AreEqual(0.263, alpha, 1e-3);
            // square: 0.1408 a⁴ with the approximate formula (exact 0.1406, error 0.2%)
            Assert.AreEqual(0.1406, new SectionRectangular(100, 100).Jt / 1e8, 3e-4);
        }

        [TestMethod]
        public void RadiiOfGyrationFollowTheDocumentedConvention()
        {
            var s = new SectionRectangular(300, 100);
            // R11 = sqrt(J22 / A), R22 = sqrt(J11 / A) (convention of the library)
            Rel(Math.Sqrt(s.J22 / s.Area), s.R11);
            Rel(Math.Sqrt(s.J11 / s.Area), s.R22);
            Rel(300 / Math.Sqrt(12), s.R22);
            Rel(Math.Sqrt(s.Jp / s.Area), s.Rp);
        }

        [TestMethod]
        public void RotatedRectangleKeepsTheInvariants()
        {
            var straight = new SectionRectangular(300, 100);
            var rotated = new SectionRectangular(300, 100, Math.PI / 6);
            Rel(straight.Area, rotated.Area);
            Rel(straight.J11, rotated.J11, 1e-9);
            Rel(straight.J22, rotated.J22, 1e-9);
            // Jxx + Jyy and Jxx Jyy - Jxy² do not depend on the axes
            Rel(straight.Jxx + straight.Jyy, rotated.Jxx + rotated.Jyy, 1e-9);
            Rel(straight.Jxx * straight.Jyy, rotated.Jxx * rotated.Jyy - rotated.Jxy * rotated.Jxy, 1e-9);
            Assert.AreNotEqual(0, rotated.Jxy, 1);
        }

        [TestMethod]
        public void ScalingASectionScalesItsProperties()
        {
            var small = new SectionRectangular(300, 100);
            var large = new SectionRectangular(600, 200);
            Rel(4 * small.Area, large.Area);
            Rel(16 * small.J11, large.J11);
            Rel(8 * small.Wel1, large.Wel1);
            Rel(8 * small.Wpl2, large.Wpl2);
        }

        [TestMethod]
        public void CircleProperties()
        {
            var s = new SectionCircular(100);
            Rel(Math.PI * 100 * 100 / 4, s.Area);
            Rel(Math.PI * Math.Pow(100, 4) / 64, s.Jxx);
            Rel(Math.PI * Math.Pow(100, 4) / 64, s.Jyy);
            Rel(Math.PI * Math.Pow(100, 4) / 32, s.Jt);
            Rel(Math.PI * Math.Pow(100, 3) / 32, s.Wel1);
            Rel(Math.Pow(100, 3) / 6, s.Wpl1);
            Rel(Math.Pow(100, 3) / 6, s.Wpl2);
            // shape factor of the circle: 16 / (3 π)
            Rel(16 / (3 * Math.PI), s.Wpl1 / s.Wel1);
        }

        [TestMethod]
        public void CircularHollowSectionProperties()
        {
            double D = 200, t = 10, d = D - 2 * t;
            var s = new SectionCHS(D, t);
            Rel(Math.PI * (D * D - d * d) / 4, s.Area);
            Rel(Math.PI * (Math.Pow(D, 4) - Math.Pow(d, 4)) / 64, s.Jxx);
            Rel(Math.PI * (Math.Pow(D, 4) - Math.Pow(d, 4)) / 32, s.Jt);
            Rel(Math.PI * (Math.Pow(D, 4) - Math.Pow(d, 4)) / (32 * D), s.Wel1);
            Rel((Math.Pow(D, 3) - Math.Pow(d, 3)) / 6, s.Wpl1);
            Rel(D / 2, s.Centroid.X);
        }

        [TestMethod]
        public void RectangularHollowSectionProperties()
        {
            // 200 x 100 x 10 without corner radius
            var s = new SectionRHS(200, 100, 10, 10, 10, 10, "");
            Rel(200 * 100 - 180 * 80, s.Area);
            Rel((100 * Math.Pow(200, 3) - 80 * Math.Pow(180, 3)) / 12, s.Jxx);
            Rel((200 * Math.Pow(100, 3) - 180 * Math.Pow(80, 3)) / 12, s.Jyy);
            Rel((100 * 200 * 200 - 80 * 180 * 180) / 4.0, s.WplX, 1e-9);
            Rel((200 * 100 * 100 - 180 * 80 * 80) / 4.0, s.WplY, 1e-9);
            // Bredt on the middle lines: 4 Am² / Σ l/t
            double am = 190 * 90;
            Rel(4 * am * am / (2 * 90 / 10.0 + 2 * 190 / 10.0), s.Jt);
            Assert.IsTrue(s.IsDoubleSymmetric);
        }

        [TestMethod]
        public void GenericShapeWithAHoleEqualsTheHollowSection()
        {
            var generic = new Section(new Shape2d(Rectangle(0, 0, 100, 200), new[] { Rectangle(10, 10, 80, 180) }));
            // the properties of a generic shape are calculated on request
            Assert.AreEqual(0, generic.Area);
            generic.SetMechanicalProperties();
            var rhs = new SectionRHS(200, 100, 10, 10, 10, 10, "");
            Rel(rhs.Area, generic.Area);
            Rel(rhs.Jxx, generic.Jxx, 1e-9);
            Rel(rhs.Jyy, generic.Jyy, 1e-9);
            Rel(rhs.WplX, generic.WplX, 1e-9);
            Rel(rhs.Centroid.Y, generic.Centroid.Y);
        }

        [TestMethod]
        public void GenericShapeEqualsTheRectangle()
        {
            var generic = new Section(new Shape2d(Rectangle(0, 0, 100, 300)));
            generic.SetMechanicalProperties();
            var rectangle = new SectionRectangular(300, 100);
            Rel(rectangle.Area, generic.Area);
            Rel(rectangle.J11, generic.J11);
            Rel(rectangle.J22, generic.J22);
            Rel(rectangle.Wel1, generic.Wel1);
            Rel(rectangle.Wpl1, generic.Wpl1, 1e-9);
            Rel(rectangle.Wpl2, generic.Wpl2, 1e-9);
        }

        [TestMethod]
        public void DoublySymmetricHWithoutFillets()
        {
            double h = 300, tw = 10, b = 200, tf = 15, hw = h - 2 * tf;
            var s = new SectionH(h, tw, b, tf, b, tf, "");
            Rel(2 * b * tf + hw * tw, s.Area);
            Rel((b * Math.Pow(h, 3) - (b - tw) * Math.Pow(hw, 3)) / 12, s.Jxx);
            Rel(2 * tf * Math.Pow(b, 3) / 12 + hw * Math.Pow(tw, 3) / 12, s.Jyy);
            Rel(b * tf * (h - tf) + tw * hw * hw / 4, s.WplX, 1e-9);
            Rel(tf * b * b / 2 + hw * tw * tw / 4, s.WplY, 1e-9);
            Rel(s.Jxx / (h / 2), s.WelXMin);
            Rel(h / 2, s.Centroid.Y);
            Rel(b / 2, s.Centroid.X);
            // warping constant of the doubly symmetric I (CNR DT 208): d² If² / (2 If + Iweb)
            double d = h - tf, i = tf * Math.Pow(b, 3) / 12;
            Rel(d * d * i * i / (2 * i + hw * Math.Pow(tw, 3) / 12), s.Jw);
            Assert.IsTrue(s.IsDoubleSymmetric);
        }

        [TestMethod]
        public void TSectionCentroidAndInertia()
        {
            double h = 300, b = 200, tw = 10, tf = 20, hw = h - tf;
            var s = new SectionT(h, b, tw, tf, "");
            double af = b * tf, aw = tw * hw, a = af + aw;
            double yc = (af * (h - tf / 2) + aw * hw / 2) / a;
            Rel(a, s.Area);
            Rel(yc, s.Centroid.Y);
            Rel(b / 2, s.Centroid.X);
            Rel(b * Math.Pow(tf, 3) / 12 + af * Math.Pow(h - tf / 2 - yc, 2) + tw * Math.Pow(hw, 3) / 12 + aw * Math.Pow(hw / 2 - yc, 2), s.Jxx);
            Rel(tf * Math.Pow(b, 3) / 12 + hw * Math.Pow(tw, 3) / 12, s.Jyy);
            Rel(s.Jxx / yc, s.WelXMin, 1e-9, "the bottom fibre is the farthest one");
            Rel(s.Jxx / (h - yc), s.WelXMax, 1e-9);
            Rel((b * Math.Pow(tf, 3) + (h - tf / 2) * Math.Pow(tw, 3)) / 3, s.Jt);
            Assert.IsTrue(s.IsSymmetricAlongYLocalAxis);
            Assert.IsFalse(s.IsSymmetricAlongXLocalAxis);
        }

        [TestMethod]
        public void TSectionPlasticNeutralAxisInTheFlange()
        {
            // flange area 4000 > web area 2800: the plastic neutral axis is in the flange, at a depth z = A / (2 b) from the top
            double h = 300, b = 200, tw = 10, tf = 20, hw = h - tf;
            var s = new SectionT(h, b, tw, tf, "");
            double a = b * tf + tw * hw, z = a / 2 / b;
            double wpl = b * z * z / 2 + b * (tf - z) * (tf - z) / 2 + tw * hw * (tf - z + hw / 2);
            Rel(wpl, s.WplX, 1e-9);
        }

        [TestMethod]
        public void EqualAngleHasPrincipalAxesAt45Degrees()
        {
            // L 100 x 10 without fillets: vertical leg 10 x 100, horizontal leg 90 x 10
            var s = new SectionL(100, 10, 100, 10, "");
            double a1 = 1000, a2 = 900, a = a1 + a2;
            double c = (a1 * 5 + a2 * 55) / a;
            Rel(a, s.Area);
            Rel(c, s.Centroid.X);
            Rel(c, s.Centroid.Y);
            double jxx = 10 * Math.Pow(100, 3) / 12 + a1 * Math.Pow(50 - c, 2) + 90 * Math.Pow(10, 3) / 12 + a2 * Math.Pow(5 - c, 2);
            double jxy = a1 * (5 - c) * (50 - c) + a2 * (55 - c) * (5 - c);
            Rel(jxx, s.Jxx);
            Rel(jxx, s.Jyy);
            Rel(Math.Abs(jxy), Math.Abs(s.Jxy));
            Rel(jxx + Math.Abs(jxy), s.J11);
            Rel(jxx - Math.Abs(jxy), s.J22);
            Assert.AreEqual(0, Math.Cos(2 * s.AngleX1), 1e-9, "principal axes at ±45°");
            Assert.IsFalse(s.IsSymmetricAlongXLocalAxis);
            Assert.IsFalse(s.IsSymmetricAlongYLocalAxis);
            // torsion and shear center on the middle lines
            Rel((95 * 1000 + 95 * 1000) / 3.0, s.Jt);
            Rel(5, s.ShearCenter.X);
            Rel(5, s.ShearCenter.Y);
        }

        [TestMethod]
        public void ChannelWithoutRadii()
        {
            double h = 200, tw = 8, b = 75, tf = 12;
            var s = new SectionC(h, tw, b, tf, b, tf, "");
            double aw = tw * h, af = (b - tw) * tf, a = aw + 2 * af;
            double xc = (aw * tw / 2 + 2 * af * (tw + (b - tw) / 2)) / a;
            Rel(a, s.Area);
            Rel(xc, s.Centroid.X);
            Rel(h / 2, s.Centroid.Y);
            Rel(tw * Math.Pow(h, 3) / 12 + 2 * ((b - tw) * Math.Pow(tf, 3) / 12 + af * Math.Pow(h / 2 - tf / 2, 2)), s.Jxx);
            Rel(h * Math.Pow(tw, 3) / 12 + aw * Math.Pow(tw / 2 - xc, 2) + 2 * (tf * Math.Pow(b - tw, 3) / 12 + af * Math.Pow(tw + (b - tw) / 2 - xc, 2)), s.Jyy);
            Assert.AreEqual(0, s.Jxy, 1e-3);
            Assert.IsTrue(s.IsSymmetricAlongXLocalAxis);
            // the shear center is on the other side of the web
            Assert.IsTrue(s.ShearCenter.X < 0);
            Rel(h / 2, s.ShearCenter.Y);
        }

        [TestMethod]
        public void CopiesHaveTheSameProperties()
        {
            var t = new SectionT(300, 200, 10, 20, "T");
            var copy = new SectionT(t);
            Rel(t.Area, copy.Area);
            Rel(t.Jxx, copy.Jxx);
            Rel(t.WplX, copy.WplX);
            Assert.IsTrue(t.Equals(copy));
            Assert.IsFalse(t.Equals(new SectionT(300, 200, 10, 21, "T")));
        }
    }

    /// <summary>
    /// Control tests of the materials: EN 1992-1-1 table 3.1, parabola-rectangle, design laws, steels, homogenized reinforced concrete
    /// </summary>
    [TestClass]
    public class MaterialControlTests
    {
        private static void Rel(double expected, double actual, double tolerance = 1e-9, string message = "") =>
            Assert.AreEqual(expected, actual, Math.Abs(expected) * tolerance + 1e-15, message);

        [DataTestMethod]
        [DataRow(20.0)]
        [DataRow(30.0)]
        [DataRow(45.0)]
        [DataRow(50.0)]
        [DataRow(55.0)]
        [DataRow(70.0)]
        [DataRow(90.0)]
        public void En1992TableFormulas(double fck)
        {
            var c = new ConcreteMaterialEN1992("", fck, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
            double fcm = fck + 8;
            double fctm = fck <= 50 ? 0.3 * Math.Pow(fck, 2.0 / 3) : 2.12 * Math.Log(1 + fcm / 10);
            Rel(fck, Math.Abs(c.Fck));
            Rel(fcm, Math.Abs(c.Fcm));
            Rel(fctm, c.Fctm);
            Rel(0.7 * fctm, c.Fctk05);
            Rel(1.3 * fctm, c.Fctk95);
            Rel(22000 * Math.Pow(fcm / 10, 0.3), c.ElasticModulusCompression);
            double ec2 = fck <= 50 ? 2.0 : 2.0 + 0.085 * Math.Pow(fck - 50, 0.53);
            double ecu2 = fck <= 50 ? 3.5 : 2.6 + 35 * Math.Pow((90 - fck) / 100, 4);
            Rel(ec2 / 1000, Math.Abs(c.StrainYCompression));
            Rel(ecu2 / 1000, Math.Abs(c.StrainUCompression));
        }

        [DataTestMethod]
        // fck, fctm, Ecm (GPa), εc2 (‰), εcu2 (‰) of EN 1992-1-1 table 3.1 (rounded values)
        [DataRow(30.0, 2.9, 33.0, 2.0, 3.5)]
        [DataRow(50.0, 4.1, 37.0, 2.0, 3.5)]
        [DataRow(60.0, 4.4, 39.0, 2.3, 2.9)]
        [DataRow(70.0, 4.6, 41.0, 2.4, 2.7)]
        [DataRow(90.0, 5.0, 44.0, 2.6, 2.6)]
        public void En1992TableValues(double fck, double fctm, double ecm, double ec2, double ecu2)
        {
            var c = new ConcreteMaterialEN1992("", fck, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
            Assert.AreEqual(fctm, c.Fctm, 0.051);
            Assert.AreEqual(ecm, c.ElasticModulusCompression / 1000, 0.51);
            Assert.AreEqual(ec2, Math.Abs(c.StrainYCompression) * 1000, 0.051);
            Assert.AreEqual(ecu2, Math.Abs(c.StrainUCompression) * 1000, 0.051);
        }

        [TestMethod]
        public void CubeStrength()
        {
            // Rck has the sign of fck, as fcm (before the fix the negative fck never matched the table: C30/37 gave -30/0.83 = -36.1)
            Assert.AreEqual(-37, ConcreteMaterialEN1992Data.C30_37.Rck, 1e-9);
            Assert.AreEqual(30, Math.Abs(new ConcreteMaterialEN1992("", 25, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle).Rck), 1e-9);
            Assert.AreEqual(105, Math.Abs(new ConcreteMaterialEN1992("", 90, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle).Rck), 1e-9);
            // a strength not in the table: fck / 0.83
            Assert.AreEqual(33 / 0.83, Math.Abs(new ConcreteMaterialEN1992("", 33, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle).Rck), 1e-9);
        }

        [TestMethod]
        public void ParabolaRectangleUpToC50()
        {
            var c = new ConcreteMaterialEN1992("", 30, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
            // σ = fck (1 - (1 - ε/εc2)²), compression negative
            Assert.AreEqual(0, c.GetStress(0), 1e-12);
            Rel(-30 * 0.75, c.GetStress(-0.001));
            Rel(-30 * (1 - Math.Pow(1 - 0.25, 2)), c.GetStress(-0.0005));
            Rel(-30, c.GetStress(-0.002));
            Rel(-30, c.GetStress(-0.003));
            Rel(-30, c.GetStress(-0.0035));
        }

        [TestMethod]
        public void ParabolaRectangleOfAHighStrengthConcrete()
        {
            var c = new ConcreteMaterialEN1992("", 70, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
            double n = 1.4 + 23.4 * Math.Pow(0.2, 4);
            double ec2 = (2.0 + 0.085 * Math.Pow(20, 0.53)) / 1000;
            Rel(-70 * (1 - Math.Pow(0.5, n)), c.GetStress(-ec2 / 2), 1e-9);
            Rel(-70, c.GetStress(-ec2));
        }

        [TestMethod]
        public void BilinearAndStressBlock()
        {
            var bilinear = new ConcreteMaterialEN1992("", 30, ConcreteMaterial.CompressionStressStrainDiagrams.Bilinear);
            Rel(0.00175, Math.Abs(bilinear.StrainYCompression));
            Rel(-15, bilinear.GetStress(-0.000875));
            Rel(-30, bilinear.GetStress(-0.003));
            var block = new ConcreteMaterialEN1992("", 30, ConcreteMaterial.CompressionStressStrainDiagrams.StressBlock);
            // λ = 0.8: no stress in the first 20% of εcu
            Rel(0.2 * 0.0035, Math.Abs(block.StrainYCompression));
            Rel(-30, block.GetStress(-0.002));
            var block70 = new ConcreteMaterialEN1992("", 70, ConcreteMaterial.CompressionStressStrainDiagrams.StressBlock);
            double ecu = (2.6 + 35 * Math.Pow(0.2, 4)) / 1000, lambda = 0.8 - 20 / 400.0;
            Rel((1 - lambda) * ecu, Math.Abs(block70.StrainYCompression), 1e-9);
        }

        [TestMethod]
        public void DesignStrengthsForEveryNationalAnnex()
        {
            var c = ConcreteMaterialEN1992Data.C30_37;
            Rel(30 / 1.5, Math.Abs(c.CalculateDesignCompressiveStrength(new StandardEN1992p11())));
            Rel(0.85 * 30 / 1.5, Math.Abs(c.CalculateDesignCompressiveStrength(new StandardNTC2018Concrete())));
            Rel(0.85 * 30 / 1.5, Math.Abs(c.CalculateDesignCompressiveStrength(new StandardDINEN1992p11())));
            Rel(0.85 * 30 / 1.5, Math.Abs(c.CalculateDesignCompressiveStrength(new StandardNSEN1992p11())));
            Rel(30 / 1.4, Math.Abs(c.CalculateDesignCompressiveStrength(new StandardDSEN1992p11())));
            Rel(30 / 1.5, Math.Abs(c.CalculateDesignCompressiveStrength(new StandardModelCode2010())));
            // fctd = αct fctk,0.05 / γc
            Rel(0.7 * 0.3 * Math.Pow(30, 2.0 / 3) / 1.5, c.CalculateDesignTensileStrength(new StandardEN1992p11()));
            // accidental situation: γc = 1.2
            Rel(30 / 1.2, Math.Abs(c.CalculateFcdAccidental(new StandardEN1992p11())));
        }

        [TestMethod]
        public void StressBlockDesignStrengthIncludesEta()
        {
            var c = new ConcreteMaterialEN1992("", 70, ConcreteMaterial.CompressionStressStrainDiagrams.StressBlock);
            Rel((1 - 20 / 200.0) * 70 / 1.5, Math.Abs(c.CalculateDesignCompressiveStrength(new StandardEN1992p11())));
            var parabola = new ConcreteMaterialEN1992("", 70, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
            Rel(70 / 1.5, Math.Abs(parabola.CalculateDesignCompressiveStrength(new StandardEN1992p11())));
        }

        [TestMethod]
        public void DesignStressOfTheParabolaIsScaledByAlphaCcOverGammaC()
        {
            var c = ConcreteMaterialEN1992Data.C30_37;
            var ntc = new StandardNTC2018Concrete();
            Rel(0.85 / 1.5 * c.GetStress(-0.001), c.CalculateDesignStressConcrete(ntc, -0.001));
            Rel(-0.85 * 30 / 1.5, c.CalculateDesignStressConcrete(ntc, -0.0035));
        }

        [TestMethod]
        public void StrengthDevelopmentWithTime()
        {
            // cement class N: s = 0.25, βcc(t) = exp(s (1 - sqrt(28 / t)))
            var c = ConcreteMaterialEN1992Data.C30_37;
            Rel(1, c.GetBetaCC(28));
            Rel(Math.Exp(0.25 * (1 - Math.Sqrt(4))), c.GetBetaCC(7));
            // fctm(t) = βcc fctm for t < 28 days
            Rel(c.GetBetaCC(7) * c.Fctm, c.GetFctm(7));
            Assert.IsTrue(c.GetBetaCC(365) > 1);
        }

        [TestMethod]
        public void ConcreteThermalExpansionIsTenMicrostrainPerDegree()
        {
            // EN 1992-1-1 3.1.3(5): α = 10·10⁻⁶ K⁻¹
            Assert.AreEqual(10e-6, ConcreteMaterialEN1992Data.C30_37.AlfaThermalExpansion, 1e-12);
            Assert.AreEqual(12e-6, SteelMaterialEN1993Data.S355.AlfaThermalExpansion, 1e-12);
            Assert.AreEqual(12e-6, new SteelMaterial("s", 210000, 355, 510).AlfaThermalExpansion, 1e-12);
        }

        [TestMethod]
        public void StructuralSteelDesignYield()
        {
            var s = SteelMaterialEN1993Data.S355;
            Assert.AreEqual(355, s.Fyk, 0);
            Assert.AreEqual(490, s.Fu, 0);
            Assert.AreEqual(210000, s.ElasticModulusCompression, 0);
            Rel(210000 / 2.6, s.GetShearModule());
            Rel(355, s.CalculateFyd(new StandardEN1993p11()));
            Rel(355 / 1.05, s.CalculateFyd(new StandardNTC2018Steel()));
            Assert.AreEqual(235, SteelMaterialEN1993Data.S235.Fyk, 0);
            Assert.AreEqual(360, SteelMaterialEN1993Data.S235.Fu, 0);
        }

        [TestMethod]
        public void RebarDesignLawIsElasticPerfectlyPlastic()
        {
            var b = SteelMaterialEN1992Data.B450C;
            var en = new StandardEN1992p11();
            double fyd = 450 / 1.15, eyd = fyd / 200000;
            Rel(fyd, b.CalculateDesignYieldingStressTension(en));
            Rel(eyd, b.CalculateDesignYieldingStrainTension(en));
            Rel(0.9 * 0.075, b.CalculateDesignUltimateStrainTension(en));
            // elastic branch, yield plateau, symmetric in compression
            Rel(200, b.CalculateDesignStress(en, 0.001));
            Rel(-200, b.CalculateDesignStress(en, -0.001));
            Rel(fyd, b.CalculateDesignStress(en, 0.01));
            Rel(-fyd, b.CalculateDesignStress(en, -0.01));
            Rel(fyd, b.CalculateDesignStress(en, eyd * 1.0001), 1e-6);
            // characteristic law: fyk after εyk
            Rel(450, b.GetStress(0.01));
        }

        [TestMethod]
        public void HomogenizationFactorAndCreep()
        {
            var c = ConcreteMaterialEN1992Data.C30_37;
            var b = SteelMaterialEN1992Data.B450C;
            double n = 200000 / c.ElasticModulusCompression;
            // φ from a required n: n Ec / Es - 1
            Rel(15 / n - 1, ReinforcedConcreteSection.CalculateHomogenizedFactorPhi(15, b, c));
            Rel(0, ReinforcedConcreteSection.CalculateHomogenizedFactorPhi(n, b, c), 1e-9);
        }

        [TestMethod]
        public void HomogenizedRectangleWithSymmetricBars()
        {
            // 300 x 500 with 3 + 3 bars Ø20 at 50 mm from the edges: the centroid does not move
            var c = ConcreteMaterialEN1992Data.C30_37;
            var bar = new RebarSectionCircular(20, SteelMaterialEN1992Data.B450C);
            var section = new ReinforcedConcreteSection(new SectionRectangular(500, 300), c);
            foreach (double y in new[] { 50.0, 450.0 })
                foreach (double x in new[] { 50.0, 150.0, 250.0 })
                    section.AddRebar(new ReinforcedConcreteRebar(bar, new Point2d(x, y)));
            double n = 200000 / c.ElasticModulusCompression, a = Math.PI * 100;
            Rel(6 * a, section.AreaRebars);
            Rel(500 * 300 + 6 * (n - 1) * a, section.GetHomogenizedArea());
            Point2d centroid = section.GetHomogenizedCentroid(out _, out _);
            Rel(150, centroid.X, 1e-9);
            Rel(250, centroid.Y, 1e-9);
            // the bars count also with their own inertia π Ø⁴ / 64
            Rel(300 * Math.Pow(500, 3) / 12 + 6 * (n - 1) * (a * 200 * 200 + Math.PI * Math.Pow(20, 4) / 64), section.GetHomogeneizedJ11(), 1e-6);
        }

        [TestMethod]
        public void HomogenizedCentroidMovesTowardsTheBars()
        {
            var c = ConcreteMaterialEN1992Data.C25_30;
            var bar = new RebarSectionCircular(16, SteelMaterialEN1992Data.B450C);
            var section = new ReinforcedConcreteSection(new SectionRectangular(400, 250), c);
            section.AddRebar(new ReinforcedConcreteRebar(bar, new Point2d(125, 40)));
            double n = 200000 / c.ElasticModulusCompression, a = Math.PI * 64;
            double area = 400 * 250 + (n - 1) * a;
            Point2d centroid = section.GetHomogenizedCentroid(out double sx, out _);
            Rel((400 * 250 * 200 + (n - 1) * a * 40) / area, centroid.Y, 1e-9);
            Rel(125, centroid.X, 1e-9);
        }
    }
}
