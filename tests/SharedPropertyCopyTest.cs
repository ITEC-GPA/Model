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
