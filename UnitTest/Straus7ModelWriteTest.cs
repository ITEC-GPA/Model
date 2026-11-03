using GPC.Converter;
using GPC.Converter.Straus7;
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
using GPC.Model.Stages;
using GPC.Model.Structure.Assignments;
using GPC.Model.Actions.Moving;
using System.Text;
using System.Xml.Linq;
using System.Security.Cryptography;

namespace UnitTest;

[TestClass]
public class Straus7ModelWriteTest
{
    [DataTestMethod,DataRow(false,false),DataRow(true,false),DataRow(false,true),DataRow(true,true),TestCategory("Straus7Native")]
    public void NativeGravityMassFamiliesMatchIndependentEquilibrium(bool structural,bool nonStructural)
    {
        var nodes=new[]{new NodeElement(new Point3d(0,0,0)),new NodeElement(new Point3d(100,0,0)),new NodeElement(new Point3d(100,100,0)),new NodeElement(new Point3d(0,100,0))};
        foreach(var node in nodes.Where(n=>n.Position.X==0))node.Assignments.Restrains.Add(new RestrainAssignment {Restrain=new NodeRestrain(node,Enumerable.Range(0,6).Select(i=>new DofRestrain((GeometryRestrain.DOF)i,true)).ToList())});
        var plate=new AreaElement(null,new ElasticPlateProperty("mass plate",new Material("E",210000,.3,1e-9,0),10,10));plate.ConnectNodes(nodes);
        LoadCaseBase lc=new LoadCase("gravity",LoadCase.LoadCaseTypes.SelfWeight) {InertiaMasses=new InertiaMassSelection {Structural=structural,NonStructural=nonStructural}};
        plate.Assignments.Physical=new ShellPhysicalData();plate.Assignments.Physical.NonStructuralMasses.Add(new ShellNonStructuralMass {Case=lc,MassPerArea=2e-8});
        var model=ModelAssembly.Assemble(new Element[]{plate},Guid.NewGuid());lc=model.LoadCases.Values.Single();
        model.ModelLoads.Add(new ModelGravityLoad(new Vector3d(0,0,-1),10000,lc,CoordinateSystem.Global));
        string fingerprint=model.AnalysisFingerprint();
        foreach(bool document in new[]{false,true})using(var stream=new MemoryStream())
        {if(document)ModelArchive.SaveDocument(model,stream);else ModelArchive.Save(model,stream);stream.Position=0;var restored=ModelArchive.Load(stream);Assert.AreEqual(fingerprint,restored.AnalysisFingerprint());Assert.AreEqual(structural,restored.LoadCases.Values.Single().InertiaMasses.Structural);}
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder();var run=plan.RunLinearStatic(model,Path.Combine(folder,"mass-flags.st7"),folder);
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath,folder);CollectionAssert.AreEqual(new[]{structural,nonStructural},reader.ReadLoadCaseMassOption(1));reader.OpenResults(run.ResultPath);
            double reaction=model.NodesElements.Values.Sum(n=>reader.ReadNodeResult(n.Id,1,true).Values[2]);
            // rho*A*t*g = 1 N; mNS*A*g = 2 N. Input units t, mm, s give N without extra factors.
            Assert.AreEqual((structural?1d:0)+(nonStructural?2d:0),reaction,1e-8);
        }
        Assert.AreEqual(fingerprint,model.AnalysisFingerprint());
    }
    [DataTestMethod,DataRow(0),DataRow(1),DataRow(2),TestCategory("Straus7Native")]
    public void NativeMeshedSectionsRetainHolesMirrorsAndTaperedAxialCompliance(int kind)
    {
        Section section=kind==0?new SectionRectangular(20,10,.17):kind==1?new SectionRHS(30,20,2,3,2,3,"unequal walls"):
            new SectionBuiltUp(new[]{new SectionBuiltUp.Part(new SectionRectangular(20,10),5,0),new SectionBuiltUp.Part(new SectionRectangular(20,10),-5,0,true)},"mirrored pair");
        var model=Cantilever();var beam=model.BeamElements.Values.Single();var free=beam.NodeJ;var lc=model.LoadCases.Values.Single();var fc=model.FreedomCases.Values.Single();
        beam.BeamProperty=new ElasticBeamProperty("BXS regions",new Material("E1000",1000,.3,0,0),section);
        beam.Assignments.Physical.Taper=new BeamSectionTaper {ScaleI1=1,ScaleJ1=2,ScaleI2=1,ScaleJ2=2,Alignment1=TaperAlignment.Symmetric,Alignment2=TaperAlignment.Symmetric};
        free.Loads.Clear();free.Loads.Add(new PointLoad(1000,0,0,0,0,0,free.Position,lc));
        free.Assignments.Restrains.Add(new RestrainAssignment {FreedomCase=fc,Restrain=new NodeRestrain(free,Enumerable.Range(1,5).Select(i=>new DofRestrain((GeometryRestrain.DOF)i,true)).ToList())});
        string fingerprint=model.AnalysisFingerprint(),folder=NativeFolder();
        var plan=Straus7ModelWritePlan.Prepare(model,null,new Dictionary<Guid,int>{{beam.Guid,10}});
        var run=plan.RunLinearStatic(model,Path.Combine(folder,"regions.st7"),folder);
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath,folder);
            Assert.AreEqual(section.Area,reader.ReadProperty(Straus7Entity.Beam,1).SectionProperties[0],section.Area*1e-9);
            reader.OpenResults(run.ResultPath);
            // BXS scales the complete outline in both directions: A(t)=A0(1+t)^2; integral 0..1 = 1/2.
            double expected=1000*1000d/(1000*section.Area)/2;
            Assert.AreEqual(expected,reader.ReadNodeResult(free.Id,1,false).Values[0],expected*5e-5);
        }
        Assert.AreEqual(fingerprint,model.AnalysisFingerprint());
        Assert.AreEqual(0,Directory.GetFiles(folder,"gpc-section-*").Length,"Owned BXS scratch files must be removed after assignment.");
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeUnsymmetricSectionRetainsShearCenterAndTorsionalResponse()
    {
        var model=Cantilever();var beam=model.BeamElements.Values.Single();var section=new SectionC(200,5,100,10,100,10);
        var material=new Material("E",210000,.3,0,0);beam.BeamProperty=new ElasticBeamProperty("channel",material,section);
        Assert.AreEqual(0,section.AngleX1,1e-10);double offset=section.ShearCenter.X-section.Centroid.X;Assert.IsTrue(Math.Abs(offset)>1);
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder();var run=plan.RunLinearStatic(model,Path.Combine(folder,"channel.st7"),folder);
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath,folder);Assert.AreEqual(offset,reader.ReadProperty(Straus7Entity.Beam,1).SectionProperties[4],1e-10);
            reader.OpenResults(run.ResultPath);double expected=1000*offset*1000/(material.GetShearModule()*section.Jt);
            Assert.AreEqual(expected,reader.ReadNodeResult(beam.NodeJ.Id,1,false).Values[3],Math.Abs(expected)*1e-6);
        }
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeShearPanelMatchesUniformShearSolution()
    {
        var nodes=new[]{new NodeElement(new Point3d(0,0,0)),new NodeElement(new Point3d(100,0,0)),new NodeElement(new Point3d(100,100,0)),new NodeElement(new Point3d(0,100,0))};
        var lc=new LoadCaseBase("shear");
        foreach(var n in nodes)
        {
            n.Assignments.Restrains.Add(new RestrainAssignment {Restrain=new NodeRestrain(n,Enumerable.Range(n.Position.Y==0?0:1,n.Position.Y==0?6:5).Select(i=>new DofRestrain((GeometryRestrain.DOF)i,true)).ToList())});
            if(n.Position.Y>0)n.Loads.Add(new PointLoad(500,0,0,0,0,0,n.Position,lc));
        }
        var plate=new AreaElement(null,new ElasticPlateProperty("panel",new Material("E260",260000,.3,0,0),0,10,ElasticPlateFormulation.ShearPanel));plate.ConnectNodes(nodes);
        var model=ModelAssembly.Assemble(new Element[]{plate},Guid.NewGuid());
        var top=model.NodesElements.Values.Where(n=>n.Position.Y>0).OrderBy(n=>n.Id).ToArray();
        // A shear-only panel has no in-plane normal stiffness. Keep its loaded edge rigid to remove that free mode.
        model.AddCostrain(new GPC.Model.Constraints.RigidLink(top[0],top[1]));string fp=model.AnalysisFingerprint();
        using(var stream=new MemoryStream()){ModelArchive.SaveDocument(model,stream);stream.Position=0;Assert.AreEqual(fp,ModelArchive.Load(stream).AnalysisFingerprint());}
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder();var run=plan.RunLinearStatic(model,Path.Combine(folder,"panel.st7"),folder);
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath,folder);reader.OpenResults(run.ResultPath);
            foreach(var n in model.NodesElements.Values.Where(n=>n.Position.Y>0))Assert.AreEqual(.001,reader.ReadNodeResult(n.Id,1,false).Values[0],1e-9);
        }
    }
    [DataTestMethod,DataRow(false),DataRow(true),TestCategory("Straus7Native")]
    public void NativeCutoffLawReproducesBrittleFailureAndDuctileForceCap(bool ductile)
    {
        var model=Cantilever();var first=model.BeamElements.Values.Single();var free=first.NodeJ;var lc=model.LoadCases.Values.Single();var fc=model.FreedomCases.Values.Single();
        first.Assignments.Formulation=BeamFormulation.Truss;first.BeamProperty=new ElasticBeamProperty("elastic",new Material("E1000",1000,.3,0,0),new SectionRectangular(10,10));
        var law=new BeamCutoffLaw {Failure=ductile?BeamCutoffFailure.Ductile:BeamCutoffFailure.Brittle,TensionLimit=500,CompressionLimit=500,KeepMass=true};
        var cutoff=new BeamElement(first.StartPoint,first.EndPoint,new ElasticBeamProperty("cutoff",new Material("E1000",1000,.3,0,0),new SectionRectangular(10,10)){CutoffLaw=law},id:2);
        cutoff.ConnectNodes(first.NodeI,free);cutoff.Assignments.Formulation=BeamFormulation.CutoffBar;cutoff.Assignments.SectionAxes=first.Assignments.SectionAxes;model.BeamElements.Add(cutoff);
        free.Loads.Clear();free.Loads.Add(new PointLoad(2000,0,0,0,0,0,free.Position,lc));
        free.Assignments.Restrains.Add(new RestrainAssignment {FreedomCase=fc,Restrain=new NodeRestrain(free,Enumerable.Range(1,5).Select(i=>new DofRestrain((GeometryRestrain.DOF)i,true)).ToList())});
        var group=model.AddGroup("bars");first.Groups.Add(group.Name,group);cutoff.Groups.Add(group.Name,group);var outputCase=new StageResultCase("NLA","NLA");model.AddLoadCase(outputCase);
        model.Stages.Add(new Stage(1,"load") {Definition=new StageDefinition(1,0),AnalysisState=new StageAnalysisState {ActiveGroupGuids=new[]{group.Guid},ActiveFreedomCaseGuids=new[]{fc.Guid},
            Increments=new[]{.25,1}.Select((f,i)=>new StageLoadIncrement {Name="step "+i,LoadFactors=new[]{new StageCaseFactor {CaseGuid=lc.Guid,Factor=f}},FreedomFactors=new[]{new StageCaseFactor {CaseGuid=fc.Guid,Factor=1}}}).ToArray()}});
        string fingerprint=model.AnalysisFingerprint();foreach(bool document in new[]{false,true})using(var stream=new MemoryStream())
        {if(document)ModelArchive.SaveDocument(model,stream);else ModelArchive.Save(model,stream);stream.Position=0;Assert.AreEqual(fingerprint,ModelArchive.Load(stream).AnalysisFingerprint());}
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder();var run=plan.RunStagedNonlinear(model,outputCase.Guid,Path.Combine(folder,"cutoff.st7"),folder,nonlinearMaterial:true);
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath,folder);CollectionAssert.AreEqual(new[]{500d,500},reader.ReadCutoffLaw(2,out int failure,out bool keep));Assert.AreEqual(ductile?1:0,failure);Assert.IsTrue(keep);
            reader.OpenResults(run.ResultPath);Assert.AreEqual(2.5,reader.ReadNodeResult(free.Id,1,false).Values[0],1e-6);Assert.AreEqual(ductile?15:20,reader.ReadNodeResult(free.Id,2,false).Values[0],1e-5);
        }
    }
    [DataTestMethod,DataRow(0),DataRow(1),DataRow(2),TestCategory("Straus7Native")]
    public void NativeSurfacePressuresPreserveFacesProjectionAndForceEquilibrium(int projection)
    {
        var nodes=new[]{new NodeElement(new Point3d(0,0,0)),new NodeElement(new Point3d(100,0,0)),new NodeElement(new Point3d(100,100,100)),new NodeElement(new Point3d(0,100,100))};
        foreach(var node in nodes.Where(n=>n.Position.X==0))node.Assignments.Restrains.Add(new RestrainAssignment {Restrain=new NodeRestrain(node,Enumerable.Range(0,6).Select(i=>new DofRestrain((GeometryRestrain.DOF)i,true)).ToList())});
        var plate=new AreaElement(null,new ElasticPlateProperty("plate",new Material("E",210000,.3,0,0),10,10));plate.ConnectNodes(nodes);
        var lc=new LoadCaseBase("pressure");var vector=new Vector3d(.001,.002,-.003);
        plate.Assignments.Physical=new ShellPhysicalData {SurfacePressures=new List<ShellSurfacePressure> {
            new ShellSurfacePressure {Case=lc,Face=ShellFace.NegativeNormal,NormalPressure=.002},
            new ShellSurfacePressure {Case=lc,Face=ShellFace.PositiveNormal,NormalPressure=.001},
            new ShellSurfacePressure {Case=lc,Face=ShellFace.PositiveNormal,GlobalPressure=vector,Projection=(SurfacePressureProjection)projection}}};
        var model=ModelAssembly.Assemble(new Element[]{plate},Guid.NewGuid());string fingerprint=model.AnalysisFingerprint();
        foreach(bool document in new[]{false,true})using(var stream=new MemoryStream())
        {
            if(document)ModelArchive.SaveDocument(model,stream);else ModelArchive.Save(model,stream);stream.Position=0;var restored=ModelArchive.Load(stream);
            Assert.AreEqual(fingerprint,restored.AnalysisFingerprint());restored.AreaElements.Values.Single().Assignments.Physical.SurfacePressures[0].NormalPressure=.003;
            Assert.AreNotEqual(fingerprint,restored.AnalysisFingerprint());Assert.AreEqual(fingerprint,model.AnalysisFingerprint());
        }
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder();var run=plan.RunLinearStatic(model,Path.Combine(folder,"pressure.st7"),folder);
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath,folder);CollectionAssert.AreEqual(new[]{.002,.001},reader.ReadPlateNormalPressure(1,1));
            CollectionAssert.AreEqual(new[]{.001,.002,-.003},reader.ReadPlateGlobalPressure(1,2,1,out int mode));Assert.AreEqual(projection,mode);
            reader.OpenResults(run.ResultPath);var normal=new Vector3d(0,-1/Math.Sqrt(2),1/Math.Sqrt(2));double area=10000*Math.Sqrt(2);
            var global=projection==0?vector:projection==1?vector*(Math.Abs(normal.DotProduct(vector))/vector.Length):new Vector3d(0,vector.Y/Math.Sqrt(2),vector.Z/Math.Sqrt(2));
            var force=(global+normal*.001)*area;
            for(int i=0;i<3;i++)Assert.AreEqual(-new[]{force.X,force.Y,force.Z}[i],model.NodesElements.Values.Sum(n=>reader.ReadNodeResult(n.Id,1,true).Values[i]),1e-6);
        }
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeRigidLinkTransfersEccentricLoadAndPreservesRigidBodyKinematics()
    {
        var model=Cantilever();var beam=model.BeamElements.Values.Single();var master=beam.NodeJ;var lc=model.LoadCases.Values.Single();
        master.Loads.Clear();var slave=new NodeElement(new Point3d(1000,100,0));slave.Loads.Add(new PointLoad(1000,0,0,0,0,0,slave.Position,lc));
        var link=new GPC.Model.Constraints.RigidLink(master,slave);model.AddCostrain(link);
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder();var run=plan.RunLinearStatic(model,Path.Combine(folder,"rigid.st7"),folder);
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath,folder);reader.OpenResults(run.ResultPath);
            var a=reader.ReadNodeResult(master.Id,1,false).Values;var b=reader.ReadNodeResult(slave.Id,1,false).Values;
            Assert.AreEqual(a[0]-100*a[5],b[0],1e-9);Assert.AreEqual(a[1],b[1],1e-9);Assert.AreEqual(a[2]+100*a[3],b[2],1e-9);
            for(int i=3;i<6;i++)Assert.AreEqual(a[i],b[i],1e-10);
            var reaction=reader.ReadNodeResult(beam.NodeI.Id,1,true).Values;Assert.AreEqual(-1000,reaction[0],1e-6);Assert.AreEqual(100000,reaction[5],1e-3);
        }
        link.Links[0].Equations[0].Value=-2;
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(()=>Straus7ModelWritePlan.Prepare(model)).Message,"RigidLinkEquationsDoNotMatchCurrentGeometry");
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeTaperedBarMatchesIntegratedAxialCompliance()
    {
        var model=Cantilever();var beam=model.BeamElements.Values.Single();var free=beam.NodeJ;var lc=model.LoadCases.Values.Single();var fc=model.FreedomCases.Values.Single();
        beam.BeamProperty=new ElasticBeamProperty("tapered rectangle",new Material("E1000",1000,.3,0,0),new SectionRectangular(10,10));
        beam.Assignments.Physical.Taper=new BeamSectionTaper {ScaleI1=1,ScaleJ1=2,ScaleI2=1,ScaleJ2=1,Alignment1=TaperAlignment.Symmetric,Alignment2=TaperAlignment.Symmetric};
        free.Loads.Clear();free.Loads.Add(new PointLoad(1000,0,0,0,0,0,free.Position,lc));
        free.Assignments.Restrains.Add(new RestrainAssignment {FreedomCase=fc,Restrain=new NodeRestrain(free,Enumerable.Range(1,5).Select(i=>new DofRestrain((GeometryRestrain.DOF)i,true)).ToList())});
        var plan=Straus7ModelWritePlan.Prepare(model,null,new Dictionary<Guid,int>{{beam.Guid,10}});string folder=NativeFolder();var run=plan.RunLinearStatic(model,Path.Combine(folder,"tapered.st7"),folder);
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath,folder);reader.OpenResults(run.ResultPath);
            double expected=1000*1000d/(1000*100)*Math.Log(2);
            Assert.AreEqual(expected,reader.ReadNodeResult(free.Id,1,false).Values[0],expected*1e-5);
        }
        Console.WriteLine("F5 tapered axial compliance: "+run.ModelPath);
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeMaterialLawPreservesThermalDampingDataAndBilinearTrussResponse()
    {
        var model=Cantilever();var beam=model.BeamElements.Values.Single();var free=beam.NodeJ;var lc=model.LoadCases.Values.Single();var fc=model.FreedomCases.Values.Single();
        var data=new MaterialAnalysisData {BeamShearModulus=300,ViscousDampingDensity=1e-12,ModalDampingRatio=.03,ThermalConductivity=50,SpecificHeat=4.6e8,
            StressStrainPairs=new[]{0d,0,.005,5,.1,14.5},NonlinearResponse=MaterialNonlinearResponse.Elastoplastic};
        beam.BeamProperty=new ElasticBeamProperty("bilinear",new Material("E1000",1000,.3,0,0){AnalysisData=data},new SectionRectangular(10,10));
        beam.Assignments.Formulation=BeamFormulation.Truss;
        free.Loads.Clear();free.Loads.Add(new PointLoad(1000,0,0,0,0,0,free.Position,lc));
        free.Assignments.Restrains.Add(new RestrainAssignment {FreedomCase=fc,Restrain=new NodeRestrain(free,Enumerable.Range(1,5).Select(i=>new DofRestrain((GeometryRestrain.DOF)i,true)).ToList())});
        var group=model.AddGroup("bar");beam.Groups.Add(group.Name,group);var outputCase=new StageResultCase("NLA","NLA");model.AddLoadCase(outputCase);
        model.Stages.Add(new Stage(1,"tension") {Definition=new StageDefinition(1,0),AnalysisState=new StageAnalysisState {ActiveGroupGuids=new[]{group.Guid},ActiveFreedomCaseGuids=new[]{fc.Guid},
            Increments=new[]{new StageLoadIncrement {Name="load",LoadFactors=new[]{new StageCaseFactor {CaseGuid=lc.Guid,Factor=1}},FreedomFactors=new[]{new StageCaseFactor {CaseGuid=fc.Guid,Factor=1}}}}}});
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder();var run=plan.RunStagedNonlinear(model,outputCase.Guid,Path.Combine(folder,"bilinear.st7"),folder,nonlinearMaterial:true);
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath,folder);var material=reader.ReadProperty(Straus7Entity.Beam,1).Material;
            Assert.AreEqual(300,material[1]);Assert.AreEqual(1e-12,material[5]);Assert.AreEqual(.03,material[6]);Assert.AreEqual(.05,material[7]);Assert.AreEqual(460000,material[8]);
            CollectionAssert.AreEqual(data.StressStrainPairs,reader.ReadMaterialLaw(1,1,out var response,out var hardening));Assert.AreEqual(1,response);Assert.AreEqual(0,hardening);
            reader.OpenResults(run.ResultPath);Assert.AreEqual(55,reader.ReadNodeResult(free.Id,1,false).Values[0],1e-5);
        }
        Console.WriteLine("F5 bilinear material: "+run.ModelPath);
    }
    [DataTestMethod,DataRow(0),DataRow(1),TestCategory("Straus7Native")]
    public void NativeWriterMatchesFrozenBridgePilotWithOffsetsAndReversedConnectivity(int variant)
    {
        var source=Environment.GetEnvironmentVariable("GPC_STRAUS7_BRIDGE_PILOT");
        if(string.IsNullOrWhiteSpace(source))Assert.Inconclusive("Set GPC_STRAUS7_BRIDGE_PILOT to the verified F4 bridge artifact folder.");
        string document=Path.Combine(source!,"bridge-"+variant+".ghx");var xml=XDocument.Load(document);
        var text=xml.Descendants("item").Single(e=>e.Attribute("name")?.Value=="UserText"&&e.Value.StartsWith("<ExternalModelReference")).Value;
        ExternalModelReference reference;using(var bytes=new MemoryStream(Encoding.UTF8.GetBytes(text)))reference=ExternalModelReference.Read(bytes);
        var run=ExternalModelArchive.Open(document,reference);var model=run.Model;string fingerprint=model.AnalysisFingerprint();
        string baseline=Path.Combine(source!,"baseline-"+variant+".st7");var before=SHA256.HashData(File.ReadAllBytes(baseline));
        var oldNodes=run.SolverMap.Entries.Where(e=>e.Family==GPC.Model.Core.Identity.EntityFamily.Node).Select(e=>e.SolverId).ToArray();
        var oldBeams=run.SolverMap.Entries.Where(e=>e.Family==GPC.Model.Core.Identity.EntityFamily.Beam).Select(e=>e.SolverId).ToArray();
        var converter=new Straus7ApiConverter();var expected=converter.ReadResults(new Straus7ResultsRequest {ModelPath=baseline,ResultPath=Path.ChangeExtension(baseline,"LSA"),CaseNumbers=new[]{1},NodeNumbers=oldNodes,BeamNumbers=oldBeams});
        Assert.AreEqual(ImportStatus.Completed,expected.Status,string.Join("; ",expected.Diagnostics.Select(d=>d.Message)));
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder(),output=Path.Combine(folder,"bridge.st7"),result;
        using(var writer=new Straus7NativeWriter(output,folder)){plan.Write(writer,model);result=writer.Solve();}
        var actual=converter.ReadResults(new Straus7ResultsRequest {ModelPath=output,ResultPath=result,CaseNumbers=new[]{1},
            NodeNumbers=plan.IdentityMap.Entries.Where(e=>e.Family==GPC.Model.Core.Identity.EntityFamily.Node).Select(e=>e.SolverId).ToArray(),
            BeamNumbers=plan.IdentityMap.Entries.Where(e=>e.Family==GPC.Model.Core.Identity.EntityFamily.Beam).Select(e=>e.SolverId).ToArray()});
        Assert.AreEqual(ImportStatus.Completed,actual.Status,string.Join("; ",actual.Diagnostics.Select(d=>d.Message)));
        foreach(var entry in plan.IdentityMap.Entries)
        {
            int old=run.SolverMap.Entries.Single(e=>e.EntityGuid==entry.EntityGuid).SolverId;
            var entity=entry.Family==GPC.Model.Core.Identity.EntityFamily.Node?Straus7Entity.Node:Straus7Entity.Beam;
            foreach(var table in actual.Tables.Where(t=>t.Entity==entity&&t.Number==entry.SolverId))
            {
                var original=expected.Tables.Single(t=>t.Entity==entity&&t.Number==old&&t.Quantity==table.Quantity);
                Assert.AreEqual(original.Values.Length,table.Values.Length);
                for(int i=0;i<table.Values.Length;i++)Assert.AreEqual(original.Values[i],table.Values[i],Math.Max(1e-7,Math.Abs(original.Values[i])*2e-8),table.Quantity+" / "+entry.EntityGuid+" / "+i);
            }
        }
        Assert.AreEqual(fingerprint,model.AnalysisFingerprint());CollectionAssert.AreEqual(before,SHA256.HashData(File.ReadAllBytes(baseline)));
        Console.WriteLine("F5 full writer versus frozen F4 bridge "+variant+": "+output);
    }
    internal static Model Cantilever(bool distributed=false) => ElasticExportContractsTest.Cantilever(distributed);
    [TestMethod] public void ExportPreflightRejectsUnmappedPhysicalInputs()
    {
        var model=Cantilever();Straus7ModelWritePlan.Prepare(model);
        model.NodesElements.Values.First().Assignments.Mass=new NodalMass {Matrix6x6RowMajor=new double[36]};
        Assert.ThrowsException<NotSupportedException>(()=>Straus7ModelWritePlan.Prepare(model));
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeGravityUsesTonneMillimetreUnitsAndStalePlansDoNotWrite()
    {
        var model=Cantilever();foreach(var node in model.NodesElements.Values)node.Loads.Clear();model.LoadCases.Clear();
        var lc=new LoadCase("weight",LoadCase.LoadCaseTypes.SelfWeight);model.AddLoadCase(lc);
        model.ModelLoads.Add(new ModelGravityLoad(new Vector3d(0,0,-1),9810,lc,CoordinateSystem.Global));
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder(),output=Path.Combine(folder,"weight.st7"),result;
        using(var writer=new Straus7NativeWriter(output,folder))
        {
            string name=model.Name;model.Name="modified";Assert.ThrowsException<InvalidOperationException>(()=>plan.Write(writer,model));Assert.AreEqual(0,writer.Reader.Count(Straus7Entity.Node));model.Name=name;
            plan.Write(writer,model);result=writer.Solve();
        }
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder);reader.OpenResults(result);int id=model.NodesElements.Values.Single(n=>n.Position.X==0).Id;
            Assert.AreEqual(100*200*1000*7.85e-9*9810,reader.ReadNodeResult(id,1,true).Values[2],1e-6);
        }
    }
    [DataTestMethod,DataRow(false),DataRow(true),TestCategory("Straus7Native")]
    public void NativeNewModelSolvesCantileverAndReadsProducedResults(bool distributed)
    {
        var root=Environment.GetEnvironmentVariable("GPC_STRAUS7_WRITE");if(string.IsNullOrWhiteSpace(root))Assert.Inconclusive("Set GPC_STRAUS7_WRITE to an artifact parent.");
        string folder=Path.Combine(Path.GetFullPath(root!),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
        string output=Path.Combine(folder,"cantilever.st7"),result;
        var model=Cantilever(distributed);var plan=Straus7ModelWritePlan.Prepare(model);
        using(var writer=new Straus7NativeWriter(output,folder)) {plan.Write(writer,model);result=writer.Solve();}
        int fixedId=model.NodesElements.Values.Single(n=>n.Position.X==0).Id,freeId=model.NodesElements.Values.Single(n=>n.Position.X==1000).Id;
        var report=new Straus7ApiConverter().ReadResults(new Straus7ResultsRequest {ModelPath=output,ResultPath=result,CaseNumbers=new[]{1},
            BeamNumbers=new[]{1},NodeNumbers=new[]{fixedId,freeId},BeamMinimumStations=plan.MinimumOutputStations});
        Assert.AreEqual(ImportStatus.Completed,report.Status,string.Join("; ",report.Diagnostics.Select(d=>d.Message)));
        double displacement=report.Tables.Single(t=>t.Number==freeId&&t.Quantity=="NodeDisplacementGlobal").Values[2];
        var property=(ElasticBeamProperty)model.BeamElements.Values.Single().BeamProperty;double ei=property.Material.E*property.Section.J11;
        // Euler-Bernoulli cantilever, no shear deformation: PL^3/(3EI) or qL^4/(8EI).
        double expected=distributed?-2*Math.Pow(1000,4)/(8*ei):-1000*Math.Pow(1000,3)/(3*ei);
        Assert.AreEqual(expected,displacement,Math.Abs(expected)*1e-7);
        var reaction=report.Tables.Single(t=>t.Number==fixedId&&t.Quantity=="NodeReactionGlobal").Values;
        Assert.AreEqual(distributed?2000:1000,reaction[2],1e-6);Assert.AreEqual(-1000000,reaction[4],1e-3);
        Assert.IsTrue(report.Tables.Single(t=>t.Quantity=="BeamForceGlobal").Rows>=17);
        Console.WriteLine("F5 new-model cantilever: "+output+"; displacement="+displacement.ToString("R"));
    }
    private static string NativeFolder()
    {
        var root=Environment.GetEnvironmentVariable("GPC_STRAUS7_WRITE");if(string.IsNullOrWhiteSpace(root))Assert.Inconclusive("Set GPC_STRAUS7_WRITE to an artifact parent.");
        string folder=Path.Combine(Path.GetFullPath(root!),Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);return folder;
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void ExportedResultsAttachByStableIdentityAndSurviveArchiveRoundTrip()
    {
        var model=Cantilever();var beam=model.BeamElements.Values.Single();
        beam.Source=new GPC.Model.Core.Identity.SourceIdentity("designer","revision-1",GPC.Model.Core.Identity.EntityFamily.Beam,"girder-A");
        string input=model.AnalysisFingerprint();var source=beam.Source;var plan=Straus7ModelWritePlan.Prepare(model);
        string folder=NativeFolder();var run=plan.RunLinearStatic(model,Path.Combine(folder,"archive-run.st7"),folder);
        var imported=run.ImportResults(model,"export-analysis",includeElementNodeForces:true);
        Assert.AreEqual(ImportStatus.Completed,imported.Status,string.Join("; ",imported.Diagnostics.Select(d=>d.Message)));
        Assert.AreSame(source,beam.Source);Assert.AreEqual(input,model.AnalysisFingerprint());Assert.IsNull(model.AnalysisSource);
        Assert.AreEqual(run.ResultHash,model.Datasets["export-analysis"].SourceHashes["results"]);
        var samples=beam.Results.SelectMany(r=>r.Results).OfType<GPC.Model.Results.Locations.StationResultBeamForces>().ToArray();
        Assert.IsTrue(samples.Length>=17);Assert.IsTrue(samples.All(s=>s.State.InputFingerprint==input&&s.Body==GPC.Model.Results.State.ActionBody.PositiveSectionFace));
        var root=samples.Single(s=>s.ParametricDistance==0);Assert.AreEqual(1000000,Math.Abs(root.ResultBeamForces.M1),1e-3);
        string resultFingerprint=ModelValues.Fingerprint(samples);
        using(var bytes=new MemoryStream())
        {
            ModelArchive.SaveDocument(model,bytes);bytes.Position=0;var reopened=ModelArchive.Load(bytes);
            Assert.AreEqual(input,reopened.AnalysisFingerprint());Assert.AreEqual("girder-A",reopened.BeamElements.Values.Single().Source.OriginalId);
            Assert.AreEqual(resultFingerprint,ModelValues.Fingerprint(reopened.BeamElements.Values.Single().Results.SelectMany(r=>r.Results).OfType<GPC.Model.Results.Locations.StationResultBeamForces>()));
        }
        Assert.AreEqual(ImportStatus.Rejected,run.ImportResults(model,"export-analysis").Status);
        model.NodesElements.Values.First().Position.Y+=1;
        Assert.AreEqual(ImportStatus.Rejected,run.ImportResults(model,"stale").Status);Assert.IsFalse(model.Datasets.ContainsKey("stale"));
        Console.WriteLine("F5 exported result/archive binding: "+run.ModelPath+"; samples="+imported.ImportedSamples);
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeMovingPathStaticCasesMatchBeamPointLoadSolutions()
    {
        var model=Cantilever();foreach(var node in model.NodesElements.Values)node.Loads.Clear();
        var lc=model.LoadCases.Values.Single();var path=new MovingLoadPath {Id=Guid.NewGuid(),CaseGuid=lc.Guid,Divisions=4,
            Start=new Point3d(0,0,0),End=new Point3d(1000,0,0),Reference=new Point3d(0,100,0),Target=MovingPathTarget.Beams,
            Template=new MovingLoadTemplate {Name="single axle",Vehicles=new[]{new MovingVehicle {Name="P1000",PointForces=new[]{new MovingPointForce {Force=new Vector3d(0,0,-1000),Axes=MovingLoadAxes.Global}}}}}};
        model.MovingLoads=new MovingLoadDefinition();model.MovingLoads.Paths.Add(path);
        var plan=Straus7ModelWritePlan.Prepare(model,new Dictionary<Guid,Straus7MovingPathOptions>{{path.Id,new Straus7MovingPathOptions {ConvertToStaticCases=true}}});
        string folder=NativeFolder(),output=Path.Combine(folder,"moving.st7"),result;IReadOnlyDictionary<int,string> generated;
        using(var writer=new Straus7NativeWriter(output,folder))
        {
            generated=plan.Write(writer,model);writer.Save();
            Console.WriteLine("Moving cases in "+output+": "+string.Join("; ",generated.Select(c=>c.Key+"="+c.Value)));
            result=writer.Solve();
        }
        var fixedNode=model.NodesElements.Values.Single(n=>n.Position.X==0);var free=model.NodesElements.Values.Single(n=>n.Position.X==1000);
        var property=(ElasticBeamProperty)model.BeamElements.Values.Single().BeamProperty;double ei=property.Material.E*property.Section.J11;var positions=new List<double>();
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder);var cases=reader.OpenResults(result);
            foreach(int c in generated.Keys)
            {
                var reaction=reader.ReadNodeResult(fixedNode.Id,c,true).Values;
                if(Math.Abs(reaction[2])<1e-7)continue;
                Assert.AreEqual(1000,reaction[2],1e-6);double a=-reaction[4]/1000;positions.Add(a);
                Console.WriteLine("Moving case "+c+" position="+a.ToString("R"));
                Assert.IsTrue(a>=-1e-7&&a<=1000+1e-7);
                double expected=-1000*a*a*(3000-a)/(6*ei);
                Assert.AreEqual(expected,reader.ReadNodeResult(free.Id,c,false).Values[2],Math.Max(1e-9,Math.Abs(expected)*1e-7));
            }
        }
        // R31 converts one position at the centre of each path division, rather than its boundaries.
        Assert.AreEqual(4,generated.Count);Assert.AreEqual(4,positions.Count);
        foreach(double expected in new[]{125d,375,625,875})Assert.IsTrue(positions.Any(p=>Math.Abs(p-expected)<1e-6),"Missing moving-load position "+expected);
        Console.WriteLine("F5 moving static solutions: "+output+"; cases="+generated.Count);
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void GeneratedMovingResultsRetainDistinctPositionsWithoutChangingLoadDefinitions()
    {
        var model=Cantilever();foreach(var n in model.NodesElements.Values)n.Loads.Clear();var lc=model.LoadCases.Values.Single();
        var path=new MovingLoadPath {Id=Guid.NewGuid(),CaseGuid=lc.Guid,Divisions=3,Start=new Point3d(0,0,0),End=new Point3d(1000,0,0),Reference=new Point3d(0,100,0),Target=MovingPathTarget.Beams,
            Template=new MovingLoadTemplate {Name="axle",Vehicles=new[]{new MovingVehicle {Name="vehicle",PointForces=new[]{new MovingPointForce {Force=new Vector3d(0,0,-1000),Axes=MovingLoadAxes.Global}}}}}};
        model.MovingLoads=new MovingLoadDefinition();model.MovingLoads.Paths.Add(path);
        var plan=Straus7ModelWritePlan.Prepare(model,new Dictionary<Guid,Straus7MovingPathOptions>{{path.Id,new Straus7MovingPathOptions {ConvertToStaticCases=true}}});
        string folder=NativeFolder(),before=model.AnalysisFingerprint();var run=plan.RunLinearStatic(model,Path.Combine(folder,"moving-results.st7"),folder);
        Assert.AreEqual(3,run.GeneratedCases.Count);var report=run.ImportResults(model,"moving-results",includeElementNodeForces:true);
        Assert.AreEqual(ImportStatus.Completed,report.Status,string.Join("; ",report.Diagnostics.Select(d=>d.Message)));
        Assert.AreEqual(before,model.AnalysisFingerprint());Assert.AreEqual(1,model.LoadCases.Count);
        var samples=model.BeamElements.Values.Single().Results.SelectMany(r=>r.Results).OfType<GPC.Model.Results.Locations.StationResultBeamForces>().ToArray();
        Assert.AreEqual(3,samples.Where(s=>s.State.Step!=null).Select(s=>s.State.Step).Distinct().Count());
        Assert.IsTrue(samples.All(s=>s.State.Semantics==GPC.Model.Analysis.AnalysisSemantics.LinearStatic));
        VerifyResultArchive(model,samples);
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void StagedRunImportsTotalsWithPhaseAndIncrementAndSurvivesArchive()
    {
        var model=Cantilever();var group=model.AddGroup("structure");model.BeamElements.Values.Single().Groups.Add(group.Name,group);
        var lc=model.LoadCases.Values.Single();var fc=model.FreedomCases.Values.Single();
        var outputCase=new StageResultCase("Construction response","NLA");model.AddLoadCase(outputCase);
        model.Stages.Add(new Stage(1,"Load and unload") {Definition=new StageDefinition(1,0),AnalysisState=new StageAnalysisState {
            ActiveGroupGuids=new[]{group.Guid},ActiveFreedomCaseGuids=new[]{fc.Guid},Increments=new[]{.5,1,0}.Select((f,i)=>new StageLoadIncrement {
                Name="Step "+i,LoadFactors=new[]{new StageCaseFactor {CaseGuid=lc.Guid,Factor=f}},FreedomFactors=new[]{new StageCaseFactor {CaseGuid=fc.Guid,Factor=1}}}).ToArray()}});
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder(),before=model.AnalysisFingerprint();
        var run=plan.RunStagedNonlinear(model,outputCase.Guid,Path.Combine(folder,"stage-results.st7"),folder);
        var report=run.ImportResults(model,"staged-results",includeElementNodeForces:true);
        Assert.AreEqual(ImportStatus.Completed,report.Status,string.Join("; ",report.Diagnostics.Select(d=>d.Message)));Assert.AreEqual(before,model.AnalysisFingerprint());
        var samples=model.BeamElements.Values.Single().Results.SelectMany(r=>r.Results).OfType<GPC.Model.Results.Locations.StationResultBeamForces>().ToArray();
        Assert.AreEqual(3,samples.Select(s=>s.State.Step).Distinct().Count());
        Assert.IsTrue(samples.All(s=>s.State.Semantics==GPC.Model.Analysis.AnalysisSemantics.NonlinearStatic&&s.State.Phase=="Load and unload"&&s.State.IsCumulative==true&&s.State.IsCombined==false));
        VerifyResultArchive(model,samples);
    }
    private static void VerifyResultArchive(Model model,GPC.Model.Results.Locations.StationResultBeamForces[] samples)
    {
        string results=ModelValues.Fingerprint(samples);
        using(var stream=new MemoryStream())
        {
            ModelArchive.Save(model,stream);stream.Position=0;var copy=ModelArchive.Load(stream);
            Assert.AreEqual(model.AnalysisFingerprint(),copy.AnalysisFingerprint());
            Assert.AreEqual(results,ModelValues.Fingerprint(copy.BeamElements.Values.Single().Results.SelectMany(r=>r.Results).OfType<GPC.Model.Results.Locations.StationResultBeamForces>()));
        }
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeLoadPatchTransfersPressureToBeamsAndKeepsStringGroups()
    {
        var nodes=new[]{new NodeElement(new Point3d(0,0,0)),new NodeElement(new Point3d(1000,0,0)),new NodeElement(new Point3d(1000,100,0)),new NodeElement(new Point3d(0,100,0))};
        var elements=new List<Element>();var property=new ElasticBeamProperty("rectangle",new Material("E",210000,.3,0,0),new SectionRectangular(200,100));var stringGuid=Guid.NewGuid();
        foreach(var n in nodes.Where(n=>n.Position.X==0))n.Assignments.Restrains.Add(new RestrainAssignment {Restrain=new NodeRestrain(n,Enumerable.Range(0,6).Select(i=>new DofRestrain((GeometryRestrain.DOF)i,true)).ToList())});
        for(int i=0;i<4;i++)
        {
            var a=nodes[i];var b=nodes[(i+1)%4];Vector3d tangent=b.Position-a.Position;tangent/=tangent.Length;
            var vertical=new Vector3d(0,0,1);var side=vertical.CrossProduct(tangent);
            var beam=new BeamElement(a.Position,b.Position,property);beam.ConnectNodes(a,b);beam.Assignments.Formulation=BeamFormulation.StraightTwoNode;
            beam.Assignments.SectionAxes=new CoordinateSystem(a.Position,side,vertical);beam.Assignments.Physical=new BeamPhysicalData {StringGroup=stringGuid};elements.Add(beam);
        }
        var patch=new AreaElement(null,new ElasticPlateProperty("load patch",null,0,0,ElasticPlateFormulation.LoadPatch));patch.ConnectNodes(nodes);
        patch.Assignments.Physical=new ShellPhysicalData {LoadPatch=new LoadPatchDistribution()};
        patch.Loads.Add(new AreaLoad(0,0,-.001,patch.Shape,new LoadCaseBase("pressure")));elements.Add(patch);
        var model=ModelAssembly.Assemble(elements,Guid.NewGuid());string before=model.AnalysisFingerprint();
        using(var bytes=new MemoryStream()){ModelArchive.SaveDocument(model,bytes);bytes.Position=0;model=ModelArchive.Load(bytes);Assert.AreEqual(before,model.AnalysisFingerprint());}
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder(),output=Path.Combine(folder,"patch.st7"),result;
        using(var writer=new Straus7NativeWriter(output,folder)){plan.Write(writer,model);result=writer.Solve();}
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder);reader.ReadLoadPatch(1,out int kind,out int bits);Assert.AreEqual(0,kind);Assert.AreEqual(0,bits);
            for(int i=1;i<=4;i++)Assert.AreEqual(1,reader.ReadBeamStringGroup(i));
            reader.OpenResults(result);double reaction=model.NodesElements.Values.Where(n=>n.Position.X==0).Sum(n=>reader.ReadNodeResult(n.Id,1,true).Values[2]);
            Assert.AreEqual(100,reaction,1e-6);
        }
        Console.WriteLine("F5 load patch and string group: "+output);
    }
    [DataTestMethod,DataRow(false),DataRow(true),TestCategory("Straus7Native")]
    public void NativeOrthotropicMembraneRespectsMaterialAxes(bool rotate)
        => SolveMembranePatch(rotate,false);
    [DataTestMethod,DataRow(false),DataRow(true),TestCategory("Straus7Native")]
    public void NativePerWidthMembranePreservesStiffnessMassAndMaterialAxes(bool rotate)
        => SolveMembranePatch(rotate,true);

    private static void SolveMembranePatch(bool rotate,bool perWidth)
    {
        var nodes=new[]{new NodeElement(new Point3d(0,0,0)),new NodeElement(new Point3d(1000,0,0)),new NodeElement(new Point3d(1000,100,0)),new NodeElement(new Point3d(0,100,0))};
        var material=new Material("ortho",210000,0,1e-9,0) {Orthotropy=new OrthotropicElasticity {E1=210000,E2=70000,E3=70000,G12=30000,G23=30000,G31=30000}};
        var plate=new AreaElement(null,new ElasticPlateProperty("membrane",material,0,10,ElasticPlateFormulation.Membrane));plate.ConnectNodes(nodes);
        if(perWidth)
        {
            plate.PlateProperty=null;
            plate.Assignments.Physical=new ShellPhysicalData {Membrane=new MembraneMaterialData {
                StiffnessWarp=2100000,StiffnessWeft=700000,ShearStiffness=300000,NuWarpWeft=0,MassPerArea=1e-8}};
        }
        plate.Assignments.LayerAxes=rotate?new CoordinateSystem(Point3d.Origin,new Vector3d(0,1,0),new Vector3d(-1,0,0)):CoordinateSystem.Global;
        var lc=new LoadCaseBase("tension");
        foreach(var n in nodes)
        {
            var fixedDofs=n.Position.X==0?new[]{0,1,2,3,4,5}:new[]{2,3,4,5};
            n.Assignments.Restrains.Add(new RestrainAssignment {Restrain=new NodeRestrain(n,fixedDofs.Select(i=>new DofRestrain((GeometryRestrain.DOF)i,true)).ToList())});
            if(n.Position.X==1000)n.Loads.Add(new PointLoad(500,0,0,0,0,0,n.Position,lc));
        }
        var model=ModelAssembly.Assemble(new Element[]{plate},Guid.NewGuid());
        if(perWidth)
        {
            var weight=new LoadCase("weight",LoadCase.LoadCaseTypes.SelfWeight);model.AddLoadCase(weight);
            model.ModelLoads.Add(new ModelGravityLoad(new Vector3d(0,0,-1),9810,weight,CoordinateSystem.Global));
            var copy=ModelArchive.CopyValue(model);Assert.AreEqual(model.AnalysisFingerprint(),copy.AnalysisFingerprint());
            copy.AreaElements.Values.Single().Assignments.LayerAxes=rotate?CoordinateSystem.Global:new CoordinateSystem(Point3d.Origin,new Vector3d(0,1,0),new Vector3d(-1,0,0));
            Assert.AreNotEqual(model.AnalysisFingerprint(),copy.AnalysisFingerprint());
        }
        var plan=Straus7ModelWritePlan.Prepare(model);
        string folder=NativeFolder(),output=Path.Combine(folder,"membrane.st7"),result;
        using(var writer=new Straus7NativeWriter(output,folder)){plan.Write(writer,model);result=writer.Solve();}
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder);reader.OpenResults(result);
            double expected=1000*1000/((rotate?70000d:210000)*100*10);
            foreach(var n in model.NodesElements.Values.Where(n=>n.Position.X==1000))Assert.AreEqual(expected,reader.ReadNodeResult(n.Id,1,false).Values[0],expected*1e-7);
            if(perWidth)Assert.AreEqual(1000*100*1e-8*9810,model.NodesElements.Values.Sum(n=>reader.ReadNodeResult(n.Id,2,true).Values[2]),1e-7);
        }
        Console.WriteLine("F5 membrane patch test: "+output+"; rotated="+rotate);
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeStagedCantileverUsesTotalLoadFactorsAndUnloads()
    {
        var model=Cantilever();var group=model.AddGroup("Structure");var beam=model.BeamElements.Values.Single();beam.Groups.Add(group.Name,group);
        var lc=model.LoadCases.Values.Single();var fc=model.FreedomCases.Values.Single();var factors=new[]{.5,1,0};
        model.Stages.Add(new Stage(1,"Load and unload") {Definition=new StageDefinition(1,0),AnalysisState=new StageAnalysisState {
            ActiveGroupGuids=new[]{group.Guid},ActiveFreedomCaseGuids=new[]{fc.Guid},
            Increments=factors.Select((f,i)=>new StageLoadIncrement {Name="Step "+i,LoadFactors=new[]{new StageCaseFactor {CaseGuid=lc.Guid,Factor=f}},FreedomFactors=new[]{new StageCaseFactor {CaseGuid=fc.Guid,Factor=1}}}).ToArray()}});
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder(),output=Path.Combine(folder,"stages.st7"),result;
        using(var writer=new Straus7NativeWriter(output,folder)){plan.Write(writer,model);result=writer.Solve(stagedNonlinear:true);}
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder);var results=reader.OpenResults(result);Assert.AreEqual(3,results.PrimaryCases);
            var property=(ElasticBeamProperty)beam.BeamProperty;double full=-1000*Math.Pow(1000,3)/(3*property.Material.E*property.Section.J11);
            int id=model.NodesElements.Values.Single(n=>n.Position.X==1000).Id;
            for(int i=0;i<3;i++)Assert.AreEqual(full*factors[i],reader.ReadNodeResult(id,i+1,false).Values[2],1e-8);
        }
        Console.WriteLine("F5 NLA total factors and unloading: "+output);
    }
    [DataTestMethod,DataRow(0),DataRow(1),DataRow(2),TestCategory("Straus7Native")]
    public void NativeBirthGeometryChangesLoadSharingAsSpecified(int birth)
    {
        var model=Cantilever();var first=model.BeamElements.Values.Single();var free=model.NodesElements.Values.Single(n=>n.Position.X==1000);
        var lc=model.LoadCases.Values.Single();var fc=model.FreedomCases.Values.Single();free.Loads.Clear();free.Loads.Add(new PointLoad(1000,0,0,0,0,0,free.Position,lc));
        free.Assignments.Restrains.Add(new RestrainAssignment {FreedomCase=fc,Restrain=new NodeRestrain(free,Enumerable.Range(1,5).Select(i=>new DofRestrain((GeometryRestrain.DOF)i,true)).ToList())});
        first.BeamProperty=new ElasticBeamProperty("axial",new Material("E1000",1000,.3,0,0),new SectionRectangular(200,100));
        var second=new BeamElement(first.StartPoint,first.EndPoint,first.BeamProperty,id:2);second.ConnectNodes(first.NodeI,first.NodeJ);
        second.Assignments.Formulation=BeamFormulation.StraightTwoNode;second.Assignments.SectionAxes=first.Assignments.SectionAxes;model.BeamElements.Add(second);
        var g1=model.AddGroup("Existing");var g2=model.AddGroup("Incoming");first.Groups.Add(g1.Name,g1);second.Groups.Add(g2.Name,g2);
        for(int i=1;i<=2;i++)model.Stages.Add(new Stage(i,"Phase "+i) {Definition=new StageDefinition(i,0),AnalysisState=new StageAnalysisState {
            BirthGeometry=i==1?StageBirthGeometry.OriginalCoordinates:(StageBirthGeometry)birth,
            ActiveGroupGuids=i==1?new[]{g1.Guid}:new[]{g1.Guid,g2.Guid},ActiveFreedomCaseGuids=new[]{fc.Guid},
            Increments=new[]{new StageLoadIncrement {Name="Equilibrium",LoadFactors=new[]{new StageCaseFactor {CaseGuid=lc.Guid,Factor=1}},FreedomFactors=new[]{new StageCaseFactor {CaseGuid=fc.Guid,Factor=1}}}}}});
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder(),output=Path.Combine(folder,"birth.st7"),result;
        using(var writer=new Straus7NativeWriter(output,folder)){plan.Write(writer,model);result=writer.Solve(stagedNonlinear:true);}
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder);var cases=reader.OpenResults(result);Assert.AreEqual(2,cases.PrimaryCases);
            Assert.AreEqual(.05,reader.ReadNodeResult(free.Id,1,false).Values[0],1e-10);
            // Original/reset: two equal bars share P. Morphed: the incoming bar is stress-free at u=P/k.
            Assert.AreEqual(birth==1?.05:.025,reader.ReadNodeResult(free.Id,2,false).Values[0],1e-9);
        }
        Console.WriteLine("F5 birth geometry "+(StageBirthGeometry)birth+": "+output);
    }
    [TestMethod,TestCategory("Straus7Native")]
    public void NativeBeamLoadEccentricityIsMeasuredFromNodeLine()
    {
        var model=Cantilever();var beam=model.BeamElements.Values.Single();foreach(var node in model.NodesElements.Values)node.Loads.Clear();
        beam.Assignments.OffsetI=beam.Assignments.OffsetJ=new Vector3d(100,0,0);beam.Assignments.OffsetAxes=beam.Assignments.SectionAxes;
        beam.Assignments.Loads.Add(new BeamLoadAssignment {Start=.5,End=.5,Eccentricity=new Vector3d(0,25,0),
            Concentrated=new PointLoad(0,0,-1000,0,0,0,new Point3d(500,0,0),model.LoadCases.Values.Single())});
        var plan=Straus7ModelWritePlan.Prepare(model);string folder=NativeFolder(),output=Path.Combine(folder,"eccentric-load.st7"),result;
        using(var writer=new Straus7NativeWriter(output,folder)){plan.Write(writer,model);result=writer.Solve();}
        using(var reader=new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output,folder);reader.OpenResults(result);
            var r=reader.ReadNodeResult(model.NodesElements.Values.Single(n=>n.Position.X==0).Id,1,true).Values;
            Assert.AreEqual(1000,r[2],1e-6);Assert.AreEqual(25000,r[3],1e-3);Assert.AreEqual(-500000,r[4],1e-3);
        }
    }
}
