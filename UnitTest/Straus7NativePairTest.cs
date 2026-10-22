using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Processing;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class Straus7NativePairTest
{
    [TestMethod, TestCategory("Straus7Native")]
    public void EightNativePairsKeepGeometryAndGlobalActionSigns()
    {
        var folder = Environment.GetEnvironmentVariable("GPC_STRAUS7_PACKED_PAIRS");
        if (string.IsNullOrWhiteSpace(folder)) Assert.Inconclusive("Generate Fixtures/Straus7/generate_packed_pairs.py and set GPC_STRAUS7_PACKED_PAIRS.");
        var path = Path.Combine(folder!, "packed-pairs.st7");
        var converter = new Straus7ApiConverter();
        var imported = converter.Import(new Straus7ImportRequest { ModelPath = path, ModelRevision = "native-pairs-v1", AnalysisId = "pair-lsa" });
        Assert.IsNotNull(imported.Model, string.Join(";", imported.Diagnostics.Select(d => d.Message)));
        var model = imported.Model;
        Assert.AreEqual(8, model.BeamElements.Count);
        foreach (var beam in model.BeamElements.Values)
        {
            int number = int.Parse(beam.Source.OriginalId), mirror = (number - 1) % 4 + 1; bool channel = number > 4;
            var expected = IdenticalPairReference.Integrate(channel, channel ? 200 : 150, channel ? 75 : 100, 10, 12, 10, mirror);
            Assert.IsInstanceOfType(beam.BeamProperty, typeof(SteelSection));
            var pair = (SectionBuiltUp)((SteelSection)beam.BeamProperty).SectionShape;
            Assert.AreSame(pair.Parts[0].Section, pair.Parts[1].Section);
            Assert.AreEqual(expected.Area, pair.Area, 1e-7);
            Assert.AreEqual(expected.Ixx, pair.Jxx, 1e-5); Assert.AreEqual(expected.Iyy, pair.Jyy, 1e-5);
        }
        var native = converter.ReadResults(new Straus7ResultsRequest { ModelPath = path, ResultPath = Path.Combine(folder!, "packed-pairs.LSA"),
            CaseNumbers = new[] { 1 }, BeamNumbers = Enumerable.Range(1, 8).ToArray(), MinimumBeamStations = 3 });
        Assert.AreEqual(ImportStatus.Completed, native.Status, string.Join(";", native.Diagnostics.Select(d => d.Message)));
        var results = Straus7LinearStaticResults.Import(model, native, "pairs", new Dictionary<int, string> { [1] = Straus7ApiConverter.CaseName(1, "Pairs") });
        Assert.AreEqual(ImportStatus.Completed, results.Status, string.Join(";", results.Diagnostics.Select(d => d.Message)));
        using var reference = System.Text.Json.JsonDocument.Parse(File.ReadAllText(Path.Combine(folder!, "properties.json")));
        foreach (var beam in model.BeamElements.Values)
        {
            var axes = beam.Assignments.SectionAxes;
            int number = int.Parse(beam.Source.OriginalId);
            var property = reference.RootElement[number - 1];
            var source = property.GetProperty("properties").EnumerateArray().Select(v => v.GetDouble()).ToArray();
            var principal = property.GetProperty("axes").EnumerateArray().Select(v => v.GetDouble()).ToArray();
            // Independent statics: torque about S equals the applied centroidal torque minus (GS x F).e3.
            var gs = new Vector3d(source[4]*principal[0]+source[5]*principal[3], source[4]*principal[1]+source[5]*principal[4],
                source[4]*principal[2]+source[5]*principal[5]);
            var nativeOffset = native.Tables.Single(t => t.Number == number && t.Quantity == "BeamForceGlobal").BeamShearCentreOffsetGlobal;
            CollectionAssert.AreEqual(new[] { gs.X, gs.Y, gs.Z }, nativeOffset);
            double expectedTorque = 7000 - gs.CrossProduct(new Vector3d(-10000, 200, 300)).X;
            foreach (var sample in beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>())
            {
                var f = sample.ResultBeamForces;
                Vector3d force = f.V1 * axes.V1 + f.V2 * axes.V2 + f.N * axes.V3;
                Vector3d moment = f.M1 * axes.V1 + f.M2 * axes.V2 + f.T * axes.V3;
                double arm = 4000 * (1 - sample.ParametricDistance);
                Assert.AreEqual(-10000, force.X, 1e-5); Assert.AreEqual(200, force.Y, 1e-5); Assert.AreEqual(300, force.Z, 1e-5);
                Assert.AreEqual(expectedTorque, moment.X, 1e-4, "Native solver torsion about the shear centre, not the applied centroidal torque.");
                Assert.AreEqual(7000, moment.X + gs.CrossProduct(force).X, 1e-4, "Recover the centroidal torque for the equilibrium check only."); Assert.AreEqual(4000 - 300 * arm, moment.Y, 1e-4); Assert.AreEqual(-6000 + 200 * arm, moment.Z, 1e-4);
            }
            Assert.IsTrue(beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().Count() >= 3);
        }
    }
}
