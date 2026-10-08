using GPC.Examples;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;
using GPC.Model.Sections.Concrete;

namespace UnitTest;

[TestClass]
public class AnalysisScenarioTest
{
    [TestMethod]
    public void SnapshotIsIndependentAndCannotBeReconstructedAfterResults()
    {
        var model = MixedModelFactory.Create(); var analysis = model.Analysis;
        model.NodesElements[40].Position.Z += 200;
        var first = analysis.OpenModel(); Assert.AreEqual(2000, first.NodesElements[40].Position.Z);
        first.NodesElements[40].Position.Z += 300;
        Assert.AreEqual(2000, analysis.OpenModel().NodesElements[40].Position.Z);
        Assert.AreEqual(0, first.Datasets.Count); Assert.IsFalse(first.AllElements.Any(e => e.Results.Any()));
        Assert.ThrowsException<InvalidOperationException>(() => model.CaptureAnalysis());
        Assert.AreEqual(AnalysisCompatibility.RequiresReanalysis, AnalysisCompatibilityValidator.Validate(model).Status);
    }

    [DataTestMethod]
    [DataRow(ReinforcementAnalysisRole.Unknown, AnalysisCompatibility.Unknown)]
    [DataRow(ReinforcementAnalysisRole.IncludedInAnalysis, AnalysisCompatibility.RequiresReanalysis)]
    [DataRow(ReinforcementAnalysisRole.ExcludedFromAnalysis, AnalysisCompatibility.Compatible)]
    public void ReinforcementChangesRespectTheRecordedFemAssumption(ReinforcementAnalysisRole role, AnalysisCompatibility expected)
    {
        var model = MixedModelFactory.Create(reinforcementRole: role);
        var source = model.Analysis.OpenModel(); var section = (ReinforcedConcreteSection)source.BeamElements[250].BeamProperty;
        section.Rebars.First().Position.Y += 10;
        var scenario = VerificationScenario.Create(model.Analysis.Id, "Proposed reinforcement");
        scenario.SetBeamDesign(model.BeamElements[250].Guid, section);
        var digest = scenario.Fingerprint; section.Rebars.First().Position.Y += 40;
        Assert.AreEqual(digest, scenario.Fingerprint, "Scenario setters must capture values.");
        var prepared = VerificationPreparation.Prepare(model, scenario);
        Assert.AreEqual(expected, prepared.Compatibility.Status);
        Assert.AreEqual(expected == AnalysisCompatibility.Compatible, prepared.IsCurrent);
        Assert.AreEqual(60, ((ReinforcedConcreteSection)prepared.Model.BeamElements[250].BeamProperty).Rebars.First().Position.Y);
        Assert.AreEqual(50, ((ReinforcedConcreteSection)model.BeamElements[250].BeamProperty).Rebars.First().Position.Y);
        Assert.AreEqual(model.Analysis.InputFingerprint, prepared.Model.Datasets["synthetic-static"].InputFingerprint);
    }

    [TestMethod]
    public void ChangedThicknessIsInspectableButNotPreparedForVerification()
    {
        var model = MixedModelFactory.Create(); var shell = model.Analysis.OpenModel().AreaElements.Values.First();
        shell.Assignments.PhysicalThickness += 25;
        var scenario = VerificationScenario.Create(model.Analysis.Id, "Thicker wall"); scenario.SetShellDesign(shell.Guid, shell.Assignments);
        var prepared = VerificationPreparation.Prepare(model, scenario);
        Assert.AreEqual(AnalysisCompatibility.RequiresReanalysis, prepared.Compatibility.Status);
        Assert.IsFalse(prepared.IsCurrent);
        Assert.IsTrue(prepared.Model.AllElements.Any(e => e.Results.Any()), "Historical results remain inspectable.");
        var sample = prepared.Model.BeamElements[250].Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().First();
        Assert.AreNotEqual(DataStatus.Ready, BeamActionPreparation.Prepare(prepared.Model, 250, sample, "").Status);
    }

    [TestMethod]
    public void ArchivePreservesIndependentAnalysisAndScenarioProof()
    {
        var model = MixedModelFactory.Create(); var scenario = VerificationScenario.Create(model.Analysis.Id, "Unchanged");
        var prepared = VerificationPreparation.Prepare(model, scenario);
        using var stream = new MemoryStream(); ModelArchive.Save(prepared.Model, stream); stream.Position = 0;
        var copy = ModelArchive.Load(stream);
        Assert.AreEqual(model.Analysis.Id, copy.Analysis.Id);
        Assert.AreEqual(scenario.Id, copy.VerificationContext.ScenarioId);
        Assert.IsTrue(copy.VerificationContext.IsCurrent(copy));
        Assert.AreEqual(scenario.Fingerprint, copy.VerificationScenarios[scenario.Id].Fingerprint);
        copy.NodesElements[40].Position.Z += 1;
        Assert.IsFalse(copy.VerificationContext.IsCurrent(copy));
        Assert.AreEqual(2000, copy.Analysis.OpenModel().NodesElements[40].Position.Z);
    }

    [TestMethod]
    public void ForeignScenarioOrDatasetCannotBeUsedForAutomaticVerification()
    {
        var model = MixedModelFactory.Create(); var other = MixedModelFactory.Create();
        Assert.ThrowsException<ArgumentException>(() => VerificationPreparation.Prepare(model, VerificationScenario.Create(other.Analysis.Id, "Other")));
        model.Datasets["synthetic-static"].AnalysisSnapshotId = other.Analysis.Id;
        Assert.AreEqual(AnalysisCompatibility.Unknown, AnalysisCompatibilityValidator.Validate(model).Status);
    }

    [TestMethod]
    public void PreparedGraphAndSampleMutationsInvalidateThePreparation()
    {
        var model = MixedModelFactory.Create(); var scenario = VerificationScenario.Create(model.Analysis.Id, "Unchanged");
        var prepared = VerificationPreparation.Prepare(model, scenario); Assert.IsTrue(prepared.IsCurrent);
        var sample = prepared.Model.BeamElements[250].Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().First();
        sample.ResultBeamForces.N += 1; Assert.IsFalse(prepared.IsCurrent);
        Assert.AreEqual(0, model.BeamElements[250].Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().First().ResultBeamForces.N);
    }

    [TestMethod]
    public void ConflictingSectionEditsInvalidatePreparationWithoutAnUnhandledException()
    {
        var model = MixedModelFactory.Create(); var prepared = VerificationPreparation.Prepare(model, VerificationScenario.Create(model.Analysis.Id, "Design"));
        var assignment = prepared.Model.BeamElements[250].Assignments.Sections[0]; var concrete = assignment.Section;
        assignment.Property = new GPC.Model.Sections.Steel.SteelSection(new GPC.Model.Sections.SectionRectangular(300, 100),
            GPC.Model.Data.Steel.SteelMaterialEN1993Data.S355);
        assignment.Section = concrete;
        Assert.IsFalse(prepared.IsCurrent);
        Assert.AreEqual(AnalysisCompatibility.Unknown, AnalysisCompatibilityValidator.Validate(prepared.Model).Status);
    }

    [TestMethod]
    public void LegacyArchiveRemainsReadableAndItsAnalysisIsUnknown()
    {
        var legacy = PostProcessingTest.Mixed(); using var stream = new MemoryStream();
        ModelArchive.Save(legacy, stream); stream.Position = 0; var copy = ModelArchive.Load(stream);
        Assert.IsNull(copy.Analysis); Assert.AreEqual(AnalysisCompatibility.Unknown, AnalysisCompatibilityValidator.Validate(copy).Status);
        Assert.ThrowsException<ArgumentException>(() => copy.CaptureAnalysis(ReinforcementAnalysisRole.ExcludedFromAnalysis));
    }

    [TestMethod]
    public void PrestressChangesRequireReanalysisEvenWhenOrdinaryReinforcementIsExcluded()
    {
        var model = MixedModelFactory.Create(); var section = (ReinforcedConcreteSection)model.BeamElements[250].BeamProperty;
        var bar = section.Rebars.First();
        section.AddRebars(new[] { new ReinforcedConcreteRebar(bar.RebarSection, new GPC.Geometry.Point2d(120, 120), 100) });
        Assert.AreEqual("PrestressRequiresReanalysis", AnalysisCompatibilityValidator.Validate(model).Code);
    }

    [TestMethod]
    public void SourceMetadataAndStoredScenarioEditsInvalidateTheProof()
    {
        var model = MixedModelFactory.Create(); var scenario = VerificationScenario.Create(model.Analysis.Id, "Design");
        var prepared = VerificationPreparation.Prepare(model, scenario);
        var property = (ReinforcedConcreteSection)model.Analysis.OpenModel().BeamElements[250].BeamProperty;
        property.Rebars.First().Position.Y += 10;
        prepared.Model.VerificationScenarios[scenario.Id].SetBeamDesign(model.BeamElements[250].Guid, property);
        Assert.IsFalse(prepared.IsCurrent);
        model.AnalysisSource = new AnalysisSource { Program = "Different solver" };
        Assert.AreEqual("AnalysisSourceChanged", AnalysisCompatibilityValidator.Validate(model).Code);
    }

    [TestMethod]
    public void RestoredScenarioCanBePreparedAgainAndForeignElementGuidsAreRejected()
    {
        var model = MixedModelFactory.Create(); var scenario = VerificationScenario.Create(model.Analysis.Id, "Armatura");
        var section = (ReinforcedConcreteSection)model.Analysis.OpenModel().BeamElements[250].BeamProperty;
        section.Rebars.First().Position.Y += 15; scenario.SetBeamDesign(model.BeamElements[250].Guid, section);
        model.VerificationScenarios.Add(scenario.Id, scenario);
        using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0;
        var copy = ModelArchive.Load(stream);
        var prepared = VerificationPreparation.Prepare(copy, copy.VerificationScenarios[scenario.Id]);
        Assert.IsTrue(prepared.IsCurrent);
        Assert.AreEqual(65, ((ReinforcedConcreteSection)prepared.Model.BeamElements[250].BeamProperty).Rebars.First().Position.Y);
        scenario.SetBeamDesign(Guid.NewGuid(), section);
        Assert.ThrowsException<ArgumentException>(() => VerificationPreparation.Prepare(model, scenario));
    }
}
