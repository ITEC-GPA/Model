using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;

namespace GPC.Model.Core
{
    /// <summary>
    /// In-memory copy of a graph of the vocabulary, equal to writing it with the data contracts of <see cref="ModelValues"/>
    /// (references preserved) and reading it back, without the XML: DataContractSerializer spends about a microsecond on every
    /// object and value each way, seconds for a model of some ten thousand elements. It does what the serializer does, as
    /// checked on .NET Framework 4.8 and .NET 8:
    /// - ISerializable: GetObjectData, an uninitialized object (its values may refer back to it), the values copied in order, the
    ///   deserialization constructor run on that object (values with their runtime types, context All), then
    ///   IDeserializationCallback with a null sender and the OnDeserialized methods;
    /// - DataContract and Serializable: an uninitialized object, OnDeserializing, the members from the base class, each class by
    ///   Order and then name (a member whose EmitDefaultValue is false is not set when it holds the default), then
    ///   IDeserializationCallback and OnDeserialized;
    /// - collections: the public parameterless constructor and the most derived public Add, in the order of enumeration (a
    ///   dictionary gets the default comparer);
    /// - arrays element by element; strings, primitives, enums and the other immutable values are shared.
    /// A shape this class does not reproduce (a runtime type outside the vocabulary, a type changed by GetObjectData, writer
    /// callbacks, get-only members, a missing deserialization constructor) raises <see cref="UnsupportedException"/>: the caller
    /// then copies through the XML, which succeeds or fails as before.
    /// </summary>
    internal sealed class ContractCopy
    {
        internal sealed class UnsupportedException : Exception
        {
            internal UnsupportedException(string message) : base(message) { }
        }

        private static readonly StreamingContext Context = new StreamingContext(StreamingContextStates.All);
        private static readonly FormatterConverter Converter = new FormatterConverter();
        private static readonly HashSet<Type> Vocabulary = new HashSet<Type>(ModelValues.DataContracts);
        // Values of the built-in contracts of the serializer, valid wherever an object is declared.
        private static readonly HashSet<Type> Primitives = new HashSet<Type> { typeof(bool), typeof(char), typeof(sbyte), typeof(byte), typeof(short),
            typeof(ushort), typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(float), typeof(double), typeof(decimal), typeof(string),
            typeof(DateTime), typeof(TimeSpan), typeof(Guid), typeof(Uri), typeof(byte[]) };
        private static readonly ConcurrentDictionary<Type, Plan> Plans = new ConcurrentDictionary<Type, Plan>();
        private static readonly Func<Type, Plan> NewPlan = type =>
        {
            var plan = Create(type);
            plan.Known = Vocabulary.Contains(type) || Primitives.Contains(type);
            return plan;
        };

        private readonly Dictionary<object, object> _copies;

        private ContractCopy(int capacity) { _copies = new Dictionary<object, object>(capacity, ReferenceComparer<object>.Instance); }

        /// <summary>The copy of <paramref name="value"/>, written as <paramref name="declared"/>.</summary>
        internal static object Copy(object value, Type declared, int capacity = 0) => new ContractCopy(capacity).Value(value, declared);

        /// <summary>How a value of <paramref name="type"/> is copied, or why it goes through the XML (the tests compare it with the
        /// contract of the serializer).</summary>
        internal static string KindOf(Type type)
        {
            var plan = PlanOf(type);
            return plan.Unsupported ?? plan.Kind.ToString();
        }

        #region Plans

        private enum Kind { Shared, Array, Dictionary, Collection, Custom, Members, Pair }

        private sealed class Member
        {
            internal FieldInfo Field;
            internal PropertyInfo Property;
            internal Type Type;
            internal object Default;
            internal bool EmitDefault;
            internal Func<object, object> Get;
            internal Action<object, object> Set;
        }

        private sealed class Plan
        {
            internal Kind Kind;
            /// <summary>In the vocabulary or a built-in value: valid where another type is declared.</summary>
            internal bool Known;
            internal string Unsupported;
            internal Type Item, Key;
            internal bool SharedItems;
            internal ConstructorInfo Constructor;
            internal Func<object, object> LeafCopy;
            internal Action<ContractCopy, object, object> Initialize;
            internal Func<ContractCopy, object, object> Factory;
            internal Func<object> New;
            internal Action<object, object> Add;
            internal Action<object, object, object> AddPair;
            internal Action<ContractCopy, object, object, Plan> CopyPairs;
            internal Member[] Members;
            internal MethodInfo[] OnDeserializing, OnDeserialized;
            internal bool Callback;
        }

        private static Plan PlanOf(Type type) => Plans.GetOrAdd(type, NewPlan);

        private static Plan Unsupported(Type type, string reason) => new Plan { Unsupported = reason + ": " + type.FullName };

        private static bool IsShared(Type type) => type.IsEnum || type != typeof(byte[]) && Primitives.Contains(type);

        private static Plan Create(Type type)
        {
            if (IsShared(type)) return new Plan { Kind = Kind.Shared };
            if (type.IsAbstract || type.IsInterface) return Unsupported(type, "NoInstances");
            if (type.IsArray)
            {
                var element = type.GetElementType();
                if (type != element.MakeArrayType()) return Unsupported(type, "MultidimensionalArray");
                return new Plan { Kind = Kind.Array, Item = element, SharedItems = IsShared(element) };
            }
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var arguments = type.GetGenericArguments();
                return new Plan { Kind = Kind.Pair, Key = arguments[0], Item = arguments[1], Constructor = type.GetConstructor(arguments) };
            }
            if (Callbacks(type, typeof(OnSerializingAttribute)).Length != 0 || Callbacks(type, typeof(OnSerializedAttribute)).Length != 0)
                return Unsupported(type, "WriterCallbacks");
            if (typeof(IObjectReference).IsAssignableFrom(type) || typeof(IExtensibleDataObject).IsAssignableFrom(type))
                return Unsupported(type, "ObjectReferenceOrExtensionData");
            bool contract = type.IsDefined(typeof(DataContractAttribute), false);
            if (!contract && typeof(IEnumerable).IsAssignableFrom(type))
            {
                var collection = CollectionPlan(type);
                if (collection != null && (Callbacks(type, typeof(OnDeserializingAttribute)).Length != 0 || Callbacks(type, typeof(OnDeserializedAttribute)).Length != 0))
                    return Unsupported(type, "CollectionCallbacks");
                if (collection != null) return collection;
                // Without a usable Add the serializer falls back to the class contract of a serializable type (e.g. Polygon3d).
                if (!type.IsSerializable) return Unsupported(type, "InvalidCollection");
            }
            Plan plan;
            if (!contract && typeof(ISerializable).IsAssignableFrom(type))
            {
                if (!type.IsSerializable) return Unsupported(type, "NotSerializable");
                var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                    new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);
                if (constructor == null) return Unsupported(type, "MissingDeserializationConstructor");
                if (Callbacks(type, typeof(OnDeserializingAttribute)).Length != 0) return Unsupported(type, "OnDeserializingOfISerializable");
                plan = new Plan { Kind = Kind.Custom, Constructor = constructor };
                // These sealed leaf contracts read only XYZ and regenerate their geometry GUID.
                // Keeping them in the graph map preserves aliases without allocating two
                // SerializationInfo dictionaries per point/vector. No subclass can opt into this path.
                if (type == typeof(GPC.Geometry.Point3d)) plan.LeafCopy = value =>
                {
                    var point = (GPC.Geometry.Point3d)value; var guid = point.Guid;
                    return new GPC.Geometry.Point3d(point.X, point.Y, point.Z);
                };
                if (type == typeof(GPC.Geometry.Vector3d)) plan.LeafCopy = value =>
                {
                    var vector = (GPC.Geometry.Vector3d)value; var guid = vector.Guid;
                    return new GPC.Geometry.Vector3d(vector.X, vector.Y, vector.Z);
                };
                if (type == typeof(Elements.NodeElement)) plan.Initialize = (context, source, target) =>
                    ((Elements.NodeElement)target).CopyContractFrom((Elements.NodeElement)source, context);
                if (type == typeof(Elements.AreaElement)) plan.Initialize = (context, source, target) =>
                    ((Elements.AreaElement)target).CopyContractFrom((Elements.AreaElement)source, context);
                if (type == typeof(GPC.Geometry.CoordinateSystem)) plan.Initialize = CoordinateContract();
            }
            else if (contract || type.IsSerializable)
            {
                var members = new List<Member>();
                for (var t = type; t != typeof(object) && t != typeof(ValueType); t = t.BaseType)
                {
                    if (contract ? !t.IsDefined(typeof(DataContractAttribute), false) : !t.IsSerializable)
                        return Unsupported(type, "UnserializableBase " + t.FullName);
                    var declared = contract ? ContractMembers(t) : FieldMembers(t);
                    if (declared == null) return Unsupported(type, "GetOnlyMember");
                    members.InsertRange(0, declared);
                }
                plan = new Plan { Kind = Kind.Members, Members = members.ToArray(), OnDeserializing = Callbacks(type, typeof(OnDeserializingAttribute)) };
            }
            else return Unsupported(type, "NotSerializable");
            plan.OnDeserialized = Callbacks(type, typeof(OnDeserializedAttribute));
            plan.Callback = typeof(IDeserializationCallback).IsAssignableFrom(type);
            if (plan.Kind == Kind.Members) plan.Initialize = MemberContract(plan.Members);
            if (type == typeof(MathNet.Numerics.LinearAlgebra.Storage.DenseColumnMajorMatrixStorage<double>))
                plan.Factory = (context, value) =>
                {
                    var storage = (MathNet.Numerics.LinearAlgebra.Storage.DenseColumnMajorMatrixStorage<double>)value;
                    if (storage.RowCount <= 0 || storage.ColumnCount <= 0 || storage.Data == null || (long)storage.RowCount * storage.ColumnCount != storage.Data.Length) return null;
                    return new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(storage.RowCount, storage.ColumnCount, context.Child(storage.Data)).Storage;
                };
            if (type == typeof(MathNet.Numerics.LinearAlgebra.Double.DenseMatrix))
            {
                // MathNet 5 stores the dimensions and data twice. Use its constructor only
                // when both representations agree; malformed/other layouts retain the generic copy.
                var fields = plan.Members.ToDictionary(m => m.Field?.Name ?? m.Property.Name);
                if (fields.Count == 6 && fields.TryGetValue("_rowCount", out var rows) && fields.TryGetValue("_columnCount", out var columns) && fields.TryGetValue("_values", out var data))
                    plan.Factory = (context, value) =>
                    {
                        var matrix = (MathNet.Numerics.LinearAlgebra.Double.DenseMatrix)value;
                        var storage = matrix.Storage as MathNet.Numerics.LinearAlgebra.Storage.DenseColumnMajorMatrixStorage<double>;
                        if (storage == null || matrix.RowCount != storage.RowCount || matrix.ColumnCount != storage.ColumnCount ||
                            (int)rows.Get(value) != matrix.RowCount || (int)columns.Get(value) != matrix.ColumnCount || !ReferenceEquals(data.Get(value), storage.Data)) return null;
                        return new MathNet.Numerics.LinearAlgebra.Double.DenseMatrix(context.Child(storage));
                    };
            }
            return plan;
        }

        /// <summary>Dictionaries, lists and enumerables with a public parameterless constructor and Add; null for other types.</summary>
        private static Plan CollectionPlan(Type type)
        {
            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null) return null;
            var interfaces = type.GetInterfaces().Where(i => i.IsGenericType).ToArray();
            var dictionary = interfaces.FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
            var enumerable = interfaces.FirstOrDefault(i => i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            var arguments = dictionary != null ? dictionary.GetGenericArguments() : enumerable?.GetGenericArguments();
            if (arguments == null) return null;
            var add = type.GetMethod("Add", BindingFlags.Instance | BindingFlags.Public, null, arguments, null);
            if (add == null) return null;
            var target = Expression.Parameter(typeof(object));
            var values = arguments.Select(a => Expression.Parameter(typeof(object))).ToArray();
            var call = Expression.Call(Expression.Convert(target, add.DeclaringType), add, values.Select((v, i) => Expression.Convert(v, arguments[i])));
            var plan = new Plan { New = Expression.Lambda<Func<object>>(Expression.Convert(Expression.New(constructor), typeof(object))).Compile() };
            if (dictionary == null)
            {
                plan.Kind = Kind.Collection; plan.Item = arguments[0];
                plan.Add = Expression.Lambda<Action<object, object>>(call, target, values[0]).Compile();
                return plan;
            }
            plan.Kind = Kind.Dictionary; plan.Key = arguments[0]; plan.Item = arguments[1];
            plan.AddPair = Expression.Lambda<Action<object, object, object>>(call, target, values[0], values[1]).Compile();
            plan.CopyPairs = (Action<ContractCopy, object, object, Plan>)Delegate.CreateDelegate(typeof(Action<ContractCopy, object, object, Plan>),
                typeof(ContractCopy).GetMethod(nameof(Pairs), BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(arguments));
            return plan;
        }

        /// <summary>The data members declared by <paramref name="type"/>, by Order and then name; null if a property cannot be set.</summary>
        private static Member[] ContractMembers(Type type)
        {
            const BindingFlags declared = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            var members = new List<KeyValuePair<DataMemberAttribute, Member>>();
            foreach (var field in type.GetFields(declared))
            {
                var attribute = field.GetCustomAttribute<DataMemberAttribute>(false);
                if (attribute != null) members.Add(new KeyValuePair<DataMemberAttribute, Member>(attribute, NewMember(field, null, field.FieldType, attribute)));
            }
            foreach (var property in type.GetProperties(declared))
            {
                var attribute = property.GetCustomAttribute<DataMemberAttribute>(false);
                if (attribute == null) continue;
                if (property.GetGetMethod(true) == null || property.GetSetMethod(true) == null || property.GetIndexParameters().Length != 0) return null;
                members.Add(new KeyValuePair<DataMemberAttribute, Member>(attribute, NewMember(null, property, property.PropertyType, attribute)));
            }
            return members.OrderBy(m => m.Key.Order).ThenBy(m => m.Key.Name ?? (m.Value.Field?.Name ?? m.Value.Property.Name), StringComparer.Ordinal)
                .Select(m => m.Value).ToArray();
        }

        /// <summary>The serializable instance fields declared by <paramref name="type"/>, by name.</summary>
        private static Member[] FieldMembers(Type type) => type
            .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            .Where(f => !f.IsNotSerialized).OrderBy(f => f.Name, StringComparer.Ordinal)
            .Select(f => NewMember(f, null, f.FieldType, null)).ToArray();

        private static Member NewMember(FieldInfo field, PropertyInfo property, Type type, DataMemberAttribute attribute)
        {
            var target = Expression.Parameter(typeof(object));
            var value = Expression.Parameter(typeof(object));
            var instance = Expression.Convert(target, field?.DeclaringType ?? property.DeclaringType);
            Expression access = field != null ? (Expression)Expression.Field(instance, field) : Expression.Property(instance, property);
            var member = new Member
            {
                Field = field, Property = property, Type = type, EmitDefault = attribute == null || attribute.EmitDefaultValue,
                Default = type.IsValueType && Nullable.GetUnderlyingType(type) == null ? Activator.CreateInstance(type) : null,
                Get = Expression.Lambda<Func<object, object>>(Expression.Convert(access, typeof(object)), target).Compile()
            };
            // Reflection is required for readonly fields and boxed value types. Their serializer
            // contract allows initialization; assigning through an unboxed copy would lose it.
            if (field?.IsInitOnly == true || (field?.DeclaringType ?? property.DeclaringType).IsValueType)
                member.Set = (o, v) => { if (field != null) field.SetValue(o, v); else property.SetValue(o, v); };
            else
                member.Set = Expression.Lambda<Action<object, object>>(Expression.Assign(access, Expression.Convert(value, type)), target, value).Compile();
            return member;
        }

        private static Action<ContractCopy, object, object> CoordinateContract()
        {
            // Exact fields read/written by the pinned Geometry CoordinateSystem contract.
            // Its public constructor normalizes axes and would change the archived values;
            // its copy constructor also discards V3. Neither is a valid contract copy.
            var type = typeof(GPC.Geometry.CoordinateSystem);
            var context = Expression.Parameter(typeof(ContractCopy));
            var source = Expression.Parameter(typeof(object));
            var target = Expression.Parameter(typeof(object));
            var from = Expression.Convert(source, type); var to = Expression.Convert(target, type);
            var expressions = new List<Expression>
            {
                // The writer requests the source GUID; the reader deliberately regenerates it.
                Expression.Property(from, nameof(GPC.Geometry.CoordinateSystem.Guid))
            };
            var copy = typeof(ContractCopy).GetMethod(nameof(Value), BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (string name in new[] { "_origin", "_v1", "_v2", "_v3", "_InvOrigin", "_trfMatrix", "_name" })
            {
                var field = type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
                if (field == null) throw new InvalidOperationException("Geometry coordinate contract changed: " + name);
                expressions.Add(Expression.Assign(Expression.Field(to, field), Expression.Convert(
                    Expression.Call(context, copy, Expression.Convert(Expression.Field(from, field), typeof(object)), Expression.Constant(typeof(object))), field.FieldType)));
            }
            expressions.Add(Expression.Call(to, type.GetMethod("UpdateComponents", BindingFlags.Instance | BindingFlags.NonPublic)));
            return Expression.Lambda<Action<ContractCopy, object, object>>(Expression.Block(expressions), context, source, target).Compile();
        }

        private static Action<ContractCopy, object, object> MemberContract(Member[] members)
        {
            var context = Expression.Parameter(typeof(ContractCopy));
            var source = Expression.Parameter(typeof(object));
            var target = Expression.Parameter(typeof(object));
            var copy = typeof(ContractCopy).GetMethod(nameof(Value), BindingFlags.Instance | BindingFlags.NonPublic);
            var statements = new List<Expression>();
            foreach (var member in members)
            {
                var owner = member.Field?.DeclaringType ?? member.Property.DeclaringType;
                Expression Access(ParameterExpression instance) => member.Field != null
                    ? (Expression)Expression.Field(Expression.Convert(instance, owner), member.Field)
                    : Expression.Property(Expression.Convert(instance, owner), member.Property);
                var read = Access(source);
                var shared = IsShared(Nullable.GetUnderlyingType(member.Type) ?? member.Type);
                Expression value = shared ? read : Expression.Convert(Expression.Call(context, copy,
                    Expression.Convert(read, typeof(object)), Expression.Constant(member.Type)), member.Type);
                Expression write = member.Field?.IsInitOnly == true || owner.IsValueType
                    ? (Expression)Expression.Invoke(Expression.Constant(member.Set), target, Expression.Convert(value, typeof(object)))
                    : Expression.Assign(Access(target), value);
                if (!member.EmitDefault)
                    write = Expression.IfThen(Expression.Not(Expression.Call(typeof(object).GetMethod(nameof(Equals), new[] { typeof(object), typeof(object) }),
                        Expression.Convert(read, typeof(object)), Expression.Constant(member.Default, typeof(object)))), write);
                statements.Add(write);
            }
            statements.Add(Expression.Empty());
            return Expression.Lambda<Action<ContractCopy, object, object>>(Expression.Block(statements), context, source, target).Compile();
        }

        /// <summary>The methods marked with <paramref name="attribute"/>, from the base class.</summary>
        private static MethodInfo[] Callbacks(Type type, Type attribute)
        {
            var methods = new List<MethodInfo>();
            for (var t = type; t != null && t != typeof(object); t = t.BaseType)
                methods.InsertRange(0, t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    .Where(m => m.IsDefined(attribute, false)));
            return methods.ToArray();
        }

        #endregion

        #region Copy

        // Typed children of the explicitly implemented built-in contracts. They still pass
        // through vocabulary validation and the same reference map as the generic contracts.
        internal T Child<T>(T source) => (T)Value(source, typeof(T));

        private object Value(object value, Type declared)
        {
            if (value == null) return null;
            var type = value.GetType();
            var plan = PlanOf(type);
            if (!plan.Known && type != declared && type != Nullable.GetUnderlyingType(declared))
                throw new UnsupportedException("UnregisteredDomainValue: " + type.FullName);
            if (plan.Unsupported != null) throw new UnsupportedException(plan.Unsupported);
            if (plan.Kind == Kind.Shared) return value;
            if (!type.IsValueType && _copies.TryGetValue(value, out var done)) return done;
            switch (plan.Kind)
            {
                case Kind.Array: return Array((Array)value, plan);
                case Kind.Pair: return Pair(value, type, plan);
                case Kind.Dictionary:
                case Kind.Collection: return Collection(value, plan);
                case Kind.Custom: return Custom(value, type, plan);
                default: return Members(value, type, plan);
            }
        }

        private Array Array(Array source, Plan plan)
        {
            var copy = System.Array.CreateInstance(plan.Item, source.Length);
            _copies.Add(source, copy);
            if (plan.SharedItems) System.Array.Copy(source, copy, source.Length);
            else if (source is object[] references)
            {
                var target = (object[])copy;
                for (int i = 0; i < references.Length; i++) target[i] = Value(references[i], plan.Item);
            }
            else for (int i = 0; i < source.Length; i++) copy.SetValue(Value(source.GetValue(i), plan.Item), i);
            return copy;
        }

        private object Pair(object source, Type type, Plan plan)
        {
            var key = Value(type.GetProperty("Key").GetValue(source), plan.Key);
            var value = Value(type.GetProperty("Value").GetValue(source), plan.Item);
            return plan.Constructor.Invoke(new[] { key, value });
        }

        private object Collection(object source, Plan plan)
        {
            var copy = plan.New();
            _copies.Add(source, copy);
            if (plan.Kind == Kind.Dictionary) plan.CopyPairs(this, source, copy, plan);
            else foreach (var item in (IEnumerable)source) plan.Add(copy, Value(item, plan.Item));
            return copy;
        }

        private void Pairs<TKey, TValue>(object source, object copy, Plan plan)
        {
            foreach (var pair in (IEnumerable<KeyValuePair<TKey, TValue>>)source)
                plan.AddPair(copy, Value(pair.Key, typeof(TKey)), Value(pair.Value, typeof(TValue)));
        }

        private object Custom(object source, Type type, Plan plan)
        {
            if (plan.LeafCopy != null)
            {
                var leaf = plan.LeafCopy(source);
                _copies.Add(source, leaf);
                return leaf;
            }
            if (plan.Initialize != null)
            {
                var target = FormatterServices.GetUninitializedObject(type);
                _copies.Add(source, target);
                plan.Initialize(this, source, target);
                Completed(target, plan);
                return target;
            }
            var info = new SerializationInfo(type, Converter);
            ((ISerializable)source).GetObjectData(info, Context);
            if (info.ObjectType != type || info.FullTypeName != type.FullName || info.AssemblyName != type.Assembly.FullName)
                throw new UnsupportedException("SerializationTypeChange: " + type.FullName);
            // As the reader: the object exists, uninitialized, while its values are read (they may refer back to it), and then the
            // deserialization constructor runs on it.
            var copy = FormatterServices.GetUninitializedObject(type);
            if (!type.IsValueType) _copies.Add(source, copy);
            var values = new object[info.MemberCount];
            // The reader adds the values with their runtime types: when every value is shared and so typed, they are those written.
            bool written = true;
            int i = 0;
            foreach (SerializationEntry entry in info)
            {
                var value = Value(entry.Value, typeof(object));
                values[i++] = value;
                written = written && ReferenceEquals(value, entry.Value) && entry.ObjectType == (value?.GetType() ?? typeof(object));
            }
            if (!written)
            {
                var read = new SerializationInfo(type, Converter);
                i = 0;
                foreach (SerializationEntry entry in info) read.AddValue(entry.Name, values[i++]);
                info = read;
            }
            try
            {
                if (type.IsValueType) copy = plan.Constructor.Invoke(new object[] { info, Context });
                else plan.Constructor.Invoke(copy, new object[] { info, Context });
            }
            catch (TargetInvocationException ex) when (ex.InnerException != null) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
            Completed(copy, plan);
            return copy;
        }

        private object Members(object source, Type type, Plan plan)
        {
            if (plan.Factory != null)
            {
                var created = plan.Factory(this, source);
                if (created != null) { _copies.Add(source, created); Completed(created, plan); return created; }
            }
            var copy = FormatterServices.GetUninitializedObject(type);
            if (!type.IsValueType) _copies.Add(source, copy);
            Call(plan.OnDeserializing, copy);
            try { plan.Initialize(this, source, copy); }
            catch (TargetInvocationException ex) when (ex.InnerException != null) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
            Completed(copy, plan);
            return copy;
        }

        private static void Completed(object copy, Plan plan)
        {
            if (plan.Callback) ((IDeserializationCallback)copy).OnDeserialization(null);
            Call(plan.OnDeserialized, copy);
        }

        /// <summary>Exceptions of the callbacks surface as when they are called directly.</summary>
        private static void Call(MethodInfo[] methods, object target)
        {
            foreach (var method in methods)
            {
                try { method.Invoke(target, new object[] { Context }); }
                catch (TargetInvocationException ex) when (ex.InnerException != null) { ExceptionDispatchInfo.Capture(ex.InnerException).Throw(); throw; }
            }
        }

        #endregion
    }
}
