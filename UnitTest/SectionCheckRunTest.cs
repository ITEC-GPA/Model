using GPC.Examples;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Execution;
using GPC.Model.Checking.Preparation;
using GPC.Model.Compatibility;
using GPC.Model.Core;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Queries;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>The section checks of a run make the steps of SectionCheckExecution.Run with the same answers; they read the whole model once
/// for all the checks, until code that may change it runs: a verifier that is not declared to read only its input (after each of its
/// calls) and the code of the caller that it declares (ModelMayHaveChanged).</summary>
[TestClass]
public class SectionCheckRunTest
{
    /// <summary>Same answer for every check; <see cref="During"/> runs inside the call, with the number of the call.</summary>
    private sealed class Verifier : IConfiguredSectionVerifier
    {
        internal int Calls;
        internal Action<int>? During;
        public string Version => "TEST ONLY";
        public string Configuration => "run";
        public IReadOnlyCollection<CheckMechanism> Capabilities => new[] { CheckMechanism.UlsBiaxialSection, CheckMechanism.Shear };
        public CheckResult Verify(BeamCheckInput input, CheckMechanism mechanism, CancellationToken token)
        {
            Calls++; During?.Invoke(Calls);
            return new CheckResult { Execution = ExecutionStatus.Completed, Data = DataStatus.Ready, Outcome = EngineeringOutcome.Satisfied, Utilization = .5 };
        }
    }

    private static readonly CheckMechanism[] Mechanisms = { CheckMechanism.UlsBiaxialSection, CheckMechanism.Shear };

    /// <summary>The five stations of the cantilever of case P+, prepared once.</summary>
    private static BeamPreparation[] Prepare(Model model) => ResultQueries.BeamSamples(model.BeamElements[250], "synthetic-static", "P+")
        .Select(s => Verification.PrepareBeam(model, 250, s, "run")).ToArray();

    /// <summary>Two checks per station (UlsBiaxialSection, Shear), in order, through <paramref name="check"/>.</summary>
    private static CheckResult[] Checks(BeamPreparation[] stations, Func<BeamPreparation, CheckMechanism, CheckResult> check)
        => stations.SelectMany(p => Mechanisms.Select(m => check(p, m))).ToArray();

    private static string[] Evidence(IEnumerable<CheckResult> results) => results.Select(r => string.Join(";", r.Execution, r.Data, r.Outcome,
        string.Join(",", r.Diagnostics.Select(d => d.Code)), r.EvidenceFingerprint ?? ModelValues.Fingerprint(new object[] { r.Provenance, r.Input, r.VerificationRevision }))).ToArray();

    private static List<string> Computed(Action call)
    {
        var keys = new List<string>(); ValidationReadScope.Computed = keys.Add;
        try { call(); }
        finally { ValidationReadScope.Computed = null; }
        return keys;
    }

    [TestMethod]
    public void ARunGivesTheAnswersOfTheChecksOneByOne()
    {
        var model = MixedModelFactory.Create(); var verifier = new Verifier(); var stations = Prepare(model);
        var alone = Checks(stations, (p, m) => SectionCheckExecution.Run(p, m, verifier));
        var run = new SectionCheckRun(model);
        var together = Checks(stations, (p, m) => run.Run(p, m, verifier, true));
        Assert.AreEqual(10, together.Length); Assert.IsTrue(together.All(r => r.Outcome == EngineeringOutcome.Satisfied && r.Provenance != null));
        CollectionAssert.AreEqual(Evidence(alone), Evidence(together));
        // Each row its own provenance, with the values of the one captured alone.
        Assert.AreNotSame(together[0].Provenance, together[9].Provenance);
        Assert.AreEqual(ModelValues.Fingerprint(new object[] { VerificationPreparation.CurrentProvenance(model) }), ModelValues.Fingerprint(new object[] { together[9].Provenance }));
        // Explicit discriminators: the same as SectionCheckExecution.Run with the specification.
        var shear = SectionCheckSpecification.ShearAxis2();
        CollectionAssert.AreEqual(Evidence(stations.Select(p => SectionCheckExecution.Run(p, shear, verifier))), Evidence(stations.Select(p => run.Run(p, shear, verifier, true))));
    }

    [TestMethod]
    public void TheWholeModelIsReadOnceForTheChecksOfARun()
    {
        var model = MixedModelFactory.Create(); var verifier = new Verifier(); var stations = Prepare(model);
        int Count(List<string> keys, string key) => keys.Count(k => k == key);
        var alone = Computed(() => Checks(stations, (p, m) => SectionCheckExecution.Run(p, m, verifier)));
        // One by one: two currency checks per row, each in a new read scope, and the capture of the provenance of the row (its design).
        foreach (var key in new[] { "analysis", "analysis-compatibility", "known-analysis", "verification:value:run" }) Assert.AreEqual(20, Count(alone, key), key);
        Assert.AreEqual(10, Count(alone, "design-fingerprint"));
        var run = new SectionCheckRun(model);
        var together = Computed(() => Checks(stations, (p, m) => run.Run(p, m, verifier, true)));
        foreach (var key in new[] { "analysis", "analysis-compatibility", "known-analysis", "verification:value:run", "design-fingerprint", "registered-results" })
            Assert.AreEqual(1, Count(together, key), key);
        // A capture on its own walks the design once (it was twice, for the scenario and for the design).
        Assert.AreEqual(1, Count(Computed(() => VerificationPreparation.CurrentProvenance(model)), "design-fingerprint"));
        // A verifier not declared to read only its input: the model is read again after each of its calls, before and after every row.
        var fresh = new SectionCheckRun(model);
        var undeclared = Computed(() => Checks(stations, (p, m) => fresh.Run(p, m, verifier, false)));
        Assert.AreEqual(20, Count(undeclared, "known-analysis"));
    }

    [TestMethod]
    public void AChangeMadeByAVerifierThatMayReadMoreIsSeenByTheFollowingChecks()
    {
        // During its 3rd call (station 1, UlsBiaxialSection) the verifier changes the sample of that station and of station 3.
        // Each on its own copy of one model: the same identities, so the same provenance and evidence.
        var origin = MixedModelFactory.Create();
        CheckResult[] Results(bool run)
        {
            var model = ModelArchive.Copy(origin); var stations = Prepare(model); var reads = new SectionCheckRun(model);
            var verifier = new Verifier { During = n => { if (n == 3) { stations[1].Sample.ResultBeamForces.M1 += 100; stations[3].Sample.ResultBeamForces.M1 += 100; } } };
            return Checks(stations, (p, m) => run ? reads.Run(p, m, verifier, false) : SectionCheckExecution.Run(p, m, verifier));
        }
        var together = Results(true);
        CollectionAssert.AreEqual(Evidence(Results(false)), Evidence(together));
        CollectionAssert.AreEqual(new[] { "", "", "InputsChangedDuringVerification", "StalePreparation", "", "", "StalePreparation", "StalePreparation", "", "" },
            together.Select(r => string.Join(",", r.Diagnostics.Select(d => d.Code))).ToArray());
    }
    [TestMethod]
    public void TheReadsServeUntilTheCallerDeclaresThatTheModelMayHaveChanged()
    {
        var model = MixedModelFactory.Create(); var verifier = new Verifier(); var stations = Prepare(model);
        var run = new SectionCheckRun(model);
        Assert.AreEqual(EngineeringOutcome.Satisfied, run.Run(stations[0], CheckMechanism.UlsBiaxialSection, verifier, true).Outcome);
        // A node of the analysis model moves between two checks. The caller's contract: code that may change the model is declared;
        // undeclared, the run answers from the reads of the earlier checks.
        model.NodesElements[40].Position.Z += 1;
        Assert.AreEqual(EngineeringOutcome.Satisfied, run.Run(stations[1], CheckMechanism.UlsBiaxialSection, verifier, true).Outcome);
        run.ModelMayHaveChanged();
        var stale = run.Run(stations[1], CheckMechanism.UlsBiaxialSection, verifier, true);
        Assert.AreEqual(DataStatus.Stale, stale.Data); Assert.AreEqual("StalePreparation", stale.Diagnostics.Single().Code);
        Assert.AreEqual(DataStatus.Stale, SectionCheckExecution.Run(stations[1], CheckMechanism.UlsBiaxialSection, verifier).Data);
        // Undone and declared: current again.
        model.NodesElements[40].Position.Z -= 1; run.ModelMayHaveChanged();
        Assert.AreEqual(EngineeringOutcome.Satisfied, run.Run(stations[1], CheckMechanism.UlsBiaxialSection, verifier, true).Outcome);
        // An input of another model is read as by SectionCheckExecution.Run.
        var other = MixedModelFactory.Create(); var foreign = Prepare(other)[2];
        Assert.AreEqual(EngineeringOutcome.Satisfied, run.Run(foreign, CheckMechanism.UlsBiaxialSection, verifier, true).Outcome);
        other.NodesElements[40].Position.Z += 1;
        Assert.AreEqual(DataStatus.Stale, run.Run(foreign, CheckMechanism.UlsBiaxialSection, verifier, true).Data);
        Assert.ThrowsException<ArgumentNullException>(() => new SectionCheckRun(null!));
    }

    [TestMethod]
    public void APreparationOfTheRunIsTheOneMadeAloneAndOwnsItsDiagnostics()
    {
        var model = MixedModelFactory.Create(); var verifier = new Verifier(); var run = new SectionCheckRun(model);
        var samples = ResultQueries.BeamSamples(model.BeamElements[250], "synthetic-static", "P+");
        string Describe(BeamPreparation p) => p.Status + ":" + string.Join(",", p.Diagnostics.Select(d => d.Code + "/" + d.ElementId + "/" + d.Message));
        var alone = samples.Select(s => Verification.PrepareBeam(model, 250, s, "run")).ToArray();
        var together = samples.Select(s => run.Prepare(250, s, "run")).ToArray();
        CollectionAssert.AreEqual(alone.Select(Describe).ToArray(), together.Select(Describe).ToArray());
        CollectionAssert.AreEqual(Evidence(alone.Select(p => SectionCheckExecution.Run(p, CheckMechanism.UlsBiaxialSection, verifier))),
            Evidence(together.Select(p => run.Run(p, CheckMechanism.UlsBiaxialSection, verifier, true))));
        // A validation error of the model (two nodes with one source identity), read once by the run: every preparation is Insufficient
        // with its own copy of the error, as each one prepared alone.
        model.NodesElements[90].Source = model.NodesElements[10].Source; run.ModelMayHaveChanged();
        alone = samples.Select(s => Verification.PrepareBeam(model, 250, s, "run")).ToArray();
        var keys = Computed(() => together = samples.Select(s => run.Prepare(250, s, "run")).ToArray());
        Assert.AreEqual(1, keys.Count(k => k == "validation-errors"));
        Assert.IsTrue(together.All(p => p.Status == DataStatus.Insufficient && p.Input == null && p.Diagnostics.Any(d => d.Code == "SourceCollision")));
        CollectionAssert.AreEqual(alone.Select(Describe).ToArray(), together.Select(Describe).ToArray());
        Assert.IsFalse(together.Skip(1).Any(p => p.Diagnostics.Any(d => together[0].Diagnostics.Contains(d))));
    }
    [TestMethod]
    public void ShellInputsAreReadWithTheRun()
    {
        var model = MixedModelFactory.Create();
        var request = new PreparationRequest { Selection = new ElementSelection { Families = new[] { EntityFamily.Shell } },
            Results = new[] { new ResultSelection { Dataset = "synthetic-static", Case = "P+" } }, Settings = "run" };
        var inputs = ModelPreparation.Prepare(model, request).Samples.Select(s => s.Shell.Input).ToArray();
        var run = new SectionCheckRun(model);
        var keys = Computed(() => Assert.IsTrue(inputs.All(run.IsCurrent)));
        Assert.AreEqual(1, keys.Count(k => k == "verification:value:run"));
        inputs[1].Sample.Forces.Mxx += 1000; run.ModelMayHaveChanged();
        CollectionAssert.AreEqual(new[] { true, false }, inputs.Select(run.IsCurrent).ToArray());
        CollectionAssert.AreEqual(inputs.Select(i => i.IsCurrent).ToArray(), inputs.Select(run.IsCurrent).ToArray());
        Assert.ThrowsException<ArgumentNullException>(() => run.IsCurrent(null!));
    }
}
