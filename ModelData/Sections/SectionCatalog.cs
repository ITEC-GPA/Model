using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GPC.Model.Data.Sections
{
    /// <summary>
    /// A catalog of commercial sections of a standard, read from a CSV file embedded in the assembly (see
    /// tools/section-catalogs): the header lines ("# key: value") give the standard and the source with its version and hash, then a header
    /// row with the names of the columns and one row for each section, with the values in mm units
    /// </summary>
    public sealed class SectionCatalog
    {
        private readonly Dictionary<string, CatalogProfile> _byKey = new Dictionary<string, CatalogProfile>();

        private SectionCatalog(string id, IReadOnlyDictionary<string, string> metadata)
        {
            Id = id;
            Metadata = metadata;
        }

        /// <summary>
        /// The identifier of the catalog (e.g. "EN10365_ParallelFlangeIH")
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The description of the catalog
        /// </summary>
        public string Title => Value("Catalog");

        /// <summary>
        /// The standard of the dimensions, as declared by the source (e.g. "EN 10365:2017 (dimensions), EN 10034:1993 (tolerances)")
        /// </summary>
        public string Standard => Value("Standard (declared by the source)");

        /// <summary>
        /// The source of the data, with its version
        /// </summary>
        public string Source => Value("Source");

        /// <summary>
        /// The address of the source file
        /// </summary>
        public string SourceFile => Value("Source file");

        /// <summary>
        /// The SHA256 of the source file the catalog was extracted from
        /// </summary>
        public string SourceSha256 => Value("Source SHA256");

        /// <summary>
        /// All the header lines of the file ("key: value"; the lines without a key have the key "Note n")
        /// </summary>
        public IReadOnlyDictionary<string, string> Metadata { get; }

        /// <summary>
        /// The sections, in the order of the source
        /// </summary>
        public IReadOnlyList<CatalogProfile> Profiles { get; private set; }

        /// <summary>
        /// The aliases not searched because equal to the designation or to the alias of another section of the catalog (the section is found
        /// with its designation)
        /// </summary>
        public IReadOnlyList<string> AmbiguousAliases { get; private set; } = new string[0];

        /// <summary>
        /// The section with a designation (see <see cref="SectionCatalogs.Normalize"/> for the accepted spellings)
        /// </summary>
        /// <param name="designation">The designation (e.g. "HEB 300", "HE 300 B", "HE300B")</param>
        /// <returns>The section, null if the catalog does not contain it</returns>
        public CatalogProfile Find(string designation)
        {
            return _byKey.TryGetValue(SectionCatalogs.Normalize(designation), out CatalogProfile profile) ? profile : null;
        }

        /// <summary>
        /// The title of the catalog
        /// </summary>
        /// <returns>The title</returns>
        public override string ToString() => $"{Id}: {Title}";

        private string Value(string key) => Metadata.TryGetValue(key, out string value) ? value : string.Empty;

        /// <summary>
        /// Reads a catalog
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <param name="reader">The CSV text</param>
        /// <param name="familyOfSeries">The geometric family of each series of the catalog</param>
        /// <returns>The catalog</returns>
        /// <exception cref="InvalidDataException">If a row has a wrong number of values, a value is not a number or a series has no
        /// family</exception>
        internal static SectionCatalog Read(string id, TextReader reader, Func<string, SectionFamily?> familyOfSeries)
        {
            var metadata = new Dictionary<string, string>();
            string line;
            string[] header = null;
            var rows = new List<string[]>();
            int notes = 0;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length == 0)
                    continue;
                if (line.StartsWith("#", StringComparison.Ordinal))
                {
                    string text = line.Substring(1).Trim();
                    int colon = text.IndexOf(": ", StringComparison.Ordinal);
                    if (colon > 0 && !metadata.ContainsKey(text.Substring(0, colon)))
                        metadata[text.Substring(0, colon)] = text.Substring(colon + 2);
                    else
                        metadata["Note " + ++notes] = text;
                    continue;
                }
                if (line.IndexOf('"') >= 0)
                    throw new InvalidDataException($"{id}: quoted values are not supported: \"{line}\"");
                string[] cells = line.Split(',');
                if (header is null)
                    header = cells;
                else if (cells.Length != header.Length)
                    throw new InvalidDataException($"{id}: {cells.Length} values instead of {header.Length} in \"{line}\"");
                else
                    rows.Add(cells);
            }

            if (header is null || header.Length < 3 || header[0] != "Designation" || header[1] != "Series" || header[2] != "InStandard")
                throw new InvalidDataException($"{id}: the header must start with Designation,Series,InStandard");

            // the optional text column Alias: another designation of the section (e.g. the metric one of the AISC shapes)
            int alias = Array.IndexOf(header, "Alias");

            var catalog = new SectionCatalog(id, metadata);
            var profiles = new List<CatalogProfile>(rows.Count);
            foreach (string[] cells in rows)
            {
                SectionFamily family = familyOfSeries(cells[1]) ??
                    throw new InvalidDataException($"{id}: no family for the series {cells[1]} of {cells[0]}");
                var values = new Dictionary<string, double>();
                for (int c = 3; c < cells.Length; c++)
                {
                    if (cells[c].Length == 0 || c == alias)
                        continue;
                    if (!double.TryParse(cells[c], NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
                        throw new InvalidDataException($"{id}: {header[c]} of {cells[0]} is not a number: {cells[c]}");
                    values[header[c]] = value;
                }
                string aliasText = alias >= 0 && cells[alias].Length > 0 ? cells[alias] : null;
                var profile = new CatalogProfile(catalog, cells[0], cells[1], family, cells[2] == "1", values, aliasText);
                profiles.Add(profile);
                string key = SectionCatalogs.Normalize(profile.Designation);
                if (catalog._byKey.ContainsKey(key))
                    throw new InvalidDataException($"{id}: {profile.Designation} is repeated");
                catalog._byKey[key] = profile;
            }

            // the aliases after all the designations: an alias equal to the designation (or to the alias) of another section is ambiguous and
            // is not searched (e.g. the metric Pipe20STD, DN 20, of the AISC Pipe3/4STD is the designation of the NPS 20 pipe)
            var ambiguous = new List<string>();
            var aliasOwners = new Dictionary<string, CatalogProfile>();
            foreach (CatalogProfile profile in profiles.Where(p => p.Alias != null))
            {
                string key = SectionCatalogs.Normalize(profile.Alias);
                if (catalog._byKey.TryGetValue(key, out CatalogProfile other) && other != profile && !aliasOwners.ContainsKey(key))
                    ambiguous.Add($"{profile.Alias} ({profile.Designation}; designation of {other.Designation})");
                else if (aliasOwners.TryGetValue(key, out other) && other != profile)
                {
                    ambiguous.Add($"{profile.Alias} ({profile.Designation}; alias of {other.Designation})");
                    catalog._byKey.Remove(key);
                }
                else if (!catalog._byKey.ContainsKey(key))
                {
                    catalog._byKey[key] = profile;
                    aliasOwners[key] = profile;
                }
            }
            catalog.AmbiguousAliases = ambiguous.AsReadOnly();
            catalog.Profiles = profiles.AsReadOnly();
            return catalog;
        }
    }
}
