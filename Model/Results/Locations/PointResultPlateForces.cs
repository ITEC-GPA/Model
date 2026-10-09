using System;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results.ResultLocations
{
    [Serializable]
    public sealed class PointResultPlateForces : ResultLocation, IPlateResultLocation
    {
        public Point2d Location { get; private set; }
        public string LocationKind { get; set; }
        public PostProcessing.ShellResultPointKind PointKind { get; set; }
        public PostProcessing.ResultCoordinateKind CoordinateKind { get; set; }
        public Point3d GlobalLocation { get; set; }
        public int? SourceNodeId { get; set; }
        public string AveragingRegion { get; set; }
        public ResultPlateForces Forces => (ResultPlateForces)ResultTypes;
        public PointResultPlateForces(ILoadCase loadCase, ResultPlateForces result, Point2d location, string locationKind)
            : base(loadCase, result, IDUNASSIGNED, "") { Location = location; LocationKind = locationKind; }
        private PointResultPlateForces(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Location = (Point2d)info.GetValue("Location", typeof(Point2d)); LocationKind = info.GetString("LocationKind");
            PointKind = SerializationFields.Read<PostProcessing.ShellResultPointKind>(info, "PointKind");
            CoordinateKind = SerializationFields.Read<PostProcessing.ResultCoordinateKind>(info, "CoordinateKind");
            GlobalLocation = SerializationFields.Read<Point3d>(info, "GlobalLocation"); SourceNodeId = SerializationFields.Read<int?>(info, "SourceNodeId");
            AveragingRegion = SerializationFields.Read<string>(info, "AveragingRegion");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context); info.AddValue("Location", Location); info.AddValue("LocationKind", LocationKind);
            info.AddValue("PointKind", PointKind); info.AddValue("CoordinateKind", CoordinateKind); info.AddValue("GlobalLocation", GlobalLocation);
            info.AddValue("SourceNodeId", SourceNodeId); info.AddValue("AveragingRegion", AveragingRegion);
        }
        public override bool Equals(object obj) => obj is PointResultPlateForces other && base.Equals(obj) && Equals(Location, other.Location) && LocationKind == other.LocationKind
            && PointKind == other.PointKind && CoordinateKind == other.CoordinateKind && Equals(GlobalLocation, other.GlobalLocation) && SourceNodeId == other.SourceNodeId && AveragingRegion == other.AveragingRegion;
        public override int GetHashCode() => base.GetHashCode();
    }
}
