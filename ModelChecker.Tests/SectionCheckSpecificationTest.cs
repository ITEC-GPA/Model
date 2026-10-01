using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Checkers.Concrete.Serviceability;
using GPC.Checkers.Concrete.Shear;
using GPC.Model.Sections.Concrete;
using GPC.Geometry;
using GPC.Model.Checker;
using GPC.Model.Materials;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Results;
using GPC.Model.Standards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Service = GPC.Model.Checker.ModelChecker;

namespace ModelChecker.Tests;

/// <summary>Step 4A/4B: typed discriminators in the plan and serviceability stress limits on the real Concrete engine.</summary>
[TestClass]
public class SectionCheckSpecificationTest
{
    private static ResultSelection Selection(CombinationCategory? category) => new() { Dataset = "synthetic-member", Case = "LC1", ConcomitantState = "LC1", Category = category };

    private static BeamCheckPlanRequest Plan(params SectionCheckSpecification[] checks) => new()
    {
        MemberIds = new[] { "T1" }, Settings = "step4 discriminators", SectionMechanisms = new CheckMechanism[0], SectionChecks = checks,
        Results = new[] { Selection(CombinationCategory.Ultimate), Selection(CombinationCategory.Characteristic), Selection(CombinationCategory.QuasiPermanent) }
    };

    private static GPC.Model.Models.Model Model()
    {
        var m = ElementScopeCharacterizationTest.Model();
        m.PhysicalMembers.Add("T1", new PhysicalMemberDefinition("T1", new[] { new BeamMemberPart(250, true), new BeamMemberPart(251, false) }, "NodeToNode", source: "fixture"));
        return m;
    }

    private static ConcreteVerificationOptions Options(StandardModelCode2010? standard = null) => new()
    {
        Standard = standard ?? new StandardNTC2018Concrete(), Criterion = SectionSolver.FailureAnalysisTypes.ConstantEccentricity,
        ServiceabilityAnalysis = SectionSolver.StressAnalysisTypes.Linear, PsiRebar = 2
    };

    private static ModelCheckRequest Request(BeamCheckPlanRequest plan)
    {
        var request = new ModelCheckRequest();
        request.Jobs.Add(new ModelCheckJob { Name = "step4", BeamPlan = plan, Options = Options() });
        return request;
    }

    [TestMethod]
    public void DirectionsAndServiceCategoriesAreDistinctTasks_OnlyForDeclaredSelections()
    {
        var m = Model();
        var plan = BeamCheckPlan.Prepare(m, Plan(SectionCheckSpecification.ShearAxis1(), SectionCheckSpecification.ShearAxis2(),
            SectionCheckSpecification.StressLimits(CombinationCategory.Characteristic), SectionCheckSpecification.StressLimits(CombinationCategory.QuasiPermanent)));
        // 8 exported samples: two shear directions on the ULS selection, one stress task per service selection.
        Assert.AreEqual(32, plan.WorkItems.Count);
        Assert.AreEqual(32, plan.WorkItems.Select(w => w.Id).Distinct().Count());
        Assert.AreEqual(8, plan.WorkItems.Count(w => w.Check!.Key == "Shear/Axis1/Default/Ultimate"));
        Assert.AreEqual(8, plan.WorkItems.Count(w => w.Check!.Key == "Shear/Axis2/Default/Ultimate"));
        Assert.IsTrue(plan.WorkItems.Where(w => w.Check!.Mechanism == CheckMechanism.Shear).All(w => w.Selection.Category == CombinationCategory.Ultimate));
        Assert.IsTrue(plan.WorkItems.Where(w => w.Check!.Category == CombinationCategory.QuasiPermanent).All(w => w.Selection.Category == CombinationCategory.QuasiPermanent));
    }

    [TestMethod]
    public void IncoherentSpecificationsAndMissingCategoriesAreRejected()
    {
        var m = Model();
        Assert.ThrowsException<ArgumentException>(() => BeamCheckPlan.Prepare(m, Plan(new SectionCheckSpecification(CheckMechanism.Shear, CombinationCategory.Ultimate))));
        Assert.ThrowsException<ArgumentException>(() => BeamCheckPlan.Prepare(m, Plan(new SectionCheckSpecification(CheckMechanism.Serviceability, CombinationCategory.Ultimate, criterion: SectionCheckCriterion.StressLimits))));
        Assert.ThrowsException<ArgumentException>(() => BeamCheckPlan.Prepare(m, Plan(SectionCheckSpecification.ShearAxis1(), SectionCheckSpecification.ShearAxis1())));
        var twice = Plan(SectionCheckSpecification.ShearAxis1()); twice.SectionMechanisms = new[] { CheckMechanism.Shear };
        Assert.ThrowsException<ArgumentException>(() => BeamCheckPlan.Prepare(m, twice));
        var frequent = Plan(SectionCheckSpecification.StressLimits(CombinationCategory.Frequent));
        var error = Assert.ThrowsException<ArgumentException>(() => BeamCheckPlan.Prepare(m, frequent));
        StringAssert.Contains(error.Message, "NoResultSelectionForCategory");
    }

    [TestMethod]
    public void LegacyRequestsKeepTheirFingerprints()
    {
        var m = Model();
        var legacy = new BeamCheckPlanRequest { MemberIds = new[] { "T1" }, Settings = "legacy", Results = new[] { Selection(null) } };
        var explicitNull = legacy.Copy(); explicitNull.SectionChecks = null;
        Assert.AreEqual(BeamCheckPlan.FingerprintScope(m, legacy), BeamCheckPlan.FingerprintScope(m, explicitNull));
        var declared = legacy.Copy(); declared.Results[0].Category = CombinationCategory.Ultimate;
        Assert.AreNotEqual(BeamCheckPlan.FingerprintScope(m, legacy), BeamCheckPlan.FingerprintScope(m, declared));
        // Legacy mechanisms keep the identity formula that does not contain any specification.
        var plan = BeamCheckPlan.Prepare(m, legacy);
        Assert.IsTrue(plan.WorkItems.All(w => w.Check == null && w.Mechanism == CheckMechanism.UlsBiaxialSection));
    }

    [TestMethod]
    public void StressLimitsRunOnTheRealEngine_ShearWithoutSectionDataIsInsufficient()
    {
        var m = Model();
        var report = new Service().Verify(m, Request(Plan(SectionCheckSpecification.ShearAxis1(),
            SectionCheckSpecification.StressLimits(CombinationCategory.Characteristic), SectionCheckSpecification.StressLimits(CombinationCategory.QuasiPermanent))));
        var results = report.Jobs[0].Results;
        Assert.AreEqual(24, report.Required);
        Assert.IsTrue(results.Where(r => r.Check!.Mechanism == CheckMechanism.Shear).All(r => r.Data == DataStatus.Insufficient
            && r.Outcome == EngineeringOutcome.NotEvaluated && r.Diagnostics.Any(d => d.Code == "MissingShearData")));
        var stresses = results.Where(r => r.Check!.Mechanism == CheckMechanism.Serviceability).ToArray();
        Assert.AreEqual(16, stresses.Length);
        Assert.IsTrue(stresses.All(r => r.Execution == ExecutionStatus.Completed && r.Outcome == EngineeringOutcome.Satisfied), string.Join(";", stresses.SelectMany(r => r.Diagnostics).Select(d => d.Code + d.Message)));
        var characteristic = stresses.Where(r => r.Check!.Category == CombinationCategory.Characteristic).Select(r => (ServiceabilityCheckDetails)r.Details).ToArray();
        Assert.IsTrue(characteristic.All(d => d.CombinationCategory == "Characteristic" && d.Metrics.Any(x => x.Key == "SteelTension")));
        Assert.IsTrue(stresses.Where(r => r.Check!.Category == CombinationCategory.QuasiPermanent).All(r => r.Details!.Metrics.Single().Key == "ConcreteCompression"));
        // Governing results stay separated by task specification.
        Assert.AreEqual(2, report.Jobs[0].GoverningByConfiguration.Count);
        Assert.ThrowsException<InvalidOperationException>(() => report.Jobs[0].Governing(CheckMechanism.Serviceability));
        Assert.AreEqual(EngineeringOutcome.NotEvaluated, report.Outcome);
    }

    [TestMethod]
    public void StressLimitsMatchDirectCheckerCallAndNtcCoefficients()
    {
        var m = Model(); var beam = m.BeamElements[250];
        var sample = Verification.BeamSample(beam, "synthetic-member", "LC1", 0, SectionSide.Unspecified);
        var prepared = Verification.PrepareBeam(m, 250, sample, "direct");
        var verifier = Options().CreateVerifier();
        var result = Verification.Run(prepared, SectionCheckSpecification.StressLimits(CombinationCategory.Characteristic), verifier);
        Assert.AreEqual(ExecutionStatus.Completed, result.Execution, string.Join(";", result.Diagnostics.Select(d => d.Code + d.Message)));
        Assert.AreEqual("Serviceability/None/StressLimits/Characteristic", result.Check!.Key);

        // Independent native call with the same options.
        var section = prepared.Input!.Section; var standard = new StandardNTC2018Concrete();
        var reference = new CoordinateSystem(section.Centroid, new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));
        var f = prepared.Input.Forces;
        var options = new SectionCheckerModelCode2010.SectionOptionsModelCode2010(reference, SectionSolver.FailureAnalysisTypes.ConstantEccentricity,
            SectionSolver.FailureDomainTypes.Plastic, SectionSolver.StressAnalysisTypes.Linear, 2, 0, false, 64);
        var checker = new SectionCheckerModelCode2010(new SectionCheckerAttribute(section), options, standard, false);
        var direct = StressLimitCheck.Evaluate(checker.GetTensionAnalysisResult(new ResultBeamForces(f.N, f.V1, f.V2, f.T, f.M1, f.M2, reference)), ServiceabilityCombination.Characteristic);
        Assert.AreEqual(direct.Ratio!.Value, result.Utilization!.Value, 1e-12);

        // NTC 2018 §4.1.2.2.5: 0.60 fck (characteristic), 0.45 fck (quasi-permanent), 0.80 fyk.
        var concrete = (ConcreteMaterialEuropeanCommon)section.ConcreteMaterial;
        Assert.AreEqual(.6 * Math.Abs(concrete.Fck), direct.ConcreteLimit!.Value, 1e-9);
        Assert.AreEqual(.8 * Math.Abs(section.Rebars.First().RebarMaterial.Fyk), direct.SteelGoverning!.Limit, 1e-9);
        var qp = StressLimitCheck.Evaluate(checker.GetTensionAnalysisResult(new ResultBeamForces(f.N, f.V1, f.V2, f.T, f.M1, f.M2, reference)), ServiceabilityCombination.QuasiPermanent);
        Assert.AreEqual(.45 * Math.Abs(concrete.Fck), qp.ConcreteLimit!.Value, 1e-9);
        Assert.IsNull(qp.SteelRatio);
        var frequent = StressLimitCheck.Evaluate(checker.GetTensionAnalysisResult(new ResultBeamForces(f.N, f.V1, f.V2, f.T, f.M1, f.M2, reference)), ServiceabilityCombination.Frequent);
        Assert.IsNull(frequent.Ratio);
        // The explicit thin-casting factor scales the concrete ratio only.
        var thin = StressLimitCheck.Evaluate(checker.GetTensionAnalysisResult(new ResultBeamForces(f.N, f.V1, f.V2, f.T, f.M1, f.M2, reference)), ServiceabilityCombination.Characteristic, .8);
        Assert.AreEqual(direct.ConcreteRatio!.Value / .8, thin.ConcreteRatio!.Value, 1e-12);
        Assert.AreEqual(direct.SteelRatio!.Value, thin.SteelRatio!.Value, 1e-15);
        Assert.IsFalse(verifier.Supports(SectionCheckSpecification.StressLimits(CombinationCategory.Frequent)));
        Assert.IsFalse(verifier.Supports(new SectionCheckSpecification(CheckMechanism.UlsBiaxialSection, CombinationCategory.UltimateSeismic)));
    }

    // Fixture section RC 300x500, 4Ø16 at 50 mm from the edges. V1 along x: bw = 500, d = 250; V2 along y: bw = 300, d = 450.
    private static ConcreteShearData ShearData(ReinforcedConcreteSection section, bool anchored = true) => new(
        new ConcreteShearReinforcement(8, 200, section.Rebars.First().RebarMaterial),
        new ConcreteShearDirection(500, 250, 402, anchored, 2), new ConcreteShearDirection(300, 450, 402, anchored, 2), "fixture drawings", 20);

    [TestMethod]
    public void ShearRunsInBothDirectionsFromTheSectionData_AndMatchesTheCore()
    {
        var m = Model(); var section = m.BeamElements[250].Assignments.Sections[0].Section; section.ShearData = ShearData(section);
        var report = new Service().Verify(m, Request(Plan(SectionCheckSpecification.ShearAxis1(), SectionCheckSpecification.ShearAxis2())));
        var results = report.Jobs[0].Results;
        Assert.AreEqual(16, results.Count);
        Assert.IsTrue(results.All(r => r.Execution == ExecutionStatus.Completed && r.Outcome == EngineeringOutcome.Satisfied),
            string.Join(";", results.SelectMany(r => r.Diagnostics).Select(d => d.Code + " " + d.Message)));
        Assert.IsTrue(results.All(r => ((ShearCheckDetails)r.Details!).Direction == (r.Check!.Direction == SectionCheckDirection.Axis1 ? 1 : 2)));
        Assert.AreEqual(2, report.Jobs[0].GoverningByConfiguration.Count);

        // Same task through the core: NTC 2018, fcd = 0.85·25/1.5, fyd = 450/1.15, Asw = 2·π·8²/4, s = 200 mm.
        var sample = Verification.BeamSample(m.BeamElements[250], "synthetic-member", "LC1", .5, SectionSide.Unspecified);
        var prepared = Verification.PrepareBeam(m, 250, sample, "direct");
        var standard = new StandardNTC2018Concrete(); var f = prepared.Input!.Forces;
        var core = SectionShearCalculator.Calculate(new SectionShearInput(standard, f.N, f.V2, 0, 150000, 300, 450, 0, 25, .85 * 25 / 1.5, 450 / 1.15, 1.5,
            200000, 2 * Math.PI * 16, 200));
        var through = Verification.Run(prepared, SectionCheckSpecification.ShearAxis2(), Options().CreateVerifier());
        var metric = through.Details!.Metrics.Single();
        Assert.AreEqual(core.VRd, metric.Capacity!.Value, 1e-6 * core.VRd); Assert.AreEqual(Math.Abs(f.V2), metric.Demand!.Value, 1e-9);
        Assert.AreEqual(core.CotTheta, ((ShearCheckDetails)through.Details).CotTheta, 1e-12);
    }

    [TestMethod]
    public void ShearDataAreRequiredConfirmedAndInvalidateThePlanWhenChanged()
    {
        var m = Model(); var section = m.BeamElements[250].Assignments.Sections[0].Section;
        // Without shear reinforcement Asl enters the method: an unconfirmed anchorage is insufficient data, not an assumption.
        section.ShearData = new ConcreteShearData(null, new ConcreteShearDirection(500, 250, 402, false, 0), null, "fixture", 20);
        var unconfirmed = Verification.Run(Verification.PrepareBeam(m, 250, Verification.BeamSample(m.BeamElements[250], "synthetic-member", "LC1", .5, SectionSide.Unspecified), "x"),
            SectionCheckSpecification.ShearAxis1(), Options().CreateVerifier());
        Assert.AreEqual(DataStatus.Insufficient, unconfirmed.Data); Assert.IsTrue(unconfirmed.Diagnostics.Any(d => d.Code == "TensionReinforcementAnchorageNotConfirmed"));
        var missingAxis = Verification.Run(Verification.PrepareBeam(m, 250, Verification.BeamSample(m.BeamElements[250], "synthetic-member", "LC1", .5, SectionSide.Unspecified), "x"),
            SectionCheckSpecification.ShearAxis2(), Options().CreateVerifier());
        Assert.IsTrue(missingAxis.Diagnostics.Any(d => d.Code == "MissingShearData"));

        section.ShearData = ShearData(section);
        var plan = BeamCheckPlan.Prepare(m, Plan(SectionCheckSpecification.ShearAxis1()));
        Assert.IsTrue(plan.IsCurrent);
        section.ShearData = new ConcreteShearData(new ConcreteShearReinforcement(10, 200, section.Rebars.First().RebarMaterial),
            section.ShearData.Axis1, section.ShearData.Axis2, "revised drawings", 20);
        Assert.IsFalse(plan.IsCurrent);
    }

    [TestMethod]
    public void ShearIsNotSupportedForStandardsWithoutAnImplementedProfile()
    {
        var verifier = Options(new StandardCNR200()).CreateVerifier();
        Assert.IsFalse(verifier.Supports(SectionCheckSpecification.ShearAxis1()));
        Assert.IsFalse(Options().CreateVerifier().Supports(new SectionCheckSpecification(CheckMechanism.Shear, CombinationCategory.UltimateSeismic, SectionCheckDirection.Axis1)));
        Assert.IsTrue(Options(new StandardDINEN1992p11()).CreateVerifier().Supports(SectionCheckSpecification.ShearAxis2()));
    }

    [TestMethod]
    public void SectionShearDataSurviveTheArchiveAndSectionsWithoutThemKeepTheirRevision()
    {
        var m = Model(); var section = m.BeamElements[250].Assignments.Sections[0].Section;
        string before = m.AnalysisFingerprint(), verification = m.VerificationFingerprint("x");
        section.ShearData = null;
        Assert.AreEqual(before, m.AnalysisFingerprint()); Assert.AreEqual(verification, m.VerificationFingerprint("x"));
        section.ShearData = ShearData(section);
        Assert.AreNotEqual(verification, m.VerificationFingerprint("x"));
        Assert.AreEqual(before, m.AnalysisFingerprint(), "stirrups do not change the FEM analysis");
        using var stream = new MemoryStream(); ModelArchive.Save(m, stream); stream.Position = 0; var restored = ModelArchive.Load(stream);
        var copy = restored.BeamElements[250].Assignments.Sections[0].Section;
        Assert.AreEqual(section.ShearData, copy.ShearData); Assert.AreEqual(8, copy.ShearData!.Reinforcement.Diameter);
        Assert.AreEqual(m.VerificationFingerprint("x"), restored.VerificationFingerprint("x"));
    }

    [TestMethod]
    public void SpecificationSurvivesArchiveRoundTripWithSealedEvidence()
    {
        var m = Model(); var request = Request(Plan(SectionCheckSpecification.StressLimits(CombinationCategory.QuasiPermanent)));
        var report = new Service().Verify(m, request);
        m.CheckReports.AddRange(report.Jobs);
        using var stream = new MemoryStream(); ModelArchive.Save(m, stream); stream.Position = 0; var restored = ModelArchive.Load(stream);
        var saved = restored.CheckReports.Single();
        Assert.IsTrue(saved.Results.All(r => r.HasUnchangedEvidence && r.Check!.Key == "Serviceability/None/StressLimits/QuasiPermanent"));
        Assert.AreEqual(CombinationCategory.QuasiPermanent, saved.BeamScope.SectionChecks!.Single().Category);
        Assert.AreEqual(report.Jobs[0].ScopeFingerprint, BeamCheckPlan.FingerprintScope(restored, saved.BeamScope));
    }
}
