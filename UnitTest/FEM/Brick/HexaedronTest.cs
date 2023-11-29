using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.Fem.FiniteElements;
using GPC.Model.Fem.Properties;
using GPC.Model.Materials;
using GPC.Model.Fem.Attributes;
using GPC.Model.LoadCases;
using GPC.Geometry;
using GPC.Model.FreedomCases;

namespace FemTest.SolverTest
{
    [TestClass]
    public class Hexaedron8Test
    {
        [TestMethod]
        public void KMatrixTest1()
        {
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0.0));
            nds.Add(new Node(2.0, 0.0, 0.0));
            nds.Add(new Node(2.0, 2.0, 0.0));
            nds.Add(new Node(0.0, 2.0, 0.0));
            nds.Add(new Node(0.0, 0.0, 2.0));
            nds.Add(new Node(2.0, 0.0, 2.0));
            nds.Add(new Node(2.0, 2.0, 2.0));
            nds.Add(new Node(0.0, 2.0, 2.0));

            SteelMaterial mat = new SteelMaterial("mat", 1.0, 0.0, 355, 510);

            BrickProperty brickProperty = new BrickProperty(mat.GetIsotropicFemMaterial(), "propBrick");

            Hexaedron e = new Hexaedron(nds.ToArray(), brickProperty);
            e.BuildMatrix();

            Console.WriteLine("klocalMatrix");
            FemUtilities.WriteMatrix(e.KElementLocalCoord);

            //Local axis == global axis
            for (int r = 0; r < 12; r++)
            {
                for (int c = 0; c < 12; c++)
                {
                    Assert.AreEqual(e.KElementLocalCoord[r, c], e.KElementGlobalCoord[r, c], 0.00000001);
                }
            }
        }

        [TestMethod]
        public void KMatrixTest2()
        {
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0.0));
            nds.Add(new Node(1.0, 0.0, 0.0));
            nds.Add(new Node(1.0, 1.0, 0.0));
            nds.Add(new Node(0.0, 1.0, 0.0));
            nds.Add(new Node(0.0, 0.0, 1.0));
            nds.Add(new Node(1.0, 0.0, 1.0));
            nds.Add(new Node(1.0, 1.0, 1.0));
            nds.Add(new Node(0.0, 1.0, 1.0));

            SteelMaterial mat = new SteelMaterial("mat", 1.0, 0.0, 355, 510);

            BrickProperty brickProperty = new BrickProperty(mat.GetIsotropicFemMaterial(), "proprBrick");

            Hexaedron e = new Hexaedron(nds.ToArray(), brickProperty);

            LoadCaseBase lc = new LoadCaseBase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.25, 0, 0.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);
            nds[2].AddAttribute(f);
            nds[5].AddAttribute(f);
            nds[6].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);
            nds[4].AddAttribute(fix);
            nds[7].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { e });

            Assert.AreEqual(1.0, fem.GetNodeDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX), 0.000001);
            Assert.AreEqual(1.0, fem.GetNodeDisplacementGlobalCoordinates(nds[2], LinearSolver.DOF.DX), 0.000001);
            Assert.AreEqual(1.0, fem.GetNodeDisplacementGlobalCoordinates(nds[6], LinearSolver.DOF.DX), 0.000001);
            Assert.AreEqual(1.0, fem.GetNodeDisplacementGlobalCoordinates(nds[6], LinearSolver.DOF.DX), 0.000001);
        }

        [TestMethod]
        public void CantileverTest1()
        {
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0.0));
            nds.Add(new Node(3.0, 0.0, 0.0));
            nds.Add(new Node(3.0, 1.0, 0.0));
            nds.Add(new Node(0.0, 1.0, 0.0));

            nds.Add(new Node(0.0, 0.0, 1.0));
            nds.Add(new Node(3.0, 0.0, 1.0));
            nds.Add(new Node(3.0, 1.0, 1.0));
            nds.Add(new Node(0.0, 1.0, 1.0));

            SteelMaterial mat = new SteelMaterial("mat", 1.0, 0.0, 355, 510);

            BrickProperty brickProperty = new BrickProperty(mat.GetIsotropicFemMaterial(), "proprBrick");

            Hexaedron e = new Hexaedron(nds.ToArray(), brickProperty);

            LoadCaseBase lc = new LoadCaseBase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, 0.0, -0.25, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);
            nds[2].AddAttribute(f);
            nds[5].AddAttribute(f);
            nds[6].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);
            nds[4].AddAttribute(fix);
            nds[7].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { e });

            Console.WriteLine(fem.GetNodeDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DZ));
        }

        [TestMethod]
        public void CantileverTest2()
        {
            double L = 3.0;
            int nrEl = 35;
            List<Node> nds = new List<Node>();
            for (int nr = 0; nr <= nrEl; nr++)
            {
                nds.Add(new Node((1.0 * nr / nrEl) * L, 0.0, 0.0));
                nds.Add(new Node((1.0 * nr / nrEl) * L, 1.0, 0.0));
                nds.Add(new Node((1.0 * nr / nrEl) * L, 1.0, 1.0));
                nds.Add(new Node((1.0 * nr / nrEl) * L, 0.0, 1.0));                                
            }

            SteelMaterial mat = new SteelMaterial("mat", 1000.0, 0.0, 355, 510);

            BrickProperty brickProperty = new BrickProperty(mat.GetIsotropicFemMaterial(), "proprBrick");

            List<Hexaedron> els = new List<Hexaedron>();
            for (int i = 4; i < nds.Count; i=i+4)
            {
                List<Node> elementNodes = new List<Node>();
                elementNodes.Add(nds[i - 4]);
                elementNodes.Add(nds[i - 3]);
                elementNodes.Add(nds[i - 2]);
                elementNodes.Add(nds[i - 1]);
                elementNodes.Add(nds[i + 0]);
                elementNodes.Add(nds[i + 1]);
                elementNodes.Add(nds[i + 2]);
                elementNodes.Add(nds[i + 3]);

                els.Add(new Hexaedron(elementNodes.ToArray(), brickProperty));
            }

            LoadCaseBase lc = new LoadCaseBase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, 0.0, -0.25, 0.0, 0.0, 0.0);

            nds[nds.Count - 1].AddAttribute(f);
            nds[nds.Count - 2].AddAttribute(f);
            nds[nds.Count - 3].AddAttribute(f);
            nds[nds.Count - 4].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);

            nds[0].AddAttribute(fix);
            nds[1].AddAttribute(fix);
            nds[2].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(els.ToArray()) ;

            Assert.AreEqual(1.0, -0.1152 / fem.GetNodeDisplacementGlobalCoordinates(nds[nds.Count-1], LinearSolver.DOF.DZ), 0.015);
        }
    }
}
