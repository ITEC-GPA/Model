using GPC.Geometry;
using GPC.Model.Results.ResultLocations;
using GPC.Model.Results.Processing;

namespace GPC.Model.PostProcessing
{
    /// <summary>Compatibility entry points. Physical transformations are implemented by ActionTransformations.</summary>
    public static class ResultTransformations
    {
        public static CoordinateSystem AtPoint(CoordinateSystem orientation, Point3d point) => ActionTransformations.AtPoint(orientation, point);
        public static StationResultBeamForces RotateBeam(StationResultBeamForces source, CoordinateSystem target) => ActionTransformations.RotateBeam(source, target);
        public static StationResultBeamForces TransportBeam(StationResultBeamForces source, CoordinateSystem target) => ActionTransformations.TransportBeam(source, target);
        public static NodeResultForces RotateNode(NodeResultForces source, CoordinateSystem target) => ActionTransformations.RotateNode(source, target);
        public static NodeResultForces TransportNode(NodeResultForces source, CoordinateSystem target) => ActionTransformations.TransportNode(source, target);
        public static NodeResultDisplacement RotateNode(NodeResultDisplacement source, CoordinateSystem target) => ActionTransformations.RotateNode(source, target);
        public static PointResultPlateForces RotateShell(PointResultPlateForces source, CoordinateSystem target) => ActionTransformations.RotateShell(source, target);
    }
}