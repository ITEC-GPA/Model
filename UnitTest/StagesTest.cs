using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.FEM;
using GPC.Model.FEM.Properties;
using GPC.Model.Materials;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FemTest
{
    [TestClass]
    public class StagesTest : UnitTestBase
    {
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

        [TestMethod]
        public void StageTest1()
        {
            PlateProperty p1 = new PlateProperty(new SteelMaterial("", 1, 0.1, 2, 3, 4), 1, 2, "P1");
            PlateProperty p2 = new PlateProperty(new SteelMaterial("", 1, 0.1, 2, 3, 4), 10, 20, "P2");

            FemModel model = new FemModel();
            model.AddProperty(p1);
            model.AddProperty(p2);
            Mesh mesh = CreateSimpleMesh(10, 10, 2, 4, 0);
            model.AddMesh(mesh, p1.Name, null, null, null, null, null);

            var stage1 = model.AddStage("Stg1", FemModel.AnalysisTypes.Linear);
            var stage2 = model.AddStage("Stg2", FemModel.AnalysisTypes.Linear);

            var enumerator = model.GetElementsEnumerator();
            while (enumerator.MoveNext())
            {
                var fe = enumerator.Current;
                stage1.AddFiniteElement(fe);
            }

            enumerator = model.GetElementsEnumerator();
            while (enumerator.MoveNext())
            {
                var fe = enumerator.Current;
                if (fe.Id > 4)
                {
                    stage2.AddFiniteElement(fe, p2.Name);
                }
                else
                {
                    stage2.AddFiniteElement(fe);
                }
            }

            var m1 = stage1.ToModel();
            var m2 = stage2.ToModel();

            Assert.AreEqual(2, (m1.GetFiniteElement(5).Property as PlateProperty).MembraneThickness);
            Assert.AreEqual(20, (m2.GetFiniteElement(5).Property as PlateProperty).MembraneThickness, (m2.GetFiniteElement(5).Property as PlateProperty).MembraneThickness.ToString());
        }


        [TestMethod]
        public void StageTest2()
        {
            PlateProperty p1 = new PlateProperty(new SteelMaterial("", 1, 0.1, 2, 3, 4), 1, 2, "P1");
            PlateProperty p2 = new PlateProperty(new SteelMaterial("", 1, 0.1, 2, 3, 4), 10, 20, "P2");

            FemModel model = new FemModel();

            model.AddProperty(p1);
            model.AddProperty(p2);
            Mesh mesh = CreateSimpleMesh(10, 10, 2, 4, 0);
            model.AddMesh(mesh, p1.Name, null, null, null, null, null);

            var stage1 = model.AddStage("Stg1", FemModel.AnalysisTypes.Linear);
            var stage2 = model.AddStage("Stg2", FemModel.AnalysisTypes.Linear);

            var enumerator = model.GetElementsEnumerator();
            while (enumerator.MoveNext())
            {
                var fe = enumerator.Current;
                stage1.AddFiniteElement(fe);
            }

            enumerator = model.GetElementsEnumerator();
            while (enumerator.MoveNext())
            {
                var fe = enumerator.Current;
                if (fe.Id > 4)
                {
                    stage2.AddFiniteElement(fe, p2.Name);
                }
                else
                {
                    stage2.AddFiniteElement(fe);
                }
            }

            var m1 = stage1.ToModel();
            var m2 = stage2.ToModel();

            Assert.AreEqual(2, (m1.GetFiniteElement(5).Property as PlateProperty).MembraneThickness);
            Assert.AreEqual(20, (m2.GetFiniteElement(5).Property as PlateProperty).MembraneThickness, (m2.GetFiniteElement(5).Property as PlateProperty).MembraneThickness.ToString());
        }
    }
}