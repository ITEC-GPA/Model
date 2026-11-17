using System.IO.Compression;
using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Elements;
using GPC.Model.Results.Locations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>Opt-in native tests (installed and licensed Straus7 R3 x64) on the versioned fixture of generate_native_freedom_cases_fixture.py: two freedom
/// cases with the same name, load cases solved under one of them, LSA combinations within and across freedom cases, and solutions with a load case
/// under both freedom cases or with one freedom case only. GPC_STRAUS7_FREEDOM = "versioned" (the zip in Fixtures/Straus7) or a folder with the
/// four files. Every source is copied into a new folder.</summary>
[TestClass]
public class Straus7NativeFreedomCasesTest
{
    private static readonly string[] Files = { "freedom.st7", "freedom.LSA", "freedom-shared.LSA", "freedom-fc1.LSA" };
    private static string Folder()
    {
        var value = Environment.GetEnvironmentVariable("GPC_STRAUS7_FREEDOM");
        if (string.IsNullOrWhiteSpace(value)) Assert.Inconclusive("Set GPC_STRAUS7_FREEDOM (see POST_PROCESSING.md, Straus7 native tests).");
        var folder = Path.Combine(Path.GetTempPath(), "gpc-straus7-freedom-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        if (value == "versioned") ZipFile.ExtractToDirectory(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Straus7", "native-freedom-cases-r31.zip"), folder);
        else foreach (var file in Files) File.Copy(Path.Combine(value!, file), Path.Combine(folder, file));
        return folder;
    }
    private static void Print(IEnumerable<ModelDiagnostic> diagnostics) { foreach (var d in diagnostics) Console.WriteLine(d.Severity + " " + d.Code + " [" + d.Record + "] " + d.Message); }
    private static ImportReport Import(Straus7ApiConverter converter, string folder)
    {
        var geometry = converter.Import(new Straus7ImportRequest { ModelPath = Path.Combine(folder, "freedom.st7"), ModelRevision = "native-freedom-v1", AnalysisId = "freedom" });
        Print(geometry.Diagnostics); Assert.IsNotNull(geometry.Model);
        return geometry;
    }
    private static Straus7ResultsReport Read(Straus7ApiConverter converter, string folder, string results, int[] cases, bool combinations = false) => converter.ReadResults(new Straus7ResultsRequest
    {
        ModelPath = Path.Combine(folder, "freedom.st7"), ResultPath = Path.Combine(folder, results), CaseNumbers = cases, BeamNumbers = new[] { 1, 2 }, PlateNumbers = new[] { 1 },
        NodeNumbers = Enumerable.Range(1, 7).ToArray(), MinimumBeamStations = 3, IncludePlateNodeResults = true, IncludeCombinations = combinations
    });
    private static IReadOnlyList<Straus7LinearStaticCase> Listing(Straus7ApiConverter converter, string folder, string results, GPC.Model.Models.Model m)
    {
        var listing = converter.ReadResults(new Straus7ResultsRequest { ModelPath = Path.Combine(folder, "freedom.st7"), ResultPath = Path.Combine(folder, results) });
        Assert.AreEqual(ImportStatus.Completed, listing.Status);
        var cases = Straus7LinearStaticResults.Cases(m, listing.Cases);
        foreach (var c in cases) Console.WriteLine($"{results} {c.Number} '{c.Name}' -> {c.ModelCase ?? "-"} [{c.Label}] {c.ReasonCode} {c.Reason}");
        return cases;
    }
    private static NodeElement Node(GPC.Model.Models.Model m, string id) => m.NodesElements.Values.Single(n => n.Source.OriginalId == id);
    private static T Sample<T>(NodeElement node, string dataset, string name) where T : ResultLocation =>
        node.Results.SelectMany(r => r.Results).OfType<T>().Single(s => s.Case?.Name == name && s.State?.DatasetId == dataset);

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeFreedomCases_PairsAndCombinationsAreImported_SupportsKeepTheirFreedomCase()
    {
        var folder = Folder(); var report = Import(new Straus7ApiConverter(), folder); var m = report.Model;
        Assert.AreEqual(7, m.NodesElements.Count); Assert.AreEqual(2, m.BeamElements.Count); Assert.AreEqual(1, m.AreaElements.Count);
        CollectionAssert.AreEquivalent(new[] { "Case 1: G1", "Case 2: Q", "Case 3: G1", "Case 4: W" }, m.LoadCases.Keys.ToArray());
        var data = Straus7FreedomCaseData.Read(m)!;
        CollectionAssert.AreEqual(new[] { "1 Freedom Case 1", "2 Freedom Case 1" }, data.FreedomCases.Select(f => f.Number + " " + f.Name).ToArray(), "Two freedom cases with the same name.");
        CollectionAssert.AreEqual(new[] { "1/1", "2/2", "3/2", "4/1" }, data.LinearCases.Select(p => p.LoadCase + "/" + p.FreedomCase).ToArray());
        // GPC.Converter 2.27 (18/11/2026; until 2.26 preserved with Straus7FreedomCasesPreserved): the supports of the generator with their
        // freedom case. FC 1: node 1 DX DY DZ RX, node 3 DY DZ, nodes 4 and 7 fixed, the spring kz = 200 N/mm of node 2. FC 2: node 1 DX DY DZ RX,
        // nodes 2 and 3 DY DZ, nodes 4 and 7 fixed. Each load case is tied to the freedom case that solves it.
        Assert.IsFalse(report.Diagnostics.Any(d => d.Code == "Straus7FreedomCasesPreserved"));
        var fc1 = m.FreedomCases["Freedom Case 1 · FC 1"]; var fc2 = m.FreedomCases["Freedom Case 1 · FC 2"]; Assert.AreEqual(2, m.FreedomCases.Count);
        string Dofs(string node, GPC.Model.FreedomCases.FreedomCase fc) => string.Join(" ", Node(m, node).Assignments.RestrainsOf(fc).SelectMany(r => r.Restrain.Restrains).Select(d => d.Dof));
        CollectionAssert.AreEqual(new[] { "DX DY DZ RX", "", "DY DZ", "DX DY DZ RX RY RZ", "", "", "DX DY DZ RX RY RZ" }, Enumerable.Range(1, 7).Select(n => Dofs(n.ToString(), fc1)).ToArray());
        CollectionAssert.AreEqual(new[] { "DX DY DZ RX", "DY DZ", "DY DZ", "DX DY DZ RX RY RZ", "", "", "DX DY DZ RX RY RZ" }, Enumerable.Range(1, 7).Select(n => Dofs(n.ToString(), fc2)).ToArray());
        var spring = Node(m, "2").Assignments.GroundSprings.Single(); Assert.AreSame(fc1, spring.FreedomCase); Assert.AreEqual(200, spring[2, 2], 1e-9);
        Assert.AreEqual(1, m.NodesElements.Values.Sum(n => n.Assignments.GroundSprings.Count));
        CollectionAssert.AreEqual(new[] { fc1, fc2, fc2, fc1 }, new[] { "Case 1: G1", "Case 2: Q", "Case 3: G1", "Case 4: W" }.Select(c => m.LoadCases[c].FreedomCase).ToArray());
        Assert.IsFalse(GPC.Model.Structure.Assignments.AssignmentValidation.ValidateAssignments(m).Any(d => d.Severity == DiagnosticSeverity.Error),
            string.Join("; ", GPC.Model.Structure.Assignments.AssignmentValidation.ValidateAssignments(m).Select(d => d.Code)));
        var definitions = CombinationExpansion.Definitions(m).ToDictionary(d => d.Name, d => string.Join(" + ", d.Terms.Select(t => t.Factor.ToString(System.Globalization.CultureInfo.InvariantCulture) + " " + t.Name)));
        Assert.AreEqual("1.35 Case 1: G1 + 1.5 Case 4: W", definitions["ULS1"]);
        Assert.AreEqual("1.5 Case 2: Q + 1.35 Case 3: G1", definitions["ULS2"]);
        Assert.AreEqual("1 Case 1: G1 + 1 Case 2: Q", definitions["MIX"]);
        Assert.IsFalse(definitions.ContainsKey("UNSOLVED"));
        Assert.AreEqual("LSACombination/4", report.Diagnostics.Single(d => d.Code == "Straus7CombinationUnsolvedPairPreserved").Record);
        Assert.AreEqual("LSACombination/3", report.Diagnostics.Single(d => d.Code == "Straus7CombinationAcrossFreedomCases").Record);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeFreedomCases_EachResultCaseIsItsLoadCase_AndBalancesTheReactionsOfItsFreedomCase()
    {
        var folder = Folder(); var converter = new Straus7ApiConverter(); var m = Import(converter, folder).Model;
        var cases = Listing(converter, folder, "freedom.LSA", m);
        CollectionAssert.AreEqual(new[] { "Case 1: G1", "Case 4: W", "Case 2: Q", "Case 3: G1" }, cases.Select(c => c.ModelCase).ToArray());
        CollectionAssert.AreEqual(new[] { "1: G1 · FC 1", "4: W · FC 1", "2: Q · FC 2", "3: G1 · FC 2" }, cases.Select(c => c.Label).ToArray());
        var native = Read(converter, folder, "freedom.LSA", new[] { 1, 2, 3, 4 }); Print(native.Diagnostics);
        Assert.AreEqual(ImportStatus.Completed, native.Status);
        var imported = Straus7LinearStaticResults.Import(m, native, "lsa"); Print(imported.Diagnostics);
        Assert.AreEqual(ImportStatus.Completed, imported.Status);
        Assert.IsTrue(imported.Diagnostics.Any(d => d.Code == "Straus7FreedomCaseResults"));
        // Weight: 2 beams of 4000 mm x 10080 mm2 and a plate 1000 x 1000 x 200 mm, 7.85e-9 t/mm3, 9806.65 mm/s2.
        double weight = (2 * 4000 * 10080 + 1000 * 1000 * 200) * 7.85e-9 * 9806.65;
        foreach (var (name, z) in new[] { ("Case 1: G1", -weight), ("Case 4: W", -8500d), ("Case 2: Q", -10000d), ("Case 3: G1", -weight) })
        {
            var balance = Straus7Equilibrium.Check(m, name, "lsa"); Print(balance.Diagnostics);
            Console.WriteLine($"{name}: applied {string.Join(", ", balance.AppliedForce.Select(v => v.ToString("F3")))} reactions {string.Join(", ", balance.ReactionForce.Select(v => v.ToString("F3")))} ({balance.Reactions})");
            Assert.AreEqual(true, balance.Balanced, name);
            Assert.AreEqual(z, balance.AppliedForce[2], 0.5, name); Assert.AreEqual(-z, balance.ReactionForce[2], 0.5, name);
            Assert.AreEqual(0, balance.NotRepresented.Count, name);
            Straus7ApiWorkflowTest.SameAsNodalActions(m, name, "lsa", balance);
        }
        // Node 2: on the spring kz = 200 N/mm in freedom case 1 (reaction -k uz), restrained in freedom case 2 (no displacement).
        var node2 = Node(m, "2");
        double uz1 = Sample<NodeResultDisplacement>(node2, "lsa", "Case 1: G1").ResultDisplacement.D3;
        Assert.IsTrue(uz1 < -1, "Freedom case 1: the elastic support settles.");
        Assert.AreEqual(-200 * uz1, Sample<NodeResultForces>(node2, "lsa", "Case 1: G1").Fz, 1e-6 * Math.Abs(200 * uz1));
        Assert.AreEqual(0, Sample<NodeResultDisplacement>(node2, "lsa", "Case 3: G1").ResultDisplacement.D3, 1e-12, "Freedom case 2: node 2 restrained.");
        Assert.IsTrue(Sample<NodeResultForces>(node2, "lsa", "Case 3: G1").Fz > 1000, "Freedom case 2: the support at node 2 carries part of the weight.");
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeFreedomCases_StrausCombinationsAreTheSumsOfTheirPairs()
    {
        var folder = Folder(); var converter = new Straus7ApiConverter();
        var native = Read(converter, folder, "freedom.LSA", Enumerable.Range(1, 8).ToArray(), combinations: true); Print(native.Diagnostics);
        Assert.AreEqual(ImportStatus.Completed, native.Status); Assert.AreEqual(4, native.File.PrimaryCases); Assert.AreEqual(4, native.File.SecondaryCases);
        CollectionAssert.AreEqual(new[] { "5: [1: ULS1] [Combination]", "6: [2: ULS2] [Combination]", "7: [3: MIX] [Combination]", "8: [4: UNSOLVED] [Combination]" },
            native.Cases.Skip(4).Select(c => c.Name).ToArray());
        // Result cases: 1 = G1 under FC 1, 2 = W under FC 1, 3 = Q under FC 2, 4 = G1 under FC 2.
        var combinations = new Dictionary<int, (int Case, double Factor)[]>
        {
            [5] = new[] { (1, 1.35), (2, 1.5) }, [6] = new[] { (4, 1.35), (3, 1.5) }, [7] = new[] { (1, 1.0), (3, 1.0) }, [8] = Array.Empty<(int, double)>()
        };
        var tables = native.Tables.Where(t => t.Entity != Straus7Entity.Beam).ToLookup(t => (t.Entity, t.Number, t.Quantity));
        int compared = 0;
        foreach (var group in tables)
            foreach (var (combination, terms) in combinations)
            {
                var actual = group.Single(t => t.CaseNumber == combination).Values;
                var expected = new double[actual.Length];
                foreach (var (c, factor) in terms) { var v = group.Single(t => t.CaseNumber == c).Values; for (int i = 0; i < v.Length; i++) expected[i] += factor * v[i]; }
                double scale = Math.Max(1e-9, expected.Concat(actual).Max(Math.Abs));
                for (int i = 0; i < actual.Length; i++) Assert.AreEqual(expected[i], actual[i], 1e-9 * scale, $"{group.Key} combination {combination} [{i}]");
                compared++;
            }
        Console.WriteLine($"{compared} node and plate tables compared; UNSOLVED (a factor on load case 2 under FC 1, which is not solved) is zero in Straus7.");
        Assert.IsTrue(compared >= 4 * 16);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeFreedomCases_SharedLoadCase_AndOneFreedomCaseSolution()
    {
        var folder = Folder(); var converter = new Straus7ApiConverter(); var m = Import(converter, folder).Model;
        // Load case 2 also under FC 1: the settings of the imported model solve it under FC 2 only, so result case 2 is left out.
        var shared = Listing(converter, folder, "freedom-shared.LSA", m);
        CollectionAssert.AreEqual(new[] { "Case 1: G1", null, "Case 4: W", "Case 2: Q", "Case 3: G1" }, shared.Select(c => c.ModelCase).ToArray());
        StringAssert.Contains(shared[1].Reason, "not under FC 1");
        Assert.AreEqual(Straus7CaseExclusionReason.NotSolvedUnderFreedomCase, shared[1].ReasonCode);
        Assert.IsTrue(shared.Where(c => c.ModelCase != null).All(c => c.ReasonCode == Straus7CaseExclusionReason.None));
        var mapped = shared.Where(c => c.ModelCase != null).Select(c => c.Number).ToArray();
        var native = Read(converter, folder, "freedom-shared.LSA", mapped);
        var imported = Straus7LinearStaticResults.Import(m, native, "shared"); Print(imported.Diagnostics);
        Assert.AreEqual(ImportStatus.Completed, imported.Status);
        Assert.AreEqual(ImportStatus.Rejected, Straus7LinearStaticResults.Import(m, Read(converter, folder, "freedom-shared.LSA", new[] { 2 }), "unmapped").Status,
            "Result case 2 has no Model load case.");
        // The same pair in another solution gives the same results.
        var reference = Read(converter, folder, "freedom.LSA", new[] { 3 });
        foreach (var t in native.Tables.Where(t => t.CaseNumber == 4 && t.Entity == Straus7Entity.Node))
        {
            var r = reference.Tables.Single(x => x.Entity == t.Entity && x.Number == t.Number && x.Quantity == t.Quantity).Values;
            for (int i = 0; i < r.Length; i++) Assert.AreEqual(r[i], t.Values[i], 1e-9 * Math.Max(1, Math.Abs(r[i])));
        }
        // One freedom case in the solution: the names are "<load case>: <name>" and map as for a model with one freedom case.
        var single = Listing(converter, folder, "freedom-fc1.LSA", new Straus7ApiConverter().Import(new Straus7ImportRequest { ModelPath = Path.Combine(folder, "freedom.st7"),
            ModelRevision = "native-freedom-v1", AnalysisId = "fc1" }).Model);
        CollectionAssert.AreEqual(new[] { "1: G1", "4: W" }, single.Select(c => c.Name).ToArray());
        CollectionAssert.AreEqual(new[] { "Case 1: G1", "Case 4: W" }, single.Select(c => c.ModelCase).ToArray());
        Assert.IsTrue(single.All(c => c.FreedomCase == null));
    }
}
