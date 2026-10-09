using GPC.Model.Results.Processing;
using System;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Results.Locations;
using GPC.Model.Core.Coordinates;

namespace GPC.Model.Structure.Topology
{
    /// <summary>Physical reference lines. All stations are normalized on the explicitly named domain.
    /// Offsets are vectors from connectivity nodes to the reference line, never translations of the nodes.</summary>
    public sealed class BeamReferenceGeometry
    {
        public Point3d Start { get; private set; }
        public Point3d End { get; private set; }
        public double Length => Axes.Length(End - Start);
        private readonly BeamElement beam;
        public BeamReferenceGeometry(BeamElement beam)
        {
            this.beam = beam ?? throw new ArgumentNullException(nameof(beam));
            if (!Axes.IsFinite(beam.StartPoint) || !Axes.IsFinite(beam.EndPoint) || beam.Length <= 1e-9)
                throw new ArgumentException("InvalidBeamReferenceLine");
            var a = beam.Assignments;
            Start = Shift(beam.StartPoint, a.OffsetI, a.OffsetAxes);
            End = Shift(beam.EndPoint, a.OffsetJ, a.OffsetAxes);
            NumericGuard.Finite(a.RigidLengthI, "RigidLengthI"); NumericGuard.Finite(a.RigidLengthJ, "RigidLengthJ");
            if (Length <= 1e-9 || a.RigidLengthI < 0 || a.RigidLengthJ < 0 || a.RigidLengthI + a.RigidLengthJ >= Length)
                throw new ArgumentException("InvalidDeformableLength");
        }
        private static Point3d Shift(Point3d p, Vector3d offset, CoordinateSystem axes)
        {
            if (offset == null) return new Point3d(p.X, p.Y, p.Z);
            Axes.Validate(axes);
            NumericGuard.Finite(offset.X, "offset"); NumericGuard.Finite(offset.Y, "offset"); NumericGuard.Finite(offset.Z, "offset");
            return p + axes.V1 * offset.X + axes.V2 * offset.Y + axes.V3 * offset.Z;
        }
        public double DomainLength(string domain)
        {
            switch (domain)
            {
                case "NodeToNode": return beam.Length;
                case "OffsetToOffset": return Length;
                case "Deformable": return Length - beam.Assignments.RigidLengthI - beam.Assignments.RigidLengthJ;
                default: throw new NotSupportedException("UnsupportedStationDomain: " + domain);
            }
        }
        /// <summary>Fraction on the entire offset line, for section/load assignment lookup.</summary>
        public double ReferenceStation(double station, string domain)
        {
            NumericGuard.Station(station); DomainLength(domain);
            if (domain == "Deformable") return (beam.Assignments.RigidLengthI + station * DomainLength(domain)) / Length;
            if (domain == "NodeToNode")
            {
                Vector3d nodeLine = beam.EndPoint - beam.StartPoint, referenceLine = End - Start;
                Vector3d fromStart = beam.StartPoint + nodeLine * station - Start;
                return Axes.Dot(fromStart, referenceLine) / (Length * Length);
            }
            return station;
        }
        public Point3d PointAt(double station, string domain)
        {
            NumericGuard.Station(station); DomainLength(domain);
            if (domain == "NodeToNode") { Vector3d nodeLine = beam.EndPoint - beam.StartPoint; return beam.StartPoint + nodeLine * station; }
            Vector3d line = End - Start;
            return Start + line * ReferenceStation(station, domain);
        }
        public double ConvertStation(double station, string fromDomain, string toDomain)
        {
            if (fromDomain == toDomain) { DomainLength(fromDomain); return NumericGuard.Station(station); }
            double q = ReferenceStation(station, fromDomain); DomainLength(toDomain);
            double target = toDomain == "Deformable" ? (q * Length - beam.Assignments.RigidLengthI) / DomainLength(toDomain) : q;
            if (toDomain == "NodeToNode")
            {
                Vector3d direction = (End - Start) / Length, nodeLine = beam.EndPoint - beam.StartPoint, delta = beam.StartPoint - Start;
                double projectedLength = Axes.Dot(nodeLine, direction);
                if (projectedLength <= 1e-9) throw new NotSupportedException("IncompatibleStationReferenceLines");
                target = (q * Length - Axes.Dot(delta, direction)) / projectedLength;
            }
            return NumericGuard.Station(target);
        }
        public Point3d CentroidAt(double station, string domain)
        {
            var a = beam.Assignments; var c = a.SectionCentroidOffset;
            if (c == null) throw new InvalidOperationException("UnresolvedSectionCentroidOffset");
            Axes.Validate(a.SectionAxes); NumericGuard.Finite(c.X, "centroid"); NumericGuard.Finite(c.Y, "centroid");
            return PointAt(ReferenceStation(station, domain), "OffsetToOffset") + a.SectionAxes.V1 * c.X + a.SectionAxes.V2 * c.Y;
        }
        public void ValidateSample(StationResultBeamForces sample)
        {
            if (sample == null) throw new ArgumentNullException(nameof(sample));
            double length = DomainLength(sample.StationDomain);
            if (sample.PhysicalDistance.HasValue && (!Axes.IsFinite(new Point3d(sample.PhysicalDistance.Value, 0, 0)) ||
                Math.Abs(sample.PhysicalDistance.Value - sample.ParametricDistance * length) > 1e-7 * Math.Max(1, length)))
                throw new ArgumentException("InconsistentStationDistance");
            Axes.Validate(beam.Assignments.SectionAxes);
            Vector3d line = End - Start;
            if (Axes.Dot(beam.Assignments.SectionAxes.V3, line) / Length < 1 - 1e-8) throw new ArgumentException("BeamAxisMismatch");
        }
        /// <summary>Explicit transport to a supplied physical centroid. Does not alter the imported sample.</summary>
        public StationResultBeamForces AtCentroid(StationResultBeamForces sample)
        {
            ValidateSample(sample);
            return ActionTransformations.TransportBeam(sample,
                ActionTransformations.AtPoint(beam.Assignments.SectionAxes, CentroidAt(sample.ParametricDistance, sample.StationDomain)));
        }
    }
}
