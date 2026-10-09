using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;
using GPC.Model.PostProcessing;

namespace GPC.Model.Persistence
{
    /// <summary>Standalone, versioned report archive. Only shared data contracts; no Model or solver instance is required.</summary>
    public static class CheckReportArchive
    {
        public static void Save(IEnumerable<CheckReport> reports, Stream destination)
        {
            var rows = (reports ?? throw new ArgumentNullException(nameof(reports))).ToArray();
            Validate(rows);
            using (var memory = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(memory, new XmlWriterSettings { Indent = true, CloseOutput = false }))
                {
                    writer.WriteStartElement("GpcCheckReports"); writer.WriteAttributeString("version", RequiresVersion5(rows) ? "5" : RequiresVersion4(rows) ? "4" : RequiresVersion3(rows) ? "3" : RequiresVersion2(rows) ? "2" : "1");
                    ModelArchive.Serializer(typeof(CheckReport[])).WriteObject(writer, rows); writer.WriteEndElement();
                }
                memory.Position = 0; memory.CopyTo(destination);
            }
        }
        public static IReadOnlyList<CheckReport> Load(Stream source)
        {
            using (var reader = XmlReader.Create(source, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null, MaxCharactersInDocument = 256L * 1024 * 1024, CloseInput = false }))
            {
                reader.MoveToContent();
                string version = reader.GetAttribute("version");
                if (reader.LocalName != "GpcCheckReports" || (version != "1" && version != "2" && version != "3" && version != "4" && version != "5")) throw new SerializationException("Unsupported check report archive version.");
                reader.ReadStartElement();
                var reports = (CheckReport[])ModelArchive.Serializer(typeof(CheckReport[])).ReadObject(reader);
                reader.ReadEndElement(); Validate(reports);
                if (version != "5" && RequiresVersion5(reports)) throw new SerializationException("Readable shell specifications require archive version 5.");
                if (version != "5" && version != "4" && RequiresVersion4(reports)) throw new SerializationException("Plate task reports require archive version 4.");
                if (version != "5" && version != "3" && version != "4" && RequiresVersion3(reports)) throw new SerializationException("Analysis/scenario provenance requires report archive version 3.");
                if (version == "1" && RequiresVersion2(reports)) throw new SerializationException("Member reports require archive version 2.");
                return Array.AsReadOnly(reports);
            }
        }
        internal static bool RequiresVersion2(IEnumerable<CheckReport> reports) => Checking.ReportSchema.RequiresVersion2(reports);
        internal static bool RequiresVersion3(IEnumerable<CheckReport> reports) => Checking.ReportSchema.RequiresVersion3(reports);
        internal static bool RequiresVersion4(IEnumerable<CheckReport> reports) => Checking.ReportSchema.RequiresVersion4(reports);
        internal static bool RequiresVersion5(IEnumerable<CheckReport> reports) => Checking.ReportSchema.RequiresVersion5(reports);
        internal static void Validate(CheckReport[] reports) => Checking.ReportSchema.Validate(reports);
    }
}
