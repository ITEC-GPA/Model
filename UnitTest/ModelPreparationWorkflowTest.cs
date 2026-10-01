using GPC.Examples;
using GPC.Model.Elements;
using GPC.Model.PostProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class ModelPreparationWorkflowTest
{
    internal static PreparationRequest Request() => new PreparationRequest { Results = new[] { new ResultSelection { Dataset = "synthetic-static", Case = "P+" } }, Settings = "test" };

    [TestMethod]
    public void MixedGroup_AllStationsAndShellPoints_PrepareWithoutDuplicateMembers()
    {
        var model = MixedModelFactory.Create(); model.AssignGroup("Wall", new Element[] { model.BeamElements[250] });
        var request = Request(); request.Selection.Groups = new[] { "Wall" }; request.Selection.Elements = new[] { new ElementKey { Family = EntityFamily.Beam, Id = 250 } };
        request.Results = new[] { request.Results[0], request.Results[0].Copy() };
        var plan = ModelPreparation.Prepare(model, request);
        Assert.AreEqual(7, plan.Samples.Count); Assert.AreEqual(5, plan.Samples.Count(s => s.Beam?.Status == DataStatus.Ready));
        Assert.AreEqual(2, plan.Samples.Count(s => s.Shell?.Status == DataStatus.Ready));
        var shell = plan.Samples.First(s => s.Shell != null).Shell.Input;
        Assert.AreEqual(300, shell.Assignments.PhysicalThickness); Assert.AreEqual(2, shell.Assignments.Layers.Count); Assert.IsTrue(shell.IsCurrent);
        shell.LocalForces.Mxx += 1; Assert.IsFalse(shell.IsCurrent);
    }

    [TestMethod]
    public void MissingResultsAndCancelledPreparation_KeepAllRequiredEntries()
    {
        var model = MixedModelFactory.Create(); var request = Request(); request.Results[0].Step = "unexported-step";
        var missing = ModelPreparation.Prepare(model, request);
        Assert.AreEqual(3, missing.Samples.Count); Assert.IsTrue(missing.Samples.All(s => s.Sample == null));
        Assert.AreEqual(DataStatus.Insufficient, missing.Samples[0].Beam.Status);
        request.Results[0].Step = null;
        var cancelled = ModelPreparation.Prepare(model, request, new CancellationToken(true));
        Assert.AreEqual(7, cancelled.Samples.Count); Assert.IsTrue(cancelled.Samples.All(s => s.Cancelled));
    }

    [TestMethod]
    public void GroupMembershipChanges_InvalidateScopeButNotAnalysis_AndRequestIsCopied()
    {
        var model = MixedModelFactory.Create(); var request = Request(); request.Selection.Groups = new[] { "Wall" };
        var plan = ModelPreparation.Prepare(model, request); var physical = model.AnalysisFingerprint();
        request.Results[0].Case = "P-"; Assert.AreEqual("P+", plan.Request.Results[0].Case);
        Assert.AreEqual(plan.ScopeFingerprint, ModelPreparation.FingerprintScope(model, plan.Request));
        model.AssignGroup("Wall", new Element[] { model.BeamElements[250] });
        Assert.AreEqual(physical, model.AnalysisFingerprint()); Assert.AreNotEqual(plan.ScopeFingerprint, ModelPreparation.FingerprintScope(model, plan.Request));
    }

    [TestMethod]
    public void PhaseChecks_RequireCumulativeConcomitantActions()
    {
        var model = MixedModelFactory.Create(); var sample = Verification.BeamSamples(model.BeamElements[250], "synthetic-static", "P+")[0];
        sample.State.Phase = "construction";
        Assert.IsTrue(Verification.PrepareBeam(model, 250, sample, "test").Diagnostics.Any(d => d.Code == "IncrementalOrUnknownPhaseState"));
        sample.State.IsCumulative = true; Assert.AreEqual(DataStatus.Ready, Verification.PrepareBeam(model, 250, sample, "test").Status);
        var shell = (GPC.Model.Results.ResultLocations.PointResultPlateForces)model.AreaElements[1090].Results[0].Results[0];
        shell.State.IsCumulative = false;
        Assert.IsTrue(ShellInputPreparation.Prepare(model, 1090, shell, "test").Diagnostics.Any(d => d.Code == "IncrementalOrUnknownPhaseState"));
    }

    [TestMethod]
    public void InvalidSelectionAndUnknownCase_DoNotBecomeEmptySuccess()
    {
        var model = MixedModelFactory.Create(); var request = Request(); request.Selection.Groups = new[] { "unknown" };
        Assert.ThrowsException<InvalidOperationException>(() => ModelPreparation.Prepare(model, request));
        request.Selection.Groups = Array.Empty<string>(); request.Results = Array.Empty<ResultSelection>();
        Assert.ThrowsException<ArgumentException>(() => ModelPreparation.Prepare(model, request));
    }
}
