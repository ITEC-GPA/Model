using System;
using System.Xml;

namespace GPC.Model.Core
{
    internal sealed class ContractXmlReader : XmlReader
    {
        private const string Xsi = "http://www.w3.org/2001/XMLSchema-instance";
        private readonly XmlReader _inner;
        private string _typePrefix, _typeNamespace;
        private bool _typeValue;
        internal ContractXmlReader(XmlReader inner) { _inner = inner; }
        private bool Element => NodeType == XmlNodeType.Element || NodeType == XmlNodeType.EndElement;
        private string TypeValue(string value)
        {
            if (value == null) return null;
            int colon = value.IndexOf(':');
            string ns = _inner.LookupNamespace(colon < 0 ? "" : value.Substring(0, colon));
            if (ns == null) return value; // Let the serializer reject unknown prefixes.
            string local = colon < 0 ? value : value.Substring(colon + 1);
            var name = XmlContractNames.Read(local, ns);
            if (name.LocalName == local && name.NamespaceName == ns) return value;
            int suffix = 0;
            do { _typePrefix = "gpcMigration" + suffix++; } while (_inner.LookupNamespace(_typePrefix) != null);
            _typeNamespace = name.NamespaceName;
            return _typePrefix + ":" + name.LocalName;
        }
        public override string LocalName => Element ? XmlContractNames.Read(_inner.LocalName, _inner.NamespaceURI).LocalName : _inner.LocalName;
        public override string NamespaceURI => Element ? XmlContractNames.Read(_inner.LocalName, _inner.NamespaceURI).NamespaceName : _inner.NamespaceURI;
        public override string Name => Prefix.Length == 0 ? LocalName : Prefix + ":" + LocalName;
        public override string Value => _typeValue || NodeType == XmlNodeType.Attribute && _inner.LocalName == "type" && _inner.NamespaceURI == Xsi ? TypeValue(_inner.Value) : _inner.Value;
        public override string GetAttribute(string name, string namespaceURI) => name == "type" && namespaceURI == Xsi ? TypeValue(_inner.GetAttribute(name, namespaceURI)) : _inner.GetAttribute(name, namespaceURI);
        public override string GetAttribute(string name)
        { int colon = name.IndexOf(':'); return colon >= 0 && name.Substring(colon + 1) == "type" && _inner.LookupNamespace(name.Substring(0, colon)) == Xsi ? TypeValue(_inner.GetAttribute(name)) : _inner.GetAttribute(name); }
        public override string GetAttribute(int i)
        {
            string value = _inner.GetAttribute(i);
            string previous = _inner.NodeType == XmlNodeType.Attribute ? _inner.Name : null;
            _inner.MoveToAttribute(i);
            bool type = _inner.LocalName == "type" && _inner.NamespaceURI == Xsi;
            if (type) value = TypeValue(value);
            if (previous == null) _inner.MoveToElement(); else _inner.MoveToAttribute(previous);
            return value;
        }
        public override string LookupNamespace(string prefix) => prefix == _typePrefix ? _typeNamespace : _inner.LookupNamespace(prefix);
        public override bool Read() { _typeValue = false; _typePrefix = null; _typeNamespace = null; return _inner.Read(); }
        public override bool ReadAttributeValue()
        { _typeValue = _typeValue || _inner.LocalName == "type" && _inner.NamespaceURI == Xsi; return _inner.ReadAttributeValue(); }
        public override bool MoveToAttribute(string name) { _typeValue = false; return _inner.MoveToAttribute(name); }
        public override bool MoveToAttribute(string name, string ns) { _typeValue = false; return _inner.MoveToAttribute(name, ns); }
        public override void MoveToAttribute(int i) { _typeValue = false; _inner.MoveToAttribute(i); }
        public override bool MoveToFirstAttribute() { _typeValue = false; return _inner.MoveToFirstAttribute(); }
        public override bool MoveToNextAttribute() { _typeValue = false; return _inner.MoveToNextAttribute(); }
        public override bool MoveToElement() { _typeValue = false; return _inner.MoveToElement(); }
        public override bool CanReadBinaryContent => _inner.CanReadBinaryContent;
        public override int ReadContentAsBase64(byte[] buffer, int index, int count) => _inner.ReadContentAsBase64(buffer, index, count);
        public override int ReadElementContentAsBase64(byte[] buffer, int index, int count) => _inner.ReadElementContentAsBase64(buffer, index, count);
        public override int ReadContentAsBinHex(byte[] buffer, int index, int count) => _inner.ReadContentAsBinHex(buffer, index, count);
        public override int ReadElementContentAsBinHex(byte[] buffer, int index, int count) => _inner.ReadElementContentAsBinHex(buffer, index, count);
        public override XmlNodeType NodeType => _inner.NodeType;
        public override string Prefix => _inner.Prefix;
        public override int AttributeCount => _inner.AttributeCount;
        public override string BaseURI => _inner.BaseURI;
        public override int Depth => _inner.Depth;
        public override bool EOF => _inner.EOF;
        public override bool IsEmptyElement => _inner.IsEmptyElement;
        public override XmlNameTable NameTable => _inner.NameTable;
        public override ReadState ReadState => _inner.ReadState;
        public override bool HasValue => _inner.HasValue;
        public override string XmlLang => _inner.XmlLang;
        public override XmlSpace XmlSpace => _inner.XmlSpace;
        public override XmlReaderSettings Settings => _inner.Settings;
        public override void ResolveEntity() => _inner.ResolveEntity();
        // The caller owns the reader and its archive envelope.
        public override void Close() { }
    }
}
