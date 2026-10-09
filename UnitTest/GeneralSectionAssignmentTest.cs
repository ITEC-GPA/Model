using GPC.Model.Models;
using GPC.Examples;
using GPC.Model.Data.Steel;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using GPC.Model.Checking.Preparation;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Structure.Assignments;
using GPC.Model.Structure.Topology;

namespace UnitTest;

[TestClass]
public class GeneralSectionAssignmentTest
{
    private static SteelSection Steel(double height = 300) => new SteelSection(
        new SectionH(height, 12, 150, 18, 150, 18, "H " + height), SteelMaterialEN1993Data.S355);

    [TestMethod]
    public void ConstantAssignmentsResolveDifferentMaterialsOnTheTwoSidesOfACut()
    {
        var concrete = (ReinforcedConcreteSection)MixedModelFactory.Create().BeamElements[250].BeamProperty;
        var steel = Steel();
        var assignments = new BeamAssignments();
        assignments.Sections.Add(new BeamSectionAssignment { Start = 0, End = .5, Property = steel });
        assignments.Sections.Add(new BeamSectionAssignment { Start = .5, End = 1, Property = concrete });
        Assert.AreSame(steel, assignments.PropertyAt(.5, SectionSide.Left));
        Assert.AreSame(concrete, assignments.PropertyAt(.5, SectionSide.Right));
        Assert.AreSame(concrete, assignments.SectionAt(.5, SectionSide.Right));
        Assert.ThrowsException<NotSupportedException>(() => assignments.SectionAt(.5, SectionSide.Left));
        Assert.ThrowsException<InvalidOperationException>(() => assignments.PropertyAt(.5, SectionSide.Unspecified));
    }

    [TestMethod]
    public void TabulatedPropertiesRetainExactStationsAndDoNotInventInterpolation()
    {
        var first = Steel();
        var last = Steel(500);
        var assignment = new BeamSectionAssignment { Start = 0, End = 1, Law = "Tabulated" };
        assignment.Stations.Add(new BeamSectionStation { Station = .5, Side = SectionSide.Left, Property = first });
        assignment.Stations.Add(new BeamSectionStation { Station = .5, Side = SectionSide.Right, Property = last });
        Assert.AreSame(first, SectionLaws.EvaluateProperty(assignment, .5, SectionSide.Left));
        Assert.AreSame(last, SectionLaws.EvaluateProperty(assignment, .5, SectionSide.Right));
        Assert.ThrowsException<InvalidOperationException>(() => SectionLaws.EvaluateProperty(assignment, .6, SectionSide.Right));
        assignment.Law = "LinearRectangular";
        assignment.Property = first; assignment.EndProperty = last;
        Assert.ThrowsException<NotSupportedException>(() => SectionLaws.EvaluateProperty(assignment, .5, SectionSide.Left));
    }

    [TestMethod]
    public void NewConcretePropertyApiKeepsTheLegacyRepresentationAndFingerprint()
    {
        var section = (ReinforcedConcreteSection)MixedModelFactory.Create().BeamElements[250].BeamProperty;
        var legacy = new BeamSectionAssignment { Start = 0, End = 1, Section = section, EndSection = section };
        var general = new BeamSectionAssignment { Start = 0, End = 1, Property = section, EndProperty = section };
        Assert.AreSame(section, general.Section);
        Assert.AreSame(section, legacy.Property);
        Assert.AreEqual(ModelArchive.Fingerprint(new object[] { legacy }), ModelArchive.Fingerprint(new object[] { general }));
    }

    [TestMethod]
    public void GeneralPropertiesSurviveArchiveAndEnterAnalysisRevision()
    {
        var model = MixedModelFactory.Create(m =>
        {
            m.BeamElements[250].Assignments.Sections.Clear();
            m.BeamElements[250].Assignments.Sections.Add(new BeamSectionAssignment { Start = 0, End = 1, Property = Steel() });
        });
        string before = model.AnalysisFingerprint();
        Assert.AreEqual(0, model.ValidateAssignments().Count);
        using var stream = new MemoryStream();
        ModelArchive.Save(model, stream); stream.Position = 0;
        var copy = ModelArchive.Load(stream);
        Assert.AreEqual(before, copy.AnalysisFingerprint());
        Assert.IsInstanceOfType(copy.BeamElements[250].Assignments.PropertyAt(.25, SectionSide.Unspecified), typeof(SteelSection));
        copy.BeamElements[250].Assignments.Sections[0].Property = Steel(500);
        Assert.AreNotEqual(before, copy.AnalysisFingerprint());
    }

    [TestMethod]
    public void MaterialNeutralPreparationBindsThePropertyAtTheSampleAndDetectsChanges()
    {
        var steel = Steel();
        var model = MixedModelFactory.Create(m =>
        {
            m.BeamElements[250].Assignments.Sections.Clear();
            m.BeamElements[250].Assignments.Sections.Add(new BeamSectionAssignment { Start = 0, End = 1, Property = steel });
        });
        var beam = model.BeamElements[250];
        var sample = beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().First();
        var prepared = BeamActionPreparation.Prepare(model, 250, sample, "settings");
        Assert.AreEqual(DataStatus.Ready, prepared.Status, string.Join(",", prepared.Diagnostics.Select(d => d.Code)));
        Assert.AreNotSame(steel, prepared.Input.Property);
        Assert.AreEqual(ModelArchive.Fingerprint(new object[] { steel }), ModelArchive.Fingerprint(new object[] { prepared.Input.Property }));
        Assert.IsTrue(prepared.Input.IsCurrent);
        double modulus = steel.Wpl1;
        Assert.IsTrue(prepared.Input.IsCurrent);
        prepared.Input.Property.Name = "Changed prepared property";
        Assert.AreEqual("H 300", steel.Name);
        Assert.IsFalse(prepared.Input.IsCurrent);
        prepared.Input.Property.Name = steel.Name;
        Assert.IsTrue(prepared.Input.IsCurrent);
        beam.Assignments.Sections[0].Property = Steel(500);
        Assert.IsFalse(prepared.Input.IsCurrent);
    }

    [TestMethod]
    public void ReversingBeamPreservesGeneralAssignmentsAndFlipsTabulatedSides()
    {
        var model = MixedModelFactory.Create(m =>
        {
            var assignment = new BeamSectionAssignment { Start = 0, End = 1, Law = "Tabulated" };
            assignment.Stations.Add(new BeamSectionStation { Station = .25, Side = SectionSide.Left, Property = Steel() });
            m.BeamElements[250].Assignments.Sections.Clear();
            m.BeamElements[250].Assignments.Sections.Add(assignment);
        });
        var copy = ModelOrientation.ReverseBeam(model, 250);
        var property = copy.BeamElements[250].Assignments.PropertyAt(.75, SectionSide.Right);
        Assert.IsInstanceOfType(property, typeof(SteelSection));
        Assert.AreEqual("H 300", property.Name);
        Assert.AreEqual(.25, model.BeamElements[250].Assignments.Sections[0].Stations[0].Station);
    }

    [TestMethod]
    public void ConflictingLegacyAndGeneralPropertiesAreDiagnosedBeforePreparation()
    {
        var model = MixedModelFactory.Create();
        var beam = model.BeamElements[250];
        var assignment = beam.Assignments.Sections[0];
        assignment.Property = Steel();
        assignment.Section = (ReinforcedConcreteSection)beam.BeamProperty;
        Assert.IsTrue(model.ValidateAssignments().Any(d => d.Code == "InvalidSectionLaw"));
        var sample = beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().First();
        var prepared = BeamActionPreparation.Prepare(model, 250, sample, "settings");
        Assert.AreEqual(DataStatus.Insufficient, prepared.Status);
        Assert.IsNull(prepared.Input);
    }
}
