using System;
using System.IO;
using System.Linq;
using System.Text;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Core;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Structure.Assignments;
using GPC.Model.Structure.Bridges;
using GPC.Model.Structure.Members;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>The descriptive bridge attributes of the elements (lane L11, correzioni-P2B.md §1bis): member, super element, section state of a
    /// beam and rigid connection of a node. Same contracts in the XML and compact archives and in the value copies, out of the analysis
    /// fingerprint, kept by the assembly, checked by the domain preflight.</summary>
    [TestClass]
    public class BridgeElementAttributesTest
    {
        private static Guid Id(int n) => Guid.Parse("00000000-0000-0000-0000-0000000011" + n.ToString("00"));
        private static BridgeMemberMetadata Metadata() => new BridgeMemberMetadata("G1", BridgeMemberFamily.Girder, 1, 2, 1, 12000.25, 18000.5, new Vector3d(0, 0.1, 1), "G1-C2");

        /// <summary>Two nodes, a beam and a triangle, without attributes.</summary>
        private static Model Bare()
        {
            var model = new Model(Id(1), "Bridge");
            var a = new NodeElement(Id(2), new Point3d(0, 0, 0), null, "1", 1);
            var b = new NodeElement(Id(3), new Point3d(6000, 0, 0), null, "2", 2);
            var c = new NodeElement(Id(4), new Point3d(0, 3000, -0.0), null, "3", 3);
            var beam = new BeamElement(Id(5), a.Position, b.Position, null, null, "1", 1);
            beam.ConnectNodes(a, b); beam.Assignments.Formulation = BeamFormulation.StraightTwoNode;
            var shell = new AreaElement(Id(6), null, null, null, "1", 1);
            shell.ConnectNodes(a, b, c);
            model.NodesElements.Add(a); model.NodesElements.Add(b); model.NodesElements.Add(c); model.BeamElements.Add(beam); model.AreaElements.Add(shell);
            return model;
        }

        /// <summary>Every attribute with every field, where it belongs: the beam has a release before them (its id does not change).</summary>
        private static Model Sample()
        {
            var model = Bare(); Decorate(model); return model;
        }

        private static void Decorate(Model model)
        {
            var beam = model.BeamElements.Values.Single();
            beam.Attributes.Add(new BridgeMemberAttribute(Metadata()));
            beam.Attributes.Add(new SuperElementAttribute("G1"));
            beam.Attributes.Add(new BridgeSectionStateAttribute(BridgeSectionBehavior.LongTerm, true, 18.375, "M01_NINF", "A1B2C3"));
            var node = model.NodesElements.Values.Single(n => n.Name == "1");
            node.Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, -1250.5)));
            node.Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0.125, -2500)));
        }

        private static void SameAttributes(Model model)
        {
            var beam = model.BeamElements.Values.Single();
            var member = beam.Attributes.Values.OfType<BridgeMemberAttribute>().Single().Metadata;
            Assert.AreEqual("G1", member.PhysicalId); Assert.AreEqual(BridgeMemberFamily.Girder, member.Family); Assert.AreEqual(1, member.Group);
            Assert.AreEqual(2, member.Segment); Assert.AreEqual(1, member.Span); Assert.AreEqual(12000.25, member.StationFromMm); Assert.AreEqual(18000.5, member.StationToMm);
            Assert.AreEqual(0.1, member.SectionUp.Y); Assert.AreEqual(1, member.SectionUp.Z); Assert.AreEqual("G1-C2", member.GeometryId);
            Assert.AreEqual("G1", beam.Attributes.Values.OfType<SuperElementAttribute>().Single().Name);
            var state = beam.Attributes.Values.OfType<BridgeSectionStateAttribute>().Single();
            Assert.AreEqual(BridgeSectionBehavior.LongTerm, state.Behavior); Assert.IsTrue(state.ConcreteExcluded);
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(18.375), BitConverter.DoubleToInt64Bits(state.ModularRatio));
            Assert.AreEqual("M01_NINF", state.ClassificationCase); Assert.AreEqual("A1B2C3", state.PropertyFingerprint);
            var links = model.NodesElements.Values.Single(n => n.Name == "1").Attributes.Values.OfType<RigidConnectionAttribute>().Select(l => l.TargetMm).ToArray();
            Assert.AreEqual(2, links.Length);
            Assert.AreEqual(-1250.5, links[0].Z); Assert.AreEqual(0.125, links[1].Y); Assert.AreEqual(-2500, links[1].Z);
        }

        private static byte[] Save(Model model, ModelArchiveFormat format) { using var s = new MemoryStream(); ModelArchive.Save(model, s, format); return s.ToArray(); }
        private static Model Load(byte[] bytes) { using var s = new MemoryStream(bytes, false); return ModelArchive.Load(s); }
        private static string Fingerprint(object value) => ModelValues.Fingerprint(new[] { value });

        [TestMethod]
        public void AttributesRoundTripInTheXmlAndCompactArchives()
        {
            var model = Sample(); SameAttributes(model);
            // An element enters a fingerprint by its identity: the content compared is that of its attributes.
            string analysis = model.AnalysisFingerprint(), content = Fingerprint(model.BeamElements.Values.Single().Attributes),
                node = Fingerprint(model.NodesElements.Values.Single(n => n.Name == "1").Attributes);
            var xml = Save(model, ModelArchiveFormat.Xml); var compact = Save(model, ModelArchiveFormat.Compact);
            Assert.AreEqual("GPCMOD", Encoding.ASCII.GetString(compact, 0, 6), "the compact codec writes the attributes");
            var fromXml = Load(xml); var fromCompact = Load(compact);
            foreach (var other in new[] { fromXml, fromCompact, ModelValues.Copy(model) })
            {
                SameAttributes(other);
                Assert.AreEqual(analysis, other.AnalysisFingerprint());
                Assert.AreEqual(content, Fingerprint(other.BeamElements.Values.Single().Attributes));
                Assert.AreEqual(node, Fingerprint(other.NodesElements.Values.Single(n => n.Name == "1").Attributes));
            }
            // The same document from both forms, and again from the compact form read back.
            string document = CompactArchiveTest.Document(fromXml);
            Assert.AreEqual(document, CompactArchiveTest.Document(fromCompact));
            Assert.AreEqual(document, CompactArchiveTest.Document(Load(Save(fromCompact, ModelArchiveFormat.Compact))));
            Assert.AreEqual(document, CompactArchiveTest.Document(model));

            // Standalone values: every attribute and the elements that carry them.
            var beam = model.BeamElements.Values.Single();
            var values = beam.Attributes.Values.Cast<object>().Concat(model.NodesElements.Values.Single(n => n.Name == "1").Attributes.Values)
                .Concat(new object[] { beam, model.NodesElements.Values.Single(n => n.Name == "1") }).ToArray();
            Assert.AreEqual(7, values.Length);
            foreach (var value in values)
            {
                var method = typeof(BridgeElementAttributesTest).GetMethod(nameof(Value), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(value.GetType());
                method.Invoke(null, new[] { value });
            }
        }

        private static string Content(object value) => Fingerprint(value is Element element ? element.Attributes : value);

        private static void Value<T>(T value) where T : class
        {
            string expected = Content(value);
            byte[] xml, compact;
            using (var s = new MemoryStream()) { ModelValueArchive.Save(value, s); xml = s.ToArray(); }
            using (var s = new MemoryStream()) { ModelValueArchive.Save(value, s, ModelArchiveFormat.Compact); compact = s.ToArray(); }
            Assert.AreEqual("GPCVAL", Encoding.ASCII.GetString(compact, 0, 6), typeof(T).Name);
            T a, b;
            using (var s = new MemoryStream(xml)) a = ModelValueArchive.Load<T>(s);
            using (var s = new MemoryStream(compact)) b = ModelValueArchive.Load<T>(s);
            Assert.AreEqual(expected, Content(a), typeof(T).Name);
            Assert.AreEqual(expected, Content(b), typeof(T).Name);
            var copy = ModelValues.CopyValue(value);
            Assert.AreNotSame(value, copy); Assert.AreEqual(expected, Content(copy), typeof(T).Name);
            // The identity of an attribute (GUID, name, id) is that of the source.
            if (value is GPC.Model.ModelObjectId source)
                foreach (var other in new[] { (GPC.Model.ModelObjectId)(object)a, (GPC.Model.ModelObjectId)(object)b, (GPC.Model.ModelObjectId)(object)copy })
                { Assert.AreEqual(source.Guid, other.Guid); Assert.AreEqual(source.Name, other.Name); Assert.AreEqual(source.Id, other.Id); }
        }

        [TestMethod]
        public void TheAnalysisFingerprintIgnoresTheBridgeAttributes()
        {
            var model = Bare();
            var released = model.BeamElements.Values.Single();
            released.Attributes.Add(new BeamReleasesAttribute());
            string analysis = model.AnalysisFingerprint(), binding = model.SolverBindingFingerprint();
            string beam = Fingerprint(released.Attributes);
            Decorate(model);
            Assert.AreEqual(analysis, model.AnalysisFingerprint(), "the mechanical model is unchanged");
            Assert.AreEqual(analysis, ModelValues.Copy(model).AnalysisFingerprint());
            Assert.AreEqual(binding, model.SolverBindingFingerprint());
            // They are data of the element all the same: the content of its attributes changes.
            Assert.AreNotEqual(beam, Fingerprint(released.Attributes));
            // A mechanical attribute still enters.
            released.Attributes.Values.OfType<BeamReleasesAttribute>().Single().I[5] = new BeamDofConnection(BeamConnectionKind.Released);
            Assert.AreNotEqual(analysis, model.AnalysisFingerprint());
            // Without bridge attributes the collection enters as before (the same bytes, the same object). The view is internal and
            // Model.NetFramework.Tests sees no internals: read through reflection.
            var bare = Bare().BeamElements.Values.Single();
            var view = typeof(Model).Assembly.GetType("GPC.Model.Structure.Bridges.BridgeElementAttributes", true)!
                .GetMethod("AnalysisView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
            Assert.AreSame(bare.Attributes, view.Invoke(null, new object[] { bare }));
            Assert.AreSame(released.Attributes.Values.OfType<BeamReleasesAttribute>().Single(),
                ((System.Collections.Generic.IDictionary<int, GPC.Model.Attributes.Attribute>)view.Invoke(null, new object[] { released })!).Values.Single());
        }

        [TestMethod]
        public void InvalidValuesAreRejected()
        {
            Assert.ThrowsException<ArgumentNullException>(() => new BridgeMemberAttribute(null!));
            Assert.ThrowsException<ArgumentException>(() => new BridgeMemberMetadata(" ", BridgeMemberFamily.Girder, 1, 1, 1, 0, 1, new Vector3d(0, 0, 1)), "the metadata validate themselves, as before");
            foreach (var name in new[] { null, "", " ", " G1", "G1 ", "\tG1" })
                Assert.ThrowsException<ArgumentException>(() => new SuperElementAttribute(name!), name ?? "null");
            Assert.ThrowsException<ArgumentException>(() => new BridgeSectionStateAttribute(BridgeSectionBehavior.Steel, false, 6, "C", "F"));
            Assert.ThrowsException<ArgumentException>(() => new BridgeSectionStateAttribute((BridgeSectionBehavior)7, false, 6, "C", "F"));
            foreach (double n in new[] { 0, -0.0, -6, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
                Assert.ThrowsException<ArgumentException>(() => new BridgeSectionStateAttribute(BridgeSectionBehavior.ShortTerm, false, n, "C", "F"), n.ToString());
            Assert.ThrowsException<ArgumentException>(() => new BridgeSectionStateAttribute(BridgeSectionBehavior.ShortTerm, false, 6, " ", "F"));
            foreach (var fingerprint in new[] { null, "", " " })
                Assert.ThrowsException<ArgumentException>(() => new BridgeSectionStateAttribute(BridgeSectionBehavior.Shrinkage, false, 6, "C", fingerprint!));
            Assert.IsNull(new BridgeSectionStateAttribute(BridgeSectionBehavior.Shrinkage, false, 6, null!, "F").ClassificationCase, "no classification case");
            Assert.ThrowsException<ArgumentException>(() => new RigidConnectionAttribute(null!));
            foreach (var p in new[] { new Point3d(double.NaN, 0, 0), new Point3d(0, double.PositiveInfinity, 0), new Point3d(0, 0, double.NegativeInfinity) })
                Assert.ThrowsException<ArgumentException>(() => new RigidConnectionAttribute(p));
            // Immutable: the target given and the target read are copies.
            var target = new Point3d(1, 2, 3); var link = new RigidConnectionAttribute(target);
            target.X = 9; link.TargetMm.Y = 9; Assert.AreEqual(1, link.TargetMm.X); Assert.AreEqual(2, link.TargetMm.Y);

            // Archived data are checked when read.
            var state = Text(new BridgeSectionStateAttribute(BridgeSectionBehavior.LongTerm, false, 18.375, "C", "F"));
            Assert.IsTrue(state.Contains(">18.375<") && state.Contains(">LongTerm<"), state);
            Refused<BridgeSectionStateAttribute>(state.Replace(">18.375<", ">-18.375<"), "InvalidBridgeSectionModularRatio");
            Refused<BridgeSectionStateAttribute>(state.Replace(">LongTerm<", ">Steel<"), "InvalidBridgeSectionBehavior");
            var super = Text(new SuperElementAttribute("G1"));
            Assert.IsTrue(super.Contains(">G1<"), super);
            Refused<SuperElementAttribute>(super.Replace(">G1<", ">G1 <"), "InvalidSuperElementName");
        }

        private static string Text<T>(T value) where T : class
        { using var s = new MemoryStream(); ModelValueArchive.Save(value, s); return Encoding.UTF8.GetString(s.ToArray()); }
        private static void Refused<T>(string xml, string code) where T : class
        {
            try
            {
                using var s = new MemoryStream(Encoding.UTF8.GetBytes(xml));
                ModelValueArchive.Load<T>(s);
            }
            catch (Exception e) { StringAssert.Contains(e.ToString(), code); return; }
            Assert.Fail("Accepted: " + code);
        }

        private static string[] Errors(Model model) => model.ValidateDomain().Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.Code).ToArray();

        [TestMethod]
        public void TheDomainPreflightRefusesAttributesOnTheWrongElement()
        {
            var model = Sample();
            CollectionAssert.AreEqual(Array.Empty<string>(), Errors(model));
            model.RequireExportable();
            // Beam attributes on a node or a plate, a rigid connection on a beam or a plate.
            foreach (var place in new Action<Model>[] {
                m => m.NodesElements.Values.First().Attributes.Add(new BridgeMemberAttribute(Metadata())),
                m => m.NodesElements.Values.First().Attributes.Add(new SuperElementAttribute("G1")),
                m => m.AreaElements.Values.Single().Attributes.Add(new BridgeSectionStateAttribute(BridgeSectionBehavior.ShortTerm, false, 6, null!, "F")) })
            {
                var wrong = Sample(); place(wrong);
                CollectionAssert.AreEqual(new[] { "BridgeBeamAttributeOnWrongElement" }, Errors(wrong));
                StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => wrong.RequireExportable()).Message, "BridgeBeamAttributeOnWrongElement");
            }
            foreach (var place in new Action<Model>[] {
                m => m.BeamElements.Values.Single().Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, 1))),
                m => m.AreaElements.Values.Single().Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, 1))) })
            {
                var wrong = Sample(); place(wrong);
                CollectionAssert.AreEqual(new[] { "RigidConnectionOnWrongElement" }, Errors(wrong));
            }
            // One of a kind per beam; rigid connections without duplicates.
            foreach (var attribute in new GPC.Model.Attributes.Attribute[] { new BridgeMemberAttribute(Metadata()), new SuperElementAttribute("G2"),
                new BridgeSectionStateAttribute(BridgeSectionBehavior.ShortTerm, false, 6, null!, "F") })
            {
                var twice = Sample(); twice.BeamElements.Values.Single().Attributes.Add(attribute);
                CollectionAssert.AreEqual(new[] { "DuplicateBridgeBeamAttribute" }, Errors(twice));
            }
            var duplicate = Sample();
            duplicate.NodesElements.Values.Single(n => n.Name == "1").Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, -1250.5)));
            CollectionAssert.AreEqual(new[] { "DuplicateRigidConnection" }, Errors(duplicate));
            var other = Sample();
            other.NodesElements.Values.Single(n => n.Name == "1").Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, -1250.25)));
            CollectionAssert.AreEqual(Array.Empty<string>(), Errors(other), "several rigid connections per node");
            // A super element renamed to an invalid name.
            var renamed = Sample(); renamed.BeamElements.Values.Single().Attributes.Values.OfType<SuperElementAttribute>().Single().Name = " ";
            CollectionAssert.AreEqual(new[] { "InvalidSuperElementName" }, Errors(renamed));
        }

        [TestMethod]
        public void TheAssemblyKeepsTheBridgeAttributes()
        {
            NodeElement Node(int id, double x, params Point3d[] links)
            {
                var node = new NodeElement(Id(id), new Point3d(x, 0, 0));
                foreach (var link in links) node.Attributes.Add(new RigidConnectionAttribute(link));
                return node;
            }
            BeamElement Beam(int id, NodeElement a, NodeElement b)
            {
                var beam = new BeamElement(Id(id), a.Position, b.Position, null); beam.ConnectNodes(a, b);
                beam.Assignments.Formulation = BeamFormulation.StraightTwoNode;
                beam.Attributes.Add(new BridgeMemberAttribute(Metadata()));
                beam.Attributes.Add(new SuperElementAttribute("G1"));
                beam.Attributes.Add(new BridgeSectionStateAttribute(BridgeSectionBehavior.LongTerm, true, 18.375, "M01_NINF", "A1B2C3"));
                return beam;
            }
            var deck = new Point3d(3000, 0, 1500);
            var first = Beam(20, Node(21, 0), Node(22, 3000, deck));
            var second = Beam(23, Node(24, 3000, deck), Node(25, 6000));
            var model = ModelAssembly.Assemble(new[] { first, second }, Id(26), "", 1.0);
            Assert.AreEqual(3, model.NodesElements.Count, "coincident nodes with the same rigid connection merge");
            Assert.AreEqual(2, model.BeamElements.Count);
            foreach (var beam in model.BeamElements.Values)
            {
                Assert.AreEqual("G1-C2", beam.Attributes.Values.OfType<BridgeMemberAttribute>().Single().Metadata.GeometryId);
                Assert.AreEqual("G1", beam.Attributes.Values.OfType<SuperElementAttribute>().Single().Name);
                Assert.AreEqual(18.375, beam.Attributes.Values.OfType<BridgeSectionStateAttribute>().Single().ModularRatio);
            }
            Assert.AreEqual(1500, model.NodesElements.Values.SelectMany(n => n.Attributes.Values.OfType<RigidConnectionAttribute>()).Single().TargetMm.Z);
            Assert.AreEqual(0, Errors(model).Length);
            // Coincident nodes with different rigid connections are a conflict, never a silent loss.
            var third = Beam(27, Node(28, 3000), Node(29, 6000));
            StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new[] { first, third }, Id(26), "", 1.0)).Message,
                "NodeMergeAssignmentConflict");
            var fourth = Beam(30, Node(31, 3000, new Point3d(3000, 0, 1400)), Node(32, 6000));
            StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(new[] { first, fourth }, Id(26), "", 1.0)).Message,
                "NodeMergeAssignmentConflict");
            // Nodes without attributes merge as before.
            var plain = ModelAssembly.Assemble(new[] { Beam(33, Node(34, 0), Node(35, 3000)), Beam(36, Node(37, 3000), Node(38, 6000)) }, Id(39), "", 1.0);
            Assert.AreEqual(3, plain.NodesElements.Count);
        }
    }
}
