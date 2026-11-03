using System;
using System.IO;
using System.Linq;
using GPC.Model.Core;
using GPC.Model.Core.Diagnostics;
using GPC.Model.ElementProperties;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Sections;
using GPC.Model.Stages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>Fixes of the 3/11 review of the BridGHe/Arachne migration (REVISIONE.md, lane L1).</summary>
    [TestClass]
    public class ReviewFixesL1Test
    {
        private static void RoundTrips(Model model)
        {
            var copy = ModelValues.Copy(model);
            Assert.AreEqual(model.AnalysisFingerprint(), copy.AnalysisFingerprint());
            foreach (bool document in new[] { true, false })
            using (var stream = new MemoryStream())
            {
                if (document) ModelArchive.SaveDocument(model, stream); else ModelArchive.Save(model, stream);
                stream.Position = 0; var restored = ModelArchive.Load(stream);
                Assert.AreEqual(model.AnalysisFingerprint(), restored.AnalysisFingerprint());
            }
        }

        // B1: a removed group cited by a stage must not make the model uncopyable or unsaveable; the edit session reports it.
        [TestMethod] public void B1_RemovedStageGroupIsDiagnosedButTheModelStaysSaveable()
        {
            var model = StageAnalysisContractsTest.Sample();
            model.RemoveGroup("structure");
            RoundTrips(model);
            using (var session = new ModelEditSession(model))
            {
                var diagnostics = session.Validate();
                Assert.IsTrue(diagnostics.Any(d => d.Code == "InvalidStageAnalysisState" && d.Message.Contains("InvalidStageGroups")));
                Assert.ThrowsException<InvalidOperationException>(() => session.Commit());
                session.Draft.Stages[10].AnalysisState.ActiveGroupGuids = new Guid[0];
                Assert.AreEqual(0, session.Validate().Count(d => d.Severity == DiagnosticSeverity.Error));
                session.Commit();
            }
            Assert.ThrowsException<ArgumentException>(() => StageAnalysisState.ValidateAll(model));
            Assert.ThrowsException<InvalidOperationException>(() => model.RequireExportable());
        }

        // B1: a removed beam or case cited by a moving-load path is reported, and the model stays copyable and saveable.
        [TestMethod] public void B1_DanglingMovingLoadReferencesAreDiagnosedButTheModelStaysSaveable()
        {
            var model = MovingLoadContractsTest.Sample();
            model.MovingLoads.Paths[0].Target = GPC.Model.Actions.Moving.MovingPathTarget.Selection; model.MovingLoads.Paths[0].TargetElementGuids = new[] { model.BeamElements.Values.Single().Guid };
            model.MovingLoads.Validate(model);
            model.RemoveElement(model.BeamElements.Values.Single());
            RoundTrips(model);
            using (var session = new ModelEditSession(model))
                Assert.IsTrue(session.Validate().Any(d => d.Code == "InvalidMovingLoads" && d.Message == "UnknownMovingTargetElement"));
            Assert.ThrowsException<InvalidOperationException>(() => model.RequireExportable());
        }

        // B1: equality and hashing of the new elastic properties never run domain validation and never throw.
        [TestMethod] public void B1_ElasticPropertyEqualityNeverThrowsAndHashesByName()
        {
            var material = new Material("E210", 210000, .3, 7.85e-9, 1.2e-5);
            var a = new ElasticPlateProperty("deck", material, 200, 200); var b = new ElasticPlateProperty("deck", ModelValues.CopyValue(material), 200, 200);
            Assert.IsTrue(a.Equals(b)); Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
            a.BendingThickness = -1; a.Material = null;
            Assert.IsFalse(a.Equals(b)); Assert.IsFalse(b.Equals(a)); a.GetHashCode();
            Assert.AreNotEqual(new ElasticPlateProperty("one", material, 200, 200).GetHashCode(), new ElasticPlateProperty("two", material, 200, 200).GetHashCode());
            var model = new Model("properties"); model.AddProperty(a);
            RoundTrips(model);
            Assert.IsTrue(model.ValidateDomain().Any(d => d.Code == "InvalidElasticProperty"));
        }

        // B1: material analysis data out of range is a domain error, not a serialization failure.
        [TestMethod] public void B1_InvalidMaterialAnalysisDataDoesNotBlockCopies()
        {
            var material = new Material("E210", 210000, .3, 7.85e-9, 1.2e-5) { AnalysisData = new MaterialAnalysisData { ModalDampingRatio = .02 } };
            var property = new ElasticBeamProperty("beam", material, new SectionRectangular(200, 100));
            material.AnalysisData.ModalDampingRatio = -1;
            var model = new Model("material"); model.AddProperty(property);
            RoundTrips(model);
            Assert.IsTrue(model.ValidateDomain().Any(d => d.Code == "InvalidElasticProperty" && d.Message.Contains("InvalidMaterialAnalysisValue")));
            Assert.AreNotEqual(new MaterialAnalysisData { ModalDampingRatio = .05 }.GetHashCode(), new MaterialAnalysisData().GetHashCode());
            Assert.AreEqual(new MaterialAnalysisData { ModalDampingRatio = .05 }.GetHashCode(), new MaterialAnalysisData { ModalDampingRatio = .05 }.GetHashCode());
        }

        // B9: stages may be absent in legacy graphs.
        [TestMethod] public void B9_StageHasDataToleratesMissingStages()
        {
            Assert.IsFalse(StageAnalysisState.HasData(null));
            var model = new Model("empty");
            typeof(Model).GetField("_stages", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(model, null);
            Assert.IsFalse(StageAnalysisState.HasData(model));
            StageAnalysisState.ValidateAll(model);
        }
    }
}
