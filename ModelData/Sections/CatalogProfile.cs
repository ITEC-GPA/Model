using System.Collections.Generic;

namespace GPC.Model.Data.Sections
{
    /// <summary>
    /// A commercial section of a catalog: the designation, the series, the dimensions and the properties published by the source, in mm
    /// units (mm, mm², mm³, mm⁴, mm⁶ for the warping constant, kg/m for the mass). The names of the values are the ones of the catalog
    /// (h, b, tw, tf, r, A, Iy, Iz, Wely, Wply, It, Iw...: axis y-y strong, z-z weak, as in the Eurocodes)
    /// </summary>
    public sealed class CatalogProfile
    {
        private readonly Dictionary<string, double> _values;

        internal CatalogProfile(SectionCatalog catalog, string designation, string series, SectionFamily family, bool isInStandard,
            Dictionary<string, double> values, string alias = null)
        {
            Catalog = catalog;
            Designation = designation;
            Series = series;
            Family = family;
            IsInStandard = isInStandard;
            _values = values;
            Alias = alias;
        }

        /// <summary>
        /// Another designation of the section, also accepted by the search (e.g. the metric one of the AISC shapes: W1100X607 for W44X408);
        /// null if none
        /// </summary>
        public string Alias { get; }

        /// <summary>
        /// The catalog of the section
        /// </summary>
        public SectionCatalog Catalog { get; }

        /// <summary>
        /// The designation as published by the source (e.g. "HE 300 B", "IPE 300", "L 100 x 100 x 10")
        /// </summary>
        public string Designation { get; }

        /// <summary>
        /// The commercial series (e.g. "HE B", "IPE", "UPN", "L")
        /// </summary>
        public string Series { get; }

        /// <summary>
        /// The geometric family, that decides the section of Model (see <see cref="SectionMappings"/>)
        /// </summary>
        public SectionFamily Family { get; }

        /// <summary>
        /// False for the sections that the source lists outside the range of the standard of the catalog (producer range)
        /// </summary>
        public bool IsInStandard { get; }

        /// <summary>
        /// The dimensions and the published properties (mm units): only the ones given by the source
        /// </summary>
        public IReadOnlyDictionary<string, double> Values => _values;

        /// <summary>
        /// A dimension or a published property
        /// </summary>
        /// <param name="key">The name (e.g. "h", "tf", "A", "Iy")</param>
        /// <returns>The value in mm units, <see cref="double.NaN"/> if the source does not give it</returns>
        public double this[string key] => _values.TryGetValue(key, out double value) ? value : double.NaN;

        /// <summary>
        /// Tells if the source gives a value
        /// </summary>
        /// <param name="key">The name</param>
        /// <returns>True if the value is published</returns>
        public bool Has(string key) => _values.ContainsKey(key);

        /// <summary>
        /// The designation
        /// </summary>
        /// <returns>The designation</returns>
        public override string ToString() => Designation;
    }
}
