using System.Xml;

namespace GPC.Model.Core
{
    // Streaming vocabulary adapter. Only element QNames, qualified type values and namespace declarations change.
    internal sealed class ContractXmlWriter : XmlWriter
    {
        private readonly XmlWriter _inner;
        private bool _namespaceAttribute;
        internal ContractXmlWriter(XmlWriter inner) { _inner = inner; }
        public override void WriteStartElement(string prefix, string localName, string ns)
        { var name = XmlContractNames.Write(localName, ns); _inner.WriteStartElement(prefix, name.LocalName, XmlContractNames.WriteNamespace(name.NamespaceName)); }
        public override void WriteStartAttribute(string prefix, string localName, string ns)
        { _namespaceAttribute = prefix == "xmlns" || localName == "xmlns"; _inner.WriteStartAttribute(prefix, localName, ns); }
        public override void WriteEndAttribute() { _inner.WriteEndAttribute(); _namespaceAttribute = false; }
        public override void WriteString(string text) => _inner.WriteString(_namespaceAttribute ? XmlContractNames.WriteNamespace(text) : text);
        public override void WriteQualifiedName(string localName, string ns)
        { var name = XmlContractNames.Write(localName, ns); _inner.WriteQualifiedName(name.LocalName, XmlContractNames.WriteNamespace(name.NamespaceName)); }
        public override string LookupPrefix(string ns) => _inner.LookupPrefix(XmlContractNames.WriteNamespace(ns));
        public override WriteState WriteState => _inner.WriteState;
        public override XmlWriterSettings Settings => _inner.Settings;
        public override string XmlLang => _inner.XmlLang;
        public override XmlSpace XmlSpace => _inner.XmlSpace;
        public override void Flush() => _inner.Flush();
        // The caller owns the stream and any enclosing archive element.
        public override void Close() => Flush();
        public override void WriteStartDocument() => _inner.WriteStartDocument();
        public override void WriteStartDocument(bool standalone) => _inner.WriteStartDocument(standalone);
        public override void WriteEndDocument() => _inner.WriteEndDocument();
        public override void WriteDocType(string name, string pubid, string sysid, string subset) => _inner.WriteDocType(name, pubid, sysid, subset);
        public override void WriteEndElement() => _inner.WriteEndElement();
        public override void WriteFullEndElement() => _inner.WriteFullEndElement();
        public override void WriteCData(string text) => _inner.WriteCData(text);
        public override void WriteComment(string text) => _inner.WriteComment(text);
        public override void WriteProcessingInstruction(string name, string text) => _inner.WriteProcessingInstruction(name, text);
        public override void WriteEntityRef(string name) => _inner.WriteEntityRef(name);
        public override void WriteCharEntity(char ch) => _inner.WriteCharEntity(ch);
        public override void WriteWhitespace(string ws) => _inner.WriteWhitespace(ws);
        public override void WriteSurrogateCharEntity(char lowChar, char highChar) => _inner.WriteSurrogateCharEntity(lowChar, highChar);
        public override void WriteChars(char[] buffer, int index, int count) => _inner.WriteChars(buffer, index, count);
        public override void WriteRaw(char[] buffer, int index, int count) => _inner.WriteRaw(buffer, index, count);
        public override void WriteRaw(string data) => _inner.WriteRaw(data);
        public override void WriteBase64(byte[] buffer, int index, int count) => _inner.WriteBase64(buffer, index, count);
    }
}
