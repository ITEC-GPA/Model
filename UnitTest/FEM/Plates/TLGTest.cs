using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM.FiniteElements;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.Materials;
using GPC.Model.FreedomCases;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using GPC.Model.LoadCases;
using System.Collections.Generic;
using GPC.Model.FEM;

namespace FemTest.Solver
{
    [TestClass]
    public class TripledLaminatedGlassTest1
    {
        [TestMethod]
        public void TestSpatial1()
        {
            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(-1.0, -1.0, 0, "1");
            nodesPlate1[1] = new Node(1.0, -1.0, 0, "2");
            nodesPlate1[2] = new Node(1.0, 1.0, 0, "3");
            nodesPlate1[3] = new Node(-1.0, 1.0, 0, "4");

            //general values §6 of article
            double G0 = 0.5173;
            double h0 = 0.38;
            double h1 = 2.875;
            double h2 = 2.875;
            double EGlass = 72000.0;
            double niGlass = 0.23;
            FiniteElement e0 = new Quad4TripleLaminatedGlass(nodesPlate1, G0, h0, h1, h2, EGlass, niGlass);
            e0.BuildMatrix();

            Node[] nodesPlate2 = new Node[4];
            nodesPlate2[0] = new Node(0.0, 0.0, 0, "1");
            nodesPlate2[1] = new Node(2.0, 0.0, 0, "2");
            nodesPlate2[2] = new Node(2.0, 2.0, 0, "3");
            nodesPlate2[3] = new Node(0.0, 2.0, 0, "4");

            FiniteElement e1 = new Quad4TripleLaminatedGlass(nodesPlate2, G0, h0, h1, h2, EGlass, niGlass);
            e1.BuildMatrix();

            for (int i = 0; i < e1.KElementGlobalCoord.RowCount; i++)
            {
                for (int j = 0; j < e1.KElementGlobalCoord.RowCount; j++)
                {
                    Assert.AreEqual(e0.KElementGlobalCoord[i,j], e1.KElementGlobalCoord[i, j]);
                }
            }
        }

        [TestMethod]
        public void EquivalentNodalForce()
        {
            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(0.0, 0.0, 0, "1");
            nodesPlate1[1] = new Node(2.0, 0.0, 0, "2");
            nodesPlate1[2] = new Node(2.0, 2.0, 0, "3");
            nodesPlate1[3] = new Node(0.0, 2.0, 0, "4");

            //general values §6 of article
            double G0 = 0.5173;
            double h0 = 0.38;
            double h1 = 2.875;
            double h2 = 2.875;
            double EGlass = 72000.0;
            double niGlass = 0.23;
            Quad4TripleLaminatedGlass e0 = new Quad4TripleLaminatedGlass(nodesPlate1, G0, h0, h1, h2, EGlass, niGlass);

            LoadCase lc = new LoadCase("lc");
            CoordinateSystem csys = new CoordinateSystem(new Vector3d(0, 0, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));
            PlatePressureAttribute p = new PlatePressureAttribute(lc, csys, 0, 0, 1);

            e0.AddLoadCaseAttribute(p);

            LinearSolver solver = new LinearSolver(new FiniteElement[] { e0 });
        }

        [TestMethod]
        public void TestDg()
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(0.0, 0.0, 0));
            nodes.Add(new Node(1.0, 0.0, 0));
            nodes.Add(new Node(1.0, 1.0, 0));
            nodes.Add(new Node(0.0, 1.0, 0));

            double h1 = 0.5;
            double h2 = 0.5;
            double EGlass = 12.0;
            double niGlass = 0.0;
            double G0 = EGlass / (2.0 * (1.0 + niGlass));
            double h0 = 0.00001;

            Material mat = new SteelMaterial("mat", EGlass, niGlass, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, h1 + h2 + h0, h1 + h2 + h0, "p");

            Quad4TripleLaminatedGlass e0 = new Quad4TripleLaminatedGlass(new Node[] { nodes[0], nodes[1], nodes[2], nodes[3] }, G0, h0, h1, h2, EGlass, niGlass);
            e0.BuildMatrix();

            mnl.Matrix<double> dgManual = mnl.Matrix<double>.Build.Dense(6, 6);
            dgManual[0, 0] = 3.0;
            dgManual[1, 1] = 3.0;
            dgManual[2, 2] = 1.5;

            dgManual[3, 3] = 0.25;
            dgManual[4, 4] = 0.25;
            dgManual[5, 5] = 0.125;

            for (int r = 0; r < e0.Dg.RowCount; r++)
            {
                for (int c = 0; c < e0.Dg.ColumnCount; c++)
                {
                    Assert.AreEqual(dgManual[r,c], e0.Dg[r,c], 0.001);
                }
            }
        }

        [TestMethod]
        public void TestDs()
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(0.0, 0.0, 0));
            nodes.Add(new Node(1.0, 0.0, 0));
            nodes.Add(new Node(1.0, 1.0, 0));
            nodes.Add(new Node(0.0, 1.0, 0));

            double h1 = 0.5;
            double h2 = 0.5;
            double EGlass = 12.0;
            double niGlass = 0.0;
            double G0 = EGlass / (2.0 * (1.0 + niGlass));
            double h0 = 0.1;

            Material mat = new SteelMaterial("mat", EGlass, niGlass, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, h1 + h2 + h0, h1 + h2 + h0, "p");

            Quad4TripleLaminatedGlass e0 = new Quad4TripleLaminatedGlass(new Node[] { nodes[0], nodes[1], nodes[2], nodes[3] }, G0, h0, h1, h2, EGlass, niGlass);
            e0.BuildMatrix();

            mnl.Matrix<double> dsManual = mnl.Matrix<double>.Build.Dense(4, 4);
            dsManual[0, 0] = 60;
            dsManual[0, 2] = 36;

            dsManual[1, 1] = 60;
            dsManual[1, 3] = 36;

            dsManual[2, 0] = 36;
            dsManual[2, 2] = 21.6;

            dsManual[3, 1] = 36;
            dsManual[3, 3] = 21.6;
            
            for (int r = 0; r < e0.Ds.RowCount; r++)
            {
                for (int c = 0; c < e0.Ds.ColumnCount; c++)
                {
                    Assert.AreEqual(dsManual[r, c], e0.Ds[r, c], 0.001);
                }
            }
        }

        [TestMethod]
        public void TestN()
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(0.0, 0.0, 0));
            nodes.Add(new Node(2.0, 0.0, 0));
            nodes.Add(new Node(2.0, 2.0, 0));
            nodes.Add(new Node(0.0, 2.0, 0));

            double h1 = 0.5;
            double h2 = 0.5;
            double EGlass = 12.0;
            double niGlass = 0.0;
            double G0 = EGlass / (2.0 * (1.0 + niGlass));
            double h0 = 0.1;

            Material mat = new SteelMaterial("mat", EGlass, niGlass, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, h1 + h2 + h0, h1 + h2 + h0, "p");

            Quad4TripleLaminatedGlass e0 = new Quad4TripleLaminatedGlass(new Node[] { nodes[0], nodes[1], nodes[2], nodes[3] }, G0, h0, h1, h2, EGlass, niGlass);

            e0.BuildMatrix();
            FEMUtilities.WriteMatrix(e0.GetNMatrix(0, 0));
            FEMUtilities.WriteMatrix(e0.GetNMatrix(2, 0));
            FEMUtilities.WriteMatrix(e0.GetNMatrix(2, 2));
            FEMUtilities.WriteMatrix(e0.GetNMatrix(0, 2));
            /*for (int r = 0; r < e0.Ds.RowCount; r++)
            {
                for (int c = 0; c < e0.Ds.ColumnCount; c++)
                {
                    Assert.AreEqual(dsManual[r, c], e0.Ds[r, c], 0.001);
                }
            }*/
        }

        [TestMethod]
        public void Test1()
        {
            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(-1.0, -1.0, 0));
            nodes.Add(new Node(1.0, -1.0, 0));
            nodes.Add(new Node(1.0, 1.0, 0));
            nodes.Add(new Node(-1.0, 1.0, 0));

            double h1 = 0.5;
            double h2 = 0.5;
            double EGlass = 12.0;
            double niGlass = 0.2;
            double G0 = EGlass / (2.0 * (1.0 + niGlass));
            double h0 = 0.1;

            Material mat = new SteelMaterial("mat", EGlass, niGlass, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, h1 + h2 + h0, h1 + h2 + h0, "p");

            FiniteElement e0 = new Quad4TripleLaminatedGlass(new Node[] { nodes[0], nodes[1], nodes[2], nodes[3] }, G0, h0, h1, h2, EGlass, niGlass);
            FiniteElement e1 = new Quad4Element(new Node[] { nodes[0], nodes[1], nodes[2], nodes[3] }, prop);

            LoadCase loadCase = new LoadCase("myLoadCase");
            FreedomCase freedomCase = new FreedomCase("freedomCase1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeForceAttribute fNode = new NodeForceAttribute(loadCase, sys, 0, 0, 1.0, 0, 0, 0);

            nodes[2].AddAttribute(fNode);
            nodes[3].AddAttribute(fNode);

            NodeRestrainAttribute fix = new NodeRestrainAttribute(freedomCase, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nodes[0].AddAttribute(fix);
            nodes[1].AddAttribute(fix);

            LinearSolver fem0 = new LinearSolver(new FiniteElement[] { e0 });
            LinearSolver fem1 = new LinearSolver(new FiniteElement[] { e1 });

            Console.WriteLine("Tripled = " + fem0.GetDisplacementGlobalCoordinates(nodes[2], GPC.Model.FEM.Solver.DOF.DZ));
            Console.WriteLine("Kirchoff = " + fem1.GetDisplacementGlobalCoordinates(nodes[2], GPC.Model.FEM.Solver.DOF.DZ));
        }

        [TestMethod]
        public void SimplySupportedTest1()
        {
            double F = 8.0 * 9.81; //N
            double EGlass = 72.0 * 1000; //MPa
            double L = 660; //mm
            double b = 200; //mm
            double hGlass = 2.875; //mm
            double hInterlayer = 0.38; //mm
            double hTot = 6.13; //mm
            double rhoEquivalent = 2.418 / 1000.0 / 1000.0; //kg/mm3
            double V = L * b * hTot; //mm3
            double Qtot = rhoEquivalent * V * 9.81; //N
            double q = Qtot / L; //N/mm

            double JSectionSolid = 1.0 / 12.0 * b * Math.Pow(hTot, 3.0);
            double JSection2Area = 2.0 * 1.0 / 12.0 * b * Math.Pow(hGlass, 3.0) + 2.0 * b * hGlass * Math.Pow((hGlass - hInterlayer) / 2.0, 2.0);

            double fLowerBound = F * (L * L * L) / (48.0 * EGlass * JSectionSolid) + 5.0 / 384.0 * q * Math.Pow(L, 4.0) / (EGlass * JSectionSolid);
            double fMaxBound = F * (L * L * L) / (48.0 * EGlass * JSection2Area) + 5.0 / 384.0 * q * Math.Pow(L, 4.0) / (EGlass * JSection2Area);

            Console.WriteLine(fLowerBound);
            Console.WriteLine(fMaxBound);

            double G0 = 0.5173;
            double h0 = 0.38;
            double h1 = 2.875;
            double h2 = 2.875;
            double niGlass = 0.23;

            List<Node> nodes = new List<Node>();
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

            List<Quad4TripleLaminatedGlass> els = new List<Quad4TripleLaminatedGlass>();
            els.Add(new Quad4TripleLaminatedGlass(new Node[] { nodes[7], nodes[8], nodes[3], nodes[9] }, G0, h0, h1, h2, EGlass, niGlass));
            els.Add(new Quad4TripleLaminatedGlass(new Node[] { nodes[1], nodes[5], nodes[7], nodes[6] }, G0, h0, h1, h2, EGlass, niGlass));
            els.Add(new Quad4TripleLaminatedGlass(new Node[] { nodes[5], nodes[2], nodes[8], nodes[7] }, G0, h0, h1, h2, EGlass, niGlass));
            els.Add(new Quad4TripleLaminatedGlass(new Node[] { nodes[6], nodes[7], nodes[9], nodes[4] }, G0, h0, h1, h2, EGlass, niGlass));
            els.Add(new Quad4TripleLaminatedGlass(new Node[] { nodes[3], nodes[9], nodes[11], nodes[10] }, G0, h0, h1, h2, EGlass, niGlass));
            els.Add(new Quad4TripleLaminatedGlass(new Node[] { nodes[9], nodes[4], nodes[12], nodes[11] }, G0, h0, h1, h2, EGlass, niGlass));
            els.Add(new Quad4TripleLaminatedGlass(new Node[] { nodes[2], nodes[5], nodes[14], nodes[13] }, G0, h0, h1, h2, EGlass, niGlass));
            els.Add(new Quad4TripleLaminatedGlass(new Node[] { nodes[5], nodes[1], nodes[15], nodes[14] }, G0, h0, h1, h2, EGlass, niGlass));

            LoadCase loadCase = new LoadCase("myLoadCase");
            FreedomCase freedomCase = new FreedomCase("freedomCase1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            PlatePressureAttribute pressure = new PlatePressureAttribute(loadCase, sys, 0.0, 0.0, (2.418 / 1000.0 * 9.81) * (hTot / 1000.0));
            els.ForEach(x => x.AddLoadCaseAttribute(pressure));

            NodeForceAttribute fNode = new NodeForceAttribute(loadCase, sys, 0, 0, F / 2.0 / 3.0, 0, 0, 0);

            nodes.Where(x => x.Position.X == 15).ToList().ForEach(x => x.AddAttribute(fNode));

            NodeRestrainAttribute x0Restrain = new NodeRestrainAttribute(freedomCase, sys);
            x0Restrain.AddExternalRestrain(LinearSolver.DOF.DX);
            x0Restrain.AddExternalRestrain(LinearSolver.DOF.RY);
            nodes.Where(x => x.Position.X == 0).ToList().ForEach(x => x.AddAttribute(x0Restrain));

            NodeRestrainAttribute x0y0Restrain = new NodeRestrainAttribute(freedomCase, sys);
            x0y0Restrain.AddExternalRestrain(LinearSolver.DOF.DX);
            x0y0Restrain.AddExternalRestrain(LinearSolver.DOF.DY);
            x0y0Restrain.AddExternalRestrain(LinearSolver.DOF.RY);
            nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).ToList().ForEach(x => x.AddAttribute(x0y0Restrain));

            NodeRestrainAttribute x330y0Restrain = new NodeRestrainAttribute(freedomCase, sys);
            x330y0Restrain.AddExternalRestrain(LinearSolver.DOF.DY);
            x330y0Restrain.AddExternalRestrain(LinearSolver.DOF.DZ);
            nodes.Where(x => x.Position.X == 330 && x.Position.Y == 0).ToList().ForEach(x => x.AddAttribute(x330y0Restrain));

            NodeRestrainAttribute x330Restrain = new NodeRestrainAttribute(freedomCase, sys);
            x330Restrain.AddExternalRestrain(LinearSolver.DOF.DZ);
            nodes.Where(x => x.Position.X == 330).ToList().ForEach(x => x.AddAttribute(x330Restrain));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(1.7, fem.GetDisplacementGlobalCoordinates(nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).First(), LinearSolver.DOF.DZ), 0.001);
        }
    }
}