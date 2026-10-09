using GPC.Examples;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Collections;
using GPC.Model.Costrains;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Restrains;
using GPC.Model.Results;
using GPC.Model.Results.ResultLocations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;

namespace UnitTest
{
    [TestClass]
    public class AssignmentWorkflowTest
    {
        [TestMethod]
        public void RigidLinkEquationsAndReferencedNodesSurviveArchive()
        {
            var model = PostProcessingTest.Mixed(); var link = new RigidLink(model.NodesElements[10], model.NodesElements[70]);
            model.Costrains.Add(link);
            // Slave is +2000 mm along Z: ux_slave = ux_master + 2000 * ry_master.
            Assert.AreEqual(2000, link.Links[0].Equations[2].Value, 1e-10);
            Assert.AreEqual(-2000, link.Links[1].Equations[2].Value, 1e-10);
            var before = model.AnalysisFingerprint();
            using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0; var copy = ModelArchive.Load(stream);
            Assert.AreEqual(before, copy.AnalysisFingerprint()); var restored = copy.Costrains.Values.Single();
            Assert.AreSame(copy.NodesElements[10], restored.StartNode); Assert.AreSame(copy.NodesElements[70], restored.Links[0].Equations[0].NodeSlave);
            Assert.AreEqual(2000, restored.Links[0].Equations[2].Value, 1e-10);
            copy.RemoveNodeChecked(700); Assert.IsFalse(copy.NodesElements.ContainsKey(700));
            Assert.ThrowsException<InvalidOperationException>(() => copy.RemoveNodeChecked(70));
        }

        [TestMethod]
        public void RotatedNodalPrescriptionAndResultsUseGlobalDofDirections()
        {
            var node = new NodeElement(Point3d.Origin); double c = Math.Sqrt(.5);
            var axes = new CoordinateSystem(Point3d.Origin, new Vector3d(c, c, 0), new Vector3d(-c, c, 0));
            var dof = new DofRestrain(GeometryRestrain.DOF.DX, false); dof.Prescribe(3);
            var assignment = new RestrainAssignment { Case = new LoadCaseBase("Settlement"), Restrain = new NodeRestrain(node, axes, new List<DofRestrain> { dof }) };
            var equation = NodalKinematics.ToGlobalEquations(assignment).Single();
            Assert.AreEqual(c, equation.Equations[0].Value, 1e-12); Assert.AreEqual(c, equation.Equations[1].Value, 1e-12);
            Assert.AreEqual(3, equation.ConstValue, 1e-12);
            var global = new ResultDisplacement(axes, 3, 0, 0, 0, 0, .02).ToCoordinateSystem(CoordinateSystem.Global);
            Assert.AreEqual(3 * c, global.D1, 1e-12); Assert.AreEqual(3 * c, global.D2, 1e-12);
            var force = new ResultBeamForces(0, 100, 0, 0, 0, 0, axes).ToGlobalCoordinateSystem();
            Assert.AreEqual(100 * c, force.V1, 1e-10); Assert.AreEqual(100 * c, force.V2, 1e-10);
        }

        [TestMethod]
        public void ZeroPrescriptionIsExplicitAndDoesNotRemoveSpring()
        {
            var model = PostProcessingTest.Mixed(); var node = model.NodesElements[10]; var d = new DofRestrain(GeometryRestrain.DOF.DX, 120);
            d.Prescribe(0); Assert.IsTrue(d.HasImposedDisplacement); Assert.IsTrue(d.HasStiffness);
            var assignment = new RestrainAssignment { Restrain = new NodeRestrain(node, CoordinateSystem.Global, new List<DofRestrain> { d }) };
            node.Assignments.AssignRestrain(assignment, AssignmentMode.Add);
            Assert.IsTrue(model.ValidateAssignments().Any(e => e.Code == "MissingImposedDisplacementCase"));
            assignment.Case = new LoadCaseBase("S"); model.LoadCases.Add((LoadCaseBase)assignment.Case);
            Assert.AreEqual(0, assignment.Restrain.GetImposedDisplacement()[GeometryRestrain.DOF.DX], 1e-12);
            Assert.ThrowsException<InvalidOperationException>(() => node.Assignments.AssignRestrain(assignment, AssignmentMode.Add));
            node.Assignments.AssignRestrain(assignment, AssignmentMode.ReplaceScope); Assert.AreEqual(1, node.Assignments.Restrains.Count);
            using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0;
            var restored = ModelArchive.Load(stream).NodesElements[10].Assignments.Restrains[0].Restrain.Restrains[0];
            Assert.IsTrue(restored.HasImposedDisplacement); Assert.AreEqual(120, restored.Stiffness, 1e-12);
        }

        [TestMethod]
        public void BeamReleaseIsLocalToItsElementAndOrientationIsNotConnectivity()
        {
            var model = PostProcessingTest.Mixed(); var first = model.BeamElements[10];
            var second = new BeamElement(null, null, null, id: 80); model.BeamElements.Add(second); model.ConnectBeam(80, 10, 150);
            var a = new BeamReleasesAttribute { CoordinateSystem = CoordinateSystem.Global }; a.I[4] = new BeamDofConnection(BeamConnectionKind.Released);
            first.Attributes.Add(a); var b = new BeamReleasesAttribute { CoordinateSystem = CoordinateSystem.Global }; second.Attributes.Add(b);
            Assert.AreEqual(BeamConnectionKind.Continuous, b.I[4].Kind); Assert.AreEqual(0, model.NodesElements[10].Assignments.Restrains.Count);
            first.Assignments.OrientationNodeId = 700;
            Assert.AreEqual(0, model.GetAdjacency()[700].Count);
            Assert.ThrowsException<InvalidOperationException>(() => model.RemoveNodeChecked(700));
        }

        [TestMethod]
        public void SelfCrossedShellIsDiagnosedAndInvalidConnectionDoesNotMutateBeam()
        {
            var model = PostProcessingTest.Mixed(); var b = model.BeamElements[10]; var original = b.NodeI;
            Assert.ThrowsException<ArgumentNullException>(() => b.ConnectNodes(model.NodesElements[150], null)); Assert.AreSame(original, b.NodeI);
            model.NodesElements[70].Position = new Point3d(10, 10, 0); model.NodesElements[150].Position = new Point3d(0, 10, 0);
            model.NodesElements[700].Position = new Point3d(10, 0, 0); model.ConnectShell(10, 10, 70, 150, 700);
            Assert.IsTrue(model.ValidateTopology().Any(d => d.Code == "UnsupportedShellWinding"));
        }

        [TestMethod]
        public void LegacyCollectionMissingPayloadIsReportedAcrossResave()
        {
            var info = new SerializationInfo(typeof(SortedCollection<NodeElement>), new FormatterConverter()); info.AddValue("LastId", 100);
            var old = new LegacyCollection(info); old.OnDeserialization(null); Assert.IsTrue(old.MissingLegacyPayload); Assert.AreEqual(0, old.Count);
            var written = new SerializationInfo(typeof(SortedCollection<NodeElement>), new FormatterConverter()); old.GetObjectData(written, new StreamingContext());
            Assert.IsTrue(new LegacyCollection(written).MissingLegacyPayload);
        }
        private sealed class LegacyCollection : SortedCollection<NodeElement> { public LegacyCollection(SerializationInfo info) : base(info, new StreamingContext()) { } }

        [TestMethod]
        public void MutatedPreparedForcesAndMutationsDuringCheckCannotProducePass()
        {
            var model = MixedModelFactory.Create(); var b = model.BeamElements[250]; var sample = Verification.BeamSample(b, "synthetic-static", "P+", .5, SectionSide.Unspecified);
            var input = Verification.PrepareBeam(model, 250, sample, "test"); input.Input.Forces.M1 = 0;
            Assert.AreEqual(DataStatus.Stale, Verification.Run(input, CheckMechanism.UlsBiaxialSection, new TestVerifier()).Data);
            input = Verification.PrepareBeam(model, 250, sample, "test");
            var result = Verification.Run(input, CheckMechanism.UlsBiaxialSection, new TestVerifier { Mutate = i => i.Section.Rebars.First().Position.X += 1 });
            Assert.AreEqual(DataStatus.Stale, result.Data); Assert.AreEqual(EngineeringOutcome.NotEvaluated, result.Outcome);
        }

        [TestMethod]
        public void AsymmetricReinforcementIsUnchangedForOppositeSignedActions()
        {
            var model = MixedModelFactory.Create(); var b = model.BeamElements[250]; b.Assignments.Sections[0].Section.Rebars.First().Position.Y = 100;
            var positive = Verification.PrepareBeam(model, 250, Verification.BeamSample(b, "synthetic-static", "P+", .5, SectionSide.Unspecified), "test").Input;
            var negative = Verification.PrepareBeam(model, 250, Verification.BeamSample(b, "synthetic-static", "P-", .5, SectionSide.Unspecified), "test").Input;
            Assert.AreNotSame(positive.Section, negative.Section);
            Assert.AreEqual(ModelArchive.Fingerprint(new object[] { positive.Section }), ModelArchive.Fingerprint(new object[] { negative.Section }));
            Assert.AreEqual(100, negative.Section.Rebars.First().Position.Y, 1e-10);
            Assert.AreEqual(-1000000, positive.Forces.M1, 1e-7); Assert.AreEqual(1000000, negative.Forces.M1, 1e-7);
        }

        [TestMethod]
        public void ArchivedReportKeepsProvenanceButChangedModelIsNotCurrent()
        {
            var model = MixedModelFactory.Create(); var engine = new TestVerifier();
            var report = CheckRunner.Beam(model, 250, "synthetic-static", "P+", "test", new[] { CheckMechanism.UlsBiaxialSection }, engine);
            model.CheckReports.Add(report); Assert.AreEqual(EngineeringOutcome.Satisfied, report.CurrentOutcome(model, engine));
            using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0; var copy = ModelArchive.Load(stream);
            var restored = copy.CheckReports.Single(); Assert.AreEqual("test", restored.Results[0].Settings);
            Assert.AreEqual(EngineeringOutcome.Satisfied, restored.CurrentOutcome(copy, engine));
            copy.NodesElements[40].Position.Z = 2500; Assert.AreEqual(EngineeringOutcome.NotEvaluated, restored.CurrentOutcome(copy, engine));
        }

        private sealed class TestVerifier : IConcreteSectionVerifier
        {
            public Action<BeamCheckInput>? Mutate;
            public string Version => "TEST ONLY";
            public IReadOnlyCollection<CheckMechanism> Capabilities => new[] { CheckMechanism.UlsBiaxialSection };
            public CheckResult Verify(BeamCheckInput input, CheckMechanism mechanism, CancellationToken token)
            { Mutate?.Invoke(input); return new CheckResult { Execution = ExecutionStatus.Completed, Data = DataStatus.Ready, Outcome = EngineeringOutcome.Satisfied, Utilization = .5 }; }
        }

        [TestMethod]
        public void LinearCombinationMatchesStationsAndSidesAndRejectsMissingOrNonlinearTerms()
        {
            var model = MixedModelFactory.Create(); var combo = new GPC.Model.Combinations.Combination("Derived");
            combo.AddLoadCaseCoefficient(model.LoadCases["P+"], 1.5); combo.AddLoadCaseCoefficient(model.LoadCases["P-"], .5);
            var result = LinearBeamCombination.AtStation(model, 250, combo, "synthetic-static", .5, SectionSide.Unspecified);
            Assert.IsTrue(result.IsAvailable); Assert.AreEqual(-1000000, result.Sample.ResultBeamForces.M1, 1e-7);
            Assert.AreEqual(1000, result.Sample.ResultBeamForces.V2, 1e-10); Assert.AreEqual(true, result.Sample.State.IsCombined);
            Assert.IsFalse(LinearBeamCombination.AtStation(model, 250, combo, "synthetic-static", .3, SectionSide.Unspecified).IsAvailable);
            Assert.IsFalse(LinearBeamCombination.AtStation(model, 250, combo, "synthetic-static", .5, SectionSide.Left).IsAvailable);
            var negative = Verification.BeamSample(model.BeamElements[250], "synthetic-static", "P-", .5, SectionSide.Unspecified);
            negative.State.Semantics = AnalysisSemantics.NonlinearStatic;
            Assert.AreEqual("UnsupportedLinearCombinationState", LinearBeamCombination.AtStation(model, 250, combo, "synthetic-static", .5, SectionSide.Unspecified).Diagnostics[0].Code);
            negative.State.Semantics = AnalysisSemantics.LinearStatic; negative.State.IsCombined = true;
            Assert.AreEqual("AlreadyCombinedOrUnknownProvenance", LinearBeamCombination.AtStation(model, 250, combo, "synthetic-static", .5, SectionSide.Unspecified).Diagnostics[0].Code);
        }

        [TestMethod]
        public void ShellMethodAndPhysicalThicknessMustBeExplicit()
        {
            var model = MixedModelFactory.Create(); var shell = model.AreaElements.Values.First();
            var sample = ResultQueries.Samples<PointResultPlateForces>(shell, new ResultSelection { Dataset = "synthetic-static", Case = "P+" }).Single();
            var preparation = ShellActionPreparation.Prepare(model, shell.Id, sample, null);
            Assert.AreEqual(DataStatus.NotSupported, preparation.Status); Assert.IsNull(preparation.DesignActions);
            Assert.IsTrue(preparation.Diagnostics.Any(d => d.Code == "MissingShellDesignMethod"));
            shell.Assignments.PhysicalThickness = null;
            Assert.IsTrue(ShellActionPreparation.Prepare(model, shell.Id, sample, null).Diagnostics.Any(d => d.Code == "MissingPhysicalThickness"));
        }

        [TestMethod]
        public void BeamForceBodyAndStateSelectionDoNotAcceptAmbiguity()
        {
            var model = MixedModelFactory.Create(); var beam = model.BeamElements[250];
            var selection = new ResultSelection { Dataset = "synthetic-static", Case = "P+" };
            var sample = ResultQueries.BeamStation(beam, selection, .5, SectionSide.Unspecified);
            sample.Body = ActionBody.OnNode;
            Assert.IsTrue(Verification.PrepareBeam(model, 250, sample, "test").Diagnostics.Any(d => d.Code == "UnresolvedSectionActionBody"));
            sample.State.Phase = "Phase 2";
            Assert.IsNull(ResultQueries.BeamStation(beam, selection, .5, SectionSide.Unspecified));
            selection.Phase = "Phase 2"; Assert.AreSame(sample, ResultQueries.BeamStation(beam, selection, .5, SectionSide.Unspecified));
        }

        [TestMethod]
        public void MassOffsetsDistributedLoadAndSolidConnectivitySurviveArchive()
        {
            var model = MixedModelFactory.Create(); var beam = model.BeamElements[250]; var axes = CoordinateSystem.Global;
            var mass = new double[36]; mass[0] = mass[7] = mass[14] = 250; mass[35] = 400000; mass[3] = mass[18] = 12;
            model.NodesElements[40].Assignments.Mass = new NodalMass { Axes = axes, Matrix6x6RowMajor = mass, IncludedInSourceTotal = true, SourceRecord = "synthetic kg/mm" };
            beam.Assignments.OffsetI = new Vector3d(20, 30, 40); beam.Assignments.OffsetJ = new Vector3d(-20, 15, 5); beam.Assignments.OffsetAxes = axes;
            beam.Assignments.Loads.Add(new BeamLoadAssignment
            {
                Start = .1,
                End = .8,
                LengthConvention = BeamLoadLengthConvention.ActualLength,
                StartIntensity = new GPC.Model.Loads.LineLoad(0, 2, 0, 0, 0, 0, new Line3d(beam.StartPoint, beam.EndPoint), model.LoadCases["P+"], axes),
                EndIntensity = new GPC.Model.Loads.LineLoad(0, 3, 0, 0, 0, 0, new Line3d(beam.StartPoint, beam.EndPoint), model.LoadCases["P+"], axes)
            });
            model.PreservedSourceData.Add(new PreservedAssignment { Kind = "Temperature", RawData = "original row", UnsupportedReason = "No thermal interpretation" });
            // Persistence of an existing solid family, with no new solid mechanics/verification.
            model.VolumeElements.Add(new VolumeElement(model.NodesElements.Values.ToArray(), null, id: 900));
            var before = model.AnalysisFingerprint(); using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0; var copy = ModelArchive.Load(stream);
            Assert.AreEqual(before, copy.AnalysisFingerprint()); Assert.AreEqual(12, copy.NodesElements[40].Assignments.Mass.Matrix6x6RowMajor[3], 1e-12);
            Assert.AreEqual(30, copy.BeamElements[250].Assignments.OffsetI.Y, 1e-12); Assert.AreEqual(3, copy.BeamElements[250].Assignments.Loads[0].EndIntensity.F2, 1e-12);
            Assert.AreSame(copy.LoadCases["P+"], copy.BeamElements[250].Assignments.Loads[0].StartIntensity.LoadCase);
            Assert.AreSame(copy.NodesElements[10], copy.VolumeElements[900].Nodes[0]); Assert.AreEqual("original row", copy.PreservedSourceData[0].RawData);
        }

        [TestMethod]
        public void AdditionalUnitFamiliesAndExplicitStripUseCanonicalDimensions()
        {
            var units = new ResultUnits(1000, 1000, 1e6, Math.PI / 180);
            Assert.AreEqual(Math.PI, units.Angle(180), 1e-12); Assert.AreEqual(1e6, units.Area(1), 1e-6);
            Assert.AreEqual(1e12, units.GeometricInertia(1), 1e-2); Assert.AreEqual(.001, units.Stress(1, 1000), 1e-14);
            var strip = ShellPreparation.StripDirection1(new ResultPlateForces(CoordinateSystem.Global, 150, 0, 0, 0, 0, 80000, 0, 0), 1000, CoordinateSystem.Global);
            Assert.AreEqual(150000, strip.N, 1e-7); Assert.AreEqual(80000000, strip.M1, 1e-6);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => units.Force(double.MaxValue));
        }
    }
}
