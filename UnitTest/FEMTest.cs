using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM;
using GPC.Model.Sections;
using GPC.Model.Materials;
using GPC.Model;
using GPC.Geometry;
using MathNet.Numerics.LinearAlgebra;
using System.IO;

namespace UnitTest
{
    [TestClass]
    public class FEMTest
    {
        [TestMethod]
        public void Benchmark1()
        {

            /// Nodes DoF
            /// Bathe Convention
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };


            /// Nodes in 3D  XYZ
            int[] Node1DoF = new int[] { 1, 1, 1, 1, 1, 1 };
            int[] Node2DoF = new int[] { 0, 1, 0, 1, 0, 1 };
            int[] Node3DoF = new int[] { 0, 1, 1, 1, 0, 1 };
            Node Node1 = new Node(Guid.NewGuid(), new Point3d(0.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF);
            Node Node2 = new Node(Guid.NewGuid(), new Point3d(0.0, 0.0, 5000.0), 2, NodeDoFID, Node2DoF);
            Node Node3 = new Node(Guid.NewGuid(), new Point3d(5000.0, 0.0, 5000.0), 2, NodeDoFID, Node3DoF);


            /// Nodes in 2D (XY)
            //int[] Node1DoF = new int[] { 1, 1, 1, 1, 1, 1 };
            //int[] Node2DoF = new int[] { 0, 0, 1, 1, 1, 0 };
            //int[] Node3DoF = new int[] { 0, 1, 1, 1, 1, 0 };
            //Node Node1 = new Node(new Guid(), new Point3d(0.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF);
            //Node Node2 = new Node(new Guid(), new Point3d(0.0, 5000.0, 0.0), 2, NodeDoFID, Node2DoF);
            //Node Node3 = new Node(new Guid(), new Point3d(5000.0, 5000.0, 0.0), 2, NodeDoFID, Node3DoF);

            ///  Section
            double E = 30000; // MPa
            double J = 6.75e8; //mm4
            double A = 300 * 300; //mm2
            Section sec = new Section();
            sec.Area = A;
            sec.I11 = J;
            sec.I22 = J;
            sec.J = 0.0;

            /// Material
            ConcreteMaterial mat = new ConcreteMaterial(E, 0.3, 0, 30);

            /// Beams
            Node[] NodesB1 = new Node[] { Node1, Node2 };
            Node[] NodesB2 = new Node[] { Node2, Node3 };
            Beam Beam1 = new Beam(Guid.NewGuid(), sec, mat, new FEMBeamIntegrator(Guid.NewGuid()), NodesB1);
            Beam Beam2 = new Beam(Guid.NewGuid(), sec, mat, new FEMBeamIntegrator(Guid.NewGuid()), NodesB2);

            Node[] Nodes = new Node[] { Node1, Node2, Node3 };
            Beam[] Beams = new Beam[] { Beam1, Beam2 };

            int _globalDoF = 0;
            int _reactionDoF = 0;

            // Arrange Nodes
            for (int nd = 0; nd < Nodes.Length; nd++)
            {
                Nodes[nd].DoF.FormIncidence(ref _globalDoF, ref _reactionDoF);
            }
            // Arrange Beam Elements
            for (int bm = 0; bm < Beams.Length; bm++)
            {
                /// Choose Integrator
                Beams[bm].ChooseIntegrator();

                /// Create Incidence
                Beams[bm].ElementIncidence();
            }

            Matrix<double> _stiffnessMatrix = Matrix<double>.Build.Dense(_globalDoF, _globalDoF, 0.0);

            for (int el = 0; el < Beams.Length; el++)
            {
                Beams[el].KInGlobal(ref _stiffnessMatrix);
            }

            string path = "C:\\Users\\r.vochescu\\Desktop\\" + "GLOBAl_K" + ".txt";
            // This text is added only once to the file.
            if (File.Exists(path) == true)
            {
                File.Delete(path);
            }
            if (!File.Exists(path))
            {
                //File.WriteAllText(path, _stiffnessMatrix.ToString());

                string matrix = "";
                for (int r = 0; r < _stiffnessMatrix.RowCount; r++)
                {
                    for (int c = 0; c < _stiffnessMatrix.ColumnCount; c++)
                    {
                        matrix = matrix + "\t" + _stiffnessMatrix[r, c].ToString();
                    }
                    matrix = matrix + Environment.NewLine;
                }
                File.WriteAllText(path, matrix);
            }

            double k11 = _stiffnessMatrix[0, 0];
            double k55 = _stiffnessMatrix[4, 4];
            double k41 = _stiffnessMatrix[3, 0];

            double ciao = 0;
            double ciao1 = ciao;


            /// Costruzione vettore delle forze esterne
            Vector<double> Fmaffem = Vector<double>.Build.Dense(5, 0);
            Fmaffem[0] = 1000e3;

            /// Solve Linear System
            Vector<double> ResultsMAFFEM = _stiffnessMatrix.Solve(Fmaffem);
            double DXhand = ResultsMAFFEM[0];
        }

        [TestMethod]
        public void Benchmark2()
        {

            /// Nodes DoF
            /// Bathe Convention
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };


            /// Nodes in 3D  XYZ
            int[] Node1DoF = new int[] { 1, 1, 1, 1, 1, 1 };
            int[] Node2DoF = new int[] { 0, 1, 0, 1, 0, 1 };
            int[] Node3DoF = new int[] { 0, 1, 1, 1, 0, 1 };
            Node Node1 = new Node(Guid.NewGuid(), new Point3d(0.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF);
            Node Node2 = new Node(Guid.NewGuid(), new Point3d(4000.0, 0.0, 5000.0), 2, NodeDoFID, Node2DoF);
            Node Node3 = new Node(Guid.NewGuid(), new Point3d(9000.0, 0.0, 5000.0), 2, NodeDoFID, Node3DoF);


            /// Nodes in 2D (XY)
            //int[] Node1DoF = new int[] { 1, 1, 1, 1, 1, 1 };
            //int[] Node2DoF = new int[] { 0, 0, 1, 1, 1, 0 };
            //int[] Node3DoF = new int[] { 0, 1, 1, 1, 1, 0 };
            //Node Node1 = new Node(new Guid(), new Point3d(0.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF);
            //Node Node2 = new Node(new Guid(), new Point3d(0.0, 5000.0, 0.0), 2, NodeDoFID, Node2DoF);
            //Node Node3 = new Node(new Guid(), new Point3d(5000.0, 5000.0, 0.0), 2, NodeDoFID, Node3DoF);

            ///  Section
            double E = 30000; // MPa
            double J = 6.75e8; //mm4
            double A = 300 * 300; //mm2
            Section sec = new Section();
            sec.Area = A;
            sec.I11 = J;
            sec.I22 = J;
            sec.J = 0.0;

            /// Material
            ConcreteMaterial mat = new ConcreteMaterial(E, 0.3, 0, 30);

            /// Beams
            Node[] NodesB1 = new Node[] { Node1, Node2 };
            Node[] NodesB2 = new Node[] { Node2, Node3 };
            Beam Beam1 = new Beam(Guid.NewGuid(), sec, mat, new FEMBeamIntegrator(Guid.NewGuid()), NodesB1);
            Beam Beam2 = new Beam(Guid.NewGuid(), sec, mat, new FEMBeamIntegrator(Guid.NewGuid()), NodesB2);

            Node[] Nodes = new Node[] { Node1, Node2, Node3 };
            Beam[] Beams = new Beam[] { Beam1, Beam2 };

            int _globalDoF = 0;
            int _reactionDoF = 0;

            // Arrange Nodes
            for (int nd = 0; nd < Nodes.Length; nd++)
            {
                Nodes[nd].DoF.FormIncidence(ref _globalDoF, ref _reactionDoF);
            }
            // Arrange Beam Elements
            for (int bm = 0; bm < Beams.Length; bm++)
            {
                /// Choose Integrator
                Beams[bm].ChooseIntegrator();

                /// Create Incidence
                Beams[bm].ElementIncidence();
            }

            Matrix<double> _stiffnessMatrix = Matrix<double>.Build.Dense(_globalDoF, _globalDoF, 0.0);

            for (int el = 0; el < Beams.Length; el++)
            {
                Beams[el].KInGlobal(ref _stiffnessMatrix);
            }

            string path = "C:\\Users\\r.vochescu\\Desktop\\" + "GLOBAl_K" + ".txt";
            // This text is added only once to the file.
            if (File.Exists(path) == true)
            {
                File.Delete(path);
            }
            if (!File.Exists(path))
            {
                //File.WriteAllText(path, _stiffnessMatrix.ToString());

                string matrix = "";
                for (int r = 0; r < _stiffnessMatrix.RowCount; r++)
                {
                    for (int c = 0; c < _stiffnessMatrix.ColumnCount; c++)
                    {
                        matrix = matrix + "\t" + _stiffnessMatrix[r, c].ToString();
                    }
                    matrix = matrix + Environment.NewLine;
                }
                File.WriteAllText(path, matrix);
            }

            double k11 = _stiffnessMatrix[0, 0];
            double k55 = _stiffnessMatrix[4, 4];
            double k41 = _stiffnessMatrix[3, 0];

            double ciao = 0;
            double ciao1 = ciao;


            /// Costruzione vettore delle forze esterne
            Vector<double> Fmaffem = Vector<double>.Build.Dense(5, 0);
            Fmaffem[0] = 1000e3;

            /// Solve Linear System
            Vector<double> ResultsMAFFEM = _stiffnessMatrix.Solve(Fmaffem);
            double DX = ResultsMAFFEM[0];
            double DY = ResultsMAFFEM[1];

            double DXcheck = 593.2735;
            double DYcheck = -471.9221;


           //Assert.IsTrue(result == expected, message);
        }
    }
}
