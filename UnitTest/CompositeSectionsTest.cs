using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
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
            var core = new Section(new Shape2d(new Polygon2d(Enumerable.Range(0, tube.Shape.Holes[0].Count)
                .Select(i => new Point2d(tube.Shape.Holes[0][i].X, tube.Shape.Holes[0][i].Y)).ToArray())), "core");
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
