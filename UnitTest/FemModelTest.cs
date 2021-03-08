using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using GPC.Geometry.Meshes;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Loads;
using GPC.Model.LoadCases;
using GPC.Model.FreedomCases;
using GPC.Model.FEM;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using GPC.Model.Restrains;
using System.Diagnostics;
using System.Linq;

namespace FemTest
{
    [TestClass]
    public class FemModelTest
    {
        public TestContext TestContext { get; set; }

        private static string _outputFolder;
        private string _testName;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {

        }

        [TestInitialize]
        public void TestInitialize()
        {
            _outputFolder = Path.Combine(Directory.GetParent(TestContext.TestDir).ToString(), TestContext.FullyQualifiedTestClassName.Split(new char[] { '.' })[1]);
            Directory.CreateDirectory(_outputFolder);
            _testName = TestContext.TestName;
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (Directory.Exists(TestContext.TestDir))
                Directory.Delete(TestContext.TestDir, true);
        }


        #region Private Methods

        private Mesh CreateSimpleMesh(int incrementX, int incrementY, int numberOfFaceX, int numberOfFaceY, int numberOfVolumeZ, int incrementZ = 0)
        {            
            Mesh mesh = new Mesh();

            double[] xIncrement = new double[numberOfFaceX + 1];
            double[] yIncrement = new double[numberOfFaceY + 1];
            double[] zIncrement = new double[numberOfVolumeZ + 1];


            for (int i = 0; i < numberOfFaceX; i++)
            {
                if (i == 0)
                {
                    xIncrement[i] = 0;
                    xIncrement[i + 1] = incrementX;
                }
                else
                    xIncrement[i + 1] = xIncrement[i] + incrementX;


                mesh.AddFaceMesh(new[] {
                    new MeshVertex(new Point3d(xIncrement[i],                0,     0)),
                    new MeshVertex(new Point3d(xIncrement[i + 1],            0,     0)),
                    new MeshVertex(new Point3d(xIncrement[i + 1],   incrementY,     0)),
                    new MeshVertex(new Point3d(xIncrement[i],       incrementY,     0))
                });


                for (int j = 0; j < numberOfFaceY; j++)
                {
                    if (j == 0)
                    {
                        yIncrement[j] = 0;
                        yIncrement[j + 1] = incrementY;
                    }
                    else
                    {
                        yIncrement[j + 1] = yIncrement[j] + incrementY;

                        mesh.AddFaceMesh(new[] {
                            new MeshVertex(new Point3d(xIncrement[i],       yIncrement[j],              0)),
                            new MeshVertex(new Point3d(xIncrement[i + 1],   yIncrement[j],              0)),
                            new MeshVertex(new Point3d(xIncrement[i + 1],   yIncrement[j + 1],          0)),
                            new MeshVertex(new Point3d(xIncrement[i],       yIncrement[j + 1],          0))
                        });
                    }

                    for (int z = 0; z < numberOfVolumeZ; z++)
                    {
                        if (z == 0)
                        {
                            zIncrement[z] = 0;
                            zIncrement[z + 1] = incrementZ;
                        }
                        else
                        {
                            zIncrement[z + 1] = zIncrement[z] + incrementZ;
                            mesh.AddVolumeMesh(new[] {
                                new MeshVertex(new Point3d(xIncrement[i],       yIncrement[j],         zIncrement[z])   ),
                                new MeshVertex(new Point3d(xIncrement[i + 1],   yIncrement[j],         zIncrement[z])   ),
                                new MeshVertex(new Point3d(xIncrement[i + 1],   yIncrement[j + 1],     zIncrement[z])   ),
                                new MeshVertex(new Point3d(xIncrement[i],       yIncrement[j + 1],     zIncrement[z])   ),
                                new MeshVertex(new Point3d(xIncrement[i],       yIncrement[j + 1],     zIncrement[z + 1])),
                                new MeshVertex(new Point3d(xIncrement[i],       yIncrement[j + 1],     zIncrement[z + 1])),
                                new MeshVertex(new Point3d(xIncrement[i],       yIncrement[j + 1],     zIncrement[z + 1])),
                                new MeshVertex(new Point3d(xIncrement[i],       yIncrement[j + 1],     zIncrement[z + 1]))
                            });
                        }
                    }
                }

            }

            return mesh;
        }

        private void ExportMesh(Mesh mesh)
        {
            MeshExport.ExportToMshFormatv2(Path.Combine(_outputFolder, $"{_testName}Mesh.msh"), new List<Mesh>() { mesh });
        }

        private Shape CreateSimpleShape(double width, double height)
        {
            Polygon3d p = new Polygon3d()
            {
                new Point3d(0,0,0),
                new Point3d(width, 0, 0),
                new Point3d(width, height, 0),
                new Point3d(0, height, 0)
            };

            return new Shape(p);
        }

        #endregion


        #region Test
          
        [TestMethod]
        [TestCategory("Missing Assert")]
        public void FemModelTest1()
        {
            // Arrange   
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0,0,0),
                new Point3d(1,0,0),
                new Point3d(2,0,0)
            };

            Mesh mesh = CreateSimpleMesh(10, 10, 3, 5, 2, 20);
            Mesh mesh2 = CreateSimpleMesh(10, 10, 3, 5, 0, 0);

            GlassMaterial gm = new GlassMaterialAstm("", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            PlateProperty pp = new PlateProperty(gm, 1, 2);

            BrickProperty bp = new BrickProperty(gm);

            Dictionary<IPointLoad, int[]> pointLoads = new Dictionary<IPointLoad, int[]>();
            pointLoads.Add(new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCase("lc1", null)), new int[] { 1 });
            pointLoads.Add(new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCase("lc2", null)), new int[] { 2 });
            pointLoads.Add(new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCase("lc3", null)), new int[] { 3 });

            Dictionary<ILineLoad, int[]> lineLoads = new Dictionary<ILineLoad, int[]>();
            lineLoads.Add(new LineLoad(1, 2, 3, 4, 5, 6, new Line3d(Point3d.Origin, new Point3d(10, 20, 0)), new LoadCase("lc1", null)), new int[] { 1 });


            Dictionary<IAreaLoad, int[]> plateLoads = new Dictionary<IAreaLoad, int[]>();
            plateLoads.Add(new AreaLoad(1, 2, 3, new Shape(p1), new LoadCase("lc1", null)), new int[] { 1 });
            plateLoads.Add(new AreaLoad(1, 2, 3, new Shape(p1), new LoadCase("lc2", null)), new int[] { 2 });
            plateLoads.Add(new AreaLoad(1, 2, 3, new Shape(p1), new LoadCase("lc3", null)), new int[] { 3 });

            Dictionary<GeometryRestrain, int[]> geometryRestrains = new Dictionary<GeometryRestrain, int[]>();
            
            geometryRestrains.Add(new PointRestrain(Point3d.Origin, new FreedomCase("fc1"), CoordinateSystem.Global, new List<DofRestrain> { new DofRestrain(LinearSolver.DOF.DX) } ), new int[] { 1 }) ;
            geometryRestrains.Add(new PointRestrain(Point3d.Origin, new FreedomCase("fc2"), CoordinateSystem.Global, new List<DofRestrain> { new DofRestrain(LinearSolver.DOF.DX) } ), new int[] { 2 }) ;
            geometryRestrains.Add(new PointRestrain(Point3d.Origin, new FreedomCase("fc3"), CoordinateSystem.Global, new List<DofRestrain> { new DofRestrain(LinearSolver.DOF.DX) } ), new int[] { 3 });


            // Act
            FemModel femModel = new FemModel();

            femModel.AddMesh(mesh, pp, bp, pointLoads, lineLoads, plateLoads, geometryRestrains);


            // Assert
        }


        [TestMethod]
        public void FemModelTest2()
        {
            // Arrange

            double maximumEdgeLenght = 20;

            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(1, 0, 0),
                new Point3d(2, 0, 0)
            };

            Shape s = CreateSimpleShape(100, 200);


            GlassMaterial gm = new GlassMaterialAstm("", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            PlateProperty pp = new PlateProperty(gm, 1, 2);

            BrickProperty bp = new BrickProperty(gm);

            List<Load> loads = new List<Load>();

            loads.Add(new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCase("lc1", null)));
            loads.Add(new LineLoad(1, 2, 3, 4, 5, 6, new Line3d(new Point3d(50, 50, 0), new Point3d(100, 100, 0)), new LoadCase("lc2", null)));

            List<GeometryRestrain> restrains = new List<GeometryRestrain>();
            restrains.Add(new PointRestrain(Point3d.Origin, new FreedomCase("fc1"), CoordinateSystem.Global, new List<DofRestrain> { new DofRestrain(LinearSolver.DOF.DX) }));
            restrains.Add(new PointRestrain(Point3d.Origin, new FreedomCase("fc2"), CoordinateSystem.Global, new List<DofRestrain> { new DofRestrain(LinearSolver.DOF.DX) }));


            // Act
            FemModel femModel = new FemModel();

            Mesh.GenerateMeshOptions.Size = 10;

            femModel.AddShape(s, pp, loads, restrains);

            var mesh = femModel.GetMesh();

            // Assert
            
            foreach (var edge in mesh.Edges)
            {
                var vertex1 = mesh.Vertices.Where(i => i.Id == edge.A).DefaultIfEmpty(null).FirstOrDefault();
                var vertex2 = mesh.Vertices.Where(i => i.Id == edge.B).DefaultIfEmpty(null).FirstOrDefault();

                if (vertex1.Point.DistanceTo(vertex2.Point) > maximumEdgeLenght)
                {
                    ExportMesh(mesh);
                    Assert.Fail(vertex1.Point.DistanceTo(vertex2.Point).ToString());
                }
            }

        }

        [TestMethod]
        public void FemModelTest3()
        {
            double maximumEdgeLenght = 20;
            FemModel femModel = new FemModel();

            Shape s1 = CreateSimpleShape(100, 200);

            GlassMaterial gm = new GlassMaterialAstm("", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            MonolithicGlassProperty pp = new MonolithicGlassProperty(1, 2, gm);

            Mesh.GenerateMeshOptions.Size = 10;

            PointLoad p1 = new PointLoad(1, 2, 3, 4, 5, 6, new Point3d(35, 35, 0), new LoadCase("LC1", null));
            LineLoad l1 = new LineLoad(1, 2, 3, 4, 5, 6, new Line3d(new Point3d(35, 150, 0), new Point3d(75, 100, 0)), new LoadCase("LC2", null));

            femModel.AddShape(s1, pp, new List<Load>() { p1, l1}, null);


            var mesh = femModel.GetMesh();


            //Arrange
            foreach (var edge in mesh.Edges)
            {
                var vertex1 = mesh.Vertices.Where(i => i.Id == edge.A).DefaultIfEmpty(null).FirstOrDefault();
                var vertex2 = mesh.Vertices.Where(i => i.Id == edge.B).DefaultIfEmpty(null).FirstOrDefault();

                if (vertex1.Point.DistanceTo(vertex2.Point) > maximumEdgeLenght)
                {
                    ExportMesh(mesh);
                    Assert.Fail(vertex1.Point.DistanceTo(vertex2.Point).ToString());
                }
            }
        }


        [TestMethod]
        public void FemModelTest4()
        {
            double maximumEdgeLenght = 20;
            //Arrange
            FemModel femModel = new FemModel();

            Shape s1 = CreateSimpleShape(100, 200);
            Shape s2 = new Shape(s1);
            s2.Pan(100, 0, 0);

            GlassMaterial gm = new GlassMaterialAstm("", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            MonolithicGlassProperty pp = new MonolithicGlassProperty(1, 2, gm);

            Mesh.GenerateMeshOptions.Size = 10;

            PointLoad p1 = new PointLoad(1, 2, 3, 4, 5, 6, new Point3d(35, 35, 0), new LoadCase("LC1", null));
            LineLoad l1 = new LineLoad(1, 2, 3, 4, 5, 6, new Line3d(new Point3d(35, 150, 0), new Point3d(75, 100, 0)), new LoadCase("LC2", null));

            //Act
            femModel.AddShape(s1, pp, null, null);
            femModel.AddShape(s2, pp, null, null);

            var mesh = femModel.GetMesh();

            //Arrange
            foreach(var edge in mesh.Edges)
            {
                var vertex1 = mesh.Vertices.Where(i => i.Id == edge.A).DefaultIfEmpty(null).FirstOrDefault();
                var vertex2 = mesh.Vertices.Where(i => i.Id == edge.B).DefaultIfEmpty(null).FirstOrDefault();

                if (vertex1.Point.DistanceTo(vertex2.Point) > maximumEdgeLenght)
                {
                    ExportMesh(mesh);
                    Assert.Fail(vertex1.Point.DistanceTo(vertex2.Point).ToString());
                }
            }
        }
        #endregion

    }
}
