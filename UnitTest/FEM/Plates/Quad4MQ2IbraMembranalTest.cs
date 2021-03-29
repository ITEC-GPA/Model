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

namespace FemTest.Solver
{
    [TestClass]
    public class FemSolverQuad4MQ2IbraMembranalTest
    {
        /// <summary>
        /// TEST LOCAL MATRIX
        /// </summary>
        [TestMethod]
        public void Quad4MQ2IbraMembranalTest1()
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

            Quad4MQ2IbraMembranal el = new Quad4MQ2IbraMembranal(nds, prop, 1);
            el.BuildMatrix();
            mnl.Matrix<double> k1 = el.KElementGlobalCoord;

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

        [TestMethod]
        public void Quad4MQ2IbraMembranalTest2()
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
        }

        [TestMethod]
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
            /*double px = 0.1;
            PlatePressureAttribute pressure = new PlatePressureAttribute(lc, sys, px, 0, 0);*/
            NodeForceAttribute F = new NodeForceAttribute(lc, sys, 0, 20.0, 0, 0, 0, 0);
            nds[4].AddAttribute(F);
            nds[9].AddAttribute(F);

            List<Quad4Membranal> els = new List<Quad4Membranal>();
            els.Add(new Quad4Membranal(new Node[] { nds[0], nds[1], nds[6], nds[5] }, prop, 1));
            els.Add(new Quad4Membranal(new Node[] { nds[1], nds[2], nds[7], nds[6] }, prop, 1));
            els.Add(new Quad4Membranal(new Node[] { nds[2], nds[3], nds[8], nds[7] }, prop, 1));
            els.Add(new Quad4Membranal(new Node[] { nds[3], nds[4], nds[9], nds[8] }, prop, 1));

            LinearSolver fem = new LinearSolver(els.ToArray());
            Console.WriteLine("kGlob=" + fem.KGlobal);
            Console.WriteLine("F=" + fem.F);
        }

        /// <summary>
        /// A cantilever beam - A robust quadrilateral membrane finite element with drilling degrees of freedom - adnan ibrahimbegovic, taylor, wilson - 1990
        /// </summary>
        [TestMethod]
        public void Quad4MQ2IbraMembranalTest3()
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
            shareFix.AddExternalRestrain(LinearSolver.DOF.RX);
            shareFix.AddExternalRestrain(LinearSolver.DOF.RY);

            nds.ForEach(x => x.AddAttribute(shareFix));

            nds[1-1].AddAttribute(dx);
            nds[6-1].AddAttribute(hinge);  

            LoadCase lc = new LoadCase("lc");
            /*double px = 0.1;
            PlatePressureAttribute pressure = new PlatePressureAttribute(lc, sys, px, 0, 0);*/
            NodeForceAttribute F = new NodeForceAttribute(lc, sys, 0, 20.0, 0, 0, 0, 0);
            nds[4].AddAttribute(F);
            nds[9].AddAttribute(F);

            List<Quad4MQ2IbraMembranal> els = new List<Quad4MQ2IbraMembranal>();
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[0], nds[1], nds[6], nds[5] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[1], nds[2], nds[7], nds[6] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[2], nds[3], nds[8], nds[7] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[3], nds[4], nds[9], nds[8] }, prop, 1));

            LinearSolver fem = new LinearSolver(els.ToArray());
            Console.WriteLine("kGlob="+fem.KGlobal);
            Console.WriteLine("F="+fem.F);

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

        [TestMethod]
        public void Quad4MQ2IbraMembranalTest4()
        {
            double E = 100.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("mat", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0 * 10.0 / 6.0, 0, 0, 1, "0"));
            nds.Add(new Node(1.0 * 10.0 / 6.0, 0, 0, 1, "1"));
            nds.Add(new Node(2.0 * 10.0 / 6.0, 0, 0, 1, "2"));
            nds.Add(new Node(3.0 * 10.0 / 6.0, 0, 0, 1, "3"));
            nds.Add(new Node(4.0 * 10.0 / 6.0, 0, 0, 1, "4"));
            nds.Add(new Node(5.0 * 10.0 / 6.0, 0, 0, 1, "5"));
            nds.Add(new Node(6.0 * 10.0 / 6.0, 0, 0, 1, "6"));

            nds.Add(new Node(0.0 * 10.0 / 6.0, 1.0, 0, 1, "7"));
            nds.Add(new Node(1.0 * 10.0 / 6.0, 1.0, 0, 1, "8"));
            nds.Add(new Node(2.0 * 10.0 / 6.0, 1.0, 0, 1, "9"));
            nds.Add(new Node(3.0 * 10.0 / 6.0, 1.0, 0, 1, "10"));
            nds.Add(new Node(4.0 * 10.0 / 6.0, 1.0, 0, 1, "11"));
            nds.Add(new Node(5.0 * 10.0 / 6.0, 1.0, 0, 1, "12"));
            nds.Add(new Node(6.0 * 10.0 / 6.0, 1.0, 0, 1, "13"));

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
            nds[1 - 1].AddAttribute(dx);
            nds[6 - 1].AddAttribute(hinge);

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute Fplus = new NodeForceAttribute(lc, sys, 0, 1.0, 0, 0, 0, 0);
            NodeForceAttribute Fminus = new NodeForceAttribute(lc, sys, 0, -1.0, 0, 0, 0, 0);
            nds[1 - 1].AddAttribute(Fplus);
            nds[12 - 1].AddAttribute(Fplus);
            nds[6 - 1].AddAttribute(Fminus);
            nds[7 - 1].AddAttribute(Fminus);

            List<Quad4MQ2IbraMembranal> els = new List<Quad4MQ2IbraMembranal>();
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[0], nds[1], nds[8], nds[7] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[1], nds[2], nds[9], nds[8] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[2], nds[3], nds[10], nds[9] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[3], nds[4], nds[11], nds[10] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[4], nds[5], nds[12], nds[11] }, prop, 1));
            els.Add(new Quad4MQ2IbraMembranal(new Node[] { nds[5], nds[6], nds[13], nds[12] }, prop, 1));
            
            LinearSolver fem = new LinearSolver(els.ToArray());

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
    }
}