using GPC.Examples;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;
using GPC.Model.Checking.Reports;
using GPC.Model.Compatibility;
using GPC.Model.Core;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Queries;
using GPC.Model.Results.State;
using GPC.Model.Results.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>The currency of a stored report reads the model in one session for all its rows (each digest once per call, not once per
/// row) and stays the check of the rows one by one: a row is stale exactly when that check finds it stale, and every call reads the
/// model as it is, so a change made after a call is seen by the next one.</summary>
[TestClass]
public class CheckReportCurrencyTest
{
    private sealed class Verifier : IConfiguredSectionVerifier
    {
        public string Version => "TEST ONLY";
        public string Configuration => "currency";
        public IReadOnlyCollection<CheckMechanism> Capabilities => new[] { CheckMechanism.UlsBiaxialSection, CheckMechanism.Shear };
        public CheckResult Verify(BeamCheckInput input, CheckMechanism mechanism, CancellationToken token)
            => new CheckResult { Execution = ExecutionStatus.Completed, Data = DataStatus.Ready, Outcome = EngineeringOutcome.Satisfied, Utilization = .5 };
    }

    /// <summary>Five stations of the cantilever, one row per station and mechanism.</summary>
    private static CheckReport Report(Model model, Verifier verifier, params CheckMechanism[] mechanisms)
        => CheckRunner.Beam(model, 250, "synthetic-static", "P+", "currency", mechanisms.Length == 0 ? new[] { CheckMechanism.UlsBiaxialSection, CheckMechanism.Shear } : mechanisms, verifier);

    /// <summary>The check of one stored section row as CheckReport wrote it before the read session (the whole model read again for
    /// each row), with public members only: the reference the session must reproduce.</summary>
    private static bool RowByRow(Model model, CheckResult r, IConcreteSectionVerifier verifier)
    {
        if (r.Provenance != null && !r.Provenance.IsCurrent(model)) return false;
        if (!r.HasUnchangedEvidence || r.Family != EntityFamily.Beam || r.VerificationRevision != model.VerificationFingerprint(r.Settings) || r.EngineVersion != verifier.Version
            || r.EngineConfiguration != (verifier as IConfiguredSectionVerifier)?.Configuration || !model.BeamElements.TryGetValue(r.ElementId, out var beam)) return false;
        var samples = beam.Results.SelectMany(x => x.Results).OfType<StationResultBeamForces>().Where(s => s.State?.DatasetId == r.Dataset && s.Case?.Name == r.Case
            && s.ParametricDistance == r.Station && s.Side == r.Side && s.State.Phase == r.Phase && s.State.Step == r.Step && s.State.ConcomitantStateId == r.ConcomitantStateId
            && s.State.Mode == r.Mode && s.State.MovingLoadPosition == r.MovingLoadPosition).ToArray();
        return samples.Length == 1 && ModelValues.Fingerprint(new object[] { samples[0] }) == r.SampleRevision;
    }

    /// <summary>Which rows are current: each stored row in a report of its own, through CurrentOutcome.</summary>
    private static bool[] CurrentRows(CheckReport report, Model model, Verifier verifier)
        => report.Results.Select(r => CheckReport.ForSingleResult(r).CurrentOutcome(model, verifier) != EngineeringOutcome.NotEvaluated).ToArray();

    private static void SameAsRowByRow(CheckReport report, Model model, Verifier verifier, bool[] current)
    {
        CollectionAssert.AreEqual(report.Results.Select(r => RowByRow(model, r, verifier)).ToArray(), current);
        Assert.AreEqual(current.All(c => c) ? report.Outcome : EngineeringOutcome.NotEvaluated, report.CurrentOutcome(model, verifier));
    }

    [TestMethod]
    public void OneChangedSampleMakesExactlyItsRowsStaleAndEachCallReadsTheModelAgain()
    {
        var model = MixedModelFactory.Create(); var verifier = new Verifier(); var report = Report(model, verifier);
        Assert.AreEqual(10, report.Required); Assert.AreEqual(10, report.Executed);
        Assert.AreEqual(EngineeringOutcome.Satisfied, report.CurrentOutcome(model, verifier));
        SameAsRowByRow(report, model, verifier, CurrentRows(report, model, verifier));
        Assert.IsTrue(CurrentRows(report, model, verifier).All(c => c));

        // After a first call one sample changes: the next call sees it, and only the two rows of that cut are stale.
        var sample = Verification.BeamSample(model.BeamElements[250], "synthetic-static", "P+", .5, SectionSide.Unspecified);
        sample.ResultBeamForces.M1 += 100;
        Assert.AreEqual(EngineeringOutcome.NotEvaluated, report.CurrentOutcome(model, verifier));
        var current = CurrentRows(report, model, verifier);
        CollectionAssert.AreEqual(report.Results.Select(r => r.Station != .5).ToArray(), current);
        Assert.AreEqual(2, current.Count(c => !c));
        SameAsRowByRow(report, model, verifier, current);

        // Changed back: current again, nothing of the earlier calls is kept.
        sample.ResultBeamForces.M1 -= 100;
        Assert.AreEqual(EngineeringOutcome.Satisfied, report.CurrentOutcome(model, verifier));
    }

    [TestMethod]
    public void AChangedElementInputMakesEveryRowThatReadsItStale()
    {
        var model = MixedModelFactory.Create(); var verifier = new Verifier(); var report = Report(model, verifier);
        Assert.AreEqual(EngineeringOutcome.Satisfied, report.CurrentOutcome(model, verifier));
        // The reinforcement of the beam enters the design and verification digests of every row.
        var bar = model.BeamElements[250].Assignments.Sections[0].Section.Rebars.First();
        bar.Position.Y += 2;
        var current = CurrentRows(report, model, verifier);
        Assert.IsTrue(current.All(c => !c));
        SameAsRowByRow(report, model, verifier, current);
        bar.Position.Y -= 2;
        Assert.AreEqual(EngineeringOutcome.Satisfied, report.CurrentOutcome(model, verifier));

        // A node of the analysis model: the analysis digest of every row.
        model.NodesElements[40].Position.Z = 2500;
        current = CurrentRows(report, model, verifier);
        Assert.IsTrue(current.All(c => !c));
        SameAsRowByRow(report, model, verifier, current);
    }

    [TestMethod]
    public void ARowWithAnotherProvenanceIsComparedOnItsOwn()
    {
        var model = MixedModelFactory.Create(); var verifier = new Verifier(); var report = Report(model, verifier);
        // Each row of this report captured its own provenance: equal values in distinct objects, current. So is a copy (as read
        // back from an archive).
        Assert.AreNotSame(report.Results[0].Provenance, report.Results[9].Provenance);
        report.Results[5].Provenance = ModelArchive.CopyValue(report.Results[5].Provenance); report.Results[5].SealEvidence();
        Assert.AreEqual(EngineeringOutcome.Satisfied, report.CurrentOutcome(model, verifier));
        // The provenance of another model's run on one row only: that row alone is stale.
        var foreign = Report(MixedModelFactory.Create(), verifier).Results[0].Provenance;
        Assert.IsFalse(foreign.IsCurrent(model));
        report.Results[3].Provenance = foreign; report.Results[3].SealEvidence();
        var current = CurrentRows(report, model, verifier);
        CollectionAssert.AreEqual(Enumerable.Range(0, 10).Select(i => i != 3).ToArray(), current);
        SameAsRowByRow(report, model, verifier, current);
    }

    /// <summary>The index of the station samples of a beam finds, for every row, the samples of the scan it replaces (same objects, same
    /// order): also two samples of one state and station (ambiguous), -0 and +0, a sample without a state, rows without a dataset or a
    /// station, outside and inside a read scope.</summary>
    [TestMethod]
    public void TheSampleIndexFindsWhatTheScanOfTheBeamFinds()
    {
        var model = MixedModelFactory.Create(); var beam = model.BeamElements[250];
        var existing = Verification.BeamSample(beam, "synthetic-static", "P+", .5, SectionSide.Unspecified);
        var minusZero = Verification.BeamSample(beam, "synthetic-static", "P-", 0, SectionSide.Unspecified);
        StationResultBeamForces Copy(StationResultBeamForces s, double station) => new StationResultBeamForces(s.Case, s.ResultBeamForces, station)
        { PhysicalDistance = s.PhysicalDistance, StationDomain = s.StationDomain, Body = s.Body, State = s.State };
        beam.AddResult(new BeamResult(new IBeamResultLocation[] { Copy(existing, .5), Copy(minusZero, -0.0),
            new StationResultBeamForces(existing.Case, existing.ResultBeamForces, .75) { StationDomain = "NodeToNode", Body = ActionBody.PositiveSectionFace } }));
        CheckResult Row(string? dataset, string caseName, double? station, SectionSide side = SectionSide.Unspecified, string? phase = null) => new CheckResult
        { Dataset = dataset, Case = caseName, Station = station, Side = side, Phase = phase, ConcomitantStateId = caseName, ElementId = 250 };
        var rows = new[] { Row("synthetic-static", "P+", .5), Row("synthetic-static", "P+", .25), Row("synthetic-static", "P-", 0.0), Row("synthetic-static", "P-", -0.0),
            Row("synthetic-static", "P+", .75), Row("synthetic-static", "P+", .5, SectionSide.Left), Row("synthetic-static", "P+", .5, phase: "Stage 2"),
            Row("synthetic-static", "P+", null), Row("synthetic-static", "P+", double.NaN), Row("other", "P+", .5), Row(null, "P-", .75), Row("synthetic-static", "missing", .5) };
        StationResultBeamForces[] Scan(CheckResult r) => beam.Results.SelectMany(x => x.Results).OfType<StationResultBeamForces>().Where(s => s.State?.DatasetId == r.Dataset
            && s.Case?.Name == r.Case && s.ParametricDistance == r.Station && s.Side == r.Side && s.State.Phase == r.Phase && s.State.Step == r.Step
            && s.State.ConcomitantStateId == r.ConcomitantStateId && s.State.Mode == r.Mode && s.State.MovingLoadPosition == r.MovingLoadPosition).ToArray();
        CollectionAssert.AreEqual(new[] { 2, 1, 2, 2, 1, 0, 0, 0, 0, 0, 0, 0 }, rows.Select(r => Scan(r).Length).ToArray());
        foreach (var row in rows)
            CollectionAssert.AreEqual(Scan(row), StationSamples.Matching(model, beam, row).ToArray());
        using (ValidationReadScope.Enter(model))
            foreach (var row in rows)
                CollectionAssert.AreEqual(Scan(row), StationSamples.Matching(model, beam, row).ToArray());
    }

    /// <summary>ShellCheckInput.AreCurrent is the IsCurrent of every input, in one read of the model: the verification digest is
    /// computed once for all the inputs (once per input before), and a change made after a call is seen by the next one.</summary>
    [TestMethod]
    public void ShellInputsAreCurrentTogetherInOneReadOfTheModel()
    {
        var model = MixedModelFactory.Create();
        var request = new PreparationRequest { Selection = new ElementSelection { Families = new[] { EntityFamily.Shell } },
            Results = new[] { new ResultSelection { Dataset = "synthetic-static", Case = "P+" } }, Settings = "currency" };
        var inputs = ModelPreparation.Prepare(model, request).Samples.Select(s => s.Shell.Input).ToArray();
        Assert.AreEqual(2, inputs.Length); Assert.IsTrue(inputs.All(i => i != null && i.IsCurrent));
        var keys = Computed(() => ShellCheckInput.AreCurrent(inputs.Concat(inputs)) ? EngineeringOutcome.Satisfied : EngineeringOutcome.NotEvaluated, EngineeringOutcome.Satisfied);
        Assert.AreEqual(1, keys.Count(k => k == "verification:value:currency"), string.Join(", ", keys));
        Assert.AreEqual(2, Computed(() => inputs.All(i => i.IsCurrent) ? EngineeringOutcome.Satisfied : EngineeringOutcome.NotEvaluated, EngineeringOutcome.Satisfied)
            .Count(k => k == "verification:value:currency"));
        Assert.IsTrue(ShellCheckInput.AreCurrent(new ShellCheckInput[0]));
        Assert.ThrowsException<ArgumentException>(() => ShellCheckInput.AreCurrent(new[] { inputs[0], null! }));

        // After a call the sample of the second shell changes: the next call sees it, for that input only.
        inputs[1].Sample.Forces.Mxx += 1000;
        Assert.IsFalse(ShellCheckInput.AreCurrent(inputs));
        CollectionAssert.AreEqual(new[] { true, false }, inputs.Select(i => i.IsCurrent).ToArray());
        Assert.IsTrue(ShellCheckInput.AreCurrent(inputs.Take(1)));
        inputs[1].Sample.Forces.Mxx -= 1000;
        Assert.IsTrue(ShellCheckInput.AreCurrent(inputs));
    }

    /// <summary>The keys of the values computed from the model during <paramref name="call"/> (thread-local test hook).</summary>
    private static List<string> Computed(Func<EngineeringOutcome> call, EngineeringOutcome expected)
    {
        var keys = new List<string>(); ValidationReadScope.Computed = keys.Add;
        try { Assert.AreEqual(expected, call()); }
        finally { ValidationReadScope.Computed = null; }
        return keys;
    }

    /// <summary>Row by row every row computed the analysis, design, compatibility and verification digests again (4,000 rows on
    /// 1,000 beams: minutes). In one session each value of the model is computed once per call, whatever the rows.</summary>
    [TestMethod]
    public void EachValueOfTheModelIsComputedOncePerCallWhateverTheRows()
    {
        var model = MixedModelFactory.Create(); var verifier = new Verifier();
        var five = Report(model, verifier, CheckMechanism.UlsBiaxialSection); var ten = Report(model, verifier);
        Assert.AreEqual(5, five.Required); Assert.AreEqual(10, ten.Required);
        var keys = Computed(() => ten.CurrentOutcome(model, verifier), EngineeringOutcome.Satisfied);
        Assert.AreEqual(keys.Count, keys.Distinct().Count(), string.Join(", ", keys));
        foreach (var key in new[] { "analysis", "analysis-compatibility", "design-fingerprint", "verification:value:currency", "station-samples" })
            CollectionAssert.Contains(keys, key);
        CollectionAssert.AreEquivalent(keys, Computed(() => five.CurrentOutcome(model, verifier), EngineeringOutcome.Satisfied));
        // Nothing is kept between calls: the next call computes the same values again.
        CollectionAssert.AreEqual(keys, Computed(() => ten.CurrentOutcome(model, verifier), EngineeringOutcome.Satisfied));
        // One provenance on its own: the analysis digest is computed once for its comparison and for the compatibility gate.
        var single = Computed(() => ten.Results[0].Provenance.IsCurrent(model) ? EngineeringOutcome.Satisfied : EngineeringOutcome.NotEvaluated, EngineeringOutcome.Satisfied);
        Assert.AreEqual(1, single.Count(k => k == "analysis"));
    }
}
