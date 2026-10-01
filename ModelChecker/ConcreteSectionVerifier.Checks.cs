using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Checkers.Concrete.Serviceability;
using GPC.Model.PostProcessing;

namespace GPC.Model.Checker
{
    /// <summary>Tasks with explicit discriminators. Only the listed combinations are implemented; the others remain NotSupported.</summary>
    public sealed partial class ConcreteSectionVerifier
    {
        public bool Supports(SectionCheckSpecification check)
        {
            if (check == null) return false;
            switch (check.Mechanism)
            {
                // The plastic domain uses the fundamental partial factors: seismic and accidental combinations are not implied.
                case CheckMechanism.UlsBiaxialSection: return check.Category == CombinationCategory.Ultimate;
                // Frequent combination: no stress limit in this implementation.
                case CheckMechanism.Serviceability: return check.Criterion == SectionCheckCriterion.StressLimits
                    && (check.Category == CombinationCategory.Characteristic || check.Category == CombinationCategory.QuasiPermanent);
                case CheckMechanism.Shear: return SupportsShear(check);
                default: return false;
            }
        }

        public CheckResult Verify(BeamCheckInput input, SectionCheckSpecification check, CancellationToken cancellationToken)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (!Supports(check)) return new CheckResult { Data = DataStatus.NotSupported, Outcome = EngineeringOutcome.NotEvaluated,
                Diagnostics = new List<ModelDiagnostic> { ModelDiagnostic.Error("UnsupportedSectionCheck", message: check?.Key) } };
            cancellationToken.ThrowIfCancellationRequested();
            switch (check.Mechanism)
            {
                case CheckMechanism.UlsBiaxialSection: return Verify(input, CheckMechanism.UlsBiaxialSection, cancellationToken);
                case CheckMechanism.Serviceability: return StressLimits(input, check.Category, cancellationToken);
                default: return Shear(input, check, cancellationToken);
            }
        }

        private CheckResult StressLimits(BeamCheckInput input, CombinationCategory category, CancellationToken cancellationToken)
        {
            var combination = category == CombinationCategory.Characteristic ? ServiceabilityCombination.Characteristic : ServiceabilityCombination.QuasiPermanent;
            var forces = SectionForces(input, out var reference);
            lock (Sync)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var entry = Checker(input.Section, reference, _serviceabilityAnalysis);
                var analysis = entry.Item2.GetTensionAnalysisResult(forces);
                cancellationToken.ThrowIfCancellationRequested();
                if (analysis?.StrainPlane == null) return Failure("CheckerStressAnalysisNotConverged");
                StressLimitResult limits;
                try { limits = StressLimitCheck.Evaluate(analysis, combination, _concreteStressLimitFactor); }
                catch (NotSupportedException ex) { return new CheckResult { Data = DataStatus.NotSupported, Outcome = EngineeringOutcome.NotEvaluated,
                    Diagnostics = new List<ModelDiagnostic> { ModelDiagnostic.Error("UnsupportedStressLimits", message: ex.Message) } }; }
                catch (InvalidOperationException ex) { var failed = Failure("CheckerStressAnalysisNotConverged"); failed.Diagnostics[0].Message = ex.Message; return failed; }
                if (!limits.Ratio.HasValue) return Failure("CheckerStressLimitMissing");

                var metrics = new List<CheckMetric>
                {
                    new CheckMetric("ConcreteCompression", Math.Abs(limits.ConcreteGoverning?.Stress ?? 0), limits.ConcreteLimit, "MPa", limits.ConcreteRatio, limits.ConcreteRatio <= 1,
                        combination == ServiceabilityCombination.Characteristic ? "k1·fck" : "k2·fck")
                };
                if (limits.SteelGoverning != null)
                    metrics.Add(new CheckMetric("SteelTension", Math.Abs(limits.SteelGoverning.Stress), limits.SteelGoverning.Limit, "MPa", limits.SteelRatio, limits.SteelRatio <= 1,
                        limits.SteelGoverning.Id.StartsWith("P", StringComparison.Ordinal) ? "prestress limit" : "k3·fyk"));
                var trace = new[]
                {
                    new CheckCalculationValue("StressAnalysis", null, "", limits.LinearAnalysis ? "Linear" : "NonLinear"),
                    new CheckCalculationValue("PsiRebar", limits.PsiRebar, "1", "creep coefficient of the bars (linear analysis)"),
                    new CheckCalculationValue("PsiTendon", limits.PsiTendon, "1", "creep coefficient of the tendons (linear analysis)"),
                    new CheckCalculationValue("ConcreteLimitFactor", limits.ConcreteLimitFactor, "1", "explicit factor on the concrete limit"),
                    new CheckCalculationValue("SigmaCMin", limits.ConcreteMinStress, "MPa", "minimum concrete stress at the vertices"),
                    new CheckCalculationValue("SigmaSMax", limits.SteelMaxStress, "MPa", "maximum absolute bar/tendon stress")
                };
                var points = new[] { limits.ConcreteGoverning, limits.SteelGoverning }.Where(p => p != null && p.Id != null)
                    .Select(p => new CheckPointValue(p.Id, p.X, p.Y, 0, p.Stress, "MPa", "SectionPlane")).ToArray();
                double ratio = limits.Ratio.Value;
                var result = new CheckResult { Execution = ExecutionStatus.Completed, Data = DataStatus.Ready,
                    Outcome = ratio <= 1 ? EngineeringOutcome.Satisfied : EngineeringOutcome.NotSatisfied, Utilization = ratio, EngineVersion = Version,
                    Standard = StandardContext,
                    Details = new ServiceabilityCheckDetails(StressLimitCheck.MethodId, "stress/limit", category.ToString(), metrics, trace, points) };
                return WithEdition(result);
            }
        }
    }
}
