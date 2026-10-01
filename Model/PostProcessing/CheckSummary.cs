using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.PostProcessing
{
    internal static class CheckResultRules
    {
        internal static bool ValidDecision(CheckResult r)
        {
            if (r == null || r.Diagnostics == null || r.SchemaVersion < 0 || r.SchemaVersion > 2 || !Enum.IsDefined(typeof(CheckApplicability), r.Applicability)
                || !Enum.IsDefined(typeof(EngineeringOutcome), r.Outcome) || !Enum.IsDefined(typeof(ExecutionStatus), r.Execution)
                || !Enum.IsDefined(typeof(DataStatus), r.Data)) return false;
            if (r.SchemaVersion == 2 && (r.Target == null || !Enum.IsDefined(typeof(CheckScope), r.Scope) || !Enum.IsDefined(typeof(CheckTargetKind), r.Target.Kind)
                || string.IsNullOrWhiteSpace(r.PlanItemId) || r.CoverageAssessment == null
                || (r.Scope == CheckScope.PhysicalMember) != (r.Target.Kind == CheckTargetKind.PhysicalMember)
                || (r.Scope == CheckScope.PhysicalMember && (r.Target.BeamId.HasValue || string.IsNullOrWhiteSpace(r.Target.MemberId)
                    || r.MemberInput?.Definition?.Id != r.Target.MemberId || string.IsNullOrWhiteSpace(r.MethodId)))
                || (r.Scope == CheckScope.SectionSample && (r.Target.BeamId != r.ElementId || r.Family != EntityFamily.Beam)))) return false;
            if (r.Applicability != CheckApplicability.Required)
                return !string.IsNullOrWhiteSpace(r.ApplicabilityReason) && r.Outcome == EngineeringOutcome.NotEvaluated
                    && r.Execution == ExecutionStatus.NotExecuted && !r.Utilization.HasValue;
            if (r.Outcome == EngineeringOutcome.NotEvaluated) return true;
            if (r.Execution != ExecutionStatus.Completed || r.Data != DataStatus.Ready || r.Details?.Convergence?.Converged == false) return false;
            if (r.Utilization.HasValue)
            {
                var u = r.Utilization.Value;
                if (double.IsNaN(u) || double.IsInfinity(u) || u < 0 || (u <= 1) != (r.Outcome == EngineeringOutcome.Satisfied)) return false;
            }
            var metrics = r.Details?.Metrics;
            var decisions = metrics?.Select(m => m.Passed ?? (m.Utilization.HasValue ? (bool?)(m.Utilization <= 1) : null)).ToArray();
            if (!r.Utilization.HasValue && (decisions == null || decisions.Length == 0 || decisions.Any(v => !v.HasValue))) return false;
            if (decisions != null && decisions.Any(v => v == false) && r.Outcome == EngineeringOutcome.Satisfied) return false;
            if (!r.Utilization.HasValue && decisions.All(v => v == true) && r.Outcome == EngineeringOutcome.NotSatisfied) return false;
            return true;
        }
        internal static bool Evaluated(CheckResult r) => r != null && r.Applicability == CheckApplicability.Required
            && r.Execution == ExecutionStatus.Completed && r.Data == DataStatus.Ready && r.Outcome != EngineeringOutcome.NotEvaluated
            && ValidDecision(r) && r.HasUnchangedEvidence;
    }

    /// <summary>Snapshot of coverage. Exclusions and missing rows never count as completed checks.</summary>
    public sealed class CheckSummary
    {
        public int Requested { get; }
        public int Recorded { get; }
        public int Applicable => Requested - NotApplicable;
        public int Completed { get; }
        public int NotApplicable { get; }
        public int Excluded { get; }
        public int Unsupported { get; }
        public int MissingData { get; }
        public int Stale { get; }
        public int Cancelled { get; }
        public int Errors { get; }
        public int Invalid { get; }
        public int MissingRows => Math.Max(0, Requested - Recorded);
        public int Outstanding => Math.Max(0, Requested - Completed - NotApplicable);
        public bool HasFailures { get; }
        public bool HasUnchangedScope { get; }
        public bool IsComplete => HasUnchangedScope && Requested > 0 && Recorded == Requested && Completed + NotApplicable == Requested;
        public EngineeringOutcome Outcome => !IsComplete || Completed == 0 ? EngineeringOutcome.NotEvaluated
            : HasFailures ? EngineeringOutcome.NotSatisfied : EngineeringOutcome.Satisfied;
        public CheckSummary(int requested, IEnumerable<CheckResult> results, bool hasUnchangedScope = true)
        {
            if (requested < 0) throw new ArgumentOutOfRangeException(nameof(requested));
            var rows = (results ?? throw new ArgumentNullException(nameof(results))).ToArray();
            Requested = requested; Recorded = rows.Length; HasUnchangedScope = hasUnchangedScope;
            // Compute each evidence digest once per summary, including for incomplete results.
            foreach (var row in rows)
            {
                bool valid = CheckResultRules.ValidDecision(row) && row.HasUnchangedEvidence;
                if (!valid) { Invalid++; continue; }
                if (row.Applicability == CheckApplicability.NotApplicable && row.Data == DataStatus.Ready) NotApplicable++;
                if (row.Applicability == CheckApplicability.Required && row.Outcome != EngineeringOutcome.NotEvaluated)
                { Completed++; if (row.Outcome == EngineeringOutcome.NotSatisfied) HasFailures = true; }
            }
            Excluded = rows.Count(r => r?.Applicability == CheckApplicability.Excluded);
            Unsupported = rows.Count(r => r?.Data == DataStatus.NotSupported);
            MissingData = rows.Count(r => r?.Data == DataStatus.Insufficient || r?.Data == DataStatus.MissingDependency);
            Stale = rows.Count(r => r?.Data == DataStatus.Stale);
            Cancelled = rows.Count(r => r?.Execution == ExecutionStatus.Cancelled);
            Errors = rows.Count(r => r?.Execution == ExecutionStatus.Error);
        }
    }

    /// <summary>Comparison identity includes actual code coefficients, method, metric definition and engine configuration.</summary>
    public sealed class GoverningCheck
    {
        public string ComparisonKey { get; }
        public CheckResult Result { get; }
        internal GoverningCheck(string key, CheckResult result) { ComparisonKey = key; Result = result; }
    }

    /// <summary>A view of stored outcomes; no rechecking or independent component envelopes.</summary>
    public sealed class CheckResultGroup
    {
        public string Key { get; }
        public IReadOnlyList<CheckResult> Results { get; }
        public CheckSummary Summary => new CheckSummary(Results.Count, Results);
        public IReadOnlyList<GoverningCheck> Governing => CheckReportViews.Governing(Results);
        internal CheckResultGroup(string key, IEnumerable<CheckResult> results) { Key = key; Results = Array.AsReadOnly(results.ToArray()); }
    }

    public sealed class ElementCheckReport
    {
        public EntityFamily Family { get; }
        public int ElementId { get; }
        public IReadOnlyList<CheckResult> Results { get; }
        public CheckSummary Summary => new CheckSummary(Results.Count, Results);
        public IReadOnlyList<GoverningCheck> Governing => CheckReportViews.Governing(Results);
        internal ElementCheckReport(EntityFamily family, int id, IEnumerable<CheckResult> results)
        { Family = family; ElementId = id; Results = Array.AsReadOnly(results.ToArray()); }
    }

    public static class CheckReportViews
    {
        // An undeclared standard stays segregated by engine; legacy metadata is never inferred.
        public static string StandardKey(CheckResult r) => r.Standard?.Identity ?? "Undeclared:" + Persistence.ModelArchive.Fingerprint(new object[] { r.EngineVersion, r.EngineConfiguration });
        public static IReadOnlyList<GoverningCheck> Governing(IEnumerable<CheckResult> results) => Array.AsReadOnly(results
            .Where(r => CheckResultRules.Evaluated(r) && r.Utilization.HasValue)
            .GroupBy(r => Persistence.ModelArchive.Fingerprint(new object[] { r.Mechanism, StandardKey(r), r.Details?.MethodId,
                r.Details?.UtilizationDefinition, r.EngineVersion, r.EngineConfiguration, r.Settings, r.Scope, r.MethodId }))
            .Select(g => new GoverningCheck(g.Key, g.OrderByDescending(r => r.Utilization).First())).ToArray());
        public static IReadOnlyList<ElementCheckReport> ByElement(IEnumerable<CheckResult> results) => Array.AsReadOnly(results
            .Where(r => r != null && r.Target?.Kind != CheckTargetKind.PhysicalMember).GroupBy(r => new { r.Family, r.ElementId })
            .Select(g => new ElementCheckReport(g.Key.Family, g.Key.ElementId, g)).ToArray());
        public static IReadOnlyList<CheckResultGroup> ByStandard(IEnumerable<CheckResult> results) => Group(results, StandardKey);
        public static IReadOnlyList<CheckResultGroup> ByMechanism(IEnumerable<CheckResult> results) => Group(results, r => r.Mechanism.ToString());
        public static IReadOnlyList<CheckResultGroup> ByJob(IEnumerable<CheckResult> results) => Group(results, r => r.Job);
        public static IReadOnlyList<CheckResultGroup> ByMember(IEnumerable<CheckResult> results) => Group(results.Where(r => r?.Target?.MemberId != null), r => r.Target.MemberId);
        public static IReadOnlyList<CheckResultGroup> ByGroup(IEnumerable<CheckResult> results) => Array.AsReadOnly(results.Where(r => r != null)
            .SelectMany(r => (r.GroupNames ?? new string[0]).Distinct(StringComparer.Ordinal).Select(g => new { Name = g, Result = r }))
            .GroupBy(p => p.Name, StringComparer.Ordinal).Select(g => new CheckResultGroup(g.Key, g.Select(p => p.Result))).ToArray());
        private static IReadOnlyList<CheckResultGroup> Group(IEnumerable<CheckResult> results, Func<CheckResult, string> key)
            => Array.AsReadOnly(results.Where(r => r != null).GroupBy(key, StringComparer.Ordinal).Select(g => new CheckResultGroup(g.Key, g)).ToArray());
    }
}
