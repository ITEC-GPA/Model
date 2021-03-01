using GPC.Model;
using GPC.Model.Elements;
using GPC.Model.Glasses;
using GPC.Model.LoadCases;
using GPC.Model.Materials;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM.Properties;
using System;
using System.IO;

namespace GeneralTest
{
    [TestClass]
    public class EquatableTest
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
    }
}