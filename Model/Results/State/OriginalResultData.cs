using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;
using GPC.Model.Core.Coordinates;

namespace GPC.Model.Results.State
{
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class OriginalResultData
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Values>k__BackingField", IsRequired = true)]
        public double? [] Values { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Axes>k__BackingField", IsRequired = true)]
        public GPC.Geometry.CoordinateSystem Axes { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ComponentOrder>k__BackingField", IsRequired = true)]
        public string ComponentOrder { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Convention>k__BackingField", IsRequired = true)]
        public string Convention { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ReaderVersion>k__BackingField", IsRequired = true)]
        public string ReaderVersion { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ForceToN>k__BackingField", IsRequired = true)]
        public double ForceToN { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<LengthToMm>k__BackingField", IsRequired = true)]
        public double LengthToMm { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<MomentToNmm>k__BackingField", IsRequired = true)]
        public double MomentToNmm { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<DenominatorLengthToMm>k__BackingField", IsRequired = true)]
        public double DenominatorLengthToMm { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<AngleToRad>k__BackingField", IsRequired = true)]
        public double AngleToRad { get; set; }

        public OriginalResultData Copy()
        {
            var copy = (OriginalResultData)MemberwiseClone();
            copy.Values = Values == null ? null : (double? [])Values.Clone();
            copy.Axes = Axes == null ? null : ActionTransformations.AtPoint(Axes, Axes.Origin);
            return copy;
        }
    }
}
