using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.Results.Locations
{
    [System.Runtime.Serialization.DataContract(Name = "SectionSide", Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public enum SectionSide { [System.Runtime.Serialization.EnumMember] Unspecified, [System.Runtime.Serialization.EnumMember] Left, [System.Runtime.Serialization.EnumMember] Right }
}
