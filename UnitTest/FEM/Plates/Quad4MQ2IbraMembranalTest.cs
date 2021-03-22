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

namespace FemTest.Solver
{
    [TestClass]
    public class FemSolverQuad4MQ2IbraMembranalTest
    {
        /// <summary>
        /// TEST LOCAL MATRIX
        /// </summary>
        [TestMethod]
        public void Quad4MQ2IbraMembranalTest1()
        {
            double E = 1.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, 1, "1");
            nds[1] = new Node(+1.0, -1.0, 0, 2, "2");
            nds[2] = new Node(+1.0, +1.0, 0, 3, "3");
            nds[3] = new Node(-1.0, +1.0, 0, 4, "4");

            Quad4MQ2IbraMembranal el = new Quad4MQ2IbraMembranal(nds, prop, 1);
            //Quad4Membranal el2 = new Quad4Membranal(nds, prop, 1);
            el.BuildMatrix();
            //el2.BuildMatrix();
            mnl.Matrix<double> k1 = el.KElementGlobalCoord;
            //mnl.Matrix<double> k2 = el2.KElementGlobalCoord;

            Console.WriteLine("k1 Non Correct= ");
            Util.WriteMatrix(k1, "F3");

            //Console.WriteLine("k2 Correct = ");
            //Util.WriteMatrix(k2, "F3");
            /*mnl.Matrix<double> kLocalManual = mnl.Matrix<double>.Build.Dense(0, 8);*/

            /*double[] r0 = new double[] { 0.5000, 0.1250, -0.2500, -0.1250, -0.2500, -0.1250, 0.0000, 0.1250 };
            double[] r1 = new double[] { 0.1250, 0.5000, 0.1250, 0.0000, -0.1250, -0.2500, -0.1250, -0.2500 };
            double[] r2 = new double[] { -0.2500, 0.1250, 0.5000, -0.1250, 0.0000, -0.1250, -0.2500, 0.1250 };
            double[] r3 = new double[] { -0.1250, 0.0000, -0.1250, 0.5000, 0.1250, -0.2500, 0.1250, -0.2500 };
            double[] r4 = new double[] { -0.2500, -0.1250, 0.0000, 0.1250, 0.5000, 0.1250, -0.2500, -0.1250 };
            double[] r5 = new double[] { -0.1250, -0.2500, -0.1250, -0.2500, 0.1250, 0.5000, 0.1250, 0.0000 };
            double[] r6 = new double[] { 0.0000, -0.1250, -0.2500, 0.1250, -0.2500, 0.1250, 0.5000, -0.1250 };
            double[] r7 = new double[] { 0.1250, -0.2500, 0.1250, -0.2500, -0.1250, 0.0000, -0.1250, 0.5000 };

            kLocalManual = kLocalManual.InsertRow(0, mnl.Vector<double>.Build.Dense(r0));
            kLocalManual = kLocalManual.InsertRow(1, mnl.Vector<double>.Build.Dense(r1));
            kLocalManual = kLocalManual.InsertRow(2, mnl.Vector<double>.Build.Dense(r2));
            kLocalManual = kLocalManual.InsertRow(3, mnl.Vector<double>.Build.Dense(r3));
            kLocalManual = kLocalManual.InsertRow(4, mnl.Vector<double>.Build.Dense(r4));
            kLocalManual = kLocalManual.InsertRow(5, mnl.Vector<double>.Build.Dense(r5));
            kLocalManual = kLocalManual.InsertRow(6, mnl.Vector<double>.Build.Dense(r6));
            kLocalManual = kLocalManual.InsertRow(7, mnl.Vector<double>.Build.Dense(r7));

            //controllo klocale elemento finito 4 nodi stato piano di tensione
            Console.WriteLine("kLocal");
            for (int i = 0; i < kLocal.RowCount; i++)
            {
                for (int j = 0; j < kLocal.ColumnCount; j++)
                {
                    Console.Write(kLocal[i, j].ToString("F4") + " ");
                    Assert.AreEqual(kLocal[i, j] - kLocalManual[i, j], 0, 0.001, "kLocal no OK -> row " + i + " col " + j);
                    //sarebbe stato meglio usare kLocal[i,j] / kLocalManual[i,j] ma 0/0 = NaN!!
                }
                Console.WriteLine();
            }*/
        }

        [TestMethod]
        public void Quad4MQ2IbraMembranalOldFem1()
        {
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };

            /// Nodes in 3D  XYZ
            /*int[] Node1DoF = new int[] { 0, 0, 1, 0, 0, 1 };
            int[] Node2DoF = new int[] { 0, 0, 1, 0, 0, 1 };
            int[] Node3DoF = new int[] { 0, 0, 1, 0, 0, 1 };
            int[] Node4DoF = new int[] { 0, 0, 1, 0, 0, 1 };*/

            int[] Node1DoF = new int[] { 0, 0, 1, 1, 1, 0 };
            int[] Node2DoF = new int[] { 0, 0, 1, 1, 1, 0 };
            int[] Node3DoF = new int[] { 0, 0, 1, 1, 1, 0 };
            int[] Node4DoF = new int[] { 0, 0, 1, 1, 1, 0 };

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
                    Console.Write(_stiffnessMatrix[r, c].ToString("F3") + " \t");
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void Quad4MQ2IbraMembranalTest2()
        {
            Material mat = new SteelMaterial("steel", 1.0, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            Node[] nds = new Node[4];
            nds[0] = new Node(-2.0, -2.0, 0, 1, "1");
            nds[1] = new Node(+2.0, -2.0, 0, 2, "2");
            nds[2] = new Node(+2.0, +2.0, 0, 3, "3");
            nds[3] = new Node(-2.0, +2.0, 0, 4, "4");

            Quad4MQ2IbraMembranal el = new Quad4MQ2IbraMembranal(nds, prop, 1);
            Quad4Membranal el2 = new Quad4Membranal(nds, prop, 1);
            el.BuildMatrix();
            el2.BuildMatrix();
            mnl.Matrix<double> k1 = el.KElementGlobalCoord;
            mnl.Matrix<double> k2 = el2.KElementGlobalCoord;

            Console.WriteLine("k1 Non Correct= ");
            Util.WriteMatrix(k1, "F1");

            Console.WriteLine("k2 Correct = ");
            Util.WriteMatrix(k2, "F1");
            /*mnl.Matrix<double> kLocalManual = mnl.Matrix<double>.Build.Dense(0, 8);*/

            /*double[] r0 = new double[] { 0.5000, 0.1250, -0.2500, -0.1250, -0.2500, -0.1250, 0.0000, 0.1250 };
            double[] r1 = new double[] { 0.1250, 0.5000, 0.1250, 0.0000, -0.1250, -0.2500, -0.1250, -0.2500 };
            double[] r2 = new double[] { -0.2500, 0.1250, 0.5000, -0.1250, 0.0000, -0.1250, -0.2500, 0.1250 };
            double[] r3 = new double[] { -0.1250, 0.0000, -0.1250, 0.5000, 0.1250, -0.2500, 0.1250, -0.2500 };
            double[] r4 = new double[] { -0.2500, -0.1250, 0.0000, 0.1250, 0.5000, 0.1250, -0.2500, -0.1250 };
            double[] r5 = new double[] { -0.1250, -0.2500, -0.1250, -0.2500, 0.1250, 0.5000, 0.1250, 0.0000 };
            double[] r6 = new double[] { 0.0000, -0.1250, -0.2500, 0.1250, -0.2500, 0.1250, 0.5000, -0.1250 };
            double[] r7 = new double[] { 0.1250, -0.2500, 0.1250, -0.2500, -0.1250, 0.0000, -0.1250, 0.5000 };

            kLocalManual = kLocalManual.InsertRow(0, mnl.Vector<double>.Build.Dense(r0));
            kLocalManual = kLocalManual.InsertRow(1, mnl.Vector<double>.Build.Dense(r1));
            kLocalManual = kLocalManual.InsertRow(2, mnl.Vector<double>.Build.Dense(r2));
            kLocalManual = kLocalManual.InsertRow(3, mnl.Vector<double>.Build.Dense(r3));
            kLocalManual = kLocalManual.InsertRow(4, mnl.Vector<double>.Build.Dense(r4));
            kLocalManual = kLocalManual.InsertRow(5, mnl.Vector<double>.Build.Dense(r5));
            kLocalManual = kLocalManual.InsertRow(6, mnl.Vector<double>.Build.Dense(r6));
            kLocalManual = kLocalManual.InsertRow(7, mnl.Vector<double>.Build.Dense(r7));

            //controllo klocale elemento finito 4 nodi stato piano di tensione
            Console.WriteLine("kLocal");
            for (int i = 0; i < kLocal.RowCount; i++)
            {
                for (int j = 0; j < kLocal.ColumnCount; j++)
                {
                    Console.Write(kLocal[i, j].ToString("F4") + " ");
                    Assert.AreEqual(kLocal[i, j] - kLocalManual[i, j], 0, 0.001, "kLocal no OK -> row " + i + " col " + j);
                    //sarebbe stato meglio usare kLocal[i,j] / kLocalManual[i,j] ma 0/0 = NaN!!
                }
                Console.WriteLine();
            }*/
        }

#if FALSE
        /// <summary>
        /// TEST LOCAL MATRIX NON RECTANGLE NON SQUARED
        /// </summary>
        [TestMethod]
        public void Quad4MembranalTest2()
        {
            Material mat = new SteelMaterial("steel", 1.0, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            Node[] nds = new Node[4];
            nds[0] = new Node(+0.0, +0, 0, 1, "1");
            nds[1] = new Node(+2.0, +0, 0, 2, "2");
            nds[2] = new Node(+2.0, +1, 0, 3, "3");
            nds[3] = new Node(+0.0, +1, 0, 4, "4");

            Quad4Membranal el = new Quad4Membranal(nds, prop, 1);
            el.BuildMatrix();
            mnl.Matrix<double> kLocal = el.KElementLocalCoord;
            mnl.Matrix<double> kLocalManual = mnl.Matrix<double>.Build.Dense(0, 8);

            double[] r0 = new double[] { 0.5000, 0.1250, 0.0000, -0.1250, -0.2500, -0.1250, -0.2500, 0.1250 };
            double[] r1 = new double[] { 0.1250, 0.7500, 0.1250, 0.2500, -0.1250, -0.3750, -0.1250, -0.6250 };
            double[] r2 = new double[] { 0.0000, 0.1250, 0.5000, -0.1250, -0.2500, -0.1250, -0.2500, 0.1250 };
            double[] r3 = new double[] { -0.1250, 0.2500, -0.1250, 0.7500, 0.1250, -0.6250, 0.1250, -0.3750 };
            double[] r4 = new double[] { -0.2500, -0.1250, -0.2500, 0.1250, 0.5000, 0.1250, 0.0000, -0.1250 };
            double[] r5 = new double[] { -0.1250, -0.3750, -0.1250, -0.6250, 0.1250, 0.7500, 0.1250, 0.2500 };
            double[] r6 = new double[] { -0.2500, -0.1250, -0.2500, 0.1250, 0.0000, 0.1250, 0.5000, -0.1250 };
            double[] r7 = new double[] { 0.1250, -0.6250, 0.1250, -0.3750, -0.1250, 0.2500, -0.1250, 0.7500 };

            kLocalManual = kLocalManual.InsertRow(0, mnl.Vector<double>.Build.Dense(r0));
            kLocalManual = kLocalManual.InsertRow(1, mnl.Vector<double>.Build.Dense(r1));
            kLocalManual = kLocalManual.InsertRow(2, mnl.Vector<double>.Build.Dense(r2));
            kLocalManual = kLocalManual.InsertRow(3, mnl.Vector<double>.Build.Dense(r3));
            kLocalManual = kLocalManual.InsertRow(4, mnl.Vector<double>.Build.Dense(r4));
            kLocalManual = kLocalManual.InsertRow(5, mnl.Vector<double>.Build.Dense(r5));
            kLocalManual = kLocalManual.InsertRow(6, mnl.Vector<double>.Build.Dense(r6));
            kLocalManual = kLocalManual.InsertRow(7, mnl.Vector<double>.Build.Dense(r7));

            //controllo klocale elemento finito 4 nodi stato piano di tensione
            Console.WriteLine("kLocal");
            for (int i = 0; i < kLocal.RowCount; i++)
            {
                for (int j = 0; j < kLocal.ColumnCount; j++)
                {
                    Console.Write(kLocal[i, j].ToString("F4") + " ");
                    Assert.AreEqual(kLocal[i, j] - kLocalManual[i, j], 0, 0.001, "kLocal no OK -> row " + i + " col " + j);
                    //sarebbe stato meglio usare kLocal[i,j] / kLocalManual[i,j] ma 0/0 = NaN!!
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// TEST LOCAL MATRIX GENERAL QUADRILATERAL
        /// </summary>
        [TestMethod]
        public void Quad4MembranalTest3()
        {
            Material mat = new SteelMaterial("steel", 1.0, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            Node[] nds = new Node[4];
            nds[0] = new Node(+0.0, +0, 0, 1, "1");
            nds[1] = new Node(+2.0, +0, 0, 2, "2");
            nds[2] = new Node(+2.0, +2, 0, 3, "3");
            nds[3] = new Node(+0.0, +1, 0, 4, "4");

            Quad4Membranal el = new Quad4Membranal(nds, prop, 1);
            el.BuildMatrix();
            mnl.Matrix<double> kLocal = el.KElementLocalCoord;
            mnl.Matrix<double> kLocalManual = mnl.Matrix<double>.Build.Dense(0, 8);

            double[] r0 = new double[] { 0.4808, 0.0577, -0.1154, -0.1538, -0.1346, -0.0962, -0.2308, 0.1923 };
            double[] r1 = new double[] { 0.0577, 0.6442, 0.0962, 0.1154, -0.0962, -0.2404, -0.0577, -0.5192 };
            double[] r2 = new double[] { -0.1154, 0.0962, 0.5577, -0.1731, -0.0577, -0.0769, -0.3846, 0.1538 };
            double[] r3 = new double[] { -0.1538, 0.1154, -0.1731, 0.5673, 0.1731, -0.3173, 0.1538, -0.3654 };
            double[] r4 = new double[] { -0.1346, -0.0962, -0.0577, 0.1731, 0.3077, 0.0769, -0.1154, -0.1538 };
            double[] r5 = new double[] { -0.0962, -0.2404, -0.0769, -0.3173, 0.0769, 0.4423, 0.0962, 0.1154 };
            double[] r6 = new double[] { -0.2308, -0.0577, -0.3846, 0.1538, -0.1154, 0.0962, 0.7308, -0.1923 };
            double[] r7 = new double[] { 0.1923, -0.5192, 0.1538, -0.3654, -0.1538, 0.1154, -0.1923, 0.7692 };

            kLocalManual = kLocalManual.InsertRow(0, mnl.Vector<double>.Build.Dense(r0));
            kLocalManual = kLocalManual.InsertRow(1, mnl.Vector<double>.Build.Dense(r1));
            kLocalManual = kLocalManual.InsertRow(2, mnl.Vector<double>.Build.Dense(r2));
            kLocalManual = kLocalManual.InsertRow(3, mnl.Vector<double>.Build.Dense(r3));
            kLocalManual = kLocalManual.InsertRow(4, mnl.Vector<double>.Build.Dense(r4));
            kLocalManual = kLocalManual.InsertRow(5, mnl.Vector<double>.Build.Dense(r5));
            kLocalManual = kLocalManual.InsertRow(6, mnl.Vector<double>.Build.Dense(r6));
            kLocalManual = kLocalManual.InsertRow(7, mnl.Vector<double>.Build.Dense(r7));

            //controllo klocale elemento finito 4 nodi stato piano di tensione
            Console.WriteLine("kLocal");
            for (int i = 0; i < kLocal.RowCount; i++)
            {
                for (int j = 0; j < kLocal.ColumnCount; j++)
                {
                    Console.Write(kLocal[i, j].ToString("F4") + " ");
                    Assert.AreEqual(kLocal[i, j] - kLocalManual[i, j], 0, 0.001, "kLocal no OK -> row " + i + " col " + j);
                    //sarebbe stato meglio usare kLocal[i,j] / kLocalManual[i,j] ma 0/0 = NaN!!
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// TEST local to Gloabal matrix
        /// </summary>
        [TestMethod]
        public void Quad4MembranalTest4()
        {
            Material mat = new SteelMaterial("steel", 1.0, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1, 0, 1, "1");
            nds[1] = new Node(+1.0, -1, 0, 2, "2");
            nds[2] = new Node(+1.0, +1, 0, 3, "3");
            nds[3] = new Node(-1.0, +1, 0, 4, "4");

            Quad4Membranal el = new Quad4Membranal(nds, prop, 1);
            el.BuildMatrix();
            mnl.Matrix<double> kLocal = el.KElementLocalCoord;
            mnl.Matrix<double> kGlobalManual = mnl.Matrix<double>.Build.Dense(0, 12);

            double[] r0 = new double[] { 0.5, 0.125, 0, -0.25, -0.125, 0, -0.25, -0.125, 0, 0, 0.125, 0 };
            double[] r1 = new double[] { 0.125, 0.5, 0, 0.125, 0, 0, -0.125, -0.25, 0, -0.125, -0.25, 0 };
            double[] r2 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r3 = new double[] { -0.25, 0.125, 0, 0.5, -0.125, 0, 0, -0.125, 0, -0.25, 0.125, 0 };
            double[] r4 = new double[] { -0.125, 0, 0, -0.125, 0.5, 0, 0.125, -0.25, 0, 0.125, -0.25, 0 };
            double[] r5 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r6 = new double[] { -0.25, -0.125, 0, 0, 0.125, 0, 0.5, 0.125, 0, -0.25, -0.125, 0 };
            double[] r7 = new double[] { -0.125, -0.25, 0, -0.125, -0.25, 0, 0.125, 0.5, 0, 0.125, 0, 0 };
            double[] r8 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r9 = new double[] { 0, -0.125, 0, -0.25, 0.125, 0, -0.25, 0.125, 0, 0.5, -0.125, 0 };
            double[] r10 = new double[] { 0.125, -0.25, 0, 0.125, -0.25, 0, -0.125, 0, 0, -0.125, 0.5, 0 };
            double[] r11 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            kGlobalManual = kGlobalManual.InsertRow(0, mnl.Vector<double>.Build.Dense(r0));
            kGlobalManual = kGlobalManual.InsertRow(1, mnl.Vector<double>.Build.Dense(r1));
            kGlobalManual = kGlobalManual.InsertRow(2, mnl.Vector<double>.Build.Dense(r2));
            kGlobalManual = kGlobalManual.InsertRow(3, mnl.Vector<double>.Build.Dense(r3));
            kGlobalManual = kGlobalManual.InsertRow(4, mnl.Vector<double>.Build.Dense(r4));
            kGlobalManual = kGlobalManual.InsertRow(5, mnl.Vector<double>.Build.Dense(r5));
            kGlobalManual = kGlobalManual.InsertRow(6, mnl.Vector<double>.Build.Dense(r6));
            kGlobalManual = kGlobalManual.InsertRow(7, mnl.Vector<double>.Build.Dense(r7));
            kGlobalManual = kGlobalManual.InsertRow(8, mnl.Vector<double>.Build.Dense(r8));
            kGlobalManual = kGlobalManual.InsertRow(9, mnl.Vector<double>.Build.Dense(r9));
            kGlobalManual = kGlobalManual.InsertRow(10, mnl.Vector<double>.Build.Dense(r10));
            kGlobalManual = kGlobalManual.InsertRow(11, mnl.Vector<double>.Build.Dense(r11));

            LinearSolver fem = new LinearSolver(new FiniteElement[] { el });

            //controllo klocale elemento finito 4 nodi stato piano di tensione
            Console.WriteLine("kLGlobal");
            for (int i = 0; i < kGlobalManual.RowCount; i++)
            {
                for (int j = 0; j < kGlobalManual.ColumnCount; j++)
                {
                    Console.Write(kGlobalManual[i, j].ToString("F4") + " ");
                    Assert.AreEqual(kGlobalManual[i, j] - fem.KGlobal[i, j], 0, 0.001, "kGlobal no OK -> row " + i + " col " + j);
                    //sarebbe stato meglio usare kLocal[i,j] / kLocalManual[i,j] ma 0/0 = NaN!!
                }
                Console.WriteLine();
            }
        }
#endif

        [TestMethod]
        public void Quad4MembranalTest3()
        {
            double E = 30000.0;
            double ni = 0.25;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0, 0, 1, "1"));
            nds.Add(new Node(12.0, 0, 0, 1, "2"));
            nds.Add(new Node(24.0, 0, 0, 1, "3"));
            nds.Add(new Node(36.0, 0, 0, 1, "4"));
            nds.Add(new Node(48.0, 0, 0, 1, "5"));

            nds.Add(new Node(0.0, 12.0, 0, 1, "6"));
            nds.Add(new Node(12.0, 12.0, 0, 1, "7"));
            nds.Add(new Node(24.0, 12.0, 0, 1, "8"));
            nds.Add(new Node(36.0, 12.0, 0, 1, "9"));
            nds.Add(new Node(48.0, 12.0, 0, 1, "10"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute(freedomCase, sys);
            hinge.AddExternalRestrain(LinearSolver.DOF.DX);
            hinge.AddExternalRestrain(LinearSolver.DOF.DY);

            NodeRestrainAttribute dx = new NodeRestrainAttribute(freedomCase, sys);
            dx.AddExternalRestrain(LinearSolver.DOF.DX);

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);

            nds.ForEach(x => x.AddAttribute(shareFix));

            nds[1 - 1].AddAttribute(dx);
            nds[6 - 1].AddAttribute(hinge);

            LoadCase lc = new LoadCase("lc");
            /*double px = 0.1;
            PlatePressureAttribute pressure = new PlatePressureAttribute(lc, sys, px, 0, 0);*/
            NodeForceAttribute F = new NodeForceAttribute(lc, sys, 0, 20.0, 0, 0, 0, 0);
            nds[4].AddAttribute(F);
            nds[9].AddAttribute(F);

            List<Quad4Membranal> els = new List<Quad4Membranal>();
            els.Add(new Quad4Membranal(new Node[] { nds[0], nds[1], nds[6], nds[5] }, prop, 1));
            els.Add(new Quad4Membranal(new Node[] { nds[1], nds[2], nds[7], nds[6] }, prop, 1));
            els.Add(new Quad4Membranal(new Node[] { nds[2], nds[3], nds[8], nds[7] }, prop, 1));
            els.Add(new Quad4Membranal(new Node[] { nds[3], nds[4], nds[9], nds[8] }, prop, 1));

            LinearSolver fem = new LinearSolver(els.ToArray());
            Console.WriteLine("kGlob=" + fem.KGlobal);
            Console.WriteLine("F=" + fem.F);
        }

        [TestMethod]
        public void Quad4MQ2IbraMembranalTest3()
        {
            double E = 30000.0;
            double ni = 0.25;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0, 0, 1, "1"));
            nds.Add(new Node(12.0, 0, 0, 1, "2"));
            nds.Add(new Node(24.0, 0, 0, 1, "3"));
            nds.Add(new Node(36.0, 0, 0, 1, "4"));
            nds.Add(new Node(48.0, 0, 0, 1, "5"));

            nds.Add(new Node(0.0, 12.0, 0, 1, "6"));
            nds.Add(new Node(12.0, 12.0, 0, 1, "7"));
            nds.Add(new Node(24.0, 12.0, 0, 1, "8"));
            nds.Add(new Node(36.0, 12.0, 0, 1, "9"));
            nds.Add(new Node(48.0, 12.0, 0, 1, "10"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute(freedomCase, sys);
            hinge.AddExternalRestrain(LinearSolver.DOF.DX);
            hinge.AddExternalRestrain(LinearSolver.DOF.DY);

            NodeRestrainAttribute dx = new NodeRestrainAttribute(freedomCase, sys);
            dx.AddExternalRestrain(LinearSolver.DOF.DX);

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));

            nds[1-1].AddAttribute(dx);
            nds[6-1].AddAttribute(hinge);  

            LoadCase lc = new LoadCase("lc");
            /*double px = 0.1;
            PlatePressureAttribute pressure = new PlatePressureAttribute(lc, sys, px, 0, 0);*/
            NodeForceAttribute F = new NodeForceAttribute(lc, sys, 0, 20.0, 0, 0, 0, 0);
            nds[4].AddAttribute(F);
            nds[9].AddAttribute(F);

            List<Quad4MQ2IbraMembranal> els = new List<Quad4MQ2IbraMembranal>();
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[0], nds[1], nds[6], nds[5] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[1], nds[2], nds[7], nds[6] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[2], nds[3], nds[8], nds[7] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[3], nds[4], nds[9], nds[8] }, prop, 1));

            LinearSolver fem = new LinearSolver(els.ToArray());
            Console.WriteLine("kGlob="+fem.KGlobal);
            Console.WriteLine("F="+fem.F);

            //Check force applied

            //check stress
            /*Console.WriteLine("stress");
            double[] elGlobalDispl = fem.GetDisplacementsGlobalCoordinates(el);
            el.GetNodesResults(elGlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);*/

            //Console.WriteLine(globalStress[0]);

            //Assert.AreEqual(sigmaTopYY, globalStress[0][1, 1], 0.001); //sigmaYY top face
        }
    }
}