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
        public static CheckReport ForPreparedResults(string job, ModelPreparation preparation, IEnumerable<CheckResult> results)
        {
            if (preparation == null || results == null) throw new ArgumentNullException();
            var report = new CheckReport { Job = job, Scope = preparation.Request.Copy(), ScopeFingerprint = preparation.ScopeFingerprint,
                Required = preparation.Samples.Count * preparation.Request.Mechanisms.Length };
            report.Results.AddRange(results);
            if (report.Results.Count != report.Required) throw new ArgumentException("Every required sample/mechanism needs an explicit outcome.");
            return report;
        }
        public static CheckReport ForSingleResult(CheckResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            var report = new CheckReport { Required = 1 }; report.Results.Add(result); return report;
        }
        public List<CheckResult> Results { get; private set; } = new List<CheckResult>();
        public int Executed => Results.Count(r => r.Execution == ExecutionStatus.Completed && r.Data == DataStatus.Ready && r.Outcome != EngineeringOutcome.NotEvaluated);
        public int Excluded => Required - Executed;
        public EngineeringOutcome Outcome => Required == 0 || Executed != Required ? EngineeringOutcome.NotEvaluated :
            Results.Any(r => r.Outcome == EngineeringOutcome.NotSatisfied) ? EngineeringOutcome.NotSatisfied : EngineeringOutcome.Satisfied;
        public CheckResult Governing(CheckMechanism mechanism) => Results.Where(r => r.Mechanism == mechanism && r.Utilization.HasValue && r.Execution == ExecutionStatus.Completed
            && r.Data == DataStatus.Ready && r.Outcome != EngineeringOutcome.NotEvaluated)
            .OrderByDescending(r => r.Utilization).FirstOrDefault();

        /// <summary>Historical outcomes remain stored; this gate must be used when displaying a report as current.</summary>
        public EngineeringOutcome CurrentOutcome(Models.Model model, IConcreteSectionVerifier verifier)
        {
            if (verifier == null || Results.Count != Required) return EngineeringOutcome.NotEvaluated;
            if (Scope != null)
            {
                try { if (ScopeFingerprint != ModelPreparation.FingerprintScope(model, Scope)) return EngineeringOutcome.NotEvaluated; }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException) { return EngineeringOutcome.NotEvaluated; }
            }
            foreach (var result in Results)
            {
                if (result.Family != EntityFamily.Beam || result.VerificationRevision != model.VerificationFingerprint(result.Settings) || result.EngineVersion != verifier.Version
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
                return report;
            }
            foreach (var sample in samples)
            {
                var preparation = cancellationToken.IsCancellationRequested ? new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.Insufficient, Diagnostics = new ModelDiagnostic[0] }
                    : Verification.PrepareBeam(model, beamId, sample, settings);
                foreach (var mechanism in mechanisms) report.Results.Add(Verification.Run(preparation, mechanism, verifier, cancellationToken));
                progress?.Report(report.Results.Count);
            }
            return report;
        }
    }
}
