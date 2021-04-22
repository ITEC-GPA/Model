using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.Elements;
using GPC.Model.Materials;
using GPC.Model.FreedomCases;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.FEM.Attributes;
using GPC.Model.LoadCases;

namespace FemTest.Solver
{
    [TestClass]
    public class Tri3SBTETest
    {
        /// <summary>
        /// Easy example, not valid as test
        /// </summary>
        [TestMethod]
        public void Tri3SBTEMembranalTest1()
        {
            double E = 1.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness, "p");

            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(0.0, 0.0, 0.0));
            nodes.Add(new Node(1.0, 0.0, 0.0));
            nodes.Add(new Node(0.0, 1.0, 0.0));

            FreedomCase fc = new FreedomCase("fc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeRestrainAttribute fix = new NodeRestrainAttribute("fc", sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute("fc", sys);
            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
            fix2.AddExternalRestrain(LinearSolver.DOF.RY);
            //fix2.AddExternalRestrain(LinearSolver.DOF.RZ);

            nodes[1 - 1].AddAttribute(fix);
            nodes[2 - 1].AddAttribute(fix);

            nodes.ForEach(node => node.AddAttribute(fix2));

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute f1 = new NodeForceAttribute("lc", sys, 0.0, 1.0, 0.0, 0.0, 0.0, 0.0);

            nodes[3 - 1].AddAttribute(f1);

            List<Tri3SBTEMembrane> elements = new List<Tri3SBTEMembrane>();
            elements.Add(new Tri3SBTEMembrane(new Node[] { nodes[1 - 1], nodes[2 - 1], nodes[3 - 1] }));
            elements.ForEach(el => el.SetProperty(prop));

            LinearSolver fem = new LinearSolver(elements.ToArray());

            /*double displacementNode = fem.GetDisplacementGlobalCoordinates(nodes[1], LinearSolver.DOF.DX);
            Console.WriteLine("DX node = " + displacementNode);
            Assert.AreEqual(1.0, displacementNode, 0.001);*/

            Console.WriteLine("Element 1");
            double[] elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[1 - 1]);
            elements[1 - 1].GetNodesResults(elementGlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));
        }

#if false
        /// <summary>
        /// Based of "A study of optima membrane triangles with drilling freedoms" - Felippa - 2003 pg. 32
        /// </summary>
        [TestMethod]
        public void TriSBTDRMembranalTest2()
        {
            double E = 768.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            Node node1 = new Node(0.0, 0.0, 0.0, 1, "1");
            Node node2 = new Node(16.0, 0.0, 0.0, 2, "2");
            Node node3 = new Node(0.0, 1.0, 0.0, 3, "3");
            Node node4 = new Node(16.0, 1.0, 0.0, 4, "4");
            Node node5 = new Node(0.0, 2.0, 0.0, 5, "5");
            Node node6 = new Node(16.0, 2.0, 0.0, 6, "6");
            Node node7 = new Node(32.0, 0.0, 0.0, 7, "7");
            Node node8 = new Node(32.0, 1.0, 0.0, 8, "8");
            Node node9 = new Node(32.0, 2.0, 0.0, 9, "9");

            FreedomCase fc = new FreedomCase("fc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            
            NodeRestrainAttribute fix2 = new NodeRestrainAttribute(fc, sys);
            fix2.AddExternalRestrain(LinearSolver.DOF.DX);
            fix2.AddExternalRestrain(LinearSolver.DOF.RZ);

            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
            fix2.AddExternalRestrain(LinearSolver.DOF.RY);

            NodeRestrainAttribute fix3 = new NodeRestrainAttribute(fc, sys);
            fix3.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix3.AddExternalRestrain(LinearSolver.DOF.RX);
            fix3.AddExternalRestrain(LinearSolver.DOF.RY);

            node1.AddAttribute(fix2);
            node2.AddAttribute(fix3);
            node3.AddAttribute(fix);
            node4.AddAttribute(fix3);
            node5.AddAttribute(fix2);
            node6.AddAttribute(fix3);
            node7.AddAttribute(fix3);
            node8.AddAttribute(fix3);
            node9.AddAttribute(fix3);

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute fPlus = new NodeForceAttribute(lc, sys, 50.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            NodeForceAttribute fMinus = new NodeForceAttribute(lc, sys, -50.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            node9.AddAttribute(fMinus);
            node7.AddAttribute(fPlus);
            /*NodeForceAttribute M = new NodeForceAttribute(lc, sys, 0.0, 0.0, 0.0, 0.0, 0.0, 100.0);
            node8.AddAttribute(M);*/

            List<Tri3SBTDRMembrane> elements = new List<Tri3SBTDRMembrane>();
            elements.Add(new Tri3SBTDRMembrane(new Node[] { node1, node2, node3 }, prop, 1));
            elements.Add(new Tri3SBTDRMembrane(new Node[] { node3, node2, node4 }, prop, 2));
            elements.Add(new Tri3SBTDRMembrane(new Node[] { node3, node4, node5 }, prop, 3));
            elements.Add(new Tri3SBTDRMembrane(new Node[] { node5, node4, node6 }, prop, 4));
            elements.Add(new Tri3SBTDRMembrane(new Node[] { node2, node7, node4 }, prop, 5));
            elements.Add(new Tri3SBTDRMembrane(new Node[] { node4, node7, node8 }, prop, 6));
            elements.Add(new Tri3SBTDRMembrane(new Node[] { node4, node8, node6 }, prop, 7));
            elements.Add(new Tri3SBTDRMembrane(new Node[] { node6, node8, node9 }, prop, 8));

            LinearSolver fem = new LinearSolver(elements.ToArray());

            Console.WriteLine(fem.GetDisplacementGlobalCoordinates(node8, LinearSolver.DOF.DY) + " vs 100");
            Assert.AreEqual(100.0, fem.GetDisplacementGlobalCoordinates(node8, LinearSolver.DOF.DY), 0.1);
        }

        /// <summary>
        /// Based of "A study of optima membrane triangles with drilling freedoms" - Felippa - 2003 pg. 33-34
        /// </summary>
        [TestMethod]
        public void TriOPTMembranalTest3()
        {
            double E = 30000.0;
            double ni = 1.0 / 4.0;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            Node node1 = new Node(0.0, 0.0, 0.0, 1, "1");
            Node node2 = new Node(0.0, 6.0, 0.0, 2, "2");
            Node node3 = new Node(0.0, 12.0, 0.0, 3, "3");
            Node node4 = new Node(24.0, 0.0, 0.0, 4, "4");
            Node node5 = new Node(24.0, 6.0, 0.0, 5, "5");
            Node node6 = new Node(24.0, 12.0, 0.0, 6, "6");
            Node node7 = new Node(48.0, 0.0, 0.0, 7, "7");
            Node node8 = new Node(48.0, 6.0, 0.0, 8, "8");
            Node node9 = new Node(48.0, 12.0, 0.0, 9, "9");

            FreedomCase fc = new FreedomCase("fc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute(fc, sys);
            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
            fix2.AddExternalRestrain(LinearSolver.DOF.RY);

            node1.AddAttribute(fix);
            node2.AddAttribute(fix);
            node3.AddAttribute(fix);

            node4.AddAttribute(fix2);
            node5.AddAttribute(fix2);
            node6.AddAttribute(fix2);
            node7.AddAttribute(fix2);
            node8.AddAttribute(fix2);
            node9.AddAttribute(fix2);

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute fCentral = new NodeForceAttribute(lc, sys, 0.0, 40.0 / 0.35601 * 100.0 * 2.0 / 4.0, 0.0, 0.0, 0.0, 0.0);
            NodeForceAttribute fLateral = new NodeForceAttribute(lc, sys, 0.0, 40.0 / 0.35601 * 100.0 * 1.0 / 4.0, 0.0, 0.0, 0.0, 0.0);
            node7.AddAttribute(fLateral);
            node8.AddAttribute(fCentral);
            node9.AddAttribute(fLateral);

            List<Tri3OPTMembrane> elements = new List<Tri3OPTMembrane>();
            elements.Add(new Tri3OPTMembrane(new Node[] { node1, node4, node5 }, prop, 1));
            elements.Add(new Tri3OPTMembrane(new Node[] { node1, node5, node2 }, prop, 2));
            elements.Add(new Tri3OPTMembrane(new Node[] { node2, node5, node6 }, prop, 3));
            elements.Add(new Tri3OPTMembrane(new Node[] { node2, node6, node3 }, prop, 4));
            elements.Add(new Tri3OPTMembrane(new Node[] { node4, node7, node8 }, prop, 5));
            elements.Add(new Tri3OPTMembrane(new Node[] { node4, node8, node5 }, prop, 6));
            elements.Add(new Tri3OPTMembrane(new Node[] { node5, node8, node9 }, prop, 7));
            elements.Add(new Tri3OPTMembrane(new Node[] { node6, node9, node5 }, prop, 8));

            LinearSolver fem = new LinearSolver(elements.ToArray());

            Assert.AreEqual(91.03, fem.GetDisplacementGlobalCoordinates(node8, LinearSolver.DOF.DY), 0.1); //In article is 92.24

            double[] elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[2]);
            elements[2].GetNodesResults(elementGlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);

            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));            
        }

        /// <summary>
        /// Based of "A study of optima membrane triangles with drilling freedoms" - Felippa - 2003 pg. 33-34
        /// </summary>
        [TestMethod]
        public void TriOPTMembranalTest4()
        {
            double E = 30000.0;
            double ni = 1.0 / 4.0;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(0.0, -1000, -1000, -1000, "NULL"));
            nodes.Add(new Node(1, 0.0, -10.0, 0.0));
            nodes.Add(new Node(2, 24.0, -4.0, 0.0));
            nodes.Add(new Node(3, 0.0, -4.0, 0.0));
            nodes.Add(new Node(4, 48.0, -16.0, 0.0));
            nodes.Add(new Node(5, 48.0, -10.0, 0.0, "P"));
            nodes.Add(new Node(6, 24.0, -16.0, 0.0));
            nodes.Add(new Node(7, 24.0, -10.0, 0.0));
            nodes.Add(new Node(8, 48.0, -4.0, 0.0));
            nodes.Add(new Node(9, 0.0, -16.0, 0.0));
            nodes.Add(new Node(10, 0.0, -7.0, 0.0));
            nodes.Add(new Node(11, 12.0, -4.0, 0.0));
            nodes.Add(new Node(12, 12.0, -7.0, 0.0));
            nodes.Add(new Node(13, 12.0, -10.0, 0.0));
            nodes.Add(new Node(14, 24.0, -7.0, 0.0));
            nodes.Add(new Node(15, 48.0, -13.0, 0.0));
            nodes.Add(new Node(16, 36.0, -16.0, 0.0));
            nodes.Add(new Node(17, 36.0, -13.0, 0.0));
            nodes.Add(new Node(18, 36.0, -10.0, 0.0));
            nodes.Add(new Node(19, 24.0, -13.0, 0.0));
            nodes.Add(new Node(20, 48.0, -7.0, 0.0));
            nodes.Add(new Node(21, 36.0, -7.0, 0.0));
            nodes.Add(new Node(22, 36.0, -4.0, 0.0));
            nodes.Add(new Node(23, 12.0, -16.0, 0.0));
            nodes.Add(new Node(24, 12.0, -13.0, 0.0));
            nodes.Add(new Node(25, 0.0, -13.0, 0.0));

            FreedomCase fc = new FreedomCase("fc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute(fc, sys);
            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
            fix2.AddExternalRestrain(LinearSolver.DOF.RY);

            nodes[1].AddAttribute(fix);
            nodes[3].AddAttribute(fix);
            nodes[9].AddAttribute(fix);
            nodes[10].AddAttribute(fix);
            nodes[25].AddAttribute(fix);

            nodes.ForEach(x => x.AddAttribute(fix2));

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute fCentral = new NodeForceAttribute(lc, sys, 0.0, 40.0 * 2.0 / 8.0, 0.0, 0.0, 0.0, 0.0);
            NodeForceAttribute fLateral = new NodeForceAttribute(lc, sys, 0.0, 40.0 * 1.0 / 8.0, 0.0, 0.0, 0.0, 0.0);

            nodes[4].AddAttribute(fLateral);
            nodes[15].AddAttribute(fCentral);
            nodes[5].AddAttribute(fCentral);
            nodes[20].AddAttribute(fCentral);
            nodes[8].AddAttribute(fLateral);

            List<Tri3OPTMembrane> elements = new List<Tri3OPTMembrane>();
            elements.Add(new Tri3OPTMembrane(1, new Node[] { nodes[14], nodes[13], nodes[7] }, prop));
            elements.Add(new Tri3OPTMembrane(2, new Node[] { nodes[19], nodes[18], nodes[7] }, prop));
            elements.Add(new Tri3OPTMembrane(3, new Node[] { nodes[14], nodes[22], nodes[2] }, prop));
            elements.Add(new Tri3OPTMembrane(4, new Node[] { nodes[25], nodes[13], nodes[1] }, prop));
            elements.Add(new Tri3OPTMembrane(5, new Node[] { nodes[3], nodes[10], nodes[11] }, prop));
            elements.Add(new Tri3OPTMembrane(6, new Node[] { nodes[11], nodes[10], nodes[12] }, prop));
            elements.Add(new Tri3OPTMembrane(7, new Node[] { nodes[10], nodes[1], nodes[12] }, prop));
            elements.Add(new Tri3OPTMembrane(8, new Node[] { nodes[12], nodes[1], nodes[13] }, prop));
            elements.Add(new Tri3OPTMembrane(9, new Node[] { nodes[11], nodes[12], nodes[2] }, prop));
            elements.Add(new Tri3OPTMembrane(10, new Node[] { nodes[2], nodes[12], nodes[14] }, prop));
            elements.Add(new Tri3OPTMembrane(11, new Node[] { nodes[12], nodes[13], nodes[14] }, prop));
            elements.Add(new Tri3OPTMembrane(12, new Node[] { nodes[4], nodes[15], nodes[16] }, prop));
            elements.Add(new Tri3OPTMembrane(13, new Node[] { nodes[16], nodes[15], nodes[17] }, prop));
            elements.Add(new Tri3OPTMembrane(14, new Node[] { nodes[15], nodes[5], nodes[17] }, prop));
            elements.Add(new Tri3OPTMembrane(15, new Node[] { nodes[17], nodes[5], nodes[18] }, prop));
            elements.Add(new Tri3OPTMembrane(16, new Node[] { nodes[16], nodes[17], nodes[6] }, prop));
            elements.Add(new Tri3OPTMembrane(17, new Node[] { nodes[6], nodes[17], nodes[19] }, prop));
            elements.Add(new Tri3OPTMembrane(18, new Node[] { nodes[17], nodes[18], nodes[19] }, prop));
            elements.Add(new Tri3OPTMembrane(19, new Node[] { nodes[5], nodes[20], nodes[18] }, prop));
            elements.Add(new Tri3OPTMembrane(20, new Node[] { nodes[18], nodes[20], nodes[21] }, prop));
            elements.Add(new Tri3OPTMembrane(21, new Node[] { nodes[20], nodes[8], nodes[21] }, prop));
            elements.Add(new Tri3OPTMembrane(22, new Node[] { nodes[21], nodes[8], nodes[22] }, prop));
            elements.Add(new Tri3OPTMembrane(23, new Node[] { nodes[18], nodes[21], nodes[7] }, prop));
            elements.Add(new Tri3OPTMembrane(24, new Node[] { nodes[7], nodes[21], nodes[14] }, prop));
            elements.Add(new Tri3OPTMembrane(25, new Node[] { nodes[21], nodes[22], nodes[14] }, prop));
            elements.Add(new Tri3OPTMembrane(26, new Node[] { nodes[6], nodes[19], nodes[23] }, prop));
            elements.Add(new Tri3OPTMembrane(27, new Node[] { nodes[23], nodes[19], nodes[24] }, prop));
            elements.Add(new Tri3OPTMembrane(28, new Node[] { nodes[19], nodes[7], nodes[24] }, prop));
            elements.Add(new Tri3OPTMembrane(29, new Node[] { nodes[24], nodes[7], nodes[13] }, prop));
            elements.Add(new Tri3OPTMembrane(30, new Node[] { nodes[23], nodes[24], nodes[9] }, prop));
            elements.Add(new Tri3OPTMembrane(31, new Node[] { nodes[9], nodes[24], nodes[25] }, prop));
            elements.Add(new Tri3OPTMembrane(32, new Node[] { nodes[24], nodes[13], nodes[25] }, prop));

            LinearSolver fem = new LinearSolver(elements.ToArray());
                       
            double displacementNodeC = fem.GetDisplacementGlobalCoordinates(nodes[5], LinearSolver.DOF.DY);
            Console.WriteLine("DY node C = " + displacementNodeC);
            Console.WriteLine(displacementNodeC / 0.35601 + " vs 1.0");
            

            Console.WriteLine("Element 9");
            double[] elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[9-1]);
            elements[9-1].GetNodesResults(elementGlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);

            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

            Console.WriteLine("Element 29");
            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[29 - 1]);
            elements[29 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
                            out globalPseudoDef, out localPseudoDef,
                            out globalForces, out localForces,
                            out globalStress, out localStress,
                            out globalEpsilon, out localEpsilon);

            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

            Console.WriteLine("Element 2");
            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[2 - 1]);
            elements[2 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
                            out globalPseudoDef, out localPseudoDef,
                            out globalForces, out localForces,
                            out globalStress, out localStress,
                            out globalEpsilon, out localEpsilon);

            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

            Assert.AreEqual(1.0, displacementNodeC / 0.35601, 0.1);
        }

        /// <summary>
        ///Example not valid as test
        /// </summary>
        [TestMethod]
        public void TriOPTMembranalExample5()
        {
            double E = 1.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(1, 0.0, 0.0, 0.0));
            nodes.Add(new Node(2, 1.0, 0.0, 0.0));
            nodes.Add(new Node(3, 0.0, 1.0, 0.0));
            nodes.Add(new Node(4, 1.0, 1.0, 0.0));
            nodes.Add(new Node(5, 0.5, 0.5, 0.0));

            FreedomCase fc = new FreedomCase("fc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            //fix.AddExternalRestrain(LinearSolver.DOF.RZ);
            
            NodeRestrainAttribute fix2 = new NodeRestrainAttribute(fc, sys);
            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
            fix2.AddExternalRestrain(LinearSolver.DOF.RY);
            //fix2.AddExternalRestrain(LinearSolver.DOF.RZ);

            nodes[1 - 1].AddAttribute(fix);
            //nodes[2 - 1].AddAttribute(fix);
            nodes[3 - 1].AddAttribute(fix);
            //nodes[4 - 1].AddAttribute(fix);

            nodes.ForEach(node => node.AddAttribute(fix2));

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute fOriz = new NodeForceAttribute(lc, sys, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0);

            //nodes.ForEach(node => node.AddAttribute(fOriz));
            nodes[2 - 1].AddAttribute(fOriz);
            nodes[4 - 1].AddAttribute(fOriz);

            List<Tri3OPTMembrane> elements = new List<Tri3OPTMembrane>();
            /*elements.Add(new Tri3OPTMembrane(1, new Node[] { nodes[1 - 1], nodes[2 - 1], nodes[3 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(2, new Node[] { nodes[4 - 1], nodes[3 - 1], nodes[2 - 1] }, prop));*/

            elements.Add(new Tri3OPTMembrane(1, new Node[] { nodes[1 - 1], nodes[5 - 1], nodes[3 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(2, new Node[] { nodes[1 - 1], nodes[2 - 1], nodes[5 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(3, new Node[] { nodes[2 - 1], nodes[4 - 1], nodes[5 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(4, new Node[] { nodes[4 - 1], nodes[3 - 1], nodes[5 - 1] }, prop));

            LinearSolver fem = new LinearSolver(elements.ToArray());

            double displacementNode = fem.GetDisplacementGlobalCoordinates(nodes[1], LinearSolver.DOF.DX);
            Console.WriteLine("DX node = " + displacementNode);
            //Assert.AreEqual(1.0, displacementNode, 0.001);

            Console.WriteLine("Element 1");
            double[] elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[1 - 1]);
            elements[1 - 1].GetNodesResults(elementGlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

            Console.WriteLine("Element 2");
            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[2 - 1]);
            elements[2 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
                            out globalPseudoDef, out localPseudoDef,
                            out globalForces, out localForces,
                            out globalStress, out localStress,
                            out globalEpsilon, out localEpsilon);
            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

            /*Console.WriteLine("Element 3");
            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[3 - 1]);
            elements[3 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
                            out globalPseudoDef, out localPseudoDef,
                            out globalForces, out localForces,
                            out globalStress, out localStress,
                            out globalEpsilon, out localEpsilon);
            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

            Console.WriteLine("Element 4");
            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[4 - 1]);
            elements[4 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
                            out globalPseudoDef, out localPseudoDef,
                            out globalForces, out localForces,
                            out globalStress, out localStress,
                            out globalEpsilon, out localEpsilon);
            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));*/
        }

        /// <summary>
        /// Example not valid as test
        /// </summary>
        [TestMethod]
        public void TriOPTMembranalExample6()
        {
            double E = 1.0;
            double ni = 0.0;
            Material mat = new SteelMaterial("steel", E, ni, 355, 510, 7850);

            double thickness = 1.0;
            PlateProperty prop = new PlateProperty(mat, thickness, thickness);

            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(1, 0.0, 0.0, 0.0));
            nodes.Add(new Node(2, 1.0, 0.0, 0.0));
            nodes.Add(new Node(3, 0.0, 1.0, 0.0));
            nodes.Add(new Node(4, 1.0, 1.0, 0.0));
            nodes.Add(new Node(5, 0.5, 0.0, 0.0));
            nodes.Add(new Node(6, 0.0, 0.5, 0.0));
            nodes.Add(new Node(7, 0.5, 0.5, 0.0));
            nodes.Add(new Node(8, 1.0, 0.5, 0.0));
            nodes.Add(new Node(9, 0.5, 1.0, 0.0));

            FreedomCase fc = new FreedomCase("fc1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            NodeRestrainAttribute fix = new NodeRestrainAttribute(fc, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            //fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute fix2 = new NodeRestrainAttribute(fc, sys);
            fix2.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix2.AddExternalRestrain(LinearSolver.DOF.RX);
            fix2.AddExternalRestrain(LinearSolver.DOF.RY);
            //fix2.AddExternalRestrain(LinearSolver.DOF.RZ);

            nodes[1 - 1].AddAttribute(fix);
            //nodes[2 - 1].AddAttribute(fix);
            nodes[3 - 1].AddAttribute(fix);
            //nodes[4 - 1].AddAttribute(fix);
            nodes[6 - 1].AddAttribute(fix);

            nodes.ForEach(node => node.AddAttribute(fix2));

            LoadCase lc = new LoadCase("lc");
            NodeForceAttribute f1 = new NodeForceAttribute(lc, sys, 1.0, 0.0, 0.0, 0.0, 0.0, 0.0);
            NodeForceAttribute f05 = new NodeForceAttribute(lc, sys, 0.5, 0.0, 0.0, 0.0, 0.0, 0.0);

            //nodes.ForEach(node => node.AddAttribute(fOriz));
            nodes[2 - 1].AddAttribute(f05);
            nodes[8 - 1].AddAttribute(f1);
            nodes[4 - 1].AddAttribute(f05);

            List<Tri3OPTMembrane> elements = new List<Tri3OPTMembrane>();
            elements.Add(new Tri3OPTMembrane(1, new Node[] { nodes[1 - 1], nodes[5 - 1], nodes[7 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(2, new Node[] { nodes[5 - 1], nodes[2 - 1], nodes[7 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(3, new Node[] { nodes[1 - 1], nodes[6 - 1], nodes[7 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(4, new Node[] { nodes[2 - 1], nodes[7 - 1], nodes[8 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(5, new Node[] { nodes[6 - 1], nodes[7 - 1], nodes[3 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(6, new Node[] { nodes[3 - 1], nodes[7 - 1], nodes[9 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(7, new Node[] { nodes[9 - 1], nodes[7 - 1], nodes[4 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(7, new Node[] { nodes[7 - 1], nodes[4 - 1], nodes[8 - 1] }, prop));

            /*elements.Add(new Tri3OPTMembrane(1, new Node[] { nodes[1 - 1], nodes[5 - 1], nodes[3 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(2, new Node[] { nodes[1 - 1], nodes[2 - 1], nodes[5 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(3, new Node[] { nodes[2 - 1], nodes[4 - 1], nodes[5 - 1] }, prop));
            elements.Add(new Tri3OPTMembrane(4, new Node[] { nodes[4 - 1], nodes[3 - 1], nodes[5 - 1] }, prop));*/

            LinearSolver fem = new LinearSolver(elements.ToArray());

            double displacementNode = fem.GetDisplacementGlobalCoordinates(nodes[1], LinearSolver.DOF.DX);
            Console.WriteLine("DX node = " + displacementNode);
            //Assert.AreEqual(1.0, displacementNode, 0.001);

            Console.WriteLine("Element 1");
            double[] elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[1 - 1]);
            elements[1 - 1].GetNodesResults(elementGlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

            Console.WriteLine("Element 2");
            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[2 - 1]);
            elements[2 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
                            out globalPseudoDef, out localPseudoDef,
                            out globalForces, out localForces,
                            out globalStress, out localStress,
                            out globalEpsilon, out localEpsilon);
            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

            /*Console.WriteLine("Element 3");
            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[3 - 1]);
            elements[3 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
                            out globalPseudoDef, out localPseudoDef,
                            out globalForces, out localForces,
                            out globalStress, out localStress,
                            out globalEpsilon, out localEpsilon);
            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));

            Console.WriteLine("Element 4");
            elementGlobalDispl = fem.GetDisplacementsGlobalCoordinates(elements[4 - 1]);
            elements[4 - 1].GetNodesResults(elementGlobalDispl, out localDispl,
                            out globalPseudoDef, out localPseudoDef,
                            out globalForces, out localForces,
                            out globalStress, out localStress,
                            out globalEpsilon, out localEpsilon);
            globalStress.ToList().ForEach(x => Console.WriteLine("global stress" + x));*/
        }
#endif
    }
}