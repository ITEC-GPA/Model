using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
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

    /// <summary>What the last save of a <see cref="BlockArchive"/> did.</summary>
    public sealed class ArchiveSaveReport
    {
        /// <summary>Cited blocks the archive could not keep: absent, damaged, or an invalid reference. Each one is a single unavailable
        /// block of the document; the save itself went on.</summary>
        public int Unavailable { get; }
        /// <summary>True when the archive file was rewritten (false: unchanged, or nothing to write).</summary>
        public bool Written { get; }
        /// <summary>An archive that could not be read as an archive (damaged or of an unknown format), renamed to this path instead of
        /// being replaced or blocking the save; null normally.</summary>
        public string? SetAside { get; }
        /// <summary>Blocks whose bytes no longer matched their address when copied forward: left out of the new file.</summary>
        public IReadOnlyList<string> Damaged { get; }
        internal ArchiveSaveReport(int unavailable, bool written, string? setAside, IReadOnlyList<string> damaged)
        { Unavailable = unavailable; Written = written; SetAside = setAside; Damaged = damaged; }
    }

    /// <summary>Domain-independent content-addressed archive, extracted from ANTHEA ModelResultArchive format 1, and the block store of
    /// ANTHEA, BridGHe and Arachne. Caller owns document schemas, codecs, revisions and retention policy. One instance per open document.
    /// <para>Save serializes all session operations, writes the archive first and invokes an atomic document writer second. Previous
    /// document blocks survive an interrupted save; retired blocks remain available for undo in this session.</para>
    /// <para>Failures are per block: an absent, damaged or oversized block, an invalid index entry or an invalid reference makes only that
    /// block unavailable. A missing archive, or one that cannot be read as an archive (it is renamed aside, never overwritten), does not
    /// block the save of the document. The index of the file is kept live: a save reads the 64-byte header, copies the kept blocks byte for
    /// byte (each one verified against its address once per session) and never decodes them. A lock file serializes the writers of one
    /// archive across processes, and a file rewritten by another writer since this session last saw it is retained whole.</para></summary>
    public sealed class BlockArchive
    {
        public const string DefaultMark = "GPC.BLOCKS";
        private const int HeaderBytes = 64, EntryBytes = 57, FormatVersion = 1, CopyBuffer = 1 << 20;
        private static readonly TimeSpan OrphanAge = TimeSpan.FromMinutes(10);
        private readonly object gate = new object();
        private readonly byte[] mark;
        private readonly Dictionary<string, ArchiveBlock> memory = new Dictionary<string, ArchiveBlock>(StringComparer.Ordinal);
        private string? file;
        private HashSet<string> savedReferences;
        private bool saving;
        private bool retainExistingOnFirstSave;
        // The live index of `file`, the digest of the index this session last read at opening or wrote there (another one means another
        // writer), and the blocks of `file` whose bytes matched their address in this session.
        private Index? index;
        private byte[]? known;
        private readonly HashSet<string> verified = new HashSet<string>(StringComparer.Ordinal);
        private ArchiveSaveReport? lastSave;

        private enum FailureKind { Missing, Unreadable, Invalid }
        private sealed class Failure
        {
            internal readonly FailureKind Kind; internal readonly string Message;
            internal Failure(FailureKind kind, string message) { Kind = kind; Message = message; }
        }
        private sealed class Index
        {
            internal string Path = "";
            internal long Length;
            internal byte[] Digest = Array.Empty<byte>();
            internal Dictionary<string, Entry> Entries = null!;
            // Entries whose position or lengths are impossible: those blocks are unavailable, the others are not affected.
            internal Dictionary<string, string> Damaged = null!;
        }
        private sealed class Entry
        {
            internal readonly byte Kind;
            internal readonly long Offset, Stored, Length;
            internal Entry(byte kind, long offset, long stored, long length) { Kind = kind; Offset = offset; Stored = stored; Length = length; }
            internal bool Oversized => Stored > ArchiveBlock.MaximumBytes || Length > ArchiveBlock.MaximumBytes;
        }

        public BlockArchive(string? archivePath = null, IEnumerable<string>? savedReferences = null, string mark = DefaultMark)
        {
            if (string.IsNullOrEmpty(mark) || mark.Length > 16 || mark.Any(c => c < 32 || c > 126)) throw new ArgumentException("Invalid archive mark.", nameof(mark));
            this.mark = new byte[16]; Encoding.ASCII.GetBytes(mark).CopyTo(this.mark, 0);
            file = archivePath == null ? null : Path.GetFullPath(archivePath);
            retainExistingOnFirstSave = savedReferences == null;
            // A reference that is not an address can never be in the archive: it is unavailable, it does not stop the document.
            this.savedReferences = new HashSet<string>((savedReferences ?? Array.Empty<string>()).Where(ArchiveBlock.IsAddress), StringComparer.Ordinal);
            known = file == null ? null : HeaderDigest(file);
        }

        public string? FilePath { get { lock (gate) return file; } }

        /// <summary>The outcome of the last completed save of this session, null before the first one.</summary>
        public ArchiveSaveReport? LastSave { get { lock (gate) return lastSave; } }

        /// <summary>How long a save waits for another writer of the same archive (another process) before failing.</summary>
        public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(10);

        public void Add(ArchiveBlock block)
        {
            if (block == null) throw new ArgumentNullException(nameof(block));
            // Kept even when the file has it: another writer may replace the file before the next save.
            lock (gate) if (!memory.ContainsKey(block.Address)) memory.Add(block.Address, block);
        }

        /// <summary>The decoded blocks, each checked against its address; a block that cannot be given back has its reason.</summary>
        public BlockContent[] Read(IReadOnlyList<string> addresses)
        {
            if (addresses == null) throw new ArgumentNullException(nameof(addresses));
            var contents = new BlockContent[addresses.Count];
            var blocks = new ArchiveBlock?[addresses.Count];
            var payloads = new byte[]?[addresses.Count];
            var entries = new Entry?[addresses.Count];
            Index? source = null;
            lock (gate)
            {
                Failure? failure = null; bool loaded = false;
                var disk = new List<int>();
                for (int i = 0; i < addresses.Count; i++)
                {
                    string address = addresses[i];
                    if (!ArchiveBlock.IsAddress(address)) { contents[i] = new BlockContent(null, "riferimento al blocco non valido"); continue; }
                    if (memory.TryGetValue(address, out var block)) { blocks[i] = block; continue; }
                    if (!loaded) { failure = LoadLive(out source); loaded = true; }
                    if (failure != null) contents[i] = new BlockContent(null, failure.Message);
                    else if (source!.Entries.TryGetValue(address, out var entry))
                    {
                        if (entry.Oversized) contents[i] = new BlockContent(null, "blocco oltre il limite di " + ArchiveBlock.MaximumBytes / (1024 * 1024) + " MiB");
                        else { entries[i] = entry; disk.Add(i); }
                    }
                    else if (source.Damaged.TryGetValue(address, out var reason)) contents[i] = new BlockContent(null, "blocco danneggiato (" + reason + ")");
                    else contents[i] = new BlockContent(null, "blocco assente dall'archivio dei risultati " + Path.GetFileName(file));
                }
                if (disk.Count > 0)
                    try
                    {
                        using (var input = OpenRead(source!.Path))
                            foreach (int i in disk.OrderBy(i => entries[i]!.Offset)) payloads[i] = ReadAt(input, entries[i]!.Offset, entries[i]!.Stored);
                    }
                    catch (Exception error) when (IoError(error))
                    { foreach (int i in disk) { payloads[i] = null; contents[i] = new BlockContent(null, "archivio dei risultati non leggibile: " + error.Message); } }
            }
            var checkedOnDisk = new ConcurrentBag<string>();
            Parallel.For(0, addresses.Count, i =>
            {
                try
                {
                    if (blocks[i] != null) contents[i] = new BlockContent(blocks[i]!.Decode(), null, blocks[i]!.Kind);
                    else if (payloads[i] != null)
                    {
                        // The buffer read from the file is this call's own: decoded without a second copy.
                        contents[i] = new BlockContent(ArchiveBlock.DecodeOwned(addresses[i], entries[i]!.Kind, entries[i]!.Length, payloads[i]!), null, entries[i]!.Kind);
                        checkedOnDisk.Add(addresses[i]);
                    }
                }
                catch (Exception error) when (IoError(error)) { contents[i] = new BlockContent(null, "blocco danneggiato (" + error.Message + ")"); }
            });
            if (!checkedOnDisk.IsEmpty)
                lock (gate) if (ReferenceEquals(index, source)) verified.UnionWith(checkedOnDisk);
            return contents;
        }

        /// <summary>Saves the given live references, plus blocks needed by the previously saved document. Returns unavailable count.
        /// A different existing destination is preserved in full until the next save (Save As may overwrite another document).
        /// A failed callback leaves this session pointing at its original path. The callback must replace its document atomically.</summary>
        public int Save(string archivePath, IEnumerable<string> references, Action writeDocument, bool requireComplete = false)
            => SaveCore(archivePath, references, writeDocument, requireComplete, false, false);

        /// <summary>As <see cref="Save(string, IEnumerable{string}, Action, bool)"/>; <paramref name="retainExisting"/> also keeps every
        /// block already in the destination, for a caller that cannot list all the blocks of its saved document this time (for example an
        /// unreadable manifest): nothing it cannot see is pruned.</summary>
        public int Save(string archivePath, IEnumerable<string> references, Action writeDocument, bool requireComplete, bool retainExisting)
            => SaveCore(archivePath, references, writeDocument, requireComplete, false, retainExisting);

        /// <summary>Writes blocks before a host-controlled document save whose completion cannot be observed.
        /// Retains the entire destination history on every call. No document commit or pruning is inferred.
        /// A later explicit Save with an atomic document callback may resume the normal retention policy.</summary>
        public int SaveSnapshot(string archivePath, IEnumerable<string> references, bool requireComplete = false)
            => SaveCore(archivePath, references, () => { }, requireComplete, true, true);

        private int SaveCore(string archivePath, IEnumerable<string> references, Action writeDocument, bool requireComplete, bool snapshot, bool retainExisting)
        {
            if (writeDocument == null) throw new ArgumentNullException(nameof(writeDocument));
            if (references == null) throw new ArgumentNullException(nameof(references));
            var current = new HashSet<string>(StringComparer.Ordinal); int invalid = 0;
            foreach (string reference in references) { if (ArchiveBlock.IsAddress(reference)) current.Add(reference); else invalid++; }
            string target = Path.GetFullPath(archivePath);
            lock (gate)
            {
                if (saving) throw new InvalidOperationException("Recursive archive save.");
                saving = true;
                try
                {
                    using (AcquireLock(target))
                        return SaveLocked(target, current, invalid, writeDocument, requireComplete, snapshot, retainExisting);
                }
                finally { saving = false; }
            }
        }

        private int SaveLocked(string target, HashSet<string> current, int invalid, Action writeDocument, bool requireComplete, bool snapshot, bool retainExisting)
        {
            RemoveOrphans(target);
            bool same = string.Equals(file, target, StringComparison.OrdinalIgnoreCase);
            var keep = new HashSet<string>(current, StringComparer.Ordinal);
            if (same) keep.UnionWith(savedReferences);
            string? setAside = null;
            Index? source = null, destination = null;
            if (file != null)
            {
                var failure = LoadLive(out source);
                if (failure != null)
                {
                    source = null;
                    // An archive that exists but cannot be read now (in use, no permission) is never replaced or abandoned while it holds
                    // blocks the documents need: they would be lost. A missing one has nothing to lose.
                    if (failure.Kind == FailureKind.Unreadable && keep.Any(a => !memory.ContainsKey(a)))
                        throw new IOException("Il progetto non è stato salvato, per non perdere i risultati: " + failure.Message + ". Se il file è aperto in un altro " +
                            "programma, chiudilo e salva di nuovo.");
                    // Not an archive (damaged, or of an unknown format): kept aside under another name, the save goes on without it.
                    if (failure.Kind == FailureKind.Invalid && same) setAside = SetAside(target);
                }
                else if (same && (retainExistingOnFirstSave || snapshot || retainExisting || known != null && !source!.Digest.SequenceEqual(known)))
                {
                    // A caller that did not supply the old document's references, a host snapshot, or a file rewritten by another writer since
                    // this session saw it: every block in it is retained, since the document on disk may cite any of them.
                    keep.UnionWith(source!.Entries.Keys);
                    if (known != null && !source.Digest.SequenceEqual(known)) verified.Clear();
                }
            }
            if (!same && File.Exists(target))
            {
                var failure = Load(target, null, out destination);
                if (failure != null)
                {
                    destination = null;
                    if (failure.Kind == FailureKind.Unreadable) throw new IOException("Archivio di destinazione non sostituibile: " + failure.Message);
                    if (failure.Kind == FailureKind.Invalid) setAside = SetAside(target);
                }
                else keep.UnionWith(destination!.Entries.Keys);
            }
            var available = new List<string>(keep.Count);
            foreach (string address in keep)
                if (memory.ContainsKey(address) || source?.Entries.ContainsKey(address) == true || destination?.Entries.ContainsKey(address) == true) available.Add(address);
            int missing = keep.Count - available.Count + invalid;
            if (requireComplete && missing != 0) throw new InvalidDataException("Required archive blocks unavailable: " + missing);
            // The file at the target already holds exactly these blocks: nothing is read, decoded or rewritten (unchanged autosave).
            var there = same ? source : destination;
            bool unchanged = setAside == null && there != null && there.Entries.Count == available.Count && available.All(there.Entries.ContainsKey);
            Index? written = null; var damaged = new List<string>();
            if (!unchanged && available.Count > 0) { available.Sort(StringComparer.Ordinal); written = Write(target, available, source, destination, same, damaged); }
            try { writeDocument(); }
            catch
            {
                // The new archive is at the session's own path even if the document is not: keep its index live, never treat it as foreign.
                if (same && written != null) Commit(written, written: true);
                else if (same && setAside != null) { index = null; known = null; verified.Clear(); }
                throw;
            }
            // The document is written: from here nothing may throw. An unused archive left beside it is harmless.
            bool deleted = false;
            try
            {
                if (keep.Count == 0 && File.Exists(target))
                {
                    // Preserve the last retired blocks even when the document becomes empty.
                    if (same && source != null) Retire(source, new HashSet<string>(StringComparer.Ordinal));
                    File.Delete(target); deleted = true;
                }
            }
            catch (Exception error) when (IoError(error)) { }
            if (!same) { verified.Clear(); index = null; known = null; }
            file = target;
            if (written != null) Commit(written, written: true);
            else if (deleted || setAside != null) { index = null; known = null; verified.Clear(); }
            else if (!same && destination != null) Commit(destination, written: false);
            // Otherwise the session's file is as it was: its live index and known digest stay.
            savedReferences = current; retainExistingOnFirstSave = false;
            lastSave = new ArchiveSaveReport(missing + damaged.Count, written != null, setAside, damaged.AsReadOnly());
            return missing + damaged.Count;
        }

        /// <summary>The index of the session's file becomes <paramref name="now"/>; its blocks no longer need to stay in memory.</summary>
        private void Commit(Index now, bool written)
        {
            index = now; known = now.Digest;
            if (written) { verified.Clear(); verified.UnionWith(now.Entries.Keys); }
            foreach (string address in now.Entries.Keys) memory.Remove(address);
        }

        /// <summary>Writes <paramref name="blocks"/> (sorted, as ANTHEA format 1) into a temporary file beside the target, then replaces it.
        /// Blocks from memory are written as they are; blocks from a file are copied byte for byte, after checking each one against its
        /// address unless this session already did. A block that fails the check is left out and listed in <paramref name="damaged"/>.</summary>
        private Index Write(string target, List<string> blocks, Index? source, Index? destination, bool same, List<string> damaged)
        {
            var origin = new Dictionary<string, KeyValuePair<Index, Entry>>(StringComparer.Ordinal);
            var toVerify = new List<KeyValuePair<string, KeyValuePair<Index, Entry>>>();
            foreach (string address in blocks)
            {
                if (memory.ContainsKey(address)) continue;
                var from = source != null && source.Entries.TryGetValue(address, out var entry) ? new KeyValuePair<Index, Entry>(source, entry)
                    : new KeyValuePair<Index, Entry>(destination!, destination!.Entries[address]);
                origin.Add(address, from);
                if (!(ReferenceEquals(from.Key, index) && verified.Contains(address))) toVerify.Add(new KeyValuePair<string, KeyValuePair<Index, Entry>>(address, from));
            }
            var bad = VerifyAll(toVerify);
            damaged.AddRange(bad.Keys.OrderBy(a => a, StringComparer.Ordinal));
            string temporary = target + "." + Guid.NewGuid().ToString("N") + ".tmp";
            var entries = new Dictionary<string, Entry>(blocks.Count, StringComparer.Ordinal);
            byte[] digest; long length;
            var retired = new Dictionary<string, ArchiveBlock>(StringComparer.Ordinal);
            try
            {
                var inputs = new Dictionary<Index, FileStream>();
                try
                {
                    using (var output = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1 << 16))
                    {
                        output.Write(new byte[HeaderBytes], 0, HeaderBytes);
                        var buffer = new byte[CopyBuffer];
                        foreach (string address in blocks)
                        {
                            if (bad.ContainsKey(address)) continue;
                            if (memory.TryGetValue(address, out var block))
                            {
                                entries.Add(address, new Entry(block.Kind, output.Position, block.Payload.Length, block.Length));
                                output.Write(block.Payload, 0, block.Payload.Length);
                                continue;
                            }
                            var from = origin[address];
                            if (!inputs.TryGetValue(from.Key, out var input)) inputs.Add(from.Key, input = OpenRead(from.Key.Path));
                            entries.Add(address, new Entry(from.Value.Kind, output.Position, from.Value.Stored, from.Value.Length));
                            Copy(input, from.Value.Offset, from.Value.Stored, output, buffer);
                        }
                        // The blocks the new file drops stay in memory for the rest of the session: an undo may cite them again.
                        if (same && source != null)
                        {
                            if (!inputs.TryGetValue(source, out var input)) inputs.Add(source, input = OpenRead(source.Path));
                            foreach (var pair in source.Entries)
                                if (!entries.ContainsKey(pair.Key) && !memory.ContainsKey(pair.Key) && !pair.Value.Oversized)
                                    retired[pair.Key] = ArchiveBlock.FromFile(pair.Key, pair.Value.Kind, pair.Value.Length, ReadAt(input, pair.Value.Offset, pair.Value.Stored));
                        }
                        long indexOffset = output.Position;
                        var table = new byte[entries.Count * EntryBytes]; int at = 0;
                        foreach (var pair in entries)
                        {
                            Unhex(pair.Key, table, at); table[at + 32] = pair.Value.Kind;
                            Int64(table, at + 33, pair.Value.Offset); Int64(table, at + 41, pair.Value.Stored); Int64(table, at + 49, pair.Value.Length);
                            at += EntryBytes;
                        }
                        output.Write(table, 0, table.Length);
                        using (var sha = SHA256.Create()) digest = sha.ComputeHash(table);
                        var header = new byte[HeaderBytes];
                        Buffer.BlockCopy(mark, 0, header, 0, 16); Int32(header, 16, FormatVersion); Int32(header, 20, entries.Count); Int64(header, 24, indexOffset);
                        Buffer.BlockCopy(digest, 0, header, 32, 32);
                        length = output.Length;
                        output.Position = 0; output.Write(header, 0, HeaderBytes);
                        output.Flush(true);
                    }
                }
                finally { foreach (var input in inputs.Values) input.Dispose(); }
                Replace(temporary, target);
            }
            finally { TryDelete(temporary); }
            foreach (var pair in retired) memory[pair.Key] = pair.Value;
            return new Index { Path = target, Length = length, Digest = digest, Entries = entries, Damaged = new Dictionary<string, string>(StringComparer.Ordinal) };
        }

        private void Retire(Index source, HashSet<string> keep)
        {
            using (var input = OpenRead(source.Path))
                foreach (var pair in source.Entries)
                    if (!keep.Contains(pair.Key) && !memory.ContainsKey(pair.Key) && !pair.Value.Oversized)
                        memory[pair.Key] = ArchiveBlock.FromFile(pair.Key, pair.Value.Kind, pair.Value.Length, ReadAt(input, pair.Value.Offset, pair.Value.Stored));
        }

        /// <summary>The index of the session's file, from the header alone when the file did not change. Under the gate.</summary>
        private Failure? LoadLive(out Index? result)
        {
            if (file == null) { result = null; return new Failure(FailureKind.Missing, "progetto non ancora salvato: blocco non disponibile"); }
            var failure = Load(file, index, out result);
            if (failure == null) { if (!ReferenceEquals(result, index)) verified.Clear(); index = result; }
            return failure;
        }

        private Failure? Load(string path, Index? cached, out Index? result)
        {
            result = null;
            string name = Path.GetFileName(path);
            if (!File.Exists(path)) return new Failure(FailureKind.Missing, "archivio dei risultati mancante: " + name);
            try
            {
                using (var stream = OpenRead(path))
                {
                    var header = new byte[HeaderBytes];
                    if (stream.Length < HeaderBytes) return new Failure(FailureKind.Invalid, "archivio dei risultati danneggiato (indice): " + name);
                    ArchiveBlock.ReadExactly(stream, header);
                    for (int i = 0; i < 16; i++) if (header[i] != mark[i]) return new Failure(FailureKind.Invalid, "archivio dei risultati di formato sconosciuto: " + name);
                    if (ReadInt32(header, 16) != FormatVersion) return new Failure(FailureKind.Invalid, "archivio dei risultati di formato sconosciuto: " + name);
                    int count = ReadInt32(header, 20); long indexOffset = ReadInt64(header, 24);
                    var digest = new byte[32]; Buffer.BlockCopy(header, 32, digest, 0, 32);
                    long length = stream.Length;
                    if (count < 0 || indexOffset < HeaderBytes || indexOffset > length || (long)count * EntryBytes != length - indexOffset || (long)count * EntryBytes > int.MaxValue)
                        return new Failure(FailureKind.Invalid, "archivio dei risultati danneggiato (indice): " + name);
                    if (cached != null && string.Equals(cached.Path, path, StringComparison.OrdinalIgnoreCase) && cached.Length == length && cached.Digest.SequenceEqual(digest))
                    { result = cached; return null; }
                    var table = new byte[count * EntryBytes];
                    stream.Position = indexOffset; ArchiveBlock.ReadExactly(stream, table);
                    using (var sha = SHA256.Create())
                        if (!sha.ComputeHash(table).SequenceEqual(digest)) return new Failure(FailureKind.Invalid, "archivio dei risultati danneggiato (indice): " + name);
                    var entries = new Dictionary<string, Entry>(count, StringComparer.Ordinal);
                    var damaged = new Dictionary<string, string>(StringComparer.Ordinal);
                    var hash = new byte[32];
                    for (int i = 0, at = 0; i < count; i++, at += EntryBytes)
                    {
                        Buffer.BlockCopy(table, at, hash, 0, 32);
                        string address = ArchiveBlock.Hex(hash);
                        var entry = new Entry(table[at + 32], ReadInt64(table, at + 33), ReadInt64(table, at + 41), ReadInt64(table, at + 49));
                        bool valid = entry.Offset >= HeaderBytes && entry.Offset <= indexOffset && entry.Stored >= 0 && entry.Stored <= indexOffset - entry.Offset &&
                            entry.Length >= 0 && ((entry.Kind & 128) == 0 || entry.Length == entry.Stored);
                        // A duplicate address keeps its first valid entry: each read is checked against the address anyway.
                        if (valid) { if (!entries.ContainsKey(address)) { entries.Add(address, entry); damaged.Remove(address); } }
                        else if (!entries.ContainsKey(address)) damaged[address] = "voce dell'indice non valida";
                    }
                    result = new Index { Path = path, Length = length, Digest = digest, Entries = entries, Damaged = damaged };
                    return null;
                }
            }
            catch (EndOfStreamException) { return new Failure(FailureKind.Invalid, "archivio dei risultati danneggiato (indice): " + name); }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            { return new Failure(FailureKind.Unreadable, "archivio dei risultati non leggibile: " + error.Message); }
        }

        private byte[]? HeaderDigest(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                using (var stream = OpenRead(path))
                {
                    if (stream.Length < HeaderBytes) return null;
                    var header = new byte[HeaderBytes]; ArchiveBlock.ReadExactly(stream, header);
                    var digest = new byte[32]; Buffer.BlockCopy(header, 32, digest, 0, 32); return digest;
                }
            }
            catch (Exception error) when (IoError(error)) { return null; }
        }

        /// <summary>Checks blocks in files against their addresses, in parallel ranges in file order; each range has one stream, one hash
        /// and one buffer. Returns the blocks that do not match.</summary>
        private static ConcurrentDictionary<string, bool> VerifyAll(List<KeyValuePair<string, KeyValuePair<Index, Entry>>> items)
        {
            var bad = new ConcurrentDictionary<string, bool>(StringComparer.Ordinal);
            foreach (var group in items.GroupBy(i => i.Value.Key.Path, StringComparer.OrdinalIgnoreCase))
            {
                var list = group.OrderBy(i => i.Value.Value.Offset).ToArray();
                var ranges = System.Collections.Concurrent.Partitioner.Create(0, list.Length, Math.Max(16, list.Length / (4 * Environment.ProcessorCount) + 1));
                Parallel.ForEach(ranges, range =>
                {
                    FileStream? input = null;
                    try
                    {
                        input = new FileStream(group.Key, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1 << 16, FileOptions.SequentialScan);
                        using (var sha = SHA256.Create())
                        {
                            var buffer = new byte[1 << 16];
                            for (int k = range.Item1; k < range.Item2; k++)
                                if (!Verify(input, sha, buffer, list[k].Key, list[k].Value.Value)) bad[list[k].Key] = true;
                        }
                    }
                    catch (Exception error) when (IoError(error)) { for (int k = range.Item1; k < range.Item2; k++) bad[list[k].Key] = true; }
                    finally { input?.Dispose(); }
                });
            }
            return bad;
        }

        /// <summary>Checks one block by streaming it (no buffer of its size, any length).</summary>
        private static bool Verify(FileStream input, SHA256 sha, byte[] buffer, string address, Entry entry)
        {
            try
            {
                input.Position = entry.Offset;
                var segment = new Segment(input, entry.Stored);
                Stream data = (entry.Kind & 128) != 0 ? (Stream)segment : new DeflateStream(segment, CompressionMode.Decompress);
                sha.Initialize();
                long total = 0; int n;
                using (data)
                    while ((n = data.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        total += n; if (total > entry.Length) return false;
                        sha.TransformBlock(buffer, 0, n, null, 0);
                    }
                if (total != entry.Length) return false;
                sha.TransformFinalBlock(buffer, 0, 0);
                return ArchiveBlock.Hex(sha.Hash) == address;
            }
            catch (Exception error) when (IoError(error)) { return false; }
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
            finally { TryDelete(temporary); }
        }

        /// <summary>Moves the new file over the target in one step. On Windows MoveFileEx with replace and write-through, as File.Move with
        /// overwrite on .NET: unlike File.Replace it works on network shares that cannot keep the replaced file's identity, and a read-only
        /// target fails cleanly (access denied) with the target untouched.</summary>
        private static void Replace(string temporary, string target)
        {
            for (int attempt = 1; ; attempt++)
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        if (!MoveFileEx(temporary, target, MoveReplaceExisting | MoveWriteThrough)) throw MoveError(Marshal.GetLastWin32Error(), target);
                    }
                    else if (File.Exists(target)) File.Replace(temporary, target, null);
                    else File.Move(temporary, target);
                    break;
                }
                catch (Exception error) when (attempt < 5 && (error is IOException || error is UnauthorizedAccessException)) { Thread.Sleep(120 * attempt); }
        }

        private const int MoveReplaceExisting = 1, MoveWriteThrough = 8;
        [DllImport("kernel32.dll", EntryPoint = "MoveFileExW", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool MoveFileEx(string existingFileName, string newFileName, int flags);
        private static Exception MoveError(int code, string target)
        {
            string message = new Win32Exception(code).Message + ": " + target;
            return code == 5 ? (Exception)new UnauthorizedAccessException(message) : new IOException(message, unchecked((int)0x80070000) | code);
        }

        /// <summary>Serializes the writers of one archive across processes: an exclusive lock file beside it, removed when closed. A folder
        /// where it cannot be created (read-only) gives no lock: a save that must write fails there anyway.</summary>
        private IDisposable? AcquireLock(string target)
        {
            string path = target + ".lock";
            var watch = System.Diagnostics.Stopwatch.StartNew();
            for (int attempt = 1; ; attempt++)
                try { return new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose); }
                // Access is also denied for a moment while another writer's lock file is being removed.
                catch (UnauthorizedAccessException) when (watch.Elapsed < TimeSpan.FromSeconds(1)) { Thread.Sleep(20); }
                catch (Exception error) when (error is UnauthorizedAccessException || error is DirectoryNotFoundException) { return null; }
                catch (IOException) when (watch.Elapsed < LockTimeout) { Thread.Sleep(Math.Min(50 * attempt, 500)); }
                catch (IOException error) { throw new IOException("Archivio in uso da un altro processo, salvataggio non eseguito: " + Path.GetFileName(target) + " (" + error.Message + ")", error); }
        }

        /// <summary>Temporary files of saves that never finished (a process ended between writing and replacing): removed under the lock,
        /// only when nobody holds them (a writer keeps its own open exclusively) and older than <see cref="OrphanAge"/>.</summary>
        private static void RemoveOrphans(string target)
        {
            try
            {
                string? folder = Path.GetDirectoryName(target); if (folder == null || !Directory.Exists(folder)) return;
                string prefix = Path.GetFileName(target) + ".";
                foreach (string candidate in Directory.EnumerateFiles(folder, prefix + "*.tmp"))
                {
                    string middle = Path.GetFileName(candidate).Substring(prefix.Length);
                    if (middle.Length != 36 || !middle.Take(32).All(Uri.IsHexDigit)) continue;
                    if (DateTime.UtcNow - File.GetLastWriteTimeUtc(candidate) > OrphanAge) TryDelete(candidate);
                }
            }
            catch (Exception error) when (IoError(error)) { }
        }

        /// <summary>Renames an archive that is not readable as one, so that it is neither overwritten nor an obstacle to the save.</summary>
        private static string SetAside(string path)
        {
            string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture), aside = path + ".danneggiato-" + stamp;
            for (int n = 2; File.Exists(aside); n++) aside = path + ".danneggiato-" + stamp + "-" + n;
            File.Move(path, aside);
            return aside;
        }

        private static void TryDelete(string path)
        { try { if (File.Exists(path)) File.Delete(path); } catch (Exception error) when (IoError(error)) { } }
        private static bool IoError(Exception error) => error is IOException || error is InvalidDataException || error is UnauthorizedAccessException;
        private static FileStream OpenRead(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        private static void Copy(FileStream input, long offset, long count, Stream output, byte[] buffer)
        {
            input.Position = offset;
            while (count > 0)
            {
                int n = input.Read(buffer, 0, (int)Math.Min(buffer.Length, count));
                if (n == 0) throw new EndOfStreamException();
                output.Write(buffer, 0, n); count -= n;
            }
        }
        private static byte[] ReadAt(FileStream stream, long offset, long length)
        { if (length < 0 || length > ArchiveBlock.MaximumBytes) throw new InvalidDataException("Lunghezza del blocco non valida."); var data = new byte[(int)length]; stream.Position = offset; ArchiveBlock.ReadExactly(stream, data); return data; }
        private static void Unhex(string value, byte[] target, int at)
        {
            for (int i = 0; i < 32; i++) target[at + i] = (byte)(Nibble(value[2 * i]) << 4 | Nibble(value[2 * i + 1]));
        }
        private static int Nibble(char c) => c <= '9' ? c - '0' : c - 'A' + 10;
        private static void Int32(byte[] target, int at, int value) { for (int i = 0; i < 4; i++) target[at + i] = (byte)(value >> (8 * i)); }
        private static void Int64(byte[] target, int at, long value) { for (int i = 0; i < 8; i++) target[at + i] = (byte)(value >> (8 * i)); }
        private static int ReadInt32(byte[] source, int at) { int value = 0; for (int i = 3; i >= 0; i--) value = value << 8 | source[at + i]; return value; }
        private static long ReadInt64(byte[] source, int at) { long value = 0; for (int i = 7; i >= 0; i--) value = value << 8 | source[at + i]; return value; }

        /// <summary>A read-only window of a stream: the stored bytes of one block.</summary>
        private sealed class Segment : Stream
        {
            private readonly Stream inner; private long remaining;
            internal Segment(Stream inner, long length) { this.inner = inner; remaining = length; }
            public override int Read(byte[] buffer, int offset, int count)
            {
                if (remaining <= 0) return 0;
                int n = inner.Read(buffer, offset, (int)Math.Min(count, remaining));
                if (n == 0) throw new EndOfStreamException();
                remaining -= n; return n;
            }
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
