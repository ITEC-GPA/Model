using System;
using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Results;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using GPC.Model.Standards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// Corrections of calculation errors found in the review (September 2026): ACI 318 β1, rotation of the displacements, Jyy of the C
    /// sections with fillets. The interlayer is tested in InterlayerMaterialTest. Bug review of October 2026: design stress of the tendons
    /// </summary>
    [TestClass]
    public class CalculationFixesTest
    {
        [TestMethod]
        public void TendonDesignStressUsesTheTotalStrain()
        {
            // Bug review 15/10/2026 (SLU-6). Strand fp(0,1)k = 1670, fpk = 1860, Ep = 195000, εuk = 3.5 %, γs = 1.15, σp0 = 1500 MPa:
            // εp = 7.692 ‰ beyond εpyd = 7.447 ‰. With the strain and the prestrain passed apart, the sign and the ultimate strain came from the
            // mechanical strain ε: 1480.5 MPa at ε = -0.1 ‰, 1500 MPa (> fpd = 1452.2) at ε = 0, 1642.2 MPa at ε = 28 ‰ (total strain beyond εuk).
            var tendon = new SteelMaterial("t", 195000, 1670, 1860, 0.035, SteelMaterial.StressStrainCurveType.ElasticHardening, SteelMaterial.SteelTypes.Tendon);
            var ntc = new StandardNTC2018Concrete();
            double epsilonP = 1500.0 / 195000;

            // Expected: the declared design law of the total strain t = ε + εp, i.e. Ep t up to εpyd = fpd / Ep, then the hardening branch of the
            // characteristic law moved to start at (εpyd, fpd): fpd + (fpk - fp(0,1)k) / (εuk - fp(0,1)k / Ep) (t - εpyd), zero for t > εuk.
            // Computed independently in Python (scratchpad agent-model/reference.py)
            (double Strain, double Stress)[] rows =
            {
                (-0.0005, 1402.5),
                (-0.0001, 1453.217940),
                (0, 1453.936659),
                (0.0001, 1454.655379),
                (0.002, 1468.311053),
                (0.028, 0),
            };
            foreach (var (strain, stress) in rows)
            {
                Assert.AreEqual(stress, tendon.CalculateDesignStress(ntc, strain, epsilonP), 1e-5, $"ε = {strain}");
                // the section solver passes the total strain: the report of the strain plane must give the same stress
                Assert.AreEqual(tendon.CalculateDesignStress(ntc, strain + epsilonP), tendon.CalculateDesignStress(ntc, strain, epsilonP), 0.0, $"ε = {strain}");
            }

            // below the design yield strain nothing changes (σp0 = 1400 MPa): Ep (ε + εp)
            Assert.AreEqual(1400 - 0.0001 * 195000, tendon.CalculateDesignStress(ntc, -0.0001, 1400.0 / 195000), 1e-9);
        }

        [TestMethod]
        public void DesignStressFromTheCharacteristicStressUsesTheTotalStrain()
        {
            // The overloads with the characteristic stress (Model Code 2010, EN 1993-1-1, AISC 360) took the branch (tension or compression) and the
            // ultimate strain from ε alone and the yield strain from ε + εp, the flaw of SLU-6 (bug review 15/10/2026, user decision 16/10). The
            // characteristic stress passed is the one of the total strain t = ε + εp. Expected: the declared law of the overload on t, |t| ≤ εyd:
            // σk(t); |t| > εu: 0; otherwise σk(t) − sign(t) (fy − fyd). Independent Python: scratchpad agent-concrete-tension/ref_steel4.py.
            var tendon = new SteelMaterial("t", 195000, 1670, 1860, 0.035, SteelMaterial.StressStrainCurveType.ElasticHardening, SteelMaterial.SteelTypes.Tendon);
            var ntc = new StandardNTC2018Concrete();
            double epsilonP = 1500.0 / 195000;
            // characteristic law of the tendon: Ep t up to fp(0,1)k / Ep, then linear up to (εuk, fpk), zero beyond εuk
            double Tendon(double t) => Characteristic(t, 195000, 1670, 1860, 0.035);
            (double Strain, double Stress)[] rows =
            {
                (-0.04, -1622.823768),  // t = -32.3 ‰, compression beyond yield (before: 0, ε below εuc)
                (-0.0005, 1402.5),      // t = 7.192 ‰ ≤ εpyd = 7.447 ‰: σk (before: 1620.33, compression branch from ε)
                (-0.0001, 1262.673913), // t = 7.592 ‰ > εpyd: σk − 217.826 (before: 1698.33)
                (0, 1282.173913),       // unchanged
                (0.026, 1632.775271),   // unchanged
                (0.028, 0),             // t = 35.69 ‰ > εuk (before: -217.83)
            };
            foreach (var (strain, stress) in rows)
                Assert.AreEqual(stress, tendon.CalculateDesignStress(ntc, Tendon(strain + epsilonP), strain, epsilonP), 1e-5, $"tendon ε = {strain}");

            // S355 with hardening (E 210000, fy 355, fu 510, εu 15 %) and εp = 2 ‰: EN 1993-1-1 with γM0 = 1.05, AISC 360 with fyd = fy
            var structural = new SteelMaterial("s", 210000, 355, 510, 0.15, SteelMaterial.StressStrainCurveType.ElasticHardening, SteelMaterial.SteelTypes.Structural);
            double Structural(double t) => Characteristic(t, 210000, 355, 510, 0.15);
            var ec3 = new StandardEN1993p11 { GammaM0 = 1.05 };
            var aisc = new StandardAISC360p16();
            Assert.AreEqual(-491.527570733, structural.CalculateDesignStress(ec3, Structural(-0.1485), -0.1505, 0.002), 1e-6, "EN 1993 t = -148.5 ‰ (before: 0)");
            Assert.AreEqual(315, structural.CalculateDesignStress(ec3, Structural(0.0015), -0.0005, 0.002), 1e-9, "EN 1993 t = 1.5 ‰ elastic (before: 331.90)");
            Assert.AreEqual(0, structural.CalculateDesignStress(ec3, Structural(0.151), 0.149, 0.002), 0.0, "EN 1993 t = 151 ‰ > εu (before: -16.90)");
            Assert.AreEqual(-508.432332638, structural.CalculateDesignStress(aisc, Structural(-0.1485), -0.1505, 0.002), 1e-6, "AISC t = -148.5 ‰ (before: 0)");
            Assert.AreEqual(315, structural.CalculateDesignStress(aisc, Structural(0.0015), -0.0005, 0.002), 1e-9, "AISC t = 1.5 ‰");

            // With εp = 0 the overloads are the code before the fix, bit for bit (the branch and the ultimate strain of ε are those of ε + 0)
            var rebar = new SteelMaterial("r", 200000, 450, 540, 0.075, SteelMaterial.StressStrainCurveType.ElasticHardening, SteelMaterial.SteelTypes.Rebar);
            foreach (double strain in new[] { -0.2, -0.075, -0.04, -0.0035, -0.002, -0.0005, -0.0, 0, 0.0005, 0.002, 0.0035, 0.01, 0.034, 0.036, 0.074, 0.076, 0.149, 0.2 })
            {
                foreach (var steel in new[] { tendon, rebar })
                    Assert.AreEqual(LegacyCharacteristicOverload(steel, steel.GetStress(strain), strain, 0, steel.CalculateDesignYieldingStrainTension(ntc),
                        steel.CalculateDesignYieldingStressTension(ntc), steel.CalculateDesignYieldingStrainCompression(ntc), steel.CalculateDesignYieldingStressCompression(ntc)),
                        steel.CalculateDesignStress(ntc, steel.GetStress(strain), strain, 0), 0.0, $"{steel.SteelType} ε = {strain}");
                Assert.AreEqual(LegacyCharacteristicOverload(structural, structural.GetStress(strain), strain, 0, structural.CalculateDesignYieldingStrainTension(ec3),
                    structural.CalculateDesignYieldingStressTension(ec3), structural.CalculateDesignYieldingStrainCompression(ec3), structural.CalculateDesignYieldingStressCompression(ec3)),
                    structural.CalculateDesignStress(ec3, structural.GetStress(strain), strain, 0), 0.0, $"EN 1993 ε = {strain}");
                Assert.AreEqual(LegacyCharacteristicOverload(structural, structural.GetStress(strain), strain, 0, structural.CalculateDesignYieldingStrainTension(aisc),
                    structural.CalculateDesignYieldingStressTension(aisc), structural.CalculateDesignYieldingStrainCompression(aisc), structural.CalculateDesignYieldingStressCompression(aisc)),
                    structural.CalculateDesignStress(aisc, structural.GetStress(strain), strain, 0), 0.0, $"AISC ε = {strain}");
            }
        }

        /// <summary>Bilinear characteristic law with hardening: E t up to fy / E, then linear up to (εu, fu), zero beyond εu (odd in t)</summary>
        private static double Characteristic(double t, double e, double fy, double fu, double eu)
        {
            double a = Math.Abs(t), ey = fy / e;
            double s = a <= ey ? e * a : a <= eu ? fy + (fu - fy) / (eu - ey) * (a - ey) : 0;
            return t >= 0 ? s : -s;
        }

        /// <summary>The overloads with the characteristic stress before 16/10/2026 (branch and ultimate strain of ε), for the check at εp = 0</summary>
        private static double LegacyCharacteristicOverload(SteelMaterial steel, double stress, double strain, double epsilonP,
            double strainYdTension, double fydTension, double strainYdCompression, double fydCompression)
        {
            if (strain >= 0)
            {
                if (Math.Abs(strain + epsilonP) <= strainYdTension) return stress;
                if (strain > steel.StrainUTension) return 0;
                return stress - (steel.StressYTension - fydTension);
            }
            if (Math.Abs(strain + epsilonP) <= strainYdCompression) return stress;
            if (strain < steel.StrainUCompression) return 0;
            return stress - (steel.StressYCompression - fydCompression);
        }

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
            SteelSection upn = C(300, 10, 100, 16, 100, 16, 16, 8);
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

        [TestMethod]
        public void HSectionsAgainstTheExactShape()
        {
            // HEB 300 (ArcelorMittal: A = 149.1 cm², Iy = 25170 cm⁴, Iz = 8563 cm⁴). Before, Jyy was 0.25% greater: the own moment of the
            // fillets was the one about the far side and their centroids were on the axis of the web and R / 6 from the flanges
            CompareHWithTheExactShape(300, 11, 300, 19, 300, 19, 27);
            SteelSection heb = H(300, 11, 300, 19, 300, 19, 27);
            Assert.AreEqual(14910, heb.Area, 0.0005 * 14910);
            Assert.AreEqual(251.7e6, heb.Jxx, 0.0005 * 251.7e6);
            Assert.AreEqual(85.63e6, heb.Jyy, 0.0005 * 85.63e6);

            // different flanges: before, the corners were not in the centroid
            CompareHWithTheExactShape(400, 10, 200, 16, 300, 20, 21);
            CompareHWithTheExactShape(400, 10, 300, 20, 200, 16, 21);

            // welded: right triangles with legs 1.41 a (before, own moment a⁴ / 24 and centroid a / 8.5 from the flange and on the axis of the web)
            CompareHWithTheExactShape(600, 12, 250, 20, 350, 25, 7, true);
        }

        private static SteelSection C(double h, double tw, double lengthTop, double tTop, double lengthBottom, double tBottom, double r1, double r2,
            bool welded = false) =>
            new SteelSection(new SectionC(h, tw, lengthTop, tTop, lengthBottom, tBottom, string.Empty, r1, r2), SteelMaterialEN1993Data.S355,
                welded ? Section.SectionTypes.Welded : Section.SectionTypes.Rolled);

        private static SteelSection H(double h, double tw, double widthTop, double tTop, double widthBottom, double tBottom, double r, bool welded = false) =>
            new SteelSection(new SectionH(h, tw, widthTop, tTop, widthBottom, tBottom, string.Empty, r), SteelMaterialEN1993Data.S355,
                welded ? Section.SectionTypes.Welded : Section.SectionTypes.Rolled);

        /// <summary>
        /// Compares the properties of the C section with the ones of its polygon with the arcs of the fillets (or the welds)
        /// </summary>
        private static void CompareWithTheExactShape(double h, double tw, double lengthTop, double tTop, double lengthBottom, double tBottom, double r1, double r2,
            bool welded = false)
        {
            var points = new List<(double X, double Y)> { (0, 0), (lengthBottom, 0) };
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

            Compare(C(h, tw, lengthTop, tTop, lengthBottom, tBottom, r1, r2, welded), points, h);
        }

        /// <summary>
        /// Compares the properties of the H section with the ones of its polygon with the arcs of the fillets (or the welds)
        /// </summary>
        private static void CompareHWithTheExactShape(double h, double tw, double widthTop, double tTop, double widthBottom, double tBottom, double r,
            bool welded = false)
        {
            double xWeb = Math.Max(widthTop, widthBottom) / 2, left = xWeb - tw / 2, right = xWeb + tw / 2, leg = 1.41 * r;
            var points = new List<(double X, double Y)> { (xWeb - widthBottom / 2, 0), (xWeb + widthBottom / 2, 0), (xWeb + widthBottom / 2, tBottom) };
            if (welded)
                points.AddRange(new[] { (right + leg, tBottom), (right, tBottom + leg), (right, h - tTop - leg), (right + leg, h - tTop) });
            else
            {
                Arc(points, right + r, tBottom + r, r, 270, 180);
                Arc(points, right + r, h - tTop - r, r, 180, 90);
            }
            points.AddRange(new[] { (xWeb + widthTop / 2, h - tTop), (xWeb + widthTop / 2, h), (xWeb - widthTop / 2, h), (xWeb - widthTop / 2, h - tTop) });
            if (welded)
                points.AddRange(new[] { (left - leg, h - tTop), (left, h - tTop - leg), (left, tBottom + leg), (left - leg, tBottom) });
            else
            {
                Arc(points, left - r, h - tTop - r, r, 90, 0);
                Arc(points, left - r, tBottom + r, r, 0, -90);
            }
            points.Add((xWeb - widthBottom / 2, tBottom));

            Compare(H(h, tw, widthTop, tTop, widthBottom, tBottom, r, welded), points, h);
        }

        /// <summary>
        /// Compares the area, the centroid and the moments of inertia of the section with the ones of the polygon (counterclockwise)
        /// </summary>
        private static void Compare(SteelSection section, List<(double X, double Y)> points, double size)
        {
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

            string values = $"A {section.Area} {a}, C ({section.Centroid.X}, {section.Centroid.Y}) ({cx}, {cy}), Jxx {section.Jxx} {jxx}, " +
                $"Jyy {section.Jyy} {jyy}, Jxy {section.Jxy} {jxy}";
            Assert.AreEqual(a, section.Area, 1e-6 * a, values);
            Assert.AreEqual(cx, section.Centroid.X, 1e-6 * size, values);
            Assert.AreEqual(cy, section.Centroid.Y, 1e-6 * size, values);
            Assert.AreEqual(jxx, section.Jxx, 1e-6 * jxx, values);
            Assert.AreEqual(jyy, section.Jyy, 1e-6 * jyy, values);
            Assert.AreEqual(jxy, section.Jxy, 1e-6 * Math.Sqrt(jxx * jyy), values);
        }

        /// <summary>
        /// Adds the points of an arc from the angle start to the angle end (degrees, in the direction of the sign of end - start)
        /// </summary>
        private static void Arc(List<(double X, double Y)> points, double xCenter, double yCenter, double radius, double start, double end)
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
