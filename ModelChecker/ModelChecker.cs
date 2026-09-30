using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Model.PostProcessing;
using GPC.Model.Standards;

namespace GPC.Model.Checker
{
    public sealed class ConcreteVerificationOptions
    {
        public StandardModelCode2010 Standard { get; set; }
        public SectionSolver.FailureAnalysisTypes Criterion { get; set; }
        public bool ConsiderTensileConcrete { get; set; }
        public int AngularDivisions { get; set; } = 64;
        public double PsiRebar { get; set; }
        public double PsiTendon { get; set; }
        public ConcreteSectionVerifier CreateVerifier() => new ConcreteSectionVerifier(Standard, Criterion, ConsiderTensileConcrete, AngularDivisions, PsiRebar, PsiTendon);
    }
    public sealed class ModelCheckJob
    {
        public string Name { get; set; }
        public PreparationRequest Preparation { get; set; }
        public ConcreteVerificationOptions Options { get; set; }
    }
    public sealed class ModelCheckRequest
    {
        public List<ModelCheckJob> Jobs { get; } = new List<ModelCheckJob>();
    }
    public sealed class VerificationProgress
    {
        public int Completed { get; internal set; }
        public int Total { get; internal set; }
        public string Job { get; internal set; }
        public EntityFamily Family { get; internal set; }
        public int ElementId { get; internal set; }
    }
    public sealed class ModelCheckReport
    {
        public IReadOnlyList<CheckReport> Jobs { get; internal set; }
        public int CreatedCheckers { get; internal set; }
        public int Required => Jobs.Sum(j => j.Required);
        public int Executed => Jobs.Sum(j => j.Executed);
        public EngineeringOutcome Outcome => Required == 0 || Executed != Required ? EngineeringOutcome.NotEvaluated
            : Jobs.Any(j => j.Outcome == EngineeringOutcome.NotSatisfied) ? EngineeringOutcome.NotSatisfied : EngineeringOutcome.Satisfied;

        public EngineeringOutcome CurrentOutcome(Models.Model model, Func<string, IConcreteSectionVerifier> verifierForJob)
        {
            if (verifierForJob == null || Jobs.Any(j => j.CurrentOutcome(model, verifierForJob(j.Job)) == EngineeringOutcome.NotEvaluated)) return EngineeringOutcome.NotEvaluated;
            return Outcome;
        }
    }

    /// <summary>Headless orchestration over the shared Model. Real section Checkers are created lazily and reused within this run.</summary>
    public sealed class ModelChecker
    {
        private readonly Func<ModelCheckJob, IConcreteSectionVerifier> _factory;
        public ModelChecker() : this(job => job.Options?.CreateVerifier()) { }
        public ModelChecker(Func<ModelCheckJob, IConcreteSectionVerifier> factory) { _factory = factory ?? throw new ArgumentNullException(nameof(factory)); }

        public ModelCheckReport Verify(Models.Model model, ModelCheckRequest request, CancellationToken cancellationToken = default,
            IProgress<VerificationProgress> progress = null)
        {
            if (model == null || request == null) throw new ArgumentNullException();
            var jobs = request.Jobs.ToArray();
            if (jobs.Length == 0 || jobs.Any(j => j == null || string.IsNullOrWhiteSpace(j.Name) || j.Preparation == null)
                || jobs.Select(j => j.Name).Distinct(StringComparer.Ordinal).Count() != jobs.Length) throw new ArgumentException("Unique named check jobs are required.");
            // Validate all selections before invoking an engine. No partial reports are attached to Model by this service.
            var plans = jobs.Select(j => ModelPreparation.Prepare(model, j.Preparation, cancellationToken)).ToArray();
            int total = plans.Sum(p => p.Samples.Count * p.Request.Mechanisms.Length), completed = 0;
            var reports = new List<CheckReport>(); var engines = new Dictionary<string, IConcreteSectionVerifier>(StringComparer.Ordinal);
            for (int i = 0; i < jobs.Length; i++)
            {
                var job = jobs[i]; var plan = plans[i]; var results = new List<CheckResult>();
                IConcreteSectionVerifier verifier = null; Exception factoryError = null;
                if (!cancellationToken.IsCancellationRequested && plan.Samples.Any(s => s.Beam?.Status == DataStatus.Ready))
                {
                    try
                    {
                        verifier = _factory(job);
                        if (verifier != null)
                        {
                            var key = verifier.GetType().AssemblyQualifiedName + "\n" + verifier.Version + "\n" + (verifier as IConfiguredSectionVerifier)?.Configuration;
                            // Reuse only the known adapter; unknown third-party factories may have additional private configuration.
                            if (verifier is ConcreteSectionVerifier && engines.TryGetValue(key, out var existing)) verifier = existing;
                            else engines[job.Name] = verifier;
                            if (verifier is ConcreteSectionVerifier) engines[key] = verifier;
                        }
                    }
                    catch (Exception ex) { factoryError = ex; }
                }
                foreach (var row in plan.Samples)
                {
                    foreach (var mechanism in plan.Request.Mechanisms)
                    {
                        CheckResult result;
                        if (row.Cancelled || cancellationToken.IsCancellationRequested)
                            result = new CheckResult { Execution = ExecutionStatus.Cancelled, Data = DataStatus.Insufficient };
                        else if (row.Beam != null)
                        {
                            result = Verification.Run(row.Beam, mechanism, verifier, cancellationToken);
                            if (factoryError != null && row.Beam.Status == DataStatus.Ready)
                            { result.Execution = ExecutionStatus.Error; result.Diagnostics.Add(ModelDiagnostic.Error("CheckerCreationFailed", message: factoryError.Message)); }
                        }
                        else
                        {
                            result = new CheckResult { Data = row.Shell.Status };
                            result.Diagnostics.AddRange(row.Shell.Diagnostics);
                            if (row.Shell.Input != null)
                            {
                                result.Data = row.Shell.Input.IsCurrent ? DataStatus.NotSupported : DataStatus.Stale;
                                result.Diagnostics.Add(ModelDiagnostic.Error(result.Data == DataStatus.Stale ? "StaleShellPreparation" : "ShellCheckerUnavailable", row.Shell.Input.Element,
                                    "The installed Checker has no executable plate verifier. A validated shell design method and concrete adapter are required."));
                                result.VerificationRevision = row.Shell.Input.VerificationRevision; result.SampleRevision = row.Shell.Input.SampleRevision;
                            }
                        }
                        result.Job = job.Name; result.Family = row.Element.Family; result.ElementId = row.Element.Id; result.Mechanism = mechanism;
                        result.Settings = plan.Request.Settings;
                        result.Source = (row.Element.Family == EntityFamily.Beam ? (Elements.Element)model.BeamElements[row.Element.Id] : model.AreaElements[row.Element.Id]).Source;
                        result.Dataset = row.Sample?.State?.DatasetId ?? row.Selection.Dataset; result.Case = row.Sample?.Case?.Name ?? row.Selection.Case;
                        result.Phase = row.Sample?.State?.Phase ?? row.Selection.Phase; result.Step = row.Sample?.State?.Step ?? row.Selection.Step;
                        result.ConcomitantStateId = row.Sample?.State?.ConcomitantStateId ?? row.Selection.ConcomitantState;
                        result.Mode = row.Sample?.State?.Mode ?? row.Selection.Mode; result.MovingLoadPosition = row.Sample?.State?.MovingLoadPosition ?? row.Selection.MovingLoadPosition;
                        result.Coverage = row.Sample?.State?.Coverage;
                        if (row.Sample is Results.ResultLocations.StationResultBeamForces beam) { result.Station = beam.ParametricDistance; result.Side = beam.Side; }
                        if (row.Sample is Results.ResultLocations.PointResultPlateForces shell)
                        { result.ShellPoint = shell.Location; result.ShellPointKind = shell.PointKind; result.ShellCoordinateKind = shell.CoordinateKind; }
                        foreach (var diagnostic in result.Diagnostics) { diagnostic.Family = result.Family; diagnostic.ElementId = result.ElementId;
                            diagnostic.Dataset = result.Dataset; diagnostic.Case = result.Case; diagnostic.Station = result.Station; }
                        results.Add(result); completed++;
                        progress?.Report(new VerificationProgress { Completed = completed, Total = total, Job = job.Name, Family = row.Element.Family, ElementId = row.Element.Id });
                    }
                }
                reports.Add(CheckReport.ForPreparedResults(job.Name, plan, results));
            }
            return new ModelCheckReport { Jobs = reports.AsReadOnly(), CreatedCheckers = engines.Values.Distinct().OfType<ConcreteSectionVerifier>().Sum(e => e.CreatedCheckers) };
        }
    }
}
