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

        private static readonly Lazy<SectionCatalog> _en10210CircularHollowCelsius = Catalog("EN10210_CircularHollowCelsius",
            series => series == "CHS" ? SectionFamily.CircularHollow : (SectionFamily?)null);

        private static readonly Lazy<SectionCatalog> _en10210RectangularHollow = Catalog("EN10210_RectangularHollow",
            series => series == "SHS" || series == "RHS" ? SectionFamily.RectangularHollow : (SectionFamily?)null);

        private static readonly Lazy<SectionCatalog> _en10219CircularHollow = Catalog("EN10219_CircularHollow",
            series => series == "CHS" ? SectionFamily.CircularHollow : (SectionFamily?)null);

        private static readonly Lazy<SectionCatalog> _aiscShapesV16 = Catalog(AiscId, AiscFamily);

        /// <summary>
        /// The identifier of the catalog of the AISC shapes
        /// </summary>
        internal const string AiscId = "AISC_ShapesV16";

        private static SectionFamily? AiscFamily(string series)
        {
            switch (series)
            {
                case "W": case "M": case "HP": return SectionFamily.ParallelFlangeIH;
                case "S": return SectionFamily.TaperFlangeI;
                case "C": case "MC": return SectionFamily.TaperFlangeChannel;
                case "L": return SectionFamily.Angle;
                case "WT": case "MT": case "ST": return SectionFamily.Tee;
                case "2L": return SectionFamily.DoubleAngle;
                case "HSS": return SectionFamily.RectangularHollow;
                case "HSS round": case "PIPE": return SectionFamily.CircularHollow;
                default: return null;
            }
        }

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
        /// Hot finished circular hollow sections to EN 10210 of the Tata Steel Celsius range: the sizes of the tables of EN 10210-2 and the
        /// other sizes of the producer (see <see cref="CatalogProfile.IsInStandard"/>), e.g. CHS 60.3 x 3.6, 88.9 x 10, 273 x 17.5; source: Tata
        /// Steel workbook of the Celsius CHS properties and Celsius brochure
        /// </summary>
        public static SectionCatalog EN10210CircularHollowCelsius => _en10210CircularHollowCelsius.Value;

        /// <summary>
        /// Hot finished square and rectangular hollow sections of EN 10210-2 (SHS, RHS); source: tables of Fondazione Promozione Acciaio
        /// </summary>
        public static SectionCatalog EN10210RectangularHollow => _en10210RectangularHollow.Value;

        /// <summary>
        /// Cold formed circular hollow sections of EN 10219-2; source: tables of Fondazione Promozione Acciaio
        /// </summary>
        public static SectionCatalog EN10219CircularHollow => _en10219CircularHollow.Value;

        /// <summary>
        /// American shapes of the AISC Steel Construction Manual, 16th Edition (W, M, S, HP, C, MC, L, WT, MT, ST, 2L, HSS, PIPE); source:
        /// AISC Shapes Database v16.0. Designations of the Manual (W44X408) and metric ones (W1100X607, see <see cref="CatalogProfile.Alias"/>)
        /// </summary>
        public static SectionCatalog AISCShapesV16 => _aiscShapesV16.Value;

        /// <summary>
        /// All the catalogs: the European ones before the American ones (the metric designations of some AISC shapes are the same of
        /// European ones, e.g. HP 360 x 174: <see cref="Find"/> returns the European one), the tables of the standards before the producer
        /// range (Celsius), the hot finished hollow sections before the cold formed ones with the same designation
        /// </summary>
        public static IReadOnlyList<SectionCatalog> All => new[]
        {
            EN10365ParallelFlangeIH, EN10365TaperFlangeI, EN10365Channels, EN10056Angles, EN10210CircularHollow, EN10210CircularHollowCelsius,
            EN10210RectangularHollow, EN10219CircularHollow, AISCShapesV16,
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
