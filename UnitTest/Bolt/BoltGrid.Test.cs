using GPC.Geometry;
using GPC.Model.Data.Steel;
using GPC.Model.Results;
using GPC.Model.Sections.Bolt;
using GPC.Utilities.Maths;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ModelObjectTest
{
    [TestClass]
    public class BoltGridTest
    {
        /// <summary>
        /// Net areas from tables.
        /// </summary>
        public static readonly Dictionary<decimal, double> ThreadedAreas = new Dictionary<decimal, double>()
        {
            { 8, 36.6},
            {10, 58.0},
            {12, 84.3},
            {14, 115},
            {16, 157},
            {18, 192},
            {20, 245},
            {22, 303},
            {24, 353},
            {27, 459},
            {30, 561},
            {33, 694},
            {36, 817},
            {39, 976},
            {42, 1120},
            {45, 1310},
            {48, 1470},
            {52, 1760},
            {56, 2030},
            {60, 2360},
            {64, 2680},
            {68, 3060}
        };

        [TestMethod]
        public void Test01_DoubleApproximation_01()
        {
            // Return correct value from double representation of diameter.
            var DiaKeys = ThreadedAreas.Keys.ToList();
            var Mat = BoltMaterialEN1993Data.Class10_9;
            double maxError = 0.005;

            foreach (var key in DiaKeys)
            {
                var SecPlus = new BoltSection((double)key + 0.0000001, Mat);
                Assert.IsTrue(Error.AreEqualsDouble(ThreadedAreas[key], SecPlus.CalculateAreaEff(), maxError));
                var SecMinus = new BoltSection((double)key - 0.0000001, Mat);
                Assert.IsTrue(Error.AreEqualsDouble(ThreadedAreas[key], SecMinus.CalculateAreaEff(), maxError));
            }
        }

        [TestMethod]
        public void Test02_ForceCalculation_01()
        {
            var BG = new RectangularBoltGrid(new double[] { }, new double[] { 200 }, 12, BoltMaterialEN1993Data.Class10_9);
            var BarSys = new CoordinateSystem(BG.CalculateBarycenter(), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, 5000, 4000, 0, 0, 0, BarSys);
            var res = BG.CalculateShearForcesElastic(VetSoll);

            // Solution
            var SolBeam = new ResultBeamForces(0, 5000.0 / 2.0, 4000.0 / 2.0, 0, 0, 0, BarSys);
            foreach (var SolB in res)
            {
                Assert.IsTrue(Error.AreEqualsDouble(SolB.Value.V1, SolBeam.V1));
                Assert.IsTrue(Error.AreEqualsDouble(SolB.Value.V2, SolBeam.V2));
            }
            Assert.IsTrue(BG.CheckShearForcesElastic(res, VetSoll));
        }

        [TestMethod]
        public void Test03_EqualCoordinateSystem_01()
        {
            var BarSys1 = new CoordinateSystem(new Point3d(2, 3, 4), Vector3d.XAxis, Vector3d.YAxis);
            var BarSys2 = new CoordinateSystem(new Point3d(2, 3, 4), Vector3d.XAxis, Vector3d.YAxis);
            var BarSys3 = new CoordinateSystem(new Point3d(2, 3.1, 4), Vector3d.XAxis, Vector3d.YAxis);
            var newX = Vector3d.XAxis.CrossProduct(new Vector3d(0, 0, 0.1));
            var newY = Vector3d.YAxis.CrossProduct(new Vector3d(0, 0, 0.1));
            var BarSys4 = new CoordinateSystem(new Point3d(2, 3, 4), newX, newY);

            double Angle = 0.1;
            var newX2 = new Vector3d(Math.Cos(Angle), Math.Sin(Angle), 0.0);
            var newY2 = new Vector3d(-Math.Sin(Angle), Math.Cos(Angle), 0.0);
            var BarSys5 = new CoordinateSystem(new Point3d(2, 3, 4), newX2, newY2);

            Assert.IsTrue(BarSys1 == BarSys2);
            Assert.IsTrue(BarSys1 != BarSys3);
            Assert.IsTrue(BarSys1 != BarSys4);
            Assert.IsTrue(BarSys1 != BarSys5);
        }

        [TestMethod]
        public void Test04_ForceTranslationInX_01()
        {
            var BarSys = new CoordinateSystem(new Point3d(10, 5, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, 5000, 4000, 100, 0, 0, BarSys);
            // Destination
            var BarSys2 = new CoordinateSystem(new Point3d(15, 5, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll2 = VetSoll.ToCoordinateSystemWithEccentricity(BarSys2);
            // Solution
            var VetSoll2_result = new ResultBeamForces(0, 5000, 4000, 100 - 4000 * 5, 0, 0, BarSys);
            // Check
            Assert.AreEqual(VetSoll2.T, VetSoll2_result.T);
        }

        [TestMethod]
        public void Test05_ForceTranslationInXAndY_01()
        {
            var BarSys = new CoordinateSystem(new Point3d(10, 5, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, 5000, 4000, 100, 0, 0, BarSys);
            // Destination
            var BarSys2 = new CoordinateSystem(new Point3d(15, 12, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll2 = VetSoll.ToCoordinateSystemWithEccentricity(BarSys2);
            // Solution
            var VetSoll2_result = new ResultBeamForces(0, 5000, 4000, 100 - 4000 * 5 + 5000 * 7, 0, 0, BarSys);
            // Check
            Assert.AreEqual(VetSoll2.T, VetSoll2_result.T);
        }

        [TestMethod]
        public void Test06_ForcesSum_01()
        {
            // Force A
            var BarSysA = new CoordinateSystem(new Point3d(10, 5, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSollA = new ResultBeamForces(0, 5000, 4000, 100, 0, 0, BarSysA);
            // Force B
            var BarSysB = new CoordinateSystem(new Point3d(15, 5, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSollB = new ResultBeamForces(0, 5000, 4000, 100, 0, 0, BarSysB);
            // Sum
            var VetSollSum = VetSollA + VetSollB;
            // Solution
            var VetSollSum_result = new ResultBeamForces(0, 10000, 8000, 200 + 4000 * 5, 0, 0, BarSysA);
            // Check
            Assert.AreEqual(VetSollSum.T, VetSollSum_result.T);
            Assert.AreEqual(VetSollSum.V1, VetSollSum_result.V1);
            Assert.AreEqual(VetSollSum.V2, VetSollSum_result.V2);
        }

        [TestMethod]
        public void Test07_ForceCalculation_01()
        {
            var BG = new RectangularBoltGrid(new double[] { }, new double[] { 200 }, 12, BoltMaterialEN1993Data.Class10_9);
            var AppSys = new CoordinateSystem(new Point3d(50, 0, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, 0, -800, 0, 0, 0, AppSys);
            var res = BG.CalculateShearForcesElastic(VetSoll);

            // Solution
            Assert.IsTrue(BG.CheckShearForcesElastic(res, VetSoll));
        }

        [TestMethod]
        public void Test08_ForceCalculation_02()
        {
            var BG = new RectangularBoltGrid(new double[] { 200 }, new double[] { }, 12, BoltMaterialEN1993Data.Class10_9);
            var AppSys = new CoordinateSystem(new Point3d(0, 50, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, -800, 0, 0, 0, 0, AppSys);
            var res = BG.CalculateShearForcesElastic(VetSoll);

            // Solution
            Assert.IsTrue(BG.CheckShearForcesElastic(res, VetSoll));
        }

        [TestMethod]
        public void Test09_ForceCalculation_03()
        {
            var BG = new RectangularBoltGrid(new double[] { 200 }, new double[] { 120, 120 }, 12, BoltMaterialEN1993Data.Class10_9);
            var AppSys = new CoordinateSystem(new Point3d(0, 50, 0), Vector3d.XAxis, Vector3d.YAxis);
            var VetSoll = new ResultBeamForces(0, -800, 300, 50000, 0, 0, AppSys);
            var res = BG.CalculateShearForcesElastic(VetSoll);

            // Solution
            Assert.IsTrue(BG.CheckShearForcesElastic(res, VetSoll));
        }

        [TestMethod]
        public void Test10_GridGeometry_01()
        {
            double diameter = 12;

            var plate = new RectangularPlateWithBolts(300, 340, SteelMaterialEN1993Data.S235, 10, new double[] { 200 }, new double[] { 120, 120 },
                diameter, BoltMaterialEN1993Data.Class10_9, new Point2d(50, 50));

            ResultBeamForces resultBeamForces = new ResultBeamForces(0, 10, 0, 0, 0, 0, new CoordinateSystem(plate.GetCoordinateSystem()));
            var dist1 = plate.CalculateE1(plate.BoltGrid.Bolts[0], resultBeamForces);
            var dist2 = plate.CalculateE1(plate.BoltGrid.Bolts[1], resultBeamForces);
            var dist4 = plate.CalculateE1(plate.BoltGrid.Bolts[3], resultBeamForces);

            // Solution
            double expDist1 = 250;
            Assert.IsTrue(Math.Abs(dist1 - expDist1) < 1);
            double expDist2 = 250;
            Assert.IsTrue(Math.Abs(dist2 - expDist2) < 1);
            double expDist4 = 50;
            Assert.IsTrue(Math.Abs(dist4 - expDist4) < 1);
        }

        [TestMethod]
        public void Test11_GridGeometry_02()
        {
            var plate = new RectangularPlateWithBolts(500, 340, SteelMaterialEN1993Data.S235, 10, new double[] { 200, 200 }, new double[] { 120, 120 },
                12, BoltMaterialEN1993Data.Class10_9, new Point2d(50, 50));

            ResultBeamForces resultBeamForces = new ResultBeamForces(0, 10, 0, 0, 0, 0, new CoordinateSystem(plate.GetCoordinateSystem()));
            var distE1_1 = plate.CalculateE1(plate.BoltGrid.Bolts[0], resultBeamForces);
            var distE1_2 = plate.CalculateE1(plate.BoltGrid.Bolts[1], resultBeamForces);
            var distE1_4 = plate.CalculateE1(plate.BoltGrid.Bolts[3], resultBeamForces);
            var distE1_7 = plate.CalculateE1(plate.BoltGrid.Bolts[6], resultBeamForces);

            // Solution
            double expDistE1_1 = 450;
            Assert.IsTrue(Math.Abs(distE1_1 - expDistE1_1) < 1);
            double expDistE1_2 = 450;
            Assert.IsTrue(Math.Abs(distE1_2 - expDistE1_2) < 1);
            double expDistE1_4 = 250;
            Assert.IsTrue(Math.Abs(distE1_4 - expDistE1_4) < 1);
            double expDistE1_7 = 50;
            Assert.IsTrue(Math.Abs(distE1_7 - expDistE1_7) < 1);
        }

        [TestMethod]
        public void Test12_GridGeometry_03()
        {
            var plate = new RectangularPlateWithBolts(500, 340, SteelMaterialEN1993Data.S235, 10, new double[] { 200, 200 }, new double[] { 120, 120 },
                12, BoltMaterialEN1993Data.Class10_9, new Point2d(50, 50));

            ResultBeamForces resultBeamForces = new ResultBeamForces(0, 10, 0, 0, 0, 0, new CoordinateSystem(plate.GetCoordinateSystem()));
            var distE2_1 = plate.CalculateE2(plate.BoltGrid.Bolts[0], resultBeamForces);
            var distE2_2 = plate.CalculateE2(plate.BoltGrid.Bolts[1], resultBeamForces);
            var distE2_4 = plate.CalculateE2(plate.BoltGrid.Bolts[3], resultBeamForces);
            var distE2_7 = plate.CalculateE2(plate.BoltGrid.Bolts[6], resultBeamForces);

            double expDistE2_1 = 50;
            Assert.IsTrue(Math.Abs(distE2_1 - expDistE2_1) < 1);
            double expDistE2_2 = 170;
            Assert.IsTrue(Math.Abs(distE2_2 - expDistE2_2) < 1);
            double expDistE2_4 = 50;
            Assert.IsTrue(Math.Abs(distE2_4 - expDistE2_4) < 1);
            double expDistE2_7 = 50;
            Assert.IsTrue(Math.Abs(distE2_7 - expDistE2_7) < 1);
        }

        [TestMethod]
        public void Test13_TensionForceCalculation_01()
        {
            // Pure traction, 9 bolts.
            var plate = new RectangularPlateWithBolts(500, 340,
                SteelMaterialEN1993Data.S235, 10, new double[] { 200, 200 }, new double[] { 120, 120 },
                12, BoltMaterialEN1993Data.Class10_9, new Point2d(50, 50));

            var barSys = new CoordinateSystem(plate.BoltGrid.CalculateBarycenter(), Vector3d.XAxis, Vector3d.YAxis);

            var resultBeamForces = new ResultBeamForces(900, 0, 0, 0, 0, 0, barSys);

            var boltsForces = new Dictionary<BoltPosition, ResultBeamForces>();
            plate.CalculateTensionForcesElastic(resultBeamForces, 15000, boltsForces, out _);

            foreach (var bf in boltsForces)
                Assert.AreEqual(100, bf.Value.N, 0.0001);
        }

        [TestMethod]
        public void Test14_TensionForceCalculation_02()
        {
            // Pure traction, 3 aligned bolts.
            var plate = new RectangularPlateWithBolts(500, 340,
                SteelMaterialEN1993Data.S235, 10, new double[] { 200, 200 }, new double[] { },
                12, BoltMaterialEN1993Data.Class10_9, new Point2d(50, 170));

            var barSys = new CoordinateSystem(plate.BoltGrid.CalculateBarycenter(), Vector3d.XAxis, Vector3d.YAxis);

            var resultBeamForces = new ResultBeamForces(900, 0, 0, 0, 0, 0, barSys);

            var boltsForces = new Dictionary<BoltPosition, ResultBeamForces>();
            plate.CalculateTensionForcesElastic(resultBeamForces, 15000, boltsForces, out _);

            foreach (var bf in boltsForces)
                Assert.AreEqual(300, bf.Value.N, 0.0001);
        }

        [TestMethod]
        public void Test15_TensionForceCalculation_03()
        {
            // Pure traction, 3 aligned bolts, with bolts eccentric to the plate.
            var plate = new RectangularPlateWithBolts(500, 340,
                SteelMaterialEN1993Data.S235, 10, new double[] { 200, 200 }, new double[] { },
                12, BoltMaterialEN1993Data.Class10_9, new Point2d(50, 50));

            var barSys = new CoordinateSystem(plate.BoltGrid.CalculateBarycenter(), Vector3d.XAxis, Vector3d.YAxis);

            var resultBeamForces = new ResultBeamForces(900, 0, 0, 0, 0, 0, barSys);

            var boltsForces = new Dictionary<BoltPosition, ResultBeamForces>();
            plate.CalculateTensionForcesElastic(resultBeamForces, 15000, boltsForces, out _);

            foreach (var bf in boltsForces)
                Assert.AreEqual(300, bf.Value.N, 0.0001);
        }

        [TestMethod]
        public void Test16_TensionForceCalculation_04()
        {
            // Bending moment in one direction, direction X.
            double barRadius = 20;
            double barArea = barRadius * barRadius * Math.PI;
            var barMaterial = BoltMaterialEN1993Data.Class10_9;

            var plate = new PlateWithBolts(
                new Polygon2d(
                    new List<Point2d>()
                    {
                        new Point2d(-400, -500),
                        new Point2d(400, -500),
                        new Point2d(400, 500),
                        new Point2d(-400, 500)
                    }
                    ),
                SteelMaterialEN1993Data.S235,
                new RectangularBoltGrid(
                    new double[] { 700 },
                    new double[] { 700 },
                    40,
                    barMaterial,
                    new Point2d(-350, -350)
                    ),
                10
                );

            var barSys = new CoordinateSystem(new Point2d(0, 1200), Vector3d.XAxis, Vector3d.YAxis);

            var resultBeamForces = new ResultBeamForces(-200000, 0, 0, 0, 0, 0, barSys);

            var boltsForces = new Dictionary<BoltPosition, ResultBeamForces>();

            { // n = 1
                plate.CalculateTensionForcesElastic(resultBeamForces, barMaterial.E, boltsForces, out var minConcrStress);

                // Comparison values with calculation from VCA.
                double referenceConcrStress = -9.604;
                double referenceSteelTension = 70.87 * barArea;

                double maxForceTension = boltsForces.Max(t => t.Value.N);

                double concrRelativeError = Error.CalcRelativeError(minConcrStress, referenceConcrStress);
                double steelRelativeError = Error.CalcRelativeError(maxForceTension, referenceSteelTension);

                Assert.IsTrue(Math.Abs(concrRelativeError) < 0.001);
                Assert.IsTrue(Math.Abs(steelRelativeError) < 0.001);
            }
        }

        [TestMethod]
        public void Test17_TensionForceCalculation_05()
        {
            // Bending moment in one direction, direction Y.
            double barRadius = 20;
            double barArea = barRadius * barRadius * Math.PI;
            var barMaterial = BoltMaterialEN1993Data.Class10_9;

            var plate = new PlateWithBolts(
                new Polygon2d(
                    new List<Point2d>()
                    {
                        new Point2d(-400, -500),
                        new Point2d(400, -500),
                        new Point2d(400, 500),
                        new Point2d(-400, 500)
                    }
                    ),
                SteelMaterialEN1993Data.S235,
                new RectangularBoltGrid(
                    new double[] { 700 },
                    new double[] { 700 },
                    40,
                    barMaterial,
                    new Point2d(-350, -350)
                    ),
                10
                );

            var barSys = new CoordinateSystem(new Point2d(1200, 0), Vector3d.XAxis, Vector3d.YAxis);

            var resultBeamForces = new ResultBeamForces(-200000, 0, 0, 0, 0, 0, barSys);

            var boltsForces = new Dictionary<BoltPosition, ResultBeamForces>();

            { // n = 1
                plate.CalculateTensionForcesElastic(resultBeamForces, barMaterial.E, boltsForces, out var minConcrStress);

                double maxForceTension = boltsForces.Max(t => t.Value.N);

                // Comparison values with calculation from VCA.
                double referenceConcrStressVCA = -10.65;
                double referenceSteelTensionVCA = 90.94 * barArea;

                double concrRelativeErrorVCA = Error.CalcRelativeError(minConcrStress, referenceConcrStressVCA);
                double steelRelativeErrorVCA = Error.CalcRelativeError(maxForceTension, referenceSteelTensionVCA);

                Assert.IsTrue(Math.Abs(concrRelativeErrorVCA) < 0.02);
                Assert.IsTrue(Math.Abs(steelRelativeErrorVCA) < 0.001);

                // Comparison values with calculation from checker (GPCChecker --> GPC.Checkers.Concrete) with fictitious section (this consider holes in concrete area).
                // Risultato più simile a questo.
                double referenceConcrStressCHK = -10.789;
                double referenceSteelTensionCHK = 90.88 * barArea;

                double concrRelativeErrorCHK = Error.CalcRelativeError(minConcrStress, referenceConcrStressCHK);
                double steelRelativeErrorCHK = Error.CalcRelativeError(maxForceTension, referenceSteelTensionCHK);

                Assert.IsTrue(Math.Abs(concrRelativeErrorCHK) < 0.002);
                Assert.IsTrue(Math.Abs(steelRelativeErrorCHK) < 0.001);
            }
        }

        [TestMethod]
        public void Test18_TensionForceCalculation_06()
        {
            // Bending moment in one direction, direction X.
            double barRadius = 20;
            double barArea = barRadius * barRadius * Math.PI;
            var barMaterial = BoltMaterialEN1993Data.Class10_9;

            var plate = new PlateWithBolts(
                new Polygon2d(
                    new List<Point2d>()
                    {
                        new Point2d(-400, -500),
                        new Point2d(400, -500),
                        new Point2d(400, 500),
                        new Point2d(-400, 500)
                    }
                    ),
                SteelMaterialEN1993Data.S235,
                new RectangularBoltGrid(
                    new double[] { 700 },
                    new double[] { 700 },
                    40,
                    barMaterial,
                    new Point2d(-350, -350)
                    ),
                10
                );

            var barSys = new CoordinateSystem(new Point2d(0, -1200), Vector3d.XAxis, Vector3d.YAxis);

            var resultBeamForces = new ResultBeamForces(-200000, 0, 0, 0, 0, 0, barSys);

            var boltsForces = new Dictionary<BoltPosition, ResultBeamForces>();

            { // n = 1
                plate.CalculateTensionForcesElastic(resultBeamForces, barMaterial.E, boltsForces, out var minConcrStress);

                // Comparison values with calculation from VCA.
                double referenceConcrStress = -9.604;
                double referenceSteelTension = 70.87 * barArea;

                double maxForceTension = boltsForces.Max(t => t.Value.N);

                double concrRelativeError = Error.CalcRelativeError(minConcrStress, referenceConcrStress);
                double steelRelativeError = Error.CalcRelativeError(maxForceTension, referenceSteelTension);

                Assert.IsTrue(Math.Abs(concrRelativeError) < 0.001);
                Assert.IsTrue(Math.Abs(steelRelativeError) < 0.001);
            }
        }

        [TestMethod]
        public void Test19_TensionForceCalculation_07()
        {
            // Bending moment in one direction, direction Y.
            double barRadius = 20;
            double barArea = barRadius * barRadius * Math.PI;
            var barMaterial = BoltMaterialEN1993Data.Class10_9;

            var plate = new PlateWithBolts(
                new Polygon2d(
                    new List<Point2d>()
                    {
                        new Point2d(-400, -500),
                        new Point2d(400, -500),
                        new Point2d(400, 500),
                        new Point2d(-400, 500)
                    }
                    ),
                SteelMaterialEN1993Data.S235,
                new RectangularBoltGrid(
                    new double[] { 700 },
                    new double[] { 700 },
                    40,
                    barMaterial,
                    new Point2d(-350, -350)
                    ),
                10
                );

            var barSys = new CoordinateSystem(new Point2d(-1200, 0), Vector3d.XAxis, Vector3d.YAxis);

            var resultBeamForces = new ResultBeamForces(-200000, 0, 0, 0, 0, 0, barSys);

            var boltsForces = new Dictionary<BoltPosition, ResultBeamForces>();

            { // n = 1
                plate.CalculateTensionForcesElastic(resultBeamForces, barMaterial.E, boltsForces, out var minConcrStress);

                double maxForceTension = boltsForces.Max(t => t.Value.N);

                // Comparison values with calculation from VCA.
                double referenceConcrStressVCA = -10.65;
                double referenceSteelTensionVCA = 90.94 * barArea;

                double concrRelativeErrorVCA = Error.CalcRelativeError(minConcrStress, referenceConcrStressVCA);
                double steelRelativeErrorVCA = Error.CalcRelativeError(maxForceTension, referenceSteelTensionVCA);

                Assert.IsTrue(Math.Abs(concrRelativeErrorVCA) < 0.02);
                Assert.IsTrue(Math.Abs(steelRelativeErrorVCA) < 0.001);

                // Comparison values with calculation from checker (GPCChecker --> GPC.Checkers.Concrete) with fictitious section (this consider holes in concrete area).
                // Result most like this.
                double referenceConcrStressCHK = -10.789;
                double referenceSteelTensionCHK = 90.88 * barArea;

                double concrRelativeErrorCHK = Error.CalcRelativeError(minConcrStress, referenceConcrStressCHK);
                double steelRelativeErrorCHK = Error.CalcRelativeError(maxForceTension, referenceSteelTensionCHK);

                Assert.IsTrue(Math.Abs(concrRelativeErrorCHK) < 0.002);
                Assert.IsTrue(Math.Abs(steelRelativeErrorCHK) < 0.001);
            }
        }

        [TestMethod]
        public void Test20_TensionForceCalculation_08()
        {
            double barRadius = 20;
            double barArea = barRadius * barRadius * Math.PI;
            var barMaterial = BoltMaterialEN1993Data.Class10_9;

            var plate = new PlateWithBolts(
                new Polygon2d(
                    new List<Point2d>()
                    {
                        new Point2d(-400, -500),
                        new Point2d(400, -500),
                        new Point2d(400, 500),
                        new Point2d(-400, 500)
                    }
                    ),
                SteelMaterialEN1993Data.S235,
                new RectangularBoltGrid(
                    new double[] { 700 },
                    new double[] { 700 },
                    40,
                    barMaterial,
                    new Point2d(-350, -350)
                    ),
                10
                );

            var barSys = new CoordinateSystem(new Point2d(1200, 1200), Vector3d.XAxis, Vector3d.YAxis);

            var resultBeamForces = new ResultBeamForces(-200000, 0, 0, 0, 0, 0, barSys);

            var boltsForces = new Dictionary<BoltPosition, ResultBeamForces>();

            { // n = 1
                plate.CalculateTensionForcesElastic(resultBeamForces, barMaterial.E, boltsForces, out var minConcrStress);

                double maxForceTension = boltsForces.Max(t => t.Value.N);

                // Comparison values with calculation from VCA.
                double referenceConcrStressVCA = -29.89;
                double referenceSteelTensionVCA = 125.8 * barArea;

                double concrRelativeErrorVCA = Error.CalcRelativeError(minConcrStress, referenceConcrStressVCA);
                double steelRelativeErrorVCA = Error.CalcRelativeError(maxForceTension, referenceSteelTensionVCA);

                Assert.IsTrue(Math.Abs(concrRelativeErrorVCA) < 0.02);
                Assert.IsTrue(Math.Abs(steelRelativeErrorVCA) < 0.002);

                // Comparison values with calculation from checker (GPCChecker --> GPC.Checkers.Concrete) with fictitious section (this consider holes in concrete area).
                // Result most like this.
                double referenceConcrStressCHK = -30.288132765449731;
                double referenceSteelTensionCHK = 125.97976025206988 * barArea;

                double concrRelativeErrorCHK = Error.CalcRelativeError(minConcrStress, referenceConcrStressCHK);
                double steelRelativeErrorCHK = Error.CalcRelativeError(maxForceTension, referenceSteelTensionCHK);

                Assert.IsTrue(Math.Abs(concrRelativeErrorCHK) < 0.001);
                Assert.IsTrue(Math.Abs(steelRelativeErrorCHK) < 0.001);
            }

            { // n = 15
                plate.CalculateTensionForcesElastic(resultBeamForces, barMaterial.E / 15.0, boltsForces, out var minConcrStress);

                double maxForceTension = boltsForces.Max(t => t.Value.N);

                // Comparison values with calculation from VCA.
                double referenceConcrStressVCA = -7.12;
                double referenceSteelTensionVCA = 171.9 * barArea;

                double concrRelativeErrorVCA = Error.CalcRelativeError(minConcrStress, referenceConcrStressVCA);
                double steelRelativeErrorVCA = Error.CalcRelativeError(maxForceTension, referenceSteelTensionVCA);

                Assert.IsTrue(Math.Abs(concrRelativeErrorVCA) < 0.01);
                Assert.IsTrue(Math.Abs(steelRelativeErrorVCA) < 0.005);

                // Comparison values with calculation from checker (GPCChecker --> GPC.Checkers.Concrete) with fictitious section (this consider holes in concrete area).
                // Result most like this.
                double referenceConcrStressCHK = -7.1786896167480494;
                double referenceSteelTensionCHK = 172.53616180090748 * barArea;

                double concrRelativeErrorCHK = Error.CalcRelativeError(minConcrStress, referenceConcrStressCHK);
                double steelRelativeErrorCHK = Error.CalcRelativeError(maxForceTension, referenceSteelTensionCHK);

                Assert.IsTrue(Math.Abs(concrRelativeErrorCHK) < 0.001);
                Assert.IsTrue(Math.Abs(steelRelativeErrorCHK) < 0.001);
            }
        }

        [TestMethod]
        public void Test21_TensionForceCalculation_09()
        {
            // Section with hole.
            double barRadius = 20;
            double barArea = barRadius * barRadius * Math.PI;
            var barMaterial = BoltMaterialEN1993Data.Class10_9;

            var plate = new PlateWithBolts(
                new Polygon2d(
                    new List<Point2d>()
                    {
                        new Point2d(-400, -500),
                        new Point2d(400, -500),
                        new Point2d(400, 500),
                        new Point2d(-400, 500)
                    }
                    ),
                SteelMaterialEN1993Data.S235,
                new RectangularBoltGrid(
                    new double[] { 700 },
                    new double[] { 700 },
                    40,
                    barMaterial,
                    new Point2d(-350, -350)
                    ),
                10,
                new Polygon2d[]
                {
                    new Polygon2d(
                        new List<Point2d>()
                        {
                            new Point2d(-300, 300),
                            new Point2d(300, 300),
                            new Point2d(300, -300),
                            new Point2d(-300, -300)
                        }
                        )
                }
                );

            var barSys = new CoordinateSystem(new Point2d(300, 0), Vector3d.XAxis, Vector3d.YAxis);

            var resultBeamForces = new ResultBeamForces(-2000000, 0, 0, 0, 0, 0, barSys);

            var boltsForces = new Dictionary<BoltPosition, ResultBeamForces>();

            { // n = 1
                plate.CalculateTensionForcesElastic(resultBeamForces, barMaterial.E, boltsForces, out var minConcrStress);

                double maxForceTension = boltsForces.Max(t => t.Value.N);

                // Comparison values with calculation from VCA.
                double referenceConcrStressVCA = -14.16;
                double referenceSteelTensionVCA = 10.06 * barArea;

                double concrRelativeErrorVCA = Error.CalcRelativeError(minConcrStress, referenceConcrStressVCA);
                double steelRelativeErrorVCA = Error.CalcRelativeError(maxForceTension, referenceSteelTensionVCA);

                Assert.IsTrue(Math.Abs(concrRelativeErrorVCA) < 0.02);
                Assert.IsTrue(Math.Abs(steelRelativeErrorVCA) < 0.04);

                // Comparison values with calculation from checker (GPCChecker --> GPC.Checkers.Concrete) with fictitious section (this consider holes in concrete area).
                // Result most like this.
                double referenceConcrStressCHK = -14.441049152944515;
                double referenceSteelTensionCHK = 10.4024006598917915 * barArea;

                double concrRelativeErrorCHK = Error.CalcRelativeError(minConcrStress, referenceConcrStressCHK);
                double steelRelativeErrorCHK = Error.CalcRelativeError(maxForceTension, referenceSteelTensionCHK);

                Assert.IsTrue(Math.Abs(concrRelativeErrorCHK) < 0.001);
                Assert.IsTrue(Math.Abs(steelRelativeErrorCHK) < 0.001);
            }

            { // n = 15
                plate.CalculateTensionForcesElastic(resultBeamForces, barMaterial.E / 15.0, boltsForces, out var minConcrStress);

                double maxForceTension = boltsForces.Max(t => t.Value.N);

                // Comparison values with calculation from VCA.
                double referenceConcrStressVCA = -10.17;
                double referenceSteelTensionVCA = 41.97 * barArea;

                double concrRelativeErrorVCA = Error.CalcRelativeError(minConcrStress, referenceConcrStressVCA);
                double steelRelativeErrorVCA = Error.CalcRelativeError(maxForceTension, referenceSteelTensionVCA);

                Assert.IsTrue(Math.Abs(concrRelativeErrorVCA) < 0.02);
                Assert.IsTrue(Math.Abs(steelRelativeErrorVCA) < 0.03);

                // Comparison values with calculation from checker (GPCChecker --> GPC.Checkers.Concrete) with fictitious section (this consider holes in concrete area).
                // Result most like this.
                double referenceConcrStressCHK = -10.305921519185328;
                double referenceSteelTensionCHK = 42.870403300467034 * barArea;

                double concrRelativeErrorCHK = Error.CalcRelativeError(minConcrStress, referenceConcrStressCHK);
                double steelRelativeErrorCHK = Error.CalcRelativeError(maxForceTension, referenceSteelTensionCHK);

                Assert.IsTrue(Math.Abs(concrRelativeErrorCHK) < 0.001);
                Assert.IsTrue(Math.Abs(steelRelativeErrorCHK) < 0.001);
            }
        }
    }
}
