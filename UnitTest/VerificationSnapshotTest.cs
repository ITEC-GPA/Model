using GPC.Examples;
using GPC.Model.Checking;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class VerificationSnapshotTest
{
    [TestMethod]
    public void FrozenInputsAreIndependentAndCurrentComparisonObservesChanges()
    {
        var source = MixedModelFactory.Create();
        var prepared = VerificationPreparation.Prepare(source, VerificationScenario.Create(source.Analysis.Id, "Stable"));
        var snapshot = VerificationSnapshot.Capture(prepared);
        var first = snapshot.OpenModel();
        Assert.IsTrue(snapshot.IsCurrent(first));
        first.NodesElements[40].Position.Z += 1;
        Assert.IsFalse(snapshot.IsCurrent(first));
        Assert.IsTrue(snapshot.IsCurrent(snapshot.OpenModel()));
        var sample = prepared.Model.BeamElements[250].Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().First();
        sample.ResultBeamForces.N += 1;
        Assert.IsFalse(snapshot.IsCurrent(prepared.Model));
        Assert.ThrowsException<InvalidOperationException>(() => VerificationSnapshot.Capture(prepared));
        Assert.IsTrue(snapshot.IsCurrent(snapshot.OpenModel()));
    }

    [TestMethod]
    public void ReadScopeDoesNotHideChangesBetweenPlanChecks()
    {
        var model = MixedModelFactory.Create();
        var request = new BeamCheckPlanRequest {
            Elements = new ElementSelection { Families = new[] { EntityFamily.Beam } },
            Results = new[] { new ResultSelection { Dataset = "synthetic-static", Case = "P+" } },
            SectionMechanisms = new[] { CheckMechanism.Shear } };
        var plan = BeamCheckPlan.PrepareActions(model, request);
        Assert.IsTrue(plan.IsCurrent);
        var original = model.NodesElements[40].Position.Z;
        model.NodesElements[40].Position.Z += 1;
        Assert.IsFalse(plan.IsCurrent);
        model.NodesElements[40].Position.Z = original;
        Assert.IsTrue(plan.IsCurrent);
        request.Settings = "changed";
        Assert.IsFalse(plan.IsCurrentFor(request));
        Assert.IsTrue(plan.IsCurrent);
        plan.WorkItems.First(w => w.Actions?.Input != null).Actions.Input.Forces.N += 1;
        Assert.IsFalse(plan.IsCurrent);
    }
}
