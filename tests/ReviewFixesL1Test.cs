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
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Structure.Assignments;
using GPC.Model.Structure.Topology;
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

        private static Model Plate(out AreaElement plate, out LoadCaseBase lc)
        {
            var nodes = new[] { new NodeElement(new Point3d(0, 0, 0)), new NodeElement(new Point3d(100, 0, 0)), new NodeElement(new Point3d(100, 100, 0)), new NodeElement(new Point3d(0, 100, 0)) };
            plate = new AreaElement(null, new ElasticPlateProperty("plate", new Material("E", 210000, .3, 0, 0), 10, 10)); plate.ConnectNodes(nodes);
            lc = new LoadCaseBase("pressure");
            plate.Loads.Add(ShellFacePressure.Normal(lc, ShellFace.PositiveNormal, .001));
            var model = ModelAssembly.Assemble(new Element[] { plate }, Guid.NewGuid());
            plate = model.AreaElements.Values.Single(); lc = model.LoadCases.Values.Single(); return model;
        }

        // B2: face and follower pressures are element loads, the single place that load consumers read.
        [TestMethod] public void B2_FacePressureIsAnElementLoadWithItsOwnSemantics()
        {
            var model = Plate(out var plate, out var lc); string fingerprint = model.AnalysisFingerprint();
            Assert.IsTrue(PhysicalAssignments.UsesSurfacePressureContract(model));
            var pressure = plate.Loads.Values.OfType<ShellFacePressure>().Single();
            Assert.IsTrue(pressure.IsFollower); Assert.AreSame(lc, pressure.LoadCase);
            RoundTrips(model);
            var copy = ModelValues.Copy(model); var copied = copy.AreaElements.Values.Single().Loads.Values.OfType<ShellFacePressure>().Single();
            Assert.AreEqual(.001, copied.NormalPressure.Value); Assert.AreEqual(ShellFace.PositiveNormal, copied.Face);
            copied.NormalPressure = .002; Assert.AreNotEqual(fingerprint, copy.AnalysisFingerprint()); Assert.AreEqual(fingerprint, model.AnalysisFingerprint());
            copied.GlobalPressure = new Vector3d(0, 0, 1);
            Assert.IsTrue(copy.ValidateAssignments().Any(d => d.Code == "InvalidShellFacePressure"));
            var node = copy.NodesElements.Values.First(); node.Loads.Add(ShellFacePressure.Normal(copy.LoadCases.Values.Single(), ShellFace.NegativeNormal, .001));
            Assert.IsTrue(copy.ValidateAssignments().Any(d => d.Code == "ShellFacePressureRequiresShell"));
            Assert.ThrowsException<ArgumentException>(() => ShellFacePressure.Global(lc, ShellFace.PositiveNormal, new Vector3d(double.NaN, 0, 0)));
            Assert.ThrowsException<NotSupportedException>(() => ModelOrientation.ReverseShell(model, plate.Id));
        }

        // B2: one activation model for stages; the complete sets are derived from either representation.
        [TestMethod] public void B2_StageActivationHasOneModel()
        {
            var model = StageAnalysisContractsTest.Sample(); var stage = model.Stages[10];
            stage.Definition = new StageDefinition(2, 0, groupChanges: new[] { new StageGroupChange("structure", StageGroupKind.Structure, StageGroupAction.Activate) });
            Assert.AreEqual("ConflictingStageActivation", Assert.ThrowsException<ArgumentException>(() => StageAnalysisState.ValidateAll(model)).Message);
            using (var session = new ModelEditSession(model)) Assert.IsTrue(session.Validate().Any(d => d.Code == "ConflictingStageActivation"));
            model.Stages.Add(new Stage(11, "second") { Definition = new StageDefinition(3, 0) });
            stage.Definition = new StageDefinition(2, 0);
            Assert.AreEqual("MixedStageActivation", Assert.ThrowsException<ArgumentException>(() => StageAnalysisState.ValidateAll(model)).Message);
            var group = model.Groups["structure"]; var other = model.AddGroup("deck");
            stage.AnalysisState = null;
            stage.Definition = new StageDefinition(2, 0, groupChanges: new[] { new StageGroupChange("structure", StageGroupKind.Structure, StageGroupAction.Activate),
                new StageGroupChange("deck", StageGroupKind.Load, StageGroupAction.Activate) });
            model.Stages[11].Definition = new StageDefinition(3, 0, groupChanges: new[] { new StageGroupChange("deck", StageGroupKind.Structure, StageGroupAction.Activate),
                new StageGroupChange("structure", StageGroupKind.Structure, StageGroupAction.Deactivate) });
            var sets = StageAnalysisState.ActiveGroupSets(model);
            CollectionAssert.AreEqual(new[] { group.Guid }, sets[0].Value); CollectionAssert.AreEqual(new[] { other.Guid }, sets[1].Value);
        }

        // B3: one registered membrane property shared by the elements, instead of a material copy on every element.
        [TestMethod] public void B3_MembraneIsASharedRegisteredPlateProperty()
        {
            var fabric = new MembranePlateProperty("PVC") { MassPerArea = 1.05e-9, StiffnessWarp = 1000, StiffnessWeft = 500, ShearStiffness = 100, NuWarpWeft = .2 };
            var nodes = new[] { new NodeElement(new Point3d(0, 0, 0)), new NodeElement(new Point3d(100, 0, 0)), new NodeElement(new Point3d(100, 100, 0)), new NodeElement(new Point3d(0, 100, 0)) };
            var a = new AreaElement(null, fabric); a.ConnectNodes(new[] { nodes[0], nodes[1], nodes[2] });
            var b = new AreaElement(null, fabric); b.ConnectNodes(new[] { nodes[0], nodes[2], nodes[3] });
            foreach (var e in new[] { a, b }) e.Assignments.LayerAxes = CoordinateSystem.Global;
            var model = ModelAssembly.Assemble(new Element[] { a, b }, Guid.NewGuid());
            Assert.AreEqual(1, model.PlateProperties.Count);
            var shells = model.AreaElements.Values.ToArray();
            Assert.AreSame(shells[0].PlateProperty, shells[1].PlateProperty); Assert.AreSame(model.PlateProperties["PVC"], shells[0].PlateProperty);
            Assert.IsTrue(PhysicalAssignments.UsesExtendedExportContract(model));
            RoundTrips(model);
            var copy = ModelValues.Copy(model); var copied = copy.AreaElements.Values.ToArray();
            Assert.AreSame(copied[0].PlateProperty, copied[1].PlateProperty); Assert.AreNotSame(shells[0].PlateProperty, copied[0].PlateProperty);
            string fingerprint = model.AnalysisFingerprint();
            ((MembranePlateProperty)copied[0].PlateProperty).StiffnessWarp = 2000;
            Assert.AreNotEqual(fingerprint, copy.AnalysisFingerprint()); Assert.AreEqual(fingerprint, model.AnalysisFingerprint());
            GPC.Converter.Straus7.Straus7ModelWritePlan.Prepare(model);
            Assert.AreEqual("MissingPhysicalThickness", Assert.ThrowsException<ArgumentException>(() => ShellThickness.From(shells[0])).Message);
            ((MembranePlateProperty)copied[0].PlateProperty).NuWarpWeft = 5;
            Assert.IsTrue(copy.ValidateDomain().Any(d => d.Code == "InvalidElasticProperty" && d.Message.Contains("NonPositiveDefiniteMembraneCompliance")));
            Assert.IsTrue(copy.ValidateAssignments().Any(d => d.Severity == DiagnosticSeverity.Error));
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
