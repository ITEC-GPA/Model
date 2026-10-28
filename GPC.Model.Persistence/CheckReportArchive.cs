using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Reports;
using GPC.Model.Core;

namespace GPC.Model.Persistence
{
    /// <summary>Standalone, versioned report archive. Only shared data contracts; no Model or solver instance is required.
    /// Reports that one XML document holds (2,000,000 objects and 256 Mi characters, the ceilings of its reader) are saved as
    /// that document, versions 1-5, as before. Larger ones are saved as a package of such documents (see
    /// <see cref="SavePackage"/>): the rows in blocks, each block within the same ceilings.</summary>
    public static class CheckReportArchive
    {
        private const string PackageHeader = "reports.xml", PackageRows = "rows/";
        private static readonly DateTimeOffset PackageTime = new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);
        /// <summary>Rows of a block of the package, halved until the block fits one document. The serializer's time grows
        /// faster than the size of one graph (60,000 compact rows: 32 s in one document, 6.8 s in eight); blocks keep the
        /// archive linear in its rows.</summary>
        internal const int BlockRows = 1000;
        /// <summary>Deflate expands its input at most 1032 times (a 258-byte match in two bits): an entry declaring more
        /// characters than that over its stored bytes is corrupt or hostile.</summary>
        internal const long DeflateExpansion = 1032;

        public static void Save(IEnumerable<CheckReport> reports, Stream destination)
        {
            var rows = (reports ?? throw new ArgumentNullException(nameof(reports))).ToArray();
            Validate(rows);
            string version = RequiresVersion5(rows) ? "5" : RequiresVersion4(rows) ? "4" : RequiresVersion3(rows) ? "3" : RequiresVersion2(rows) ? "2" : "1";
            // Build the archive before touching the caller's destination on serialization errors.
            using (var memory = new MemoryStream())
            {
                if (!TryWrite(memory, "GpcCheckReports", version, null, rows, true, out _)) { memory.SetLength(0); SavePackage(rows, version, memory); }
                memory.Position = 0; memory.CopyTo(destination);
            }
        }

        /// <summary>One document, only if the reader of the archive takes it back: within the objects of the serializer and
        /// the characters of the reader (a byte is at most one character).</summary>
        private static bool TryWrite(MemoryStream memory, string root, string version, string rows, Array value, bool indent, out SerializationException error)
        {
            error = null;
            try
            {
                using (var writer = XmlWriter.Create(new BoundedStream(memory, DomainXmlSerializer.DocumentCharacters),
                    new XmlWriterSettings { NewLineHandling = NewLineHandling.Entitize, Indent = indent, CloseOutput = false }))
                {
                    writer.WriteStartElement(root); writer.WriteAttributeString("version", version);
                    if (rows != null) writer.WriteAttributeString("rows", rows);
                    ModelArchive.Serializer(value.GetType()).WriteObject(writer, value); writer.WriteEndElement();
                }
                return true;
            }
            // More objects or characters than one document holds. Any other serialization error recurs in the package,
            // down to the single row (or the header) that carries it.
            catch (SerializationException ex) { error = ex; return false; }
        }

        /// <summary>Package: deterministic ZIP (fixed time and order, no indentation). reports.xml (root GpcCheckReports) holds
        /// the reports without their rows and, in the attribute rows, the number of rows of each; rows/000001.xml... (root
        /// GpcCheckRows) hold all the rows in report order, in blocks of <see cref="BlockRows"/> halved until a block fits one
        /// document. Every entry is a document that the reader takes back within the ceilings of the XML archive; shared
        /// objects stay shared within a document.</summary>
        private static void SavePackage(CheckReport[] reports, string version, Stream destination)
        {
            var all = reports.SelectMany(r => r.Results).ToArray();
            using (var zip = new ZipArchive(destination, ZipArchiveMode.Create, true))
            {
                string counts = string.Join(" ", reports.Select(r => r.Results.Count.ToString(CultureInfo.InvariantCulture)));
                if (!TryEntry(zip, PackageHeader, "GpcCheckReports", version, counts, reports.Select(r => r.WithoutResults()).ToArray(), out var error))
                    throw new SerializationException("The report scopes cannot be written in one report archive document: " + error.Message, error);
                int index = 0;
                void Block(int start, int count)
                {
                    var block = new CheckResult[count]; Array.Copy(all, start, block, 0, count);
                    if (TryEntry(zip, PackageRows + (index + 1).ToString("D6", CultureInfo.InvariantCulture) + ".xml", "GpcCheckRows", version, null, block, out var failure)) { index++; return; }
                    if (count == 1) throw new SerializationException("A check result exceeds the objects or characters of a report archive document, or cannot be written: " + failure.Message, failure);
                    Block(start, count / 2); Block(start + count / 2, count - count / 2);
                }
                for (int start = 0; start < all.Length; start += BlockRows) Block(start, Math.Min(BlockRows, all.Length - start));
            }
        }

        private static bool TryEntry(ZipArchive zip, string name, string root, string version, string rows, Array value, out SerializationException error)
        {
            using (var document = new MemoryStream())
            {
                if (!TryWrite(document, root, version, rows, value, false, out error)) return false;
                var entry = zip.CreateEntry(name, CompressionLevel.Fastest); entry.LastWriteTime = PackageTime;
                using (var stream = entry.Open()) { document.Position = 0; document.CopyTo(stream); }
                return true;
            }
        }

        public static IReadOnlyList<CheckReport> Load(Stream source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!source.CanSeek) { var buffer = new MemoryStream(); source.CopyTo(buffer); buffer.Position = 0; source = buffer; }
            long start = source.Position; var head = new byte[4]; int read = 0;
            while (read < 4) { int n = source.Read(head, read, 4 - read); if (n == 0) break; read += n; }
            source.Position = start;
            if (!(read == 4 && head[0] == 0x50 && head[1] == 0x4B && head[2] == 3 && head[3] == 4)) return LoadXml(source);
            // ZIP offsets count from the start of the stream.
            if (start != 0) { var tail = new MemoryStream(); source.CopyTo(tail); tail.Position = 0; source = tail; }
            return LoadPackage(source);
        }

        private static IReadOnlyList<CheckReport> LoadXml(Stream source)
        {
            using (var reader = XmlReader.Create(source, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null, MaxCharactersInDocument = DomainXmlSerializer.DocumentCharacters, CloseInput = false }))
            {
                var reports = (CheckReport[])Read(reader, "GpcCheckReports", ModelArchive.Serializer(typeof(CheckReport[])), out string version, out _);
                return Checked(reports, version);
            }
        }

        /// <summary>Every entry is read like the XML archive, within 256 Mi characters and 2,000,000 objects, and moreover
        /// within the characters it declares, which deflate must be able to expand from its stored bytes, and the objects so
        /// many characters can hold (<see cref="ModelValues.DocumentSerializer"/>): the work and the memory of a load follow
        /// the size of the file, and a small entry cannot declare a large array or graph. The rows must be exactly those the
        /// header counts, in numbered entries and nothing else.</summary>
        private static IReadOnlyList<CheckReport> LoadPackage(Stream source)
        {
            try
            {
                using (var zip = new ZipArchive(source, ZipArchiveMode.Read, true))
                {
                    var entries = zip.Entries;
                    if (entries.Count == 0 || entries[0].FullName != PackageHeader) throw new SerializationException("Unsupported check report package.");
                    var reports = (CheckReport[])ReadEntry(entries[0], "GpcCheckReports", typeof(CheckReport[]), out string version, out string counts);
                    var expected = (counts ?? "").Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => int.TryParse(c, NumberStyles.None, CultureInfo.InvariantCulture, out int n) ? n : -1).ToArray();
                    if (reports == null || expected.Length != reports.Length || expected.Any(n => n < 0) || reports.Any(r => r == null || r.Results == null || r.Results.Count != 0))
                        throw new SerializationException("Corrupt check report package.");
                    long total = expected.Sum(n => (long)n);
                    var rows = new List<CheckResult>();
                    for (int i = 1; i < entries.Count; i++)
                    {
                        if (entries[i].FullName != PackageRows + i.ToString("D6", CultureInfo.InvariantCulture) + ".xml") throw new SerializationException("Unsupported check report package.");
                        var block = (CheckResult[])ReadEntry(entries[i], "GpcCheckRows", typeof(CheckResult[]), out string blockVersion, out _);
                        if (blockVersion != version || block == null || block.Length == 0 || block.Length > total - rows.Count) throw new SerializationException("Corrupt check report package.");
                        rows.AddRange(block);
                    }
                    if (rows.Count != total) throw new SerializationException("Corrupt check report package.");
                    int next = 0;
                    for (int r = 0; r < reports.Length; r++) { reports[r].Results.AddRange(rows.GetRange(next, expected[r])); next += expected[r]; }
                    return Checked(reports, version);
                }
            }
            catch (InvalidDataException ex) { throw new SerializationException("Corrupt check report package.", ex); }
            catch (EndOfStreamException ex) { throw new SerializationException("Corrupt check report package.", ex); }
        }

        private static Array ReadEntry(ZipArchiveEntry entry, string root, Type type, out string version, out string rows)
        {
            long characters = entry.Length;
            if (characters <= 0 || characters > DomainXmlSerializer.DocumentCharacters || entry.CompressedLength < characters / DeflateExpansion)
                throw new SerializationException("Corrupt or oversized check report package entry.");
            using (var stream = entry.Open())
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null, MaxCharactersInDocument = characters, CloseInput = false }))
                return Read(reader, root, ModelValues.DocumentSerializer(type, characters), out version, out rows);
        }

        private static Array Read(XmlReader reader, string root, DomainXmlSerializer serializer, out string version, out string rows)
        {
            reader.MoveToContent();
            version = reader.GetAttribute("version"); rows = reader.GetAttribute("rows");
            if (reader.LocalName != root || (version != "1" && version != "2" && version != "3" && version != "4" && version != "5")) throw new SerializationException("Unsupported check report archive version.");
            reader.ReadStartElement();
            var value = (Array)serializer.ReadObject(reader);
            reader.ReadEndElement();
            return value;
        }

        private static IReadOnlyList<CheckReport> Checked(CheckReport[] reports, string version)
        {
            Validate(reports);
            if (version != "5" && RequiresVersion5(reports)) throw new SerializationException("Readable shell specifications require archive version 5.");
            if (version != "5" && version != "4" && RequiresVersion4(reports)) throw new SerializationException("Plate task reports require archive version 4.");
            if (version != "5" && version != "3" && version != "4" && RequiresVersion3(reports)) throw new SerializationException("Analysis/scenario provenance requires report archive version 3.");
            if (version == "1" && RequiresVersion2(reports)) throw new SerializationException("Member reports require archive version 2.");
            return Array.AsReadOnly(reports);
        }

        internal static bool RequiresVersion2(IEnumerable<CheckReport> reports) => global::GPC.Model.Checking.Reports.ReportSchema.RequiresVersion2(reports);
        internal static bool RequiresVersion3(IEnumerable<CheckReport> reports) => global::GPC.Model.Checking.Reports.ReportSchema.RequiresVersion3(reports);
        internal static bool RequiresVersion4(IEnumerable<CheckReport> reports) => global::GPC.Model.Checking.Reports.ReportSchema.RequiresVersion4(reports);
        internal static bool RequiresVersion5(IEnumerable<CheckReport> reports) => global::GPC.Model.Checking.Reports.ReportSchema.RequiresVersion5(reports);
        internal static void Validate(CheckReport[] reports) => global::GPC.Model.Checking.Reports.ReportSchema.Validate(reports);

        /// <summary>Write-only pass-through refusing more than its bytes: a document longer than its reader allows is
        /// refused while it is written (SerializationException, like the object ceiling of the serializer).</summary>
        private sealed class BoundedStream : Stream
        {
            private readonly Stream _inner;
            private readonly long _limit;
            private long _written;
            internal BoundedStream(Stream inner, long limit) { _inner = inner; _limit = limit; }
            public override void Write(byte[] buffer, int offset, int count)
            {
                if (count > _limit - _written) throw new SerializationException("The check report document exceeds " + _limit + " characters.");
                _written += count; _inner.Write(buffer, offset, count);
            }
            public override void Flush() => _inner.Flush();
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }
    }
}
