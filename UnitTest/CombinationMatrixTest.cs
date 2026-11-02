using System.Globalization;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>GPCModel 5.38: <see cref="CombinationMatrix"/> gives, for every location, the states that <see cref="CombinationRebuild"/> registers,
/// with the same values bit for bit, the same roles and state ids, the same principal components and the same diagnostics, in the same order;
/// also through the rebuild itself for the elements it does not prepare.</summary>
[TestClass]
public class CombinationMatrixTest
{
    // Twelve static cases on two beams, two plates (corners), a support and two displaced nodes; some cases repeat others, so that ties occur.
    private static readonly string[] Cases = { "G1", "Q", "C1", "C2", "C3", "C4", "C5", "C6", "C7", "C8", "C9", "C10" };

    private static GPC.Model.Models.Model Random(int seed, bool dropOnBeam = false)
    {
        var json = CombinationRebuildTest.Json(); // with the combinations SLU_1 and ENV, the output of the calls
        json["STLD"] = "{\"STLD\":{" + string.Join(",", Cases.Select((c, i) => "\"" + (i + 1) + "\":{\"NO\":" + (i + 1) + ",\"NAME\":\"" + c + "\",\"TYPE\":\"L\",\"DESC\":\"\"}")) + "}}";
        var random = new Random(seed); var rows = new Dictionary<string, List<string>> { ["beam"] = new(), ["plate"] = new(), ["reaction"] = new(), ["displacement"] = new() };
        var repeated = new Dictionary<string, string[]>();
        string V(string key, int count, double scale)
        {
            // C4 repeats C3 and C8 repeats G1 at every location: equal alternatives, the first governs.
            var values = Enumerable.Range(0, count).Select(_ => (random.Next(0, 9) == 0 ? 0 : Math.Round((random.NextDouble() - .5) * scale, random.Next(1, 6)))
                .ToString("R", CultureInfo.InvariantCulture)).ToArray();
            repeated[key] = values;
            return string.Join(",", values.Select(v => "\"" + v + "\""));
        }
        string Values(string load, string location, int count, double scale)
        {
            string source = load == "C4" ? "C3" : load == "C8" ? "G1" : null;
            if (source != null) return string.Join(",", repeated[source + "|" + location].Select(v => "\"" + v + "\""));
            return V(load + "|" + location, count, scale);
        }
        int index = 0;
        foreach (var load in Cases)
        {
            foreach (var (element, parts) in new[] { ("3", new[] { "I[2]", "1/4", "2/4", "J[3]" }), ("1", new[] { "I[1]", "J[2]" }) })
                foreach (var part in parts)
                    if (!(dropOnBeam && element == "3" && load == "C7"))
                        rows["beam"].Add("[\"" + ++index + "\",\"" + element + "\",\"" + load + "\",\"" + part + "\"," + Values(load, "B" + element + part, 6, 40) + "]");
            foreach (var plate in new[] { "10", "11" })
                foreach (var node in new[] { "2", "3", "4", "5" })
                {
                    var v = Values(load, "P" + plate + node, 8, 20).Split(',');
                    // Fxx, Fyy, Fxy, Fmax, Fmin, Angle, Mxx, Myy, Mxy, Mmax, Mmin, Angle, Vxx, Vyy (the principal values are not read).
                    rows["plate"].Add("[\"" + ++index + "\",\"" + plate + "\",\"" + load + "\",\"" + node + "\"," + string.Join(",", v[0], v[1], v[2], "\"0\"", "\"0\"", "\"0\"", v[3], v[4], v[5], "\"0\"", "\"0\"", "\"0\"", v[6], v[7]) + "]");
                }
            rows["reaction"].Add("[\"" + ++index + "\",\"1\",\"" + load + "\"," + Values(load, "R1", 6, 10) + "]");
            foreach (var node in new[] { "1", "3" }) rows["displacement"].Add("[\"" + ++index + "\",\"" + node + "\",\"" + load + "\"," + Values(load, "D" + node, 6, .01) + "]");
        }
        CivilNxResponse Table(string head, List<string> data) => new("post/table", "{\"T\":{\"FORCE\":\"KN\",\"DIST\":\"M\",\"HEAD\":[" + head + "],\"DATA\":[" + string.Join(",", data) + "]}}");
        var tables = new[]
        {
            Table("\"Index\",\"Elem\",\"Load\",\"Part\",\"Axial\",\"Shear-y\",\"Shear-z\",\"Torsion\",\"Moment-y\",\"Moment-z\"", rows["beam"]),
            Table("\"Index\",\"Elem\",\"Load\",\"Node\",\"Fxx\",\"Fyy\",\"Fxy\",\"Fmax\",\"Fmin\",\"Angle\",\"Mxx\",\"Myy\",\"Mxy\",\"Mmax\",\"Mmin\",\"Angle\",\"Vxx\",\"Vyy\"", rows["plate"]),
            Table("\"Index\",\"Node\",\"Load\",\"FX\",\"FY\",\"FZ\",\"MX\",\"MY\",\"MZ\"", rows["reaction"]),
            Table("\"Index\",\"Node\",\"Load\",\"DX\",\"DY\",\"DZ\",\"RX\",\"RY\",\"RZ\"", rows["displacement"]),
        };
        var m = CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(json), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile()).Model;
        var report = CivilNxResults.Import(m, CivilNxModelWorkflowTest.Snapshot(json), tables, "static");
        Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        return m;
    }

    /// <summary>Random alternatives: some factors negative or zero, an alternative without load, one repeating another (a tie).</summary>
    private static CombinationAlternative[] Alternatives(int seed, int count)
    {
        var random = new Random(seed); var list = new List<CombinationAlternative>();
        for (int a = 0; a < count; a++)
        {
            var factors = new Dictionary<string, double>();
            if (a == 1) factors["G1"] = 0; // no load: a zero state
            else if (a == 3) foreach (var p in list[2].Factors) factors[p.Key] = p.Value; // the same as the previous: a tie
            else foreach (var c in Cases.OrderBy(_ => random.Next()).Take(random.Next(1, 8)))
                    factors[c] = random.Next(0, 6) == 0 ? 0 : Math.Round((random.NextDouble() - .3) * 3, random.Next(0, 4));
            list.Add(new CombinationAlternative("ENV=A" + a.ToString(CultureInfo.InvariantCulture), factors));
        }
        return list.ToArray();
    }

    private static string[] Roles(ResultLocation sample)
    {
        string text = sample.State.Coverage; int end = text.IndexOf(" over the ", StringComparison.Ordinal);
        return text.StartsWith("Governs ", StringComparison.Ordinal) ? text.Substring(8, end - 8).Split(new[] { ", " }, StringSplitOptions.None) : new[] { "linear" };
    }

    private static void Same(CombinationRebuildReport rebuilt, CombinationMatrixReport matrix, string what)
    {
        CollectionAssert.AreEqual(rebuilt.Diagnostics.Select(d => d.Code + "|" + d.Severity + "|" + d.ElementId + "|" + d.Message).ToArray(),
            matrix.Diagnostics.Select(d => d.Code + "|" + d.Severity + "|" + d.ElementId + "|" + d.Message).ToArray(), what + ": diagnostics");
        Assert.AreEqual(rebuilt.Locations, matrix.Locations, what + ": locations"); Assert.AreEqual(rebuilt.Alternatives, matrix.Alternatives, what + ": alternatives");
        var states = matrix.Results.SelectMany(l => l.States.Select(s => (Location: l, State: s))).ToArray();
        Assert.AreEqual(rebuilt.Samples.Count, states.Length, what + ": states");
        for (int i = 0; i < states.Length; i++)
        {
            var sample = rebuilt.Samples[i]; var (location, state) = states[i];
            Assert.AreEqual(sample.State.ConcomitantStateId, state.StateId, what + ": state id " + i);
            CollectionAssert.AreEqual(Roles(sample), state.Roles.ToArray(), what + ": roles " + i);
            var expected = ResultAlgebra.Values(sample); var actual = state.Values.ToArray();
            Assert.AreEqual(expected.Length, actual.Length);
            for (int k = 0; k < expected.Length; k++)
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected[k]), BitConverter.DoubleToInt64Bits(actual[k]), what + ": value " + i + "/" + k + " " + expected[k] + " vs " + actual[k]);
            Assert.IsTrue(ReferenceEquals(sample.State.DerivedFrom[0], location.Location) || ResultAlgebra.SameLocation(sample, location.Location), what + ": location " + i);
            CollectionAssert.AreEqual(ResultAlgebra.Names(sample), location.Components.ToArray(), what + ": components " + i);
        }
        Assert.AreEqual(rebuilt.PlatePrincipals.Count, matrix.PlatePrincipals.Count, what + ": principal locations");
        for (int i = 0; i < rebuilt.PlatePrincipals.Count; i++)
        {
            var r = rebuilt.PlatePrincipals[i]; var m = matrix.PlatePrincipals[i];
            Assert.AreSame(r.Owner, m.Owner); Assert.AreSame(r.Location, m.Location, what + ": principal location " + i);
            for (int k = 0; k < r.Components.Count; k++)
            {
                var a = r.Components[k]; var b = m.Components[k];
                Assert.AreEqual(a.Component, b.Component); Assert.AreEqual(a.MinimumStateId, b.MinimumStateId); Assert.AreEqual(a.MaximumStateId, b.MaximumStateId);
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(a.Minimum), BitConverter.DoubleToInt64Bits(b.Minimum)); Assert.AreEqual(BitConverter.DoubleToInt64Bits(a.Maximum), BitConverter.DoubleToInt64Bits(b.Maximum));
                CollectionAssert.AreEqual(a.MinimumState.Select(BitConverter.DoubleToInt64Bits).ToArray(), b.MinimumState.Select(BitConverter.DoubleToInt64Bits).ToArray());
                CollectionAssert.AreEqual(a.MaximumState.Select(BitConverter.DoubleToInt64Bits).ToArray(), b.MaximumState.Select(BitConverter.DoubleToInt64Bits).ToArray());
            }
        }
    }

    private static void Compare(GPC.Model.Models.Model m, IReadOnlyList<CombinationAlternative> alternatives, string what)
    {
        var output = m.Combinations["ENV"];
        foreach (bool principals in new[] { false, true })
        {
            var cache = new CombinationRebuildCache { SourceFingerprints = false, PlatePrincipals = principals };
            var rebuilt = CombinationRebuild.Rebuild(m, output, alternatives, "static", m.AllElements, false, default, cache);
            var matrix = CombinationMatrix.Prepare(m, "static", m.AllElements);
            Same(rebuilt, matrix.Evaluate(output, alternatives, principals), what + (principals ? " + principali" : ""));
            Assert.AreEqual(0, matrix.Evaluate(output, alternatives, principals).RebuiltElements, what + ": every element of the fixture is prepared");
            // The elements that are not prepared go through the rebuild itself: the same report.
            matrix.RebuildAll = true;
            var through = matrix.Evaluate(output, alternatives, principals);
            Same(rebuilt, through, what + " (rebuild)");
            if (!rebuilt.IsRejected) Assert.AreEqual(matrix.Elements, through.RebuiltElements, what + ": every element through the rebuild");
        }
    }

    [TestMethod]
    public void Matrix_GivesTheStatesOfTheRebuild_BitForBit_OnTheFixtureOfTheRebuild()
    {
        var m = CombinationRebuildTest.Imported();
        Compare(m, new[] { new CombinationAlternative("", new Dictionary<string, double> { ["G1"] = 1.35, ["Q"] = 1.5 }) }, "lineare");
        Compare(m, new[]
        {
            new CombinationAlternative("ENV=Q", new Dictionary<string, double> { ["Q"] = 1 }),
            new CombinationAlternative("ENV=SLU_1", new Dictionary<string, double> { ["G1"] = 1.35, ["Q"] = 1.5 }),
            new CombinationAlternative("ENV=-Q", new Dictionary<string, double> { ["Q"] = -0.5, ["G1"] = 1 })
        }, "inviluppo");
    }

    [TestMethod]
    public void Matrix_GivesTheStatesOfTheRebuild_BitForBit_OnRandomResultsAndAlternatives()
    {
        for (int seed = 1; seed <= 6; seed++)
        {
            var m = Random(seed);
            // A linear combination with some load (the random alternatives may have none).
            var linear = Alternatives(100 + seed, 8).First(a => a.Factors.Values.Any(f => f != 0));
            Compare(m, new[] { new CombinationAlternative("", linear.Factors) }, "lineare " + seed);
            Compare(m, Alternatives(200 + seed, 40), "inviluppo " + seed);
        }
    }

    [TestMethod]
    public void Matrix_SkipsTheElementsWithIncompleteStaticCases_AndRejectsTheCallsOfTheRebuild_WithTheSameDiagnostics()
    {
        var m = Random(7, dropOnBeam: true); var output = m.Combinations["ENV"];
        var alternatives = Alternatives(300, 12).Select((a, i) => new CombinationAlternative(a.Label, i == 0 ? new Dictionary<string, double>(a.Factors) { ["C7"] = 2 } : a.Factors)).ToArray();
        Compare(m, alternatives, "caso mancante sulla trave");
        var matrix = CombinationMatrix.Prepare(m, "static", m.AllElements);
        foreach (var wrong in new[]
        {
            new[] { new CombinationAlternative("", new Dictionary<string, double> { ["Nessuno"] = 1 }) },
            new[] { new CombinationAlternative("X", new Dictionary<string, double> { ["G1"] = 1 }), new CombinationAlternative("X", new Dictionary<string, double> { ["Q"] = 1 }) },
            new[] { new CombinationAlternative("", new Dictionary<string, double> { ["G1"] = 0 }) },
            new[] { new CombinationAlternative("", new Dictionary<string, double> { ["G1"] = double.NaN }) },
        })
        {
            var rebuilt = CombinationRebuild.Rebuild(m, output, wrong, "static", m.AllElements, false);
            var evaluated = matrix.Evaluate(output, wrong);
            Assert.IsTrue(rebuilt.IsRejected && evaluated.IsRejected);
            Same(rebuilt, evaluated, "rifiuto");
        }
    }

    [TestMethod]
    public void Matrix_OnlyForThePreparedDataset_AndTheDeclaredOutput()
    {
        var m = Random(8);
        Assert.ThrowsException<ArgumentException>(() => CombinationMatrix.Prepare(m, "altro", m.AllElements));
        var matrix = CombinationMatrix.Prepare(m, "static", m.AllElements);
        var undeclared = new GPC.Model.Combinations.Combination("Non dichiarata", Guid.NewGuid());
        var report = matrix.Evaluate(undeclared, Alternatives(400, 2));
        Assert.IsTrue(report.IsRejected); StringAssert.Contains(report.Diagnostics.Single().Message, "DeclareOutputCaseBeforeAnalysis");
    }
}
