using GPC.Geometry;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UnitTest
{
    /// <summary>
    /// Torsion constant and shear centre of the open sections T, C and L (and of the pairs 2C, 2L of <see cref="SectionBuiltUp"/>) solved with
    /// the finite elements on the outline (option B of the open sections, 16/11/2026), against the exact series of the rectangle, the thin-walled
    /// limit and the section calculator of Straus7
    /// </summary>
    [TestClass]
    public class OpenSectionTorsionTest
    {
        /// <summary>
        /// The torsion constant of the rectangle (series of Saint-Venant): a b³ [1/3 - 64 b / (π⁵ a) Σ tanh(n π a / 2b) / n⁵], n odd, a ≥ b
        /// </summary>
        private static double RectangleTorsion(double a, double b)
        {
            if (a < b)
                (a, b) = (b, a);
            double sum = 0;
            for (int n = 1; n < 200; n += 2)
                sum += Math.Tanh(n * Math.PI * a / (2 * b)) / Math.Pow(n, 5);
            return a * Math.Pow(b, 3) * (1.0 / 3.0 - 64.0 * b / (Math.Pow(Math.PI, 5) * a) * sum);
        }

        /// <summary>
        /// A case of the native qualification 198 of BridGHe: the dimensions as BridGHe gives them to GPC (GpcSections.Standard: T and C with
        /// the flange width B and the height H, L with the horizontal leg B thick tf and the vertical leg H thick tw), the pair mirrored about
        /// the vertical axis at the gap, and the values of the section calculator of Straus7 in the principal axes of GPC (J in mm4, SL1 and
        /// SL2 in mm). Straus7: R3 physical comparison of the coordinator, native-20261116-022939-822/physical-comparison.json (BridGHe
        /// bbdc3836, set AX), as recorded by task 209 (chatgpt-compiti/evidenze-209/physical-comparison-coordinator.json)
        /// </summary>
        private sealed record NativeCase(string Type, double B, double H, double Tf, double Tw, bool Mirror, double Gap, double J, double SL1, double SL2);

        private static readonly NativeCase[] Straus7 =
        {
            new("T", 140, 300, 12, 8, false, 0, 127310.503671523, 0, 85.9226684570313),
            new("C", 75, 200, 12, 10, false, 0, 140723.278276233, -41.4414443969727, 0),
            new("Angle", 100, 150, 12, 10, false, 0, 100378.821514206, -35.8190534824794, -26.6510546073409),
            new("C", 75, 200, 12, 10, true, 0, 266476.483860229, 0, 0),
            new("C", 75, 200, 12, 10, true, 10, 267472.375243242, 0, 0),
            new("C", 75, 200, 12, 10, true, 120, 267472.375243241, 0, 0),
            new("C", 75, 200, 12, 10, true, 101.96, 267472.375243242, 0, 0),
            new("C", 75, 200, 12, 10, true, 101.99, 267472.375243241, 0, 0),
            new("Angle", 100, 150, 12, 10, true, 0, 189533.312292717, 0, 0),
            new("Angle", 100, 150, 12, 10, true, 10, 190030.85159132, 0, 0),
            new("Angle", 100, 150, 12, 10, true, 40, 190030.851591321, 0, 0),
            new("Angle", 100, 150, 12, 10, true, 21.83, 190030.85159132, 0, 0),
            new("Angle", 100, 150, 12, 10, true, 21.85, 190030.851591321, 0, 0),
            new("Angle", 150, 100, 10, 12, false, 0, 100378.821514207, -35.8190538531707, 26.6510541091313),
            new("T", 300, 80, 12, 10, false, 0, 193987.600442279, -6.22784614562988, 0),
            new("C", 300, 80, 12, 10, false, 0, 359866.696599688, 0, -273.368438720703),
            new("T", 48, 40, 18, 16, false, 0, 116457.854112789, -3.3451669216156, 0),
            new("C", 40, 30, 14, 12, false, 0, 67476.9431563453, 0, -18.5936851501465),
            new("Angle", 40, 30, 14, 12, false, 0, 39954.2296654059, -5.86892254951491, 4.16367172033296),
        };

        private static Section Single(NativeCase c) => c.Type switch
        {
            "T" => new SectionT(c.H, c.B, c.Tw, c.Tf, "T"),
            "C" => new SectionC(c.H, c.Tw, c.B, c.Tf, c.B, c.Tf, "C"),
            _ => new SectionL(c.B, c.Tf, c.H, c.Tw, "L"),
        };

        private static Section Build(NativeCase c, Section single) => !c.Mirror ? single :
            new SectionBuiltUp(new[] { new SectionBuiltUp.Part(single, 0, 0), new SectionBuiltUp.Part(single, -c.Gap, 0, mirrorX: true) }, "pair");

        /// <summary>The offsets of the shear centre from the centroid in the principal axes, as the Straus7 writer of the Converter writes them</summary>
        private static (double SL1, double SL2) Principal(Section s)
        {
            double dx = s.ShearCenter.X - s.Centroid.X, dy = s.ShearCenter.Y - s.Centroid.Y, c = Math.Cos(s.AngleX1), n = Math.Sin(s.AngleX1);
            return (c * dx + n * dy, -n * dx + c * dy);
        }

        [TestMethod]
        public void SinglesAgreeWithTheSectionCalculatorOfStraus7()
        {
            // before (thin-walled formulas): J up to +16% (T 48x40), shear centre up to 1.96 times (L 40x30); now J within 0.5% (the finite
            // elements of Model converge from above: the default mesh is 0.2-0.4% stiffer than the converged value, Straus7 0.04% below it)
            foreach (NativeCase c in Straus7.Where(c => !c.Mirror))
            {
                Section s = Single(c);
                double size = Math.Max(c.B, c.H);
                Assert.AreEqual(c.J, s.Jt, 0.006 * c.J, $"{c}: J");
                var (sl1, sl2) = Principal(s);
                Assert.AreEqual(c.SL1, sl1, 0.003 * size, $"{c}: SL1");
                Assert.AreEqual(c.SL2, sl2, 0.003 * size, $"{c}: SL2");
                Assert.AreEqual(PropertyAvailability.Numerical, s.GetAvailability(SectionProperty.TorsionConstant));
                Assert.AreEqual(PropertyAvailability.Numerical, s.GetAvailability(SectionProperty.ShearCenter));
                Assert.AreEqual(PropertyAvailability.Approximate, s.GetAvailability(SectionProperty.WarpingConstant));

                // the lower bound of the exact solution: the section holds the disjoint rectangles of its walls (monotony of the torsional
                // rigidity with the region), and the finite elements of the warping function are above the exact solution
                double rectangles = c.Type == "T" ? RectangleTorsion(c.B, c.Tf) + RectangleTorsion(c.H - c.Tf, c.Tw)
                    : c.Type == "C" ? 2 * RectangleTorsion(c.B - c.Tw, c.Tf) + RectangleTorsion(c.H, c.Tw)
                    : RectangleTorsion(c.B, c.Tf) + RectangleTorsion(c.H - c.Tf, c.Tw);
                Assert.IsTrue(s.Jt > rectangles, $"{c}: J {s.Jt} below the rectangles {rectangles}");
            }
        }

        [TestMethod]
        public void PairsAreTheSumOfTheirParts()
        {
            // parts connected at discrete points: the Saint-Venant torsion of separate regions is additive, J = 2 J of the part. Straus7 gives
            // for its mirrored pairs 5.0-5.6% less than twice its own J of the part (2C 266476-267472 against 2 x 140723 mm4, 2L 189533-190031
            // against 2 x 100379 mm4), while its J of the single part is 0.04% from the converged one: a limit of its calculator on the pairs
            foreach (NativeCase c in Straus7.Where(c => c.Mirror))
            {
                Section part = Single(c);
                Section pair = Build(c, part);
                NativeCase single = Straus7.First(s => !s.Mirror && s.Type == c.Type && s.B == c.B && s.H == c.H && s.Tf == c.Tf && s.Tw == c.Tw);
                Assert.AreEqual(2 * part.Jt, pair.Jt, 1e-12 * pair.Jt, $"{c}: J");
                Assert.AreEqual(2 * single.J, pair.Jt, 0.006 * pair.Jt, $"{c}: J against 2 J of the part of Straus7");

                // shear centre: on the axis of symmetry; the 2C is doubly symmetric (centroid), the 2L at the height of the shear centre of its
                // angles (the parts bend together without shear flow between them, the resultant of the shear of each part passes through its
                // shear centre). Straus7 gives the centroid for every mirrored pair (SL1 = SL2 = 0)
                Assert.AreEqual(pair.Centroid.X, pair.ShearCenter.X, 1e-9 * c.B, $"{c}: shear centre X");
                double y = c.Type == "C" ? pair.Centroid.Y : part.ShearCenter.Y;
                Assert.AreEqual(y, pair.ShearCenter.Y, 1e-9 * c.H, $"{c}: shear centre Y");
                if (c.Type == "Angle")
                    Assert.AreEqual(-39.47, pair.ShearCenter.Y - pair.Centroid.Y, 0.05, $"{c}: shear centre from the centroid");
            }
        }

        [TestMethod]
        public void ThinWalledLimit()
        {
            // t = b / k: the finite elements tend to the thin-walled formulas (error proportional to t / b) and to the rectangles of the walls
            // (the junctions add a term of the order of t⁴)
            double b = 100, h = 200;
            double previousJ = double.MaxValue, previousRectangles = double.MaxValue;
            foreach (double k in new[] { 10.0, 30.0, 100.0 })
            {
                double t = b / k;
                var tee = new SectionT(h, b, t, t, "T");
                var channel = new SectionC(h, t, b, t, b, t, "C");
                var angle = new SectionL(b, t, h, t, "L");
                double worstJ = 0, worstRectangles = 0;
                foreach (var (s, thin, rectangles) in new (Section, double, double)[]
                {
                    (tee, tee.ThinWallTorsionConstant(), RectangleTorsion(b, t) + RectangleTorsion(h - t, t)),
                    (channel, channel.ThinWallTorsionConstant(), RectangleTorsion(h, t) + 2 * RectangleTorsion(b - t, t)),
                    (angle, angle.ThinWallTorsionConstant(), RectangleTorsion(b, t) + RectangleTorsion(h - t, t)),
                })
                {
                    Assert.IsTrue(s.Jt > rectangles);
                    worstJ = Math.Max(worstJ, Math.Abs(s.Jt / thin - 1));
                    worstRectangles = Math.Max(worstRectangles, s.Jt / rectangles - 1);
                }
                Assert.IsTrue(worstJ < previousJ && worstRectangles < previousRectangles, $"t = b / {k}: no convergence");
                previousJ = worstJ;
                previousRectangles = worstRectangles;

                // shear centres: the middle of the flange of the T, the corner of the middle lines of the L, the formula of the channel (the
                // T within 0.23 t for t = b / 10, 0.08 t for b / 30, 0.02 t for b / 100)
                Point2d thinTee = tee.ThinWallShearCenter(), thinAngle = angle.ThinWallShearCenter(), thinChannel = channel.ThinWallShearCenter();
                Assert.AreEqual(thinTee.X, tee.ShearCenter.X, 1e-9);
                Assert.AreEqual(thinTee.Y, tee.ShearCenter.Y, 0.3 * t);
                Assert.AreEqual(thinAngle.X, angle.ShearCenter.X, 0.1 * t);
                Assert.AreEqual(thinAngle.Y, angle.ShearCenter.Y, 0.15 * t);
                Assert.AreEqual(thinChannel.X, channel.ShearCenter.X, 0.05 * t);
                Assert.AreEqual(thinChannel.Y, channel.ShearCenter.Y, 1e-9);
            }
            Assert.IsTrue(previousJ < 0.002, $"t = b / 100: {previousJ:P3} from the thin-walled formulas");
            Assert.IsTrue(previousRectangles < 0.005, $"t = b / 100: {previousRectangles:P3} from the rectangles");
        }

        [TestMethod]
        public void TheTorsionIsComputedAtTheFirstAccessAndNotSerialized()
        {
            var tee = new SectionT(40, 48, 16, 18, "T 48x40");
            var copy = new SectionT(40, 48, 16, 18, "T 48x40");
            Assert.IsTrue(tee.Equals(copy));
            Assert.AreEqual(tee.GetHashCode(), copy.GetHashCode());
            double jt = tee.Jt;
            Point2d shearCenter = tee.ShearCenter;
            Assert.AreEqual(tee.CalculateTorsionProperties().TorsionConstant, jt);
            Assert.IsTrue(tee.Equals(copy), "equal before and after the computation");
            Assert.AreEqual(copy.Jt, jt);
            Assert.AreEqual(copy.ShearCenter.Y, shearCenter.Y);
            Assert.AreEqual(tee.Centroid.X, shearCenter.X, "on the axis of symmetry");

            var formatter = new ContractFormatter();
            foreach (Section section in new Section[] { tee, new SectionC(30, 12, 40, 14, 40, 14, "C 40x30"), new SectionL(40, 14, 30, 12, "L 40x30") })
            {
                double j = section.Jt;
                Point2d s = section.ShearCenter;
                using var stream = new System.IO.MemoryStream();
                formatter.Serialize(stream, section);
                stream.Position = 0;
                var read = (Section)formatter.Deserialize(stream);
                Assert.IsTrue(section.Equals(read), section.Name);
                Assert.AreEqual(j, read.Jt, section.Name);
                Assert.AreEqual(s.X, read.ShearCenter.X, section.Name);
                Assert.AreEqual(s.Y, read.ShearCenter.Y, section.Name);
                Assert.AreEqual(PropertyAvailability.Numerical, read.GetAvailability(SectionProperty.TorsionConstant), section.Name);
            }
        }

        [TestMethod]
        public void DegenerateOutlinesKeepTheThinWalledFormulas()
        {
            // no web below the flange, a web as wide as the flange, an angle with a leg as long as the thickness of the other one: not a region
            // for the finite elements, the thin-walled values (approximate) as before. (A channel without flanges beyond the web is not built:
            // its constructor overflows the stack in CalculateWplY, before and after this change)
            foreach (var (section, thin) in new (Section, double)[]
            {
                (new SectionT(12, 100, 10, 12, "no web"), new SectionT(12, 100, 10, 12, "no web").ThinWallTorsionConstant()),
                (new SectionT(100, 10, 10, 12, "web as wide as the flange"), new SectionT(100, 10, 10, 12, "").ThinWallTorsionConstant()),
                (new SectionL(10, 12, 100, 10, "short leg"), new SectionL(10, 12, 100, 10, "").ThinWallTorsionConstant()),
            })
            {
                Assert.AreEqual(thin, section.Jt, section.Name);
                Assert.AreEqual(PropertyAvailability.Approximate, section.GetAvailability(SectionProperty.TorsionConstant), section.Name);
                Assert.AreEqual(PropertyAvailability.Approximate, section.GetAvailability(SectionProperty.ShearCenter), section.Name);
            }
        }

        [TestMethod]
        public void FilletsAndTaperedFlangesAreInTheRegion()
        {
            // rolled sections: the outline with the fillets (and the tapered flanges) is solved, not the sharp one
            var upn = new SectionCTaperFlange(200, 8.5, 75, 11.5, 0.08, 11.5, 6, (75 - 8.5) / 2, "UPN 200");
            Assert.AreEqual(upn.CalculateTorsionProperties().TorsionConstant, upn.Jt);
            Assert.AreEqual(PropertyAvailability.Numerical, upn.GetAvailability(SectionProperty.TorsionConstant));

            var angle = new SectionL(100, 10, 100, 10, "L 100x10", 12, 6);
            angle.SetEdgeTypeFromSteelType(Section.SectionTypes.Rolled);
            angle.SetMechanicalProperties();
            Assert.AreEqual(angle.CalculateTorsionProperties().TorsionConstant, angle.Jt);
            Assert.IsTrue(angle.Jt > new SectionL(100, 10, 100, 10, "sharp").Jt, "the root fillet adds stiffness");
        }
    }
}
