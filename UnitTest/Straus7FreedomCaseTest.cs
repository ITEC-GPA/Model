using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Model.Core.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>Linear static results and LSA combinations of Straus7 models with several freedom cases, through the transport double of the R3 API
/// (no Straus7 needed). The result case names are those of the R31 solver in native-freedom-cases-r31.zip (Straus7NativeFreedomCasesTest).</summary>
public partial class Straus7AssignmentImportTest
{
    private static Straus7ResultCase Result(int number, string name) => new() { Number = number, Name = name };
    private static GPC.Model.Models.Model CaseModel(params string[] names)
    {
        var model = new GPC.Model.Models.Model();
        for (int i = 0; i < names.Length; i++) model.LoadCases.Add(new GPC.Model.LoadCases.LoadCaseBase(Straus7ApiConverter.CaseName(i + 1, names[i])));
        return model;
    }

    [TestMethod]
    public void CaseMap_ResultCasesOfSeveralFreedomCases_MapToTheirLoadCase()
    {
        var model = CaseModel("G1", "Q", "G1", "W");
        // R31: "<result case>: [<load case>: <name>] [<freedom case>: <name>]", numbered by freedom case, then load case.
        var cases = Straus7LinearStaticResults.Cases(model, new[] { Result(1, "1: [1: G1] [1: Freedom Case 1]"), Result(2, "2: [4: W] [1: Freedom Case 1]"),
            Result(3, "3: [2: Q] [2: Freedom Case 1]"), Result(4, "4: [3: G1] [2: Freedom Case 1]") });
        CollectionAssert.AreEqual(new[] { "Case 1: G1", "Case 4: W", "Case 2: Q", "Case 3: G1" }, cases.Select(c => c.ModelCase).ToArray());
        CollectionAssert.AreEqual(new[] { "1: G1 · FC 1", "4: W · FC 1", "2: Q · FC 2", "3: G1 · FC 2" }, cases.Select(c => c.Label).ToArray(), "Unique labels although the names repeat.");
        CollectionAssert.AreEqual(new int?[] { 1, 1, 2, 2 }, cases.Select(c => c.FreedomCase).ToArray());
        CollectionAssert.AreEqual(new int?[] { 1, 4, 2, 3 }, cases.Select(c => c.LoadCase).ToArray());
        Assert.IsTrue(cases.All(c => c.FreedomCaseName == "Freedom Case 1" && c.Reason == null));
        var map = Straus7LinearStaticResults.CaseMap(model, new[] { Result(2, "2: [4: W] [1: Freedom Case 1]") });
        Assert.AreEqual("Case 4: W", map[2], "The leading number is the result case, the bracket the load case.");

        // Load case 2 under both freedom cases: GPC.Model keeps one result per load case, so neither is mapped.
        var shared = Straus7LinearStaticResults.Cases(model, new[] { Result(1, "1: [1: G1] [1: Freedom Case 1]"), Result(2, "2: [2: Q] [1: Freedom Case 1]"),
            Result(3, "3: [4: W] [1: Freedom Case 1]"), Result(4, "4: [2: Q] [2: Freedom Case 1]"), Result(5, "5: [3: G1] [2: Freedom Case 1]") });
        CollectionAssert.AreEqual(new[] { "Case 1: G1", null, "Case 4: W", null, "Case 3: G1" }, shared.Select(c => c.ModelCase).ToArray());
        StringAssert.Contains(shared[1].Reason, "load case 2 under 1, 2"); StringAssert.Contains(shared[3].Reason, "not of each freedom case");
        CollectionAssert.AreEqual(new[] { Straus7CaseExclusionReason.None, Straus7CaseExclusionReason.ListedUnderSeveralFreedomCases, Straus7CaseExclusionReason.None,
            Straus7CaseExclusionReason.ListedUnderSeveralFreedomCases, Straus7CaseExclusionReason.None }, shared.Select(c => c.ReasonCode).ToArray(), "No freedom case data: the listing decides.");

        // Not a pair of the model: another result case number, a load case name or number of no Model load case.
        var wrong = Straus7LinearStaticResults.Cases(model, new[] { Result(1, "7: [1: G1] [1: Freedom Case 1]"), Result(2, "2: [1: G2] [1: Freedom Case 1]"),
            Result(3, "3: [9: G1] [1: Freedom Case 1]"), Result(4, "4: [1: G1] [x: Freedom Case 1]") });
        Assert.IsTrue(wrong.All(c => c.ModelCase == null && c.FreedomCase == null && c.Reason != null));
        Assert.IsTrue(wrong.All(c => c.ReasonCode == Straus7CaseExclusionReason.NoMatchingLoadCase));
        Assert.IsTrue(cases.All(c => c.ReasonCode == Straus7CaseExclusionReason.None));

        // One freedom case: the names and the map of R31 single-freedom-case solutions are unchanged.
        var single = Straus7LinearStaticResults.Cases(model, new[] { Result(1, "1: G1"), Result(2, "4: W") });
        CollectionAssert.AreEqual(new[] { "Case 1: G1", "Case 4: W" }, single.Select(c => c.ModelCase).ToArray());
        Assert.IsTrue(single.All(c => c.FreedomCase == null)); CollectionAssert.AreEqual(new[] { "1: G1", "4: W" }, single.Select(c => c.Label).ToArray());
    }

    /// <summary>Scenario load cases 1 Loads, 2 Gravity, 3 Seismic; two freedom cases with the same name; settings: 1 under FC 1, 2 under FC 2,
    /// 3 under both. Combination factors row-major [load case - 1, freedom case - 1].</summary>
    private static ExtendedApi FreedomScenario()
    {
        var api = new ExtendedApi(); api.Inner.FreedomCases = 2; api.FreedomNames = new[] { "Freedom Case 1", "Freedom Case 1" };
        api.Pairs.UnionWith(new[] { (1, 1), (2, 2), (3, 1), (3, 2) });
        api.Combinations.Add(new Straus7Combination { Number = 1, Name = "FC1", Enabled = true, Factors = new[] { 1.35, 0, 0, 0, 0, 0 } });
        api.Combinations.Add(new Straus7Combination { Number = 2, Name = "MIX", Enabled = true, Factors = new[] { 1.0, 0, 0, 1.5, 0, 0 } });
        api.Combinations.Add(new Straus7Combination { Number = 3, Name = "UNSOLVED", Enabled = true, Factors = new[] { 0, 0, 1.0, 0, 0, 0 } });
        api.Combinations.Add(new Straus7Combination { Number = 4, Name = "SHARED", Enabled = false, Factors = new[] { 1.0, 0, 0, 0, 0, 1.0 } });
        return api;
    }

    [TestMethod]
    public void SeveralFreedomCases_PairsAreModelData_CombinationsFollowThem_RestraintsStayPreserved()
    {
        var report = Import(FreedomScenario()); Partial(report); var m = report.Model;
        var data = Straus7FreedomCaseData.Read(m)!;
        CollectionAssert.AreEqual(new[] { 1, 2 }, data.FreedomCases.Select(f => f.Number).ToArray());
        Assert.IsTrue(data.FreedomCases.All(f => f.Name == "Freedom Case 1"));
        CollectionAssert.AreEqual(new[] { "1/1/Case 1: Loads", "2/2/Case 2: Gravity", "3/1/Case 3: Seismic", "3/2/Case 3: Seismic" },
            data.LinearCases.Select(p => p.LoadCase + "/" + p.FreedomCase + "/" + p.ModelCase).ToArray());
        var info = report.Diagnostics.Single(d => d.Code == "Straus7FreedomCases"); Assert.AreEqual(DiagnosticSeverity.Information, info.Severity);
        StringAssert.Contains(info.Message, "FC 2 'Freedom Case 1' (load cases 2, 3)"); StringAssert.Contains(info.Message, "Load case 3 is solved under several freedom cases");

        var definitions = CombinationExpansion.Definitions(m).ToDictionary(d => d.Name);
        CollectionAssert.AreEquivalent(new[] { "FC1", "MIX" }, definitions.Keys.ToArray());
        Assert.AreEqual("Case 1: Loads", definitions["FC1"].Terms.Single().Name); Assert.AreEqual(1.35, definitions["FC1"].Terms.Single().Factor);
        CollectionAssert.AreEqual(new[] { "Case 1: Loads:1", "Case 2: Gravity:1.5" }, definitions["MIX"].Terms.Select(t => t.Name + ":" + t.Factor.ToString(System.Globalization.CultureInfo.InvariantCulture)).ToArray(),
            "Load case 1 under FC 1 plus load case 2 under FC 2, as Straus7 adds the two result cases.");
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "Straus7CombinationAcrossFreedomCases" && d.Severity == DiagnosticSeverity.Information && d.Record == "LSACombination/2"));
        var unsolved = report.Diagnostics.Single(d => d.Code == "Straus7CombinationUnsolvedPairPreserved");
        Assert.AreEqual("LSACombination/3", unsolved.Record); StringAssert.Contains(unsolved.Message, "2 under FC 1");
        var shared = report.Diagnostics.Single(d => d.Code == "Straus7CombinationSharedCasePreserved");
        Assert.AreEqual("LSACombination/4", shared.Record); StringAssert.Contains(shared.Message, "(3)");
        Assert.IsFalse(report.Diagnostics.Any(d => d.Code == "Straus7CombinationFreedomCasesPreserved"), "With the pairs each combination has its own reason.");
        Assert.AreEqual(4, report.Preserved.Count(p => p.SourceRecord.StartsWith("LSACombination/")), "Every native combination stays preserved.");

        // Restraints and node springs of several freedom cases: not in GPC.Model (validation and ground springs know no freedom case).
        Assert.AreEqual(0, m.NodesElements.Values.Sum(n => n.Assignments.Restrains.Count));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "Straus7FreedomCasesPreserved"));
        Assert.AreEqual(0, m.FreedomCases.Count);
    }

    [TestMethod]
    public void SeveralFreedomCases_ResultCasesFollowTheImportedPairs()
    {
        var m = Import(FreedomScenario()).Model;
        var cases = Straus7LinearStaticResults.Cases(m, new[] { Result(1, "1: [1: Loads] [1: Freedom Case 1]"), Result(2, "2: [3: Seismic] [1: Freedom Case 1]"),
            Result(3, "3: [2: Gravity] [2: Freedom Case 1]"), Result(4, "4: [3: Seismic] [2: Freedom Case 1]") });
        CollectionAssert.AreEqual(new[] { "Case 1: Loads", null, "Case 2: Gravity", null }, cases.Select(c => c.ModelCase).ToArray());
        StringAssert.Contains(cases[1].Reason, "under FC 1, FC 2");
        // A subset of the listing does not hide the other pair: the settings of the imported model decide.
        Assert.AreEqual(0, Straus7LinearStaticResults.CaseMap(m, new[] { Result(4, "4: [3: Seismic] [2: Freedom Case 1]") }).Count);
        // A pair the settings do not solve, or a freedom case renamed since the import.
        var other = Straus7LinearStaticResults.Cases(m, new[] { Result(1, "1: [1: Loads] [2: Freedom Case 1]"), Result(2, "2: [1: Loads] [1: Supports]"),
            Result(3, "3: [1: Loads] [5: Freedom Case 1]") });
        Assert.IsTrue(other.All(c => c.ModelCase == null));
        StringAssert.Contains(other[0].Reason, "under FC 1, not under FC 2"); StringAssert.Contains(other[1].Reason, "named 'Freedom Case 1'");
        StringAssert.Contains(other[2].Reason, "no freedom case 5");
        CollectionAssert.AreEqual(new[] { Straus7CaseExclusionReason.None, Straus7CaseExclusionReason.SolvedUnderSeveralFreedomCases, Straus7CaseExclusionReason.None,
            Straus7CaseExclusionReason.SolvedUnderSeveralFreedomCases }, cases.Select(c => c.ReasonCode).ToArray());
        CollectionAssert.AreEqual(new[] { Straus7CaseExclusionReason.NotSolvedUnderFreedomCase, Straus7CaseExclusionReason.FreedomCaseRenamed,
            Straus7CaseExclusionReason.FreedomCaseMissing }, other.Select(c => c.ReasonCode).ToArray());
        // The parameters of each reason are data of the case and of the import, not only words of the text.
        var data = Straus7FreedomCaseData.Read(m)!;
        Assert.AreEqual(1, other[0].LoadCase); Assert.AreEqual(2, other[0].FreedomCase); CollectionAssert.AreEqual(new[] { 1 }, data.FreedomCasesOf(1).ToArray());
        Assert.AreEqual("Supports", other[1].FreedomCaseName); Assert.AreEqual("Freedom Case 1", data.FreedomCases.Single(f => f.Number == other[1].FreedomCase).Name);
        Assert.AreEqual(5, other[2].FreedomCase); Assert.IsFalse(data.FreedomCases.Any(f => f.Number == 5));
        CollectionAssert.AreEqual(new[] { 1, 2 }, data.FreedomCasesOf(cases[1].LoadCase!.Value).ToArray());
    }

    /// <summary>17/11/2026 (GPC.Converter 2.25): the code of a reason is a contract of callers (ANTHEA translates the reasons by code, not by
    /// text): each value keeps its number and its name, None is exactly a mapped case, and every other value comes with its English text.</summary>
    [TestMethod]
    public void ExclusionReasonCodes_AreStableAndMatchTheMapping()
    {
        CollectionAssert.AreEqual(new[] { "None=0", "NoMatchingLoadCase=1", "FreedomCaseMissing=2", "FreedomCaseRenamed=3", "NotSolvedUnderFreedomCase=4",
            "SolvedUnderSeveralFreedomCases=5", "ListedUnderSeveralFreedomCases=6" },
            Enum.GetValues(typeof(Straus7CaseExclusionReason)).Cast<Straus7CaseExclusionReason>().Select(v => v + "=" + (int)v).ToArray());
        var m = Import(FreedomScenario()).Model;
        var listed = new[] { Result(1, "1: [1: Loads] [1: Freedom Case 1]"), Result(2, "2: [3: Seismic] [1: Freedom Case 1]"), Result(3, "3: [2: Gravity] [2: Freedom Case 1]"),
            Result(4, "4: [1: Loads] [2: Freedom Case 1]"), Result(5, "5: [1: Loads] [1: Supports]"), Result(6, "6: [1: Loads] [7: Freedom Case 1]"), Result(7, "7: Wind") };
        var withData = Straus7LinearStaticResults.Cases(m, listed);
        var noData = Straus7LinearStaticResults.Cases(CaseModel("G1", "Q"), new[] { Result(1, "1: [1: G1] [1: Freedom Case 1]"), Result(2, "2: [1: G1] [2: Freedom Case 1]"),
            Result(3, "3: [2: Q] [2: Freedom Case 1]") });
        CollectionAssert.AreEqual(new[] { Straus7CaseExclusionReason.None, Straus7CaseExclusionReason.SolvedUnderSeveralFreedomCases, Straus7CaseExclusionReason.None,
            Straus7CaseExclusionReason.NotSolvedUnderFreedomCase, Straus7CaseExclusionReason.FreedomCaseRenamed, Straus7CaseExclusionReason.FreedomCaseMissing,
            Straus7CaseExclusionReason.NoMatchingLoadCase }, withData.Select(c => c.ReasonCode).ToArray());
        CollectionAssert.AreEqual(new[] { Straus7CaseExclusionReason.ListedUnderSeveralFreedomCases, Straus7CaseExclusionReason.ListedUnderSeveralFreedomCases,
            Straus7CaseExclusionReason.None }, noData.Select(c => c.ReasonCode).ToArray());
        foreach (var c in withData.Concat(noData))
        {
            Assert.AreEqual(c.ModelCase != null, c.ReasonCode == Straus7CaseExclusionReason.None, c.Name);
            Assert.AreEqual(c.ReasonCode == Straus7CaseExclusionReason.None, c.Reason == null, c.Name);
        }
        // One freedom case: every case maps, with no reason (the codes add nothing to the models of one freedom case).
        Assert.IsTrue(Straus7LinearStaticResults.Cases(CaseModel("G1", "Q"), new[] { Result(1, "1: G1"), Result(2, "2: Q") }).All(c => c.ReasonCode == Straus7CaseExclusionReason.None && c.Reason == null));
    }

    /// <summary>Two LSA combinations with one name, one per freedom case (as "SLE_QP" in a model of the user): two Model combinations with the label of their
    /// freedom case, each resolved by its own id. A third with the same name across freedom cases keeps its id; the Model loses none.</summary>
    [TestMethod]
    public void SeveralFreedomCases_CombinationsWithOneNameBecomeDistinctModelCombinations()
    {
        var api = FreedomScenario(); api.Combinations.Clear();
        api.Combinations.Add(new Straus7Combination { Number = 1, Name = "SLE_QP", Enabled = true, Factors = new[] { 1.0, 0, 0, 0, 0, 0 } });
        api.Combinations.Add(new Straus7Combination { Number = 2, Name = "SLE_QP", Enabled = true, Factors = new[] { 0, 0, 0, 0.6, 0, 0 } });
        api.Combinations.Add(new Straus7Combination { Number = 3, Name = "SLE_QP", Enabled = true, Factors = new[] { 1.0, 0, 0, 1.0, 0, 0 } });
        api.Combinations.Add(new Straus7Combination { Number = 4, Name = "ULS", Enabled = true, Factors = new[] { 1.35, 0, 0, 0, 0, 0 } });
        var report = Import(api); Partial(report); var m = report.Model;
        CollectionAssert.AreEquivalent(new[] { "SLE_QP · FC 1", "SLE_QP · FC 2", "SLE_QP [LSA/3]", "ULS" }, m.Combinations.Keys.ToArray());
        CollectionAssert.AreEqual(new[] { "SLE_QP · FC 1", "SLE_QP · FC 2", "SLE_QP [LSA/3]", "ULS" }, CombinationExpansion.Definitions(m).Select(d => d.Name).ToArray());
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "Straus7CombinationNamesQualified" && d.Message.Contains("as 'SLE_QP · FC 2'")));
        Assert.IsFalse(report.Diagnostics.Any(d => d.Code == "CombinationNameRepeated"));
        var first = CombinationResults.Resolve(m, "LSA/1"); var second = CombinationResults.Resolve(m, "LSA/2");
        Assert.AreEqual("SLE_QP · FC 1", first.Output.Name); Assert.AreEqual("SLE_QP · FC 2", second.Output.Name);
        Assert.AreEqual(1.0, first.Alternatives.Single().Factors["Case 1: Loads"]); Assert.AreEqual(0.6, second.Alternatives.Single().Factors["Case 2: Gravity"]);
        // One freedom case: the names stay as before; the Model skips the second with a warning instead of in silence.
        var single = new ExtendedApi();
        single.Combinations.Add(new Straus7Combination { Number = 1, Name = "ULS", Enabled = true, Factors = new[] { 1.35, 0, 0 } });
        single.Combinations.Add(new Straus7Combination { Number = 2, Name = "ULS", Enabled = true, Factors = new[] { 0, 1.5, 0 } });
        var one = Import(single); Partial(one);
        CollectionAssert.AreEqual(new[] { "ULS", "ULS" }, CombinationExpansion.Definitions(one.Model).Select(d => d.Name).ToArray());
        Assert.AreEqual(1, one.Model.Combinations.Count);
        Assert.AreEqual("LSACombination/2", one.Diagnostics.Single(d => d.Code == "CombinationNameRepeated").Record);
    }

    [TestMethod]
    public void OneFreedomCase_NoFreedomCaseDataAndUnchangedCombinations()
    {
        var api = new ExtendedApi(); api.Pairs.Add((1, 1));
        api.Combinations.Add(new Straus7Combination { Number = 1, Name = "ULS", Enabled = true, Factors = new[] { 1.35, 1.0, 0 } });
        var report = Import(api); Partial(report);
        Assert.IsNull(Straus7FreedomCaseData.Read(report.Model));
        Assert.IsFalse(report.Preserved.Any(p => p.Kind == Straus7FreedomCaseData.Kind)); Assert.IsFalse(report.Diagnostics.Any(d => d.Code.StartsWith("Straus7FreedomCase")));
        CollectionAssert.AreEqual(new[] { "Case 1: Loads", "Case 2: Gravity" }, CombinationExpansion.Definitions(report.Model).Single().Terms.Select(t => t.Name).ToArray());
    }
}
