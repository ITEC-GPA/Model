using System;

namespace GPC.Model.Standards
{
    public enum GroundAnchorEvidence { CalculatedProfiles, MeasuredAnchorTests }

    /// <summary>Owned normative factors for ground-anchor pullout, distinct from pile shaft resistance.
    /// Resistance samples are unfactored N; the corresponding action is already an A1 design value.</summary>
    public sealed class GroundAnchorResistanceProfile
    {
        public string Country { get; } = "IT";
        public string Edition { get; } = "2018";
        public string NationalAnnex { get; } = "Norma nazionale italiana, nessun annesso EC applicato";
        public string Reference { get; } = "NTC 2018 §6.6.2, Tab.6.6.I–III; G.U.20/02/2018 pp.198–199";
        public string Combination { get; } = "A1+M1+R3";
        public bool Permanent { get; }
        public GroundAnchorEvidence Evidence { get; }
        public int EvidenceCount { get; }
        public double MeanCorrelationFactor { get; }
        public double MinimumCorrelationFactor { get; }
        public double ResistanceFactor { get; }
        internal GroundAnchorResistanceProfile(bool permanent, GroundAnchorEvidence evidence, int count,
            double meanFactor, double minimumFactor)
        {
            if (count < 1 || !Enum.IsDefined(typeof(GroundAnchorEvidence), evidence)) throw new ArgumentOutOfRangeException(nameof(count));
            Permanent = permanent; Evidence = evidence; EvidenceCount = count;
            MeanCorrelationFactor = meanFactor; MinimumCorrelationFactor = minimumFactor;
            ResistanceFactor = permanent ? 1.2 : 1.1;
        }
    }
}
