using GPC.Converter;
using GPC.Examples;
using GPC.Geometry;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class ResultImportWorkflowTest
{
    private static (GPC.Model.Models.Model model, ResultImportBatch batch) Fixture()
    {
        var model = MixedModelFactory.Create(value => {
            value.AnalysisSource = new AnalysisSource { Program = "Synthetic", SolverVersion = "fixture-1", ModelRevision = "mixed-v1", AnalysisId = "static" };
            value.AreaElements[1090].Source = new SourceIdentity("Synthetic", "mixed-v1", EntityFamily.Shell, "B1"); // Same text ID, different family.
        });
        var batch = new ResultImportBatch { Source = model.AnalysisSource, DatasetId = "imported", SourceHash = "synthetic-record-hash",
            ReaderVersion = "fixture-reader-1", ExpectedInputFingerprint = model.AnalysisFingerprint(), Units = new ResultUnits(1000, 1000, 1e6),
            ShellDenominatorLengthToMm = 1000, ResolvedConvention = "GPC canonical positive section face / shell tensor", IsSynthetic = true, Semantics = AnalysisSemantics.LinearStatic };
        batch.Beams.Add(new BeamForceRecord { ElementId = "B1", Case = "P+", Record = "beam row 1", Axes = CoordinateSystem.Global,
            Values = new double?[] { 1, 2, 3, 4, 5, 6 }, Station = .5, PhysicalDistance = 1, StationDomain = "NodeToNode", Body = ActionBody.PositiveSectionFace,
            State = new ResultState { Semantics = AnalysisSemantics.LinearStatic, ConcomitantStateId = "P+", IsCombined = false,
                Components = Enumerable.Repeat(ComponentAvailability.Available, 6).ToArray() } });
        batch.Shells.Add(new ShellForceRecord { ElementId = "B1", Case = "P+", Record = "plate row 1", Axes = CoordinateSystem.Global,
            Values = new double?[] { 1, 2, 3, 4, 5, 6, 7, 8 }, Location = new Point2d(.3,.3), CoordinateKind = ResultCoordinateKind.Natural, PointKind = ShellResultPointKind.IntegrationPoint,
            State = new ResultState { Semantics = AnalysisSemantics.LinearStatic, ConcomitantStateId = "P+", IsCombined = false,
                Components = Enumerable.Repeat(ComponentAvailability.Available, 8).ToArray() } });
        return (model, batch);
    }

    [TestMethod]
    public void MixedResults_UnitsProvenanceAndFamilyIdentity_RoundTrip()
    {
        var (model, batch) = Fixture(); var before = model.AnalysisFingerprint();
        var report = ResultMapper.Import(model, batch);
        Assert.AreEqual(ImportStatus.Completed, report.Status, string.Join(";", report.Diagnostics.Select(d => d.Message)));
        Assert.AreEqual(2, report.ImportedSamples); Assert.AreEqual(before, model.AnalysisFingerprint());
        var select = new ResultSelection { Dataset = "imported", Case = "P+" };
        var beam = ResultQueries.Samples<StationResultBeamForces>(model.BeamElements[250], select).Single();
        var shell = ResultQueries.Samples<PointResultPlateForces>(model.AreaElements[1090], select).Single();
        Assert.AreEqual(1000, beam.ResultBeamForces.N); Assert.AreEqual(5e6, beam.ResultBeamForces.M1); Assert.AreEqual(1000, beam.PhysicalDistance);
        Assert.AreEqual(1, shell.Forces.Fxx); Assert.AreEqual(6000, shell.Forces.Mxx); Assert.AreEqual(1d, beam.State.Original.Values[0]);
        batch.Beams[0].Values[0] = 99; Assert.AreEqual(1d, beam.State.Original.Values[0]);
        using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0;
        var restored = ModelArchive.Load(stream);
        Assert.AreEqual("static", restored.AnalysisSource.AnalysisId);
        Assert.AreEqual(1d, ResultQueries.Samples<StationResultBeamForces>(restored.BeamElements[250], select).Single().State.Original.Values[0]);
    }

    [TestMethod]
    public void Results_RejectWrongAnalysisAndDuplicateDataset_WithoutMutation()
    {
        var (model, batch) = Fixture(); var count = model.BeamElements[250].Results.Count;
        batch.Source = new AnalysisSource { Program = "Synthetic", SolverVersion = "wrong", ModelRevision = "mixed-v1", AnalysisId = "static" };
        Assert.AreEqual(ImportStatus.Rejected, ResultMapper.Import(model, batch).Status);
        Assert.AreEqual(count, model.BeamElements[250].Results.Count);
        batch.Source = model.AnalysisSource; Assert.AreEqual(ImportStatus.Completed, ResultMapper.Import(model, batch).Status);
        Assert.AreEqual(ImportStatus.Rejected, ResultMapper.Import(model, batch).Status);
        Assert.AreEqual(count + 1, model.BeamElements[250].Results.Count);
    }

    [TestMethod]
    public void Results_LastBadRecordAndDuplicates_AreAtomic()
    {
        var (model, batch) = Fixture(); var count = model.BeamElements[250].Results.Count;
        batch.Shells[0].ElementId = "unknown";
        Assert.AreEqual(ImportStatus.Rejected, ResultMapper.Import(model, batch).Status);
        Assert.IsFalse(model.Datasets.ContainsKey(batch.DatasetId)); Assert.AreEqual(count, model.BeamElements[250].Results.Count);
        batch.Shells[0].ElementId = "B1"; batch.Beams.Add(batch.Beams[0]);
        Assert.AreEqual(ImportStatus.Rejected, ResultMapper.Import(model, batch).Status); Assert.AreEqual(count, model.BeamElements[250].Results.Count);
    }

    [TestMethod]
    public void Results_MissingComponentsRemainMissing_AndCancellationDoesNotAttach()
    {
        var (model, batch) = Fixture(); batch.Beams[0].Values[2] = null; batch.Beams[0].State.Components[2] = ComponentAvailability.NotExported;
        Assert.AreEqual(ImportStatus.Cancelled, ResultMapper.Import(model, batch, new CancellationToken(true)).Status);
        Assert.IsFalse(model.Datasets.ContainsKey(batch.DatasetId));
        Assert.AreEqual(ImportStatus.Partial, ResultMapper.Import(model, batch).Status);
        var sample = ResultQueries.Samples<StationResultBeamForces>(model.BeamElements[250], new ResultSelection { Dataset = "imported", Case = "P+" }).Single();
        Assert.IsTrue(double.IsNaN(sample.ResultBeamForces.V2)); Assert.IsNull(sample.State.Original.Values[2]);
        Assert.AreNotEqual(DataStatus.Ready, Verification.PrepareBeam(model, 250, sample, "test").Status);
    }
}
