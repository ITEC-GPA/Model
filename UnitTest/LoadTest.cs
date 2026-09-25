using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace ModelObjectTest
{
    [TestClass]
    public class LoadTest
    {
        

        [TestMethod]
        public void PointLoad4()
        {

            LoadCase loadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);

            // sistema di riferimento locale
            Point3d origin = new Point3d(1, 1, 1);
            Vector3d asseX = new Vector3d(1, 0, 0);
            Vector3d asseY = new Vector3d(0, 1, 0);
            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, "CS");

            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(1, 0, 0);
            Vector3d moment = new Vector3d(0, 0, 0);


            Point3d point = new Point3d(1, 1, 1);
            PointLoad pl1 = new PointLoad(force, moment, point, loadCase, coordinateSystem);


            PointLoad pl1Global = pl1.ToGlobal();


            Console.WriteLine(pl1.GetGlobalLoadVector().force);
            Console.WriteLine(pl1Global.GetGlobalLoadVector().force);


            // Assert
            Assert.IsTrue((Math.Abs(pl1Global.F1 - pl1.F1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.F2 - pl1.F2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.F3 - pl1.F3)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M1 - pl1.M1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M2 - pl1.M2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1Global.M3 - pl1.M3)) < 0.001);
        }

        [TestMethod]
        public void PointLoad5()
        {

            LoadCase loadCase = new LoadCase("SelfWeight", LoadCase.LoadCaseTypes.SelfWeight);

            // sistema di riferimento locale
            Point3d origin = new Point3d(2, 2, 2);
            Point3d asseX = new Point3d(2, 2, 5);
            Point3d asseY = new Point3d(2, 0, 3);
            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, "CS");


            Vector3d force = new Vector3d(1, 0, 0);

            PointLoad pl = new PointLoad(force, new Vector3d(0, 0, 0), new Point3d(2, 2, 2), loadCase, coordinateSystem);

            PointLoad globalLoad = pl.ToGlobal();

            var local = globalLoad.ToLocal(coordinateSystem);

            Assert.AreEqual(globalLoad.GetLocalLoadVector().force, new Vector3d(0, 0, 1));
            Assert.AreEqual(globalLoad.GetLocalLoadVector().moment, new Vector3d(0, 0, 0));

            Assert.AreEqual(local.GetLocalLoadVector().force, force);
            Assert.AreEqual(local.GetLocalLoadVector().moment, new Vector3d(0, 0, 0));

        }


        [TestMethod]
        public void PointLoad6()
        {

            LoadCase loadCase = new LoadCase("SelfWeight", LoadCase.LoadCaseTypes.SelfWeight);

            // sistema di riferimento locale
            Point3d origin = new Point3d(2, 2, 2);
            Point3d asseX = new Point3d(2, 2, 5);
            Point3d asseY = new Point3d(2, 0, 3);
            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, "CS");


            Vector3d force = new Vector3d(1, 1, 0);

            PointLoad pl = new PointLoad(force, new Vector3d(0, 0, 0), new Point3d(2, 2, 2), loadCase, coordinateSystem);

            PointLoad globalLoad = pl.ToGlobal();

            var local = globalLoad.ToLocal(coordinateSystem);

            Assert.AreEqual(globalLoad.GetLocalLoadVector().force, new Vector3d(0, -1, 1));

            Assert.AreEqual(local.GetLocalLoadVector().force, force);
            Assert.AreEqual(local.GetLocalLoadVector().moment, new Vector3d(0, 0, 0));

        }



        [TestMethod]
        public void LineLoad2()
        {
            LoadCase loadCase = new LoadCase("SelfWeight", LoadCase.LoadCaseTypes.SelfWeight);

            // sistema di riferimento locale
            Point3d origin = new Point3d(2, 2, 2);
            Point3d asseX = new Point3d(2, 2, 5);
            Point3d asseY = new Point3d(2, 0, 3);
            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, "CS");

            Vector3d force = new Vector3d(1, 1, 0);

            LineLoad ll = new LineLoad(force, new Vector3d(0, 0, 0), 
                                              new Line3d(new Point3d(0, 1, 2), new Point3d(2, 3, 0)), 
                                              loadCase, 
                                              coordinateSystem);

            LineLoad globalLoad = ll.ToGlobal();

            Assert.AreEqual(globalLoad.GetLocalLoadVector().force, new Vector3d(0, -1, 1) * ll.Line.GetLength());
                        

            LineLoad local = globalLoad.ToLocal(coordinateSystem);
            Assert.AreEqual(local.GetLocalLoadVector().force, force * ll.Line.GetLength() );
            Assert.AreEqual(local.GetLocalLoadVector().moment, new Vector3d(0, 0, 0));
        }



        [TestMethod]
        public void LineLoadConvert1()
        {

            LoadCase loadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseTypes.SelfWeight);


            // sistema di riferimento locale
            Point3d origin = new Point3d(6, -6, 0);
            Point3d p1 = new Point3d(5.02, -14.79, 4.39);
            Point3d p2 = new Point3d(5.56, -1.60, 8.71);

            Vector3d asseX = origin.VectorTo(p1);
            Vector3d asseY = origin.VectorTo(p2);

            CoordinateSystem coordinateSystem = new CoordinateSystem(origin, asseX, asseY, "CS");

            // linea nel globale
            Line3d line = new Line3d(new Point3d(14.91, -5.63, -0.58), new Point3d(9.95, -10.67, 5.88));

            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(1, 2, 3);             // sono forze e momenti per unità di lunghezza
            Vector3d moment = new Vector3d(4, 5, 6);


            LineLoad ll = new LineLoad(force, moment, line, loadCase, coordinateSystem);


            Point3d originPlane = new Point3d(14.91, -5.63, -0.58);
            Plane referencePlane = new Plane(originPlane,
                                             originPlane.VectorTo(new Point3d(14.99, -7.22, -1.76)),
                                             originPlane.VectorTo(new Point3d(12.43, -8.15, 2.65)));
            var areaLoad = ll.ConvertToAreaLoad(referencePlane, 3);

            var borders = areaLoad.Shape.Fill.Explode();

            // Assert
            Assert.AreEqual(9.58, borders[0].GetLength(), 0.01);
            Assert.AreEqual(3.00, borders[1].GetLength(), 0.01);
            Assert.AreEqual(9.58, borders[2].GetLength(), 0.01);
            Assert.AreEqual(3.00, borders[3].GetLength(), 0.01);

            var fillGlobal = areaLoad.Shape.Fill;

            Assert.AreEqual(14.84, fillGlobal[0].X, 0.1);
            Assert.AreEqual(9.89,  fillGlobal[1].X, 0.1);
            Assert.AreEqual(10.01, fillGlobal[2].X, 0.1);
            Assert.AreEqual(14.97, fillGlobal[3].X, 0.1);

            Assert.AreEqual(-4.42,  fillGlobal[0].Y, 0.01);
            Assert.AreEqual(-9.47,  fillGlobal[1].Y, 0.01);
            Assert.AreEqual(-11.87, fillGlobal[2].Y, 0.01);
            Assert.AreEqual(-6.83,  fillGlobal[3].Y, 0.01);

            var localAreaVector = areaLoad.GetLocalLoadVector();

            var globalForce = coordinateSystem.ToGlobal(force);
            Assert.AreEqual(globalForce.X * line.GetLength(), localAreaVector.X, 0.2);
            Assert.AreEqual(globalForce.Y * line.GetLength(), localAreaVector.Y, 0.2);
            Assert.AreEqual(globalForce.Z * line.GetLength(), localAreaVector.Z, 0.2);

            Assert.AreEqual(ll.GetLocalLoadVector().force.Length, 
                            areaLoad.GetLocalLoadVector().Length, 0.001, 
                            $"{ll.GetLocalLoadVector().force.Length} {areaLoad.GetLocalLoadVector().Length}");

        }

        [TestMethod]
        public void PointLoadConvert1()
        {
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