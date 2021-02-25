using GPC.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTest
{
    [TestClass]
    public class UnitsTest
    {
        [TestMethod]
        public void ConvertLength()
        {
            double dist = 10; // m

            double mm = dist.ConvertLengthToNmm(Units.Knm);
            Assert.IsTrue(mm == 10000.0);

            double m = mm.ConvertLengthFromNmm(Units.Knm);
            Assert.IsTrue(m == dist);
        }

        [TestMethod]
        public void ConvertForce()
        {
            double force = 10; // kN

            double N = force.ConvertForceToNmm(Units.Knm);
            Assert.IsTrue(N == 10000);

            double kN = N.ConvertForceFromNmm(Units.Knm);
            Assert.IsTrue(kN == force);
        }
    }
}
