using GPC.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Model.Analysis;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Execution;
using GPC.Model.Checking.Preparation;
using GPC.Model.Core.Identity;
using GPC.Model.Results.Locations;
using GPC.Model.Structure.Members;

namespace GPC.Model.Checking.Reports
{
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class CheckReport
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Required>k__BackingField", IsRequired = true)]
        public int Required { get; internal set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Job>k__BackingField", IsRequired = true)]
        public string Job { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Scope>k__BackingField", IsRequired = true)]
        public PreparationRequest Scope { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ScopeFingerprint>k__BackingField", IsRequired = true)]
        public string ScopeFingerprint { get; private set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<BeamScope>k__BackingField", IsRequired = false)]
        public BeamCheckPlanRequest BeamScope { get; private set; }

        [System.Runtime.Serialization.OptionalField]
        [System.Runtime.Serialization.DataMember(IsRequired = false)]
        private string _resultSlotsFingerprint;
        public bool HasUnchangedScope => _resultSlotsFingerprint == null || _resultSlotsFingerprint == ResultSlots();

        internal void SealScope()
        {
            _resultSlotsFingerprint = ResultSlots();
        }

        /// <summary>The same report without its rows (scope, counts and seal unchanged): the header of a report archive package,
        /// whose rows are stored apart and added back to <see cref="Results"/> on load.</summary>
        internal CheckReport WithoutResults() => new CheckReport { Required = Required, Job = Job, Scope = Scope, ScopeFingerprint = ScopeFingerprint,
            BeamScope = BeamScope, _resultSlotsFingerprint = _resultSlotsFingerprint };

        private static object[] ResultSlot(CheckResult r)
        {
            if (r == null)
                return null;
            var fields = new object[]
            {
                r.Job,
                r.Family,
                r.ElementId,
                r.Mechanism,
                r.Dataset,
                r.Case,
                r.Station,
                r.Side,
                r.Phase,
                r.Step,
                r.ConcomitantStateId,
                r.Mode,
                r.MovingLoadPosition,
                r.ShellPoint,
                r.ShellPointKind,
                r.ShellCoordinateKind,
                r.Face,
                r.Layer
            };
            return r.SchemaVersion < 2 ? fields : fields.Concat(new object[] { r.Target, r.Scope, r.PlanItemId, r.MemberLocation, r.MethodId }).ToArray();
        }

        private string ResultSlots() => Core.ModelValues.Fingerprint(new object[] { Required, Job, Results.Select(ResultSlot).ToArray() });
        public static CheckReport ForPlan(string job, BeamCheckPlan plan, IEnumerable<CheckResult> results)
        {
            if (plan == null || results == null)
                throw new ArgumentNullException();
            var report = new CheckReport
            {
                Job = job,
                BeamScope = plan.Request.Copy(),
                ScopeFingerprint = plan.ScopeFingerprint,
                Required = plan.WorkItems.Count
            };
            report.Results.AddRange(results);
            if (!report.Results.Select(r => r?.PlanItemId).SequenceEqual(plan.WorkItems.Select(w => w.Id)))
                throw new ArgumentException("Every plan item needs its own ordered outcome.");
            report.SealScope();
            return report;
        }

        public static CheckReport ForPreparedResults(string job, ModelPreparation preparation, IEnumerable<CheckResult> results)
        {
            if (preparation == null || results == null)
                throw new ArgumentNullException();
            var report = new CheckReport
            {
                Job = job,
                Scope = preparation.Request.Copy(),
                ScopeFingerprint = preparation.ScopeFingerprint,
                Required = preparation.Samples.Count * preparation.Request.Mechanisms.Length
            };
            report.Results.AddRange(results);
            if (report.Results.Count != report.Required)
                throw new ArgumentException("Every required sample/mechanism needs an explicit outcome.");
            report.SealScope();
            return report;
        }

        /// <summary>One outcome for each explicit shell task, including absent and unsupported inputs.</summary>
        public static CheckReport ForShellPlan(string job, ModelPreparation preparation, IEnumerable<string> itemIds, IEnumerable<CheckResult> results)
        {
            if (preparation == null || itemIds == null || results == null)
                throw new ArgumentNullException();
            var ids = itemIds.ToArray();
            if (ids.Any(string.IsNullOrWhiteSpace) || ids.Distinct(StringComparer.Ordinal).Count() != ids.Length)
                throw new ArgumentException("UniqueShellTaskIdsRequired");
            var report = new CheckReport
            {
                Job = job,
                Scope = preparation.Request.Copy(),
                ScopeFingerprint = preparation.ScopeFingerprint,
                Required = ids.Length
            };
            report.Results.AddRange(results);
            if (!ids.SequenceEqual(report.Results.Select(r => r?.PlanItemId)) || report.Results.Any(r => r.Family != EntityFamily.Shell || r.SchemaVersion < 3 || r.SchemaVersion > 4))
                throw new ArgumentException("Every shell task needs one ordered shell outcome.");
            report.SealScope();
            return report;
        }

        public static CheckReport ForSingleResult(CheckResult result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            var report = new CheckReport
            {
                Required = 1,
                Job = result.Job
            };
            report.Results.Add(result);
            report.SealScope();
            return report;
        }

        [field: System.Runtime.Serialization.DataMember(Name = "<Results>k__BackingField", IsRequired = true)]
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
            if (groups.Count > 1)
                throw new InvalidOperationException("Incompatible verification metrics: use GoverningByConfiguration.");
            return groups.FirstOrDefault()?.Result;
        }

        /// <summary>Historical outcomes remain stored; this gate must be used when displaying a report as current.</summary>
        public EngineeringOutcome CurrentOutcome(Models.Model model, IConcreteSectionVerifier verifier) => CurrentOutcome(model, verifier, null);
        public EngineeringOutcome CurrentOutcome(Models.Model model, IConcreteSectionVerifier verifier, IPhysicalMemberVerifier memberVerifier) => CurrentOutcome(model, verifier, memberVerifier, null);
        /// <summary>Pass the current job request to also compare changed design-context/coverage choices outside Model.</summary>
        public EngineeringOutcome CurrentOutcome(Models.Model model, IConcreteSectionVerifier verifier, IPhysicalMemberVerifier memberVerifier, BeamCheckPlanRequest currentRequest)
        {
            if (model == null || Results.Count != Required || !HasUnchangedScope)
                return EngineeringOutcome.NotEvaluated;
            // Every row is compared with the same state of the model, read in one session: the compatibility, analysis, design, dataset,
            // scenario, verification and sample digests are computed once per call and the samples of each beam are indexed once, instead
            // of once per row (rows x elements). The engine identities are read outside the session. The comparisons are those of the
            // row-by-row check, in the same order; a new call reads the model again.
            var reads = new ValidationReadScope.Session(model);
            using (reads.Enter())
            {
                if (AnalysisCompatibilityValidator.Validate(model).Status != AnalysisCompatibility.Compatible)
                    return EngineeringOutcome.NotEvaluated;
                if (BeamScope != null)
                {
                    try
                    {
                        if (ScopeFingerprint != BeamCheckPlan.FingerprintScope(model, BeamScope) || currentRequest != null && ScopeFingerprint != BeamCheckPlan.FingerprintScope(model, currentRequest))
                            return EngineeringOutcome.NotEvaluated;
                    }
                    catch (Exception ex)when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException || ex is KeyNotFoundException)
                    {
                        return EngineeringOutcome.NotEvaluated;
                    }
                }

                if (Scope != null)
                {
                    try
                    {
                        if (ScopeFingerprint != ModelPreparation.FingerprintScope(model, Scope))
                            return EngineeringOutcome.NotEvaluated;
                    }
                    catch (Exception ex)when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
                    {
                        return EngineeringOutcome.NotEvaluated;
                    }
                }
            }

            foreach (var result in Results)
            {
                bool current;
                using (reads.Enter())
                    current = result?.Provenance == null || result.Provenance.IsCurrent(model);
                if (!current)
                    return EngineeringOutcome.NotEvaluated;
                if (result?.Scope == CheckScope.PhysicalMember)
                {
                    if (BeamScope == null || !result.HasUnchangedEvidence || memberVerifier == null || !memberVerifier.Supports(result.MethodId, result.Mechanism) || result.EngineVersion != memberVerifier.Version || result.EngineConfiguration != memberVerifier.Configuration || result.Standard?.Identity != memberVerifier.Standard?.Identity)
                        return EngineeringOutcome.NotEvaluated;
                    continue;
                }

                if (result == null || verifier == null || !result.HasUnchangedEvidence || result.Family != EntityFamily.Beam)
                    return EngineeringOutcome.NotEvaluated;
                string revision;
                using (reads.Enter())
                    revision = model.VerificationFingerprint(result.Settings);
                if (result.VerificationRevision != revision || result.EngineVersion != verifier.Version || result.EngineConfiguration != (verifier as IConfiguredSectionVerifier)?.Configuration || !model.BeamElements.TryGetValue(result.ElementId, out var beam))
                    return EngineeringOutcome.NotEvaluated;
                using (reads.Enter())
                {
                    var samples = StationSamples.Matching(model, beam, result);
                    if (samples.Count != 1 || ValidationReadScope.FingerprintOf(model, samples[0]) != result.SampleRevision)
                        return EngineeringOutcome.NotEvaluated;
                }
            }

            return Outcome;
        }
    }

    /// <summary>Compatibility facade over the sequential beam execution service.</summary>
    public static class CheckRunner
    {
        public static CheckReport Beam(Models.Model model, int beamId, string dataset, string caseName, string settings, CheckMechanism[] mechanisms, IConcreteSectionVerifier verifier, CancellationToken cancellationToken = default(CancellationToken), IProgress<int> progress = null) => global::GPC.Model.Checking.Execution.BeamCheckExecution.Beam(model, beamId, dataset, caseName, settings, mechanisms, verifier, cancellationToken, progress);
    }
}
