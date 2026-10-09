using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.Results.Locations
{
    [System.Runtime.Serialization.DataContract(Name = "ResultCoordinateKind", Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public enum ResultCoordinateKind { [System.Runtime.Serialization.EnumMember] Unknown, [System.Runtime.Serialization.EnumMember] Natural, [System.Runtime.Serialization.EnumMember] LocalPhysical, [System.Runtime.Serialization.EnumMember] Global }
}
