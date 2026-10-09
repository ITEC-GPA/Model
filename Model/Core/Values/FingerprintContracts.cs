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
        [ThreadStatic] private static string _legacyModel;
        [ThreadStatic] private static string _legacyCore;
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
            if (_legacyModel == null && CanonicalNames.TryGetValue(type, out var cached)) return cached;
            string name = Contracts.TryGetValue(type, out var contract) ? contract.WireName : HistoricalTypeNames.For(type);
            if (_legacyModel != null)
            {
                name = Regex.Replace(name, @"GPCModel, Version=[^,\]]+", "GPCModel, Version=" + _legacyModel);
                return Regex.Replace(name, @"System.Private.CoreLib, Version=[^,\]]+", "System.Private.CoreLib, Version=" + _legacyCore);
            }
            // Generic Type.FullName embeds assembly-qualified arguments. Deployment versions are not physical inputs.
            return CanonicalNames.GetOrAdd(type, _ => name.IndexOf(", Version=", StringComparison.Ordinal) < 0 ? name
                : Regex.Replace(name, @", Version=[^,\]]+, Culture=[^,\]]+, PublicKeyToken=[^\]]+", ""));
        }
        internal static IDisposable Legacy(string modelVersion, string coreVersion) => new LegacyScope(modelVersion, coreVersion);
        private sealed class LegacyScope : IDisposable
        {
            private readonly string _model = _legacyModel, _core = _legacyCore;
            internal LegacyScope(string model, string core) { _legacyModel = model; _legacyCore = core; }
            public void Dispose() { _legacyModel = _model; _legacyCore = _core; }
        }
        internal static Member[] Members(Type type) => Contracts.TryGetValue(type, out var contract) ? contract.Members : null;
    }
}
