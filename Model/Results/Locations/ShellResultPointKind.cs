using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.Results.Locations
{
    [System.Runtime.Serialization.DataContract(Name = "ShellResultPointKind", Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public enum ShellResultPointKind { [System.Runtime.Serialization.EnumMember] Unknown, [System.Runtime.Serialization.EnumMember] Centroid, [System.Runtime.Serialization.EnumMember] IntegrationPoint, [System.Runtime.Serialization.EnumMember] ElementNodeExtrapolated, [System.Runtime.Serialization.EnumMember] Averaged }
}
