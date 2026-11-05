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
using GPC.Model.Analysis;
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

        // B5: orthotropy only on the generic analysis material, explicit check, and beams reject it.
        [TestMethod] public void B5_OrthotropyIsExplicitAndConfinedToTheGenericMaterial()
        {
            var tensor = new OrthotropicElasticity { E1 = 1000, E2 = 500, E3 = 100, G12 = 100, G23 = 50, G31 = 50, Nu12 = .2, Nu23 = .1, Nu31 = .05 };
            var generic = new Material("orthotropic", 1000, .2, 1e-9, 0);
            Assert.IsFalse(generic.IsOrthotropic); generic.Orthotropy = tensor; Assert.IsTrue(generic.IsOrthotropic);
            Assert.ThrowsException<NotSupportedException>(() => generic.E);
            var steel = new GPC.Model.Materials.SteelMaterial("S355", 210000, 355, 510);
            Assert.AreEqual("OrthotropyRequiresGenericElasticMaterial: SteelMaterial", Assert.ThrowsException<NotSupportedException>(() => steel.Orthotropy = tensor).Message);
            Assert.IsFalse(steel.IsOrthotropic); Assert.IsTrue(steel.E > 0);
            Assert.AreEqual("OrthotropicBeamMaterialNotSupported", Assert.ThrowsException<ArgumentException>(() => new ElasticBeamProperty("beam", generic, new SectionRectangular(200, 100))).Message);
            var copy = ModelValues.CopyValue(generic); Assert.IsTrue(copy.IsOrthotropic); Assert.AreEqual(500, copy.Orthotropy.E2);
        }

        // B5: the edit session validates physical members as straight by default; curved members are an explicit opt-in.
        [TestMethod] public void B5_EditSessionRequiresStraightPhysicalMembersUnlessCurvedAreRequested()
        {
            var model = new Model("members"); var a = new NodeElement(new Point3d(0, 0, 0)); var b = new NodeElement(new Point3d(3000, 0, 0)); var c = new NodeElement(new Point3d(3000, 4000, 0));
            foreach (var node in new[] { a, b, c }) model.NodesElements.Add(node);
            BeamElement Beam(NodeElement i, NodeElement j) { var beam = new BeamElement(i.Position, j.Position, null); beam.ConnectNodes(i, j); beam.Assignments.Formulation = BeamFormulation.StraightTwoNode; return beam; }
            var first = Beam(a, b); var second = Beam(b, c); model.BeamElements.Add(first); model.BeamElements.Add(second);
            var member = new GPC.Model.Structure.Members.PhysicalMemberDefinition("corner", new[] { new GPC.Model.Structure.Members.BeamMemberPart(first.Id, true), new GPC.Model.Structure.Members.BeamMemberPart(second.Id, true) }, "NodeToNode");
            model.PhysicalMembers.Add(member.Id, member);
            using (var session = new ModelEditSession(model)) Assert.IsTrue(session.Validate().Any(d => d.Code == "InvalidPhysicalMember"));
            using (var session = new ModelEditSession(model, curvedPhysicalMembers: true)) Assert.IsFalse(session.Validate().Any(d => d.Code == "InvalidPhysicalMember"));
        }

        private static BeamElement LoadedBeam(NodeElement a, NodeElement b, LoadCaseBase lc, BeamProperty property = null)
        {
            var beam = new BeamElement(a.Position, b.Position, property); beam.ConnectNodes(a, b); beam.Assignments.Formulation = BeamFormulation.StraightTwoNode;
            beam.Loads.Add(new PointLoad(0, 0, -1000, 0, 0, 0, a.Position, lc)); return beam;
        }

        // B6: equal content never merges two cases or properties with different GUIDs.
        [TestMethod] public void B6_AssemblyRejectsHomonymousCasesAndPropertiesWithDifferentGuids()
        {
            var a = new NodeElement(new Point3d(0, 0, 0)); var b = new NodeElement(new Point3d(1000, 0, 0)); var c = new NodeElement(new Point3d(2000, 0, 0));
            var first = LoadedBeam(a, b, new LoadCaseBase(Guid.NewGuid(), "dead")); var second = LoadedBeam(b, c, new LoadCaseBase(Guid.NewGuid(), "dead"));
            Assert.AreEqual("LoadCaseIdentityConflict: dead", Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new Element[] { first, second }, Guid.NewGuid())).Message);
            var shared = new LoadCaseBase(Guid.NewGuid(), "dead"); var copy = ModelValues.CopyValue(shared);
            first = LoadedBeam(a, b, shared); second = LoadedBeam(b, c, copy);
            var model = ModelAssembly.Assemble(new Element[] { first, second }, Guid.NewGuid());
            Assert.AreEqual(shared.Guid, model.LoadCases.Values.Single().Guid);
            var material = new Material("E", 210000, .3, 0, 0);
            first = LoadedBeam(a, b, shared, new ElasticBeamProperty("rect", material, new SectionRectangular(200, 100)));
            second = LoadedBeam(b, c, shared, new ElasticBeamProperty("rect", material, new SectionRectangular(200, 100)));
            Assert.AreEqual("PropertyIdentityConflict: rect", Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new Element[] { first, second }, Guid.NewGuid())).Message);
        }

        // B6: two copies of one element citing cases with different GUIDs are a conflict, whatever the input order.
        [TestMethod] public void B6_DuplicateElementsCitingDifferentCaseIdentitiesAreRejectedInAnyOrder()
        {
            var a = new NodeElement(new Point3d(0, 0, 0)); var b = new NodeElement(new Point3d(1000, 0, 0));
            var beam = LoadedBeam(a, b, new LoadCaseBase(Guid.NewGuid(), "dead"));
            var twin = ModelValues.CopyValue(beam); twin.Loads.Values.Single().LoadCase = new LoadCaseBase(Guid.NewGuid(), "dead");
            foreach (var input in new[] { new Element[] { beam, twin }, new Element[] { twin, beam } })
                StringAssert.StartsWith(Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(input, Guid.NewGuid())).Message, "ConflictingEntityGuid");
            var same = ModelValues.CopyValue(beam);
            string forward = ModelAssembly.Assemble(new Element[] { beam, same }, new Guid("11111111-1111-1111-1111-111111111111")).AnalysisFingerprint();
            Assert.AreEqual(forward, ModelAssembly.Assemble(new Element[] { same, beam }, new Guid("11111111-1111-1111-1111-111111111111")).AnalysisFingerprint());
        }

        private static Model Cable(FormFindingMethod method)
        {
            var a = new NodeElement(new Point3d(0, 0, 0)); var b = new NodeElement(new Point3d(1000, 0, 0));
            var beam = new BeamElement(a.Position, b.Position, null); beam.ConnectNodes(a, b); beam.Assignments.Formulation = BeamFormulation.Cable;
            var model = ModelAssembly.Assemble(new[] { beam }, Guid.NewGuid());
            model.FormFinding = new FormFindingDefinition { Method = method };
            return model;
        }

        // B7: the linear method keeps fixed length, axial force and node weights and ignores them with a warning (legacy Arachne).
        [TestMethod] public void B7_LinearFormFindingIgnoresIterativeInputsWithWarnings()
        {
            var model = Cable(FormFindingMethod.LinearForceDensity); var beam = model.BeamElements.Values.Single();
            var force = new FormFindingGoal { BeamGuid = beam.Guid, Kind = FormFindingGoalKind.AxialForce, Target = 100 };
            model.FormFinding.Goals.Add(new FormFindingGoal { BeamGuid = beam.Guid, Kind = FormFindingGoalKind.ForceDensity, Target = 1 });
            model.FormFinding.Goals.Add(new FormFindingGoal { BeamGuid = beam.Guid, Kind = FormFindingGoalKind.Length, Target = 900 });
            model.FormFinding.Goals.Add(force);
            model.FormFinding.CoordinateGoals.Add(new FormFindingCoordinateGoal { NodeGuid = model.NodesElements.Values.First().Guid, Weights = new Vector3d(0, 0, 5) });
            model.FormFinding.Validate(model);
            CollectionAssert.AreEquivalent(new[] { "FormFindingFixedLengthIgnored", "FormFindingFixedAxialForceIgnored", "FormFindingCoordinateWeightsIgnored" },
                model.FormFinding.IgnoredInputs().Select(d => d.Code).ToArray());
            Assert.IsTrue(model.FormFinding.IgnoredInputs().All(d => d.Severity == DiagnosticSeverity.Warning));
            Assert.IsFalse(model.FormFinding.Uses(force)); Assert.IsTrue(model.FormFinding.Uses(model.FormFinding.Goals[0]));
            Assert.IsTrue(model.ValidateDomain().Any(d => d.Code == "FormFindingFixedLengthIgnored"));
            Assert.IsFalse(model.ValidateDomain().Any(d => d.Severity == DiagnosticSeverity.Error));
            model.FormFinding.Method = FormFindingMethod.NonlinearForceDensity;
            Assert.AreEqual(0, model.FormFinding.IgnoredInputs().Count); Assert.IsTrue(model.FormFinding.Uses(force));
        }

        // B7, changed by the user on 4/11/2026 («cable e fixed axial force possono convivere»): a cable force and a fixed axial force on
        // one beam coexist, as in Arachne 7b1108d (the fixed axial force replaces the axial target of the cable; lane L5). With every method;
        // the linear one keeps its warning that it ignores the fixed axial force.
        [TestMethod] public void B7_CableForceAndAxialForceOnOneBeamCoexist()
        {
            foreach (var method in new[] { FormFindingMethod.LinearForceDensity, FormFindingMethod.SteppedLinearForceDensity, FormFindingMethod.NonlinearForceDensity })
            {
                var model = Cable(method); var beam = model.BeamElements.Values.Single();
                model.FormFinding.Goals.Add(new FormFindingGoal { BeamGuid = beam.Guid, Kind = FormFindingGoalKind.CableForce, Target = 100 });
                model.FormFinding.Validate(model);
                model.FormFinding.Goals.Add(new FormFindingGoal { BeamGuid = beam.Guid, Kind = FormFindingGoalKind.AxialForce, Target = 150, Weight = 2 });
                model.FormFinding.Validate(model);
                Assert.IsFalse(model.ValidateDomain().Any(d => d.Severity == DiagnosticSeverity.Error), method.ToString());
                Assert.AreEqual(method == FormFindingMethod.LinearForceDensity, model.FormFinding.IgnoredInputs().Any(d => d.Code == "FormFindingFixedAxialForceIgnored"), method.ToString());
                // still one goal of a kind per beam: a second cable force is refused
                model.FormFinding.Goals.Add(new FormFindingGoal { BeamGuid = beam.Guid, Kind = FormFindingGoalKind.CableForce, Target = 120 });
                Assert.AreEqual("InvalidOrDuplicateFormFindingGoal", Assert.ThrowsException<ArgumentException>(() => model.FormFinding.Validate(model)).Message);
            }
        }

        // B7: the legacy BridGHe load-patch shape (int[4] padded with zeros, weights on automatic kinds) is accepted.
        [TestMethod] public void B7_LoadPatchAcceptsZeroPaddedEdgesAndIgnoresAutomaticWeights()
        {
            var patch = new LoadPatchDistribution { Kind = LoadPatchDistributionKind.AutomaticTwoEdges, SelectedEdges = new[] { 1, 3, 0, 0 }, EdgeWeights = new[] { .5, 0, .5, 0 } };
            patch.Validate(4);
            CollectionAssert.AreEqual(new[] { 1, 3 }, patch.Edges); Assert.IsTrue(patch.IgnoresWeights); CollectionAssert.AreEqual(new double[4], patch.EffectiveWeights);
            patch.Kind = LoadPatchDistributionKind.Manual; Assert.IsFalse(patch.IgnoresWeights); CollectionAssert.AreEqual(new[] { .5, 0, .5, 0 }, patch.EffectiveWeights); patch.Validate(4);
            patch.SelectedEdges = new[] { 1, 1, 0, 0 }; Assert.ThrowsException<ArgumentException>(() => patch.Validate(4));
            patch.Kind = LoadPatchDistributionKind.AutomaticTwoEdges; patch.SelectedEdges = new[] { 1, 3, 0, 0 };
            var nodes = new[] { new NodeElement(new Point3d(0, 0, 0)), new NodeElement(new Point3d(100, 0, 0)), new NodeElement(new Point3d(100, 100, 0)), new NodeElement(new Point3d(0, 100, 0)) };
            var shell = new AreaElement(null, new ElasticPlateProperty("patch", null, 0, 0, ElasticPlateFormulation.LoadPatch)); shell.ConnectNodes(nodes);
            shell.Assignments.Physical = new ShellPhysicalData { LoadPatch = patch };
            var model = ModelAssembly.Assemble(new Element[] { shell }, Guid.NewGuid());
            var diagnostics = model.ValidateAssignments();
            Assert.IsTrue(diagnostics.Any(d => d.Code == "LoadPatchWeightsIgnored" && d.Severity == DiagnosticSeverity.Warning));
            Assert.IsFalse(diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        }

        // A4: form-found shapes carry their own semantics, which structural consumers reject.
        [TestMethod] public void A4_FormFindingSemanticsIsNotAStructuralState()
        {
            Assert.AreEqual(7, (int)AnalysisSemantics.FormFinding); // appended: the historical values keep their numbers
            Assert.IsFalse(AnalysisSemantics.FormFinding.IsStructuralState()); Assert.IsFalse(AnalysisSemantics.Unknown.IsStructuralState());
            foreach (var s in new[] { AnalysisSemantics.LinearStatic, AnalysisSemantics.NonlinearStatic, AnalysisSemantics.ConcomitantEnvelopeState }) Assert.IsTrue(s.IsStructuralState());
            Assert.AreEqual("FormFindingResultIsNotStructural", AnalysisSemanticsRules.NonStructuralCode(AnalysisSemantics.FormFinding));
            Assert.AreEqual("NonConcomitantState", AnalysisSemanticsRules.NonStructuralCode(AnalysisSemantics.Modal));
            var dataset = new AnalysisDataset { Id = "form-finding", Semantics = AnalysisSemantics.FormFinding, NormalizedUnits = "N,mm,s,tonne" };
            Assert.AreEqual(AnalysisSemantics.FormFinding, ModelValues.CopyValue(dataset).Semantics);
        }

        // B9: physical validations that were missing.
        [TestMethod] public void B9_SupportsTemperaturesAndMemberSegmentsAreValidated()
        {
            var model = PhysicalAssignmentsTest.Sample(); var shell = model.AreaElements.Values.Single(); var node = model.NodesElements.Values.First();
            PhysicalAssignments.Validate(shell); PhysicalAssignments.Validate(node);
            shell.Assignments.Physical.Supports.Add(new ShellDistributedSupport { Stiffness = .1, Gap = -1, Face = ShellFace.PositiveNormal, Behavior = DistributedSpringBehavior.CompressionOnly });
            Assert.AreEqual("NegativePhysicalAssignment", Assert.ThrowsException<ArgumentException>(() => PhysicalAssignments.Validate(shell)).Message);
            shell.Assignments.Physical.Supports[shell.Assignments.Physical.Supports.Count - 1].Gap = 0;
            shell.Assignments.Physical.Supports[shell.Assignments.Physical.Supports.Count - 1].Stiffness = 0;
            Assert.AreEqual("NonPositivePhysicalAssignment", Assert.ThrowsException<ArgumentException>(() => PhysicalAssignments.Validate(shell)).Message);
            shell.Assignments.Physical.Supports.RemoveAt(shell.Assignments.Physical.Supports.Count - 1); PhysicalAssignments.Validate(shell);
            node.Assignments.Physical = node.Assignments.Physical ?? new NodePhysicalData();
            node.Assignments.Physical.Temperatures.Add(new NodalTemperatureAssignment { Case = new LoadCaseBase("cold"), Kind = NodalTemperatureKind.Prescribed, Celsius = -274 });
            Assert.AreEqual("TemperatureBelowAbsoluteZero", Assert.ThrowsException<ArgumentException>(() => PhysicalAssignments.Validate(node)).Message);
            // A zero-length part is refused (by the reference line, before the new segment guard).
            var members = new Model("members"); var a = new NodeElement(new Point3d(0, 0, 0)); var b = new NodeElement(new Point3d(0, 0, 0));
            members.NodesElements.Add(a); members.NodesElements.Add(b);
            var beam = new BeamElement(a.Position, b.Position, null); beam.ConnectNodes(a, b); beam.Assignments.Formulation = BeamFormulation.StraightTwoNode; members.BeamElements.Add(beam);
            var member = new GPC.Model.Structure.Members.PhysicalMemberDefinition("point", new[] { new GPC.Model.Structure.Members.BeamMemberPart(beam.Id, true) }, "NodeToNode");
            Assert.AreEqual("InvalidBeamReferenceLine", Assert.ThrowsException<ArgumentException>(() => new GPC.Model.Structure.Members.PhysicalMemberPath(members, member)).Message);
        }

        // B9: the data-contract comparison now covers the NodeElement and CoordinateSystem fast copies.
        [TestMethod] public void B9_NodeAndCoordinateFastCopiesMatchIndependentDataContractRoundTrip()
        {
            var node = new NodeElement(new Point3d(1, 2, 3), id: 7, name: "N") { CoordinateSystem = new CoordinateSystem(new Point3d(1, 2, 3), new Vector3d(0, 1, 0), new Vector3d(-1, 0, 0)) };
            node.Assignments.Physical = new NodePhysicalData(); node.Assignments.Physical.Temperatures.Add(new NodalTemperatureAssignment { Case = new LoadCaseBase("T"), Celsius = 15 });
            var serializer = new System.Runtime.Serialization.DataContractSerializer(typeof(NodeElement), new System.Runtime.Serialization.DataContractSerializerSettings
            { KnownTypes = ModelValues.DataContracts, PreserveObjectReferences = true, MaxItemsInObjectGraph = 100000 });
            string Graph(object value)
            { using (var stream = new MemoryStream()) { serializer.WriteObject(stream, value); return MigrationCopyContractsTest.ContractGraph(System.Text.Encoding.UTF8.GetString(stream.ToArray())); } }
            NodeElement expected;
            using (var stream = new MemoryStream()) { serializer.WriteObject(stream, node); stream.Position = 0; expected = (NodeElement)serializer.ReadObject(stream); }
            var actual = ModelValues.CopyValue(node);
            Assert.AreEqual(Graph(expected), Graph(actual)); Assert.AreEqual(node.Guid, actual.Guid);
            Assert.AreNotSame(node.CoordinateSystem, actual.CoordinateSystem); Assert.AreEqual(-1, actual.CoordinateSystem.V2.X, 1e-15);
            Assert.AreNotSame(node.Position, actual.Position); Assert.AreEqual(15, actual.Assignments.Physical.Temperatures.Single().Celsius);
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
