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

namespace FemTest.Solver { 
    [TestClass]
    public class BeamTest {

        /// <summary>
        /// Sforzo Assiale su trave incastro - libero - direzione +X
        /// </summary>
        [TestMethod]
        public void Axial1()
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

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 1000;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, 0.0, 0.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(FX / (E * A) * L, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX), 1e-6);

            Assert.AreEqual(FX, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.N]);
            Assert.AreEqual(FX, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.N]);

            var reactNode0 = fem.GetReaction(nds[0]);
        }

        /// <summary>
        /// Sforzo uniforme Assiale su trave incastro - libero - direzione +X
        /// </summary>
        [TestMethod]
        public void Axial2()
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

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double qx = 1;
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, 0, 0);

            b.AddLoadCaseAttribute(q);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(qx * L / 2.0 / (E * A) * L, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX), 1e-6);
            Assert.AreEqual(-qx * L, fem.GetReaction(nds[0], DOF.DX));
            Assert.AreEqual(0.0, fem.GetReaction(nds[1], DOF.DX));

            Assert.AreEqual(qx*L, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.N]);
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.N]);
        }

        /// <summary>
        /// Momento flettente nodale su trave incastro - libero - direzione trave +X
        /// </summary>
        [TestMethod]
        public void BendingTest1()
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
            double MZ = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, 0.0, 0.0, 0, MY, MZ);

            nds[1].AddAttribute(f);

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(MZ * L*L / (2.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-4);

            Assert.AreEqual(MZ, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M3],1e-6);
            Assert.AreEqual(MZ, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.M3], 1e-6);

            Assert.AreEqual(-MY, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M2], 1e-6);
            Assert.AreEqual(-MY, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.M2], 1e-6);
        }

        /// <summary>
        /// Momento flettente nodale su trave incastro - libero - direzione -Y
        /// </summary>
        [TestMethod]
        public void BendingTest2()
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

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, 0.0, 0.0, -1000.0, 0.0, 0.0);

            double MX = -1000;
            NodeForceAttribute m = new NodeForceAttribute("lc", sys, 0.0, 0.0, 0.0, MX, 0.0, 0.0);

            nds[1].AddAttribute(m);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(1.0 / 2.0 * -MX * L*L / (E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DZ), 1e-4);

            Assert.AreEqual(-MX, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M2], 1e-6);
            Assert.AreEqual(-MX, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.M2], 1e-6);
        }

        /// <summary>
        /// Forza di taglio su trave incastro - libero - direzione +X
        /// </summary>
        [TestMethod]
        public void ShearForceTest1()
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

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FY = 10.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, FY, 0.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(DOF.DX);
            fix.AddExternalRestrain(DOF.DY);
            fix.AddExternalRestrain(DOF.DZ);
            fix.AddExternalRestrain(DOF.RX);
            fix.AddExternalRestrain(DOF.RY);
            fix.AddExternalRestrain(DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(1.0 / 3.0 * FY * L*L*L/(E*sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-4);

            Assert.AreEqual(-FY, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.V2]);
            Assert.AreEqual(-FY, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.V2]);
        }

        [TestMethod]
        public void ShearForceTest2()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 10.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, 0, 1000.0));

            EulerBeam b = new EulerBeam(nds.ToArray(), sec);

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double FX = 10/2.0;
            double FY = 10;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, FX, FY, 0.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(67.9061/2, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX), 1e-4);
            Assert.AreEqual(67.9061, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-4);

            Assert.AreEqual(-FY, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.V2]);
            Assert.AreEqual(-FY, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.V2]);

            Assert.AreEqual(FX, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.V3]);
            Assert.AreEqual(FX, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.V3]);
        }

        [TestMethod]
        public void ShearForceTest3()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 10.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(0, 1000.0, 0));

            EulerBeam b = new EulerBeam(nds.ToArray(), sec);

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 10.0, 0.0, 10.0, 0.0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
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

        /// <summary>
        /// cantilever sottoposto a torsione
        /// </summary>
        [TestMethod]
        public void TorsionTest1()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 10.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(1000, 0, 0));

            EulerBeam b = new EulerBeam(nds.ToArray(), sec);

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double MX = 1000.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, 0.0, 0.0, MX, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(1.1672, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.RX) * 180.0 / Math.PI, 1e-4);

            Assert.AreEqual(MX, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.T]);
            Assert.AreEqual(MX, fem.GetBeamInternalForces(b, b.L)[Beam.InternalAction.T]);
        }

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

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = 1.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0.0, -F, 0.0, 0, 0.0, 0.0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute support = new NodeRestrainAttribute("fc", sys);
            support.AddExternalRestrain(LinearSolver.DOF.DX);
            support.AddExternalRestrain(LinearSolver.DOF.DY);
            support.AddExternalRestrain(LinearSolver.DOF.DZ);
            support.AddExternalRestrain(LinearSolver.DOF.RX);

            nds[0].AddAttribute(support);
            nds[2].AddAttribute(support);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(-33.9531, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-4);

            Assert.AreEqual(0, fem.GetBeamInternalForces(beams[0], 0)[Beam.InternalAction.M3]);
            Assert.AreEqual(F*L/4.0, fem.GetBeamInternalForces(beams[0], beams[0].L)[Beam.InternalAction.M3], 1e-6);

            Assert.AreEqual(F / 2.0, fem.GetBeamInternalForces(beams[0], 0.0)[Beam.InternalAction.V2], 1e-6);
            Assert.AreEqual(F / 2.0, fem.GetBeamInternalForces(beams[0], beams[0].L)[Beam.InternalAction.V2], 1e-6);

            Assert.AreEqual(F * L /4.0, fem.GetBeamInternalForces(beams[1], 0)[Beam.InternalAction.M3], 1e-6);
            Assert.AreEqual(0.0, fem.GetBeamInternalForces(beams[1], beams[1].L)[Beam.InternalAction.M3], 1e-6);

            Assert.AreEqual(-F/2.0, fem.GetBeamInternalForces(beams[1],0)[Beam.InternalAction.V2], 1e-6);
            Assert.AreEqual(-F / 2.0, fem.GetBeamInternalForces(beams[1], beams[1].L)[Beam.InternalAction.V2], 1e-6);
        }

        [TestMethod]
        public void EndReleaseTest1()
        {
            Section sec = new SectionCHS(100.0, 50.0, new SteelMaterial("m", 1.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(1000, 0, 0));
            nds.Add(new Node(1000, 1000, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = 1.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, F, 0, F, 0, 0, 0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            //add release
            beams[0].AddEndRelease(2, new Beam.LocalDOF[] {
                Beam.LocalDOF.AxialU1,
                Beam.LocalDOF.U2,
                Beam.LocalDOF.U3,
                Beam.LocalDOF.TorsionR1,
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3
                },
                "fc", "rel1"); ;
            beams[0].BuildMatrix();

            FEMUtilities.WriteMatrix(beams[0].KElementGlobalCoord);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(67.9061, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX), 1e-4);
            Assert.AreEqual(67.9061, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DZ), 1e-4);
        }

        [TestMethod]
        public void ArcTest1()
        {
            Section sec = new SectionCHS(10.0, 5.0, new SteelMaterial("m", 1000.0, 0.0, 355, 510, 7850), "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(1000, 300, 0));
            nds.Add(new Node(2000, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[2], nds[1] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            double F = -100.0;
            NodeForceAttribute f = new NodeForceAttribute("lc", sys, 0, F, 0, 0, 0, 0);

            nds[1].AddAttribute(f);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[2].AddAttribute(fix);

            //add release
            Beam.LocalDOF[] hinge = new Beam.LocalDOF[] {
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3
                };
            beams[0].AddEndRelease(1, hinge, "fc", "rel");
            beams[0].AddEndRelease(2, hinge, "fc", "rel");
            beams[1].AddEndRelease(1, hinge, "fc", "rel");

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(-8.0497, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-2);
        }

        /// <summary>
        /// Cantilever with uniform load
        /// </summary>
        [TestMethod]
        public void AppliedDistributedLoadTest1()
        {
            double E = 100000.0;
            Section sec = new SectionRHS(100.0, 100.0, 49.99, 49.99, 49.99, 49.99, false, new SteelMaterial("m", E, 0.0, 355, 510, 7850), "sec");
            double A = sec.Area;
            Console.WriteLine("A = " + sec.Area);

            double L = 1000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));

            LoadCase lc = new LoadCase("lc1");
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

            Assert.AreEqual( (qx * L/2) / E / sec.Area * L, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX),1e-6); //displacement
            Assert.AreEqual(-qx * L, fem.GetReaction(nds[0], LinearSolver.DOF.DX), 1e-2); //reaction

            Assert.AreEqual(-qy * L, fem.GetReaction(nds[0], LinearSolver.DOF.DY), 0.001); //Shear reaction
            Assert.AreEqual(-qy * L * L / 2.0, fem.GetReaction(nds[0], LinearSolver.DOF.RZ), 0.001); //Bending Moment reaction

            Assert.AreEqual(qy * Math.Pow(L, 4.0) / (8.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 0.001); //displacement
            Assert.AreEqual(qy * Math.Pow(L, 3.0) / (6.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.RZ), 0.001); //rotation

            Assert.AreEqual(-qz * L, fem.GetReaction(nds[0], LinearSolver.DOF.DZ), 0.001); //Shear reaction
            Assert.AreEqual(qz * L * L / 2.0, fem.GetReaction(nds[0], LinearSolver.DOF.RY), 0.001); //Bending Moment reaction

            Assert.AreEqual(qz * Math.Pow(L, 4.0) / (8.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DZ), 0.001); //displacement
            Assert.AreEqual(-qz * Math.Pow(L, 3.0) / (6.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.RY), 0.001); //rotation

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
            double A = sec.Area;
            Console.WriteLine("A = " + sec.Area);

            double L = 1000.0;

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(L/2.0, 0, 0));
            nds.Add(new Node(2.0 * L/2.0, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            beams.Add(new EulerBeam(new Node[] { nds[0], nds[1] }, sec));
            beams.Add(new EulerBeam(new Node[] { nds[1], nds[2] }, sec));

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            Random random = new Random();
            double qx = 1; //random.Next(-1000, 1000);
            double qy = 2; //random.Next(-1000, 1000);
            double qz = 3;// random.Next(-1000, 1000);
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, qy, qz);
            beams[0].AddLoadCaseAttribute(q);
            beams[1].AddLoadCaseAttribute(q);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute("fc", sys);
            hinge.AddExternalRestrain(LinearSolver.DOF.DX);
            hinge.AddExternalRestrain(LinearSolver.DOF.DY);
            hinge.AddExternalRestrain(LinearSolver.DOF.DZ);
            hinge.AddExternalRestrain(LinearSolver.DOF.RX);

            nds[0].AddAttribute(hinge);
            nds[2].AddAttribute(hinge);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual((qx * L) / E / sec.Area / 8.0 * L, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DX), 1e-6); //displacement
            Assert.AreEqual(-qx * L / 2.0, fem.GetReaction(nds[0], LinearSolver.DOF.DX), 1e-2); //reaction

            Assert.AreEqual(-qy * L / 2.0, fem.GetReaction(nds[0], LinearSolver.DOF.DY), 0.001); //Shear
            Assert.AreEqual(-qy * L * L / 8.0, fem.GetBeamInternalForces(beams[0],beams[0].L)[Beam.InternalAction.M3], 1e-6); //Bending Moment
            Assert.AreEqual(-qy * L * L / 8.0, fem.GetBeamInternalForces(beams[1],0)[Beam.InternalAction.M3], 1e-6); //Bending Moment

            Assert.AreEqual(5.0 / 384.0 * qy * Math.Pow(L, 4.0) / (E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 0.001); //displacement
            Assert.AreEqual(qy * Math.Pow(L, 3.0) / (24.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[0], LinearSolver.DOF.RZ), 0.001); //rotation
            Assert.AreEqual(-qy * Math.Pow(L, 3.0) / (24.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[2], LinearSolver.DOF.RZ), 0.001); //rotation

            Assert.AreEqual(-qz * L / 2.0, fem.GetReaction(nds[0], LinearSolver.DOF.DZ), 0.001); //Shear

            Assert.AreEqual(5.0 / 384.0 * qz * Math.Pow(L, 4.0) / (E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DZ), 0.001); //displacement
            Assert.AreEqual(-qz * Math.Pow(L, 3.0) / (24.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[0], LinearSolver.DOF.RY), 0.001); //rotation
            Assert.AreEqual(qz * Math.Pow(L, 3.0) / (24.0 * E * sec.J22), fem.GetDisplacementGlobalCoordinates(nds[2], LinearSolver.DOF.RY), 0.001); //rotation
        }

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

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            Random random = new Random();
            double qx = 0; //random.Next(-1000, 1000);
            double qy = 1; //random.Next(-1000, 1000);
            double qz = 2;// random.Next(-1000, 1000);
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, qy, qz);
            beams[0].AddLoadCaseAttribute(q);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);
            nds[1].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-6); //displacement
            Assert.AreEqual(-qy * L /2.0, fem.GetReaction(nds[0], LinearSolver.DOF.DY), 1e-2); //reaction

            Assert.AreEqual(-1.0 / 24.0 * qy * L * L, fem.GetBeamInternalForces(beams[0], beams[0].L / 2.0, Beam.InternalAction.M3));
        }

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

            LoadCase lc = new LoadCase("lc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            Random random = new Random();
            double qx = 0; //random.Next(-1000, 1000);
            double qy = 1; //random.Next(-1000, 1000);
            double qz = 1;// random.Next(-1000, 1000);
            BeamDistribuitedLoadAttribute q = new BeamDistribuitedLoadAttribute("lc", qx, qy, qz);
            beams[0].AddLoadCaseAttribute(q);

            FreedomCase fc = new FreedomCase("fc");
            NodeRestrainAttribute hinge = new NodeRestrainAttribute("fc", sys);
            hinge.AddExternalRestrain(LinearSolver.DOF.DX);
            hinge.AddExternalRestrain(LinearSolver.DOF.DY);
            hinge.AddExternalRestrain(LinearSolver.DOF.DZ);
            hinge.AddExternalRestrain(LinearSolver.DOF.RX);

            nds[0].AddAttribute(hinge);
            nds[1].AddAttribute(hinge);

            LinearSolver fem = new LinearSolver(beams.ToArray());

            Assert.AreEqual(0.0, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-6); //displacement
            Assert.AreEqual(-qy * L / 2.0, fem.GetReaction(nds[0], LinearSolver.DOF.DY), 1e-2); //reaction

            Assert.AreEqual(-1.0 / 8.0 * qy * L * L, fem.GetBeamInternalForces(beams[0],beams[0].L/2.0)[Beam.InternalAction.M3],1e-6);

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
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

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
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

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
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nds[0].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { b });

            Assert.AreEqual(0.0154, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DX), 1e-4);
            Assert.AreEqual(0.0264, fem.GetDisplacementGlobalCoordinates(nds[1], DOF.DY), 1e-4);

            Assert.AreEqual(9063.0779, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M3], 0.01);

            Assert.AreEqual(-4226.1826, fem.GetBeamInternalForces(b, 0)[Beam.InternalAction.M2], 0.01);
        }
    }
}
