using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Sections;
using SectionShearAreas = GPC.Model.Sections.SectionShearAreas;

namespace UnitTest;

/// <summary>Review L4 (4/11/2026) of the FE shear areas: sections of several parts (built-up, welded with T-junctions) and the principal axes
/// of the section. Expected values are independent of the multi-part code: the same geometry as one polygon, or the parts on their own.</summary>
[TestClass]
public class ShearAreaPartsTest
{
    private static Section Outline(string name, params (double X, double Y)[] points)
    {
        var section = new Section(new Shape2d(new Polygon2d(points.Select(p => new Point2d(p.X, p.Y)).ToArray())), name);
        section.SetMechanicalProperties(); return section;
    }

    /// <summary>The cruciform of SectionWelded.CruciformPlates(width, height, thickness) as one polygon.</summary>
    internal static Section CrossOutline(double width, double height, double thickness)
    {
        double w = width / 2, h = height / 2, t = thickness / 2;
        return Outline("cross", (-w, -t), (-t, -t), (-t, -h), (t, -h), (t, -t), (w, -t), (w, t), (t, t), (t, h), (-t, h), (-t, t), (-w, t));
    }

    /// <summary>SectionWelded.CoverPlated(I 400 (200x15, web 10), 300, 20) as one polygon: plates wider than the flanges.</summary>
    internal static Section CoverPlatedOutline() => Outline("cover plated", (0, 0), (300, 0), (300, 20), (250, 20), (250, 35), (155, 35), (155, 405), (250, 405),
        (250, 420), (300, 420), (300, 440), (0, 440), (0, 420), (50, 420), (50, 405), (145, 405), (145, 35), (50, 35), (50, 20), (0, 20));
    internal static SectionWelded CoverPlated() => SectionWelded.CoverPlated(new SectionH(400, 10, 200, 15, 200, 15, "I"), 300, 20);

    [TestMethod]
    public void L4_DoubleAnglesWithAGussetGapCarryTheShearInParallel()
    {
        // Before: ShearAreasRequireConnectedSection, so the whole Straus7 export failed. The angles bend together (gusset, battens), each about
        // axes parallel to those of the pair through its own centroid, without shear flow between them. An equal-leg angle has its principal
        // axes at ±45°: bending about X or Y is half bending about each, V = k (J11 + J22)/2 with V1 = k J22/√2 along axis 1 and V2 = k J11/√2,
        // energy V1²/SA1 + V2²/SA2. For the pair, on both axes: A = (J11 + J22)² / (J22²/SA1 + J11²/SA2) of one angle (R31: 1232 at ν = 0).
        var angle = new SectionL(100, 10, 100, 10, "L100x10");
        var (a1, a2) = SectionShearAreas.Compute(angle);
        double expected = Math.Pow(angle.J11 + angle.J22, 2) / (angle.J22 * angle.J22 / a1 + angle.J11 * angle.J11 / a2);
        foreach (double gap in new[] { 10d, 0d })
        {
            // Backs in contact (gap 0) are still connected at discrete points only, as for the torsion constant of SectionBuiltUp.
            var pair = SectionBuiltUp.DoubleAngle(angle, gap, "2L");
            var (p1, p2) = SectionShearAreas.Compute(pair);
            Assert.AreEqual(expected, p1, 2e-3 * expected, "gap " + gap + " SA1"); Assert.AreEqual(expected, p2, 2e-3 * expected, "gap " + gap + " SA2");
        }
    }

    [TestMethod]
    public void L4_WeldedCruciformIsOneBodyAsItsOutline()
    {
        // Before: the half plates touched the horizontal plate at no common vertex (T-junctions): far too small or no shear areas.
        foreach (var (width, height, thickness) in new[] { (300d, 300d, 20d), (317d, 263d, 13d), (411d, 177d, 9d) })
        {
            var (w1, w2) = SectionShearAreas.Compute(SectionWelded.CruciformPlates(width, height, thickness));
            var (o1, o2) = SectionShearAreas.Compute(CrossOutline(width, height, thickness));
            Console.WriteLine($"L4 cruciform {width}x{height}x{thickness}: welded {w1:F1} / {w2:F1}, outline {o1:F1} / {o2:F1}");
            Assert.AreEqual(o1, w1, 3e-3 * o1, width + " SA1"); Assert.AreEqual(o2, w2, 3e-3 * o2, width + " SA2");
        }
    }

    [TestMethod]
    public void L4_CoverPlatesWiderThanTheFlangesJoinAlongTheirWholeContact()
    {
        var (w1, w2) = SectionShearAreas.Compute(CoverPlated());
        var (o1, o2) = SectionShearAreas.Compute(CoverPlatedOutline());
        Assert.AreEqual(o1, w1, 3e-3 * o1, "SA1"); Assert.AreEqual(o2, w2, 3e-3 * o2, "SA2");
    }

    /// <summary>Jyy − Jxx relative to Jxx + Jyy.</summary>
    private static double Unbalance(ISectionShape s) => (s.Jyy - s.Jxx) / (s.Jxx + s.Jyy);

    /// <summary>A box 300 high, flanges 20, webs 8, whose width makes Jyy exceed Jxx by less than 1e-10: equal for GPC (axis 1 = X), not for
    /// the mesh.</summary>
    internal static SectionRHS EqualInertiaBox()
    {
        static SectionRHS Box(double width) => new SectionRHS(300, width, 20, 20, 8, 8, "box");
        double lo = 300, hi = 500;
        Assert.IsTrue(Unbalance(Box(lo)) < 0 && Unbalance(Box(hi)) > 0);
        for (int i = 0; i < 200 && Unbalance(Box(hi)) > 5e-11; i++) { double mid = (lo + hi) / 2; if (Unbalance(Box(mid)) > 0) hi = mid; else lo = mid; }
        var box = Box(hi); Assert.IsTrue(Unbalance(box) > 1e-13 && Unbalance(box) <= 5e-11, Unbalance(box).ToString("R"));
        return box;
    }

    [TestMethod]
    public void L4_ShearAreasFollowTheAxesOfTheSectionWhenTheInertiasAreEqual()
    {
        // Before: the axes were derived again from the mesh, where Jyy > Jxx by 1e-11: SA1 and SA2 were swapped against the axes the writer
        // gives to Straus7 (AngleX1 = 0, J11 = Jxx). The thick flanges carry the shear along X, the thin webs along Y.
        var box = EqualInertiaBox(); Assert.AreEqual(0, box.AngleX1); Assert.AreEqual(box.Jxx, box.J11);
        var (s1, s2) = SectionShearAreas.Compute(box);
        double flanges = 2 * 20 * box.Width, webs = 2 * 8 * (300 - 40);
        Assert.IsTrue(s1 > .7 * flanges && s1 < flanges, "SA1 " + s1.ToString("R") + " flanges " + flanges);
        Assert.IsTrue(s2 > .8 * webs && s2 < 1.3 * webs, "SA2 " + s2.ToString("R") + " webs " + webs);
    }

    [TestMethod]
    public void L4_RotatedNonSquareSectionKeepsItsPrincipalShearAreasWithPoisson()
    {
        // Review L4 on the C2 test (only ν = 0 on rectangles, where both axes give 5/6 A): with ν = 0.3 the two axes of a 2:1 rectangle differ
        // by 6 %, so a swap or a wrong rotation shows.
        // Review L6: analysis uses ν = 0 (decisione utente 4/11), where both axes of a rectangle give 5/6 A; the axes are checked on the internal
        // Poisson solution, which shares them with the analysis one.
        var plain = new SectionRectangular(200, 100); var (p1, p2) = SectionShearAreas.ComputeWithPoisson(plain, .3);
        Assert.IsTrue(p2 - p1 > .05 * p2, p1.ToString("R") + " / " + p2.ToString("R"));
        foreach (double angle in new[] { .3, -1.1 })
        {
            var rotated = new SectionRectangular(200, 100, angle); Assert.AreEqual(angle, rotated.AngleX1, 1e-12);
            var (r1, r2) = SectionShearAreas.ComputeWithPoisson(rotated, .3);
            Assert.AreEqual(p1, r1, 2e-4 * plain.Area, "angle " + angle + " SA1"); Assert.AreEqual(p2, r2, 2e-4 * plain.Area, "angle " + angle + " SA2");
        }
    }

    [TestMethod]
    public void L4_PartsAreCountedAsSeparateBodies()
    {
        int pair = SectionShearAreas.Compute(SectionBuiltUp.DoubleAngle(new SectionL(100, 10, 100, 10, "L"), 10, "2L")).Bodies;
        int cross = SectionShearAreas.Compute(SectionWelded.CruciformPlates(300, 300, 20)).Bodies;
        int plated = SectionShearAreas.Compute(CoverPlated()).Bodies;
        int single = SectionShearAreas.Compute(new SectionH(400, 10, 200, 15, 200, 15, "I")).Bodies;
        Assert.AreEqual(2, pair); Assert.AreEqual(1, cross); Assert.AreEqual(1, plated); Assert.AreEqual(1, single);
    }

    /// <summary>Beams in a row, each with its own steel section, clamped at the first node and loaded at the last one.</summary>
    internal static GPC.Model.Models.Model SteelRow(IReadOnlyList<ISectionShape> sections)
    {
        var nodes = Enumerable.Range(0, sections.Count + 1).Select(i => new GPC.Model.Elements.NodeElement(new Point3d(1000 * i, 0, 0))).ToArray();
        nodes[0].Assignments.Restrains.Add(new GPC.Model.Structure.Assignments.RestrainAssignment { Restrain = new GPC.Model.Restraints.NodeRestrain(nodes[0],
            Enumerable.Range(0, 6).Select(i => new GPC.Model.Restraints.DofRestrain((GPC.Model.Restraints.GeometryRestrain.DOF)i, true)).ToList()) });
        var beams = sections.Select((s, i) =>
        {
            var beam = new GPC.Model.Elements.BeamElement(nodes[i].Position, nodes[i + 1].Position,
                new GPC.Model.Sections.Steel.SteelSection((Section)s, GPC.Model.Data.Steel.SteelMaterialEN1993Data.S355));
            beam.ConnectNodes(nodes[i], nodes[i + 1]); beam.Assignments.Formulation = GPC.Model.Structure.Assignments.BeamFormulation.StraightTwoNode;
            beam.Assignments.SectionAxes = new CoordinateSystem(nodes[i].Position, new Vector3d(0, 1, 0), new Vector3d(0, 0, 1));
            return (GPC.Model.Elements.Element)beam;
        }).ToArray();
        nodes[^1].Loads.Add(new GPC.Model.Loads.PointLoad(0, 0, -1000, 0, 0, 0, nodes[^1].Position, new GPC.Model.LoadCases.LoadCaseBase("P")));
        return GPC.Model.Models.ModelAssembly.Assemble(beams, Guid.NewGuid());
    }

    [TestMethod]
    public void L4_SectionsOfSeparatePartsAreExportedWithAWarning()
    {
        // Before: the whole Straus7 export failed (ShearAreasRequireConnectedSection) for a double angle with a gusset gap.
        var pair = SectionBuiltUp.DoubleAngle(new SectionL(100, 10, 100, 10, "L100x10"), 10, "2L100x10");
        var model = SteelRow(new ISectionShape[] { pair, SectionWelded.CruciformPlates(317, 263, 13, "cross") });
        var plan = Straus7ModelWritePlan.Prepare(model); var (api, log) = Straus7ExporterCorrectionsTest.Recorder.Create(); plan.Write(api, model);
        var sent = log.Named(nameof(IStraus7ModelWriteApi.CreateBeamProperty)).ToDictionary(c => (string)c.Args[1]!, c => (double[])c.Args[3]!);
        var expected = SectionShearAreas.Compute(pair);
        Assert.AreEqual(expected.Area1, sent["2L100x10"][6]); Assert.AreEqual(expected.Area2, sent["2L100x10"][7]);
        var warning = plan.Diagnostics.Single();
        Assert.AreEqual("SeparateSectionPartsShearAreas", warning.Code); Assert.AreEqual(GPC.Model.Core.Diagnostics.DiagnosticSeverity.Warning, warning.Severity);
        StringAssert.Contains(warning.Message, "2L100x10: 2 separate parts");
    }

    [TestMethod]
    public void L4_DistinctSectionsOfOneExportAreSolvedInParallelWithTheSerialValues()
    {
        // 50 distinct welded I-girder segments (flange thickness and depth change along a bridge), as in a BridGHe model.
        var girders = Enumerable.Range(0, 50).Select(i => (ISectionShape)new SectionH(1800 + 8 * i, 16, 600, 30 + i % 10, 800, 40 + i / 5, "G" + i)).ToArray();
        Straus7ModelWritePlan.Prepare(SteelRow(new ISectionShape[] { new SectionH(1000, 12, 300, 20, 300, 20, "warm-up") })); // JIT, first meshes
        var serial = System.Diagnostics.Stopwatch.StartNew();
        var expected = girders.ToDictionary(g => g.Name, g => SectionShearAreas.Compute(g)); serial.Stop();
        var model = SteelRow(girders); var watch = System.Diagnostics.Stopwatch.StartNew();
        var plan = Straus7ModelWritePlan.Prepare(model); watch.Stop();
        var elastic = SteelRow(girders); foreach (var beam in elastic.BeamElements.Values)
            beam.BeamProperty = new GPC.Model.ElementProperties.ElasticBeamProperty(beam.BeamProperty.Name, new GPC.Model.Materials.Material("E", 210000, .3, 0, 0), ((GPC.Model.Sections.Steel.SteelSection)beam.BeamProperty).SectionShape, 1, 1);
        var baseline = System.Diagnostics.Stopwatch.StartNew(); Straus7ModelWritePlan.Prepare(elastic); baseline.Stop();
        var (api, log) = Straus7ExporterCorrectionsTest.Recorder.Create(); plan.Write(api, model);
        var sent = log.Named(nameof(IStraus7ModelWriteApi.CreateBeamProperty)).ToDictionary(c => (string)c.Args[1]!, c => (double[])c.Args[3]!);
        Assert.AreEqual(50, sent.Count);
        foreach (var pair in expected) { Assert.AreEqual(pair.Value.Area1, sent[pair.Key][6], pair.Key); Assert.AreEqual(pair.Value.Area2, sent[pair.Key][7], pair.Key); }
        Console.WriteLine($"L4 timing: 50 distinct I-girders, shear areas one by one {serial.Elapsed.TotalSeconds:F2} s; Prepare with them (parallel) {watch.Elapsed.TotalSeconds:F2} s; " +
            $"Prepare with explicit areas {baseline.Elapsed.TotalSeconds:F2} s; {Environment.ProcessorCount} processors, server GC {System.Runtime.GCSettings.IsServerGC}");
    }

    /// <summary>Vertices and faces of the outlines meshed one by one, without merging: the BXS the writer sends for a built-up section.</summary>
    private static (double[][] Vertices, int[][] Faces) SeparateMeshes(SectionBuiltUp section)
    {
        var vertices = new List<double[]>(); var faces = new List<int[]>();
        foreach (var outline in section.GetOutlines())
        {
            var mesh = new Section(outline).GetMesh(initialMeshOnly: true); var ids = new Dictionary<int, int>();
            foreach (var v in mesh.Vertices) { ids.Add(v.Id, vertices.Count); vertices.Add(new[] { v.Point.X, v.Point.Y, 0 }); }
            foreach (var face in mesh.Faces) faces.Add(mesh.GetFaceVertices(face).Select(v => ids[v.Id]).ToArray());
        }
        return (vertices.ToArray(), faces.ToArray());
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void L4_NativeBxsShearAreasOfSectionsOfSeveralPartsAreReported()
    {
        // What R31 computes for the same regions (St7GenerateBXS, ipBXSSA1/SA2, without the Poisson term): the BXS of the writer (parts meshed
        // apart) and the conforming mesh of the GPC shear areas. Reported for the notes; R31 and GPC at ν = 0 are compared where both join the parts.
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        var cases = new (string Name, ISectionShape Section)[] {
            ("2L 100x10, gap 10", SectionBuiltUp.DoubleAngle(new SectionL(100, 10, 100, 10, "L"), 10, "2L")),
            ("cruciform 317x263x13", SectionWelded.CruciformPlates(317, 263, 13)), ("cover plated 300x20", CoverPlated()),
            ("box with equal inertias", EqualInertiaBox()) };
        using var writer = new Straus7NativeWriter(Path.Combine(folder, "bxs.st7"), folder);
        // One angle on its own, in its principal axes (±45°) and as R31 computes it: the parts of the first case.
        var angle = new SectionL(100, 10, 100, 10, "L"); var (lv, lt) = SectionShearAreas.AnalysisMesh(angle); var single = writer.NativeSectionMeshProperties(lv, lt);
        var a0 = SectionShearAreas.Compute(angle);
        Console.WriteLine($"L4 native L 100x10 alone: GPC ν=0 {a0.Area1:F1} / {a0.Area2:F1} (sum {a0.Area1 + a0.Area2:F1}); R31 {single[14]:F1} / {single[15]:F1} " +
            $"(sum {single[14] + single[15]:F1}, angle {single[5]:F4}, I11 {single[3]:E4}, I22 {single[4]:E4})");
        foreach (var c in cases)
        {
            var gpc0 = SectionShearAreas.Compute(c.Section); int bodies = gpc0.Bodies;
            var (jv, jt) = SectionShearAreas.AnalysisMesh(c.Section); var joined = writer.NativeSectionMeshProperties(jv, jt);
            string separate = "-";
            if (c.Section is SectionBuiltUp built) { var (sv, sf) = SeparateMeshes(built); var p = writer.NativeSectionMeshProperties(sv, sf); separate = $"{p[14]:F1} / {p[15]:F1}"; }
            Console.WriteLine($"L4 native {c.Name}: bodies {bodies}; GPC ν=0 {gpc0.Area1:F1} / {gpc0.Area2:F1}; " +
                $"R31 joined mesh {joined[14]:F1} / {joined[15]:F1} (A {joined[2]:F1}, I11 {joined[3]:E4}, I22 {joined[4]:E4}, angle {joined[5]:F4}); R31 parts apart {separate}");
            Assert.AreEqual(c.Section.Area, joined[2], 1e-6 * c.Section.Area, c.Name + " area of the analysis mesh");
            // R31 computes without the Poisson term: compare at ν = 0 (its axes may be swapped when I11 = I22: compare the sorted pair).
            var r31 = new[] { joined[14], joined[15] }.OrderBy(v => v).ToArray(); var gpc = new[] { gpc0.Area1, gpc0.Area2 }.OrderBy(v => v).ToArray();
            Assert.AreEqual(r31[0], gpc[0], .01 * r31[0], c.Name + " smaller"); Assert.AreEqual(r31[1], gpc[1], .01 * r31[1], c.Name + " larger");
        }
    }
}
