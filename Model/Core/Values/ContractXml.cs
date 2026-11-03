using System;
using System.Runtime.Serialization;
using System.Xml;

namespace GPC.Model.Core
{
    /// <summary>Shared numeric and historical XML wire conventions for application wrapper archives.
    /// The caller owns the serializer's closed type vocabulary and resource limits; no types are registered here.
    /// The reader/writer remain owned by the caller.</summary>
    public static class ContractXml
    {
        public static void Write(DataContractSerializer serializer, XmlWriter writer, object value)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            serializer.WriteObject(new ContractXmlWriter(writer), value);
        }
        public static object Read(DataContractSerializer serializer, XmlReader reader)
        {
            if (serializer == null) throw new ArgumentNullException(nameof(serializer));
            if (reader == null) throw new ArgumentNullException(nameof(reader));
            return serializer.ReadObject(new ContractXmlReader(reader));
        }
    }
}
