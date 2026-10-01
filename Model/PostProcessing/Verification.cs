using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Results;
using GPC.Model.Results.ResultLocations;
using GPC.Model.Sections.Concrete;

namespace GPC.Model.PostProcessing
{
    public enum CheckMechanism { UlsBiaxialSection, Shear, Torsion, Serviceability, Stability, Detailing }

    public sealed class BeamCheckInput
    {
        internal Models.Model Model { get; set; }
        internal string SampleFingerprint { get; set; }
        internal string PreparedForcesFingerprint { get; set; }
        internal string PreparedSectionFingerprint { get; set; }
        public int BeamId { get; internal set; }
        public StationResultBeamForces Sample { get; internal set; }
        public ReinforcedConcreteSection Section { get; internal set; }
        public ResultBeamForces Forces { get; internal set; }
        public string VerificationRevision { get; internal set; }
        public string Settings { get; internal set; }
        public bool IsCurrent => Verification.IsCurrent(this);
    }

    [Serializable]
    public sealed class CheckResult
    {
        public ExecutionStatus Execution { get; set; }
        public DataStatus Data { get; set; }
        public EngineeringOutcome Outcome { get; set; }
        public CheckMechanism Mechanism { get; set; }
        public double? Utilization { get; set; }
        public string EngineVersion { get; set; }
        public string EngineConfiguration { get; set; }
        public string Settings { get; set; }
        public string SampleRevision { get; set; }
        public string Phase { get; set; }
        public string Step { get; set; }
        public string Coverage { get; set; }
        public string ConcomitantStateId { get; set; }
        public string VerificationRevision { get; set; }
        public EntityFamily Family { get; set; } = EntityFamily.Beam;
        public SourceIdentity Source { get; set; }
        public string Job { get; set; }
        public string MovingLoadPosition { get; set; }
        public int? Mode { get; set; }
        public Point2d ShellPoint { get; set; }
        public ShellResultPointKind ShellPointKind { get; set; }
        public ResultCoordinateKind ShellCoordinateKind { get; set; }
        public int ElementId { get; set; }
        public string Case { get; set; }
        public string Dataset { get; set; }
        public double? Station { get; set; }
        public SectionSide Side { get; set; }
        public List<ModelDiagnostic> Diagnostics { get; set; } = new List<ModelDiagnostic>();
        // Optional fields retain compatibility with archives written before the report contracts.
        [field: System.Runtime.Serialization.OptionalField] public int SchemaVersion { get; set; } = 1;
        [field: System.Runtime.Serialization.OptionalField] public CheckApplicability Applicability { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public string ApplicabilityReason { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public CheckStandardContext Standard { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public CheckInputSnapshot Input { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public CheckDetails Details { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public string Face { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public string Layer { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public string[] GroupNames { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public string EvidenceFingerprint { get; private set; }
        [field: System.Runtime.Serialization.OptionalField] public CheckTargetReference Target { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public CheckScope Scope { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public string PlanItemId { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public string MethodId { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public MemberLocation MemberLocation { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public MemberInputSnapshot MemberInput { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public CoverageAssessment CoverageAssessment { get; set; }

        /// <summary>Detects subsequent edits to stored evidence; it is not a digital signature. Legacy results have no seal.</summary>
        public bool HasUnchangedEvidence => EvidenceFingerprint == null || EvidenceFingerprint == Evidence();
        public void SealEvidence() { EvidenceFingerprint = Evidence(); }
        private string Evidence()
        {
            var fields = new object[] { SchemaVersion, Execution, Data, Outcome, Mechanism,
            Utilization, EngineVersion, EngineConfiguration, Settings, SampleRevision, VerificationRevision, Family, ElementId,
            Source, Job, Dataset, Case, Station, Side, Phase, Step, Coverage, ConcomitantStateId, MovingLoadPosition, Mode,
                ShellPoint, ShellPointKind, ShellCoordinateKind, Face, Layer, GroupNames, Applicability, ApplicabilityReason, Standard, Input, Details, Diagnostics };
            return Persistence.ModelArchive.Fingerprint(SchemaVersion < 2 ? fields : fields.Concat(new object[] {
                Target, Scope, PlanItemId, MethodId, MemberLocation, MemberInput, CoverageAssessment }));
        }
    }

    public interface IConcreteSectionVerifier
    {
        string Version { get; }
        IReadOnlyCollection<CheckMechanism> Capabilities { get; }
        CheckResult Verify(BeamCheckInput input, CheckMechanism mechanism, CancellationToken cancellationToken);
    }

    public interface IConfiguredSectionVerifier : IConcreteSectionVerifier
    {
        /// <summary>Stable, complete configuration snapshot, including material safety factors and numerical options.</summary>
        string Configuration { get; }
    }

    public sealed class BeamPreparation
    {
        public int BeamId { get; internal set; }
        public StationResultBeamForces Sample { get; internal set; }
        public BeamCheckInput Input { get; internal set; }
        public DataStatus Status { get; internal set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; internal set; }
    }

    public static class Verification
    {
        public static IReadOnlyList<StationResultBeamForces> BeamSamples(BeamElement beam, string dataset, string caseName)
            => beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>()
                .Where(r => r.State?.DatasetId == dataset && r.Case?.Name == caseName)
                .OrderBy(r => r.ParametricDistance).ThenBy(r => r.Side).ToArray();

        /// <summary>Exact exported stations only. Missing points never trigger implicit interpolation.</summary>
        public static StationResultBeamForces BeamSample(BeamElement beam, string dataset, string caseName, double station, SectionSide side)
            => BeamSamples(beam, dataset, caseName).SingleOrDefault(r => r.ParametricDistance == NumericGuard.Station(station) && r.Side == side);

        public static BeamPreparation PrepareBeam(Models.Model model, int beamId, StationResultBeamForces sample, string settings)
        {
            var b = model.BeamElements[beamId]; var errors = model.ValidateTopology().Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            errors.AddRange(model.ValidateAssignments().Where(d => d.Severity == DiagnosticSeverity.Error));
            if (errors.Count > 0) return new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.Insufficient, Diagnostics = errors };
            var status = DataStatus.Insufficient;
            if (sample == null || !b.Results.SelectMany(r => r.Results).Any(r => ReferenceEquals(r, sample))) errors.Add(ModelDiagnostic.Error("MissingSample", b));
            else
            {
                var state = sample.State;
                if (state != null && !ResultAlgebra.HasCurrentDerivation(model, b, state)) { errors.Add(ModelDiagnostic.Error("StaleDerivedSources", b)); status = DataStatus.Stale; }
                if (state == null || string.IsNullOrEmpty(state.DatasetId) || !model.Datasets.TryGetValue(state.DatasetId, out var dataset)
                    || dataset.ModelRevision != state.ModelRevision || dataset.InputFingerprint != state.InputFingerprint || dataset.NormalizedUnits != "N,mm,rad")
                    errors.Add(ModelDiagnostic.Error("DatasetMismatchOrUnknownUnits", b));
                if (state == null || string.IsNullOrWhiteSpace(state.DatasetId) || string.IsNullOrWhiteSpace(state.ConcomitantStateId)
                    || sample.Case == null || state.Components == null || state.Components.Length != 6 || state.Components.Any(c => c != ComponentAvailability.Available))
                    errors.Add(ModelDiagnostic.Error("IncompleteState", b));
                if (state == null || (state.Semantics != AnalysisSemantics.LinearStatic && state.Semantics != AnalysisSemantics.NonlinearStatic && state.Semantics != AnalysisSemantics.ConcomitantEnvelopeState))
                    errors.Add(ModelDiagnostic.Error("NonConcomitantState", b));
                if (state != null && (state.IsCumulative == false || (!string.IsNullOrEmpty(state.Phase) && state.IsCumulative != true)))
                    errors.Add(ModelDiagnostic.Error("IncrementalOrUnknownPhaseState", b));
                if (state != null && state.InputFingerprint != model.AnalysisFingerprint()) { errors.Add(ModelDiagnostic.Error("StaleAnalysis", b)); status = DataStatus.Stale; }
                if (sample.Body != ActionBody.PositiveSectionFace) errors.Add(ModelDiagnostic.Error("UnresolvedSectionActionBody", b));
                try { new BeamReferenceGeometry(b).ValidateSample(sample); }
                catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) { errors.Add(ModelDiagnostic.Error("InvalidBeamReference", b, ex.Message)); }
                if (sample.ResultBeamForces == null || new[] { sample.ResultBeamForces.N, sample.ResultBeamForces.V1, sample.ResultBeamForces.V2, sample.ResultBeamForces.T, sample.ResultBeamForces.M1, sample.ResultBeamForces.M2 }.Any(v => double.IsNaN(v) || double.IsInfinity(v)))
                    errors.Add(ModelDiagnostic.Error("NonFiniteOrMissingForces", b));
            }
            ReinforcedConcreteSection section = null;
            try
            {
                if (sample != null) section = b.Assignments.SectionAt(new BeamReferenceGeometry(b).ConvertStation(sample.ParametricDistance, sample.StationDomain, b.Assignments.StationDomain ?? "NodeToNode"), sample.Side);
                if (section == null) errors.Add(ModelDiagnostic.Error("MissingReinforcementSection", b));
                else if (section.Rebars.Count() == 0) errors.Add(ModelDiagnostic.Error("MissingReinforcement", b));
                Axes.Validate(b.Assignments.SectionAxes);
            }
            catch (ArgumentException ex) { errors.Add(ModelDiagnostic.Error("InvalidSectionAxes", b, ex.Message)); }
            catch (InvalidOperationException ex) { errors.Add(ModelDiagnostic.Error("SectionAssignment", b, ex.Message)); }
            catch (NotSupportedException ex) { errors.Add(ModelDiagnostic.Error("SectionAssignment", b, ex.Message)); status = DataStatus.NotSupported; }
            if (errors.Count > 0) return new BeamPreparation { BeamId = beamId, Sample = sample, Status = status, Diagnostics = errors };
            // SectionAxes describes orientation. Keep the sample's point: this is a rotation, never moment transport.
            var orientation = b.Assignments.SectionGeometryAxes ?? b.Assignments.SectionAxes;
            try { Axes.Validate(sample.ResultBeamForces.CoordinateSystem); }
            catch (ArgumentException ex) { return new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.Insufficient, Diagnostics = new[] { ModelDiagnostic.Error("InvalidResultAxes", b, ex.Message) } }; }
            var target = new CoordinateSystem(sample.ResultBeamForces.CoordinateSystem.Origin, orientation.V1, orientation.V2, orientation.V3);
            if (!b.Assignments.ActionsAtSectionCentroidConfirmed && b.Assignments.SectionCentroidOffset == null)
                return new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.NotSupported, Diagnostics = new[] { ModelDiagnostic.Error("UnresolvedReductionPoint", b) } };
            ResultBeamForces preparedForces;
            try
            {
                var normalized = ResultOrientation.Beam(sample, ResultTransformations.AtPoint(b.Assignments.SectionAxes, sample.ResultBeamForces.CoordinateSystem.Origin));
                if (b.Assignments.SectionCentroidOffset != null) normalized = new BeamReferenceGeometry(b).AtCentroid(normalized);
                preparedForces = ResultOrientation.Beam(normalized, ResultTransformations.AtPoint(orientation, normalized.ResultBeamForces.CoordinateSystem.Origin)).ResultBeamForces;
            }
            catch (ArgumentException ex) { return new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.Insufficient, Diagnostics = new[] { ModelDiagnostic.Error("InvalidResultAxes", b, ex.Message) } }; }
            return new BeamPreparation
            {
                BeamId = beamId,
                Sample = sample,
                Status = DataStatus.Ready,
                Diagnostics = errors,
                Input = new BeamCheckInput
                {
                    Model = model,
                    SampleFingerprint = Persistence.ModelArchive.Fingerprint(new object[] { sample }),
                    PreparedForcesFingerprint = Persistence.ModelArchive.Fingerprint(new object[] { preparedForces }),
                    PreparedSectionFingerprint = Persistence.ModelArchive.Fingerprint(new object[] { section }),
                    BeamId = beamId,
                    Sample = sample,
                    Section = section,
                    Forces = preparedForces,
                    Settings = settings,
                    VerificationRevision = model.VerificationFingerprint(settings)
                }
            };
        }

        public static CheckResult Run(BeamPreparation preparation, CheckMechanism mechanism, IConcreteSectionVerifier verifier, CancellationToken cancellationToken = default(CancellationToken))
        {
            var result = new CheckResult { Mechanism = mechanism, Execution = ExecutionStatus.NotExecuted, Data = preparation.Status, Outcome = EngineeringOutcome.NotEvaluated };
            result.ElementId = preparation.BeamId; result.Case = preparation.Sample?.Case?.Name;
            result.Dataset = preparation.Sample?.State?.DatasetId; result.Station = preparation.Sample?.ParametricDistance;
            result.Side = preparation.Sample?.Side ?? SectionSide.Unspecified;
            result.Phase = preparation.Sample?.State?.Phase; result.Step = preparation.Sample?.State?.Step;
            result.Coverage = preparation.Sample?.State?.Coverage; result.ConcomitantStateId = preparation.Sample?.State?.ConcomitantStateId;
            result.MovingLoadPosition = preparation.Sample?.State?.MovingLoadPosition; result.Mode = preparation.Sample?.State?.Mode;
            if (cancellationToken.IsCancellationRequested) { result.Execution = ExecutionStatus.Cancelled; return result; }
            if (preparation.Input == null)
            {
                result.Diagnostics.AddRange(preparation.Diagnostics);
                foreach (var diagnostic in result.Diagnostics)
                { diagnostic.Station = preparation.Sample?.ParametricDistance; diagnostic.Dataset = result.Dataset; diagnostic.Case = result.Case; }
                return result;
            }
            var input = preparation.Input;
            if (!IsCurrent(input))
            { result.Data = DataStatus.Stale; result.Diagnostics.Add(ModelDiagnostic.Error("StalePreparation")); return result; }
            result.ElementId = input.BeamId; result.Station = input.Sample.ParametricDistance; result.Side = input.Sample.Side;
            result.Case = input.Sample.Case.Name; result.Dataset = input.Sample.State.DatasetId; result.VerificationRevision = input.VerificationRevision;
            result.Settings = input.Settings; result.SampleRevision = input.SampleFingerprint;
            result.Phase = input.Sample.State.Phase; result.Step = input.Sample.State.Step; result.Coverage = input.Sample.State.Coverage;
            result.ConcomitantStateId = input.Sample.State.ConcomitantStateId;
            result.Input = new CheckInputSnapshot(input.PreparedSectionFingerprint, input.SampleFingerprint,
                input.PreparedForcesFingerprint, new BeamForceSnapshot(input.Forces));
            if (verifier == null) { result.Data = DataStatus.MissingDependency; result.Diagnostics.Add(ModelDiagnostic.Error("MissingDependency")); return result; }
            try
            {
                var configuration = (verifier as IConfiguredSectionVerifier)?.Configuration;
                result.EngineVersion = verifier.Version; result.EngineConfiguration = configuration;
                if (!verifier.Capabilities.Contains(mechanism)) { result.Data = DataStatus.NotSupported; result.Diagnostics.Add(ModelDiagnostic.Error("UnsupportedMechanism")); return result; }
                var evaluated = verifier.Verify(input, mechanism, cancellationToken);
                if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(cancellationToken);
                if (evaluated == null) throw new InvalidOperationException("Verifier returned no result.");
                if (!IsCurrent(input) || configuration != (verifier as IConfiguredSectionVerifier)?.Configuration)
                { result.Data = DataStatus.Stale; result.Diagnostics.Add(ModelDiagnostic.Error("InputsChangedDuringVerification")); return result; }
                if (!CheckResultRules.ValidDecision(evaluated))
                    throw new InvalidOperationException("Invalid evaluated outcome or utilization returned by verifier.");
                evaluated.ElementId = result.ElementId; evaluated.Station = result.Station; evaluated.Side = result.Side; evaluated.Case = result.Case;
                evaluated.Dataset = result.Dataset; evaluated.VerificationRevision = result.VerificationRevision; evaluated.EngineVersion = verifier.Version; evaluated.Mechanism = mechanism;
                evaluated.EngineConfiguration = configuration; evaluated.Settings = input.Settings; evaluated.SampleRevision = result.SampleRevision;
                evaluated.Phase = result.Phase; evaluated.Step = result.Step; evaluated.Coverage = result.Coverage; evaluated.ConcomitantStateId = result.ConcomitantStateId;
                evaluated.MovingLoadPosition = result.MovingLoadPosition; evaluated.Mode = result.Mode; evaluated.Family = EntityFamily.Beam;
                evaluated.Input = result.Input;
                evaluated.SealEvidence();
                return evaluated;
            }
            catch (OperationCanceledException) { result.Execution = ExecutionStatus.Cancelled; }
            catch (Exception ex) { result.Execution = ExecutionStatus.Error; result.Diagnostics.Add(ModelDiagnostic.Error("VerifierError", message: ex.Message)); }
            return result;
        }

        internal static bool IsCurrent(BeamCheckInput input) => input.VerificationRevision == input.Model.VerificationFingerprint(input.Settings)
            && ResultAlgebra.HasCurrentDerivation(input.Model, input.Model.BeamElements[input.BeamId], input.Sample.State)
            && input.PreparedSectionFingerprint == Persistence.ModelArchive.Fingerprint(new object[] { input.Section })
            && input.SampleFingerprint == Persistence.ModelArchive.Fingerprint(new object[] { input.Sample })
            && input.PreparedForcesFingerprint == Persistence.ModelArchive.Fingerprint(new object[] { input.Forces });
    }

    /// <summary>Structural design method must be provided and benchmarked separately from axis and unit conversions.</summary>
    public interface IShellDesignActionMethod
    {
        string NameAndVersion { get; }
        ResultPlateForces DesignActions(ResultPlateForces normalized, ShellAssignments reinforcement);
    }

    public static class ShellPreparation
    {
        // Explicit uniaxial strip extraction from already determined design actions. No Wood-Armer/Baumann assumption.
        public static ResultBeamForces StripDirection1(ResultPlateForces designActions, double widthMm, CoordinateSystem sectionAxes)
        {
            if (NumericGuard.Finite(widthMm, nameof(widthMm)) <= 0) throw new ArgumentOutOfRangeException(nameof(widthMm));
            Axes.Validate(sectionAxes);
            return new ResultBeamForces(designActions.Fxx * widthMm, 0, designActions.Fxz * widthMm, 0, designActions.Mxx * widthMm, 0, sectionAxes);
        }
    }
}
