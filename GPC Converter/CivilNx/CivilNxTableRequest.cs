using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace GPC.Converter.CivilNx
{
    public enum CivilNxResultTable { BeamForce, PlateForcePerUnitLength, DisplacementGlobal, ReactionGlobal, SectionProperties, MaterialProperties }

    public sealed class CivilNxTableRequest
    {
        public CivilNxResultTable Table { get; set; }
        public string Name { get; set; } = "GPC Results";
        public int[] ElementIds { get; set; }
        public string[] LoadCases { get; set; }
        /// <summary>Null requests every part the API returns (I, 1/4, 2/4, 3/4, J); the response labels them I[node], 1/4, 2/4, 3/4, J[node].</summary>
        public string[] BeamParts { get; set; }
        public string[] StageSteps { get; set; }
        public string ToJson()
        {
            if (!Enum.IsDefined(typeof(CivilNxResultTable), Table) || string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Explicit table required.");
            var argument = new Dictionary<string, object>
            {
                ["TABLE_NAME"] = Name, ["TABLE_TYPE"] = Type(Table),
                ["UNIT"] = new Dictionary<string, object> { ["FORCE"] = "N", ["DIST"] = "mm" },
                ["STYLES"] = new Dictionary<string, object> { ["FORMAT"] = "Scientific", ["PLACE"] = 12 }
            };
            if (Table == CivilNxResultTable.SectionProperties || Table == CivilNxResultTable.MaterialProperties)
                return CivilNxJson.Write(new Dictionary<string, object> { ["Argument"] = argument });
            if (ElementIds == null || ElementIds.Length == 0 || ElementIds.Any(id => id <= 0) || LoadCases == null || LoadCases.Length == 0 || LoadCases.Any(string.IsNullOrWhiteSpace))
                throw new ArgumentException("Explicit table, elements and cases required.");
            argument["NODE_ELEMS"] = new Dictionary<string, object> { ["KEYS"] = ElementIds.Distinct().ToArray() };
            argument["LOAD_CASE_NAMES"] = LoadCases;
            if (Table == CivilNxResultTable.BeamForce)
            {
                if (BeamParts != null && (BeamParts.Length == 0 || BeamParts.Any(string.IsNullOrWhiteSpace))) throw new ArgumentException("Explicit beam parts required.");
                if (BeamParts != null) argument["PARTS"] = BeamParts;
            }
            else if (Table == CivilNxResultTable.PlateForcePerUnitLength)
            {
                argument["AVERAGE_NODAL_RESULT"] = false;
                argument["NODE_FLAG"] = new Dictionary<string, object> { ["CENTER"] = true, ["NODES"] = false };
            }
            if (StageSteps != null && StageSteps.Length > 0) { argument["OPT_CS"] = true; argument["STAGE_STEP"] = StageSteps; }
            return CivilNxJson.Write(new Dictionary<string, object> { ["Argument"] = argument });
        }
        private static string Type(CivilNxResultTable table)
        {
            switch (table)
            {
                case CivilNxResultTable.BeamForce: return "BEAMFORCE";
                case CivilNxResultTable.PlateForcePerUnitLength: return "PLATEFORCEUL";
                case CivilNxResultTable.DisplacementGlobal: return "DISPLACEMENTG";
                case CivilNxResultTable.ReactionGlobal: return "REACTIONG";
                case CivilNxResultTable.MaterialProperties: return "MATERIAL";
                default: return "SECTIONALL";
            }
        }
    }

    internal static class CivilNxJson
    {
        private static readonly System.Text.RegularExpressions.Regex EmptyAnswer =
            new System.Text.RegularExpressions.Regex(@"^\s*\{\s*(""message""\s*:\s*""""\s*)?\}\s*$", System.Text.RegularExpressions.RegexOptions.CultureInvariant);
        /// <summary>The API answers {"message":""} (HTTP 200) for a database without records; a non-empty message is not an empty database.</summary>
        public static bool IsEmptyDatabase(string json) => json != null && EmptyAnswer.IsMatch(json);
        public static T Read<T>(string json)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json)))
                return (T)new DataContractJsonSerializer(typeof(T), new DataContractJsonSerializerSettings { UseSimpleDictionaryFormat = true, MaxItemsInObjectGraph = 2000000 }).ReadObject(stream);
        }
        public static string Write(Dictionary<string, object> value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return "{" + string.Join(",", value.Select(pair => Quote(pair.Key) + ":" + Value(pair.Value))) + "}";
        }
        private static string Value(object value)
        {
            if (value is Dictionary<string, object> dictionary) return Write(dictionary);
            if (value is string text) return Quote(text);
            if (value is bool flag) return flag ? "true" : "false";
            if (value is int number) return number.ToString(System.Globalization.CultureInfo.InvariantCulture);
            if (value is string[] texts) return "[" + string.Join(",", texts.Select(Quote)) + "]";
            if (value is int[] numbers) return "[" + string.Join(",", numbers.Select(n => n.ToString(System.Globalization.CultureInfo.InvariantCulture))) + "]";
            throw new ArgumentException("Unsupported request field type.");
        }
        private static string Quote(string value)
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(string)).WriteObject(stream, value);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }

    [DataContract]
    public sealed class CivilNxResultTableData
    {
        [DataMember(Name = "HEAD")] public string[] Headers { get; private set; }
        [DataMember(Name = "DATA")] public string[][] Rows { get; private set; }
        [DataMember(Name = "FORCE")] public string ForceUnit { get; private set; }
        [DataMember(Name = "DIST")] public string LengthUnit { get; private set; }
        public static CivilNxResultTableData Read(CivilNxResponse response, string tableName)
        {
            var tables = CivilNxJson.Read<Dictionary<string, CivilNxResultTableData>>(response.Json);
            if (tables == null || !tables.TryGetValue(tableName, out var table) || table?.Headers == null || table.Rows == null)
                throw new NotSupportedException("UnknownCivilNxResultSchema: retain the original response for a compatible schema profile.");
            if (table.Headers.Any(string.IsNullOrWhiteSpace)
                || table.Rows.Any(r => r == null || r.Length != table.Headers.Length)) throw new InvalidDataException("AmbiguousCivilNxResultColumns");
            return table;
        }
        public string Get(int row, string column)
        {
            var index = Array.IndexOf(Headers, column);
            if (index < 0) throw new InvalidDataException("MissingCivilNxResultColumn: " + column);
            if (Headers.Count(h => h == column) != 1) throw new InvalidDataException("AmbiguousCivilNxResultColumn: " + column);
            return Rows[row][index];
        }
    }
}
