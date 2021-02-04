using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace UnitTest
{
    [TestClass]
    public class LoadTest
    {
        [TestMethod]
        public void ToGlobal() 
        {
            // sistema di riferimento locale
            Point3d Origin = new Point3d(2, 0, 0);
            Point3d AsseX = new Point3d(2, 2, 0);
            Point3d AsseY = new Point3d(2, 0, 2);
            CoordinateSystem CoordinateSystem1 = new CoordinateSystem(Origin, AsseX, AsseY, 0.0, "CS", new Guid());

            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(1, 0, 0);
            Vector3d moment = new Vector3d(0, 1, 0);
            Point3d point = new Point3d(0, 0, 1);

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseType.SelfWeight);

            PointLoad pl1 = new PointLoad(force, moment, point, LoadCase, CoordinateSystem1, new Guid());

            // PointLoad previsto nel globale
            Vector3d expForce = new Vector3d(0, 1, 0);
            Vector3d expMoment = new Vector3d(0, 0, 1);
            Point3d expPoint = new Point3d(3, 0, 0);

            PointLoad expl1 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global, Guid.NewGuid());

            PointLoad pl2 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global, Guid.NewGuid());

            pl1.ToGlobal();

            double[] GlobalForces = pl1.GetGlobalForces();

            double[] LocalForces = pl2.GetLocalForces(CoordinateSystem1);

            // Assert
            Assert.IsTrue((Math.Abs(pl1.Fx - expl1.Fx)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Fy - expl1.Fy)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Fz - expl1.Fz)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Mx - expl1.Mx)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.My - expl1.My)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Mz - expl1.Mz)) < 0.001);
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
        public void ToGlobal2()
        {
            // sistema di riferimento locale
            Point3d Origin = new Point3d(2, 0, 5);
            Point3d AsseX = new Point3d(2, 2, 5);
            Point3d AsseY = new Point3d(2, 0, 3);
            CoordinateSystem CoordinateSystem1 = new CoordinateSystem(Origin, AsseX, AsseY, 0.0, "CS", new Guid());

            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(-1, +1, -1);
            Vector3d moment = new Vector3d(1, 0, 1);
            Point3d point = new Point3d(-2, 0, 1);

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseType.SelfWeight);

            PointLoad pl1 = new PointLoad(force, moment, point, LoadCase, CoordinateSystem1, new Guid());

            // PointLoad previsto nel globale
            Vector3d expForce = new Vector3d(+1,-1, -1);
            Vector3d expMoment = new Vector3d(-1, +1, 0);
            Point3d expPoint = new Point3d(1, -2, 5);

            PointLoad expl1 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global, Guid.NewGuid());

            PointLoad pl2 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global, Guid.NewGuid());

            pl1.ToGlobal();

            double[] GlobalForces = pl1.GetGlobalForces();

            double[] LocalForces = pl2.GetLocalForces(CoordinateSystem1);

            // Assert
            Assert.IsTrue((Math.Abs(pl1.Fx - expl1.Fx)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Fy - expl1.Fy)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Fz - expl1.Fz)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Mx - expl1.Mx)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.My - expl1.My)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Mz - expl1.Mz)) < 0.001);
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
        public void ToGlobal3()
        {
            // sistema di riferimento locale
            Point3d Origin = new Point3d(-2, -2, -2);
            Point3d AsseX = new Point3d(-2, -2, -4);
            Point3d AsseY = new Point3d(-2, 0, -2);
            CoordinateSystem CoordinateSystem1 = new CoordinateSystem(Origin, AsseX, AsseY, 0.0, "CS", new Guid());

            // PointLoad nel sistema locale
            Vector3d force = new Vector3d(-1, +1, +1);
            Vector3d moment = new Vector3d(+1, -1, -1);
            Point3d point = new Point3d(-2, 2, 2);

            LoadCase LoadCase = new LoadCase("SelfWeight", loadCaseType: GPC.Model.LoadCases.LoadCase.LoadCaseType.SelfWeight);

            PointLoad pl1 = new PointLoad(force, moment, point, LoadCase, CoordinateSystem1, new Guid());

            // PointLoad previsto nel globale
            Vector3d expForce = new Vector3d(+1, +1, +1);
            Vector3d expMoment = new Vector3d(-1, -1, -1);
            Point3d expPoint = new Point3d(0, 0, 0);

            PointLoad expl1 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global, Guid.NewGuid());

            PointLoad pl2 = new PointLoad(expForce, expMoment, expPoint, LoadCase, CoordinateSystem.Global, Guid.NewGuid());

            pl1.ToGlobal();

            double[] GlobalForces = pl1.GetGlobalForces();

            double[] LocalForces = pl2.GetLocalForces(CoordinateSystem1);

            // Assert
            Assert.IsTrue((Math.Abs(pl1.Fx - expl1.Fx)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Fy - expl1.Fy)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Fz - expl1.Fz)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Mx - expl1.Mx)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.My - expl1.My)) < 0.001);
            Assert.IsTrue((Math.Abs(pl1.Mz - expl1.Mz)) < 0.001);
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
    }
}