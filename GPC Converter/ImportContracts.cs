using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using GPC.Geometry;
using GPC.Model.ElementProperties;
using GPC.Model.Models;
using GPC.Model.PostProcessing;

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
        public string Record { get; set; }
        public List<PreservedAssignment> OtherAssignments { get; } = new List<PreservedAssignment>();
    }
    public sealed class ShellRecord
    {
        public string Id { get; set; }
        public string[] Nodes { get; set; }
        public CoordinateSystem CoordinateSystem { get; set; }
        public PlateProperty Property { get; set; }
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
