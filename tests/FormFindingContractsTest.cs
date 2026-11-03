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
            Assert.ThrowsException<ArgumentException>(()=>model.FormFinding.Validate(model));
        }
    }
}
