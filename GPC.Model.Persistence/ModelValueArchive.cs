using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using GPC.Model.Core;

namespace GPC.Model.Persistence
{
    /// <summary>Archive of one standalone registered domain value, without a FEM Model container.
    /// Uses the same closed vocabulary and numerical XML codec as ModelArchive. Streams remain open.
    /// The caller selects T; no CLR type name supplied by a document is resolved.</summary>
    public static class ModelValueArchive
    {
        public static void Save<T>(T value,Stream destination) where T:class
        {
            RequireRoot<T>();if(value==null||destination==null)throw new ArgumentNullException();
            if(!ModelValues.DataContracts.Contains(value.GetType()))throw new SerializationException("UnregisteredDomainValueType");
            using(var writer=XmlWriter.Create(destination,new XmlWriterSettings {CloseOutput=false,NewLineHandling=NewLineHandling.Entitize}))
            {
                writer.WriteStartElement("gpc-value");writer.WriteAttributeString("version","1");
                ModelValues.Serializer(typeof(T)).WriteObject(writer,value);writer.WriteEndElement();
            }
        }
        public static T Load<T>(Stream source) where T:class
        {
            RequireRoot<T>();if(source==null)throw new ArgumentNullException(nameof(source));
            using(var buffer=new MemoryStream())
            {
                var block=new byte[8192];int n;
                while((n=source.Read(block,0,block.Length))!=0)
                {if(buffer.Length+n>DomainXmlSerializer.DocumentCharacters)throw new SerializationException("DomainValueArchiveTooLarge");buffer.Write(block,0,n);}
                buffer.Position=0;
                using(var reader=XmlReader.Create(buffer,new XmlReaderSettings {DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null,MaxCharactersInDocument=Math.Max(1,buffer.Length)}))
                {
                    reader.MoveToContent();if(reader.LocalName!="gpc-value"||reader.NamespaceURI!=""||reader.GetAttribute("version")!="1")throw new SerializationException("UnsupportedDomainValueArchive");
                    reader.ReadStartElement();var value=(T)ModelValues.DocumentSerializer(typeof(T),buffer.Length).ReadObject(reader);
                    reader.ReadEndElement();if(reader.MoveToContent()!=XmlNodeType.None)throw new SerializationException("TrailingDomainValueContent");
                    return value??throw new SerializationException("NullDomainValue");
                }
            }
        }
        private static void RequireRoot<T>()
        {if(!ModelValues.DataContracts.Contains(typeof(T)))throw new SerializationException("UnregisteredDomainValueType");}
    }
}
