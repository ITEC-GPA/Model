using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Results;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GPC.Model.Elements;

namespace ModelObjectTest
{
    [TestClass]
    public class ResultTest
    {
        [TestMethod]
        public void PrincipalStressTest1() 
        {
            ResultPlateStress rps1 = new ResultPlateStress(new GhostElement(1),new LoadCase("lc1"), new ResultStressPoint(1), CoordinateSystem.Global, -341, -895, 573, 777, -18);

            rps1.GetPrincipalStress(out double s11, out double s22, out double s33);

            Assert.AreEqual(s11, 705, 1);
            Assert.AreEqual(s22, -524, 1);
            Assert.AreEqual(s33, -1417, 1);

        }

        [TestMethod]
        public void PrincipalStressTest2()
        {
            ResultPlateStress rps1 = new ResultPlateStress(new GhostElement(1), new LoadCase("lc1"), new ResultStressPoint(1), CoordinateSystem.Global, 49.8, 49.9, 0.059, -3.66, 0.521);


            rps1.GetPrincipalStress(out double s11, out double s22);

            Assert.AreEqual(s11, 49.838, 0.5);
            Assert.AreEqual(s22, 49.524, 0.5);
        }


        [TestMethod]
        public void PrincipalStressTest3()
        {
            ResultPlateStress rps1 = new ResultPlateStress(new GhostElement(1), new LoadCase("lc1"), new ResultStressPoint(1), CoordinateSystem.Global, 0, 0, 573, 777, -18);


            rps1.GetPrincipalStress(out double s11, out double s22, out double s33);

            Assert.AreEqual(s11, 956, 1);
            Assert.AreEqual(s22, 17.1, 1);
            Assert.AreEqual(s33, -974, 1);
        }

        [TestMethod]
        public void PrincipalStressTest4()
        {
            ResultPlateStress rps1 = new ResultPlateStress(new GhostElement(1), new LoadCase("lc1"), new ResultStressPoint(1), CoordinateSystem.Global, 100, 200, 573, 0, 0);


            rps1.GetPrincipalStress(out double s11, out double s22);

            Assert.AreEqual(s11, 725.1774, 1);
            Assert.AreEqual(s22, -425.1774, 1);
        }

        [TestMethod]
        public void VonMisesStressTest1()
        {
            ResultPlateStress rps1 = new ResultPlateStress(new GhostElement(1), new LoadCase("lc1"), new ResultStressPoint(1), CoordinateSystem.Global, 56.89, 40.32, -1.065, 0, 0);

            rps1.GetVMStress(out double vm);

            Assert.AreEqual(vm, 51.151, 1);
        }


        [TestMethod]
        public void VonMisesStressTest2()
        {
            // https://www.graniteng.com/mohr-3d?lang=en

            ResultPlateStress rps1 = new ResultPlateStress(new GhostElement(1), new LoadCase("lc1"), new ResultStressPoint(1), CoordinateSystem.Global, 100, 200, 573, 400, 500);

            rps1.GetVMStress(out double vm);

            Assert.AreEqual(vm, 1498, 1);
        }
    }
}