using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Results;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    /// <summary>Explicit source convention conversions. Topology and source assignments remain authoritative;
    /// these methods return detached data in the requested physical frame.</summary>
    public static class ResultOrientation
    {
        /// <summary>Reversing a positive cut face negates its entire wrench before rotation.
        /// reverseStations also maps I/J, xi and left/right; length is the source station-domain length.</summary>
        public static StationResultBeamForces Beam(StationResultBeamForces source, CoordinateSystem target, bool reverseStations = false, double? domainLength = null)
        {
            if (source?.Body != ActionBody.PositiveSectionFace) throw new ArgumentException("UnresolvedSectionActionBody");
            Axes.Validate(target); Axes.Validate(source.ResultBeamForces.CoordinateSystem);
            double dot = Axes.Dot(source.ResultBeamForces.CoordinateSystem.V3, target.V3);
            if (Math.Abs(dot) < 1 - 1e-8) throw new ArgumentException("IncompatibleBeamDirection");
            if (reverseStations && dot > 0) throw new ArgumentException("StationReversalRequiresOppositeDirection");
            var rotated = ResultTransformations.RotateBeam(source, target); var f = rotated.ResultBeamForces;
            if (dot < 0)
                rotated = new StationResultBeamForces(source.Case, new ResultBeamForces(-f.N, -f.V1, -f.V2, -f.T, -f.M1, -f.M2, f.CoordinateSystem), rotated.ParametricDistance)
                { Side = source.Side, PhysicalDistance = source.PhysicalDistance, Body = source.Body, StationDomain = source.StationDomain, State = rotated.State };
            if (reverseStations)
            {
                rotated.ParametricDistance = 1 - source.ParametricDistance;
                rotated.Side = source.Side == SectionSide.Left ? SectionSide.Right : source.Side == SectionSide.Right ? SectionSide.Left : source.Side;
                if (source.PhysicalDistance.HasValue)
                {
                    if (!domainLength.HasValue || NumericGuard.Finite(domainLength.Value, "length") <= 0
                        || Math.Abs(source.PhysicalDistance.Value - source.ParametricDistance * domainLength.Value) > 1e-7 * Math.Max(1, domainLength.Value))
                        throw new ArgumentException("ExplicitConsistentDomainLengthRequired");
                    rotated.PhysicalDistance = domainLength.Value - source.PhysicalDistance.Value;
                }
            }
            rotated.State.Transformation += "; explicit cut-face normalization" + (reverseStations ? "; I/J station and side reversal" : "");
            return rotated;
        }

        /// <summary>Normal reversal for first-moment shell tensors M = integral(z sigma dz).
        /// Natural coordinates refer to unchanged connectivity unless reverseConnectivity is explicitly requested;
        /// that mapping reverses a triangle/quad while retaining its first node.</summary>
        public static PointResultPlateForces Shell(PointResultPlateForces source, CoordinateSystem target, bool reverseConnectivity = false, int nodeCount = 0)
        {
            if (source?.Forces == null) throw new ArgumentNullException(nameof(source));
            var f = source.Forces; var old = f.CoordinateSystem;
            Axes.Validate(old); Axes.Validate(target);
            double sign = Axes.Dot(old.V3, target.V3);
            if (Math.Abs(sign) < 1 - 1e-10 || Axes.Length(old.Origin - target.Origin) > 1e-8) throw new ArgumentException("IncompatibleShellPlane");
            if (reverseConnectivity && (sign > 0 || nodeCount != 3 && nodeCount != 4)) throw new ArgumentException("InvalidShellConnectivityReversal");
            // Also performs the common completeness and finite-value checks.
            var copy = ResultTransformations.RotateShell(source, old);
            sign = sign < 0 ? -1 : 1;
            double a = Axes.Dot(target.V1, old.V1), b = Axes.Dot(target.V1, old.V2);
            double c = Axes.Dot(target.V2, old.V1), d = Axes.Dot(target.V2, old.V2);
            double[] Tensor(double xx, double yy, double xy) => new[] {
                a*a*xx + b*b*yy + 2*a*b*xy, c*c*xx + d*d*yy + 2*c*d*xy, a*c*xx + b*d*yy + (a*d+b*c)*xy };
            var n = Tensor(f.Fxx, f.Fyy, f.Fxy); var m = Tensor(f.Mxx, f.Myy, f.Mxy);
            foreach (double value in n.Concat(m).Concat(new[] { a*f.Fxz+b*f.Fyz, c*f.Fxz+d*f.Fyz })) NumericGuard.Finite(value, "shell resultant");
            var location = copy.Location;
            if (location != null) { NumericGuard.Finite(location.X, "location"); NumericGuard.Finite(location.Y, "location"); }
            if (location != null && source.CoordinateKind == ResultCoordinateKind.LocalPhysical)
                location = new Point2d(a * location.X + b * location.Y, c * location.X + d * location.Y);
            else if (location != null && source.CoordinateKind == ResultCoordinateKind.Natural && reverseConnectivity)
                location = new Point2d(location.Y, location.X);
            var result = new PointResultPlateForces(source.Case, new ResultPlateForces(ResultTransformations.AtPoint(target, target.Origin),
                n[0], n[1], n[2], sign * (a*f.Fxz+b*f.Fyz), sign * (c*f.Fxz+d*f.Fyz), sign*m[0], sign*m[1], sign*m[2]), location, source.LocationKind)
            { PointKind = source.PointKind, CoordinateKind = source.CoordinateKind, SourceNodeId = source.SourceNodeId,
                AveragingRegion = source.AveragingRegion, GlobalLocation = copy.GlobalLocation, State = copy.State };
            result.State.Transformation += "; explicit shell normal/tensor normalization";
            return result;
        }

        /// <summary>Preserves the physical face names and layer identity; signed depth/direction follow the target normal.</summary>
        public static ShellAssignments ShellLayers(ShellAssignments source, CoordinateSystem target)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            Axes.Validate(source.LayerAxes); Axes.Validate(target);
            var old = source.LayerAxes; double sign = Axes.Dot(old.V3, target.V3);
            if (Math.Abs(sign) < 1 - 1e-10 || Axes.Length(old.Origin - target.Origin) > 1e-8) throw new ArgumentException("IncompatibleLayerPlane");
            var result = new ShellAssignments { PhysicalThickness = source.PhysicalThickness, Offset = source.Offset * sign,
                LayerAxes = ResultTransformations.AtPoint(target, target.Origin), ReinforcementZone = source.ReinforcementZone };
            foreach (var layer in source.Layers)
            {
                NumericGuard.Finite(layer.DirectionRadians, "layer angle"); NumericGuard.Finite(layer.AxisPositionThroughThickness, "layer depth");
                var dir = old.V1 * Math.Cos(layer.DirectionRadians) + old.V2 * Math.Sin(layer.DirectionRadians);
                result.Layers.Add(new ShellRebarLayer { PhysicalFace = layer.PhysicalFace, Steel = layer.Steel, Diameter = layer.Diameter, Pitch = layer.Pitch,
                    AxisPositionThroughThickness = sign * layer.AxisPositionThroughThickness, DirectionRadians = Math.Atan2(Axes.Dot(dir, target.V2), Axes.Dot(dir, target.V1)), Order = layer.Order });
            }
            return result;
        }
    }
}
