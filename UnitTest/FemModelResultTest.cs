using GPC.Geometry;
using GPC.Model.Combinations;
using GPC.Model.FEM;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM.Materials;
using GPC.Model.FEM.Properties;
using GPC.Model.LoadCases;
using GPC.Model.Results;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FemTest
{
    [TestClass]
    public class FemModelResultTest : UnitTestBase
    {


        [TestMethod]
        public void ResultTest1()
        {
            FemModel femModel = new FemModel();

            Plate plate = new Plate(new Node[] {    new Node(0, 0, 0),
                                                    new Node(0, 1, 0),
                                                    new Node(1, 1, 0),
                                                    new Node(1, 0, 0)
                                               });

            femModel.AddProperty(new PlateProperty(new IsotropicFemMaterial(10, 0.1, 0.1, 1), 10, 10, "P1"));

            femModel.AddFiniteElement(plate, "P1");

            IEnumerable<ResultStress> res = new List<ResultStress>() { null, null, new ResultStress(CoordinateSystem.Global, 1, 2, 0, 3, 4, 5) };
            IEnumerable<ResultLocationPoint> points = new List<ResultLocationPoint>() { null, null, new ResultLocationPoint(1, new Point2d(0, 1)) };


            var loadCase = new LoadCase("lc", LoadCase.LoadCaseTypes.SelfWeight);
            var cmb = new Combination("cmb1");
            cmb.AddLoadCaseCoefficient(loadCase, 1);

            plate.AddResult(new PlateResult(cmb, CoordinateSystem.Global, res.ToArray(), points.ToArray()));

            femModel.GetNode(1).AddResult(new NodeResult(cmb, CoordinateSystem.Global, new ResultDisplacement(1, 2, 3, 4, 5, 6)));

            IEnumerable<FiniteElementResult> stresses = femModel.GetCombinationElementStressResults(cmb);
            IEnumerable<NodeResult> displacements = femModel.GetCombinationNodeDisplacementResults(cmb);

            // Plate
            Assert.IsTrue(femModel.GetFiniteElement(1).Results.ToList()[0].Case.Name == "cmb1");
            Assert.IsTrue((stresses.First().Results[2] as ResultStress).Sxx == 1);

            // Nodo
            Assert.IsTrue(femModel.GetNode(1).Results.ToList()[0].Case.Name == "cmb1");
            Assert.IsTrue((displacements.First().Result as ResultDisplacement).D1 == 1);

        }



        [TestMethod]
        public void ResultTest2()
        {
            FemModel femModel = new FemModel();

            Plate plate = new Plate(new Node[] {    new Node(0, 0, 0),
                                                    new Node(0, 1, 0),
                                                    new Node(1, 1, 0),
                                                    new Node(1, 0, 0)
                                               });


            femModel.AddProperty(new PlateProperty(new IsotropicFemMaterial(10, 0.1, 0.1, 1), 10, 10, "P1"));

            femModel.AddGroup("Group1");
            femModel.SetGroup(new[] { plate }, "Group1");

            femModel.AddFiniteElement(plate, "P1");

            IEnumerable<ResultStress> res = new List<ResultStress>() { null, null, new ResultStress(CoordinateSystem.Global, 1, 2, 0, 3, 4, 5) };
            IEnumerable<ResultLocationPoint> points = new List<ResultLocationPoint>() { null, null, new ResultLocationPoint(1, new Point2d(0, 1)) };

            var loadCase = new LoadCase("lc", LoadCase.LoadCaseTypes.SelfWeight);
            var cmb = new Combination("cmb1");
            cmb.AddLoadCaseCoefficient(loadCase, 1);

            plate.AddResult(new PlateResult(cmb, CoordinateSystem.Global, res.ToArray(), points.ToArray()));

            femModel.GetNode(1).AddResult(new NodeResult(cmb, CoordinateSystem.Global, new ResultDisplacement(1, 2, 3, 4, 5, 6)));

            IEnumerable<FiniteElementResult> stresses2 = femModel.GetCombinationElementStressResults(cmb, "Group1");

            IEnumerable<FiniteElementResult> stresses = femModel.GetCombinationElementStressResults(cmb);
            IEnumerable<NodeResult> displacements = femModel.GetCombinationNodeDisplacementResults(cmb);


            // Plate
            Assert.IsTrue(femModel.GetFiniteElement(1).Results.ToList()[0].Case.Name == "cmb1");
            Assert.IsTrue((stresses.First().Results[2] as ResultStress).Sxx == 1);

            // Nodo
            Assert.IsTrue(femModel.GetNode(1).Results.ToList()[0].Case.Name == "cmb1");
            Assert.IsTrue((displacements.First().Result as ResultDisplacement).D1 == 1);

            Assert.IsTrue(stresses.Count() == stresses2.Count());
        }



    }
}
