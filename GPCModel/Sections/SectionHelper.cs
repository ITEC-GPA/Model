using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using GPC.Model.Maths.GaussIntegrations;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    internal static class SectionHelper
    {
        /// <summary>
        /// Generate the mesh of the section. If <paramref name="size"/> not set, size is set as the default value of the minimum of the bounding box size divided by 2.
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <param name="size">The mesh size</param>
        /// <param name="initialMeshOnly">If true, use only shape vertices for meshing</param>
        /// <param name="recombine">If true, recombine the mesh into quad mesh</param>
        /// <param name="refine"></param>
        /// <returns></returns>
        internal static Mesh GenerateMesh(Shape2d shape, double size = 0, bool initialMeshOnly = false, bool recombine = true, bool refine = false)
        {
            if (shape is null)
                return null;

            if (size <= 0)
            {
                BoundingBox3d bBox = shape.GetBoundingBox();
                size = Math.Min(bBox.Size.X, bBox.Size.Y) / 2.0;
            }

            DelaunayMesh.DelaunayGenerateOptions generateOptions = new DelaunayMesh.DelaunayGenerateOptions()
            {
                Recombine = recombine,
                MeshSize = size,
                InitialMeshOnly = initialMeshOnly,
                Refine = refine,
            };

            if (DelaunayMesh.Generate(shape, generateOptions, out Mesh meshes, out DelaunayMesh.DelaunayGenerateMeshStatus meshStatus))
                return meshes;
            else
                throw new ArgumentException($"Fail to create mesh. {meshStatus.GetLastCustomErrorMessage()}");
        }

        internal static Point2d CalculateCentroid(double Sx, double Sy, double area)
        {
            if (area == 0)
                throw new ArgumentException();

            return new Point2d(Sy / area, Sx / area);
        }

        internal static double CalculateAngle(double Jxx, double Jyy, double Jxy)
        {
            double angle = -1.0 / 2.0 * Math.Atan2(2.0 * Jxy, (Jxx - Jyy));

            if (Math.Abs(angle - Math.PI) < GeometryBase.GetDefaultAngularTolerance() || Math.Abs(angle) < GeometryBase.GetDefaultAngularTolerance())
                return 0.0;

            return angle;
        }

        internal static double CalculateAngle(double J11, double J22, double Jxx, double Jyy, double Jxy)
        {
            if (Math.Abs(J11 - Jxx) < GeometryBase.GetDefaultTolerance() &&
                Math.Abs(J22 - Jyy) < GeometryBase.GetDefaultTolerance())
                return 0.0;

            return CalculateAngle(Jxx, Jyy, Jxy);
        }

        internal static double CalculateJ11(double Jxx, double Jyy, double Jxy)
        {
            return (Jxx + Jyy) / 2.0 + 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(Jxy, 2));
        }

        internal static double CalculateJ22(double Jxx, double Jyy, double Jxy)
        {
            return (Jxx + Jyy) / 2.0 - 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(Jxy, 2));
        }

        /// <summary>
        /// Calculate moment of inertia in alpha direction, counterclockwise rotation, zero for positive X axis.
        /// Given the moments of inertia with respect to the x and y axes determine the moment of inertia with respect
        /// to an axis rotated by alpha passing through the origin.
        /// It corresponds to rotating the section by an angle equal to -alpha and determining the moment of inertia
        /// with respect to the x-axis.
        /// </summary>
        /// <param name="Jxx">Moment of inertia with respect to the X axis.</param>
        /// <param name="Jyy">Moment of inertia with respect to the Y axis.</param>
        /// <param name="Jxy">Product of inertia with respect to the X and Y axes.</param>
        /// <param name="alpha">Angle of the axis with respect to which to calculate the moment of inertia.</param>
        /// <returns></returns>
        internal static double CalculateJAlpha(in double Jxx, in double Jyy, in double Jxy, in double alpha)
        {
            double cosAlpha = Math.Cos(alpha);
            double sinAlpha = Math.Sin(alpha);
            return Jxx * cosAlpha * cosAlpha + Jyy * sinAlpha * sinAlpha - 2.0 * Jxy * sinAlpha * cosAlpha;
        }

        /// <summary>
        /// Calculate product of inertia in alpha direction, counterclockwise rotation, zero for positive X axis.
        /// Given the moments of inertia with respect to the x and y axes determine the moment of inertia with respect
        /// to an axis rotated by alpha passing through the origin.
        /// It corresponds to rotating the section by an angle equal to -alpha and determining the product of inertia
        /// with respect to the x and y axes.
        /// </summary>
        /// <param name="Jxx">Moment of inertia with respect to the X axis.</param>
        /// <param name="Jyy">Moment of inertia with respect to the Y axis.</param>
        /// <param name="Jxy">Product of inertia with respect to the X and Y axes.</param>
        /// <param name="alpha">Angle of the axis with respect to which to calculate the moment of inertia.</param>
        /// <returns></returns>
        internal static double CalculateJxyAlpha(in double Jxx, in double Jyy, in double Jxy, in double alpha)
        {
            double cosAlpha = Math.Cos(alpha);
            double sinAlpha = Math.Sin(alpha);
            return (Jxx - Jyy) * sinAlpha * cosAlpha + Jxy * (cosAlpha * cosAlpha - sinAlpha * sinAlpha);
        }

        internal static void CalculateIntegralInertiaMoment(Mesh mesh, MeshFace face, Point2d centroid, out double jxx, out double jyy, out double jxy)
        {
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

        internal static void CalculateInertiaMoments(Mesh mesh, Point2d centroid, out double Jxx, out double Jyy, out double Jxy, out double Jp)
        {
            double[] JxxArray = new double[mesh.FacesCount];
            double[] JyyArray = new double[mesh.FacesCount];
            double[] JxyArray = new double[mesh.FacesCount];

            Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, mesh.FacesCount), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    MeshFace meshFace = mesh.Faces.ElementAt(i);

                    CalculateIntegralInertiaMoment(mesh, meshFace, centroid, out double jxx, out double jyy, out double jxy);

                    JxxArray[i] = jxx;
                    JyyArray[i] = jyy;
                    JxyArray[i] = jxy;
                }
            });

            Jxx = JxxArray.Sum();
            Jyy = JyyArray.Sum();
            Jxy = JxyArray.Sum();
            Jp = Jxx + Jyy;

            if (Math.Abs(Jxy) < 100000)
                Jxy = 0.0; // Se la sezione simmetrica vale zero e pu� diventare negativo per errore numerico 
        }

        internal static void CalculateStaticMoments(Mesh mesh, out double Sx, out double Sy)
        {
            double[] SxArray = new double[mesh.FacesCount];
            double[] SyArray = new double[mesh.FacesCount];

            Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, mesh.FacesCount), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    MeshFace meshFace = mesh.Faces.ElementAt(i);

                    double area = mesh.GetFaceArea(meshFace);
                    Point2d centroid = mesh.GetFaceCentroid(meshFace);

                    SxArray[i] = area * centroid.Y;
                    SyArray[i] = area * centroid.X;
                }
            });

            Sx = SxArray.Sum();
            Sy = SyArray.Sum();
        }
    }
}
