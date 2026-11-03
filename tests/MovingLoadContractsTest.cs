using System;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Actions.Moving;
using GPC.Model.Core;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Structure.Assignments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class MovingLoadContractsTest
    {
        internal static Model Sample()
        {
            var a=new NodeElement(new Point3d(0,0,0)); var b=new NodeElement(new Point3d(1000,0,0));
            var beam=new BeamElement(a.Position,b.Position,null); beam.ConnectNodes(a,b); beam.Assignments.Formulation=BeamFormulation.StraightTwoNode;
            var model=ModelAssembly.Assemble(new[]{beam},Guid.NewGuid()); var load=new LoadCaseBase("traffic"); model.AddLoadCase(load);
            model.MovingLoads=new MovingLoadDefinition();
            model.MovingLoads.Paths.Add(new MovingLoadPath {Id=Guid.NewGuid(),CaseGuid=load.Guid,Start=new Point3d(0,0,0),End=new Point3d(1000,0,0),Reference=new Point3d(0,100,0),Target=MovingPathTarget.Beams,
                Template=new MovingLoadTemplate {Name="vehicle",LaneFactors=new[]{1.0,.8},MinimumLaneWidth=3000,Vehicles=new[]{new MovingVehicle {Name="truck",StartTime=-2,Speed=10000,AllowedLanes=new[]{1,2},
                    PointForces=new[]{new MovingPointForce {Force=new Vector3d(0,0,-100000),ApplyAdjacencyFactor=true}},
                    DistributedForces=new[]{new MovingDistributedForce {X1=0,X2=1000,Y1=-500,Y2=500,ForcePerLength=new Vector3d(0,0,-2)}}}}}});
            model.MovingLoads.Validate(model); return model;
        }
        internal static void AddAdvancedLoads(Model model)
        {
            var template=model.MovingLoads.Paths[0].Template;
            template.Centrifugal=new MovingCentrifugalLaw {FactorKind=MovingCentrifugalFactorKind.SpeedSquaredOverGravityRadius,LimitFactor=true,MinimumFactor=0,MaximumFactor=.2};
            template.Vehicles[0].PointForces[0].ApplyCentrifugal=true;
            template.Vehicles[0].HeatSources=new[]{new MovingHeatSource {X=100,Y=20,SizeX=50,SizeY=100,Power=250000}};
            model.MovingLoads.Validate(model);
        }
        [TestMethod] public void HeatAndCentrifugalInputsSurviveBothArchivesAndDetachOnCopy()
        {
            var model=Sample();Assert.IsFalse(model.MovingLoads.UsesExtendedContract);AddAdvancedLoads(model);
            Assert.IsTrue(model.MovingLoads.UsesExtendedContract);string fingerprint=model.AnalysisFingerprint();
            foreach(bool document in new[]{false,true})using(var stream=new MemoryStream())
            {
                if(document)ModelArchive.SaveDocument(model,stream);else ModelArchive.Save(model,stream);
                stream.Position=0;var copy=ModelArchive.Load(stream);Assert.AreEqual(fingerprint,copy.AnalysisFingerprint());
                var t=copy.MovingLoads.Paths[0].Template;t.Vehicles[0].HeatSources[0].Power++;
                Assert.AreNotEqual(fingerprint,copy.AnalysisFingerprint());Assert.AreEqual(fingerprint,model.AnalysisFingerprint());
                t.Vehicles[0].HeatSources[0].Power--;t.Centrifugal.FactorKind=MovingCentrifugalFactorKind.Constant;t.Centrifugal.Factor=.1;Assert.AreNotEqual(fingerprint,copy.AnalysisFingerprint());
            }
        }
        [TestMethod] public void HeatOnlyVehiclesAndCentrifugalValidationAreExplicit()
        {
            var model=Sample();AddAdvancedLoads(model);var t=model.MovingLoads.Paths[0].Template;var v=t.Vehicles[0];
            t.Centrifugal=null;Assert.ThrowsException<ArgumentException>(()=>model.MovingLoads.Validate(model));
            v.PointForces=Array.Empty<MovingPointForce>();v.DistributedForces=Array.Empty<MovingDistributedForce>();model.MovingLoads.Validate(model);
            v.HeatSources[0].SizeY=0;Assert.ThrowsException<ArgumentException>(()=>model.MovingLoads.Validate(model));
            v.HeatSources[0].SizeX=0;model.MovingLoads.Validate(model);
            v.HeatSources[0].Power=double.NaN;Assert.ThrowsException<ArgumentException>(()=>model.MovingLoads.Validate(model));
        }
        [TestMethod] public void MovingDomainCopyDetachesMutableGeometryVehiclesAndForces()
        {
            var model=Sample(); string before=model.AnalysisFingerprint(); var copy=ModelValues.CopyValue(model); copy.MovingLoads.Validate(copy);
            Assert.AreEqual(before,copy.AnalysisFingerprint()); Assert.AreEqual(model.MovingLoads.Paths[0].CaseGuid,copy.MovingLoads.Paths[0].CaseGuid);
            copy.MovingLoads.Paths[0].Reference.Y=200;
            copy.MovingLoads.Paths[0].Template.Vehicles[0].PointForces[0].Force.Z=-200000;
            Assert.AreEqual(before,model.AnalysisFingerprint()); Assert.AreNotEqual(before,copy.AnalysisFingerprint());
        }
        [TestMethod] public void MovingPathsSurviveDocumentAndResultArchive()
        {
            var model=Sample();
            foreach(bool document in new[]{true,false})
                using(var stream=new MemoryStream())
                {
                    if(document) ModelArchive.SaveDocument(model,stream); else ModelArchive.Save(model,stream);
                    stream.Position=0; var restored=ModelArchive.Load(stream); restored.MovingLoads.Validate(restored);
                    Assert.AreEqual(model.AnalysisFingerprint(),restored.AnalysisFingerprint());
                    Assert.AreEqual(-2,restored.MovingLoads.Paths[0].Template.Vehicles[0].StartTime);
                    Assert.AreEqual(MovingPathTarget.Beams,restored.MovingLoads.Paths[0].Target);
                }
        }
        [TestMethod] public void DistributedResultantDependsOnLengthAndIsIndependentOfFootprintWidth()
        {
            var load=Sample().MovingLoads.Paths[0].Template.Vehicles[0].DistributedForces[0];
            Assert.AreEqual(-2000,load.Resultant().Z); load.Y1=-5000; load.Y2=5000; Assert.AreEqual(-2000,load.Resultant().Z);
            load.X2=2000; Assert.AreEqual(-4000,load.Resultant().Z);
            load.ForcePerLength=new Vector3d(0,0,0); load.Validate(); Assert.AreEqual(0,load.Resultant().Length);
        }
        [DataTestMethod,DataRow(1),DataRow(2),DataRow(3)] public void PathDependentDistributedLoadsRequireExplicitLoadedLength(int mobility)
        {
            var load=new MovingDistributedForce {Mobility=(MovingDistributionMobility)mobility,ForcePerLength=new Vector3d(0,0,-2)};
            load.Validate();Assert.ThrowsException<InvalidOperationException>(()=>load.Resultant());Assert.AreEqual(-6000,load.Resultant(3000).Z);
            Assert.ThrowsException<ArgumentException>(()=>load.Resultant(-1));
        }
        [TestMethod] public void ChangesInvalidateBothAnalysisAndSolverBinding()
        {
            Action<MovingLoadPath>[] changes={
                p=>p.Divisions++, p=>p.Template.LaneFactors[1]=.7, p=>p.Template.Vehicles[0].Speed++,
                p=>p.Template.Vehicles[0].PointForces[0].Axes=MovingLoadAxes.Global, p=>p.End.X++,
                p=>p.Template.Vehicles[0].DistributedForces[0].Mobility=MovingDistributionMobility.Floating};
            foreach(var change in changes)
            {
                var model=Sample(); string analysis=model.AnalysisFingerprint(),binding=model.SolverBindingFingerprint(); change(model.MovingLoads.Paths[0]);
                Assert.AreNotEqual(analysis,model.AnalysisFingerprint()); Assert.AreNotEqual(binding,model.SolverBindingFingerprint());
            }
        }
        [TestMethod] public void LaneFactorsDependOnActiveCountOrSeverityRatherThanLanePosition()
        {
            var template=Sample().MovingLoads.Paths[0].Template; template.LaneFactors=new[]{1.0,.9,.7};
            CollectionAssert.AreEqual(new[]{.9,.9},template.ActiveLaneFactors(2));
            CollectionAssert.AreEqual(new[]{.7,.7,.7},template.ActiveLaneFactors(3));
            template.LaneFactorRule=MovingLaneFactorRule.BySeverityRank;
            CollectionAssert.AreEqual(new[]{1.0,.9},template.ActiveLaneFactors(2));
            CollectionAssert.AreEqual(new[]{1.0,.9,.7},template.ActiveLaneFactors(3));
            Assert.ThrowsException<ArgumentOutOfRangeException>(()=>template.ActiveLaneFactors(4));
        }
        [TestMethod] public void CaseAndElementReferencesAreValidatedByGuid()
        {
            var model=Sample(); var path=model.MovingLoads.Paths[0]; path.Target=MovingPathTarget.Selection;
            path.TargetElementGuids=new[]{model.BeamElements.Values.Single().Guid}; model.MovingLoads.Validate(model);
            path.TargetElementGuids=new[]{model.NodesElements.Values.First().Guid};
            StringAssert.Contains(Assert.ThrowsException<ArgumentException>(()=>model.MovingLoads.Validate(model)).Message,"UnknownMovingTargetElement");
            path.TargetElementGuids=new[]{model.BeamElements.Values.Single().Guid}; path.CaseGuid=Guid.NewGuid();
            StringAssert.Contains(Assert.ThrowsException<ArgumentException>(()=>model.MovingLoads.Validate(model)).Message,"UnknownMovingLoadCase");
        }
        [TestMethod] public void InvalidGeometrySelectionsAndNonfiniteLoadsAreRejected()
        {
            Action<MovingLoadPath>[] invalid={p=>p.Reference=new Point3d(500,0,0),p=>p.End=p.Start,p=>p.Surface=MovingPathSurface.Curved,
                p=>p.Template.Vehicles[0].AllowedLanes=new[]{3},p=>p.Template.Vehicles[0].PointForces[0].Force.Z=double.NaN,
                p=>p.Template.LaneFactors[0]=-1,p=>p.Template.Vehicles[0].Speed=-1,p=>p.TargetElementGuids=new[]{Guid.NewGuid()}};
            foreach(var change in invalid) { var model=Sample(); change(model.MovingLoads.Paths[0]); Assert.ThrowsException<ArgumentException>(()=>model.MovingLoads.Validate(model)); }
        }
    }
}
