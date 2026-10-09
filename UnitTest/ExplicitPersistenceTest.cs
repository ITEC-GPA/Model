using GPC.Model.Models;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Xml.Linq;
using GPC.Examples;
using GPC.Model.Persistence;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Analysis;
using GPC.Model.Checking.Preparation;
using GPC.Model.Checking.Scenarios;
using GPC.Model.Core.Identity;
using GPC.Model.Results.Queries;

namespace UnitTest;

[TestClass]
public class ExplicitPersistenceTest
{
    [TestMethod]
    public void GenericInputIdentityIsIndependentOfAssemblyAndRuntimeVersion()
    {
        // Same value independently evaluated on PowerShell/.NET 10.0.11; this suite runs on .NET 6.
        var values = new List<ResultSelection> { new() { Dataset = "dataset", Case = "case" } };
        var map = new Dictionary<string, double> { ["k"] = 12.5 };
        Assert.AreEqual("3A0029CAAFB0C4F3D01A1F9891D12A99F63E2F81DC1A6993B364738065950A0B",
            ModelArchive.Fingerprint(new object[] { values, map }));
    }

    [TestMethod]
    public void RealVersion3FixtureMigratesToExplicitDocumentWithoutChangingAnalysis()
    {
        using var file = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "model-v3-1.6.1.7.xml.gz"));
        using var zip = new GZipStream(file, CompressionMode.Decompress);
        var original = ModelArchive.Load(zip);
        Assert.AreEqual(original.Analysis.InputFingerprint, original.AnalysisFingerprint());
        Assert.AreEqual(AnalysisCompatibility.Compatible, AnalysisCompatibilityValidator.Validate(original).Status);
        using var stream = new MemoryStream(); ModelArchive.SaveDocument(original, stream); stream.Position = 0;
        var xml = XDocument.Load(stream);
        Assert.AreEqual("4", xml.Root!.Attribute("version")!.Value);
        Assert.AreEqual("ModelDocument", xml.Root.Elements().Single().Name.LocalName);
        stream.Position = 0; var copy = ModelArchive.Load(stream);
        Assert.AreEqual(original.Guid, copy.Guid);
        Assert.AreEqual(original.AnalysisFingerprint(), copy.AnalysisFingerprint());
        Assert.AreEqual(original.VerificationFingerprint("test"), copy.VerificationFingerprint("test"));
        Assert.AreSame(copy.NodesElements[10], copy.BeamElements[250].NodeI);
        Assert.AreSame(copy.BeamProperties.Values.Single(), copy.BeamElements[250].BeamProperty);
        copy.Groups["Wall"].Name = "Renamed";
        Assert.IsTrue(copy.AreaElements.Values.All(a => a.Groups.ContainsKey("Renamed")));
        copy.NodesElements[40].Position.Z += 1;
        Assert.AreEqual(AnalysisCompatibility.RequiresReanalysis, AnalysisCompatibilityValidator.Validate(copy).Status);
        Assert.AreNotEqual(original.Analysis.InputFingerprint, copy.AnalysisFingerprint());
    }

    [TestMethod]
    public void DocumentRetainsScenariosAndRejectsUnknownSchema()
    {
        var source = MixedModelFactory.Create(); var scenario = VerificationScenario.Create(source.Analysis.Id, "Persisted");
        var prepared = VerificationPreparation.Prepare(source, scenario);
        using var stream = new MemoryStream(); ModelArchive.SaveDocument(prepared.Model, stream); stream.Position = 0;
        var copy = ModelArchive.Load(stream);
        Assert.IsTrue(copy.VerificationContext.IsCurrent(copy));
        Assert.AreEqual(scenario.Id, copy.VerificationScenarios.Single().Key);
        stream.Position = 0; var xml = XDocument.Load(stream);
        xml.Descendants().Single(e => e.Name.LocalName == "Schema").Value = "99";
        using var changed = new MemoryStream(); xml.Save(changed); changed.Position = 0;
        Assert.ThrowsException<SerializationException>(() => ModelArchive.Load(changed));
    }

    [TestMethod]
    public void IndexedStoreMatchesLegacyQueriesAndDoesNotShareMutableSamples()
    {
        var model = MixedModelFactory.Create(); var store = InMemoryResultStore.Capture(model);
        Assert.AreEqual(model.AllElements.Sum(e => e.Results.Sum(r => r.Results.Count)), store.Records.Count);
        foreach (var element in model.AllElements)
            foreach (var loadCase in model.LoadCases.Values)
            {
                var selection = new ResultSelection { Dataset = "synthetic-static", Case = loadCase.Name };
                var legacy = ResultQueries.Samples<ResultLocation>(element, selection);
                var indexed = ResultQueries.Samples<ResultLocation>(store, element.Guid, selection);
                CollectionAssert.AreEqual(legacy.Select(s => ModelArchive.Fingerprint(new object[] { s })).ToArray(),
                    indexed.Select(s => ModelArchive.Fingerprint(new object[] { s })).ToArray());
            }
        var beam = store.Records.First(r => r.Address.Family == EntityFamily.Beam);
        var copy = beam.Read<StationResultBeamForces>(); var original = copy.ResultBeamForces.N;
        copy.ResultBeamForces.N += 100; model.BeamElements[250].Results.Clear();
        Assert.AreEqual(original, beam.Read<StationResultBeamForces>().ResultBeamForces.N);
        Assert.AreEqual(1, store.At(beam.Address).Count);
    }

    [TestMethod]
    public void DuplicateSamplesAndDistinctCutsArePreserved()
    {
        var model = MixedModelFactory.Create(); var element = model.BeamElements[250];
        var results = element.Results.First().Results;
        var sample = results.OfType<StationResultBeamForces>().First();
        var store = InMemoryResultStore.Capture(model);
        var record = store.Records.First(r => r.Address.ElementGuid == element.Guid);
        results.Add(sample);
        Assert.AreEqual(2, InMemoryResultStore.Capture(model).At(record.Address).Count);
        var differentSide = record.Read<StationResultBeamForces>(); differentSide.Side = SectionSide.Right;
        results.Add(differentSide);
        var changed = InMemoryResultStore.Capture(model);
        Assert.AreEqual(2, changed.At(record.Address).Count);
        Assert.AreEqual(store.Records.Count + 2, changed.Records.Count);
    }

    [TestMethod]
    public void ArchiveVocabularyIsExplicitAndCoversExistingPublicData()
    {
        var domain = typeof(GPC.Model.Models.Model).Assembly.GetTypes().Where(t => t.IsVisible && !t.IsAbstract && !t.ContainsGenericParameters
            && (t.IsSerializable || t.IsEnum) && t.Namespace != null && !t.Namespace.StartsWith("GPC.Model.FiniteElementAnalysis"));
        CollectionAssert.AreEquivalent(domain.ToArray(), ArchiveContractRegistry.SupportedTypes.ToArray());
    }
}
