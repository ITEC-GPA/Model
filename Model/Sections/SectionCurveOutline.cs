using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Sections
{
    /// <summary>A snapshot of a section region in XY: a closed boundary, holes and material islands inside holes.
    /// Curves are owned copies. Tessellation is explicit; orientation does not determine whether a loop is a hole.</summary>
    [Serializable]
    public sealed class SectionCurveOutline
    {
        private readonly Curve3d _boundary;
        private readonly Curve3d[] _holes;
        private readonly SectionCurveOutline[] _children;

        /// <summary>Creates a snapshot. Loops must be closed; XY planarity is checked when converting to a shape.
        /// As with Shape2d, containment and self-intersections are the caller's responsibility.</summary>
        public SectionCurveOutline(Curve3d boundary, IEnumerable<Curve3d> holes = null,
            IEnumerable<SectionCurveOutline> children = null, bool isApproximation = false)
        {
            _boundary = CopyClosed(boundary);
            _holes = holes?.Select(CopyClosed).ToArray() ?? Array.Empty<Curve3d>();
            _children = children?.ToArray() ?? Array.Empty<SectionCurveOutline>();
            if (_children.Any(c => c == null)) throw new ArgumentException("A child cannot be null.", nameof(children));
            IsApproximation = isApproximation || _children.Any(c => c.IsApproximation);
        }

        /// <summary>A copy of the outer loop in section coordinates.</summary>
        public Curve3d Boundary => _boundary.DuplicateCurve();
        /// <summary>Copies of the hole loops in section coordinates.</summary>
        public IReadOnlyList<Curve3d> Holes => Array.AsReadOnly(_holes.Select(c => c.DuplicateCurve()).ToArray());
        /// <summary>Immutable snapshots of the islands inside holes.</summary>
        public IReadOnlyList<SectionCurveOutline> Children => Array.AsReadOnly(_children);
        /// <summary>True when the source was already a sampled approximation of another geometry.</summary>
        public bool IsApproximation { get; }

        /// <summary>Builds a new polygonal shape for existing calculations and meshers. Tolerance bounds the
        /// chord deviation from these curves, not the error of an already approximated source.</summary>
        public Shape2d ToShape(double chordTolerance = GeometryBase.Tolerance, double maxSegmentLength = double.PositiveInfinity)
        {
            Polygon2d Polygon(Curve3d curve)
            {
                Polygon3d polygon = curve.ToPolygon3d(chordTolerance, maxSegmentLength);
                if (polygon.Any(p => Math.Abs(p.Z) > chordTolerance))
                    throw new ArgumentException("Section curves must lie in the XY plane (Z = 0).");
                return new Polygon2d(polygon);
            }
            return new Shape2d(Polygon(_boundary), _holes.Length == 0 ? null : _holes.Select(Polygon).ToArray(),
                _children.Length == 0 ? null : _children.Select(c => c.ToShape(chordTolerance, maxSegmentLength)).ToArray());
        }

        /// <summary>Copies a polygonal region, retaining holes and recursively nested islands.</summary>
        public static SectionCurveOutline FromShape(Shape shape, bool isApproximation = false)
        {
            if (shape == null) throw new ArgumentNullException(nameof(shape));
            return new SectionCurveOutline(shape.Fill.ToCurve3d(), shape.Holes?.Select(c => c.ToCurve3d()),
                shape.Childs?.Select(c => FromShape(c, isApproximation)), isApproximation);
        }

        private static Curve3d CopyClosed(Curve3d curve)
        {
            if (curve == null) throw new ArgumentNullException(nameof(curve));
            if (!curve.IsClosed) throw new ArgumentException("A section loop must be closed.", nameof(curve));
            return curve.DuplicateCurve();
        }

        internal SectionCurveOutline Placed(double x, double y, bool mirrorX, bool mirrorY)
        {
            double sx = mirrorX ? -1 : 1, sy = mirrorY ? -1 : 1;
            Point3d Point(Point3d p) => new Point3d(sx * p.X + x, sy * p.Y + y, p.Z);
            Vector3d Direction(Vector3d v) => new Vector3d(sx * v.X, sy * v.Y, v.Z);
            Curve3d Place(Curve3d curve)
            {
                switch (curve)
                {
                    case EllipseCurve3d ellipse:
                        return new EllipseCurve3d(Point(ellipse.Center), Direction(ellipse.Normal) * (sx * sy),
                            Direction(ellipse.AxisDirection), ellipse.SemiAxisX, ellipse.SemiAxisY, ellipse.StartAngle, ellipse.SweepAngle, ellipse.Domain);
                    case LineCurve3d line:
                        return new LineCurve3d(Point(line.StartPoint), Point(line.EndPoint), line.Domain);
                    case ArcCurve3d arc:
                        // Normals are axial vectors: a reflection reverses handedness.
                        return new ArcCurve3d(Point(arc.Center), Direction(arc.Normal) * (sx * sy),
                            Direction(arc.StartDirection), arc.Radius, arc.SweepAngle, arc.Domain);
                    case PolylineCurve3d polyline:
                        return new PolylineCurve3d(polyline.Vertices.Select(Point), polyline.Domain);
                    case PolyCurve3d polycurve:
                        return new PolyCurve3d(Enumerable.Range(0, polycurve.SegmentCount).Select(i => Place(polycurve.GetSegment(i))),
                            polycurve.JoinTolerance, polycurve.Domain);
                    default:
                        throw new NotSupportedException("Placement is not implemented for " + curve.GetType().Name);
                }
            }
            return new SectionCurveOutline(Place(_boundary), _holes.Select(Place),
                _children.Select(c => c.Placed(x, y, mirrorX, mirrorY)), IsApproximation);
        }
    }

    /// <summary>Optional curved geometry contract; existing ISectionShape implementations remain compatible.</summary>
    public interface ISectionCurveShape
    {
        /// <summary>Returns snapshots of the material regions, empty for sections defined only by properties.</summary>
        IReadOnlyList<SectionCurveOutline> GetCurveOutlines();
    }

    /// <summary>Access to curved outlines through existing section interfaces.</summary>
    public static class SectionCurveExtensions
    {
        /// <summary>Uses native curves when available, otherwise copies the existing polygonal shape.</summary>
        public static IReadOnlyList<SectionCurveOutline> GetCurveOutlines(this ISectionShape section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            if (section is ISectionCurveShape curves) return curves.GetCurveOutlines();
            return FromShape(section.Shape);
        }

        internal static IReadOnlyList<SectionCurveOutline> FromShape(Shape shape, bool approximation = false) =>
            shape == null ? Array.Empty<SectionCurveOutline>() : new[] { SectionCurveOutline.FromShape(shape, approximation) };
    }
}
