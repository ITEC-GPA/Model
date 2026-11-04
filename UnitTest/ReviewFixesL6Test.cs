using System.Reflection;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Actions.Moving;
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
        var single = SectionShearAreas.Compute(h);
        Assert.AreEqual(1, single.Bodies); Assert.IsFalse(single.HasSeparateParts); Assert.AreEqual(SectionShearAreas.SaintVenantFlexure, single.Method);
        Assert.AreEqual("SaintVenantFlexure(nu=0)", single.Method);
        Assert.AreEqual(0, single.PoissonRatio); Assert.IsTrue(single.Nodes > single.Triangles / 3, single.ToString());
        var (a1, a2) = single; Assert.AreEqual(single.Area1, a1); Assert.AreEqual(single.Area2, a2);
        var pair = SectionShearAreas.Compute(SectionBuiltUp.DoubleAngle(new SectionL(100, 10, 100, 10, "L"), 10, "2L"));
        Assert.AreEqual(2, pair.Bodies); Assert.IsTrue(pair.HasSeparateParts); Assert.AreEqual(SectionShearAreas.SeparatePartsBendingTogether, pair.Method);
        // The forwarder kept for the F6 branches gives the same values, bit for bit, at ν = 0 whatever ν it gets (P2B passes the material's).
#pragma warning disable CS0618
        var forwarded = GPC.Converter.SectionShearAreas.Compute(h, .3, out int bodies);
#pragma warning restore CS0618
        Assert.AreEqual(single.Area1, forwarded.Area1); Assert.AreEqual(single.Area2, forwarded.Area2); Assert.AreEqual(1, bodies);
    }

    [TestMethod]
    public void L6_ShearAreasForAnalysisAreDeSaintVenantAtNuZero()
    {
        // Decisione utente 4/11: De Saint Venant a ν = 0 («seguirei la scienza delle costruzioni»). Before: the writer used the ν of the steel
        // (0.3), so a 2:1 rectangle had SA1 = 15 689 mm² instead of Jourawski's 5/6 A = 16 667 along its wide side.
        var rectangle = new SectionRectangular(200, 100);
        var areas = SectionShearAreas.Compute(rectangle);
        Assert.AreEqual(5d / 6 * rectangle.Area, areas.Area1, 2e-4 * rectangle.Area, "Jourawski 5/6 A"); Assert.AreEqual(5d / 6 * rectangle.Area, areas.Area2, 2e-4 * rectangle.Area);
        Assert.AreEqual(SectionShearAreas.ComputeWithPoisson(rectangle, 0).Area1, areas.Area1, "the public API is the solution without the Poisson term");
        var pipe = new SectionCHS(500, 5); Assert.AreEqual(pipe.Area / 2, SectionShearAreas.Compute(pipe).Area1, .005 * pipe.Area, "thin tube A/2");
        // The Straus7 writer sends them for a steel section (ν = 0.3 in its material), not the ones with the Poisson term.
        var model = Straus7ModelWriteTest.Cantilever();
        model.BeamElements.Values.Single().BeamProperty = new GPC.Model.Sections.Steel.SteelSection(rectangle, GPC.Model.Data.Steel.SteelMaterialEN1993Data.S355);
        Assert.AreEqual(.3, ((GPC.Model.Sections.Steel.SteelSection)model.BeamElements.Values.Single().BeamProperty).SteelMaterial.Ni);
        var sent = (double[])Straus7ExporterCorrectionsTest.Record(model).Named(nameof(IStraus7ModelWriteApi.CreateBeamProperty)).Single().Args[3]!;
        Assert.AreEqual(areas.Area1, sent[6]); Assert.AreEqual(areas.Area2, sent[7]);
        Assert.IsTrue(sent[6] - SectionShearAreas.ComputeWithPoisson(rectangle, .3).Area1 > .03 * rectangle.Area, "not the ν = 0.3 value: " + sent[6].ToString("R"));
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
            var areas = SectionShearAreas.Compute(section);
            Console.WriteLine($"L6 mesh {section.Name} {section.GetType().Name}: {areas}");
            Assert.AreEqual(SectionShearAreas.MeshSize(section.Area), areas.MeshSize, 1e-12 * areas.MeshSize, section.Name);
            Assert.AreEqual(6.4 * Math.Sqrt(section.Area / n), areas.MeshSize, 1e-12 * areas.MeshSize, section.Name);
            Assert.IsTrue(areas.Triangles > .85 * n && areas.Triangles < 1.25 * n, section.Name + ": " + areas.Triangles + " triangles");
        }
        // A wall thinner than the size gets smaller triangles, not a coarser solution.
        var pipe = SectionShearAreas.Compute(new SectionCHS(500, 5));
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
            var gpc = SectionShearAreas.Compute(welded);
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
        var cross = WeldedSections()[0].Section; var crossGpc = SectionShearAreas.Compute(cross);
        var old = writer.NativeSectionMeshProperties(PartsApart(cross.GetOutlines()).Vertices, PartsApart(cross.GetOutlines()).Faces);
        Assert.IsTrue(Math.Min(old[14], old[15]) < .9 * Math.Min(crossGpc.Area1, crossGpc.Area2), "the old BXS was disconnected");
        // Reference: the same geometry as one polygon, whose BXS the writer meshes as every single region (initial mesh, unchanged by L6).
        foreach (var (name, outline) in new[] { ("cruciform 317x263x13 as one polygon", ShearAreaPartsTest.CrossOutline(317, 263, 13)), ("cover plated as one polygon", ShearAreaPartsTest.CoverPlatedOutline()) })
        {
            var (v, f) = SentBxs(outline); var p = writer.NativeSectionMeshProperties(v, f); var g = SectionShearAreas.Compute(outline);
            var s = new[] { p[14], p[15] }.OrderBy(x => x).ToArray(); var e = new[] { g.Area1, g.Area2 }.OrderBy(x => x).ToArray();
            Console.WriteLine($"L6 native BXS {name}: {f.Length} faces; J {p[32]:E5} ({p[32] / outline.Jt - 1:P2} of GPC {outline.Jt:E5}), SA {s[1]:F1} / {s[0]:F1} ({s[1] / e[1] - 1:P2} / {s[0] / e[0] - 1:P2} of GPC {e[1]:F1} / {e[0]:F1})");
        }
        // Observation (reported, not a GPC rule): R31 merges coincident nodes of a BXS, so parts of a non-welded built-up section that touch
        // vertex to vertex (backs of two angles in contact) are one body for its stress recovery, while GPC treats them as connected at points.
        foreach (double gap in new[] { 0d, 10d })
        {
            var pair = SectionBuiltUp.DoubleAngle(new SectionL(100, 10, 100, 10, "L"), gap, "2L"); var (v, f) = SentBxs(pair); var p = writer.NativeSectionMeshProperties(v, f);
            var g = SectionShearAreas.Compute(pair);
            Console.WriteLine($"L6 native BXS 2L 100x10 gap {gap} (not welded, parts apart): R31 J {p[32]:E5}, SA {p[14]:F1} / {p[15]:F1}; GPC J {pair.Jt:E5}, SA ν=0 {g.Area1:F1} / {g.Area2:F1} ({g.Bodies} bodies)");
        }
    }

    /// <summary>A vehicle of one point force and/or one distributed force (1000 N in total each, 20 m/s), in the given axes.</summary>
    private static MovingLoadTemplate Vehicle(MovingLoadAxes axes, Vector3d force, bool point, bool distributed, bool? centrifugal, MovingCentrifugalLaw? law)
    {
        var points = point ? new[] { new MovingPointForce { Force = force, Axes = axes, ApplyCentrifugal = centrifugal } } : new MovingPointForce[0];
        // 100 mm long, 10 N/mm along path x: 1000 N.
        var lines = distributed ? new[] { new MovingDistributedForce { Mobility = MovingDistributionMobility.Grouped, X1 = -50, X2 = 50, Y1 = -50, Y2 = 50,
            ForcePerLength = new Vector3d(force.X / 100, force.Y / 100, force.Z / 100), Axes = axes, ApplyCentrifugal = centrifugal } } : new MovingDistributedForce[0];
        return new MovingLoadTemplate { Name = "axle", Centrifugal = law, Vehicles = new[] { new MovingVehicle { Name = "P1000", Speed = 20000, PointForces = points, DistributedForces = lines } } };
    }

    /// <summary>The cantilever of Straus7ModelWriteTest (beam from (0,0,0) to (1000,0,0), clamped at X = 0) with a moving-load path along it,
    /// converted to static cases.</summary>
    private static (GPC.Model.Models.Model Model, MovingLoadPath Path) PathOnTheCantilever(MovingLoadTemplate template, MovingPathShape shape, Point3d reference)
    {
        var model = Straus7ModelWriteTest.Cantilever(); foreach (var node in model.NodesElements.Values) node.Loads.Clear();
        var path = new MovingLoadPath { Id = Guid.NewGuid(), CaseGuid = model.LoadCases.Values.Single().Guid, Divisions = 4, Shape = shape,
            Start = new Point3d(0, 0, 0), End = new Point3d(1000, 0, 0), Reference = reference, Target = MovingPathTarget.Beams, Template = template };
        model.MovingLoads = new MovingLoadDefinition(); model.MovingLoads.Paths.Add(path); model.MovingLoads.Validate(model);
        return (model, path);
    }

    /// <summary>Every writer API of the native writer, for a forwarding proxy.</summary>
    public interface IForwardedWriter : IStraus7ModelWriteApi, IStraus7WriteApi, IStraus7DistributionWriteApi, IStraus7MaterialWriteApi,
        IStraus7SectionGeometryWriteApi, IStraus7SectionMeshWriteApi, IStraus7NativeSectionWriteApi, IStraus7InertiaWriteApi,
        IStraus7ConstraintWriteApi, IStraus7SurfacePressureWriteApi, IStraus7MovingWriteApi, IStraus7AdvancedMovingWriteApi, IStraus7StageWriteApi { }

    /// <summary>Forwards every call to the native writer, setting the centrifugal flag (flags[3]) of the moving forces in global axes as the
    /// writer did before L6: what R31 receives from such a model, now rejected at Prepare.</summary>
    public class GlobalCentrifugalFlag : DispatchProxy
    {
        internal Straus7NativeWriter Target = null!; internal int Flagged;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if ((method!.Name == nameof(IStraus7MovingWriteApi.AddMovingPointForce) || method.Name == nameof(IStraus7MovingWriteApi.AddMovingDistributedForce))
                && args![3] is int[] flags && flags[1] == (int)MovingLoadAxes.Global) { flags[3] = 1; Flagged++; }
            try { return method.Invoke(Target, args); }
            catch (TargetInvocationException error) { System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error.InnerException!).Throw(); throw; }
        }
        internal static (IForwardedWriter Api, GlobalCentrifugalFlag Proxy) Create(Straus7NativeWriter target)
        {
            var api = DispatchProxy.Create<IForwardedWriter, GlobalCentrifugalFlag>(); var proxy = (GlobalCentrifugalFlag)(object)api; proxy.Target = target; return (api, proxy);
        }
    }

    /// <summary>The reaction of the clamp in each generated static case that carries a load (written natively, optionally through
    /// <see cref="GlobalCentrifugalFlag"/>).</summary>
    private static List<double[]> ClampReactions(GPC.Model.Models.Model model, MovingLoadPath path, string name, bool flagGlobalCentrifugal = false)
    {
        var plan = Straus7ModelWritePlan.Prepare(model, new Dictionary<Guid, Straus7MovingPathOptions> { { path.Id, new Straus7MovingPathOptions { ConvertToStaticCases = true } } });
        string folder = Straus7ExporterCorrectionsTest.NativeFolder(), output = Path.Combine(folder, name + ".st7"), result; IReadOnlyDictionary<int, string> generated;
        using (var writer = new Straus7NativeWriter(output, folder))
        {
            if (flagGlobalCentrifugal) { var (api, proxy) = GlobalCentrifugalFlag.Create(writer); generated = plan.Write(api, model); Assert.IsTrue(proxy.Flagged > 0, name + ": no force flagged"); }
            else generated = plan.Write(writer, model);
            result = writer.Solve(); // Solve saves the model first

        }
        int clamp = model.NodesElements.Values.Single(n => n.Position.X == 0).Id; var reactions = new List<double[]>();
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(output, folder); reader.OpenResults(result);
        foreach (int c in generated.Keys)
        {
            var r = reader.ReadNodeResult(clamp, c, true).Values;
            Console.WriteLine($"L6 {name} case {c}: reaction X {r[0]:G6} Y {r[1]:G6} Z {r[2]:G6} N");
            if (Math.Abs(r[0]) + Math.Abs(r[1]) + Math.Abs(r[2]) > 1e-6) reactions.Add(r.Take(3).ToArray());
        }
        return reactions;
    }

    [TestMethod]
    public void L6_CentrifugalForcesInGlobalAxesAreRejectedAtPrepare()
    {
        // Before: written as they were; R31 applies no centrifugal force to them (native test below), so the centrifugal force of the GPC model
        // was lost without a trace. Now rejected at Prepare, for the moving-load plan and for the full export.
        var law = new MovingCentrifugalLaw { FactorKind = MovingCentrifugalFactorKind.Constant, Factor = .1, OffsetForce = 50 };
        var reference = new Point3d(500, -1.25, 0);
        foreach (var (point, distributed) in new[] { (true, false), (false, true) })
        {
            var (model, path) = PathOnTheCantilever(Vehicle(MovingLoadAxes.Global, new Vector3d(0, 0, -1000), point, distributed, true, law), MovingPathShape.Circular, reference);
            var full = Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(model));
            StringAssert.Contains(full.Message, "StrausCentrifugalForceRequiresPathAxes"); StringAssert.Contains(full.Message, point ? "point force" : "distributed force");
            var moving = Assert.ThrowsException<NotSupportedException>(() => Straus7MovingWritePlan.Prepare(model, new Dictionary<Guid, int> { { path.CaseGuid, 1 } }));
            StringAssert.Contains(moving.Message, "StrausCentrifugalForceRequiresPathAxes");
            // In path axes, or in global axes without centrifugal force, the export goes on.
            Straus7ModelWritePlan.Prepare(PathOnTheCantilever(Vehicle(MovingLoadAxes.Path, new Vector3d(0, 0, -1000), point, distributed, true, law), MovingPathShape.Circular, reference).Model);
            Straus7ModelWritePlan.Prepare(PathOnTheCantilever(Vehicle(MovingLoadAxes.Global, new Vector3d(0, 0, -1000), point, distributed, null, law), MovingPathShape.Circular, reference).Model);
        }
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void L6_NativeR31AppliesNoCentrifugalForceInGlobalAxes()
    {
        // The rejection above rests on this: the forces written with the centrifugal flag set, as before L6 (through a forwarding proxy),
        // along the circular path of the L4 test (radius 100000.625 mm). In path axes R31 applies 50 + 0.1 × 1000 = 150 N; in global axes nothing.
        var law = new MovingCentrifugalLaw { FactorKind = MovingCentrifugalFactorKind.Constant, Factor = .1, OffsetForce = 50 };
        var onTheArc = new Point3d(500, -1.25, 0);
        static double Horizontal(double[] r) => Math.Sqrt(r[0] * r[0] + r[1] * r[1]);
        foreach (var (point, distributed, kind) in new[] { (true, false, "point"), (false, true, "distributed") })
        {
            var (inPath, pathP) = PathOnTheCantilever(Vehicle(MovingLoadAxes.Path, new Vector3d(0, 0, -1000), point, distributed, true, law), MovingPathShape.Circular, onTheArc);
            var path = ClampReactions(inPath, pathP, kind + "-path-axes");
            var (inGlobal, pathG) = PathOnTheCantilever(Vehicle(MovingLoadAxes.Global, new Vector3d(0, 0, -1000), point, distributed, null, law), MovingPathShape.Circular, onTheArc);
            var global = ClampReactions(inGlobal, pathG, kind + "-global-axes-flagged", flagGlobalCentrifugal: true);
            Console.WriteLine($"L6 centrifugal {kind} force: path axes {string.Join(", ", path.Select(r => Horizontal(r).ToString("G6")))} N; " +
                $"global axes with the flag {string.Join(", ", global.Select(r => Horizontal(r).ToString("G6")))} N");
            Assert.IsTrue(path.Count >= 3 && global.Count >= 3, kind + ": loaded cases");
            foreach (var r in path) Assert.IsTrue(Horizontal(r) > 100, kind + " in path axes gets its centrifugal force: " + Horizontal(r));
            if (point) foreach (var r in path) Assert.AreEqual(150, Horizontal(r), 1e-3, "point force in path axes: 50 N + 0.1 × 1000 N");
            foreach (var r in global) { Assert.AreEqual(1000, r[2], 1e-6, kind + " vertical load applied"); Assert.AreEqual(0, Horizontal(r), 1e-6, kind + " in global axes: no centrifugal force"); }
        }
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void L6_NativePathAxesFollowTheReferencePoint()
    {
        // The sign convention of the path axes, pinned for the BridGHe adapter (review L4: with the reference point at −Y, the force (0, 0, −1000)
        // went upwards). Straight path from (0,0,0) to (1000,0,0) along the cantilever; a force (300, 200, −1000) in path axes. The applied force is
        // minus the reaction of the clamp. Expected: x = +X (Start to End); y towards the reference point; z = x × y.
        var force = new Vector3d(300, 200, -1000);
        foreach (var (side, reference, expected) in new[] {
            ("reference at +Y (left of the travel)", new Point3d(500, 1000, 0), new[] { 300d, 200, -1000 }),
            ("reference at -Y (right of the travel)", new Point3d(500, -1000, 0), new[] { 300d, -200, 1000 }) })
        {
            var (model, path) = PathOnTheCantilever(Vehicle(MovingLoadAxes.Path, force, true, false, null, null), MovingPathShape.Straight, reference);
            var reactions = ClampReactions(model, path, "path-axes-" + (reference.Y > 0 ? "plus" : "minus") + "-y");
            Assert.IsTrue(reactions.Count >= 3, side + ": loaded cases");
            foreach (var r in reactions)
            {
                Console.WriteLine($"L6 path axes, {side}: applied force ({-r[0]:G6}, {-r[1]:G6}, {-r[2]:G6}) N for (300, 200, -1000) in path axes");
                for (int k = 0; k < 3; k++) Assert.AreEqual(expected[k], -r[k], 1e-6, side + " component " + k);
            }
        }
        // Global axes do not depend on the reference point.
        foreach (var reference in new[] { new Point3d(500, 1000, 0), new Point3d(500, -1000, 0) })
        {
            var (model, path) = PathOnTheCantilever(Vehicle(MovingLoadAxes.Global, force, true, false, null, null), MovingPathShape.Straight, reference);
            foreach (var r in ClampReactions(model, path, "global-axes-" + (reference.Y > 0 ? "plus" : "minus") + "-y"))
                for (int k = 0; k < 3; k++) Assert.AreEqual(new[] { 300d, 200, -1000 }[k], -r[k], 1e-6, "global axes, reference " + reference.Y + ", component " + k);
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
        var expected = SectionShearAreas.Compute(computed);
        var areas = SectionShearAreas.Compute(bare);
        Assert.AreEqual(expected.Area1, areas.Area1); Assert.AreEqual(expected.Area2, areas.Area2);
        Assert.IsTrue(areas.Area1 > 2 * 200 * 15 * .7 && areas.Area2 > 270 * 10, areas.ToString());
        Assert.AreEqual(0, bare.Area, "the caller's section is not changed");
        // The same outline as a SectionH: same shear areas (same geometry, same principal axes).
        var h = SectionShearAreas.Compute(new SectionH(300, 10, 200, 15, 200, 15, "I"));
        Assert.AreEqual(h.Area1, areas.Area1, 1e-3 * h.Area1); Assert.AreEqual(h.Area2, areas.Area2, 1e-3 * h.Area2);
        // A section without an outline cannot be solved: an explicit error, never zero areas.
        var declared = new Section(1000, 1e6, 1e5, 1e4, 0, new Point2d(0, 0), new Point3d(0, 0, 0), 0, "declared");
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => SectionShearAreas.Compute(declared)).Message, "ShearAreasRequireSectionGeometry");
    }
}
