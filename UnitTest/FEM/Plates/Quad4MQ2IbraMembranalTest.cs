using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM;
using mnl = MathNet.Numerics.LinearAlgebra;
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
        [TestMethod]
        public void GetIndexCalculationTest1()
        {
            for (int i = 1; i <= 4; i++)
            {
                Quad4MQ2IbraMembranal.GetIndexCalculation(i, out int j, out int k, out int l, out int m);
                if (i == 1)
                {   
                    Assert.AreEqual(5, m);
                    Assert.AreEqual(8, l);
                    Assert.AreEqual(2, k);
                    Assert.AreEqual(4, j);
                }
                if (i == 2)
                {   
                    Assert.AreEqual(6, m);
                    Assert.AreEqual(5, l);
                    Assert.AreEqual(3, k);
                    Assert.AreEqual(1, j);
                }
                if (i == 3)
                {   
                    Assert.AreEqual(7, m);
                    Assert.AreEqual(6, l);
                    Assert.AreEqual(4, k);
                    Assert.AreEqual(2, j);
                }
                if (i == 4)
                {
                    Assert.AreEqual(8, m);
                    Assert.AreEqual(7, l);
                    Assert.AreEqual(1, k);
                    Assert.AreEqual(3, j);
                }
                Console.WriteLine("i="+i + " j="+j + " k="+k+" l="+l+" m="+m);
            }
        }

        [TestMethod]
        public void LijTest1()
        {
            Point2d p = new Point2d(0.5, 0.5);
            Point2d p1 = new Point2d(10.5, 0.5);
            Point2d p2 = new Point2d(0.5, 20.5);
            Point2d p3 = new Point2d(Math.Sqrt(2.0)+0.5, Math.Sqrt(2.0)+0.5);

            Assert.AreEqual(10, Quad4MQ2IbraMembranal.Lij(p, p1));
            Assert.AreEqual(20, Quad4MQ2IbraMembranal.Lij(p, p2));
            Assert.AreEqual(2, Quad4MQ2IbraMembranal.Lij(p, p3));
        }

        [TestMethod]
        public void XYijTest1()
        {
            Point2d p = new Point2d(0.5, 0.5);
            Point2d p1 = new Point2d(10.5, 0.5);
            Point2d p2 = new Point2d(0.5, 20.5);
            Point2d p3 = new Point2d(Math.Sqrt(2.0) + 0.5, Math.Sqrt(2.0) + 0.5);

            Assert.AreEqual(10, Quad4MQ2IbraMembranal.Xij(p, p1));
            Assert.AreEqual(0.0, Quad4MQ2IbraMembranal.Xij(p2, p2));
            Assert.AreEqual(Math.Sqrt(2.0), Quad4MQ2IbraMembranal.Xij(p, p3));

            Assert.AreEqual(0.0, Quad4MQ2IbraMembranal.Yij(p, p1));
            Assert.AreEqual(20.0, Quad4MQ2IbraMembranal.Yij(p, p2));
            Assert.AreEqual(Math.Sqrt(2.0), Quad4MQ2IbraMembranal.Yij(p, p3));
        }

        [TestMethod]
        public void GetDataCalculation()
        {
            Node[] nodes = new Node[]{
                new Node(0.0, 0.0, 0.0, 0),
                new Node(1.0, 0.0, 0.0, 1),
                new Node(1.0, 1.0, 0.0, 2),
                new Node(0.0, 1.0, 0.0, 3),
            };

            for (int i = 1; i < 4; i++) {
                Quad4MQ2IbraMembranal.GetDataCalculation(i, nodes, out double lij, out double sij, out double cij, out double lik, out double sik, out double cik, out int j, out int k, out int m, out int l);

                if (i == 1)
                {
                    //with node i+1
                    Assert.AreEqual(0.0, cik); //x
                    Assert.AreEqual(-1.0, sik); //y

                    //with node i-1
                    Assert.AreEqual(-1.0, cik); //x
                    Assert.AreEqual(0.0, sik); //y
                }
                if (i == 2)
                {
                    //with node i+1
                    Assert.AreEqual(1.0, cik); //x
                    Assert.AreEqual(0.0, sik); //y

                    //with node i-1
                    Assert.AreEqual(0.0, cik); //x
                    Assert.AreEqual(-1.0, sik); //y
                }
                if (i == 3)
                {
                    //with node i+1
                    Assert.AreEqual(0.0, cik); //x
                    Assert.AreEqual(1.0, sik); //y

                    //with node i-1
                    Assert.AreEqual(1.0, cik); //x
                    Assert.AreEqual(0.0, sik); //y
                }
                if (i == 4)
                {
                    //with node i+1
                    Assert.AreEqual(-1.0, cik); //x
                    Assert.AreEqual(0.0, sik); //y

                    //with node i-1
                    Assert.AreEqual(0.0, cik); //x
                    Assert.AreEqual(1.0, sik); //y
                }
            }

            ////////////////////////////////////////////

            nodes = new Node[]{
                new Node(0.0, 0.0, 0.0, 0),
                new Node(2.0, 0.0, 0.0, 1),
                new Node(1.0, 1.0, 0.0, 2),
                new Node(0.0, 1.0, 0.0, 3),
            };

            for (int i = 1; i < 4; i++)
            {
                Quad4MQ2IbraMembranal.GetDataCalculation(i, nodes, out double lij, out double sij, out double cij, out double lik, out double sik, out double cik, out int j, out int k, out int m, out int l);

                if (i == 1)
                {
                    //with node i+1
                    Assert.AreEqual(0.0, cik); //x
                    Assert.AreEqual(-1.0, sik); //y

                    //with node i-1
                    Assert.AreEqual(-1.0, cik); //x
                    Assert.AreEqual(0.0, sik); //y
                }
                if (i == 2)
                {
                    //with node i+1
                    Assert.AreEqual(Math.Sqrt(2.0), cik); //x
                    Assert.AreEqual(Math.Sqrt(2.0), sik); //y

                    //with node i-1
                    Assert.AreEqual(0.0, cik); //x
                    Assert.AreEqual(-1.0, sik); //y
                }
                if (i == 3)
                {
                    //with node i+1
                    Assert.AreEqual(0.0, cik); //x
                    Assert.AreEqual(1.0, sik); //y

                    //with node i-1
                    Assert.AreEqual(Math.Sqrt(2.0), cik); //x
                    Assert.AreEqual(Math.Sqrt(2.0), sik); //y
                }
                if (i == 4)
                {
                    //with node i+1
                    Assert.AreEqual(-1.0, cik); //x
                    Assert.AreEqual(0.0, sik); //y

                    //with node i-1
                    Assert.AreEqual(0.0, cik); //x
                    Assert.AreEqual(1.0, sik); //y
                }
            }
        }

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
            el.BuildMatrix();
            mnl.Matrix<double> k1 = el.KElementGlobalCoord;

            Console.WriteLine("k1 ");
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
        public void Quad4MQ2IbraMembranalTest1a()
        {
            double E = 1.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            List<Node> nds = new List<Node>();
            nds.Add( new Node(+0.0, +0.0, 0, 1, "1"));
            nds.Add( new Node(+1.0, +0.0, 0, 2, "2"));
            nds.Add( new Node(+1.0, +1.0, 0, 3, "3"));
            nds.Add( new Node(+0.0, +1.0, 0, 4, "4"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute(freedomCase, sys);
            hinge.AddExternalRestrain(LinearSolver.DOF.DX);
            hinge.AddExternalRestrain(LinearSolver.DOF.DY);

            /*NodeRestrainAttribute dx = new NodeRestrainAttribute(freedomCase, sys);
            dx.AddExternalRestrain(LinearSolver.DOF.DX);*/

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));

            nds[1 - 1].AddAttribute(hinge);
            nds[4 - 1].AddAttribute(hinge);
            //nds[4 - 1].AddAttribute(dx);

            LoadCase lc = new LoadCase("lc");

            NodeForceAttribute F = new NodeForceAttribute(lc, sys, 1.0, 0.0, 0, 0, 0, 0);
            nds[2 - 1].AddAttribute(F);
            nds[3 - 1].AddAttribute(F);

            List<Quad4MQ2IbraMembranal> els = new List<Quad4MQ2IbraMembranal>();
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[0], nds[1], nds[2], nds[3] }, prop, 1));

            LinearSolver fem = new LinearSolver(els.ToArray());
        }

        [TestMethod]
        public void Quad4MQ2IbraMembranalTest2()
        {
            double E = 1.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(+0.0, +0.0, 0, 1, "1"));
            nds.Add(new Node(+2.0, +0.0, 0, 2, "2"));
            nds.Add(new Node(+1.0, +1.0, 0, 3, "3"));
            nds.Add(new Node(+0.0, +1.0, 0, 4, "4"));

            Quad4MQ2IbraMembranal el = new Quad4MQ2IbraMembranal(nds.ToArray(), prop, 1);

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

            nds[1 - 1].AddAttribute(hinge);
            nds[4 - 1].AddAttribute(dx);

            LoadCase lc = new LoadCase("lc");
            
            NodeForceAttribute F = new NodeForceAttribute(lc, sys, 10.0, 0.0, 0, 0, 0, 0);
            nds[2-1].AddAttribute(F);
            nds[3-1].AddAttribute(F);

            List<Quad4MQ2IbraMembranal> els = new List<Quad4MQ2IbraMembranal>();
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[0], nds[1], nds[2], nds[3] }, prop, 1));

            LinearSolver fem = new LinearSolver(els.ToArray());
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
        public void Quad4MQ2IbraMembranalOldFem2()
        {
            /*

            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };

            /// Nodes in 3D  XYZ
            int[] Node1DoF = new int[] { 0, 0, 1, 1, 1, 0 };
            int[] Node2DoF = new int[] { 0, 0, 1, 1, 1, 0 };
            int[] Node3DoF = new int[] { 0, 0, 1, 1, 1, 0 };
            int[] Node4DoF = new int[] { 0, 0, 1, 1, 1, 0 };

            List<GPC.Model.FEMOld.Node> nodes = new List<GPC.Model.FEMOld.Node>();
            nodes.Add(new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(+0.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF));
            nodes.Add(new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(12.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF));
            nodes.Add(new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(24.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF));
            nodes.Add(new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(36.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF));
            nodes.Add(new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(48.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF));

            nodes.Add(new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(+0.0, 12.0, 0.0), 1, NodeDoFID, Node1DoF));
            nodes.Add(new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(12.0, 12.0, 0.0), 1, NodeDoFID, Node1DoF));
            nodes.Add(new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(24.0, 12.0, 0.0), 1, NodeDoFID, Node1DoF));
            nodes.Add(new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(36.0, 12.0, 0.0), 1, NodeDoFID, Node1DoF));
            nodes.Add(new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(48.0, 12.0, 0.0), 1, NodeDoFID, Node1DoF));

            int _globalDoF = 0;
            int _reactionDoF = 0;

            // Arrange Nodes
            for (int nd = 0; nd < nodes.Count; nd++)
            {
                nodes[nd].DoF.FormIncidence(ref _globalDoF, ref _reactionDoF);
            }

            ///  Section
            double E = 30000; // MPa
            double ni = 0.25;

            /// Material
            Material mat = new SteelMaterial("Steel", E, ni, 355, 510, 355 / E, 0, 0, new Guid());// new Material("Steel", E, ni, 0.0, 0.0, new Guid());
            PlateProperty property = new PlateProperty(mat, 1.0, 1.0);
            GPC.Model.FEMOld.PlateDKQ shell = new GPC.Model.FEMOld.PlateDKQ(new Guid(), property, 1, nodes.ToArray());

            mnl.Matrix<double> _stiffnessMatrix = mnl.Matrix<double>.Build.Dense(_globalDoF, _globalDoF, 0.0);
            shell.BuildElementDoFIncidence();
            shell.KInGlobal(ref _stiffnessMatrix);
            Util.WriteMatrix(_stiffnessMatrix, "F3");
            */
        }

        [TestMethod]
        public void Quad4MembranalTestAsReference()
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
            NodeForceAttribute F = new NodeForceAttribute(lc, sys, 0, 20.0, 0, 0, 0, 0);
            nds[4].AddAttribute(F);
            nds[9].AddAttribute(F);

            List<Quad4Membranal> els = new List<Quad4Membranal>();
            els.Add(new Quad4Membranal(new Node[] { nds[0], nds[1], nds[6], nds[5] }, prop, 1));
            els.Add(new Quad4Membranal(new Node[] { nds[1], nds[2], nds[7], nds[6] }, prop, 1));
            els.Add(new Quad4Membranal(new Node[] { nds[2], nds[3], nds[8], nds[7] }, prop, 1));
            els.Add(new Quad4Membranal(new Node[] { nds[3], nds[4], nds[9], nds[8] }, prop, 1));

            LinearSolver fem = new LinearSolver(els.ToArray());
        }

        /// <summary>
        /// A cantilever beam - A robust quadrilateral membrane finite element with drilling degrees of freedom - adnan ibrahimbegovic, taylor, wilson - 1990
        /// </summary>
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
            NodeForceAttribute F = new NodeForceAttribute(lc, sys, 0, 20.0, 0, 0, 0, 0);
            nds[4].AddAttribute(F);
            nds[9].AddAttribute(F);

            List<Quad4MQ2IbraMembranal> els = new List<Quad4MQ2IbraMembranal>();
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[0], nds[1], nds[6], nds[5] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[1], nds[2], nds[7], nds[6] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[2], nds[3], nds[8], nds[7] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[3], nds[4], nds[9], nds[8] }, prop, 1));

            LinearSolver fem = new LinearSolver(els.ToArray());

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

        /// <summary>
        /// A cantilever beam - A robust quadrilateral membrane finite element with drilling degrees of freedom - adnan ibrahimbegovic, taylor, wilson - 1990
        /// </summary>
        [TestMethod]
        public void Quad4MQ2IbraMembranalTest3a()
        {
            double E = 30000.0;
            double ni = 0.25;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0, 0, 1, "1"));
            nds.Add(new Node(6.0, 0, 0, 1, "2"));
            nds.Add(new Node(12.0, 0, 0, 1, "3"));
            nds.Add(new Node(18.0, 0, 0, 1, "4"));
            nds.Add(new Node(24.0, 0, 0, 1, "5"));
            nds.Add(new Node(30.0, 0, 0, 1, "6"));
            nds.Add(new Node(36.0, 0, 0, 1, "7"));
            nds.Add(new Node(42.0, 0, 0, 1, "8"));
            nds.Add(new Node(48.0, 0, 0, 1, "9"));

            nds.Add(new Node(0.0, 12.0, 0, 1, "10"));
            nds.Add(new Node(6.0, 12.0, 0, 1, "11"));
            nds.Add(new Node(12.0, 12.0, 0, 1, "12"));
            nds.Add(new Node(18.0, 12.0, 0, 1, "13"));
            nds.Add(new Node(24.0, 12.0, 0, 1, "14"));
            nds.Add(new Node(30.0, 12.0, 0, 1, "15"));
            nds.Add(new Node(36.0, 12.0, 0, 1, "16"));
            nds.Add(new Node(42.0, 12.0, 0, 1, "17"));
            nds.Add(new Node(48.0, 12.0, 0, 1, "18"));

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

            nds[1 - 1].AddAttribute(dx);
            nds[10 - 1].AddAttribute(hinge);

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute F = new NodeForceAttribute(lc, sys, 0, 20.0, 0, 0, 0, 0);
            nds[9-1].AddAttribute(F);
            nds[18-1].AddAttribute(F);

            List<Quad4MQ2IbraMembranal> els = new List<Quad4MQ2IbraMembranal>();
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[0], nds[1], nds[10], nds[9] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[1], nds[2], nds[11], nds[10] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[2], nds[3], nds[12], nds[11] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[3], nds[4], nds[13], nds[12] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[4], nds[5], nds[14], nds[13] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[5], nds[6], nds[15], nds[14] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[6], nds[7], nds[16], nds[15] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[7], nds[8], nds[17], nds[16] }, prop, 1));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(0.3553, fem.GetDisplacementGlobalCoordinates(nds[9 - 1], LinearSolver.DOF.DY), 0.025);

            Console.WriteLine("stress");
            double[] elGlobalDispl = fem.GetDisplacementsGlobalCoordinates(els[2-1]);
            els[2-1].GetNodesResults(elGlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);

            mnl.Matrix<double> centroidStress = mnl.Matrix<double>.Build.Dense(3,3);
            globalStress.ToList().ForEach(x => centroidStress = centroidStress + x / 4.0);
            Console.WriteLine(centroidStress);

            //Assert.AreEqual(sigmaTopYY, globalStress[0][1, 1], 0.001); //sigmaYY top face
        }

        /// <summary>
        /// Simple supported beam - Force applied
        /// </summary>
        [TestMethod]
        public void Quad4MQ2IbraMembranalTest4()
        {
            double E = 100.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0 * 10.0 / 6.0, 0, 0, 1, "0"));
            nds.Add(new Node(1.0 * 10.0 / 6.0, 0, 0, 1, "1"));
            nds.Add(new Node(2.0 * 10.0 / 6.0, 0, 0, 1, "2"));
            nds.Add(new Node(3.0 * 10.0 / 6.0, 0, 0, 1, "3"));
            nds.Add(new Node(4.0 * 10.0 / 6.0, 0, 0, 1, "4"));
            nds.Add(new Node(5.0 * 10.0 / 6.0, 0, 0, 1, "5"));
            nds.Add(new Node(6.0 * 10.0 / 6.0, 0, 0, 1, "6"));

            nds.Add(new Node(0.0 * 10.0 / 6.0, 1.0, 0, 1, "7"));
            nds.Add(new Node(1.0 * 10.0 / 6.0, 1.0, 0, 1, "8"));
            nds.Add(new Node(2.0 * 10.0 / 6.0, 1.0, 0, 1, "9"));
            nds.Add(new Node(3.0 * 10.0 / 6.0, 1.0, 0, 1, "10"));
            nds.Add(new Node(4.0 * 10.0 / 6.0, 1.0, 0, 1, "11"));
            nds.Add(new Node(5.0 * 10.0 / 6.0, 1.0, 0, 1, "12"));
            nds.Add(new Node(6.0 * 10.0 / 6.0, 1.0, 0, 1, "13"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute(freedomCase, sys);
            hinge.AddExternalRestrain(LinearSolver.DOF.DX);
            hinge.AddExternalRestrain(LinearSolver.DOF.DY);

            NodeRestrainAttribute dy = new NodeRestrainAttribute(freedomCase, sys);
            dy.AddExternalRestrain(LinearSolver.DOF.DY);

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));
            nds[1 - 1].AddAttribute(dy);
            nds[6 - 1].AddAttribute(hinge);

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute Fplus = new NodeForceAttribute(lc, sys, 1.0, 0.0, 0, 0, 0, 0);
            NodeForceAttribute Fminus = new NodeForceAttribute(lc, sys, -1.0, 0.0, 0, 0, 0, 0);
            nds[0].AddAttribute(Fplus);
            nds[7].AddAttribute(Fminus);

            nds[13].AddAttribute(Fplus);
            nds[6].AddAttribute(Fminus);

            List<Quad4MQ2IbraMembranal> els = new List<Quad4MQ2IbraMembranal>();
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[0], nds[1], nds[8], nds[7] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[1], nds[2], nds[9], nds[8] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[2], nds[3], nds[10], nds[9] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[3], nds[4], nds[11], nds[10] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[4], nds[5], nds[12], nds[11] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[5], nds[6], nds[13], nds[12] }, prop, 1));
            
            LinearSolver fem = new LinearSolver(els.ToArray());

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

        /// <summary>
        /// Simple supported beam - Moment applied
        /// </summary>
        [TestMethod]
        public void Quad4MQ2IbraMembranalTest5()
        {
            double E = 100.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0 * 10.0 / 6.0, 0, 0, 1, "0"));
            nds.Add(new Node(1.0 * 10.0 / 6.0, 0, 0, 1, "1"));
            nds.Add(new Node(2.0 * 10.0 / 6.0, 0, 0, 1, "2"));
            nds.Add(new Node(3.0 * 10.0 / 6.0, 0, 0, 1, "3"));
            nds.Add(new Node(4.0 * 10.0 / 6.0, 0, 0, 1, "4"));
            nds.Add(new Node(5.0 * 10.0 / 6.0, 0, 0, 1, "5"));
            nds.Add(new Node(6.0 * 10.0 / 6.0, 0, 0, 1, "6"));

            nds.Add(new Node(0.0 * 10.0 / 6.0, 1.0, 0, 1, "7"));
            nds.Add(new Node(1.0 * 10.0 / 6.0, 1.0, 0, 1, "8"));
            nds.Add(new Node(2.0 * 10.0 / 6.0, 1.0, 0, 1, "9"));
            nds.Add(new Node(3.0 * 10.0 / 6.0, 1.0, 0, 1, "10"));
            nds.Add(new Node(4.0 * 10.0 / 6.0, 1.0, 0, 1, "11"));
            nds.Add(new Node(5.0 * 10.0 / 6.0, 1.0, 0, 1, "12"));
            nds.Add(new Node(6.0 * 10.0 / 6.0, 1.0, 0, 1, "13"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute(freedomCase, sys);
            hinge.AddExternalRestrain(LinearSolver.DOF.DX);
            hinge.AddExternalRestrain(LinearSolver.DOF.DY);

            NodeRestrainAttribute dy = new NodeRestrainAttribute(freedomCase, sys);
            dy.AddExternalRestrain(LinearSolver.DOF.DY);

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));
            nds[1 - 1].AddAttribute(dy);
            nds[6 - 1].AddAttribute(hinge);

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute Mplus = new NodeForceAttribute(lc, sys, 0.0, 0.0, 0, 0, 0, 0.5);
            NodeForceAttribute Mminus = new NodeForceAttribute(lc, sys, 0.0, 0.0, 0, 0, 0, -0.5);
            nds[0].AddAttribute(Mplus);
            nds[7].AddAttribute(Mplus);
            nds[6].AddAttribute(Mminus);
            nds[13].AddAttribute(Mminus);

            List<Quad4MQ2IbraMembranal> els = new List<Quad4MQ2IbraMembranal>();
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[0], nds[1], nds[8], nds[7] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[1], nds[2], nds[9], nds[8] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[2], nds[3], nds[10], nds[9] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[3], nds[4], nds[11], nds[10] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[4], nds[5], nds[12], nds[11] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[5], nds[6], nds[13], nds[12] }, prop, 1));

            LinearSolver fem = new LinearSolver(els.ToArray());

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

        /// <summary>
        /// Simple supported beam - Force applied
        /// </summary>
        [TestMethod]
        public void Quad4MQ2IbraMembranalTest4a()
        {
            double E = 100.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0, 0, 1, "0"));
            nds.Add(new Node(1.0, 0, 0, 2, "1"));
            nds.Add(new Node(2.0, 0, 0, 3, "2"));
            nds.Add(new Node(3.0, 0, 0, 4, "3"));
            nds.Add(new Node(4.0, 0, 0, 5, "4"));
            nds.Add(new Node(5.0, 0, 0, 6, "5"));
            nds.Add(new Node(6.0, 0, 0, 7, "6"));
            nds.Add(new Node(7.0, 0, 0, 8, "7"));
            nds.Add(new Node(8.0, 0, 0, 9, "8"));
            nds.Add(new Node(9.0, 0, 0, 10, "9"));
            nds.Add(new Node(10.0, 0, 0, 11, "10"));

            nds.Add(new Node(0.0, 1.0, 0, 12, "11"));
            nds.Add(new Node(1.0, 1.0, 0, 13, "12"));
            nds.Add(new Node(2.0, 1.0, 0, 14, "13"));
            nds.Add(new Node(3.0, 1.0, 0, 15, "14"));
            nds.Add(new Node(4.0, 1.0, 0, 16, "15"));
            nds.Add(new Node(5.0, 1.0, 0, 17, "16"));
            nds.Add(new Node(6.0, 1.0, 0, 18, "17"));
            nds.Add(new Node(7.0, 1.0, 0, 19, "18"));
            nds.Add(new Node(8.0, 1.0, 0, 20, "19"));
            nds.Add(new Node(9.0, 1.0, 0, 21, "20"));
            nds.Add(new Node(10.0, 1.0, 0, 22, "21"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute(freedomCase, sys);
            hinge.AddExternalRestrain(LinearSolver.DOF.DX);
            hinge.AddExternalRestrain(LinearSolver.DOF.DY);

            NodeRestrainAttribute dy = new NodeRestrainAttribute(freedomCase, sys);
            dy.AddExternalRestrain(LinearSolver.DOF.DY);

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));
            nds[0].AddAttribute(dy);
            nds[10].AddAttribute(hinge);

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute Fplus = new NodeForceAttribute(lc, sys, 1.0, 0.0, 0, 0, 0, 0);
            NodeForceAttribute Fminus = new NodeForceAttribute(lc, sys, -1.0, 0.0, 0, 0, 0, 0);
            nds[0].AddAttribute(Fplus);
            nds[11].AddAttribute(Fminus);

            nds[21].AddAttribute(Fplus);
            nds[10].AddAttribute(Fminus);

            List<Quad4MQ2IbraMembranal> els = new List<Quad4MQ2IbraMembranal>();
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[0], nds[1], nds[12], nds[11] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[1], nds[2], nds[13], nds[12] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[2], nds[3], nds[14], nds[13] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[3], nds[4], nds[15], nds[14] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[4], nds[5], nds[16], nds[15] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[5], nds[6], nds[17], nds[16] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[6], nds[7], nds[18], nds[17] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[7], nds[8], nds[19], nds[18] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[8], nds[9], nds[20], nds[19] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[9], nds[10], nds[21], nds[20] }, prop, 1));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(1.5, fem.GetDisplacementGlobalCoordinates(nds[16], LinearSolver.DOF.DY), 0.01);
            Assert.AreEqual(0.3, fem.GetDisplacementGlobalCoordinates(nds[16], LinearSolver.DOF.DX), 0.01);
            Assert.AreEqual(0.0, fem.GetDisplacementGlobalCoordinates(nds[16], LinearSolver.DOF.RZ), 0.01);

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

        /// <summary>
        /// Simple supported beam - Moment applied
        /// </summary>
        [TestMethod]
        public void Quad4MQ2IbraMembranalTest5a()
        {
            double E = 100.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0, 0, 1, "0"));
            nds.Add(new Node(1.0, 0, 0, 1, "1"));
            nds.Add(new Node(2.0, 0, 0, 1, "2"));
            nds.Add(new Node(3.0, 0, 0, 1, "3"));
            nds.Add(new Node(4.0, 0, 0, 1, "4"));
            nds.Add(new Node(5.0, 0, 0, 1, "5"));
            nds.Add(new Node(6.0, 0, 0, 1, "6"));
            nds.Add(new Node(7.0, 0, 0, 1, "7"));
            nds.Add(new Node(8.0, 0, 0, 1, "8"));
            nds.Add(new Node(9.0, 0, 0, 1, "9"));
            nds.Add(new Node(10.0, 0, 0, 1, "10"));

            nds.Add(new Node(0.0, 1.0, 0, 1, "11"));
            nds.Add(new Node(1.0, 1.0, 0, 1, "12"));
            nds.Add(new Node(2.0, 1.0, 0, 1, "13"));
            nds.Add(new Node(3.0, 1.0, 0, 1, "14"));
            nds.Add(new Node(4.0, 1.0, 0, 1, "15"));
            nds.Add(new Node(5.0, 1.0, 0, 1, "16"));
            nds.Add(new Node(6.0, 1.0, 0, 1, "17"));
            nds.Add(new Node(7.0, 1.0, 0, 1, "18"));
            nds.Add(new Node(8.0, 1.0, 0, 1, "19"));
            nds.Add(new Node(9.0, 1.0, 0, 1, "20"));
            nds.Add(new Node(10.0, 1.0, 0, 1, "21"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute(freedomCase, sys);
            hinge.AddExternalRestrain(LinearSolver.DOF.DX);
            hinge.AddExternalRestrain(LinearSolver.DOF.DY);

            NodeRestrainAttribute dy = new NodeRestrainAttribute(freedomCase, sys);
            dy.AddExternalRestrain(LinearSolver.DOF.DY);

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));
            nds[0].AddAttribute(dy);
            nds[10].AddAttribute(hinge);

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute Mplus = new NodeForceAttribute(lc, sys, 0.0, 0.0, 0, 0, 0, 0.5);
            NodeForceAttribute Mminus = new NodeForceAttribute(lc, sys, 0.0, 0.0, 0, 0, 0, -0.5);
            nds[0].AddAttribute(Mplus);
            nds[11].AddAttribute(Mplus);

            nds[21].AddAttribute(Mminus);
            nds[10].AddAttribute(Mminus);

            List<Quad4MQ2IbraMembranal> els = new List<Quad4MQ2IbraMembranal>();
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[0], nds[1], nds[12], nds[11] }, prop, 0));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[1], nds[2], nds[13], nds[12] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[2], nds[3], nds[14], nds[13] }, prop, 2));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[3], nds[4], nds[15], nds[14] }, prop, 3));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[4], nds[5], nds[16], nds[15] }, prop, 4));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[5], nds[6], nds[17], nds[16] }, prop, 5));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[6], nds[7], nds[18], nds[17] }, prop, 6));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[7], nds[8], nds[19], nds[18] }, prop, 7));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[8], nds[9], nds[20], nds[19] }, prop, 8));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[9], nds[10], nds[21], nds[20] }, prop, 9));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(1.5, fem.GetDisplacementGlobalCoordinates(nds[16], LinearSolver.DOF.DY), 0.02);
            Assert.AreEqual(0.3, fem.GetDisplacementGlobalCoordinates(nds[16], LinearSolver.DOF.DX), 0.01);
            Assert.AreEqual(0.0, fem.GetDisplacementGlobalCoordinates(nds[16], LinearSolver.DOF.RZ), 0.01);

            //check stress
            Console.WriteLine("stress");
            double[] elGlobalDispl = fem.GetDisplacementsGlobalCoordinates(els[4]);
            els[4].GetNodesResults(elGlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);

            globalStress.ToList().ForEach(x => Console.WriteLine(x));
                        
            //Assert.AreEqual(sigmaTopYY, globalStress[0][1, 1], 0.001); //sigmaYY top face
        }
    }
}