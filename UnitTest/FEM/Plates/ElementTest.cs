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
    public class ElementTest
    {
        [TestMethod]
        public void Quad4Test1()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0, "p");

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
            nodesPlate1[1] = new Node(+1.0, 0, 0, "2");
            nodesPlate1[2] = new Node(+2.0, +2, 0, "3");
            nodesPlate1[3] = new Node(0.0, +1, 0, "4");

            Plate e0 = new Quad4Element(nodesPlate1);
            e0.SetProperty(prop);

            LoadCase loadCase = new LoadCase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            PlatePressureAttribute pressure = new PlatePressureAttribute(loadCase, sys, 0.0, 0.0, 1.0);
            e0.AddLoadCaseAttribute(pressure);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0 });

            Assert.AreEqual(0.4167, fem.F[2], 0.001); //UX, UY, UZ
            Assert.AreEqual(0.50, fem.F[8], 0.001);
            Assert.AreEqual(0.5833, fem.F[14], 0.001);
            Assert.AreEqual(0.50, fem.F[20], 0.001);
        }

        [TestMethod]
        public void Quad4ToQuad8NodesTest1()
        {
            Node[] nodes = new Node[4];
            nodes[1 - 1] = new Node(0.0, 0.0, 0.0);
            nodes[2 - 1] = new Node(1.0, 0.0, 0.0);
            nodes[3 - 1] = new Node(1.0, 1.0, 0.0);
            nodes[4 - 1] = new Node(0.0, 1.0, 0.0);

            Node[] nodes8 = Quad4Element.Get8Nodes(nodes);

            Assert.AreEqual(0.0, nodes8[1 - 1].Position.X);
            Assert.AreEqual(0.0, nodes8[1 - 1].Position.Y);

            Assert.AreEqual(1.0, nodes8[2 - 1].Position.X);
            Assert.AreEqual(0.0, nodes8[2 - 1].Position.Y);

            Assert.AreEqual(1.0, nodes8[3 - 1].Position.X);
            Assert.AreEqual(1.0, nodes8[3 - 1].Position.Y);

            Assert.AreEqual(0.0, nodes8[4 - 1].Position.X);
            Assert.AreEqual(1.0, nodes8[4 - 1].Position.Y);

            Assert.AreEqual(0.5, nodes8[5 - 1].Position.X);
            Assert.AreEqual(0.0, nodes8[5 - 1].Position.Y);

            Assert.AreEqual(1.0, nodes8[6 - 1].Position.X);
            Assert.AreEqual(0.5, nodes8[6 - 1].Position.Y);

            Assert.AreEqual(0.5, nodes8[7 - 1].Position.X);
            Assert.AreEqual(1.0, nodes8[7 - 1].Position.Y);

            Assert.AreEqual(0.0, nodes8[8 - 1].Position.X);
            Assert.AreEqual(0.5, nodes8[8 - 1].Position.Y);
        }

        [TestMethod]
        public void Quad4GetLocalNodesTest1()
        {
            Node[] nodes = new Node[4];
            nodes[1 - 1] = new Node(0.0, 0.0, 0.0);
            nodes[2 - 1] = new Node(1.0, 0.0, 0.0);
            nodes[3 - 1] = new Node(1.0, 1.0, 0.0);
            nodes[4 - 1] = new Node(0.0, 1.0, 0.0);

            Node[] localNodes = Quad4Element.GetLocalNodes(nodes, out CoordinateSystem sys);

            Assert.AreEqual(0.0, localNodes[1 - 1].Position.X);
            Assert.AreEqual(0.0, localNodes[1 - 1].Position.Y);

            Assert.AreEqual(1.0, localNodes[2 - 1].Position.X);
            Assert.AreEqual(0.0, localNodes[2 - 1].Position.Y);

            Assert.AreEqual(1.0, localNodes[3 - 1].Position.X);
            Assert.AreEqual(1.0, localNodes[3 - 1].Position.Y);

            Assert.AreEqual(0.0, localNodes[4 - 1].Position.X);
            Assert.AreEqual(1.0, localNodes[4 - 1].Position.Y);

            //anticlock wise nodes
            nodes[1 - 1] = new Node(0.0, 0.0, 0.0);
            nodes[2 - 1] = new Node(0.0, 1.0, 0.0);
            nodes[3 - 1] = new Node(1.0, 1.0, 0.0);
            nodes[4 - 1] = new Node(1.0, 0.0, 0.0);

            localNodes = Quad4Element.GetLocalNodes(nodes, out sys);

            Assert.AreEqual(0.0, localNodes[1 - 1].Position.X);
            Assert.AreEqual(0.0, localNodes[1 - 1].Position.Y);

            Assert.AreEqual(1.0, localNodes[2 - 1].Position.X);
            Assert.AreEqual(0.0, localNodes[2 - 1].Position.Y);

            Assert.AreEqual(1.0, localNodes[3 - 1].Position.X);
            Assert.AreEqual(1.0, localNodes[3 - 1].Position.Y);

            Assert.AreEqual(0.0, localNodes[4 - 1].Position.X);
            Assert.AreEqual(1.0, localNodes[4 - 1].Position.Y);
        }

        [TestMethod]
        public void Tri3ElementTest1()
        {
            LoadCase loadCase = new LoadCase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("mat", 10000, 0.0, 355, 510, 7850);
            double t = 1.0;
            PlateProperty prop = new PlateProperty(mat, t, t, "p");

            #region restrains
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute fix = new NodeRestrainAttribute(freedomCase, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute fixRZ = new NodeRestrainAttribute(freedomCase, sys);
            fixRZ.AddExternalRestrain(LinearSolver.DOF.RZ);
            #endregion

            #region nodalforces
            NodeForceAttribute F = new NodeForceAttribute(loadCase, sys, 0.0, 1.0, 0.0, 1.0, 0, 0);
            #endregion

            Node nodeA = new Node(0.0, 8, 0,  "A");
            nodeA.AddAttribute(fixRZ);            
            nodeA.AddAttribute(F);                
                                                  
            Node nodeB = new Node(0.0, 0, 0,  "B");
            nodeB.AddAttribute(fix);              
                                                  
            Node nodeC = new Node(8.0, 8, 0,  "C");
            nodeC.AddAttribute(F);                
            nodeC.AddAttribute(fixRZ);            
                                                  
            Node nodeD = new Node(8.0, 0, 0,  "D");
            nodeD.AddAttribute(fix);

            FiniteElement e0 = new Tri3Element(new Node[] { nodeA, nodeB, nodeC });
            e0.SetProperty(prop);
          
            Plate e1 = new Tri3Element(new Node[] { nodeB, nodeD, nodeC });
            e1.SetProperty(prop);
          
            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0, e1 });

            double DY = fem.GetDisplacementGlobalCoordinates(nodeC, LinearSolver.DOF.DY);
            double DZ = fem.GetDisplacementGlobalCoordinates(nodeC, LinearSolver.DOF.DZ);
            Assert.AreEqual(0.0096, DZ, 1e-4);
            Assert.AreEqual(0.0002, DY, 1e-4);

            double sigmaTopYY = -(F.M1 + F.M1) / (1.0 / 6.0 * 8.0 * (t * t)) + (F.F2 + F.F2) / (t * 8.0);

            double[] e0GlobalDispl = fem.GetDisplacementsGlobalCoordinates(e0);
            e0.GetNodesResults(e0GlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
            Assert.AreEqual(sigmaTopYY, globalStress[0][1,1], 0.001); //sigmaYY top face

            double[] e1GlobalDispl = fem.GetDisplacementsGlobalCoordinates(e1);
            e1.GetNodesResults(e1GlobalDispl, out localDispl,
                            out globalPseudoDef, out localPseudoDef,
                            out globalForces, out localForces,
                            out globalStress, out localStress,
                            out globalEpsilon, out localEpsilon);
            Assert.AreEqual(sigmaTopYY, globalStress[0][1, 1], 0.001); //sigmaYY top face
        }

        [TestMethod]
        public void Tri3ElementTest2()
        {
            LoadCase loadCase = new LoadCase("myLoadCase", new Guid());
            FreedomCase freedomCase = new FreedomCase("freedomCase1");

            Material mat = new SteelMaterial("mat", 10000, 0.0, 355, 510, 7850);
            double t = 1.0;
            PlateProperty prop = new PlateProperty(mat, t, t, "p");

            #region restrains
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));
            NodeRestrainAttribute fix = new NodeRestrainAttribute(freedomCase, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            NodeRestrainAttribute fixRZ = new NodeRestrainAttribute(freedomCase, sys);
            fixRZ.AddExternalRestrain(LinearSolver.DOF.RZ);
            #endregion

            #region forces
            PlatePressureAttribute p = new PlatePressureAttribute(loadCase, sys, 0.0, 0.0, 1.0);
            #endregion

            Node nodeA = new Node(0.0, 8, 0);
            nodeA.Name = "A";
         
            nodeA.AddAttribute(fixRZ);
            
            Node nodeB = new Node(0.0, 0, 0);
            nodeA.Name = "B";               
        ;                 
            nodeB.AddAttribute(fix);        
                                            
            Node nodeC = new Node(8.0, 8, 0);
            nodeA.Name = "C";               
                
            nodeC.AddAttribute(fixRZ);      
                                            
            Node nodeD = new Node(8.0, 0, 0);
            nodeA.Name = "D";
        
            nodeD.AddAttribute(fix);

            Plate e0 = new Tri3Element(new Node[] { nodeA, nodeB, nodeC });
            e0.SetProperty(prop);
         
            e0.AddLoadCaseAttribute(p);
            Plate e1 = new Tri3Element(new Node[] { nodeB, nodeD, nodeC });
            e1.SetProperty(prop);
           
            e1.AddLoadCaseAttribute(p);
            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0, e1 });

            double DZC = fem.GetDisplacementGlobalCoordinates(nodeC, LinearSolver.DOF.DZ);
            Assert.AreEqual(0.97765, DZC, 1e-4); //value from SAP
            double DZA = fem.GetDisplacementGlobalCoordinates(nodeA, LinearSolver.DOF.DZ);
            Assert.AreEqual(0.75597, DZA, 1e-4); //value from SAP

            double[] e0GlobalDispl = fem.GetDisplacementsGlobalCoordinates(e0);
            /*e0.GetResults(e0GlobalDispl, out double[] localDispl,
                            out mnl.Matrix<double>[] globalPseudoDef, out mnl.Matrix<double>[] localPseudoDef,
                            out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces,
                            out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress,
                            out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
            */
        }

        [TestMethod]
        public void Quad4Test2()
        {
            Material mat = new SteelMaterial("mat", 12, 0.0, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 1.0, 1.0, "p");

            Node[] nodesPlate1 = new Node[4];
            nodesPlate1[0] = new Node(0.0, 0, 0, "1");
            nodesPlate1[1] = new Node(+1.0, 0, 0, "2");
            nodesPlate1[2] = new Node(+1.0, +1, 0, "3");
            nodesPlate1[3] = new Node(0.0, +1, 0, "4");

            FiniteElement e0 = new Quad4Element(nodesPlate1);
            e0.SetProperty(prop);

            LoadCase loadCase = new LoadCase("myLoadCase");
            FreedomCase freedomCase = new FreedomCase("freedomCase1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            /*PlatePressureAttribute pressure = new PlatePressureAttribute(loadCase, sys, 0.0, 0.0, 1.0);
            e0.AddLoadCaseAttribute(pressure);*/

            NodeForceAttribute F = new NodeForceAttribute(loadCase, sys, 0, 0, 1, 0, 0, 0);
            nodesPlate1[2].AddAttribute(F);
            nodesPlate1[3].AddAttribute(F);

            NodeRestrainAttribute fix = new NodeRestrainAttribute(freedomCase, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nodesPlate1[0].AddAttribute(fix);
            nodesPlate1[1].AddAttribute(fix);

            LinearSolver fem = new LinearSolver(new FiniteElement[] { e0 });

            Assert.AreEqual(0.667, fem.GetDisplacementGlobalCoordinates(nodesPlate1[2], LinearSolver.DOF.DZ), 0.001);
        }

        [TestMethod]
        public void Quad4Test3()
        {
            Material mat = new SteelMaterial("mat", 72000, 0.23, 355, 510, 7850);
            PlateProperty prop = new PlateProperty(mat, 6.13, 6.13, "p");

            List<Node> nodes = new List<Node>();
            nodes.Add(new Node(-1e6, -1e6, -1e6));
            nodes.Add(new Node(330, -100, 0));
            nodes.Add(new Node(330, 100, 0));
            nodes.Add(new Node(15, 100, 0));
            nodes.Add(new Node(15, -100, 0));

            List<Quad4Element> els = new List<Quad4Element>();
            els.Add(new Quad4Element(new Node[] { nodes[1], nodes[2], nodes[3], nodes[4] }, prop));

            LoadCase loadCase = new LoadCase("myLoadCase");
            FreedomCase freedomCase = new FreedomCase("freedomCase1");
            CoordinateSystem sys = new CoordinateSystem(new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0));

            /*PlatePressureAttribute pressure = new PlatePressureAttribute(loadCase, sys, 0.0, 0.0, 1.0);
            e0.AddLoadCaseAttribute(pressure);*/

            NodeForceAttribute F = new NodeForceAttribute(loadCase, sys, 0, 0, 8.0 * 9.81 / 2 / 2, 0, 0, 0);

            nodes.Where(x => x.Position.X == 15).ToList().ForEach(x => x.AddAttribute(F));

            NodeRestrainAttribute fix = new NodeRestrainAttribute(freedomCase, sys);
            fix.AddExternalRestrain(LinearSolver.DOF.DX);
            fix.AddExternalRestrain(LinearSolver.DOF.DY);
            fix.AddExternalRestrain(LinearSolver.DOF.DZ);
            fix.AddExternalRestrain(LinearSolver.DOF.RX);
            fix.AddExternalRestrain(LinearSolver.DOF.RY);
            fix.AddExternalRestrain(LinearSolver.DOF.RZ);

            nodes.Where(x => x.Position.X == 330).ToList().ForEach(x => x.AddAttribute(fix));

            LinearSolver fem = new LinearSolver(els.ToArray());

            Assert.AreEqual(1.4196, fem.GetDisplacementGlobalCoordinates(nodes[3], LinearSolver.DOF.DZ), 0.001);
        }
    }
}