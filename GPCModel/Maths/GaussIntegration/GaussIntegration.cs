using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Utilities.Fem;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.Maths.GaussIntegrations
{
    /// <summary>
    /// Class for integrate function with Gauss quadrature method
    /// </summary>
    public static class GaussIntegration
    {
        #region Line element

        /// <summary>
        /// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>
        /// </summary>
        /// <param name="function">The function (with variables x and y) to integrate</param>
        /// <param name="vertices">The vertices of the domain. Vertices must be 3</param>
        /// <param name="numberOfGaussPoints">The number of Gauss points</param>
        /// <param name="shapeFunction">The shape function for coordinate transformation</param>
        /// <param name="dNdCsi">The partial derivative of shape function respect the variable csi</param>
        /// <param name="numberOFShapeFunction">The number of shape function</param>
        /// <returns>The value of the integral</returns>
        public static double IntegrationLine(Func<double, double, double> function, Point3d[] vertices, LineGaussPoints.GaussPointNumber numberOfGaussPoints,
            Func<int, double, double> shapeFunction, Func<int, double, double> dNdCsi, int numberOFShapeFunction)
        {
            if (vertices.Length != 2)
                throw new ArgumentException("Points must be 2. Domain must be a triangle");

            GaussPoint[] gaussPoints;

            switch ((int)numberOfGaussPoints)
            {
                case 1:
                    gaussPoints = LineGaussPoints.Line1;
                    break;
                case 2:
                    gaussPoints = LineGaussPoints.Line2;
                    break;
                case 3:
                    gaussPoints = LineGaussPoints.Line3;
                    break;
                case 4:
                    gaussPoints = LineGaussPoints.Line4;
                    break;
                case 6:
                    gaussPoints = LineGaussPoints.Line6;
                    break;
                case 9:
                    gaussPoints = LineGaussPoints.Line9;
                    break;
                case 16:
                    gaussPoints = LineGaussPoints.Line16;
                    break;
                case 20:
                    gaussPoints = LineGaussPoints.Line20;
                    break;
                case 32:
                    gaussPoints = LineGaussPoints.Line32;
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
                int degree = numberOFShapeFunction / vertices.Length;
                Parallel.For(0, vertices.Length, (i) =>
                {
                    shapeFunctionNode[i] = vertices[i];
                });

                Parallel.For(0, numberOFShapeFunction - vertices.Length, (i) =>
                {
                    shapeFunctionNode[vertices.Length + i] = i * (vertices[i] + vertices[i + 1]) / degree;
                });
            }

            var jacobian = JacobianMatrix1D(dNdCsi, vertices);

            double[] ris = new double[gaussPoints.Length];

            Parallel.For(0, gaussPoints.Length, (i) =>
            {
                Point3d point = GaussIntegration.TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, shapeFunction, shapeFunctionNode);
                ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi).Determinant() * function(point.X, point.Y);
            });

            return ris.Sum();
        }

        /// <summary>
        /// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>
        /// </summary>
        /// <param name="function">The function (with variables x and y) to integrate</param>
        /// <param name="vertices">The vertices of the domain. Vertices must be 2</param>
        /// <param name="numberOfGaussPoints">The number of Gauss points</param>
        /// <returns>The value of the integral</returns>
        /// <remarks>Linear shape functions and its derivatives are used</remarks>
        public static double IntegrationLineLinearShapeFunction(Func<double, double, double> function, Point3d[] vertices, LineGaussPoints.GaussPointNumber numberOfGaussPoints)
        {
            return IntegrationLine(function, vertices, numberOfGaussPoints, LinearShapeFunctionsLine2.NaturalShapeFunction,
                LinearShapeFunctionsLine2.DNdCsi, 2);
        }

        /// <summary>
        /// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>
        /// </summary>
        /// <param name="function">The function (with variables x and y) to integrate</param>
        /// <param name="vertices">The vertices of the domain. Vertices must be 2</param>
        /// <param name="numberOfGaussPoints">The number of Gauss points</param>
        /// <returns>The value of the integral</returns>
        /// <remarks>Quadratic shape functions and its derivatives are used</remarks>
        public static double IntegrationLineQuadraticShapeFunction(Func<double, double, double> function, Point3d[] vertices, LineGaussPoints.GaussPointNumber numberOfGaussPoints)
        {
            return IntegrationLine(function, vertices, numberOfGaussPoints, QuadraticShapeFunctionLine3.NaturalShapeFunction,
                QuadraticShapeFunctionLine3.DNdCsi, 3);
        }

        #endregion

        #region Triangular element

        /// <summary>
        /// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>
        /// </summary>
        /// <param name="function">The function (with variables x and y) to integrate</param>
        /// <param name="vertices">The vertices of the domain. Vertices must be 3</param>
        /// <param name="numberOfGaussPoints">The number of Gauss points</param>
        /// <param name="shapeFunction">The shape function for coordinate transformation</param>
        /// <param name="dNdCsi">The partial derivative of shape function respect the variable csi</param>
        /// <param name="dNdEta">The partial derivative of shape function respect the variable eta</param>/param>
        /// <param name="numberOFShapeFunction">The number of shape function</param>
        /// <returns>The value of the integral</returns>
        public static double IntegrationTriangular(Func<double, double, double> function, Point3d[] vertices, TriangleGaussPoints.GaussPointNumber numberOfGaussPoints, Func<int, double, double, double> shapeFunction,
            Func<int, double, double, double> dNdCsi, Func<int, double, double, double> dNdEta, int numberOFShapeFunction)
        {
            if (vertices.Length != 3)
                throw new ArgumentException("Points must be 3. Domain must be a triangle");

            GaussPoint[] gaussPoints;

            switch ((int)numberOfGaussPoints)
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
                case 48:
                    gaussPoints = TriangleGaussPoints.Tri48;
                    break;
                case 61:
                    gaussPoints = TriangleGaussPoints.Tri61;
                    break;
                case 79:
                    gaussPoints = TriangleGaussPoints.Tri79;
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
                for (int i = 0; i < vertices.Length; i++)
                {
                    shapeFunctionNode[i] = vertices[i];
                }

                for (int i = 0; i < vertices.Length; i++)
                {
                    if (i != vertices.Length - 1)
                        shapeFunctionNode[vertices.Length + i] = (vertices[i] + vertices[i + 1]) / 2.0;
                    else
                        shapeFunctionNode[vertices.Length + i] = (vertices[i] + vertices[0]) / 2.0;
                }
            }

            var jacobian = JacobianMatrix2D(dNdCsi, dNdEta, shapeFunctionNode);

            double[] ris = new double[gaussPoints.Length];

            if (gaussPoints.Length >= 33)
            {
                Parallel.For(0, gaussPoints.Length, (i) =>
                {
                    Point3d point = GaussIntegration.TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, shapeFunctionNode);
                    ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(point.X, point.Y);
                });
            }
            else
            {
                for (int i = 0; i < gaussPoints.Length; i++)
                {
                    Point3d point = GaussIntegration.TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, shapeFunctionNode);
                    ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(point.X, point.Y);
                }
            }

            return ris.Sum() / 2.0;
        }

        /// <summary>
        /// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>
        /// </summary>
        /// <param name="function">The function (with variables x and y) to integrate</param>
        /// <param name="vertices">The vertices of the domain. Vertices must be 3</param>
        /// <param name="numberOfGaussPoints">The number of Gauss points</param>
        /// <returns>The value of the integral</returns>
        /// <remarks>Linear shape functions and its derivatives are used</remarks>
        public static double IntegrationTriangularLinearShapeFunction(Func<double, double, double> function, Point3d[] vertices, TriangleGaussPoints.GaussPointNumber numberOfGaussPoints)
        {
            return IntegrationTriangular(function, vertices, numberOfGaussPoints, LinearShapeFunctionsTri3.NaturalShapeFunction,
                LinearShapeFunctionsTri3.DNdCsi, LinearShapeFunctionsTri3.DNdEta, 3);
        }

        /// <summary>
        /// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>
        /// </summary>
        /// <param name="function">The function (with variables x and y) to integrate</param>
        /// <param name="vertices">The vertices of the domain. Vertices must be 3</param>
        /// <param name="numberOfGaussPoints">The number of Gauss points</param>
        /// <returns>The value of the integral</returns>
        /// <remarks>Quadratic shape functions and its derivatives are used</remarks>
        public static double IntegrationTriangularQuadraticShapeFunction(Func<double, double, double> function, Point3d[] vertices, TriangleGaussPoints.GaussPointNumber numberOfGaussPoints)
        {
            return IntegrationTriangular(function, vertices, numberOfGaussPoints, QuadraticShapeFunctionsTri6.NaturalShapeFunction,
                QuadraticShapeFunctionsTri6.DNdCsi, QuadraticShapeFunctionsTri6.DNdEta, 6);
        }

        #endregion

        #region Quadrangular element

        /// <summary>
        /// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>
        /// </summary>
        /// <param name="function">The function (with variables x and y) to integrate</param>
        /// <param name="vertices">The vertices of the domain. Vertices must be 4</param>
        /// <param name="numberOfGaussPoints">The number of Gauss points</param>
        /// <param name="shapeFunction">The shape function for coordinate transformation</param>
        /// <param name="dNdCsi">The partial derivative of shape function respect the variable csi</param>
        /// <param name="dNdEta">The partial derivative of shape function respect the variable eta</param>/param>
        /// <param name="numberOFShapeFunction">The number of shape function</param>
        /// <returns>The value of the integral</returns>
        public static double IntegrationQuadrilateral(Func<double, double, double> function, Point3d[] vertices, QuadrangleGaussPoints.GaussPointNumber numberOfGaussPoints, Func<int, double, double, double> shapeFunction,
            Func<int, double, double, double> dNdCsi, Func<int, double, double, double> dNdEta, int numberOFShapeFunction)
        {
            if (vertices.Length != 4)
                throw new ArgumentException("Points must be 4. Domain must be a quadrilateral");

            GaussPoint[] gaussPoints;

            switch ((int)numberOfGaussPoints)
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
                case 25:
                    gaussPoints = QuadrangleGaussPoints.Quad25;
                    break;
                case 49:
                    gaussPoints = QuadrangleGaussPoints.Quad49;
                    break;
                case 121:
                    gaussPoints = QuadrangleGaussPoints.Quad121;
                    break;
                case 400:
                    gaussPoints = QuadrangleGaussPoints.Quad400;
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
                for (int i = 0; i < vertices.Length; i++)
                {
                    shapeFunctionNode[i] = vertices[i];
                }

                for (int i = 0; i < vertices.Length; i++)
                {
                    if (i != vertices.Length - 1)
                        shapeFunctionNode[vertices.Length + i] = (vertices[i] + vertices[i + 1]) / 2.0;
                    else
                        shapeFunctionNode[vertices.Length + i] = (vertices[i] + vertices[0]) / 2.0;
                }
            }


            var jacobian = JacobianMatrix2D(dNdCsi, dNdEta, shapeFunctionNode);

            double[] ris = new double[gaussPoints.Length];

            if (gaussPoints.Length >= 49)
            {
                Parallel.For(0, gaussPoints.Length, (i) =>
                {
                    Point3d point = GaussIntegration.TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, shapeFunctionNode);
                    ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(point.X, point.Y);
                });
            }
            else
            {
                for (int i = 0; i < gaussPoints.Length; i++)
                {
                    Point3d point = GaussIntegration.TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, shapeFunctionNode);
                    ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(point.X, point.Y);
                }
            }

            return ris.Sum();
        }

        /// <summary>
        /// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>
        /// </summary>
        /// <param name="function">The function (with variables x and y) to integrate</param>
        /// <param name="vertices">The vertices of the domain. Vertices must be 4</param>
        /// <param name="numberOfGaussPoints">The number of Gauss points</param>
        /// <returns>The value of the integral</returns>
        /// <remarks>Linear shape functions and its derivatives are used</remarks>
        public static double IntegrationQuadrilateralLinearShapeFunction(Func<double, double, double> function, Point3d[] vertices, QuadrangleGaussPoints.GaussPointNumber numberOfGaussPoints)
        {
            return IntegrationQuadrilateral(function, vertices, numberOfGaussPoints, LinearShapeFunctionQuad4.NaturalShapeFunction,
                LinearShapeFunctionQuad4.DNdCsi, LinearShapeFunctionQuad4.DNdEta, 4);
        }

        /// <summary>
        /// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>
        /// </summary>
        /// <param name="function">The function (with variables x and y) to integrate</param>
        /// <param name="vertices">The vertices of the domain. Vertices must be 4</param>
        /// <param name="numberOfGaussPoints">The number of Gauss points</param>
        /// <returns>The value of the integral</returns>
        /// <remarks>Quadratic shape functions and its derivatives are used</remarks>
        public static double IntegrationQuadrilateralQuadraticShapeFunction(Func<double, double, double> function, Point3d[] vertices, QuadrangleGaussPoints.GaussPointNumber numberOfGaussPoints)
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
        public static double IntegrationHexaedron(Func<double, double, double, double> function, Point3d[] vertices, HexahedroGaussPoints.GaussPointNumber numberOfGaussPoints,
            Func<int, double, double, double, double> shapeFunction, Func<int, double, double, double, double> dNdCsi,
            Func<int, double, double, double, double> dNdEta, Func<int, double, double, double, double> dNdZeta, int numberOFShapeFunction)
        {
            if (vertices.Length != 8)
                throw new ArgumentException("Points must be 8. Polygon must be a Hexaedron");

            GaussPoint[] gaussPoints;

            switch ((int)numberOfGaussPoints)
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

        /// <summary>
        /// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>
        /// </summary>
        /// <param name="function">The function (with variables x and y) to integrate</param>
        /// <param name="vertices">The vertices of the domain. Vertices must be 8</param>
        /// <param name="numberOfGaussPoints">The number of Gauss points</param>
        /// <returns>The value of the integral</returns>
        /// <returns>The value of the integral</returns>
        /// <remarks>The vertices must be added with this order:
        /// Bottom, clockwise order. Top, clockwise order. The 1st must be associated with 5th, 2nd with 6th, 3rd with 7th and 4th with 8th.
        /// Linear shape functions and its derivative are used
        /// </remarks>
        public static double IntegrationHexaedronLinearShapeFunction(Func<double, double, double, double> function, Point3d[] vertices, HexahedroGaussPoints.GaussPointNumber numberOfGaussPoints)
        {
            return IntegrationHexaedron(function, vertices, numberOfGaussPoints, TriLinearShapeFunctionHexaedron8.NaturalShapeFunction,
                TriLinearShapeFunctionHexaedron8.DNdCsi, TriLinearShapeFunctionHexaedron8.DNdEta, TriLinearShapeFunctionHexaedron8.DNdZeta, 8);
        }

        #endregion

        #region 1D

        /// <summary>
        /// Jacobian matrix for change of variables 
        /// </summary>
        /// <param name="csi">>Nataral coordinate</param>
        /// <param name="dNdCsi">The partial derivative of shape function respect the variable</param>
        /// <param name="points">The domain of integration</param>
        /// <returns>Matrix</returns>
        private static Matrix<double> Jacob1D(double csi, Func<int, double, double> dNdCsi, Point3d[] points)
        {
            // Matrice jacobiana per cambiamento di variabile
            // dN/dCsi = dx/dCsi * dN/dx + dy/dCsi * dN/dy
            // dN/dEta = dx/dEta * dN/dx + dy/dEta * dN/dy
            // => dN/dNatural = J * dN/dLocal;
            // => dN/dLocal = J^-1 * dN/dNatural;
            // => dF/dNatural = J^-1 dF/dLocal;

            // dx/dCsi, dy/dCsi
            // dy/dEta, dy/dEta
            //
            // La matrice monodimensionale potrebbe essere considerata anche solo come il valore di j11
            // J11      0
            //  0       1

            double j11 = 0;

            for (int i = 0; i < points.Length; i++)
            {
                j11 += dNdCsi(i + 1, csi) * points[i].X;
            }

            return Matrix<double>.Build.Dense(2, 2, new[] { j11, 0, 0, 1 });
        }

        private static Func<double, Matrix<double>> JacobianMatrix1D(Func<int, double, double> dNdCsi, Point3d[] points)
        {
            // Return J(csi,eta) = J(csi,eta,dNdCsi, dNdEta,nodes) with "nodes" and derivative of shape function assigned        
            return (double csi) => Jacob1D(csi, dNdCsi, points);
        }

        #endregion

        #region 2D

        /// <summary>
        /// Return J(csi,eta) = J(csi,eta,dNdCsi, dNdEta,nodes) with "nodes" and derivative of shape function assigned
        /// </summary>
        /// <param name="dNdCsi">The partial derivative of shape function respect the variable csi</param>
        /// <param name="dNdEta">The partial derivative of shape function respect the variable eta</param>
        /// <param name="points">The domain of integration</param>
        /// <returns></returns>
        private static Func<double, double, Matrix<double>> JacobianMatrix2D(Func<int, double, double, double> dNdCsi,
            Func<int, double, double, double> dNdEta, Point3d[] points)
        {
            return (double csi, double eta) => Jacob2D(csi, eta, dNdCsi, dNdEta, points);
        }

        /// <summary>
        /// Jacobian matrix for change of variables 
        /// </summary>
        /// <param name="csi">First natural coordinate</param>
        /// <param name="eta">Second natural coordinate</param>
        /// <param name="dNdCsi">The partial derivative of shape function respect the variable csi</param>
        /// <param name="dNdEta">The partial derivative of shape function respect the variable eta</param>
        /// <param name="points">The domain of integration</param>
        /// <returns>Matrix</returns>
        private static Matrix<double> Jacob2D(double csi, double eta, Func<int, double, double, double> dNdCsi,
            Func<int, double, double, double> dNdEta, Point3d[] points)
        {
            // Matrice jacobiana per cambiamento di variabile
            // dN/dCsi = dx/dCsi * dN/dx + dy/dCsi * dN/dy
            // dN/dEta = dx/dEta * dN/dx + dy/dEta * dN/dy
            // => dN/dNatural = J * dN/dLocal;
            // => dN/dLocal = J^-1 * dN/dNatural;
            // => dF/dNatural = J^-1 dF/dLocal;

            // dx/dCsi, dy/dCsi
            // dy/dEta, dy/dEta

            double j11 = 0;
            double j12 = 0;
            double j21 = 0;
            double j22 = 0;

            for (int i = 0; i < points.Length; i++)
            {
                j11 += dNdCsi(i + 1, csi, eta) * points[i].X;
                j12 += dNdCsi(i + 1, csi, eta) * points[i].Y;
                j21 += dNdEta(i + 1, csi, eta) * points[i].X;
                j22 += dNdEta(i + 1, csi, eta) * points[i].Y;

            }

            return Matrix<double>.Build.Dense(2, 2, new[] { j11, j12, j21, j22 });
        }

        #endregion

        #region 3D

        /// <summary>
        /// Jacobian matrix for change of variables 
        /// </summary>
        /// <param name="csi">First natural coordinate</param>
        /// <param name="eta">Second natural coordinate</param>
        /// <param name="zeta">Third natural coordinate</param>
        /// <param name="dNdCsi">The partial derivative of shape function respect the variable csi</param>
        /// <param name="dNdEta">The partial derivative of shape function respect the variable eta</param>
        /// <param name="dNdZeta">The partial derivative of shape function respect the variable zeta</param>
        /// <param name="points">The domain of integration</param>
        /// <returns>Matrix</returns>
        private static Matrix<double> Jacob3D(double csi, double eta, double zeta, Func<int, double, double, double, double> dNdCsi,
            Func<int, double, double, double, double> dNdEta, Func<int, double, double, double, double> dNdZeta, Point3d[] points)
        {

            // Matrice jacobiana per cambiamento di variabile
            // dN/dCsi = dx/dCsi * dN/dx + dy/dCsi * dN/dy + dz/dCsi * dN/dz
            // dN/dEta = dx/dEta * dN/dx + dy/dEta * dN/dy + dz/dEta * dN/dz
            // dN/dZEta = dx/dEta * dN/dx + dy/dEta * dN/dy + dz/dZeta * dN/dz
            // => dN/dNatural = J * dN/dLocal;
            // => dN/dLocal = J^-1 * dN/dNatural;
            // => dF/dNatural = J^-1 dF/dLocal;

            // dx/dCsi, dy/dCsi, dz/dCsi
            // dy/dEta, dy/dEta, dz/dEta
            // dz/dEta, dz/dEta, dz/dZeta

            double j11 = 0.0;
            double j12 = 0.0;
            double j13 = 0.0;

            double j21 = 0.0;
            double j22 = 0.0;
            double j23 = 0.0;

            double j31 = 0.0;
            double j32 = 0.0;
            double j33 = 0.0;

            for (int i = 0; i < points.Length; i++)
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
            }

            return Matrix<double>.Build.Dense(3, 3, new[] { j11, j12, j13, j21, j22, j23, j31, j32, j33 });
        }

        /// <summary>
        /// Return J(x,y,z) = J(x,y,z, dNdCsi, dNdEta,dNdZeta, nodes) with "nodes" and derivative of shape function assigned
        /// </summary>
        /// <param name="dNdCsi">The partial derivative of shape function respect the variable csi</param>
        /// <param name="dNdEta">The partial derivative of shape function respect the variable eta</param>
        /// <param name="dNdZeta">The partial derivative of shape function respect the variable zeta</param>
        /// <param name="points">The domain of integration</param>
        /// <returns></returns>
        private static Func<double, double, double, Matrix<double>> JacobianMatrix3D(Func<int, double, double, double, double> dNdCsi,
            Func<int, double, double, double, double> dNdEta, Func<int, double, double, double, double> dNdZeta, Point3d[] points)
        {
            return (double input1, double input2, double input3) => Jacob3D(input1, input2, input3, dNdCsi, dNdEta, dNdZeta, points);
        }

        #endregion

        #region GetXYZ

        /// <summary>
        /// Transform natural coordinate <paramref name="csi"/>, <paramref name="eta"/>, <paramref name="zeta"/> into a 3D point in global coordinate
        /// </summary>
        /// <param name="csi">First natural coordinate</param>
        /// <param name="eta">Second natural coordinate</param>
        /// <param name="zeta">Third natural coordinate</param>
        /// <param name="shapeFunction">Shape functions</param>
        /// <param name="vertices">The domain</param>
        /// <returns>Point3d</returns>
        private static Point3d TransformNaturalCoordToGlobalCoord(double csi, double eta, double zeta, Func<int, double, double, double, double> shapeFunction, Point3d[] vertices)
        {
            double valueX = 0;
            double valueY = 0;
            double valueZ = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                valueX += shapeFunction(i + 1, csi, eta, zeta) * vertices[i].X;
                valueY += shapeFunction(i + 1, csi, eta, zeta) * vertices[i].Y;
                valueZ += shapeFunction(i + 1, csi, eta, zeta) * vertices[i].Z;
            }

            return new Point3d(valueX, valueY, valueZ);
        }

        /// <summary>
        /// Transform natural coordinate <paramref name="csi"/>, <paramref name="eta"/> into a 3D point in global coordinate
        /// </summary>
        /// <param name="csi">First natural coordinate</param>
        /// <param name="eta">Second natural coordinate</param>
        /// <param name="shapeFunction">Shape functions</param>
        /// <param name="vertices">The domain</param>
        /// <returns>Point3d</returns>
        private static Point3d TransformNaturalCoordToGlobalCoord(double csi, double eta, Func<int, double, double, double> shapeFunction, Point3d[] vertices)
        {
            double valueX = 0;
            double valueY = 0;
            for (int i = 0; i < vertices.Length; i++)
            {
                valueX += shapeFunction(i + 1, csi, eta) * vertices[i].X;
                valueY += shapeFunction(i + 1, csi, eta) * vertices[i].Y;
            }

            return new Point3d(valueX, valueY, 0);
        }

        /// <summary>
        /// Transform natural coordinate <paramref name="csi"/> into a 3D point in global coordinate
        /// </summary>
        /// <param name="csi">First natural coordinate</param>
        /// <param name="shapeFunction">Shape functions</param>
        /// <param name="vertices">The domain</param>
        /// <returns>Point3d</returns>
        private static Point3d TransformNaturalCoordToGlobalCoord(double csi, Func<int, double, double> shapeFunction, Point3d[] vertices)
        {
            double valueX = shapeFunction(1, csi) * vertices[0].X + shapeFunction(2, csi) * vertices[1].X;
            double valueY = shapeFunction(1, csi) * vertices[0].Y + shapeFunction(2, csi) * vertices[1].Y;

            return new Point3d(valueX, valueY, 0);
        }

        #endregion
    }
}
