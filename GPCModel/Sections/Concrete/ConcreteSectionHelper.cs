using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                jxx = GaussIntegration.IntegrationTriangularLinearShapeFunction((x, y) => ((y - centroid.Y) * (y - centroid.Y)), points, 4);
                jyy = GaussIntegration.IntegrationTriangularLinearShapeFunction((x, y) => ((x - centroid.X) * (x - centroid.X)), points, 4);
                jxy = GaussIntegration.IntegrationTriangularLinearShapeFunction((x, y) => ((x - centroid.X) * (y - centroid.Y)), points, 4);
            }
            else if (face.IsQuad)
            {
                jxx = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction((x, y) => ((y - centroid.Y) * (y - centroid.Y)), points, 8);
                jyy = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction((x, y) => ((x - centroid.X) * (x - centroid.X)), points, 8);
                jxy = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction((x, y) => ((x - centroid.X) * (y - centroid.Y)), points, 8);
            }
            else
                throw new ArgumentException();
        }

        internal static double CalculateN(ReinforcedConcreteRebar rebar, ConcreteMaterial concreteMaterial)
        {
            return rebar.RebarMaterial.E / concreteMaterial.E;
        }

        internal static double CalculateN(int rebar, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial)
        {
            return rebars[rebar].RebarMaterial.E / concreteMaterial.E;
        }

        internal static void CalculateStaticMoments(Mesh mesh, out double Sx, out double Sy)
        {
            double[] SxArray = new double[mesh.FacesCount];
            double[] SyArray = new double[mesh.FacesCount];

            Parallel.For(0, mesh.FacesCount, (i) =>
            {
                double area = mesh.GetFaceArea(mesh.Faces[i + 1]);
                Point2d centroid = mesh.GetFaceCentroid(mesh.Faces[i + 1]);

                SxArray[i] = area * centroid.Y;
                SyArray[i] += area * centroid.X;
            });

            Sx = SxArray.Sum();
            Sy = SyArray.Sum();
        }

        internal static void CalculateInertiaMoments(Mesh mesh, Point2d centroid, out double Jxx, out double Jyy, out double Jxy, out double Jp)
        {
            double[] JxxArray = new double[mesh.FacesCount];
            double[] JyyArray = new double[mesh.FacesCount];
            double[] JxyArray = new double[mesh.FacesCount];

            Parallel.For(0, mesh.FacesCount, (i) =>
            {
                CalculateIntegralInertiaMoment(mesh, mesh.Faces[i + 1], centroid, out double jxx, out double jyy, out double jxy);

                JxxArray[i] = jxx;
                JyyArray[i] = jyy;
                JxyArray[i] = jxy;
            });

            Jxx = JxxArray.Sum();
            Jyy = JyyArray.Sum();
            Jxy = JxyArray.Sum();
            Jp = Jxx + Jyy;
        }




        internal static void CalculateHomogeneizedInertiaMoments(ReinforcedConcreteRebar[] rebars, Point2d sectionCentroid, Point2d centroid,
            ConcreteMaterial concreteMaterial, double Jxx, double Jyy, double Jxy, double area,
            out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized)
        {
            double[] JxxRebarArray = new double[rebars.Count()];
            double[] JyyRebarArray = new double[rebars.Count()];
            double[] JxyRebarArray = new double[rebars.Count()];

            Parallel.For(0, rebars.Count(), (i) =>
            {
                JxxRebarArray[i] = (CalculateN(rebars[i], concreteMaterial) - 1) * (rebars[i].RebarSection.Jxx + rebars[i].Area *
                    (Math.Pow((rebars[i].Position.Y - centroid.Y), 2)));
                JyyRebarArray[i] = (CalculateN(rebars[i], concreteMaterial) - 1) * (rebars[i].RebarSection.Jyy + rebars[i].Area *
                    (Math.Pow((rebars[i].Position.X - centroid.X), 2)));
                JxyRebarArray[i] = (CalculateN(rebars[i], concreteMaterial) - 1) * (rebars[i].RebarSection.Jxy + rebars[i].Area *
                    (rebars[i].Position.X - centroid.X) * (rebars[i].Position.Y - centroid.Y));
            });

            JxxHomogenized = Jxx + JxxRebarArray.Sum();
            JyyHomogenized = Jyy + JyyRebarArray.Sum();
            JxyHomogenized = Jxy + JxyRebarArray.Sum();
            JpHomogenized = JxxHomogenized + JyyHomogenized;

            JxxHomogenized += Math.Pow(sectionCentroid.Y - centroid.Y, 2) * area;
            JyyHomogenized += Math.Pow(sectionCentroid.X - centroid.X, 2) * area;
            JxyHomogenized += (sectionCentroid.X - centroid.X) * (sectionCentroid.Y - centroid.Y) * area;
        }

        internal static void CalculateHomogeneizedInertiaMoments(double n, ReinforcedConcreteRebar[] rebars, Point2d sectionCentroid, Point2d centroid,
            double Jxx, double Jyy, double Jxy, double area,
            out double JxxHomogenized, out double JyyHomogenized, out double JxyHomogenized, out double JpHomogenized)
        {
            double[] JxxRebarArray = new double[rebars.Count()];
            double[] JyyRebarArray = new double[rebars.Count()];
            double[] JxyRebarArray = new double[rebars.Count()];

            Parallel.For(0, rebars.Count(), (i) =>
            {
                JxxRebarArray[i] = (n - 1) * (rebars[i].RebarSection.Jxx + rebars[i].Area * (Math.Pow((rebars[i].Position.Y - centroid.Y), 2)));
                JyyRebarArray[i] = (n - 1) * (rebars[i].RebarSection.Jyy + rebars[i].Area * (Math.Pow((rebars[i].Position.X - centroid.X), 2)));
                JxyRebarArray[i] = (n - 1) * (rebars[i].RebarSection.Jxy + rebars[i].Area * (rebars[i].Position.X - centroid.X) * (rebars[i].Position.Y - centroid.Y));
            });

            JxxHomogenized = Jxx + JxxRebarArray.Sum();
            JyyHomogenized = Jyy + JyyRebarArray.Sum();
            JxyHomogenized = Jxy + JxyRebarArray.Sum();
            JpHomogenized = JxxHomogenized + JyyHomogenized;

            JxxHomogenized += Math.Pow(sectionCentroid.Y - centroid.Y, 2) * area;
            JyyHomogenized += Math.Pow(sectionCentroid.X - centroid.X, 2) * area;
            JxyHomogenized += (sectionCentroid.X - centroid.X) * (sectionCentroid.Y - centroid.Y) * area;

            // NOTA: ci siamo ricondotti a momenti d'inerzia rispetto al baricentro della sezione di solo calcestruzzo
        }

        /// <summary>
        /// Return all homogenized mechanical properties with default value of homogenized factor n
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="centroid"></param>
        /// <param name="rebars"></param>
        /// <param name="concreteMaterial"></param>
        /// <param name="area"></param>
        /// <param name="areaH">The homogeneized area</param>
        /// <param name="SxH">The first moment of area calculated respect input X-axis of the homogeneized section</param>
        /// <param name="SyH">The first moment of area calculated respect input Y-axis of the homogeneized section</param>
        /// <param name="Jxx"></param>
        /// <param name="Jyy"></param>
        /// <param name="Jxy"></param>
        /// <param name="centroidH">The centroid of homogeneized section</param>
        /// <param name="JxxH">The first moment of area calculated respect X-axis passing throw the centroid of the homogeneized section</param>
        /// <param name="JyyH">The first moment of area calculated respect Y-axis passing throw the centroid of the homogeneized section</param>
        /// <param name="JxyH"></param>
        /// <param name="JpH"></param>
        /// <param name="J11H">The first moment of area calculated respect the first principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</param>
        /// <param name="J22H">The first moment of area calculated respect the second principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</param>
        /// <param name="angleX">The angle of rotation of the principal axis respect the X-Axis</param>
        internal static void GetHomogeneizedMechanicalProperties(Mesh mesh, Point2d centroid, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, double area,
            double Jxx, double Jyy, double Jxy, out double areaH, out double SxH, out double SyH,
            out Point2d centroidH, out double JxxH, out double JyyH, out double JxyH, out double JpH, out double J11H, out double J22H, out double angleX)
        {
            areaH = GetHomogenizedArea(rebars, concreteMaterial, area);
            centroidH = GetHomogenizedCentroid(mesh, rebars, concreteMaterial, area, out SxH, out SyH);
            CalculateHomogeneizedInertiaMoments(rebars, centroidH, centroid, concreteMaterial, Jxx, Jyy, Jxy, area,
            out JxxH, out JyyH, out JxyH, out JpH);
            J11H = CalculateJ11(JxxH, JyyH, JxyH);
            J22H = CalculateJ22(JxxH, JyyH, JxyH);
            angleX = CalculateAngle(JxxH, JyyH, JxyH);
        }

        /// <summary>
        /// Return all homogenized mechanical properties with homogeneized factor <paramref name="n"/>
        /// </summary>
        /// <param name="n">The homogeneized factor</param>
        /// <param name="mesh"></param>
        /// <param name="rebars"></param>
        /// <param name="sectionCentroid"></param>
        /// <param name="area"></param>
        /// <param name="Jxx"></param>
        /// <param name="Jyy"></param>
        /// <param name="Jxy"></param>
        /// <param name="areaH">The homogeneized area</param>
        /// <param name="SxH">The first moment of area calculated respect input X-axis of the homogeneized section</param>
        /// <param name="SyH">The first moment of area calculated respect input Y-axis of the homogeneized section</param>
        /// <param name="centroidH">The centroid of homogeneized section</param>
        /// <param name="JxxH">The first moment of area calculated respect X-axis passing throw the centroid of the homogeneized section</param>
        /// <param name="JyyH">The first moment of area calculated respect Y-axis passing throw the centroid of the homogeneized section</param>
        /// <param name="JxyH"></param>
        /// <param name="JpH"></param>
        /// <param name="J11H">The first moment of area calculated respect the first principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</param>
        /// <param name="J22H">The first moment of area calculated respect the second principal axis 
        /// passing throw the centroid of only concrete section of the homogeneized section</param>
        /// <param name="angleX">The angle of rotation of the principal axis respect the X-Axis</param>
        internal static void GetHomogeneizedMechanicalProperties(double n, Mesh mesh, ReinforcedConcreteRebar[] rebars, Point2d sectionCentroid,
            double area, double Jxx, double Jyy, double Jxy, out double areaH, out double SxH, out double SyH, out Point2d centroidH,
            out double JxxH, out double JyyH, out double JxyH, out double JpH, out double J11H, out double J22H, out double angleX)
        {
            areaH = GetHomogenizedArea(n, rebars, area);
            centroidH = GetHomogenizedCentroid(n, mesh, rebars, area, out SxH, out SyH);
            CalculateHomogeneizedInertiaMoments(n, rebars, sectionCentroid, centroidH,
            Jxx, Jyy, Jxy, area,
            out JxxH, out JyyH, out JxyH, out JpH);
            J11H = CalculateJ11(JxxH, JyyH, JxyH);
            J22H = CalculateJ22(JxxH, JyyH, JxyH);
            angleX = CalculateAngle(JxxH, JyyH, JxyH);
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
            CalculateStaticMoments(mesh, out double Sx, out double Sy);

            double[] AreaHomogArray = new double[rebars.Count()];
            double[] SxHomogenizedArray = new double[rebars.Count()];
            double[] SyHomogenizedArray = new double[rebars.Count()];

            Parallel.For(0, rebars.Count(), (i) =>
            {
                SxHomogenizedArray[i] += (CalculateN(rebars[i], concreteMaterial) - 1) * rebars[i].Area * rebars[i].Position.Y;
                SyHomogenizedArray[i] += (CalculateN(rebars[i], concreteMaterial) - 1) * rebars[i].Area * rebars[i].Position.X;
            });

            SxHomog = Sx + SxHomogenizedArray.Sum();
            SyHomog = Sy + SyHomogenizedArray.Sum();

            return CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(rebars, concreteMaterial, area));
        }

        /// <summary>
        /// The centroid of the homogenized section with homogenized factor <paramref name="n"/>
        /// </summary>
        /// <param name="n">The homogenized factor</param>
        /// <param name="mesh"></param>
        /// <param name="rebars"></param>
        /// <param name="area"></param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns></returns>
        internal static Point2d GetHomogenizedCentroid(double n, Mesh mesh, ReinforcedConcreteRebar[] rebars, double area, out double SxHomog, out double SyHomog)
        {
            CalculateStaticMoments(mesh, out double Sx, out double Sy);

            double[] SxHomogenizedArray = new double[rebars.Count()];
            double[] SyHomogenizedArray = new double[rebars.Count()];

            Parallel.For(0, rebars.Count(), (i) =>
            {
                SxHomogenizedArray[i] = (n - 1) * rebars[i].Area * rebars[i].Position.Y;
                SyHomogenizedArray[i] = (n - 1) * rebars[i].Area * rebars[i].Position.X;
            });

            SxHomog = Sx + SxHomogenizedArray.Sum();
            SyHomog = Sy + SyHomogenizedArray.Sum();

            return CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(n, rebars, area));
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
        internal static Point2d GetHomogenizedCentroid(double Sx, double Sy, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, double area, out double SxHomog, out double SyHomog)
        {
            double[] AreaHomogArray = new double[rebars.Count()];
            double[] SxHomogenizedArray = new double[rebars.Count()];
            double[] SyHomogenizedArray = new double[rebars.Count()];

            Parallel.For(0, rebars.Count(), (i) =>
            {
                SxHomogenizedArray[i] += (CalculateN(rebars[i], concreteMaterial) - 1) * rebars[i].Area * rebars[i].Position.Y;
                SyHomogenizedArray[i] += (CalculateN(rebars[i], concreteMaterial) - 1) * rebars[i].Area * rebars[i].Position.X;
            });

            SxHomog = Sx + SxHomogenizedArray.Sum();
            SyHomog = Sy + SyHomogenizedArray.Sum();

            return CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(rebars, concreteMaterial, area));
        }

        /// <summary>
        /// The centroid of the homogenized section with homogenized factor <paramref name="n"/>
        /// </summary>
        /// <param name="n">The homogenized factor</param>
        /// <param name="rebars"></param>
        /// <param name="Sx"></param>
        /// <param name="Sy"></param>
        /// <param name="area"></param>
        /// <param name="SxHomog">The first moment of area respect X-Axis</param>
        /// <param name="SyHomog">The first moment of area respect Y-Axis</param>
        /// <returns></returns>
        internal static Point3d GetHomogenizedCentroid(double n, ReinforcedConcreteRebar[] rebars, double Sx, double Sy, double area, out double SxHomog, out double SyHomog)
        {
            double[] SxHomogenizedArray = new double[rebars.Count()];
            double[] SyHomogenizedArray = new double[rebars.Count()];

            Parallel.For(0, rebars.Count(), (i) =>
            {
                SxHomogenizedArray[i] = (n - 1) * rebars[i].Area * rebars[i].Position.Y;
                SyHomogenizedArray[i] = (n - 1) * rebars[i].Area * rebars[i].Position.X;
            });

            SxHomog = Sx + SxHomogenizedArray.Sum();
            SyHomog = Sy + SyHomogenizedArray.Sum();

            return CalculateCentroid(SxHomog, SyHomog, GetHomogenizedArea(n, rebars, area));
        }


        /// <summary>
        /// The homogenized area with default value of homogenized factor n
        /// </summary>
        /// <returns>The homogenized area</returns>
        internal static double GetHomogenizedArea(ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial, double area)
        {
            double[] AreaHomogArray = new double[rebars.Count()];

            Parallel.For(0, rebars.Count(), (i) =>
            {
                AreaHomogArray[i] = (CalculateN(rebars[i], concreteMaterial) - 1) * rebars[i].Area;
            });

            return area + AreaHomogArray.Sum();
        }

        /// <summary>
        /// The homogenized area with homogenized factor <paramref name="n"/>
        /// </summary>
        /// <param name="n"></param>
        /// <param name="rebars"></param>
        /// <param name="area"></param>
        /// <returns>The homogenized area</returns>
        internal static double GetHomogenizedArea(double n, ReinforcedConcreteRebar[] rebars, double area)
        {
            double[] AreaHomogArray = new double[rebars.Count()];

            Parallel.For(0, rebars.Count(), (i) =>
            {
                AreaHomogArray[i] = (n - 1) * rebars[i].Area;
            });

            return area + AreaHomogArray.Sum();
        }

        internal static double GetHomogeneizedJ11(double n, Point2d sectionCentroid, Mesh mesh, ReinforcedConcreteRebar[] rebars, double area, double Jxx, double Jyy, double Jxy)
        {
            Point2d centroidH = GetHomogenizedCentroid(n, mesh, rebars, area, out double _, out double _);
            CalculateHomogeneizedInertiaMoments(n, rebars, sectionCentroid, centroidH, Jxx, Jyy, Jxy, area, out double JxxH, out double JyyH, out double JxyH, out double _);
            return CalculateJ11(JxxH, JyyH, JxyH);
        }

        internal static double GetHomogeneizedJ11(Mesh mesh, Point2d sectionCentroid, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double area, double Jxx, double Jyy, double Jxy)
        {
            Point2d centroidH = GetHomogenizedCentroid(mesh, rebars, concreteMaterial, area, out _, out _);
            CalculateHomogeneizedInertiaMoments(rebars, sectionCentroid, centroidH, concreteMaterial, Jxx, Jyy, Jxy, area,
            out double JxxH, out double JyyH, out double JxyH, out double _);
            return CalculateJ11(JxxH, JyyH, JxyH);
        }

        internal static double GetHomogeneizedJ22(double n, Point2d sectionCentroid, Mesh mesh, ReinforcedConcreteRebar[] rebars, double area, double Jxx, double Jyy, double Jxy)
        {
            Point2d centroidH = GetHomogenizedCentroid(n, mesh, rebars, area, out double _, out double _);
            CalculateHomogeneizedInertiaMoments(n, rebars, sectionCentroid, centroidH, Jxx, Jyy, Jxy, area, out double JxxH, out double JyyH, out double JxyH, out double _);
            return CalculateJ22(JxxH, JyyH, JxyH);
        }

        internal static double GetHomogeneizedJ22(Mesh mesh, Point2d sectionCentroid, ReinforcedConcreteRebar[] rebars, ConcreteMaterial concreteMaterial,
            double area, double Jxx, double Jyy, double Jxy)
        {
            Point2d centroidH = GetHomogenizedCentroid(mesh, rebars, concreteMaterial, area, out _, out _);
            CalculateHomogeneizedInertiaMoments(rebars, sectionCentroid, centroidH, concreteMaterial, Jxx, Jyy, Jxy, area,
            out double JxxH, out double JyyH, out double JxyH, out double _);
            return CalculateJ22(JxxH, JyyH, JxyH);
        }


        internal static ReinforcedConcreteRebar[] SetRadialRebars(double diameter, double concreteCover, int numberOfRebars, IRebarSection rebarSection, Point2d centroid = default, double epsilonP = 0.0)
        {
            Polygon2d polygon = new Polygon2d(diameter - concreteCover * 2.0, numberOfRebars, centroid);

            ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[polygon.Count];

            for (int i = 0; i < polygon.Count; i++)
            {
                rebars[i] = new ReinforcedConcreteRebar(rebarSection, polygon[i], i, epsilonP);
            }

            return rebars;
        }

    }
}
