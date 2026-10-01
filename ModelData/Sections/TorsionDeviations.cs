using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace GPC.Model.Data.Sections
{
    /// <summary>
    /// A catalog profile whose torsion constant or warping constant computed by the formulas of its section of Model deviates from the
    /// published value more than <see cref="TorsionDeviations.Threshold"/>; with the values solved with the finite elements on the exact outline
    /// (<see cref="Model.Sections.Section.CalculateTorsionProperties"/>) for comparison
    /// </summary>
    public sealed class TorsionDeviation
    {
        internal TorsionDeviation(string catalogId, string designation, string series, double itPublished, double itModel, double itNumerical,
            double iwPublished, double iwModel, double iwNumerical)
        {
            CatalogId = catalogId;
            Designation = designation;
            Series = series;
            ItPublished = itPublished;
            ItModel = itModel;
            ItNumerical = itNumerical;
            IwPublished = iwPublished;
            IwModel = iwModel;
            IwNumerical = iwNumerical;
        }

        /// <summary>The identifier of the catalog (see <see cref="SectionCatalog.Id"/>)</summary>
        public string CatalogId { get; }

        /// <summary>The designation of the profile</summary>
        public string Designation { get; }

        /// <summary>The series</summary>
        public string Series { get; }

        /// <summary>The published torsion constant (NaN if not published)</summary>
        public double ItPublished { get; }

        /// <summary>The torsion constant of the formulas of the section of Model</summary>
        public double ItModel { get; }

        /// <summary>The torsion constant solved with the finite elements (NaN if not solved)</summary>
        public double ItNumerical { get; }

        /// <summary>The published warping constant (NaN if not published)</summary>
        public double IwPublished { get; }

        /// <summary>The warping constant of the formulas of the section of Model</summary>
        public double IwModel { get; }

        /// <summary>The warping constant solved with the finite elements (NaN if not solved)</summary>
        public double IwNumerical { get; }

        /// <summary>The relative deviation of the torsion constant of Model from the published one</summary>
        public double ItModelDeviation => ItModel / ItPublished - 1;

        /// <summary>The relative deviation of the numerical torsion constant from the published one</summary>
        public double ItNumericalDeviation => ItNumerical / ItPublished - 1;

        /// <summary>The relative deviation of the warping constant of Model from the published one</summary>
        public double IwModelDeviation => IwModel / IwPublished - 1;

        /// <summary>The relative deviation of the numerical warping constant from the published one</summary>
        public double IwNumericalDeviation => IwNumerical / IwPublished - 1;

        /// <summary>True if the torsion constant of Model deviates more than the threshold</summary>
        public bool ItExceeds => Math.Abs(ItModelDeviation) > TorsionDeviations.Threshold;

        /// <summary>True if the warping constant of Model deviates more than the threshold</summary>
        public bool IwExceeds => Math.Abs(IwModelDeviation) > TorsionDeviations.Threshold;

        /// <summary>
        /// The description
        /// </summary>
        /// <returns>The designation and the deviations</returns>
        public override string ToString() => $"{CatalogId} {Designation}: It {ItModelDeviation:P1}, Iw {IwModelDeviation:P1}";
    }

    /// <summary>
    /// The trace of the catalog profiles whose torsion constant It or warping constant Iw computed by the formulas of Model deviate from the
    /// published values more than <see cref="Threshold"/> (the formulas are kept: see ModelData/Sections/README.md). Read from the CSV embedded in
    /// the assembly, generated and checked by the test SectionCatalogsTest.TorsionDeviationsAreTraced
    /// </summary>
    public static class TorsionDeviations
    {
        /// <summary>
        /// The acceptable relative deviation from the published values (they are rounded to 3-4 significant digits)
        /// </summary>
        public const double Threshold = 0.05;

        private static readonly Lazy<(IReadOnlyList<TorsionDeviation> Rows, IReadOnlyDictionary<string, string> Metadata)> _trace =
            new Lazy<(IReadOnlyList<TorsionDeviation>, IReadOnlyDictionary<string, string>)>(Read);

        /// <summary>
        /// The profiles of the trace
        /// </summary>
        public static IReadOnlyList<TorsionDeviation> All => _trace.Value.Rows;

        /// <summary>
        /// The header lines of the trace ("key: value")
        /// </summary>
        public static IReadOnlyDictionary<string, string> Metadata => _trace.Value.Metadata;

        /// <summary>
        /// The deviation of a profile
        /// </summary>
        /// <param name="profile">The profile</param>
        /// <returns>The deviation, null if the profile is within the threshold or has no published torsion constants</returns>
        public static TorsionDeviation Find(CatalogProfile profile) =>
            profile is null ? null : All.FirstOrDefault(d => d.CatalogId == profile.Catalog.Id && d.Designation == profile.Designation);

        private static (IReadOnlyList<TorsionDeviation>, IReadOnlyDictionary<string, string>) Read()
        {
            var metadata = new Dictionary<string, string>();
            var rows = new List<TorsionDeviation>();
            string resource = typeof(TorsionDeviations).Assembly.GetManifestResourceNames()
                .SingleOrDefault(n => n.EndsWith(".TorsionDeviations.csv", StringComparison.Ordinal))
                ?? throw new InvalidOperationException("The trace of the torsion deviations is not embedded in the assembly");
            using (Stream stream = typeof(TorsionDeviations).Assembly.GetManifestResourceStream(resource))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                string line;
                bool header = true;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.Length == 0)
                        continue;
                    if (line.StartsWith("#", StringComparison.Ordinal))
                    {
                        string text = line.Substring(1).Trim();
                        int colon = text.IndexOf(": ", StringComparison.Ordinal);
                        if (colon > 0)
                            metadata[text.Substring(0, colon)] = text.Substring(colon + 2);
                        continue;
                    }
                    if (header)
                    {
                        header = false;
                        continue;
                    }
                    string[] c = line.Split(',');
                    if (c.Length != 9)
                        throw new InvalidDataException($"TorsionDeviations: 9 values expected in \"{line}\"");
                    double V(int i) => c[i].Length == 0 ? double.NaN : double.Parse(c[i], NumberStyles.Float, CultureInfo.InvariantCulture);
                    rows.Add(new TorsionDeviation(c[0], c[1], c[2], V(3), V(4), V(5), V(6), V(7), V(8)));
                }
            }
            return (rows.AsReadOnly(), metadata);
        }
    }
}
