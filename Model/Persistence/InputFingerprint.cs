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

namespace GPC.Model.Persistence
{
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
            Type type = value.GetType(); w.Write(type.FullName);
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
                // Only input data records reach this branch; no computed property getters are invoked.
                for (Type current = type; current != null; current = current.BaseType)
                    foreach (var field in current.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly).Where(f => !f.IsNotSerialized).OrderBy(f => f.Name, StringComparer.Ordinal))
                    { w.Write(field.Name); Write(w, field.GetValue(value), path); }
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
