using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM.Properties;
using GPC.Model.Materials;

namespace FemTest.Solver
{
    [TestClass]
    public class TetrahedronTest
    {
        [TestMethod]
        public void VolumeTest1()
        {
            double a = 1.0;
            double b = 1.0;
            double h = 1.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0, 0));
            nds.Add(new Node(a, 0.0, 0, 0));
            nds.Add(new Node(0.0, b, 0, 0));
            nds.Add(new Node(0.0, 0.0, h, 0));

            double A = (1.0 / 2.0 * a * b);
            double V = A * h / 3.0;
            Assert.AreEqual(V, Tethraedron.GetVolume(nds.ToArray()));
        }

        [TestMethod]
        public void GetCoefficientTest1()
        {
            double x = 1.0;
            double y = 1.0;
            double h = 1.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0, 0));
            nds.Add(new Node(x, 0.0, 0, 0));
            nds.Add(new Node(0.0, y, 0, 0));
            nds.Add(new Node(0.0, 0.0, h, 0));
            
            Func<int, Node, double> N = (int index, Node n) =>
            {
                double V = Tethraedron.GetVolume(nds.ToArray());
                double a = Tethraedron.GetCoefficientShapeFunction(index, "a", nds.ToArray());
                double b = Tethraedron.GetCoefficientShapeFunction(index, "b", nds.ToArray());
                double c = Tethraedron.GetCoefficientShapeFunction(index, "c", nds.ToArray());
                double d = Tethraedron.GetCoefficientShapeFunction(index, "d", nds.ToArray());
                return 1.0 / (6.0 * V) * (a + b * n.Position.X + c * n.Position.Y + d * n.Position.Z);
            };

            Assert.AreEqual(-1.0, Tethraedron.GetCoefficientShapeFunction(1, "b", nds.ToArray()));
            Assert.AreEqual(1.0, Tethraedron.GetCoefficientShapeFunction(2, "b", nds.ToArray()));

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
        public void KMatrixTest1()
        {
            List<Node> nds = new List<Node>();
            nds.Add(new Node(2.0, 3.0, 4.0, 0));
            nds.Add(new Node(6.0, 3.0, 2.0, 0));
            nds.Add(new Node(2.0, 5.0, 1.0, 0));
            nds.Add(new Node(4.0, 3.0, 6.0, 0));

            Assert.AreEqual(4.0, Tethraedron.GetVolume(nds.ToArray()));

            SteelMaterial mat = new SteelMaterial("mat", 96.0, 1.0 / 3.0, 355, 510, 7850.0);

            BrickProperty brickProperty = new BrickProperty(mat);

            Tethraedron e = new Tethraedron(nds.ToArray(), brickProperty, 1);
            e.BuildMatrix();

            Util.WriteMatrix(e.KElementLocalCoord);

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

            /*149 108 24 −1 6 12 −54 −48 0 −94 −66 −36
            108 344 54 −24 104 42 −24 −216 −12 −60 −232 −84
            24 54 113 0 30 35 0 −24 −54 −24 −60 −94
            −1 −24 0 29 −18 −12 −18 24 0 −10 18 12
            6 104 30 −18 44 18 12 −72 −12 0 −76 −36
            12 42 35 −12 18 29 0 −24 −18 0 −36 −46
            −54 −24 0 −18 12 0 36 0 0 36 12 0
            −48 −216 −24 24 −72 −24 0 144 0 24 144 48
            0 −12 −54 0 −12 −18 0 0 36 0 24 36
            −94 −60 −24 −10 0 0 36 24 0 68 36 24
            −66 −232 −60 18 −76 −36 12 144 24 36 164 72
            −36 −84 −94 12 −36 −46 0 48 36 24 72 104*/

            Assert.AreEqual(e.KElementGlobalCoord[0, 0], 149, 0.0001);
            Assert.AreEqual(e.KElementGlobalCoord[0, 1], 108, 0.0001);
            Assert.AreEqual(e.KElementGlobalCoord[0, 2], 24, 0.0001);

            Assert.AreEqual(e.KElementGlobalCoord[0, 3], -1, 0.0001);
            Assert.AreEqual(e.KElementGlobalCoord[0, 4], 6, 0.0001);
            Assert.AreEqual(e.KElementGlobalCoord[0, 5], 12, 0.0001);

            Assert.AreEqual(e.KElementGlobalCoord[0, 6], -54, 0.0001);
            Assert.AreEqual(e.KElementGlobalCoord[0, 7], -48, 0.0001);
            Assert.AreEqual(e.KElementGlobalCoord[0, 8], 0, 0.0001);

            Assert.AreEqual(e.KElementGlobalCoord[0, 9], -94, 0.0001);
            Assert.AreEqual(e.KElementGlobalCoord[0, 10], -66, 0.0001);
            Assert.AreEqual(e.KElementGlobalCoord[0, 11], -36, 0.0001);
        }
    }
}
