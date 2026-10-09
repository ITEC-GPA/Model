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
    public sealed class ShellAssignments
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<PhysicalThickness>k__BackingField", IsRequired = true)]
        /// <summary>Legacy storage; new models put thickness in PlateProperty.</summary>
        public double? PhysicalThickness { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Offset>k__BackingField", IsRequired = true)]
        public double? Offset { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<LayerAxes>k__BackingField", IsRequired = true)]
        public CoordinateSystem LayerAxes { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Layers>k__BackingField", IsRequired = true)]
        public List<ShellRebarLayer> Layers { get; private set; } = new List<ShellRebarLayer>();

        [field: System.Runtime.Serialization.DataMember(Name = "<ReinforcementZone>k__BackingField", IsRequired = true)]
        public string ReinforcementZone { get; set; }
    }
}
