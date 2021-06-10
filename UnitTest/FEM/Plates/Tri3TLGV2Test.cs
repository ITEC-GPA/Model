using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.Materials;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using System.Collections.Generic;
using GPC.Model.FEM;
using GPC.Model.FEM.Materials;

namespace FemTest.SolverTest
{
    [TestClass]
    public class Tri3TripledLaminatedGlassV2Test
    {
        /// <summary>
        /// triangolo equilatero, Check KDTK
        /// </summary>
        [TestMethod]
        public void Test1()
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(0.0, 0.0, 0));
            nodes.Add(new Node(1.0, 0.0, 0));
            nodes.Add(new Node(Math.Cos(60.0 * Math.PI / 180.0), Math.Sin(60.0 * Math.PI / 180.0), 0));

            double hTot = 1.0; //Jtot = 1/12 * 1 * (h1^3 + h2^3) = 2/12 * 1 * (hi^3) 
            double jTot = 1.0 / 12.0 * Math.Pow(hTot, 3.0);
            double h1 = Math.Pow(6.0 * jTot, 1.0 / 3.0);
            double h2 = Math.Pow(6.0 * jTot, 1.0 / 3.0);
            double EGlass = 12.0;
            double niGlass = 0.0;

            double G0 = 0.0;
            double h0 = 0.1;

            PlateProperty p = new PlateProperty(new IsotropicFemMaterial(EGlass, niGlass, 0, 0), hTot, hTot, "");

            List<FiniteElement> els = new List<FiniteElement>();
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[0], nodes[1], nodes[2] }, G0, h0, h1, h2, EGlass, niGlass));
            els.Add(new Tri3DK(new Node[] { nodes[0], nodes[1], nodes[2] }, p));

            els[0].BuildMatrix();
            els[1].BuildMatrix();

            var mDKT = els[1].KElementLocalCoord;
            var mTLG = els[0].KElementLocalCoord;

            FEMUtilities.WriteMatrix("Tri3TLG local matrix = ", els[0].KElementLocalCoord);
            //FEMUtilities.WriteMatrix("Tri3TLG global matrix = ", els[0].KElementGlobalCoord);

            FEMUtilities.WriteMatrix("DKT local matrix = ", els[1].KElementLocalCoord);
            //FEMUtilities.WriteMatrix("DKT global matrix = ", els[1].KElementGlobalCoord);

            var indexes = new int[9] { 0, 1, 2, 5, 6, 7, 10, 11, 12 };
            var indexesDKT = new int[9] { 0, 1, 2, 3, 4, 5, 6, 7, 8 };
            for (int i = 0; i < indexes.Length; i++)
            {
                var ind = indexes[i];
                var indDKT = indexesDKT[i];

                Assert.AreEqual(mDKT[indDKT, 0], mTLG[ind, 0], 1e-4, "k[" + ind + ", 0] different");
                Assert.AreEqual(mDKT[indDKT, 1], mTLG[ind, 1], 1e-4, "k[" + ind + ",1] different");
                Assert.AreEqual(mDKT[indDKT, 2], mTLG[ind, 2], 1e-4, "k[" + ind + ",2] different");

                Assert.AreEqual(mDKT[indDKT, 3], mTLG[ind, 5], 1e-4, "k[" + ind + ",5] different");
                Assert.AreEqual(mDKT[indDKT, 4], mTLG[ind, 6], 1e-4, "k[" + ind + ",6] different");
                Assert.AreEqual(mDKT[indDKT, 5], mTLG[ind, 7], 1e-4, "k[" + ind + ",7] different");

                Assert.AreEqual(mDKT[indDKT, 6], mTLG[ind, 10], 1e-4, "k[" + ind + ",10] different");
                Assert.AreEqual(mDKT[indDKT, 7], mTLG[ind, 11], 1e-4, "k[" + ind + ",11] different");
                Assert.AreEqual(mDKT[indDKT, 8], mTLG[ind, 12], 1e-4, "k[" + ind + ",12] different");
            }
        }

        /// <summary>
        /// rettangolo, check DKT part
        /// </summary>
        [TestMethod]
        public void Test4()
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(0.0, 0.0, 0));
            nodes.Add(new Node(10.0, 0.0, 0));
            nodes.Add(new Node(10.0, 2.0, 0));
            nodes.Add(new Node(0.0, 2.0, 0));

            double hTot = 1.0; //Jtot = 1/12 * 1 * (h1^3 + h2^3) = 2/12 * 1 * (hi^3) 
            double jTot = 1.0 / 12.0 * Math.Pow(hTot, 3.0);
            double h1 = Math.Pow(6.0 * jTot, 1.0 / 3.0);
            double h2 = Math.Pow(6.0 * jTot, 1.0 / 3.0);
            double EGlass = 12.0;
            double niGlass = 0.0;
            double G0 = 0.0;
            double h0 = 0.1;

            PlateProperty p = new PlateProperty(new IsotropicFemMaterial(EGlass, niGlass, 0, 0), hTot, hTot, "");

            List<Tri3DK> els = new List<Tri3DK>();
            els.Add(new Tri3DK(new Node[] { nodes[0], nodes[1], nodes[3] }, p));
            els.Add(new Tri3DK(new Node[] { nodes[1], nodes[2], nodes[3] }, p));

            List<Tri3TripledLaminatedGlassV2> els2 = new List<Tri3TripledLaminatedGlassV2>();
            els2.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[0], nodes[1], nodes[3] }, G0, h0, h1, h2, EGlass, niGlass));
            els2.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[1], nodes[2], nodes[3] }, G0, h0, h1, h2, EGlass, niGlass));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeForceAttribute fNode = new NodeForceAttribute("loadCase", sys, 0, 0, 1.0, 0, 0, 0);

            nodes[1].AddAttribute(fNode);
            nodes[2].AddAttribute(fNode);

            NodeRestrainAttribute fix1 = new NodeRestrainAttribute("freedomCase", sys);
            fix1.AddExternalRestrain(Solver.DOF.DX);
            fix1.AddExternalRestrain(Solver.DOF.DY);
            fix1.AddExternalRestrain(Solver.DOF.DZ);
            fix1.AddExternalRestrain(Solver.DOF.RX);
            fix1.AddExternalRestrain(Solver.DOF.RY);
            fix1.AddExternalRestrain(Solver.DOF.RZ);

            nodes[0].AddAttribute(fix1);
            nodes[3].AddAttribute(fix1);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("freedomCase", sys);
            fix2.AddExternalRestrain(Solver.DOF.DX);
            fix2.AddExternalRestrain(Solver.DOF.DY);
            fix2.AddExternalRestrain(Solver.DOF.RZ);
            nodes.ForEach(x => x.AddAttribute(fix2));

            LinearSolver fem1 = new LinearSolver(els.ToArray());

            fix1.AddExternalRestrain(Solver.DOF.DDZ);

            fix2.AddExternalRestrain(Solver.DOF.DDZ);

            LinearSolver fem2 = new LinearSolver(els2.ToArray());

            Assert.AreEqual(fem1.GetNodeDisplacementGlobalCoordinates(nodes[1], Solver.DOF.DZ), fem2.GetNodeDisplacementGlobalCoordinates(nodes[1], Solver.DOF.DZ), 1e-4);
        }

        /// <summary>
        /// Test slippage Kg
        /// </summary>
        [TestMethod]
        public void TestSlippageKg()
        {
            double hGlass1 = 0.5; //0.7937;
            double hGlass2 = 0.5; // 0.7937;
            double EGlass = 1000.0;
            double niGlass = 0.0;
            double hInterlayer = 0.001;
            double G0 = 0.0 * EGlass / (2.0 * (1.0 + niGlass));
            double hTot = hGlass1 + hGlass2;

            PlateProperty p = new PlateProperty(new IsotropicFemMaterial(EGlass, niGlass, 0, 0), hTot, hTot, "");

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(0, 0, 0));
            nodes.Add(new Node(1, 0, 0));
            nodes.Add(new Node(1, 1, 0));
            nodes.Add(new Node(0, 1, 0));
            nodes.Add(new Node(2, 0, 0));
            nodes.Add(new Node(2, 1, 0));
            #endregion

            #region plates
            List<Tri3TripledLaminatedGlassV2> els = new List<Tri3TripledLaminatedGlassV2>();
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[0], nodes[1], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[1], nodes[2], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[1], nodes[4], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[4], nodes[5], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            #endregion

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.RZ);
            fix.AddExternalRestrain(Solver.DOF.DDZ);

            NodeRestrainAttribute ddPlus = new NodeRestrainAttribute("freedomCase", sys);
            ddPlus.AddImposedDisplacement(Solver.DOF.DDY, 1.0);

            /*NodeRestrainAttribute ddMin = new NodeRestrainAttribute("freedomCase", sys);
            ddMin.AddImposedDisplacement(Solver.DOF.DDX, -1.0);*/

            NodeRestrainAttribute ddFix = new NodeRestrainAttribute("freedomCase", sys);
            ddFix.AddExternalRestrain(Solver.DOF.DDX);
            ddFix.AddExternalRestrain(Solver.DOF.DDY);

            nodes[0].AddAttribute(ddPlus);
            nodes[3].AddAttribute(ddPlus);

            nodes[4].AddAttribute(ddFix);
            nodes[5].AddAttribute(ddFix);

            nodes.ForEach(x => x.AddAttribute(fix));

            var fem = new LinearSolver(els.ToArray());

            /*FEMUtilities.WriteMatrix(fem.KGlobalRestrains);
            FEMUtilities.WriteMatrix(fem.FRestrains);*/

            List<Node> nodes2 = new List<Node>();
            #region nodes2
            nodes2.Add(new Node(0, 0, 0));
            nodes2.Add(new Node(1, 0, 0));
            nodes2.Add(new Node(1, 1, 0));
            nodes2.Add(new Node(0, 1, 0));
            nodes2.Add(new Node(2, 0, 0));
            nodes2.Add(new Node(2, 1, 0));
            #endregion

            List<Tri3PlaneStress> els2 = new List<Tri3PlaneStress>();
            els2.Add(new Tri3PlaneStress(new Node[] { nodes2[0], nodes2[1], nodes2[3] }, p));
            els2.Add(new Tri3PlaneStress(new Node[] { nodes2[1], nodes2[2], nodes2[3] }, p));
            els2.Add(new Tri3PlaneStress(new Node[] { nodes2[1], nodes2[4], nodes2[2] }, p));
            els2.Add(new Tri3PlaneStress(new Node[] { nodes2[4], nodes2[5], nodes2[2] }, p));

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("freedomCase", sys);
            fix2.AddExternalRestrain(Solver.DOF.DZ);

            nodes2.ForEach(x => x.AddAttribute(fix2));

            NodeRestrainAttribute d2 = new NodeRestrainAttribute("freedomCase", sys);
            d2.AddExternalRestrain(Solver.DOF.DX);
            d2.AddExternalRestrain(Solver.DOF.DY);

            NodeRestrainAttribute dPlus = new NodeRestrainAttribute("freedomCase", sys);
            dPlus.AddImposedDisplacement(Solver.DOF.DY, 1.0);

            nodes2[0].AddAttribute(dPlus);
            nodes2[3].AddAttribute(dPlus);

            nodes2[4].AddAttribute(d2);
            nodes2[5].AddAttribute(d2);

            var fem2 = new LinearSolver(els2.ToArray());

            Assert.AreEqual(fem.GetNodeDisplacementGlobalCoordinates(nodes[0], Solver.DOF.DDY), fem2.GetNodeDisplacementGlobalCoordinates(nodes2[0], Solver.DOF.DY), 1e-5);
            Assert.AreEqual(fem.GetNodeDisplacementGlobalCoordinates(nodes[0], Solver.DOF.DDX), fem2.GetNodeDisplacementGlobalCoordinates(nodes2[0], Solver.DOF.DX), 1e-5);

            Assert.AreEqual(fem.GetNodeDisplacementGlobalCoordinates(nodes[1], Solver.DOF.DDY), fem2.GetNodeDisplacementGlobalCoordinates(nodes2[1], Solver.DOF.DY), 1e-5);
            Assert.AreEqual(fem.GetNodeDisplacementGlobalCoordinates(nodes[1], Solver.DOF.DDX), fem2.GetNodeDisplacementGlobalCoordinates(nodes2[1], Solver.DOF.DX), 1e-5);

            Assert.AreEqual(fem.GetNodeDisplacementGlobalCoordinates(nodes[2], Solver.DOF.DDY), fem2.GetNodeDisplacementGlobalCoordinates(nodes2[2], Solver.DOF.DY), 1e-5);
            Assert.AreEqual(fem.GetNodeDisplacementGlobalCoordinates(nodes[2], Solver.DOF.DDX), fem2.GetNodeDisplacementGlobalCoordinates(nodes2[2], Solver.DOF.DX), 1e-5);

            Assert.AreEqual(fem.GetNodeDisplacementGlobalCoordinates(nodes[3], Solver.DOF.DDY), fem2.GetNodeDisplacementGlobalCoordinates(nodes2[3], Solver.DOF.DY), 1e-5);
            Assert.AreEqual(fem.GetNodeDisplacementGlobalCoordinates(nodes[3], Solver.DOF.DDX), fem2.GetNodeDisplacementGlobalCoordinates(nodes2[3], Solver.DOF.DX), 1e-5);
        }

        /// <summary>
        /// Test Bs
        /// </summary>
        [TestMethod]
        public void TestBs()
        {
            double hGlass1 = 0.001;
            double hGlass2 = hGlass1;
            double EGlass = 2.0;
            double niGlass = 0.0;
            double hInterlayer = 1;
            double G0 = EGlass / (2.0 * (1.0 + niGlass));
            double hTot = hGlass1 + hGlass2;

            PlateProperty p = new PlateProperty(new IsotropicFemMaterial(EGlass, niGlass, 0, 0), hTot, hTot, "");

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(0, 0, 0));
            nodes.Add(new Node(1, 0, 0));
            nodes.Add(new Node(0, 1, 0));
            #endregion

            #region plates
            List<Tri3TripledLaminatedGlassV2> els = new List<Tri3TripledLaminatedGlassV2>();
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[0], nodes[1], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            #endregion

            var Bs1 = els[0].GetBsi(1, 0, 0);
            var Bs2 = els[0].GetBsi(2, 1, 0);
            var Bs3 = els[0].GetBsi(3, 0, 1);

            for (int r = 0; r < Bs1.RowCount; r++)
            {
                for (int c = 0; c < Bs1.ColumnCount; c++)
                {
                    if ((r == 0 && c == 0) || (r == 1 && c == 1) || (r == 3 && c == 3))
                    {
                        Assert.AreEqual(1.0, Bs1[r, c]);
                        Assert.AreEqual(1.0, Bs2[r, c]);
                        Assert.AreEqual(1.0, Bs3[r, c]);
                    }
                    else if (r == 2 && c == 4)
                    {
                        Assert.AreEqual(-1.0, Bs1[r, c]);
                        Assert.AreEqual(-1.0, Bs2[r, c]);
                        Assert.AreEqual(-1.0, Bs3[r, c]);
                    }
                    else
                    {
                        Assert.AreEqual(0.0, Bs1[r, c]);
                        Assert.AreEqual(0.0, Bs2[r, c]);
                        Assert.AreEqual(0.0, Bs3[r, c]);
                    }
                }
            }
        }

        /// <summary>
        /// rettangolo 
        /// </summary>
        [TestMethod]
        public void Test5()
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(0.0, 0.0, 0));
            nodes.Add(new Node(2.0, 0.0, 0));
            nodes.Add(new Node(0.0, 2.0, 0));

            double hTot = 1.0; //Jtot = 1/12 * 1 * (h1^3 + h2^3) = 2/12 * 1 * (hi^3) 
            double jTot = 1.0 / 12.0 * Math.Pow(hTot, 3.0);

            double h1 = Math.Pow(6.0 * jTot, 1.0 / 3.0);
            double h2 = Math.Pow(6.0 * jTot, 1.0 / 3.0);
            double EGlass = 50.0;
            double niGlass = 0.0;
            double G0 = 0.0 * EGlass / (2.0 * (1.0 + niGlass));
            double h0 = 0.01;

            PlateProperty p = new PlateProperty(new IsotropicFemMaterial(EGlass, niGlass, 0, 0), hTot, hTot, "");

            var el = new Tri3TripledLaminatedGlassV2(new Node[] { nodes[0], nodes[1], nodes[2] }, G0, h0, h1, h2, EGlass, niGlass);
            var el2 = new Tri3DK(new Node[] { nodes[0], nodes[1], nodes[2] }, p);

            el.BuildMatrix();
            el2.BuildMatrix();

            FEMUtilities.WriteMatrix("Kg = ", el.KGlass);
            FEMUtilities.WriteMatrix("Ks = ", el.KLayer);
            FEMUtilities.WriteMatrix("KLocalUnordered = ", el.KLocalUnordered);

            FEMUtilities.WriteMatrix("KDKT = ", el2.KElementLocalCoord);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0, 1, 0, 0, 0);

            nodes[0].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);

            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            fix.AddExternalRestrain(Solver.DOF.DDX);
            fix.AddExternalRestrain(Solver.DOF.DDY);
            fix.AddExternalRestrain(Solver.DOF.DDZ);

            nodes[2].AddAttribute(fix);

            NodeRestrainAttribute dz = new NodeRestrainAttribute("fc", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);

            nodes[1].AddAttribute(fix); //dz

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
            fix2.AddExternalRestrain(Solver.DOF.DX);
            fix2.AddExternalRestrain(Solver.DOF.DY);
            fix2.AddExternalRestrain(Solver.DOF.DDZ);
            fix2.AddExternalRestrain(Solver.DOF.RZ);

            nodes.ForEach(x => x.AddAttribute(fix2));

            var fem0 = new LinearSolver(new FiniteElement[] { el });

            FEMUtilities.WriteMatrix("KGlob = ", fem0.KGlobal, "F3");
            FEMUtilities.WriteMatrix("KGlobRestr = ", fem0.KGlobalRestrains, "F3");
        }

        /// <summary>
        /// Similitudine con Kirchoff - piastra 10x10 appoggiata su 4 lati con carico uniforme
        /// </summary>
        [TestMethod]
        public void KirchoffTest1()
        {
            double hGlass1 = 0.5;
            double hGlass2 = 0.5;
            double EGlass = 1000.0;
            double niGlass = 0.0;

            double G0 = EGlass / (2.0 * (1.0 + niGlass));

            double hInterlayer = 0.01;

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(-1e6, -1e6, -1e6));
            nodes.Add(new Node(0, 0, 0));
            nodes.Add(new Node(0, 1, 0));
            nodes.Add(new Node(0, 2, 0));
            nodes.Add(new Node(0, 3, 0));
            nodes.Add(new Node(0, 4, 0));
            nodes.Add(new Node(0, 5, 0));
            nodes.Add(new Node(0, 6, 0));
            nodes.Add(new Node(0, 7, 0));
            nodes.Add(new Node(0, 8, 0));
            nodes.Add(new Node(0, 9, 0));
            nodes.Add(new Node(0, 10, 0));
            nodes.Add(new Node(1, 0, 0));
            nodes.Add(new Node(1, 1, 0));
            nodes.Add(new Node(1, 2, 0));
            nodes.Add(new Node(1, 3, 0));
            nodes.Add(new Node(1, 4, 0));
            nodes.Add(new Node(1, 5, 0));
            nodes.Add(new Node(1, 6, 0));
            nodes.Add(new Node(1, 7, 0));
            nodes.Add(new Node(1, 8, 0));
            nodes.Add(new Node(1, 9, 0));
            nodes.Add(new Node(1, 10, 0));
            nodes.Add(new Node(2, 0, 0));
            nodes.Add(new Node(2, 1, 0));
            nodes.Add(new Node(2, 2, 0));
            nodes.Add(new Node(2, 3, 0));
            nodes.Add(new Node(2, 4, 0));
            nodes.Add(new Node(2, 5, 0));
            nodes.Add(new Node(2, 6, 0));
            nodes.Add(new Node(2, 7, 0));
            nodes.Add(new Node(2, 8, 0));
            nodes.Add(new Node(2, 9, 0));
            nodes.Add(new Node(2, 10, 0));
            nodes.Add(new Node(3, 0, 0));
            nodes.Add(new Node(3, 1, 0));
            nodes.Add(new Node(3, 2, 0));
            nodes.Add(new Node(3, 3, 0));
            nodes.Add(new Node(3, 4, 0));
            nodes.Add(new Node(3, 5, 0));
            nodes.Add(new Node(3, 6, 0));
            nodes.Add(new Node(3, 7, 0));
            nodes.Add(new Node(3, 8, 0));
            nodes.Add(new Node(3, 9, 0));
            nodes.Add(new Node(3, 10, 0));
            nodes.Add(new Node(4, 0, 0));
            nodes.Add(new Node(4, 1, 0));
            nodes.Add(new Node(4, 2, 0));
            nodes.Add(new Node(4, 3, 0));
            nodes.Add(new Node(4, 4, 0));
            nodes.Add(new Node(4, 5, 0));
            nodes.Add(new Node(4, 6, 0));
            nodes.Add(new Node(4, 7, 0));
            nodes.Add(new Node(4, 8, 0));
            nodes.Add(new Node(4, 9, 0));
            nodes.Add(new Node(4, 10, 0));
            nodes.Add(new Node(5, 0, 0));
            nodes.Add(new Node(5, 1, 0));
            nodes.Add(new Node(5, 2, 0));
            nodes.Add(new Node(5, 3, 0));
            nodes.Add(new Node(5, 4, 0));
            nodes.Add(new Node(5, 5, 0));
            nodes.Add(new Node(5, 6, 0));
            nodes.Add(new Node(5, 7, 0));
            nodes.Add(new Node(5, 8, 0));
            nodes.Add(new Node(5, 9, 0));
            nodes.Add(new Node(5, 10, 0));
            nodes.Add(new Node(6, 0, 0));
            nodes.Add(new Node(6, 1, 0));
            nodes.Add(new Node(6, 2, 0));
            nodes.Add(new Node(6, 3, 0));
            nodes.Add(new Node(6, 4, 0));
            nodes.Add(new Node(6, 5, 0));
            nodes.Add(new Node(6, 6, 0));
            nodes.Add(new Node(6, 7, 0));
            nodes.Add(new Node(6, 8, 0));
            nodes.Add(new Node(6, 9, 0));
            nodes.Add(new Node(6, 10, 0));
            nodes.Add(new Node(7, 0, 0));
            nodes.Add(new Node(7, 1, 0));
            nodes.Add(new Node(7, 2, 0));
            nodes.Add(new Node(7, 3, 0));
            nodes.Add(new Node(7, 4, 0));
            nodes.Add(new Node(7, 5, 0));
            nodes.Add(new Node(7, 6, 0));
            nodes.Add(new Node(7, 7, 0));
            nodes.Add(new Node(7, 8, 0));
            nodes.Add(new Node(7, 9, 0));
            nodes.Add(new Node(7, 10, 0));
            nodes.Add(new Node(8, 0, 0));
            nodes.Add(new Node(8, 1, 0));
            nodes.Add(new Node(8, 2, 0));
            nodes.Add(new Node(8, 3, 0));
            nodes.Add(new Node(8, 4, 0));
            nodes.Add(new Node(8, 5, 0));
            nodes.Add(new Node(8, 6, 0));
            nodes.Add(new Node(8, 7, 0));
            nodes.Add(new Node(8, 8, 0));
            nodes.Add(new Node(8, 9, 0));
            nodes.Add(new Node(8, 10, 0));
            nodes.Add(new Node(9, 0, 0));
            nodes.Add(new Node(9, 1, 0));
            nodes.Add(new Node(9, 2, 0));
            nodes.Add(new Node(9, 3, 0));
            nodes.Add(new Node(9, 4, 0));
            nodes.Add(new Node(9, 5, 0));
            nodes.Add(new Node(9, 6, 0));
            nodes.Add(new Node(9, 7, 0));
            nodes.Add(new Node(9, 8, 0));
            nodes.Add(new Node(9, 9, 0));
            nodes.Add(new Node(9, 10, 0));
            nodes.Add(new Node(10, 0, 0));
            nodes.Add(new Node(10, 1, 0));
            nodes.Add(new Node(10, 2, 0));
            nodes.Add(new Node(10, 3, 0));
            nodes.Add(new Node(10, 4, 0));
            nodes.Add(new Node(10, 5, 0));
            nodes.Add(new Node(10, 6, 0));
            nodes.Add(new Node(10, 7, 0));
            nodes.Add(new Node(10, 8, 0));
            nodes.Add(new Node(10, 9, 0));
            nodes.Add(new Node(10, 10, 0));
            #endregion

            #region els
            List<Tri3TripledLaminatedGlassV2> els = new List<Tri3TripledLaminatedGlassV2>();
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[12], nodes[2], nodes[1] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[3], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[4], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[15], nodes[5], nodes[4] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[6], nodes[5] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[7], nodes[6] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[8], nodes[7] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[9], nodes[8] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[10], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[11], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[13], nodes[12] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[14], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[15], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[16], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[17], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[18], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[29], nodes[19], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[20], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[21], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[22], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[24], nodes[23] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[25], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[36], nodes[26], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[27], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[28], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[39], nodes[29], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[30], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[31], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[32], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[43], nodes[33], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[35], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[36], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[37], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[38], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[39], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[40], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[41], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[42], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[43], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[44], nodes[43] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[46], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[47], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[48], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[49], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[50], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[51], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[52], nodes[51] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[53], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[64], nodes[54], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[55], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[57], nodes[56] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[58], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[59], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[60], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[71], nodes[61], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[72], nodes[62], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[63], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[64], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[65], nodes[64] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[66], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[78], nodes[68], nodes[67] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[69], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[80], nodes[70], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[81], nodes[71], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[72], nodes[71] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[83], nodes[73], nodes[72] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[74], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[85], nodes[75], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[86], nodes[76], nodes[75] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[87], nodes[77], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[89], nodes[79], nodes[78] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[90], nodes[80], nodes[79] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[91], nodes[81], nodes[80] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[92], nodes[82], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[93], nodes[83], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[94], nodes[84], nodes[83] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[95], nodes[85], nodes[84] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[96], nodes[86], nodes[85] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[97], nodes[87], nodes[86] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[98], nodes[88], nodes[87] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[100], nodes[90], nodes[89] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[101], nodes[91], nodes[90] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[102], nodes[92], nodes[91] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[103], nodes[93], nodes[92] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[104], nodes[94], nodes[93] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[105], nodes[95], nodes[94] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[106], nodes[96], nodes[95] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[107], nodes[97], nodes[96] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[108], nodes[98], nodes[97] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[109], nodes[99], nodes[98] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[111], nodes[101], nodes[100] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[112], nodes[102], nodes[101] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[113], nodes[103], nodes[102] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[114], nodes[104], nodes[103] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[115], nodes[105], nodes[104] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[116], nodes[106], nodes[105] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[117], nodes[107], nodes[106] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[118], nodes[108], nodes[107] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[119], nodes[109], nodes[108] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[120], nodes[110], nodes[109] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[12], nodes[13], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[14], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[15], nodes[4] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[15], nodes[16], nodes[5] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[17], nodes[6] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[18], nodes[7] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[19], nodes[8] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[20], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[21], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[22], nodes[11] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[24], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[25], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[26], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[27], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[28], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[29], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[29], nodes[30], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[31], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[32], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[33], nodes[22] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[35], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[36], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[36], nodes[37], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[38], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[39], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[39], nodes[40], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[41], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[42], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[43], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[43], nodes[44], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[46], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[47], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[48], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[49], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[50], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[51], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[52], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[53], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[54], nodes[43] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[55], nodes[44] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[57], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[58], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[59], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[60], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[61], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[62], nodes[51] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[63], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[64], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[64], nodes[65], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[66], nodes[55] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[68], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[69], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[70], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[71], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[71], nodes[72], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[72], nodes[73], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[74], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[75], nodes[64] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[76], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[77], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[78], nodes[79], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[80], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[80], nodes[81], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[81], nodes[82], nodes[71] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[83], nodes[72] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[83], nodes[84], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[85], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[85], nodes[86], nodes[75] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[86], nodes[87], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[87], nodes[88], nodes[77] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[89], nodes[90], nodes[79] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[90], nodes[91], nodes[80] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[91], nodes[92], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[92], nodes[93], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[93], nodes[94], nodes[83] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[94], nodes[95], nodes[84] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[95], nodes[96], nodes[85] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[96], nodes[97], nodes[86] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[97], nodes[98], nodes[87] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[98], nodes[99], nodes[88] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[100], nodes[101], nodes[90] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[101], nodes[102], nodes[91] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[102], nodes[103], nodes[92] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[103], nodes[104], nodes[93] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[104], nodes[105], nodes[94] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[105], nodes[106], nodes[95] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[106], nodes[107], nodes[96] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[107], nodes[108], nodes[97] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[108], nodes[109], nodes[98] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[109], nodes[110], nodes[99] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[111], nodes[112], nodes[101] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[112], nodes[113], nodes[102] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[113], nodes[114], nodes[103] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[114], nodes[115], nodes[104] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[115], nodes[116], nodes[105] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[116], nodes[117], nodes[106] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[117], nodes[118], nodes[107] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[118], nodes[119], nodes[108] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[119], nodes[120], nodes[109] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[120], nodes[121], nodes[110] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            #endregion

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            PlatePressureAttribute p = new PlatePressureAttribute("lc", sys, 0, 0, -0.1);
            els.ForEach(pl => pl.AddLoadCaseAttribute(p));

            NodeRestrainAttribute dz = new NodeRestrainAttribute("freedomCase", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.RZ);
            fix.AddExternalRestrain(Solver.DOF.DDZ);

            nodes.ForEach(n => n.AddAttribute(fix));

            nodes.Where(nd => nd.Position.X == 0 && nd.Position.Y == 0).ToList().ForEach(nd => nd.AddAttribute(dz));
            nodes.Where(nd => nd.Position.X == 10 && nd.Position.Y == 0).ToList().ForEach(nd => nd.AddAttribute(dz));
            nodes.Where(nd => nd.Position.X == 10 && nd.Position.Y == 10).ToList().ForEach(nd => nd.AddAttribute(dz));
            nodes.Where(nd => nd.Position.X == 0 && nd.Position.Y == 10).ToList().ForEach(nd => nd.AddAttribute(dz));

            LinearSolver fem0 = new LinearSolver(els.ToArray());

            Node centrale = nodes.Where(x => x.Position.X == 5 && x.Position.Y == 5).First();
            Node nodoAngolo = nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).First();

            Console.WriteLine(fem0.GetNodeDisplacementGlobalCoordinates(centrale, Solver.DOF.DZ));
            Assert.AreEqual(1.0, -0.3251 / fem0.GetNodeDisplacementGlobalCoordinates(centrale, Solver.DOF.DZ), 0.02); //Come soluzione kirchoff

            double hc = (2.0 * hInterlayer + hGlass1 + hGlass2) / 2.0;

            Assert.AreEqual(1.0, 3.7426 / 180.0 * Math.PI / fem0.GetNodeDisplacementGlobalCoordinates(nodoAngolo, Solver.DOF.RY), 0.02); //come soluzione kirchoff
            Assert.AreEqual(0.034, fem0.GetNodeDisplacementGlobalCoordinates(nodoAngolo, Solver.DOF.RY) * hc, 0.001); //come soluzione kirchoff
            Assert.AreEqual(0.034, fem0.GetNodeDisplacementGlobalCoordinates(nodoAngolo, Solver.DOF.DDX), 0.001); //come soluzione kirchoff //TODO: attenzione segno

            var element = els[144]; //elemento centrale
            foreach (Node n in element.Nodes)
            {
                Console.WriteLine(n.Position);
            }

            Console.WriteLine(els[144].LocalCoordinateSystem.V1);
            Console.WriteLine(els[144].LocalCoordinateSystem.V2);
            Console.WriteLine(els[144].LocalCoordinateSystem.V3);

            var globalDispl = fem0.GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            double csi = 1;
            double eta = 0;

            var curvatures = element.GetGlassCurvatures(csi, eta, globalDispl);

            Assert.AreEqual(-0.0123, curvatures[0, 0], 1e-3);
            Assert.AreEqual(-0.0123, curvatures[1, 1], 1e-3);
            Assert.AreEqual(0.0, curvatures[0, 1], 1e-3);

            #region glass top
            var epsilonTopGlassTopFace = element.GetGlassStrains(Tri3TripledLaminatedGlassV2.Glass.Top, Plate.Face.Top, csi, eta, globalDispl);
            Assert.AreEqual(-0.0062, epsilonTopGlassTopFace[0, 0], 1e-3);
            Assert.AreEqual(-0.0062, epsilonTopGlassTopFace[1, 1], 1e-3);
            Assert.AreEqual(0.0, epsilonTopGlassTopFace[0, 1], 1e-3);

            var epsilonTopGlassMiddleFace = element.GetGlassStrains(Tri3TripledLaminatedGlassV2.Glass.Top, Plate.Face.Middle, csi, eta, globalDispl);
            Assert.AreEqual(-0.0062 / 2, epsilonTopGlassMiddleFace[0, 0], 1e-3);
            Assert.AreEqual(-0.0062 / 2, epsilonTopGlassMiddleFace[1, 1], 1e-3);
            Assert.AreEqual(0.0, epsilonTopGlassMiddleFace[0, 1], 1e-3);

            var epsilonTopGlassBottomFace = element.GetGlassStrains(Tri3TripledLaminatedGlassV2.Glass.Top, Plate.Face.Bottom, csi, eta, globalDispl);
            Assert.AreEqual(0.0, epsilonTopGlassBottomFace[0, 0], 1e-3);
            Assert.AreEqual(0.0, epsilonTopGlassBottomFace[1, 1], 1e-3);
            Assert.AreEqual(0.0, epsilonTopGlassBottomFace[0, 1], 1e-3);

            var stressTopGlassTopFace = element.GetGlassStress(Tri3TripledLaminatedGlassV2.Glass.Top, Plate.Face.Top, csi, eta, globalDispl);
            Assert.AreEqual(1.0, -6.193 / stressTopGlassTopFace[0, 0], 0.001);
            Assert.AreEqual(1.0, -6.193 / stressTopGlassTopFace[1, 1], 0.001);
            Assert.AreEqual(0.0, stressTopGlassTopFace[0, 1], 0.2);
            #endregion

            #region glass bottom
            var epsilonBottomGlassTopFace = element.GetGlassStrains(Tri3TripledLaminatedGlassV2.Glass.Bottom, Plate.Face.Top, csi, eta, globalDispl);
            Assert.AreEqual(0.0, epsilonBottomGlassTopFace[0, 0], 1e-3);
            Assert.AreEqual(0.0, epsilonBottomGlassTopFace[1, 1], 1e-3);
            Assert.AreEqual(0.0, epsilonBottomGlassTopFace[0, 1], 1e-3);

            var epsilonBottomGlassMiddleFace = element.GetGlassStrains(Tri3TripledLaminatedGlassV2.Glass.Bottom, Plate.Face.Middle, csi, eta, globalDispl);
            Assert.AreEqual(0.0062 / 2.0, epsilonBottomGlassMiddleFace[0, 0], 1e-3);
            Assert.AreEqual(0.0062 / 2.0, epsilonBottomGlassMiddleFace[1, 1], 1e-3);
            Assert.AreEqual(0.0, epsilonBottomGlassMiddleFace[0, 1], 1e-3);

            var epsilonBottomGlassBottomFace = element.GetGlassStrains(Tri3TripledLaminatedGlassV2.Glass.Bottom, Plate.Face.Bottom, csi, eta, globalDispl);
            Assert.AreEqual(0.0062, epsilonBottomGlassBottomFace[0, 0], 1e-3);
            Assert.AreEqual(0.0062, epsilonBottomGlassBottomFace[1, 1], 1e-3);
            Assert.AreEqual(0.0, epsilonBottomGlassBottomFace[0, 1], 1e-3);

            var stressBottomGlassBottomFace = element.GetGlassStress(Tri3TripledLaminatedGlassV2.Glass.Bottom, Plate.Face.Bottom, csi, eta, globalDispl);
            Assert.AreEqual(1.0, 6.193 / stressBottomGlassBottomFace[0, 0], 0.001);
            Assert.AreEqual(1.0, 6.193 / stressBottomGlassBottomFace[1, 1], 0.001);
            Assert.AreEqual(0.0, stressBottomGlassBottomFace[0, 1], 0.2);
            #endregion
        }

        /// <summary>
        /// Similitudine con Kirchoff - piastra 100x100 appoggiata su 4 lati con carico concentrato in mezzeria
        /// </summary>
        [TestMethod]
        public void KirchoffTest2()
        {
            double hGlass1 = 0.5;
            double hGlass2 = 0.5;
            double EGlass = 1000.0;
            double niGlass = 0;

            double G0 = EGlass / (2.0 * (1.0 + niGlass));

            double hInterlayer = 0.01;

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(-1e6, -1e6, -1e6));
            nodes.Add(new Node(0, 0, 0));
            nodes.Add(new Node(0, 10, 0));
            nodes.Add(new Node(0, 20, 0));
            nodes.Add(new Node(0, 30, 0));
            nodes.Add(new Node(0, 40, 0));
            nodes.Add(new Node(0, 50, 0));
            nodes.Add(new Node(0, 60, 0));
            nodes.Add(new Node(0, 70, 0));
            nodes.Add(new Node(0, 80, 0));
            nodes.Add(new Node(0, 90, 0));
            nodes.Add(new Node(0, 100, 0));
            nodes.Add(new Node(10, 0, 0));
            nodes.Add(new Node(10, 10, 0));
            nodes.Add(new Node(10, 20, 0));
            nodes.Add(new Node(10, 30, 0));
            nodes.Add(new Node(10, 40, 0));
            nodes.Add(new Node(10, 50, 0));
            nodes.Add(new Node(10, 60, 0));
            nodes.Add(new Node(10, 70, 0));
            nodes.Add(new Node(10, 80, 0));
            nodes.Add(new Node(10, 90, 0));
            nodes.Add(new Node(10, 100, 0));
            nodes.Add(new Node(20, 0, 0));
            nodes.Add(new Node(20, 10, 0));
            nodes.Add(new Node(20, 20, 0));
            nodes.Add(new Node(20, 30, 0));
            nodes.Add(new Node(20, 40, 0));
            nodes.Add(new Node(20, 50, 0));
            nodes.Add(new Node(20, 60, 0));
            nodes.Add(new Node(20, 70, 0));
            nodes.Add(new Node(20, 80, 0));
            nodes.Add(new Node(20, 90, 0));
            nodes.Add(new Node(20, 100, 0));
            nodes.Add(new Node(30, 0, 0));
            nodes.Add(new Node(30, 10, 0));
            nodes.Add(new Node(30, 20, 0));
            nodes.Add(new Node(30, 30, 0));
            nodes.Add(new Node(30, 40, 0));
            nodes.Add(new Node(30, 50, 0));
            nodes.Add(new Node(30, 60, 0));
            nodes.Add(new Node(30, 70, 0));
            nodes.Add(new Node(30, 80, 0));
            nodes.Add(new Node(30, 90, 0));
            nodes.Add(new Node(30, 100, 0));
            nodes.Add(new Node(40, 0, 0));
            nodes.Add(new Node(40, 10, 0));
            nodes.Add(new Node(40, 20, 0));
            nodes.Add(new Node(40, 30, 0));
            nodes.Add(new Node(40, 40, 0));
            nodes.Add(new Node(40, 50, 0));
            nodes.Add(new Node(40, 60, 0));
            nodes.Add(new Node(40, 70, 0));
            nodes.Add(new Node(40, 80, 0));
            nodes.Add(new Node(40, 90, 0));
            nodes.Add(new Node(40, 100, 0));
            nodes.Add(new Node(50, 0, 0));
            nodes.Add(new Node(50, 10, 0));
            nodes.Add(new Node(50, 20, 0));
            nodes.Add(new Node(50, 30, 0));
            nodes.Add(new Node(50, 40, 0));
            nodes.Add(new Node(50, 50, 0));
            nodes.Add(new Node(50, 60, 0));
            nodes.Add(new Node(50, 70, 0));
            nodes.Add(new Node(50, 80, 0));
            nodes.Add(new Node(50, 90, 0));
            nodes.Add(new Node(50, 100, 0));
            nodes.Add(new Node(60, 0, 0));
            nodes.Add(new Node(60, 10, 0));
            nodes.Add(new Node(60, 20, 0));
            nodes.Add(new Node(60, 30, 0));
            nodes.Add(new Node(60, 40, 0));
            nodes.Add(new Node(60, 50, 0));
            nodes.Add(new Node(60, 60, 0));
            nodes.Add(new Node(60, 70, 0));
            nodes.Add(new Node(60, 80, 0));
            nodes.Add(new Node(60, 90, 0));
            nodes.Add(new Node(60, 100, 0));
            nodes.Add(new Node(70, 0, 0));
            nodes.Add(new Node(70, 10, 0));
            nodes.Add(new Node(70, 20, 0));
            nodes.Add(new Node(70, 30, 0));
            nodes.Add(new Node(70, 40, 0));
            nodes.Add(new Node(70, 50, 0));
            nodes.Add(new Node(70, 60, 0));
            nodes.Add(new Node(70, 70, 0));
            nodes.Add(new Node(70, 80, 0));
            nodes.Add(new Node(70, 90, 0));
            nodes.Add(new Node(70, 100, 0));
            nodes.Add(new Node(80, 0, 0));
            nodes.Add(new Node(80, 10, 0));
            nodes.Add(new Node(80, 20, 0));
            nodes.Add(new Node(80, 30, 0));
            nodes.Add(new Node(80, 40, 0));
            nodes.Add(new Node(80, 50, 0));
            nodes.Add(new Node(80, 60, 0));
            nodes.Add(new Node(80, 70, 0));
            nodes.Add(new Node(80, 80, 0));
            nodes.Add(new Node(80, 90, 0));
            nodes.Add(new Node(80, 100, 0));
            nodes.Add(new Node(90, 0, 0));
            nodes.Add(new Node(90, 10, 0));
            nodes.Add(new Node(90, 20, 0));
            nodes.Add(new Node(90, 30, 0));
            nodes.Add(new Node(90, 40, 0));
            nodes.Add(new Node(90, 50, 0));
            nodes.Add(new Node(90, 60, 0));
            nodes.Add(new Node(90, 70, 0));
            nodes.Add(new Node(90, 80, 0));
            nodes.Add(new Node(90, 90, 0));
            nodes.Add(new Node(90, 100, 0));
            nodes.Add(new Node(100, 0, 0));
            nodes.Add(new Node(100, 10, 0));
            nodes.Add(new Node(100, 20, 0));
            nodes.Add(new Node(100, 30, 0));
            nodes.Add(new Node(100, 40, 0));
            nodes.Add(new Node(100, 50, 0));
            nodes.Add(new Node(100, 60, 0));
            nodes.Add(new Node(100, 70, 0));
            nodes.Add(new Node(100, 80, 0));
            nodes.Add(new Node(100, 90, 0));
            nodes.Add(new Node(100, 100, 0));
            #endregion

            List<Tri3TripledLaminatedGlassV2> els = new List<Tri3TripledLaminatedGlassV2>();
            #region els
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[12], nodes[2], nodes[1] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[3], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[4], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[15], nodes[5], nodes[4] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[6], nodes[5] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[7], nodes[6] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[8], nodes[7] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[9], nodes[8] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[10], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[11], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[13], nodes[12] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[14], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[15], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[16], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[17], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[18], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[29], nodes[19], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[20], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[21], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[22], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[24], nodes[23] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[25], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[36], nodes[26], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[27], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[28], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[39], nodes[29], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[30], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[31], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[32], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[43], nodes[33], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[35], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[36], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[37], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[38], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[39], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[40], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[41], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[42], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[43], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[44], nodes[43] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[46], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[47], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[48], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[49], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[50], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[51], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[52], nodes[51] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[53], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[64], nodes[54], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[55], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[57], nodes[56] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[58], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[59], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[60], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[71], nodes[61], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[72], nodes[62], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[63], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[64], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[65], nodes[64] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[66], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[78], nodes[68], nodes[67] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[69], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[80], nodes[70], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[81], nodes[71], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[72], nodes[71] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[83], nodes[73], nodes[72] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[74], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[85], nodes[75], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[86], nodes[76], nodes[75] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[87], nodes[77], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[89], nodes[79], nodes[78] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[90], nodes[80], nodes[79] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[91], nodes[81], nodes[80] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[92], nodes[82], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[93], nodes[83], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[94], nodes[84], nodes[83] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[95], nodes[85], nodes[84] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[96], nodes[86], nodes[85] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[97], nodes[87], nodes[86] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[98], nodes[88], nodes[87] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[100], nodes[90], nodes[89] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[101], nodes[91], nodes[90] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[102], nodes[92], nodes[91] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[103], nodes[93], nodes[92] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[104], nodes[94], nodes[93] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[105], nodes[95], nodes[94] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[106], nodes[96], nodes[95] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[107], nodes[97], nodes[96] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[108], nodes[98], nodes[97] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[109], nodes[99], nodes[98] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[111], nodes[101], nodes[100] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[112], nodes[102], nodes[101] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[113], nodes[103], nodes[102] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[114], nodes[104], nodes[103] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[115], nodes[105], nodes[104] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[116], nodes[106], nodes[105] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[117], nodes[107], nodes[106] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[118], nodes[108], nodes[107] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[119], nodes[109], nodes[108] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[120], nodes[110], nodes[109] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[12], nodes[13], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[14], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[15], nodes[4] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[15], nodes[16], nodes[5] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[17], nodes[6] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[18], nodes[7] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[19], nodes[8] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[20], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[21], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[22], nodes[11] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[24], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[25], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[26], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[27], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[28], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[29], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[29], nodes[30], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[31], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[32], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[33], nodes[22] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[35], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[36], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[36], nodes[37], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[38], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[39], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[39], nodes[40], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[41], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[42], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[43], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[43], nodes[44], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[46], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[47], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[48], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[49], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[50], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[51], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[52], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[53], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[54], nodes[43] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[55], nodes[44] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[57], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[58], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[59], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[60], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[61], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[62], nodes[51] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[63], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[64], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[64], nodes[65], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[66], nodes[55] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[68], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[69], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[70], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[71], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[71], nodes[72], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[72], nodes[73], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[74], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[75], nodes[64] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[76], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[77], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[78], nodes[79], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[80], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[80], nodes[81], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[81], nodes[82], nodes[71] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[83], nodes[72] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[83], nodes[84], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[85], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[85], nodes[86], nodes[75] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[86], nodes[87], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[87], nodes[88], nodes[77] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[89], nodes[90], nodes[79] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[90], nodes[91], nodes[80] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[91], nodes[92], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[92], nodes[93], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[93], nodes[94], nodes[83] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[94], nodes[95], nodes[84] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[95], nodes[96], nodes[85] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[96], nodes[97], nodes[86] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[97], nodes[98], nodes[87] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[98], nodes[99], nodes[88] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[100], nodes[101], nodes[90] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[101], nodes[102], nodes[91] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[102], nodes[103], nodes[92] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[103], nodes[104], nodes[93] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[104], nodes[105], nodes[94] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[105], nodes[106], nodes[95] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[106], nodes[107], nodes[96] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[107], nodes[108], nodes[97] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[108], nodes[109], nodes[98] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[109], nodes[110], nodes[99] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[111], nodes[112], nodes[101] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[112], nodes[113], nodes[102] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[113], nodes[114], nodes[103] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[114], nodes[115], nodes[104] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[115], nodes[116], nodes[105] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[116], nodes[117], nodes[106] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[117], nodes[118], nodes[107] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[118], nodes[119], nodes[108] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[119], nodes[120], nodes[109] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[120], nodes[121], nodes[110] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            #endregion

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0, 10, 0, 0, 0);
            nodes.Where(nd => nd.Position.X == 50 && nd.Position.Y == 50).First().AddAttribute(f);

            NodeRestrainAttribute dz = new NodeRestrainAttribute("freedomCase", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);

            fix.AddExternalRestrain(Solver.DOF.RZ);

            fix.AddExternalRestrain(Solver.DOF.DDZ);

            nodes.ForEach(x => x.AddAttribute(fix));

            nodes.Where(nd => nd.Position.X == 0 && nd.Position.Y == 0).ToList().ForEach(nd => nd.AddAttribute(dz));
            nodes.Where(nd => nd.Position.X == 100 && nd.Position.Y == 0).ToList().ForEach(nd => nd.AddAttribute(dz));
            nodes.Where(nd => nd.Position.X == 100 && nd.Position.Y == 100).ToList().ForEach(nd => nd.AddAttribute(dz));
            nodes.Where(nd => nd.Position.X == 0 && nd.Position.Y == 100).ToList().ForEach(nd => nd.AddAttribute(dz));

            LinearSolver fem0 = new LinearSolver(els.ToArray());

            var nodoCentrale = nodes.Where(x => x.Position.X == 50 && x.Position.Y == 50).First();

            Console.WriteLine(fem0.GetNodeDisplacementGlobalCoordinates(nodoCentrale, Solver.DOF.DZ));
            Assert.AreEqual(1.0, 51.3318 / fem0.GetNodeDisplacementGlobalCoordinates(nodoCentrale, Solver.DOF.DZ), 0.05); //Come soluzione kirchoff, Straus non usa stessi elementi, usato SAP

            var pt1 = nodes.Where(x => x.Position.X == 70 && x.Position.Y == 50).First();
            var pt2 = nodes.Where(x => x.Position.X == 80 && x.Position.Y == 50).First();
            var pt3 = nodes.Where(x => x.Position.X == 70 && x.Position.Y == 60).First();
            var element = els.Where(x => x.Nodes.Contains(pt1) && x.Nodes.Contains(pt2) && x.Nodes.Contains(pt3)).First(); //elemento
            foreach (Node n in element.Nodes)
            {
                Console.WriteLine(n.Position);
            }

            #region glass top
            var stressTopGlassTopFaceNode1 = fem0.GetTri3TLG2GlassStress(element, Tri3TripledLaminatedGlassV2.Glass.Top, Plate.Face.Top, 3, sys);
            Assert.AreEqual(1.0, 6.785 / stressTopGlassTopFaceNode1[0, 0], 0.05);
            Assert.AreEqual(1.0, 13.829 / stressTopGlassTopFaceNode1[1, 1], 0.07);
            //Assert.AreEqual(1.0, -0.26 / stressTopGlassTopFaceNode1[1,0], 0.01);
            #endregion
        }

        /// <summary>
        /// Similitudine con Kirchoff - trave appoggiata con forza concentrata in mezzeria
        /// </summary>
        [TestMethod]
        public void KirchoffBeamTest3()
        {
            double hGlass1 = 0.5;
            double hGlass2 = 0.5;
            double EGlass = 1000.0;
            double niGlass = 0.0;

            double EInterlayer = EGlass;
            double niInterlayer = niGlass;
            double G0 = EInterlayer / (2.0 * (1.0 + niInterlayer));

            double hInterlayer = 0.01;

            double F = 4.0;
            double L = 10.0;
            double b = 1.0;
            double h = hGlass1 + hGlass2 + hInterlayer;

            double V = F / 2.0;
            double M = F * L / 4.0;

            double J = 1.0 / 12.0 * b * h * h * h;
            double W = 1.0 / 6.0 * b * h * h;
            double spost = F * L * L * L / (48.0 * EGlass * J);

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(-1e6, -1e6, -1e6));
            nodes.Add(new Node(0.5, 4.16667, 0));
            nodes.Add(new Node(1, 4.16667, 0));
            nodes.Add(new Node(1, 4.5, 0));
            nodes.Add(new Node(0.5, 4.5, 0));
            nodes.Add(new Node(1.5, 4.16667, 0));
            nodes.Add(new Node(2, 4.16667, 0));
            nodes.Add(new Node(2, 4.5, 0));
            nodes.Add(new Node(1.5, 4.5, 0));
            nodes.Add(new Node(2.5, 4.16667, 0));
            nodes.Add(new Node(3, 4.16667, 0));
            nodes.Add(new Node(3, 4.5, 0));
            nodes.Add(new Node(2.5, 4.5, 0));
            nodes.Add(new Node(3.5, 4.16667, 0));
            nodes.Add(new Node(4, 4.16667, 0));
            nodes.Add(new Node(4, 4.5, 0));
            nodes.Add(new Node(3.5, 4.5, 0));
            nodes.Add(new Node(4.5, 4.16667, 0));
            nodes.Add(new Node(5, 4.16667, 0));
            nodes.Add(new Node(5, 4.5, 0));
            nodes.Add(new Node(4.5, 4.5, 0));
            nodes.Add(new Node(5.5, 4.16667, 0));
            nodes.Add(new Node(6, 4.16667, 0));
            nodes.Add(new Node(6, 4.5, 0));
            nodes.Add(new Node(5.5, 4.5, 0));
            nodes.Add(new Node(6.5, 4.16667, 0));
            nodes.Add(new Node(7, 4.16667, 0));
            nodes.Add(new Node(7, 4.5, 0));
            nodes.Add(new Node(6.5, 4.5, 0));
            nodes.Add(new Node(7.5, 4.16667, 0));
            nodes.Add(new Node(8, 4.16667, 0));
            nodes.Add(new Node(8, 4.5, 0));
            nodes.Add(new Node(7.5, 4.5, 0));
            nodes.Add(new Node(8.5, 4.16667, 0));
            nodes.Add(new Node(9, 4.16667, 0));
            nodes.Add(new Node(9, 4.5, 0));
            nodes.Add(new Node(8.5, 4.5, 0));
            nodes.Add(new Node(9.5, 4.16667, 0));
            nodes.Add(new Node(10, 4.16667, 0));
            nodes.Add(new Node(10, 4.5, 0));
            nodes.Add(new Node(9.5, 4.5, 0));
            nodes.Add(new Node(0, 4.16667, 0));
            nodes.Add(new Node(0, 4.5, 0));
            nodes.Add(new Node(0.5, 3.5, 0));
            nodes.Add(new Node(1, 3.5, 0));
            nodes.Add(new Node(1, 3.83333, 0));
            nodes.Add(new Node(0.5, 3.83333, 0));
            nodes.Add(new Node(1.5, 3.5, 0));
            nodes.Add(new Node(2, 3.5, 0));
            nodes.Add(new Node(2, 3.83333, 0));
            nodes.Add(new Node(1.5, 3.83333, 0));
            nodes.Add(new Node(2.5, 3.5, 0));
            nodes.Add(new Node(3, 3.5, 0));
            nodes.Add(new Node(3, 3.83333, 0));
            nodes.Add(new Node(2.5, 3.83333, 0));
            nodes.Add(new Node(3.5, 3.5, 0));
            nodes.Add(new Node(4, 3.5, 0));
            nodes.Add(new Node(4, 3.83333, 0));
            nodes.Add(new Node(3.5, 3.83333, 0));
            nodes.Add(new Node(4.5, 3.5, 0));
            nodes.Add(new Node(5, 3.5, 0));
            nodes.Add(new Node(5, 3.83333, 0));
            nodes.Add(new Node(4.5, 3.83333, 0));
            nodes.Add(new Node(5.5, 3.5, 0));
            nodes.Add(new Node(6, 3.5, 0));
            nodes.Add(new Node(6, 3.83333, 0));
            nodes.Add(new Node(5.5, 3.83333, 0));
            nodes.Add(new Node(6.5, 3.5, 0));
            nodes.Add(new Node(7, 3.5, 0));
            nodes.Add(new Node(7, 3.83333, 0));
            nodes.Add(new Node(6.5, 3.83333, 0));
            nodes.Add(new Node(7.5, 3.5, 0));
            nodes.Add(new Node(8, 3.5, 0));
            nodes.Add(new Node(8, 3.83333, 0));
            nodes.Add(new Node(7.5, 3.83333, 0));
            nodes.Add(new Node(8.5, 3.5, 0));
            nodes.Add(new Node(9, 3.5, 0));
            nodes.Add(new Node(9, 3.83333, 0));
            nodes.Add(new Node(8.5, 3.83333, 0));
            nodes.Add(new Node(9.5, 3.5, 0));
            nodes.Add(new Node(10, 3.5, 0));
            nodes.Add(new Node(10, 3.83333, 0));
            nodes.Add(new Node(9.5, 3.83333, 0));
            nodes.Add(new Node(0, 3.5, 0));
            nodes.Add(new Node(0, 3.83333, 0));
            #endregion

            List<Tri3TripledLaminatedGlassV2> els = new List<Tri3TripledLaminatedGlassV2>();
            #region els
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[4], nodes[2], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[8], nodes[6], nodes[7] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[12], nodes[10], nodes[11] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[14], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[18], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[22], nodes[23] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[26], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[30], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[36], nodes[34], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[38], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[1], nodes[4] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[3], nodes[5], nodes[8] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[7], nodes[9], nodes[12] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[11], nodes[13], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[15], nodes[17], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[21], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[25], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[29], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[33], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[37], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[44], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[1], nodes[45], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[48], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[5], nodes[49], nodes[6] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[52], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[9], nodes[53], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[56], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[57], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[60], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[61], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[64], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[65], nodes[22] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[68], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[69], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[72], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[29], nodes[73], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[78], nodes[76], nodes[77] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[77], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[80], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[81], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[43], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[46], nodes[1] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[47], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[2], nodes[50], nodes[5] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[51], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[6], nodes[54], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[55], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[10], nodes[58], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[59], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[62], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[63], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[66], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[67], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[22], nodes[70], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[71], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[74], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[75], nodes[78] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[78], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[77], nodes[79], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[82], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[1], nodes[2], nodes[4] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[5], nodes[6], nodes[8] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[9], nodes[10], nodes[12] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[14], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[18], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[22], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[26], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[29], nodes[30], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[34], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[38], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[1], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[2], nodes[5], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[6], nodes[9], nodes[7] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[10], nodes[13], nodes[11] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[17], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[21], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[22], nodes[25], nodes[23] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[29], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[33], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[37], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[43], nodes[44], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[45], nodes[1] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[48], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[49], nodes[5] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[52], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[53], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[55], nodes[56], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[57], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[60], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[61], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[64], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[65], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[68], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[69], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[71], nodes[72], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[73], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[76], nodes[78] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[78], nodes[77], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[80], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[81], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[83], nodes[43], nodes[84] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[46], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[44], nodes[47], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[50], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[51], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[54], nodes[6] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[55], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[58], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[59], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[62], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[63], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[66], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[64], nodes[67], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[70], nodes[22] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[71], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[74], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[72], nodes[75], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[78], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[79], nodes[77] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[77], nodes[82], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            #endregion

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0, F / 4.0, 0, 0, 0);
            nodes.Where(nd => nd.Position.X == 5).ToList().ForEach(x => x.AddAttribute(f));

            NodeRestrainAttribute dz = new NodeRestrainAttribute("freedomCase", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);

            nodes.Where(nd => nd.Position.X == 0).ToList().ForEach(nd => nd.AddAttribute(dz));
            nodes.Where(nd => nd.Position.X == 10).ToList().ForEach(nd => nd.AddAttribute(dz));

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.RZ);
            fix.AddExternalRestrain(Solver.DOF.DDZ);

            nodes.ForEach(n => n.AddAttribute(fix));

            LinearSolver fem0 = new LinearSolver(els.ToArray());

            var dzTLG = fem0.GetNodeDisplacementGlobalCoordinates(nodes.Where(x => x.Position.X == 5).First(), Solver.DOF.DZ);
            Console.WriteLine("displ = " + dzTLG + " vs " + spost);
            Assert.AreEqual(1.0, spost / dzTLG, 0.005); //Come soluzione kirchoff

            var pt1 = nodes.Where(x => x.Position.X == 5 && x.Position.Y == 3.83333).First();
            var pt2 = nodes.Where(x => x.Position.X == 5.5 && x.Position.Y == 3.83333).First();
            var pt3 = nodes.Where(x => x.Position.X == 5 && x.Position.Y == 4.16667).First();
            var element = els.Where(x => x.Nodes.Contains(pt1) && x.Nodes.Contains(pt2) && x.Nodes.Contains(pt3)).First(); //elemento
            Console.WriteLine("Plate nodes:");
            foreach (Node n in element.Nodes)
            {
                Console.WriteLine(n.Position);
            }

            #region glass top
            double sigma = M / W;
            var stressTopGlassTopFaceNode1 = fem0.GetTri3TLG2GlassStress(element, Tri3TripledLaminatedGlassV2.Glass.Top, Plate.Face.Top, 1, sys);
            Assert.AreEqual(1.0, sigma / stressTopGlassTopFaceNode1[0, 0], 0.05);

            var stressTopGlassTopFaceNode2 = fem0.GetTri3TLG2GlassStress(element, Tri3TripledLaminatedGlassV2.Glass.Top, Plate.Face.Top, 2, sys);

            var stressTopGlassTopFaceNode3 = fem0.GetTri3TLG2GlassStress(element, Tri3TripledLaminatedGlassV2.Glass.Top, Plate.Face.Top, 3, sys);
            Assert.AreEqual(1.0, stressTopGlassTopFaceNode3[0, 0] / sigma, 0.05);

            var bendingGlass = fem0.GetTri3TLG2GlassBending(element, 1);
            var forcesGlass = fem0.GetTri3TLG2GlassForces(element, 1);
            var bendingInterlayer = fem0.GetTri3TLG2InterlayerBending(element, 1);
            var bending = bendingGlass[0, 0] + forcesGlass[0, 0] * (hGlass1 / 2.0 + hGlass2 / 2.0 + hInterlayer);
            Assert.AreEqual(1.0, (M / b) / bending, 0.05);

            element = els[45]; //elemento
            Console.WriteLine("Plate 46:");
            foreach (Node n in element.Nodes)
            {
                Console.WriteLine(n.Position);
            }
            bendingGlass = fem0.GetTri3TLG2GlassBending(element, 1);
            forcesGlass = fem0.GetTri3TLG2GlassForces(element, 1);
            bendingInterlayer = fem0.GetTri3TLG2InterlayerBending(element, 1);
            var stressInterlayerNode1 = fem0.GetTri3TLG2InterlayerStress(element, 1);
            var tau = 1.5 * V / (b * h);
            FEMUtilities.WriteMatrix(stressInterlayerNode1);
            Console.WriteLine("theoric = " + tau);
            //Assert.AreEqual(1.0, tau / stressInterlayerNode1, 0.05);

            //NOTE: elemento kirchoff funzionante, ritorna valori sballati per stress interlayer in quanto, la teoria del TLG è "tirata per i capelli" e insorgono problemi numerici.
            //lo stesso esempio ricalcolato con valori di G0 e hInterlayer "semi-reali" dopo confronto con modello a brick anche il valore dello stress nell'interlayer converge. Vedi esempio sotto.
            #endregion
        }

        /// <summary>
        /// trave appoggiata con forza concentrata in mezzeria - caso con valori di G0, interlayer ecc semi-reali
        /// confronto con caso a brick
        /// </summary>
        [TestMethod]
        public void BeamTest3a()
        {
            double hGlass1 = 0.5;
            double hGlass2 = 0.5;
            double EGlass = 1000.0;
            double niGlass = 0.0;

            double EInterlayer = 0.1;
            double niInterlayer = 0.0;
            double G0 = EInterlayer / (2.0 * (1.0 + niInterlayer));

            double hInterlayer = 0.1;

            double F = 4.0;
            double L = 10.0;
            double b = 1.0;
            double h = hGlass1 + hGlass2 + hInterlayer;

            double V = F / 2.0;
            double M = F * L / 4.0;

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(-1e6, -1e6, -1e6));
            nodes.Add(new Node(0.5, 4.16667, 0));
            nodes.Add(new Node(1, 4.16667, 0));
            nodes.Add(new Node(1, 4.5, 0));
            nodes.Add(new Node(0.5, 4.5, 0));
            nodes.Add(new Node(1.5, 4.16667, 0));
            nodes.Add(new Node(2, 4.16667, 0));
            nodes.Add(new Node(2, 4.5, 0));
            nodes.Add(new Node(1.5, 4.5, 0));
            nodes.Add(new Node(2.5, 4.16667, 0));
            nodes.Add(new Node(3, 4.16667, 0));
            nodes.Add(new Node(3, 4.5, 0));
            nodes.Add(new Node(2.5, 4.5, 0));
            nodes.Add(new Node(3.5, 4.16667, 0));
            nodes.Add(new Node(4, 4.16667, 0));
            nodes.Add(new Node(4, 4.5, 0));
            nodes.Add(new Node(3.5, 4.5, 0));
            nodes.Add(new Node(4.5, 4.16667, 0));
            nodes.Add(new Node(5, 4.16667, 0));
            nodes.Add(new Node(5, 4.5, 0));
            nodes.Add(new Node(4.5, 4.5, 0));
            nodes.Add(new Node(5.5, 4.16667, 0));
            nodes.Add(new Node(6, 4.16667, 0));
            nodes.Add(new Node(6, 4.5, 0));
            nodes.Add(new Node(5.5, 4.5, 0));
            nodes.Add(new Node(6.5, 4.16667, 0));
            nodes.Add(new Node(7, 4.16667, 0));
            nodes.Add(new Node(7, 4.5, 0));
            nodes.Add(new Node(6.5, 4.5, 0));
            nodes.Add(new Node(7.5, 4.16667, 0));
            nodes.Add(new Node(8, 4.16667, 0));
            nodes.Add(new Node(8, 4.5, 0));
            nodes.Add(new Node(7.5, 4.5, 0));
            nodes.Add(new Node(8.5, 4.16667, 0));
            nodes.Add(new Node(9, 4.16667, 0));
            nodes.Add(new Node(9, 4.5, 0));
            nodes.Add(new Node(8.5, 4.5, 0));
            nodes.Add(new Node(9.5, 4.16667, 0));
            nodes.Add(new Node(10, 4.16667, 0));
            nodes.Add(new Node(10, 4.5, 0));
            nodes.Add(new Node(9.5, 4.5, 0));
            nodes.Add(new Node(0, 4.16667, 0));
            nodes.Add(new Node(0, 4.5, 0));
            nodes.Add(new Node(0.5, 3.5, 0));
            nodes.Add(new Node(1, 3.5, 0));
            nodes.Add(new Node(1, 3.83333, 0));
            nodes.Add(new Node(0.5, 3.83333, 0));
            nodes.Add(new Node(1.5, 3.5, 0));
            nodes.Add(new Node(2, 3.5, 0));
            nodes.Add(new Node(2, 3.83333, 0));
            nodes.Add(new Node(1.5, 3.83333, 0));
            nodes.Add(new Node(2.5, 3.5, 0));
            nodes.Add(new Node(3, 3.5, 0));
            nodes.Add(new Node(3, 3.83333, 0));
            nodes.Add(new Node(2.5, 3.83333, 0));
            nodes.Add(new Node(3.5, 3.5, 0));
            nodes.Add(new Node(4, 3.5, 0));
            nodes.Add(new Node(4, 3.83333, 0));
            nodes.Add(new Node(3.5, 3.83333, 0));
            nodes.Add(new Node(4.5, 3.5, 0));
            nodes.Add(new Node(5, 3.5, 0));
            nodes.Add(new Node(5, 3.83333, 0));
            nodes.Add(new Node(4.5, 3.83333, 0));
            nodes.Add(new Node(5.5, 3.5, 0));
            nodes.Add(new Node(6, 3.5, 0));
            nodes.Add(new Node(6, 3.83333, 0));
            nodes.Add(new Node(5.5, 3.83333, 0));
            nodes.Add(new Node(6.5, 3.5, 0));
            nodes.Add(new Node(7, 3.5, 0));
            nodes.Add(new Node(7, 3.83333, 0));
            nodes.Add(new Node(6.5, 3.83333, 0));
            nodes.Add(new Node(7.5, 3.5, 0));
            nodes.Add(new Node(8, 3.5, 0));
            nodes.Add(new Node(8, 3.83333, 0));
            nodes.Add(new Node(7.5, 3.83333, 0));
            nodes.Add(new Node(8.5, 3.5, 0));
            nodes.Add(new Node(9, 3.5, 0));
            nodes.Add(new Node(9, 3.83333, 0));
            nodes.Add(new Node(8.5, 3.83333, 0));
            nodes.Add(new Node(9.5, 3.5, 0));
            nodes.Add(new Node(10, 3.5, 0));
            nodes.Add(new Node(10, 3.83333, 0));
            nodes.Add(new Node(9.5, 3.83333, 0));
            nodes.Add(new Node(0, 3.5, 0));
            nodes.Add(new Node(0, 3.83333, 0));
            #endregion

            List<Tri3TripledLaminatedGlassV2> els = new List<Tri3TripledLaminatedGlassV2>();
            #region els
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[4], nodes[2], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[8], nodes[6], nodes[7] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[12], nodes[10], nodes[11] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[14], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[18], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[22], nodes[23] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[26], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[30], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[36], nodes[34], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[38], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[1], nodes[4] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[3], nodes[5], nodes[8] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[7], nodes[9], nodes[12] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[11], nodes[13], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[15], nodes[17], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[21], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[25], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[29], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[33], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[37], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[44], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[1], nodes[45], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[48], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[5], nodes[49], nodes[6] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[52], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[9], nodes[53], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[56], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[57], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[60], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[61], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[64], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[65], nodes[22] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[68], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[69], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[72], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[29], nodes[73], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[78], nodes[76], nodes[77] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[77], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[80], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[81], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[43], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[46], nodes[1] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[47], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[2], nodes[50], nodes[5] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[51], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[6], nodes[54], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[55], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[10], nodes[58], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[59], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[62], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[63], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[66], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[67], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[22], nodes[70], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[71], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[74], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[75], nodes[78] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[78], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[77], nodes[79], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[82], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[1], nodes[2], nodes[4] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[5], nodes[6], nodes[8] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[9], nodes[10], nodes[12] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[14], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[18], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[22], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[26], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[29], nodes[30], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[34], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[38], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[1], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[2], nodes[5], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[6], nodes[9], nodes[7] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[10], nodes[13], nodes[11] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[17], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[21], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[22], nodes[25], nodes[23] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[29], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[33], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[37], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[43], nodes[44], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[45], nodes[1] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[48], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[49], nodes[5] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[52], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[53], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[55], nodes[56], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[57], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[60], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[61], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[64], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[65], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[68], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[69], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[71], nodes[72], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[73], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[76], nodes[78] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[78], nodes[77], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[80], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[81], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[83], nodes[43], nodes[84] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[46], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[44], nodes[47], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[50], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[51], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[54], nodes[6] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[55], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[58], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[59], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[62], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[63], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[66], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[64], nodes[67], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[70], nodes[22] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[71], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[74], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[72], nodes[75], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[78], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[79], nodes[77] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[77], nodes[82], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            #endregion

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0, F / 4.0, 0, 0, 0);
            nodes.Where(nd => nd.Position.X == 5).ToList().ForEach(x => x.AddAttribute(f));

            NodeRestrainAttribute dz = new NodeRestrainAttribute("freedomCase", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);

            nodes.Where(nd => nd.Position.X == 0).ToList().ForEach(nd => nd.AddAttribute(dz));
            nodes.Where(nd => nd.Position.X == 10).ToList().ForEach(nd => nd.AddAttribute(dz));

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.RZ);
            fix.AddExternalRestrain(Solver.DOF.DDZ);

            nodes.ForEach(x => x.AddAttribute(fix));

            LinearSolver fem0 = new LinearSolver(els.ToArray());

            var dzTLG = fem0.GetNodeDisplacementGlobalCoordinates(nodes[63], Solver.DOF.DZ);
            var spost = 3.70;
            Console.WriteLine("displ = " + dzTLG + " vs " + spost);
            Assert.AreEqual(1.0, spost / dzTLG, 0.02); //Come soluzione kirchoff

            var pt1 = nodes.Where(x => x.Position.X == 5 && x.Position.Y == 3.83333).First();
            var pt2 = nodes.Where(x => x.Position.X == 5.5 && x.Position.Y == 3.83333).First();
            var pt3 = nodes.Where(x => x.Position.X == 5 && x.Position.Y == 4.16667).First();
            var element = els.Where(x => x.Nodes.Contains(pt1) && x.Nodes.Contains(pt2) && x.Nodes.Contains(pt3)).First(); //elemento
            Console.WriteLine("Plate:");
            foreach (Node n in element.Nodes)
            {
                Console.WriteLine(n.Position);
            }

            #region glass top
            double sigma = 108;
            var stressTopGlassTopFaceNode2 = fem0.GetTri3TLG2GlassStress(element, Tri3TripledLaminatedGlassV2.Glass.Top, Plate.Face.Top, 1);
            Assert.AreEqual(1.0, sigma / stressTopGlassTopFaceNode2[0, 0], 0.051);

            var bendingGlass = fem0.GetTri3TLG2GlassBending(element, 1);
            var forcesGlass = fem0.GetTri3TLG2GlassForces(element, 1);
            var bendingInterlayer = fem0.GetTri3TLG2InterlayerBending(element, 1);
            var bending = bendingGlass[0, 0] + forcesGlass[0, 0] * (hGlass1 / 2.0 + hGlass2 / 2.0 + hInterlayer);
            Assert.AreEqual(1.0, (M / b) / bending, 0.01);

            pt1 = nodes.Where(x => x.Position.X == 2 && x.Position.Y == 3.83333).First();
            pt2 = nodes.Where(x => x.Position.X == 2.5 && x.Position.Y == 3.83333).First();
            pt3 = nodes.Where(x => x.Position.X == 2 && x.Position.Y == 4.16667).First();
            element = els.Where(x => x.Nodes.Contains(pt1) && x.Nodes.Contains(pt2) && x.Nodes.Contains(pt3)).First(); //elemento
            Console.WriteLine("Plate nodes:");
            foreach (Node n in element.Nodes)
            {
                Console.WriteLine(n.Position);
            }
            bendingGlass = fem0.GetTri3TLG2GlassBending(element, 1);
            forcesGlass = fem0.GetTri3TLG2GlassForces(element, 1);
            bendingInterlayer = fem0.GetTri3TLG2InterlayerBending(element, 1);
            var stressInterlayerNode1 = fem0.GetTri3TLG2InterlayerStress(element, 1);
            double tau = 0.259;
            Assert.AreEqual(1.0, tau / stressInterlayerNode1[0, 2], 0.055);
            #endregion
        }

        /// <summary>
        /// Rotated
        /// Piastra kirchoff 10x10 semplicemente appoggiata nei nodi d'angolo con carico concentrato in mezzeria. Rotazione 45 gradi
        /// </summary>
        [TestMethod]
        public void RotatedTest1()
        {
            double hGlass1 = 0.5;
            double hGlass2 = 0.5;
            double EGlass = 1000.0;
            double niGlass = 0.0;

            double G0 = EGlass / (2.0 * (1.0 + niGlass));

            double hInterlayer = 0.01;

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(-1e6, -1e6, -1e6));
            nodes.Add(new Node(0, 0, 0));
            nodes.Add(new Node(-0.707107, 0.707107, 0));
            nodes.Add(new Node(-1.41421, 1.41421, 0));
            nodes.Add(new Node(-2.12132, 2.12132, 0));
            nodes.Add(new Node(-2.82843, 2.82843, 0));
            nodes.Add(new Node(-3.53553, 3.53553, 0));
            nodes.Add(new Node(-4.24264, 4.24264, 0));
            nodes.Add(new Node(-4.94975, 4.94975, 0));
            nodes.Add(new Node(-5.65685, 5.65685, 0));
            nodes.Add(new Node(-6.36396, 6.36396, 0));
            nodes.Add(new Node(-7.07107, 7.07107, 0));
            nodes.Add(new Node(0.707107, 0.707107, 0));
            nodes.Add(new Node(0, 1.41421, 0));
            nodes.Add(new Node(-0.707107, 2.12132, 0));
            nodes.Add(new Node(-1.41421, 2.82843, 0));
            nodes.Add(new Node(-2.12132, 3.53553, 0));
            nodes.Add(new Node(-2.82843, 4.24264, 0));
            nodes.Add(new Node(-3.53553, 4.94975, 0));
            nodes.Add(new Node(-4.24264, 5.65685, 0));
            nodes.Add(new Node(-4.94975, 6.36396, 0));
            nodes.Add(new Node(-5.65685, 7.07107, 0));
            nodes.Add(new Node(-6.36396, 7.77817, 0));
            nodes.Add(new Node(1.41421, 1.41421, 0));
            nodes.Add(new Node(0.707107, 2.12132, 0));
            nodes.Add(new Node(0, 2.82843, 0));
            nodes.Add(new Node(-0.707107, 3.53553, 0));
            nodes.Add(new Node(-1.41421, 4.24264, 0));
            nodes.Add(new Node(-2.12132, 4.94975, 0));
            nodes.Add(new Node(-2.82843, 5.65685, 0));
            nodes.Add(new Node(-3.53553, 6.36396, 0));
            nodes.Add(new Node(-4.24264, 7.07107, 0));
            nodes.Add(new Node(-4.94975, 7.77817, 0));
            nodes.Add(new Node(-5.65685, 8.48528, 0));
            nodes.Add(new Node(2.12132, 2.12132, 0));
            nodes.Add(new Node(1.41421, 2.82843, 0));
            nodes.Add(new Node(0.707107, 3.53553, 0));
            nodes.Add(new Node(0, 4.24264, 0));
            nodes.Add(new Node(-0.707107, 4.94975, 0));
            nodes.Add(new Node(-1.41421, 5.65685, 0));
            nodes.Add(new Node(-2.12132, 6.36396, 0));
            nodes.Add(new Node(-2.82843, 7.07107, 0));
            nodes.Add(new Node(-3.53553, 7.77817, 0));
            nodes.Add(new Node(-4.24264, 8.48528, 0));
            nodes.Add(new Node(-4.94975, 9.19239, 0));
            nodes.Add(new Node(2.82843, 2.82843, 0));
            nodes.Add(new Node(2.12132, 3.53553, 0));
            nodes.Add(new Node(1.41421, 4.24264, 0));
            nodes.Add(new Node(0.707107, 4.94975, 0));
            nodes.Add(new Node(0, 5.65685, 0));
            nodes.Add(new Node(-0.707107, 6.36396, 0));
            nodes.Add(new Node(-1.41421, 7.07107, 0));
            nodes.Add(new Node(-2.12132, 7.77817, 0));
            nodes.Add(new Node(-2.82843, 8.48528, 0));
            nodes.Add(new Node(-3.53553, 9.19239, 0));
            nodes.Add(new Node(-4.24264, 9.89949, 0));
            nodes.Add(new Node(3.53553, 3.53553, 0));
            nodes.Add(new Node(2.82843, 4.24264, 0));
            nodes.Add(new Node(2.12132, 4.94975, 0));
            nodes.Add(new Node(1.41421, 5.65685, 0));
            nodes.Add(new Node(0.707107, 6.36396, 0));
            nodes.Add(new Node(0, 7.07107, 0));
            nodes.Add(new Node(-0.707107, 7.77817, 0));
            nodes.Add(new Node(-1.41421, 8.48528, 0));
            nodes.Add(new Node(-2.12132, 9.19239, 0));
            nodes.Add(new Node(-2.82843, 9.89949, 0));
            nodes.Add(new Node(-3.53553, 10.6066, 0));
            nodes.Add(new Node(4.24264, 4.24264, 0));
            nodes.Add(new Node(3.53553, 4.94975, 0));
            nodes.Add(new Node(2.82843, 5.65685, 0));
            nodes.Add(new Node(2.12132, 6.36396, 0));
            nodes.Add(new Node(1.41421, 7.07107, 0));
            nodes.Add(new Node(0.707107, 7.77817, 0));
            nodes.Add(new Node(0, 8.48528, 0));
            nodes.Add(new Node(-0.707107, 9.19239, 0));
            nodes.Add(new Node(-1.41421, 9.89949, 0));
            nodes.Add(new Node(-2.12132, 10.6066, 0));
            nodes.Add(new Node(-2.82843, 11.3137, 0));
            nodes.Add(new Node(4.94975, 4.94975, 0));
            nodes.Add(new Node(4.24264, 5.65685, 0));
            nodes.Add(new Node(3.53553, 6.36396, 0));
            nodes.Add(new Node(2.82843, 7.07107, 0));
            nodes.Add(new Node(2.12132, 7.77817, 0));
            nodes.Add(new Node(1.41421, 8.48528, 0));
            nodes.Add(new Node(0.707107, 9.19239, 0));
            nodes.Add(new Node(0, 9.89949, 0));
            nodes.Add(new Node(-0.707107, 10.6066, 0));
            nodes.Add(new Node(-1.41421, 11.3137, 0));
            nodes.Add(new Node(-2.12132, 12.0208, 0));
            nodes.Add(new Node(5.65685, 5.65685, 0));
            nodes.Add(new Node(4.94975, 6.36396, 0));
            nodes.Add(new Node(4.24264, 7.07107, 0));
            nodes.Add(new Node(3.53553, 7.77817, 0));
            nodes.Add(new Node(2.82843, 8.48528, 0));
            nodes.Add(new Node(2.12132, 9.19239, 0));
            nodes.Add(new Node(1.41421, 9.89949, 0));
            nodes.Add(new Node(0.707107, 10.6066, 0));
            nodes.Add(new Node(0, 11.3137, 0));
            nodes.Add(new Node(-0.707107, 12.0208, 0));
            nodes.Add(new Node(-1.41421, 12.7279, 0));
            nodes.Add(new Node(6.36396, 6.36396, 0));
            nodes.Add(new Node(5.65685, 7.07107, 0));
            nodes.Add(new Node(4.94975, 7.77817, 0));
            nodes.Add(new Node(4.24264, 8.48528, 0));
            nodes.Add(new Node(3.53553, 9.19239, 0));
            nodes.Add(new Node(2.82843, 9.89949, 0));
            nodes.Add(new Node(2.12132, 10.6066, 0));
            nodes.Add(new Node(1.41421, 11.3137, 0));
            nodes.Add(new Node(0.707107, 12.0208, 0));
            nodes.Add(new Node(0, 12.7279, 0));
            nodes.Add(new Node(-0.707107, 13.435, 0));
            nodes.Add(new Node(7.07107, 7.07107, 0));
            nodes.Add(new Node(6.36396, 7.77817, 0));
            nodes.Add(new Node(5.65685, 8.48528, 0));
            nodes.Add(new Node(4.94975, 9.19239, 0));
            nodes.Add(new Node(4.24264, 9.89949, 0));
            nodes.Add(new Node(3.53553, 10.6066, 0));
            nodes.Add(new Node(2.82843, 11.3137, 0));
            nodes.Add(new Node(2.12132, 12.0208, 0));
            nodes.Add(new Node(1.41421, 12.7279, 0));
            nodes.Add(new Node(0.707107, 13.435, 0));
            nodes.Add(new Node(0, 14.1421, 0));
            #endregion

            List<Tri3TripledLaminatedGlassV2> els = new List<Tri3TripledLaminatedGlassV2>();
            #region els
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[1], nodes[12] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[2], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[15], nodes[3], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[4], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[5], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[6], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[7], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[8], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[9], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[22], nodes[10], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[12], nodes[23] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[13], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[14], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[15], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[16], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[29], nodes[17], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[18], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[19], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[20], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[21], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[23], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[36], nodes[24], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[25], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[26], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[39], nodes[27], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[28], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[29], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[30], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[43], nodes[31], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[44], nodes[32], nodes[43] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[34], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[35], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[36], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[37], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[38], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[39], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[40], nodes[51] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[41], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[42], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[55], nodes[43], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[45], nodes[56] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[46], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[47], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[48], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[49], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[50], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[51], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[64], nodes[52], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[53], nodes[64] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[54], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[56], nodes[67] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[57], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[58], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[71], nodes[59], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[72], nodes[60], nodes[71] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[61], nodes[72] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[62], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[63], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[64], nodes[75] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[77], nodes[65], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[67], nodes[78] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[80], nodes[68], nodes[79] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[81], nodes[69], nodes[80] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[70], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[83], nodes[71], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[72], nodes[83] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[85], nodes[73], nodes[84] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[86], nodes[74], nodes[85] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[87], nodes[75], nodes[86] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[88], nodes[76], nodes[87] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[90], nodes[78], nodes[89] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[91], nodes[79], nodes[90] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[92], nodes[80], nodes[91] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[93], nodes[81], nodes[92] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[94], nodes[82], nodes[93] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[95], nodes[83], nodes[94] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[96], nodes[84], nodes[95] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[97], nodes[85], nodes[96] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[98], nodes[86], nodes[97] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[99], nodes[87], nodes[98] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[101], nodes[89], nodes[100] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[102], nodes[90], nodes[101] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[103], nodes[91], nodes[102] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[104], nodes[92], nodes[103] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[105], nodes[93], nodes[104] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[106], nodes[94], nodes[105] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[107], nodes[95], nodes[106] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[108], nodes[96], nodes[107] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[109], nodes[97], nodes[108] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[110], nodes[98], nodes[109] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[112], nodes[100], nodes[111] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[113], nodes[101], nodes[112] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[114], nodes[102], nodes[113] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[115], nodes[103], nodes[114] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[116], nodes[104], nodes[115] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[117], nodes[105], nodes[116] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[118], nodes[106], nodes[117] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[119], nodes[107], nodes[118] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[120], nodes[108], nodes[119] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[121], nodes[109], nodes[120] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[2], nodes[1], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[3], nodes[2], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[4], nodes[3], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[5], nodes[4], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[6], nodes[5], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[7], nodes[6], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[8], nodes[7], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[9], nodes[8], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[10], nodes[9], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[11], nodes[10], nodes[22] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[12], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[13], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[15], nodes[14], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[15], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[16], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[17], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[18], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[19], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[20], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[22], nodes[21], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[23], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[24], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[25], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[26], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[27], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[29], nodes[28], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[29], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[30], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[31], nodes[43] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[32], nodes[44] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[34], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[36], nodes[35], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[36], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[37], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[39], nodes[38], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[39], nodes[51] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[40], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[41], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[43], nodes[42], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[44], nodes[43], nodes[55] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[45], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[46], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[47], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[48], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[49], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[50], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[51], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[52], nodes[64] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[53], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[55], nodes[54], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[56], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[57], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[58], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[59], nodes[71] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[60], nodes[72] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[61], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[62], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[64], nodes[63], nodes[75] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[64], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[65], nodes[77] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[67], nodes[79] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[68], nodes[80] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[69], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[71], nodes[70], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[72], nodes[71], nodes[83] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[72], nodes[84] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[73], nodes[85] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[74], nodes[86] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[75], nodes[87] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[77], nodes[76], nodes[88] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[78], nodes[90] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[80], nodes[79], nodes[91] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[81], nodes[80], nodes[92] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[81], nodes[93] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[83], nodes[82], nodes[94] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[83], nodes[95] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[85], nodes[84], nodes[96] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[86], nodes[85], nodes[97] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[87], nodes[86], nodes[98] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[88], nodes[87], nodes[99] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[90], nodes[89], nodes[101] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[91], nodes[90], nodes[102] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[92], nodes[91], nodes[103] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[93], nodes[92], nodes[104] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[94], nodes[93], nodes[105] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[95], nodes[94], nodes[106] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[96], nodes[95], nodes[107] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[97], nodes[96], nodes[108] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[98], nodes[97], nodes[109] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[99], nodes[98], nodes[110] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[101], nodes[100], nodes[112] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[102], nodes[101], nodes[113] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[103], nodes[102], nodes[114] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[104], nodes[103], nodes[115] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[105], nodes[104], nodes[116] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[106], nodes[105], nodes[117] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[107], nodes[106], nodes[118] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[108], nodes[107], nodes[119] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[109], nodes[108], nodes[120] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[110], nodes[109], nodes[121] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            #endregion

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeForceAttribute force = new NodeForceAttribute("lc", sys, 0, 0, 100.0, 0, 0, 0);
            nodes.Where(nd => nd.Position.X == 0 && nd.Position.Y >= 7.07 && nd.Position.Y <= 7.1).ToList().ForEach(nd => nd.AddAttribute(force));

            NodeRestrainAttribute dz = new NodeRestrainAttribute("freedomCase", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);

            double maxX = nodes.Select(x => x.Position.X).Max();
            double maxY = nodes.Select(x => x.Position.Y).Max();

            nodes.Where(nd => nd.Position.X >= -7.1 && nd.Position.X <= -7.0).First().AddAttribute(dz);
            nodes.Where(nd => nd.Position.X == maxX).First().AddAttribute(dz);
            nodes.Where(nd => nd.Position.X == 0 && nd.Position.Y == 0).First().AddAttribute(dz);
            nodes.Where(nd => nd.Position.Y == maxY).First().AddAttribute(dz);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.RZ);
            fix.AddExternalRestrain(Solver.DOF.DDZ);

            nodes.ForEach(n => n.AddAttribute(fix));

            LinearSolver fem0 = new LinearSolver(els.ToArray());

            double displ = 5.1240;
            double obtain = fem0.GetNodeDisplacementGlobalCoordinates(nodes.Where(nd => nd.Position.X == 0 && nd.Position.Y >= 7.07 && nd.Position.Y <= 7.1).First(), Solver.DOF.DZ);

            Assert.AreEqual(1.0, displ / obtain, 0.025);

            var pt1 = nodes.Where(x => x.Position.X == 2.82843 && x.Position.Y == 2.82843).First();
            var pt2 = nodes.Where(x => x.Position.X == 3.53553 && x.Position.Y == 3.53553).First();
            var pt3 = nodes.Where(x => x.Position.X == 2.82843 && x.Position.Y == 4.24264).First();
            var element = els.Where(x => x.Nodes.Contains(pt1) && x.Nodes.Contains(pt2) && x.Nodes.Contains(pt3)).First(); //elemento
            Console.WriteLine("Element nodes:");
            foreach (Node n in element.Nodes)
            {
                Console.WriteLine(n.Position);
            }

            var globalSys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            var stressGlobal = fem0.GetTri3TLG2GlassStress(element, Tri3TripledLaminatedGlassV2.Glass.Bottom, Plate.Face.Bottom, 1, globalSys);
            Assert.AreEqual(1.0, -74.22 / stressGlobal[0, 0], 0.075);
            Assert.AreEqual(1.0, -71.21 / stressGlobal[1, 1], 0.02);
            Assert.AreEqual(1.0, -58.70 / stressGlobal[0, 1], 0.03);
        }

        [TestMethod]
        ///Refe. to §6 of article: A plate finite element for modelling of triplex laminated glass and comparison with other computational method
        public void SimplySupportedTest3()
        {
            double F = 8.0 * 9.81; //N
            double EGlass = 72.0 * 1000; //MPa
            double L = 660; //mm
            double b = 200; //mm
            double hGlass = 2.875; //mm
            double hGlass1 = hGlass;
            double hGlass2 = hGlass;
            double hInterlayer = 0.38; //mm
            double hTot = hGlass + hInterlayer + hGlass; //mm
            double rhoEquivalent = 2.418 / 1000.0 / 1000.0; //kg/mm3
            double V = L * b * hTot; //mm3
            double Qtot = rhoEquivalent * V * 9.81; //N
            double q = Qtot / L; //N/mm

            double JSectionSolid = 1.0 / 12.0 * b * Math.Pow(hTot, 3.0);
            double JSection2Area = 2.0 * 1.0 / 12.0 * b * Math.Pow(hGlass, 3.0);

            double fLowerBound = F * (L * L * L) / (48.0 * EGlass * JSectionSolid) + 5.0 / 384.0 * q * Math.Pow(L, 4.0) / (EGlass * JSectionSolid);
            double fMaxBound = F * (L * L * L) / (48.0 * EGlass * JSection2Area) + 5.0 / 384.0 * q * Math.Pow(L, 4.0) / (EGlass * JSection2Area);

            Console.WriteLine("lower bound (monolitic) = " + fLowerBound);
            Console.WriteLine("max bound (uncoupled) = " + fMaxBound);

            double G0 = 0.5173;
            double niGlass = 0.23;

            Material mat = new SteelMaterial("mat", EGlass, niGlass, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), hGlass + hInterlayer + hGlass, hGlass + hInterlayer + hGlass, "p");

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(-1e6, -1e6, -1e6));
            nodes.Add(new Node(330, -100, 0));
            nodes.Add(new Node(330, 100, 0));
            nodes.Add(new Node(15, 100, 0));
            nodes.Add(new Node(15, -100, 0));
            nodes.Add(new Node(330, 0, 0));
            nodes.Add(new Node(172.5, -100, 0));
            nodes.Add(new Node(172.5, 0, 0));
            nodes.Add(new Node(172.5, 100, 0));
            nodes.Add(new Node(15, 0, 0));
            nodes.Add(new Node(0, 100, 0));
            nodes.Add(new Node(0, 0, 0));
            nodes.Add(new Node(0, -100, 0));
            nodes.Add(new Node(365, 100, 0));
            nodes.Add(new Node(365, 0, 0));
            nodes.Add(new Node(365, -100, 0));
            nodes.Add(new Node(172.5, 50, 0));
            nodes.Add(new Node(93.75, 0, 0));
            nodes.Add(new Node(93.75, 50, 0));
            nodes.Add(new Node(93.75, 100, 0));
            nodes.Add(new Node(15, 50, 0));
            nodes.Add(new Node(330, -50, 0));
            nodes.Add(new Node(251.25, -100, 0));
            nodes.Add(new Node(251.25, -50, 0));
            nodes.Add(new Node(251.25, 0, 0));
            nodes.Add(new Node(172.5, -50, 0));
            nodes.Add(new Node(330, 50, 0));
            nodes.Add(new Node(251.25, 50, 0));
            nodes.Add(new Node(251.25, 100, 0));
            nodes.Add(new Node(93.75, -100, 0));
            nodes.Add(new Node(93.75, -50, 0));
            nodes.Add(new Node(15, -50, 0));
            nodes.Add(new Node(7.5, 100, 0));
            nodes.Add(new Node(7.5, 50, 0));
            nodes.Add(new Node(7.5, 0, 0));
            nodes.Add(new Node(0, 50, 0));
            nodes.Add(new Node(7.5, -50, 0));
            nodes.Add(new Node(7.5, -100, 0));
            nodes.Add(new Node(0, -50, 0));
            nodes.Add(new Node(347.5, 100, 0));
            nodes.Add(new Node(347.5, 50, 0));
            nodes.Add(new Node(347.5, 0, 0));
            nodes.Add(new Node(365, 50, 0));
            nodes.Add(new Node(347.5, -50, 0));
            nodes.Add(new Node(347.5, -100, 0));
            nodes.Add(new Node(365, -50, 0));
            nodes.Add(new Node(93.75, 75, 0));
            nodes.Add(new Node(54.375, 50, 0));
            nodes.Add(new Node(54.375, 75, 0));
            nodes.Add(new Node(54.375, 100, 0));
            nodes.Add(new Node(15, 75, 0));
            nodes.Add(new Node(251.25, -25, 0));
            nodes.Add(new Node(211.875, -50, 0));
            nodes.Add(new Node(211.875, -25, 0));
            nodes.Add(new Node(211.875, 0, 0));
            nodes.Add(new Node(172.5, -25, 0));
            nodes.Add(new Node(251.25, 75, 0));
            nodes.Add(new Node(211.875, 50, 0));
            nodes.Add(new Node(211.875, 75, 0));
            nodes.Add(new Node(211.875, 100, 0));
            nodes.Add(new Node(172.5, 75, 0));
            nodes.Add(new Node(93.75, -25, 0));
            nodes.Add(new Node(54.375, -50, 0));
            nodes.Add(new Node(54.375, -25, 0));
            nodes.Add(new Node(54.375, 0, 0));
            nodes.Add(new Node(15, -25, 0));
            nodes.Add(new Node(7.5, 25, 0));
            nodes.Add(new Node(3.75, 0, 0));
            nodes.Add(new Node(3.75, 25, 0));
            nodes.Add(new Node(3.75, 50, 0));
            nodes.Add(new Node(0, 25, 0));
            nodes.Add(new Node(7.5, -75, 0));
            nodes.Add(new Node(3.75, -100, 0));
            nodes.Add(new Node(3.75, -75, 0));
            nodes.Add(new Node(3.75, -50, 0));
            nodes.Add(new Node(0, -75, 0));
            nodes.Add(new Node(347.5, 25, 0));
            nodes.Add(new Node(356.25, 50, 0));
            nodes.Add(new Node(356.25, 25, 0));
            nodes.Add(new Node(356.25, 0, 0));
            nodes.Add(new Node(365, 25, 0));
            nodes.Add(new Node(347.5, -75, 0));
            nodes.Add(new Node(356.25, -50, 0));
            nodes.Add(new Node(356.25, -75, 0));
            nodes.Add(new Node(356.25, -100, 0));
            nodes.Add(new Node(365, -75, 0));
            nodes.Add(new Node(172.5, 25, 0));
            nodes.Add(new Node(133.125, 0, 0));
            nodes.Add(new Node(133.125, 25, 0));
            nodes.Add(new Node(133.125, 50, 0));
            nodes.Add(new Node(93.75, 25, 0));
            nodes.Add(new Node(133.125, 75, 0));
            nodes.Add(new Node(133.125, 100, 0));
            nodes.Add(new Node(54.375, 25, 0));
            nodes.Add(new Node(15, 25, 0));
            nodes.Add(new Node(330, -75, 0));
            nodes.Add(new Node(290.625, -100, 0));
            nodes.Add(new Node(290.625, -75, 0));
            nodes.Add(new Node(290.625, -50, 0));
            nodes.Add(new Node(251.25, -75, 0));
            nodes.Add(new Node(330, -25, 0));
            nodes.Add(new Node(290.625, -25, 0));
            nodes.Add(new Node(290.625, 0, 0));
            nodes.Add(new Node(211.875, -100, 0));
            nodes.Add(new Node(211.875, -75, 0));
            nodes.Add(new Node(172.5, -75, 0));
            nodes.Add(new Node(330, 25, 0));
            nodes.Add(new Node(290.625, 25, 0));
            nodes.Add(new Node(290.625, 50, 0));
            nodes.Add(new Node(251.25, 25, 0));
            nodes.Add(new Node(330, 75, 0));
            nodes.Add(new Node(290.625, 75, 0));
            nodes.Add(new Node(290.625, 100, 0));
            nodes.Add(new Node(211.875, 25, 0));
            nodes.Add(new Node(133.125, -100, 0));
            nodes.Add(new Node(133.125, -75, 0));
            nodes.Add(new Node(133.125, -50, 0));
            nodes.Add(new Node(93.75, -75, 0));
            nodes.Add(new Node(133.125, -25, 0));
            nodes.Add(new Node(54.375, -100, 0));
            nodes.Add(new Node(54.375, -75, 0));
            nodes.Add(new Node(15, -75, 0));
            nodes.Add(new Node(11.25, 50, 0));
            nodes.Add(new Node(11.25, 75, 0));
            nodes.Add(new Node(11.25, 100, 0));
            nodes.Add(new Node(7.5, 75, 0));
            nodes.Add(new Node(11.25, 0, 0));
            nodes.Add(new Node(11.25, 25, 0));
            nodes.Add(new Node(3.75, 75, 0));
            nodes.Add(new Node(3.75, 100, 0));
            nodes.Add(new Node(0, 75, 0));
            nodes.Add(new Node(11.25, -50, 0));
            nodes.Add(new Node(11.25, -25, 0));
            nodes.Add(new Node(7.5, -25, 0));
            nodes.Add(new Node(11.25, -100, 0));
            nodes.Add(new Node(11.25, -75, 0));
            nodes.Add(new Node(3.75, -25, 0));
            nodes.Add(new Node(0, -25, 0));
            nodes.Add(new Node(338.75, 100, 0));
            nodes.Add(new Node(338.75, 75, 0));
            nodes.Add(new Node(338.75, 50, 0));
            nodes.Add(new Node(347.5, 75, 0));
            nodes.Add(new Node(338.75, 25, 0));
            nodes.Add(new Node(338.75, 0, 0));
            nodes.Add(new Node(356.25, 100, 0));
            nodes.Add(new Node(356.25, 75, 0));
            nodes.Add(new Node(365, 75, 0));
            nodes.Add(new Node(338.75, -25, 0));
            nodes.Add(new Node(338.75, -50, 0));
            nodes.Add(new Node(347.5, -25, 0));
            nodes.Add(new Node(338.75, -75, 0));
            nodes.Add(new Node(338.75, -100, 0));
            nodes.Add(new Node(356.25, -25, 0));
            nodes.Add(new Node(365, -25, 0));
            #endregion

            List<Tri3TripledLaminatedGlassV2> els = new List<Tri3TripledLaminatedGlassV2>();
            #region elements
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[144], nodes[146], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[145], nodes[42], nodes[146] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[77], nodes[80], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[78], nodes[14], nodes[80] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[153], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[152], nodes[45], nodes[153] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[85], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[83], nodes[15], nodes[85] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[81], nodes[84], nodes[83] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[43], nodes[83], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[149], nodes[82], nodes[152] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[152], nodes[79] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[79], nodes[78] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[78], nodes[77] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[141], nodes[77], nodes[145] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[39], nodes[145], nodes[144] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[138], nodes[141], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[139], nodes[40], nodes[141] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[140], nodes[76], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[142], nodes[41], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[143], nodes[149], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[147], nodes[43], nodes[149] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[148], nodes[81], nodes[43] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[150], nodes[44], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[95], nodes[151], nodes[150] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[150], nodes[148] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[100], nodes[148], nodes[147] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[5], nodes[147], nodes[143] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[106], nodes[143], nodes[142] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[142], nodes[140] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[110], nodes[140], nodes[139] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[2], nodes[139], nodes[138] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[112], nodes[110], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[111], nodes[26], nodes[110] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[108], nodes[106], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[107], nodes[5], nodes[106] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[102], nodes[100], nodes[5] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[101], nodes[21], nodes[100] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[98], nodes[95], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[97], nodes[1], nodes[95] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[99], nodes[96], nodes[97] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[97], nodes[98] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[98], nodes[101] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[101], nodes[102] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[109], nodes[102], nodes[107] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[107], nodes[108] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[108], nodes[111] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[111], nodes[112] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[56], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[27], nodes[56] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[109], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[113], nodes[24], nodes[109] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[51], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[23], nodes[51] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[99], nodes[23] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[104], nodes[22], nodes[99] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[105], nodes[103], nodes[104] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[104], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[55], nodes[52], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[7], nodes[53], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[86], nodes[54], nodes[113] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[113], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[57], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[8], nodes[58], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[92], nodes[60], nodes[8] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[91], nodes[16], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[89], nodes[86], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[88], nodes[7], nodes[86] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[87], nodes[55], nodes[7] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[118], nodes[25], nodes[55] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[116], nodes[105], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[115], nodes[6], nodes[105] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[117], nodes[114], nodes[115] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[115], nodes[116] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[116], nodes[118] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[118], nodes[87] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[90], nodes[87], nodes[88] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[88], nodes[89] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[89], nodes[91] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[91], nodes[92] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[120], nodes[29], nodes[117] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[117], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[30], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[64], nodes[61], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[93], nodes[17], nodes[90] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[90], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[18], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[46], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[3], nodes[48], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[47], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[93], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[94], nodes[64], nodes[93] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[9], nodes[63], nodes[64] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[62], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[120], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[121], nodes[119], nodes[120] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[135], nodes[4], nodes[121] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[131], nodes[121], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[132], nodes[31], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[126], nodes[65], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[127], nodes[9], nodes[94] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[122], nodes[94], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[123], nodes[20], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[124], nodes[50], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[71], nodes[134], nodes[135] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[36], nodes[135], nodes[131] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[133], nodes[131], nodes[132] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[132], nodes[126] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[126], nodes[127] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[127], nodes[122] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[125], nodes[122], nodes[123] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[123], nodes[124] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[37], nodes[71] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[71], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[136], nodes[36], nodes[133] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[133], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[34], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[66], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[128], nodes[33], nodes[125] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[129], nodes[125], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[72], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[73], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[137], nodes[74], nodes[136] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[11], nodes[136], nodes[67] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[67], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[68], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[130], nodes[69], nodes[128] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[10], nodes[128], nodes[129] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[145], nodes[146], nodes[144] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[77], nodes[42], nodes[145] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[78], nodes[80], nodes[77] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[14], nodes[78] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[152], nodes[153], nodes[79] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[45], nodes[152] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[83], nodes[85], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[15], nodes[83] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[44], nodes[84], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[81], nodes[83], nodes[43] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[43], nodes[82], nodes[149] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[149], nodes[152], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[79], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[78], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[77], nodes[141] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[141], nodes[145], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[139], nodes[141], nodes[138] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[140], nodes[40], nodes[139] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[142], nodes[76], nodes[140] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[143], nodes[41], nodes[142] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[147], nodes[149], nodes[143] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[148], nodes[43], nodes[147] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[150], nodes[81], nodes[148] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[151], nodes[44], nodes[150] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[1], nodes[151], nodes[95] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[95], nodes[150], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[148], nodes[100] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[100], nodes[147], nodes[5] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[5], nodes[143], nodes[106] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[106], nodes[142], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[140], nodes[110] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[110], nodes[139], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[111], nodes[110], nodes[112] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[108], nodes[26], nodes[111] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[107], nodes[106], nodes[108] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[102], nodes[5], nodes[107] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[101], nodes[100], nodes[102] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[98], nodes[21], nodes[101] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[97], nodes[95], nodes[98] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[96], nodes[1], nodes[97] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[22], nodes[96], nodes[99] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[99], nodes[97], nodes[23] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[98], nodes[51] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[101], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[102], nodes[109] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[109], nodes[107], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[108], nodes[56] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[111], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[56], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[57], nodes[27], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[113], nodes[109], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[24], nodes[113] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[51], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[23], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[104], nodes[99], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[103], nodes[22], nodes[104] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[6], nodes[103], nodes[105] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[105], nodes[104], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[52], nodes[55] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[55], nodes[53], nodes[7] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[7], nodes[54], nodes[86] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[86], nodes[113], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[57], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[58], nodes[8] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[91], nodes[60], nodes[92] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[89], nodes[16], nodes[91] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[88], nodes[86], nodes[89] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[87], nodes[7], nodes[88] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[118], nodes[55], nodes[87] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[116], nodes[25], nodes[118] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[115], nodes[105], nodes[116] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[114], nodes[6], nodes[115] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[29], nodes[114], nodes[117] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[117], nodes[115], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[116], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[118], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[87], nodes[90] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[90], nodes[88], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[89], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[91], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[119], nodes[29], nodes[120] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[120], nodes[117], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[30], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[61], nodes[64] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[64], nodes[17], nodes[93] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[93], nodes[90], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[18], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[46], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[50], nodes[48], nodes[3] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[47], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[94], nodes[93], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[9], nodes[64], nodes[94] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[63], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[62], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[121], nodes[120], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[4], nodes[119], nodes[121] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[134], nodes[4], nodes[135] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[135], nodes[121], nodes[131] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[131], nodes[31], nodes[132] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[132], nodes[65], nodes[126] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[126], nodes[9], nodes[127] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[127], nodes[94], nodes[122] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[122], nodes[20], nodes[123] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[123], nodes[50], nodes[124] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[134], nodes[71] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[71], nodes[135], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[36], nodes[131], nodes[133] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[133], nodes[132], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[126], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[127], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[122], nodes[125] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[125], nodes[123], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[72], nodes[37], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[71], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[36], nodes[136] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[136], nodes[133], nodes[67] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[34], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[66], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[33], nodes[128] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[128], nodes[125], nodes[129] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[12], nodes[72], nodes[75] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[73], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[74], nodes[137] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[137], nodes[136], nodes[11] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[11], nodes[67], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[68], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[69], nodes[130] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[130], nodes[128], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            #endregion

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            #region force
            PlatePressureAttribute pressure = new PlatePressureAttribute("loadCase", sys, 0.0, 0.0, (2.418 / 1000.0 * 9.81) * (hTot / 1000.0));
            els.ForEach(x => x.AddLoadCaseAttribute(pressure));

            NodeForceAttribute fNode = new NodeForceAttribute("loadCase", sys, 0, 0, (F / 2.0) / 9.0, 0, 0, 0);
            nodes.Where(x => x.Position.X == 15).ToList().ForEach(x => x.AddAttribute(fNode));
            #endregion

            #region restrains
            NodeRestrainAttribute x0Restrain = new NodeRestrainAttribute("freedomCase", sys);
            x0Restrain.AddExternalRestrain(Solver.DOF.DDX);
            x0Restrain.AddExternalRestrain(Solver.DOF.RY);
            nodes.Where(x => x.Position.X == 0).ToList().ForEach(x => x.AddAttribute(x0Restrain));
            if (nodes.Where(x => x.Position.X == 0).ToList().Count() != 9)
            {
                Console.WriteLine("restrains X = 0 not applied");
                return;
            }
            else
            {
                Console.WriteLine("9 restrains X = 0 applied");
            }

            NodeRestrainAttribute x0y0Restrain = new NodeRestrainAttribute("freedomCase", sys);
            x0y0Restrain.AddExternalRestrain(Solver.DOF.DDX);
            x0y0Restrain.AddExternalRestrain(Solver.DOF.DDY);
            x0y0Restrain.AddExternalRestrain(Solver.DOF.RY);
            nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).ToList().ForEach(x => x.AddAttribute(x0y0Restrain));
            if (nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).ToList().Count() != 1)
            {
                Console.WriteLine("restrains X = 0 Y=0 not applied");
                return;
            }
            else
            {
                Console.WriteLine("1 restrains X = 0 Y=0 applied");
            }

            NodeRestrainAttribute x330Restrain = new NodeRestrainAttribute("freedomCase", sys);
            x330Restrain.AddExternalRestrain(Solver.DOF.DZ);
            nodes.Where(x => x.Position.X == 330).ToList().ForEach(x => x.AddAttribute(x330Restrain));
            if (nodes.Where(x => x.Position.X == 330).ToList().Count() != 9)
            {
                Console.WriteLine("restrains X = 330 not applied");
                return;
            }
            else
            {
                Console.WriteLine("9 restrains X = 330 applied");
            }

            NodeRestrainAttribute x330y0Restrain = new NodeRestrainAttribute("freedomCase", sys);
            x330y0Restrain.AddExternalRestrain(Solver.DOF.DDY);
            x330y0Restrain.AddExternalRestrain(Solver.DOF.DZ);
            nodes.Where(x => x.Position.X == 330 && x.Position.Y == 0).ToList().ForEach(x => x.AddAttribute(x330y0Restrain));
            if (nodes.Where(x => x.Position.X == 330 && x.Position.Y == 0).ToList().Count() != 1)
            {
                Console.WriteLine("restrains X = 330 Y = 0 not applied");
                return;
            }
            else
            {
                Console.WriteLine("1 restrains X = 330 Y=0 applied");
            }

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);

            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);

            fix.AddExternalRestrain(Solver.DOF.RZ);

            fix.AddExternalRestrain(Solver.DOF.DDZ);

            nodes.ForEach(x => x.AddAttribute(fix));
            #endregion

            LinearSolver fem = new LinearSolver(els.ToArray());

            double DZTLG = fem.GetNodeDisplacementGlobalCoordinates(nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).First(), Solver.DOF.DZ);
            Console.WriteLine("displacement triple laminated glass = " + DZTLG);

            Assert.AreEqual(1.0, 3.73 / DZTLG, 0.01);
        }

        /// <summary>
        /// piastra 12x20, piastra semplicemente appoggiata con forze concentrate. elementi distorti
        /// </summary>
        [TestMethod]
        public void QuadrilateralTestRobert6()
        {
            double hGlass1 = 0.25;
            double hGlass2 = 0.25;
            double EGlass = 10000.0;
            double niGlass = 0.2;

            double G0 = 10.0;

            double hInterlayer = 0.01;

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(-1e6, -1e6, -1e6));
            nodes.Add(new Node(0, 0, 0));
            nodes.Add(new Node(0, 20, 0));
            nodes.Add(new Node(1, 0, 0));
            nodes.Add(new Node(4, 0, 0));
            nodes.Add(new Node(6, 0, 0));
            nodes.Add(new Node(9, 0, 0));
            nodes.Add(new Node(10, 0, 0));
            nodes.Add(new Node(12, 0, 0));
            nodes.Add(new Node(0, 2.5, 0));
            nodes.Add(new Node(1.5, 2, 0));
            nodes.Add(new Node(4, 2.5, 0));
            nodes.Add(new Node(6.5, 2, 0));
            nodes.Add(new Node(8, 1.7, 0));
            nodes.Add(new Node(10.5, 2.1, 0));
            nodes.Add(new Node(12, 2.5, 0));
            nodes.Add(new Node(0, 4, 0));
            nodes.Add(new Node(1.5, 4.3, 0));
            nodes.Add(new Node(4, 4, 0));
            nodes.Add(new Node(6.5, 4.5, 0));
            nodes.Add(new Node(8.5, 4.3, 0));
            nodes.Add(new Node(10.5, 4.3, 0));
            nodes.Add(new Node(12, 4, 0));
            nodes.Add(new Node(0, 6.5, 0));
            nodes.Add(new Node(1.5, 6.8, 0));
            nodes.Add(new Node(3.5, 6.8, 0));
            nodes.Add(new Node(6, 6.5, 0));
            nodes.Add(new Node(9, 6.5, 0));
            nodes.Add(new Node(10, 6.5, 0));
            nodes.Add(new Node(12, 6.5, 0));
            nodes.Add(new Node(0, 8, 0));
            nodes.Add(new Node(1.6, 8.6, 0));
            nodes.Add(new Node(4, 8, 0));
            nodes.Add(new Node(6.5, 8.5, 0));
            nodes.Add(new Node(9, 8, 0));
            nodes.Add(new Node(10, 8, 0));
            nodes.Add(new Node(12, 8, 0));
            nodes.Add(new Node(0, 10, 0));
            nodes.Add(new Node(1.3, 10.3, 0));
            nodes.Add(new Node(3.4, 10.6, 0));
            nodes.Add(new Node(6, 10, 0));
            nodes.Add(new Node(8, 10.6, 0));
            nodes.Add(new Node(10.3, 9.4, 0));
            nodes.Add(new Node(12, 10, 0));
            nodes.Add(new Node(0, 12, 0));
            nodes.Add(new Node(1.6, 12.6, 0));
            nodes.Add(new Node(4, 12, 0));
            nodes.Add(new Node(6, 12, 0));
            nodes.Add(new Node(8.5, 11.5, 0));
            nodes.Add(new Node(10.3, 12.3, 0));
            nodes.Add(new Node(12, 12, 0));
            nodes.Add(new Node(0, 14, 0));
            nodes.Add(new Node(1, 14, 0));
            nodes.Add(new Node(4, 14, 0));
            nodes.Add(new Node(6.5, 13.5, 0));
            nodes.Add(new Node(8.7, 13.7, 0));
            nodes.Add(new Node(10, 14, 0));
            nodes.Add(new Node(12, 14, 0));
            nodes.Add(new Node(0, 16.5, 0));
            nodes.Add(new Node(1.6, 15.9, 0));
            nodes.Add(new Node(3, 15.9, 0));
            nodes.Add(new Node(6, 16.5, 0));
            nodes.Add(new Node(8.7, 16.2, 0));
            nodes.Add(new Node(10.6, 16.2, 0));
            nodes.Add(new Node(12, 16.5, 0));
            nodes.Add(new Node(0, 18.5, 0));
            nodes.Add(new Node(1.5, 18, 0));
            nodes.Add(new Node(4, 18.5, 0));
            nodes.Add(new Node(6.3, 18.2, 0));
            nodes.Add(new Node(8.7, 18.2, 0));
            nodes.Add(new Node(10.3, 18.8, 0));
            nodes.Add(new Node(12, 18.5, 0));
            nodes.Add(new Node(1, 20, 0));
            nodes.Add(new Node(4, 20, 0));
            nodes.Add(new Node(6, 20, 0));
            nodes.Add(new Node(9, 20, 0));
            nodes.Add(new Node(10, 20, 0));
            nodes.Add(new Node(12, 20, 0));
            #endregion

            List<Tri3TripledLaminatedGlassV2> els = new List<Tri3TripledLaminatedGlassV2>();
            #region plates
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[9], nodes[3], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[10], nodes[4], nodes[11] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[11], nodes[5], nodes[12] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[12], nodes[6], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[7], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[8], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[10], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[11], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[12], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[13], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[14], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[15], nodes[22] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[17], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[18], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[19], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[20], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[21], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[22], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[24], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[25], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[26], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[27], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[28], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[29], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[31], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[32], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[39], nodes[33], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[34], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[35], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[36], nodes[43] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[44], nodes[38], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[39], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[40], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[41], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[42], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[43], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[45], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[46], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[47], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[48], nodes[55] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[55], nodes[49], nodes[56] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[50], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[52], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[53], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[54], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[55], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[56], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[57], nodes[64] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[59], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[60], nodes[67] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[61], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[62], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[63], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[64], nodes[71] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[2], nodes[66], nodes[72] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[72], nodes[67], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[68], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[69], nodes[75] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[70], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[71], nodes[77] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[1], nodes[3], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[3], nodes[4], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[4], nodes[5], nodes[11] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[5], nodes[6], nodes[12] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[6], nodes[7], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[7], nodes[8], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[9], nodes[10], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[10], nodes[11], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[11], nodes[12], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[12], nodes[13], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[14], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[15], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[17], nodes[23] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[18], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[19], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[20], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[21], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[22], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[24], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[25], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[26], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[27], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[28], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[29], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[31], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[32], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[33], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[34], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[35], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[36], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[38], nodes[44] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[39], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[39], nodes[40], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[41], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[42], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[43], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[44], nodes[45], nodes[51] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[46], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[47], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[48], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[49], nodes[55] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[50], nodes[56] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[52], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[53], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[54], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[55], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[55], nodes[56], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[57], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[59], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[60], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[61], nodes[67] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[62], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[63], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[64], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[66], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[67], nodes[72] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[68], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[69], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[70], nodes[75] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[71], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            #endregion

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0, 100, 0, 0, 0);
            nodes.Where(x => x.Position.X == 6.0 && x.Position.Y == 10.0).ToList().ForEach(x => x.AddAttribute(f));

            NodeRestrainAttribute dz = new NodeRestrainAttribute("freedomCase", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);

            nodes[1].AddAttribute(dz);
            nodes[8].AddAttribute(dz);
            nodes[77].AddAttribute(dz);
            nodes[2].AddAttribute(dz);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.RZ);
            fix.AddExternalRestrain(Solver.DOF.DDZ);

            nodes.ForEach(x => x.AddAttribute(fix));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Node center = nodes.Where(x => x.Position.X == 6 && x.Position.Y == 10).First();
            var femDZCenter = fem.GetNodeDisplacementGlobalCoordinates(center, Solver.DOF.DZ);
            Assert.AreEqual(1.0, 14.209 / femDZCenter, 0.01);

            Node angle = nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).First();
            var femAngle = fem.GetNodeDisplacementGlobalCoordinates(angle);
            Assert.AreEqual(1.0, -0.133 / femAngle[Solver.DOF.DDX], 0.15);
            Assert.AreEqual(1.0, -0.496 / femAngle[Solver.DOF.DDY], 0.15);
            Assert.AreEqual(1.0, 2.092 / femAngle[Solver.DOF.RX], 0.15);
            Assert.AreEqual(1.0, -0.68 / femAngle[Solver.DOF.RY], 0.15);

            Node centerBorder1 = nodes.Where(x => x.Position.X == 0 && x.Position.Y == 10).First();
            var femCenterBorder1 = fem.GetNodeDisplacementGlobalCoordinates(centerBorder1);
            Assert.AreEqual(1.0, 13.136 / femCenterBorder1[Solver.DOF.DZ], 0.15);
            Assert.AreEqual(1.0, -0.009 / femCenterBorder1[Solver.DOF.DDX], 0.15);
            Assert.AreEqual(-0.001, femCenterBorder1[Solver.DOF.DDY], 0.01);
            Assert.AreEqual(0.002, femCenterBorder1[Solver.DOF.RX], 0.03);
            Assert.AreEqual(-0.045, femCenterBorder1[Solver.DOF.RY], 0.01);

            Node centerBorder2 = nodes.Where(x => x.Position.X == 6 && x.Position.Y == 0).First();
            var femCenterBorder2 = fem.GetNodeDisplacementGlobalCoordinates(centerBorder2);
            Assert.AreEqual(1.0, 2.245 / femCenterBorder2[Solver.DOF.DZ], 0.2);
            Assert.AreEqual(-0.001, femCenterBorder2[Solver.DOF.DDX], 0.01);
            Assert.AreEqual(-0.416, femCenterBorder2[Solver.DOF.DDY], 0.01);
            Assert.AreEqual(1.603, femCenterBorder2[Solver.DOF.RX], 0.05);
            Assert.AreEqual(-0.006, femCenterBorder2[Solver.DOF.RY], 0.01);
        }

        /// <summary>
        /// piastra 12x20, piastra semplicemente appoggiata con forze concentrate. elementi distorti. Mesh raffinata
        /// </summary>
        [TestMethod]
        public void QuadrilateralTestRobert6a()
        {
            double hGlass1 = 0.25;
            double hGlass2 = 0.25;
            double EGlass = 10000.0;
            double niGlass = 0.2;

            double G0 = 10.0;

            double hInterlayer = 0.01;

            List<Node> nodes = new List<Node>();
            #region nodes
            nodes.Add(new Node(-1e6, -1e6, -1e6));
            nodes.Add(new Node(0, 0, 0));
            nodes.Add(new Node(0, 20, 0));
            nodes.Add(new Node(1, 0, 0));
            nodes.Add(new Node(4, 0, 0));
            nodes.Add(new Node(6, 0, 0));
            nodes.Add(new Node(9, 0, 0));
            nodes.Add(new Node(10, 0, 0));
            nodes.Add(new Node(12, 0, 0));
            nodes.Add(new Node(0, 2.5, 0));
            nodes.Add(new Node(1.5, 2, 0));
            nodes.Add(new Node(4, 2.5, 0));
            nodes.Add(new Node(6.5, 2, 0));
            nodes.Add(new Node(8, 1.7, 0));
            nodes.Add(new Node(10.5, 2.1, 0));
            nodes.Add(new Node(12, 2.5, 0));
            nodes.Add(new Node(0, 4, 0));
            nodes.Add(new Node(1.5, 4.3, 0));
            nodes.Add(new Node(4, 4, 0));
            nodes.Add(new Node(6.5, 4.5, 0));
            nodes.Add(new Node(8.5, 4.3, 0));
            nodes.Add(new Node(10.5, 4.3, 0));
            nodes.Add(new Node(12, 4, 0));
            nodes.Add(new Node(0, 6.5, 0));
            nodes.Add(new Node(1.5, 6.8, 0));
            nodes.Add(new Node(3.5, 6.8, 0));
            nodes.Add(new Node(6, 6.5, 0));
            nodes.Add(new Node(9, 6.5, 0));
            nodes.Add(new Node(10, 6.5, 0));
            nodes.Add(new Node(12, 6.5, 0));
            nodes.Add(new Node(0, 8, 0));
            nodes.Add(new Node(1.6, 8.6, 0));
            nodes.Add(new Node(4, 8, 0));
            nodes.Add(new Node(6.5, 8.5, 0));
            nodes.Add(new Node(9, 8, 0));
            nodes.Add(new Node(10, 8, 0));
            nodes.Add(new Node(12, 8, 0));
            nodes.Add(new Node(0, 10, 0));
            nodes.Add(new Node(1.3, 10.3, 0));
            nodes.Add(new Node(3.4, 10.6, 0));
            nodes.Add(new Node(6, 10, 0));
            nodes.Add(new Node(8, 10.6, 0));
            nodes.Add(new Node(10.3, 9.4, 0));
            nodes.Add(new Node(12, 10, 0));
            nodes.Add(new Node(0, 12, 0));
            nodes.Add(new Node(1.6, 12.6, 0));
            nodes.Add(new Node(4, 12, 0));
            nodes.Add(new Node(6, 12, 0));
            nodes.Add(new Node(8.5, 11.5, 0));
            nodes.Add(new Node(10.3, 12.3, 0));
            nodes.Add(new Node(12, 12, 0));
            nodes.Add(new Node(0, 14, 0));
            nodes.Add(new Node(1, 14, 0));
            nodes.Add(new Node(4, 14, 0));
            nodes.Add(new Node(6.5, 13.5, 0));
            nodes.Add(new Node(8.7, 13.7, 0));
            nodes.Add(new Node(10, 14, 0));
            nodes.Add(new Node(12, 14, 0));
            nodes.Add(new Node(0, 16.5, 0));
            nodes.Add(new Node(1.6, 15.9, 0));
            nodes.Add(new Node(3, 15.9, 0));
            nodes.Add(new Node(6, 16.5, 0));
            nodes.Add(new Node(8.7, 16.2, 0));
            nodes.Add(new Node(10.6, 16.2, 0));
            nodes.Add(new Node(12, 16.5, 0));
            nodes.Add(new Node(0, 18.5, 0));
            nodes.Add(new Node(1.5, 18, 0));
            nodes.Add(new Node(4, 18.5, 0));
            nodes.Add(new Node(6.3, 18.2, 0));
            nodes.Add(new Node(8.7, 18.2, 0));
            nodes.Add(new Node(10.3, 18.8, 0));
            nodes.Add(new Node(12, 18.5, 0));
            nodes.Add(new Node(1, 20, 0));
            nodes.Add(new Node(4, 20, 0));
            nodes.Add(new Node(6, 20, 0));
            nodes.Add(new Node(9, 20, 0));
            nodes.Add(new Node(10, 20, 0));
            nodes.Add(new Node(12, 20, 0));
            nodes.Add(new Node(0.5, 0, 0));
            nodes.Add(new Node(0, 1.25, 0));
            nodes.Add(new Node(0.625, 1.125, 0));
            nodes.Add(new Node(1.25, 1, 0));
            nodes.Add(new Node(0.75, 2.25, 0));
            nodes.Add(new Node(2.5, 0, 0));
            nodes.Add(new Node(2.625, 1.125, 0));
            nodes.Add(new Node(4, 1.25, 0));
            nodes.Add(new Node(2.75, 2.25, 0));
            nodes.Add(new Node(5, 0, 0));
            nodes.Add(new Node(5.125, 1.125, 0));
            nodes.Add(new Node(6.25, 1, 0));
            nodes.Add(new Node(5.25, 2.25, 0));
            nodes.Add(new Node(7.5, 0, 0));
            nodes.Add(new Node(7.375, 0.925, 0));
            nodes.Add(new Node(8.5, 0.85, 0));
            nodes.Add(new Node(7.25, 1.85, 0));
            nodes.Add(new Node(9.5, 0, 0));
            nodes.Add(new Node(9.375, 0.95, 0));
            nodes.Add(new Node(10.25, 1.05, 0));
            nodes.Add(new Node(9.25, 1.9, 0));
            nodes.Add(new Node(11, 0, 0));
            nodes.Add(new Node(11.125, 1.15, 0));
            nodes.Add(new Node(12, 1.25, 0));
            nodes.Add(new Node(11.25, 2.3, 0));
            nodes.Add(new Node(0, 3.25, 0));
            nodes.Add(new Node(0.75, 3.2, 0));
            nodes.Add(new Node(1.5, 3.15, 0));
            nodes.Add(new Node(0.75, 4.15, 0));
            nodes.Add(new Node(2.75, 3.2, 0));
            nodes.Add(new Node(4, 3.25, 0));
            nodes.Add(new Node(2.75, 4.15, 0));
            nodes.Add(new Node(5.25, 3.25, 0));
            nodes.Add(new Node(6.5, 3.25, 0));
            nodes.Add(new Node(5.25, 4.25, 0));
            nodes.Add(new Node(7.375, 3.125, 0));
            nodes.Add(new Node(8.25, 3, 0));
            nodes.Add(new Node(7.5, 4.4, 0));
            nodes.Add(new Node(9.375, 3.1, 0));
            nodes.Add(new Node(10.5, 3.2, 0));
            nodes.Add(new Node(9.5, 4.3, 0));
            nodes.Add(new Node(11.25, 3.225, 0));
            nodes.Add(new Node(12, 3.25, 0));
            nodes.Add(new Node(11.25, 4.15, 0));
            nodes.Add(new Node(0, 5.25, 0));
            nodes.Add(new Node(0.75, 5.4, 0));
            nodes.Add(new Node(1.5, 5.55, 0));
            nodes.Add(new Node(0.75, 6.65, 0));
            nodes.Add(new Node(2.625, 5.475, 0));
            nodes.Add(new Node(3.75, 5.4, 0));
            nodes.Add(new Node(2.5, 6.8, 0));
            nodes.Add(new Node(5, 5.45, 0));
            nodes.Add(new Node(6.25, 5.5, 0));
            nodes.Add(new Node(4.75, 6.65, 0));
            nodes.Add(new Node(7.5, 5.45, 0));
            nodes.Add(new Node(8.75, 5.4, 0));
            nodes.Add(new Node(7.5, 6.5, 0));
            nodes.Add(new Node(9.5, 5.4, 0));
            nodes.Add(new Node(10.25, 5.4, 0));
            nodes.Add(new Node(9.5, 6.5, 0));
            nodes.Add(new Node(11.125, 5.325, 0));
            nodes.Add(new Node(12, 5.25, 0));
            nodes.Add(new Node(11, 6.5, 0));
            nodes.Add(new Node(0, 7.25, 0));
            nodes.Add(new Node(0.775, 7.475, 0));
            nodes.Add(new Node(1.55, 7.7, 0));
            nodes.Add(new Node(0.8, 8.3, 0));
            nodes.Add(new Node(2.65, 7.55, 0));
            nodes.Add(new Node(3.75, 7.4, 0));
            nodes.Add(new Node(2.8, 8.3, 0));
            nodes.Add(new Node(5, 7.45, 0));
            nodes.Add(new Node(6.25, 7.5, 0));
            nodes.Add(new Node(5.25, 8.25, 0));
            nodes.Add(new Node(7.625, 7.375, 0));
            nodes.Add(new Node(9, 7.25, 0));
            nodes.Add(new Node(7.75, 8.25, 0));
            nodes.Add(new Node(9.5, 7.25, 0));
            nodes.Add(new Node(10, 7.25, 0));
            nodes.Add(new Node(9.5, 8, 0));
            nodes.Add(new Node(11, 7.25, 0));
            nodes.Add(new Node(12, 7.25, 0));
            nodes.Add(new Node(11, 8, 0));
            nodes.Add(new Node(0, 9, 0));
            nodes.Add(new Node(0.725, 9.225, 0));
            nodes.Add(new Node(1.45, 9.45, 0));
            nodes.Add(new Node(0.65, 10.15, 0));
            nodes.Add(new Node(2.575, 9.375, 0));
            nodes.Add(new Node(3.7, 9.3, 0));
            nodes.Add(new Node(2.35, 10.45, 0));
            nodes.Add(new Node(4.975, 9.275, 0));
            nodes.Add(new Node(6.25, 9.25, 0));
            nodes.Add(new Node(4.7, 10.3, 0));
            nodes.Add(new Node(7.375, 9.275, 0));
            nodes.Add(new Node(8.5, 9.3, 0));
            nodes.Add(new Node(7, 10.3, 0));
            nodes.Add(new Node(9.325, 9, 0));
            nodes.Add(new Node(10.15, 8.7, 0));
            nodes.Add(new Node(9.15, 10, 0));
            nodes.Add(new Node(11.075, 8.85, 0));
            nodes.Add(new Node(12, 9, 0));
            nodes.Add(new Node(11.15, 9.7, 0));
            nodes.Add(new Node(0, 11, 0));
            nodes.Add(new Node(0.725, 11.225, 0));
            nodes.Add(new Node(1.45, 11.45, 0));
            nodes.Add(new Node(0.8, 12.3, 0));
            nodes.Add(new Node(2.575, 11.375, 0));
            nodes.Add(new Node(3.7, 11.3, 0));
            nodes.Add(new Node(2.8, 12.3, 0));
            nodes.Add(new Node(4.85, 11.15, 0));
            nodes.Add(new Node(6, 11, 0));
            nodes.Add(new Node(5, 12, 0));
            nodes.Add(new Node(7.125, 11.025, 0));
            nodes.Add(new Node(8.25, 11.05, 0));
            nodes.Add(new Node(7.25, 11.75, 0));
            nodes.Add(new Node(9.275, 10.95, 0));
            nodes.Add(new Node(10.3, 10.85, 0));
            nodes.Add(new Node(9.4, 11.9, 0));
            nodes.Add(new Node(11.15, 10.925, 0));
            nodes.Add(new Node(12, 11, 0));
            nodes.Add(new Node(11.15, 12.15, 0));
            nodes.Add(new Node(0, 13, 0));
            nodes.Add(new Node(0.65, 13.15, 0));
            nodes.Add(new Node(1.3, 13.3, 0));
            nodes.Add(new Node(0.5, 14, 0));
            nodes.Add(new Node(2.65, 13.15, 0));
            nodes.Add(new Node(4, 13, 0));
            nodes.Add(new Node(2.5, 14, 0));
            nodes.Add(new Node(5.125, 12.875, 0));
            nodes.Add(new Node(6.25, 12.75, 0));
            nodes.Add(new Node(5.25, 13.75, 0));
            nodes.Add(new Node(7.425, 12.675, 0));
            nodes.Add(new Node(8.6, 12.6, 0));
            nodes.Add(new Node(7.6, 13.6, 0));
            nodes.Add(new Node(9.375, 12.875, 0));
            nodes.Add(new Node(10.15, 13.15, 0));
            nodes.Add(new Node(9.35, 13.85, 0));
            nodes.Add(new Node(11.075, 13.075, 0));
            nodes.Add(new Node(12, 13, 0));
            nodes.Add(new Node(11, 14, 0));
            nodes.Add(new Node(0, 15.25, 0));
            nodes.Add(new Node(0.65, 15.1, 0));
            nodes.Add(new Node(1.3, 14.95, 0));
            nodes.Add(new Node(0.8, 16.2, 0));
            nodes.Add(new Node(2.4, 14.95, 0));
            nodes.Add(new Node(3.5, 14.95, 0));
            nodes.Add(new Node(2.3, 15.9, 0));
            nodes.Add(new Node(4.875, 14.975, 0));
            nodes.Add(new Node(6.25, 15, 0));
            nodes.Add(new Node(4.5, 16.2, 0));
            nodes.Add(new Node(7.475, 14.975, 0));
            nodes.Add(new Node(8.7, 14.95, 0));
            nodes.Add(new Node(7.35, 16.35, 0));
            nodes.Add(new Node(9.5, 15.025, 0));
            nodes.Add(new Node(10.3, 15.1, 0));
            nodes.Add(new Node(9.65, 16.2, 0));
            nodes.Add(new Node(11.15, 15.175, 0));
            nodes.Add(new Node(12, 15.25, 0));
            nodes.Add(new Node(11.3, 16.35, 0));
            nodes.Add(new Node(0, 17.5, 0));
            nodes.Add(new Node(0.775, 17.225, 0));
            nodes.Add(new Node(1.55, 16.95, 0));
            nodes.Add(new Node(0.75, 18.25, 0));
            nodes.Add(new Node(2.525, 17.075, 0));
            nodes.Add(new Node(3.5, 17.2, 0));
            nodes.Add(new Node(2.75, 18.25, 0));
            nodes.Add(new Node(4.825, 17.275, 0));
            nodes.Add(new Node(6.15, 17.35, 0));
            nodes.Add(new Node(5.15, 18.35, 0));
            nodes.Add(new Node(7.425, 17.275, 0));
            nodes.Add(new Node(8.7, 17.2, 0));
            nodes.Add(new Node(7.5, 18.2, 0));
            nodes.Add(new Node(9.575, 17.35, 0));
            nodes.Add(new Node(10.45, 17.5, 0));
            nodes.Add(new Node(9.5, 18.5, 0));
            nodes.Add(new Node(11.225, 17.5, 0));
            nodes.Add(new Node(12, 17.5, 0));
            nodes.Add(new Node(11.15, 18.65, 0));
            nodes.Add(new Node(0, 19.25, 0));
            nodes.Add(new Node(0.625, 19.125, 0));
            nodes.Add(new Node(1.25, 19, 0));
            nodes.Add(new Node(0.5, 20, 0));
            nodes.Add(new Node(2.625, 19.125, 0));
            nodes.Add(new Node(4, 19.25, 0));
            nodes.Add(new Node(2.5, 20, 0));
            nodes.Add(new Node(5.075, 19.175, 0));
            nodes.Add(new Node(6.15, 19.1, 0));
            nodes.Add(new Node(5, 20, 0));
            nodes.Add(new Node(7.5, 19.1, 0));
            nodes.Add(new Node(8.85, 19.1, 0));
            nodes.Add(new Node(7.5, 20, 0));
            nodes.Add(new Node(9.5, 19.25, 0));
            nodes.Add(new Node(10.15, 19.4, 0));
            nodes.Add(new Node(9.5, 20, 0));
            nodes.Add(new Node(11.075, 19.325, 0));
            nodes.Add(new Node(12, 19.25, 0));
            nodes.Add(new Node(11, 20, 0));
            #endregion

            List<Tri3TripledLaminatedGlassV2> els = new List<Tri3TripledLaminatedGlassV2>();
            #region plates
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[81], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[86], nodes[85], nodes[11] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[90], nodes[89], nodes[12] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[94], nodes[93], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[98], nodes[97], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[102], nodes[101], nodes[15] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[106], nodes[105], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[109], nodes[108], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[112], nodes[111], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[115], nodes[114], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[118], nodes[117], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[121], nodes[120], nodes[22] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[125], nodes[124], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[128], nodes[127], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[131], nodes[130], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[134], nodes[133], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[137], nodes[136], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[140], nodes[139], nodes[29] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[144], nodes[143], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[147], nodes[146], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[150], nodes[149], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[153], nodes[152], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[156], nodes[155], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[159], nodes[158], nodes[36] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[163], nodes[162], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[166], nodes[165], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[169], nodes[168], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[172], nodes[171], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[175], nodes[174], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[178], nodes[177], nodes[43] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[182], nodes[181], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[185], nodes[184], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[188], nodes[187], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[191], nodes[190], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[194], nodes[193], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[197], nodes[196], nodes[50] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[201], nodes[200], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[204], nodes[203], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[207], nodes[206], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[210], nodes[209], nodes[55] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[213], nodes[212], nodes[56] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[216], nodes[215], nodes[57] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[220], nodes[219], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[223], nodes[222], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[226], nodes[225], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[229], nodes[228], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[232], nodes[231], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[235], nodes[234], nodes[64] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[239], nodes[238], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[242], nodes[241], nodes[67] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[245], nodes[244], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[248], nodes[247], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[251], nodes[250], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[254], nodes[253], nodes[71] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[258], nodes[257], nodes[72] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[261], nodes[260], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[264], nodes[263], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[267], nodes[266], nodes[75] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[270], nodes[269], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[273], nodes[272], nodes[77] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[78], nodes[80] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[80], nodes[3], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[9], nodes[80], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[81], nodes[83], nodes[84] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[4], nodes[85] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[10], nodes[84], nodes[86] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[85], nodes[87], nodes[88] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[88], nodes[5], nodes[89] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[11], nodes[88], nodes[90] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[89], nodes[91], nodes[92] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[92], nodes[6], nodes[93] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[12], nodes[92], nodes[94] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[93], nodes[95], nodes[96] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[96], nodes[7], nodes[97] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[96], nodes[98] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[97], nodes[99], nodes[100] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[100], nodes[8], nodes[101] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[100], nodes[102] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[103], nodes[82], nodes[104] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[104], nodes[10], nodes[105] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[104], nodes[106] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[105], nodes[86], nodes[107] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[107], nodes[11], nodes[108] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[107], nodes[109] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[108], nodes[90], nodes[110] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[110], nodes[12], nodes[111] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[110], nodes[112] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[111], nodes[94], nodes[113] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[113], nodes[13], nodes[114] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[113], nodes[115] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[114], nodes[98], nodes[116] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[116], nodes[14], nodes[117] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[116], nodes[118] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[117], nodes[102], nodes[119] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[119], nodes[15], nodes[120] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[119], nodes[121] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[122], nodes[106], nodes[123] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[123], nodes[17], nodes[124] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[123], nodes[125] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[124], nodes[109], nodes[126] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[126], nodes[18], nodes[127] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[126], nodes[128] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[127], nodes[112], nodes[129] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[129], nodes[19], nodes[130] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[129], nodes[131] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[130], nodes[115], nodes[132] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[132], nodes[20], nodes[133] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[132], nodes[134] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[133], nodes[118], nodes[135] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[135], nodes[21], nodes[136] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[135], nodes[137] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[136], nodes[121], nodes[138] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[138], nodes[22], nodes[139] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[138], nodes[140] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[141], nodes[125], nodes[142] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[142], nodes[24], nodes[143] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[142], nodes[144] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[143], nodes[128], nodes[145] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[145], nodes[25], nodes[146] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[145], nodes[147] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[146], nodes[131], nodes[148] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[148], nodes[26], nodes[149] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[148], nodes[150] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[149], nodes[134], nodes[151] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[151], nodes[27], nodes[152] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[151], nodes[153] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[152], nodes[137], nodes[154] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[154], nodes[28], nodes[155] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[154], nodes[156] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[155], nodes[140], nodes[157] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[157], nodes[29], nodes[158] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[157], nodes[159] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[160], nodes[144], nodes[161] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[161], nodes[31], nodes[162] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[161], nodes[163] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[162], nodes[147], nodes[164] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[164], nodes[32], nodes[165] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[164], nodes[166] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[165], nodes[150], nodes[167] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[167], nodes[33], nodes[168] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[39], nodes[167], nodes[169] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[168], nodes[153], nodes[170] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[170], nodes[34], nodes[171] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[170], nodes[172] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[171], nodes[156], nodes[173] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[173], nodes[35], nodes[174] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[173], nodes[175] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[174], nodes[159], nodes[176] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[176], nodes[36], nodes[177] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[176], nodes[178] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[179], nodes[163], nodes[180] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[180], nodes[38], nodes[181] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[44], nodes[180], nodes[182] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[181], nodes[166], nodes[183] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[183], nodes[39], nodes[184] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[183], nodes[185] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[184], nodes[169], nodes[186] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[186], nodes[40], nodes[187] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[186], nodes[188] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[187], nodes[172], nodes[189] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[189], nodes[41], nodes[190] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[189], nodes[191] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[190], nodes[175], nodes[192] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[192], nodes[42], nodes[193] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[192], nodes[194] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[193], nodes[178], nodes[195] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[195], nodes[43], nodes[196] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[195], nodes[197] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[198], nodes[182], nodes[199] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[199], nodes[45], nodes[200] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[199], nodes[201] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[200], nodes[185], nodes[202] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[202], nodes[46], nodes[203] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[202], nodes[204] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[203], nodes[188], nodes[205] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[205], nodes[47], nodes[206] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[205], nodes[207] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[206], nodes[191], nodes[208] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[208], nodes[48], nodes[209] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[208], nodes[210] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[209], nodes[194], nodes[211] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[211], nodes[49], nodes[212] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[55], nodes[211], nodes[213] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[212], nodes[197], nodes[214] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[214], nodes[50], nodes[215] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[214], nodes[216] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[217], nodes[201], nodes[218] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[218], nodes[52], nodes[219] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[218], nodes[220] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[219], nodes[204], nodes[221] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[221], nodes[53], nodes[222] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[221], nodes[223] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[222], nodes[207], nodes[224] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[224], nodes[54], nodes[225] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[224], nodes[226] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[225], nodes[210], nodes[227] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[227], nodes[55], nodes[228] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[227], nodes[229] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[228], nodes[213], nodes[230] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[230], nodes[56], nodes[231] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[230], nodes[232] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[231], nodes[216], nodes[233] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[233], nodes[57], nodes[234] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[233], nodes[235] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[236], nodes[220], nodes[237] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[237], nodes[59], nodes[238] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[237], nodes[239] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[238], nodes[223], nodes[240] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[240], nodes[60], nodes[241] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[240], nodes[242] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[241], nodes[226], nodes[243] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[243], nodes[61], nodes[244] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[243], nodes[245] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[244], nodes[229], nodes[246] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[246], nodes[62], nodes[247] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[246], nodes[248] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[247], nodes[232], nodes[249] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[249], nodes[63], nodes[250] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[249], nodes[251] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[250], nodes[235], nodes[252] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[252], nodes[64], nodes[253] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[252], nodes[254] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[255], nodes[239], nodes[256] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[256], nodes[66], nodes[257] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[2], nodes[256], nodes[258] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[257], nodes[242], nodes[259] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[259], nodes[67], nodes[260] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[72], nodes[259], nodes[261] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[260], nodes[245], nodes[262] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[262], nodes[68], nodes[263] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[73], nodes[262], nodes[264] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[263], nodes[248], nodes[265] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[265], nodes[69], nodes[266] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[74], nodes[265], nodes[267] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[266], nodes[251], nodes[268] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[268], nodes[70], nodes[269] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[75], nodes[268], nodes[270] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[269], nodes[254], nodes[271] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[271], nodes[71], nodes[272] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[76], nodes[271], nodes[273] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[80], nodes[81], nodes[82] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[84], nodes[85], nodes[86] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[88], nodes[89], nodes[90] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[92], nodes[93], nodes[94] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[96], nodes[97], nodes[98] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[100], nodes[101], nodes[102] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[104], nodes[105], nodes[106] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[107], nodes[108], nodes[109] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[110], nodes[111], nodes[112] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[113], nodes[114], nodes[115] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[116], nodes[117], nodes[118] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[119], nodes[120], nodes[121] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[123], nodes[124], nodes[125] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[126], nodes[127], nodes[128] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[129], nodes[130], nodes[131] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[132], nodes[133], nodes[134] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[135], nodes[136], nodes[137] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[138], nodes[139], nodes[140] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[142], nodes[143], nodes[144] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[145], nodes[146], nodes[147] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[148], nodes[149], nodes[150] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[151], nodes[152], nodes[153] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[154], nodes[155], nodes[156] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[157], nodes[158], nodes[159] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[161], nodes[162], nodes[163] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[164], nodes[165], nodes[166] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[167], nodes[168], nodes[169] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[170], nodes[171], nodes[172] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[173], nodes[174], nodes[175] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[176], nodes[177], nodes[178] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[180], nodes[181], nodes[182] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[183], nodes[184], nodes[185] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[186], nodes[187], nodes[188] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[189], nodes[190], nodes[191] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[192], nodes[193], nodes[194] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[195], nodes[196], nodes[197] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[199], nodes[200], nodes[201] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[202], nodes[203], nodes[204] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[205], nodes[206], nodes[207] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[208], nodes[209], nodes[210] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[211], nodes[212], nodes[213] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[214], nodes[215], nodes[216] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[218], nodes[219], nodes[220] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[221], nodes[222], nodes[223] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[224], nodes[225], nodes[226] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[227], nodes[228], nodes[229] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[230], nodes[231], nodes[232] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[233], nodes[234], nodes[235] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[237], nodes[238], nodes[239] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[240], nodes[241], nodes[242] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[243], nodes[244], nodes[245] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[246], nodes[247], nodes[248] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[249], nodes[250], nodes[251] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[252], nodes[253], nodes[254] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[256], nodes[257], nodes[258] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[259], nodes[260], nodes[261] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[262], nodes[263], nodes[264] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[265], nodes[266], nodes[267] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[268], nodes[269], nodes[270] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[271], nodes[272], nodes[273] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[1], nodes[78], nodes[79] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[78], nodes[3], nodes[80] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[79], nodes[80], nodes[9] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[3], nodes[83], nodes[81] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[83], nodes[4], nodes[84] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[81], nodes[84], nodes[10] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[4], nodes[87], nodes[85] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[87], nodes[5], nodes[88] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[85], nodes[88], nodes[11] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[5], nodes[91], nodes[89] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[91], nodes[6], nodes[92] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[89], nodes[92], nodes[12] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[6], nodes[95], nodes[93] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[95], nodes[7], nodes[96] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[93], nodes[96], nodes[13] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[7], nodes[99], nodes[97] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[99], nodes[8], nodes[100] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[97], nodes[100], nodes[14] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[9], nodes[82], nodes[103] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[82], nodes[10], nodes[104] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[103], nodes[104], nodes[16] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[10], nodes[86], nodes[105] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[86], nodes[11], nodes[107] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[105], nodes[107], nodes[17] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[11], nodes[90], nodes[108] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[90], nodes[12], nodes[110] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[108], nodes[110], nodes[18] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[12], nodes[94], nodes[111] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[94], nodes[13], nodes[113] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[111], nodes[113], nodes[19] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[13], nodes[98], nodes[114] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[98], nodes[14], nodes[116] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[114], nodes[116], nodes[20] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[14], nodes[102], nodes[117] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[102], nodes[15], nodes[119] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[117], nodes[119], nodes[21] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[16], nodes[106], nodes[122] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[106], nodes[17], nodes[123] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[122], nodes[123], nodes[23] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[17], nodes[109], nodes[124] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[109], nodes[18], nodes[126] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[124], nodes[126], nodes[24] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[18], nodes[112], nodes[127] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[112], nodes[19], nodes[129] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[127], nodes[129], nodes[25] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[19], nodes[115], nodes[130] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[115], nodes[20], nodes[132] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[130], nodes[132], nodes[26] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[20], nodes[118], nodes[133] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[118], nodes[21], nodes[135] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[133], nodes[135], nodes[27] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[21], nodes[121], nodes[136] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[121], nodes[22], nodes[138] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[136], nodes[138], nodes[28] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[23], nodes[125], nodes[141] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[125], nodes[24], nodes[142] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[141], nodes[142], nodes[30] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[24], nodes[128], nodes[143] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[128], nodes[25], nodes[145] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[143], nodes[145], nodes[31] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[25], nodes[131], nodes[146] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[131], nodes[26], nodes[148] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[146], nodes[148], nodes[32] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[26], nodes[134], nodes[149] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[134], nodes[27], nodes[151] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[149], nodes[151], nodes[33] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[27], nodes[137], nodes[152] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[137], nodes[28], nodes[154] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[152], nodes[154], nodes[34] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[28], nodes[140], nodes[155] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[140], nodes[29], nodes[157] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[155], nodes[157], nodes[35] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[30], nodes[144], nodes[160] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[144], nodes[31], nodes[161] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[160], nodes[161], nodes[37] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[31], nodes[147], nodes[162] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[147], nodes[32], nodes[164] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[162], nodes[164], nodes[38] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[32], nodes[150], nodes[165] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[150], nodes[33], nodes[167] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[165], nodes[167], nodes[39] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[33], nodes[153], nodes[168] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[153], nodes[34], nodes[170] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[168], nodes[170], nodes[40] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[34], nodes[156], nodes[171] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[156], nodes[35], nodes[173] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[171], nodes[173], nodes[41] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[35], nodes[159], nodes[174] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[159], nodes[36], nodes[176] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[174], nodes[176], nodes[42] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[37], nodes[163], nodes[179] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[163], nodes[38], nodes[180] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[179], nodes[180], nodes[44] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[38], nodes[166], nodes[181] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[166], nodes[39], nodes[183] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[181], nodes[183], nodes[45] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[39], nodes[169], nodes[184] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[169], nodes[40], nodes[186] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[184], nodes[186], nodes[46] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[40], nodes[172], nodes[187] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[172], nodes[41], nodes[189] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[187], nodes[189], nodes[47] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[41], nodes[175], nodes[190] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[175], nodes[42], nodes[192] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[190], nodes[192], nodes[48] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[42], nodes[178], nodes[193] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[178], nodes[43], nodes[195] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[193], nodes[195], nodes[49] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[44], nodes[182], nodes[198] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[182], nodes[45], nodes[199] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[198], nodes[199], nodes[51] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[45], nodes[185], nodes[200] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[185], nodes[46], nodes[202] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[200], nodes[202], nodes[52] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[46], nodes[188], nodes[203] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[188], nodes[47], nodes[205] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[203], nodes[205], nodes[53] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[47], nodes[191], nodes[206] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[191], nodes[48], nodes[208] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[206], nodes[208], nodes[54] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[48], nodes[194], nodes[209] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[194], nodes[49], nodes[211] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[209], nodes[211], nodes[55] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[49], nodes[197], nodes[212] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[197], nodes[50], nodes[214] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[212], nodes[214], nodes[56] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[51], nodes[201], nodes[217] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[201], nodes[52], nodes[218] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[217], nodes[218], nodes[58] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[52], nodes[204], nodes[219] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[204], nodes[53], nodes[221] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[219], nodes[221], nodes[59] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[53], nodes[207], nodes[222] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[207], nodes[54], nodes[224] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[222], nodes[224], nodes[60] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[54], nodes[210], nodes[225] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[210], nodes[55], nodes[227] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[225], nodes[227], nodes[61] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[55], nodes[213], nodes[228] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[213], nodes[56], nodes[230] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[228], nodes[230], nodes[62] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[56], nodes[216], nodes[231] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[216], nodes[57], nodes[233] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[231], nodes[233], nodes[63] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[58], nodes[220], nodes[236] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[220], nodes[59], nodes[237] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[236], nodes[237], nodes[65] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[59], nodes[223], nodes[238] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[223], nodes[60], nodes[240] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[238], nodes[240], nodes[66] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[60], nodes[226], nodes[241] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[226], nodes[61], nodes[243] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[241], nodes[243], nodes[67] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[61], nodes[229], nodes[244] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[229], nodes[62], nodes[246] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[244], nodes[246], nodes[68] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[62], nodes[232], nodes[247] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[232], nodes[63], nodes[249] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[247], nodes[249], nodes[69] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[63], nodes[235], nodes[250] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[235], nodes[64], nodes[252] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[250], nodes[252], nodes[70] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[65], nodes[239], nodes[255] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[239], nodes[66], nodes[256] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[255], nodes[256], nodes[2] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[66], nodes[242], nodes[257] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[242], nodes[67], nodes[259] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[257], nodes[259], nodes[72] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[67], nodes[245], nodes[260] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[245], nodes[68], nodes[262] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[260], nodes[262], nodes[73] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[68], nodes[248], nodes[263] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[248], nodes[69], nodes[265] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[263], nodes[265], nodes[74] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[69], nodes[251], nodes[266] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[251], nodes[70], nodes[268] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[266], nodes[268], nodes[75] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[70], nodes[254], nodes[269] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[254], nodes[71], nodes[271] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            els.Add(new Tri3TripledLaminatedGlassV2(new Node[] { nodes[269], nodes[271], nodes[76] }, G0, hInterlayer, hGlass1, hGlass2, EGlass, niGlass));
            #endregion

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0, 100, 0, 0, 0);
            nodes.Where(x => x.Position.X == 6.0 && x.Position.Y == 10.0).ToList().ForEach(x => x.AddAttribute(f));

            NodeRestrainAttribute dz = new NodeRestrainAttribute("freedomCase", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);

            nodes[1].AddAttribute(dz);
            nodes[8].AddAttribute(dz);
            nodes[77].AddAttribute(dz);
            nodes[2].AddAttribute(dz);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.RZ);
            fix.AddExternalRestrain(Solver.DOF.DDZ);

            nodes.ForEach(x => x.AddAttribute(fix));

            LinearSolver fem = new LinearSolver(els.ToArray());

            double minDZ = 1e6;
            double maxDZ = -1e6;
            for (int i = 1; i < nodes.Count; i++)
            {
                minDZ = Math.Min(minDZ, fem.GetNodeDisplacementGlobalCoordinates(nodes[i], Solver.DOF.DZ));
                maxDZ = Math.Max(maxDZ, fem.GetNodeDisplacementGlobalCoordinates(nodes[i], Solver.DOF.DZ));
            }

            Console.WriteLine("min dz = " + minDZ);
            Console.WriteLine("max dz = " + maxDZ);

            Node center = nodes.Where(x => x.Position.X == 6 && x.Position.Y == 10).First();
            var femDZCenter = fem.GetNodeDisplacementGlobalCoordinates(center, Solver.DOF.DZ);
            Assert.AreEqual(1.0, 14.389 / femDZCenter, 0.01);

            Node angle = nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).First();
            var femAngle = fem.GetNodeDisplacementGlobalCoordinates(angle);
            Assert.AreEqual(-0.135, femAngle[Solver.DOF.DDX], 0.05);
            Assert.AreEqual(-0.502, femAngle[Solver.DOF.DDY], 0.05);
            Assert.AreEqual(2.184, femAngle[Solver.DOF.RX], 0.05);
            Assert.AreEqual(-0.752, femAngle[Solver.DOF.RY], 0.05);

            Node centerBorder1 = nodes.Where(x => x.Position.X == 0 && x.Position.Y == 10).First();
            var femCenterBorder1 = fem.GetNodeDisplacementGlobalCoordinates(centerBorder1);
            Assert.AreEqual(13.354, femCenterBorder1[Solver.DOF.DZ], 0.05);
            Assert.AreEqual(-0.007, femCenterBorder1[Solver.DOF.DDX], 0.05);
            Assert.AreEqual(0.000, femCenterBorder1[Solver.DOF.DDY], 0.05);
            Assert.AreEqual(0.001, femCenterBorder1[Solver.DOF.RX], 0.05);
            Assert.AreEqual(-0.035, femCenterBorder1[Solver.DOF.RY], 0.05);

            Node centerBorder2 = nodes.Where(x => x.Position.X == 6 && x.Position.Y == 0).First();
            var femCenterBorder2 = fem.GetNodeDisplacementGlobalCoordinates(centerBorder2);
            Assert.AreEqual(2.376, femCenterBorder2[Solver.DOF.DZ], 0.05);
            Assert.AreEqual(0.000, femCenterBorder2[Solver.DOF.DDX], 0.05);
            Assert.AreEqual(-0.417, femCenterBorder2[Solver.DOF.DDY], 0.05);
            Assert.AreEqual(1.619, femCenterBorder2[Solver.DOF.RX], 0.05);
            Assert.AreEqual(0.000, femCenterBorder2[Solver.DOF.RY], 0.05);
        }
    }
}