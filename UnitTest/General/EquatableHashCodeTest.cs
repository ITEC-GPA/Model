using GPC.Model;
using GPC.Model.Elements;
using GPC.Model.Glasses;
using GPC.Model.LoadCases;
using GPC.Model.Materials;
using GPC.Geometry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM.Properties;
using System;
using System.IO;
using GPC.Model.FEM;
using GPC.Model.Combinations;
using System.Collections.Generic;
using GPC.Model.Loads;
using GPC.TestUtilities;
using GPC.Model.FEM.Collections;
using GPC.Model.Restrains;

namespace GeneralTest
{
    [TestClass]
    public class EquatableHashCodeTest : UnitTestBase
    {

        [TestMethod]
        public void Test1()
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
        public void Test2()
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
        public void Test3()
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
            LoadCase sdl2 = new LoadCase("SDL", GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad, Guid.NewGuid());
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
            var pl1 = new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCase("lc1", null));
            var pl2 = new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCase("lc1", null));
            var pl3 = new PointLoad(1, 2, 3, 4, 5, 6, Point3d.Origin, new LoadCase("lc3", null));

            Assert.IsTrue(pl1.GetHashCode() == pl2.GetHashCode(), $"Obj1 {pl1.GetHashCode()} Obj2 {pl2.GetHashCode()}");
            Assert.IsTrue(pl1.GetHashCode() != pl3.GetHashCode(), $"Obj1 {pl1.GetHashCode()} Obj2 {pl3.GetHashCode()}");
        }



        [TestMethod]
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
        }


        [TestMethod]
        public void FemObjectEqualityComparer1()
        {
            Node n1 = new Node(Point3d.Origin, 1);
            Node n2 = new Node(Point3d.Origin, 2);
            Node n3 = new Node(Point3d.Origin, 2);

            Dictionary<FEMObject, int> dictWithComparer = new Dictionary<FEMObject, int>(new FEMObject.FemObjectWithIdComparer());
            Dictionary<FEMObject, int> dict = new Dictionary<FEMObject, int>();

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

            Dictionary<Node, int> dictWithComparer = new Dictionary<Node, int>(new FEMObject.FemObjectWithIdComparer());
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

            Dictionary<Node, int> dictWithComparer = new Dictionary<Node, int>(new FEMObject.FemObjectOnlyIdComparer());
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

            cnode1.Add(n1);
            cnode1.Add(n2);
            cnode1.Add(n3);

            cnode2.Add(n3);
            cnode2.Add(n1);
            cnode2.Add(n2);

            cnode3.Add(n1);
            cnode3.Add(n4);
            cnode3.Add(n2);
            cnode3.Add(n3);

            Assert.AreEqual(cnode1, cnode2);
            Assert.AreEqual(cnode1.GetHashCode(), cnode1.GetHashCode());
            Assert.AreNotEqual(cnode1, cnode3);
            Assert.AreNotEqual(cnode1.GetHashCode(), cnode3.GetHashCode());
        }

        [TestMethod]
        public void DofRestrainEqualsAndHashCode()
        {
            DofRestrain dr1 = new DofRestrain(Solver.DOF.DX, 0.5);
            DofRestrain dr2 = new DofRestrain(Solver.DOF.DZ, true);
            DofRestrain dr3 = new DofRestrain(Solver.DOF.DX, 0.5);

            Assert.IsFalse(dr1.Equals(dr2));
            Assert.IsTrue(dr1.Equals(dr3));
        }
    }
}