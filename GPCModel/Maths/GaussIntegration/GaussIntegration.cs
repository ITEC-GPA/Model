using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.Sections.Steel;
using GPC.Utilities.Fem;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPC.Model.Maths.GaussIntegrations
{
	/// <summary>
	/// Class for integrate function with Gauss quadrature method
	/// </summary>
	public static class GaussIntegration
	{
		#region Line element

		/// <summary>
		/// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>.
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
				throw new ArgumentException("Points must be 2. Domain must be a Line");

			GaussPoint[] gaussPoints;

			if (LineGaussPoints.GaussPointNumberAssociation.ContainsKey(numberOfGaussPoints))
			{
				gaussPoints = LineGaussPoints.GaussPointNumberAssociation[numberOfGaussPoints];
			}
			else
				throw new ArgumentException("Wrong number of Gauss Points");

			Point3d[] shapeFunctionNode = new Point3d[numberOFShapeFunction];

			if (numberOFShapeFunction == vertices.Length)
			{
				shapeFunctionNode = vertices;
			}
			else
			{
				int degree = numberOFShapeFunction / vertices.Length;
				for (int i = 0; i < vertices.Length; i++)
				{
					shapeFunctionNode[i] = vertices[i];
				};

				for (int i = 0; i < numberOFShapeFunction - vertices.Length; i++)
				{
					shapeFunctionNode[vertices.Length + i] = (vertices[i] + vertices[i + 1]) / (degree + 1);
				};
			}

			var jacobian = JacobianMatrix1D(dNdCsi, shapeFunctionNode);

			double[] ris = new double[gaussPoints.Length];

			Parallel.For(0, gaussPoints.Length, (i) =>
			{
				var point = GaussIntegration.TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, shapeFunction, shapeFunctionNode);
				ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi).Determinant() * function(point.Item1, point.Item2);
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
		/// Calculate the integral of function <paramref name="function"/> on the domain <paramref name="vertices"/>.
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

			bool parallelComputing = false;

			if (TriangleGaussPoints.GaussPointNumberAssociation.ContainsKey(numberOfGaussPoints))
			{
				gaussPoints = TriangleGaussPoints.GaussPointNumberAssociation[numberOfGaussPoints];
				if ((int)numberOfGaussPoints >= 33)
					parallelComputing = true;
			}
			else
				throw new ArgumentException("Wrong number of Quad Gauss Points");

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


			double res = 0;
			if (parallelComputing)
			{
				double[] ris = new double[gaussPoints.Length];

				Parallel.For(0, gaussPoints.Length, (i) =>
				{
					TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, shapeFunctionNode, out double x, out double y);
					ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(x, y);
				});

				res = ris.Sum() / 2.0;
			}
			else
			{
				for (int i = 0; i < gaussPoints.Length; i++)
				{
					TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, shapeFunctionNode, out double x, out double y);
					res += gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(x, y);
				}

				res /= 2.0;
			}

			return res;
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
			bool parallelComputing = false;

			if (QuadrangleGaussPoints.GaussPointNumberAssociation.ContainsKey(numberOfGaussPoints))
			{
				gaussPoints = QuadrangleGaussPoints.GaussPointNumberAssociation[numberOfGaussPoints];
				if ((int)numberOfGaussPoints >= 49)
					parallelComputing = true;
			}
			else
				throw new ArgumentException("Wrong number of Quad Gauss Points");

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

			double res = 0;
			if (parallelComputing)
			{
				double[] ris = new double[gaussPoints.Length];

				Parallel.For(0, gaussPoints.Length, (i) =>
				{
					TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, shapeFunctionNode, out double x, out double y);
					ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(x, y);
				});

				res = ris.Sum();

			}
			else
			{
				for (int i = 0; i < gaussPoints.Length; i++)
				{
					TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, gaussPoints[i].Eta, shapeFunction, shapeFunctionNode, out double x, out double y);
					res += gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta).Determinant() * function(x, y);
				}
			}

			return res;
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
			bool parallelComputing = false;

			if (HexahedroGaussPoints.GaussPointNumberAssociation.ContainsKey(numberOfGaussPoints))
			{
				gaussPoints = HexahedroGaussPoints.GaussPointNumberAssociation[numberOfGaussPoints];
				if ((int)numberOfGaussPoints >= 27)
					parallelComputing = true;
			}
			else
				throw new ArgumentException("Wrong number of Quad Gauss Points");

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

			if (parallelComputing)
			{
				Parallel.For(0, gaussPoints.Length, (i) =>
				{
					var point = GaussIntegration.TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, gaussPoints[i].Eta, gaussPoints[i].Zeta, shapeFunction, shapeFunctionNode);
					ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta, gaussPoints[i].Zeta).Determinant() * function(point.Item1, point.Item2, point.Item3);
				});
			}
			else
			{
				for (int i = 0; i < gaussPoints.Length; i++)
				{
					var point = GaussIntegration.TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, gaussPoints[i].Eta, gaussPoints[i].Zeta, shapeFunction, shapeFunctionNode);
					ris[i] = gaussPoints[i].Weight * jacobian(gaussPoints[i].Csi, gaussPoints[i].Eta, gaussPoints[i].Zeta).Determinant() * function(point.Item1, point.Item2, point.Item3);
				};
			}

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

		#region Mesh

		/// <summary>
		/// Calculate the integral of function <paramref name="function"/> arrays over the <paramref name="mesh"/> domain
		/// </summary>
		/// <param name="function">The function (with variables x and y) to integrate</param>
		/// <param name="mesh"></param>
		/// <param name="quadNumberOfGaussPoints">The number of Gauss points for quad face</param>
		/// <param name="triNumberOfGaussPoints">The number of Gauss points for tri face</param>
		/// <returns>The value of the integral</returns>
		/// <remarks>Linear shape functions and its derivatives are used</remarks>
		public static T[] IntegrationLinearShapeFunction<T>(Func<double, double, T>[] function, Mesh mesh,
			QuadrangleGaussPoints.GaussPointNumber quadNumberOfGaussPoints, TriangleGaussPoints.GaussPointNumber triNumberOfGaussPoints)
		{
			return IntegrationLinearShapeFunction(function, GetGlobalCoordinateGaussPointsLinearShapeFunction(mesh, quadNumberOfGaussPoints, triNumberOfGaussPoints));
		}

		/// <summary>
		/// Get the mesh gauss points in global coordinate system associated with relative multiplicative factor
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="quadNumberOfGaussPoints">The number of Gauss points for quad face</param>
		/// <param name="triNumberOfGaussPoints">The number of Gauss points for tri face</param>
		/// <returns>The value of the integral</returns>
		/// <remarks>Linear shape functions and its derivatives are used</remarks>
		public static GlobalCoordinateGaussPoint[][] GetGlobalCoordinateGaussPointsLinearShapeFunction(Mesh mesh,
			QuadrangleGaussPoints.GaussPointNumber quadNumberOfGaussPoints, TriangleGaussPoints.GaussPointNumber triNumberOfGaussPoints)
		{
			Func<int, double, double, double> shapeFunctionQuad = LinearShapeFunctionQuad4.NaturalShapeFunction;
			Func<int, double, double, double> dNdCsiQuad = LinearShapeFunctionQuad4.DNdCsi;
			Func<int, double, double, double> dNdEtaQuad = LinearShapeFunctionQuad4.DNdEta;

			Func<int, double, double, double> shapeFunctionTri = LinearShapeFunctionsTri3.NaturalShapeFunction;
			Func<int, double, double, double> dNdCsiTri = LinearShapeFunctionsTri3.DNdCsi;
			Func<int, double, double, double> dNdEtaTri = LinearShapeFunctionsTri3.DNdEta;

			GaussPoint[] gaussPointsQuad;
			GaussPoint[] gaussPointsTri;

			bool parallelComputing = false;

			if (QuadrangleGaussPoints.GaussPointNumberAssociation.ContainsKey(quadNumberOfGaussPoints))
			{
				gaussPointsQuad = QuadrangleGaussPoints.GaussPointNumberAssociation[quadNumberOfGaussPoints];
				if ((int)quadNumberOfGaussPoints >= 49)
					parallelComputing = true;
			}
			else
				throw new ArgumentException("Wrong number of Quad Gauss Points");

			if (TriangleGaussPoints.GaussPointNumberAssociation.ContainsKey(triNumberOfGaussPoints))
			{
				gaussPointsTri = TriangleGaussPoints.GaussPointNumberAssociation[triNumberOfGaussPoints];
				if ((int)triNumberOfGaussPoints >= 49)
					parallelComputing = true;
			}
			else
				throw new ArgumentException("Wrong number of Tri Gauss Points");


			int faceCount = mesh.FacesCount;
			IEnumerator<MeshFace> facesEnumerator = mesh.GetFacesEnumerator();

			GlobalCoordinateGaussPoint[][] globalGaussPoints = new GlobalCoordinateGaussPoint[faceCount][];
			int index = 0;

			while (facesEnumerator.MoveNext())
			{
				Point3d[] shapeFunctionNode = mesh.GetFacePoints(facesEnumerator.Current);

				if (facesEnumerator.Current.IsQuad)
				{
					globalGaussPoints[index] = new GlobalCoordinateGaussPoint[gaussPointsQuad.Length];
					Func<double, double, Matrix<double>> jacobian = JacobianMatrix2D(dNdCsiQuad, dNdEtaQuad, shapeFunctionNode);

					if (parallelComputing)
					{
						Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, gaussPointsQuad.Length), (range) =>
						{
							double x = 0;
							double y = 0;
							for (int i = range.Item1; i < range.Item2; i++)
							{
								TransformNaturalCoordToGlobalCoord(gaussPointsQuad[i].Csi, gaussPointsQuad[i].Eta, shapeFunctionQuad, shapeFunctionNode, out x, out y);
								globalGaussPoints[index][i] = new GlobalCoordinateGaussPoint(x, y, 0, jacobian(gaussPointsQuad[i].Csi, gaussPointsQuad[i].Eta).Determinant(), gaussPointsQuad[i].Weight, 1.0);
							}
						});
					}
					else
					{
						double x = 0;
						double y = 0;
						for (int i = 0; i < gaussPointsQuad.Length; i++)
						{
							TransformNaturalCoordToGlobalCoord(gaussPointsQuad[i].Csi, gaussPointsQuad[i].Eta, shapeFunctionQuad, shapeFunctionNode, out x, out y);
							globalGaussPoints[index][i] = new GlobalCoordinateGaussPoint(x, y, 0, jacobian(gaussPointsQuad[i].Csi, gaussPointsQuad[i].Eta).Determinant(), gaussPointsQuad[i].Weight, 1.0);
						}
					}
				}
				else
				{
					globalGaussPoints[index] = new GlobalCoordinateGaussPoint[gaussPointsTri.Length];
					Func<double, double, Matrix<double>> jacobian = JacobianMatrix2D(dNdCsiTri, dNdEtaTri, shapeFunctionNode);

					if (parallelComputing)
					{
						Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, gaussPointsTri.Length), (range) =>
						{
							double x = 0;
							double y = 0;
							for (int i = range.Item1; i < range.Item2; i++)
							{
								TransformNaturalCoordToGlobalCoord(gaussPointsTri[i].Csi, gaussPointsTri[i].Eta, shapeFunctionTri, shapeFunctionNode, out x, out y);
								globalGaussPoints[index][i] = new GlobalCoordinateGaussPoint(x, y, 0, jacobian(gaussPointsTri[i].Csi, gaussPointsTri[i].Eta).Determinant(), gaussPointsTri[i].Weight, 0.5);
							}
						});
					}
					else
					{
						double x = 0;
						double y = 0;
						for (int i = 0; i < gaussPointsTri.Length; i++)
						{
							TransformNaturalCoordToGlobalCoord(gaussPointsTri[i].Csi, gaussPointsTri[i].Eta, shapeFunctionTri, shapeFunctionNode, out x, out y);
							globalGaussPoints[index][i] = new GlobalCoordinateGaussPoint(x, y, 0, jacobian(gaussPointsTri[i].Csi, gaussPointsTri[i].Eta).Determinant(), gaussPointsTri[i].Weight, 0.5);
						}
					}
				}

				index++;
			}

			return globalGaussPoints;
		}

		/// <summary>
		/// Calculate the integral of function <paramref name="function"/> arrays over the <paramref name="mesh"/> domain
		/// </summary>
		/// <param name="function">The function (with variables x and y) to integrate</param>
		/// <param name="mesh"></param>
		/// <param name="hexahedroNumberOfGaussPoints">The number of Gauss points for hexahedro volume</param>
		/// <returns>The value of the integral</returns>
		/// <remarks>Linear shape functions and its derivatives are used</remarks>
		public static T[] IntegrationLinearShapeFunction<T>(Func<double, double, T>[] function, Mesh mesh, HexahedroGaussPoints.GaussPointNumber hexahedroNumberOfGaussPoints)
		{
			return IntegrationLinearShapeFunction(function, GetGlobalCoordinateGaussPointsLinearShapeFunction(mesh, hexahedroNumberOfGaussPoints));
		}

		/// <summary>
		/// Get the mesh gauss points in global coordinate system associated with relative multiplicative factor
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="hexahedroNumberOfGaussPoints">The number of Gauss points for volume element</param>
		/// <returns>The value of the integral</returns>
		/// <remarks>Linear shape functions and its derivatives are used</remarks>
		public static GlobalCoordinateGaussPoint[][] GetGlobalCoordinateGaussPointsLinearShapeFunction(Mesh mesh,
			HexahedroGaussPoints.GaussPointNumber hexahedroNumberOfGaussPoints)
		{
			Func<int, double, double, double, double> shapeFunctionQuad = TriLinearShapeFunctionHexaedron8.NaturalShapeFunction;
			Func<int, double, double, double, double> dNdCsiQuad = TriLinearShapeFunctionHexaedron8.DNdCsi;
			Func<int, double, double, double, double> dNdEtaQuad = TriLinearShapeFunctionHexaedron8.DNdEta;
			Func<int, double, double, double, double> dNdZetaQuad = TriLinearShapeFunctionHexaedron8.DNdZeta;

			GaussPoint[] gaussPointsQuad;

			bool parallelComputing = false;

			if (HexahedroGaussPoints.GaussPointNumberAssociation.ContainsKey(hexahedroNumberOfGaussPoints))
			{
				gaussPointsQuad = HexahedroGaussPoints.GaussPointNumberAssociation[hexahedroNumberOfGaussPoints];
				if ((int)hexahedroNumberOfGaussPoints >= 27)
					parallelComputing = true;
			}
			else
				throw new ArgumentException("Wrong number of Quad Gauss Points");


			int volumeCount = mesh.VolumesCount;
			IEnumerator<MeshVolume> facesEnumerator = mesh.GetVolumesEnumerator();

			GlobalCoordinateGaussPoint[][] globalGaussPoints = new GlobalCoordinateGaussPoint[volumeCount][];
			int index = 0;

			while (facesEnumerator.MoveNext())
			{
				MeshVolume meshVolume = facesEnumerator.Current;
				Point3d[] shapeFunctionNode = mesh.GetVolumePoints(meshVolume);

				if (facesEnumerator.Current.IsQuadrangular)
				{
					globalGaussPoints[index] = new GlobalCoordinateGaussPoint[gaussPointsQuad.Length];
					Func<double, double, double, Matrix<double>> jacobian = JacobianMatrix3D(dNdCsiQuad, dNdEtaQuad, dNdZetaQuad, shapeFunctionNode);

					if (parallelComputing)
					{
						Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, gaussPointsQuad.Length), (range) =>
						{
							double x = 0;
							double y = 0;
							for (int i = range.Item1; i < range.Item2; i++)
							{
								TransformNaturalCoordToGlobalCoord(gaussPointsQuad[i].Csi, gaussPointsQuad[i].Eta, gaussPointsQuad[i].Zeta, shapeFunctionQuad, shapeFunctionNode, out x, out y, out double z);
								globalGaussPoints[index][i] = new GlobalCoordinateGaussPoint(x, y, z, jacobian(gaussPointsQuad[i].Csi, gaussPointsQuad[i].Eta, gaussPointsQuad[i].Zeta).Determinant(), gaussPointsQuad[i].Weight, 1.0);
							}
						});
					}
					else
					{
						double x = 0;
						double y = 0;
						for (int i = 0; i < gaussPointsQuad.Length; i++)
						{
							TransformNaturalCoordToGlobalCoord(gaussPointsQuad[i].Csi, gaussPointsQuad[i].Eta, gaussPointsQuad[i].Zeta, shapeFunctionQuad, shapeFunctionNode, out x, out y, out double z);
							globalGaussPoints[index][i] = new GlobalCoordinateGaussPoint(x, y, z, jacobian(gaussPointsQuad[i].Csi, gaussPointsQuad[i].Eta, gaussPointsQuad[i].Zeta).Determinant(), gaussPointsQuad[i].Weight, 1.0);
						}
					}
				}
				else
				{

				}

				index++;
			}

			return globalGaussPoints;
		}

		#endregion

		#region ThinWall Section

		/// <summary>
		/// Calculate the integral of function <paramref name="function"/> arrays over the <paramref name="thinWallSection"/> domain
		/// </summary>
		/// <param name="function">The function (with variables x and y) to integrate</param>
		/// <param name="thinWallSection"></param>
		/// <param name="numberOfGaussPoints">The number of Gauss points</param>
		/// <returns>The value of the integral</returns>
		/// <remarks>Linear shape functions and its derivatives are used</remarks>
		public static T[] IntegrationLinearShapeFunction<T>(Func<double, double, T>[] function, SteelSectionPosition steelSectionPosition, LineGaussPoints.GaussPointNumber numberOfGaussPoints)
		{
			return IntegrationLinearShapeFunction(function, GetGlobalCoordinateGaussPointsLinearShapeFunction(steelSectionPosition, numberOfGaussPoints));
		}

		/// <summary>
		/// Get the gauss points in global coordinate system associated with relative multiplicative factor
		/// </summary>
		/// <param name="steelSectionPosition"></param>
		/// <param name="numberOfGaussPoints">The number of Gauss points for line</param>
		public static GlobalCoordinateGaussPoint[][] GetGlobalCoordinateGaussPointsLinearShapeFunction(SteelSectionPosition steelSectionPosition, LineGaussPoints.GaussPointNumber numberOfGaussPoints)
		{
			GaussPoint[] gaussPoints;
			Func<int, double, double> shapeFunction = LinearShapeFunctionsLine2.NaturalShapeFunction;
			Func<int, double, double> dNdCsi = LinearShapeFunctionsLine2.DNdCsi;

			bool parallelComputing = false;

			if (LineGaussPoints.GaussPointNumberAssociation.ContainsKey(numberOfGaussPoints))
			{
				gaussPoints = LineGaussPoints.GaussPointNumberAssociation[numberOfGaussPoints];
				if ((int)numberOfGaussPoints >= 20)
					parallelComputing = true;
			}
			else
				throw new ArgumentException("Wrong number of Gauss Points");


			int thinWallCount = steelSectionPosition.Section.ThinWalls.Length;

			GlobalCoordinateGaussPoint[][] globalGaussPoints = new GlobalCoordinateGaussPoint[thinWallCount][];

			for (int index = 0; index < thinWallCount; index++)
			{
				Point2d[] line = steelSectionPosition.Section.ThinWalls[index].GetMiddleLine();
				Point3d[] shapeFunctionNode = line.Select(n => new Point3d(steelSectionPosition.PositionToGlobal(n))).ToArray();

				globalGaussPoints[index] = new GlobalCoordinateGaussPoint[gaussPoints.Length];
				Func<double, Matrix<double>> jacobian = JacobianMatrix1D(dNdCsi, shapeFunctionNode);

				if (parallelComputing)
				{
					Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, gaussPoints.Length), (range) =>
					{
						double x = 0;
						double y = 0;
						for (int i = range.Item1; i < range.Item2; i++)
						{
							TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, shapeFunction, shapeFunctionNode, out x, out y);
							globalGaussPoints[index][i] = new GlobalCoordinateGaussPoint(x, y, 0, jacobian(gaussPoints[i].Csi).Determinant(), gaussPoints[i].Weight, 1.0);
						}
					});
				}
				else
				{
					double x = 0;
					double y = 0;
					for (int i = 0; i < gaussPoints.Length; i++)
					{
						TransformNaturalCoordToGlobalCoord(gaussPoints[i].Csi, shapeFunction, shapeFunctionNode, out x, out y);
						globalGaussPoints[index][i] = new GlobalCoordinateGaussPoint(x, y, 0, jacobian(gaussPoints[i].Csi).Determinant(), gaussPoints[i].Weight, 1.0);
					}
				}
			}

			return globalGaussPoints;
		}

		#endregion

		#region GlobalCoordinateGaussPoint Integration

		public static double[] IntegrationLinearShapeFunction(Func<double, double, double>[] function, GlobalCoordinateGaussPoint[][][] globalGaussPoints)
		{
			double[] res = new double[function.Length];

			Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, function.Length), (range) =>
			{
				for (int f = range.Item1; f < range.Item2; f++)
				{
					res[f] = IntegrationLinearShapeFunction(function[f], globalGaussPoints);
				}
			});

			return res;
		}

		public static T[] IntegrationLinearShapeFunction<T>(Func<double, double, T>[] function, GlobalCoordinateGaussPoint[][][] globalGaussPoints)
		{
			T[] res = new T[function.Length];

			Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, function.Length), (range) =>
			{
				for (int f = range.Item1; f < range.Item2; f++)
				{
					res[f] = IntegrationLinearShapeFunction(function[f], globalGaussPoints);
				}
			});

			return res;
		}

		public static double IntegrationLinearShapeFunction(Func<double, double, double> function, GlobalCoordinateGaussPoint[][][] globalGaussPoints, bool parallelComputing = false)
		{
			if (parallelComputing)
			{
				List<double> results = new List<double>();

				Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, globalGaussPoints.Length), (range) =>
				{
					double res = 0;
					for (int g = range.Item1; g < range.Item2; g++)
					{
						for (int j = 0; j < globalGaussPoints[g].Length; j++)
						{
							for (int k = 0; k < globalGaussPoints[g][j].Length; k++)
							{
								res += globalGaussPoints[g][j][k].EvaluateFunction(function);
							}
						}
					}

					results.Add(res);
				});

				return results.Sum();
			}
			else
			{
				double results = 0;
				for (int g = 0; g < globalGaussPoints.Length; g++)
				{
					for (int j = 0; j < globalGaussPoints[g].Length; j++)
					{
						for (int k = 0; k < globalGaussPoints[g][j].Length; k++)
						{
							results += globalGaussPoints[g][j][k].EvaluateFunction(function);
						}
					}
				}


				return results;
			}
		}

		public static T IntegrationLinearShapeFunction<T>(Func<double, double, T> function, GlobalCoordinateGaussPoint[][][] globalGaussPoints, bool parallelComputing = false)
		{
			if (parallelComputing)
			{
				T[][][] results = new T[globalGaussPoints.Length][][];

				Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, globalGaussPoints.Length), (range) =>
				{
					for (int g = range.Item1; g < range.Item2; g++)
					{
						results[g] = new T[globalGaussPoints[g].Length][];
						for (int j = 0; j < globalGaussPoints[g].Length; j++)
						{
							results[g][j] = new T[globalGaussPoints[g][j].Length];
							for (int k = 0; k < globalGaussPoints[g][j].Length; k++)
							{
								results[g][j][k] = globalGaussPoints[g][j][k].EvaluateFunction(function);
							}
						}
					}
				});

				return GlobalCoordinateGaussPoint.MassSum<T>(results);
			}
			else
			{
				T[][][] results = new T[globalGaussPoints.Length][][];
				for (int g = 0; g < globalGaussPoints.Length; g++)
				{
					results[g] = new T[globalGaussPoints[g].Length][];
					for (int j = 0; j < globalGaussPoints[g].Length; j++)
					{
						results[g][j] = new T[globalGaussPoints[g][j].Length];
						for (int k = 0; k < globalGaussPoints[g][j].Length; k++)
						{
							results[g][j][k] = globalGaussPoints[g][j][k].EvaluateFunction(function);
						}
					}
				}

				return GlobalCoordinateGaussPoint.MassSum<T>(results);
			}
		}

		public static double[] IntegrationLinearShapeFunction(Func<double, double, double>[] function, GlobalCoordinateGaussPoint[][] globalGaussPoints)
		{
			double[] res = new double[function.Length];

			Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, function.Length), (range) =>
			{
				for (int f = range.Item1; f < range.Item2; f++)
				{
					res[f] = IntegrationLinearShapeFunction(function[f], globalGaussPoints);
				}
			});

			return res;
		}

		public static T[] IntegrationLinearShapeFunction<T>(Func<double, double, T>[] function, GlobalCoordinateGaussPoint[][] globalGaussPoints)
		{
			T[] res = new T[function.Length];

			Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, function.Length), (range) =>
			{
				for (int f = range.Item1; f < range.Item2; f++)
				{
					res[f] = IntegrationLinearShapeFunction(function[f], globalGaussPoints);
				}
			});

			return res;
		}

		public static double IntegrationLinearShapeFunction(Func<double, double, double> function, GlobalCoordinateGaussPoint[][] globalGaussPoints, bool parallelComputing = false)
		{
			if (parallelComputing)
			{
				List<double> results = new List<double>();

				Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, globalGaussPoints.Length), (range) =>
				{
					double res = 0;
					for (int g = range.Item1; g < range.Item2; g++)
					{
						for (int j = 0; j < globalGaussPoints[g].Length; j++)
						{
							res += globalGaussPoints[g][j].EvaluateFunction(function);
						}
					}

					results.Add(res);
				});

				return results.Sum();
			}
			else
			{
				double results = 0;
				for (int g = 0; g < globalGaussPoints.Length; g++)
				{
					for (int j = 0; j < globalGaussPoints[g].Length; j++)
					{
						results += globalGaussPoints[g][j].EvaluateFunction(function);
					}
				}


				return results;
			}
		}

		public static T IntegrationLinearShapeFunction<T>(Func<double, double, T> function, GlobalCoordinateGaussPoint[][] globalGaussPoints, bool parallelComputing = false)
		{
			if (parallelComputing)
			{
				T[][] results = new T[globalGaussPoints.Length][];

				Parallel.ForEach(System.Collections.Concurrent.Partitioner.Create(0, globalGaussPoints.Length), (range) =>
				{
					for (int g = range.Item1; g < range.Item2; g++)
					{
						results[g] = new T[globalGaussPoints[g].Length];
						for (int j = 0; j < globalGaussPoints[g].Length; j++)
						{
							results[g][j] = globalGaussPoints[g][j].EvaluateFunction(function);
						}
					}
				});

				return GlobalCoordinateGaussPoint.MassSum<T>(results);
			}
			else
			{
				T[][] results = new T[globalGaussPoints.Length][];
				for (int g = 0; g < globalGaussPoints.Length; g++)
				{
					results[g] = new T[globalGaussPoints[g].Length];
					for (int j = 0; j < globalGaussPoints[g].Length; j++)
					{
						results[g][j] = globalGaussPoints[g][j].EvaluateFunction(function);
					}
				}

				return GlobalCoordinateGaussPoint.MassSum<T>(results);
			}
		}

		public static double[] IntegrationLinearShapeFunction(Func<double, double, double>[] function, GlobalCoordinateGaussPoint[] globalGaussPoints)
		{
			double[] res = new double[function.Length];

			Parallel.For(0, function.Length, (f) =>
			{
				res[f] = IntegrationLinearShapeFunction(function[f], globalGaussPoints);
			});

			return res;
		}

		public static T[] IntegrationLinearShapeFunction<T>(Func<double, double, T>[] function, GlobalCoordinateGaussPoint[] globalGaussPoints)
		{
			T[] res = new T[function.Length];

			Parallel.For(0, function.Length, (f) =>
			{
				res[f] = IntegrationLinearShapeFunction(function[f], globalGaussPoints);
			});

			return res;
		}

		public static double IntegrationLinearShapeFunction(Func<double, double, double> function, GlobalCoordinateGaussPoint[] globalGaussPoints, bool parallelComputing = false)
		{
			if (parallelComputing)
			{
				var results = new double[globalGaussPoints.Length];

				Parallel.For(0, globalGaussPoints.Length, f =>
				{
					results[f] = globalGaussPoints[f].EvaluateFunction(function);
				});

				return results.Sum();
			}
			else
			{
				double results = 0;
				for (int g = 0; g < globalGaussPoints.Length; g++)
				{
					results += globalGaussPoints[g].EvaluateFunction(function);
				}

				return results;
			}
		}

		public static T IntegrationLinearShapeFunction<T>(Func<double, double, T> function, GlobalCoordinateGaussPoint[] globalGaussPoints, bool parallelComputing = false)
		{
			if (parallelComputing)
			{
				T[] results = new T[globalGaussPoints.Length];

				Parallel.For(0, globalGaussPoints.Length, (g) =>
				{
					results[g] = globalGaussPoints[g].EvaluateFunction(function);
				});

				return GlobalCoordinateGaussPoint.MassSum<T>(results);
			}
			else
			{
				T[] results = new T[globalGaussPoints.Length];
				for (int g = 0; g < globalGaussPoints.Length; g++)
				{
					results[g] = globalGaussPoints[g].EvaluateFunction(function);
				}

				return GlobalCoordinateGaussPoint.MassSum<T>(results);
			}
		}

		#endregion

		#region Private Utility Methods

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
			//double L = 0.5 * (points[1] - points[0]).DistanceTo(Point3d.Origin);

			// dx/dCsi, dy/dCsi
			// dy/dEta, dy/dEta
			//
			// La matrice monodimensionale potrebbe essere considerata anche solo come il valore di j11
			// J11      0
			//  0       1

			double j11 = 0;

			for (int i = 0; i < points.Length; i++)
			{
				j11 += dNdCsi(i + 1, csi) * points[i].DistanceTo(points[0]);
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
		private static (double, double, double) TransformNaturalCoordToGlobalCoord(double csi, double eta, double zeta, Func<int, double, double, double, double> shapeFunction, Point3d[] vertices)
		{
			TransformNaturalCoordToGlobalCoord(csi, eta, zeta, shapeFunction, vertices, out double valueX, out double valueY, out double valueZ);
			return (valueX, valueY, valueZ);
		}

		/// <summary>
		/// Transform natural coordinate <paramref name="csi"/>, <paramref name="eta"/> into a 3D point in global coordinate
		/// </summary>
		/// <param name="csi">First natural coordinate</param>
		/// <param name="eta">Second natural coordinate</param>
		/// <param name="shapeFunction">Shape functions</param>
		/// <param name="vertices">The domain</param>
		/// <returns>Point3d</returns>
		private static (double, double, double) TransformNaturalCoordToGlobalCoord(double csi, double eta, Func<int, double, double, double> shapeFunction, Point3d[] vertices)
		{
			TransformNaturalCoordToGlobalCoord(csi, eta, shapeFunction, vertices, out double valueX, out double valueY);
			return (valueX, valueY, 0);
		}

		/// <summary>
		/// Transform natural coordinate <paramref name="csi"/> into a 3D point in global coordinate
		/// </summary>
		/// <param name="csi">First natural coordinate</param>
		/// <param name="shapeFunction">Shape functions</param>
		/// <param name="vertices">The domain</param>
		/// <returns>Point3d</returns>
		private static (double, double, double) TransformNaturalCoordToGlobalCoord(double csi, Func<int, double, double> shapeFunction, Point3d[] vertices)
		{
			TransformNaturalCoordToGlobalCoord(csi, shapeFunction, vertices, out double valueX, out double valueY);
			return (valueX, valueY, 0);
		}

		/// <summary>
		/// Transform natural coordinate <paramref name="csi"/>, <paramref name="eta"/>, <paramref name="zeta"/> into a 3D point in global coordinate
		/// </summary>
		/// <param name="csi">First natural coordinate</param>
		/// <param name="eta">Second natural coordinate</param>
		/// <param name="zeta">Third natural coordinate</param>
		/// <param name="shapeFunction">Shape functions</param>
		/// <param name="vertices">The domain</param>
		/// <param name="valueX">X global coordinate</param>
		/// <param name="valueY">Y global coordinate</param>
		/// <param name="valueZ">Z global coordinate</param>
		/// <returns>Point3d</returns>
		private static void TransformNaturalCoordToGlobalCoord(double csi, double eta, double zeta, Func<int, double, double, double, double> shapeFunction, Point3d[] vertices,
			out double valueX, out double valueY, out double valueZ)
		{
			valueX = 0;
			valueY = 0;
			valueZ = 0;
			for (int i = 0; i < vertices.Length; i++)
			{
				valueX += shapeFunction(i + 1, csi, eta, zeta) * vertices[i].X;
				valueY += shapeFunction(i + 1, csi, eta, zeta) * vertices[i].Y;
				valueZ += shapeFunction(i + 1, csi, eta, zeta) * vertices[i].Z;
			}
		}

		/// <summary>
		/// Transform natural coordinate <paramref name="csi"/>, <paramref name="eta"/> into a 3D point in global coordinate
		/// </summary>
		/// <param name="csi">First natural coordinate</param>
		/// <param name="eta">Second natural coordinate</param>
		/// <param name="shapeFunction">Shape functions</param>
		/// <param name="vertices">The domain</param>
		/// <param name="valueX">X global coordinate</param>
		/// <param name="valueY">Y global coordinate</param>
		/// <returns>Point3d</returns>
		private static void TransformNaturalCoordToGlobalCoord(double csi, double eta, Func<int, double, double, double> shapeFunction, Point3d[] vertices, out double valueX, out double valueY)
		{
			double locvalueX = 0;
			double locvalueY = 0;
			for (int i = 0; i < vertices.Length; i++)
			{
				locvalueX += shapeFunction(i + 1, csi, eta) * vertices[i].X;
				locvalueY += shapeFunction(i + 1, csi, eta) * vertices[i].Y;
			}

			valueX = locvalueX;
			valueY = locvalueY;
		}

		/// <summary>
		/// Transform natural coordinate <paramref name="csi"/> into a 2D point in global coordinate
		/// </summary>
		/// <param name="csi">First natural coordinate</param>
		/// <param name="shapeFunction">Shape functions</param>
		/// <param name="vertices">The domain</param>
		/// <param name="valueX">X global coordinate</param>
		/// <param name="valueY">Y global coordinate</param>
		/// <returns>Point3d</returns>
		private static void TransformNaturalCoordToGlobalCoord(double csi, Func<int, double, double> shapeFunction, Point3d[] vertices, out double valueX, out double valueY)
		{
			valueX = 0;
			valueY = 0;
			for (int i = 0; i < vertices.Length; i++)
			{
				valueX += shapeFunction(i + 1, csi) * vertices[i].X;
				valueY += shapeFunction(i + 1, csi) * vertices[i].Y;
			}
		}

		#endregion

		#endregion

		#region Nested Class GlobalCoordinateGaussPoint

		public struct GlobalCoordinateGaussPoint
		{
			/// <summary>
			/// X coordinate of Gauss in global coordinate system
			/// </summary>
			public double GpX { get; }

			/// <summary>
			/// Y coordinate of Gauss in global coordinate system
			/// </summary>
			public double GpY { get; }

			/// <summary>
			/// Z coordinate of Gauss in global coordinate system
			/// </summary>
			public double GpZ { get; }

			private readonly double _determinantWeightFactorMultiplication;

			private static IGaussPointCalculator _calculator;

			private bool _calculatorInstantiated;


			/// <param name="gpX">X coordinate of Gauss in global coordinate system</param>
			/// <param name="gpY">Y coordinate of Gauss in global coordinate system</param>
			/// <param name="gpZ">Z coordinate of Gauss in global coordinate system</param>
			/// <param name="jacobianDeterminant">The determinant of the Jacobian matrix of trasformation</param>
			/// <param name="gaussPointWeight">The Gauss point weight</param>
			/// <param name="factor">Multiplicative factor for function evaluation</param>
			public GlobalCoordinateGaussPoint(double gpX, double gpY, double gpZ, double jacobianDeterminant, double gaussPointWeight, double factor = 1.0)
			{
				GpX = gpX;
				GpY = gpY;
				GpZ = gpZ;

				_determinantWeightFactorMultiplication = jacobianDeterminant * gaussPointWeight * factor;

				_calculatorInstantiated = false;
			}

			/// <exception cref="KeyNotFoundException"></exception>
			public T EvaluateFunction<T>(Func<double, T> function)
			{
				if (!_calculatorInstantiated)
				{
					_calculator = GaussPointCalculator.GetInstance<T>();
					_calculatorInstantiated = true;
				}

				return ((ICalculator<T>)_calculator).Multiply(_determinantWeightFactorMultiplication, function(GpX));
			}

			public double EvaluateFunction(Func<double, double, double> function)
			{
				return _determinantWeightFactorMultiplication * function(GpX, GpY);
			}

			/// <exception cref="KeyNotFoundException"></exception>
			public T EvaluateFunction<T>(Func<double, double, T> function)
			{
				if (!_calculatorInstantiated)
				{
					_calculator = GaussPointCalculator.GetInstance<T>();
					_calculatorInstantiated = true;
				}

				return ((ICalculator<T>)_calculator).Multiply(_determinantWeightFactorMultiplication, function(GpX, GpY));
			}

			/// <exception cref="KeyNotFoundException"></exception>
			public T EvaluateFunction<T>(Func<double, double, double, T> function)
			{
				if (!_calculatorInstantiated)
				{
					_calculator = GaussPointCalculator.GetInstance<T>();
					_calculatorInstantiated = true;
				}

				return ((ICalculator<T>)_calculator).Multiply(_determinantWeightFactorMultiplication, function(GpX, GpY, GpZ));
			}

			/// <exception cref="KeyNotFoundException"></exception>
			public static T MassSum<T>(T[][][] values)
			{
				_calculator = GaussPointCalculator.GetInstance<T>();

				return ((ICalculator<T>)_calculator).MassSum(values);
			}

			/// <exception cref="KeyNotFoundException"></exception>
			public static T MassSum<T>(T[][] values)
			{
				_calculator = GaussPointCalculator.GetInstance<T>();

				return ((ICalculator<T>)_calculator).MassSum(values);
			}

			/// <exception cref="KeyNotFoundException"></exception>
			public static T MassSum<T>(T[] values)
			{
				_calculator = GaussPointCalculator.GetInstance<T>();

				return ((ICalculator<T>)_calculator).MassSum(values);
			}
		}

		protected static class GaussPointCalculator
		{
			public static readonly Dictionary<Type, IGaussPointCalculator> Calculators = new Dictionary<Type, IGaussPointCalculator>()
			{
				{ typeof(double), new DoubleCalculator() },
				{ typeof(Tuple<double>), new TupleOneDoubleCalculator() },
				{ typeof(Tuple<double, double>), new TupleTwoDoubleCalculator() },
				{ typeof((double, double)), new ValueTupleTwoDoubleCalculator () },
				{ typeof(Tuple<double, double, double>), new TupleThreeDoubleCalculator() },
				{ typeof((double, double, double)), new ValueTupleThreeDoubleCalculator() }
			};

			public static ICalculator<T> GetInstance<T>()
			{
				return (ICalculator<T>)Calculators[typeof(T)];
			}
		}

		protected interface ICalculator<T> : IGaussPointCalculator
		{
			T Multiply(double constants, T function);
			T MassSum(T[][][] value);
			T MassSum(T[][] value);
			T MassSum(T[] value);
		}

		protected interface IGaussPointCalculator
		{

		}

		protected class DoubleCalculator : ICalculator<double>
		{
			public double Multiply(double constants, double function) { return constants * function; }
			public double MassSum(double[][][] value)
			{
				return value.Select(i => i.Select(j => j.Sum()).Sum()).Sum();
			}
			public double MassSum(double[][] value)
			{
				return value.Select(i => i.Sum()).Sum();
			}
			public double MassSum(double[] value)
			{
				return value.Sum();
			}
		}

		protected class TupleOneDoubleCalculator : ICalculator<Tuple<double>>
		{
			public Tuple<double> Multiply(double constants, Tuple<double> function)
			{
				return new Tuple<double>(constants * function.Item1);
			}

			public Tuple<double> MassSum(Tuple<double>[][][] value)
			{
				return new Tuple<double>(value.Select(i => i.Select(j => j.Select(k => k.Item1).Sum()).Sum()).Sum());
			}

			public Tuple<double> MassSum(Tuple<double>[][] value)
			{
				return new Tuple<double>(value.Select(i => i.Select(j => j.Item1).Sum()).Sum());
			}

			public Tuple<double> MassSum(Tuple<double>[] value)
			{
				return new Tuple<double>(value.Select(i => i.Item1).Sum());
			}
		}

		protected class TupleTwoDoubleCalculator : ICalculator<Tuple<double, double>>
		{
			public Tuple<double, double> Multiply(double constants, Tuple<double, double> function)
			{
				return new Tuple<double, double>(constants * function.Item1, constants * function.Item2);
			}

			public Tuple<double, double> MassSum(Tuple<double, double>[][][] value)
			{
				double res1 = 0;
				double res2 = 0;
				for (int i = 0; i < value.Length; i++)
				{
					for (int j = 0; j < value[i].Length; j++)
					{
						for (int k = 0; k < value[i][j].Length; k++)
						{
							res1 += value[i][j][k].Item1;
							res2 += value[i][j][k].Item2;
						}
					}
				}

				return new Tuple<double, double>(res1, res2);
			}

			public Tuple<double, double> MassSum(Tuple<double, double>[][] value)
			{
				double res1 = 0;
				double res2 = 0;
				for (int i = 0; i < value.Length; i++)
				{
					for (int j = 0; j < value[i].Length; j++)
					{
						res1 += value[i][j].Item1;
						res2 += value[i][j].Item2;
					}
				}

				return new Tuple<double, double>(res1, res2);
			}

			public Tuple<double, double> MassSum(Tuple<double, double>[] value)
			{
				double res1 = 0;
				double res2 = 0;
				for (int i = 0; i < value.Length; i++)
				{
					res1 += value[i].Item1;
					res2 += value[i].Item2;
				}

				return new Tuple<double, double>(res1, res2);
			}
		}

		protected class ValueTupleTwoDoubleCalculator : ICalculator<(double, double)>
		{
			public (double, double) Multiply(double constants, (double, double) function)
			{
				return (constants * function.Item1, constants * function.Item2);
			}

			public (double, double) MassSum((double, double)[][][] value)
			{
				double res1 = 0;
				double res2 = 0;
				for (int i = 0; i < value.Length; i++)
				{
					for (int j = 0; j < value[i].Length; j++)
					{
						for (int k = 0; k < value[i][j].Length; k++)
						{
							res1 += value[i][j][k].Item1;
							res2 += value[i][j][k].Item2;
						}
					}
				}

				return (res1, res2);
			}

			public (double, double) MassSum((double, double)[][] value)
			{
				double res1 = 0;
				double res2 = 0;
				for (int i = 0; i < value.Length; i++)
				{
					for (int j = 0; j < value[i].Length; j++)
					{
						res1 += value[i][j].Item1;
						res2 += value[i][j].Item2;
					}
				}

				return (res1, res2);
			}

			public (double, double) MassSum((double, double)[] value)
			{
				double res1 = 0;
				double res2 = 0;
				for (int i = 0; i < value.Length; i++)
				{
					res1 += value[i].Item1;
					res2 += value[i].Item2;
				}

				return (res1, res2);
			}
		}

		protected class TupleThreeDoubleCalculator : ICalculator<Tuple<double, double, double>>
		{
			public Tuple<double, double, double> Multiply(double constants, Tuple<double, double, double> function)
			{
				return new Tuple<double, double, double>(constants * function.Item1, constants * function.Item2, constants * function.Item3);
			}

			public Tuple<double, double, double> MassSum(Tuple<double, double, double>[][][] value)
			{
				double res1 = 0;
				double res2 = 0;
				double res3 = 0;

				for (int i = 0; i < value.Length; i++)
				{
					for (int j = 0; j < value[i].Length; j++)
					{
						for (int k = 0; k < value[i][j].Length; k++)
						{
							res1 += value[i][j][k].Item1;
							res2 += value[i][j][k].Item2;
							res3 += value[i][j][k].Item3;
						}
					}
				}

				return new Tuple<double, double, double>(res1, res2, res3);
			}

			public Tuple<double, double, double> MassSum(Tuple<double, double, double>[][] value)
			{
				double res1 = 0;
				double res2 = 0;
				double res3 = 0;

				for (int i = 0; i < value.Length; i++)
				{
					for (int j = 0; j < value[i].Length; j++)
					{
						res1 += value[i][j].Item1;
						res2 += value[i][j].Item2;
						res3 += value[i][j].Item3;
					}
				}

				return new Tuple<double, double, double>(res1, res2, res3);
			}

			public Tuple<double, double, double> MassSum(Tuple<double, double, double>[] value)
			{
				double res1 = 0;
				double res2 = 0;
				double res3 = 0;

				for (int i = 0; i < value.Length; i++)
				{
					res1 += value[i].Item1;
					res2 += value[i].Item2;
					res3 += value[i].Item3;
				}

				return new Tuple<double, double, double>(res1, res2, res3);
			}
		}

		protected class ValueTupleThreeDoubleCalculator : ICalculator<(double, double, double)>
		{
			public (double, double, double) Multiply(double constants, (double, double, double) function)
			{
				return (constants * function.Item1, constants * function.Item2, constants * function.Item3);
			}

			public (double, double, double) MassSum((double, double, double)[][][] value)
			{
				double res1 = 0;
				double res2 = 0;
				double res3 = 0;

				for (int i = 0; i < value.Length; i++)
				{
					for (int j = 0; j < value[i].Length; j++)
					{
						for (int k = 0; k < value[i][j].Length; k++)
						{
							res1 += value[i][j][k].Item1;
							res2 += value[i][j][k].Item2;
							res3 += value[i][j][k].Item3;
						}
					}
				}

				return (res1, res2, res3);
			}

			public (double, double, double) MassSum((double, double, double)[][] value)
			{
				double res1 = 0;
				double res2 = 0;
				double res3 = 0;

				for (int i = 0; i < value.Length; i++)
				{
					for (int j = 0; j < value[i].Length; j++)
					{
						res1 += value[i][j].Item1;
						res2 += value[i][j].Item2;
						res3 += value[i][j].Item3;
					}
				}

				return (res1, res2, res3);
			}

			public (double, double, double) MassSum((double, double, double)[] value)
			{
				double res1 = 0;
				double res2 = 0;
				double res3 = 0;

				for (int i = 0; i < value.Length; i++)
				{
					res1 += value[i].Item1;
					res2 += value[i].Item2;
					res3 += value[i].Item3;
				}

				return (res1, res2, res3);
			}
		}

		#endregion
	}
}
