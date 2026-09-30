using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Collections;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Results;
using GPC.Model.Results.ElementResults;
using GPC.Model.Results.ResultLocations;
using GPC.Model.Restrains;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.Serialization;

namespace UnitTest
{
    [TestClass]
    public class PostProcessingTest
    {
        public static Model Mixed()
        {
            var m = new Model("Synthetic mixed topology");
            m.NodesElements.Add(new NodeElement(new Point3d(0, 0, 0), id: 10));
            m.NodesElements.Add(new NodeElement(new Point3d(0, 0, 2000), id: 70));
            m.NodesElements.Add(new NodeElement(new Point3d(1000, 0, 0), id: 150));
            m.NodesElements.Add(new NodeElement(new Point3d(0, 0, 0), id: 700));
            m.BeamElements.Add(new BeamElement(null, null, null, id: 10));
            m.ConnectBeam(10, 10, 70);
            m.BeamElements[10].Assignments.Formulation = BeamFormulation.StraightTwoNode;
            m.AreaElements.Add(new AreaElement(null, null, id: 10));
            m.ConnectShell(10, 10, 70, 150);
            return m;
        }
        [TestMethod]
        public void SharedTopologyAndCoincidentIdentity()
        {
            var m = Mixed(); Assert.AreEqual(0, m.ValidateTopology().Count);
            Assert.AreNotEqual(m.NodesElements[10], m.NodesElements[700]);
            Assert.AreEqual(2, m.GetAdjacency()[10].Count); Assert.AreEqual(0, m.GetAdjacency()[700].Count);
            m.NodesElements[10].Position = new Point3d(3, 4, 5);
            Assert.AreSame(m.NodesElements[10].Position, m.BeamElements[10].StartPoint);
            Assert.AreSame(m.NodesElements[10].Position, m.AreaElements[10].Points[0]);
            Assert.ThrowsException<InvalidOperationException>(() => m.RemoveNodeChecked(10));
            Assert.IsTrue(m.NodesElements.ContainsKey(10));
        }
        [TestMethod]
        public void IdentityDoesNotChangeWhenPointMoves()
        {
            var n = new NodeElement(Point3d.Origin); int hash = n.GetHashCode();
            n.Position = new Point3d(1, 2, 3); Assert.AreEqual(hash, n.GetHashCode());
            Assert.IsTrue(n.Equals((object)n)); Assert.IsTrue(((IEquatable<NodeElement>)n).Equals(n));
            Assert.IsFalse(n.Equals(new NodeElement(new Point3d(1, 2, 3))));
        }
        [TestMethod]
        public void SortedCollectionClearAndSparseIds()
        {
            var c = new SortedCollection<NodeElement>(); c.Add(80, new NodeElement(Point3d.Origin, id: 80));
            Assert.AreEqual(81, c.Add(new NodeElement(Point3d.Origin)));
            c.Clear(); Assert.AreEqual(0, c.Count); Assert.AreEqual(1, c.Add(new NodeElement(Point3d.Origin)));
        }
        [TestMethod]
        public void XmlRoundTripPreservesSharedGraph()
        {
            var m = Mixed(); var b = m.BeamElements[10]; b.RotationAroundFirstAxis = .31;
            var release = new BeamReleasesAttribute(); release.I[5] = new BeamDofConnection(BeamConnectionKind.Released); b.Attributes.Add(release);
            m.NodesElements[10].Assignments.Restrains.Add(new RestrainAssignment { Restrain = NodeRestrain.GetAllFixed(m.NodesElements[10], CoordinateSystem.Global) });
            double[] k = new double[36]; k[0] = 10; k[7] = 20; k[1] = k[6] = 3;
            m.NodesElements[10].Assignments.GroundSprings.Add(new SpringMatrix(k, CoordinateSystem.Global));
            var lc = new LoadCaseBase("LC+"); m.LoadCases.Add(lc);
            b.AddResult(new BeamResult(new[] { new StationResultBeamForces(lc, new ResultBeamForces(1, 2, 3, 4, 5, 6, CoordinateSystem.Global), .5) { Side = SectionSide.Left } }));
            using var s = new MemoryStream(); ModelArchive.Save(m, s); s.Position = 0; var copy = ModelArchive.Load(s);
            Assert.AreEqual(4, copy.NodesElements.Count); Assert.AreEqual(1, copy.BeamElements.Count);
            Assert.AreSame(copy.NodesElements[10], copy.BeamElements[10].NodeI);
            Assert.AreSame(copy.NodesElements[10], copy.AreaElements[10].Nodes[0]);
            Assert.AreNotSame(copy.NodesElements[10], copy.NodesElements[700]);
            Assert.AreEqual(.31, copy.BeamElements[10].RotationAroundFirstAxis, 1e-14);
            Assert.AreEqual(3, copy.NodesElements[10].Assignments.GroundSprings[0][0, 1], 1e-14);
            Assert.AreSame(copy.NodesElements[10], copy.NodesElements[10].Assignments.Restrains[0].Restrain.Point);
            Assert.AreEqual(SectionSide.Left, ((StationResultBeamForces)copy.BeamElements[10].Results[0].Results[0]).Side);
        }
        [TestMethod]
        public void InvalidStationRejectedByConstructorSetterAndReader()
        {
            foreach (var invalid in new[] { -0.1, 1.1, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            {
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => new StationResultBeamForces(null, null, invalid));
                var result = new StationResultBeamForces(null, null, .5);
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => result.ParametricDistance = invalid);
                var info = new SerializationInfo(typeof(StationResultBeamForces), new FormatterConverter());
                result.GetObjectData(info, new StreamingContext());
                var bad = new SerializationInfo(typeof(StationResultBeamForces), new FormatterConverter());
                foreach (SerializationEntry e in info) bad.AddValue(e.Name, e.Name == "ParametricDistance" ? invalid : e.Value);
                Assert.ThrowsException<ArgumentOutOfRangeException>(() => new StationReader(bad));
            }
        }
        private sealed class StationReader : StationResultBeamForces { public StationReader(SerializationInfo i) : base(i, new StreamingContext()) { } }
        [TestMethod]
        public void UnitsKeepBeamShellAndStripDistinct()
        {
            var u = new ResultUnits(1000, 1000, 1000000);
            Assert.AreEqual(80000000, u.BeamMoment(80), 1e-6);
            Assert.AreEqual(80000, u.ShellMoment(80, 1000), 1e-8);
            Assert.AreEqual(150, u.ForcePerLength(150, 1000), 1e-10);
            Assert.AreEqual(150000, u.ForcePerLength(150, 1000) * 1000, 1e-7);
        }
        [TestMethod]
        public void EccentricityAndVerticalAxesHaveAnalyticalSigns()
        {
            var a = new CoordinateSystem(new Point3d(100, 0, 0), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));
            var f = new ResultBeamForces(0, 0, 1000, 0, 0, 0, a).ToCoordinateSystemWithEccentricity(CoordinateSystem.Global);
            Assert.AreEqual(100000, f.T, 1e-8);
            var axes = Axes.Beam(Point3d.Origin, new Point3d(0, 0, 2000), new Vector3d(1, 0, 0), Math.PI / 2);
            Assert.AreEqual(1, axes.V3.Z, 1e-12); Assert.AreEqual(1, axes.V1.Y, 1e-12);
            var forces = new ResultBeamForces(100, 10, 20, 40, 50, 60, axes).ToGlobalCoordinateSystem();
            Assert.AreEqual(-20, forces.V1, 1e-10); Assert.AreEqual(10, forces.V2, 1e-10); Assert.AreEqual(100, forces.N, 1e-10);
        }
        [TestMethod]
        public void ShellRotationAnalyticalAt90And30Degrees()
        {
            var f = new ResultPlateForces(CoordinateSystem.Global, 10, 30, 4, 5, 7, 40, 70, 8);
            foreach (double angle in new[] { Math.PI / 2, Math.PI / 6 })
            {
                double c = Math.Cos(angle), s = Math.Sin(angle);
                var axes = new CoordinateSystem(Point3d.Origin, new Vector3d(c, s, 0), new Vector3d(-s, c, 0));
                var r = f.ToCoordinateSystem(axes);
                if (angle == Math.PI / 2) { Assert.AreEqual(30, r.Fxx, 1e-10); Assert.AreEqual(-4, r.Fxy, 1e-10); Assert.AreEqual(7, r.Fxz, 1e-10); Assert.AreEqual(-5, r.Fyz, 1e-10); Assert.AreEqual(70, r.Mxx, 1e-10); Assert.AreEqual(-8, r.Mxy, 1e-10); }
                else { Assert.AreEqual(18.4641016151378, r.Fxx, 1e-10); Assert.AreEqual(10.6602540378444, r.Fxy, 1e-10); Assert.AreEqual(54.4282032302755, r.Mxx, 1e-10); }
            }
            Assert.ThrowsException<NotSupportedException>(() => f.ToCoordinateSystem(new CoordinateSystem(Point3d.Origin, new Vector3d(1, 0, 0), new Vector3d(0, -1, 0))));
        }
        [TestMethod]
        public void CoupledSpringRotationUsesOperatorTransformation()
        {
            var k = new double[36]; k[0] = 10; k[7] = 20; k[1] = k[6] = 3; k[35] = 40;
            var spring = new SpringMatrix(k, CoordinateSystem.Global);
            var rotated = spring.ToCoordinateSystem(new CoordinateSystem(Point3d.Origin, new Vector3d(0, 1, 0), new Vector3d(-1, 0, 0)));
            Assert.AreEqual(20, rotated[0, 0], 1e-12); Assert.AreEqual(-3, rotated[0, 1], 1e-12);
            CollectionAssert.AreEqual(new[] { 43.0, -16.0, 0, 0, 0, 120.0 }, rotated.Apply(new[] { 2.0, -1.0, 0, 0, 0, 3.0 }));
            var units = new ResultUnits(1000, 1000, 1e6); k[3] = 2; k[18] = 2;
            Assert.AreEqual(2000, units.Spring(k)[3], 1e-10); Assert.AreEqual(2000, units.Spring(k)[18], 1e-10);
        }
        [TestMethod]
        public void LegacyMutableCoordinateInvalidatesAnalysis()
        {
            var m = Mixed(); var old = m.AnalysisFingerprint(); m.NodesElements[10].Position.X = 12;
            Assert.AreNotEqual(old, m.AnalysisFingerprint());
        }
    }
}
