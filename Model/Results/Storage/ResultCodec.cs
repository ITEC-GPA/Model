using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;

namespace GPC.Model.Results.Storage
{
    /// <summary>A run of consecutive <see cref="ElementResult"/> of one element, encoded once and never modified.</summary>
    internal sealed class ResultSegment
    {
        internal byte[] Payload { get; }
        internal int Length { get; }
        internal int Count { get; }
        internal byte[] Digest { get; }
        internal string[] Datasets { get; }
        internal ResultSegment(byte[] payload, int length, int count, byte[] digest, string[] datasets)
        { Payload = payload; Length = length; Count = count; Digest = digest; Datasets = datasets; }
        internal bool SameContent(ResultSegment other) => other != null && Length == other.Length && Count == other.Count && Digest.SequenceEqual(other.Digest);
    }

    /// <summary>The compact, immutable results of one element: segments in list order. Shared by copies of the element.</summary>
    internal sealed class ResultStorage
    {
        internal static readonly ResultStorage Empty = new ResultStorage(new ResultSegment[0]);
        private byte[] _digest;
        internal ResultSegment[] Segments { get; }
        internal int Count { get; }
        internal ResultStorage(ResultSegment[] segments) { Segments = segments; Count = segments.Sum(s => s.Count); }
        internal ResultStorage Append(ResultSegment segment) => new ResultStorage(Segments.Concat(new[] { segment }).ToArray());
        internal long Bytes => Segments.Sum(s => (long)s.Payload.Length);
        internal bool ContainsDataset(string id) => Segments.Any(s => Array.IndexOf(s.Datasets, id) >= 0);
        /// <summary>SHA-256 of the segment digests in order: equal lists encode to equal segments, so equal results give equal digests
        /// whether the element was read in the meantime or not.</summary>
        internal byte[] Digest
        {
            get
            {
                if (_digest != null) return _digest;
                using (var sha = SHA256.Create())
                {
                    var all = new byte[Segments.Length * 36];
                    for (int i = 0; i < Segments.Length; i++)
                    { Buffer.BlockCopy(BitConverter.GetBytes(Segments[i].Count), 0, all, i * 36, 4); Buffer.BlockCopy(Segments[i].Digest, 0, all, i * 36 + 4, 32); }
                    return _digest = sha.ComputeHash(all);
                }
            }
        }
    }

    /// <summary>The cases and combinations of the model that results may reference, by reference (writing) and by name/GUID (reading).</summary>
    internal sealed class ResultCaseTable
    {
        private readonly Dictionary<object, Tuple<byte, Guid, string>> _keys = new Dictionary<object, Tuple<byte, Guid, string>>(ResultIdentity.Instance);
        private readonly Models.Model _model;
        internal ResultCaseTable(Models.Model model)
        {
            _model = model;
            foreach (var c in model.LoadCases.Values) if (c != null) _keys[c] = Tuple.Create((byte)0, c.Guid, c.Name);
            foreach (var c in model.Combinations.Values) if (c != null) _keys[c] = Tuple.Create((byte)1, c.Guid, c.Name);
        }
        internal Tuple<byte, Guid, string> Key(ILoadCase value)
            => _keys.TryGetValue(value, out var key) ? key : throw new NotSupportedException("ResultCaseNotInModel: " + (value as ModelObject)?.Name);
        internal static ILoadCase Resolve(Models.Model model, byte kind, Guid guid, string name)
        {
            if (kind == 0)
            {
                if (model.LoadCases.TryGetValue(name, out var named) && named.Guid == guid) return named;
                var byGuid = model.LoadCases.Values.Where(c => c.Guid == guid).Take(2).ToArray();
                if (byGuid.Length == 1) return byGuid[0];
            }
            else if (kind == 1)
            {
                if (model.Combinations.TryGetValue(name, out var named) && named.Guid == guid) return named;
                var byGuid = model.Combinations.Values.Where(c => c.Guid == guid).Take(2).ToArray();
                if (byGuid.Length == 1) return byGuid[0];
            }
            throw new SerializationException("ResultCaseMissing: " + name);
        }
        internal Models.Model Model => _model;
    }

    /// <summary>The results of an element reference a sample of another element, or an object outside the result vocabulary.</summary>
    internal sealed class ResultEncodingException : NotSupportedException
    {
        internal bool Foreign { get; }
        internal ResultEncodingException(string message, bool foreign) : base(message) { Foreign = foreign; }
    }

    internal sealed class ResultIdentity : IEqualityComparer<object>
    {
        internal static readonly ResultIdentity Instance = new ResultIdentity();
        public new bool Equals(object x, object y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => RuntimeHelpers.GetHashCode(obj);
    }

    /// <summary>
    /// Binary codec of the results of one element. It follows the same contracts as the XML serializer (GetObjectData and the
    /// deserialization constructor, DataMember fields, Serializable fields), keeps shared references inside the element, writes
    /// cases and combinations as references to the owning model and resolves type names only inside a closed vocabulary.
    /// Field names are written once per layout, strings once per segment. The GUIDs of the result geometries (points, vectors,
    /// axes) are not identities: they are not written and are generated again on reading, as in the copies of the geometry.
    /// </summary>
    internal static class ResultCodec
    {
        private const int Magic = 0x52435047; // "GPCR"
        private const byte Version = 1;
        private static readonly StreamingContext Context = new StreamingContext(StreamingContextStates.All);
        private static readonly FormatterConverter Converter = new FormatterConverter();
        private enum Tag : byte
        {
            Null = 0, Ref = 1, Case = 2, NewCase = 3, False = 4, True = 5, Int32 = 6, Int64 = 7, Double = 8, String = 9, Guid = 10, Enum = 11,
            Decimal = 12, Byte = 13, SByte = 14, Int16 = 15, UInt16 = 16, UInt32 = 17, UInt64 = 18, Single = 19, Char = 20, DateTime = 21, TimeSpan = 22,
            Object = 30, Array = 31, Doubles = 32, List = 33
        }

        #region Vocabulary

        private enum ShapeKind { Serializable, DataContract, Custom }
        private sealed class Member
        {
            internal string Name; internal FieldInfo Field; internal PropertyInfo Property;
            internal object Get(object o) => Field != null ? Field.GetValue(o) : Property.GetValue(o);
            internal void Set(object o, object v) { if (Field != null) Field.SetValue(o, v); else Property.SetValue(o, v); }
        }
        private sealed class Shape
        {
            internal Type Type; internal ShapeKind Kind; internal Member[] Members; internal Dictionary<string, Member> ByName;
            internal ConstructorInfo Constructor; internal bool Geometry;
            internal MethodInfo[] OnSerializing, OnSerialized, OnDeserializing, OnDeserialized;
        }

        private static readonly Dictionary<string, Type> TypesByName;
        private static readonly Dictionary<Type, string> NamesByType;
        private static readonly ConcurrentDictionary<Type, Shape> Shapes = new ConcurrentDictionary<Type, Shape>();

        static ResultCodec()
        {
            var types = new HashSet<Type>();
            var primitives = new[] { typeof(bool), typeof(byte), typeof(sbyte), typeof(short), typeof(ushort), typeof(int), typeof(uint), typeof(long),
                typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(char), typeof(string), typeof(Guid), typeof(DateTime), typeof(TimeSpan), typeof(object) };
            foreach (var t in primitives) { types.Add(t); if (t.IsValueType) types.Add(typeof(Nullable<>).MakeGenericType(t)); }
            var pending = new Stack<Type>(Core.ModelValues.DataContracts);
            while (pending.Count != 0)
            {
                var t = pending.Pop();
                if (t == null || t.IsGenericParameter || t.IsPointer || t.IsByRef) continue;
                if (t.IsArray) { if (t.GetArrayRank() == 1) pending.Push(t.GetElementType()); continue; }
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>)) { pending.Push(t.GetGenericArguments()[0]); continue; }
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>)) { var u = t.GetGenericArguments()[0]; if (types.Add(t)) pending.Push(u); continue; }
                if (t.IsInterface || t.ContainsGenericParameters || !types.Add(t) || t.IsEnum || t.IsPrimitive) continue;
                // Declared member types of contracts and serializable records (values in GetObjectData are typed at run time).
                if (typeof(ISerializable).IsAssignableFrom(t)) continue;
                foreach (var m in DeclaredMembers(t)) pending.Push(m.Field?.FieldType ?? m.Property.PropertyType);
            }
            NamesByType = types.ToDictionary(t => t, StableName);
            TypesByName = NamesByType.GroupBy(p => p.Value, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.First().Key, StringComparer.Ordinal);
        }

        private static string StableName(Type t)
        {
            if (t.IsGenericType && !t.IsGenericTypeDefinition)
                return t.GetGenericTypeDefinition().FullName + "[" + string.Join(",", t.GetGenericArguments().Select(StableName)) + "]";
            return t.FullName;
        }

        private static IEnumerable<Member> DeclaredMembers(Type type)
        {
            if (type.GetCustomAttribute<DataContractAttribute>(false) != null)
            {
                var list = new List<Member>();
                for (var t = type; t != null && t != typeof(object); t = t.BaseType)
                {
                    foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    { var a = f.GetCustomAttribute<DataMemberAttribute>(); if (a != null) list.Add(new Member { Name = t.Name + ":" + (a.Name ?? f.Name), Field = f }); }
                    foreach (var p in t.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    { var a = p.GetCustomAttribute<DataMemberAttribute>(); if (a != null) list.Add(new Member { Name = t.Name + ":" + (a.Name ?? p.Name), Property = p }); }
                }
                return list.OrderBy(m => m.Name, StringComparer.Ordinal);
            }
            var fields = new List<Member>();
            for (var t = type; t != null && t != typeof(object); t = t.BaseType)
                foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    if (!f.IsNotSerialized) fields.Add(new Member { Name = t.Name + ":" + f.Name, Field = f });
            return fields.OrderBy(m => m.Name, StringComparer.Ordinal);
        }

        private static Shape ShapeOf(Type type) => Shapes.GetOrAdd(type, t =>
        {
            var shape = new Shape { Type = t, Geometry = typeof(global::GPC.Geometry.BaseObject).IsAssignableFrom(t) };
            if (typeof(IObjectReference).IsAssignableFrom(t)) throw new ResultEncodingException("UnsupportedResultValue: " + t.FullName, false);
            if (typeof(ISerializable).IsAssignableFrom(t))
            {
                shape.Kind = ShapeKind.Custom;
                shape.Constructor = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                    new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null) ?? throw new ResultEncodingException("MissingDeserializationConstructor: " + t.FullName, false);
            }
            else if (t.GetCustomAttribute<DataContractAttribute>(false) != null || t.IsSerializable)
            {
                shape.Kind = t.GetCustomAttribute<DataContractAttribute>(false) != null ? ShapeKind.DataContract : ShapeKind.Serializable;
                shape.Members = DeclaredMembers(t).ToArray(); shape.ByName = shape.Members.ToDictionary(m => m.Name, StringComparer.Ordinal);
            }
            else throw new ResultEncodingException("UnsupportedResultValue: " + t.FullName, false);
            MethodInfo[] Callbacks(Type attribute)
            {
                var methods = new List<MethodInfo>();
                for (var b = t; b != null && b != typeof(object); b = b.BaseType)
                    methods.InsertRange(0, b.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                        .Where(m => m.IsDefined(attribute, false)));
                return methods.ToArray();
            }
            shape.OnSerializing = Callbacks(typeof(OnSerializingAttribute)); shape.OnSerialized = Callbacks(typeof(OnSerializedAttribute));
            shape.OnDeserializing = Callbacks(typeof(OnDeserializingAttribute)); shape.OnDeserialized = Callbacks(typeof(OnDeserializedAttribute));
            return shape;
        });

        // Model objects that belong to the results (lists, samples, values); the entities of the model are never copied into them.
        private static bool IsResultObject(Type type) => type.Namespace != null && (type.Namespace == "GPC.Model.Results" || type.Namespace.StartsWith("GPC.Model.Results.", StringComparison.Ordinal));

        private static void Invoke(MethodInfo[] methods, object target) { foreach (var m in methods) m.Invoke(target, new object[] { Context }); }

        #endregion

        #region Writing

        /// <summary>Encodes <paramref name="count"/> results from <paramref name="start"/>. References outside the run are rejected.</summary>
        internal static ResultSegment Encode(IReadOnlyList<ElementResult> results, int start, int count, ResultCaseTable cases)
        {
            var writer = new Writer(cases);
            for (int i = start; i < start + count; i++)
            {
                var r = results[i];
                if (r == null) throw new ResultEncodingException("NullElementResult", false);
                writer.Owned.Add(r);
                if (r.Results != null) foreach (var s in r.Results) if (s != null) writer.Owned.Add(s);
            }
            writer.Header(count);
            for (int i = start; i < start + count; i++) writer.Value(results[i]);
            return writer.Finish(count);
        }

        private sealed class Writer
        {
            private readonly MemoryStream _stream = new MemoryStream();
            private readonly BinaryWriter _out;
            private readonly ResultCaseTable _cases;
            private readonly Dictionary<object, int> _objects = new Dictionary<object, int>(ResultIdentity.Instance);
            private readonly Dictionary<string, int> _strings = new Dictionary<string, int>(StringComparer.Ordinal);
            private readonly Dictionary<Type, int> _types = new Dictionary<Type, int>();
            private readonly Dictionary<Type, List<KeyValuePair<string[], int>>> _layouts = new Dictionary<Type, List<KeyValuePair<string[], int>>>();
            private int _layoutCount;
            private readonly Dictionary<object, int> _caseIndex = new Dictionary<object, int>(ResultIdentity.Instance);
            private readonly SortedSet<string> _datasets = new SortedSet<string>(StringComparer.Ordinal);
            internal readonly HashSet<object> Owned = new HashSet<object>(ResultIdentity.Instance);
            internal Writer(ResultCaseTable cases) { _cases = cases; _out = new BinaryWriter(_stream, new UTF8Encoding(false, true)); }

            internal void Header(int count) { _out.Write(Magic); _out.Write(Version); VarUInt(count); }

            internal ResultSegment Finish(int count)
            {
                _out.Flush();
                var raw = _stream.GetBuffer(); int length = (int)_stream.Length;
                byte[] digest; using (var sha = SHA256.Create()) digest = sha.ComputeHash(raw, 0, length);
                using (var packed = new MemoryStream())
                {
                    using (var deflate = new DeflateStream(packed, CompressionLevel.Fastest, true)) deflate.Write(raw, 0, length);
                    return new ResultSegment(packed.ToArray(), length, count, digest, _datasets.ToArray());
                }
            }

            private void VarUInt(long value) { ulong v = (ulong)value; while (v >= 0x80) { _out.Write((byte)(v | 0x80)); v >>= 7; } _out.Write((byte)v); }
            private void VarInt(long value) => VarUInt((value << 1) ^ (value >> 63));
            private void Write(Tag tag) => _out.Write((byte)tag);

            private void String(string value)
            {
                if (_strings.TryGetValue(value, out int index)) { VarUInt(index + 1); return; }
                VarUInt(0); var bytes = Encoding.UTF8.GetBytes(value); VarUInt(bytes.Length); _out.Write(bytes);
                _strings.Add(value, _strings.Count);
            }

            private void TypeRef(Type type)
            {
                if (_types.TryGetValue(type, out int index)) { VarUInt(index + 1); return; }
                if (!NamesByType.TryGetValue(type, out var name)) throw new ResultEncodingException("UnregisteredResultType: " + type.FullName, false);
                VarUInt(0); String(name); _types.Add(type, _types.Count);
            }

            internal void Value(object value)
            {
                if (value == null) { Write(Tag.Null); return; }
                var type = value.GetType();
                if (type.IsEnum) { Write(Tag.Enum); TypeRef(type); VarInt(Convert.ToInt64(value)); return; }
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean: Write((bool)value ? Tag.True : Tag.False); return;
                    case TypeCode.Int32: Write(Tag.Int32); VarInt((int)value); return;
                    case TypeCode.Int64: Write(Tag.Int64); VarInt((long)value); return;
                    case TypeCode.Double: Write(Tag.Double); _out.Write((double)value); return;
                    case TypeCode.String: Write(Tag.String); String((string)value); return;
                    case TypeCode.Decimal: Write(Tag.Decimal); _out.Write((decimal)value); return;
                    case TypeCode.Byte: Write(Tag.Byte); _out.Write((byte)value); return;
                    case TypeCode.SByte: Write(Tag.SByte); _out.Write((sbyte)value); return;
                    case TypeCode.Int16: Write(Tag.Int16); _out.Write((short)value); return;
                    case TypeCode.UInt16: Write(Tag.UInt16); _out.Write((ushort)value); return;
                    case TypeCode.UInt32: Write(Tag.UInt32); _out.Write((uint)value); return;
                    case TypeCode.UInt64: Write(Tag.UInt64); _out.Write((ulong)value); return;
                    case TypeCode.Single: Write(Tag.Single); _out.Write((float)value); return;
                    case TypeCode.Char: Write(Tag.Char); _out.Write((ushort)(char)value); return;
                    case TypeCode.DateTime: Write(Tag.DateTime); _out.Write(((DateTime)value).ToBinary()); return;
                }
                if (value is Guid guid) { Write(Tag.Guid); _out.Write(guid.ToByteArray()); return; }
                if (value is TimeSpan span) { Write(Tag.TimeSpan); _out.Write(span.Ticks); return; }
                if (!type.IsValueType && _objects.TryGetValue(value, out int id)) { Write(Tag.Ref); VarUInt(id); return; }
                if (value is ILoadCase loadCase) { Case(loadCase); return; }
                if ((value is ElementResult || value is ResultLocation) && !Owned.Contains(value))
                    throw new ResultEncodingException("ForeignResultReference", true);
                if (value is ModelObject && !IsResultObject(type))
                    throw new ResultEncodingException("ModelEntityInResults: " + type.FullName, false);
                if (!type.IsValueType) _objects.Add(value, _objects.Count);
                if (value is double[] doubles) { Write(Tag.Doubles); VarUInt(doubles.Length); foreach (var d in doubles) _out.Write(d); return; }
                if (type.IsArray)
                {
                    if (type.GetArrayRank() != 1) throw new ResultEncodingException("UnsupportedResultValue: " + type.FullName, false);
                    var array = (Array)value; Write(Tag.Array); TypeRef(type.GetElementType()); VarUInt(array.Length);
                    foreach (var item in array) Value(item);
                    return;
                }
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var list = (IList)value; Write(Tag.List); TypeRef(type.GetGenericArguments()[0]); VarUInt(list.Count);
                    foreach (var item in list) Value(item);
                    return;
                }
                if (value is ResultState state && state.DatasetId != null) _datasets.Add(state.DatasetId);
                Object(value, type);
            }

            private void Case(ILoadCase value)
            {
                if (_caseIndex.TryGetValue(value, out int index)) { Write(Tag.Case); VarUInt(index); return; }
                var key = _cases.Key(value);
                Write(Tag.NewCase); _out.Write(key.Item1); _out.Write(key.Item2.ToByteArray()); String(key.Item3 ?? "");
                _caseIndex.Add(value, _caseIndex.Count);
            }

            private void Object(object value, Type type)
            {
                if (!NamesByType.ContainsKey(type)) throw new ResultEncodingException("UnregisteredResultType: " + type.FullName, false);
                var shape = ShapeOf(type);
                Invoke(shape.OnSerializing, value);
                string[] names; object[] values;
                if (shape.Kind == ShapeKind.Custom)
                {
                    var info = new SerializationInfo(type, Converter);
                    ((ISerializable)value).GetObjectData(info, Context);
                    if (info.ObjectType != type) throw new ResultEncodingException("SerializationTypeChange: " + type.FullName, false);
                    var entries = new List<SerializationEntry>(info.MemberCount);
                    foreach (SerializationEntry e in info) if (!(shape.Geometry && e.Name == "Guid")) entries.Add(e);
                    names = entries.Select(e => e.Name).ToArray(); values = entries.Select(e => e.Value).ToArray();
                }
                else
                {
                    names = shape.Members.Select(m => m.Name).ToArray(); values = shape.Members.Select(m => m.Get(value)).ToArray();
                }
                Write(Tag.Object); Layout(type, names);
                foreach (var v in values) Value(v);
                Invoke(shape.OnSerialized, value);
            }

            private void Layout(Type type, string[] names)
            {
                if (!_layouts.TryGetValue(type, out var known)) _layouts.Add(type, known = new List<KeyValuePair<string[], int>>());
                foreach (var layout in known)
                    if (layout.Key.Length == names.Length && Same(layout.Key, names)) { VarUInt(layout.Value + 1); return; }
                VarUInt(0); TypeRef(type); VarUInt(names.Length); foreach (var n in names) String(n);
                known.Add(new KeyValuePair<string[], int>(names, _layoutCount++));
            }

            private static bool Same(string[] a, string[] b)
            {
                for (int i = 0; i < a.Length; i++) if (!string.Equals(a[i], b[i], StringComparison.Ordinal)) return false;
                return true;
            }
        }

        #endregion

        #region Reading

        /// <summary>Decodes the segments in order into new objects whose cases are those of <paramref name="model"/>.</summary>
        internal static List<ElementResult> Decode(ResultStorage storage, Models.Model model)
        {
            var list = new List<ElementResult>(storage.Count);
            foreach (var segment in storage.Segments) list.AddRange(Decode(segment, model));
            return list;
        }

        internal static List<ElementResult> Decode(ResultSegment segment, Models.Model model)
        {
            var raw = new byte[segment.Length];
            try
            {
                using (var inflate = new DeflateStream(new MemoryStream(segment.Payload, false), CompressionMode.Decompress))
                {
                    int read = 0;
                    while (read < raw.Length) { int n = inflate.Read(raw, read, raw.Length - read); if (n == 0) break; read += n; }
                    if (read != raw.Length || inflate.ReadByte() != -1) throw new SerializationException("CorruptResultSegment");
                }
            }
            catch (InvalidDataException ex) { throw new SerializationException("CorruptResultSegment", ex); }
            using (var sha = SHA256.Create()) if (!sha.ComputeHash(raw).SequenceEqual(segment.Digest)) throw new SerializationException("CorruptResultSegment");
            var reader = new Reader(raw, model);
            int count = reader.Header();
            if (count != segment.Count) throw new SerializationException("CorruptResultSegment");
            var list = new List<ElementResult>(count);
            for (int i = 0; i < count; i++) list.Add(reader.Value() as ElementResult ?? throw new SerializationException("CorruptResultSegment"));
            reader.End();
            return list;
        }

        private sealed class Layout { internal Type Type; internal string[] Names; internal Shape Shape; internal bool NewGuid; internal Member[] Members; }

        private sealed class Reader
        {
            private readonly byte[] _data;
            private int _at;
            private readonly Models.Model _model;
            private readonly List<object> _objects = new List<object>();
            private readonly List<string> _strings = new List<string>();
            private readonly List<Type> _types = new List<Type>();
            private readonly List<Layout> _layouts = new List<Layout>();
            private readonly List<ILoadCase> _cases = new List<ILoadCase>();
            private readonly List<object> _completed = new List<object>();
            internal Reader(byte[] data, Models.Model model) { _data = data; _model = model; }

            private byte Byte() { if (_at >= _data.Length) throw new SerializationException("CorruptResultSegment"); return _data[_at++]; }
            private byte[] Bytes(int n) { if (n < 0 || _at + n > _data.Length) throw new SerializationException("CorruptResultSegment"); var b = new byte[n]; Buffer.BlockCopy(_data, _at, b, 0, n); _at += n; return b; }
            private long VarUInt()
            {
                ulong v = 0; int shift = 0;
                while (true) { byte b = Byte(); v |= (ulong)(b & 0x7F) << shift; if (b < 0x80) break; shift += 7; if (shift > 63) throw new SerializationException("CorruptResultSegment"); }
                return (long)v;
            }
            private int Index(int count) { long v = VarUInt(); if (v < 0 || v > count) throw new SerializationException("CorruptResultSegment"); return (int)v; }
            private long VarInt() { long v = VarUInt(); return (long)((ulong)v >> 1) ^ -(v & 1); }
            private double Double() { var v = BitConverter.ToDouble(_data, Check(8)); return v; }
            private int Check(int n) { if (_at + n > _data.Length) throw new SerializationException("CorruptResultSegment"); int at = _at; _at += n; return at; }

            internal int Header()
            {
                if (BitConverter.ToInt32(_data, Check(4)) != Magic || Byte() != Version) throw new SerializationException("UnsupportedResultSegment");
                long count = VarUInt(); if (count < 0 || count > int.MaxValue) throw new SerializationException("CorruptResultSegment");
                return (int)count;
            }
            internal void End()
            {
                if (_at != _data.Length) throw new SerializationException("CorruptResultSegment");
                foreach (var o in _completed) if (o is IDeserializationCallback callback) callback.OnDeserialization(null);
            }

            private string String()
            {
                int index = Index(_strings.Count);
                if (index != 0) return _strings[index - 1];
                long length = VarUInt(); if (length > _data.Length) throw new SerializationException("CorruptResultSegment");
                var value = Encoding.UTF8.GetString(_data, Check((int)length), (int)length);
                _strings.Add(value); return value;
            }

            private Type TypeRef()
            {
                int index = Index(_types.Count);
                if (index != 0) return _types[index - 1];
                var name = String();
                if (!TypesByName.TryGetValue(name, out var type)) throw new SerializationException("UnknownResultType: " + name);
                _types.Add(type); return type;
            }

            internal object Value()
            {
                var tag = (Tag)Byte();
                switch (tag)
                {
                    case Tag.Null: return null;
                    case Tag.Ref: { long id = VarUInt(); if (id < 0 || id >= _objects.Count) throw new SerializationException("CorruptResultSegment"); return _objects[(int)id]; }
                    case Tag.Case: { long id = VarUInt(); if (id < 0 || id >= _cases.Count) throw new SerializationException("CorruptResultSegment"); return _cases[(int)id]; }
                    case Tag.NewCase:
                        {
                            byte kind = Byte(); var guid = new Guid(Bytes(16)); var name = String();
                            var value = ResultCaseTable.Resolve(_model, kind, guid, name); _cases.Add(value); return value;
                        }
                    case Tag.False: return false;
                    case Tag.True: return true;
                    case Tag.Int32: return checked((int)VarInt());
                    case Tag.Int64: return VarInt();
                    case Tag.Double: return Double();
                    case Tag.String: return String();
                    case Tag.Guid: return new Guid(Bytes(16));
                    case Tag.Enum: { var type = TypeRef(); if (!type.IsEnum) throw new SerializationException("CorruptResultSegment"); return System.Enum.ToObject(type, VarInt()); }
                    case Tag.Decimal: { var b = Bytes(16); return new decimal(new[] { BitConverter.ToInt32(b, 0), BitConverter.ToInt32(b, 4), BitConverter.ToInt32(b, 8), BitConverter.ToInt32(b, 12) }); }
                    case Tag.Byte: return Byte();
                    case Tag.SByte: return (sbyte)Byte();
                    case Tag.Int16: return BitConverter.ToInt16(_data, Check(2));
                    case Tag.UInt16: return BitConverter.ToUInt16(_data, Check(2));
                    case Tag.UInt32: return BitConverter.ToUInt32(_data, Check(4));
                    case Tag.UInt64: return BitConverter.ToUInt64(_data, Check(8));
                    case Tag.Single: return BitConverter.ToSingle(_data, Check(4));
                    case Tag.Char: return (char)BitConverter.ToUInt16(_data, Check(2));
                    case Tag.DateTime: return DateTime.FromBinary(BitConverter.ToInt64(_data, Check(8)));
                    case Tag.TimeSpan: return new TimeSpan(BitConverter.ToInt64(_data, Check(8)));
                    case Tag.Doubles:
                        {
                            long n = VarUInt(); if (n < 0 || n * 8 > _data.Length - _at) throw new SerializationException("CorruptResultSegment");
                            var array = new double[n]; _objects.Add(array);
                            Buffer.BlockCopy(_data, Check((int)n * 8), array, 0, (int)n * 8); return array;
                        }
                    case Tag.Array:
                        {
                            var element = TypeRef(); long n = VarUInt(); if (n < 0 || n > _data.Length - _at) throw new SerializationException("CorruptResultSegment");
                            var array = System.Array.CreateInstance(element, (int)n); _objects.Add(array);
                            for (int i = 0; i < n; i++) array.SetValue(Value(), i);
                            return array;
                        }
                    case Tag.List:
                        {
                            var element = TypeRef(); long n = VarUInt(); if (n < 0 || n > _data.Length - _at) throw new SerializationException("CorruptResultSegment");
                            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(element), (int)n); _objects.Add(list);
                            for (int i = 0; i < n; i++) list.Add(Value());
                            return list;
                        }
                    case Tag.Object: return Object();
                    default: throw new SerializationException("CorruptResultSegment");
                }
            }

            private Layout LayoutRef()
            {
                int index = Index(_layouts.Count);
                if (index != 0) return _layouts[index - 1];
                var type = TypeRef(); long n = VarUInt(); if (n < 0 || n > _data.Length - _at) throw new SerializationException("CorruptResultSegment");
                var names = new string[n]; for (int i = 0; i < n; i++) names[i] = String();
                // Checked once per layout, not per object.
                if (type.IsAbstract || type.IsInterface || type.IsArray || type.IsPrimitive || type.IsEnum || type == typeof(string)
                    || typeof(ModelObject).IsAssignableFrom(type) && !IsResultObject(type)) throw new SerializationException("UnsupportedResultType: " + type.FullName);
                var layout = new Layout { Type = type, Names = names };
                try { layout.Shape = ShapeOf(type); }
                catch (ResultEncodingException ex) { throw new SerializationException(ex.Message); }
                layout.NewGuid = layout.Shape.Geometry && System.Array.IndexOf(names, "Guid") < 0;
                if (layout.Shape.Kind != ShapeKind.Custom)
                {
                    layout.Members = new Member[n];
                    for (int i = 0; i < n; i++)
                        if (!layout.Shape.ByName.TryGetValue(names[i], out layout.Members[i])) throw new SerializationException("UnknownResultMember: " + type.FullName + "." + names[i]);
                }
                _layouts.Add(layout); return layout;
            }

            private object Object()
            {
                var layout = LayoutRef(); var type = layout.Type; var shape = layout.Shape;
                var value = FormatterServices.GetUninitializedObject(type);
                int id = -1;
                if (!type.IsValueType) { id = _objects.Count; _objects.Add(value); }
                Invoke(shape.OnDeserializing, value);
                if (shape.Kind == ShapeKind.Custom)
                {
                    var info = new SerializationInfo(type, Converter);
                    if (layout.NewGuid) info.AddValue("Guid", System.Guid.NewGuid());
                    foreach (var name in layout.Names) info.AddValue(name, Value());
                    try { shape.Constructor.Invoke(value, new object[] { info, Context }); }
                    catch (TargetInvocationException ex) { throw new SerializationException("ResultConstructorFailed: " + type.FullName, ex.InnerException); }
                }
                else
                {
                    foreach (var member in layout.Members) member.Set(value, Value());
                }
                Invoke(shape.OnDeserialized, value);
                if (value is IDeserializationCallback) _completed.Add(value);
                return value;
            }
        }

        #endregion
    }
}
