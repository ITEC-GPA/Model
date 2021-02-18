using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM;
using mnl = MathNet.Numerics.LinearAlgebra;

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

            bool v = el0.Equals(el1);

            Assert.IsFalse(el0.Equals(el1));
        }

        [TestMethod]
        public void TriangularMembranalKTest1()
        {
            Node[] nds = new Node[3];
            nds[0] = new Node(0, 0, 0, 1, "1");
            nds[1] = new Node(0, 100, 0, 2, "2");
            nds[2] = new Node(100, 0, 0, 3, "3");

            TriangularMembranal el = new TriangularMembranal(nds,1);
            el.BuildMatrix();
            mnl.Matrix<double> kLocal = el.KElementLocalCoord;
            //Add DZ global DOF
            kLocal = kLocal.InsertColumn(kLocal.ColumnCount, mnl.Vector<double>.Build.Dense(kLocal.RowCount));
            kLocal = kLocal.InsertRow(kLocal.RowCount, mnl.Vector<double>.Build.Dense(kLocal.ColumnCount));

            kLocal = kLocal.InsertColumn(4, mnl.Vector<double>.Build.Dense(kLocal.RowCount));
            kLocal = kLocal.InsertRow(4, mnl.Vector<double>.Build.Dense(kLocal.ColumnCount));

            kLocal = kLocal.InsertColumn(2, mnl.Vector<double>.Build.Dense(kLocal.RowCount));
            kLocal = kLocal.InsertRow(2, mnl.Vector<double>.Build.Dense(kLocal.ColumnCount));

            mnl.Matrix<double> kGlobal = el.DofGlobalToLocal.Transpose() *  el.KElementLocalCoord * el.DofGlobalToLocal;
            Assert.AreEqual(kLocal, kGlobal, "kLocal not equal to Kglobal with local axis");
        }
    }
}