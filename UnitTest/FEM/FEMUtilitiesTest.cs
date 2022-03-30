using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem;
using GPC.Utilities.Fem;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FemTest.SolverTest
{
    [TestClass]
    public class FemUtilitiesTest
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

            Assert.AreEqual(3, FemUtilities.FFirstFix(1, F3)(1, 1));
            Assert.AreEqual(6, FemUtilities.FFirstFix(1, F3)(2, 3));
        }

        [TestMethod]
        public void Test2()
        {
            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, "1");
            nds[1] = new Node(+1.0, -1.0, 0, "2");
            nds[2] = new Node(+1.0, +1.0, 0, "3");
            nds[3] = new Node(-1.0, +1.0, 0, "4");

            Func<double, double, Node[], double> X = (double csi, double eta, Node[] nodi) =>
            {
                double x = 0;
                for (int i = 1; i <= nds.Length; i++)
                {
                    x = x + LinearShapeFunctionQuad4.NaturalShapeFunction(i, csi, eta) * nodi[i - 1].Position.X;
                }
                return x;
            };

            Assert.AreEqual(X(-1,-1,nds), FemUtilities.GetLocalCoordinate2D("X",-1, -1, LinearShapeFunctionQuad4.NaturalShapeFunction, nds));
        }
    }
}
