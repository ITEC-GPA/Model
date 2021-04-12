using GPC.Geometry;
using GPC.Model.FEM;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Materials;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace FemTest.Solver { 
    [TestClass]
    public class BeamTest {

        [TestMethod]
        public void Axial1()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            Beam b = new Beam(nds.ToArray(), sec);

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 1000;
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, FX, 0.0, 0.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(FX / (Math.PI * 100.0*100.0 / 4.0 * E) * L, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX), 1e-6);
        }

        [TestMethod]
        public void BendingTest1()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            Beam b = new Beam(nds.ToArray(), sec);

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 0.0;
            double MZ = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, FX, 0.0, 0.0, 0.0, 0.0, MZ);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(10.1859, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-4);
        }

        [TestMethod]
        public void BendingTest2()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, -L, 0));

            Beam b = new Beam(nds.ToArray(), sec);

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 0.0, 0.0, 0.0, -1000.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(10.1859, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DZ), 1e-4);
        }

        [TestMethod]
        public void ShearForceTest1()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 10.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(1000, 0, 0));

            Beam b = new Beam(nds.ToArray(), sec);

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 0.0, 10.0, 0.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(67.9061, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-4);
        }

        [TestMethod]
        public void ShearForceTest2()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 10.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, 0, 1000.0));

            Beam b = new Beam(nds.ToArray(), sec);

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 10.0, 10.0, 0.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(67.9061, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX), 1e-4);
            Assert.AreEqual(67.9061, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-4);
        }

        [TestMethod]
        public void ShearForceTest3()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 10.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, 1000.0, 0));

            Beam b = new Beam(nds.ToArray(), sec);

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 10.0, 0.0, 10.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(67.9061, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX), 1e-4);
            Assert.AreEqual(67.9061, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DZ), 1e-4);
        }

        [TestMethod]
        public void TorsionTest1()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 10.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(1000, 0, 0));

            Beam b = new Beam(nds.ToArray(), sec);

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double MX = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 0.0, 0.0, 0.0, MX, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(1.1672, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.RX) * 180.0 / Math.PI, 1e-4);
        }

        [TestMethod]
        public void SimplySupportedTest1()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 1.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(1000, 0, 0));
            nds.Add(new Node(2000, 0, 0));

            List<Beam> beams = new List<Beam>();
            beams.Add(new Beam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new Beam(new Node[] { nds[1], nds[2] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = 1.0;
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 0.0, -F, 0.0, 0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute support = new NodeRestrainAttribute(fc, sys);
            support.AddExternalRestrain(LinearSolver.DOF.DX);
            support.AddExternalRestrain(LinearSolver.DOF.DY);
            support.AddExternalRestrain(LinearSolver.DOF.DZ);
            support.AddExternalRestrain(LinearSolver.DOF.RX);

            nds[0].AddAttribute(support);
            nds[2].AddAttribute(support);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(-33.9531, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-4);
        }
    }
}
