using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using System;

namespace GPC.Model.Design.Steel
{
    /// <summary>Owned scalar snapshot of a constant-thickness steel tube, mm and MPa.
    /// One physical section, not a homogenised wall strip. Strengths refer to the product thickness.
    /// No net/effective properties are inferred from the gross section.</summary>
    public sealed class CircularHollowSteelSection
    {
        public double Diameter { get; }
        public double Thickness { get; }
        public double Area { get; }
        public double Inertia { get; }
        public double ElasticModulus { get; }
        public double YieldStrength { get; }
        public double UltimateStrength { get; }
        public int NominalGrade { get; }
        public SteelMaterial.SteelTypes MaterialType { get; }
        public string MaterialSource { get; }
        public bool UnperforatedConfirmed { get; }
        public bool IsBridge { get; }

        public CircularHollowSteelSection(ISteelSection section, int nominalGrade, string materialSource,
            bool unperforatedConfirmed, bool isBridge = false)
        {
            if (section == null || section.SteelMaterial == null) throw new ArgumentNullException(nameof(section));
            if (!(section.SectionShape is SectionCHS shape)) throw new ArgumentException("A circular hollow steel section is required.");
            Diameter = shape.Diameter; Thickness = shape.Thickness; Area = section.Area; Inertia = section.J11;
            ElasticModulus = section.SteelMaterial.E; YieldStrength = section.SteelMaterial.Fyk; UltimateStrength = section.SteelMaterial.Fu;
            MaterialType = section.SteelMaterial.SteelType; NominalGrade = nominalGrade; MaterialSource = materialSource ?? "";
            UnperforatedConfirmed = unperforatedConfirmed; IsBridge = isBridge;
            if (!Positive(Diameter) || !Positive(Thickness) || 2 * Thickness >= Diameter || !Positive(Area) || !Positive(Inertia)
                || !Positive(ElasticModulus) || !Positive(YieldStrength) || !Positive(UltimateStrength)
                || YieldStrength > UltimateStrength || ElasticModulus <= YieldStrength || nominalGrade <= 0)
                throw new ArgumentException("Finite positive geometry and material properties required.");
        }
        private static bool Positive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
    }
}
