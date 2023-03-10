using GPC.Geometry;
using GPC.Model.Materials;
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
            var Mat = new SteelMaterial("10.9", 200000, 700, 1000, 0.3, SteelMaterial.SteelTypes.Structural);

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
            var Mat = new SteelMaterial("10.9", 200000, 700, 1000, 0.3, SteelMaterial.SteelTypes.Structural);
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
            var BG = new BoltGrid();
            var BarSys = new CoordinateSystem(BG.CalculateBarycenter(), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, 5000, 4000, 0, 0, 0, BarSys);
            var res = BG.CalculateShearForcesElastic(VetSoll);

            // Solution
            var SolBeam = new ResultBeamForces(0, 5000.0 / 12.0, 4000.0 / 12.0, 0, 0, 0, BarSys);
            for (int i = 0; i < 12; i++)
            {
                Assert.IsTrue(Error.AreEqualsDouble(res[i + 1].V1, SolBeam.V1));
                Assert.IsTrue(Error.AreEqualsDouble(res[i + 1].V2, SolBeam.V2));
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
    }
}
