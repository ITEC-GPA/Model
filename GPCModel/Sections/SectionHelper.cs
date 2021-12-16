using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Geometry.Meshes;

namespace GPC.Model.Sections
{

    internal static class SectionHelper
    {

        /// <summary>
        /// Generate the mesh of the section. If <paramref name="size"/> not set, size is set as the default value of the minimum of the bounding box size divided by 2.
        /// </summary>
        /// <param name="shape">The shape</param>
        /// <param name="size">The mesh size</param>
        /// <returns></returns>
        internal static Mesh GenerateMesh(Shape2d shape, double size = 0)
        {
            if (shape is null)
                return null;


            if (size <= 0)
            {
                BoundingBox3d bBox = shape.GetBoundingBox();
                size = Math.Max(bBox.Size.X, bBox.Size.Y);
            }

            Mesh.GenerateOptions generateOptions = new Mesh.GenerateOptions()
            {
                Algorithm = Mesh.GenerateOptions.MeshAlgorithm.FrontalDelaunayForQuads,
                Recombine = true,
                RecombinationAlgorithm = Mesh.GenerateOptions.RecombinationMeshAlgorithm.SimpleFullQuad,
                UseGlobalProgressID = true,

                MeshSize = size,
            };

            if (Mesh.Generate(new Shape2d[] { shape }, generateOptions, out List<Mesh> meshes, out Mesh.GenerateMeshStatus meshStatus))
                return meshes[0];
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
            double angle = -1.0 / 2.0 * Math.Atan2(2.0 * Jxy, (Jyy - Jxx));

            if (Jyy < Jxx)
                angle += Math.PI / 2.0;

            if (Math.Abs(angle - Math.PI) < GeometryBase.GetDefaultAngularTolerance())
                return 0.0;

            if (Math.Abs(angle) < GeometryBase.GetDefaultAngularTolerance())
                return 0.0;

            return angle;
        }

        internal static double CalculateJ11(double Jxx, double Jyy, double Jxy)
        {
            return (Jxx + Jyy) / 2.0 + 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(Jxy, 2));
        }

        internal static double CalculateJ22(double Jxx, double Jyy, double Jxy)
        {
            return (Jxx + Jyy) / 2.0 - 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(Jxy, 2));
        }


    }
}
