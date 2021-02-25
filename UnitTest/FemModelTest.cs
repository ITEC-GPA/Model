using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GPC.Model.FEM;
using GPC.Geometry.Meshes;
using GPC.Geometry;

namespace FemTest
{
    [TestClass]
    public class FemModelTest
    {
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {

        }

        [TestInitialize]
        public void TestInitialize()
        {

        }

        [TestCleanup]
        public void CleanUp()
        {
            if (Directory.Exists(TestContext.TestDir))
                Directory.Delete(TestContext.TestDir, true);
        }


        #region Private Methods

        private Mesh CreateSimpleMesh(int incrementX, int incrementY, int numberOfFaceX, int numberOfFaceY)
        {            
            Mesh mesh = new Mesh();

            double[] xIncrement = new double[numberOfFaceX + 1];
            double[] yIncrement = new double[numberOfFaceY + 1];


            for (int i = 0; i < numberOfFaceX; i++)
            {
                if (i == 0)
                    xIncrement[i] = 0;
                else
                    xIncrement[i + 1] = xIncrement[i] + incrementX;


                mesh.AddFaceMesh(new[] {
                        new MeshVertex(new Point3d(xIncrement[i],                0,     0)),
                        new MeshVertex(new Point3d(xIncrement[i + 1],            0,     0)),
                        new MeshVertex(new Point3d(xIncrement[i + 1],   incrementY,     0)),
                        new MeshVertex(new Point3d(xIncrement[i],       incrementY,     0))
                });

                for (int j = 1; j < numberOfFaceY; j++)
                {
                    if (j == 0)
                        yIncrement[j] = 0;
                    else
                        yIncrement[j + 1] = yIncrement[j] + incrementY;

                    mesh.AddFaceMesh(new[] {
                        new MeshVertex(new Point3d(xIncrement[i],       yIncrement[i],         0)),
                        new MeshVertex(new Point3d(xIncrement[i + 1],   yIncrement[i + 1],     0)),
                        new MeshVertex(new Point3d(xIncrement[i + 1],   yIncrement[i + 1],     0)),
                        new MeshVertex(new Point3d(xIncrement[i],       yIncrement[i],         0))
                    });
                }

            }

            return mesh;
        }

        #endregion
        #region Test

        [TestMethod]
        public void Test1()
        {
            // Arrange            
            FemModel femModel = new FemModel();

            // Act
            Mesh mesh = CreateSimpleMesh(10, 10, 3, 5);

            //
        }

        #endregion

    }
}
