using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>Synthetic responses with the schema returned by a real Civil NX model through MAPI (padded NODE arrays,
/// {"message":""} for empty databases, string result cells); values are invented, not solver output.</summary>
[TestClass]
public class CivilNxModelWorkflowTest
{
    private const string Empty = "{\"message\":\"\"}";
    // Units KN, M. Column 1-2 vertical along +Z from (0,0,0); beam 3 horizontal along +X at z=3; plate 10 horizontal, plate 11 the same nodes rotated by ANGLE 90.
    internal static Dictionary<string, string> Json() => new()
    {
        ["UNIT"] = "{\"UNIT\":{\"1\":{\"FORCE\":\"KN\",\"DIST\":\"M\",\"HEAT\":\"KJ\",\"TEMPER\":\"C\"}}}",
        ["NODE"] = "{\"NODE\":{\"1\":{\"X\":0,\"Y\":0,\"Z\":0},\"2\":{\"X\":0,\"Y\":0,\"Z\":3},\"3\":{\"X\":4,\"Y\":0,\"Z\":3},\"4\":{\"X\":4,\"Y\":2,\"Z\":3},\"5\":{\"X\":0,\"Y\":2,\"Z\":3}}}",
        ["ELEM"] = "{\"ELEM\":{\"1\":{\"TYPE\":\"BEAM\",\"MATL\":1,\"SECT\":1,\"NODE\":[1,2,0,0,0,0,0,0],\"ANGLE\":0,\"STYPE\":0},"
            + "\"3\":{\"TYPE\":\"BEAM\",\"MATL\":2,\"SECT\":2,\"NODE\":[2,3,0,0,0,0,0,0],\"ANGLE\":0,\"STYPE\":0},"
            + "\"10\":{\"TYPE\":\"PLATE\",\"MATL\":1,\"SECT\":1,\"NODE\":[2,3,4,5,0,0,0,0],\"ANGLE\":0,\"STYPE\":1},"
            + "\"11\":{\"TYPE\":\"PLATE\",\"MATL\":1,\"SECT\":2,\"NODE\":[2,3,4,5,0,0,0,0],\"ANGLE\":90,\"STYPE\":1}}}",
        ["MATL"] = "{\"MATL\":{\"1\":{\"TYPE\":\"CONC\",\"NAME\":\"C30/37\",\"PARAM\":[{\"P_TYPE\":1,\"STANDARD\":\"EN04(RC)\",\"CODE\":\"\",\"DB\":\"C30/37\",\"bELAST\":false,\"ELAST\":32836000}]},"
            + "\"2\":{\"TYPE\":\"STEEL\",\"NAME\":\"S355\",\"PARAM\":[{\"P_TYPE\":1,\"STANDARD\":\"EN05(S)\",\"CODE\":\"\",\"DB\":\"S355\",\"bELAST\":false,\"ELAST\":210000000}]}}}",
        ["SECT"] = "{\"SECT\":{\"1\":{\"SECTTYPE\":\"DBUSER\",\"SECT_NAME\":\"Pila\",\"SECT_BEFORE\":{\"OFFSET_PT\":\"CC\",\"OFFSET_CENTER\":0,\"SHAPE\":\"SB\",\"DATATYPE\":2,\"SECT_I\":{\"vSIZE\":[1.2,0.8,0,0,0,0,0,0,0,0]}}},"
            + "\"2\":{\"SECTTYPE\":\"DBUSER\",\"SECT_NAME\":\"HEB300\",\"SECT_BEFORE\":{\"OFFSET_PT\":\"CT\",\"OFFSET_CENTER\":0,\"SHAPE\":\"H\",\"DATATYPE\":1,\"SECT_I\":{\"DB_NAME\":\"EN10365\",\"SECT_NAME\":\"HE 300 B\"}}}}}",
        ["THIK"] = "{\"THIK\":{\"1\":{\"NAME\":\"Soletta\",\"TYPE\":\"VALUE\",\"bINOUT\":false,\"T_IN\":0.3,\"T_OUT\":0,\"O_VALUE\":0},"
            + "\"2\":{\"NAME\":\"Offset\",\"TYPE\":\"VALUE\",\"bINOUT\":false,\"T_IN\":0.6,\"T_OUT\":0,\"OFFSET\":2,\"O_VALUE\":-0.1}}}",
        ["GRUP"] = "{\"GRUP\":{\"1\":{\"NAME\":\"Impalcato\",\"P_TYPE\":0,\"N_LIST\":[2,3],\"E_LIST\":[3,10]}}}",
        ["SKEW"] = Empty, ["OFFS"] = Empty, ["FRLS"] = Empty, ["LCOM-GEN"] = Empty,
        ["CONS"] = "{\"CONS\":{\"1\":{\"ITEMS\":[{\"ID\":1,\"GROUP_NAME\":\"Base\",\"CONSTRAINT\":\"1111100\"}]}}}",
        ["STLD"] = "{\"STLD\":{\"1\":{\"NO\":1,\"NAME\":\"G1\",\"TYPE\":\"D\",\"DESC\":\"\"},\"2\":{\"NO\":2,\"NAME\":\"Q\",\"TYPE\":\"L\",\"DESC\":\"\"}}}",
        ["CNLD"] = "{\"CNLD\":{\"3\":{\"ITEMS\":[{\"ID\":1,\"LCNAME\":\"Q\",\"GROUP_NAME\":\"\",\"FX\":0,\"FY\":5,\"FZ\":-20,\"MX\":1,\"MY\":0,\"MZ\":0}]}}}",
        ["BMLD"] = "{\"BMLD\":{\"3\":{\"ITEMS\":[{\"ID\":1,\"LCNAME\":\"G1\",\"GROUP_NAME\":\"\",\"CMD\":\"BEAM\",\"TYPE\":\"UNILOAD\",\"DIRECTION\":\"GZ\",\"USE_PROJECTION\":false,\"USE_ECCEN\":false,\"D\":[0,1,0,0],\"P\":[-10,-10,0,0]},"
            + "{\"ID\":2,\"LCNAME\":\"Q\",\"GROUP_NAME\":\"\",\"CMD\":\"BEAM\",\"TYPE\":\"CONLOAD\",\"DIRECTION\":\"LZ\",\"USE_PROJECTION\":false,\"USE_ECCEN\":false,\"D\":[0.25,0,0,0],\"P\":[-8,0,0,0]},"
            + "{\"ID\":3,\"LCNAME\":\"Q\",\"GROUP_NAME\":\"\",\"CMD\":\"BEAM\",\"TYPE\":\"UNILOAD\",\"DIRECTION\":\"GZ\",\"USE_PROJECTION\":false,\"USE_ECCEN\":false,\"D\":[0.2,0.5,0.8,0],\"P\":[-1,-3,-3,0]}]}}}",
        ["PRES"] = "{\"PRES\":{\"10\":{\"ITEMS\":[{\"ID\":1,\"LCNAME\":\"G1\",\"GROUP_NAME\":\"\",\"CMD\":\"PRES\",\"ELEM_TYPE\":\"PLATE\",\"FACE_EDGE_TYPE\":\"FACE\",\"DIRECTION\":\"LZ\",\"FORCES\":[-2,0,0,0,0]},"
            + "{\"ID\":2,\"LCNAME\":\"Q\",\"GROUP_NAME\":\"\",\"CMD\":\"HYDRO\",\"ELEM_TYPE\":\"PLATE\",\"FACE_EDGE_TYPE\":\"FACE\",\"DIRECTION\":\"LZ\",\"FORCES\":[0,1,1,2,2]},"
            + "{\"ID\":3,\"LCNAME\":\"Q\",\"GROUP_NAME\":\"\",\"CMD\":\"PRES\",\"ELEM_TYPE\":\"PLATE\",\"FACE_EDGE_TYPE\":\"EDGE\",\"DIRECTION\":\"GZ\",\"OPT_PROJECTION\":false,\"EDGE_LOADS\":[-3,0,0],\"EDGE_FACE\":2}]}}}",
        ["BODF"] = "{\"BODF\":{\"1\":{\"LCNAME\":\"G1\",\"GROUP_NAME\":\"\",\"FV\":[0,0,-1]}}}",
        ["NBOF"] = "{\"NBOF\":{\"1\":{\"LCNAME\":\"Q\",\"OPT_USE_GROUP\":false,\"KEY_NODE_ITEMS\":[2,3],\"X\":0.3,\"Y\":0,\"Z\":0}}}",
        ["MATERIAL"] = "{\"T\":{\"FORCE\":\"N\",\"DIST\":\"mm\",\"HEAD\":[\"Index\",\"ID\",\"Name\",\"Type\",\"Density\",\"Poisson\"],\"DATA\":[[\"1\",\"1\",\"C30/37\",\"Concrete\",\"2.5000e-05\",\"0.2\"],[\"2\",\"2\",\"S355\",\"Steel\",\"7.6982e-05\",\"0.3\"]]}}",
    };
    internal static CivilNxSnapshot Snapshot(Dictionary<string, string> json) => new(json.ToDictionary(p => p.Key, p => new CivilNxResponse("db/" + p.Key, p.Value)));
    internal static AnalysisSource Identity() => new() { Program = "MIDAS Civil NX", SolverVersion = "fixture", ModelRevision = "rev", AnalysisId = "analysis" };
    private static ImportReport Import(Dictionary<string, string>? json = null)
    {
        var report = CivilNxGeometryReader.Import(Snapshot(json ?? Json()), Identity(), new CivilNxModelProfile());
        Assert.AreEqual(ImportStatus.Partial, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Code + ": " + d.Message)));
        return report;
    }
    private static T Find<T>(GPC.Model.Models.Model m, EntityFamily family, string id) where T : class =>
        (T)(object)m.FindBySource(new SourceIdentity("MIDAS Civil NX", "rev", family, id))!;
    private static void Vector(Vector3d v, double x, double y, double z) { Assert.AreEqual(x, v.X, 1e-9); Assert.AreEqual(y, v.Y, 1e-9); Assert.AreEqual(z, v.Z, 1e-9); }

    [TestMethod]
    public void Model_MapsGeometryAxesPropertiesGroupsAndSupports()
    {
        var report = Import(); var m = report.Model;
        Assert.AreEqual(5, m.NodesElements.Count); Assert.AreEqual(2, m.BeamElements.Count); Assert.AreEqual(2, m.AreaElements.Count);
        var column = Find<BeamElement>(m, EntityFamily.Beam, "1");
        Assert.AreEqual(3000, column.Length, 1e-9);
        Vector(column.Assignments.SectionAxes.V2, 1, 0, 0); // vertical member: MIDAS z (GPC V2) = GCS X at beta 0.
        var rc = (ReinforcedConcreteSection)column.BeamProperty; Assert.AreEqual(1200 * 800, rc.Area, 1e-6);
        var girder = Find<BeamElement>(m, EntityFamily.Beam, "3");
        var steel = (SteelSection)girder.BeamProperty; Assert.AreEqual(355, steel.SteelMaterial.Fyk);
        Assert.AreEqual(-150, girder.Assignments.SectionCentroidOffset.Y, 1e-6, "CT: reference line at the top of HE 300 B.");
        var plate = Find<AreaElement>(m, EntityFamily.Shell, "10"); var rotated = Find<AreaElement>(m, EntityFamily.Shell, "11");
        Vector(plate.CoordinateSystem.V1, 1, 0, 0); Vector(plate.CoordinateSystem.V3, 0, 0, 1);
        Vector(rotated.CoordinateSystem.V1, 0, 1, 0); Vector(rotated.CoordinateSystem.V3, 0, 0, 1);
        Assert.AreEqual(300, ((ConcretePlateProperty)plate.PlateProperty).MembraneThickness);
        Assert.AreEqual(-100, rotated.Assignments.Offset); Assert.AreEqual(600, rotated.Assignments.PhysicalThickness);
        Assert.AreEqual(4, m.GetGroupElements("Impalcato").Count);
        var support = Find<NodeElement>(m, EntityFamily.Node, "1").Assignments.Restrains.Single().Restrain;
        Assert.AreEqual(5, support.Restrains.Count);
        var gravity = m.ModelLoads.Values.OfType<ModelGravityLoad>().Single();
        Assert.AreEqual(-1, gravity.GravityVersor.Z); Assert.AreEqual(ModelGravityLoad.GRAVITYACCELERATION, gravity.Acceleration);
        Assert.AreEqual(GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight, ((GPC.Model.LoadCases.LoadCase)m.LoadCases["G1"]).LoadCaseType);
        Assert.AreEqual(2.5e-5 / 9806.65, rc.ConcreteMaterial.Density, 1e-18, "Weight density of the analysis (MATERIAL table), as mass density.");
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilNxNBOFPreserved"));
        Assert.IsNotNull(m.PreservedSourceData.Single(p => p.Kind == CivilNxGeometryReader.InputBindingKind));
    }

    [TestMethod]
    public void Loads_ConvertUnitsAxesAndSegments_PreservingVaryingPressures()
    {
        var report = Import(); var m = report.Model;
        var node = Find<NodeElement>(m, EntityFamily.Node, "3").Loads.Values.Cast<PointLoad>().Single();
        Assert.AreEqual(5000, node.F2); Assert.AreEqual(-20000, node.F3); Assert.AreEqual(1e6, node.M1);
        var girder = Find<BeamElement>(m, EntityFamily.Beam, "3"); var loads = girder.Assignments.Loads;
        Assert.AreEqual(4, loads.Count, "Uniform load, point load and a three-point load in two segments.");
        Assert.AreEqual(-10, loads[0].StartIntensity.F3, 1e-12); // kN/m = N/mm
        Assert.AreEqual(-8000, loads[1].Concentrated.F2); Assert.AreSame(girder.Assignments.SectionAxes, loads[1].Concentrated.CoordinateSystem);
        Assert.AreEqual(1000, loads[1].Concentrated.Point.X, 1e-9);
        Assert.AreEqual(-40000, Equilibrium.BeamLoad(girder, loads[0]).Force.Z, 1e-6);
        Assert.AreEqual(-(1 + 3) / 2.0 * 1200 - 3 * 1200, loads.Skip(2).Sum(l => Equilibrium.BeamLoad(girder, l).Force.Z), 1e-6);
        var plate = Find<AreaElement>(m, EntityFamily.Shell, "10");
        Assert.AreEqual(-0.002, plate.Loads.Values.OfType<NormalAreaLoad>().Single().Pressure, 1e-15);
        var edge = plate.Loads.Values.OfType<LineLoad>().Single();
        Assert.AreEqual(-3, edge.F3); Assert.AreEqual(4000, edge.Line.Start.X); Assert.AreEqual(2000, edge.Line.End.Y, 1e-9);
        var hydro = plate.Loads.Values.OfType<NonUniformPlatePressure>().Single();
        CollectionAssert.AreEqual(new[] { 0.001, 0.001, 0.002, 0.002 }, hydro.Pressures.ToArray());
        Assert.AreEqual(1.5e-3 * 8e6, hydro.GetGlobalLoadVector().Z, 1e-6, "Mean 1.5 kN/m² on 4 x 2 m along the plate normal +Z.");
    }

    [TestMethod]
    public void EmptyAnswersAreEmpty_ButErrorMessagesAndUnknownElementsReject()
    {
        var json = Json(); json["CNLD"] = Empty; json["BMLD"] = Empty; json["PRES"] = Empty;
        Assert.AreEqual(0, Import(json).Model.NodesElements.Values.Sum(n => n.Loads.Count));
        json = Json(); json["CONS"] = "{\"message\":\"database error\"}";
        Assert.AreEqual(ImportStatus.Rejected, CivilNxGeometryReader.Import(Snapshot(json), Identity(), new CivilNxModelProfile()).Status);
        json = Json(); json["ELEM"] = json["ELEM"].Replace("\"TYPE\":\"BEAM\",\"MATL\":1", "\"TYPE\":\"SOLID\",\"MATL\":1");
        Assert.AreEqual(ImportStatus.Rejected, CivilNxGeometryReader.Import(Snapshot(json), Identity(), new CivilNxModelProfile()).Status);
    }

    private static CivilNxResponse Table(string head, params string[] rows) =>
        new("post/table", "{\"T\":{\"FORCE\":\"KN\",\"DIST\":\"M\",\"HEAD\":[" + head + "],\"DATA\":[" + string.Join(",", rows) + "]}}");

    [TestMethod]
    public void Results_UseVerifiedSignConventionsAndBindToTheUnchangedModel()
    {
        var m = Import().Model; var snapshot = Snapshot(Json());
        var tables = new[]
        {
            Table("\"Index\",\"Elem\",\"Load\",\"Part\",\"Axial\",\"Shear-y\",\"Shear-z\",\"Torsion\",\"Moment-y\",\"Moment-z\"",
                "[\"1\",\"3\",\"Q\",\"I[2]\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]", "[\"2\",\"3\",\"Q\",\"1/4\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]",
                "[\"3\",\"3\",\"Q\",\"J[3]\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]"),
            Table("\"Index\",\"Elem\",\"Load\",\"Node\",\"Fxx\",\"Fyy\",\"Fxy\",\"Fmax\",\"Fmin\",\"Angle\",\"Mxx\",\"Myy\",\"Mxy\",\"Mmax\",\"Mmin\",\"Angle\",\"Vxx\",\"Vyy\"",
                "[\"1\",\"11\",\"Q\",\"Cent\",\"1\",\"2\",\"3\",\"0\",\"0\",\"0\",\"4\",\"5\",\"6\",\"0\",\"0\",\"0\",\"7\",\"8\"]",
                "[\"2\",\"11\",\"Q\",\"2\",\"9\",\"9\",\"9\",\"0\",\"0\",\"0\",\"9\",\"9\",\"9\",\"0\",\"0\",\"0\",\"9\",\"9\"]"),
            Table("\"Index\",\"Node\",\"Load\",\"FX\",\"FY\",\"FZ\",\"MX\",\"MY\",\"MZ\"", "[\"1\",\"1\",\"Q\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]"),
            Table("\"Index\",\"Node\",\"Load\",\"DX\",\"DY\",\"DZ\",\"RX\",\"RY\",\"RZ\"", "[\"1\",\"3\",\"Q\",\"0.001\",\"0\",\"-0.002\",\"0\",\"0.0001\",\"0\"]"),
        };
        var report = CivilNxResults.Import(m, snapshot, tables, "dataset");
        Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        Assert.AreEqual(3 + 2 + 1 + 1, report.ImportedSamples, "Centre and element-node plate rows are both imported when present.");
        var girder = Find<BeamElement>(m, EntityFamily.Beam, "3");
        var quarter = girder.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().Single(s => s.ParametricDistance == .25).ResultBeamForces;
        Assert.AreEqual(1000, quarter.N); Assert.AreEqual(2000, quarter.V1); Assert.AreEqual(3000, quarter.V2);
        Assert.AreEqual(4e6, quarter.T); Assert.AreEqual(-5e6, quarter.M1); Assert.AreEqual(6e6, quarter.M2);
        var plate = Find<AreaElement>(m, EntityFamily.Shell, "11").Results.SelectMany(r => r.Results).OfType<PointResultPlateForces>().Single(s => s.PointKind == ShellResultPointKind.Centroid).Forces;
        Assert.AreEqual(1, plate.Fxx); Assert.AreEqual(7, plate.Fxz); Assert.AreEqual(-4000, plate.Mxx); Assert.AreEqual(-6000, plate.Mxy);
        Vector(plate.CoordinateSystem.V1, 0, 1, 0);
        var reaction = Find<NodeElement>(m, EntityFamily.Node, "1").Results.SelectMany(r => r.Results).OfType<NodeResultForces>().Single();
        Assert.AreEqual(NodalForceKind.SupportReaction, reaction.Kind); Assert.AreEqual(3000, reaction.Fz); Assert.AreEqual(6e6, reaction.Mz);
        Assert.AreEqual(1, Find<NodeElement>(m, EntityFamily.Node, "3").Results.SelectMany(r => r.Results).OfType<NodeResultDisplacement>().Single().ResultDisplacement.D1, 1e-12);
    }

    [TestMethod]
    public void Results_AreRejectedForAChangedSourceOrModelAndUnknownParts()
    {
        var m = Import().Model;
        var table = Table("\"Index\",\"Elem\",\"Load\",\"Part\",\"Axial\",\"Shear-y\",\"Shear-z\",\"Torsion\",\"Moment-y\",\"Moment-z\"", "[\"1\",\"3\",\"Q\",\"I[2]\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]");
        var changed = Json(); changed["NODE"] = changed["NODE"].Replace("\"X\":4,\"Y\":0", "\"X\":4.5,\"Y\":0");
        Assert.AreEqual(ImportStatus.Rejected, CivilNxResults.Import(m, Snapshot(changed), new[] { table }, "d1").Status);
        var wrongPart = Table("\"Index\",\"Elem\",\"Load\",\"Part\",\"Axial\",\"Shear-y\",\"Shear-z\",\"Torsion\",\"Moment-y\",\"Moment-z\"", "[\"1\",\"3\",\"Q\",\"I[3]\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]");
        Assert.AreEqual(ImportStatus.Rejected, CivilNxResults.Import(m, Snapshot(Json()), new[] { wrongPart }, "d2").Status);
        Find<NodeElement>(m, EntityFamily.Node, "3").Loads.Add(new PointLoad(1, 0, 0, 0, 0, 0, new Point3d(4000, 0, 3000), m.LoadCases["Q"]));
        Assert.AreEqual(ImportStatus.Rejected, CivilNxResults.Import(m, Snapshot(Json()), new[] { table }, "d3").Status, "Model inputs changed after the import.");
        Assert.AreEqual(0, m.Datasets.Count);
    }
}
