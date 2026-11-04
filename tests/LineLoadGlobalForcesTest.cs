using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// <see cref="LineLoad.GetGlobalForces"/> rotates the components to the global axes and nothing else (user decision of 4/11/2026,
    /// «correggilo», lane L9): before, the origin of the coordinate system was subtracted from the rotated vectors, so a load in beam axes
    /// with the origin at the start node had its intensities shifted by the coordinates of the node (documented by lane P2B).
    /// </summary>
    [TestClass]
    public class LineLoadGlobalForcesTest
    {
        private static readonly LoadCaseBase Case = new LoadCaseBase(Guid.Parse("6f3c2a10-0000-4000-8000-000000000001"), "Q");
        private static long[] Bits(double[] values) => values.Select(BitConverter.DoubleToInt64Bits).ToArray();

        private static CoordinateSystem BeamAxes(Point3d origin)
        {
            double s = Math.Sqrt(0.5);
            return new CoordinateSystem(origin, new Vector3d(s, s, 0), new Vector3d(-s, s, 0), "beam");
        }

        [TestMethod]
        public void BeamAxesAtTheStartNodeGiveTheRotatedComponents()
        {
            var start = new Point3d(1000, 2000, 300); var end = new Point3d(2000, 3000, 300);
            var axes = BeamAxes(start);
            var load = new LineLoad(1.5, -2.25, -5, 0.5, 0, 7, new Line3d(start, end), Case, axes);
            var global = load.GetGlobalForces();
            var force = axes.ToGlobal(new Vector3d(1.5, -2.25, -5)); var moment = axes.ToGlobal(new Vector3d(0.5, 0, 7));
            CollectionAssert.AreEqual(Bits(new[] { force.X, force.Y, force.Z, moment.X, moment.Y, moment.Z }), Bits(global));
            // the same as the load moved to the global axes, and as a point load in the same axes
            var moved = load.ToGlobal();
            CollectionAssert.AreEqual(Bits(new[] { moved.F1, moved.F2, moved.F3, moved.M1, moved.M2, moved.M3 }), Bits(global));
            CollectionAssert.AreEqual(Bits(new PointLoad(1.5, -2.25, -5, 0.5, 0, 7, start, Case, axes).GetGlobalForces()), Bits(global));
            // the origin does not enter: the same axes at the global origin give the same components
            CollectionAssert.AreEqual(Bits(new LineLoad(1.5, -2.25, -5, 0.5, 0, 7, new Line3d(start, end), Case, BeamAxes(new Point3d(0, 0, 0))).GetGlobalForces()), Bits(global));
            // a vertical load in the beam axes stays vertical: Z = -5, nothing along X or Y (before: X - 1000, Y - 2000, Z - 300)
            Assert.AreEqual(-5, global[2], 1e-12);
            Assert.AreEqual(1.5 * Math.Sqrt(0.5) + 2.25 * Math.Sqrt(0.5), global[0], 1e-12);
        }

        /// <summary>For axes at the global origin the components are those of before, bit for bit (the subtraction of +0 changed nothing).</summary>
        [TestMethod]
        public void AxesAtTheGlobalOriginGiveTheValuesOfBefore()
        {
            var line = new Line3d(new Point3d(0, 0, 0), new Point3d(5, 0, 0));
            foreach (var axes in new[] { CoordinateSystem.Global, BeamAxes(new Point3d(0, 0, 0)), new CoordinateSystem(new Point3d(0, 0, 0), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0)) })
            {
                var load = new LineLoad(-0.0, 3.1, -7.7, 1e-300, -2.5, 0, line, Case, axes);
                var before = new double[6];
                var f = axes.ToGlobal(new Vector3d(-0.0, 3.1, -7.7)); var m = axes.ToGlobal(new Vector3d(1e-300, -2.5, 0));
                before[0] = f.X - axes.Origin.X; before[1] = f.Y - axes.Origin.Y; before[2] = f.Z - axes.Origin.Z;
                before[3] = m.X - axes.Origin.X; before[4] = m.Y - axes.Origin.Y; before[5] = m.Z - axes.Origin.Z;
                CollectionAssert.AreEqual(Bits(before), Bits(load.GetGlobalForces()), axes.Name);
            }
        }
    }
}
