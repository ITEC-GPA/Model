using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GPC.TestUtilities;
using GPC.Model.Glasses;
using GPC.Model.Materials;
using GPC.Model;

namespace ModelObjectTest
{
    [TestClass]
    public class GlassTest : UnitTestBase
    {

        private GlassMaterialAstm GetGlassMaterialAstm()
        {
            return new GlassMaterialAstm("Glass", 70000, 0.23, 1, 16, 23.3, 18.3, 0.001, 2500, 0.1);
        }

        private InterlayerMaterial GetInterlayerMaterial()
        {
            var it = new InterlayerMaterial("", 1, 0, InterlayerMaterial.InterlayerType.NormalPVB);
            it.AddShearModule(3, new double[] { 10, 20, 50 }, new double[] { 0.1, 0.2, 0.30 });
            it.AddShearModule(100, new double[] { 10, 20, 50 }, new double[] { 0.15, 0.25, 0.35 });
            return it;
        }


        [TestMethod]
        public void LaminatedGlassPackage()
        {

            MonolithicGlass mg1 = new MonolithicGlass("Mg1", 8, GetGlassMaterialAstm());
            MonolithicGlass mg2 = new MonolithicGlass("Mg2", 20, GetGlassMaterialAstm());
            MonolithicGlass mg3 = new MonolithicGlass("Mg3", 15, GetGlassMaterialAstm());
            MonolithicGlass mg4 = new MonolithicGlass("Mg4", 4, GetGlassMaterialAstm());
            MonolithicGlass mg5 = new MonolithicGlass("Mg5", 10, GetGlassMaterialAstm());

            Interlayer intr1 = new Interlayer("Int1", 0.76, GetInterlayerMaterial());
            Interlayer intr2 = new Interlayer("Int2", 0.76, GetInterlayerMaterial());
            Interlayer intr3 = new Interlayer("Int3", 0.76, GetInterlayerMaterial());
            Interlayer intr4 = new Interlayer("Int4", 0.76, GetInterlayerMaterial());

            LaminatedGlass lg = new LaminatedGlass("lg", new MonolithicGlass[] { mg1, mg2, mg3, mg4, mg5 }, new Interlayer[] { intr1, intr2, intr3, intr4 });

            IGlassPackage[] array = lg.GetGlassPackage();

            Assert.IsTrue(array.Length == 9);

            Assert.IsTrue(array[0].GetType() == typeof(MonolithicGlass));
            Assert.IsTrue(array[1].GetType() == typeof(Interlayer));
            Assert.IsTrue(array[2].GetType() == typeof(MonolithicGlass));
            Assert.IsTrue(array[3].GetType() == typeof(Interlayer));
            Assert.IsTrue(array[4].GetType() == typeof(MonolithicGlass));
            Assert.IsTrue(array[5].GetType() == typeof(Interlayer));
            Assert.IsTrue(array[6].GetType() == typeof(MonolithicGlass));
            Assert.IsTrue(array[7].GetType() == typeof(Interlayer));
            Assert.IsTrue(array[8].GetType() == typeof(MonolithicGlass));


            Assert.IsTrue((array[0] as ModelObject).Name  == mg1.Name);

            Assert.IsTrue((array[7] as ModelObject).Name == intr4.Name);
        }
    }
}
