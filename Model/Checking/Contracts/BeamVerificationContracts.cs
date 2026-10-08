using GPC.Model.Checking;
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
        public bool IsCurrent => BeamCheckPreparation.IsCurrent(this);
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
        /// <summary>Direction, sub-check and combination category of a task planned through SectionChecks; null otherwise.</summary>
        [field: System.Runtime.Serialization.OptionalField] public SectionCheckSpecification Check { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public VerificationProvenance Provenance { get; set; }

        /// <summary>Detects subsequent edits to stored evidence; it is not a digital signature. Legacy results have no seal.</summary>
        public bool HasUnchangedEvidence => EvidenceFingerprint == null || EvidenceFingerprint == Evidence();
        public void SealEvidence() { EvidenceFingerprint = Evidence(); }
        private string Evidence()
        {
            var fields = new object[] { SchemaVersion, Execution, Data, Outcome, Mechanism,
            Utilization, EngineVersion, EngineConfiguration, Settings, SampleRevision, VerificationRevision, Family, ElementId,
            Source, Job, Dataset, Case, Station, Side, Phase, Step, Coverage, ConcomitantStateId, MovingLoadPosition, Mode,
                ShellPoint, ShellPointKind, ShellCoordinateKind, Face, Layer, GroupNames, Applicability, ApplicabilityReason, Standard, Input, Details, Diagnostics };
            // The specification enters only when present, so seals written before it existed remain valid.
            var evidence = SchemaVersion < 2 ? fields : fields.Concat(new object[] {
                Target, Scope, PlanItemId, MethodId, MemberLocation, MemberInput, CoverageAssessment }).Concat(Check == null ? new object[0] : new object[] { Check });
            return Persistence.ModelArchive.Fingerprint(evidence.Concat(Provenance == null ? new object[0] : new object[] { Provenance }));
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

    /// <summary>Section engine for tasks with direction, sub-check and combination category. Supports states the actual
    /// implemented combinations; an unsupported specification is never evaluated through a nearby one.</summary>
    public interface ISectionCheckVerifier : IConfiguredSectionVerifier
    {
        bool Supports(SectionCheckSpecification check);
        CheckResult Verify(BeamCheckInput input, SectionCheckSpecification check, CancellationToken cancellationToken);
    }

    public sealed class BeamPreparation
    {
        public int BeamId { get; internal set; }
        public StationResultBeamForces Sample { get; internal set; }
        public BeamCheckInput Input { get; internal set; }
        public DataStatus Status { get; internal set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; internal set; }
    }

}
