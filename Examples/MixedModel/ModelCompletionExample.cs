using GPC.Model.Models;
using GPC.Examples;
using GPC.Model.Combinations;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using GPC.Model.Checking.Preparation;
using GPC.Model.Compatibility;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Results.Processing;
using GPC.Model.Results.Queries;
using GPC.Model.Results.State;

internal static class ModelCompletionExample
{
    public static int Run()
    {
        var combination = new Combination("2P+ - 0.5P-");
        var model = MixedModelFactory.Create(m =>
        {
            combination.AddLoadCaseCoefficient(m.LoadCases["P+"], 2);
            combination.AddLoadCaseCoefficient(m.LoadCases["P-"], -.5);
            m.Combinations.Add(combination);
        });
        Console.WriteLine("Model-only synthetic demonstration: assigned analytical data, no FEM solver or Checker.");
        var owners = new Element[] { model.NodesElements[10], model.BeamElements[250], model.AreaElements[1090] };
        foreach (var owner in owners)
        {
            var key = new ElementKey { Family = GPC.Model.Models.Model.FamilyOf(owner), Id = owner.Id };
            var source = owner.Results.SelectMany(r => r.Results).First(r => r.Case.Name == "P+");
            var derived = ResultAlgebra.LinearCombination(model, key, combination, source);
            if (!derived.IsAvailable) { Console.Error.WriteLine(string.Join("; ", derived.Diagnostics.Select(d => d.Message))); return 1; }
            ResultAlgebra.Attach(derived);
            var local = ResultPreparation.Prepare(model, key, derived.Sample);
            Console.WriteLine($"{key.Family} {key.Id}: combination={derived.Sample.Case.Name}; local input={local.Status}; current={local.IsCurrent}");
            if (local.Status != DataStatus.Ready) return 1;
        }
        var beam = model.BeamElements[250]; var beamKey = new ElementKey { Family = EntityFamily.Beam, Id = beam.Id };
        var states = new ResultLocation[] { Verification.BeamSample(beam, "synthetic-static", "P+", 0, SectionSide.Unspecified), Verification.BeamSample(beam, "synthetic-static", "P-", 0, SectionSide.Unspecified) };
        var envelope = ResultAlgebra.Envelope(model, beamKey, states);
        Console.WriteLine($"Envelope: max V2 from {envelope.MaximumSources[2].Case.Name}; max M1 from {envelope.MaximumSources[4].Case.Name}; complete concomitant states preserved.");
        var applied = Equilibrium.PointLoad(model.NodesElements[40].Loads.Values.OfType<PointLoad>().First(l => l.LoadCaseName == "P+"), "tip");
        var reactionSample = model.NodesElements[10].Results.SelectMany(r => r.Results).OfType<NodeResultForces>().First(r => r.Case.Name == "P+");
        var reaction = Equilibrium.NodalAction(model, 10, reactionSample, ActionBody.OnNode, "support", "total-reaction");
        var scope = new EquilibriumScope { Selection = new ResultSelection { Dataset = "synthetic-static", Case = "P+", ConcomitantState = "P+" },
            CompleteCoverageConfirmed = true, CoverageEvidence = "Analytical cantilever free body only: tip load and support reaction. Assigned shell demonstration forces are outside this balance." };
        scope.ExpectedPhysicalActions.AddRange(new[] { "tip", "support" });
        var balance = Equilibrium.Check(scope, new[] { applied, reaction });
        Console.WriteLine($"Analytical cantilever equilibrium: {balance.Status}; balanced={balance.IsBalanced}");
        using var archive = new MemoryStream(); ModelArchive.Save(model, archive); archive.Position = 0; var restored = ModelArchive.Load(archive);
        var combined = restored.BeamElements[250].Results.SelectMany(r => r.Results).Single(r => r.Case.Name == combination.Name);
        var prepared = ResultPreparation.Prepare(restored, beamKey, combined);
        Console.WriteLine($"Archive: revision preserved={model.AnalysisFingerprint() == restored.AnalysisFingerprint()}; derived input after reopening={prepared.Status}");
        return balance.IsBalanced == true && prepared.Status == DataStatus.Ready && envelope.IsCurrent ? 0 : 1;
    }
}
