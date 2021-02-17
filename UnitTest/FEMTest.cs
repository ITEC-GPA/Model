using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM;

namespace UnitTest
{
    [TestClass]
    public class FEMTest
    {
        [TestMethod]
        public void EqualsNodesTest1() {
            //Node in same place with different ID
            Node n1 = new Node(0, 0, 0, 1);
            Node n2 = new Node(0, 0, 0, 2);
            Assert.IsFalse(n1.Equals(n2));
        }

        [TestMethod]
        public void EqualsFiniteElementTest1()
        {
            //Node in same place with different ID
            Node[] nodes = new Node[3];
            nodes[0] = new Node(0, 0, 0, 1);
            nodes[1] = new Node(1, 0, 0, 2);
            nodes[2] = new Node(0, 1, 0, 3);

            FiniteElement el0 = new TriangularMembranal(nodes, 0);
            FiniteElement el1 = new TriangularMembranal(nodes, 1);

            Assert.IsFalse(el0.Equals(el1));
        }
    }
}