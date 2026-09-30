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
                "SectionH(h, tw, b, tf, b, tf, r), rolled (root fillets r)",
                "Area, moments of inertia and elastic moduli of the exact geometry; torsion and warping constants of Model",
                p => Rolled(new SectionH(p["h"], p["tw"], p["b"], p["tf"], p["b"], p["tf"], p.Designation, p["r"]))),
            new SectionMapping(SectionFamily.TaperFlangeI, typeof(SectionHTaperFlange), MappingFidelity.Exact,
                "SectionHTaperFlange(h, tw, b, tf, slope, r1, r2, point): IPN slope 14% (declared by the source) with tf at b / 4 from the tip " +
                "of the flange (DIN 1025-1); J slope 8° with tf at the middle of the outstand",
                "IPN: area, moments of inertia and moduli of the exact geometry (taper flanges, root fillets, toe radii). J (BS joists, producer " +
                "range): the published properties are reproduced within 1.3% (area) and 1% (moments of inertia), the geometric convention of the " +
                "source is not documented. Torsion and warping constants of Model",
                p => new SectionHTaperFlange(p["h"], p["tw"], p["b"], p["tf"], TaperSlopeI(p), p["r1"], Or0(p["r2"]),
                    p.Series == "IPN" ? p["b"] / 4.0 : (p["b"] - p["tw"]) / 4.0, p.Designation)),
            new SectionMapping(SectionFamily.ParallelFlangeChannel, typeof(SectionC), MappingFidelity.Exact,
                "SectionC(h, tw, b, tf, b, tf, r1, r2), rolled (root fillets r1, toe radii r2)",
                "Area, moments of inertia and moduli of the exact geometry; torsion and warping constants of Model",
                p => Rolled(new SectionC(p["h"], p["tw"], p["b"], p["tf"], p["b"], p["tf"], p.Designation, p["r1"], Or0(p["r2"])))),
            new SectionMapping(SectionFamily.TaperFlangeChannel, typeof(SectionCTaperFlange), MappingFidelity.Exact,
                "SectionCTaperFlange(h, tw, b, tf, slope, r1, r2, point): UPN h <= 300 slope 8% with tf at b / 2 from the tip of the flange, " +
                "h > 300 slope 5% with tf at the middle of the outstand (DIN 1026-1, checked on the published areas)",
                "Area, moments of inertia and moduli of the exact geometry (taper flanges, root fillets, toe radii); torsion and warping constants " +
                "and shear centre of Model",
                p => new SectionCTaperFlange(p["h"], p["tw"], p["b"], p["tf"], p["h"] <= 300 ? 0.08 : 0.05, p["r1"], Or0(p["r2"]),
                    p["h"] <= 300 ? p["b"] / 2.0 : (p["b"] - p["tw"]) / 2.0, p.Designation)),
            new SectionMapping(SectionFamily.Angle, typeof(SectionL), MappingFidelity.Exact,
                "SectionL(b, t, h, t, r1, r2), rolled: horizontal leg b along y, vertical leg h along z, root fillet r1, toe radii r2 " +
                "(r2 = r1 / 2 of EN 10056-1 when the source does not publish it)",
                "Area, centroid, moments and product of inertia of the exact geometry; torsion and warping constants of Model. The radius r3 " +
                "published for a few sizes is not represented",
                p => Rolled(new SectionL(p["b"], p["t"], p["h"], p["t"], p.Designation, p["r1"], p.Has("r2") ? p["r2"] : p["r1"] / 2.0))),
            new SectionMapping(SectionFamily.Tee, null, MappingFidelity.NotSupported, string.Empty, "No catalog of tees yet", null),
            new SectionMapping(SectionFamily.CircularHollow, typeof(SectionCHS), MappingFidelity.Exact, "SectionCHS(D, t)",
                "Exact properties of the annulus (torsion constant 2 I)",
                p => new SectionCHS(p["D"], p["t"], p.Designation)),
            new SectionMapping(SectionFamily.RectangularHollow, typeof(SectionRHSRoundedCorners), MappingFidelity.Exact,
                "SectionRHSRoundedCorners(h, b, t, ro, ri): corner radii of the calculations of the standard, EN 10210-2 ro = 1.5 t, ri = t",
                "Area, moments of inertia and moduli of the exact geometry with the rounded corners; torsion constant and modulus of EN 10210-2",
                p => new SectionRHSRoundedCorners(p["h"], p["b"], p["t"], 1.5 * p["t"], p["t"], p.Designation)),
            new SectionMapping(SectionFamily.DoubleAngle, null, MappingFidelity.NotSupported, string.Empty, "No catalog yet", null),
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
