using GPC.Model.Elements;
using GPC.Model.Elements.Glasses;
using GPC.Model.LoadCases;
using GPC.Model.Materials;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace UnitTest
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
            GlassMaterialPrEn gm = new GlassMaterialPrEn("test", 10, 0.2, 30, GlassMaterialPrEn.GlassType.DrawnSheetGlass, GlassMaterialPrEn.SurfaceTreatment.AsProduced, GlassMaterialPrEn.PrestressType.Annealed, GlassMaterialPrEn.ManufactoringProcess.HorizontalToughening, 20, 30);
            MonolithicGlass mg = new MonolithicGlass("test", 10, gm);

            MonolithicGlassProperty mgp1 = new MonolithicGlassProperty(mg);
            MonolithicGlassProperty mgp2 = new MonolithicGlassProperty(mg);

            Assert.IsTrue(mgp1.Equals(mgp2));
        }

        [TestMethod]
        public void Test2()
        {
            GlassMaterialPrEn gm = new GlassMaterialPrEn("test", 10, 0.2, 30, GlassMaterialPrEn.GlassType.DrawnSheetGlass, GlassMaterialPrEn.SurfaceTreatment.AsProduced, GlassMaterialPrEn.PrestressType.Annealed, GlassMaterialPrEn.ManufactoringProcess.HorizontalToughening, 20, 30);

            MonolithicGlass mg1 = new MonolithicGlass("test", 10, gm);
            MonolithicGlass mg2 = new MonolithicGlass("test", 10, gm);

            MonolithicGlassProperty mgp1 = new MonolithicGlassProperty(mg1);
            MonolithicGlassProperty mgp2 = new MonolithicGlassProperty(mg2);

            Assert.IsTrue(mg1.Equals(mg2), "Glass are not equals");
            Assert.IsTrue(mgp1.Equals(mgp2), "Properties are not equals");
        }

        [TestMethod]
        public void Test3()
        {
            GlassMaterialPrEn gm1 = new GlassMaterialPrEn("test", 10, 0.2, 30, GlassMaterialPrEn.GlassType.DrawnSheetGlass, GlassMaterialPrEn.SurfaceTreatment.AsProduced, GlassMaterialPrEn.PrestressType.Annealed, GlassMaterialPrEn.ManufactoringProcess.HorizontalToughening, 20, 30);
            GlassMaterialPrEn gm2 = new GlassMaterialPrEn("test", 10, 0.2, 30, GlassMaterialPrEn.GlassType.DrawnSheetGlass, GlassMaterialPrEn.SurfaceTreatment.AsProduced, GlassMaterialPrEn.PrestressType.Annealed, GlassMaterialPrEn.ManufactoringProcess.HorizontalToughening, 20, 30);

            MonolithicGlass mg1 = new MonolithicGlass("test", 10, gm1);
            MonolithicGlass mg2 = new MonolithicGlass("test", 10, gm2);

            MonolithicGlassProperty mgp1 = new MonolithicGlassProperty(mg1);
            MonolithicGlassProperty mgp2 = new MonolithicGlassProperty(mg2);

            Assert.IsTrue(gm1.Equals(gm2), "Glass materials are not equals");
            Assert.IsTrue(mg1.Equals(mg2), "Glass are not equals");
            Assert.IsTrue(mgp1.Equals(mgp2), "Properties are not equals");
        }

        [TestMethod]
        public void Test4()
        {
            LoadCase sdl1 = new LoadCase("SDL", LoadCase.LoadCaseType.SuperImposedDeadLoad, Guid.NewGuid());
            LoadCase sdl2 = new LoadCase("SDL", LoadCase.LoadCaseType.SuperImposedDeadLoad, Guid.NewGuid());

            Assert.IsTrue(sdl1.Equals(sdl2));
        }
    }
}