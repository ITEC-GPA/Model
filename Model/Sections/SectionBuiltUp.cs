using GPC.Geometry;
using GPC.Geometry.Meshes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A built-up section: sections of Model (the parts) moved and mirrored in the plane, connected so that they bend together (e.g. two
    /// angles or two channels back to back, a cruciform of four angles). The parts must not overlap.
    /// <list type="bullet">
    /// <item>Area, centroid and moments of inertia: the ones of the parts moved to the centroid of the section.</item>
    /// <item>Elastic moduli: from the extreme vertices of the outlines of the parts; plastic moduli: exact on the union of the outlines.</item>
    /// <item>Torsion constant: the sum of the ones of the parts (open parts connected at discrete points, as the double angles).</item>
    /// <item>Warping constant: not available (<see cref="double.NaN"/>): it depends on the connection of the parts.</item>
    /// <item>Shear centre: Σ(Iyy,i xs,i) / Σ Iyy,i, Σ(Ixx,i ys,i) / Σ Ixx,i with the moments of the parts about their centroids, exact
    /// when the shear centres of the parts are aligned, as in the double angles back to back.</item>
    /// </list>
    /// The section has no single shape: <see cref="Section.Shape"/> is null and the mesh is not available (mesh the parts)
    /// </summary>
    [Serializable]
    public class SectionBuiltUp : Section, ISerializable
    {
        /// <summary>
        /// A part of a built-up section: a point (x, y) of the part goes to (±x + <see cref="X"/>, ±y + <see cref="Y"/>), with the minus
        /// when mirrored
        /// </summary>
        [Serializable]
        public sealed class Part
        {
            /// <summary>
            /// Creates a part
            /// </summary>
            /// <param name="section">The section of the part, with its properties calculated</param>
            /// <param name="x">The move along X (after the mirror)</param>
            /// <param name="y">The move along Y (after the mirror)</param>
            /// <param name="mirrorX">True to mirror the part about its vertical axis (x to -x)</param>
            /// <param name="mirrorY">True to mirror the part about its horizontal axis (y to -y)</param>
            public Part(Section section, double x, double y, bool mirrorX = false, bool mirrorY = false)
            {
                Section = section ?? throw new ArgumentNullException(nameof(section));
                X = x;
                Y = y;
                MirrorX = mirrorX;
                MirrorY = mirrorY;
            }

            /// <summary>The section of the part</summary>
            public Section Section { get; }

            /// <summary>The move along X</summary>
            public double X { get; }

            /// <summary>The move along Y</summary>
            public double Y { get; }

            /// <summary>True if mirrored about its vertical axis</summary>
            public bool MirrorX { get; }

            /// <summary>True if mirrored about its horizontal axis</summary>
            public bool MirrorY { get; }

            /// <summary>
            /// The position of a point of the part in the section
            /// </summary>
            /// <param name="x">X in the part</param>
            /// <param name="y">Y in the part</param>
            /// <returns>The point in the section</returns>
            public Point2d Place(double x, double y) => new Point2d((MirrorX ? -x : x) + X, (MirrorY ? -y : y) + Y);

            internal Shape2d PlacedOutline()
            {
                Shape2d shape = Section.GetPlasticShape();
                Polygon2d fill = Place(shape.Fill);
                if (shape.Holes is null || shape.Holes.Length == 0)
                    return new Shape2d(fill);
                return new Shape2d(fill, shape.Holes.Select(Place).ToArray());
            }

            private Polygon2d Place(Polygon3d polygon)
            {
                var points = new Point2d[polygon.Count];
                for (int i = 0; i < points.Length; i++)
                    points[i] = Place(polygon[i].X, polygon[i].Y);
                return new Polygon2d(points);
            }
        }

        private readonly Part[] _parts;

        /// <summary>
        /// Creates the section and calculates its properties
        /// </summary>
        /// <param name="parts">The parts (at least one)</param>
        /// <param name="name">The name</param>
        /// <exception cref="ArgumentException">If there are no parts</exception>
        public SectionBuiltUp(IEnumerable<Part> parts, string name)
            : base(name)
        {
            _parts = parts?.ToArray() ?? throw new ArgumentNullException(nameof(parts));
            if (_parts.Length == 0)
                throw new ArgumentException("A built-up section needs at least one part");
            SetMechanicalProperties();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected SectionBuiltUp(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _parts = (Part[])info.GetValue("Parts", typeof(Part[]));
        }

        /// <summary>
        /// Two angles back to back: the vertical legs are back to back at the distance <paramref name="spacing"/> (the gusset plate), symmetric
        /// about the vertical axis X = 0, the other legs outstanding on the two sides at the bottom
        /// </summary>
        /// <param name="angle">The angle (see <see cref="SectionL"/>: its vertical leg is the one back to back), with the working of its
        /// corners set</param>
        /// <param name="spacing">The distance between the backs of the vertical legs</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionBuiltUp DoubleAngle(SectionL angle, double spacing, string name)
        {
            if (spacing < 0)
                throw new ArgumentException("The spacing cannot be negative");
            return new SectionBuiltUp(new[] { new Part(angle, spacing / 2.0, 0), new Part(angle, -spacing / 2.0, 0, mirrorX: true) }, name);
        }

        /// <summary>
        /// Two channels: back to back (the webs at the distance <paramref name="spacing"/>, flanges outwards) or toe to toe (the tips of the
        /// flanges at the distance <paramref name="spacing"/>, a box when it is 0), symmetric about the vertical axis X = 0
        /// </summary>
        /// <param name="channel">The channel (see <see cref="SectionC"/>: web on the left, flanges towards the positive X), with the working of
        /// its corners set</param>
        /// <param name="spacing">The distance between the webs (back to back) or between the tips of the flanges (toe to toe)</param>
        /// <param name="backToBack">True back to back, false toe to toe</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionBuiltUp DoubleChannel(SectionC channel, double spacing, bool backToBack, string name)
        {
            if (spacing < 0)
                throw new ArgumentException("The spacing cannot be negative");
            double width = channel.Width;
            return backToBack
                ? new SectionBuiltUp(new[] { new Part(channel, spacing / 2.0, 0), new Part(channel, -spacing / 2.0, 0, mirrorX: true) }, name)
                : new SectionBuiltUp(new[] { new Part(channel, -spacing / 2.0 - width, 0), new Part(channel, spacing / 2.0 + width, 0, mirrorX: true) },
                    name);
        }

        /// <summary>
        /// Four angles back to back in a cruciform (star battened): one in each quadrant with its corner towards the centre, the legs of two
        /// angles back to back at the distance <paramref name="gap"/> (the battens), centred at the origin
        /// </summary>
        /// <param name="angle">The angle (see <see cref="SectionL"/>), with the working of its corners set</param>
        /// <param name="gap">The distance between the backs of the legs</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        /// <exception cref="ArgumentException">If the gap is negative</exception>
        public static SectionBuiltUp Cruciform(SectionL angle, double gap, string name)
        {
            if (gap < 0)
                throw new ArgumentException("The gap cannot be negative");
            double g = gap / 2.0;
            return new SectionBuiltUp(new[]
            {
                new Part(angle, g, g), new Part(angle, -g, g, mirrorX: true), new Part(angle, -g, -g, mirrorX: true, mirrorY: true),
                new Part(angle, g, -g, mirrorY: true),
            }, name);
        }

        /// <summary>
        /// A generic section made of separate regions (e.g. regions outside one another, that a single shape cannot hold): each region is a
        /// generic <see cref="Section"/>. The torsion constant is the sum of the ones of the regions (computed with the finite elements)
        /// </summary>
        /// <param name="regions">The regions, in the coordinates of the section</param>
        /// <param name="name">The name</param>
        /// <returns>The section</returns>
        public static SectionBuiltUp FromRegions(IEnumerable<Shape2d> regions, string name)
        {
            var parts = (regions ?? throw new ArgumentNullException(nameof(regions))).Select((shape, i) =>
            {
                var region = new Section(shape, $"{name} {i + 1}");
                region.SetMechanicalProperties();
                return new Part(region, 0, 0);
            }).ToArray();
            return new SectionBuiltUp(parts, name);
        }

        /// <summary>
        /// The parts
        /// </summary>
        public IReadOnlyList<Part> Parts => _parts;

        /// <summary>
        /// The height of the bounding box
        /// </summary>
        public override double Height
        {
            get
            {
                var (_, minY, _, maxY) = Bounds();
                return maxY - minY;
            }
            set => throw new NotSupportedException("The height of a built-up section is the one of its parts");
        }

        /// <summary>
        /// The width of the bounding box
        /// </summary>
        public override double Width
        {
            get
            {
                var (minX, _, maxX, _) = Bounds();
                return maxX - minX;
            }
            set => throw new NotSupportedException("The width of a built-up section is the one of its parts");
        }

        /// <summary>
        /// The outlines of the parts in the section
        /// </summary>
        /// <returns>The outlines</returns>
        public IReadOnlyList<Shape2d> GetOutlines() => _parts.Select(p => p.PlacedOutline()).ToArray();

        /// <summary>
        /// The thin walls of the parts placed in the section (the parts without thin walls, e.g. circular bars, have none); before, not
        /// implemented (<see cref="NotImplementedException"/>)
        /// </summary>
        public override ThinWallSection.ThinWall[] ThinWalls => _parts.SelectMany(PlacedThinWalls).ToArray();

        private static IEnumerable<ThinWallSection.ThinWall> PlacedThinWalls(Part part)
        {
            ThinWallSection.ThinWall[] walls;
            try
            {
                walls = part.Section.ThinWalls;
            }
            catch (NotImplementedException)
            {
                walls = null;
            }

            if (walls is null)
                yield break;

            foreach (ThinWallSection.ThinWall wall in walls)
            {
                Point2d[] middle = wall.GetMiddleLine();
                yield return new ThinWallSection.ThinWall(part.Place(middle[0].X, middle[0].Y), part.Place(middle[1].X, middle[1].Y), wall.T);
            }
        }

        /// <summary>
        /// A built-up section has no single shape: null (see <see cref="GetOutlines"/>)
        /// </summary>
        /// <returns>null</returns>
        protected override Shape2d GetShape() => null;

        /// <summary>
        /// The mesh is not available: mesh the parts
        /// </summary>
        /// <exception cref="NotSupportedException">Always</exception>
        public override Mesh GetMesh(double meshSize = 0, bool initialMeshOnly = false, bool recombine = false, bool refine = false)
        {
            throw new NotSupportedException("A built-up section has no single mesh: mesh its parts (see Parts)");
        }

        /// <summary>
        /// The mesh is not available: mesh the parts
        /// </summary>
        /// <exception cref="NotSupportedException">Always</exception>
        protected override Mesh CreateMesh() => GetMesh();

        /// <summary>
        /// The exact plastic moduli are computed on the outlines of the parts (see <see cref="Section.Wpl1"/>)
        /// </summary>
        /// <returns>null: the plastic moduli use <see cref="GetOutlines"/></returns>
        internal override Shape2d GetPlasticShape() => null;

        /// <summary>
        /// The parts are connected at discrete points: the torsion of the built-up section is not solved (solve the parts)
        /// </summary>
        /// <param name="meshSize">Not used</param>
        /// <returns>Not solved</returns>
        public override SectionTorsionProperties CalculateTorsionProperties(double meshSize = 0) =>
            new SectionTorsionProperties("the parts of a built-up section are connected at discrete points: solve the parts");

        /// <summary>
        /// Sets the working of the corners of the parts from the type of the section (each part once; the parts without corners, e.g. circular,
        /// are not changed) and calculates their properties again. Before, <see cref="NotImplementedException"/>: a <see cref="Steel.SteelSection"/>
        /// of a built-up section could not be created
        /// </summary>
        /// <param name="sectionType">The type of the section</param>
        public override void SetEdgeTypeFromSteelType(SectionTypes sectionType)
        {
            var done = new List<Section>();
            foreach (Part part in _parts)
            {
                if (done.Any(s => ReferenceEquals(s, part.Section)))
                    continue;
                done.Add(part.Section);
                try
                {
                    part.Section.SetEdgeTypeFromSteelType(sectionType);
                }
                catch (NotImplementedException)
                {
                    continue;
                }
                part.Section.SetMechanicalProperties();
            }
        }

        /// <summary>
        /// The warping constant not available is NaN: it is not computed with the finite elements
        /// </summary>
        private protected override bool SolvesTorsionNumerically => false;

        /// <summary>
        /// Geometric properties exact (the parts); torsion constant (sum of the parts) and shear centre (see the class): approximate; warping
        /// constant: not available
        /// </summary>
        /// <param name="property">The property</param>
        /// <returns>The declared availability</returns>
        protected override PropertyAvailability DeclaredAvailability(SectionProperty property) =>
            Declared(property, PropertyAvailability.Approximate, PropertyAvailability.NotAvailable, PropertyAvailability.Approximate);

        /// <summary>The area: the sum of the areas of the parts</summary>
        /// <returns>The area</returns>
        protected override double CalculateArea() => _parts.Sum(p => p.Section.Area);

        /// <summary>The centroid: the one of the centroids of the parts</summary>
        /// <returns>The centroid</returns>
        protected override Point2d CalculateCentroid()
        {
            double sx = 0, sy = 0, area = 0;
            foreach (Part part in _parts)
            {
                Point2d c = part.Place(part.Section.Centroid.X, part.Section.Centroid.Y);
                sx += part.Section.Area * c.X;
                sy += part.Section.Area * c.Y;
                area += part.Section.Area;
            }
            return new Point2d(sx / area, sy / area);
        }

        /// <summary>The moment of inertia about X: the ones of the parts moved to the centroid</summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJxx() => _parts.Sum(p =>
            p.Section.Jxx + p.Section.Area * Math.Pow(p.Place(p.Section.Centroid.X, p.Section.Centroid.Y).Y - _centroid.Y, 2));

        /// <summary>The moment of inertia about Y: the ones of the parts moved to the centroid</summary>
        /// <returns>The moment of inertia</returns>
        protected override double CalculateJyy() => _parts.Sum(p =>
            p.Section.Jyy + p.Section.Area * Math.Pow(p.Place(p.Section.Centroid.X, p.Section.Centroid.Y).X - _centroid.X, 2));

        /// <summary>The product of inertia: the ones of the parts (with the sign changed by a single mirror) moved to the centroid</summary>
        /// <returns>The product of inertia</returns>
        protected override double CalculateJxy()
        {
            double j = 0;
            foreach (Part part in _parts)
            {
                Point2d c = part.Place(part.Section.Centroid.X, part.Section.Centroid.Y);
                double sign = part.MirrorX == part.MirrorY ? 1.0 : -1.0;
                j += sign * part.Section.Jxy + part.Section.Area * (c.X - _centroid.X) * (c.Y - _centroid.Y);
            }
            return SectionHelper.IsProductOfInertiaZero(_jxx, _jyy, j) ? 0.0 : j;
        }

        /// <summary>The torsion constant: the sum of the ones of the parts</summary>
        /// <returns>The torsion constant</returns>
        protected override double CalculateJt() => _parts.Sum(p => p.Section.Jt);

        /// <summary>The warping constant is not available: it depends on the connection of the parts</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateJw() => double.NaN;

        /// <summary>The shear centre, see the description of the class</summary>
        /// <returns>The shear centre</returns>
        protected override Point2d CalculateShearCenter()
        {
            double x = 0, y = 0, jxx = 0, jyy = 0;
            foreach (Part part in _parts)
            {
                Point2d s = part.Place(part.Section.ShearCenter.X, part.Section.ShearCenter.Y);
                x += part.Section.Jyy * s.X;
                y += part.Section.Jxx * s.Y;
                jxx += part.Section.Jxx;
                jyy += part.Section.Jyy;
            }
            return new Point2d(x / jyy, y / jxx);
        }

        /// <summary>The plastic modulus about X: the exact one of the outlines, computed at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWplX() => double.NaN;

        /// <summary>The plastic modulus about Y: the exact one of the outlines, computed at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWplY() => double.NaN;

        /// <summary>The plastic moduli about the principal axes, computed at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWpl1() => double.NaN;

        /// <summary>The plastic moduli about the principal axes, computed at the first access</summary>
        /// <returns><see cref="double.NaN"/></returns>
        protected override double CalculateWpl2() => double.NaN;

        /// <summary>
        /// The exact plastic modulus of the outlines of the parts
        /// </summary>
        /// <param name="angle">The direction of the bending axis from X</param>
        /// <returns>The plastic modulus</returns>
        private protected override double PlasticModulus(double angle) =>
            SectionHelper.CalculatePlasticModulus(GetOutlines().Cast<Shape>().ToArray(), _centroid, angle);

        /// <summary>Elastic modulus about X of the top fibre</summary>
        /// <returns>The modulus</returns>
        protected override double CalculateWelXMax() => _jxx / Math.Abs(Extreme(0.0, true, false));

        /// <summary>Elastic modulus about X of the bottom fibre</summary>
        /// <returns>The modulus</returns>
        protected override double CalculateWelXMin() => _jxx / Math.Abs(Extreme(0.0, false, false));

        /// <summary>Elastic modulus about Y of the right fibre</summary>
        /// <returns>The modulus</returns>
        protected override double CalculateWelYMax() => _jyy / Math.Abs(Extreme(0.0, true, true));

        /// <summary>Elastic modulus about Y of the left fibre</summary>
        /// <returns>The modulus</returns>
        protected override double CalculateWelYMin() => _jyy / Math.Abs(Extreme(0.0, false, true));

        /// <summary>Elastic modulus about the axis 1 of the fibre with the maximum coordinate y1</summary>
        /// <returns>The modulus</returns>
        protected override double CalculateWel1Max() => _j11 / Math.Abs(Extreme(_angleX1, true, false));

        /// <summary>Elastic modulus about the axis 1 of the fibre with the minimum coordinate y1</summary>
        /// <returns>The modulus</returns>
        protected override double CalculateWel1Min() => _j11 / Math.Abs(Extreme(_angleX1, false, false));

        /// <summary>Elastic modulus about the axis 2 of the fibre with the maximum coordinate x1</summary>
        /// <returns>The modulus</returns>
        protected override double CalculateWel2Max() => _j22 / Math.Abs(Extreme(_angleX1, true, true));

        /// <summary>Elastic modulus about the axis 2 of the fibre with the minimum coordinate x1</summary>
        /// <returns>The modulus</returns>
        protected override double CalculateWel2Min() => _j22 / Math.Abs(Extreme(_angleX1, false, true));

        /// <summary>Symmetric about the horizontal axis through the centroid if the outlines are</summary>
        /// <returns>True if symmetric</returns>
        protected override bool CalculateIsSymmetricAlongXLocalAxis() => IsSymmetric(false);

        /// <summary>Symmetric about the vertical axis through the centroid if the outlines are</summary>
        /// <returns>True if symmetric</returns>
        protected override bool CalculateIsSymmetricAlongYLocalAxis() => IsSymmetric(true);

        /// <summary>
        /// Serializes the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionBuiltUpVersion", 1);
            info.AddValue("Parts", _parts, typeof(Part[]));
        }

        /// <summary>
        /// The description of the section
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"Built-up section of {_parts.Length} parts";

        private IEnumerable<Point2d> Vertices()
        {
            foreach (Shape2d outline in GetOutlines())
                for (int i = 0; i < outline.Fill.Count; i++)
                    yield return new Point2d(outline.Fill[i].X, outline.Fill[i].Y);
        }

        private (double MinX, double MinY, double MaxX, double MaxY) Bounds()
        {
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            foreach (Point2d p in Vertices())
            {
                minX = Math.Min(minX, p.X);
                maxX = Math.Max(maxX, p.X);
                minY = Math.Min(minY, p.Y);
                maxY = Math.Max(maxY, p.Y);
            }
            return (minX, minY, maxX, maxY);
        }

        /// <summary>
        /// The extreme coordinate of the vertices in the axes through the centroid rotated by an angle
        /// </summary>
        /// <param name="angle">The angle of the axes from X</param>
        /// <param name="maximum">True for the maximum</param>
        /// <param name="along">True for the coordinate along the axis (x1), false for the one perpendicular to it (y1)</param>
        private double Extreme(double angle, bool maximum, bool along)
        {
            double cos = Math.Cos(angle), sin = Math.Sin(angle);
            double extreme = maximum ? double.MinValue : double.MaxValue;
            foreach (Point2d p in Vertices())
            {
                double dx = p.X - _centroid.X, dy = p.Y - _centroid.Y;
                double v = along ? dx * cos + dy * sin : dy * cos - dx * sin;
                extreme = maximum ? Math.Max(extreme, v) : Math.Min(extreme, v);
            }
            return extreme;
        }

        private bool IsSymmetric(bool aboutVertical)
        {
            Point2d[] points = Vertices().ToArray();
            var (minX, minY, maxX, maxY) = Bounds();
            double tolerance = 1e-9 * Math.Max(maxX - minX, maxY - minY);
            foreach (Point2d p in points)
            {
                double mx = aboutVertical ? 2 * _centroid.X - p.X : p.X, my = aboutVertical ? p.Y : 2 * _centroid.Y - p.Y;
                if (!points.Any(q => Math.Abs(q.X - mx) <= tolerance && Math.Abs(q.Y - my) <= tolerance))
                    return false;
            }
            return true;
        }
    }
}
