using GPC.Model.Models;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using GPC.Model.Core;
using GPC.Model.Persistence;
using GPC.Model.Analysis;
using GPC.Model.Results.Locations;
using GPC.Model.Structure.Assignments;

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
        var differences = new List<string>();
        foreach (var contract in expected)
        {
            Assert.IsTrue(actual.TryGetValue(StableType(contract.Type), out var type), contract.Type);
            var name = XmlContractNames.Historical(exporter.GetSchemaTypeName(type!));
            if (contract.Name != name.Name || contract.Namespace != name.Namespace) differences.Add(contract.Type + " : " + name);
        }
        Assert.AreEqual(0, differences.Count, string.Join(Environment.NewLine, differences));
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
    [TestMethod]
    public void ArchiveRoundTripPreservesUserTextThatLooksLikeNamespaceMetadata()
    {
        using var file = File.OpenRead(Fixture("model.xml.gz"));
        using var zip = new GZipStream(file, CompressionMode.Decompress);
        var model = ModelArchive.Load(zip);
        const string text = "http://schemas.datacontract.org/2004/07/GPC.Model.Results.Storage";
        model.Name = text;
        using var output = new MemoryStream();
        ModelArchive.SaveDocument(model, output);
        output.Position = 0;
        var xml = System.Xml.Linq.XDocument.Load(output);
        var xsi = System.Xml.Linq.XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");
        foreach (var element in xml.Descendants().Where(e => e.Attribute(xsi + "type") != null))
        {
            var value = element.Attribute(xsi + "type")!.Value;
            int colon = value.IndexOf(':');
            var ns = colon < 0 ? element.GetDefaultNamespace() : element.GetNamespaceOfPrefix(value[..colon]);
            Assert.IsFalse(ns!.NamespaceName.EndsWith("GPC.Model.Results.Storage"), "New CLR names leaked into archive type metadata.");
        }
        output.Position = 0;
        var copy = ModelArchive.Load(output);
        Assert.AreEqual(text, copy.Name);
        Assert.AreEqual(model.VerificationFingerprint("migration"), copy.VerificationFingerprint("migration"));
        Assert.IsTrue(copy.VerificationContext.IsCurrent(copy));
    }
    [TestMethod]
    public void NamespaceMigrationDoesNotRewriteWhitespaceOrOpaqueStringValues()
    {
        var value = new GPC.Model.Core.Identity.SourceIdentity(
            "http://schemas.datacontract.org/2004/07/GPC.Model.Core.Identity", "gpcMigration0:SourceIdentity", GPC.Model.Core.Identity.EntityFamily.Node, "original");
        var serializer = ModelValues.Serializer(typeof(object));
        using var output = new MemoryStream();
        serializer.WriteObject(output, value);
        output.Position = 0;
        var copy = (GPC.Model.Core.Identity.SourceIdentity)serializer.ReadObject(output);
        Assert.AreEqual(value, copy);
        var strings = new[] { "   ", "\r\n\t", "gpcMigration0:CheckScope", "GPC.Model.Results.Storage.NodeResult" };
        serializer = ModelValues.Serializer(typeof(string[]));
        using var text = new MemoryStream();
        serializer.WriteObject(text, strings); text.Position = 0;
        CollectionAssert.AreEqual(strings, (string[])serializer.ReadObject(text));
    }
    [TestMethod]
    public void HistoricalTypesStillResolveWhenDocumentUsesTheMigrationPrefix()
    {
        using var file = File.OpenRead(Fixture("model.xml.gz")); using var zip = new GZipStream(file, CompressionMode.Decompress);
        var xml = System.Xml.Linq.XDocument.Load(zip);
        xml.Root!.SetAttributeValue(System.Xml.Linq.XNamespace.Xmlns + "gpcMigration0", "urn:customer:existing-prefix");
        using var data = new MemoryStream(); xml.Save(data); data.Position = 0;
        var model = ModelArchive.Load(data);
        Assert.AreEqual(Expected()["Analysis"], model.AnalysisFingerprint());
        Assert.AreEqual(AnalysisCompatibility.Compatible, AnalysisCompatibilityValidator.Validate(model).Status);
    }
    [TestMethod]
    public void PublicDomainTypesUseTheCurrentNamespaceLayout()
    {
        var forbidden = new[] { "GPC.Model.PostProcessing", "GPC.Model.Costrains", "GPC.Model.Restrains", "GPC.Model.Results.ElementResults", "GPC.Model.Results.ResultLocations" };
        var remaining = typeof(GPC.Model.Models.Model).Assembly.GetExportedTypes().Where(t => forbidden.Contains(t.Namespace)).Select(t => t.FullName).ToArray();
        Assert.AreEqual(0, remaining.Length, string.Join("\n", remaining));
    }

}
