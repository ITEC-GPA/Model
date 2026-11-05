using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Core;
using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Restraints;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>A copy of a model under a new model identity (<see cref="ModelValues.Copy(Model, ModelCopyOptions, Guid)"/>, lane L11: BridGHe
    /// builds its bridge model as a copy of a reference model it keeps). The copy is the copy of <see cref="ModelValues.Copy(Model, ModelCopyOptions)"/>
    /// with only the model GUID changed (and the model its assembly map names): the same elements, properties, cases and identities, the
    /// same sharing, the same documents but for the model GUID, the same analysis state.</summary>
    [TestClass]
    public class ModelIdentityCopyTest
    {
        private static readonly Guid Identity = Guid.Parse("5a1f0e2d-0000-4000-8000-0000000b0001");
        private static readonly ModelCopyOptions Both = ModelCopyOptions.ShareProperties | ModelCopyOptions.ShareCoordinateSystems;
        private static readonly Regex GuidText = new Regex("[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", RegexOptions.CultureInvariant);

        private static byte[] Save(Model model, ModelArchiveFormat format) { using var s = new MemoryStream(); ModelArchive.Save(model, s, format); return s.ToArray(); }
        private static Model Load(byte[] bytes) { using var s = new MemoryStream(bytes, false); return ModelArchive.Load(s); }
        private static string Fingerprint(object value) => ModelValues.Fingerprint(new[] { value });

        /// <summary>The XML document of the model with the GUID <paramref name="from"/> written as <paramref name="to"/>, then every GUID numbered
        /// by first appearance (the copies generate the GUIDs of the geometry again).</summary>
        private static string Text(Model model)
        {
            using var stream = new MemoryStream();
            ModelArchive.SaveDocument(model, stream);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
        private static int Count(string text, Guid guid) => Regex.Matches(text, Regex.Escape(guid.ToString("D"))).Count;
        private static string Document(Model model, Guid from, Guid to)
        {
            string text = Text(model).Replace(from.ToString("D"), to.ToString("D"));
            var seen = new Dictionary<string, int>(StringComparer.Ordinal);
            return GuidText.Replace(text, m => "#" + (seen.TryGetValue(m.Value, out int i) ? i : seen[m.Value] = seen.Count));
        }

        /// <summary>The identities and the content of what the copy keeps from its source, element by element.</summary>
        private static string Content(Model model) => string.Join("\n", model.AllElements.Select(e => e.GetType().Name + " " + e.Id + " " + e.Guid.ToString("D") + " " + e.Name + " "
                + Fingerprint(e.Attributes) + " " + e.Results.Count)
            .Concat(model.BeamProperties.Values.Select(p => "beam property " + p.Name + " " + p.Guid.ToString("D") + " " + Fingerprint(p)))
            .Concat(model.PlateProperties.Values.Select(p => "plate property " + p.Name + " " + p.Guid.ToString("D") + " " + Fingerprint(p)))
            .Concat(model.LoadCases.Values.Select(c => "case " + c.Name + " " + c.Guid.ToString("D")))
            .Concat(model.Groups.Values.Select(g => "group " + g.Name + " " + g.Guid.ToString("D")))
            .Concat(model.AssemblyMap == null ? Enumerable.Empty<string>() : model.AssemblyMap.Entries.Select(a => "assembly " + a.InputGuid.ToString("D") + " " + a.CanonicalGuid.ToString("D") + " " + a.Family + " " + a.ModelId + " " + a.OriginalModelId))
            .Concat(new[] { "datasets " + Fingerprint(model.Datasets), "analysis " + (model.Analysis == null ? "null" : Fingerprint(model.Analysis)), "members " + Fingerprint(model.PhysicalMembers) }));

        /// <summary>The net of the compact archive tests (assembled: it has an assembly map), with its frames, results and, on request, an
        /// analysis snapshot recorded before the results.</summary>
        private static Model Net(bool snapshot)
        {
            var model = CompactArchiveTest.Grid(5, results: !snapshot);
            foreach (var beam in model.BeamElements.Values)
            {
                var d = new Vector3d(beam.StartPoint, beam.EndPoint); d = d / d.Length;
                var side = new Vector3d(0, 0, 1).CrossProduct(d); side = side / side.Length;
                beam.CoordinateSystem = new CoordinateSystem(beam.StartPoint, d, side, "beam " + beam.Id);
            }
            foreach (var node in model.NodesElements.Values)
                foreach (var r in node.Assignments.Restrains)
                    r.Restrain = new NodeRestrain(node, new CoordinateSystem(node.Position, new Vector3d(1, 0, 0.25), new Vector3d(0, 1, 0), "inclined"), r.Restrain.Restrains.ToList());
            if (snapshot) model.CaptureAnalysis();
            return model;
        }

        private static IEnumerable<CoordinateSystem> Frames(Model model) => model.AllElements.SelectMany(e => new[] { e.CoordinateSystem }
            .Concat(e is NodeElement n ? n.Assignments.Restrains.Select(r => r.Restrain?.CoordinateSystem) : Enumerable.Empty<CoordinateSystem>())).Where(cs => cs != null);

        [TestMethod]
        public void TheCopyIsThePlainCopyWithAnotherModelIdentity()
        {
            foreach (bool snapshot in new[] { false, true })
                foreach (var options in new[] { ModelCopyOptions.None, ModelCopyOptions.ShareProperties, ModelCopyOptions.ShareCoordinateSystems, Both })
                {
                    string label = (snapshot ? "snapshot " : "results ") + options;
                    var model = Net(snapshot); Guid source = model.Guid;
                    string analysis = model.AnalysisFingerprint(), binding = model.SolverBindingFingerprint(), sourceContent = Content(model);
                    var plain = ModelValues.Copy(model, options);
                    var copy = ModelValues.Copy(model, options, Identity);

                    // Only the identity of the model changes: the source keeps its own.
                    Assert.AreEqual(Identity, copy.Guid, label); Assert.AreEqual(source, model.Guid, label); Assert.AreEqual(source, plain.Guid, label);
                    Assert.AreEqual(Identity, copy.AssemblyMap.ModelGuid, label); Assert.AreEqual(source, model.AssemblyMap.ModelGuid, label);
                    Assert.AreEqual(model.Name, copy.Name, label);
                    Assert.AreEqual(Content(plain), Content(copy), label);
                    Assert.AreEqual(sourceContent, Content(copy), label);
                    Assert.AreEqual(sourceContent, Content(model), label + ": the source is unchanged");

                    // The same sharing as the plain copy with the same options.
                    var sourceBeams = model.BeamElements.Values.ToArray(); var plainBeams = plain.BeamElements.Values.ToArray(); var beams = copy.BeamElements.Values.ToArray();
                    for (int i = 0; i < beams.Length; i++)
                    {
                        Assert.AreNotSame(sourceBeams[i], beams[i]);
                        Assert.AreEqual(ReferenceEquals(sourceBeams[i].BeamProperty, plainBeams[i].BeamProperty), ReferenceEquals(sourceBeams[i].BeamProperty, beams[i].BeamProperty), label);
                        Assert.AreSame(copy.BeamProperties[beams[i].BeamProperty.Name], beams[i].BeamProperty, label);
                    }
                    Assert.AreEqual(options.SharesProperties, ReferenceEquals(model.BeamProperties["Cable"], copy.BeamProperties["Cable"]), label);
                    Assert.AreEqual(options.SharesProperties, ReferenceEquals(model.PlateProperties["Fabric"], copy.PlateProperties["Fabric"]), label);
                    var sourceFrames = Frames(model).ToArray(); var frames = Frames(copy).ToArray(); var plainFrames = Frames(plain).ToArray();
                    Assert.AreEqual(sourceFrames.Length, frames.Length);
                    Assert.IsTrue(sourceFrames.Zip(frames, (a, b) => ReferenceEquals(a, b) == options.SharesCoordinateSystems).All(x => x), label);
                    Assert.IsTrue(sourceFrames.Zip(plainFrames, (a, b) => ReferenceEquals(a, b) == options.SharesCoordinateSystems).All(x => x), label);

                    // The same documents (XML and compact read back) but for the model GUID: written twice (the model and its assembly
                    // map); a snapshot keeps the source (analysis state of the source model, below).
                    string plainText = Text(plain), copyText = Text(copy);
                    Assert.AreEqual(2, Count(copyText, Identity), label);
                    Assert.AreEqual(Count(plainText, source) - 2, Count(copyText, source), label);
                    Assert.AreEqual(snapshot ? 1 : 0, Count(copyText, source), label);
                    string expected = Document(plain, source, source);
                    Assert.AreEqual(expected, Document(copy, Identity, source), label);
                    foreach (var format in new[] { ModelArchiveFormat.Xml, ModelArchiveFormat.Compact })
                    {
                        var bytes = Save(copy, format);
                        if (format == ModelArchiveFormat.Compact) Assert.AreEqual("GPCMOD", Encoding.ASCII.GetString(bytes, 0, 6), label);
                        var reopened = Load(bytes);
                        Assert.AreEqual(Identity, reopened.Guid, label); Assert.AreEqual(Identity, reopened.AssemblyMap.ModelGuid, label);
                        Assert.AreEqual(Document(Load(Save(plain, format)), source, source), Document(reopened, Identity, source), label + " " + format);
                        Assert.AreEqual(copy.AnalysisFingerprint(), reopened.AnalysisFingerprint(), label + " " + format);
                    }

                    // The fingerprints: the model identity is an input of the analysis fingerprint, nothing else differs. Back to the identity
                    // of the source, the copy gives the fingerprints of the source and of the plain copy again.
                    Assert.AreEqual(analysis, plain.AnalysisFingerprint(), label);
                    Assert.AreNotEqual(analysis, copy.AnalysisFingerprint(), label);
                    var back = ModelValues.Copy(copy, options, source);
                    Assert.AreEqual(analysis, back.AnalysisFingerprint(), label);
                    Assert.AreEqual(binding, back.SolverBindingFingerprint(), label);
                    Assert.AreEqual(plain.VerificationFingerprint(null), back.VerificationFingerprint(null), label);
                    Assert.AreEqual(expected, Document(back, source, source), label);

                    // The assembly map names the copy: an exporter can bind its entities.
                    var ids = copy.AllElements.ToDictionary(e => e.Guid, e => e.Id);
                    Assert.AreEqual(copy.AllElements.Count(), copy.AssemblyMap.ToSolver(copy, "test", ids).Entries.Count, label);
                    Assert.ThrowsException<InvalidOperationException>(() => model.AssemblyMap.ToSolver(copy, "test", ids), label + ": the map of the source is not the copy's");

                    // The analysis state is copied as the plain copy copies it, unchanged: it names the source model.
                    Assert.AreEqual(snapshot, copy.Analysis != null, label);
                    if (snapshot)
                    {
                        Assert.AreEqual(source, copy.Analysis.ModelGuid, label);
                        Assert.AreEqual(Fingerprint(plain.Analysis), Fingerprint(copy.Analysis), label);
                        Assert.AreEqual(AnalysisCompatibility.Compatible, AnalysisCompatibilityValidator.Validate(plain).Status, label);
                        var foreign = AnalysisCompatibilityValidator.Validate(copy);
                        Assert.AreEqual(AnalysisCompatibility.Unknown, foreign.Status, label); Assert.AreEqual("ForeignAnalysisSnapshot", foreign.Code, label);
                        Assert.AreEqual(AnalysisCompatibility.Compatible, AnalysisCompatibilityValidator.Validate(back).Status, label);
                    }
                    else
                    {
                        Assert.IsTrue(copy.AllElements.Any(e => e.Results.Count != 0), label);
                        Assert.AreEqual(Fingerprint(plain.Datasets), Fingerprint(copy.Datasets), label);
                        string[] States(Model m) => m.NodesElements.Values.SelectMany(n => n.Results.SelectMany(r => r.Results)).Select(r => r.State.DatasetId + " " + r.State.InputFingerprint + " " + r.State.Step).ToArray();
                        CollectionAssert.AreEqual(States(plain), States(copy), label + ": the results keep their provenance");
                        Assert.IsTrue(States(copy).Length > 0, label);
                    }
                }
        }

        [TestMethod]
        public void AnEmptyIdentityOrAMissingModelIsRefused()
        {
            var model = Net(false);
            Assert.ThrowsException<ArgumentException>(() => ModelValues.Copy(model, ModelCopyOptions.None, Guid.Empty));
            Assert.ThrowsException<ArgumentException>(() => ModelValues.Copy(model, Both, Guid.Empty));
            Assert.ThrowsException<ArgumentNullException>(() => ModelValues.Copy(null!, ModelCopyOptions.None, Identity));
            // The identity of the source gives the plain copy.
            var same = ModelValues.Copy(model, ModelCopyOptions.None, model.Guid);
            Assert.AreEqual(model.AnalysisFingerprint(), same.AnalysisFingerprint());
            Assert.AreEqual(Document(ModelValues.Copy(model), model.Guid, model.Guid), Document(same, model.Guid, model.Guid));
            // A model without an assembly map.
            var plain = new Model(Guid.Parse("5a1f0e2d-0000-4000-8000-0000000b0002"), "plain");
            plain.NodesElements.Add(new NodeElement(Guid.Parse("5a1f0e2d-0000-4000-8000-0000000b0003"), new Point3d(1, 2, 3), null, "1", 1));
            var renamed = ModelValues.Copy(plain, ModelCopyOptions.None, Identity);
            Assert.IsNull(renamed.AssemblyMap); Assert.AreEqual(Identity, renamed.Guid);
            Assert.AreEqual(plain.NodesElements.Values.Single().Guid, renamed.NodesElements.Values.Single().Guid);
            Assert.AreEqual(Identity, Load(Save(renamed, ModelArchiveFormat.Xml)).Guid);
        }
    }
}
