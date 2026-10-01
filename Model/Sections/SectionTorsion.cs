using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using System;
using System.Collections.Generic;

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
    /// Saint-Venant torsion and warping of a cross-section with the finite elements. The warping function φ of the twist about the centroid solves
    /// ∇²φ = 0 with ∂φ/∂n = y nx - x ny on the boundary (weak form: ∫ ∇φ·∇v dA = ∫ (y ∂v/∂x - x ∂v/∂y) dA); the elements are 6-node triangles
    /// on the triangulation of the region, integrated exactly (6 Gauss points), and the system is solved with the conjugate gradient. Then:
    /// torsion constant It = Ip - ∫ (y ∂φ/∂x - x ∂φ/∂y) dA; shear centre (Trefftz) the pole whose warping function φ + xs y - ys x is orthogonal
    /// to x and y; warping constant the integral of the square of that function with zero mean. The warping function is single valued, so
    /// the closed cells and the holes need no special treatment. Separate parts are solved together: the torsion constant is their sum,
    /// warping constant and shear centre are not defined
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
        /// Solves the torsion of a region
        /// </summary>
        /// <param name="shape">The region (fill minus holes plus childs)</param>
        /// <param name="meshSize">The size of the elements (not positive: <see cref="DefaultMeshSize"/>)</param>
        /// <returns>The torsion properties; not solved (see <see cref="SectionTorsionProperties.Error"/>) if the region has no area, the mesh
        /// fails or the solver does not converge</returns>
        internal static SectionTorsionProperties Calculate(Shape2d shape, double meshSize = 0)
        {
            if (shape is null || shape.Fill is null || shape.Fill.Count < 3)
                return new SectionTorsionProperties("the section has no region");

            Point3d origin = shape.Fill[0];
            SectionHelper.IntegrateShape(shape, origin.X, origin.Y, out double area, out double sx, out double sy, out _, out _, out _);
            if (!(area > 0))
                return new SectionTorsionProperties("the region has no area");
            double xc = origin.X + sy / area, yc = origin.Y + sx / area;

            double size = meshSize > 0 ? meshSize : DefaultMeshSize(shape, area);
            if (!(size > 0) || double.IsInfinity(size))
                return new SectionTorsionProperties("the size of the mesh can not be chosen");

            Mesh mesh = Triangulate(shape, size, out string meshError);
            if (mesh is null)
                return new SectionTorsionProperties("the mesh failed: " + meshError, size);

            // the nodes: the vertices of the mesh, then the middle points of the edges; coordinates relative to the centroid
            var index = new Dictionary<int, int>();
            var x = new List<double>();
            var y = new List<double>();
            foreach (MeshVertex vertex in mesh.Vertices)
            {
                index[vertex.Id] = x.Count;
                x.Add(vertex.Point.X - xc);
                y.Add(vertex.Point.Y - yc);
            }
            int vertices = x.Count;

            var triangles = new List<int[]>();
            void AddTriangle(int a, int b, int c)
            {
                double doubleArea = (x[b] - x[a]) * (y[c] - y[a]) - (x[c] - x[a]) * (y[b] - y[a]);
                if (doubleArea > 0)
                    triangles.Add(new[] { a, b, c, -1, -1, -1 });
                else if (doubleArea < 0)
                    triangles.Add(new[] { a, c, b, -1, -1, -1 });
            }
            foreach (MeshFace face in mesh.Faces)
            {
                int a = index[face.A], b = index[face.B], c = index[face.C];
                AddTriangle(a, b, c);
                if (face.IsQuad)
                    AddTriangle(a, c, index[face.D]);
            }
            if (triangles.Count == 0)
                return new SectionTorsionProperties("the mesh has no elements", size);

            var middle = new Dictionary<long, int>();
            foreach (int[] t in triangles)
            {
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
                    t[3 + k] = m;
                }
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
            foreach (int[] t in triangles)
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
            foreach (int[] t in triangles)
            {
                double x1 = x[t[0]], y1 = y[t[0]], x2 = x[t[1]], y2 = y[t[1]], x3 = x[t[2]], y3 = y[t[2]];
                double doubleArea = (x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1);
                double elementArea = 0.5 * doubleArea;
                double[] lx = { (y2 - y3) / doubleArea, (y3 - y1) / doubleArea, (y1 - y2) / doubleArea };
                double[] ly = { (x3 - x2) / doubleArea, (x1 - x3) / doubleArea, (x2 - x1) / doubleArea };

                Array.Clear(stiffness, 0, stiffness.Length);
                foreach (double[] g in GaussPoints)
                {
                    double weight = g[3] * elementArea;
                    Derivatives(g, lx, ly, dx, dy);
                    double xg = g[0] * x1 + g[1] * x2 + g[2] * x3, yg = g[0] * y1 + g[1] * y2 + g[2] * y3;
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
                return new SectionTorsionProperties(torsionConstant, double.NaN, new Point2d(double.NaN, double.NaN), parts, size, triangles.Count, n, iterations);

            // shear centre and warping constant: integrals on the centroidal axes
            double a0 = 0, ixx = 0, iyy = 0, ixy = 0, iphiX = 0, iphiY = 0;
            foreach (int[] t in triangles)
            {
                double x1 = x[t[0]], y1 = y[t[0]], x2 = x[t[1]], y2 = y[t[1]], x3 = x[t[2]], y3 = y[t[2]];
                double elementArea = 0.5 * ((x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1));
                foreach (double[] g in GaussPoints)
                {
                    double weight = g[3] * elementArea;
                    double xg = g[0] * x1 + g[1] * x2 + g[2] * x3, yg = g[0] * y1 + g[1] * y2 + g[2] * y3;
                    double phiG = Interpolate(g, t, phi, shape6);
                    a0 += weight;
                    ixx += weight * yg * yg;
                    iyy += weight * xg * xg;
                    ixy += weight * xg * yg;
                    iphiX += weight * phiG * xg;
                    iphiY += weight * phiG * yg;
                }
            }

            double determinant = ixx * iyy - ixy * ixy;
            double xs = (ixy * iphiX - iyy * iphiY) / determinant;
            double ys = (ixx * iphiX - ixy * iphiY) / determinant;

            // the warping function about the shear centre: φ + xs y - ys x (linear terms: exact at the nodes)
            var omega = new double[n];
            for (int i = 0; i < n; i++)
                omega[i] = phi[i] + xs * y[i] - ys * x[i];
            double mean = 0, square = 0;
            foreach (int[] t in triangles)
            {
                double x1 = x[t[0]], y1 = y[t[0]], x2 = x[t[1]], y2 = y[t[1]], x3 = x[t[2]], y3 = y[t[2]];
                double elementArea = 0.5 * ((x2 - x1) * (y3 - y1) - (x3 - x1) * (y2 - y1));
                foreach (double[] g in GaussPoints)
                {
                    double weight = g[3] * elementArea;
                    double omegaG = Interpolate(g, t, omega, shape6);
                    mean += weight * omegaG;
                    square += weight * omegaG * omegaG;
                }
            }
            double warpingConstant = Math.Max(0.0, square - mean * mean / a0);

            return new SectionTorsionProperties(torsionConstant, warpingConstant, new Point2d(xc + xs, yc + ys), 1, size, triangles.Count, n, iterations);
        }

        /// <summary>
        /// The default size of the mesh: half the minimum thickness of the region (two quadratic elements across the thinnest wall), at most
        /// 1/12 of the square root of the area (the compact regions), increased if the elements would be more than <see cref="MaximumElements"/>
        /// </summary>
        /// <param name="shape">The region</param>
        /// <param name="area">The area of the region</param>
        /// <returns>The size</returns>
        internal static double DefaultMeshSize(Shape shape, double area)
        {
            double size = Math.Min(0.5 * MinimumThickness(shape), Math.Sqrt(area) / 12.0);
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
