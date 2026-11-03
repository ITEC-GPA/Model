using System;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Structure.Assignments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class PhysicalAssignmentsTest
    {
        [TestMethod] public void OrthotropicComplianceMatchesIndependentIsotropicLimit()
        {
            const double e=210000, nu=0.3, g=e/(2*(1+nu));
            var o=new OrthotropicElasticity {E1=e,E2=e,E3=e,G12=g,G23=g,G31=g,Nu12=nu,Nu23=nu,Nu31=nu};
            var s=o.Compliance(); Assert.AreEqual(1/e,s[0],1e-18); Assert.AreEqual(-nu/e,s[1],1e-18); Assert.AreEqual(1/g,s[21],1e-18);
            for(int i=0;i<6;i++) for(int j=0;j<6;j++) Assert.AreEqual(s[i*6+j],s[j*6+i]);
            o.Nu12=o.Nu23=o.Nu31=0.6; Assert.ThrowsException<ArgumentException>(()=>o.Validate());
            o.Nu12=o.Nu23=o.Nu31=0.3; o.G31=0; Assert.ThrowsException<ArgumentException>(()=>o.Validate());
        }
        [TestMethod] public void OrthotropicMaterialCopyKeepsTensorAndRejectsScalarIsotropicUse()
        {
            var m=new Material("fabric",1000,0.2,1e-9,0) {Orthotropy=new OrthotropicElasticity {E1=1000,E2=500,E3=100,G12=200,G23=40,G31=50,Nu12=0.2,Nu23=0.1,Nu31=0.05}};
            var c=ModelValues.CopyValue(m); Assert.AreEqual(m.Orthotropy,c.Orthotropy); Assert.AreNotSame(m.Orthotropy,c.Orthotropy);
            Assert.ThrowsException<NotSupportedException>(()=>m.GetShearModule()); Assert.ThrowsException<NotSupportedException>(()=>{var ignored=m.E;});
            var before=ModelValues.Fingerprint(new object[]{m}); c.Orthotropy.E2=450;
            Assert.AreNotEqual(before,ModelValues.Fingerprint(new object[]{c})); Assert.AreEqual(before,ModelValues.Fingerprint(new object[]{m}));
            Assert.IsFalse(m.Equals(c));
        }
        [TestMethod] public void MembraneStiffnessUsesWidthUnitsAndReciprocalPoissonCoefficients()
        {
            var m=new MembraneMaterialData {MassPerArea=1.05e-9, StiffnessWarp=1000,StiffnessWeft=500,ShearStiffness=100,NuWarpWeft=0.2,StrengthWarp=80,StrengthWeft=60};
            var a=m.Stiffness(); Assert.AreEqual(1000/0.98,a[0],1e-10); Assert.AreEqual(500/0.98,a[4],1e-10);
            Assert.AreEqual(100/0.98,a[1],1e-10); Assert.AreEqual(a[1],a[3]); Assert.AreEqual(100,a[8]);
            m.ShearStiffness=null; m.Validate(); Assert.ThrowsException<InvalidOperationException>(()=>m.Stiffness());
            m.NuWarpWeft=2; Assert.ThrowsException<ArgumentException>(()=>m.Validate());
        }
        [TestMethod] public void PhysicalMembraneMaterialSurvivesArchiveWithoutInventedThickness()
        {
            var m=Sample(); m.AreaElements.Values.Single().Assignments.Physical.Membrane=new MembraneMaterialData {Name="1050",MassPerArea=1.05e-9,StrengthWarp=80,StrengthWeft=60};
            using(var bytes=new MemoryStream()) { ModelArchive.SaveDocument(m,bytes); bytes.Position=0; var copy=ModelArchive.Load(bytes);
                Assert.AreEqual(m.AnalysisFingerprint(),copy.AnalysisFingerprint()); Assert.IsNull(copy.AreaElements.Values.Single().Assignments.Physical.Membrane.StiffnessWarp); }
        }
        internal static Model Sample()
        {
            var nodes = new[] {new NodeElement(new Point3d(0,0,0)), new NodeElement(new Point3d(1000,0,0)), new NodeElement(new Point3d(0,1000,0))};
            var lc = new LoadCaseBase("physical");
            nodes[0].Assignments.Physical = new NodePhysicalData();
            nodes[0].Assignments.Physical.Temperatures.Add(new NodalTemperatureAssignment {Case=lc, Kind=NodalTemperatureKind.Initial, Celsius=-10});
            var beam = new BeamElement(nodes[0].Position,nodes[1].Position,null); beam.ConnectNodes(nodes[0],nodes[1]); beam.Assignments.Formulation=BeamFormulation.StraightTwoNode;
            beam.Assignments.Physical = new BeamPhysicalData {Taper=new BeamSectionTaper {ScaleI1=0.8, ScaleJ2=1.2, Alignment1=TaperAlignment.PositiveSide}, MinimumOutputStations=11};
            beam.Assignments.Physical.TemperatureGradients.Add(new BeamTemperatureGradient {Case=lc, Axis1=0.01, Axis2=-0.02});
            beam.Assignments.Physical.InitialStates.Add(new BeamInitialState {Case=lc, Kind=BeamInitialStateKind.AxialForce, Value=12000});
            beam.Assignments.Physical.Supports.Add(new BeamDistributedSupport {Stiffness=50, Gap=2, Behavior=DistributedSpringBehavior.CompressionOnly, Direction=new Vector3d(0,-1,0)});
            var shell = new AreaElement(null,null); shell.ConnectNodes(nodes);
            shell.Assignments.Physical = new ShellPhysicalData();
            shell.Assignments.Physical.InitialStresses.Add(new ShellInitialStress {Case=lc, Components=new Vector3d(1,2,-0.5)});
            shell.Assignments.Physical.NonStructuralMasses.Add(new ShellNonStructuralMass {Case=lc, MassPerArea=2e-9, DynamicFactor=0.8, GlobalOffset=new Vector3d(0,0,40)});
            shell.Assignments.Physical.Supports.Add(new ShellDistributedSupport {Stiffness=0.1, Face=ShellFace.PositiveNormal, Behavior=DistributedSpringBehavior.Bilateral});
            return ModelAssembly.Assemble(new Element[] {beam,shell},Guid.NewGuid());
        }

        [TestMethod] public void AssemblyRegistersAndRebindsPhysicalLoadCases()
        {
            var m=Sample(); var lc=m.LoadCases["physical"];
            Assert.AreSame(lc,m.NodesElements.Values.Single(n=>n.Assignments.Physical!=null).Assignments.Physical.Temperatures[0].Case);
            Assert.AreSame(lc,m.BeamElements.Values.Single().Assignments.Physical.InitialStates[0].Case);
            Assert.AreSame(lc,m.AreaElements.Values.Single().Assignments.Physical.NonStructuralMasses[0].Case);
        }
        [TestMethod] public void PhysicalDataCopiesDetachVectorsAndCollections()
        {
            var m=Sample(); var before=m.AnalysisFingerprint(); var c=ModelValues.Copy(m);
            Assert.AreEqual(before,c.AnalysisFingerprint());
            c.BeamElements.Values.Single().Assignments.Physical.Supports[0].Direction.Y=1;
            c.AreaElements.Values.Single().Assignments.Physical.InitialStresses[0].Components.X=9;
            c.NodesElements.Values.Single(n=>n.Assignments.Physical!=null).Assignments.Physical.Temperatures.Clear();
            Assert.AreEqual(before,m.AnalysisFingerprint()); Assert.AreNotEqual(before,c.AnalysisFingerprint());
        }
        [TestMethod] public void PhysicalDataSurvivesDocumentSnapshotAndExternalArchive()
        {
            var m=Sample(); var fingerprint=m.AnalysisFingerprint(); m.CaptureAnalysis();
            Assert.AreEqual(fingerprint,m.Analysis.OpenModel().AnalysisFingerprint());
            using(var stream=new MemoryStream())
            {
                ModelArchive.SaveDocument(m,stream); stream.Position=0; var c=ModelArchive.Load(stream);
                Assert.AreEqual(fingerprint,c.AnalysisFingerprint());
                Assert.AreEqual(ShellFace.PositiveNormal,c.AreaElements.Values.Single().Assignments.Physical.Supports[0].Face);
                Assert.AreEqual(NodalTemperatureKind.Initial,c.NodesElements.Values.Single(n=>n.Assignments.Physical!=null).Assignments.Physical.Temperatures[0].Kind);
            }
            var archive=new ExternalModelArchive(); var reference=archive.Prepare(m);
            Assert.AreEqual(fingerprint,archive.Open(reference).Model.AnalysisFingerprint());
        }
        [TestMethod] public void EachPhysicalFamilyInvalidatesAnalysisAndSolverBinding()
        {
            Action<Model>[] edits={
                m=>m.NodesElements.Values.Single(n=>n.Assignments.Physical!=null).Assignments.Physical.Temperatures[0].Kind=NodalTemperatureKind.Reference,
                m=>m.BeamElements.Values.Single().Assignments.Physical.TemperatureGradients[0].Axis1+=0.01,
                m=>m.BeamElements.Values.Single().Assignments.Physical.InitialStates[0].Kind=BeamInitialStateKind.AxialStrain,
                m=>m.BeamElements.Values.Single().Assignments.Physical.Supports[0].Gap+=1,
                m=>m.BeamElements.Values.Single().Assignments.Physical.Taper.ScaleJ1+=0.1,
                m=>m.BeamElements.Values.Single().Assignments.Physical.MinimumOutputStations=15,
                m=>m.AreaElements.Values.Single().Assignments.Physical.InitialStresses[0].Components.Z+=1,
                m=>m.AreaElements.Values.Single().Assignments.Physical.NonStructuralMasses[0].DynamicFactor=1,
                m=>m.AreaElements.Values.Single().Assignments.Physical.Supports[0].Face=ShellFace.NegativeNormal};
            foreach(var edit in edits)
            {
                var m=Sample(); var analysis=m.AnalysisFingerprint(); var binding=m.SolverBindingFingerprint(); edit(m);
                Assert.AreNotEqual(analysis,m.AnalysisFingerprint()); Assert.AreNotEqual(binding,m.SolverBindingFingerprint());
            }
        }
        [TestMethod] public void InvalidPhysicalInputsAreRejectedBeforeAssembly()
        {
            var m=Sample(); var b=m.BeamElements.Values.Single(); var p=b.Assignments.Physical;
            p.TemperatureGradients[0].Axis1=double.NaN; Assert.ThrowsException<ArgumentException>(()=>PhysicalAssignments.Validate(b)); p.TemperatureGradients[0].Axis1=0;
            p.InitialStates.Add(p.InitialStates[0]); Assert.ThrowsException<ArgumentException>(()=>PhysicalAssignments.Validate(b)); p.InitialStates.RemoveAt(1);
            p.Supports[0].Direction.Y=2; Assert.ThrowsException<ArgumentException>(()=>PhysicalAssignments.Validate(b)); p.Supports[0].Direction.Y=1;
            p.Taper.ScaleI1=0; Assert.ThrowsException<ArgumentException>(()=>PhysicalAssignments.Validate(b)); p.Taper.ScaleI1=1;
            p.MinimumOutputStations=1; Assert.ThrowsException<ArgumentException>(()=>PhysicalAssignments.Validate(b)); p.MinimumOutputStations=2;
            p.InitialStates[0].Case=null; Assert.ThrowsException<ArgumentException>(()=>PhysicalAssignments.Validate(b));
        }
        [TestMethod] public void CoincidentNodesWithDifferentTemperaturesCannotMerge()
        {
            var lc=new LoadCaseBase("T"); var a=new NodeElement(new Point3d(0,0,0)); var b=new NodeElement(new Point3d(0,0,0));
            foreach(var n in new[]{a,b}) n.Assignments.Physical=new NodePhysicalData();
            a.Assignments.Physical.Temperatures.Add(new NodalTemperatureAssignment {Case=lc,Celsius=10});
            b.Assignments.Physical.Temperatures.Add(new NodalTemperatureAssignment {Case=lc,Celsius=20});
            Assert.ThrowsException<ArgumentException>(()=>ModelAssembly.Assemble(new[]{a,b},Guid.NewGuid(),mergeToleranceMm:0));
        }
    }
}
