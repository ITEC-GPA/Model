using GPC.Geometry;
using GPC.Model.Combinations;
using GPC.Model.Data.Concrete;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Model femModel = new Model();

            AreaElement plate = new AreaElement(new Shape(new Polygon3d(new Point3d[]{
                new Point3d(0, 0, 0),
                new Point3d(0, 1, 0),
                new Point3d(1, 1, 0),
                new Point3d(1, 0, 0) })), new ConcretePlateProperty(ConcreteMaterialEN1992Data.C28_35, 10, 15, "P1"));

            femModel.AreaElements.Add(plate);
            for (int i = 0; i < plate.Points.Length; i++)
                femModel.NodesElements.Add(i + 1, new NodeElement(plate.Points[i]));

            var resultStress = new ResultStress(CoordinateSystem.Global, 1, 2, 0, 3, 4, 5);
            var resultPlateStress = new ResultPlateStress(CoordinateSystem.Global, resultStress, resultStress, resultStress);
            IEnumerable<ResultPlateStress> res = new List<ResultPlateStress>() { resultPlateStress, resultPlateStress, resultPlateStress };
            var resultLocationPoint = new ResultLocationPoint(res, new Point2d(0, 1));

            IEnumerable<ResultLocationPoint> locationsResult = new List<ResultLocationPoint>() { resultLocationPoint, resultLocationPoint, resultLocationPoint };

            var loadCase = new LoadCase("lc", LoadCase.LoadCaseTypes.SelfWeight);
            var cmb = new Combination("cmb1");
            cmb.AddLoadCaseCoefficient(loadCase, 1);

            plate.AddResult(new PlateResult(cmb, locationsResult.ToArray()));

            femModel.GetNode(1).AddResult(new NodeResult(cmb, new[] { new ResultLocationId(new INodeResult[] { new ResultDisplacement(1, 2, 3, 4, 5, 6) }, 1) }));

            var stresses = femModel.GetCombinationResultsPlateStress(cmb);
            var displacements = femModel.GetCombinationNodeDisplacementResults(cmb);

            // Plate
            Assert.IsTrue(femModel.GetNodeElement(1).Results.ToList()[0].Case.Name == "cmb1");
            Assert.IsTrue((stresses.First().GetResultLocations()[2].ResultTypes.First() as ResultPlateStress).UpperFace.Sxx == 1);

            // Nodo
            Assert.IsTrue(femModel.GetNode(1).Results.ToList()[0].Case.Name == "cmb1");

            Assert.IsTrue((displacements.First().GetResultLocations().First().ResultTypes[0] as ResultDisplacement).D1 == 1);
        }

        [TestMethod]
        public void ResultTest2()
        {
            Model femModel = new Model();

            AreaElement plate = new AreaElement(new Shape(new Polygon3d(new Point3d[]{
                new Point3d(0, 0, 0),
                new Point3d(0, 1, 0),
                new Point3d(1, 1, 0),
                new Point3d(1, 0, 0) })), new ConcretePlateProperty(ConcreteMaterialEN1992Data.C28_35, 10, 15, "P1"));

            femModel.AreaElements.Add(plate);

            femModel.AddGroup("Group1");
            femModel.SetGroup(new[] { plate }, "Group1");

            femModel.AreaElements.Add(plate);

            ResultStress resultStress = new ResultStress(CoordinateSystem.Global, 1, 2, 0, 3, 4, 5);
            ResultPlateStress resultPlateStress = new ResultPlateStress(CoordinateSystem.Global, resultStress, resultStress, resultStress);
            IEnumerable<ResultPlateStress> res = new List<ResultPlateStress>() { resultPlateStress, resultPlateStress, resultPlateStress };

            ResultLocationPoint resultLocationPoint = new ResultLocationPoint(res, new Point2d(0, 1));
            IEnumerable<ResultLocationPoint> locationsResult = new List<ResultLocationPoint>() { resultLocationPoint, resultLocationPoint, resultLocationPoint };

            LoadCase loadCase = new LoadCase("lc", LoadCase.LoadCaseTypes.SelfWeight);
            Combination cmb = new Combination("cmb1");
            cmb.AddLoadCaseCoefficient(loadCase, 1);

            plate.AddResult(new PlateResult(cmb, locationsResult.ToArray()));

            femModel.GetNode(1).AddResult(new NodeResult(cmb, new[] { new ResultLocationId(new INodeResult[] { new ResultDisplacement(1, 2, 3, 4, 5, 6) }, 1) }));

            IEnumerable<ElementResult> stresses2 = femModel.GetCombinationResultsPlateStress(cmb, "Group1");

            IEnumerable<ElementResult> stresses = femModel.GetCombinationResultsPlateStress(cmb);
            IEnumerable<NodeResult> displacements = femModel.GetCombinationNodeDisplacementResults(cmb);

            // Plate
            Assert.IsTrue(femModel.GetNodeElement(1).Results.ToList()[0].Case.Name == "cmb1");
            Assert.IsTrue((stresses.First().GetResultLocations()[2].ResultTypes.First() as ResultPlateStress).UpperFace.Sxx == 1);

            // Nodo
            Assert.IsTrue(femModel.GetNode(1).Results.ToList()[0].Case.Name == "cmb1");
            Assert.IsTrue((displacements.First().GetResultLocations().First().ResultTypes[0] as ResultDisplacement).D1 == 1);

            Assert.IsTrue(stresses.Count() == stresses2.Count());
        }
    }
}
