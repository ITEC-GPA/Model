using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Restraints;
using GPC.Model.Structure.Assignments;
using GPC.Model.Structure.Members;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>The ids of the elements of an input model numbered again (<see cref="ModelValues.Renumber"/>, for BridGHe: Bridge Construction
    /// Sequence removes the inactive elements of a copy and numbers the rest 1..n in the order of the former transformer). The copy keeps the
    /// identities; every reference by id follows (nodal links, orientation nodes, parts of the physical members, physical surfaces and their
    /// zones, assembly map); the result is the model assembled with that numbering.</summary>
    [TestClass]
    public class ModelRenumberTest
    {
        private static readonly Guid Owner = Guid.Parse("c0de0000-0000-0000-0000-000000001000");
        private static readonly Regex RandomGuid = new Regex("[0-9a-f]{8}-[0-9a-f]{4}-4[0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}", RegexOptions.CultureInvariant);
        // Element GUIDs of version 0: never taken for the random GUIDs of the geometry values.
        private static Guid Id(int k) => Guid.Parse("a1000000-0000-0000-0000-" + k.ToString("D12"));

        private static string Document(Model model)
        {
            using var stream = new MemoryStream(); ModelArchive.SaveDocument(model, stream);
            return RandomGuid.Replace(Encoding.UTF8.GetString(stream.ToArray()), "v4");
        }

        /// <summary>Nine nodes with input ids 10..90 (a restrained end, a nodal link of node 6 to node 3, an orientation node 7), five beams (two
        /// with the orientation node), two shells with a common edge, a point load; in input order (index = rank).</summary>
        private static List<Element> Inputs()
        {
            var steel = new GPC.Model.Materials.Material("Steel", 210000, 0.3, 7.85e-9, 1.2e-5);
            var section = new GPC.Model.Sections.Section(1000, 2e6, 1e6, 5e5, 0, new Point2d(0, 0), new Point3d(0, 0, 0), 0, "S");
            var beamProperty = new ElasticBeamProperty("Beam", steel, section, 800, 600);
            var plateProperty = new MembranePlateProperty("Membrane") { MassPerArea = 1e-9 };
            var load = new LoadCaseBase(Id(900), "Q");
            var points = new[] { (0, 0, 0), (1000, 0, 0), (2000, 0, 0), (3000, 0, 0), (4000, 0, 0), (5000, 0, 0), (0, 500, 300), (500, -1000, 0), (1500, -1000, 0) };
            var nodes = points.Select((p, k) => new NodeElement(Id(k + 1), new Point3d(p.Item1, p.Item2, p.Item3), null, "N" + (k + 1), 10 * (k + 1))).ToArray();
            nodes[0].Assignments.Restrains.Add(new RestrainAssignment { Restrain = new NodeRestrain(nodes[0], new List<DofRestrain> {
                new DofRestrain(GeometryRestrain.DOF.DX, true), new DofRestrain(GeometryRestrain.DOF.DY, true), new DofRestrain(GeometryRestrain.DOF.DZ, true) }) });
            nodes[5].Assignments.Links.Add(new NodalLink { OtherNodeId = 30, SourceRecord = "link 6-3" });
            nodes[3].Loads.Add(new PointLoad(0, 0, -1000, 0, 0, 0, nodes[3].Position, load));
            var elements = new List<Element>(nodes);
            for (int k = 0; k < 5; k++)
            {
                var beam = new BeamElement(Id(101 + k), nodes[k].Position, nodes[k + 1].Position, beamProperty, null, "B" + (k + 1));
                beam.ConnectNodes(nodes[k], nodes[k + 1]); beam.Assignments.Formulation = BeamFormulation.StraightTwoNode;
                if (k == 1 || k == 3) beam.Assignments.OrientationNodeId = 70;
                elements.Add(beam);
            }
            foreach (var (k, a, b, c) in new[] { (1, 0, 1, 7), (2, 1, 8, 7) })
            {
                var shell = new AreaElement(Id(200 + k), null, plateProperty, null, "S" + k);
                shell.ConnectNodes(nodes[a], nodes[b], nodes[c]);
                elements.Add(shell);
            }
            return elements;
        }

        /// <summary>The physical member of the five beams and the physical surface of the two shells (zone: the first), by GUID.</summary>
        private static Model Decorate(Model model)
        {
            var beams = model.BeamElements.Values.ToDictionary(b => b.Guid); var shells = model.AreaElements.Values.ToDictionary(s => s.Guid);
            model.PhysicalMembers.Add("M", new PhysicalMemberDefinition("M", Enumerable.Range(0, 5).Select(k => new BeamMemberPart(beams[Id(101 + k)].Id, k != 2)), "NodeToNode", "Member", "test"));
            model.PhysicalSurfaces.Add("S", new PhysicalSurfaceDefinition("S", new[] { shells[Id(201)].Id, shells[Id(202)].Id },
                new[] { new SurfaceZoneDefinition("Z", new[] { shells[Id(201)].Id }) }, "Surface", "test"));
            return model;
        }

        private static Func<Element, long> Rank(IReadOnlyList<Element> inputs, bool backward)
        {
            var rank = inputs.Select((e, i) => (e.Guid, i)).ToDictionary(p => p.Guid, p => (long)(backward ? -p.i : p.i));
            return e => rank[e.Guid];
        }

        private static Model Assemble(bool backward)
        {
            var inputs = Inputs();
            return Decorate(ModelAssembly.Assemble(inputs, Owner, "Renumber", null, null, false, Rank(inputs, backward)));
        }

        private static string Ids(Model model) => string.Join(" ", model.AllElements.Select(e => Model.FamilyOf(e) + ":" + e.Id + "=" + e.Guid.ToString("D").Substring(24)));

        [TestMethod]
        public void TheRenumberedModelIsTheModelAssembledWithThatNumbering()
        {
            var forward = Assemble(false); var backward = Assemble(true);
            string before = Document(forward), beforeIds = Ids(forward);
            Assert.AreNotEqual(Ids(backward), beforeIds, "the two numberings differ");
            var renumbered = ModelValues.Renumber(forward, Rank(Inputs(), true));

            // The source is not changed; the copy keeps the identity of the model and of the elements.
            Assert.AreEqual(before, Document(forward)); Assert.AreEqual(beforeIds, Ids(forward));
            Assert.AreEqual(forward.Guid, renumbered.Guid);
            CollectionAssert.AreEquivalent(forward.AllElements.Select(e => e.Guid).ToArray(), renumbered.AllElements.Select(e => e.Guid).ToArray());

            // The numbering of the model assembled in that order, and the same model.
            Assert.AreEqual(Ids(backward), Ids(renumbered));
            Assert.AreEqual(backward.AnalysisFingerprint(), renumbered.AnalysisFingerprint());
            Assert.AreEqual(backward.SolverBindingFingerprint(), renumbered.SolverBindingFingerprint());
            string a = Document(backward), b = Document(renumbered);
            if (a != b) { string folder = Path.Combine(Path.GetTempPath(), "gpc-renumber-diff"); Directory.CreateDirectory(folder); File.WriteAllText(Path.Combine(folder, "assembled.xml"), a); File.WriteAllText(Path.Combine(folder, "renumbered.xml"), b); }
            Assert.AreEqual(a, b, "XML documents (both in %TEMP%\\gpc-renumber-diff)");
            var expected = ModelRevision.Capture(backward); var actual = ModelRevision.Capture(renumbered);
            // The document token is exact per instance (the geometry values of two assemblies have their own GUIDs): the XML documents above.
            Assert.AreEqual(expected.Analysis, actual.Analysis); Assert.AreEqual(expected.Verification, actual.Verification);
            Assert.AreNotEqual(forward.AnalysisFingerprint(), renumbered.AnalysisFingerprint(), "the numbering is an input of the analysis");
        }

        [TestMethod]
        public void EveryReferenceByIdFollowsItsElement()
        {
            var source = Assemble(false); var model = ModelValues.Renumber(source, Rank(Inputs(), true));
            NodeElement Node(Model m, int k) => m.NodesElements.Values.Single(n => n.Guid == Id(k));
            BeamElement Beam(Model m, int k) => m.BeamElements.Values.Single(b => b.Guid == Id(k));
            AreaElement Shell(Model m, int k) => m.AreaElements.Values.Single(s => s.Guid == Id(k));
            foreach (var m in new[] { source, model })
            {
                Assert.AreEqual(Node(m, 3).Id, Node(m, 6).Assignments.Links.Single().OtherNodeId, "nodal link");
                Assert.AreEqual("link 6-3", Node(m, 6).Assignments.Links.Single().SourceRecord);
                foreach (int k in new[] { 102, 104 }) Assert.AreEqual(Node(m, 7).Id, Beam(m, k).Assignments.OrientationNodeId, "orientation node");
                foreach (int k in new[] { 101, 103, 105 }) Assert.IsNull(Beam(m, k).Assignments.OrientationNodeId);
                var parts = m.PhysicalMembers["M"].Parts;
                CollectionAssert.AreEqual(Enumerable.Range(0, 5).Select(k => Beam(m, 101 + k).Id).ToArray(), parts.Select(p => p.BeamId).ToArray(), "member parts");
                CollectionAssert.AreEqual(new[] { true, true, false, true, true }, parts.Select(p => p.AlongIJ).ToArray());
                Assert.AreEqual("Member", m.PhysicalMembers["M"].Name); Assert.AreEqual("NodeToNode", m.PhysicalMembers["M"].StationDomain);
                var surface = m.PhysicalSurfaces["S"];
                CollectionAssert.AreEqual(new[] { Shell(m, 201).Id, Shell(m, 202).Id }, surface.ElementIds.ToArray(), "surface");
                CollectionAssert.AreEqual(new[] { Shell(m, 201).Id }, surface.Zones.Single().ElementIds.ToArray(), "zone");
                Assert.AreEqual("Surface", surface.Name); Assert.AreEqual("test", surface.Source);
                foreach (var entry in m.AssemblyMap.Entries) Assert.AreEqual(m.AllElements.Single(e => e.Guid == entry.CanonicalGuid).Id, entry.ModelId, "assembly map");
                m.AssemblyMap.ToSolver(m, "test", m.AllElements.Select(e => new KeyValuePair<Guid, int>(e.Guid, e.Id)));
                Assert.AreSame(Node(m, 1), m.BeamElements.Values.Single(b => b.Guid == Id(101)).NodeI, "connectivity by object");
                Assert.AreSame(Node(m, 1), Node(m, 1).Assignments.Restrains.Single().Restrain.Point, "restrained node");
                var errors = m.ValidateTopology().Concat(m.ValidateAssignments()).Where(d => d.Severity == GPC.Model.Core.Diagnostics.DiagnosticSeverity.Error).Select(d => d.Code).ToArray();
                Assert.AreEqual(0, errors.Length, string.Join(", ", errors));
            }
            // Ids 1..n per family in the order of the sequence (backward: the last input first).
            CollectionAssert.AreEqual(Enumerable.Range(1, 9).Reverse().Select(Id).ToArray(), model.NodesElements.Values.Select(n => n.Guid).ToArray());
            CollectionAssert.AreEqual(Enumerable.Range(101, 5).Reverse().Select(Id).ToArray(), model.BeamElements.Values.Select(b => b.Guid).ToArray());
            CollectionAssert.AreEqual(new[] { Id(202), Id(201) }, model.AreaElements.Values.Select(s => s.Guid).ToArray());
            CollectionAssert.AreEqual(Enumerable.Range(1, 9).ToArray(), model.NodesElements.Keys.ToArray());
            // The recorded input ids of the assembly map stay those of the inputs.
            CollectionAssert.AreEqual(source.AssemblyMap.Entries.Select(e => e.OriginalModelId).ToArray(), model.AssemblyMap.Entries.Select(e => e.OriginalModelId).ToArray());
        }

        [TestMethod]
        public void EqualKeysKeepTheCurrentOrderAndRemovedElementsCloseTheGaps()
        {
            var source = Assemble(false);
            var same = ModelValues.Renumber(source, e => 0);
            Assert.AreEqual(Ids(source), Ids(same)); Assert.AreEqual(Document(source), Document(same));

            // Elements removed from a copy: the map names another element set until it is refreshed; then the ids close the gaps.
            var copy = ModelValues.Copy(source);
            var first = copy.BeamElements.Values.Single(b => b.Guid == Id(101)); var shell = copy.AreaElements.Values.Single(s => s.Guid == Id(201));
            int node = copy.NodesElements.Values.Single(n => n.Guid == Id(1)).Id;
            copy.RemoveElement(first); copy.RemoveElement(shell); copy.RemoveNodeChecked(node);
            copy.PhysicalMembers.Clear(); copy.PhysicalSurfaces.Clear();
            var error = Assert.ThrowsException<ArgumentException>(() => ModelValues.Renumber(copy, e => 0));
            StringAssert.Contains(error.Message, "RenumberRequiresCurrentAssemblyMap");
            ModelAssembly.RefreshIdentityMap(copy);
            var closed = ModelValues.Renumber(copy, e => 0);
            CollectionAssert.AreEqual(Enumerable.Range(1, 8).ToArray(), closed.NodesElements.Keys.ToArray());
            CollectionAssert.AreEqual(Enumerable.Range(1, 4).ToArray(), closed.BeamElements.Keys.ToArray());
            CollectionAssert.AreEqual(new[] { 1 }, closed.AreaElements.Keys.ToArray());
            Assert.AreEqual(closed.NodesElements.Values.Single(n => n.Guid == Id(3)).Id, closed.NodesElements.Values.Single(n => n.Guid == Id(6)).Assignments.Links.Single().OtherNodeId);
            closed.AssemblyMap.ToSolver(closed, "test", closed.AllElements.Select(e => new KeyValuePair<Guid, int>(e.Guid, e.Id)));
        }

        [TestMethod]
        public void ModelsWithResultsSnapshotsOrDanglingReferencesAreRefused()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ModelValues.Renumber(null, e => 0));
            Assert.ThrowsException<ArgumentNullException>(() => ModelValues.Renumber(Assemble(false), null));
            void Refused(Model model, string code) => StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => ModelValues.Renumber(model, e => -e.Id)).Message, code);
            Refused(CompactArchiveTest.Grid(3, results: true), "RenumberRequiresInputModel");
            var snapshot = CompactArchiveTest.Grid(3, results: false); snapshot.CaptureAnalysis(); Refused(snapshot, "RenumberRequiresInputModel");
            var orientation = Assemble(false); orientation.BeamElements[1].Assignments.OrientationNodeId = 999; Refused(orientation, "RenumberUnknownNodeReference: 999");
            var link = Assemble(false); link.NodesElements.Values.Single(n => n.Guid == Id(6)).Assignments.Links.Single().OtherNodeId = 77; Refused(link, "RenumberUnknownNodeReference: 77");
            var member = Assemble(false); member.PhysicalMembers.Add("X", new PhysicalMemberDefinition("X", new[] { new BeamMemberPart(42, true) }, "NodeToNode")); Refused(member, "RenumberUnknownBeamReference: 42");
            var surface = Assemble(false); surface.PhysicalSurfaces.Add("Y", new PhysicalSurfaceDefinition("Y", new[] { 9 })); Refused(surface, "RenumberUnknownShellReference: 9");
            // Without an assembly map (a model built element by element) the ids are numbered all the same.
            var plain = Assemble(false);
            var unmapped = ModelValues.Renumber(WithoutMap(plain), e => -e.Id);
            Assert.IsNull(unmapped.AssemblyMap);
            CollectionAssert.AreEqual(plain.NodesElements.Values.Reverse().Select(n => n.Guid).ToArray(), unmapped.NodesElements.Values.Select(n => n.Guid).ToArray());
        }

        /// <summary>The same model built element by element (no assembly map): its elements registered in a new model one by one.</summary>
        private static Model WithoutMap(Model assembled)
        {
            var copy = ModelValues.Copy(assembled);
            var model = new Model(assembled.Guid, assembled.Name);
            foreach (var n in copy.NodesElements.Values) model.NodesElements.Add(n);
            foreach (var b in copy.BeamElements.Values) { model.BeamElements.Add(b); if (!model.BeamProperties.ContainsKey(b.BeamProperty.Name)) model.BeamProperties.Add(b.BeamProperty.Name, b.BeamProperty); }
            foreach (var s in copy.AreaElements.Values) { model.AreaElements.Add(s); if (!model.PlateProperties.ContainsKey(s.PlateProperty.Name)) model.PlateProperties.Add(s.PlateProperty.Name, s.PlateProperty); }
            foreach (var c in copy.LoadCases.Values) model.LoadCases.Add(c);
            return model;
        }
    }
}
