using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace GPC.Model.Core
{
    internal static partial class FingerprintContracts
    {
        internal sealed class Member
        {
            internal string WireName;
            internal FieldInfo Accessor;
            internal bool OmitNull;
        }
        internal sealed class Declared
        {
            internal Declared(string wireName, Member[] members) { WireName = wireName; Members = members; }
            internal readonly string WireName;
            internal readonly Member[] Members;
        }
        private const BindingFlags InstanceDeclared = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        private static readonly Dictionary<Type, Declared> Contracts = new Dictionary<Type, Declared>();
        // Fields that a built-in contract leaves out on purpose, keyed by declaring type and name, with the reason.
        private static readonly Dictionary<string, string> Excluded = new Dictionary<string, string>(StringComparer.Ordinal);
        // Contracts declared by the caller assemblies (FingerprintVocabularyAttribute), loaded on the first type met of each assembly.
        private static readonly ConcurrentDictionary<Assembly, Lazy<IReadOnlyDictionary<Type, Declared>>> Vocabularies =
            new ConcurrentDictionary<Assembly, Lazy<IReadOnlyDictionary<Type, Declared>>>();
        private static readonly IReadOnlyDictionary<Type, Declared> NoRecords = new Dictionary<Type, Declared>();
        private static readonly ConcurrentDictionary<Type, string> CanonicalNames = new ConcurrentDictionary<Type, string>();
        // Historical assembly versions of the fingerprint being validated. AsyncLocal, not ThreadStatic: a fingerprint computed inside the
        // scope on another thread (parallel or asynchronous work) must still use the historical names (architecture review R7).
        private sealed class LegacyNames { internal string Model, Core; }
        private static readonly AsyncLocal<LegacyNames> _legacy = new AsyncLocal<LegacyNames>();
        static FingerprintContracts() { RegisterBuiltIns(); }
        private static void Register(string clrName, string wireName, params Member[] members)
            => Contracts.Add(typeof(FingerprintContracts).Assembly.GetType(clrName, true), new Declared(wireName, members));
        private static FieldInfo FieldOf(string declaringType, string clrName)
            => typeof(FingerprintContracts).Assembly.GetType(declaringType, true).GetField(clrName, InstanceDeclared)
                ?? throw new InvalidOperationException("Update the explicit fingerprint mapping for " + declaringType + "." + clrName);
        private static Member Field(string declaringType, string clrName, string wireName, bool omitNull)
            => new Member { WireName = wireName, OmitNull = omitNull, Accessor = FieldOf(declaringType, clrName) };
        private static void Exclude(string declaringType, string clrName, string reason)
            => Excluded.Add(Key(FieldOf(declaringType, clrName)), reason);
        private static string Key(FieldInfo field) => field.DeclaringType.FullName + "/" + field.Name;

        internal static string WireName(Type type)
        {
            var legacy = _legacy.Value;
            if (legacy == null && CanonicalNames.TryGetValue(type, out var cached)) return cached;
            string name = Find(type)?.WireName ?? HistoricalTypeNames.For(type);
            if (legacy != null)
            {
                name = Regex.Replace(name, @"GPCModel, Version=[^,\]]+", "GPCModel, Version=" + legacy.Model);
                return Regex.Replace(name, @"System.Private.CoreLib, Version=[^,\]]+", "System.Private.CoreLib, Version=" + legacy.Core);
            }
            // Generic Type.FullName embeds assembly-qualified arguments. Deployment versions are not physical inputs.
            return CanonicalNames.GetOrAdd(type, _ => name.IndexOf(", Version=", StringComparison.Ordinal) < 0 ? name
                : Regex.Replace(name, @", Version=[^,\]]+, Culture=[^,\]]+, PublicKeyToken=[^\]]+", ""));
        }
        internal static IDisposable Legacy(string modelVersion, string coreVersion) => new LegacyScope(modelVersion, coreVersion);
        private sealed class LegacyScope : IDisposable
        {
            private readonly LegacyNames _previous = _legacy.Value;
            // A null model version means no historical names, as before.
            internal LegacyScope(string model, string core) { _legacy.Value = model == null ? null : new LegacyNames { Model = model, Core = core }; }
            public void Dispose() { _legacy.Value = _previous; }
        }
        internal static Member[] Members(Type type) => Find(type)?.Members;

        private static Declared Find(Type type)
        {
            if (Contracts.TryGetValue(type, out var contract)) return contract;
            var records = Vocabularies.GetOrAdd(type.Assembly,
                a => new Lazy<IReadOnlyDictionary<Type, Declared>>(() => Load(a), LazyThreadSafetyMode.ExecutionAndPublication)).Value;
            return records.TryGetValue(type, out contract) ? contract : null;
        }
        private static IReadOnlyDictionary<Type, Declared> Load(Assembly assembly)
        {
            var attribute = (FingerprintVocabularyAttribute)Attribute.GetCustomAttribute(assembly, typeof(FingerprintVocabularyAttribute));
            if (attribute == null) return NoRecords;
            var declaration = attribute.Declaration;
            if (declaration == null || declaration.Assembly != assembly || !typeof(IFingerprintVocabulary).IsAssignableFrom(declaration))
                throw new InvalidOperationException("FingerprintVocabulary of " + assembly.GetName().Name
                    + ": the declaration must be a class of the same assembly implementing IFingerprintVocabulary.");
            var vocabulary = new FingerprintVocabulary(assembly);
            ((IFingerprintVocabulary)Activator.CreateInstance(declaration, true)).Declare(vocabulary);
            return vocabulary.Records;
        }

        /// <summary>The instance fields from the most derived class to the base, each class in ordinal order: the order of the historical reflection.</summary>
        internal static FieldInfo[] InstanceFields(Type type)
        {
            var fields = new List<FieldInfo>();
            for (Type current = type; current != null; current = current.BaseType)
                fields.AddRange(current.GetFields(InstanceDeclared).OrderBy(f => f.Name, StringComparer.Ordinal));
            return fields.ToArray();
        }
        internal static bool IsBuiltInWireName(string wireName) => Contracts.Values.Any(c => c.WireName == wireName);
        /// <summary>Fields of the built-in contracts neither declared as members nor excluded with a reason. The coverage test requires none.</summary>
        internal static IEnumerable<string> Uncovered()
        {
            foreach (var pair in Contracts)
            {
                var mapped = new HashSet<string>(pair.Value.Members.Select(m => Key(m.Accessor)), StringComparer.Ordinal);
                foreach (var field in InstanceFields(pair.Key))
                    if (!field.IsNotSerialized && !mapped.Contains(Key(field)) && !Excluded.ContainsKey(Key(field)))
                        yield return pair.Key.FullName + "." + field.Name;
            }
        }

        internal static bool Strict => AppContext.TryGetSwitch(FingerprintVocabulary.StrictSwitch, out bool on) && on;
        /// <summary>Identity and serialization-version entries of GetObjectData, which are never inputs. Before the architecture review (R8)
        /// the rule was only the name: "Guid" or any name ending in "Version". An unlisted "...Version" entry still follows that rule,
        /// so the fingerprints do not change, but the strict mode refuses it: a real input named so would be left out without a word.</summary>
        internal static bool SkipsEntry(Type type, string name)
        {
            if (IdentityEntries.Contains(name)) return true;
            if (!name.EndsWith("Version", StringComparison.Ordinal)) return false;
            if (Strict) throw new InvalidOperationException("UndeclaredFingerprintEntry: " + type.FullName + " writes \"" + name
                + "\". Add it to the identity entries of FingerprintContracts if it is a serialization version; an input needs a name not ending in Version.");
            return true;
        }
    }
}
