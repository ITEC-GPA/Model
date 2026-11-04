using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using GPC.Model.Core;

namespace GPC.Model.Persistence
{
    /// <summary>Archive of one standalone registered domain value, without a FEM Model container.
    /// Uses the same closed vocabulary and numerical XML codec as ModelArchive. Streams remain open.
    /// The caller selects T; no CLR type name supplied by a document is resolved.
    /// <para>Version 1 is XML; version 2 (<see cref="ModelArchiveFormat.Compact"/>, opt-in) is the same contracts in binary form, deflated.
    /// <see cref="Load{T}"/> reads both.</para></summary>
    public static class ModelValueArchive
    {
        // "GPCVAL", version 2, flags; the raw length of the binary graph; the graph, deflated.
        private static readonly byte[] CompactMagic = { (byte)'G', (byte)'P', (byte)'C', (byte)'V', (byte)'A', (byte)'L' };
        private const byte CompactVersion = 2;

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

        /// <summary>Save with an explicit <paramref name="format"/>: <see cref="ModelArchiveFormat.Compact"/> writes version 2, or version 1
        /// when the binary form does not reproduce the value.</summary>
        public static void Save<T>(T value, Stream destination, ModelArchiveFormat format) where T : class
        {
            if (format == ModelArchiveFormat.Xml) { Save(value, destination); return; }
            if (format != ModelArchiveFormat.Compact) throw new ArgumentOutOfRangeException(nameof(format));
            RequireRoot<T>(); if (value == null || destination == null) throw new ArgumentNullException();
            if (!ModelValues.DataContracts.Contains(value.GetType())) throw new SerializationException("UnregisteredDomainValueType");
            using (var memory = new MemoryStream())
            {
                memory.Write(CompactMagic, 0, CompactMagic.Length); memory.WriteByte(CompactVersion); memory.WriteByte(0); memory.Write(new byte[8], 0, 8);
                long raw;
                try
                {
                    using (var deflate = new DeflateStream(memory, CompressionLevel.Fastest, true))
                    using (var counting = new ModelArchive.CountingStream(deflate))
                    { ModelImage.WriteBinaryGraph(counting, value, typeof(T), null); raw = counting.Count; }
                }
                catch (ContractCopy.UnsupportedException) { Save(value, destination); return; }
                memory.Position = 8; memory.Write(BitConverter.GetBytes(raw), 0, 8);
                memory.Position = 0; memory.CopyTo(destination);
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
                if (buffer.Length >= 16 && buffer.GetBuffer().Take(6).SequenceEqual(CompactMagic)) return LoadCompact<T>(buffer.GetBuffer(), (int)buffer.Length);
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

        /// <summary>Version 2: the graph is read within the bytes it declares, which deflate must be able to expand from the stored bytes.</summary>
        private static T LoadCompact<T>(byte[] data, int length) where T : class
        {
            if (data[6] != CompactVersion || data[7] != 0) throw new SerializationException("UnsupportedDomainValueArchive");
            long raw = BitConverter.ToInt64(data, 8); int stored = length - 16;
            if (raw < 5 || stored <= 0 || raw > stored * DomainXmlSerializer.DeflateExpansion) throw new SerializationException("CorruptDomainValueArchive");
            try
            {
                using (var inflate = new DeflateStream(new MemoryStream(data, 16, stored, false), CompressionMode.Decompress))
                {
                    var value = ModelImage.ReadBinaryGraph(inflate, raw, typeof(T)) as T ?? throw new SerializationException("NullDomainValue");
                    if (inflate.ReadByte() != -1) throw new SerializationException("TrailingDomainValueContent");
                    return value;
                }
            }
            catch (InvalidDataException ex) { throw new SerializationException("CorruptDomainValueArchive", ex); }
        }

        private static void RequireRoot<T>()
        {if(!ModelValues.DataContracts.Contains(typeof(T)))throw new SerializationException("UnregisteredDomainValueType");}
    }
}
