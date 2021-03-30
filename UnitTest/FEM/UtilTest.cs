using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.FEM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FemTest.Solver
{
    [TestClass]
    public class UtilTest
    {
        public double F(double x, double y, double z)
        {
            return x + y + z;
        }

        [TestMethod]
        public void Test1()
        {
            Func<double, double, double, double> F3 = F;

            Assert.AreEqual(3, F3.FirstFix(1)(1, 1));
            Assert.AreEqual(6, F3.FirstFix(1)(2, 3));

            Assert.AreEqual(3, Util.FFirstFix(1, F3)(1, 1));
            Assert.AreEqual(6, Util.FFirstFix(1, F3)(2, 3));
        }
    }
}
