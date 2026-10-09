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
    public sealed class NodalLink
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<OtherNodeId>k__BackingField", IsRequired = true)]
        public int OtherNodeId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Spring>k__BackingField", IsRequired = true)]
        public SpringMatrix Spring { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<KinematicCoefficients>k__BackingField", IsRequired = true)]
        // Rows express C_i u_i + C_j u_j = rhs. This is not an artificial stiff beam.
        public double[] KinematicCoefficients { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<RightHandSide>k__BackingField", IsRequired = true)]
        public double[] RightHandSide { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<LeverArm>k__BackingField", IsRequired = true)]
        public Vector3d LeverArm { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SourceRecord>k__BackingField", IsRequired = true)]
        public string SourceRecord { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<UnsupportedLaw>k__BackingField", IsRequired = true)]
        public string UnsupportedLaw { get; set; }
    }
}
