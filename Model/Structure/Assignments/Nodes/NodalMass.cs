using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Restraints;
using GPC.Model.Sections.Concrete;
using GPC.Model.Materials;
using GPC.Model.Core.Coordinates;

namespace GPC.Model.Structure.Assignments
{
    /// <summary>Mass in kg; rotational inertia in kg mm^2; cross terms kg mm. No implicit conversion to weight.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class NodalMass
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Axes>k__BackingField", IsRequired = true)]
        public CoordinateSystem Axes { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Matrix6x6RowMajor>k__BackingField", IsRequired = true)]
        public double[] Matrix6x6RowMajor { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SourceRecord>k__BackingField", IsRequired = true)]
        public string SourceRecord { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<IncludedInSourceTotal>k__BackingField", IsRequired = true)]
        public bool? IncludedInSourceTotal { get; set; }
    }
}
