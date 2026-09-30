using GPC.Examples;
using GPC.Geometry;
using GPC.Model.Combinations;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Results;
using GPC.Model.Results.ElementResults;
using GPC.Model.Results.ResultLocations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class ModelResultAlgebraTest
{
    private static (GPC.Model.Models.Model model, Combination combination) Fixture()
    {
        var c = new Combination("C");
        var model = MixedModelFactory.Create(m => {
            c.AddLoadCaseCoefficient(m.LoadCases["P+"], 2); c.AddLoadCaseCoefficient(m.LoadCases["P-"], -.5); m.Combinations.Add(c);
        });
        return (model, c);
    }
    private static ResultLocation Sample(Element owner, string name = "P+") => owner.Results.SelectMany(r => r.Results).First(r => r.Case.Name == name);
    private static ElementKey Key(Element e) => new() { Family = GPC.Model.Models.Model.FamilyOf(e), Id = e.Id };

    [DataTestMethod]
    [DataRow("node")][DataRow("beam")][DataRow("plate")][DataRow("displacement")]
    public void LinearCombinations_AllFamilies_StorePrepareAndArchive(string family)
    {
        var (model, combination) = Fixture();
        Element owner = family == "beam" ? model.BeamElements[250] : family == "plate" ? model.AreaElements[1090] : model.NodesElements[10];
        ResultLocation template = family == "displacement" ? owner.Results.SelectMany(r => r.Results).OfType<NodeResultDisplacement>().First() : Sample(owner);
        if (family == "displacement")
        {
            var values = owner.Results.SelectMany(r => r.Results).OfType<NodeResultDisplacement>().ToArray();
            values[0].ResultDisplacement.D1 = 3; values[1].ResultDisplacement.D1 = -4;
        }
        var result = ResultAlgebra.LinearCombination(model, Key(owner), combination, template);
        Assert.IsTrue(result.IsAvailable, string.Join(";", result.Diagnostics.Select(d => d.Message))); Assert.IsTrue(result.IsCurrent);
        if (result.Sample is NodeResultForces n) { Assert.AreEqual(-2500, n.Fy); Assert.AreEqual(5e6, n.Mx); Assert.AreEqual(NodalForceKind.SupportReaction, n.Kind); }
        if (result.Sample is StationResultBeamForces b) { Assert.AreEqual(2500, b.ResultBeamForces.V2); Assert.AreEqual(-5e6, b.ResultBeamForces.M1); }
        if (result.Sample is PointResultPlateForces p) { Assert.AreEqual(225, p.Forces.Fxx); Assert.AreEqual(200000, p.Forces.Mxx); }
        if (result.Sample is NodeResultDisplacement d) Assert.AreEqual(8, d.ResultDisplacement.D1);
        var before = model.AnalysisFingerprint(); ResultAlgebra.Attach(result); Assert.AreEqual(before, model.AnalysisFingerprint());
        Assert.AreEqual(DataStatus.Ready, ResultPreparation.Prepare(model, Key(owner), result.Sample).Status);
        Assert.ThrowsException<InvalidOperationException>(() => ResultAlgebra.Attach(result));
        using var archive = new MemoryStream(); ModelArchive.Save(model, archive); archive.Position = 0; var copy = ModelArchive.Load(archive);
        Element copiedOwner = family == "beam" ? copy.BeamElements[250] : family == "plate" ? copy.AreaElements[1090] : copy.NodesElements[10];
        var copied = copiedOwner.Results.SelectMany(r => r.Results).Single(r => r.Case.Name == "C");
        Assert.AreEqual(DataStatus.Ready, ResultPreparation.Prepare(copy, Key(copiedOwner), copied).Status);
        Assert.IsTrue(copiedOwner.Results.SelectMany(r => r.Results).Any(r => ReferenceEquals(r, copied.State.DerivedFrom[0])));
        copied.State.DerivedFrom[0].State.SourceRecord = "altered";
        Assert.AreEqual(DataStatus.Stale, ResultPreparation.Prepare(copy, Key(copiedOwner), copied).Status);
    }

    [DataTestMethod]
    [DataRow("incomplete")][DataRow("nonlinear")][DataRow("stale")][DataRow("combined")][DataRow("incremental")]
    [DataRow("missing-case")][DataRow("different-owner")][DataRow("different-origin")][DataRow("nonfinite")]
    public void Combination_RejectsAmbiguousOrIncompatibleData(string fault)
    {
        var (model, c) = Fixture(); var owner = model.NodesElements[10]; var first = (NodeResultForces)Sample(owner); var other = (NodeResultForces)Sample(owner, "P-");
        switch (fault)
        {
            case "incomplete": other.State.Components[0] = ComponentAvailability.Missing; break;
            case "nonlinear": other.State.Semantics = AnalysisSemantics.NonlinearStatic; break;
            case "stale": model.NodesElements[40].Position.X = 1; break;
            case "combined": other.State.IsCombined = true; break;
            case "incremental": other.State.IsCumulative = false; break;
            case "missing-case": other.State.DatasetId = "missing"; break;
            case "different-owner": other.Kind = NodalForceKind.ElementEndForce; other.OwnerElementId = 250; other.OwnerElementFamily = EntityFamily.Beam; other.ElementEnd = "I"; other.Body = ActionBody.OnElement; break;
            case "different-origin": other.ResultBeamForces.CoordinateSystem.Origin.X = 2; break;
            case "nonfinite": other.ResultBeamForces.V2 = double.NaN; break;
        }
        var count = owner.Results.Count;
        var result = ResultAlgebra.LinearCombination(model, Key(owner), c, first); Assert.IsFalse(result.IsAvailable, fault); Assert.IsTrue(result.Diagnostics.Count > 0);
        Assert.AreEqual(count, owner.Results.Count);
    }

    [TestMethod]
    public void Envelope_PreservesGoverningConcomitantVectorAndCase()
    {
        var (m, _) = Fixture(); var beam = m.BeamElements[250]; var a = Sample(beam); var b = Sample(beam, "P-");
        var envelope = ResultAlgebra.Envelope(m, Key(beam), new[] { a, b });
        var maxShear = (StationResultBeamForces)envelope.Maxima[2]; var maxMoment = (StationResultBeamForces)envelope.Maxima[4];
        Assert.AreEqual("P+", maxShear.Case.Name); Assert.AreEqual(-2e6, maxShear.ResultBeamForces.M1);
        Assert.AreEqual("P-", maxMoment.Case.Name); Assert.AreEqual(-1000, maxMoment.ResultBeamForces.V2);
        maxShear.ResultBeamForces.M1 = 999; Assert.AreEqual(-2e6, ((StationResultBeamForces)a).ResultBeamForces.M1);
        b.State.Semantics = AnalysisSemantics.IndependentExtrema;
        Assert.ThrowsException<ArgumentException>(() => ResultAlgebra.Envelope(m, Key(beam), new[] { a, b }));
    }

    [TestMethod]
    public void ExplicitLinearHistory_AccumulatesFromZeroAndRejectsGaps()
    {
        var (m, _) = Fixture(); var node = m.NodesElements[10]; var lc = m.LoadCases["P+"]; var increments = new List<ResultLocation>();
        for (int i = 0; i < 3; i++)
        {
            var state = Sample(node).State.Copy(); state.IsCumulative = false; state.HistoryId = "construction"; state.IncrementIndex = i; state.IncrementsStartAtZero = true;
            state.Phase = "stage-" + i; state.Step = i.ToString(); state.ConcomitantStateId = "increment-" + i;
            var sample = new NodeResultDisplacement(lc, new ResultDisplacement(1 + i, 0, 0, 0, 0, 0)) { State = state };
            increments.Add(sample); node.AddResult(new NodeResult(new List<INodeResultLocation> { sample }));
        }
        var result = ResultAlgebra.AccumulateLinearHistory(m, Key(node), increments, lc);
        Assert.IsTrue(result.IsCurrent, string.Join(";", result.Diagnostics.Select(d => d.Message)));
        Assert.AreEqual(6, ((NodeResultDisplacement)result.Sample).ResultDisplacement.D1); Assert.AreEqual("stage-2", result.Sample.State.Phase);
        ResultAlgebra.Attach(result); Assert.AreEqual(DataStatus.Ready, ResultPreparation.Prepare(m, Key(node), result.Sample).Status);
        increments[1].State.IncrementIndex = 9; Assert.IsFalse(result.IsCurrent);
        Assert.IsFalse(ResultAlgebra.AccumulateLinearHistory(m, Key(node), increments, lc).IsAvailable);
    }

    [TestMethod]
    public void DerivedResult_InvalidatesWhenAnInputChangesAfterCalculation()
    {
        var (m, c) = Fixture(); var node = m.NodesElements[10]; var sample = (NodeResultForces)Sample(node);
        var result = ResultAlgebra.LinearCombination(m, Key(node), c, sample); Assert.IsTrue(result.IsCurrent);
        sample.ResultBeamForces.M1 += 1; Assert.IsFalse(result.IsCurrent);
        Assert.ThrowsException<InvalidOperationException>(() => ResultAlgebra.Attach(result));
    }
}
