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
        /// <summary>Fewest characters of one object of a document. DataContractSerializer counts one object per element, and
        /// the declared size (z:Size) of an array before allocating it (measured: a double[1000] needs 1001); the shortest
        /// element is "&lt;a/&gt;".</summary>
        internal const int CharactersPerObject = 4;
        /// <summary>Ceiling of the objects of a document of <paramref name="characters"/> characters: no valid document holds
        /// more, so the ceiling follows its size and a short text cannot declare a large array or graph (without a ceiling,
        /// 20 KB declaring z:Size="1500000000" allocated 11 GB before failing).</summary>
        internal static int ObjectsIn(long characters) => (int)Math.Min(int.MaxValue, Math.Max(1, characters / CharactersPerObject));
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
