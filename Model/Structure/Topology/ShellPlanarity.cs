using System;
using System.Collections.Generic;
using GPC.Geometry;

namespace GPC.Model.Structure.Topology
{
    /// <summary>
    /// Planarity of the corners of a three- or four-node shell (5/11/2026). The corners are planar when their largest distance from the mean
    /// plane (Newell normal through the centroid, the measure of <see cref="Polygon3d"/>) is within <see cref="ToleranceMm(IReadOnlyList{Point3d}, double)"/>:
    /// the larger of an absolute floor, <see cref="GeometryBase.Tolerance"/> by default, and <see cref="RelativeTolerance"/> times the largest
    /// distance between two corners.
    /// <para>Why: the coordinates a solver exports carry a round-off of about 1 µm on a model in metres. With the absolute 1e-4 mm alone,
    /// 575 of the 2507 planar quads of a real Straus7 model (P118-25-D-FM01, corners up to 9 m from the origin, largest deviation 0.24 µm,
    /// 6.8e-7 of the plate size) were refused as not planar and the whole import was rejected. A relative bound accepts that round-off for
    /// any plate larger than about 0.1 m and still refuses an actual warping, for example a corner 1 mm out of the plane of a 1 m plate.
    /// <see cref="GeometryBase.Tolerance"/> itself does not change.</para>
    /// </summary>
    public static class ShellPlanarity
    {
        /// <summary>The relative bound: the deviation from the mean plane per unit of the largest distance between two corners.</summary>
        public const double RelativeTolerance = 1e-5;

        /// <summary>The largest distance between two corners, mm.</summary>
        public static double LongestMm(IReadOnlyList<Point3d> corners)
        {
            if (corners == null) throw new ArgumentNullException(nameof(corners));
            double longest = 0;
            for (int a = 0; a < corners.Count; a++)
                for (int b = a + 1; b < corners.Count; b++)
                    longest = Math.Max(longest, Distance(corners[a], corners[b]));
            return longest;
        }

        /// <summary>The planarity tolerance of these corners, mm: the larger of <paramref name="absoluteMm"/> and
        /// <see cref="RelativeTolerance"/> times <see cref="LongestMm"/>.</summary>
        public static double ToleranceMm(IReadOnlyList<Point3d> corners, double absoluteMm = GeometryBase.Tolerance)
        {
            if (double.IsNaN(absoluteMm) || double.IsInfinity(absoluteMm) || absoluteMm < 0) throw new ArgumentOutOfRangeException(nameof(absoluteMm));
            return Math.Max(absoluteMm, RelativeTolerance * LongestMm(corners));
        }

        /// <summary>The largest distance of a corner from the mean plane, mm (zero for three corners; positive infinity for corners
        /// without an area, whose plane is undefined).</summary>
        public static double DeviationMm(IReadOnlyList<Point3d> corners)
        {
            if (corners == null) throw new ArgumentNullException(nameof(corners));
            if (corners.Count <= 3) return 0;
            double nx = 0, ny = 0, nz = 0, cx = 0, cy = 0, cz = 0;
            Point3d origin = corners[0];
            for (int i = 1; i < corners.Count - 1; i++)
            {
                double ax = corners[i].X - origin.X, ay = corners[i].Y - origin.Y, az = corners[i].Z - origin.Z;
                double bx = corners[i + 1].X - origin.X, by = corners[i + 1].Y - origin.Y, bz = corners[i + 1].Z - origin.Z;
                nx += ay * bz - az * by; ny += az * bx - ax * bz; nz += ax * by - ay * bx;
            }
            double length = Math.Sqrt(nx * nx + ny * ny + nz * nz), longest = LongestMm(corners);
            if (!(length > 1e-12 * longest * longest))
            {
                // No Newell area (a self-crossed quad, whose two triangles cancel): the normal of the largest corner triangle, as ShellGeometry,
                // so that a planar bow-tie stays planar and is refused for its winding, not for its planarity.
                length = 0;
                for (int a = 0; a < corners.Count; a++) for (int b = a + 1; b < corners.Count; b++) for (int c = b + 1; c < corners.Count; c++)
                {
                    double ax = corners[b].X - corners[a].X, ay = corners[b].Y - corners[a].Y, az = corners[b].Z - corners[a].Z;
                    double bx = corners[c].X - corners[a].X, by = corners[c].Y - corners[a].Y, bz = corners[c].Z - corners[a].Z;
                    double tx = ay * bz - az * by, ty = az * bx - ax * bz, tz = ax * by - ay * bx, t = Math.Sqrt(tx * tx + ty * ty + tz * tz);
                    if (t > length) { length = t; nx = tx; ny = ty; nz = tz; }
                }
                if (!(length > 1e-12 * longest * longest)) return double.PositiveInfinity; // aligned corners: no plane
            }
            nx /= length; ny /= length; nz /= length;
            foreach (var p in corners) { cx += p.X; cy += p.Y; cz += p.Z; }
            cx /= corners.Count; cy /= corners.Count; cz /= corners.Count;
            double deviation = 0;
            foreach (var p in corners) deviation = Math.Max(deviation, Math.Abs(nx * (p.X - cx) + ny * (p.Y - cy) + nz * (p.Z - cz)));
            return deviation;
        }

        /// <summary>True when the corners are planar within <see cref="ToleranceMm(IReadOnlyList{Point3d}, double)"/>.</summary>
        public static bool IsPlanar(IReadOnlyList<Point3d> corners, double absoluteMm = GeometryBase.Tolerance) =>
            DeviationMm(corners) <= ToleranceMm(corners, absoluteMm);

        private static double Distance(Point3d a, Point3d b)
        {
            double dx = b.X - a.X, dy = b.Y - a.Y, dz = b.Z - a.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
