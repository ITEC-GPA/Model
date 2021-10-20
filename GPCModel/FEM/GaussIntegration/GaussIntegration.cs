using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using MathNet.Numerics.LinearAlgebra;
using GPC.Utilities.Fem;

namespace GPC.Model.FEM.GaussIntegration
{
	public static class GaussIntegration
	{
		#region Triangular element

		public static double IntegrationTriangular(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints, Func<int, double, double, double> shapeFunction, 
            Func<int, double, double, double> derivRespectCsi, Func<int, double, double, double> derivRespectEta)
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
                case 32:
                    gaussPoints = TriangleGaussPoints.Tri33;
                    break;

                default:
                    throw new ArgumentException("Wrong number of Gauss Points");
            }

            var jacobian = JacobianMatrix2D(derivRespectCsi, derivRespectEta, vertices);

            double[] ris = new double[gaussPoints.Length];

            Parallel.For(0, gaussPoints.Length, (i) =>
            {
                Point3d point = GaussIntegration.GetLocalCoordinate2D(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, vertices);
                ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(point.X, point.Y);
            });
                
            return ris.Sum() / 2.0;
        }

        public static double IntegrationTriangularLinearShapeFunction(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints)
		{
            return IntegrationTriangular(function, vertices, numberOfGaussPoints, LinearShapeFunctionsTri3.NaturalShapeFunction, 
                LinearShapeFunctionsTri3.DNdCsi, LinearShapeFunctionsTri3.DNdEta);
		}

        public static double IntegrationTriangularQuadraticShapeFunction(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints)
        {
            return IntegrationTriangular(function, vertices, numberOfGaussPoints, QuadraticShapeFunctionsTri6.NaturalShapeFunction, 
                QuadraticShapeFunctionsTri6.DNdCsi, QuadraticShapeFunctionsTri6.DNdEta);
        }

		#endregion

		#region Quadrangular element

		public static double IntegrationQuadrilateral(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints, Func<int, double, double, double> shapeFunction,
            Func<int, double, double, double> derivRespectCsi, Func<int, double, double, double> derivRespectEta)
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

            var jacobian = JacobianMatrix2D(derivRespectCsi, derivRespectEta, vertices);

            double[] ris = new double[gaussPoints.Length];

            Parallel.For(0, gaussPoints.Length, (i) =>
            {
                Point3d point = GaussIntegration.GetLocalCoordinate2D(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, vertices);
                ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(point.X, point.Y);
            });

            return ris.Sum();
        }

        public static double IntegrationQuadrilateralLinearShapeFunction(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints)
        {
            return IntegrationQuadrilateral(function, vertices, numberOfGaussPoints, LinearShapeFunctionQuad4.NaturalShapeFunction,
                LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta);
        }

        public static double IntegrationQuadrilateralQuadraticShapeFunction(Func<double, double, double> function, Point3d[] vertices, int numberOfGaussPoints)
        {
            return IntegrationQuadrilateral(function, vertices, numberOfGaussPoints, QuadraticShapeFunctionQuad8.NaturalShapeFunction,
                QuadraticShapeFunctionQuad8.DNdCsi, QuadraticShapeFunctionQuad8.DNdEta);
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
        public static Func<double, double, Matrix<double>> JacobianMatrix2D(Func<int, double, double, double> dFdInput1, Func<int, double, double, double> dFdInput2, Point3d[] points)
        {
            return (double csi, double eta) => Jacob2D(csi, eta, dFdInput1, dFdInput2, points);
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
        public static Matrix<double> Jacob2D(double csi, double eta, Func<int, double, double, double> dNdCsi, Func<int, double, double, double> dNdEta, Point3d[] points)
        {
            double[] j11 = new double[points.Length];
            double[] j12 = new double[points.Length];
            double[] j21 = new double[points.Length];
            double[] j22 = new double[points.Length];


            Parallel.For(0, points.Length, (node) =>
            {
                int i = node + 1;
                double xi = points[node].X;
                double yi = points[node].Y;

                j11[node] = dNdCsi(i, csi, eta) * xi;
                j12[node] = dNdCsi(i, csi, eta) * yi;
                j21[node] = dNdEta(i, csi, eta) * xi;
                j22[node] = dNdEta(i, csi, eta) * yi;
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
        public static Matrix<double> Jacob3D(double csi, double eta, double zeta, Func<int, double, double, double, double> dNdCsi, 
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

            Parallel.For(0, points.Length, (node) =>
            {
                int i = node + 1;

                j11 += dNdCsi(i, csi, eta, zeta) * points[node].X;
                j12 += dNdCsi(i, csi, eta, zeta) * points[node].Y;
                j13 += dNdCsi(i, csi, eta, zeta) * points[node].Z;

                j21 += dNdEta(i, csi, eta, zeta) * points[node].X;
                j22 += dNdEta(i, csi, eta, zeta) * points[node].Y;
                j23 += dNdEta(i, csi, eta, zeta) * points[node].Z;

                j31 += dNdZeta(i, csi, eta, zeta) * points[node].X;
                j32 += dNdZeta(i, csi, eta, zeta) * points[node].Y;
                j33 += dNdZeta(i, csi, eta, zeta) * points[node].Z;
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
        public static Func<double, double, double, Matrix<double>> JacobianMatrix3D(Func<int, double, double, double, double> dFdInput1, 
            Func<int, double, double, double, double> dFdInput2, Func<int, double, double, double, double> dFdInput3, Point3d[] points)
        {
            return (double input1, double input2, double input3) => Jacob3D(input1, input2, input3, dFdInput1, dFdInput2, dFdInput3, points);
        }

        #endregion

        #region GetXYZ

        public static double GetLocalCoordinate3D(string direction, double csi, double eta, double zeta, Func<int, double, double, double, double> shapeFunction, Polygon3d poly)
        {
            double val = 0;

            for (int i = 1; i <= poly.Count; i++)
            {
                double factor;
                switch (direction.ToUpper())
                {
                    case "X":
                        factor = poly[i - 1].X;
                        break;
                    case "Y":
                        factor = poly[i - 1].Y;
                        break;
                    case "Z":
                        factor = poly[i - 1].Z;
                        break;
                    default:
                        throw new IndexOutOfRangeException("direction can be X, Y or Z");
                }
                val += shapeFunction(i, csi, eta, zeta) * factor;
            }
            return val;
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
