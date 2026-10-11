using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace GPC.Model.Core
{
    /// <summary>Internal snapshot codec with stable wire names across CLR namespace changes.</summary>
    internal sealed class DomainXmlSerializer
    {
        /// <summary>Ceiling of the objects of an XML document (archives 1-4, report archives, value copies).</summary>
        internal const int DocumentItems = 2000000;
        /// <summary>Ceiling of the characters read from an XML document.</summary>
        internal const long DocumentCharacters = 256L * 1024 * 1024;
        private readonly DataContractSerializer _serializer;
        internal DomainXmlSerializer(Type type, IEnumerable<Type> knownTypes, int maxItems = DocumentItems)
        { _serializer = new DataContractSerializer(type, new DataContractSerializerSettings
            { KnownTypes = knownTypes, PreserveObjectReferences = true, MaxItemsInObjectGraph = maxItems }); }
        internal void WriteObject(Stream destination, object value)
        {
            using (var writer = XmlWriter.Create(destination, new XmlWriterSettings { NewLineHandling = NewLineHandling.Entitize, CloseOutput = false, OmitXmlDeclaration = true }))
                WriteObject(writer, value);
        }
        internal void WriteObject(XmlWriter destination, object value) => _serializer.WriteObject(new ContractXmlWriter(destination), value);
        internal object ReadObject(Stream source, long maxCharacters = DocumentCharacters)
        {
            using (var reader = XmlReader.Create(source, new XmlReaderSettings { CloseInput = false,
                DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = maxCharacters }))
                return ReadObject(reader);
        }
        internal object ReadObject(XmlReader source) => _serializer.ReadObject(new ContractXmlReader(source));
    }
}
