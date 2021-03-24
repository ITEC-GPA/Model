using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GPC.Model.FEM.Collections;
using GPC.Model.FEM;
using GPC.Model.FEM.FiniteElements;
using GPC.Geometry;
using GPC.TestUtilities;

namespace FemTest
{
    [TestClass]
    public class FemObjectCollectionsTest : UnitTestBase
    {

        [TestMethod]
        public void FemObjectCollectionTest1()
        {
            FemObjectCollection<Node> cnode = new FemObjectCollection<Node>();

            Node n1 = new Node(Point3d.Origin, 1);
            Node n2 = new Node(Point3d.Origin, 2);
            Node n3 = new Node(new Point3d(0, 1, 2), 2);

            cnode.Add(n1);
            cnode.Add(n2);
            cnode.Add(n3);

            Assert.IsTrue(cnode.Count == 3);

            Assert.IsTrue(cnode.Contains(n3));

            Assert.IsTrue(cnode.GetElementById(2).Id == 2);

            Assert.IsTrue(cnode.GetElementById(3).Position.Equals(new Point3d(0, 1, 2)));
        }
    }
}
