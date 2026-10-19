using System;
using System.Collections.Generic;
using GPC.Model.Standards;

namespace GPC.Model.Geotechnics.Seepage
{
    public enum HydraulicActionCategory { PermanentStructural, PermanentNonStructural, Variable }
    public enum HydraulicGradientMeasure { Mean, Exit }

    /// <summary>Characteristic resultant in N; positive upward. Weights are total (not submerged) when water resultants are included.
    /// Correlated pressures on opposite faces must be combined into one signed action before factoring, to avoid double factoring.</summary>
    public sealed class HydraulicForceAction
    {
        public string Id { get; set; } = "";
        public HydraulicActionCategory Category { get; set; }
        public double UpwardForce { get; set; }
        public string Source { get; set; } = "";
    }
    public sealed class HydraulicUpliftInput
    {
        public string CaseId { get; set; } = "";
        public string StageId { get; set; } = "";
        public string ElementId { get; set; } = "";
        public GeotechnicalSituation Situation { get; set; }
        public IReadOnlyList<HydraulicForceAction> CharacteristicActions { get; set; } = Array.Empty<HydraulicForceAction>();
        /// <summary>Additional resistance Rd in N, already at design level. No partial factor is applied again.
        /// Its method, M2 treatment and any anchorage/pile resistance factors must be documented by the caller.</summary>
        public double AdditionalDesignResistance { get; set; }
        public string ResistanceSource { get; set; } = "";
    }
    /// <summary>Free-outflow HYD check for saturated soil and predominantly vertical flow.
    /// Positive gradient is upward, gammaSat in N/mm³. No factor has already been applied to the gradient.</summary>
    public sealed class HydraulicGradientInput
    {
        public string CaseId { get; set; } = "";
        public string StageId { get; set; } = "";
        public string ElementId { get; set; } = "";
        public GeotechnicalSituation Situation { get; set; }
        public HydraulicGradientMeasure Measure { get; set; }
        public double UpwardGradient { get; set; }
        public double SaturatedUnitWeight { get; set; }
        public bool PredominantlyVerticalFlow { get; set; }
        public bool FreeOutflow { get; set; }
        public string ApplicabilitySource { get; set; } = "";
    }
    /// <summary>Loaded-outflow HYD check. Pressures/stresses in MPa, at characteristic level.
    /// EffectiveStressWithoutSeepage includes the applied boundary load and is evaluated without seepage.</summary>
    public sealed class HydraulicLoadedExitInput
    {
        public string CaseId { get; set; } = "";
        public string StageId { get; set; } = "";
        public string ElementId { get; set; } = "";
        public GeotechnicalSituation Situation { get; set; }
        public double PorePressure { get; set; }
        public double HydrostaticPressure { get; set; }
        public double EffectiveStressWithoutSeepage { get; set; }
        public bool PredominantlyVerticalFlow { get; set; }
        public bool LoadedOutflow { get; set; }
        public string ApplicabilitySource { get; set; } = "";
    }
}
