using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Converter.Straus7
{
    /// <summary>The BXS of a built-up section that is not welded keeps its parts apart (user decision 5 of 4/11, «separa i nodi»; review L10).
    /// <para>R31 merges the nodes of a BXS closer than a tolerance of its own (measured on 2 L 100×10 at three scales: relative to the size of the
    /// section, merged at 2.2e-5 of its diagonal and kept apart at 3.1e-5), so the parts of a built-up section that touch vertex to vertex (two
    /// angles back to back, two channels toe to toe, the angles of a cruciform without gap) became one body for the BXS properties of R31 and for
    /// its stress recovery: 2 L 100×10 back to back, J 2.98e5 against 1.27e5 of GPC, SA2 1799 against 1232. GPC connects such parts at
    /// discrete points (SectionBuiltUp: torsion constant and shear areas of separate bodies). A welded section is one conforming mesh
    /// (SectionWelded.GetConformingMesh) and never comes here.</para>
    /// <para>Each vertex of a part closer than <see cref="Ratio"/> × diagonal to a vertex of another part moves into its own part, along the
    /// bisector of its interior angle, by the distance that puts the vertices of different parts at least <see cref="Ratio"/> × diagonal apart
    /// (the touching vertices end up 1.4 to 2 times that distance apart, at least 2.3 times the largest distance R31 was seen to merge). The
    /// sides along a contact move with their vertices: each part loses a sliver of that width along its contacts (2 L 100×10: 6e-4 of its
    /// area). Splitting those sides near the vertices, to lose only the corners, does not work: R31 then joins the parts along the collinear
    /// pieces of the contact (2 L 100×10: SA2 1367 instead of 1238; review L10, measured). Parts that do not touch keep their mesh value for
    /// value; the GPC properties written over the BXS never change.</para></summary>
    internal static class Straus7SectionPartSeparation
    {
        /// <summary>Separation of the vertices of different parts, relative to the diagonal of the section.</summary>
        internal const double Ratio = 5e-5;

        /// <summary>Moves the touching vertices of the parts (vertices X, Y, 0 in mm; faces as zero-based indices of their part).</summary>
        /// <returns>The number of vertices moved.</returns>
        /// <exception cref="NotSupportedException">StrausBxsCannotSeparateTouchingParts: the vertices cannot be moved apart without folding a face.</exception>
        internal static int Separate(IList<(List<double[]> Vertices, List<int[]> Faces)> parts, string section)
        {
            if (parts == null || parts.Count < 2) return 0;
            double minX = double.MaxValue, minY = double.MaxValue, maxX = double.MinValue, maxY = double.MinValue;
            foreach (var part in parts) foreach (var v in part.Vertices) { minX = Math.Min(minX, v[0]); maxX = Math.Max(maxX, v[0]); minY = Math.Min(minY, v[1]); maxY = Math.Max(maxY, v[1]); }
            double diagonal = Math.Sqrt((maxX - minX) * (maxX - minX) + (maxY - minY) * (maxY - minY));
            if (!(diagonal > 0)) return 0;
            double gap = Ratio * diagonal;
            // Touching vertices of different parts, grouped by union-find over (part, index).
            var all = new List<(int Part, int Index)>();
            for (int p = 0; p < parts.Count; p++) for (int i = 0; i < parts[p].Vertices.Count; i++) all.Add((p, i));
            var parent = Enumerable.Range(0, all.Count).ToArray();
            int Find(int i) { while (parent[i] != i) i = parent[i] = parent[parent[i]]; return i; }
            var touching = new SortedSet<int>();
            for (int a = 0; a < all.Count; a++)
                for (int b = a + 1; b < all.Count; b++)
                {
                    if (all[a].Part == all[b].Part || Distance(Vertex(parts, all[a]), Vertex(parts, all[b])) >= gap) continue;
                    parent[Find(a)] = Find(b); touching.Add(a); touching.Add(b);
                }
            if (touching.Count == 0) return 0;
            var bisector = touching.ToDictionary(k => k, k => Bisector(parts[all[k].Part], all[k].Index, section));
            var original = touching.ToDictionary(k => k, k => (double[])Vertex(parts, all[k]).Clone());
            foreach (var members in touching.GroupBy(Find).Select(g => g.ToArray()))
            {
                // The smallest step (the gap, doubled at most six times) that puts the vertices of different parts of the cluster the gap apart.
                double step = gap; bool apart = false;
                for (int attempt = 0; attempt < 7 && !apart; attempt++, step *= 2)
                {
                    foreach (int k in members) { var p = Vertex(parts, all[k]); var d = bisector[k]; p[0] = original[k][0] + step * d[0]; p[1] = original[k][1] + step * d[1]; }
                    apart = members.All(i => members.All(j => all[i].Part == all[j].Part || Distance(Vertex(parts, all[i]), Vertex(parts, all[j])) >= gap));
                }
                if (!apart) throw new NotSupportedException("StrausBxsCannotSeparateTouchingParts: " + section);
            }
            // Every vertex the gap from every vertex of the other parts, and no face with a moved vertex folded or halved.
            for (int p = 0; p < parts.Count; p++)
                for (int q = p + 1; q < parts.Count; q++)
                    foreach (var u in parts[p].Vertices) foreach (var w in parts[q].Vertices)
                        if (Distance(u, w) < gap) throw new NotSupportedException("StrausBxsCannotSeparateTouchingParts: " + section);
            foreach (var group in touching.GroupBy(k => all[k].Part))
            {
                var vertices = parts[group.Key].Vertices; var moved = group.ToDictionary(k => all[k].Index, k => original[k]);
                foreach (var face in parts[group.Key].Faces.Where(f => f.Any(moved.ContainsKey)))
                {
                    double before = SignedArea(face.Select(i => moved.TryGetValue(i, out var start) ? start : vertices[i]).ToArray());
                    double after = SignedArea(face.Select(i => vertices[i]).ToArray());
                    if (Math.Sign(before) != Math.Sign(after) || Math.Abs(after) < .5 * Math.Abs(before))
                        throw new NotSupportedException("StrausBxsCannotSeparateTouchingParts: " + section + " (a face of a part would fold)");
                }
            }
            return touching.Count;
        }

        private static double[] Vertex(IList<(List<double[]> Vertices, List<int[]> Faces)> parts, (int Part, int Index) key) => parts[key.Part].Vertices[key.Index];
        private static double Distance(double[] a, double[] b) { double dx = a[0] - b[0], dy = a[1] - b[1]; return Math.Sqrt(dx * dx + dy * dy); }
        private static double SignedArea(double[][] polygon)
        {
            double area = 0;
            for (int i = 0; i < polygon.Length; i++) { var a = polygon[i]; var b = polygon[(i + 1) % polygon.Length]; area += a[0] * b[1] - b[0] * a[1]; }
            return area / 2;
        }

        /// <summary>The unit bisector of the interior angle of a part at a vertex of its boundary: the boundary sides are the sides of the
        /// counterclockwise faces that no other face of the part uses; the interior goes counterclockwise from the outgoing side to the incoming one.</summary>
        private static double[] Bisector((List<double[]> Vertices, List<int[]> Faces) part, int vertex, string section)
        {
            var directed = new HashSet<(int, int)>();
            foreach (var face in part.Faces)
            {
                var ring = SignedArea(face.Select(i => part.Vertices[i]).ToArray()) >= 0 ? face : face.Reverse().ToArray();
                for (int k = 0; k < ring.Length; k++) directed.Add((ring[k], ring[(k + 1) % ring.Length]));
            }
            var outgoing = directed.Where(e => e.Item1 == vertex && !directed.Contains((e.Item2, e.Item1))).ToArray();
            var incoming = directed.Where(e => e.Item2 == vertex && !directed.Contains((e.Item2, e.Item1))).ToArray();
            if (outgoing.Length != 1 || incoming.Length != 1)
                throw new NotSupportedException("StrausBxsCannotSeparateTouchingParts: " + section + " (a touching vertex is not on one boundary of its part)");
            var v = part.Vertices[vertex]; var next = part.Vertices[outgoing[0].Item2]; var previous = part.Vertices[incoming[0].Item1];
            double toNext = Math.Atan2(next[1] - v[1], next[0] - v[0]), toPrevious = Math.Atan2(previous[1] - v[1], previous[0] - v[0]);
            double interior = toPrevious - toNext; while (interior <= 0) interior += 2 * Math.PI; while (interior > 2 * Math.PI) interior -= 2 * Math.PI;
            double angle = toNext + interior / 2;
            return new[] { Math.Cos(angle), Math.Sin(angle) };
        }
    }
}
