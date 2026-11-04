using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Core;
using GPC.Model.Core.Diagnostics;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Restraints;
using GPC.Model.Results.Storage;
using GPC.Model.Sections;
using GPC.Model.Structure.Assignments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// Copies that share the immutable properties (<see cref="ModelCopyOptions.ShareProperties"/>, lane L9, user decision of 4/11/2026
    /// «Condividi proprietà»): the elements are copied, the property, material and section instances are those of the source. The copy
    /// fingerprints, saves and reloads as the complete copy does; its elements cite the properties registered in the copy; an assembly of
    /// shared copies is a model of its own. The default copies stay complete.
    /// </summary>
    [TestClass]
    public class SharedPropertyCopyTest
    {
        private static byte[] Save(Model model, ModelArchiveFormat format) { using var s = new MemoryStream(); ModelArchive.Save(model, s, format); return s.ToArray(); }
        private static Model Load(byte[] bytes) { using var s = new MemoryStream(bytes, false); return ModelArchive.Load(s); }
        private static string[] Codes(Model model) => model.ValidateDomain().Select(d => d.Severity + ":" + d.Code + ":" + d.Message).OrderBy(c => c, StringComparer.Ordinal).ToArray();

        [TestMethod]
        public void SharedModelCopiesCiteThePropertiesRegisteredInTheCopy()
        {
            var model = CompactArchiveTest.Grid(8, results: true);
            var shared = ModelValues.Copy(model, ModelCopyOptions.ShareProperties);
            var deep = ModelValues.Copy(model);

            var cable = (ElasticBeamProperty)model.BeamProperties["Cable"];
            var fabric = model.PlateProperties["Fabric"];
            Assert.AreSame(cable, shared.BeamProperties["Cable"]);
            Assert.AreSame(fabric, shared.PlateProperties["Fabric"]);
            Assert.AreNotSame(cable, deep.BeamProperties["Cable"], "the default copy stays complete");
            Assert.AreNotSame(cable.Material, ((ElasticBeamProperty)deep.BeamProperties["Cable"]).Material);
            // the registry is the copy's own, holding the instances of the source; every element cites the registered instance
            Assert.AreNotSame(model.BeamProperties, shared.BeamProperties);
            Assert.IsTrue(shared.BeamElements.Values.All(b => ReferenceEquals(b.BeamProperty, shared.BeamProperties[b.BeamProperty.Name])));
            Assert.IsTrue(shared.AreaElements.Values.All(p => ReferenceEquals(p.PlateProperty, shared.PlateProperties[p.PlateProperty.Name])));
            Assert.IsTrue(deep.BeamElements.Values.All(b => ReferenceEquals(b.BeamProperty, deep.BeamProperties[b.BeamProperty.Name])));

            // everything else is a copy: elements, nodes, frames, cases, groups, attributes
            Assert.IsTrue(shared.AllElements.Zip(model.AllElements, (a, b) => !ReferenceEquals(a, b) && a.Guid == b.Guid).All(same => same));
            Assert.IsTrue(shared.AllElements.Zip(model.AllElements, (a, b) => !ReferenceEquals(a.CoordinateSystem, b.CoordinateSystem)).All(same => same));
            Assert.AreNotSame(model.LoadCases["FF"], shared.LoadCases["FF"]);
            var beam = shared.BeamElements.Values.First();
            Assert.AreSame(shared.NodesElements[beam.NodeI.Id], beam.NodeI);
            Assert.AreNotSame(model.BeamElements.Values.First().Attributes.Values.Single(), beam.Attributes.Values.Single());

            // the same fingerprints, results, diagnostics and documents as the complete copy
            Assert.AreEqual(deep.AnalysisFingerprint(), shared.AnalysisFingerprint());
            Assert.AreEqual(model.AnalysisFingerprint(), shared.AnalysisFingerprint());
            Assert.AreEqual(deep.VerificationFingerprint(null), shared.VerificationFingerprint(null));
            CollectionAssert.AreEqual(Codes(deep), Codes(shared));
            // the XML document holds the results too (decoded)
            Assert.AreEqual(CompactArchiveTest.Document(deep), CompactArchiveTest.Document(shared));

            // saved and read back as the complete copy: one instance per property again, the same fingerprint and document. The bytes of
            // two complete copies differ by the geometry GUIDs that a copy generates again (the outline of the section here, which the
            // compact form writes): the shared copy, which keeps the outline of its source, writes the bytes of its source.
            foreach (var format in new[] { ModelArchiveFormat.Xml, ModelArchiveFormat.Compact })
            {
                var fromShared = Save(shared, format); var fromDeep = Save(deep, format);
                if (format == ModelArchiveFormat.Compact) CollectionAssert.AreEqual(Save(model, format), fromShared, "compact bytes of the source");
                var reopened = Load(fromShared);
                Assert.AreEqual(model.AnalysisFingerprint(), reopened.AnalysisFingerprint(), format.ToString());
                Assert.AreEqual(CompactArchiveTest.Document(Load(fromDeep)), CompactArchiveTest.Document(reopened), format.ToString());
                Assert.IsTrue(reopened.BeamElements.Values.All(b => ReferenceEquals(b.BeamProperty, reopened.BeamProperties["Cable"])), format.ToString());
                Assert.AreNotSame(cable, reopened.BeamProperties["Cable"]);
            }

            // a changed element of the copy leaves the source as it was
            string before = model.AnalysisFingerprint();
            beam.NodeI.Position = new Point3d(-1, -2, -3);
            beam.Attributes.Clear();
            shared.LoadCases["FF"].Name = "Renamed";
            Assert.AreEqual(before, model.AnalysisFingerprint());
            Assert.AreEqual("FF", model.LoadCases.Values.Single().Name);
        }

        [TestMethod]
        public void SharedValueCopiesCopyTheValueAndShareWhatItCites()
        {
            var model = CompactArchiveTest.Grid(3, results: false);
            var property = (ElasticBeamProperty)model.BeamProperties["Cable"];
            var source = model.BeamElements.Values.First();

            var beam = ModelValues.CopyValue(source, ModelCopyOptions.ShareProperties);
            Assert.AreNotSame(source, beam);
            Assert.AreSame(property, beam.BeamProperty);
            Assert.AreNotSame(source.NodeI, beam.NodeI);
            Assert.AreNotSame(source.CoordinateSystem, beam.CoordinateSystem);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { ModelValues.CopyValue(source) }), ModelValues.Fingerprint(new object[] { beam }));

            // the value itself is always copied, also a property: then its material and section are shared
            var copiedProperty = ModelValues.CopyValue(property, ModelCopyOptions.ShareProperties);
            Assert.AreNotSame(property, copiedProperty);
            Assert.AreEqual(property.Guid, copiedProperty.Guid);
            Assert.AreSame(property.Material, copiedProperty.Material);
            Assert.AreSame(property.Section, copiedProperty.Section);
            var copiedSection = ModelValues.CopyValue((Section)property.Section, ModelCopyOptions.ShareProperties);
            Assert.AreNotSame(property.Section, copiedSection);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { property.Section }), ModelValues.Fingerprint(new object[] { copiedSection }));

            // aliases inside a value: two beams of one array share the one property instance, their common node stays one copy
            var pair = model.BeamElements.Values.Where(b => ReferenceEquals(b.NodeI, source.NodeI) || ReferenceEquals(b.NodeJ, source.NodeI)).Take(2).ToArray();
            Assert.AreEqual(2, pair.Length);
            var copies = ModelValues.CopyValue(pair, ModelCopyOptions.ShareProperties);
            Assert.IsTrue(copies.All(b => ReferenceEquals(b.BeamProperty, property)));
            NodeElement Common(BeamElement b) => b.NodeI.Guid == source.NodeI.Guid ? b.NodeI : b.NodeJ;
            Assert.AreSame(Common(copies[0]), Common(copies[1]));
            Assert.AreNotSame(source.NodeI, Common(copies[0]));

            // nothing is kept between copies: the next default copy is complete again
            var complete = ModelValues.CopyValue(source);
            Assert.AreNotSame(property, complete.BeamProperty);
            Assert.AreNotSame(property.Material, ((ElasticBeamProperty)complete.BeamProperty).Material);
            Assert.AreNotSame(property.Section, ((ElasticBeamProperty)complete.BeamProperty).Section);

            // copy on modify: the shared instance is never changed, the copy carries the new value
            var stiffer = ModelValues.CopyValue(property);
            stiffer.Material.ElasticModulusTension = 2 * property.Material.ElasticModulusTension;
            beam.BeamProperty = stiffer;
            Assert.AreEqual(210000, property.Material.ElasticModulusTension);
            Assert.AreEqual(210000, property.Material.E);
            Assert.AreSame(property, source.BeamProperty);
        }

        private static readonly ModelCopyOptions Both = ModelCopyOptions.ShareProperties | ModelCopyOptions.ShareCoordinateSystems;

        /// <summary>A coordinate system bit for bit: name, origin, axes and what its cached components give (ToGlobal/ToLocal of a point).</summary>
        private static string Frame(CoordinateSystem cs)
        {
            string B(double v) => BitConverter.DoubleToInt64Bits(v).ToString("X16");
            string P(double x, double y, double z) => B(x) + B(y) + B(z);
            var g = cs.ToGlobal(new Point3d(1.5, -2.5, 3.25)); var l = cs.ToLocal(new Point3d(1.5, -2.5, 3.25));
            return cs.Name + "|" + P(cs.Origin.X, cs.Origin.Y, cs.Origin.Z) + P(cs.V1.X, cs.V1.Y, cs.V1.Z) + P(cs.V2.X, cs.V2.Y, cs.V2.Z) + P(cs.V3.X, cs.V3.Y, cs.V3.Z)
                + P(g.X, g.Y, g.Z) + P(l.X, l.Y, l.Z);
        }
        private static IEnumerable<CoordinateSystem> Frames(Model model) => model.AllElements.SelectMany(e => new[] { e.CoordinateSystem }
            .Concat(e is NodeElement n ? n.Assignments.Restrains.Select(r => r.Restrain?.CoordinateSystem) : Enumerable.Empty<CoordinateSystem>())
            .Concat(e.Loads.Values.Select(x => x.CoordinateSystem))).Where(cs => cs != null);

        /// <summary>The net with its own frames: beam axes at the start node, restraints along an inclined plane at their node.</summary>
        private static Model Framed(int k, bool results = true)
        {
            var model = CompactArchiveTest.Grid(k, results);
            foreach (var beam in model.BeamElements.Values)
            {
                var d = new Vector3d(beam.StartPoint, beam.EndPoint); d = d / d.Length;
                var side = new Vector3d(0, 0, 1).CrossProduct(d); side = side / side.Length;
                beam.CoordinateSystem = new CoordinateSystem(beam.StartPoint, d, side, "beam " + beam.Id);
            }
            foreach (var node in model.NodesElements.Values)
                foreach (var r in node.Assignments.Restrains)
                    r.Restrain = new NodeRestrain(node, new CoordinateSystem(node.Position, new Vector3d(1, 0, 0.25), new Vector3d(0, 1, 0), "inclined"), r.Restrain.Restrains.ToList());
            return model;
        }

        /// <summary>User decision of 4/11/2026 (lane L9, «si» to sharing the coordinate systems, provided a test shows they stay unchanged):
        /// copies with <see cref="ModelCopyOptions.ShareCoordinateSystems"/> cite the coordinate systems of the source, fingerprint and save
        /// as the complete copy, and the edits made downstream (new frames assigned, nodes moved, restraints replaced, assemblies, complete
        /// copies and archives of the copy changed in place, compaction) leave every coordinate system of the source as it was.</summary>
        [TestMethod]
        public void SharedCoordinateSystemsStayAsTheyWereAfterTheEditsDownstream()
        {
            var model = Framed(6);
            var frames = Frames(model).Select(Frame).ToArray(); string fingerprint = model.AnalysisFingerprint();
            Assert.IsTrue(frames.Length > model.AllElements.Count(), "frames of elements and restraints");
            var shared = ModelValues.Copy(model, Both);
            var deep = ModelValues.Copy(model);
            var onlyProperties = ModelValues.Copy(model, ModelCopyOptions.ShareProperties);
            CollectionAssert.AreEqual(Frames(model).ToArray(), Frames(shared).ToArray(), "shared instances");
            Assert.IsTrue(Frames(model).Zip(Frames(deep), (a, b) => !ReferenceEquals(a, b)).All(x => x), "the complete copy copies them");
            Assert.IsTrue(Frames(model).Zip(Frames(onlyProperties), (a, b) => !ReferenceEquals(a, b)).All(x => x), "ShareProperties alone copies them");
            Assert.IsTrue(shared.AllElements.Zip(model.AllElements, (a, b) => ReferenceEquals(a.CoordinateSystem, b.CoordinateSystem)).All(x => x));
            Assert.IsTrue(Frames(model).Zip(Frames(shared), (a, b) => ReferenceEquals(a, b)).All(x => x));
            Assert.AreSame(model.BeamProperties["Cable"], shared.BeamProperties["Cable"]);
            // the same fingerprints, document and compact bytes (a coordinate system writes no GUID in the compact form)
            Assert.AreEqual(deep.AnalysisFingerprint(), shared.AnalysisFingerprint());
            Assert.AreEqual(CompactArchiveTest.Document(deep), CompactArchiveTest.Document(shared));
            CollectionAssert.AreEqual(Save(model, ModelArchiveFormat.Compact), Save(shared, ModelArchiveFormat.Compact));
            var reopened = Load(Save(shared, ModelArchiveFormat.Compact));
            Assert.AreEqual(fingerprint, reopened.AnalysisFingerprint());
            Assert.IsTrue(Frames(reopened).Zip(Frames(model), (a, b) => !ReferenceEquals(a, b) && Frame(a) == Frame(b)).All(x => x));
            // a value copy shares what it cites and copies itself
            var beam = model.BeamElements.Values.First();
            Assert.AreSame(beam.CoordinateSystem, ModelValues.CopyValue(beam, Both).CoordinateSystem);
            var frameCopy = ModelValues.CopyValue(beam.CoordinateSystem, Both);
            Assert.AreNotSame(beam.CoordinateSystem, frameCopy); Assert.AreEqual(Frame(beam.CoordinateSystem), Frame(frameCopy));

            // Downstream, as a component does: new frames and restraints assigned, nodes moved, the results compacted again
            foreach (var b in shared.BeamElements.Values) b.CoordinateSystem = new CoordinateSystem(b.StartPoint, new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), "turned");
            foreach (var node in shared.NodesElements.Values)
            {
                node.Position = new Point3d(node.Position.X + 1, node.Position.Y, node.Position.Z);
                foreach (var r in node.Assignments.Restrains) r.Restrain = new NodeRestrain(node, new CoordinateSystem(node.Position, new Vector3d(0, 1, 0), new Vector3d(0, 0, 1)), r.Restrain.Restrains.ToList());
            }
            foreach (var element in shared.AllElements) { var count = element.Results.Count; }
            shared.CompactResults();
            // what is not shared may be changed in place: a complete copy, an assembly of shared element copies, a model read back
            var again = ModelValues.Copy(ModelValues.Copy(model, Both));
            var bare = Framed(4, results: false); var bareFrames = Frames(bare).Select(Frame).ToArray();
            var assembled = ModelAssembly.Assemble(bare.AllElements.Select(e => ModelValues.CopyValue(e, Both)).ToArray(), Guid.NewGuid(), "assembled");
            var changed = new List<CoordinateSystem>();
            foreach (var other in new[] { again, assembled, reopened })
                foreach (var cs in Frames(other).ToArray())
                {
                    if (changed.Any(x => ReferenceEquals(x, cs))) continue;
                    Assert.IsFalse(Frames(model).Concat(Frames(bare)).Any(x => ReferenceEquals(x, cs)), "a copy of its own");
                    cs.RotateV3(0.3); cs.SetOrigin(new Point3d(-1, -2, -3)); cs.Origin.X = 7; changed.Add(cs);
                }
            CollectionAssert.AreEqual(frames, Frames(model).Select(Frame).ToArray(), "the coordinate systems of the source are unchanged");
            CollectionAssert.AreEqual(bareFrames, Frames(bare).Select(Frame).ToArray(), "the coordinate systems of the assembled drafts are unchanged");
            Assert.AreEqual(fingerprint, model.AnalysisFingerprint());
        }

        /// <summary>ModelAssembly copies its input as one graph (complete): the assembled model registers its own property, cited by every
        /// beam, whether the inputs share a property instance with their source or carry their own copies; same fingerprint. A modified
        /// property that keeps the identity of the original is refused, so a copy on modify that replaces a property for some elements
        /// only needs a new identity.</summary>
        [TestMethod]
        public void SharedElementCopiesAssembleIntoAModelOfTheirOwn()
        {
            var source = CompactArchiveTest.Grid(4, results: false);
            var property = source.BeamProperties["Cable"];
            Element[] Drafts(ModelCopyOptions options) => source.AllElements.Select(e => ModelValues.CopyValue(e, options)).ToArray();
            var sharedDrafts = Drafts(ModelCopyOptions.ShareProperties);
            Assert.IsTrue(sharedDrafts.OfType<BeamElement>().All(b => ReferenceEquals(b.BeamProperty, property)));
            Assert.IsTrue(sharedDrafts.OfType<AreaElement>().All(p => ReferenceEquals(p.PlateProperty, source.PlateProperties["Fabric"])));

            var owner = Guid.Parse("7b0c2a55-0000-4000-8000-00000000a000");
            var shared = ModelAssembly.Assemble(sharedDrafts, owner, "Scale");
            var deep = ModelAssembly.Assemble(Drafts(ModelCopyOptions.None), owner, "Scale");
            foreach (var model in new[] { shared, deep })
            {
                Assert.AreEqual(1, model.BeamProperties.Count);
                var registered = model.BeamProperties["Cable"];
                Assert.AreNotSame(property, registered, "the assembled model owns its property");
                Assert.IsTrue(model.BeamElements.Values.All(b => ReferenceEquals(b.BeamProperty, registered)));
                Assert.IsTrue(model.AreaElements.Values.All(p => ReferenceEquals(p.PlateProperty, model.PlateProperties["Fabric"])));
                Assert.IsFalse(model.ValidateDomain().Any(d => d.Severity == DiagnosticSeverity.Error));
                Assert.AreEqual(model.BeamElements.Count, sharedDrafts.OfType<BeamElement>().Count());
            }
            if (!shared.LoadCases.ContainsKey("FF")) shared.LoadCases.Add(ModelValues.CopyValue(source.LoadCases["FF"]));
            if (!deep.LoadCases.ContainsKey("FF")) deep.LoadCases.Add(ModelValues.CopyValue(source.LoadCases["FF"]));
            Assert.AreEqual(deep.AnalysisFingerprint(), shared.AnalysisFingerprint());
            Assert.AreEqual(source.AnalysisFingerprint(), shared.AnalysisFingerprint());
            CollectionAssert.AreEqual(Codes(deep), Codes(shared));
            // the drafts and their source are untouched by the assembly
            Assert.IsTrue(sharedDrafts.OfType<BeamElement>().All(b => ReferenceEquals(b.BeamProperty, property)));
            Assert.AreSame(property, source.BeamProperties["Cable"]);

            // copy on modify keeping the identity: refused by the assembly (two contents for one property identity)
            var drafts = Drafts(ModelCopyOptions.ShareProperties);
            var modified = (ElasticBeamProperty)ModelValues.CopyValue(property);
            modified.ShearArea1 = 10;
            drafts.OfType<BeamElement>().Last().BeamProperty = modified;
            var refused = Assert.ThrowsException<ArgumentException>(() => ModelAssembly.Assemble(drafts, owner, "Scale"));
            StringAssert.StartsWith(refused.Message, "PropertyConflict");
            // with an identity of its own (and its own name) it is a second property of the model
            var renamed = new ElasticBeamProperty("Cable stiff", modified.Material, modified.Section, 10);
            drafts.OfType<BeamElement>().Last().BeamProperty = renamed;
            var two = ModelAssembly.Assemble(drafts, owner, "Scale");
            Assert.AreEqual(2, two.BeamProperties.Count);
            Assert.AreEqual(0, ((ElasticBeamProperty)two.BeamProperties["Cable"]).ShearArea1);
        }
    }
}
