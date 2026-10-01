using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using GPC.Checkers.Concrete.Shear;
using GPC.Model.Materials;
using GPC.Model.PostProcessing;

namespace GPC.Model.Checker
{
    /// <summary>
    /// Shear in the two section directions through <see cref="SectionShearCalculator"/>. Geometry and reinforcement come from the
    /// explicit <see cref="Sections.Concrete.ConcreteShearData"/> of the section; nothing is derived from the outline here.
    /// Not implemented: prestress, composite steel–concrete sections, seismic/accidental combinations, torsion interaction.
    /// </summary>
    public sealed partial class ConcreteSectionVerifier
    {
        private readonly double? _shearCotTheta;

        private bool SupportsShear(SectionCheckSpecification check)
            => check.Category == CombinationCategory.Ultimate && (ShearProfiles.TryResolve(_standard, out _) || ShearProfiles.NotApplicableReason(_standard) != null);

        partial void AddShearConfiguration(SortedDictionary<string, string> entries)
        {
            entries["ShearCotTheta"] = _shearCotTheta.HasValue ? _shearCotTheta.Value.ToString("R", CultureInfo.InvariantCulture) : "method";
            entries["ShearProfile"] = ShearProfiles.TryResolve(_standard, out var profile) ? profile.ToString() : "unsupported";
        }

        private static CheckResult Unavailable(DataStatus data, string code, string message)
            => new CheckResult { Data = data, Outcome = EngineeringOutcome.NotEvaluated, Diagnostics = new List<ModelDiagnostic> { ModelDiagnostic.Error(code, message: message) } };

        private CheckResult Shear(BeamCheckInput input, SectionCheckSpecification check, CancellationToken cancellationToken)
        {
            var notApplicable = ShearProfiles.NotApplicableReason(_standard);
            if (notApplicable != null) return NotApplicable(notApplicable);
            var section = input.Section; var data = section.ShearData; bool axis1 = check.Direction == SectionCheckDirection.Axis1;
            var direction = data == null ? null : axis1 ? data.Axis1 : data.Axis2;
            if (direction == null) return Unavailable(DataStatus.Insufficient, "MissingShearData", "Shear data of the section for " + check.Direction);
            if (section.SteelSections.Count > 0) return Unavailable(DataStatus.NotSupported, "CompositeShearNotImplemented", "Steel sections inside the concrete.");
            if (section.Rebars.Any(r => r.EpsilonP != 0 || r.RebarMaterial.SteelType == SteelMaterial.SteelTypes.Tendon))
                return Unavailable(DataStatus.NotSupported, "PrestressedShearNotImplemented", "Prestress components are not included in the shear methods.");
            if (!(section.ConcreteMaterial is ConcreteMaterialEuropeanCommon concrete)) return Unavailable(DataStatus.NotSupported, "UnsupportedConcreteMaterial", null);
            var profile = ShearProfiles.Resolve(_standard);
            var bars = section.Rebars.ToArray();
            if (bars.Length == 0) return Unavailable(DataStatus.Insufficient, "MissingReinforcement", null);

            bool stirrups = data.Reinforcement != null && direction.Legs > 0;
            bool usesAsl = !stirrups || profile == ShearProfile.ModelCode2010;
            double asl = usesAsl ? direction.TensionReinforcementArea : 0;
            if (usesAsl && !direction.TensionReinforcementAnchored)
                return Unavailable(DataStatus.Insufficient, "TensionReinforcementAnchorageNotConfirmed", "Asl enters the method: confirm its anchorage.");
            if (asl > section.AreaRebars) return Unavailable(DataStatus.Insufficient, "ShearDataExceedsReinforcement", "Asl exceeds the section reinforcement.");
            var stirrupMaterial = stirrups ? data.Reinforcement.Material : bars[0].RebarMaterial;
            if (stirrups && profile == ShearProfile.DsEN1992p11 && (stirrupMaterial.StrainUTension < .05 || stirrupMaterial.Fu < 1.08 * stirrupMaterial.Fyk))
                return Unavailable(DataStatus.NotSupported, "DuctilityClassBRequired", "DS: shear reinforcement of ductility class B or C required (εuk ≥ 5%, fu/fyk ≥ 1.08).");
            double lever = direction.LeverFactor;
            if (profile == ShearProfile.DinEN1992p11)
            {
                if (!direction.LongitudinalCover.HasValue) return Unavailable(DataStatus.Insufficient, "MissingLongitudinalCover", "DIN: cv is required for the lever arm limit.");
                double cv = direction.LongitudinalCover.Value, d = direction.EffectiveDepth;
                lever = Math.Min(lever, Math.Max(d - cv - 30, d - 2 * cv) / d);
                if (lever <= 0) return Unavailable(DataStatus.Insufficient, "InvalidLeverArm", "DIN: z ≤ 0 with the given d and cv.");
            }
            bool needsAggregate = profile == ShearProfile.ModelCode2010 || profile == ShearProfile.NsEN1992p11;
            if (needsAggregate && !data.AggregateSize.HasValue) return Unavailable(DataStatus.Insufficient, "MissingAggregateSize", "dg is required by " + profile + ".");

            bool fibres = concrete.ConcreteType == ConcreteMaterial.ConcreteTypes.FRC;
            var f = input.Forces;
            double v = axis1 ? f.V1 : f.V2;
            double m = profile == ShearProfile.ModelCode2010 ? (axis1 ? f.M2 : f.M1) : 0; // moment about the axis normal to V
            var shearInput = new SectionShearInput(_standard, f.N, v, m, section.Area, direction.WebWidth, direction.EffectiveDepth, asl,
                Math.Abs(concrete.Fck), Math.Abs(concrete.CalculateFcd(_standard)), Math.Abs(stirrupMaterial.CalculateFyd(_standard)), _standard.GammaC,
                bars[0].RebarMaterial.E, stirrups ? direction.Legs * data.Reinforcement.BarArea : 0, stirrups ? data.Reinforcement.Spacing : 1,
                stirrups ? data.Reinforcement.AngleDegrees : 90, _shearCotTheta, lever, needsAggregate ? data.AggregateSize.Value : 20,
                profile == ShearProfile.ModelCode2010 ? direction.AxialEccentricity : 0,
                // FRC: Fctu is the characteristic ultimate residual strength; the matrix fctk is the 5% fractile from fck.
                fibres ? Math.Abs(concrete.Fctu) : 0, Math.Abs(concrete.Fctk05));
            cancellationToken.ThrowIfCancellationRequested();
            SectionShearResult shear;
            try { shear = SectionShearCalculator.Calculate(shearInput); }
            catch (ArgumentException ex) { return Unavailable(DataStatus.Insufficient, "ShearOutsideMethodRange", ex.Message); }
            catch (NotSupportedException ex) { return Unavailable(DataStatus.NotSupported, "ShearMethodNotImplemented", ex.Message); }

            var trace = new List<CheckCalculationValue>
            {
                new CheckCalculationValue("VRsd", shear.VRsd, "N", "shear reinforcement"), new CheckCalculationValue("VRcd", shear.VRcd, "N", "concrete"),
                new CheckCalculationValue("Asw", shearInput.Asw, "mm2", stirrups ? direction.Legs + " legs" : "no shear reinforcement"),
                new CheckCalculationValue("bw", direction.WebWidth, "mm", data.Source), new CheckCalculationValue("d", direction.EffectiveDepth, "mm", data.Source),
                new CheckCalculationValue("z/d", lever, "1", profile == ShearProfile.DinEN1992p11 ? "min(z/d; max(d − cv − 30; d − 2cv)/d)" : "given")
            };
            trace.AddRange(shear.Details.Select(d => new CheckCalculationValue(d.Symbol, d.Value, d.Unit, d.Expression)));
            var result = new CheckResult { EngineVersion = Version, Standard = StandardContext };
            if (fibres && profile != ShearProfile.CnrDT204)
                result.Diagnostics.Add(new ModelDiagnostic { Code = "FibreContributionNotUsed", Severity = DiagnosticSeverity.Warning,
                    Message = profile + " does not include the residual strength of the fibres: plain-concrete resistance." });
            if (profile == ShearProfile.CnrDT200)
                result.Diagnostics.Add(new ModelDiagnostic { Code = "NoFrpStrengthening", Severity = DiagnosticSeverity.Warning,
                    Message = "The section has no FRP data: VRd,f = 0, resistance of the reinforced concrete member." });
            if (Math.Abs(f.T) > 1e-6 * Math.Max(1, Math.Abs(v) * direction.EffectiveDepth))
                result.Diagnostics.Add(new ModelDiagnostic { Code = "ConcomitantTorsionNotChecked", Severity = DiagnosticSeverity.Warning,
                    Message = "Shear only: the interaction with the concomitant torsion is not part of this task." });
            if (shear.Verdict == ShearVerdict.NotEvaluated)
            {
                result.Data = DataStatus.NotSupported; result.Outcome = EngineeringOutcome.NotEvaluated;
                result.Diagnostics.Add(ModelDiagnostic.Error("ShearMethodNotApplicable", message: shear.Status)); return WithEdition(result);
            }
            bool passed = shear.Verdict == ShearVerdict.Satisfied;
            var metric = new CheckMetric("V", shear.Demand, shear.VRd, "N", shear.Ratio, passed, shear.Reference);
            result.Execution = ExecutionStatus.Completed; result.Data = DataStatus.Ready; result.Utilization = shear.Ratio;
            result.Outcome = passed ? EngineeringOutcome.Satisfied : EngineeringOutcome.NotSatisfied;
            result.Details = new ShearCheckDetails(SectionShearCalculator.MethodId + "." + profile, axis1 ? 1 : 2, shear.CotTheta, new[] { metric }, trace);
            return WithEdition(result);
        }
    }
}
