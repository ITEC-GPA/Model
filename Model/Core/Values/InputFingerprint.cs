using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using GPC.Geometry;
using GPC.Model.Elements;

namespace GPC.Model.Core
{
    /// <summary>Value canonicalization for revision comparison, independent of XML object-reference numbering and geometric GUIDs.</summary>
    internal static class InputFingerprint
    {
        public static string Compute(IEnumerable<object> inputs) => Compute(inputs, null);
        /// <param name="memo">Canonical bytes of the values already met in the same validation read scope; null outside it.
        /// The result is the same with or without it.</param>
        internal static string Compute(IEnumerable<object> inputs, FingerprintMemo memo)
        {
            // Memoized bytes carry the canonical wire names: under the historical names of an analysis being validated they do not apply.
            if (memo != null && FingerprintContracts.UsesLegacyNames) memo = null;
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory))
            using (var sha = SHA256.Create())
            {
                var path = new HashSet<object>(ReferenceComparer.Instance);
                foreach (var input in inputs)
                {
                    Write(writer, input, path, memo, null);
                    // The bytes go to the hash in blocks between inputs: the same SHA-256 as of the whole buffer, without keeping the
                    // canonical bytes of a whole revision or plan digest (hundreds of KB, twice in the large object heap).
                    writer.Flush();
                    if (memory.Length >= HashBlock) { sha.TransformBlock(memory.GetBuffer(), 0, (int)memory.Length, null, 0); memory.SetLength(0); }
                }
                writer.Flush(); sha.TransformFinalBlock(memory.GetBuffer(), 0, (int)memory.Length);
                return Hex(sha.Hash);
            }
        }
        private const int HashBlock = 16 * 1024;
        internal static string Hex(byte[] hash) => BitConverter.ToString(hash).Replace("-", "");
        /// <summary>The bytes that the walk of <paramref name="value"/> writes from an empty path, without memo. Every object the walk
        /// tests against the path goes into <paramref name="reach"/>: from a path that holds none of them the walk writes the same bytes.</summary>
        internal static byte[] Canonical(object value, HashSet<object> reach)
        {
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory))
            {
                Write(writer, value, new HashSet<object>(ReferenceComparer.Instance), null, reach);
                writer.Flush(); return memory.ToArray();
            }
        }
        private static void Write(BinaryWriter w, object value, HashSet<object> path, FingerprintMemo memo, HashSet<object> reach)
        {
            if (memo != null && FingerprintMemo.Accepts(value))
            {
                var bytes = memo.Bytes(value, path);
                if (bytes != null) { w.Write(bytes); return; }
            }
            if (value == null) { w.Write("null"); return; }
            Type type = value.GetType(); w.Write(FingerprintContracts.WireName(type));
            if (type.IsEnum) { w.Write(Convert.ToInt64(value)); return; }
            if (value is double d) { w.Write(d == 0 ? 0.0 : d); return; }
            if (value is string s) { w.Write(s); return; }
            if (type.IsPrimitive || value is decimal || value is Guid) { w.Write(Convert.ToString(value, CultureInfo.InvariantCulture)); return; }
            if (value is CoordinateSystem axes)
            { Write(w, axes.Origin, path, memo, reach); Write(w, axes.V1, path, memo, reach); Write(w, axes.V2, path, memo, reach); Write(w, axes.V3, path, memo, reach); return; }
            if (value is Point3d p3) { w.Write(p3.X); w.Write(p3.Y); w.Write(p3.Z); return; }
            if (value is Vector3d v3) { w.Write(v3.X); w.Write(v3.Y); w.Write(v3.Z); return; }
            if (value is Point2d p2) { w.Write(p2.X); w.Write(p2.Y); return; }
            if (value is Vector2d v2) { w.Write(v2.X); w.Write(v2.Y); return; }
            // References to entities are identities. Their inputs are visited separately by ModelRevisions.
            if (value is Element element) { w.Write(element.Guid.ToString("D")); w.Write(element.Id); return; }
            reach?.Add(value);
            if (!path.Add(value)) { w.Write("cycle"); if (value is ModelObject modelObject) w.Write(modelObject.Guid.ToString("D")); return; }
            try
            {
                if (value is IDictionary dictionary)
                {
                    var raw = new List<DictionaryEntry>(); var enumerator = dictionary.GetEnumerator();
                    while (enumerator.MoveNext()) raw.Add(enumerator.Entry);
                    var entries = raw.OrderBy(e => Convert.ToString(e.Key, CultureInfo.InvariantCulture), StringComparer.Ordinal).ToArray();
                    w.Write(entries.Length); foreach (var e in entries) { Write(w, e.Key, path, memo, reach); Write(w, e.Value, path, memo, reach); }
                    return;
                }
                if (value is IEnumerable sequence)
                {
                    var items = sequence.Cast<object>().ToArray(); w.Write(items.Length); foreach (var item in items) Write(w, item, path, memo, reach); return;
                }
                if (value is ISerializable serializable)
                {
                    // Section.GetObjectData writes its lazy shape field. Materialize the authoritative shape first,
                    // otherwise merely viewing/validating a section would change its revision.
                    if (value is Sections.ISectionShape section) { var shape = section.Shape; }
                    var info = new SerializationInfo(type, Converter); serializable.GetObjectData(info, new StreamingContext());
                    var entries = new SerializationEntry[info.MemberCount]; var names = new string[entries.Length]; int count = 0;
                    foreach (SerializationEntry e in info) { entries[count] = e; names[count++] = e.Name; }
                    // The names of a SerializationInfo are unique: the order is the one of the former stable ordinal sort.
                    Array.Sort(names, entries, 0, count, StringComparer.Ordinal);
                    for (int i = 0; i < count; i++)
                    {
                        var e = entries[i];
                        if (FingerprintContracts.SkipsEntry(type, e.Name)) continue;
                        w.Write(e.Name); Write(w, e.Value, path, memo, reach);
                    }
                    return;
                }
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                {
                    // The bytes of the historical reflection of its private fields "key" and "value", without reading them.
                    w.Write("key"); Write(w, type.GetProperty("Key").GetValue(value), path, memo, reach);
                    w.Write("value"); Write(w, type.GetProperty("Value").GetValue(value), path, memo, reach);
                    return;
                }
                var members = FingerprintContracts.Members(type);
                if (members != null)
                {
                    foreach (var member in members)
                    {
                        var fieldValue = member.Accessor.GetValue(value);
                        if (fieldValue == null && member.OmitNull) continue;
                        w.Write(member.WireName); Write(w, fieldValue, path, memo, reach);
                    }
                    return;
                }
                // No reflection of private fields (architecture review R8): a record without an explicit contract is refused.
                throw new InvalidOperationException("UnregisteredFingerprintType: " + type.FullName + ". Declare its fingerprint contract: "
                    + (type.Assembly == typeof(InputFingerprint).Assembly ? "FingerprintContracts.RegisterBuiltIns." : "FingerprintVocabularyAttribute in its assembly."));
            }
            finally { path.Remove(value); }
        }
        // Only the readers of a SerializationInfo use the converter, never GetObjectData: one stateless instance serves every walk.
        private static readonly FormatterConverter Converter = new FormatterConverter();
        internal sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
