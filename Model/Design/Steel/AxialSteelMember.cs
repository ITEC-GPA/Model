using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using System;

namespace GPC.Model.Design.Steel
{
    public enum AxialSteelSectionKind { Other, CircularBar, CircularHollow }

    /// <summary>Owned scalar snapshot of an ordinary steel member. N, mm, MPa.
    /// Material strengths must already account for product thickness; NominalGrade identifies
    /// the steel grade, not the reduced yield strength. NetArea concerns ordinary connection holes,
    /// not threaded bars or prestressing tendons. Effective lengths are in the two principal planes.</summary>
    public sealed class AxialSteelMember
    {
        public AxialSteelSectionKind Kind { get; }
        public string ShapeType { get; }
        public double Area { get; }
        public double Inertia1 { get; }
        public double Inertia2 { get; }
        public double Diameter { get; }
        public double Thickness { get; }
        public Section.FormedTypes Formation { get; }
        public SteelMaterial.SteelTypes MaterialType { get; }
        public double ElasticModulus { get; }
        public double YieldStrength { get; }
        public double UltimateStrength { get; }
        public int NominalGrade { get; }
        public string MaterialSource { get; }
        public double? NetArea { get; }
        public string NetAreaSource { get; }
        public double? EffectiveLength1 { get; }
        public double? EffectiveLength2 { get; }
        public string BucklingLengthSource { get; }
        public bool AxialOnlyConfirmed { get; }
        public bool IsBridge { get; }

        public AxialSteelMember(ISteelSection section, int nominalGrade, string materialSource,
            bool axialOnlyConfirmed, double? effectiveLength1 = null, double? effectiveLength2 = null,
            string bucklingLengthSource = "", double? netArea = null, string netAreaSource = "", bool isBridge = false)
        {
            if (section == null || section.SteelMaterial == null) throw new ArgumentNullException(nameof(section));
            ShapeType = section.SectionShape.GetType().Name;
            if (section.SectionShape is SectionCHS hollow)
            { Kind = AxialSteelSectionKind.CircularHollow; Diameter = hollow.Diameter; Thickness = hollow.Thickness; }
            else if (section.SectionShape is SectionCircular solid)
            { Kind = AxialSteelSectionKind.CircularBar; Diameter = solid.Diameter; }
            else Kind = AxialSteelSectionKind.Other;
            Area = section.Area; Inertia1 = section.J11; Inertia2 = section.J22;
            Formation = section.FormedType; MaterialType = section.SteelMaterial.SteelType;
            ElasticModulus = section.SteelMaterial.E; YieldStrength = section.SteelMaterial.Fyk;
            UltimateStrength = section.SteelMaterial.Fu; NominalGrade = nominalGrade;
            MaterialSource = materialSource ?? ""; NetArea = netArea; NetAreaSource = netAreaSource ?? "";
            EffectiveLength1 = effectiveLength1; EffectiveLength2 = effectiveLength2;
            BucklingLengthSource = bucklingLengthSource ?? ""; AxialOnlyConfirmed = axialOnlyConfirmed; IsBridge = isBridge;
            if (!Positive(Area) || !Positive(Inertia1) || !Positive(Inertia2) || !Positive(ElasticModulus)
                || !Positive(YieldStrength) || !Positive(UltimateStrength) || UltimateStrength < YieldStrength
                || ElasticModulus <= YieldStrength || nominalGrade <= 0
                || (netArea.HasValue && (!Positive(netArea.Value) || netArea.Value > Area))
                || (effectiveLength1.HasValue && !Positive(effectiveLength1.Value))
                || (effectiveLength2.HasValue && !Positive(effectiveLength2.Value))
                || (Kind != AxialSteelSectionKind.Other && !Positive(Diameter))
                || (Kind == AxialSteelSectionKind.CircularHollow && (!Positive(Thickness) || 2 * Thickness >= Diameter)))
                throw new ArgumentException("Finite positive section/material values, valid net area and positive effective lengths required.");
        }
        private static bool Positive(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value > 0;
    }
}
