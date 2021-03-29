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

namespace UnitTest.FEM
{
    class OldFemTest
    {
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
        public void Benchmark10004()
        {
            /// Benchmark10004
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };

            /// Nodes in 3D  XYZ
            int[] Node1DoF = new int[] { 0, 0, 0, 0, 0, 0 };
            int[] Node2DoF = new int[] { 0, 0, 0, 0, 0, 0 };
            int[] Node3DoF = new int[] { 0, 0, 0, 0, 0, 0 };
            int[] Node4DoF = new int[] { 0, 0, 0, 0, 0, 0 };

            GPC.Model.FEMOld.Node Node1 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(0.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF);
            GPC.Model.FEMOld.Node Node2 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(+1.0, 0.0, 0.0), 2, NodeDoFID, Node2DoF);
            GPC.Model.FEMOld.Node Node3 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(+2.0, +2.0, 0.0), 3, NodeDoFID, Node3DoF);
            GPC.Model.FEMOld.Node Node4 = new GPC.Model.FEMOld.Node(Guid.NewGuid(), new Point3d(0.0, +1.0, 0.0), 4, NodeDoFID, Node4DoF);

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

            Console.WriteLine("Stiffness matrix");
            for (int r = 0; r < _stiffnessMatrix.RowCount; r++)
            {
                for (int c = 0; c < _stiffnessMatrix.ColumnCount; c++)
                {
                    Console.Write(_stiffnessMatrix[r, c].ToString("F3") + " ");
                }
                Console.WriteLine();
            }
        }

        /*[TestMethod]
        public void Quad4MQ2IbraMembranalOldFem1()
        {
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };

            /// Nodes in 3D  XYZ
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
        }*/

        /*
        [TestMethod]
        public void Quad4MQ2IbraMembranalOldFem2()
        {
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
            
        }*/

        /*[TestMethod]
        public void Quad4MQ2IbraMembranalOldFem1()
        {
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };

            /// Nodes in 3D  XYZ
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
        }*/

        /*
        [TestMethod]
        public void Quad4MQ2IbraMembranalOldFem2()
        {
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
            
        }*/

        /*[TestMethod]
        public void Quad4MQ2IbraMembranalOldFem1()
        {
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };

            /// Nodes in 3D  XYZ
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
        }*/

        /*
        [TestMethod]
        public void Quad4MQ2IbraMembranalOldFem2()
        {
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
            
        }*/
    }
}
