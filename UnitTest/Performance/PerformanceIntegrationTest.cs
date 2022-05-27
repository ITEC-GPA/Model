using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Maths.GaussIntegrations;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.TestUtilities;
using GPC.Utilities.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace PerformanceTest
{
	[TestClass]
	public class PerformanceIntegrationTest : UnitTestBase
	{
		protected void CommonEqualAssert(double result, double expectedValue, double tolerance = 0.01)
		{
			if (expectedValue == 0)
			{
				Assert.IsTrue(Math.Abs(result - expectedValue) < tolerance, $"1) calculated value: {result}, expValue: {expectedValue}");
			}
			else
			{
				if (result != 0)
				{
					Assert.IsTrue(Math.Abs((result - expectedValue) / result) < tolerance, $"1) calculated value: {result}, expValue: {expectedValue}");
				}
				else
				{
					Assert.IsTrue(Math.Abs(result - expectedValue) < tolerance, $"1) calculated value: {result}, expValue: {expectedValue}");
				}
			}
		}

		protected ReinforcedConcreteSection GetCircularSection(double diameter = 300, double subdivision = 32, double rebarDiameter = 18, double concreteCover = 50,
			int numberOfRebars = 16, ConcreteMaterial concreteMaterial = null, SteelMaterial rebarMaterial = null)
		{
			if (concreteMaterial == null)
				concreteMaterial = ConcreteMaterialEN1992.C25_30;

			if (rebarMaterial == null)
				rebarMaterial = SteelMaterial.B450C;

			Shape2d shape = new Shape2d(new Polygon2d(diameter, subdivision));
			ShapeEx shapeEx = new ShapeEx(shape, concreteMaterial);

			RebarSectionCircular rebar = new RebarSectionCircular(rebarDiameter, rebarMaterial);
			ReinforcedConcreteRebar[] rebars = new ReinforcedConcreteRebar[numberOfRebars];

			var rebarPerimeter = new Polygon2d(diameter - concreteCover * 2, numberOfRebars);
			for (int j = 0; j < rebarPerimeter.Count; j++)
			{
				rebars[j] = new ReinforcedConcreteRebar(rebar, rebarPerimeter[j]);
			}

			ReinforcedConcreteSection section = new ReinforcedConcreteSection(shapeEx);
			section.AddRebars(rebars);

			return section;
		}

		protected ReinforcedConcreteSection GetCircularSection(double diameter = 300, double subdivision = 32, ConcreteMaterial concreteMaterial = null)
		{
			if (concreteMaterial == null)
				concreteMaterial = ConcreteMaterialEN1992.C25_30;

			Shape2d shape = new Shape2d(new Polygon2d(diameter, subdivision));
			ShapeEx shapeEx = new ShapeEx(shape, concreteMaterial);
			ReinforcedConcreteSection section = new ReinforcedConcreteSection(shapeEx);

			return section;
		}


		[TestMethod]
		public void IntegrationTest1()
		{

			double constant = 3.0;

			Point3d[] polygon = new Point3d[] { new Point2d(-1.0, -1.0), new Point2d(+1.0, -1.0), new Point2d(+1.0, +1.0), new Point2d(-1.0, +1.0) };

			Func<double, double, double> func = (x, y) => constant * x * x;


			double result = 0;
			Action ac2 = new Action(() =>
			{
				result = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, polygon, QuadrangleGaussPoints.GaussPointNumber.Quad400);
			});

			var bb0 = MeasureTime.FunctionExecutionTime(20, ac2, true); ;

			Console.WriteLine(bb0);

		}

		[TestMethod]
		public void IntegrationQuad4Test1LSF()
		{
			int numberOfExecutons = 1000;

			Point3d[] vertices = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(4.0, 10, 0), new Point3d(5.0, 10.0, 0.0) };
			Func<double, double, double> func = (double x, double y) => x * x + y / 2 + x;

			Action actionP79 = new Action(() =>
			{
				GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad400);
			});

			Action actionP61 = new Action(() =>
			{
				GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad121);
			});

			Action actionP48 = new Action(() =>
			{
				GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad49);
			});

			Action actionP33 = new Action(() =>
			{
				GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad25);
			});

			Action actionP12 = new Action(() =>
			{
				GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad12);
			});

			Action actionP6 = new Action(() =>
			{
				GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, vertices, QuadrangleGaussPoints.GaussPointNumber.Quad8);
			});

			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP79, true, "400 Gauss Points");
			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP61, true, "121 Gauss Points");
			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP48, true, "49 Gauss Points");
			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP33, true, "25 Gauss Points");
			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP12, true, "12 Gauss Points");
			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP6, true, "8 Gauss Points");
		}

		[TestMethod]
		public void IntegrationTri3Test1LSF()
		{
			int numberOfExecutons = 1000;

			Point3d[] vertices = new Point3d[] { new Point3d(5.0, 5.0, 0), new Point3d(10, 7.0, 0), new Point3d(4.0, 10, 0) };
			Func<double, double, double> func = (double x, double y) => x * x + y / 2 + x;

			Action actionP79 = new Action(() =>
			{
				GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri79);
			});

			Action actionP61 = new Action(() =>
			{
				GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri61);
			});

			Action actionP48 = new Action(() =>
			{
				GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri48);
			});

			Action actionP33 = new Action(() =>
			{
				GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri33);
			});

			Action actionP12 = new Action(() =>
			{
				GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri12);
			});

			Action actionP6 = new Action(() =>
			{
				GaussIntegration.IntegrationTriangularLinearShapeFunction(func, vertices, TriangleGaussPoints.GaussPointNumber.Tri6);
			});

			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP79, true, "79 Gauss Points");
			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP61, true, "61 Gauss Points");
			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP48, true, "48 Gauss Points");
			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP33, true, "33 Gauss Points");
			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP12, true, "12 Gauss Points");
			MeasureTime.FunctionExecutionTime(numberOfExecutons, actionP6, true, "6 Gauss Points");
		}

		[TestMethod]
		public void IntegrationQuadFunction()
		{

			Point3d[] vertices = new Point3d[] { new Point3d(5, 51, 0), new Point3d(18, 27, 0), new Point3d(34, 13, 0), new Point3d(46, 18, 0) };
			Polygon3d poly = new Polygon3d(vertices);


			Func<double, double, double> func = (double x, double y) => Math.Pow(Math.Sin(x), 12) + Math.Pow(Math.Cos(y), 6) + 1;
			double expectedValue = 628;

			double value = GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly.ToArray(), QuadrangleGaussPoints.GaussPointNumber.Quad400);

			Console.WriteLine(value);
			CommonEqualAssert(value, expectedValue);


			Action action = new Action(() =>
			{
				GaussIntegration.IntegrationQuadrilateralLinearShapeFunction(func, poly.ToArray(), QuadrangleGaussPoints.GaussPointNumber.Quad400);
			});


			MeasureTime.FunctionExecutionTime(100, action, true);
		}

		[TestMethod]
		public void IntegrateCircular_1()
		{
			ReinforcedConcreteSection section = GetCircularSection(300, 32, ConcreteMaterialEN1992.C25_30);

			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();

			Func<double, double, double> func = new Func<double, double, double>((x, y) => { return x * y; });

			Func<double, double, double>[] arrayFunc = new Func<double, double, double>[16 * 49];
			arrayFunc = arrayFunc.Select(i => func).ToArray();

			double[] results = GaussIntegration.IntegrationLinearShapeFunction(arrayFunc, section.Mesh, QuadrangleGaussPoints.GaussPointNumber.Quad400, TriangleGaussPoints.GaussPointNumber.Tri79);

			watch.Stop();
			Console.WriteLine($"Parallel + parallel: {watch.ElapsedMilliseconds}");

			Assert.IsTrue(results[0] != 0);
			Assert.AreEqual(results.Sum() / results.Length, results[0], 0.0001);
		}

		[TestMethod]
		public void IntegrateCircular_2()
		{
			int numberOfFunctions = 16 * 49;
			double diameter = 300;
			double initialTolerance = 0.015;
			double expJ = Math.PI / 64 * Math.Pow(diameter, 4);

			double[] subdivision = new double[] { 32, 64 };
			(QuadrangleGaussPoints.GaussPointNumber, TriangleGaussPoints.GaussPointNumber)[] gp =
				new (QuadrangleGaussPoints.GaussPointNumber, TriangleGaussPoints.GaussPointNumber)[]
				{
					(QuadrangleGaussPoints.GaussPointNumber.Quad400, TriangleGaussPoints.GaussPointNumber.Tri79),
					(QuadrangleGaussPoints.GaussPointNumber.Quad121, TriangleGaussPoints.GaussPointNumber.Tri61),
					(QuadrangleGaussPoints.GaussPointNumber.Quad49, TriangleGaussPoints.GaussPointNumber.Tri33),
					(QuadrangleGaussPoints.GaussPointNumber.Quad12, TriangleGaussPoints.GaussPointNumber.Tri6),
				};

			for (int j = 0; j < subdivision.Length; j++)
			{
				ReinforcedConcreteSection section = GetCircularSection(diameter, subdivision[j], ConcreteMaterialEN1992.C25_30);

				double area = section.Area;
				double xg = section.Centroid.X;
				double yg = section.Centroid.Y;

				Func<double, double, double> funcX = new Func<double, double, double>((x, y) => Math.Pow(x, 2));
				Func<double, double, double> funcY = new Func<double, double, double>((x, y) => Math.Pow(y, 2));
				Func<double, double, double> funcXY = new Func<double, double, double>((x, y) => y * x);

				Func<double, double, double>[] arrayFuncX = new Func<double, double, double>[numberOfFunctions];
				Func<double, double, double>[] arrayFuncY = new Func<double, double, double>[numberOfFunctions];
				Func<double, double, double>[] arrayFuncXY = new Func<double, double, double>[numberOfFunctions];

				arrayFuncX = arrayFuncX.Select(i => funcX).ToArray();
				arrayFuncY = arrayFuncY.Select(i => funcY).ToArray();
				arrayFuncXY = arrayFuncY.Select(i => funcXY).ToArray();

				Console.WriteLine($"{subdivision[j]} subdivisions and {section.Mesh.FacesCount} faces");

				for (int k = 0; k < gp.Length; k++)
				{
					var watch = new System.Diagnostics.Stopwatch();
					watch.Start();

					double[] resultsX = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncX, section.Mesh, gp[k].Item1, gp[k].Item2);
					double[] resultsY = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncY, section.Mesh, gp[k].Item1, gp[k].Item2);
					double[] resultsXY = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncXY, section.Mesh, gp[k].Item1, gp[k].Item2);

					watch.Stop();
					Console.WriteLine($"Time for {(int)gp[k].Item1} and {(int)gp[k].Item2} gp and {numberOfFunctions} functions: {watch.ElapsedMilliseconds}");

					for (int i = 0; i < numberOfFunctions; i++)
					{
						CommonEqualAssert(resultsX[i], expJ, initialTolerance / (j + 1));
						CommonEqualAssert(resultsY[i], expJ, initialTolerance / (j + 1));
					}
				}
			}
		}

		[TestMethod]
		[TestCategory("Not implemented")]
		public void IntegrateCircular_3()
		{
			double diameter = 300;

			ReinforcedConcreteSection section = GetCircularSection(diameter, 32, ConcreteMaterialEN1992.C25_30);
			double expJ = Math.PI / 64 * Math.Pow(diameter, 4);

			var watch = new System.Diagnostics.Stopwatch();
			watch.Start();

			Func<double, double, (double, double)> func = new Func<double, double, (double, double)>((x, y) => { return (x * x, y * y); });

			Func<double, double, (double, double)>[] arrayFunc = new Func<double, double, (double, double)>[16 * 49];
			arrayFunc = arrayFunc.Select(i => func).ToArray();

			(double, double)[] results = GaussIntegration.IntegrationLinearShapeFunction(arrayFunc, section.Mesh, QuadrangleGaussPoints.GaussPointNumber.Quad400, TriangleGaussPoints.GaussPointNumber.Tri79);

			watch.Stop();
			Console.WriteLine($"Parallel + parallel: {watch.ElapsedMilliseconds}");

			for(int i = 0; i < results.Length; i++)
			{
				CommonEqualAssert(results[i].Item1, expJ);
				CommonEqualAssert(results[i].Item2, expJ);
			}
		}

		[TestMethod]
		public void IntegrateRectangular_1()
		{
			int numberOfFuncions = 16;
			double b = 300;
			double h = 300;
			ConcreteSectionRectangular section = new ConcreteSectionRectangular(h, b, ConcreteMaterialEN1992.C25_30);

			section.SetMeshSize(50);
			GPC.Geometry.Meshes.Mesh mesh = section.Mesh;

			double area = section.Area;
			double xg = section.Centroid.X;
			double yg = section.Centroid.Y;

			double expJx = b * Math.Pow(h, 3) / 12.0;
			double expJy = h * Math.Pow(b, 3) / 12.0;

			Func<double, double, double> funcY = new Func<double, double, double>((x, y) => { return Math.Pow(x - xg, 2); });
			Func<double, double, double> funcX = new Func<double, double, double>((x, y) => { return Math.Pow(y - yg, 2); });

			Func<double, double, double>[] arrayFuncX = new Func<double, double, double>[numberOfFuncions];
			Func<double, double, double>[] arrayFuncY = new Func<double, double, double>[numberOfFuncions];

			arrayFuncX = arrayFuncX.Select(i => funcX).ToArray();
			arrayFuncY = arrayFuncY.Select(i => funcY).ToArray();

			double[] resultsX400 = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncX, section.Mesh, QuadrangleGaussPoints.GaussPointNumber.Quad400, TriangleGaussPoints.GaussPointNumber.Tri79);
			double[] resultsY400 = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncY, section.Mesh, QuadrangleGaussPoints.GaussPointNumber.Quad400, TriangleGaussPoints.GaussPointNumber.Tri79);

			double[] resultsX121 = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncX, section.Mesh, QuadrangleGaussPoints.GaussPointNumber.Quad121, TriangleGaussPoints.GaussPointNumber.Tri61);
			double[] resultsY121 = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncY, section.Mesh, QuadrangleGaussPoints.GaussPointNumber.Quad121, TriangleGaussPoints.GaussPointNumber.Tri61);

			double[] resultsX49 = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncX, section.Mesh, QuadrangleGaussPoints.GaussPointNumber.Quad49, TriangleGaussPoints.GaussPointNumber.Tri33);
			double[] resultsY49 = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncY, section.Mesh, QuadrangleGaussPoints.GaussPointNumber.Quad49, TriangleGaussPoints.GaussPointNumber.Tri33);

			double[] resultsX12 = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncX, section.Mesh, QuadrangleGaussPoints.GaussPointNumber.Quad12, TriangleGaussPoints.GaussPointNumber.Tri6);
			double[] resultsY12 = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncY, section.Mesh, QuadrangleGaussPoints.GaussPointNumber.Quad12, TriangleGaussPoints.GaussPointNumber.Tri6);

			for (int i = 0; i < numberOfFuncions; i++)
			{
				CommonEqualAssert(resultsX400[i], expJx);
				CommonEqualAssert(resultsY400[i], expJy);
				CommonEqualAssert(resultsX121[i], expJx);
				CommonEqualAssert(resultsY121[i], expJy);
				CommonEqualAssert(resultsX49[i], expJx);
				CommonEqualAssert(resultsY49[i], expJy);
				CommonEqualAssert(resultsX12[i], expJx);
				CommonEqualAssert(resultsY12[i], expJy);
			}
		}

		[TestMethod]
		public void IntegrateRectangular_2()
		{
			int numberOfFunctions = 16 * 49;
			double b = 300;
			double h = 700;
			ConcreteSectionRectangular section = new ConcreteSectionRectangular(h, b, ConcreteMaterialEN1992.C25_30);

			(QuadrangleGaussPoints.GaussPointNumber, TriangleGaussPoints.GaussPointNumber)[] gp =
				new (QuadrangleGaussPoints.GaussPointNumber, TriangleGaussPoints.GaussPointNumber)[]
				{
					(QuadrangleGaussPoints.GaussPointNumber.Quad400, TriangleGaussPoints.GaussPointNumber.Tri79),
					(QuadrangleGaussPoints.GaussPointNumber.Quad121, TriangleGaussPoints.GaussPointNumber.Tri61),
					(QuadrangleGaussPoints.GaussPointNumber.Quad49, TriangleGaussPoints.GaussPointNumber.Tri33),
				};
			double[] meshSize = new double[] { 0, 400, 200, 100, 50 };

			for (int j = 0; j < meshSize.Length; j++)
			{
				section.SetMeshSize(meshSize[j]);
				GPC.Geometry.Meshes.Mesh mesh = section.Mesh;

				double area = section.Area;
				double xg = section.Centroid.X;
				double yg = section.Centroid.Y;

				double expJx = b * Math.Pow(h, 3) / 12.0;
				double expJy = h * Math.Pow(b, 3) / 12.0;

				Func<double, double, double> funcY = new Func<double, double, double>((x, y) => { return Math.Pow(x - xg, 2); });
				Func<double, double, double> funcX = new Func<double, double, double>((x, y) => { return Math.Pow(y - yg, 2); });

				Func<double, double, double>[] arrayFuncX = new Func<double, double, double>[numberOfFunctions];
				Func<double, double, double>[] arrayFuncY = new Func<double, double, double>[numberOfFunctions];

				arrayFuncX = arrayFuncX.Select(i => funcX).ToArray();
				arrayFuncY = arrayFuncY.Select(i => funcY).ToArray();

				Console.WriteLine($"{meshSize[j]} meshSize and {section.Mesh.FacesCount} faces");

				for (int k = 0; k < gp.Length; k++)
				{
					var watch = new System.Diagnostics.Stopwatch();
					watch.Start();

					double[] resultsX = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncX, section.Mesh, gp[k].Item1, gp[k].Item2);
					double[] resultsY = GaussIntegration.IntegrationLinearShapeFunction(arrayFuncY, section.Mesh, gp[k].Item1, gp[k].Item2);

					watch.Stop();
					Console.WriteLine($"Time for {(int)gp[k].Item1} and {(int)gp[k].Item2} gp and {numberOfFunctions} functions: {watch.ElapsedMilliseconds}");

					for (int i = 0; i < numberOfFunctions; i++)
					{
						CommonEqualAssert(resultsX[i], expJx);
						CommonEqualAssert(resultsY[i], expJy);
					}
				}
			}
		}
	}
}
	
