using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core.Coordinates;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Models;
using GPC.Model.Results.Processing;
using GPC.Model.Structure.Topology;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// 5/11/2026: the planarity of the shells is relative to their size (ShellPlanarity). A real Straus7 model (2507 planar
    /// quads up to 9 m from the origin) was rejected because the round-off of its coordinates (a deviation from the mean plane up to
    /// 0.24 µm) exceeded the absolute GeometryBase.Tolerance of 1e-4 mm. GeometryBase.Tolerance does not change.
    /// </summary>
    [TestClass]
    public class ShellPlanarityTest
    {
        private const double Side = 350, X0 = 9083, Y0 = 4000, Z0 = 1500;

        /// <summary>A square of side 350 mm far from the origin, twisted by ±<paramref name="twist"/> mm: every corner is exactly
        /// <paramref name="twist"/> from the mean plane.</summary>
        private static Point3d[] Twisted(double twist, double side = Side) => new[]
        {
            new Point3d(X0, Y0, Z0 + twist), new Point3d(X0 + side, Y0, Z0 - twist),
            new Point3d(X0 + side, Y0 + side, Z0 + twist), new Point3d(X0, Y0 + side, Z0 - twist),
        };

        private static (Model Model, AreaElement Shell) Connected(Point3d[] corners)
        {
            var model = new Model();
            var nodes = corners.Select(p => new NodeElement(p)).ToArray();
            foreach (var node in nodes) model.NodesElements.Add(node);
            var shell = new AreaElement(null, null); shell.ConnectNodes(nodes); model.AreaElements.Add(shell);
            return (model, shell);
        }

        [TestMethod]
        public void ToleranceIsRelativeToThePlateWithTheAbsoluteFloor()
        {
            double diagonal = Side * Math.Sqrt(2);
            Assert.AreEqual(diagonal, ShellPlanarity.LongestMm(Twisted(0)), 1e-9);
            Assert.AreEqual(1e-5 * diagonal, ShellPlanarity.ToleranceMm(Twisted(0)), 1e-15);
            Assert.AreEqual(GeometryBase.Tolerance, ShellPlanarity.ToleranceMm(Twisted(0, 5)), 0, "a 5 mm plate keeps the absolute 1e-4 mm");
            Assert.AreEqual(1e-3, ShellPlanarity.ToleranceMm(Twisted(0, 5), 1e-3), 0, "an explicit absolute floor is kept");
            Assert.AreEqual(5e-4, ShellPlanarity.DeviationMm(Twisted(5e-4)), 1e-12);
            Assert.AreEqual(0, ShellPlanarity.DeviationMm(Twisted(1).Take(3).ToArray()), "three corners are always planar");
            Assert.AreEqual(1e-4, GeometryBase.Tolerance, "the absolute tolerance of the geometry kernel does not change");
        }

        [TestMethod]
        public void RoundOffBeyondTheAbsoluteToleranceIsPlanar()
        {
            var corners = Twisted(5e-4);
            // The rule until 5/11/2026: absolute 1e-4 mm on the polygon and on the topology.
            Assert.ThrowsException<ArgumentException>(() => new Polygon3d(corners, GeometryBase.Tolerance));
            Assert.IsTrue(ShellPlanarity.IsPlanar(corners));
            var (model, shell) = Connected(corners);
            var shape = shell.Shape;
            // The shape keeps the node positions themselves: no corner is moved onto a plane, so no number changes.
            Assert.IsTrue(shape.Fill.Zip(shell.Nodes, (p, n) => ReferenceEquals(p, n.Position)).All(x => x) && shape.Fill.Count == 4);
            Assert.IsFalse(model.ValidateTopology().Any(d => d.Code == "NonPlanarShell"));
            Assert.IsFalse(model.ValidateTopology(false, GeometryBase.Tolerance).Any(d => d.Code == "NonPlanarShell"));
        }

        [TestMethod]
        public void WarpingBeyondTheRelativeBoundIsRefused()
        {
            var corners = Twisted(0.5); // 0.5 mm on a 350 mm plate: 1.0e-3 of the diagonal, a real warping
            Assert.IsFalse(ShellPlanarity.IsPlanar(corners));
            var (model, shell) = Connected(corners);
            Assert.ThrowsException<ArgumentException>(() => shell.Shape);
            Assert.IsTrue(model.ValidateTopology().Any(d => d.Code == "NonPlanarShell"));
            Assert.AreEqual(0, model.ValidateTopology(true, GeometryBase.Tolerance).Count(d => d.Code == "NonPlanarShell"),
                "a solver that supports warped quads still accepts it");
        }

        [TestMethod]
        public void SmallPlatesKeepTheAbsoluteFloor()
        {
            Assert.IsTrue(ShellPlanarity.IsPlanar(Twisted(9e-5, 10)));
            Assert.IsFalse(ShellPlanarity.IsPlanar(Twisted(2e-4, 10)));
            var (model, shell) = Connected(Twisted(2e-4, 10));
            Assert.ThrowsException<ArgumentException>(() => shell.Shape);
            Assert.IsTrue(model.ValidateTopology().Any(d => d.Code == "NonPlanarShell"));
        }

        [TestMethod]
        public void ExactlyPlanarShellsAreUnchanged()
        {
            var (model, shell) = Connected(Twisted(0));
            Assert.AreEqual(0, ShellPlanarity.DeviationMm(Twisted(0)));
            Assert.AreEqual(Side * Side, shell.Shape.GetArea(), 1e-6);
            Assert.AreEqual(0, model.ValidateTopology().Count);
        }

        [TestMethod]
        public void NonUniformPressureFollowsTheShellRule()
        {
            // Until 5/11/2026 the constructor refused the fourth vertex beyond 1e-7 of the diagonal (here about 2e-3 mm against 5e-5 mm)
            // and GetGeometry used the absolute 1e-4 mm.
            var pressures = new[] { 1d, 2, 3, 4 };
            var load = new NonUniformPlatePressure(Twisted(5e-4), pressures, new LoadCaseBase("H"), CoordinateSystem.Global);
            Assert.AreEqual(4, load.GetGeometry().Fill.Count);
            Assert.AreEqual(Side * Side * pressures.Average(), load.GetTotalPressure(), 1e-9 * Side * Side * pressures.Average(),
                "the bilinear integral is the one of the flat square: the vertices are not moved");
            Assert.ThrowsException<ArgumentException>(() => new NonUniformPlatePressure(Twisted(0.5), pressures, new LoadCaseBase("H"), CoordinateSystem.Global));
            Assert.ThrowsException<ArgumentException>(() => new NonUniformPlatePressure(Twisted(2e-4, 10), pressures, new LoadCaseBase("H"), CoordinateSystem.Global),
                "a small area keeps the 1e-4 mm floor");
        }

        /// <summary>Resultant of <see cref="Equilibrium.ShellPressure"/> for a traction of 2 N/mm² along global Z on <paramref name="corners"/>, or the
        /// message of its refusal.</summary>
        private static (EquilibriumContribution Pressure, string Refusal) Pressure(Point3d[] corners)
        {
            var (_, shell) = Connected(corners);
            try { return (Equilibrium.ShellPressure(shell, new Vector3d(0, 0, 2), "H", "pressure", "test"), null); }
            catch (ArgumentException ex) { return (null, ex.Message); }
        }

        [TestMethod]
        public void UniformPressureFollowsTheShellRule_OnBothSidesOfTheBound()
        {
            // 17/11/2026: until then Equilibrium.ShellPressure refused the fourth corner beyond 1e-7 mm of the plane of the first three. A twisted
            // square is 4 times its deviation away from it: here 2e-3 mm, refused, although the plate is planar for the import (bound 4.95e-3 mm).
            double bound = ShellPlanarity.ToleranceMm(Twisted(0));
            Assert.AreEqual(1e-5 * Side * Math.Sqrt(2), bound, 1e-15);
            foreach (var deviation in new[] { 5e-4, 0.99 * bound })
            {
                var (accepted, refusal) = Pressure(Twisted(deviation));
                Assert.IsNull(refusal, "deviation " + deviation + " mm, within the bound of the shells");
                Assert.AreEqual(2 * Side * Side, accepted.Force.Z, 1e-8 * 2 * Side * Side, "the area of the square, to second order in the deviation");
                Assert.AreEqual(0, accepted.Force.X, 0); Assert.AreEqual(0, accepted.Force.Y, 0);
                // About the first corner, the centroid of the square (Side/2, Side/2) times the force.
                Assert.AreEqual(2 * Side * Side * Side / 2, accepted.Moment.X, 1e-7 * 2 * Side * Side * Side);
                Assert.AreEqual(-2 * Side * Side * Side / 2, accepted.Moment.Y, 1e-7 * 2 * Side * Side * Side);
            }
            Assert.AreEqual("InvalidPressureWindingOrPlane", Pressure(Twisted(1.01 * bound)).Refusal, "just beyond the bound of the shells");
            Assert.AreEqual("InvalidPressureWindingOrPlane", Pressure(Twisted(0.5)).Refusal, "0.5 mm on a 350 mm plate is an actual warping");
            var oneMetre = new[] { new Point3d(0, 0, 0), new Point3d(1000, 0, 0), new Point3d(1000, 1000, 0), new Point3d(0, 1000, 1) };
            Assert.AreEqual("InvalidPressureWindingOrPlane", Pressure(oneMetre).Refusal, "a corner 1 mm out of the plane of a 1 m plate is an actual warping");
            oneMetre[3] = new Point3d(0, 1000, 0.05);
            Assert.IsNull(Pressure(oneMetre).Refusal, "0.05 mm on a 1 m plate: deviation 0.0125 mm, bound 0.0141 mm");
            Assert.IsNull(Pressure(Twisted(9e-5, 10)).Refusal, "a small plate keeps the 1e-4 mm floor");
            Assert.AreEqual("InvalidPressureWindingOrPlane", Pressure(Twisted(2e-4, 10)).Refusal, "beyond the 1e-4 mm floor of a small plate");
        }

        [TestMethod]
        public void UniformPressureWithinTheFormerBoundKeepsItsResultBitForBit()
        {
            // Within 1e-7 mm the plate takes the arithmetic of before: two triangles from the first corner, each with its area along the normal
            // of the first one and its centroid. The same sums here, in the same order.
            foreach (var corners in new[] { Twisted(0), Twisted(2e-8), Twisted(0, 5) })
            {
                var c = Pressure(corners).Pressure; var traction = new Vector3d(0, 0, 2);
                Vector3d ab = corners[1] - corners[0], ac = corners[2] - corners[0]; var normal = ab.CrossProduct(ac); normal /= Axes.Length(normal);
                var force = new Vector3d(0, 0, 0); var moment = new Vector3d(0, 0, 0);
                for (int i = 1; i < 3; i++)
                {
                    Vector3d v = corners[i] - corners[0], w = corners[i + 1] - corners[0];
                    var f = traction * (Axes.Dot(v.CrossProduct(w), normal) / 2); force += f; moment += ((v + w) / 3).CrossProduct(f);
                }
                Assert.AreEqual(force.X, c.Force.X, 0); Assert.AreEqual(force.Y, c.Force.Y, 0); Assert.AreEqual(force.Z, c.Force.Z, 0);
                Assert.AreEqual(moment.X, c.Moment.X, 0); Assert.AreEqual(moment.Y, c.Moment.Y, 0); Assert.AreEqual(moment.Z, c.Moment.Z, 0);
                Assert.AreEqual(corners[0].X, c.Point.X, 0); Assert.AreEqual(corners[0].Y, c.Point.Y, 0); Assert.AreEqual(corners[0].Z, c.Point.Z, 0);
            }
        }
    }
}
