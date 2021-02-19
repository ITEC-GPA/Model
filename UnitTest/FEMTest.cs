using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.Elements;
using GPC.Model.Materials;
using GPC.Model.FreedomCases;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Model.LoadCases;

namespace UnitTest
{
    [TestClass]
    public class FEMTest
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
        public void TriangularMembranalKTest1()
        {
            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            Node[] nds = new Node[3];
            nds[0] = new Node(0, 0, 0, 1, "1");
            nds[1] = new Node(0, 1, 0, 2, "2");
            nds[2] = new Node(1, 0, 0, 3, "3");

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
            Assert.AreEqual(kLocal, kGlobal, "kLocal not equal to Kglobal with local axis");
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

            FEMModel fem = new FEMModel(elements.ToArray());
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

            //controllo klocale elemento finito 3 nodi stato piano di tensione
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
            DXDYDZ.AddRestrain(FEMModel.DOF.DX);
            DXDYDZ.AddRestrain(FEMModel.DOF.DY);
            DXDYDZ.AddRestrain(FEMModel.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute(fc, sys);
            DZ.AddRestrain(FEMModel.DOF.DZ);

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

            FEMModel fem = new FEMModel(elements.ToArray());
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

            //controllo klocale elemento finito 3 nodi stato piano di tensione
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
            DXDYDZ.AddRestrain(FEMModel.DOF.DX);
            DXDYDZ.AddRestrain(FEMModel.DOF.DY);
            DXDYDZ.AddRestrain(FEMModel.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute(freedomCase, sys);
            DZ.AddRestrain(FEMModel.DOF.DZ);

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

            FEMModel fem = new FEMModel(elements.ToArray());
            double[] Node4DX = fem.GetDisplacementGlobalCoordinates(nd4, FEMModel.DOF.DX);
            double[] Node4DY = fem.GetDisplacementGlobalCoordinates(nd4, FEMModel.DOF.DY);

            double[] Node3DX = fem.GetDisplacementGlobalCoordinates(nd3, FEMModel.DOF.DX);
            double[] Node3DY = fem.GetDisplacementGlobalCoordinates(nd3, FEMModel.DOF.DY);

            double[] Node3CopyDX = fem.GetDisplacementGlobalCoordinates(nd3copy, FEMModel.DOF.DX);
            double[] Node3CopyDY = fem.GetDisplacementGlobalCoordinates(nd3copy, FEMModel.DOF.DY);

            /*Node 4 Displacement
            DX(mm) 0.009130
            DY(mm) - 0.005478
            DZ(mm) 0.000000*/
            double dXNode4 = 0.009130;
            double dYNode4 = -0.005478;
            Assert.AreEqual(Node4DX[0], dXNode4, 0.000001);
            Assert.AreEqual(Node4DY[0], dYNode4, 0.000001);

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
            DXDYDZ.AddRestrain(FEMModel.DOF.DX);
            DXDYDZ.AddRestrain(FEMModel.DOF.DY);
            DXDYDZ.AddRestrain(FEMModel.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute(freedomCase, sys);
            DZ.AddRestrain(FEMModel.DOF.DZ);

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

            FEMModel fem = new FEMModel(elements.ToArray());
            double[] Node4DX = fem.GetDisplacementGlobalCoordinates(nd4, FEMModel.DOF.DX);
            double[] Node4DY = fem.GetDisplacementGlobalCoordinates(nd4, FEMModel.DOF.DY);

            double[] Node3DX = fem.GetDisplacementGlobalCoordinates(nd3, FEMModel.DOF.DX);
            double[] Node3DY = fem.GetDisplacementGlobalCoordinates(nd3, FEMModel.DOF.DY);

            double[] Node3CopyDX = fem.GetDisplacementGlobalCoordinates(nd3copy, FEMModel.DOF.DX);
            double[] Node3CopyDY = fem.GetDisplacementGlobalCoordinates(nd3copy, FEMModel.DOF.DY);

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
    }
}