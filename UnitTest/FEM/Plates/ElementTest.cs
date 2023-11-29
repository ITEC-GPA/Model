//using GPC.Geometry;

//using GPC.Model.Fem.Attributes;
//using GPC.Model.Fem.FiniteElements;
//using GPC.Model.Fem.Properties;
//using GPC.Model.FreedomCases;
//using GPC.Model.LoadCases;
//using GPC.Model.Materials;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Collections.Generic;
//using System.Linq;

//namespace FemTest.SolverTest
//{
//    [TestClass]
//    public class ElementTest
//    {
//        [TestMethod]
//        public void Quad4Test1()
//        {
//            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510);
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 1.0, 1.0, "p");

//            Node[] nodesPlate1 = new Node[4];
//            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
//            nodesPlate1[1] = new Node(+1.0, 0, 0, "2");
//            nodesPlate1[2] = new Node(+2.0, +2, 0, "3");
//            nodesPlate1[3] = new Node(0.0, +1, 0, "4");

//            Plate e0 = new Quad4Element(nodesPlate1);
//            e0.SetProperty(prop);

//            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase");
//            FreedomCase freedomCase = new FreedomCase("freedomCase1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
//            PlatePressureAttribute pressure = new PlatePressureAttribute("loadCase", sys, 0.0, 0.0, 1.0);
//            e0.AddLoadCaseAttribute(pressure);

//            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0 });

//            Assert.AreEqual(0.4167, fem.F[2], 0.001); //UX, UY, UZ
//            Assert.AreEqual(0.50, fem.F[8], 0.001);
//            Assert.AreEqual(0.5833, fem.F[14], 0.001);
//            Assert.AreEqual(0.50, fem.F[20], 0.001);
//        }

//        [TestMethod]
//        public void Quad4ToQuad8NodesTest1()
//        {
//            Node[] nodes = new Node[4];
//            nodes[1 - 1] = new Node(0.0, 0.0, 0.0);
//            nodes[2 - 1] = new Node(1.0, 0.0, 0.0);
//            nodes[3 - 1] = new Node(1.0, 1.0, 0.0);
//            nodes[4 - 1] = new Node(0.0, 1.0, 0.0);

//            Node[] nodes8 = Quad4Element.Get8Nodes(nodes);

//            Assert.AreEqual(0.0, nodes8[1 - 1].Position.X);
//            Assert.AreEqual(0.0, nodes8[1 - 1].Position.Y);

//            Assert.AreEqual(1.0, nodes8[2 - 1].Position.X);
//            Assert.AreEqual(0.0, nodes8[2 - 1].Position.Y);

//            Assert.AreEqual(1.0, nodes8[3 - 1].Position.X);
//            Assert.AreEqual(1.0, nodes8[3 - 1].Position.Y);

//            Assert.AreEqual(0.0, nodes8[4 - 1].Position.X);
//            Assert.AreEqual(1.0, nodes8[4 - 1].Position.Y);

//            Assert.AreEqual(0.5, nodes8[5 - 1].Position.X);
//            Assert.AreEqual(0.0, nodes8[5 - 1].Position.Y);

//            Assert.AreEqual(1.0, nodes8[6 - 1].Position.X);
//            Assert.AreEqual(0.5, nodes8[6 - 1].Position.Y);

//            Assert.AreEqual(0.5, nodes8[7 - 1].Position.X);
//            Assert.AreEqual(1.0, nodes8[7 - 1].Position.Y);

//            Assert.AreEqual(0.0, nodes8[8 - 1].Position.X);
//            Assert.AreEqual(0.5, nodes8[8 - 1].Position.Y);
//        }

//        [TestMethod]
//        public void Quad4GetLocalNodesTest1()
//        {
//            Node[] nodes = new Node[4];
//            nodes[1 - 1] = new Node(0.0, 0.0, 0.0);
//            nodes[2 - 1] = new Node(1.0, 0.0, 0.0);
//            nodes[3 - 1] = new Node(1.0, 1.0, 0.0);
//            nodes[4 - 1] = new Node(0.0, 1.0, 0.0);

//            Node[] localNodes = Quad4Element.GetLocalNodes(nodes, out CoordinateSystem sys);

//            Assert.AreEqual(0.0, localNodes[1 - 1].Position.X);
//            Assert.AreEqual(0.0, localNodes[1 - 1].Position.Y);

//            Assert.AreEqual(1.0, localNodes[2 - 1].Position.X);
//            Assert.AreEqual(0.0, localNodes[2 - 1].Position.Y);

//            Assert.AreEqual(1.0, localNodes[3 - 1].Position.X);
//            Assert.AreEqual(1.0, localNodes[3 - 1].Position.Y);

//            Assert.AreEqual(0.0, localNodes[4 - 1].Position.X);
//            Assert.AreEqual(1.0, localNodes[4 - 1].Position.Y);

//            //anticlock wise nodes
//            nodes[1 - 1] = new Node(0.0, 0.0, 0.0);
//            nodes[2 - 1] = new Node(0.0, 1.0, 0.0);
//            nodes[3 - 1] = new Node(1.0, 1.0, 0.0);
//            nodes[4 - 1] = new Node(1.0, 0.0, 0.0);

//            localNodes = Quad4Element.GetLocalNodes(nodes, out sys);

//            Assert.AreEqual(0.0, localNodes[1 - 1].Position.X);
//            Assert.AreEqual(0.0, localNodes[1 - 1].Position.Y);

//            Assert.AreEqual(1.0, localNodes[2 - 1].Position.X);
//            Assert.AreEqual(0.0, localNodes[2 - 1].Position.Y);

//            Assert.AreEqual(1.0, localNodes[3 - 1].Position.X);
//            Assert.AreEqual(1.0, localNodes[3 - 1].Position.Y);

//            Assert.AreEqual(0.0, localNodes[4 - 1].Position.X);
//            Assert.AreEqual(1.0, localNodes[4 - 1].Position.Y);
//        }

//        [TestMethod]
//        public void Tri3ElementTest1()
//        {
//            //TODO: aggiornare per calcolo tensioni
//            Material mat = new SteelMaterial("mat", 10000, 0.0, 355, 510);
//            double t = 1.0;
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), t, t, "p");

//            #region restrains
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
//            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
//            fix.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix.AddExternalRestrain(LinearSolver.DOF.DY);
//            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix.AddExternalRestrain(LinearSolver.DOF.RY);
//            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

//            NodeRestrainAttribute fixRZ = new NodeRestrainAttribute("freedomCase", sys);
//            fixRZ.AddExternalRestrain(LinearSolver.DOF.RZ);
//            #endregion

//            #region nodalforces
//            NodeForceAttribute F = new NodeForceAttribute("loadCase", sys, 0.0, 1.0, 0.0, 1.0, 0, 0);
//            #endregion

//            Node nodeA = new Node(0.0, 8, 0, "A");
//            nodeA.AddAttribute(fixRZ);
//            nodeA.AddAttribute(F);

//            Node nodeB = new Node(0.0, 0, 0, "B");
//            nodeB.AddAttribute(fix);

//            Node nodeC = new Node(8.0, 8, 0, "C");
//            nodeC.AddAttribute(F);
//            nodeC.AddAttribute(fixRZ);

//            Node nodeD = new Node(8.0, 0, 0, "D");
//            nodeD.AddAttribute(fix);

//            FiniteElement e0 = new Tri3Element(new Node[] { nodeA, nodeB, nodeC });
//            e0.SetProperty(prop);

//            Plate e1 = new Tri3Element(new Node[] { nodeB, nodeD, nodeC });
//            e1.SetProperty(prop);

//            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0, e1 });

//            double DY = fem.GetNodeDisplacementGlobalCoordinates(nodeC, Solver.DOF.DY);
//            double DZ = fem.GetNodeDisplacementGlobalCoordinates(nodeC, Solver.DOF.DZ);
//            Assert.AreEqual(0.0096, DZ, 1e-4);
//            Assert.AreEqual(0.0002, DY, 1e-4);

//            double sigmaTopYY = -(F.M1 + F.M1) / (1.0 / 6.0 * 8.0 * (t * t)) + (F.F2 + F.F2) / (t * 8.0);

//            double[] e0GlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(e0);
//            //TODO: test positivo sistemare
//            /*e0.GetNodesResults(e0GlobalDispl,
//                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
//                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
//                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
//                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
//            Assert.AreEqual(sigmaTopYY, globalStress[0][1,1], 0.001); //sigmaYY top face

//            double[] e1GlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(e1);
//            e1.GetNodesResults(e1GlobalDispl,
//                            out globalPseudoDef, out localPseudoDef,
//                            out globalForces, out localForces,
//                            out globalStress, out localStress,
//                            out globalEpsilon, out localEpsilon);
//            Assert.AreEqual(sigmaTopYY, globalStress[0][1, 1], 0.001); //sigmaYY top face
//            */
//        }

//        [TestMethod]
//        public void Tri3ElementTest2()
//        {
//            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase");
//            FreedomCase freedomCase = new FreedomCase("freedomCase1");

//            Material mat = new SteelMaterial("mat", 10000, 0.0, 355, 510);
//            double t = 1.0;
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), t, t, "p");

//            #region restrains
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
//            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
//            fix.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix.AddExternalRestrain(LinearSolver.DOF.DY);
//            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix.AddExternalRestrain(LinearSolver.DOF.RY);
//            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

//            NodeRestrainAttribute fixRZ = new NodeRestrainAttribute("freedomCase", sys);
//            fixRZ.AddExternalRestrain(LinearSolver.DOF.RZ);
//            #endregion

//            #region forces
//            PlatePressureAttribute p = new PlatePressureAttribute("loadCase", sys, 0.0, 0.0, 1.0);
//            #endregion

//            Node nodeA = new Node(0.0, 8, 0, "A");

//            nodeA.AddAttribute(fixRZ);

//            Node nodeB = new Node(0.0, 0, 0, "B");
//            ;
//            nodeB.AddAttribute(fix);

//            Node nodeC = new Node(8.0, 8, 0, "C");

//            nodeC.AddAttribute(fixRZ);

//            Node nodeD = new Node(8.0, 0, 0, "D");

//            nodeD.AddAttribute(fix);

//            Plate e0 = new Tri3Element(new Node[] { nodeA, nodeB, nodeC });
//            e0.SetProperty(prop);

//            e0.AddLoadCaseAttribute(p);
//            Plate e1 = new Tri3Element(new Node[] { nodeB, nodeD, nodeC });
//            e1.SetProperty(prop);

//            e1.AddLoadCaseAttribute(p);
//            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0, e1 });

//            double DZC = fem.GetNodeDisplacementGlobalCoordinates(nodeC, LinearSolver.DOF.DZ);
//            Assert.AreEqual(0.97765, DZC, 1e-4); //value from SAP
//            double DZA = fem.GetNodeDisplacementGlobalCoordinates(nodeA, LinearSolver.DOF.DZ);
//            Assert.AreEqual(0.75597, DZA, 1e-4); //value from SAP

//            double[] e0GlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(e0);
//            /*e0.GetResults(e0GlobalDispl, out double[] localDispl,
//                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
//                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
//                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
//                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
//            */
//        }

//        [TestMethod]
//        public void Quad4Test2()
//        {
//            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510);
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 1.0, 1.0, "p");

//            Node[] nodesPlate1 = new Node[4];
//            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
//            nodesPlate1[1] = new Node(+1.0, 0, 0, "2");
//            nodesPlate1[2] = new Node(+1.0, +1, 0, "3");
//            nodesPlate1[3] = new Node(0.0, +1, 0, "4");

//            FiniteElement e0 = new Quad4Element(nodesPlate1);
//            e0.SetProperty(prop);

//            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase");
//            FreedomCase freedomCase = new FreedomCase("freedomCase1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            /*PlatePressureAttribute pressure = new PlatePressureAttribute(loadCase, sys, 0.0, 0.0, 1.0);
//            e0.AddLoadCaseAttribute(pressure);*/

//            NodeForceAttribute F = new NodeForceAttribute("loadCase", sys, 0, 0, 1, 0, 0, 0);
//            nodesPlate1[2].AddAttribute(F);
//            nodesPlate1[3].AddAttribute(F);

//            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
//            fix.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix.AddExternalRestrain(LinearSolver.DOF.DY);
//            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix.AddExternalRestrain(LinearSolver.DOF.RY);
//            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

//            nodesPlate1[0].AddAttribute(fix);
//            nodesPlate1[1].AddAttribute(fix);

//            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0 });

//            Assert.AreEqual(0.667, fem.GetNodeDisplacementGlobalCoordinates(nodesPlate1[2], LinearSolver.DOF.DZ), 0.001);
//        }

//        [TestMethod]
//        public void Quad4Test3()
//        {
//            Material mat = new SteelMaterial("mat", 72000, 0.23, 355, 510);
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 6.13, 6.13, "p");

//            List<Node> nodes = new List<Node>();
//            nodes.Add(new Node(-1e6, -1e6, -1e6));
//            nodes.Add(new Node(330, -100, 0));
//            nodes.Add(new Node(330, 100, 0));
//            nodes.Add(new Node(15, 100, 0));
//            nodes.Add(new Node(15, -100, 0));

//            List<Quad4Element> els = new List<Quad4Element>();
//            els.Add(new Quad4Element(new Node[] { nodes[1], nodes[2], nodes[3], nodes[4] }, prop));

//            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase");
//            FreedomCase freedomCase = new FreedomCase("freedomCase1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            /*PlatePressureAttribute pressure = new PlatePressureAttribute(loadCase, sys, 0.0, 0.0, 1.0);
//            e0.AddLoadCaseAttribute(pressure);*/

//            NodeForceAttribute F = new NodeForceAttribute("loadCase", sys, 0, 0, 8.0 * 9.81 / 2 / 2, 0, 0, 0);

//            nodes.Where(x => x.Position.X == 15).ToList().ForEach(x => x.AddAttribute(F));

//            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
//            fix.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix.AddExternalRestrain(LinearSolver.DOF.DY);
//            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix.AddExternalRestrain(LinearSolver.DOF.RY);
//            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

//            nodes.Where(x => x.Position.X == 330).ToList().ForEach(x => x.AddAttribute(fix));

//            LinearSolver fem = new LinearSolver(els.ToArray());

//            Assert.AreEqual(1.4196, fem.GetNodeDisplacementGlobalCoordinates(nodes[3], LinearSolver.DOF.DZ), 0.001);
//        }

//        [TestMethod]
//        public void Quad4Test4()
//        {
//            Material mat = new SteelMaterial("mat", 72000, 0.23, 355, 510);
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 6.13, 6.13, "p");

//            List<Node> nodes = new List<Node>();
//            nodes.Add(new Node(-1e6, -1e6, -1e6));
//            nodes.Add(new Node(330, -100, 0));
//            nodes.Add(new Node(330, 100, 0));
//            nodes.Add(new Node(15, 100, 0));
//            nodes.Add(new Node(15, -100, 0));
//            nodes.Add(new Node(330, 0, 0));
//            nodes.Add(new Node(172.5, -100, 0));
//            nodes.Add(new Node(172.5, 0, 0));
//            nodes.Add(new Node(172.5, 100, 0));
//            nodes.Add(new Node(15, 0, 0));

//            List<Quad4Element> els = new List<Quad4Element>();
//            els.Add(new Quad4Element(new Node[] { nodes[7], nodes[8], nodes[3], nodes[9] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[1], nodes[5], nodes[7], nodes[6] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[5], nodes[2], nodes[8], nodes[7] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[6], nodes[7], nodes[9], nodes[4] }, prop));

//            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase");
//            FreedomCase freedomCase = new FreedomCase("freedomCase1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            /*PlatePressureAttribute pressure = new PlatePressureAttribute(loadCase, sys, 0.0, 0.0, 1.0);
//            e0.AddLoadCaseAttribute(pressure);*/

//            NodeForceAttribute F = new NodeForceAttribute("loadCase", sys, 0, 0, 8.0 * 9.81 / 2 / 3, 0, 0, 0);

//            nodes.Where(x => x.Position.X == 15).ToList().ForEach(x => x.AddAttribute(F));

//            NodeRestrainAttribute fix = new NodeRestrainAttribute("freedomCase", sys);
//            fix.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix.AddExternalRestrain(LinearSolver.DOF.DY);
//            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix.AddExternalRestrain(LinearSolver.DOF.RY);
//            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

//            nodes.Where(x => x.Position.X == 330).ToList().ForEach(x => x.AddAttribute(fix));

//            LinearSolver fem = new LinearSolver(els.ToArray());

//            Assert.AreEqual(1.4314, fem.GetNodeDisplacementGlobalCoordinates(nodes.Where(x => x.Position.X == 15 && x.Position.Y == 0).First(), LinearSolver.DOF.DZ), 0.001);
//        }

//        [TestMethod]
//        public void Quad4Test5()
//        {
//            Material mat = new SteelMaterial("mat", 72000, 0.23, 355, 510);
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), 6.13, 6.13, "p");

//            List<Node> nodes = new List<Node>();
//            nodes.Add(new Node(-1e6, -1e6, -1e6));
//            nodes.Add(new Node(330, -100, 0));
//            nodes.Add(new Node(330, 100, 0));
//            nodes.Add(new Node(15, 100, 0));
//            nodes.Add(new Node(15, -100, 0));
//            nodes.Add(new Node(330, 0, 0));
//            nodes.Add(new Node(172.5, -100, 0));
//            nodes.Add(new Node(172.5, 0, 0));
//            nodes.Add(new Node(172.5, 100, 0));
//            nodes.Add(new Node(15, 0, 0));
//            nodes.Add(new Node(0, 100, 0));
//            nodes.Add(new Node(0, 0, 0));
//            nodes.Add(new Node(0, -100, 0));
//            nodes.Add(new Node(365, 100, 0));
//            nodes.Add(new Node(365, 0, 0));
//            nodes.Add(new Node(365, -100, 0));

//            List<Quad4Element> els = new List<Quad4Element>();
//            els.Add(new Quad4Element(new Node[] { nodes[7], nodes[8], nodes[3], nodes[9] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[1], nodes[5], nodes[7], nodes[6] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[5], nodes[2], nodes[8], nodes[7] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[6], nodes[7], nodes[9], nodes[4] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[3], nodes[9], nodes[11], nodes[10] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[9], nodes[4], nodes[12], nodes[11] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[2], nodes[5], nodes[14], nodes[13] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[5], nodes[1], nodes[15], nodes[14] }, prop));

//            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase");
//            FreedomCase freedomCase = new FreedomCase("freedomCase1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            /*PlatePressureAttribute pressure = new PlatePressureAttribute(loadCase, sys, 0.0, 0.0, 1.0);
//            e0.AddLoadCaseAttribute(pressure);*/

//            NodeForceAttribute F = new NodeForceAttribute("loadCase", sys, 0, 0, 8.0 * 9.81 / 2.0 / 3.0, 0, 0, 0);

//            nodes.Where(x => x.Position.X == 15).ToList().ForEach(x => x.AddAttribute(F));

//            NodeRestrainAttribute x0Restrain = new NodeRestrainAttribute("freedomCase", sys);
//            x0Restrain.AddExternalRestrain(LinearSolver.DOF.DX);
//            x0Restrain.AddExternalRestrain(LinearSolver.DOF.RY);
//            nodes.Where(x => x.Position.X == 0).ToList().ForEach(x => x.AddAttribute(x0Restrain));

//            NodeRestrainAttribute x0y0Restrain = new NodeRestrainAttribute("freedomCase", sys);
//            x0y0Restrain.AddExternalRestrain(LinearSolver.DOF.DX);
//            x0y0Restrain.AddExternalRestrain(LinearSolver.DOF.DY);
//            x0y0Restrain.AddExternalRestrain(LinearSolver.DOF.RY);
//            nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).ToList().ForEach(x => x.AddAttribute(x0y0Restrain));

//            NodeRestrainAttribute x330y0Restrain = new NodeRestrainAttribute("freedomCase", sys);
//            x330y0Restrain.AddExternalRestrain(LinearSolver.DOF.DY);
//            x330y0Restrain.AddExternalRestrain(LinearSolver.DOF.DZ);
//            nodes.Where(x => x.Position.X == 330 && x.Position.Y == 0).ToList().ForEach(x => x.AddAttribute(x330y0Restrain));

//            NodeRestrainAttribute x330Restrain = new NodeRestrainAttribute("freedomCase", sys);
//            x330Restrain.AddExternalRestrain(LinearSolver.DOF.DZ);
//            nodes.Where(x => x.Position.X == 330).ToList().ForEach(x => x.AddAttribute(x330Restrain));

//            LinearSolver fem = new LinearSolver(els.ToArray());

//            Assert.AreEqual(1.6686, fem.GetNodeDisplacementGlobalCoordinates(nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).First(), LinearSolver.DOF.DZ), 0.001);
//        }

//        [TestMethod]
//        public void Quad4Test6()
//        {
//            double t = 6.13;
//            Material mat = new SteelMaterial("mat", 72000, 0.23, 355, 510);
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), t, t, "p");

//            List<Node> nodes = new List<Node>();
//            nodes.Add(new Node(-1e6, -1e6, -1e6));
//            nodes.Add(new Node(330, -100, 0));
//            nodes.Add(new Node(330, 100, 0));
//            nodes.Add(new Node(15, 100, 0));
//            nodes.Add(new Node(15, -100, 0));
//            nodes.Add(new Node(330, 0, 0));
//            nodes.Add(new Node(172.5, -100, 0));
//            nodes.Add(new Node(172.5, 0, 0));
//            nodes.Add(new Node(172.5, 100, 0));
//            nodes.Add(new Node(15, 0, 0));
//            nodes.Add(new Node(0, 100, 0));
//            nodes.Add(new Node(0, 0, 0));
//            nodes.Add(new Node(0, -100, 0));
//            nodes.Add(new Node(365, 100, 0));
//            nodes.Add(new Node(365, 0, 0));
//            nodes.Add(new Node(365, -100, 0));

//            List<Quad4Element> els = new List<Quad4Element>();
//            els.Add(new Quad4Element(new Node[] { nodes[7], nodes[8], nodes[3], nodes[9] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[1], nodes[5], nodes[7], nodes[6] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[5], nodes[2], nodes[8], nodes[7] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[6], nodes[7], nodes[9], nodes[4] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[3], nodes[9], nodes[11], nodes[10] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[9], nodes[4], nodes[12], nodes[11] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[2], nodes[5], nodes[14], nodes[13] }, prop));
//            els.Add(new Quad4Element(new Node[] { nodes[5], nodes[1], nodes[15], nodes[14] }, prop));

//            LoadCaseBase loadCase = new LoadCaseBase("myLoadCase");
//            FreedomCase freedomCase = new FreedomCase("freedomCase1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            PlatePressureAttribute pressure = new PlatePressureAttribute("loadCase", sys, 0.0, 0.0, (2.418 / 1000.0 * 9.81) * (t / 1000.0));
//            els.ForEach(x => x.AddLoadCaseAttribute(pressure));

//            /*NodeForceAttribute F = new NodeForceAttribute(loadCase, sys, 0, 0, 8.0 * 9.81 / 2.0 / 3.0, 0, 0, 0);

//            nodes.Where(x => x.Position.X == 15).ToList().ForEach(x => x.AddAttribute(F));*/

//            NodeRestrainAttribute x0Restrain = new NodeRestrainAttribute("freedomCase", sys);
//            x0Restrain.AddExternalRestrain(LinearSolver.DOF.DX);
//            x0Restrain.AddExternalRestrain(LinearSolver.DOF.RY);
//            nodes.Where(x => x.Position.X == 0).ToList().ForEach(x => x.AddAttribute(x0Restrain));

//            NodeRestrainAttribute x0y0Restrain = new NodeRestrainAttribute("freedomCase", sys);
//            x0y0Restrain.AddExternalRestrain(LinearSolver.DOF.DX);
//            x0y0Restrain.AddExternalRestrain(LinearSolver.DOF.DY);
//            x0y0Restrain.AddExternalRestrain(LinearSolver.DOF.RY);
//            nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).ToList().ForEach(x => x.AddAttribute(x0y0Restrain));

//            NodeRestrainAttribute x330y0Restrain = new NodeRestrainAttribute("freedomCase", sys);
//            x330y0Restrain.AddExternalRestrain(LinearSolver.DOF.DY);
//            x330y0Restrain.AddExternalRestrain(LinearSolver.DOF.DZ);
//            nodes.Where(x => x.Position.X == 330 && x.Position.Y == 0).ToList().ForEach(x => x.AddAttribute(x330y0Restrain));

//            NodeRestrainAttribute x330Restrain = new NodeRestrainAttribute("freedomCase", sys);
//            x330Restrain.AddExternalRestrain(LinearSolver.DOF.DZ);
//            nodes.Where(x => x.Position.X == 330).ToList().ForEach(x => x.AddAttribute(x330Restrain));

//            LinearSolver fem = new LinearSolver(els.ToArray());

//            Assert.AreEqual(0.2415, fem.GetNodeDisplacementGlobalCoordinates(nodes.Where(x => x.Position.X == 0 && x.Position.Y == 0).First(), LinearSolver.DOF.DZ), 0.001);
//        }

//        [TestMethod]
//        public void Quad4AreaTest1()
//        {
//            Node[] nodesPlate1 = new Node[4];
//            nodesPlate1[0] = new Node(0.0, 0.0, 0);
//            nodesPlate1[1] = new Node(1.0, 0.0, 0);
//            nodesPlate1[2] = new Node(1.0, 1.0, 0);
//            nodesPlate1[3] = new Node(0.0, 1.0, 0);

//            Plate e0 = new Quad4Element(nodesPlate1);

//            Assert.AreEqual(1.0, e0.GetArea());
//        }

//        [TestMethod]
//        public void Quad4AreaTest2()
//        {
//            Node[] nodesPlate1 = new Node[4];
//            nodesPlate1[0] = new Node(0.0, 0.0, 0);
//            nodesPlate1[1] = new Node(2.0, 0.0, 0);
//            nodesPlate1[2] = new Node(2.0, 1.0, 0);
//            nodesPlate1[3] = new Node(0.0, 1.0, 0);

//            Plate e0 = new Quad4Element(nodesPlate1);

//            Assert.AreEqual(2.0, e0.GetArea());
//        }

//        [TestMethod]
//        public void Quad4AreaTest3()
//        {
//            Node[] nodesPlate1 = new Node[4];
//            nodesPlate1[0] = new Node(0.0, 0.0, 0);
//            nodesPlate1[1] = new Node(1.0, 0.0, 0);
//            nodesPlate1[2] = new Node(2.0, 2.0, 0);
//            nodesPlate1[3] = new Node(1.0, 2.0, 0);

//            Plate e0 = new Quad4Element(nodesPlate1);

//            Assert.AreEqual(2.0, e0.GetArea());
//        }

//        [TestMethod]
//        public void Quad4AreaTest4()
//        {
//            Node[] nodesPlate1 = new Node[4];
//            nodesPlate1[0] = new Node(0.0, 0.0, 0);
//            nodesPlate1[1] = new Node(1.0, 2.0, 0);
//            nodesPlate1[2] = new Node(2.0, 2.0, 0);
//            nodesPlate1[3] = new Node(1.0, 0.0, 0);

//            Plate e0 = new Quad4Element(nodesPlate1);

//            Assert.AreEqual(2.0, e0.GetArea(), 1e-3);
//        }
//    }
//}