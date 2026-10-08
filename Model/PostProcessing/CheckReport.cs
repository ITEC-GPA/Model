using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GPC.Model.PostProcessing
{
    [Serializable]
    public sealed class CheckReport
    {
        public int Required { get; internal set; }
        public string Job { get; private set; }
        public PreparationRequest Scope { get; private set; }
        public string ScopeFingerprint { get; private set; }
        [field: System.Runtime.Serialization.OptionalField] public BeamCheckPlanRequest BeamScope { get; private set; }
        [System.Runtime.Serialization.OptionalField] private string _resultSlotsFingerprint;
        public bool HasUnchangedScope => _resultSlotsFingerprint == null || _resultSlotsFingerprint == ResultSlots();
        internal void SealScope() { _resultSlotsFingerprint = ResultSlots(); }
        private static object[] ResultSlot(CheckResult r)
        {
            if (r == null) return null;
            var fields = new object[] {
            r.Job, r.Family, r.ElementId, r.Mechanism, r.Dataset, r.Case, r.Station, r.Side, r.Phase, r.Step, r.ConcomitantStateId,
                r.Mode, r.MovingLoadPosition, r.ShellPoint, r.ShellPointKind, r.ShellCoordinateKind, r.Face, r.Layer };
            return r.SchemaVersion < 2 ? fields : fields.Concat(new object[] { r.Target, r.Scope, r.PlanItemId, r.MemberLocation, r.MethodId }).ToArray();
        }
        private string ResultSlots() => Persistence.ModelArchive.Fingerprint(new object[] { Required, Job, Results.Select(ResultSlot).ToArray() });
        public static CheckReport ForPlan(string job, BeamCheckPlan plan, IEnumerable<CheckResult> results)
        {
            if (plan == null || results == null) throw new ArgumentNullException();
            var report = new CheckReport { Job = job, BeamScope = plan.Request.Copy(), ScopeFingerprint = plan.ScopeFingerprint, Required = plan.WorkItems.Count };
            report.Results.AddRange(results);
            if (!report.Results.Select(r => r?.PlanItemId).SequenceEqual(plan.WorkItems.Select(w => w.Id))) throw new ArgumentException("Every plan item needs its own ordered outcome.");
            report.SealScope(); return report;
        }
        public static CheckReport ForPreparedResults(string job, ModelPreparation preparation, IEnumerable<CheckResult> results)
        {
            if (preparation == null || results == null) throw new ArgumentNullException();
            var report = new CheckReport { Job = job, Scope = preparation.Request.Copy(), ScopeFingerprint = preparation.ScopeFingerprint,
                Required = preparation.Samples.Count * preparation.Request.Mechanisms.Length };
            report.Results.AddRange(results);
            if (report.Results.Count != report.Required) throw new ArgumentException("Every required sample/mechanism needs an explicit outcome.");
            report.SealScope();
            return report;
        }
        public static CheckReport ForSingleResult(CheckResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            var report = new CheckReport { Required = 1, Job = result.Job }; report.Results.Add(result); report.SealScope(); return report;
        }
        public List<CheckResult> Results { get; private set; } = new List<CheckResult>();
        public CheckSummary Summary => new CheckSummary(Required, Results, HasUnchangedScope);
        public IReadOnlyList<ElementCheckReport> Elements => CheckReportViews.ByElement(Results);
        public IReadOnlyList<CheckResultGroup> Members => CheckReportViews.ByMember(Results);
        public int Executed => Results.Count(CheckResultRules.Evaluated);
        /// <summary>Legacy coverage deficit. For explicit exclusions use Summary.Excluded.</summary>
        public int Excluded => Required - Executed;
        public EngineeringOutcome Outcome => Summary.Outcome;
        public IReadOnlyList<GoverningCheck> GoverningByConfiguration => CheckReportViews.Governing(Results);
        public CheckResult Governing(CheckMechanism mechanism)
        {
            var groups = CheckReportViews.Governing(Results.Where(r => r?.Mechanism == mechanism));
            if (groups.Count > 1) throw new InvalidOperationException("Incompatible verification metrics: use GoverningByConfiguration.");
            return groups.FirstOrDefault()?.Result;
        }

        /// <summary>Historical outcomes remain stored; this gate must be used when displaying a report as current.</summary>
        public EngineeringOutcome CurrentOutcome(Models.Model model, IConcreteSectionVerifier verifier)
            => CurrentOutcome(model, verifier, null);
        public EngineeringOutcome CurrentOutcome(Models.Model model, IConcreteSectionVerifier verifier, IPhysicalMemberVerifier memberVerifier)
            => CurrentOutcome(model, verifier, memberVerifier, null);
        /// <summary>Pass the current job request to also compare changed design-context/coverage choices outside Model.</summary>
        public EngineeringOutcome CurrentOutcome(Models.Model model, IConcreteSectionVerifier verifier, IPhysicalMemberVerifier memberVerifier, BeamCheckPlanRequest currentRequest)
        {
            if (model == null || Results.Count != Required || !HasUnchangedScope) return EngineeringOutcome.NotEvaluated;
            if (AnalysisCompatibilityValidator.Validate(model).Status != AnalysisCompatibility.Compatible) return EngineeringOutcome.NotEvaluated;
            if (BeamScope != null)
            {
                try { if (ScopeFingerprint != BeamCheckPlan.FingerprintScope(model, BeamScope)
                        || currentRequest != null && ScopeFingerprint != BeamCheckPlan.FingerprintScope(model, currentRequest)) return EngineeringOutcome.NotEvaluated; }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException || ex is KeyNotFoundException) { return EngineeringOutcome.NotEvaluated; }
            }
            if (Scope != null)
            {
                try { if (ScopeFingerprint != ModelPreparation.FingerprintScope(model, Scope)) return EngineeringOutcome.NotEvaluated; }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException) { return EngineeringOutcome.NotEvaluated; }
            }
            foreach (var result in Results)
            {
                if (result?.Provenance != null && !result.Provenance.IsCurrent(model)) return EngineeringOutcome.NotEvaluated;
                if (result?.Scope == CheckScope.PhysicalMember)
                {
                    if (BeamScope == null || !result.HasUnchangedEvidence || memberVerifier == null || !memberVerifier.Supports(result.MethodId, result.Mechanism)
                        || result.EngineVersion != memberVerifier.Version || result.EngineConfiguration != memberVerifier.Configuration
                        || result.Standard?.Identity != memberVerifier.Standard?.Identity) return EngineeringOutcome.NotEvaluated;
                    continue;
                }
                if (result == null || verifier == null || !result.HasUnchangedEvidence || result.Family != EntityFamily.Beam || result.VerificationRevision != model.VerificationFingerprint(result.Settings) || result.EngineVersion != verifier.Version
                    || result.EngineConfiguration != (verifier as IConfiguredSectionVerifier)?.Configuration
                    || !model.BeamElements.TryGetValue(result.ElementId, out var beam)) return EngineeringOutcome.NotEvaluated;
                var samples = beam.Results.SelectMany(r => r.Results).OfType<Results.ResultLocations.StationResultBeamForces>()
                    .Where(r => r.State?.DatasetId == result.Dataset && r.Case?.Name == result.Case && r.ParametricDistance == result.Station && r.Side == result.Side
                        && r.State.Phase == result.Phase && r.State.Step == result.Step && r.State.ConcomitantStateId == result.ConcomitantStateId
                        && r.State.Mode == result.Mode && r.State.MovingLoadPosition == result.MovingLoadPosition).ToArray();
                if (samples.Length != 1 || Persistence.ModelArchive.Fingerprint(new object[] { samples[0] }) != result.SampleRevision) return EngineeringOutcome.NotEvaluated;
            }
            return Outcome;
        }
    }

    public static class CheckRunner
    {
        /// <summary>Sequential reference path. Every supplied station/state is checked before enveloping outcomes.</summary>
        public static CheckReport Beam(Models.Model model, int beamId, string dataset, string caseName, string settings,
            CheckMechanism[] mechanisms, IConcreteSectionVerifier verifier, CancellationToken cancellationToken = default(CancellationToken), IProgress<int> progress = null)
        {
            var samples = Verification.BeamSamples(model.BeamElements[beamId], dataset, caseName);
            var report = new CheckReport { Required = Math.Max(1, samples.Count) * mechanisms.Length };
            if (samples.Count == 0)
            {
                foreach (var mechanism in mechanisms) report.Results.Add(new CheckResult
                {
                    Mechanism = mechanism,
                    ElementId = beamId,
                    Case = caseName,
                    Dataset = dataset,
                    Data = DataStatus.Insufficient,
                    Diagnostics = new List<ModelDiagnostic> { ModelDiagnostic.Error("MissingBeamSamples", model.BeamElements[beamId]) }
                });
                report.SealScope(); return report;
            }
            foreach (var sample in samples)
            {
                var preparation = cancellationToken.IsCancellationRequested ? new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.Insufficient, Diagnostics = new ModelDiagnostic[0] }
                    : Verification.PrepareBeam(model, beamId, sample, settings);
                foreach (var mechanism in mechanisms) report.Results.Add(Verification.Run(preparation, mechanism, verifier, cancellationToken));
                progress?.Report(report.Results.Count);
            }
            report.SealScope(); return report;
        }
    }
}
