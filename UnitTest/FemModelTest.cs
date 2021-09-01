using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Collections.Generic;
using GPC.Geometry.Meshes;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Loads;
using GPC.Model.LoadCases;
using GPC.Model.Combinations;
using GPC.Model.FreedomCases;
using GPC.Model.FEM;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Materials;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.Restrains;
using GPC.Model.Results;
using System.Diagnostics;
using System.Linq;
using GPC.TestUtilities;
using GPC.Model.FEM.Collections;

namespace FemTest
{
    [TestClass]
    public class FemModelTest : UnitTestBase
    {


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
            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder(GetTestName() + "Mesh", "msh"), new List<Mesh>() { mesh });
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
        [TestCategory("Performance")]        
        public void FemModelTest1()
        {
            // Arrange   
            Polygon3d p1 = new Polygon3d()
            {
                new Point3d(0, 0, 0),
                new Point3d(1, 10, 0),
                new Point3d(2, 10, 0)
            };

            Stopwatch stopWatch = new Stopwatch();
            //stopWatch.Start();

            //Mesh mesh = CreateSimpleMesh(10, 10, 3, 5, 2, 20);
            //Mesh mesh2 = CreateSimpleMesh(10, 10, 3, 5, 0, 0);
            Mesh mesh = CreateSimpleMesh(40, 40, 25, 60, 2, 0);
            Debug.WriteLine($"Mesh vertices={mesh.VerticesCount}");
            //Debug.WriteLine(stopWatch.Elapsed, "Mesh created");

            GlassMaterial gm = new GlassMaterialAstm("", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            PlateProperty pp = new PlateProperty(gm.GetIsotropicFemMaterial(), 1, 2, "p");

            BrickProperty bp = new BrickProperty(gm.GetIsotropicFemMaterial(), "bp1");

            Dictionary<IPointLoad, int[]> pointLoads = new Dictionary<IPointLoad, int[]>();
            pointLoads.Add(new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCaseBase("lc1")), new int[] { 1 });
            pointLoads.Add(new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCaseBase("lc2")), new int[] { 2 });
            pointLoads.Add(new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCaseBase("lc3")), new int[] { 3 });

            Dictionary<ILineLoad, int[]> lineLoads = new Dictionary<ILineLoad, int[]>();
            lineLoads.Add(new LineLoad(1, 2, 3, 4, 5, 6, new Line3d(Point3d.Origin, new Point3d(10, 20, 0)), new LoadCaseBase("lc1")), new int[] { 1 });


            Dictionary<IAreaLoad, int[]> plateLoads = new Dictionary<IAreaLoad, int[]>();
            plateLoads.Add(new AreaLoad(1, 2, 3, new Shape(p1), new LoadCaseBase("lc1")), new int[] { 1 });
            plateLoads.Add(new AreaLoad(1, 2, 3, new Shape(p1), new LoadCaseBase("lc2")), new int[] { 2 });
            plateLoads.Add(new AreaLoad(1, 2, 3, new Shape(p1), new LoadCaseBase("lc3")), new int[] { 3 });

            Dictionary<GeometryRestrain, int[]> geometryRestrains = new Dictionary<GeometryRestrain, int[]>();
            
            geometryRestrains.Add(new PointRestrain(Point3d.Origin, new FreedomCase("fc1"), CoordinateSystem.Global, new List<DofRestrain> { new DofRestrain(LinearSolver.DOF.DX) } ), new int[] { 1 }) ;
            geometryRestrains.Add(new PointRestrain(Point3d.Origin, new FreedomCase("fc2"), CoordinateSystem.Global, new List<DofRestrain> { new DofRestrain(LinearSolver.DOF.DX) } ), new int[] { 2 }) ;
            geometryRestrains.Add(new PointRestrain(Point3d.Origin, new FreedomCase("fc3"), CoordinateSystem.Global, new List<DofRestrain> { new DofRestrain(LinearSolver.DOF.DX) } ), new int[] { 3 });


            // Act
            FemModel femModel = new FemModel();
            femModel.AddProperty(pp);
            femModel.AddProperty(bp);

            //stopWatch.Restart();
            stopWatch.Start();
            femModel.AddMesh(mesh, pp.Name, bp.Name, pointLoads, lineLoads, plateLoads, geometryRestrains);
            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "Mesh added");

            
            // Starting optimization from about 1600 ms
            Debug.WriteLine("Finish");
            Assert.IsTrue(stopWatch.ElapsedMilliseconds < 800, "Too slow");
        }


        [TestMethod]
        [TestCategory("Mesh")]
        public void Mesh1()
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
            PlateProperty pp = new PlateProperty(gm.GetIsotropicFemMaterial(), 1, 2, "p");

            BrickProperty bp = new BrickProperty(gm.GetIsotropicFemMaterial(), "bp1");

            List<Load> loads = new List<Load>
            {
                new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCaseBase("lc1")),
                new LineLoad(1, 2, 3, 4, 5, 6, new Line3d(new Point3d(50, 50, 0), new Point3d(100, 100, 0)), new LoadCaseBase("lc2")),
                new NormalAreaLoad(1, s, new LoadCaseBase("lc3"))
            };

            List<GeometryRestrain> restrains = new List<GeometryRestrain>();
            restrains.Add(new PointRestrain(Point3d.Origin, new FreedomCase("fc1"), CoordinateSystem.Global, new List<DofRestrain> { new DofRestrain(LinearSolver.DOF.DX) }));
            restrains.Add(new PointRestrain(Point3d.Origin, new FreedomCase("fc2"), CoordinateSystem.Global, new List<DofRestrain> { new DofRestrain(LinearSolver.DOF.DX) }));
            //restrains.Add(new LineRestrain(new Line3d(new Point3d(0, 0, 0), new Point3d(1, 0, 0)), new FreedomCase("fc2"), CoordinateSystem.Global, new List<DofRestrain> { new DofRestrain(LinearSolver.DOF.DX) }));


            // Act
            FemModel femModel = new FemModel();
            femModel.AddProperty(pp);
            femModel.AddProperty(bp);

            Mesh.GenerateOptions meshOptions = new Mesh.GenerateOptions
            {
                MeshSize = 10
            };

            femModel.AddShape(s, pp.Name, meshOptions, loads, restrains);

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

            foreach(var element in femModel.GetElements())
            {
                if (element is Plate plate)
                {
                    if (plate.AttributesLoadCase.Count != 1)
                        Assert.Fail($"Plate{plate.Id}");
                }
            }

        }

        [TestMethod]
        [TestCategory("Mesh")]
        public void Mesh2()
        {
            double maximumEdgeLenght = 20;
            FemModel femModel = new FemModel();

            Shape s1 = CreateSimpleShape(100, 200);

            GlassMaterial gm = new GlassMaterialAstm("gp1", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            MonolithicGlassProperty pp = new MonolithicGlassProperty(1, 2, gm.GetIsotropicFemMaterial(), "mgp");

            Mesh.GenerateOptions meshOptions = new Mesh.GenerateOptions();
            meshOptions.MeshSize = 10;

            PointLoad p1 = new PointLoad(1, 2, 3, 4, 5, 6, new Point3d(35, 35, 0), new LoadCaseBase("LC1"));
            LineLoad l1 = new LineLoad(1, 2, 3, 4, 5, 6, new Line3d(new Point3d(35, 150, 0), new Point3d(75, 100, 0)), new LoadCaseBase("LC2"));

            femModel.AddProperty(pp);
            femModel.AddShape(s1, pp.Name, meshOptions, new List<Load>() { p1, l1}, null);


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
        [TestCategory("Mesh")]
        public void Mesh3()
        {
            double meshSize = 100;
            double maximumEdgeLenght = meshSize*1.2;

            //Arrange
            FemModel femModel = new FemModel();

            Shape s1 = CreateSimpleShape(100, 200);
            Shape s2 = new Shape(s1);
            s2.Move(100, 0, 0);
            
            GlassMaterial gm = new GlassMaterialAstm("gp1", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            MonolithicGlassProperty pp = new MonolithicGlassProperty(1, 2, gm.GetIsotropicFemMaterial(), "gp1");

            Mesh.GenerateOptions meshOptions = new Mesh.GenerateOptions();
            meshOptions.MeshSize = meshSize;

            PointLoad p1 = new PointLoad(1, 2, 3, 4, 5, 6, new Point3d(35, 35, 0), new LoadCaseBase("LC1"));
            LineLoad l1 = new LineLoad(1, 2, 3, 4, 5, 6, new Line3d(new Point3d(35, 150, 0), new Point3d(75, 100, 0)), new LoadCaseBase("LC2"));

            femModel.AddProperty(pp);
            //Act
            femModel.AddShape(s1, pp.Name, meshOptions, null, null);
            femModel.AddShape(s2, pp.Name, meshOptions, null, null);

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


        [TestMethod]
        [TestCategory("Mesh")]
        public void Mesh4()
        {
            double maximumEdgeLenght = 55;

            //Arrange
            FemModel femModel = new FemModel();

            Shape s1 = CreateSimpleShape(800, 1600);

            GlassMaterial gm = new GlassMaterialAstm("gp1", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            MonolithicGlassProperty pp = new MonolithicGlassProperty(1, 2, gm.GetIsotropicFemMaterial(), "gp1");

            Mesh.GenerateOptions meshOptions = new Mesh.GenerateOptions();
            meshOptions.MeshSize = 50;

            LineLoad l1 = new LineLoad(1, 2, 3, 4, 5, 6, new Line3d(new Point3d(0, 500, 0), new Point3d(800, 500, 0)), new LoadCaseBase("LC2"));

            femModel.AddProperty(pp);
            //Act
            femModel.AddShape(s1, pp.Name, meshOptions, null, null);

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
        [TestCategory("Elements")]
        public void Elements1()
        {

            Shape s1 = CreateSimpleShape(800, 1600);

            GlassMaterial gm = new GlassMaterialAstm("gp1", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            MonolithicGlassProperty pp = new MonolithicGlassProperty(1, 2, gm.GetIsotropicFemMaterial(), "gp1");

            Mesh.GenerateOptions meshOptions = new Mesh.GenerateOptions
            {
                MeshSize = 50
            };


            FemModel femModel = new FemModel();
            femModel.AddProperty(pp);
            femModel.AddShape(s1, pp.Name, meshOptions, null, null);

            Node node = femModel.GetNode(0);

            if (node != null)
            {
                Console.WriteLine(node.Id);
                Assert.Fail();
            }
            else
            {
                // ok
            }

        }


        [TestMethod]
        [TestCategory("Missing Assert")]
        [TestCategory("Constrain")]
        public void Constrain1()
        {
            FemModel femModel = new FemModel();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int r1 = femModel.AddCostrain(new GPC.Model.FEM.Costrains.RigidLink(new Node(0, 0, 0), new Node(0, 0, 1)));
            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "R1");

            stopWatch.Restart();
            int r2 = femModel.AddCostrain(new GPC.Model.FEM.Costrains.RigidLink(new Node(0, 0, 1), new Node(0, 0, 2)));
            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "R2");
        }




        [TestMethod]
        [TestCategory("Attributes")]
        public void Attributes1()
        {
            //Arrange
            FemModel femModel = new FemModel();

            Shape s1 = CreateSimpleShape(800, 1600);

            GlassMaterial gm = new GlassMaterialAstm("gp1", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            MonolithicGlassProperty pp = new MonolithicGlassProperty(1, 2, gm.GetIsotropicFemMaterial(), "gp1");

            Mesh.GenerateOptions meshOptions = new Mesh.GenerateOptions
            {
                MeshSize = 50
            };

            NormalAreaLoad l1 = new NormalAreaLoad(1, s1, new LoadCaseBase("LC2"));

            femModel.AddProperty(pp);

            //Act
            femModel.AddShape(s1, pp.Name, meshOptions, new List<Load>() { l1 }, null);

            foreach(var element in femModel.GetElements())
            {
                Assert.IsTrue(element.AttributesLoadCase.Count == 1, element.AttributesLoadCase.Count.ToString()) ;
                Assert.IsTrue(element.AttributesLoadCase.FirstOrDefault().LoadCaseName == "LC2");
                Assert.IsTrue(element.AttributesLoadCase.FirstOrDefault().GetType() == typeof(PlateNormalPressureAttribute));
            }

        }

        [TestMethod]
        [TestCategory("Attributes")]
        public void Attributes2()
        {
            //Arrange
            FemModel femModel = new FemModel();

            Shape s1 = CreateSimpleShape(320, 800);

            GlassMaterial gm = new GlassMaterialAstm("gp1", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            MonolithicGlassProperty pp = new MonolithicGlassProperty(1, 2, gm.GetIsotropicFemMaterial(), "gp1");

            Mesh.GenerateOptions meshOptions = new Mesh.GenerateOptions
            {
                MeshSize = 16
            };

            NormalAreaLoad l1 = new NormalAreaLoad(1, s1, new LoadCaseBase("LC2"));

            femModel.AddProperty(pp);

            //Act
            femModel.AddShape(s1, pp.Name, meshOptions, new List<Load>() { l1 }, null);

            MeshExport.ExportToMshFormatv2(base.GetFilePathInOutputFolder("exp", "msh"), new List<Mesh>() { femModel.GetMesh() });


            Assert.IsTrue(femModel.GetElements().Length == (800*320)/(16*16), $"Count:{femModel.GetElements().Length} Expected:{(800 * 320) / (16 * 16)} ");

            foreach (var element in femModel.GetElements())
            {
                Assert.IsTrue(element.AttributesLoadCase.Count == 1, $"Id:{element.Id} {element.AttributesLoadCase.Count}" );
                Assert.IsTrue(element.AttributesLoadCase.FirstOrDefault().LoadCaseName == "LC2");
                Assert.IsTrue(element.AttributesLoadCase.FirstOrDefault().GetType() == typeof(PlateNormalPressureAttribute));
            }
            
        }

        #endregion

    }
}
