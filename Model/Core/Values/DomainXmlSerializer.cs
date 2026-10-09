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
        private readonly DataContractSerializer _serializer;
        internal DomainXmlSerializer(Type type, IEnumerable<Type> knownTypes)
        { _serializer = new DataContractSerializer(type, new DataContractSerializerSettings
            { KnownTypes = knownTypes, PreserveObjectReferences = true, MaxItemsInObjectGraph = 2000000 }); }
        internal void WriteObject(Stream destination, object value)
        {
            using (var writer = XmlWriter.Create(destination, new XmlWriterSettings { NewLineHandling = NewLineHandling.Entitize, CloseOutput = false, OmitXmlDeclaration = true }))
                WriteObject(writer, value);
        }
        internal void WriteObject(XmlWriter destination, object value) => _serializer.WriteObject(new ContractXmlWriter(destination), value);
        internal object ReadObject(Stream source)
        {
            using (var reader = XmlReader.Create(source, new XmlReaderSettings { CloseInput = false,
                DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 256L * 1024 * 1024 }))
                return ReadObject(reader);
        }
        internal object ReadObject(XmlReader source) => _serializer.ReadObject(new ContractXmlReader(source));
    }
}
