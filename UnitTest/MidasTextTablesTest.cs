using System.Globalization;
using System.Text;
using System.Text.Json;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Converter.MidasCivil;
using GPC.Model.Analysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>The Civil NX tables translated from MIDAS text files (<see cref="MidasTextFiles.ReadTables(Stream, string, string, string, Encoding, CancellationToken)"/>):
/// JSON in the form of the API responses, in N and mm, equal to the native API databases of the same model and importable by the API reader.</summary>
[TestClass]
public class MidasTextTablesTest
{
    private static MidasTextTables Read(string text, string fileName = "model.mct")
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        return MidasTextFiles.ReadTables(stream, fileName, "rev");
    }
    private static string Fixture(string folder) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "MidasCivil", folder);

    [TestMethod]
    public void Tables_HaveTheApiFormInNewtonsAndMillimetres()
    {
        var tables = Read(MidasTextCoverageTest.Coverage);
        Assert.IsNotNull(tables.Batch); Assert.AreEqual("MIDAS Civil", tables.Program); Assert.AreEqual(64, tables.SourceHash.Length);
        using var unit = JsonDocument.Parse(tables.Tables["UNIT"]);
        var u = unit.RootElement.GetProperty("UNIT").GetProperty("1");
        Assert.AreEqual("N", u.GetProperty("FORCE").GetString()); Assert.AreEqual("MM", u.GetProperty("DIST").GetString()); Assert.AreEqual("C", u.GetProperty("TEMPER").GetString());
        using var node = JsonDocument.Parse(tables.Tables["NODE"]);
        Assert.AreEqual(6000, node.RootElement.GetProperty("NODE").GetProperty("3").GetProperty("X").GetDouble(), "6 m in mm.");
        using var elem = JsonDocument.Parse(tables.Tables["ELEM"]);
        var plate = elem.RootElement.GetProperty("ELEM").GetProperty("11");
        CollectionAssert.AreEqual(new[] { 2, 3, 4, 0, 0, 0, 0, 0 }, plate.GetProperty("NODE").EnumerateArray().Select(n => n.GetInt32()).ToArray());
        Assert.AreEqual(30, plate.GetProperty("ANGLE").GetDouble()); Assert.IsFalse(plate.TryGetProperty("unused", out _));
        using var loads = JsonDocument.Parse(tables.Tables["BMLD"]);
        var uniform = loads.RootElement.GetProperty("BMLD").GetProperty("3").GetProperty("ITEMS")[0];
        Assert.AreEqual(1, uniform.GetProperty("ID").GetInt32()); Assert.AreEqual("Q", uniform.GetProperty("LCNAME").GetString());
        Assert.AreEqual(-10, uniform.GetProperty("P")[0].GetDouble(), 1e-12, "-10 kN/m = -10 N/mm."); Assert.AreEqual(1, uniform.GetProperty("D")[1].GetDouble());
        Assert.IsFalse(uniform.TryGetProperty("USE_ECCEN", out var eccentric) && eccentric.ValueKind == JsonValueKind.Null, "Null members are omitted.");
        using var combinations = JsonDocument.Parse(tables.Tables["LCOM-GEN"]);
        var env = combinations.RootElement.GetProperty("LCOM-GEN").GetProperty("2");
        Assert.AreEqual("ENV", env.GetProperty("NAME").GetString()); Assert.AreEqual(1, env.GetProperty("iTYPE").GetInt32());
        Assert.AreEqual("CB", env.GetProperty("vCOMB")[0].GetProperty("ANAL").GetString()); Assert.AreEqual("C1", env.GetProperty("vCOMB")[0].GetProperty("LCNAME").GetString());
        using var marker = JsonDocument.Parse(tables.Tables[CivilNxTablesName]);
        Assert.IsTrue(marker.RootElement.GetProperty(CivilNxTablesName).GetProperty("1").GetProperty("LENIENT").GetBoolean());
        var rejected = Read(MidasTextCoverageTest.Coverage.Replace("*ENDDATA", ""));
        Assert.IsNull(rejected.Batch); Assert.AreEqual(0, rejected.Tables.Count); Assert.AreEqual("SourceReadFailed", rejected.Diagnostics.Single().Code);
    }
    private const string CivilNxTablesName = "GPC-TEXT";

    /// <summary>Leaves of a JSON value by path ("1.SECT_BEFORE.SECT_I.vSIZE[0]").</summary>
    private static Dictionary<string, JsonElement> Leaves(JsonElement value, string path = "")
    {
        var result = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        if (value.ValueKind == JsonValueKind.Object)
            foreach (var p in value.EnumerateObject()) foreach (var leaf in Leaves(p.Value, path.Length == 0 ? p.Name : path + "." + p.Name)) result.Add(leaf.Key, leaf.Value);
        else if (value.ValueKind == JsonValueKind.Array)
        { int i = 0; foreach (var item in value.EnumerateArray()) foreach (var leaf in Leaves(item, path + "[" + (i++).ToString(CultureInfo.InvariantCulture) + "]")) result.Add(leaf.Key, leaf.Value); }
        else result.Add(path, value);
        return result;
    }

    /// <summary>Fields of the API databases that the MCT export does not carry.</summary>
    private static readonly string[] NotInMct = { ".PLMT", ".P_NAME" };

    [DataTestMethod]
    [DataRow("identical-pairs-native-9.5.5", "packed-pairs-full.mct")]
    [DataRow("catalog-pairs-native-9.5.5", "catalog-pairs-full.mct")]
    public void NativeMctTables_AreTheApiDatabasesOfTheSameModel(string folder, string file)
    {
        var tables = MidasTextFiles.ReadTables(Path.Combine(Fixture(folder), file), "native");
        Assert.IsNotNull(tables.Batch, string.Join("; ", tables.Diagnostics.Select(d => d.Message)));
        using var apiUnit = JsonDocument.Parse(File.ReadAllText(Path.Combine(Fixture(folder), "UNIT.json")));
        var unit = apiUnit.RootElement.GetProperty("UNIT").GetProperty("1");
        double length = MidasLength(unit.GetProperty("DIST").GetString()!), force = MidasForce(unit.GetProperty("FORCE").GetString()!);
        double Scale(string table, string path) => table == "NODE" || path.Contains(".vSIZE[") || path.EndsWith("USERDEF_OFFSET_YI") || path.EndsWith("USERDEF_OFFSET_ZI") ? length
            : table == "MATL" && path.EndsWith(".ELAST") ? force / (length * length) : 1;
        int compared = 0;
        foreach (var table in new[] { "NODE", "ELEM", "SECT", "MATL", "CONS", "STLD", "LCOM-GEN", "LCOM-CONC", "LCOM-STEEL", "LCOM-SRC", "LCOM-STLCOMP", "LCOM-SEISMIC" })
        {
            using var api = JsonDocument.Parse(File.ReadAllText(Path.Combine(Fixture(folder), table + ".json")));
            if (!api.RootElement.TryGetProperty(table, out var apiRows)) { Assert.IsFalse(tables.Tables.ContainsKey(table), table + ": empty in the API, absent in the MCT"); continue; }
            using var text = JsonDocument.Parse(tables.Tables[table]);
            var textRows = text.RootElement.GetProperty(table);
            CollectionAssert.AreEquivalent(apiRows.EnumerateObject().Select(p => p.Name).ToArray(), textRows.EnumerateObject().Select(p => p.Name).ToArray(), table + " keys");
            var expected = Leaves(apiRows); var actual = Leaves(textRows);
            foreach (var leaf in expected)
            {
                if (NotInMct.Any(leaf.Key.EndsWith)) continue;
                if (table == "CONS" && leaf.Key.EndsWith(".CONSTRAINT"))
                {   // Civil NX writes six flags in the MCT and the warping flag (7th) in the API.
                    var flags = leaf.Value.GetString()!;
                    Assert.AreEqual(flags.Substring(0, 6), actual[leaf.Key].GetString()!.Substring(0, 6), table + " " + leaf.Key);
                    Assert.IsTrue(flags.Length == 6 || flags[6] == '0', "No warping restraint in the fixture."); compared++; continue;
                }
                Assert.IsTrue(actual.TryGetValue(leaf.Key, out var value), table + ": " + leaf.Key + " missing in the MCT tables");
                if (leaf.Value.ValueKind == JsonValueKind.Number)
                {
                    double e = leaf.Value.GetDouble() * Scale(table, leaf.Key);
                    Assert.AreEqual(e, value.GetDouble(), 1e-12 * Math.Max(1, Math.Abs(e)), table + " " + leaf.Key);
                }
                else Assert.AreEqual(leaf.Value.ToString(), value.ToString(), table + " " + leaf.Key);
                compared++;
            }
        }
        Assert.IsTrue(compared > 400, compared + " values compared."); Console.WriteLine(compared + " values compared.");
    }
    private static double MidasLength(string unit) => unit.ToUpperInvariant() switch { "MM" => 1, "CM" => 10, "M" => 1000, "IN" => 25.4, "FT" => 304.8, _ => throw new NotSupportedException(unit) };
    private static double MidasForce(string unit) => unit.ToUpperInvariant() switch { "N" => 1, "KN" => 1000, "KGF" => 9.80665, "TONF" => 9806.65, _ => throw new NotSupportedException(unit) };

    [TestMethod]
    public void CoverageTables_AreTheHandWrittenApiDatabases()
    {
        var tables = Read(MidasTextCoverageTest.Coverage);
        var api = MidasTextCoverageTest.CoverageApiJson();
        foreach (var table in new[] { "LCOM-GEN", "LCOM-STEEL", "STLD", "CONS", "THIK", "ELNK", "RIGD", "FRLS", "OFFS" })
        {
            using var expected = JsonDocument.Parse(api[table]); using var actual = JsonDocument.Parse(tables.Tables[table]);
            var a = Leaves(actual.RootElement.GetProperty(table));
            foreach (var leaf in Leaves(expected.RootElement.GetProperty(table)))
            {
                Assert.IsTrue(a.TryGetValue(leaf.Key, out var value), table + ": " + leaf.Key);
                double factor = table == "THIK" && (leaf.Key.EndsWith("T_IN") || leaf.Key.EndsWith("T_OUT")) || table == "OFFS" && leaf.Key.Contains("RGD") ? 1000
                    : table == "ELNK" && leaf.Key.Contains("SDR[") ? int.Parse(leaf.Key.Split('[', ']')[1]) < 3 ? 1 : 1e6 : 1;
                if (leaf.Value.ValueKind == JsonValueKind.Number) Assert.AreEqual(leaf.Value.GetDouble() * factor, value.GetDouble(), 1e-9, table + " " + leaf.Key);
                else Assert.AreEqual(leaf.Value.ToString(), value.ToString(), table + " " + leaf.Key);
            }
        }
    }

    [DataTestMethod]
    [DataRow("coverage")][DataRow("springs")][DataRow("families")][DataRow("pairs")][DataRow("catalog")]
    public void TablesThroughTheApiReader_GiveTheModelOfTheTextReader(string sample)
    {
        string text = sample switch
        {
            "coverage" => MidasTextCoverageTest.Coverage,
            "springs" => MidasTextCoverageTest.SpringsText, "families" => MidasTextCoverageTest.FamiliesText,
            "pairs" => File.ReadAllText(Path.Combine(Fixture("identical-pairs-native-9.5.5"), "packed-pairs-full.mct")),
            _ => File.ReadAllText(Path.Combine(Fixture("catalog-pairs-native-9.5.5"), "catalog-pairs-full.mct"))
        };
        var tables = Read(text);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        var direct = new MidasCivilTextReader("rev").Import(stream);
        Assert.AreEqual(ImportStatus.Partial, direct.Status);
        var api = CivilNxGeometryReader.Import(tables.ToSnapshot(), new AnalysisSource { Program = "MIDAS Civil NX", SolverVersion = "s", ModelRevision = "rev", AnalysisId = "a" },
            new CivilNxModelProfile());
        Assert.AreEqual(ImportStatus.Partial, api.Status, string.Join("; ", api.Diagnostics.Where(d => d.Severity == GPC.Model.Core.Diagnostics.DiagnosticSeverity.Error).Select(d => d.Message)));
        ModelSignature.AssertEqual(direct.Model, api.Model);
    }
}
