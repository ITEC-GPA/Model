using System;
using GPC.Geometry;
using GPC.Model.PostProcessing;

namespace GPC.Converter
{
    internal static class Frames
    {
        /// <summary>A new coordinate system with copies of the axis vectors: the CoordinateSystem constructor unitizes its vectors in place,
        /// so passing the vectors of a Model coordinate system would alter the Model (and its analysis fingerprint).</summary>
        public static CoordinateSystem At(CoordinateSystem axes, Point3d origin) => new CoordinateSystem(new Point3d(origin.X, origin.Y, origin.Z),
            new Vector3d(axes.V1.X, axes.V1.Y, axes.V1.Z), new Vector3d(axes.V2.X, axes.V2.Y, axes.V2.Z), new Vector3d(axes.V3.X, axes.V3.Y, axes.V3.Z));
    }

    /// <summary>MIDAS unit labels and element axes shared by the MCT reader and the Civil NX API profile.</summary>
    internal static class MidasConventions
    {
        public static double? ForceFactor(string unit)
        {
            switch (unit?.Trim().ToUpperInvariant())
            {
                case "N": return 1; case "KN": return 1000; case "KGF": return 9.80665; case "TONF": return 9806.65;
                case "LBF": return 4.4482216152605; case "KIPS": return 4448.2216152605;
                default: return null;
            }
        }

        public static double? LengthFactor(string unit)
        {
            switch (unit?.Trim().ToUpperInvariant())
            {
                case "MM": return 1; case "CM": return 10; case "M": return 1000; case "IN": return 25.4; case "FT": return 304.8;
                default: return null;
            }
        }

        /// <summary>Beam element axes. MIDAS x = I to J, z at beta zero = projected GCS Z (GCS X for an exactly vertical member),
        /// y completes the right-handed triad; beta rotates about x. GPC (V1,V2,V3) = MIDAS (y,z,x).</summary>
        public static CoordinateSystem BeamAxes(Point3d i, Point3d j, double betaDegrees)
        {
            Vector3d x = j - i; var norm = Axes.Length(x);
            if (norm <= 1e-9 || double.IsInfinity(norm)) throw new ArgumentException("Degenerate beam.");
            x = x / norm;
            var z = x.X == 0 && x.Y == 0 ? new Vector3d(1, 0, 0) : new Vector3d(0, 0, 1) - x * x.Z;
            if (Axes.Length(z) <= 1e-10) throw new ArgumentException("Near-vertical beta convention requires an explicit solver axis export.");
            z = z / Axes.Length(z);
            return Axes.Beam(i, j, z.CrossProduct(x), (betaDegrees % 360) * Math.PI / 180);
        }

        /// <summary>Plate element axes (MIDAS Civil Analysis Manual, plate elements): z normal by the right-hand rule on the node order,
        /// x from the mid-point of N1-N4 to the mid-point of N2-N3 (quadrilateral) or parallel to N1-N2 (triangle), origin at the centre.</summary>
        public static CoordinateSystem PlateAxes(Point3d[] p)
        {
            if (p == null || (p.Length != 3 && p.Length != 4)) throw new ArgumentException("Three or four plate nodes required.");
            Vector3d normal = p.Length == 3 ? ((Vector3d)(p[1] - p[0])).CrossProduct(p[2] - p[0]) : ((Vector3d)(p[2] - p[0])).CrossProduct(p[3] - p[1]);
            double n = Axes.Length(normal);
            if (n <= 1e-9 || double.IsInfinity(n)) throw new ArgumentException("Degenerate plate.");
            var z = normal / n;
            Vector3d x = p.Length == 3 ? (Vector3d)(p[1] - p[0]) : ((Vector3d)(p[1] - p[0]) + (Vector3d)(p[2] - p[3])) / 2;
            x = x - z * Axes.Dot(x, z);
            double l = Axes.Length(x);
            if (l <= 1e-9) throw new ArgumentException("Degenerate plate x-axis.");
            x = x / l;
            double cx = 0, cy = 0, cz = 0;
            foreach (var point in p) { cx += point.X; cy += point.Y; cz += point.Z; }
            var axes = new CoordinateSystem(new Point3d(cx / p.Length, cy / p.Length, cz / p.Length), x, z.CrossProduct(x), z);
            Axes.Validate(axes); return axes;
        }
    }
}
