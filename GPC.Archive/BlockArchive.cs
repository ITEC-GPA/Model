using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPC.Persistence
{
    public sealed class BlockContent
    {
        public byte[]? Bytes { get; }
        public string? Failure { get; }
        public byte Kind { get; }
        internal BlockContent(byte[]? bytes, string? failure, byte kind = 0) { Bytes = bytes; Failure = failure; Kind = kind; }
    }

    /// <summary>Domain-independent content-addressed archive, extracted from ANTHEA ModelResultArchive format 1.
    /// Caller owns document schemas, codecs, revisions and retention policy. One instance per open document.
    /// Save serializes all session operations, writes the archive first and invokes an atomic document writer second.
    /// Previous document blocks survive an interrupted save; retired blocks remain available for undo in this session.</summary>
    public sealed class BlockArchive
    {
        public const string DefaultMark = "GPC.BLOCKS";
        private const int HeaderBytes = 64, EntryBytes = 57;
        private readonly object gate = new object();
        private readonly byte[] mark;
        private readonly Dictionary<string, ArchiveBlock> memory = new Dictionary<string, ArchiveBlock>(StringComparer.Ordinal);
        private string? file;
        private HashSet<string> savedReferences;
        private bool saving;
        private bool retainExistingOnFirstSave;
        private readonly Dictionary<string, IndexCache> cache = new Dictionary<string, IndexCache>(StringComparer.OrdinalIgnoreCase);
        private sealed class IndexCache
        {
            internal string Digest = "";
            internal long Length, Offset, Stamp;
            internal int Count;
            internal Dictionary<string, Entry> Entries = null!;
        }

        private sealed class Entry
        {
            internal byte Kind;
            internal long Offset, Stored, Length;
            internal Entry(byte kind, long offset, long stored, long length) { Kind = kind; Offset = offset; Stored = stored; Length = length; }
        }

        public BlockArchive(string? archivePath = null, IEnumerable<string>? savedReferences = null, string mark = DefaultMark)
        {
            if (string.IsNullOrEmpty(mark) || mark.Length > 16 || mark.Any(c => c < 32 || c > 126)) throw new ArgumentException("Invalid archive mark.", nameof(mark));
            this.mark = new byte[16]; Encoding.ASCII.GetBytes(mark).CopyTo(this.mark, 0);
            file = archivePath == null ? null : Path.GetFullPath(archivePath);
            retainExistingOnFirstSave = savedReferences == null;
            this.savedReferences = Addresses(savedReferences ?? Array.Empty<string>());
        }
        public string? FilePath { get { lock (gate) return file; } }
        public void Add(ArchiveBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            lock (gate) if (!memory.ContainsKey(block.Address)) memory.Add(block.Address, block);
        }
        public BlockContent[] Read(IReadOnlyList<string> addresses)
        {
            if (addresses == null) throw new ArgumentNullException(nameof(addresses));
            var contents = new BlockContent[addresses.Count];
            var blocks = new ArchiveBlock?[addresses.Count];
            lock (gate)
            {
                Dictionary<string, Entry>? index = null; string? failure = null; bool loaded = false;
                var disk = new List<KeyValuePair<int, Entry>>();
                for (int i = 0; i < addresses.Count; i++)
                {
                    ArchiveBlock.ValidateAddress(addresses[i]);
                    if (memory.TryGetValue(addresses[i], out var block)) { blocks[i] = block; continue; }
                    if (!loaded) { failure = LoadIndex(file, out index); loaded = true; }
                    if (failure == null && index!.TryGetValue(addresses[i], out var entry)) disk.Add(new KeyValuePair<int, Entry>(i, entry));
                    else contents[i] = new BlockContent(null, failure ?? "blocco assente dall'archivio dei risultati " + Path.GetFileName(file));
                }
                if (disk.Count > 0)
                    try
                    {
                        using (var input = OpenRead(file!))
                            foreach (var item in disk.OrderBy(e => e.Value.Offset)) blocks[item.Key] = ReadBlock(input, addresses[item.Key], item.Value);
                    }
                    catch (Exception error) when (IoError(error))
                    { foreach (var item in disk) contents[item.Key] = new BlockContent(null, "archivio dei risultati non leggibile: " + error.Message); }
            }
            Parallel.For(0, addresses.Count, i =>
            {
                if (blocks[i] == null || contents[i]?.Failure != null) return;
                try { contents[i] = new BlockContent(blocks[i]!.Decode(), null, blocks[i]!.Kind); }
                catch (Exception error) when (IoError(error)) { contents[i] = new BlockContent(null, "blocco danneggiato (" + error.Message + ")"); }
            });
            return contents;
        }

        /// <summary>Saves the given live references, plus blocks needed by the previously saved document. Returns unavailable count.
        /// A different existing destination is preserved in full until the next save (Save As may overwrite another document).
        /// A failed callback leaves this session pointing at its original path. The callback must replace its document atomically.</summary>
        public int Save(string archivePath, IEnumerable<string> references, Action writeDocument, bool requireComplete = false)
            => SaveCore(archivePath, references, writeDocument, requireComplete, false);

        /// <summary>Writes blocks before a host-controlled document save whose completion cannot be observed.
        /// Retains the entire destination history on every call. No document commit or pruning is inferred.
        /// A later explicit Save with an atomic document callback may resume the normal retention policy.</summary>
        public int SaveSnapshot(string archivePath, IEnumerable<string> references, bool requireComplete = false)
            => SaveCore(archivePath, references, () => { }, requireComplete, true);

        private int SaveCore(string archivePath, IEnumerable<string> references, Action writeDocument, bool requireComplete, bool retainHistory)
        {
            if (writeDocument == null) throw new ArgumentNullException(nameof(writeDocument));
            var current = Addresses(references); string target = Path.GetFullPath(archivePath);
            lock (gate)
            {
                if (saving) throw new InvalidOperationException("Recursive archive save.");
                saving = true;
                try
                {
                    bool same = string.Equals(file, target, StringComparison.OrdinalIgnoreCase);
                    var keep = new SortedSet<string>(current, StringComparer.Ordinal);
                    if (same) keep.UnionWith(savedReferences);
                    Dictionary<string, Entry>? sourceIndex, targetIndex = null;
                    string? sourceFailure = LoadIndex(file, out sourceIndex);
                    // A caller that did not supply the old document's references cannot safely prune its archive.
                    // Conservatively retain its entire index until one document save commits in this session.
                    if (same && (retainExistingOnFirstSave || retainHistory) && sourceIndex != null) keep.UnionWith(sourceIndex.Keys);
                    if (sourceFailure != null && file != null && File.Exists(file) && (retainExistingOnFirstSave || retainHistory || keep.Any(a => !memory.ContainsKey(a))))
                        throw new InvalidDataException("Il progetto non è stato salvato, per non perdere i risultati: " + sourceFailure);
                    if (!same && File.Exists(target))
                    {
                        var failure = LoadIndex(target, out targetIndex);
                        if (failure != null) throw new InvalidDataException("Archivio di destinazione non sostituibile: " + failure);
                        keep.UnionWith(targetIndex!.Keys);
                    }
                    int missing = keep.Count(a => !memory.ContainsKey(a) && sourceIndex?.ContainsKey(a) != true && targetIndex?.ContainsKey(a) != true);
                    if (requireComplete && missing != 0) throw new InvalidDataException("Required archive blocks unavailable: " + missing);
                    // Reading only the checked index is sufficient: unchanged autosave never reads/decodes/recompresses the payload.
                    bool unchanged = same && sourceIndex != null && sourceIndex.Count == keep.Count && keep.All(sourceIndex.ContainsKey);
                    if (!unchanged && keep.Count > missing)
                        Write(target, keep, sourceIndex, targetIndex, same);
                    // Save callback and state transition are serialized with other saves on this session.
                    writeDocument();
                    if (keep.Count == 0 && File.Exists(target))
                    {
                        // Preserve the last retired blocks even when the document becomes empty.
                        if (same && sourceIndex != null) Retire(sourceIndex, Array.Empty<string>());
                        File.Delete(target);
                    }
                    file = target; savedReferences = current; retainExistingOnFirstSave = false;
                    // New data can now be fetched from the committed archive. Unavailable references and retired data stay in memory.
                    foreach (string address in keep) memory.Remove(address);
                    return missing;
                }
                finally { saving = false; }
            }
        }

        private void Write(string target, SortedSet<string> keep, Dictionary<string, Entry>? sourceIndex, Dictionary<string, Entry>? targetIndex, bool same)
        {
            string temporary = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                using (var input = sourceIndex == null ? null : OpenRead(file!))
                using (var previousTarget = targetIndex == null ? null : OpenRead(target))
                {
                    output.Write(new byte[HeaderBytes], 0, HeaderBytes);
                    var entries = new List<KeyValuePair<string, Entry>>();
                    foreach (string address in keep)
                    {
                        ArchiveBlock? block;
                        if (!memory.TryGetValue(address, out block))
                        {
                            if (input != null && sourceIndex!.TryGetValue(address, out var entry)) block = ReadBlock(input, address, entry);
                            else if (previousTarget != null && targetIndex!.TryGetValue(address, out var prior)) block = ReadBlock(previousTarget, address, prior);
                            else continue;
                        }
                        entries.Add(new KeyValuePair<string, Entry>(address, new Entry(block.Kind, output.Position, block.Payload.Length, block.Length)));
                        output.Write(block.Payload, 0, block.Payload.Length);
                    }
                    if (same && sourceIndex != null) Retire(sourceIndex, keep);
                    long indexOffset = output.Position;
                    byte[] table;
                    using (var buffer = new MemoryStream())
                    {
                        using (var writer = new BinaryWriter(buffer, Encoding.UTF8, true))
                            foreach (var pair in entries)
                            {
                                writer.Write(Unhex(pair.Key)); writer.Write(pair.Value.Kind); writer.Write(pair.Value.Offset);
                                writer.Write(pair.Value.Stored); writer.Write(pair.Value.Length);
                            }
                        table = buffer.ToArray();
                    }
                    output.Write(table, 0, table.Length); output.Position = 0;
                    using (var writer = new BinaryWriter(output, Encoding.UTF8, true))
                    { writer.Write(mark); writer.Write(1); writer.Write(entries.Count); writer.Write(indexOffset); writer.Write(Unhex(ArchiveBlock.Digest(table))); }
                    output.Flush(true);
                }
                Replace(temporary, target);
                cache.Remove(target);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }

        private void Retire(Dictionary<string, Entry> index, IEnumerable<string> keep)
        {
            var live = new HashSet<string>(keep, StringComparer.Ordinal);
            using (var input = OpenRead(file!))
                foreach (var pair in index)
                    if (!live.Contains(pair.Key) && !memory.ContainsKey(pair.Key)) memory[pair.Key] = ReadBlock(input, pair.Key, pair.Value);
        }

        private string? LoadIndex(string? path, out Dictionary<string, Entry>? index)
        {
            index = null;
            if (path == null) return "progetto non ancora salvato: blocco non disponibile";
            if (!File.Exists(path)) return "archivio dei risultati mancante: " + Path.GetFileName(path);
            try
            {
                using (var stream = OpenRead(path))
                using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    if (!reader.ReadBytes(16).SequenceEqual(mark) || reader.ReadInt32() != 1) throw new InvalidDataException("formato sconosciuto");
                    int count = reader.ReadInt32(); long indexOffset = reader.ReadInt64(); var digest = reader.ReadBytes(32);
                    long tableLength = (long)count * EntryBytes;
                    if (count < 0 || tableLength > ArchiveBlock.MaximumBytes || indexOffset < HeaderBytes || indexOffset > stream.Length || tableLength != stream.Length - indexOffset)
                        throw new InvalidDataException("indice non valido");
                    string digestKey = BitConverter.ToString(digest);
                    long stamp = File.GetLastWriteTimeUtc(path).Ticks;
                    if (cache.TryGetValue(path, out var cached) && cached.Length == stream.Length && cached.Count == count && cached.Offset == indexOffset && cached.Stamp == stamp && cached.Digest == digestKey)
                    { index = cached.Entries; return null; }
                    var table = ReadAt(stream, indexOffset, tableLength);
                    if (!Unhex(ArchiveBlock.Digest(table)).SequenceEqual(digest)) throw new InvalidDataException("impronta dell'indice diversa");
                    var entries = new Dictionary<string, Entry>(count, StringComparer.Ordinal);
                    using (var rows = new BinaryReader(new MemoryStream(table, false)))
                        for (int i = 0; i < count; i++)
                        {
                            string address = BitConverter.ToString(rows.ReadBytes(32)).Replace("-", "");
                            var entry = new Entry(rows.ReadByte(), rows.ReadInt64(), rows.ReadInt64(), rows.ReadInt64());
                            if (entry.Offset < HeaderBytes || entry.Offset > indexOffset || entry.Stored < 0 || entry.Stored > ArchiveBlock.MaximumBytes || entry.Stored > indexOffset - entry.Offset ||
                                entry.Length < 0 || entry.Length > ArchiveBlock.MaximumBytes || (entry.Kind >= 128 && entry.Length != entry.Stored) || entries.ContainsKey(address))
                                throw new InvalidDataException("voce dell'indice non valida");
                            entries.Add(address, entry);
                        }
                    long end = HeaderBytes;
                    foreach (var entry in entries.Values.OrderBy(e => e.Offset))
                    { if (entry.Offset < end) throw new InvalidDataException("blocchi sovrapposti"); end = entry.Offset + entry.Stored; }
                    index = entries;
                    cache[path] = new IndexCache { Entries = entries, Length = stream.Length, Count = count, Offset = indexOffset, Stamp = stamp, Digest = digestKey };
                }
                return null;
            }
            catch (Exception error) when (IoError(error)) { return "archivio dei risultati non leggibile: " + error.Message; }
        }

        /// <summary>Atomic sibling-file replacement, also available to application document writers.</summary>
        public static void WriteFile(string path, Action<Stream> write)
        {
            if (write == null) throw new ArgumentNullException(nameof(write));
            string target = Path.GetFullPath(path), temporary = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { write(stream); stream.Flush(true); }
                Replace(temporary, target);
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
        private static void Replace(string temporary, string target)
        {
            for (int attempt = 1; ; attempt++)
                try { if (File.Exists(target)) File.Replace(temporary, target, null); else File.Move(temporary, target); break; }
                catch (Exception error) when (attempt < 5 && IoError(error)) { Thread.Sleep(120 * attempt); }
        }
        private static HashSet<string> Addresses(IEnumerable<string> addresses)
        { if (addresses == null) throw new ArgumentNullException(nameof(addresses)); var set = new HashSet<string>(addresses, StringComparer.Ordinal); foreach (string a in set) ArchiveBlock.ValidateAddress(a); return set; }
        private static bool IoError(Exception error) => error is IOException || error is InvalidDataException || error is UnauthorizedAccessException;
        private static FileStream OpenRead(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete);
        private static ArchiveBlock ReadBlock(FileStream input, string address, Entry entry) => ArchiveBlock.FromFile(address, entry.Kind, entry.Length, ReadAt(input, entry.Offset, entry.Stored));
        private static byte[] ReadAt(FileStream stream, long offset, long length)
        { if (length < 0 || length > ArchiveBlock.MaximumBytes) throw new InvalidDataException("Lunghezza del blocco non valida."); var data = new byte[(int)length]; stream.Position = offset; ArchiveBlock.ReadExactly(stream, data); return data; }
        private static byte[] Unhex(string value)
        { var bytes = new byte[value.Length / 2]; for (int i = 0; i < bytes.Length; i++) bytes[i] = Convert.ToByte(value.Substring(2 * i, 2), 16); return bytes; }
    }
}
