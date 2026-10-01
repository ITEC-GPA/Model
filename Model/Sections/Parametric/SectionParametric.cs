using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A section of a parametric shape: its outline is built from its parameters (see the derived classes), with the origin at the bottom left
    /// corner of the bounding box. Area, centroid, moments of inertia, elastic and plastic moduli are exact on the outline (the arcs are polygons
    /// with <see cref="SectionOutline.SegmentsPerQuarter"/> sides every 90°, relative error about 1e-4); torsion constant, warping constant and
    /// shear centre are solved with the finite elements at the first access (see <see cref="Section.CalculateTorsionProperties"/>) unless the
    /// class gives them. The availability of the properties is declared by <see cref="Section.GetAvailability"/>
    /// </summary>
    [Serializable]
    public abstract class SectionParametric : Section, ISerializable
    {
        /// <summary>
        /// Creates the section (the derived classes call <see cref="Build"/> at the end of their constructors)
        /// </summary>
        /// <param name="name">The name</param>
        protected SectionParametric(string name)
            : base(name)
        {
        }

        /// <summary>
        /// Deserialization constructor (the outline and the properties are the ones of <see cref="Section"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionParametric(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// The outline from the parameters, in any position (it is moved with the bottom left corner of its bounding box at the origin)
        /// </summary>
        /// <returns>The outline</returns>
        protected abstract Shape2d CreateOutline();

        /// <summary>
        /// Builds the outline and calculates the properties: the last call of the constructors of the derived classes
        /// </summary>
        protected void Build()
        {
            _shape = AtTheOrigin(CreateOutline());
            ResetMesh();
            SetMechanicalProperties();
        }

        /// <summary>
        /// The outline (built by <see cref="Build"/>)
        /// </summary>
        /// <returns>The outline</returns>
        protected override Shape2d GetShape() => _shape ?? AtTheOrigin(CreateOutline());

        /// <summary>
        /// The height of the bounding box (it cannot be set: create a new section)
        /// </summary>
        public override double Height
        {
            get => Bounds().MaxY - Bounds().MinY;
            set => throw new NotSupportedException("The height of a parametric section is given by its parameters");
        }

        /// <summary>
        /// The width of the bounding box (it cannot be set: create a new section)
        /// </summary>
        public override double Width
        {
            get => Bounds().MaxX - Bounds().MinX;
            set => throw new NotSupportedException("The width of a parametric section is given by its parameters");
        }

        /// <summary>
        /// A parametric section has no thin walls: null
        /// </summary>
        public override ThinWallSection.ThinWall[] ThinWalls => null;

        /// <summary>
        /// The vertices of the outline
        /// </summary>
        /// <returns>The vertices</returns>
        public override Point2d[] GetSectionPoints() => Shape.GetPoints2d();

        /// <summary>
        /// A parametric section has no corners to work: nothing (before, the base class threw <see cref="NotImplementedException"/>, so a
        /// <see cref="Steel.SteelSection"/> could not be created)
        /// </summary>
        /// <param name="sectionType">The type of the section</param>
        public override void SetEdgeTypeFromSteelType(SectionTypes sectionType)
        {
        }

        /// <summary>
        /// Symmetric about the horizontal axis through the centroid if the vertices of the outline are
        /// </summary>
        /// <returns>True if symmetric</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis() => IsSymmetric(false);

        /// <summary>
        /// Symmetric about the vertical axis through the centroid if the vertices of the outline are
        /// </summary>
        /// <returns>True if symmetric</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis() => IsSymmetric(true);

        /// <summary>
        /// The description of the section
        /// </summary>
        /// <returns>The type and the name</returns>
        public override string ToString() => $"{GetType().Name} {Name}";

        /// <summary>
        /// Checks that a dimension is positive
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="name">The name of the parameter</param>
        /// <returns>The value</returns>
        /// <exception cref="ArgumentException">If the value is not a positive number</exception>
        private protected static double Positive(double value, string name) =>
            value > 0 && !double.IsInfinity(value) ? value : throw new ArgumentException(name + " must be positive", name);

        /// <summary>
        /// Checks that a dimension is not negative
        /// </summary>
        /// <param name="value">The value</param>
        /// <param name="name">The name of the parameter</param>
        /// <returns>The value</returns>
        /// <exception cref="ArgumentException">If the value is negative or not a number</exception>
        private protected static double NotNegative(double value, string name) =>
            value >= 0 && !double.IsInfinity(value) ? value : throw new ArgumentException(name + " cannot be negative", name);

        /// <summary>
        /// A polygon of the points, without the consecutive duplicates
        /// </summary>
        /// <param name="points">The points</param>
        /// <returns>The polygon</returns>
        private protected static Polygon2d Polygon(IEnumerable<Point2d> points)
        {
            var list = new List<Point2d>();
            foreach (Point2d p in points)
            {
                if (list.Count == 0 || Math.Abs(list[list.Count - 1].X - p.X) > 1e-9 || Math.Abs(list[list.Count - 1].Y - p.Y) > 1e-9)
                    list.Add(p);
            }
            while (list.Count > 1 && Math.Abs(list[0].X - list[list.Count - 1].X) <= 1e-9 && Math.Abs(list[0].Y - list[list.Count - 1].Y) <= 1e-9)
                list.RemoveAt(list.Count - 1);
            return new Polygon2d(list.ToArray());
        }

        /// <summary>
        /// The polygon of an ellipse
        /// </summary>
        /// <param name="cx">X of the centre</param>
        /// <param name="cy">Y of the centre</param>
        /// <param name="a">The semi-axis along X</param>
        /// <param name="b">The semi-axis along Y</param>
        /// <returns>The polygon (<see cref="SectionOutline.SegmentsPerQuarter"/> sides every 90°, counterclockwise)</returns>
        private protected static Polygon2d Ellipse(double cx, double cy, double a, double b)
        {
            int count = 4 * SectionOutline.SegmentsPerQuarter;
            return new Polygon2d(Enumerable.Range(0, count)
                .Select(i => new Point2d(cx + a * Math.Cos(2 * Math.PI * i / count), cy + b * Math.Sin(2 * Math.PI * i / count))).ToArray());
        }

        /// <summary>
        /// The polygon of an oval with semicircular ends (stadium), counterclockwise
        /// </summary>
        /// <param name="cx">X of the centre</param>
        /// <param name="cy">Y of the centre</param>
        /// <param name="width">The width</param>
        /// <param name="height">The height</param>
        /// <returns>The polygon</returns>
        private protected static Polygon2d Stadium(double cx, double cy, double width, double height)
        {
            double r = Math.Min(width, height) / 2;
            bool horizontal = width >= height;
            double half = (Math.Max(width, height) - 2 * r) / 2;
            int n = 2 * SectionOutline.SegmentsPerQuarter;
            var points = new List<Point2d>();
            // the two semicircles: the first one centred at +half along the long axis, the second at -half
            for (int end = 0; end < 2; end++)
            {
                double sign = end == 0 ? 1 : -1;
                double ex = horizontal ? cx + sign * half : cx, ey = horizontal ? cy : cy + sign * half;
                double start = horizontal ? (end == 0 ? -Math.PI / 2 : Math.PI / 2) : (end == 0 ? 0 : Math.PI);
                for (int k = 0; k <= n; k++)
                {
                    double angle = start + Math.PI * k / n;
                    points.Add(new Point2d(ex + r * Math.Cos(angle), ey + r * Math.Sin(angle)));
                }
            }
            return Polygon(points);
        }

        /// <summary>
        /// The outline moved with the bottom left corner of its bounding box at the origin
        /// </summary>
        private static Shape2d AtTheOrigin(Shape2d shape)
        {
            double minX = double.MaxValue, minY = double.MaxValue;
            for (int i = 0; i < shape.Fill.Count; i++)
            {
                minX = Math.Min(minX, shape.Fill[i].X);
                minY = Math.Min(minY, shape.Fill[i].Y);
            }
            if (minX == 0 && minY == 0)
                return shape;

            Polygon2d Move(Polygon3d p) => new Polygon2d(Enumerable.Range(0, p.Count).Select(i => new Point2d(p[i].X - minX, p[i].Y - minY)).ToArray());
            Shape2d MoveShape(Shape s)
            {
                Polygon2d[] holes = s.Holes?.Where(h => h != null && h.Count >= 3).Select(Move).ToArray();
                Shape2d[] childs = s.Childs?.Where(c => c?.Fill != null).Select(MoveShape).ToArray();
                return new Shape2d(Move(s.Fill), holes != null && holes.Length > 0 ? holes : null, childs != null && childs.Length > 0 ? childs : null);
            }
            return MoveShape(shape);
        }

        private (double MinX, double MinY, double MaxX, double MaxY) Bounds()
        {
            Polygon3d fill = Shape.Fill;
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            for (int i = 0; i < fill.Count; i++)
            {
                minX = Math.Min(minX, fill[i].X);
                maxX = Math.Max(maxX, fill[i].X);
                minY = Math.Min(minY, fill[i].Y);
                maxY = Math.Max(maxY, fill[i].Y);
            }
            return (minX, minY, maxX, maxY);
        }

        /// <summary>
        /// True if the corners of the outline (fill and holes, without the vertices aligned with their neighbours) are symmetric about the
        /// vertical or the horizontal axis through the centroid
        /// </summary>
        private bool IsSymmetric(bool aboutVertical)
        {
            var (minX, minY, maxX, maxY) = Bounds();
            double tolerance = 1e-9 * Math.Max(maxX - minX, maxY - minY);

            var points = new List<Point2d>();
            void AddCorners(Polygon3d ring)
            {
                int count = ring.Count;
                for (int i = 0; i < count; i++)
                {
                    Point3d p = ring[(i + count - 1) % count], q = ring[i], r = ring[(i + 1) % count];
                    double cross = (q.X - p.X) * (r.Y - q.Y) - (q.Y - p.Y) * (r.X - q.X);
                    double length = Math.Sqrt((r.X - p.X) * (r.X - p.X) + (r.Y - p.Y) * (r.Y - p.Y));
                    if (Math.Abs(cross) > tolerance * length)
                        points.Add(new Point2d(q.X, q.Y));
                }
            }
            AddCorners(Shape.Fill);
            if (Shape.Holes != null)
            {
                foreach (Polygon3d hole in Shape.Holes)
                    AddCorners(hole);
            }
            var grid = new HashSet<(long, long)>(points.Select(p => ((long)Math.Round(p.X / tolerance), (long)Math.Round(p.Y / tolerance))));
            foreach (Point2d p in points)
            {
                double mx = aboutVertical ? 2 * _centroid.X - p.X : p.X, my = aboutVertical ? p.Y : 2 * _centroid.Y - p.Y;
                long ix = (long)Math.Round(mx / tolerance), iy = (long)Math.Round(my / tolerance);
                bool found = false;
                for (long i = ix - 2; i <= ix + 2 && !found; i++)
                {
                    for (long j = iy - 2; j <= iy + 2 && !found; j++)
                        found = grid.Contains((i, j));
                }
                if (!found)
                    return false;
            }
            return true;
        }
    }
}
