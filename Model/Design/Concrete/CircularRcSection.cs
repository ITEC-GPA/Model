using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;

namespace GPC.Model.Design.Concrete
{
    /// <summary>Owned ordinary RC circular section definition, mm and MPa. Cover is clear to the outer hoop.
    /// A single uniform ring of bars; fresh independent material and mesh instances on every CreateSection call.
    /// Gross properties are exact circular values; section integration uses an explicitly declared inscribed polygon.</summary>
    public sealed class CircularRcSection
    {
        public double Diameter { get; }
        public double Cover { get; }
        public int BarCount { get; }
        public double BarDiameter { get; }
        public double LinkDiameter { get; }
        public double LinkSpacing { get; }
        public double Fck { get; }
        public double Fyk { get; }
        public double SteelModulus { get; }
        public double UltimateSteelStrain { get; }
        public int OutlineDivisions { get; }
        public string MaterialSource { get; }
        public double BarRadius => Diameter / 2 - Cover - LinkDiameter - BarDiameter / 2;
        public double GrossArea => new SectionCircular(Diameter).Area;
        public double GrossInertia => new SectionCircular(Diameter).J11;
        public double ConcreteModulus => CreateConcrete().E;
        public double GrossFlexuralRigidity => ConcreteModulus * GrossInertia;

        public CircularRcSection(double diameter, double cover, int barCount, double barDiameter,
            double linkDiameter, double linkSpacing, double fck, double fyk, double steelModulus,
            double ultimateSteelStrain, string materialSource, int outlineDivisions = 64)
        {
            Diameter = diameter; Cover = cover; BarCount = barCount; BarDiameter = barDiameter;
            LinkDiameter = linkDiameter; LinkSpacing = linkSpacing; Fck = fck; Fyk = fyk;
            SteelModulus = steelModulus; UltimateSteelStrain = ultimateSteelStrain;
            MaterialSource = materialSource ?? ""; OutlineDivisions = outlineDivisions;
            if (new[] { diameter, barDiameter, linkDiameter, linkSpacing, fck, fyk, steelModulus, ultimateSteelStrain }.Any(v => !Positive(v))
                || double.IsNaN(cover) || double.IsInfinity(cover) || cover < 0 || barCount < 4 || barCount > 512 || barCount % 2 != 0
                || fck < 16 || fck > 50 || steelModulus <= fyk || ultimateSteelStrain <= fyk / steelModulus
                || outlineDivisions < 32 || outlineDivisions > 256 || outlineDivisions % 4 != 0 || BarRadius <= 0
                || 2 * BarRadius * Math.Sin(Math.PI / barCount) <= barDiameter)
                throw new ArgumentException("Finite ordinary circular RC section with a nonoverlapping even ring of bars required; C16-C50 and 32-256 outline divisions in multiples of four.");
            var outline = new Polygon2d(Diameter, OutlineDivisions);
            var ring = new Polygon2d(2 * BarRadius, BarCount);
            for (int i = 0; i < BarCount; i++)
                if (!outline.IsPointInside(ring[i]) || outline.DistanceTo(ring[i]) < BarDiameter / 2)
                    throw new ArgumentException("A bar intersects the numerical outline; increase cover or explicitly refine the section polygon.");
        }

        public ConcreteMaterialEN1992 CreateConcrete() => new ConcreteMaterialEN1992("Circular RC", Fck,
            ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
        public SteelMaterial CreateSteel() => new SteelMaterial("Circular RC bars", SteelModulus, Fyk, Fyk, UltimateSteelStrain,
            SteelMaterial.StressStrainCurveType.ElasticPerfectPlastic, SteelMaterial.SteelTypes.Rebar);
        public ReinforcedConcreteSection CreateSection()
        {
            var section = new ReinforcedConcreteSection(new Shape2d(new Polygon2d(Diameter, OutlineDivisions)), CreateConcrete());
            var bar = new RebarSectionCircular(BarDiameter, CreateSteel());
            var ring = new Polygon2d(2 * BarRadius, BarCount);
            section.AddRebars(Enumerable.Range(0, BarCount).Select(i => new ReinforcedConcreteRebar(bar, ring[i])));
            return section;
        }
        private static bool Positive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
    }
}
