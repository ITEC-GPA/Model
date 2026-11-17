using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Compatibility;
using GPC.Model.Core.Units;
using GPC.Model.Models;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Processing;
using GPC.Model.Results.State;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

public partial class Straus7ApiWorkflowTest
{
    /// <summary>The reactions of <see cref="Straus7Equilibrium"/>, with one currency check of the dataset per case, are the
    /// <see cref="Equilibrium.NodalAction"/> contributions bit for bit; without a current dataset they go through NodalAction and fail as before.</summary>
    internal static void SameAsNodalActions(GPC.Model.Models.Model model, string caseName, string datasetId, Straus7CaseBalance balance)
    {
        var force = new Vector3d(0, 0, 0); var moment = new Vector3d(0, 0, 0); int count = 0;
        foreach (var node in model.NodesElements.Values)
            foreach (var sample in node.Results.SelectMany(r => r.Results).OfType<NodeResultForces>()
                .Where(s => s.Kind == NodalForceKind.SupportReaction && s.Case?.Name == caseName && s.State?.DatasetId == datasetId))
            {
                var c = Equilibrium.NodalAction(model, node.Id, sample, ActionBody.OnNode, "reaction " + ++count, "reaction");
                force += c.Force; moment += c.Moment + ((Vector3d)(c.Point - Point3d.Origin)).CrossProduct(c.Force);
            }
        Assert.IsTrue(count > 0); Assert.AreEqual(count, balance.Reactions);
        CollectionAssert.AreEqual(new[] { force.X, force.Y, force.Z }, balance.ReactionForce, caseName + " reaction force");
        CollectionAssert.AreEqual(new[] { moment.X, moment.Y, moment.Z }, balance.ReactionMoment, caseName + " reaction moment");
    }

    [TestMethod]
    public void Equilibrium_ReactionsAreTheNodalActionsBitForBit_AndAMissingDatasetFailsAsBefore()
    {
        var converter = new Straus7ApiConverter(() => new SyntheticApi()); var model = converter.Import(Request()).Model;
        var result = Straus7LinearStaticResults.Import(model, converter.ReadResults(ResultsRequest()), "fixture-results", CaseMap, isSynthetic: true);
        Assert.AreEqual(ImportStatus.Completed, result.Status, string.Join(";", result.Diagnostics.Select(d => d.Message)));
        SameAsNodalActions(model, "Case 1: G", "fixture-results", Straus7Equilibrium.Check(model, "Case 1: G", "fixture-results"));
        model.Datasets.Remove("fixture-results");
        Assert.ThrowsException<ArgumentException>(() => Straus7Equilibrium.Check(model, "Case 1: G", "fixture-results"), "NodalAction: DatasetMismatchOrStaleAnalysis.");
    }

    /// <summary>A uniform normal pressure on a 1000 x 1000 mm quadrilateral (−1 kN) against a support reaction at node 1 that balances it. With the fourth
    /// node 1e-4 mm out of plane (accepted by the planarity rule of the import, not by Equilibrium.ShellPressure, 1e-7 mm) the case is not evaluated,
    /// without an exception; plane, it is evaluated and balanced.</summary>
    [DataTestMethod]
    [DataRow(0d, true)]
    [DataRow(1e-4, false)]
    public void Equilibrium_PressureOnAPlateOutOfPlaneIsNotEvaluated_OnAPlanePlateItIs(double lift, bool evaluated)
    {
        var geometry = new ImportBatch { Program = "Fixture", SolverVersion = "1", ModelRevision = "v1", AnalysisId = "G", SourceHash = "fixture-geometry" };
        geometry.Nodes.Add(new NodeRecord { Id = "1", GlobalPosition = new Point3d(0, 0, 0) });
        geometry.Nodes.Add(new NodeRecord { Id = "2", GlobalPosition = new Point3d(1000, 0, 0) });
        geometry.Nodes.Add(new NodeRecord { Id = "3", GlobalPosition = new Point3d(1000, 1000, 0) });
        geometry.Nodes.Add(new NodeRecord { Id = "4", GlobalPosition = new Point3d(0, 1000, lift) });
        geometry.Shells.Add(new ShellRecord { Id = "1", Nodes = new[] { "1", "2", "3", "4" }, CoordinateSystem = new CoordinateSystem(new Point3d(500, 500, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0)) });
        geometry.LoadCases.Add(new LoadCaseRecord { Name = "G" });
        geometry.ShellLoads.Add(new ShellLoadRecord { ShellId = "1", Case = "G", Normal = true, Pressure = -0.001, Record = "pressure" });
        var mapped = ModelMapper.Map(geometry); Assert.IsNotNull(mapped.Model, string.Join(";", mapped.Diagnostics.Select(d => d.Message)));
        var m = mapped.Model;
        var batch = new ResultImportBatch { Source = m.AnalysisSource, DatasetId = "pressure", ExpectedInputFingerprint = m.AnalysisFingerprint(), SourceHash = "fixture-results",
            ReaderVersion = "fixture-1", Units = new ResultUnits(1, 1, 1), ShellDenominatorLengthToMm = 1, ResolvedConvention = "Reaction on the node, global axes",
            IsSynthetic = true, Semantics = AnalysisSemantics.LinearStatic };
        batch.NodeForces.Add(new NodeForceRecord { ElementId = "1", Case = "G", Axes = ResultTransformations.AtPoint(CoordinateSystem.Global, new Point3d(0, 0, 0)), Record = "support",
            Values = new double?[] { 0, 0, 1000, 500000, -500000, 0 }, Kind = NodalForceKind.SupportReaction, Body = ActionBody.OnNode,
            State = new ResultState { Semantics = AnalysisSemantics.LinearStatic, ConcomitantStateId = "G", IsCombined = false, IsCumulative = true,
                Components = Enumerable.Repeat(ComponentAvailability.Available, 6).ToArray() } });
        var imported = ResultMapper.Import(m, batch); Assert.AreEqual(ImportStatus.Completed, imported.Status, string.Join(";", imported.Diagnostics.Select(d => d.Message)));
        var balance = Straus7Equilibrium.Check(m, "G", "pressure");
        if (evaluated)
        {
            Assert.AreEqual(true, balance.Balanced, string.Join(";", balance.Diagnostics.Select(d => d.Message)));
            Assert.AreEqual(-1000, balance.AppliedForce[2], 1e-9); Assert.AreEqual(1000, balance.ReactionForce[2], 1e-9); Assert.AreEqual(0, balance.NotRepresented.Count);
        }
        else
        {
            Assert.IsNull(balance.Balanced);
            StringAssert.Contains(balance.Diagnostics.Single(d => d.Code == "Straus7EquilibriumNotEvaluated").Message, "InvalidPressureWindingOrPlane");
            StringAssert.Contains(balance.NotRepresented.Single(), "on plate 1 ");
        }
    }
}