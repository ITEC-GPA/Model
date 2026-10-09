using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Analysis;
using GPC.Model.Checking.Preparation;
using GPC.Model.Core.Identity;
using GPC.Model.Results.Processing;
using GPC.Model.Results.Queries;

namespace UnitTest;

/// <summary>Combinations rebuilt from the static results read from a solver: a linear combination gives one state per location, an
/// envelope the governing alternatives as complete concomitant states, registered in the model for the checks.</summary>
[TestClass]
public class CombinationRebuildTest
{
    // SLU_1 = 1.35 G1 + 1.5 Q; ENV = envelope of Q and SLU_1.
    private static Dictionary<string, string> Json()
    {
        var json = CivilNxModelWorkflowTest.Json();
        json["LCOM-CONC"] = "{\"LCOM-CONC\":{\"1\":{\"NAME\":\"SLU_1\",\"ACTIVE\":\"STRENGTH\",\"iTYPE\":0,\"vCOMB\":[{\"ANAL\":\"ST\",\"LCNAME\":\"G1\",\"FACTOR\":1.35},{\"ANAL\":\"ST\",\"LCNAME\":\"Q\",\"FACTOR\":1.5}]},"
            + "\"2\":{\"NAME\":\"ENV\",\"ACTIVE\":\"STRENGTH\",\"iTYPE\":1,\"vCOMB\":[{\"ANAL\":\"ST\",\"LCNAME\":\"Q\",\"FACTOR\":1},{\"ANAL\":\"CBC\",\"LCNAME\":\"SLU_1\",\"FACTOR\":1}]}}}";
        return json;
    }
    private static CivilNxResponse Table(string head, params string[] rows) =>
        new("post/table", "{\"T\":{\"FORCE\":\"KN\",\"DIST\":\"M\",\"HEAD\":[" + head + "],\"DATA\":[" + string.Join(",", rows) + "]}}");
    private const string BeamHead = "\"Index\",\"Elem\",\"Load\",\"Part\",\"Axial\",\"Shear-y\",\"Shear-z\",\"Torsion\",\"Moment-y\",\"Moment-z\"";
    private static string Beam(string load, string part, string values) => "[\"1\",\"3\",\"" + load + "\",\"" + part + "\"," + values + "]";
    private static CivilNxResponse[] Tables(bool withQOnBeam = true)
    {
        var beamRows = new List<string>();
        foreach (var part in new[] { "I[2]", "1/4", "J[3]" })
        {
            beamRows.Add(Beam("G1", part, "\"10\",\"1\",\"2\",\"0.5\",\"3\",\"4\""));
            if (withQOnBeam) beamRows.Add(Beam("Q", part, "\"-2\",\"3\",\"-1\",\"1\",\"-6\",\"2\""));
        }
        return new[]
        {
            Table(BeamHead, beamRows.ToArray()),
            Table("\"Index\",\"Elem\",\"Load\",\"Node\",\"Fxx\",\"Fyy\",\"Fxy\",\"Fmax\",\"Fmin\",\"Angle\",\"Mxx\",\"Myy\",\"Mxy\",\"Mmax\",\"Mmin\",\"Angle\",\"Vxx\",\"Vyy\"",
                "[\"1\",\"10\",\"G1\",\"Cent\",\"1\",\"2\",\"3\",\"0\",\"0\",\"0\",\"4\",\"5\",\"6\",\"0\",\"0\",\"0\",\"7\",\"8\"]",
                "[\"2\",\"10\",\"Q\",\"Cent\",\"-1\",\"0.5\",\"2\",\"0\",\"0\",\"0\",\"-4\",\"1\",\"0\",\"0\",\"0\",\"0\",\"2\",\"-3\"]"),
            Table("\"Index\",\"Node\",\"Load\",\"FX\",\"FY\",\"FZ\",\"MX\",\"MY\",\"MZ\"", "[\"1\",\"1\",\"G1\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\"]", "[\"2\",\"1\",\"Q\",\"-1\",\"0\",\"1\",\"0\",\"2\",\"0\"]"),
            Table("\"Index\",\"Node\",\"Load\",\"DX\",\"DY\",\"DZ\",\"RX\",\"RY\",\"RZ\"", "[\"1\",\"3\",\"G1\",\"0.001\",\"0\",\"-0.002\",\"0\",\"0.0001\",\"0\"]",
                "[\"2\",\"3\",\"Q\",\"-0.001\",\"0.002\",\"0\",\"0\",\"0\",\"0.0002\"]"),
        };
    }
    private static GPC.Model.Models.Model Imported(Dictionary<string, string>? json = null, bool withQOnBeam = true)
    {
        json ??= Json();
        var m = CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(json), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile()).Model;
        var report = CivilNxResults.Import(m, CivilNxModelWorkflowTest.Snapshot(json), Tables(withQOnBeam), "static");
        Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        return m;
    }
    private static BeamElement Girder(GPC.Model.Models.Model m) => m.BeamElements.Values.Single(b => b.Source.OriginalId == "3");
    private static double[] Values(StationResultBeamForces s) { var f = s.ResultBeamForces; return new[] { f.N, f.V1, f.V2, f.T, f.M1, f.M2 }; }

    [TestMethod]
    public void LinearCombination_OneStatePerLocation_EqualToResultAlgebra_AndReadyForTheChecks()
    {
        var m = Imported(); var plan = new ResultFilter { Combinations = new[] { "SLU_1" } }.Resolve(m);
        CollectionAssert.AreEquivalent(new[] { "G1", "Q" }, plan.StaticCases.ToArray());
        var report = CombinationResults.Rebuild(m, plan, "static").Single();
        Assert.IsFalse(report.IsRejected, string.Join("; ", report.Diagnostics.Select(d => d.Message))); Assert.IsTrue(report.Attached);
        Assert.AreEqual(3 + 1 + 1 + 1, report.Locations, "Three beam stations, the plate centre, a reaction and a displacement."); Assert.AreEqual(6, report.Samples.Count);

        var girder = Girder(m); var select = new ResultSelection { Dataset = "static", Case = "SLU_1" };
        var quarter = ResultQueries.Samples<StationResultBeamForces>(girder, select).Single(s => s.ParametricDistance == .25);
        Assert.AreEqual((1.35 * 10 + 1.5 * -2) * 1000, quarter.ResultBeamForces.N, 1e-9);
        Assert.AreEqual((1.35 * -3 + 1.5 * 6) * 1e6, quarter.ResultBeamForces.M1, 1e-6, "M1 = -Moment-y.");
        Assert.AreEqual(AnalysisSemantics.LinearStatic, quarter.State.Semantics); Assert.AreEqual("linear:SLU_1", quarter.State.ConcomitantStateId);
        var template = ResultQueries.Samples<StationResultBeamForces>(girder, new ResultSelection { Dataset = "static", Case = "G1" }).Single(s => s.ParametricDistance == .25);
        var algebra = (StationResultBeamForces)ResultAlgebra.LinearCombination(m, new ElementKey { Family = EntityFamily.Beam, Id = girder.Id }, m.Combinations["SLU_1"], template).Sample;
        for (int k = 0; k < 6; k++) Assert.AreEqual(Values(algebra)[k], Values(quarter)[k], 1e-9 * Math.Max(1, Math.Abs(Values(algebra)[k])), "Same sums as ResultAlgebra.LinearCombination.");
        Assert.IsTrue(ResultAlgebra.HasCurrentDerivation(m, girder, quarter.State));
        var prepared = BeamActionPreparation.Prepare(m, girder.Id, quarter, "settings");
        var blocking = new[] { "MissingSample", "DatasetMismatchOrUnknownUnits", "IncompleteState", "NonConcomitantState", "StaleAnalysis" };
        Assert.IsFalse(prepared.Diagnostics.Any(d => blocking.Contains(d.Code)), string.Join("; ", prepared.Diagnostics.Select(d => d.Code + " " + d.Message)));

        var again = CombinationResults.Rebuild(m, plan, "static").Single();
        Assert.AreEqual(0, again.Samples.Count); Assert.IsTrue(again.Diagnostics.All(d => d.Code == "CombinationElementSkipped" && d.Message.Contains("DuplicateDerivedResult")));
        using var archive = new MemoryStream(); ModelArchive.Save(m, archive); archive.Position = 0; var copy = ModelArchive.Load(archive);
        var reopened = ResultQueries.Samples<StationResultBeamForces>(Girder(copy), select).Single(s => s.ParametricDistance == .25);
        Assert.IsTrue(ResultAlgebra.HasCurrentDerivation(copy, Girder(copy), reopened.State), "Derived from the archived static samples.");
    }

    [TestMethod]
    public void Envelope_RegistersEachGoverningAlternativeOnce_AsACompleteConcomitantState()
    {
        var m = Imported(); var envelope = m.Combinations["ENV"]; Assert.AreEqual(0, envelope.LoadCaseCount);
        var report = CombinationResults.Rebuild(m, new ResultFilter { Combinations = new[] { "ENV" } }.Resolve(m), "static").Single();
        Assert.IsFalse(report.IsRejected, string.Join("; ", report.Diagnostics.Select(d => d.Message))); Assert.AreEqual(2, report.Alternatives);
        var girder = Girder(m);
        var states = ResultQueries.Samples<StationResultBeamForces>(girder, new ResultSelection { Dataset = "static", Case = "ENV" }).Where(s => s.ParametricDistance == .25).ToArray();
        Assert.AreEqual(2, states.Length, "Both alternatives govern some component at this station.");
        Assert.IsTrue(states.All(s => s.State.Semantics == AnalysisSemantics.ConcomitantEnvelopeState && ReferenceEquals(s.Case, envelope)));
        var q = states.Single(s => s.State.ConcomitantStateId == "envelope:ENV:ENV=Q"); var slu = states.Single(s => s.State.ConcomitantStateId == "envelope:ENV:ENV=SLU_1");
        CollectionAssert.AreEqual(new[] { -2000d, 3000, -1000, 1e6, 6e6, 2e6 }, Values(q));
        double[] expected = { 10500, 5850, 1200, 2.175e6, 4.95e6, 8.4e6 }; var actual = Values(slu);
        for (int k = 0; k < 6; k++) Assert.AreEqual(expected[k], actual[k], 1e-6 * Math.Max(1, Math.Abs(expected[k])));
        StringAssert.Contains(q.State.Coverage, "max M1"); StringAssert.Contains(q.State.Coverage, "min N"); StringAssert.Contains(slu.State.Coverage, "max N");
        CollectionAssert.AreEquivalent(ResultQueries.Samples<StationResultBeamForces>(girder, new ResultSelection { Dataset = "static", Case = "Q" }).Where(s => s.ParametricDistance == .25).ToArray(),
            q.State.DerivedFrom, "The governing state keeps its static sources.");

        // Every component extreme is among the registered states, which are complete vectors of one alternative.
        foreach (var plate in m.AreaElements.Values.Where(a => a.Results.Any()))
        {
            var g1 = ResultQueries.Samples<PointResultPlateForces>(plate, new ResultSelection { Dataset = "static", Case = "G1" }).SingleOrDefault();
            if (g1 == null) continue;
            var qq = ResultQueries.Samples<PointResultPlateForces>(plate, new ResultSelection { Dataset = "static", Case = "Q" }).Single();
            double[] V(PointResultPlateForces p) => new[] { p.Forces.Fxx, p.Forces.Fyy, p.Forces.Fxy, p.Forces.Fxz, p.Forces.Fyz, p.Forces.Mxx, p.Forces.Myy, p.Forces.Mxy };
            var alternatives = new[] { V(qq), V(g1).Zip(V(qq), (a, b) => 1.35 * a + 1.5 * b).ToArray() };
            var registered = ResultQueries.Samples<PointResultPlateForces>(plate, new ResultSelection { Dataset = "static", Case = "ENV" }).Select(V).ToArray();
            for (int k = 0; k < 8; k++)
            {
                Assert.AreEqual(alternatives.Max(a => a[k]), registered.Max(a => a[k]), 1e-9); Assert.AreEqual(alternatives.Min(a => a[k]), registered.Min(a => a[k]), 1e-9);
            }
        }
    }

    [TestMethod]
    public void Rebuild_OnlyAtThePlannedElements_AndSkipsElementsWithIncompleteStaticResults()
    {
        var m = Imported(withQOnBeam: false);
        var plan = new ResultFilter { Elements = new ElementSelection { Groups = new[] { "Impalcato" }, Families = new[] { EntityFamily.Node, EntityFamily.Beam, EntityFamily.Shell } },
            Combinations = new[] { "SLU_1" } }.Resolve(m);
        var report = CombinationResults.Rebuild(m, plan, "static").Single();
        Assert.IsFalse(report.IsRejected);
        Assert.AreEqual("3", m.BeamElements.Values.Single(b => b.Id == report.Diagnostics.Single(d => d.Code == "CombinationElementSkipped").ElementId).Source.OriginalId);
        Assert.AreEqual(2, report.Locations, "The plate centre and the displacement of node 3; node 1 (reaction) is outside the group.");
        var support = m.NodesElements.Values.Single(n => n.Source.OriginalId == "1");
        Assert.AreEqual(0, ResultQueries.Samples<NodeResultForces>(support, new ResultSelection { Dataset = "static", Case = "SLU_1" }).Count);
    }

    [TestMethod]
    public void UndeclaredCombinations_AreDeclaredBeforeTheResults_AndUnknownInputsAreRejected()
    {
        var json = Json(); var m = CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(json), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile()).Model;
        m.Combinations.Remove("ENV"); // As imported before envelopes were declared.
        Assert.ThrowsException<InvalidOperationException>(() => CombinationResults.Rebuild(m, "ENV", m.AllElements, "static"));
        Assert.AreEqual(0, CombinationResults.Declare(m, new[] { "ENV", "SLU_1" }).Count); Assert.AreEqual(0, m.Combinations["ENV"].LoadCaseCount);
        Assert.AreEqual(ImportStatus.Completed, CivilNxResults.Import(m, CivilNxModelWorkflowTest.Snapshot(json), Tables(), "static").Status);
        Assert.IsFalse(CombinationResults.Rebuild(m, "ENV", m.AllElements, "static").IsRejected);
        Assert.AreEqual(0, CombinationResults.Declare(m, new[] { "LCOM-CONC/ENV" }).Count, "Already declared: nothing changes.");
        Assert.IsTrue(CombinationResults.Rebuild(m, "SLU_1", m.AllElements, "missing").IsRejected);
        Assert.ThrowsException<ArgumentException>(() => CombinationResults.Rebuild(m, "NOPE", m.AllElements, "static"));
        var stale = Imported(); stale.AreaElements.Values.First().Assignments.PhysicalThickness = 999;
        Assert.IsTrue(CombinationResults.Rebuild(stale, "SLU_1", stale.AllElements, "static").IsRejected, "Results no longer match the edited model.");
    }
}
