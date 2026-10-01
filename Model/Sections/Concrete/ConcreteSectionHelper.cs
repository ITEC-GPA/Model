using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Materials;
using GPC.Model.Sections.Rebar;
using GPC.Model.Sections.Steel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Sections.Concrete
{
    /// <summary>
    /// Homogenized properties of the concrete sections with rebars and steel sections (the rebars and the steel sections inside the concrete use n - 1)
    /// </summary>
    internal static class ConcreteSectionHelper
    {
        /// <summary>
        /// The homogenization factor of a rebar: Es / Ec
        /// </summary>
        /// <param name="rebar">The rebar</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <returns>The factor</returns>
        internal static double CalculateN(ReinforcedConcreteRebar rebar, ConcreteMaterial concreteMaterial)
        {
            return rebar.RebarMaterial.ElasticModulusTension / concreteMaterial.ElasticModulusCompression;
        }

        /// <summary>
        /// The homogenization factor of a steel section: Es / Ec
        /// </summary>
        /// <param name="steelSection">The steel section</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <returns>The factor</returns>
        internal static double CalculateN(SteelSectionPosition steelSection, ConcreteMaterial concreteMaterial)
        {
            return steelSection.Section.SteelMaterial.ElasticModulusTension / concreteMaterial.ElasticModulusCompression;
        }

        /// <summary>
        /// The homogenized static moments: the concrete (its area by its centroid) plus (n - 1) A of the rebars and n A of the steel sections minus
        /// the concrete they replace. Before, the static moments of the concrete were integrated on its mesh while the homogenized area used the
        /// area of the shape: for a circle (mesh of 32 sides) the centroid moved towards the origin by 0.6%
        /// </summary>
        /// <param name="concreteCentroid">The centroid of the concrete</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="SxHomog">The homogenized static moment respect to X</param>
        /// <param name="SyHomog">The homogenized static moment respect to Y</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        internal static void CalculateHomogeneizedStaticMoments(Point2d concreteCentroid, double area, ReinforcedConcreteRebar[] rebars,
            ConcreteMaterial concreteMaterial, out double SxHomog, out double SyHomog,
            IList<SteelSectionPosition> steelSections = null)
        {
            SxHomog = area * concreteCentroid.Y;
            SyHomog = area * concreteCentroid.X;
            AddRebarsStaticMoments(rebars, concreteMaterial, ref SxHomog, ref SyHomog);
            AddSteelSectionStaticMoments(steelSections, concreteMaterial, ref SxHomog, ref SyHomog);
        }

        /// <summary>
        /// Adds the static moments of the rebars, (n - 1) A y and (n - 1) A x
        /// </summary>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="SxHomog">The static moment respect to X to increase</param>
        /// <param name="SyHomog">The static moment respect to Y to increase</param>
        /// <param name="nRebarsCommon">A factor for all the rebars (null: the one of each rebar)</param>
        private static void AddRebarsStaticMoments(ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            ref double SxHomog, ref double SyHomog, double? nRebarsCommon = null)
        {
            for (int i = 0; i < rebars.Count(); i++)
            {
                double n = nRebarsCommon ?? CalculateN(rebars[i], concreteMaterial);
                // n-1 Is used to disregard the area of concrete that is replaced by the bar.
                SxHomog += (n - 1) * rebars[i].Area * rebars[i].Position.Y;
                SyHomog += (n - 1) * rebars[i].Area * rebars[i].Position.X;
            }
        }

        /// <summary>
        /// Adds the static moments of the steel sections, n A y and n A x, minus the ones of the concrete they replace (see
        /// <see cref="SteelSectionPosition.ReplacedConcrete"/>: the whole section inside the concrete, n - 1; nothing outside, n; the overlap for
        /// a section partly inside. Before, n - 1 or n for the whole section, from the first thin wall)
        /// </summary>
        /// <param name="steelSections">The steel sections</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="SxHomog">The static moment respect to X to increase</param>
        /// <param name="SyHomog">The static moment respect to Y to increase</param>
        /// <param name="nSteelSectionCommon">A factor for all the steel sections (null: the one of each section)</param>
        private static void AddSteelSectionStaticMoments(IList<SteelSectionPosition> steelSections, ConcreteMaterial concreteMaterial,
            ref double SxHomog, ref double SyHomog, double? nSteelSectionCommon = null)
        {
            if (steelSections != null && steelSections.Count > 0)
            {
                for (int i = 0; i < steelSections.Count(); i++)
                {
                    var steelSection = steelSections[i];
                    double nSteelSection = nSteelSectionCommon ?? CalculateN(steelSection, concreteMaterial);

                    var centroid = steelSection.CalculateCentroid();
                    SxHomog += nSteelSection * steelSection.CalculateArea() * centroid.Y;
                    SyHomog += nSteelSection * steelSection.CalculateArea() * centroid.X;

                    // the concrete replaced by the steel section
                    steelSection.ReplacedConcrete(Point2d.Origin, out _, out double sx, out double sy, out _, out _, out _);
                    SxHomog -= sx;
                    SyHomog -= sy;
                }
            }
        }

        /// <summary>
        /// Adds the moments of inertia of the rebars about the axes through a point, (n - 1) (J + A d²)
        /// </summary>
        /// <param name="rebars">The rebars</param>
        /// <param name="centroid">The origin of the axes</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="JxxHomogenized">The moment of inertia about X to increase</param>
        /// <param name="JyyHomogenized">The moment of inertia about Y to increase</param>
        /// <param name="JxyHomogenized">The product of inertia to increase</param>
        /// <param name="nRebarsCommon">A factor for all the rebars (null: the one of each rebar)</param>
        private static void AddRebarsInertiaMoments(ReinforcedConcreteRebar[] rebars, Point2d centroid, ConcreteMaterial concreteMaterial,
            ref double JxxHomogenized, ref double JyyHomogenized, ref double JxyHomogenized, double? nRebarsCommon = null)
        {
            for (int i = 0; i < rebars.Count(); i++)
            {
                double n = nRebarsCommon ?? CalculateN(rebars[i], concreteMaterial);
                // n-1 Is used to disregard the area of concrete that is replaced by the rebar.
                JxxHomogenized += (n - 1) * (rebars[i].RebarSection.Jxx + rebars[i].Area * Math.Pow(rebars[i].Position.Y - centroid.Y, 2));
                JyyHomogenized += (n - 1) * (rebars[i].RebarSection.Jyy + rebars[i].Area * Math.Pow(rebars[i].Position.X - centroid.X, 2));
                JxyHomogenized += (n - 1) * (rebars[i].RebarSection.Jxy + rebars[i].Area * (rebars[i].Position.X - centroid.X) * (rebars[i].Position.Y - centroid.Y));
            }
        }

        /// <summary>
        /// Adds the moments of inertia of the steel sections about the axes through a point, n J, minus the ones of the concrete they replace
        /// (see <see cref="AddSteelSectionStaticMoments"/>)
        /// </summary>
        /// <param name="steelSections">The steel sections</param>
        /// <param name="centroid">The origin of the axes</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="JxxHomogenized">The moment of inertia about X to increase</param>
        /// <param name="JyyHomogenized">The moment of inertia about Y to increase</param>
        /// <param name="JxyHomogenized">The product of inertia to increase</param>
        /// <param name="nSteelSectionCommon">A factor for all the steel sections (null: the one of each section)</param>
        private static void AddSteelSectionInertiaMoments(IList<SteelSectionPosition> steelSections,
            Point2d centroid, ConcreteMaterial concreteMaterial,
            ref double JxxHomogenized, ref double JyyHomogenized, ref double JxyHomogenized, double? nSteelSectionCommon = null)
        {
            if (steelSections != null && steelSections.Count > 0)
            {
                for (int i = 0; i < steelSections.Count(); i++)
                {
                    var steelSection = steelSections[i];
                    double n = nSteelSectionCommon ?? CalculateN(steelSection, concreteMaterial);

                    JxxHomogenized += n * steelSection.CalculateJxx(centroid);
                    JyyHomogenized += n * steelSection.CalculateJyy(centroid);
                    JxyHomogenized += n * steelSection.CalculateJxy(centroid);

                    // the concrete replaced by the steel section
                    steelSection.ReplacedConcrete(centroid, out _, out _, out _, out double ixx, out double iyy, out double ixy);
                    JxxHomogenized -= ixx;
                    JyyHomogenized -= iyy;
                    JxyHomogenized -= ixy;
                }
            }
        }

        /// <summary>
        /// The homogenized moments of inertia about the axes through the homogenized centroid
        /// </summary>
        /// <param name="rebars">The rebars</param>
        /// <param name="sectionCentroid">The centroid of the concrete</param>
        /// <param name="centroid">The homogenized centroid</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="Jxx">The moment of inertia of the concrete about X (its centroid)</param>
        /// <param name="Jyy">The moment of inertia of the concrete about Y (its centroid)</param>
        /// <param name="Jxy">The product of inertia of the concrete (its centroid)</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="JxxHomogenized">The homogenized moment of inertia about X</param>
        /// <param name="JyyHomogenized">The homogenized moment of inertia about Y</param>
        /// <param name="JxyHomogenized">The homogenized product of inertia</param>
        /// <param name="JpHomogenized">The homogenized polar moment of inertia</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        /// <param name="nRebars">A factor for all the rebars (null: the one of each rebar)</param>
        /// <param name="nSteelSections">A factor for all the steel sections (null: the one of each section)</param>
        internal static void CalculateHomogeneizedInertiaMoments(ReinforcedConcreteRebar[] rebars,
            Point2d sectionCentroid, Point2d centroid, ConcreteMaterial concreteMaterial,
            double Jxx, double Jyy, double Jxy, double area,
            out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized,
            IList<SteelSectionPosition> steelSections = null, double? nRebars = null, double? nSteelSections = null)
        {
            JxxHomogenized = Jxx;
            JyyHomogenized = Jyy;
            JxyHomogenized = Jxy;

            AddRebarsInertiaMoments(rebars, centroid, concreteMaterial, ref JxxHomogenized, ref JyyHomogenized, ref JxyHomogenized, nRebars);
            AddSteelSectionInertiaMoments(steelSections, centroid, concreteMaterial, ref JxxHomogenized, ref JyyHomogenized, ref JxyHomogenized, nSteelSections);

            JxxHomogenized += Math.Pow(sectionCentroid.Y - centroid.Y, 2) * area;
            JyyHomogenized += Math.Pow(sectionCentroid.X - centroid.X, 2) * area;
            JxyHomogenized += (sectionCentroid.X - centroid.X) * (sectionCentroid.Y - centroid.Y) * area;

            JpHomogenized = JxxHomogenized + JyyHomogenized;
        }

        /// <summary>
        /// The homogenized moments of inertia about the axes through the homogenized centroid, with the creep coefficient (mean factor of the
        /// rebars and of the steel sections)
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="rebars">The rebars</param>
        /// <param name="sectionCentroid">The centroid of the concrete</param>
        /// <param name="centroid">The homogenized centroid</param>
        /// <param name="Jxx">The moment of inertia of the concrete about X (its centroid)</param>
        /// <param name="Jyy">The moment of inertia of the concrete about Y (its centroid)</param>
        /// <param name="Jxy">The product of inertia of the concrete (its centroid)</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="JxxHomogenized">The homogenized moment of inertia about X</param>
        /// <param name="JyyHomogenized">The homogenized moment of inertia about Y</param>
        /// <param name="JxyHomogenized">The homogenized product of inertia</param>
        /// <param name="JpHomogenized">The homogenized polar moment of inertia</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        internal static void CalculateHomogeneizedInertiaMoments(double phi, ConcreteMaterial concreteMaterial, ReinforcedConcreteRebar[] rebars,
            Point2d sectionCentroid, Point2d centroid,
            double Jxx, double Jyy, double Jxy, double area,
            out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized,
            IList<SteelSectionPosition> steelSections = null)
        {
            // Rebars
            double? nRebars = null;
            if (rebars != null && rebars.Length > 0)
                nRebars = CalculateHomogenizedFactorN(phi, rebars, concreteMaterial);
            // Steel sections
            double? nSteelSections = null;
            if (steelSections != null && steelSections.Count > 0)
                nSteelSections = CalculateHomogenizedFactorN(phi, steelSections, concreteMaterial);

            CalculateHomogeneizedInertiaMoments(rebars, sectionCentroid, centroid, concreteMaterial, Jxx, Jyy, Jxy, area,
                out JxxHomogenized, out JyyHomogenized, out JxyHomogenized, out JpHomogenized, steelSections, nRebars, nSteelSections);
        }

        /// <summary>
        /// The centroid of the homogenized section with default value of homogenized factor n
        /// </summary>
        /// <param name="concreteCentroid">The centroid of the concrete</param>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        /// <returns>The centroid</returns>
        internal static Point2d GetHomogenizedCentroid(Point2d concreteCentroid, ReinforcedConcreteRebar[] rebars,
            ConcreteMaterial concreteMaterial, double area, out double SxHomog, out double SyHomog,
            IList<SteelSectionPosition> steelSections = null)
        {
            CalculateHomogeneizedStaticMoments(concreteCentroid, area, rebars, concreteMaterial, out SxHomog, out SyHomog, steelSections);

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(rebars, concreteMaterial, area, steelSections));
        }

        /// <summary>
        /// The centroid of the homogenized section with the creep coefficient <paramref name="phi"/>
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <param name="concreteCentroid">The centroid of the concrete</param>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        /// <returns>The centroid</returns>
        internal static Point2d GetHomogenizedCentroid(double phi, Point2d concreteCentroid, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double area, out double SxHomog, out double SyHomog, IList<SteelSectionPosition> steelSections = null)
        {
            // Rebars
            double? nRebars = null;
            if (rebars != null && rebars.Length > 0)
                nRebars = CalculateHomogenizedFactorN(phi, rebars, concreteMaterial);
            // Steel sections
            double? nSteelSections = null;
            if (steelSections != null && steelSections.Count > 0)
                nSteelSections = CalculateHomogenizedFactorN(phi, steelSections, concreteMaterial);

            SxHomog = area * concreteCentroid.Y;
            SyHomog = area * concreteCentroid.X;

            AddRebarsStaticMoments(rebars, concreteMaterial, ref SxHomog, ref SyHomog, nRebars);
            AddSteelSectionStaticMoments(steelSections, concreteMaterial, ref SxHomog, ref SyHomog, nSteelSections);

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(phi, rebars, concreteMaterial, area, steelSections));
        }

        /// <summary>
        /// The centroid of the homogenized section with default value of homogenized factor n, from the static moments of the concrete
        /// </summary>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="Sx">The static moment of the concrete respect to X</param>
        /// <param name="Sy">The static moment of the concrete respect to Y</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        /// <returns>The centroid</returns>
        internal static Point2d GetHomogenizedCentroid(ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double Sx, double Sy, double area, out double SxHomog, out double SyHomog,
            IList<SteelSectionPosition> steelSections = null)
        {
            SxHomog = Sx;
            SyHomog = Sy;

            AddRebarsStaticMoments(rebars, concreteMaterial, ref SxHomog, ref SyHomog);
            AddSteelSectionStaticMoments(steelSections, concreteMaterial, ref SxHomog, ref SyHomog);

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(rebars, concreteMaterial, area, steelSections));
        }

        /// <summary>
        /// The centroid of the homogenized section with the creep coefficient <paramref name="phi"/>, from the static moments of the concrete
        /// (the rebars must not be empty)
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="Sx">The static moment of the concrete respect to X</param>
        /// <param name="Sy">The static moment of the concrete respect to Y</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        /// <returns>The centroid</returns>
        /// <exception cref="InvalidOperationException">If <paramref name="rebars"/> is empty</exception>
        internal static Point2d GetHomogenizedCentroid(double phi, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double Sx, double Sy, double area, out double SxHomog, out double SyHomog,
            IList<SteelSectionPosition> steelSections = null)
        {
            // Rebars
            double nRebars = CalculateHomogenizedFactorN(phi, rebars, concreteMaterial);
            // Steel sections
            double? nSteelSections = null;
            if (steelSections != null && steelSections.Count > 0)
                nSteelSections = CalculateHomogenizedFactorN(phi, steelSections, concreteMaterial);

            SxHomog = Sx;
            SyHomog = Sy;

            AddRebarsStaticMoments(rebars, concreteMaterial, ref SxHomog, ref SyHomog, nRebars);
            AddSteelSectionStaticMoments(steelSections, concreteMaterial, ref SxHomog, ref SyHomog, nSteelSections);

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(phi, rebars, concreteMaterial, area, steelSections));
        }

        /// <summary>
        /// The homogenized area with default value of homogenized factor n
        /// </summary>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        /// <returns>The homogenized area</returns>
        internal static double GetHomogenizedArea(ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, double area,
            IList<SteelSectionPosition> steelSections = null)
        {
            double areaH = 0;
            for (int i = 0; i < rebars.Length; i++)
            {
                areaH += (CalculateN(rebars[i], concreteMaterial) - 1) * rebars[i].Area;
            }
            if (steelSections != null)
            {
                for (int i = 0; i < steelSections.Count(); i++)
                {
                    var steelSection = steelSections[i];
                    double nSteelSection = CalculateN(steelSection, concreteMaterial);
                    // minus the concrete replaced by the steel section
                    steelSection.ReplacedConcrete(Point2d.Origin, out double replaced, out _, out _, out _, out _, out _);
                    areaH += nSteelSection * steelSection.CalculateArea() - replaced;
                }
            }

            return area + areaH;
        }

        /// <summary>
        /// The homogenized area with the creep coefficient <paramref name="phi"/> (the factor of each rebar and steel section)
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        /// <returns>The homogenized area</returns>
        internal static double GetHomogenizedArea(double phi, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, double area,
            IList<SteelSectionPosition> steelSections = null)
        {
            double areaH = 0;
            for (int i = 0; i < rebars.Length; i++)
            {
                areaH += (CalculateHomogenizedFactorN(phi, rebars[i], concreteMaterial) - 1) * rebars[i].Area;
            }
            if (steelSections != null)
            {
                for (int i = 0; i < steelSections.Count(); i++)
                {
                    var steelSection = steelSections[i];
                    double nSteelSection = CalculateHomogenizedFactorN(phi, steelSection, concreteMaterial);
                    // minus the concrete replaced by the steel section
                    steelSection.ReplacedConcrete(Point2d.Origin, out double replaced, out _, out _, out _, out _, out _);
                    areaH += nSteelSection * steelSection.CalculateArea() - replaced;
                }
            }

            return area + areaH;
        }

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 1 with the creep coefficient
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <param name="sectionCentroid">The centroid of the concrete</param>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="Jxx">The moment of inertia of the concrete about X (its centroid)</param>
        /// <param name="Jyy">The moment of inertia of the concrete about Y (its centroid)</param>
        /// <param name="Jxy">The product of inertia of the concrete (its centroid)</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        /// <returns>The moment of inertia</returns>
        internal static double GetHomogeneizedJ11(double phi, Point2d sectionCentroid, ReinforcedConcreteRebar[] rebars,
            ConcreteMaterial concreteMaterial, double area, double Jxx, double Jyy, double Jxy, IList<SteelSectionPosition> steelSections = null)
        {
            Point2d centroidH = GetHomogenizedCentroid(phi, sectionCentroid, rebars, concreteMaterial, area, out double _, out double _, steelSections);
            CalculateHomogeneizedInertiaMoments(phi, concreteMaterial, rebars, sectionCentroid, centroidH, Jxx, Jyy, Jxy, area,
                out double JxxH, out double JyyH, out double JxyH, out double _, steelSections);
            return SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
        }

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 1 (n = Es / Ec)
        /// </summary>
        /// <param name="sectionCentroid">The centroid of the concrete</param>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="Jxx">The moment of inertia of the concrete about X (its centroid)</param>
        /// <param name="Jyy">The moment of inertia of the concrete about Y (its centroid)</param>
        /// <param name="Jxy">The product of inertia of the concrete (its centroid)</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        /// <returns>The moment of inertia</returns>
        internal static double GetHomogeneizedJ11(Point2d sectionCentroid, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double area, double Jxx, double Jyy, double Jxy, IList<SteelSectionPosition> steelSections = null)
        {
            Point2d centroidH = GetHomogenizedCentroid(sectionCentroid, rebars, concreteMaterial, area, out _, out _, steelSections);
            CalculateHomogeneizedInertiaMoments(rebars, sectionCentroid, centroidH, concreteMaterial, Jxx, Jyy, Jxy, area,
                out double JxxH, out double JyyH, out double JxyH, out double _, steelSections);
            return SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
        }

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 2 with the creep coefficient
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <param name="sectionCentroid">The centroid of the concrete</param>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="Jxx">The moment of inertia of the concrete about X (its centroid)</param>
        /// <param name="Jyy">The moment of inertia of the concrete about Y (its centroid)</param>
        /// <param name="Jxy">The product of inertia of the concrete (its centroid)</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        /// <returns>The moment of inertia</returns>
        internal static double GetHomogeneizedJ22(double phi, Point2d sectionCentroid, ReinforcedConcreteRebar[] rebars,
            ConcreteMaterial concreteMaterial, double area, double Jxx, double Jyy, double Jxy, IList<SteelSectionPosition> steelSections = null)
        {
            Point2d centroidH = GetHomogenizedCentroid(phi, sectionCentroid, rebars, concreteMaterial, area, out double _, out double _, steelSections);
            CalculateHomogeneizedInertiaMoments(phi, concreteMaterial, rebars, sectionCentroid, centroidH, Jxx, Jyy, Jxy, area,
                out double JxxH, out double JyyH, out double JxyH, out double _, steelSections);
            return SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
        }

        /// <summary>
        /// The homogenized moment of inertia about the principal axis 2 (n = Es / Ec)
        /// </summary>
        /// <param name="sectionCentroid">The centroid of the concrete</param>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <param name="area">The area of the concrete</param>
        /// <param name="Jxx">The moment of inertia of the concrete about X (its centroid)</param>
        /// <param name="Jyy">The moment of inertia of the concrete about Y (its centroid)</param>
        /// <param name="Jxy">The product of inertia of the concrete (its centroid)</param>
        /// <param name="steelSections">The steel sections (optional)</param>
        /// <returns>The moment of inertia</returns>
        internal static double GetHomogeneizedJ22(Point2d sectionCentroid, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double area, double Jxx, double Jyy, double Jxy, IList<SteelSectionPosition> steelSections = null)
        {
            Point2d centroidH = GetHomogenizedCentroid(sectionCentroid, rebars, concreteMaterial, area, out _, out _, steelSections);
            CalculateHomogeneizedInertiaMoments(rebars, sectionCentroid, centroidH, concreteMaterial, Jxx, Jyy, Jxy, area,
                out double JxxH, out double JyyH, out double JxyH, out double _, steelSections);
            return SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
        }

        /// <summary>
        /// Rebars on a circle: the vertices of a regular polygon of diameter D - 2 c
        /// </summary>
        /// <param name="diameter">The diameter of the section</param>
        /// <param name="concreteCover">The cover (to the center of the bars)</param>
        /// <param name="numberOfRebars">The number of rebars</param>
        /// <param name="rebarSection">The bar section</param>
        /// <param name="centroid">The center</param>
        /// <param name="epsilonP">The prestress STRESS passed to the rebars (the constructor takes a stress, not a strain)</param>
        /// <returns>The rebars</returns>
        internal static ReinforcedConcreteRebar[] SetRadialRebars(double diameter, double concreteCover, int numberOfRebars, IRebarSection rebarSection,
            Point2d centroid = default, double epsilonP = 0.0)
        {
            Polygon2d polygon = new Polygon2d(diameter - concreteCover * 2.0, numberOfRebars, centroid);

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[polygon.Count];

            for (int i = 0; i < polygon.Count; i++)
            {
                rebars[i] = new ReinforcedConcreteRebar(rebarSection, polygon[i], epsilonP);
            }

            return rebars;
        }

        /// <summary>
        /// Get n factor = ES / ( EC / (1 + phi))
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <param name="rebar">The rebar</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <returns>The factor</returns>
        internal static double CalculateHomogenizedFactorN(double phi, ReinforcedConcreteRebar rebar, ConcreteMaterial concreteMaterial)
        {
            return (rebar.RebarMaterial.ElasticModulusTension / (concreteMaterial.ElasticModulusCompression / (1 + phi)));
        }

        /// <summary>
        /// Get n factor = ES / ( EC / (1 + phi)), with the mean ES of the rebars
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <param name="rebars">The rebars</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <returns>The factor</returns>
        internal static double CalculateHomogenizedFactorN(double phi, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial)
        {
            return (rebars.Select(i => i.RebarMaterial.ElasticModulusTension).Average() / (concreteMaterial.ElasticModulusCompression / (1 + phi)));
        }

        /// <summary>
        /// Get n factor = ES / ( EC / (1 + phi))
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <param name="steelSection">The steel section</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <returns>The factor</returns>
        internal static double CalculateHomogenizedFactorN(double phi, SteelSectionPosition steelSection, ConcreteMaterial concreteMaterial)
        {
            return (steelSection.Section.SteelMaterial.ElasticModulusTension / (concreteMaterial.ElasticModulusCompression / (1 + phi)));
        }

        /// <summary>
        /// Get n factor = ES / ( EC / (1 + phi)), with the mean ES of the steel sections
        /// </summary>
        /// <param name="phi">The creep coefficient</param>
        /// <param name="steelSections">The steel sections</param>
        /// <param name="concreteMaterial">The concrete</param>
        /// <returns>The factor</returns>
        internal static double CalculateHomogenizedFactorN(double phi, IList<SteelSectionPosition> steelSections, ConcreteMaterial concreteMaterial)
        {
            return (steelSections.Select(i => i.Section.SteelMaterial.ElasticModulusTension).Average() / (concreteMaterial.ElasticModulusCompression / (1 + phi)));
        }
    }
}
