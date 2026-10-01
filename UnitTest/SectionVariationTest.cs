using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest
{
    /// <summary>
    /// The variable sections: the outlines interpolated vertex by vertex against the sections of the interpolated dimensions
    /// </summary>
    [TestClass]
    public class SectionVariationTest
    {
        private static void Rel(double expected, double actual, double tolerance = 1e-9) =>
            Assert.AreEqual(expected, actual, tolerance * Math.Max(1.0, Math.Abs(expected)), $"expected {expected}, actual {actual}");

        private static SectionH Rolled(double height)
        {
            var h = new SectionH(height, 12, 300, 20, 300, 20, $"H {height}", 27);
            h.SetEdgeTypeFromSteelType(Section.SectionTypes.Rolled);
            h.SetMechanicalProperties();
            return h;
        }

        [TestMethod]
        public void TaperedH()
        {
            // straight edges: the H at the middle is the H of the mean height (the fillets keep their radius)
            var variation = new SectionVariation(Rolled(600), Rolled(1200));
            Section middle = variation.SectionAt(0.5);
            SectionH expected = Rolled(900);
            var outline = new Section(expected.GetPlasticShape(), "outline");
            outline.SetMechanicalProperties();
            Rel(outline.Area, middle.Area, 1e-12);
            Rel(outline.Jxx, middle.Jxx, 1e-12);
            Rel(expected.Area, middle.Area, 1e-4); // the analytic fillets against their polygons
            Rel(expected.WplX, middle.WplX, 1e-9);
            Rel(600, variation.SectionAt(0).Height(), 1e-12);

            // parabolic with the vertex at the start: a quarter of the change at the middle
            var haunch = new SectionVariation(Rolled(600), Rolled(1200), VariationLaw.ParabolicFlatAtStart);
            Rel(0.25, haunch.Factor(0.5));
            Rel(Rolled(750).Area, haunch.SectionAt(0.5).Area, 1e-4);

            Assert.ThrowsException<ArgumentException>(() => new SectionVariation(Rolled(600), new SectionH(1200, 12, 300, 20, 300, 20, "sharp")));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => variation.SectionAt(1.5));
        }

        [TestMethod]
        public void BoxGirderOfVariableHeight()
        {
            SectionBoxGirder Box(double h) => new SectionBoxGirder(h, 8000, 250, 300, 3, 400, 5000, 5000, 200, 0, 500, 200, 300, 150);
            var variation = new SectionVariation(Box(2000), Box(4000));
            Section section = variation.SectionAt(0.25);
            Rel(Box(2500).Area, section.Area, 1e-9);
            Rel(Box(2500).Jxx, section.Jxx, 1e-9);
            Assert.IsTrue(section.Jt > 0);

            var angle = new SectionL(100, 10, 100, 10, "L");
            Assert.ThrowsException<ArgumentException>(() => new SectionVariation(SectionBuiltUp.DoubleAngle(angle, 10, "2L"), Box(2000)));
        }
    }

    internal static class SectionExtensions
    {
        public static double Height(this Section section)
        {
            var points = section.Shape.GetPoints2d();
            double min = double.MaxValue, max = double.MinValue;
            foreach (var p in points)
            {
                min = Math.Min(min, p.Y);
                max = Math.Max(max, p.Y);
            }
            return max - min;
        }
    }
}
