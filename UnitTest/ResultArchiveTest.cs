using System.IO.Compression;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Examples;
using GPC.Model.Analysis;
using GPC.Model.Checking.Preparation;
using GPC.Model.Checking.Scenarios;
using GPC.Model.Core;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using GPC.Model.Results.Processing;
using GPC.Model.Results.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>Compact results outside the graph (revision of 10/10/2026, point R1): same values, identities, cases and derivations as
/// the XML serializer; copies, preparations and archives share the compact results instead of copying the samples.</summary>
[TestClass]
public class ResultArchiveTest
{
    // Imported static results plus the combinations rebuilt from them, whose states point to the source samples of the same element.
    private static Model WithDerived()
    {
        var m = CombinationRebuildTest.Imported();
        var plan = new ResultFilter { Combinations = new[] { "SLU_1", "ENV" } }.Resolve(m);
        foreach (var report in CombinationResults.Rebuild(m, plan, "static"))
            Assert.IsFalse(report.IsRejected, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        Assert.IsTrue(Samples(m.AllElements).Any(s => s.State?.DerivedFrom != null), "Derived samples expected.");
        return m;
    }
    private static IEnumerable<ResultLocation> Samples(IEnumerable<Element> elements) => elements.SelectMany(e => e.Results).SelectMany(r => r.Results);
    private static IEnumerable<ResultLocation> Samples(IEnumerable<ElementResult> results) => results.SelectMany(r => r.Results);
    private static string Values(IEnumerable<ElementResult> results) => ModelValues.Fingerprint(new object[] { results.ToArray() });
    private static Guid[] Identities(IEnumerable<ElementResult> results) => results.Select(r => r.Guid).Concat(Samples(results).Select(s => s.Guid)).ToArray();
    private static byte[] Entry(byte[] package, string name)
    {
        using var zip = new ZipArchive(new MemoryStream(package)); using var stream = zip.GetEntry(name)!.Open();
        using var copy = new MemoryStream(); stream.CopyTo(copy); return copy.ToArray();
    }
    private static bool OwnCase(Model m, ILoadCase c) => m.LoadCases.Values.Any(x => ReferenceEquals(x, c)) || m.Combinations.Values.Any(x => ReferenceEquals(x, c));
    private static void SameResults(Model expected, Model actual)
    {
        var left = expected.AllElements.ToArray(); var right = actual.AllElements.ToArray();
        Assert.AreEqual(left.Length, right.Length);
        for (int i = 0; i < left.Length; i++)
        {
            Assert.AreEqual(left[i].Guid, right[i].Guid);
            Assert.AreEqual(Values(left[i].Results), Values(right[i].Results), "Values of element " + left[i].Id);
            CollectionAssert.AreEqual(Identities(left[i].Results), Identities(right[i].Results));
            Assert.IsTrue(Samples(right[i].Results).All(s => OwnCase(actual, s.Case)), "Cases bound to the model that reads them.");
        }
    }

    [TestMethod]
    public void Codec_KeepsValuesIdentitiesCasesAndDerivations_AndEncodesAgainToTheSameBytes()
    {
        var m = WithDerived(); var cases = new ResultCaseTable(m);
        foreach (var e in m.AllElements.Where(e => e.Results.Count > 0))
        {
            var segment = ResultCodec.Encode(e.Results, 0, e.Results.Count, cases);
            var decoded = ResultCodec.Decode(segment, m);
            Assert.AreEqual(Values(e.Results), Values(decoded));
            CollectionAssert.AreEqual(Identities(e.Results), Identities(decoded));
            var samples = Samples(decoded).ToArray();
            Assert.IsTrue(samples.All(s => OwnCase(m, s.Case)));
            foreach (var s in samples.Where(s => s.State?.DerivedFrom != null))
                Assert.IsTrue(s.State.DerivedFrom.All(d => samples.Any(x => ReferenceEquals(x, d))), "Sources are samples of the same decoded list.");
            Assert.IsTrue(ResultCodec.Encode(decoded, 0, decoded.Count, cases).SameContent(segment), "Deterministic encoding.");
            Assert.IsTrue(segment.Payload.Length < segment.Length, "Deflated.");
        }
    }

    [TestMethod]
    public void Copy_SharesCompactResults_DecodesOnFirstAccess_AndKeepsTheDigest()
    {
        var m = WithDerived(); var digest = AnalysisStorage.Results(m);
        var copy = ModelValues.Copy(m);
        Assert.IsTrue(copy.AllElements.Where(e => e.HasResults).All(e => !e.ResultsLoaded), "Nothing decoded by the copy.");
        Assert.AreEqual(digest, AnalysisStorage.Results(copy), "Digest from the compact form.");
        SameResults(m, copy);
        Assert.AreEqual(digest, AnalysisStorage.Results(copy), "Same digest once read.");
        foreach (var e in copy.AllElements)
            foreach (var s in Samples(e.Results).Where(s => s.State?.DerivedFrom != null))
                Assert.IsTrue(ResultAlgebra.HasCurrentDerivation(copy, e, s.State), "Derivations survive the copy.");
        Assert.AreEqual(Values(m.BeamElements.Values.First().Results), Values(ModelValues.Copy(copy).BeamElements.Values.First().Results), "Copy of a copy.");
    }

    [TestMethod]
    public void ReadingResults_DoesNotMakePreparationsStale_ButChangingThemDoes()
    {
        var source = MixedModelFactory.Create();
        var prepared = VerificationPreparation.Prepare(source, VerificationScenario.Create(source.Analysis.Id, "Stable"));
        Assert.IsTrue(prepared.Model.AllElements.Where(e => e.HasResults).All(e => !e.ResultsLoaded));
        Assert.IsTrue(prepared.IsCurrent);
        var snapshot = VerificationSnapshot.Capture(prepared);
        foreach (var e in prepared.Model.AllElements) Assert.IsNotNull(e.Results);
        Assert.IsTrue(prepared.IsCurrent, "Reading is not a change.");
        Assert.IsTrue(snapshot.IsCurrent(prepared.Model));
        var sample = prepared.Model.BeamElements[250].Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().First();
        var n = sample.ResultBeamForces.N;
        sample.ResultBeamForces.N = n + 1;
        Assert.IsFalse(prepared.IsCurrent); Assert.IsFalse(snapshot.IsCurrent(prepared.Model));
        sample.ResultBeamForces.N = n;
        Assert.IsTrue(prepared.IsCurrent, "Same values, same digest.");
    }

    [TestMethod]
    public void CompactResults_ReleasesTheObjects_AndAddedResultsWaitWithoutDecoding()
    {
        var m = WithDerived(); var digest = AnalysisStorage.Results(m); var values = m.AllElements.Select(e => Values(e.Results)).ToArray();
        Assert.IsTrue(m.CompactResults() > 0);
        Assert.IsTrue(m.AllElements.Where(e => e.HasResults).All(e => !e.ResultsLoaded));
        Assert.AreEqual(digest, AnalysisStorage.Results(m));
        Assert.IsTrue(m.ContainsResultsOf("static")); Assert.IsFalse(m.ContainsResultsOf("other"));
        Assert.IsTrue(m.AllElements.Where(e => e.HasResults).All(e => !e.ResultsLoaded), "Queries do not decode.");
        CollectionAssert.AreEqual(values, m.AllElements.Select(e => Values(e.Results)).ToArray());

        // Results added to compact ones are kept after them, without decoding.
        var girder = m.BeamElements.Values.First(b => b.HasResults); int count = Samples(new[] { girder }).Count();
        var extra = ResultCodec.Decode(ResultCodec.Encode(girder.Results, 0, 1, new ResultCaseTable(m)), m)[0];
        m.CompactResults();
        girder.AddResult(extra);
        Assert.IsFalse(girder.ResultsLoaded);
        var withExtra = AnalysisStorage.Results(m);
        Assert.AreNotEqual(digest, withExtra);
        Assert.AreEqual(count + extra.Results.Count, Samples(new[] { girder }).Count());
        Assert.IsTrue(ReferenceEquals(extra, girder.Results.Last()));
        Assert.AreEqual(withExtra, AnalysisStorage.Results(m), "Decoded list encodes to the same segments.");
    }

    [TestMethod]
    public void Archive_WithResults_IsAPackage_WithoutResults_StaysXml()
    {
        var m = WithDerived();
        using var package = new MemoryStream(); ModelArchive.Save(m, package);
        var bytes = package.ToArray(); Assert.AreEqual((byte)'P', bytes[0]); Assert.AreEqual((byte)'K', bytes[1]);
        using (var zip = new ZipArchive(new MemoryStream(bytes)))
            CollectionAssert.AreEquivalent(new[] { "model.xml", "results.bin" }, zip.Entries.Select(e => e.FullName).ToArray());
        var loaded = ModelArchive.Load(new MemoryStream(bytes));
        Assert.IsTrue(loaded.AllElements.Where(e => e.HasResults).All(e => !e.ResultsLoaded));
        Assert.AreEqual(AnalysisStorage.Results(m), AnalysisStorage.Results(loaded));
        SameResults(m, loaded);
        Assert.AreEqual(m.AnalysisFingerprint(), loaded.AnalysisFingerprint());
        // The input graph regenerates the GUIDs of some geometries on reading (as the XML documents always did); the compact results do not change.
        using var again = new MemoryStream(); ModelArchive.Save(loaded, again);
        CollectionAssert.AreEqual(Entry(bytes, "results.bin"), Entry(again.ToArray(), "results.bin"), "Same compact results after a reload.");

        var inputs = CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(CombinationRebuildTest.Json()), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile()).Model;
        using var document = new MemoryStream(); ModelArchive.Save(inputs, document);
        Assert.AreEqual('<', System.Text.Encoding.UTF8.GetString(document.ToArray()).TrimStart('﻿')[0], "Models without results keep the XML document.");
    }

    [TestMethod]
    public void Archive_Mixed_PreparesTheSameVerification()
    {
        var source = MixedModelFactory.Create();
        using var stream = new MemoryStream(); ModelArchive.Save(source, stream); stream.Position = 0;
        var loaded = ModelArchive.Load(stream);
        SameResults(source, loaded);
        var prepared = VerificationPreparation.Prepare(loaded, VerificationScenario.Create(loaded.Analysis.Id, "Stable"));
        Assert.IsTrue(prepared.IsCurrent);
        Assert.AreEqual(Values(source.BeamElements[250].Results), Values(prepared.Model.BeamElements[250].Results));
    }

    [TestMethod]
    public void AnalysisSnapshot_IsACompactImage_AndOpensIndependentGraphs()
    {
        var source = MixedModelFactory.Create();
        var first = source.Analysis.OpenModel(); var second = source.Analysis.OpenModel();
        Assert.AreNotSame(first.NodesElements[40], second.NodesElements[40]);
        Assert.AreEqual(source.Analysis.InputFingerprint, first.AnalysisFingerprint());
        var field = typeof(AnalysisSnapshot).GetField("_model", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
        Assert.IsTrue(ModelImage.IsImage((byte[])field.GetValue(source.Analysis)!));
    }

    [TestMethod]
    public void ValidatedSnapshots_AreNotReopened_ButAChangedDeclarationIsStillRejected()
    {
        var source = MixedModelFactory.Create();
        ModelValues.Copy(source); // validated once in this process
        using var document = new MemoryStream(); ModelArchive.SaveDocument(source, document);
        var xml = System.Xml.Linq.XDocument.Parse(System.Text.Encoding.UTF8.GetString(document.ToArray()).TrimStart('﻿'));
        var declared = xml.Descendants().Single(e => e.Name.LocalName == "_x003C_InputFingerprint_x003E_k__BackingField" && e.Parent!.Name.LocalName == "Analysis");
        Assert.IsNotNull(ModelArchive.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml.ToString()))).Analysis);
        declared.Value = "changed";
        Assert.ThrowsException<System.Runtime.Serialization.SerializationException>(() => ModelArchive.Load(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(xml.ToString()))));
    }

    [TestMethod]
    public void ForeignSampleReference_IsRejected_NotCopiedSilently()
    {
        var m = WithDerived();
        var owners = m.AllElements.Where(e => e.Results.Count > 0).Take(2).ToArray();
        var derived = Samples(owners[0].Results).First();
        derived.State.DerivedFrom = new[] { Samples(owners[1].Results).First() };
        var error = Assert.ThrowsException<ResultEncodingException>(() => ModelValues.Copy(m));
        Assert.IsTrue(error.Foreign);
    }

    [TestMethod]
    public void CorruptSegment_IsRejected()
    {
        var m = WithDerived(); var e = m.AllElements.First(x => x.Results.Count > 0);
        var segment = ResultCodec.Encode(e.Results, 0, e.Results.Count, new ResultCaseTable(m));
        var payload = (byte[])segment.Payload.Clone(); payload[payload.Length / 2] ^= 0x5A;
        var corrupt = new ResultSegment(payload, segment.Length, segment.Count, segment.Digest, segment.Datasets);
        Assert.ThrowsException<System.Runtime.Serialization.SerializationException>(() => ResultCodec.Decode(corrupt, m));
    }
}
