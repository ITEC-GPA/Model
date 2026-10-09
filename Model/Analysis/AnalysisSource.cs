using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.Analysis
{
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class AnalysisSource
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Program>k__BackingField", IsRequired = true)]
        public string Program { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SolverVersion>k__BackingField", IsRequired = true)]
        public string SolverVersion { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ModelRevision>k__BackingField", IsRequired = true)]
        public string ModelRevision { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<AnalysisId>k__BackingField", IsRequired = true)]
        public string AnalysisId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<GeometryHash>k__BackingField", IsRequired = true)]
        public string GeometryHash { get; set; }
    }
}
