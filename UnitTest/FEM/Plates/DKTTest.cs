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
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 1.0, 1.0, "p");

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
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 1.0, 1.0, "p");

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
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 1.0, 1.0, "p");

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

            double dz = fem.GetNodeDisplacementGlobalCoordinates(nodeC, LinearSolver.DOF.DZ);
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

        /// <summary>
        /// Check K local with manual
        /// </summary>
        [TestMethod]
        public void Quad4DKTTest1()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 1.0, 1.0, "p");

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

        /// <summary>
        /// Check KGlobal with manual
        /// </summary>
        [TestMethod]
        public void Quad4DKTTest2()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 1.0, 1.0, "p");

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
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 1.0, 1.0, "p");

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
        public void Quad4DKTEquivalentNodesForcesTest1()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 1.0, 1.0, "p");

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(+0.0, +0.0, 0);
            nodesPlate1[1] = new Node(+2.0, +0.0, 0);
            nodesPlate1[2] = new Node(+0.1, +2.0, 0);
            nodesPlate1[3] = new Node(+0.0, +2.0, 0);

            Plate e0 = new Quad4DK(nodesPlate1);
            e0.SetProperty(prop);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            PlatePressureAttribute pressure = new PlatePressureAttribute("loadCase", sys, 0.0, 0.0, 1.0);
            e0.AddLoadCaseAttribute(pressure);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);

            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nodesPlate1.ToList().ForEach(x => x.AddAttribute(fix));

            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0 });
        }

        [TestMethod]
        public void Quad4DKTTest4()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 1.0, 1.0, "p");

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
            nodesPlate1[1] = new Node(+2.0, 0, 0, "2");
            nodesPlate1[2] = new Node(+2.0, +2, 0, "3");
            nodesPlate1[3] = new Node(0.0, +2, 0, "4");

            Plate e0 = new Quad4DK(nodesPlate1);
            e0.SetProperty(prop);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            PlatePressureAttribute pressure = new PlatePressureAttribute("loadCase", sys, 0.0, 0.0, 1.0);
            e0.AddLoadCaseAttribute(pressure);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0 });

            Assert.AreEqual(1, fem.F[2], 0.001); //UX, UY, UZ
            Assert.AreEqual(1, fem.F[8], 0.001);
            Assert.AreEqual(1, fem.F[14], 0.001);
            Assert.AreEqual(1, fem.F[20], 0.001);
        }

        [TestMethod]
        public void Quad4DKTestRobert()
        {
            double E = 12;
            double ni = 0.0;
            double t = 1;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), t, t, "p");

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
            nodesPlate1[1] = new Node(+2.0, 0, 0, "2");
            nodesPlate1[2] = new Node(+2.0, +2, 0, "3");
            nodesPlate1[3] = new Node(0.0, +2, 0, "4");

            Plate e0 = new Quad4DK(nodesPlate1);
            e0.SetProperty(prop);

            e0.BuildMatrix();
            FEMUtilities.WriteMatrix(e0.KElementGlobalCoord);
        }

        [TestMethod]
        public void Quad4DKTestRobert2()
        {
            double E = 12;
            double ni = 0.0;
            double t = 0.6299;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), t, t, "p");

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
            nodesPlate1[1] = new Node(+2.0, 0, 0, "2");
            nodesPlate1[2] = new Node(+2.0, +2, 0, "3");
            nodesPlate1[3] = new Node(0.0, +2, 0, "4");

            Plate e0 = new Quad4DK(nodesPlate1);
            e0.SetProperty(prop);

            e0.BuildMatrix();
            FEMUtilities.WriteMatrix(e0.KElementGlobalCoord);
        }

        /// <summary>
        /// piastra 12x20, piastra semplicemente appoggiata con forze concentrate. elementi rettangolari non quadrati
        /// </summary>
        [TestMethod]
        public void QuadrilateralTestRobert5()
        {
            double h = 0.31498;
            double E = 10000.0;
            double ni = 0.0;

            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), h, h, "p");

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(-1e6, -1e6, -1e6));
            nodes.Add(new Node(0, 0, 0));
            nodes.Add(new Node(0, 20, 0));
            nodes.Add(new Node(12, 0, 0));
            nodes.Add(new Node(12, 20, 0));
            nodes.Add(new Node(2.5, 0, 0));
            nodes.Add(new Node(4.7, 0, 0));
            nodes.Add(new Node(6, 0, 0));
            nodes.Add(new Node(8.7, 0, 0));
            nodes.Add(new Node(10.5, 0, 0));
            nodes.Add(new Node(0, 2.6, 0));
            nodes.Add(new Node(2.5, 2.6, 0));
            nodes.Add(new Node(4.7, 2.6, 0));
            nodes.Add(new Node(6, 2.6, 0));
            nodes.Add(new Node(8.7, 2.6, 0));
            nodes.Add(new Node(10.5, 2.6, 0));
            nodes.Add(new Node(12, 2.6, 0));
            nodes.Add(new Node(0, 4.3, 0));
            nodes.Add(new Node(2.5, 4.3, 0));
            nodes.Add(new Node(4.7, 4.3, 0));
            nodes.Add(new Node(6, 4.3, 0));
            nodes.Add(new Node(8.7, 4.3, 0));
            nodes.Add(new Node(10.5, 4.3, 0));
            nodes.Add(new Node(12, 4.3, 0));
            nodes.Add(new Node(0, 6, 0));
            nodes.Add(new Node(2.5, 6, 0));
            nodes.Add(new Node(4.7, 6, 0));
            nodes.Add(new Node(6, 6, 0));
            nodes.Add(new Node(8.7, 6, 0));
            nodes.Add(new Node(10.5, 6, 0));
            nodes.Add(new Node(12, 6, 0));
            nodes.Add(new Node(0, 7.6, 0));
            nodes.Add(new Node(2.5, 7.6, 0));
            nodes.Add(new Node(4.7, 7.6, 0));
            nodes.Add(new Node(6, 7.6, 0));
            nodes.Add(new Node(8.7, 7.6, 0));
            nodes.Add(new Node(10.5, 7.6, 0));
            nodes.Add(new Node(12, 7.6, 0));
            nodes.Add(new Node(0, 10, 0));
            nodes.Add(new Node(2.5, 10, 0));
            nodes.Add(new Node(4.7, 10, 0));
            nodes.Add(new Node(6, 10, 0));
            nodes.Add(new Node(8.7, 10, 0));
            nodes.Add(new Node(10.5, 10, 0));
            nodes.Add(new Node(12, 10, 0));
            nodes.Add(new Node(0, 12.4, 0));
            nodes.Add(new Node(2.5, 12.4, 0));
            nodes.Add(new Node(4.7, 12.4, 0));
            nodes.Add(new Node(6, 12.4, 0));
            nodes.Add(new Node(8.7, 12.4, 0));
            nodes.Add(new Node(10.5, 12.4, 0));
            nodes.Add(new Node(12, 12.4, 0));
            nodes.Add(new Node(0, 14.2, 0));
            nodes.Add(new Node(2.5, 14.2, 0));
            nodes.Add(new Node(4.7, 14.2, 0));
            nodes.Add(new Node(6, 14.2, 0));
            nodes.Add(new Node(8.7, 14.2, 0));
            nodes.Add(new Node(10.5, 14.2, 0));
            nodes.Add(new Node(12, 14.2, 0));
            nodes.Add(new Node(0, 16, 0));
            nodes.Add(new Node(2.5, 16, 0));
            nodes.Add(new Node(4.7, 16, 0));
            nodes.Add(new Node(6, 16, 0));
            nodes.Add(new Node(8.7, 16, 0));
            nodes.Add(new Node(10.5, 16, 0));
            nodes.Add(new Node(12, 16, 0));
            nodes.Add(new Node(0, 17.6, 0));
            nodes.Add(new Node(2.5, 17.6, 0));
            nodes.Add(new Node(4.7, 17.6, 0));
            nodes.Add(new Node(6, 17.6, 0));
            nodes.Add(new Node(8.7, 17.6, 0));
            nodes.Add(new Node(10.5, 17.6, 0));
            nodes.Add(new Node(12, 17.6, 0));
            nodes.Add(new Node(2.5, 20, 0));
            nodes.Add(new Node(4.7, 20, 0));
            nodes.Add(new Node(6, 20, 0));
            nodes.Add(new Node(8.7, 20, 0));
            nodes.Add(new Node(10.5, 20, 0));
            #endregion

            #region plates
            List<Quad4DK> els = new List<Quad4DK>();
            els.Add(new Quad4DK(new Node[] { nodes[71], nodes[72], nodes[4], nodes[77] }));
            els.Add(new Quad4DK(new Node[] { nodes[1], nodes[5], nodes[11], nodes[10] }));
            els.Add(new Quad4DK(new Node[] { nodes[5], nodes[6], nodes[12], nodes[11] }));
            els.Add(new Quad4DK(new Node[] { nodes[6], nodes[7], nodes[13], nodes[12] }));
            els.Add(new Quad4DK(new Node[] { nodes[7], nodes[8], nodes[14], nodes[13] }));
            els.Add(new Quad4DK(new Node[] { nodes[8], nodes[9], nodes[15], nodes[14] }));
            els.Add(new Quad4DK(new Node[] { nodes[9], nodes[3], nodes[16], nodes[15] }));
            els.Add(new Quad4DK(new Node[] { nodes[10], nodes[11], nodes[18], nodes[17] }));
            els.Add(new Quad4DK(new Node[] { nodes[11], nodes[12], nodes[19], nodes[18] }));
            els.Add(new Quad4DK(new Node[] { nodes[12], nodes[13], nodes[20], nodes[19] }));
            els.Add(new Quad4DK(new Node[] { nodes[13], nodes[14], nodes[21], nodes[20] }));
            els.Add(new Quad4DK(new Node[] { nodes[14], nodes[15], nodes[22], nodes[21] }));
            els.Add(new Quad4DK(new Node[] { nodes[15], nodes[16], nodes[23], nodes[22] }));
            els.Add(new Quad4DK(new Node[] { nodes[17], nodes[18], nodes[25], nodes[24] }));
            els.Add(new Quad4DK(new Node[] { nodes[18], nodes[19], nodes[26], nodes[25] }));
            els.Add(new Quad4DK(new Node[] { nodes[19], nodes[20], nodes[27], nodes[26] }));
            els.Add(new Quad4DK(new Node[] { nodes[20], nodes[21], nodes[28], nodes[27] }));
            els.Add(new Quad4DK(new Node[] { nodes[21], nodes[22], nodes[29], nodes[28] }));
            els.Add(new Quad4DK(new Node[] { nodes[22], nodes[23], nodes[30], nodes[29] }));
            els.Add(new Quad4DK(new Node[] { nodes[24], nodes[25], nodes[32], nodes[31] }));
            els.Add(new Quad4DK(new Node[] { nodes[25], nodes[26], nodes[33], nodes[32] }));
            els.Add(new Quad4DK(new Node[] { nodes[26], nodes[27], nodes[34], nodes[33] }));
            els.Add(new Quad4DK(new Node[] { nodes[27], nodes[28], nodes[35], nodes[34] }));
            els.Add(new Quad4DK(new Node[] { nodes[28], nodes[29], nodes[36], nodes[35] }));
            els.Add(new Quad4DK(new Node[] { nodes[29], nodes[30], nodes[37], nodes[36] }));
            els.Add(new Quad4DK(new Node[] { nodes[31], nodes[32], nodes[39], nodes[38] }));
            els.Add(new Quad4DK(new Node[] { nodes[32], nodes[33], nodes[40], nodes[39] }));
            els.Add(new Quad4DK(new Node[] { nodes[33], nodes[34], nodes[41], nodes[40] }));
            els.Add(new Quad4DK(new Node[] { nodes[34], nodes[35], nodes[42], nodes[41] }));
            els.Add(new Quad4DK(new Node[] { nodes[35], nodes[36], nodes[43], nodes[42] }));
            els.Add(new Quad4DK(new Node[] { nodes[36], nodes[37], nodes[44], nodes[43] }));
            els.Add(new Quad4DK(new Node[] { nodes[38], nodes[39], nodes[46], nodes[45] }));
            els.Add(new Quad4DK(new Node[] { nodes[39], nodes[40], nodes[47], nodes[46] }));
            els.Add(new Quad4DK(new Node[] { nodes[40], nodes[41], nodes[48], nodes[47] }));
            els.Add(new Quad4DK(new Node[] { nodes[41], nodes[42], nodes[49], nodes[48] }));
            els.Add(new Quad4DK(new Node[] { nodes[42], nodes[43], nodes[50], nodes[49] }));
            els.Add(new Quad4DK(new Node[] { nodes[43], nodes[44], nodes[51], nodes[50] }));
            els.Add(new Quad4DK(new Node[] { nodes[45], nodes[46], nodes[53], nodes[52] }));
            els.Add(new Quad4DK(new Node[] { nodes[46], nodes[47], nodes[54], nodes[53] }));
            els.Add(new Quad4DK(new Node[] { nodes[47], nodes[48], nodes[55], nodes[54] }));
            els.Add(new Quad4DK(new Node[] { nodes[48], nodes[49], nodes[56], nodes[55] }));
            els.Add(new Quad4DK(new Node[] { nodes[49], nodes[50], nodes[57], nodes[56] }));
            els.Add(new Quad4DK(new Node[] { nodes[50], nodes[51], nodes[58], nodes[57] }));
            els.Add(new Quad4DK(new Node[] { nodes[52], nodes[53], nodes[60], nodes[59] }));
            els.Add(new Quad4DK(new Node[] { nodes[53], nodes[54], nodes[61], nodes[60] }));
            els.Add(new Quad4DK(new Node[] { nodes[54], nodes[55], nodes[62], nodes[61] }));
            els.Add(new Quad4DK(new Node[] { nodes[55], nodes[56], nodes[63], nodes[62] }));
            els.Add(new Quad4DK(new Node[] { nodes[56], nodes[57], nodes[64], nodes[63] }));
            els.Add(new Quad4DK(new Node[] { nodes[57], nodes[58], nodes[65], nodes[64] }));
            els.Add(new Quad4DK(new Node[] { nodes[59], nodes[60], nodes[67], nodes[66] }));
            els.Add(new Quad4DK(new Node[] { nodes[60], nodes[61], nodes[68], nodes[67] }));
            els.Add(new Quad4DK(new Node[] { nodes[61], nodes[62], nodes[69], nodes[68] }));
            els.Add(new Quad4DK(new Node[] { nodes[62], nodes[63], nodes[70], nodes[69] }));
            els.Add(new Quad4DK(new Node[] { nodes[63], nodes[64], nodes[71], nodes[70] }));
            els.Add(new Quad4DK(new Node[] { nodes[64], nodes[65], nodes[72], nodes[71] }));
            els.Add(new Quad4DK(new Node[] { nodes[66], nodes[67], nodes[73], nodes[2] }));
            els.Add(new Quad4DK(new Node[] { nodes[67], nodes[68], nodes[74], nodes[73] }));
            els.Add(new Quad4DK(new Node[] { nodes[68], nodes[69], nodes[75], nodes[74] }));
            els.Add(new Quad4DK(new Node[] { nodes[69], nodes[70], nodes[76], nodes[75] }));
            els.Add(new Quad4DK(new Node[] { nodes[70], nodes[71], nodes[77], nodes[76] }));
            #endregion
            els.ForEach(x => x.SetProperty(prop));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0, 100, 0, 0, 0);
            nodes.Where(x => x.Position.X == 6.0 && x.Position.Y == 10.0).ToList().ForEach(x => x.AddAttribute(f));

            NodeRestrainAttribute dz = new NodeRestrainAttribute("freedomCase", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);

            nodes[1].AddAttribute(dz);
            nodes[3].AddAttribute(dz);
            nodes[4].AddAttribute(dz);
            nodes[2].AddAttribute(dz);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            //fix.AddExternalRestrain(Solver.DOF.DZ);

            /*fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);*/
            fix.AddExternalRestrain(Solver.DOF.RZ);

            //fix.AddExternalRestrain(Solver.DOF.DDX);
            //fix.AddExternalRestrain(Solver.DOF.DDY);
            //fix.AddExternalRestrain(Solver.DOF.DDZ);

            nodes.ForEach(x => x.AddAttribute(fix));

            LinearSolver fem = new LinearSolver(els.ToArray());
        }

        /// <summary>
        /// Batoz articolo
        /// </summary>
        [TestMethod]
        public void PatchTest1()
        {
            double h = 1.0;
            double E = 1000.0;
            double ni = 0.0;

            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), h, h, "p");

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(-1e6, -1e6, -1e6));
            nodes.Add(new Node(0, 0, 0));
            nodes.Add(new Node(40, 0, 0));
            nodes.Add(new Node(0, 20, 0));
            nodes.Add(new Node(40, 20, 0));
            nodes.Add(new Node(29, 4, 0));
            nodes.Add(new Node(29, 14, 0));
            nodes.Add(new Node(5, 17.5, 0));
            nodes.Add(new Node(20, 7, 0));
            #endregion

            #region plates
            List<Quad4DK> els = new List<Quad4DK>();
            els.Add(new Quad4DK(new Node[] { nodes[5], nodes[6], nodes[7], nodes[8] }));
            els.Add(new Quad4DK(new Node[] { nodes[2], nodes[4], nodes[6], nodes[5] }));
            els.Add(new Quad4DK(new Node[] { nodes[4], nodes[3], nodes[7], nodes[6] }));
            els.Add(new Quad4DK(new Node[] { nodes[3], nodes[1], nodes[8], nodes[7] }));
            els.Add(new Quad4DK(new Node[] { nodes[1], nodes[2], nodes[5], nodes[8] }));

            #endregion
            els.ForEach(x => x.SetProperty(prop));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0, -2, 0, 0, 0);
            nodes.Where(x => x.Position.X == 40.0 && x.Position.Y == 20.0).ToList().ForEach(x => x.AddAttribute(f));

            NodeForceAttribute mxPlus = new NodeForceAttribute("lc", sys, 0, 0, 0, 20, 0, 0);
 
            NodeForceAttribute mxMinus = new NodeForceAttribute("lc", sys, 0, 0, 0, -20, 0, 0);

            NodeForceAttribute myPlus = new NodeForceAttribute("lc", sys, 0, 0, 0, 0, 10, 0);

            NodeForceAttribute myMinus = new NodeForceAttribute("lc", sys, 0, 0, 0, 0, -10, 0);

            nodes[1].AddAttribute(mxPlus);
            nodes[1].AddAttribute(myMinus);

            nodes[2].AddAttribute(mxPlus);
            nodes[2].AddAttribute(myPlus);

            nodes[3].AddAttribute(mxMinus);
            nodes[3].AddAttribute(myMinus);

            nodes[4].AddAttribute(mxMinus);
            nodes[4].AddAttribute(myPlus);

            NodeRestrainAttribute dz = new NodeRestrainAttribute("freedomCase", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);

            nodes[1].AddAttribute(dz);
            nodes[2].AddAttribute(dz);
            nodes[3].AddAttribute(dz);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            //fix.AddExternalRestrain(Solver.DOF.DZ);

            /*fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);*/
            fix.AddExternalRestrain(Solver.DOF.RZ);

            //fix.AddExternalRestrain(Solver.DOF.DDX);
            //fix.AddExternalRestrain(Solver.DOF.DDY);
            //fix.AddExternalRestrain(Solver.DOF.DDZ);

            nodes.ForEach(x => x.AddAttribute(fix));

            //LinearSolver fem = new LinearSolver(els.ToArray());

            els[0].BuildMatrix();
            FEMUtilities.WriteMatrix(els[0].KElementLocalCoord);
        }
    }
}