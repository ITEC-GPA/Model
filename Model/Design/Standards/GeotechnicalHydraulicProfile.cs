using System;

namespace GPC.Model.Standards
{
    /// <summary>Immutable hydraulic safety criteria; separate from retaining-wall resistance factors.
    /// Critical gradients and demands are calculated by Geotechnics. Null profile means unsupported.</summary>
    public sealed class GeotechnicalHydraulicProfile
    {
        public string Country { get; }
        public string Edition { get; }
        public string NationalAnnex { get; }
        public string Reference { get; }
        public GeotechnicalActionFactors UpliftActions { get; }
        public GeotechnicalMaterialFactors UpliftResistanceMaterials { get; }
        public double MeanGradientFactor { get; }
        public double ExitGradientFactor { get; }
        public double LoadedExitFactor { get; }
        public GeotechnicalHydraulicProfile(string country, string edition, string nationalAnnex, string reference,
            GeotechnicalActionFactors upliftActions, GeotechnicalMaterialFactors upliftResistanceMaterials,
            double meanGradientFactor, double exitGradientFactor, double loadedExitFactor)
        {
            if (string.IsNullOrWhiteSpace(country) || string.IsNullOrWhiteSpace(edition) || string.IsNullOrWhiteSpace(nationalAnnex)
                || string.IsNullOrWhiteSpace(reference)) throw new ArgumentException("Normative identity and reference are required.");
            foreach (double f in new[] { meanGradientFactor, exitGradientFactor, loadedExitFactor })
                if (double.IsNaN(f) || double.IsInfinity(f) || f < 1) throw new ArgumentException("Hydraulic safety factors must be finite and at least one.");
            Country = country; Edition = edition; NationalAnnex = nationalAnnex; Reference = reference;
            UpliftActions = upliftActions ?? throw new ArgumentNullException(nameof(upliftActions));
            UpliftResistanceMaterials = upliftResistanceMaterials ?? throw new ArgumentNullException(nameof(upliftResistanceMaterials));
            MeanGradientFactor = meanGradientFactor; ExitGradientFactor = exitGradientFactor; LoadedExitFactor = loadedExitFactor;
        }
    }
}
