using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GPC.Model.Elements;
using GPC.Model.Elements.Glasses;
using GPC.Model.Materials;
using GPC.Model.FEM.Properties;

namespace UnitTest
{
    [TestClass]
    public class HashCodeTest
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
    }
}
