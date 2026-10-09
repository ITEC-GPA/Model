using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using GPC.Geometry;
using GPC.Model.Persistence;
using GPC.Model.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Reports;
using GPC.Model.Compatibility;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Results.Locations;

namespace UnitTest;

[TestClass]
public class CheckReportContractsTest
{
    private static CheckResult Passed(double utilization = .5) => new()
    {
        Execution = ExecutionStatus.Completed, Data = DataStatus.Ready,
        Outcome = utilization <= 1 ? EngineeringOutcome.Satisfied : EngineeringOutcome.NotSatisfied,
        Utilization = utilization, EngineVersion = "fixture-1", EngineConfiguration = "fixture-config",
        Dataset = "D", Case = "C", ConcomitantStateId = "C/step2", ElementId = 7, Family = EntityFamily.Beam
    };
    private static CheckResult[] Restore(params CheckResult[] results)
    {
        using var stream = new MemoryStream();
        CheckReportArchive.Save(results.Select(CheckReport.ForSingleResult), stream); stream.Position = 0;
        return CheckReportArchive.Load(stream).SelectMany(r => r.Results).ToArray();
    }

    [TestMethod]
    public void IncompleteWorkKeepsFailuresAndEveryReasonVisible()
    {
        var rows = new[] { Passed(1.2), new CheckResult { Data = DataStatus.NotSupported },
            new CheckResult { Data = DataStatus.Insufficient }, new CheckResult { Data = DataStatus.MissingDependency },
            new CheckResult { Execution = ExecutionStatus.Cancelled }, new CheckResult { Execution = ExecutionStatus.Error },
            new CheckResult { Applicability = CheckApplicability.Excluded, ApplicabilityReason = "Explicit scope exclusion" },
            new CheckResult { Data = DataStatus.Stale } };
        var summary = new CheckSummary(9, rows);
        Assert.AreEqual(1, summary.Completed); Assert.AreEqual(1, summary.MissingRows);
        Assert.AreEqual(1, summary.Unsupported); Assert.AreEqual(2, summary.MissingData);
        Assert.AreEqual(1, summary.Cancelled); Assert.AreEqual(1, summary.Errors); Assert.AreEqual(1, summary.Excluded);
        Assert.AreEqual(1, summary.Stale); Assert.AreEqual(8, summary.Outstanding);
        Assert.IsTrue(summary.HasFailures); Assert.IsFalse(summary.IsComplete);
        Assert.AreEqual(EngineeringOutcome.NotEvaluated, summary.Outcome);
    }

    [TestMethod]
    public void NotApplicableNeedsReasonAndReadyData_ExclusionNeverCompletesARequest()
    {
        var na = new CheckResult { Applicability = CheckApplicability.NotApplicable, Data = DataStatus.Ready };
        Assert.AreEqual(1, new CheckSummary(1, new[] { na }).Invalid);
        na.ApplicabilityReason = "No applicable mechanism, according to the selected method";
        var allNa = new CheckSummary(1, new[] { na });
        Assert.IsTrue(allNa.IsComplete); Assert.AreEqual(EngineeringOutcome.NotEvaluated, allNa.Outcome);
        Assert.AreEqual(EngineeringOutcome.Satisfied, new CheckSummary(2, new[] { Passed(), na }).Outcome);
        na.Data = DataStatus.NotSupported;
        Assert.IsFalse(new CheckSummary(2, new[] { Passed(), na }).IsComplete);
        na.Data = DataStatus.Ready; na.Applicability = CheckApplicability.Excluded;
        Assert.AreEqual(EngineeringOutcome.NotEvaluated, new CheckSummary(2, new[] { Passed(), na }).Outcome);
    }

    [TestMethod]
    public void BooleanDetailingChecksDoNotNeedAnInventedUtilization()
    {
        var row = Passed(); row.Utilization = null; row.Mechanism = CheckMechanism.Detailing;
        row.Details = new DetailingCheckDetails("fixture-spacing", new[] { new CheckMetric("spacing", 100, 150, "mm", passed: true) });
        Assert.AreEqual(EngineeringOutcome.Satisfied, CheckReport.ForSingleResult(row).Outcome);
        row.Details = new DetailingCheckDetails("fixture-spacing", new[] { new CheckMetric("spacing", 200, 150, "mm", passed: false) });
        Assert.AreEqual(EngineeringOutcome.NotEvaluated, CheckReport.ForSingleResult(row).Outcome);
        row.Outcome = EngineeringOutcome.NotSatisfied;
        Assert.AreEqual(EngineeringOutcome.NotSatisfied, CheckReport.ForSingleResult(row).Outcome);
        Assert.AreEqual(0, CheckReportViews.Governing(new[] { row }).Count);
    }

    [TestMethod]
    public void InvalidNumericVerdictsAndNonConvergenceCannotProducePass()
    {
        foreach (var bad in new[] { double.NaN, double.PositiveInfinity, -1, 1.2 })
        {
            var row = Passed(); row.Utilization = bad;
            Assert.AreEqual(EngineeringOutcome.NotEvaluated, CheckReport.ForSingleResult(row).Outcome);
        }
        var unconverged = Passed(); unconverged.Details = new ServiceabilityCheckDetails("fixture-sls", "stress-limit", "rare",
            new[] { new CheckMetric("stress", 10, 20, "N/mm2", .5) }, convergence: new CheckConvergence(false, 50, .1, .001, "relative residual"));
        Assert.AreEqual(EngineeringOutcome.NotEvaluated, CheckReport.ForSingleResult(unconverged).Outcome);
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new CheckMetric("x", double.NaN, 1, "N"));
        Assert.ThrowsException<ArgumentException>(() => new CheckMetric("x", 2, 1, "N", 2, true));
    }

    [TestMethod]
    public void GoverningPreservesWholeConcomitantStateAndSeparatesNormsAndMethods()
    {
        var first = Passed(.7); var second = Passed(.9); var differentNorm = Passed(1.1); var differentMethod = Passed(.8);
        var context = new CheckStandardContext("Code", "2018", "IT", "fixture", "gamma=1.5");
        first.Standard = second.Standard = differentMethod.Standard = context;
        differentNorm.Standard = new CheckStandardContext("Code", "2026", "IT", "fixture", "gamma=1.5");
        second.Case = "B"; second.ConcomitantStateId = "B-step"; second.Station = .75; second.Side = SectionSide.Left;
        differentMethod.Details = new ShearCheckDetails("different-criterion", 1, 1, new[] { new CheckMetric("V", 80, 100, "N", .8) });
        var groups = CheckReportViews.Governing(new[] { first, second, differentNorm, differentMethod });
        Assert.AreEqual(3, groups.Count); Assert.IsTrue(groups.Any(g => ReferenceEquals(second, g.Result)));
        Assert.AreEqual("B-step", groups.Single(g => ReferenceEquals(second, g.Result)).Result.ConcomitantStateId);
        var report = CheckReport.ForSingleResult(first); report.Results.Add(differentNorm);
        Assert.ThrowsException<InvalidOperationException>(() => report.Governing(CheckMechanism.UlsBiaxialSection));
        Assert.AreEqual(2, CheckReportViews.ByStandard(new[] { first, second, differentNorm }).Count);
    }

    [TestMethod]
    public void ElementFamiliesAndOverlappingGroupsDoNotChangeGlobalCounts()
    {
        var beam = Passed(); beam.GroupNames = new[] { "A", "B", "A" }; beam.Job = "Job 1";
        var shell = new CheckResult { Family = EntityFamily.Shell, ElementId = 7, Data = DataStatus.NotSupported,
            GroupNames = new[] { "A" }, Job = "Job 2", Face = "top", Layer = "layer 1" };
        var rows = new[] { beam, shell };
        Assert.AreEqual(2, CheckReportViews.ByElement(rows).Count);
        Assert.AreEqual(2, CheckReportViews.ByGroup(rows).Single(g => g.Key == "A").Results.Count);
        Assert.AreEqual(1, CheckReportViews.ByGroup(rows).Single(g => g.Key == "B").Results.Count);
        Assert.AreEqual(2, CheckReportViews.ByJob(rows).Count);
        Assert.AreEqual(2, new CheckSummary(2, rows).Recorded);
        Assert.AreEqual(EngineeringOutcome.NotEvaluated, new CheckSummary(2, rows).Outcome);
        var restored = Restore(shell).Single(); Assert.AreEqual("top", restored.Face); Assert.AreEqual("layer 1", restored.Layer);
    }

    [TestMethod]
    public void SnapshotsAreIndependentAndSealDetectsEditedEvidence()
    {
        var axes = ResultTransformations.AtPoint(CoordinateSystem.Global, new Point3d(1, 2, 3));
        var forces = new ResultBeamForces(-100, 20, 30, 40, 50, 60, axes);
        var snapshot = new BeamForceSnapshot(forces); forces.N = -900; axes.Origin.X = 99;
        var copy = snapshot.Axes; copy.Origin.Y = 99;
        Assert.AreEqual(-100, snapshot.N); Assert.AreEqual(1, snapshot.Axes.Origin.X); Assert.AreEqual(2, snapshot.Axes.Origin.Y);
        var row = Passed(); row.Input = new CheckInputSnapshot("section", "sample", "forces", snapshot);
        row.SealEvidence(); Assert.IsTrue(row.HasUnchangedEvidence);
        var restored = Restore(row).Single(); Assert.IsTrue(restored.HasUnchangedEvidence);
        restored.Case = "changed";
        Assert.IsFalse(restored.HasUnchangedEvidence); Assert.AreEqual(EngineeringOutcome.NotEvaluated, CheckReport.ForSingleResult(restored).Outcome);
    }

    [TestMethod]
    public void TypedDetailsRoundTripWithoutSolverObjects()
    {
        var types = new CheckDetails[] {
            new ShearCheckDetails("fixture-shear", 2, 1.5, new[] { new CheckMetric("V", 20, 40, "N", .5) }),
            new TorsionCheckDetails("fixture-torsion", 100, new[] { new CheckMetric("T", 20, 40, "Nmm", .5) }),
            new ServiceabilityCheckDetails("fixture-sls", "stress", "quasi-permanent", new[] { new CheckMetric("stress", 10, 20, "N/mm2", .5) },
                new[] { new CheckCalculationValue("alpha", 1, "1", "fixture") },
                new[] { new CheckPointValue("sigma", 10, 20, 0, 10, "N/mm2", "section") }, new CheckConvergence(true, 4, .0001, .001, "relative residual")),
            new DetailingCheckDetails("fixture-spacing", new[] { new CheckMetric("spacing", 100, 200, "mm", .5) }) };
        var rows = types.Select(d => { var r = Passed(); r.Details = d; r.SealEvidence(); return r; }).ToArray();
        var restored = Restore(rows);
        CollectionAssert.AreEqual(types.Select(t => t.GetType()).ToArray(), restored.Select(r => r.Details.GetType()).ToArray());
        Assert.IsTrue(restored.All(r => r.HasUnchangedEvidence));
        Assert.AreEqual(4, restored[2].Details.Convergence.Iterations);
        Assert.AreEqual(10, restored[2].Details.Points.Single().Value);
    }

    [TestMethod]
    public void HistoricalResultWithoutOptionalFieldsRemainsReadable()
    {
        using var stream = new MemoryStream(); CheckReportArchive.Save(new[] { CheckReport.ForSingleResult(Passed()) }, stream);
        stream.Position = 0; var xml = XDocument.Load(stream);
        string[] added = { "SchemaVersion", "Applicability", "ApplicabilityReason", "Standard", "Input", "Details", "Face", "Layer", "GroupNames", "EvidenceFingerprint" };
        xml.Descendants().Where(e => added.Any(n => e.Name.LocalName == "_x003C_" + n + "_x003E_k__BackingField")).Remove();
        xml.Descendants().Where(e => e.Name.LocalName == "_resultSlotsFingerprint").Remove();
        using var legacy = new MemoryStream(Encoding.UTF8.GetBytes(xml.ToString()));
        var report = CheckReportArchive.Load(legacy).Single(); var row = report.Results.Single();
        Assert.AreEqual(0, row.SchemaVersion); Assert.IsNull(row.Standard); Assert.IsNull(row.Input); Assert.IsNull(row.Details);
        Assert.AreEqual(CheckApplicability.Required, row.Applicability); Assert.AreEqual(EngineeringOutcome.Satisfied, report.Outcome);
    }

    [TestMethod]
    public void UnsupportedSchemasAreRejectedBeforeWritingToDestination()
    {
        var row = Passed(); row.SchemaVersion = 999;
        using var stream = new MemoryStream(); stream.WriteByte(42);
        Assert.ThrowsException<SerializationException>(() => CheckReportArchive.Save(new[] { CheckReport.ForSingleResult(row) }, stream));
        Assert.AreEqual(1, stream.Length);
        using var unknown = new MemoryStream(Encoding.UTF8.GetBytes("<GpcCheckReports version=\"999\"/>"));
        Assert.ThrowsException<SerializationException>(() => CheckReportArchive.Load(unknown));
    }
}
