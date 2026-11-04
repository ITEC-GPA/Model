using GPC.Geometry;
using GPC.Geometry.Meshes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Sections
{
    /// <summary>
    /// How <see cref="SectionShearAreas.Compute"/> obtained the shear areas
    /// </summary>
    public enum SectionShearAreaMethod
    {
        /// <summary>
        /// One connected body: the Saint-Venant flexure solved by finite elements (Trefftz energy), A = I² / ∫|∇χ + a|² dA
        /// </summary>
        SaintVenantFlexure,

        /// <summary>
        /// Separate bodies (the parts of a built-up section connected at discrete points, or regions that do not touch) that bend together
        /// without shear flow between them: the flexure of each body, A = (Σ I_k)² / Σ U_k
        /// </summary>
        SeparatePartsBendingTogether,
    }

    /// <summary>
    /// The shear areas of a section computed by <see cref="SectionShearAreas.Compute"/>, in mm², in the principal axes of the section
    /// </summary>
    public sealed class SectionShearAreaProperties
    {
        internal SectionShearAreaProperties(double area1, double area2, int bodies, double poissonRatio, int triangles, int nodes)
        {
            Area1 = area1;
            Area2 = area2;
            Bodies = bodies;
            PoissonRatio = poissonRatio;
            Triangles = triangles;
            Nodes = nodes;
        }

        /// <summary>
        /// The shear area for a shear force along the principal axis 1 (<see cref="ISectionShape.AngleX1"/>; Straus7 SA1), mm²
        /// </summary>
        public double Area1 { get; }

        /// <summary>
        /// The shear area for a shear force along the principal axis 2 (Straus7 SA2), mm²
        /// </summary>
        public double Area2 { get; }

        /// <summary>
        /// The separate bodies that carry the shear: 1 for a connected section (a single region, the parts of a <see cref="SectionWelded"/>);
        /// more for the parts of another <see cref="SectionBuiltUp"/> or regions that do not touch
        /// </summary>
        public int Bodies { get; }

        /// <summary>
        /// True if the section is made of separate bodies (<see cref="Bodies"/> &gt; 1): the areas assume that the parts bend together without
        /// shear flow between them (battens, gusset plates, bolts), which the caller has to check (EN 1993-1-1 6.4 for built-up members)
        /// </summary>
        public bool HasSeparateParts => Bodies > 1;

        /// <summary>
        /// The method of the calculation
        /// </summary>
        public SectionShearAreaMethod Method => Bodies > 1 ? SectionShearAreaMethod.SeparatePartsBendingTogether : SectionShearAreaMethod.SaintVenantFlexure;

        /// <summary>
        /// The Poisson ratio of the material used in the flexure solution
        /// </summary>
        public double PoissonRatio { get; }

        /// <summary>
        /// The linear triangles of the analysis mesh
        /// </summary>
        public int Triangles { get; }

        /// <summary>
        /// The nodes of the analysis mesh
        /// </summary>
        public int Nodes { get; }

        /// <summary>
        /// The two areas, for <c>var (a1, a2) = SectionShearAreas.Compute(section, nu)</c>
        /// </summary>
        public void Deconstruct(out double area1, out double area2)
        {
            area1 = Area1;
            area2 = Area2;
        }

        /// <summary>
        /// The description of the values
        /// </summary>
        public override string ToString() =>
            $"SA1 = {Area1:G6}, SA2 = {Area2:G6} mm² ({Method}, {Bodies} bodies, ν = {PoissonRatio:G3}, {Triangles} triangles)";
    }

    /// <summary>Effective (energy) shear areas of a section from its GPC geometry, in mm², in the principal axes of the section.
    /// Saint-Venant flexure solved by finite elements (Trefftz/Pilkey energy method): for a shear force V along a principal
    /// axis y, with x the other principal axis and I = ∫y² dA, the shear stresses are τ = (V/I)(∇χ + a), a = (0, ν x²/(2(1+ν))),
    /// where ∇²χ = −y in the section and τ·n = 0 on every contour (outer and holes). A_s = V²/∫τ² dA = I²/∫|∇χ + a|² dA.
    /// Linear triangles on a section mesh; ν is the Poisson ratio of the material (a section carries no material).
    /// <para>Axes (review L4, 4/11/2026): 1 and 2 are the principal axes of the GPC section (<see cref="ISectionShape.AngleX1"/>, J11 about
    /// axis 1), the axes of the Straus7 writer. They are not derived again from the mesh, where two equal moments of inertia leave them
    /// undetermined and SA1/SA2 could be swapped.</para>
    /// <para>Parts (review L4): the regions of a section are meshed one by one and joined as the torsion joins them
    /// (<see cref="SectionMeshJoiner"/>: coincident vertices merged, a vertex on a side of another region splits that triangle), so the parts of a
    /// <see cref="SectionWelded"/>, welded along their sides, are one conforming body. The parts of any other <see cref="SectionBuiltUp"/> are
    /// connected at discrete points (battens, gussets, bolts), as for its torsion constant: they are not joined. Separate bodies (those parts,
    /// or regions that do not touch) bend together, as the moments of inertia of the built-up section assume: in each principal direction of
    /// the section they have the same curvature rate, about axes parallel to the principal axes of the section through their own centroids
    /// (the connections carry the longitudinal forces between them), and no shear flow passes from one to another. Body k carries
    /// V_k = I_k κ' and its flexure stresses store U_k; the section stores ΣU_k for V = ΣV_k, so A_i = (Σ I_k)² / Σ U_k: the energy shear area
    /// of the whole, the same rule R31 applies to a BXS of separate regions (checked natively on two angles with a gusset gap).</para>
    /// <para>Moved from GPC.Converter to GPCModel (review L6, 4/11/2026), next to the torsion that shares the joining of the regions, so that
    /// every adapter (the Straus7 writer, the BridGHe adapter for <c>ElasticBeamProperty.ShearArea1/2</c>) calls one public API.</para></summary>
    public static class SectionShearAreas
    {
        /// <summary>Minimum number of triangles of the section mesh (the default mesh of each region, uniformly subdivided if needed).</summary>
        public const int MinimumTriangles = 20000;

        /// <summary>The shear areas of the section in its principal axes 1 and 2 (Straus7 ipSA1/ipSA2), with the separate bodies and the method.
        /// <para>A generic <see cref="Section"/> built from an outline whose properties are not computed yet (area not positive) is solved on a
        /// copy of its outline with <see cref="Section.SetMechanicalProperties"/>: the section itself is not changed.</para></summary>
        /// <param name="section">The section, with a geometric outline (a single shape or the outlines of a <see cref="SectionBuiltUp"/>)</param>
        /// <param name="poissonRatio">The Poisson ratio of the material (−1 &lt; ν &lt; 0.5)</param>
        /// <returns>The shear areas</returns>
        /// <exception cref="NotSupportedException">The section has no geometric outline or no properties, or its outline does not match its
        /// principal axes (ShearAreasRequireSectionGeometry, ShearAreasRequireSectionProperties, ShearAreasAxesDoNotMatchSectionGeometry)</exception>
        /// <exception cref="ArgumentException">The mesh is empty or degenerate</exception>
        public static SectionShearAreaProperties Compute(ISectionShape section, double poissonRatio)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            if (double.IsNaN(poissonRatio) || poissonRatio <= -1 || poissonRatio >= .5) throw new ArgumentOutOfRangeException(nameof(poissonRatio));
            section = WithProperties(section);
            var mesh = SectionMesh.Create(section);
            double angle = AxisOneInShape(section);
            var (i11, i22, i12) = mesh.Inertia(mesh.Axes(angle));
            // A gross mismatch between the outline and the stated axes (a shape in another frame) is rejected, never computed on wrong axes.
            double j11 = section.J11, j22 = section.J22;
            if (j11 - j22 > 1e-3 * (j11 + j22) && (i11 - i22 < .5 * (j11 - j22) || Math.Abs(i12) > .25 * (j11 - j22)))
                throw new NotSupportedException("ShearAreasAxesDoNotMatchSectionGeometry: " + section.Name);
            var parts = mesh.Bodies();
            double c = poissonRatio / (2 * (1 + poissonRatio));
            int triangles = mesh.Triangles.Count, nodes = mesh.UsedNodes();
            if (parts.Count == 1)
            {
                // The Laplacian does not depend on the shear direction: one factorization, two right-hand sides.
                var frame = new Frame(mesh, mesh.Axes(angle)); var solver = new EnvelopeCholesky(mesh, frame);
                // Shear along axis 1: bending about axis 2 (I = ∫u1²), transverse coordinate u2; and conversely.
                var (inertia1, energy1) = Flexure(mesh, frame, solver, 0, c); var (inertia2, energy2) = Flexure(mesh, frame, solver, 1, c);
                double factor1 = inertia1 * inertia1 / (mesh.Area * energy1), factor2 = inertia2 * inertia2 / (mesh.Area * energy2);
                return new SectionShearAreaProperties(factor1 * section.Area, factor2 * section.Area, 1, poissonRatio, triangles, nodes);
            }
            double sum1 = 0, sum2 = 0, stored1 = 0, stored2 = 0;
            foreach (var body in parts)
            {
                // The axes of the section through the centroid of the body.
                var frame = new Frame(body, body.Axes(angle)); var solver = new EnvelopeCholesky(body, frame);
                var (inertia1, energy1) = Flexure(body, frame, solver, 0, c); var (inertia2, energy2) = Flexure(body, frame, solver, 1, c);
                sum1 += inertia1; stored1 += energy1; sum2 += inertia2; stored2 += energy2;
            }
            double scale = section.Area / mesh.Area;
            return new SectionShearAreaProperties(sum1 * sum1 / stored1 * scale, sum2 * sum2 / stored2 * scale, parts.Count, poissonRatio, triangles, nodes);
        }

        /// <summary>The section with its properties: the section itself, or for a generic <see cref="Section"/> whose properties are not computed
        /// a copy of its outline with them (the caller's section is not changed).</summary>
        private static ISectionShape WithProperties(ISectionShape section)
        {
            if (section.Area > 0) return section;
            if (section.GetType() == typeof(Section) && section.Shape != null)
            {
                var copy = new Section(section.Shape, section.Name);
                copy.SetMechanicalProperties();
                if (copy.Area > 0) return copy;
            }
            throw new NotSupportedException("ShearAreasRequireSectionProperties: " + section.Name);
        }

        /// <summary>The direction of the principal axis 1 in the coordinates of the outline: <see cref="ISectionShape.AngleX1"/>, except for a
        /// rotated <see cref="SectionRectangular"/>, whose outline stays in its own axes (rotated by <see cref="SectionRectangular.Angle"/>).</summary>
        private static double AxisOneInShape(ISectionShape section)
        {
            double angle = section.AngleX1;
            if (double.IsNaN(angle) || double.IsInfinity(angle)) throw new ArgumentException("ShearAreasRequirePrincipalAxes: " + section.Name);
            var shape = section is Steel.SteelSection steel ? steel.SectionShape : section;
            return shape is SectionRectangular rectangle ? angle - rectangle.Angle : angle;
        }

        /// <summary>The triangulation the shear areas are computed on, in the coordinates of the section (mm), with zero-based triangles: the
        /// regions joined as for <see cref="Compute"/>. For comparisons with the native section of a solver.</summary>
        public static (double[][] Vertices, int[][] Triangles) AnalysisMesh(ISectionShape section)
        {
            if (section == null) throw new ArgumentNullException(nameof(section));
            return SectionMesh.Create(WithProperties(section)).Export();
        }

        /// <summary>The flexure for shear along u[along] (the other coordinate transverse): I = ∫ u[along]² dA and the energy ∫|∇χ + a|² dA,
        /// so that A_s = I²/energy.</summary>
        private static (double Inertia, double Energy) Flexure(SectionMesh mesh, Frame frame, EnvelopeCholesky solver, int along, double c)
        {
            int across = 1 - along; double[] x = frame.U[across], y = frame.U[along];
            var chi = solver.Solve(Load(mesh, frame, along, c, out double inertia));
            double energy = 0; var triangles = mesh.Triangles;
            for (int e = 0; e < triangles.Count; e++)
            {
                var t = triangles[e]; int g = 6 * e;
                double gx = 0, gy = 0; for (int a = 0; a < 3; a++) { gx += frame.Gradient[g + 2 * a + across] * chi[t[a]]; gy += frame.Gradient[g + 2 * a + along] * chi[t[a]]; }
                // ∫ gx² + (gy + c x²)² with the Dunavant rule (see Quadrature).
                double sum = 0;
                for (int r = 0; r < 2; r++)
                {
                    double w = RuleA[r], b = 1 - 2 * w;
                    for (int k = 0; k < 3; k++)
                    {
                        double l0 = k == 0 ? b : w, l1 = k == 1 ? b : w, l2 = k == 2 ? b : w;
                        double px = l0 * x[t[0]] + l1 * x[t[1]] + l2 * x[t[2]], q = gy + c * px * px;
                        sum += RuleW[r] * (gx * gx + q * q);
                    }
                }
                energy += sum * frame.Area[e];
            }
            if (!(energy > 0)) throw new ArgumentException("DegenerateSectionShearFlow");
            return (inertia, energy);
        }

        /// <summary>The load of the flexure problem for shear along u[along], and I = ∫ u[along]² dA.</summary>
        private static double[] Load(SectionMesh mesh, Frame frame, int along, double c, out double inertia)
        {
            int across = 1 - along; double[] x = frame.U[across], y = frame.U[along]; inertia = 0;
            var rhs = new double[x.Length]; var triangles = mesh.Triangles;
            for (int e = 0; e < triangles.Count; e++)
            {
                var t = triangles[e]; double area = frame.Area[e];
                // ∫ x² and ∫ y² with the Dunavant rule (see Quadrature).
                double sx = 0, sy = 0;
                for (int r = 0; r < 2; r++)
                {
                    double w = RuleA[r], b = 1 - 2 * w;
                    for (int k = 0; k < 3; k++)
                    {
                        double l0 = k == 0 ? b : w, l1 = k == 1 ? b : w, l2 = k == 2 ? b : w;
                        double px = l0 * x[t[0]] + l1 * x[t[1]] + l2 * x[t[2]], py = l0 * y[t[0]] + l1 * y[t[1]] + l2 * y[t[2]];
                        sx += RuleW[r] * (px * px); sy += RuleW[r] * (py * py);
                    }
                }
                double xx = sx * area; inertia += sy * area;
                // ∫ y N_a − ∫ a_y ∂N_a/∂y, a_y = c x², with ∫ y N_a = A (2 y_a + y_b + y_c)/12 for linear y.
                for (int a = 0; a < 3; a++) rhs[t[a]] += area * (2 * y[t[a]] + y[t[(a + 1) % 3]] + y[t[(a + 2) % 3]]) / 12 - c * xx * frame.Gradient[6 * e + 2 * a + along];
            }
            return rhs;
        }

        /// <summary>The node coordinates in the axes of the shear and, for each triangle, the gradients (∂N_a/∂u1, ∂N_a/∂u2) and the area:
        /// computed once for the factorization and both flexure problems (fewer allocations: several sections are solved in parallel).</summary>
        private sealed class Frame
        {
            internal readonly double[][] U; internal readonly double[] Gradient, Area;
            internal Frame(SectionMesh mesh, double[][] u)
            {
                U = u; var triangles = mesh.Triangles; Gradient = new double[6 * triangles.Count]; Area = new double[triangles.Count];
                double[] x = u[0], y = u[1];
                for (int e = 0; e < triangles.Count; e++)
                {
                    var t = triangles[e];
                    double x1 = x[t[0]], y1 = y[t[0]], x2 = x[t[1]], y2 = y[t[1]], x3 = x[t[2]], y3 = y[t[2]];
                    double det = (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1); Area[e] = Math.Abs(det) / 2; int g = 6 * e;
                    Gradient[g] = (y2 - y3) / det; Gradient[g + 1] = (x3 - x2) / det; Gradient[g + 2] = (y3 - y1) / det;
                    Gradient[g + 3] = (x1 - x3) / det; Gradient[g + 4] = (y1 - y2) / det; Gradient[g + 5] = (x2 - x1) / det;
                }
            }
        }

        // Dunavant 6-point rule, exact for polynomials of degree 4 on a triangle (barycentric a, a, 1 - 2a): for r in 0..1, k in 0..2 the
        // point with barycentric b = 1 − 2 RuleA[r] at vertex k and RuleA[r] at the others, weight RuleW[r], the sum times the area.
        private static readonly double[] RuleA = { 0.445948490915965, 0.091576213509771 };
        private static readonly double[] RuleW = { 0.223381589678011, 0.109951743655322 };

        /// <summary>Direct solution of the singular Neumann problem K χ = f: reverse Cuthill–McKee ordering and envelope Cholesky with
        /// χ = 0 at the first node. The right-hand side is projected on the range of K (zero sum), so the pin leaves no residual;
        /// χ is defined up to a constant that does not enter the stresses. Thin walls give a narrow envelope. One connected body.</summary>
        private sealed class EnvelopeCholesky
        {
            private readonly int[] order, first; private readonly double[][] rows;
            internal EnvelopeCholesky(SectionMesh mesh, Frame frame)
            {
                int n = frame.U[0].Length; var used = new bool[n]; var neighbours = new HashSet<int>[n];
                for (int i = 0; i < n; i++) neighbours[i] = new HashSet<int>();
                foreach (var t in mesh.Triangles) for (int a = 0; a < 3; a++) { used[t[a]] = true; for (int b = 0; b < 3; b++) if (a != b) neighbours[t[a]].Add(t[b]); }
                order = ReverseCuthillMcKee(neighbours, used); var position = new int[n];
                for (int i = 0; i < n; i++) position[i] = -1;
                for (int i = 0; i < order.Length; i++) position[order[i]] = i;
                int m = order.Length; first = new int[m];
                // Row 0 is the pinned node: no coupling with it is stored.
                for (int i = 0; i < m; i++) { int f = i; foreach (int j in neighbours[order[i]]) if (position[j] > 0) f = Math.Min(f, position[j]); first[i] = i == 0 ? 0 : f; }
                rows = Enumerable.Range(0, m).Select(i => new double[i - first[i] + 1]).ToArray();
                var triangles = mesh.Triangles; var gradient = frame.Gradient;
                for (int e = 0; e < triangles.Count; e++)
                {
                    var t = triangles[e]; double area = frame.Area[e]; int g = 6 * e;
                    for (int a = 0; a < 3; a++) for (int b = 0; b < 3; b++)
                    {
                        int i = position[t[a]], j = position[t[b]]; if (j > i || i == 0 || j == 0) continue;
                        rows[i][j - first[i]] += area * (gradient[g + 2 * a] * gradient[g + 2 * b] + gradient[g + 2 * a + 1] * gradient[g + 2 * b + 1]);
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

        /// <summary>Triangle mesh of the regions of a section: joined into one conforming mesh where they touch (a welded section, a single
        /// region), each part on its own for the other built-up sections; its connected bodies.</summary>
        private sealed class SectionMesh
        {
            private readonly List<double> px, py;
            internal List<int[]> Triangles;
            internal double Area { get; private set; }
            private double cx, cy;
            private SectionMesh(List<double> x, List<double> y, List<int[]> triangles) { px = x; py = y; Triangles = triangles; }

            internal static SectionMesh Create(ISectionShape section)
            {
                var outlines = section is SectionBuiltUp built ? built.GetOutlines().ToArray() : section.Shape == null ? new Shape2d[0] : new[] { section.Shape };
                if (outlines.Length == 0) throw new NotSupportedException("ShearAreasRequireSectionGeometry: " + section.Name);
                // The parts of a welded section work as one region; those of another built-up section are connected at discrete points.
                bool join = !(section is SectionBuiltUp) || section is SectionWelded;
                // The default mesh of each region (DelaunayMesh size of the region), refined uniformly below to MinimumTriangles: the mesh of
                // the C2 comparisons with R31 (review L4: the size computed before from `new Section(o).Area`, zero without its properties,
                // was always this default; the triangles are unchanged).
                var meshes = outlines.Select(o => new Section(o).GetMesh()).ToArray();
                if (meshes.Any(m => m == null || m.Faces.Count == 0)) throw new ArgumentException("EmptySectionMesh: " + section.Name);
                double size = meshes.SelectMany(m => m.Vertices).Max(v => Math.Max(Math.Abs(v.Point.X), Math.Abs(v.Point.Y)));
                // Length scale of the joining: the side of the target triangles (search cells), 1e-7 of it to merge or split.
                double spacing = Math.Sqrt(4 * Math.Abs(section.Area) / (Math.Sqrt(3) * MinimumTriangles));
                if (!(spacing > 0) || double.IsInfinity(spacing)) spacing = 1e-3 * Math.Max(size, 1);
                var x = new List<double>(); var y = new List<double>(); var triangles = new List<int[]>();
                SectionMeshJoiner.Join(meshes, join, spacing, x, y, triangles);
                var result = new SectionMesh(x, y, triangles);
                result.Triangles.RemoveAll(t => Math.Abs(result.Det(t)) <= 1e-14 * size * size);
                result.Measure();
                while (result.Triangles.Count < MinimumTriangles) result.Subdivide();
                return result;
            }
            private double Det(int[] t) => (px[t[1]] - px[t[0]]) * (py[t[2]] - py[t[0]]) - (px[t[2]] - px[t[0]]) * (py[t[1]] - py[t[0]]);

            /// <summary>Area and centroid.</summary>
            private void Measure()
            {
                double area = 0, sx = 0, sy = 0;
                foreach (var t in Triangles)
                {
                    double a = Math.Abs(Det(t)) / 2; area += a;
                    sx += a * (px[t[0]] + px[t[1]] + px[t[2]]) / 3; sy += a * (py[t[0]] + py[t[1]] + py[t[2]]) / 3;
                }
                if (!(area > 0)) throw new ArgumentException("EmptySectionMesh");
                Area = area; cx = sx / area; cy = sy / area;
            }

            /// <summary>The number of vertices used by the triangles.</summary>
            internal int UsedNodes()
            {
                var used = new bool[px.Count]; int count = 0;
                foreach (var t in Triangles) foreach (int i in t) if (!used[i]) { used[i] = true; count++; }
                return count;
            }

            /// <summary>The connected bodies: this mesh when it is connected, otherwise one mesh for each (no shear flow between them).</summary>
            internal List<SectionMesh> Bodies()
            {
                var parent = Enumerable.Range(0, px.Count).ToArray();
                Func<int, int> find = i => { while (parent[i] != i) { parent[i] = parent[parent[i]]; i = parent[i]; } return i; };
                foreach (var t in Triangles) { parent[find(t[1])] = find(t[0]); parent[find(t[2])] = find(t[0]); }
                var groups = new Dictionary<int, List<int[]>>(); var roots = new List<int>();
                foreach (var t in Triangles)
                {
                    int root = find(t[0]);
                    if (!groups.TryGetValue(root, out var list)) { groups.Add(root, list = new List<int[]>()); roots.Add(root); }
                    list.Add(t);
                }
                if (roots.Count == 1) return new List<SectionMesh> { this };
                var bodies = new List<SectionMesh>(roots.Count);
                foreach (int root in roots)
                {
                    var index = new Dictionary<int, int>(); var x = new List<double>(); var y = new List<double>(); var triangles = new List<int[]>();
                    foreach (var t in groups[root])
                        triangles.Add(t.Select(i => { if (!index.TryGetValue(i, out int j)) { index.Add(i, j = x.Count); x.Add(px[i]); y.Add(py[i]); } return j; }).ToArray());
                    var body = new SectionMesh(x, y, triangles); body.Measure(); bodies.Add(body);
                }
                return bodies;
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

            /// <summary>Node coordinates about the centroid in the axes at <paramref name="angle"/> (rad) from the section X axis: u1 along it.</summary>
            internal double[][] Axes(double angle)
            {
                double c = Math.Cos(angle), s = Math.Sin(angle); var u1 = new double[px.Count]; var u2 = new double[px.Count];
                for (int i = 0; i < px.Count; i++) { u1[i] = c * (px[i] - cx) + s * (py[i] - cy); u2[i] = -s * (px[i] - cx) + c * (py[i] - cy); }
                return new[] { u1, u2 };
            }

            /// <summary>∫u2², ∫u1² and ∫u1u2 over the mesh, exact for the linear coordinates of <see cref="Axes"/>.</summary>
            internal (double I11, double I22, double I12) Inertia(double[][] u)
            {
                double i11 = 0, i22 = 0, i12 = 0;
                foreach (var t in Triangles)
                {
                    double a = Math.Abs(Det(t)) / 2, x1 = u[0][t[0]], x2 = u[0][t[1]], x3 = u[0][t[2]], y1 = u[1][t[0]], y2 = u[1][t[1]], y3 = u[1][t[2]];
                    i11 += a / 6 * (y1 * y1 + y2 * y2 + y3 * y3 + y1 * y2 + y2 * y3 + y3 * y1);
                    i22 += a / 6 * (x1 * x1 + x2 * x2 + x3 * x3 + x1 * x2 + x2 * x3 + x3 * x1);
                    i12 += a / 12 * (2 * (x1 * y1 + x2 * y2 + x3 * y3) + x1 * y2 + x2 * y1 + x1 * y3 + x3 * y1 + x2 * y3 + x3 * y2);
                }
                return (i11, i22, i12);
            }

            /// <summary>The vertices used by the triangles and the triangles, zero-based.</summary>
            internal (double[][] Vertices, int[][] Triangles) Export()
            {
                var index = new Dictionary<int, int>(); var vertices = new List<double[]>();
                var triangles = Triangles.Select(t => t.Select(i => { if (!index.TryGetValue(i, out int j)) { index.Add(i, j = vertices.Count); vertices.Add(new[] { px[i], py[i], 0 }); } return j; }).ToArray()).ToArray();
                return (vertices.ToArray(), triangles);
            }
        }
    }
}
