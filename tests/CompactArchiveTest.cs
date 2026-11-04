using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Core;
using GPC.Model.Core.Identity;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Restraints;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;
using GPC.Model.Results.Storage;
using GPC.Model.Structure.Assignments;
using GPC.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>The compact archives (lane L5, review A2): ModelArchive version 6, ModelValueArchive version 2, sidecar manifest version 2.
    /// A model read back from the compact form is the model read back from the XML: the same document, the same references (compared
    /// on the XML document of both, the GUIDs numbered by first appearance because those of the geometry are generated again on reading).</summary>
    [TestClass]
    public class CompactArchiveTest
    {
        private string folder = "";
        [TestInitialize] public void Start() { folder = Path.Combine(Path.GetTempPath(), "gpc-compact-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder); }
        [TestCleanup] public void End() { Directory.Delete(folder, true); }

        private static readonly Regex GuidText = new Regex("[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}", RegexOptions.CultureInvariant);

        /// <summary>The XML document of the model, its GUIDs replaced by their order of first appearance.</summary>
        internal static string Document(Model model)
        {
            using var stream = new MemoryStream();
            ModelArchive.SaveDocument(model, stream);
            var seen = new Dictionary<string, int>(StringComparer.Ordinal);
            return GuidText.Replace(Encoding.UTF8.GetString(stream.ToArray()), m => "#" + (seen.TryGetValue(m.Value, out int i) ? i : seen[m.Value] = seen.Count));
        }

        private static byte[] Save(Model model, ModelArchiveFormat format) { using var s = new MemoryStream(); ModelArchive.Save(model, s, format); return s.ToArray(); }
        private static Model Load(byte[] bytes) { using var s = new MemoryStream(bytes, false); return ModelArchive.Load(s); }
        private static bool Compact(byte[] bytes) => bytes.Length > 8 && Encoding.ASCII.GetString(bytes, 0, 6) == "GPCMOD";

        /// <summary>Every archive fixture of the test folder (versions 3 and 5, reports, detailing, member limits): read, written in the
        /// compact form, read back: the same document as the XML gives.</summary>
        [TestMethod]
        public void CompactArchivesReadBackEveryFixtureAsTheXmlDoes()
        {
            string fixtures = Path.Combine(AppContext.BaseDirectory, "Fixtures");
            var files = Directory.GetFiles(fixtures, "*.xml.gz").Concat(Directory.GetFiles(fixtures, "*.zip").Where(f => Path.GetFileName(f).StartsWith("model-") || Path.GetFileName(f).StartsWith("doubles-"))).ToArray();
            Assert.IsTrue(files.Length >= 3, "fixtures: " + files.Length);
            int read = 0;
            foreach (var file in files)
            {
                byte[] bytes = File.ReadAllBytes(file);
                if (file.EndsWith(".gz", StringComparison.Ordinal)) using (var zip = new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress)) using (var plain = new MemoryStream()) { zip.CopyTo(plain); bytes = plain.ToArray(); }
                Model source;
                try { source = Load(bytes); } catch (SerializationException) { continue; } // a check-report fixture is not a model archive
                read++;
                // Lazy values (section shapes) computed by the fingerprint are written by both forms: the XML of the same state is the reference.
                string fingerprint = source.AnalysisFingerprint();
                var compact = Save(source, ModelArchiveFormat.Compact);
                string expected = Document(Load(Save(source, ModelArchiveFormat.Xml)));
                Assert.IsTrue(Compact(compact), Path.GetFileName(file));
                var reopened = Load(compact);
                Assert.AreEqual(fingerprint, reopened.AnalysisFingerprint(), Path.GetFileName(file));
                Assert.AreEqual(expected, Document(reopened), Path.GetFileName(file));
                // and again from the model read back (no state of the source is needed)
                Assert.AreEqual(expected, Document(Load(Save(reopened, ModelArchiveFormat.Compact))), Path.GetFileName(file));
            }
            Assert.IsTrue(read >= 3, "model fixtures read: " + read);
        }

        [TestMethod]
        public void CompactNetIsExactAndAnOrderOfMagnitudeSmaller()
        {
            var model = Grid(12, results: true);
            string expected = Document(model), fingerprint = model.AnalysisFingerprint();
            var xml = Save(model, ModelArchiveFormat.Xml); var compact = Save(model, ModelArchiveFormat.Compact);
            Assert.IsTrue(Compact(compact));
            Assert.IsTrue(compact.Length * 5 < xml.Length, compact.Length + " vs " + xml.Length);
            var reopened = Load(compact);
            Assert.AreEqual(fingerprint, reopened.AnalysisFingerprint());
            Assert.AreEqual(expected, Document(reopened));
            Assert.AreEqual(Document(Load(xml)), Document(reopened));
            // the default form is the XML of before (ANTHEA): same bytes as the explicit Xml form, readable by every version
            using (var s = new MemoryStream()) { ModelArchive.Save(model, s); CollectionAssert.AreEqual(xml, s.ToArray()); }
            Assert.IsFalse(Compact(xml));
        }

        [TestMethod]
        public void CompactValuesAreTheValuesOfTheXml()
        {
            var model = Grid(3, results: false);
            var values = new object[] { model.BeamProperties.Values.First(), model.PlateProperties.Values.First(), model.LoadCases.Values.First(),
                ((ElasticBeamProperty)model.BeamProperties.Values.First()).Material };
            foreach (var value in values)
            {
                var method = typeof(CompactArchiveTest).GetMethod(nameof(Value), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!.MakeGenericMethod(value.GetType());
                method.Invoke(null, new[] { value });
            }
        }

        private static void Value<T>(T value) where T : class
        {
            byte[] xml, compact;
            using (var s = new MemoryStream()) { ModelValueArchive.Save(value, s); xml = s.ToArray(); }
            using (var s = new MemoryStream()) { ModelValueArchive.Save(value, s, ModelArchiveFormat.Compact); compact = s.ToArray(); }
            Assert.AreEqual("GPCVAL", Encoding.ASCII.GetString(compact, 0, 6));
            T a, b;
            using (var s = new MemoryStream(xml)) a = ModelValueArchive.Load<T>(s);
            using (var s = new MemoryStream(compact)) b = ModelValueArchive.Load<T>(s);
            Assert.AreEqual(ModelValues.Fingerprint(new object[] { value }), ModelValues.Fingerprint(new object[] { b }), typeof(T).Name);
            byte[] again1, again2;
            using (var s = new MemoryStream()) { ModelValueArchive.Save(a, s); again1 = s.ToArray(); }
            using (var s = new MemoryStream()) { ModelValueArchive.Save(b, s); again2 = s.ToArray(); }
            var seen1 = new Dictionary<string, int>(); var seen2 = new Dictionary<string, int>();
            Assert.AreEqual(GuidText.Replace(Encoding.UTF8.GetString(again1), m => "#" + (seen1.TryGetValue(m.Value, out int i) ? i : seen1[m.Value] = seen1.Count)),
                GuidText.Replace(Encoding.UTF8.GetString(again2), m => "#" + (seen2.TryGetValue(m.Value, out int i) ? i : seen2[m.Value] = seen2.Count)), typeof(T).Name);
        }

        [TestMethod]
        public void CorruptOrTruncatedCompactArchivesAreRefusedExplicitly()
        {
            var compact = Save(Grid(4, results: true), ModelArchiveFormat.Compact);
            foreach (int length in new[] { 7, 20, 40, compact.Length / 2, compact.Length - 1 })
                Assert.ThrowsException<SerializationException>(() => Load(compact.Take(length).ToArray()), "length " + length);
            var future = (byte[])compact.Clone(); future[6] = 7;
            Assert.ThrowsException<SerializationException>(() => Load(future));
            var declared = (byte[])compact.Clone(); BitConverter.GetBytes(long.MaxValue / 2).CopyTo(declared, 8);
            Assert.ThrowsException<SerializationException>(() => Load(declared));
        }

        /// <summary>Above 64 MB of XML the default form is the package of version 5, as before; the XML is no longer written to its end
        /// before the package (the document stops at the limit).</summary>
        [TestMethod]
        public void LargeModelsAreStillPackagesInTheDefaultForm()
        {
            var model = Grid(40, results: false);
            var bytes = Save(model, ModelArchiveFormat.Xml);
            Assert.AreEqual("PK", Encoding.ASCII.GetString(bytes, 0, 2));
            Assert.AreEqual(model.AnalysisFingerprint(), Load(bytes).AnalysisFingerprint());
        }

        /// <summary>A sidecar run (manifest version 2) is cut into parts below the ceiling of a block: the XML graph of 1,024 nodes
        /// (48 MB) takes several parts; in both graph forms a new session on the saved file opens the same model and results.</summary>
        [TestMethod]
        public void SidecarRunsAreCutIntoPartsAndReopenInBothForms()
        {
            var model = Grid(32, results: true);
            string expected = Document(model), fingerprint = model.AnalysisFingerprint();
            foreach (var format in new[] { ModelArchiveFormat.Xml, ModelArchiveFormat.Compact })
            {
                string document = Path.Combine(folder, format + ".ref"), sidecar = ExternalModelArchive.PathFor(document);
                var store = new ExternalModelArchive { GraphFormat = format };
                var reference = store.Prepare(model);
                Assert.AreEqual(expected, Document(store.Open(reference).Model), format + " unsaved");
                reference = store.Save(sidecar, new[] { reference }, r => BlockArchive.WriteFile(document, s => r[0].Write(s)))[0];
                int blocks = BitConverter.ToInt32(File.ReadAllBytes(sidecar), 20);
                if (format == ModelArchiveFormat.Xml) Assert.IsTrue(blocks >= 5, "blocks " + blocks); // 3+ graph parts, results, manifest
                ExternalModelReference read;
                using (var input = File.OpenRead(document)) read = ExternalModelReference.Read(input);
                var session = new ExternalModelArchive(sidecar, new[] { read });
                var run = session.Open(read);
                Assert.AreEqual(fingerprint, run.Model.AnalysisFingerprint());
                Assert.AreEqual(expected, Document(run.Model), format.ToString());
                Assert.AreEqual(1, session.BlocksForDataset(read, "form-finding").Count);
                Assert.AreEqual(0, session.BlocksForDataset(read, "other").Count);
                // an unchanged autosave keeps every block of the run
                session.Save(sidecar, new[] { read }, r => BlockArchive.WriteFile(document, s => r[0].Write(s)));
                Assert.AreEqual(expected, Document(ExternalModelArchive.Open(document, read).Model));
            }
        }

        /// <summary>Review A2 blocker: a model of more than 60,000 elements (10,201 nodes, 30,000 bars, 20,000 triangles) with results
        /// is prepared, saved and reopened from its file in a new session with the same fingerprint (version 1 failed beyond ~25,000
        /// elements: one graph block above 256 MB).</summary>
        [TestMethod]
        public void SidecarsHoldModelsOfMoreThanSixtyThousandElements()
        {
            var model = Grid(101, results: true);
            Assert.IsTrue(model.AllElements.Count() > 60000);
            string fingerprint = model.AnalysisFingerprint(); var numbers = Numbers(model);
            string document = Path.Combine(folder, "large.ref"), sidecar = ExternalModelArchive.PathFor(document);
            var store = new ExternalModelArchive();
            var reference = store.Prepare(model);
            reference = store.Save(sidecar, new[] { reference }, r => BlockArchive.WriteFile(document, s => r[0].Write(s)))[0];
            ExternalModelReference read;
            using (var input = File.OpenRead(document)) read = ExternalModelReference.Read(input);
            var run = ExternalModelArchive.Open(document, read);
            Assert.AreEqual(fingerprint, run.Model.AnalysisFingerprint());
            CollectionAssert.AreEqual(numbers, Numbers(run.Model));
            var archive = Save(run.Model, ModelArchiveFormat.Compact);
            Assert.AreEqual(fingerprint, Load(archive).AnalysisFingerprint());
        }

        /// <summary>A version 1 sidecar written by Model cc050bec (tools/ExternalArchiveProbe write, 40 nodes, 2 cases, solver map) is read
        /// by this version, and a session on it saves a new version 2 run beside it without losing the old one.</summary>
        [TestMethod]
        public void VersionOneSidecarsAreStillRead()
        {
            string source = Path.Combine(AppContext.BaseDirectory, "Fixtures", "external-v1-cc050bec");
            foreach (var file in Directory.GetFiles(source)) File.Copy(file, Path.Combine(folder, Path.GetFileName(file)));
            string document = Path.Combine(folder, "probe.gh-reference"), sidecar = ExternalModelArchive.PathFor(document);
            ExternalModelReference old;
            using (var input = File.OpenRead(document)) old = ExternalModelReference.Read(input);
            var run = ExternalModelArchive.Open(document, old);
            Assert.AreEqual(File.ReadAllText(Path.Combine(folder, "input.sha256")), run.Model.AnalysisFingerprint());
            Assert.AreEqual(File.ReadAllText(Path.Combine(folder, "solver-map.sha256")), ModelValues.Fingerprint(new object[] { run.SolverMap! }));
            Assert.AreEqual(File.ReadAllText(Path.Combine(folder, "numeric.sha256")), ProbeNumbers(run.Model));
            var session = new ExternalModelArchive(sidecar, new[] { old });
            var added = session.Prepare(Grid(4, results: true));
            var saved = session.Save(sidecar, new[] { added, old }, r => BlockArchive.WriteFile(document, s => { foreach (var item in r) item.Write(s); }));
            var again = new ExternalModelArchive(sidecar, saved);
            Assert.AreEqual(File.ReadAllText(Path.Combine(folder, "input.sha256")), again.Open(saved[1]).Model.AnalysisFingerprint());
            Assert.AreEqual(Grid(4, results: true).AllElements.Count(), again.Open(saved[0]).Model.AllElements.Count());
        }

        // The digest of tools/ExternalArchiveProbe (Numeric), to check the fixture written by the version before.
        private static string ProbeNumbers(Model model)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                foreach (var node in model.NodesElements.Values)
                {
                    writer.Write(node.Guid.ToByteArray()); writer.Write(node.Id);
                    writer.Write(node.Position.X); writer.Write(node.Position.Y); writer.Write(node.Position.Z);
                    foreach (var sample in node.Results.SelectMany(r => r.Results).Cast<NodeResultDisplacement>())
                    {
                        writer.Write(((GPC.Model.ModelObject)sample.Case).Guid.ToByteArray()); writer.Write(sample.Case.Name); writer.Write(sample.State.DatasetId); writer.Write(sample.State.Step);
                        var d = sample.ResultDisplacement; writer.Write(d.D1); writer.Write(d.D2); writer.Write(d.D3); writer.Write(d.R1); writer.Write(d.R2); writer.Write(d.R3);
                    }
                }
            return ArchiveBlock.Digest(stream.ToArray());
        }

        private static long[] Numbers(Model model) => model.NodesElements.Values.SelectMany(n => new[] { n.Position.X, n.Position.Y, n.Position.Z }
            .Concat(n.Results.SelectMany(r => r.Results).Cast<NodeResultDisplacement>().SelectMany(d => new[] { d.ResultDisplacement.D1, d.ResultDisplacement.D2, d.ResultDisplacement.D3 })))
            .Select(BitConverter.DoubleToInt64Bits).ToArray();

        /// <summary>A tensile net like the one of Arachne: k x k nodes, the perimeter restrained, horizontal, vertical and diagonal cables
        /// with a force-density goal, two membrane triangles per square; optionally one displacement result per node.</summary>
        internal static Model Grid(int k, bool results)
        {
            var owner = Guid.Parse("7b0c2a55-0000-4000-8000-00000000a000");
            Guid Id(string key) { using var sha = System.Security.Cryptography.SHA256.Create(); return new Guid(sha.ComputeHash(Encoding.UTF8.GetBytes(owner + key)).Take(16).ToArray()); }
            var load = new LoadCaseBase(Id("case"), "FF");
            var steel = new GPC.Model.Materials.Material("Steel", 210000, 0.3, 7.849e-9, 1.17e-5);
            double r = 5, area = Math.PI * r * r, j = Math.PI * Math.Pow(r, 4) / 4;
            var outline = new Shape2d(new Polygon2d(Enumerable.Range(0, 64).Select(i => new Point2d(r * Math.Cos(2 * Math.PI * i / 64), r * Math.Sin(2 * Math.PI * i / 64)))));
            var section = new GPC.Model.Sections.Section(outline, area, j, j, 0, j, j, 2 * j, new Point2d(0, 0), 0, j / r, j / r, j / r, j / r, "D10");
            var property = new ElasticBeamProperty("Cable", steel, section);
            var fabric = new MembranePlateProperty("Fabric") { MassPerArea = 1.05e-9 };
            var nodes = new NodeElement[k, k]; var elements = new List<Element>();
            for (int y = 0; y < k; y++)
                for (int x = 0; x < k; x++)
                {
                    bool edge = x == 0 || y == 0 || x == k - 1 || y == k - 1;
                    var node = nodes[x, y] = new NodeElement(Id("node:" + x + ":" + y), new Point3d(1000.0 * x + 0.1 * Math.Sin(y), 1000.0 * y + 0.1 * Math.Cos(x), edge ? 200 * Math.Sin(0.3 * x) : 0.01 * Math.Sin(x + y)));
                    elements.Add(node);
                    if (edge) node.Assignments.Restrains.Add(new RestrainAssignment { Restrain = new NodeRestrain(node, new List<DofRestrain> {
                        new DofRestrain(GeometryRestrain.DOF.DX, true), new DofRestrain(GeometryRestrain.DOF.DY, true), new DofRestrain(GeometryRestrain.DOF.DZ, true) }) });
                }
            void Beam(NodeElement a, NodeElement c, string key)
            {
                var beam = new BeamElement(Id(key), a.Position, c.Position, property);
                beam.ConnectNodes(a, c);
                beam.Assignments.Formulation = BeamFormulation.Cable;
                beam.Attributes.Add(new FormFindingGoalAttribute(new FormFindingGoal { Kind = FormFindingGoalKind.ForceDensity, Target = 1, Weight = 1 }, load));
                elements.Add(beam);
            }
            void Triangle(NodeElement a, NodeElement c, NodeElement d, string key)
            {
                var shell = new AreaElement(Id(key), null, fabric);
                shell.ConnectNodes(a, c, d);
                elements.Add(shell);
            }
            for (int y = 0; y < k; y++)
                for (int x = 0; x < k; x++)
                {
                    if (x > 0) Beam(nodes[x - 1, y], nodes[x, y], "h:" + x + ":" + y);
                    if (y > 0) Beam(nodes[x, y - 1], nodes[x, y], "v:" + x + ":" + y);
                    if (x > 0 && y > 0)
                    {
                        Beam(nodes[x - 1, y - 1], nodes[x, y], "d:" + x + ":" + y);
                        Triangle(nodes[x - 1, y - 1], nodes[x, y - 1], nodes[x, y], "t1:" + x + ":" + y);
                        Triangle(nodes[x - 1, y - 1], nodes[x, y], nodes[x - 1, y], "t2:" + x + ":" + y);
                    }
                }
            var model = ModelAssembly.Assemble(elements, owner, "Scale");
            if (!model.LoadCases.ContainsKey(load.Name)) model.LoadCases.Add(load);
            if (!results) return model;
            string input = model.AnalysisFingerprint();
            var cases = model.LoadCases.Values.ToArray();
            foreach (var node in model.NodesElements.Values)
                node.AddResult(new NodeResult(cases.Select(c => (INodeResultLocation)new NodeResultDisplacement(c,
                    new ResultDisplacement(node.Position.X * 1e-4, node.Position.Y * 1e-4, Math.Sin(node.Id), 0, 0, 0))
                { State = new ResultState { DatasetId = "form-finding", InputFingerprint = input, Step = c.Name } }).ToList()));
            model.CompactResults();
            return model;
        }
    }
}
