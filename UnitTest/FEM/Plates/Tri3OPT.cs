//using System;
//using System.Linq;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Collections.Generic;
//using GPC.Model.Fem.FiniteElements;

//using mnl = MathNet.Numerics.LinearAlgebra;
//using GPC.Model.Elements;
//using GPC.Model.Materials;
//using GPC.Model.FreedomCases;
//using GPC.Geometry;
//using GPC.Model.Fem.Properties;
//using GPC.Model.Fem.Attributes;
//using GPC.Model.LoadCases;

//namespace FemTest.SolverTest
//{
//    [TestClass]
//    public class Tri3OPTTest
//    {
//        /// <summary>
//        /// Based of "A study of optima membrane triangles with drilling freedoms" - Felippa - 2003 pg. 22
//        /// </summary>
//        [TestMethod]
//        public void TriOPTMembranalTest1()
//        {
//            double E = 120.0;
//            double ni = 1.0 / 4.0;
//            Material mat = new SteelMaterial("steel", E, ni, 355, 510);

//            double thickness = 1.0 / 8.0;
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), thickness, thickness, "p");

//            Node[] nds = new Node[3];
//            nds[0] = new Node(+0.00, +0.00, 0.0, "1");
//            nds[1] = new Node(+4.08, -3.44, 0.0, "2");
//            nds[2] = new Node(+3.40, +1.14, 0.0, "3");

//            Tri3OPTMembrane el = new Tri3OPTMembrane(nds, prop);

//            LinearSolver fem = new LinearSolver(new FiniteElement[] { el });
//            Console.WriteLine("kGlob=" + fem.KGlobal);

//            mnl.Matrix<double> kGlobalTest = mnl.Matrix<double>.Build.Dense(0, 18);
//            kGlobalTest = kGlobalTest.InsertRow(0, mnl.Vector<double>.Build.Dense(new double[] { 10.392, 0.67281, 0, 0, 0, 7.0955, -1.9232, 1.8325, 0, 0, 0, 1.6467, -8.4687, -2.5053, 0, 0, 0, -10.739 }));
//            kGlobalTest = kGlobalTest.InsertRow(1, mnl.Vector<double>.Build.Dense(new double[] { 0.67281, 5.9602, 0, 0, 0, 12.462, 1.364,-0.29577, 0, 0, 0,-2.1909,-2.0368,-5.6644, 0, 0, 0, 3.1805 }));
//            kGlobalTest = kGlobalTest.InsertRow(2, mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
//            kGlobalTest = kGlobalTest.InsertRow(3, mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
//            kGlobalTest = kGlobalTest.InsertRow(4, mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
//            kGlobalTest = kGlobalTest.InsertRow(5, mnl.Vector<double>.Build.Dense(new double[] { 7.0955, 12.462, 0, 0, 0, 32.557, 0.75498, 0.26929, 0, 0, 0,-1.7792,-7.8504,-12.731, 0, 0, 0,-0.13702 }));
//            kGlobalTest = kGlobalTest.InsertRow(6, mnl.Vector<double>.Build.Dense(new double[] { -1.9232, 1.364, 0, 0, 0, 0.75498, 3.7959,-0.83734, 0, 0, 0,-6.7570,-1.8728,-0.52670, 0, 0, 0,-3.9838 }));
//            kGlobalTest = kGlobalTest.InsertRow(7, mnl.Vector<double>.Build.Dense(new double[] { 1.8325,-0.29577, 0, 0, 0, 0.26929,-0.83734, 6.0125, 0, 0, 0,-5.5238,-0.99520,-5.7167, 0, 0, 0, 1.9063 }));
//            kGlobalTest = kGlobalTest.InsertRow(8, mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
//            kGlobalTest = kGlobalTest.InsertRow(9, mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
//            kGlobalTest = kGlobalTest.InsertRow(10, mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
//            kGlobalTest = kGlobalTest.InsertRow(11, mnl.Vector<double>.Build.Dense(new double[] { 1.6467,-2.1909, 0, 0, 0,-1.7792,-6.7570,-5.5238, 0, 0, 0, 21.728, 5.1102, 7.7147, 0, 0, 0, 5.4278 }));
//            kGlobalTest = kGlobalTest.InsertRow(12, mnl.Vector<double>.Build.Dense(new double[] {  -8.4687,-2.0368, 0, 0, 0,-7.8504,-1.8728,-0.99520, 0, 0, 0, 5.1102, 10.341, 3.032, 0, 0, 0, 14.723 }));
//            kGlobalTest = kGlobalTest.InsertRow(13, mnl.Vector<double>.Build.Dense(new double[] { -2.5053,-5.6644, 0, 0, 0,-12.731,-0.52670,-5.7167, 0, 0, 0, 7.7147, 3.032, 11.381, 0, 0, 0,-5.0869 }));
//            kGlobalTest = kGlobalTest.InsertRow(14, mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
//            kGlobalTest = kGlobalTest.InsertRow(15, mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
//            kGlobalTest = kGlobalTest.InsertRow(16, mnl.Vector<double>.Build.Dense(new double[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }));
//            kGlobalTest = kGlobalTest.InsertRow(17, mnl.Vector<double>.Build.Dense(new double[] { -10.739, 3.1805, 0, 0, 0,-0.13702,-3.9838, 1.9063, 0, 0, 0, 5.4278, 14.723,-5.0869, 0, 0, 0, 34.716 }));

//            for (int r = 0; r < kGlobalTest.RowCount; r++)
//            {
//                for (int c = 0; c < kGlobalTest.ColumnCount; c++)
//                {
//                    Assert.AreEqual(kGlobalTest[r, c], fem.KGlobal[r, c], 0.001);
//                }
//            }
//        }

//        /// <summary>
//        /// Based of "A study of optima membrane triangles with drilling freedoms" - Felippa - 2003 pg. 32
//        /// </summary>
//        [TestMethod]
//        public void TriOPTMembranalTest2()
//        {
//            double E = 768.0;
//            double ni = 0.0;
//            Material mat = new SteelMaterial("steel", E, ni, 355, 510);

//            double thickness = 1.0;
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), thickness, thickness, "p");

//            Node node1 = new Node(0.0, 0.0, 0.0, "1");
//            Node node2 = new Node(16.0, 0.0, 0.0, "2");
//            Node node3 = new Node(0.0, 1.0, 0.0, "3");
//            Node node4 = new Node(16.0, 1.0, 0.0, "4");
//            Node node5 = new Node(0.0, 2.0, 0.0, "5");
//            Node node6 = new Node(16.0, 2.0, 0.0, "6");
//            Node node7 = new Node(32.0, 0.0, 0.0, "7");
//            Node node8 = new Node(32.0, 1.0, 0.0, "8");
//            Node node9 = new Node(32.0, 2.0, 0.0, "9");

//            FreedomCase fc = new FreedomCase("fc1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
//            fix.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix.AddExternalRestrain(LinearSolver.DOF.DY);
//            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

//            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix.AddExternalRestrain(LinearSolver.DOF.RY);

//            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
//            fix2.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RZ);

//            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RY);

//            NodeRestrainAttribute fix3 = new NodeRestrainAttribute("fc", sys);
//            fix3.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix3.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix3.AddExternalRestrain(LinearSolver.DOF.RY);

//            node1.AddAttribute(fix2);
//            node2.AddAttribute(fix3);
//            node3.AddAttribute(fix);
//            node4.AddAttribute(fix3);
//            node5.AddAttribute(fix2);
//            node6.AddAttribute(fix3);
//            node7.AddAttribute(fix3);
//            node8.AddAttribute(fix3);
//            node9.AddAttribute(fix3);

//            LoadCaseBase lc = new LoadCaseBase("lc");
//            NodeForceAttribute fPlus = new NodeForceAttribute("lc", sys, 50.0, 0.0, 0.0, 0.0, 0.0, 0.0);
//            NodeForceAttribute fMinus = new NodeForceAttribute("lc", sys, -50.0, 0.0, 0.0, 0.0, 0.0, 0.0);
//            node9.AddAttribute(fMinus);
//            node7.AddAttribute(fPlus);
//            /*NodeForceAttribute M = new NodeForceAttribute(lc, sys, 0.0, 0.0, 0.0, 0.0, 0.0, 100.0);
//            node8.AddAttribute(M);*/

//            List<Tri3OPTMembrane> elements = new List<Tri3OPTMembrane>();
//            elements.Add(new Tri3OPTMembrane(new Node[] { node1, node2, node3 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node3, node2, node4 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node3, node4, node5 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node5, node4, node6 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node2, node7, node4 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node4, node7, node8 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node4, node8, node6 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node6, node8, node9 }, prop));

//            LinearSolver fem = new LinearSolver(elements.ToArray());

//            Assert.AreEqual(100.0, fem.GetNodeDisplacementGlobalCoordinates(node8, LinearSolver.DOF.DY), 0.1);
//        }

//        /// <summary>
//        /// Based of "A study of optima membrane triangles with drilling freedoms" - Felippa - 2003 pg. 33-34
//        /// </summary>
//        [TestMethod]
//        public void TriOPTMembranalTest3()
//        {
//            double E = 30000.0;
//            double ni = 1.0 / 4.0;
//            Material mat = new SteelMaterial("steel", E, ni, 355, 510);

//            double thickness = 1.0;
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), thickness, thickness, "p");

//            Node node1 = new Node(0.0, 0.0, 0.0, "1");
//            Node node2 = new Node(0.0, 6.0, 0.0, "2");
//            Node node3 = new Node(0.0, 12.0, 0.0, "3");
//            Node node4 = new Node(24.0, 0.0, 0.0, "4");
//            Node node5 = new Node(24.0, 6.0, 0.0, "5");
//            Node node6 = new Node(24.0, 12.0, 0.0, "6");
//            Node node7 = new Node(48.0, 0.0, 0.0, "7");
//            Node node8 = new Node(48.0, 6.0, 0.0, "8");
//            Node node9 = new Node(48.0, 12.0, 0.0, "9");

//            FreedomCase fc = new FreedomCase("fc1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
//            fix.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix.AddExternalRestrain(LinearSolver.DOF.DY);
//            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

//            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix.AddExternalRestrain(LinearSolver.DOF.RY);

//            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
//            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RY);

//            node1.AddAttribute(fix);
//            node2.AddAttribute(fix);
//            node3.AddAttribute(fix);

//            node4.AddAttribute(fix2);
//            node5.AddAttribute(fix2);
//            node6.AddAttribute(fix2);
//            node7.AddAttribute(fix2);
//            node8.AddAttribute(fix2);
//            node9.AddAttribute(fix2);

//            LoadCaseBase lc = new LoadCaseBase("lc");
//            NodeForceAttribute fCentral = new NodeForceAttribute("lc", sys, 0.0, 40.0 / 0.35601 * 100.0 * 2.0 / 4.0, 0.0, 0.0, 0.0, 0.0);
//            NodeForceAttribute fLateral = new NodeForceAttribute("lc", sys, 0.0, 40.0 / 0.35601 * 100.0 * 1.0 / 4.0, 0.0, 0.0, 0.0, 0.0);
//            node7.AddAttribute(fLateral);
//            node8.AddAttribute(fCentral);
//            node9.AddAttribute(fLateral);

//            List<Tri3OPTMembrane> elements = new List<Tri3OPTMembrane>();
//            elements.Add(new Tri3OPTMembrane(new Node[] { node1, node4, node5 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node1, node5, node2 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node2, node5, node6 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node2, node6, node3 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node4, node7, node8 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node4, node8, node5 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node5, node8, node9 }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { node6, node9, node5 }, prop));

//            LinearSolver fem = new LinearSolver(elements.ToArray());

//            Assert.AreEqual(91.03, fem.GetNodeDisplacementGlobalCoordinates(node8, LinearSolver.DOF.DY), 0.1); //In article is 92.24

//            double[] elementGlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(elements[2]);
//            elements[2].GetNodesResults(elementGlobalDispl, out double[] localDispl,
//                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
//                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
//                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
//                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);

//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));            
//        }

//        /// <summary>
//        /// Based of "A study of optima membrane triangles with drilling freedoms" - Felippa - 2003 pg. 33-34
//        /// </summary>
//        [TestMethod]
//        public void TriOPTMembranalTest4()
//        {
//            double E = 30000.0;
//            double ni = 1.0 / 4.0;
//            Material mat = new SteelMaterial("steel", E, ni, 355, 510);

//            double thickness = 1.0;
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), thickness, thickness, "p");

//            List<Node> nodes = new List<Node>();
//            nodes.Add(new Node(-1000.0, -1000.0, -1000.0, "NULL"));
//            nodes.Add(new Node(0.0, -10.0, 0.0));
//            nodes.Add(new Node(24.0, -4.0, 0.0));
//            nodes.Add(new Node(0.0, -4.0, 0.0));
//            nodes.Add(new Node(48.0, -16.0, 0.0));
//            nodes.Add(new Node(48.0, -10.0, 0.0, "P"));
//            nodes.Add(new Node(24.0, -16.0, 0.0));
//            nodes.Add(new Node(24.0, -10.0, 0.0));
//            nodes.Add(new Node(48.0, -4.0, 0.0));
//            nodes.Add(new Node(0.0, -16.0, 0.0));
//            nodes.Add(new Node(0.0, -7.0, 0.0));
//            nodes.Add(new Node(12.0, -4.0, 0.0));
//            nodes.Add(new Node(12.0, -7.0, 0.0));
//            nodes.Add(new Node(12.0, -10.0, 0.0));
//            nodes.Add(new Node(24.0, -7.0, 0.0));
//            nodes.Add(new Node(48.0, -13.0, 0.0));
//            nodes.Add(new Node(36.0, -16.0, 0.0));
//            nodes.Add(new Node(36.0, -13.0, 0.0));
//            nodes.Add(new Node(36.0, -10.0, 0.0));
//            nodes.Add(new Node(24.0, -13.0, 0.0));
//            nodes.Add(new Node(48.0, -7.0, 0.0));
//            nodes.Add(new Node(36.0, -7.0, 0.0));
//            nodes.Add(new Node(36.0, -4.0, 0.0));
//            nodes.Add(new Node(12.0, -16.0, 0.0));
//            nodes.Add(new Node(12.0, -13.0, 0.0));
//            nodes.Add(new Node(0.0, -13.0, 0.0));

//            FreedomCase fc = new FreedomCase("fc1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
//            fix.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix.AddExternalRestrain(LinearSolver.DOF.DY);
//            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

//            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix.AddExternalRestrain(LinearSolver.DOF.RY);

//            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
//            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RY);

//            nodes[1].AddAttribute(fix);
//            nodes[3].AddAttribute(fix);
//            nodes[9].AddAttribute(fix);
//            nodes[10].AddAttribute(fix);
//            nodes[25].AddAttribute(fix);

//            nodes.ForEach(x => x.AddAttribute(fix2));

//            LoadCaseBase lc = new LoadCaseBase("lc");
//            NodeForceAttribute fCentral = new NodeForceAttribute("lc", sys, 0.0, 40.0 * 2.0 / 8.0, 0.0, 0.0, 0.0, 0.0);
//            NodeForceAttribute fLateral = new NodeForceAttribute("lc", sys, 0.0, 40.0 * 1.0 / 8.0, 0.0, 0.0, 0.0, 0.0);

//            nodes[4].AddAttribute(fLateral);
//            nodes[15].AddAttribute(fCentral);
//            nodes[5].AddAttribute(fCentral);
//            nodes[20].AddAttribute(fCentral);
//            nodes[8].AddAttribute(fLateral);

//            List<Tri3OPTMembrane> elements = new List<Tri3OPTMembrane>();
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[14], nodes[13], nodes[7] },   prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[19], nodes[18], nodes[7] },   prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[14], nodes[22], nodes[2] },   prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[25], nodes[13], nodes[1] },   prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[3], nodes[10], nodes[11] },   prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[11], nodes[10], nodes[12] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[10], nodes[1], nodes[12] },   prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[12], nodes[1], nodes[13] },   prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[11], nodes[12], nodes[2] },   prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[2], nodes[12], nodes[14] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[12], nodes[13], nodes[14] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[4], nodes[15], nodes[16] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[16], nodes[15], nodes[17] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[15], nodes[5], nodes[17] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[17], nodes[5], nodes[18] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[16], nodes[17], nodes[6] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[6], nodes[17], nodes[19] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[17], nodes[18], nodes[19] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[5], nodes[20], nodes[18] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[18], nodes[20], nodes[21] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[20], nodes[8], nodes[21] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[21], nodes[8], nodes[22] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[18], nodes[21], nodes[7] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[7], nodes[21], nodes[14] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[21], nodes[22], nodes[14] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[6], nodes[19], nodes[23] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[23], nodes[19], nodes[24] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[19], nodes[7], nodes[24] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[24], nodes[7], nodes[13] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[23], nodes[24], nodes[9] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[9], nodes[24], nodes[25] },  prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[24], nodes[13], nodes[25] }, prop));

//            LinearSolver fem = new LinearSolver(elements.ToArray());

//            double displacementNodeC = fem.GetNodeDisplacementGlobalCoordinates(nodes[5], LinearSolver.DOF.DY);
//            Console.WriteLine("DY node C = " + displacementNodeC);
//            Console.WriteLine(displacementNodeC / 0.35601 + " vs 1.0");


//            Console.WriteLine("Element 9");
//            double[] elementGlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(elements[9-1]);
//            elements[9-1].GetNodesResults(elementGlobalDispl, out double[] localDispl,
//                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
//                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
//                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
//                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);

//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

//            Console.WriteLine("Element 29");
//            elementGlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(elements[29 - 1]);
//            elements[29 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
//                            out globalPseudoDef, out localPseudoDef,
//                            out globalForces, out localForces,
//                            out globalStress, out localStress,
//                            out globalEpsilon, out localEpsilon);

//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

//            Console.WriteLine("Element 2");
//            elementGlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(elements[2 - 1]);
//            elements[2 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
//                            out globalPseudoDef, out localPseudoDef,
//                            out globalForces, out localForces,
//                            out globalStress, out localStress,
//                            out globalEpsilon, out localEpsilon);

//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

//            Assert.AreEqual(1.0, displacementNodeC / 0.35601, 0.1);
//        }

//        /// <summary>
//        ///Example not valid as test
//        /// </summary>
//        [TestMethod]
//        public void TriOPTMembranalExample5()
//        {
//            double E = 1.0;
//            double ni = 0.0;
//            Material mat = new SteelMaterial("steel", E, ni, 355, 510);

//            double thickness = 1.0;
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), thickness, thickness, "p");

//            List<Node> nodes = new List<Node>();
//            nodes.Add(new Node(0.0, 0.0, 0.0));
//            nodes.Add(new Node(1.0, 0.0, 0.0));
//            nodes.Add(new Node(0.0, 1.0, 0.0));
//            nodes.Add(new Node(1.0, 1.0, 0.0));
//            nodes.Add(new Node(0.5, 0.5, 0.0));

//            FreedomCase fc = new FreedomCase("fc1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
//            fix.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix.AddExternalRestrain(LinearSolver.DOF.DY);
//            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix.AddExternalRestrain(LinearSolver.DOF.RY);
//            //fix.AddExternalRestrain(LinearSolver.DOF.RZ);

//            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
//            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RY);
//            //fix2.AddExternalRestrain(LinearSolver.DOF.RZ);

//            nodes[1 - 1].AddAttribute(fix);
//            //nodes[2 - 1].AddAttribute(fix);
//            nodes[3 - 1].AddAttribute(fix);
//            //nodes[4 - 1].AddAttribute(fix);

//            nodes.ForEach(node => node.AddAttribute(fix2));

//            LoadCaseBase lc = new LoadCaseBase("lc");
//            NodeForceAttribute fOriz = new NodeForceAttribute("lc", sys, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0);

//            //nodes.ForEach(node => node.AddAttribute(fOriz));
//            nodes[2 - 1].AddAttribute(fOriz);
//            nodes[4 - 1].AddAttribute(fOriz);

//            List<Tri3OPTMembrane> elements = new List<Tri3OPTMembrane>();
//            /*elements.Add(new Tri3OPTMembrane(1, new Node[] { nodes[1 - 1], nodes[2 - 1], nodes[3 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(2, new Node[] { nodes[4 - 1], nodes[3 - 1], nodes[2 - 1] }, prop));*/

//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[1 - 1], nodes[5 - 1], nodes[3 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[1 - 1], nodes[2 - 1], nodes[5 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[2 - 1], nodes[4 - 1], nodes[5 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[4 - 1], nodes[3 - 1], nodes[5 - 1] }, prop));

//            LinearSolver fem = new LinearSolver(elements.ToArray());

//            double displacementNode = fem.GetNodeDisplacementGlobalCoordinates(nodes[1], LinearSolver.DOF.DX);
//            Console.WriteLine("DX node = " + displacementNode);
//            //Assert.AreEqual(1.0, displacementNode, 0.001);

//            Console.WriteLine("Element 1");
//            double[] elementGlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(elements[1 - 1]);
//            elements[1 - 1].GetNodesResults(elementGlobalDispl, out double[] localDispl,
//                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
//                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
//                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
//                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

//            Console.WriteLine("Element 2");
//            elementGlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(elements[2 - 1]);
//            elements[2 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
//                            out globalPseudoDef, out localPseudoDef,
//                            out globalForces, out localForces,
//                            out globalStress, out localStress,
//                            out globalEpsilon, out localEpsilon);
//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

//            /*Console.WriteLine("Element 3");
//            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[3 - 1]);
//            elements[3 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
//                            out globalPseudoDef, out localPseudoDef,
//                            out globalForces, out localForces,
//                            out globalStress, out localStress,
//                            out globalEpsilon, out localEpsilon);
//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

//            Console.WriteLine("Element 4");
//            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[4 - 1]);
//            elements[4 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
//                            out globalPseudoDef, out localPseudoDef,
//                            out globalForces, out localForces,
//                            out globalStress, out localStress,
//                            out globalEpsilon, out localEpsilon);
//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));*/
//        }

//        /// <summary>
//        /// Example not valid as test
//        /// </summary>
//        [TestMethod]
//        public void TriOPTMembranalExample6()
//        {
//            double E = 1.0;
//            double ni = 0.0;
//            Material mat = new SteelMaterial("steel", E, ni, 355, 510);

//            double thickness = 1.0;
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), thickness, thickness, "p");

//            List<Node> nodes = new List<Node>();
//            nodes.Add(new Node(0.0, 0.0, 0.0));
//            nodes.Add(new Node(1.0, 0.0, 0.0));
//            nodes.Add(new Node(0.0, 1.0, 0.0));
//            nodes.Add(new Node(1.0, 1.0, 0.0));
//            nodes.Add(new Node(0.5, 0.0, 0.0));
//            nodes.Add(new Node(0.0, 0.5, 0.0));
//            nodes.Add(new Node(0.5, 0.5, 0.0));
//            nodes.Add(new Node(1.0, 0.5, 0.0));
//            nodes.Add(new Node(0.5, 1.0, 0.0));


//            FreedomCase fc = new FreedomCase("fc1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
//            fix.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix.AddExternalRestrain(LinearSolver.DOF.DY);
//            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix.AddExternalRestrain(LinearSolver.DOF.RY);
//            //fix.AddExternalRestrain(LinearSolver.DOF.RZ);

//            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
//            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RY);
//            //fix2.AddExternalRestrain(LinearSolver.DOF.RZ);

//            nodes[1 - 1].AddAttribute(fix);
//            //nodes[2 - 1].AddAttribute(fix);
//            nodes[3 - 1].AddAttribute(fix);
//            //nodes[4 - 1].AddAttribute(fix);
//            nodes[6 - 1].AddAttribute(fix);

//            nodes.ForEach(node => node.AddAttribute(fix2));

//            LoadCaseBase lc = new LoadCaseBase("lc");
//            NodeForceAttribute f1 = new NodeForceAttribute("lc", sys, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0);
//            NodeForceAttribute f05 = new NodeForceAttribute("lc", sys, 0.5, 0.0, 0.0, 0.0, 0.0, 0.0);

//            //nodes.ForEach(node => node.AddAttribute(fOriz));
//            nodes[2 - 1].AddAttribute(f05);
//            nodes[8 - 1].AddAttribute(f1);
//            nodes[4 - 1].AddAttribute(f05);

//            List<Tri3OPTMembrane> elements = new List<Tri3OPTMembrane>();
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[1 - 1], nodes[5 - 1], nodes[7 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[5 - 1], nodes[2 - 1], nodes[7 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[1 - 1], nodes[6 - 1], nodes[7 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[2 - 1], nodes[7 - 1], nodes[8 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[6 - 1], nodes[7 - 1], nodes[3 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[3 - 1], nodes[7 - 1], nodes[9 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[9 - 1], nodes[7 - 1], nodes[4 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[7 - 1], nodes[4 - 1], nodes[8 - 1] }, prop));

//            /*elements.Add(new Tri3OPTMembrane(1, new Node[] { nodes[1 - 1], nodes[5 - 1], nodes[3 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(2, new Node[] { nodes[1 - 1], nodes[2 - 1], nodes[5 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(3, new Node[] { nodes[2 - 1], nodes[4 - 1], nodes[5 - 1] }, prop));
//            elements.Add(new Tri3OPTMembrane(4, new Node[] { nodes[4 - 1], nodes[3 - 1], nodes[5 - 1] }, prop));*/

//            LinearSolver fem = new LinearSolver(elements.ToArray());

//            double displacementNode = fem.GetNodeDisplacementGlobalCoordinates(nodes[1], LinearSolver.DOF.DX);
//            Console.WriteLine("DX node = " + displacementNode);
//            //Assert.AreEqual(1.0, displacementNode, 0.001);

//            Console.WriteLine("Element 1");
//            double[] elementGlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(elements[1 - 1]);
//            elements[1 - 1].GetNodesResults(elementGlobalDispl, out double[] localDispl,
//                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
//                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
//                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
//                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

//            Console.WriteLine("Element 2");
//            elementGlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(elements[2 - 1]);
//            elements[2 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
//                            out globalPseudoDef, out localPseudoDef,
//                            out globalForces, out localForces,
//                            out globalStress, out localStress,
//                            out globalEpsilon, out localEpsilon);
//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

//            /*Console.WriteLine("Element 3");
//            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[3 - 1]);
//            elements[3 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
//                            out globalPseudoDef, out localPseudoDef,
//                            out globalForces, out localForces,
//                            out globalStress, out localStress,
//                            out globalEpsilon, out localEpsilon);
//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

//            Console.WriteLine("Element 4");
//            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[4 - 1]);
//            elements[4 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
//                            out globalPseudoDef, out localPseudoDef,
//                            out globalForces, out localForces,
//                            out globalStress, out localStress,
//                            out globalEpsilon, out localEpsilon);
//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));*/
//        }

//        /// <summary>
//        /// Easy example, not valid as test
//        /// </summary>
//        [TestMethod]
//        public void TriOPTMembranalTest6()
//        {
//            double E = 1.0;
//            double ni = 0.0;
//            Material mat = new SteelMaterial("steel", E, ni, 355, 510);

//            double thickness = 1.0;
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), thickness, thickness, "p");

//            List<Node> nodes = new List<Node>();
//            nodes.Add(new Node(0.0, 0.0, 0.0));
//            nodes.Add(new Node(1.0, 0.0, 0.0));
//            nodes.Add(new Node(0.0, 1.0, 0.0));

//            FreedomCase fc = new FreedomCase("fc1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
//            fix.AddExternalRestrain(LinearSolver.DOF.DX);
//            fix.AddExternalRestrain(LinearSolver.DOF.DY);
//            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix.AddExternalRestrain(LinearSolver.DOF.RY);
//            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

//            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
//            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
//            fix2.AddExternalRestrain(LinearSolver.DOF.RY);
//            //fix2.AddExternalRestrain(LinearSolver.DOF.RZ);

//            nodes[1 - 1].AddAttribute(fix);
//            nodes[2 - 1].AddAttribute(fix);

//            nodes.ForEach(node => node.AddAttribute(fix2));

//            LoadCaseBase lc = new LoadCaseBase("lc");
//            NodeForceAttribute f1 = new NodeForceAttribute("lc", sys, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0);

//            nodes[3 - 1].AddAttribute(f1);

//            List<Tri3OPTMembrane> elements = new List<Tri3OPTMembrane>();
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[1 - 1], nodes[2 - 1], nodes[3 - 1] }, prop));

//            LinearSolver fem = new LinearSolver(elements.ToArray());

//            /*double displacementNode = fem.GetNodeDisplacementGlobalCoordinates(nodes[1], LinearSolver.DOF.DX);
//            Console.WriteLine("DX node = " + displacementNode);
//            Assert.AreEqual(1.0, displacementNode, 0.001);*/

//            Console.WriteLine("Element 1");
//            double[] elementGlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(elements[1 - 1]);
//            elements[1 - 1].GetNodesResults(elementGlobalDispl, out double[] localDispl,
//                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
//                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
//                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
//                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));
//        }

//        /// <summary>
//        /// Easy example, not valid as test
//        /// </summary>
//        [TestMethod]
//        public void TriOPTMembranalTest7()
//        {
//            double E = 1.0;
//            double ni = 0.0;
//            Material mat = new SteelMaterial("steel", E, ni, 355, 510);

//            double thickness = 1.0;
//            PlateProperty prop = new PlateProperty(mat.GetIsotropicFemMaterial(), thickness, thickness, "p");

//            List<Node> nodes = new List<Node>();
//            nodes.Add(new Node(0.0, 0.0, 0.0));
//            nodes.Add(new Node(1.0, 0.0, 0.0));
//            nodes.Add(new Node(0.5, 1.0, 0.0));

//            FreedomCase fc = new FreedomCase("fc1");
//            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

//            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
//            fix.AddExternalRestrain(Solver.DOF.DX);
//            fix.AddExternalRestrain(Solver.DOF.DY);
//            fix.AddExternalRestrain(Solver.DOF.DZ);
//            fix.AddExternalRestrain(Solver.DOF.RX);
//            fix.AddExternalRestrain(Solver.DOF.RY);
//            fix.AddExternalRestrain(Solver.DOF.RZ);

//            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
//            fix2.AddExternalRestrain(Solver.DOF.DZ);
//            fix2.AddExternalRestrain(Solver.DOF.RX);
//            fix2.AddExternalRestrain(Solver.DOF.RY);
//            //fix2.AddExternalRestrain(LinearSolver.DOF.RZ);

//            nodes[1 - 1].AddAttribute(fix);
//            nodes[2 - 1].AddAttribute(fix);

//            nodes.ForEach(node => node.AddAttribute(fix2));

//            LoadCaseBase lc = new LoadCaseBase("lc");
//            NodeForceAttribute f1 = new NodeForceAttribute("lc", sys, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0);

//            nodes[3 - 1].AddAttribute(f1);

//            List<Tri3OPTMembrane> elements = new List<Tri3OPTMembrane>();
//            elements.Add(new Tri3OPTMembrane(new Node[] { nodes[1 - 1], nodes[2 - 1], nodes[3 - 1] }, prop));

//            LinearSolver fem = new LinearSolver(elements.ToArray());

//            /*double displacementNode = fem.GetNodeDisplacementGlobalCoordinates(nodes[1], LinearSolver.DOF.DX);
//            Console.WriteLine("DX node = " + displacementNode);
//            Assert.AreEqual(1.0, displacementNode, 0.001);*/

//            Console.WriteLine("Element 1");
//            double[] elementGlobalDispl = fem.GetDisplacementsAtNodesOfElementInGlobalCoordinates(elements[1 - 1]);
//            elements[1 - 1].GetNodesResults(elementGlobalDispl, out double[] localDispl,
//                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
//                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
//                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
//                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
//            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));
//        }
//    }
//}