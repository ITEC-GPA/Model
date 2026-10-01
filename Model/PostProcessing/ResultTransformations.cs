using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Results;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    /// <summary>Transform complete canonical samples without changing the source. No connectivity or face reversal is inferred.</summary>
    public static class ResultTransformations
    {
        public static CoordinateSystem AtPoint(CoordinateSystem orientation, Point3d point)
        {
            Axes.Validate(orientation);
            if (!Axes.IsFinite(point)) throw new ArgumentException("Invalid reduction point.");
            return new CoordinateSystem(new Point3d(point.X, point.Y, point.Z),
                new Vector3d(orientation.V1.X, orientation.V1.Y, orientation.V1.Z),
                new Vector3d(orientation.V2.X, orientation.V2.Y, orientation.V2.Z),
                new Vector3d(orientation.V3.X, orientation.V3.Y, orientation.V3.Z));
        }

        public static StationResultBeamForces RotateBeam(StationResultBeamForces source, CoordinateSystem target)
            => Beam(source, target, false);

        public static NodeResultForces RotateNode(NodeResultForces source, CoordinateSystem target) => Node(source, target, false);
        public static NodeResultForces TransportNode(NodeResultForces source, CoordinateSystem target) => Node(source, target, true);

        private static NodeResultForces Node(NodeResultForces source, CoordinateSystem target, bool transport)
        {
            if (source?.ResultBeamForces == null) throw new ArgumentNullException(nameof(source));
            Complete(source.State, 6); var f = source.ResultBeamForces;
            Axes.Validate(f.CoordinateSystem); Axes.Validate(target); Finite(source.Fx, source.Fy, source.Fz, source.Mx, source.My, source.Mz);
            if (!transport && Axes.Length(f.CoordinateSystem.Origin - target.Origin) > 1e-8)
                throw new NotSupportedException("RotationRequiresSamePoint: use explicit TransportNode for moment transport.");
            var axes = AtPoint(target, target.Origin);
            var result = transport ? f.ToCoordinateSystemWithEccentricity(axes) : f.ToCoordinateSystem(axes);
            Finite(result.N, result.V1, result.V2, result.T, result.M1, result.M2);
            return new NodeResultForces(source.Case, result, source.Id, source.Name)
            {
                Kind = source.Kind, Body = source.Body, OwnerElementFamily = source.OwnerElementFamily, OwnerElementId = source.OwnerElementId,
                ElementEnd = source.ElementEnd, AggregationSet = source.AggregationSet,
                State = Transformed(source.State, transport ? "Nodal wrench rotation and explicit moment transport" : "Nodal force/moment rotation at unchanged point")
            };
        }

        public static NodeResultDisplacement RotateNode(NodeResultDisplacement source, CoordinateSystem target)
        {
            if (source?.ResultDisplacement == null) throw new ArgumentNullException(nameof(source));
            Complete(source.State, 6); var d = source.ResultDisplacement;
            Axes.Validate(d.CoordinateSystem); Axes.Validate(target); Finite(d.D1, d.D2, d.D3, d.R1, d.R2, d.R3);
            if (Axes.Length(d.CoordinateSystem.Origin - target.Origin) > 1e-8) throw new NotSupportedException("NodalDisplacementRequiresSamePoint");
            var rotated = d.ToCoordinateSystem(AtPoint(target, target.Origin));
            Finite(rotated.D1, rotated.D2, rotated.D3, rotated.R1, rotated.R2, rotated.R3);
            return new NodeResultDisplacement(source.Case, rotated, source.Id, source.Name)
            { State = Transformed(source.State, "Nodal translation/rotation vectors rotated at unchanged node") };
        }

        /// <summary>Explicit moment transport M' = M + (O - O') cross F, followed by rotation.</summary>
        public static StationResultBeamForces TransportBeam(StationResultBeamForces source, CoordinateSystem target)
            => Beam(source, target, true);

        private static StationResultBeamForces Beam(StationResultBeamForces source, CoordinateSystem target, bool transport)
        {
            if (source == null || source.ResultBeamForces == null) throw new ArgumentNullException(nameof(source));
            Complete(source.State, 6); Axes.Validate(source.ResultBeamForces.CoordinateSystem); Axes.Validate(target);
            if (!transport && Axes.Length(source.ResultBeamForces.CoordinateSystem.Origin - target.Origin) > 1e-8)
                throw new NotSupportedException("RotationRequiresSamePoint: use explicit TransportBeam for moment transport.");
            var f = source.ResultBeamForces;
            Finite(f.N, f.V1, f.V2, f.T, f.M1, f.M2);
            var axes = AtPoint(target, target.Origin);
            var forces = transport ? f.ToCoordinateSystemWithEccentricity(axes) : f.ToCoordinateSystem(axes);
            Finite(forces.N, forces.V1, forces.V2, forces.T, forces.M1, forces.M2);
            return new StationResultBeamForces(source.Case, forces, source.ParametricDistance, source.Id, source.Name)
            {
                Body = source.Body, Side = source.Side, PhysicalDistance = source.PhysicalDistance, StationDomain = source.StationDomain,
                State = Transformed(source.State, transport ? "Beam rotation and explicit moment transport" : "Beam rotation at unchanged reduction point")
            };
        }

        public static PointResultPlateForces RotateShell(PointResultPlateForces source, CoordinateSystem target)
        {
            if (source == null || source.Forces == null) throw new ArgumentNullException(nameof(source));
            Complete(source.State, 8); var f = source.Forces;
            Finite(f.Fxx, f.Fyy, f.Fxy, f.Fxz, f.Fyz, f.Mxx, f.Myy, f.Mxy);
            var forces = f.ToCoordinateSystem(AtPoint(target, target.Origin));
            Finite(forces.Fxx, forces.Fyy, forces.Fxy, forces.Fxz, forces.Fyz, forces.Mxx, forces.Myy, forces.Mxy);
            var location = source.Location == null ? null : new Point2d(source.Location.X, source.Location.Y);
            if (location != null && source.CoordinateKind == ResultCoordinateKind.LocalPhysical)
            {
                Finite(location.X, location.Y);
                var delta = f.CoordinateSystem.V1 * location.X + f.CoordinateSystem.V2 * location.Y;
                location = new Point2d(Axes.Dot(delta, target.V1), Axes.Dot(delta, target.V2));
            }
            return new PointResultPlateForces(source.Case, forces, location, source.LocationKind)
            {
                PointKind = source.PointKind, CoordinateKind = source.CoordinateKind,
                GlobalLocation = source.GlobalLocation == null ? null : new Point3d(source.GlobalLocation.X, source.GlobalLocation.Y, source.GlobalLocation.Z),
                SourceNodeId = source.SourceNodeId, AveragingRegion = source.AveragingRegion,
                State = Transformed(source.State, "Shell tensor and transverse shear rotation; unchanged point and normal")
            };
        }
        private static ResultState Transformed(ResultState state, string operation)
        {
            var result = state.Copy();
            result.Transformation = string.IsNullOrEmpty(state.Transformation) ? operation : state.Transformation + "; " + operation;
            return result;
        }
        private static void Complete(ResultState state, int count)
        {
            if (state?.Components == null || state.Components.Length != count || state.Components.Any(c => c != ComponentAvailability.Available))
                throw new ArgumentException("IncompleteComponentsCannotBeRotated");
        }
        private static void Finite(params double[] values)
        { foreach (var value in values) NumericGuard.Finite(value, "resultant"); }
    }
}
