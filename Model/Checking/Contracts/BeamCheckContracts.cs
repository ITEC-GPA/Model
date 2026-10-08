using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Persistence;

namespace GPC.Model.PostProcessing
{
    public enum CheckScope { SectionSample, PhysicalMember, ShellPoint }
    public enum CheckTargetKind { FemElement, PhysicalMember }
    public enum BeamCoveragePolicy { ExportedSamples, RequiredLocations }

    [Serializable]
    public sealed class CheckTargetReference
    {
        public CheckTargetKind Kind { get; private set; }
        public int? BeamId { get; private set; }
        public string MemberId { get; private set; }
        [field: System.Runtime.Serialization.OptionalField, FingerprintWhenSet] public EntityFamily? Family { get; private set; }
        [field: System.Runtime.Serialization.OptionalField, FingerprintWhenSet] public int? ElementId { get; private set; }
        public CheckTargetReference(int beamId, string memberId = null) { Kind = CheckTargetKind.FemElement; BeamId = beamId; MemberId = memberId; }
        public CheckTargetReference(string memberId) { CheckValue.Text(memberId, nameof(memberId)); Kind = CheckTargetKind.PhysicalMember; MemberId = memberId; }
        public CheckTargetReference(EntityFamily family, int elementId)
        {
            if (family != EntityFamily.Shell) throw new ArgumentException("Use the beam/member constructors for their existing identities.");
            Kind = CheckTargetKind.FemElement; Family = family; ElementId = elementId;
        }
    }

    [Serializable]
    public sealed class RequiredBeamLocation
    {
        public string MemberId { get; set; }
        public double? Distance { get; set; }
        public int? BeamId { get; set; }
        public double? Station { get; set; }
        public string StationDomain { get; set; }
        public SectionSide Side { get; set; }
        internal RequiredBeamLocation Copy() => (RequiredBeamLocation)MemberwiseClone();
    }
    [Serializable]
    public sealed class MemberCheckSpecification
    {
        public string MemberId { get; set; }
        public string MethodId { get; set; }
        public CheckMechanism Mechanism { get; set; } = CheckMechanism.Stability;
        public double Start { get; set; }
        public double? End { get; set; }
        public MemberDesignContext Context { get; set; } = new MemberDesignContext();
        internal MemberCheckSpecification Copy() => (MemberCheckSpecification)MemberwiseClone();
    }
    /// <summary>New opt-in beam path. Null Elements means no direct FEM selection; members are always explicitly named.</summary>
    [Serializable]
    public sealed class BeamCheckPlanRequest
    {
        public ElementSelection Elements { get; set; }
        public string[] MemberIds { get; set; } = new string[0];
        public ResultSelection[] Results { get; set; } = new ResultSelection[0];
        /// <summary>Legacy local mechanisms: one task per sample and mechanism, for every result selection.</summary>
        public CheckMechanism[] SectionMechanisms { get; set; } = new[] { CheckMechanism.UlsBiaxialSection };
        /// <summary>Local tasks with direction, sub-check and combination category. A specification with a category is planned only
        /// for the result selections declared with that category. Null keeps the historical scope fingerprint.</summary>
        [field: System.Runtime.Serialization.OptionalField, FingerprintWhenSet] public SectionCheckSpecification[] SectionChecks { get; set; }
        public MemberCheckSpecification[] MemberChecks { get; set; } = new MemberCheckSpecification[0];
        public BeamCoveragePolicy CoveragePolicy { get; set; }
        public RequiredBeamLocation[] Locations { get; set; } = new RequiredBeamLocation[0];
        public string Settings { get; set; }
        internal bool HasSectionChecks => SectionMechanisms.Length != 0 || (SectionChecks?.Length ?? 0) != 0;
        public BeamCheckPlanRequest Copy() => new BeamCheckPlanRequest { Elements = Elements?.Copy(), MemberIds = (string[])MemberIds.Clone(),
            Results = Results.Select(r => r.Copy()).ToArray(), SectionMechanisms = (CheckMechanism[])SectionMechanisms.Clone(),
            SectionChecks = SectionChecks?.Select(s => s?.Copy()).ToArray(),
            MemberChecks = MemberChecks.Select(s => s.Copy()).ToArray(), CoveragePolicy = CoveragePolicy,
            Locations = Locations.Select(l => l.Copy()).ToArray(), Settings = Settings };
    }

    [Serializable]
    public sealed class CoverageAssessment
    {
        public BeamCoveragePolicy Policy { get; private set; }
        public int RequiredLocations { get; private set; }
        public int AvailableLocations { get; private set; }
        public int MissingLocations => RequiredLocations - AvailableLocations;
        public bool ContinuousCoverage => false;
        public string Limitation => "Only the recorded locations/states are covered; no continuous maximum is certified.";
        public CoverageAssessment(BeamCoveragePolicy policy, int required, int available)
        {
            if (!Enum.IsDefined(typeof(BeamCoveragePolicy), policy) || required < 0 || available < 0 || available > required) throw new ArgumentException("InvalidCoverage");
            Policy = policy; RequiredLocations = required; AvailableLocations = available;
        }
    }
    [Serializable]
    public sealed class MemberSampleSnapshot
    {
        public MemberLocation Location { get; private set; }
        public CheckInputSnapshot Input { get; private set; }
        private readonly ResultSelection _state;
        public ResultSelection State => _state.Copy();
        internal MemberSampleSnapshot(MemberLocation location, BeamCheckInput input)
        {
            Location = location; Input = Capture(input); var s = input.Sample.State;
            _state = new ResultSelection { Dataset = s.DatasetId, Case = input.Sample.Case.Name, Phase = s.Phase, Step = s.Step,
                ConcomitantState = s.ConcomitantStateId, Mode = s.Mode, MovingLoadPosition = s.MovingLoadPosition };
        }
        internal static CheckInputSnapshot Capture(BeamCheckInput input) => new CheckInputSnapshot(input.PreparedSectionFingerprint,
            input.SampleFingerprint, input.PreparedForcesFingerprint, new BeamForceSnapshot(input.Forces));
    }
    [Serializable]
    public sealed class MemberInputSnapshot
    {
        private readonly MemberSampleSnapshot[] _samples;
        private readonly CoordinateSystem _axes;
        public PhysicalMemberDefinition Definition { get; private set; }
        public MemberDesignContext Context { get; private set; }
        public double Length { get; private set; }
        public double DeformableLength { get; private set; }
        public double SpanStart { get; private set; }
        public double SpanEnd { get; private set; }
        public CoordinateSystem Axes => ActionTransformations.AtPoint(_axes, _axes.Origin);
        public IReadOnlyList<MemberSampleSnapshot> Samples => Array.AsReadOnly(_samples);
        internal MemberInputSnapshot(PhysicalMemberGeometry geometry, MemberCheckSpecification specification, IEnumerable<BeamCheckInput> inputs)
        {
            Definition = geometry.Definition; Context = specification.Context; Length = geometry.Length; DeformableLength = geometry.DeformableLength;
            SpanStart = specification.Start; SpanEnd = specification.End ?? geometry.Length; _axes = geometry.Axes;
            _samples = inputs.Select(i => new MemberSampleSnapshot(geometry.FromElement(i.BeamId, i.Sample.ParametricDistance, i.Sample.StationDomain, i.Sample.Side), i)).ToArray();
        }
    }
    public sealed class PhysicalMemberCheckInput
    {
        public MemberInputSnapshot Snapshot { get; internal set; }
        public IReadOnlyList<BeamCheckInput> Sections { get; internal set; }
        public string MethodId { get; internal set; }
        public CheckMechanism Mechanism { get; internal set; }
    }
    /// <summary>Optional global engine, distinct from a section engine. Method IDs identify actual supported algorithms.</summary>
    public interface IPhysicalMemberVerifier
    {
        string Version { get; }
        string Configuration { get; }
        CheckStandardContext Standard { get; }
        bool Supports(string methodId, CheckMechanism mechanism);
        CheckResult Verify(PhysicalMemberCheckInput input, CancellationToken cancellationToken);
    }
}
