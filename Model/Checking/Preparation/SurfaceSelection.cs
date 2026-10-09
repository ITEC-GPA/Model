using System;

namespace GPC.Model.Checking.Preparation
{
    [Serializable]
    [System.Runtime.Serialization.DataContract(Name = "SurfaceSelection", Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.Checking")]
    public sealed class SurfaceSelection
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<SurfaceId>k__BackingField", IsRequired = true)]
        public string SurfaceId { get; set; }
        /// <summary>Null selects the whole surface; otherwise select an explicit zone.</summary>
        [field: System.Runtime.Serialization.DataMember(Name = "<ZoneId>k__BackingField", IsRequired = true)]
        public string ZoneId { get; set; }
    }
}
