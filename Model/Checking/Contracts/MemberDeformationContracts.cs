using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Checking.Reports;
using GPC.Model.Results.Queries;
using GPC.Model.Structure.Members;

namespace GPC.Model.Checking.Contracts
{
    /// <summary>Deformation criterion of a physical-member serviceability task. Each criterion is a distinct task.</summary>
    public enum MemberDeformationCriterion
    {
        /// <summary>Deflection relative to the supports of the design span (chord, or the support of a cantilever).</summary>
        VerticalDeflection,
        /// <summary>Relative transverse displacement between the ends of the design span (storey drift of a column).</summary>
        HorizontalDisplacement,
        /// <summary>Natural frequency estimated from the deflection of the declared mass state.</summary>
        Vibration
    }

    /// <summary>Displacement component in the member axes (PhysicalMemberGeometry.Axes: V3 along the member, V1/V2 transverse).</summary>
    public enum MemberDeformationDirection
    {
        Axis1,
        Axis2,
        /// <summary>Magnitude of the two transverse components; horizontal displacements only.</summary>
        Transverse
    }

    /// <summary>Source of the deflection due to the variable actions (NTC delta2, EN 1990 w3). Never inferred from case names.</summary>
    public enum MemberVariableDeflectionBasis
    {
        /// <summary>No variable-action deflection is prepared.</summary>
        None,
        /// <summary>The declared selection is the state of the variable actions alone, paired with the only task state of the category.</summary>
        Direct,
        /// <summary>Linear superposition: the task state minus the declared state (for example the permanent actions alone).</summary>
        Difference
    }

    /// <summary>
    /// Discriminators of a member deformation task: criterion, member-axis direction and, for vertical deflections, the declared
    /// source of the variable-action part. Limits, schemes and sources belong to the engine configuration.
    /// </summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class MemberDeformationSpecification
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Criterion>k__BackingField", IsRequired = true)]
        public MemberDeformationCriterion Criterion { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Direction>k__BackingField", IsRequired = true)]
        public MemberDeformationDirection Direction { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<VariableBasis>k__BackingField", IsRequired = true)]
        public MemberVariableDeflectionBasis VariableBasis { get; set; }

        /// <summary>Exact state of the variable actions (Direct) or of the deducted actions (Difference); null with None.</summary>
        [field: System.Runtime.Serialization.DataMember(Name = "<VariableSelection>k__BackingField", IsRequired = true)]
        public ResultSelection VariableSelection { get; set; }

        public MemberDeformationSpecification()
        {
        }

        public MemberDeformationSpecification(MemberDeformationCriterion criterion, MemberDeformationDirection direction,
            MemberVariableDeflectionBasis variableBasis = MemberVariableDeflectionBasis.None, ResultSelection variableSelection = null)
        {
            Criterion = criterion;
            Direction = direction;
            VariableBasis = variableBasis;
            VariableSelection = variableSelection;
        }

        /// <summary>Stable readable key, for example "VerticalDeflection/Axis2/Difference".</summary>
        public string Key => Criterion + "/" + Direction + "/" + VariableBasis;

        public MemberDeformationSpecification Copy()
        {
            var copy = (MemberDeformationSpecification)MemberwiseClone();
            copy.VariableSelection = VariableSelection?.Copy();
            return copy;
        }

        /// <summary>Rejects incoherent discriminators; it does not state that an engine supports the task.</summary>
        internal void Validate()
        {
            if (!Enum.IsDefined(typeof(MemberDeformationCriterion), Criterion) || !Enum.IsDefined(typeof(MemberDeformationDirection), Direction)
                || !Enum.IsDefined(typeof(MemberVariableDeflectionBasis), VariableBasis))
                throw new ArgumentException("InvalidMemberDeformationSpecification");
            if (Direction == MemberDeformationDirection.Transverse && Criterion != MemberDeformationCriterion.HorizontalDisplacement)
                throw new ArgumentException("TransverseDirectionOnlyForHorizontalDisplacement: " + Key);
            if (VariableBasis != MemberVariableDeflectionBasis.None && Criterion != MemberDeformationCriterion.VerticalDeflection)
                throw new ArgumentException("VariableDeflectionOnlyForVerticalDeflection: " + Key);
            if ((VariableBasis == MemberVariableDeflectionBasis.None) != (VariableSelection == null))
                throw new ArgumentException("VariableSelectionRequiresBasis: " + Key);
            if (VariableSelection != null && (string.IsNullOrWhiteSpace(VariableSelection.Dataset) || string.IsNullOrWhiteSpace(VariableSelection.Case)))
                throw new ArgumentException("InvalidVariableSelection: " + Key);
        }
    }

    /// <summary>Displacements of one member node in one result state, rotated into the member axes. mm and rad.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class MemberDisplacementSample
    {
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly ResultSelection _state;

        [field: System.Runtime.Serialization.DataMember(Name = "<Location>k__BackingField", IsRequired = true)]
        public MemberLocation Location { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<NodeId>k__BackingField", IsRequired = true)]
        public int NodeId { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<D1>k__BackingField", IsRequired = true)]
        public double D1 { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<D2>k__BackingField", IsRequired = true)]
        public double D2 { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<D3>k__BackingField", IsRequired = true)]
        public double D3 { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<R1>k__BackingField", IsRequired = true)]
        public double R1 { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<R2>k__BackingField", IsRequired = true)]
        public double R2 { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<R3>k__BackingField", IsRequired = true)]
        public double R3 { get; private set; }

        /// <summary>Fingerprint of the source nodal sample, as CheckInputSnapshot.SampleFingerprint for the forces.</summary>
        [field: System.Runtime.Serialization.DataMember(Name = "<SampleFingerprint>k__BackingField", IsRequired = true)]
        public string SampleFingerprint { get; private set; }

        public ResultSelection State => _state.Copy();

        internal MemberDisplacementSample(MemberLocation location, int nodeId, ResultSelection state, double[] values, string sampleFingerprint)
        {
            if (location == null || state == null || values == null || values.Length != 6) throw new ArgumentException("InvalidMemberDisplacementSample");
            foreach (var v in values) CheckValue.Finite(v, nameof(values));
            CheckValue.Text(sampleFingerprint, nameof(sampleFingerprint));
            Location = location; NodeId = nodeId; _state = state.Copy();
            D1 = values[0]; D2 = values[1]; D3 = values[2]; R1 = values[3]; R2 = values[4]; R3 = values[5];
            SampleFingerprint = sampleFingerprint;
        }
    }

    /// <summary>Deformation input of a member serviceability task: the declared task and category, the member-node displacements of
    /// the task state (Total) and, for the variable-action deflection, of the declared selection (Variable), ordered along the member.
    /// Only exported nodes are covered; no displacement is interpolated between them.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class MemberDeformationSnapshot
    {
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly MemberDeformationSpecification _specification;
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly MemberDisplacementSample[] _total;
        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private readonly MemberDisplacementSample[] _variable;

        [field: System.Runtime.Serialization.DataMember(Name = "<Category>k__BackingField", IsRequired = true)]
        public CombinationCategory Category { get; private set; }

        public MemberDeformationSpecification Specification => _specification.Copy();
        public IReadOnlyList<MemberDisplacementSample> Total => Array.AsReadOnly(_total);
        /// <summary>Empty when the specification declares no variable-action deflection.</summary>
        public IReadOnlyList<MemberDisplacementSample> Variable => Array.AsReadOnly(_variable);

        internal MemberDeformationSnapshot(MemberDeformationSpecification specification, CombinationCategory category,
            IEnumerable<MemberDisplacementSample> total, IEnumerable<MemberDisplacementSample> variable)
        {
            _specification = (specification ?? throw new ArgumentNullException(nameof(specification))).Copy();
            Category = category;
            _total = total.OrderBy(s => s.Location.Distance).ToArray();
            _variable = (variable ?? Enumerable.Empty<MemberDisplacementSample>()).OrderBy(s => s.Location.Distance).ToArray();
        }
    }
}
