using GPC.Geometry;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Sections.Steel
{
    /// <summary>
    /// The part of a steel section inside the concrete of a composite section: the region replaced by the steel in the homogenized properties
    /// (see <see cref="SteelSectionPosition.ReplacedConcrete"/>)
    /// </summary>
    internal sealed class ConcreteOverlap
    {
        /// <summary>
        /// The relative tolerance on the area: a steel section with the overlap within it from its area is inside, within it from zero is outside
        /// </summary>
        private const double Tolerance = 1e-6;

        private ConcreteOverlap(double steelArea, double area, Shape[] region)
        {
            SteelArea = steelArea;
            Area = area;
            Region = region;
        }

        /// <summary>
        /// The area of the outline of the steel section
        /// </summary>
        internal double SteelArea { get; }

        /// <summary>
        /// The area of the steel inside the concrete
        /// </summary>
        internal double Area { get; }

        /// <summary>
        /// The region of the steel inside the concrete
        /// </summary>
        internal IReadOnlyList<Shape> Region { get; }

        /// <summary>
        /// True if the steel section is entirely inside the concrete
        /// </summary>
        internal bool IsInside => Area >= (1 - Tolerance) * SteelArea;

        /// <summary>
        /// True if the steel section is outside the concrete (they can touch)
        /// </summary>
        internal bool IsOutside => Area <= Tolerance * SteelArea;

        /// <summary>
        /// The overlap of the outlines of a steel section with the concrete (boolean intersection with Clipper)
        /// </summary>
        /// <param name="steel">The outlines of the steel section in the coordinates of the concrete section</param>
        /// <param name="concrete">The shape of the concrete</param>
        /// <returns>The overlap; null if the steel or the concrete have no region or the intersection fails</returns>
        internal static ConcreteOverlap Calculate(IReadOnlyList<Shape2d> steel, Shape2d concrete)
        {
            if (steel is null || steel.Count == 0 || concrete?.Fill is null || concrete.Fill.Count < 3)
                return null;

            double steelArea = steel.Sum(AreaOf);
            if (!(steelArea > 0))
                return null;

            // Boxes apart: the intersection is empty, the result of Clipper, without the boolean operation (for example the steel of a composite
            // girder under its slab: before, for every section of every check)
            if (Apart(steel, concrete))
                return new ConcreteOverlap(steelArea, 0, new Shape[0]);

            // Clipper ignores the childs of the shapes: they are passed as separate shapes (even-odd rule)
            Shape[] subject = steel.SelectMany(Flatten).ToArray();
            Shape[] clip = Flatten(concrete).ToArray();
            if (!Shape.Intersection(subject, clip, out Shape[] region) || region is null)
                return null;

            return new ConcreteOverlap(steelArea, region.Sum(AreaOf), region);
        }

        /// <summary>
        /// The integrals of the overlap region about a pole
        /// </summary>
        internal void Integrals(Point2d pole, out double area, out double sx, out double sy, out double ixx, out double iyy, out double ixy)
        {
            area = sx = sy = ixx = iyy = ixy = 0;
            foreach (Shape shape in Region)
            {
                SectionHelper.IntegrateShape(shape, pole.X, pole.Y, out double a, out double x, out double y, out double xx, out double yy, out double xy);
                area += a;
                sx += x;
                sy += y;
                ixx += xx;
                iyy += yy;
                ixy += xy;
            }
        }

        /// <summary>
        /// True if the bounding boxes of the steel and of the concrete (all their vertices) are apart by more than a relative 1e-9: the shapes
        /// can not overlap, also after the change of coordinate system of the boolean operation (a rotation: errors of some ulp). Touching boxes
        /// and coordinates that are not finite are left to Clipper
        /// </summary>
        private static bool Apart(IReadOnlyList<Shape2d> steel, Shape2d concrete)
        {
            double[] s = { double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity };
            double[] c = { double.PositiveInfinity, double.PositiveInfinity, double.NegativeInfinity, double.NegativeInfinity };
            for (int i = 0; i < steel.Count; i++)
                if (!Box(steel[i], s))
                    return false;
            if (!Box(concrete, c))
                return false;
            double size = 1;
            foreach (double v in s) size = System.Math.Max(size, System.Math.Abs(v));
            foreach (double v in c) size = System.Math.Max(size, System.Math.Abs(v));
            double gap = 1e-9 * size;
            return s[2] < c[0] - gap || s[0] > c[2] + gap || s[3] < c[1] - gap || s[1] > c[3] + gap;
        }

        /// <summary>
        /// Extends the box (min x, min y, max x, max y) with the vertices of the fill, of the holes and of the childs of a shape
        /// </summary>
        /// <returns>False if a vertex is not finite</returns>
        private static bool Box(Shape shape, double[] box)
        {
            if (shape is null)
                return true;
            if (!Box(shape.Fill, box))
                return false;
            if (shape.Holes != null)
                foreach (Polygon3d hole in shape.Holes)
                    if (!Box(hole, box))
                        return false;
            if (shape.Childs != null)
                foreach (Shape child in shape.Childs)
                    if (!Box(child, box))
                        return false;
            return true;
        }

        private static bool Box(Polygon3d polygon, double[] box)
        {
            if (polygon is null)
                return true;
            for (int i = 0; i < polygon.Count; i++)
            {
                double x = polygon[i].X, y = polygon[i].Y;
                if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y))
                    return false;
                box[0] = System.Math.Min(box[0], x);
                box[1] = System.Math.Min(box[1], y);
                box[2] = System.Math.Max(box[2], x);
                box[3] = System.Math.Max(box[3], y);
            }
            return true;
        }

        private static double AreaOf(Shape shape)
        {
            if (shape?.Fill is null || shape.Fill.Count < 3)
                return 0;
            Point3d origin = shape.Fill[0];
            SectionHelper.IntegrateShape(shape, origin.X, origin.Y, out double area, out _, out _, out _, out _, out _);
            return area;
        }

        /// <summary>
        /// A shape as a <see cref="Shape2d"/> (the result of the boolean operations), with its holes and its childs
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <returns>The new shape</returns>
        internal static Shape2d ToShape2d(Shape shape)
        {
            Polygon2d Plane(Polygon3d p) => new Polygon2d(Enumerable.Range(0, p.Count).Select(i => new Point2d(p[i].X, p[i].Y)).ToArray());
            Polygon2d[] holes = shape.Holes?.Where(h => h != null && h.Count >= 3).Select(Plane).ToArray();
            Shape2d[] childs = shape.Childs?.Where(c => c?.Fill != null && c.Fill.Count >= 3).Select(ToShape2d).ToArray();
            return new Shape2d(Plane(shape.Fill), holes != null && holes.Length > 0 ? holes : null, childs != null && childs.Length > 0 ? childs : null);
        }

        /// <summary>
        /// A shape without its childs, followed by its childs (recursively): the input of the boolean operations, that ignore the childs
        /// </summary>
        internal static IEnumerable<Shape> Flatten(Shape shape)
        {
            if (shape?.Fill is null || shape.Fill.Count < 3)
                yield break;

            Polygon2d fill = new Polygon2d(Enumerable.Range(0, shape.Fill.Count).Select(i => new Point2d(shape.Fill[i].X, shape.Fill[i].Y)).ToArray());
            Polygon2d[] holes = shape.Holes?.Where(h => h != null && h.Count >= 3)
                .Select(h => new Polygon2d(Enumerable.Range(0, h.Count).Select(i => new Point2d(h[i].X, h[i].Y)).ToArray())).ToArray();
            yield return new Shape2d(fill, holes != null && holes.Length > 0 ? holes : null);

            if (shape.Childs != null)
            {
                foreach (Shape child in shape.Childs)
                {
                    foreach (Shape part in Flatten(child))
                        yield return part;
                }
            }
        }
    }
}
