using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.PostProcessing
{

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
}
