using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Core.Identity;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;
using GPC.Model.Results.Storage;
using GPC.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class ExternalArchiveTest
    {
        [DataContract]
        public sealed class ForeignWrapper
        {
            [DataMember] public double[] Values = Array.Empty<double>();
        }

        [TestMethod]
        public void SharedXmlBoundaryPreservesForeignWrapperDoubleBits()
        {
            var source = new ForeignWrapper { Values = new[] { BitConverter.Int64BitsToDouble(long.MinValue),
                double.Epsilon, double.MaxValue, 1.0000000000000002, -3.141592653589793, double.PositiveInfinity } };
            var serializer = new DataContractSerializer(typeof(ForeignWrapper));
            using var stream = new MemoryStream();
            using (var writer = System.Xml.XmlDictionaryWriter.CreateBinaryWriter(stream, null, null, false))
                ContractXml.Write(serializer, writer, source);
            stream.Position = 0;
            using var reader = System.Xml.XmlDictionaryReader.CreateBinaryReader(stream, System.Xml.XmlDictionaryReaderQuotas.Max);
            var restored = (ForeignWrapper)ContractXml.Read(serializer, reader);
            CollectionAssert.AreEqual(source.Values.Select(BitConverter.DoubleToInt64Bits).ToArray(),
                restored.Values.Select(BitConverter.DoubleToInt64Bits).ToArray());
        }

        private string folder = "";
        [TestInitialize] public void Start() { folder = Path.Combine(Path.GetTempPath(), "gpc-external-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder); }
        [TestCleanup] public void End() { Directory.Delete(folder, true); }
        private string FileAt(string name) => Path.Combine(folder, name);
        private static ArchiveBlock Block(string text, bool compress = true) => ArchiveBlock.Create(7, Encoding.UTF8.GetBytes(text), compress);
        private static string Text(BlockArchive store, string address)
        { var item = store.Read(new[] { address })[0]; Assert.IsNull(item.Failure, item.Failure); return Encoding.UTF8.GetString(item.Bytes!); }
        private static string[] Addresses(byte[] archive)
        {
            int count = BitConverter.ToInt32(archive, 20); long index = BitConverter.ToInt64(archive, 24);
            return Enumerable.Range(0, count).Select(i => BitConverter.ToString(archive, (int)index + 57 * i, 32).Replace("-", "")).ToArray();
        }

        [TestMethod]
        public void HostSnapshotsNeverPruneTheLastCommittedDocument()
        {
            string path = FileAt("host.gpc-results");
            var store = new BlockArchive(); var first = Block("last successfully saved document");
            store.Add(first); store.Save(path, new[] { first.Address }, () => { });
            for (int i = 0; i < 4; i++)
            {
                var candidate = Block("uncommitted host document " + i); store.Add(candidate);
                store.SaveSnapshot(path, new[] { candidate.Address }, requireComplete: true);
                Assert.AreEqual("last successfully saved document", Text(new BlockArchive(path), first.Address));
            }
            byte[] before = File.ReadAllBytes(path); var stamp = File.GetLastWriteTimeUtc(path);
            store.SaveSnapshot(path, Array.Empty<string>(), requireComplete: true);
            CollectionAssert.AreEqual(before, File.ReadAllBytes(path));
            Assert.AreEqual(stamp, File.GetLastWriteTimeUtc(path));
            var reopened = new BlockArchive(path);
            reopened.SaveSnapshot(path, Array.Empty<string>(), requireComplete: true);
            Assert.AreEqual("last successfully saved document", Text(reopened, first.Address));
        }

        [TestMethod]
        public void CapturedAntheaArchiveReadsEveryBlockAndRewritesIdentically()
        {
            string fixture = Path.Combine(AppContext.BaseDirectory, "Fixtures", "anthea-blocks-v1-20261103.bin");
            byte[] bytes = File.ReadAllBytes(fixture); string[] addresses = Addresses(bytes);
            var store = new BlockArchive(fixture, addresses, "ANTHEA.RISULTATI");
            var content = store.Read(addresses); Assert.IsTrue(content.Length > 10);
            // Independent legacy layout decoding; no expected data created with the extracted writer.
            int index = (int)BitConverter.ToInt64(bytes, 24);
            for (int i = 0; i < addresses.Length; i++)
            {
                int row = index + i * 57, offset = (int)BitConverter.ToInt64(bytes, row + 33), stored = (int)BitConverter.ToInt64(bytes, row + 41);
                using var raw = new MemoryStream(bytes, offset, stored, false);
                using var inflate = new DeflateStream(raw, CompressionMode.Decompress);
                using var expected = new MemoryStream(); inflate.CopyTo(expected);
                Assert.IsNull(content[i].Failure, content[i].Failure);
                CollectionAssert.AreEqual(expected.ToArray(), content[i].Bytes);
                Assert.AreEqual(addresses[i], ArchiveBlock.Digest(content[i].Bytes!));
            }
            string copy = FileAt("copy.anthea-risultati");
            Assert.AreEqual(0, store.Save(copy, addresses, () => { }));
            CollectionAssert.AreEqual(bytes, File.ReadAllBytes(copy), "Compressed payload and wire layout unchanged across runtimes.");
        }

        [TestMethod]
        public void UnsavedBlocksAreDetachedDeduplicatedAndReopenInBothEncodings()
        {
            byte[] input = Encoding.UTF8.GetBytes("hello"); var deflated = ArchiveBlock.Create(1, input); var raw = Block("raw", false);
            input[0] = 99; var store = new BlockArchive(); store.Add(deflated); store.Add(deflated); store.Add(raw);
            Assert.AreEqual("hello", Text(store, deflated.Address)); Assert.AreEqual("raw", Text(store, raw.Address));
            byte[] output = store.Read(new[] { deflated.Address })[0].Bytes!; output[0] = 98;
            Assert.AreEqual("hello", Text(store, deflated.Address));
            string path = FileAt("a.bin"); store.Save(path, new[] { raw.Address, deflated.Address }, () => { });
            Assert.AreEqual(2, Addresses(File.ReadAllBytes(path)).Length);
            Assert.AreEqual("raw", Text(new BlockArchive(path), raw.Address)); Assert.AreEqual("hello", Text(new BlockArchive(path), deflated.Address));
        }

        [TestMethod]
        public void UnchangedSaveDoesNotRewritePayloadOrTimestamp()
        {
            var block = Block(new string('x', 20000)); var store = new BlockArchive(); store.Add(block);
            string path = FileAt("a.bin"); store.Save(path, new[] { block.Address }, () => { });
            var before = File.ReadAllBytes(path); var oldTime = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc); File.SetLastWriteTimeUtc(path, oldTime);
            new BlockArchive(path, new[] { block.Address }).Save(path, new[] { block.Address }, () => { });
            Assert.AreEqual(oldTime, File.GetLastWriteTimeUtc(path)); CollectionAssert.AreEqual(before, File.ReadAllBytes(path));
        }

        [TestMethod]
        public void InterruptedSavePreservesPriorDocumentAndRetiredBlocksSupportUndo()
        {
            ArchiveBlock a = Block("old"), b = Block("new"); var store = new BlockArchive(); store.Add(a); store.Add(b);
            string path = FileAt("a.bin"), doc = FileAt("a.doc");
            store.Save(path, new[] { a.Address }, () => File.WriteAllText(doc, a.Address));
            Assert.ThrowsException<IOException>(() => store.Save(path, new[] { b.Address }, () => BlockArchive.WriteFile(doc, s => { s.WriteByte(1); throw new IOException("interrupted"); })));
            Assert.AreEqual(a.Address, File.ReadAllText(doc)); Assert.AreEqual("old", Text(new BlockArchive(path), a.Address));
            store.Save(path, new[] { b.Address }, () => File.WriteAllText(doc, b.Address));
            store.Save(path, new[] { b.Address }, () => { }); // old-only blocks are now retired
            Assert.AreEqual(1, Addresses(File.ReadAllBytes(path)).Length); Assert.AreEqual("old", Text(store, a.Address));
            store.Save(path, new[] { a.Address }, () => { }); Assert.AreEqual("old", Text(new BlockArchive(path), a.Address));
            Assert.AreEqual(0, Directory.GetFiles(folder, "*.tmp").Length);
        }

        [TestMethod]
        public void SaveAsFailurePreservesExistingDestinationAndSessionSource()
        {
            ArchiveBlock a = Block("a"), b = Block("b"); string first = FileAt("a.bin"), second = FileAt("b.bin");
            var store = new BlockArchive(); store.Add(a); store.Save(first, new[] { a.Address }, () => { });
            var prior = new BlockArchive(); prior.Add(b); prior.Save(second, new[] { b.Address }, () => { });
            byte[] source = File.ReadAllBytes(first);
            Assert.ThrowsException<IOException>(() => store.Save(second, new[] { a.Address }, () => throw new IOException()));
            Assert.AreEqual(first, store.FilePath); Assert.AreEqual("b", Text(new BlockArchive(second), b.Address));
            CollectionAssert.AreEqual(source, File.ReadAllBytes(first));
            store.Save(second, new[] { a.Address }, () => { }); Assert.AreEqual(second, store.FilePath);
            store.Save(second, new[] { a.Address }, () => { }); Assert.AreEqual(1, Addresses(File.ReadAllBytes(second)).Length);
        }

        [TestMethod]
        public void FirstSaveWithoutPriorDocumentReferencesRetainsItsWholeArchiveOnFailure()
        {
            var a = Block("old"); var b = Block("new"); string path = FileAt("a.bin");
            var first = new BlockArchive(); first.Add(a); first.Save(path, new[] { a.Address }, () => { });
            var session = new BlockArchive(path); session.Add(b);
            Assert.ThrowsException<IOException>(() => session.Save(path, new[] { b.Address }, () => throw new IOException()));
            Assert.AreEqual("old", Text(new BlockArchive(path), a.Address));
            session.Save(path, new[] { b.Address }, () => { }); session.Save(path, new[] { b.Address }, () => { });
            Assert.AreEqual(1, Addresses(File.ReadAllBytes(path)).Length);
        }

        [TestMethod]
        public void MissingCorruptAndFutureArchivesAreExplicitAndNeverSilentlyReplaced()
        {
            var a = Block("alpha"); string path = FileAt("a.bin"); var store = new BlockArchive(); store.Add(a); store.Save(path, new[] { a.Address }, () => { });
            var original = File.ReadAllBytes(path); var broken = (byte[])original.Clone(); broken[64] ^= 0xFF; File.WriteAllBytes(path, broken);
            Assert.IsNotNull(new BlockArchive(path).Read(new[] { a.Address })[0].Failure);
            // Review D2 (3/11/2026): an archive that is not readable as one is renamed aside, byte for byte, and the document is saved.
            broken = (byte[])original.Clone(); broken[16] = 2; File.WriteAllBytes(path, broken);
            var future = new BlockArchive(path, new[] { a.Address }); Assert.IsNotNull(future.Read(new[] { a.Address })[0].Failure);
            bool documentWritten = false; Assert.AreEqual(1, future.Save(path, new[] { a.Address }, () => documentWritten = true));
            Assert.IsTrue(documentWritten); Assert.IsFalse(File.Exists(path));
            string aside = future.LastSave!.SetAside!; StringAssert.StartsWith(aside, path + ".danneggiato-"); CollectionAssert.AreEqual(broken, File.ReadAllBytes(aside));
            File.Delete(aside); var missing = new BlockArchive(path, new[] { a.Address }); documentWritten = false;
            StringAssert.Contains(missing.Read(new[] { a.Address })[0].Failure!, "mancante");
            Assert.ThrowsException<InvalidDataException>(() => missing.Save(path, new[] { a.Address }, () => documentWritten = true, requireComplete: true));
            Assert.IsFalse(documentWritten);
            Assert.AreEqual(1, missing.Save(path, new[] { a.Address }, () => { })); Assert.IsFalse(File.Exists(path));
        }

        [TestMethod]
        public void InvalidIndexEntriesAndDuplicateAddressesAffectOnlyTheirBlockEvenWithCorrectIndexHash()
        {
            ArchiveBlock a = Block("a"), b = Block("b"); string path = FileAt("a.bin"); var store = new BlockArchive(); store.Add(a); store.Add(b);
            store.Save(path, new[] { a.Address, b.Address }, () => { }); byte[] original = File.ReadAllBytes(path);
            string first = Addresses(original)[0], second = Addresses(original)[1];
            foreach (int variant in new[] { 0, 1, 2, 3 })
            {
                var bytes = (byte[])original.Clone(); int index = (int)BitConverter.ToInt64(bytes, 24);
                if (variant == 0) Buffer.BlockCopy(BitConverter.GetBytes(long.MaxValue), 0, bytes, index + 33, 8);
                if (variant == 1) Buffer.BlockCopy(bytes, index, bytes, index + 57, 32);
                if (variant == 2) Buffer.BlockCopy(BitConverter.GetBytes((long)ArchiveBlock.MaximumBytes + 1), 0, bytes, index + 49, 8);
                if (variant == 3) Buffer.BlockCopy(bytes, index + 33, bytes, index + 57 + 33, 8);
                using var sha = System.Security.Cryptography.SHA256.Create(); var digest = sha.ComputeHash(bytes, index, bytes.Length - index); Buffer.BlockCopy(digest, 0, bytes, 32, 32);
                File.WriteAllBytes(path, bytes);
                // Variants 0 and 2 damage the first entry, 1 and 3 the second: only that block is unavailable.
                string lost = variant == 0 || variant == 2 ? first : second, kept = lost == first ? second : first;
                var reader = new BlockArchive(path, new[] { a.Address, b.Address }); var content = reader.Read(new[] { lost, kept });
                Assert.IsNotNull(content[0].Failure, "variant " + variant); Assert.IsNull(content[1].Failure, "variant " + variant);
                string copy = FileAt("copy" + variant + ".bin");
                Assert.AreEqual(1, reader.Save(copy, new[] { a.Address, b.Address }, () => { }), "variant " + variant);
                Assert.IsNull(new BlockArchive(copy).Read(new[] { kept })[0].Failure, "variant " + variant);
            }
        }

        [TestMethod]
        public void InvalidReferenceStringsAreSingleUnavailableBlocks()
        {
            // Review D2: a malformed reference in a document made the whole project impossible to open or save.
            var a = Block("a"); string path = FileAt("a.bin"); var store = new BlockArchive(); store.Add(a); store.Save(path, new[] { a.Address }, () => { });
            var session = new BlockArchive(path, new[] { a.Address, "not-a-block", "" });
            var content = session.Read(new[] { "not-a-block", a.Address });
            StringAssert.Contains(content[0].Failure!, "non valido"); Assert.IsNull(content[1].Failure);
            bool written = false; Assert.AreEqual(1, session.Save(path, new[] { a.Address, "not-a-block" }, () => written = true));
            Assert.IsTrue(written); Assert.AreEqual("a", Text(new BlockArchive(path), a.Address));
        }

        [TestMethod]
        public void CopiedForwardBlocksAreVerifiedAndDamagedOnesDoNotStopTheSave()
        {
            ArchiveBlock a = Block("first block"), b = Block("second block"), c = Block("third"); string path = FileAt("a.bin");
            var store = new BlockArchive(); store.Add(a); store.Add(b); store.Save(path, new[] { a.Address, b.Address }, () => { });
            byte[] bytes = File.ReadAllBytes(path); int row = (int)BitConverter.ToInt64(bytes, 24) + Array.IndexOf(Addresses(bytes), b.Address) * 57;
            bytes[(int)BitConverter.ToInt64(bytes, row + 33)] ^= 0x40; File.WriteAllBytes(path, bytes);
            var session = new BlockArchive(path, new[] { a.Address, b.Address }); session.Add(c);
            Assert.AreEqual(1, session.Save(path, new[] { a.Address, b.Address, c.Address }, () => { }));
            CollectionAssert.AreEqual(new[] { b.Address }, session.LastSave!.Damaged.ToArray());
            CollectionAssert.AreEquivalent(new[] { a.Address, c.Address }, Addresses(File.ReadAllBytes(path)));
            Assert.AreEqual("first block", Text(new BlockArchive(path), a.Address));
        }

        [TestMethod]
        public void PermanentlyMissingBlocksDoNotRewriteTheArchiveAtEverySave()
        {
            var a = Block("a"); string path = FileAt("a.bin"), absent = Block("never stored").Address;
            var store = new BlockArchive(); store.Add(a); store.Save(path, new[] { a.Address, absent }, () => { });
            var time = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc); File.SetLastWriteTimeUtc(path, time);
            Assert.AreEqual(1, store.Save(path, new[] { a.Address, absent }, () => { }));
            Assert.AreEqual(time, File.GetLastWriteTimeUtc(path)); Assert.IsFalse(store.LastSave!.Written);
        }

        [TestMethod]
        public void SaveNeverThrowsAfterTheDocumentIsWritten()
        {
            // An emptied document deletes its archive after the document is written; a failure there must not report a failed save.
            var a = Block("a"); string path = FileAt("a.bin"); var store = new BlockArchive(); store.Add(a); store.Save(path, new[] { a.Address }, () => { });
            store.Save(path, Array.Empty<string>(), () => { }); // the previous document still cited the block: kept once more
            bool written = false;
            using (new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) Assert.AreEqual(0, store.Save(path, Array.Empty<string>(), () => written = true));
            Assert.IsTrue(written); Assert.AreEqual("a", Text(store, a.Address));
        }

        [TestMethod]
        public void MissingSidecarNeverStopsTheSessionOrTheSaveOnlyItsRuns()
        {
            // Review D2: the constructor and Save threw for a missing sidecar, so the whole document could not be saved or autosaved.
            var model = ModelWithResults(); var store = new ExternalModelArchive(); var reference = store.Prepare(model);
            string doc = FileAt("project.gh"), sidecar = ExternalModelArchive.PathFor(doc);
            var live = store.Save(sidecar, new[] { reference }, r => BlockArchive.WriteFile(doc, s => r[0].Write(s)));
            File.Delete(sidecar);
            var session = new ExternalModelArchive(sidecar, live); bool written = false;
            live = session.Save(sidecar, live, r => { written = true; BlockArchive.WriteFile(doc, s => r[0].Write(s)); });
            Assert.IsTrue(written); Assert.IsTrue(session.LastSave.Unavailable > 0);
            Assert.ThrowsException<InvalidDataException>(() => session.Open(live[0]));
        }

        [TestMethod]
        public void AutosaveDoesNotReadTheManifestsAgain()
        {
            // Every save used to read, inflate, hash and parse the manifest of every live and retained run; now each is read once.
            var model = ModelWithResults(); var store = new ExternalModelArchive(); var reference = store.Prepare(model);
            string doc = FileAt("project.gh"), sidecar = ExternalModelArchive.PathFor(doc);
            var live = store.Save(sidecar, new[] { reference }, r => BlockArchive.WriteFile(doc, s => r[0].Write(s)));
            byte[] bytes = File.ReadAllBytes(sidecar); int row = (int)BitConverter.ToInt64(bytes, 24) + Array.IndexOf(Addresses(bytes), live[0].ManifestAddress) * 57;
            bytes[(int)BitConverter.ToInt64(bytes, row + 33) + 1] ^= 0x40; File.WriteAllBytes(sidecar, bytes);
            var time = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc); File.SetLastWriteTimeUtc(sidecar, time);
            for (int i = 0; i < 3; i++) live = store.Save(sidecar, live, r => BlockArchive.WriteFile(doc, s => r[0].Write(s)));
            Assert.AreEqual(time, File.GetLastWriteTimeUtc(sidecar));
            Assert.AreEqual(1, store.BlocksForDataset(live[0], "run-1").Count);
        }

        [TestMethod]
        public void AnotherWriterOfTheSameArchiveNeverLosesItsBlocks()
        {
            // Two sessions (two processes) opened on the same document. The second one's save fails after its archive: the document on disk
            // is the first one's, so every block the first one wrote must still be there.
            var x = Block("x"); string path = FileAt("a.bin"), doc = FileAt("a.doc");
            var origin = new BlockArchive(); origin.Add(x); origin.Save(path, new[] { x.Address }, () => File.WriteAllText(doc, x.Address));
            var one = new BlockArchive(path, new[] { x.Address }); var two = new BlockArchive(path, new[] { x.Address });
            ArchiveBlock a = Block("from one"), b = Block("from two"); one.Add(a); two.Add(b);
            one.Save(path, new[] { a.Address }, () => File.WriteAllText(doc, a.Address));
            Assert.ThrowsException<IOException>(() => two.Save(path, new[] { b.Address }, () => throw new IOException("document not written")));
            Assert.AreEqual("from one", Text(new BlockArchive(path), File.ReadAllText(doc)));
        }

        [TestMethod]
        public void LockedArchiveFailsTheSaveBeforeTheDocumentAndOrphanTemporariesAreRemoved()
        {
            var a = Block("a"); string path = FileAt("a.bin"); var store = new BlockArchive { LockTimeout = TimeSpan.FromMilliseconds(200) }; store.Add(a);
            string orphan = path + "." + Guid.NewGuid().ToString("N") + ".tmp", fresh = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            File.WriteAllText(orphan, "left by a process that ended"); File.SetLastWriteTimeUtc(orphan, DateTime.UtcNow.AddHours(-1)); File.WriteAllText(fresh, "being written");
            bool written = false;
            using (new FileStream(path + ".lock", FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose))
                Assert.ThrowsException<IOException>(() => store.Save(path, new[] { a.Address }, () => written = true));
            Assert.IsFalse(written); Assert.IsFalse(File.Exists(path)); Assert.IsTrue(File.Exists(orphan));
            store.Save(path, new[] { a.Address }, () => written = true);
            Assert.IsTrue(written); Assert.IsFalse(File.Exists(orphan)); Assert.IsTrue(File.Exists(fresh)); Assert.IsFalse(File.Exists(path + ".lock"));
        }

        [TestMethod]
        public void SaveAsOverADamagedDestinationKeepsItAsideAndSaves()
        {
            var a = Block("a"); string first = FileAt("a.bin"), second = FileAt("b.bin");
            var store = new BlockArchive(); store.Add(a); store.Save(first, new[] { a.Address }, () => { });
            File.WriteAllBytes(second, Encoding.ASCII.GetBytes("not an archive"));
            bool written = false; Assert.AreEqual(0, store.Save(second, new[] { a.Address }, () => written = true));
            Assert.IsTrue(written); Assert.AreEqual("a", Text(new BlockArchive(second), a.Address));
            Assert.AreEqual("not an archive", File.ReadAllText(store.LastSave!.SetAside!));
        }

        private static Model ModelWithResults()
        {
            var model = ModelAssembly.Assemble(new[] { new NodeElement(new Point3d(1, 2, 3)), new NodeElement(new Point3d(4, 5, 6)) }, Guid.NewGuid());
            var load = new LoadCaseBase("G"); model.LoadCases.Add(load);
            var sample = new NodeResultDisplacement(load, new ResultDisplacement(1.0 / 3, BitConverter.Int64BitsToDouble(long.MinValue), double.Epsilon, 0, 0, 0)) { State = new ResultState { DatasetId = "run-1" } };
            model.NodesElements[1].AddResult(new NodeResult(new List<INodeResultLocation> { sample }));
            model.CompactResults(); return model;
        }
        private static string Content(Model model)
        {
            // Model/Element value fingerprints intentionally carry identity, not the full graph.
            // Compare engineering inputs plus the actual compact results section, including every double bit.
            using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0;
            using var zip = new ZipArchive(stream, ZipArchiveMode.Read); using var results = zip.GetEntry("results.bin")!.Open();
            using var bytes = new MemoryStream(); results.CopyTo(bytes);
            return model.AnalysisFingerprint() + ":" + ArchiveBlock.Digest(bytes.ToArray()) + ":" + ModelValues.Fingerprint(new object[] { model.AssemblyMap, model.Name });
        }

        [TestMethod]
        public void ModelDocumentAndZipRetainF2AssemblyMap()
        {
            var model = ModelWithResults(); string map = ModelValues.Fingerprint(new object[] { model.AssemblyMap });
            using var zip = new MemoryStream(); ModelArchive.Save(model, zip); Assert.AreEqual((byte)'P', zip.ToArray()[0]); zip.Position = 0;
            var copy = ModelArchive.Load(zip); Assert.AreEqual(map, ModelValues.Fingerprint(new object[] { copy.AssemblyMap })); Assert.AreEqual(Content(model), Content(copy));
            using var xml = new MemoryStream(); ModelArchive.SaveDocument(model, xml); xml.Position = 0;
            Assert.AreEqual(map, ModelValues.Fingerprint(new object[] { ModelArchive.Load(xml).AssemblyMap }));
        }

        [TestMethod]
        public void ExternalRunIsExactDetachedAndKeepsSolverMapAndDatasetIndex()
        {
            var model = ModelWithResults(); var map = model.AssemblyMap.ToSolver(model, "test", model.AllElements.Select(e => new KeyValuePair<Guid, int>(e.Guid, e.Id + 100)));
            var store = new ExternalModelArchive(); var reference = store.Prepare(model, map); var copy = store.Open(reference);
            Assert.AreEqual(Content(model), Content(copy.Model)); Assert.AreEqual(ModelValues.Fingerprint(new object[] { map }), ModelValues.Fingerprint(new object[] { copy.SolverMap }));
            Assert.AreEqual(1, store.BlocksForDataset(reference, "run-1").Count); Assert.AreEqual(0, store.BlocksForDataset(reference, "absent").Count);
            Assert.IsTrue(reference.IsCurrent(model)); copy.Model.NodesElements[1].Position.X = 100; Assert.IsFalse(reference.IsCurrent(copy.Model));
            Assert.AreNotEqual(100.0, model.NodesElements[1].Position.X);
            using var small = new MemoryStream(); reference.Write(small); Assert.IsTrue(small.Length < 1200); small.Position = 0;
            Assert.AreEqual(reference.ManifestAddress, ExternalModelReference.Read(small).ManifestAddress);
        }

        [TestMethod]
        public void PortableReferenceSupportsSaveAsTransferHistoryAndUnchangedAutosave()
        {
            var model = ModelWithResults(); var store = new ExternalModelArchive(); var first = store.Prepare(model);
            string doc = FileAt("project.gh"), sidecar = ExternalModelArchive.PathFor(doc);
            var live = store.Save(sidecar, new[] { first }, r => BlockArchive.WriteFile(doc, s => r[0].Write(s)));
            DateTime time = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc); File.SetLastWriteTimeUtc(sidecar, time);
            store.Save(sidecar, live, r => BlockArchive.WriteFile(doc, s => r[0].Write(s))); Assert.AreEqual(time, File.GetLastWriteTimeUtc(sidecar));
            model.NodesElements[1].Position.X += 1; var second = store.Prepare(model);
            live = store.Save(sidecar, new[] { second, first }, r => BlockArchive.WriteFile(doc, s => r[0].Write(s)));
            var reopened = new ExternalModelArchive(sidecar, live); Assert.AreEqual(Content(model), Content(reopened.Open(live[0]).Model)); Assert.IsFalse(live[1].IsCurrent(model));
            string copyDoc = FileAt("copy.gh"); live = reopened.Save(ExternalModelArchive.PathFor(copyDoc), live, r => BlockArchive.WriteFile(copyDoc, s => r[0].Write(s)));
            string transfer = FileAt("transfer"); Directory.CreateDirectory(transfer); File.Copy(copyDoc, Path.Combine(transfer, "copy.gh")); File.Copy(ExternalModelArchive.PathFor(copyDoc), Path.Combine(transfer, live[0].ArchiveFile));
            Assert.AreEqual(Content(model), Content(ExternalModelArchive.Open(Path.Combine(transfer, "copy.gh"), live[0]).Model));
            Assert.IsFalse(live[1].IsCurrent(model)); Assert.AreNotEqual(Content(model), Content(reopened.Open(live[1]).Model));
        }

        [TestMethod]
        public void UnavailableExternalResultCannotMasqueradeAsCompleteModel()
        {
            var model = ModelWithResults(); var store = new ExternalModelArchive(); var reference = store.Prepare(model);
            string path = FileAt("a.gpc-results"); reference = store.Save(path, new[] { reference }, _ => { })[0];
            byte[] bytes = File.ReadAllBytes(path); string address = store.BlocksForDataset(reference, "run-1")[0];
            int row = (int)BitConverter.ToInt64(bytes, 24) + Array.IndexOf(Addresses(bytes), address) * 57;
            int offset = (int)BitConverter.ToInt64(bytes, row + 33); bytes[offset] ^= 0x40; File.WriteAllBytes(path, bytes);
            Assert.ThrowsException<InvalidDataException>(() => new ExternalModelArchive(path).Open(reference));
            File.Delete(path); Assert.ThrowsException<InvalidDataException>(() => ExternalModelArchive.Open(FileAt("a.gh"), reference));
        }

        [TestMethod]
        public void ExternalReferenceRejectsFutureVersionAndEscapingPath()
        {
            var store = new ExternalModelArchive(); var reference = store.Prepare(ModelWithResults());
            using var stream = new MemoryStream(); reference.Write(stream); string xml = Encoding.UTF8.GetString(stream.ToArray());
            foreach (string bad in new[] { xml.Replace("<Version>1</Version>", "<Version>2</Version>"), xml.Replace("unsaved.gpc-results", "../outside.gpc-results") })
            { using var input = new MemoryStream(Encoding.UTF8.GetBytes(bad)); Assert.ThrowsException<SerializationException>(() => ExternalModelReference.Read(input)); }
        }

        [TestMethod]
        public void ExistingGpcArchivesCanBeExternalizedWithoutResultOrInputChanges()
        {
            using var input = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "model-v5-5.23.0.2.zip"));
            var model = ModelArchive.Load(input); var store = new ExternalModelArchive(); var reference = store.Prepare(model);
            Assert.AreEqual(Content(model), Content(store.Open(reference).Model));
        }
    }
}
