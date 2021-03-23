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

namespace GeneralTest
{
    [TestClass]
    public class EquatableHashCodeTest
    {
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Nothing
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // Nothing
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (Directory.Exists(TestContext.TestDir))
                Directory.Delete(TestContext.TestDir, true);
        }

        [TestMethod]
        public void Test1()
        {
            GlassMaterialEn16612 gm = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass, GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed, GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);
            MonolithicGlass mg = new MonolithicGlass("test", 10, gm);

            MonolithicGlassProperty mgp1 = new MonolithicGlassProperty(mg);
            MonolithicGlassProperty mgp2 = new MonolithicGlassProperty(mg);

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

            MonolithicGlassProperty mgp1 = new MonolithicGlassProperty(mg1);
            MonolithicGlassProperty mgp2 = new MonolithicGlassProperty(mg2);

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

            MonolithicGlassProperty mgp1 = new MonolithicGlassProperty(mg1);
            MonolithicGlassProperty mgp2 = new MonolithicGlassProperty(mg2);

            Assert.IsFalse(gm1.Equals(mg1));
            Assert.IsFalse(gm1.Equals(mgp1));

            Assert.IsTrue(gm1.Equals(gm2), "Glass materials are not equals");
            Assert.IsTrue(mg1.Equals(mg2), "Glass are not equals");
            Assert.IsTrue(mgp1.Equals(mgp2), "Properties are not equals");
        }

        [TestMethod]
        public void LoadCase()
        {
            LoadCase sdl1 = new LoadCase("SDL", GPC.Model.LoadCases.LoadCase.LoadCaseType.SuperImposedDeadLoad, Guid.NewGuid());
            LoadCase sdl2 = new LoadCase("SDL", GPC.Model.LoadCases.LoadCase.LoadCaseType.SuperImposedDeadLoad, Guid.NewGuid());
            LoadCasePrEn ldpr = new LoadCasePrEn("SDL", GPC.Model.LoadCases.LoadCase.LoadCaseType.SuperImposedDeadLoad, LoadCasePrEn.LoadCasePrEnType.SnowCanopies, Guid.NewGuid());

            LoadCase lc3 = new LoadCasePrEn("SDL", GPC.Model.LoadCases.LoadCase.LoadCaseType.SuperImposedDeadLoad, LoadCasePrEn.LoadCasePrEnType.SnowCanopies, Guid.NewGuid());

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
        public void FemObject()
        {
            Stage s1 = new Stage("S1", FemModel.AnalysisType.Linear);
            Stage s2 = new Stage("S2", FemModel.AnalysisType.Linear);

            Node n1 = new Node(Point3d.Origin, 1);
            Node n2 = new Node(Point3d.Origin, 2);

            n1.SetStageActive(s1, false);
            n1.SetStageActive(s2, true);
            n2.SetStageActive(s1, false);
            n2.SetStageActive(s2, true);


            Assert.IsFalse(n1.IsStageActive(s1));
            Assert.IsTrue(n1.IsStageActive(s2));

            Assert.IsTrue(n1.Equals(n2));

            Assert.AreNotEqual(s1.GetHashCode(), s2.GetHashCode());

            Assert.AreEqual(n1.GetHashCode(), n2.GetHashCode());
        }


        [TestMethod]
        public void GlassTest()
        {
            GlassMaterialEn16612 gm1 = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass, GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed, GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);
            GlassMaterialEn16612 gm2 = new GlassMaterialEn16612("test", 10, 0.2, 30, GlassMaterialEn16612.GlassTypes.DrawnSheetGlass, GlassMaterialEn16612.SurfaceTreatments.AsProduced, GlassMaterialEn16612.PrestressTypes.Annealed, GlassMaterialEn16612.ManufactoringProcesses.HorizontalToughening, 20, 30);

            MonolithicGlass mg1 = new MonolithicGlass("test", 10, gm1);
            MonolithicGlass mg2 = new MonolithicGlass("test", 10, gm2);

            MonolithicGlassProperty mgp1 = new MonolithicGlassProperty(mg1);
            MonolithicGlassProperty mgp2 = new MonolithicGlassProperty(mg2);

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
            Combination cmb1 = new CombinationEn("cmb1", CombinationEn.CombinationType.UltimateEquilibrium);
            Combination cmb2 = new CombinationEn("cmb2", CombinationEn.CombinationType.ServiceabilityFrequent);
            Combination cmb3 = new CombinationEn("cmb2", CombinationEn.CombinationType.UltimateEquilibrium);

            List<Combination> combinations = new List<Combination>();
            combinations.Add(cmb1);
            combinations.Add(cmb2);
            combinations.Add(cmb3);

            Stage stc1 = new Stage("stg1", FemModel.AnalysisType.Linear, false, combinations);
            Stage stc2 = new Stage("stg1", FemModel.AnalysisType.Linear, false, combinations);


            Assert.AreEqual(stc1.GetHashCode(), stc2.GetHashCode());
            Assert.AreEqual(stc1, stc2);
        }




        [TestMethod]
        public void FemObjectEqualityComparer()
        {
            Node n1 = new Node(Point3d.Origin, 1);
            Node n2 = new Node(Point3d.Origin, 2);
            Node n3 = new Node(Point3d.Origin, 2);

            Dictionary<FEMObject, int> dictWithComparer = new Dictionary<FEMObject, int>(new FemObjectIdComparer());
            Dictionary<FEMObject, int> dict = new Dictionary<FEMObject, int>();

            dictWithComparer.Add(n1, 1);
            dictWithComparer.Add(n2, 1);

            dict.Add(n1, 1);
            
            Assert.IsTrue(dictWithComparer.ContainsKey(n3));
            Assert.IsTrue(dict.ContainsKey(n2));
            Assert.IsTrue(dict.ContainsKey(n3));
        }

    }
}