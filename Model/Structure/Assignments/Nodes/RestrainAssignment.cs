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
    public sealed class RestrainAssignment
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Restrain>k__BackingField", IsRequired = true)]
        public NodeRestrain Restrain { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Case>k__BackingField", IsRequired = true)]
        public ILoadCase Case { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Phase>k__BackingField", IsRequired = true)]
        public string Phase { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SourceRecord>k__BackingField", IsRequired = true)]
        public string SourceRecord { get; set; }
    }
}
