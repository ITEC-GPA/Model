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
    public class FemSolverQuad4GQ12MembranalTest
    {
        /*[TestMethod]
        public void Quad4GQ12MembranalTestB()
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

            int i = 0;
            nds.ToList().ForEach(x => {
                i++;
                Console.WriteLine("B signed in Node " + i);
                Util.WriteMatrix(Quad4QFSUQMembranal.BMatrix(x.Position.X, x.Position.Y, nds));
                }
            );
        }*/
        

        /// <summary>
        /// TEST LOCAL MATRIX
        /// </summary>
        
        [TestMethod]
        public void Quad4GQ12MembranalTest1()
        {
            double E = 1.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            Node[] nds = new Node[4];
            nds[0] = new Node(-1.0, -1.0, 0, "1");
            nds[1] = new Node(+1.0, -1.0, 0, "2");
            nds[2] = new Node(+1.0, +1.0, 0, "3");
            nds[3] = new Node(-1.0, +1.0, 0, "4");

            Quad4GQ12Membranal el = new Quad4GQ12Membranal(nds);
            el.SetProperty(prop);
            el.BuildMatrix();
            mnl.Matrix<double> k1 = el.KElementLocalCoord;

            Console.WriteLine("k1 ");
            Util.WriteMatrix(k1, "F3");

            nds[0] = new Node(0.0, 0.0, 0, "1");
            nds[1] = new Node(2.0, 0.0, 0, "2");
            nds[2] = new Node(2.0, 2.0, 0, "3");
            nds[3] = new Node(0.0, 2.0, 0, "4");

            el = new Quad4GQ12Membranal(nds);
            el.SetProperty(prop);
            el.BuildMatrix();
            k1 = el.KElementLocalCoord;

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

        ///SINGLE ELEMENT WITH X AXIAL FORCES
        [TestMethod]
        public void Quad4GQ12MembranalTest1a()
        {
            double E = 1.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 0, 1);

            List<Node> nds = new List<Node>();
            /*nds.Add( new Node(+0.0, +0.0, 0, 1, "1"));
            nds.Add( new Node(+1.0, +0.0, 0, 2, "2"));
            nds.Add( new Node(+1.0, +1.0, 0, 3, "3"));
            nds.Add( new Node(+0.0, +1.0, 0, 4, "4"));*/

            nds.Add(new Node(-1.0, -1.0, 0, "1"));
            nds.Add(new Node(+1.0, -1.0, 0, "2"));
            nds.Add(new Node(+1.0, +1.0, 0, "3"));
            nds.Add(new Node(-1.0, +1.0, 0, "4"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute(freedomCase, sys);
            hinge.AddExternalRestrain(LinearSolver.DOF.DX);
            hinge.AddExternalRestrain(LinearSolver.DOF.DY);
            hinge.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));

            nds[1 - 1].AddAttribute(hinge);
            nds[4 - 1].AddAttribute(hinge);
            //nds[4 - 1].AddAttribute(dx);

            LoadCase lc = new LoadCase("lc");
            double q = 1.0;
            double F = 0.5 * q;
            double M = 0.0; // F * (0.5 * 0.5) / 2.0;
            NodeForceAttribute FTop = new NodeForceAttribute(lc, sys, F, 0.0, 0, 0, 0, M);
            NodeForceAttribute FBottom = new NodeForceAttribute(lc, sys, F, 0.0, 0, 0, 0, -M);
            nds[2 - 1].AddAttribute(FBottom);
            nds[3 - 1].AddAttribute(FTop);

            List<Quad4GQ12Membranal> els = new List<Quad4GQ12Membranal>();
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[0], nds[1], nds[2], nds[3] }));
            els.ForEach(el => el.SetProperty(prop));
            LinearSolver fem = new LinearSolver(els.ToArray());
        }

        //DISTORTED ELEMENT WITH AXIAL FORCE
        /*public void Quad4MQ2IbraMembranalTest2()
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
        }*/

        

        /*[TestMethod]
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
        }*/

        /// <summary>
        /// A cantilever beam - A robust quadrilateral membrane finite element with drilling degrees of freedom - adnan ibrahimbegovic, taylor, wilson - 1990
        /// Mesh 4x1
        /// </summary>
        [TestMethod]
        public void Quad4GQ12MembranalTest3()
        {
            double E = 30000.0;
            double ni = 0.25;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0, 0, "1"));
            nds.Add(new Node(12.0, 0, 0, "2"));
            nds.Add(new Node(24.0, 0, 0, "3"));
            nds.Add(new Node(36.0, 0, 0, "4"));
            nds.Add(new Node(48.0, 0, 0, "5"));

            nds.Add(new Node(0.0, 12.0, 0, "6"));
            nds.Add(new Node(12.0, 12.0, 0, "7"));
            nds.Add(new Node(24.0, 12.0, 0, "8"));
            nds.Add(new Node(36.0, 12.0, 0, "9"));
            nds.Add(new Node(48.0, 12.0, 0, "10"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute(freedomCase, sys);
            hinge.AddExternalRestrain(LinearSolver.DOF.DX);
            hinge.AddExternalRestrain(LinearSolver.DOF.DY);
            hinge.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute dx = new NodeRestrainAttribute(freedomCase, sys);
            dx.AddExternalRestrain(LinearSolver.DOF.DX);

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));

            //nds[1-1].AddAttribute(dx);
            nds[1 - 1].AddAttribute(hinge);
            nds[6 - 1].AddAttribute(hinge);  

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute F = new NodeForceAttribute(lc, sys, 0, 20.0, 0, 0, 0, 0);
            nds[4].AddAttribute(F);
            nds[9].AddAttribute(F);

            List<Quad4GQ12Membranal> els = new List<Quad4GQ12Membranal>();
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[0], nds[1], nds[6], nds[5] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[1], nds[2], nds[7], nds[6] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[2], nds[3], nds[8], nds[7] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[3], nds[4], nds[9], nds[8] }));
            els.ForEach(el => el.SetProperty(prop));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(0.3283, fem.GetDisplacementGlobalCoordinates(nds[4], LinearSolver.DOF.DY), 0.001);

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
        /// Mesh 4x2
        /// </summary>
        [TestMethod]
        public void Quad4GQ12MembranalTest3a()
        {
            double E = 30000.0;
            double ni = 0.25;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0, 0, "1"));
            nds.Add(new Node(12.0, 0, 0, "2"));
            nds.Add(new Node(24.0, 0, 0, "3"));
            nds.Add(new Node(36.0, 0, 0, "4"));
            nds.Add(new Node(48.0, 0, 0, "5"));

            nds.Add(new Node(0.0, 6.0, 0, "6"));
            nds.Add(new Node(12.0, 6.0, 0, "7"));
            nds.Add(new Node(24.0, 6.0, 0, "8"));
            nds.Add(new Node(36.0, 6.0, 0, "9"));
            nds.Add(new Node(48.0, 6.0, 0, "10"));

            nds.Add(new Node(0.0, 12.0, 0, "11"));
            nds.Add(new Node(12.0, 12.0, 0, "12"));
            nds.Add(new Node(24.0, 12.0, 0, "13"));
            nds.Add(new Node(36.0, 12.0, 0, "14"));
            nds.Add(new Node(48.0, 12.0, 0, "15"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(freedomCase, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute dx = new NodeRestrainAttribute(freedomCase, sys);
            dx.AddExternalRestrain(LinearSolver.DOF.DX);

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));

            nds[1 - 1].AddAttribute(fix);
            nds[6 - 1].AddAttribute(fix);
            nds[11 - 1].AddAttribute(fix);

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute F = new NodeForceAttribute(lc, sys, 0, 20.0, 0, 0, 0, 0);
            nds[4].AddAttribute(F);
            nds[9].AddAttribute(F);

            List<Quad4GQ12Membranal> els = new List<Quad4GQ12Membranal>();
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[0], nds[1], nds[6], nds[5] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[1], nds[2], nds[7], nds[6] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[2], nds[3], nds[8], nds[7] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[3], nds[4], nds[9], nds[8] }));

            els.Add(new Quad4GQ12Membranal(new Node[] { nds[5], nds[6], nds[11], nds[10] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[6], nds[7], nds[12], nds[11] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[7], nds[8], nds[13], nds[12] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[8], nds[9], nds[14], nds[13] }));

            els.ForEach(el => el.SetProperty(prop));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(1.0, fem.GetDisplacementGlobalCoordinates(nds[9], LinearSolver.DOF.DY) / 0.3553, 0.04);

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
        /// Mesh 8x2
        /// </summary>
        [TestMethod]
        public void Quad4GQ12MembranalTest3b()
        {
            double E = 30000.0;
            double ni = 0.25;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0, 0, "1"));
            nds.Add(new Node(6.0, 0, 0, "2"));
            nds.Add(new Node(12.0, 0, 0, "3"));
            nds.Add(new Node(18.0, 0, 0, "4"));
            nds.Add(new Node(24.0, 0, 0, "5"));
            nds.Add(new Node(30.0, 0, 0, "6"));
            nds.Add(new Node(36.0, 0, 0, "7"));
            nds.Add(new Node(42.0, 0, 0, "8"));
            nds.Add(new Node(48.0, 0, 0, "9"));

            nds.Add(new Node(0.0, 6.0, 0, "10"));
            nds.Add(new Node(6.0, 6.0, 0, "11"));
            nds.Add(new Node(12.0, 6.0, 0, "12"));
            nds.Add(new Node(18.0, 6.0, 0, "13"));
            nds.Add(new Node(24.0, 6.0, 0, "14"));
            nds.Add(new Node(30.0, 6.0, 0, "15"));
            nds.Add(new Node(36.0, 6.0, 0, "16"));
            nds.Add(new Node(42.0, 6.0, 0, "17"));
            nds.Add(new Node(48.0, 6.0, 0, "18"));

            nds.Add(new Node(0.0, 12.0, 0, "19"));
            nds.Add(new Node(6.0, 12.0, 0, "20"));
            nds.Add(new Node(12.0, 12.0, 0, "21"));
            nds.Add(new Node(18.0, 12.0, 0, "22"));
            nds.Add(new Node(24.0, 12.0, 0, "23"));
            nds.Add(new Node(30.0, 12.0, 0, "24"));
            nds.Add(new Node(36.0, 12.0, 0, "25"));
            nds.Add(new Node(42.0, 12.0, 0, "26"));
            nds.Add(new Node(48.0, 12.0, 0, "27"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(freedomCase, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute dx = new NodeRestrainAttribute(freedomCase, sys);
            dx.AddExternalRestrain(LinearSolver.DOF.DX);

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));

            nds[1 - 1].AddAttribute(fix);
            nds[19 - 1].AddAttribute(fix);
            nds[10 - 1].AddAttribute(fix);

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute Fcent = new NodeForceAttribute(lc, sys, 0, 20.0, 0, 0, 0, 0);
            NodeForceAttribute Fext = new NodeForceAttribute(lc, sys, 0, 10.0, 0, 0, 0, 0);
            nds[9 - 1].AddAttribute(Fext);
            nds[18 - 1].AddAttribute(Fcent);
            nds[27 - 1].AddAttribute(Fext);

            List<Quad4GQ12Membranal> els = new List<Quad4GQ12Membranal>();
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[0], nds[1], nds[10], nds[9] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[1], nds[2], nds[11], nds[10] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[2], nds[3], nds[12], nds[11] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[3], nds[4], nds[13], nds[12] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[4], nds[5], nds[14], nds[13] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[5], nds[6], nds[15], nds[14] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[6], nds[7], nds[16], nds[15] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[7], nds[8], nds[17], nds[16] }));

            els.Add(new Quad4GQ12Membranal(new Node[] { nds[9], nds[10], nds[19], nds[18] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[10], nds[11], nds[20], nds[19] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[11], nds[12], nds[21], nds[20] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[12], nds[13], nds[22], nds[21] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[13], nds[14], nds[23], nds[22] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[14], nds[15], nds[24], nds[23] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[15], nds[16], nds[25], nds[24] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[16], nds[17], nds[26], nds[25] }));
            els.ForEach(el => el.SetProperty(prop));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(0.3553, fem.GetDisplacementGlobalCoordinates(nds[17], LinearSolver.DOF.DY), 0.01);

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
        /// Mesh 6 x 1
        /// </summary>
        
        public void Quad4GQ12MembranalTest4()
        {
            double E = 100.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0 * 10.0 / 6.0, 0, 0, "0"));
            nds.Add(new Node(1.0 * 10.0 / 6.0, 0, 0, "1"));
            nds.Add(new Node(2.0 * 10.0 / 6.0, 0, 0, "2"));
            nds.Add(new Node(3.0 * 10.0 / 6.0, 0, 0, "3"));
            nds.Add(new Node(4.0 * 10.0 / 6.0, 0, 0, "4"));
            nds.Add(new Node(5.0 * 10.0 / 6.0, 0, 0, "5"));
            nds.Add(new Node(6.0 * 10.0 / 6.0, 0, 0, "6"));

            nds.Add(new Node(0.0 * 10.0 / 6.0, 1.0, 0, "7"));
            nds.Add(new Node(1.0 * 10.0 / 6.0, 1.0, 0, "8"));
            nds.Add(new Node(2.0 * 10.0 / 6.0, 1.0, 0, "9"));
            nds.Add(new Node(3.0 * 10.0 / 6.0, 1.0, 0, "10"));
            nds.Add(new Node(4.0 * 10.0 / 6.0, 1.0, 0, "11"));
            nds.Add(new Node(5.0 * 10.0 / 6.0, 1.0, 0, "12"));
            nds.Add(new Node(6.0 * 10.0 / 6.0, 1.0, 0, "13"));

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

            List<Quad4GQ12Membranal> els = new List<Quad4GQ12Membranal>();
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[0], nds[1], nds[8], nds[7] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[1], nds[2], nds[9], nds[8] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[2], nds[3], nds[10], nds[9] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[3], nds[4], nds[11], nds[10] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[4], nds[5], nds[12], nds[11] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[5], nds[6], nds[13], nds[12] }));
            els.ForEach(el => el.SetProperty(prop));
            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(1.5, fem.GetDisplacementGlobalCoordinates(nds[10], LinearSolver.DOF.DY));

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
        /// Mesh 6x1
        /// </summary>
        
        public void Quad4GQ12MembranalTest5()
        {
            double E = 100.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0 * 10.0 / 6.0, 0, 0, "0"));
            nds.Add(new Node(1.0 * 10.0 / 6.0, 0, 0, "1"));
            nds.Add(new Node(2.0 * 10.0 / 6.0, 0, 0, "2"));
            nds.Add(new Node(3.0 * 10.0 / 6.0, 0, 0, "3"));
            nds.Add(new Node(4.0 * 10.0 / 6.0, 0, 0, "4"));
            nds.Add(new Node(5.0 * 10.0 / 6.0, 0, 0, "5"));
            nds.Add(new Node(6.0 * 10.0 / 6.0, 0, 0, "6"));

            nds.Add(new Node(0.0 * 10.0 / 6.0, 1.0, 0, "7"));
            nds.Add(new Node(1.0 * 10.0 / 6.0, 1.0, 0, "8"));
            nds.Add(new Node(2.0 * 10.0 / 6.0, 1.0, 0, "9"));
            nds.Add(new Node(3.0 * 10.0 / 6.0, 1.0, 0, "10"));
            nds.Add(new Node(4.0 * 10.0 / 6.0, 1.0, 0, "11"));
            nds.Add(new Node(5.0 * 10.0 / 6.0, 1.0, 0, "12"));
            nds.Add(new Node(6.0 * 10.0 / 6.0, 1.0, 0, "13"));

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

            List<Quad4GQ12Membranal> els = new List<Quad4GQ12Membranal>();
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[0], nds[1], nds[8], nds[7] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[1], nds[2], nds[9], nds[8] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[2], nds[3], nds[10], nds[9] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[3], nds[4], nds[11], nds[10] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[4], nds[5], nds[12], nds[11] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[5], nds[6], nds[13], nds[12] }));
            els.ForEach(el => el.SetProperty(prop));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(1.5, fem.GetDisplacementGlobalCoordinates(nds[10], LinearSolver.DOF.DY));

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
        /// Mesh 10x1
        /// </summary>
        [TestMethod]
        public void Quad4GQ12MembranalTest4a()
        {
            double E = 100.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0, 0, "0"));
            nds.Add(new Node(1.0, 0, 0, "1"));
            nds.Add(new Node(2.0, 0, 0, "2"));
            nds.Add(new Node(3.0, 0, 0, "3"));
            nds.Add(new Node(4.0, 0, 0, "4"));
            nds.Add(new Node(5.0, 0, 0, "5"));
            nds.Add(new Node(6.0, 0, 0, "6"));
            nds.Add(new Node(7.0, 0, 0, "7"));
            nds.Add(new Node(8.0, 0, 0, "8"));
            nds.Add(new Node(9.0, 0, 0, "9"));
            nds.Add(new Node(10.0, 0, 0, "10"));

            nds.Add(new Node(0.0, 1.0, 0, "11"));
            nds.Add(new Node(1.0, 1.0, 0, "12"));
            nds.Add(new Node(2.0, 1.0, 0, "13"));
            nds.Add(new Node(3.0, 1.0, 0, "14"));
            nds.Add(new Node(4.0, 1.0, 0, "15"));
            nds.Add(new Node(5.0, 1.0, 0, "16"));
            nds.Add(new Node(6.0, 1.0, 0, "17"));
            nds.Add(new Node(7.0, 1.0, 0, "18"));
            nds.Add(new Node(8.0, 1.0, 0, "19"));
            nds.Add(new Node(9.0, 1.0, 0, "20"));
            nds.Add(new Node(10.0, 1.0, 0, "21"));

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

            List<Quad4GQ12Membranal> els = new List<Quad4GQ12Membranal>();
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[0], nds[1], nds[12], nds[11] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[1], nds[2], nds[13], nds[12] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[2], nds[3], nds[14], nds[13] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[3], nds[4], nds[15], nds[14] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[4], nds[5], nds[16], nds[15] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[5], nds[6], nds[17], nds[16] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[6], nds[7], nds[18], nds[17] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[7], nds[8], nds[19], nds[18] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[8], nds[9], nds[20], nds[19] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[9], nds[10], nds[21], nds[20] }));
            els.ForEach(el => el.SetProperty(prop));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(1.5, fem.GetDisplacementGlobalCoordinates(nds[16], LinearSolver.DOF.DY), 0.001);
            Assert.AreEqual(0.3, fem.GetDisplacementGlobalCoordinates(nds[16], LinearSolver.DOF.DX), 0.001);

            Assert.AreEqual(1.5, fem.GetDisplacementGlobalCoordinates(nds[5], LinearSolver.DOF.DY), 0.001);
            Assert.AreEqual(0.3, fem.GetDisplacementGlobalCoordinates(nds[5], LinearSolver.DOF.DX), 0.001);

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
        /// 10x1
        /// </summary>
        [TestMethod]
        public void Quad4GQ12MembranalTest5a()
        {
            double E = 100.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0, 0, "0"));
            nds.Add(new Node(1.0, 0, 0, "1"));
            nds.Add(new Node(2.0, 0, 0, "2"));
            nds.Add(new Node(3.0, 0, 0, "3"));
            nds.Add(new Node(4.0, 0, 0, "4"));
            nds.Add(new Node(5.0, 0, 0, "5"));
            nds.Add(new Node(6.0, 0, 0, "6"));
            nds.Add(new Node(7.0, 0, 0, "7"));
            nds.Add(new Node(8.0, 0, 0, "8"));
            nds.Add(new Node(9.0, 0, 0, "9"));
            nds.Add(new Node(10.0, 0, 0, "10"));

            nds.Add(new Node(0.0, 1.0, 0, "11"));
            nds.Add(new Node(1.0, 1.0, 0, "12"));
            nds.Add(new Node(2.0, 1.0, 0, "13"));
            nds.Add(new Node(3.0, 1.0, 0, "14"));
            nds.Add(new Node(4.0, 1.0, 0, "15"));
            nds.Add(new Node(5.0, 1.0, 0, "16"));
            nds.Add(new Node(6.0, 1.0, 0, "17"));
            nds.Add(new Node(7.0, 1.0, 0, "18"));
            nds.Add(new Node(8.0, 1.0, 0, "19"));
            nds.Add(new Node(9.0, 1.0, 0, "20"));
            nds.Add(new Node(10.0, 1.0, 0, "21"));

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

            List<Quad4GQ12Membranal> els = new List<Quad4GQ12Membranal>();
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[0], nds[1], nds[12], nds[11] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[1], nds[2], nds[13], nds[12] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[2], nds[3], nds[14], nds[13] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[3], nds[4], nds[15], nds[14] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[4], nds[5], nds[16], nds[15] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[5], nds[6], nds[17], nds[16] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[6], nds[7], nds[18], nds[17] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[7], nds[8], nds[19], nds[18] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[8], nds[9], nds[20], nds[19] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[9], nds[10], nds[21], nds[20] }));
            els.ForEach(el => el.SetProperty(prop));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(1.5, fem.GetDisplacementGlobalCoordinates(nds[16], LinearSolver.DOF.DY), 0.01);
            Assert.AreEqual(0.3, fem.GetDisplacementGlobalCoordinates(nds[16], LinearSolver.DOF.DX), 0.01);

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

        /// <summary>
        /// Cook's problem
        /// </summary>
        [TestMethod]
        public void Quad4GQ12MembranalTest6()
        {
            //TODO: assicurarsi convergenza con + elementi
            double E = 1;
            double ni = 1.0 / 3.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0, 0, "0"));
            nds.Add(new Node(24.0, 22.0, 0, "1"));
            nds.Add(new Node(24.0, 35.0, 0, "2"));
            nds.Add(new Node(0.0, 22.0, 0, "3"));
            nds.Add(new Node(24.0, 52.0, 0, "4"));
            nds.Add(new Node(0.0, 44.0, 0, "5"));
            nds.Add(new Node(48.0, 44.0, 0, "6"));
            nds.Add(new Node(48.0, 52.0, 0, "7"));
            nds.Add(new Node(48.0, 60.0, 0, "8"));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            FreedomCase freedomCase = new FreedomCase("freedomcase");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(freedomCase, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute shareFix = new NodeRestrainAttribute(freedomCase, sys);
            shareFix.AddExternalRestrain(LinearSolver.DOF.DZ);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));
            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);
            nds[5].AddAttribute(fix);

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute Fc = new NodeForceAttribute(lc, sys, 0.0, 0.5, 0, 0, 0, 0.0);
            NodeForceAttribute Fl = new NodeForceAttribute(lc, sys, 0.0, 0.25, 0, 0, 0, 0.0);
            nds[6].AddAttribute(Fc);
            nds[7].AddAttribute(Fl);
            nds[8].AddAttribute(Fl);

            List<Quad4GQ12Membranal> els = new List<Quad4GQ12Membranal>();
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[0], nds[1], nds[2], nds[3] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[3], nds[2], nds[4], nds[5] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[1], nds[6], nds[7], nds[2] }));
            els.Add(new Quad4GQ12Membranal(new Node[] { nds[2], nds[7], nds[8], nds[4] }));
            els.ForEach(el => el.SetProperty(prop));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(23.91, fem.GetDisplacementGlobalCoordinates(nds[7], LinearSolver.DOF.DY), 0.01);

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