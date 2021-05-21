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

namespace FemTest.SolverTest
{
    [TestClass]
    public class MembraneTest
    {
        [TestMethod]
        public void Tri3PlaneStressKTest1()
        {
            Material mat = new SteelMaterial("steel", 200000.0, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 0, 1, "p");

            Node[] nds = new Node[3];
            nds[0] = new Node(0.0, 0, 0, "1");
            nds[1] = new Node(1.0, 0, 0, "2");
            nds[2] = new Node(0.0, 1, 0, "3");

            Tri3PlaneStress el = new Tri3PlaneStress(nds);
            el.SetProperty(prop);

            el.BuildMatrix();
            mnl.Matrix<double> kLocal = el.KElementLocalCoord;
            mnl.Matrix<double> kLocalManual = mnl.Matrix<double>.Build.Dense(0, 6);
            double[] r0 = new double[] { 145833, 62500, -104167, -41667, -41667, -20833 };
            double[] r1 = new double[] { 62500, 145833, -20833, -41667, -41667, -104167 };
            double[] r2 = new double[] { -104167, -20833, 104167, 0, 0, 20833 };
            double[] r3 = new double[] { -41667, -41667, 0, 41667, 41667, 0 };
            double[] r4 = new double[] { -41667, -41667, 0, 41667, 41667, 0 };
            double[] r5 = new double[] { -20833, -104167, 20833, 0, 0, 104167 };

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
                    //Assert.AreEqual(kLocal[i,j] - kLocalManual[i,j], 0, 1, "kLocal no OK -> row " + i + " col " + j );
                    Console.Write(kLocal[i, j] + " ");
                    //sarebbe stato meglio usare kLocal[i,j] / kLocalManual[i,j] ma 0/0 = NaN!!
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void Tri3PlaneStressKTest2()
        {
            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 0, 1, "p");

            Node[] nds = new Node[3];
            nds[0] = new Node(0.0, 0, 0, "1");
            nds[1] = new Node(100.0, 0, 0, "2");
            nds[2] = new Node(0.0, 100, 0, "3");

            Tri3PlaneStress el = new Tri3PlaneStress(nds);
            el.SetProperty(prop);

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

            Console.WriteLine("dofglobalToLocal = " + el.DofGlobalToLocal);

            //controllo che passaggio da coordinate locali a globali sia fatto corretamente
            Assert.AreEqual(kLocal, kGlobal, "kLocal not equal to Kglobal");
        }

        [TestMethod]
        public void PlatePressureTest1()
        {
            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("steel", 200000, 0.2, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 0, 1, "p");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute DXDYDZ = new NodeRestrainAttribute("freedomCase", sys);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DX);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DY);
            DXDYDZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            NodeRestrainAttribute DZ = new NodeRestrainAttribute("freedomCase", sys);
            DZ.AddExternalRestrain(LinearSolver.DOF.DZ);

            //CoordinateSystem sys2 = new CoordinateSystem(new Point3d(1, 1, 0), new Point3d(2, 2, 0), new Point3d(0, 2, 0));

            List<Node> nodesPlate1 = new List<Node>();
            Node nd1 = new Node(0.0, 0, 0, "1");
            Node nd2 = new Node(0.0, 1, 0, "2");
            Node nd3 = new Node(1.0, 0, 0, "3");

            nd1.AddAttribute(DXDYDZ);
            nd2.AddAttribute(DXDYDZ);

            nodesPlate1.Add(nd1);
            nodesPlate1.Add(nd2);
            nodesPlate1.Add(nd3);

            List<Node> nodesPlate2 = new List<Node>();
            Node nd2copy = new Node(0.0, 1, 0, "2");
            Node nd3copy = new Node(1.0, 0, 0, "3");
            Node nd4 = new Node(1.0, 1, 0, "4");

            nd2copy.AddAttribute(DZ);
            nd3copy.AddAttribute(DZ);
            nd4.AddAttribute(DZ);

            nodesPlate2.Add(nd2copy);
            nodesPlate2.Add(nd3copy);
            nodesPlate2.Add(nd4);

            List<FiniteElement> elements = new List<FiniteElement>();
            Plate e0 = new Tri3PlaneStress(nodesPlate1.ToArray(), prop);
        
            PlatePressureAttribute p = new PlatePressureAttribute("loadCase", sys, -10.0, 0, 0);
            e0.AddLoadCaseAttribute(p);

            elements.Add(e0);
            elements.Add(new Tri3PlaneStress(nodesPlate2.ToArray(), prop));

            LinearSolver fem = new LinearSolver(elements.ToArray());
            double Node4DX = fem.GetNodeDisplacementGlobalCoordinates(nd4, LinearSolver.DOF.DX);
            double Node4DY = fem.GetNodeDisplacementGlobalCoordinates(nd4, LinearSolver.DOF.DY);

            double Node3DX = fem.GetNodeDisplacementGlobalCoordinates(nd3, LinearSolver.DOF.DX);
            double Node3DY = fem.GetNodeDisplacementGlobalCoordinates(nd3, LinearSolver.DOF.DY);

            double Node3CopyDX = fem.GetNodeDisplacementGlobalCoordinates(nd3copy, LinearSolver.DOF.DX);
            double Node3CopyDY = fem.GetNodeDisplacementGlobalCoordinates(nd3copy, LinearSolver.DOF.DY);

            /*Node 4 Displacement
            DX (mm)	-0.000002	
            DY (mm)	-0.000007	
            DZ(mm) 0.000000*/
            double dXNode4 = -0.000002;
            double dYNode4 = -0.000007;
            Assert.AreEqual(Node4DX, dXNode4, 0.000001);
            Assert.AreEqual(Node4DY, dYNode4, 0.000001);

            /*Node 3 Displacement
            DX (mm)	-0.000014	
            DY (mm)	-0.000005
            DZ(mm) 0.000000*/
            double dXNode3 = -0.000014;
            double dYNode3 = -0.000005;
            Assert.AreEqual(Node3DX, dXNode3, 0.000001);
            Assert.AreEqual(Node3DY, dYNode3, 0.000001);
            Assert.AreEqual(Node3CopyDX, dXNode3, 0.000001);
            Assert.AreEqual(Node3CopyDY, dYNode3, 0.000001);
        }

        /// <summary>
        /// TEST LOCAL MATRIX
        /// </summary>
        [TestMethod]
        public void Quad4MembranalTest1()
        {
            Material mat = new SteelMaterial("steel", 1.0, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 0, 1, "p");

            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1, 0, "1");
            nds[1] = new Node(+1.0, -1, 0, "2");
            nds[2] = new Node(+1.0, +1, 0, "3");
            nds[3] = new Node(-1.0, +1, 0, "4");

            Quad4Membranal el = new Quad4Membranal(nds);
            el.SetProperty(prop);
        
            el.BuildMatrix();
            mnl.Matrix<double> kLocal = el.KElementLocalCoord;
            mnl.Matrix<double> kLocalManual = mnl.Matrix<double>.Build.Dense(0, 8);
            
            double[] r0 = new double[] { 0.5000, 0.1250, -0.2500, -0.1250, -0.2500, -0.1250, 0.0000, 0.1250 };
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
            }
        }

        /// <summary>
        /// TEST LOCAL MATRIX NON RECTANGLE NON SQUARED
        /// </summary>
        [TestMethod]
        public void Quad4MembranalTest2()
        {
            Material mat = new SteelMaterial("steel", 1.0, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 0, 1, "p");

            Node[] nds = new Node[4];
            nds[0] = new Node(+0.0, +0, 0, "1");
            nds[1] = new Node(+2.0, +0, 0, "2");
            nds[2] = new Node(+2.0, +1, 0, "3");
            nds[3] = new Node(+0.0, +1, 0, "4");

            Quad4Membranal el = new Quad4Membranal(nds);
            el.SetProperty(prop);
       
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
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 0, 1, "p");

            Node[] nds = new Node[4];
            nds[0] = new Node(+0.0, +0, 0, "1");
            nds[1] = new Node(+2.0, +0, 0, "2");
            nds[2] = new Node(+2.0, +2, 0, "3");
            nds[3] = new Node(+0.0, +1, 0, "4");

            Quad4Membranal el = new Quad4Membranal(nds);
            el.SetProperty(prop);
           
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
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 0, 1, "p");

            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1, 0, "1");
            nds[1] = new Node(+1.0, -1, 0, "2");
            nds[2] = new Node(+1.0, +1, 0, "3");
            nds[3] = new Node(-1.0, +1, 0, "4");

            Quad4Membranal el = new Quad4Membranal(nds);
            el.SetProperty(prop);
     
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

        [TestMethod]
        public void Quad4MembranalTest5()
        {
            double E = 1.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), thickness, thickness, "p");

            Node[] nds = new Node[4];
            nds[0] = new Node(0.0, 0, 0, "1");
            nds[1] = new Node(+1.0, 0, 0, "2");
            nds[2] = new Node(+2.0, +2, 0, "3");
            nds[3] = new Node(0.0, +1, 0, "4");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);

            NodeRestrainAttribute dZ = new NodeRestrainAttribute("freedomCase", sys);
            dZ.AddExternalRestrain(LinearSolver.DOF.DZ);
            
            nds[0].AddAttribute(fix);
            nds[1].AddAttribute(fix);
            nds[2].AddAttribute(dZ);
            nds[3].AddAttribute(dZ);

            LoadCaseBase lc = new LoadCaseBase("lc");
            double px = 0.1;
            PlatePressureAttribute pressure = new PlatePressureAttribute("lc", sys, px, 0, 0);
            
            Quad4Membranal el = new Quad4Membranal(nds);
            el.SetProperty(prop);
    
            el.AddLoadCaseAttribute(pressure);
            
            LinearSolver fem = new LinearSolver(new FiniteElement[] { el });
            Console.WriteLine("kGlob="+fem.KGlobal);
            Console.WriteLine("F="+fem.F);

            //Check force applied
            Assert.AreEqual(0.04167, fem.F[0], 0.001);
            Assert.AreEqual(0.050, fem.F[3], 0.001);
            Assert.AreEqual(0.0583, fem.F[6], 0.001);
            Assert.AreEqual(0.050, fem.F[9], 0.001);

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