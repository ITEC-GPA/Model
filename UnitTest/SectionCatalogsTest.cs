using GPC.Model.Data.Sections;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Globalization;
using System.Text;

namespace UnitTest
{
    /// <summary>
    /// The catalogs of commercial sections and their map to the sections of Model: the properties of the sections of Model are compared with
    /// the published ones for every section of every catalog
    /// </summary>
    [TestClass]
    public class SectionCatalogsTest
    {
        public TestContext TestContext { get; set; } = null!;

        private const string ArcelorMittalSha256 = "84D01778D48973BC03670C77DBE6E17141B20F1BE449DC614F26CCEEE283C5B9";
        private const string PromozioneAcciaioSha256 = "92AA2618551E23476AC9F2167FC178E00A28D875092179BCA99AF25263C7BD66";
        private const string AiscSha256 = "82D0CEB96A0D938AE1A6BD9637CB10A1E269225B5D668DCE5B0BDC8D86013496";

        [TestMethod]
        public void TheAiscShapesAreFoundWithTheirManualAndMetricDesignations()
        {
            Assert.AreEqual(2299, SectionCatalogs.AISCShapesV16.Profiles.Count);
            CatalogProfile w = SectionCatalogs.AISCShapesV16.Find("W44X408")!;
            Assert.AreSame(w, SectionCatalogs.AISCShapesV16.Find("W1100X607"));
            Assert.AreEqual("W1100X607", w.Alias);
            Assert.AreEqual(SectionFamily.ParallelFlangeIH, w.Family);
            Assert.AreEqual(44.8 * 25.4, w["h"], 1e-9);
            Assert.AreEqual(38700 * Math.Pow(25.4, 4), w["Iy"], 1e-3);
            Assert.AreEqual(SectionFamily.DoubleAngle, SectionCatalogs.Find("2L8X6X1X3/8LLBB")!.Family);
            Assert.AreEqual(SectionFamily.CircularHollow, SectionCatalogs.Find("Pipe12STD")!.Family);
            Assert.AreEqual(SectionFamily.Tee, SectionCatalogs.Find("WT22X204")!.Family);
            // HP 360 x 174 is both European and American (AISC HP14X117): Find returns the European one, FindAll both
            Assert.AreEqual(2, SectionCatalogs.FindAll("HP 360 x 174").Count);
            Assert.AreSame(SectionCatalogs.EN10365ParallelFlangeIH, SectionCatalogs.Find("HP 360 x 174")!.Catalog);
        }

        [TestMethod]
        public void CatalogsAreReadWithTheirStandardAndSource()
        {
            foreach (SectionCatalog catalog in SectionCatalogs.All)
            {
                Assert.IsTrue(catalog.Profiles.Count > 0, catalog.Id);
                if (catalog == SectionCatalogs.AISCShapesV16)
                {
                    StringAssert.StartsWith(catalog.Standard, "AISC Steel Construction Manual, 16th Edition");
                    StringAssert.Contains(catalog.Source, "AISC Shapes Database v16.0");
                    Assert.AreEqual(AiscSha256, catalog.SourceSha256);
                    continue;
                }
                StringAssert.StartsWith(catalog.Standard, "EN 10", catalog.Id);
                bool arcelorMittal = catalog.Id.StartsWith("EN10365") || catalog.Id.StartsWith("EN10056");
                StringAssert.Contains(catalog.Source, arcelorMittal ? "ArcelorMittal" : "Fondazione Promozione Acciaio", catalog.Id);
                Assert.AreEqual(arcelorMittal ? ArcelorMittalSha256 : PromozioneAcciaioSha256, catalog.SourceSha256, catalog.Id);
            }

            Assert.AreEqual(624, SectionCatalogs.EN10365ParallelFlangeIH.Profiles.Count);
            Assert.AreEqual(31, SectionCatalogs.EN10365TaperFlangeI.Profiles.Count);
            Assert.AreEqual(48, SectionCatalogs.EN10365Channels.Profiles.Count);
            Assert.AreEqual(76, SectionCatalogs.EN10056Angles.Profiles.Count);
            Assert.AreEqual(237, SectionCatalogs.EN10210CircularHollow.Profiles.Count);
            Assert.AreEqual(255, SectionCatalogs.EN10210RectangularHollow.Profiles.Count);
            Assert.AreEqual(221, SectionCatalogs.EN10219CircularHollow.Profiles.Count);

            // the same designation in the catalogs of the hot finished and of the cold formed sections: both found, the hot finished first
            string common = SectionCatalogs.EN10210CircularHollow.Profiles.Select(p => p.Designation)
                .First(d => SectionCatalogs.EN10219CircularHollow.Find(d) != null);
            Assert.AreEqual(2, SectionCatalogs.FindAll(common).Count);
            Assert.AreSame(SectionCatalogs.EN10210CircularHollow, SectionCatalogs.Find(common)!.Catalog);

            // HE 300 B of EN 10365: h, b, tw, tf, r and the published properties in mm units
            CatalogProfile heb = SectionCatalogs.EN10365ParallelFlangeIH.Find("HE 300 B")!;
            Assert.AreEqual("HE B", heb.Series);
            Assert.AreEqual(SectionFamily.ParallelFlangeIH, heb.Family);
            Assert.IsTrue(heb.IsInStandard);
            Assert.AreEqual(300, heb["h"]);
            Assert.AreEqual(300, heb["b"]);
            Assert.AreEqual(11, heb["tw"]);
            Assert.AreEqual(19, heb["tf"]);
            Assert.AreEqual(27, heb["r"]);
            Assert.AreEqual(14910, heb["A"], 1e-9);
            Assert.AreEqual(251.6e6, heb["Iy"], 1e-3); // 25160 cm⁴ in the sales programme 2026 (25170 in older editions)
            Assert.AreEqual(1.687e12, heb["Iw"], 1e3);
            Assert.IsTrue(double.IsNaN(heb["r2"]));
            Assert.IsFalse(heb.Has("r2"));
        }

        [TestMethod]
        public void TheProducerSizesAreMarked()
        {
            Assert.IsFalse(SectionCatalogs.Find("IPE 750 x 220")!.IsInStandard);
            Assert.IsTrue(SectionCatalogs.Find("IPE 750 x 196")!.IsInStandard);
            Assert.IsFalse(SectionCatalogs.Find("L 200 x 100 x 16")!.IsInStandard);
            Assert.IsTrue(SectionCatalogs.Find("L 200 x 100 x 15")!.IsInStandard);
        }

        [TestMethod]
        public void DesignationsAreFoundInTheCommonSpellings()
        {
            CatalogProfile heb = SectionCatalogs.Find("HE 300 B")!;
            foreach (string spelling in new[] { "HEB 300", "HEB300", "heb 300", "HE300B", "HE-300-B" })
                Assert.AreSame(heb, SectionCatalogs.Find(spelling), spelling);
            Assert.AreSame(SectionCatalogs.Find("HE 100 AA"), SectionCatalogs.Find("HEAA 100"));
            Assert.AreSame(SectionCatalogs.Find("L 100 x 100 x 10"), SectionCatalogs.Find("L100x10"));
            Assert.AreSame(SectionCatalogs.Find("L 100 x 100 x 10"), SectionCatalogs.Find("L 100×100×10"));
            Assert.AreEqual("UPN", SectionCatalogs.Find("UPN 300")!.Series);
            Assert.AreEqual(SectionFamily.TaperFlangeChannel, SectionCatalogs.Find("UPN 300")!.Family);
            Assert.AreEqual(SectionFamily.ParallelFlangeChannel, SectionCatalogs.Find("UPE 300")!.Family);
            Assert.IsNull(SectionCatalogs.Find("HEB 301"));
            Assert.AreEqual(1, SectionCatalogs.FindAll("IPE 300").Count);
        }

        [TestMethod]
        public void EveryFamilyHasAMapping()
        {
            foreach (SectionFamily family in Enum.GetValues(typeof(SectionFamily)))
            {
                SectionMapping mapping = SectionMappings.For(family);
                Assert.AreEqual(mapping.Fidelity == MappingFidelity.NotSupported, mapping.ModelType is null, family.ToString());
                Assert.IsFalse(string.IsNullOrWhiteSpace(mapping.Notes), family.ToString());
            }
            Assert.ThrowsException<ArgumentException>(() => SectionMappings.For(SectionFamily.Angle).Create(SectionCatalogs.Find("IPE 300")!));
            Assert.ThrowsException<KeyNotFoundException>(() => SectionMappings.CreateSection("XYZ 123"));
        }

        [TestMethod]
        public void RolledSectionsHaveTheirFillets()
        {
            // the section of the map is the rolled one: its area includes the fillets (HE 300 B: 149.1 cm² against 144.1 without)
            Section heb = SectionMappings.CreateSection("HEB 300");
            Assert.IsInstanceOfType(heb, typeof(SectionH));
            Assert.AreEqual("HE 300 B", heb.Name);
            Assert.AreEqual(14910, heb.Area, 0.0005 * 14910);
            var sharp = new SectionH(300, 11, 300, 19, 300, 19, string.Empty, 27);
            Assert.AreEqual(4 * (1 - Math.PI / 4) * 27 * 27, heb.Area - sharp.Area, 1e-6);
        }

        /// <summary>
        /// The properties of the section of Model against the published ones for every section of the catalogs: the maximum deviations of
        /// each family are written in the output and must be within the tolerances of the fidelity of the mapping
        /// </summary>
        [TestMethod]
        public void PropertiesOfModelAgainstThePublishedOnes()
        {
            var deviations = new Dictionary<(string, string), (double Max, string Where, double Sum, int Count)>();
            var errors = new List<string>();
            var inconsistent = new List<string>();
            var inconsistentPipes = new List<string>();
            foreach (CatalogProfile profile in SectionCatalogs.All.SelectMany(c => c.Profiles))
            {
                if (SectionMappings.For(profile.Family).Fidelity == MappingFidelity.NotSupported)
                    continue;
                Section section;
                try
                {
                    section = SectionMappings.CreateSection(profile);
                }
                catch (Exception e)
                {
                    errors.Add($"{profile.Designation}: {e.GetType().Name} {e.Message}");
                    continue;
                }

                // the published area of a parallel flange section must be the one of its published dimensions (root fillets included):
                // for some sections of the source it is not (e.g. UB 610 x 229 x 101: r = 20 published, area of r = 12.7), they are
                // listed and not used to measure the fidelity of Model
                if (profile.Family == SectionFamily.ParallelFlangeIH)
                {
                    double geometricArea = 2 * profile["b"] * profile["tf"] + (profile["h"] - 2 * profile["tf"]) * profile["tw"] +
                        (4 - Math.PI) * profile["r"] * profile["r"];
                    if (Math.Abs(geometricArea / profile["A"] - 1) > 0.0035 && profile.Catalog != SectionCatalogs.AISCShapesV16)
                    {
                        inconsistent.Add(profile.Designation);
                        continue;
                    }
                }

                // the published area of the AISC pipes must be the one of the published design thickness: not for some sizes (e.g. the
                // XS pipes from 12 to 26 in are 3% smaller, the Pipe10STD 3% bigger), listed and not used to measure the fidelity
                if (profile.Series == "PIPE" && Math.Abs(Math.PI * (profile["D"] - profile["t"]) * profile["t"] / profile["A"] - 1) > 0.005)
                {
                    inconsistentPipes.Add(profile.Designation);
                    continue;
                }

                foreach ((string key, double value) in ModelValues(profile, section))
                {
                    double published = profile[key];
                    if (double.IsNaN(published) || published == 0)
                        continue;
                    double deviation = (value - published) / Math.Abs(published);
                    var id = (Group(profile), key);
                    deviations.TryGetValue(id, out var d);
                    deviations[id] = Math.Abs(deviation) > Math.Abs(d.Max) || d.Count == 0
                        ? (deviation, profile.Designation, d.Sum + Math.Abs(deviation), d.Count + 1)
                        : (d.Max, d.Where, d.Sum + Math.Abs(deviation), d.Count + 1);
                }
            }

            var report = new StringBuilder("Family;Property;Sections;Max deviation;Section;Mean |deviation|;Tolerance\n");
            foreach (var entry in deviations.OrderBy(e => e.Key.Item1).ThenBy(e => e.Key.Item2))
                report.AppendLine(string.Format(CultureInfo.InvariantCulture, "{0};{1};{2};{3:P3};{4};{5:P3};{6:P1}", entry.Key.Item1, entry.Key.Item2,
                    entry.Value.Count, entry.Value.Max, entry.Value.Where, entry.Value.Sum / entry.Value.Count,
                    Tolerance(entry.Key.Item1, entry.Key.Item2)));
            report.AppendLine($"Published area not consistent with the published dimensions ({inconsistent.Count}): {string.Join(", ", inconsistent)}");
            report.AppendLine($"AISC pipes with the published area not consistent with the design thickness ({inconsistentPipes.Count}): " +
                string.Join(", ", inconsistentPipes));
            foreach (string error in errors)
                report.AppendLine("ERROR;" + error);
            TestContext.WriteLine(report.ToString());
            File.WriteAllText(Path.Combine(Path.GetTempPath(), "gpc-section-catalogs-deviations.csv"), report.ToString());
            Assert.AreEqual(0, errors.Count, string.Join(Environment.NewLine, errors));

            foreach (var entry in deviations)
            {
                double tolerance = Tolerance(entry.Key.Item1, entry.Key.Item2);
                Assert.IsTrue(Math.Abs(entry.Value.Max) <= tolerance,
                    $"{entry.Key.Item1} {entry.Key.Item2}: {entry.Value.Max:P3} ({entry.Value.Where}) beyond {tolerance:P2}");
            }
            Assert.AreEqual(InconsistentSourceSections, inconsistent.Count, string.Join(", ", inconsistent));
            Assert.AreEqual(18, inconsistentPipes.Count, string.Join(", ", inconsistentPipes));
        }

        /// <summary>
        /// The parallel flange sections of the ArcelorMittal sales programme V2026-1 whose published area is not the one of their published
        /// dimensions (more than 0.35%): UB, UC, UBP, HP, HD, HL and HLZ sizes with a published root radius different from the one of their
        /// properties or with rounded thicknesses
        /// </summary>
        private const int InconsistentSourceSections = 127;

        /// <summary>
        /// The group of the fidelity: the family, the J series apart (BS joists, geometric convention of the source not documented)
        /// </summary>
        private static string Group(CatalogProfile profile) =>
            profile.Catalog == SectionCatalogs.AISCShapesV16 ? "AISC " + profile.Series
            : profile.Series == "J" ? profile.Family + " J" : profile.Family.ToString();

        /// <summary>
        /// The properties of the section of Model with the names of the catalogs (axis y-y of the Eurocodes = X of Model, z-z = Y)
        /// </summary>
        private static IEnumerable<(string, double)> ModelValues(CatalogProfile profile, Section section)
        {
            yield return ("A", section.Area);
            if (profile.Family == SectionFamily.CircularHollow)
            {
                yield return ("I", section.Jxx);
                yield return ("Wel", section.WelX);
                yield return ("Wpl", section.WplX);
                yield return ("It", section.Jt);
                yield break;
            }
            // the torsion modulus of EN 10210-2 (the C of AISC is another quantity: 2 (b - t)(h - t) t - 4.5 (4 - π) t³)
            if (profile.Family == SectionFamily.RectangularHollow && profile.Catalog != SectionCatalogs.AISCShapesV16)
                yield return ("Ct", ((SectionRHSRoundedCorners)section).TorsionModulus);
            if (profile.Family == SectionFamily.Tee)
                yield return ("zs", section.Height - section.Centroid.Y); // from the outer face of the flange
            if (profile.Family == SectionFamily.DoubleAngle)
            {
                yield return ("Iy", section.Jxx);
                yield return ("Iz", section.Jyy);
                yield return ("Wely", section.WelX);
                yield return ("Welz", section.WelY);
                yield return ("Wply", section.WplX);
                yield return ("Wplz", section.WplY);
                yield return ("zs", section.Centroid.Y);
                double dx = section.ShearCenter.X - section.Centroid.X, dy = section.ShearCenter.Y - section.Centroid.Y;
                yield return ("ro", Math.Sqrt(dx * dx + dy * dy + (section.Jxx + section.Jyy) / section.Area));
                yield break;
            }
            yield return ("Iy", section.Jxx);
            yield return ("Iz", section.Jyy);
            yield return ("Wely", section.WelX);
            yield return ("Welz", section.WelY);
            if (profile.Family != SectionFamily.Angle)
            {
                yield return ("Wply", section.WplX);
                yield return ("Wplz", section.WplY);
                yield return ("It", section.Jt);
                yield return ("Iw", section.Jw);
                if (profile.Family == SectionFamily.ParallelFlangeChannel || profile.Family == SectionFamily.TaperFlangeChannel)
                {
                    // ys: centroid from the back of the web; ym: distance of the shear centre (outside the web, at negative X) from the
                    // centroid (ym = ys + e for the UPE 300 of the source: 29 + 31.4 = 60 mm); eo (AISC): shear centre from the back of the web
                    yield return ("ys", section.Centroid.X);
                    yield return ("ym", section.Centroid.X - section.ShearCenter.X);
                    yield return ("eo", -section.ShearCenter.X);
                }
            }
            else
            {
                yield return ("Iu", section.J11);
                yield return ("Iv", section.J22);
                yield return ("Iyz", section.Jxy);
                yield return ("zs", section.Centroid.Y);
                yield return ("ys", section.Centroid.X);
            }
        }

        /// <summary>
        /// The accepted deviation, from the maximum measured on 30/09/2026 (the published values have 3 or 4 significant digits). Area, moments
        /// of inertia and moduli come from the exact geometry; the torsion and warping constants of the channels and of the taper flange
        /// sections come from the thin wall formulas of Model (It up to 20% less, Iw up to 18% more than the published ones)
        /// </summary>
        private static double Tolerance(string group, string property)
        {
            var tolerances = new Dictionary<string, (double Area, double Inertia, double Elastic, double Plastic, double Torsion, double Warping, double Position)>
            {
                ["ParallelFlangeIH"] = (0.0035, 0.0012, 0.0012, 0.0012, 0.002, 0.018, 0),
                ["TaperFlangeI"] = (0.004, 0.0055, 0.0055, 0.01, 0.12, 0.17, 0),
                ["TaperFlangeI J"] = (0.02, 0.03, 0.03, 0.025, 0.21, 0.18, 0),
                ["ParallelFlangeChannel"] = (0.0035, 0.004, 0.004, 0.025, 0.17, 0.08, 0.025),
                // the shear centre of the UPN from the thin wall formula of the parallel flanges: up to 9% far from the published one
                ["TaperFlangeChannel"] = (0.004, 0.009, 0.0075, 0.021, 0.12, 0.175, 0.095),
                ["Angle"] = (0.009, 0.016, 0.0155, 0, 0, 0, 0.02),
                ["CircularHollow"] = (0.0045, 0.0035, 0.004, 0.0045, 0.0035, 0, 0),
                // the moments of inertia of the thick walls of small sizes (t / b = 0.2) up to 1.3% from the published values (the exact
                // geometry of Model is checked against the integration of the outline in SectionRHSRoundedCornersTest)
                ["RectangularHollow"] = (0.0045, 0.013, 0.012, 0.006, 0.004, 0, 0),
                // AISC: fillets of W and WT from kdes, of M, HP, MT and of the taper shapes equivalent to the published area (area exact by
                // construction); S, ST, C with the slope 1/6 and MC with the slope of the published centroid: Iz up to 4.6% from the published
                // one; angles and double angles without radii as the published area (Iv of the thin angles up to 6%); It of S, ST, C, MC and
                // Iw of S from the thin wall formulas
                ["AISC W"] = (0.008, 0.015, 0.013, 0.011, 0.012, 0.035, 0),
                ["AISC M"] = (0.001, 0.01, 0.007, 0.007, 0.03, 0.02, 0),
                ["AISC HP"] = (0.001, 0.013, 0.01, 0.008, 0.021, 0.016, 0),
                ["AISC S"] = (0.001, 0.037, 0.036, 0.015, 0.22, 0.23, 0),
                ["AISC C"] = (0.001, 0.043, 0.046, 0.013, 0.2, 0.027, 0.009),
                ["AISC MC"] = (0.001, 0.04, 0.041, 0.007, 0.16, 0.029, 0.007),
                ["AISC L"] = (0.014, 0.06, 0.029, 0, 0, 0, 0.024),
                ["AISC 2L"] = (0.014, 0.031, 0.031, 0.036, 0, 0, 0.024),
                ["AISC WT"] = (0.008, 0.017, 0.013, 0.013, 0.024, 0.015, 0.008),
                ["AISC MT"] = (0.001, 0.007, 0.005, 0.008, 0.038, 0.004, 0.025),
                ["AISC ST"] = (0.001, 0.037, 0.039, 0.016, 0.19, 0.013, 0.008),
                ["AISC HSS"] = (0.004, 0.0095, 0.0075, 0.0055, 0.046, 0, 0),
                ["AISC HSS round"] = (0.0055, 0.005, 0.005, 0.005, 0.0065, 0, 0),
                // the moments of inertia of the smallest pipes are published with 3 digits (Pipe1/2XS 2.1%)
                ["AISC PIPE"] = (0.005, 0.022, 0.011, 0.011, 0.022, 0, 0),
            };
            var t = tolerances[group];
            switch (property)
            {
                case "A": return t.Area;
                case "I": case "Iy": case "Iz": case "Iu": case "Iv": case "Iyz": return t.Inertia;
                case "Wel": case "Wely": case "Welz": return t.Elastic;
                case "Wpl": case "Wply": case "Wplz": return t.Plastic;
                case "It": case "Ct": return t.Torsion;
                case "Iw": return t.Warping;
                case "ys": case "zs": case "ym": case "eo": case "ro": return t.Position;
                default: throw new ArgumentException(property);
            }
        }
    }
}
