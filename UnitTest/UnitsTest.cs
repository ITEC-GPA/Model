using GPC.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitSystemTest
{
    [TestClass]
    public class UnitsTest
    {
        [TestMethod]
        public void ConvertLength()
        {
            double dist = 10; // m

            double mm = dist.ConvertLengthToDefault(Units.Knm);
            Assert.IsTrue(mm == 10000.0);

            double m = mm.ConvertLengthFromDefault(Units.Knm);
            Assert.IsTrue(m == dist);
        }

        [TestMethod]
        public void ConvertForce()
        {
            double force = 10; // kN

            double N = force.ConvertForceToDefault(Units.Knm);
            Assert.IsTrue(N == 10000);

            double kN = N.ConvertForceFromDefault(Units.Knm);
            Assert.IsTrue(kN == force);
        }


        [TestMethod]
        public void ConvertMoment()
        {
            double moment = 10; // kNm

            double N = moment.ConverMomentToDefault(Units.Knm);
            Assert.IsTrue(N == 10 * 1000 * 1000);

            double kN = N.ConverMomentFromDefault(Units.Knm);
            Assert.IsTrue(kN == moment);
        }


        [TestMethod]
        public void ConvertDensity()
        {
            double density = 200.0; // kg/m3

            double D = density.ConvertDensityToDefault(Units.SI); // T/mm^3
            Assert.AreEqual(D, density / 1000.0 / 1E9, 0.0000001, D.ToString());

            double sI = D.ConvertDensityFromDefault(Units.SI);
            Assert.AreEqual(sI, density, 0.0000001, sI.ToString());
        }


        [TestMethod]
        public void EqualityTest()
        {
            UnitsSystem units = Units.Knm;
            Assert.IsFalse(units == Units.DefaultUnits);
            Assert.IsTrue(Units.Nmm == Units.DefaultUnits);
        }
    }
}
