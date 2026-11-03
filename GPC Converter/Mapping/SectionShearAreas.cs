using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Sections;

namespace GPC.Converter
{
    /// <summary>Effective shear areas of a section from its GPC geometry, in mm², in the principal axes of the section.
    /// Elementary (Jourawski) shear-flow energy: A_s = I² / ∫ Q(t)²/b(t) dt, with chords b(t) normal to the shear direction.
    /// Exact for rectangles (5/6 A) and consistent for hollow and thin-walled outlines; it is not a warping-function solution.</summary>
    public static class SectionShearAreas
    {
        /// <summary>Shear areas in the principal 1 and 2 directions (Straus7 ipSA1/ipSA2).</summary>
        /// <exception cref="NotSupportedException">The section has no geometric outline.</exception>
        public static (double Area1, double Area2) Compute(ISectionShape section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            var outlines = section is SectionBuiltUp built ? built.GetOutlines().ToArray() : section.Shape == null ? new GPC.Geometry.Shape2d[0] : new[] { section.Shape };
            if (outlines.Length == 0) throw new NotSupportedException("ShearAreasRequireSectionGeometry: " + section.Name);
            var triangles = new List<double[]>();
            foreach (var outline in outlines)
            {
                var mesh = new Section(outline).GetMesh(initialMeshOnly: true);
                if (mesh == null || mesh.Faces.Count == 0) throw new ArgumentException("EmptySectionMesh: " + section.Name);
                foreach (var face in mesh.Faces)
                {
                    var v = mesh.GetFaceVertices(face);
                    for (int k = 1; k + 1 < v.Length; k++)
                        triangles.Add(new[] { v[0].Point.X, v[0].Point.Y, v[k].Point.X, v[k].Point.Y, v[k + 1].Point.X, v[k + 1].Point.Y });
                }
            }
            double area = 0, sx = 0, sy = 0;
            foreach (var t in triangles)
            {
                double a = Math.Abs((t[2] - t[0]) * (t[5] - t[1]) - (t[4] - t[0]) * (t[3] - t[1])) / 2;
                area += a; sx += a * (t[0] + t[2] + t[4]) / 3; sy += a * (t[1] + t[3] + t[5]) / 3;
            }
            if (!(area > 0)) throw new ArgumentException("EmptySectionMesh: " + section.Name);
            double cx = sx / area, cy = sy / area, ixx = 0, iyy = 0, ixy = 0;
            foreach (var t in triangles)
            {
                double a = Math.Abs((t[2] - t[0]) * (t[5] - t[1]) - (t[4] - t[0]) * (t[3] - t[1])) / 2;
                double x1 = t[0] - cx, x2 = t[2] - cx, x3 = t[4] - cx, y1 = t[1] - cy, y2 = t[3] - cy, y3 = t[5] - cy;
                ixx += a / 6 * (y1 * y1 + y2 * y2 + y3 * y3 + y1 * y2 + y2 * y3 + y3 * y1);
                iyy += a / 6 * (x1 * x1 + x2 * x2 + x3 * x3 + x1 * x2 + x2 * x3 + x3 * x1);
                ixy += a / 12 * (2 * (x1 * y1 + x2 * y2 + x3 * y3) + x1 * y2 + x2 * y1 + x1 * y3 + x3 * y1 + x2 * y3 + x3 * y2);
            }
            // Principal axes of the meshed outline itself (a rigid rotation leaves the shear areas unchanged);
            // axis 1 carries the maximum second moment, as ISectionShape.J11.
            double theta = 0.5 * Math.Atan2(2 * ixy, iyy - ixx);
            Func<double, double> about = angle => ixx * Math.Cos(angle) * Math.Cos(angle) + iyy * Math.Sin(angle) * Math.Sin(angle) - 2 * ixy * Math.Sin(angle) * Math.Cos(angle);
            if (about(theta + Math.PI / 2) > about(theta)) theta += Math.PI / 2;
            double c = Math.Cos(theta), s = Math.Sin(theta);
            // Principal coordinates about the mesh centroid: u1 along axis 1, u2 along axis 2.
            var principal = triangles.Select(t => Enumerable.Range(0, 3).SelectMany(i => new[] {
                c * (t[2 * i] - cx) + s * (t[2 * i + 1] - cy), -s * (t[2 * i] - cx) + c * (t[2 * i + 1] - cy) }).ToArray()).ToList();
            // The factors are relative to the meshed area, so a polygonal approximation of arcs does not bias the GPC area.
            double factor1 = Factor(principal, 0, area), factor2 = Factor(principal, 1, area);
            return (factor1 * section.Area, factor2 * section.Area);
        }

        private static readonly double[] GaussPoints = { -0.9324695142031521, -0.6612093864662645, -0.2386191860831969, 0.2386191860831969, 0.6612093864662645, 0.9324695142031521 };
        private static readonly double[] GaussWeights = { 0.1713244923791704, 0.3607615730481386, 0.4679139345726910, 0.4679139345726910, 0.3607615730481386, 0.1713244923791704 };

        /// <summary>A_s/A for shear along coordinate <paramref name="axis"/> (0: u1, 1: u2) of triangles stored as (u1,u2) triplets.</summary>
        private static double Factor(List<double[]> triangles, int axis, double area)
        {
            int other = 1 - axis;
            var levels = triangles.SelectMany(t => new[] { t[axis], t[2 + axis], t[4 + axis] }).OrderBy(v => v).ToList();
            double scale = Math.Max(Math.Abs(levels[0]), Math.Abs(levels[levels.Count - 1]));
            var breaks = new List<double> { levels[0] };
            foreach (double v in levels) if (v - breaks[breaks.Count - 1] > 1e-12 * scale) breaks.Add(v);
            int n = breaks.Count - 1;
            if (n < 1) throw new ArgumentException("DegenerateSectionMesh");
            // The total chord is linear inside each interval free of vertices: sample it at the thirds of the interval.
            var low = new double[n]; var high = new double[n];
            foreach (var t in triangles)
            {
                var p = new[] { (A: t[axis], B: t[other]), (A: t[2 + axis], B: t[2 + other]), (A: t[4 + axis], B: t[4 + other]) }.OrderBy(q => q.A).ToArray();
                if (p[2].A - p[0].A <= 1e-12 * scale) continue;
                int first = Nearest(breaks, p[0].A), last = Nearest(breaks, p[2].A);
                for (int k = first; k < last && k < n; k++)
                {
                    double h = breaks[k + 1] - breaks[k];
                    low[k] += Chord(p, breaks[k] + h / 3); high[k] += Chord(p, breaks[k] + 2 * h / 3);
                }
            }
            Func<int, double, double> width = (k, x) =>
            {
                double h = breaks[k + 1] - breaks[k], ratio = (x - breaks[k] - h / 3) / (h / 3);
                return Math.Max(0, low[k] + (high[k] - low[k]) * ratio);
            };
            // First moment above each level, integrated from the top; the integrand s*b(s) is quadratic (two-point Gauss is exact).
            var above = new double[n + 1];
            for (int k = n - 1; k >= 0; k--) above[k] = above[k + 1] + Moment(k, breaks[k], breaks[k + 1], width);
            double inertia = 0, energy = 0, widest = Enumerable.Range(0, n).Max(k => Math.Max(low[k], high[k]));
            for (int k = 0; k < n; k++)
            {
                double a = breaks[k], b = breaks[k + 1], mid = (a + b) / 2, half = (b - a) / 2;
                for (int g = 0; g < GaussPoints.Length; g++)
                {
                    double x = mid + half * GaussPoints[g], w = width(k, x), q = above[k + 1] + Moment(k, x, b, width);
                    inertia += GaussWeights[g] * half * x * x * w;
                    // A chord without material carries no shear flow: the parts on both sides are taken as connected elsewhere.
                    if (w > 1e-9 * widest) energy += GaussWeights[g] * half * q * q / w;
                }
            }
            if (!(energy > 0)) throw new ArgumentException("DegenerateSectionShearFlow");
            return inertia * inertia / (area * energy);
        }

        /// <summary>Index of the merged level closest to a vertex coordinate.</summary>
        private static int Nearest(List<double> breaks, double value)
        {
            int index = breaks.BinarySearch(value); if (index >= 0) return index;
            index = ~index; if (index == 0) return 0; if (index == breaks.Count) return index - 1;
            return value - breaks[index - 1] <= breaks[index] - value ? index - 1 : index;
        }

        private static double Moment(int k, double from, double to, Func<int, double, double> width)
        {
            double mid = (from + to) / 2, half = (to - from) / 2, g = half / Math.Sqrt(3);
            return half * ((mid - g) * width(k, mid - g) + (mid + g) * width(k, mid + g));
        }

        /// <summary>Length of the intersection of a triangle (vertices sorted by A) with the line A = x, inside the triangle.</summary>
        private static double Chord((double A, double B)[] p, double x)
        {
            if (x <= p[0].A || x >= p[2].A) return 0;
            double longEdge = p[0].B + (p[2].B - p[0].B) * (x - p[0].A) / (p[2].A - p[0].A);
            double shortEdge = x < p[1].A
                ? p[0].B + (p[1].B - p[0].B) * (x - p[0].A) / (p[1].A - p[0].A)
                : p[1].B + (p[2].B - p[1].B) * (x - p[1].A) / (p[2].A - p[1].A);
            return Math.Abs(longEdge - shortEdge);
        }
    }
}
