using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Processing;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class CivilNxIdenticalPairTest
{
    private static Dictionary<string, string> Source(bool channel, bool shortBack, int beta = 0)
    {
        var json = CivilNxModelWorkflowTest.Json();
        double h = channel ? 200 : shortBack ? 100 : 150, b = channel ? 75 : shortBack ? 150 : 100;
        var root = JsonNode.Parse(json["SECT"])!;
        root["SECT"]!["2"] = JsonSerializer.SerializeToNode(new { SECTTYPE = "DBUSER", SECT_NAME = "pair", SECT_BEFORE = new
            { OFFSET_PT = "CC", OFFSET_CENTER = 0, SHAPE = channel ? "2C" : "2L", DATATYPE = 2,
                SECT_I = new { vSIZE = new[] { h / 1000, b / 1000, .01, .012, .01, 0, 0, 0, 0, 0 } } } });
        json["SECT"] = root.ToJsonString();
        var elements = JsonNode.Parse(json["ELEM"])!; elements["ELEM"]!["3"]!["ANGLE"] = beta;
        json["ELEM"] = elements.ToJsonString();
        var r = IdenticalPairReference.Integrate(channel, h, b, 10, 12, 10, 3);
        string N(double v) => v.ToString("R", CultureInfo.InvariantCulture);
        json["SECTIONALL"] = JsonSerializer.Serialize(new { T = new { FORCE = "N", DIST = "mm",
            HEAD = new[] { "ID", "Area", "Iyy", "Izz", "Ixx" },
            DATA = new[] { new[] { "2", N(r.Area), N(r.Ixx), N(r.Iyy), "100000" } } } });
        return json;
    }

    [DataTestMethod]
    [DataRow(false, false, 0)] [DataRow(false, true, 0)] [DataRow(true, false, 0)]
    [DataRow(false, false, 90)] [DataRow(false, true, 90)] [DataRow(true, false, 90)]
    public void PairPreservesFacingLegBetaAndSourceActionAxes(bool channel, bool shortBack, int beta)
    {
        var json = Source(channel, shortBack, beta); var snapshot = CivilNxModelWorkflowTest.Snapshot(json);
        var report = CivilNxGeometryReader.Import(snapshot, CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile());
        Assert.AreNotEqual(ImportStatus.Rejected, report.Status, string.Join(";", report.Diagnostics.Select(d => d.Message)));
        var beam = report.Model.BeamElements.Values.Single(b => b.Source.OriginalId == "3");
        var pair = (SectionBuiltUp)((SteelSection)beam.BeamProperty).SectionShape;
        double h = channel ? 200 : shortBack ? 100 : 150, b = channel ? 75 : shortBack ? 150 : 100;
        var expected = IdenticalPairReference.Integrate(channel, h, b, 10, 12, 10, 3);
        Assert.AreEqual(expected.Area, pair.Area, 1e-8); Assert.AreEqual(expected.Ixx, pair.Jxx, 1e-6); Assert.AreEqual(expected.Iyy, pair.Jyy, 1e-6);
        Assert.AreSame(pair.Parts[0].Section, pair.Parts[1].Section);
        Assert.AreEqual(2 * b + 10, pair.Width, 1e-8); Assert.AreEqual(h, pair.Height, 1e-8);
        Assert.AreEqual(beta == 0 ? 1 : 0, beam.Assignments.SectionAxes.V1.Y, 1e-9);
        Assert.AreEqual(beta == 0 ? 0 : 1, beam.Assignments.SectionAxes.V1.Z, 1e-9);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilNxIdenticalPairGeometry"));

        var table = new CivilNxResponse("post/table", "{\"T\":{\"FORCE\":\"KN\",\"DIST\":\"M\",\"HEAD\":[\"Index\",\"Elem\",\"Load\",\"Part\",\"Axial\",\"Shear-y\",\"Shear-z\",\"Torsion\",\"Moment-y\",\"Moment-z\"],\"DATA\":[[\"1\",\"3\",\"Q\",\"I[2]\",\"-1\",\"2\",\"3\",\"4\",\"5\",\"6\"]]}}");
        var results = CivilNxResults.Import(report.Model, snapshot, new[] { table }, "pair-results");
        Assert.AreEqual(ImportStatus.Completed, results.Status, string.Join(";", results.Diagnostics.Select(d => d.Message)));
        var forces = beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().Single().ResultBeamForces;
        Assert.AreEqual(-1000, forces.N); Assert.AreEqual(2000, forces.V1); Assert.AreEqual(3000, forces.V2);
        Assert.AreEqual(-5e6, forces.M1); Assert.AreEqual(6e6, forces.M2);
    }

    [DataTestMethod]
    [DataRow("wrong-inertia")] [DataRow("single-db-name")] [DataRow("unknown-extra-dimension")]
    public void IncompletePairIsPreservedWithoutInventingASection(string problem)
    {
        var json = Source(false, false);
        if (problem == "wrong-inertia")
        {
            var table = JsonNode.Parse(json["SECTIONALL"])!; table["T"]!["DATA"]![0]![2] = "999999999";
            json["SECTIONALL"] = table.ToJsonString();
        }
        if (problem == "single-db-name" || problem == "unknown-extra-dimension")
        {
            var root = JsonNode.Parse(json["SECT"])!; var before = root["SECT"]!["2"]!["SECT_BEFORE"]!;
            if (problem == "single-db-name") { before["DATATYPE"] = 1; before["SECT_I"]!["SECT_NAME"] = "L8X6X1"; }
            else before["SECT_I"]!["vSIZE"]![5] = .01;
            json["SECT"] = root.ToJsonString();
        }
        var report = CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(json), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile());
        Assert.AreNotEqual(ImportStatus.Rejected, report.Status);
        Assert.IsNull(report.Model.BeamElements.Values.Single(b => b.Source.OriginalId == "3").BeamProperty);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code.StartsWith("CivilNx") && d.Code.Contains("Unresolved")));
    }
}
