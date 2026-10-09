using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text.Json;
using GPC.Examples;
using GPC.Model.Core;
using GPC.Model.ElementProperties;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Structure;

if (args.Length != 2 || args[0] != "capture") throw new ArgumentException("capture <new-output-directory>; never overwrite an existing reference");
var folder = Path.GetFullPath(args[1]);
if (Directory.Exists(folder)) throw new InvalidOperationException("Reference directory already exists; capture is not a test update operation.");
Directory.CreateDirectory(folder);
var model = MixedModelFactory.Create(m => { foreach (var p in m.AreaElements.Values) PlateSections.UpgradeLegacy(p); });
model.PhysicalSurfaces.Add("wall", new PhysicalSurfaceDefinition("wall", new[] {1090, 1130}, new[] {new SurfaceZoneDefinition("left", new[]{1090})}));
var scenario = VerificationScenario.Create(model.Analysis.Id, "Before namespace migration");
scenario.SetShellSection(model.AreaElements[1090].Guid, model.AreaElements[1090].PlateProperty);
model = VerificationPreparation.Prepare(model, scenario).Model;
using (var stream = File.Create(Path.Combine(folder,"model.xml.gz")))
using (var zip = new GZipStream(stream,CompressionLevel.Optimal)) ModelArchive.SaveDocument(model,zip);
var fingerprints = new Dictionary<string,string> {
    ["ModelVersion"] = typeof(GPC.Model.Models.Model).Assembly.GetName().Version!.ToString(),
    ["Analysis"] = model.AnalysisFingerprint(), ["Verification"] = model.VerificationFingerprint("migration"),
    ["Scenario"] = scenario.Fingerprint,
    ["Generics"] = ModelValues.Fingerprint(new object[] { new List<SectionSide>{SectionSide.Left,SectionSide.Right},
        new Dictionary<string,ShellAssignments[]> { ["shell"] = new[]{ new ShellAssignments() } } }) };
File.WriteAllText(Path.Combine(folder,"fingerprints.json"),JsonSerializer.Serialize(fingerprints,new JsonSerializerOptions{WriteIndented=true}));
var exporter = new XsdDataContractExporter();
var contracts = ModelValues.DataContracts.OrderBy(t=>t.FullName,StringComparer.Ordinal).Select(t=>new {
    Type=t.FullName, Name=exporter.GetSchemaTypeName(t).Name, Namespace=exporter.GetSchemaTypeName(t).Namespace }).ToArray();
File.WriteAllText(Path.Combine(folder,"contracts.json"),JsonSerializer.Serialize(contracts,new JsonSerializerOptions{WriteIndented=true}));
Console.WriteLine($"Captured {contracts.Length} contracts and a Model {fingerprints["ModelVersion"]} archive in {folder}");
