using GPC.Examples;
using GPC.Model.Core;
using GPC.Model.ElementProperties;
using GPC.Model.Models;
using GPC.Model.PostProcessing;
using GPC.Model.Sections.Concrete;
using GPC.Model.Structure;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Model.Core.Tests;

[TestClass]
public class CoreWithoutPersistenceTest
{
    private static void RequireNoPersistence()
    {
        Assert.IsFalse(File.Exists(Path.Combine(AppContext.BaseDirectory, "GPC.Model.Persistence.dll")), "This test host must have no I/O adapter.");
        Assert.IsFalse(typeof(GPC.Model.Models.Model).Assembly.GetReferencedAssemblies().Any(a => a.Name == "GPC.Model.Persistence"));
        Assert.IsFalse(AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "GPC.Model.Persistence"));
        Assert.IsFalse(typeof(GPC.Model.Models.Model).Assembly.GetTypes().Any(t => t.Namespace == "GPC.Model.Persistence"));
    }
    [TestMethod]
    public void CaptureCopyAndEditWorkWithoutTheIoAssembly()
    {
        RequireNoPersistence();
        var model = MixedModelFactory.Create();
        var snapshot = model.Analysis.OpenModel();
        Assert.AreEqual(model.Guid, snapshot.Guid);
        Assert.AreEqual(0, snapshot.Datasets.Count);
        var copy = ModelValues.Copy(model);
        Assert.AreNotSame(model.NodesElements[10], copy.NodesElements[10]);
        Assert.AreSame(copy.NodesElements[10], copy.BeamElements[250].NodeI);
        using var edit = new ModelEditSession(copy);
        edit.Draft.PhysicalSurfaces.Add("wall", new PhysicalSurfaceDefinition("wall", new[] { 1090, 1130 }));
        var published = edit.Commit();
        Assert.IsFalse(published.AnalysisChanged); Assert.IsTrue(published.VerificationChanged);
        Assert.AreEqual(AnalysisCompatibility.Compatible, published.AnalysisCompatibilityAtCommit.Status);
        RequireNoPersistence();
    }
    [TestMethod]
    public void ScenarioAndShellPreparationNeedOnlyDomainData()
    {
        RequireNoPersistence();
        var model = MixedModelFactory.Create(reinforcementRole: ReinforcementAnalysisRole.ExcludedFromAnalysis);
        var section = (ReinforcedConcretePlateSection)PlateSections.UpgradeLegacy(ModelValues.Copy(model).AreaElements[1090]);
        var scenario = VerificationScenario.Create(model.Analysis.Id, "independent domain");
        scenario.SetShellSection(model.AreaElements[1090].Guid, section);
        var scenarioModel = VerificationPreparation.Prepare(model, scenario).Model;
        var request = new PreparationRequest {
            Selection = new ElementSelection { Families = new[] { EntityFamily.Shell }, Groups = new[] { "Wall" } },
            Results = new[] { new ResultSelection { Dataset = "synthetic-static", Case = "P+" } } };
        var prepared = ModelPreparation.PrepareShellActions(scenarioModel, request, ShellInputAxes.Reinforcement);
        Assert.AreEqual(2, prepared.Samples.Count);
        Assert.IsTrue(prepared.Samples.All(s => s.Shell.Status == DataStatus.Ready && s.Shell.Input.IsCurrent));
        section.RebarLayers[0].Pitch += 1;
        Assert.IsTrue(prepared.Samples.All(s => s.Shell.Input.IsCurrent));
        RequireNoPersistence();
    }
    [TestMethod]
    public void ClosedValueVocabularyRejectsUnregisteredObjects()
    {
        RequireNoPersistence();
        Assert.ThrowsException<System.Runtime.Serialization.SerializationException>(() => ModelValues.CopyValue(new UnexpectedValue()));
        Assert.IsNull(ModelValues.CopyValue<UnexpectedValue>(null!));
    }
    [Serializable] private sealed class UnexpectedValue { public double Value = 1; }
}
