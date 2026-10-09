using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A cold-formed section of constant thickness: the middle line (a polyline) bent with the inner radius r at its vertices, the outline is the
    /// middle line widened by half the thickness on each side (the bends are arcs of radius r and r + t). Factories for the usual shapes (with the
    /// dimensions out to out): <see cref="Channel"/>, <see cref="LippedChannel"/>, <see cref="Zed"/>, <see cref="LippedZed"/>, <see cref="Hat"/>,
    /// <see cref="Sigma"/>. Area, centroid, moments of inertia, elastic and plastic moduli exact on the outline; torsion constant, warping
    /// constant and shear centre from the finite elements at the first access; the thin walls are the flat parts of the middle line (the widths
    /// for the classification and the effective widths, that depend on the code, are not applied). The origin is the bottom left corner of the
    /// bounding box
    /// </summary>
    [Serializable]
    public class SectionColdFormed : ThinWallSection, ISerializable
    {
        /// <summary>
        /// The chords of a bend of 90°
        /// </summary>
        private const int BendSegments = 16;

        private readonly Point2d[] _middleLine;
        private readonly double _thickness, _innerRadius, _width;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="middleLine">The vertices of the middle line (at least 2; it is moved with the outline at the origin)</param>
        /// <param name="thickness">The thickness</param>
        /// <param name="innerRadius">The inner radius of the bends (0: sharp inside, the outer radius is the thickness)</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If the thickness is not positive, the radius is negative, there are less than 2 vertices or the
        /// bends do not fit in the parts</exception>
        public SectionColdFormed(IEnumerable<Point2d> middleLine, double thickness, double innerRadius, string name = "")
            : base(name)
        {
            Point2d[] points = middleLine?.ToArray() ?? throw new ArgumentNullException(nameof(middleLine));
            if (points.Length < 2)
                throw new ArgumentException("The middle line needs at least 2 vertices", nameof(middleLine));
            _thickness = thickness > 0 ? thickness : throw new ArgumentException("The thickness must be positive", nameof(thickness));
            _innerRadius = innerRadius >= 0 ? innerRadius : throw new ArgumentException("The radius cannot be negative", nameof(innerRadius));

            // the outline at the origin: the middle line moved by the same amount
            Shape2d outline = Outline(points, out _);
            var (minX, minY, _, _) = SectionHelper.Bounds(outline);
            _middleLine = points.Select(p => new Point2d(p.X - minX, p.Y - minY)).ToArray();

            _shape = Outline(_middleLine, out List<ThinWall> walls);
            SetThinWalls(walls.ToArray());
            var (_, _, maxX, maxY) = SectionHelper.Bounds(_shape);
            Height = maxY;
            _width = maxX;
            SetMechanicalProperties();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionColdFormed(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _middleLine = (Point2d[])info.GetValue("ColdFormedMiddleLine", typeof(Point2d[]));
            _thickness = info.GetDouble("ColdFormedThickness");
            _innerRadius = info.GetDouble("ColdFormedInnerRadius");
            var (_, _, maxX, maxY) = SectionHelper.Bounds(_shape);
            Height = maxY;
            _width = maxX;
        }

        /// <summary>
        /// A plain channel: web on the left, flanges towards the positive X
        /// </summary>
        /// <param name="height">The height out to out</param>
        /// <param name="width">The width of the flanges out to out</param>
        /// <param name="thickness">The thickness</param>
        /// <param name="innerRadius">The inner radius of the bends</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionColdFormed Channel(double height, double width, double thickness, double innerRadius, string name = "")
        {
            double t = thickness / 2;
            return new SectionColdFormed(new[] { new Point2d(width, t), new Point2d(t, t), new Point2d(t, height - t), new Point2d(width, height - t) },
                thickness, innerRadius, name);
        }

        /// <summary>
        /// A lipped channel: web on the left, flanges towards the positive X, lips towards the other flange
        /// </summary>
        /// <param name="height">The height out to out</param>
        /// <param name="width">The width of the flanges out to out</param>
        /// <param name="lip">The depth of the lips out to out</param>
        /// <param name="thickness">The thickness</param>
        /// <param name="innerRadius">The inner radius of the bends</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionColdFormed LippedChannel(double height, double width, double lip, double thickness, double innerRadius, string name = "")
        {
            double t = thickness / 2, x = width - t;
            return new SectionColdFormed(new[]
            {
                new Point2d(x, lip), new Point2d(x, t), new Point2d(t, t), new Point2d(t, height - t), new Point2d(x, height - t), new Point2d(x, height - lip),
            }, thickness, innerRadius, name);
        }

        /// <summary>
        /// A plain zed: the bottom flange towards the negative X, the top one towards the positive X
        /// </summary>
        /// <param name="height">The height out to out</param>
        /// <param name="topWidth">The width of the top flange out to out</param>
        /// <param name="bottomWidth">The width of the bottom flange out to out</param>
        /// <param name="thickness">The thickness</param>
        /// <param name="innerRadius">The inner radius of the bends</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionColdFormed Zed(double height, double topWidth, double bottomWidth, double thickness, double innerRadius, string name = "")
        {
            double t = thickness / 2;
            return new SectionColdFormed(new[]
            {
                new Point2d(-(bottomWidth - t), t), new Point2d(0, t), new Point2d(0, height - t), new Point2d(topWidth - t, height - t),
            }, thickness, innerRadius, name);
        }

        /// <summary>
        /// A lipped zed: the bottom flange towards the negative X with its lip up, the top one towards the positive X with its lip down
        /// </summary>
        /// <param name="height">The height out to out</param>
        /// <param name="topWidth">The width of the top flange out to out</param>
        /// <param name="bottomWidth">The width of the bottom flange out to out</param>
        /// <param name="lip">The depth of the lips out to out</param>
        /// <param name="thickness">The thickness</param>
        /// <param name="innerRadius">The inner radius of the bends</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionColdFormed LippedZed(double height, double topWidth, double bottomWidth, double lip, double thickness, double innerRadius,
            string name = "")
        {
            double t = thickness / 2;
            return new SectionColdFormed(new[]
            {
                new Point2d(-(bottomWidth - t), lip), new Point2d(-(bottomWidth - t), t), new Point2d(0, t), new Point2d(0, height - t),
                new Point2d(topWidth - t, height - t), new Point2d(topWidth - t, height - lip),
            }, thickness, innerRadius, name);
        }

        /// <summary>
        /// A hat (omega): the top flange between two webs, the bottom flanges outwards
        /// </summary>
        /// <param name="height">The height out to out</param>
        /// <param name="topWidth">The width of the top flange out to out (between the outer faces of the webs)</param>
        /// <param name="bottomFlange">The width of each bottom flange, from the outer face of its web to the tip</param>
        /// <param name="thickness">The thickness</param>
        /// <param name="innerRadius">The inner radius of the bends</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionColdFormed Hat(double height, double topWidth, double bottomFlange, double thickness, double innerRadius, string name = "")
        {
            double t = thickness / 2, w = topWidth / 2 - t;
            return new SectionColdFormed(new[]
            {
                new Point2d(-topWidth / 2 - bottomFlange, t), new Point2d(-w, t), new Point2d(-w, height - t), new Point2d(w, height - t), new Point2d(w, t),
                new Point2d(topWidth / 2 + bottomFlange, t),
            }, thickness, innerRadius, name);
        }

        /// <summary>
        /// A sigma: a lipped channel with the middle of the web indented towards the flanges (web on the left)
        /// </summary>
        /// <param name="height">The height out to out</param>
        /// <param name="width">The width of the flanges out to out</param>
        /// <param name="lip">The depth of the lips out to out</param>
        /// <param name="indent">The depth of the indent of the web (middle lines)</param>
        /// <param name="indentHeight">The height of the flat part of the indent</param>
        /// <param name="slopeHeight">The height of each inclined part of the web</param>
        /// <param name="thickness">The thickness</param>
        /// <param name="innerRadius">The inner radius of the bends</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionColdFormed Sigma(double height, double width, double lip, double indent, double indentHeight, double slopeHeight,
            double thickness, double innerRadius, string name = "")
        {
            double t = thickness / 2, x = width - t, m = height / 2, f = indentHeight / 2;
            return new SectionColdFormed(new[]
            {
                new Point2d(x, lip), new Point2d(x, t), new Point2d(t, t), new Point2d(t, m - f - slopeHeight), new Point2d(t + indent, m - f),
                new Point2d(t + indent, m + f), new Point2d(t, m + f + slopeHeight), new Point2d(t, height - t), new Point2d(x, height - t),
                new Point2d(x, height - lip),
            }, thickness, innerRadius, name);
        }

        /// <summary>The thickness</summary>
        public double Thickness => _thickness;

        /// <summary>The inner radius of the bends</summary>
        public double InnerRadius => _innerRadius;

        /// <summary>The vertices of the middle line (in the coordinates of the section)</summary>
        public IReadOnlyList<Point2d> MiddleLine => _middleLine;

        /// <summary>The width of the bounding box</summary>
        public override double Width => _width;

        /// <summary>The two offsets of the bent middle line: exact arcs of radius r and r+t, joined at the ends.</summary>
        public override IReadOnlyList<SectionCurveOutline> GetCurveOutlines()
        {
            double half = _thickness / 2, rm = _innerRadius + half;
            Vector3d Direction(int i)
            {
                double dx = _middleLine[i + 1].X - _middleLine[i].X, dy = _middleLine[i + 1].Y - _middleLine[i].Y;
                double length = Math.Sqrt(dx * dx + dy * dy);
                return new Vector3d(dx / length, dy / length, 0);
            }
            Point3d Offset(Point2d p, Vector3d d, double offset) => new Point3d(p.X - d.Y * offset, p.Y + d.X * offset, 0);
            var pieces = new List<Curve3d>();
            List<Curve3d> Side(double offset)
            {
                var side = new List<Curve3d>();
                Point3d current = Offset(_middleLine[0], Direction(0), offset);
                void LineTo(Point3d end)
                {
                    if (current.DistanceTo(end) > 1e-12 * _thickness) side.Add(new LineCurve3d(current, end));
                    current = end;
                }
                for (int i = 1; i < _middleLine.Length - 1; i++)
                {
                    Vector3d u = Direction(i - 1), w = Direction(i);
                    double cross = u.X * w.Y - u.Y * w.X, dot = u.X * w.X + u.Y * w.Y;
                    double phi = Math.Atan2(Math.Abs(cross), dot);
                    if (phi < 1e-9) continue;
                    double sign = cross > 0 ? 1 : -1, distance = rm * Math.Tan(phi / 2);
                    var tangent = new Point2d(_middleLine[i].X - u.X * distance, _middleLine[i].Y - u.Y * distance);
                    var center = Offset(tangent, u, sign * rm);
                    double radius = rm - sign * offset;
                    if (radius == 0) LineTo(center);
                    else
                    {
                        var arc = new ArcCurve3d(center, new Vector3d(0, 0, 1), new Vector3d(sign * u.Y, -sign * u.X, 0), radius, sign * phi);
                        LineTo(arc.StartPoint);
                        side.Add(arc);
                        current = arc.EndPoint;
                    }
                }
                LineTo(Offset(_middleLine[_middleLine.Length - 1], Direction(_middleLine.Length - 2), offset));
                return side;
            }
            var left = Side(half);
            var right = Side(-half);
            // A zero inside radius can collapse one entire side to a point. End caps still exist in that case.
            int last = _middleLine.Length - 1;
            pieces.AddRange(left);
            pieces.Add(new LineCurve3d(Offset(_middleLine[last], Direction(last - 1), half), Offset(_middleLine[last], Direction(last - 1), -half)));
            pieces.AddRange(right.AsEnumerable().Reverse().Select(c => c.Reversed()));
            pieces.Add(new LineCurve3d(Offset(_middleLine[0], Direction(0), -half), Offset(_middleLine[0], Direction(0), half)));
            return new[] { new SectionCurveOutline(new PolyCurve3d(pieces)) };
        }

        /// <summary>
        /// The outline of a middle line: the samples of the middle line with the bends (points and directions), offset by half the thickness on
        /// each side; the flat parts as thin walls
        /// </summary>
        private Shape2d Outline(Point2d[] points, out List<ThinWall> walls)
        {
            double rm = _innerRadius + _thickness / 2;
            var samples = new List<(double X, double Y, double Dx, double Dy)>();
            walls = new List<ThinWall>();

            (double x, double y) Direction(int i)
            {
                double dx = points[i + 1].X - points[i].X, dy = points[i + 1].Y - points[i].Y, length = Math.Sqrt(dx * dx + dy * dy);
                if (length == 0)
                    throw new ArgumentException("Two consecutive vertices of the middle line coincide");
                return (dx / length, dy / length);
            }

            var first = Direction(0);
            samples.Add((points[0].X, points[0].Y, first.x, first.y));
            Point2d flatStart = points[0];
            var used = new double[points.Length]; // the length of each part used by the bends at its ends
            for (int k = 1; k < points.Length - 1; k++)
            {
                var u = Direction(k - 1);
                var w = Direction(k);
                double cross = u.x * w.y - u.y * w.x, dot = u.x * w.x + u.y * w.y;
                double phi = Math.Atan2(Math.Abs(cross), dot); // the turn
                if (phi < 1e-9)
                    continue;
                double d = rm * Math.Tan(phi / 2);
                double lengthIn = Distance(points[k - 1], points[k]), lengthOut = Distance(points[k], points[k + 1]);
                if (used[k - 1] + d > lengthIn * (1 + 1e-9) || d > lengthOut * (1 + 1e-9))
                    throw new ArgumentException($"The bend at the vertex {k} does not fit in its parts");
                used[k] = d;

                double sign = cross > 0 ? 1 : -1; // left turn: the centre on the left
                var t1 = new Point2d(points[k].X - u.x * d, points[k].Y - u.y * d);
                double cx = t1.X - sign * u.y * rm, cy = t1.Y + sign * u.x * rm;
                walls.Add(new ThinWall(flatStart, t1, _thickness));
                flatStart = new Point2d(points[k].X + w.x * d, points[k].Y + w.y * d);

                double start = Math.Atan2(t1.Y - cy, t1.X - cx);
                int segments = Math.Max(1, (int)Math.Ceiling(phi / (Math.PI / 2) * BendSegments));
                for (int s = 0; s <= segments; s++)
                {
                    double angle = start + sign * phi * s / segments;
                    double px = cx + rm * Math.Cos(angle), py = cy + rm * Math.Sin(angle);
                    // the tangent of the arc in the direction of the middle line
                    samples.Add((px, py, -sign * Math.Sin(angle), sign * Math.Cos(angle)));
                }
            }
            int lastIndex = points.Length - 1;
            var last = Direction(lastIndex - 1);
            if (used[lastIndex - 1] > Distance(points[lastIndex - 1], points[lastIndex]) * (1 + 1e-9))
                throw new ArgumentException("The last bend does not fit in the last part");
            samples.Add((points[lastIndex].X, points[lastIndex].Y, last.x, last.y));
            walls.Add(new ThinWall(flatStart, points[lastIndex], _thickness));

            // left side forward, right side backward
            double h = _thickness / 2;
            var outline = new List<Point2d>();
            foreach (var p in samples)
                outline.Add(new Point2d(p.X - p.Dy * h, p.Y + p.Dx * h));
            for (int i = samples.Count - 1; i >= 0; i--)
                outline.Add(new Point2d(samples[i].X + samples[i].Dy * h, samples[i].Y - samples[i].Dx * h));

            var unique = new List<Point2d>();
            foreach (Point2d p in outline)
            {
                if (unique.Count == 0 || Distance(unique[unique.Count - 1], p) > 1e-9 * _thickness)
                    unique.Add(p);
            }
            return new Shape2d(new Polygon2d(unique.ToArray()));
        }

        private static double Distance(Point2d a, Point2d b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

        /// <summary>
        /// The outline (built by the constructor)
        /// </summary>
        /// <returns>The outline</returns>
        protected override Shape2d GetShape() => _shape;

        /// <summary>The area of the outline</summary>
        /// <returns>The exact area</returns>
        protected override double CalculateArea() => Shape.GetArea();

        /// <summary>The centroid of the outline</summary>
        /// <returns>The exact centroid</returns>
        protected override Point2d CalculateCentroid()
        {
            SectionHelper.CalculateStaticMoments(Shape, out double sx, out double sy);
            return SectionHelper.CalculateCentroid(sx, sy, _area);
        }

        /// <summary>The moment of inertia about X, exact on the outline</summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJxx()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out double jxx, out _, out _, out _);
            return jxx;
        }

        /// <summary>The moment of inertia about Y, exact on the outline</summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJyy()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out _, out double jyy, out _, out _);
            return jyy;
        }

        /// <summary>The product of inertia, exact on the outline</summary>
        /// <returns>The product of inertia</returns>
        protected override double CalculateJxy()
        {
            SectionHelper.CalculateInertiaMoments(Shape, _centroid, out _, out _, out double jxy, out _);
            return jxy;
        }

        /// <summary>The plastic modulus about X: exact on the outline, computed at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWplX() => double.NaN;

        /// <summary>The plastic modulus about Y: exact on the outline, computed at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWplY() => double.NaN;

        /// <summary>The plastic modulus about the axis 1: exact on the outline, computed at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWpl1() => double.NaN;

        /// <summary>The plastic modulus about the axis 2: exact on the outline, computed at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWpl2() => double.NaN;

        /// <summary>The torsion constant: computed with the finite elements at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateJt() => double.NaN;

        /// <summary>The warping constant: computed with the finite elements at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateJw() => double.NaN;

        /// <summary>The shear centre: computed with the finite elements at the first access</summary>
        /// <returns>null</returns>
        protected override Point2d CalculateShearCenter() => null;

        /// <summary>Symmetric about the horizontal axis if the outline is</summary>
        /// <returns>True if symmetric</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis() => SectionHelper.IsSymmetric(Shape, _centroid, false);

        /// <summary>Symmetric about the vertical axis if the outline is</summary>
        /// <returns>True if symmetric</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis() => SectionHelper.IsSymmetric(Shape, _centroid, true);

        /// <summary>
        /// Geometric properties exact on the outline, torsion constant, warping constant and shear centre numerical
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The declared availability</returns>
        protected override PropertyAvailability DeclaredAvailability(SectionProperty property) =>
            Declared(property, PropertyAvailability.Numerical, PropertyAvailability.Numerical, PropertyAvailability.Numerical);

        /// <summary>
        /// The vertices of the outline
        /// </summary>
        /// <returns>The vertices</returns>
        public override Point2d[] GetSectionPoints() => Shape.GetPoints2d();

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionColdFormedVersion", 1);
            info.AddValue("ColdFormedMiddleLine", _middleLine, typeof(Point2d[]));
            info.AddValue("ColdFormedThickness", _thickness);
            info.AddValue("ColdFormedInnerRadius", _innerRadius);
        }

        /// <summary>
        /// The description: "CF t / r" and the name
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"CF {Name} t={_thickness} r={_innerRadius}";
    }
}
