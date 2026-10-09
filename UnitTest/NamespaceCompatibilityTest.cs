using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using GPC.Model.Core;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;

namespace UnitTest;

[TestClass]
public class NamespaceCompatibilityTest
{
    private static string Fixture(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", "Namespaces-v3", name);
    private static Dictionary<string,string> Expected() => JsonSerializer.Deserialize<Dictionary<string,string>>(File.ReadAllText(Fixture("fingerprints.json")))!;
    private static string StableType(string name) => Regex.Replace(name, @", Version=[^,\]]+, Culture=[^,\]]+, PublicKeyToken=[^\]]+", "");
    public sealed class ContractName { public string Type { get; set; } = ""; public string Name { get; set; } = ""; public string Namespace { get; set; } = ""; }
    [TestMethod]
    public void EveryPreviouslyKnownXmlContractRetainsItsQualifiedName()
    {
        var expected = JsonSerializer.Deserialize<ContractName[]>(File.ReadAllText(Fixture("contracts.json")))!;
        var actual = ModelValues.DataContracts.ToDictionary(t => StableType(HistoricalTypeNames.For(t)));
        var exporter = new XsdDataContractExporter();
        foreach (var contract in expected)
        {
            Assert.IsTrue(actual.TryGetValue(StableType(contract.Type), out var type), contract.Type);
            var name = exporter.GetSchemaTypeName(type!);
            Assert.AreEqual(contract.Name, name.Name, contract.Type);
            Assert.AreEqual(contract.Namespace, name.Namespace, contract.Type);
        }
    }
    [TestMethod]
    public void Model3ArchiveRetainsAnalysisScenarioAndVerificationIdentities()
    {
        using var file = File.OpenRead(Fixture("model.xml.gz")); using var zip = new GZipStream(file, CompressionMode.Decompress);
        var model = ModelArchive.Load(zip); var expected = Expected();
        Assert.AreEqual("3.0.0.0", expected["ModelVersion"]);
        Assert.AreEqual(expected["Analysis"], model.AnalysisFingerprint());
        Assert.AreEqual(expected["Verification"], model.VerificationFingerprint("migration"));
        Assert.AreEqual(expected["Scenario"], model.VerificationScenarios.Values.Single().Fingerprint);
        Assert.AreEqual(AnalysisCompatibility.Compatible, AnalysisCompatibilityValidator.Validate(model).Status);
        Assert.IsTrue(model.VerificationContext.IsCurrent(model));
        Assert.AreEqual(2, model.PhysicalSurfaces["wall"].ResolveElements(model).Count);
        var copy = ModelValues.Copy(model);
        Assert.AreEqual(expected["Verification"], copy.VerificationFingerprint("migration"));
        Assert.AreEqual(AnalysisCompatibility.Compatible, AnalysisCompatibilityValidator.Validate(copy).Status);
    }
    [TestMethod]
    public void EnumArrayAndGenericFingerprintsKeepTheirHistoricalTypeIdentities()
    {
        Assert.AreEqual(Expected()["Generics"], ModelValues.Fingerprint(new object[] {
            new List<SectionSide>{SectionSide.Left,SectionSide.Right},
            new Dictionary<string,ShellAssignments[]> { ["shell"] = new[]{new ShellAssignments()} } }));
    }
}
