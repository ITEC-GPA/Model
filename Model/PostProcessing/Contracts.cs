using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.PostProcessing
{
    public enum EntityFamily { Node, Beam, Shell, Solid, Link }
    public enum DiagnosticSeverity { Information, Warning, Error }
    public enum SectionSide { Unspecified, Left, Right }
    public enum AnalysisSemantics { Unknown, LinearStatic, NonlinearStatic, ConcomitantEnvelopeState, IndependentExtrema, Modal, ResponseSpectrum }
    public enum ComponentAvailability { Available, Missing, NotExported, NotApplicable }
    public enum ShellResultPointKind { Unknown, Centroid, IntegrationPoint, ElementNodeExtrapolated, Averaged }
    public enum ResultCoordinateKind { Unknown, Natural, LocalPhysical, Global }
    public enum ExecutionStatus { NotExecuted, Completed, Cancelled, Error }
    public enum DataStatus { Ready, Insufficient, NotSupported, MissingDependency, Stale }
    public enum EngineeringOutcome { NotEvaluated, Satisfied, NotSatisfied }

    [Serializable]
    public sealed class SourceIdentity : IEquatable<SourceIdentity>
    {
        public string Program { get; private set; }
        public string ModelRevision { get; private set; }
        public EntityFamily Family { get; private set; }
        public string OriginalId { get; private set; }
        public SourceIdentity(string program, string modelRevision, EntityFamily family, string originalId)
        {
            if (string.IsNullOrWhiteSpace(program) || string.IsNullOrWhiteSpace(modelRevision) || string.IsNullOrWhiteSpace(originalId))
                throw new ArgumentException("Source program, model/revision and original ID are required.");
            Program = program; ModelRevision = modelRevision; Family = family; OriginalId = originalId;
        }
        public bool Equals(SourceIdentity other) => other != null && Program == other.Program && ModelRevision == other.ModelRevision && Family == other.Family && OriginalId == other.OriginalId;
        public override bool Equals(object obj) => Equals(obj as SourceIdentity);
        public override int GetHashCode() => Program.GetHashCode() ^ ModelRevision.GetHashCode() ^ (int)Family ^ OriginalId.GetHashCode();
    }

    [Serializable]
    public sealed class ModelDiagnostic
    {
        public string Code { get; set; }
        public DiagnosticSeverity Severity { get; set; }
        public int? ElementId { get; set; }
        public EntityFamily? Family { get; set; }
        public double? Station { get; set; }
        public string Dataset { get; set; }
        public string Case { get; set; }
        public string Record { get; set; }
        public string Message { get; set; }
        public string SuggestedAction { get; set; }
        public static ModelDiagnostic Error(string code, Element element = null, string message = null) => new ModelDiagnostic
        {
            Code = code,
            Severity = DiagnosticSeverity.Error,
            ElementId = element?.Id,
            Family = element is NodeElement ? EntityFamily.Node : element is BeamElement ? EntityFamily.Beam : element is AreaElement ? EntityFamily.Shell : element is VolumeElement ? EntityFamily.Solid : (EntityFamily?)null,
            Message = message ?? code,
            SuggestedAction = "Supply or correct the source assignment explicitly."
        };
    }

    /// <summary>Shared analysis state. Null component flags mean unknown completeness, never six zeros.</summary>
    [Serializable]
    public sealed class ResultState
    {
        public string DatasetId { get; set; }
        public string ModelRevision { get; set; }
        public string InputFingerprint { get; set; }
        public string Phase { get; set; }
        public string Step { get; set; }
        public string MovingLoadPosition { get; set; }
        public bool? IsCumulative { get; set; }
        public AnalysisSemantics Semantics { get; set; }
        public string ConcomitantStateId { get; set; }
        public string Normalization { get; set; }
        public int? Mode { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public string HistoryId { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public int? IncrementIndex { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public bool? IncrementsStartAtZero { get; set; }
        public ComponentAvailability[] Components { get; set; }
        public bool IsSynthetic { get; set; }
        public bool? IsCombined { get; set; }
        public string SourceRecord { get; set; }
        public string SourceHash { get; set; }
        public string Transformation { get; set; }
        public string Coverage { get; set; }
        public OriginalResultData Original { get; set; }
        /// <summary>Authoritative source samples for a derived result; retained as shared references by ModelArchive.</summary>
        [field: System.Runtime.Serialization.OptionalField] public Results.ResultLocations.ResultLocation[] DerivedFrom { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public string DerivedSourceFingerprint { get; set; }

        public ResultState Copy()
        {
            var copy = (ResultState)MemberwiseClone();
            copy.Components = Components == null ? null : (ComponentAvailability[])Components.Clone();
            copy.Original = Original?.Copy();
            copy.DerivedFrom = DerivedFrom == null ? null : (Results.ResultLocations.ResultLocation[])DerivedFrom.Clone();
            return copy;
        }
    }

    [Serializable]
    public sealed class OriginalResultData
    {
        public double?[] Values { get; set; }
        public GPC.Geometry.CoordinateSystem Axes { get; set; }
        public string ComponentOrder { get; set; }
        public string Convention { get; set; }
        public string ReaderVersion { get; set; }
        public double ForceToN { get; set; }
        public double LengthToMm { get; set; }
        public double MomentToNmm { get; set; }
        public double DenominatorLengthToMm { get; set; }
        public double AngleToRad { get; set; }
        public OriginalResultData Copy()
        {
            var copy = (OriginalResultData)MemberwiseClone();
            copy.Values = Values == null ? null : (double?[])Values.Clone();
            copy.Axes = Axes == null ? null : ResultTransformations.AtPoint(Axes, Axes.Origin);
            return copy;
        }
    }

    [Serializable]
    public sealed class AnalysisSource
    {
        public string Program { get; set; }
        public string SolverVersion { get; set; }
        public string ModelRevision { get; set; }
        public string AnalysisId { get; set; }
        public string GeometryHash { get; set; }
    }
}
