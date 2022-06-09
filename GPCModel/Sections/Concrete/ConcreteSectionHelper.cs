using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Materials;
using GPC.Model.Maths.GaussIntegrations;
using GPC.Model.Sections.Rebar;

namespace GPC.Model.Sections.Concrete
{
    internal static class ConcreteSectionHelper
    {

        internal static void CalculateIntegralInertiaMoment(Mesh mesh, MeshFace face, Point2d centroid, out double jxx, out double jyy, out double jxy)
        {
            double area = mesh.GetFaceArea(face);
            Point3d[] points = mesh.GetFacePoints(face);

            if (face.IsTriangle)
            {
                jxx = GaussIntegration.IntegrationTriangularLinearShapeFunction((x, y) => ((y - centroid.Y) * (y - centroid.Y)), points, TriangleGaussPoints.GaussPointNumber.Tri33);
                jyy = GaussIntegration.IntegrationTriangularLinearShapeFunction((x, y) => ((x - centroid.X) * (x - centroid.X)), points, TriangleGaussPoints.GaussPointNumber.Tri33);
                jxy = GaussIntegration.IntegrationTriangularLinearShapeFunction((x, y) => ((x - centroid.X) * (y - centroid.Y)), points, TriangleGaussPoints.GaussPointNumber.Tri33);
            }
            else if (face.IsQuad)
            {
                jxx = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction((x, y) => ((y - centroid.Y) * (y - centroid.Y)), points, QuadrangleGaussPoints.GaussPointNumber.Quad49);
                jyy = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction((x, y) => ((x - centroid.X) * (x - centroid.X)), points, QuadrangleGaussPoints.GaussPointNumber.Quad49);
                jxy = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction((x, y) => ((x - centroid.X) * (y - centroid.Y)), points, QuadrangleGaussPoints.GaussPointNumber.Quad49);
            }
            else
                throw new ArgumentException();
        }

        /// <summary>
        /// Return ES / EC
        /// </summary>
        internal static double CalculateN(ReinforcedConcreteRebar rebar, ConcreteMaterial concreteMaterial)
        {
            return rebar.RebarMaterial.E / concreteMaterial.E;
        }


        internal static void CalculateStaticMoments(Mesh mesh, out double Sx, out double Sy)
        {

            double[] SxArray = new double[mesh.FacesCount];
            double[] SyArray = new double[mesh.FacesCount];


            Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, mesh.FacesCount), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    double area = mesh.GetFaceArea(mesh.Faces[i + 1]);
                    Point2d centroid = mesh.GetFaceCentroid(mesh.Faces[i + 1]);

                    SxArray[i] = area * centroid.Y;
                    SyArray[i] = area * centroid.X;
                }

            });


            Sx = SxArray.Sum();
            Sy = SyArray.Sum();

        }

        internal static void CalculateHomogeneizedStaticMoments(Mesh mesh, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, out double SxHomog, out double SyHomog)
        {

            CalculateStaticMoments(mesh, out double Sx, out double Sy);

            SxHomog = Sx;
            SyHomog = Sy;

            for (int i = 0; i < rebars.Count(); i++)
            {
                double n = CalculateN(rebars[i], concreteMaterial);

                SxHomog += n * rebars[i].Area * rebars[i].Position.Y;
                SyHomog += n * rebars[i].Area * rebars[i].Position.X;
            }

        }

        internal static void CalculateInertiaMoments(Mesh mesh, Point2d centroid, out double Jxx, out double Jyy, out double Jxy, out double Jp)
        {

            double[] JxxArray = new double[mesh.FacesCount];
            double[] JyyArray = new double[mesh.FacesCount];
            double[] JxyArray = new double[mesh.FacesCount];


            Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, mesh.FacesCount), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    CalculateIntegralInertiaMoment(mesh, mesh.Faces[i + 1], centroid, out double jxx, out double jyy, out double jxy);

                    JxxArray[i] = jxx;
                    JyyArray[i] = jyy;
                    JxyArray[i] = jxy;
                }
            });

            Jxx = JxxArray.Sum();
            Jyy = JyyArray.Sum();
            Jxy = JxyArray.Sum();
            Jp = Jxx + Jyy;

            if (Jxy < 0)
                Jxy = 0.0; // non può essere negativo. Se la sezione simmetrica vale zero e può diventare negativo per errore numerico 
        }

        internal static void CalculateHomogeneizedInertiaMoments(ReinforcedConcreteRebar[] rebars, Point2d sectionCentroid, Point2d centroid,
            ConcreteMaterial concreteMaterial, double Jxx, double Jyy, double Jxy, double area,
            out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized)
        {

            JxxHomogenized = Jxx;
            JyyHomogenized = Jyy;
            JxyHomogenized = Jxy;

            for (int i = 0; i < rebars.Count(); i++)
            {
                double n = CalculateN(rebars[i], concreteMaterial);

                JxxHomogenized += n * (rebars[i].RebarSection.Jxx + rebars[i].Area * Math.Pow(rebars[i].Position.Y - centroid.Y, 2));
                JyyHomogenized += n * (rebars[i].RebarSection.Jyy + rebars[i].Area * Math.Pow(rebars[i].Position.X - centroid.X, 2));
                JxyHomogenized += n * (rebars[i].RebarSection.Jxy + rebars[i].Area * (rebars[i].Position.X - centroid.X) * (rebars[i].Position.Y - centroid.Y));
            }

            JxxHomogenized += Math.Pow(sectionCentroid.Y - centroid.Y, 2) * area;
            JyyHomogenized += Math.Pow(sectionCentroid.X - centroid.X, 2) * area;
            JxyHomogenized += (sectionCentroid.X - centroid.X) * (sectionCentroid.Y - centroid.Y) * area;

            JpHomogenized = JxxHomogenized + JyyHomogenized;
        }

        internal static void CalculateHomogeneizedInertiaMoments(double phi, ConcreteMaterial concreteMaterial, ReinforcedConcreteRebar[] rebars,
            Point2d sectionCentroid, Point2d centroid,
            double Jxx, double Jyy, double Jxy, double area,
            out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized)
        {
            double n = CalculateHomogenizedFactorN(phi, rebars, concreteMaterial);

            // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo

            JxxHomogenized = Jxx;
            JyyHomogenized = Jyy;
            JxyHomogenized = Jxy;

            for (int i = 0; i < rebars.Count(); i++)
            {
                JxxHomogenized += n * (rebars[i].RebarSection.Jxx + rebars[i].Area * (Math.Pow((rebars[i].Position.Y - centroid.Y), 2)));
                JyyHomogenized += n * (rebars[i].RebarSection.Jyy + rebars[i].Area * (Math.Pow((rebars[i].Position.X - centroid.X), 2)));
                JxyHomogenized += n * (rebars[i].RebarSection.Jxy + rebars[i].Area * (rebars[i].Position.X - centroid.X) * (rebars[i].Position.Y - centroid.Y));
            }

            JpHomogenized = JxxHomogenized + JyyHomogenized;

            JxxHomogenized += Math.Pow(sectionCentroid.Y - centroid.Y, 2) * area;
            JyyHomogenized += Math.Pow(sectionCentroid.X - centroid.X, 2) * area;
            JxyHomogenized += (sectionCentroid.X - centroid.X) * (sectionCentroid.Y - centroid.Y) * area;
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
        internal static Point2d GetHomogenizedCentroid(Mesh mesh, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, double area, out double SxHomog, out double SyHomog)
        {
            CalculateHomogeneizedStaticMoments(mesh, rebars, concreteMaterial, out SxHomog, out SyHomog);

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(rebars, concreteMaterial, area));
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
        internal static Point2d GetHomogenizedCentroid(double phi, Mesh mesh, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, double area, out double SxHomog, out double SyHomog)
        {
            double n = CalculateHomogenizedFactorN(phi, rebars, concreteMaterial);

            CalculateStaticMoments(mesh, out double Sx, out double Sy);

            SxHomog = Sx;
            SyHomog = Sy;

            for (int i = 0; i < rebars.Count(); i++)
            {
                SxHomog += n * rebars[i].Area * rebars[i].Position.Y;
                SyHomog += n * rebars[i].Area * rebars[i].Position.X;
            }

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(phi, rebars, concreteMaterial, area));
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
        internal static Point2d GetHomogenizedCentroid(ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, double Sx, double Sy, double area, out double SxHomog, out double SyHomog)
        {

            SxHomog = Sx;
            SyHomog = Sy;

            for (int i = 0; i < rebars.Count(); i++)
            {
                double n = CalculateN(rebars[i], concreteMaterial);
                SxHomog += n * rebars[i].Area * rebars[i].Position.Y;
                SyHomog += n * rebars[i].Area * rebars[i].Position.X;
            }

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(rebars, concreteMaterial, area));
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
        internal static Point2d GetHomogenizedCentroid(double phi, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, double Sx, double Sy, double area, out double SxHomog, out double SyHomog)
        {
            double n = CalculateHomogenizedFactorN(phi, rebars, concreteMaterial);

            SxHomog = Sx;
            SyHomog = Sy;

            for (int i = 0; i < rebars.Count(); i++)
            {
                SxHomog += n * rebars[i].Area * rebars[i].Position.Y;
                SyHomog += n * rebars[i].Area * rebars[i].Position.X;
            }

            return SectionHelper.CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(phi, rebars, concreteMaterial, area));
        }

        /// <summary>
        /// The homogenized area with default value of homogenized factor n
        /// </summary>
        /// <returns>The homogenized area</returns>
        internal static double GetHomogenizedArea(ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, double area)
        {

            double areaH = 0;
            for (int i = 0; i < rebars.Length; i++)
            {
                areaH += CalculateN(rebars[i], concreteMaterial) * rebars[i].Area;
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
        internal static double GetHomogenizedArea(double phi, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, double area)
        {
            double[] AreaHomogArray = new double[rebars.Count()];

            Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, rebars.Count()), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    AreaHomogArray[i] = (CalculateHomogenizedFactorN(phi, rebars[i], concreteMaterial)) * rebars[i].Area;
                }
            });


            return area + AreaHomogArray.Sum();
        }

        internal static double GetHomogeneizedJ11(double phi, Point2d sectionCentroid, Mesh mesh, ReinforcedConcreteRebar[] rebars,
            ConcreteMaterial concreteMaterial, double area, double Jxx, double Jyy, double Jxy)
        {
            Point2d centroidH = GetHomogenizedCentroid(phi, mesh, rebars, concreteMaterial, area, out double _, out double _);
            CalculateHomogeneizedInertiaMoments(phi, concreteMaterial, rebars, sectionCentroid, centroidH, Jxx, Jyy, Jxy, area, out double JxxH, out double JyyH, out double JxyH, out double _);
            return SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
        }

        internal static double GetHomogeneizedJ11(Mesh mesh, Point2d sectionCentroid, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double area, double Jxx, double Jyy, double Jxy)
        {
            Point2d centroidH = GetHomogenizedCentroid(mesh, rebars, concreteMaterial, area, out _, out _);
            CalculateHomogeneizedInertiaMoments(rebars, sectionCentroid, centroidH, concreteMaterial, Jxx, Jyy, Jxy, area,
            out double JxxH, out double JyyH, out double JxyH, out double _);
            return SectionHelper.CalculateJ11(JxxH, JyyH, JxyH);
        }

        internal static double GetHomogeneizedJ22(double phi, Point2d sectionCentroid, Mesh mesh, ReinforcedConcreteRebar[] rebars,
            ConcreteMaterial concreteMaterial, double area, double Jxx, double Jyy, double Jxy)
        {
            Point2d centroidH = GetHomogenizedCentroid(phi, mesh, rebars, concreteMaterial, area, out double _, out double _);
            CalculateHomogeneizedInertiaMoments(phi, concreteMaterial, rebars, sectionCentroid, centroidH, Jxx, Jyy, Jxy, area,
                out double JxxH, out double JyyH, out double JxyH, out double _);
            return SectionHelper.CalculateJ22(JxxH, JyyH, JxyH);
        }

        internal static double GetHomogeneizedJ22(Mesh mesh, Point2d sectionCentroid, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double area, double Jxx, double Jyy, double Jxy)
        {
            Point2d centroidH = GetHomogenizedCentroid(mesh, rebars, concreteMaterial, area, out _, out _);
            CalculateHomogeneizedInertiaMoments(rebars, sectionCentroid, centroidH, concreteMaterial, Jxx, Jyy, Jxy, area,
            out double JxxH, out double JyyH, out double JxyH, out double _);
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
            return (rebar.RebarMaterial.E / (concreteMaterial.E / (1 + phi)));
        }

        /// <summary>
        /// Get n factor = ES / ( EC (1 + phi)) 
        /// </summary>
        internal static double CalculateHomogenizedFactorN(double phi, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial)
        {
            return (rebars.Select(i => i.RebarMaterial.E).Average() / (concreteMaterial.E / (1 + phi)));
        }
    }
}
