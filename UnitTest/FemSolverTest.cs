using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.Elements;
using GPC.Model.Materials;
using GPC.Model.FreedomCases;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using GPC.Model.LoadCases;

namespace FemTest
{
    [TestClass]
    public class FemSolverTest
    {
        [TestMethod]
        public void EqualsNodesTest1()
        {
            //Node in same place with different ID
            Node n1 = new Node(0, 0, 0, 1);
            Node n2 = new Node(0, 0, 0, 2);

            //controllo equals nodi
            Assert.IsFalse(n1.Equals(n2));
        }

        [TestMethod]
        public void EqualsFiniteElementTest1()
        {
            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat,0,1);

            //Node in same place with different ID
            Node[] nodes = new Node[3];
            nodes[0] = new Node(0, 0, 0, 1);
            nodes[1] = new Node(1, 0, 0, 2);
            nodes[2] = new Node(0, 1, 0, 3);

            FiniteElement el0 = new TriangularMembranal(nodes, prop, 0);
            FiniteElement el1 = new TriangularMembranal(nodes, prop, 1);

            //Controllo equals elementi
            Assert.IsFalse(el0.Equals(el1));
        }

        [TestMethod]
        public void AlwaysOrderedGDL()
        {
            SortedSet<LinearSolver.DOF> unordered = new SortedSet<LinearSolver.DOF>();

            unordered.Add(LinearSolver.DOF.RY);
            unordered.Add(LinearSolver.DOF.RX);
            unordered.Add(LinearSolver.DOF.RZ);
            unordered.Add(LinearSolver.DOF.DX);
            unordered.Add(LinearSolver.DOF.DY);

            var order = unordered;

            Assert.IsTrue(order.ElementAt(0) == LinearSolver.DOF.DX);
            Assert.IsTrue(order.ElementAt(1) == LinearSolver.DOF.DY);
            Assert.IsTrue(order.ElementAt(2) == LinearSolver.DOF.RX);
            Assert.IsTrue(order.ElementAt(3) == LinearSolver.DOF.RY);
            Assert.IsTrue(order.ElementAt(4) == LinearSolver.DOF.RZ);

            /*SortedSet<int> unordered2 = new SortedSet<int>();
            unordered2.Add(5);
            unordered2.Add(105);
            unordered2.Add(7);
            unordered2.Add(4);*/
        }

        [TestMethod]
        public void TriangularMembranalKTest1()
        {
            Material mat = new SteelMaterial("steel", 200000.0, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            Node[] nds = new Node[3];
            nds[0] = new Node(0, 0, 0, 1, "1");
            nds[1] = new Node(1, 0, 0, 2, "2");
            nds[2] = new Node(0, 1, 0, 3, "3");

            TriangularMembranal el = new TriangularMembranal(nds, prop, 1);
            el.BuildMatrix();
            mnl.Matrix<double> kLocal = el.KElementLocalCoord;
            mnl.Matrix<double> kLocalManual = mnl.Matrix<double>.Build.Dense(0, 6);
            double[] r0 = new double[] { 145833,  62500, - 41667, - 20833, - 104167, - 41667 };
            double[] r1 = new double[] { 62500,   145833, - 41667, - 104167, - 20833, - 41667 };
            double[] r2 = new double[] { -41667, - 41667, 41667,   0,   0,   41667 };
            double[] r3 = new double[] { -20833, - 104167, 0,   104167,  20833,   0 };
            double[] r4 = new double[] { -104167, - 20833,  0,   20833,   104167,  0 };
            double[] r5 = new double[] { -41667, - 41667,  41667,   0,   0,   41667 };

            kLocalManual = kLocalManual.InsertRow(0, mnl.Vector<double>.Build.Dense(r0));
            kLocalManual = kLocalManual.InsertRow(1, mnl.Vector<double>.Build.Dense(r1));
            kLocalManual = kLocalManual.InsertRow(2, mnl.Vector<double>.Build.Dense(r2));
            kLocalManual = kLocalManual.InsertRow(3, mnl.Vector<double>.Build.Dense(r3));
            kLocalManual = kLocalManual.InsertRow(4, mnl.Vector<double>.Build.Dense(r4));
            kLocalManual = kLocalManual.InsertRow(5, mnl.Vector<double>.Build.Dense(r5));

            //controllo klocale elemento finito 3 nodi stato piano di tensione
            for (int i = 0; i < kLocal.RowCount; i++)
            {
                for (int j = 0; j < kLocal.ColumnCount; j++)
                {
                    Assert.AreEqual(kLocal[i,j] - kLocalManual[i,j], 0, 1, "kLocal no OK -> row " + i + " col " + j );
                    //sarebbe stato meglio usare kLocal[i,j] / kLocalManual[i,j] ma 0/0 = NaN!!
                }
            }
        }

        [TestMethod]
        public void TriangularMembranalKTest2()
        {
            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            Node[] nds = new Node[3];
            nds[0] = new Node(0, 0, 0, 1, "1");
            nds[1] = new Node(0, 100, 0, 2, "2");
            nds[2] = new Node(100, 0, 0, 3, "3");

            TriangularMembranal el = new TriangularMembranal(nds, prop, 1);
            el.BuildMatrix();
            mnl.Matrix<double> kLocal = el.KElementLocalCoord;
            //Add DZ global DOF
            kLocal = kLocal.InsertColumn(kLocal.ColumnCount, mnl.Vector<double>.Build.Dense(kLocal.RowCount));
            kLocal = kLocal.InsertRow(kLocal.RowCount, mnl.Vector<double>.Build.Dense(kLocal.ColumnCount));

            kLocal = kLocal.InsertColumn(4, mnl.Vector<double>.Build.Dense(kLocal.RowCount));
            kLocal = kLocal.InsertRow(4, mnl.Vector<double>.Build.Dense(kLocal.ColumnCount));

            kLocal = kLocal.InsertColumn(2, mnl.Vector<double>.Build.Dense(kLocal.RowCount));
            kLocal = kLocal.InsertRow(2, mnl.Vector<double>.Build.Dense(kLocal.ColumnCount));

            mnl.Matrix<double> kGlobal = el.DofGlobalToLocal.Transpose() * el.KElementLocalCoord * el.DofGlobalToLocal;
            //controllo che passaggio da coordinate locali a globali sia fatto corretamente
            Assert.AreEqual(kLocal, kGlobal, "kLocal not equal to Kglobal");
        }

        [TestMethod]
        public void AssemblyGlobalMatrixTest1()
        {
            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            List<Node> nodesPlate1 = new List<Node>();
            nodesPlate1.Add(new Node(0, 0, 0, 1, "1"));
            nodesPlate1.Add(new Node(0, 100, 0, 2, "2"));
            nodesPlate1.Add(new Node(100, 0, 0, 3, "3"));

            List<Node> nodesPlate2 = new List<Node>();
            nodesPlate2.Add(new Node(100, 0, 0, 2, "2"));
            nodesPlate2.Add(new Node(0, 100, 0, 3, "3"));
            nodesPlate2.Add(new Node(100, 100, 0, 4, "4"));

            List<FiniteElement> elements = new List<FiniteElement>();

            elements.Add(new TriangularMembranal(nodesPlate1.ToArray(), prop, 1));
            elements.Add(new TriangularMembranal(nodesPlate2.ToArray(), prop, 2));

            LinearSolver fem = new LinearSolver(elements.ToArray());
            mnl.Matrix<double> K = fem.KGlobal;

            mnl.Matrix<double> KManual = mnl.Matrix<double>.Build.Dense(0, fem.KGlobal.ColumnCount);
            double[] r0 = new double[] { 145833.3, 62500.0,0.0, -41666.7, -20833.3, 0.0, -104166.7, -41666.7, 0.0, 0.0, 0.0, 0.0 };
            double[] r1 = new double[] { 62500.0, 145833.3, 0.0, -41666.7, -104166.7, 0.0, -20833.3, -41666.7, 0.0, 0.0, 0.0, 0.0 };
            double[] r2 = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            double[] r3 = new double[] { -41666.7, -41666.7, 0.0, 145833.3, 0.0, 0.0, 0.0, 62500.0, 0.0, -104166.7, -20833.3, 0.0 };
            double[] r4 = new double[] { -20833.3, -104166.7, 0.0, 0.0, 145833.3, 0.0, 62500.0, 0.0, 0.0, -41666.7, -41666.7, 0.0 };
            double[] r5 = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            double[] r6 = new double[] { -104166.7, -20833.3, 0.0, 0.0, 62500.0, 0.0, 145833.3, 0.0, 0.0, -41666.7, -41666.7, 0.0 };
            double[] r7 = new double[] { -41666.7, -41666.7, 0.0, 62500.0, 0.0, 0.0, 0.0, 145833.3, 0.0, -20833.3, -104166.7, 0.0 };
            double[] r8 = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };
            double[] r9 = new double[] { 0.0, 0.0, 0.0, -104166.7, -41666.7, 0.0, -41666.7, -20833.3, 0.0, 145833.3, 62500.0, 0.0 };
            double[] r10 = new double[] { 0.0, 0.0, 0.0, -20833.3, -41666.7, 0.0, -41666.7, -104166.7, 0.0, 62500.0, 145833.3, 0.0 };
            double[] r11 = new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 };

            KManual = KManual.InsertRow(0, mnl.Vector<double>.Build.Dense(r0));
            KManual = KManual.InsertRow(1, mnl.Vector<double>.Build.Dense(r1));
            KManual = KManual.InsertRow(2, mnl.Vector<double>.Build.Dense(r2));
            KManual = KManual.InsertRow(3, mnl.Vector<double>.Build.Dense(r3));
            KManual = KManual.InsertRow(4, mnl.Vector<double>.Build.Dense(r4));
            KManual = KManual.InsertRow(5, mnl.Vector<double>.Build.Dense(r5));
            KManual = KManual.InsertRow(6, mnl.Vector<double>.Build.Dense(r6));
            KManual = KManual.InsertRow(7, mnl.Vector<double>.Build.Dense(r7));
            KManual = KManual.InsertRow(8, mnl.Vector<double>.Build.Dense(r8));
            KManual = KManual.InsertRow(9, mnl.Vector<double>.Build.Dense(r9));
            KManual = KManual.InsertRow(10, mnl.Vector<double>.Build.Dense(r10));
            KManual = KManual.InsertRow(11, mnl.Vector<double>.Build.Dense(r11));

            for (int i = 0; i < K.RowCount; i++)
            {
                for (int j = 0; j < K.ColumnCount; j++)
                {
                    Assert.AreEqual(K[i, j] - KManual[i, j], 0, 1, "KGlobal no OK -> row " + i + " col " + j);
                    //sarebbe stato meglio usare k[i,j] / kManual[i,j] ma 0/0 = NaN!!
                }
            }
        }

        [TestMethod]
        public void AddRestrainMatrixTest1()
        {
            FreedomCase fc = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);
            
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute DXDYDZ = new NodeRestrainAttribute(fc, sys);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DX);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DY);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute(fc, sys);
            DZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            List<Node> nodesPlate1 = new List<Node>();
            Node nd1 = new Node(0, 0, 0, 1, "1");
            Node nd2 = new Node(0, 100, 0, 2, "2");
            Node nd3 = new Node(100, 0, 0, 3, "3");

            nd1.AddAttribute(DXDYDZ);
            nd2.AddAttribute(DXDYDZ);

            nodesPlate1.Add(nd1);
            nodesPlate1.Add(nd2);
            nodesPlate1.Add(nd3);

            List<Node> nodesPlate2 = new List<Node>();
            Node nd2copy = new Node(0, 100, 0, 2, "2");
            Node nd3copy = new Node(100, 0, 0, 3, "3");
            Node nd4 = new Node(100, 100, 0, 4, "4");

            nd2copy.AddAttribute(DZ);
            nd3copy.AddAttribute(DZ);
            nd4.AddAttribute(DZ);

            nodesPlate2.Add(nd2copy);
            nodesPlate2.Add(nd3copy);
            nodesPlate2.Add(nd4);

            List<FiniteElement> elements = new List<FiniteElement>();
            elements.Add(new TriangularMembranal(nodesPlate1.ToArray(), prop, 1));
            elements.Add(new TriangularMembranal(nodesPlate2.ToArray(), prop, 2));

            LinearSolver fem = new LinearSolver(elements.ToArray());
            mnl.Matrix<double> K = fem.KGlobal;

            mnl.Matrix<double> KManual = mnl.Matrix<double>.Build.Dense(0, fem.KGlobal.ColumnCount);
            double[] r0 = new double[] { 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r1 = new double[] { 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r2 = new double[] { 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r3 = new double[] { 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r4 = new double[] { 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0 };
            double[] r5 = new double[] { 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0 };
            double[] r6 = new double[] { 0, 0, 0, 0, 0, 0, 145833, 0, 0, -41666.7, -41666.7, 0 };
            double[] r7 = new double[] { 0, 0, 0, 0, 0, 0, 0, 145833, 0, -20833.3, -104167, 0 };
            double[] r8 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0 };
            double[] r9 = new double[] { 0, 0, 0, 0, 0, 0, -41666.7, -20833.3, 0, 145833, 62500, 0 };
            double[] r10 = new double[] { 0, 0, 0, 0, 0, 0, -41666.7, -104167, 0, 62500, 145833, 0 };
            double[] r11 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1 };

            KManual = KManual.InsertRow(0, mnl.Vector<double>.Build.Dense(r0));
            KManual = KManual.InsertRow(1, mnl.Vector<double>.Build.Dense(r1));
            KManual = KManual.InsertRow(2, mnl.Vector<double>.Build.Dense(r2));
            KManual = KManual.InsertRow(3, mnl.Vector<double>.Build.Dense(r3));
            KManual = KManual.InsertRow(4, mnl.Vector<double>.Build.Dense(r4));
            KManual = KManual.InsertRow(5, mnl.Vector<double>.Build.Dense(r5));
            KManual = KManual.InsertRow(6, mnl.Vector<double>.Build.Dense(r6));
            KManual = KManual.InsertRow(7, mnl.Vector<double>.Build.Dense(r7));
            KManual = KManual.InsertRow(8, mnl.Vector<double>.Build.Dense(r8));
            KManual = KManual.InsertRow(9, mnl.Vector<double>.Build.Dense(r9));
            KManual = KManual.InsertRow(10, mnl.Vector<double>.Build.Dense(r10));
            KManual = KManual.InsertRow(11, mnl.Vector<double>.Build.Dense(r11));

            for (int i = 0; i < K.RowCount; i++)
            {
                for (int j = 0; j < K.ColumnCount; j++)
                {
                    Assert.AreEqual(K[i, j] - KManual[i, j], 0, 1, "KGlobal no OK -> row " + i + " col " + j);
                    //sarebbe stato meglio usare k[i,j] / kManual[i,j] ma 0/0 = NaN!!
                }
            }
        }

        [TestMethod]
        public void AddRestrainAndForceMatrixTest1()
        {
            LoadCase loadCase = new LoadCase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute DXDYDZ = new NodeRestrainAttribute(freedomCase, sys);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DX);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DY);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute(freedomCase, sys);
            DZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            NodeForceAttribute fX1000 = new NodeForceAttribute(loadCase, sys, 1000, 0, 0, 0, 0, 0);

            List<Node> nodesPlate1 = new List<Node>();
            Node nd1 = new Node(0, 0, 0, 1, "1");
            Node nd2 = new Node(0, 100, 0, 2, "2");
            Node nd3 = new Node(100, 0, 0, 3, "3");

            nd1.AddAttribute(DXDYDZ);
            nd2.AddAttribute(DXDYDZ);

            nodesPlate1.Add(nd1);
            nodesPlate1.Add(nd2);
            nodesPlate1.Add(nd3);

            List<Node> nodesPlate2 = new List<Node>();
            Node nd2copy = new Node(0, 100, 0, 2, "2");
            Node nd3copy = new Node(100, 0, 0, 3, "3");
            Node nd4 = new Node(100, 100, 0, 4, "4");

            nd2copy.AddAttribute(DZ);
            nd3copy.AddAttribute(DZ);
            nd4.AddAttribute(DZ);
            nd4.AddAttribute(fX1000);

            nodesPlate2.Add(nd2copy);
            nodesPlate2.Add(nd3copy);
            nodesPlate2.Add(nd4);

            List<FiniteElement> elements = new List<FiniteElement>();
            elements.Add(new TriangularMembranal(nodesPlate1.ToArray(), prop, 1));
            elements.Add(new TriangularMembranal(nodesPlate2.ToArray(), prop, 2));

            LinearSolver fem = new LinearSolver(elements.ToArray());
            double[] Node4DX = fem.GetDisplacementGlobalCoordinates(nd4, LinearSolver.DOF.DX);
            double[] Node4DY = fem.GetDisplacementGlobalCoordinates(nd4, LinearSolver.DOF.DY);

            double[] Node3DX = fem.GetDisplacementGlobalCoordinates(nd3, LinearSolver.DOF.DX);
            double[] Node3DY = fem.GetDisplacementGlobalCoordinates(nd3, LinearSolver.DOF.DY);

            double[] Node3CopyDX = fem.GetDisplacementGlobalCoordinates(nd3copy, LinearSolver.DOF.DX);
            double[] Node3CopyDY = fem.GetDisplacementGlobalCoordinates(nd3copy, LinearSolver.DOF.DY);

            /*Node 4 Displacement
            DX(mm) 0.009130
            DY(mm) - 0.005478
            DZ(mm) 0.000000*/
            double dXNode4 = 0.009130;
            double dYNode4 = -0.005478;
            Assert.AreEqual(dXNode4, Node4DX[0], 0.000001);
            Assert.AreEqual(dYNode4, Node4DY[0], 0.000001);

            /*Node 3 Displacement
            DX(mm) 0.001043
            DY(mm) - 0.002609
            DZ(mm) 0.000000*/
            double dXNode3 = 0.001043;
            double dYNode3 = -0.002609;
            Assert.AreEqual(Node3DX[0], dXNode3, 0.000001);
            Assert.AreEqual(Node3DY[0], dYNode3, 0.000001);
            Assert.AreEqual(Node3CopyDX[0], dXNode3, 0.000001);
            Assert.AreEqual(Node3CopyDY[0], dYNode3, 0.000001);
        }

        [TestMethod]
        public void AddRestrainAndForceMatrixTest2()
        {
            LoadCase loadCase = new LoadCase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute DXDYDZ = new NodeRestrainAttribute(freedomCase, sys);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DX);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DY);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute(freedomCase, sys);
            DZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            CoordinateSystem sys2 = new CoordinateSystem(new Point3d(1, 1, 0), new Point3d(2, 2, 0), new Point3d(0, 2, 0));
            NodeForceAttribute f1 = new NodeForceAttribute(loadCase, sys, 1000, 0, 0, 0, 0, 0);
            NodeForceAttribute f2 = new NodeForceAttribute(loadCase, sys2, 1000, -500, 0, 0, 0, 0);

            List<Node> nodesPlate1 = new List<Node>();
            Node nd1 = new Node(0, 0, 0, 1, "1");
            Node nd2 = new Node(0, 100, 0, 2, "2");
            Node nd3 = new Node(100, 0, 0, 3, "3");

            nd1.AddAttribute(DXDYDZ);
            nd2.AddAttribute(DXDYDZ);

            nodesPlate1.Add(nd1);
            nodesPlate1.Add(nd2);
            nodesPlate1.Add(nd3);

            List<Node> nodesPlate2 = new List<Node>();
            Node nd2copy = new Node(0, 100, 0, 2, "2");
            Node nd3copy = new Node(100, 0, 0, 3, "3");
            Node nd4 = new Node(100, 100, 0, 4, "4");

            nd2copy.AddAttribute(DZ);
            nd3copy.AddAttribute(DZ);
            nd3copy.AddAttribute(f2);
            nd4.AddAttribute(DZ);
            nd4.AddAttribute(f1);

            nodesPlate2.Add(nd2copy);
            nodesPlate2.Add(nd3copy);
            nodesPlate2.Add(nd4);

            List<FiniteElement> elements = new List<FiniteElement>();
            elements.Add(new TriangularMembranal(nodesPlate1.ToArray(), prop, 1));
            elements.Add(new TriangularMembranal(nodesPlate2.ToArray(), prop, 2));

            LinearSolver fem = new LinearSolver(elements.ToArray());
            double[] Node4DX = fem.GetDisplacementGlobalCoordinates(nd4, LinearSolver.DOF.DX);
            double[] Node4DY = fem.GetDisplacementGlobalCoordinates(nd4, LinearSolver.DOF.DY);

            double[] Node3DX = fem.GetDisplacementGlobalCoordinates(nd3, LinearSolver.DOF.DX);
            double[] Node3DY = fem.GetDisplacementGlobalCoordinates(nd3, LinearSolver.DOF.DY);

            double[] Node3CopyDX = fem.GetDisplacementGlobalCoordinates(nd3copy, LinearSolver.DOF.DX);
            double[] Node3CopyDY = fem.GetDisplacementGlobalCoordinates(nd3copy, LinearSolver.DOF.DY);

            /*Node 4 Displacement
            DX (mm)	0.009315	
            DY (mm)	0.003745	
            DZ(mm) 0.000000*/
            double dXNode4 = 0.009315;
            double dYNode4 = 0.003745;
            Assert.AreEqual(Node4DX[0], dXNode4, 0.000001);
            Assert.AreEqual(Node4DY[0], dYNode4, 0.000001);

            /*Node 3 Displacement
            DX (mm)	0.011004	
            DY (mm)	0.006430
            DZ(mm) 0.000000*/
            double dXNode3 = 0.011004;
            double dYNode3 = 0.00643;
            Assert.AreEqual(Node3DX[0], dXNode3, 0.000001);
            Assert.AreEqual(Node3DY[0], dYNode3, 0.000001);
            Assert.AreEqual(Node3CopyDX[0], dXNode3, 0.000001);
            Assert.AreEqual(Node3CopyDY[0], dYNode3, 0.000001);
        }

        [TestMethod]
        public void PlatePressureTest1()
        {
            LoadCase loadCase = new LoadCase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute DXDYDZ = new NodeRestrainAttribute(freedomCase, sys);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DX);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DY);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute(freedomCase, sys);
            DZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            //CoordinateSystem sys2 = new CoordinateSystem(new Point3d(1, 1, 0), new Point3d(2, 2, 0), new Point3d(0, 2, 0));

            List<Node> nodesPlate1 = new List<Node>();
            Node nd1 = new Node(0, 0, 0, 1, "1");
            Node nd2 = new Node(0, 1, 0, 2, "2");
            Node nd3 = new Node(1, 0, 0, 3, "3");

            nd1.AddAttribute(DXDYDZ);
            nd2.AddAttribute(DXDYDZ);

            nodesPlate1.Add(nd1);
            nodesPlate1.Add(nd2);
            nodesPlate1.Add(nd3);

            List<Node> nodesPlate2 = new List<Node>();
            Node nd2copy = new Node(0, 1, 0, 2, "2");
            Node nd3copy = new Node(1, 0, 0, 3, "3");
            Node nd4 = new Node(1, 1, 0, 4, "4");

            nd2copy.AddAttribute(DZ);
            nd3copy.AddAttribute(DZ);
            nd4.AddAttribute(DZ);

            nodesPlate2.Add(nd2copy);
            nodesPlate2.Add(nd3copy);
            nodesPlate2.Add(nd4);

            List<FiniteElement> elements = new List<FiniteElement>();
            Plate e0 = new TriangularMembranal(nodesPlate1.ToArray(), prop, 1);
            PlatePressureAttribute p = new PlatePressureAttribute(loadCase, sys, -10.0, 0, 0);
            e0.AddAttribute(p);

            elements.Add(e0);
            elements.Add(new TriangularMembranal(nodesPlate2.ToArray(), prop, 2));

            LinearSolver fem = new LinearSolver(elements.ToArray());
            double[] Node4DX = fem.GetDisplacementGlobalCoordinates(nd4, LinearSolver.DOF.DX);
            double[] Node4DY = fem.GetDisplacementGlobalCoordinates(nd4, LinearSolver.DOF.DY);

            double[] Node3DX = fem.GetDisplacementGlobalCoordinates(nd3, LinearSolver.DOF.DX);
            double[] Node3DY = fem.GetDisplacementGlobalCoordinates(nd3, LinearSolver.DOF.DY);

            double[] Node3CopyDX = fem.GetDisplacementGlobalCoordinates(nd3copy, LinearSolver.DOF.DX);
            double[] Node3CopyDY = fem.GetDisplacementGlobalCoordinates(nd3copy, LinearSolver.DOF.DY);

            /*Node 4 Displacement
            DX (mm)	-0.000002	
            DY (mm)	-0.000007	
            DZ(mm) 0.000000*/
            double dXNode4 = -0.000002;
            double dYNode4 = -0.000007;
            Assert.AreEqual(Node4DX[0], dXNode4, 0.000001);
            Assert.AreEqual(Node4DY[0], dYNode4, 0.000001);

            /*Node 3 Displacement
            DX (mm)	-0.000014	
            DY (mm)	-0.000005
            DZ(mm) 0.000000*/
            double dXNode3 = -0.000014;
            double dYNode3 = -0.000005;
            Assert.AreEqual(Node3DX[0], dXNode3, 0.000001);
            Assert.AreEqual(Node3DY[0], dYNode3, 0.000001);
            Assert.AreEqual(Node3CopyDX[0], dXNode3, 0.000001);
            Assert.AreEqual(Node3CopyDY[0], dYNode3, 0.000001);
        }

        [TestMethod]
        public void TriangleDKTTest1()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0);

            Node[] nodesPlate1 = new Node[3];
            nodesPlate1[0] = new Node(0, 0, 0, 1, "1");
            nodesPlate1[1] = new Node(1, 0, 0, 2, "2");
            nodesPlate1[2] = new Node(0, 1, 0, 3, "3");

            FiniteElement e0 = new TriangularDK(nodesPlate1, prop, 1);
            e0.BuildMatrix();

            mnl.Matrix<double> SAPkMatrix = mnl.Matrix<double>.Build.Dense(0,9);
            
            mnl.Vector<double> r0 = mnl.Vector<double>.Build.Dense(new double[] { 10.50, 1.63, -1.63, -5.25, 1.00, -2.63, -5.25, 2.63, -1.00 });
            mnl.Vector<double> r1 = mnl.Vector<double>.Build.Dense(new double[] { 1.63, 1.38, 0.06, 0.44, 0.16, 0.22, - 2.06, 0.53, 0.16 });
            mnl.Vector<double> r2 = mnl.Vector<double>.Build.Dense(new double[] { -1.63, 0.06, 1.38, 2.06, 0.16, 0.53, - 0.44, 0.22, 0.16 });
            mnl.Vector<double> r3 = mnl.Vector<double>.Build.Dense(new double[] { -5.25, 0.44, 2.06, 5.63, 0.25, 2.31, - 0.37, - 0.31, 1.25 });
            mnl.Vector<double> r4 = mnl.Vector<double>.Build.Dense(new double[] { 1.00, 0.16, 0.16, 0.25, 0.72, - 0.13, - 1.25, 0.38, 0.22 });
            mnl.Vector<double> r5 = mnl.Vector<double>.Build.Dense(new double[] { -2.63, 0.22, 0.53, 2.31, - 0.13, 1.41, 0.31, - 0.41, 0.38 });
            mnl.Vector<double> r6 = mnl.Vector<double>.Build.Dense(new double[] { -5.25, - 2.06, - 0.44, - 0.37, - 1.25, 0.31, 5.63, - 2.31, - 0.25 });
            mnl.Vector<double> r7 = mnl.Vector<double>.Build.Dense(new double[] { 2.63, 0.53, 0.22, - 0.31, 0.38, - 0.41, - 2.31, 1.41, - 0.13 });
            mnl.Vector<double> r8 = mnl.Vector<double>.Build.Dense(new double[] { -1.00, 0.16, 0.16, 1.25, 0.22, 0.38, - 0.25, - 0.13, 0.72 });

            SAPkMatrix = SAPkMatrix.InsertRow(0, r0);
            SAPkMatrix = SAPkMatrix.InsertRow(1, r1);
            SAPkMatrix = SAPkMatrix.InsertRow(2, r2);
            SAPkMatrix = SAPkMatrix.InsertRow(3, r3);
            SAPkMatrix = SAPkMatrix.InsertRow(4, r4);
            SAPkMatrix = SAPkMatrix.InsertRow(5, r5);
            SAPkMatrix = SAPkMatrix.InsertRow(6, r6);
            SAPkMatrix = SAPkMatrix.InsertRow(7, r7);
            SAPkMatrix = SAPkMatrix.InsertRow(8, r8);

            Console.WriteLine("Element local stiffness matrix");
            for (int r = 0; r < e0.KElementLocalCoord.RowCount; r++)
            {
                for (int c = 0; c < e0.KElementLocalCoord.ColumnCount; c++)
                {
                    Assert.AreEqual(e0.KElementLocalCoord[r, c] - SAPkMatrix[r, c], 0.0, 0.01);
                    //Console.Write(e0.KElementLocalCoord[r,c].ToString("F2") + " ");    
                }
                //Console.WriteLine();
            }
            
        }

        [TestMethod]
        public void TriangleDKTTest2()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0);

            Node[] nodesPlate1 = new Node[3];
            nodesPlate1[0] = new Node(0, 0, 0, 1, "1");
            nodesPlate1[1] = new Node(1, 0, 0, 2, "2");
            nodesPlate1[2] = new Node(0, 1, 0, 3, "3");

            mnl.Matrix<double> SAPkMatrix = mnl.Matrix<double>.Build.Dense(0, 18);
            mnl.Vector<double>[] row = new mnl.Vector<double>[18];

            row[0] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            row[1] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            row[2] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 10.5, 1.63, -1.63, 0, 0, 0, -5.25, 1, -2.63, 0, 0, 0, -5.25, 2.63, -1, 0 });
            row[3] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 1.63, 1.38, 0.06, 0, 0, 0, 0.44, 0.16, 0.22, 0, 0, 0, -2.06, 0.53, 0.16, 0 });
            row[4] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, -1.63, 0.06, 1.38, 0, 0, 0, 2.06, 0.16, 0.53, 0, 0, 0, -0.44, 0.22, 0.16, 0 });
            row[5] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            row[6] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            row[7] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            row[8] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, -5.25, 0.44, 2.06, 0, 0, 0, 5.63, 0.25, 2.31, 0, 0, 0, -0.37, -0.31, 1.25, 0 });
            row[9] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 1, 0.16, 0.16, 0, 0, 0, 0.25, 0.72, -0.13, 0, 0, 0, -1.25, 0.38, 0.22, 0 });
            row[10] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, -2.63, 0.22, 0.53, 0, 0, 0, 2.31, -0.13, 1.41, 0, 0, 0, 0.31, -0.41, 0.38, 0 });
            row[11] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            row[12] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            row[13] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });
            row[14] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, -5.25, -2.06, -0.44, 0, 0, 0, -0.37, -1.25, 0.31, 0, 0, 0, 5.63, -2.31, -0.25, 0 });
            row[15] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 2.63, 0.53, 0.22, 0, 0, 0, -0.31, 0.38, -0.41, 0, 0, 0, -2.31, 1.41, -0.13, 0 });
            row[16] = mnl.Vector<double>.Build.Dense(new double[] { 0, 0, -1, 0.16, 0.16, 0, 0, 0, 1.25, 0.22, 0.38, 0, 0, 0, -0.25, -0.13, 0.72, 0 });
            row[17] = mnl.Vector<double>.Build.Dense(new double[] { 0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0 });

            for (int i = 0; i < 18; i++)
            {
                SAPkMatrix = SAPkMatrix.InsertRow(i, row[i]);
            }

            FiniteElement e0 = new TriangularDK(nodesPlate1, prop, 1);
            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0 });
            
            Console.WriteLine("Element Global stiffness matrix");
            for (int r = 0; r < fem.KGlobal.RowCount; r++)
            {
                for (int c = 0; c < fem.KGlobal.ColumnCount; c++)
                {
                    Assert.AreEqual(fem.KGlobal[r, c] - SAPkMatrix[r, c], 0.0, 0.01, "error in position " + r +" "+ c);
                    //Console.Write(fem.KGlobal[r, c].ToString("F2") + " ");
                }
                //Console.WriteLine();
            }
        }

        /// <summary>
        /// Example PatchTest in "A study of three-node trinagular plate bending elements - batoz (1980)
        /// international journal for numerical methods in engineering, vol. 15 - 1771-1812 -> pg. 1797
        /// </summary>
        [TestMethod]
        public void TriangleDKTTest3()
        {
            LoadCase loadCase = new LoadCase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("mat", 10000, 0.3, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0);

            #region restrains
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute fixDXDYDZRZ = new NodeRestrainAttribute(freedomCase, sys);
            fixDXDYDZRZ.AddExternalRestrain(LinearSolver.DOF.DX);
            fixDXDYDZRZ.AddExternalRestrain(LinearSolver.DOF.DY);
            fixDXDYDZRZ.AddExternalRestrain(LinearSolver.DOF.DZ);
            fixDXDYDZRZ.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute fixDXDYRZ = new NodeRestrainAttribute(freedomCase, sys);
            fixDXDYRZ.AddExternalRestrain(LinearSolver.DOF.DX);
            fixDXDYRZ.AddExternalRestrain(LinearSolver.DOF.DY);
            fixDXDYRZ.AddExternalRestrain(LinearSolver.DOF.RZ);
            #endregion

            #region nodalforces
            NodeForceAttribute F = new NodeForceAttribute(loadCase, sys, 0, 0, 5.0, 0, 0, 0);
            #endregion

            Node nodeA = new Node(0, 8, 0, 1, "A");
            nodeA.AddAttribute(fixDXDYDZRZ);
            Node nodeB = new Node(0, 0, 0, 2, "B");
            nodeB.AddAttribute(fixDXDYDZRZ);
            Node nodeC = new Node(8, 8, 0, 3, "C");
            nodeC.AddAttribute(F);
            nodeC.AddAttribute(fixDXDYRZ);
            Node nodeD = new Node(8, 0, 0, 3, "D");
            nodeD.AddAttribute(fixDXDYDZRZ);

            FiniteElement e0 = new TriangularDK(new Node[] { nodeA, nodeB, nodeC }, prop, 1);
            FiniteElement e1 = new TriangularDK(new Node[] { nodeB, nodeD, nodeC }, prop, 1);
            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0, e1 });

            double dz = fem.GetDisplacementGlobalCoordinates(nodeC, LinearSolver.DOF.DZ).First();
            Assert.AreEqual(0.24960, dz, 1e-6);

            double[] displElement = fem.GetDisplacementsGlobalCoordinates(e0);
            e0.GetResults(displElement, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);

            double tauXY1 = globalStress[0][1, 0]; //node 1
            double tauXY2 = globalStress[0][0, 1]; //node 1

            Assert.AreEqual(-15.0, tauXY1, 0.01);
            Assert.AreEqual(-15.0, tauXY2, 0.01);
        }

        [TestMethod]
        public void Benchmark10001()
        {
            /// Benchmark10001 - Bathe, Numerical Methods in Finite Elements Analysis - Esercizio Nr 5.11 pg 358
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };

            /// Nodes in 3D  XYZ
            /*int[] Node1DoF = new int[] { 0, 0, 1, 0, 0, 1 };
            int[] Node2DoF = new int[] { 0, 0, 1, 0, 0, 1 };
            int[] Node3DoF = new int[] { 0, 0, 1, 0, 0, 1 };
            int[] Node4DoF = new int[] { 0, 0, 1, 0, 0, 1 };*/

            int[] Node1DoF = new int[] { 1, 1, 0, 0, 0, 1 };
            int[] Node2DoF = new int[] { 1, 1, 0, 0, 0, 1 };
            int[] Node3DoF = new int[] { 1, 1, 0, 0, 0, 1 };
            int[] Node4DoF = new int[] { 1, 1, 0, 0, 0, 1 };

            GPC.Model.FEMOld.Node Node1 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(0.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF);
            GPC.Model.FEMOld.Node Node2 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(2.0, 0.0, 0.0), 2, NodeDoFID, Node2DoF);
            GPC.Model.FEMOld.Node Node3 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(2.0, 3.0, 0.0), 3, NodeDoFID, Node3DoF);
            GPC.Model.FEMOld.Node Node4 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(0.0, 3.0, 0.0), 4, NodeDoFID, Node4DoF);

            GPC.Model.FEMOld.Node[] nodes = new GPC.Model.FEMOld.Node[4];
            nodes[0] = Node1;
            nodes[1] = Node2;
            nodes[2] = Node3;
            nodes[3] = Node4;

            int _globalDoF = 0;
            int _reactionDoF = 0;

            // Arrange Nodes
            for (int nd = 0; nd < nodes.Length; nd++)
            {
                nodes[nd].DoF.FormIncidence(ref _globalDoF, ref _reactionDoF);
            }

            ///  Section
            double E = 12; // MPa
            double ni = 0.0;

            /// Material
            Material mat = new SteelMaterial("Steel", E, ni, 355, 510, 355 / E, 0, 0, new Guid());// new Material("Steel", E, ni, 0.0, 0.0, new Guid());
            PlateProperty property = new PlateProperty(mat, 1.0, 1.0);
            GPC.Model.FEMOld.PlateDKQ shell = new GPC.Model.FEMOld.PlateDKQ(new Guid(), property, 1, nodes);

            mnl.Matrix<double> _stiffnessMatrix = mnl.Matrix<double>.Build.Dense(_globalDoF, _globalDoF, 0.0);
            shell.BuildElementDoFIncidence();
            shell.KInGlobal(ref _stiffnessMatrix);

            Console.WriteLine("Element local stiffness matrix");
            for (int r = 0; r < _stiffnessMatrix.RowCount; r++)
            {
                for (int c = 0; c < _stiffnessMatrix.ColumnCount; c++)
                {
                    Console.Write(_stiffnessMatrix[r, c].ToString("F1") + " ");
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void Benchmark10002()
        {
            /// Benchmark10001 - Bathe, Numerical Methods in Finite Elements Analysis - Esercizio Nr 5.11 pg 358
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };

            /// Nodes in 3D  XYZ
            /*int[] Node1DoF = new int[] { 0, 0, 1, 0, 0, 1 };
            int[] Node2DoF = new int[] { 0, 0, 1, 0, 0, 1 };
            int[] Node3DoF = new int[] { 0, 0, 1, 0, 0, 1 };
            int[] Node4DoF = new int[] { 0, 0, 1, 0, 0, 1 };*/

            int[] Node1DoF = new int[] { 1, 1, 0, 0, 0, 1 };
            int[] Node2DoF = new int[] { 1, 1, 0, 0, 0, 1 };
            int[] Node3DoF = new int[] { 1, 1, 0, 0, 0, 1 };
            int[] Node4DoF = new int[] { 1, 1, 0, 0, 0, 1 };

            GPC.Model.FEMOld.Node Node1 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(0.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF);
            GPC.Model.FEMOld.Node Node2 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(2.0, 0.0, 0.0), 2, NodeDoFID, Node2DoF);
            GPC.Model.FEMOld.Node Node3 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(3.0, 1.0, 0.0), 3, NodeDoFID, Node3DoF);
            GPC.Model.FEMOld.Node Node4 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(1.0, 1.0, 0.0), 4, NodeDoFID, Node4DoF);

            GPC.Model.FEMOld.Node[] nodes = new GPC.Model.FEMOld.Node[4];
            nodes[0] = Node1;
            nodes[1] = Node2;
            nodes[2] = Node3;
            nodes[3] = Node4;

            int _globalDoF = 0;
            int _reactionDoF = 0;

            // Arrange Nodes
            for (int nd = 0; nd < nodes.Length; nd++)
            {
                nodes[nd].DoF.FormIncidence(ref _globalDoF, ref _reactionDoF);
            }

            ///  Section
            double E = 12; // MPa
            double ni = 0.0;

            /// Material
            Material mat = new SteelMaterial("Steel", E, ni, 355, 510, 355 / E, 0, 0, new Guid());// new Material("Steel", E, ni, 0.0, 0.0, new Guid());
            PlateProperty property = new PlateProperty(mat, 1.0, 1.0);
            GPC.Model.FEMOld.PlateDKQ shell = new GPC.Model.FEMOld.PlateDKQ(new Guid(), property, 1, nodes);

            mnl.Matrix<double> _stiffnessMatrix = mnl.Matrix<double>.Build.Dense(_globalDoF, _globalDoF, 0.0);
            shell.BuildElementDoFIncidence();
            shell.KInGlobal(ref _stiffnessMatrix);

            Console.WriteLine("Element local stiffness matrix");
            for (int r = 0; r < _stiffnessMatrix.RowCount; r++)
            {
                for (int c = 0; c < _stiffnessMatrix.ColumnCount; c++)
                {
                    Console.Write(_stiffnessMatrix[r, c].ToString("F1") + " ");
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void Benchmark10003()
        {
            /// Benchmark10001 - Bathe, Numerical Methods in Finite Elements Analysis - Esercizio Nr 5.11 pg 358
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };

            /// Nodes in 3D  XYZ
            int[] Node1DoF = new int[] { 0, 0, 1, 1, 1, 1 };
            int[] Node2DoF = new int[] { 0, 0, 1, 1, 1, 1 };
            int[] Node3DoF = new int[] { 0, 0, 1, 1, 1, 1 };
            int[] Node4DoF = new int[] { 0, 0, 1, 1, 1, 1 };

            GPC.Model.FEMOld.Node Node1 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(-1.0, -1.0, 0.0), 1, NodeDoFID, Node1DoF);
            GPC.Model.FEMOld.Node Node2 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(+1.0, -1.0, 0.0), 2, NodeDoFID, Node2DoF);
            GPC.Model.FEMOld.Node Node3 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(+1.0, +1.0, 0.0), 3, NodeDoFID, Node3DoF);
            GPC.Model.FEMOld.Node Node4 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(-1.0, +1.0, 0.0), 4, NodeDoFID, Node4DoF);

            GPC.Model.FEMOld.Node[] nodes = new GPC.Model.FEMOld.Node[4];
            nodes[0] = Node1;
            nodes[1] = Node2;
            nodes[2] = Node3;
            nodes[3] = Node4;

            int _globalDoF = 0;
            int _reactionDoF = 0;

            // Arrange Nodes
            for (int nd = 0; nd < nodes.Length; nd++)
            {
                nodes[nd].DoF.FormIncidence(ref _globalDoF, ref _reactionDoF);
            }

            ///  Section
            double E = 1; // MPa
            double ni = 0.0;

            /// Material
            Material mat = new SteelMaterial("Steel", E, ni, 355, 510, 355 / E, 0, 0, new Guid());
            PlateProperty property = new PlateProperty(mat, 1.0, 1.0);
            GPC.Model.FEMOld.PlateDKQ shell = new GPC.Model.FEMOld.PlateDKQ(new Guid(), property, 1, nodes);

            mnl.Matrix<double> _stiffnessMatrix = mnl.Matrix<double>.Build.Dense(_globalDoF, _globalDoF, 0.0);
            shell.BuildElementDoFIncidence();
            shell.KInGlobal(ref _stiffnessMatrix);

            Console.WriteLine("Element local stiffness matrix");
            for (int r = 0; r < _stiffnessMatrix.RowCount; r++)
            {
                for (int c = 0; c < _stiffnessMatrix.ColumnCount; c++)
                {
                    Console.Write(_stiffnessMatrix[r, c].ToString("F2") + " ");
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void RectangleDKTTest1()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0);

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(1, -1, 0, 1, "1");
            nodesPlate1[1] = new Node(1, -1, 0, 2, "2");
            nodesPlate1[2] = new Node(1, 1, 0, 3, "3");
            nodesPlate1[3] = new Node(-1, 1, 0, 3, "4");

            FiniteElement e0 = new RectangularDK(nodesPlate1, prop, 1);
            e0.BuildMatrix();

            Console.WriteLine("Element local stiffness matrix");
            for (int r = 0; r < e0.KElementLocalCoord.RowCount; r++)
            {
                for (int c = 0; c < e0.KElementLocalCoord.ColumnCount; c++)
                {
                    Console.Write(e0.KElementLocalCoord[r, c].ToString("F2") + " ");
                }
                Console.WriteLine();
            }

            //actually does not word
            Assert.AreEqual(true, false);
        }

        [TestMethod]
        public void RectangleDKTTest2()
        {
            //LoadCase loadCase = new LoadCase("myLoadCase", new Guid());
            /*FreedomCase freedomCase = new FreedomCase("freedomCase1");*/

            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0);

            /*CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute RXRYRZ = new NodeRestrainAttribute(freedomCase, sys);
            RXRYRZ.AddRestrain(LinearSolver.DOF.RX);
            RXRYRZ.AddRestrain(LinearSolver.DOF.RY);
            RXRYRZ.AddRestrain(LinearSolver.DOF.RZ);*/

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(0, 0, 0, 1, "1");
            nodesPlate1[1] = new Node(2, 0, 0, 2, "2");
            nodesPlate1[2] = new Node(3, 1, 0, 3, "3");
            nodesPlate1[3] = new Node(1, 1, 0, 3, "4");

            FiniteElement e0 = new RectangularDK(nodesPlate1, prop, 1);
            e0.BuildMatrix();

            Console.WriteLine("Element local stiffness matrix");
            for (int r = 0; r < e0.KElementLocalCoord.RowCount; r++)
            {
                for (int c = 0; c < e0.KElementLocalCoord.ColumnCount; c++)
                {
                    Console.Write(e0.KElementLocalCoord[r, c].ToString("F2") + " ");
                }
                Console.WriteLine();
            }

            //actually does not word
            Assert.AreEqual(true, false);
        }

        [TestMethod]
        public void TriangleElementTest1()
        {
            LoadCase loadCase = new LoadCase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("mat", 10000, 0.0, 355, 510, 7850);
            double t = 1.0;
            PlateProperty prop = new PlateProperty(mat, t, t);

            #region restrains
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute fix = new NodeRestrainAttribute(freedomCase, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute fixRZ = new NodeRestrainAttribute(freedomCase, sys);
            fixRZ.AddExternalRestrain(LinearSolver.DOF.RZ);
            #endregion

            #region nodalforces
            NodeForceAttribute F = new NodeForceAttribute(loadCase, sys, 0.0, 1.0, 0.0, 1.0, 0, 0);
            #endregion

            Node nodeA = new Node(0, 8, 0, 1, "A");
            nodeA.AddAttribute(fixRZ);
            nodeA.AddAttribute(F);

            Node nodeB = new Node(0, 0, 0, 2, "B");
            nodeB.AddAttribute(fix);

            Node nodeC = new Node(8, 8, 0, 3, "C");
            nodeC.AddAttribute(F);
            nodeC.AddAttribute(fixRZ);

            Node nodeD = new Node(8, 0, 0, 3, "D");
            nodeD.AddAttribute(fix);

            FiniteElement e0 = new TriangleElement(new Node[] { nodeA, nodeB, nodeC }, prop, 1);
            FiniteElement e1 = new TriangleElement(new Node[] { nodeB, nodeD, nodeC }, prop, 1);
            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0, e1 });

            double DY = fem.GetDisplacementGlobalCoordinates(nodeC, LinearSolver.DOF.DY).First();
            double DZ = fem.GetDisplacementGlobalCoordinates(nodeC, LinearSolver.DOF.DZ).First();
            Assert.AreEqual(0.0096, DZ, 1e-4);
            Assert.AreEqual(0.0002, DY, 1e-4);

            double sigmaTopYY = -(F.M1 + F.M1) / (1.0 / 6.0 * 8.0 * (t * t)) + (F.F2 + F.F2) / (t * 8.0);

            double[] e0GlobalDispl = fem.GetDisplacementsGlobalCoordinates(e0);
            e0.GetResults(e0GlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
            Assert.AreEqual(sigmaTopYY, globalStress[0][1,1], 0.001); //sigmaYY top face

            double[] e1GlobalDispl = fem.GetDisplacementsGlobalCoordinates(e1);
            e1.GetResults(e1GlobalDispl, out localDispl,
                            out globalPseudoDef, out localPseudoDef,
                            out globalForces, out localForces,
                            out globalStress, out localStress,
                            out globalEpsilon, out localEpsilon);
            Assert.AreEqual(sigmaTopYY, globalStress[0][1, 1], 0.001); //sigmaYY top face
        }

        [TestMethod]
        public void TriangleElementTest2()
        {
            LoadCase loadCase = new LoadCase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("mat", 10000, 0.0, 355, 510, 7850);
            double t = 1.0;
            PlateProperty prop = new PlateProperty(mat, t, t);

            #region restrains
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute fix = new NodeRestrainAttribute(freedomCase, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute fixRZ = new NodeRestrainAttribute(freedomCase, sys);
            fixRZ.AddExternalRestrain(LinearSolver.DOF.RZ);
            #endregion

            #region forces
            PlatePressureAttribute p = new PlatePressureAttribute(loadCase, sys, 0.0, 0.0, 1.0);
            #endregion

            Node nodeA = new Node(0, 8, 0, 1, "A");
            nodeA.AddAttribute(fixRZ);
            
            Node nodeB = new Node(0, 0, 0, 2, "B");
            nodeB.AddAttribute(fix);

            Node nodeC = new Node(8, 8, 0, 3, "C");
            nodeC.AddAttribute(fixRZ);

            Node nodeD = new Node(8, 0, 0, 3, "D");
            nodeD.AddAttribute(fix);

            Plate e0 = new TriangleElement(new Node[] { nodeA, nodeB, nodeC }, prop, 1);
            e0.AddAttribute(p);
            Plate e1 = new TriangleElement(new Node[] { nodeB, nodeD, nodeC }, prop, 2);
            e1.AddAttribute(p);
            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0, e1 });

            double DZC = fem.GetDisplacementGlobalCoordinates(nodeC, LinearSolver.DOF.DZ).First();
            Assert.AreEqual(0.97765, DZC, 1e-4); //value from SAP
            double DZA = fem.GetDisplacementGlobalCoordinates(nodeA, LinearSolver.DOF.DZ).First();
            Assert.AreEqual(0.75597, DZA, 1e-4); //value from SAP

            double[] e0GlobalDispl = fem.GetDisplacementsGlobalCoordinates(e0);
            /*e0.GetResults(e0GlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
            */
        }

        [TestMethod]
        public void QuadrilateralMembranalKTest1()
        {
            Material mat = new SteelMaterial("steel", 1.0, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            Node[] nds = new Node[4];
            nds[0] = new Node(-1, -1, 0, 1, "1");
            nds[1] = new Node(+1, -1, 0, 2, "2");
            nds[2] = new Node(+1, +1, 0, 3, "3");
            nds[3] = new Node(-1, +1, 0, 4, "4");

            RectangularMembranal el = new RectangularMembranal(nds, prop, 1);
            el.BuildMatrix();
            mnl.Matrix<double> kLocal = el.KElementLocalCoord;
            mnl.Matrix<double> kLocalManual = mnl.Matrix<double>.Build.Dense(0, 6);
            /*double[] r0 = new double[] { 145833, 62500, -41667, -20833, -104167, -41667 };
            double[] r1 = new double[] { 62500, 145833, -41667, -104167, -20833, -41667 };
            double[] r2 = new double[] { -41667, -41667, 41667, 0, 0, 41667 };
            double[] r3 = new double[] { -20833, -104167, 0, 104167, 20833, 0 };
            double[] r4 = new double[] { -104167, -20833, 0, 20833, 104167, 0 };
            double[] r5 = new double[] { -41667, -41667, 41667, 0, 0, 41667 };

            kLocalManual = kLocalManual.InsertRow(0, mnl.Vector<double>.Build.Dense(r0));
            kLocalManual = kLocalManual.InsertRow(1, mnl.Vector<double>.Build.Dense(r1));
            kLocalManual = kLocalManual.InsertRow(2, mnl.Vector<double>.Build.Dense(r2));
            kLocalManual = kLocalManual.InsertRow(3, mnl.Vector<double>.Build.Dense(r3));
            kLocalManual = kLocalManual.InsertRow(4, mnl.Vector<double>.Build.Dense(r4));
            kLocalManual = kLocalManual.InsertRow(5, mnl.Vector<double>.Build.Dense(r5));*/

            //controllo klocale elemento finito 4 nodi stato piano di tensione
            Console.WriteLine("kLocal");
            for (int i = 0; i < kLocal.RowCount; i++)
            {
                for (int j = 0; j < kLocal.ColumnCount; j++)
                {
                    Console.Write(kLocal[i, j].ToString("F2") + " ");
                    //Assert.AreEqual(kLocal[i, j] - kLocalManual[i, j], 0, 1, "kLocal no OK -> row " + i + " col " + j);
                    //sarebbe stato meglio usare kLocal[i,j] / kLocalManual[i,j] ma 0/0 = NaN!!
                }
                Console.WriteLine();
            }
        }
    }
}