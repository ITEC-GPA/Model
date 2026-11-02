using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace GPC.Model.Core
{
    // Explicit CLR-to-persisted identities. Type names in generic/array fingerprints are data, not deployment names.
    internal static partial class HistoricalTypeNames
    {
        private static readonly Regex Pattern;
        static HistoricalTypeNames()
        {
            Pattern = Names.Count == 0 ? null : new Regex(
                "(?:" + string.Join("|", Names.Keys.OrderByDescending(n => n.Length).Select(Regex.Escape)) + @")(?=\+|\[|,|$)", RegexOptions.CultureInvariant);
        }
        internal static string For(Type type) => Rewrite(NetFramework.Running ? NetFullName(type) : type.FullName);
        internal static string Rewrite(string name) => Pattern == null ? name : Pattern.Replace(name, m => Names[m.Value]);

        // Type.FullName qualifies the generic arguments with their assemblies, and .NET Framework keeps the framework types in other
        // assemblies than .NET 5 and later, where the recorded fingerprints were computed: mscorlib for System.Private.CoreLib, System.Core
        // for HashSet<T> (StageCombinationsMap), System for Stack<T>. On .NET Framework the arguments are named as .NET names them, so the
        // same inputs give the same fingerprint on both runtimes and an analysis snapshot written on .NET validates there.
        private const string CoreLibrary = "System.Private.CoreLib, Version={0}, Culture=neutral, PublicKeyToken=7cec85d7bea7798e";
        // The framework types outside mscorlib on .NET Framework or outside the core library on .NET (the same on .NET 6, 8 and 10); the
        // other types of mscorlib are in System.Private.CoreLib. The version is that of the running framework: the canonical names leave
        // it out and the historical ones replace the one of the core library.
        private static readonly Dictionary<string, string> NetAssemblies = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["System.Collections.Generic.HashSet`1"] = CoreLibrary,
            ["System.Collections.Generic.Queue`1"] = CoreLibrary,
            ["System.Collections.Generic.LinkedList`1"] = Net("System.Collections"),
            ["System.Collections.Generic.LinkedListNode`1"] = Net("System.Collections"),
            ["System.Collections.Generic.Stack`1"] = Net("System.Collections"),
            ["System.Collections.Generic.SortedSet`1"] = Net("System.Collections"),
            ["System.Collections.Generic.SortedList`2"] = Net("System.Collections"),
            ["System.Collections.Generic.SortedDictionary`2"] = Net("System.Collections"),
            ["System.Collections.Concurrent.ConcurrentDictionary`2"] = Net("System.Collections.Concurrent"),
            ["System.Collections.ObjectModel.ObservableCollection`1"] = Net("System.ObjectModel"),
        };
        private static string Net(string assembly) => assembly + ", Version={0}, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a";
        private static string NetFullName(Type type)
        {
            if (type.HasElementType) return NetFullName(type.GetElementType()) + type.FullName.Substring(type.GetElementType().FullName.Length);
            if (!type.IsGenericType || type.IsGenericTypeDefinition) return type.FullName;
            return type.GetGenericTypeDefinition().FullName + "[" + string.Join(",", type.GetGenericArguments().Select(a => "[" + NetFullName(a) + ", " + NetAssembly(a) + "]")) + "]";
        }
        private static string NetAssembly(Type type)
        {
            while (type.HasElementType) type = type.GetElementType();
            string identity = NetAssemblies.TryGetValue(type.IsGenericType ? type.GetGenericTypeDefinition().FullName : type.FullName, out var known) ? known
                : type.Assembly == typeof(object).Assembly ? CoreLibrary : null;
            return identity == null ? type.Assembly.FullName : string.Format(CultureInfo.InvariantCulture, identity, type.Assembly.GetName().Version);
        }
    }
}
