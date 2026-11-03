using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model;
using GPC.Model.Core;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Identity;
using GPC.Model.Core.Units;
using GPC.Model.Materials;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using GPC.Model.Sections.Concrete;
using GPC.Model.Structure.Assignments;
using GPC.Model.Structure.Members;
using GPC.Model.Structure.Topology;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class MigrationFoundationsTest
    {
        private static NodeElement Node(double x, double y = 0, double z = 0, int id = -1) => new NodeElement(new Point3d(x, y, z), id: id);
        private static BeamElement Beam(NodeElement a, NodeElement b)
        { var result = new BeamElement(a.Position, b.Position, null); result.ConnectNodes(a, b); result.Assignments.Formulation = BeamFormulation.StraightTwoNode; return result; }
        private static AreaElement Shell(params NodeElement[] nodes)
        { var result = new AreaElement(null, null); result.ConnectNodes(nodes); return result; }

        [TestMethod]
        public void AssemblyIsOrderIndependentDetachedAndRetainsPersistentAliases()
        {
            var a = Node(0, id: 42); var b = Node(1000, id: 7); var c = Node(1000, 1000, id: 91);
            var beam = Beam(a, b); var shell = Shell(a, b, c);
            var source = new Model(); var parent = source.AddGroup("deck"); var child = source.AddGroup("span", parent);
            beam.AddGroup(child); shell.AddGroup(child);
            var owner = Guid.NewGuid();
            var first = ModelAssembly.Assemble(new Element[] { shell, beam }, owner);
            var second = ModelAssembly.Assemble(new Element[] { beam, shell }, owner);
            Assert.AreEqual(first.AnalysisFingerprint(), second.AnalysisFingerprint());
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { first.AssemblyMap }), ModelValues.Fingerprint(new object[] { second.AssemblyMap }));
            Assert.AreEqual(42, a.Id); Assert.AreNotSame(a, first.NodesElements.Values.Single(n => n.Guid == a.Guid));
            Assert.AreSame(first.NodesElements.Values.Single(n => n.Guid == a.Guid), first.BeamElements[1].NodeI);
            Assert.AreSame(first.Groups["span"], first.AreaElements[1].Groups["span"]);
            Assert.AreSame(first.Groups["deck"], first.Groups["span"].Parent);
            Assert.AreEqual(0, first.ValidateGroups().Count);
            first.NodesElements.Values.Single(n => n.Guid == a.Guid).Position.X = 37;
            Assert.AreEqual(0.0, a.Position.X); Assert.AreEqual(0.0, second.NodesElements.Values.Single(n => n.Guid == a.Guid).Position.X);
            Assert.AreEqual(0, first.PhysicalMembers.Count, "Groups do not create physical members.");
        }

        [TestMethod]
        public void CoincidentNodesStayDistinctUntilExplicitMergeAndCanBeProtected()
        {
            var a = Node(0); var b = Node(0); var c = Node(0.05); var owner = Guid.NewGuid();
            Assert.AreEqual(3, ModelAssembly.Assemble(new[] { a, b, c }, owner).NodesElements.Count);
            var exact = ModelAssembly.Assemble(new[] { c, b, a }, owner, mergeToleranceMm: 0);
            Assert.AreEqual(2, exact.NodesElements.Count); Assert.AreEqual(3, exact.AssemblyMap.Entries.Count);
            Assert.AreEqual(1, ModelAssembly.Assemble(new[] { a, b, c }, owner, mergeToleranceMm: 0.1).NodesElements.Count);
            var separate = ModelAssembly.Assemble(new[] { a, b, c }, owner, mergeToleranceMm: 0.1, keepSeparate: new[] { b.Guid });
            Assert.AreEqual(2, separate.NodesElements.Count);
            Assert.AreEqual(b.Guid, separate.AssemblyMap.Entries.Single(e => e.InputGuid == b.Guid).CanonicalGuid);
            Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new[] { a }, owner, keepSeparate: new[] { Guid.NewGuid() }));
        }

        [TestMethod]
        public void MergeUsesRepresentativeDistanceAndIsIndependentOfEnumeration()
        {
            var nodes = new[] { Node(0), Node(0.9), Node(1.8), Node(2.7) }; var owner = Guid.NewGuid();
            var a = ModelAssembly.Assemble(nodes, owner, mergeToleranceMm: 1);
            var b = ModelAssembly.Assemble(nodes.Reverse(), owner, mergeToleranceMm: 1);
            Assert.AreEqual(a.AnalysisFingerprint(), b.AnalysisFingerprint());
            foreach (var entry in a.AssemblyMap.Entries)
                Assert.IsTrue(Axes.Length(nodes.Single(n => n.Guid == entry.InputGuid).Position - a.NodesElements[entry.ModelId].Position) <= 1);
        }

        [TestMethod]
        public void MergeRejectsConflictingAssignmentsLoadsAndCollapsedConnectivity()
        {
            var a = Node(0); var b = Node(0); b.Assignments.Active = false;
            Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new[] { a, b }, Guid.NewGuid(), mergeToleranceMm: 0));
            b.Assignments.Active = null; a.Loads.Add(new PointLoad(1, 0, 0, 0, 0, 0, a.Position, new LoadCaseBase("G")));
            Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new[] { a, b }, Guid.NewGuid(), mergeToleranceMm: 0));
            a.Loads.Clear(); b.Position.X = 0.01;
            Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new[] { Beam(a, b) }, Guid.NewGuid(), mergeToleranceMm: 0.1));
        }

        [TestMethod]
        public void ConflictingCopiesOfOneGuidAreRejectedAndEqualCopiesAreDeduplicated()
        {
            var a = Node(7); var b = ModelValues.CopyValue(a); var owner = Guid.NewGuid();
            Assert.AreEqual(1, ModelAssembly.Assemble(new[] { a, b }, owner).NodesElements.Count);
            b.Position.X = 8;
            Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new[] { a, b }, owner));
            Assert.AreEqual(7.0, a.Position.X);
        }

        [TestMethod]
        public void IntegerReferencesAreRemappedOrRejectedWhenAmbiguous()
        {
            var a = Node(0, id: 400); var b = Node(100, id: 900); var c = Node(0, 100, id: 100);
            a.Assignments.Links.Add(new NodalLink { OtherNodeId = 900 });
            var beam = Beam(a, b); beam.Assignments.OrientationNodeId = 100;
            var model = ModelAssembly.Assemble(new Element[] { beam, c }, Guid.NewGuid());
            var lookup = model.NodesElements.Values.ToDictionary(n => n.Guid);
            Assert.AreEqual(lookup[b.Guid].Id, lookup[a.Guid].Assignments.Links[0].OtherNodeId);
            Assert.AreEqual(lookup[c.Guid].Id, model.BeamElements[1].Assignments.OrientationNodeId);
            Assert.AreEqual(900, a.Assignments.Links[0].OtherNodeId);
            var duplicate = Node(70, 50, id: 100);
            Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new Element[] { beam, c, duplicate }, Guid.NewGuid()));
        }

        [TestMethod]
        public void SharedCasesAndPropertiesAreCanonicalizedWhileConflictsFail()
        {
            var a = Node(0); var b = Node(100); var c = Node(0, 100); var d = Node(100, 100);
            var p = new ConcretePlateProperty(new ConcreteMaterialEN1992("C25/30"), 100, 100, "plate");
            var first = Shell(a, b, c); first.PlateProperty = p;
            var second = Shell(b, d, c); second.PlateProperty = ModelValues.CopyValue(p);
            // Review B6: homonymous cases merge only as copies of one entity (same GUID); two GUIDs are two cases.
            var g = new LoadCaseBase("G");
            a.Loads.Add(new PointLoad(1, 0, 0, 0, 0, 0, a.Position, g));
            b.Loads.Add(new PointLoad(2, 0, 0, 0, 0, 0, b.Position, ModelValues.CopyValue(g)));
            var model = ModelAssembly.Assemble(new[] { first, second }, Guid.NewGuid());
            var values = model.AreaElements.Values.ToArray(); Assert.AreSame(values[0].PlateProperty, values[1].PlateProperty);
            foreach (var load in model.AllElements.SelectMany(e => e.Loads.Values)) Assert.AreSame(model.LoadCases["G"], load.LoadCase);
            b.Loads.Values.Single().LoadCase = new LoadCaseBase("G");
            Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new[] { first, second }, Guid.NewGuid()));
            b.Loads.Values.Single().LoadCase = ModelValues.CopyValue(g);
            second.PlateProperty = new ConcretePlateProperty(new ConcreteMaterialEN1992("C25/30"), 150, 150, "plate");
            Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new[] { first, second }, Guid.NewGuid()));
        }

        [TestMethod]
        public void IdentityMapsSurviveArchiveAndSolverIdsMustBeCompleteUniqueAndCurrent()
        {
            var a = Node(0); var b = Node(0); var model = ModelAssembly.Assemble(new[] { a, b }, Guid.NewGuid(), mergeToleranceMm: 0);
            Model reopened;
            using (var stream = new MemoryStream()) { ModelArchive.Save(model, stream); stream.Position = 0; reopened = ModelArchive.Load(stream); }
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { model.AssemblyMap }), ModelValues.Fingerprint(new object[] { reopened.AssemblyMap }));
            var entity = reopened.NodesElements[1];
            var ids = new[] { new KeyValuePair<Guid, int>(entity.Guid, 700) };
            var map = reopened.AssemblyMap.ToSolver(reopened, "Straus7", ids);
            Assert.AreEqual(700, map.Entries[0].SolverId); Assert.AreEqual(reopened.AnalysisFingerprint(), map.AnalysisFingerprint);
            var detached = ModelValues.CopyValue(map); detached.Validate();
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { map }), ModelValues.Fingerprint(new object[] { detached }));
            Assert.ThrowsException<ArgumentException>(() => reopened.AssemblyMap.ToSolver(reopened, "Straus7", Array.Empty<KeyValuePair<Guid, int>>()));
            reopened.NodesElements.Add(Node(1));
            Assert.ThrowsException<InvalidOperationException>(() => reopened.AssemblyMap.ToSolver(reopened, "Straus7", ids));
        }

        [TestMethod]
        public void PlanarShellAreaAxesAndWindingHaveAnalyticValues()
        {
            var rectangle = new[] { new Point3d(0, 0, 0), new Point3d(3, 0, 0), new Point3d(3, 4, 0), new Point3d(0, 4, 0) };
            var q = new ShellGeometry(rectangle); var triangle = new ShellGeometry(rectangle.Take(3));
            Assert.AreEqual(12, q.Area, 1e-12); Assert.AreEqual(6, triangle.Area, 1e-12);
            Assert.AreEqual(0, q.PlanarityDeviation); Assert.AreEqual(1.5, q.PointAt(0, 0).X, 1e-12);
            Assert.AreEqual(1, q.NormalAt(0, 0).Z, 1e-12); Axes.Validate(q.AxesAt(0.3, -0.2));
            Assert.AreEqual(-1, new ShellGeometry(rectangle.Reverse()).NormalAt(0, 0).Z, 1e-12);
            rectangle[1].X = 100; Assert.AreEqual(12, q.Area, 1e-12);
        }

        [TestMethod]
        public void WarpedShellIntegralMatchesIndependentRefinedTriangulation()
        {
            var p = new[] { new Point3d(-1, -1, 1), new Point3d(1, -1, -1), new Point3d(1, 1, 1), new Point3d(-1, 1, -1) };
            var geometry = new ShellGeometry(p); Assert.IsTrue(geometry.RelativeWarpage > 0.1);
            double Tri(Point3d a, Point3d b, Point3d c) { Vector3d ab = b - a, ac = c - a; return Axes.Length(ab.CrossProduct(ac)) / 2; }
            Point3d Surface(double x, double y) => new Point3d(x, y, x * y);
            double reference = 0; const int n = 200; double step = 2.0 / n;
            for (int i = 0; i < n; i++) for (int j = 0; j < n; j++)
            {
                double x = -1 + i * step, y = -1 + j * step;
                var a = Surface(x, y); var b = Surface(x + step, y); var c = Surface(x + step, y + step); var d = Surface(x, y + step);
                reference += Tri(a, b, c) + Tri(a, c, d);
            }
            Assert.AreEqual(reference, geometry.Area, reference * 2e-5);
            Assert.AreEqual(geometry.Area, new ShellGeometry(p.Reverse()).Area, 1e-10);
            var model = new Model(); foreach (var point in p) model.NodesElements.Add(new NodeElement(point));
            model.AreaElements.Add(Shell(model.NodesElements.Values.ToArray()));
            Assert.IsTrue(model.ValidateTopology().Any(d => d.Code == "NonPlanarShell"));
            Assert.AreEqual(0, model.ValidateTopology(true, 1e-4).Count);
        }

        [TestMethod]
        public void ShellGeometryRejectsCollapsedFoldedNonFiniteAndOutOfDomainInputs()
        {
            Assert.ThrowsException<ArgumentException>(() => new ShellGeometry(new[] { new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(2, 0, 0) }));
            Assert.ThrowsException<ArgumentException>(() => new ShellGeometry(new[] { new Point3d(0, 0, 0), new Point3d(1, 1, 0), new Point3d(0, 1, 0), new Point3d(1, 0, 0) }));
            Assert.ThrowsException<ArgumentException>(() => new ShellGeometry(new[] { new Point3d(double.NaN, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0) }));
            var triangle = new ShellGeometry(new[] { new Point3d(0, 0, 0), new Point3d(1, 0, 0), new Point3d(0, 1, 0) });
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => triangle.PointAt(0.7, 0.7));
        }

        [TestMethod]
        public void NonStraightPhysicalPathKeepsCornerSectionsAndReversedBeamStations()
        {
            var model = new Model(); var a = Node(0); var b = Node(3000); var c = Node(3000, 4000);
            foreach (var node in new[] { a, b, c }) model.NodesElements.Add(node);
            var first = Beam(a, b); var second = Beam(c, b); model.BeamElements.Add(first); model.BeamElements.Add(second);
            var definition = new PhysicalMemberDefinition("girder", new[] { new BeamMemberPart(first.Id, true), new BeamMemberPart(second.Id, false) }, "NodeToNode");
            var path = new PhysicalMemberPath(model, definition);
            Assert.AreEqual(7000, path.Length, 1e-9); Assert.AreEqual(2, path.Locate(3000).Count);
            var right = path.Locate(3000, SectionSide.Right).Single(); Assert.AreEqual(second.Id, right.BeamId); Assert.AreEqual(1.0, right.Station); Assert.AreEqual(SectionSide.Left, right.Side);
            Assert.AreEqual(0.5, path.Locate(5000).Single().Station, 1e-12); Assert.AreEqual(2000, path.PointsAt(5000).Single().Y, 1e-9);
            c.Position.Y = 9000; Assert.AreEqual(7000, path.Length, 1e-9); Assert.AreEqual(2000, path.PointsAt(5000).Single().Y, 1e-9);
            first.Assignments.SectionAxes = Axes.Beam(a.Position, b.Position, new Vector3d(0, 1, 0));
            second.Assignments.SectionAxes = Axes.Beam(c.Position, b.Position, new Vector3d(1, 0, 0));
            Assert.ThrowsException<NotSupportedException>(() => new PhysicalMemberGeometry(model, definition));
        }

        [TestMethod]
        public void UnitConversionsDistinguishTemperatureDifferenceGradientMassAndWeight()
        {
            var si = new DomainUnits(GPC.Model.Units.SI); var knm = new DomainUnits(GPC.Model.Units.Knm); var ips = new DomainUnits(GPC.Model.Units.IPS);
            Assert.AreEqual(9810, si.Acceleration(9.81), 1e-10); Assert.AreEqual(2e-9, si.MassPerArea(2), 1e-20);
            Assert.AreEqual(1, knm.ForceDensity(1), 1e-12); Assert.AreEqual(100, knm.MembraneStiffness(100), 1e-12);
            Assert.AreEqual(1e6, knm.Moment(1), 1e-8); Assert.AreEqual(0.001, knm.ForcePerArea(1), 1e-12);
            Assert.AreEqual(20, ips.AbsoluteTemperature(68), 1e-10); Assert.AreEqual(10, ips.TemperatureDifference(18), 1e-10);
            Assert.AreEqual(10.0 / 25.4, ips.TemperatureGradient(18), 1e-12);
            Assert.AreEqual(1.2e-5, ips.ThermalExpansion(1.2e-5 / 1.8), 1e-15);
            var spring = knm.Spring(Enumerable.Repeat(1.0, 36).ToArray());
            Assert.AreEqual(1, spring[0], 1e-12); Assert.AreEqual(1000, spring[3], 1e-12); Assert.AreEqual(1000, spring[18], 1e-12); Assert.AreEqual(1e6, spring[21], 1e-8);
            Assert.ThrowsException<ArgumentException>(() => si.Spring(new double[6]));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new DomainUnits(GPC.Model.Units.SI, 0));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => si.Force(double.PositiveInfinity));
        }

        [TestMethod]
        public void BridgeMetadataIsExplicitDetachedPersistedAndFingerprintSensitive()
        {
            var direction = new Vector3d(0, 0, 1);
            var member = new PhysicalMemberDefinition("girder-1", new[] { new BeamMemberPart(1, true) }, "NodeToNode");
            var baseline = ModelValues.Fingerprint(new object[] { member });
            var metadata = new BridgeMemberMetadata(member.Id, BridgeMemberFamily.Girder, 2, 3, 1, 12000, 18000, direction, "alignment-1");
            var tagged = member.WithBridgeMetadata(metadata); direction.Z = 8; metadata.SectionUp.Z = 9;
            Assert.AreEqual(1.0, metadata.SectionUp.Z); Assert.IsNull(member.BridgeMetadata);
            Assert.AreNotEqual(baseline, ModelValues.Fingerprint(new object[] { tagged }));
            Assert.AreEqual(baseline, ModelValues.Fingerprint(new object[] { tagged.WithBridgeMetadata(null) }));
            var model = new Model(); model.PhysicalMembers.Add(tagged.Id, tagged);
            using (var stream = new MemoryStream())
            {
                ModelArchive.Save(model, stream); stream.Position = 0; var loaded = ModelArchive.Load(stream).PhysicalMembers[tagged.Id];
                Assert.AreEqual(ModelValues.Fingerprint(new object[] { tagged }), ModelValues.Fingerprint(new object[] { loaded }));
                Assert.AreEqual(18000.0, loaded.BridgeMetadata.StationToMm);
            }
            Assert.ThrowsException<ArgumentException>(() => new PhysicalMemberDefinition("other", new[] { new BeamMemberPart(1, true) }, "NodeToNode").WithBridgeMetadata(metadata));
        }

        [TestMethod]
        public void RestraintReferencesAndSharedIntegerLinksAreRemappedOnce()
        {
            var a = Node(0, id: 101); var b = Node(1, id: 500); var c = Node(2, id: 999);
            a.Assignments.Restrains.Add(new RestrainAssignment { Restrain = GPC.Model.Restraints.NodeRestrain.GetAllFixed(a, CoordinateSystem.Global), Case = new LoadCaseBase("C") });
            var link = new NodalLink { OtherNodeId = 999 }; a.Assignments.Links.Add(link); b.Assignments.Links.Add(link);
            var model = ModelAssembly.Assemble(new[] { a, b, c }, Guid.NewGuid());
            var nodes = model.NodesElements.Values.ToDictionary(n => n.Guid);
            Assert.AreSame(nodes[a.Guid], nodes[a.Guid].Assignments.Restrains[0].Restrain.Point);
            Assert.AreSame(model.LoadCases["C"], nodes[a.Guid].Assignments.Restrains[0].Case);
            Assert.AreSame(nodes[a.Guid].Assignments.Links[0], nodes[b.Guid].Assignments.Links[0]);
            Assert.AreEqual(nodes[c.Guid].Id, nodes[a.Guid].Assignments.Links[0].OtherNodeId); Assert.AreEqual(999, link.OtherNodeId);
        }

        [TestMethod]
        public void ExactMergeDoesNotUnderflowDistanceAndSolverIdsCannotCollide()
        {
            var model = ModelAssembly.Assemble(new[] { Node(0), Node(1e-200) }, Guid.NewGuid(), mergeToleranceMm: 0);
            Assert.AreEqual(2, model.NodesElements.Count);
            var ids = model.NodesElements.Values.Select(n => new KeyValuePair<Guid, int>(n.Guid, 1));
            Assert.ThrowsException<System.Runtime.Serialization.SerializationException>(() => model.AssemblyMap.ToSolver(model, "solver", ids));
        }
    }
}
