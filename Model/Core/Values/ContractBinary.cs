using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Text;
using GPC.Geometry;
using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Numerics.LinearAlgebra.Storage;

namespace GPC.Model.Core
{
    /// <summary>
    /// Compact binary form of a graph of the vocabulary: the format of the compact archives of GPC.Model.Persistence (review A2,
    /// lane L5). It writes what the data contracts of <see cref="ModelValues"/> write, and reads back what DataContractSerializer
    /// reads, with the plans of <see cref="ContractCopy"/> (same contracts, same order of construction and callbacks):
    /// - references are preserved (an object is written once, then referenced by its number in order of appearance);
    /// - a type is written once per archive by its stable name, resolved on reading only inside the closed vocabulary
    ///   (<see cref="ContractCopy.Vocabulary"/> and the built-in values), never by a CLR type name of the data;
    /// - a value of the declared type carries no type at all; the names of the entries of GetObjectData and of the data members are
    ///   written once per layout, strings once per archive;
    /// - doubles are exact (8 bytes, or one byte for 0, -0, 1, -1, a varint for an integral value): no text conversion;
    /// - Point3d and Vector3d (sealed, their deserialization constructors read only X, Y and Z) are three doubles, Point2d and
    ///   Vector2d two, as in ContractCopy; a CoordinateSystem whose matrix and inverse origin are those its axes and origin give is written as its
    ///   origin, its axes and its name (the matrix and the inverse origin are rebuilt bit for bit, and keep their numbers).
    /// A shape this codec does not reproduce raises <see cref="ContractCopy.UnsupportedException"/> while writing: the caller writes
    /// the XML archive instead. Reading bounds every count by the bytes left, so a short input cannot declare large arrays.
    /// </summary>
    internal static class ContractBinary
    {
        private const int Magic = 0x47425047; // "GPBG"
        // Version 2: Point2d and Vector2d are leaves too (two doubles). Version 1 (lane L5, first commit) is still read.
        private const byte Version = 2;
        /// <summary>Deepest nesting written or read; deeper graphs are written as XML (and a deeper input is refused).</summary>
        internal const int MaximumDepth = 2000;
        private static readonly StreamingContext Context = new StreamingContext(StreamingContextStates.All);
        private static readonly FormatterConverter Converter = new FormatterConverter();
        private static readonly Encoding Text = new UTF8Encoding(false, true);

        private enum Tag : byte
        {
            Null = 0, Ref = 1, Declared = 2, Typed = 3, Frame = 4,
            False = 8, True = 9, Int32 = 10, Int64 = 11,
            Double = 12, Zero = 13, NegativeZero = 14, One = 15, MinusOne = 16, Integral = 17,
            String = 18, Guid = 19, Enum = 20, DeclaredEnum = 21,
            Decimal = 22, Byte = 23, SByte = 24, Int16 = 25, UInt16 = 26, UInt32 = 27, UInt64 = 28, Single = 29, Char = 30,
            DateTime = 31, TimeSpan = 32, Uri = 33
        }

        #region Names

        private static readonly Dictionary<Type, string> NamesByType;
        private static readonly Dictionary<string, Type> TypesByName;
        private static readonly string[] FrameEntries = { "Guid", "Origin", "V1Direction", "V2Direction", "V3Direction", "InvariantOrigin", "TransformationMatrix", "Name" };
        private static readonly Func<object, object> MatrixRows, MatrixColumns, MatrixValues;

        static ContractBinary()
        {
            // The types a value may carry where another type is declared: the closed vocabulary and the built-in values.
            var types = ContractCopy.Vocabulary.Concat(ContractCopy.Primitives).Distinct().ToArray();
            NamesByType = types.ToDictionary(t => t, StableName);
            TypesByName = new Dictionary<string, Type>(StringComparer.Ordinal);
            foreach (var pair in NamesByType)
            {
                if (TypesByName.ContainsKey(pair.Value)) throw new InvalidOperationException("Duplicate stable contract name: " + pair.Value);
                TypesByName.Add(pair.Value, pair.Key);
            }
            var matrix = ContractCopy.PlanOf(typeof(DenseMatrix));
            if (matrix.Members != null)
            {
                var fields = matrix.Members.Where(m => m.Field != null).ToDictionary(m => m.Field.Name, m => m.Get, StringComparer.Ordinal);
                fields.TryGetValue("_rowCount", out MatrixRows); fields.TryGetValue("_columnCount", out MatrixColumns); fields.TryGetValue("_values", out MatrixValues);
            }
        }

        /// <summary>Version-free CLR name, with the historical names of moved types (<see cref="HistoricalTypeNames"/>).</summary>
        private static string StableName(Type type)
        {
            if (type.IsArray) return StableName(type.GetElementType()) + "[]";
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
                return HistoricalTypeNames.Rewrite(type.GetGenericTypeDefinition().FullName) + "[" + string.Join(",", type.GetGenericArguments().Select(StableName)) + "]";
            return HistoricalTypeNames.Rewrite(type.FullName);
        }

        internal static bool IsBinary(byte[] bytes, int offset = 0) => bytes != null && bytes.Length >= offset + 5 && BitConverter.ToInt32(bytes, offset) == Magic;

        // The pairs of a dictionary, enumerated as IEnumerable<KeyValuePair<K, V>> as ContractCopy does (type metadata only).
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object, List<KeyValuePair<object, object>>>> Pairs =
            new System.Collections.Concurrent.ConcurrentDictionary<Type, Func<object, List<KeyValuePair<object, object>>>>();
        private static Func<object, List<KeyValuePair<object, object>>> PairsOf(Type type, ContractCopy.Plan plan) => Pairs.GetOrAdd(type, t =>
            (Func<object, List<KeyValuePair<object, object>>>)Delegate.CreateDelegate(typeof(Func<object, List<KeyValuePair<object, object>>>),
                typeof(ContractBinary).GetMethod(nameof(PairList), BindingFlags.Static | BindingFlags.NonPublic).MakeGenericMethod(plan.Key, plan.Item)));
        private static List<KeyValuePair<object, object>> PairList<TKey, TValue>(object dictionary)
        {
            var list = new List<KeyValuePair<object, object>>();
            foreach (var pair in (IEnumerable<KeyValuePair<TKey, TValue>>)dictionary) list.Add(new KeyValuePair<object, object>(pair.Key, pair.Value));
            return list;
        }

        #endregion

        #region Writing

        /// <summary>Writes <paramref name="root"/>, declared as <paramref name="rootType"/>. Throws <see cref="ContractCopy.UnsupportedException"/>
        /// (before or while writing) for a shape the codec does not reproduce: the caller then writes the XML form.</summary>
        internal static void Write(Stream destination, object root, Type rootType)
        {
            if (destination == null || rootType == null) throw new ArgumentNullException();
            var writer = new Writer(destination);
            writer.Int32(Magic); writer.Byte(Version);
            writer.Value(root, rootType, 0);
            writer.Flush();
        }

        private sealed class Writer
        {
            private readonly Stream _destination;
            private readonly byte[] _buffer = new byte[1 << 16];
            private int _at;
            private readonly Dictionary<object, int> _objects = new Dictionary<object, int>(1 << 12, ReferenceComparer<object>.Instance);
            private readonly Dictionary<string, int> _strings = new Dictionary<string, int>(StringComparer.Ordinal);
            private readonly Dictionary<Type, int> _types = new Dictionary<Type, int>();
            private readonly Dictionary<Type, List<KeyValuePair<string[], int>>> _layouts = new Dictionary<Type, List<KeyValuePair<string[], int>>>();
            private int _layoutCount;
            internal Writer(Stream destination) { _destination = destination; }

            internal void Flush() { if (_at != 0) _destination.Write(_buffer, 0, _at); _at = 0; }
            private void Room(int bytes) { if (_at + bytes > _buffer.Length) Flush(); }
            internal void Byte(byte value) { Room(1); _buffer[_at++] = value; }
            private void Write(Tag tag) { Room(1); _buffer[_at++] = (byte)tag; }
            internal void Int32(int value) { Room(4); for (int i = 0; i < 4; i++) _buffer[_at++] = (byte)(value >> (8 * i)); }
            private void Int64(long value) { Room(8); for (int i = 0; i < 8; i++) _buffer[_at++] = (byte)(value >> (8 * i)); }
            private void VarUInt(ulong value) { Room(10); while (value >= 0x80) { _buffer[_at++] = (byte)(value | 0x80); value >>= 7; } _buffer[_at++] = (byte)value; }
            private void VarInt(long value) => VarUInt((ulong)((value << 1) ^ (value >> 63)));
            private void Raw(byte[] bytes)
            {
                if (bytes.Length > _buffer.Length - _at) { Flush(); if (bytes.Length > _buffer.Length) { _destination.Write(bytes, 0, bytes.Length); return; } }
                Buffer.BlockCopy(bytes, 0, _buffer, _at, bytes.Length); _at += bytes.Length;
            }

            private void String(string value)
            {
                if (_strings.TryGetValue(value, out int index)) { VarUInt((ulong)index + 1); return; }
                VarUInt(0); byte[] bytes;
                try { bytes = Text.GetBytes(value); }
                catch (EncoderFallbackException) { throw new ContractCopy.UnsupportedException("InvalidUnicodeString"); }
                 VarUInt((ulong)bytes.Length); Raw(bytes);
                _strings.Add(value, _strings.Count);
            }

            private void TypeRef(Type type)
            {
                if (_types.TryGetValue(type, out int index)) { VarUInt((ulong)index + 1); return; }
                if (!NamesByType.TryGetValue(type, out var name)) throw new ContractCopy.UnsupportedException("UnregisteredDomainValue: " + type.FullName);
                VarUInt(0); String(name); _types.Add(type, _types.Count);
            }

            private void Number(double value)
            {
                long bits = BitConverter.DoubleToInt64Bits(value);
                if (bits == 0) { Write(Tag.Zero); return; }
                if (bits == long.MinValue) { Write(Tag.NegativeZero); return; }
                if (value == 1) { Write(Tag.One); return; }
                if (value == -1) { Write(Tag.MinusOne); return; }
                if (value >= -9007199254740992.0 && value <= 9007199254740992.0 && Math.Floor(value) == value) { Write(Tag.Integral); VarInt((long)value); return; }
                Write(Tag.Double); Int64(bits);
            }

            private bool Seen(object value, Type type)
            {
                if (type.IsValueType) return false;
                if (_objects.TryGetValue(value, out int id)) { Write(Tag.Ref); VarUInt((ulong)id); return true; }
                return false;
            }
            private void Register(object value) => _objects.Add(value, _objects.Count);

            /// <summary>One value declared as <paramref name="declared"/>.</summary>
            internal void Value(object value, Type declared, int depth)
            {
                if (value == null) { Write(Tag.Null); return; }
                if (depth > MaximumDepth) throw new ContractCopy.UnsupportedException("GraphTooDeep");
                var type = value.GetType();
                if (Primitive(value, type, declared)) return;
                var plan = ContractCopy.PlanOf(type);
                if (!plan.Known && type != declared && type != Nullable.GetUnderlyingType(declared))
                    throw new ContractCopy.UnsupportedException("UnregisteredDomainValue: " + type.FullName);
                if (plan.Unsupported != null) throw new ContractCopy.UnsupportedException(plan.Unsupported);
                if (Seen(value, type)) return;
                if (type == typeof(CoordinateSystem) && Frame((CoordinateSystem)value, declared, depth)) return;
                if (type == declared || type == Nullable.GetUnderlyingType(declared)) Write(Tag.Declared);
                else { Write(Tag.Typed); TypeRef(type); }
                Content(value, type, plan, depth);
            }

            private bool Primitive(object value, Type type, Type declared)
            {
                if (type.IsEnum)
                {
                    if (type == declared || type == Nullable.GetUnderlyingType(declared)) Write(Tag.DeclaredEnum);
                    else { if (!ContractCopy.Vocabulary.Contains(type)) throw new ContractCopy.UnsupportedException("UnregisteredDomainValue: " + type.FullName); Write(Tag.Enum); TypeRef(type); }
                    VarInt(Convert.ToInt64(value)); return true;
                }
                switch (Type.GetTypeCode(type))
                {
                    case TypeCode.Boolean: Write((bool)value ? Tag.True : Tag.False); return true;
                    case TypeCode.Int32: Write(Tag.Int32); VarInt((int)value); return true;
                    case TypeCode.Int64: Write(Tag.Int64); VarInt((long)value); return true;
                    case TypeCode.Double: Number((double)value); return true;
                    case TypeCode.String:
                        // Strings are references, as for the serializer: an instance written again is a reference, an equal one a new string.
                        if (_objects.TryGetValue(value, out int known)) { Write(Tag.Ref); VarUInt((ulong)known); return true; }
                        Register(value); Write(Tag.String); String((string)value); return true;
                    case TypeCode.Decimal: { Write(Tag.Decimal); foreach (int part in decimal.GetBits((decimal)value)) Int32(part); return true; }
                    case TypeCode.Byte: Write(Tag.Byte); Byte((byte)value); return true;
                    case TypeCode.SByte: Write(Tag.SByte); Byte((byte)(sbyte)value); return true;
                    case TypeCode.Int16: Write(Tag.Int16); VarInt((short)value); return true;
                    case TypeCode.UInt16: Write(Tag.UInt16); VarUInt((ushort)value); return true;
                    case TypeCode.UInt32: Write(Tag.UInt32); VarUInt((uint)value); return true;
                    case TypeCode.UInt64: Write(Tag.UInt64); VarUInt((ulong)value); return true;
                    case TypeCode.Single: Write(Tag.Single); Int32(BitConverter.ToInt32(BitConverter.GetBytes((float)value), 0)); return true;
                    case TypeCode.Char: Write(Tag.Char); VarUInt((char)value); return true;
                    case TypeCode.DateTime: Write(Tag.DateTime); Int64(((DateTime)value).ToBinary()); return true;
                }
                if (type == typeof(Guid)) { Write(Tag.Guid); Raw(((Guid)value).ToByteArray()); return true; }
                if (type == typeof(TimeSpan)) { Write(Tag.TimeSpan); VarInt(((TimeSpan)value).Ticks); return true; }
                if (type == typeof(Uri)) { Write(Tag.Uri); String(((Uri)value).OriginalString); return true; }
                return false;
            }

            private void Content(object value, Type type, ContractCopy.Plan plan, int depth)
            {
                switch (plan.Kind)
                {
                    case ContractCopy.Kind.Array:
                        {
                            var array = (Array)value; Register(value); VarUInt((ulong)array.Length);
                            if (value is double[] doubles) foreach (var d in doubles) Number(d);
                            else if (value is byte[] bytes) Raw(bytes);
                            else if (value is object[] references) foreach (var item in references) Value(item, plan.Item, depth + 1);
                            else for (int i = 0; i < array.Length; i++) Value(array.GetValue(i), plan.Item, depth + 1);
                            return;
                        }
                    case ContractCopy.Kind.Pair:
                        Value(type.GetProperty("Key").GetValue(value), plan.Key, depth + 1);
                        Value(type.GetProperty("Value").GetValue(value), plan.Item, depth + 1);
                        return;
                    case ContractCopy.Kind.Collection:
                        {
                            Register(value); var items = new List<object>();
                            foreach (var item in (IEnumerable)value) items.Add(item);
                            VarUInt((ulong)items.Count);
                            foreach (var item in items) Value(item, plan.Item, depth + 1);
                            return;
                        }
                    case ContractCopy.Kind.Dictionary:
                        {
                            Register(value); var pairs = PairsOf(type, plan)(value);
                            VarUInt((ulong)pairs.Count);
                            foreach (var pair in pairs) { Value(pair.Key, plan.Key, depth + 1); Value(pair.Value, plan.Item, depth + 1); }
                            return;
                        }
                    case ContractCopy.Kind.Custom: Custom(value, type, plan, depth); return;
                    case ContractCopy.Kind.Members: Members(value, type, plan, depth); return;
                    default: throw new ContractCopy.UnsupportedException("UnsupportedValue: " + type.FullName);
                }
            }

            private void Custom(object value, Type type, ContractCopy.Plan plan, int depth)
            {
                if (type == typeof(Point3d) || type == typeof(Vector3d))
                {
                    // As ContractCopy: the writer requests the GUID, the reader generates a new one.
                    if (type == typeof(Point3d)) { var point = (Point3d)value; var guid = point.Guid; Register(value); Number(point.X); Number(point.Y); Number(point.Z); }
                    else { var vector = (Vector3d)value; var guid = vector.Guid; Register(value); Number(vector.X); Number(vector.Y); Number(vector.Z); }
                    return;
                }
                if (type == typeof(Point2d)) { var point = (Point2d)value; var guid = point.Guid; Register(value); Number(point.X); Number(point.Y); return; }
                if (type == typeof(Vector2d)) { var vector = (Vector2d)value; var guid = vector.Guid; Register(value); Number(vector.X); Number(vector.Y); return; }
                var info = new SerializationInfo(type, Converter);
                ((ISerializable)value).GetObjectData(info, Context);
                if (info.ObjectType != type || info.FullTypeName != type.FullName || info.AssemblyName != type.Assembly.FullName)
                    throw new ContractCopy.UnsupportedException("SerializationTypeChange: " + type.FullName);
                var names = new string[info.MemberCount]; var values = new object[info.MemberCount]; int i = 0;
                foreach (SerializationEntry entry in info) { names[i] = entry.Name; values[i++] = entry.Value; }
                if (!type.IsValueType) Register(value);
                Layout(type, names);
                for (i = 0; i < values.Length; i++) Value(values[i], typeof(object), depth + 1);
            }

            private void Members(object value, Type type, ContractCopy.Plan plan, int depth)
            {
                var members = plan.Members; var names = new List<string>(members.Length); var values = new List<KeyValuePair<object, Type>>(members.Length);
                foreach (var member in members)
                {
                    var item = member.Get(value);
                    if (!member.EmitDefault && Equals(item, member.Default)) continue;
                    names.Add(member.Name); values.Add(new KeyValuePair<object, Type>(item, member.Type));
                }
                if (!type.IsValueType) Register(value);
                Layout(type, names.ToArray());
                foreach (var item in values) Value(item.Key, item.Value, depth + 1);
            }

            private void Layout(Type type, string[] names)
            {
                if (!_layouts.TryGetValue(type, out var known)) _layouts.Add(type, known = new List<KeyValuePair<string[], int>>());
                foreach (var layout in known)
                    if (Same(layout.Key, names)) { VarUInt((ulong)layout.Value + 1); return; }
                VarUInt(0); VarUInt((ulong)names.Length); foreach (var name in names) String(name);
                known.Add(new KeyValuePair<string[], int>(names, _layoutCount++));
            }

            private static bool Same(string[] a, string[] b)
            {
                if (a.Length != b.Length) return false;
                for (int i = 0; i < a.Length; i++) if (!string.Equals(a[i], b[i], StringComparison.Ordinal)) return false;
                return true;
            }

            /// <summary>A coordinate system whose matrix is [V1 V2 V3 Origin] and whose inverse origin is what its constructor computes:
            /// origin, axes, flags and name. False (nothing written) for any other shape, written then by its GetObjectData.</summary>
            private bool Frame(CoordinateSystem frame, Type declared, int depth)
            {
                if (!declared.IsAssignableFrom(typeof(CoordinateSystem)) || MatrixRows == null || MatrixColumns == null || MatrixValues == null) return false;
                var info = new SerializationInfo(typeof(CoordinateSystem), Converter);
                frame.GetObjectData(info, Context);
                if (info.ObjectType != typeof(CoordinateSystem) || info.MemberCount != FrameEntries.Length) return false;
                var values = new object[FrameEntries.Length]; int i = 0;
                foreach (SerializationEntry entry in info) { if (entry.Name != FrameEntries[i]) return false; values[i++] = entry.Value; }
                var origin = values[1] as Point3d; var v1 = values[2] as Vector3d; var v2 = values[3] as Vector3d; var v3 = values[4] as Vector3d;
                var inverse = values[5] as Vector3d; var matrix = values[6] as DenseMatrix; var name = values[7];
                if (origin == null || v1 == null || v2 == null || v3 == null || origin.GetType() != typeof(Point3d) || v1.GetType() != typeof(Vector3d) ||
                    v2.GetType() != typeof(Vector3d) || v3.GetType() != typeof(Vector3d) || name != null && !(name is string)) return false;
                byte flags = 0;
                double[] data = null;
                if (matrix != null && matrix.GetType() == typeof(DenseMatrix) && !_objects.ContainsKey(matrix))
                {
                    var storage = matrix.Storage as DenseColumnMajorMatrixStorage<double>;
                    data = storage?.Data;
                    if (storage != null && storage.GetType() == typeof(DenseColumnMajorMatrixStorage<double>) && data != null && data.Length == 12 &&
                        storage.RowCount == 3 && storage.ColumnCount == 4 && matrix.RowCount == 3 && matrix.ColumnCount == 4 &&
                        (int)MatrixRows(matrix) == 3 && (int)MatrixColumns(matrix) == 4 && ReferenceEquals(MatrixValues(matrix), data) &&
                        !_objects.ContainsKey(storage) && !_objects.ContainsKey(data) &&
                        Same(data, 0, v1) && Same(data, 3, v2) && Same(data, 6, v3) && Same(data, 9, origin)) flags |= 2;
                }
                if ((flags & 2) != 0 && inverse != null && inverse.GetType() == typeof(Vector3d) && !_objects.ContainsKey(inverse) &&
                    Same(inverse, InverseOrigin(origin.X, origin.Y, origin.Z, data))) flags |= 1;
                Write(Tag.Frame); Register(frame);
                Value(origin, typeof(Point3d), depth + 1); Value(v1, typeof(Vector3d), depth + 1); Value(v2, typeof(Vector3d), depth + 1); Value(v3, typeof(Vector3d), depth + 1);
                Byte(flags);
                if ((flags & 1) != 0) { var guid = inverse.Guid; Register(inverse); } else Value(inverse, typeof(object), depth + 1);
                if ((flags & 2) != 0) { Register(matrix); Register(matrix.Storage); Register(data); } else Value(matrix, typeof(object), depth + 1);
                Value(name, typeof(object), depth + 1);
                return true;
            }

            private static bool Same(double[] data, int at, Point3d p) => Bits(data[at], p.X) && Bits(data[at + 1], p.Y) && Bits(data[at + 2], p.Z);
            private static bool Same(double[] data, int at, Vector3d v) => Bits(data[at], v.X) && Bits(data[at + 1], v.Y) && Bits(data[at + 2], v.Z);
            private static bool Same(Vector3d v, double[] xyz) => Bits(v.X, xyz[0]) && Bits(v.Y, xyz[1]) && Bits(v.Z, xyz[2]);
            private static bool Bits(double a, double b) => BitConverter.DoubleToInt64Bits(a) == BitConverter.DoubleToInt64Bits(b);
        }

        /// <summary>The inverse origin of the Geometry CoordinateSystem (SetInverseOrigin), from the origin and the matrix data
        /// (column-major: data[3c + r] is the entry [r, c]), with the same operations in the same order.</summary>
        private static double[] InverseOrigin(double x, double y, double z, double[] m) => new[]
        {
            -(m[0] * x + m[1] * y + m[2] * z),
            -(m[3] * x + m[4] * y + m[5] * z),
            -(m[6] * x + m[7] * y + m[8] * z)
        };

        #endregion

        #region Reading

        /// <summary>Reads a graph of <paramref name="length"/> bytes written by <see cref="Write"/> with <paramref name="rootType"/>.</summary>
        internal static object Read(Stream source, long length, Type rootType)
        {
            if (source == null || rootType == null) throw new ArgumentNullException();
            if (length < 5) throw new SerializationException("CorruptBinaryGraph");
            var reader = new Reader(source, length);
            try
            {
                if (reader.Int32() != Magic) throw new SerializationException("UnsupportedBinaryGraph");
                byte version = reader.Byte();
                if (version != Version && version != 1) throw new SerializationException("UnsupportedBinaryGraphVersion");
                reader.Leaves2d = version >= 2;
                var root = reader.Value(rootType, 0);
                reader.End();
                return root;
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
            catch (EndOfStreamException ex) { throw new SerializationException("CorruptBinaryGraph", ex); }
            catch (InvalidCastException ex) { throw new SerializationException("CorruptBinaryGraph", ex); }
            catch (OverflowException ex) { throw new SerializationException("CorruptBinaryGraph", ex); }
            catch (ArgumentException ex) { throw new SerializationException("CorruptBinaryGraph", ex); }
            catch (InvalidDataException ex) { throw new SerializationException("CorruptBinaryGraph", ex); }
        }

        private sealed class ReadLayout
        {
            internal string[] Names;
            internal Type Type;
            internal ContractCopy.Member[] Members;
        }

        private sealed class Reader
        {
            private readonly Stream _source;
            private readonly byte[] _buffer = new byte[1 << 16];
            private int _at, _count;
            private long _left; // bytes of the graph not yet read into the buffer
            private readonly List<object> _objects = new List<object>(1 << 12);
            private readonly List<string> _strings = new List<string>();
            private readonly List<Type> _types = new List<Type>();
            private readonly List<ReadLayout> _layouts = new List<ReadLayout>();
            private readonly Dictionary<Type, Dictionary<string, ContractCopy.Member>> _members = new Dictionary<Type, Dictionary<string, ContractCopy.Member>>();
            internal Reader(Stream source, long length) { _source = source; _left = length; }
            /// <summary>Point2d and Vector2d written as two doubles (version 2).</summary>
            internal bool Leaves2d;

            /// <summary>Bytes of the graph still to read: no valid graph declares more items than that.</summary>
            private long Remaining => _left + _count - _at;
            private void Fill(int bytes)
            {
                if (_count - _at >= bytes) return;
                if (_at != 0) { Buffer.BlockCopy(_buffer, _at, _buffer, 0, _count - _at); _count -= _at; _at = 0; }
                while (_count < bytes)
                {
                    if (_left <= 0) throw new EndOfStreamException();
                    int n = _source.Read(_buffer, _count, (int)Math.Min(_buffer.Length - _count, _left));
                    if (n <= 0) throw new EndOfStreamException();
                    _count += n; _left -= n;
                }
            }
            internal byte Byte() { Fill(1); return _buffer[_at++]; }
            private Tag Next() => (Tag)Byte();
            internal int Int32() { Fill(4); int v = BitConverter.ToInt32(_buffer, _at); _at += 4; return v; }
            private long Int64() { Fill(8); long v = BitConverter.ToInt64(_buffer, _at); _at += 8; return v; }
            private ulong VarUInt()
            {
                ulong v = 0; int shift = 0;
                while (true) { byte b = Byte(); v |= (ulong)(b & 0x7F) << shift; if (b < 0x80) return v; shift += 7; if (shift > 63) throw new SerializationException("CorruptBinaryGraph"); }
            }
            private long VarInt() { ulong v = VarUInt(); return (long)(v >> 1) ^ -(long)(v & 1); }
            private int Count()
            {
                ulong n = VarUInt();
                if (n > (ulong)Math.Min(Remaining, int.MaxValue)) throw new SerializationException("CorruptBinaryGraph");
                return (int)n;
            }
            private int Index(int count) { ulong v = VarUInt(); if (v > (ulong)count) throw new SerializationException("CorruptBinaryGraph"); return (int)v; }
            private void Raw(byte[] target)
            {
                int at = 0;
                while (at < target.Length)
                {
                    Fill(1); int n = Math.Min(target.Length - at, _count - _at);
                    Buffer.BlockCopy(_buffer, _at, target, at, n); _at += n; at += n;
                }
            }

            internal void End() { if (Remaining != 0) throw new SerializationException("CorruptBinaryGraph"); }

            private string String() => String(out _);
            private string String(out bool fresh)
            {
                int index = Index(_strings.Count);
                fresh = index == 0;
                if (index != 0) return _strings[index - 1];
                int length = Count(); Fill(Math.Min(length, _buffer.Length));
                string value;
                if (length <= _buffer.Length) { value = Text.GetString(_buffer, _at, length); _at += length; }
                else { var bytes = new byte[length]; Raw(bytes); value = Text.GetString(bytes); }
                _strings.Add(value); return value;
            }

            private Type TypeRef()
            {
                int index = Index(_types.Count);
                if (index != 0) return _types[index - 1];
                var name = String();
                if (!TypesByName.TryGetValue(name, out var type)) throw new SerializationException("UnknownBinaryGraphType: " + name);
                _types.Add(type); return type;
            }

            private double Number(Tag tag)
            {
                switch (tag)
                {
                    case ContractBinary.Tag.Zero: return 0.0;
                    case ContractBinary.Tag.NegativeZero: return BitConverter.Int64BitsToDouble(long.MinValue);
                    case ContractBinary.Tag.One: return 1.0;
                    case ContractBinary.Tag.MinusOne: return -1.0;
                    case ContractBinary.Tag.Integral: return VarInt();
                    case ContractBinary.Tag.Double: return BitConverter.Int64BitsToDouble(Int64());
                    default: throw new SerializationException("CorruptBinaryGraph");
                }
            }
            private double Number() => Number(Next());

            private static Type Concrete(Type declared) => Nullable.GetUnderlyingType(declared) ?? declared;
            private static void Assignable(Type declared, Type type)
            { if (!declared.IsAssignableFrom(type) && Nullable.GetUnderlyingType(declared) != type) throw new SerializationException("BinaryGraphTypeMismatch: " + type.FullName); }

            internal object Value(Type declared, int depth)
            {
                if (depth > MaximumDepth) throw new SerializationException("BinaryGraphTooDeep");
                var tag = Next();
                switch (tag)
                {
                    case ContractBinary.Tag.Null: return null;
                    case ContractBinary.Tag.Ref:
                        {
                            ulong id = VarUInt(); if (id >= (ulong)_objects.Count) throw new SerializationException("CorruptBinaryGraph");
                            var value = _objects[(int)id]; if (value != null) Assignable(declared, value.GetType()); return value;
                        }
                    case ContractBinary.Tag.Declared:
                        {
                            var type = Concrete(declared);
                            return Content(type, Plan(type), depth);
                        }
                    case ContractBinary.Tag.Typed:
                        {
                            var type = TypeRef(); Assignable(declared, type);
                            return Content(type, Plan(type), depth);
                        }
                    case ContractBinary.Tag.Frame: Assignable(declared, typeof(CoordinateSystem)); return Frame(depth);
                    case ContractBinary.Tag.False: return Primitive(false, declared);
                    case ContractBinary.Tag.True: return Primitive(true, declared);
                    case ContractBinary.Tag.Int32: return Primitive(checked((int)VarInt()), declared);
                    case ContractBinary.Tag.Int64: return Primitive(VarInt(), declared);
                    case ContractBinary.Tag.Double:
                    case ContractBinary.Tag.Zero:
                    case ContractBinary.Tag.NegativeZero:
                    case ContractBinary.Tag.One:
                    case ContractBinary.Tag.MinusOne:
                    case ContractBinary.Tag.Integral: return Primitive(Number(tag), declared);
                    case ContractBinary.Tag.String:
                        {
                            // A new string instance, as the serializer reads one (the table holds its characters once).
                            var text = String(out bool fresh); if (!fresh) text = new string(text.ToCharArray());
                            _objects.Add(text); return Primitive(text, declared);
                        }
                    case ContractBinary.Tag.Guid: { var bytes = new byte[16]; Raw(bytes); return Primitive(new Guid(bytes), declared); }
                    case ContractBinary.Tag.Enum:
                        {
                            var type = TypeRef(); if (!type.IsEnum) throw new SerializationException("CorruptBinaryGraph");
                            Assignable(declared, type); return Enum.ToObject(type, VarInt());
                        }
                    case ContractBinary.Tag.DeclaredEnum:
                        {
                            var type = Concrete(declared); if (!type.IsEnum) throw new SerializationException("CorruptBinaryGraph");
                            return Enum.ToObject(type, VarInt());
                        }
                    case ContractBinary.Tag.Decimal: return Primitive(new decimal(new[] { Int32(), Int32(), Int32(), Int32() }), declared);
                    case ContractBinary.Tag.Byte: return Primitive(Byte(), declared);
                    case ContractBinary.Tag.SByte: return Primitive((sbyte)Byte(), declared);
                    case ContractBinary.Tag.Int16: return Primitive(checked((short)VarInt()), declared);
                    case ContractBinary.Tag.UInt16: return Primitive(checked((ushort)VarUInt()), declared);
                    case ContractBinary.Tag.UInt32: return Primitive(checked((uint)VarUInt()), declared);
                    case ContractBinary.Tag.UInt64: return Primitive(VarUInt(), declared);
                    case ContractBinary.Tag.Single: return Primitive(BitConverter.ToSingle(BitConverter.GetBytes(Int32()), 0), declared);
                    case ContractBinary.Tag.Char: return Primitive(checked((char)VarUInt()), declared);
                    case ContractBinary.Tag.DateTime: return Primitive(DateTime.FromBinary(Int64()), declared);
                    case ContractBinary.Tag.TimeSpan: return Primitive(new TimeSpan(VarInt()), declared);
                    case ContractBinary.Tag.Uri: return Primitive(new Uri(String(), UriKind.RelativeOrAbsolute), declared);
                    default: throw new SerializationException("CorruptBinaryGraph");
                }
            }

            private static object Primitive(object value, Type declared) { Assignable(declared, value.GetType()); return value; }

            private static ContractCopy.Plan Plan(Type type)
            {
                var plan = ContractCopy.PlanOf(type);
                if (plan.Unsupported != null || plan.Kind == ContractCopy.Kind.Shared) throw new SerializationException("UnsupportedBinaryGraphType: " + type.FullName);
                return plan;
            }

            private object Content(Type type, ContractCopy.Plan plan, int depth)
            {
                switch (plan.Kind)
                {
                    case ContractCopy.Kind.Array:
                        {
                            int n = Count(); var array = Array.CreateInstance(plan.Item, n); _objects.Add(array);
                            if (array is double[] doubles) for (int i = 0; i < n; i++) doubles[i] = Number();
                            else if (array is byte[] bytes) Raw(bytes);
                            else if (array is object[] references) for (int i = 0; i < n; i++) references[i] = Value(plan.Item, depth + 1);
                            else for (int i = 0; i < n; i++) array.SetValue(Value(plan.Item, depth + 1), i);
                            return array;
                        }
                    case ContractCopy.Kind.Pair:
                        {
                            var key = Value(plan.Key, depth + 1); var item = Value(plan.Item, depth + 1);
                            return plan.Constructor.Invoke(new[] { key, item });
                        }
                    case ContractCopy.Kind.Collection:
                        {
                            var collection = plan.New(); _objects.Add(collection);
                            int n = Count();
                            for (int i = 0; i < n; i++) plan.Add(collection, Value(plan.Item, depth + 1));
                            return collection;
                        }
                    case ContractCopy.Kind.Dictionary:
                        {
                            var dictionary = plan.New(); _objects.Add(dictionary);
                            int n = Count();
                            for (int i = 0; i < n; i++)
                            {
                                var key = Value(plan.Key, depth + 1); var item = Value(plan.Item, depth + 1);
                                plan.AddPair(dictionary, key, item);
                            }
                            return dictionary;
                        }
                    case ContractCopy.Kind.Custom: return Custom(type, plan, depth);
                    case ContractCopy.Kind.Members: return Members(type, plan, depth);
                    default: throw new SerializationException("UnsupportedBinaryGraphType: " + type.FullName);
                }
            }

            private object Custom(Type type, ContractCopy.Plan plan, int depth)
            {
                if (type == typeof(Point3d))
                {
                    double x = Number(), y = Number(), z = Number(); var point = new Point3d(x, y, z); _objects.Add(point); return point;
                }
                if (type == typeof(Vector3d))
                {
                    double x = Number(), y = Number(), z = Number(); var vector = new Vector3d(x, y, z); _objects.Add(vector); return vector;
                }
                if (Leaves2d && type == typeof(Point2d)) { double x = Number(), y = Number(); var point = new Point2d(x, y); _objects.Add(point); return point; }
                if (Leaves2d && type == typeof(Vector2d)) { double x = Number(), y = Number(); var vector = new Vector2d(x, y); _objects.Add(vector); return vector; }
                object value = type.IsValueType ? null : FormatterServices.GetUninitializedObject(type);
                if (value != null) _objects.Add(value);
                var layout = Layout(type, false);
                var info = new SerializationInfo(type, Converter);
                foreach (var name in layout.Names) info.AddValue(name, Value(typeof(object), depth + 1));
                Construct(plan, ref value, info);
                Completed(value, plan);
                return value;
            }

            private object Members(Type type, ContractCopy.Plan plan, int depth)
            {
                var value = FormatterServices.GetUninitializedObject(type);
                if (!type.IsValueType) _objects.Add(value);
                Invoke(plan.OnDeserializing, value);
                var layout = Layout(type, true);
                for (int i = 0; i < layout.Members.Length; i++)
                {
                    var member = layout.Members[i]; member.Set(value, Value(member.Type, depth + 1));
                }
                Completed(value, plan);
                return value;
            }

            private ReadLayout Layout(Type type, bool members)
            {
                int index = Index(_layouts.Count);
                ReadLayout layout;
                if (index != 0)
                {
                    layout = _layouts[index - 1];
                    if (layout.Type != type) throw new SerializationException("CorruptBinaryGraph");
                    return layout;
                }
                int n = Count(); var names = new string[n];
                for (int i = 0; i < n; i++) names[i] = String();
                layout = new ReadLayout { Type = type, Names = names };
                if (members)
                {
                    // Checked once per layout: every name a member of the contract, no member twice, the required members present.
                    if (!_members.TryGetValue(type, out var byName))
                    {
                        var plan = ContractCopy.PlanOf(type);
                        if (plan.Members.Select(m => m.Name).Distinct(StringComparer.Ordinal).Count() != plan.Members.Length)
                            throw new SerializationException("UnsupportedBinaryGraphType: " + type.FullName);
                        _members.Add(type, byName = plan.Members.ToDictionary(m => m.Name, StringComparer.Ordinal));
                    }
                    layout.Members = new ContractCopy.Member[n];
                    for (int i = 0; i < n; i++)
                        if (!byName.TryGetValue(names[i], out layout.Members[i])) throw new SerializationException("UnknownBinaryGraphMember: " + type.FullName + "." + names[i]);
                    if (names.Distinct(StringComparer.Ordinal).Count() != n || byName.Values.Any(m => m.IsRequired && Array.IndexOf(names, m.Name) < 0))
                        throw new SerializationException("CorruptBinaryGraph: " + type.FullName);
                }
                _layouts.Add(layout);
                return layout;
            }

            private object Frame(int depth)
            {
                var frame = FormatterServices.GetUninitializedObject(typeof(CoordinateSystem));
                _objects.Add(frame);
                var origin = (Point3d)Value(typeof(Point3d), depth + 1); var v1 = (Vector3d)Value(typeof(Vector3d), depth + 1);
                var v2 = (Vector3d)Value(typeof(Vector3d), depth + 1); var v3 = (Vector3d)Value(typeof(Vector3d), depth + 1);
                if (origin == null || v1 == null || v2 == null || v3 == null) throw new SerializationException("CorruptBinaryGraph");
                byte flags = Byte(); if (flags > 3 || flags == 1) throw new SerializationException("CorruptBinaryGraph");
                double[] data = new[] { v1.X, v1.Y, v1.Z, v2.X, v2.Y, v2.Z, v3.X, v3.Y, v3.Z, origin.X, origin.Y, origin.Z };
                object inverse = null, matrix = null;
                if ((flags & 1) != 0) { var xyz = InverseOrigin(origin.X, origin.Y, origin.Z, data); inverse = new Vector3d(xyz[0], xyz[1], xyz[2]); _objects.Add(inverse); }
                else inverse = Value(typeof(object), depth + 1);
                if ((flags & 2) != 0) { var dense = new DenseMatrix(3, 4, data); matrix = dense; _objects.Add(dense); _objects.Add(dense.Storage); _objects.Add(data); }
                else matrix = Value(typeof(object), depth + 1);
                var name = Value(typeof(object), depth + 1);
                var info = new SerializationInfo(typeof(CoordinateSystem), Converter);
                info.AddValue("Origin", origin); info.AddValue("V1Direction", v1); info.AddValue("V2Direction", v2); info.AddValue("V3Direction", v3);
                info.AddValue("InvariantOrigin", inverse); info.AddValue("TransformationMatrix", matrix); info.AddValue("Name", name);
                var plan = ContractCopy.PlanOf(typeof(CoordinateSystem));
                Construct(plan, ref frame, info);
                Completed(frame, plan);
                return frame;
            }

            private static void Construct(ContractCopy.Plan plan, ref object value, SerializationInfo info)
            {
                try
                {
                    if (value == null) value = plan.Constructor.Invoke(new object[] { info, Context });
                    else plan.Constructor.Invoke(value, new object[] { info, Context });
                }
                catch (TargetInvocationException ex) when (ex.InnerException != null) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
            }

            private static void Completed(object value, ContractCopy.Plan plan)
            {
                if (plan.Callback) ((IDeserializationCallback)value).OnDeserialization(null);
                Invoke(plan.OnDeserialized, value);
            }

            private static void Invoke(MethodInfo[] methods, object target)
            {
                if (methods == null) return;
                foreach (var method in methods)
                {
                    try { method.Invoke(target, new object[] { Context }); }
                    catch (TargetInvocationException ex) when (ex.InnerException != null) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
                }
            }

        }

        #endregion
    }
}
