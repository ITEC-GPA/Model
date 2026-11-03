using System;
using System.IO;
using System.Linq;
using GPC.Model.Core;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Stages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class StageAnalysisContractsTest
    {
        internal static Model Sample()
        {
            var model = new Model("construction");
            var group = model.AddGroup("structure");
            var a = new LoadCaseBase("dead"); var b = new LoadCaseBase("live");
            model.AddLoadCase(a); model.AddLoadCase(b);
            var restraint = new FreedomCase("supports"); model.AddFreedomCase(restraint);
            model.Stages.Add(new Stage(10, "erection") { Definition = new StageDefinition(2, 0), AnalysisState = new StageAnalysisState {
                BirthGeometry = StageBirthGeometry.DeformedCoordinates, MoveRestrainedNodes = true, RotateIncomingClusters = true,
                FluidLevelMm = -1200, ActiveGroupGuids = new[] { group.Guid }, ActiveFreedomCaseGuids = new[] { restraint.Guid },
                Increments = new[] {
                    new StageLoadIncrement { Name = "load", PseudoTimeSeconds = 0, LoadFactors = new[] {
                        new StageCaseFactor { CaseGuid = a.Guid, Factor = 1.35 }, new StageCaseFactor { CaseGuid = b.Guid, Factor = -.25 } },
                        FreedomFactors = new[] { new StageCaseFactor { CaseGuid = restraint.Guid, Factor = .5 } } },
                    new StageLoadIncrement { Name = "unload", PseudoTimeSeconds = 86400 } } } });
            return model;
        }

        [TestMethod] public void StageStateRoundTripsAcrossCopyDocumentAndArchiveWithoutAliasing()
        {
            var model = Sample(); var fingerprint = model.AnalysisFingerprint();
            foreach (bool document in new[] { true, false })
            using (var stream = new MemoryStream())
            {
                if (document) ModelArchive.SaveDocument(model, stream); else ModelArchive.Save(model, stream);
                stream.Position = 0; var restored = ModelArchive.Load(stream); StageAnalysisState.ValidateAll(restored);
                Assert.AreEqual(fingerprint, restored.AnalysisFingerprint());
                Assert.AreEqual(86400d, restored.Stages[10].AnalysisState.Increments[1].PseudoTimeSeconds.Value);
            }
            var copy = ModelValues.CopyValue(model);
            copy.Stages[10].AnalysisState.Increments[0].LoadFactors[0].Factor = 7;
            copy.Stages[10].AnalysisState.ActiveGroupGuids[0] = Guid.NewGuid();
            Assert.AreEqual(fingerprint, model.AnalysisFingerprint());
            Assert.AreNotEqual(fingerprint, copy.AnalysisFingerprint());
        }

        [TestMethod] public void EveryPhysicalStageFieldInvalidatesAnalysisAndSolverBinding()
        {
            Action<StageAnalysisState>[] changes = {
                s => s.BirthGeometry = StageBirthGeometry.OriginalCoordinates, s => s.MoveRestrainedNodes = false,
                s => s.RotateIncomingClusters = false, s => s.FluidLevelMm = 0, s => s.ActiveGroupGuids = new Guid[0],
                s => s.ActiveFreedomCaseGuids = new Guid[0], s => s.Increments[0].Name = "other",
                s => s.Increments[0].LoadFactors[0].Factor = 2, s => s.Increments[0].LoadFactors[0].CaseGuid = Guid.NewGuid(),
                s => s.Increments[0].FreedomFactors[0].Factor = 0, s => s.Increments[0].PseudoTimeSeconds = 1,
                s => Array.Reverse(s.Increments) };
            foreach (var change in changes)
            {
                var model = Sample(); string analysis = model.AnalysisFingerprint(), binding = model.SolverBindingFingerprint();
                change(model.Stages[10].AnalysisState);
                Assert.AreNotEqual(analysis, model.AnalysisFingerprint()); Assert.AreNotEqual(binding, model.SolverBindingFingerprint());
            }
        }

        [TestMethod] public void InvalidIdentitiesFactorsAndPseudoTimesAreRejected()
        {
            Action<StageAnalysisState>[] invalid = {
                s => s.ActiveGroupGuids[0] = Guid.NewGuid(), s => s.ActiveGroupGuids = new[] { s.ActiveGroupGuids[0], s.ActiveGroupGuids[0] },
                s => s.ActiveFreedomCaseGuids = new Guid[0], s => s.Increments[0].LoadFactors[0].Factor = double.NaN,
                s => s.Increments[0].FreedomFactors[0].Factor = double.PositiveInfinity,
                s => s.Increments[0].LoadFactors = new[] { s.Increments[0].LoadFactors[0], s.Increments[0].LoadFactors[0] },
                s => s.Increments[1].PseudoTimeSeconds = null, s => s.Increments[1].PseudoTimeSeconds = -1,
                s => s.Increments[0].PseudoTimeSeconds = 86401, s => s.FluidLevelMm = double.NaN,
                s => s.BirthGeometry = (StageBirthGeometry)99, s => s.BirthGeometry = StageBirthGeometry.ResetHistory,
                s => s.Increments = new StageLoadIncrement[0] };
            foreach (var change in invalid)
            {
                var model = Sample(); change(model.Stages[10].AnalysisState);
                Assert.ThrowsException<ArgumentException>(() => StageAnalysisState.ValidateAll(model));
            }
        }

        [TestMethod] public void ActiveRestraintsAreIndependentOfEnforcedDisplacementFactor()
        {
            var model = Sample(); var state = model.Stages[10].AnalysisState;
            state.Increments[0].FreedomFactors[0].Factor = 0; state.Validate(model);
            Assert.AreEqual(1, state.ActiveFreedomCaseGuids.Length);
            Assert.AreEqual(0, state.Increments[1].LoadFactors.Length);
            Assert.AreEqual(0, state.Increments[1].FreedomFactors.Length);
            state.ActiveFreedomCaseGuids = new Guid[0]; state.Increments[0].FreedomFactors = new StageCaseFactor[0]; state.Validate(model);
        }

        [TestMethod] public void GroupMembershipBecomesPhysicalOnlyForStagedActivation()
        {
            var model = Sample(); var node = new GPC.Model.Elements.NodeElement(new GPC.Geometry.Point3d(0, 0, 0));
            model.NodesElements.Add(node);
            string analysis = model.AnalysisFingerprint(), binding = model.SolverBindingFingerprint();
            model.AssignGroup("structure", new[] { node });
            Assert.AreNotEqual(analysis, model.AnalysisFingerprint()); Assert.AreNotEqual(binding, model.SolverBindingFingerprint());
            model.Stages[10].AnalysisState = null;
            analysis = model.AnalysisFingerprint(); binding = model.SolverBindingFingerprint();
            model.UnassignGroup("structure", new[] { node });
            Assert.AreEqual(analysis, model.AnalysisFingerprint()); Assert.AreEqual(binding, model.SolverBindingFingerprint());
        }
    }
}
