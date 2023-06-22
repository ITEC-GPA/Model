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
    internal static class ConcreteSectionHelper
    {
        /// <summary>
        /// Return ES / EC
        /// </summary>
        internal static double CalculateN(ReinforcedConcreteRebar rebar, ConcreteMaterial concreteMaterial)
        {
            return rebar.RebarMaterial.ElasticModulusTension / concreteMaterial.ElasticModulusCompression;
        }

        /// <summary>
        /// Return ES / EC
        /// </summary>
        internal static double CalculateN(SteelSectionPosition steelSection, ConcreteMaterial concreteMaterial)
        {
            return steelSection.Section.Material.ElasticModulusTension / concreteMaterial.ElasticModulusCompression;
        }

        internal static void CalculateHomogeneizedStaticMoments(Mesh mesh, ReinforcedConcreteRebar[] rebars,
            ConcreteMaterial concreteMaterial, out double SxHomog, out double SyHomog,
            IList<SteelSectionPosition> steelSections = null)
        {
            SectionHelper.CalculateStaticMoments(mesh, out double Sx, out double Sy);

            SxHomog = Sx;
            SyHomog = Sy;

            AddRebarAndSteelHomogeneizedStaticMoments(rebars, concreteMaterial, ref SxHomog, ref SyHomog, steelSections);
        }

        private static void AddRebarsStaticMoments(ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            ref double SxHomog, ref double SyHomog, double? nCommon = null)
        {
            for (int i = 0; i < rebars.Count(); i++)
            {
                double n = nCommon ?? CalculateN(rebars[i], concreteMaterial);
                // n-1 Is used to disregard the area of concrete that is replaced by the bar.
                SxHomog += (n - 1) * rebars[i].Area * rebars[i].Position.Y;
                SyHomog += (n - 1) * rebars[i].Area * rebars[i].Position.X;
            }
        }

        private static void AddRebarAndSteelHomogeneizedStaticMoments(ReinforcedConcreteRebar[] rebars,
            ConcreteMaterial concreteMaterial, ref double SxHomog, ref double SyHomog,
            IList<SteelSectionPosition> steelSections)
        {
            AddRebarsStaticMoments(rebars, concreteMaterial, ref SxHomog, ref SyHomog);
            if (steelSections != null)
            {
                for (int i = 0; i < steelSections.Count(); i++)
                {
                    var steelSection = steelSections[i];
                    double nSteelSection = CalculateN(steelSection, concreteMaterial);
                    // n-1 Is used to disregard the area of concrete that is replaced by the steel section.
                    if (steelSection.IsInsideConcrete)
                        nSteelSection -= 1.0;

                    var centroid = steelSection.CalculateCentroid();
                    SxHomog += nSteelSection * steelSection.CalculateArea() * centroid.Y;
                    SyHomog += nSteelSection * steelSection.CalculateArea() * centroid.X;
                }
            }
        }

        private static void AddRebarsInertiaMoments(ReinforcedConcreteRebar[] rebars, Point2d centroid, ConcreteMaterial concreteMaterial,
            ref double JxxHomogenized, ref double JyyHomogenized, ref double JxyHomogenized, double? nCommon = null)
        {
            for (int i = 0; i < rebars.Count(); i++)
            {
                double n = nCommon ?? CalculateN(rebars[i], concreteMaterial);
                // n-1 Is used to disregard the area of concrete that is replaced by the rebar.
                JxxHomogenized += (n - 1) * (rebars[i].RebarSection.Jxx + rebars[i].Area * Math.Pow(rebars[i].Position.Y - centroid.Y, 2));
                JyyHomogenized += (n - 1) * (rebars[i].RebarSection.Jyy + rebars[i].Area * Math.Pow(rebars[i].Position.X - centroid.X, 2));
                JxyHomogenized += (n - 1) * (rebars[i].RebarSection.Jxy + rebars[i].Area * (rebars[i].Position.X - centroid.X) * (rebars[i].Position.Y - centroid.Y));
            }
        }

        private static void AddSteelSectionInertiaMoments(IList<SteelSectionPosition> steelSections,
            Point2d centroid, ConcreteMaterial concreteMaterial,
            ref double JxxHomogenized, ref double JyyHomogenized, ref double JxyHomogenized, double? nCommon = null)
        {
            if (steelSections != null && steelSections.Count > 0)
            {
                for (int i = 0; i < steelSections.Count(); i++)
                {
                    var steelSection = steelSections[i];
                    double n = nCommon ?? CalculateN(steelSection, concreteMaterial);
                    // n-1 Is used to disregard the area of concrete that is replaced by the steel section.
                    if (steelSection.IsInsideConcrete)
                        n -= 1.0;

                    JxxHomogenized += n * steelSection.CalculateJxx(centroid);
                    JyyHomogenized += n * steelSection.CalculateJyy(centroid);
                    JxyHomogenized += n * steelSection.CalculateJxy(centroid);
                }
            }
        }

        internal static void CalculateHomogeneizedInertiaMoments(ReinforcedConcreteRebar[] rebars,
            Point2d sectionCentroid, Point2d centroid, ConcreteMaterial concreteMaterial,
            double Jxx, double Jyy, double Jxy, double area,
            out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized,
            IList<SteelSectionPosition> steelSections = null)
        {
            JxxHomogenized = Jxx;
            JyyHomogenized = Jyy;
            JxyHomogenized = Jxy;

            AddRebarsInertiaMoments(rebars, centroid, concreteMaterial, ref JxxHomogenized, ref JyyHomogenized, ref JxyHomogenized);
            AddSteelSectionInertiaMoments(steelSections, centroid, concreteMaterial, ref JxxHomogenized, ref JyyHomogenized, ref JxyHomogenized);

            JxxHomogenized += Math.Pow(sectionCentroid.Y - centroid.Y, 2) * area;
            JyyHomogenized += Math.Pow(sectionCentroid.X - centroid.X, 2) * area;
            JxyHomogenized += (sectionCentroid.X - centroid.X) * (sectionCentroid.Y - centroid.Y) * area;

            JpHomogenized = JxxHomogenized + JyyHomogenized;
        }

        internal static void CalculateHomogeneizedInertiaMoments(double phi, ConcreteMaterial concreteMaterial, ReinforcedConcreteRebar[] rebars,
            Point2d sectionCentroid, Point2d centroid,
            double Jxx, double Jyy, double Jxy, double area,
            out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized,
            IList<SteelSectionPosition> steelSections = null)
        {
            // Rebars
            double nRebars = CalculateHomogenizedFactorN(phi, rebars, concreteMaterial);
            // Steel sections
            double? nSteelSections = null;
            if (steelSections != null && steelSections.Count > 0)
                nSteelSections = CalculateHomogenizedFactorN(phi, steelSections, concreteMaterial);

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
        /// The centroid of the homogenized section with default value of homogenized factor n
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="rebars"></param>
        /// <param name="concreteMaterial"></param>
        /// <param name="area"></param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns>The centroid</returns>
        internal static Point2d GetHomogenizedCentroid(Mesh mesh, ReinforcedConcreteRebar[] rebars,
            ConcreteMaterial concreteMaterial, double area, out double SxHomog, out double SyHomog,
            IList<SteelSectionPosition> steelSections = null)
        {
            CalculateHomogeneizedStaticMoments(mesh, rebars, concreteMaterial, out SxHomog, out SyHomog, steelSections);

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(rebars, concreteMaterial, area, steelSections));
        }

        /// <summary>
        /// The centroid of the homogenized section with homogenized factor <paramref name="phi"/>
        /// </summary>
        /// <param name="phi">The homogenized factor</param>
        /// <param name="mesh"></param>
        /// <param name="rebars"></param>
        /// <param name="concreteMaterial"></param>
        /// <param name="area"></param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns></returns>
        internal static Point2d GetHomogenizedCentroid(double phi, Mesh mesh, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double area, out double SxHomog, out double SyHomog, IList<SteelSectionPosition> steelSections = null)
        {
            double nRebars = CalculateHomogenizedFactorN(phi, rebars, concreteMaterial);

            SectionHelper.CalculateStaticMoments(mesh, out double Sx, out double Sy);

            SxHomog = Sx;
            SyHomog = Sy;

            AddRebarsStaticMoments(rebars, concreteMaterial, ref SxHomog, ref SyHomog, nRebars);

            if (steelSections != null && steelSections.Count > 0)
            {
                double nSteelSections = CalculateHomogenizedFactorN(phi, steelSections, concreteMaterial);

                for (int i = 0; i < steelSections.Count(); i++)
                {
                    var steelSection = steelSections[i];
                    var centroid = steelSection.CalculateCentroid();
                    SxHomog += (nSteelSections - 1.0) * steelSection.CalculateArea() * centroid.Y;
                    SyHomog += (nSteelSections - 1.0) * steelSection.CalculateArea() * centroid.X;
                }
            }

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(phi, rebars, concreteMaterial, area, steelSections));
        }

        /// <summary>
        /// The centroid of the homogenized section with default value of homogenized factor n
        /// </summary>
        /// <param name="Sx"></param>
        /// <param name="Sy"></param>
        /// <param name="rebars"></param>
        /// <param name="concreteMaterial"></param>
        /// <param name="area"></param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns>The centroid</returns>
        internal static Point2d GetHomogenizedCentroid(ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double Sx, double Sy, double area, out double SxHomog, out double SyHomog,
            IList<SteelSectionPosition> steelSections = null)
        {
            SxHomog = Sx;
            SyHomog = Sy;

            AddRebarAndSteelHomogeneizedStaticMoments(rebars, concreteMaterial, ref SxHomog, ref SyHomog, steelSections);

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(rebars, concreteMaterial, area, steelSections));
        }

        /// <summary>
        /// The centroid of the homogenized section with homogenized factor <paramref name="phi"/>
        /// </summary>
        /// <param name="phi">The homogenized factor</param>
        /// <param name="rebars"></param>
        /// <param name="concreteMaterial"></param>
        /// <param name="Sx"></param>
        /// <param name="Sy"></param>
        /// <param name="area"></param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns></returns>
        internal static Point2d GetHomogenizedCentroid(double phi, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double Sx, double Sy, double area, out double SxHomog, out double SyHomog,
            IList<SteelSectionPosition> steelSections = null)
        {
            double nRebars = CalculateHomogenizedFactorN(phi, rebars, concreteMaterial);

            SxHomog = Sx;
            SyHomog = Sy;

            AddRebarsStaticMoments(rebars, concreteMaterial, ref SxHomog, ref SyHomog, nRebars);

            if (steelSections != null && steelSections.Count > 0)
            {
                double nSteelSections = CalculateHomogenizedFactorN(phi, steelSections, concreteMaterial);

                for (int i = 0; i < steelSections.Count(); i++)
                {
                    var steelSection = steelSections[i];
                    var centroid = steelSection.CalculateCentroid();
                    SxHomog += (nSteelSections - 1.0) * steelSection.CalculateArea() * centroid.Y;
                    SyHomog += (nSteelSections - 1.0) * steelSection.CalculateArea() * centroid.X;
                }
            }

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(phi, rebars, concreteMaterial, area, steelSections));
        }

        /// <summary>
        /// The homogenized area with default value of homogenized factor n
        /// </summary>
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
                    // n-1 Is used to disregard the area of concrete that is replaced by the steel section.
                    if (steelSection.IsInsideConcrete)
                        nSteelSection -= 1.0;

                    areaH += nSteelSection * steelSection.CalculateArea();
                }
            }

            return area + areaH;
        }

        /// <summary>
        /// The homogenized area with homogenized factor <paramref name="phi"/>
        /// </summary>
        /// <param name="phi"></param>
        /// <param name="rebars"></param>
        /// <param name="concreteMaterial"></param>
        /// <param name="area"></param>
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
                    // n-1 Is used to disregard the area of concrete that is replaced by the steel section.
                    if (steelSection.IsInsideConcrete)
                        nSteelSection -= 1.0;

                    areaH += nSteelSection * steelSection.CalculateArea();
                }
            }

            return area + areaH;
        }

        internal static double GetHomogeneizedJ11(double phi, Point2d sectionCentroid, Mesh mesh, ReinforcedConcreteRebar[] rebars,
            ConcreteMaterial concreteMaterial, double area, double Jxx, double Jyy, double Jxy, IList<SteelSectionPosition> steelSections = null)
        {
            Point2d centroidH = GetHomogenizedCentroid(phi, mesh, rebars, concreteMaterial, area, out double _, out double _, steelSections);
            CalculateHomogeneizedInertiaMoments(phi, concreteMaterial, rebars, sectionCentroid, centroidH, Jxx, Jyy, Jxy, area,
                out double JxxH, out double JyyH, out double JxyH, out double _, steelSections);
            return SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
        }

        internal static double GetHomogeneizedJ11(Mesh mesh, Point2d sectionCentroid, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double area, double Jxx, double Jyy, double Jxy, IList<SteelSectionPosition> steelSections = null)
        {
            Point2d centroidH = GetHomogenizedCentroid(mesh, rebars, concreteMaterial, area, out _, out _, steelSections);
            CalculateHomogeneizedInertiaMoments(rebars, sectionCentroid, centroidH, concreteMaterial, Jxx, Jyy, Jxy, area,
                out double JxxH, out double JyyH, out double JxyH, out double _, steelSections);
            return SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
        }

        internal static double GetHomogeneizedJ22(double phi, Point2d sectionCentroid, Mesh mesh, ReinforcedConcreteRebar[] rebars,
            ConcreteMaterial concreteMaterial, double area, double Jxx, double Jyy, double Jxy, IList<SteelSectionPosition> steelSections = null)
        {
            Point2d centroidH = GetHomogenizedCentroid(phi, mesh, rebars, concreteMaterial, area, out double _, out double _, steelSections);
            CalculateHomogeneizedInertiaMoments(phi, concreteMaterial, rebars, sectionCentroid, centroidH, Jxx, Jyy, Jxy, area,
                out double JxxH, out double JyyH, out double JxyH, out double _, steelSections);
            return SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
        }

        internal static double GetHomogeneizedJ22(Mesh mesh, Point2d sectionCentroid, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double area, double Jxx, double Jyy, double Jxy, IList<SteelSectionPosition> steelSections = null)
        {
            Point2d centroidH = GetHomogenizedCentroid(mesh, rebars, concreteMaterial, area, out _, out _, steelSections);
            CalculateHomogeneizedInertiaMoments(rebars, sectionCentroid, centroidH, concreteMaterial, Jxx, Jyy, Jxy, area,
                out double JxxH, out double JyyH, out double JxyH, out double _, steelSections);
            return SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
        }

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
        /// Get n factor = ES / ( EC (1 + phi)) 
        /// </summary>
        internal static double CalculateHomogenizedFactorN(double phi, ReinforcedConcreteRebar rebar, ConcreteMaterial concreteMaterial)
        {
            return (rebar.RebarMaterial.ElasticModulusTension / (concreteMaterial.ElasticModulusCompression / (1 + phi)));
        }

        /// <summary>
        /// Get n factor = ES / ( EC (1 + phi)) 
        /// </summary>
        internal static double CalculateHomogenizedFactorN(double phi, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial)
        {
            return (rebars.Select(i => i.RebarMaterial.ElasticModulusTension).Average() / (concreteMaterial.ElasticModulusCompression / (1 + phi)));
        }

        /// <summary>
        /// Get n factor = ES / ( EC (1 + phi)) 
        /// </summary>
        internal static double CalculateHomogenizedFactorN(double phi, SteelSectionPosition steelSection, ConcreteMaterial concreteMaterial)
        {
            return (steelSection.Section.Material.ElasticModulusTension / (concreteMaterial.ElasticModulusCompression / (1 + phi)));
        }

        /// <summary>
        /// Get n factor = ES / ( EC (1 + phi)) 
        /// </summary>
        internal static double CalculateHomogenizedFactorN(double phi, IList<SteelSectionPosition> steelSections, ConcreteMaterial concreteMaterial)
        {
            return (steelSections.Select(i => i.Section.Material.ElasticModulusTension).Average() / (concreteMaterial.ElasticModulusCompression / (1 + phi)));
        }
    }
}
