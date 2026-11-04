using GPC.Converter.Straus7;
using GPC.Model.Sections;
using SectionShearAreas = GPC.Model.Sections.SectionShearAreas;

namespace UnitTest;

/// <summary>Review L10 (4/11/2026), user decision 5 on the Straus7 writer («separa i nodi»): the parts of a built-up section that is not
/// welded stay separate bodies in the BXS, as GPC treats them; welded sections keep their conforming mesh.</summary>
[TestClass]
public class ReviewFixesL10BxsTest
{

    /// <summary>Non-welded built-up sections whose parts touch vertex to vertex, with the same section with its parts apart for reference.</summary>
    internal static (string Name, SectionBuiltUp Touching, SectionBuiltUp Apart)[] TouchingSections()
    {
        var angle = new SectionL(100, 10, 100, 10, "L"); var channel = new SectionC(200, 8.5, 75, 11.5, 75, 11.5, "C 200 sharp");
        return new[] {
            ("2 L 100x10 back to back", SectionBuiltUp.DoubleAngle(angle, 0, "2L"), SectionBuiltUp.DoubleAngle(angle, 10, "2L gap 10")),
            ("2 L 10x1 back to back", SectionBuiltUp.DoubleAngle(new SectionL(10, 1, 10, 1, "L"), 0, "2L small"), SectionBuiltUp.DoubleAngle(new SectionL(10, 1, 10, 1, "L"), 1, "2L small gap 1")),
            ("2 L 1000x100 back to back", SectionBuiltUp.DoubleAngle(new SectionL(1000, 100, 1000, 100, "L"), 0, "2L large"), SectionBuiltUp.DoubleAngle(new SectionL(1000, 100, 1000, 100, "L"), 100, "2L large gap 100")),
            ("2 C 200 toe to toe", SectionBuiltUp.DoubleChannel(channel, 0, false, "2C box"), SectionBuiltUp.DoubleChannel(channel, 10, false, "2C gap 10")),
            ("4 L 100x10 cruciform", SectionBuiltUp.Cruciform(angle, 0, "4L"), SectionBuiltUp.Cruciform(angle, 10, "4L gap 10")),
        };
    }

    /// <summary>The vertices of a BXS sent part by part, by part (connected components of the faces, in the order of their vertices).</summary>
    private static int[] Parts(double[][] vertices, int[][] faces)
    {
        var parent = Enumerable.Range(0, vertices.Length).ToArray();
        int Find(int i) { while (parent[i] != i) i = parent[i] = parent[parent[i]]; return i; }
        foreach (var f in faces) for (int k = 1; k < f.Length; k++) parent[Find(f[k])] = Find(f[0]);
        var order = new Dictionary<int, int>(); var owner = new int[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) { int root = Find(i); if (!order.TryGetValue(root, out int part)) order.Add(root, part = order.Count); owner[i] = part; }
        return owner;
    }
    private static double PartGap(double[][] vertices, int[] owner)
    {
        double gap = double.MaxValue;
        for (int i = 0; i < vertices.Length; i++)
            for (int j = i + 1; j < vertices.Length; j++)
                if (owner[i] != owner[j]) gap = Math.Min(gap, Math.Sqrt(Math.Pow(vertices[i][0] - vertices[j][0], 2) + Math.Pow(vertices[i][1] - vertices[j][1], 2)));
        return gap;
    }
    private static double Diagonal(double[][] v) => Math.Sqrt(Math.Pow(v.Max(p => p[0]) - v.Min(p => p[0]), 2) + Math.Pow(v.Max(p => p[1]) - v.Min(p => p[1]), 2));
    private static double MeshArea(double[][] v, int[][] faces) => faces.Sum(f => Math.Abs((v[f[1]][0] - v[f[0]][0]) * (v[f[2]][1] - v[f[0]][1]) - (v[f[2]][0] - v[f[0]][0]) * (v[f[1]][1] - v[f[0]][1])) / 2);

    [TestMethod]
    public void L10_TouchingPartsOfABuiltUpSectionAreKeptApartInTheBxs()
    {
        // Before: the parts meshed one by one with their touching vertices at the same point, which R31 merges (one body: 2 L back to back
        // J 2.98e5 against 1.27e5 of GPC). Now the touching vertices move into their own part: the vertices of different parts are at least
        // 5e-5 of the diagonal apart (merged by R31 at 2.2e-5, kept apart at 3.1e-5 of the diagonal); the sides along the contact move with them,
        // so each part loses a sliver along its contacts (below 2e-3 of the area); every other vertex keeps its value, and no vertex is added.
        foreach (var (name, touching, apart) in TouchingSections())
        {
            var outlines = touching.GetOutlines(); var before = ReviewFixesL6Test.PartsApart(outlines); var (vertices, faces) = ReviewFixesL6Test.SentBxs(touching);
            var ownerBefore = Parts(before.Vertices, before.Faces); var owner = Parts(vertices, faces);
            Assert.AreEqual(outlines.Count, ownerBefore.Distinct().Count(), name); Assert.AreEqual(outlines.Count, owner.Distinct().Count(), name + ": one body per part");
            double diagonal = Diagonal(before.Vertices), gapBefore = PartGap(before.Vertices, ownerBefore), gapAfter = PartGap(vertices, owner);
            // The original vertices of each part come first in its block, in the same order.
            int moved = 0, added = 0; double maxMove = 0;
            for (int part = 0; part < outlines.Count; part++)
            {
                var old = Enumerable.Range(0, before.Vertices.Length).Where(i => ownerBefore[i] == part).ToArray(); var now = Enumerable.Range(0, vertices.Length).Where(i => owner[i] == part).ToArray();
                added += now.Length - old.Length;
                for (int k = 0; k < old.Length; k++)
                {
                    var a = before.Vertices[old[k]]; var b = vertices[now[k]]; double d = Math.Sqrt(Math.Pow(a[0] - b[0], 2) + Math.Pow(a[1] - b[1], 2));
                    if (d != 0) moved++; maxMove = Math.Max(maxMove, d);
                }
            }
            double area = MeshArea(vertices, faces), areaBefore = MeshArea(before.Vertices, before.Faces);
            Console.WriteLine($"L10 BXS {name}: {before.Vertices.Length} -> {vertices.Length} vertices ({added} added on the sides), {before.Faces.Length} -> {faces.Length} faces; {moved} moved by at most {maxMove:E3} mm ({maxMove / diagonal:E2} of the diagonal); " +
                $"gap between parts {gapBefore:E2} -> {gapAfter:E3} mm ({gapAfter / diagonal:E2} of the diagonal); area {areaBefore:F4} -> {area:F4} ({area / areaBefore - 1:E2})");
            Assert.AreEqual(0, gapBefore, 1e-9 * diagonal, name + ": the parts touch");
            Assert.IsTrue(gapAfter >= .999999 * Straus7SectionPartSeparation_Ratio * diagonal, name + ": parts apart " + gapAfter);
            Assert.IsTrue(moved > 0 && maxMove <= 1.5 * Straus7SectionPartSeparation_Ratio * diagonal, name + ": negligible move " + maxMove);
            Assert.AreEqual(areaBefore, area, 2e-3 * areaBefore, name + ": area of the BXS"); Assert.AreEqual(0, added, name + ": no vertex added");
            Assert.AreEqual(touching.Area, areaBefore, 1e-9 * touching.Area, name);
            // Parts apart (gap) keep their BXS value for value: nothing to separate.
            var reference = ReviewFixesL6Test.PartsApart(apart.GetOutlines()); var sent = ReviewFixesL6Test.SentBxs(apart);
            Assert.AreEqual(reference.Vertices.Length, sent.Vertices.Length, apart.Name); Assert.AreEqual(reference.Faces.Length, sent.Faces.Length, apart.Name);
            for (int i = 0; i < sent.Vertices.Length; i++) CollectionAssert.AreEqual(reference.Vertices[i], sent.Vertices[i], apart.Name);
            for (int i = 0; i < sent.Faces.Length; i++) CollectionAssert.AreEqual(reference.Faces[i], sent.Faces[i], apart.Name);
        }
        // Welded sections keep their conforming mesh value for value (one body for R31, review L6).
        foreach (var (name, welded) in ReviewFixesL6Test.WeldedSections())
        {
            var (vertices, faces) = ReviewFixesL6Test.SentBxs(welded); var conforming = welded.GetConformingMesh();
            Assert.AreEqual(conforming.Vertices.Length, vertices.Length, name);
            for (int i = 0; i < vertices.Length; i++) CollectionAssert.AreEqual(conforming.Vertices[i], vertices[i], name);
            for (int i = 0; i < faces.Length; i++) CollectionAssert.AreEqual(conforming.Triangles[i], faces[i], name);
        }
    }

    /// <summary>Straus7SectionPartSeparation.Ratio (internal to the Converter).</summary>
    private const double Straus7SectionPartSeparation_Ratio = 5e-5;

    [TestMethod, TestCategory("Straus7Native")]
    public void L10_NativeTouchingPartsOfABuiltUpSectionStaySeparateBodiesInR31()
    {
        // What R31 computes on the BXS the writer sends (St7GenerateBXS: Area 2, I11 3, I22 4, SA1 14, SA2 15, J 32): before (touching
        // vertices, merged by R31) and after (kept apart), against GPC (J of separate parts, SA at ν = 0) and against R31 on the same section
        // with its parts apart. Tolerances of the L6 test of the welded sections: J 3 %, SA 2 %.
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        using var writer = new Straus7NativeWriter(Path.Combine(folder, "bxs.st7"), folder);
        static double[] Pair(double a, double b) => new[] { a, b }.OrderBy(v => v).ToArray();
        // Observation: R31's merge tolerance (2 L 100x10, diagonal 223.6 mm; merged at 1e-3 mm, apart at 1e-2 mm).
        var angle = new SectionL(100, 10, 100, 10, "L");
        foreach (double gap in new[] { 1.5e-3, 2e-3, 2.5e-3, 3e-3, 4e-3, 5e-3, 7e-3 })
        {
            var pair = SectionBuiltUp.DoubleAngle(angle, gap, "2L"); var (pv, pf) = ReviewFixesL6Test.PartsApart(pair.GetOutlines()); var p = writer.NativeSectionMeshProperties(pv, pf);
            Console.WriteLine($"L10 native BXS 2 L 100x10, parts {gap:E1} mm apart ({gap / 223.607:E2} of the diagonal) without separation: R31 J {p[32]:E5} ({(p[32] > 2e5 ? "merged" : "apart")})");
        }
        var failures = new List<string>();
        void Check(bool condition, string message) { if (!condition) failures.Add(message); }
        foreach (var (name, touching, apart) in TouchingSections())
        {
            var gpc = SectionShearAreas.Compute(touching); var shear = Pair(gpc.Area1, gpc.Area2);
            var before = ReviewFixesL6Test.PartsApart(touching.GetOutlines()); var old = writer.NativeSectionMeshProperties(before.Vertices, before.Faces);
            var (v, f) = ReviewFixesL6Test.SentBxs(touching); var now = writer.NativeSectionMeshProperties(v, f);
            var (av, af) = ReviewFixesL6Test.SentBxs(apart); var reference = writer.NativeSectionMeshProperties(av, af); var referenceGpc = SectionShearAreas.Compute(apart);
            string Line(string label, double[] p) { var s = Pair(p[14], p[15]); return $"{label}: A {p[2]:G7} ({p[2] / touching.Area - 1:E2}), J {p[32]:E5} ({p[32] / touching.Jt - 1:P2}), SA {s[1]:F1} / {s[0]:F1} ({s[1] / shear[1] - 1:P2} / {s[0] / shear[0] - 1:P2})"; }
            Console.WriteLine($"L10 native BXS {name}: GPC A {touching.Area:G7}, J {touching.Jt:E5}, SA ν=0 {shear[1]:F1} / {shear[0]:F1} ({gpc.Bodies} bodies)");
            Console.WriteLine($"L10 native BXS {name} " + Line("before (touching, merged by R31)", old));
            Console.WriteLine($"L10 native BXS {name} " + Line("after (kept apart)", now));
            Console.WriteLine($"L10 native BXS {name} reference {apart.Name}: R31 J {reference[32]:E5} of GPC {apart.Jt:E5} ({reference[32] / apart.Jt - 1:P2}), SA {Pair(reference[14], reference[15])[1]:F1} / {Pair(reference[14], reference[15])[0]:F1} of GPC {Pair(referenceGpc.Area1, referenceGpc.Area2)[1]:F1} / {Pair(referenceGpc.Area1, referenceGpc.Area2)[0]:F1}");
            Check(old[32] > 1.5 * touching.Jt, name + ": before, R31 merged the parts (J " + old[32] + ")");
            Check(Math.Abs(now[32] - touching.Jt) <= .03 * touching.Jt, name + " J " + now[32]);
            var s = Pair(now[14], now[15]);
            Check(Math.Abs(s[1] - shear[1]) <= .02 * shear[1] && Math.Abs(s[0] - shear[0]) <= .02 * shear[0], name + " SA " + s[1] + " / " + s[0]);
            Check(Math.Abs(now[2] - touching.Area) <= 2e-3 * touching.Area, name + " A " + now[2]);
            var i = Pair(now[3], now[4]); var gi = Pair(touching.J11, touching.J22);
            Check(Math.Abs(i[1] - gi[1]) <= 4e-3 * gi[1] && Math.Abs(i[0] - gi[0]) <= 4e-3 * gi[0], name + " I " + i[1] + " / " + i[0]);
            // R31 on the touching parts kept apart as on the same parts with a gap (the coarse BXS of R31, not the separation, sets the residue).
            Check(Math.Abs(now[32] - reference[32] * touching.Jt / apart.Jt) <= .01 * now[32], name + " J against the parts with a gap");
        }
        Assert.AreEqual(0, failures.Count, string.Join("; ", failures));
    }

}
