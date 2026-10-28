using System.Text;
using System.Text.Json;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Converter.MidasCivil;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Attributes;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using GPC.Model.Structure.Assignments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>MCT/MGT commands read through the Civil NX reader: the text and the API give the same model. Native fixtures are Civil NX 9.5.5
/// exports (MCT + JSON of the same model); the synthetic files follow the record layouts written by the MIDAS exports.</summary>
[TestClass]
public class MidasTextCoverageTest
{
    private static ImportReport Text(string text, string program = MidasTextFiles.CivilProgram, string revision = "rev")
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        IModelFileReader reader = program == MidasTextFiles.GenProgram ? new MidasGenTextReader(revision) : new MidasCivilTextReader(revision);
        return new SolverFileAdapter(program, reader).Import(stream);
    }
    private static void Partial(ImportReport report) =>
        Assert.AreEqual(ImportStatus.Partial, report.Status, string.Join("; ", report.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.Code + ": " + d.Message)));
    private static T Find<T>(GPC.Model.Models.Model m, EntityFamily family, string id, string program = "MIDAS Civil") where T : class =>
        (T)(object)m.FindBySource(new SourceIdentity(program, m.AnalysisSource.ModelRevision, family, id))!;
    private static void Vector(Vector3d v, double x, double y, double z, double tolerance = 1e-9)
    { Assert.AreEqual(x, v.X, tolerance); Assert.AreEqual(y, v.Y, tolerance); Assert.AreEqual(z, v.Z, tolerance); }
    private static string Fixture(string folder) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "MidasCivil", folder);

    // ---- Native Civil NX 9.5.5: the MCT export and the API databases of the same model ----

    [DataTestMethod]
    [DataRow("identical-pairs-native-9.5.5", "packed-pairs-full.mct", "pairs-native-20261022")]
    [DataRow("catalog-pairs-native-9.5.5", "catalog-pairs-full.mct", "catalog-pairs-native-20261023")]
    public void NativeMctAndApiGiveTheSameModel(string folder, string file, string revision)
    {
        var names = CivilNxModelProfile.RequiredTables.Concat(CivilNxModelProfile.OptionalTables).Concat(new[] { "MATERIAL", "SECTIONALL" });
        var snapshot = new CivilNxSnapshot(names.ToDictionary(n => n, n => new CivilNxResponse(n is "SECTIONALL" or "MATERIAL" ? "post/table" : "db/" + n,
            File.ReadAllText(Path.Combine(Fixture(folder), n + ".json")))));
        var api = CivilNxGeometryReader.Import(snapshot, new AnalysisSource { Program = "MIDAS Civil NX", SolverVersion = "9.5.5", ModelRevision = revision, AnalysisId = "a" },
            new CivilNxModelProfile());
        Assert.AreEqual(ImportStatus.Partial, api.Status);
        using var stream = File.OpenRead(Path.Combine(Fixture(folder), file));
        var text = new MidasCivilTextReader(revision).Import(stream); Partial(text);
        ModelSignature.AssertEqual(api.Model, text.Model);
        Assert.IsTrue(text.Model.NodesElements.Values.Any(n => n.Loads.Count != 0) && text.Model.NodesElements.Values.Any(n => n.Assignments.Restrains.Count != 0));
    }

    // ---- Synthetic model with every mapped command, and the same model as API databases ----

    internal const string Coverage = @"; synthetic MCT, layouts of the MIDAS Civil NX exports
*VERSION
   synthetic-coverage
*UNIT    ; Unit System
   KN   , M, KJ, C
*NODE
     1, 0, 0, 0
     2, 0, 0, 3
     3, 6, 0, 3
     4, 6, 4, 3
     5, 0, 4, 3
     6, 6, 0, 0
     9, 6, 4, 0
    10, 0, 4, 0
*MATERIAL
    1, CONC , C30/37, 0, 0, , C, NO, 0.05, 1, EN04(RC)  ,            , C30/37        , NO, 3.2836e+07
    2, STEEL, S355  , 0, 0, , C, NO, 0.02, 1, EN05(S)   ,            , S355          , NO, 2.1e+08
    3, CONC , C35/45 user, 0, 0, , C, NO, 0.05, 2,  3.4e+07,   0.2,  1.0000e-05,     25,     2.5493
*SECTION    ; Section
    1, DBUSER    , Pila  , CC, 0, 0, 0, 0, 0, 0, YES, NO, SB , 2, 0.6, 0.4, 0, 0, 0, 0, 0, 0, 0, 0
    2, DBUSER    , Trave , CT, 0, 0, 0, 0, 0, 0, YES, NO, H  , 2, 0.4, 0.2, 0.01, 0.016, 0.2, 0.016, 0, 0, 0, 0
    3, DBUSER    , Tubo  , CC, 0, 0, 0, 0, 0, 0, YES, NO, P  , 2, 0.1683, 0.008, 0, 0, 0, 0, 0, 0, 0, 0
    4, VALUE     , Tirante, CC, 0, 0, 0, 0, 0, 0, YES, NO, SR , BUILT, 0.03, 0, 0, 0, 0, 0, 0, 0, 0
       0.000706858, 0, 0, 7.95e-08, 3.976e-08, 3.976e-08
       0.015, 0.015, 0.015, 0.015, 0, 0, 0.0942, 0, 0, 0
       0, 0, 0, 0, 0, 0, 0, 0, 0, 0
*THICKNESS    ; Thickness
    1, VALUE, Soletta, YES, 0.25, 0,  NO, 0, 0
    2, VALUE, Sbalzo, YES, 0.2, 0,  YES, 0, 0.5
*ELEMENT    ; Elements
     1, BEAM  ,    1,     1,     1,     2,     0,     0
     2, BEAM  ,    1,     1,     6,     3,    90,     0
     3, BEAM  ,    2,     2,     2,     3,     0,     0
     4, TRUSS ,    2,     3,     1,     3,     0,     0
     5, TENSTR,    2,     4,     6,     2,     0,     1,     0,     0,    NO
    10, PLATE ,    1,     1,     2,     3,     4,     5,     1,     0
    11, PLATE ,    3,     2,     2,     3,     4,     0,     2,    30
*GROUP    ; Group
   Deck, 2to5, 3 10 11, 0
*LOCALAXIS
   6, 3, 1, 1, 0, -1, 1, 0
*CONSTRAINT    ; Supports
   1, 111111, Base
   6, 111000, Base
   10, 1111110, Base
*OFFSET    ; Beam End Offsets
   3, GLOBAL , 0.1, 0, 0, -0.1, 0, 0,
*FRAME-RLS    ; Beam End Release
   3,  NO, 000011, 0, 0, 0, 0, 0, 0
             000001, 0, 0, 0, 0, 0, 0.25, Rel
*ELASTICLINK    ; Elastic Link
     1,     4,     9, GEN  ,     0, NO, NO, NO, NO, NO, NO, 100, 200, 300, 400, 500, 600, NO, 0.5, 0.5, Links
     2,     5,    10, RIGID,     0, NO, 0.5, 0.5, Links
*RIGIDLINK    ; Rigid Link
   2, 111111, 5,
*STLDCASE    ; Static Load Cases
   G, D , permanent
   Q, L , live
   W, W , wind
*USE-STLD, G
*SELFWEIGHT    ; Self Weight
0, 0, -1, LG
*USE-STLD, Q
*CONLOAD    ; Nodal Loads
   3, 0, 5, -20, 1, 0, 0, ,
*BEAMLOAD    ; Element Beam Loads
   3, BEAM   , UNILOAD, GZ, NO , NO, aDir[1], , , , 0, -10, 1, -10, 0, 0, 0, 0, , NO, 0, 0, NO,
   3, BEAM   , CONLOAD, LZ, NO , NO, aDir[1], , , , 0.25, -8, 0, 0, 0, 0, 0, 0, , NO, 0, 0, NO,
   3, BEAM   , UNILOAD, GZ, NO , NO, aDir[1], , , , 0.2, -1, 0.5, -3, 0.8, -3, 0, 0, , NO, 0, 0, NO,
   1, BEAM   , UNIMOMENT, LX, NO , NO, aDir[1], , , , 0, 2, 1, 2, 0, 0, 0, 0, , NO, 0, 0, NO,
*PRESSURE    ; Pressure Loads
    10, PRES , PLATE, FACE, LZ, 0, 0, 0, NO, -2, 0, 0, 0, 0, ,0
    10, HYDRO, PLATE, FACE, LZ, 0, 0, 0, NO, 0, 1, 1, 2, 2, ,0
    10, PRES , PLATE, EDGE, 2, GZ, 0, 0, 0, -3, 0, 0,
*USE-STLD, W
*PRESSURE    ; Pressure Loads
    11, PRES , PLATE, FACE, GX, 0, 0, 0, NO, 1.5, 0, 0, 0, 0, ,0
*LOADCOMB    ; Combinations
   NAME=C1, GEN, ACTIVE, 0, 0, , 0, 0, 0
        ST, G, 1.35, ST, Q, 1.5
   NAME=ENV, GEN, ACTIVE, 0, 1, , 0, 0, 0
        CB, C1, 1
        ST, W, 1
   NAME=S1, STEEL, STRENGTH, 0, 0, , 0, 0, 0, 1
        ST, G, 1, ST, W, 1.5
*ENDDATA
";

    private static Dictionary<string, object> Rows(params (string Key, object Row)[] rows) => rows.ToDictionary(r => r.Key, r => r.Row);
    private static object Items(params object[] items) => new Dictionary<string, object> { ["ITEMS"] = items };

    /// <summary>The model of <see cref="Coverage"/> as Civil NX databases (schemas of CivilNxModelWorkflowTest and CivilNxBoundaryTest).</summary>
    private static CivilNxSnapshot CoverageApi() => new CivilNxSnapshot(CoverageApiJson().ToDictionary(t => t.Key, t => new CivilNxResponse("db/" + t.Key, t.Value)));

    /// <summary>The API databases of <see cref="Coverage"/>, table name → JSON response.</summary>
    internal static Dictionary<string, string> CoverageApiJson()
    {
        object Section(string name, string offset, string shape, params double[] size) => new Dictionary<string, object> { ["SECTTYPE"] = "DBUSER", ["SECT_NAME"] = name,
            ["SECT_BEFORE"] = new Dictionary<string, object> { ["OFFSET_PT"] = offset, ["OFFSET_CENTER"] = 0, ["SHAPE"] = shape, ["DATATYPE"] = 2,
                ["SECT_I"] = new Dictionary<string, object> { ["vSIZE"] = size.Concat(Enumerable.Repeat(0d, 10 - size.Length)).ToArray() } } };
        object Element(string type, int material, int section, int[] nodes, double angle, int subtype) => new Dictionary<string, object>
            { ["TYPE"] = type, ["MATL"] = material, ["SECT"] = section, ["NODE"] = nodes.Concat(Enumerable.Repeat(0, 8 - nodes.Length)).ToArray(), ["ANGLE"] = angle, ["STYPE"] = subtype };
        object BeamLoad(int id, string type, string direction, double[] d, double[] p) => new Dictionary<string, object> { ["ID"] = id, ["LCNAME"] = "Q", ["GROUP_NAME"] = "",
            ["CMD"] = "BEAM", ["TYPE"] = type, ["DIRECTION"] = direction, ["USE_PROJECTION"] = false, ["USE_ECCEN"] = false, ["D"] = d, ["P"] = p };
        var tables = new Dictionary<string, object>
        {
            ["UNIT"] = Rows(("1", new { FORCE = "KN", DIST = "M", HEAT = "KJ", TEMPER = "C" })),
            ["NODE"] = Rows(("1", new { X = 0, Y = 0, Z = 0 }), ("2", new { X = 0, Y = 0, Z = 3 }), ("3", new { X = 6, Y = 0, Z = 3 }), ("4", new { X = 6, Y = 4, Z = 3 }),
                ("5", new { X = 0, Y = 4, Z = 3 }), ("6", new { X = 6, Y = 0, Z = 0 }), ("9", new { X = 6, Y = 4, Z = 0 }), ("10", new { X = 0, Y = 4, Z = 0 })),
            ["MATL"] = Rows(("1", new { TYPE = "CONC", NAME = "C30/37", PARAM = new[] { new { P_TYPE = 1, STANDARD = "EN04(RC)", DB = "C30/37", ELAST = 3.2836e7 } } }),
                ("2", new { TYPE = "STEEL", NAME = "S355", PARAM = new[] { new { P_TYPE = 1, STANDARD = "EN05(S)", DB = "S355", ELAST = 2.1e8 } } }),
                ("3", new { TYPE = "CONC", NAME = "C35/45 user", PARAM = new[] { new Dictionary<string, object> { ["P_TYPE"] = 2, ["ELAST"] = 3.4e7, ["POISN"] = 0.2, ["THERMAL"] = 1e-5, ["DEN"] = 25 } } })),
            ["SECT"] = Rows(("1", Section("Pila", "CC", "SB", .6, .4)), ("2", Section("Trave", "CT", "H", .4, .2, .01, .016, .2, .016)), ("3", Section("Tubo", "CC", "P", .1683, .008)),
                ("4", new Dictionary<string, object> { ["SECTTYPE"] = "VALUE", ["SECT_NAME"] = "Tirante", ["SECT_BEFORE"] = new Dictionary<string, object> { ["OFFSET_PT"] = "CC", ["SHAPE"] = "SR",
                    ["SECT_I"] = new Dictionary<string, object> { ["STIFF"] = new { AREA = 0.000706858, RXX = 7.95e-08, RYY = 3.976e-08, RZZ = 3.976e-08 } } } })),
            ["THIK"] = Rows(("1", new Dictionary<string, object> { ["NAME"] = "Soletta", ["TYPE"] = "VALUE", ["bINOUT"] = false, ["T_IN"] = .25, ["T_OUT"] = 0, ["OFFSET"] = 0, ["O_VALUE"] = 0 }),
                ("2", new Dictionary<string, object> { ["NAME"] = "Sbalzo", ["TYPE"] = "VALUE", ["bINOUT"] = false, ["T_IN"] = .2, ["T_OUT"] = 0, ["OFFSET"] = 1, ["O_VALUE"] = .5 })),
            ["ELEM"] = Rows(("1", Element("BEAM", 1, 1, new[] { 1, 2 }, 0, 0)), ("2", Element("BEAM", 1, 1, new[] { 6, 3 }, 90, 0)), ("3", Element("BEAM", 2, 2, new[] { 2, 3 }, 0, 0)),
                ("4", Element("TRUSS", 2, 3, new[] { 1, 3 }, 0, 0)), ("5", Element("TENSTR", 2, 4, new[] { 6, 2 }, 0, 1)),
                ("10", Element("PLATE", 1, 1, new[] { 2, 3, 4, 5 }, 0, 1)), ("11", Element("PLATE", 3, 2, new[] { 2, 3, 4 }, 30, 2))),
            ["GRUP"] = Rows(("1", new { NAME = "Deck", N_LIST = new[] { 2, 3, 4, 5 }, E_LIST = new[] { 3, 10, 11 } })),
            ["SKEW"] = Rows(("6", new { iMETHOD = 3, V1X = 1, V1Y = 1, V1Z = 0, V2X = -1, V2Y = 1, V2Z = 0 })),
            ["CONS"] = Rows(("1", Items(new { ID = 1, GROUP_NAME = "Base", CONSTRAINT = "111111" })), ("6", Items(new { ID = 1, GROUP_NAME = "Base", CONSTRAINT = "111000" })),
                ("10", Items(new { ID = 1, GROUP_NAME = "Base", CONSTRAINT = "1111110" }))),
            ["OFFS"] = Rows(("3", Items(new { TYPE = "GLOBAL", RGDXi = .1, RGDYi = 0, RGDZi = 0, RGDXj = -.1, RGDYj = 0, RGDZj = 0 }))),
            ["FRLS"] = Rows(("3", Items(new { GROUP_NAME = "Rel", bVALUE = false, FLAG_I = "000011", VALUE_I = new double[6], FLAG_J = "000001", VALUE_J = new[] { 0, 0, 0, 0, 0, .25 } }))),
            ["ELNK"] = Rows(("1", new { NODE = new[] { 4, 9 }, LINK = "GEN", ANGLE = 0, R_S = new bool[6], SDR = new double[] { 100, 200, 300, 400, 500, 600 }, bSHEAR = false }),
                ("2", new { NODE = new[] { 5, 10 }, LINK = "RIGID", ANGLE = 0, bSHEAR = false })),
            ["RIGD"] = Rows(("2", Items(new { ID = 1, DOF = 111111, S_NODE = new[] { 5 } }))),
            ["STLD"] = Rows(("1", new { NAME = "G", TYPE = "D" }), ("2", new { NAME = "Q", TYPE = "L" }), ("3", new { NAME = "W", TYPE = "W" })),
            ["BODF"] = Rows(("1", new { LCNAME = "G", GROUP_NAME = "LG", FV = new[] { 0, 0, -1 } })),
            ["CNLD"] = Rows(("3", Items(new { ID = 1, LCNAME = "Q", GROUP_NAME = "", FX = 0, FY = 5, FZ = -20, MX = 1, MY = 0, MZ = 0 }))),
            ["BMLD"] = Rows(("3", Items(BeamLoad(1, "UNILOAD", "GZ", new double[] { 0, 1, 0, 0 }, new double[] { -10, -10, 0, 0 }), BeamLoad(2, "CONLOAD", "LZ", new[] { .25, 0, 0, 0 }, new double[] { -8, 0, 0, 0 }),
                BeamLoad(3, "UNILOAD", "GZ", new[] { .2, .5, .8, 0 }, new double[] { -1, -3, -3, 0 }))),
                ("1", Items(BeamLoad(4, "UNIMOMENT", "LX", new double[] { 0, 1, 0, 0 }, new double[] { 2, 2, 0, 0 })))),
            ["PRES"] = Rows(("10", Items(new { ID = 1, LCNAME = "Q", ELEM_TYPE = "PLATE", FACE_EDGE_TYPE = "FACE", DIRECTION = "LZ", FORCES = new double[] { -2, 0, 0, 0, 0 } },
                    new { ID = 2, LCNAME = "Q", ELEM_TYPE = "PLATE", FACE_EDGE_TYPE = "FACE", DIRECTION = "LZ", FORCES = new double[] { 0, 1, 1, 2, 2 } },
                    new { ID = 3, LCNAME = "Q", ELEM_TYPE = "PLATE", FACE_EDGE_TYPE = "EDGE", DIRECTION = "GZ", OPT_PROJECTION = false, EDGE_LOADS = new double[] { -3, 0, 0 }, EDGE_FACE = 2 })),
                ("11", Items(new { ID = 4, LCNAME = "W", ELEM_TYPE = "PLATE", FACE_EDGE_TYPE = "FACE", DIRECTION = "GX", FORCES = new[] { 1.5, 0, 0, 0, 0 } }))),
            ["LCOM-GEN"] = Rows(("1", new { NAME = "C1", ACTIVE = "ACTIVE", iTYPE = 0, vCOMB = new[] { new { ANAL = "ST", LCNAME = "G", FACTOR = 1.35 }, new { ANAL = "ST", LCNAME = "Q", FACTOR = 1.5 } } }),
                ("2", new { NAME = "ENV", ACTIVE = "ACTIVE", iTYPE = 1, vCOMB = new[] { new { ANAL = "CB", LCNAME = "C1", FACTOR = 1.0 }, new { ANAL = "ST", LCNAME = "W", FACTOR = 1.0 } } })),
            ["LCOM-STEEL"] = Rows(("1", new { NAME = "S1", ACTIVE = "STRENGTH", iTYPE = 0, vCOMB = new[] { new { ANAL = "ST", LCNAME = "G", FACTOR = 1.0 }, new { ANAL = "ST", LCNAME = "W", FACTOR = 1.5 } } })),
        };
        return tables.ToDictionary(t => t.Key, t => JsonSerializer.Serialize(new Dictionary<string, object> { [t.Key] = t.Value }));
    }

    [TestMethod]
    public void CoverageModel_TextAndApiDatabasesGiveTheSameModel()
    {
        var text = Text(Coverage); Partial(text);
        var api = CivilNxGeometryReader.Import(CoverageApi(), new AnalysisSource { Program = "MIDAS Civil NX", SolverVersion = "s", ModelRevision = "rev", AnalysisId = "a" }, new CivilNxModelProfile());
        Assert.AreEqual(ImportStatus.Partial, api.Status, string.Join("; ", api.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.Code + ": " + d.Message)));
        ModelSignature.AssertEqual(api.Model, text.Model);
        Assert.AreEqual(8, text.Model.NodesElements.Count); Assert.AreEqual(5, text.Model.BeamElements.Count); Assert.AreEqual(2, text.Model.AreaElements.Count);
    }

    [TestMethod]
    public void CoverageModel_PropertiesAxesBoundariesAndLoadsHaveTheMidasValues()
    {
        var report = Text(Coverage); Partial(report); var m = report.Model;
        Assert.AreEqual("synthetic-coverage", m.AnalysisSource.SolverVersion);
        var column = Find<BeamElement>(m, EntityFamily.Beam, "1"); var rotated = Find<BeamElement>(m, EntityFamily.Beam, "2");
        var rc = (ReinforcedConcreteSection)column.BeamProperty; Assert.AreEqual(600 * 400, rc.Area, 1e-6);
        Vector(column.Assignments.SectionAxes.V2, 1, 0, 0); // Vertical member: MIDAS z (GPC V2) is GCS X at beta 0.
        Vector(rotated.Assignments.SectionAxes.V2, 0, 1, 0);  // Beta 90 about the member axis +Z.
        var girder = Find<BeamElement>(m, EntityFamily.Beam, "3"); var steel = (SteelSection)girder.BeamProperty;
        Assert.IsInstanceOfType(steel.SectionShape, typeof(SectionH)); Assert.AreEqual(355, steel.SteelMaterial.Fyk);
        Assert.AreEqual(-200, girder.Assignments.SectionCentroidOffset.Y, 1e-6, "CT: reference line at the top of the 400 mm section.");
        Vector(girder.Assignments.OffsetI, 100, 0, 0); Vector(girder.Assignments.OffsetJ, -100, 0, 0);
        var release = girder.Attributes.Values.OfType<BeamReleasesAttribute>().Single();
        Assert.AreEqual(BeamConnectionKind.Released, release.I[4].Kind); Assert.AreEqual(BeamConnectionKind.Released, release.I[5].Kind);
        Assert.AreEqual(BeamConnectionKind.Continuous, release.I[3].Kind); Assert.AreEqual(BeamConnectionKind.RelativeFixity, release.J[5].Kind); Assert.AreEqual(.25, release.J[5].Value);
        var truss = Find<BeamElement>(m, EntityFamily.Beam, "4"); var tie = Find<BeamElement>(m, EntityFamily.Beam, "5");
        Assert.AreEqual(BeamFormulation.Truss, truss.Assignments.Formulation); Assert.IsInstanceOfType(((SteelSection)truss.BeamProperty).SectionShape, typeof(SectionCHS));
        Assert.AreEqual(BeamFormulation.TensionOnly, tie.Assignments.Formulation); Assert.IsNull(tie.BeamProperty, "VALUE section: numeric properties only.");
        Assert.AreEqual(2, report.Diagnostics.Count(d => d.Code == "AxialOnlyBeamFormulation" && d.Severity == DiagnosticSeverity.Warning));
        var slab = Find<AreaElement>(m, EntityFamily.Shell, "10"); var cantilever = Find<AreaElement>(m, EntityFamily.Shell, "11");
        Assert.AreEqual(250, ((ConcretePlateProperty)slab.PlateProperty).MembraneThickness); Assert.AreEqual(250, slab.Assignments.PhysicalThickness);
        Vector(slab.CoordinateSystem.V1, 1, 0, 0); Vector(slab.CoordinateSystem.V3, 0, 0, 1);
        Assert.AreEqual(100, cantilever.Assignments.Offset!.Value, 1e-9, "Offset ratio 0.5 of 200 mm.");
        Vector(cantilever.CoordinateSystem.V1, Math.Cos(Math.PI / 6), Math.Sin(Math.PI / 6), 0); // Triangle x along N1-N2 (+X), rotated by ANGLE 30.
        Assert.AreEqual("C35/45 user", ((ConcretePlateProperty)cantilever.PlateProperty).ConcreteMaterial.Name);
        Assert.AreEqual(2.5e-5 / 9806.65, ((ConcretePlateProperty)cantilever.PlateProperty).ConcreteMaterial.Density, 1e-18, "User weight density 25 kN/m3.");
        var skew = Find<NodeElement>(m, EntityFamily.Node, "6").Assignments.Restrains.Single().Restrain;
        Assert.AreEqual(3, skew.Restrains.Count); Vector(skew.CoordinateSystem.V1, Math.Sqrt(.5), Math.Sqrt(.5), 0);
        Assert.AreEqual(6, Find<NodeElement>(m, EntityFamily.Node, "10").Assignments.Restrains.Single().Restrain.Restrains.Count);
        var spring = Find<NodeElement>(m, EntityFamily.Node, "4").Assignments.Links.Single(l => l.Spring != null).Spring;
        Assert.AreEqual(100, spring[0, 0]); Assert.AreEqual(6e8, spring[5, 5]);
        Assert.AreEqual(1, Find<NodeElement>(m, EntityFamily.Node, "5").Assignments.Links.Count(l => l.KinematicCoefficients != null));
        Assert.AreEqual(1, Find<NodeElement>(m, EntityFamily.Node, "2").Assignments.Links.Count(l => l.KinematicCoefficients != null));
        var gravity = m.ModelLoads.Values.OfType<ModelGravityLoad>().Single();
        Assert.AreEqual("G", gravity.LoadCase.Name); Assert.AreEqual(-1, gravity.GravityVersor.Z, "Self weight of a load group: the load group only activates it in a stage.");
        var loads = girder.Assignments.Loads;
        Assert.AreEqual(4, loads.Count); Assert.AreEqual(-60000, loads.Take(1).Sum(l => GPC.Model.Results.Processing.Equilibrium.BeamLoad(girder, l).Force.Z), 1e-6);
        Assert.AreEqual(2e6 * 3, column.Assignments.Loads.Sum(l => GPC.Model.Results.Processing.Equilibrium.BeamLoad(column, l).Moment.Z), 1e-3, "Distributed torsion 2 kNm/m on 3 m.");
        Assert.AreEqual(-0.002, slab.Loads.Values.OfType<NormalAreaLoad>().Single().Pressure, 1e-15);
        CollectionAssert.AreEqual(new[] { 0.001, 0.001, 0.002, 0.002 }, slab.Loads.Values.OfType<NonUniformPlatePressure>().Single().Pressures.ToArray());
        Assert.AreEqual(-3, slab.Loads.Values.OfType<LineLoad>().Single().F3);
        Assert.AreEqual(0.0015, cantilever.Loads.Values.OfType<AreaLoad>().Single().P1, 1e-15);
        CollectionAssert.IsSubsetOf(new[] { "C1", "S1", "ENV" }, m.Combinations.Keys.ToArray());
        var definitions = GPC.Converter.CombinationExpansion.Definitions(m);
        Assert.AreEqual(1.35, definitions.Single(d => d.Name == "C1").Terms.Single(t => t.Name == "G").Factor);
        Assert.AreEqual(CombinationKind.Envelope, definitions.Single(d => d.Name == "ENV").Kind);
        Assert.AreEqual("LCOM-STEEL", definitions.Single(d => d.Name == "S1").Source);
        Assert.AreEqual(7, m.GetGroupElements("Deck").Count, "Nodes 2-5, girder 3, plates 10 and 11.");
        Assert.IsTrue(m.BeamElements.Values.All(b => b.Assignments.OtherAssignments.Single().Kind == "MIDAS material/section/orientation references"));
        Assert.IsTrue(report.Preserved.Any(p => p.Kind == "MIDAS Civil *SECTION" && p.RawData.Contains("Tirante")));
    }

    internal const string SpringsText = "*UNIT\nKN, M, KJ, C\n*NODE\n1, 0, 0, 0\n2, 4, 0, 0\n3, 8, 0, 0\n*ELEMENT\n1, BEAM, 1, 1, 1, 2, 0, 0\n2, BEAM, 1, 1, 2, 3, 0, 0\n"
        + "*LOCALAXIS\n3, 3, 0, 1, 0, -1, 0, 0\n*CONSTRAINT\n1, 110000,\n"
        + "*SPRING    ; Point Spring Supports\n   1 , LINEAR, NO, NO, YES, NO, NO, NO, 1000, 2000, 0, 0, 0, 300, NO, 0, 0, 0, 0, 0, 0, Springs, 0, 0, 0, 0, 0\n"
        + "   3 , LINEAR, NO, NO, NO, NO, NO, NO, 10, 0, 0, 0, 0, 0, NO, 0, 0, 0, 0, 0, 0, , 0, 0, 0, 0, 0\n"
        + "   2 , COMP, DZ, 0, 0, 0, 1000, , 0, 0, 0, 0\n*ENDDATA\n";

    [TestMethod]
    public void PointSprings_FixedDofsAreSupportsAndStiffnessesGroundSpringsInTheNodeAxes()
    {
        var report = Text(SpringsText); Partial(report); var m = report.Model;
        var first = Find<NodeElement>(m, EntityFamily.Node, "1");
        var restraint = first.Assignments.Restrains.Single().Restrain;
        CollectionAssert.AreEquivalent(new[] { "DX", "DY", "DZ" }, restraint.Restrains.Select(r => r.Dof.ToString()).ToArray(), "CONSTRAINT and the fixed spring DOF.");
        var k = first.Assignments.GroundSprings.Single();
        Assert.AreEqual(1000, k[0, 0]); Assert.AreEqual(2000, k[1, 1]); Assert.AreEqual(0, k[2, 2]); Assert.AreEqual(3e8, k[5, 5]);
        var local = Find<NodeElement>(m, EntityFamily.Node, "3").Assignments.GroundSprings.Single();
        Assert.AreEqual(10, local[0, 0]); Vector(local.CoordinateSystem.V1, 0, 1, 0); Assert.AreEqual(8000, local.CoordinateSystem.Origin.X);
        Assert.AreEqual(0, Find<NodeElement>(m, EntityFamily.Node, "2").Assignments.GroundSprings.Count);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilNxSpringLawPreserved"));
        Assert.IsFalse(m.ValidateAssignments().Any(d => d.Severity == DiagnosticSeverity.Error));
    }

    internal const string FamiliesText = "*UNIT\nKN, M, KJ, C\n*NODE\n1, 0, 0, 0\n2, 4, 0, 0\n3, 4, 0, 3\n4, 0, 0, 3\n5, 0, 3, 0\n6, 4, 3, 0\n7, 4, 3, 3\n8, 0, 3, 3\n"
        + "*MATERIAL\n1, STEEL, S355, 0, 0, , C, NO, 0.02, 1, EN05(S), , S355, NO, 2.1e+08\n"
        + "*SECTION\n1, DBUSER, Tubo, CC, 0, 0, 0, 0, 0, 0, YES, NO, P, 2, 0.1, 0.005, 0, 0, 0, 0, 0, 0, 0, 0\n"
        + "*THICKNESS\n1, VALUE, Parete, YES, 0.3, 0, NO, 0, 0\n"
        + "*ELEMENT\n"
        + "1, COMPTR, 1, 1, 1, 2, 0, 1, 0, 0, NO\n2, COMPTR, 1, 1, 2, 3, 0, 2, 0.01, 0, NO\n3, TENSTR, 1, 1, 3, 4, 0, 3, 50, 0, NO\n4, TENSTR, 1, 1, 4, 1, 0, 2, 0, 0, NO\n"
        + "5, WALL, 1, 1, 1, 2, 3, 4, 2, 7, 0, 0\n6, PLSTRS, 1, 1, 5, 6, 7, 8, 1, 0\n7, SOLID, 1, 1, 1, 2, 6, 5, 4, 3, 7, 8\n"
        + "*GROUP\nAll, , 1to7, 0\n*STLDCASE\nQ, L, \n*USE-STLD, Q\n*PRESSURE\n5, PRES, PLATE, FACE, LZ, 0, 0, 0, NO, -2, 0, 0, 0, 0, ,0\n*ENDDATA\n";

    [TestMethod]
    public void ElementFamilies_AxialMembersMapAndUnsupportedFamiliesArePreservedWithoutRejectingTheFile()
    {
        var report = Text(FamiliesText); Partial(report); var m = report.Model;
        Assert.AreEqual(BeamFormulation.Truss, Find<BeamElement>(m, EntityFamily.Beam, "1").Assignments.Formulation, "Compression-only truss as a truss.");
        Assert.AreEqual(BeamFormulation.Cable, Find<BeamElement>(m, EntityFamily.Beam, "3").Assignments.Formulation);
        Assert.IsInstanceOfType(Find<BeamElement>(m, EntityFamily.Beam, "3").BeamProperty, typeof(SteelSection));
        Assert.AreEqual(2, m.BeamElements.Count); Assert.AreEqual(0, m.AreaElements.Count); Assert.AreEqual(2, m.GetGroupElements("All").Count);
        var codes = report.Diagnostics.Select(d => d.Code).ToHashSet();
        foreach (var code in new[] { "CivilCompressionOnlyAsTruss", "CivilElementFamilyPreserved", "GenWallPreserved", "CivilElementAssignmentPreserved", "CivilAxialLimitPreserved" })
            Assert.IsTrue(codes.Contains(code), code);
        Assert.AreEqual(5, report.Diagnostics.Count(d => d.Code == "CivilElementFamilyPreserved"), "COMPTR gap, TENSTR hook, wall, plane stress, solid.");
        Assert.IsTrue(report.Preserved.Any(p => p.Kind == "MIDAS Civil *ELEMENT" && p.RawData.Contains("SOLID")));
    }

    [TestMethod]
    public void MultiLineRecords_StagesCombinationsAndCompositeSectionsAreReadRecordByRecord()
    {
        var text = "*UNIT\nKN, M, KJ, C\n*NODE\n1, 0, 0, 0\n2, 10, 0, 0\n*MATERIAL\n1, STEEL, S355, 0, 0, , C, NO, 0.02, 1, EN05(S), , S355, NO, 2.1e+08\n"
            + "*SECTION\n 1, COMPOSITE , Trave mista, CT, 0, 1, 0, 0, 1, 0, YES, NO, NO, I\n       1.135, 0.012, 0.5, 0.025, 0.8, 0.04\n       0, 0, 0, 0, 0, 0, 0\n       0\n       0\n"
            + "       2.5, 0, 0, 2.5, 0.25, 0, 6.5, 2.5, 7850, 2500, 1, NO, 0, 0\n"
            + "*ELEMENT\n1, BEAM, 1, 1, 1, 2, 0, 0\n*GROUP\nTravi, 1to2, 1, 0\n*CONSTRAINT\n1, 111111, Appoggi\n"
            + "*STLDCASE\nG1, D, \nG2, D, \n*USE-STLD, G1\n*SELFWEIGHT\n0, 0, -1, G1 load\n"
            + "*STAGE    ; Define Construction Stage\n  NAME=Fase 1, 7, YES, NO, NO, 5\n  AELEM=Travi, 0\n  ABNDR=Appoggi, DEFORMED\n  ALOAD=G1 load, FIRST\n"
            + "  NAME=Fase 2, 28, YES, NO, NO, 5\n  STEP=7, 14\n  ALOAD=G2 load, 10\n"
            + "*LOADCOMB\n   NAME=SLU, GEN, ACTIVE, 0, 0, , 0, 0, 0\n        ST, G1, 1.35, ST, G2, 1.35\n        CS, Summation, 1\n*ENDDATA\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        var batch = new MidasCivilTextReader("stages").Read(stream, default);
        var composite = batch.Sections.Single();
        Assert.AreEqual(2, composite.GeometryComponents.Length); Assert.AreEqual(250, composite.GeometryComponents[1].Dimensions[0], 1e-9);
        var report = Text(text); Partial(report); var m = report.Model;
        Assert.AreEqual(2, m.Stages.Count);
        var first = m.Stages.Values.Single(s => s.Name == "Fase 1").Definition;
        Assert.AreEqual(7, first.DurationDays); Assert.AreEqual(3, first.GroupChanges.Count);
        CollectionAssert.AreEqual(new[] { 7d, 14 }, m.Stages.Values.Single(s => s.Name == "Fase 2").Definition.AdditionalOutputDays.ToArray());
        Assert.AreEqual(0, m.NodesElements.Values.Sum(n => n.Assignments.Restrains.Count), "Staged supports are not permanent.");
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilNxStagedBoundaryPreserved"));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilNxSelfWeightLoadGroup"));
        var slu = GPC.Converter.CombinationExpansion.Definitions(m).Single();
        Assert.AreEqual(3, slu.Terms.Count); Assert.IsFalse(slu.Terms.Single(t => t.Analysis == "CS").IsStaticCase);
    }

    [TestMethod]
    public void GenMgt_StoriesDiaphragmsAndWallsArePreservedAndTheRestIsMapped()
    {
        var text = ";---------------------------------------------------------------------------\n;  midas Gen Text(MGT) File.\n;  Date : 2026/10/28\n"
            + ";---------------------------------------------------------------------------\n\n*VERSION\n   9.5.5\n\n*UNIT    ; Unit System\n   KN   , M, KJ, C\n"
            + "*NODE\n1, 0, 0, 0\n2, 5, 0, 0\n3, 5, 0, 3\n4, 0, 0, 3\n5, 5, 4, 3\n6, 0, 4, 3\n"
            + "*MATERIAL\n1, CONC, C25/30, 0, 0, , C, NO, 0.05, 1, NTC18(RC), , C25/30, NO, 3.1476e+07\n"
            + "*SECTION\n1, DBUSER, Trave, CC, 0, 0, 0, 0, 0, 0, YES, NO, SB, 2, 0.5, 0.3, 0, 0, 0, 0, 0, 0, 0, 0\n"
            + "*THICKNESS\n1, VALUE, Solaio, YES, 0.2, 0, NO, 0, 0\n"
            + "*ELEMENT\n1, BEAM, 1, 1, 4, 3, 0, 0\n2, WALL, 1, 1, 1, 2, 3, 4, 1, 1, 0, 0\n3, PLATE, 1, 1, 4, 3, 5, 6, 3, -90\n"
            + "*STORY    ; Story\n;  NAME=NAME, LEVEL, bFLDIAP, WINDWX, WINDWY, WINDCX, WINDCY, ECCX, ECCY, IECCX, IECCY, TAFX, TAFY, WTAFX, WTAFY, AreaPosNum ; line 1\n"
            + "   NAME=1F, 3, YES , 5, 4, 2.5, 2, 0.25, 0.2, 0, 0, 1, 1, 0.75, 0.6, 0\n   NAME=B1, 0, NO , 5, 4, 2.5, 2, 0.25, 0.2, 0, 0, 1, 1, 0.75, 0.6, 0\n"
            + "*WALLMARK    ; Modify Wall Mark Data\n   W1, 1\n*CONSTRAINT\n1 2, 111111,\n*ENDDATA\n";
        var bytes = Encoding.UTF8.GetBytes(text);
        Assert.AreEqual(MidasTextFiles.GenProgram, MidasTextFiles.DetectProgram("model.txt", bytes));
        var reader = MidasTextFiles.CreateReader("model.mct", bytes, "gen"); Assert.AreEqual("MIDAS Gen", reader.Program, "The header decides over the extension.");
        var report = Text(text, MidasTextFiles.GenProgram, "gen"); Partial(report); var m = report.Model;
        Assert.AreEqual("MIDAS Gen", m.AnalysisSource.Program);
        Assert.IsInstanceOfType(Find<BeamElement>(m, EntityFamily.Beam, "1", "MIDAS Gen").BeamProperty, typeof(ReinforcedConcreteSection));
        var plate = Find<AreaElement>(m, EntityFamily.Shell, "3", "MIDAS Gen");
        Assert.AreEqual(200, plate.Assignments.PhysicalThickness); Vector(plate.CoordinateSystem.V1, 0, -1, 0); // Plate x (+X) rotated by ANGLE -90 about +Z.
        Assert.AreEqual(1, m.AreaElements.Count, "The wall is preserved, not mapped.");
        Assert.AreEqual(1, report.Diagnostics.Count(d => d.Code == "GenFloorDiaphragmPreserved"));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "GenWallPreserved"));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilCommandPreserved" && d.Record.StartsWith("*WALLMARK")));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilInputWithoutResults" && d.Record == "MGT"));
        Assert.AreEqual(2, m.NodesElements.Values.Count(n => n.Assignments.Restrains.Count == 1));
    }

    [DataTestMethod]
    [DataRow("a.mct", ";  MIDAS/Civil Text(MCT) File.", "MIDAS Civil")]
    [DataRow("a.mgt", ";  MIDAS CIVIL NX Text(MCT) File.", "MIDAS Civil")]
    [DataRow("a.mct", ";  midas Gen Text(MGT) File.", "MIDAS Gen")]
    [DataRow("a.MGT", "; Rhino2Midas export", "MIDAS Gen")]
    [DataRow("a.mct", "", "MIDAS Civil")]
    [DataRow("a.txt", "; no header", null)]
    public void Program_IsDetectedFromTheExportHeaderThenTheExtension(string name, string header, string? program)
    {
        Assert.AreEqual(program, MidasTextFiles.DetectProgram(name, Encoding.ASCII.GetBytes(";-----\n" + header + "\n*VERSION\n")));
        if (program == null) Assert.ThrowsException<NotSupportedException>(() => MidasTextFiles.CreateReader(name, null, "r"));
    }

    [DataTestMethod]
    [DataRow("5,     1,     0\n", "5,     7,     0\n")]
    [DataRow("000001, 0, 0, 0, 0, 0, 0.25, Rel\n", "")]
    [DataRow("   ST, G, 1.35, ST, Q, 1.5", "   ST, G, 1.35, ST, Q")]
    [DataRow("NAME=S1, STEEL", "NAME=S1, FOO")]
    [DataRow("2, VALUE, Sbalzo, YES, 0.2, 0,  YES, 0, 0.5", "2, VALUE, Sbalzo, YES, 0.2, 0,  YES, 4, 0.5")]
    [DataRow("   6, 3, 1, 1, 0, -1, 1, 0", "   6, 4, 1, 1, 0, -1, 1, 0")]
    [DataRow("       0.000706858, 0, 0, 7.95e-08, 3.976e-08, 3.976e-08\n", "")]
    public void InvalidNewRecords_RejectTheFileAtomically(string oldValue, string newValue)
    {
        var fixture = Coverage.Replace("\r\n", "\n"); var line = oldValue.Replace("\r\n", "\n");
        var index = fixture.IndexOf(line.TrimStart(), StringComparison.Ordinal);
        Assert.IsTrue(index >= 0, "The mutation must match the fixture.");
        var report = Text(fixture.Remove(index, line.TrimStart().Length).Insert(index, newValue.TrimStart()));
        Assert.AreEqual(ImportStatus.Rejected, report.Status); Assert.IsNull(report.Model);
    }

    /// <summary>Opt-in: every .mct/.mgt below GPC_MIDAS_TEXT_SAMPLES (real exports, ANSI) is read; counts and rejections are printed, and the
    /// translated tables of every imported file give the same model through the API reader.</summary>
    [TestMethod, TestCategory("MidasNative")]
    public void RealExports_AreReadAndTheirTablesGiveTheSameModel()
    {
        var folder = Environment.GetEnvironmentVariable("GPC_MIDAS_TEXT_SAMPLES");
        if (string.IsNullOrWhiteSpace(folder)) Assert.Inconclusive("Set GPC_MIDAS_TEXT_SAMPLES to a folder of MIDAS .mct/.mgt exports.");
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var failures = new List<string>();
        foreach (var path in Directory.EnumerateFiles(folder!, "*.*", SearchOption.AllDirectories).Where(p => p.EndsWith(".mct", StringComparison.OrdinalIgnoreCase) || p.EndsWith(".mgt", StringComparison.OrdinalIgnoreCase)))
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var report = MidasTextFiles.Import(path, "real", textEncoding: Encoding.GetEncoding(1252));
            var m = report.Model;
            var warnings = report.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Warning).GroupBy(d => d.Code).OrderByDescending(g => g.Count()).Take(6).Select(g => g.Key + "×" + g.Count());
            Console.WriteLine($"{Path.GetFileName(path)}: {report.Status} {watch.Elapsed.TotalSeconds:F1}s" + (m == null ? " " + string.Join("; ", report.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.Record + " " + d.Message).Take(3))
                : $" nodes {m.NodesElements.Count}, beams {m.BeamElements.Count} ({m.BeamElements.Values.Count(b => b.BeamProperty is not null)} with property), shells {m.AreaElements.Count} ({m.AreaElements.Values.Count(s => s.PlateProperty is not null)}),"
                + $" restraints {m.NodesElements.Values.Sum(n => n.Assignments.Restrains.Count)}, springs {m.NodesElements.Values.Sum(n => n.Assignments.GroundSprings.Count)}, links {m.NodesElements.Values.Sum(n => n.Assignments.Links.Count)},"
                + $" releases {m.BeamElements.Values.Count(b => b.Attributes.Values.OfType<BeamReleasesAttribute>().Any())}, cases {m.LoadCases.Count}, combinations {m.Combinations.Count}, stages {m.Stages.Count},"
                + $" loads {m.NodesElements.Values.Sum(n => n.Loads.Count) + m.BeamElements.Values.Sum(b => b.Assignments.Loads.Count) + m.AreaElements.Values.Sum(s => s.Loads.Count) + m.ModelLoads.Count} | {string.Join(", ", warnings)}"));
            if (report.Status == ImportStatus.Rejected) continue;
            // The translated tables, read back by the API reader, give the same model.
            var tables = MidasTextFiles.ReadTables(path, "real", textEncoding: Encoding.GetEncoding(1252));
            var api = CivilNxGeometryReader.Import(tables.ToSnapshot(), new AnalysisSource { Program = "MIDAS Civil NX", SolverVersion = "s", ModelRevision = "real", AnalysisId = "a" },
                new CivilNxModelProfile());
            Console.WriteLine($"  tables {tables.Tables.Count} ({tables.Tables.Values.Sum(t => t.Length) / 1024} KiB), API reader {api.Status}");
            if (api.Model == null || api.Model.NodesElements.Count != m!.NodesElements.Count || api.Model.BeamElements.Count != m.BeamElements.Count
                || api.Model.AreaElements.Count != m.AreaElements.Count || api.Model.Combinations.Count != m.Combinations.Count) failures.Add(Path.GetFileName(path) + " (tables)");
        }
        Assert.AreEqual(0, failures.Count, "Tables that do not give the model of the file: " + string.Join(", ", failures));
    }
}
