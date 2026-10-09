using System;
using GPC.Geometry;

namespace GPC.Model.Core.Coordinates
{
    public static class Axes
    {
        public static double Dot(Vector3d a, Vector3d b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        public static double Length(Vector3d a) => Math.Sqrt(Dot(a, a));
        public static bool IsFinite(Point3d p) => p != null && Finite(p.X) && Finite(p.Y) && Finite(p.Z);
        private static bool Finite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
        public static void Validate(CoordinateSystem axes)
        {
            if (axes == null || !IsFinite(axes.Origin)) throw new ArgumentException("Invalid coordinate system.");
            var x = axes.V1; var y = axes.V2; var z = axes.V3;
            double error = Math.Abs(Dot(x, x) - 1) + Math.Abs(Dot(y, y) - 1) + Math.Abs(Dot(z, z) - 1)
                + Math.Abs(Dot(x, y)) + Math.Abs(Dot(x, z)) + Math.Abs(Dot(y, z)) + Math.Abs(Dot(x.CrossProduct(y), z) - 1);
            if (!Finite(error) || error > 1e-8) throw new ArgumentException("A right-handed orthonormal basis is required.");
        }
        /// <summary>GPC section axes: 3 = I to J. The supplied transverse direction is mandatory, including vertical beams.</summary>
        public static CoordinateSystem Beam(Point3d i, Point3d j, Vector3d direction1, double sectionRotationRadians = 0)
        {
            NumericGuard.Finite(sectionRotationRadians, nameof(sectionRotationRadians));
            Vector3d z = j - i; double l = Length(z);
            if (l <= 1e-9 || double.IsNaN(l) || double.IsInfinity(l)) throw new ArgumentException("Degenerate beam.");
            z = z / l;
            var x = direction1 - z * Dot(direction1, z); double xl = Length(x);
            if (xl <= 1e-9 || double.IsNaN(xl)) throw new ArgumentException("Missing/parallel transverse direction.");
            x = x / xl; var y = z.CrossProduct(x);
            var rotated = x * Math.Cos(sectionRotationRadians) + y * Math.Sin(sectionRotationRadians);
            var result = new CoordinateSystem(i, rotated, z.CrossProduct(rotated), z);
            Validate(result); return result;
        }
    }
}
