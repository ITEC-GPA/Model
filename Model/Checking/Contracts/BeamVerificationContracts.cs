using GPC.Model.Checking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Sections.Concrete;
using GPC.Model.Checking.Preparation;
using GPC.Model.Checking.Reports;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Structure.Members;

namespace GPC.Model.Checking.Contracts
{
    public enum CheckMechanism
    {
        UlsBiaxialSection,
        Shear,
        Torsion,
        Serviceability,
        Stability,
        Detailing
    }

    public sealed class BeamCheckInput
    {
        internal Models.Model Model { get; set; }
        internal string SampleFingerprint { get; set; }
        internal string PreparedForcesFingerprint { get; set; }
        internal string PreparedSectionFingerprint { get; set; }
        internal string SourceSectionFingerprint { get; set; }
        internal string DatasetFingerprint { get; set; }
        public int BeamId { get; internal set; }
        public StationResultBeamForces Sample { get; internal set; }
        public ReinforcedConcreteSection Section { get; internal set; }
        public ResultBeamForces Forces { get; internal set; }
        public string VerificationRevision { get; internal set; }
        public string Settings { get; internal set; }
        public bool IsCurrent => BeamCheckPreparation.IsCurrent(this);
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class CheckResult
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Execution>k__BackingField", IsRequired = true)]
        public ExecutionStatus Execution { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Data>k__BackingField", IsRequired = true)]
        public DataStatus Data { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Outcome>k__BackingField", IsRequired = true)]
        public EngineeringOutcome Outcome { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Mechanism>k__BackingField", IsRequired = true)]
        public CheckMechanism Mechanism { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Utilization>k__BackingField", IsRequired = true)]
        public double? Utilization { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<EngineVersion>k__BackingField", IsRequired = true)]
        public string EngineVersion { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<EngineConfiguration>k__BackingField", IsRequired = true)]
        public string EngineConfiguration { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Settings>k__BackingField", IsRequired = true)]
        public string Settings { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SampleRevision>k__BackingField", IsRequired = true)]
        public string SampleRevision { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Phase>k__BackingField", IsRequired = true)]
        public string Phase { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Step>k__BackingField", IsRequired = true)]
        public string Step { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Coverage>k__BackingField", IsRequired = true)]
        public string Coverage { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ConcomitantStateId>k__BackingField", IsRequired = true)]
        public string ConcomitantStateId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<VerificationRevision>k__BackingField", IsRequired = true)]
        public string VerificationRevision { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Family>k__BackingField", IsRequired = true)]
        public EntityFamily Family { get; set; } = EntityFamily.Beam;

        [field: System.Runtime.Serialization.DataMember(Name = "<Source>k__BackingField", IsRequired = true)]
        public SourceIdentity Source { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Job>k__BackingField", IsRequired = true)]
        public string Job { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<MovingLoadPosition>k__BackingField", IsRequired = true)]
        public string MovingLoadPosition { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Mode>k__BackingField", IsRequired = true)]
        public int? Mode { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ShellPoint>k__BackingField", IsRequired = true)]
        public Point2d ShellPoint { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ShellPointKind>k__BackingField", IsRequired = true)]
        public ShellResultPointKind ShellPointKind { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ShellCoordinateKind>k__BackingField", IsRequired = true)]
        public ResultCoordinateKind ShellCoordinateKind { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ElementId>k__BackingField", IsRequired = true)]
        public int ElementId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Case>k__BackingField", IsRequired = true)]
        public string Case { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Dataset>k__BackingField", IsRequired = true)]
        public string Dataset { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Station>k__BackingField", IsRequired = true)]
        public double? Station { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Side>k__BackingField", IsRequired = true)]
        public SectionSide Side { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Diagnostics>k__BackingField", IsRequired = true)]
        public List<ModelDiagnostic> Diagnostics { get; set; } = new List<ModelDiagnostic>();

        // Optional fields retain compatibility with archives written before the report contracts.
        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<SchemaVersion>k__BackingField", IsRequired = false)]
        public int SchemaVersion { get; set; } = 1;

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<Applicability>k__BackingField", IsRequired = false)]
        public CheckApplicability Applicability { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<ApplicabilityReason>k__BackingField", IsRequired = false)]
        public string ApplicabilityReason { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<Standard>k__BackingField", IsRequired = false)]
        public CheckStandardContext Standard { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<Input>k__BackingField", IsRequired = false)]
        public CheckInputSnapshot Input { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<Details>k__BackingField", IsRequired = false)]
        public CheckDetails Details { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<Face>k__BackingField", IsRequired = false)]
        public string Face { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<Layer>k__BackingField", IsRequired = false)]
        public string Layer { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<GroupNames>k__BackingField", IsRequired = false)]
        public string[] GroupNames { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<EvidenceFingerprint>k__BackingField", IsRequired = false)]
        public string EvidenceFingerprint { get; private set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<Target>k__BackingField", IsRequired = false)]
        public CheckTargetReference Target { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<Scope>k__BackingField", IsRequired = false)]
        public CheckScope Scope { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<PlanItemId>k__BackingField", IsRequired = false)]
        public string PlanItemId { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<MethodId>k__BackingField", IsRequired = false)]
        public string MethodId { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<MemberLocation>k__BackingField", IsRequired = false)]
        public MemberLocation MemberLocation { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<MemberInput>k__BackingField", IsRequired = false)]
        public MemberInputSnapshot MemberInput { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<CoverageAssessment>k__BackingField", IsRequired = false)]
        public CoverageAssessment CoverageAssessment { get; set; }

        /// <summary>Direction, sub-check and combination category of a task planned through SectionChecks; null otherwise.</summary>
        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<Check>k__BackingField", IsRequired = false)]
        public SectionCheckSpecification Check { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<Provenance>k__BackingField", IsRequired = false)]
        public VerificationProvenance Provenance { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<ShellInput>k__BackingField", IsRequired = false)]
        public ShellInputSnapshot ShellInput { get; set; }

        [field: System.Runtime.Serialization.OptionalField, GPC.Model.Core.FingerprintWhenSet]
        [field: System.Runtime.Serialization.DataMember(Name = "<ShellCheck>k__BackingField", IsRequired = false)]
        public ShellCheckSnapshot ShellCheck { get; set; }
        /// <summary>Detects subsequent edits to stored evidence; it is not a digital signature. Legacy results have no seal.</summary>
        public bool HasUnchangedEvidence => EvidenceFingerprint == null || EvidenceFingerprint == Evidence();

        public void SealEvidence()
        {
            EvidenceFingerprint = Evidence();
        }

        private string Evidence()
        {
            var fields = new object[]
            {
                SchemaVersion,
                Execution,
                Data,
                Outcome,
                Mechanism,
                Utilization,
                EngineVersion,
                EngineConfiguration,
                Settings,
                SampleRevision,
                VerificationRevision,
                Family,
                ElementId,
                Source,
                Job,
                Dataset,
                Case,
                Station,
                Side,
                Phase,
                Step,
                Coverage,
                ConcomitantStateId,
                MovingLoadPosition,
                Mode,
                ShellPoint,
                ShellPointKind,
                ShellCoordinateKind,
                Face,
                Layer,
                GroupNames,
                Applicability,
                ApplicabilityReason,
                Standard,
                Input,
                Details,
                Diagnostics
            };
            // The specification enters only when present, so seals written before it existed remain valid.
            var evidence = SchemaVersion < 2 ? fields : fields.Concat(new object[] { Target, Scope, PlanItemId, MethodId, MemberLocation, MemberInput, CoverageAssessment }).Concat(Check == null ? new object[0] : new object[] { Check });
            return Core.ModelValues.Fingerprint(evidence.Concat(Provenance == null ? new object[0] : new object[] { Provenance }).Concat(ShellInput == null ? new object[0] : new object[] { ShellInput }).Concat(ShellCheck == null ? new object[0] : new object[] { ShellCheck }));
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
