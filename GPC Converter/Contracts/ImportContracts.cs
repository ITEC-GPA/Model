using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using GPC.Geometry;
using GPC.Model.ElementProperties;
using GPC.Model.Models;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter
{
    public enum ImportStatus { Completed, Partial, Rejected, Cancelled }
    public enum CapabilityStatus { ImplementedSyntheticTests, InterpretedWithoutRealFileTest, PreservedOnly, NotSupported }

    public sealed class ModelFileReadException : FormatException
    {
        public string Record { get; }
        public ModelFileReadException(string record, string message) : base(record + ": " + message) { Record = record; }
    }

    // Import DTOs; ownership and FEM identity are assigned only by ModelMapper.
    public sealed class NodeRecord
    {
        public string Id { get; set; }
        public Point3d GlobalPosition { get; set; }
        public CoordinateSystem CoordinateSystem { get; set; } // Optional explicit node orientation; never inferred from a connected beam.
        public string Record { get; set; }
    }
    public sealed class BeamRecord
    {
        public string Id { get; set; }
        public string I { get; set; }
        public string J { get; set; }
        public CoordinateSystem CoordinateSystem { get; set; }
        public BeamProperty Property { get; set; }
        // Unknown maps to StraightTwoNode.
        public BeamFormulation Formulation { get; set; }
        // Source references resolved by ModelMapper when Property is null.
        public string MaterialId { get; set; }
        public string SectionId { get; set; }
        // Rigid offsets, mm: vectors from the connectivity nodes to the beam ends, in OffsetAxes.
        public Vector3d OffsetI { get; set; }
        public Vector3d OffsetJ { get; set; }
        public CoordinateSystem OffsetAxes { get; set; }
        // Rigid zones along the reference line, mm.
        public double RigidLengthI { get; set; }
        public double RigidLengthJ { get; set; }
        // Element-level centroid position from the reference line in V1,V2 (mm); overrides the section reference.
        public Vector2d CentroidOffset { get; set; }
        public string Record { get; set; }
        public List<PreservedAssignment> OtherAssignments { get; } = new List<PreservedAssignment>();
    }
    public sealed class ShellRecord
    {
        public string Id { get; set; }
        public string[] Nodes { get; set; }
        public CoordinateSystem CoordinateSystem { get; set; }
        public PlateProperty Property { get; set; }
        public string MaterialId { get; set; }
        public string ThicknessId { get; set; }
        // Reference surface to mid-surface along V3, mm; null when the source declares none.
        public double? Offset { get; set; }
        public string Record { get; set; }
    }

    public enum MaterialKind { Unknown, Steel, Concrete, Other }
    /// <summary>Elastic data in N/mm², t/mm³ and 1/°C. Strengths come only from a recognised grade, never from elastic values.</summary>
    public sealed class MaterialRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public MaterialKind Kind { get; set; }
        public string Standard { get; set; } // Source code/database label, e.g. "EN05(S)"; retained, not interpreted.
        public string Grade { get; set; } // Source grade designation, e.g. "S355" or "C30/37"; null when user-defined.
        public double? ElasticModulus { get; set; }
        public double? Poisson { get; set; }
        public double? Density { get; set; }
        public double? ThermalExpansion { get; set; }
        public string Record { get; set; }
    }

    public enum SectionShapeKind { Unknown, I, Channel, Angle, Tee, Box, Pipe, SolidRectangle, SolidCircle }
    public enum SectionHorizontalReference { Centroid, Center, MinimumV1, MaximumV1 }
    public enum SectionVerticalReference { Centroid, Center, MinimumV2, MaximumV2 }
    /// <summary>Position of the beam reference line on the cross-section: horizontal along V1, vertical along V2; Center is the
    /// middle of the bounding box. Shifts are additional distances in mm from that point, positive along V1/V2.</summary>
    public sealed class SectionReference
    {
        public SectionHorizontalReference Horizontal { get; set; }
        public SectionVerticalReference Vertical { get; set; }
        public double HorizontalShift { get; set; }
        public double VerticalShift { get; set; }
        public static SectionReference Centroid => new SectionReference();
    }
    /// <summary>Numeric section properties in mm units, principal axes 1/2 along V1/V2.</summary>
    public sealed class SectionValues
    {
        public double Area { get; set; }
        public double I11 { get; set; }
        public double I22 { get; set; }
        public double Torsion { get; set; }
        public double Warping { get; set; }
    }
    /// <summary>One of: catalog designation, shape with dimensions (mm, order documented per kind in PropertyMapper), or numeric values.</summary>
    public sealed class SectionRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CatalogDesignation { get; set; }
        public SectionShapeKind Shape { get; set; }
        public double[] Dimensions { get; set; }
        public SectionValues Values { get; set; }
        public SectionReference Reference { get; set; }
        public string Record { get; set; }
    }

    /// <summary>Plate thickness in mm. OutOfPlane is null when equal to InPlane.</summary>
    public sealed class ThicknessRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double InPlane { get; set; }
        public double? OutOfPlane { get; set; }
        public double? Offset { get; set; }
        public string Record { get; set; }
    }

    /// <summary>Concentrated (Start == End, Values in N/Nmm) or linear distributed (StartValues/EndValues in N/mm, Nmm/mm).
    /// Components Fx,Fy,Fz,Mx,My,Mz in CoordinateSystem; stations are fractions of the node-to-node length.</summary>
    public sealed class BeamLoadRecord
    {
        public string BeamId { get; set; }
        public string Case { get; set; }
        public double Start { get; set; }
        public double End { get; set; }
        public double[] Values { get; set; }
        public double[] StartValues { get; set; }
        public double[] EndValues { get; set; }
        public CoordinateSystem CoordinateSystem { get; set; }
        // Global unit normal of the projection plane for loads per projected length; null = per actual length.
        public Vector3d ProjectionPlaneNormal { get; set; }
        public Vector3d Eccentricity { get; set; }
        public CoordinateSystem EccentricityAxes { get; set; }
        public string Record { get; set; }
    }

    /// <summary>Uniform load on the whole shell in N/mm²: Normal = pressure along the element V3, otherwise traction (Px,Py,Pz) in CoordinateSystem.
    /// With Edge set, a uniform line load in N/mm on the edge from node Edge to the next node (0-based, connectivity order).
    /// With NodalPressures set, a pressure varying over the shell (one value per node, connectivity order) along V3 (Normal) or Direction.</summary>
    public sealed class ShellLoadRecord
    {
        public string ShellId { get; set; }
        public string Case { get; set; }
        public bool Normal { get; set; }
        public double Pressure { get; set; }
        public double[] Components { get; set; }
        public CoordinateSystem CoordinateSystem { get; set; }
        public int? Edge { get; set; }
        public double[] NodalPressures { get; set; }
        public Vector3d Direction { get; set; }
        public string Record { get; set; }
    }

    public enum CombinationKind { Linear, Envelope, Absolute, Srss }
    /// <summary>A term of a source combination: a static load case or another combination (by its Id), with its factor.
    /// Analysis keeps the source label (e.g. "ST", "CBC"); terms of other analyses make the combination not expandable.</summary>
    [System.Runtime.Serialization.DataContract]
    public sealed class CombinationTermRecord
    {
        [System.Runtime.Serialization.DataMember] public string Name { get; set; }
        [System.Runtime.Serialization.DataMember] public bool IsCombination { get; set; }
        [System.Runtime.Serialization.DataMember] public bool IsStaticCase { get; set; }
        [System.Runtime.Serialization.DataMember] public string Analysis { get; set; }
        [System.Runtime.Serialization.DataMember] public double Factor { get; set; }
    }
    /// <summary>A source load combination as defined in the solver: Id unique in the batch, Name as shown by the solver.</summary>
    [System.Runtime.Serialization.DataContract]
    public sealed class CombinationRecord
    {
        [System.Runtime.Serialization.DataMember] public string Id { get; set; }
        [System.Runtime.Serialization.DataMember] public string Name { get; set; }
        [System.Runtime.Serialization.DataMember] public CombinationKind Kind { get; set; }
        [System.Runtime.Serialization.DataMember] public string Status { get; set; }
        [System.Runtime.Serialization.DataMember] public string Source { get; set; }
        [System.Runtime.Serialization.DataMember] public List<CombinationTermRecord> Terms { get; set; } = new List<CombinationTermRecord>();
        [System.Runtime.Serialization.DataMember] public string Record { get; set; }
    }

    /// <summary>Self weight of the whole model in a case: Factors multiply the standard gravity along global X, Y, Z (e.g. 0, 0, -1).</summary>
    public sealed class GravityRecord
    {
        public string Case { get; set; }
        public Vector3d Factors { get; set; }
        public string Record { get; set; }
    }
    public sealed class GroupRecord
    {
        public string Name { get; set; }
        public string ParentName { get; set; }
        public string Record { get; set; }
        public List<SourceIdentity> Members { get; } = new List<SourceIdentity>();
    }
    public sealed class LoadCaseRecord
    {
        public string Name { get; set; }
        public string Record { get; set; }
    }
    public sealed class NodeLoadRecord
    {
        public string NodeId { get; set; }
        public string Case { get; set; }
        // Fx,Fy,Fz,Mx,My,Mz in N/Nmm, in the explicitly supplied coordinate system.
        public double[] Components { get; set; }
        public CoordinateSystem CoordinateSystem { get; set; }
        public string Record { get; set; }
    }
    public sealed class NodeRestrainRecord
    {
        public string NodeId { get; set; }
        public bool[] FixedDofs { get; set; } // DX,DY,DZ,RX,RY,RZ; permanent restraints only.
        public CoordinateSystem CoordinateSystem { get; set; }
        public string Record { get; set; }
    }
    public sealed class BeamReleaseRecord
    {
        public string BeamId { get; set; }
        public GPC.Model.Attributes.BeamReleasesAttribute Release { get; set; }
        public string Record { get; set; }
    }
    public sealed class NodeLinkRecord
    {
        public string I { get; set; }
        public string J { get; set; }
        public bool[] RigidDofs { get; set; } // Global DX,DY,DZ,RX,RY,RZ; rigid body lever arms included by ModelMapper.
        public SpringMatrix Spring { get; set; }
        public string Record { get; set; }
    }
    public sealed class ImportBatch
    {
        public string Program { get; set; }
        public string SolverVersion { get; set; }
        public string ModelRevision { get; set; }
        public string AnalysisId { get; set; }
        public string SourceHash { get; set; }
        public List<NodeRecord> Nodes { get; } = new List<NodeRecord>();
        public List<BeamRecord> Beams { get; } = new List<BeamRecord>();
        public List<ShellRecord> Shells { get; } = new List<ShellRecord>();
        public List<GroupRecord> Groups { get; } = new List<GroupRecord>();
        public List<LoadCaseRecord> LoadCases { get; } = new List<LoadCaseRecord>();
        public List<NodeLoadRecord> NodeLoads { get; } = new List<NodeLoadRecord>();
        public List<NodeRestrainRecord> NodeRestrains { get; } = new List<NodeRestrainRecord>();
        public List<BeamReleaseRecord> BeamReleases { get; } = new List<BeamReleaseRecord>();
        public List<NodeLinkRecord> NodeLinks { get; } = new List<NodeLinkRecord>();
        public List<MaterialRecord> Materials { get; } = new List<MaterialRecord>();
        public List<SectionRecord> Sections { get; } = new List<SectionRecord>();
        public List<ThicknessRecord> Thicknesses { get; } = new List<ThicknessRecord>();
        public List<BeamLoadRecord> BeamLoads { get; } = new List<BeamLoadRecord>();
        public List<ShellLoadRecord> ShellLoads { get; } = new List<ShellLoadRecord>();
        public List<GravityRecord> Gravity { get; } = new List<GravityRecord>();
        public List<CombinationRecord> Combinations { get; } = new List<CombinationRecord>();
        public List<ModelDiagnostic> Diagnostics { get; } = new List<ModelDiagnostic>();
        public List<PreservedAssignment> Uninterpreted { get; } = new List<PreservedAssignment>();
    }
    public sealed class ImportReport
    {
        public ImportStatus Status { get; internal set; }
        public GPC.Model.Models.Model Model { get; internal set; }
        public List<ModelDiagnostic> Diagnostics { get; } = new List<ModelDiagnostic>();
        public List<PreservedAssignment> Preserved { get; } = new List<PreservedAssignment>();
        public string SourceHash { get; internal set; }
        public bool VerificationEnabled => false; // These geometry adapters do not import a verified result dataset.
    }
    public interface IModelFileReader
    {
        string Program { get; }
        IReadOnlyDictionary<string, CapabilityStatus> Capabilities { get; }
        ImportBatch Read(Stream source, CancellationToken cancellationToken);
    }
    public static class SourceEvidence
    {
        public static string Sha256(Stream source)
        {
            using (var hash = SHA256.Create()) return BitConverter.ToString(hash.ComputeHash(source)).Replace("-", "");
        }
        public static bool SameAnalysis(ImportBatch geometry, ImportBatch results) =>
            !string.IsNullOrWhiteSpace(geometry.ModelRevision) && !string.IsNullOrWhiteSpace(geometry.AnalysisId)
            && geometry.Program == results.Program && geometry.SolverVersion == results.SolverVersion
            && geometry.ModelRevision == results.ModelRevision && geometry.AnalysisId == results.AnalysisId;
    }
}
