using System;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Loads.Assignments;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Restraints;
using GPC.Model.Sections;
using GPC.Model.Structure.Assignments;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace UnitTest {
[TestClass] public class ElasticExportContractsTest {
    internal static Model Cantilever(bool distributed=false)
    {
        var fixedNode=new NodeElement(new Point3d(0,0,0));var freeNode=new NodeElement(new Point3d(1000,0,0));
        var freedom=new FreedomCase("Supports");var lc=new LoadCaseBase("P");
        fixedNode.Assignments.Restrains.Add(new RestrainAssignment {FreedomCase=freedom,
            Restrain=new NodeRestrain(fixedNode,Enumerable.Range(0,6).Select(i=>new DofRestrain((GeometryRestrain.DOF)i,true)).ToList())});
        var property=new ElasticBeamProperty("Elastic rectangle",new Material("E210",210000,.3,7.85e-9,1.2e-5),new SectionRectangular(200,100));
        var beam=new BeamElement(fixedNode.Position,freeNode.Position,property);beam.ConnectNodes(fixedNode,freeNode);
        beam.Assignments.Formulation=BeamFormulation.StraightTwoNode;
        beam.Assignments.SectionAxes=new CoordinateSystem(fixedNode.Position,new Vector3d(0,1,0),new Vector3d(0,0,1));
        beam.Assignments.Physical=new BeamPhysicalData {MinimumOutputStations=17};
        if(distributed)beam.Assignments.Loads.Add(new BeamLoadAssignment {Start=0,End=1,LengthConvention=BeamLoadLengthConvention.ActualLength,
            StartIntensity=new LineLoad(0,0,-2,0,0,0,beam.Line,lc),EndIntensity=new LineLoad(0,0,-2,0,0,0,beam.Line,lc)});
        else freeNode.Loads.Add(new PointLoad(0,0,-1000,0,0,0,freeNode.Position,lc));
        return ModelAssembly.Assemble(new Element[]{beam},Guid.NewGuid());
    }
    [TestMethod] public void ElasticPropertiesAndFreedomCasesRoundTripAndDetach()
    {
        var model=Cantilever();var fingerprint=model.AnalysisFingerprint();var copy=ModelValues.Copy(model);
        Assert.AreEqual(fingerprint,copy.AnalysisFingerprint());
        var fixedNode=copy.NodesElements.Values.Single(n=>n.Assignments.Restrains.Count!=0);
        Assert.AreEqual(copy.FreedomCases.Values.Single().Guid,fixedNode.Assignments.Restrains[0].FreedomCase.Guid);
        using(var bytes=new MemoryStream())
        {ModelArchive.SaveDocument(model,bytes);bytes.Position=0;var restored=ModelArchive.Load(bytes);Assert.AreEqual(fingerprint,restored.AnalysisFingerprint());}
        var original=(ElasticBeamProperty)model.BeamElements.Values.Single().BeamProperty;
        var changed=(ElasticBeamProperty)copy.BeamElements.Values.Single().BeamProperty;
        changed.Material.Density*=2;
        Assert.IsFalse(original.Equals(changed));Assert.AreNotEqual(fingerprint,copy.AnalysisFingerprint());Assert.AreEqual(fingerprint,model.AnalysisFingerprint());
    }
    [TestMethod] public void OrthotropicPlateAxesInvalidateAnalysisAndElasticPropertyEquality()
    {
        var nodes=new[]{new NodeElement(new Point3d(0,0,0)),new NodeElement(new Point3d(100,0,0)),new NodeElement(new Point3d(0,100,0))};
        var material=new Material("orthotropic",1000,.2,1e-9,0) {Orthotropy=new OrthotropicElasticity {E1=1000,E2=500,E3=100,G12=100,G23=50,G31=50,Nu12=.2,Nu23=.1,Nu31=.05}};
        var plate=new AreaElement(null,new ElasticPlateProperty("membrane",material,0,1,ElasticPlateFormulation.Membrane));plate.ConnectNodes(nodes);plate.Assignments.LayerAxes=CoordinateSystem.Global;
        var model=ModelAssembly.Assemble(new Element[]{plate},Guid.NewGuid());var copy=ModelValues.Copy(model);string before=copy.AnalysisFingerprint();
        copy.AreaElements.Values.Single().Assignments.LayerAxes=new CoordinateSystem(Point3d.Origin,new Vector3d(0,1,0),new Vector3d(-1,0,0));
        Assert.AreNotEqual(before,copy.AnalysisFingerprint());Assert.AreEqual(before,model.AnalysisFingerprint());
        var property=(ElasticPlateProperty)copy.AreaElements.Values.Single().PlateProperty;property.MembraneThickness=2;
        Assert.IsFalse(property.Equals(model.AreaElements.Values.Single().PlateProperty));
    }
    [TestMethod] public void FreedomCaseScopesDoNotOverwriteEachOtherAndAreCanonicalAfterAssembly()
    {
        var node=new NodeElement(Point3d.Origin);var a=new FreedomCase("A");var b=new FreedomCase("B");
        foreach(var c in new[]{a,b})node.Assignments.AssignRestrain(new RestrainAssignment {FreedomCase=c,Restrain=new NodeRestrain(node,new System.Collections.Generic.List<DofRestrain>{new DofRestrain(GeometryRestrain.DOF.DX)})},AssignmentMode.Add);
        var model=ModelAssembly.Assemble(new Element[]{node},Guid.NewGuid());Assert.AreEqual(2,model.FreedomCases.Count);
        foreach(var r in model.NodesElements.Values.Single().Assignments.Restrains)Assert.AreSame(model.FreedomCases[r.FreedomCase.Name],r.FreedomCase);
        string before=model.AnalysisFingerprint();model.NodesElements.Values.Single().Assignments.Restrains[0].FreedomCase=model.FreedomCases["B"];
        Assert.AreNotEqual(before,model.AnalysisFingerprint());
    }
    [TestMethod] public void LoadPatchContractPreservesManualWeightsAndRejectsUnsupportedEdges()
    {
        var p=new LoadPatchDistribution {Kind=LoadPatchDistributionKind.Manual,SelectedEdges=new[]{1,3},EdgeWeights=new[]{.4,0,.6,0}};p.Validate(4);
        var copy=ModelValues.CopyValue(p);copy.EdgeWeights[0]=.2;Assert.AreEqual(.4,p.EdgeWeights[0]);
        Assert.AreNotEqual(ModelValues.Fingerprint(new object[]{p}),ModelValues.Fingerprint(new object[]{copy}));
        p.SelectedEdges=new[]{4};Assert.ThrowsException<ArgumentException>(()=>p.Validate(3));
        Assert.ThrowsException<ArgumentException>(()=>new ElasticPlateProperty("patch",new Material("E",1000,.2,0,0),0,0,ElasticPlateFormulation.LoadPatch));
    }
}
}
