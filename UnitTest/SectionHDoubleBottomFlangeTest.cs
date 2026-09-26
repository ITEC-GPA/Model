using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// The H section with two bottom plates: exact properties, reduction to the H section with one plate, composite bridge section
    /// </summary>
    [TestClass]
    public class SectionHDoubleBottomFlangeTest
    {
        // bridge girder: web 1800 x 14, top 500 x 25, bottom plates 800 x 40 and 600 x 30
        private const double H = 1895, Tw = 14, Bt = 500, Tt = 25, B1 = 800, T1 = 40, B2 = 600, T2 = 30;

        private static SteelSection Section(bool welded = false, double r = 0) =>
            new SteelSection(new SectionHDoubleBottomFlange(H, Tw, Bt, Tt, B1, T1, B2, T2, "", r), SteelMaterialEN1993Data.S355,
                welded ? GPC.Model.Sections.Section.SectionTypes.Welded : GPC.Model.Sections.Section.SectionTypes.Rolled);

        /// <summary>Area, centroid and moments of inertia of a polygon (Green's formulas)</summary>
        private static (double A, double X, double Y, double Ixx, double Iyy) Polygon(IList<(double X, double Y)> p)
        {
            double a = 0, sx = 0, sy = 0, ixx = 0, iyy = 0;
            for (int i = 0; i < p.Count; i++)
            {
                var (x0, y0) = p[i]; var (x1, y1) = p[(i + 1) % p.Count];
                double c = x0 * y1 - x1 * y0;
                a += c / 2; sy += (x0 + x1) * c / 6; sx += (y0 + y1) * c / 6;
                ixx += (y0 * y0 + y0 * y1 + y1 * y1) * c / 12; iyy += (x0 * x0 + x0 * x1 + x1 * x1) * c / 12;
            }
            double cx = sy / a, cy = sx / a;
            return (a, cx, cy, ixx - a * cy * cy, iyy - a * cx * cx);
        }

        /// <summary>Counterclockwise contour, with optional welds (legs 1.41 a) between the web and the flanges</summary>
        private static List<(double X, double Y)> Contour(double weld)
        {
            double xc = B1 / 2, y1 = T2, y2 = T2 + T1, y3 = H - Tt, leg = 1.41 * weld;
            var p = new List<(double X, double Y)> { (xc - B2 / 2, 0), (xc + B2 / 2, 0), (xc + B2 / 2, y1), (xc + B1 / 2, y1), (xc + B1 / 2, y2) };
            if (weld > 0) p.AddRange(new[] { (xc + Tw / 2 + leg, y2), (xc + Tw / 2, y2 + leg), (xc + Tw / 2, y3 - leg), (xc + Tw / 2 + leg, y3) });
            else p.AddRange(new[] { (xc + Tw / 2, y2), (xc + Tw / 2, y3) });
            p.AddRange(new[] { (xc + Bt / 2, y3), (xc + Bt / 2, H), (xc - Bt / 2, H), (xc - Bt / 2, y3) });
            if (weld > 0) p.AddRange(new[] { (xc - Tw / 2 - leg, y3), (xc - Tw / 2, y3 - leg), (xc - Tw / 2, y2 + leg), (xc - Tw / 2 - leg, y2) });
            else p.AddRange(new[] { (xc - Tw / 2, y3), (xc - Tw / 2, y2) });
            p.AddRange(new[] { (xc - B1 / 2, y2), (xc - B1 / 2, y1), (xc - B2 / 2, y1) });
            return p;
        }

        private static void Compare(SteelSection s, List<(double X, double Y)> contour)
        {
            var e = Polygon(contour);
            Assert.AreEqual(e.A, s.Area, 1e-9 * e.A);
            Assert.AreEqual(e.X, s.Centroid.X, 1e-9 * H);
            Assert.AreEqual(e.Y, s.Centroid.Y, 1e-9 * H);
            Assert.AreEqual(e.Ixx, s.Jxx, 1e-9 * e.Ixx);
            Assert.AreEqual(e.Iyy, s.Jyy, 1e-9 * e.Iyy);
            Assert.AreEqual(0, s.Jxy, 1e-9 * e.Ixx);
        }

        [TestMethod]
        public void PropertiesOfTheFourPlates() => Compare(new SteelSection(new SectionHDoubleBottomFlange(H, Tw, Bt, Tt, B1, T1, B2, T2), SteelMaterialEN1993Data.S355), Contour(0));

        [TestMethod]
        public void PropertiesWithTheWeldsBetweenWebAndFlanges() => Compare(Section(true, 8), Contour(8));

        [TestMethod]
        public void EqualPlatesAreAnHWithTheirTotalThickness()
        {
            var two = new SectionHDoubleBottomFlange(H, Tw, Bt, Tt, B1, T1, B1, T2);
            var one = new SectionH(H, Tw, Bt, Tt, B1, T1 + T2, "");
            Assert.AreEqual(one.Area, two.Area, 1e-9 * one.Area);
            Assert.AreEqual(one.Centroid.Y, two.Centroid.Y, 1e-9 * H);
            Assert.AreEqual(one.Jxx, two.Jxx, 1e-9 * one.Jxx);
            Assert.AreEqual(one.Jyy, two.Jyy, 1e-9 * one.Jyy);
            Assert.AreEqual(one.Jw, two.Jw, 1e-9 * one.Jw);
            Assert.AreEqual(one.Wel1, two.Wel1, 1e-9 * one.Wel1);
        }

        [TestMethod]
        public void GeometryAndValidation()
        {
            var s = new SectionHDoubleBottomFlange(H, Tw, Bt, Tt, B1, T1, B2, T2);
            Assert.AreEqual(H - Tt - T1 - T2, s.HeightWeb);
            Assert.AreEqual(B1, s.Width);
            Assert.AreEqual(H, s.Shape.Fill.Max(p => p.Y) - s.Shape.Fill.Min(p => p.Y), 1e-9);
            Assert.ThrowsException<ArgumentException>(() => new SectionHDoubleBottomFlange(90, Tw, Bt, Tt, B1, T1, B2, T2));
            Assert.ThrowsException<ArgumentException>(() => new SectionHDoubleBottomFlange(H, Tw, Bt, Tt, B1, T1, B2, 0));
        }

        [TestMethod]
        public void CompositeBridgeSectionWithTwoBottomPlates()
        {
            var steel = new SectionHDoubleBottomFlange(H, Tw, Bt, Tt, B1, T1, B2, T2);
            var section = ReinforcedConcreteSection.CreateBridgeSection(3000, 250, ConcreteMaterialEN1992Data.C35_45, null, 150, 45, null, 150,
                steel, SteelMaterialEN1993Data.S355);
            var position = section.SteelSections.Single();
            var points = position.Section.Shape.Fill.Select(p => position.PositionToGlobal(p)).ToArray();
            Assert.AreEqual(0, points.Max(p => p.Y), 1e-9, "top of the steel at the bottom of the slab");
            Assert.AreEqual(-H, points.Min(p => p.Y), 1e-9);
            Assert.AreEqual(1500, (points.Max(p => p.X) + points.Min(p => p.X)) / 2, 1e-9, "centred on the slab");
            Assert.IsFalse(position.IsInsideConcrete);
        }
    }
}
