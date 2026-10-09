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
    public sealed class ShellRebarLayer
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<PhysicalFace>k__BackingField", IsRequired = true)]
        public string PhysicalFace { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Steel>k__BackingField", IsRequired = true)]
        public SteelMaterial Steel { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Diameter>k__BackingField", IsRequired = true)]
        public double Diameter { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Pitch>k__BackingField", IsRequired = true)]
        public double Pitch { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<AxisPositionThroughThickness>k__BackingField", IsRequired = true)]
        public double AxisPositionThroughThickness { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<DirectionRadians>k__BackingField", IsRequired = true)]
        public double DirectionRadians { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Order>k__BackingField", IsRequired = true)]
        public int Order { get; set; }
    }
}
