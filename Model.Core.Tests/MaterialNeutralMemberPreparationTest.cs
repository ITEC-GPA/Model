using GPC.Examples;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;
using GPC.Model.Core;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Data.Steel;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Queries;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using GPC.Model.Structure.Assignments;
using GPC.Model.Structure.Members;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Model.Core.Tests;

[TestClass]
public class MaterialNeutralMemberPreparationTest
{
    private static (GPC.Model.Models.Model, BeamCheckPlanRequest) Fixture()
    {
        var model = MixedModelFactory.Create(m => {
            var section = new SteelSection(new SectionH(300, 12, 150, 18, 150, 18, "H"), SteelMaterialEN1993Data.S355);
            var beam = m.BeamElements[250]; beam.BeamProperty = section;
            beam.Assignments.Sections.Clear(); beam.Assignments.Sections.Add(new BeamSectionAssignment { Start = 0, End = 1, Property = section });
        });
        model.PhysicalMembers.Add("S", new PhysicalMemberDefinition("S", new[] { new BeamMemberPart(250, true) }, "NodeToNode"));
        return (model, new BeamCheckPlanRequest { MemberIds = new[] { "S" }, SectionMechanisms = Array.Empty<CheckMechanism>(),
            Results = new[] { new ResultSelection { Dataset = "synthetic-static", Case = "P+", ConcomitantState = "P+" } },
            MemberChecks = new[] { new MemberCheckSpecification { MemberId = "S", MethodId = "fixture-method", Context = new MemberDesignContext(globalStateConfirmed: true) } } });
    }
    [TestMethod]
    public void NeutralPreparationPreservesLocationsActionsAndSnapshotsWithoutConcreteSections()
    {
        var (model, request) = Fixture(); var plan = BeamCheckPlan.PrepareActions(model, request); var work = plan.WorkItems.Single();
        Assert.AreEqual(DataStatus.Ready, work.Data, string.Join(";", work.Diagnostics.Select(d => d.Code)));
        Assert.IsTrue(plan.IsCurrent); Assert.AreEqual(CheckScope.PhysicalMember, work.Scope);
        Assert.AreEqual(5, work.Member.ActionSections.Count); Assert.AreEqual(0, work.Member.Sections.Count);
        Assert.AreEqual(2000d, work.Member.Snapshot.Length);
        CollectionAssert.AreEqual(new[] { 0d, 500, 1000, 1500, 2000 }, work.Member.Snapshot.Samples.Select(s => s.Location.Distance).ToArray());
        for (int j = 0; j < 5; j++)
        {
            Assert.IsInstanceOfType<SteelSection>(work.Member.ActionSections[j].Property);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { work.Member.ActionSections[j].Snapshot }),
                ModelValues.Fingerprint(new object[] { work.Member.Snapshot.Samples[j].Input }));
        }
    }
    [DataTestMethod]
    [DataRow("prepared actions")] [DataRow("prepared property")] [DataRow("source actions")]
    public void PreparedAndSourceChangesInvalidateMemberPlan(string change)
    {
        var (model, request) = Fixture(); var plan = BeamCheckPlan.PrepareActions(model, request); var input = plan.WorkItems.Single().Member.ActionSections[0];
        Assert.IsTrue(plan.IsCurrent);
        if (change == "prepared actions") input.Forces.N = -1;
        else if (change == "prepared property") ((SteelSection)input.Property).SteelMaterial.Fyk *= .9;
        else ResultQueries.Samples<StationResultBeamForces>(model.BeamElements[250], request.Results[0])[0].ResultBeamForces.N = -1;
        Assert.IsFalse(plan.IsCurrent);
    }
    [TestMethod]
    public void LegacyConcretePreparationStillReportsMissingReinforcement()
    {
        var (model, request) = Fixture(); var work = BeamCheckPlan.Prepare(model, request).WorkItems.Single();
        Assert.AreNotEqual(DataStatus.Ready, work.Data);
        Assert.AreEqual(0, work.Member.ActionSections.Count);
    }
}
