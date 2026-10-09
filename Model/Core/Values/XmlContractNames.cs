using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace GPC.Model.Core
{
    /// <summary>Closed migration of legacy ISerializable and enum wire identities. User text is never rewritten.</summary>
    internal static partial class XmlContractNames
    {
        private static readonly Dictionary<XName, XName> WriteNames;
        private static readonly Dictionary<string, string> WriteNamespaces;
        static XmlContractNames()
        {
            // All partial-class field initializers have run before this body, regardless of source file order.
            WriteNames = ReadNames.ToDictionary(p => p.Value, p => p.Key);
            WriteNamespaces = WriteNames.Where(p => p.Key.NamespaceName != p.Value.NamespaceName)
                .GroupBy(p => p.Key.NamespaceName)
                .ToDictionary(g => g.Key, g => g.Select(p => p.Value.NamespaceName).Distinct().Single());
        }
        internal static XmlQualifiedName Historical(XmlQualifiedName current)
        { var name = Write(current.Name, current.Namespace); return new XmlQualifiedName(name.LocalName, name.NamespaceName); }
        internal static XName Read(string local, string ns)
        { var name = XName.Get(local, ns); return ReadNames.TryGetValue(name, out var mapped) ? mapped : name; }
        internal static XName Write(string local, string ns)
        { var name = XName.Get(local, ns ?? ""); return WriteNames.TryGetValue(name, out var mapped) ? mapped : name; }
        internal static string WriteNamespace(string ns) => ns != null && WriteNamespaces.TryGetValue(ns, out var old) ? old : ns;
    }
}
