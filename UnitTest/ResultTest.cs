using GPC.Geometry;
using GPC.Model.Results;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModelObjectTest
{
    [TestClass]
    public class ResultTest : UnitTestBase
    {
        [TestMethod]
        public void PrincipalStressTest1()
        {

            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, -341, -895, 573, 777, -18); ;

            Assert.AreEqual(rps1.S11, 705, 1);
            Assert.AreEqual(rps1.S22, -524, 1);
            Assert.AreEqual(rps1.S33, -1417, 1);

        }

        [TestMethod]
        public void PrincipalStressTest2()
        {
            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, 49.8, 49.9, 0.059, -3.66, 0.521);

            Assert.AreEqual(rps1.S11, 49.838, 0.5);
            Assert.AreEqual(rps1.S22, 49.524, 0.5);
        }


        [TestMethod]
        public void PrincipalStressTest3()
        {
            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, 0, 0, 573, 777, -18);

            Assert.AreEqual(rps1.S11, 956, 1);
            Assert.AreEqual(rps1.S22, 17.1, 1);
            Assert.AreEqual(rps1.S33, -974, 1);
        }

        [TestMethod]
        public void PrincipalStressTest4()
        {
            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, 100, 200, 573, 0, 0);

            Assert.AreEqual(rps1.S11, 725.1774, 1);
            Assert.AreEqual(rps1.S22, -425.1774, 1);
        }

        [TestMethod]
        public void VonMisesStressTest1()
        {
            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, 56.89, 40.32, -1.065, 0, 0);

            double vm = rps1.SVM;

            Assert.AreEqual(vm, 51.151, 1);
        }


        [TestMethod]
        public void VonMisesStressTest2()
        {
            // https://www.graniteng.com/mohr-3d?lang=en

            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, 100, 200, 573, 400, 500);

            double vm = rps1.SVM;

            Assert.AreEqual(vm, 1498, 1);
        }

    }
}