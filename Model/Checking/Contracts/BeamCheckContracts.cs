using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Checking.Preparation;
using GPC.Model.Checking.Reports;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Identity;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Queries;
using GPC.Model.Structure.Members;

namespace GPC.Model.Checking.Contracts
{
    public enum CheckScope
    {
        SectionSample,
        PhysicalMember,
        ShellPoint
    }

    public enum CheckTargetKind
    {
        FemElement,
        PhysicalMember
    }

    public enum BeamCoveragePolicy
    {
        ExportedSamples,
        RequiredLocations
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class CheckTargetReference
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Kind>k__BackingField", IsRequired = true)]
        public CheckTargetKind Kind { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<BeamId>k__BackingField", IsRequired = true)]
        public int? BeamId { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<MemberId>k__BackingField", IsRequired = true)]
        public string MemberId { get; private set; }

        [field: System.Runtime.Serialization.OptionalField, FingerprintWhenSet]
        [field: System.Runtime.Serialization.DataMember(Name = "<Family>k__BackingField", IsRequired = false)]
        public EntityFamily? Family { get; private set; }

        [field: System.Runtime.Serialization.OptionalField, FingerprintWhenSet]
        [field: System.Runtime.Serialization.DataMember(Name = "<ElementId>k__BackingField", IsRequired = false)]
        public int? ElementId { get; private set; }

        public CheckTargetReference(int beamId, string memberId = null)
        {
            Kind = CheckTargetKind.FemElement;
            BeamId = beamId;
            MemberId = memberId;
        }

        public CheckTargetReference(string memberId)
        {
            CheckValue.Text(memberId, nameof(memberId));
            Kind = CheckTargetKind.PhysicalMember;
            MemberId = memberId;
        }

        public CheckTargetReference(EntityFamily family, int elementId)
        {
            if (family != EntityFamily.Shell)
                throw new ArgumentException("Use the beam/member constructors for their existing identities.");
            Kind = CheckTargetKind.FemElement;
            Family = family;
            ElementId = elementId;
        }
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class RequiredBeamLocation
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<MemberId>k__BackingField", IsRequired = true)]
        public string MemberId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Distance>k__BackingField", IsRequired = true)]
        public double? Distance { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<BeamId>k__BackingField", IsRequired = true)]
        public int? BeamId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Station>k__BackingField", IsRequired = true)]
        public double? Station { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<StationDomain>k__BackingField", IsRequired = true)]
        public string StationDomain { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Side>k__BackingField", IsRequired = true)]
        public SectionSide Side { get; set; }

        internal RequiredBeamLocation Copy() => (RequiredBeamLocation)MemberwiseClone();
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class MemberCheckSpecification
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<MemberId>k__BackingField", IsRequired = true)]
        public string MemberId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<MethodId>k__BackingField", IsRequired = true)]
        public string MethodId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Mechanism>k__BackingField", IsRequired = true)]
        public CheckMechanism Mechanism { get; set; } = CheckMechanism.Stability;

        [field: System.Runtime.Serialization.DataMember(Name = "<Start>k__BackingField", IsRequired = true)]
        public double Start { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<End>k__BackingField", IsRequired = true)]
        public double? End { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Context>k__BackingField", IsRequired = true)]
        public MemberDesignContext Context { get; set; } = new MemberDesignContext();

        internal MemberCheckSpecification Copy() => (MemberCheckSpecification)MemberwiseClone();
    }

    /// <summary>New opt-in beam path. Null Elements means no direct FEM selection; members are always explicitly named.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamCheckPlanRequest
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Elements>k__BackingField", IsRequired = true)]
        public ElementSelection Elements { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<MemberIds>k__BackingField", IsRequired = true)]
        public string[] MemberIds { get; set; } = new string[0];

        [field: System.Runtime.Serialization.DataMember(Name = "<Results>k__BackingField", IsRequired = true)]
        public ResultSelection[] Results { get; set; } = new ResultSelection[0];

        [field: System.Runtime.Serialization.DataMember(Name = "<SectionMechanisms>k__BackingField", IsRequired = true)]
        /// <summary>Legacy local mechanisms: one task per sample and mechanism, for every result selection.</summary>
        public CheckMechanism[] SectionMechanisms { get; set; } = new[]
        {
            CheckMechanism.UlsBiaxialSection
        };

        /// <summary>Local tasks with direction, sub-check and combination category. A specification with a category is planned only
        /// for the result selections declared with that category. Null keeps the historical scope fingerprint.</summary>
        [field: System.Runtime.Serialization.OptionalField, FingerprintWhenSet]
        [field: System.Runtime.Serialization.DataMember(Name = "<SectionChecks>k__BackingField", IsRequired = false)]
        public SectionCheckSpecification[] SectionChecks { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<MemberChecks>k__BackingField", IsRequired = true)]
        public MemberCheckSpecification[] MemberChecks { get; set; } = new MemberCheckSpecification[0];

        [field: System.Runtime.Serialization.DataMember(Name = "<CoveragePolicy>k__BackingField", IsRequired = true)]
        public BeamCoveragePolicy CoveragePolicy { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Locations>k__BackingField", IsRequired = true)]
        public RequiredBeamLocation[] Locations { get; set; } = new RequiredBeamLocation[0];

        [field: System.Runtime.Serialization.DataMember(Name = "<Settings>k__BackingField", IsRequired = true)]
        public string Settings { get; set; }
        internal bool HasSectionChecks => SectionMechanisms.Length != 0 || (SectionChecks?.Length ?? 0) != 0;

        public BeamCheckPlanRequest Copy() => new BeamCheckPlanRequest
        {
            Elements = Elements?.Copy(),
            MemberIds = (string[])MemberIds.Clone(),
            Results = Results.Select(r => r.Copy()).ToArray(),
            SectionMechanisms = (CheckMechanism[])SectionMechanisms.Clone(),
            SectionChecks = SectionChecks?.Select(s => s?.Copy()).ToArray(),
            MemberChecks = MemberChecks.Select(s => s.Copy()).ToArray(),
            CoveragePolicy = CoveragePolicy,
            Locations = Locations.Select(l => l.Copy()).ToArray(),
            Settings = Settings
        };
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class CoverageAssessment
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Policy>k__BackingField", IsRequired = true)]
        public BeamCoveragePolicy Policy { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<RequiredLocations>k__BackingField", IsRequired = true)]
        public int RequiredLocations { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<AvailableLocations>k__BackingField", IsRequired = true)]
        public int AvailableLocations { get; private set; }
        public int MissingLocations => RequiredLocations - AvailableLocations;
        public bool ContinuousCoverage => false;
        public string Limitation => "Only the recorded locations/states are covered; no continuous maximum is certified.";

        public CoverageAssessment(BeamCoveragePolicy policy, int required, int available)
        {
            if (!Enum.IsDefined(typeof(BeamCoveragePolicy), policy) || required < 0 || available < 0 || available > required)
                throw new ArgumentException("InvalidCoverage");
            Policy = policy;
            RequiredLocations = required;
            AvailableLocations = available;
        }
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class MemberSampleSnapshot
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Location>k__BackingField", IsRequired = true)]
        public MemberLocation Location { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Input>k__BackingField", IsRequired = true)]
        public CheckInputSnapshot Input { get; private set; }

        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly ResultSelection _state;
        public ResultSelection State => _state.Copy();

        internal MemberSampleSnapshot(MemberLocation location, BeamCheckInput input)
        {
            Location = location;
            Input = Capture(input);
            var s = input.Sample.State;
            _state = new ResultSelection
            {
                Dataset = s.DatasetId,
                Case = input.Sample.Case.Name,
                Phase = s.Phase,
                Step = s.Step,
                ConcomitantState = s.ConcomitantStateId,
                Mode = s.Mode,
                MovingLoadPosition = s.MovingLoadPosition
            };
        }

        internal static CheckInputSnapshot Capture(BeamCheckInput input) => new CheckInputSnapshot(input.PreparedSectionFingerprint, input.SampleFingerprint, input.PreparedForcesFingerprint, new BeamForceSnapshot(input.Forces));
    }

    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class MemberInputSnapshot
    {
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly MemberSampleSnapshot[] _samples;
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly CoordinateSystem _axes;
        [field: System.Runtime.Serialization.DataMember(Name = "<Definition>k__BackingField", IsRequired = true)]
        public PhysicalMemberDefinition Definition { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Context>k__BackingField", IsRequired = true)]
        public MemberDesignContext Context { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Length>k__BackingField", IsRequired = true)]
        public double Length { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<DeformableLength>k__BackingField", IsRequired = true)]
        public double DeformableLength { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SpanStart>k__BackingField", IsRequired = true)]
        public double SpanStart { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SpanEnd>k__BackingField", IsRequired = true)]
        public double SpanEnd { get; private set; }
        public CoordinateSystem Axes => ActionTransformations.AtPoint(_axes, _axes.Origin);
        public IReadOnlyList<MemberSampleSnapshot> Samples => Array.AsReadOnly(_samples);

        internal MemberInputSnapshot(PhysicalMemberGeometry geometry, MemberCheckSpecification specification, IEnumerable<BeamCheckInput> inputs)
        {
            Definition = geometry.Definition;
            Context = specification.Context;
            Length = geometry.Length;
            DeformableLength = geometry.DeformableLength;
            SpanStart = specification.Start;
            SpanEnd = specification.End ?? geometry.Length;
            _axes = geometry.Axes;
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
