using System;
using System.Runtime.Serialization;

namespace GPC.Model.LoadCases
{
    /// <summary>Dimensionless inclusion flags for acceleration loads in a load case.
    /// Structural mass is derived from density and geometry; non-structural mass is explicitly assigned.</summary>
    [Serializable,DataContract]
    public sealed class InertiaMassSelection
    {
        [field:DataMember(Order=0,IsRequired=true)]public bool Structural {get;set;}
        [field:DataMember(Order=1,IsRequired=true)]public bool NonStructural {get;set;}
    }
}
