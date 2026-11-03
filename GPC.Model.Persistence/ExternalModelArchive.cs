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
    /// One graph block, one compact results block per nonempty element, an optional solver map and a small manifest.
    /// Result blocks use ModelImage.WriteResults/ReadResults unchanged: no JSON conversion, no second deflate codec.
    /// Dataset names index the element blocks; cases/combinations keep their identities in the graph and the existing codec.
    /// The caller supplies current AND retained historical references at Save; no application retention policy is imposed.</summary>
    public sealed class ExternalModelArchive
    {
        public const string Extension = ".gpc-results";
        private const byte GraphKind = 2, ResultsKind = 3, ManifestKind = 4, SolverMapKind = 5;
        private readonly BlockArchive archive;

        [DataContract(Namespace = "urn:gpc:external:1")]
        private sealed class Manifest
        {
            [DataMember(Order = 0)] public int Version = 1;
            [DataMember(Order = 1)] public Guid Model;
            [DataMember(Order = 2)] public string Fingerprint;
            [DataMember(Order = 3)] public string Graph;
            [DataMember(Order = 4)] public ElementBlock[] Elements;
            [DataMember(Order = 5)] public string SolverMap;
            internal IEnumerable<string> Addresses() => new[] { Graph, SolverMap }.Concat(Elements.Select(e => e.Address)).Where(a => a != null);
        }
        [DataContract(Namespace = "urn:gpc:external:1")]
        private sealed class ElementBlock
        {
            [DataMember(Order = 0)] public Guid Element;
            [DataMember(Order = 1)] public string Address;
            [DataMember(Order = 2)] public string[] Datasets;
        }

        /// <param name="archivePath">Actual sidecar path; null for an unsaved document.</param>
        /// <param name="savedRuns">References in the already saved document, including its history.</param>
        public ExternalModelArchive(string archivePath = null, IEnumerable<ExternalModelReference> savedRuns = null)
        {
            archive = new BlockArchive(archivePath);
            if (savedRuns != null)
            {
                var addresses = LiveAddresses(savedRuns);
                archive = new BlockArchive(archivePath, addresses);
            }
        }
        public static string PathFor(string documentPath) => Path.ChangeExtension(Path.GetFullPath(documentPath), Extension);

        public ExternalModelReference Prepare(Models.Model model, SolverIdentityMap solverMap = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            CheckReportArchive.Validate(model.CheckReports.ToArray());
            string fingerprint = model.AnalysisFingerprint();
            if (model.Analysis != null && model.Analysis.InputFingerprint != fingerprint) throw new InvalidOperationException("StaleAnalysisResults");
            ValidateMap(model, solverMap, fingerprint);
            var elements = model.AllElements.ToArray();
            var results = ModelImage.CaptureResults(model, elements);
            var manifest = new Manifest { Model = model.Guid, Fingerprint = fingerprint, Elements = new ElementBlock[elements.Length] };
            using (var stream = new MemoryStream())
            {
                ModelImage.WriteGraph(stream, ModelDocument.Capture(model), typeof(ModelDocument), elements);
                manifest.Graph = Add(GraphKind, stream.ToArray(), true);
            }
            for (int i = 0; i < elements.Length; i++)
            {
                var entry = new ElementBlock { Element = elements[i].Guid, Datasets = results[i].Segments.SelectMany(s => s.Datasets).Distinct(StringComparer.Ordinal).OrderBy(s => s, StringComparer.Ordinal).ToArray() };
                if (results[i].Count != 0)
                    using (var stream = new MemoryStream())
                    {
                        using (var writer = new BinaryWriter(stream, System.Text.Encoding.UTF8, true)) ModelImage.WriteResults(writer, new[] { elements[i].Guid }, new[] { results[i] });
                        entry.Address = Add(ResultsKind, stream.ToArray(), false);
                    }
                manifest.Elements[i] = entry;
            }
            if (solverMap != null)
                using (var stream = new MemoryStream())
                { ModelArchive.Serializer(typeof(SolverIdentityMap)).WriteObject(stream, solverMap); manifest.SolverMap = Add(SolverMapKind, stream.ToArray(), true); }
            string address;
            using (var stream = new MemoryStream())
            { new DataContractSerializer(typeof(Manifest)).WriteObject(stream, manifest); address = Add(ManifestKind, stream.ToArray(), true); }
            return new ExternalModelReference(archive.FilePath == null ? "unsaved" + Extension : Path.GetFileName(archive.FilePath), address, model.Guid, fingerprint);
        }

        /// <summary>Save/Save As. Callback receives references rebased to the destination name; write them atomically in the document.
        /// The returned references can be committed to the application's session after success. Reuse them for unchanged autosave.</summary>
        public IReadOnlyList<ExternalModelReference> Save(string archivePath, IEnumerable<ExternalModelReference> liveRuns, Action<IReadOnlyList<ExternalModelReference>> writeDocument)
        {
            if (writeDocument == null || liveRuns == null) throw new ArgumentNullException();
            var runs = liveRuns.ToArray(); var addresses = LiveAddresses(runs);
            var rebased = Array.AsReadOnly(runs.Select(r => r.Rebase(archivePath)).ToArray());
            archive.Save(archivePath, addresses, () => writeDocument(rebased), requireComplete: true);
            return rebased;
        }

        /// <summary>Opens an unsaved prepared run or a run in this session, failing explicitly on any unavailable block.</summary>
        public ExternalModelRun Open(ExternalModelReference reference)
        {
            var manifest = ReadManifest(reference);
            Models.Model model; byte[] graph = Get(manifest.Graph, GraphKind);
            using (var stream = new MemoryStream(graph, false)) model = ((ModelDocument)ModelImage.ReadGraph(stream, typeof(ModelDocument), graph.Length)).Restore();
            if (model.Guid != reference.ModelGuid || model.AnalysisFingerprint() != reference.AnalysisFingerprint) throw new SerializationException("ExternalModelFingerprintMismatch");
            var ids = new Guid[manifest.Elements.Length]; var results = new ResultStorage[ids.Length];
            for (int i = 0; i < ids.Length; i++)
            {
                var entry = manifest.Elements[i]; ids[i] = entry.Element;
                if (entry.Address == null) { results[i] = ResultStorage.Empty; continue; }
                byte[] bytes = Get(entry.Address, (byte)(ResultsKind | 128));
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

        /// <summary>Addresses of compact element blocks containing a dataset, without decoding numerical results.</summary>
        public IReadOnlyList<string> BlocksForDataset(ExternalModelReference reference, string dataset)
            => Array.AsReadOnly(ReadManifest(reference).Elements.Where(e => e.Address != null && e.Datasets.Contains(dataset, StringComparer.Ordinal)).Select(e => e.Address).Distinct().ToArray());
        private string Add(byte kind, byte[] bytes, bool compress)
        { var block = ArchiveBlock.Create(kind, bytes, compress); archive.Add(block); return block.Address; }
        private byte[] Get(string address, byte kind)
        {
            var content = archive.Read(new[] { address })[0];
            if (content.Failure != null) throw new InvalidDataException("ExternalModelDataUnavailable: " + address + ": " + content.Failure);
            if (content.Kind != kind) throw new InvalidDataException("ExternalModelBlockKindMismatch");
            return content.Bytes;
        }
        private Manifest ReadManifest(ExternalModelReference reference)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference)); reference.Validate();
            byte[] bytes = Get(reference.ManifestAddress, ManifestKind); Manifest manifest;
            using (var stream = new MemoryStream(bytes, false))
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = Math.Max(1, bytes.Length) }))
                manifest = (Manifest)new DataContractSerializer(typeof(Manifest), new DataContractSerializerSettings { MaxItemsInObjectGraph = Math.Max(1, bytes.Length) }).ReadObject(reader);
            if (manifest.Version != 1 || manifest.Model != reference.ModelGuid || manifest.Fingerprint != reference.AnalysisFingerprint || !IsAddress(manifest.Graph) || manifest.Elements == null ||
                manifest.SolverMap != null && !IsAddress(manifest.SolverMap) || manifest.Elements.Any(e => e == null || e.Element == Guid.Empty || e.Address != null && !IsAddress(e.Address) || e.Datasets == null || e.Datasets.Any(d => d == null)) ||
                manifest.Elements.Select(e => e.Element).Distinct().Count() != manifest.Elements.Length)
                throw new SerializationException("InvalidExternalModelManifest");
            return manifest;
        }
        private string[] LiveAddresses(IEnumerable<ExternalModelReference> runs)
            => runs.SelectMany(r => ReadManifest(r).Addresses().Concat(new[] { r.ManifestAddress })).Distinct(StringComparer.Ordinal).ToArray();
        internal static bool IsAddress(string value) => value != null && value.Length == 64 && value.All(c => c >= '0' && c <= '9' || c >= 'A' && c <= 'F');
        private static void ValidateMap(Models.Model model, SolverIdentityMap map, string fingerprint)
        {
            if (map == null) return;
            map.Validate();
            if (model.AssemblyMap == null || map.ModelGuid != model.Guid || map.AnalysisFingerprint != fingerprint) throw new InvalidOperationException("StaleSolverIdentityMap");
            var expected = model.AssemblyMap.ToSolver(model, map.Solver, map.Entries.Select(e => new KeyValuePair<Guid, int>(e.EntityGuid, e.SolverId)));
            if (ModelValues.Fingerprint(new object[] { expected }) != ModelValues.Fingerprint(new object[] { map })) throw new InvalidOperationException("SolverIdentityMapMismatch");
        }
    }
}
