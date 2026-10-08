using System.Collections.Generic;
using System.IO;
using System.Threading;
using GPC.Geometry;
using GPC.Model.PostProcessing;

namespace GPC.Converter
{
    // Readers resolve solver labels, permutations and sign conventions. The mapper never guesses them.
    public abstract class ResultRecord
    {
        public string ElementId { get; set; }
        public string Case { get; set; }
        public ResultState State { get; set; }
        public CoordinateSystem Axes { get; set; }
        public double?[] Values { get; set; }
        public string Record { get; set; }
    }
    public abstract class ForceRecord : ResultRecord { }
    public sealed class NodeForceRecord : ForceRecord
    {
        // Values: Fx,Fy,Fz,Mx,My,Mz about Axes.Origin, which must coincide with the node.
        public NodalForceKind Kind { get; set; }
        public ActionBody Body { get; set; }
        public SourceIdentity OwnerSource { get; set; }
        public string ElementEnd { get; set; }
        public string AggregationSet { get; set; }
    }
    public sealed class NodeDisplacementRecord : ResultRecord
    {
        // Values: Dx,Dy,Dz,Rx,Ry,Rz; translations and rotations, never section actions.
    }
    public sealed class BeamForceRecord : ForceRecord
    {
        // Values: N, V1, V2, T, M1, M2 in the supplied CoordinateSystem (V3 longitudinal).
        public double Station { get; set; }
        public double? PhysicalDistance { get; set; }
        public SectionSide Side { get; set; }
        public string StationDomain { get; set; }
        public ActionBody Body { get; set; }
    }
    public sealed class ShellForceRecord : ForceRecord
    {
        // Values: Fxx, Fyy, Fxy, Fxz, Fyz, Mxx, Myy, Mxy.
        public Point2d Location { get; set; }
        public ShellResultPointKind PointKind { get; set; }
        public ResultCoordinateKind CoordinateKind { get; set; }
        public string SourceNodeId { get; set; }
        public string AveragingRegion { get; set; }
    }
    public sealed class ResultImportBatch
    {
        public AnalysisSource Source { get; set; }
        public string DatasetId { get; set; }
        public string SourceHash { get; set; }
        // Explicit reader/caller binding to the fully assigned imported model, not inferred from file hashes.
        public string ExpectedInputFingerprint { get; set; }
        /// <summary>Original analysed inputs from the solver binding; separate from the current target mutation guard.</summary>
        public string AnalysisInputFingerprint { get; set; }
        public ResultUnits Units { get; set; }
        public double ShellDenominatorLengthToMm { get; set; }
        public string ResolvedConvention { get; set; }
        public string ReaderVersion { get; set; }
        public AnalysisSemantics Semantics { get; set; }
        public bool IsSynthetic { get; set; }
        public List<BeamForceRecord> Beams { get; } = new List<BeamForceRecord>();
        public List<ShellForceRecord> Shells { get; } = new List<ShellForceRecord>();
        public List<NodeForceRecord> NodeForces { get; } = new List<NodeForceRecord>();
        public List<NodeDisplacementRecord> NodeDisplacements { get; } = new List<NodeDisplacementRecord>();
    }
    public sealed class ResultImportReport
    {
        public ImportStatus Status { get; internal set; }
        public int ImportedSamples { get; internal set; }
        public List<ModelDiagnostic> Diagnostics { get; } = new List<ModelDiagnostic>();
    }
    public interface IResultFileReader
    {
        string Program { get; }
        IReadOnlyDictionary<string, CapabilityStatus> Capabilities { get; }
        ResultImportBatch Read(Stream source, CancellationToken cancellationToken);
    }
}
