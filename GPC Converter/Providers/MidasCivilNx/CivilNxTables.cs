using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using GPC.Model.Analysis;

namespace GPC.Converter.CivilNx
{
    /// <summary>Civil NX database records already parsed by another reader (the MIDAS MCT/MGT text files), in N, mm and °C, with the
    /// source record of every row. <see cref="CivilNxModelProfile"/> maps them with the same code as the API responses.</summary>
    internal sealed class CivilNxTables
    {
        private readonly Dictionary<string, IDictionary> rows = new Dictionary<string, IDictionary>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> records = new Dictionary<string, string>(StringComparer.Ordinal);
        private readonly Dictionary<string, string> tableRecords = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>Text-file conventions: undefined material/section references and unsupported element families leave the element
        /// without property or out of the model with a diagnostic, instead of rejecting the whole file.</summary>
        public bool Lenient { get; set; }

        public Dictionary<string, T> Table<T>(string table)
        {
            if (rows.TryGetValue(table, out var existing)) return (Dictionary<string, T>)existing;
            var created = new Dictionary<string, T>(StringComparer.Ordinal); rows.Add(table, created); return created;
        }

        public bool Has(string table) => rows.TryGetValue(table, out var data) && data.Count != 0;

        /// <summary>Name of the marker table that asks <see cref="CivilNxModelProfile"/> for the conventions of the text files when the
        /// translated tables come back as API responses ({"GPC-TEXT":{"1":{"PROGRAM":..., "LENIENT":true}}}).</summary>
        public const string TextConventionsTable = "GPC-TEXT";

        /// <summary>Names of the tables with rows, in the order of their first record.</summary>
        public IEnumerable<string> Names => rows.Where(p => p.Value.Count != 0).Select(p => p.Key);

        /// <summary>The table as the API response: {"TABLE":{"key":{...}}}, the field names of the API, null members omitted.</summary>
        public string Json(string table)
        {
            var text = new StringBuilder("{"); Quote(text, table); text.Append(":{");
            bool first = true;
            if (rows.TryGetValue(table, out var data))
                foreach (DictionaryEntry row in data)
                {
                    if (!first) text.Append(','); first = false;
                    Quote(text, (string)row.Key); text.Append(':'); Value(text, row.Value);
                }
            return text.Append("}}").ToString();
        }

        private static void Value(StringBuilder text, object value)
        {
            switch (value)
            {
                case null: text.Append("null"); return;
                case string s: Quote(text, s); return;
                case bool b: text.Append(b ? "true" : "false"); return;
                case int i: text.Append(i.ToString(CultureInfo.InvariantCulture)); return;
                case long l: text.Append(l.ToString(CultureInfo.InvariantCulture)); return;
                case double d:
                    if (double.IsNaN(d) || double.IsInfinity(d)) throw new InvalidOperationException("Non-finite value in a translated table.");
                    text.Append(d.ToString("R", CultureInfo.InvariantCulture)); return;
                case IEnumerable list:
                    text.Append('['); bool first = true;
                    foreach (var item in list) { if (!first) text.Append(','); first = false; Value(text, item); }
                    text.Append(']'); return;
            }
            text.Append('{'); bool any = false;
            foreach (var member in value.GetType().GetMembers(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = member.GetCustomAttribute<DataMemberAttribute>(); if (attribute == null) continue;
                var data = member is PropertyInfo p ? p.GetValue(value) : member is FieldInfo f ? f.GetValue(value) : null;
                if (data == null) continue;
                if (any) text.Append(','); any = true;
                Quote(text, attribute.Name ?? member.Name); text.Append(':'); Value(text, data);
            }
            text.Append('}');
        }

        private static void Quote(StringBuilder text, string value)
        {
            text.Append('"');
            foreach (char c in value)
            {
                if (c == '"' || c == '\\') text.Append('\\').Append(c);
                else if (c < ' ') text.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                else text.Append(c);
            }
            text.Append('"');
        }

        public Dictionary<string, T> Rows<T>(string table) => rows.TryGetValue(table, out var data) ? (Dictionary<string, T>)data : new Dictionary<string, T>();

        /// <summary>Source record of a row ("ELEM/12") or of an item of a row ("BMLD/12/3").</summary>
        public void Record(string table, string key, string record) { if (!records.ContainsKey(table + "/" + key)) records.Add(table + "/" + key, record); }
        /// <summary>Source command of a whole table, for diagnostics that are not about one row.</summary>
        public void TableRecord(string table, string record) { if (!tableRecords.ContainsKey(table)) tableRecords.Add(table, record); }

        public string Resolve(string table, string key)
        {
            if (key == null) return tableRecords.TryGetValue(table, out var command) ? command : table;
            return records.TryGetValue(table + "/" + key, out var record) ? record : (tableRecords.TryGetValue(table, out var block) ? block + " " : table + "/") + key;
        }
    }

    /// <summary>Row of the marker table <see cref="CivilNxTables.TextConventionsTable"/>: source program and version of the translated text file.</summary>
    [DataContract]
    internal sealed class TextSourceData
    {
        [DataMember(Name = "PROGRAM")] public string Program { get; set; }
        [DataMember(Name = "VERSION")] public string Version { get; set; }
        [DataMember(Name = "LENIENT")] public bool Lenient { get; set; }
    }

    public sealed partial class CivilNxModelProfile
    {
        /// <summary>Maps already parsed records with the API reader. The identity names the source program (e.g. "MIDAS Civil").</summary>
        internal static ImportBatch ReadTables(CivilNxTables tables, AnalysisSource identity, CancellationToken cancellationToken)
        {
            if (tables == null || identity == null || string.IsNullOrWhiteSpace(identity.Program) || string.IsNullOrWhiteSpace(identity.ModelRevision))
                throw new ArgumentException("ExplicitModelIdentityRequired");
            return new Reader(new CivilNxSnapshot(new Dictionary<string, CivilNxResponse>()), identity, cancellationToken, new string[0], tables).Read();
        }
    }
}
