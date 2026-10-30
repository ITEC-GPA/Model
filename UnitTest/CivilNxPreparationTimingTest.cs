using System.Diagnostics;
using System.Globalization;
using System.Text;
using GPC.Converter;
using GPC.Converter.CivilNx;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>Wall-clock timing, outside the suite, of the cold preparation of a GPC model from a Civil NX model and its static results
/// (geometry import, then result tables), the first step of a combination in ANTHEA's Models module: set GPC_CIVILNX_TIMING=1.</summary>
[TestClass]
public class CivilNxPreparationTimingTest
{
    /// <summary>A chain of <paramref name="beams"/> HE 300 B beams along X (nodes 1..beams+1, 3 m apart), fixed at node 1, with
    /// <paramref name="cases"/> static cases and no loads.</summary>
    private static Dictionary<string, string> Json(int beams, int cases)
    {
        var json = CivilNxModelWorkflowTest.Json();
        var nodes = new StringBuilder("{\"NODE\":{"); var elements = new StringBuilder("{\"ELEM\":{"); var loadCases = new StringBuilder("{\"STLD\":{");
        for (int n = 1; n <= beams + 1; n++) nodes.Append(n == 1 ? "" : ",").Append('"').Append(n).Append("\":{\"X\":").Append(3 * (n - 1)).Append(",\"Y\":0,\"Z\":0}");
        for (int e = 1; e <= beams; e++) elements.Append(e == 1 ? "" : ",").Append('"').Append(e).Append("\":{\"TYPE\":\"BEAM\",\"MATL\":2,\"SECT\":2,\"NODE\":[")
            .Append(e).Append(',').Append(e + 1).Append(",0,0,0,0,0,0],\"ANGLE\":0,\"STYPE\":0}");
        for (int c = 1; c <= cases; c++) loadCases.Append(c == 1 ? "" : ",").Append('"').Append(c).Append("\":{\"NO\":").Append(c).Append(",\"NAME\":\"L").Append(c).Append("\",\"TYPE\":\"D\",\"DESC\":\"\"}");
        json["NODE"] = nodes.Append("}}").ToString(); json["ELEM"] = elements.Append("}}").ToString(); json["STLD"] = loadCases.Append("}}").ToString();
        json["GRUP"] = "{\"message\":\"\"}"; json["CNLD"] = "{\"message\":\"\"}"; json["BMLD"] = "{\"message\":\"\"}"; json["PRES"] = "{\"message\":\"\"}";
        json["BODF"] = "{\"message\":\"\"}"; json["NBOF"] = "{\"message\":\"\"}";
        return json;
    }

    private static string V(double value) => "\"" + value.ToString("R", CultureInfo.InvariantCulture) + "\"";

    /// <summary>Beam forces at I, 1/4, 2/4, 3/4 and J of every beam, displacements of every node and the reactions of node 1, ten cases per
    /// response (as CivilNxResults.ReadAsync requests them).</summary>
    private static CivilNxResponse[] Tables(int beams, int cases)
    {
        var responses = new List<CivilNxResponse>();
        string Table(string head, List<string> rows) => "{\"T\":{\"FORCE\":\"KN\",\"DIST\":\"M\",\"HEAD\":[" + head + "],\"DATA\":[" + string.Join(",", rows) + "]}}";
        for (int at = 1; at <= cases; at += 10)
        {
            var beam = new List<string>(); var displacement = new List<string>(); var reaction = new List<string>();
            for (int c = at; c < at + 10 && c <= cases; c++)
            {
                for (int e = 1; e <= beams; e++)
                    foreach (var part in new[] { "I[" + e + "]", "1/4", "2/4", "3/4", "J[" + (e + 1) + "]" })
                        beam.Add("[\"" + (beam.Count + 1) + "\",\"" + e + "\",\"L" + c + "\",\"" + part + "\"," + V(c) + "," + V(.1 * e) + "," + V(-.2 * c) + "," + V(.01) + "," + V(c + .5 * e) + "," + V(-e) + "]");
                for (int n = 1; n <= beams + 1; n++)
                    displacement.Add("[\"" + (displacement.Count + 1) + "\",\"" + n + "\",\"L" + c + "\"," + V(1e-4 * n) + "," + V(0) + "," + V(-1e-4 * c) + "," + V(0) + "," + V(1e-5) + "," + V(0) + "]");
                reaction.Add("[\"" + (reaction.Count + 1) + "\",\"1\",\"L" + c + "\"," + V(c) + "," + V(0) + "," + V(-c) + "," + V(0) + "," + V(2 * c) + "," + V(0) + "]");
            }
            responses.Add(new CivilNxResponse("post/table", Table("\"Index\",\"Elem\",\"Load\",\"Part\",\"Axial\",\"Shear-y\",\"Shear-z\",\"Torsion\",\"Moment-y\",\"Moment-z\"", beam)));
            responses.Add(new CivilNxResponse("post/table", Table("\"Index\",\"Node\",\"Load\",\"DX\",\"DY\",\"DZ\",\"RX\",\"RY\",\"RZ\"", displacement)));
            responses.Add(new CivilNxResponse("post/table", Table("\"Index\",\"Node\",\"Load\",\"FX\",\"FY\",\"FZ\",\"MX\",\"MY\",\"MZ\"", reaction)));
        }
        return responses.ToArray();
    }

    [TestMethod]
    public void TimingOfTheColdPreparation()
    {
        if (Environment.GetEnvironmentVariable("GPC_CIVILNX_TIMING") != "1") Assert.Inconclusive("Set GPC_CIVILNX_TIMING=1 to time the cold preparation.");
        foreach (var (beams, cases) in new[] { (100, 50), (500, 50), (1000, 50), (1000, 100) })
        {
            var json = Json(beams, cases); var tables = Tables(beams, cases);
            var watch = Stopwatch.StartNew();
            var geometry = CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(json), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile());
            double import = watch.Elapsed.TotalMilliseconds;
            Assert.IsNotNull(geometry.Model, string.Join("; ", geometry.Diagnostics.Select(d => d.Code + ": " + d.Message)));
            watch.Restart();
            var results = CivilNxResults.Import(geometry.Model, CivilNxModelWorkflowTest.Snapshot(json), tables, "static");
            double read = watch.Elapsed.TotalMilliseconds;
            Assert.AreEqual(ImportStatus.Completed, results.Status, string.Join("; ", results.Diagnostics.Select(d => d.Code + ": " + d.Message)));
            Console.WriteLine($"{beams} beams x {cases} cases ({results.ImportedSamples} samples, {tables.Sum(t => t.Json.Length) / 1e6:F1} MB): geometry {import:F0} ms, results {read:F0} ms");
        }
        // One part of the per-sample cost of ResultMapper.Import: the digest of the state and location that guards duplicates (one SHA-256
        // of the canonical bytes per sample), here for 100,000 beam samples.
        var watch2 = Stopwatch.StartNew(); var keys = new HashSet<string>();
        for (int i = 0; i < 100000; i++)
            keys.Add(GPC.Model.Core.ModelValues.Fingerprint(new object[] { GPC.Model.Core.Identity.EntityFamily.Beam, i / 500, "L" + (i % 100), null, null, null, null,
                "Civil NX static case L" + (i % 100), (i % 5) / 4.0, GPC.Model.Results.Locations.SectionSide.Unspecified }));
        Console.WriteLine($"duplicate keys of ResultMapper.Import: {watch2.Elapsed.TotalMilliseconds / 100:F1} us per sample ({keys.Count} keys)");
    }
}
