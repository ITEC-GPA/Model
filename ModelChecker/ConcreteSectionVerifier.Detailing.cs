using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Checkers.Concrete.Cracking;
using GPC.Checkers.Concrete.Detailing;
using GPC.Model.Materials;
using GPC.Model.PostProcessing;
using GPC.Model.Sections.Concrete;

namespace GPC.Model.Checker
{
    /// <summary>
    /// 1D detailing of beams and columns as a physical-member task (one per member and state, never per FEM sample) through
    /// <see cref="MemberDetailingCalculator"/>. Every distinct section of the member is checked with its <see cref="ConcreteDetailingData"/> and the
    /// links of its <see cref="ConcreteShearData"/>; the column NEd is the largest compression among the member samples of the state.
    /// Anchorage and laps need the zone data of bars and ends: available as a Checker API (AnchorageCalculator), not planned here.
    /// </summary>
    public sealed partial class ConcreteSectionVerifier : IPhysicalMemberVerifier
    {
        public const string BeamDetailingMethod = "Concrete.Detailing1D.Beam";
        public const string ColumnDetailingMethod = "Concrete.Detailing1D.Column";

        CheckStandardContext IPhysicalMemberVerifier.Standard => StandardContext;

        public bool Supports(string methodId, CheckMechanism mechanism)
            => mechanism == CheckMechanism.Detailing && (methodId == BeamDetailingMethod || methodId == ColumnDetailingMethod)
                && (DetailingProfiles.TryResolve(_standard, out _) || DetailingProfiles.NotApplicableReason(_standard) != null || DetailingProfiles.NotSupportedReason(_standard) != null);

        public CheckResult Verify(PhysicalMemberCheckInput input, CancellationToken cancellationToken)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!Supports(input.MethodId, input.Mechanism)) return Unavailable(DataStatus.NotSupported, "UnsupportedMemberCheck", input.MethodId);
            var notApplicable = DetailingProfiles.NotApplicableReason(_standard);
            if (notApplicable != null) return NotApplicable(notApplicable);
            var notSupported = DetailingProfiles.NotSupportedReason(_standard);
            if (notSupported != null) return Unavailable(DataStatus.NotSupported, "DetailingMethodNotImplemented", notSupported);
            var profile = DetailingProfiles.Resolve(_standard);
            var kind = input.MethodId == ColumnDetailingMethod ? MemberDetailingKind.Column : MemberDetailingKind.Beam;
            var sections = input.Sections.Select(s => s.Section).Where(s => s != null).Distinct().ToArray();
            if (sections.Length == 0) return Unavailable(DataStatus.Insufficient, "MissingMemberSections", null);
            double compression = input.Sections.Select(s => Math.Max(0, -s.Forces.N)).DefaultIfEmpty(0).Max();
            var metrics = new List<CheckMetric>(); var trace = new List<CheckCalculationValue>
            {
                new CheckCalculationValue("Kind", null, "", kind.ToString()), new CheckCalculationValue("NEd", compression, "N", "largest compression among the member samples of the state")
            };
            var pending = new List<string>(); bool failed = false, dataPending = false;
            for (int k = 0; k < sections.Length; k++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var section = sections[k]; string suffix = sections.Length > 1 ? "@" + (string.IsNullOrWhiteSpace(section.Name) ? k.ToString() : section.Name) : "";
                if (section.SteelSections.Count > 0) return Unavailable(DataStatus.NotSupported, "CompositeDetailingNotImplemented", "Steel sections inside the concrete.");
                if (section.Rebars.Any(r => r.EpsilonP != 0 || r.RebarMaterial.SteelType == SteelMaterial.SteelTypes.Tendon))
                    return Unavailable(DataStatus.NotSupported, "PrestressedDetailingNotImplemented", "Detailing of prestressed sections is not implemented.");
                if (!(section.ConcreteMaterial is ConcreteMaterialEuropeanCommon concrete)) return Unavailable(DataStatus.NotSupported, "UnsupportedConcreteMaterial", null);
                if (section.RebarsCount == 0) return Unavailable(DataStatus.Insufficient, "MissingReinforcement", section.Name);
                var data = section.DetailingData;
                if (data == null) return Unavailable(DataStatus.Insufficient, "MissingDetailingData", "Covers, widths, aggregate and confirmations of section " + section.Name + ".");
                var shear = section.ShearData;
                if (shear == null) return Unavailable(DataStatus.Insufficient, "MissingShearData", "Links of section " + section.Name + ": shear data required (null links = member without links).");
                var links = shear.Reinforcement; int legs = (shear.Axis2 ?? shear.Axis1)?.Legs ?? 0;
                var bar = section.Rebars.First().RebarMaterial;
                MemberDetailingResult r;
                try
                {
                    r = MemberDetailingCalculator.Calculate(profile, new MemberDetailingInput(kind, CrackSectionGeometry.From(section), section.Area, Math.Abs(concrete.Fck),
                        Math.Abs(concrete.Fctm), Math.Abs(bar.Fyk), Math.Abs(bar.CalculateFyd(_standard)), data.TopWidth, data.BottomWidth, data.WebWidth, compression, links != null,
                        links?.Diameter ?? 0, links?.Spacing ?? 0, legs, data.Aggregate, data.NominalCover, data.MinimumDurabilityCover, data.CoverDeviation, data.LapZone,
                        data.CompressionBarsRestrained, data.EndZonesConfirmed));
                }
                catch (ArgumentException ex) { return Unavailable(DataStatus.Insufficient, "DetailingOutsideMethodRange", ex.Message); }
                catch (NotSupportedException ex) { return Unavailable(DataStatus.NotSupported, "DetailingMethodNotImplemented", ex.Message); }
                trace.Add(new CheckCalculationValue("Section" + suffix, null, "", (section.Name ?? "") + ", " + data.Source));
                foreach (var check in r.Checks)
                {
                    if (check.Passed.HasValue)
                    {
                        metrics.Add(new CheckMetric(check.Key + suffix, check.Actual, check.Limit, check.Unit, null, check.Passed, check.Reference));
                        failed |= check.Passed == false;
                    }
                    else
                    {
                        pending.Add(check.Key + suffix + ": " + check.Explanation); dataPending |= !check.NotImplemented;
                        trace.Add(new CheckCalculationValue("Pending " + check.Key + suffix, null, "", check.Reference + " · " + check.Explanation));
                    }
                }
            }
            var result = new CheckResult { EngineVersion = Version, Standard = StandardContext,
                Details = new DetailingCheckDetails(MemberDetailingCalculator.MethodId + "." + profile + "." + kind, metrics, trace) };
            if (failed)
            {
                result.Execution = ExecutionStatus.Completed; result.Data = DataStatus.Ready; result.Outcome = EngineeringOutcome.NotSatisfied;
                foreach (var item in pending) result.Diagnostics.Add(new ModelDiagnostic { Code = "DetailingPending", Severity = DiagnosticSeverity.Warning, Message = item });
            }
            else if (pending.Count > 0)
            {
                result.Data = dataPending ? DataStatus.Insufficient : DataStatus.NotSupported; result.Outcome = EngineeringOutcome.NotEvaluated;
                result.Diagnostics.Add(ModelDiagnostic.Error(dataPending ? "DetailingPending" : "DetailingRuleNotImplemented", message: string.Join("; ", pending)));
            }
            else { result.Execution = ExecutionStatus.Completed; result.Data = DataStatus.Ready; result.Outcome = EngineeringOutcome.Satisfied; }
            return WithEdition(result);
        }
    }
}
