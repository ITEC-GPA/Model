using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using GPC.Geometry;
using GPC.Model.Elements;

namespace GPC.Model.Core
{
    /// <summary>A field added after fingerprints were archived: it enters the fingerprint only when not null,
    /// so requests and selections that do not use it keep their historical identity.</summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal sealed class FingerprintWhenSetAttribute : Attribute { }

    /// <summary>Value canonicalization for revision comparison, independent of XML object-reference numbering and geometric GUIDs.</summary>
    internal static class InputFingerprint
    {
        public static string Compute(IEnumerable<object> inputs)
        {
            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory))
            using (var sha = SHA256.Create())
            {
                var path = new HashSet<object>(ReferenceComparer.Instance);
                foreach (var input in inputs) Write(writer, input, path);
                writer.Flush(); return BitConverter.ToString(sha.ComputeHash(memory.ToArray())).Replace("-", "");
            }
        }
        private static void Write(BinaryWriter w, object value, HashSet<object> path)
        {
            if (value == null) { w.Write("null"); return; }
            Type type = value.GetType(); w.Write(FingerprintContracts.WireName(type));
            if (type.IsEnum) { w.Write(Convert.ToInt64(value)); return; }
            if (value is double d) { w.Write(d == 0 ? 0.0 : d); return; }
            if (value is string s) { w.Write(s); return; }
            if (type.IsPrimitive || value is decimal || value is Guid) { w.Write(Convert.ToString(value, CultureInfo.InvariantCulture)); return; }
            if (value is CoordinateSystem axes)
            { Write(w, axes.Origin, path); Write(w, axes.V1, path); Write(w, axes.V2, path); Write(w, axes.V3, path); return; }
            if (value is Point3d p3) { w.Write(p3.X); w.Write(p3.Y); w.Write(p3.Z); return; }
            if (value is Vector3d v3) { w.Write(v3.X); w.Write(v3.Y); w.Write(v3.Z); return; }
            if (value is Point2d p2) { w.Write(p2.X); w.Write(p2.Y); return; }
            if (value is Vector2d v2) { w.Write(v2.X); w.Write(v2.Y); return; }
            // References to entities are identities. Their inputs are visited separately by ModelRevisions.
            if (value is Element element) { w.Write(element.Guid.ToString("D")); w.Write(element.Id); return; }
            if (!path.Add(value)) { w.Write("cycle"); if (value is ModelObject modelObject) w.Write(modelObject.Guid.ToString("D")); return; }
            try
            {
                if (value is IDictionary dictionary)
                {
                    var raw = new List<DictionaryEntry>(); var enumerator = dictionary.GetEnumerator();
                    while (enumerator.MoveNext()) raw.Add(enumerator.Entry);
                    var entries = raw.OrderBy(e => Convert.ToString(e.Key, CultureInfo.InvariantCulture), StringComparer.Ordinal).ToArray();
                    w.Write(entries.Length); foreach (var e in entries) { Write(w, e.Key, path); Write(w, e.Value, path); }
                    return;
                }
                if (value is IEnumerable sequence)
                {
                    var items = sequence.Cast<object>().ToArray(); w.Write(items.Length); foreach (var item in items) Write(w, item, path); return;
                }
                if (value is ISerializable serializable)
                {
                    // Section.GetObjectData writes its lazy shape field. Materialize the authoritative shape first,
                    // otherwise merely viewing/validating a section would change its revision.
                    if (value is Sections.ISectionShape section) { var shape = section.Shape; }
                    var info = new SerializationInfo(type, new FormatterConverter()); serializable.GetObjectData(info, new StreamingContext());
                    var entries = new List<SerializationEntry>(); foreach (SerializationEntry e in info) entries.Add(e);
                    foreach (var e in entries.OrderBy(e => e.Name, StringComparer.Ordinal))
                    {
                        if (e.Name.EndsWith("Version", StringComparison.Ordinal) || e.Name == "Guid") continue;
                        w.Write(e.Name); Write(w, e.Value, path);
                    }
                    return;
                }
                var members = FingerprintContracts.Members(type);
                if (members != null)
                {
                    foreach (var member in members)
                    {
                        var fieldValue = member.Accessor.GetValue(value);
                        if (fieldValue == null && member.OmitNull) continue;
                        w.Write(member.WireName); Write(w, fieldValue, path);
                    }
                    return;
                }
                // Compatibility for caller-owned records. Persisted Model records use the explicit vocabulary above.
                for (Type current = type; current != null; current = current.BaseType)
                    foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(f => !f.IsNotSerialized).OrderBy(f => f.Name, StringComparer.Ordinal))
                    {
                        var fieldValue = field.GetValue(value);
                        if (fieldValue == null && field.IsDefined(typeof(FingerprintWhenSetAttribute), false)) continue;
                        w.Write(field.Name); Write(w, fieldValue, path);
                    }
            }
            finally { path.Remove(value); }
        }
        private sealed class ReferenceComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceComparer Instance = new ReferenceComparer();
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
        }
    }
}
