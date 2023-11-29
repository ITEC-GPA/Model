using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model;
using GPC.Model.Collections;
using GPC.Model.Combinations;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Materials;
using GPC.Model.Restrains;
using GPC.Model.Results;
using GPC.Model.Sections.Glass;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace GeneralTest
{
    [TestClass]
    public class EquatableHashCodeTest : UnitTestBase
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

                int v1 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i], 0, 0)));
                int v2 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i + 1], 0, 0)));
                int v3 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i + 1], incrementY, 0)));
                int v4 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i], incrementY, 0)));

                mesh.Faces.AddUnique(new MeshFace(v1, v2, v3, v4));

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

                        int v11 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i], yIncrement[j], 0)));
                        int v12 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i + 1], yIncrement[j], 0)));
                        int v13 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i + 1], yIncrement[j + 1], 0)));
                        int v14 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i], yIncrement[j + 1], 0)));

                        mesh.Faces.AddUnique(new MeshFace(v11, v12, v13, v14));
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

                            int v21 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i], yIncrement[j], zIncrement[z])));
                            int v22 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i + 1], yIncrement[j], zIncrement[z])));
                            int v23 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i + 1], yIncrement[j + 1], zIncrement[z])));
                            int v24 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i], yIncrement[j + 1], zIncrement[z])));
                            int v25 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i], yIncrement[j + 1], zIncrement[z + 1])));
                            int v26 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i], yIncrement[j + 1], zIncrement[z + 1])));
                            int v27 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i], yIncrement[j + 1], zIncrement[z + 1])));
                            int v28 = mesh.Vertices.AddUnique(new MeshVertex(new Point3d(xIncrement[i], yIncrement[j + 1], zIncrement[z + 1])));

                            mesh.Volumes.AddUnique(new MeshVolume(v21, v22, v23, v24, v25, v26, v27, v28));
                        }
                    }
                }
            }

            return mesh;
        }

        [TestMethod]
        public void GlassProperty1()
        {
            GlassMaterialEn16612 gm = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass,
                GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed,
                GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);
            MonolithicGlass mg = new MonolithicGlass("test", 10, gm);

            GlassPlateProperty mgp1 = new GlassPlateProperty("", new List<IGlassLayer>() { mg });
            GlassPlateProperty mgp2 = new GlassPlateProperty("", new List<IGlassLayer>() { mg });

            Assert.IsTrue(mg is ModelObject);
            Assert.IsTrue(mgp1.Equals(mgp2));
            Assert.IsFalse(gm.Equals(mg));
        }

        [TestMethod]
        public void GlassProperty2()
        {
            GlassMaterialEn16612 gm = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass,
                GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed,
                GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);

            MonolithicGlass mg1 = new MonolithicGlass("test", 10, gm);
            MonolithicGlass mg2 = new MonolithicGlass("test", 10, gm);

            GlassPlateProperty mgp1 = new GlassPlateProperty("", new List<IGlassLayer>() { mg1 });
            GlassPlateProperty mgp2 = new GlassPlateProperty("", new List<IGlassLayer>() { mg2 });

            Assert.IsTrue(mg1.Equals(mg2), "Glass are not equals");
            Assert.IsTrue(mgp1.Equals(mgp2), "Properties are not equals");
            Assert.IsTrue(mgp1.Equals(mgp2), "Properties are not equals");

            Assert.IsFalse(mg1.Equals(gm));
        }

        [TestMethod]
        public void GlassProperty3()
        {
            GlassMaterialEn16612 gm1 = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass,
                GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed,
                GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);
            GlassMaterialEn16612 gm2 = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass,
                GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed,
                GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);

            MonolithicGlass mg1 = new MonolithicGlass("test", 10, gm1);
            MonolithicGlass mg2 = new MonolithicGlass("test", 10, gm2);

            GlassPlateProperty mgp1 = new GlassPlateProperty("", new List<IGlassLayer>() { mg1 });
            GlassPlateProperty mgp2 = new GlassPlateProperty("", new List<IGlassLayer>() { mg2 });

            Assert.IsFalse(gm1.Equals(mg1));
            Assert.IsFalse(gm1.Equals(mgp1));

            Assert.IsTrue(gm1.Equals(gm2), "Glass materials are not equals");
            Assert.IsTrue(mg1.Equals(mg2), "Glass are not equals");
            Assert.IsTrue(mgp1.Equals(mgp2), "Properties are not equals");
        }

        [TestMethod]
        public void LoadCase()
        {
            LoadCase sdl1 = new LoadCase("SDL", GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            LoadCaseEn16612 ldpr = new LoadCaseEn16612("SDL", GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad, LoadCaseEn16612.LoadCaseEn16612Types.SnowCanopies);

            LoadCase lc3 = new LoadCaseEn16612("SDL", GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad, LoadCaseEn16612.LoadCaseEn16612Types.SnowCanopies);

            Assert.IsTrue(sdl1.Equals(ldpr));
            Assert.IsFalse(ldpr.Equals(sdl1));

            //Assert.IsTrue(sdl1.Equals(sdl2));
            Assert.IsFalse(ldpr.Equals(sdl1));

            Assert.IsTrue(lc3.Equals(ldpr));

        }

        [TestMethod]
        public void LaminatedGlass()
        {
            GlassMaterialEn16612 gm1 = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass, GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed, GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);
            GlassMaterialEn16612 gm2 = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass, GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed, GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);

            MonolithicGlass mg1 = new MonolithicGlass("test", 10, gm1);
            MonolithicGlass mg2 = new MonolithicGlass("test", 10, gm2);

            InterlayerMaterial im1 = new InterlayerMaterial("im", 1000, 2000, InterlayerMaterial.InterlayerType.AcusticPVB);
            InterlayerMaterial im2 = new InterlayerMaterial("im", 1000, 2000, InterlayerMaterial.InterlayerType.AcusticPVB);

            Interlayer intr1 = new Interlayer("int", 0.4, im1);
            Interlayer intr2 = new Interlayer("int", 0.5, im2);

            GlassPlateProperty l1 = new GlassPlateProperty("test", new List<IGlassLayer>() { mg1, intr1, mg2 });
            GlassPlateProperty l2 = new GlassPlateProperty("test", new List<IGlassLayer>() { mg1, intr1, mg2 });
            GlassPlateProperty l3 = new GlassPlateProperty("test", new List<IGlassLayer>() { mg1, intr2, mg2 });

            Assert.IsTrue(gm1.Equals(gm2));
            Assert.IsTrue(mg1.Equals(mg2));
            Assert.IsTrue(im1.Equals(im2));

            Assert.IsFalse(intr1.Equals(intr2));
            Assert.IsTrue(l1.Equals(l2));
            Assert.IsFalse(l1.Equals(l3));
        }

        [TestMethod]
        public void GlassTest()
        {
            GlassMaterialEn16612 gm1 = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass, GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed, GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);
            GlassMaterialEn16612 gm2 = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass, GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed, GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);

            MonolithicGlass mg1 = new MonolithicGlass("test", 10, gm1);
            MonolithicGlass mg2 = new MonolithicGlass("test", 10, gm2);

            GlassPlateProperty mgp1 = new GlassPlateProperty("", new List<IGlassLayer>() { mg1 });
            GlassPlateProperty mgp2 = new GlassPlateProperty("", new List<IGlassLayer>() { mg2 });

            Assert.IsTrue(gm1.Equals(gm2));

            Assert.IsTrue(gm1.GetHashCode() == gm2.GetHashCode(), $"Material Obj1 {gm1.GetHashCode()} Obj2 {gm2.GetHashCode()}");
            Assert.IsTrue(mg1.GetHashCode() == mg2.GetHashCode(), $"Glass Obj1 {mg1.GetHashCode()} Obj2 {mg2.GetHashCode()}");
            Assert.IsTrue(mgp1.GetHashCode() == mgp2.GetHashCode(), $"Property Obj1 {mgp1.GetHashCode()} Obj2 {mgp2.GetHashCode()}");
        }

        [TestMethod]
        public void LoadTest1()
        {
            var pl1 = new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCaseBase("lc1"));
            var pl2 = new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCaseBase("lc1"));
            var pl3 = new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCaseBase("lc3"));

            Assert.IsTrue(pl1.GetHashCode() == pl2.GetHashCode(), $"Obj1 {pl1.GetHashCode()} Obj2 {pl2.GetHashCode()}");
            Assert.IsTrue(pl1.GetHashCode() != pl3.GetHashCode(), $"Obj1 {pl1.GetHashCode()} Obj2 {pl3.GetHashCode()}");
        }

        [TestMethod]
        public void FemObjectEqualityComparer1()
        {
            NodeElement n1 = new NodeElement(Point3d.Origin, null, "", 1);
            NodeElement n2 = new NodeElement(Point3d.Origin, null, "", 2);
            NodeElement n3 = new NodeElement(Point3d.Origin, null, "", 2);

            UniqueIdCollection<NodeElement> dict = new UniqueIdCollection<NodeElement> { n1, n2, n3 };

            Assert.IsTrue(dict.ContainsKey(n1.Id));
            Assert.IsTrue(dict.ContainsKey(n2.Id));
            Assert.IsTrue(dict.ContainsKey(n3.Id));
            Assert.IsTrue(dict.ContainsValue(n1));
            Assert.IsTrue(dict.ContainsValue(n2));
            Assert.IsTrue(dict.ContainsValue(n3));
        }

        [TestMethod]
        public void DofRestrainEqualsAndHashCode()
        {
            DofRestrain dr1 = new DofRestrain(GeometryRestrain.DOF.DX, 0.5);
            DofRestrain dr2 = new DofRestrain(GeometryRestrain.DOF.DZ);
            DofRestrain dr3 = new DofRestrain(GeometryRestrain.DOF.DX, 0.5);

            Assert.IsFalse(dr1.Equals(dr2));
            Assert.IsTrue(dr1.Equals(dr3));
        }

        [TestMethod]
        public void ResultStress1()
        {
            ResultStress rs1 = new ResultStress(CoordinateSystem.Global, 100, 200, 0, 573, 400, 500);
            ResultStress rs2 = new ResultStress(CoordinateSystem.Global, 100, 200, 0, 573, 400, 500);

            Assert.IsTrue(rs1.Equals(rs2));
            Assert.IsTrue(rs1 == rs2);
            Assert.IsFalse(rs1 != rs2);
        }

        [TestMethod]
        public void ResultNode1()
        {
            LoadCase lc1 = new LoadCase("lc", GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);
            LoadCase lc2 = new LoadCase("lc", GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);

            ResultDisplacement rd1 = new ResultDisplacement(0, 1, 2, 3, 0, 0);
            ResultDisplacement rd2 = new ResultDisplacement(0, 1, 2, 3, 0, 0);

            ResultLocationId resultLocationId1 = new ResultLocationId(new INodeResult[] { rd1 }, 1);
            ResultLocationId resultLocationId2 = new ResultLocationId(new INodeResult[] { rd2 }, 2);

            NodeResult nr1 = new NodeResult(lc1, new[] { resultLocationId1 });
            NodeResult nr2 = new NodeResult(lc2, new[] { resultLocationId2 });

            Assert.IsTrue(rd1.Equals(rd2));
            Assert.IsTrue(lc1.Equals(lc2));
            Assert.IsTrue(resultLocationId1.Equals(resultLocationId2));
            Assert.IsTrue(nr1.Equals(nr2));
        }

        [TestMethod]
        public void ResultNode2()
        {
            LoadCase lc1 = new LoadCase("lc", GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);
            LoadCase lc2 = new LoadCase("lc", GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);

            Combination cmb1 = new Combination("cmb1");
            cmb1.AddLoadCaseCoefficient(lc1, 1);
            Combination cmb2 = new Combination("cmb1");
            cmb2.AddLoadCaseCoefficient(lc2, 1);

            ResultDisplacement rd1 = new ResultDisplacement(0, 1, 2, 3, 0, 0);
            ResultDisplacement rd2 = new ResultDisplacement(0, 1, 2, 3, 0, 0);

            ResultLocationId resultLocationId1 = new ResultLocationId(new INodeResult[] { rd1 }, 1);
            ResultLocationId resultLocationId2 = new ResultLocationId(new INodeResult[] { rd2 }, 2);

            NodeResult nr1 = new NodeResult(cmb1, new[] { resultLocationId1 });
            NodeResult nr2 = new NodeResult(cmb2, new[] { resultLocationId2 });

            Assert.IsTrue(lc1.Equals(lc2));
            Assert.IsTrue(cmb1.Equals(cmb2));
            Assert.IsTrue(rd1.Equals(rd2));
            Assert.IsTrue(nr1.Equals(nr2));
        }

        [TestMethod]
        public void Mesh()
        {
            Mesh mesh1 = CreateSimpleMesh(20, 30, 3, 4, 0, 0);
            Mesh mesh2 = CreateSimpleMesh(20, 30, 3, 4, 0, 0);

            Assert.IsTrue(mesh1.Equals(mesh2));
        }
    }
}