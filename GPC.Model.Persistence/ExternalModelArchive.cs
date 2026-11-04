using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using GPC.Model.Core;
using GPC.Model.Core.Identity;
using GPC.Model.Models;
using GPC.Model.Results.Storage;
using GPC.Persistence;

namespace GPC.Model.Persistence
{
    /// <summary>Small portable document reference, containing no model or result payload. Resolve beside the owning document.
    /// A historical run remains readable; IsCurrent distinguishes it from results of the current engineering input.</summary>
    [DataContract(Namespace = "urn:gpc:external:1")]
    public sealed class ExternalModelReference
    {
        [DataMember(Order = 0)] public int Version { get; private set; }
        [DataMember(Order = 1)] public string ArchiveFile { get; private set; }
        [DataMember(Order = 2)] public string ManifestAddress { get; private set; }
        [DataMember(Order = 3)] public Guid ModelGuid { get; private set; }
        [DataMember(Order = 4)] public string AnalysisFingerprint { get; private set; }
        internal ExternalModelReference(string file, string address, Guid model, string fingerprint)
        { Version = 1; ArchiveFile = file; ManifestAddress = address; ModelGuid = model; AnalysisFingerprint = fingerprint; Validate(); }
        [OnDeserialized] private void OnRead(StreamingContext context) => Validate();
        internal void Validate()
        {
            if (Version != 1 || ModelGuid == Guid.Empty || string.IsNullOrWhiteSpace(ArchiveFile) || ArchiveFile == "." || ArchiveFile == ".." ||
                ArchiveFile.IndexOfAny(new[] { '/', '\\', ':' }) >= 0 || ArchiveFile.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                !ExternalModelArchive.IsAddress(ManifestAddress) || !ExternalModelArchive.IsAddress(AnalysisFingerprint))
                throw new SerializationException("InvalidExternalModelReference");
        }
        public bool IsCurrent(Models.Model model) => model != null && ModelGuid == model.Guid && AnalysisFingerprint == model.AnalysisFingerprint();
        public string ResolvePath(string documentPath) { Validate(); return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(documentPath)), ArchiveFile); }
        internal ExternalModelReference Rebase(string archivePath) => new ExternalModelReference(Path.GetFileName(archivePath), ManifestAddress, ModelGuid, AnalysisFingerprint);
        public void Write(Stream destination) { Validate(); new DataContractSerializer(typeof(ExternalModelReference)).WriteObject(destination, this); }
        public static ExternalModelReference Read(Stream source)
        {
            using (var reader = XmlReader.Create(source, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 8192, CloseInput = false }))
                return (ExternalModelReference)new DataContractSerializer(typeof(ExternalModelReference)).ReadObject(reader);
        }
    }

    public sealed class ExternalModelRun
    {
        public Models.Model Model { get; }
        public SolverIdentityMap SolverMap { get; }
        internal ExternalModelRun(Models.Model model, SolverIdentityMap map) { Model = model; SolverMap = map; }
    }

    /// <summary>Prepares a run once, then saves only its small references on each autosave. Heavy blocks are sidecars.
    /// The caller supplies current AND retained historical references at Save; no application retention policy is imposed.
    /// <para>Version 2 (written by <see cref="Prepare"/>): the graph of the model without its results (<see cref="ModelDocument"/>) cut
    /// into parts of at most <see cref="PartBytes"/> bytes, one block each, so a model of any size stays below the ceiling of a block
    /// (<see cref="ArchiveBlock.MaximumBytes"/>; version 1 put the graph in one block and could not prepare a model of about 25,000
    /// elements); the graph in the compact binary form (<see cref="GraphFormat"/>, XML on request or when the binary form does not
    /// reproduce it); the compact results of consecutive elements grouped in parts (one block per part, not per element; the manifest
    /// lists the parts, the elements they span and their datasets); the solver map in the form of the graph; a small manifest.
    /// Version 1 sidecars (one graph block, one results block per element) are read as before.</para>
    /// <para>Manifests are content-addressed, so each one is read at most once per session (or not at all for runs prepared here):
    /// a save costs one lookup per run, not a decode of every manifest. Failures are per run: a missing or damaged sidecar, manifest or
    /// block makes its runs unavailable at <see cref="Open(ExternalModelReference)"/> and never stops opening the session, saving or
    /// autosaving the document; blocks of a run whose manifest cannot be read are retained, never pruned.</para></summary>
    public sealed class ExternalModelArchive
    {
        public const string Extension = ".gpc-results";
        private const byte GraphKind = 2, ResultsKind = 3, ManifestKind = 4, SolverMapKind = 5, GraphPartKind = 6, ResultPartKind = 7;
        /// <summary>Largest part of a graph or of the results of a version 2 run (bytes before deflate), well below the ceiling of a block.</summary>
        public const int PartBytes = 16 * 1024 * 1024;
        private const int GraphXml = 0, GraphBinary = 1;
        private readonly BlockArchive archive;
        private readonly object gate = new object();
        // Manifest address -> the manifest, read or prepared once in this session (immutable: the address is its SHA-256).
        private readonly Dictionary<string, Manifest> manifests = new Dictionary<string, Manifest>(StringComparer.Ordinal);

        [DataContract(Namespace = "urn:gpc:external:1")]
        private sealed class Manifest
        {
            [DataMember(Order = 0)] public int Version = 1;
            [DataMember(Order = 1)] public Guid Model;
            [DataMember(Order = 2)] public string Fingerprint;
            [DataMember(Order = 3)] public string Graph;
            [DataMember(Order = 4)] public ElementBlock[] Elements;
            [DataMember(Order = 5)] public string SolverMap;
            // Version 2
            [DataMember(Order = 6, EmitDefaultValue = false)] public string[] GraphParts;
            [DataMember(Order = 7, EmitDefaultValue = false)] public int GraphFormat;
            [DataMember(Order = 8, EmitDefaultValue = false)] public long GraphLength;
            [DataMember(Order = 9, EmitDefaultValue = false)] public int ElementCount;
            [DataMember(Order = 10, EmitDefaultValue = false)] public ResultPart[] ResultParts;
            [DataMember(Order = 11, EmitDefaultValue = false)] public int SolverMapFormat;
            internal IEnumerable<string> Addresses() => Version == 1
                ? new[] { Graph, SolverMap }.Concat(Elements.Select(e => e.Address)).Where(a => a != null)
                : GraphParts.Concat(ResultParts.Select(p => p.Address)).Concat(new[] { SolverMap }).Where(a => a != null);
        }
        [DataContract(Namespace = "urn:gpc:external:1")]
        private sealed class ElementBlock
        {
            [DataMember(Order = 0)] public Guid Element;
            [DataMember(Order = 1)] public string Address;
            [DataMember(Order = 2)] public string[] Datasets;
        }
        /// <summary>Version 2: the results of the elements First to First + Count - 1 that have results, in one block.</summary>
        [DataContract(Namespace = "urn:gpc:external:1")]
        private sealed class ResultPart
        {
            [DataMember(Order = 0)] public string Address;
            [DataMember(Order = 1)] public int First;
            [DataMember(Order = 2)] public int Count;
            [DataMember(Order = 3)] public string[] Datasets;
        }

        /// <param name="archivePath">Actual sidecar path; null for an unsaved document.</param>
        /// <param name="savedRuns">References in the already saved document, including its history. A run whose manifest cannot be read
        /// (missing or damaged sidecar) does not stop the session: the first save then retains every block of the sidecar.</param>
        public ExternalModelArchive(string archivePath = null, IEnumerable<ExternalModelReference> savedRuns = null)
        {
            archive = new BlockArchive(archivePath);
            if (savedRuns != null)
            {
                var addresses = Addresses(savedRuns, out bool unresolved);
                if (!unresolved) archive = new BlockArchive(archivePath, addresses);
            }
        }
        public static string PathFor(string documentPath) => Path.ChangeExtension(Path.GetFullPath(documentPath), Extension);

        /// <summary>The form of the graph and of the solver map of the runs prepared from now on: <see cref="ModelArchiveFormat.Compact"/>
        /// (default, read from GPC.Model.Persistence 3.14 on) or <see cref="ModelArchiveFormat.Xml"/>. Both are version 2 runs.</summary>
        public ModelArchiveFormat GraphFormat { get; set; } = ModelArchiveFormat.Compact;

        /// <summary>The outcome of the last save: unavailable blocks, a sidecar set aside because it could not be read as one.</summary>
        public ArchiveSaveReport LastSave => archive.LastSave;

        public ExternalModelReference Prepare(Models.Model model, SolverIdentityMap solverMap = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (GraphFormat != ModelArchiveFormat.Xml && GraphFormat != ModelArchiveFormat.Compact) throw new InvalidOperationException("UnsupportedGraphFormat");
            CheckReportArchive.Validate(model.CheckReports.ToArray());
            string fingerprint = model.AnalysisFingerprint();
            if (model.Analysis != null && model.Analysis.InputFingerprint != fingerprint) throw new InvalidOperationException("StaleAnalysisResults");
            ValidateMap(model, solverMap, fingerprint);
            var elements = model.AllElements.ToArray();
            var results = ModelImage.CaptureResults(model, elements);
            var manifest = new Manifest { Version = 2, Model = model.Guid, Fingerprint = fingerprint, ElementCount = elements.Length };
            bool binary = GraphFormat == ModelArchiveFormat.Compact;
            var graph = Write(stream => ModelImage.WriteBinaryGraph(stream, ModelDocument.Capture(model), typeof(ModelDocument), elements), ref binary,
                stream => ModelImage.WriteGraph(stream, ModelDocument.Capture(model), typeof(ModelDocument), elements));
            manifest.GraphFormat = binary ? GraphBinary : GraphXml; manifest.GraphLength = graph.Length;
            // The parts are deflated in parallel, each one alone (a block holds one part).
            manifest.GraphParts = AddAll(graph.Parts.AsParallel().AsOrdered().Select(p => ArchiveBlock.Create(GraphPartKind, p, true)).ToArray());
            manifest.ResultParts = ResultParts(results);
            if (solverMap != null)
            {
                bool compact = GraphFormat == ModelArchiveFormat.Compact;
                var map = Write(stream => ModelImage.WriteBinaryGraph(stream, solverMap, typeof(SolverIdentityMap), null), ref compact,
                    stream => ModelArchive.Serializer(typeof(SolverIdentityMap)).WriteObject(stream, solverMap));
                manifest.SolverMapFormat = compact ? GraphBinary : GraphXml;
                manifest.SolverMap = Add(SolverMapKind, Join(map.Parts), true);
            }
            string address;
            using (var stream = new MemoryStream())
            { new DataContractSerializer(typeof(Manifest)).WriteObject(stream, manifest); address = Add(ManifestKind, stream.ToArray(), true); }
            lock (gate) manifests[address] = manifest;
            return new ExternalModelReference(archive.FilePath == null ? "unsaved" + Extension : Path.GetFileName(archive.FilePath), address, model.Guid, fingerprint);
        }

        /// <summary>The bytes <paramref name="binary"/> writes, or (when <paramref name="compact"/> is false or the binary form does not reproduce
        /// the graph) those <paramref name="xml"/> writes; <paramref name="compact"/> tells which.</summary>
        private static PartStream Write(Action<Stream> binary, ref bool compact, Action<Stream> xml)
        {
            if (compact)
            {
                var parts = new PartStream(PartBytes);
                try { binary(parts); parts.Close(); return parts; }
                catch (ContractCopy.UnsupportedException) { compact = false; }
            }
            var text = new PartStream(PartBytes); xml(text); text.Close(); return text;
        }

        private string[] AddAll(ArchiveBlock[] blocks) { foreach (var block in blocks) archive.Add(block); return blocks.Select(b => b.Address).ToArray(); }

        /// <summary>Consecutive elements with results, in parts of about <see cref="PartBytes"/> raw bytes, one block each, deflated in
        /// parallel: the section of <see cref="ModelImage.WriteResultSection"/> (raw segments, deflated as a whole, so the layouts every
        /// segment repeats compress across elements).</summary>
        private ResultPart[] ResultParts(ResultStorage[] results)
        {
            var spans = new List<KeyValuePair<int, int>>();
            int at = 0;
            while (at < results.Length)
            {
                while (at < results.Length && results[at].Count == 0) at++;
                if (at == results.Length) break;
                int first = at, last = at; long bytes = 0;
                for (int i = at; i < results.Length && bytes < PartBytes; i++)
                {
                    if (results[i].Count == 0) continue;
                    foreach (var segment in results[i].Segments) bytes += segment.Length + 16;
                    last = i;
                }
                spans.Add(new KeyValuePair<int, int>(first, last));
                at = last + 1;
            }
            var parts = spans.AsParallel().AsOrdered().Select(span =>
            {
                using (var stream = new MemoryStream())
                {
                    ModelImage.WriteResultSection(stream, results, span.Key, span.Value);
                    var datasets = results.Skip(span.Key).Take(span.Value - span.Key + 1).SelectMany(r => r.Segments).SelectMany(s => s.Datasets)
                        .Distinct(StringComparer.Ordinal).OrderBy(s => s, StringComparer.Ordinal).ToArray();
                    return new KeyValuePair<ArchiveBlock, ResultPart>(ArchiveBlock.Create(ResultPartKind, stream.ToArray(), true),
                        new ResultPart { First = span.Key, Count = span.Value - span.Key + 1, Datasets = datasets });
                }
            }).ToArray();
            foreach (var part in parts) { archive.Add(part.Key); part.Value.Address = part.Key.Address; }
            return parts.Select(p => p.Value).ToArray();
        }

        private static void ReadResultPart(byte[] bytes, ResultPart part, ResultStorage[] results)
        {
            using (var stream = new MemoryStream(bytes, false))
            {
                ModelImage.ReadResultSection(stream, bytes.Length, results, part.First, part.Count);
                if (stream.Position != stream.Length) throw new SerializationException("CorruptExternalResultPart");
            }
            for (int i = part.First; i < part.First + part.Count; i++)
                if (results[i] != null && !results[i].Segments.SelectMany(s => s.Datasets).All(d => part.Datasets.Contains(d, StringComparer.Ordinal)))
                    throw new SerializationException("ExternalResultManifestMismatch");
        }

        private static byte[] Join(IReadOnlyList<byte[]> parts)
        {
            if (parts.Count == 1) return parts[0];
            var all = new byte[parts.Sum(p => (long)p.Length)]; long at = 0;
            foreach (var part in parts) { Buffer.BlockCopy(part, 0, all, (int)at, part.Length); at += part.Length; }
            return all;
        }

        /// <summary>Save/Save As. Callback receives references rebased to the destination name; write them atomically in the document.
        /// The returned references can be committed to the application's session after success. Reuse them for unchanged autosave.
        /// A run whose blocks are not all available is saved as it is (the document keeps citing it; <see cref="LastSave"/> counts the
        /// missing blocks): one unavailable run never stops the save of the document.</summary>
        public IReadOnlyList<ExternalModelReference> Save(string archivePath, IEnumerable<ExternalModelReference> liveRuns, Action<IReadOnlyList<ExternalModelReference>> writeDocument)
        {
            if (writeDocument == null || liveRuns == null) throw new ArgumentNullException();
            var runs = liveRuns.ToArray(); var addresses = Addresses(runs, out bool unresolved);
            var rebased = Array.AsReadOnly(runs.Select(r => r.Rebase(archivePath)).ToArray());
            archive.Save(archivePath, addresses, () => writeDocument(rebased), requireComplete: false, retainExisting: unresolved);
            return rebased;
        }

        /// <summary>Prepares a sidecar for a host-controlled document save. The host must write the returned
        /// relative references in its document. All existing sidecar blocks are retained because the host's
        /// final commit is not observable here; a failed or repeated host save cannot prune earlier results.</summary>
        public IReadOnlyList<ExternalModelReference> SaveSnapshot(string archivePath, IEnumerable<ExternalModelReference> liveRuns)
        {
            if (liveRuns == null) throw new ArgumentNullException(nameof(liveRuns));
            var runs = liveRuns.ToArray();
            var addresses = Addresses(runs, out _);
            var rebased = Array.AsReadOnly(runs.Select(r => r.Rebase(archivePath)).ToArray());
            archive.SaveSnapshot(archivePath, addresses);
            return rebased;
        }

        /// <summary>Opens an unsaved prepared run or a run in this session, failing explicitly on any unavailable block.</summary>
        public ExternalModelRun Open(ExternalModelReference reference)
        {
            var manifest = ReadManifest(reference);
            return manifest.Version == 1 ? OpenVersion1(reference, manifest) : OpenVersion2(reference, manifest);
        }

        private ExternalModelRun OpenVersion2(ExternalModelReference reference, Manifest manifest)
        {
            // One read of the sidecar for the graph and the results (one pass in file order, decoded in parallel).
            var addresses = manifest.GraphParts.Concat(manifest.ResultParts.Select(p => p.Address)).ToArray();
            var contents = archive.Read(addresses);
            var parts = new byte[manifest.GraphParts.Length][];
            for (int i = 0; i < parts.Length; i++) parts[i] = Bytes(addresses[i], contents[i], GraphPartKind);
            if (parts.Sum(p => (long)p.Length) != manifest.GraphLength) throw new SerializationException("ExternalModelGraphLengthMismatch");
            Models.Model model;
            using (var stream = new PartReader(parts))
                model = (manifest.GraphFormat == GraphBinary
                    ? (ModelDocument)ModelImage.ReadBinaryGraph(stream, manifest.GraphLength, typeof(ModelDocument))
                    : (ModelDocument)ModelImage.ReadGraph(stream, typeof(ModelDocument), manifest.GraphLength)).Restore();
            if (model.Guid != reference.ModelGuid || model.AnalysisFingerprint() != reference.AnalysisFingerprint) throw new SerializationException("ExternalModelFingerprintMismatch");
            var elements = model.AllElements.ToArray();
            if (elements.Length != manifest.ElementCount) throw new SerializationException("ExternalResultElementCountMismatch");
            var results = new ResultStorage[elements.Length];
            for (int p = 0; p < manifest.ResultParts.Length; p++)
            {
                var part = manifest.ResultParts[p];
                if (part.First + (long)part.Count > elements.Length) throw new SerializationException("ExternalResultManifestMismatch");
                ReadResultPart(Bytes(part.Address, contents[parts.Length + p], ResultPartKind), part, results);
            }
            for (int i = 0; i < results.Length; i++) if (results[i] == null) results[i] = ResultStorage.Empty;
            ModelImage.AttachResults(model, elements.Select(e => e.Guid).ToArray(), results);
            SolverIdentityMap map = null;
            if (manifest.SolverMap != null)
            {
                byte[] bytes = Get(manifest.SolverMap, SolverMapKind);
                using (var stream = new MemoryStream(bytes, false))
                    map = manifest.SolverMapFormat == GraphBinary ? (SolverIdentityMap)ModelImage.ReadBinaryGraph(stream, bytes.Length, typeof(SolverIdentityMap))
                        : (SolverIdentityMap)Core.ModelValues.DocumentSerializer(typeof(SolverIdentityMap), bytes.Length).ReadObject(stream, bytes.Length);
                if (map == null) throw new SerializationException("InvalidExternalModelManifest");
            }
            ValidateMap(model, map, manifest.Fingerprint); model.Analysis?.Validate(); CheckReportArchive.Validate(model.CheckReports.ToArray());
            return new ExternalModelRun(model, map);
        }

        private ExternalModelRun OpenVersion1(ExternalModelReference reference, Manifest manifest)
        {
            Models.Model model; byte[] graph = Get(manifest.Graph, GraphKind);
            using (var stream = new MemoryStream(graph, false)) model = ((ModelDocument)ModelImage.ReadGraph(stream, typeof(ModelDocument), graph.Length)).Restore();
            if (model.Guid != reference.ModelGuid || model.AnalysisFingerprint() != reference.AnalysisFingerprint) throw new SerializationException("ExternalModelFingerprintMismatch");
            var ids = new Guid[manifest.Elements.Length]; var results = new ResultStorage[ids.Length];
            // One read of the sidecar for all element blocks (one pass in file order, decoded in parallel), not one per element.
            var stored = manifest.Elements.Where(e => e.Address != null).Select(e => e.Address).Distinct(StringComparer.Ordinal).ToArray();
            var contents = archive.Read(stored); var byAddress = new Dictionary<string, BlockContent>(stored.Length, StringComparer.Ordinal);
            for (int i = 0; i < stored.Length; i++) byAddress.Add(stored[i], contents[i]);
            for (int i = 0; i < ids.Length; i++)
            {
                var entry = manifest.Elements[i]; ids[i] = entry.Element;
                if (entry.Address == null) { results[i] = ResultStorage.Empty; continue; }
                byte[] bytes = Bytes(entry.Address, byAddress[entry.Address], (byte)(ResultsKind | 128));
                using (var stream = new MemoryStream(bytes, false))
                using (var reader = new BinaryReader(stream))
                {
                    ModelImage.ReadResults(reader, bytes.Length, out var storedIds, out var storedResults);
                    if (stream.Position != stream.Length || storedIds.Length != 1 || storedIds[0] != entry.Element ||
                        !storedResults[0].Segments.SelectMany(s => s.Datasets).Distinct(StringComparer.Ordinal).OrderBy(s => s, StringComparer.Ordinal).SequenceEqual(entry.Datasets))
                        throw new SerializationException("ExternalResultManifestMismatch");
                    results[i] = storedResults[0];
                }
            }
            if (ids.Length != model.AllElements.Count()) throw new SerializationException("ExternalResultElementCountMismatch");
            ModelImage.AttachResults(model, ids, results);
            SolverIdentityMap map = null;
            if (manifest.SolverMap != null)
            {
                byte[] bytes = Get(manifest.SolverMap, SolverMapKind);
                using (var stream = new MemoryStream(bytes, false)) map = (SolverIdentityMap)Core.ModelValues.DocumentSerializer(typeof(SolverIdentityMap), bytes.Length).ReadObject(stream, bytes.Length);
            }
            ValidateMap(model, map, manifest.Fingerprint); model.Analysis?.Validate(); CheckReportArchive.Validate(model.CheckReports.ToArray());
            return new ExternalModelRun(model, map);
        }

        public static ExternalModelRun Open(string documentPath, ExternalModelReference reference)
            => new ExternalModelArchive(reference.ResolvePath(documentPath)).Open(reference);

        /// <summary>Addresses of the compact result blocks containing a dataset, without decoding numerical results (version 2: the parts
        /// with an element of that dataset; version 1: the blocks of those elements).</summary>
        public IReadOnlyList<string> BlocksForDataset(ExternalModelReference reference, string dataset)
        {
            var manifest = ReadManifest(reference);
            return manifest.Version == 1
                ? Array.AsReadOnly(manifest.Elements.Where(e => e.Address != null && e.Datasets.Contains(dataset, StringComparer.Ordinal)).Select(e => e.Address).Distinct().ToArray())
                : Array.AsReadOnly(manifest.ResultParts.Where(p => p.Datasets.Contains(dataset, StringComparer.Ordinal)).Select(p => p.Address).Distinct().ToArray());
        }
        private string Add(byte kind, byte[] bytes, bool compress)
        { var block = ArchiveBlock.Create(kind, bytes, compress); archive.Add(block); return block.Address; }
        private byte[] Get(string address, byte kind) => Bytes(address, archive.Read(new[] { address })[0], kind);
        private static byte[] Bytes(string address, BlockContent content, byte kind)
        {
            if (content.Failure != null) throw new InvalidDataException("ExternalModelDataUnavailable: " + address + ": " + content.Failure);
            if (content.Kind != kind) throw new InvalidDataException("ExternalModelBlockKindMismatch");
            return content.Bytes;
        }
        private Manifest ReadManifest(ExternalModelReference reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference)); reference.Validate();
            Manifest manifest;
            lock (gate) manifests.TryGetValue(reference.ManifestAddress, out manifest);
            if (manifest == null)
            {
                byte[] bytes = Get(reference.ManifestAddress, ManifestKind);
                using (var stream = new MemoryStream(bytes, false))
                using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = Math.Max(1, bytes.Length) }))
                    manifest = (Manifest)new DataContractSerializer(typeof(Manifest), new DataContractSerializerSettings { MaxItemsInObjectGraph = Math.Max(1, bytes.Length) }).ReadObject(reader);
                if (!(manifest.Version == 1 ? ValidVersion1(manifest) : manifest.Version == 2 && ValidVersion2(manifest)))
                    throw new SerializationException("InvalidExternalModelManifest");
                lock (gate) manifests[reference.ManifestAddress] = manifest;
            }
            if (manifest.Model != reference.ModelGuid || manifest.Fingerprint != reference.AnalysisFingerprint) throw new SerializationException("InvalidExternalModelManifest");
            return manifest;
        }

        private static bool ValidVersion1(Manifest manifest) =>
            IsAddress(manifest.Graph) && manifest.Elements != null && (manifest.SolverMap == null || IsAddress(manifest.SolverMap)) &&
            !manifest.Elements.Any(e => e == null || e.Element == Guid.Empty || e.Address != null && !IsAddress(e.Address) || e.Datasets == null || e.Datasets.Any(d => d == null)) &&
            manifest.Elements.Select(e => e.Element).Distinct().Count() == manifest.Elements.Length &&
            manifest.GraphParts == null && manifest.ResultParts == null;

        /// <summary>Version 2: a graph in parts of known form and length, result parts in element order without overlaps.</summary>
        private static bool ValidVersion2(Manifest manifest)
        {
            if (manifest.Graph != null || manifest.Elements != null || manifest.GraphParts == null || manifest.GraphParts.Length == 0 ||
                manifest.GraphParts.Any(a => !IsAddress(a)) || manifest.GraphFormat != GraphXml && manifest.GraphFormat != GraphBinary ||
                manifest.GraphLength <= 0 || manifest.GraphLength > (long)manifest.GraphParts.Length * PartBytes || manifest.ElementCount < 0 ||
                manifest.ResultParts == null || manifest.SolverMap != null && !IsAddress(manifest.SolverMap) ||
                manifest.SolverMapFormat != GraphXml && manifest.SolverMapFormat != GraphBinary) return false;
            long next = 0;
            foreach (var part in manifest.ResultParts)
            {
                if (part == null || !IsAddress(part.Address) || part.First < next || part.Count <= 0 || part.First + (long)part.Count > manifest.ElementCount ||
                    part.Datasets == null || part.Datasets.Any(d => d == null)) return false;
                next = part.First + (long)part.Count;
            }
            return true;
        }

        /// <summary>The blocks of the runs, from the manifests of this session. A run whose manifest cannot be read keeps its manifest
        /// address and sets <paramref name="unresolved"/>: its other blocks are unknown, so the caller retains the whole sidecar.</summary>
        private string[] Addresses(IEnumerable<ExternalModelReference> runs, out bool unresolved)
        {
            unresolved = false; var addresses = new HashSet<string>(StringComparer.Ordinal);
            foreach (var run in runs)
            {
                if (run == null) throw new ArgumentNullException(nameof(runs));
                addresses.Add(run.ManifestAddress);
                try { addresses.UnionWith(ReadManifest(run).Addresses()); }
                catch (Exception error) when (error is InvalidDataException || error is SerializationException || error is XmlException) { unresolved = true; }
            }
            return addresses.ToArray();
        }
        internal static bool IsAddress(string value) => value != null && value.Length == 64 && value.All(c => c >= '0' && c <= '9' || c >= 'A' && c <= 'F');
        private static void ValidateMap(Models.Model model, SolverIdentityMap map, string fingerprint)
        {
            if (map == null) return;
            map.Validate();
            if (model.AssemblyMap == null || map.ModelGuid != model.Guid || map.AnalysisFingerprint != fingerprint) throw new InvalidOperationException("StaleSolverIdentityMap");
            // fingerprint is the analysis fingerprint of the model, computed by the caller (and checked against the map above): it is not
            // computed again, and the maps are compared member by member (the members of their fingerprint contract).
            var expected = model.AssemblyMap.ToSolver(model, map.Solver, map.Entries.Select(e => new KeyValuePair<Guid, int>(e.EntityGuid, e.SolverId)), fingerprint);
            if (!expected.SameContent(map)) throw new InvalidOperationException("SolverIdentityMapMismatch");
        }

        /// <summary>Write-only stream that keeps what is written in arrays of at most a part each.</summary>
        private sealed class PartStream : Stream
        {
            private readonly int _part;
            private readonly List<byte[]> _parts = new List<byte[]>();
            private byte[] _current; private int _at;
            private long _length;
            internal PartStream(int part) { _part = part; _current = new byte[Math.Min(part, 1 << 20)]; }
            internal IReadOnlyList<byte[]> Parts => _parts;
            public override void Write(byte[] buffer, int offset, int count)
            {
                while (count > 0)
                {
                    if (_at == _current.Length)
                    {
                        if (_current.Length < _part) Array.Resize(ref _current, (int)Math.Min(_part, 2L * _current.Length));
                        else { _parts.Add(_current); _current = new byte[_part]; _at = 0; }
                    }
                    int n = Math.Min(count, _current.Length - _at);
                    Buffer.BlockCopy(buffer, offset, _current, _at, n); _at += n; offset += n; count -= n; _length += n;
                }
            }
            public override void Close()
            {
                if (_current != null) { if (_at > 0 || _parts.Count == 0) { Array.Resize(ref _current, _at); _parts.Add(_current); } _current = null; }
                base.Close();
            }
            public override void Flush() { }
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => _current != null;
            public override long Length => _length;
            public override long Position { get => _length; set => throw new NotSupportedException(); }
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }

        /// <summary>Read-only stream over the parts of a graph, in order.</summary>
        private sealed class PartReader : Stream
        {
            private readonly byte[][] _parts;
            private int _part, _at;
            internal PartReader(byte[][] parts) { _parts = parts; }
            public override int Read(byte[] buffer, int offset, int count)
            {
                while (_part < _parts.Length && _at == _parts[_part].Length) { _part++; _at = 0; }
                if (_part == _parts.Length || count == 0) return 0;
                int n = Math.Min(count, _parts[_part].Length - _at);
                Buffer.BlockCopy(_parts[_part], _at, buffer, offset, n); _at += n; return n;
            }
            public override void Flush() { }
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
