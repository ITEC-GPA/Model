using GPC.Examples;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;
using GPC.Model.Compatibility;
using GPC.Model.Core;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Queries;
using GPC.Model.Results.State;
using GPC.Model.Results.Storage;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using GPC.Model.Structure.Assignments;
using GPC.Model.Structure.Topology;

namespace UnitTest;

/// <summary>Plan preparation and revalidation write each property copy once and walk each model sample, property and dataset once
/// per read scope. The fingerprints are the same as without memo, every cut owns its copy, and any change made after Prepare (model,
/// dataset, settings, property, results, owned copies) is seen by the next check, also when it is undone.</summary>
[TestClass]
public class PreparationCacheTest
{
    private const string Settings = "Steel shear\nMaterialRouting:fixture";

    /// <summary>Beams sharing one steel section (half of them through a section assignment), one mid-span sample per case,
    /// two mechanisms per cut: the two tasks of a cut share its prepared input.</summary>
    private static (GPC.Model.Models.Model Model, BeamCheckPlanRequest Request, SteelSection Section) Fixture(int beams = 4, int cases = 2)
    {
        var model = new GPC.Model.Models.Model("Preparation cache");
        var section = new SteelSection(new GPC.Model.Sections.SectionH(300, 12, 150, 18, 150, 18, "Stocky H"), new SteelMaterial("S355", 210000, 355, 510));
        model.AddProperty(section);
        var group = model.AddGroup("Steel").Name; var all = new List<BeamElement>();
        for (int i = 0; i < beams; i++)
        {
            int id = 10 * (i + 1);
            model.NodesElements.Add(new NodeElement(new Point3d(i * 3000, 0, 0), id: id));
            model.NodesElements.Add(new NodeElement(new Point3d(i * 3000, 0, 1000), id: id + 1));
            var beam = new BeamElement(null, null, section, id: id);
            model.BeamElements.Add(beam); model.ConnectBeam(id, id, id + 1); all.Add(beam);
            beam.Assignments.Formulation = BeamFormulation.StraightTwoNode; beam.Assignments.SectionAxes = CoordinateSystem.Global;
            beam.Assignments.SectionGeometryAxes = CoordinateSystem.Global; beam.Assignments.StationDomain = "NodeToNode";
            beam.Assignments.ActionsAtSectionCentroidConfirmed = true;
            if (i % 2 == 1) beam.Assignments.Sections.Add(new BeamSectionAssignment { Start = 0, End = 1, Property = section });
        }
        model.AssignGroup(group, all.ToArray());
        var names = Enumerable.Range(0, cases).Select(k => "C" + k).ToArray();
        foreach (string name in names) model.LoadCases.Add(new LoadCaseBase(name));
        model.CaptureAnalysis(ReinforcementAnalysisRole.ExcludedFromAnalysis, "Assigned analytical actions.");
        string revision = model.AnalysisFingerprint();
        model.Datasets.Add("scale", new AnalysisDataset { Id = "scale", Program = "Assigned analytical fixture", ModelRevision = "1",
            InputFingerprint = revision, NormalizedUnits = "N,mm,rad", Semantics = AnalysisSemantics.LinearStatic, IsSynthetic = true });
        foreach (var beam in model.BeamElements.Values)
        {
            var point = new BeamReferenceGeometry(beam).PointAt(.5, "NodeToNode");
            beam.AddResult(new BeamResult(names.Select((name, k) => new StationResultBeamForces(model.LoadCases[name], new ResultBeamForces(1000 * k, 0, 1000 + beam.Id, 0, 2e6, 0,
                ResultTransformations.AtPoint(CoordinateSystem.Global, point)), .5) {
                StationDomain = "NodeToNode", PhysicalDistance = 500, Body = ActionBody.PositiveSectionFace,
                State = new ResultState { DatasetId = "scale", ModelRevision = "1", InputFingerprint = revision, ConcomitantStateId = name,
                    Components = Enumerable.Repeat(ComponentAvailability.Available, 6).ToArray(), IsCumulative = true, IsSynthetic = true,
                    Semantics = AnalysisSemantics.LinearStatic } }).ToArray()));
        }
        var request = new BeamCheckPlanRequest {
            Elements = new ElementSelection { Groups = new[] { group }, Families = new[] { EntityFamily.Beam } },
            Results = names.Select(n => new ResultSelection { Dataset = "scale", Case = n, ConcomitantState = n }).ToArray(),
            SectionMechanisms = new[] { CheckMechanism.Shear, CheckMechanism.UlsBiaxialSection }, Settings = Settings };
        return (model, request, section);
    }

    private static BeamActionInput[] Inputs(BeamCheckPlan plan) => plan.WorkItems.Select(w => w.Actions?.Input).Where(i => i != null).Distinct().ToArray();
    private static StationResultBeamForces ModelSample(GPC.Model.Models.Model model, int beam, int index = 0)
        => (StationResultBeamForces)model.BeamElements[beam].Results.SelectMany(r => r.Results).ElementAt(index);

    [TestMethod]
    public void PlanInputsCarryTheFingerprintsOfAnUnscopedPreparation()
    {
        var (model, request, section) = Fixture();
        var plan = BeamCheckPlan.PrepareActions(model, request);
        var inputs = Inputs(plan);
        Assert.AreEqual(8, inputs.Length); Assert.AreEqual(16, plan.WorkItems.Count);
        Assert.IsTrue(plan.WorkItems.All(w => w.Data == DataStatus.Ready), string.Join(";", plan.WorkItems.SelectMany(w => w.Diagnostics).Select(d => d.Code)));
        // Outside any read scope ModelValues.Fingerprint and the revisions read the model without memo.
        string revision = model.VerificationFingerprint(Settings), dataset = ModelValues.Fingerprint(new object[] { model.Datasets["scale"] });
        foreach (var input in inputs)
        {
            var beam = input.Element;
            Assert.AreEqual(revision, input.Revision);
            Assert.AreEqual(dataset, input.DatasetFingerprint);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { input.Property }), input.PreparedPropertyFingerprint);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { section }), input.PreparedPropertyFingerprint);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { beam.BeamProperty, beam.Assignments.Sections.ToArray() }), input.Snapshot.SectionFingerprint);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { input.Sample }), input.Snapshot.SampleFingerprint);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { input.Forces }), input.Snapshot.ForcesFingerprint);
            // The same cut prepared on its own, in its own scope.
            var single = BeamActionPreparation.Prepare(model, beam.Id, input.Sample, Settings).Input;
            Assert.AreEqual(single.PreparedPropertyFingerprint, input.PreparedPropertyFingerprint);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { single.Snapshot }), ModelValues.Fingerprint(new object[] { input.Snapshot }));
        }
        // Every cut owns a distinct copy of the one shared section.
        var properties = inputs.Select(i => i.Property).ToArray();
        Assert.AreEqual(properties.Length, properties.Distinct(InputFingerprint.ReferenceComparer.Instance).Count());
        Assert.IsTrue(properties.All(p => !ReferenceEquals(p, section) && !ReferenceEquals(((SteelSection)p).SteelMaterial, section.SteelMaterial)));
        // The scope digests and revisions are the same with and without memo, also when the memo is already filled.
        string scope = BeamCheckPlan.FingerprintScope(model, plan.Request), analysis = model.AnalysisFingerprint();
        string actions = ModelValues.Fingerprint(new object[] { "MaterialActions-v1", scope }
            .Concat(BeamCheckPlan.ResolveBeamIds(model, plan.Request).SelectMany(id => new object[] { id, model.BeamElements[id].BeamProperty })));
        Assert.AreEqual(actions, plan.ScopeFingerprint);
        using (ValidationReadScope.Enter(model))
        {
            Assert.AreEqual(scope, BeamCheckPlan.FingerprintScope(model, plan.Request));
            Assert.AreEqual(scope, BeamCheckPlan.FingerprintScope(model, request.Copy()));
            Assert.AreEqual(analysis, model.AnalysisFingerprint());
            Assert.AreEqual(revision, model.VerificationFingerprint(Settings));
        }
        Assert.IsTrue(plan.IsCurrent); Assert.IsTrue(plan.IsCurrentFor(request.Copy()));
    }

    [TestMethod]
    public void OwnedCopiesOfOneSourceStayIndependent()
    {
        var (model, request, _) = Fixture();
        var plan = BeamCheckPlan.PrepareActions(model, request);
        var inputs = Inputs(plan);
        // A plain field: the setters of the strengths rebuild the stress-strain tables, so they cannot be undone exactly.
        var changed = inputs[0]; double alpha = ((SteelSection)changed.Property).SteelMaterial.AlfaThermalExpansion;
        ((SteelSection)changed.Property).SteelMaterial.AlfaThermalExpansion = alpha * 2;
        Assert.IsFalse(changed.IsCurrent); Assert.IsFalse(plan.IsCurrent);
        foreach (var other in inputs.Skip(1))
        {
            Assert.AreEqual(alpha, ((SteelSection)other.Property).SteelMaterial.AlfaThermalExpansion);
            Assert.IsTrue(other.IsCurrent);
        }
        ((SteelSection)changed.Property).SteelMaterial.AlfaThermalExpansion = alpha;
        Assert.IsTrue(changed.IsCurrent); Assert.IsTrue(plan.IsCurrent);
    }

    [DataTestMethod]
    [DataRow("node position")]
    [DataRow("beam removed")]
    [DataRow("dataset metadata")]
    [DataRow("dataset removed")]
    [DataRow("source property")]
    [DataRow("section assignment")]
    [DataRow("sample forces")]
    [DataRow("sample state")]
    [DataRow("sample removed")]
    [DataRow("sample added")]
    [DataRow("owned forces")]
    [DataRow("owned property")]
    [DataRow("settings")]
    public void ChangesAfterPreparationMakeThePlanStale(string change)
    {
        var (model, request, section) = Fixture();
        var plan = BeamCheckPlan.PrepareActions(model, request);
        var input = plan.WorkItems.First(w => w.Target.BeamId == 20).Actions.Input;
        // Revalidate once first, so that a memo surviving its scope would answer the next checks.
        Assert.IsTrue(plan.IsCurrent); Assert.IsTrue(input.IsCurrent); Assert.IsTrue(plan.IsCurrentFor(request));
        var sample = ModelSample(model, 20);
        bool inputStale = true;
        switch (change)
        {
            case "node position": model.NodesElements[20].Position.Z += 1; break;
            case "beam removed": model.BeamElements.Remove(20); break;
            case "dataset metadata": model.Datasets["scale"].Program = "Other importer"; break;
            case "dataset removed": model.Datasets.Clear(); break;
            case "source property": section.SteelMaterial.Fyk *= .9; break;
            case "section assignment": model.BeamElements[20].Assignments.Sections.Clear(); break;
            case "sample forces": sample.ResultBeamForces.V2 += 1; break;
            case "sample state": sample.State.Phase = "other"; break;
            case "sample removed": model.BeamElements[20].Results.Clear(); break;
            case "sample added":
                // Another state at the same cut: the scope of the plan changes, the prepared input does not.
                inputStale = false;
                model.BeamElements[20].AddResult(new BeamResult(new[] { new StationResultBeamForces(model.LoadCases["C0"], new ResultBeamForces(0, 0, 1, 0, 0, 0,
                    ResultTransformations.AtPoint(CoordinateSystem.Global, sample.ResultBeamForces.CoordinateSystem.Origin)), .5) {
                    StationDomain = "NodeToNode", PhysicalDistance = 500, Body = ActionBody.PositiveSectionFace, State = new ResultState { DatasetId = "scale",
                        ModelRevision = "1", InputFingerprint = sample.State.InputFingerprint, ConcomitantStateId = "C0", Components = sample.State.Components.ToArray(),
                        IsCumulative = true, IsSynthetic = true, Semantics = AnalysisSemantics.LinearStatic } } }));
                break;
            case "owned forces": input.Forces.N += 1; break;
            case "owned property": ((SteelSection)input.Property).SteelMaterial.Fyk *= .9; break;
            case "settings":
                var changed = request.Copy(); changed.Settings = Settings + "\nother";
                Assert.IsFalse(plan.IsCurrentFor(changed));
                Assert.IsTrue(plan.IsCurrent); Assert.IsTrue(input.IsCurrent);
                return;
        }
        Assert.IsFalse(plan.IsCurrent, change);
        Assert.IsFalse(plan.IsCurrentFor(request), change);
        Assert.AreEqual(!inputStale, input.IsCurrent, change);
    }

    [DataTestMethod]
    [DataRow("node position")]
    [DataRow("source property")]
    [DataRow("sample forces")]
    [DataRow("owned property")]
    public void UndoneChangesMakeThePlanCurrentAgain(string change)
    {
        var (model, request, section) = Fixture();
        var plan = BeamCheckPlan.PrepareActions(model, request);
        var input = plan.WorkItems.First(w => w.Target.BeamId == 30).Actions.Input;
        var sample = ModelSample(model, 30, 1);
        Action apply, undo;
        switch (change)
        {
            case "node position": { double z = model.NodesElements[31].Position.Z; apply = () => model.NodesElements[31].Position.Z = z + 5; undo = () => model.NodesElements[31].Position.Z = z; break; }
            case "source property": { double alpha = section.SteelMaterial.AlfaThermalExpansion; apply = () => section.SteelMaterial.AlfaThermalExpansion = alpha * 2; undo = () => section.SteelMaterial.AlfaThermalExpansion = alpha; break; }
            case "sample forces": { double m = sample.ResultBeamForces.M1; apply = () => sample.ResultBeamForces.M1 = m + 1; undo = () => sample.ResultBeamForces.M1 = m; break; }
            default: { var steel = (SteelSection)input.Property; double alpha = steel.SteelMaterial.AlfaThermalExpansion; apply = () => steel.SteelMaterial.AlfaThermalExpansion = alpha * 2; undo = () => steel.SteelMaterial.AlfaThermalExpansion = alpha; break; }
        }
        for (int round = 0; round < 2; round++)
        {
            Assert.IsTrue(plan.IsCurrent, change);
            apply();
            Assert.IsFalse(plan.IsCurrent, change); Assert.IsFalse(plan.IsCurrentFor(request.Copy()), change);
            undo();
        }
        Assert.IsTrue(plan.IsCurrent, change); Assert.IsTrue(input.IsCurrent, change);
    }

    [TestMethod]
    public void ConcurrentPreparationAndRevalidationAgree()
    {
        var (model, request, section) = Fixture(6, 3);
        _ = section.Shape;
        var plan = BeamCheckPlan.PrepareActions(model, request);
        var expected = Inputs(plan).Select(i => i.PreparedPropertyFingerprint + i.Snapshot.SectionFingerprint + i.Snapshot.SampleFingerprint + i.Revision).ToArray();
        Parallel.For(0, 32, new ParallelOptions { MaxDegreeOfParallelism = 8 }, k =>
        {
            if (k % 2 == 0) { Assert.IsTrue(plan.IsCurrent); Assert.IsTrue(plan.IsCurrentFor(request.Copy())); return; }
            var other = BeamCheckPlan.PrepareActions(model, request);
            Assert.AreEqual(plan.ScopeFingerprint, other.ScopeFingerprint);
            CollectionAssert.AreEqual(expected, Inputs(other).Select(i => i.PreparedPropertyFingerprint + i.Snapshot.SectionFingerprint + i.Snapshot.SampleFingerprint + i.Revision).ToArray());
            Assert.IsTrue(Inputs(other).All(i => Inputs(plan).All(p => !ReferenceEquals(p.Property, i.Property))));
            Assert.IsTrue(other.IsCurrent);
        });
    }

    [TestMethod]
    public void ConcretePlanOwnsItsSectionsAndTracksRebarChanges()
    {
        var model = MixedModelFactory.Create();
        var request = new BeamCheckPlanRequest {
            Elements = new ElementSelection { Families = new[] { EntityFamily.Beam } },
            Results = new[] { new ResultSelection { Dataset = "synthetic-static", Case = "P+" } },
            SectionMechanisms = new[] { CheckMechanism.UlsBiaxialSection, CheckMechanism.Shear } };
        var plan = BeamCheckPlan.Prepare(model, request);
        var inputs = plan.WorkItems.Select(w => w.Section?.Input).Where(i => i != null).Distinct().ToArray();
        Assert.IsTrue(inputs.Length > 1);
        var source = (ReinforcedConcreteSection)model.BeamElements[inputs[0].BeamId].BeamProperty;
        foreach (var input in inputs)
        {
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { input.Section }), input.PreparedSectionFingerprint);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { input.Sample }), input.SampleFingerprint);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { model.Datasets[input.Sample.State.DatasetId] }), input.DatasetFingerprint);
            Assert.AreEqual(model.VerificationFingerprint(input.Settings), input.VerificationRevision);
        }
        Assert.AreEqual(inputs.Length, inputs.Select(i => i.Section).Distinct(InputFingerprint.ReferenceComparer.Instance).Count());
        Assert.IsTrue(plan.IsCurrent);
        double y = source.Rebars.First().Position.Y;
        source.Rebars.First().Position.Y = y + 10;
        Assert.IsFalse(plan.IsCurrent);
        source.Rebars.First().Position.Y = y;
        Assert.IsTrue(plan.IsCurrent);
        inputs[0].Section.Rebars.First().Position.Y += 20;
        Assert.AreEqual(y, source.Rebars.First().Position.Y);
        Assert.IsFalse(plan.IsCurrent); Assert.IsFalse(inputs[0].IsCurrent);
        Assert.IsTrue(inputs.Skip(1).All(i => i.IsCurrent));
    }
}
