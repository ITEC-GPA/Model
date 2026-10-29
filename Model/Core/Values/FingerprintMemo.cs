using System;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace GPC.Model.Core
{
    /// <summary>
    /// Canonical fingerprint bytes of the model values met inside one validation read scope, keyed by reference. Within the scope the
    /// model is not mutated (no checker, callback or user code runs in it), so the walk of the same object writes the same bytes: each
    /// sample, property or dataset is walked once however many fingerprints of the scope contain it, and the fingerprints are the same
    /// as without memo. It lives and dies with its scope (or with the read session of one call that enters the scope again) and its
    /// thread: no lock, no growth across calls or models.
    /// Rule for the callers: an object fingerprinted with the memo is not changed afterwards in the same scope (model values only).
    /// </summary>
    internal sealed class FingerprintMemo
    {
        private sealed class Entry
        {
            // Null: the walk from an empty path failed; the caller walks again, so it fails, or meets a cycle, exactly as without memo.
            internal byte[] Bytes;
            // Every object the walk tested against its path: from a path that holds none of them the bytes are the same.
            internal HashSet<object> Reach;
            internal string Digest;
        }

        private readonly Dictionary<object, Entry> _entries = new Dictionary<object, Entry>(InputFingerprint.ReferenceComparer.Instance);

        /// <summary>The values that recur in the fingerprints of a scope: result samples, element properties and datasets.</summary>
        internal static bool Accepts(object value) => value is Results.Locations.ResultLocation || value is ElementProperties.ElementProperty
            || value is Analysis.AnalysisDataset;

        /// <summary>The bytes the walk of <paramref name="value"/> writes from <paramref name="path"/>, or null when they may differ
        /// (the value reaches an object of the path, a cycle) or the walk failed: the caller then walks it.</summary>
        internal byte[] Bytes(object value, HashSet<object> path)
        {
            var entry = Find(value);
            if (entry.Bytes == null) return null;
            if (path.Count != 0)
                foreach (var ancestor in path)
                    if (entry.Reach.Contains(ancestor)) return null;
            return entry.Bytes;
        }

        /// <summary>The fingerprint of the single value, as <c>InputFingerprint.Compute(new[] { value })</c>.</summary>
        internal string Digest(object value)
        {
            if (!Accepts(value) || FingerprintContracts.UsesLegacyNames) return InputFingerprint.Compute(new[] { value });
            var entry = Find(value);
            if (entry.Bytes == null) return InputFingerprint.Compute(new[] { value });
            if (entry.Digest == null)
                using (var sha = SHA256.Create()) entry.Digest = InputFingerprint.Hex(sha.ComputeHash(entry.Bytes));
            return entry.Digest;
        }

        private Entry Find(object value)
        {
            if (_entries.TryGetValue(value, out var entry)) return entry;
            entry = new Entry();
            var reach = new HashSet<object>(InputFingerprint.ReferenceComparer.Instance);
            try { entry.Bytes = InputFingerprint.Canonical(value, reach); entry.Reach = reach; }
            catch (Exception) { entry.Bytes = null; entry.Reach = null; }
            _entries.Add(value, entry);
            return entry;
        }
    }
}
