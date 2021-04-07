using GPC.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using GPC.TestUtilities;

namespace UnitSystemTest
{
    [TestClass]
    public class UnitsTest  : UnitTestBase
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
        public void EqualityTest()
        {
            UnitsSystem units = Units.Knm;
            Assert.IsFalse(units == Units.DefaultUnits);
            Assert.IsTrue(Units.Nmm == Units.DefaultUnits);
        }
    }
}
