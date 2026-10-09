using System;
using GPC.Geometry;
using GPC.Model.Loads;

namespace GPC.Model.PostProcessing
{
    public enum BeamLoadLengthConvention { Unknown, ActualLength, ProjectedLength }

    /// <summary>Owned by one beam. Reuses PointLoad and LineLoad components and case; linear distribution is explicit.</summary>
    [Serializable]
    public sealed class BeamLoadAssignment
    {
        public double Start { get; set; }
        public double End { get; set; }
        public PointLoad Concentrated { get; set; }
        public LineLoad StartIntensity { get; set; }
        public LineLoad EndIntensity { get; set; }
        public Vector3d Eccentricity { get; set; }
        public CoordinateSystem EccentricityCoordinateSystem { get; set; }
        public BeamLoadLengthConvention LengthConvention { get; set; }
        [field: System.Runtime.Serialization.OptionalField] public string StationDomain { get; set; } = "NodeToNode";
        /// <summary>Global unit normal to the projection plane, required for ProjectedLength.</summary>
        [field: System.Runtime.Serialization.OptionalField] public Vector3d ProjectionPlaneNormal { get; set; }
        public bool IsEquivalentNodalRepresentation { get; set; }
        public string OriginalAssignmentId { get; set; }
        public string SourceRecord { get; set; }
    }
}
