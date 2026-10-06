using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class ModelLoadsTest
{
    private static readonly LoadCaseBase Case = new("H");
    private static Point3d[] Rectangle() => new[] { new Point3d(0, 0, 0), new Point3d(4000, 0, 0), new Point3d(4000, 2000, 0), new Point3d(0, 2000, 0) };

    [TestMethod]
    public void NonUniformPressure_RectangleWithLinearValues_HasTheTrapezoidResultant()
    {
        // p = 1 + x / 1000 (N/mm², x in mm) along -Z: total = 2000 * ∫0..4000 (1 + x/1000) dx = 2000 * 12000; centroid x = ∫p x / ∫p.
        var load = new NonUniformPlatePressure(Rectangle(), new[] { 1d, 5, 5, 1 }, Case, new CoordinateSystem(Point3d.Origin, new Vector3d(1, 0, 0), new Vector3d(0, -1, 0), new Vector3d(0, 0, -1)));
        Assert.AreEqual(2000 * 12000, load.GetTotalPressure(), 1e-6);
        var (force, moment) = load.GetGlobalResultant(new Point3d(0, 0, 0));
        Assert.AreEqual(-24e6, force.Z, 1e-6);
        double xc = 2000 * (4000.0 * 4000 / 2 + 4000.0 * 4000 * 4000 / 3000) / 24e6;
        Assert.AreEqual(-xc * force.Z, moment.Y, 1e-3, "My = -x Fz");
        Assert.AreEqual(1000 * force.Z, moment.X, 1e-3, "Mx = y Fz with y = 1000");
    }

    [TestMethod]
    public void NonUniformPressure_IrregularQuadAndTriangle_MatchFineNumericalIntegration()
    {
        var quad = new[] { new Point3d(0, 0, 0), new Point3d(3000, -500, 0), new Point3d(3500, 2500, 0), new Point3d(-200, 1800, 0) };
        var values = new[] { 2d, -1, 4, 0.5 };
        var load = new NonUniformPlatePressure(quad, values, Case, CoordinateSystem.Global);
        // Reference: midpoint rule on a 400 x 400 grid of the isoparametric map.
        double total = 0, mx = 0, my = 0; int n = 400;
        for (int i = 0; i < n; i++) for (int j = 0; j < n; j++)
        {
            double xi = -1 + (2 * i + 1.0) / n, eta = -1 + (2 * j + 1.0) / n;
            var w = new[] { (1 - xi) * (1 - eta) / 4, (1 + xi) * (1 - eta) / 4, (1 + xi) * (1 + eta) / 4, (1 - xi) * (1 + eta) / 4 };
            double x = 0, y = 0, p = 0; for (int k = 0; k < 4; k++) { x += w[k] * quad[k].X; y += w[k] * quad[k].Y; p += w[k] * values[k]; }
            double dxdxi = (-(1 - eta) * quad[0].X + (1 - eta) * quad[1].X + (1 + eta) * quad[2].X - (1 + eta) * quad[3].X) / 4;
            double dydxi = (-(1 - eta) * quad[0].Y + (1 - eta) * quad[1].Y + (1 + eta) * quad[2].Y - (1 + eta) * quad[3].Y) / 4;
            double dxdeta = (-(1 - xi) * quad[0].X - (1 + xi) * quad[1].X + (1 + xi) * quad[2].X + (1 - xi) * quad[3].X) / 4;
            double dydeta = (-(1 - xi) * quad[0].Y - (1 + xi) * quad[1].Y + (1 + xi) * quad[2].Y + (1 - xi) * quad[3].Y) / 4;
            double dA = Math.Abs(dxdxi * dydeta - dydxi * dxdeta) * (2.0 / n) * (2.0 / n);
            total += p * dA; mx += p * y * dA; my -= p * x * dA;
        }
        var (force, moment) = load.GetGlobalResultant(new Point3d(0, 0, 0));
        Assert.AreEqual(total, force.Z, 1e-5 * Math.Abs(total)); Assert.AreEqual(mx, moment.X, 1e-5 * Math.Abs(mx)); Assert.AreEqual(my, moment.Y, 1e-5 * Math.Abs(my));
        var triangle = new NonUniformPlatePressure(new[] { new Point3d(0, 0, 0), new Point3d(3000, 0, 0), new Point3d(0, 3000, 0) }, new[] { 3d, 0, 0 }, Case, CoordinateSystem.Global);
        Assert.AreEqual(4.5e6 * 3 / 3, triangle.GetTotalPressure(), 1e-6, "Linear: area times the mean of the vertex values.");
        Assert.AreEqual(4.5e6 * 750, triangle.GetGlobalResultant(new Point3d(0, 0, 0)).Moment.X, 1e-3, "Centroid of the linear load at y = 750.");
    }

    [DataTestMethod]
    [DataRow("count")]
    [DataRow("nonplanar")]
    [DataRow("concave")]
    [DataRow("nan")]
    public void NonUniformPressure_RejectsInvalidGeometryOrValues(string defect)
    {
        var points = Rectangle(); var values = new[] { 1d, 1, 1, 1 };
        if (defect == "count") values = new[] { 1d, 1, 1 };
        if (defect == "nonplanar") points[2] = new Point3d(4000, 2000, 10);
        if (defect == "concave") points[2] = new Point3d(1000, 500, 0);
        if (defect == "nan") values[1] = double.NaN;
        Assert.ThrowsException<ArgumentException>(() => new NonUniformPlatePressure(points, values, Case, CoordinateSystem.Global));
    }

    private static GPC.Model.Models.Model Slab(double offset)
    {
        var m = new GPC.Model.Models.Model("loads");
        var nodes = Rectangle().Select(p => new NodeElement(p, null)).ToArray(); foreach (var n in nodes) m.NodesElements.Add(n);
        var shell = new AreaElement(null, new ConcretePlateProperty(new GPC.Model.Materials.ConcreteMaterialEN1992("C30/37"), 300, 300, "slab"),
            new CoordinateSystem(new Point3d(2000, 1000, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0), new Vector3d(0, 0, 1)));
        m.AreaElements.Add(shell); m.ConnectShell(shell.Id, nodes.Select(n => n.Id).ToArray()); shell.Assignments.Offset = offset;
        var beam = new BeamElement(null, null, new SteelSection(new SectionRectangular(100, 50, "plate"), GPC.Model.Data.Steel.SteelMaterialEN1993Data.S355));
        m.BeamElements.Add(beam); m.ConnectBeam(beam.Id, nodes[0].Id, nodes[1].Id);
        beam.Assignments.Formulation = BeamFormulation.StraightTwoNode;
        beam.Assignments.SectionAxes = Axes.Beam(nodes[0].Position, nodes[1].Position, new Vector3d(0, 1, 0));
        var sw = new LoadCase("SW", LoadCase.LoadCaseTypes.SelfWeight); m.LoadCases.Add(sw);
        m.ModelLoads.Add(new ModelGravityLoad(new Vector3d(0, 0, -1), ModelGravityLoad.GRAVITYACCELERATION, sw, CoordinateSystem.Global));
        shell.Loads.Add(new NonUniformPlatePressure(nodes.Select(n => n.Position).ToArray(), new[] { 0d, 0.01, 0.01, 0 }, sw, shell.CoordinateSystem));
        return m;
    }

    [TestMethod]
    public void Gravity_WeighsBeamsAndPlatesOnTheirMidSurface()
    {
        var m = Slab(150); var gravity = m.ModelLoads.Values.OfType<ModelGravityLoad>().Single();
        var c = Equilibrium.Gravity(m, gravity, "self weight");
        double plate = 4000.0 * 2000 * 300 * 2.5e-9 * 9806.65, beam = 100 * 50 * 4000 * 7.85e-9 * 9806.65;
        Assert.AreEqual(-(plate + beam), c.Force.Z, 1e-6);
        Assert.AreEqual(-(plate * 1000 + beam * 0), c.Moment.X, 1e-3 * plate, "Mx = y Fz: plate centroid at y = 1000, beam on y = 0.");
        Assert.AreEqual(plate * 2000 + beam * 2000, c.Moment.Y, 1e-3 * plate, "My = -x Fz: both at x = 2000.");
        Assert.AreEqual(0, c.Moment.Z, 1e-6, "The offset along Z does not create a moment under vertical gravity.");
        var horizontal = new GPC.Model.Models.Model("h"); var lateral = Slab(150); lateral.ModelLoads.Clear();
        var sw = (LoadCase)lateral.LoadCases["SW"];
        lateral.ModelLoads.Add(new ModelGravityLoad(new Vector3d(1, 0, 0), 1000, sw, CoordinateSystem.Global));
        var x = Equilibrium.Gravity(lateral, lateral.ModelLoads.Values.OfType<ModelGravityLoad>().Single(), "inertia");
        double plateX = 4000.0 * 2000 * 300 * 2.5e-9 * 1000;
        Assert.AreEqual(plateX * 150, x.Moment.Y, 1e-6 * plateX * 150 + 1e-3, "Horizontal gravity: plate mass at z = +150 (offset along V3), beam at z = 0.");
        var withoutProperty = Slab(0); withoutProperty.BeamElements.Values.Single().BeamProperty = null;
        Assert.ThrowsException<ArgumentException>(() => Equilibrium.Gravity(withoutProperty, withoutProperty.ModelLoads.Values.OfType<ModelGravityLoad>().Single(), "sw"));
        var skipped = new List<Element>(); Equilibrium.Gravity(withoutProperty, withoutProperty.ModelLoads.Values.OfType<ModelGravityLoad>().Single(), "sw", skipped);
        Assert.AreEqual(1, skipped.Count);
    }

    [TestMethod]
    public void ModelLoadsAndPressures_SurviveTheArchiveAndEnterTheFingerprint()
    {
        var m = Slab(0); var fingerprint = m.AnalysisFingerprint();
        using var stream = new MemoryStream(); ModelArchive.Save(m, stream); stream.Position = 0; var copy = ModelArchive.Load(stream);
        Assert.AreEqual(fingerprint, copy.AnalysisFingerprint());
        var gravity = copy.ModelLoads.Values.OfType<ModelGravityLoad>().Single();
        Assert.AreEqual(-1, gravity.GravityVersor.Z); Assert.AreEqual(ModelGravityLoad.GRAVITYACCELERATION, gravity.Acceleration);
        Assert.AreEqual(LoadCase.LoadCaseTypes.SelfWeight, ((LoadCase)gravity.LoadCase).LoadCaseType);
        var pressure = copy.AreaElements.Values.Single().Loads.Values.OfType<NonUniformPlatePressure>().Single();
        CollectionAssert.AreEqual(new[] { 0d, 0.01, 0.01, 0 }, pressure.Pressures.ToArray());
        Assert.AreEqual(m.AreaElements.Values.Single().Loads.Values.OfType<NonUniformPlatePressure>().Single().GetTotalPressure(), pressure.GetTotalPressure(), 1e-9);
        gravity.Acceleration = 9000; Assert.AreNotEqual(fingerprint, copy.AnalysisFingerprint());
        var empty = new GPC.Model.Models.Model("e"); var before = empty.AnalysisFingerprint();
        using var plain = new MemoryStream(); ModelArchive.Save(empty, plain);
        Assert.IsFalse(System.Text.Encoding.UTF8.GetString(plain.ToArray()).Contains("ModelLoads"), "Models without model loads keep their archive layout.");
        Assert.AreEqual(before, empty.AnalysisFingerprint());
    }

    [TestMethod]
    public void ReverseShell_KeepsTheNonUniformPressureOnItsPhysicalPoints()
    {
        var m = Slab(0); var shellId = m.AreaElements.Values.Single().Id;
        var before = Equilibrium.PlatePressure(m.AreaElements[shellId].Loads.Values.OfType<NonUniformPlatePressure>().Single(), "p");
        var reversed = ModelOrientation.ReverseShell(m, shellId);
        var after = Equilibrium.PlatePressure(reversed.AreaElements[shellId].Loads.Values.OfType<NonUniformPlatePressure>().Single(), "p");
        Assert.AreEqual(before.Force.Z, after.Force.Z, 1e-9); Assert.AreEqual(before.Moment.X, after.Moment.X, 1e-6); Assert.AreEqual(before.Moment.Y, after.Moment.Y, 1e-6);
    }
}
