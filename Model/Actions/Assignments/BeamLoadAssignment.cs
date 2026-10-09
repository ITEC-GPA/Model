using System;
using GPC.Geometry;
using GPC.Model.Loads;

namespace GPC.Model.Loads.Assignments
{
    public enum BeamLoadLengthConvention
    {
        Unknown,
        ActualLength,
        ProjectedLength
    }

    /// <summary>Owned by one beam. Reuses PointLoad and LineLoad components and case; linear distribution is explicit.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class BeamLoadAssignment
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<Start>k__BackingField", IsRequired = true)]
        public double Start { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<End>k__BackingField", IsRequired = true)]
        public double End { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Concentrated>k__BackingField", IsRequired = true)]
        public PointLoad Concentrated { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<StartIntensity>k__BackingField", IsRequired = true)]
        public LineLoad StartIntensity { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<EndIntensity>k__BackingField", IsRequired = true)]
        public LineLoad EndIntensity { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<Eccentricity>k__BackingField", IsRequired = true)]
        public Vector3d Eccentricity { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<EccentricityCoordinateSystem>k__BackingField", IsRequired = true)]
        public CoordinateSystem EccentricityCoordinateSystem { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<LengthConvention>k__BackingField", IsRequired = true)]
        public BeamLoadLengthConvention LengthConvention { get; set; }

        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<StationDomain>k__BackingField", IsRequired = false)]
        public string StationDomain { get; set; } = "NodeToNode";

        /// <summary>Global unit normal to the projection plane, required for ProjectedLength.</summary>
        [field: System.Runtime.Serialization.OptionalField]
        [field: System.Runtime.Serialization.DataMember(Name = "<ProjectionPlaneNormal>k__BackingField", IsRequired = false)]
        public Vector3d ProjectionPlaneNormal { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<IsEquivalentNodalRepresentation>k__BackingField", IsRequired = true)]
        public bool IsEquivalentNodalRepresentation { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<OriginalAssignmentId>k__BackingField", IsRequired = true)]
        public string OriginalAssignmentId { get; set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<SourceRecord>k__BackingField", IsRequired = true)]
        public string SourceRecord { get; set; }
    }
}
