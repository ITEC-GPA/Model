using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Sections
{
    /// <summary>
    /// The torsion properties of a cross-section computed with the finite elements on its region (see <see cref="Section.CalculateTorsionProperties"/>):
    /// Saint-Venant torsion constant, warping constant and shear centre
    /// </summary>
    [Serializable]
    public sealed class SectionTorsionProperties
    {
        /// <summary>
        /// The Saint-Venant torsion constant (<see cref="double.NaN"/> if not solved)
        /// </summary>
        public double TorsionConstant { get; }

        /// <summary>
        /// The warping constant respect to the shear centre (<see cref="double.NaN"/> if not solved or if the region is made of separate parts)
        /// </summary>
        public double WarpingConstant { get; }

        /// <summary>
        /// The shear centre (Trefftz), in the coordinates of the shape (coordinates <see cref="double.NaN"/> if not solved or if the region is made of
        /// separate parts)
        /// </summary>
        public Point2d ShearCenter { get; }

        /// <summary>
        /// The number of separate parts of the region (the torsion constant is the sum of the ones of the parts; warping constant and shear centre
        /// are not defined for more than one part)
        /// </summary>
        public int Parts { get; }

        /// <summary>
        /// The size of the mesh
        /// </summary>
        public double MeshSize { get; }

        /// <summary>
        /// The number of elements (6-node triangles)
        /// </summary>
        public int Elements { get; }

        /// <summary>
        /// The number of nodes
        /// </summary>
        public int Nodes { get; }

        /// <summary>
        /// The iterations of the solver
        /// </summary>
        public int Iterations { get; }

        /// <summary>
        /// Why the torsion is not solved (null if solved)
        /// </summary>
        public string Error { get; }

        /// <summary>
        /// True if the torsion constant is solved
        /// </summary>
        public bool IsSolved => Error is null;

        /// <summary>
        /// Creates the properties of a solved region
        /// </summary>
        internal SectionTorsionProperties(double torsionConstant, double warpingConstant, Point2d shearCenter, int parts, double meshSize,
            int elements, int nodes, int iterations)
        {
            TorsionConstant = torsionConstant;
            WarpingConstant = warpingConstant;
            ShearCenter = shearCenter;
            Parts = parts;
            MeshSize = meshSize;
            Elements = elements;
            Nodes = nodes;
            Iterations = iterations;
        }

        /// <summary>
        /// Creates the properties of a region not solved: all the values are <see cref="double.NaN"/>
        /// </summary>
        /// <param name="error">Why the torsion is not solved</param>
        /// <param name="meshSize">The size of the mesh (NaN if not chosen)</param>
        internal SectionTorsionProperties(string error, double meshSize = double.NaN)
        {
            TorsionConstant = double.NaN;
            WarpingConstant = double.NaN;
            ShearCenter = new Point2d(double.NaN, double.NaN);
            MeshSize = meshSize;
            Error = error;
        }

        /// <summary>
        /// The description of the values
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => IsSolved
            ? $"It = {TorsionConstant:G6}, Iw = {WarpingConstant:G6}, shear centre ({ShearCenter.X:G6}; {ShearCenter.Y:G6}), mesh {MeshSize:G4}: {Elements} elements"
            : "Not solved: " + Error;
    }

    /// <summary>
    /// A region of a section for the torsion with the finite elements: its shape and the ratios of its moduli to the ones of the reference
    /// material (shear modulus for the torsion, elastic modulus for the shear centre and the warping)
    /// </summary>
    internal readonly struct TorsionRegion
    {
        /// <summary>
        /// Creates a region
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <param name="shearWeight">G / G of the reference material</param>
        /// <param name="axialWeight">E / E of the reference material</param>
        internal TorsionRegion(Shape2d shape, double shearWeight = 1.0, double axialWeight = 1.0)
        {
            Shape = shape;
            ShearWeight = shearWeight;
            AxialWeight = axialWeight;
        }

        /// <summary>The shape</summary>
        internal Shape2d Shape { get; }

        /// <summary>G / G of the reference material</summary>
        internal double ShearWeight { get; }

        /// <summary>E / E of the reference material</summary>
        internal double AxialWeight { get; }
    }

    /// <summary>
    /// Saint-Venant torsion and warping of a cross-section with the finite elements. The warping function φ of the twist about a pole O solves
    /// ∇·(g ∇φ) = 0 with the boundary free (weak form: ∫ g ∇φ·∇v dA = ∫ g (y ∂v/∂x - x ∂v/∂y) dA, g the ratio of the shear moduli of the
    /// regions); the elements are 6-node triangles on the triangulation of the regions, integrated exactly (6 Gauss points), and the system is
    /// solved with the conjugate gradient. Then: torsion constant It = ∫ g (x² + y²) dA - ∫ g (y ∂φ/∂x - x ∂φ/∂y) dA (in the reference material:
    /// G It is the torsional stiffness); shear centre (Trefftz) the pole whose warping function φ + xs y - ys x is orthogonal to x and y with the
    /// weights e of the elastic moduli (centroidal axes of the homogenized section); warping constant the integral of e times the square of that
    /// function with zero mean. The warping function is single valued, so the closed cells and the holes need no special treatment. The regions
    /// are meshed one by one and joined: the coincident vertices are merged and the vertices of a region on a side of another one split its
    /// triangle, so the mesh is conforming on the interfaces. Separate parts are solved together: the torsion constant is their sum, warping
    /// constant and shear centre are not defined
    /// </summary>
    internal static class SectionTorsion
    {
        /// <summary>
        /// The maximum number of elements of the default mesh: the mesh size is increased to stay under it
        /// </summary>
        private const int MaximumElements = 40000;

        /// <summary>
        /// The relative tolerance of the residual of the conjugate gradient
        /// </summary>
        private const double Tolerance = 1e-10;

        /// <summary>
        /// The barycentric coordinates and the weights (sum 1) of the 6-point Gauss rule of the triangle (exact for polynomials of degree 4)
        /// </summary>
        private static readonly double[][] GaussPoints = CreateGaussPoints();

        /// <summary>
        /// Solves the torsion of a region of one material
        /// </summary>
        /// <param name="shape">The region (fill minus holes plus childs)</param>
        /// <param name="meshSize">The size of the elements (not positive: <see cref="DefaultMeshSize"/>)</param>
        /// <returns>The torsion properties; not solved (see <see cref="SectionTorsionProperties.Error"/>) if the region has no area, the mesh
        /// fails or the solver does not converge</returns>
        internal static SectionTorsionProperties Calculate(Shape2d shape, double meshSize = 0) =>
            Calculate(new[] { new TorsionRegion(shape) }, meshSize);

        /// <summary>
        /// Solves the torsion of regions of different materials (they must not overlap; they can touch along their sides)
        /// </summary>
        /// <param name="regions">The regions</param>
        /// <param name="meshSize">The size of the elements (not positive: <see cref="DefaultMeshSize"/>)</param>
        /// <returns>The torsion properties in the reference material; not solved (see <see cref="SectionTorsionProperties.Error"/>) if the regions
        /// have no area, the mesh fails or the solver does not converge</returns>
        internal static SectionTorsionProperties Calculate(IReadOnlyList<TorsionRegion> regions, double meshSize = 0)
        {
            List<TorsionRegion> valid = regions?.Where(r => r.Shape?.Fill != null && r.Shape.Fill.Count >= 3 && r.ShearWeight > 0 && r.AxialWeight > 0).ToList();
            if (valid is null || valid.Count == 0)
                return new SectionTorsionProperties("the section has no region");

            // the pole O: the centroid of the regions
            Point3d first = valid[0].Shape.Fill[0];
            double area = 0, sx = 0, sy = 0;
            foreach (TorsionRegion region in valid)
            {
                SectionHelper.IntegrateShape(region.Shape, first.X, first.Y, out double a, out double x1, out double y1, out _, out _, out _);
                area += a;
                sx += x1;
                sy += y1;
            }
            if (!(area > 0))
                return new SectionTorsionProperties("the region has no area");
            double xc = first.X + sy / area, yc = first.Y + sx / area;

            double size = meshSize > 0 ? meshSize : DefaultMeshSize(valid.Select(r => (Shape)r.Shape), area);
            if (!(size > 0) || double.IsInfinity(size))
                return new SectionTorsionProperties("the size of the mesh can not be chosen");

            // the vertices of the meshes of the regions, merged when coincident; coordinates relative to the pole
            var x = new List<double>();
            var y = new List<double>();
            var triangles = new List<int[]>();
            var regionOf = new List<int>();
            double tolerance = 1e-7 * size;
            var merge = new Dictionary<(long, long), List<int>>();
            int Vertex(double px, double py)
            {
                long cx = (long)Math.Floor(px / (4 * tolerance)), cy = (long)Math.Floor(py / (4 * tolerance));
                for (long i = cx - 1; i <= cx + 1; i++)
                {
                    for (long j = cy - 1; j <= cy + 1; j++)
                    {
                        if (!merge.TryGetValue((i, j), out List<int> list))
                            continue;
                        foreach (int v in list)
                        {
                            if (Math.Abs(x[v] - px) <= tolerance && Math.Abs(y[v] - py) <= tolerance)
                                return v;
                        }
                    }
                }
                x.Add(px);
                y.Add(py);
                if (!merge.TryGetValue((cx, cy), out List<int> cell))
                    merge[(cx, cy)] = cell = new List<int>();
                cell.Add(x.Count - 1);
                return x.Count - 1;
            }

            for (int r = 0; r < valid.Count; r++)
            {
                Mesh mesh = Triangulate(valid[r].Shape, size, out string meshError);
                if (mesh is null)
                    return new SectionTorsionProperties("the mesh failed: " + meshError, size);

                var index = new Dictionary<int, int>();
                foreach (MeshVertex vertex in mesh.Vertices)
                    index[vertex.Id] = Vertex(vertex.Point.X - xc, vertex.Point.Y - yc);

                void AddTriangle(int a, int b, int c)
                {
                    if (a == b || b == c || c == a)
                        return;
                    double doubleArea = (x[b] - x[a]) * (y[c] - y[a]) - (x[c] - x[a]) * (y[b] - y[a]);
                    if (doubleArea > 0)
                        triangles.Add(new[] { a, b, c });
                    else if (doubleArea < 0)
                        triangles.Add(new[] { a, c, b });
                    else
                        return;
                    regionOf.Add(r);
                }
                foreach (MeshFace face in mesh.Faces)
                {
                    int a = index[face.A], b = index[face.B], c = index[face.C];
                    AddTriangle(a, b, c);
                    if (face.IsQuad)
                        AddTriangle(a, c, index[face.D]);
                }
            }
            if (triangles.Count == 0)
                return new SectionTorsionProperties("the mesh has no elements", size);
            if (valid.Count > 1)
                Conform(x, y, triangles, regionOf, size, tolerance);

            int vertices = x.Count;
            var nodes = new List<int[]>(triangles.Count);
            var middle = new Dictionary<long, int>();
            foreach (int[] t in triangles)
            {
                var element = new[] { t[0], t[1], t[2], -1, -1, -1 };
                for (int k = 0; k < 3; k++)
                {
                    int p = t[k], q = t[(k + 1) % 3];
                    long key = (long)Math.Min(p, q) * vertices + Math.Max(p, q);
                    if (!middle.TryGetValue(key, out int m))
                    {
                        m = x.Count;
                        x.Add(0.5 * (x[p] + x[q]));
                        y.Add(0.5 * (y[p] + y[q]));
                        middle.Add(key, m);
                    }
                    element[3 + k] = m;
                }
                nodes.Add(element);
            }
            int n = x.Count;

            // the separate parts (union of the vertices of the elements)
            var parent = new int[vertices];
            for (int i = 0; i < vertices; i++)
                parent[i] = i;
            int Root(int i)
            {
                while (parent[i] != i)
                    i = parent[i] = parent[parent[i]];
                return i;
            }
            foreach (int[] t in triangles)
            {
                parent[Root(t[1])] = Root(t[0]);
                parent[Root(t[2])] = Root(t[0]);
            }
            var roots = new HashSet<int>();
            foreach (int[] t in triangles)
                roots.Add(Root(t[0]));
            var fixedNodes = new List<int>(roots);
            int parts = fixedNodes.Count;

            // the matrix (compressed rows) and the load
            var neighbours = new HashSet<int>[n];
            for (int i = 0; i < n; i++)
                neighbours[i] = new HashSet<int> { i };
            foreach (int[] t in nodes)
            {
                foreach (int i in t)
                {
                    foreach (int j in t)
                        neighbours[i].Add(j);
                }
            }
            var start = new int[n + 1];
            for (int i = 0; i < n; i++)
                start[i + 1] = start[i] + neighbours[i].Count;
            var column = new int[start[n]];
            var value = new double[start[n]];
            for (int i = 0; i < n; i++)
            {
                int k = start[i];
                foreach (int j in neighbours[i])
                    column[k++] = j;
                Array.Sort(column, start[i], neighbours[i].Count);
            }
            neighbours = null;

            var load = new double[n];
            double polar = 0;
            var dx = new double[6];
            var dy = new double[6];
            var shape6 = new double[6];
            var stiffness = new double[6, 6];
            for (int e = 0; e < nodes.Count; e++)
            {
                int[] t = nodes[e];
                double g = valid[regionOf[e]].ShearWeight;
                double x1 = x[t[0]], y1 = y[t[0]], x2 = x[t[1]], y2 = y[t[1]], x3 = x[t[2]], y3 = y[t[2]];
                double doubleArea = (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
                double elementArea = 0.5 * doubleArea;
                double[] lx = { (y2 - y3) / doubleArea, (y3 - y1) / doubleArea, (y1 - y2) / doubleArea };
                double[] ly = { (x3 - x2) / doubleArea, (x1 - x3) / doubleArea, (x2 - x1) / doubleArea };

                Array.Clear(stiffness, 0, stiffness.Length);
                foreach (double[] gauss in GaussPoints)
                {
                    double weight = g * gauss[3] * elementArea;
                    Derivatives(gauss, lx, ly, dx, dy);
                    double xg = gauss[0] * x1 + gauss[1] * x2 + gauss[2] * x3, yg = gauss[0] * y1 + gauss[1] * y2 + gauss[2] * y3;
                    polar += weight * (xg * xg + yg * yg);

                    for (int a = 0; a < 6; a++)
                    {
                        load[t[a]] += weight * (yg * dx[a] - xg * dy[a]);
                        for (int b = 0; b < 6; b++)
                            stiffness[a, b] += weight * (dx[a] * dx[b] + dy[a] * dy[b]);
                    }
                }

                for (int a = 0; a < 6; a++)
                {
                    for (int b = 0; b < 6; b++)
                        value[Position(start, column, t[a], t[b])] += stiffness[a, b];
                }
            }

            // one node of each part is fixed (the warping function is defined but for a constant): its row and column become the identity
            var solverLoad = (double[])load.Clone();
            foreach (int p in fixedNodes)
            {
                for (int k = start[p]; k < start[p + 1]; k++)
                {
                    int j = column[k];
                    value[k] = j == p ? 1.0 : 0.0;
                    if (j != p)
                        value[Position(start, column, j, p)] = 0.0;
                }
                solverLoad[p] = 0.0;
            }

            var phi = new double[n];
            if (!ConjugateGradient(start, column, value, solverLoad, phi, out int iterations))
                return new SectionTorsionProperties($"the solver did not converge in {iterations} iterations", size);

            double work = 0;
            for (int i = 0; i < n; i++)
                work += load[i] * phi[i];
            double torsionConstant = polar - work;

            if (parts > 1)
                return new SectionTorsionProperties(torsionConstant, double.NaN, new Point2d(double.NaN, double.NaN), parts, size, nodes.Count, n, iterations);

            // shear centre and warping constant: integrals with the weights of the elastic moduli, on the centroidal axes of the homogenized section
            double a0 = 0, s0x = 0, s0y = 0;
            ForEachGaussPoint(nodes, x, y, (e, gauss, weight, xg, yg) =>
            {
                double w = weight * valid[regionOf[e]].AxialWeight;
                a0 += w;
                s0x += w * xg;
                s0y += w * yg;
            });
            double xe = s0x / a0, ye = s0y / a0;

            double ixx = 0, iyy = 0, ixy = 0, iphiX = 0, iphiY = 0;
            ForEachGaussPoint(nodes, x, y, (e, gauss, weight, xg, yg) =>
            {
                double w = weight * valid[regionOf[e]].AxialWeight;
                double u = xg - xe, v = yg - ye;
                double phiG = Interpolate(gauss, nodes[e], phi, shape6);
                ixx += w * v * v;
                iyy += w * u * u;
                ixy += w * u * v;
                iphiX += w * phiG * u;
                iphiY += w * phiG * v;
            });

            // the pole of the warping function φ + xs y - ys x orthogonal to the centroidal axes (xs, ys from the pole O)
            double determinant = ixx * iyy - ixy * ixy;
            double xs = (ixy * iphiX - iyy * iphiY) / determinant;
            double ys = (ixx * iphiX - ixy * iphiY) / determinant;

            var omega = new double[n];
            for (int i = 0; i < n; i++)
                omega[i] = phi[i] + xs * y[i] - ys * x[i];
            double mean = 0, square = 0;
            ForEachGaussPoint(nodes, x, y, (e, gauss, weight, xg, yg) =>
            {
                double w = weight * valid[regionOf[e]].AxialWeight;
                double omegaG = Interpolate(gauss, nodes[e], omega, shape6);
                mean += w * omegaG;
                square += w * omegaG * omegaG;
            });
            double warpingConstant = Math.Max(0.0, square - mean * mean / a0);

            return new SectionTorsionProperties(torsionConstant, warpingConstant, new Point2d(xc + xs, yc + ys), 1, size, nodes.Count, n, iterations);
        }

        /// <summary>
        /// Calls an action for every Gauss point of the elements: element, barycentric coordinates and weight, weight times the area, coordinates
        /// </summary>
        private static void ForEachGaussPoint(List<int[]> nodes, List<double> x, List<double> y, Action<int, double[], double, double, double> action)
        {
            for (int e = 0; e < nodes.Count; e++)
            {
                int[] t = nodes[e];
                double x1 = x[t[0]], y1 = y[t[0]], x2 = x[t[1]], y2 = y[t[1]], x3 = x[t[2]], y3 = y[t[2]];
                double elementArea = 0.5 * ((x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1));
                foreach (double[] gauss in GaussPoints)
                {
                    action(e, gauss, gauss[3] * elementArea, gauss[0] * x1 + gauss[1] * x2 + gauss[2] * x3,
                        gauss[0] * y1 + gauss[1] * y2 + gauss[2] * y3);
                }
            }
        }

        /// <summary>
        /// Makes the joined meshes of the regions conforming: a vertex inside a side of the boundary of the mesh (a side of one triangle only)
        /// splits that triangle in two, until no vertex lies inside a boundary side
        /// </summary>
        /// <param name="x">The X of the vertices</param>
        /// <param name="y">The Y of the vertices</param>
        /// <param name="triangles">The triangles (counterclockwise)</param>
        /// <param name="region">The region of each triangle</param>
        /// <param name="size">The size of the mesh (the cells of the search grid)</param>
        /// <param name="tolerance">The distance of a vertex from a side to be on it</param>
        private static void Conform(List<double> x, List<double> y, List<int[]> triangles, List<int> region, double size, double tolerance)
        {
            var grid = new Dictionary<(long, long), List<int>>();
            for (int i = 0; i < x.Count; i++)
            {
                var cell = ((long)Math.Floor(x[i] / size), (long)Math.Floor(y[i] / size));
                if (!grid.TryGetValue(cell, out List<int> list))
                    grid[cell] = list = new List<int>();
                list.Add(i);
            }

            int Hanging(int a, int b)
            {
                double ex = x[b] - x[a], ey = y[b] - y[a], length2 = ex * ex + ey * ey;
                double length = Math.Sqrt(length2);
                if (length == 0)
                    return -1;
                long x0 = (long)Math.Floor((Math.Min(x[a], x[b]) - tolerance) / size), x1 = (long)Math.Floor((Math.Max(x[a], x[b]) + tolerance) / size);
                long y0 = (long)Math.Floor((Math.Min(y[a], y[b]) - tolerance) / size), y1 = (long)Math.Floor((Math.Max(y[a], y[b]) + tolerance) / size);
                int best = -1;
                double bestT = double.MaxValue;
                for (long i = x0; i <= x1; i++)
                {
                    for (long j = y0; j <= y1; j++)
                    {
                        if (!grid.TryGetValue((i, j), out List<int> list))
                            continue;
                        foreach (int v in list)
                        {
                            if (v == a || v == b)
                                continue;
                            double wx = x[v] - x[a], wy = y[v] - y[a];
                            double t = (wx * ex + wy * ey) / length2;
                            double distance = Math.Abs(ex * wy - ey * wx) / length;
                            if (distance <= tolerance && t * length > tolerance && (1 - t) * length > tolerance && t < bestT)
                            {
                                best = v;
                                bestT = t;
                            }
                        }
                    }
                }
                return best;
            }

            int count = x.Count;
            for (int pass = 0; pass < 100; pass++)
            {
                var uses = new Dictionary<long, int>();
                foreach (int[] t in triangles)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        long key = (long)Math.Min(t[k], t[(k + 1) % 3]) * count + Math.Max(t[k], t[(k + 1) % 3]);
                        uses[key] = uses.TryGetValue(key, out int u) ? u + 1 : 1;
                    }
                }

                bool changed = false;
                int triangleCount = triangles.Count;
                for (int ti = 0; ti < triangleCount; ti++)
                {
                    int[] t = triangles[ti];
                    for (int k = 0; k < 3; k++)
                    {
                        int a = t[k], b = t[(k + 1) % 3], c = t[(k + 2) % 3];
                        if (uses[(long)Math.Min(a, b) * count + Math.Max(a, b)] != 1)
                            continue;
                        int p = Hanging(a, b);
                        if (p < 0)
                            continue;
                        triangles[ti] = new[] { a, p, c };
                        triangles.Add(new[] { p, b, c });
                        region.Add(region[ti]);
                        changed = true;
                        break;
                    }
                }
                if (!changed)
                    return;
            }
        }

        /// <summary>
        /// The default size of the mesh: half the minimum thickness of the regions (two quadratic elements across the thinnest wall), at most
        /// 1/12 of the square root of the area (the compact regions), increased if the elements would be more than <see cref="MaximumElements"/>
        /// </summary>
        /// <param name="shapes">The regions</param>
        /// <param name="area">The area of the regions</param>
        /// <returns>The size</returns>
        internal static double DefaultMeshSize(IEnumerable<Shape> shapes, double area)
        {
            double size = Math.Sqrt(area) / 12.0;
            foreach (Shape shape in shapes)
                size = Math.Min(size, 0.5 * MinimumThickness(shape));
            double smallest = Math.Sqrt(area / (0.45 * MaximumElements));
            return Math.Max(size, smallest);
        }

        /// <summary>
        /// The minimum thickness of the region: from the middle of each side of the boundary, the distance along the normal towards the inside
        /// to the first other side
        /// </summary>
        /// <param name="shape">The region</param>
        /// <returns>The minimum thickness (infinity if not found)</returns>
        internal static double MinimumThickness(Shape shape)
        {
            var rings = new List<(Polygon3d polygon, bool material)>();
            void AddRings(Shape s)
            {
                if (s?.Fill is null)
                    return;
                rings.Add((s.Fill, true));
                if (s.Holes != null)
                {
                    foreach (Polygon3d hole in s.Holes)
                        rings.Add((hole, false));
                }
                if (s.Childs != null)
                {
                    foreach (Shape child in s.Childs)
                        AddRings(child);
                }
            }
            AddRings(shape);

            var ax = new List<double>();
            var ay = new List<double>();
            var bx = new List<double>();
            var by = new List<double>();
            var nx = new List<double>();
            var ny = new List<double>();
            foreach (var (polygon, material) in rings)
            {
                int count = polygon.Count;
                if (count < 3)
                    continue;
                double doubleArea = 0;
                for (int i = 0; i < count; i++)
                {
                    Point3d p = polygon[i], q = polygon[(i + 1) % count];
                    doubleArea += p.X * q.Y - q.X * p.Y;
                }
                // the material is on the left of a counterclockwise fill and on the right of a counterclockwise hole
                double side = (doubleArea > 0) == material ? 1.0 : -1.0;
                for (int i = 0; i < count; i++)
                {
                    Point3d p = polygon[i], q = polygon[(i + 1) % count];
                    double length = Math.Sqrt((q.X - p.X) * (q.X - p.X) + (q.Y - p.Y) * (q.Y - p.Y));
                    if (length == 0)
                        continue;
                    ax.Add(p.X); ay.Add(p.Y); bx.Add(q.X); by.Add(q.Y);
                    nx.Add(-side * (q.Y - p.Y) / length);
                    ny.Add(side * (q.X - p.X) / length);
                }
            }

            double minimum = double.PositiveInfinity;
            for (int i = 0; i < ax.Count; i++)
            {
                double px = 0.5 * (ax[i] + bx[i]), py = 0.5 * (ay[i] + by[i]);
                for (int j = 0; j < ax.Count; j++)
                {
                    if (j == i)
                        continue;
                    double ex = bx[j] - ax[j], ey = by[j] - ay[j];
                    double denominator = nx[i] * ey - ny[i] * ex;
                    if (Math.Abs(denominator) < 1e-14)
                        continue;
                    double wx = ax[j] - px, wy = ay[j] - py;
                    double distance = (wx * ey - wy * ex) / denominator;
                    double s = (wx * ny[i] - wy * nx[i]) / denominator;
                    if (distance > 0 && distance < minimum && s >= 0 && s <= 1)
                        minimum = distance;
                }
            }

            return minimum;
        }

        /// <summary>
        /// The triangulation of the region with good elements (minimum angle 20°, then without limit if it fails)
        /// </summary>
        /// <param name="shape">The region</param>
        /// <param name="size">The size of the elements</param>
        /// <param name="error">The error of the mesher (null if the mesh is generated)</param>
        /// <returns>The mesh, null if it fails</returns>
        private static Mesh Triangulate(Shape2d shape, double size, out string error)
        {
            error = null;
            foreach (double minimumAngle in new[] { 20.0, 0.0 })
            {
                var options = new DelaunayMesh.DelaunayGenerateOptions { MeshSize = size, Recombine = false, MinAngle = minimumAngle };
                if (DelaunayMesh.Generate(shape, options, out Mesh mesh, out DelaunayMesh.DelaunayGenerateMeshStatus status) && mesh != null && mesh.FacesCount > 0)
                    return mesh;
                error = status?.GetLastCustomErrorMessage();
            }
            return null;
        }

        /// <summary>
        /// The position of the entry (row, column) in the compressed rows
        /// </summary>
        private static int Position(int[] start, int[] column, int row, int col) =>
            Array.BinarySearch(column, start[row], start[row + 1] - start[row], col);

        /// <summary>
        /// The derivatives of the shape functions of the 6-node triangle at a Gauss point: corners L (2L - 1), middle points 4 Li Lj
        /// (nodes 4, 5, 6 between 1-2, 2-3, 3-1)
        /// </summary>
        private static void Derivatives(double[] l, double[] lx, double[] ly, double[] dx, double[] dy)
        {
            for (int k = 0; k < 3; k++)
            {
                dx[k] = (4.0 * l[k] - 1.0) * lx[k];
                dy[k] = (4.0 * l[k] - 1.0) * ly[k];
                int m = (k + 1) % 3;
                dx[3 + k] = 4.0 * (l[k] * lx[m] + l[m] * lx[k]);
                dy[3 + k] = 4.0 * (l[k] * ly[m] + l[m] * ly[k]);
            }
        }

        /// <summary>
        /// The value of a nodal field at a Gauss point of a 6-node triangle
        /// </summary>
        private static double Interpolate(double[] l, int[] t, double[] field, double[] shape6)
        {
            for (int k = 0; k < 3; k++)
            {
                shape6[k] = l[k] * (2.0 * l[k] - 1.0);
                shape6[3 + k] = 4.0 * l[k] * l[(k + 1) % 3];
            }
            double v = 0;
            for (int a = 0; a < 6; a++)
                v += shape6[a] * field[t[a]];
            return v;
        }

        /// <summary>
        /// The 6-point Gauss rule of the triangle (Dunavant, degree 4): barycentric coordinates and weight
        /// </summary>
        private static double[][] CreateGaussPoints()
        {
            const double a = 0.445948490915965, wa = 0.223381589678011;
            const double b = 0.091576213509771, wb = 0.109951743655322;
            return new[]
            {
                new[] { a, a, 1 - 2 * a, wa }, new[] { a, 1 - 2 * a, a, wa }, new[] { 1 - 2 * a, a, a, wa },
                new[] { b, b, 1 - 2 * b, wb }, new[] { b, 1 - 2 * b, b, wb }, new[] { 1 - 2 * b, b, b, wb },
            };
        }

        /// <summary>
        /// The conjugate gradient with the diagonal preconditioner (symmetric positive definite matrix in compressed rows), from x = 0
        /// </summary>
        /// <param name="start">The start of each row</param>
        /// <param name="column">The columns</param>
        /// <param name="value">The values</param>
        /// <param name="b">The known vector</param>
        /// <param name="x">The solution</param>
        /// <param name="iterations">The iterations</param>
        /// <returns>True if the residual is reduced under <see cref="Tolerance"/> times the known vector</returns>
        private static bool ConjugateGradient(int[] start, int[] column, double[] value, double[] b, double[] x, out int iterations)
        {
            int n = b.Length;
            var inverse = new double[n];
            for (int i = 0; i < n; i++)
            {
                int k = Position(start, column, i, i);
                inverse[i] = 1.0 / value[k];
            }

            var r = (double[])b.Clone();
            var z = new double[n];
            var p = new double[n];
            var q = new double[n];
            double normB = Math.Sqrt(Dot(b, b));
            iterations = 0;
            if (normB == 0)
                return true;

            for (int i = 0; i < n; i++)
                p[i] = z[i] = inverse[i] * r[i];
            double rz = Dot(r, z);
            int maximum = Math.Max(1000, Math.Min(200000, 2 * n));

            for (iterations = 1; iterations <= maximum; iterations++)
            {
                for (int i = 0; i < n; i++)
                {
                    double s = 0;
                    for (int k = start[i]; k < start[i + 1]; k++)
                        s += value[k] * p[column[k]];
                    q[i] = s;
                }
                double alpha = rz / Dot(p, q);
                for (int i = 0; i < n; i++)
                {
                    x[i] += alpha * p[i];
                    r[i] -= alpha * q[i];
                }
                if (Math.Sqrt(Dot(r, r)) <= Tolerance * normB)
                    return true;

                for (int i = 0; i < n; i++)
                    z[i] = inverse[i] * r[i];
                double rzNew = Dot(r, z);
                double beta = rzNew / rz;
                rz = rzNew;
                for (int i = 0; i < n; i++)
                    p[i] = z[i] + beta * p[i];
            }

            return false;
        }

        private static double Dot(double[] a, double[] b)
        {
            double s = 0;
            for (int i = 0; i < a.Length; i++)
                s += a[i] * b[i];
            return s;
        }
    }
}
