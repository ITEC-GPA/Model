using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Results;
using GPC.Model.Sections.Bolt;
using GPC.Utilities.Maths;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace UnitTest.Bolt
{
    [TestClass]
    public class BoltGridTest
    {
        [TestMethod]
        public void Test01_DoubleApproximation()
        {
            // Return correct value from double representation of diameter.
            var DiaKeys = BoltSection.ThreadedAreas.Keys.ToList();
            var Mat = BoltMaterialEN1993Data.Class10_9;

            foreach (var key in DiaKeys)
            {
                var SecPlus = new BoltSection((double)key + 0.0000001, Mat);
                Assert.AreEqual(BoltSection.ThreadedAreas[key], SecPlus.CalculateAreaEff());
                var SecMinus = new BoltSection((double)key - 0.0000001, Mat);
                Assert.AreEqual(BoltSection.ThreadedAreas[key], SecMinus.CalculateAreaEff());
            }
        }

        [TestMethod]
        public void Test02_DoubleApproximation()
        {
            // Return correct value from double representation of diameter, with approximate method.
            var DiaKeys = BoltSection.ThreadedAreas.Keys.ToList();
            var Mat = BoltMaterialEN1993Data.Class10_9;
            double maxError = 0.03;
            double diff = 0.01; // It must be greater than the tolerance value used in CalculateDiameterDecimal().

            foreach (var key in DiaKeys)
            {
                var SecPlus = new BoltSection((double)key + diff, Mat);
                Assert.IsTrue(Error.AreEqualsDouble(BoltSection.ThreadedAreas[key], SecPlus.CalculateAreaEff(), maxError));
                var SecMinus = new BoltSection((double)key - diff, Mat);
                Assert.IsTrue(Error.AreEqualsDouble(BoltSection.ThreadedAreas[key], SecMinus.CalculateAreaEff(), maxError));
            }
        }

        [TestMethod]
        public void Test03_ForceCalculation()
        {
            var BG = new BoltGrid(new double[] { }, new double[] { 200 }, 12);
            var BarSys = new CoordinateSystem(BG.CalculateBarycenter(), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, 5000, 4000, 0, 0, 0, BarSys);
            var res = BG.CalculateShearForcesElastic(VetSoll);

            // Solution
            var SolBeam = new ResultBeamForces(0, 5000.0 / 2.0, 4000.0 / 2.0, 0, 0, 0, BarSys);
            foreach (var SolB in res)
            {
                Assert.IsTrue(Error.AreEqualsDouble(SolB.Value.V1, SolBeam.V1));
                Assert.IsTrue(Error.AreEqualsDouble(SolB.Value.V2, SolBeam.V2));
            }
            Assert.IsTrue(BG.CheckShearForcesElastic(res, VetSoll));
        }

        [TestMethod]
        public void Test04_EqualCoordinateSystem()
        {
            var BarSys1 = new CoordinateSystem(new Point3d(2, 3, 4), Vector3d.XAxis, Vector3d.YAxis);
            var BarSys2 = new CoordinateSystem(new Point3d(2, 3, 4), Vector3d.XAxis, Vector3d.YAxis);
            var BarSys3 = new CoordinateSystem(new Point3d(2, 3.1, 4), Vector3d.XAxis, Vector3d.YAxis);
            var newX = Vector3d.XAxis.CrossProduct(new Vector3d(0, 0, 0.1));
            var newY = Vector3d.YAxis.CrossProduct(new Vector3d(0, 0, 0.1));
            var BarSys4 = new CoordinateSystem(new Point3d(2, 3, 4), newX, newY);

            double Angle = 0.1;
            var newX2 = new Vector3d(Math.Cos(Angle), Math.Sin(Angle), 0.0);
            var newY2 = new Vector3d(-Math.Sin(Angle), Math.Cos(Angle), 0.0);
            var BarSys5 = new CoordinateSystem(new Point3d(2, 3, 4), newX2, newY2);

            Assert.IsTrue(BarSys1 == BarSys2);
            Assert.IsTrue(BarSys1 != BarSys3);
            Assert.IsTrue(BarSys1 != BarSys4);
            Assert.IsTrue(BarSys1 != BarSys5);
        }

        [TestMethod]
        public void Test05_ForceTranslationInX()
        {
            var BarSys = new CoordinateSystem(new Point3d(10, 5, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, 5000, 4000, 100, 0, 0, BarSys);
            // Destination
            var BarSys2 = new CoordinateSystem(new Point3d(15, 5, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll2 = VetSoll.ToCoordinateSystemWithEccentricity(BarSys2);
            // Solution
            var VetSoll2_result = new ResultBeamForces(0, 5000, 4000, 100 - 4000 * 5, 0, 0, BarSys);
            // Check
            Assert.AreEqual(VetSoll2.T, VetSoll2_result.T);
        }

        [TestMethod]
        public void Test06_ForceTranslationInXAndY()
        {
            var BarSys = new CoordinateSystem(new Point3d(10, 5, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, 5000, 4000, 100, 0, 0, BarSys);
            // Destination
            var BarSys2 = new CoordinateSystem(new Point3d(15, 12, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll2 = VetSoll.ToCoordinateSystemWithEccentricity(BarSys2);
            // Solution
            var VetSoll2_result = new ResultBeamForces(0, 5000, 4000, 100 - 4000 * 5 + 5000 * 7, 0, 0, BarSys);
            // Check
            Assert.AreEqual(VetSoll2.T, VetSoll2_result.T);
        }

        [TestMethod]
        public void Test07_ForcesSum()
        {
            // Force A
            var BarSysA = new CoordinateSystem(new Point3d(10, 5, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSollA = new ResultBeamForces(0, 5000, 4000, 100, 0, 0, BarSysA);
            // Force B
            var BarSysB = new CoordinateSystem(new Point3d(15, 5, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSollB = new ResultBeamForces(0, 5000, 4000, 100, 0, 0, BarSysB);
            // Sum
            var VetSollSum = VetSollA + VetSollB;
            // Solution
            var VetSollSum_result = new ResultBeamForces(0, 10000, 8000, 200 + 4000 * 5, 0, 0, BarSysA);
            // Check
            Assert.AreEqual(VetSollSum.T, VetSollSum_result.T);
            Assert.AreEqual(VetSollSum.V1, VetSollSum_result.V1);
            Assert.AreEqual(VetSollSum.V2, VetSollSum_result.V2);
        }

        [TestMethod]
        public void Test08_ForceCalculation()
        {
            var BG = new BoltGrid(new double[] { }, new double[] { 200 }, 12);
            var AppSys = new CoordinateSystem(new Point3d(50, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, 0, -800, 0, 0, 0, AppSys);
            var res = BG.CalculateShearForcesElastic(VetSoll);

            // Solution
            Assert.IsTrue(BG.CheckShearForcesElastic(res, VetSoll));
        }

        [TestMethod]
        public void Test09_ForceCalculation()
        {
            var BG = new BoltGrid(new double[] { 200 }, new double[] { }, 12);
            var AppSys = new CoordinateSystem(new Point3d(0, 50, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, -800, 0, 0, 0, 0, AppSys);
            var res = BG.CalculateShearForcesElastic(VetSoll);

            // Solution
            Assert.IsTrue(BG.CheckShearForcesElastic(res, VetSoll));
        }

        [TestMethod]
        public void Test10_ForceCalculation()
        {
            var BG = new BoltGrid(new double[] { 200 }, new double[] { 120, 120 }, 12);
            var AppSys = new CoordinateSystem(new Point3d(0, 50, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, -800, 300, 50000, 0, 0, AppSys);
            var res = BG.CalculateShearForcesElastic(VetSoll);

            // Solution
            Assert.IsTrue(BG.CheckShearForcesElastic(res, VetSoll));
        }
    }
}
