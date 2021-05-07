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
using static GPC.Model.FEM.Solver;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace FemTest.SolverTest { 
    [TestClass]
    public class BeamTest {

        /// <summary>
        /// Sforzo Assiale su trave incastro - libero - direzione +X
        /// </summary>
        [TestMethod]
        public void CantilverAxial1()
        {
            double E = 100000.0;
            double H = 1;
            double t = (H / 2.0)*0.999;
            Section sec = new SectionRHS(H,H,t,t,t,t,true, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");
            double A = sec.Area;

            double L = 1000;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            EulerBeam b = new EulerBeam(nds.ToArray(), sec);

            LoadCaseBase lc = new LoadCaseBase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 1000;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, 0.0, 0.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(FX / (E * A) * L, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DX), 1e-6);

            Assert.AreEqual(FX, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.N]);
            Assert.AreEqual(FX, fem.GetBeamInternalForces(b, b.L/2.0)[Beam.InternalAction.N]);
            Assert.AreEqual(FX, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.N]);

            Assert.AreEqual(-FX, fem.GetReaction(nds[0], DOF.DX));
        }

        /// <summary>
        /// Sforzo uniforme Assiale su trave incastro - libero - direzione +X
        /// </summary>
        [TestMethod]
        public void CantileverAxialUniformLoadTest1()
        {
            double E = 100000.0;
            double H = 1;
            double t = (H / 2.0) * 0.999;
            Section sec = new SectionRHS(H, H, t, t, t, t, true, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");
            double A = sec.Area;

            double L = 1000;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            EulerBeam b = new EulerBeam(nds.ToArray(), sec);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double qx = 1;
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, 0, 0);

            b.AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(qx * L / 2.0 / (E * A) * L, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX), 1e-6);
            Assert.AreEqual(-qx * L, fem.GetReaction(nds[0], DOF.DX));
            Assert.AreEqual(0.0, fem.GetReaction(nds[1], DOF.DX));

            Assert.AreEqual(qx * L, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.N]);
            Assert.AreEqual(qx * L / 2.0, fem.GetBeamInternalForces(b, b.L/2.0)[Beam.InternalAction.N]);
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.N]);
        }

        /// <summary>
        /// Momento flettente nodale su trave incastro - libero - direzione trave +X
        /// </summary>
        [TestMethod]
        public void CantileverBendingTest1()
        {
            double E = 100000.0;
            double H = 1;
            double t = (H / 2.0) * 0.999;
            Section sec = new SectionRHS(H, H, t, t, t, t, true, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            EulerBeam b = new EulerBeam(nds.ToArray(), sec);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 0.0;
            double MY = 1000.0;
            double MZ = 2000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, 0.0, 0.0, 0, MY, MZ);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(MZ * L*L / (2.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 1e-4);

            Assert.AreEqual(MZ, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M3],1e-6);
            Assert.AreEqual(MZ, fem.GetBeamInternalForces(b, b.L/2.0)[Beam.InternalAction.M3], 1e-6);
            Assert.AreEqual(MZ, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.M3], 1e-6);

            Assert.AreEqual(-MY, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M2], 1e-6);
            Assert.AreEqual(-MY, fem.GetBeamInternalForces(b, b.L/2.0)[Beam.InternalAction.M2], 1e-6);
            Assert.AreEqual(-MY, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.M2], 1e-6);
        }

        /// <summary>
        /// Momento flettente nodale su trave incastro - libero - direzione -Y
        /// </summary>
        [TestMethod]
        public void CantileverBendingTest2()
        {
            double E = 100000.0;
            double H = 1;
            double t = (H / 2.0) * 0.999;
            Section sec = new SectionRHS(H, H, t, t, t, t, true, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");
            double A = sec.Area;

            double L = 1000;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, -L, 0));

            EulerBeam b = new EulerBeam(nds.ToArray(), sec);

            LoadCaseBase lc = new LoadCaseBase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, 0.0, 0.0, -1000.0, 0.0, 0.0);

            double MX = -1000;
            NodeForceAttribute m = new NodeForceAttribute("lc", sys, 0.0, 0.0, 0.0, MX, 0.0, 0.0);

            nds[1].AddAttribute(m);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(1.0 / 2.0 * -MX * L*L / (E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DZ), 1e-4);

            Assert.AreEqual(-MX, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M2], 1e-6);
            Assert.AreEqual(-MX, fem.GetBeamInternalForces(b, b.L / 2.0)[Beam.InternalAction.M2], 1e-6);
            Assert.AreEqual(-MX, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.M2], 1e-6);
        }

        /// <summary>
        /// Forza di taglio su trave incastro - libero - direzione +X
        /// </summary>
        [TestMethod]
        public void CantileverShearForceTest1()
        {
            double E = 100000.0;
            double H = 1;
            double t = (H / 2.0) * 0.999;
            Section sec = new SectionRHS(H, H, t, t, t, t, true, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");
            double A = sec.Area;

            double L = 1000;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            EulerBeam b = new EulerBeam(nds.ToArray(), sec);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FY = 10.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, FY, 0.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(1.0 / 3.0 * FY * L*L*L/(E*sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 1e-4);

            Assert.AreEqual(-FY, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.V2]);
            Assert.AreEqual(-FY, fem.GetBeamInternalForces(b, b.L/2.0)[Beam.InternalAction.V2]);
            Assert.AreEqual(-FY, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.V2]);

            Assert.AreEqual(FY * b.L, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M3], 1e-3);
            Assert.AreEqual(FY * b.L / 2.0, fem.GetBeamInternalForces(b, b.L / 2.0)[Beam.InternalAction.M3], 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.M3], 1e-3);
        }

        [TestMethod]
        public void CantileverShearForceTest2()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 10.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, 0, 1000.0));

            EulerBeam b = new EulerBeam(nds.ToArray(), sec);
;
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 10/2.0;
            double FY = 10;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, FY, 0.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(67.9061/2, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DX), 1e-4);
            Assert.AreEqual(67.9061, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 1e-4);

            Assert.AreEqual(-FY, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.V2]);
            Assert.AreEqual(-FY, fem.GetBeamInternalForces(b, b.L/2.0)[Beam.InternalAction.V2]);
            Assert.AreEqual(-FY, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.V2]);

            Assert.AreEqual(FY * b.L, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M3], 1e-3);
            Assert.AreEqual(FY * b.L/2.0, fem.GetBeamInternalForces(b, b.L / 2.0)[Beam.InternalAction.M3], 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.M3], 1e-3);

            Assert.AreEqual(FX, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.V3]);
            Assert.AreEqual(FX, fem.GetBeamInternalForces(b, b.L/2.0)[Beam.InternalAction.V3]);
            Assert.AreEqual(FX, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.V3]);

            Assert.AreEqual(-FX * b.L, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M2], 1e-3);
            Assert.AreEqual(-FX * b.L/2.0, fem.GetBeamInternalForces(b, b.L / 2.0)[Beam.InternalAction.M2], 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.M2], 1e-3);
        }

        [TestMethod]
        public void CantileverShearForceTest3()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 10.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, 1000.0, 0));

            EulerBeam b = new EulerBeam(nds.ToArray(), sec);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double fx = 10;
            double fz = 20;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, fx, 0.0, fz, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(67.9061, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DX), 1e-4);
            Assert.AreEqual(2.0 * 67.9061, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DZ), 1e-4);

            Assert.AreEqual(fx, fem.GetBeamInternalForces(b, 0.0, Beam.InternalAction.V2));
            Assert.AreEqual(fx, fem.GetBeamInternalForces(b, b.L/2.0, Beam.InternalAction.V2));
            Assert.AreEqual(fx, fem.GetBeamInternalForces(b, b.L, Beam.InternalAction.V2));

            Assert.AreEqual(-fx * b.L, fem.GetBeamInternalForces(b, 0.0, Beam.InternalAction.M3), 1e-3);
            Assert.AreEqual(-fx * b.L/2.0, fem.GetBeamInternalForces(b, b.L / 2.0, Beam.InternalAction.M3), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(b, b.L, Beam.InternalAction.M3), 1e-3);
        }

        /// <summary>
        /// cantilever sottoposto a torsione
        /// </summary>
        [TestMethod]
        public void CantileverTorsionTest1()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 10.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(1000, 0, 0));

            EulerBeam b = new EulerBeam(nds.ToArray(), sec);

            LoadCaseBase lc = new LoadCaseBase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double MX = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, 0.0, 0.0, MX, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(1.1672, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.RX) * 180.0 / Math.PI, 1e-4);

            Assert.AreEqual(1.1672, fem.GetBeamDisplacementInLocalCoordinates(b, b.L, Beam.LocalDOF.TorsionR1) * 180.0 / Math.PI, 1e-4);
            Assert.AreEqual(1.1672 / 2.0, fem.GetBeamDisplacementInLocalCoordinates(b, b.L/2.0, Beam.LocalDOF.TorsionR1) * 180.0 / Math.PI, 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(b, 0.0, Beam.LocalDOF.TorsionR1) * 180.0 / Math.PI, 1e-4);

            Assert.AreEqual(MX, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.T]);
            Assert.AreEqual(MX, fem.GetBeamInternalForces(b, b.L/2.0)[Beam.InternalAction.T]);
            Assert.AreEqual(MX, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.T]);
        }

        /// <summary>
        /// 2 beams 1 node force
        /// </summary>
        [TestMethod]
        public void SimplySupportedTest1()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 1.0, 0.0, 355, 510, 7850), "sec");

            double L = 2000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L/2.0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = 1.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, -F, 0.0, 0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute support = new NodeRestrainAttribute("fc", sys);
            support.AddExternalRestrain(Solver.DOF.DX);
            support.AddExternalRestrain(Solver.DOF.DY);
            support.AddExternalRestrain(Solver.DOF.DZ);
            support.AddExternalRestrain(Solver.DOF.RX);

            nds[0].AddAttribute(support);
            nds[2].AddAttribute(support);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(-33.9531, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 1e-4);

            Assert.AreEqual(-23.3427, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L/2.0, Beam.LocalDOF.U2), 1e-4);

            Assert.AreEqual(0, fem.GetBeamInternalForces(beams[0], 0)[Beam.InternalAction.M3]);
            Assert.AreEqual(F * L / 4.0 / 2.0, fem.GetBeamInternalForces(beams[0], beams[0].L/2.0)[Beam.InternalAction.M3], 1e-4);
            Assert.AreEqual(F*L/4.0, fem.GetBeamInternalForces(beams[0], beams[0].L)[Beam.InternalAction.M3], 1e-6);

            Assert.AreEqual(F / 2.0, fem.GetBeamInternalForces(beams[0], 0.0)[Beam.InternalAction.V2], 1e-6);
            Assert.AreEqual(F / 2.0, fem.GetBeamInternalForces(beams[0], beams[0].L / 2.0)[Beam.InternalAction.V2], 1e-4);
            Assert.AreEqual(F / 2.0, fem.GetBeamInternalForces(beams[0], beams[0].L)[Beam.InternalAction.V2], 1e-6);

            Assert.AreEqual(F * L /4.0, fem.GetBeamInternalForces(beams[1], 0)[Beam.InternalAction.M3], 1e-6);
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(beams[1], beams[1].L)[Beam.InternalAction.M3], 1e-6);

            Assert.AreEqual(-F/2.0, fem.GetBeamInternalForces(beams[1],0)[Beam.InternalAction.V2], 1e-6);
            Assert.AreEqual(-F / 2.0, fem.GetBeamInternalForces(beams[1], beams[1].L)[Beam.InternalAction.V2], 1e-6);
        }

        /// <summary>
        /// esempio svincolo completo End2
        /// </summary>
        [TestMethod]
        public void AllEndReleaseTest1()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 1.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(1000, 0, 0));
            nds.Add(new Node(1000, 1000, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = 100.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, F, F, F, 0, 0, 0);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            //add release
            beams[0].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] {
                Beam.LocalDOF.AxialU1,
                Beam.LocalDOF.U2,
                Beam.LocalDOF.U3,
                Beam.LocalDOF.TorsionR1,
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3
                },
                "fc", "rel1");
            
            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(6790.6109, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DX), 1e-4);
            Assert.AreEqual(12.7324, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 1e-4);
            Assert.AreEqual(6790.6109, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DZ), 1e-4);

            Assert.AreEqual(0.0, fem.GetReaction(nds[0],DOF.DX), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[0], DOF.DY), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[0], DOF.DZ), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[0], DOF.RX), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[0], DOF.RY), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[0], DOF.RZ), 1e-4);

            Assert.AreEqual(-F, fem.GetReaction(nds[2], DOF.DX), 1e-4);
            Assert.AreEqual(-F, fem.GetReaction(nds[2], DOF.DY), 1e-4);
            Assert.AreEqual(-F, fem.GetReaction(nds[2], DOF.DZ), 1e-4);
            Assert.AreEqual(100000, fem.GetReaction(nds[2], DOF.RX), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[2], DOF.RY), 1e-4);
            Assert.AreEqual(-100000, fem.GetReaction(nds[2], DOF.RZ), 1e-4);

            var displ = fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.AxialU1]);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.U2]);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.U3]);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.TorsionR1]);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.R2]);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.R3]);

            displ = fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.AxialU1]);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.U2]);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.U3]);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.TorsionR1]);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.R2]);
            Assert.AreEqual(0.0, displ[Beam.LocalDOF.R3]);
        }

        /// <summary>
        /// svincolo completo End1
        /// </summary>
        [TestMethod]
        public void AllEndReleaseTest2()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 1.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(1000, 0, 0));
            nds.Add(new Node(1000, 1000, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = 100.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, F, F, F, 0, 0, 0);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            //add release
            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] {
                Beam.LocalDOF.AxialU1,
                Beam.LocalDOF.U2,
                Beam.LocalDOF.U3,
                Beam.LocalDOF.TorsionR1,
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3
                },
                "fc", "rel1");

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(12.7324, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DX), 1e-4);
            Assert.AreEqual(6790.6109, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 1e-4);
            Assert.AreEqual(6790.6109, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DZ), 1e-4);

            Assert.AreEqual(0.0, fem.GetReaction(nds[2], DOF.DX), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[2], DOF.DY), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[2], DOF.DZ), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[2], DOF.RX), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[2], DOF.RY), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[2], DOF.RZ), 1e-4);

            Assert.AreEqual(-F, fem.GetReaction(nds[0], DOF.DX), 1e-4);
            Assert.AreEqual(-F, fem.GetReaction(nds[0], DOF.DY), 1e-4);
            Assert.AreEqual(-F, fem.GetReaction(nds[0], DOF.DZ), 1e-4);
            Assert.AreEqual(0, fem.GetReaction(nds[0], DOF.RX), 1e-4);
            Assert.AreEqual(100000, fem.GetReaction(nds[0], DOF.RY), 1e-4);
            Assert.AreEqual(-100000, fem.GetReaction(nds[0], DOF.RZ), 1e-4);
        }

        /// <summary>
        /// end release end 1 + end 2
        /// </summary>
        [TestMethod]
        public void ArcTest1()
        {
            Section sec = new SectionCHS(100.0, 5.0, new SteelMaterial("m", 1000.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(1000, 300, 0));
            nds.Add(new Node(2000, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = -100.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, 0, 0, 0, 0);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            NodeRestrainAttribute dz = new NodeRestrainAttribute("fc", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);
            dz.AddExternalRestrain(Solver.DOF.RY);
            dz.AddExternalRestrain(Solver.DOF.RX);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            nds[1].AddAttribute(dz);

            //add release
            Beam.LocalDOF[] hinge = new Beam.LocalDOF[] {
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3
                };

            beams[0].AddEndRelease(Beam.EndSide.End1, hinge, "fc", "rel");
            //beams[0].AddEndRelease(Beam.EndSide.End2, hinge, "fc", "rel");
            
            beams[1].AddEndRelease(Beam.EndSide.End1, hinge, "fc", "rel");
            beams[1].AddEndRelease(Beam.EndSide.End2, hinge, "fc", "rel");

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(-0.423666, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 1e-4);

            Assert.AreEqual(-174.0051, fem.GetBeamInternalForces(beams[0], 0.0, Beam.InternalAction.N), 1e-4);
            Assert.AreEqual(-174.0051, fem.GetBeamInternalForces(beams[0], beams[0].L/2.0, Beam.InternalAction.N), 1e-4);
            Assert.AreEqual(-174.0051, fem.GetBeamInternalForces(beams[0], beams[0].L, Beam.InternalAction.N), 1e-4);

            Assert.AreEqual(-174.0051, fem.GetBeamInternalForces(beams[1], 0.0, Beam.InternalAction.N), 1e-4);
            Assert.AreEqual(-174.0051, fem.GetBeamInternalForces(beams[1], beams[1].L / 2.0, Beam.InternalAction.N), 1e-4);
            Assert.AreEqual(-174.0051, fem.GetBeamInternalForces(beams[1], beams[1].L, Beam.InternalAction.N), 1e-4);
        }

        /// <summary>
        /// End release end 1 + end 2
        /// </summary>
        [TestMethod]
        public void DoubleEndReleaseTest1()
        {
            Section sec = new SectionCHS(100.0, 5.0, new SteelMaterial("m", 1000.0, 0.0, 355, 510, 7850), "sec");

            double L = 2000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L / 3.0, 0, 0));
            nds.Add(new Node(2.0 * L / 3.0, 0, 0));
            nds.Add(new Node(3.0 * L / 3.0, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = -100.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, 0, 0, 0, 0);

            nds[1].AddAttribute(f);
            nds[2].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            NodeRestrainAttribute dz = new NodeRestrainAttribute("fc", sys);
            dz.AddExternalRestrain(Solver.DOF.DZ);
            dz.AddExternalRestrain(Solver.DOF.RY);
            dz.AddExternalRestrain(Solver.DOF.RX);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            nds[1].AddAttribute(dz);
            nds[2].AddAttribute(dz);

            //add release
            Beam.LocalDOF[] hinge = new Beam.LocalDOF[] {
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3
                };

            //beams[0].AddEndRelease(Beam.EndSide.End1, hinge, "fc", "rel");
            //beams[0].AddEndRelease(Beam.EndSide.End2, hinge, "fc", "rel");
            
            beams[1].AddEndRelease(Beam.EndSide.End1, hinge, "fc", "rel");
            beams[1].AddEndRelease(Beam.EndSide.End2, hinge, "fc", "rel");

            //beams[2].AddEndRelease(Beam.EndSide.End1, hinge, "fc", "rel");
            //beams[1].AddEndRelease(Beam.EndSide.End2, hinge, "fc", "rel");

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(-5.850634, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 1e-4);
            Assert.AreEqual(-5.850634, fem.GetDisplacementGlobalCoordinates(nds[2], Solver.DOF.DY), 1e-4);

            Assert.AreEqual(100, fem.GetReaction(nds[0], Solver.DOF.DY), 1e-4);
            Assert.AreEqual(100, fem.GetReaction(nds[3], Solver.DOF.DY), 1e-4);

            Assert.AreEqual(66666.66, fem.GetReaction(nds[0], Solver.DOF.RZ), 1e-2);
            Assert.AreEqual(-66666.66, fem.GetReaction(nds[3], Solver.DOF.RZ), 1e-2);
        }

        /// <summary>
        /// end release in end 1 and 2, Pyramid
        /// </summary>
        [TestMethod]
        public void DoubleEndReleaseTest2()
        {
            Section sec = new SectionCHS(100.0, 5.0, new SteelMaterial("m", 1.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(20.0, 0, 0));
            nds.Add(new Node(10.0, 17.32, 0));
            nds.Add(new Node(10.0, 5.7736, 10.0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[3] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[3] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = -100.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, 0, F, 0, 0, 0);

            nds[3].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[1].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            //add release
            Beam.LocalDOF[] hinge = new Beam.LocalDOF[] {
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3
                };

            beams[0].AddEndRelease(Beam.EndSide.End1, hinge, "fc", "rel");
            beams[0].AddEndRelease(Beam.EndSide.End2, hinge, "fc", "rel");

            beams[1].AddEndRelease(Beam.EndSide.End1, hinge, "fc", "rel");
            beams[1].AddEndRelease(Beam.EndSide.End2, hinge, "fc", "rel");

            beams[2].AddEndRelease(Beam.EndSide.End1, hinge, "fc", "rel");
            beams[2].AddEndRelease(Beam.EndSide.End2, hinge, "fc", "rel");

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0, fem.GetDisplacementGlobalCoordinates(nds[3], Solver.DOF.DX), 1e-4);
            Assert.AreEqual(0, fem.GetDisplacementGlobalCoordinates(nds[3], Solver.DOF.DY), 1e-4);
            Assert.AreEqual(-0.7961, fem.GetDisplacementGlobalCoordinates(nds[3], Solver.DOF.DZ), 1e-4);

            Assert.AreEqual(33.33, fem.GetReaction(nds[0], Solver.DOF.DZ), 1e-2);
            Assert.AreEqual(33.33, fem.GetReaction(nds[1], Solver.DOF.DZ), 1e-2);
            Assert.AreEqual(33.33, fem.GetReaction(nds[2], Solver.DOF.DZ), 1e-2);
        }

        /// <summary>
        /// Cantilever with uniform load
        /// </summary>
        [TestMethod]
        public void AppliedDistributedLoadTest1()
        {
            double E = 100000.0;
            Section sec = new SectionRHS(100.0, 100.0, 49.99, 49.99, 49.99, 49.99, false, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));

            LoadCaseBase lc = new LoadCaseBase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            Random random = new Random();
            double qx = random.Next(-1000,1000);
            double qy = random.Next(-1000, 1000);
            double qz = random.Next(-1000, 1000);
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, qy, qz);
            beams[0].AddLoadCaseAttribute(q);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual( (qx * L/2) / E / sec.Area * L, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DX),1e-6); //displacement
            Assert.AreEqual(-qx * L, fem.GetReaction(nds[0], Solver.DOF.DX), 1e-2); //reaction

            Assert.AreEqual(-qy * L, fem.GetReaction(nds[0], Solver.DOF.DY), 0.001); //Shear reaction
            Assert.AreEqual(-qy * L * L / 2.0, fem.GetReaction(nds[0], Solver.DOF.RZ), 0.001); //Bending Moment reaction

            Assert.AreEqual(qy * Math.Pow(L, 4.0) / (8.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 0.001); //displacement
            Assert.AreEqual(qy * Math.Pow(L, 3.0) / (6.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.RZ), 0.001); //rotation

            Assert.AreEqual(-qz * L, fem.GetReaction(nds[0], Solver.DOF.DZ), 0.001); //Shear reaction
            Assert.AreEqual(qz * L * L / 2.0, fem.GetReaction(nds[0], Solver.DOF.RY), 0.001); //Bending Moment reaction

            Assert.AreEqual(qz * Math.Pow(L, 4.0) / (8.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DZ), 0.001); //displacement
            Assert.AreEqual(-qz * Math.Pow(L, 3.0) / (6.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.RY), 0.001); //rotation

            Assert.AreEqual(1.0 / 2.0 * qy * L * L, fem.GetBeamInternalForces(beams[0], 0.0)[Beam.InternalAction.M3], 0.0001); //M(x=0)
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(beams[0], L)[Beam.InternalAction.M3], 0.0001); //M(x=L)

            Assert.AreEqual(-qy * L, fem.GetBeamInternalForces(beams[0], 0.0)[Beam.InternalAction.V2], 0.0001); //Shear(x=0)
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(beams[0], L)[Beam.InternalAction.V2], 0.0001); //Shear(x=0)

            Assert.AreEqual(1.0 / 2.0 * qz * L * L, fem.GetBeamInternalForces(beams[0],0.0)[Beam.InternalAction.M2], 0.0001); //M(x=0)
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(beams[0], L)[Beam.InternalAction.M2], 0.0001); //M(x=0)

            Assert.AreEqual(-qz * L, fem.GetBeamInternalForces(beams[0], 0.0)[Beam.InternalAction.V3], 0.0001); //Shear(x=0)
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(beams[0], L)[Beam.InternalAction.V3], 0.0001); //Shear(x=L)
        }
        
        /// <summary>
        /// Simply supported beam descretized with 2 elements and uniform load
        /// </summary>
        [TestMethod]
        public void SimplySupportedTest2()
        {
            double E = 100.0;
            Section sec = new SectionRHS(100.0, 100.0, 49.99, 49.99, 49.99, 49.99, false, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");
            Console.WriteLine("A = " + sec.Area);

            double L = 1000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L/2.0, 0, 0));
            nds.Add(new Node(2.0 * L/2.0, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            Random random = new Random();
            double qx = 1; //random.Next(-1000, 1000);
            double qy = 2; //random.Next(-1000, 1000);
            double qz = 3; //random.Next(-1000, 1000);
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, qy, qz);
            beams[0].AddLoadCaseAttribute(q);
            beams[1].AddLoadCaseAttribute(q);

            NodeRestrainAttribute hinge = new NodeRestrainAttribute("fc", sys);
            hinge.AddExternalRestrain(Solver.DOF.DX);
            hinge.AddExternalRestrain(Solver.DOF.DY);
            hinge.AddExternalRestrain(Solver.DOF.DZ);
            hinge.AddExternalRestrain(Solver.DOF.RX);

            nds[0].AddAttribute(hinge);
            nds[2].AddAttribute(hinge);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual((qx * L) / E / sec.Area / 8.0 * L, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX), 1e-6); //displacement
            Assert.AreEqual(-qx * L / 2.0, fem.GetReaction(nds[0], Solver.DOF.DX), 1e-2); //reaction

            Assert.AreEqual(-qy * L / 2.0, fem.GetReaction(nds[0], Solver.DOF.DY), 0.001); //Shear
            Assert.AreEqual(-qy * L * L / 8.0, fem.GetBeamInternalForces(beams[0],beams[0].L)[Beam.InternalAction.M3], 1e-6); //Bending Moment
            Assert.AreEqual(-qy * L * L / 8.0, fem.GetBeamInternalForces(beams[1],0)[Beam.InternalAction.M3], 1e-6); //Bending Moment

            Assert.AreEqual(5.0 / 384.0 * qy * Math.Pow(L, 4.0) / (E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 0.001); //displacement
            Assert.AreEqual(qy * Math.Pow(L, 3.0) / (24.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[0], Solver.DOF.RZ), 0.001); //rotation
            Assert.AreEqual(-qy * Math.Pow(L, 3.0) / (24.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[2], Solver.DOF.RZ), 0.001); //rotation

            Assert.AreEqual(-qz * L / 2.0, fem.GetReaction(nds[0], Solver.DOF.DZ), 0.001); //Shear

            Assert.AreEqual(5.0 / 384.0 * qz * Math.Pow(L, 4.0) / (E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DZ), 0.001); //displacement
            Assert.AreEqual(-qz * Math.Pow(L, 3.0) / (24.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[0], Solver.DOF.RY), 0.001); //rotation
            Assert.AreEqual(qz * Math.Pow(L, 3.0) / (24.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[2], Solver.DOF.RY), 0.001); //rotation
        }

        /// <summary>
        /// Fixed beam 45 deg inclined descretized with 2 elements and uniform load
        /// </summary>
        [TestMethod]
        public void FixFixTest2()
        {
            double E = 100.0;
            Section sec = new SectionRHS(100.0, 100.0, 10, 10, 10, 10, false, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L / 2.0, L / 2.0, 0));
            nds.Add(new Node(L, L, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            Random random = new Random();
            double qx = 0; //random.Next(-1000, 1000);
            double qy = 1; //random.Next(-1000, 1000);
            double qz = 0; //random.Next(-1000, 1000);
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, qy, qz);
            beams[0].AddLoadCaseAttribute(q);
            beams[1].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(21.1721, fem.GetBeamDisplacementInLocalCoordinatesAtNode(beams[1], 0)[Beam.LocalDOF.U2], 1e-4); //displacement
            Assert.AreEqual(21.1721, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0)[Beam.LocalDOF.U2], 1e-4); //displacement
        }

        /// <summary>
        /// Fixed beam 45 deg inclined descretized with 1 elements and uniform load
        /// </summary>
        [TestMethod]
        public void FixFixTest3()
        {
            double E = 100.0;
            Section sec = new SectionRHS(100.0, 100.0, 10, 10, 10, 10, false, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));;
            nds.Add(new Node(L, L, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            Random random = new Random();
            double qx = 0; //random.Next(-1000, 1000);
            double qy = 1; //random.Next(-1000, 1000);
            double qz = 0; //random.Next(-1000, 1000);
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, qy, qz);
            beams[0].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[1].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0)[Beam.LocalDOF.U2], 1e-4); //displacement
            Assert.AreEqual(21.1721, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0)[Beam.LocalDOF.U2], 1e-4); //displacement
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L)[Beam.LocalDOF.U2], 1e-4); //displacement
        }

        /// <summary>
        /// Fixed beam 45 deg inclined descretized with 3 elements and uniform load
        /// </summary>
        [TestMethod]
        public void FixFixTest4()
        {
            double E = 100.0;
            Section sec = new SectionRHS(100.0, 100.0, 10, 10, 10, 10, false, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L / 3.0, L / 3.0, 0));
            nds.Add(new Node(L * 2.0 / 3.0, L * 2.0 / 3.0, 0));
            nds.Add(new Node(L, L, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            Random random = new Random();
            double qx = 0; //random.Next(-1000, 1000);
            double qy = 1; //random.Next(-1000, 1000);
            double qz = 1; //random.Next(-1000, 1000);
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, qy, qz);
            beams[0].AddLoadCaseAttribute(q);
            beams[1].AddLoadCaseAttribute(q);
            beams[2].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(21.1721, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[0].L / 2.0)[Beam.LocalDOF.U2], 1e-4); //displacement
            Assert.AreEqual(21.1721, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[0].L / 2.0)[Beam.LocalDOF.U3], 1e-4); //displacement
        }

        /// <summary>
        /// Fix-Hinge beam with 3 elements and uniform load
        /// </summary>
        [TestMethod]
        public void FixHingeTest4()
        {
            double E = 100.0;
            Section sec = new SectionRHS(100.0, 100.0, 10, 10, 10, 10, false, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L / 3.0, .0, 0));
            nds.Add(new Node(L * 2.0 / 3.0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            Random random = new Random();
            double qx = 0; //random.Next(-1000, 1000);
            double qy = 1; //random.Next(-1000, 1000);
            double qz = 1; //random.Next(-1000, 1000);
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, qy, qz);
            beams[0].AddLoadCaseAttribute(q);
            beams[1].AddLoadCaseAttribute(q);
            beams[2].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            NodeRestrainAttribute hinge = new NodeRestrainAttribute("fc", sys);
            hinge.AddExternalRestrain(Solver.DOF.DX);
            hinge.AddExternalRestrain(Solver.DOF.DY);
            hinge.AddExternalRestrain(Solver.DOF.DZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(hinge);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(10.5860, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[0].L / 2.0)[Beam.LocalDOF.U2], 1e-4); //displacement
            Assert.AreEqual(10.5860, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[0].L / 2.0)[Beam.LocalDOF.U3], 1e-4); //displacement
        }

        /// <summary>
        /// Fix-Fix + uniform load
        /// </summary>
        [TestMethod]
        public void FixFixTest1()
        {
            double E = 100.0;
            Section sec = new SectionRHS(100.0, 100.0, 49.99, 49.99, 49.99, 49.99, false, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));

            LoadCaseBase lc = new LoadCaseBase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            Random random = new Random();
            double qx = 0; //random.Next(-1000, 1000);
            double qy = 1; //random.Next(-1000, 1000);
            double qz = 2;// random.Next(-1000, 1000);
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, qy, qz);
            beams[0].AddLoadCaseAttribute(q);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[1].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 1e-6); //displacement
            Assert.AreEqual(-qy * L /2.0, fem.GetReaction(nds[0], Solver.DOF.DY), 1e-2); //reaction

            Assert.AreEqual(-1.0 / 24.0 * qy * L * L, fem.GetBeamInternalForces(beams[0], beams[0].L / 2.0, Beam.InternalAction.M3));
        }

        /// <summary>
        /// One beam - uniform loading
        /// </summary>
        [TestMethod]
        public void SimplySupportedTest3()
        {
            double E = 100.0;
            Section sec = new SectionRHS(100.0, 100.0, 49.99, 49.99, 49.99, 49.99, false, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));

            LoadCaseBase lc = new LoadCaseBase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            Random random = new Random();
            double qx = 0; //random.Next(-1000, 1000);
            double qy = 1; //random.Next(-1000, 1000);
            double qz = 1;// random.Next(-1000, 1000);
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, qy, qz);
            beams[0].AddLoadCaseAttribute(q);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute("fc", sys);
            hinge.AddExternalRestrain(Solver.DOF.DX);
            hinge.AddExternalRestrain(Solver.DOF.DY);
            hinge.AddExternalRestrain(Solver.DOF.DZ);
            hinge.AddExternalRestrain(Solver.DOF.RX);

            nds[0].AddAttribute(hinge);
            nds[1].AddAttribute(hinge);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetDisplacementGlobalCoordinates(nds[1], Solver.DOF.DY), 1e-6); //displacement
            Assert.AreEqual(-qy * L / 2.0, fem.GetReaction(nds[0], Solver.DOF.DY), 1e-2); //reaction

            Assert.AreEqual(-1.0 / 8.0 * qy * L * L, fem.GetBeamInternalForces(beams[0],beams[0].L/2.0)[Beam.InternalAction.M3],1e-6);
             //displacement

            Assert.AreEqual(0.0, fem.GetBeamInternalForces(beams[0],0)[Beam.InternalAction.M3], 1e-6);
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(beams[0],beams[0].L)[Beam.InternalAction.M3], 1e-6);
        }

        /// <summary>
        /// Momento flettente nodale su trave incastro - libero - direzione trave +X, rotazione 45 gradi
        /// </summary>
        [TestMethod]
        public void RotatedBeamTest1()
        {
            double E = 100000.0;
            double H = 100;
            double B = 50;
            double tw = 5.0;
            double tf = 5.0;
            Section sec = new SectionRHS(H, B, tf, tf, tw, tw, true, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            double rotationRad = 25.0 * Math.PI / 180.0;
            EulerBeam b = new EulerBeam(nds.ToArray(), sec, rotationRad);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 0.0;
            double FY = 10.0;
            double MY = 0.0;
            double MZ = 0.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, FY, 0.0, 0, MY, MZ);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(0.0264, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-4);
            Assert.AreEqual(-0.0154, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-4);

            Assert.AreEqual(9063.0779, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M3], 0.01);

            Assert.AreEqual(-4226.1826, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M2], 0.01);
        }

        /// <summary>
        /// Momento flettente nodale su trave incastro - libero - direzione trave +Y, rotazione 45 gradi
        /// </summary>
        [TestMethod]
        public void RotatedBeamTest2()
        {
            double E = 100000.0;
            double H = 100;
            double B = 50;
            double tw = 5.0;
            double tf = 5.0;
            Section sec = new SectionRHS(H, B, tf, tf, tw, tw, true, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, L, 0));

            double rotationRad = 35.0 * Math.PI / 180.0;
            EulerBeam b = new EulerBeam(nds.ToArray(), sec, rotationRad);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 0.0;
            double FY = 0.0;
            double FZ = 10.0;
            double MY = 0.0;
            double MZ = 0.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, FY, FZ, 0, MY, MZ);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(0.0189, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DX), 1e-4);
            Assert.AreEqual(0.0461, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-4);

            Assert.AreEqual(8191.5204, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M2], 0.01);

            Assert.AreEqual(5735.7644, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M3], 0.01);
        }

        /// <summary>
        /// Momento flettente nodale su trave incastro - libero - direzione trave +Z, rotazione 45 gradi
        /// </summary>
        [TestMethod]
        public void RotatedBeamTest3()
        {
            double E = 100000.0;
            double H = 100;
            double B = 50;
            double tw = 5.0;
            double tf = 5.0;
            Section sec = new SectionRHS(H, B, tf, tf, tw, tw, true, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");

            double L = 1000;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, 0, L));

            double rotationRad = 25.0 * Math.PI / 180.0;
            EulerBeam b = new EulerBeam(nds.ToArray(), sec, rotationRad);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 0.0;
            double FY = 10.0;
            double FZ = 0.0;
            double MY = 0.0;
            double MZ = 0.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, FY, FZ, 0, MY, MZ);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(0.0154, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DX), 1e-4);
            Assert.AreEqual(0.0264, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-4);

            Assert.AreEqual(9063.0779, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M3], 0.01);

            Assert.AreEqual(-4226.1826, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M2], 0.01);
        }

        /// <summary>
        /// 2 beam con end release assiale in end 1
        /// </summary>
        [TestMethod]
        public void EndReleaseAxialEnd1Test1()
        {
            double E = 1000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);
            double h = 2.0;
            double b = 1.0;
            double t = 0.2;
            Section sec = new SectionRHS(h, b, t, t, t, t, true, mat, "rhsSec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.AxialU1 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double fx = 1000;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, fx, 0, 0, 0, 0, 0);
            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(9.6154, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DX), 1e-4);
            Assert.AreEqual(-fx, fem.GetReaction(nds[0],DOF.DX));
            Assert.AreEqual(0.0, fem.GetReaction(nds[2], DOF.DX));

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.AxialU1));
            Assert.AreEqual(9.6154 / 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(9.6154, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.AxialU1), 1e-4);

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.AxialU1));
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.AxialU1));
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.AxialU1));
        }

        /// <summary>
        /// 2 beam cpm end release in end 2
        /// </summary>
        [TestMethod]
        public void EndReleaseAxialEnd2Test1()
        {
            double E = 1000.0;
            double ni = 0;           

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double h = 2.0;
            double b = 1.0;
            double t = 0.2;
            Section sec = new SectionRHS(h, b, t, t, t, t, true, mat, "rhsSec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            beams[0].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] { Beam.LocalDOF.AxialU1 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double fx = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, fx, 0, 0, 0, 0, 0);
            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(9.6154, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DX), 1e-4);
            Assert.AreEqual(0.0, fem.GetReaction(nds[0], DOF.DX));
            Assert.AreEqual(-fx, fem.GetReaction(nds[2], DOF.DX));

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.AxialU1));
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.AxialU1), 1e-4);

            Assert.AreEqual(9.6154, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(9.6154 / 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.AxialU1));
        }

        [TestMethod]
        public void EndReleaseAxialTest1()
        {
            double E = 1000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double h = 2.0;
            double b = 1.0;
            double t = 0.2;
            Section sec = new SectionRHS(h, b, t, t, t, t, true, mat, "rhsSec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.AxialU1 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double fx = 100.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, fx, 0, 0, 0, 0, 0);
            nds[1].AddAttribute(f);
            nds[2].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
            fix2.AddExternalRestrain(Solver.DOF.DY);
            fix2.AddExternalRestrain(Solver.DOF.DZ);
            fix2.AddExternalRestrain(Solver.DOF.RX);
            fix2.AddExternalRestrain(Solver.DOF.RY);
            fix2.AddExternalRestrain(Solver.DOF.RZ);
            nds.ForEach(x => x.AddAttribute(fix2));

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.961538, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DX), 1e-4);
            Assert.AreEqual(-fx, fem.GetReaction(nds[0], DOF.DX));
            Assert.AreEqual(-fx, fem.GetReaction(nds[3], DOF.DX));

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.AxialU1));
            Assert.AreEqual(0.480769, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.961538, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.AxialU1), 1e-4);

            Assert.AreEqual(0.96154, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.96154 , fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.961538, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.AxialU1), 1e-4);

            Assert.AreEqual(0.961538, fem.GetBeamDisplacementInLocalCoordinates(beams[2], 0.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.480769, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[1].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[1].L, Beam.LocalDOF.AxialU1), 1e-4);
        }

        [TestMethod]
        public void AxialUniformLoadTest2()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double qValue = 1;
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qValue, 0, 0);
            beams[1].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
            fix2.AddExternalRestrain(Solver.DOF.DY);
            fix2.AddExternalRestrain(Solver.DOF.DZ);
            fix2.AddExternalRestrain(Solver.DOF.RX);
            fix2.AddExternalRestrain(Solver.DOF.RY);
            fix2.AddExternalRestrain(Solver.DOF.RZ);
            nds.ForEach(x => x.AddAttribute(fix2));

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.AxialU1));
            Assert.AreEqual(0.003183, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.006366, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.AxialU1), 1e-4);

            Assert.AreEqual(0.006366, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.007560, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 1.0 / 4.0 * beams[1].L, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.007958, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 2.0 / 4.0 * beams[1].L, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.007560, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 3.0 / 4.0 * beams[1].L, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.006366, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.AxialU1), 1e-4);

            Assert.AreEqual(0.006366, fem.GetBeamDisplacementInLocalCoordinates(beams[2], 0.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.003183, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[1].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[1].L, Beam.LocalDOF.AxialU1), 1e-4);
        }

        [TestMethod]
        public void EndReleaseAxialUniformLoadTest1()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.AxialU1 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double qValue = 1;
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qValue, 0, 0);
            beams[1].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
            fix2.AddExternalRestrain(Solver.DOF.DY);
            fix2.AddExternalRestrain(Solver.DOF.DZ);
            fix2.AddExternalRestrain(Solver.DOF.RX);
            fix2.AddExternalRestrain(Solver.DOF.RY);
            fix2.AddExternalRestrain(Solver.DOF.RZ);
            nds.ForEach(x => x.AddAttribute(fix2));

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.AxialU1));
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.AxialU1), 1e-4);

            Assert.AreEqual(0.019099, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.017507, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.012732, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.AxialU1), 1e-4);

            Assert.AreEqual(0.012732, fem.GetBeamDisplacementInLocalCoordinates(beams[2], 0.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.006366, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[1].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[1].L, Beam.LocalDOF.AxialU1), 1e-4);
        }

        [TestMethod]
        public void EndReleaseAxialUniformLoadTest2()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] { Beam.LocalDOF.AxialU1 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double qValue = 1;
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qValue, 0, 0);
            beams[1].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
            fix2.AddExternalRestrain(Solver.DOF.DY);
            fix2.AddExternalRestrain(Solver.DOF.DZ);
            fix2.AddExternalRestrain(Solver.DOF.RX);
            fix2.AddExternalRestrain(Solver.DOF.RY);
            fix2.AddExternalRestrain(Solver.DOF.RZ);
            nds.ForEach(x => x.AddAttribute(fix2));

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.AxialU1));
            Assert.AreEqual(0.006366, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.012732, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.AxialU1), 1e-4);

            Assert.AreEqual(0.012732, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.017507, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.019099, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.AxialU1), 1e-4);

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], 0.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[1].L / 2.0, Beam.LocalDOF.AxialU1), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[1].L, Beam.LocalDOF.AxialU1), 1e-4);
        }

        [TestMethod]
        public void EndReleaseTorsionEnd1Test1()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.TorsionR1 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double torque = 100.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, 0, 0, torque, 0, 0);
            nds[1].AddAttribute(f);
            nds[2].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
            fix2.AddExternalRestrain(Solver.DOF.DX);
            fix2.AddExternalRestrain(Solver.DOF.DY);
            fix2.AddExternalRestrain(Solver.DOF.DZ);            
            fix2.AddExternalRestrain(Solver.DOF.RY);
            fix2.AddExternalRestrain(Solver.DOF.RZ);
            nds.ForEach(x => x.AddAttribute(fix2));

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(116.722004 * Math.PI / 180.0 / 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(116.722004 * Math.PI / 180.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.TorsionR1), 1e-4);

            Assert.AreEqual(116.722004 * Math.PI / 180.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(116.722004 * Math.PI / 180.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(116.722004 * Math.PI / 180.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.TorsionR1), 1e-4);

            Assert.AreEqual(116.722004 * Math.PI / 180.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], 0.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(116.722004 * Math.PI / 180.0 / 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[2].L / 2.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[2].L, Beam.LocalDOF.TorsionR1), 1e-4);
        }

        [TestMethod]
        public void EndReleaseTorsionEnd2Test1()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] { Beam.LocalDOF.TorsionR1 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double torque = 100.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, 0, 0, torque, 0, 0);
            nds[1].AddAttribute(f);
            nds[2].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
            fix2.AddExternalRestrain(Solver.DOF.DX);
            fix2.AddExternalRestrain(Solver.DOF.DY);
            fix2.AddExternalRestrain(Solver.DOF.DZ);
            fix2.AddExternalRestrain(Solver.DOF.RY);
            fix2.AddExternalRestrain(Solver.DOF.RZ);
            nds.ForEach(x => x.AddAttribute(fix2));

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(116.722004 * Math.PI / 180.0 / 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(116.722004 * Math.PI / 180.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.TorsionR1), 1e-4);

            Assert.AreEqual(116.722004 * Math.PI / 180.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(116.722004 * Math.PI / 180.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(116.722004 * Math.PI / 180.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.TorsionR1), 1e-4);

            Assert.AreEqual(116.722004 * Math.PI / 180.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], 0.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(116.722004 * Math.PI / 180.0 / 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[2].L / 2.0, Beam.LocalDOF.TorsionR1), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[2].L, Beam.LocalDOF.TorsionR1), 1e-4);
        }

        [TestMethod]
        public void EndReleaseShearEnd1Test1()
        {
            double E = 1000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.U2, Beam.LocalDOF.U3}, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double F = 10;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, F / 2.0, 0, 0, 0);
            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(42.441, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-3);
            Assert.AreEqual(42.441 / 2.0, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(-F, fem.GetReaction(nds[0], DOF.DY),1e-3);
            Assert.AreEqual(-F / 2.0, fem.GetReaction(nds[0], DOF.DZ),1e-3);

            Assert.AreEqual(75 / 2.0, fem.GetReaction(nds[0], DOF.RY),1e-3);
            Assert.AreEqual(-75, fem.GetReaction(nds[0], DOF.RZ),1e-3);

            Assert.AreEqual(0.0, fem.GetReaction(nds[2], DOF.DY));
            Assert.AreEqual(0.0, fem.GetReaction(nds[2], DOF.DZ));

            Assert.AreEqual(25.0 / 2.0, fem.GetReaction(nds[2], DOF.RY));
            Assert.AreEqual(-25.0, fem.GetReaction(nds[2], DOF.RZ));

            Assert.AreEqual(-12.7324, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-4);
            Assert.AreEqual(-3.1831, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L/2.0, Beam.LocalDOF.U3), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-4);

            Assert.AreEqual(-25.4648, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-4);
            Assert.AreEqual(-6.3662, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-4);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-4);
        }

        [TestMethod]
        public void EndReleaseShearEnd1UniformTest1()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.U2, Beam.LocalDOF.U3 }, "fc", "releaseName");

            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", 0, 1, 0.5);
            beams[1].AddLoadCaseAttribute(q);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(-3.961190, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-3);
            Assert.AreEqual(-1.980595, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(18.957122, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(14.058687, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L/2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(7.922379, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(9.478561, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(7.029343, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(3.961190, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);
        }

        [TestMethod]
        public void EndReleaseShearEnd2UniformTest1()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] { Beam.LocalDOF.U2, Beam.LocalDOF.U3 }, "fc", "releaseName");

            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", 0, 1, 0.5);
            beams[1].AddLoadCaseAttribute(q);

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(7.922379, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-3);
            Assert.AreEqual(3.961190, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(7.922379, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(14.058687, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(18.957122, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(3.961190, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(7.029343, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(9.478561, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);
        }

        [TestMethod]
        public void EndReleaseShearEnd1Test2()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.U2, Beam.LocalDOF.U3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double F = 10;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, F / 2.0, 0, 0, 0);
            nds[1].AddAttribute(f);

            double F2 = 15;
            NodeForceAttribute f2 = new NodeForceAttribute("lc", sys, 0, F2, F2 / 2.0, 0, 0, 0);
            nds[2].AddAttribute(f2);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());
                        
            Assert.AreEqual(8.2760, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(8.2760 / 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(4.244132, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(4.244132*2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(2.970892, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(2.970892*2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);
        }

        [TestMethod]
        public void EndReleaseShearEnd2Test1()
        {
            double E = 1000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            beams[0].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] { Beam.LocalDOF.U2, Beam.LocalDOF.U3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double F = 10;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, F/2.0, 0, 0, 0);
            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(42.441, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-3);
            Assert.AreEqual(42.441 / 2.0, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(0, fem.GetReaction(nds[0], DOF.DY), 1e-3);
            Assert.AreEqual(0, fem.GetReaction(nds[0], DOF.DZ), 1e-3);

            Assert.AreEqual(-25 / 2.0, fem.GetReaction(nds[0], DOF.RY), 1e-3);
            Assert.AreEqual(25, fem.GetReaction(nds[0], DOF.RZ), 1e-3);

            Assert.AreEqual(-F, fem.GetReaction(nds[2], DOF.DY), 1e-3);
            Assert.AreEqual(-F / 2.0, fem.GetReaction(nds[2], DOF.DZ), 1e-3);

            Assert.AreEqual(-75.0 / 2.0, fem.GetReaction(nds[2], DOF.RY), 1e-3);
            Assert.AreEqual(75.0, fem.GetReaction(nds[2], DOF.RZ), 1e-3);

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U3), 1e-4);
            Assert.AreEqual(-6.3662 / 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U3), 1e-4);
            Assert.AreEqual(-25.4648 / 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U3), 1e-4);

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U2), 1e-4);
            Assert.AreEqual(-6.3662, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U2), 1e-4);
            Assert.AreEqual(-25.4648, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U2), 1e-4);
        }

        [TestMethod]
        public void EndReleaseShearEnd2Test2()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] { Beam.LocalDOF.U2, Beam.LocalDOF.U3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double F = 15;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, F / 2.0, 0, 0, 0);
            nds[1].AddAttribute(f);

            double F2 = 10;
            NodeForceAttribute f2 = new NodeForceAttribute("lc", sys, 0, F2, F2 / 2.0, 0, 0, 0);
            nds[2].AddAttribute(f2);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(4.138029, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(4.138029 * 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(2.970892, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(2.970892 * 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(4.244132, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(4.244132 * 2.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);
        }

        [TestMethod]
        public void EndReleaseRotationEnd1Test1()
        {
            double E = 1000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.R2, Beam.LocalDOF.R3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double F = 10.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, F, 0, 0, 0);
            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(33.953, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-3);
            Assert.AreEqual(33.953, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(-5.0, fem.GetReaction(nds[0], DOF.DY), 1e-3);
            Assert.AreEqual(-5.0, fem.GetReaction(nds[0], DOF.DZ), 1e-3);

            Assert.AreEqual(50.0, fem.GetReaction(nds[0], DOF.RY), 1e-3);
            Assert.AreEqual(-50.0, fem.GetReaction(nds[0], DOF.RZ), 1e-3);

            Assert.AreEqual(-5.0, fem.GetReaction(nds[2], DOF.DY), 1e-3);
            Assert.AreEqual(-5.0, fem.GetReaction(nds[2], DOF.DZ), 1e-3);

            Assert.AreEqual(-50.0, fem.GetReaction(nds[2], DOF.RY), 1e-3);
            Assert.AreEqual(50.0, fem.GetReaction(nds[2], DOF.RZ), 1e-3);

            Assert.AreEqual(33.953, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(10.6103, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(33.953, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(10.6103, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(33.953, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(10.6103, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(33.953, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(10.6103, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);
        }

        [TestMethod]
        public void EndReleaseRotationEnd1Test2()
        {
            double E = 10.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, L, 0));
            nds.Add(new Node(0.0, 2.0 * L, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            beams[0].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.R2, Beam.LocalDOF.R3 }, "fc", "releaseName");
            //beams[0].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] { Beam.LocalDOF.R2, Beam.LocalDOF.R3 }, "fc", "releaseName");

            //beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.R2, Beam.LocalDOF.R3 }, "fc", "releaseName");
            //beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] { Beam.LocalDOF.R2, Beam.LocalDOF.R3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double F = 10.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, F, 0, F/2.0, 0, 0, 0);
            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
            fix2.AddExternalRestrain(Solver.DOF.RY);

            nds[1].AddAttribute(fix2);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(1485.446136, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DX), 1e-3);
            Assert.AreEqual(742.723068, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(-3.125, fem.GetReaction(nds[0], DOF.DX), 1e-3);
            Assert.AreEqual(-1.5625, fem.GetReaction(nds[0], DOF.DZ), 1e-3);

            Assert.AreEqual(-6.875, fem.GetReaction(nds[2], DOF.DX), 1e-3);
            Assert.AreEqual(-3.4375, fem.GetReaction(nds[2], DOF.DZ), 1e-3);
            Assert.AreEqual(18.75, fem.GetReaction(nds[2], DOF.RX), 1e-3);
            Assert.AreEqual(-37.5, fem.GetReaction(nds[2], DOF.RZ), 1e-3);

            Assert.AreEqual(0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(-1140.610, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(-1485.446, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(570.3052, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(742.723, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(-1485.446, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(-663.145, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(742.723, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(331.572, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);
        }

        [TestMethod]
        public void EndReleaseRotationEnd2Test1()
        {
            double E = 1000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            beams[0].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] { Beam.LocalDOF.R2, Beam.LocalDOF.R3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double F = 10.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, F, 0, 0, 0);
            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(33.953, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-3);
            Assert.AreEqual(33.953, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(-5.0, fem.GetReaction(nds[0], DOF.DY), 1e-3);
            Assert.AreEqual(-5.0, fem.GetReaction(nds[0], DOF.DZ), 1e-3);

            Assert.AreEqual(50.0, fem.GetReaction(nds[0], DOF.RY), 1e-3);
            Assert.AreEqual(-50.0, fem.GetReaction(nds[0], DOF.RZ), 1e-3);

            Assert.AreEqual(-5.0, fem.GetReaction(nds[2], DOF.DY), 1e-3);
            Assert.AreEqual(-5.0, fem.GetReaction(nds[2], DOF.DZ), 1e-3);

            Assert.AreEqual(-50.0, fem.GetReaction(nds[2], DOF.RY), 1e-3);
            Assert.AreEqual(50.0, fem.GetReaction(nds[2], DOF.RZ), 1e-3);

            Assert.AreEqual(33.953, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(10.6103, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(33.953, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(10.6103, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(33.953, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(10.6103, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(33.953, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(10.6103, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);
        }

        [TestMethod]
        public void EndReleaseRotationEnd2Test2()
        {
            double E = 10.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, L, 0));
            nds.Add(new Node(0.0, 2.0 * L, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            //beams[0].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.R2, Beam.LocalDOF.R3 }, "fc", "releaseName");
            //beams[0].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] { Beam.LocalDOF.R2, Beam.LocalDOF.R3 }, "fc", "releaseName");

            //beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] { Beam.LocalDOF.R2, Beam.LocalDOF.R3 }, "fc", "releaseName");
            beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] { Beam.LocalDOF.R2, Beam.LocalDOF.R3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double F = 10.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, F, 0, F / 2.0, 0, 0, 0);
            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
            fix2.AddExternalRestrain(Solver.DOF.RY);

            nds[1].AddAttribute(fix2);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(1485.446136, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DX), 1e-3);
            Assert.AreEqual(742.723068, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(-6.875, fem.GetReaction(nds[0], DOF.DX), 1e-3);
            Assert.AreEqual(-3.4375, fem.GetReaction(nds[0], DOF.DZ), 1e-3);
            Assert.AreEqual(-18.75, fem.GetReaction(nds[0], DOF.RX), 1e-3);
            Assert.AreEqual(37.5, fem.GetReaction(nds[0], DOF.RZ), 1e-3);

            Assert.AreEqual(-3.125, fem.GetReaction(nds[2], DOF.DX), 1e-3);
            Assert.AreEqual(-1.5625, fem.GetReaction(nds[2], DOF.DZ), 1e-3);

            Assert.AreEqual(-1485.446, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(-663.145, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(742.723, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(331.572, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(-1140.610, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(-1485.446, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(742.723, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(570.3052, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);            
        }

        [TestMethod]
        public void EndReleaseShearAndRotationEnd1Test1()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] {
                Beam.LocalDOF.U2,
                Beam.LocalDOF.U3,
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double F = 10.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F / 2.0, F, 0, 0, 0);
            nds[1].AddAttribute(f);
            nds[2].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(3.395305, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-3);
            Assert.AreEqual(6.790611, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(3.395305, fem.GetDisplacementGlobalCoordinates(nds[2], DOF.DY), 1e-3);
            Assert.AreEqual(6.790611, fem.GetDisplacementGlobalCoordinates(nds[2], DOF.DZ), 1e-3);

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(1.061033, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(3.395305, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(2.0 * 1.061033, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(2.0 * 3.395305, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(8.488264, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(5.941785, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(3.395305, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(16.976527, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(11.883569, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(6.790611, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);
        }

        [TestMethod]
        public void EndReleaseShearAndRotationEnd2Test1()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] {
                Beam.LocalDOF.U2,
                Beam.LocalDOF.U3,
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            double F = 10.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F / 2.0, F, 0, 0, 0);
            nds[1].AddAttribute(f);
            nds[2].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(3.395305, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-3);
            Assert.AreEqual(6.790611, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(3.395305, fem.GetDisplacementGlobalCoordinates(nds[2], DOF.DY), 1e-3);
            Assert.AreEqual(6.790611, fem.GetDisplacementGlobalCoordinates(nds[2], DOF.DZ), 1e-3);

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(1.061033, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(3.395305, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(2.0 * 1.061033, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(2.0 * 3.395305, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(3.395305, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(5.941785, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(8.488264, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(6.790611, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(11.883569, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(16.976527, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);
        }

        [TestMethod]
        public void EndReleaseShearAndRotationEnd1UniformLoadTest1()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] {
                Beam.LocalDOF.U2,
                Beam.LocalDOF.U3,
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", 0, 1, 2);
            beams[1].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-3);
            Assert.AreEqual(0.0, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(11.883569, fem.GetDisplacementGlobalCoordinates(nds[2], DOF.DY), 1e-3);
            Assert.AreEqual(23.767138, fem.GetDisplacementGlobalCoordinates(nds[2], DOF.DZ), 1e-3);

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(0, fem.GetBeamDisplacementInLocalCoordinates(beams[0], beams[0].L, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(69.603762, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(45.942727, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(23.767138, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(34.801881, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(22.971363, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(11.883569, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);
        }

        [TestMethod]
        public void EndReleaseShearAndRotationEnd2UniformLoadTest1()
        {
            double E = 10000.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] {
                Beam.LocalDOF.U2,
                Beam.LocalDOF.U3,
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", 0, 1, 2);
            beams[1].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetDisplacementGlobalCoordinates(nds[2], DOF.DY), 1e-3);
            Assert.AreEqual(0.0, fem.GetDisplacementGlobalCoordinates(nds[2], DOF.DZ), 1e-3);

            Assert.AreEqual(11.883569, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-3);
            Assert.AreEqual(23.767138, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DZ), 1e-3);

            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[2].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(0.0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[2].L, Beam.LocalDOF.U2), 1e-3);

            Assert.AreEqual(0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[0].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(0, fem.GetBeamDisplacementInLocalCoordinates(beams[2], beams[0].L, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(23.767138, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(45.942727, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U3), 1e-3);
            Assert.AreEqual(69.603762, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U3), 1e-3);

            Assert.AreEqual(11.883569, fem.GetBeamDisplacementInLocalCoordinates(beams[1], 0.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(22.971363, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L / 2.0, Beam.LocalDOF.U2), 1e-3);
            Assert.AreEqual(34.801881, fem.GetBeamDisplacementInLocalCoordinates(beams[1], beams[1].L, Beam.LocalDOF.U2), 1e-3);
        }

        /// <summary>
        /// 3 beam, the middle with uniform loading
        /// </summary>
        [TestMethod]
        public void DoubleEndReleaseTest3()
        {
            double E = 10.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] {
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3 }, "fc", "releaseName");

            beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] {
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3 }, "fc", "releaseName");

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            
            /*double F = 10.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, F, 0, 0, 0);
            nds[1].AddAttribute(f);*/

            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", 0.0, 1.0, 1.0);

            beams[1].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(3660.567, fem.GetBeamDisplacementInGlobalCoordinates(beams[1], beams[1].L/2.0, DOF.DY), 0.01);
        }

        /// <summary>
        /// 3 beam, the middle with uniform loading
        /// </summary>
        [TestMethod]
        public void EndReleaseRotationEnd1Test3()
        {
            double E = 10.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End1, new Beam.LocalDOF[] {
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3 }, "fc", "releaseName");

            /*beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] {
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3 }, "fc", "releaseName");*/

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            /*double F = 10.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, F, 0, 0, 0);
            nds[1].AddAttribute(f);*/

            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", 0.0, 1.0, 1.0);

            beams[1].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(3395.3054, fem.GetBeamDisplacementInGlobalCoordinates(beams[1], beams[1].L / 2.0, DOF.DY), 0.01);
            Assert.AreEqual(3395.3054, fem.GetBeamDisplacementInGlobalCoordinates(beams[1], beams[1].L / 2.0, DOF.DZ), 0.01);
        }

        /// <summary>
        /// 3 beam, the middle with uniform loading
        /// </summary>
        [TestMethod]
        public void EndReleaseRotationEnd2Test3()
        {
            double E = 10.0;
            double ni = 0;

            Material mat = new SteelMaterial("m", E, ni, 355, 510, 7850);

            double D = 1;
            double t = D / 2;
            Section sec = new SectionCHS(D, t, mat, "sec");

            double L = 10;
            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));
            nds.Add(new Node(2.0 * L, 0, 0));
            nds.Add(new Node(3.0 * L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[3] }, sec));

            beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] {
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3 }, "fc", "releaseName");

            /*beams[1].AddEndRelease(Beam.EndSide.End2, new Beam.LocalDOF[] {
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3 }, "fc", "releaseName");*/

            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            /*double F = 10.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, F, 0, 0, 0);
            nds[1].AddAttribute(f);*/

            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", 0.0, 1.0, 1.0);

            beams[1].AddLoadCaseAttribute(q);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(Solver.DOF.DX);
            fix.AddExternalRestrain(Solver.DOF.DY);
            fix.AddExternalRestrain(Solver.DOF.DZ);
            fix.AddExternalRestrain(Solver.DOF.RX);
            fix.AddExternalRestrain(Solver.DOF.RY);
            fix.AddExternalRestrain(Solver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[3].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(3395.3054, fem.GetBeamDisplacementInGlobalCoordinates(beams[1], beams[1].L / 2.0, DOF.DY), 0.01);
            Assert.AreEqual(3395.3054, fem.GetBeamDisplacementInGlobalCoordinates(beams[1], beams[1].L / 2.0, DOF.DZ), 0.01);
        }
    }
}
