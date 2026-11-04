using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using GPC.Model.Core;

namespace GPC.Model.Persistence
{
    /// <summary>Versioned XML with a closed set of locally compiled data contracts. No CLR type names from files are resolved.
    /// Models with results, or too large for an XML document, are saved as a package (version 5): the document without the
    /// element results plus their compact form.</summary>
    public static class ModelArchive
    {
        private const string PackageModel = "model.xml", PackageResults = "results.bin";
        private const int ResultsMagic = 0x53525047; // "GPRS"
        /// <summary>Largest XML document written by Save (bytes); larger models are saved as a package.</summary>
        private const long XmlDocumentBytes = 64L * 1024 * 1024;
        private static readonly DateTimeOffset PackageTime = new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);
        public static IReadOnlyCollection<Type> DataContracts => Core.ModelValues.DataContracts;
        internal static Core.DomainXmlSerializer Serializer(Type type) => Core.ModelValues.Serializer(type);
        public static T CopyValue<T>(T value) where T : class => Core.ModelValues.CopyValue(value);
        public static Models.Model Copy(Models.Model model) => Core.ModelValues.Copy(model);

        /// <summary>Explicit document contract for new applications, including models without an analysis snapshot.</summary>
        public static void SaveDocument(Models.Model model, Stream destination)
        {
            if (model == null || destination == null) throw new ArgumentNullException();
            CheckReportArchive.Validate(model.CheckReports.ToArray());
            using (var memory = new MemoryStream())
            {
                WriteDocument(model, memory);
                memory.Position = 0; memory.CopyTo(destination);
            }
        }

        private static void WriteDocument(Models.Model model, Stream destination)
        {
            using (var writer = XmlWriter.Create(destination, new XmlWriterSettings { NewLineHandling = NewLineHandling.Entitize, Indent = true, CloseOutput = false }))
            {
                writer.WriteStartElement("GpcModelArchive"); writer.WriteAttributeString("version", "4");
                Serializer(typeof(ModelDocument)).WriteObject(writer, ModelDocument.Capture(model)); writer.WriteEndElement();
            }
        }

        /// <summary>XML document (versions 1-4) for models without results up to 64 MB; package (version 5) for models with
        /// results or larger ones.</summary>
        public static void Save(Models.Model model, Stream destination)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            CheckReportArchive.Validate(model.CheckReports.ToArray());
            // Build the archive before touching the caller's destination on serialization errors.
            using (var memory = new MemoryStream())
            {
                if (model.AllElements.Any(e => e.HasResults) || !TrySaveXml(model, memory)) { memory.SetLength(0); SavePackage(model, memory); }
                memory.Position = 0; memory.CopyTo(destination);
            }
        }

        /// <summary>Save with an explicit <paramref name="format"/>. <see cref="ModelArchiveFormat.Xml"/> is <see cref="Save(Models.Model, Stream)"/>;
        /// <see cref="ModelArchiveFormat.Compact"/> writes version 6 (the document and the compact results in binary form, deflated), read
        /// by <see cref="Load"/> from GPC.Model.Persistence 3.14 on, or the XML form when the binary form does not reproduce the graph.</summary>
        public static void Save(Models.Model model, Stream destination, ModelArchiveFormat format)
        {
            if (format == ModelArchiveFormat.Xml) { Save(model, destination); return; }
            if (format != ModelArchiveFormat.Compact) throw new ArgumentOutOfRangeException(nameof(format));
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            CheckReportArchive.Validate(model.CheckReports.ToArray());
            using (var memory = new MemoryStream())
            {
                if (!TrySaveCompact(model, memory))
                {
                    memory.SetLength(0);
                    if (model.AllElements.Any(e => e.HasResults) || !TrySaveXml(model, memory)) { memory.SetLength(0); SavePackage(model, memory); }
                }
                memory.Position = 0; memory.CopyTo(destination);
            }
        }

        private static bool TrySaveXml(Models.Model model, MemoryStream memory)
        {
            // The document is written until it exceeds the size of an XML document, not to its end (a model of 10,000 elements wrote
            // 500 MB of XML to discard them): the same documents as before are XML, the larger ones a package.
            var limited = new LimitedStream(memory, XmlDocumentBytes);
            try
            {
                if (model.PhysicalSurfaces.Count > 0 || model.Analysis?.UsesCanonicalFingerprint == true || CheckReportArchive.RequiresVersion4(model.CheckReports)) WriteDocument(model, limited);
                else
                    using (var writer = XmlWriter.Create(limited, new XmlWriterSettings { NewLineHandling = NewLineHandling.Entitize, Indent = true, CloseOutput = false }))
                    {
                        writer.WriteStartElement("GpcModelArchive"); writer.WriteAttributeString("version", model.Analysis != null || model.VerificationContext != null || model.VerificationScenarios.Count != 0 || CheckReportArchive.RequiresVersion3(model.CheckReports)
                            ? "3" : model.PhysicalMembers.Count != 0 || CheckReportArchive.RequiresVersion2(model.CheckReports) ? "2" : "1");
                        Serializer(typeof(Models.Model)).WriteObject(writer, model); writer.WriteEndElement();
                    }
            }
            // Larger documents are read slowly as text and approach the characters a document may hold: package.
            catch (Exception) when (limited.Exceeded) { return false; }
            // The graph exceeds the objects of an XML document: the package has no such ceiling.
            catch (SerializationException) { return false; }
            return !limited.Exceeded && memory.Length <= XmlDocumentBytes;
        }

        /// <summary>Write-only view of a stream that refuses to grow it beyond a length (and remembers that it did).</summary>
        private sealed class LimitedStream : Stream
        {
            private readonly Stream _inner;
            private readonly long _limit;
            internal bool Exceeded { get; private set; }
            internal LimitedStream(Stream inner, long limit) { _inner = inner; _limit = limit; }
            public override void Write(byte[] buffer, int offset, int count)
            {
                if (Exceeded || _inner.Length + count > _limit) { Exceeded = true; throw new IOException("ArchiveDocumentLimit"); }
                _inner.Write(buffer, offset, count);
            }
            public override void Flush() => _inner.Flush();
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => _inner.Length;
            public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }

        #region Version 6 (compact)

        // "GPCMOD", version 6, flags; the raw and stored lengths of the deflated graph and of the deflated result section.
        private static readonly byte[] CompactMagic = { (byte)'G', (byte)'P', (byte)'C', (byte)'M', (byte)'O', (byte)'D' };
        private const byte CompactVersion = 6;

        private static bool IsCompact(byte[] head) => head.Length >= 6 && head.Take(6).SequenceEqual(CompactMagic);

        /// <summary>Version 6: the document without the element results (<see cref="ModelDocument"/>, as in the package of version 5) in
        /// binary form (ModelImage.WriteBinaryGraph), deflated, then, when some element has results, their section
        /// (ModelImage.WriteResultSection: raw segments, deflated as a whole). False, with nothing kept, when the binary form does not
        /// reproduce the graph.</summary>
        private static bool TrySaveCompact(Models.Model model, MemoryStream memory)
        {
            var elements = model.AllElements.ToArray();
            var results = ModelImage.CaptureResults(model, elements);
            long start = memory.Position;
            memory.Write(CompactMagic, 0, CompactMagic.Length); memory.WriteByte(CompactVersion); memory.WriteByte(0);
            memory.Write(new byte[32], 0, 32);
            long at = memory.Position, graphRaw, resultsRaw = 0;
            try { graphRaw = Deflated(memory, stream => ModelImage.WriteBinaryGraph(stream, ModelDocument.Capture(model), typeof(ModelDocument), elements)); }
            catch (ContractCopy.UnsupportedException) { return false; }
            long graphStored = memory.Position - at; at = memory.Position;
            if (results.Any(r => r.Count != 0)) resultsRaw = Deflated(memory, stream => ModelImage.WriteResultSection(stream, results, 0, results.Length - 1));
            long resultsStored = memory.Position - at, end = memory.Position;
            memory.Position = start + 8;
            using (var writer = new BinaryWriter(memory, System.Text.Encoding.UTF8, true)) { writer.Write(graphRaw); writer.Write(graphStored); writer.Write(resultsRaw); writer.Write(resultsStored); }
            memory.Position = end;
            return true;
        }

        private static long Deflated(Stream destination, Action<Stream> write)
        {
            using (var deflate = new DeflateStream(destination, CompressionLevel.Fastest, true))
            using (var counting = new CountingStream(deflate)) { write(counting); return counting.Count; }
        }

        /// <summary>Version 6: each section is read within the bytes it declares, which deflate must be able to expand from its stored
        /// bytes; the sections fill the archive exactly.</summary>
        private static Models.Model LoadCompact(Stream source)
        {
            try
            {
                var reader = new BinaryReader(source, System.Text.Encoding.UTF8, true);
                var head = reader.ReadBytes(8);
                if (head.Length != 8 || !IsCompact(head) || head[6] != CompactVersion || head[7] != 0) throw new SerializationException("Unsupported archive version.");
                long graphRaw = reader.ReadInt64(), graphStored = reader.ReadInt64(), resultsRaw = reader.ReadInt64(), resultsStored = reader.ReadInt64();
                long left = source.Length - source.Position;
                if (graphRaw < 5 || graphStored <= 0 || resultsRaw < 0 || resultsStored < 0 || graphStored > left || resultsStored != left - graphStored ||
                    graphRaw > graphStored * Core.DomainXmlSerializer.DeflateExpansion || resultsRaw > resultsStored * Core.DomainXmlSerializer.DeflateExpansion ||
                    (resultsRaw == 0) != (resultsStored == 0))
                    throw new SerializationException("Corrupt archive package.");
                Models.Model model;
                using (var inflate = Inflated(reader, graphStored))
                {
                    model = ((ModelDocument)ModelImage.ReadBinaryGraph(inflate, graphRaw, typeof(ModelDocument))).Restore();
                    if (inflate.ReadByte() != -1) throw new SerializationException("Corrupt archive package.");
                }
                if (resultsRaw != 0)
                {
                    var elements = model.AllElements.ToArray(); var results = new Results.Storage.ResultStorage[elements.Length];
                    using (var inflate = Inflated(reader, resultsStored))
                    using (var counted = new CountingReader(inflate))
                    {
                        ModelImage.ReadResultSection(counted, resultsRaw, results, 0, results.Length);
                        if (counted.Count != resultsRaw || inflate.ReadByte() != -1) throw new SerializationException("Corrupt archive package.");
                    }
                    for (int i = 0; i < results.Length; i++) if (results[i] == null) results[i] = Results.Storage.ResultStorage.Empty;
                    ModelImage.AttachResults(model, elements.Select(e => e.Guid).ToArray(), results);
                }
                CheckReportArchive.Validate(model.CheckReports.ToArray());
                model.Analysis?.Validate();
                return model;
            }
            catch (InvalidDataException ex) { throw new SerializationException("Corrupt archive package.", ex); }
            catch (EndOfStreamException ex) { throw new SerializationException("Corrupt archive package.", ex); }
        }

        private static Stream Inflated(BinaryReader reader, long stored)
        {
            var packed = reader.ReadBytes(checked((int)stored));
            if (packed.Length != stored) throw new SerializationException("Corrupt archive package.");
            return new DeflateStream(new MemoryStream(packed, false), CompressionMode.Decompress);
        }

        /// <summary>Read-only pass-through counting the bytes read.</summary>
        internal sealed class CountingReader : Stream
        {
            private readonly Stream _inner;
            internal long Count { get; private set; }
            internal CountingReader(Stream inner) { _inner = inner; }
            public override int Read(byte[] buffer, int offset, int count) { int n = _inner.Read(buffer, offset, count); Count += n; return n; }
            public override void Flush() { }
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => Count; set => throw new NotSupportedException(); }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }

        /// <summary>Write-only pass-through counting the bytes written.</summary>
        internal sealed class CountingStream : Stream
        {
            private readonly Stream _inner;
            internal long Count { get; private set; }
            internal CountingStream(Stream inner) { _inner = inner; }
            public override void Write(byte[] buffer, int offset, int count) { _inner.Write(buffer, offset, count); Count += count; }
            public override void Flush() => _inner.Flush();
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => Count;
            public override long Position { get => Count; set => throw new NotSupportedException(); }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }

        #endregion

        /// <summary>Package version 5: ZIP with the document without element results (model.xml) and their compact form (results.bin).</summary>
        private static void SavePackage(Models.Model model, Stream destination)
        {
            var elements = model.AllElements.ToArray();
            var results = ModelImage.CaptureResults(model, elements);
            using (var zip = new ZipArchive(destination, ZipArchiveMode.Create, true))
            {
                var document = zip.CreateEntry(PackageModel, CompressionLevel.Fastest); document.LastWriteTime = PackageTime;
                using (var stream = document.Open())
                using (var writer = XmlWriter.Create(stream, new XmlWriterSettings { NewLineHandling = NewLineHandling.Entitize, CloseOutput = false }))
                {
                    writer.WriteStartElement("GpcModelArchive"); writer.WriteAttributeString("version", "5");
                    ModelImage.WriteGraph(writer, ModelDocument.Capture(model), typeof(ModelDocument), elements); writer.WriteEndElement();
                }
                // Segments are already deflated.
                var compact = zip.CreateEntry(PackageResults, CompressionLevel.NoCompression); compact.LastWriteTime = PackageTime;
                using (var stream = compact.Open())
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(ResultsMagic); writer.Write((byte)1);
                    ModelImage.WriteResults(writer, elements.Select(e => e.Guid).ToArray(), results);
                }
            }
        }

        public static Models.Model Load(Stream source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (!source.CanSeek) { var buffer = new MemoryStream(); source.CopyTo(buffer); buffer.Position = 0; source = buffer; }
            long start = source.Position; var head = new byte[8]; int read = 0;
            while (read < 8) { int n = source.Read(head, read, 8 - read); if (n == 0) break; read += n; }
            source.Position = start;
            if (read >= 6 && IsCompact(head)) return LoadCompact(source);
            if (!(read >= 4 && head[0] == 0x50 && head[1] == 0x4B && head[2] == 3 && head[3] == 4)) return LoadXml(source, source.Length - start);
            // ZIP offsets count from the start of the stream.
            if (start != 0) { var tail = new MemoryStream(); source.CopyTo(tail); tail.Position = 0; source = tail; }
            return LoadPackage(source);
        }

        /// <summary>Versions 1-4: one XML document, read within the ceilings of one document (256 Mi characters, 2,000,000
        /// objects) and within its <paramref name="bytes"/> (a byte is at most one character), with the objects so many
        /// characters can hold (<see cref="Core.ModelValues.DocumentSerializer"/>): a short document cannot declare a large array.</summary>
        private static Models.Model LoadXml(Stream source, long bytes)
        {
            long characters = Math.Max(1, Math.Min(bytes, Core.DomainXmlSerializer.DocumentCharacters));
            using (var reader = XmlReader.Create(source, new XmlReaderSettings
            { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = characters, CloseInput = false }))
            {
                reader.MoveToContent();
                string version = reader.GetAttribute("version");
                if (reader.LocalName != "GpcModelArchive" || (version != "1" && version != "2" && version != "3" && version != "4")) throw new SerializationException("Unsupported archive version.");
                reader.ReadStartElement();
                var model = version == "4" ? ((ModelDocument)Core.ModelValues.DocumentSerializer(typeof(ModelDocument), characters).ReadObject(reader)).Restore()
                    : (Models.Model)Core.ModelValues.DocumentSerializer(typeof(Models.Model), characters).ReadObject(reader);
                reader.ReadEndElement();
                CheckReportArchive.Validate(model.CheckReports.ToArray());
                if(version!="4" && model.PhysicalSurfaces.Count!=0) throw new SerializationException("Physical surfaces require archive version 4.");
                if (version != "4" && CheckReportArchive.RequiresVersion4(model.CheckReports)) throw new SerializationException("Plate task reports require archive version 4.");
                if (version != "3" && version != "4" && (model.Analysis != null || model.VerificationContext != null || model.VerificationScenarios.Count != 0 || CheckReportArchive.RequiresVersion3(model.CheckReports)))
                    throw new SerializationException("Analysis/scenario provenance requires archive version 3.");
                model.Analysis?.Validate();
                if (version == "1" && (model.PhysicalMembers.Count != 0 || CheckReportArchive.RequiresVersion2(model.CheckReports))) throw new SerializationException("Physical member data require archive version 2.");
                return model;
            }
        }

        /// <summary>Version 5. model.xml has no ceiling of one XML document (the package holds the models beyond it): it is read
        /// within the characters it declares, which deflate must be able to expand from its stored bytes, and the objects so many
        /// characters can hold (<see cref="Core.ModelValues.ImageSerializer(Type, long)"/>), so the work and the memory of a load
        /// follow the size of the file and a small entry cannot declare a large array or graph. results.bin is read within the
        /// bytes it declares and lists exactly the elements of the model, as the writer does.</summary>
        private static Models.Model LoadPackage(Stream source)
        {
            try
            {
                using (var zip = new ZipArchive(source, ZipArchiveMode.Read, true))
                {
                    if (zip.Entries.Count != 2) throw new SerializationException("Unsupported archive package.");
                    var documentEntry = zip.GetEntry(PackageModel); var resultsEntry = zip.GetEntry(PackageResults);
                    if (documentEntry == null || resultsEntry == null) throw new SerializationException("Unsupported archive package.");
                    long characters = DeclaredLength(documentEntry), bytes = DeclaredLength(resultsEntry);
                    Models.Model model;
                    using (var stream = documentEntry.Open())
                    using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = characters, CloseInput = false }))
                    {
                        reader.MoveToContent();
                        if (reader.LocalName != "GpcModelArchive" || reader.GetAttribute("version") != "5") throw new SerializationException("Unsupported archive version.");
                        reader.ReadStartElement();
                        model = ((ModelDocument)ModelImage.ReadGraph(reader, typeof(ModelDocument), characters)).Restore();
                        reader.ReadEndElement();
                    }
                    using (var stream = resultsEntry.Open())
                    using (var reader = new BinaryReader(stream))
                    {
                        if (reader.ReadInt32() != ResultsMagic || reader.ReadByte() != 1) throw new SerializationException("Unsupported archive package.");
                        ModelImage.ReadResults(reader, bytes - 5, out var elements, out var results);
                        if (stream.ReadByte() != -1) throw new SerializationException("Corrupt archive package.");
                        // AttachResults reads an empty list as "no results" (images); the writer of the package lists every element.
                        if (elements.Length != model.AllElements.Count()) throw new SerializationException("ResultElementCountMismatch");
                        ModelImage.AttachResults(model, elements, results);
                    }
                    CheckReportArchive.Validate(model.CheckReports.ToArray());
                    model.Analysis?.Validate();
                    return model;
                }
            }
            catch (InvalidDataException ex) { throw new SerializationException("Corrupt archive package.", ex); }
            catch (EndOfStreamException ex) { throw new SerializationException("Corrupt archive package.", ex); }
            catch (XmlException ex) { throw new SerializationException("Corrupt archive package.", ex); }
        }

        /// <summary>The uncompressed length an entry declares, refused beyond what deflate can expand from its stored bytes
        /// (at most 1032 times): a corrupt or hostile entry.</summary>
        private static long DeclaredLength(ZipArchiveEntry entry)
        {
            long length = entry.Length;
            if (length <= 0 || entry.CompressedLength < length / Core.DomainXmlSerializer.DeflateExpansion)
                throw new SerializationException("Corrupt or oversized archive package entry: " + entry.FullName + ".");
            return length;
        }

        /// <summary>Deterministic value digest for Model data and geometry; independent of XML reference numbering.</summary>
        public static string Fingerprint(IEnumerable<object> input)
        {
            return Core.ModelValues.Fingerprint(input);
        }
    }
}
