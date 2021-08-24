using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace ModelObjectTest
{
    [TestClass]
    public class LoadTest : UnitTestBase
    {
        [TestMethod]
        public void PointLoad1()
        {
            // sistema di riferimento locale
            Point3d Origin = new Point3d(2, 0, 0);
            Point3d AsseX = new Point3d(2, 2, 0);
            Point3d AsseY = new Point3d(2, 0, 2);
            CoordinateSystem CoordinateSystem1 = new CoordinateSystem(Origin, AsseX, AsseY, "CS", new Guid());

            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(1, 0, 0);
            Vector3d moment = new Vector3d(0, 1, 0);
            Point3d point = new Point3d(0, 0, 1);

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);

            PointLoad pl1 = new PointLoad(force, moment, point, LoadCase, CoordinateSystem1);

            // PointLoad previsto nel globale
            Vector3d expForce = new Vector3d(0, 1, 0);
            Vector3d expMoment = new Vector3d(0, 0, 1);
            Point3d expPoint = new Point3d(3, 0, 0);

            PointLoad expl1 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            PointLoad pl2 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            var pl1Global = pl1.ToGlobal();

            double[] GlobalForces = pl1.GetGlobalForces();

            double[] LocalForces = pl2.GetLocalForces(CoordinateSystem1);

            // Assert
            Assert.IsTrue((Math.Abs(pl1Global.F1 - expl1.F1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.F2 - expl1.F2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.F3 - expl1.F3)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M1 - expl1.M1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M2 - expl1.M2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M3 - expl1.M3)) < 0.001);
            Assert.IsTrue(Math.Abs(pl1Global.Point.X - expPoint.X) < 0.001);
            Assert.IsTrue(Math.Abs(pl1Global.Point.Y - expPoint.Y) < 0.001);
            Assert.IsTrue(Math.Abs(pl1Global.Point.Z - expPoint.Z) < 0.001);

            Assert.IsTrue((Math.Abs(GlobalForces[0] - expForce.X) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[1] - expForce.Y) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[2] - expForce.Z) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[3] - expMoment.X) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[4] - expMoment.Y) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[5] - expMoment.Z) < 0.001));

            Assert.IsTrue((Math.Abs(LocalForces[0] - force.X) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[1] - force.Y) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[2] - force.Z) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[3] - moment.X) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[4] - moment.Y) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[5] - moment.Z) < 0.001));
        }

        [TestMethod]
        public void PointLoad2()
        {
            // sistema di riferimento locale
            Point3d Origin = new Point3d(2, 0, 5);
            Point3d AsseX = new Point3d(2, 2, 5);
            Point3d AsseY = new Point3d(2, 0, 3);
            CoordinateSystem CoordinateSystem1 = new CoordinateSystem(Origin, AsseX, AsseY, "CS", new Guid());

            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(-1, +1, -1);
            Vector3d moment = new Vector3d(1, 0, 1);
            Point3d point = new Point3d(-2, 0, 1);

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);

            PointLoad pl1 = new PointLoad(force, moment, point, LoadCase, CoordinateSystem1);

            // PointLoad previsto nel globale
            Vector3d expForce = new Vector3d(+1, -1, -1);
            Vector3d expMoment = new Vector3d(-1, +1, 0);
            Point3d expPoint = new Point3d(1, -2, 5);

            PointLoad expl1 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            PointLoad pl2 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            var pl1Global = pl1.ToGlobal();

            double[] GlobalForces = pl1.GetGlobalForces();

            double[] LocalForces = pl2.GetLocalForces(CoordinateSystem1);

            // Assert
            Assert.IsTrue((Math.Abs(pl1Global.F1 - expl1.F1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.F2 - expl1.F2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.F3 - expl1.F3)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M1 - expl1.M1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M2 - expl1.M2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M3 - expl1.M3)) < 0.001);
            Assert.IsTrue(Math.Abs(pl1Global.Point.X - expPoint.X) < 0.001);
            Assert.IsTrue(Math.Abs(pl1Global.Point.Y - expPoint.Y) < 0.001);
            Assert.IsTrue(Math.Abs(pl1Global.Point.Z - expPoint.Z) < 0.001);

            Assert.IsTrue((Math.Abs(GlobalForces[0] - expForce.X) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[1] - expForce.Y) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[2] - expForce.Z) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[3] - expMoment.X) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[4] - expMoment.Y) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[5] - expMoment.Z) < 0.001));

            Assert.IsTrue((Math.Abs(LocalForces[0] - force.X) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[1] - force.Y) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[2] - force.Z) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[3] - moment.X) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[4] - moment.Y) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[5] - moment.Z) < 0.001));
        }

        [TestMethod]
        public void PointLoad3()
        {
            // sistema di riferimento locale
            Point3d Origin = new Point3d(-2, -2, -2);
            Point3d AsseX = new Point3d(-2, -2, -4);
            Point3d AsseY = new Point3d(-2, 0, -2);
            CoordinateSystem CoordinateSystem1 = new CoordinateSystem(Origin, AsseX, AsseY, "CS", new Guid());

            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(-1, +1, +1);
            Vector3d moment = new Vector3d(+1, -1, -1);
            Point3d point = new Point3d(-2, 2, 2);

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);

            PointLoad pl1 = new PointLoad(force, moment, point, LoadCase, CoordinateSystem1);

            // PointLoad previsto nel globale
            Vector3d expForce = new Vector3d(+1, +1, +1);
            Vector3d expMoment = new Vector3d(-1, -1, -1);
            Point3d expPoint = new Point3d(0, 0, 0);

            PointLoad expl1 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            PointLoad pl2 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            var pl1Global = pl1.ToGlobal();

            double[] GlobalForces = pl1.GetGlobalForces();

            double[] LocalForces = pl2.GetLocalForces(CoordinateSystem1);

            // Assert
            Assert.IsTrue((Math.Abs(pl1Global.F1 - expl1.F1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.F2 - expl1.F2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.F3 - expl1.F3)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M1 - expl1.M1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M2 - expl1.M2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M3 - expl1.M3)) < 0.001);
            Assert.IsTrue(Math.Abs(pl1Global.Point.X - expPoint.X) < 0.001);
            Assert.IsTrue(Math.Abs(pl1Global.Point.Y - expPoint.Y) < 0.001);
            Assert.IsTrue(Math.Abs(pl1Global.Point.Z - expPoint.Z) < 0.001);

            Assert.IsTrue((Math.Abs(GlobalForces[0] - expForce.X) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[1] - expForce.Y) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[2] - expForce.Z) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[3] - expMoment.X) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[4] - expMoment.Y) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[5] - expMoment.Z) < 0.001));

            Assert.IsTrue((Math.Abs(LocalForces[0] - force.X) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[1] - force.Y) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[2] - force.Z) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[3] - moment.X) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[4] - moment.Y) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[5] - moment.Z) < 0.001));

        }

        [TestMethod]
        public void LineLoad1()
        {
            // sistema di riferimento locale
            Point3d Origin = new Point3d(2, 0, 0);
            Point3d AsseX = new Point3d(2, 2, 0);
            Point3d AsseY = new Point3d(2, 0, 2);
            CoordinateSystem CoordinateSystem1 = new CoordinateSystem(Origin, AsseX, AsseY, "CS", new Guid());

            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(1, 0, 0);             // sono forze e momenti per unità di lunghezza
            Vector3d moment = new Vector3d(0, 1, 0);
            Point3d point1 = new Point3d(0, 0, 1);
            Line3d line = new Line3d(point1, point1);

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);

            LineLoad ll1 = new LineLoad(force, moment, line, LoadCase, CoordinateSystem1);

            // PointLoad previsto nel globale
            Vector3d expForce = new Vector3d(0, 1, 0);
            Vector3d expMoment = new Vector3d(0, 0, 1);
            Point3d expPoint = new Point3d(3, 0, 0);

            PointLoad expl1 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            PointLoad pl2 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            var ll1Global = ll1.ToGlobal();

            double[] GlobalForces = ll1.GetGlobalForces();

            double[] LocalForces = pl2.GetLocalForces(CoordinateSystem1);

            // Assert
            Assert.IsTrue((Math.Abs(ll1Global.F1 - expl1.F1)) < 0.001, ll1Global.F1.ToString());
            Assert.IsTrue((Math.Abs(ll1Global.F2 - expl1.F2)) < 0.001);
            Assert.IsTrue((Math.Abs(ll1Global.F3 - expl1.F3)) < 0.001);
            Assert.IsTrue((Math.Abs(ll1Global.M1 - expl1.M1)) < 0.001);
            Assert.IsTrue((Math.Abs(ll1Global.M2 - expl1.M2)) < 0.001);
            Assert.IsTrue((Math.Abs(ll1Global.M3 - expl1.M3)) < 0.001);
            Assert.IsTrue(Math.Abs(ll1Global.Line.Start.X - expPoint.X) < 0.001);
            Assert.IsTrue(Math.Abs(ll1Global.Line.Start.Y - expPoint.Y) < 0.001);
            Assert.IsTrue(Math.Abs(ll1Global.Line.Start.Z - expPoint.Z) < 0.001);

            Assert.IsTrue((Math.Abs(GlobalForces[0] - expForce.X) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[1] - expForce.Y) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[2] - expForce.Z) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[3] - expMoment.X) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[4] - expMoment.Y) < 0.001));
            Assert.IsTrue((Math.Abs(GlobalForces[5] - expMoment.Z) < 0.001));

            Assert.IsTrue((Math.Abs(LocalForces[0] - force.X) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[1] - force.Y) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[2] - force.Z) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[3] - moment.X) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[4] - moment.Y) < 0.001));
            Assert.IsTrue((Math.Abs(LocalForces[5] - moment.Z) < 0.001));
        }

        [TestMethod]
        public void LineLoadConvert1()
        {
            // sistema di riferimento locale
            Point3d origin = new Point3d(8.00, 3.00, -7.00);
            Point3d asseX = new Point3d(0.00, -11.00, 12.00);
            Point3d asseY = new Point3d(19.23, -1.10, -5.29);
            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, "CS");


            // linea nel locale
            Line3d line = new Line3d(new Point3d(0, 0, 0), new Point3d(24.92, 0, 0));


            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(1, 2, 3);             // sono forze e momenti per unità di lunghezza
            Vector3d moment = new Vector3d(4, 5, 6);

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);

            LineLoad ll = new LineLoad(force, moment, line, LoadCase, coordinateSystem);


            Plane referencePlane = new Plane(new Point3d(8.00, 3.00, -7.00),
                                             new Vector3d(-0.321029, -0.561801, 0.762444),
                                             new Vector3d(0.440778, -0.80118, -0.404752));
            var areaLoad = ll.ConvertToAreaLoad(referencePlane, 3);

            var borders = areaLoad.Shape.Fill.Explode();

            // Assert
            Assert.AreEqual(24.92, borders[0].GetLength(), 0.01);
            Assert.AreEqual(3.00,  borders[1].GetLength(), 0.01);
            Assert.AreEqual(24.92, borders[2].GetLength(), 0.01);
            Assert.AreEqual(3.00,  borders[3].GetLength(), 0.01);

            var fillGlobal = areaLoad.CoordinateSystem.ToGlobal(areaLoad.Shape.Fill);

            Assert.AreEqual(8.66,  fillGlobal[3].X, 0.01);
            Assert.AreEqual(0.66,  fillGlobal[2].X, 0.01);
            Assert.AreEqual(-0.66, fillGlobal[1].X, 0.01);
            Assert.AreEqual(7.33,  fillGlobal[0].X, 0.01);

            Assert.AreEqual(+1.79,  fillGlobal[3].Y, 0.01);
            Assert.AreEqual(-12.20, fillGlobal[2].Y, 0.01);
            Assert.AreEqual(-9.79,  fillGlobal[1].Y, 0.01);
            Assert.AreEqual(+4.20,  fillGlobal[0].Y, 0.01);

            var localAreaVector = areaLoad.GetLocalLoadVector();
            Assert.AreEqual(+1.0000 * line.GetLength(), localAreaVector.X, 0.2);
            Assert.AreEqual(-1.0932 * line.GetLength(), localAreaVector.Y, 0.2);
            Assert.AreEqual(+3.4358 * line.GetLength(), localAreaVector.Z, 0.2);

            Assert.AreEqual(ll.GetLocalLoadVector().force.Length, areaLoad.GetLocalLoadVector().Length, 0.001, 
                $"{ll.GetLocalLoadVector().force.Length} {areaLoad.GetLocalLoadVector().Length}");

        }

        [TestMethod]
        public void LineLoadConvert2()
        {
            // sistema di riferimento locale
            Point3d origin = new Point3d(8.00, 3.00, -7.00);
            Point3d asseX = new Point3d(0.00, -11.00, 12.00);
            Point3d asseY = new Point3d(19.23, -1.10, -5.29);
            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, "CS");


            // linea nel locale
            Line3d line = new Line3d(new Point3d(0, 0, 0), new Point3d(24.92, 0, 0));


            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(1, 2, 3);             // sono forze e momenti per unità di lunghezza
            Vector3d moment = new Vector3d(4, 5, 6);

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);

            LineLoad ll = new LineLoad(force, moment, line, LoadCase, coordinateSystem);


            Plane referencePlane = new Plane(new Point3d(8.00, 3.00, -7.00),
                                             new Vector3d(-0.321029, -0.561801, 0.762444),
                                             new Vector3d(0.440778, -0.80118, -0.404752));
            var areaLoad = ll.ConvertToNormalAreaLoad(referencePlane, 3);

            var borders = areaLoad.Shape.Fill.Explode();

            // Assert
            Assert.AreEqual(24.92, borders[0].GetLength(), 0.01);
            Assert.AreEqual(3.00,  borders[1].GetLength(), 0.01);
            Assert.AreEqual(24.92, borders[2].GetLength(), 0.01);
            Assert.AreEqual(3.00,  borders[3].GetLength(), 0.01);

            var fillGlobal = areaLoad.CoordinateSystem.ToGlobal(areaLoad.Shape.Fill);

            Assert.AreEqual(8.66, fillGlobal[3].X, 0.01);
            Assert.AreEqual(0.66, fillGlobal[2].X, 0.01);
            Assert.AreEqual(-0.66, fillGlobal[1].X, 0.01);
            Assert.AreEqual(7.33, fillGlobal[0].X, 0.01);

            Assert.AreEqual(+1.79, fillGlobal[3].Y, 0.01);
            Assert.AreEqual(-12.20, fillGlobal[2].Y, 0.01);
            Assert.AreEqual(-9.79, fillGlobal[1].Y, 0.01);
            Assert.AreEqual(+4.20, fillGlobal[0].Y, 0.01);

            var localAreaVector = areaLoad.GetLocalLoadVector();
            Assert.AreEqual(0, localAreaVector.X, 0.0001);
            Assert.AreEqual(0, localAreaVector.Y, 0.0001);
            Assert.AreEqual(+3.4358 * line.GetLength(), localAreaVector.Z, 0.2);

            Assert.AreEqual(ll.GetLocalLoadVector().force.Length, areaLoad.GetLocalLoadVector().Length, 0.001, 
                $"{ll.GetLocalLoadVector().force.Length} {areaLoad.GetLocalLoadVector().Length}");

        }



        [TestMethod]
        public void PointLoadConvert1()
        {
            Vector3d v = new Vector3d(0, 1, 2);
            v /= 2;

            Console.WriteLine(v);


            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(1, 2, 3);             // sono forze e momenti per unità di lunghezza
            Vector3d moment = new Vector3d(4, 5, 6);

            LoadCase loadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);

            PointLoad load = new PointLoad(force, moment, new Point3d(500, 500, 0), loadCase, CoordinateSystem.Global);

            Plane referencePlane = new Plane(new Point3d(0, 0, 0),
                                             new Vector3d(1, 0, 0),
                                             new Vector3d(0, 1, 0));

            var areaLoad = load.ConvertToNormalAreaLoad(referencePlane, 3);

            var borders = areaLoad.Shape.Fill.Explode();

            // Assert
            Assert.AreEqual(3, borders[0].GetLength(), 0.01);
            Assert.AreEqual(3, borders[1].GetLength(), 0.01);
            Assert.AreEqual(3, borders[2].GetLength(), 0.01);
            Assert.AreEqual(3, borders[3].GetLength(), 0.01);

            Assert.AreEqual(load.F3 / (3 * 3), areaLoad.Pressure, 0.01);

        }


    }
}