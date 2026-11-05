using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Models;
using GPC.Model.Structure.Topology;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// 5/11/2026: the planarity of the shells is relative to their size (ShellPlanarity). A real Straus7 model (P118-25-D-FM01, 2507 planar
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
    }
}
