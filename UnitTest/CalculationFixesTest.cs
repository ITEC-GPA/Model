using System;
using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Results;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// Corrections of calculation errors found in the review (September 2026): ACI 318 β1, rotation of the displacements, Jyy of the C
    /// sections with fillets. The interlayer is tested in InterlayerMaterialTest
    /// </summary>
    [TestClass]
    public class CalculationFixesTest
    {
        [TestMethod]
        public void Aci318Beta1()
        {
            // ACI 318-14/19 Table 22.2.2.4.3 (SI). Before: 0.85 up to 30 MPa, 0.65 from 58 MPa and in between an interpolation with the
            // arguments in the wrong order (-5451 for 40 MPa)
            Assert.AreEqual(0.85, Beta1(20), 1e-12);
            Assert.AreEqual(0.85, Beta1(28), 1e-12);
            Assert.AreEqual(0.85 - 0.05 * 12 / 7.0, Beta1(40), 1e-12);
            Assert.AreEqual(0.85 - 0.05 * 20 / 7.0, Beta1(48), 1e-12);
            Assert.AreEqual(0.65, Beta1(55), 1e-12);
            Assert.AreEqual(0.65, Beta1(70), 1e-12);

            // the stress block starts at (1 - β1) εcu
            var concrete = new ConcreteMaterialACI318("", 40, ConcreteMaterial.CompressionStressStrainDiagrams.StressBlock);
            Assert.AreEqual(-0.003 * (1 - Beta1(40)), concrete.StrainYCompression, 1e-12);
        }

        private static double Beta1(double fc) => new ConcreteMaterialACI318("", fc, ConcreteMaterial.CompressionStressStrainDiagrams.StressBlock).GetBeta1();

        [TestMethod]
        public void DisplacementsInAnotherCoordinateSystem()
        {
            // new axes: V1 = global Y, V2 = -global X, V3 = global Z. Before, the components were permuted (d1 = 3, d2 = 2, d3 = -1)
            var cs = new CoordinateSystem(Point3d.Origin, new Vector3d(0, 1, 0), new Vector3d(-1, 0, 0));
            var displacement = new ResultDisplacement(CoordinateSystem.Global, 1, 2, 3, 0.1, 0.2, 0.3, 7);

            ResultDisplacement local = displacement.ToCoordinateSystem(cs);
            Assert.AreEqual(2, local.D1, 1e-12);
            Assert.AreEqual(-1, local.D2, 1e-12);
            Assert.AreEqual(3, local.D3, 1e-12);
            Assert.AreEqual(0.2, local.R1, 1e-12);
            Assert.AreEqual(-0.1, local.R2, 1e-12);
            Assert.AreEqual(0.3, local.R3, 1e-12);
            Assert.AreEqual(7, local.Id);

            // and back
            ResultDisplacement global = local.ToCoordinateSystem(CoordinateSystem.Global);
            Assert.AreEqual(1, global.D1, 1e-12);
            Assert.AreEqual(2, global.D2, 1e-12);
            Assert.AreEqual(3, global.D3, 1e-12);
            Assert.AreEqual(0.1, global.R1, 1e-12);
            Assert.AreEqual(0.2, global.R2, 1e-12);
            Assert.AreEqual(0.3, global.R3, 1e-12);
        }

        private static SteelSection RolledC(double k) =>
            new SteelSection(new SectionC(200 * k, 8.5 * k, 75 * k, 11.5 * k, 75 * k, 11.5 * k, string.Empty, 12 * k, 0), SteelMaterialEN1993Data.S275,
                Section.SectionTypes.Rolled);

        [TestMethod]
        public void JyyOfARolledCSectionWithFillets()
        {
            // all the terms must scale with the fourth power of the dimensions. Before, the transport term of the fillets was R1² - R1² d²
            // (a length squared minus a length to the fourth) instead of (R1² - π R1² / 4) d²
            SteelSection section = RolledC(1), scaled = RolledC(10);
            Assert.AreEqual(1e4, scaled.Jyy / section.Jyy, 1e-9 * 1e4);
            Assert.AreEqual(1e4, scaled.Jxx / section.Jxx, 1e-9 * 1e4);

            // the fillets add little to the thin walls
            var sharp = new SectionC(200, 8.5, 75, 11.5, 75, 11.5, string.Empty, 12, 0);
            Assert.IsTrue(Math.Abs(section.Jyy / sharp.Jyy - 1) < 0.05, $"{section.Jyy} {sharp.Jyy}");
        }

        [TestMethod]
        public void RolledCSectionsAgainstTheExactShape()
        {
            // UPN 300 without the slope of the flanges (ArcelorMittal: A = 5962.407, Jyy = 5539278.83, Jxx = 81997310.3). Before, Jyy was about
            // 5.76e6 (+4%) and Jxy 5.2e5 instead of 0: the thin walls were moved from the origin to the centroid of the section as if it were
            // their centroid, the outside fillets R2 were subtracted from the area and from the centroid but not from the moments of inertia,
            // and the wrong transport term of the fillets (R1² - R1² d²) partly compensated the two errors
            CompareWithTheExactShape(300, 10, 100, 16, 100, 16, 16, 8);
            SteelSection upn = Rolled(300, 10, 100, 16, 100, 16, 16, 8);
            Assert.AreEqual(5539278.83, upn.Jyy, 1e-6 * 5539278.83);
            Assert.AreEqual(81997310.3, upn.Jxx, 1e-6 * 81997310.3);
            Assert.AreEqual(0, upn.Jxy, 1e-9 * upn.Jxx);
            Assert.AreEqual(upn.Jyy, upn.J22, 1e-9 * upn.Jxx);

            // different flanges: Jxy is not 0
            CompareWithTheExactShape(300, 10, 120, 18, 90, 14, 15, 7.5);

            // welded: right triangles with legs 1.41 R1 in the inside corners (before, centroid R1 / 3.5 from the sides and own moment
            // (1.41 R1)⁴ / 24 instead of / 36)
            CompareWithTheExactShape(300, 10, 120, 18, 90, 14, 6, 0, true);
        }

        private static SteelSection Rolled(double h, double tw, double lengthTop, double tTop, double lengthBottom, double tBottom, double r1, double r2,
            bool welded = false) =>
            new SteelSection(new SectionC(h, tw, lengthTop, tTop, lengthBottom, tBottom, string.Empty, r1, r2), SteelMaterialEN1993Data.S355,
                welded ? Section.SectionTypes.Welded : Section.SectionTypes.Rolled);

        /// <summary>
        /// Compares the properties of the rolled C section with the ones of its polygon with the arcs of the fillets (or the
        /// welds)
        /// </summary>
        private static void CompareWithTheExactShape(double h, double tw, double lengthTop, double tTop, double lengthBottom, double tBottom, double r1, double r2,
            bool welded = false)
        {
            var points = new System.Collections.Generic.List<(double X, double Y)> { (0, 0), (lengthBottom, 0) };
            Arc(points, lengthBottom - r2, tBottom - r2, r2, 0, 90);
            if (welded)
            {
                double leg = 1.41 * r1;
                points.AddRange(new[] { (tw + leg, tBottom), (tw, tBottom + leg), (tw, h - tTop - leg), (tw + leg, h - tTop) });
            }
            else
            {
                Arc(points, tw + r1, tBottom + r1, r1, 270, 180);
                Arc(points, tw + r1, h - tTop - r1, r1, 180, 90);
            }
            Arc(points, lengthTop - r2, h - tTop + r2, r2, 270, 360);
            points.Add((lengthTop, h));
            points.Add((0, h));

            double a = 0, sx = 0, sy = 0, ixx = 0, iyy = 0, ixy = 0;
            for (int i = 0; i < points.Count; i++)
            {
                var (x0, y0) = points[i];
                var (x1, y1) = points[(i + 1) % points.Count];
                double c = x0 * y1 - x1 * y0;
                a += c / 2;
                sy += (x0 + x1) * c / 6;
                sx += (y0 + y1) * c / 6;
                ixx += (y0 * y0 + y0 * y1 + y1 * y1) * c / 12;
                iyy += (x0 * x0 + x0 * x1 + x1 * x1) * c / 12;
                ixy += (x0 * y1 + 2 * x0 * y0 + 2 * x1 * y1 + x1 * y0) * c / 24;
            }
            double cx = sy / a, cy = sx / a;
            double jxx = ixx - a * cy * cy, jyy = iyy - a * cx * cx, jxy = ixy - a * cx * cy;

            SteelSection section = Rolled(h, tw, lengthTop, tTop, lengthBottom, tBottom, r1, r2, welded);
            string values = $"A {section.Area} {a}, C ({section.Centroid.X}, {section.Centroid.Y}) ({cx}, {cy}), Jxx {section.Jxx} {jxx}, " +
                $"Jyy {section.Jyy} {jyy}, Jxy {section.Jxy} {jxy}";
            Assert.AreEqual(a, section.Area, 1e-6 * a, values);
            Assert.AreEqual(cx, section.Centroid.X, 1e-6 * h, values);
            Assert.AreEqual(cy, section.Centroid.Y, 1e-6 * h, values);
            Assert.AreEqual(jxx, section.Jxx, 1e-6 * jxx, values);
            Assert.AreEqual(jyy, section.Jyy, 1e-6 * jyy, values);
            Assert.AreEqual(jxy, section.Jxy, 1e-6 * Math.Sqrt(jxx * jyy), values);
        }

        /// <summary>
        /// Adds the points of an arc from the angle start to the angle end (degrees, in the direction of the sign of end - start)
        /// </summary>
        private static void Arc(System.Collections.Generic.List<(double X, double Y)> points, double xCenter, double yCenter, double radius, double start, double end)
        {
            const int segments = 2000;
            for (int i = 0; i <= segments; i++)
            {
                double angle = (start + (end - start) * i / segments) * Math.PI / 180;
                points.Add((xCenter + radius * Math.Cos(angle), yCenter + radius * Math.Sin(angle)));
            }
        }
    }
}
