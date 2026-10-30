using GPC.Examples;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;
using GPC.Model.Core;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Data.Steel;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Processing;
using GPC.Model.Results.Queries;
using GPC.Model.Results.State;
using GPC.Model.Results.Storage;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using GPC.Model.Structure.Assignments;
using GPC.Model.Structure.Members;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Model.Core.Tests;

/// <summary>Member serviceability tasks: declared categories, member-node displacements in the member axes, currency of the plan
/// and of the scope digest under a displacement-only change, refused declarations and unchanged historical fingerprints.
/// Fixture: straight 4 m member along X of four elements, section axis 1 = global Y (member axes V1 = Y, V2 = Z, V3 = X),
/// analytical nodal displacements dx = 0.001x, dy = 0.002x, dz = -a x (L^3 - 2 L x^2 + x^3) assigned per case.</summary>
[TestClass]
public class MemberDeformationPreparationTest
{
    private const double L = 4000;
    private static readonly Dictionary<string, double> Amplitude = new() { ["SLE-R"] = 1e-13, ["SLE-R2"] = 1.2e-13, ["SLE-F"] = .8e-13, ["G"] = .6e-13, ["Q"] = .4e-13, ["ULS"] = 1.5e-13 };
    private static double W(string name, double x) => Amplitude[name] * x * (L * L * L - 2 * L * x * x + x * x * x);

    private static GPC.Model.Models.Model Fixture(Action<GPC.Model.Models.Model>? configure = null, Func<string, NodeElement, ResultState, ResultState>? state = null)
    {
        var model = new GPC.Model.Models.Model("Member deformation fixture: assigned analytical nodal displacements");
        for (int i = 0; i <= 4; i++) model.NodesElements.Add(new NodeElement(new Point3d(1000 * i, 0, 0), id: i + 1));
        var section = new SteelSection(new SectionH(300, 12, 150, 18, 150, 18, "H"), SteelMaterialEN1993Data.S355); model.AddProperty(section);
        var parts = new List<BeamMemberPart>();
        for (int i = 0; i < 4; i++)
        {
            var beam = new BeamElement(null, null, section, id: 100 + i); model.BeamElements.Add(beam); model.ConnectBeam(beam.Id, i + 1, i + 2);
            beam.Assignments.Formulation = BeamFormulation.StraightTwoNode; beam.Assignments.StationDomain = "NodeToNode";
            beam.Assignments.SectionAxes = Axes.Beam(beam.StartPoint, beam.EndPoint, new Vector3d(0, 1, 0));
            beam.Assignments.SectionGeometryAxes = beam.Assignments.SectionAxes; beam.Assignments.ActionsAtSectionCentroidConfirmed = true;
            beam.Assignments.Sections.Add(new BeamSectionAssignment { Start = 0, End = 1, Property = section });
            parts.Add(new BeamMemberPart(beam.Id, true));
        }
        model.PhysicalMembers.Add("B", new PhysicalMemberDefinition("B", parts, "NodeToNode", source: "Fixture: explicit straight member"));
        foreach (var name in Amplitude.Keys) model.LoadCases.Add(new LoadCaseBase(name));
        configure?.Invoke(model);
        model.CaptureAnalysis(ReinforcementAnalysisRole.ExcludedFromAnalysis, "Assigned analytical displacements; no FE solution.");
        string fp = model.AnalysisFingerprint();
        model.Datasets.Add("d", new AnalysisDataset { Id = "d", Program = "Analytical", ModelRevision = "r1", InputFingerprint = fp, NormalizedUnits = "N,mm,rad", IsSynthetic = true, Semantics = AnalysisSemantics.LinearStatic });
        foreach (var lc in model.LoadCases.Values)
            foreach (var node in model.NodesElements.Values)
            {
                double x = node.Position.X;
                var s = new ResultState { DatasetId = "d", ModelRevision = "r1", InputFingerprint = fp, Components = Enumerable.Repeat(ComponentAvailability.Available, 6).ToArray(),
                    Semantics = AnalysisSemantics.LinearStatic, ConcomitantStateId = lc.Name, IsCumulative = true, IsSynthetic = true };
                if (state != null) s = state(lc.Name, node, s);
                if (s == null) continue;
                var axes = ActionTransformations.AtPoint(CoordinateSystem.Global, node.Position);
                node.AddResult(new NodeResult(new List<INodeResultLocation> { new NodeResultDisplacement(lc,
                    new ResultDisplacement(axes, .001 * x, .002 * x, -W(lc.Name, x), 0, 0, 0)) { State = s } }));
            }
        return model;
    }
    private static ResultSelection State(string name, CombinationCategory? category) => new() { Dataset = "d", Case = name, ConcomitantState = name, Category = category };
    private static MemberCheckSpecification Deflection(CombinationCategory category = CombinationCategory.Characteristic, MemberVariableDeflectionBasis basis = MemberVariableDeflectionBasis.None, string? variable = null)
        => new() { MemberId = "B", MethodId = "fixture-sle", Mechanism = CheckMechanism.Serviceability, Category = category, Context = new MemberDesignContext(globalStateConfirmed: true),
            Deformation = new MemberDeformationSpecification(MemberDeformationCriterion.VerticalDeflection, MemberDeformationDirection.Axis2, basis, variable == null ? null : State(variable, null)) };
    private static BeamCheckPlanRequest Request(MemberCheckSpecification[] checks, params ResultSelection[] results)
        => new() { MemberIds = new[] { "B" }, SectionMechanisms = Array.Empty<CheckMechanism>(), Results = results, MemberChecks = checks };
    private static NodeResultDisplacement Sample(GPC.Model.Models.Model model, int node, string name)
        => ResultQueries.Samples<NodeResultDisplacement>(model.NodesElements[node], State(name, null)).Single();

    [TestMethod]
    public void DeformationTaskPreparesTheNodalDisplacementsInMemberAxes()
    {
        var model = Fixture();
        var plan = BeamCheckPlan.PrepareActions(model, Request(new[] { Deflection() }, State("SLE-R", CombinationCategory.Characteristic), State("ULS", CombinationCategory.Ultimate)));
        var work = plan.WorkItems.Single();
        Assert.AreEqual(DataStatus.Ready, work.Data, string.Join(";", work.Diagnostics.Select(d => d.Code)));
        Assert.IsTrue(plan.IsCurrent);
        Assert.AreEqual(CheckScope.PhysicalMember, work.Scope); Assert.AreEqual(CheckMechanism.Serviceability, work.Mechanism);
        Assert.AreEqual("SLE-R", work.Selection.Case); Assert.AreEqual(CombinationCategory.Characteristic, work.Selection.Category);
        Assert.AreEqual(0, work.Member.ActionSections.Count); Assert.AreEqual(0, work.Member.Snapshot.Samples.Count);
        Assert.AreEqual(5, work.Coverage.RequiredLocations); Assert.AreEqual(5, work.Coverage.AvailableLocations);
        var deformation = work.Member.Snapshot.Deformation;
        Assert.AreEqual(CombinationCategory.Characteristic, deformation.Category);
        Assert.AreEqual("VerticalDeflection/Axis2/None", deformation.Specification.Key);
        Assert.AreEqual(0, deformation.Variable.Count);
        CollectionAssert.AreEqual(new[] { 0d, 1000, 2000, 3000, 4000 }, deformation.Total.Select(s => s.Location.Distance).ToArray());
        CollectionAssert.AreEqual(new[] { 1, 2, 3, 4, 5 }, deformation.Total.Select(s => s.NodeId).ToArray());
        foreach (var s in deformation.Total)
        {
            double x = s.Location.Distance;
            // Member axes: V1 = global Y, V2 = global Z, V3 = global X.
            Assert.AreEqual(.002 * x, s.D1, 1e-12); Assert.AreEqual(-W("SLE-R", x), s.D2, 1e-12); Assert.AreEqual(.001 * x, s.D3, 1e-12);
            Assert.AreEqual("SLE-R", s.State.ConcomitantState);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { Sample(model, s.NodeId, "SLE-R") }), s.SampleFingerprint);
        }
        // 5/384 qL^4/EI form: a L^4 x 5/16 at mid-span.
        Assert.AreEqual(-1e-13 * 5 / 16 * Math.Pow(L, 4), deformation.Total[2].D2, 1e-9);
    }

    [TestMethod]
    public void DisplacementOnlyChangesMakeThePlanAndTheScopeStale()
    {
        var model = Fixture();
        var request = Request(new[] { Deflection(basis: MemberVariableDeflectionBasis.Difference, variable: "G") }, State("SLE-R", CombinationCategory.Characteristic));
        var plan = BeamCheckPlan.PrepareActions(model, request); string scope = BeamCheckPlan.FingerprintScope(model, request);
        Assert.AreEqual(DataStatus.Ready, plan.WorkItems.Single().Data); Assert.IsTrue(plan.IsCurrent);
        // A case outside the request does not enter the scope.
        Sample(model, 3, "SLE-F").ResultDisplacement.D3 = -1;
        Assert.IsTrue(plan.IsCurrent); Assert.AreEqual(scope, BeamCheckPlan.FingerprintScope(model, request));
        // The task state and the declared variable state do, exactly like the forces of a force-based task.
        foreach (var name in new[] { "SLE-R", "G" })
        {
            var sample = Sample(model, 3, name); double old = sample.ResultDisplacement.D3;
            sample.ResultDisplacement.D3 = old - .5;
            Assert.IsFalse(plan.IsCurrent, name); Assert.AreNotEqual(scope, BeamCheckPlan.FingerprintScope(model, request), name);
            sample.ResultDisplacement.D3 = old;
            Assert.IsTrue(plan.IsCurrent, name); Assert.AreEqual(scope, BeamCheckPlan.FingerprintScope(model, request), name);
        }
        // A rotation is part of the prepared sample too.
        Sample(model, 1, "SLE-R").ResultDisplacement.R2 = 1e-4;
        Assert.IsFalse(plan.IsCurrent);
    }

    [TestMethod]
    public void DeclaredCategoriesSelectTheTaskStates()
    {
        var model = Fixture();
        var results = new[] { State("SLE-R", CombinationCategory.Characteristic), State("SLE-R2", CombinationCategory.Characteristic),
            State("SLE-F", CombinationCategory.Frequent), State("ULS", CombinationCategory.Ultimate) };
        var stability = new MemberCheckSpecification { MemberId = "B", MethodId = "fixture-uls", Context = new MemberDesignContext(globalStateConfirmed: true) };
        var restricted = new MemberCheckSpecification { MemberId = "B", MethodId = "fixture-uls", Category = CombinationCategory.Ultimate, Context = new MemberDesignContext(globalStateConfirmed: true) };
        var characteristic = Deflection(); var frequent = Deflection(CombinationCategory.Frequent);
        var plan = BeamCheckPlan.PrepareActions(model, Request(new[] { characteristic, frequent, stability, restricted }, results));
        string Id(MemberCheckSpecification s, ResultSelection r) => ModelValues.Fingerprint(new object[] { CheckScope.PhysicalMember, new CheckTargetReference("B"), s, r });
        // Deformation tasks only for their category; without a category a force-based task keeps the historical behaviour (every selection).
        var expected = new[] { Id(characteristic, results[0]), Id(characteristic, results[1]), Id(frequent, results[2]), Id(restricted, results[3]) }
            .Concat(results.Select(r => Id(stability, r))).ToArray();
        CollectionAssert.AreEquivalent(expected, plan.WorkItems.Select(w => w.Id).ToArray());
        Assert.AreEqual("SLE-R,SLE-R2", string.Join(",", plan.WorkItems.Where(w => w.Member.Snapshot.Deformation?.Category == CombinationCategory.Characteristic).Select(w => w.Selection.Case)));
        Assert.AreEqual(3, plan.WorkItems.Count(w => w.Member.Snapshot.Deformation != null && w.Data == DataStatus.Ready));
    }

    [TestMethod]
    public void VariableDeflectionStatesArePreparedAsDeclared()
    {
        var model = Fixture();
        var difference = BeamCheckPlan.PrepareActions(model, Request(new[] { Deflection(basis: MemberVariableDeflectionBasis.Difference, variable: "G") },
            State("SLE-R", CombinationCategory.Characteristic), State("SLE-R2", CombinationCategory.Characteristic)));
        Assert.AreEqual(2, difference.WorkItems.Count);
        foreach (var work in difference.WorkItems)
        {
            Assert.AreEqual(DataStatus.Ready, work.Data);
            var d = work.Member.Snapshot.Deformation;
            Assert.AreEqual(5, d.Variable.Count); Assert.IsTrue(d.Variable.All(s => s.State.Case == "G"));
            Assert.AreEqual(-W("G", 2000), d.Variable[2].D2, 1e-12);
            Assert.AreEqual(10, work.Coverage.RequiredLocations);
        }
        var direct = BeamCheckPlan.PrepareActions(model, Request(new[] { Deflection(basis: MemberVariableDeflectionBasis.Direct, variable: "Q") }, State("SLE-R", CombinationCategory.Characteristic)));
        Assert.AreEqual(-W("Q", 3000), direct.WorkItems.Single().Member.Snapshot.Deformation.Variable[3].D2, 1e-12);
        // A variable-only state cannot be paired with one of several task states.
        StringAssert.StartsWith(Assert.ThrowsException<ArgumentException>(() => BeamCheckPlan.PrepareActions(model, Request(new[] { Deflection(basis: MemberVariableDeflectionBasis.Direct, variable: "Q") },
            State("SLE-R", CombinationCategory.Characteristic), State("SLE-R2", CombinationCategory.Characteristic)))).Message, "AmbiguousVariableDeflectionPairing");
    }

    [DataTestMethod]
    [DataRow("no deformation", "MemberServiceabilityRequiresDeformation")]
    [DataRow("ultimate category", "ServiceabilityCategoryRequired")]
    [DataRow("no category", "ServiceabilityCategoryRequired")]
    [DataRow("stability mechanism", "MemberDeformationRequiresServiceability")]
    [DataRow("transverse deflection", "TransverseDirectionOnlyForHorizontalDisplacement")]
    [DataRow("basis without selection", "VariableSelectionRequiresBasis")]
    [DataRow("selection without basis", "VariableSelectionRequiresBasis")]
    [DataRow("variable vibration", "VariableDeflectionOnlyForVerticalDeflection")]
    [DataRow("no selection of the category", "NoResultSelectionForCategory")]
    [DataRow("unspecified category", "InvalidMemberCheckCategory")]
    [DataRow("serviceability category on stability", "UltimateCategoryRequired")]
    public void IncoherentDeclarationsAreRefusedBeforePreparation(string problem, string code)
    {
        var model = Fixture(); var spec = Deflection();
        switch (problem)
        {
            case "no deformation": spec.Deformation = null; break;
            case "ultimate category": spec.Category = CombinationCategory.Ultimate; break;
            case "no category": spec.Category = null; break;
            case "stability mechanism": spec.Mechanism = CheckMechanism.Stability; break;
            case "transverse deflection": spec.Deformation.Direction = MemberDeformationDirection.Transverse; break;
            case "basis without selection": spec.Deformation.VariableBasis = MemberVariableDeflectionBasis.Difference; break;
            case "selection without basis": spec.Deformation.VariableSelection = State("G", null); break;
            case "variable vibration": spec.Deformation = new MemberDeformationSpecification(MemberDeformationCriterion.Vibration, MemberDeformationDirection.Axis2, MemberVariableDeflectionBasis.Difference, State("G", null)); break;
            case "no selection of the category": spec.Category = CombinationCategory.QuasiPermanent; break;
            case "unspecified category": spec.Category = CombinationCategory.Unspecified; break;
            case "serviceability category on stability": spec.Deformation = null; spec.Mechanism = CheckMechanism.Stability; break;
        }
        var e = Assert.ThrowsException<ArgumentException>(() => BeamCheckPlan.PrepareActions(model, Request(new[] { spec }, State("SLE-R", CombinationCategory.Characteristic), State("ULS", CombinationCategory.Ultimate))));
        StringAssert.StartsWith(e.Message, code);
    }

    [DataTestMethod]
    [DataRow("missing interior node", "MissingMemberNodeDisplacement")]
    [DataRow("missing end node", "MissingMemberDisplacementBoundary")]
    [DataRow("nonlinear difference", "VariableDifferenceRequiresLinearStates")]
    [DataRow("unconfirmed state", "GlobalMemberStateNotConfirmed")]
    [DataRow("same state", "VariableSelectionIsTheTaskState")]
    [DataRow("incomplete components", "IncompleteState")]
    public void MissingOrUnqualifiedDisplacementsLeaveTheTaskInsufficient(string problem, string code)
    {
        var model = Fixture(state: (name, node, s) => {
            if (problem == "missing interior node" && node.Id == 3 && name == "SLE-R") return null!;
            if (problem == "missing end node" && node.Id == 5 && name == "SLE-R") return null!;
            if (problem == "nonlinear difference" && name == "G") s.Semantics = AnalysisSemantics.NonlinearStatic;
            if (problem == "incomplete components" && node.Id == 2 && name == "SLE-R") s.Components[3] = ComponentAvailability.Missing;
            return s;
        });
        var spec = Deflection(basis: MemberVariableDeflectionBasis.Difference, variable: problem == "same state" ? "SLE-R" : "G");
        var selection = State("SLE-R", CombinationCategory.Characteristic);
        if (problem == "unconfirmed state") selection.ConcomitantState = null;
        var work = BeamCheckPlan.PrepareActions(model, Request(new[] { spec }, selection)).WorkItems.Single();
        Assert.AreEqual(DataStatus.Insufficient, work.Data);
        Assert.IsTrue(work.Diagnostics.Any(d => d.Code == code), string.Join(";", work.Diagnostics.Select(d => d.Code)));
        if (problem.StartsWith("missing")) Assert.AreEqual(9, work.Coverage.AvailableLocations);
    }

    [TestMethod]
    public void DisplacementsMustBeReferredToTheirNode()
    {
        var model = Fixture();
        var sample = Sample(model, 2, "SLE-R"); sample.ResultDisplacement.CoordinateSystem = CoordinateSystem.Global;
        // Re-register the state after the edit is not needed: the sample is still the registered one; the analysis is unchanged.
        var work = BeamCheckPlan.PrepareActions(model, Request(new[] { Deflection() }, State("SLE-R", CombinationCategory.Characteristic))).WorkItems.Single();
        Assert.AreEqual(DataStatus.Insufficient, work.Data);
        Assert.IsTrue(work.Diagnostics.Any(d => d.Code == "InvalidNodalDisplacement" && d.Message == "NodalDisplacementNotAtNode"));
    }

    [TestMethod]
    public void DeformationContractsCopyWithTheirFingerprints()
    {
        var model = Fixture();
        var request = Request(new[] { Deflection(basis: MemberVariableDeflectionBasis.Difference, variable: "G") }, State("SLE-R", CombinationCategory.Characteristic));
        var snapshot = BeamCheckPlan.PrepareActions(model, request).WorkItems.Single().Member.Snapshot;
        Assert.AreEqual(ModelValues.Fingerprint(new object[] { snapshot }), ModelValues.Fingerprint(new object[] { ModelValues.CopyValue(snapshot) }));
        var copy = ModelValues.CopyValue(request);
        Assert.AreEqual(ModelValues.Fingerprint(new object[] { request }), ModelValues.Fingerprint(new object[] { copy }));
        Assert.AreEqual("G", copy.MemberChecks[0].Deformation.VariableSelection.Case);
        Assert.AreEqual(CombinationCategory.Characteristic, copy.MemberChecks[0].Category);
        // Identity of a task: the declared category and deformation enter it.
        var other = Request(new[] { Deflection(basis: MemberVariableDeflectionBasis.Difference, variable: "Q") }, State("SLE-R", CombinationCategory.Characteristic));
        Assert.AreNotEqual(BeamCheckPlan.PrepareActions(model, request).WorkItems.Single().Id, BeamCheckPlan.PrepareActions(model, other).WorkItems.Single().Id);
    }

    private static MemberServiceabilityLimits Limits(double ratio = 300, string source = "Fixture: floors L/300")
        => new(MemberDeflectionScheme.SupportedEnds, "Fixture: pinned span ends", maxDeflectionRatio: new MemberDeclaredValue(ratio, source),
            precamber: new MemberDeclaredValue(0, "Fixture: no camber"));
    private static string Fp(object value) => ModelValues.Fingerprint(new object[] { value });

    /// <summary>The serviceability declarations of a member enter its deformation tasks only: declaring or editing them changes
    /// neither the force-based snapshot (the MemberFingerprint of a stability engine) nor the scope of a request without deformation
    /// tasks; the deformation snapshot carries them, its plan and scope become stale, and removing them restores both.</summary>
    [TestMethod]
    public void MemberLimitsEnterOnlyTheDeformationTasks()
    {
        var model = Fixture();
        var stability = new MemberCheckSpecification { MemberId = "B", MethodId = "fixture-uls", Context = new MemberDesignContext(globalStateConfirmed: true) };
        var forces = Request(new[] { stability }, State("ULS", null));
        var deformation = Request(new[] { Deflection() }, State("SLE-R", CombinationCategory.Characteristic));
        string ForceSnapshot() => Fp(BeamCheckPlan.PrepareActions(model, forces).WorkItems.Single().Member.Snapshot);
        string forceScope = BeamCheckPlan.FingerprintScope(model, forces), forceSnapshot = ForceSnapshot(), scope = BeamCheckPlan.FingerprintScope(model, deformation);
        var plan = BeamCheckPlan.PrepareActions(model, deformation);
        Assert.IsTrue(plan.IsCurrent); Assert.IsNull(plan.WorkItems.Single().Member.Snapshot.Deformation.ServiceabilityLimits);
        var definition = model.PhysicalMembers["B"];
        model.PhysicalMembers["B"] = definition.WithServiceabilityLimits(Limits());
        Assert.AreEqual(forceScope, BeamCheckPlan.FingerprintScope(model, forces)); Assert.AreEqual(forceSnapshot, ForceSnapshot());
        Assert.IsFalse(plan.IsCurrent); Assert.AreNotEqual(scope, BeamCheckPlan.FingerprintScope(model, deformation));
        var declared = BeamCheckPlan.PrepareActions(model, deformation);
        var snapshot = declared.WorkItems.Single().Member.Snapshot;
        Assert.IsTrue(declared.IsCurrent); Assert.AreEqual(DataStatus.Ready, declared.WorkItems.Single().Data);
        Assert.AreEqual(300, snapshot.Deformation.ServiceabilityLimits.MaxDeflectionRatio.Value);
        Assert.AreEqual(MemberDeflectionScheme.SupportedEnds, snapshot.Deformation.ServiceabilityLimits.DeflectionScheme);
        Assert.IsNull(snapshot.Definition.ServiceabilityLimits); Assert.AreEqual(Fp(definition), Fp(snapshot.Definition));
        Assert.AreEqual(Fp(snapshot), Fp(ModelValues.CopyValue(snapshot)));
        Assert.AreEqual("Fixture: floors L/300", ModelValues.CopyValue(snapshot).Deformation.ServiceabilityLimits.MaxDeflectionRatio.Source);
        // Each declared value and its source are inputs; the identity of the task is not.
        string limited = BeamCheckPlan.FingerprintScope(model, deformation);
        model.PhysicalMembers["B"] = definition.WithServiceabilityLimits(Limits(250));
        Assert.IsFalse(declared.IsCurrent); Assert.AreNotEqual(limited, BeamCheckPlan.FingerprintScope(model, deformation));
        model.PhysicalMembers["B"] = definition.WithServiceabilityLimits(Limits(source: "Fixture: another source"));
        Assert.AreNotEqual(limited, BeamCheckPlan.FingerprintScope(model, deformation));
        Assert.AreEqual(plan.WorkItems.Single().Id, BeamCheckPlan.PrepareActions(model, deformation).WorkItems.Single().Id);
        model.PhysicalMembers["B"] = definition.WithServiceabilityLimits(null);
        Assert.AreEqual(scope, BeamCheckPlan.FingerprintScope(model, deformation)); Assert.IsTrue(plan.IsCurrent);
    }

    [TestMethod]
    public void MemberLimitsAreCompleteDeclarations()
    {
        var value = new MemberDeclaredValue(2, "s");
        Assert.AreEqual("EmptyMemberServiceabilityLimits", Assert.ThrowsException<ArgumentException>(() => new MemberServiceabilityLimits()).Message);
        Assert.AreEqual("DeflectionSchemeAndSourceTogether", Assert.ThrowsException<ArgumentException>(() => new MemberServiceabilityLimits(MemberDeflectionScheme.CantileverAtEnd)).Message);
        Assert.AreEqual("DeflectionSchemeAndSourceTogether", Assert.ThrowsException<ArgumentException>(() => new MemberServiceabilityLimits(deflectionSchemeSource: "s", driftRatio: value)).Message);
        Assert.AreEqual("InvalidMemberDeflectionScheme", Assert.ThrowsException<ArgumentException>(() => new MemberServiceabilityLimits((MemberDeflectionScheme)7, "s")).Message);
        Assert.AreEqual("CantileverReferenceOnSupportedSpan", Assert.ThrowsException<ArgumentException>(() => new MemberServiceabilityLimits(MemberDeflectionScheme.SupportedEnds, "s", value)).Message);
        Assert.ThrowsException<ArgumentException>(() => new MemberServiceabilityLimits(MemberDeflectionScheme.SupportedEnds, " "));
        var nonpositive = new MemberDeclaredValue(0, "s"); var negative = new MemberDeclaredValue(-300, "s");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MemberServiceabilityLimits(maxDeflectionRatio: nonpositive));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MemberServiceabilityLimits(variableDeflectionRatio: negative));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MemberServiceabilityLimits(driftRatio: negative));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MemberServiceabilityLimits(minimumFrequency: nonpositive));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MemberServiceabilityLimits(cantileverLengthFactor: nonpositive));
        Assert.ThrowsException<ArgumentException>(() => new MemberDeclaredValue(250, " "));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MemberDeclaredValue(double.NaN, "s"));
        // A precamber is signed along the checked axis; a cantilever reference without a scheme applies to the engine's cantilever scheme.
        Assert.AreEqual(-5, new MemberServiceabilityLimits(precamber: new MemberDeclaredValue(-5, "s")).Precamber.Value);
        Assert.AreEqual(2, new MemberServiceabilityLimits(cantileverLengthFactor: value).CantileverLengthFactor.Value);
    }

    /// <summary>Fingerprint of a definition without declarations captured with GPCModel 5.28.0.0 (master 9f40d98a) before the member
    /// serviceability declarations existed; the archive forms are in UnitTest MemberServiceabilityArchiveTest.</summary>
    [TestMethod]
    public void DefinitionsWithoutLimitsKeepTheirFingerprints()
    {
        const string master = "1834C73FF5C510100176F9948AB7E4F01EA54B99584C6C3F394C0F2E849F63BC";
        var definition = new PhysicalMemberDefinition("S", new[] { new BeamMemberPart(250, true) }, "NodeToNode");
        Assert.AreEqual(master, Fp(definition)); Assert.IsNull(definition.ServiceabilityLimits);
        var declared = definition.WithServiceabilityLimits(Limits());
        Assert.AreNotEqual(master, Fp(declared)); Assert.IsNull(definition.ServiceabilityLimits);
        Assert.AreEqual(master, Fp(declared.WithServiceabilityLimits(null)));
        Assert.AreNotEqual(Fp(declared), Fp(definition.WithServiceabilityLimits(Limits(250))));
        var copy = ModelValues.CopyValue(declared);
        Assert.AreEqual(Fp(declared), Fp(copy));
        Assert.AreEqual(MemberDeflectionScheme.SupportedEnds, copy.ServiceabilityLimits.DeflectionScheme);
        Assert.AreEqual("Fixture: pinned span ends", copy.ServiceabilityLimits.DeflectionSchemeSource);
        Assert.AreEqual(300, copy.ServiceabilityLimits.MaxDeflectionRatio.Value); Assert.AreEqual(0, copy.ServiceabilityLimits.Precamber.Value);
        Assert.IsNull(copy.ServiceabilityLimits.DriftRatio); Assert.IsNull(copy.ServiceabilityLimits.CantileverLengthFactor);
        CollectionAssert.AreEqual(definition.Parts.Select(p => p.BeamId).ToArray(), copy.Parts.Select(p => p.BeamId).ToArray());
    }

    /// <summary>Fingerprints of the historical member request of MaterialNeutralMemberPreparationTest, captured with GPCModel 5.23.0.2
    /// (master 853b56ef) before the member serviceability contracts: request, specification and task identity are unchanged.
    /// The scope and snapshot digests include element GUIDs, new at every fixture build; Checker's
    /// MemberServiceabilityLegacyArchiveTest compares them on an archived model.</summary>
    [TestMethod]
    public void HistoricalMemberRequestsKeepTheirFingerprints()
    {
        var model = MixedModelFactory.Create(m => {
            var section = new SteelSection(new SectionH(300, 12, 150, 18, 150, 18, "H"), SteelMaterialEN1993Data.S355);
            var beam = m.BeamElements[250]; beam.BeamProperty = section;
            beam.Assignments.Sections.Clear(); beam.Assignments.Sections.Add(new BeamSectionAssignment { Start = 0, End = 1, Property = section });
        });
        model.PhysicalMembers.Add("S", new PhysicalMemberDefinition("S", new[] { new BeamMemberPart(250, true) }, "NodeToNode"));
        var request = new BeamCheckPlanRequest { MemberIds = new[] { "S" }, SectionMechanisms = Array.Empty<CheckMechanism>(),
            Results = new[] { new ResultSelection { Dataset = "synthetic-static", Case = "P+", ConcomitantState = "P+" } },
            MemberChecks = new[] { new MemberCheckSpecification { MemberId = "S", MethodId = "fixture-method", Context = new MemberDesignContext(globalStateConfirmed: true) } } };
        Assert.AreEqual("F64BAE324376A9F976AD326D99D5F2DD2D0677C676CE9EE744AFEC867C98DC3A", ModelValues.Fingerprint(new object[] { request }));
        Assert.AreEqual("16803899557F6C51FACC2A9DFE3D6530914C41B7DEC3263C1801A23E03DA4340", ModelValues.Fingerprint(new object[] { request.MemberChecks[0] }));
        var work = BeamCheckPlan.PrepareActions(model, request).WorkItems.Single();
        Assert.AreEqual("B3D7B085389408DAEA96127DE36F2513B67F0A482BC8B0AB3E09DAE284BB39C9", work.Id);
        Assert.IsNull(work.Member.Snapshot.Deformation);
        Assert.AreEqual(0, work.Member.DisplacementSourcesCount());
    }
}

internal static class MemberInputTestAccess
{
    // DisplacementSources is internal: the count through reflection keeps the public surface unchanged.
    internal static int DisplacementSourcesCount(this PhysicalMemberCheckInput input)
        => ((System.Collections.ICollection)typeof(PhysicalMemberCheckInput).GetProperty("DisplacementSources", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(input)!).Count;
}
