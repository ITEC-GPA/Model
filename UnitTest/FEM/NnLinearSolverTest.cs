using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GPC.Model.Fem.FiniteElements;

using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.Materials;
using GPC.Model.FreedomCases;
using GPC.Geometry;
using GPC.Model.Fem.Properties;
using GPC.Model.Fem.Attributes;
using GPC.Model.LoadCases;
using GPC.Model.Sections;

namespace FemTest.SolverTest
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
            var sec = new SectionCHS(10.0, 5.0, "sec");

            List<Node> nds = new List<Node>();
            nds.Add(new Node(0, 0, 0));
            nds.Add(new Node(1000, 300, 0));
            nds.Add(new Node(2000, 0, 0));

            List<EulerBeam> beams = new List<EulerBeam>();
            EulerBeam eulerBeam1 = new EulerBeam(new Node[] { nds[0], nds[1] });
            eulerBeam1.SetProperty(sec);
            EulerBeam eulerBeam2 = new EulerBeam(new Node[] { nds[2], nds[3] });
            eulerBeam2.SetProperty(sec);
            beams.Add(eulerBeam1);
            beams.Add(eulerBeam2);

            LoadCaseBase lc = new LoadCaseBase("lc1");
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
            Beam.LocalDOF[] hinge = new Beam.LocalDOF[] {
                Beam.LocalDOF.R2,
                Beam.LocalDOF.R3
                };
            beams[0].AddEndRelease(1, hinge, "fc", "rel");
            beams[0].AddEndRelease(2, hinge, "fc", "rel");
            beams[1].AddEndRelease(1, hinge, "fc", "rel");

            NonLinearStaticSolver fem = new NonLinearStaticSolver(beams.ToArray());

            //Assert.AreEqual(-662.13331, fem.GetDisplacementGlobalCoordinates(nds[1], LinearSolver.DOF.DY), 1e-2);
            Assert.AreEqual(true, false); //non funziona
        }
    }
}