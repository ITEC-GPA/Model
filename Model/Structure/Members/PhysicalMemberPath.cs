using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Core.Coordinates;
using GPC.Model.Results.Locations;
using GPC.Model.Structure.Topology;

namespace GPC.Model.Structure.Members
{
    /// <summary>Ordered, open piecewise-linear physical path, including non-collinear members.
    /// A detached geometric snapshot; arc length is in mm on the definition's station domain.
    /// It supplies no single section frame and performs no force transport across corners.
    /// Procedures requiring a straight member continue to use PhysicalMemberGeometry.</summary>
    public sealed class PhysicalMemberPath
    {
        private sealed class Segment
        {
            internal int BeamId;
            internal bool AlongIJ;
            internal Point3d Start, End;
            internal double Distance, Length;
        }
        private readonly Segment[] segments;
        public PhysicalMemberDefinition Definition { get; }
        public double Length { get; }
        public PhysicalMemberPath(Models.Model model, PhysicalMemberDefinition definition, double connectivityToleranceMm = 1e-6)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            if (NumericGuard.Finite(connectivityToleranceMm, nameof(connectivityToleranceMm)) < 0) throw new ArgumentOutOfRangeException(nameof(connectivityToleranceMm));
            var result = new List<Segment>(); var visited = new HashSet<Guid>(); var beams = new HashSet<int>();
            Guid? previous = null; Point3d last = null;
            foreach (var part in definition.Parts)
            {
                if (part == null || !beams.Add(part.BeamId) || !model.BeamElements.TryGetValue(part.BeamId, out var beam) || beam.NodeI == null || beam.NodeJ == null)
                    throw new ArgumentException("MissingOrRepeatedMemberBeam");
                if (beam.Assignments.Formulation != Assignments.BeamFormulation.StraightTwoNode && beam.Assignments.Formulation != Assignments.BeamFormulation.Truss &&
                    beam.Assignments.Formulation != Assignments.BeamFormulation.TensionOnly && beam.Assignments.Formulation != Assignments.BeamFormulation.Cable)
                    throw new NotSupportedException("MemberPathRequiresTwoNodeElements");
                var startNode = part.AlongIJ ? beam.NodeI : beam.NodeJ; var endNode = part.AlongIJ ? beam.NodeJ : beam.NodeI;
                foreach (var node in new[] { startNode, endNode })
                    if (!model.NodesElements.TryGetValue(node.Id, out var registered) || !ReferenceEquals(registered, node)) throw new ArgumentException("DanglingMemberNode");
                if (previous.HasValue && previous != startNode.Guid) throw new ArgumentException("DisconnectedOrUnorderedMember");
                if (!previous.HasValue) visited.Add(startNode.Guid);
                if (!visited.Add(endNode.Guid)) throw new ArgumentException("CyclicMember");
                var geometry = new BeamReferenceGeometry(beam);
                var start = geometry.PointAt(part.AlongIJ ? 0 : 1, definition.StationDomain);
                var end = geometry.PointAt(part.AlongIJ ? 1 : 0, definition.StationDomain);
                if (last != null && Axes.Length(start - last) > connectivityToleranceMm) throw new ArgumentException("UnresolvedMemberReferenceGap");
                double length = geometry.DomainLength(definition.StationDomain);
                if (!(length > 0)) throw new ArgumentException("DegenerateMemberSegment"); // stations divide by the segment length (review B9)
                result.Add(new Segment { BeamId = beam.Id, AlongIJ = part.AlongIJ, Start = start, End = end, Distance = Length, Length = length });
                Length = NumericGuard.Finite(Length + length, "memberLength"); last = end; previous = endNode.Guid;
            }
            if (result.Count == 0) throw new ArgumentException("EmptyMemberPath");
            segments = result.ToArray();
        }
        private IEnumerable<Segment> At(double distance, SectionSide side)
        {
            NumericGuard.Finite(distance, nameof(distance));
            if (distance < 0 || distance > Length || !Enum.IsDefined(typeof(SectionSide), side)) throw new ArgumentOutOfRangeException(nameof(distance));
            return segments.Where(p => distance >= p.Distance && distance <= p.Distance + p.Length &&
                !(distance == p.Distance && distance > 0 && side == SectionSide.Left) &&
                !(distance == p.Distance + p.Length && distance < Length && side == SectionSide.Right));
        }
        private static SectionSide Reverse(SectionSide side) => side == SectionSide.Left ? SectionSide.Right : side == SectionSide.Right ? SectionSide.Left : side;
        /// <summary>At a corner Unspecified retains both sections; side is relative to the path.</summary>
        public IReadOnlyList<MemberLocation> Locate(double distance, SectionSide side = SectionSide.Unspecified) => At(distance, side)
            .Select(p => new MemberLocation(Definition.Id, p.BeamId, distance,
                p.AlongIJ ? (distance - p.Distance) / p.Length : 1 - (distance - p.Distance) / p.Length,
                Definition.StationDomain, p.AlongIJ ? side : Reverse(side))).ToArray();
        public IReadOnlyList<Point3d> PointsAt(double distance, SectionSide side = SectionSide.Unspecified) => At(distance, side)
            .Select(p => p.Start + (p.End - p.Start) * ((distance - p.Distance) / p.Length)).ToArray();
        public IReadOnlyList<Vector3d> TangentsAt(double distance, SectionSide side = SectionSide.Unspecified) => At(distance, side)
            .Select(p => (Vector3d)((p.End - p.Start) / p.Length)).ToArray();
    }
}
