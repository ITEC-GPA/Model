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
using GPC.Model.Sections;

namespace FemTest.Solver
{
    [TestClass]
    public class NnLinearSolverTest {
        [TestMethod]
        public void OneDimensionTest1()
        {

        }

        [TestMethod]
        public void ArcTestNnLin1()
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
            double F = -1000.0;
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
            BeamReleasesAttribute.LocalDOF[] hinge = new BeamReleasesAttribute.LocalDOF[] {
                BeamReleasesAttribute.LocalDOF.R2,
                BeamReleasesAttribute.LocalDOF.R3
                };
            beams[0].AddRelease(1, hinge, "fc", "rel");
            beams[0].AddRelease(2, hinge, "fc", "rel");
            beams[1].AddRelease(1, hinge, "fc", "rel");

            NnLinearStaticSolver fem = new NnLinearStaticSolver(beams.ToArray());

            //Assert.AreEqual(-662.13331, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-2);
            Assert.AreEqual(true, false); //non funziona
        }
    }
}