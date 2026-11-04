using GPC.Geometry;
using GPC.Model.Sections;
using SectionShearAreas = GPC.Model.Sections.SectionShearAreas;

namespace UnitTest;

/// <summary>Review L6 (4/11/2026), the follow-ups of L4: the shear-area solver in GPCModel with a public API, the conforming BXS of the
/// welded sections, the centrifugal force of moving loads in global axes and the path axes.</summary>
[TestClass]
public class ReviewFixesL6Test
{
    /// <summary>An I of 200 × 15 flanges and a 270 × 10 web as one outline (a generic section, as an adapter builds it from its plates).</summary>
    private static Shape2d IOutline() => new Shape2d(new Polygon2d(new[] { (0d, 0d), (200, 0), (200, 15), (105, 15), (105, 285), (200, 285), (200, 300),
        (0, 300), (0, 285), (95, 285), (95, 15), (0, 15) }.Select(p => new Point2d(p.Item1, p.Item2)).ToArray()));

    [TestMethod]
    public void L6_ShearAreasOfGpcModelReturnTheAreasTheBodiesAndTheMethod()
    {
        // The solver lives in GPCModel (GPC.Model.Sections), no longer in the Converter behind InternalsVisibleTo.
        Assert.AreEqual("GPCModel", typeof(SectionShearAreas).Assembly.GetName().Name);
        Assert.IsFalse(typeof(SectionShearAreas).Assembly.GetCustomAttributes(typeof(System.Runtime.CompilerServices.InternalsVisibleToAttribute), false)
            .Cast<System.Runtime.CompilerServices.InternalsVisibleToAttribute>().Any(a => a.AssemblyName == "GPC.Converter"), "no InternalsVisibleTo for the Converter");
        var h = new SectionH(300, 10, 200, 15, 200, 15, "I");
        var single = SectionShearAreas.Compute(h, .3);
        Assert.AreEqual(1, single.Bodies); Assert.IsFalse(single.HasSeparateParts); Assert.AreEqual(SectionShearAreaMethod.SaintVenantFlexure, single.Method);
        Assert.AreEqual(.3, single.PoissonRatio); Assert.IsTrue(single.Nodes > single.Triangles / 3, single.ToString());
        var (a1, a2) = single; Assert.AreEqual(single.Area1, a1); Assert.AreEqual(single.Area2, a2);
        var pair = SectionShearAreas.Compute(SectionBuiltUp.DoubleAngle(new SectionL(100, 10, 100, 10, "L"), 10, "2L"), .3);
        Assert.AreEqual(2, pair.Bodies); Assert.IsTrue(pair.HasSeparateParts); Assert.AreEqual(SectionShearAreaMethod.SeparatePartsBendingTogether, pair.Method);
        // The forwarder kept for the F6 branches gives the same values, bit for bit.
#pragma warning disable CS0618
        var forwarded = GPC.Converter.SectionShearAreas.Compute(h, .3, out int bodies);
#pragma warning restore CS0618
        Assert.AreEqual(single.Area1, forwarded.Area1); Assert.AreEqual(single.Area2, forwarded.Area2); Assert.AreEqual(1, bodies);
    }

    [TestMethod]
    public void L6_TheAnalysisMeshHasAnExplicitDensityWhateverTheSection()
    {
        // Before (review L4, latent): the mesh size came from the area of an uncomputed section (0), so every region took the default
        // DelaunayMesh size (half its smaller side) and was subdivided until 20 000 triangles: 20 480 for a bridge girder, 73 728 for a
        // rectangle 2:1, 76 288 for a cover-plated I, by accident of the shape. Now 6.4 √(A/N) with 20°, then two subdivisions: about N.
        const int n = SectionShearAreas.TargetTriangles;
        var sections = new ISectionShape[] { new SectionRectangular(200, 100), new SectionRectangular(2000, 1000), new SectionCircular(100),
            new SectionH(300, 10, 200, 15, 200, 15, "I"), new SectionH(1800, 16, 600, 30, 800, 40, "girder"), new SectionL(50, 5, 50, 5, "L50"),
            ShearAreaPartsTest.CoverPlated(), SectionWelded.CruciformPlates(317, 263, 13) };
        foreach (var section in sections)
        {
            var areas = SectionShearAreas.Compute(section, .3);
            Console.WriteLine($"L6 mesh {section.Name} {section.GetType().Name}: {areas}");
            Assert.AreEqual(SectionShearAreas.MeshSize(section.Area), areas.MeshSize, 1e-12 * areas.MeshSize, section.Name);
            Assert.AreEqual(6.4 * Math.Sqrt(section.Area / n), areas.MeshSize, 1e-12 * areas.MeshSize, section.Name);
            Assert.IsTrue(areas.Triangles > .85 * n && areas.Triangles < 1.25 * n, section.Name + ": " + areas.Triangles + " triangles");
        }
        // A wall thinner than the size gets smaller triangles, not a coarser solution.
        var pipe = SectionShearAreas.Compute(new SectionCHS(500, 5), .3);
        Assert.IsTrue(pipe.Triangles > n && pipe.Triangles < 2 * n, pipe.ToString());
    }

    /// <summary>A welded plate girder of three plates: bottom flange 800 × 40, web 1730 × 16, top flange 600 × 30 (a BridGHe girder).</summary>
    internal static SectionWelded PlateGirder() => new SectionWelded(new[] {
        new SectionBuiltUp.Part(new SectionRectangular(40, 800, "bottom flange"), -400, 0), new SectionBuiltUp.Part(new SectionRectangular(1730, 16, "web"), -8, 40),
        new SectionBuiltUp.Part(new SectionRectangular(30, 600, "top flange"), -300, 1770) }, "welded girder");

    /// <summary>The welded sections of the BXS checks: open and closed, plates of equal and different widths (T-junctions).</summary>
    internal static (string Name, SectionWelded Section)[] WeldedSections() => new[] {
        ("cruciform 317x263x13", SectionWelded.CruciformPlates(317, 263, 13, "cruciform")), ("cover plated I 400 + 300x20", ShearAreaPartsTest.CoverPlated()),
        ("channel box 2 C 200x75", SectionWelded.ChannelBox(new SectionC(200, 8.5, 75, 11.5, 75, 11.5, "C 200 sharp"), "channel box")),
        ("plate girder 1800", PlateGirder()) };

    /// <summary>The BXS the writer sends for the section of a cantilever (elastic property: the BXS does not depend on the property type, and a
    /// SteelSection does not take a generic polygon): vertices and faces of SetBeamSectionMesh.</summary>
    private static (double[][] Vertices, int[][] Faces) SentBxs(ISectionShape section)
    {
        var model = Straus7ModelWriteTest.Cantilever();
        model.BeamElements.Values.Single().BeamProperty = new GPC.Model.ElementProperties.ElasticBeamProperty("BXS", new GPC.Model.Materials.Material("E", 210000, .3, 0, 0), section);
        var call = Straus7ExporterCorrectionsTest.Record(model).Named(nameof(GPC.Converter.Straus7.IStraus7SectionMeshWriteApi.SetBeamSectionMesh)).Single();
        return ((double[][])call.Args[1]!, (int[][])call.Args[2]!);
    }

    /// <summary>The connected components of a triangulation (faces sharing a vertex) and the vertices lying strictly inside a side that only
    /// one face uses (hanging nodes: the faces on the two sides of it are not connected along it).</summary>
    private static (int Components, int Hanging) Topology(double[][] vertices, int[][] faces)
    {
        var parent = Enumerable.Range(0, vertices.Length).ToArray();
        int Find(int i) { while (parent[i] != i) i = parent[i] = parent[parent[i]]; return i; }
        foreach (var f in faces) for (int k = 1; k < f.Length; k++) parent[Find(f[k])] = Find(f[0]);
        int components = faces.Select(f => Find(f[0])).Distinct().Count();
        var uses = new Dictionary<(int, int), int>();
        foreach (var f in faces) for (int k = 0; k < f.Length; k++) { int a = f[k], b = f[(k + 1) % f.Length]; var key = a < b ? (a, b) : (b, a); uses[key] = uses.TryGetValue(key, out int u) ? u + 1 : 1; }
        double scale = vertices.Max(v => Math.Max(Math.Abs(v[0]), Math.Abs(v[1]))), tolerance = 1e-9 * scale; int hanging = 0;
        foreach (var edge in uses.Where(e => e.Value == 1).Select(e => e.Key))
        {
            double[] p = vertices[edge.Item1], q = vertices[edge.Item2]; double ex = q[0] - p[0], ey = q[1] - p[1], length = Math.Sqrt(ex * ex + ey * ey);
            for (int v = 0; v < vertices.Length; v++)
            {
                if (v == edge.Item1 || v == edge.Item2) continue;
                double wx = vertices[v][0] - p[0], wy = vertices[v][1] - p[1], t = (wx * ex + wy * ey) / (length * length);
                if (t * length > tolerance && (1 - t) * length > tolerance && Math.Abs(ex * wy - ey * wx) / length <= tolerance) hanging++;
            }
        }
        return (components, hanging);
    }

    /// <summary>The BXS of the writer before L6: each outline meshed on its own (initial mesh), on separate nodes.</summary>
    internal static (double[][] Vertices, int[][] Faces) PartsApart(IEnumerable<GPC.Geometry.Shape2d> outlines)
    {
        var vertices = new List<double[]>(); var faces = new List<int[]>();
        foreach (var outline in outlines)
        {
            var mesh = new Section(outline).GetMesh(initialMeshOnly: true); var ids = new Dictionary<int, int>();
            foreach (var v in mesh.Vertices) { ids.Add(v.Id, vertices.Count); vertices.Add(new[] { v.Point.X, v.Point.Y, v.Point.Z }); }
            foreach (var face in mesh.Faces) faces.Add(mesh.GetFaceVertices(face).Select(v => ids[v.Id]).ToArray());
        }
        return (vertices.ToArray(), faces.ToArray());
    }

    [TestMethod]
    public void L6_TheBxsOfAWeldedSectionIsOneConformingBody()
    {
        // Before: the parts of a welded section were meshed one by one on separate nodes, so R31 recovered the stresses of a disconnected
        // cruciform (its own SA1 13 % low). Now the joined mesh of SectionWelded.GetConformingMesh: one body, no hanging node at the T-junctions.
        foreach (var (name, welded) in WeldedSections())
        {
            var (vertices, faces) = SentBxs(welded);
            var (components, hanging) = Topology(vertices, faces);
            var apart = Topology(PartsApart(welded.GetOutlines()).Vertices, PartsApart(welded.GetOutlines()).Faces);
            Console.WriteLine($"L6 BXS {name}: {vertices.Length} nodes, {faces.Length} faces, {components} component(s), {hanging} hanging; parts apart {apart.Components} components");
            Assert.AreEqual(1, components, name); Assert.AreEqual(0, hanging, name);
            Assert.IsTrue(apart.Components > 1, name + ": the old BXS had separate parts");
            double area = faces.Sum(f => Math.Abs((vertices[f[1]][0] - vertices[f[0]][0]) * (vertices[f[2]][1] - vertices[f[0]][1]) - (vertices[f[2]][0] - vertices[f[0]][0]) * (vertices[f[1]][1] - vertices[f[0]][1])) / 2);
            Assert.AreEqual(welded.Area, area, 1e-9 * welded.Area, name + " area of the BXS");
        }
        // Built-up sections connected at discrete points and single regions keep their BXS, value for value.
        var pair = SectionBuiltUp.DoubleAngle(new SectionL(100, 10, 100, 10, "L"), 10, "2L");
        foreach (var section in new ISectionShape[] { pair, new SectionL(100, 10, 100, 10, "L"), new SectionT(300, 10, 200, 15, "T") })
        {
            var (vertices, faces) = SentBxs(section);
            var expected = PartsApart(section is SectionBuiltUp built ? built.GetOutlines() : new[] { section.Shape });
            Assert.AreEqual(expected.Vertices.Length, vertices.Length, section.Name);
            for (int i = 0; i < vertices.Length; i++) CollectionAssert.AreEqual(expected.Vertices[i], vertices[i], section.Name);
            Assert.AreEqual(expected.Faces.Length, faces.Length, section.Name);
            for (int i = 0; i < faces.Length; i++) CollectionAssert.AreEqual(expected.Faces[i], faces[i], section.Name);
        }
        Assert.AreEqual(2, Topology(SentBxs(pair).Vertices, SentBxs(pair).Faces).Components, "the double angle stays two bodies");
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void L6_NativeBxsOfWeldedSectionsGivesTheGpcProperties()
    {
        // What R31 computes on the BXS the writer sends (St7GenerateBXS: ipBXS Area 2, I11 3, I22 4, SA1 14, SA2 15, J 32), before (parts
        // apart) and after (conforming mesh), against GPC: area and inertias exact, J of the joined FE torsion, SA at ν = 0 (R31 has no
        // Poisson term). R31's axes may be swapped against GPC's: inertias and areas are compared as sorted pairs.
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        using var writer = new GPC.Converter.Straus7.Straus7NativeWriter(Path.Combine(folder, "bxs.st7"), folder);
        foreach (var (name, welded) in WeldedSections())
        {
            var gpc = SectionShearAreas.Compute(welded, 0);
            var (bv, bf) = PartsApart(welded.GetOutlines()); var before = writer.NativeSectionMeshProperties(bv, bf);
            var (av, af) = SentBxs(welded); var after = writer.NativeSectionMeshProperties(av, af);
            static double[] Pair(double a, double b) => new[] { a, b }.OrderBy(v => v).ToArray();
            var inertia = Pair(welded.J11, welded.J22); var shear = Pair(gpc.Area1, gpc.Area2);
            string Line(string label, double[] p) { var i = Pair(p[3], p[4]); var s = Pair(p[14], p[15]);
                return $"{label}: A {p[2]:F1} ({p[2] / welded.Area - 1:P3}), I {i[1]:E5} / {i[0]:E5} ({i[1] / inertia[1] - 1:P3} / {i[0] / inertia[0] - 1:P3}), " +
                    $"J {p[32]:E5} ({p[32] / welded.Jt - 1:P2}), SA {s[1]:F1} / {s[0]:F1} ({s[1] / shear[1] - 1:P2} / {s[0] / shear[0] - 1:P2})"; }
            Console.WriteLine($"L6 native BXS {name}: GPC A {welded.Area:F1}, I {inertia[1]:E5} / {inertia[0]:E5}, J {welded.Jt:E5}, SA ν=0 {shear[1]:F1} / {shear[0]:F1}");
            Console.WriteLine("L6 native BXS " + name + " " + Line("before (parts apart)", before));
            Console.WriteLine("L6 native BXS " + name + " " + Line("after (conforming)", after));
            Assert.AreEqual(welded.Area, after[2], 1e-6 * welded.Area, name + " A");
            var ri = Pair(after[3], after[4]); Assert.AreEqual(inertia[1], ri[1], 1e-6 * inertia[1], name + " I11"); Assert.AreEqual(inertia[0], ri[0], 1e-6 * inertia[0], name + " I22");
            Assert.AreEqual(welded.Jt, after[32], .03 * welded.Jt, name + " J");
            var rs = Pair(after[14], after[15]); Assert.AreEqual(shear[1], rs[1], .02 * shear[1], name + " SA larger"); Assert.AreEqual(shear[0], rs[0], .02 * shear[0], name + " SA smaller");
        }
        // The disconnected cruciform of the old BXS: its smaller shear area 13 % low (L4); the conforming one within 2 %.
        var cross = WeldedSections()[0].Section; var crossGpc = SectionShearAreas.Compute(cross, 0);
        var old = writer.NativeSectionMeshProperties(PartsApart(cross.GetOutlines()).Vertices, PartsApart(cross.GetOutlines()).Faces);
        Assert.IsTrue(Math.Min(old[14], old[15]) < .9 * Math.Min(crossGpc.Area1, crossGpc.Area2), "the old BXS was disconnected");
        // Reference: the same geometry as one polygon, whose BXS the writer meshes as every single region (initial mesh, unchanged by L6).
        foreach (var (name, outline) in new[] { ("cruciform 317x263x13 as one polygon", ShearAreaPartsTest.CrossOutline(317, 263, 13)), ("cover plated as one polygon", ShearAreaPartsTest.CoverPlatedOutline()) })
        {
            var (v, f) = SentBxs(outline); var p = writer.NativeSectionMeshProperties(v, f); var g = SectionShearAreas.Compute(outline, 0);
            var s = new[] { p[14], p[15] }.OrderBy(x => x).ToArray(); var e = new[] { g.Area1, g.Area2 }.OrderBy(x => x).ToArray();
            Console.WriteLine($"L6 native BXS {name}: {f.Length} faces; J {p[32]:E5} ({p[32] / outline.Jt - 1:P2} of GPC {outline.Jt:E5}), SA {s[1]:F1} / {s[0]:F1} ({s[1] / e[1] - 1:P2} / {s[0] / e[0] - 1:P2} of GPC {e[1]:F1} / {e[0]:F1})");
        }
        // Observation (reported, not a GPC rule): R31 merges coincident nodes of a BXS, so parts of a non-welded built-up section that touch
        // vertex to vertex (backs of two angles in contact) are one body for its stress recovery, while GPC treats them as connected at points.
        foreach (double gap in new[] { 0d, 10d })
        {
            var pair = SectionBuiltUp.DoubleAngle(new SectionL(100, 10, 100, 10, "L"), gap, "2L"); var (v, f) = SentBxs(pair); var p = writer.NativeSectionMeshProperties(v, f);
            var g = SectionShearAreas.Compute(pair, 0);
            Console.WriteLine($"L6 native BXS 2L 100x10 gap {gap} (not welded, parts apart): R31 J {p[32]:E5}, SA {p[14]:F1} / {p[15]:F1}; GPC J {pair.Jt:E5}, SA ν=0 {g.Area1:F1} / {g.Area2:F1} ({g.Bodies} bodies)");
        }
    }

    [TestMethod]
    public void L6_AGenericSectionFromAnOutlineNeedsNoPrecomputedProperties()
    {
        // Before: a generic Section built from an outline (its properties not computed yet, Area = 0) got SA1 = SA2 = 0 without any error,
        // because the factors were multiplied by the area of the section. The BridGHe adapter builds such sections for its girders.
        var bare = new Section(IOutline(), "I outline");
        Assert.AreEqual(0, bare.Area);
        var computed = new Section(IOutline(), "I outline"); computed.SetMechanicalProperties();
        var expected = SectionShearAreas.Compute(computed, .3);
        var areas = SectionShearAreas.Compute(bare, .3);
        Assert.AreEqual(expected.Area1, areas.Area1); Assert.AreEqual(expected.Area2, areas.Area2);
        Assert.IsTrue(areas.Area1 > 2 * 200 * 15 * .7 && areas.Area2 > 270 * 10, areas.ToString());
        Assert.AreEqual(0, bare.Area, "the caller's section is not changed");
        // The same outline as a SectionH: same shear areas (same geometry, same principal axes).
        var h = SectionShearAreas.Compute(new SectionH(300, 10, 200, 15, 200, 15, "I"), .3);
        Assert.AreEqual(h.Area1, areas.Area1, 1e-3 * h.Area1); Assert.AreEqual(h.Area2, areas.Area2, 1e-3 * h.Area2);
        // A section without an outline cannot be solved: an explicit error, never zero areas.
        var declared = new Section(1000, 1e6, 1e5, 1e4, 0, new Point2d(0, 0), new Point3d(0, 0, 0), 0, "declared");
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => SectionShearAreas.Compute(declared, .3)).Message, "ShearAreasRequireSectionGeometry");
    }
}
