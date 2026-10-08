using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restrains;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;

namespace GPC.Model.PostProcessing
{
    public enum BeamFormulation { Unknown, StraightTwoNode, Truss, Cable, Link, Curved, HigherOrder, TensionOnly }
    public enum NodalForceKind { Unknown, SupportReaction, SpringForce, LinkForce, ElementEndForce, ElementNodeForce }
    public enum ActionBody { Unknown, OnNode, OnSupport, OnElement, PositiveSectionFace }
    public enum SpringKind { GeneralLinear, SymmetricElastic, Gap, CompressionOnly, Nonlinear }
    public enum AssignmentMode { Add, ReplaceScope }

    [Serializable]
    public sealed class RestrainAssignment
    {
        public NodeRestrain Restrain { get; set; }
        public ILoadCase Case { get; set; }
        public string Phase { get; set; }
        public string SourceRecord { get; set; }
    }

    [Serializable]
    public sealed class NodeAssignments
    {
        // Availability is independent of kinematic constraints and exported results.
        public ComponentAvailability[] Dofs { get; set; }
        public bool? Active { get; set; }
        public List<RestrainAssignment> Restrains { get; private set; } = new List<RestrainAssignment>();
        public List<NodalLink> Links { get; private set; } = new List<NodalLink>();
        public List<SpringMatrix> GroundSprings { get; private set; } = new List<SpringMatrix>();
        public NodalMass Mass { get; set; }

        /// <summary>Add rejects overlapping DOFs in the same case/phase; ReplaceScope explicitly replaces that scope.</summary>
        public void AssignRestrain(RestrainAssignment assignment, AssignmentMode mode)
        {
            if (assignment?.Restrain == null) throw new ArgumentNullException(nameof(assignment));
            if (!Enum.IsDefined(typeof(AssignmentMode), mode)) throw new ArgumentOutOfRangeException(nameof(mode));
            var scope = Restrains.Where(a => Equals(a.Case, assignment.Case) && a.Phase == assignment.Phase).ToArray();
            if (mode == AssignmentMode.Add && scope.Any(a => a.Restrain.Restrains.Any(d => assignment.Restrain.Restrains.Any(n => n.Dof == d.Dof))))
                throw new InvalidOperationException("ConflictingRestrain: select ReplaceScope explicitly.");
            if (mode == AssignmentMode.ReplaceScope) foreach (var old in scope) Restrains.Remove(old);
            Restrains.Add(assignment);
        }
    }

    /// <summary>Mass in kg; rotational inertia in kg mm^2; cross terms kg mm. No implicit conversion to weight.</summary>
    [Serializable]
    public sealed class NodalMass
    {
        public CoordinateSystem Axes { get; set; }
        public double[] Matrix6x6RowMajor { get; set; }
        public string SourceRecord { get; set; }
        public bool? IncludedInSourceTotal { get; set; }
    }

    [Serializable]
    public sealed class NodalLink
    {
        public int OtherNodeId { get; set; }
        public SpringMatrix Spring { get; set; }
        // Rows express C_i u_i + C_j u_j = rhs. This is not an artificial stiff beam.
        public double[] KinematicCoefficients { get; set; }
        public double[] RightHandSide { get; set; }
        public Vector3d LeverArm { get; set; }
        public string SourceRecord { get; set; }
        public string UnsupportedLaw { get; set; }
    }

    /// <summary>DOF order DX,DY,DZ,RX,RY,RZ; forces Fx,Fy,Fz,Mx,My,Mz. Canonical N,mm,rad.
    /// Entries have units row action / column displacement: N/mm, N/rad, Nmm/mm, Nmm/rad.</summary>
    [Serializable]
    public sealed class SpringMatrix
    {
        private double[] _values;
        public CoordinateSystem CoordinateSystem { get; private set; }
        public SpringKind Kind { get; private set; }
        public string SourceRecord { get; set; }
        public SpringMatrix(double[] rowMajor, CoordinateSystem axes, SpringKind kind = SpringKind.SymmetricElastic)
        {
            _values = rowMajor == null ? null : (double[])rowMajor.Clone(); CoordinateSystem = axes; Kind = kind; Validate();
        }
        public double this[int row, int column] => _values[row * 6 + column];
        public double[] ToArray() => (double[])_values.Clone();
        [OnDeserialized] private void OnDeserialized(StreamingContext context) => Validate();
        private void Validate()
        {
            if (_values == null || _values.Length != 36) throw new ArgumentException("A spring requires all 36 coefficients.");
            foreach (double v in _values) NumericGuard.Finite(v, "stiffness");
            Axes.Validate(CoordinateSystem);
        }
        public IReadOnlyList<ModelDiagnostic> Diagnose()
        {
            var issues = new List<ModelDiagnostic>();
            if (Kind != SpringKind.SymmetricElastic && Kind != SpringKind.GeneralLinear) issues.Add(ModelDiagnostic.Error("UnsupportedSpringLaw"));
            if (Kind == SpringKind.SymmetricElastic)
            {
                bool symmetric = true;
                for (int r = 0; r < 6; r++) for (int c = r + 1; c < 6; c++)
                        if (Math.Abs(this[r, c] - this[c, r]) > 1e-10 * Math.Max(1, Math.Max(Math.Abs(this[r, c]), Math.Abs(this[c, r])))) symmetric = false;
                if (!symmetric) issues.Add(ModelDiagnostic.Error("NonSymmetricSpring"));
                else
                {
                    var m = MathNet.Numerics.LinearAlgebra.Double.DenseMatrix.Create(6, 6, (r, c) => this[r, c]);
                    if (m.Evd(MathNet.Numerics.LinearAlgebra.Symmetricity.Symmetric).EigenValues.Any(v => v.Real < -1e-10))
                        issues.Add(ModelDiagnostic.Error("NonPositiveSpring"));
                }
            }
            return issues;
        }
        public double[] Apply(double[] displacement)
        {
            if (displacement == null || displacement.Length != 6) throw new ArgumentException("Six displacements are required.");
            if (Diagnose().Count != 0) throw new NotSupportedException("Spring is preserved but not eligible for linear evaluation.");
            var result = new double[6];
            for (int r = 0; r < 6; r++) for (int c = 0; c < 6; c++) result[r] += this[r, c] * NumericGuard.Finite(displacement[c], "displacement");
            return result;
        }
        public SpringMatrix ToCoordinateSystem(CoordinateSystem target)
        {
            Axes.Validate(target);
            if (Axes.Length(target.Origin - CoordinateSystem.Origin) > 1e-8) throw new NotSupportedException("Spring rotation requires the same point.");
            var oldAxes = new[] { CoordinateSystem.V1, CoordinateSystem.V2, CoordinateSystem.V3 };
            var newAxes = new[] { target.V1, target.V2, target.V3 };
            var q = new double[6, 6];
            for (int r = 0; r < 6; r++) for (int c = 0; c < 6; c++) if (r / 3 == c / 3) q[r, c] = Axes.Dot(newAxes[r % 3], oldAxes[c % 3]);
            var result = new double[36];
            for (int r = 0; r < 6; r++) for (int c = 0; c < 6; c++) for (int i = 0; i < 6; i++) for (int j = 0; j < 6; j++)
                            result[r * 6 + c] += q[r, i] * this[i, j] * q[c, j];
            return new SpringMatrix(result, target, Kind) { SourceRecord = SourceRecord };
        }
    }

    [Serializable]
    public sealed class BeamSectionAssignment
    {
        [OptionalField, GPC.Model.Persistence.FingerprintWhenSet]
        private GPC.Model.ElementProperties.BeamProperty _property;
        [OptionalField, GPC.Model.Persistence.FingerprintWhenSet]
        private GPC.Model.ElementProperties.BeamProperty _endProperty;

        public double Start { get; set; }
        public double End { get; set; }
        /// <summary>Legacy concrete API. New material-neutral callers use <see cref="Property"/>.</summary>
        public ReinforcedConcreteSection Section { get; set; }
        [field: OptionalField] public ReinforcedConcreteSection EndSection { get; set; }
        /// <summary>The assigned property, including steel, concrete and composite beam properties.
        /// Concrete values retain their historical serialized representation.</summary>
        public GPC.Model.ElementProperties.BeamProperty Property
        {
            get => SectionLaws.ResolveProperty(_property, Section);
            set { Section = value as ReinforcedConcreteSection; _property = Section is null ? value : null; }
        }
        public GPC.Model.ElementProperties.BeamProperty EndProperty
        {
            get => SectionLaws.ResolveProperty(_endProperty, EndSection);
            set { EndSection = value as ReinforcedConcreteSection; _endProperty = EndSection is null ? value : null; }
        }
        /// <summary>For Tabulated: exact, absolute stations in the assignment domain. No implicit interpolation.</summary>
        [field: OptionalField] public List<BeamSectionStation> Stations { get; private set; } = new List<BeamSectionStation>();
        public string Law { get; set; } = "Constant";
        [OnDeserialized] private void OnDeserialized(StreamingContext context) { if (Stations == null) Stations = new List<BeamSectionStation>(); }
    }

    [Serializable]
    public sealed class BeamAssignments
    {
        public BeamFormulation Formulation { get; set; }
        public string LogicalMember { get; set; }
        public int? OrientationNodeId { get; set; }
        public CoordinateSystem SectionAxes { get; set; }
        /// <summary>Physical axes of the stored section geometry/rebars when different from the beam's oriented cut frame.
        /// Used after I/J reversal to retain arbitrary, asymmetric section shapes without mirroring their coordinates.</summary>
        [field: OptionalField] public CoordinateSystem SectionGeometryAxes { get; set; }
        public Vector3d OffsetI { get; set; }
        public Vector3d OffsetJ { get; set; }
        public CoordinateSystem OffsetAxes { get; set; }
        /// <summary>Rigid zones measured along the offset reference line, in mm.</summary>
        [field: OptionalField] public double RigidLengthI { get; set; }
        [field: OptionalField] public double RigidLengthJ { get; set; }
        /// <summary>Centroid relative to the offset reference line, in SectionAxes V1,V2 (mm). Null means unresolved.</summary>
        [field: OptionalField] public Vector2d SectionCentroidOffset { get; set; }
        public string StationDomain { get; set; }
        public bool ActionsAtSectionCentroidConfirmed { get; set; }
        public List<BeamLoadAssignment> Loads { get; private set; } = new List<BeamLoadAssignment>();
        public List<BeamSectionAssignment> Sections { get; private set; } = new List<BeamSectionAssignment>();
        [field: OptionalField] public BeamAnalysisProfile AnalysisProfile { get; set; }
        public List<PreservedAssignment> OtherAssignments { get; private set; } = new List<PreservedAssignment>();
        public ReinforcedConcreteSection SectionAt(double station, SectionSide side)
            => SectionLaws.RequireConcrete(PropertyAt(station, side));

        /// <summary>Resolves the exact assigned property at a station/side. Missing intervals and ambiguous cuts fail explicitly.</summary>
        public GPC.Model.ElementProperties.BeamProperty PropertyAt(double station, SectionSide side)
        {
            NumericGuard.Station(station);
            var matches = Sections.Where(a => station >= a.Start && station <= a.End
                && (station != a.Start || station == 0 || side != SectionSide.Left)
                && (station != a.End || station == 1 || side != SectionSide.Right)).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException("MissingOrAmbiguousSectionAssignment");
            return SectionLaws.EvaluateProperty(matches[0], station, side);
        }
    }

    [Serializable]
    public sealed class ShellRebarLayer
    {
        public string PhysicalFace { get; set; }
        public SteelMaterial Steel { get; set; }
        public double Diameter { get; set; }
        public double Pitch { get; set; }
        public double AxisPositionThroughThickness { get; set; }
        public double DirectionRadians { get; set; }
        public int Order { get; set; }
    }

    [Serializable]
    public sealed class ShellAssignments
    {
        public double? PhysicalThickness { get; set; }
        public double? Offset { get; set; }
        public CoordinateSystem LayerAxes { get; set; }
        public List<ShellRebarLayer> Layers { get; private set; } = new List<ShellRebarLayer>();
        public string ReinforcementZone { get; set; }
    }

    [Serializable]
    public sealed class PreservedAssignment
    {
        public string Kind { get; set; }
        public string SourceRecord { get; set; }
        public string RawData { get; set; }
        public string UnitsAndAxes { get; set; }
        public string UnsupportedReason { get; set; }
    }
}
