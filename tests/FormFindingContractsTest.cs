using System;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Core;
using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Structure.Assignments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class FormFindingContractsTest
    {
        private static Model Sample()
        {
            var a = new NodeElement(Guid.Parse("00000000-0000-0000-0000-000000000001"),new Point3d(0,0,0));
            var b = new NodeElement(Guid.Parse("00000000-0000-0000-0000-000000000002"),new Point3d(1000,0,0));
            var beam = new BeamElement(Guid.Parse("00000000-0000-0000-0000-000000000003"),a.Position,b.Position,null);
            beam.ConnectNodes(a,b); beam.Assignments.Formulation = BeamFormulation.Cable;
            var model=ModelAssembly.Assemble(new[]{beam},Guid.Parse("00000000-0000-0000-0000-000000000004"));
            model.FormFinding = new FormFindingDefinition { Method = FormFindingMethod.NonlinearForceDensity, ProjectionPlane=CoordinateSystem.Global };
            model.FormFinding.Goals.Add(new FormFindingGoal { BeamGuid=beam.Guid, Kind=FormFindingGoalKind.MembraneForcePerWidth, Target=2, Direction=FabricDirection.Warp });
            model.FormFinding.CoordinateGoals.Add(new FormFindingCoordinateGoal {NodeGuid=a.Guid,Weights=new Vector3d(0,0,5)});
            model.FormFinding.Validate(model); return model;
        }
        [TestMethod] public void AppliedPressureResultsAreDistinctFromReactionsAndPersist()
        {
            var model = Sample();
            var load = new GPC.Model.LoadCases.LoadCaseBase("Pressure"); model.LoadCases.Add(load);
            var node = model.NodesElements.Values.First();
            var sample = new GPC.Model.Results.Locations.NodeResultForces(load,
                new GPC.Model.Results.ResultBeamForces(3, 1, 2, 0, 0, 0, CoordinateSystem.Global))
                { Kind = GPC.Model.Results.State.NodalForceKind.AppliedPressure, Body = GPC.Model.Results.State.ActionBody.OnNode };
            GPC.Model.Results.Processing.NodalActions.Validate(model, node, sample);
            sample.Body = GPC.Model.Results.State.ActionBody.OnSupport;
            Assert.ThrowsException<ArgumentException>(() => GPC.Model.Results.Processing.NodalActions.Validate(model, node, sample));
            sample.Body = GPC.Model.Results.State.ActionBody.OnNode;
            node.AddResult(new GPC.Model.Results.Storage.NodeResult(new System.Collections.Generic.List<GPC.Model.Results.Locations.INodeResultLocation> { sample }));
            using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0;
            var restored = ModelArchive.Load(stream);
            var pressure = restored.NodesElements.Values.First(n => n.Results.Count > 0).Results.SelectMany(r => r.Results).OfType<GPC.Model.Results.Locations.NodeResultForces>().Single();
            Assert.AreEqual(GPC.Model.Results.State.NodalForceKind.AppliedPressure, pressure.Kind);
            Assert.AreEqual(1, pressure.Fx); Assert.AreEqual(2, pressure.Fy); Assert.AreEqual(3, pressure.Fz);
        }
        [TestMethod] public void StableConstructorsReproduceAssemblyIdentity()
        {
            var a=Sample(); var b=Sample(); Assert.AreEqual(a.Guid,b.Guid);
            CollectionAssert.AreEqual(a.AllElements.Select(e=>e.Guid).ToArray(),b.AllElements.Select(e=>e.Guid).ToArray());
            Assert.AreEqual(a.AnalysisFingerprint(),b.AnalysisFingerprint());
            Assert.ThrowsException<ArgumentException>(()=>new NodeElement(Guid.Empty,new Point3d(0,0,0)));
        }
        [TestMethod] public void CopyDetachesAllFormFindingInputs()
        {
            var model=Sample(); string before=model.AnalysisFingerprint(); var copy=ModelValues.CopyValue(model);
            Assert.AreEqual(before,copy.AnalysisFingerprint()); copy.FormFinding.Goals[0].Target=3;
            copy.FormFinding.CoordinateGoals[0].Weights.Z=6;
            Assert.AreEqual(before,model.AnalysisFingerprint()); Assert.AreNotEqual(before,copy.AnalysisFingerprint());
        }
        [TestMethod] public void GoalDirectionAndSolverSettingsInvalidateResults()
        {
            var model=Sample(); var baseline=model.AnalysisFingerprint(); model.FormFinding.Goals[0].Direction=FabricDirection.Weft;
            Assert.AreNotEqual(baseline,model.AnalysisFingerprint()); baseline=model.AnalysisFingerprint(); model.FormFinding.SolverToleranceA/=2;
            Assert.AreNotEqual(baseline,model.AnalysisFingerprint());
        }
        [TestMethod] public void SnapshotAndExternalArchiveRetainFormFinding()
        {
            var model=Sample(); string before=model.AnalysisFingerprint(); model.CaptureAnalysis();
            Assert.AreEqual(before,model.Analysis.OpenModel().AnalysisFingerprint());
            var archive=new ExternalModelArchive(); var reference=archive.Prepare(model); var opened=archive.Open(reference).Model;
            Assert.AreEqual(before,opened.AnalysisFingerprint()); Assert.AreEqual(FabricDirection.Warp,opened.FormFinding.Goals.Single().Direction);
        }
        [TestMethod] public void DocumentPreservesFormFindingAndIdentityMap()
        {
            var model=Sample(); using(var bytes=new MemoryStream())
            {
                ModelArchive.SaveDocument(model,bytes); bytes.Position=0; var copy=ModelArchive.Load(bytes);
                Assert.AreEqual(model.AnalysisFingerprint(),copy.AnalysisFingerprint()); Assert.IsNotNull(copy.AssemblyMap);
            }
        }
        [TestMethod] public void UnknownTargetsDuplicateGoalsAndNonfiniteValuesAreRejected()
        {
            var model=Sample(); var g=model.FormFinding.Goals[0]; g.BeamGuid=Guid.NewGuid();
            Assert.ThrowsException<ArgumentException>(()=>model.FormFinding.Validate(model)); g.BeamGuid=model.BeamElements.Values.Single().Guid;
            g.Target=double.NaN; Assert.ThrowsException<ArgumentException>(()=>model.FormFinding.Validate(model)); g.Target=2;
            model.FormFinding.Goals.Add(g); Assert.ThrowsException<ArgumentException>(()=>model.FormFinding.Validate(model));
        }
        [TestMethod] public void CoordinatePenaltiesAreNotSilentlyDiscardedByLinearMethod()
        {
            var model=Sample(); model.FormFinding.Method=FormFindingMethod.LinearForceDensity;
            // Review B7 (approved change): kept and reported as ignored, like legacy Arachne, instead of rejected.
            model.FormFinding.Validate(model);
            Assert.AreEqual("FormFindingCoordinateWeightsIgnored", model.FormFinding.IgnoredInputs().Single().Code);
        }
        [TestMethod] public void CurvatureGoalRequiresChordAndRoundTripsExtendedContract()
        {
            var m=Sample(); var goal=m.FormFinding.Goals[0]; goal.Kind=FormFindingGoalKind.CurvatureRatio; goal.Direction=FabricDirection.Unspecified; goal.Target=10;
            Assert.ThrowsException<ArgumentException>(()=>m.FormFinding.Validate(m)); goal.ChordLengthMm=1000; m.FormFinding.Validate(m);
            var before=m.AnalysisFingerprint(); var archive=new ExternalModelArchive(); var reopened=archive.Open(archive.Prepare(m)).Model;
            Assert.AreEqual(before,reopened.AnalysisFingerprint()); Assert.AreEqual(1000,reopened.FormFinding.Goals[0].ChordLengthMm.Value);
            goal.ChordLengthMm=1100; Assert.AreNotEqual(before,m.AnalysisFingerprint());
        }
        [TestMethod] public void ConflictingSourcesAndLinearLengthGoalsFailExplicitly()
        {
            var m=Sample(); m.FormFinding.Goals.Add(new FormFindingGoal {BeamGuid=m.BeamElements.Values.Single().Guid,Kind=FormFindingGoalKind.ForceDensity,Target=2});
            Assert.ThrowsException<ArgumentException>(()=>m.FormFinding.Validate(m)); m.FormFinding.Goals.RemoveAt(1);
            m.FormFinding.CoordinateGoals.Clear(); m.FormFinding.Method=FormFindingMethod.LinearForceDensity;
            var goal=m.FormFinding.Goals[0]; goal.Kind=FormFindingGoalKind.Length; goal.Direction=FabricDirection.Unspecified; goal.Target=1000;
            // Review B7 (approved change): the linear method keeps the fixed length and ignores it with a warning, as legacy Arachne.
            m.FormFinding.Validate(m); Assert.IsFalse(m.FormFinding.Uses(goal));
            Assert.AreEqual("FormFindingFixedLengthIgnored", m.FormFinding.IgnoredInputs().Single().Code);
            m.FormFinding.Method=FormFindingMethod.SteppedLinearForceDensity; m.FormFinding.Validate(m);
            m.FormFinding.Method=FormFindingMethod.NonlinearForceDensity; m.FormFinding.Validate(m);
        }
        [TestMethod] public void SteppedLinearMethodIsDistinctAndPersists()
        {
            var m=Sample(); m.FormFinding.CoordinateGoals.Clear(); m.FormFinding.Method=FormFindingMethod.LinearForceDensity;
            var before=m.AnalysisFingerprint(); m.FormFinding.Method=FormFindingMethod.SteppedLinearForceDensity; m.FormFinding.Validate(m);
            Assert.AreNotEqual(before,m.AnalysisFingerprint()); using(var bytes=new MemoryStream()) {
                ModelArchive.SaveDocument(m,bytes); bytes.Position=0; Assert.AreEqual(FormFindingMethod.SteppedLinearForceDensity,ModelArchive.Load(bytes).FormFinding.Method); }
        }
        [TestMethod] public void SignedForceTargetsRoundTripAndZeroDivisorsFailOnlyWhereRequired()
        {
            var model=Sample(); var goal=model.FormFinding.Goals[0]; goal.Target=-2;
            model.FormFinding.Validate(model);
            using(var stream=new MemoryStream()) { ModelArchive.SaveDocument(model,stream); stream.Position=0; Assert.AreEqual(-2,ModelArchive.Load(stream).FormFinding.Goals[0].Target); }
            goal.Target=0; model.FormFinding.Validate(model);
            goal.Kind=FormFindingGoalKind.AxialForce; goal.Direction=FabricDirection.Unspecified; model.FormFinding.Validate(model);
            model.FormFinding.CoordinateGoals.Clear(); model.FormFinding.Method=FormFindingMethod.SteppedLinearForceDensity;
            StringAssert.Contains(Assert.ThrowsException<ArgumentException>(()=>model.FormFinding.Validate(model)).Message,"ZeroTargetDivisor");
            goal.Target=-100; model.FormFinding.Validate(model);
            goal.Kind=FormFindingGoalKind.Length; Assert.ThrowsException<ArgumentException>(()=>model.FormFinding.Validate(model));
        }
    }
}
