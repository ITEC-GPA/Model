using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GPC.Model.Data.Sections
{
    /// <summary>
    /// The catalogs of commercial sections (read at the first access) and the search by designation. Each catalog keeps the standard and
    /// the source it comes from; the sections of Model are created by <see cref="SectionMappings"/>
    /// </summary>
    public static class SectionCatalogs
    {
        private static readonly Lazy<SectionCatalog> _en10365ParallelFlangeIH = Catalog("EN10365_ParallelFlangeIH",
            series => SectionFamily.ParallelFlangeIH);

        private static readonly Lazy<SectionCatalog> _en10365TaperFlangeI = Catalog("EN10365_TaperFlangeI",
            series => SectionFamily.TaperFlangeI);

        private static readonly Lazy<SectionCatalog> _en10365Channels = Catalog("EN10365_Channels",
            series => series == "UPN" ? SectionFamily.TaperFlangeChannel : series == "UPE" || series == "PFC" ? SectionFamily.ParallelFlangeChannel
                : (SectionFamily?)null);

        private static readonly Lazy<SectionCatalog> _en10056Angles = Catalog("EN10056_Angles",
            series => series == "L" ? SectionFamily.Angle : (SectionFamily?)null);

        private static readonly Lazy<SectionCatalog> _en10210CircularHollow = Catalog("EN10210_CircularHollow",
            series => series == "CHS" ? SectionFamily.CircularHollow : (SectionFamily?)null);

        private static readonly Lazy<SectionCatalog> _en10210RectangularHollow = Catalog("EN10210_RectangularHollow",
            series => series == "SHS" || series == "RHS" ? SectionFamily.RectangularHollow : (SectionFamily?)null);

        private static readonly Lazy<SectionCatalog> _en10219CircularHollow = Catalog("EN10219_CircularHollow",
            series => series == "CHS" ? SectionFamily.CircularHollow : (SectionFamily?)null);

        /// <summary>
        /// Parallel flange I and H sections of EN 10365:2017 (IPE, HE, HL, HLZ, HD, HP, UBP, UB, UC) and producer sizes (see
        /// <see cref="CatalogProfile.IsInStandard"/>); source: ArcelorMittal sales programme
        /// </summary>
        public static SectionCatalog EN10365ParallelFlangeIH => _en10365ParallelFlangeIH.Value;

        /// <summary>
        /// Taper flange I sections of EN 10365:2017 (IPN, J); source: ArcelorMittal sales programme
        /// </summary>
        public static SectionCatalog EN10365TaperFlangeI => _en10365TaperFlangeI.Value;

        /// <summary>
        /// Channels of EN 10365:2017 (UPE, PFC with parallel flanges, UPN with taper flanges); source: ArcelorMittal sales programme
        /// </summary>
        public static SectionCatalog EN10365Channels => _en10365Channels.Value;

        /// <summary>
        /// Equal and unequal leg angles of EN 10056-1:2017; source: ArcelorMittal sales programme
        /// </summary>
        public static SectionCatalog EN10056Angles => _en10056Angles.Value;

        /// <summary>
        /// Hot finished circular hollow sections of EN 10210-2; source: tables of Fondazione Promozione Acciaio
        /// </summary>
        public static SectionCatalog EN10210CircularHollow => _en10210CircularHollow.Value;

        /// <summary>
        /// Hot finished square and rectangular hollow sections of EN 10210-2 (SHS, RHS); source: tables of Fondazione Promozione Acciaio
        /// </summary>
        public static SectionCatalog EN10210RectangularHollow => _en10210RectangularHollow.Value;

        /// <summary>
        /// Cold formed circular hollow sections of EN 10219-2; source: tables of Fondazione Promozione Acciaio
        /// </summary>
        public static SectionCatalog EN10219CircularHollow => _en10219CircularHollow.Value;

        /// <summary>
        /// All the catalogs (the hot finished hollow sections before the cold formed ones with the same designation)
        /// </summary>
        public static IReadOnlyList<SectionCatalog> All => new[]
        {
            EN10365ParallelFlangeIH, EN10365TaperFlangeI, EN10365Channels, EN10056Angles, EN10210CircularHollow, EN10210RectangularHollow,
            EN10219CircularHollow,
        };

        /// <summary>
        /// The sections with a designation in all the catalogs (the same designation can be in the catalogs of different standards)
        /// </summary>
        /// <param name="designation">The designation (see <see cref="Normalize"/>)</param>
        /// <returns>The sections found, in the order of <see cref="All"/></returns>
        public static IReadOnlyList<CatalogProfile> FindAll(string designation)
        {
            return All.Select(c => c.Find(designation)).Where(p => p != null).ToList();
        }

        /// <summary>
        /// The section with a designation, searched in the catalogs in the order of <see cref="All"/>
        /// </summary>
        /// <param name="designation">The designation (see <see cref="Normalize"/>)</param>
        /// <returns>The first section found, null if none</returns>
        public static CatalogProfile Find(string designation)
        {
            foreach (SectionCatalog catalog in All)
            {
                CatalogProfile profile = catalog.Find(designation);
                if (profile != null)
                    return profile;
            }
            return null;
        }

        /// <summary>
        /// The key used to compare designations: upper case, without spaces, '-' and '_', with 'X' as separator of the dimensions and '.' as
        /// decimal separator. The European H sections are also accepted in the short form (HEA 300, HEB300, HEM 300, HEAA 300 = HE 300 A...)
        /// and the equal leg angles with one leg (L 100 x 10 = L 100 x 100 x 10)
        /// </summary>
        /// <param name="designation">The designation</param>
        /// <returns>The key</returns>
        public static string Normalize(string designation)
        {
            if (designation is null)
                return string.Empty;

            var builder = new StringBuilder(designation.Length);
            foreach (char c in designation.ToUpperInvariant())
            {
                if (char.IsWhiteSpace(c) || c == '-' || c == '_')
                    continue;
                builder.Append(c == '×' || c == '*' ? 'X' : c == ',' ? '.' : c);
            }
            string key = builder.ToString();

            // HEA300, HEAA300 -> HE300A, HE300AA
            Match match = Regex.Match(key, @"^HE(AA|A|B|M|C)(\d+)$");
            if (match.Success)
                return "HE" + match.Groups[2].Value + match.Groups[1].Value;

            // L100X10 -> L100X100X10
            match = Regex.Match(key, @"^L(\d+(?:\.\d+)?)X(\d+(?:\.\d+)?)$");
            if (match.Success)
                return "L" + match.Groups[1].Value + "X" + match.Groups[1].Value + "X" + match.Groups[2].Value;

            return key;
        }

        private static Lazy<SectionCatalog> Catalog(string id, Func<string, SectionFamily?> familyOfSeries)
        {
            return new Lazy<SectionCatalog>(() =>
            {
                string resource = typeof(SectionCatalogs).Assembly.GetManifestResourceNames()
                    .SingleOrDefault(n => n.EndsWith("." + id + ".csv", StringComparison.Ordinal))
                    ?? throw new InvalidOperationException($"The catalog {id} is not embedded in the assembly");
                using (Stream stream = typeof(SectionCatalogs).Assembly.GetManifestResourceStream(resource))
                using (var reader = new StreamReader(stream, Encoding.UTF8))
                    return SectionCatalog.Read(id, reader, familyOfSeries);
            });
        }
    }
}
