using System;
using System.Collections.Generic;

namespace GPC.Model.Analysis
{
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class AnalysisDataset
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Id>k__BackingField", IsRequired = true)]
        public string Id { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Program>k__BackingField", IsRequired = true)]
        public string Program { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SolverVersion>k__BackingField", IsRequired = true)]
        public string SolverVersion { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ModelRevision>k__BackingField", IsRequired = true)]
        public string ModelRevision { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<AnalysisId>k__BackingField", IsRequired = true)]
        public string AnalysisId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<InputFingerprint>k__BackingField", IsRequired = true)]
        public string InputFingerprint { get; set; }

        [field: System.Runtime.Serialization.OptionalField, GPC.Model.Core.FingerprintWhenSet]
        [field: System.Runtime.Serialization.DataMember(Name = "<AnalysisSnapshotId>k__BackingField", IsRequired = false)]
        public string AnalysisSnapshotId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<NormalizedUnits>k__BackingField", IsRequired = true)]
        public string NormalizedUnits { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Semantics>k__BackingField", IsRequired = true)]
        public AnalysisSemantics Semantics { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<IsSynthetic>k__BackingField", IsRequired = true)]
        public bool IsSynthetic { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SourceHashes>k__BackingField", IsRequired = true)]
        public Dictionary<string, string> SourceHashes { get; private set; } = new Dictionary<string, string>();
    }
}
