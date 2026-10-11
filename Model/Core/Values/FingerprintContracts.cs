using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;

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
        private sealed class Contract { internal string WireName; internal Member[] Members; }
        private static readonly Dictionary<Type, Contract> Contracts = new Dictionary<Type, Contract>();
        private static readonly ConcurrentDictionary<Type, string> CanonicalNames = new ConcurrentDictionary<Type, string>();
        // Historical assembly versions of the fingerprint being validated. AsyncLocal, not ThreadStatic: a fingerprint computed inside the
        // scope on another thread (parallel or asynchronous work) must still use the historical names (architecture review R7).
        private sealed class LegacyNames { internal string Model, Core; }
        private static readonly System.Threading.AsyncLocal<LegacyNames> _legacy = new System.Threading.AsyncLocal<LegacyNames>();
        static FingerprintContracts() { RegisterBuiltIns(); }
        private static void Register(string clrName, string wireName, params Member[] members)
            => Contracts.Add(typeof(FingerprintContracts).Assembly.GetType(clrName, true), new Contract { WireName = wireName, Members = members });
        private static Member Field(string declaringType, string clrName, string wireName, bool omitNull)
            => new Member { WireName = wireName, OmitNull = omitNull,
                Accessor = typeof(FingerprintContracts).Assembly.GetType(declaringType, true)
                    .GetField(clrName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                    ?? throw new InvalidOperationException("Update the explicit fingerprint mapping for " + declaringType + "." + clrName) };
        internal static string WireName(Type type)
        {
            var legacy = _legacy.Value;
            if (legacy == null && CanonicalNames.TryGetValue(type, out var cached)) return cached;
            string name = Contracts.TryGetValue(type, out var contract) ? contract.WireName : HistoricalTypeNames.For(type);
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
        internal static Member[] Members(Type type) => Contracts.TryGetValue(type, out var contract) ? contract.Members : null;
    }
}
