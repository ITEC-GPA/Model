using GPC.Model.Models;
using GPC.Converter;
using GPC.Examples;
using GPC.Geometry;
using GPC.Model;
using GPC.Model.Attributes;
using GPC.Model.Combinations;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Persistence;
using GPC.Model.Results;
using GPC.Model.Results.Storage;
using GPC.Model.Results.Locations;
using GPC.Model.Sections.Concrete;
using GPC.Model.Stages;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using GPC.Model.Analysis;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Reports;
using GPC.Model.Compatibility;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Results.State;
using GPC.Model.Structure.Assignments;

namespace UnitTest
{
    [TestClass]
    public class MixedWorkflowTest
    {
        [TestMethod]
        public void FullSyntheticExampleRoundTripAndRevisions()
        {
            var m = MixedModelFactory.Create(); m.Stages.Add(new Stage(40, "Stage 1") { IsCumulative = true });
            var fingerprint = m.AnalysisFingerprint();
            using var s = new MemoryStream(); ModelArchive.Save(m, s); s.Position = 0; var copy = ModelArchive.Load(s);
            Assert.AreEqual(fingerprint, copy.AnalysisFingerprint());
            Assert.AreEqual(10, copy.BeamElements[250].Results.Sum(r => r.Results.Count));
            Assert.AreEqual(2, copy.NodesElements[40].Loads.Count);
            Assert.AreEqual(2, copy.AreaElements.Values.First().Assignments.Layers.Count);
            Assert.AreEqual(4, ((ReinforcedConcreteSection)copy.BeamElements[250].BeamProperty).Rebars.Count());
            Assert.AreEqual(true, copy.Stages[40].IsCumulative);
            Assert.AreEqual(BeamConnectionKind.Released, ((BeamReleasesAttribute)copy.BeamElements[250].Attributes.Values.Single()).J[5].Kind);
            Assert.AreSame(copy.Groups["Wall"], copy.AreaElements.Values.First().Groups["Wall"]);
        }
        [TestMethod]
        public void CantileverSignedAnalyticalSamplesAndReactions()
        {
            var m = MixedModelFactory.Create(); var b = m.BeamElements[250];
            var i = Verification.BeamSample(b, "synthetic-static", "P+", 0, SectionSide.Unspecified);
            var mid = Verification.BeamSample(b, "synthetic-static", "P+", .5, SectionSide.Unspecified);
            Assert.AreEqual(-2000000, i.ResultBeamForces.M1, 1e-7); Assert.AreEqual(1000, i.ResultBeamForces.V2, 1e-10);
            Assert.AreEqual(-1000000, mid.ResultBeamForces.M1, 1e-7);
            var reaction = (NodeResultForces)m.NodesElements[10].Results[0].Results[0];
            Assert.AreEqual(-1000, reaction.Fy, 1e-10); Assert.AreEqual(2000000, reaction.Mx, 1e-7);
            Assert.AreEqual(NodalForceKind.SupportReaction, reaction.Kind); Assert.AreEqual(ActionBody.OnNode, reaction.Body);
            Assert.IsNull(Verification.BeamSample(b, "synthetic-static", "P+", .123, SectionSide.Unspecified));
        }
        [TestMethod]
        public void InternalMaximumAndShearDiscontinuityRemainDistinct()
        {
            var m = MixedModelFactory.Create(); var b = m.BeamElements[250]; var lc = m.LoadCases["P+"];
            // Simply supported beam fixture: q=2 N/mm, L=4000 mm. Assigned analytical values only.
            b.Results.Clear();
            b.AddResult(new BeamResult(new[]{
                new StationResultBeamForces(lc,new ResultBeamForces(0,0,4000,0,0,0,CoordinateSystem.Global),0),
                new StationResultBeamForces(lc,new ResultBeamForces(0,0,0,0,4000000,0,CoordinateSystem.Global),.5),
                new StationResultBeamForces(lc,new ResultBeamForces(0,0,-4000,0,0,0,CoordinateSystem.Global),1)}));
            Assert.AreEqual(4000000, b.Results.SelectMany(r => r.Results).Cast<StationResultBeamForces>().Max(r => r.ResultBeamForces.M1), 1e-7);
            var left = new StationResultBeamForces(lc, new ResultBeamForces(0, 0, 1000, 0, 500000, 0, CoordinateSystem.Global), .5) { Side = SectionSide.Left, State = new ResultState { DatasetId = "jump" } };
            var right = new StationResultBeamForces(lc, new ResultBeamForces(0, 0, -1000, 0, 500000, 0, CoordinateSystem.Global), .5) { Side = SectionSide.Right, State = new ResultState { DatasetId = "jump" } };
            b.AddResult(new BeamResult(new[] { left, right }));
            Assert.AreSame(left, Verification.BeamSample(b, "jump", "P+", .5, SectionSide.Left));
            Assert.AreSame(right, Verification.BeamSample(b, "jump", "P+", .5, SectionSide.Right));
            using var s = new MemoryStream(); ModelArchive.Save(m, s); s.Position = 0; var copy = ModelArchive.Load(s);
            Assert.AreEqual(-1000, Verification.BeamSample(copy.BeamElements[250], "jump", "P+", .5, SectionSide.Right).ResultBeamForces.V2, 1e-10);
        }
        [TestMethod]
        public void MissingNonConcomitantAndStaleStatesNeverBecomePass()
        {
            var m = MixedModelFactory.Create(); var sample = Verification.BeamSample(m.BeamElements[250], "synthetic-static", "P+", .5, SectionSide.Unspecified);
            Assert.AreEqual(DataStatus.Ready, Verification.PrepareBeam(m, 250, sample, "test").Status);
            sample.State.Semantics = AnalysisSemantics.IndependentExtrema;
            var prepared = Verification.PrepareBeam(m, 250, sample, "test"); Assert.IsNull(prepared.Input);
            Assert.IsTrue(prepared.Diagnostics.Any(d => d.Code == "NonConcomitantState"));
            sample.State.Semantics = AnalysisSemantics.LinearStatic; sample.State.Components[0] = ComponentAvailability.NotExported;
            Assert.IsNull(Verification.PrepareBeam(m, 250, sample, "test").Input);
            sample.State.Components[0] = ComponentAvailability.Available; m.NodesElements[40].Position.Z = 2200;
            Assert.AreEqual(DataStatus.Stale, Verification.PrepareBeam(m, 250, sample, "test").Status);
        }
        [TestMethod]
        public void RebarEditInvalidatesChecksWithoutInvalidatingFemForces()
        {
            var m = MixedModelFactory.Create(); var b = m.BeamElements[250]; var sample = Verification.BeamSample(b, "synthetic-static", "P+", .5, SectionSide.Unspecified);
            var oldAnalysis = m.AnalysisFingerprint(); var oldVerification = m.VerificationFingerprint("test");
            var prepared = Verification.PrepareBeam(m, 250, sample, "test");
            b.Assignments.Sections[0].Section.Rebars.First().Position.X = 60;
            Assert.AreEqual(oldAnalysis, m.AnalysisFingerprint()); Assert.AreNotEqual(oldVerification, m.VerificationFingerprint("test"));
            Assert.AreEqual(DataStatus.Stale, Verification.Run(prepared, CheckMechanism.UlsBiaxialSection, null).Data);
            Assert.AreEqual(-1000000, sample.ResultBeamForces.M1, 1e-7);
        }
        [TestMethod]
        public void SectionTransitionsUseSideAndDoNotSwapRebarsWithMomentSign()
        {
            var m = MixedModelFactory.Create(); var b = m.BeamElements[250]; var first = b.Assignments.Sections[0].Section;
            var second = new ReinforcedConcreteSection(first.ConcreteShape, first.ConcreteMaterial, "Second section"); second.AddRebars(first.Rebars.Take(2));
            b.Assignments.Sections.Clear(); b.Assignments.Sections.Add(new BeamSectionAssignment { Start = 0, End = .5, Section = first });
            b.Assignments.Sections.Add(new BeamSectionAssignment { Start = .5, End = 1, Section = second });
            Assert.AreSame(first, b.Assignments.SectionAt(.5, SectionSide.Left)); Assert.AreSame(second, b.Assignments.SectionAt(.5, SectionSide.Right));
            Assert.ThrowsException<InvalidOperationException>(() => b.Assignments.SectionAt(.5, SectionSide.Unspecified));
            Assert.AreEqual(4, first.Rebars.Count()); Assert.AreEqual(2, second.Rebars.Count());
        }
        [TestMethod]
        public void MissingEngineCancellationAndSkippedMechanismsAreExplicit()
        {
            var m = MixedModelFactory.Create(); var sample = Verification.BeamSample(m.BeamElements[250], "synthetic-static", "P+", .5, SectionSide.Unspecified);
            var prepared = Verification.PrepareBeam(m, 250, sample, "test");
            Assert.AreEqual(DataStatus.MissingDependency, Verification.Run(prepared, CheckMechanism.UlsBiaxialSection, null).Data);
            var cancelled = Verification.Run(prepared, CheckMechanism.UlsBiaxialSection, null, new CancellationToken(true));
            Assert.AreEqual(ExecutionStatus.Cancelled, cancelled.Execution); Assert.AreEqual(EngineeringOutcome.NotEvaluated, cancelled.Outcome);
            var report = CheckRunner.Beam(m, 250, "synthetic-static", "P+", "test", new[] { CheckMechanism.UlsBiaxialSection, CheckMechanism.Shear }, new RecordingVerifier());
            Assert.AreEqual(10, report.Required); Assert.AreEqual(5, report.Executed); Assert.AreEqual(5, report.Excluded);
            Assert.AreEqual(EngineeringOutcome.NotEvaluated, report.Outcome);
            Assert.AreEqual(0, report.Governing(CheckMechanism.UlsBiaxialSection).Station ?? throw new AssertFailedException("Missing governing station."), 1e-12);
        }
        private sealed class RecordingVerifier : IConcreteSectionVerifier
        {
            public string Version => "TEST ONLY";
            public IReadOnlyCollection<CheckMechanism> Capabilities => new[] { CheckMechanism.UlsBiaxialSection };
            public CheckResult Verify(BeamCheckInput input, CheckMechanism mechanism, CancellationToken token) => new CheckResult
            {
                Execution = ExecutionStatus.Completed,
                Data = DataStatus.Ready,
                Outcome = EngineeringOutcome.Satisfied,
                Utilization = Math.Abs(input.Forces.M1) / 4000000
            };
        }
        [TestMethod]
        public void SapArrayMappingHandlesTextIdsSharedNodesAndScientificNotation()
        {
            var joints = new[] { "n-A", "GLOBAL", "Cartesian", "0", "0", "", "0", "No", "", "40", "GLOBAL", "Cartesian", "0", "0", "", "2e0", "No", "", "90", "GLOBAL", "Cartesian", "1e0", "0", "", "0", "No", "" };
            var batch = SapEditingTables.Read(joints, new[] { "n-A", "n-A", "40", "", "" }, new[] { "S", "n-A", "40", "90", "", "" }, "model/rev1", "legacy-array-fixture", 1000, true, CultureInfo.InvariantCulture);
            var report = ModelMapper.Map(batch); Assert.AreEqual(ImportStatus.Partial, report.Status); Assert.IsFalse(report.VerificationEnabled);
            var b = report.Model.BeamElements.Values.Single(); var a = report.Model.AreaElements.Values.Single();
            Assert.AreSame(b.NodeI, a.Nodes[0]); Assert.AreEqual(2000, b.EndPoint.Z, 1e-10);
            Assert.AreEqual(EntityFamily.Beam, b.Source.Family); Assert.AreEqual("n-A", b.Source.OriginalId);
            Assert.ThrowsException<FormatException>(() => SapEditingTables.Read(new[] { "incomplete" }, Array.Empty<string>(), Array.Empty<string>(), "m", "v", 1, true, CultureInfo.InvariantCulture));
        }
        [TestMethod]
        public void ImportCollisionCancellationAndMismatchedAnalysisAreRejected()
        {
            var batch = new ImportBatch { Program = "Synthetic", ModelRevision = "r1", AnalysisId = "A" };
            batch.Nodes.Add(new NodeRecord { Id = "10", GlobalPosition = new Point3d(0, 0, 0), Record = "1" });
            batch.Nodes.Add(new NodeRecord { Id = "10", GlobalPosition = new Point3d(1, 0, 0), Record = "2" });
            var rejected = ModelMapper.Map(batch); Assert.AreEqual(ImportStatus.Rejected, rejected.Status); Assert.IsNull(rejected.Model); Assert.AreEqual("2", rejected.Diagnostics[0].Record);
            Assert.AreEqual(ImportStatus.Cancelled, ModelMapper.Map(batch, new CancellationToken(true)).Status);
            Assert.IsFalse(SourceEvidence.SameAnalysis(batch, new ImportBatch { Program = "Synthetic", ModelRevision = "r2", AnalysisId = "A" }));
            using var source = new MemoryStream(Encoding.UTF8.GetBytes("uninterpreted real-format placeholder"));
            var unsupported = new SolverFileAdapter("MIDAS Civil NX").Import(source);
            Assert.AreEqual(ImportStatus.Rejected, unsupported.Status); Assert.IsNull(unsupported.Model);
            Assert.AreEqual("uninterpreted real-format placeholder", Encoding.UTF8.GetString(Convert.FromBase64String(unsupported.Preserved[0].RawData)));
        }
        [TestMethod]
        public void ResultEqualityIncludesCaseAndPlateStressRetainsLegacyData()
        {
            var force = new ResultBeamForces(1, 2, 3, 4, 5, 6, CoordinateSystem.Global);
            Assert.AreNotEqual(new StationResultBeamForces(new LoadCaseBase("A"), force, .5), new StationResultBeamForces(new LoadCaseBase("B"), force, .5));
            var stress = new ResultStress(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6);
            var legacy = new PointResultPlateStress(new LoadCaseBase("A"), stress, Point2d.Origin);
            Assert.AreSame(stress, legacy.LegacyUnlocatedStress); Assert.IsNull(legacy.ResultPlateStress);
            var typed = new ResultPlateStress(CoordinateSystem.Global, stress, stress, stress);
            Assert.AreSame(typed, new PointResultPlateStress(new LoadCaseBase("A"), typed, Point2d.Origin).ResultPlateStress);
        }
        [TestMethod]
        public void SparseAreaCombinationQueryUsesIdsAsKeys()
        {
            var m = PostProcessingTest.Mixed(); var combination = new Combination("C");
            var r = new PointResultPlateForces(combination, new ResultPlateForces(CoordinateSystem.Global, 1, 2, 3, 4, 5, 6, 7, 8), Point2d.Origin, "centroid");
            m.AreaElements[10].AddResult(new PlateElementResult(new List<IPlateResultLocation> { r }));
            CollectionAssert.AreEqual(new ResultLocation[] { r }, m.GetCombinationAreaResults(combination));
        }
    }
}
