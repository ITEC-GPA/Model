using GPC.Model.Structure.Assignments;
using GPC.Examples;
using GPC.Geometry;
using GPC.Model.ElementProperties;
using GPC.Model.Persistence;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using GPC.Model.Results.Locations;
using GPC.Model.Models;
using GPC.Model.Analysis;
using GPC.Model.Checking.Preparation;
using GPC.Model.Checking.Scenarios;
using GPC.Model.Core.Diagnostics;

namespace UnitTest;

[TestClass]
public class PlateSectionTest
{
    internal static ReinforcedConcretePlateSection Section(double thickness=300)
    {
        var shell=MixedModelFactory.Create().AreaElements[1090];
        return new ReinforcedConcretePlateSection(((ConcretePlateProperty)shell.PlateProperty).ConcreteMaterial,
            thickness,240,300,shell.Assignments.Layers,"RC plate");
    }
    [TestMethod]
    public void SectionOwnsReinforcementAndIndependentThicknesses()
    {
        var section=Section(); Assert.AreEqual(300,section.PhysicalThickness); Assert.AreEqual(240,section.BendingThickness);
        Assert.IsTrue(section.RebarLayers.Count>0);
        var copy=ModelArchive.CopyValue(section);
        Assert.AreEqual(ModelArchive.Fingerprint(new object[]{section}),ModelArchive.Fingerprint(new object[]{copy}));
        copy.RebarLayers[0].Pitch*=2;
        Assert.AreNotEqual(section.RebarLayers[0].Pitch,copy.RebarLayers[0].Pitch);
        Assert.IsFalse(section.Equals(copy));
        var plain=new ConcretePlateProperty(section.ConcreteMaterial,240,300,section.Name){PhysicalThickness=300};
        Assert.IsFalse(plain.Equals(section)); Assert.IsFalse(section.Equals(plain));
    }

    private static PointResultPlateForces Sample(GPC.Model.Elements.AreaElement element) => element.Results.SelectMany(r=>r.Results).OfType<PointResultPlateForces>().First();
    private static string Errors(ShellInputPreparation p) => string.Join(";",p.Diagnostics.Select(d=>d.Code+":"+d.Message));

    [DataTestMethod]
    [DataRow(ReinforcementAnalysisRole.Unknown)]
    [DataRow(ReinforcementAnalysisRole.IncludedInAnalysis)]
    [DataRow(ReinforcementAnalysisRole.ExcludedFromAnalysis)]
    public void MovingUnchangedLegacyDataIntoTheSectionPreservesFemProvenance(ReinforcementAnalysisRole role)
    {
        var model=MixedModelFactory.Create(reinforcementRole:role); var shell=model.AreaElements[1090];
        var physical=model.AnalysisFingerprint(); var reinforcement=AnalysisStorage.Reinforcement(model);
        var original=ShellInputPreparation.Prepare(model,shell.Id,Sample(shell),"");
        Assert.AreEqual(DataStatus.Ready,original.Status,Errors(original));
        PlateSections.UpgradeLegacy(shell);
        Assert.AreEqual(physical,model.AnalysisFingerprint()); Assert.AreEqual(reinforcement,AnalysisStorage.Reinforcement(model));
        Assert.AreEqual(AnalysisCompatibility.Compatible,AnalysisCompatibilityValidator.Validate(model).Status);
        Assert.AreEqual(0,shell.Assignments.Layers.Count); Assert.IsNull(shell.Assignments.PhysicalThickness);
        var prepared=ShellInputPreparation.Prepare(model,shell.Id,Sample(shell),"");
        Assert.AreEqual(DataStatus.Ready,prepared.Status,Errors(prepared));
        Assert.AreEqual(original.Input.Thickness.Physical,prepared.Input.Thickness.Physical);
        Assert.AreEqual(original.Input.Reinforcement[0].Pitch,prepared.Input.Reinforcement[0].Pitch);
        Assert.IsFalse(original.Input.IsCurrent,"Old prepared ownership changes even when FEM assumptions are equivalent.");
        Assert.AreNotSame(shell.PlateProperty,prepared.Input.Property);
    }
    [DataTestMethod]
    [DataRow(ReinforcementAnalysisRole.Unknown,AnalysisCompatibility.Unknown)]
    [DataRow(ReinforcementAnalysisRole.IncludedInAnalysis,AnalysisCompatibility.RequiresReanalysis)]
    [DataRow(ReinforcementAnalysisRole.ExcludedFromAnalysis,AnalysisCompatibility.Compatible)]
    public void SectionRebarChangesRespectTheRecordedAnalysisRole(ReinforcementAnalysisRole role,AnalysisCompatibility expected)
    {
        var model=MixedModelFactory.Create(m=>PlateSections.UpgradeLegacy(m.AreaElements[1090]),role);
        var shell=model.AreaElements[1090]; var prepared=ShellInputPreparation.Prepare(model,shell.Id,Sample(shell),"");
        Assert.AreEqual(DataStatus.Ready,prepared.Status,Errors(prepared));
        var section=(ReinforcedConcretePlateSection)shell.PlateProperty;
        double oldPitch=section.RebarLayers[0].Pitch; section.RebarLayers[0].Pitch=200;
        Assert.AreEqual(expected,AnalysisCompatibilityValidator.Validate(model).Status);
        Assert.IsFalse(prepared.Input.IsCurrent); Assert.AreEqual(oldPitch,prepared.Input.Reinforcement[0].Pitch);
    }
    [TestMethod]
    public void SectionCopiesAndThicknessEditsAreObserved()
    {
        var model=MixedModelFactory.Create(m=>PlateSections.UpgradeLegacy(m.AreaElements[1090])); var shell=model.AreaElements[1090];
        var prepared=ShellInputPreparation.Prepare(model,shell.Id,Sample(shell),"");
        prepared.Input.Reinforcement[0].Pitch+=1;
        Assert.IsFalse(prepared.Input.IsCurrent);
        Assert.AreEqual(150,((ReinforcedConcretePlateSection)shell.PlateProperty).RebarLayers[0].Pitch);
        shell.PlateProperty.PhysicalThickness+=10;
        Assert.AreEqual(AnalysisCompatibility.RequiresReanalysis,AnalysisCompatibilityValidator.Validate(model).Status);
    }
    [TestMethod]
    public void NewSectionsSurviveDocumentRoundTripWithIndependentAnalysis()
    {
        var model=MixedModelFactory.Create(m=>PlateSections.UpgradeLegacy(m.AreaElements[1090]));
        var copy=ModelArchive.Copy(model); var shell=copy.AreaElements[1090];
        Assert.IsInstanceOfType(shell.PlateProperty,typeof(ReinforcedConcretePlateSection));
        Assert.AreEqual(0,shell.Assignments.Layers.Count);
        Assert.AreEqual(AnalysisCompatibility.Compatible,AnalysisCompatibilityValidator.Validate(copy).Status);
        Assert.AreEqual(DataStatus.Ready,ShellInputPreparation.Prepare(copy,shell.Id,Sample(shell),"").Status);
        ((ReinforcedConcretePlateSection)shell.PlateProperty).RebarLayers[0].Pitch=500;
        Assert.AreEqual(150,((ReinforcedConcretePlateSection)copy.Analysis.OpenModel().AreaElements[1090].PlateProperty).RebarLayers[0].Pitch);
    }
    [TestMethod]
    public void ConflictingLegacyAndSectionDataAreRejected()
    {
        var model=MixedModelFactory.Create(m=>PlateSections.UpgradeLegacy(m.AreaElements[1090])); var shell=model.AreaElements[1090];
        shell.Assignments.PhysicalThickness=300;
        Assert.IsTrue(model.ValidateAssignments().Any(d=>d.Code=="ConflictingShellThicknessStorage"));
        Assert.AreNotEqual(DataStatus.Ready,ShellInputPreparation.Prepare(model,shell.Id,Sample(shell),"").Status);
        shell.Assignments.PhysicalThickness=null;
        shell.Assignments.Layers.Add(((ReinforcedConcretePlateSection)shell.PlateProperty).RebarLayers[0]);
        Assert.IsTrue(model.ValidateAssignments().Any(d=>d.Code=="ConflictingShellReinforcementStorage"));
    }
    [TestMethod]
    public void ScenarioCapturesPlateSectionsWithoutChangingTheSource()
    {
        var model=MixedModelFactory.Create(reinforcementRole:ReinforcementAnalysisRole.ExcludedFromAnalysis);
        var shell=model.AreaElements[1090]; var sourceCopy=ModelArchive.Copy(model); PlateSections.UpgradeLegacy(sourceCopy.AreaElements[1090]);
        var section=(ReinforcedConcretePlateSection)sourceCopy.AreaElements[1090].PlateProperty; section.RebarLayers[0].Pitch=200;
        var scenario=VerificationScenario.Create(model.Analysis.Id,"Plate bars"); scenario.SetShellSection(shell.Guid,section);
        string fingerprint=scenario.Fingerprint; section.RebarLayers[0].Pitch=400;
        Assert.AreEqual(fingerprint,scenario.Fingerprint);
        var prepared=VerificationPreparation.Prepare(model,scenario);
        Assert.IsTrue(prepared.IsCurrent,prepared.Compatibility.Message);
        Assert.AreEqual(200,((ReinforcedConcretePlateSection)prepared.Model.AreaElements[1090].PlateProperty).RebarLayers[0].Pitch);
        Assert.AreEqual(150,shell.Assignments.Layers[0].Pitch);
        var copy=ModelArchive.Copy(prepared.Model); Assert.IsTrue(copy.VerificationContext.IsCurrent(copy));
        Assert.AreEqual(fingerprint,copy.VerificationScenarios[scenario.Id].Fingerprint);
    }
    [TestMethod]
    public void InvalidSectionReinforcementBlocksEditPublication()
    {
        var model=MixedModelFactory.Create(m=>PlateSections.UpgradeLegacy(m.AreaElements[1090]));
        using var edit=new ModelEditSession(model);
        ((ReinforcedConcretePlateSection)edit.Draft.AreaElements[1090].PlateProperty).RebarLayers[0].AxisPositionThroughThickness=150;
        Assert.IsTrue(edit.Validate().Any(d=>d.Code=="InvalidShellRebarLayer"));
        Assert.ThrowsException<InvalidOperationException>(()=>edit.Commit());
    }
}
