using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Core.Coordinates;
using GPC.Model.Elements;

namespace GPC.Model.Structure.Topology
{
    /// <summary>Detached geometry of a linear triangle or a bilinear Q4 patch in mm.
    /// This is a geometric map, not an assertion that a solver's plate formulation supports warping.
    /// Q4 corners follow (-1,-1), (1,-1), (1,1), (-1,1); triangle coordinates are barycentric (r,s).
    /// The normal follows connectivity. Axes are right handed, with axis 1 along the first tangent.</summary>
    public sealed class ShellGeometry
    {
        private readonly Point3d[] points;
        private readonly Lazy<double> area;
        public int NodeCount => points.Length;
        public double CharacteristicLength { get; }
        /// <summary>Largest distance to the plane of the largest corner triangle, in mm.</summary>
        public double PlanarityDeviation { get; }
        public double RelativeWarpage => PlanarityDeviation / CharacteristicLength;
        /// <summary>Surface integral of the bilinear patch, mm². Converged composite Gauss quadrature.</summary>
        public double Area => area.Value;

        public ShellGeometry(AreaElement element) : this((element ?? throw new ArgumentNullException(nameof(element))).Points) { }

        public ShellGeometry(IEnumerable<Point3d> vertices)
        {
            points = (vertices ?? throw new ArgumentNullException(nameof(vertices))).Select(p =>
                Axes.IsFinite(p) ? new Point3d(p.X, p.Y, p.Z) : throw new ArgumentException("NonFiniteShellCoordinate")).ToArray();
            if (points.Length != 3 && points.Length != 4) throw new ArgumentException("ThreeOrFourShellNodesRequired");
            double longest = 0, largest = 0;
            Vector3d reference = null; Point3d origin = null;
            for (int a = 0; a < points.Length; a++) for (int b = a + 1; b < points.Length; b++)
            {
                longest = Math.Max(longest, Axes.Length(points[b] - points[a]));
                for (int c = b + 1; c < points.Length; c++)
                {
                    Vector3d ab = points[b] - points[a], ac = points[c] - points[a];
                    var normal = ab.CrossProduct(ac);
                    double magnitude = Axes.Length(normal);
                    if (magnitude > largest) { largest = magnitude; reference = normal; origin = points[a]; }
                }
            }
            CharacteristicLength = NumericGuard.Finite(longest, "shellLength");
            if (longest <= 0 || !Finite(largest) || largest <= 1e-12 * longest * longest)
                throw new ArgumentException("DegenerateShell");
            PlanarityDeviation = points.Max(p => Math.Abs(Axes.Dot(p - origin, reference)) / largest);
            // The Q4 Jacobian vector is affine in r,s. Positive projection at every corner
            // implies a nonzero, consistently oriented Jacobian everywhere in the patch.
            var center = Jacobian(0, 0);
            double size = Axes.Length(center);
            if (!Finite(size) || size <= 1e-12 * longest * longest) throw new ArgumentException("DegenerateShell");
            if (points.Length == 4)
                foreach (double r in new[] { -1.0, 1.0 }) foreach (double s in new[] { -1.0, 1.0 })
                    if (Axes.Dot(Jacobian(r, s), center) <= 1e-12 * longest * longest * size)
                        throw new ArgumentException("FoldedOrDegenerateShell");
            area = new Lazy<double>(() => IntegrateArea());
        }

        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        public bool IsPlanar(double toleranceMm)
        {
            if (NumericGuard.Finite(toleranceMm, nameof(toleranceMm)) < 0) throw new ArgumentOutOfRangeException(nameof(toleranceMm));
            return PlanarityDeviation <= toleranceMm;
        }

        private void Parameters(double r, double s)
        {
            NumericGuard.Finite(r, nameof(r)); NumericGuard.Finite(s, nameof(s));
            if (points.Length == 3 ? r < 0 || s < 0 || r + s > 1 : Math.Abs(r) > 1 || Math.Abs(s) > 1)
                throw new ArgumentOutOfRangeException("Natural coordinates outside shell.");
        }
        private void Tangents(double r, double s, out Vector3d dr, out Vector3d ds)
        {
            var b = points[1] - points[0]; var c = points[2] - points[0];
            if (points.Length == 3) { dr = b; ds = c; return; }
            var d = points[3] - points[0];
            dr = (b * (1 - s) + (c - d) * (1 + s)) / 4;
            ds = (d * (1 - r) + (c - b) * (1 + r)) / 4;
        }
        private Vector3d Jacobian(double r, double s) { Tangents(r, s, out var dr, out var ds); return dr.CrossProduct(ds); }
        public Point3d PointAt(double r, double s)
        {
            Parameters(r, s);
            var b = points[1] - points[0]; var c = points[2] - points[0];
            return points.Length == 3 ? points[0] + b * r + c * s
                : points[0] + b * ((1 + r) * (1 - s) / 4) + c * ((1 + r) * (1 + s) / 4) + (points[3] - points[0]) * ((1 - r) * (1 + s) / 4);
        }
        public Vector3d NormalAt(double r, double s)
        {
            Parameters(r, s); var normal = Jacobian(r, s); return normal / Axes.Length(normal);
        }
        public CoordinateSystem AxesAt(double r, double s)
        {
            Parameters(r, s); Tangents(r, s, out var tangent, out var unused);
            var x = tangent / Axes.Length(tangent); var z = NormalAt(r, s);
            return new CoordinateSystem(PointAt(r, s), x, z.CrossProduct(x), z);
        }

        /// <summary>Composite 2x2 Gauss integration. Relative convergence is measured between
        /// successive grids; failure at the declared subdivision limit is explicit.</summary>
        public double IntegrateArea(double relativeTolerance = 1e-8, int maximumSubdivisions = 64)
        {
            if (NumericGuard.Finite(relativeTolerance, nameof(relativeTolerance)) <= 0 || relativeTolerance >= 1)
                throw new ArgumentOutOfRangeException(nameof(relativeTolerance));
            if (maximumSubdivisions < 2 || maximumSubdivisions > 256 || (maximumSubdivisions & (maximumSubdivisions - 1)) != 0)
                throw new ArgumentOutOfRangeException(nameof(maximumSubdivisions), "A power of two from 2 through 256 is required.");
            if (points.Length == 3) return Axes.Length(Jacobian(0, 0)) / 2;
            double previous = 0;
            for (int n = 1; n <= maximumSubdivisions; n *= 2)
            {
                double sum = 0, h = 1.0 / n, q = h / Math.Sqrt(3);
                for (int a = 0; a < n; a++) for (int b = 0; b < n; b++)
                    foreach (double u in new[] { -q, q }) foreach (double v in new[] { -q, q })
                        sum += Axes.Length(Jacobian(-1 + (2 * a + 1) * h + u, -1 + (2 * b + 1) * h + v)) * h * h;
                NumericGuard.Finite(sum, "shellArea");
                if (n > 1 && Math.Abs(sum - previous) <= relativeTolerance * sum) return sum;
                previous = sum;
            }
            throw new InvalidOperationException("ShellAreaQuadratureNotConverged");
        }
    }
}
