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
        public string StandardEdition { get; set; }
        public string NationalAnnex { get; set; }
        /// <summary>Stress analysis of the serviceability stress limits (SectionChecks with Serviceability/StressLimits).</summary>
        public SectionSolver.StressAnalysisTypes ServiceabilityAnalysis { get; set; } = SectionSolver.StressAnalysisTypes.NonLinear;
        /// <summary>Explicit factor on the concrete stress limits, 1 = none (for example 0.8 for thin castings when the standard requires it).</summary>
        public double ConcreteStressLimitFactor { get; set; } = 1;
        /// <summary>
        /// Assigned cot θ of the shear checks (SectionChecks Shear/Axis1-2); null = chosen by the method within its range.
        /// It is also the strut inclination shared by torsion and shear in the Torsion task, which requires it.
        /// </summary>
        public double? ShearCotTheta { get; set; }
        /// <summary>Duration of the load in the crack checks (kt): long term unless stated.</summary>
        public CrackLoadDuration CrackLoadDuration { get; set; } = CrackLoadDuration.LongTerm;
        /// <summary>Design wlim, mm, of the crack checks where the standard admits it (Eurocode family, Model Code 2010); null = limit of the standard.</summary>
        public double? CrackDesignLimit { get; set; }
        public ConcreteSectionVerifier CreateVerifier() => new ConcreteSectionVerifier(Standard, Criterion, ConsiderTensileConcrete, AngularDivisions, PsiRebar, PsiTendon,
            StandardEdition, NationalAnnex, ServiceabilityAnalysis, ConcreteStressLimitFactor, ShearCotTheta, CrackLoadDuration, CrackDesignLimit);
    }

    /// <summary>Duration of the load for the tension stiffening of the crack width (kt 0.4 long term, 0.6 short term).</summary>
    public enum CrackLoadDuration { LongTerm, ShortTerm }
    public sealed class ModelCheckJob
    {
        public string Name { get; set; }
        public PreparationRequest Preparation { get; set; }
        public BeamCheckPlanRequest BeamPlan { get; set; }
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
        public CheckTargetReference Target { get; internal set; }
    }
    public sealed class ModelCheckReport
    {
        public IReadOnlyList<CheckReport> Jobs { get; internal set; } = new CheckReport[0];
        public int CreatedCheckers { get; internal set; }
        public int Required => Jobs.Sum(j => j.Required);
        public int Executed => Jobs.Sum(j => j.Executed);
        public CheckSummary Summary => new CheckSummary(Required, Jobs.SelectMany(j => j.Results), Jobs.All(j => j.HasUnchangedScope && j.Results.Count == j.Required));
        public IReadOnlyList<ElementCheckReport> Elements => CheckReportViews.ByElement(Jobs.SelectMany(j => j.Results));
        public IReadOnlyList<CheckResultGroup> Members => CheckReportViews.ByMember(Jobs.SelectMany(j => j.Results));
        public IReadOnlyList<CheckResultGroup> Groups => CheckReportViews.ByGroup(Jobs.SelectMany(j => j.Results));
        public IReadOnlyList<CheckResultGroup> Standards => CheckReportViews.ByStandard(Jobs.SelectMany(j => j.Results));
        public IReadOnlyList<CheckResultGroup> Mechanisms => CheckReportViews.ByMechanism(Jobs.SelectMany(j => j.Results));
        public EngineeringOutcome Outcome => Jobs.Any(j => !j.Summary.IsComplete) ? EngineeringOutcome.NotEvaluated : Summary.Outcome;

        public EngineeringOutcome CurrentOutcome(Models.Model model, Func<string, IConcreteSectionVerifier> verifierForJob)
            => CurrentOutcome(model, verifierForJob, null);
        public EngineeringOutcome CurrentOutcome(Models.Model model, Func<string, IConcreteSectionVerifier> verifierForJob, Func<string, IPhysicalMemberVerifier> memberVerifierForJob)
            => CurrentOutcome(model, verifierForJob, memberVerifierForJob, null);
        public EngineeringOutcome CurrentOutcome(Models.Model model, Func<string, IConcreteSectionVerifier> verifierForJob, Func<string, IPhysicalMemberVerifier> memberVerifierForJob,
            Func<string, BeamCheckPlanRequest> currentPlanForJob)
        {
            if (Jobs.Any(j => j.CurrentOutcome(model, verifierForJob?.Invoke(j.Job), memberVerifierForJob?.Invoke(j.Job), currentPlanForJob?.Invoke(j.Job)) == EngineeringOutcome.NotEvaluated)) return EngineeringOutcome.NotEvaluated;
            return Outcome;
        }
    }

    /// <summary>Headless orchestration over the shared Model. Real section Checkers are created lazily and reused within this run.</summary>
    public sealed partial class ModelChecker
    {
        private readonly Func<ModelCheckJob, IConcreteSectionVerifier> _factory;
        private readonly Func<ModelCheckJob, IPhysicalMemberVerifier> _memberFactory;
        /// <summary>Concrete section verifier from the job options, also for the member detailing tasks (other member methods: not supported).</summary>
        public ModelChecker() : this(job => job.Options?.CreateVerifier(), job => job.Options?.CreateVerifier()) { }
        public ModelChecker(Func<ModelCheckJob, IConcreteSectionVerifier> factory) { _factory = factory ?? throw new ArgumentNullException(nameof(factory)); }
        public ModelChecker(Func<ModelCheckJob, IConcreteSectionVerifier> sectionFactory, Func<ModelCheckJob, IPhysicalMemberVerifier> memberFactory) : this(sectionFactory)
        { _memberFactory = memberFactory ?? throw new ArgumentNullException(nameof(memberFactory)); }

        private IConcreteSectionVerifier ResolveSectionEngine(ModelCheckJob job, Dictionary<string, IConcreteSectionVerifier> engines)
        {
            var verifier = _factory(job);
            if (verifier == null) return null;
            var key = verifier.GetType().AssemblyQualifiedName + "\n" + verifier.Version + "\n" + (verifier as IConfiguredSectionVerifier)?.Configuration;
            if (verifier is ConcreteSectionVerifier && engines.TryGetValue(key, out var existing)) return existing;
            engines[job.Name] = verifier;
            if (verifier is ConcreteSectionVerifier) engines[key] = verifier;
            return verifier;
        }

        public ModelCheckReport Verify(Models.Model model, ModelCheckRequest request, CancellationToken cancellationToken = default,
            IProgress<VerificationProgress> progress = null)
        {
            if (model == null || request == null) throw new ArgumentNullException();
            var jobs = request.Jobs.ToArray();
            if (jobs.Length == 0 || jobs.Any(j => j == null || string.IsNullOrWhiteSpace(j.Name) || (j.Preparation == null) == (j.BeamPlan == null))
                || jobs.Select(j => j.Name).Distinct(StringComparer.Ordinal).Count() != jobs.Length) throw new ArgumentException("Unique named check jobs are required.");
            // Validate all selections before invoking an engine. No partial reports are attached to Model by this service.
            var plans = jobs.Select(j => j.Preparation == null ? null : ModelPreparation.Prepare(model, j.Preparation, cancellationToken)).ToArray();
            var beamPlans = jobs.Select(j => j.BeamPlan == null ? null : BeamCheckPlan.Prepare(model, j.BeamPlan, cancellationToken)).ToArray();
            int total = plans.Sum(p => p == null ? 0 : p.Samples.Count * p.Request.Mechanisms.Length) + beamPlans.Sum(p => p?.WorkItems.Count ?? 0), completed = 0;
            var reports = new List<CheckReport>(); var engines = new Dictionary<string, IConcreteSectionVerifier>(StringComparer.Ordinal);
            for (int i = 0; i < jobs.Length; i++)
            {
                var job = jobs[i]; var plan = plans[i]; var results = new List<CheckResult>();
                if (beamPlans[i] != null)
                {
                    reports.Add(PlannedBeamChecking.Run(model, job, beamPlans[i], j => ResolveSectionEngine(j, engines), _memberFactory, cancellationToken, item => {
                        completed++; progress?.Report(new VerificationProgress { Completed = completed, Total = total, Job = job.Name,
                            Family = EntityFamily.Beam, ElementId = item.Target.BeamId ?? default(int), Target = item.Target }); }));
                    continue;
                }
                IConcreteSectionVerifier verifier = null; Exception factoryError = null;
                if (!cancellationToken.IsCancellationRequested && plan.Samples.Any(s => s.Beam?.Status == DataStatus.Ready))
                {
                    try
                    {
                        verifier = ResolveSectionEngine(job, engines);
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
                        var element = row.Element.Family == EntityFamily.Beam ? (Elements.Element)model.BeamElements[row.Element.Id] : model.AreaElements[row.Element.Id];
                        result.Source = element.Source;
                        result.GroupNames = element.Groups.Keys.OrderBy(n => n, StringComparer.Ordinal).ToArray();
                        if (result.Standard == null && verifier is ConcreteSectionVerifier concrete) result.Standard = concrete.StandardContext;
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
                        result.SealEvidence();
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
