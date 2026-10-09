using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Loads.Assignments;
using GPC.Model.Results.Processing;

namespace UnitTest;

/// <summary>Straus7 properties and attributes through a transport double of the R3 API, in m, kN, kPa, kg: the conventions are those
/// verified on the native solver by <see cref="Straus7NativeIntegrationTest"/>; values are invented, not solver output.</summary>
[TestClass]
public class Straus7AssignmentImportTest
{
    private string directory = null!, modelPath = null!;
    [TestInitialize] public void Files()
    {
        directory = Path.Combine(Path.GetTempPath(), "gpc-straus7-assignments-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(directory);
        modelPath = Path.Combine(directory, "synthetic.st7"); File.WriteAllText(modelPath, "Synthetic bytes for transport-double tests, NOT a native ST7 file.");
    }
    [TestCleanup] public void Cleanup() { File.Delete(modelPath); Directory.Delete(directory); }
    private ImportReport Import(AssignmentApi api) => new Straus7ApiConverter(() => api).Import(new Straus7ImportRequest { ModelPath = modelPath, ModelRevision = "r1", AnalysisId = "a1" });
    private static void Partial(ImportReport report) => Assert.AreEqual(ImportStatus.Partial, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Code + ": " + d.Message)));
    private static readonly string Loads = Straus7ApiConverter.CaseName(1, "Loads");

    // Nodes 1 (0,0,0), 2 (4,0,0), 3 (4,2,0), 4 (0,2,0) m; beam 1: 1-2 concrete rectangle B 0.4 x D 0.6; beam 2: 4-3 steel I with offset;
    // both along +X with V1 = +Y, V2 = +Z. Plate 1: 1-2-3-4, normal +Z. Cases: 1 loads, 2 gravity, 3 seismic.
    private static AssignmentApi Scenario()
    {
        var api = new AssignmentApi();
        api.Distributed(1, Straus7LoadFrame.Principal, 2, 1, 1, 5, -5, -8, -2, -3, .25, .25);
        api.Distributed(1, Straus7LoadFrame.Global, 1, 1, 2, 1, 1, 2, 0, 0, .1, .2);
        api.Distributed(1, Straus7LoadFrame.Global, 3, 1, 3, 0, -3, 0, 0, 0, 0, 0, project: 1);
        api.Distributed(1, Straus7LoadFrame.PrincipalMoment, 3, 1, 4, 2, 4, 0, 0, 0, .5, 0);
        api.Distributed(1, Straus7LoadFrame.Principal, 5, 1, 5, 0, -1, 0, 0, 0, 0, 0); // Local y = principal 2 for a zero principal angle.
        api.Point(1, 1, 6, false, false, 0, -10, 0, .3);
        api.Point(1, 1, 7, false, true, 0, 0, -5, .7);
        api.Point(1, 1, 8, true, false, 2, 0, 0, .5);
        api.Point(1, 1, 9, true, true, 0, 0, 1, .25);
        api.Point(2, 1, 1, false, false, 0, -20, 0, .5);
        api.Add(Straus7Entity.Beam, 2, Straus7Attributes.BeamOffset); api.BeamOffsets[2] = new[] { .05, .1 };
        api.Add(Straus7Entity.Beam, 2, Straus7Attributes.BeamTaper);
        api.Add(Straus7Entity.Plate, 1, Straus7Attributes.PlateThickness); api.PlateThicknesses[1] = new[] { .25, .25 };
        api.Add(Straus7Entity.Plate, 1, Straus7Attributes.PlateOffset); api.PlateOffsets[1] = new[] { .05 };
        api.Add(Straus7Entity.Plate, 1, Straus7Attributes.PlateFacePressure, lc: 1); api.NormalPressures[(1, 1)] = new[] { 2d, 5 };
        api.Add(Straus7Entity.Plate, 1, Straus7Attributes.PlateGlobalPressure, lc: 1, local: 2); api.GlobalPressures[(1, 2, 1)] = (new[] { 0d, 0, -4 }, 0);
        api.Add(Straus7Entity.Plate, 1, Straus7Attributes.PlateGlobalPressure, lc: 1, local: 1); api.GlobalPressures[(1, 1, 1)] = (new[] { 0d, 0, -1 }, 1);
        api.Add(Straus7Entity.Plate, 1, Straus7Attributes.PlateFaceShear, lc: 1);
        api.Restraint(1, 1, new[] { 1, 1, 1, 1, 1, 1 });
        api.Restraint(2, 5, new[] { 1, 1, 1, 0, 0, 0 }); api.Systems[5] = new() { Type = 0, Data = new[] { 0d, 0, 0, 0, 1, 0, -1, 0, 0 } };
        api.Restraint(3, 1, new[] { 1, 1, 1, 0, 0, 0 }, new[] { 0, 0, .01, 0, 0, 0 });
        api.Restraint(4, 6, new[] { 1, 1, 1, 0, 0, 0 }); api.Systems[6] = new() { Type = 1, Data = new double[9] };
        api.Add(Straus7Entity.Node, 2, Straus7Attributes.Force, lc: 1); api.NodeLoads[(2, 1, false)] = new[] { 1d, 2, -3 };
        api.Add(Straus7Entity.Node, 4, Straus7Attributes.Moment, lc: 1); api.NodeLoads[(4, 1, true)] = new[] { 0, .5, 0 };
        return api;
    }

    [TestMethod]
    public void Assignments_PropertiesOffsetsRestraintsLoadsAndGravity_ConvertFromSourceUnits()
    {
        var report = Import(Scenario()); Partial(report); var m = report.Model;
        var column = m.BeamElements.Values.Single(b => b.Source.OriginalId == "1"); var girder = m.BeamElements.Values.Single(b => b.Source.OriginalId == "2");
        var plate = m.AreaElements.Values.Single();
        var rc = (ReinforcedConcreteSection)column.BeamProperty;
        Assert.IsInstanceOfType(rc.SectionShape, typeof(SectionRectangular)); Assert.AreEqual(240000, rc.SectionShape.Area, 1e-6);
        Assert.AreEqual(2.5e-9, rc.ConcreteMaterial.Density, 1e-21, "2500 kg/m3 as t/mm3.");
        var steel = (SteelSection)girder.BeamProperty; var expected = AssignmentApi.IProperties(.2, .15, .4, .015, .012, .008);
        Assert.IsInstanceOfType(steel.SectionShape, typeof(SectionH)); Assert.AreEqual(355, steel.SteelMaterial.Fyk);
        Assert.AreEqual(expected[0] * 1e6, steel.SectionShape.Area, 1e-6 * expected[0] * 1e6); Assert.AreEqual(7.85e-9, steel.SteelMaterial.Density, 1e-21);
        Assert.AreEqual(50, girder.Assignments.SectionCentroidOffset.X, 1e-9); Assert.AreEqual(100, girder.Assignments.SectionCentroidOffset.Y, 1e-9);
        Assert.AreEqual(0, column.Assignments.SectionCentroidOffset?.X ?? 0); Assert.AreEqual(0, column.Assignments.SectionCentroidOffset?.Y ?? 0);

        Assert.AreEqual(12, column.Assignments.Loads.Count);
        Vector3d force = new(0, 0, 0);
        foreach (var l in column.Assignments.Loads) force += Equilibrium.BeamLoad(column, l).Force;
        // Fx: linear 1 -> 2 kN/m over 0.7 x 4 m; Fz: trapezoid -22, projected -12, local -4, point -10 and -5 kN.
        Assert.AreEqual(4200, force.X, 1e-6); Assert.AreEqual(0, force.Y, 1e-6); Assert.AreEqual(-53000, force.Z, 1e-6);
        // Distributed torsion: triangle 0 -> 4 -> 0 kNm/m over 4 m.
        double torsion = column.Assignments.Loads.Where(l => l.SourceRecord.Contains("/PrincipalMoment/")).Sum(l => Equilibrium.BeamLoad(column, l).Moment.X);
        Assert.AreEqual(8e6, torsion, 1e-3);
        var principalPoint = column.Assignments.Loads.Single(l => l.SourceRecord.Contains("/point/30/")).Concentrated;
        Assert.AreEqual(1200, principalPoint.Point.X, 1e-9); Assert.AreEqual(-10000, principalPoint.F2);
        Assert.AreEqual(2e6, column.Assignments.Loads.Single(l => l.SourceRecord.Contains("/point/32/")).Concentrated.M1);
        Assert.AreEqual(1e6, column.Assignments.Loads.Single(l => l.SourceRecord.Contains("/point/33/")).Concentrated.M3);
        var projected = column.Assignments.Loads.Single(l => l.SourceRecord.Contains("/Global/3/"));
        Assert.AreEqual(BeamLoadLengthConvention.ProjectedLength, projected.LengthConvention); Assert.AreEqual(1, projected.ProjectionPlaneNormal.Z);
        Assert.IsTrue(column.Assignments.Loads.All(l => l.Eccentricity == null));

        // The solver applies the girder load on its centroidal (offset) axis: (0.05, 0.1) m along V1 = Y, V2 = Z.
        var girderLoad = girder.Assignments.Loads.Single(); var g = Equilibrium.BeamLoad(girder, girderLoad);
        Assert.AreEqual(100, girderLoad.Eccentricity.Y); Assert.AreEqual(-20000, g.Force.Z, 1e-9);
        var gm = g.Moment + ((Vector3d)g.Point).CrossProduct(g.Force);
        Assert.AreEqual(2050 * -20000, gm.X, 1e-3); Assert.AreEqual(2000 * 20000, gm.Y, 1e-3);

        var slab = (ConcretePlateProperty)plate.PlateProperty;
        Assert.AreEqual(250, slab.MembraneThickness); Assert.AreEqual(250, plate.Assignments.PhysicalThickness); Assert.AreEqual(50, plate.Assignments.Offset);
        StringAssert.Contains(slab.Name, "t=250");
        Assert.AreEqual(-0.003, plate.Loads.Values.OfType<NormalAreaLoad>().Single().Pressure, 1e-15, "-z face 2 kPa minus +z face 5 kPa.");
        Assert.AreEqual(-0.004, plate.Loads.Values.OfType<AreaLoad>().Single().P3, 1e-15); Assert.AreEqual(2, plate.Loads.Count);

        var fixedNode = m.NodesElements.Values.Single(n => n.Source.OriginalId == "1").Assignments.Restrains.Single().Restrain;
        Assert.AreEqual(6, fixedNode.Restrains.Count); Assert.AreEqual(1, fixedNode.CoordinateSystem.V3.Z);
        var skew = m.NodesElements.Values.Single(n => n.Source.OriginalId == "2").Assignments.Restrains.Single().Restrain;
        Assert.AreEqual(3, skew.Restrains.Count); Assert.AreEqual(1, skew.CoordinateSystem.V1.Y, 1e-12); Assert.AreEqual(-1, skew.CoordinateSystem.V2.X, 1e-12);
        Assert.AreEqual(4000, skew.CoordinateSystem.Origin.X);
        foreach (var free in new[] { "3", "4" }) Assert.AreEqual(0, m.NodesElements.Values.Single(n => n.Source.OriginalId == free).Assignments.Restrains.Count, free);
        var nodeForce = m.NodesElements.Values.Single(n => n.Source.OriginalId == "2").Loads.Values.OfType<PointLoad>().Single();
        Assert.AreEqual(1000, nodeForce.F1); Assert.AreEqual(-3000, nodeForce.F3); Assert.AreEqual(Loads, nodeForce.LoadCase.Name);
        Assert.AreEqual(5e5, m.NodesElements.Values.Single(n => n.Source.OriginalId == "4").Loads.Values.OfType<PointLoad>().Single().M2);

        var gravity = m.ModelLoads.Values.OfType<ModelGravityLoad>().Single();
        Assert.AreEqual(Straus7ApiConverter.CaseName(2, "Gravity"), gravity.LoadCase.Name);
        Assert.AreEqual(-9806.65, gravity.GravityVector.Z, 1e-9, "-9.80665 m/s2 along +Z."); Assert.AreEqual(0, gravity.GravityVector.X, 1e-12);

        var warnings = report.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).Select(d => d.Code).ToHashSet();
        foreach (var code in new[] { "Straus7BeamAttributePreserved", "Straus7ProjectedPressurePreserved", "Straus7PlateAttributePreserved", "Straus7EnforcedDisplacementPreserved",
            "Straus7RestraintUcsPreserved", "Straus7InertiaCasePreserved", "Straus7AssignmentsPartlyMapped" })
            Assert.IsTrue(warnings.Contains(code), code);
        Assert.IsFalse(warnings.Contains("Straus7SectionByValues")); Assert.IsFalse(warnings.Contains("Straus7AssignmentsNotMapped"));
    }

    [DataTestMethod]
    [DataRow(0, "0.25:0.75:-5:-5")]
    [DataRow(1, "0.25:0.75:-5:-8")]
    [DataRow(2, "0:0.25:-2:-5|0.25:1:-5:-3")]
    [DataRow(3, "0:0.25:-2:-5|0.25:0.75:-5:-8")]
    [DataRow(4, "0.25:0.75:-5:-8|0.75:1:-8:-3")]
    [DataRow(5, "0:0.25:-2:-5|0.25:0.75:-5:-8|0.75:1:-8:-3")]
    public void DistributedLoadTypes_ArePiecewiseLinearSegments(int type, string segments)
    {
        var api = new AssignmentApi(); api.Distributed(1, Straus7LoadFrame.Principal, 2, 1, 1, type, -5, -8, -2, -3, .25, .25);
        var report = Import(api); Partial(report);
        var loads = report.Model.BeamElements.Values.Single(b => b.Source.OriginalId == "1").Assignments.Loads;
        Assert.AreEqual(segments, string.Join("|", loads.Select(l => FormattableString.Invariant($"{l.Start}:{l.End}:{l.StartIntensity.F2}:{l.EndIntensity.F2}"))));
    }

    [TestMethod]
    public void RotatedPrincipalAxesAndSeveralFreedomCases_ArePreservedWithWarnings()
    {
        var api = new AssignmentApi { FreedomCases = 2 }; api.BeamProperties[2].SectionProperties[10] = .1;
        api.Distributed(2, Straus7LoadFrame.Principal, 5, 1, 1, 0, -1, 0, 0, 0, 0, 0); api.Distributed(2, Straus7LoadFrame.Principal, 2, 1, 2, 0, -1, 0, 0, 0, 0, 0);
        api.Restraint(1, 1, new[] { 1, 1, 1, 1, 1, 1 });
        var report = Import(api); Partial(report); var m = report.Model;
        var girder = m.BeamElements.Values.Single(b => b.Source.OriginalId == "2");
        Assert.IsNull(girder.BeamProperty, "Steel and concrete sections need an outline; the Straus7 values alone give no property.");
        Assert.IsInstanceOfType(m.BeamElements.Values.Single(b => b.Source.OriginalId == "1").BeamProperty, typeof(ReinforcedConcreteSection));
        Assert.AreEqual(1, girder.Assignments.Loads.Count, "Only the principal-axis load is mapped.");
        Assert.AreEqual(0, m.NodesElements.Values.Sum(n => n.Assignments.Restrains.Count));
        var codes = report.Diagnostics.Select(d => d.Code).ToHashSet();
        foreach (var code in new[] { "Straus7SectionByValues", "Straus7LocalBeamLoadPreserved", "Straus7FreedomCasesPreserved", "SectionByValuesOnly" }) Assert.IsTrue(codes.Contains(code), code);
    }

    [DataTestMethod]
    [DataRow("positions-crossed")][DataRow("unknown-type")][DataRow("unknown-case")][DataRow("point-outside")][DataRow("bad-direction")]
    [DataRow("bad-projection")][DataRow("gravity-direction")][DataRow("nan-offset")][DataRow("bad-restraint")]
    public void InvalidAttributes_RejectTheWholeCandidate(string fault)
    {
        var api = new AssignmentApi();
        switch (fault)
        {
            case "positions-crossed": api.Distributed(1, Straus7LoadFrame.Principal, 2, 1, 1, 1, -5, -8, 0, 0, .6, .6); break;
            case "unknown-type": api.Distributed(1, Straus7LoadFrame.Principal, 2, 1, 1, 9, -5, -8, 0, 0, 0, 0); break;
            case "unknown-case": api.Distributed(1, Straus7LoadFrame.Principal, 2, 9, 1, 0, -5, 0, 0, 0, 0, 0); break;
            case "point-outside": api.Point(1, 1, 1, false, false, 0, -10, 0, 1.5); break;
            case "bad-direction": api.Distributed(1, Straus7LoadFrame.Global, 4, 1, 1, 0, -5, 0, 0, 0, 0, 0); break;
            case "bad-projection": api.Distributed(1, Straus7LoadFrame.Global, 3, 1, 1, 0, -5, 0, 0, 0, 0, 0, project: 2); break;
            case "gravity-direction": api.Cases[2].GravityDirection = 4; break;
            case "nan-offset": api.Add(Straus7Entity.Beam, 2, Straus7Attributes.BeamOffset); api.BeamOffsets[2] = new[] { double.NaN, 0 }; break;
            case "bad-restraint": api.Restraint(1, 1, new[] { 1, 1, 1 }); break;
        }
        var report = Import(api);
        Assert.AreEqual(ImportStatus.Rejected, report.Status, fault); Assert.IsNull(report.Model);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error), fault);
    }

    private sealed class AssignmentApi : IStraus7ReadApi, IStraus7AssignmentReadApi
    {
        public int FreedomCases = 1;
        private readonly Dictionary<(Straus7Entity, int, int), List<Straus7Attribute>> attributes = new();
        private readonly Dictionary<(int, Straus7LoadFrame, int, int, int), Straus7DistributedLoad> distributed = new();
        private readonly Dictionary<(int, int, int, bool, bool), double[]> points = new();
        private readonly Dictionary<int, Straus7Restraint> restraints = new();
        public readonly Dictionary<(int, int, bool), double[]> NodeLoads = new();
        public readonly Dictionary<int, double[]> BeamOffsets = new(), PlateOffsets = new(), PlateThicknesses = new();
        public readonly Dictionary<(int, int), double[]> NormalPressures = new();
        public readonly Dictionary<(int, int, int), (double[] Values, int Project)> GlobalPressures = new();
        public readonly Dictionary<int, Straus7Ucs> Systems = new();
        public readonly Dictionary<int, Straus7LoadCaseData> Cases = new()
        {
            [1] = new() { Number = 1, Type = 0 }, [2] = new() { Number = 2, Type = 1, GravityDirection = 3, Gravity = -9.80665 }, [3] = new() { Number = 3, Type = 2 }
        };
        public readonly Dictionary<int, Straus7Property> BeamProperties = new()
        {
            [1] = new() { Number = 1, Name = "Column 400x600", Formulation = 6, SectionType = 3, Geometry = new[] { .4, .6, 0, 0, 0, 0 },
                SectionProperties = new[] { .24, .0072, .0032, .0075, 0, 0, 0, 0, .2, .3, 0 }, Material = new[] { 32e6, 13.333e6, .2, 2500, 1e-5 }, MaterialName = "Concrete C30/37" },
            [2] = new() { Number = 2, Name = "Girder I", Formulation = 6, SectionType = 7, Geometry = new[] { .2, .15, .4, .015, .012, .008 },
                SectionProperties = IProperties(.2, .15, .4, .015, .012, .008), Material = new[] { 210e6, 80.77e6, .3, 7850, 1.2e-5 }, MaterialName = "Steel S355" },
        };

        /// <summary>A, I11, I22, J, SL1, SL2, SA1, SA2, XBAR, YBAR, ANGLE of a mono-symmetric I: B1/T1 bottom, B2/T2 top flange, T3 web, D along axis 2.</summary>
        public static double[] IProperties(double b1, double b2, double d, double t1, double t2, double t3)
        {
            var parts = new[] { (W: b1, H: t1, Y: t1 / 2), (W: b2, H: t2, Y: d - t2 / 2), (W: t3, H: d - t1 - t2, Y: t1 + (d - t1 - t2) / 2) };
            double area = parts.Sum(p => p.W * p.H), y = parts.Sum(p => p.W * p.H * p.Y) / area;
            double i11 = parts.Sum(p => p.W * Math.Pow(p.H, 3) / 12 + p.W * p.H * (p.Y - y) * (p.Y - y)), i22 = parts.Sum(p => p.H * Math.Pow(p.W, 3) / 12);
            return new[] { area, i11, i22, parts.Sum(p => p.W * Math.Pow(Math.Min(p.W, p.H), 3) / 3), 0, 0, 0, 0, Math.Max(b1, b2) / 2, y, 0 };
        }

        public void Add(Straus7Entity entity, int number, int attribute, int axis = 0, int lc = 0, int id = 0, int local = 0)
        {
            if (!attributes.TryGetValue((entity, number, attribute), out var list)) attributes[(entity, number, attribute)] = list = new();
            list.Add(new Straus7Attribute { Local = local, Axis = axis, Case = lc, Id = id });
        }
        public void Distributed(int beam, Straus7LoadFrame frame, int direction, int lc, int id, int type, double pa, double pb, double p1, double p2, double a, double b, int project = 0)
        {
            Add(Straus7Entity.Beam, beam, frame switch { Straus7LoadFrame.Principal => Straus7Attributes.BeamDLL, Straus7LoadFrame.Global => Straus7Attributes.BeamDLG, _ => Straus7Attributes.BeamDML },
                direction, lc, id);
            distributed[(beam, frame, direction, lc, id)] = new() { Type = type, Project = project, Values = new[] { pa, pb, p1, p2, a, b } };
        }
        public void Point(int beam, int lc, int id, bool moment, bool global, double x, double y, double z, double position)
        {
            Add(Straus7Entity.Beam, beam, moment ? global ? Straus7Attributes.BeamCMG : Straus7Attributes.BeamCML : global ? Straus7Attributes.BeamCFG : Straus7Attributes.BeamCFL, 0, lc, id);
            points[(beam, lc, id, moment, global)] = new[] { x, y, z, position };
        }
        public void Restraint(int node, int ucs, int[] status, double[]? values = null)
        {
            Add(Straus7Entity.Node, node, Straus7Attributes.Restraint, lc: 1);
            restraints[node] = new() { Ucs = ucs, Status = status, Values = values ?? new double[6] };
        }

        public Straus7Attribute[] ReadAttributes(Straus7Entity entity, int number, int attribute) => attributes.TryGetValue((entity, number, attribute), out var list) ? list.ToArray() : Array.Empty<Straus7Attribute>();
        public int FreedomCaseCount => FreedomCases;
        public Straus7Restraint ReadRestraint(int node, int freedomCase) => restraints[node];
        public Straus7Ucs ReadUcs(int id) => Systems[id];
        public double[] ReadNodeLoad(int node, int loadCase, bool moment) => NodeLoads[(node, loadCase, moment)];
        public double[] ReadBeamOffset(int beam) => BeamOffsets[beam];
        public double[] ReadPlateOffset(int plate) => PlateOffsets[plate];
        public double[] ReadPlateThickness(int plate) => PlateThicknesses[plate];
        public Straus7DistributedLoad ReadBeamDistributedLoad(int beam, Straus7LoadFrame frame, int direction, int loadCase, int id) => distributed[(beam, frame, direction, loadCase, id)];
        public double[] ReadBeamPointLoad(int beam, int loadCase, int id, bool moment, bool global) => points[(beam, loadCase, id, moment, global)];
        public double[] ReadPlateNormalPressure(int plate, int loadCase) => NormalPressures[(plate, loadCase)];
        public double[] ReadPlateGlobalPressure(int plate, int surface, int loadCase, out int project) { var p = GlobalPressures[(plate, surface, loadCase)]; project = p.Project; return p.Values; }
        public Straus7LoadCaseData ReadLoadCaseData(int number) => Cases[number];

        public string Version => "3.1.5-fixture";
        public void OpenModelReadOnly(string path, string scratchPath) { }
        public void CloseModel() { }
        public void Dispose() { }
        public int[] ReadUnits() => new[] { 0, 1, 1, 0, 0, 0 }; // m, kN, kPa, kg, Celsius, J.
        public int Count(Straus7Entity entity) => entity switch { Straus7Entity.Node => 4, Straus7Entity.Beam => 2, Straus7Entity.Plate => 1, _ => 0 };
        public Straus7Node ReadNode(int number) => new() { Number = number, Coordinates = number switch { 1 => new[] { 0d, 0, 0 }, 2 => new[] { 4d, 0, 0 }, 3 => new[] { 4d, 2, 0 }, _ => new[] { 0d, 2, 0 } } };
        public Straus7Element ReadElement(Straus7Entity entity, int number) => entity == Straus7Entity.Beam
            ? new() { Number = number, Nodes = number == 1 ? new[] { 1, 2 } : new[] { 4, 3 }, Property = number, GroupId = 1, Formulation = 6,
                InitialAxes = new[] { 0d, 1, 0, 0, 0, 1, 1, 0, 0 }, Centroid = new[] { 2d, number == 1 ? 0 : 2, 0 } }
            : new() { Number = number, Nodes = new[] { 1, 2, 3, 4 }, Property = 1, GroupId = 1, Formulation = 4, InitialAxes = new[] { 1d, 0, 0, 0, 1, 0, 0, 0, 1 }, Centroid = new[] { 2d, 1, 0 } };
        public Straus7Property ReadProperty(Straus7Entity entity, int number) => entity == Straus7Entity.Beam ? BeamProperties[number]
            : new() { Number = number, Name = "Slab 200", Formulation = 4, MaterialType = 1, Geometry = new[] { .2, .2 }, Material = new[] { 32e6, .2, 2500, 1e-5 }, MaterialName = "Concrete C30/37" };
        public int GroupCount => 1;
        public Straus7Group ReadGroup(int index) => new() { Id = 1, ParentId = -1, Name = "Model" };
        public int LoadCaseCount => 3;
        public string ReadLoadCase(int number) => number switch { 1 => "Loads", 2 => "Gravity", _ => "Seismic" };
        public int StageCount => 0;
        public Straus7ResultFile ValidateResults(string path) => throw new NotSupportedException();
        public Straus7ResultFile OpenResults(string path) => throw new NotSupportedException();
        public void CloseResults() { }
        public Straus7ResultCase ReadResultCase(int number) => throw new NotSupportedException();
        public Straus7ResultTable ReadBeamForces(int number, int resultCase, int minimumStations) => throw new NotSupportedException();
        public Straus7ResultTable ReadPlateResult(int number, int resultCase, bool moments) => throw new NotSupportedException();
        public Straus7ResultTable ReadNodeResult(int number, int resultCase, bool reactions) => throw new NotSupportedException();
    }
}
