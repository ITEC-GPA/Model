using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM;
using GPC.Model.Sections;
using GPC.Model.Materials;
using GPC.Model;
using GPC.Geometry;
using MathNet.Numerics.LinearAlgebra;
using System.IO;
using GPC.Model.Elements;

namespace UnitTest
{

    [TestClass]
    public class FEMTestPlates
    {
        public TestContext TestContext { get; set; }
        private static string _outputFolder;
        private string _testName;

        [TestInitialize]
        public void TestInitialize()
        {
            _outputFolder = System.IO.Path.Combine(Directory.GetParent(TestContext.TestDir).ToString(), "OutputTests");
            Directory.CreateDirectory(_outputFolder);
            _testName = TestContext.TestName;
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (Directory.Exists(TestContext.TestDir))
                Directory.Delete(TestContext.TestDir, true);
        }

        [TestMethod]
        public void Benchmark10001()
        {
            /// Benchmark10001 - Bathe, Numerical Methods in Finite Elements Analysis - Esercizio Nr 5.11 pg 358
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };

            /// Nodes in 3D  XYZ
            int[] Node1DoF = new int[] { 1, 1, 1, 0, 0, 1 };
            int[] Node2DoF = new int[] { 1, 1, 1, 0, 0, 1 };
            int[] Node3DoF = new int[] { 0, 0, 1, 0, 0, 1 };
            int[] Node4DoF = new int[] { 0, 0, 1, 0, 0, 1 };

            Node Node1 = new Node(Guid.NewGuid(), new Point3d(+0.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF);
            Node Node2 = new Node(Guid.NewGuid(), new Point3d(+20, 10, 0.0), 2, NodeDoFID, Node2DoF);
            Node Node3 = new Node(Guid.NewGuid(), new Point3d(+15, 20, 0.0), 3, NodeDoFID, Node3DoF);
            Node Node4 = new Node(Guid.NewGuid(), new Point3d(-20, +20, 0.0), 4, NodeDoFID, Node4DoF);

            Node[] nodes = new Node[4];
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

            CoordinateSystem Csys = new CoordinateSystem(Node1.Position, Node2.Position, Node3.Position, 0, string.Empty, new Guid());

            //Point3d p1 = new Point3d(1, 1, 0);
            //Point3d p2 = new Point3d(3, 4, 0);
            //Point3d p3 = new Point3d(3, 4, 4);
            //Point3d p4 = new Point3d(1, 1, 4);
            //GPC.Model.CoordinateSystems.CoordinateSystem Csys = new GPC.Model.CoordinateSystems.CoordinateSystem(Guid.Empty, p1, p2, p3);
            //Point3d p1local = Csys.PointToLocal(p1);
            //Point3d p2local = Csys.PointToLocal(p2);
            //Point3d p3local = Csys.PointToLocal(p3);
            //Point3d p4local = Csys.PointToLocal(p4);

            ///  Section
            double E = 210000; // MPa
            double ni = 0.3;

            /// Material
            Material mat = new SteelMaterial("Steel", E, ni, 355, 510, 355/E, 0, 0, new Guid());// new Material("Steel", E, ni, 0.0, 0.0, new Guid());
            PlateProperty property = new PlateProperty(mat, 1.00, 1.00);
            //CoordinateSystemPlateQuad4 quad4 = new PlateQuad4(new Guid(), property, nodes);
            PlateDKQ4 shell = new PlateDKQ4(new Guid(), property, nodes);

            Matrix<double> _stiffnessMatrix = Matrix<double>.Build.Dense(_globalDoF, _globalDoF, 0.0);
            shell.ElementIncidence();
            shell.KInGlobal(ref _stiffnessMatrix);



            string TestName = "DKQ_SHELL_STIFF-MATRIX_REDUCED.txt";
            string path = Path.Combine(_outputFolder, TestName);

            // This text is added only once to the file.
            if (File.Exists(path) == true)
            {
                File.Delete(path);
            }
            if (!File.Exists(path))
            {
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

         
            /// Costruzione vettore delle forze esterne
            Vector<double> Fmaffem = Vector<double>.Build.Dense(12, 0);
            Fmaffem[4] = 10e3;

            /// Solve Linear System
            Vector<double> ResultsMAFFEM = _stiffnessMatrix.Solve(Fmaffem);
            double DX = ResultsMAFFEM[4];
            double DY = ResultsMAFFEM[5];

            double test = 0.0;
            double test1 = test;
        }

        [TestMethod]
        public void Benchmark10002()
        {
            /// Benchmark10001 - Bathe, Numerical Methods in Finite Elements Analysis - Esercizio Nr 5.11 pg 358
            /// 0 - active degree of freedom
            /// 1 - non-active degree of freedom
            int[] NodeDoFID = new int[] { 1, 2, 3, 4, 5, 6 };

            /// Nodes in 3D  XYZ
            int[] Node1DoF = new int[] { 1, 1, 1, 0, 0, 1 };
            int[] Node2DoF = new int[] { 1, 1, 1, 0, 0, 1 };
            int[] Node3DoF = new int[] { 0, 0, 0, 0, 0, 1 };
            int[] Node4DoF = new int[] { 0, 0, 1, 0, 0, 1 };

            Node Node1 = new Node(Guid.NewGuid(), new Point3d(+0.0, 0.0, 0.0), 1, NodeDoFID, Node1DoF);
            Node Node2 = new Node(Guid.NewGuid(), new Point3d(+20, 10, 0.0), 2, NodeDoFID, Node2DoF);
            Node Node3 = new Node(Guid.NewGuid(), new Point3d(+15, 20, 0.0), 3, NodeDoFID, Node3DoF);
            Node Node4 = new Node(Guid.NewGuid(), new Point3d(-20, +20, 0.0), 4, NodeDoFID, Node4DoF);

            Node[] nodes = new Node[4];
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

            CoordinateSystem Csys = new CoordinateSystem(Node1.Position, Node2.Position, Node3.Position, 0, string.Empty, new Guid());

            //Point3d p1 = new Point3d(1, 1, 0);
            //Point3d p2 = new Point3d(3, 4, 0);
            //Point3d p3 = new Point3d(3, 4, 4);
            //Point3d p4 = new Point3d(1, 1, 4);
            //GPC.Model.CoordinateSystems.CoordinateSystem Csys = new GPC.Model.CoordinateSystems.CoordinateSystem(Guid.Empty, p1, p2, p3);
            //Point3d p1local = Csys.PointToLocal(p1);
            //Point3d p2local = Csys.PointToLocal(p2);
            //Point3d p3local = Csys.PointToLocal(p3);
            //Point3d p4local = Csys.PointToLocal(p4);

            ///  Section
            double E = 210000; // MPa
            double ni = 0.3;

            /// Material
            Material mat = new SteelMaterial("Steel",E,ni,355,510,355/E,0.0,0.0,new Guid());//new Material("Steel", E, ni, 0.0, 0.0, new Guid());
            PlateProperty property = new PlateProperty(mat, 1.00, 1.00);
            //CoordinateSystemPlateQuad4 quad4 = new PlateQuad4(new Guid(), property, nodes);
            PlateDKQ4 shell = new PlateDKQ4(new Guid(), property, nodes);

            Matrix<double> _stiffnessMatrix = Matrix<double>.Build.Dense(_globalDoF, _globalDoF, 0.0);
            shell.ElementIncidence();
            shell.KInGlobal(ref _stiffnessMatrix);


            string TestName = "DKQ_SHELL_STIFF-MATRIX_REDUCED.txt";
            string path = Path.Combine(_outputFolder, TestName);
            // This text is added only once to the file.
            if (File.Exists(path) == true)
            {
                File.Delete(path);
            }
            if (!File.Exists(path))
            {
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


            /// Costruzione vettore delle forze esterne
            Vector<double> Fmaffem = Vector<double>.Build.Dense(_stiffnessMatrix.RowCount, 0);
            Fmaffem[6] = 10e3;

            /// Solve Linear System
            Vector<double> ResultsMAFFEM = _stiffnessMatrix.Solve(Fmaffem);
            double DZ3 = ResultsMAFFEM[6];
            double RX3 = ResultsMAFFEM[7];
            double RY3 = ResultsMAFFEM[8];

            double test = 0.0;
            double test1 = test;
        }
    }
    [TestClass]
    public class FEMTestBeams
    {
        [TestMethod]
        public void Benchmark00001()
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

            /// Material
            double E = 30000; // MPa
            ConcreteMaterial mat = new ConcreteMaterial(E, 0.3, 0, 30);

            ///  Section
            double J = 6.75e8; //mm4
            double A = 300 * 300; //mm2
            Section sec = new Section(mat);
            sec.Area = A;
            sec.J22 = J;
            sec.J11 = J;
            sec.Jt = 0.0;

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

            //string path = "C:\\Users\\r.vochescu\\Desktop\\" + "GLOBAl_K" + ".txt";
            //// This text is added only once to the file.
            //if (File.Exists(path) == true)
            //{
            //    File.Delete(path);
            //}
            //if (!File.Exists(path))
            //{
            //    //File.WriteAllText(path, _stiffnessMatrix.ToString());

            //    string matrix = "";
            //    for (int r = 0; r < _stiffnessMatrix.RowCount; r++)
            //    {
            //        for (int c = 0; c < _stiffnessMatrix.ColumnCount; c++)
            //        {
            //            matrix = matrix + "\t" + _stiffnessMatrix[r, c].ToString();
            //        }
            //        matrix = matrix + Environment.NewLine;
            //    }
            //    File.WriteAllText(path, matrix);
            //}



            /// Costruzione vettore delle forze esterne
            Vector<double> Fmaffem = Vector<double>.Build.Dense(5, 0);
            Fmaffem[0] = 1000e3;

            /// Solve Linear System
            Vector<double> ResultsMAFFEM = _stiffnessMatrix.Solve(Fmaffem);
            double DXfem = ResultsMAFFEM[0];

            double DXexpected = 900.4661;
            double toll = Math.Pow(10, -4);
            Assert.IsTrue((DXfem - DXexpected) < toll);
        }

        [TestMethod]
        public void Benchmark00002()
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

            /// Material
            double E = 30000; // MPa
            ConcreteMaterial mat = new ConcreteMaterial(E, 0.3, 0, 30);

            ///  Section
            double J = 6.75e8; //mm4
            double A = 300 * 300; //mm2
            Section sec = new Section(mat);
            sec.Area = A;
            sec.J22 = J;
            sec.J11 = J;
            sec.Jt = 0.0;         

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

            //string path = "C:\\Users\\r.vochescu\\Desktop\\" + "GLOBAl_K" + ".txt";
            //// This text is added only once to the file.
            //if (File.Exists(path) == true)
            //{
            //    File.Delete(path);
            //}
            //if (!File.Exists(path))
            //{
            //    //File.WriteAllText(path, _stiffnessMatrix.ToString());

            //    string matrix = "";
            //    for (int r = 0; r < _stiffnessMatrix.RowCount; r++)
            //    {
            //        for (int c = 0; c < _stiffnessMatrix.ColumnCount; c++)
            //        {
            //            matrix = matrix + "\t" + _stiffnessMatrix[r, c].ToString();
            //        }
            //        matrix = matrix + Environment.NewLine;
            //    }
            //    File.WriteAllText(path, matrix);
            //}

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

            double DXexpected = 593.2735;
            double DYexpected = -471.9221;

            double toll = Math.Pow(10,-4);
           Assert.IsTrue((DX - DXexpected) < toll && (DY - DYexpected) < toll);
        }
    }
}
