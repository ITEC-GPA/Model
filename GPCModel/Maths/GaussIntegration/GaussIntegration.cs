using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using MathNet.Numerics.LinearAlgebra;
using GPC.Utilities.Fem;

namespace GPC.Model.Maths.GaussIntegrations
{
	public static class GaussIntegration
	{
		#region Triangular element

		public static double IntegrationTriangular(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints, Func<int, double, double, double> shapeFunction, 
            Func<int, double, double, double> dNdCsi, Func<int, double, double, double> dNdEta, int numberOFShapeFunction)
		{
            if (vertices.Length != 3)
                throw new ArgumentException("Point must be 3. Polygon must be a triangle");

            GaussPoint[] gaussPoints;

            switch (numberOfGaussPoints)
            {
                case 1:
                    gaussPoints = TriangleGaussPoints.Tri1;
                    break;
                case 3:
                    gaussPoints = TriangleGaussPoints.Tri3;
                    break;
                case 4:
                    gaussPoints = TriangleGaussPoints.Tri4;
                    break;
                case 6:
                    gaussPoints = TriangleGaussPoints.Tri6;
                    break;
                case 12:
                    gaussPoints = TriangleGaussPoints.Tri12;
                    break;
                case 33:
                    gaussPoints = TriangleGaussPoints.Tri33;
                    break;

                default:
                    throw new ArgumentException("Wrong number of Gauss Points");
            }

            Point3d[] shapeFunctionNode = new Point3d[numberOFShapeFunction];

            if (numberOFShapeFunction == vertices.Length)
            {
                shapeFunctionNode = vertices;
            }
            else
            {
                Parallel.For(0, vertices.Length, (i) =>
                {
                    shapeFunctionNode[i] = vertices[i];
                });

                Parallel.For(0, vertices.Length, (i) =>
                {
                    if (i != vertices.Length - 1)
                        shapeFunctionNode[vertices.Length + i] = (vertices[i] + vertices[i + 1]) / 2.0;
                    else
                        shapeFunctionNode[vertices.Length + i] = (vertices[i] + vertices[0]) / 2.0;
                });
            }

            var jacobian = JacobianMatrix2D(dNdCsi, dNdEta, shapeFunctionNode);

            double[] ris = new double[gaussPoints.Length];

            Parallel.For(0, gaussPoints.Length, (i) =>
            {
                Point3d point = GaussIntegration.GetLocalCoordinate2D(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, shapeFunctionNode);
                ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(point.X, point.Y);
            });
            
            return ris.Sum() / 2.0;
        }

        public static double IntegrationTriangularLinearShapeFunction(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints)
		{
            return IntegrationTriangular(function, vertices, numberOfGaussPoints, LinearShapeFunctionsTri3.NaturalShapeFunction, 
                LinearShapeFunctionsTri3.DNdCsi, LinearShapeFunctionsTri3.DNdEta, 3);
		}

        public static double IntegrationTriangularQuadraticShapeFunction(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints)
        {
            return IntegrationTriangular(function, vertices, numberOfGaussPoints, QuadraticShapeFunctionsTri6.NaturalShapeFunction, 
                QuadraticShapeFunctionsTri6.DNdCsi, QuadraticShapeFunctionsTri6.DNdEta, 6);
        }

		#endregion

		#region Quadrangular element

		public static double IntegrationQuadrilateral(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints, Func<int, double, double, double> shapeFunction,
            Func<int, double, double, double> derivRespectCsi, Func<int, double, double, double> derivRespectEta, int numberOFShapeFunction)
        {
            if (vertices.Length != 4)
                throw new ArgumentException("Polygon must be a quadrilateral");

            GaussPoint[] gaussPoints;

            switch (numberOfGaussPoints)
            {
                case 1:
                    gaussPoints = QuadrangleGaussPoints.Quad1;
                    break;
                case 4:
                    gaussPoints = QuadrangleGaussPoints.Quad4;
                    break;
                case 8:
                    gaussPoints = QuadrangleGaussPoints.Quad8;
                    break;
                case 12:
                    gaussPoints = QuadrangleGaussPoints.Quad12;
                    break;

                default:
                    throw new ArgumentException("Wrong number of Gauss Points");
            }

            Point3d[] shapeFunctionNode = new Point3d[numberOFShapeFunction];

            if (numberOFShapeFunction == vertices.Length)
            {
                shapeFunctionNode = vertices;
            }
            else
            {
                Parallel.For(0, vertices.Length, (i) =>
                {
                    shapeFunctionNode[i] = vertices[i];
                });

                Parallel.For(0, vertices.Length, (i) =>
                {
                    if (i != vertices.Length - 1)
                        shapeFunctionNode[vertices.Length + i] = (vertices[i] + vertices[i + 1]) / 2.0;
                    else
                        shapeFunctionNode[vertices.Length + i] = (vertices[i] + vertices[0]) / 2.0;
                });
            }

            var jacobian = JacobianMatrix2D(derivRespectCsi, derivRespectEta, shapeFunctionNode);

            double[] ris = new double[gaussPoints.Length];

            Parallel.For(0, gaussPoints.Length, (i) =>
            {
                Point3d point = GaussIntegration.GetLocalCoordinate2D(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, shapeFunctionNode);
                ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(point.X, point.Y);
            });

            return ris.Sum();
        }

        public static double IntegrationQuadrilateralLinearShapeFunction(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints)
        {
            return IntegrationQuadrilateral(function, vertices, numberOfGaussPoints, LinearShapeFunctionQuad4.NaturalShapeFunction,
                LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, 4);
        }

        public static double IntegrationQuadrilateralQuadraticShapeFunction(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints)
        {
            return IntegrationQuadrilateral(function, vertices, numberOfGaussPoints, QuadraticShapeFunctionQuad8.NaturalShapeFunction,
                QuadraticShapeFunctionQuad8.DNdCsi, QuadraticShapeFunctionQuad8.DNdEta, 8);
        }

		#endregion

        #region Hexaedron element

        /// <summary>
        /// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>
        /// </summary>
        /// <param name="function">The function (with variables x and y) to integrate</param>
        /// <param name="vertices">The vertices of the domain. Vertices must be 8</param>
        /// <param name="numberOfGaussPoints">The number of Gauss points</param>
        /// <param name="shapeFunction">The shape function for coordinate transformation</param>
        /// <param name="dNdCsi">The partial derivative of shape function respect the variable csi</param>
        /// <param name="dNdEta">The partial derivative of shape function respect the variable eta</param>/param>
        /// <param name="dNdZeta">The partial derivative of shape function respect the variable zeta</param>/param>
        /// <param name="numberOFShapeFunction">The number of shape function</param>
        /// <returns>The value of the integral</returns>
        /// <remarks>The vertices must be added with this order:
        /// Bottom, clockwise order. Top, clockwise order. The 1st must be associated with 5th, 2nd with 6th, 3rd with 7th and 4th with 8th
        /// </remarks>
        public static double IntegrationHexaedron(Func<double, double, double, double> function, Point3d[] vertices, int numberOfGaussPoints,
            Func<int, double, double, double, double> shapeFunction, Func<int, double, double, double, double> dNdCsi,
            Func<int, double, double, double, double> dNdEta, Func<int, double, double, double, double> dNdZeta, int numberOFShapeFunction)
        {
            if (vertices.Length != 8)
                throw new ArgumentException("Points must be 8. Polygon must be a Hexaedron");

            GaussPoint[] gaussPoints;

            switch (numberOfGaussPoints)
            {
                case 1:
                    gaussPoints = HexahedroGaussPoints.Hexa1;
                    break;
                case 8:
                    gaussPoints = HexahedroGaussPoints.Hexa8;
                    break;

                default:
                    throw new ArgumentException("Wrong number of Gauss Points");
            }

            Point3d[] shapeFunctionNode = new Point3d[numberOFShapeFunction];

            if (numberOFShapeFunction == vertices.Length)
            {
                shapeFunctionNode = vertices;
            }
            else
            {
                throw new NotImplementedException("Quadratic shape function not implemented");
            }

            var jacobian = JacobianMatrix3D(dNdCsi, dNdEta, dNdZeta, shapeFunctionNode);

            double[] ris = new double[gaussPoints.Length];

            Parallel.For(0, gaussPoints.Length, (i) =>
            {
                Point3d point = GaussIntegration.TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, gaussPoints[i].Eta, gaussPoints[i].Zeta, shapeFunction, shapeFunctionNode);
                ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta, gaussPoints[i].Zeta).Determinant() * function(point.X, point.Y, point.Z);
            });

            return ris.Sum();
        }

        public static double IntegrationHexaedronLinearShapeFunction(Func<double, double, double, double> function, Point3d[] vertices, int numberOfGaussPoints)
        {
            return IntegrationHexaedron(function, vertices, numberOfGaussPoints, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction,
                TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, 8);
        }

        #endregion

        #region 1D

		/// <summary>
		/// Matrice jacobiana per cambiamento di variabile
		/// dN/dCsi = dx/dCsi * dN/dx + dy/dCsi * dN/dy
		/// dN/dEta = dx/dEta * dN/dx + dy/dEta * dN/dy
		/// => dN/dNatural = J * dN/dLocal;
		/// => dN/dLocal = J^-1 * dN/dNatural;
		/// => dF/dNatural = J^-1 dF/dLocal;
		/// </summary>
		/// <param name="csi">coordinata naturale</param>
		/// <param name="dNdCsi">derivata funzioni di forma rispetto a Csi che descrive la GEOMETRIA (passaggio da coordinate locali a naturali) in funzione dell'indice di nodo e coordinate naturali</param>
		/// <param name="points"></param>
		/// <returns>
		/// dx/dCsi, dy/dCsi
		/// dy/dEta, dy/dEta
		/// </returns>
		public static Matrix<double> Jacob1D(double csi, Func<int, double, double> dNdCsi, Point3d[] points)
        {
            Matrix<double> J = Matrix<double>.Build.Dense(2, 2);

            J[0, 0] = points[0].X * dNdCsi(1, csi) + points[0].X * dNdCsi(2, csi);
            J[0, 1] = 0;
            J[1, 0] = 0;
            J[1, 1] = 1;

            return J;
        }

        /// <summary>
        /// Return J(csi,eta) = J(csi,eta,dNdCsi, dNdEta,nodes) with "nodes" and derivative of shape function assigned
        /// arg1 = dFdInput1; arg1 = dFdInput2, arg3 = nodes
        /// </summary>
        public static Func<double, Matrix<double>> JacobianMatrix1D(Func<int, double, double> dFdInput1, Point3d[] points)
        {
            return (double csi) => Jacob1D(csi, dFdInput1, points);
        }

        #endregion

        #region 2D

        /// <summary>
        /// Return J(csi,eta) = J(csi,eta,dNdCsi, dNdEta,nodes) with "nodes" and derivative of shape function assigned
        /// arg1 = dFdInput1; arg1 = dFdInput2, arg3 = nodes
        /// </summary>
        public static Func<double, double, Matrix<double>> JacobianMatrix2D(Func<int, double, double, double> dNdCsi, 
            Func<int, double, double, double> dNdEta, Point3d[] points)
        {
            return (double csi, double eta) => Jacob2D(csi, eta, dNdCsi, dNdEta, points);
        }

        /// <summary>
        /// Matrice jacobiana per cambiamento di variabile
        /// dN/dCsi = dx/dCsi * dN/dx + dy/dCsi * dN/dy
        /// dN/dEta = dx/dEta * dN/dx + dy/dEta * dN/dy
        /// => dN/dNatural = J * dN/dLocal;
        /// => dN/dLocal = J^-1 * dN/dNatural;
        /// => dF/dNatural = J^-1 dF/dLocal;
        /// </summary>
        /// <param name="csi">coordinata naturale</param>
        /// <param name="eta">coordinata naturale</param>
        /// <param name="dNdCsi">derivata funzioni di forma rispetto a Csi che descrive la GEOMETRIA (passaggio da coordinate locali a naturali) in funzione dell'indice di nodo e coordinate naturali</param>
        /// <param name="dNdEta">derivata funzioni di forma rispetto a Eta che descrive la GEOMETRIA (passaggio da coordinate locali a naturali) in funzione dell'indice di nodo e coordinate naturali</param>
        /// <param name="points">The domain</param>
        /// <returns>
        /// dx/dCsi, dy/dCsi
        /// dy/dEta, dy/dEta
        /// </returns>
        public static Matrix<double> Jacob2D(double csi, double eta, Func<int, double, double, double> dNdCsi,
            Func<int, double, double, double> dNdEta, Point3d[] points)
        {
            double[] j11 = new double[points.Length];
            double[] j12 = new double[points.Length];
            double[] j21 = new double[points.Length];
            double[] j22 = new double[points.Length];

            Parallel.For(0, points.Length, (i) =>
            {
                j11[i] = dNdCsi(i + 1, csi, eta) * points[i].X;
                j12[i] = dNdCsi(i + 1, csi, eta) * points[i].Y;
                j21[i] = dNdEta(i + 1, csi, eta) * points[i].X;
                j22[i] = dNdEta(i + 1, csi, eta) * points[i].Y;
            });

            Matrix<double> J = Matrix<double>.Build.Dense(2, 2);

            J[0, 0] = j11.Sum();
            J[0, 1] = j12.Sum();
            J[1, 0] = j21.Sum();
            J[1, 1] = j22.Sum();

            return J;
        }

        #endregion

        #region 3D

        /// <summary>
        /// Matrice jacobiana per cambiamento di variabile
        /// dN/dCsi = dx/dCsi * dN/dx + dy/dCsi * dN/dy + dz/dCsi * dN/dz
        /// dN/dEta = dx/dEta * dN/dx + dy/dEta * dN/dy + dz/dEta * dN/dz
        /// dN/dZEta = dx/dEta * dN/dx + dy/dEta * dN/dy + dz/dZeta * dN/dz
        /// => dN/dNatural = J * dN/dLocal;
        /// => dN/dLocal = J^-1 * dN/dNatural;
        /// => dF/dNatural = J^-1 dF/dLocal;
        /// </summary>
        /// <param name="csi">coordinata naturale</param>
        /// <param name="eta">coordinata naturale</param>
        /// <param name="zeta">coordinata naturale</param>
        /// <param name="dNdCsi">derivata funzioni di forma rispetto a Csi che descrive la GEOMETRIA (passaggio da coordinate locali a naturali) in funzione dell'indice di nodo e coordinate naturali</param>
        /// <param name="dNdEta">derivata funzioni di forma rispetto a Eta che descrive la GEOMETRIA (passaggio da coordinate locali a naturali) in funzione dell'indice di nodo e coordinate naturali</param>
        /// <param name="dNdZeta">derivata funzioni di forma rispetto a Eta che descrive la GEOMETRIA (passaggio da coordinate locali a naturali) in funzione dell'indice di nodo e coordinate naturali</param>
        /// <param name="points">The domain</param>
        /// <returns>
        /// dx/dCsi, dy/dCsi, dz/dCsi
        /// dy/dEta, dy/dEta, dz/dEta
        /// dz/dEta, dz/dEta, dz/dZeta
        /// </returns>
        private static Matrix<double> Jacob3D(double csi, double eta, double zeta, Func<int, double, double, double, double> dNdCsi,
            Func<int, double, double, double, double> dNdEta, Func<int, double, double, double, double> dNdZeta, Point3d[] points)
        {
            double j11 = 0.0;
            double j12 = 0.0;
            double j13 = 0.0;

            double j21 = 0.0;
            double j22 = 0.0;
            double j23 = 0.0;

            double j31 = 0.0;
            double j32 = 0.0;
            double j33 = 0.0;

            Parallel.For(0, points.Length, (i) =>
            {
                j11 += dNdCsi(i + 1, csi, eta, zeta) * points[i].X;
                j12 += dNdCsi(i + 1, csi, eta, zeta) * points[i].Y;
                j13 += dNdCsi(i + 1, csi, eta, zeta) * points[i].Z;

                j21 += dNdEta(i + 1, csi, eta, zeta) * points[i].X;
                j22 += dNdEta(i + 1, csi, eta, zeta) * points[i].Y;
                j23 += dNdEta(i + 1, csi, eta, zeta) * points[i].Z;

                j31 += dNdZeta(i + 1, csi, eta, zeta) * points[i].X;
                j32 += dNdZeta(i + 1, csi, eta, zeta) * points[i].Y;
                j33 += dNdZeta(i + 1, csi, eta, zeta) * points[i].Z;
            });

            Matrix<double> J = Matrix<double>.Build.Dense(3, 3);

            J[0, 0] = j11;
            J[0, 1] = j12;
            J[0, 2] = j13;

            J[1, 0] = j21;
            J[1, 1] = j22;
            J[1, 2] = j23;

            J[2, 0] = j31;
            J[2, 1] = j32;
            J[2, 2] = j33;

            return J;
        }

        /// <summary>
        /// Return J(x,y,z) = J(x,y,z, dNdCsi, dNdEta,dNdZeta, nodes) with "nodes" and derivative of shape function assigned
        /// arg1 = dFdInput1; arg2 = dFdInput2, arg3 = dFdInput3, arg4 = nodes
        /// </summary>
        private static Func<double, double, double, Matrix<double>> JacobianMatrix3D(Func<int, double, double, double, double> dNdCsi,
            Func<int, double, double, double, double> dNdEta, Func<int, double, double, double, double> dNdZeta, Point3d[] points)
        {
            return (double input1, double input2, double input3) => Jacob3D(input1, input2, input3, dNdCsi, dNdEta, dNdZeta, points);
        }

        #endregion

        #region GetXYZ

        private static Point3d TransformNaturalCoordToGlobalCoord(double csi, double eta, double zeta, Func<int, double, double, double, double> shapeFunction, Point3d[] vertices)
        {
            double[] valueX = new double[vertices.Length];
            double[] valueY = new double[vertices.Length];
            double[] valueZ = new double[vertices.Length];

            for (int i = 0; i < vertices.Length; i++)
            {
                valueX[i] = shapeFunction(i + 1, csi, eta, zeta) * vertices[i].X;
                valueY[i] = shapeFunction(i + 1, csi, eta, zeta) * vertices[i].Y;
                valueZ[i] = shapeFunction(i + 1, csi, eta, zeta) * vertices[i].Y;
            }

            return new Point3d(valueX.Sum(), valueY.Sum(), valueZ.Sum());
        }

        public static Point3d GetLocalCoordinate2D(double csi, double eta, Func<int, double, double, double> shapeFunction, Point3d[] points)
        {
            double valueX = 0;
            double valueY = 0;

            for (int i = 1; i <= points.Length; i++)
            {
                valueX += shapeFunction(i, csi, eta) * points[i - 1].X;
                valueY += shapeFunction(i, csi, eta) * points[i - 1].Y;
            }

            return new Point3d(valueX, valueY, 0);
        }

        public static Point3d GetLocalCoordinate1D(double csi, Func<int, double, double> shapeFunction, Line3d line)
		{
            double valueX = shapeFunction(0, csi) * line.Start.X + shapeFunction(1, csi) * line.End.X;
            double valueY = shapeFunction(0, csi) * line.Start.Y + shapeFunction(1, csi) * line.End.Y;

            return new Point3d(valueX, valueY, 0);
        }

        #endregion
    }
}
