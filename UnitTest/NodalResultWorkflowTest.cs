using GPC.Converter;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class NodalResultWorkflowTest
{
    private static ResultState State(int n) => new() { Semantics = AnalysisSemantics.LinearStatic, ConcomitantStateId = "G", IsCombined = false,
        IsCumulative = true, Components = Enumerable.Repeat(ComponentAvailability.Available, n).ToArray() };
    private static ResultSelection Selection => new() { Dataset = "nodal", Case = "G" };
    private static (GPC.Model.Models.Model m, ResultImportBatch batch) Fixture()
    {
        var geometry = new ImportBatch { Program = "Fixture", SolverVersion = "1", ModelRevision = "v1", AnalysisId = "G", SourceHash = "fixture-geometry" };
        var p = new Point3d(1000, 2000, 0);
        geometry.Nodes.Add(new NodeRecord { Id = "1", GlobalPosition = p, CoordinateSystem = new CoordinateSystem(p, new Vector3d(0, 1, 0), new Vector3d(-1, 0, 0)) });
        geometry.Nodes.Add(new NodeRecord { Id = "2", GlobalPosition = new Point3d(1000, 2000, 2000) });
        geometry.Nodes.Add(new NodeRecord { Id = "3", GlobalPosition = new Point3d(2000, 2000, 2000) });
        geometry.Beams.Add(new BeamRecord { Id = "1", I = "1", J = "2", CoordinateSystem = ResultTransformations.AtPoint(CoordinateSystem.Global, p) });
        var plateAxes = new CoordinateSystem(p, new Vector3d(0, 0, 1), new Vector3d(1, 0, 0));
        geometry.Shells.Add(new ShellRecord { Id = "1", Nodes = new[] { "1", "2", "3" }, CoordinateSystem = plateAxes });
        geometry.LoadCases.Add(new LoadCaseRecord { Name = "G" });
        var mapped = ModelMapper.Map(geometry); Assert.IsNotNull(mapped.Model, string.Join(";", mapped.Diagnostics.Select(d => d.Message)));
        var m = mapped.Model;
        var axes = ResultTransformations.AtPoint(CoordinateSystem.Global, new Point3d(1, 2, 0));
        var batch = new ResultImportBatch { Source = m.AnalysisSource, DatasetId = "nodal", ExpectedInputFingerprint = m.AnalysisFingerprint(), SourceHash = "fixture-results",
            ReaderVersion = "fixture-1", Units = new ResultUnits(1000, 1000, 1e6, Math.PI / 180), ShellDenominatorLengthToMm = 1000,
            ResolvedConvention = "Explicit physical vector components and positive section face", IsSynthetic = true, Semantics = AnalysisSemantics.LinearStatic };
        batch.NodeForces.Add(new NodeForceRecord { ElementId = "1", Case = "G", Axes = axes, Record = "support", Values = new double?[] { 1, 2, 3, 4, 5, 6 },
            Kind = NodalForceKind.SupportReaction, Body = ActionBody.OnNode, State = State(6) });
        batch.NodeForces.Add(new NodeForceRecord { ElementId = "1", Case = "G", Axes = axes, Record = "beam I", Values = new double?[] { -7, -8, -9, -10, -11, -12 },
            Kind = NodalForceKind.ElementEndForce, Body = ActionBody.OnElement, OwnerSource = m.BeamElements.Values.Single().Source, ElementEnd = "I", State = State(6) });
        batch.NodeForces.Add(new NodeForceRecord { ElementId = "1", Case = "G", Axes = axes, Record = "plate node 1", Values = new double?[] { 1, 2, 3, 4, 5, 6 },
            Kind = NodalForceKind.ElementNodeForce, Body = ActionBody.OnElement, OwnerSource = m.AreaElements.Values.Single().Source, State = State(6) });
        batch.NodeDisplacements.Add(new NodeDisplacementRecord { ElementId = "1", Case = "G", Axes = axes, Record = "displacement", Values = new double?[] { .01, .02, .03, 10, 20, 30 }, State = State(6) });
        batch.Beams.Add(new BeamForceRecord { ElementId = "1", Case = "G", Record = "beam station", Axes = ResultTransformations.AtPoint(axes, new Point3d(1, 2, 1)),
            Values = new double?[] { 1, 2, 3, 4, 5, 6 }, State = State(6), Body = ActionBody.PositiveSectionFace, StationDomain = "NodeToNode", Station = .5, PhysicalDistance = 1 });
        batch.Shells.Add(new ShellForceRecord { ElementId = "1", Case = "G", Record = "plate point", Axes = ResultTransformations.AtPoint(plateAxes, axes.Origin),
            Values = new double?[] { 1, 2, 3, 4, 5, 6, 7, 8 }, State = State(8), PointKind = ShellResultPointKind.IntegrationPoint,
            CoordinateKind = ResultCoordinateKind.LocalPhysical, Location = new Point2d(.2, .3) });
        return (m, batch);
    }
    private static NodeElement Node(GPC.Model.Models.Model m) => m.NodesElements.Values.Single(n => n.Source.OriginalId == "1");
    private static NodeResultForces Reaction(GPC.Model.Models.Model m) => ResultQueries.NodeForce(Node(m), Selection, NodalForceKind.SupportReaction, ActionBody.OnNode);
    private static ElementKey Key(NodeElement n) => new() { Family = EntityFamily.Node, Id = n.Id };
    private static void Import(GPC.Model.Models.Model m, ResultImportBatch batch)
    { var report = ResultMapper.Import(m, batch); Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join(";", report.Diagnostics.Select(d => d.Message))); }

    [TestMethod]
    public void ImportAndArchive_PreserveNodalUnitsKindsOwnersAndSourceComponents()
    {
        var (m, batch) = Fixture(); var before = m.AnalysisFingerprint(); Import(m, batch);
        Assert.AreEqual(before, m.AnalysisFingerprint()); Assert.AreEqual(6, m.AllElements.Sum(e => e.Results.Sum(r => r.Results.Count)));
        using var stream = new MemoryStream(); ModelArchive.Save(m, stream); stream.Position = 0; m = ModelArchive.Load(stream);
        var f = Reaction(m); Assert.AreEqual(1000, f.Fx); Assert.AreEqual(2000, f.Fy); Assert.AreEqual(3000, f.Fz);
        Assert.AreEqual(4e6, f.Mx); Assert.AreEqual(5e6, f.My); Assert.AreEqual(6e6, f.Mz);
        Assert.AreEqual("Fx,Fy,Fz,Mx,My,Mz", f.State.Original.ComponentOrder);
        var forces = ResultQueries.Samples<NodeResultForces>(Node(m), Selection);
        CollectionAssert.AreEquivalent(new[] { EntityFamily.Beam, EntityFamily.Shell }, forces.Where(r => r.OwnerElementFamily.HasValue).Select(r => r.OwnerElementFamily!.Value).ToArray());
        var d = ResultQueries.Samples<NodeResultDisplacement>(Node(m), Selection).Single();
        Assert.AreEqual(10, d.ResultDisplacement.D1); Assert.AreEqual(30, d.ResultDisplacement.D3);
        Assert.AreEqual(Math.PI / 6, d.ResultDisplacement.R3, 1e-12); Assert.AreEqual(Math.PI / 180, d.State.Original.AngleToRad);
        Assert.AreEqual("Dx,Dy,Dz,Rx,Ry,Rz", d.State.Original.ComponentOrder);
        batch.NodeForces[0].Values[0] = 99; Assert.AreEqual(1d, f.State.Original.Values[0]);
    }

    [DataTestMethod]
    [DataRow("unknown-kind")][DataRow("unknown-body")][DataRow("missing-owner")][DataRow("foreign-owner")]
    [DataRow("wrong-end")][DataRow("plate-end")][DataRow("body-support")][DataRow("support-owner")]
    [DataRow("spring-id")][DataRow("link-id")][DataRow("duplicate-force")][DataRow("duplicate-displacement")]
    [DataRow("nan")][DataRow("missing-value")][DataRow("bad-axes")][DataRow("wrong-origin")][DataRow("missing-node")]
    public void InvalidNodalData_RejectsTheEntireMixedDataset(string fault)
    {
        var (m, batch) = Fixture(); var f = batch.NodeForces[0]; var end = batch.NodeForces[1];
        switch (fault)
        {
            case "unknown-kind": f.Kind = NodalForceKind.Unknown; break;
            case "unknown-body": f.Body = ActionBody.Unknown; break;
            case "missing-owner": end.OwnerSource = null; break;
            case "foreign-owner": end.OwnerSource = new SourceIdentity("Fixture", "v1", EntityFamily.Beam, "absent"); break;
            case "wrong-end": end.ElementEnd = "J"; break;
            case "plate-end": batch.NodeForces[2].ElementEnd = "I"; break;
            case "body-support": end.Body = ActionBody.OnSupport; break;
            case "support-owner": f.OwnerSource = end.OwnerSource; break;
            case "spring-id": f.Kind = NodalForceKind.SpringForce; break;
            case "link-id": f.Kind = NodalForceKind.LinkForce; break;
            case "duplicate-force": batch.NodeForces.Add(f); break;
            case "duplicate-displacement": batch.NodeDisplacements.Add(batch.NodeDisplacements[0]); break;
            case "nan": batch.NodeDisplacements[0].Values[5] = double.NaN; break;
            case "missing-value": f.Values[0] = null; break;
            case "bad-axes": f.Axes = new CoordinateSystem(f.Axes.Origin, new Vector3d(1, 0, 0), new Vector3d(0, 1, 0), new Vector3d(0, 0, -1)); break;
            case "wrong-origin": f.Axes = CoordinateSystem.Global; break;
            case "missing-node": f.ElementId = "absent"; break;
        }
        var before = m.AnalysisFingerprint(); Assert.AreEqual(ImportStatus.Rejected, ResultMapper.Import(m, batch).Status, fault);
        Assert.AreEqual(0, m.Datasets.Count); Assert.AreEqual(0, m.AllElements.Sum(e => e.Results.Count)); Assert.AreEqual(before, m.AnalysisFingerprint());
    }

    [TestMethod]
    public void NodalRotations_AnalyticComponentsAndExplicitMomentTransport()
    {
        var (m, batch) = Fixture(); Import(m, batch); var f = Reaction(m); var n = Node(m);
        var rotated = ResultTransformations.RotateNode(f, n.CoordinateSystem);
        Assert.AreEqual(2000, rotated.Fx, 1e-8); Assert.AreEqual(-1000, rotated.Fy, 1e-8); Assert.AreEqual(5e6, rotated.Mx, 1e-8); Assert.AreEqual(-4e6, rotated.My, 1e-8);
        Assert.ThrowsException<NotSupportedException>(() => ResultTransformations.RotateNode(f, CoordinateSystem.Global));
        var moved = ResultTransformations.TransportNode(f, CoordinateSystem.Global);
        Assert.AreEqual(10e6, moved.Mx, 1e-8); Assert.AreEqual(2e6, moved.My, 1e-8); Assert.AreEqual(6e6, moved.Mz, 1e-8);
        var d = ResultQueries.Samples<NodeResultDisplacement>(n, Selection).Single();
        var rd = ResultTransformations.RotateNode(d, n.CoordinateSystem);
        Assert.AreEqual(20, rd.ResultDisplacement.D1, 1e-8); Assert.AreEqual(-10, rd.ResultDisplacement.D2, 1e-8);
        Assert.AreEqual(20 * Math.PI / 180, rd.ResultDisplacement.R1, 1e-12);
        Assert.ThrowsException<NotSupportedException>(() => ResultTransformations.RotateNode(d, CoordinateSystem.Global));
        rotated.State.Original.Values[0] = 77; Assert.AreEqual(1d, f.State.Original.Values[0]);
        rotated.State.Original.Axes.Origin.X = 100; Assert.AreEqual(1, f.State.Original.Axes.Origin.X);
    }

    [TestMethod]
    public void ShellLocalPoint_RotatesWithAxesWhilePhysicalPointAndNaturalCoordinatesStayFixed()
    {
        var (m, batch) = Fixture(); Import(m, batch);
        var plate = m.AreaElements.Values.Single(); var p = ResultQueries.Samples<PointResultPlateForces>(plate, Selection).Single();
        var axes = p.Forces.CoordinateSystem; var target = new CoordinateSystem(axes.Origin, axes.V2, axes.V1 * -1, axes.V3);
        var rotated = ResultTransformations.RotateShell(p, target);
        Assert.AreEqual(300, rotated.Location.X, 1e-8); Assert.AreEqual(-200, rotated.Location.Y, 1e-8);
        var world = axes.Origin + axes.V1 * p.Location.X + axes.V2 * p.Location.Y;
        var other = target.Origin + target.V1 * rotated.Location.X + target.V2 * rotated.Location.Y;
        Assert.AreEqual(0, Axes.Length(world - other), 1e-8); Assert.AreEqual(200, p.Location.X);
        p.CoordinateKind = ResultCoordinateKind.Natural;
        Assert.AreEqual(200, ResultTransformations.RotateShell(p, target).Location.X);
    }

    [TestMethod]
    public void Preparation_SelectsGroupsKeepsMissingRowsAndSeparatesActionsFromDesignChecks()
    {
        var (m, batch) = Fixture(); Import(m, batch); m.AddGroup("scope");
        m.AssignGroup("scope", m.AllElements.Where(e => e.Source.OriginalId != "3"));
        var scope = new ElementSelection { Groups = new[] { "scope" }, Families = new[] { EntityFamily.Node, EntityFamily.Beam, EntityFamily.Shell } };
        var prepared = ResultPreparation.Prepare(m, scope, new[] { Selection });
        Assert.AreEqual(7, prepared.Count); Assert.AreEqual(6, prepared.Count(p => p.IsCurrent)); Assert.AreEqual(1, prepared.Count(p => p.LocalSample == null));
        var nodal = prepared.Single(p => p.LocalSample is NodeResultForces f && f.Kind == NodalForceKind.SupportReaction);
        Assert.AreEqual(2000, ((NodeResultForces)nodal.LocalSample).Fx, 1e-8);
        Assert.ThrowsException<NotSupportedException>(() => ModelPreparation.Prepare(m, new PreparationRequest { Selection = scope, Results = new[] { Selection } }));
        var beam = m.BeamElements.Values.Single(); var b = ResultQueries.Samples<StationResultBeamForces>(beam, Selection).Single();
        Assert.AreNotEqual(DataStatus.Ready, Verification.PrepareBeam(m, beam.Id, b, "missing design properties").Status);
        var cancelled = ResultPreparation.Prepare(m, Key(Node(m)), Reaction(m), cancellationToken: new CancellationToken(true));
        Assert.IsTrue(cancelled.Cancelled); Assert.IsFalse(cancelled.IsCurrent); Assert.IsNull(cancelled.LocalSample);
    }

    [DataTestMethod]
    [DataRow("source")][DataRow("prepared")][DataRow("model")][DataRow("dataset")][DataRow("scope")][DataRow("removed")]
    public void PreparedSnapshots_InvalidateAfterMutations(string change)
    {
        var (m, batch) = Fixture(); Import(m, batch); var n = Node(m); var f = Reaction(m); var p = ResultPreparation.Prepare(m, Key(n), f);
        Assert.IsTrue(p.IsCurrent);
        switch (change)
        {
            case "source": f.ResultBeamForces.V1++; break;
            case "prepared": ((NodeResultForces)p.LocalSample).ResultBeamForces.V1++; break;
            case "model": n.Position.X++; break;
            case "dataset": m.Datasets["nodal"].Semantics = AnalysisSemantics.ResponseSpectrum; break;
            case "scope": p.Element.Id++; break;
            case "removed": n.Results.Clear(); break;
        }
        Assert.IsFalse(p.IsCurrent, change);
    }

    [TestMethod]
    public void MissingComponentsAndModalDisplacements_AreStoredButNotPreparedAsCompletePhysicalResults()
    {
        var (m, batch) = Fixture(); batch.NodeForces[0].Values[1] = null; batch.NodeForces[0].State.Components[1] = ComponentAvailability.NotExported;
        Assert.AreEqual(ImportStatus.Partial, ResultMapper.Import(m, batch).Status); var f = Reaction(m);
        Assert.IsTrue(double.IsNaN(f.Fy)); Assert.IsFalse(ResultPreparation.Prepare(m, Key(Node(m)), f).IsCurrent);
        Assert.ThrowsException<ArgumentException>(() => ResultTransformations.RotateNode(f, Node(m).CoordinateSystem));
        var d = ResultQueries.Samples<NodeResultDisplacement>(Node(m), Selection).Single();
        m.Datasets["nodal"].Semantics = d.State.Semantics = AnalysisSemantics.Modal;
        Assert.AreEqual(DataStatus.NotSupported, ResultPreparation.Prepare(m, Key(Node(m)), d).Status);
    }

    [DataTestMethod]
    [DataRow(NodalForceKind.SpringForce)][DataRow(NodalForceKind.LinkForce)]
    public void SpringAndLinkActions_KeepDistinctSourceIdentities(NodalForceKind kind)
    {
        var (m, batch) = Fixture(); batch.NodeForces[0].Kind = kind; batch.NodeForces[0].AggregationSet = "source-1";
        batch.NodeForces.Add(new NodeForceRecord { ElementId = "1", Case = "G", Axes = batch.NodeForces[0].Axes, Values = new double?[] { 7, 8, 9, 10, 11, 12 },
            Kind = kind, Body = ActionBody.OnNode, AggregationSet = "source-2", State = State(6), Record = "another contribution" });
        Import(m, batch);
        Assert.AreEqual(1000, ResultQueries.NodeForce(Node(m), Selection, kind, ActionBody.OnNode, aggregationSet: "source-1").Fx);
        Assert.AreEqual(7000, ResultQueries.NodeForce(Node(m), Selection, kind, ActionBody.OnNode, aggregationSet: "source-2").Fx);
    }

    [TestMethod]
    public void DuplicateNodeWithDifferentOrientation_IsNotSilentlyMerged()
    {
        var batch = new ImportBatch { Program = "Fixture", ModelRevision = "axes" };
        batch.Nodes.Add(new NodeRecord { Id = "1", GlobalPosition = new Point3d(1, 2, 3) });
        batch.Nodes.Add(new NodeRecord { Id = "1", GlobalPosition = new Point3d(1, 2, 3),
            CoordinateSystem = new CoordinateSystem(Point3d.Origin, new Vector3d(0, 1, 0), new Vector3d(-1, 0, 0)) });
        var result = ModelMapper.Map(batch); Assert.AreEqual(ImportStatus.Rejected, result.Status); Assert.IsNull(result.Model);
    }

    [DataTestMethod]
    [DataRow("beam-axis")][DataRow("shell-normal")][DataRow("node-origin")][DataRow("stale-model")]
    public void PreparationRejectsInconsistentAxesAndStaleAnalysis(string change)
    {
        var (m, batch) = Fixture(); Import(m, batch);
        if (change == "beam-axis")
        {
            var beam = m.BeamElements.Values.Single(); beam.Assignments.SectionAxes = new CoordinateSystem(beam.StartPoint, new Vector3d(0, 1, 0), new Vector3d(0, 0, 1));
            var sample = ResultQueries.Samples<StationResultBeamForces>(beam, Selection).Single();
            // Isolate axis validation from fingerprint validation using a synthetic consistent dataset stamp.
            m.Datasets["nodal"].InputFingerprint = sample.State.InputFingerprint = m.AnalysisFingerprint();
            var prepared = ResultPreparation.Prepare(m, new ElementKey { Family = EntityFamily.Beam, Id = beam.Id }, sample);
            Assert.IsFalse(prepared.IsCurrent); Assert.IsTrue(prepared.Diagnostics.Any(d => d.Message == "BeamAxisMismatch"));
        }
        else if (change == "shell-normal")
        {
            var plate = m.AreaElements.Values.Single(); var sample = ResultQueries.Samples<PointResultPlateForces>(plate, Selection).Single(); var axes = sample.Forces.CoordinateSystem;
            sample.Forces.CoordinateSystem = new CoordinateSystem(axes.Origin, axes.V1, axes.V2 * -1, axes.V3 * -1);
            var prepared = ResultPreparation.Prepare(m, new ElementKey { Family = EntityFamily.Shell, Id = plate.Id }, sample);
            Assert.IsFalse(prepared.IsCurrent); Assert.IsTrue(prepared.Diagnostics.Any(d => d.Message == "ShellNormalMismatch"));
        }
        else
        {
            var sample = Reaction(m);
            if (change == "node-origin") sample.ResultBeamForces.CoordinateSystem = CoordinateSystem.Global;
            else Node(m).Position.X++;
            var prepared = ResultPreparation.Prepare(m, Key(Node(m)), sample); Assert.IsFalse(prepared.IsCurrent);
            Assert.AreEqual(change == "stale-model" ? DataStatus.Stale : DataStatus.NotSupported, prepared.Status);
        }
    }
}
