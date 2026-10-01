using GPC.Geometry;
using System;
using System.Collections.Generic;

namespace GPC.Model.Sections
{
    /// <summary>
    /// The exact outline of a rolled or welded section: a polygon with its corners rounded by arcs tangent to the two sides (root fillets and
    /// toe radii, at any angle between the sides, e.g. with taper flanges) or cut by a straight line (welds). The arcs are discretised with
    /// <see cref="SegmentsPerQuarter"/> chords every 90°: the area of a fillet is exact within 0.02%, the properties of a section within 3e-5
    /// </summary>
    internal static class SectionOutline
    {
        /// <summary>
        /// The chords of an arc of 90°
        /// </summary>
        internal const int SegmentsPerQuarter = 64;

        /// <summary>
        /// A vertex of the outline with the working of its corner
        /// </summary>
        internal readonly struct Vertex
        {
            /// <summary>
            /// Creates a vertex
            /// </summary>
            /// <param name="x">X</param>
            /// <param name="y">Y</param>
            /// <param name="size">The radius of the arc, or the length of the cut along each side for a weld (0: sharp corner)</param>
            /// <param name="cut">True for a straight cut (weld), false for an arc</param>
            internal Vertex(double x, double y, double size = 0, bool cut = false)
            {
                X = x;
                Y = y;
                Size = size;
                Cut = cut;
            }

            internal double X { get; }

            internal double Y { get; }

            internal double Size { get; }

            internal bool Cut { get; }
        }

        /// <summary>
        /// An inside corner of a section with its working: a fillet of radius r, a weld with throat r (legs 1.41 r, as
        /// <see cref="SectionCorner.Inside"/>) or nothing
        /// </summary>
        /// <param name="x">X of the corner</param>
        /// <param name="y">Y of the corner</param>
        /// <param name="edgeType">The working</param>
        /// <param name="r">The radius or the throat</param>
        /// <returns>The vertex</returns>
        internal static Vertex Inside(double x, double y, EdgeType edgeType, double r)
        {
            switch (edgeType)
            {
                case EdgeType.Fillet:
                    return new Vertex(x, y, r);
                case EdgeType.Chamfer:
                    return new Vertex(x, y, 1.41 * r, true);
                default:
                    return new Vertex(x, y);
            }
        }

        /// <summary>
        /// Creates the outline
        /// </summary>
        /// <param name="vertices">The vertices of the polygon (any orientation) with the working of their corners</param>
        /// <returns>The shape of the outline</returns>
        /// <exception cref="ArgumentException">If a working does not fit in the sides of its corner</exception>
        internal static Shape2d Create(IReadOnlyList<Vertex> vertices) => new Shape2d(Polygon(vertices));

        /// <summary>
        /// Creates the polygon of an outline (e.g. a hole of a hollow section)
        /// </summary>
        /// <param name="vertices">The vertices of the polygon (any orientation) with the working of their corners</param>
        /// <returns>The polygon</returns>
        /// <exception cref="ArgumentException">If a working does not fit in the sides of its corner</exception>
        internal static Polygon2d Polygon(IReadOnlyList<Vertex> vertices)
        {
            var points = new List<Point2d>();
            int count = vertices.Count;
            for (int i = 0; i < count; i++)
            {
                Vertex vertex = vertices[i];
                Vertex previous = vertices[(i + count - 1) % count];
                Vertex next = vertices[(i + 1) % count];
                if (vertex.Size <= 0)
                {
                    points.Add(new Point2d(vertex.X, vertex.Y));
                    continue;
                }

                // incoming and outgoing directions of the sides
                double ux = vertex.X - previous.X, uy = vertex.Y - previous.Y, lengthIn = Math.Sqrt(ux * ux + uy * uy);
                double wx = next.X - vertex.X, wy = next.Y - vertex.Y, lengthOut = Math.Sqrt(wx * wx + wy * wy);
                ux /= lengthIn;
                uy /= lengthIn;
                wx /= lengthOut;
                wy /= lengthOut;

                // the angle between the two sides at the vertex: the working is inside it (outside the material at a reflex corner of the
                // section, inside it at a convex corner)
                double theta = Math.Acos(Math.Max(-1.0, Math.Min(1.0, -(ux * wx + uy * wy))));
                if (Math.Sin(theta) < 1e-12)
                {
                    points.Add(new Point2d(vertex.X, vertex.Y));
                    continue;
                }

                double distance = vertex.Cut ? vertex.Size : vertex.Size / Math.Tan(theta / 2.0);
                if (distance > lengthIn * (1 + 1e-9) || distance > lengthOut * (1 + 1e-9))
                    throw new ArgumentException($"The working {vertex.Size} of the corner ({vertex.X}, {vertex.Y}) does not fit in its sides");

                double x1 = vertex.X - ux * distance, y1 = vertex.Y - uy * distance;
                double x2 = vertex.X + wx * distance, y2 = vertex.Y + wy * distance;
                if (vertex.Cut)
                {
                    points.Add(new Point2d(x1, y1));
                    points.Add(new Point2d(x2, y2));
                    continue;
                }

                // centre on the bisector, at r / sin(θ / 2) from the vertex
                double bx = wx - ux, by = wy - uy, lengthB = Math.Sqrt(bx * bx + by * by);
                double centreDistance = vertex.Size / Math.Sin(theta / 2.0);
                double cx = vertex.X + bx / lengthB * centreDistance, cy = vertex.Y + by / lengthB * centreDistance;

                double start = Math.Atan2(y1 - cy, x1 - cx);
                double sweep = Math.Atan2(y2 - cy, x2 - cx) - start;
                while (sweep > Math.PI)
                    sweep -= 2.0 * Math.PI;
                while (sweep <= -Math.PI)
                    sweep += 2.0 * Math.PI;

                int segments = Math.Max(1, (int)Math.Ceiling(Math.Abs(sweep) / (Math.PI / 2.0) * SegmentsPerQuarter));
                for (int k = 0; k <= segments; k++)
                {
                    double angle = start + sweep * k / segments;
                    points.Add(new Point2d(cx + vertex.Size * Math.Cos(angle), cy + vertex.Size * Math.Sin(angle)));
                }
            }

            return new Polygon2d(points.ToArray());
        }
    }
}
