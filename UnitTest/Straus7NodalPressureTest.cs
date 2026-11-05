using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Restraints;
using GPC.Model.Structure.Assignments;

namespace UnitTest;

/// <summary>
/// 5/11/2026 (request of the user, «forze nodali»): the Straus7 API has only plate pressures constant over the element, so the writer turns a
/// <see cref="NonUniformPlatePressure"/> into its work-equivalent nodal forces (<see cref="NonUniformPlatePressure.GetGlobalNodalForces"/>),
/// added to the point loads of the same node and case. Output only: reading the .st7 back gives node forces with the same resultant.
/// </summary>
[TestClass]
public class Straus7NodalPressureTest
{
    private static readonly double Cos30 = Math.Sqrt(3) / 2;

    private static void AssertVector(Vector3d expected, Vector3d actual, double tolerance, string message = "")
    {
        Assert.AreEqual(expected.X, actual.X, tolerance, message + " X"); Assert.AreEqual(expected.Y, actual.Y, tolerance, message + " Y");
        Assert.AreEqual(expected.Z, actual.Z, tolerance, message + " Z");
    }

    /// <summary>Sum and moment about the pole of the nodal forces of the load, against its resultant.</summary>
    private static void AssertEquivalent(NonUniformPlatePressure load, Point3d pole, double relative)
    {
        var forces = load.GetGlobalNodalForces(); var vertices = load.Vertices;
        var sum = new Vector3d(0, 0, 0); var moment = new Vector3d(0, 0, 0);
        for (int i = 0; i < forces.Length; i++) { sum += forces[i]; moment += new Vector3d(pole, vertices[i]).CrossProduct(forces[i]); }
        var (force, resultantMoment) = load.GetGlobalResultant(pole);
        AssertVector(force, sum, relative * force.Length, "force"); AssertVector(resultantMoment, moment, relative * Math.Max(1, resultantMoment.Length), "moment");
    }

    [TestMethod]
    public void NodalForcesAreTheConsistentLoadsOfTheShapeFunctions()
    {
        // Rectangle 4000 x 2000, bilinear p (1, 5, 5, 1) along -Z: F_i = A/36 (4 p_i + 2 p_adjacent + 2 p_adjacent + p_opposite).
        var rectangle = new[] { new Point3d(0, 0, 0), new Point3d(4000, 0, 0), new Point3d(4000, 2000, 0), new Point3d(0, 2000, 0) };
        var down = new CoordinateSystem(Point3d.Origin, new Vector3d(1, 0, 0), new Vector3d(0, -1, 0), new Vector3d(0, 0, -1));
        var load = new NonUniformPlatePressure(rectangle, new[] { 1d, 5, 5, 1 }, new LoadCaseBase("H"), down);
        double area = 8e6; var expected = new[] { 21, 33, 33, 21 }.Select(k => -k * area / 36).ToArray();
        var forces = load.GetGlobalNodalForces();
        for (int i = 0; i < 4; i++) { Assert.AreEqual(expected[i], forces[i].Z, 1e-9 * area, "vertex " + i); Assert.AreEqual(0, forces[i].X); Assert.AreEqual(0, forces[i].Y); }
        AssertEquivalent(load, new Point3d(123, -456, 789), 1e-12);
        // Triangle, linear p (3, 0, 0): F_i = A/12 (p_i + sum p).
        var triangle = new NonUniformPlatePressure(new[] { new Point3d(0, 0, 0), new Point3d(3000, 0, 0), new Point3d(0, 3000, 0) }, new[] { 3d, 0, 0 },
            new LoadCaseBase("H"), CoordinateSystem.Global);
        var t = triangle.GetGlobalNodalForces(); double a = 4.5e6;
        Assert.AreEqual(a / 12 * 6, t[0].Z, 1e-6); Assert.AreEqual(a / 12 * 3, t[1].Z, 1e-6); Assert.AreEqual(a / 12 * 3, t[2].Z, 1e-6);
        AssertEquivalent(triangle, new Point3d(-10, 20, 30), 1e-12);
    }

    [TestMethod]
    public void NodalForcesFollowAnInclinedNormalAndAWarpedQuadWithinTheBound()
    {
        // A 1000 mm square rotated by 30° about X: the pressure acts along its own normal (0, -sin 30°, cos 30°).
        var corners = new[] { new Point3d(0, 0, 0), new Point3d(1000, 0, 0), new Point3d(1000, 1000 * Cos30, 500), new Point3d(0, 1000 * Cos30, 500) };
        var normal = new Vector3d(0, -0.5, Cos30);
        var axes = new CoordinateSystem(Point3d.Origin, new Vector3d(1, 0, 0), new Vector3d(0, Cos30, 0.5), normal);
        var inclined = new NonUniformPlatePressure(corners, new[] { 2d, 2, 4, 4 }, new LoadCaseBase("N"), axes);
        foreach (var f in inclined.GetGlobalNodalForces()) Assert.AreEqual(0, f.CrossProduct(normal).Length, 1e-9 * f.Length, "along the normal");
        AssertVector(normal * (1e6 * 3), inclined.GetGlobalNodalForces().Aggregate((x, y) => x + y), 1e-6, "mean 3 over 1e6 mm²");
        AssertEquivalent(inclined, new Point3d(500, 0, 0), 1e-12);
        // Round-off of a real model (5e-4 mm twist at 9 m from the origin): planar for ShellPlanarity, same resultant.
        double x0 = 9083, y0 = 4000, z0 = 1500, e = 5e-4;
        var twisted = new[] { new Point3d(x0, y0, z0 + e), new Point3d(x0 + 350, y0, z0 - e), new Point3d(x0 + 350, y0 + 350, z0 + e), new Point3d(x0, y0 + 350, z0 - e) };
        AssertEquivalent(new NonUniformPlatePressure(twisted, new[] { 1d, 2, 3, 4 }, new LoadCaseBase("W"), CoordinateSystem.Global), new Point3d(0, 0, 0), 1e-11);
    }

    /// <summary>Two 1000 mm plates side by side in the XY plane (normal +Z), sharing the nodes at x = 1000; the nodes at x = 0 are fixed.</summary>
    private static (Model Model, NodeElement[] Nodes, AreaElement[] Plates) Strip(Action<AreaElement[], NodeElement[]> load)
    {
        var nodes = new[] { new NodeElement(new Point3d(0, 0, 0)), new NodeElement(new Point3d(1000, 0, 0)), new NodeElement(new Point3d(2000, 0, 0)),
            new NodeElement(new Point3d(0, 1000, 0)), new NodeElement(new Point3d(1000, 1000, 0)), new NodeElement(new Point3d(2000, 1000, 0)) };
        foreach (var node in nodes.Where(n => n.Position.X == 0))
            node.Assignments.Restrains.Add(new RestrainAssignment { Restrain = new NodeRestrain(node, Enumerable.Range(0, 6).Select(i => new DofRestrain((GeometryRestrain.DOF)i, true)).ToList()) });
        var property = new ElasticPlateProperty("plate", new Material("E", 210000, .3, 0, 0), 20, 20);
        var plates = new[] { new AreaElement(null, property), new AreaElement(null, property) };
        plates[0].ConnectNodes(nodes[0], nodes[1], nodes[4], nodes[3]); plates[1].ConnectNodes(nodes[1], nodes[2], nodes[5], nodes[4]);
        load(plates, nodes);
        var model = ModelAssembly.Assemble(plates.Cast<Element>().ToArray(), Guid.NewGuid());
        return (model, model.NodesElements.Values.OrderBy(n => n.Position.Y).ThenBy(n => n.Position.X).ToArray(), model.AreaElements.Values.OrderBy(p => p.Points.Min(q => q.X)).ToArray());
    }

    private static NonUniformPlatePressure Varying(AreaElement plate, LoadCaseBase lc, Func<Point3d, double> p) =>
        new NonUniformPlatePressure(plate.Nodes.Select(n => n.Position).ToArray(), plate.Nodes.Select(n => p(n.Position)).ToArray(), lc, CoordinateSystem.Global);

    [TestMethod]
    public void WriterAddsTheNodalForcesToThePointLoadsInGlobalAxes()
    {
        var lc = new LoadCaseBase("P");
        // A point load in rotated axes on the shared node (1000, 0): local X is global Y.
        var rotated = new CoordinateSystem(Point3d.Origin, new Vector3d(0, 1, 0), new Vector3d(-1, 0, 0), new Vector3d(0, 0, 1));
        var (model, nodes, plates) = Strip((p, n) =>
        {
            foreach (var plate in p) plate.Loads.Add(Varying(plate, lc, q => -0.001 * (1 + q.X / 1000)));
            n[1].Loads.Add(new PointLoad(new Vector3d(500, 0, -100), new Vector3d(0, 0, 0), n[1].Position, lc, rotated));
        });
        var log = Straus7ExporterCorrectionsTest.Record(model);
        Assert.AreEqual(0, log.Named(nameof(IStraus7SurfacePressureWriteApi.SetGlobalSurfacePressure)).Length + log.Named(nameof(IStraus7SurfacePressureWriteApi.SetNormalSurfacePressure)).Length,
            "no constant plate pressure is written for the varying one");
        var forces = log.Named(nameof(IStraus7ModelWriteApi.SetNodeLoad)).Where(c => !(bool)c.Args[2]!).ToDictionary(c => (int)c.Args[0]!, c => (double[])c.Args[3]!);
        var expected = new Dictionary<int, Vector3d>();
        foreach (var plate in plates)
        {
            var load = plate.Loads.Values.OfType<NonUniformPlatePressure>().Single(); var f = load.GetGlobalNodalForces(); var v = load.Vertices;
            for (int i = 0; i < f.Length; i++) { int id = nodes.Single(n => n.Position.DistanceTo(v[i]) < 1e-9).Id; expected[id] = expected.TryGetValue(id, out var s) ? s + f[i] : f[i]; }
        }
        expected[nodes[1].Id] += new Vector3d(0, 500, -100);
        Assert.AreEqual(expected.Count, forces.Count);
        foreach (var pair in expected) AssertVector(pair.Value, new Vector3d(forces[pair.Key][0], forces[pair.Key][1], forces[pair.Key][2]), 1e-9, "node " + pair.Key);
        // Total: the two resultants and the point load.
        var total = forces.Values.Aggregate(new Vector3d(0, 0, 0), (s, f) => s + new Vector3d(f[0], f[1], f[2]));
        AssertVector(plates.Select(p => p.Loads.Values.OfType<NonUniformPlatePressure>().Single().GetGlobalLoadVector()).Aggregate((x, y) => x + y) + new Vector3d(0, 500, -100), total, 1e-6);
    }

    [TestMethod]
    public void WriterRefusesAPressureThatIsNotOnTheCornersOfItsPlate()
    {
        var lc = new LoadCaseBase("P");
        var (model, _, _) = Strip((p, n) => p[0].Loads.Add(new NonUniformPlatePressure(new[] { new Point3d(0, 0, 0), new Point3d(500, 0, 0), new Point3d(500, 1000, 0), new Point3d(0, 1000, 0) },
            new[] { 1d, 1, 1, 1 }, lc, CoordinateSystem.Global)));
        var error = Assert.ThrowsException<NotSupportedException>(() => Straus7ExporterCorrectionsTest.Record(model));
        StringAssert.Contains(error.Message, "NonUniformPlatePressureRequiresThePlateCorners");
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeNodalForcesGiveTheDisplacementsOfTheDistributedPressureAndEquilibrium()
    {
        const double q = 0.002; // N/mm²
        LoadCaseBase distributed = new("1 distributed"), nodal = new("2 nodal"), varying = new("3 varying");
        Func<Point3d, double> hydrostatic = p => -q * (1 + p.X / 1000 + 0.5 * p.Y / 1000);
        var (model, nodes, plates) = Strip((p, n) =>
        {
            foreach (var plate in p)
            {
                plate.Loads.Add(ShellFacePressure.Normal(distributed, ShellFace.PositiveNormal, q)); // pushes along -V3 = -Z
                plate.Loads.Add(Varying(plate, nodal, _ => -q));                                     // the same uniform pressure, as nodal forces
                plate.Loads.Add(Varying(plate, varying, hydrostatic));
            }
        });
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, Path.Combine(folder, "nodal-pressure.st7"), folder);
        var free = nodes.Where(n => n.Position.X > 0).ToArray();
        using (var reader = new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath, folder); reader.OpenResults(run.ResultPath);
            // 1 and 2: the same displacements (Straus7 builds from the pressure the load vector written in case 2).
            foreach (var node in free)
            {
                var a = reader.ReadNodeResult(node.Id, 1, false).Values; var b = reader.ReadNodeResult(node.Id, 2, false).Values;
                double scale = a.Take(3).Max(Math.Abs);
                for (int k = 0; k < 6; k++) Assert.AreEqual(a[k], b[k], 1e-9 * Math.Max(scale, 1e-12), $"node {node.Id} component {k}");
            }
            Assert.IsTrue(reader.ReadNodeResult(free.Last().Id, 1, false).Values[2] < 0, "the plate bends along -Z");
            // 3: reactions in equilibrium with the resultant of the varying pressure (force and moment about the origin).
            var resultant = new Vector3d(0, 0, 0); var moment = new Vector3d(0, 0, 0);
            foreach (var plate in plates) { var (f, m) = plate.Loads.Values.OfType<NonUniformPlatePressure>().Single(l => l.LoadCase.Name == varying.Name).GetGlobalResultant(Point3d.Origin); resultant += f; moment += m; }
            var reactions = new Vector3d(0, 0, 0); var reactionMoment = new Vector3d(0, 0, 0);
            foreach (var node in nodes.Where(n => n.Position.X == 0))
            {
                var r = reader.ReadNodeResult(node.Id, 3, true).Values; var force = new Vector3d(r[0], r[1], r[2]);
                reactions += force; reactionMoment += new Vector3d(Point3d.Origin, node.Position).CrossProduct(force) + new Vector3d(r[3], r[4], r[5]);
            }
            AssertVector(resultant * -1, reactions, 1e-6 * resultant.Length, "reaction force");
            AssertVector(moment * -1, reactionMoment, 1e-6 * moment.Length, "reaction moment");
        }
        // Output only: reading the .st7 back gives point loads on the nodes, with the resultant of the pressure.
        var back = new Straus7ApiConverter().Import(new Straus7ImportRequest { ModelPath = run.ModelPath, ModelRevision = "nodal-pressure", AnalysisId = "nodal-pressure" });
        Assert.IsNotNull(back.Model, string.Join("; ", back.Diagnostics.Select(d => d.Message)));
        Assert.IsFalse(back.Model.AreaElements.Values.SelectMany(p => p.Loads.Values).OfType<NonUniformPlatePressure>().Any());
        var reread = back.Model.NodesElements.Values.SelectMany(n => n.Loads.Values.OfType<PointLoad>()).Where(l => l.LoadCase.Name == varying.Name)
            .Select(l => { var g = l.GetGlobalForces(); return new Vector3d(g[0], g[1], g[2]); }).Aggregate(new Vector3d(0, 0, 0), (s, f) => s + f);
        var expectedTotal = plates.Select(p => p.Loads.Values.OfType<NonUniformPlatePressure>().Single(l => l.LoadCase.Name == varying.Name).GetGlobalLoadVector()).Aggregate((x, y) => x + y);
        AssertVector(expectedTotal, reread, 1e-6 * expectedTotal.Length, "resultant read back");
        Console.WriteLine("Nodal pressure: " + run.ModelPath);
    }
}
