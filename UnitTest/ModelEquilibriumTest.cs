using GPC.Examples;
using GPC.Geometry;
using GPC.Model.Loads;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class ModelEquilibriumTest
{
    private static EquilibriumScope Scope(params string[] expected)
    {
        var s = new EquilibriumScope { Selection = new ResultSelection { Dataset = "synthetic-static", Case = "P+", ConcomitantState = "P+" },
            CompleteCoverageConfirmed = true, CoverageEvidence = "Analytic test free body: all external actions listed." };
        s.ExpectedPhysicalActions.AddRange(expected); return s;
    }
    [TestMethod]
    public void Cantilever_NodalLoadAndReactionBalanceForcesAndMomentsAboutAnyPoint()
    {
        var model = MixedModelFactory.Create();
        var load = Equilibrium.PointLoad((PointLoad)model.NodesElements[40].Loads.Values.First(l => l.LoadCaseName == "P+"), "tip");
        var sample = model.NodesElements[10].Results.SelectMany(r => r.Results).OfType<NodeResultForces>().First(r => r.Case.Name == "P+");
        var reaction = Equilibrium.NodalAction(model, 10, sample, ActionBody.OnNode, "support", "total-reaction");
        var scope = Scope("tip", "support"); scope.ReferencePoint = new Point3d(200, 500, 900);
        var report = Equilibrium.Check(scope, new[] { load, reaction });
        Assert.AreEqual(DataStatus.Ready, report.Status, string.Join(";", report.Diagnostics.Select(d => d.Message)));
        Assert.AreEqual(true, report.IsBalanced); Assert.AreEqual(0, Axes.Length(report.MomentResidual), 1e-7);
        sample.ResultBeamForces.M1 += 1; Assert.AreEqual(DataStatus.Stale, Equilibrium.Check(scope, new[] { load, reaction }).Status);
    }

    [TestMethod]
    public void TriangularBeamLoad_IntegratesForceCentroidEccentricityAndDistributedCouples()
    {
        var model = MixedModelFactory.Create(); var beam = model.BeamElements[250]; var line = new Line3d(beam.StartPoint, beam.EndPoint); var lc = model.LoadCases["P+"];
        var load = new BeamLoadAssignment { Start = 0, End = 1, OriginalAssignmentId = "q", LengthConvention = BeamLoadLengthConvention.ActualLength,
            StartIntensity = new LineLoad(0, 0, 0, 0, 0, 0, line, lc), EndIntensity = new LineLoad(0, 6, 0, 0, 0, 4, line, lc),
            Eccentricity = new Vector3d(100, 0, 0), EccentricityCoordinateSystem = CoordinateSystem.Global };
        var c = Equilibrium.BeamLoad(beam, load);
        Assert.AreEqual(6000, c.Force.Y, 1e-8); Assert.AreEqual(-8e6, c.Moment.X, 1e-7); Assert.AreEqual(604000, c.Moment.Z, 1e-7);
        var support = Equilibrium.Explicit("support", "reaction", "P+", Point3d.Origin, new Vector3d(0, -6000, 0), new Vector3d(8e6, 0, -604000), "analytic");
        Assert.AreEqual(true, Equilibrium.Check(Scope("q", "support"), new[] { c, support }).IsBalanced);
        load.EndIntensity.F2 = 7; Assert.IsFalse(c.IsCurrent);
    }

    [TestMethod]
    public void ProjectedBeamLoad_UsesExplicitProjectionPlaneAndRejectsMissingNormal()
    {
        var m = MixedModelFactory.Create(); var b = m.BeamElements[250]; var lc = m.LoadCases["P+"]; var line = new Line3d(b.StartPoint, b.EndPoint);
        var load = new BeamLoadAssignment { Start = 0, End = 1, OriginalAssignmentId = "projected", LengthConvention = BeamLoadLengthConvention.ProjectedLength,
            StartIntensity = new LineLoad(1, 0, 0, 0, 0, 0, line, lc), EndIntensity = new LineLoad(1, 0, 0, 0, 0, 0, line, lc) };
        Assert.ThrowsException<ArgumentException>(() => Equilibrium.BeamLoad(b, load));
        load.ProjectionPlaneNormal = new Vector3d(Math.Sqrt(3) / 2, 0, .5);
        Assert.AreEqual(1000 * Math.Sqrt(3), Equilibrium.BeamLoad(b, load).Force.X, 1e-9);
    }

    [DataTestMethod]
    [DataRow("coverage")][DataRow("missing")][DataRow("duplicate")][DataRow("mixed-representation")][DataRow("missing-part")][DataRow("wrong-case")][DataRow("bad-tolerance")]
    public void Equilibrium_NeverReportsPassOnIncompleteOrDoubleCountedActions(string fault)
    {
        var a = Equilibrium.Explicit("a", "distributed", "P+", Point3d.Origin, new Vector3d(0, 0, 0), new Vector3d(0, 0, 0), "test");
        var terms = new List<EquilibriumContribution> { a }; var scope = Scope("a");
        switch (fault)
        {
            case "coverage": scope.CompleteCoverageConfirmed = false; break;
            case "missing": scope.ExpectedPhysicalActions.Add("absent"); break;
            case "duplicate": terms.Add(a); break;
            case "mixed-representation": terms.Add(Equilibrium.Explicit("a", "equivalent", "P+", Point3d.Origin, new Vector3d(0, 0, 0), new Vector3d(0, 0, 0), "test")); break;
            case "missing-part": a.ExpectedParts = 2; break;
            case "wrong-case": scope.Selection.Case = "P-"; break;
            case "bad-tolerance": scope.ForceTolerance = double.NaN; break;
        }
        var result = Equilibrium.Check(scope, terms); Assert.IsNull(result.IsBalanced); Assert.AreEqual(DataStatus.Insufficient, result.Status); Assert.IsTrue(result.Diagnostics.Count > 0);
    }

    [TestMethod]
    public void Equilibrium_ReportsUnbalancedAndAcceptsCompleteEquivalentParts()
    {
        var a = Equilibrium.Explicit("a", "nodal-equivalent", "P+", Point3d.Origin, new Vector3d(1, 0, 0), new Vector3d(0, 0, 0), "test");
        var b = Equilibrium.Explicit("a", "nodal-equivalent", "P+", Point3d.Origin, new Vector3d(-1, 0, 0), new Vector3d(0, 0, 0), "test");
        Assert.AreEqual(false, Equilibrium.Check(Scope("a"), new[] { a }).IsBalanced);
        a.Part = "I"; b.Part = "J"; a.ExpectedParts = b.ExpectedParts = 2;
        Assert.AreEqual(true, Equilibrium.Check(Scope("a"), new[] { a, b }).IsBalanced);
    }

    [TestMethod]
    public void UniformPlateTraction_UsesAreaAndPhysicalCentroid()
    {
        var m = MixedModelFactory.Create(); var shell = m.AreaElements[1090];
        var c = Equilibrium.ShellPressure(shell, new Vector3d(0, 2, 0), "P+", "pressure", "uniform pressure test");
        Assert.AreEqual(2e6, c.Force.Y, 1e-8); Assert.AreEqual(-2e6 * 4000 / 3, c.Moment.X, 1e-6);
        Assert.AreEqual(2e6 * 1000 / 3, c.Moment.Z, 1e-6);
        m.NodesElements[90].Position.X += 1; Assert.IsFalse(c.IsCurrent);
    }

    [TestMethod]
    public void AreaLoadArchive_RoundTripsBothVectorAndNormalPressure()
    {
        var m = MixedModelFactory.Create(); var shell = m.AreaElements[1090]; var shape = new Shape(new Polygon3d(shell.Points));
        shell.Loads.Add(new AreaLoad(1, 2, 3, shape, m.LoadCases["P+"])); shell.Loads.Add(new NormalAreaLoad(7, shape, m.LoadCases["P+"], shell.CoordinateSystem));
        using var stream = new MemoryStream(); ModelArchive.Save(m, stream); stream.Position = 0; var copy = ModelArchive.Load(stream);
        var loads = copy.AreaElements[1090].Loads.Values;
        Assert.AreEqual(3, loads.OfType<AreaLoad>().Single().P3); Assert.AreEqual(7, loads.OfType<NormalAreaLoad>().Single().Pressure);
        Assert.AreEqual(m.AnalysisFingerprint(), copy.AnalysisFingerprint());
    }
}
