using GPC.Geometry;
using GPC.Model.Results;
using GPC.Model.LoadCases;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra;
using System;
using GPC.Utilities.Extensions ;

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

            Matrix<double> stress = Matrix<double>.Build.Sparse(3, 3);
            stress[0, 0] = 100;

            CoordinateSystem cs1 = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(-1, -1, 0), new Point3d(1, -1, 0));
            ResultStress rs1 = new ResultStress(cs1, stress[0, 0], stress[1, 1], stress[2, 2], stress[0, 1], stress[0, 2], stress[1, 2]);

            CoordinateSystem cs2 = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 1, 0), new Point3d(-1, 1, 0));
            ResultStress rs2 = new ResultStress(cs2, stress[0, 0], 0, 0, 0, 0, 0);


            Console.WriteLine("rs1 Global:");
            Console.WriteLine(rs1.GetTensor(true));

            Console.WriteLine("rs2 Global:");
            Console.WriteLine(rs2.GetTensor(true));

            var sum = rs1 + rs2;

            Console.WriteLine("Sum");
            Console.WriteLine(sum.GetTensor());

            Console.WriteLine("Sum Global");
            Console.WriteLine(sum.GetTensor(true));

            Assert.AreEqual(stress[0, 0] * 2.0, sum.Sxx, 1e-5, sum.Sxx.ToString());
            Assert.AreEqual(stress[1, 1] * 2.0, sum.Syy, 1e-5, sum.Syy.ToString());

        }


        [TestMethod]
        public void StressOperatorsSumTest4()
        {

            Matrix<double> stress = Matrix<double>.Build.Sparse(3, 3);
            stress[0, 0] = 100;

            CoordinateSystem cs1 = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(-1, -1, 0), new Point3d(1, -1, 0));
            ResultStress rs1 = new ResultStress(cs1, stress[0, 0], stress[1, 1], stress[2, 2], stress[0, 1], stress[0, 2], stress[1, 2]);

            CoordinateSystem cs2 = new CoordinateSystem(new Point3d(1, 2, 3), new Vector3d(1, 1, 0), new Vector3d(-1, 1, 0));
            ResultStress rs2 = new ResultStress(cs2, stress[0, 0], stress[1, 1], stress[2, 2], stress[0, 1], stress[0, 2], stress[1, 2]);


            Console.WriteLine("Rs1 Global:");
            Console.WriteLine(rs1.GetTensor(true));

            Console.WriteLine("Rs2 Global:");
            Console.WriteLine(rs2.GetTensor(true));

            var sum = rs1 + rs2;



            Console.WriteLine("Sum");
            Console.WriteLine(sum.GetTensor());

            Console.WriteLine("Sum Global");
            Console.WriteLine(sum.GetTensor(true));

            Assert.AreEqual(stress[0, 0] * 2.0, sum.Sxx, 1e-5, sum.Sxx.ToString());
            Assert.AreEqual(stress[1, 1] * 2.0, sum.Syy, 1e-5, sum.Syy.ToString());
            
            var sum2 = rs2 + rs1;

            Console.WriteLine("Sum2");
            Console.WriteLine(sum2.GetTensor());

            Console.WriteLine("Sum2 Global");
            Console.WriteLine(sum2.GetTensor(true));

            Assert.AreEqual(sum.Sxx , sum2.Sxx, 1e-5, sum.Sxx.ToString());
        }


        [TestMethod]
        public void StressOperatorsMultiplicationTest1()
        {

            Matrix<double> stress = Matrix<double>.Build.Sparse(3, 3);
            stress[0, 0] = -100;

            CoordinateSystem cs1 = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, -1, 0), new Point3d(1, 1, 0));
            ResultStress rs1 = new ResultStress(cs1, stress[0, 0], stress[1, 1], stress[2, 2], stress[0, 1], stress[0, 2], stress[1, 2]);


            var rs1Rotated = cs1.TrfMatrix.Resize(3, 3) * rs1.GetTensor() * cs1.TrfMatrix.Resize(3, 3).Transpose();
            var rs1Rotated2 = rs1.GetTensor() * cs1.TrfMatrix.Resize(3, 3);


            Console.WriteLine("RS1 ROTATED");
            Console.WriteLine(rs1Rotated);

            Console.WriteLine("RS1 ROTATED");
            Console.WriteLine(rs1Rotated2);

            Assert.IsTrue(rs1.Sxx == stress[0, 0]);
            Assert.AreEqual(-50, rs1Rotated[0, 0], 1E-10, rs1Rotated[0, 0].ToString());
            Assert.AreEqual(-50, rs1Rotated[1, 1], 1E-10, rs1Rotated[1, 1].ToString());
            Assert.AreEqual(50, rs1Rotated[1, 0], 1E-10, rs1Rotated[1, 0].ToString());

        }

        [TestMethod]
        public void StressTensorTest1()
        {

            Matrix<double> stress = Matrix<double>.Build.Sparse(3, 3);
            stress[0, 0] = -100;

            CoordinateSystem cs1 = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, -1, 0), new Point3d(1, 1, 0));
            ResultStress rs1 = new ResultStress(cs1, stress[0, 0], stress[1, 1], stress[2, 2], stress[0, 1], stress[0, 2], stress[1, 2]);


            var rs1Rotated = cs1.TrfMatrix.Resize(3, 3) * rs1.GetTensor() * cs1.TrfMatrix.Resize(3, 3).Transpose();

            Console.WriteLine("RS1 ROTATED");
            Console.WriteLine(rs1Rotated);

            Assert.IsTrue(rs1.Sxx == stress[0, 0]);
            Assert.AreEqual(-50, rs1Rotated[0, 0], 1E-10, rs1Rotated[0, 0].ToString());
            Assert.AreEqual(-50, rs1Rotated[1, 1], 1E-10, rs1Rotated[1, 1].ToString());
            Assert.AreEqual(50, rs1Rotated[1, 0],  1E-10, rs1Rotated[1, 0].ToString());
        }


        [TestMethod]
        public void StressTensorTest2()
        {

            Matrix<double> stress = Matrix<double>.Build.Sparse(3, 3);
            stress[0, 0] = 100;

            CoordinateSystem cs1 = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(-1, -1, 0), new Point3d(1, -1, 0));
            ResultStress rs1 = new ResultStress(cs1, stress[0, 0], stress[1, 1], stress[2, 2], stress[0, 1], stress[0, 2], stress[1, 2]);


            var rs1Rotated = cs1.TrfMatrix.Resize(3, 3) * rs1.GetTensor() * cs1.TrfMatrix.Resize(3, 3).Transpose();



            Console.WriteLine("RS1 ROTATED");
            Console.WriteLine(rs1Rotated);

            Assert.IsTrue(rs1.Sxx == stress[0, 0]);
            Assert.AreEqual(50, rs1Rotated[0, 0], 1E-10, rs1Rotated[0, 0].ToString());
            Assert.AreEqual(50, rs1Rotated[1, 1], 1E-10, rs1Rotated[1, 1].ToString());
            Assert.AreEqual(50, rs1Rotated[1, 0], 1E-10, rs1Rotated[1, 0].ToString());
        }


        [TestMethod]
        public void DisplacementOperatorsMultiplicationTest1()
        {

            CoordinateSystem cs1 = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(-1, -1, 0), new Point3d(1, -1, 0));

            ResultDisplacement rd = new ResultDisplacement(cs1, 1,2,3,4,5,6);

            var rd1 = rd * 2;
            var rd2 = rd * 3.0;

            Assert.AreEqual(rd1.D2, rd.D2 * 2, 1E-10);
            Assert.AreEqual(rd2.D2, rd.D2 * 3, 1E-10);
        }


        [TestMethod]
        public void DisplacementOperatorsSumTest1()
        {

            CoordinateSystem cs1 = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(-1, -1, 0), new Point3d(1, -1, 0));
            ResultDisplacement rd1 = new ResultDisplacement(cs1, 1, 2, 3, 4, 5, 6);

            CoordinateSystem cs2 = new CoordinateSystem(new Point3d(0, 0, 0), new Vector3d(1, 1, 0), new Vector3d(-1, 1, 0));
            ResultDisplacement rd2 = new ResultDisplacement(cs2, 1, 2, 3, 4, 5, 6);

            var sum = rd1 + rd2;
            
            Console.WriteLine(sum.GetLocalDisplacementsTuple().displacements);
            Console.WriteLine(sum.GetLocalDisplacementsTuple().rotations);

            Assert.AreEqual(sum.D2, rd1.D2 - rd2.D2, 1E-10, sum.D2.ToString());
            Assert.AreEqual(sum.D3, rd1.D3 * 2.0, 1E-10);

        }


        [TestMethod]
        public void StressArithmeticMeanTest1()
        {
            ResultStress rs1 = new ResultStress(CoordinateSystem.Global, 100, 200, 0, 573, 400, 500, "a");
            ResultStress rs2 = new ResultStress(CoordinateSystem.Global, 100, 200, 0, 573, 400, 500, "a");

            var rs = ResultStress.GetArithmeticMean(new ResultStress[] { rs1, rs2 });

            Assert.IsTrue(rs.Sxx == 100);
            Assert.IsTrue(rs.Syy == 200);
            Assert.IsTrue(rs.Szz == 0);
            Assert.IsTrue(rs.Sxy == 573);
            Assert.IsTrue(rs.Sxz == 400);
            Assert.IsTrue(rs.Syz == 500);
            Assert.IsTrue(rs.Name == "a");

        }


        [TestMethod]
        public void StressArithmeticMeanTest2()
        {
            ResultStress rs1 = new ResultStress(CoordinateSystem.Global, 100, 200, -100, 1000, -1000, 500, "rs1");
            ResultStress rs2 = new ResultStress(CoordinateSystem.Global, 200, 300, 100, 2000, -2000, 500, "rs2");

            var rs = ResultStress.GetArithmeticMean(new ResultStress[] { rs1, rs2 });

            Assert.IsTrue(rs.Sxx == 150);
            Assert.IsTrue(rs.Syy == 250);
            Assert.IsTrue(rs.Szz == 0);
            Assert.IsTrue(rs.Sxy == 1500, rs.Sxy.ToString());
            Assert.IsTrue(rs.Sxz == -1500);
            Assert.IsTrue(rs.Syz == 500);

            Assert.IsTrue(rs.Name == "rs1 rs2");
        }


        [TestMethod]
        public void StressArithmeticMeanTest3()
        {
            ResultStress rs1 = new ResultStress(CoordinateSystem.Global, 100, 200, -100, 1000, -1000, 500);
            ResultStress rs2 = new ResultStress(CoordinateSystem.Global, 200, 300, 100, 2000, -2000, 500);

            PlateResult pr = new PlateResult(new LoadCase("aa", LoadCase.LoadCaseTypes.SelfWeight), CoordinateSystem.Global, new ResultStress[] { rs1, rs1, rs2, rs2, rs2, rs2 }, 
                             new ResultLocationId[] { new ResultLocationId(1), new ResultLocationId(1), new ResultLocationId(2), new ResultLocationId(2), new ResultLocationId(3), new ResultLocationId(3) });

            var a = pr.GetMeanFaceResults();

            //var rs = ResultStress.GetArithmeticMean(new ResultStress[] { rs1, rs2 });

            //Assert.IsTrue(rs.Sxx == 150);
            //Assert.IsTrue(rs.Syy == 250);
            //Assert.IsTrue(rs.Szz == 0);
            //Assert.IsTrue(rs.Sxy == 1500, rs.Sxy.ToString());
            //Assert.IsTrue(rs.Sxz == -1500);
            //Assert.IsTrue(rs.Syz == 500);
        }
    }
}