using GPC.Geometry;
using GPC.Model.FEM;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Costrains;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Materials;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using static GPC.Model.FEM.Solver;

namespace FemTest.SolverTest { 
    [TestClass]
    public class CostrainTest {
        [TestMethod]
        public void CreateRigidLink()
        {
            Node node0 = new Node(0, 0, 0);

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 1, 0));
            nds.Add(new Node(0, 0, 1));

            Costrain c = new RigidLink(node0, nds.ToArray());

            Assert.AreEqual(6 * 2, ((RigidLink)c).Links.Count());
        }

        [TestMethod]
        public void Axial1()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(300.0, 0, 0));
            nds.Add(new Node(700.0, 0, 0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));;

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 1000;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, 0.0, 0.0, 0.0, 0.0, 0.0);

            nds[2].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigids1 = RigidLink.GetRigidLink(nds[2], nds[1]);
            MultiPointsCostrain[] rigids2 = RigidLink.GetRigidLink(nds[1], nds[2]);

            LinearSolver fem1 = new LinearSolver(els.ToArray(), rigids1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigids2);

            Assert.AreEqual(FX / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (300), fem1.GetDisplacementGlobalCoordinates(nds[2], LinearSolver.DOF.DX), 1e-6);
            Assert.AreEqual(FX / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (300), fem2.GetDisplacementGlobalCoordinates(nds[2], LinearSolver.DOF.DX), 1e-6);
        }

        [TestMethod]
        public void Axial2()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(300.0, 0, 0));
            nds.Add(new Node(700.0, 0, 0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec)); ;

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 1000;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, 0.0, 0.0, 0.0, 0.0, 0.0);

            nds[2].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            Costrain c1 = new RigidLink(nds[2], nds[1]);
            //Costrain c2 = new RigidLink(nds[1], nds[2]);

            LinearSolver fem1 = new LinearSolver(els.ToArray(), new Costrain[] { c1 });
            //LinearSolver fem2 = new LinearSolver(els.ToArray(), new Costrain[] { c2 });

            Assert.AreEqual(FX / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (300), fem1.GetDisplacementGlobalCoordinates(nds[2], DOF.DX), 1e-6);
            //Assert.AreEqual(FX / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (300), fem2.GetDisplacementGlobalCoordinates(nds[2], DOF.DX), 1e-6);
        }

        [TestMethod]
        public void AxialXPositive()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(300.0, 0, 0));
            nds.Add(new Node(700.0, 0, 0));
            nds.Add(new Node(1000.0, 0, 0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 1000;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, 0.0, 0.0, 0.0, 0.0, 0.0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigids1 = RigidLink.GetRigidLink(nds[2], nds[1]);
            MultiPointsCostrain[] rigids2 = RigidLink.GetRigidLink(nds[1], nds[2]);
            
            LinearSolver fem1 = new LinearSolver(els.ToArray(), rigids1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigids2);

            Assert.AreEqual(FX / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-6);
            Assert.AreEqual(FX / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-6);
        }

        [TestMethod]
        public void AxialXNegative()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(-300.0, 0, 0));
            nds.Add(new Node(-700.0, 0, 0));
            nds.Add(new Node(-1000.0, 0, 0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 1000;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, 0.0, 0.0, 0.0, 0.0, 0.0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigids1 = RigidLink.GetRigidLink(nds[2], nds[1]);
            MultiPointsCostrain[] rigids2 = RigidLink.GetRigidLink(nds[1], nds[2]);

            LinearSolver fem1 = new LinearSolver(els.ToArray(), rigids1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigids2);

            Assert.AreEqual(FX / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-6);
            Assert.AreEqual(FX / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-6);
        }

        [TestMethod]
        public void AxialYNegative()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, -300.0, 0));
            nds.Add(new Node(0, -700.0, 0));
            nds.Add(new Node(0, -1000.0, 0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = 1000;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, 0.0, 0.0, 0.0, 0.0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigids1 = RigidLink.GetRigidLink(nds[2], nds[1]);
            MultiPointsCostrain[] rigids2 = RigidLink.GetRigidLink(nds[1], nds[2]);

            LinearSolver fem1 = new LinearSolver(els.ToArray(), rigids1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigids2);

            Assert.AreEqual(F / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-6);
            Assert.AreEqual(F / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-6);
        }

        [TestMethod]
        public void AxialYPositive()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, 300.0, 0));
            nds.Add(new Node(0, 700.0, 0));
            nds.Add(new Node(0, 1000.0, 0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = 1000;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, 0.0, 0.0, 0.0, 0.0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigids1 = RigidLink.GetRigidLink(nds[2], nds[1]);
            MultiPointsCostrain[] rigids2 = RigidLink.GetRigidLink(nds[1], nds[2]);

            LinearSolver fem1 = new LinearSolver(els.ToArray(), rigids1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigids2);

            Assert.AreEqual(F / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-6);
            Assert.AreEqual(F / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-6);
        }

        [TestMethod]
        public void AxialZPositive()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, 0, 300.0));
            nds.Add(new Node(0, 0, 700.0));
            nds.Add(new Node(0, 0, 1000.0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = 1000;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0, F, 0.0, 0.0, 0.0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigids1 = RigidLink.GetRigidLink(nds[2], nds[1]);
            MultiPointsCostrain[] rigids2 = RigidLink.GetRigidLink(nds[1], nds[2]);

            LinearSolver fem1 = new LinearSolver(els.ToArray(), rigids1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigids2);

            Assert.AreEqual(F / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-6);
            Assert.AreEqual(F / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-6);
        }

        [TestMethod]
        public void AxialZNegative()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, 0, -300.0));
            nds.Add(new Node(0, 0, -700.0));
            nds.Add(new Node(0, 0, -1000.0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = 1000;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0, F, 0.0, 0.0, 0.0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigids1 = RigidLink.GetRigidLink(nds[2], nds[1]);
            MultiPointsCostrain[] rigids2 = RigidLink.GetRigidLink(nds[1], nds[2]);

            LinearSolver fem1 = new LinearSolver(els.ToArray(), rigids1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigids2);

            Assert.AreEqual(F / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-6);
            Assert.AreEqual(F / (Math.PI * 100.0 * 100.0 / 4.0 * E) * (1000 - 400), fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-6);
        }

        [TestMethod]
        public void BendingTest1XPositive()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(300.0, 0, 0));
            nds.Add(new Node(700.0, 0, 0));
            nds.Add(new Node(1000.0, 0, 0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double M = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, 0.0, 0.0, 0.0, -M, M);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigid1 = RigidLink.GetRigidLink(nds[1], nds[2]);
            MultiPointsCostrain[] rigid2 = RigidLink.GetRigidLink(nds[2], nds[1]);

            LinearSolver fem = new LinearSolver(els.ToArray(), rigid1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigid2);

            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-4);

            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-4);
        }

        [TestMethod]
        public void BendingTest2XNegative()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(-300.0, 0, 0));
            nds.Add(new Node(-700.0, 0, 0));
            nds.Add(new Node(-1000.0, 0, 0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double M = -1000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0.0, 0.0, 0, -M, M);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigid = RigidLink.GetRigidLink(nds[1], nds[2]);
            MultiPointsCostrain[] rigid2 = RigidLink.GetRigidLink(nds[2], nds[1]);

            LinearSolver fem = new LinearSolver(els.ToArray(), rigid);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigid2);

            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-4);

            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-4);
        }

        [TestMethod]
        public void BendingTest3ZPositive()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0));
            nds.Add(new Node(0.0, 0, 300.0));
            nds.Add(new Node(0.0, 0, 700.0));
            nds.Add(new Node(0.0, 0, 1000.0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double M = -1000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0.0, 0.0, M, -M, 0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigid1 = RigidLink.GetRigidLink(nds[1], nds[2]);
            MultiPointsCostrain[] rigid2 = RigidLink.GetRigidLink(nds[2], nds[1]);

            LinearSolver fem1 = new LinearSolver(els.ToArray(), rigid1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigid2);

            Assert.AreEqual(6.1115, fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
            Assert.AreEqual(6.1115, fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-4);

            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-4);
        }

        [TestMethod]
        public void BendingTest4ZNegative()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0));
            nds.Add(new Node(0.0, 0.0, -300.0));
            nds.Add(new Node(0.0, 0.0, -700.0));
            nds.Add(new Node(0.0, 0.0, -1000.0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double M = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0.0, 0.0, M, -M, 0);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigid1 = RigidLink.GetRigidLink(nds[1], nds[2]);
            MultiPointsCostrain[] rigid2 = RigidLink.GetRigidLink(nds[2], nds[1]);

            LinearSolver fem1 = new LinearSolver(els.ToArray(), rigid1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigid2);

            Assert.AreEqual(6.1115, fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
            Assert.AreEqual(6.1115, fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-4);

            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DY), 1e-4);
            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-4);
        }

        [TestMethod]
        public void BendingTest5YPositive()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0));
            nds.Add(new Node(0.0, 300.0, 0));
            nds.Add(new Node(0.0, 700.0, 0));
            nds.Add(new Node(0.0, 1000.0, 0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double M = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0.0, 0.0, M, 0, -M);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigid1 = RigidLink.GetRigidLink(nds[1], nds[2]);
            MultiPointsCostrain[] rigid2 = RigidLink.GetRigidLink(nds[2], nds[1]);

            LinearSolver fem = new LinearSolver(els.ToArray(), rigid1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigid2);

            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-4);
            Assert.AreEqual(6.1115, fem.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-4);

            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-4);
            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-4);
        }

        [TestMethod]
        public void BendingTest6YNegative()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0));
            nds.Add(new Node(0.0, -300.0, 0));
            nds.Add(new Node(0.0, -700.0, 0));
            nds.Add(new Node(0.0, -1000.0, 0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double M = -1000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0.0, 0.0, M, 0, -M);

            nds[3].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            MultiPointsCostrain[] rigid1 = RigidLink.GetRigidLink(nds[1], nds[2]);
            MultiPointsCostrain[] rigid2 = RigidLink.GetRigidLink(nds[2], nds[1]);

            LinearSolver fem1 = new LinearSolver(els.ToArray(), rigid1);
            LinearSolver fem2 = new LinearSolver(els.ToArray(), rigid2);

            Assert.AreEqual(6.1115, fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-4);
            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-4);

            Assert.AreEqual(6.1115, fem1.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DX), 1e-4);
            Assert.AreEqual(6.1115, fem2.GetDisplacementGlobalCoordinates(nds[3], LinearSolver.DOF.DZ), 1e-4);
        }

        [TestMethod]
        public void RotatedSupport()
        {
            double E = 10.0;
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0.0, 0.0, 0));
            nds.Add(new Node(300.0, 0.0, 0));
            nds.Add(new Node(600.0, 0.0, 0));

            List<EulerBeam> els = new List<EulerBeam>();
            els.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            els.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = -1000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, F, 0, 0, 0, 0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[2].AddAttribute(fix);

            MultiPointsCostrain.Equation[] eqts = new MultiPointsCostrain.Equation[2];
            eqts[0] = new MultiPointsCostrain.Equation(nds[0], DOF.DX, 1.0);
            eqts[1] = new MultiPointsCostrain.Equation(nds[0], DOF.DY, 1.0);
            MultiPointsCostrain rotatedSupport = new MultiPointsCostrain(eqts, 0);

            LinearSolver fem1 = new LinearSolver(els.ToArray(), new MultiPointsCostrain[] { rotatedSupport });

            Assert.AreEqual(310.8808, fem1.GetReaction(nds[0], LinearSolver.DOF.DX), 1e-4);
            Assert.AreEqual(310.8808, fem1.GetReaction(nds[0], LinearSolver.DOF.DY), 1e-4);
            Assert.AreEqual(2.375, fem1.GetDisplacementGlobalCoordinates(nds[0], LinearSolver.DOF.DX), 1e-4);
            Assert.AreEqual(-2.375, fem1.GetDisplacementGlobalCoordinates(nds[0], LinearSolver.DOF.DY), 1e-4);
        }
    }
}
