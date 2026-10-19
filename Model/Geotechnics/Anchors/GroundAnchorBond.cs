using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Geotechnics.Anchors
{
    public enum GroundAnchorInjection { IGU, IRS }
    public enum GroundAnchorSoilKind
    {
        Gravel, SandyGravel, GravellySand, CoarseSand, MediumSand, FineSand, SiltySand,
        Silt, Clay, Marl, MarlyLimestone, WeatheredLimestone, WeatheredRock
    }

    /// <summary>Distances along the anchor axis in mm; pressures MPa. The pressuremeter limit pressure is a soil
    /// property, distinct from the execution injection pressure. No lithostatic-pressure substitution is implied.</summary>
    public sealed class GroundAnchorBondLayer
    {
        public double Start { get; }
        public double End { get; }
        public GroundAnchorSoilKind Soil { get; }
        public double LimitPressure { get; }
        public double InjectionPressure { get; }
        public double DiameterFactor { get; }
        public string PressureSource { get; }
        public GroundAnchorBondLayer(double start, double end, GroundAnchorSoilKind soil, double limitPressure,
            double injectionPressure, double diameterFactor, string pressureSource)
        {
            if (!Finite(start) || start < 0 || !Finite(end) || end <= start || !Enum.IsDefined(typeof(GroundAnchorSoilKind), soil)
                || !Positive(limitPressure) || !Positive(injectionPressure) || !Positive(diameterFactor))
                throw new ArgumentException("Ordered positive axis lengths, soil family and separate positive pressures required.");
            Start = start; End = end; Soil = soil; LimitPressure = limitPressure; InjectionPressure = injectionPressure;
            DiameterFactor = diameterFactor; PressureSource = pressureSource ?? "";
        }
        internal static bool Finite(double x) => !double.IsNaN(x) && !double.IsInfinity(x);
        internal static bool Positive(double x) => Finite(x) && x > 0;
    }

    /// <summary>One isolated anchor and one complete investigation profile. Lengths mm, including the free/bonded
    /// axis lengths. MinimumCover is the minimum distance to a free ground/excavation surface, not free-axis length.
    /// ExecutionSource must document injection system, grout quantities and quality. ApplicabilitySource records
    /// the stable zone, separation from adjacent bulbs, confinement and validity of the empirical method.</summary>
    public sealed class GroundAnchorBond
    {
        public string Id { get; }
        public double FreeLength { get; }
        public double BondLength { get; }
        public double DrillDiameter { get; }
        public double MinimumCover { get; }
        public GroundAnchorInjection Injection { get; }
        public IReadOnlyList<GroundAnchorBondLayer> Layers { get; }
        public string ExecutionSource { get; }
        public string ApplicabilitySource { get; }
        public GroundAnchorBond(string id, double freeLength, double bondLength, double drillDiameter,
            double minimumCover, GroundAnchorInjection injection, IEnumerable<GroundAnchorBondLayer> layers,
            string executionSource, string applicabilitySource)
        {
            if (string.IsNullOrWhiteSpace(id) || !GroundAnchorBondLayer.Finite(freeLength) || freeLength < 0
                || !GroundAnchorBondLayer.Positive(bondLength) || !GroundAnchorBondLayer.Positive(drillDiameter)
                || !GroundAnchorBondLayer.Finite(minimumCover) || minimumCover < 0 || !Enum.IsDefined(typeof(GroundAnchorInjection), injection)
                || !GroundAnchorBondLayer.Finite(freeLength + bondLength)) throw new ArgumentException("Complete finite anchor geometry required.");
            var copy = (layers ?? throw new ArgumentNullException(nameof(layers))).ToArray();
            if (copy.Length == 0 || copy.Any(l => l == null)) throw new ArgumentException("A complete independent soil profile is required.");
            for (int i = 1; i < copy.Length; i++)
                if (Math.Abs(copy[i].Start - copy[i - 1].End) > 1e-8) throw new ArgumentException("Axis layers must be ordered and contiguous without gaps/overlaps.");
            if (copy[0].Start > freeLength || copy[copy.Length - 1].End < freeLength + bondLength)
                throw new ArgumentException("The investigation profile must cover the whole bonded axis.");
            Id = id; FreeLength = freeLength; BondLength = bondLength; DrillDiameter = drillDiameter; MinimumCover = minimumCover;
            Injection = injection; Layers = Array.AsReadOnly(copy); ExecutionSource = executionSource ?? ""; ApplicabilitySource = applicabilitySource ?? "";
        }
    }
}
