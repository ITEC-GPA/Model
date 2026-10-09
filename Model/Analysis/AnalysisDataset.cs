using System;
using System.Collections.Generic;

namespace GPC.Model.PostProcessing
{
    [Serializable]
    public sealed class AnalysisDataset
    {
        public string Id { get; set; }
        public string Program { get; set; }
        public string SolverVersion { get; set; }
        public string ModelRevision { get; set; }
        public string AnalysisId { get; set; }
        public string InputFingerprint { get; set; }
        [field: System.Runtime.Serialization.OptionalField, GPC.Model.Core.FingerprintWhenSet]
        public string AnalysisSnapshotId { get; set; }
        public string NormalizedUnits { get; set; }
        public AnalysisSemantics Semantics { get; set; }
        public bool IsSynthetic { get; set; }
        public Dictionary<string, string> SourceHashes { get; private set; } = new Dictionary<string, string>();
    }
}
