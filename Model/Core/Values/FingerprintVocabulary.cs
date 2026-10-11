using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace GPC.Model.Core
{
    /// <summary>Names the class that declares the fingerprint contracts of the records of this assembly (architecture review R8).
    /// A record without a declared contract cannot enter a revision fingerprint: the fingerprint is refused (UnregisteredFingerprintType).</summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    public sealed class FingerprintVocabularyAttribute : Attribute
    {
        /// <param name="declaration">A class of the same assembly implementing <see cref="IFingerprintVocabulary"/> with a parameterless constructor.</param>
        public FingerprintVocabularyAttribute(Type declaration) { Declaration = declaration; }
        public Type Declaration { get; }
    }

    /// <summary>Declares the fingerprint contracts of the records of one assembly. Called once, on the first fingerprint that meets one of its types.</summary>
    public interface IFingerprintVocabulary
    {
        void Declare(FingerprintVocabulary vocabulary);
    }

    /// <summary>One persisted member of a fingerprint contract.</summary>
    public sealed class FingerprintMember
    {
        /// <param name="wireName">The persisted name. Keep it when the field is renamed: the fingerprints of the stored reports depend on it.</param>
        /// <param name="field">The CLR instance field of the record or of one of its bases; null for the field named as the wire name.</param>
        /// <param name="omitWhenNull">For a member added after fingerprints were stored: a null value does not enter, so the old fingerprints stay valid.</param>
        public FingerprintMember(string wireName, string field = null, bool omitWhenNull = false)
        {
            if (string.IsNullOrEmpty(wireName)) throw new ArgumentException("The wire name is required.", nameof(wireName));
            WireName = wireName; Field = field ?? wireName; OmitWhenNull = omitWhenNull;
        }
        public string WireName { get; }
        public string Field { get; }
        public bool OmitWhenNull { get; }
        /// <summary>A member whose field has the name of the wire name.</summary>
        public static implicit operator FingerprintMember(string wireName) => new FingerprintMember(wireName);
        /// <summary>The backing field of an auto-property, named as the compiler names it (the name used by the historical reflection).
        /// When the property is renamed, keep the wire name: <c>new FingerprintMember("&lt;Old&gt;k__BackingField", "&lt;New&gt;k__BackingField")</c>.</summary>
        public static FingerprintMember AutoProperty(string property, bool omitWhenNull = false)
            => new FingerprintMember("<" + property + ">k__BackingField", null, omitWhenNull);
    }

    /// <summary>
    /// Explicit fingerprint contracts of caller-owned records (architecture review R8). Each record declares its persisted type name and its
    /// members in fingerprint order; renaming a CLR type or field keeps the declared names, so the stored reports stay current. Every instance
    /// field of the record and of its bases must be declared once, except the [NonSerialized] ones (caches): a field added without a
    /// decision fails on the first use of the vocabulary instead of silently entering, or not entering, the fingerprints.
    /// </summary>
    public sealed class FingerprintVocabulary
    {
        private readonly Assembly _owner;
        private readonly Dictionary<Type, FingerprintContracts.Declared> _records = new Dictionary<Type, FingerprintContracts.Declared>();
        private readonly HashSet<string> _wireNames = new HashSet<string>(StringComparer.Ordinal);
        internal FingerprintVocabulary(Assembly owner) { _owner = owner; }

        /// <param name="type">A class or struct of the declaring assembly.</param>
        /// <param name="wireName">The persisted type name; keep it when the type is renamed or moved.</param>
        /// <param name="members">The persisted members in fingerprint order.</param>
        public FingerprintVocabulary Record(Type type, string wireName, params FingerprintMember[] members)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            string name = type.FullName;
            if (type.Assembly != _owner) throw new ArgumentException("A vocabulary declares only the records of its own assembly: " + name, nameof(type));
            if (type.IsEnum || type.IsPrimitive || type.IsInterface || type.IsAbstract || type.ContainsGenericParameters)
                throw new ArgumentException("Not a concrete record: " + name, nameof(type));
            // These values are written by their collection or serialization contract: a declaration would never be used.
            if (typeof(IEnumerable).IsAssignableFrom(type) || typeof(ISerializable).IsAssignableFrom(type) || typeof(Elements.Element).IsAssignableFrom(type))
                throw new ArgumentException("The fingerprint of " + name + " does not use field contracts.", nameof(type));
            if (string.IsNullOrEmpty(wireName)) throw new ArgumentException("The wire name is required: " + name, nameof(wireName));
            if (_records.ContainsKey(type)) throw new ArgumentException("Declared twice: " + name, nameof(type));
            if (!_wireNames.Add(wireName) || FingerprintContracts.IsBuiltInWireName(wireName))
                throw new ArgumentException("The wire name " + wireName + " is already used.", nameof(wireName));
            if (members == null || members.Any(m => m == null)) throw new ArgumentNullException(nameof(members));

            var fields = FingerprintContracts.InstanceFields(type);
            var mapped = new List<FingerprintContracts.Member>(); var used = new HashSet<FieldInfo>(); var wires = new HashSet<string>(StringComparer.Ordinal);
            foreach (var member in members)
            {
                if (!wires.Add(member.WireName)) throw new ArgumentException(name + ": the member " + member.WireName + " is declared twice.", nameof(members));
                // The nearest declaration wins, as for a field hidden by a derived class.
                var field = fields.FirstOrDefault(f => f.Name == member.Field)
                    ?? throw new ArgumentException(name + " has no instance field " + member.Field + " (member " + member.WireName + ").", nameof(members));
                if (field.IsNotSerialized) throw new ArgumentException(name + "." + field.Name + " is [NonSerialized]: it cannot be a member.", nameof(members));
                if (!used.Add(field)) throw new ArgumentException(name + "." + field.Name + " is mapped twice.", nameof(members));
                mapped.Add(new FingerprintContracts.Member { WireName = member.WireName, Accessor = field, OmitNull = member.OmitWhenNull });
            }
            var missing = fields.Where(f => !f.IsNotSerialized && !used.Contains(f)).Select(f => f.DeclaringType.Name + "." + f.Name).ToArray();
            if (missing.Length > 0)
                throw new ArgumentException(name + ": declare the fingerprint member of " + string.Join(", ", missing) + " or mark the field [NonSerialized].", nameof(members));
            _records.Add(type, new FingerprintContracts.Declared(wireName, mapped.ToArray()));
            return this;
        }

        internal IReadOnlyDictionary<Type, FingerprintContracts.Declared> Records => _records;
    }
}
