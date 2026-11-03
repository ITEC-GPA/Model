using System;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Attributes;
using GPC.Model.Core;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>Vocabulary added for the Arachne F6 migration (correzioni-P2A.md): element-level form-finding inputs, element labels
    /// and sections with declared properties and an outline.</summary>
    [TestClass]
    public class ArachneVocabularyTest
    {
        private static Model Sample(out LoadCaseBase cables)
        {
            var model = new Model(Guid.Parse("00000000-0000-0000-0000-0000000000a1"), "Arachne");
            cables = new LoadCaseBase(Guid.Parse("00000000-0000-0000-0000-0000000000a2"), "Cables"); model.LoadCases.Add(cables);
            var a = new NodeElement(Guid.Parse("00000000-0000-0000-0000-0000000000a3"), new Point3d(0, 0, 0), null, "1", 1);
            var b = new NodeElement(Guid.Parse("00000000-0000-0000-0000-0000000000a4"), new Point3d(1000, 0, -0.0), null, "2", 2);
            a.Attributes.Add(new ElementLabelAttribute("S1"));
            b.Attributes.Add(new FormFindingCoordinateAttribute(new Vector3d(0, 0, 5)));
            b.Loads.Add(new PointLoad(0, 0, -4, 0, 0, 0, b.Position, cables, CoordinateSystem.Global));
            var beam = new BeamElement(Guid.Parse("00000000-0000-0000-0000-0000000000a5"), a.Position, b.Position, null, null, "1", 1);
            beam.ConnectNodes(a, b);
            beam.Attributes.Add(new FormFindingGoalAttribute(new FormFindingGoal { Kind = FormFindingGoalKind.ForceDensity, Target = 2, Weight = 0, ProjectToPlane = true }, cables));
            beam.Attributes.Add(new FormFindingGoalAttribute(new FormFindingGoal { Kind = FormFindingGoalKind.Length, Target = 900, Weight = 3 }, cables));
            model.NodesElements.Add(a); model.NodesElements.Add(b); model.BeamElements.Add(beam);
            return model;
        }

        [TestMethod]
        public void FormFindingAttributesCopyAndPersistWithSharedCases()
        {
            var model = Sample(out var cables);
            string fingerprint = model.AnalysisFingerprint();
            void Same(Model other)
            {
                Assert.AreEqual(fingerprint, other.AnalysisFingerprint());
                var beam = other.BeamElements.Values.Single();
                var goals = beam.Attributes.Values.OfType<FormFindingGoalAttribute>().ToArray();
                Assert.AreEqual(2, goals.Length);
                Assert.AreEqual(FormFindingGoalKind.ForceDensity, goals[0].Goal.Kind); Assert.AreEqual(2, goals[0].Goal.Target); Assert.AreEqual(0, goals[0].Goal.Weight); Assert.IsTrue(goals[0].Goal.ProjectToPlane);
                Assert.AreEqual(FormFindingGoalKind.Length, goals[1].Goal.Kind); Assert.AreEqual(900, goals[1].Goal.Target);
                var b = other.NodesElements.Values.Single(n => n.Name == "2");
                // One case instance shared by the goals, the nodal load and the registry
                Assert.AreSame(other.LoadCases.Values.Single(), goals[0].LoadCase);
                Assert.AreSame(goals[0].LoadCase, goals[1].LoadCase);
                Assert.AreSame(goals[0].LoadCase, b.Loads.Values.Single().LoadCase);
                Assert.AreEqual(5, b.Attributes.Values.OfType<FormFindingCoordinateAttribute>().Single().Weights.Z);
                Assert.AreEqual(long.MinValue, BitConverter.DoubleToInt64Bits(b.Position.Z), "negative zero kept");
                Assert.AreEqual("S1", other.NodesElements.Values.Single(n => n.Name == "1").Attributes.Values.OfType<ElementLabelAttribute>().Single().Label);
            }
            var copy = ModelValues.Copy(model); Same(copy);
            Assert.AreNotSame(model.BeamElements.Values.Single().Attributes.Values.First(), copy.BeamElements.Values.Single().Attributes.Values.First());
            copy.BeamElements.Values.Single().Attributes.Values.OfType<FormFindingGoalAttribute>().First().Goal.Target = 7;
            Assert.AreEqual(2, model.BeamElements.Values.Single().Attributes.Values.OfType<FormFindingGoalAttribute>().First().Goal.Target, "detached copy");
            using (var stream = new MemoryStream())
            {
                ModelArchive.Save(model, stream); stream.Position = 0; Same(ModelArchive.Load(stream));
            }
            var beamCopy = ModelValues.CopyValue(model.BeamElements.Values.Single());
            Assert.AreEqual(2, beamCopy.Attributes.Values.OfType<FormFindingGoalAttribute>().Count());
            // The objectives enter the fingerprint
            model.BeamElements.Values.Single().Attributes.Values.OfType<FormFindingGoalAttribute>().Last().Goal.Target = 901;
            Assert.AreNotEqual(fingerprint, model.AnalysisFingerprint());
        }

        [TestMethod]
        public void GroupsWithStableIdentitiesAreRegisteredByTheAssembly()
        {
            var parent = Group.Create(Guid.Parse("00000000-0000-0000-0000-0000000000f1"), "Net");
            var child = Group.Create(Guid.Parse("00000000-0000-0000-0000-0000000000f2"), "Warp", parent);
            Assert.AreSame(parent, child.Parent); Assert.AreSame(child, parent.Childs.Single());
            var a = new NodeElement(Guid.Parse("00000000-0000-0000-0000-0000000000f3"), new Point3d(0, 0, 0));
            var b = new NodeElement(Guid.Parse("00000000-0000-0000-0000-0000000000f4"), new Point3d(1000, 0, 0));
            var beam = new BeamElement(Guid.Parse("00000000-0000-0000-0000-0000000000f5"), a.Position, b.Position, null);
            beam.ConnectNodes(a, b); beam.Assignments.Formulation = GPC.Model.Structure.Assignments.BeamFormulation.Cable; beam.AddGroup(child);
            var model = ModelAssembly.Assemble(new[] { beam }, Guid.Parse("00000000-0000-0000-0000-0000000000f6"));
            Assert.AreEqual(2, model.Groups.Count);
            Assert.AreEqual(child.Guid, model.Groups["Warp"].Guid); Assert.AreSame(model.Groups["Net"], model.Groups["Warp"].Parent);
            var copy = ModelValues.Copy(model);
            Assert.AreEqual(model.AnalysisFingerprint(), copy.AnalysisFingerprint());
            Assert.ThrowsException<ArgumentException>(() => Group.Create(Guid.Empty, "x"));
            Assert.ThrowsException<ArgumentException>(() => Group.Create(Guid.NewGuid(), " "));
        }

        [TestMethod]
        public void RefreshedIdentityMapAcceptsShellsAddedAfterTheAssembly()
        {
            var points = new[] { new Point3d(0, 0, 0), new Point3d(1000, 0, 0), new Point3d(0, 1000, 0) };
            var nodes = points.Select((p, i) => new NodeElement(Guid.Parse("00000000-0000-0000-0000-00000000010" + i), p)).ToArray();
            var beams = new[] { (0, 1), (1, 2), (2, 0) }.Select((e, i) =>
            {
                var beam = new BeamElement(Guid.Parse("00000000-0000-0000-0000-00000000011" + i), points[e.Item1], points[e.Item2], null);
                beam.ConnectNodes(nodes[e.Item1], nodes[e.Item2]); beam.Assignments.Formulation = GPC.Model.Structure.Assignments.BeamFormulation.Cable; return beam;
            }).ToArray();
            var model = ModelAssembly.Assemble(beams, Guid.Parse("00000000-0000-0000-0000-000000000120"));
            var ids = model.AllElements.ToDictionary(e => e.Guid, e => e.Id);
            var shell = new AreaElement(Guid.Parse("00000000-0000-0000-0000-000000000121"), null, null, null, "1", 1);
            shell.ConnectNodes(model.NodesElements.Values.OrderBy(n => n.Id).ToArray());
            model.AreaElements.Add(shell); ids.Add(shell.Guid, 1);
            var map = GPC.Model.Core.Identity.SolverIdentityMap.Create(model, "test", ids);
            Assert.ThrowsException<InvalidOperationException>(() => model.AssemblyMap.ToSolver(model, "test", ids), "the map of the assembly is stale");
            ModelAssembly.RefreshIdentityMap(model);
            Assert.AreEqual(7, model.AssemblyMap.ToSolver(model, "test", ids).Entries.Count);
            Assert.AreEqual(map.AnalysisFingerprint, model.AssemblyMap.ToSolver(model, "test", ids).AnalysisFingerprint);
        }

        [TestMethod]
        public void InvalidVocabularyIsRejected()
        {
            Assert.ThrowsException<ArgumentException>(() => new ElementLabelAttribute(""));
            Assert.ThrowsException<ArgumentNullException>(() => new FormFindingGoalAttribute(null, null));
            Assert.ThrowsException<ArgumentNullException>(() => new FormFindingCoordinateAttribute(null));
            Assert.ThrowsException<ArgumentException>(() => new Section(null, -1, 0, 0, 0, 0, 0, 0, new Point2d(0, 0), 0, 0, 0, 0, 0, "x"));
            Assert.ThrowsException<ArgumentException>(() => new Section(null, 1, double.NaN, 0, 0, 0, 0, 0, new Point2d(0, 0), 0, 0, 0, 0, 0, "x"));
        }

        [TestMethod]
        public void AssemblyKeepsSelfContainedAttributesAndTheRequestedNumbering()
        {
            var cases = new LoadCaseBase(Guid.Parse("00000000-0000-0000-0000-0000000000b1"), "Cables");
            var points = new[] { new Point3d(0, 0, 0), new Point3d(1000, 0, 0), new Point3d(2000, 0, 0) };
            // GUIDs in the reverse of the requested order
            var nodes = points.Select((p, i) => new NodeElement(Guid.Parse("00000000-0000-0000-0000-0000000000c" + (9 - i)), p, null, (i + 1).ToString())).ToArray();
            nodes[0].Attributes.Add(new ElementLabelAttribute("S1"));
            nodes[1].Attributes.Add(new FormFindingCoordinateAttribute(new Vector3d(0, 0, 2)));
            var beams = new[] { 0, 1 }.Select(i =>
            {
                var beam = new BeamElement(Guid.Parse("00000000-0000-0000-0000-0000000000d" + (9 - i)), points[i], points[i + 1], null, null, (i + 1).ToString());
                beam.ConnectNodes(nodes[i], nodes[i + 1]); beam.Assignments.Formulation = GPC.Model.Structure.Assignments.BeamFormulation.Cable;
                // Each beam cites its own copy of the case: the assembly keeps one
                beam.Attributes.Add(new FormFindingGoalAttribute(new FormFindingGoal { Kind = FormFindingGoalKind.ForceDensity, Target = i + 1, Weight = 0 }, ModelValues.CopyValue(cases)));
                return beam;
            }).ToArray();
            long Order(Element e) => long.Parse(e.Name);
            var model = ModelAssembly.Assemble(beams, Guid.Parse("00000000-0000-0000-0000-0000000000b2"), "", null, null, false, Order);
            CollectionAssert.AreEqual(new[] { "1", "2", "3" }, model.NodesElements.Values.OrderBy(n => n.Id).Select(n => n.Name).ToArray());
            CollectionAssert.AreEqual(new[] { "1", "2" }, model.BeamElements.Values.OrderBy(b => b.Id).Select(b => b.Name).ToArray());
            var goals = model.BeamElements.Values.SelectMany(b => b.Attributes.Values.OfType<FormFindingGoalAttribute>()).ToArray();
            Assert.AreEqual(2, goals.Length);
            Assert.AreSame(model.LoadCases.Values.Single(), goals[0].LoadCase); Assert.AreSame(goals[0].LoadCase, goals[1].LoadCase);
            Assert.AreEqual("S1", model.NodesElements.Values.Single(n => n.Name == "1").Attributes.Values.OfType<ElementLabelAttribute>().Single().Label);
            // Without a sequence: GUID order, as before
            var byGuid = ModelAssembly.Assemble(beams, Guid.Parse("00000000-0000-0000-0000-0000000000b2"));
            CollectionAssert.AreEqual(new[] { "3", "2", "1" }, byGuid.NodesElements.Values.OrderBy(n => n.Id).Select(n => n.Name).ToArray());
            // Attributes citing other elements still require an adapter
            var released = new BeamElement(Guid.Parse("00000000-0000-0000-0000-0000000000e1"), points[0], points[1], null);
            released.ConnectNodes(new NodeElement(Guid.Parse("00000000-0000-0000-0000-0000000000e2"), points[0]), new NodeElement(Guid.Parse("00000000-0000-0000-0000-0000000000e3"), points[1]));
            released.Attributes.Add(new BeamReleasesAttribute());
            Assert.ThrowsException<NotSupportedException>(() => ModelAssembly.Assemble(new[] { released }, Guid.Parse("00000000-0000-0000-0000-0000000000b3")));
        }

        [TestMethod]
        public void DeclaredSectionKeepsItsValuesAndOutline()
        {
            var outline = new Shape2d(new Polygon2d(new[] { new Point2d(0, 0), new Point2d(10, 0), new Point2d(10, 20), new Point2d(0, 20) }));
            // Values unrelated to the outline on purpose: nothing may be recomputed from it.
            var section = new Section(outline, 123.25, 0, 0, 0, 1e4, 2e4, 0, new Point2d(5, 10), 0.25, 300, 400, 0, 0, "Declared");
            void Same(Section s)
            {
                Assert.AreEqual(123.25, s.Area); Assert.AreEqual(0, s.Jxx); Assert.AreEqual(0, s.Jyy); Assert.AreEqual(1e4, s.J11); Assert.AreEqual(2e4, s.J22);
                Assert.AreEqual(0, s.Jt); Assert.AreEqual(0.25, s.AngleX1); Assert.AreEqual(300, s.Wel1); Assert.AreEqual(400, s.Wel2);
                Assert.AreEqual(5, s.Centroid.X); Assert.AreEqual(10, s.ShearCenter.Y); Assert.AreEqual("Declared", s.Name);
                Assert.IsNotNull(s.Shape);
            }
            Same(section);
            Same(ModelValues.CopyValue(section));
            using (var stream = new MemoryStream())
            {
                ModelValueArchive.Save(section, stream); stream.Position = 0; Same(ModelValueArchive.Load<Section>(stream));
            }
            var noOutline = new Section(null, 10, 1, 2, 0, 1, 2, 0, new Point2d(0, 0), 0, 1, 1, 0, 0, "Generic");
            Assert.IsNull(ModelValues.CopyValue(noOutline).Shape);
        }
    }
}
