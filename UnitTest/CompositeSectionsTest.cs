using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UnitTest
{
    /// <summary>
    /// The steel sections in the concrete sections: homogenized properties with the exact overlap, torsion of the composite section
    /// </summary>
    [TestClass]
    public class CompositeSectionsTest
    {
        private static void Rel(double expected, double actual, double tolerance = 1e-9) =>
            Assert.AreEqual(expected, actual, tolerance * Math.Max(1.0, Math.Abs(expected)), $"expected {expected}, actual {actual}");

        private static SteelSectionPosition Plate(double height, double width, double x, double y, SteelMaterial steel) =>
            new SteelSectionPosition(new SteelSection(new SectionRectangular(height, width), steel), null, 0, new Vector2d(x, y));

        [TestMethod]
        public void PartlyEncasedSteelUsesTheOverlap()
        {
            // concrete 400 x 300, steel plate 20 x 200 from y = 200 to 400: half inside. Before, the whole plate was inside or outside
            // depending on its first thin wall
            var c = ConcreteMaterialEN1992Data.C30_37;
            var steel = SteelMaterialEN1993Data.S355;
            var section = new ReinforcedConcreteSection(new SectionRectangular(300, 400), c, null,
                new System.Collections.Generic.List<SteelSectionPosition> { Plate(200, 20, 190, 200, steel) });
            double n = steel.ElasticModulusTension / c.ElasticModulusCompression;

            double area = 120000 + n * 4000 - 2000;
            Rel(area, section.GetHomogenizedArea());
            Assert.IsFalse(section.SteelSections[0].IsInsideConcrete);
            Rel(2000, section.SteelSections[0].ConcreteOverlapArea, 1e-9);

            double yc = (120000 * 150 + n * 4000 * 300 - 2000 * 250) / area;
            var properties = section.GetHomogeneizedMechanicalProperties();
            Rel(yc, properties.centroidH.Y);
            Rel(200, properties.centroidH.X);
            double jxx = 400 * Math.Pow(300, 3) / 12 + 120000 * Math.Pow(150 - yc, 2)
                + n * (20 * Math.Pow(200, 3) / 12 + 4000 * Math.Pow(300 - yc, 2))
                - (20 * Math.Pow(100, 3) / 12 + 2000 * Math.Pow(250 - yc, 2));
            Rel(jxx, properties.JxxH, 1e-9);
            double jyy = 300 * Math.Pow(400, 3) / 12 + n * 200 * Math.Pow(20, 3) / 12 - 100 * Math.Pow(20, 3) / 12;
            Rel(jyy, properties.JyyH, 1e-9);

            // GetHomogeneizedJ11 and J22 are the ones of the mechanical properties (before, without the steel sections)
            Rel(properties.J11H, section.GetHomogeneizedJ11(), 1e-12);
            Rel(properties.J22H, section.GetHomogeneizedJ22(), 1e-12);
            Rel(section.GetHomogeneizedMechanicalProperties(1.5).J11H, section.GetHomogeneizedJ11(1.5), 1e-12);
        }

        [TestMethod]
        public void SteelInsideOrOutsideAsBefore()
        {
            // inside: (n - 1) A; outside: n A
            var c = ConcreteMaterialEN1992Data.C30_37;
            var steel = SteelMaterialEN1993Data.S355;
            double n = steel.ElasticModulusTension / c.ElasticModulusCompression;
            var inside = new ReinforcedConcreteSection(new SectionRectangular(300, 400), c, null,
                new System.Collections.Generic.List<SteelSectionPosition> { Plate(100, 20, 190, 100, steel) });
            Assert.IsTrue(inside.SteelSections[0].IsInsideConcrete);
            Rel(120000 + (n - 1) * 2000, inside.GetHomogenizedArea());
            Rel(150, inside.GetHomogenizedCentroid(out _, out _).Y);

            var outside = new ReinforcedConcreteSection(new SectionRectangular(300, 400), c, null,
                new System.Collections.Generic.List<SteelSectionPosition> { Plate(100, 20, 190, -100, steel) });
            Assert.IsFalse(outside.SteelSections[0].IsInsideConcrete);
            Rel(0, outside.SteelSections[0].ConcreteOverlapArea, 1e-9);
            Rel(120000 + n * 2000, outside.GetHomogenizedArea());
        }

        [TestMethod]
        public void BuiltUpSectionsInTheConcrete()
        {
            // before, SetSteelSectionIsInside threw NotImplementedException for the sections without thin walls
            var c = ConcreteMaterialEN1992Data.C30_37;
            var steel = SteelMaterialEN1993Data.S355;
            var angles = SectionBuiltUp.DoubleAngle(new SectionL(80, 8, 80, 8, "L 80 x 8"), 10, "2L 80 x 8");
            Assert.AreEqual(4, angles.ThinWalls.Length);
            var position = new SteelSectionPosition(new SteelSection(angles, steel), null, 0, new Vector2d(200, 100), InsertionPointType.Centroid);
            var section = new ReinforcedConcreteSection(new SectionRectangular(300, 400), c, null,
                new System.Collections.Generic.List<SteelSectionPosition> { position });
            Assert.IsTrue(section.SteelSections[0].IsInsideConcrete);
            double n = steel.ElasticModulusTension / c.ElasticModulusCompression;
            Rel(120000 + (n - 1) * angles.Area, section.GetHomogenizedArea());
        }

        private static readonly ConcreteMaterial C30 = ConcreteMaterialEN1992Data.C30_37;
        private static readonly SteelMaterial S355 = SteelMaterialEN1993Data.S355;
        private static double N => S355.ElasticModulusTension / C30.ElasticModulusCompression;

        [TestMethod]
        public void FilledTubes()
        {
            // CHS: the core is the hole of the tube, the tube is outside the concrete (n As)
            var chs = new SectionCHS(323.9, 10, "CHS 323.9 x 10");
            var filled = ReinforcedConcreteSection.CreateFilledTube(chs, S355, C30, name: "CFT");
            Rel(new Shape2d(new Polygon2d(323.9 - 20, 256, new Point2d(323.9 / 2, 323.9 / 2))).GetArea(), filled.Area, 1e-9);
            Rel(Math.PI * Math.Pow(323.9 - 20, 2) / 4, filled.Area, 2e-4); // the exact outline of the tube: 256 sides, 1e-4 (before, 32: -0.6%)
            Assert.IsFalse(filled.SteelSections[0].IsInsideConcrete);
            Rel(filled.Area + N * chs.Area, filled.GetHomogenizedArea());
            Rel(323.9 / 2, filled.GetHomogenizedCentroid(out _, out _).X, 1e-9);

            // RHS with rounded corners: the core has the inner radius
            var rhs = new SectionRHSRoundedCorners(300, 200, 10, 15, 10, "RHS 300 x 200 x 10");
            filled = ReinforcedConcreteSection.CreateFilledTube(rhs, S355, C30);
            Rel(280 * 180 - (4 - Math.PI) * 10 * 10, filled.Area, 1e-4);
            Rel(filled.Area + N * rhs.Area, filled.GetHomogenizedArea());
            Point2d centroid = filled.GetHomogenizedCentroid(out _, out _);
            Rel(100, centroid.X, 1e-9);
            Rel(150, centroid.Y, 1e-9);

            Assert.ThrowsException<ArgumentException>(() => ReinforcedConcreteSection.CreateFilledTube(new SectionRectangular(100, 100), S355, C30));
        }

        [TestMethod]
        public void DoubleSkinTube()
        {
            var outer = new SectionCHS(400, 10, "CHS 400 x 10");
            var inner = new SectionCHS(200, 8, "CHS 200 x 8");
            var section = ReinforcedConcreteSection.CreateDoubleSkinTube(outer, inner, S355, C30);
            Assert.AreEqual(2, section.SteelSections.Count);
            Assert.IsFalse(section.SteelSections.Any(s => s.IsInsideConcrete));
            Rel(0, section.SteelSections[1].ConcreteOverlapArea, 1e-9);
            double hole = new Shape2d(new Polygon2d(380, 256, new Point2d(200, 200))).GetArea();
            double core = new Shape2d(new Polygon2d(200, 256, new Point2d(200, 200))).GetArea();
            Rel(hole - core, section.Area, 1e-9);
            Rel(section.Area + N * (outer.Area + inner.Area), section.GetHomogenizedArea());
            Rel(200, section.GetHomogenizedCentroid(out _, out _).Y, 1e-9);

            Assert.ThrowsException<ArgumentException>(() =>
                ReinforcedConcreteSection.CreateDoubleSkinTube(new SectionCHS(200, 10, "outer"), new SectionCHS(190, 5, "inner"), S355, C30));
        }

        [TestMethod]
        public void EncasedSections()
        {
            // HE 300 B in 500 x 500 with four bars Ø25 at the corners
            var heb = new SectionH(300, 11, 300, 19, 300, 19, "HE 300 B", 27);
            var bar = new RebarSectionCircular(25, SteelMaterialEN1992Data.B450C);
            double nb = SteelMaterialEN1992Data.B450C.ElasticModulusTension / C30.ElasticModulusCompression, ab = Math.PI * 25 * 25 / 4;
            var section = ReinforcedConcreteSection.CreateEncased(500, 500, heb, S355, C30, 0, bar, 50);
            Assert.IsTrue(section.SteelSections[0].IsInsideConcrete);
            Assert.AreEqual(4, section.RebarsCount);
            Rel(250000 + (N - 1) * heb.Area + 4 * (nb - 1) * ab, section.GetHomogenizedArea());
            var properties = section.GetHomogeneizedMechanicalProperties();
            Rel(250, properties.centroidH.X, 1e-9);
            Rel(250, properties.centroidH.Y, 1e-9);
            double bars = 4 * (nb - 1) * (Math.PI * Math.Pow(25, 4) / 64 + ab * 200 * 200);
            Rel(Math.Pow(500, 4) / 12 + (N - 1) * heb.Jxx + bars, properties.JxxH, 1e-9);

            // rotated by 90°: the weak axis of the H about X
            var rotated = ReinforcedConcreteSection.CreateEncased(500, 500, heb, S355, C30, Math.PI / 2, bar, 50);
            Rel(Math.Pow(500, 4) / 12 + (N - 1) * heb.Jyy + bars, rotated.GetHomogeneizedMechanicalProperties().JxxH, 1e-9);

            Assert.ThrowsException<ArgumentException>(() => ReinforcedConcreteSection.CreateEncased(250, 250, heb, S355, C30));

            // in a circle of 600 with 8 bars
            var circular = ReinforcedConcreteSection.CreateEncasedCircular(600, heb, S355, C30, 0, bar, 8, 50);
            Assert.IsTrue(circular.SteelSections[0].IsInsideConcrete);
            Assert.AreEqual(8, circular.RebarsCount);
            Rel(300, circular.GetHomogenizedCentroid(out _, out _).X, 1e-9);

            // partially encased: the concrete between the flanges, flush with their tips
            var partially = ReinforcedConcreteSection.CreatePartiallyEncased(heb, S355, C30);
            Assert.IsTrue(partially.SteelSections[0].IsInsideConcrete);
            Rel(300 * 300 + (N - 1) * heb.Area, partially.GetHomogenizedArea());
            Assert.ThrowsException<ArgumentException>(() =>
                ReinforcedConcreteSection.CreatePartiallyEncased(new SectionH(300, 11, 300, 19, 200, 19, "mono"), S355, C30));
        }

        [TestMethod]
        public void SlabOnGirdersWithHaunches()
        {
            // slab 4000 x 250 on two IPE 600 at X = 1000 and 3000, haunches 100 high widening 1:1 (220 at the bottom, 420 at the top)
            var ipe = new SectionH(600, 12, 220, 19, 220, 19, "IPE 600", 24);
            var section = ReinforcedConcreteSection.CreateSlabOnGirders(4000, 250, C30, new[] { ((Section)ipe, 1000.0), ((Section)ipe, 3000.0) }, S355, 100, 1);
            Rel(4000 * 250 + 2 * (220 + 420) / 2.0 * 100, section.Area, 1e-9);
            Assert.AreEqual(2, section.SteelSections.Count);
            Assert.IsFalse(section.SteelSections.Any(s => s.IsInsideConcrete));
            Point2d top = section.SteelSections[0].PositionToGlobal(new Point2d(110, 600));
            Rel(1000, top.X, 1e-9);
            Rel(-100, top.Y, 1e-9);
            Rel(section.Area + 2 * N * ipe.Area, section.GetHomogenizedArea());
            Rel(2000, section.GetHomogenizedCentroid(out _, out _).X, 1e-9);

            // without haunches: the rectangle
            var plain = ReinforcedConcreteSection.CreateSlabOnGirders(4000, 250, C30, new[] { ((Section)ipe, 2000.0) }, S355);
            Rel(1e6, plain.Area);
            Rel(-600, plain.SteelSections[0].PositionToGlobal(new Point2d(0, 0)).Y, 1e-9);

            Assert.ThrowsException<ArgumentException>(() =>
                ReinforcedConcreteSection.CreateSlabOnGirders(4000, 250, C30, new[] { ((Section)ipe, 100.0) }, S355, 100, 1));

            // bars: as many as fit between the limits, centred
            Assert.AreEqual(7, plain.AddRebarRow(new RebarSectionCircular(12, SteelMaterialEN1992Data.B450C), 200, 150, 50, 1000));
            Assert.AreEqual(7, plain.RebarsCount);
            Rel(75, plain.GetRebars().Min(r => r.Position.X), 1e-12);
        }

        [TestMethod]
        public void SlabOnSteelBox()
        {
            var box = new SectionSteelBox(1000, 15, 400, 20, 1300, 25, 1600, 1200);
            var section = ReinforcedConcreteSection.CreateSlabOnSteelBox(2400, 250, C30, box, S355, 50, 0.5);
            Point2d middleTop = section.SteelSections[0].PositionToGlobal(new Point2d(box.Width / 2, box.Height));
            Rel(1200, middleTop.X, 1e-9);
            Rel(-50, middleTop.Y, 1e-9);
            Rel(2400 * 250 + 2 * (400 + 450) / 2.0 * 50, section.Area, 1e-9);
            SectionTorsionProperties torsion = section.CalculateHomogenizedTorsionProperties();
            Assert.IsTrue(torsion.IsSolved, torsion.Error);
            Rel(1200, torsion.ShearCenter.X, 1e-3);
        }

        [TestMethod]
        public void MirroredSteelSections()
        {
            // two angles L 100 x 10 in a 400 x 300 rectangle, one mirrored about its vertical axis: the section is symmetric
            var angle = new SectionL(100, 10, 100, 10, "L 100 x 10");
            var right = new SteelSectionPosition(new SteelSection(angle, S355), null, 0, new Vector2d(250, 50));
            var left = new SteelSectionPosition(new SteelSection(angle, S355), null, 0, new Vector2d(150, 50), mirrorX: true, mirrorY: false);
            Assert.IsTrue(left.MirrorX);
            Point2d global = left.PositionToGlobal(new Point2d(10, 50));
            Rel(140, global.X);
            Rel(100, global.Y);
            Point2d back = left.PositionToLocal(global);
            Rel(10, back.X);
            Rel(50, back.Y);

            var section = new ReinforcedConcreteSection(new SectionRectangular(300, 400), C30, null,
                new System.Collections.Generic.List<SteelSectionPosition> { right, left });
            Assert.IsTrue(section.SteelSections.All(s => s.IsInsideConcrete));
            var properties = section.GetHomogeneizedMechanicalProperties();
            Rel(200, properties.centroidH.X, 1e-9);
            Assert.AreEqual(0, properties.JxyH, 1e-9 * properties.JxxH);

            // mirrored about its horizontal axis too: a rotation of 180°
            var both = new SteelSectionPosition(new SteelSection(angle, S355), null, 0, new Vector2d(0, 0), true, true);
            var rotated = new SteelSectionPosition(new SteelSection(angle, S355), null, Math.PI, new Vector2d(0, 0));
            Rel(rotated.PositionToGlobal(new Point2d(30, 70)).X, both.PositionToGlobal(new Point2d(30, 70)).X, 1e-12);
            Rel(rotated.PositionToGlobal(new Point2d(30, 70)).Y, both.PositionToGlobal(new Point2d(30, 70)).Y, 1e-12);

#pragma warning disable SYSLIB0011
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var stream = new System.IO.MemoryStream())
            {
                formatter.Serialize(stream, left);
                stream.Position = 0;
                var copy = (SteelSectionPosition)formatter.Deserialize(stream);
                Assert.IsTrue(copy.MirrorX);
                Assert.IsFalse(copy.MirrorY);
                Rel(140, copy.PositionToGlobal(new Point2d(10, 50)).X);
            }
#pragma warning restore SYSLIB0011
        }

        [TestMethod]
        public void HomogenizedPropertiesWithCreepWithoutSteel()
        {
            // without bars and steel sections the properties of the concrete (before, all zero)
            var section = new ReinforcedConcreteSection(new SectionRectangular(500, 300), C30);
            var properties = section.GetHomogeneizedMechanicalProperties(2.0);
            Rel(150000, properties.areaH);
            Rel(150, properties.centroidH.X);
            Rel(250, properties.centroidH.Y);
            Rel(300 * Math.Pow(500, 3) / 12, properties.JxxH);
            Rel(properties.J11H, section.GetHomogeneizedMechanicalProperties().J11H);
            Rel(150000 * 250, properties.SxH);
        }

        [TestMethod]
        public void TorsionOfTwoRegionsOfTheSameMaterialIsTheOneOfTheirUnion()
        {
            // slab 1000 x 200 and a plate 300 x 20 under it, of a "steel" with the moduli of the concrete: the same as the T of one material
            var c = ConcreteMaterialEN1992Data.C30_37;
            var same = new SteelMaterial("same", c.ElasticModulusCompression, 355, 510) { Ni = c.Ni };
            var section = new ReinforcedConcreteSection(new SectionRectangular(200, 1000), c, null,
                new System.Collections.Generic.List<SteelSectionPosition> { Plate(20, 300, 350, -20, same) });
            SectionTorsionProperties composite = section.CalculateHomogenizedTorsionProperties(0, 5);
            Assert.IsTrue(composite.IsSolved, composite.Error);

            var union = new Section(new Shape2d(new Polygon2d(new[]
            {
                new Point2d(0, 0), new Point2d(350, 0), new Point2d(350, -20), new Point2d(650, -20), new Point2d(650, 0), new Point2d(1000, 0),
                new Point2d(1000, 200), new Point2d(0, 200),
            })), "T");
            union.SetMechanicalProperties();
            SectionTorsionProperties single = union.CalculateTorsionProperties(5);
            Rel(single.TorsionConstant, composite.TorsionConstant, 1e-4);
            Rel(single.WarpingConstant, composite.WarpingConstant, 1e-3);
            Assert.AreEqual(single.ShearCenter.Y, composite.ShearCenter.Y, 0.01);
            Assert.AreEqual(500, composite.ShearCenter.X, 1e-3);
        }

        [TestMethod]
        public void TorsionOfAFilledTube()
        {
            // CHS 200 x 10 filled with concrete: no warping, the torsion constant is Jc + Gs / Gc Js (the core is the hole of the tube)
            var c = ConcreteMaterialEN1992Data.C30_37;
            var steel = SteelMaterialEN1993Data.S355;
            var tube = new SectionCHS(200, 10, "CHS 200 x 10");
            var hole = tube.GetPlasticShape().Holes[0];
            var core = new Section(new Shape2d(new Polygon2d(Enumerable.Range(0, hole.Count).Select(i => new Point2d(hole[i].X, hole[i].Y)).ToArray())), "core");
            core.SetMechanicalProperties();
            var section = new ReinforcedConcreteSection(core, c, null,
                new System.Collections.Generic.List<SteelSectionPosition> { new SteelSectionPosition(new SteelSection(tube, steel), null, 0, null) });
            Assert.IsFalse(section.SteelSections[0].IsInsideConcrete);

            SectionTorsionProperties composite = section.CalculateHomogenizedTorsionProperties();
            Assert.IsTrue(composite.IsSolved, composite.Error);
            double gc = c.ElasticModulusCompression / (2 * (1 + c.Ni)), gs = steel.ElasticModulusTension / (2 * (1 + steel.Ni));
            double expected = core.CalculateTorsionProperties().TorsionConstant + gs / gc * tube.CalculateTorsionProperties().TorsionConstant;
            Rel(expected, composite.TorsionConstant, 1e-3);
            Assert.AreEqual(100, composite.ShearCenter.X, 1e-3);
            Assert.AreEqual(100, composite.ShearCenter.Y, 1e-3);
        }

        [TestMethod]
        public void SteelBoxClosedByTheSlab()
        {
            // open steel box closed by the slab: a closed cell, the torsion constant is many times the one of the separate parts and close to
            // the one of Bredt with the slab as a wall of thickness 250 Gc / Gs
            var c = ConcreteMaterialEN1992Data.C30_37;
            var steel = SteelMaterialEN1993Data.S355;
            var box = new SectionSteelBox(1000, 15, 400, 20, 1300, 25, 1600, 1200);
            var position = new SteelSectionPosition(new SteelSection(box, steel), null, 0, new Vector2d(1200 - box.Width / 2, -1000));
            var section = new ReinforcedConcreteSection(new SectionRectangular(250, 2400), c, null,
                new System.Collections.Generic.List<SteelSectionPosition> { position });

            SectionTorsionProperties composite = section.CalculateHomogenizedTorsionProperties();
            Assert.IsTrue(composite.IsSolved, composite.Error);
            double gc = c.ElasticModulusCompression / (2 * (1 + c.Ni)), gs = steel.ElasticModulusTension / (2 * (1 + steel.Ni));
            double separate = ((Section)section.SectionShape).CalculateTorsionProperties().TorsionConstant + gs / gc * box.CalculateTorsionProperties().TorsionConstant;
            Assert.IsTrue(composite.TorsionConstant > 20 * separate, $"{composite.TorsionConstant} {separate}");

            // Bredt in the steel: middle lines of the webs (spacing 1600 at the slab, 1200 at the bottom), slab and bottom flange. It is an
            // estimate: the thick slab, its overhangs and the top flanges at the corners of the cell add about 17% (2.83e11 against 2.42e11)
            double h = 1000 - 25.0 / 2 + 250.0 / 2, web = Math.Sqrt(h * h + 200.0 * 200.0);
            double am = (1600 + 1200) / 2.0 * h;
            double sum = 1600 / (250 * gc / gs) + 1200 / 25.0 + 2 * web / 15;
            double bredt = 4 * am * am / sum * gs / gc;
            Assert.IsTrue(composite.TorsionConstant > bredt && composite.TorsionConstant < 1.2 * bredt, $"{composite.TorsionConstant} {bredt}");
        }
    }
}
