using System.Text;
using System.Text.Json;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Converter.MidasCivil;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Results.Locations;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

// Newly generated native Civil NX 9.5.5 exports, not invented API responses.
[TestClass]
public class MidasNativePairImportTest
{
    private static string Folder => Path.Combine(AppContext.BaseDirectory, "Fixtures", "MidasCivil", "identical-pairs-native-9.5.5");
    private static AnalysisSource Identity() => new() { Program = "MIDAS Civil NX", SolverVersion = "9.5.5", ModelRevision = "pairs-native-20261022", AnalysisId = "pairs-static" };
    private static CivilNxSnapshot Snapshot(bool computed = true)
    {
        var names = CivilNxModelProfile.RequiredTables.Concat(CivilNxModelProfile.OptionalTables).Concat(new[] { "MATERIAL" });
        if (computed) names = names.Concat(new[] { "SECTIONALL" });
        return new CivilNxSnapshot(names.ToDictionary(n => n, n => new CivilNxResponse(n is "SECTIONALL" or "MATERIAL" ? "post/table" : "db/" + n,
            File.ReadAllText(Path.Combine(Folder, n + ".json")))));
    }
    private static (bool Channel, double H, double B, double Gap) Dimensions(int id) => (id <= 2, id <= 2 ? 200 : id <= 4 ? 150 : 100,
        id <= 2 ? 75 : id <= 4 ? 100 : 150, id % 2 == 1 ? 10 : 30);
    private static SectionBuiltUp Geometry(int id, object shape)
    {
        Assert.IsInstanceOfType(shape, typeof(SectionBuiltUp));
        var pair = (SectionBuiltUp)shape; var d = Dimensions(id);
        var expected = IdenticalPairReference.Integrate(d.Channel, d.H, d.B, 10, 12, d.Gap, 3);
        Assert.AreEqual(expected.Area, pair.Area, 1e-7);
        Assert.AreEqual(expected.Ixx, pair.Jxx, 1e-5); Assert.AreEqual(expected.Iyy, pair.Jyy, 1e-5);
        Assert.AreEqual(2 * d.B + d.Gap, pair.Width, 1e-8); Assert.AreEqual(d.H, pair.Height, 1e-8);
        Assert.AreSame(pair.Parts[0].Section, pair.Parts[1].Section);
        Assert.AreEqual(2, pair.GetCurveOutlines().Count());
        return pair;
    }

    [DataTestMethod]
    [DataRow(1)] [DataRow(2)] [DataRow(3)] [DataRow(4)] [DataRow(5)] [DataRow(6)]
    public void NativeApiGeometryInertiasAndSixActionsMatchIndependentStatics(int id)
    {
        var snapshot = Snapshot();
        var imported = CivilNxGeometryReader.Import(snapshot, Identity(), new CivilNxModelProfile());
        Assert.AreNotEqual(ImportStatus.Rejected, imported.Status, string.Join(";", imported.Diagnostics.Select(d => d.Message)));
        Assert.AreEqual(12, imported.Model.BeamElements.Count);
        var results = CivilNxResults.Import(imported.Model, snapshot, new[] { new CivilNxResponse("post/table", File.ReadAllText(Path.Combine(Folder, "BEAMFORCE.json"))) }, "native-pairs");
        Assert.AreEqual(ImportStatus.Completed, results.Status, string.Join(";", results.Diagnostics.Select(d => d.Message)));
        foreach (var number in new[] { 2 * id - 1, 2 * id })
        {
            var beam = imported.Model.BeamElements.Values.Single(b => b.Source.OriginalId == number.ToString());
            Geometry(id, ((SteelSection)beam.BeamProperty).SectionShape);
            var axes = beam.Assignments.SectionAxes;
            foreach (var sample in beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>())
            {
                var f = sample.ResultBeamForces;
                Vector3d force = f.V1 * axes.V1 + f.V2 * axes.V2 + f.N * axes.V3;
                Vector3d moment = f.M1 * axes.V1 + f.M2 * axes.V2 + f.T * axes.V3;
                double arm = 4000 * (1 - sample.ParametricDistance);
                Assert.AreEqual(-10000, force.X, 1e-5); Assert.AreEqual(200, force.Y, 1e-5); Assert.AreEqual(300, force.Z, 1e-5);
                Assert.AreEqual(7000, moment.X, 1e-4); Assert.AreEqual(4000 - 300 * arm, moment.Y, 1e-4); Assert.AreEqual(-6000 + 200 * arm, moment.Z, 1e-4);
            }
            Assert.AreEqual(5, beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().Count());
        }
    }

    [DataTestMethod]
    [DataRow(true)] [DataRow(false)]
    public void NativeSectionInspectionWorksWithOrWithoutOptionalComputedTable(bool computed)
    {
        var catalog = new CivilNxModelProfile().ReadSectionCatalog(Snapshot(computed), Identity());
        Assert.AreEqual(6, catalog.Sections.Count);
        using var values = JsonDocument.Parse(File.ReadAllText(Path.Combine(Folder, "SECTIONALL.json")));
        var table = values.RootElement.EnumerateObject().Single().Value;
        var columns = table.GetProperty("HEAD").EnumerateArray().Select(v => v.GetString()).ToList();
        foreach (var section in catalog.Sections)
        {
            var inspected = SectionShapeInspection.Read(section);
            var pair = Geometry(int.Parse(section.Id), inspected.Shape);
            Assert.AreEqual(2, inspected.Parts.SelectMany(p => p.Shape.GetCurveOutlines()).Count());
            var row = table.GetProperty("DATA").EnumerateArray().Single(r => r[columns.IndexOf("ID")].GetString() == section.Id);
            double Value(string key) => double.Parse(row[columns.IndexOf(key)].GetString()!, System.Globalization.CultureInfo.InvariantCulture);
            // Native SECTIONALL currently formats section data to 4 decimals, irrespective of requested precision.
            Assert.AreEqual(Value("Area"), pair.Area, .00006);
            Assert.AreEqual(Value("Iyy"), pair.Jxx, .00006); Assert.AreEqual(Value("Izz"), pair.Jyy, .00006);
        }
    }

    [TestMethod]
    public void NativeMctReadsTheSamePairsMaterialsAndBetaAxes()
    {
        using var file = File.OpenRead(Path.Combine(Folder, "packed-pairs-full.mct"));
        var report = new MidasCivilTextReader("native-pairs").Import(file);
        Assert.AreEqual(ImportStatus.Partial, report.Status, string.Join(";", report.Diagnostics.Select(d => d.Message)));
        Assert.IsFalse(report.VerificationEnabled, "MCT contains inputs, no matched result dataset.");
        Assert.AreEqual(12, report.Model.BeamElements.Count);
        foreach (var beam in report.Model.BeamElements.Values)
        {
            int n = int.Parse(beam.Source.OriginalId);
            Assert.IsInstanceOfType(beam.BeamProperty, typeof(SteelSection));
            var steel = (SteelSection)beam.BeamProperty;
            Geometry((n + 1) / 2, steel.SectionShape);
            Assert.AreEqual(355, steel.SteelMaterial.Fyk);
            Assert.AreEqual(n % 2 == 1 ? 1 : 0, beam.Assignments.SectionAxes.V1.Y, 1e-9);
            Assert.AreEqual(n % 2 == 1 ? 0 : 1, beam.Assignments.SectionAxes.V1.Z, 1e-9);
        }
    }

    [DataTestMethod]
    [DataRow("S355", true)] [DataRow("custom steel", false)]
    public void TextUserSteelPreservesElasticInputAndDiagnosesNormativeModulus(string name, bool recognised)
    {
        string text = "*UNIT\nKN,M\n*NODE\n1,0,0,0\n2,4,0,0\n*ELEMENT\n1,BEAM,1,1,1,2,0,0\n"
            + "*MATERIAL\n1,STEEL," + name + ",0,0,,C,NO,0,2,205000000,0.28,0.000012,76.9822025,0\n"
            + "*SECTION\n1,DBUSER,pair,CC,0,0,0,0,0,0,YES,NO,2C,2,0.2,0.075,0.01,0.012,0.01,0,0,0,0,0\n*ENDDATA\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        var reader = new MidasCivilTextReader("user-material");
        var source = reader.Read(stream, default).Materials.Single();
        Assert.AreEqual(205000, source.ElasticModulus!.Value, 1e-7);
        Assert.AreEqual(7.85e-9, source.Density!.Value, 1e-18);
        stream.Position = 0;
        var report = reader.Import(stream);
        Assert.AreEqual(ImportStatus.Partial, report.Status);
        var beam = report.Model.BeamElements.Values.Single();
        if (!recognised) { Assert.IsNull(beam.BeamProperty); return; }
        var material = ((SteelSection)beam.BeamProperty).SteelMaterial;
        Assert.AreEqual(355, material.Fyk);
        // The existing mapper retains the normative material and diagnoses a different analysis modulus.
        Assert.AreEqual(210000, material.ElasticModulusCompression, 1e-7);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "MaterialElasticModulusDiffers"));
        Assert.AreEqual(7.85e-9, material.Density, 1e-18);
    }

    [TestMethod]
    public void TextScopedNodalLoadCannotBeFlattenedToAnOrdinaryLoad()
    {
        string text = "*UNIT\nN,MM\n*NODE\n1,0,0,0\n*STLDCASE\nQ,L,test\n*USE-STLD,Q\n*CONLOAD\n1,1,2,3,4,5,6,,UnknownScope\n*ENDDATA\n";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(text));
        Assert.AreEqual(ImportStatus.Rejected, new MidasCivilTextReader("scope").Import(stream).Status);
    }

    [DataTestMethod]
    [DataRow("offset")] [DataRow("database")] [DataRow("different-shape")] [DataRow("extra-dimension")] [DataRow("invalid-thickness")]
    public void TextPairsDoNotGuessUnsupportedDetails(string change)
    {
        string row = "1,DBUSER,pair,CC,0,0,0,0,0,0,YES,NO,2L,2,150,100,10,12,10,0,0,0,0,0";
        if (change == "offset") row = row.Replace(",CC,", ",CT,");
        if (change == "database") row = row.Replace(",2L,2,", ",2L,1,");
        if (change == "different-shape") row = row.Replace(",2L,", ",CL,");
        if (change == "extra-dimension") row = row.Replace(",10,0,0,0,0,0", ",10,1,0,0,0,0");
        if (change == "invalid-thickness") row = row.Replace(",150,100,10,12,", ",150,100,110,12,");
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("*UNIT\nN,MM\n*NODE\n1,0,0,0\n*SECTION\n" + row + "\n*ENDDATA\n"));
        if (change is "extra-dimension" or "invalid-thickness")
        {
            Assert.AreEqual(ImportStatus.Rejected, new MidasCivilTextReader("bad").Import(stream).Status); return;
        }
        var batch = new MidasCivilTextReader("source").Read(stream, default);
        if (change == "offset")
        {
            var inspected = SectionShapeInspection.Read(batch.Sections.Single());
            Assert.IsNotNull(inspected.Shape); Assert.IsNull(inspected.CentroidOffset);
        }
        else Assert.AreEqual(0, batch.Sections.Count);
    }
}
