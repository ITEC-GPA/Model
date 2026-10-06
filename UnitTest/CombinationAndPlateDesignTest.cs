using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class CombinationAndPlateDesignTest
{
    private static CombinationTermRecord Case(string name, double factor) => new() { Name = name, IsStaticCase = true, Analysis = "ST", Factor = factor };
    private static CombinationTermRecord Ref(string id, double factor) => new() { Name = id, IsCombination = true, Analysis = "CBC", Factor = factor };
    private static CombinationRecord Def(string name, CombinationKind kind, params CombinationTermRecord[] terms)
    {
        var d = new CombinationRecord { Id = "C/" + name, Name = name, Kind = kind, Source = "C", Record = name }; d.Terms.AddRange(terms); return d;
    }

    private static CombinationRecord[] Definitions() => new[]
    {
        Def("T_0", CombinationKind.Linear, Case("Q+", 0.01), Case("Q-", 0.01)),
        Def("ENV_Q", CombinationKind.Envelope, Case("Q+", 1), Case("Q-", 1), Ref("C/T_0", 1)),
        Def("ENV_S", CombinationKind.Envelope, Ref("C/SLU_1", 1), Ref("C/SLU_2", 1)),
        Def("SLU_1", CombinationKind.Linear, Case("G", 1.35), Case("S", 1.35)),
        Def("SLU_2", CombinationKind.Linear, Case("G", 1.0), Case("S", 1.35)),
        Def("ULS", CombinationKind.Linear, Case("G", 1.35), Ref("C/ENV_S", 1), Ref("C/ENV_Q", -1.5)),
        Def("ABS", CombinationKind.Absolute, Case("G", 1)),
    };

    [TestMethod]
    public void Expansion_GivesTheCartesianProductOfTheEnvelopes_WithMergedFactors()
    {
        var alternatives = CombinationExpansion.Expand(Definitions(), "C/ULS");
        Assert.AreEqual(2 * 3, alternatives.Count);
        var first = alternatives[0];
        Assert.AreEqual(1.35 + 1.35, first.Factors["G"], 1e-12, "G from ULS and from SLU_1 are summed.");
        Assert.AreEqual(1.35, first.Factors["S"], 1e-12); Assert.AreEqual(-1.5, first.Factors["Q+"], 1e-12);
        StringAssert.Contains(first.Label, "ENV_S=SLU_1"); StringAssert.Contains(first.Label, "ENV_Q=Q+");
        var nested = alternatives.Single(a => a.Label.Contains("ENV_S=SLU_2") && a.Label.Contains("ENV_Q=T_0"));
        Assert.AreEqual(-0.015, nested.Factors["Q+"], 1e-12); Assert.AreEqual(-0.015, nested.Factors["Q-"], 1e-12); Assert.AreEqual(2.35, nested.Factors["G"], 1e-12);
        Assert.ThrowsException<NotSupportedException>(() => CombinationExpansion.Expand(Definitions(), "C/ABS"));
        Assert.ThrowsException<InvalidOperationException>(() => CombinationExpansion.Expand(Definitions(), "C/ULS", 5), "Explosion limit.");
        var cyclic = new[] { Def("A", CombinationKind.Linear, Ref("C/B", 1)), Def("B", CombinationKind.Linear, Ref("C/A", 1)) };
        Assert.ThrowsException<InvalidOperationException>(() => CombinationExpansion.Expand(cyclic, "C/A"));
    }

    [TestMethod]
    public void EnvelopeOfTheAlternatives_EqualsTheSolverComponentWiseExtremes()
    {
        // Solver rule for factor * envelope inside a sum: max uses the envelope maximum for positive factors and its minimum for negative ones.
        var value = new Dictionary<string, double> { ["G"] = 10, ["S"] = -4, ["Q+"] = 7, ["Q-"] = -3 };
        var alternatives = CombinationExpansion.Expand(Definitions(), "C/ULS");
        var sums = alternatives.Select(a => a.Factors.Sum(f => f.Value * value[f.Key])).ToArray();
        double envS = Math.Max(1.35 * 10 + 1.35 * -4, 10 + 1.35 * -4), envQmin = new[] { 7.0, -3, 0.01 * 7 + 0.01 * -3 }.Min();
        Assert.AreEqual(1.35 * 10 + envS - 1.5 * envQmin, sums.Max(), 1e-12);
    }

    [TestMethod]
    public void CivilNxCombinations_LinearOnesBecomeModelCombinations_AllArePreserved()
    {
        var json = CivilNxModelWorkflowTestData.Json();
        json["LCOM-CONC"] = "{\"LCOM-CONC\":{\"1\":{\"NAME\":\"SLU_1\",\"ACTIVE\":\"STRENGTH\",\"iTYPE\":0,\"vCOMB\":[{\"ANAL\":\"ST\",\"LCNAME\":\"G1\",\"FACTOR\":1.35},{\"ANAL\":\"ST\",\"LCNAME\":\"Q\",\"FACTOR\":1.5}]},"
            + "\"2\":{\"NAME\":\"ENV\",\"ACTIVE\":\"INACTIVE\",\"iTYPE\":1,\"vCOMB\":[{\"ANAL\":\"ST\",\"LCNAME\":\"Q\",\"FACTOR\":1},{\"ANAL\":\"CBC\",\"LCNAME\":\"SLU_1\",\"FACTOR\":1}]},"
            + "\"3\":{\"NAME\":\"RS\",\"ACTIVE\":\"STRENGTH\",\"iTYPE\":0,\"vCOMB\":[{\"ANAL\":\"RS\",\"LCNAME\":\"Spectrum\",\"FACTOR\":1}]}}}";
        var report = CivilNxGeometryReader.Import(CivilNxModelWorkflowTestData.Snapshot(json), CivilNxModelWorkflowTestData.Identity(), new CivilNxModelProfile());
        Assert.AreEqual(ImportStatus.Partial, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        var m = report.Model; var slu = m.Combinations["SLU_1"];
        Assert.AreEqual(1.35, slu.GetLoadCaseCoefficient(m.LoadCases["G1"]), 1e-12); Assert.AreEqual(1.5, slu.GetLoadCaseCoefficient(m.LoadCases["Q"]), 1e-12);
        Assert.AreEqual(2, m.Combinations.Count, "Non-static terms are not Model combinations.");
        Assert.AreEqual(0, m.Combinations["ENV"].LoadCaseCount, "An envelope is declared without factors, for its rebuilt states.");
        var definitions = CombinationExpansion.Definitions(m);
        Assert.AreEqual(3, definitions.Count); Assert.AreEqual("INACTIVE", definitions.Single(d => d.Name == "ENV").Status);
        Assert.AreEqual(2, CombinationExpansion.Expand(definitions, "LCOM-CONC/ENV").Count);
        Assert.ThrowsException<NotSupportedException>(() => CombinationExpansion.Expand(definitions, "LCOM-CONC/RS"));
        var missing = CivilNxModelWorkflowTestData.Json(); missing["LCOM-CONC"] = json["LCOM-CONC"].Replace("\"LCNAME\":\"SLU_1\"", "\"LCNAME\":\"NOPE\"");
        Assert.AreEqual(ImportStatus.Rejected, CivilNxGeometryReader.Import(CivilNxModelWorkflowTestData.Snapshot(missing), CivilNxModelWorkflowTestData.Identity(), new CivilNxModelProfile()).Status);
    }

    [TestMethod]
    public void PlateElementNodeRows_AreImportedAsExtrapolatedNodePoints()
    {
        var json = CivilNxModelWorkflowTestData.Json();
        var m = CivilNxGeometryReader.Import(CivilNxModelWorkflowTestData.Snapshot(json), CivilNxModelWorkflowTestData.Identity(), new CivilNxModelProfile()).Model;
        var table = new CivilNxResponse("post/table", "{\"T\":{\"FORCE\":\"KN\",\"DIST\":\"M\",\"HEAD\":[\"Index\",\"Elem\",\"Load\",\"Node\",\"Fxx\",\"Fyy\",\"Fxy\",\"Fmax\",\"Fmin\",\"Angle\",\"Mxx\",\"Myy\",\"Mxy\",\"Mmax\",\"Mmin\",\"Angle\",\"Vxx\",\"Vyy\"],\"DATA\":["
            + "[\"1\",\"10\",\"Q\",\"3\",\"1\",\"2\",\"3\",\"0\",\"0\",\"0\",\"4\",\"5\",\"6\",\"0\",\"0\",\"0\",\"7\",\"8\"],"
            + "[\"2\",\"10\",\"Q\",\"5\",\"2\",\"2\",\"3\",\"0\",\"0\",\"0\",\"4\",\"5\",\"6\",\"0\",\"0\",\"0\",\"7\",\"8\"]]}}");
        var report = CivilNxResults.Import(m, CivilNxModelWorkflowTestData.Snapshot(json), new[] { table }, "plate-nodes");
        Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        var plate = m.AreaElements.Values.Single(a => a.Source.OriginalId == "10");
        var samples = plate.Results.SelectMany(r => r.Results).OfType<PointResultPlateForces>().ToArray();
        Assert.AreEqual(2, samples.Length); Assert.IsTrue(samples.All(s => s.PointKind == ShellResultPointKind.ElementNodeExtrapolated));
        var at3 = samples.Single(s => s.Forces.Fxx == 1);
        Assert.AreEqual(2000, at3.Location.X, 1e-9); Assert.AreEqual(-1000, at3.Location.Y, 1e-9, "Node 3 (4,0,3) m from the centre (2,1,3) m in the plate axes.");
        Assert.AreEqual(m.NodesElements.Values.Single(n => n.Source.OriginalId == "3").Id, at3.SourceNodeId);
        var foreign = new CivilNxResponse("post/table", table.Json.Replace("\"10\",\"Q\",\"3\"", "\"10\",\"Q\",\"1\""));
        Assert.AreEqual(ImportStatus.Rejected, CivilNxResults.Import(m, CivilNxModelWorkflowTestData.Snapshot(json), new[] { foreign }, "bad").Status, "Node 1 is not a plate node.");
    }

    [DataTestMethod]
    [DataRow(10d, 5d, 3d, 13d, 8d, 0d, 0d)]
    [DataRow(10d, -5d, 3d, 11.8, 0d, 0d, 5.9)]
    [DataRow(-10d, -5d, -3d, 0d, 0d, 13d, 8d)]
    [DataRow(0d, 0d, 4d, 4d, 4d, 4d, 4d)]
    public void WoodArmer_MatchesTheTextbookRules(double mxx, double myy, double mxy, double xPos, double yPos, double xNeg, double yNeg)
    {
        var m = WoodArmer.Compute(mxx, myy, mxy);
        Assert.AreEqual(xPos, m.MxPositiveFace, 1e-12); Assert.AreEqual(yPos, m.MyPositiveFace, 1e-12);
        Assert.AreEqual(xNeg, m.MxNegativeFace, 1e-12); Assert.AreEqual(yNeg, m.MyNegativeFace, 1e-12);
    }

    [TestMethod]
    public void WoodArmer_EveryNormalDirectionIsCovered()
    {
        // The Wood-Armer moments must cover the normal moment in every direction: m(θ) = mx c² + my s² + 2 mxy s c on each face.
        var random = new Random(7);
        for (int i = 0; i < 2000; i++)
        {
            double mx = random.NextDouble() * 200 - 100, my = random.NextDouble() * 200 - 100, mxy = random.NextDouble() * 200 - 100;
            var w = WoodArmer.Compute(mx, my, mxy);
            for (double t = 0; t < Math.PI; t += Math.PI / 36)
            {
                double c = Math.Cos(t), s = Math.Sin(t), demand = mx * c * c + my * s * s + 2 * mxy * s * c;
                Assert.IsTrue(w.MxPositiveFace * c * c + w.MyPositiveFace * s * s >= demand - 1e-9, "+z face");
                Assert.IsTrue(w.MxNegativeFace * c * c + w.MyNegativeFace * s * s >= -demand - 1e-9, "-z face");
            }
        }
    }
}

/// <summary>Access to the synthetic Civil NX responses of <see cref="CivilNxModelWorkflowTest"/>.</summary>
internal static class CivilNxModelWorkflowTestData
{
    public static Dictionary<string, string> Json() => CivilNxModelWorkflowTest.Json();
    public static CivilNxSnapshot Snapshot(Dictionary<string, string> json) => CivilNxModelWorkflowTest.Snapshot(json);
    public static AnalysisSource Identity() => CivilNxModelWorkflowTest.Identity();
}
