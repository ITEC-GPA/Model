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

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseType.SelfWeight);

            PointLoad pl1 = new PointLoad(force, moment, point, LoadCase, CoordinateSystem1);

            // PointLoad previsto nel globale
            Vector3d expForce = new Vector3d(0, 1, 0);
            Vector3d expMoment = new Vector3d(0, 0, 1);
            Point3d expPoint = new Point3d(3, 0, 0);

            PointLoad expl1 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            PointLoad pl2 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            pl1.ToGlobal();

            double[] GlobalForces = pl1.GetGlobalForces();

            double[] LocalForces = pl2.GetLocalForces(CoordinateSystem1);

            // Assert
            Assert.IsTrue((Math.Abs(pl1.F1 - expl1.F1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.F2 - expl1.F2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.F3 - expl1.F3)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.M1 - expl1.M1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.M2 - expl1.M2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.M3 - expl1.M3)) < 0.001);
            Assert.IsTrue(Math.Abs(pl1.Point.X - expPoint.X) < 0.001);
            Assert.IsTrue(Math.Abs(pl1.Point.Y - expPoint.Y) < 0.001);
            Assert.IsTrue(Math.Abs(pl1.Point.Z - expPoint.Z) < 0.001);
            
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

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseType.SelfWeight);

            PointLoad pl1 = new PointLoad(force, moment, point, LoadCase, CoordinateSystem1);

            // PointLoad previsto nel globale
            Vector3d expForce = new Vector3d(+1,-1, -1);
            Vector3d expMoment = new Vector3d(-1, +1, 0);
            Point3d expPoint = new Point3d(1, -2, 5);

            PointLoad expl1 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            PointLoad pl2 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            pl1.ToGlobal();

            double[] GlobalForces = pl1.GetGlobalForces();

            double[] LocalForces = pl2.GetLocalForces(CoordinateSystem1);

            // Assert
            Assert.IsTrue((Math.Abs(pl1.F1 - expl1.F1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.F2 - expl1.F2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.F3 - expl1.F3)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.M1 - expl1.M1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.M2 - expl1.M2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.M3 - expl1.M3)) < 0.001);
            Assert.IsTrue(Math.Abs(pl1.Point.X - expPoint.X) < 0.001);
            Assert.IsTrue(Math.Abs(pl1.Point.Y - expPoint.Y) < 0.001);
            Assert.IsTrue(Math.Abs(pl1.Point.Z - expPoint.Z) < 0.001);
            
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

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseType.SelfWeight);

            PointLoad pl1 = new PointLoad(force, moment, point, LoadCase, CoordinateSystem1);

            // PointLoad previsto nel globale
            Vector3d expForce = new Vector3d(+1, +1, +1);
            Vector3d expMoment = new Vector3d(-1, -1, -1);
            Point3d expPoint = new Point3d(0, 0, 0);

            PointLoad expl1 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            PointLoad pl2 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            pl1.ToGlobal();

            double[] GlobalForces = pl1.GetGlobalForces();

            double[] LocalForces = pl2.GetLocalForces(CoordinateSystem1);

            // Assert
            Assert.IsTrue((Math.Abs(pl1.F1 - expl1.F1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.F2 - expl1.F2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.F3 - expl1.F3)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.M1 - expl1.M1)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.M2 - expl1.M2)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.M3 - expl1.M3)) < 0.001);
            Assert.IsTrue(Math.Abs(pl1.Point.X - expPoint.X) < 0.001);
            Assert.IsTrue(Math.Abs(pl1.Point.Y - expPoint.Y) < 0.001);
            Assert.IsTrue(Math.Abs(pl1.Point.Z - expPoint.Z) < 0.001);

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

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseType.SelfWeight);

            LineLoad ll1 = new LineLoad(force, moment, line, LoadCase, CoordinateSystem1);

            // PointLoad previsto nel globale
            Vector3d expForce = new Vector3d(0, 1, 0);
            Vector3d expMoment = new Vector3d(0, 0, 1);
            Point3d expPoint = new Point3d(3, 0, 0);

            PointLoad expl1 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            PointLoad pl2 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global);

            ll1.ToGlobal();

            double[] GlobalForces = ll1.GetGlobalForces();

            double[] LocalForces = pl2.GetLocalForces(CoordinateSystem1);

            // Assert
            Assert.IsTrue((Math.Abs(ll1.F1 - expl1.F1)) < 0.001);
            Assert.IsTrue((Math.Abs(ll1.F2 - expl1.F2)) < 0.001);
            Assert.IsTrue((Math.Abs(ll1.F3 - expl1.F3)) < 0.001);
            Assert.IsTrue((Math.Abs(ll1.M1 - expl1.M1)) < 0.001);
            Assert.IsTrue((Math.Abs(ll1.M2 - expl1.M2)) < 0.001);
            Assert.IsTrue((Math.Abs(ll1.M3 - expl1.M3)) < 0.001);
            Assert.IsTrue(Math.Abs(ll1.Line.Start.X - expPoint.X) < 0.001);
            Assert.IsTrue(Math.Abs(ll1.Line.Start.Y - expPoint.Y) < 0.001);
            Assert.IsTrue(Math.Abs(ll1.Line.Start.Z - expPoint.Z) < 0.001);

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
    }
}