using GPC.Model.Standards;

namespace GPC.Model.Geotechnics.EmbeddedWalls
{
    /// <summary>Local basal-heave mechanism of a long excavation in homogeneous undrained clay.
    /// Lengths mm, pressures MPa, total unit weight N/mm³. All strengths and loads are characteristic.
    /// This is not a pore-pressure uplift, piping or global-stability input.</summary>
    public sealed class UndrainedBasalHeaveInput
    {
        public string CaseId { get; set; } = "";
        public string StageId { get; set; } = "";
        public double ExcavationDepth { get; set; }
        public double ExcavationWidth { get; set; }
        /// <summary>Depth from excavation bottom to the stiff stratum; null means no limiting stratum.</summary>
        public double? StiffLayerDepth { get; set; }
        public double TotalUnitWeight { get; set; }
        public double UndrainedStrength { get; set; }
        /// <summary>Assigned Nc for this mechanism/geometry, with its documented source. No implicit graph interpolation.</summary>
        public double BearingCapacityFactor { get; set; }
        public string BearingCapacitySource { get; set; } = "";
        /// <summary>Uniform permanent non-structural surcharge on the retained ground, MPa.</summary>
        public double PermanentSurcharge { get; set; }
        /// <summary>Uniform variable surcharge on the retained ground, MPa, before psi and partial factors.</summary>
        public double VariableSurcharge { get; set; }
        public double VariableCombinationFactor { get; set; } = 1;
        public string ActionSource { get; set; } = "";
        /// <summary>Use cu*H/B' relief on the external vertical plane. False uses the conservative no-relief mechanism.</summary>
        public bool IncludeExternalBlockShear { get; set; }
        public bool HomogeneousUndrainedMechanismConfirmed { get; set; }
        public bool LongExcavationConfirmed { get; set; }
        public bool CharacteristicValuesConfirmed { get; set; }
        public string ApplicabilitySource { get; set; } = "";
        public GeotechnicalSituation Situation { get; set; } = GeotechnicalSituation.PersistentTransient;
    }
}
