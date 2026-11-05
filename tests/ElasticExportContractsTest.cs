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
    [TestMethod] public void GenericSectionRetainsDeclaredElasticModuliWithoutInventingAnOutline()
    {
        var section=new Section(100,800,200,30,0,new Point2d(2,3),new Point3d(2,3,0),0,"tabulated",80,40,75,35);
        Assert.AreEqual(80,section.Wel1);Assert.AreEqual(40,section.Wel2);Assert.AreEqual(75,section.WelX);Assert.AreEqual(35,section.WelY);Assert.IsNull(section.Shape);
        var copy=ModelValues.CopyValue(section);Assert.AreEqual(section,copy);
        using(var stream=new MemoryStream())
        {ModelValueArchive.Save(section,stream);stream.Position=0;var restored=ModelValueArchive.Load<Section>(stream);Assert.AreEqual(80,restored.Wel1);Assert.AreEqual(ModelValues.Fingerprint(new object[]{section}),ModelValues.Fingerprint(new object[]{restored}));}
        Assert.ThrowsException<ArgumentOutOfRangeException>(()=>new Section(100,800,200,30,0,new Point2d(0,0),new Point3d(0,0,0),0,"bad",double.NaN,40));
    }
    /// <summary>The inclusion flags work on a detached copy of the physical section (the composite section definition with the modular
    /// ratios is a contract of GPC.Checkers.CompositeBridge since lane L12).</summary>
    [TestMethod] public void CompositeInclusionFlagsRetainPhysicalGeometry()
    {
        var concrete=new ConcreteMaterialEN1992("C30",30,ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
        var steel=new SteelMaterialEN1993("S355",210000,355,510);
        var section=GPC.Model.Sections.Concrete.ReinforcedConcreteSection.CreateBridgeSection(1000,200,concrete,null,0,40,null,0,new SectionRectangular(300,200),steel);
        double n0=steel.ElasticModulusTension/concrete.ElasticModulusCompression;
        string fingerprint=ModelValues.Fingerprint(new object[]{section});
        var full=GPC.Model.Analysis.CompositeSectionProperties.Calculate(section,0,true,false,true);
        Assert.AreEqual(200000+n0*60000,full.areaH,1e-6);
        var steelOnly=GPC.Model.Analysis.CompositeSectionProperties.Calculate(section,0,false,false,true);
        Assert.AreEqual(n0*60000,steelOnly.areaH,1e-6);Assert.AreEqual(-150,steelOnly.centroidH.Y,1e-8);
        Assert.AreEqual(fingerprint,ModelValues.Fingerprint(new object[]{section}),"the section is not changed");
        Assert.ThrowsException<ArgumentOutOfRangeException>(()=>GPC.Model.Analysis.CompositeSectionProperties.Calculate(section,-1,true,true,true));
        Assert.IsNull(typeof(ModelValues).Assembly.GetType("GPC.Model.Analysis.CompositeSectionDefinition"));
    }
    [TestMethod] public void MaterialDeclarationsRetainValuesWithoutChangingConstitutiveResponse()
    {
        var model=Cantilever();var material=((ElasticBeamProperty)model.BeamElements.Values.Single().BeamProperty).Material;
        string old=model.AnalysisFingerprint();double stress=material.GetStress(.0001),elasticity=material.E;
        material.Declarations=new MaterialDeclarations {ExpectedYieldStress=400,ExpectedTensileStrength=500,ExpectedCompressiveStrength=45,
            CompressiveYieldStrength=250,TensileYieldStrength=260,TensileUltimateStrength=300,ShearUltimateStrength=150,
            Alloy="declared alloy",AluminiumForm=AluminiumManufacturingForm.CastSand};
        string fingerprint=model.AnalysisFingerprint();Assert.AreNotEqual(old,fingerprint);
        Assert.AreEqual(stress,material.GetStress(.0001));Assert.AreEqual(elasticity,material.E);
        foreach(bool document in new[]{false,true})using(var stream=new MemoryStream())
        {
            if(document)ModelArchive.SaveDocument(model,stream);else ModelArchive.Save(model,stream);
            stream.Position=0;var copy=ModelArchive.Load(stream);Assert.AreEqual(fingerprint,copy.AnalysisFingerprint());
            var restored=((ElasticBeamProperty)copy.BeamElements.Values.Single().BeamProperty).Material;
            Assert.AreEqual(material.Declarations,restored.Declarations);restored.Declarations.ExpectedYieldStress=410;
            Assert.AreNotEqual(fingerprint,copy.AnalysisFingerprint());Assert.AreEqual(fingerprint,model.AnalysisFingerprint());
        }
        using(var stream=new MemoryStream()){ModelValueArchive.Save(material.Declarations,stream);stream.Position=0;Assert.AreEqual(material.Declarations,ModelValueArchive.Load<MaterialDeclarations>(stream));}
        material.Declarations.ExpectedYieldStress=double.NaN;Assert.ThrowsException<ArgumentException>(()=>material.Declarations.Validate());
        material.Declarations=null;Assert.AreEqual(old,model.AnalysisFingerprint());
    }
    [TestMethod] public void MaterialAnalysisPropertiesAreCopiedArchivedAndFingerprintRelevant()
    {
        var model=Cantilever();var material=((ElasticBeamProperty)model.BeamElements.Values.Single().BeamProperty).Material;
        material.AnalysisData=new MaterialAnalysisData {BeamShearModulus=80000,ViscousDampingDensity=1e-12,ModalDampingRatio=.03,ThermalConductivity=50,SpecificHeat=4.6e8,
            StressStrainPairs=new[]{0d,0,.001,200,.01,250},NonlinearResponse=MaterialNonlinearResponse.Elastoplastic};
        var fingerprint=model.AnalysisFingerprint();
        foreach(bool document in new[]{false,true})using(var stream=new MemoryStream())
        {
            if(document)ModelArchive.SaveDocument(model,stream);else ModelArchive.Save(model,stream);
            stream.Position=0;var copy=ModelArchive.Load(stream);Assert.AreEqual(fingerprint,copy.AnalysisFingerprint());
            var restored=((ElasticBeamProperty)copy.BeamElements.Values.Single().BeamProperty).Material;
            Assert.AreEqual(material.AnalysisData,restored.AnalysisData);restored.AnalysisData.StressStrainPairs[3]=210;
            Assert.AreNotEqual(fingerprint,copy.AnalysisFingerprint());Assert.AreEqual(fingerprint,model.AnalysisFingerprint());
        }
        material.AnalysisData.StressStrainPairs[2]=-1;Assert.ThrowsException<ArgumentException>(()=>material.AnalysisData.Validate());
        material.AnalysisData.StressStrainPairs=null;Assert.ThrowsException<ArgumentException>(()=>material.AnalysisData.Validate());
    }
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
