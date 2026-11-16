using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
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
}
