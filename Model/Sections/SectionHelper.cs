using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Geometry.Meshes.DelaunayMesh;
using GPC.Model.Maths.GaussIntegrations;
using System;
using System.Collections.Generic;
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
        /// <param name="recombine">If true, quadrilaterals as square as possible (see <see cref="DelaunayMesh"/>); false (default): triangles,
        /// the mesh used by the checks until September 2026 (Recombine was not implemented)</param>
        /// <param name="refine"></param>
        /// <returns></returns>
        internal static Mesh GenerateMesh(Shape2d shape, double size = 0, bool initialMeshOnly = false, bool recombine = false, bool refine = false)
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
            // Change due to .NET Core, change in (Jxx - Jyy) that is not zero if two double are equals.
            // If ( (Jxx = Jyy) && (Jxy! = 0))
            // the ellipse degenerates into a circle and all directions are principal.
            double ZERO = 1e-12;
            if (Math.Abs((Jyy - Jxx) / (Jyy + Jxx)) < ZERO)
            {
                if (Math.Abs((Jxy) / (Jyy + Jxx)) < ZERO)
                {
                    // in this case we are in the presence of a gyroscope and all axes are principal
                    // the inertia matrix is in the form
                    // I 0
                    // 0 I
                    return 0.0;
                }
                else
                {
                    // in this case the inertia matrix is in the form
                    // IX IXY
                    // IXY IX
                    // the eigenvectors are 45° and the eigenvalues are the principal inertias
                    // and have distinct values: the axis of the maximum moment is at -45° if IXY > 0, at +45° if IXY < 0, as in the general formula
                    // below (before, always +45°: with IXY > 0 it was the axis of the minimum moment, e.g. an equal leg angle mirrored)
                    return Jxy > 0 ? -Math.PI / 4.0 : Math.PI / 4.0;
                }
            }

            double angle = -1.0 / 2.0 * Math.Atan2(2.0 * Jxy, (Jxx - Jyy));

            if (Math.Abs(angle - Math.PI) < GeometryBase.GetDefaultAngularTolerance() || Math.Abs(angle) < GeometryBase.GetDefaultAngularTolerance())
                return 0.0;

            return angle;
        }

        /// <summary>
        /// The angle of the principal axis 1, the one of the maximum moment of inertia <paramref name="J11"/>, from the X axis
        /// </summary>
        /// <returns>0 if the X axis is the axis 1; -pi/2 if X and Y are principal and Jyy is bigger than Jxx (the axis 1 is the Y axis, directed
        /// towards the negative Y, as the angle given by <see cref="CalculateAngle(double, double, double)"/>); otherwise the angle of the axis 1</returns>
        /// <remarks>The comparisons are relative (before, absolute with the geometric tolerance 1e-4: for a section in metres, with moments of
        /// about 1e-7, the angle was always zero); with X and Y principal the angle is exactly 0 or -pi/2 (before, -pi/2 or +pi/2 depending on
        /// the sign of a zero product of inertia)</remarks>
        internal static double CalculateAngle(double J11, double J22, double Jxx, double Jyy, double Jxy)
        {
            if (IsProductOfInertiaZero(Jxx, Jyy, Jxy))
                return XIsAxisOne(Jxx, Jyy) ? 0.0 : -Math.PI / 2.0;

            double tolerance = ProductOfInertiaZero * (Math.Abs(J11) + Math.Abs(J22));
            if (Math.Abs(J11 - Jxx) <= tolerance && Math.Abs(J22 - Jyy) <= tolerance)
                return 0.0;

            return CalculateAngle(Jxx, Jyy, Jxy);
        }

        /// <summary>
        /// The product of inertia is zero (the X and Y axes are principal) if it is negligible respect to the moments of inertia
        /// </summary>
        internal static bool IsProductOfInertiaZero(double Jxx, double Jyy, double Jxy)
        {
            return Math.Abs(Jxy) <= ProductOfInertiaZero * (Math.Abs(Jxx) + Math.Abs(Jyy));
        }

        private const double ProductOfInertiaZero = 1e-10;

        /// <summary>
        /// With X and Y principal: true if the axis 1 is X, that is if Jxx is the bigger moment or if the two moments are equal but for the rounding
        /// errors (all the axes are principal, e.g. a circle or a square); false if the axis 1 is Y
        /// </summary>
        private static bool XIsAxisOne(double Jxx, double Jyy)
        {
            return Jxx >= Jyy - ProductOfInertiaZero * (Math.Abs(Jxx) + Math.Abs(Jyy));
        }

        /// <returns>The maximum principal moment of inertia (the moment respect to the axis 1)</returns>
        /// <remarks>The axis 1 is always the principal axis of the maximum moment, also when X and Y are principal (e.g. for a wide rectangle
        /// J11 = Jyy and the axis 1 is the Y axis): see <see cref="CalculateAngle(double, double, double, double, double)"/></remarks>
        internal static double CalculateJ11(double Jxx, double Jyy, double Jxy)
        {
            if (IsProductOfInertiaZero(Jxx, Jyy, Jxy))
                return XIsAxisOne(Jxx, Jyy) ? Jxx : Jyy;

            return (Jxx + Jyy) / 2.0 + 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(Jxy, 2));
        }

        /// <returns>The minimum principal moment of inertia (the moment respect to the axis 2)</returns>
        internal static double CalculateJ22(double Jxx, double Jyy, double Jxy)
        {
            if (IsProductOfInertiaZero(Jxx, Jyy, Jxy))
                return XIsAxisOne(Jxx, Jyy) ? Jyy : Jxx;

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

            if (IsProductOfInertiaZero(Jxx, Jyy, Jxy)) // before, |Jxy| < 100000: wrong for the sections in metres and for the small ones in mm
                Jxy = 0.0; // Se la sezione simmetrica vale zero e pu� diventare negativo per errore numerico 
        }

        /// <summary>
        /// Static moments of the mesh faces in the X-Y plane: sum of the face area (absolute value) by the face centroid.
        /// The quads are divided in two triangles from the first vertex, as in <see cref="Polygon3d.GetCentroid"/>
        /// </summary>
        internal static void CalculateStaticMoments(Mesh mesh, out double Sx, out double Sy)
        {
            Sx = 0;
            Sy = 0;

            for (int i = 0; i < mesh.FacesCount; i++)
            {
                MeshFace face = mesh.Faces[i];
                Point3d a = mesh.Vertices.GetElementById(face.A).Point;
                Point3d b = mesh.Vertices.GetElementById(face.B).Point;
                Point3d c = mesh.Vertices.GetElementById(face.C).Point;

                // twice the signed area and the (area x centroid) of the triangles
                double area2 = (b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y);
                double momentX = area2 * (a.Y + b.Y + c.Y);
                double momentY = area2 * (a.X + b.X + c.X);

                if (face.IsQuad)
                {
                    Point3d d = mesh.Vertices.GetElementById(face.D).Point;
                    double area2Second = (c.X - a.X) * (d.Y - a.Y) - (d.X - a.X) * (c.Y - a.Y);
                    area2 += area2Second;
                    momentX += area2Second * (a.Y + c.Y + d.Y);
                    momentY += area2Second * (a.X + c.X + d.X);
                }

                // area * centroid = |A| * (moment / A)
                double sign = area2 < 0 ? -1.0 : 1.0;
                Sx += sign * momentX / 6.0;
                Sy += sign * momentY / 6.0;
            }
        }

        /// <summary>
        /// Static moments of the shape (fill minus holes plus childs, the region meshed by <see cref="GenerateMesh"/>), exact
        /// </summary>
        internal static void CalculateStaticMoments(Shape2d shape, out double Sx, out double Sy)
        {
            if (shape is null || shape.Fill.Count == 0)
            {
                Sx = 0;
                Sy = 0;
                return;
            }

            // integration relative to a point of the shape, for a better precision
            Point3d origin = shape.Fill[0];
            IntegrateShape(shape, origin.X, origin.Y, out double area, out double sx, out double sy, out _, out _, out _);

            Sx = sx + area * origin.Y;
            Sy = sy + area * origin.X;
        }

        /// <summary>
        /// Moments of inertia of the shape (fill minus holes plus childs, the region meshed by <see cref="GenerateMesh"/>) respect to the axes through <paramref name="centroid"/>, exact
        /// </summary>
        internal static void CalculateInertiaMoments(Shape2d shape, Point2d centroid, out double Jxx, out double Jyy, out double Jxy, out double Jp)
        {
            IntegrateShape(shape, centroid.X, centroid.Y, out _, out _, out _, out Jxx, out Jyy, out Jxy);
            Jp = Jxx + Jyy;

            if (IsProductOfInertiaZero(Jxx, Jyy, Jxy)) // before, |Jxy| < 100000: wrong for the sections in metres and for the small ones in mm
                Jxy = 0.0; // Se la sezione simmetrica vale zero e può diventare negativo per errore numerico
        }

        /// <summary>
        /// Integrals on the shape region (fill minus holes, plus childs) computed on the boundary with the Green's theorem: exact for polygons.
        /// The coordinates are relative to (<paramref name="originX"/>, <paramref name="originY"/>)
        /// </summary>
        /// <param name="area">Integral of dA</param>
        /// <param name="sx">Integral of y dA</param>
        /// <param name="sy">Integral of x dA</param>
        /// <param name="ixx">Integral of y^2 dA</param>
        /// <param name="iyy">Integral of x^2 dA</param>
        /// <param name="ixy">Integral of x y dA</param>
        internal static void IntegrateShape(Shape shape, double originX, double originY,
            out double area, out double sx, out double sy, out double ixx, out double iyy, out double ixy)
        {
            area = 0; sx = 0; sy = 0; ixx = 0; iyy = 0; ixy = 0;
            AddShapeIntegrals(shape, originX, originY, ref area, ref sx, ref sy, ref ixx, ref iyy, ref ixy);
        }

        private static void AddShapeIntegrals(Shape shape, double originX, double originY,
            ref double area, ref double sx, ref double sy, ref double ixx, ref double iyy, ref double ixy)
        {
            if (shape is null)
                return;

            AddPolygonIntegrals(shape.Fill, 1.0, originX, originY, ref area, ref sx, ref sy, ref ixx, ref iyy, ref ixy);

            if (shape.Holes != null)
            {
                for (int i = 0; i < shape.Holes.Length; i++)
                    AddPolygonIntegrals(shape.Holes[i], -1.0, originX, originY, ref area, ref sx, ref sy, ref ixx, ref iyy, ref ixy);
            }

            // shapes inside the holes
            if (shape.Childs != null)
            {
                for (int i = 0; i < shape.Childs.Length; i++)
                    AddShapeIntegrals(shape.Childs[i], originX, originY, ref area, ref sx, ref sy, ref ixx, ref iyy, ref ixy);
            }
        }

        /// <summary>
        /// Plastic modulus of the shape (fill minus holes plus childs) for the bending about an axis with direction <paramref name="angle"/>:
        /// the integral of the distance from the plastic neutral axis, the parallel axis that divides the area in two halves. Exact for polygons
        /// </summary>
        /// <param name="angle">The direction of the axis (0: X axis, <see cref="Math.PI"/> / 2: Y axis), as the angle of the elastic moduli</param>
        internal static double CalculatePlasticModulus(Shape shape, Point2d centroid, double angle)
        {
            if (shape is null || shape.Fill is null || shape.Fill.Count < 3)
                return 0.0;

            // the rings in the coordinates (u, w): u along the axis, w = (y - yc) cos - (x - xc) sin the distance from it
            double cos = Math.Cos(angle), sin = Math.Sin(angle);
            var rings = new List<(double[] u, double[] w, double factor)>();
            AddPlasticRings(shape, centroid, cos, sin, 1.0, rings);

            double wMin = double.MaxValue, wMax = double.MinValue;
            foreach (var ring in rings)
            {
                for (int i = 0; i < ring.w.Length; i++)
                {
                    wMin = Math.Min(wMin, ring.w[i]);
                    wMax = Math.Max(wMax, ring.w[i]);
                }
            }

            ClippedIntegrals(rings, double.MinValue, out double area, out double firstMoment);
            if (area <= 0)
                return 0.0;

            // the plastic neutral axis: area above it = half the area (the area above w = c decreases from area to 0)
            double low = wMin, high = wMax;
            for (int k = 0; k < 200 && high - low > 1e-14 * (wMax - wMin); k++)
            {
                double c = 0.5 * (low + high);
                ClippedIntegrals(rings, c, out double areaAbove, out _);
                if (areaAbove > 0.5 * area)
                    low = c;
                else
                    high = c;
            }

            double neutralAxis = 0.5 * (low + high);
            ClippedIntegrals(rings, neutralAxis, out double aAbove, out double qAbove);
            double aBelow = area - aAbove, qBelow = firstMoment - qAbove;

            // integral of |w - c|: (w - c) above the axis, (c - w) below it
            return (qAbove - neutralAxis * aAbove) + (neutralAxis * aBelow - qBelow);
        }

        /// <param name="sign">1 for a region to add, -1 for a region to subtract</param>
        private static void AddPlasticRings(Shape shape, Point2d centroid, double cos, double sin, double sign, List<(double[] u, double[] w, double factor)> rings)
        {
            AddPlasticRing(shape.Fill, centroid, cos, sin, sign, rings);

            if (shape.Holes != null)
            {
                for (int i = 0; i < shape.Holes.Length; i++)
                    AddPlasticRing(shape.Holes[i], centroid, cos, sin, -sign, rings);
            }

            // shapes inside the holes
            if (shape.Childs != null)
            {
                for (int i = 0; i < shape.Childs.Length; i++)
                    AddPlasticRings(shape.Childs[i], centroid, cos, sin, sign, rings);
            }
        }

        private static void AddPlasticRing(Polygon3d polygon, Point2d centroid, double cos, double sin, double sign, List<(double[] u, double[] w, double factor)> rings)
        {
            int count = polygon.Count;
            if (count < 3)
                return;

            var u = new double[count];
            var w = new double[count];
            double doubleArea = 0;
            for (int i = 0; i < count; i++)
            {
                double x = polygon[i].X - centroid.X, y = polygon[i].Y - centroid.Y;
                u[i] = x * cos + y * sin;
                w[i] = y * cos - x * sin;
            }
            for (int i = 0; i < count; i++)
            {
                int next = (i + 1) % count;
                doubleArea += u[i] * w[next] - u[next] * w[i];
            }

            // the integrals are positive for counterclockwise rings (the rotation keeps the orientation)
            rings.Add((u, w, doubleArea < 0 ? -sign : sign));
        }

        /// <summary>
        /// Area and first moment (integral of w dA) of the part of the rings with w >= <paramref name="c"/>: every ring is clipped by the half-plane
        /// (Sutherland-Hodgman) and integrated with the Green's theorem; the sides added along w = c do not change the integrals of the other rings
        /// </summary>
        private static void ClippedIntegrals(List<(double[] u, double[] w, double factor)> rings, double c, out double area, out double firstMoment)
        {
            area = 0;
            firstMoment = 0;

            foreach (var ring in rings)
            {
                double[] u = ring.u, w = ring.w;
                int count = u.Length;
                double a = 0, m = 0;
                bool hasFirst = false;
                double u0 = 0, w0 = 0, uPrevious = 0, wPrevious = 0;

                void AddVertex(double uv, double wv)
                {
                    if (!hasFirst)
                    {
                        u0 = uv; w0 = wv;
                        hasFirst = true;
                    }
                    else
                    {
                        double cross = uPrevious * wv - uv * wPrevious;
                        a += cross;
                        m += (wPrevious + wv) * cross;
                    }
                    uPrevious = uv;
                    wPrevious = wv;
                }

                for (int i = 0; i < count; i++)
                {
                    int next = (i + 1) % count;
                    double dp = w[i] - c, dq = w[next] - c;

                    if (dp >= 0)
                        AddVertex(u[i], w[i]);
                    if ((dp >= 0) != (dq >= 0))
                        AddVertex(u[i] + (u[next] - u[i]) * dp / (dp - dq), c);
                }

                if (!hasFirst)
                    continue;

                // closing side
                double closing = uPrevious * w0 - u0 * wPrevious;
                a += closing;
                m += (wPrevious + w0) * closing;

                area += ring.factor * a / 2.0;
                firstMoment += ring.factor * m / 6.0;
            }
        }

        /// <param name="sign">1 to add the polygon region, -1 to subtract it. The orientation of the polygon does not matter</param>
        private static void AddPolygonIntegrals(Polygon3d polygon, double sign, double originX, double originY,
            ref double area, ref double sx, ref double sy, ref double ixx, ref double iyy, ref double ixy)
        {
            int count = polygon.Count;
            double a = 0, x1 = 0, y1 = 0, xx = 0, yy = 0, xy = 0;

            for (int i = 0; i < count; i++)
            {
                Point3d p = polygon[i];
                Point3d q = polygon[(i + 1) % count];
                double x0 = p.X - originX, y0 = p.Y - originY;
                double xn = q.X - originX, yn = q.Y - originY;
                double cross = x0 * yn - xn * y0;

                a += cross;
                x1 += (x0 + xn) * cross;
                y1 += (y0 + yn) * cross;
                xx += (x0 * x0 + x0 * xn + xn * xn) * cross;
                yy += (y0 * y0 + y0 * yn + yn * yn) * cross;
                xy += (x0 * yn + 2.0 * x0 * y0 + 2.0 * xn * yn + xn * y0) * cross;
            }

            // the formulas give positive values for counterclockwise polygons
            double factor = a < 0 ? -sign : sign;

            area += factor * a / 2.0;
            sy += factor * x1 / 6.0;
            sx += factor * y1 / 6.0;
            iyy += factor * xx / 12.0;
            ixx += factor * yy / 12.0;
            ixy += factor * xy / 24.0;
        }
    }
}
