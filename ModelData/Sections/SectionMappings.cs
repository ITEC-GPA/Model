using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Data.Sections
{
    /// <summary>
    /// How the sections of a geometric family are represented in Model: the section class, how the values of the catalog fill it and how
    /// faithful the representation is
    /// </summary>
    public sealed class SectionMapping
    {
        private readonly Func<CatalogProfile, Section> _factory;

        internal SectionMapping(SectionFamily family, Type modelType, MappingFidelity fidelity, string parameters, string notes,
            Func<CatalogProfile, Section> factory)
        {
            Family = family;
            ModelType = modelType;
            Fidelity = fidelity;
            Parameters = parameters;
            Notes = notes;
            _factory = factory;
        }

        /// <summary>
        /// The geometric family
        /// </summary>
        public SectionFamily Family { get; }

        /// <summary>
        /// The section class of Model (null when <see cref="Fidelity"/> is <see cref="MappingFidelity.NotSupported"/>)
        /// </summary>
        public Type ModelType { get; }

        /// <summary>
        /// How faithful the representation is
        /// </summary>
        public MappingFidelity Fidelity { get; }

        /// <summary>
        /// How the values of the catalog fill the section (e.g. "SectionH(h, tw, b, tf, b, tf, r), rolled")
        /// </summary>
        public string Parameters { get; }

        /// <summary>
        /// The simplifications and the documented deviations
        /// </summary>
        public string Notes { get; }

        /// <summary>
        /// Creates the section of Model with its properties calculated
        /// </summary>
        /// <param name="profile">The commercial section</param>
        /// <returns>The section, named with the designation</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="profile"/> is null</exception>
        /// <exception cref="ArgumentException">If the section is of another family</exception>
        /// <exception cref="NotSupportedException">If Model has no section for the family</exception>
        public Section Create(CatalogProfile profile)
        {
            if (profile is null)
                throw new ArgumentNullException(nameof(profile));
            if (profile.Family != Family)
                throw new ArgumentException($"{profile.Designation} is a {profile.Family}, not a {Family}", nameof(profile));
            if (_factory is null)
                throw new NotSupportedException($"Model has no section for the family {Family} ({profile.Designation})");
            return _factory(profile);
        }

        /// <summary>
        /// The family and the section of Model
        /// </summary>
        /// <returns>The description</returns>
        public override string ToString() => $"{Family} -> {ModelType?.Name ?? "not supported"} ({Fidelity})";
    }

    /// <summary>
    /// The map between the commercial sections of the catalogs (<see cref="SectionCatalogs"/>) and the sections of Model: one mapping for
    /// each geometric family. The hot rolled sections are created with their root fillets (and toe radii where Model represents them)
    /// </summary>
    public static class SectionMappings
    {
        private static readonly SectionMapping[] _mappings =
        {
            new SectionMapping(SectionFamily.ParallelFlangeIH, typeof(SectionH), MappingFidelity.Exact,
                "SectionH(h, tw, b, tf, b, tf, r), rolled (root fillets r); AISC W r = kdes - tf, M and HP r equivalent to the published area " +
                "(AISC does not publish the radius of its properties; kdes is a design value of the local checks)",
                "Area, moments of inertia and elastic moduli of the exact geometry; torsion and warping constants of Model. The AISC M shape " +
                "with sloped flanges is represented with parallel flanges",
                p => Rolled(new SectionH(p["h"], p["tw"], p["b"], p["tf"], p["b"], p["tf"], p.Designation, FilletI(p)))),
            new SectionMapping(SectionFamily.TaperFlangeI, typeof(SectionHTaperFlange), MappingFidelity.Exact,
                "SectionHTaperFlange(h, tw, b, tf, slope, r1, r2, point): IPN slope 14% (declared by the source) with tf at b / 4 from the tip " +
                "of the flange (DIN 1025-1); J slope 8° with tf at the middle of the outstand; AISC S slope 1/6 with the average tf (at the " +
                "middle of the outstand), r1 equivalent to the published area, no toe radius",
                "IPN, S: area, moments of inertia and moduli of the exact geometry (taper flanges, root fillets, toe radii). J (BS joists, producer " +
                "range): the published properties are reproduced within 1.3% (area) and 1% (moments of inertia), the geometric convention of the " +
                "source is not documented. Torsion and warping constants of Model",
                p => IsAisc(p) ? AiscTaper(p, true, 1.0 / 6.0)
                    : new SectionHTaperFlange(p["h"], p["tw"], p["b"], p["tf"], TaperSlopeI(p), p["r1"], Or0(p["r2"]),
                        p.Series == "IPN" ? p["b"] / 4.0 : (p["b"] - p["tw"]) / 4.0, p.Designation)),
            new SectionMapping(SectionFamily.ParallelFlangeChannel, typeof(SectionC), MappingFidelity.Exact,
                "SectionC(h, tw, b, tf, b, tf, r1, r2), rolled (root fillets r1, toe radii r2)",
                "Area, moments of inertia and moduli of the exact geometry; torsion and warping constants of Model",
                p => Rolled(new SectionC(p["h"], p["tw"], p["b"], p["tf"], p["b"], p["tf"], p.Designation, p["r1"], Or0(p["r2"])))),
            new SectionMapping(SectionFamily.TaperFlangeChannel, typeof(SectionCTaperFlange), MappingFidelity.Exact,
                "SectionCTaperFlange(h, tw, b, tf, slope, r1, r2, point): UPN h <= 300 slope 8% with tf at b / 2 from the tip of the flange, " +
                "h > 300 slope 5% with tf at the middle of the outstand (DIN 1026-1, checked on the published areas); AISC C slope 1/6, MC the " +
                "slope that gives the published centroid (the slope of the MC shapes varies and is not published), both with the average tf (at " +
                "the middle of the outstand), r1 equivalent to the published area, no toe radius",
                "Area, moments of inertia and moduli of the exact geometry (taper flanges, root fillets, toe radii); torsion and warping constants " +
                "and shear centre of Model",
                p => IsAisc(p) ? AiscTaper(p, false, 1.0 / 6.0)
                    : new SectionCTaperFlange(p["h"], p["tw"], p["b"], p["tf"], p["h"] <= 300 ? 0.08 : 0.05, p["r1"], Or0(p["r2"]),
                        p["h"] <= 300 ? p["b"] / 2.0 : (p["b"] - p["tw"]) / 2.0, p.Designation)),
            new SectionMapping(SectionFamily.Angle, typeof(SectionL), MappingFidelity.Exact,
                "SectionL(b, t, h, t, r1, r2), rolled: horizontal leg b along y, vertical leg h along z, root fillet r1, toe radii r2 " +
                "(r2 = r1 / 2 of EN 10056-1 when the source does not publish it; AISC without radii, as its published properties)",
                "Area, centroid, moments and product of inertia of the exact geometry; torsion and warping constants of Model. The radius r3 " +
                "published for a few sizes is not represented",
                p => Angle(p)),
            new SectionMapping(SectionFamily.Tee, typeof(SectionT), MappingFidelity.Exact,
                "SectionT(h, b, tw, tf, r), rolled: flange at the top; AISC WT r = kdes - tf, MT r equivalent to the published area; " +
                "ST SectionTTaperFlange slope 1/6 with the average tf, r1 equivalent to the published area",
                "Area, moments of inertia and moduli of the exact geometry; torsion and warping constants of Model",
                p => Tee(p)),
            new SectionMapping(SectionFamily.CircularHollow, typeof(SectionCHS), MappingFidelity.Exact,
                "SectionCHS(D, t): AISC t = tdes; D of the round HSS from the designation (the column OD is rounded)",
                "Exact properties of the annulus (torsion constant 2 I)",
                p => new SectionCHS(p.Series == "HSS round" ? DiameterOfTheDesignation(p) : p["D"], p["t"], p.Designation)),
            new SectionMapping(SectionFamily.RectangularHollow, typeof(SectionRHSRoundedCorners), MappingFidelity.Exact,
                "SectionRHSRoundedCorners(h, b, t, ro, ri): corner radii of the calculations of the standard, EN 10210-2 ro = 1.5 t, ri = t; " +
                "AISC t = tdes, ro = 2 t, ri = t",
                "Area, moments of inertia and moduli of the exact geometry with the rounded corners; torsion constant and modulus of EN 10210-2",
                p => IsAisc(p)
                    ? new SectionRHSRoundedCorners(p["h"], p["b"], p["t"], 2.0 * p["t"], p["t"], p.Designation)
                    : new SectionRHSRoundedCorners(p["h"], p["b"], p["t"], 1.5 * p["t"], p["t"], p.Designation)),
            new SectionMapping(SectionFamily.DoubleAngle, typeof(SectionBuiltUp), MappingFidelity.Exact,
                "SectionBuiltUp.DoubleAngle(L, s): the single angle of the same catalog (LLBB: long legs back to back, SLBB: short legs back " +
                "to back), vertical legs back to back at the spacing s",
                "Area, moments of inertia and moduli of the two angles; torsion constant the sum of the two; warping constant not available",
                p => DoubleAngle(p)),
        };

        /// <summary>
        /// The mappings of all the families
        /// </summary>
        public static IReadOnlyList<SectionMapping> All => _mappings;

        /// <summary>
        /// The mapping of a family
        /// </summary>
        /// <param name="family">The family</param>
        /// <returns>The mapping</returns>
        public static SectionMapping For(SectionFamily family) => _mappings.Single(m => m.Family == family);

        /// <summary>
        /// Creates the section of Model of a commercial section (see <see cref="SectionMapping.Create"/>)
        /// </summary>
        /// <param name="profile">The commercial section</param>
        /// <returns>The section with its properties calculated</returns>
        public static Section CreateSection(CatalogProfile profile)
        {
            if (profile is null)
                throw new ArgumentNullException(nameof(profile));
            return For(profile.Family).Create(profile);
        }

        /// <summary>
        /// Creates the section of Model of a designation (see <see cref="SectionCatalogs.Find"/>)
        /// </summary>
        /// <param name="designation">The designation (e.g. "HEB 300")</param>
        /// <returns>The section with its properties calculated</returns>
        /// <exception cref="KeyNotFoundException">If no catalog contains the designation</exception>
        public static Section CreateSection(string designation)
        {
            CatalogProfile profile = SectionCatalogs.Find(designation) ??
                throw new KeyNotFoundException($"The section {designation} is not in the catalogs");
            return CreateSection(profile);
        }

        /// <summary>
        /// Creates the steel section of a commercial section: hot rolled, hot finished
        /// </summary>
        /// <param name="profile">The commercial section</param>
        /// <param name="material">The steel</param>
        /// <returns>The steel section</returns>
        public static SteelSection CreateSteelSection(CatalogProfile profile, SteelMaterial material)
        {
            return new SteelSection(CreateSection(profile), material, Section.SectionTypes.Rolled, Section.FormedTypes.HotFinished);
        }

        /// <summary>
        /// A hot rolled section: fillets in the inside corners, properties calculated again
        /// </summary>
        private static Section Rolled(Section section)
        {
            section.SetEdgeTypeFromSteelType(Section.SectionTypes.Rolled);
            section.SetMechanicalProperties();
            return section;
        }

        private static double Or0(double value) => double.IsNaN(value) ? 0.0 : value;

        private static bool IsAisc(CatalogProfile profile) => profile.Catalog.Id == SectionCatalogs.AiscId;

        /// <summary>
        /// The root fillet of a parallel flange I section: the published one (EN), kdes - tf (AISC W), the one of the published area (AISC M,
        /// HP: 4 fillets of area (1 - π / 4) r²)
        /// </summary>
        private static double FilletI(CatalogProfile p)
        {
            if (!IsAisc(p))
                return p["r"];
            if (p.Series == "W")
                return p["kdes"] - p["tf"];
            double sharp = 2.0 * p["b"] * p["tf"] + (p["h"] - 2.0 * p["tf"]) * p["tw"];
            return Math.Sqrt(Math.Max(0.0, p["A"] - sharp) / (4.0 - Math.PI));
        }

        /// <summary>
        /// A rolled tee (AISC): WT r = kdes - tf, MT r of the published area (2 fillets), ST taper flange
        /// </summary>
        private static Section Tee(CatalogProfile p)
        {
            if (p.Series == "ST")
            {
                double point = (p["b"] - p["tw"]) / 4.0;
                double rMax = 0.45 * Math.Min(p["b"] - p["tw"], p["h"] - p["tf"]);
                double r1 = RadiusOfTheArea(r => new SectionTTaperFlange(p["h"], p["b"], p["tw"], p["tf"], 1.0 / 6.0, r, 0.0, point, p.Designation),
                    p["A"], rMax);
                return new SectionTTaperFlange(p["h"], p["b"], p["tw"], p["tf"], 1.0 / 6.0, r1, 0.0, point, p.Designation);
            }
            double sharp = p["b"] * p["tf"] + (p["h"] - p["tf"]) * p["tw"];
            double fillet = p.Series == "WT" ? p["kdes"] - p["tf"] : Math.Sqrt(Math.Max(0.0, p["A"] - sharp) / (2.0 - Math.PI / 2.0));
            return Rolled(new SectionT(p["h"], p["b"], p["tw"], p["tf"], p.Designation, fillet));
        }

        /// <summary>
        /// An AISC shape with taper flanges (S, C, MC): tf is the average thickness, i.e. the one at the middle of the outstand; the root fillet
        /// r1 gives the published area; the slope is 1/6 for S and C, for MC the one that gives the published centroid (it varies from shape to
        /// shape); no toe radius
        /// </summary>
        private static Section AiscTaper(CatalogProfile p, bool iSection, double slope)
        {
            double outstand = iSection ? (p["b"] - p["tw"]) / 2.0 : p["b"] - p["tw"];
            double point = outstand / 2.0;
            double rMax = 0.45 * Math.Min(outstand, p["h"] / 2.0 - p["tf"]);
            Func<double, double, Section> make = (s, r) => iSection
                ? (Section)new SectionHTaperFlange(p["h"], p["tw"], p["b"], p["tf"], s, r, 0.0, point, p.Designation)
                : new SectionCTaperFlange(p["h"], p["tw"], p["b"], p["tf"], s, r, 0.0, point, p.Designation);

            double r1 = RadiusOfTheArea(r => make(slope, r), p["A"], rMax);
            if (p.Series == "MC")
            {
                // the centroid moves towards the web when the slope grows: slope and radius alternated until both are stable; the slope
                // leaves at least 5% of tf at the tips
                double maxSlope = Math.Min(0.25, 0.95 * p["tf"] / point);
                for (int k = 0; k < 4; k++)
                {
                    double radius = r1;
                    slope = Bisection(s => make(s, radius).Centroid.X - p["ys"], 0.0, maxSlope, false);
                    r1 = RadiusOfTheArea(r => make(slope, r), p["A"], rMax);
                }
            }
            return make(slope, r1);
        }

        /// <summary>
        /// The root radius that gives the published area (0 when the section without fillets is already bigger)
        /// </summary>
        private static double RadiusOfTheArea(Func<double, Section> make, double area, double rMax)
        {
            if (make(0.0).Area >= area)
                return 0.0;
            return Bisection(r => make(r).Area - area, 0.0, rMax, true);
        }

        /// <summary>
        /// The root of a monotonic function by bisection (the ends when it has no root in the interval)
        /// </summary>
        private static double Bisection(Func<double, double> f, double low, double high, bool increasing)
        {
            double sign = increasing ? 1.0 : -1.0;
            if (sign * f(high) <= 0)
                return high;
            if (sign * f(low) >= 0)
                return low;
            for (int k = 0; k < 40; k++)
            {
                double middle = 0.5 * (low + high);
                if (sign * f(middle) < 0)
                    low = middle;
                else
                    high = middle;
            }
            return 0.5 * (low + high);
        }

        /// <summary>
        /// The outside diameter of a round HSS from its designation (HSS10.750X0.250: 10.750 in)
        /// </summary>
        private static double DiameterOfTheDesignation(CatalogProfile p) =>
            double.Parse(p.Designation.Substring(3).Split('X')[0], System.Globalization.CultureInfo.InvariantCulture) * 25.4;

        /// <summary>
        /// A rolled angle: EN r1, r2 (r2 = r1 / 2 when not published); AISC without radii, as the published properties
        /// </summary>
        private static SectionL Angle(CatalogProfile p)
        {
            if (IsAisc(p))
                return new SectionL(p["b"], p["t"], p["h"], p["t"], p.Designation);
            double r2 = p.Has("r2") ? p["r2"] : p["r1"] / 2.0;
            return (SectionL)Rolled(new SectionL(p["b"], p["t"], p["h"], p["t"], p.Designation, p["r1"], r2));
        }

        /// <summary>
        /// Two angles back to back from the single angle of the same catalog: 2L8X6X1X3/8LLBB is two L8X6X1 with the long legs back to back
        /// at 3/8 in
        /// </summary>
        private static SectionBuiltUp DoubleAngle(CatalogProfile p)
        {
            string label = p.Designation.Substring(1);
            if (label.EndsWith("LLBB", StringComparison.Ordinal) || label.EndsWith("SLBB", StringComparison.Ordinal))
                label = label.Substring(0, label.Length - 4);
            string single = string.Join("X", label.Split('X').Take(3));
            CatalogProfile angle = p.Catalog.Find(single) ??
                throw new KeyNotFoundException($"The single angle {single} of {p.Designation} is not in the catalog");
            // the angle of the catalog, without radii as its published properties, with the legs of the configuration
            bool shortBackToBack = p["config"] == 2;
            double vertical = shortBackToBack ? p["short"] : p["long"], horizontal = shortBackToBack ? p["long"] : p["short"];
            var section = new SectionL(horizontal, angle["t"], vertical, angle["t"], single);
            return SectionBuiltUp.DoubleAngle(section, p["s"], p.Designation);
        }

        /// <summary>
        /// The slope of the flanges of a taper flange I section: the one of the catalog (IPN 14%), otherwise 8° (J, BS joists)
        /// </summary>
        private static double TaperSlopeI(CatalogProfile profile)
        {
            double slope = profile["Slope"];
            return double.IsNaN(slope) ? Math.Tan(8.0 * Math.PI / 180.0) : slope;
        }
    }
}
