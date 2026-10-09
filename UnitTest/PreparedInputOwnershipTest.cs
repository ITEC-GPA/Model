using GPC.Examples;
using GPC.Geometry;
using GPC.Model.Collections;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using GPC.Model.Sections.Concrete;
using GPC.Model.Checking.Preparation;
using GPC.Model.Compatibility;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Structure.Members;

namespace UnitTest;

[TestClass]
public class PreparedInputOwnershipTest
{
    [TestMethod]
    public void ConcretePreparationOwnsSectionMaterialAndRebarGeometry()
    {
        var model = MixedModelFactory.Create(); var beam = model.BeamElements[250];
        var sample = Verification.BeamSamples(beam, "synthetic-static", "P+")[0];
        var prepared = Verification.PrepareBeam(model, 250, sample, "ownership");
        Assert.AreEqual(DataStatus.Ready, prepared.Status);
        var input = prepared.Input; var source = (ReinforcedConcreteSection)beam.BeamProperty;
        Assert.AreNotSame(source, input.Section);
        Assert.AreNotSame(source.ConcreteMaterial, input.Section.ConcreteMaterial);
        Assert.AreNotSame(source.Rebars.First().Position, input.Section.Rebars.First().Position);
        Assert.AreEqual(ModelArchive.Fingerprint(new object[] { source }), ModelArchive.Fingerprint(new object[] { input.Section }));
        Assert.IsTrue(input.IsCurrent);
        source.Rebars.First().Position.Y += 10;
        Assert.AreEqual(50, input.Section.Rebars.First().Position.Y); Assert.IsFalse(input.IsCurrent);
        source.Rebars.First().Position.Y -= 10;
        Assert.IsTrue(input.IsCurrent);
        input.Section.Rebars.First().Position.Y += 20;
        Assert.AreEqual(50, source.Rebars.First().Position.Y); Assert.IsFalse(input.IsCurrent);
    }

    [DataTestMethod]
    [DataRow("dataset metadata")]
    [DataRow("dataset removed")]
    [DataRow("sample removed")]
    [DataRow("beam removed")]
    public void DetachedConcreteInputStillTracksItsSourceLifetime(string change)
    {
        var model = MixedModelFactory.Create(); var beam = model.BeamElements[250];
        var input = Verification.PrepareBeam(model, 250, Verification.BeamSamples(beam, "synthetic-static", "P+")[0], "ownership").Input;
        Assert.IsTrue(input.IsCurrent);
        switch (change)
        {
            case "dataset metadata": model.Datasets["synthetic-static"].Program = "Other importer"; break;
            case "dataset removed": model.Datasets.Clear(); break;
            case "sample removed": beam.Results.Clear(); break;
            case "beam removed": model.BeamElements.Remove(250); break;
        }
        Assert.IsFalse(input.IsCurrent);
    }

    [TestMethod]
    public void ShellPreparationOwnsLayersPropertyAndAxesWhileTrackingSourceChanges()
    {
        var model = MixedModelFactory.Create(); var shell = model.AreaElements[1090];
        var sample = (PointResultPlateForces)shell.Results[0].Results[0];
        var prepared = ShellInputPreparation.Prepare(model, shell.Id, sample, "ownership");
        Assert.AreEqual(DataStatus.Ready, prepared.Status);
        var input = prepared.Input;
        Assert.AreNotSame(shell.Assignments, input.Assignments);
        Assert.AreNotSame(shell.Assignments.LayerAxes, input.Assignments.LayerAxes);
        Assert.AreNotSame(shell.Assignments.Layers[0].Steel, input.Assignments.Layers[0].Steel);
        Assert.AreNotSame(shell.PlateProperty, input.Property);
        Assert.IsTrue(input.IsCurrent);
        shell.Assignments.Layers[0].Pitch = 200;
        Assert.AreEqual(150, input.Assignments.Layers[0].Pitch); Assert.IsFalse(input.IsCurrent);
        shell.Assignments.Layers[0].Pitch = 150;
        Assert.IsTrue(input.IsCurrent);
        input.Assignments.Layers[0].Diameter = 30;
        Assert.AreEqual(16, shell.Assignments.Layers[0].Diameter); Assert.IsFalse(input.IsCurrent);
    }

    [TestMethod]
    public void RestraintsOwnTheirFrameAndRequireValidCoordinateSystems()
    {
        var axes = new CoordinateSystem(new Point3d(0, 0, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));
        var flags = new bool?[] { true, false, null, true, false, null };
        var restraint = new MemberRestraint(100, axes, flags, "Designer", "Phase1");
        axes.Origin.X = 50; flags[0] = false;
        Assert.AreEqual(0, restraint.Axes.Origin.X); Assert.AreEqual(true, restraint.Restrained[0]);
        restraint.Axes.Origin.X = 20; Assert.AreEqual(0, restraint.Axes.Origin.X);
        Assert.ThrowsException<ArgumentException>(() => new MemberRestraint(0, null, flags, "Designer"));
    }

    [TestMethod]
    public void ImportedStaleRebarCounterCannotReuseAnExistingIdentity()
    {
        var reference = (ReinforcedConcreteSection)MixedModelFactory.Create().BeamElements[250].BeamProperty;
        var rebars = new UniqueIdCollection<ReinforcedConcreteRebar>();
        var oldBar = new ReinforcedConcreteRebar(reference.Rebars.First().RebarSection, new Point2d(30, 30)); oldBar.Id = 12;
        // Dictionary-based import and historical archives can retain a counter below the actual keys.
        rebars.Add(12, oldBar);
        var section = new ReinforcedConcreteSection(new GPC.Model.Sections.SectionRectangular(500, 300, "RC"), reference.ConcreteMaterial, rebars);
        var next = new ReinforcedConcreteRebar(oldBar.RebarSection, new Point2d(50, 50));
        section.AddRebar(next);
        Assert.AreEqual(13, next.Id); Assert.AreEqual(2, section.RebarsCount);
        Assert.AreSame(oldBar, section.Rebars.Single(r => r.Id == 12));
    }
}
