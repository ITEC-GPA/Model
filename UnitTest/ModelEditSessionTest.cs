using GPC.Examples;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Sections.Concrete;

namespace UnitTest;

[TestClass]
public class ModelEditSessionTest
{
    private static ReinforcedConcreteSection Section(Model model) => (ReinforcedConcreteSection)model.BeamElements[250].BeamProperty;

    [TestMethod]
    public void EmptyDocumentCanBeEditedBeforeAnyAnalysisHasBeenImported()
    {
        var source = new Model("New document"); using var edit = new ModelEditSession(source);
        edit.Draft.Name = "Renamed document";
        var result = edit.Commit();
        Assert.AreEqual("New document", source.Name);
        Assert.AreEqual("Renamed document", result.Model.Name);
        Assert.IsFalse(result.AnalysisChanged); Assert.IsFalse(result.VerificationChanged);
        Assert.AreEqual(AnalysisCompatibility.Unknown, result.AnalysisCompatibilityAtCommit.Status);
    }

    [DataTestMethod]
    [DataRow(ReinforcementAnalysisRole.ExcludedFromAnalysis, AnalysisCompatibility.Compatible)]
    [DataRow(ReinforcementAnalysisRole.IncludedInAnalysis, AnalysisCompatibility.RequiresReanalysis)]
    [DataRow(ReinforcementAnalysisRole.Unknown, AnalysisCompatibility.Unknown)]
    public void CommitExposesCompatibilityAccordingToTheRecordedReinforcementRole(ReinforcementAnalysisRole role, AnalysisCompatibility expected)
    {
        var source = MixedModelFactory.Create(reinforcementRole: role); using var edit = new ModelEditSession(source);
        Section(edit.Draft).Rebars.First().Position.Y += 10;
        var result = edit.Commit();
        Assert.IsFalse(result.AnalysisChanged, "The geometric FEM digest alone does not authorize reuse of the analysis.");
        Assert.IsTrue(result.VerificationChanged);
        Assert.AreEqual(expected, result.AnalysisCompatibilityAtCommit.Status);
    }

    [TestMethod]
    public void UnchangedCommitPreservesIdentityAndInternalAliasesWithoutSharingObjects()
    {
        var source = MixedModelFactory.Create();
        using var edit = new ModelEditSession(source);
        var result = edit.Commit(); var copy = result.Model;
        Assert.IsFalse(result.AnalysisChanged); Assert.IsFalse(result.VerificationChanged);
        Assert.AreEqual(result.Before.Document, ModelRevision.Capture(source).Document);
        Assert.AreEqual(result.After.Document, ModelRevision.Capture(copy).Document);
        Assert.AreEqual(source.BeamElements[250].Guid, copy.BeamElements[250].Guid);
        Assert.AreNotSame(source.BeamElements[250], copy.BeamElements[250]);
        Assert.AreSame(copy.NodesElements[10], copy.BeamElements[250].NodeI);
        Assert.AreSame(Section(copy), copy.BeamElements[250].Assignments.Sections[0].Section);
        Assert.AreNotSame(Section(source).Rebars.First().Position, Section(copy).Rebars.First().Position);
        Assert.AreEqual(source.Groups.Count, copy.Groups.Count);
        Assert.AreEqual(source.Datasets.Count, copy.Datasets.Count);
        Assert.AreEqual(source.Analysis.Id, copy.Analysis.Id);
        Assert.ThrowsException<InvalidOperationException>(() => edit.Commit());
        Assert.ThrowsException<InvalidOperationException>(() => edit.Validate());
    }

    [TestMethod]
    public void DesignEditInvalidatesVerificationOnlyAndPublishedModelIsIndependentOfDraft()
    {
        var source = MixedModelFactory.Create();
        using var edit = new ModelEditSession(source);
        Section(edit.Draft).Rebars.First().Position.Y += 10;
        Assert.AreEqual(50, Section(source).Rebars.First().Position.Y);
        var result = edit.Commit();
        Assert.IsFalse(result.AnalysisChanged); Assert.IsTrue(result.VerificationChanged);
        Assert.AreEqual(AnalysisCompatibility.Compatible, AnalysisCompatibilityValidator.Validate(result.Model).Status);
        Assert.AreEqual(60, Section(result.Model).Rebars.First().Position.Y);
        Section(edit.Draft).Rebars.First().Position.Y += 20;
        Assert.AreEqual(60, Section(result.Model).Rebars.First().Position.Y);
        Assert.AreEqual(50, Section(source).Rebars.First().Position.Y);
    }

    [TestMethod]
    public void GeometryEditRetainsHistoricalResultsButRequiresNewAnalysis()
    {
        var source = MixedModelFactory.Create(); var originalAnalysis = source.Datasets["synthetic-static"].InputFingerprint;
        using var edit = new ModelEditSession(source);
        edit.Draft.NodesElements[40].Position.Z += 100;
        var result = edit.Commit();
        Assert.IsTrue(result.AnalysisChanged); Assert.IsTrue(result.VerificationChanged);
        Assert.AreEqual(AnalysisCompatibility.RequiresReanalysis, AnalysisCompatibilityValidator.Validate(result.Model).Status);
        Assert.AreEqual(originalAnalysis, result.Model.Datasets["synthetic-static"].InputFingerprint);
        Assert.IsTrue(result.Model.BeamElements[250].Results.Any());
        Assert.AreEqual(2000, source.NodesElements[40].Position.Z);
    }

    [DataTestMethod]
    [DataRow("node")]
    [DataRow("bar")]
    [DataRow("dataset")]
    [DataRow("group")]
    public void ConcurrentLegacyEditsAreDetectedInsteadOfOverwritten(string change)
    {
        var source = MixedModelFactory.Create(); using var edit = new ModelEditSession(source);
        switch (change)
        {
            case "node": source.NodesElements[10].Position.X += 1; break;
            case "bar": Section(source).Rebars.First().Position.Y += 1; break;
            case "dataset": source.Datasets["synthetic-static"].Program = "Changed"; break;
            case "group": source.AssignGroup("Wall", new[] { source.BeamElements[250] }); break;
        }
        StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => edit.Commit()).Message, "ModelEditConflict");
    }

    [TestMethod]
    public void InvalidMemberCanBeRepairedInDraftWithoutPublishingIt()
    {
        var source = MixedModelFactory.Create(); using var edit = new ModelEditSession(source);
        edit.Draft.PhysicalMembers.Add("T1", new PhysicalMemberDefinition("T1", new[] { new BeamMemberPart(999, true) }, "NodeToNode"));
        Assert.IsTrue(edit.Validate().Any(d => d.Code == "InvalidPhysicalMember"));
        StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => edit.Commit()).Message, "InvalidModelEdit");
        Assert.AreEqual(0, source.PhysicalMembers.Count);
        edit.Draft.PhysicalMembers["T1"] = new PhysicalMemberDefinition("T1", new[] { new BeamMemberPart(250, true) }, "NodeToNode");
        var result = edit.Commit();
        Assert.IsFalse(result.AnalysisChanged); Assert.IsTrue(result.VerificationChanged);
        Assert.AreEqual(2000, new PhysicalMemberGeometry(result.Model, result.Model.PhysicalMembers["T1"]).Length);
    }

    [TestMethod]
    public void InvalidConnectivityPreventsCommitAndDisposalCancelsTheEdit()
    {
        var source = MixedModelFactory.Create(); var edit = new ModelEditSession(source);
        edit.Draft.NodesElements.Remove(40);
        Assert.IsTrue(edit.Validate().Any(d => d.Severity == DiagnosticSeverity.Error));
        Assert.ThrowsException<InvalidOperationException>(() => edit.Commit());
        edit.Dispose();
        Assert.ThrowsException<InvalidOperationException>(() => edit.Commit());
        Assert.IsTrue(source.NodesElements.ContainsKey(40));
    }
}
