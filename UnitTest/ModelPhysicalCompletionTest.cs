using GPC.Model.Models;
using GPC.Examples;
using GPC.Geometry;
using GPC.Model.Persistence;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Analysis;
using GPC.Model.Checking.Preparation;
using GPC.Model.Compatibility;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Loads.Assignments;
using GPC.Model.Results.Processing;
using GPC.Model.Results.State;
using GPC.Model.Structure.Assignments;
using GPC.Model.Structure.Topology;

namespace UnitTest;

[TestClass]
public class ModelPhysicalCompletionTest
{
    internal static ResultState State(int count) => new() { Components = Enumerable.Repeat(ComponentAvailability.Available, count).ToArray(),
        Semantics = AnalysisSemantics.LinearStatic, ConcomitantStateId = "G", IsCumulative = true, IsCombined = false };

    [TestMethod]
    public void OffsetRigidDomainsAndCentroidTransport_HaveAnalyticalDistancesAndMoments()
    {
        var model = MixedModelFactory.Create(); var beam = model.BeamElements[250]; var a = beam.Assignments;
        a.OffsetI = new Vector3d(100, 0, 0); a.OffsetJ = new Vector3d(100, 0, 0); a.OffsetAxes = CoordinateSystem.Global;
        a.RigidLengthI = 200; a.RigidLengthJ = 300; a.SectionCentroidOffset = new Vector2d(20, 0);
        var geometry = new BeamReferenceGeometry(beam);
        Assert.AreEqual(1500, geometry.DomainLength("Deformable")); Assert.AreEqual(.475, geometry.ReferenceStation(.5, "Deformable"), 1e-12);
        var point = geometry.PointAt(.5, "Deformable"); Assert.AreEqual(100, point.X); Assert.AreEqual(950, point.Z);
        Assert.AreEqual(.5, geometry.ConvertStation(.475, "OffsetToOffset", "Deformable"), 1e-12);
        var sample = new StationResultBeamForces(model.LoadCases["P+"], new ResultBeamForces(0, 0, 10, 0, 0, 0, ResultTransformations.AtPoint(CoordinateSystem.Global, point)), .5)
        { StationDomain = "Deformable", PhysicalDistance = 750, Body = ActionBody.PositiveSectionFace, State = State(6) };
        var transported = geometry.AtCentroid(sample);
        Assert.AreEqual(120, transported.ResultBeamForces.CoordinateSystem.Origin.X); Assert.AreEqual(-200, transported.ResultBeamForces.T, 1e-10);
        Assert.AreEqual(0, sample.ResultBeamForces.T); Assert.AreEqual(100, sample.ResultBeamForces.CoordinateSystem.Origin.X);
        sample.PhysicalDistance = 1000; Assert.ThrowsException<ArgumentException>(() => geometry.ValidateSample(sample));
        a.RigidLengthJ = 1800; Assert.ThrowsException<ArgumentException>(() => new BeamReferenceGeometry(beam));
    }

    [TestMethod]
    public void OppositeBeamFace_ReversesStationsAndSidesWithoutChangingPhysicalAxialTension()
    {
        var a = new StationResultBeamForces(null, new ResultBeamForces(10, 20, 30, 40, 50, 60, CoordinateSystem.Global), .2)
        { State = State(6), PhysicalDistance = 200, Side = SectionSide.Left, Body = ActionBody.PositiveSectionFace, StationDomain = "NodeToNode" };
        var axes = new CoordinateSystem(Point3d.Origin, new Vector3d(1, 0, 0), new Vector3d(0, -1, 0));
        var r = ResultOrientation.Beam(a, axes, true, 1000);
        Assert.AreEqual(.8, r.ParametricDistance, 1e-12); Assert.AreEqual(800, r.PhysicalDistance); Assert.AreEqual(SectionSide.Right, r.Side);
        Assert.AreEqual(10, r.ResultBeamForces.N); Assert.AreEqual(-20, r.ResultBeamForces.V1); Assert.AreEqual(30, r.ResultBeamForces.V2);
        Assert.AreEqual(40, r.ResultBeamForces.T); Assert.AreEqual(-50, r.ResultBeamForces.M1); Assert.AreEqual(60, r.ResultBeamForces.M2);
        var back = ResultOrientation.Beam(r, CoordinateSystem.Global, true, 1000);
        Assert.AreEqual(a.ResultBeamForces.N, back.ResultBeamForces.N); Assert.AreEqual(a.ResultBeamForces.M1, back.ResultBeamForces.M1);
        Assert.AreEqual(.2, back.ParametricDistance, 1e-12); Assert.AreEqual(SectionSide.Left, back.Side);
    }

    [TestMethod]
    public void LongitudinalOffsets_ConvertThePhysicalCutPositionWithoutRescalingItsLocation()
    {
        var m = MixedModelFactory.Create(); var b = m.BeamElements[250];
        b.Assignments.OffsetAxes = CoordinateSystem.Global; b.Assignments.OffsetI = new Vector3d(100, 0, 100); b.Assignments.OffsetJ = new Vector3d(100, 0, -100);
        b.Assignments.SectionCentroidOffset = new Vector2d(0, 0); var g = new BeamReferenceGeometry(b);
        Assert.AreEqual(1800, g.Length); Assert.AreEqual(1.0 / 6, g.ConvertStation(.2, "NodeToNode", "OffsetToOffset"), 1e-12);
        Assert.AreEqual(.2, g.ConvertStation(1.0 / 6, "OffsetToOffset", "NodeToNode"), 1e-12);
        Assert.AreEqual(400, g.CentroidAt(.2, "NodeToNode").Z, 1e-8);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => g.CentroidAt(0, "NodeToNode"));
    }

    [TestMethod]
    public void OppositeShellNormal_TransformsMomentsShearPointsAndPhysicalRebarFaces()
    {
        var axes = new CoordinateSystem(Point3d.Origin, new Vector3d(1, 0, 0), new Vector3d(0, -1, 0));
        var source = new PointResultPlateForces(null, new ResultPlateForces(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6, 7, 8), new Point2d(10, 20), "integration")
        { State = State(8), CoordinateKind = ResultCoordinateKind.LocalPhysical, PointKind = ShellResultPointKind.IntegrationPoint };
        var r = ResultOrientation.Shell(source, axes);
        Assert.AreEqual(1, r.Forces.Fxx); Assert.AreEqual(2, r.Forces.Fyy); Assert.AreEqual(-3, r.Forces.Fxy);
        Assert.AreEqual(-4, r.Forces.Fxz); Assert.AreEqual(5, r.Forces.Fyz);
        Assert.AreEqual(-6, r.Forces.Mxx); Assert.AreEqual(-7, r.Forces.Myy); Assert.AreEqual(8, r.Forces.Mxy); Assert.AreEqual(-20, r.Location.Y);
        var back = ResultOrientation.Shell(r, CoordinateSystem.Global); Assert.AreEqual(20, back.Location.Y); Assert.AreEqual(6, back.Forces.Mxx);
        source.CoordinateKind = ResultCoordinateKind.Natural;
        Assert.AreEqual(20, ResultOrientation.Shell(source, axes, true, 4).Location.X);
        var layers = new ShellAssignments { LayerAxes = CoordinateSystem.Global, PhysicalThickness = 200, Offset = 10 };
        layers.Layers.Add(new ShellRebarLayer { PhysicalFace = "outside", AxisPositionThroughThickness = 80, DirectionRadians = .4, Diameter = 12, Pitch = 150, Order = 1 });
        var reversed = ResultOrientation.ShellLayers(layers, axes);
        Assert.AreEqual("outside", reversed.Layers[0].PhysicalFace); Assert.AreEqual(-80, reversed.Layers[0].AxisPositionThroughThickness);
        Assert.AreEqual(-.4, reversed.Layers[0].DirectionRadians, 1e-12); Assert.AreEqual(-10, reversed.Offset); Assert.AreEqual(80, layers.Layers[0].AxisPositionThroughThickness);
    }

    internal static BeamAnalysisProfile Profile() => new() { Interpolation = ProfileInterpolation.Linear,
        Stations = { new BeamAnalysisStation { Station = 0, EA = 100, EI1 = 1000, EI2 = 2000, GA1 = 10, GA2 = 20, GJ = 100, MassPerLength = 1 },
            new BeamAnalysisStation { Station = 1, EA = 300, EI1 = 5000, EI2 = 6000, GA1 = 30, GA2 = 40, GJ = 200, MassPerLength = 2 } },
        Modifiers = new BeamStiffnessModifiers { Axial = .5, Bending1 = .25 } };
    [TestMethod]
    public void AnalysisProfile_InterpolatesRigiditiesWithoutReducingResistance_AndSurvivesArchive()
    {
        var model = MixedModelFactory.Create(); var beam = model.BeamElements[250]; var old = model.AnalysisFingerprint();
        var section = beam.Assignments.Sections[0].Section; double area = section.Area;
        beam.Assignments.AnalysisProfile = Profile(); beam.Assignments.RigidLengthI = 40; beam.Assignments.SectionCentroidOffset = new Vector2d(2, 3);
        Assert.AreEqual(100, beam.Assignments.AnalysisProfile.At(.5).EA); Assert.AreEqual(750, beam.Assignments.AnalysisProfile.At(.5).EI1);
        Assert.AreEqual(area, section.Area); Assert.AreNotEqual(old, model.AnalysisFingerprint());
        using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0; var copy = ModelArchive.Load(stream);
        Assert.AreEqual(model.AnalysisFingerprint(), copy.AnalysisFingerprint());
        Assert.AreEqual(100, copy.BeamElements[250].Assignments.AnalysisProfile.At(.5).EA);
        var fp = copy.AnalysisFingerprint(); copy.BeamElements[250].Assignments.AnalysisProfile.Modifiers.Axial = .9; Assert.AreNotEqual(fp, copy.AnalysisFingerprint());
    }

    [TestMethod]
    public void ConstantAnalysisProfile_RequiresSideAtInternalJump()
    {
        var p = Profile(); p.Interpolation = ProfileInterpolation.Constant;
        p.Stations.Insert(1, new BeamAnalysisStation { Station = .5, EA = 200, EI1 = 1000, EI2 = 2000, GA1 = 10, GA2 = 20, GJ = 100, MassPerLength = 1 });
        Assert.ThrowsException<InvalidOperationException>(() => p.At(.5));
        Assert.AreEqual(50, p.At(.5, SectionSide.Left).EA); Assert.AreEqual(100, p.At(.5, SectionSide.Right).EA);
        p.Modifiers.Bending1 = -1; Assert.ThrowsException<ArgumentException>(() => p.At(.25));
    }

    [TestMethod]
    public void LinearRectangularSection_InterpolatesGeometryAndExplicitBarPaths()
    {
        var model = MixedModelFactory.Create(); var a = model.BeamElements[250].Assignments.Sections[0]; var first = a.Section;
        var last = new ReinforcedConcreteSection(new SectionRectangular(700, 500), first.ConcreteMaterial);
        foreach (var bar in first.Rebars) last.AddRebar(new ReinforcedConcreteRebar(bar.RebarSection, new Point2d(bar.Position.X + 100, bar.Position.Y + 200), id: bar.Id));
        a.Law = "LinearRectangular"; a.EndSection = last;
        var middle = model.BeamElements[250].Assignments.SectionAt(.5, SectionSide.Unspecified);
        Assert.AreEqual(240000, middle.Area, 1e-8); Assert.AreEqual(4, middle.Rebars.Count());
        Assert.AreEqual(100, middle.Rebars.First().Position.X); Assert.AreEqual(150, middle.Rebars.First().Position.Y);
        Assert.AreEqual(150000, first.Area); Assert.AreEqual(50, first.Rebars.First().Position.X);
        a.EndSection = new ReinforcedConcreteSection(new SectionRectangular(700, 500), first.ConcreteMaterial);
        Assert.ThrowsException<NotSupportedException>(() => model.BeamElements[250].Assignments.SectionAt(.5, SectionSide.Unspecified));
    }

    [TestMethod]
    public void TabulatedSection_RequiresExactStationAndDiscontinuitySide()
    {
        var model = MixedModelFactory.Create(); var a = model.BeamElements[250].Assignments.Sections[0]; a.Law = "Tabulated";
        a.Stations.Add(new BeamSectionStation { Station = .5, Side = SectionSide.Left, Section = a.Section });
        Assert.AreSame(a.Section, model.BeamElements[250].Assignments.SectionAt(.5, SectionSide.Left));
        Assert.ThrowsException<InvalidOperationException>(() => model.BeamElements[250].Assignments.SectionAt(.51, SectionSide.Left));
        Assert.ThrowsException<InvalidOperationException>(() => model.BeamElements[250].Assignments.SectionAt(.5, SectionSide.Right));
        using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0; var copy = ModelArchive.Load(stream);
        Assert.AreEqual(a.Section.Area, copy.BeamElements[250].Assignments.SectionAt(.5, SectionSide.Left).Area);
    }

    [TestMethod]
    public void ReverseBeam_PreservesPhysicalSectionsLoadsReleasesAndProducesASeparateStaleRevision()
    {
        var model = MixedModelFactory.Create(m => {
            var b = m.BeamElements[250]; b.Assignments.RigidLengthI = 100; b.Assignments.RigidLengthJ = 200;
            b.Assignments.SectionCentroidOffset = new Vector2d(20, 30); b.Assignments.AnalysisProfile = Profile();
            var lc = m.LoadCases["P+"]; var line = b.Line;
            b.Assignments.Loads.Add(new BeamLoadAssignment { Start = .2, End = .7, LengthConvention = BeamLoadLengthConvention.ActualLength, OriginalAssignmentId = "q",
                StartIntensity = new GPC.Model.Loads.LineLoad(0, 1, 0, 0, 0, 0, line, lc), EndIntensity = new GPC.Model.Loads.LineLoad(0, 3, 0, 0, 0, 0, line, lc) });
        });
        var old = model.BeamElements[250]; var fp = model.AnalysisFingerprint();
        var reversed = ModelOrientation.ReverseBeam(model, 250); var beam = reversed.BeamElements[250];
        Assert.AreEqual(10, old.NodeI.Id); Assert.AreEqual(40, beam.NodeI.Id); Assert.AreSame(reversed.NodesElements[10], beam.NodeJ);
        Assert.AreEqual(fp, model.AnalysisFingerprint()); Assert.AreNotEqual(fp, reversed.AnalysisFingerprint());
        Assert.AreEqual(200, beam.Assignments.RigidLengthI); Assert.AreEqual(-30, beam.Assignments.SectionCentroidOffset.Y);
        Assert.AreEqual(1, beam.Assignments.SectionGeometryAxes.V2.Y); Assert.AreEqual(-1, beam.Assignments.SectionAxes.V2.Y);
        Assert.AreEqual(old.Assignments.Sections[0].Section.Rebars.First().Position.Y, beam.Assignments.Sections[0].Section.Rebars.First().Position.Y);
        var load = beam.Assignments.Loads.Single(); Assert.AreEqual(.3, load.Start, 1e-12); Assert.AreEqual(.8, load.End, 1e-12); Assert.AreEqual(3, load.StartIntensity.F2);
        var beforeLoad = Equilibrium.BeamLoad(old, old.Assignments.Loads[0]); var afterLoad = Equilibrium.BeamLoad(beam, load);
        Vector3d beforeArm = beforeLoad.Point - Point3d.Origin, afterArm = afterLoad.Point - Point3d.Origin;
        Assert.AreEqual(beforeLoad.Force.Y, afterLoad.Force.Y, 1e-9);
        Assert.AreEqual((beforeLoad.Moment + beforeArm.CrossProduct(beforeLoad.Force)).X, (afterLoad.Moment + afterArm.CrossProduct(afterLoad.Force)).X, 1e-7);
        var release = beam.Attributes.Values.OfType<GPC.Model.Attributes.BeamReleasesAttribute>().Single();
        Assert.AreEqual(GPC.Model.Attributes.BeamConnectionKind.Released, release.I[5].Kind);
        Assert.AreEqual(150, beam.Assignments.AnalysisProfile.At(0).EA); Assert.AreEqual(50, beam.Assignments.AnalysisProfile.At(1).EA);
        var sample = beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().First();
        Assert.AreEqual(1, sample.ParametricDistance); Assert.AreEqual(2e6, sample.ResultBeamForces.M1);
        Assert.AreEqual(DataStatus.Stale, ResultPreparation.Prepare(reversed, new ElementKey { Family = EntityFamily.Beam, Id = 250 }, sample).Status);
        var back = ModelOrientation.ReverseBeam(reversed, 250).BeamElements[250];
        Assert.AreEqual(10, back.NodeI.Id); Assert.IsNull(back.Assignments.SectionGeometryAxes); Assert.AreEqual(30, back.Assignments.SectionCentroidOffset.Y);
        Assert.AreEqual(-2e6, ((StationResultBeamForces)back.Results[0].Results[0]).ResultBeamForces.M1);
    }

    [TestMethod]
    public void ReverseShell_PreservesPhysicalFacesAndGlobalLoadDirection()
    {
        var model = MixedModelFactory.Create(); var original = model.AreaElements[1090];
        var pressure = new GPC.Model.Loads.NormalAreaLoad(2, original.Shape, model.LoadCases["P+"], original.CoordinateSystem); original.Loads.Add(pressure);
        var fp = model.AnalysisFingerprint();
        var reversed = ModelOrientation.ReverseShell(model, 1090); var shell = reversed.AreaElements[1090];
        CollectionAssert.AreEqual(new[] { 10, 90, 40 }, shell.Nodes.Select(n => n.Id).ToArray());
        Assert.AreEqual(fp, model.AnalysisFingerprint()); Assert.AreEqual(-original.CoordinateSystem.V3.Y, shell.CoordinateSystem.V3.Y);
        Assert.AreEqual("ground", shell.Assignments.Layers[0].PhysicalFace); Assert.AreEqual(-110, shell.Assignments.Layers[0].AxisPositionThroughThickness);
        Assert.AreEqual(pressure.GetGlobalLoadVector().Y, shell.Loads.Values.OfType<GPC.Model.Loads.NormalAreaLoad>().Single().GetGlobalLoadVector().Y, 1e-8);
        var r = (PointResultPlateForces)shell.Results[0].Results[0]; Assert.AreEqual(-80000, r.Forces.Mxx); Assert.AreEqual(-10, r.Forces.Fxy);
        Assert.AreEqual(DataStatus.Stale, ResultPreparation.Prepare(reversed, new ElementKey { Family = EntityFamily.Shell, Id = 1090 }, r).Status);
        var back = ModelOrientation.ReverseShell(reversed, 1090).AreaElements[1090];
        CollectionAssert.AreEqual(original.Nodes.Select(n => n.Id).ToArray(), back.Nodes.Select(n => n.Id).ToArray());
        Assert.AreEqual(80000, ((PointResultPlateForces)back.Results[0].Results[0]).Forces.Mxx);
    }

    [TestMethod]
    public void Reversal_RejectsOpaqueAssignmentsWithoutTouchingOriginal()
    {
        var model = MixedModelFactory.Create(); model.BeamElements[250].Assignments.OtherAssignments.Add(new PreservedAssignment { Kind = "unknown" });
        var fp = model.AnalysisFingerprint(); Assert.ThrowsException<NotSupportedException>(() => ModelOrientation.ReverseBeam(model, 250));
        Assert.AreEqual(fp, model.AnalysisFingerprint()); Assert.AreEqual(10, model.BeamElements[250].NodeI.Id);
    }

    [TestMethod]
    public void Archive_WithAbsentNewOptionalFields_StillLoadsLegacyAssignments()
    {
        var model = MixedModelFactory.Create(); model.BeamElements[250].Assignments.RigidLengthI = 25;
        using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0;
        var xml = System.Xml.Linq.XDocument.Load(stream);
        var optional = new[] { "RigidLengthI", "RigidLengthJ", "SectionCentroidOffset", "SectionGeometryAxes", "AnalysisProfile", "EndSection", "Stations", "DerivedFrom", "DerivedSourceFingerprint", "HistoryId", "IncrementIndex", "IncrementsStartAtZero" };
        var fields = xml.Descendants().Where(e => optional.Any(n => e.Name.LocalName == "_x003C_" + n + "_x003E_k__BackingField")).ToArray();
        Assert.IsTrue(fields.Length > 0); foreach (var e in fields) e.Remove();
        using var older = new MemoryStream(); xml.Save(older); older.Position = 0; var copy = ModelArchive.Load(older);
        Assert.IsNotNull(copy.BeamElements[250].Assignments.Sections[0].Stations); Assert.AreEqual(0, copy.BeamElements[250].Assignments.RigidLengthI);
        Assert.IsNotNull(copy.AnalysisFingerprint());
    }
}
