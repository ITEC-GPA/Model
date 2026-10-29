using System.IO.Compression;
using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Elements;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Processing;
using GPC.Model.Results.State;
using GPC.Model.Stages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>Opt-in native tests (installed and licensed Straus7 R3 x64) on the versioned fixture of generate_native_stages_fixture.py:
/// stages, masses, temperatures, reactions and equilibrium, results of an active stage and of a staged nonlinear solution.
/// GPC_STRAUS7_STAGES = "versioned" (the zip in Fixtures/Straus7) or a folder with the four files. Every source is copied into a new folder.</summary>
[TestClass]
public class Straus7NativeStagesTest
{
    private static string Folder()
    {
        var value = Environment.GetEnvironmentVariable("GPC_STRAUS7_STAGES");
        if (string.IsNullOrWhiteSpace(value)) Assert.Inconclusive("Set GPC_STRAUS7_STAGES (see POST_PROCESSING.md, Straus7 native tests).");
        var folder = Path.Combine(Path.GetTempPath(), "gpc-straus7-stages-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        if (value == "versioned") ZipFile.ExtractToDirectory(Path.Combine(AppContext.BaseDirectory, "Fixtures", "Straus7", "native-stages-r31.zip"), folder);
        else foreach (var file in new[] { "stages.st7", "stages.LSA", "stages-stage1.LSA", "stages.NLA" }) File.Copy(Path.Combine(value!, file), Path.Combine(folder, file));
        return folder;
    }
    private static void Print(IEnumerable<ModelDiagnostic> diagnostics) { foreach (var d in diagnostics) Console.WriteLine(d.Severity + " " + d.Code + " [" + d.Record + "] " + d.Message); }
    private static ImportReport Import(Straus7ApiConverter converter, string folder)
    {
        var geometry = converter.Import(new Straus7ImportRequest { ModelPath = Path.Combine(folder, "stages.st7"), ModelRevision = "native-stages-v1", AnalysisId = "stages" });
        Print(geometry.Diagnostics); Assert.IsNotNull(geometry.Model);
        return geometry;
    }
    private static Straus7ResultsReport Read(Straus7ApiConverter converter, string folder, string results) => converter.ReadResults(new Straus7ResultsRequest
    {
        ModelPath = Path.Combine(folder, "stages.st7"), ResultPath = Path.Combine(folder, results), CaseNumbers = new[] { 1, 2, 3 }, BeamNumbers = new[] { 1, 2, 3 },
        PlateNumbers = new[] { 1 }, NodeNumbers = Enumerable.Range(1, 8).ToArray(), MinimumBeamStations = 3, IncludePlateNodeResults = true
    });

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeStages_DefinitionsMassesAndTemperaturesAreModelData()
    {
        var folder = Folder(); var converter = new Straus7ApiConverter(); var m = Import(converter, folder).Model;
        var stages = m.Stages.Values.OrderBy(s => s.Definition.Sequence).ToArray();
        CollectionAssert.AreEqual(new[] { "Stage 1: Deck only", "Stage 2: Complete" }, stages.Select(s => s.Name).ToArray());
        CollectionAssert.AreEqual(new[] { "Group 1: Model", @"Group 2: Model\Deck" }, stages[0].Definition.GroupChanges.Select(c => c.GroupName).ToArray());
        Assert.AreEqual(@"Group 3: Model\Span 2", stages[1].Definition.GroupChanges.Single().GroupName);
        var data = Straus7ModelData.Read(m);
        CollectionAssert.AreEqual(new[] { 1 }, data.Stages[0].ActiveBeams); CollectionAssert.AreEqual(new[] { 1, 2, 3 }, data.Stages[1].ActiveBeams);
        CollectionAssert.AreEqual(new[] { 1 }, data.Stages[0].ActivePlates); Assert.AreEqual(Straus7StageType.Unmorphed, data.Stages[1].Type);
        Assert.AreEqual(5, data.Masses.Count);
        CollectionAssert.AreEqual(new[] { .5, .5, .5 }, data.Masses.Single(x => x.Kind == Straus7MassKind.NodeTranslational).Values);
        CollectionAssert.AreEqual(new[] { 10d, 20, 30 }, data.Masses.Single(x => x.Kind == Straus7MassKind.NodeRotational).Values);
        Assert.AreEqual(100, data.Masses.Single(x => x.Kind == Straus7MassKind.NodeNonStructural).Offset[2]);
        Assert.AreEqual(1e-4, data.Masses.Single(x => x.Kind == Straus7MassKind.BeamNonStructural).Values[0], 1e-15);
        Assert.AreEqual(2e-7, data.Masses.Single(x => x.Kind == Straus7MassKind.PlateNonStructural).Values[0], 1e-20);
        Assert.AreEqual(10, data.Temperatures.Single(x => x.Kind == Straus7TemperatureKind.ReferenceTemperature).Values[0]);
        Assert.AreEqual(3, data.Temperatures.Count(x => x.Kind == Straus7TemperatureKind.NodeTemperature && x.Values[0] == 40));
        CollectionAssert.AreEqual(new[] { .01, 0 }, data.Temperatures.Single(x => x.Kind == Straus7TemperatureKind.BeamGradient).Values);
        Assert.AreEqual(.02, data.Temperatures.Single(x => x.Kind == Straus7TemperatureKind.PlateGradient).Values[0], 1e-15);
        var inertia = data.Inertia.Single(); Assert.IsTrue(inertia.StructuralMass && inertia.NonStructuralMass);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeStages_UnstagedSolutionBalancesLoadsMassesAndReactions()
    {
        var folder = Folder(); var converter = new Straus7ApiConverter(); var m = Import(converter, folder).Model;
        var native = Read(converter, folder, "stages.LSA"); Print(native.Diagnostics);
        Assert.AreEqual(ImportStatus.Completed, native.Status); Assert.AreEqual(2, native.ModelStageCount);
        var imported = Straus7LinearStaticResults.Import(m, native, "lsa"); Print(imported.Diagnostics);
        Assert.IsTrue(imported.Status is ImportStatus.Completed or ImportStatus.Partial, "A model with stages solved without an active stage is an unstaged linear static solution.");
        foreach (var (name, z) in new[] { ("Dead", -34846.6), ("Live", -1000), ("Thermal", 0) })
        {
            var c = m.LoadCases.Keys.Single(k => k.EndsWith(": " + name));
            var balance = Straus7Equilibrium.Check(m, c, "lsa"); Print(balance.Diagnostics);
            Console.WriteLine($"{c}: applied {string.Join(", ", balance.AppliedForce.Select(v => v.ToString("F3")))} reactions {string.Join(", ", balance.ReactionForce.Select(v => v.ToString("F3")))}");
            Assert.AreEqual(true, balance.Balanced, c + " balanced");
            Assert.AreEqual(z, balance.AppliedForce[2], 0.1, c + " applied Z (hand check of the weight in the PROVENANCE)");
            Assert.AreEqual(-z, balance.ReactionForce[2], 0.1);
            Assert.AreEqual(0, balance.NotRepresented.Count);
        }
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeStages_ActiveStageResultsSkipTheInactiveElements()
    {
        var folder = Folder(); var converter = new Straus7ApiConverter(); var m = Import(converter, folder).Model;
        var native = Read(converter, folder, "stages-stage1.LSA"); Print(native.Diagnostics);
        Assert.AreEqual(ImportStatus.Completed, native.Status);
        CollectionAssert.AreEqual(new[] { 2, 2, 2 }, new[] { 1, 2, 3 }.Select(c => native.InactiveElements[c]).ToArray(), "Beam 2 and truss 3 are inactive in stage 1.");
        var imported = Straus7LinearStaticResults.Import(m, native, "stage1"); Print(imported.Diagnostics);
        Assert.IsTrue(imported.Status is ImportStatus.Completed or ImportStatus.Partial);
        Assert.IsTrue(imported.Diagnostics.Any(d => d.Code == "Straus7ActiveStageResults"));
        BeamElement Beam(string id) => m.BeamElements.Values.Single(b => b.Source.OriginalId == id);
        Assert.AreEqual(0, Beam("2").Results.SelectMany(r => r.Results).Count(), "No result for an inactive beam.");
        var stations = Beam("1").Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().ToArray();
        Assert.IsTrue(stations.Length > 0 && stations.All(s => s.State.Phase == "Stage 1: Deck only"));
        var node3 = m.NodesElements.Values.Single(n => n.Source.OriginalId == "3");
        Assert.AreEqual(0, node3.Results.SelectMany(r => r.Results).Count(), "Node 3 belongs only to inactive elements: no zero result.");
        var dead = m.LoadCases.Keys.Single(k => k.EndsWith(": Dead"));
        Assert.IsNull(Straus7Equilibrium.Check(m, dead, "stage1").Balanced, "Staged states are not checked.");
        // Combination of the active stage: the linear static cases of that configuration combine.
        var report = CombinationRebuild.Rebuild(m, m.Combinations["ULS"], new[] { new CombinationAlternative("", new Dictionary<string, double> { [dead] = 1.35, [m.LoadCases.Keys.Single(k => k.EndsWith(": Live"))] = 1.5 }) },
            "stage1", m.AllElements, false);
        Print(report.Diagnostics); Assert.IsFalse(report.IsRejected); Assert.IsTrue(report.Samples.Count > 0);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeStages_StagedNonlinearStatesPerStageAndIncrement()
    {
        var folder = Folder(); var converter = new Straus7ApiConverter(); var m = Import(converter, folder).Model;
        var native = Read(converter, folder, "stages.NLA"); Print(native.Diagnostics);
        Assert.AreEqual(ImportStatus.Completed, native.Status); Assert.AreEqual(3, native.File.Solver);
        Assert.AreEqual(ImportStatus.Rejected, Straus7LinearStaticResults.Import(m, native, "wrong").Status, "Nonlinear results are not linear static cases.");
        var imported = Straus7StageResults.Import(m, native, "nla"); Print(imported.Diagnostics);
        Assert.IsTrue(imported.Status is ImportStatus.Completed or ImportStatus.Partial);
        var samples = m.AllElements.SelectMany(e => e.Results.SelectMany(r => r.Results)).Where(s => s.State?.DatasetId == "nla").ToArray();
        Assert.IsTrue(samples.All(s => s.Case is StageResultCase && s.Case.Name == Straus7StageResults.NonlinearCase && s.State.Semantics == GPC.Model.Analysis.AnalysisSemantics.NonlinearStatic));
        CollectionAssert.AreEquivalent(new[] { "Stage 1: Deck only/1: Dead", "Stage 2: Complete/2: Live half", "Stage 2: Complete/3: Live full" },
            samples.Select(s => s.State.Phase + "/" + s.State.Step).Distinct().ToArray());
        var beam2 = m.BeamElements.Values.Single(b => b.Source.OriginalId == "2").Results.SelectMany(r => r.Results).ToArray();
        Assert.IsTrue(beam2.Length > 0 && beam2.All(s => s.State.Phase == "Stage 2: Complete"), "Beam 2 is born in stage 2.");
        var reaction = m.NodesElements.Values.Single(n => n.Source.OriginalId == "4").Results.SelectMany(r => r.Results).OfType<NodeResultForces>()
            .Single(r => r.State.Step == "2: Live half" && r.Kind == NodalForceKind.SupportReaction);
        Assert.AreEqual(8272.0, reaction.Fz, 0.5, "Truss support: Dead + 0.5 Live (unmorphed stage, as the linear solution 7775.2 + 0.5 x 993.6).");
    }
}
