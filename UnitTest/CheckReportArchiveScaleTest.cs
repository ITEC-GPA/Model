using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using GPC.Geometry;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;
using GPC.Model.Checking.Reports;
using GPC.Model.Compatibility;
using GPC.Model.Core;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Persistence;
using GPC.Model.Results;

namespace UnitTest;

/// <summary>Report archives beyond one XML document (2,000,000 objects, 256 Mi characters) are saved as a package of such
/// documents, each read within the same ceilings and within those its size allows; the text of versions 1-5 is written and
/// read as before.</summary>
[TestClass]
public class CheckReportArchiveScaleTest
{
    /// <summary>A steel section row as the native pipelines store it: metrics, trace, input snapshot with its axes,
    /// standard, diagnostics and plan identity (about 180 objects with one trace entry, five more for each further entry).</summary>
    private static CheckResult Row(int i, int trace)
    {
        var forces = new ResultBeamForces(-200000 - i, 0, 0, 0, 20e6 + 1e4 * i, 3e6, ResultTransformations.AtPoint(CoordinateSystem.Global, new Point3d(3000.0 * (i / 4), 0, 500)));
        double utilization = .2 + (i % 97) / 200.0;
        var row = new CheckResult
        {
            SchemaVersion = 2, Execution = ExecutionStatus.Completed, Data = DataStatus.Ready, Outcome = EngineeringOutcome.Satisfied,
            Mechanism = CheckMechanism.UlsBiaxialSection, Utilization = utilization, EngineVersion = "fixture-2.0", EngineConfiguration = "fixture-configuration",
            Settings = "fixture settings", Dataset = "columns", Case = "C" + i % 4, ConcomitantStateId = "C" + i % 4, ElementId = 10 * (i / 4 + 1),
            Family = EntityFamily.Beam, Station = .5, Target = new CheckTargetReference(10 * (i / 4 + 1)), Scope = CheckScope.SectionSample,
            PlanItemId = "item-" + i, MethodId = "Fixture.Pipeline.Section", GroupNames = new[] { "Steel" }, SampleRevision = "sample-" + i, VerificationRevision = "revision",
            Standard = new CheckStandardContext("EN 1993-1-1", "2005", null, "Fixture.Pipeline.Section", "configuration", "Selected section checks at one concomitant cut."),
            Input = new CheckInputSnapshot("section", "sample-" + i, "forces-" + i, new BeamForceSnapshot(forces)),
            Details = new NativeMethodDetails("Fixture.Pipeline.Section", "Section checks at one cut", "native-" + i,
                Enumerable.Range(0, 6).Select(m => new CheckMetric("Metric." + m, 1000.0 * m + i, 5000.0 * (m + 1), "N", utilization * (m + 1) / 6, true)),
                Enumerable.Range(0, trace).Select(t => new CheckCalculationValue("Trace." + t, i + t / 8.0, t % 2 == 0 ? "N" : "1",
                    "EN 1993-1-1:2005 6.2.9.1; plastic resistance of the class 1 cross-section, step " + t)),
                new[] { "External classifications are caller declarations." })
        };
        row.Diagnostics.Add(ModelDiagnostic.Error("Fixture.Advice", message: "Row " + i));
        row.SealEvidence();
        return row;
    }

    private static CheckReport Report(string job, IEnumerable<CheckResult> results)
    {
        var rows = results.ToArray(); foreach (var r in rows) { r.Job = job; r.SealEvidence(); }
        var report = CheckReport.ForSingleResult(rows[0]); report.Results.AddRange(rows.Skip(1));
        report.Required = rows.Length; report.SealScope();
        return report;
    }

    private static byte[] Save(params CheckReport[] reports) { using var stream = new MemoryStream(); CheckReportArchive.Save(reports, stream); return stream.ToArray(); }
    private static IReadOnlyList<CheckReport> Load(byte[] bytes) { using var stream = new MemoryStream(bytes); return CheckReportArchive.Load(stream); }
    private static bool IsPackage(byte[] bytes) => bytes.Length > 4 && bytes[0] == 0x50 && bytes[1] == 0x4B && bytes[2] == 3 && bytes[3] == 4;

    /// <summary>The text of versions 1-5 as the code before the package wrote it (same writer, serializer and ceiling).</summary>
    private static byte[] LegacyText(CheckReport[] reports, string version)
    {
        using var memory = new MemoryStream();
        using (var writer = XmlWriter.Create(memory, new XmlWriterSettings { NewLineHandling = NewLineHandling.Entitize, Indent = true, CloseOutput = false }))
        {
            writer.WriteStartElement("GpcCheckReports"); writer.WriteAttributeString("version", version);
            new DomainXmlSerializer(typeof(CheckReport[]), ModelValues.DataContracts, DomainXmlSerializer.DocumentItems).WriteObject(writer, reports); writer.WriteEndElement();
        }
        return memory.ToArray();
    }

    private static void SameRows(IReadOnlyList<CheckReport> expected, IReadOnlyList<CheckReport> actual)
    {
        Assert.AreEqual(expected.Count, actual.Count);
        for (int j = 0; j < expected.Count; j++)
        {
            Assert.AreEqual(expected[j].Required, actual[j].Required); Assert.AreEqual(expected[j].Outcome, actual[j].Outcome);
            Assert.IsTrue(actual[j].HasUnchangedScope);
            CollectionAssert.AreEqual(expected[j].Results.Select(r => r.PlanItemId + "|" + r.EvidenceFingerprint).ToArray(),
                actual[j].Results.Select(r => r.PlanItemId + "|" + r.EvidenceFingerprint).ToArray());
            Assert.IsTrue(actual[j].Results.All(r => r.HasUnchangedEvidence));
        }
    }

    [TestMethod]
    public void SmallReportsKeepTheTextOfVersions1To5()
    {
        var report = Report("small", Enumerable.Range(0, 20).Select(i => Row(i, 32)));
        var bytes = Save(report);
        Assert.IsFalse(IsPackage(bytes));
        CollectionAssert.AreEqual(LegacyText(new[] { report }, "2"), bytes);
        SameRows(new[] { report }, Load(bytes));
    }

    /// <summary>Text with the GUIDs of geometry objects masked: they are not identities and are regenerated when read.</summary>
    private static string WithoutGeometryGuids(byte[] xml) => System.Text.RegularExpressions.Regex.Replace(Encoding.UTF8.GetString(xml),
        "(<Guid [^>]*>)[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}(</Guid>)", "$1$2");

    /// <summary>Archive written by Model 5.23.0.1 (GPC.Model.Persistence 3.3.0.7, before the package): twelve rows of the
    /// steel section pipeline of GPCModelChecker 2.32 (full and GoverningAndFailed traces). It loads, and saving what was
    /// loaded writes the same document again (apart from the regenerated geometry GUIDs).</summary>
    [TestMethod]
    public void ArchiveOfTheVersionBeforeThePackageStillLoadsAndResavesTheSameDocument()
    {
        byte[] legacy;
        using (var file = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "check-reports-v3-5.23.0.1.xml.gz")))
        using (var gz = new GZipStream(file, CompressionMode.Decompress)) using (var plain = new MemoryStream()) { gz.CopyTo(plain); legacy = plain.ToArray(); }
        var reports = Load(legacy);
        Assert.AreEqual(2, reports.Count); Assert.AreEqual(12, reports.Sum(r => r.Results.Count));
        Assert.IsTrue(reports.All(r => r.HasUnchangedScope && r.Outcome == EngineeringOutcome.Satisfied && r.BeamScope != null));
        Assert.IsTrue(reports.SelectMany(r => r.Results).All(r => r.HasUnchangedEvidence && r.Details is NativeMethodDetails && r.Provenance != null));
        Assert.AreEqual(3, reports[1].Results.Count(r => r.Details.Trace.Count == 1 && r.Details.Trace[0].Key == "Trace.Omitted"));
        var again = Save(reports.ToArray());
        Assert.AreEqual(legacy.Length, again.Length);
        Assert.AreEqual(WithoutGeometryGuids(legacy), WithoutGeometryGuids(again));
    }

    /// <summary>Rows, seals and scopes back; with <paramref name="again"/>, also the same bytes for the same report and the
    /// same blocks when what was loaded is saved again.</summary>
    private static (double Save, double Load, int Entries, double Megabytes) RoundTrip(bool again, params CheckReport[] reports)
    {
        var watch = Stopwatch.StartNew(); var bytes = Save(reports); double save = watch.Elapsed.TotalSeconds;
        Assert.IsTrue(IsPackage(bytes));
        watch.Restart(); var loaded = Load(bytes); double load = watch.Elapsed.TotalSeconds;
        SameRows(reports, loaded);
        using var zip = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        Assert.AreEqual("reports.xml", zip.Entries[0].FullName);
        Assert.IsTrue(zip.Entries.All(e => e.Length <= DomainXmlSerializer.DocumentCharacters));
        if (again)
        {
            CollectionAssert.AreEqual(bytes, Save(reports));
            using var resaved = new ZipArchive(new MemoryStream(Save(loaded.ToArray())), ZipArchiveMode.Read);
            Assert.AreEqual(zip.Entries.Count, resaved.Entries.Count);
            Assert.AreEqual(WithoutGeometryGuids(Entry(zip.Entries[1])), WithoutGeometryGuids(Entry(resaved.Entries[1])));
        }
        return (save, load, zip.Entries.Count, bytes.Length / 1048576.0);
    }

    private static byte[] Entry(ZipArchiveEntry entry) { using var stream = entry.Open(); using var copy = new MemoryStream(); stream.CopyTo(copy); return copy.ToArray(); }

    /// <summary>6,000 rows with 40 trace entries: about 2,250,000 objects, beyond one document. Saved as a package of
    /// blocks, read back with the same rows and seals.</summary>
    [TestMethod]
    public void LargeReportsWithFullTracesRoundTripAsAPackage()
    {
        var (save, load, entries, size) = RoundTrip(true, Report("full", Enumerable.Range(0, 6000).Select(i => Row(i, 40))));
        Assert.AreEqual(7, entries);
        Console.WriteLine($"6,000 rows, 40 trace entries: {size:F1} MB in {entries} entries, save {save:F2} s, load {load:F2} s");
    }

    /// <summary>60,000 rows with one trace entry (as GoverningAndFailed leaves most rows) in two jobs: about 10,800,000 objects.</summary>
    [TestMethod]
    public void LargeReportsWithCompactTracesRoundTripAsAPackage()
    {
        var (save, load, entries, size) = RoundTrip(false, Report("compact A", Enumerable.Range(0, 30000).Select(i => Row(i, 1))),
            Report("compact B", Enumerable.Range(30000, 30000).Select(i => Row(i, 1))));
        Assert.AreEqual(61, entries);
        Console.WriteLine($"60,000 rows, one trace entry: {size:F1} MB in {entries} entries, save {save:F2} s, load {load:F2} s");
    }

    /// <summary>A valid package of a small report, written by the package writer itself (Save writes small reports as text).</summary>
    private static byte[] SmallPackage(int rows = 3, int trace = 2)
    {
        var report = Report("small", Enumerable.Range(0, rows).Select(i => Row(i, trace)));
        using var memory = new MemoryStream();
        typeof(CheckReportArchive).GetMethod("SavePackage", BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, new object[] { new[] { report }, "2", memory });
        var bytes = memory.ToArray();
        SameRows(new[] { report }, Load(bytes));
        return bytes;
    }

    /// <summary>The same entries, changed by <paramref name="edit"/> (name and text of each, in order).</summary>
    private static byte[] Rezip(byte[] package, Func<List<(string Name, string Text)>, List<(string Name, string Text)>> edit)
    {
        List<(string Name, string Text)> entries;
        using (var zip = new ZipArchive(new MemoryStream(package), ZipArchiveMode.Read))
            entries = zip.Entries.Select(e => (e.FullName, Encoding.UTF8.GetString(Entry(e)))).ToList();
        using var memory = new MemoryStream();
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true))
            foreach (var (name, text) in edit(entries))
            { using var stream = zip.CreateEntry(name, CompressionLevel.Fastest).Open(); var bytes = Encoding.UTF8.GetBytes(text); stream.Write(bytes, 0, bytes.Length); }
        return memory.ToArray();
    }

    /// <summary>The objects of an entry follow its characters: a 400-byte document declaring an array of 1.5 billion reports
    /// is refused by the object ceiling before the array is allocated (with no ceiling: 11 GB).</summary>
    [TestMethod]
    public void SmallPackageDeclaringAHugeArrayIsRefusedBeforeAllocating()
    {
        var bytes = Rezip(SmallPackage(), entries => new List<(string Name, string Text)> { ("reports.xml", "<GpcCheckReports version=\"1\" rows=\"\"><ArrayOfCheckReport z:Id=\"1\" z:Size=\"1500000000\" "
            + "xmlns=\"http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing\" xmlns:i=\"http://www.w3.org/2001/XMLSchema-instance\" "
            + "xmlns:z=\"http://schemas.microsoft.com/2003/10/Serialization/\"></ArrayOfCheckReport></GpcCheckReports>") });
        long before = GC.GetTotalMemory(true);
        var error = Assert.ThrowsException<SerializationException>(() => Load(bytes));
        StringAssert.Contains(error.Message, "Maximum number of items");
        Assert.IsTrue(GC.GetTotalMemory(false) - before < 64L * 1024 * 1024);
    }

    /// <summary>The characters an entry declares are bounded by what deflate can expand from its stored bytes.</summary>
    [TestMethod]
    public void EntryDeclaringMoreThanDeflateCanExpandIsRefused()
    {
        var bytes = SmallPackage();
        // Central directory of the last entry: uncompressed size at offset 24 of its header (signature 50 4B 01 02).
        int central = Enumerable.Range(0, bytes.Length - 4).Last(k => bytes[k] == 0x50 && bytes[k + 1] == 0x4B && bytes[k + 2] == 1 && bytes[k + 3] == 2);
        BitConverter.GetBytes(200 * 1024 * 1024).CopyTo(bytes, central + 24);
        StringAssert.Contains(Assert.ThrowsException<SerializationException>(() => Load(bytes)).Message, "oversized");
    }

    /// <summary>Exactly the rows the header counts, in numbered entries, with the version of the header; nothing else.</summary>
    [TestMethod]
    public void PackageWithOtherEntriesOrRowsIsRefused()
    {
        var valid = SmallPackage();
        foreach (var edit in new Func<List<(string Name, string Text)>, List<(string Name, string Text)>>[] {
            e => e.Append(("other.xml", "<x/>")).ToList(),                                                   // extra entry
            e => e.Concat(e.Skip(1)).Select((x, i) => i == 0 ? x : ("rows/" + i.ToString("D6") + ".xml", x.Text)).ToList(), // more rows than counted
            e => e.Take(1).ToList(),                                                                          // rows missing
            e => e.Select(x => x.Name == "rows/000001.xml" ? ("rows/000002.xml", x.Text) : x).ToList(),       // numbering
            e => e.Select(x => x.Name == "reports.xml" ? ("model.xml", x.Text) : x).ToList(),                 // no header
            e => e.Select(x => x.Name == "reports.xml" ? (x.Name, x.Text.Replace("rows=\"3\"", "rows=\"2\"")) : x).ToList(),
            e => e.Select(x => x.Name == "reports.xml" ? (x.Name, x.Text.Replace("rows=\"3\"", "rows=\"3 0\"")) : x).ToList(),
            e => e.Select(x => x.Name != "reports.xml" ? (x.Name, x.Text.Replace("version=\"2\"", "version=\"3\"")) : x).ToList(),
            e => e.Select(x => (x.Name, x.Text.Replace("version=\"2\"", "version=\"6\""))).ToList() })
            Assert.ThrowsException<SerializationException>(() => Load(Rezip(valid, edit)));
        SameRows(Load(valid), Load(Rezip(valid, e => e)));
    }

    [DataContract]
    private sealed class UnregisteredDetails : CheckDetails
    { public UnregisteredDetails() : base("fixture-unregistered", "fixture", new[] { new CheckMetric("m", 1, 2, "N", .5) }, null) { } }

    /// <summary>An error that is not a size (here a type outside the contracts) still fails the save with its own message,
    /// found again on the row that carries it, and the destination is left unchanged.</summary>
    [TestMethod]
    public void OtherSerializationErrorsStillFailTheSave()
    {
        var report = Report("bad", Enumerable.Range(0, 5).Select(i => Row(i, 2)));
        report.Results[3].Details = new UnregisteredDetails();
        using var stream = new MemoryStream(); stream.WriteByte(42);
        var error = Assert.ThrowsException<SerializationException>(() => CheckReportArchive.Save(new[] { report }, stream));
        StringAssert.Contains(error.Message, nameof(UnregisteredDetails));
        CollectionAssert.AreEqual(new byte[] { 42 }, stream.ToArray());
    }

    /// <summary>A row that no document holds cannot be saved; the destination is left unchanged, as for any refused report.</summary>
    [TestMethod]
    public void RowLargerThanADocumentIsRefusedOnSave()
    {
        var report = Report("huge", new[] { Row(0, 500_000) });
        using var stream = new MemoryStream(); stream.WriteByte(42);
        StringAssert.Contains(Assert.ThrowsException<SerializationException>(() => CheckReportArchive.Save(new[] { report }, stream)).Message, "check result exceeds");
        CollectionAssert.AreEqual(new byte[] { 42 }, stream.ToArray());
    }
}
