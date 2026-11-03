using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Sections;

namespace GPC.Converter
{
    /// <summary>Effective (energy) shear areas of a section from its GPC geometry, in mm², in the principal axes of the section.
    /// Saint-Venant flexure solved by finite elements (Trefftz/Pilkey energy method): for a shear force V along a principal
    /// axis y, with x the other principal axis and I = ∫y² dA, the shear stresses are τ = (V/I)(∇χ + a), a = (0, ν x²/(2(1+ν))),
    /// where ∇²χ = −y in the section and τ·n = 0 on every contour (outer and holes). A_s = V²/∫τ² dA = I²/∫|∇χ + a|² dA.
    /// Linear triangles on a size-controlled section mesh; ν is the Poisson ratio of the material.</summary>
    public static class SectionShearAreas
    {
        /// <summary>Minimum number of triangles of the section mesh (size-controlled, then uniformly subdivided if needed).</summary>
        public const int MinimumTriangles = 20000;

        /// <summary>Shear areas in the principal 1 and 2 directions (Straus7 ipSA1/ipSA2).</summary>
        /// <exception cref="NotSupportedException">The section has no geometric outline or its parts are not connected.</exception>
        public static (double Area1, double Area2) Compute(ISectionShape section, double poissonRatio)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            if (double.IsNaN(poissonRatio) || poissonRatio <= -1 || poissonRatio >= .5) throw new ArgumentOutOfRangeException(nameof(poissonRatio));
            var mesh = SectionMesh.Create(section);
            var (u1, u2) = mesh.Principal(); var u = new[] { u1, u2 };
            // The Laplacian does not depend on the shear direction: one factorization, two right-hand sides.
            var solver = new EnvelopeCholesky(mesh, u);
            // Shear along axis 1: bending about axis 2 (I = ∫u1²), transverse coordinate u2; and conversely.
            double factor1 = Factor(mesh, u, solver, 0, poissonRatio), factor2 = Factor(mesh, u, solver, 1, poissonRatio);
            return (factor1 * section.Area, factor2 * section.Area);
        }

        /// <summary>A_s/A for shear along principal coordinate u[along] (the other one transverse).</summary>
        private static double Factor(SectionMesh mesh, double[][] u, EnvelopeCholesky solver, int along, double nu)
        {
            int across = 1 - along; double[] x = u[across], y = u[along]; double c = nu / (2 * (1 + nu)), inertia = 0;
            var rhs = new double[x.Length];
            foreach (var t in mesh.Triangles)
            {
                var g = Gradients(u[0], u[1], t, out double area);
                double xx = Quadrature(x, y, t, area, (px, py) => px * px); inertia += Quadrature(x, y, t, area, (px, py) => py * py);
                // ∫ y N_a − ∫ a_y ∂N_a/∂y, a_y = c x², with ∫ y N_a = A (2 y_a + y_b + y_c)/12 for linear y.
                for (int a = 0; a < 3; a++) rhs[t[a]] += area * (2 * y[t[a]] + y[t[(a + 1) % 3]] + y[t[(a + 2) % 3]]) / 12 - c * xx * g[a, along];
            }
            var chi = solver.Solve(rhs);
            double energy = 0;
            foreach (var t in mesh.Triangles)
            {
                var g = Gradients(u[0], u[1], t, out double area);
                double gx = 0, gy = 0; for (int a = 0; a < 3; a++) { gx += g[a, across] * chi[t[a]]; gy += g[a, along] * chi[t[a]]; }
                energy += Quadrature(x, y, t, area, (px, py) => gx * gx + (gy + c * px * px) * (gy + c * px * px));
            }
            if (!(energy > 0)) throw new ArgumentException("DegenerateSectionShearFlow");
            return inertia * inertia / (mesh.Area * energy);
        }

        /// <summary>Linear-triangle gradients (∂N/∂u1, ∂N/∂u2) and area.</summary>
        private static double[,] Gradients(double[] x, double[] y, int[] t, out double area)
        {
            double x1 = x[t[0]], y1 = y[t[0]], x2 = x[t[1]], y2 = y[t[1]], x3 = x[t[2]], y3 = y[t[2]];
            double det = (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1); area = Math.Abs(det) / 2;
            return new[,] { { (y2 - y3) / det, (x3 - x2) / det }, { (y3 - y1) / det, (x1 - x3) / det }, { (y1 - y2) / det, (x2 - x1) / det } };
        }

        // Dunavant 6-point rule, exact for polynomials of degree 4 on a triangle (barycentric a, a, 1 - 2a).
        private static readonly double[] RuleA = { 0.445948490915965, 0.091576213509771 };
        private static readonly double[] RuleW = { 0.223381589678011, 0.109951743655322 };
        private static double Quadrature(double[] x, double[] y, int[] t, double area, Func<double, double, double> f)
        {
            double sum = 0;
            for (int r = 0; r < 2; r++)
            {
                double a = RuleA[r], b = 1 - 2 * a;
                for (int k = 0; k < 3; k++)
                {
                    double l0 = k == 0 ? b : a, l1 = k == 1 ? b : a, l2 = k == 2 ? b : a;
                    sum += RuleW[r] * f(l0 * x[t[0]] + l1 * x[t[1]] + l2 * x[t[2]], l0 * y[t[0]] + l1 * y[t[1]] + l2 * y[t[2]]);
                }
            }
            return sum * area;
        }

        /// <summary>Direct solution of the singular Neumann problem K χ = f: reverse Cuthill–McKee ordering and envelope Cholesky with
        /// χ = 0 at the first node. The right-hand side is projected on the range of K (zero sum), so the pin leaves no residual;
        /// χ is defined up to a constant that does not enter the stresses. Thin walls give a narrow envelope.</summary>
        private sealed class EnvelopeCholesky
        {
            private readonly int[] order, first; private readonly double[][] rows;
            internal EnvelopeCholesky(SectionMesh mesh, double[][] u)
            {
                int n = u[0].Length; var used = new bool[n]; var neighbours = new HashSet<int>[n];
                for (int i = 0; i < n; i++) neighbours[i] = new HashSet<int>();
                foreach (var t in mesh.Triangles) for (int a = 0; a < 3; a++) { used[t[a]] = true; for (int b = 0; b < 3; b++) if (a != b) neighbours[t[a]].Add(t[b]); }
                order = ReverseCuthillMcKee(neighbours, used); var position = new int[n];
                for (int i = 0; i < n; i++) position[i] = -1;
                for (int i = 0; i < order.Length; i++) position[order[i]] = i;
                int m = order.Length; first = new int[m];
                // Row 0 is the pinned node: no coupling with it is stored.
                for (int i = 0; i < m; i++) { int f = i; foreach (int j in neighbours[order[i]]) if (position[j] > 0) f = Math.Min(f, position[j]); first[i] = i == 0 ? 0 : f; }
                rows = Enumerable.Range(0, m).Select(i => new double[i - first[i] + 1]).ToArray();
                foreach (var t in mesh.Triangles)
                {
                    var g = Gradients(u[0], u[1], t, out double area);
                    for (int a = 0; a < 3; a++) for (int b = 0; b < 3; b++)
                    {
                        int i = position[t[a]], j = position[t[b]]; if (j > i || i == 0 || j == 0) continue;
                        rows[i][j - first[i]] += area * (g[a, 0] * g[b, 0] + g[a, 1] * g[b, 1]);
                    }
                }
                rows[0][0] = 1;
                for (int i = 1; i < m; i++)
                {
                    var li = rows[i];
                    for (int j = first[i]; j <= i; j++)
                    {
                        var lj = rows[j]; int start = Math.Max(first[i], first[j]); double s = li[j - first[i]];
                        for (int k = start; k < j; k++) s -= li[k - first[i]] * lj[k - first[j]];
                        if (j < i) li[j - first[i]] = s / lj[j - first[j]];
                        else { if (!(s > 0)) throw new ArgumentException("SingularSectionShearProblem"); li[i - first[i]] = Math.Sqrt(s); }
                    }
                }
            }
            internal double[] Solve(double[] rhs)
            {
                int m = order.Length; double mean = order.Sum(i => rhs[i]) / m; var y = new double[m];
                for (int i = 0; i < m; i++) y[i] = rhs[order[i]] - mean;
                y[0] = 0;
                for (int i = 1; i < m; i++) { var li = rows[i]; double s = y[i]; for (int k = Math.Max(first[i], 1); k < i; k++) s -= li[k - first[i]] * y[k]; y[i] = s / li[i - first[i]]; }
                for (int i = m - 1; i >= 1; i--) { var li = rows[i]; y[i] /= li[i - first[i]]; for (int k = Math.Max(first[i], 1); k < i; k++) y[k] -= li[k - first[i]] * y[i]; }
                var result = new double[rhs.Length];
                for (int i = 0; i < m; i++) result[order[i]] = y[i];
                return result;
            }
            private static int[] ReverseCuthillMcKee(HashSet<int>[] neighbours, bool[] used)
            {
                var nodes = Enumerable.Range(0, neighbours.Length).Where(i => used[i]).ToArray();
                int start = nodes.OrderBy(i => neighbours[i].Count).First();
                for (int pass = 0; pass < 2; pass++) start = Levels(neighbours, start).Last(); // pseudo-peripheral start
                var visited = new bool[neighbours.Length]; var result = new List<int>(nodes.Length); var queue = new Queue<int>();
                visited[start] = true; queue.Enqueue(start);
                while (queue.Count != 0)
                {
                    int i = queue.Dequeue(); result.Add(i);
                    foreach (int j in neighbours[i].Where(j => !visited[j]).OrderBy(j => neighbours[j].Count)) { visited[j] = true; queue.Enqueue(j); }
                }
                result.Reverse(); return result.ToArray();
            }
            private static List<int> Levels(HashSet<int>[] neighbours, int start)
            {
                var visited = new bool[neighbours.Length]; var result = new List<int>(); var queue = new Queue<int>(); visited[start] = true; queue.Enqueue(start);
                while (queue.Count != 0) { int i = queue.Dequeue(); result.Add(i); foreach (int j in neighbours[i]) if (!visited[j]) { visited[j] = true; queue.Enqueue(j); } }
                return result;
            }
        }

        /// <summary>Conforming triangle mesh of all section regions (coincident vertices of different parts merged).</summary>
        private sealed class SectionMesh
        {
            private readonly List<double> px = new List<double>(), py = new List<double>();
            internal List<int[]> Triangles = new List<int[]>();
            internal double Area { get; private set; }
            private double cx, cy;

            internal static SectionMesh Create(ISectionShape section)
            {
                var outlines = section is SectionBuiltUp built ? built.GetOutlines().ToArray() : section.Shape == null ? new GPC.Geometry.Shape2d[0] : new[] { section.Shape };
                if (outlines.Length == 0) throw new NotSupportedException("ShearAreasRequireSectionGeometry: " + section.Name);
                var result = new SectionMesh(); var merged = new Dictionary<(long, long), int>();
                // Well-shaped triangles of about the target size, thin walls included.
                double total = outlines.Sum(o => Math.Abs(new Section(o).Area)), spacing = Math.Sqrt(4 * total / (Math.Sqrt(3) * MinimumTriangles));
                var meshes = outlines.Select(o => new Section(o).GetMesh(spacing)).ToArray();
                if (meshes.Any(m => m == null || m.Faces.Count == 0)) throw new ArgumentException("EmptySectionMesh: " + section.Name);
                double size = meshes.SelectMany(m => m.Vertices).Max(v => Math.Max(Math.Abs(v.Point.X), Math.Abs(v.Point.Y))), grid = Math.Max(size, 1) * 1e-9;
                foreach (var mesh in meshes)
                {
                    var ids = new Dictionary<int, int>();
                    foreach (var v in mesh.Vertices)
                    {
                        var key = ((long)Math.Round(v.Point.X / grid), (long)Math.Round(v.Point.Y / grid));
                        if (!merged.TryGetValue(key, out int index)) { merged.Add(key, index = result.px.Count); result.px.Add(v.Point.X); result.py.Add(v.Point.Y); }
                        ids.Add(v.Id, index);
                    }
                    foreach (var face in mesh.Faces)
                    {
                        var f = mesh.GetFaceVertices(face).Select(v => ids[v.Id]).ToArray();
                        for (int k = 1; k + 1 < f.Length; k++) result.Triangles.Add(new[] { f[0], f[k], f[k + 1] });
                    }
                }
                result.Triangles.RemoveAll(t => Math.Abs(result.Det(t)) <= 1e-14 * size * size);
                result.Check();
                while (result.Triangles.Count < MinimumTriangles) result.Subdivide();
                return result;
            }
            private double Det(int[] t) => (px[t[1]] - px[t[0]]) * (py[t[2]] - py[t[0]]) - (px[t[2]] - px[t[0]]) * (py[t[1]] - py[t[0]]);

            /// <summary>Area, centroid and connectivity: disconnected parts cannot exchange shear flow.</summary>
            private void Check()
            {
                double area = 0, sx = 0, sy = 0;
                foreach (var t in Triangles)
                {
                    double a = Math.Abs(Det(t)) / 2; area += a;
                    sx += a * (px[t[0]] + px[t[1]] + px[t[2]]) / 3; sy += a * (py[t[0]] + py[t[1]] + py[t[2]]) / 3;
                }
                if (!(area > 0)) throw new ArgumentException("EmptySectionMesh");
                Area = area; cx = sx / area; cy = sy / area;
                var parent = Enumerable.Range(0, px.Count).ToArray();
                Func<int, int> find = i => { while (parent[i] != i) { parent[i] = parent[parent[i]]; i = parent[i]; } return i; };
                foreach (var t in Triangles) { parent[find(t[1])] = find(t[0]); parent[find(t[2])] = find(t[0]); }
                var used = Triangles.SelectMany(t => t).Distinct().ToArray();
                if (used.Select(find).Distinct().Count() != 1) throw new NotSupportedException("ShearAreasRequireConnectedSection");
            }

            /// <summary>Each triangle into four through its edge midpoints (shared, so the mesh stays conforming).</summary>
            private void Subdivide()
            {
                var midpoints = new Dictionary<(int, int), int>(); var refined = new List<int[]>(Triangles.Count * 4);
                Func<int, int, int> mid = (a, b) =>
                {
                    var key = a < b ? (a, b) : (b, a);
                    if (!midpoints.TryGetValue(key, out int m)) { midpoints.Add(key, m = px.Count); px.Add((px[a] + px[b]) / 2); py.Add((py[a] + py[b]) / 2); }
                    return m;
                };
                foreach (var t in Triangles)
                {
                    int ab = mid(t[0], t[1]), bc = mid(t[1], t[2]), ca = mid(t[2], t[0]);
                    refined.Add(new[] { t[0], ab, ca }); refined.Add(new[] { ab, t[1], bc }); refined.Add(new[] { ca, bc, t[2] }); refined.Add(new[] { ab, bc, ca });
                }
                Triangles = refined;
            }

            /// <summary>Node coordinates about the centroid in the principal axes of the meshed region (axis 1: maximum inertia).</summary>
            internal (double[] U1, double[] U2) Principal()
            {
                double ixx = 0, iyy = 0, ixy = 0;
                foreach (var t in Triangles)
                {
                    double a = Math.Abs(Det(t)) / 2;
                    double x1 = px[t[0]] - cx, x2 = px[t[1]] - cx, x3 = px[t[2]] - cx, y1 = py[t[0]] - cy, y2 = py[t[1]] - cy, y3 = py[t[2]] - cy;
                    ixx += a / 6 * (y1 * y1 + y2 * y2 + y3 * y3 + y1 * y2 + y2 * y3 + y3 * y1);
                    iyy += a / 6 * (x1 * x1 + x2 * x2 + x3 * x3 + x1 * x2 + x2 * x3 + x3 * x1);
                    ixy += a / 12 * (2 * (x1 * y1 + x2 * y2 + x3 * y3) + x1 * y2 + x2 * y1 + x1 * y3 + x3 * y1 + x2 * y3 + x3 * y2);
                }
                // A rigid rotation leaves the shear areas unchanged: use the axes of the meshed outline itself.
                double theta = 0.5 * Math.Atan2(2 * ixy, iyy - ixx);
                Func<double, double> about = angle => ixx * Math.Cos(angle) * Math.Cos(angle) + iyy * Math.Sin(angle) * Math.Sin(angle) - 2 * ixy * Math.Sin(angle) * Math.Cos(angle);
                if (about(theta + Math.PI / 2) > about(theta)) theta += Math.PI / 2;
                double c = Math.Cos(theta), s = Math.Sin(theta); var u1 = new double[px.Count]; var u2 = new double[px.Count];
                for (int i = 0; i < px.Count; i++) { u1[i] = c * (px[i] - cx) + s * (py[i] - cy); u2[i] = -s * (px[i] - cx) + c * (py[i] - cy); }
                return (u1, u2);
            }
        }
    }
}
