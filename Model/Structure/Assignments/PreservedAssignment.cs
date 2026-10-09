using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restraints;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;

namespace GPC.Model.Structure.Assignments
{
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class PreservedAssignment
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Kind>k__BackingField", IsRequired = true)]
        public string Kind { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SourceRecord>k__BackingField", IsRequired = true)]
        public string SourceRecord { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<RawData>k__BackingField", IsRequired = true)]
        public string RawData { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<UnitsAndAxes>k__BackingField", IsRequired = true)]
        public string UnitsAndAxes { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<UnsupportedReason>k__BackingField", IsRequired = true)]
        public string UnsupportedReason { get; set; }
    }
}
