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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>The descriptive rigid connection of a node (lane L11, correzioni-P2B.md §1bis; kept in GPCModel by lane L12, which moved the
    /// bridge data of the beams to GPC.Checkers.CompositeBridge.BridgeModel): the same contract in the XML and compact archives and in the
    /// value copies, out of the analysis fingerprint, kept by the assembly, checked by the domain preflight.</summary>
    [TestClass]
    public class RigidConnectionAttributeTest
    {
        private static Guid Id(int n) => Guid.Parse("00000000-0000-0000-0000-0000000011" + n.ToString("00"));

        /// <summary>Two nodes, a beam and a triangle, without attributes.</summary>
        private static Model Bare()
        {
            var model = new Model(Id(1), "Frame");
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

        /// <summary>Two rigid connections on node 1.</summary>
        private static Model Sample()
        {
            var model = Bare(); Decorate(model); return model;
        }

        private static void Decorate(Model model)
        {
            var node = model.NodesElements.Values.Single(n => n.Name == "1");
            node.Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, -1250.5)));
            node.Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0.125, -2500)));
        }

        private static void SameAttributes(Model model)
        {
            var links = model.NodesElements.Values.Single(n => n.Name == "1").Attributes.Values.OfType<RigidConnectionAttribute>().Select(l => l.TargetMm).ToArray();
            Assert.AreEqual(2, links.Length);
            Assert.AreEqual(-1250.5, links[0].Z); Assert.AreEqual(0.125, links[1].Y); Assert.AreEqual(-2500, links[1].Z);
        }

        private static byte[] Save(Model model, ModelArchiveFormat format) { using var s = new MemoryStream(); ModelArchive.Save(model, s, format); return s.ToArray(); }
        private static Model Load(byte[] bytes) { using var s = new MemoryStream(bytes, false); return ModelArchive.Load(s); }
        private static string Fingerprint(object value) => ModelValues.Fingerprint(new[] { value });

        [TestMethod]
        public void RigidConnectionsRoundTripInTheXmlAndCompactArchives()
        {
            var model = Sample(); SameAttributes(model);
            // An element enters a fingerprint by its identity: the content compared is that of its attributes.
            string analysis = model.AnalysisFingerprint(), node = Fingerprint(model.NodesElements.Values.Single(n => n.Name == "1").Attributes);
            var xml = Save(model, ModelArchiveFormat.Xml); var compact = Save(model, ModelArchiveFormat.Compact);
            Assert.AreEqual("GPCMOD", Encoding.ASCII.GetString(compact, 0, 6), "the compact codec writes the attributes");
            // Two saves of one model are the same bytes (lane L12: the attribute writes the point it holds, not a new copy with a new GUID).
            CollectionAssert.AreEqual(xml, Save(model, ModelArchiveFormat.Xml)); CollectionAssert.AreEqual(compact, Save(model, ModelArchiveFormat.Compact));
            var fromXml = Load(xml); var fromCompact = Load(compact);
            foreach (var other in new[] { fromXml, fromCompact, ModelValues.Copy(model) })
            {
                SameAttributes(other);
                Assert.AreEqual(analysis, other.AnalysisFingerprint());
                Assert.AreEqual(node, Fingerprint(other.NodesElements.Values.Single(n => n.Name == "1").Attributes));
            }
            // The same document from both forms, and again from the compact form read back.
            string document = CompactArchiveTest.Document(fromXml);
            Assert.AreEqual(document, CompactArchiveTest.Document(fromCompact));
            Assert.AreEqual(document, CompactArchiveTest.Document(Load(Save(fromCompact, ModelArchiveFormat.Compact))));
            Assert.AreEqual(document, CompactArchiveTest.Document(model));

            // Standalone values: the two attributes and the node that carries them.
            var carrier = model.NodesElements.Values.Single(n => n.Name == "1");
            var values = carrier.Attributes.Values.Cast<object>().Concat(new object[] { carrier }).ToArray();
            Assert.AreEqual(3, values.Length);
            foreach (var value in values)
            {
                var method = typeof(RigidConnectionAttributeTest).GetMethod(nameof(Value), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(value.GetType());
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
        public void TheAnalysisFingerprintIgnoresTheRigidConnections()
        {
            var model = Bare();
            var released = model.BeamElements.Values.Single();
            released.Attributes.Add(new BeamReleasesAttribute());
            var carrier = model.NodesElements.Values.Single(n => n.Name == "1");
            string analysis = model.AnalysisFingerprint(), binding = model.SolverBindingFingerprint();
            string node = Fingerprint(carrier.Attributes);
            Decorate(model);
            Assert.AreEqual(analysis, model.AnalysisFingerprint(), "the mechanical model is unchanged");
            Assert.AreEqual(analysis, ModelValues.Copy(model).AnalysisFingerprint());
            Assert.AreEqual(binding, model.SolverBindingFingerprint());
            // They are data of the element all the same: the content of its attributes changes.
            Assert.AreNotEqual(node, Fingerprint(carrier.Attributes));
            // A mechanical attribute still enters.
            released.Attributes.Values.OfType<BeamReleasesAttribute>().Single().I[5] = new BeamDofConnection(BeamConnectionKind.Released);
            Assert.AreNotEqual(analysis, model.AnalysisFingerprint());
            // Without rigid connections the collection enters as before (the same bytes, the same object). The view is internal and
            // Model.NetFramework.Tests sees no internals: read through reflection.
            var bare = Bare().NodesElements.Values.First();
            var view = typeof(Model).Assembly.GetType("GPC.Model.Attributes.RigidConnections", true)!
                .GetMethod("AnalysisView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;
            Assert.AreSame(bare.Attributes, view.Invoke(null, new object[] { bare }));
            Assert.AreSame(released.Attributes, view.Invoke(null, new object[] { released }));
            carrier.Attributes.Add(new ElementLabelAttribute("N1"));
            Assert.AreSame(carrier.Attributes.Values.OfType<ElementLabelAttribute>().Single(),
                ((System.Collections.Generic.IDictionary<int, GPC.Model.Attributes.Attribute>)view.Invoke(null, new object[] { carrier })!).Values.Single());
        }

        [TestMethod]
        public void InvalidTargetsAreRejected()
        {
            Assert.ThrowsException<ArgumentException>(() => new RigidConnectionAttribute(null!));
            foreach (var p in new[] { new Point3d(double.NaN, 0, 0), new Point3d(0, double.PositiveInfinity, 0), new Point3d(0, 0, double.NegativeInfinity) })
                Assert.ThrowsException<ArgumentException>(() => new RigidConnectionAttribute(p));
            // Immutable: the target given and the target read are copies.
            var target = new Point3d(1, 2, 3); var link = new RigidConnectionAttribute(target);
            target.X = 9; link.TargetMm.Y = 9; Assert.AreEqual(1, link.TargetMm.X); Assert.AreEqual(2, link.TargetMm.Y);
            // The rigid connection is an attribute of GPCModel, the bridge vocabulary is not (lane L12).
            Assert.AreEqual("GPC.Model.Attributes", typeof(RigidConnectionAttribute).Namespace);
            Assert.IsFalse(typeof(Model).Assembly.GetTypes().Any(t => t.Namespace == "GPC.Model.Structure.Bridges"), "no bridge vocabulary in GPCModel");
            Assert.IsNull(typeof(Model).Assembly.GetType("GPC.Model.Structure.Members.BridgeMemberMetadata"));
            Assert.IsNull(typeof(GPC.Model.Structure.Members.PhysicalMemberDefinition).GetProperty("BridgeMetadata"));
            Assert.IsNull(typeof(GPC.Model.Structure.Members.BeamMemberPart).GetProperty("BridgeMetadata"));

            // Archived data are checked when read.
            var text = Text(new RigidConnectionAttribute(new Point3d(1, 2, 3)));
            Assert.IsTrue(text.Contains("TargetMm"), text);
            var newer = System.Text.RegularExpressions.Regex.Replace(text, "(<RigidConnectionAttributeVersion[^>]*>)1(<)", "${1}2$2");
            Assert.AreNotEqual(text, newer, text);
            Refused<RigidConnectionAttribute>(newer, "NewerSerializationVersion");
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
        public void TheDomainPreflightRefusesRigidConnectionsOnTheWrongElement()
        {
            var model = Sample();
            CollectionAssert.AreEqual(Array.Empty<string>(), Errors(model));
            model.RequireExportable();
            // A rigid connection on a beam or a plate.
            foreach (var place in new Action<Model>[] {
                m => m.BeamElements.Values.Single().Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, 1))),
                m => m.AreaElements.Values.Single().Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, 1))) })
            {
                var wrong = Sample(); place(wrong);
                CollectionAssert.AreEqual(new[] { "RigidConnectionOnWrongElement" }, Errors(wrong));
                StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => wrong.RequireExportable()).Message, "RigidConnectionOnWrongElement");
            }
            // Rigid connections without duplicates.
            var duplicate = Sample();
            duplicate.NodesElements.Values.Single(n => n.Name == "1").Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, -1250.5)));
            CollectionAssert.AreEqual(new[] { "DuplicateRigidConnection" }, Errors(duplicate));
            var other = Sample();
            other.NodesElements.Values.Single(n => n.Name == "1").Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, -1250.25)));
            CollectionAssert.AreEqual(Array.Empty<string>(), Errors(other), "several rigid connections per node");
        }

        [TestMethod]
        public void TheAssemblyKeepsTheRigidConnections()
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
                beam.Attributes.Add(new ElementLabelAttribute("B" + id));
                return beam;
            }
            var deck = new Point3d(3000, 0, 1500);
            var first = Beam(20, Node(21, 0), Node(22, 3000, deck));
            var second = Beam(23, Node(24, 3000, deck), Node(25, 6000));
            var model = ModelAssembly.Assemble(new[] { first, second }, Id(26), "", 1.0);
            Assert.AreEqual(3, model.NodesElements.Count, "coincident nodes with the same rigid connection merge");
            Assert.AreEqual(2, model.BeamElements.Count);
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
