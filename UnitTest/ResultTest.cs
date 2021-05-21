using GPC.Geometry;
using GPC.Model.Results;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace ModelObjectTest
{
    [TestClass]
    public class ResultTest : UnitTestBase
    {
        [TestMethod]
        public void PrincipalStressTest1()
        {

            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, -341, -895, 0, 573, 777, -18); ;

            Assert.AreEqual(rps1.S11, 705, 1);
            Assert.AreEqual(rps1.S22, -524, 1);
            Assert.AreEqual(rps1.S33, -1417, 1);

        }

        [TestMethod]
        public void PrincipalStressTest2()
        {
            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, 49.8, 49.9, 0, 0.059, -3.66, 0.521);

            Assert.AreEqual(rps1.S11, 49.838, 0.5);
            Assert.AreEqual(rps1.S22, 49.524, 0.5);
        }


        [TestMethod]
        public void PrincipalStressTest3()
        {
            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, 0, 0, 0, 573, 777, -18);

            Assert.AreEqual(rps1.S11, 956, 1);
            Assert.AreEqual(rps1.S22, 17.1, 1);
            Assert.AreEqual(rps1.S33, -974, 1);
        }

        [TestMethod]
        public void PrincipalStressTest4()
        {
            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, 100, 200, 0, 573, 0, 0);

            Assert.AreEqual(rps1.S11, 725.1774, 1);
            Assert.AreEqual(rps1.S22, -425.1774, 1);
        }

        [TestMethod]
        public void VonMisesStressTest1()
        {
            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, 56.89, 40.32, 0, -1.065, 0, 0);

            double vm = rps1.SVM;

            Assert.AreEqual(vm, 51.151, 1);
        }


        [TestMethod]
        public void VonMisesStressTest2()
        {
            // https://www.graniteng.com/mohr-3d?lang=en

            ResultStress rps1 = new ResultStress(CoordinateSystem.Global, 100, 200, 0, 573, 400, 500);

            double vm = rps1.SVM;

            Assert.AreEqual(vm, 1498, 1);
        }



        [TestMethod]
        public void StressOperatorsSumSubtactTest1()
        {
            ResultStress result = new ResultStress(CoordinateSystem.Global, 200, 400, 0, 1146, 800, 1000);

            ResultStress rs1 = new ResultStress(CoordinateSystem.Global, 100, 200, 0, 573, 400, 500);
            ResultStress rs2 = new ResultStress(CoordinateSystem.Global, 100, 200, 0, 573, 400, 500);

            Assert.IsTrue(rs1 + rs2 == result);
            Assert.IsTrue(result - rs2 == rs1);
            Assert.IsTrue(result - rs1 == rs2);
        }



        [TestMethod]
        public void StressOperatorsSumTest2()
        {
            CoordinateSystem cs = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(0, 1, 0), new Point3d(1, 0, 0));

            var trasformationMatrix = cs.TrfMatrix.Resize(3, 3);

            Matrix<double> stress = Matrix<double>.Build.Sparse(3, 3);
            stress[0, 0] = 100;
            stress[0, 1] = 101;
            stress[0, 2] = 102;
            stress[1, 0] = 101;
            stress[1, 1] = 200;
            stress[1, 2] = 202;
            stress[2, 0] = 102;
            stress[2, 1] = 202;
            stress[2, 2] = 300;

            Matrix<double> stressRotated = trasformationMatrix * stress * trasformationMatrix.Transpose();

            ResultStress rs1 = new ResultStress(CoordinateSystem.Global, stress[0, 0], stress[1, 1], stress[2,2], stress[0, 1], stress[0, 2], stress[1, 2]);
            ResultStress rs1Rotated = new ResultStress(cs, stressRotated[0, 0], stressRotated[1, 1], stressRotated[2, 2], stressRotated[0, 1], stressRotated[0, 2], stressRotated[1, 2]);

            var sum = rs1 + rs1Rotated;


            Console.WriteLine("Original:");
            Console.WriteLine(rs1.GetTensor());
            Console.WriteLine("Rotated:");
            Console.WriteLine(rs1Rotated.GetTensor());

            Console.WriteLine(sum.GetTensor());
            Console.WriteLine((rs1 + rs1Rotated).GetTensor());

            Assert.IsTrue(sum.Sxx == stress[0, 0] * 2.0, sum.Sxx.ToString());
            Assert.IsTrue(sum.Syy == stress[1, 1] * 2.0, sum.Syy.ToString());

            Assert.IsTrue(sum.GetTensor().Equals((rs1 + rs1Rotated).GetTensor()));
            Assert.IsTrue((sum.GetTensor() - (rs1 + rs1Rotated).GetTensor()).Equals(Matrix<double>.Build.Dense(3, 3)));


        }


        [TestMethod]
        public void StressOperatorsSumTest3()
        {
            CoordinateSystem cs2 = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(-1, -1, 0), new Point3d(1, -1, 0));
            CoordinateSystem cs1 = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 1, 0), new Point3d(-1, 1, 0));

            var trasformationMatrix = cs1.TrfMatrix.Resize(3, 3);

            Matrix<double> stress = Matrix<double>.Build.Sparse(3, 3);
            stress[0, 0] = 100;
            
            Matrix<double> stressRotated = trasformationMatrix * stress * trasformationMatrix.Transpose();

            ResultStress rs1 = new ResultStress(cs2, stress[0, 0], stress[1, 1], stress[2, 2], stress[0, 1], stress[0, 2], stress[1, 2]);
            ResultStress rs1Rotated = new ResultStress(cs1, -100, 0, 0, 0, 0, 0);

            var sum = rs1 + rs1Rotated;


            Console.WriteLine("Original:");
            Console.WriteLine(rs1.GetTensor());
            Console.WriteLine("Rotated:");
            Console.WriteLine(rs1Rotated.GetTensor());

            Console.WriteLine("SUM:");
            Console.WriteLine(sum.GetTensor());

            Assert.IsTrue(sum.Sxx == stress[0, 0] * 2.0, sum.Sxx.ToString());
            Assert.IsTrue(sum.Syy == stress[1, 1] * 2.0, sum.Syy.ToString());

            Assert.IsTrue(sum.GetTensor().Equals((rs1 + rs1Rotated).GetTensor()));
            Assert.IsTrue((sum.GetTensor() - (rs1 + rs1Rotated).GetTensor()).Equals(Matrix<double>.Build.Dense(3, 3)));


        }
    }
}