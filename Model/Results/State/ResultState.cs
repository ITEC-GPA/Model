using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;
using GPC.Model.Analysis;
using GPC.Model.Results.Locations;

namespace GPC.Model.Results.State
{
    /// <summary>Shared analysis state. Null component flags mean unknown completeness, never six zeros.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class ResultState
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<DatasetId>k__BackingField", IsRequired = true)]
        public string DatasetId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ModelRevision>k__BackingField", IsRequired = true)]
        public string ModelRevision { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<InputFingerprint>k__BackingField", IsRequired = true)]
        public string InputFingerprint { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Phase>k__BackingField", IsRequired = true)]
        public string Phase { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Step>k__BackingField", IsRequired = true)]
        public string Step { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<MovingLoadPosition>k__BackingField", IsRequired = true)]
        public string MovingLoadPosition { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<IsCumulative>k__BackingField", IsRequired = true)]
        public bool? IsCumulative { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Semantics>k__BackingField", IsRequired = true)]
        public AnalysisSemantics Semantics { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ConcomitantStateId>k__BackingField", IsRequired = true)]
        public string ConcomitantStateId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Normalization>k__BackingField", IsRequired = true)]
        public string Normalization { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Mode>k__BackingField", IsRequired = true)]
        public int? Mode { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<HistoryId>k__BackingField", IsRequired = false)]
        public string HistoryId { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<IncrementIndex>k__BackingField", IsRequired = false)]
        public int? IncrementIndex { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<IncrementsStartAtZero>k__BackingField", IsRequired = false)]
        public bool? IncrementsStartAtZero { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Components>k__BackingField", IsRequired = true)]
        public ComponentAvailability[] Components { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<IsSynthetic>k__BackingField", IsRequired = true)]
        public bool IsSynthetic { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<IsCombined>k__BackingField", IsRequired = true)]
        public bool? IsCombined { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SourceRecord>k__BackingField", IsRequired = true)]
        public string SourceRecord { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SourceHash>k__BackingField", IsRequired = true)]
        public string SourceHash { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Transformation>k__BackingField", IsRequired = true)]
        public string Transformation { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Coverage>k__BackingField", IsRequired = true)]
        public string Coverage { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Original>k__BackingField", IsRequired = true)]
        public OriginalResultData Original { get; set; }

        /// <summary>Authoritative source samples for a derived result; retained as shared references by ModelValues.</summary>
        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<DerivedFrom>k__BackingField", IsRequired = false)]
        public global::GPC.Model.Results.Locations.ResultLocation[] DerivedFrom { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<DerivedSourceFingerprint>k__BackingField", IsRequired = false)]
        public string DerivedSourceFingerprint { get; set; }

        public ResultState Copy()
        {
            var copy = (ResultState)MemberwiseClone();
            copy.Components = Components == null ? null : (ComponentAvailability[])Components.Clone();
            copy.Original = Original?.Copy();
            copy.DerivedFrom = DerivedFrom == null ? null : (global::GPC.Model.Results.Locations.ResultLocation[])DerivedFrom.Clone();
            return copy;
        }
    }
}
