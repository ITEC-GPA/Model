using System.IO.Compression;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using GPC.Examples;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Core;
using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Persistence;

namespace UnitTest;

/// <summary>Model archives are read within ceilings that follow their size: documents 1-4 within the ceilings of one XML
/// document and within their bytes; the document of a package (version 5) and the analysis snapshots within what their
/// bytes can expand to; in each, the objects so many characters can hold. A short text cannot declare a large array (without
/// a ceiling, 20 KB declaring z:Size="1500000000" allocated 11 GB before failing), and the results section of a package is
/// read within the bytes it declares. What the writer produces loads as before.</summary>
[TestClass]
public class ModelArchiveLimitsTest
{
    /// <summary>MixedModelFactory (analysis snapshot, synthetic results) saved by Model 5.23.0.2 (GPC.Model.Persistence
    /// 3.4.0.0), before these limits.</summary>
    private const string PackageFixture = "model-v5-5.23.0.2.zip";
    private const string LegacyFixture = "model-v3-1.6.1.7.xml.gz";

    private static byte[] Fixture(string name) => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
    private static byte[] Gunzip(byte[] bytes)
    {
        using var gz = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress); using var plain = new MemoryStream();
        gz.CopyTo(plain); return plain.ToArray();
    }
    private static Model Load(byte[] bytes) { using var stream = new MemoryStream(bytes); return ModelArchive.Load(stream); }
    private static byte[] Save(Model model) { using var stream = new MemoryStream(); ModelArchive.Save(model, stream); return stream.ToArray(); }
    private static string Text(byte[] bytes) => Encoding.UTF8.GetString(bytes);
    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);
    private static Type DocumentType => typeof(ModelArchive).Assembly.GetType("GPC.Model.Persistence.ModelDocument")!;

    private static List<(string Name, byte[] Data)> Entries(byte[] package)
    {
        using var zip = new ZipArchive(new MemoryStream(package), ZipArchiveMode.Read);
        return zip.Entries.Select(e => { using var stream = e.Open(); using var copy = new MemoryStream(); stream.CopyTo(copy); return (e.FullName, copy.ToArray()); }).ToList();
    }
    /// <summary>A package of the entries, compressed as the writer does (the document deflated, the results stored).</summary>
    private static byte[] Zip(IEnumerable<(string Name, byte[] Data)> entries)
    {
        using var memory = new MemoryStream();
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Create, true))
            foreach (var (name, data) in entries)
            { using var stream = zip.CreateEntry(name, name == "results.bin" ? CompressionLevel.NoCompression : CompressionLevel.Fastest).Open(); stream.Write(data, 0, data.Length); }
        return memory.ToArray();
    }
    private static byte[] Edit(byte[] package, string name, Func<byte[], byte[]> edit) => Zip(Entries(package).Select(e => e.Name == name ? (e.Name, edit(e.Data)) : e));

    /// <summary>Refused with a SerializationException, without allocating what the forgery declares.</summary>
    private static string Refused(byte[] archive)
    {
        long before = GC.GetTotalMemory(true);
        var error = Assert.ThrowsException<SerializationException>(() => Load(archive));
        Assert.IsTrue(GC.GetTotalMemory(false) - before < 64L * 1024 * 1024, "Memory allocated by a refused archive.");
        return error.Message;
    }

    private static readonly Regex Size = new Regex("(\\w+):Size=\"\\d+\"");
    /// <summary>The text with its first array declaring <paramref name="size"/> items.</summary>
    private static string Declaring(string xml, long size)
    {
        Assert.IsTrue(Size.IsMatch(xml), "An array is declared.");
        return Size.Replace(xml, "$1:Size=\"" + size + "\"", 1);
    }

    /// <summary>Text with the GUIDs of geometry objects masked: they are not identities and are regenerated when read.</summary>
    private static string WithoutGeometryGuids(byte[] xml) => Regex.Replace(Text(xml),
        "(<Guid [^>]*>)[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}(</Guid>)", "$1$2");

    [TestMethod]
    public void PackageWrittenBeforeTheLimitsLoadsAndSavesTheSameEntriesAgain()
    {
        var bytes = Fixture(PackageFixture);
        var model = Load(bytes);
        Assert.IsTrue(model.AllElements.Where(e => e.HasResults).All(e => !e.ResultsLoaded));
        var entries = Entries(bytes); var again = Entries(Save(model));
        CollectionAssert.AreEqual(new[] { "model.xml", "results.bin" }, again.Select(e => e.Name).ToArray());
        CollectionAssert.AreEqual(entries[1].Data, again[1].Data, "Same compact results.");
        Assert.AreEqual(WithoutGeometryGuids(entries[0].Data), WithoutGeometryGuids(again[0].Data), "Same document apart from the regenerated geometry GUIDs.");
        var fresh = MixedModelFactory.Create();
        CollectionAssert.AreEqual(fresh.AllElements.Select(e => e.Id + ":" + e.Results.Count).ToArray(), model.AllElements.Select(e => e.Id + ":" + e.Results.Count).ToArray());
        Assert.AreEqual(model.Analysis.InputFingerprint, model.Analysis.OpenModel().AnalysisFingerprint());
    }

    private static byte[] SnapshotBytes(Model model) => (byte[])typeof(AnalysisSnapshot)
        .GetField("_model", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(model.Analysis)!;
    /// <summary>The graph of an image: magic, version, length of the graph, deflated graph, its digest, results.</summary>
    private static byte[] Graph(byte[] image)
    {
        using var inflate = new DeflateStream(new MemoryStream(image, 9, BitConverter.ToInt32(image, 5)), CompressionMode.Decompress);
        using var plain = new MemoryStream(); inflate.CopyTo(plain); return plain.ToArray();
    }

    /// <summary>The ceilings are at least eight times what the documents the writer produces need: one object per four
    /// characters against 60 to 160 characters per object measured on these archives (snapshots without indentation are the
    /// densest); the densest text the writer could produce is about one per twelve (a primitive array in the default
    /// namespace: &lt;int&gt;1&lt;/int&gt;).</summary>
    [TestMethod]
    public void DocumentsTheWriterProducesHoldFarFewerObjectsThanTheirCeilings()
    {
        void Holds(byte[] xml, Type root, bool archive)
        {
            using var reader = XmlReader.Create(new MemoryStream(xml), new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            reader.MoveToContent(); if (archive) reader.ReadStartElement();
            Assert.IsNotNull(new DomainXmlSerializer(root, ModelValues.DataContracts, DomainXmlSerializer.ObjectsIn(Text(xml).Length) / 8).ReadObject(reader));
        }
        var package = Fixture(PackageFixture); var model = Load(package);
        Holds(Entries(package)[0].Data, DocumentType, true);
        Holds(Graph(SnapshotBytes(model)), typeof(Model), false);
        using (var document = new MemoryStream()) { ModelArchive.SaveDocument(model, document); Holds(document.ToArray(), DocumentType, true); }
        var legacy = Gunzip(Fixture(LegacyFixture));
        Holds(legacy, typeof(Model), true);
        var snapshot = SnapshotBytes(Load(legacy)); Assert.IsFalse(ModelImage.IsImage(snapshot));
        Holds(snapshot, typeof(Model), false);
    }

    /// <summary>The package holds the models beyond one XML document: 28,000 nodes are about 2,130,000 objects in 165 MB of
    /// text, beyond the ceiling of one document, and within the ceiling their size allows (67,108,864 objects).</summary>
    [TestMethod]
    public void PackageBeyondOneDocumentLoadsWithinTheCeilingOfItsSize()
    {
        var model = new Model("nodes");
        for (int i = 1; i <= 28000; i++) model.NodesElements.Add(new NodeElement(new Point3d(i, 2 * i, 3 * i), id: i));
        var watch = System.Diagnostics.Stopwatch.StartNew(); var package = Save(model); double save = watch.Elapsed.TotalSeconds;
        using (var zip = new ZipArchive(new MemoryStream(package), ZipArchiveMode.Read))
        {
            var entry = zip.GetEntry("model.xml")!;
            using var stream = entry.Open(); using var reader = XmlReader.Create(stream);
            reader.MoveToContent(); reader.ReadStartElement();
            StringAssert.Contains(Assert.ThrowsException<SerializationException>(() => ModelValues.DocumentSerializer(DocumentType, entry.Length).ReadObject(reader)).Message,
                "Maximum number of items that can be serialized or deserialized in an object graph is '2000000'");
        }
        watch.Restart(); var loaded = Load(package); double load = watch.Elapsed.TotalSeconds;
        Assert.AreEqual(28000, loaded.NodesElements.Count);
        Assert.AreEqual(3 * 27999.0, loaded.NodesElements[27999].Position.Z);
        Console.WriteLine($"28,000 nodes: package {package.Length / 1048576.0:F1} MB, save {save:F1} s, load {load:F1} s");
    }

    /// <summary>The document of a package has no ceiling of one XML document, but its objects follow its characters: a
    /// 150 KB document declaring an array of 1.5 billion items is refused before the array is allocated.</summary>
    [TestMethod]
    public void PackageDocumentDeclaringAHugeArrayIsRefusedBeforeAllocating()
    {
        var bytes = Edit(Fixture(PackageFixture), "model.xml", xml => Bytes(Declaring(Text(xml), 1500000000)));
        StringAssert.Contains(Refused(bytes), "Maximum number of items");
    }

    /// <summary>Documents 1-4 keep the ceiling of 2,000,000 objects and moreover follow their bytes: 1,900,000 items are within
    /// the first, not within 430 KB.</summary>
    [TestMethod]
    public void DocumentDeclaringAnArrayBeyondItsSizeIsRefusedBeforeAllocating()
    {
        var xml = Text(Gunzip(Fixture(LegacyFixture)));
        StringAssert.Contains(Refused(Bytes(Declaring(xml, 1900000))), "Maximum number of items");
        StringAssert.Contains(Refused(Bytes(Declaring(xml, 1500000000))), "Maximum number of items");
    }

    private static readonly Regex SnapshotModel = new Regex("(<(?:\\w+:)?_model(?=[\\s>])[^>]*>)([^<]*)(</)");
    private static readonly Regex SnapshotHash = new Regex("(<(?:\\w+:)?_contentHash(?=[\\s>])[^>]*>)([^<]*)(</)");
    /// <summary>The document with the bytes of its analysis snapshot changed by <paramref name="edit"/>, declaring the digest of
    /// the new bytes, as a forger would.</summary>
    private static string WithSnapshot(string xml, Func<byte[], byte[]> edit)
    {
        var bytes = edit(Convert.FromBase64String(SnapshotModel.Match(xml).Groups[2].Value));
        xml = SnapshotModel.Replace(xml, m => m.Groups[1].Value + Convert.ToBase64String(bytes) + m.Groups[3].Value, 1);
        using var sha = SHA256.Create();
        return SnapshotHash.Replace(xml, m => m.Groups[1].Value + Convert.ToBase64String(sha.ComputeHash(bytes)) + m.Groups[3].Value, 1);
    }
    /// <summary>The image with its graph text changed by <paramref name="edit"/> and deflated again.</summary>
    private static byte[] WithGraph(byte[] image, Func<string, string> edit)
    {
        int length = BitConverter.ToInt32(image, 5);
        using var packed = new MemoryStream();
        using (var deflate = new DeflateStream(packed, CompressionLevel.Fastest, true)) { var text = Bytes(edit(Text(Graph(image)))); deflate.Write(text, 0, text.Length); }
        var graph = packed.ToArray();
        return image.Take(5).Concat(BitConverter.GetBytes(graph.Length)).Concat(graph).Concat(image.Skip(9 + length)).ToArray();
    }

    /// <summary>The analysis snapshot is validated when the archive is read, and its digest is declared next to it: an image
    /// is read within what deflate can expand from its graph, the XML of the snapshots before the images within its bytes.</summary>
    [TestMethod]
    public void ForgedAnalysisSnapshotsAreReadWithinTheirSize()
    {
        var package = Fixture(PackageFixture);
        // The same graph deflated again: other bytes, other digest, the same inputs.
        Assert.IsNotNull(Load(Edit(package, "model.xml", xml => Bytes(WithSnapshot(Text(xml), image => WithGraph(image, graph => graph))))).Analysis);
        StringAssert.Contains(Refused(Edit(package, "model.xml", xml => Bytes(WithSnapshot(Text(xml), image => WithGraph(image, graph => Declaring(graph, 1500000000)))))),
            "Maximum number of items");

        var legacy = Text(Gunzip(Fixture(LegacyFixture)));
        Assert.IsNotNull(Load(Bytes(WithSnapshot(legacy, snapshot => snapshot))).Analysis);
        StringAssert.Contains(Refused(Bytes(WithSnapshot(legacy, snapshot => Bytes(Declaring(Text(snapshot), 1500000000))))), "Maximum number of items");
    }

    /// <summary>The package with the uncompressed size that the central directory declares for <paramref name="name"/> set by
    /// <paramref name="size"/> (compressed, uncompressed).</summary>
    private static byte[] Declared(byte[] package, string name, Func<long, long, long> size)
    {
        var bytes = (byte[])package.Clone();
        for (int k = 0; k + 46 < bytes.Length; k++)
            if (bytes[k] == 0x50 && bytes[k + 1] == 0x4B && bytes[k + 2] == 1 && bytes[k + 3] == 2
                && Encoding.ASCII.GetString(bytes, k + 46, BitConverter.ToUInt16(bytes, k + 28)) == name)
            {
                BitConverter.GetBytes((uint)size(BitConverter.ToUInt32(bytes, k + 20), BitConverter.ToUInt32(bytes, k + 24))).CopyTo(bytes, k + 24);
                return bytes;
            }
        throw new AssertFailedException("No central directory header for " + name + ".");
    }

    /// <summary>The characters and bytes an entry declares are bounded by what deflate can expand from its stored bytes; a
    /// document longer than it declares is refused too.</summary>
    [TestMethod]
    public void EntryDeclaringMoreThanDeflateCanExpandIsRefused()
    {
        var package = Fixture(PackageFixture);
        StringAssert.Contains(Refused(Declared(package, "model.xml", (stored, length) => 2000 * stored)), "oversized");
        StringAssert.Contains(Refused(Declared(package, "results.bin", (stored, length) => 2000 * stored)), "oversized");
        Refused(Declared(package, "model.xml", (stored, length) => length / 2));
    }

    /// <summary>Offsets of the fields of the first segment in results.bin: magic, version, elements; for each element its GUID
    /// and segments; for each segment results, raw length, digest, datasets, payload size and payload.</summary>
    private static (int Segments, int Items, int Length, int Datasets, int Size) FirstSegment(byte[] results)
    {
        int at = 9, count = BitConverter.ToInt32(results, 5);
        for (int i = 0; i < count; i++)
        {
            int segments = at + 16; at = segments + 4;
            if (BitConverter.ToInt32(results, segments) == 0) continue;
            int items = at, datasets = at + 40; at = datasets + 4;
            for (int d = 0; d < BitConverter.ToInt32(results, datasets); d++) { int length = results[at++]; Assert.IsTrue(length < 128); at += length; }
            return (segments, items, items + 4, datasets, at);
        }
        throw new AssertFailedException("No result segment.");
    }
    private static byte[] Patch(byte[] data, int offset, int value) { var copy = (byte[])data.Clone(); BitConverter.GetBytes(value).CopyTo(copy, offset); return copy; }

    /// <summary>Counts of the results section beyond its bytes are refused before they are allocated (before, the entry stream
    /// gave no bound: 2^31 elements were allocated); a segment declaring more raw bytes than its payload expands to, or more
    /// results than raw bytes, is refused when read, not when first decoded. The section lists every element of the model.</summary>
    [TestMethod]
    public void ResultsSectionDeclaringMoreThanItsBytesIsRefused()
    {
        var package = Fixture(PackageFixture);
        var results = Entries(package)[1].Data; var first = FirstSegment(results);
        foreach (var forged in new Func<byte[], byte[]>[] {
            r => Patch(r, 5, int.MaxValue),                         // elements
            r => Patch(r, first.Segments, int.MaxValue),            // segments of an element
            r => Patch(r, first.Datasets, int.MaxValue),            // dataset names of a segment
            r => Patch(r, first.Size, int.MaxValue),                // payload
            r => Patch(r, first.Length, int.MaxValue),              // raw bytes beyond what the payload expands to
            r => Patch(r, first.Items, BitConverter.ToInt32(r, first.Length) + 1) })  // more results than raw bytes
            StringAssert.Contains(Refused(Edit(package, "results.bin", forged)), "CorruptModelImage");
        StringAssert.Contains(Refused(Edit(package, "results.bin", r => Patch(r, 5, 0).Take(9).ToArray())), "ResultElementCountMismatch");
        StringAssert.Contains(Refused(Edit(package, "results.bin", r => r.Concat(new byte[] { 0 }).ToArray())), "Corrupt archive package");
    }

    [TestMethod]
    public void PackageWithOtherEntriesOrVersionsIsRefused()
    {
        var package = Fixture(PackageFixture);
        Assert.AreEqual(Load(package).AllElements.Count(), Load(Zip(Entries(package))).AllElements.Count());
        foreach (var edit in new Func<List<(string Name, byte[] Data)>, IEnumerable<(string Name, byte[] Data)>>[] {
            e => e.Append(("other.xml", Bytes("<x/>"))),                                                         // extra entry
            e => e.Take(1),                                                                                      // no results
            e => e.Select(x => x.Name == "model.xml" ? ("document.xml", x.Data) : x),                            // no document
            e => e.Select(x => x.Name == "model.xml" ? (x.Name, Bytes(Text(x.Data).Replace("version=\"5\"", "version=\"4\""))) : x),
            e => e.Select(x => x.Name == "results.bin" ? (x.Name, Patch(x.Data, 4, 2)) : x) })                 // results version
            Assert.ThrowsException<SerializationException>(() => Load(Zip(edit(Entries(package)))));
    }

    /// <summary>A package, like a document, is read from the current position of the stream (its offsets count from its start).</summary>
    [TestMethod]
    public void ArchiveIsReadFromTheCurrentPositionOfTheStream()
    {
        var package = Fixture(PackageFixture); var expected = Load(package).AllElements.Count();
        foreach (var archive in new[] { package, Gunzip(Fixture(LegacyFixture)) })
        {
            using var stream = new MemoryStream(); stream.WriteByte(42); stream.Write(archive, 0, archive.Length); stream.Position = 1;
            Assert.AreEqual(archive == package ? expected : Load(archive).AllElements.Count(), ModelArchive.Load(stream).AllElements.Count());
        }
    }

    /// <summary>Scenario values and samples (AnalysisStorage) are documents within the ceilings of one document and within
    /// their bytes.</summary>
    [TestMethod]
    public void StoredValuesAreReadWithinTheirSize()
    {
        var values = Enumerable.Range(0, 100000).Select(i => i / 7.0).ToArray();
        CollectionAssert.AreEqual(values, AnalysisStorage.Read<double[]>(AnalysisStorage.Write(values)));
        var forged = Bytes(Declaring(Text(AnalysisStorage.Write(new double[3])), 1900000));
        StringAssert.Contains(Assert.ThrowsException<SerializationException>(() => AnalysisStorage.Read<double[]>(forged)).Message, "Maximum number of items");
    }
}
