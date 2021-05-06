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
    public class DKTTest
    {
        [TestMethod]
        public void Tri3DKTTest1()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0, "p");

            Node[] nodesPlate1 = new Node[3];
            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
            nodesPlate1[1] = new Node(1.0, 0, 0, "2");
            nodesPlate1[2] = new Node(0.0, 1, 0, "3");

            FiniteElement e0 = new Tri3DK(nodesPlate1);
            e0.SetProperty(prop);
        
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
        public void Tri3DKTTest2()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0, "p");

            Node[] nodesPlate1 = new Node[3];
            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
            nodesPlate1[1] = new Node(1.0, 0, 0, "2");
            nodesPlate1[2] = new Node(0.0, 1, 0, "3");

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

            FiniteElement e0 = new Tri3DK(nodesPlate1);
            e0.SetProperty(prop);
            
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
        public void Tri3DKTTest3()
        {
            //TODO: sistemare per calcolo tensioni
            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("mat", 10000, 0.3, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0, "p");

            #region restrains
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute fixDXDYDZRZ = new NodeRestrainAttribute("freedomCase", sys);
            fixDXDYDZRZ.AddExternalRestrain(LinearSolver.DOF.DX);
            fixDXDYDZRZ.AddExternalRestrain(LinearSolver.DOF.DY);
            fixDXDYDZRZ.AddExternalRestrain(LinearSolver.DOF.DZ);
            fixDXDYDZRZ.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute fixDXDYRZ = new NodeRestrainAttribute("freedomCase", sys);
            fixDXDYRZ.AddExternalRestrain(LinearSolver.DOF.DX);
            fixDXDYRZ.AddExternalRestrain(LinearSolver.DOF.DY);
            fixDXDYRZ.AddExternalRestrain(LinearSolver.DOF.RZ);
            #endregion

            #region nodalforces
            NodeForceAttribute F = new NodeForceAttribute("loadCase", sys, 0, 0, 5.0, 0, 0, 0);
            #endregion

            Node nodeA = new Node(0.0, 8, 0, "A");
            nodeA.AddAttribute(fixDXDYDZRZ);
            Node nodeB = new Node(0.0, 0, 0, "B");
            nodeB.AddAttribute(fixDXDYDZRZ);
            Node nodeC = new Node(8.0, 8, 0, "C");
            nodeC.AddAttribute(F);
            nodeC.AddAttribute(fixDXDYRZ);
            Node nodeD = new Node(8.0, 0, 0, "D");
            nodeD.AddAttribute(fixDXDYDZRZ);

            FiniteElement e0 = new Tri3DK(new Node[] { nodeA, nodeB, nodeC });
            e0.SetProperty(prop);
          
            FiniteElement e1 = new Tri3DK(new Node[] { nodeB, nodeD, nodeC });
            e1.SetProperty(prop);
        
            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0, e1 });

            double dz = fem.GetDisplacementGlobalCoordinates(nodeC, LinearSolver.DOF.DZ);
            Assert.AreEqual(0.24960, dz, 1e-6);

            double[] displElement = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(e0);
            e0.GetNodesResults(displElement, 
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
        public void Quad4DKTTest1()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0, "p");

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(-1.0, -1, 0);
            nodesPlate1[1] = new Node(+1.0, -1, 0);
            nodesPlate1[2] = new Node(+1.0, +1, 0);
            nodesPlate1[3] = new Node(-1.0, +1, 0);

            FiniteElement e0 = new Quad4DK(nodesPlate1);
            e0.SetProperty(prop);
     
            e0.BuildMatrix();

            mnl.Matrix<double> kLocalManual = mnl.Matrix<double>.Build.Dense(12, 12);

            double[] r0 = new double[] { 2.5, 1, -1, -1, 0.5, -1, -0.5, 0.5, -0.5, -1, 1, -0.5 };
            double[] r1 = new double[] { 1, 1.375, 0, 0.5, 0.625, 0, -0.5, 0.375, 0, -1, 0.625, 0 };
            double[] r2 = new double[] { -1, 0, 1.375, 1, 0, 0.625, 0.5, 0, 0.375, -0.5, 0, 0.625 };
            double[] r3 = new double[] { -1, 0.5, 1, 2.5, 1, 1, -1, 1, 0.5, -0.5, 0.5, 0.5 };
            double[] r4 = new double[] { 0.5, 0.625, 0, 1, 1.375, 0, -1, 0.625, 0, -0.5, 0.375, 0 };
            double[] r5 = new double[] { -1, 0, 0.625, 1, 0, 1.375, 0.5, 0, 0.625, -0.5, 0, 0.375 };
            double[] r6 = new double[] { -0.5, -0.5, 0.5, -1, -1, 0.5, 2.5, -1, 1, -1, -0.5, 1 };
            double[] r7 = new double[] { 0.5, 0.375, 0, 1, 0.625, 0, -1, 1.375, 0, -0.5, 0.625, 0 };
            double[] r8 = new double[] { -0.5, 0, 0.375, 0.5, 0, 0.625, 1, 0, 1.375, -1, 0, 0.625 };
            double[] r9 = new double[] { -1, -1, -0.5, -0.5, -0.5, -0.5, -1, -0.5, -1, 2.5, -1, -1 };
            double[] r10 = new double[] { 1, 0.625, 0, 0.5, 0.375, 0, -0.5, 0.625, 0, -1, 1.375, 0 };
            double[] r11 = new double[] { -0.5, 0, 0.625, 0.5, 0, 0.375, 1, 0, 0.625, -1, 0, 1.375 };

            kLocalManual = kLocalManual.InsertRow(0, mnl.Vector<double>.Build.Dense(r0));
            kLocalManual = kLocalManual.InsertRow(1, mnl.Vector<double>.Build.Dense(r1));
            kLocalManual = kLocalManual.InsertRow(2, mnl.Vector<double>.Build.Dense(r2));
            kLocalManual = kLocalManual.InsertRow(3, mnl.Vector<double>.Build.Dense(r3));
            kLocalManual = kLocalManual.InsertRow(4, mnl.Vector<double>.Build.Dense(r4));
            kLocalManual = kLocalManual.InsertRow(5, mnl.Vector<double>.Build.Dense(r5));
            kLocalManual = kLocalManual.InsertRow(6, mnl.Vector<double>.Build.Dense(r6));
            kLocalManual = kLocalManual.InsertRow(7, mnl.Vector<double>.Build.Dense(r7));
            kLocalManual = kLocalManual.InsertRow(8, mnl.Vector<double>.Build.Dense(r8));
            kLocalManual = kLocalManual.InsertRow(9, mnl.Vector<double>.Build.Dense(r9));
            kLocalManual = kLocalManual.InsertRow(10, mnl.Vector<double>.Build.Dense(r10));
            kLocalManual = kLocalManual.InsertRow(11, mnl.Vector<double>.Build.Dense(r11));

            Console.WriteLine("Element local stiffness matrix");
            for (int r = 0; r < e0.KElementLocalCoord.RowCount; r++)
            {
                for (int c = 0; c < e0.KElementLocalCoord.ColumnCount; c++)
                {
                    Console.Write(e0.KElementLocalCoord[r, c].ToString("F3") + " ");
                    Assert.AreEqual(e0.KElementLocalCoord[r, c] - kLocalManual[r, c], 0, 0.001, "kLocal no OK -> row " + r + " col " + c);
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void Quad4DKTTest2()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0, "p");

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
            nodesPlate1[1] = new Node(2.0, 0, 0, "2");
            nodesPlate1[2] = new Node(2.0, 2, 0, "3");
            nodesPlate1[3] = new Node(0.0, 2, 0, "4");

            FiniteElement e0 = new Quad4DK(nodesPlate1);
            e0.SetProperty(prop);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0 });

            mnl.Matrix<double> kGlobalManual = mnl.Matrix<double>.Build.Dense(0, 24);

            double[] r0 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r1 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r2 = new double[] { 0, 0, 2.5, 1, -1, 0, 0, 0, -1, 0.5, -1, 0, 0, 0, -0.5, 0.5, -0.5, 0, 0, 0, -1, 1, -0.5, 0 };
            double[] r3 = new double[] { 0, 0, 1, 1.375, 0, 0, 0, 0, 0.5, 0.625, 0, 0, 0, 0, -0.5, 0.375, 0, 0, 0, 0, -1, 0.625, 0, 0 };
            double[] r4 = new double[] { 0, 0, -1, 0, 1.375, 0, 0, 0, 1, 0, 0.625, 0, 0, 0, 0.5, 0, 0.375, 0, 0, 0, -0.5, 0, 0.625, 0 };
            double[] r5 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r6 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r7 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r8 = new double[] { 0, 0, -1, 0.5, 1, 0, 0, 0, 2.5, 1, 1, 0, 0, 0, -1, 1, 0.5, 0, 0, 0, -0.5, 0.5, 0.5, 0 };
            double[] r9 = new double[] { 0, 0, 0.5, 0.625, 0, 0, 0, 0, 1, 1.375, 0, 0, 0, 0, -1, 0.625, 0, 0, 0, 0, -0.5, 0.375, 0, 0 };
            double[] r10 = new double[] { 0, 0, -1, 0, 0.625, 0, 0, 0, 1, 0, 1.375, 0, 0, 0, 0.5, 0, 0.625, 0, 0, 0, -0.5, 0, 0.375, 0 };
            double[] r11 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r12 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r13 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r14 = new double[] { 0, 0, -0.5, -0.5, 0.5, 0, 0, 0, -1, -1, 0.5, 0, 0, 0, 2.5, -1, 1, 0, 0, 0, -1, -0.5, 1, 0 };
            double[] r15 = new double[] { 0, 0, 0.5, 0.375, 0, 0, 0, 0, 1, 0.625, 0, 0, 0, 0, -1, 1.375, 0, 0, 0, 0, -0.5, 0.625, 0, 0 };
            double[] r16 = new double[] { 0, 0, -0.5, 0, 0.375, 0, 0, 0, 0.5, 0, 0.625, 0, 0, 0, 1, 0, 1.375, 0, 0, 0, -1, 0, 0.625, 0 };
            double[] r17 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r18 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r19 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] r20 = new double[] { 0, 0, -1, -1, -0.5, 0, 0, 0, -0.5, -0.5, -0.5, 0, 0, 0, -1, -0.5, -1, 0, 0, 0, 2.5, -1, -1, 0 };
            double[] r21 = new double[] { 0, 0, 1, 0.625, 0, 0, 0, 0, 0.5, 0.375, 0, 0, 0, 0, -0.5, 0.625, 0, 0, 0, 0, -1, 1.375, 0, 0 };
            double[] r22 = new double[] { 0, 0, -0.5, 0, 0.625, 0, 0, 0, 0.5, 0, 0.375, 0, 0, 0, 1, 0, 0.625, 0, 0, 0, -1, 0, 1.375, 0 };
            double[] r23 = new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

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
            kGlobalManual = kGlobalManual.InsertRow(12, mnl.Vector<double>.Build.Dense(r12));
            kGlobalManual = kGlobalManual.InsertRow(13, mnl.Vector<double>.Build.Dense(r13));
            kGlobalManual = kGlobalManual.InsertRow(14, mnl.Vector<double>.Build.Dense(r14));
            kGlobalManual = kGlobalManual.InsertRow(15, mnl.Vector<double>.Build.Dense(r15));
            kGlobalManual = kGlobalManual.InsertRow(16, mnl.Vector<double>.Build.Dense(r16));
            kGlobalManual = kGlobalManual.InsertRow(17, mnl.Vector<double>.Build.Dense(r17));
            kGlobalManual = kGlobalManual.InsertRow(18, mnl.Vector<double>.Build.Dense(r18));
            kGlobalManual = kGlobalManual.InsertRow(19, mnl.Vector<double>.Build.Dense(r19));
            kGlobalManual = kGlobalManual.InsertRow(20, mnl.Vector<double>.Build.Dense(r20));
            kGlobalManual = kGlobalManual.InsertRow(21, mnl.Vector<double>.Build.Dense(r21));
            kGlobalManual = kGlobalManual.InsertRow(22, mnl.Vector<double>.Build.Dense(r22));
            kGlobalManual = kGlobalManual.InsertRow(23, mnl.Vector<double>.Build.Dense(r23));

            Console.WriteLine("Element global stiffness matrix");
            for (int r = 0; r < fem.KGlobal.RowCount; r++)
            {
                for (int c = 0; c < fem.KGlobal.ColumnCount; c++)
                {
                    Console.Write(fem.KGlobal[r, c].ToString("F3") + " ");
                    Assert.AreEqual(fem.KGlobal[r, c] - kGlobalManual[r, c], 0, 0.01, "kGlobal no OK -> row " + r + " col " + c);
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void Quad4DKTTest3()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0, "p");

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
            nodesPlate1[1] = new Node(+1.0, 0, 0, "2");
            nodesPlate1[2] = new Node(+2.0, +2, 0, "3");
            nodesPlate1[3] = new Node(0.0, +1, 0, "4");

            Plate e0 = new Quad4DK(nodesPlate1);
            e0.SetProperty(prop);

            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            PlatePressureAttribute pressure = new PlatePressureAttribute("loadCase", sys, 0.0, 0.0, 1.0);
            e0.AddLoadCaseAttribute(pressure);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0 });

            Assert.AreEqual(0.4167, fem.F[2], 0.001); //UX, UY, UZ
            Assert.AreEqual(0.50, fem.F[8], 0.001);
            Assert.AreEqual(0.5833, fem.F[14], 0.001);
            Assert.AreEqual(0.50, fem.F[20], 0.001);
        }

        [TestMethod]
        public void Quad4DKTTest4()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0, "p");

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
            nodesPlate1[1] = new Node(+2.0, 0, 0, "2");
            nodesPlate1[2] = new Node(+2.0, +2, 0, "3");
            nodesPlate1[3] = new Node(0.0, +2, 0, "4");

            Plate e0 = new Quad4DK(nodesPlate1);
            e0.SetProperty(prop);

            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            PlatePressureAttribute pressure = new PlatePressureAttribute("loadCase", sys, 0.0, 0.0, 1.0);
            e0.AddLoadCaseAttribute(pressure);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0 });

            Assert.AreEqual(1, fem.F[2], 0.001); //UX, UY, UZ
            Assert.AreEqual(1, fem.F[8], 0.001);
            Assert.AreEqual(1, fem.F[14], 0.001);
            Assert.AreEqual(1, fem.F[20], 0.001);
        }
    }
}