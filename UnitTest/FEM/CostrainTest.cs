using GPC.Geometry;
using GPC.Model.FEM;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Costrain;
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
    public class CostrainTest {

        [TestMethod]
        public void Axial1()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(300.0, 0, 0));
            nds.Add(new Node(700.0, 0, 0));
            nds.Add(new Node(1000.0, 0, 0));

            List<Beam> els = new List<Beam>();
            els.Add(new Beam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new Beam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 1000;
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, FX, 0.0, 0.0, 0.0, 0.0, 0.0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute(fc, sys);
            fix2.AddExternalRestrain(LinearSolver.DOF.DY);
            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
            fix2.AddExternalRestrain(LinearSolver.DOF.RY);
            fix2.AddExternalRestrain(LinearSolver.DOF.RZ);
            nds.ForEach(x => x.AddAttribute(fix2));

            HashSet<MultiPointCostrain.Link> eqtnsDX = new HashSet<MultiPointCostrain.Link>();
            eqtnsDX.Add(new MultiPointCostrain.Link(nds[2], LinearSolver.DOF.DX, -1.0));
            eqtnsDX.Add(new MultiPointCostrain.Link(nds[1], LinearSolver.DOF.DX, 1.0));

            HashSet<MultiPointCostrain> links = new HashSet<MultiPointCostrain>();
            links.Add(new MultiPointCostrain(eqtnsDX.ToArray(), 0.0));           

            LinearSolver fem = new LinearSolver(els.ToArray(), links.ToArray());

            Assert.AreEqual(FX / (Math.PI * 100.0*100.0 / 4.0 * E) * (1000-400), fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-6);
        }

        [TestMethod]
        public void BendingTest1()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(300.0, 0, 0));
            nds.Add(new Node(700.0, 0, 0));
            nds.Add(new Node(1000.0, 0, 0));

            List<Beam> els = new List<Beam>();
            els.Add(new Beam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new Beam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double MZ = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 0.0, 0.0, 0.0, 0.0, 0.0, MZ);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute(fc, sys);
            fix2.AddExternalRestrain(LinearSolver.DOF.DX);
            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
            fix2.AddExternalRestrain(LinearSolver.DOF.RY);
            nds.ForEach(x => x.AddAttribute(fix2));

            HashSet<MultiPointCostrain.Link> eqtnsDY = new HashSet<MultiPointCostrain.Link>();
            eqtnsDY.Add(new MultiPointCostrain.Link(nds[2], LinearSolver.DOF.DY, -1.0));
            eqtnsDY.Add(new MultiPointCostrain.Link(nds[1], LinearSolver.DOF.DY, 1.0));
            eqtnsDY.Add(new MultiPointCostrain.Link(nds[1], LinearSolver.DOF.RZ, 400));

            HashSet<MultiPointCostrain.Link> eqtnsRZ = new HashSet<MultiPointCostrain.Link>();
            eqtnsRZ.Add(new MultiPointCostrain.Link(nds[2], LinearSolver.DOF.RZ, -1.0));
            eqtnsRZ.Add(new MultiPointCostrain.Link(nds[1], LinearSolver.DOF.RZ, 1.0));

            HashSet<MultiPointCostrain> links = new HashSet<MultiPointCostrain>();
            links.Add(new MultiPointCostrain(eqtnsDY.ToArray(), 0.0));
            links.Add(new MultiPointCostrain(eqtnsRZ.ToArray(), 0.0));

            LinearSolver fem = new LinearSolver(els.ToArray(), links.ToArray());

            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
        }

        [TestMethod]
        public void BendingTest2()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(300.0, 0, 0));
            nds.Add(new Node(700.0, 0, 0));
            nds.Add(new Node(1000.0, 0, 0));

            List<Beam> els = new List<Beam>();
            els.Add(new Beam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new Beam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double M = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 0, 0.0, 0.0, 0, 0, M);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointCostrain[] rigid = MultiPointCostrain.RigidLink(nds[1], nds[2]);

            LinearSolver fem = new LinearSolver(els.ToArray(), rigid);

            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
            //Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-4);
        }

        [TestMethod]
        public void BendingTest3()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(-300.0, 0, 0));
            nds.Add(new Node(-700.0, 0, 0));
            nds.Add(new Node(-1000.0, 0, 0));

            List<Beam> els = new List<Beam>();
            els.Add(new Beam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new Beam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double M = -1000.0;
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 0, 0.0, 0.0, 0, 0, M);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointCostrain[] rigid = MultiPointCostrain.RigidLink(nds[1], nds[2]);

            LinearSolver fem = new LinearSolver(els.ToArray(), rigid);

            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
        }

        [TestMethod]
        public void BendingTest4()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0));
            nds.Add(new Node(0.0, 0, 300.0));
            nds.Add(new Node(0.0, 0, 700.0));
            nds.Add(new Node(0.0, 0, 1000.0));

            List<Beam> els = new List<Beam>();
            els.Add(new Beam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new Beam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double M = -1000.0;
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 0, 0.0, 0.0, M, 0, 0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointCostrain[] rigid = MultiPointCostrain.RigidLink(nds[1], nds[2]);

            LinearSolver fem = new LinearSolver(els.ToArray(), rigid);

            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
        }

        [TestMethod]
        public void BendingTest5()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0));
            nds.Add(new Node(0.0, 0, -300.0));
            nds.Add(new Node(0.0, 0, -700.0));
            nds.Add(new Node(0.0, 0, -1000.0));

            List<Beam> els = new List<Beam>();
            els.Add(new Beam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new Beam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double M = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 0, 0.0, 0.0, M, 0, 0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointCostrain[] rigid = MultiPointCostrain.RigidLink(nds[1], nds[2]);

            LinearSolver fem = new LinearSolver(els.ToArray(), rigid);

            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
        }

        [TestMethod]
        public void BendingTest6()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0));
            nds.Add(new Node(0.0, 300.0, 0));
            nds.Add(new Node(0.0, 700.0, 0));
            nds.Add(new Node(0.0, 1000.0, 0));

            List<Beam> els = new List<Beam>();
            els.Add(new Beam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new Beam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double M = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute(lc, sys, 0, 0.0, 0.0, M, 0, 0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointCostrain[] rigid = MultiPointCostrain.RigidLink(nds[1], nds[2]);

            LinearSolver fem = new LinearSolver(els.ToArray(), rigid);

            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-4);
        }

        /*
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
        */
    }
}
