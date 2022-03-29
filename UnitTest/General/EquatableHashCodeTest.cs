using GPC.Model;
using GPC.Model.Elements;
using GPC.Model.Glasses;
using GPC.Model.LoadCases;
using GPC.Model.Materials;
using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Fem.Properties;
using System;
using System.Linq;
using System.IO;
using GPC.Model.Fem;
using GPC.Model.Combinations;
using System.Collections.Generic;
using GPC.Model.Loads;
using GPC.TestUtilities;
using GPC.Model.Fem.Collections;
using GPC.Model.Restrains;
using GPC.Model.Results;
using GPC.Geometry.Meshes;

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
        public void GlassProperty1()
        {
            GlassMaterialEn16612 gm = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass, GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed, GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);
            MonolithicGlass mg = new MonolithicGlass("test", 10, gm);

            MonolithicGlassProperty mgp1 = new MonolithicGlassProperty(mg, string.Empty);
            MonolithicGlassProperty mgp2 = new MonolithicGlassProperty(mg, string.Empty);

            Assert.IsTrue(mg is ModelObject);
            Assert.IsTrue(mgp1.Equals(mgp2));
            Assert.IsFalse(gm.Equals(mg));
        }

        [TestMethod]
        public void GlassProperty2()
        {
            GlassMaterialEn16612 gm = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass, GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed, GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);

            MonolithicGlass mg1 = new MonolithicGlass("test", 10, gm);
            MonolithicGlass mg2 = new MonolithicGlass("test", 10, gm);

            MonolithicGlassProperty mgp1 = new MonolithicGlassProperty(mg1, string.Empty);
            MonolithicGlassProperty mgp2 = new MonolithicGlassProperty(mg2, string.Empty);

            Assert.IsTrue(mg1.Equals(mg2), "Glass are not equals");
            Assert.IsTrue(mgp1.Equals(mgp2), "Properties are not equals");
            Assert.IsTrue(mgp1.Equals(mgp2), "Properties are not equals");

            Assert.IsFalse(mg1.Equals(gm));
        }

        [TestMethod]
        public void GlassProperty3()
        {
            GlassMaterialEn16612 gm1 = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass, GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed, GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);
            GlassMaterialEn16612 gm2 = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass, GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed, GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);

            MonolithicGlass mg1 = new MonolithicGlass("test", 10, gm1);
            MonolithicGlass mg2 = new MonolithicGlass("test", 10, gm2);

            MonolithicGlassProperty mgp1 = new MonolithicGlassProperty(mg1, string.Empty);
            MonolithicGlassProperty mgp2 = new MonolithicGlassProperty(mg2, string.Empty);

            Assert.IsFalse(gm1.Equals(mg1));
            Assert.IsFalse(gm1.Equals(mgp1));

            Assert.IsTrue(gm1.Equals(gm2), "Glass materials are not equals");
            Assert.IsTrue(mg1.Equals(mg2), "Glass are not equals");
            Assert.IsTrue(mgp1.Equals(mgp2), "Properties are not equals");
        }

        [TestMethod]
        public void LoadCase()
        {
            LoadCase sdl1 = new LoadCase("SDL", GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad, Guid.NewGuid());
            LoadCaseEn16612 ldpr = new LoadCaseEn16612("SDL", GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad, LoadCaseEn16612.LoadCaseEn16612Types.SnowCanopies, Guid.NewGuid());

            LoadCase lc3 = new LoadCaseEn16612("SDL", GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad, LoadCaseEn16612.LoadCaseEn16612Types.SnowCanopies, Guid.NewGuid());

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

            LaminatedGlass l1 = new LaminatedGlass("test", new MonolithicGlass[] { mg1, mg2 }, new Interlayer[] { intr1 });
            LaminatedGlass l2 = new LaminatedGlass("test", new MonolithicGlass[] { mg1, mg2 }, new Interlayer[] { intr1 });
            LaminatedGlass l3 = new LaminatedGlass("test", new MonolithicGlass[] { mg1, mg2 }, new Interlayer[] { intr2 });

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

            MonolithicGlassProperty mgp1 = new MonolithicGlassProperty(mg1, string.Empty);
            MonolithicGlassProperty mgp2 = new MonolithicGlassProperty(mg2, string.Empty);

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



        /*[TestMethod]
        public void StageConstruction1()
        {
            Combination cmb1 = new CombinationEn("cmb1", StandardEN1990.LimitStates.UltimateEquilibrium);
            Combination cmb2 = new CombinationEn("cmb2", StandardEN1990.LimitStates.ServiceabilityFrequent);
            Combination cmb3 = new CombinationEn("cmb2", StandardEN1990.LimitStates.UltimateEquilibrium);

            List<Combination> combinations = new List<Combination>();
            combinations.Add(cmb1);
            combinations.Add(cmb2);
            combinations.Add(cmb3);

            //Stage stc1 = new Stage("stg1", FemModel.AnalysisType.Linear, false, combinations);
            //Stage stc2 = new Stage("stg1", FemModel.AnalysisType.Linear, false, combinations);


            //Assert.AreEqual(stc1.GetHashCode(), stc2.GetHashCode());
            //Assert.AreEqual(stc1, stc2);
        }*/


        [TestMethod]
        public void FemObjectEqualityComparer1()
        {
            Node n1 = new Node(Point3d.Origin, 1);
            Node n2 = new Node(Point3d.Origin, 2);
            Node n3 = new Node(Point3d.Origin, 2);

            Dictionary<FemObject, int> dictWithComparer = new Dictionary<FemObject, int>(new FemObject.FemObjectWithIdComparer());
            Dictionary<FemObject, int> dict = new Dictionary<FemObject, int>();

            dictWithComparer.Add(n1, 1);
            dictWithComparer.Add(n2, 1);

            dict.Add(n1, 1);
            
            Assert.IsTrue(dictWithComparer.ContainsKey(n3));
            Assert.IsTrue(dict.ContainsKey(n2));
            Assert.IsTrue(dict.ContainsKey(n3));
        }


        [TestMethod]
        public void FemObjectEqualityComparer2()
        {
            Node n1 = new Node(Point3d.Origin, 1);
            Node n2 = new Node(Point3d.Origin, 2);
            Node n3 = new Node(Point3d.Origin, 2);

            Dictionary<Node, int> dictWithComparer = new Dictionary<Node, int>(new FemObject.FemObjectWithIdComparer());
            Dictionary<Node, int> dict = new Dictionary<Node, int>();

            dictWithComparer.Add(n1, 1);
            dictWithComparer.Add(n2, 1);

            dict.Add(n1, 1);

            Assert.IsTrue(dictWithComparer.ContainsKey(n3));
            Assert.IsTrue(dict.ContainsKey(n2));
            Assert.IsTrue(dict.ContainsKey(n3));
        }


        [TestMethod]
        public void FemObjectEqualityComparer3()
        {
            Node n1 = new Node(Point3d.Origin, 1);
            Node n2 = new Node(Point3d.Origin, 2);
            Node n3 = new Node(Point3d.Origin, 2);
            Node n4 = new Node(Point3d.Origin, 2);

            Dictionary<Node, int> dictWithComparer = new Dictionary<Node, int>(new FemObject.FemObjectOnlyIdComparer());
            Dictionary<Node, int> dict = new Dictionary<Node, int>();

            dictWithComparer.Add(n1, 1);
            dictWithComparer.Add(n2, 1);

            dict.Add(n1, 1);
            Assert.IsTrue(dict.ContainsKey(n2));
            Assert.IsTrue(dict.ContainsKey(n3));


            Assert.IsTrue(dictWithComparer.Count == 2);
            Assert.IsTrue(dictWithComparer.ContainsKey(n2));
            Assert.IsTrue(dictWithComparer.ContainsKey(n3));
            Assert.IsTrue(dictWithComparer.ContainsKey(n4));
        }



        [TestMethod]
        public void FemObjectEqualityComparer4()
        {
            FemObjectCollection<Node> cnode1 = new FemObjectCollection<Node>();
            FemObjectCollection<Node> cnode2 = new FemObjectCollection<Node>();
            FemObjectCollection<Node> cnode3 = new FemObjectCollection<Node>();

            Node n1 = new Node(Point3d.Origin, 1);
            Node n2 = new Node(Point3d.Origin, 2);
            Node n3 = new Node(new Point3d(0, 1, 2), 2);
            Node n4 = new Node(new Point3d(2, 1, 2), 2);

            cnode1.AddUnique(n1);
            cnode1.AddUnique(n2);
            cnode1.AddUnique(n3);

            cnode2.AddUnique(n3);
            cnode2.AddUnique(n1);
            cnode2.AddUnique(n2);

            cnode3.AddUnique(n1);
            cnode3.AddUnique(n4);
            cnode3.AddUnique(n2);
            cnode3.AddUnique(n3);

            Assert.AreEqual(cnode1, cnode2);
            Assert.AreEqual(cnode1.GetHashCode(), cnode1.GetHashCode());
            Assert.AreNotEqual(cnode1, cnode3);
            Assert.AreNotEqual(cnode1.GetHashCode(), cnode3.GetHashCode());
        }

        [TestMethod]
        public void DofRestrainEqualsAndHashCode()
        {
            DofRestrain dr1 = new DofRestrain(Solver.DOF.DX, 0.5);
            DofRestrain dr2 = new DofRestrain(Solver.DOF.DZ);
            DofRestrain dr3 = new DofRestrain(Solver.DOF.DX, 0.5);

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

            NodeResult nr1 = new NodeResult(lc1, new [] { resultLocationId1 });
            NodeResult nr2 = new NodeResult(lc2, new [] { resultLocationId2 });

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

            NodeResult nr1 = new NodeResult(cmb1, new [] { resultLocationId1 });
            NodeResult nr2 = new NodeResult(cmb2, new [] { resultLocationId2 });

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

        [TestMethod]
        [TestCategory("Mesh")]
        public void Mesh2()
        {
            Mesh mesh = CreateSimpleMesh(20, 30, 3, 4, 0, 0);

            GlassMaterial gm = new GlassMaterialAstm("gp1", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            MonolithicGlassProperty pp = new MonolithicGlassProperty(1, 2, gm.GetIsotropicFemMaterial(), "gp1");

            FemModel femModel = new FemModel();
            femModel.AddProperty(pp);
            femModel.AddMesh(mesh, pp.Name, null, null, null, null, null);

            Mesh mesh2 = femModel.GetMesh();

            Assert.IsTrue(mesh.Equals(mesh2));
        }


        [TestMethod]
        public void Mesh3()
        {
            Mesh mesh = CreateSimpleMesh(20, 30, 3, 4, 0, 0);

            GlassMaterial gm = new GlassMaterialAstm("gp1", 1, 0.2, 3, 4, 5, 6, 0.008, 0.008, 9);
            MonolithicGlassProperty pp = new MonolithicGlassProperty(1, 2, gm.GetIsotropicFemMaterial(), "gp1");

            FemModel femModel = new FemModel();
            femModel.AddProperty(pp);
            femModel.AddMesh(mesh, pp.Name, null, null, null, null, null, "gp1");

            Mesh mesh2 = femModel.GetMesh();

            Assert.IsTrue(mesh.Equals(mesh2));

            Assert.IsTrue(femModel.GetElements().First().GetGroups().First().Name == "gp1");

        }
    }
}