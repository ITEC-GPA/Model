using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GPC.Model.FEM.Collections;
using GPC.Model.FEM;
using GPC.Model.FEM.FiniteElements;
using GPC.Geometry;
using GPC.TestUtilities;
using System.Collections.Generic;

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
            Node n3 = new Node(new Point3d(0, 1, 2), 1);

            cnode.Add(n1);
            cnode.Add(n2);
            cnode.Add(n3);

            Assert.IsTrue(cnode.Count == 2);

            Assert.IsTrue(cnode.Contains(n3));

            Assert.IsTrue(cnode.GetElementById(1).Id == 1);
            Assert.IsTrue(cnode.GetElementById(2).Id == 2);

            Assert.AreEqual(cnode.GetElementById(1).Position, Point3d.Origin);
            Assert.AreEqual(cnode.GetElementById(2).Position, new Point3d(0, 1, 2));

        }


        [TestMethod]
        public void FemObjectCollectionTest2()
        {
            FemObjectCollection<FiniteElement> cfe = new FemObjectCollection<FiniteElement>();

            Plate p1 = new Plate(new Node[] { new Node(new Point3d(0, 1, 2), 1), new Node(new Point3d(1, 1, 2), 2), new Node(new Point3d(2, 1, 2), 3), new Node(new Point3d(3, 1, 2), 4) } );
            p1.SetId(1);

            Plate p2 = new Plate(new Node[] { new Node(new Point3d(0, 2, 2), 1), new Node(new Point3d(1, 2, 2), 2), new Node(new Point3d(2, 3, 2), 3), new Node(new Point3d(3, 4, 2), 4) });
            p2.SetId(1);

            Plate p3 = new Plate(new Node[] { new Node(new Point3d(0, 1, 2), 1), new Node(new Point3d(1, 1, 2), 2), new Node(new Point3d(2, 1, 2), 3), new Node(new Point3d(3, 1, 2), 4) });
            p3.SetId(1);

            cfe.Add(p1);
            cfe.Add(p2);
            cfe.Add(p3);

            Assert.IsTrue(cfe.Count == 2, cfe.Count.ToString());

            Assert.IsTrue(cfe.GetElementById(1).Id == 1);
            Assert.IsTrue(cfe.GetElementById(2).Id == 2);

            Assert.AreEqual(cfe.GetElementById(1).Nodes[0].Position, new Point3d(0, 1, 2));
            Assert.AreEqual(cfe.GetElementById(2).Nodes[0].Position, new Point3d(0, 2, 2));

        }
    }
}
