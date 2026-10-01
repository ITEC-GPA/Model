using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Elements;

namespace GPC.Model.PostProcessing
{
    [Serializable]
    public sealed class BeamMemberPart
    {
        public int BeamId { get; private set; }
        public bool AlongIJ { get; private set; }
        public BeamMemberPart(int beamId, bool alongIJ) { BeamId = beamId; AlongIJ = alongIJ; }
    }

    /// <summary>Explicit physical identity. A group or LogicalMember label never creates this definition implicitly.</summary>
    [Serializable]
    public sealed class PhysicalMemberDefinition
    {
        private readonly BeamMemberPart[] _parts;
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Source { get; private set; }
        public string StationDomain { get; private set; }
        public IReadOnlyList<BeamMemberPart> Parts => Array.AsReadOnly(_parts);
        public PhysicalMemberDefinition(string id, IEnumerable<BeamMemberPart> parts, string stationDomain, string name = null, string source = null)
        {
            CheckValue.Text(id, nameof(id));
            _parts = (parts ?? throw new ArgumentNullException(nameof(parts))).ToArray();
            if (_parts.Length == 0 || _parts.Any(p => p == null) || _parts.Select(p => p.BeamId).Distinct().Count() != _parts.Length)
                throw new ArgumentException("An ordered, nonempty list of distinct complete beam elements is required.");
            if (stationDomain != "NodeToNode" && stationDomain != "OffsetToOffset")
                throw new NotSupportedException("A physical path must retain rigid zones: use NodeToNode or OffsetToOffset.");
            Id = id; Name = name ?? id; Source = source; StationDomain = stationDomain;
        }
    }

    [Serializable]
    public sealed class MemberLocation
    {
        public string MemberId { get; private set; }
        public int BeamId { get; private set; }
        public double Distance { get; private set; }
        public double Station { get; private set; }
        public string StationDomain { get; private set; }
        public SectionSide Side { get; private set; }
        internal MemberLocation(string member, int beam, double distance, double station, string domain, SectionSide side)
        { MemberId = member; BeamId = beam; Distance = distance; Station = station; StationDomain = domain; Side = side; }
    }

    /// <summary>Validated straight path. Mapping does not reverse the Model or rotate/transport any forces.</summary>
    public sealed class PhysicalMemberGeometry
    {
        private sealed class Part
        {
            internal BeamElement Beam; internal BeamMemberPart Reference; internal BeamReferenceGeometry Geometry;
            internal double Start, Length;
        }
        private readonly Part[] _parts;
        private readonly CoordinateSystem _axes;
        public PhysicalMemberDefinition Definition { get; }
        public double Length { get; }
        public double DeformableLength { get; }
        public CoordinateSystem Axes => ResultTransformations.AtPoint(_axes, _axes.Origin);
        private static SectionSide Reverse(SectionSide side) => side == SectionSide.Left ? SectionSide.Right : side == SectionSide.Right ? SectionSide.Left : side;
        public PhysicalMemberGeometry(Models.Model model, PhysicalMemberDefinition definition)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            if (definition.Parts.Count == 0 || definition.Parts.Any(p => p == null) || definition.Parts.Select(p => p.BeamId).Distinct().Count() != definition.Parts.Count)
                throw new ArgumentException("InvalidMemberParts");
            var parts = new List<Part>(); var visitedNodes = new HashSet<int>();
            Point3d first = null, last = null; Vector3d direction = null; int? previousNode = null;
            foreach (var reference in definition.Parts)
            {
                if (!model.BeamElements.TryGetValue(reference.BeamId, out var beam) || beam.NodeI == null || beam.NodeJ == null)
                    throw new ArgumentException("MissingMemberBeamOrConnectivity");
                if (beam.Assignments.Formulation != BeamFormulation.StraightTwoNode) throw new NotSupportedException("MemberRequiresStraightTwoNodeBeams");
                var geometry = new BeamReferenceGeometry(beam);
                GPC.Model.PostProcessing.Axes.Validate(beam.Assignments.SectionAxes);
                int startNode = reference.AlongIJ ? beam.NodeI.Id : beam.NodeJ.Id, endNode = reference.AlongIJ ? beam.NodeJ.Id : beam.NodeI.Id;
                if (previousNode.HasValue && previousNode != startNode) throw new ArgumentException("DisconnectedOrUnorderedMember");
                if (!previousNode.HasValue) visitedNodes.Add(startNode);
                if (!visitedNodes.Add(endNode)) throw new ArgumentException("CyclicMember");
                var start = geometry.PointAt(reference.AlongIJ ? 0 : 1, definition.StationDomain);
                var end = geometry.PointAt(reference.AlongIJ ? 1 : 0, definition.StationDomain);
                double length = geometry.DomainLength(definition.StationDomain);
                var tangent = (end - start) / length;
                if (last != null && GPC.Model.PostProcessing.Axes.Length(start - last) > 1e-6) throw new NotSupportedException("UnresolvedMemberReferenceGap");
                if (direction != null && GPC.Model.PostProcessing.Axes.Dot(direction, tangent) < 1 - 1e-10) throw new NotSupportedException("NonStraightMember");
                if (first == null) { first = start; direction = tangent; }
                parts.Add(new Part { Beam = beam, Reference = reference, Geometry = geometry, Start = Length, Length = length });
                Length += length; DeformableLength += geometry.DomainLength("Deformable"); last = end; previousNode = endNode;
            }
            _parts = parts.ToArray();
            _axes = GPC.Model.PostProcessing.Axes.Beam(first, last, _parts[0].Beam.Assignments.SectionAxes.V1);
        }

        /// <summary>At a common node Unspecified returns both source sections; Left/Right is relative to the member path.</summary>
        public IReadOnlyList<MemberLocation> Locate(double distance, SectionSide side = SectionSide.Unspecified)
        {
            CheckValue.Finite(distance, nameof(distance));
            if (distance < 0 || distance > Length || !Enum.IsDefined(typeof(SectionSide), side)) throw new ArgumentOutOfRangeException(nameof(distance));
            return _parts.Where(p => distance >= p.Start && distance <= p.Start + p.Length
                    && !(distance == p.Start && distance > 0 && side == SectionSide.Left)
                    && !(distance == p.Start + p.Length && distance < Length && side == SectionSide.Right))
                .Select(p => new MemberLocation(Definition.Id, p.Beam.Id, distance,
                    p.Reference.AlongIJ ? (distance - p.Start) / p.Length : 1 - (distance - p.Start) / p.Length,
                    Definition.StationDomain, p.Reference.AlongIJ ? side : Reverse(side))).ToArray();
        }
        public MemberLocation FromElement(int beamId, double station, string domain, SectionSide side)
        {
            var p = _parts.SingleOrDefault(v => v.Beam.Id == beamId) ?? throw new ArgumentException("BeamOutsideMember");
            double q = p.Geometry.ConvertStation(station, domain, Definition.StationDomain);
            return new MemberLocation(Definition.Id, beamId, p.Start + (p.Reference.AlongIJ ? q : 1 - q) * p.Length, station, domain, side);
        }
    }

    /// <summary>Unknown restraint is null, never an implicit free/fixed boundary condition. Axes are GPC.Geometry frames.</summary>
    [Serializable]
    public sealed class MemberRestraint
    {
        private readonly CoordinateSystem _axes;
        private readonly bool?[] _restrained;
        public double Distance { get; private set; }
        public string Phase { get; private set; }
        public string Source { get; private set; }
        public CoordinateSystem Axes => ResultTransformations.AtPoint(_axes, _axes.Origin);
        /// <summary>Order V1,V2,V3 translations then rotations. Elastic stiffness is not represented by these flags.</summary>
        public IReadOnlyList<bool?> Restrained => Array.AsReadOnly(_restrained);
        public MemberRestraint(double distance, CoordinateSystem axes, IEnumerable<bool?> restrained, string source, string phase = null)
        {
            CheckValue.Finite(distance, nameof(distance)); CheckValue.Text(source, nameof(source));
            _restrained = (restrained ?? throw new ArgumentNullException(nameof(restrained))).ToArray();
            if (distance < 0 || _restrained.Length != 6) throw new ArgumentException("Six explicit nullable restraint flags required.");
            _axes = ResultTransformations.AtPoint(axes, axes.Origin); Distance = distance; Source = source; Phase = phase;
        }
    }
    [Serializable]
    public sealed class MemberDesignContext
    {
        private readonly MemberRestraint[] _restraints;
        public double? EffectiveLength1 { get; private set; }
        public double? EffectiveLength2 { get; private set; }
        public double? LateralTorsionalLength { get; private set; }
        public string LengthSource { get; private set; }
        public string Assumptions { get; private set; }
        public bool GlobalStateConfirmed { get; private set; }
        public IReadOnlyList<MemberRestraint> Restraints => Array.AsReadOnly(_restraints);
        public MemberDesignContext(IEnumerable<MemberRestraint> restraints = null, double? effectiveLength1 = null,
            double? effectiveLength2 = null, double? lateralTorsionalLength = null, string lengthSource = null,
            string assumptions = null, bool globalStateConfirmed = false)
        {
            foreach (var length in new[] { effectiveLength1, effectiveLength2, lateralTorsionalLength })
            { CheckValue.Finite(length, nameof(length)); if (length <= 0) throw new ArgumentOutOfRangeException(nameof(length)); }
            if (effectiveLength1.HasValue || effectiveLength2.HasValue || lateralTorsionalLength.HasValue) CheckValue.Text(lengthSource, nameof(lengthSource));
            _restraints = (restraints ?? Enumerable.Empty<MemberRestraint>()).ToArray();
            if (_restraints.Any(r => r == null)) throw new ArgumentException("NullMemberRestraint");
            EffectiveLength1 = effectiveLength1; EffectiveLength2 = effectiveLength2; LateralTorsionalLength = lateralTorsionalLength;
            LengthSource = lengthSource; Assumptions = assumptions; GlobalStateConfirmed = globalStateConfirmed;
        }
    }
}
