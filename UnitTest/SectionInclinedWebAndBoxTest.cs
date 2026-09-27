using System;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// The H section with an inclined web and the steel box girder: exact properties computed by hand (the inclined web is a parallelogram of
    /// horizontal width tw / cos α, own Jyy = A (w² + e²) / 12 and own Jxy = -A e hw / 12), reduction to the known sections, validations
    /// </summary>
    [TestClass]
    public class SectionInclinedWebAndBoxTest
    {
        private static void Rel(double expected, double actual, double tolerance = 1e-9, string message = "") =>
            Assert.AreEqual(expected, actual, Math.Abs(expected) * tolerance + 1e-9, message);

        [TestMethod]
        public void InclinedWebExactProperties()
        {
            double h = 1000, tw = 12, bt = 400, tt = 20, bb = 500, tb = 30, e = 200, hw = h - tt - tb;
            var s = new SectionHInclinedWeb(h, tw, bt, tt, bb, tb, e);
            double l = Math.Sqrt(hw * hw + e * e), w = tw * l / hw;
            Rel(l, s.LengthWeb);
            Rel(w, s.HorizontalThicknessWeb);
            Rel(Math.Atan(e / hw), s.WebAngle);
            // axes of the flanges: top at max(bt/2, bb/2 - e), bottom e to the right
            double xt = Math.Max(bt / 2, bb / 2 - e), xb = xt + e, xw = (xt + xb) / 2, yw = tb + hw / 2;
            Rel(xt, s.TopAxisX); Rel(xb, s.BottomAxisX);
            double at = bt * tt, ab = bb * tb, aw = hw * w, a = at + ab + aw;
            Rel(a, s.Area);
            Rel(l * tw, aw, 1e-12, "area of the web: L tw");
            double yc = (at * (h - tt / 2) + ab * tb / 2 + aw * yw) / a, xc = (at * xt + ab * xb + aw * xw) / a;
            Rel(yc, s.Centroid.Y); Rel(xc, s.Centroid.X);
            Rel(bt * Math.Pow(tt, 3) / 12 + at * Math.Pow(h - tt / 2 - yc, 2) + bb * Math.Pow(tb, 3) / 12 + ab * Math.Pow(tb / 2 - yc, 2)
                + w * Math.Pow(hw, 3) / 12 + aw * Math.Pow(yw - yc, 2), s.Jxx);
            Rel(tt * Math.Pow(bt, 3) / 12 + at * Math.Pow(xt - xc, 2) + tb * Math.Pow(bb, 3) / 12 + ab * Math.Pow(xb - xc, 2)
                + aw * (w * w + e * e) / 12 + aw * Math.Pow(xw - xc, 2), s.Jyy);
            // the web goes down to the right: negative own product
            Rel(-aw * e * hw / 12 + at * (xt - xc) * (h - tt / 2 - yc) + ab * (xb - xc) * (tb / 2 - yc) + aw * (xw - xc) * (yw - yc), s.Jxy);
            Assert.AreNotEqual(0, s.Jxy, 1.0);
            Assert.AreNotEqual(0, Math.Sin(2 * s.AngleX1), 1e-6, "principal axes rotated");
            Assert.IsFalse(s.IsSymmetricAlongYLocalAxis);
            Assert.IsFalse(s.IsSymmetricAlongXLocalAxis);
            Rel(Math.Max(xt + bt / 2, xb + bb / 2), s.Width);
        }

        [TestMethod]
        public void InclinedWebWithoutOffsetIsTheH()
        {
            var inclined = new SectionHInclinedWeb(1000, 12, 400, 20, 500, 30, 0);
            var h = new SectionH(1000, 12, 400, 20, 500, 30, "");
            Rel(h.Area, inclined.Area);
            Rel(h.Jxx, inclined.Jxx);
            Rel(h.Jyy, inclined.Jyy);
            Assert.AreEqual(0, inclined.Jxy, 1e-3);
            Rel(h.Centroid.Y, inclined.Centroid.Y);
            Rel(h.WelXMin, inclined.WelXMin);
            Rel(h.WplX, inclined.WplX, 1e-9);
            Rel(h.WplY, inclined.WplY, 1e-9);
            Rel(h.Jt, inclined.Jt, 1e-9);
            Assert.IsTrue(inclined.IsSymmetricAlongYLocalAxis);
        }

        [TestMethod]
        public void InclinedWebBothDirectionsAreMirrored()
        {
            var right = new SectionHInclinedWeb(1000, 12, 400, 20, 500, 30, 150);
            var left = new SectionHInclinedWeb(1000, 12, 400, 20, 500, 30, -150);
            Rel(right.Area, left.Area);
            Rel(right.Jxx, left.Jxx);
            Rel(right.Jyy, left.Jyy);
            Rel(-right.Jxy, left.Jxy);
            Rel(right.WplX, left.WplX, 1e-9);
            Rel(right.J11, left.J11);
        }

        [TestMethod]
        public void InclinedWebForVerticalBendingEqualsAVerticalWebOfWidthTwOverCos()
        {
            // for bending about the horizontal axis the inclined web is a vertical web of thickness tw / cos α
            var inclined = new SectionHInclinedWeb(1000, 12, 400, 20, 500, 30, 300);
            var equivalent = new SectionH(1000, inclined.HorizontalThicknessWeb, 400, 20, 500, 30, "");
            Rel(equivalent.Area, inclined.Area);
            Rel(equivalent.Centroid.Y, inclined.Centroid.Y);
            Rel(equivalent.Jxx, inclined.Jxx);
            Rel(equivalent.WplX, inclined.WplX, 1e-9);
        }

        [TestMethod]
        public void InclinedWebValidation()
        {
            Assert.ThrowsException<ArgumentException>(() => new SectionHInclinedWeb(1000, 12, 400, 20, 500, 30, 2000));
            Assert.ThrowsException<ArgumentException>(() => new SectionHInclinedWeb(40, 12, 400, 20, 500, 30, 10));
            Assert.ThrowsException<ArgumentException>(() => new SectionHInclinedWeb(1000, 450, 400, 20, 500, 30, 10));
            var s = new SectionHInclinedWeb(1000, 12, 400, 20, 500, 30, 100, "A");
            var copy = new SectionHInclinedWeb(s);
            Assert.IsTrue(s.Equals(copy));
            Assert.IsFalse(s.Equals(new SectionHInclinedWeb(1000, 12, 400, 20, 500, 30, 101, "A")));
            Assert.AreEqual("HI 1000x12x400x20x500x30/100", s.ToString());
        }

        [TestMethod]
        public void BoxExactProperties()
        {
            double h = 1500, tw = 14, bt = 500, tt = 30, bb = 1400, tb = 25, st = 2000, sb = 1200, hw = h - tt - tb, e = (st - sb) / 2;
            var s = new SectionSteelBox(h, tw, bt, tt, bb, tb, st, sb);
            double l = Math.Sqrt(hw * hw + e * e), w = tw * l / hw;
            Rel(Math.Atan(e / hw), s.WebAngle);
            Rel(l, s.LengthWeb);
            Rel(w, s.HorizontalThicknessWeb);
            Rel(sb - w, s.BottomFlangeInternalWidth);
            Rel((bb - sb - w) / 2, s.BottomFlangeOutstand);
            double at = 2 * bt * tt, ab = bb * tb, aw = 2 * hw * w, a = at + ab + aw, yw = tb + hw / 2;
            Rel(a, s.Area);
            Rel(2 * l * tw, aw, 1e-12);
            double yc = (at * (h - tt / 2) + ab * tb / 2 + aw * yw) / a;
            Rel(yc, s.Centroid.Y);
            Rel(s.Width / 2, s.Centroid.X);
            Rel(2 * bt * Math.Pow(tt, 3) / 12 + at * Math.Pow(h - tt / 2 - yc, 2) + bb * Math.Pow(tb, 3) / 12 + ab * Math.Pow(tb / 2 - yc, 2)
                + 2 * w * Math.Pow(hw, 3) / 12 + aw * Math.Pow(yw - yc, 2), s.Jxx);
            double xw = (st + sb) / 4;
            Rel(2 * (tt * Math.Pow(bt, 3) / 12 + bt * tt * st * st / 4) + tb * Math.Pow(bb, 3) / 12 + aw * (w * w + e * e) / 12 + aw * xw * xw, s.Jyy);
            Assert.AreEqual(0, s.Jxy);
            Assert.IsTrue(s.IsSymmetricAlongYLocalAxis);
            Rel(Math.Max(st + bt, bb), s.Width);
        }

        [TestMethod]
        public void BoxWithVerticalWebsIsTwoHalves()
        {
            // vertical webs: the box is the sum of rectangles
            var s = new SectionSteelBox(1200, 12, 400, 25, 1000, 30, 900, 900);
            Assert.AreEqual(0, s.WebAngle, 0);
            double hw = 1200 - 25 - 30;
            Rel(2 * 400 * 25 + 1000 * 30 + 2 * 12 * hw, s.Area);
            Rel(900 - 12, s.BottomFlangeInternalWidth);
            // plastic modulus about X from the shape equals the one of the equivalent H (webs 2 tw, top flange 2 bt)
            var h = new SectionH(1200, 24, 800, 25, 1000, 30, "");
            Rel(h.Jxx, s.Jxx);
            Rel(h.WplX, s.WplX, 1e-9);
        }

        [TestMethod]
        public void BoxValidation()
        {
            // top flanges overlapping, webs outside the bottom flange, webs too inclined
            Assert.ThrowsException<ArgumentException>(() => new SectionSteelBox(1500, 14, 500, 30, 1400, 25, 400, 300));
            Assert.ThrowsException<ArgumentException>(() => new SectionSteelBox(1500, 14, 500, 30, 1000, 25, 2000, 1200));
            Assert.ThrowsException<ArgumentException>(() => new SectionSteelBox(500, 14, 500, 30, 1400, 25, 4000, 1000));
            var s = new SectionSteelBox(1500, 14, 500, 30, 1400, 25, 2000, 1200, "C");
            Assert.IsTrue(s.Equals(new SectionSteelBox(s)));
            Assert.AreEqual("BOX 1500x14x500x30x1400x25/2000-1200", s.ToString());
            Assert.IsTrue(s.Jt > 0);
        }
    }
}
