using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM.Properties;
using GPC.Model.Materials;

namespace FemTest.SolverTest
{
    [TestClass]
    public class Tetrahedron4Test
    {
        [TestMethod]
        public void VolumeTest1()
        {
            double a = 1.0;
            double b = 1.0;
            double h = 1.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0));
            nds.Add(new Node(a, 0.0, 0));
            nds.Add(new Node(0.0, b, 0 ));
            nds.Add(new Node(0.0, 0.0, h));

            double A = (1.0 / 2.0 * a * b);
            double V = A * h / 3.0;
            Assert.AreEqual(V, Tethraedron4.GetVolume(nds.ToArray()));
        }

        [TestMethod]
        public void GetCoefficientTest1()
        {
            double x = 1.0;
            double y = 1.0;
            double h = 1.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0));
            nds.Add(new Node(x, 0.0, 0));
            nds.Add(new Node(0.0, y, 0));
            nds.Add(new Node(0.0, 0.0, h));
            
            Func<int, Node, double> N = (int index, Node n) =>
            {
                double V = Tethraedron4.GetVolume(nds.ToArray());
                double a = Tethraedron4.GetCoefficientShapeFunction(index, "a", nds.ToArray());
                double b = Tethraedron4.GetCoefficientShapeFunction(index, "b", nds.ToArray());
                double c = Tethraedron4.GetCoefficientShapeFunction(index, "c", nds.ToArray());
                double d = Tethraedron4.GetCoefficientShapeFunction(index, "d", nds.ToArray());
                return 1.0 / (6.0 * V) * (a + b * n.Position.X + c * n.Position.Y + d * n.Position.Z);
            };

            Assert.AreEqual(-1.0, Tethraedron4.GetCoefficientShapeFunction(1, "b", nds.ToArray()));
            Assert.AreEqual(1.0, Tethraedron4.GetCoefficientShapeFunction(2, "b", nds.ToArray()));

            Assert.AreEqual(1.0, N(1, nds[0]));
            Assert.AreEqual(0.0, N(2, nds[0]));
            Assert.AreEqual(0.0, N(3, nds[0]));
            Assert.AreEqual(0.0, N(4, nds[0]));

            Assert.AreEqual(0.0, N(1, nds[1]));
            Assert.AreEqual(1.0, N(2, nds[1]));
            Assert.AreEqual(0.0, N(3, nds[1]));
            Assert.AreEqual(0.0, N(4, nds[1]));

            Assert.AreEqual(0.0, N(1, nds[2]));
            Assert.AreEqual(0.0, N(2, nds[2]));
            Assert.AreEqual(1.0, N(3, nds[2]));
            Assert.AreEqual(0.0, N(4, nds[2]));

            Assert.AreEqual(0.0, N(1, nds[3]));
            Assert.AreEqual(0.0, N(2, nds[3]));
            Assert.AreEqual(0.0, N(3, nds[3]));
            Assert.AreEqual(1.0, N(4, nds[3]));
        }

        [TestMethod]
        public void GetCoefficientTest2()
        {
            List<Node> nds = new List<Node>();
            nds.Add(new Node(2.0, 3.0, 4.0));
            nds.Add(new Node(6.0, 3.0, 2.0));
            nds.Add(new Node(2.0, 5.0, 1.0));
            nds.Add(new Node(4.0, 3.0, 6.0));

            Func<int, Node, double> N = (int index, Node n) =>
            {
                double V = Tethraedron4.GetVolume(nds.ToArray());
                double a = Tethraedron4.GetCoefficientShapeFunction(index, "a", nds.ToArray());
                double b = Tethraedron4.GetCoefficientShapeFunction(index, "b", nds.ToArray());
                double c = Tethraedron4.GetCoefficientShapeFunction(index, "c", nds.ToArray());
                double d = Tethraedron4.GetCoefficientShapeFunction(index, "d", nds.ToArray());
                return 1.0 / (6.0 * V) * (a + b * n.Position.X + c * n.Position.Y + d * n.Position.Z);
            };

            Assert.AreEqual(1.0, N(1, nds[0]), 0.000001);
            Assert.AreEqual(0.0, N(2, nds[0]), 0.000001);
            Assert.AreEqual(0.0, N(3, nds[0]), 0.000001);
            Assert.AreEqual(0.0, N(4, nds[0]), 0.000001);

            Assert.AreEqual(0.0, N(1, nds[1]), 0.000001);
            Assert.AreEqual(1.0, N(2, nds[1]), 0.000001);
            Assert.AreEqual(0.0, N(3, nds[1]), 0.000001);
            Assert.AreEqual(0.0, N(4, nds[1]), 0.000001);

            Assert.AreEqual(0.0, N(1, nds[2]), 0.000001);
            Assert.AreEqual(0.0, N(2, nds[2]), 0.000001);
            Assert.AreEqual(1.0, N(3, nds[2]), 0.000001);
            Assert.AreEqual(0.0, N(4, nds[2]), 0.000001);

            Assert.AreEqual(0.0, N(1, nds[3]), 0.000001);
            Assert.AreEqual(0.0, N(2, nds[3]), 0.000001);
            Assert.AreEqual(0.0, N(3, nds[3]), 0.000001);
            Assert.AreEqual(1.0, N(4, nds[3]), 0.000001);
        }

        [TestMethod]
        public void KMatrixTest1()
        {
            List<Node> nds = new List<Node>();
            nds.Add(new Node(2.0, 3.0, 4.0));
            nds.Add(new Node(6.0, 3.0, 2.0));
            nds.Add(new Node(2.0, 5.0, 1.0));
            nds.Add(new Node(4.0, 3.0, 6.0));

            Assert.AreEqual(4.0, Tethraedron4.GetVolume(nds.ToArray()));

            SteelMaterial mat = new SteelMaterial("mat", 96.0, 1.0 / 3.0, 355, 510.0);

            BrickProperty brickProperty = new BrickProperty(mat.GetIsotropicFemMaterial(), "propr");

            Tethraedron4 e = new Tethraedron4(nds.ToArray(), brickProperty);
            e.BuildMatrix();

            FEMUtilities.WriteMatrix(e.KElementLocalCoord);

            //Local axis == global axis
            for (int r = 0; r < 12; r++)
            {
                for (int c = 0; c < 12; c++)
                {
                    Assert.AreEqual(e.KElementLocalCoord[r, c], e.KElementGlobalCoord[r, c], 0.00000001);
                }
            }

            /*
             * 	X	Y	Z	FX	FY	FZ	MX	MY	MZ
	            (m)	(m)	(m)	(N)	(N)	(N)	(N.m)	(N.m)	(N.m)
            Node 1	2.0000	3.0000	4.0000	149.0000	108.0000	24.0000	 	 	 
            Node 2	6.0000	3.0000	2.0000	-1.0000	    6.0000	    12.0000	 	 	 
            Node 3	2.0000	5.0000	1.0000	-54.0000	-48.0000	0.00000 	 	 	 
            Node 4	4.0000	3.0000	6.0000	-94.0000	-66.0000	-36.0000	 	 	 
            */

            List<mnl.Vector<double>> rows = new List<mnl.Vector<double>>();

            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 149, 108, 24, -1, 6, 12, -54, -48, 0, -94, -66, -36 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 108, 344, 54, -24, 104, 42, -24, -216, -12, -60, -232, -84 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 24, 54, 113, 0, 30, 35, 0, -24, -54, -24, -60, -94 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -1, -24, 0, 29, -18, -12, -18, 24, 0, -10, 18, 12 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 6, 104, 30, -18, 44, 18, 12, -72, -12, 0, -76, -36 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 12, 42, 35, -12, 18, 29, 0, -24, -18, 0, -36, -46 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -54, -24, 0, -18, 12, 0, 36, 0, 0, 36, 12, 0 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -48, -216, -24, 24, -72, -24, 0, 144, 0, 24, 144, 48 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 0, -12, -54, 0, -12, -18, 0, 0, 36, 0, 24, 36 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -94, -60, -24, -10, 0, 0, 36, 24, 0, 68, 36, 24 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -66, -232, -60, 18, -76, -36, 12, 144, 24, 36, 164, 72 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -36, -84, -94, 12, -36, -46, 0, 48, 36, 24, 72, 104 }));

            mnl.Matrix<double> k = mnl.Matrix<double>.Build.Dense(0, 12);

            int index = 0;
            rows.ForEach(r => { k = k.InsertRow(index, r); index++; });

            for (int r = 0; r < 12; r++)
            {
                for (int c = 0; c < 12; c++)
                {
                    Assert.AreEqual(e.KElementGlobalCoord[r, c], k[r,c], 0.0001, "Error in row "+r+" col " +c);
                }
            }
        }

        /*[TestMethod]
        public void KMatrixTest2()
        {
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0.0));
            nds.Add(new Node(1.0, 0.0, 0.0));
            nds.Add(new Node(0.0, 1.0, 0.0));
            nds.Add(new Node(0.0, 0.0, 1.0));

            Assert.AreEqual(0.16666666666666, Tethraedron4.GetVolume(nds.ToArray()), 0.00001);

            SteelMaterial mat = new SteelMaterial("mat", 1.0, 0.0, 355, 510.0);

            BrickProperty brickProperty = new BrickProperty(mat, "propr");

            Tethraedron4 e = new Tethraedron4(nds.ToArray(), brickProperty);
            e.BuildMatrix();

            FEMUtilities.WriteMatrix(e.KElementLocalCoord);

            //Local axis == global axis
            for (int r = 0; r < 12; r++)
            {
                for (int c = 0; c < 12; c++)
                {
                    Assert.AreEqual(e.KElementLocalCoord[r, c], e.KElementGlobalCoord[r, c], 0.00000001);
                }
            }

            
            List<mnl.Vector<double>> rows = new List<mnl.Vector<double>>();

            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 0.3333, 0.0833, 0.0833, -0.1667, -0.0833, -0.0833, -0.0833, 0, 0, -0.0833, 0, 0 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 108, 344, 54, -24, 104, 42, -24, -216, -12, -60, -232, -84 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 24, 54, 113, 0, 30, 35, 0, -24, -54, -24, -60, -94 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -1, -24, 0, 29, -18, -12, -18, 24, 0, -10, 18, 12 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 6, 104, 30, -18, 44, 18, 12, -72, -12, 0, -76, -36 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 12, 42, 35, -12, 18, 29, 0, -24, -18, 0, -36, -46 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -54, -24, 0, -18, 12, 0, 36, 0, 0, 36, 12, 0 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -48, -216, -24, 24, -72, -24, 0, 144, 0, 24, 144, 48 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { 0, -12, -54, 0, -12, -18, 0, 0, 36, 0, 24, 36 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -94, -60, -24, -10, 0, 0, 36, 24, 0, 68, 36, 24 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -66, -232, -60, 18, -76, -36, 12, 144, 24, 36, 164, 72 }));
            rows.Add(mnl.Vector<double>.Build.DenseOfArray(new double[] { -36, -84, -94, 12, -36, -46, 0, 48, 36, 24, 72, 104 }));

            mnl.Matrix<double> k = mnl.Matrix<double>.Build.Dense(0, 12);

            int index = 0;
            rows.ForEach(r => { k = k.InsertRow(index, r); index++; });

            for (int r = 0; r < 12; r++)
            {
                for (int c = 0; c < 12; c++)
                {
                    Assert.AreEqual(e.KElementGlobalCoord[r, c], k[r, c], 0.0001, "Error in row " + r + " col " + c);
                }
            }
        }*/
    }
}
