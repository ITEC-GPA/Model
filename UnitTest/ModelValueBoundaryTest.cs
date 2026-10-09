using System.IO.Compression;
using GPC.Examples;
using GPC.Model.Core;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Structure;

namespace UnitTest;

[TestClass]
public class ModelValueBoundaryTest
{
    [TestMethod]
    public void DomainCopyPreservesSurfaceGraphAndRewiresGroupEventsIndependently()
    {
        var model = MixedModelFactory.Create();
        model.PhysicalSurfaces.Add("wall", new PhysicalSurfaceDefinition("wall", new[] { 1090, 1130 }));
        var copy = ModelValues.Copy(model);
        Assert.AreEqual(model.Guid, copy.Guid);
        Assert.AreEqual(model.AnalysisFingerprint(), copy.AnalysisFingerprint());
        Assert.AreEqual(model.VerificationFingerprint(null), copy.VerificationFingerprint(null));
        Assert.AreEqual(AnalysisCompatibility.Compatible, AnalysisCompatibilityValidator.Validate(copy).Status);
        Assert.AreSame(copy.NodesElements[10], copy.BeamElements[250].NodeI);
        Assert.AreNotSame(model.NodesElements[10], copy.NodesElements[10]);
        copy.Groups["Wall"].Name = "Renamed";
        Assert.IsTrue(copy.AreaElements.Values.All(p => p.Groups.ContainsKey("Renamed")));
        Assert.IsTrue(model.AreaElements.Values.All(p => p.Groups.ContainsKey("Wall")));
        copy.PhysicalSurfaces.Clear(); Assert.AreEqual(1, model.PhysicalSurfaces.Count);
    }
    [TestMethod]
    public void HistoricalArchiveCanBeEditedThroughDomainCopiesWithoutReplacingItsAnalysisIdentity()
    {
        using var file = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "model-v3-1.6.1.7.xml.gz"));
        using var zip = new GZipStream(file, CompressionMode.Decompress);
        var model = ModelArchive.Load(zip);
        var copy = ModelValues.Copy(model);
        Assert.AreEqual(model.Analysis.Id, copy.Analysis.Id);
        Assert.AreEqual(model.Analysis.InputFingerprint, copy.Analysis.InputFingerprint);
        Assert.AreEqual(AnalysisCompatibility.Compatible, AnalysisCompatibilityValidator.Validate(copy).Status);
        Assert.AreEqual(model.VerificationFingerprint("check"), copy.VerificationFingerprint("check"));
        copy.NodesElements[40].Position.Z += 1;
        Assert.AreEqual(AnalysisCompatibility.RequiresReanalysis, AnalysisCompatibilityValidator.Validate(copy).Status);
        Assert.AreEqual(AnalysisCompatibility.Compatible, AnalysisCompatibilityValidator.Validate(model).Status);
    }
}
