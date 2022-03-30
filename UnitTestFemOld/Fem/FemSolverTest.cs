using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GPC.Model.Fem.FiniteElements;
using GPC.Model.Fem;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.Materials;
using GPC.Model.FreedomCases;
using GPC.Geometry;
using GPC.Model.Fem.Properties;
using GPC.Model.Fem.Attributes;
using GPC.Model.LoadCases;
using GPC.Model.Sections;

namespace FemTest.SolverTest
{
    [TestClass]
    public class GeneralTest
    {
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
        public void AssemblyGlobalMatrixTest1()
        {
            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 0, 1, "p");

            List<Node> nodesPlate1 = new List<Node>();
            nodesPlate1.Add(new Node(0.0, 0, 0, "1"));
            nodesPlate1.Add(new Node(0.0, 100, 0, "2"));
            nodesPlate1.Add(new Node(100.0, 0, 0, "3"));

            List<Node> nodesPlate2 = new List<Node>();
            nodesPlate2.Add(new Node(100.0, 0, 0, "2"));
            nodesPlate2.Add(new Node(0.0, 100, 0, "3"));
            nodesPlate2.Add(new Node(100.0, 100, 0, "4"));

            List<FiniteElement> elements = new List<FiniteElement>();

            elements.Add(new Tri3PlaneStress(nodesPlate1.ToArray(), prop));
            elements.Add(new Tri3PlaneStress(nodesPlate2.ToArray(), prop));

            LinearSolver fem = new LinearSolver(elements.ToArray());
            mnl.Matrix<double> K = fem.KGlobal;

            mnl.Matrix<double> KManual = mnl.Matrix<double>.Build.Dense(0, fem.KGlobal.ColumnCount);
            double[] r0 = new double[] { 145833.3, 62500.0, 0.0, -41666.7, -20833.3, 0.0, -104166.7, -41666.7, 0.0, 0.0, 0.0, 0.0 };
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

            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 0, 1, "p");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute DXDYDZ = new NodeRestrainAttribute("fc", sys);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DX);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DY);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute("fc", sys);
            DZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            List<Node> nodesPlate1 = new List<Node>();
            Node nd1 = new Node(0.0, 0, 0, "1");
            Node nd2 = new Node(0.0, 100, 0, "2");
            Node nd3 = new Node(100.0, 0, 0, "3");

            nd1.AddAttribute(DXDYDZ);
            nd2.AddAttribute(DXDYDZ);

            nodesPlate1.Add(nd1);
            nodesPlate1.Add(nd2);
            nodesPlate1.Add(nd3);

            List<Node> nodesPlate2 = new List<Node>();
            Node nd2copy = new Node(0.0, 100, 0, "2");
            Node nd3copy = new Node(100.0, 0, 0, "3");
            Node nd4 = new Node(100.0, 100, 0, "4");

            nd2copy.AddAttribute(DZ);
            nd3copy.AddAttribute(DZ);
            nd4.AddAttribute(DZ);

            nodesPlate2.Add(nd2copy);
            nodesPlate2.Add(nd3copy);
            nodesPlate2.Add(nd4);

            List<FiniteElement> elements = new List<FiniteElement>();
            elements.Add(new Tri3PlaneStress(nodesPlate1.ToArray(), prop));
            elements.Add(new Tri3PlaneStress(nodesPlate2.ToArray(), prop));

            LinearSolver fem = new LinearSolver(elements.ToArray());
            mnl.Matrix<double> K = fem.KGlobalRestrains;

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
            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 0, 1, "p");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute DXDYDZ = new NodeRestrainAttribute("freedomCase", sys);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DX);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DY);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute("freedomCase", sys);
            DZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            NodeForceAttribute fX1000 = new NodeForceAttribute("loadCase", sys, 1000, 0, 0, 0, 0, 0);

            List<Node> nodesPlate1 = new List<Node>();
            Node nd1 = new Node(0.0, 0, 0, "1");
            Node nd2 = new Node(0.0, 100, 0, "2");
            Node nd3 = new Node(100.0, 0, 0, "3");

            nd1.AddAttribute(DXDYDZ);
            nd2.AddAttribute(DXDYDZ);

            nodesPlate1.Add(nd1);
            nodesPlate1.Add(nd2);
            nodesPlate1.Add(nd3);

            List<Node> nodesPlate2 = new List<Node>();
            Node nd2copy = new Node(0.0, 100, 0, "2");
            Node nd3copy = new Node(100.0, 0, 0, "3");
            Node nd4 = new Node(100.0, 100, 0, "4");

            nd2copy.AddAttribute(DZ);
            nd3copy.AddAttribute(DZ);
            nd4.AddAttribute(DZ);
            nd4.AddAttribute(fX1000);

            nodesPlate2.Add(nd2copy);
            nodesPlate2.Add(nd3copy);
            nodesPlate2.Add(nd4);

            List<FiniteElement> elements = new List<FiniteElement>();
            elements.Add(new Tri3PlaneStress(nodesPlate1.ToArray(), prop));
            elements.Add(new Tri3PlaneStress(nodesPlate2.ToArray(), prop));

            LinearSolver fem = new LinearSolver(elements.ToArray());
            double Node4DX = fem.GetNodeDisplacementGlobalCoordinates(nd4, Solver.DOF.DX);
            double Node4DY = fem.GetNodeDisplacementGlobalCoordinates(nd4, Solver.DOF.DY);

            double Node3DX = fem.GetNodeDisplacementGlobalCoordinates(nd3, Solver.DOF.DX);
            double Node3DY = fem.GetNodeDisplacementGlobalCoordinates(nd3, Solver.DOF.DY);

            double Node3CopyDX = fem.GetNodeDisplacementGlobalCoordinates(nd3copy, Solver.DOF.DX);
            double Node3CopyDY = fem.GetNodeDisplacementGlobalCoordinates(nd3copy, Solver.DOF.DY);

            /*Node 4 Displacement
            DX(mm) 0.009130
            DY(mm) - 0.005478
            DZ(mm) 0.000000*/
            double dXNode4 = 0.009130;
            double dYNode4 = -0.005478;
            Assert.AreEqual(dXNode4, Node4DX, 0.000001);
            Assert.AreEqual(dYNode4, Node4DY, 0.000001);

            /*Node 3 Displacement
            DX(mm) 0.001043
            DY(mm) - 0.002609
            DZ(mm) 0.000000*/
            double dXNode3 = 0.001043;
            double dYNode3 = -0.002609;
            Assert.AreEqual(Node3DX, dXNode3, 0.000001);
            Assert.AreEqual(Node3DY, dYNode3, 0.000001);
            Assert.AreEqual(Node3CopyDX, dXNode3, 0.000001);
            Assert.AreEqual(Node3CopyDY, dYNode3, 0.000001);
        }

        [TestMethod]
        public void AddRestrainAndForceMatrixTest2()
        {
            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 0, 1, "p");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute DXDYDZ = new NodeRestrainAttribute("freedomCase", sys);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DX);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DY);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute("freedomCase", sys);
            DZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            CoordinateSystem sys2 = new CoordinateSystem(new Point3d(1, 1, 0), new Point3d(2, 2, 0), new Point3d(0, 2, 0));
            NodeForceAttribute f1 = new NodeForceAttribute("loadCase", sys, 1000, 0, 0, 0, 0, 0);
            NodeForceAttribute f2 = new NodeForceAttribute("loadCase", sys2, 1000, -500, 0, 0, 0, 0);

            List<Node> nodesPlate1 = new List<Node>();
            Node nd1 = new Node(0.0, 0, 0, "1");
            Node nd2 = new Node(0.0, 100, 0, "2");
            Node nd3 = new Node(100.0, 0, 0, "3");

            nd1.AddAttribute(DXDYDZ);
            nd2.AddAttribute(DXDYDZ);

            nodesPlate1.Add(nd1);
            nodesPlate1.Add(nd2);
            nodesPlate1.Add(nd3);

            List<Node> nodesPlate2 = new List<Node>();
            Node nd2copy = new Node(0.0, 100, 0, "2");
            Node nd3copy = new Node(100.0, 0, 0, "3");
            Node nd4 = new Node(100.0, 100, 0, "4");

            nd2copy.AddAttribute(DZ);
            nd3copy.AddAttribute(DZ);
            nd3copy.AddAttribute(f2);
            nd4.AddAttribute(DZ);
            nd4.AddAttribute(f1);

            nodesPlate2.Add(nd2copy);
            nodesPlate2.Add(nd3copy);
            nodesPlate2.Add(nd4);

            List<FiniteElement> elements = new List<FiniteElement>();
            elements.Add(new Tri3PlaneStress(nodesPlate1.ToArray(), prop));
            elements.Add(new Tri3PlaneStress(nodesPlate2.ToArray(), prop));

            LinearSolver fem = new LinearSolver(elements.ToArray());
            double Node4DX = fem.GetNodeDisplacementGlobalCoordinates(nd4, Solver.DOF.DX);
            double Node4DY = fem.GetNodeDisplacementGlobalCoordinates(nd4, Solver.DOF.DY);

            double Node3DX = fem.GetNodeDisplacementGlobalCoordinates(nd3, Solver.DOF.DX);
            double Node3DY = fem.GetNodeDisplacementGlobalCoordinates(nd3, Solver.DOF.DY);

            double Node3CopyDX = fem.GetNodeDisplacementGlobalCoordinates(nd3copy, Solver.DOF.DX);
            double Node3CopyDY = fem.GetNodeDisplacementGlobalCoordinates(nd3copy, Solver.DOF.DY);

            /*Node 4 Displacement
            DX (mm)	0.009315	
            DY (mm)	0.003745	
            DZ(mm) 0.000000*/
            double dXNode4 = 0.009315;
            double dYNode4 = 0.003745;
            Assert.AreEqual(Node4DX, dXNode4, 0.000001);
            Assert.AreEqual(Node4DY, dYNode4, 0.000001);

            /*Node 3 Displacement
            DX (mm)	0.011004	
            DY (mm)	0.006430
            DZ(mm) 0.000000*/
            double dXNode3 = 0.011004;
            double dYNode3 = 0.00643;
            Assert.AreEqual(Node3DX, dXNode3, 0.000001);
            Assert.AreEqual(Node3DY, dYNode3, 0.000001);
            Assert.AreEqual(Node3CopyDX, dXNode3, 0.000001);
            Assert.AreEqual(Node3CopyDY, dYNode3, 0.000001);
        }

        [TestMethod]
        public void AssemblyMixedElement()
        {
            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("steel", 1, 0.0, 355, 510);
            BrickProperty prop = new BrickProperty(mat.GetIsotropicFemMaterial(), "p");
            double d = 0.5;
            double t = d / 2.0;
            Section sec = new SectionCHS(d, t, mat, "p");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0)); //0
            nds.Add(new Node(1, 0, 0)); //1
            nds.Add(new Node(0, 1, 0)); //2
            nds.Add(new Node(0, 0, 1)); //3
            nds.Add(new Node(0, 0, 2)); //4

            FiniteElement[] els = new FiniteElement[2];
            EulerBeam eulerBeam1 = new EulerBeam(new Node[] { nds[3], nds[4] });
            eulerBeam1.SetProperty(sec);
            els[0] = eulerBeam1;
            els[1] = new Tethraedron4(new Node[] { nds[0], nds[1], nds[2], nds[3] }, prop);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeForceAttribute F = new NodeForceAttribute("loadCase", sys, 10, 0, 0, 0, 0, 0);
            nds[0].AddAttribute(F);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[4].AddAttribute(fix);

            NodeRestrainAttribute dxdydz = new NodeRestrainAttribute("freedomCase", sys);
            dxdydz.AddExternalRestrain(LinearSolver.DOF.DX);
            dxdydz.AddExternalRestrain(LinearSolver.DOF.DY);
            dxdydz.AddExternalRestrain(LinearSolver.DOF.DZ);

            nds[1].AddAttribute(dxdydz);
            nds[2].AddAttribute(dxdydz);

            /*nds[1].AddAttribute(fix);
            nds[2].AddAttribute(fix);*/

            els[0].BuildMatrix();
            Console.WriteLine("Matrix Beam");
            FemUtilities.WriteMatrix(els[0].KElementGlobalCoord, "F3");
            els[1].BuildMatrix();
            Console.WriteLine("Tetraedron");
            FemUtilities.WriteMatrix(els[1].KElementGlobalCoord, "F3");

            LinearSolver fem = new LinearSolver(els);
        }
    }
}