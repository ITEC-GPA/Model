using System.IO.Compression;
using GPC.Examples;
using GPC.Model.Core;
using GPC.Model.Persistence;
using GPC.Model.Structure.Members;

namespace UnitTest;

/// <summary>Serviceability declarations of the physical members in the model archives. The fixtures hold one small model with two
/// physical members (B: two elements, the second reversed; C: one element, OffsetToOffset) saved by GPCModel 5.28.0.0 (master
/// 9f40d98a) before the declarations existed: version 2 by ModelArchive.Save, version 4 by SaveDocument.</summary>
[TestClass]
public class MemberServiceabilityArchiveTest
{
    private static byte[] Fixture(string form)
    {
        using var file = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "member-limits-legacy-5.28.0.0-" + form + ".xml.gz"));
        using var zip = new GZipStream(file, CompressionMode.Decompress); using var plain = new MemoryStream();
        zip.CopyTo(plain); return plain.ToArray();
    }
    private static string Fp(object value) => ModelValues.Fingerprint(new object[] { value });
    private static readonly MemberServiceabilityLimits Full = new(MemberDeflectionScheme.CantileverAtEnd, "Drawing S-12: clamped at the column, free at the span start",
        new MemberDeclaredValue(2, "Declared: limit of the cantilever referred to twice its length"), new MemberDeclaredValue(250, "Project agreement: cantilevers L/250"),
        new MemberDeclaredValue(-4.5, "Drawing S-12: camber 4.5 mm"), new MemberDeclaredValue(300, "Project agreement: variable actions L/300"),
        new MemberDeclaredValue(500, "Project agreement: h/500"), new MemberDeclaredValue(3, "Project agreement: 3 Hz"));

    [DataTestMethod] [DataRow("v2")] [DataRow("v4")]
    public void ArchivesWrittenBeforeTheDeclarationsLoadAndSaveUnchanged(string form)
    {
        var bytes = Fixture(form);
        var model = ModelArchive.Load(new MemoryStream(bytes));
        Assert.AreEqual(2, model.PhysicalMembers.Count);
        Assert.IsTrue(model.PhysicalMembers.Values.All(m => m.ServiceabilityLimits == null));
        // Fingerprints of the definitions computed by master on the model of the fixtures.
        Assert.AreEqual("4C0978771A8BE5069D8F83B8481E9C97FB1F720DCFAC3B125351F6EE40EF8934", Fp(model.PhysicalMembers["B"]));
        Assert.AreEqual("2937334092ACEBF2DF642D66686F109585547174F80EE241A313EC8CFB465BF0", Fp(model.PhysicalMembers["C"]));
        using var saved = new MemoryStream();
        if (form == "v2") ModelArchive.Save(model, saved); else ModelArchive.SaveDocument(model, saved);
        // The archive text of master is written again; the geometry frames take new GUIDs at every load (unrelated, as before).
        Assert.AreEqual(WithoutGuids(bytes), WithoutGuids(saved.ToArray()));
        StringAssert.DoesNotMatch(System.Text.Encoding.UTF8.GetString(saved.ToArray()), new System.Text.RegularExpressions.Regex("ServiceabilityLimits"));
    }
    private static string WithoutGuids(byte[] xml) => System.Text.RegularExpressions.Regex.Replace(System.Text.Encoding.UTF8.GetString(xml),
        "(<Guid [^>]*>)[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}(</Guid>)", "$1$2");

    [TestMethod]
    public void DeclarationsRoundTripThroughEveryArchiveForm()
    {
        var model = ModelArchive.Load(new MemoryStream(Fixture("v2")));
        model.PhysicalMembers["B"] = model.PhysicalMembers["B"].WithServiceabilityLimits(Full);
        model.PhysicalMembers["C"] = model.PhysicalMembers["C"].WithServiceabilityLimits(new MemberServiceabilityLimits(driftRatio: new MemberDeclaredValue(300, "Storey drift h/300")));
        string b = Fp(model.PhysicalMembers["B"]), c = Fp(model.PhysicalMembers["C"]);
        Assert.AreNotEqual("4C0978771A8BE5069D8F83B8481E9C97FB1F720DCFAC3B125351F6EE40EF8934", b);
        void Same(GPC.Model.Models.Model copy, string form)
        {
            Assert.AreEqual(b, Fp(copy.PhysicalMembers["B"]), form); Assert.AreEqual(c, Fp(copy.PhysicalMembers["C"]), form);
            var limits = copy.PhysicalMembers["B"].ServiceabilityLimits;
            Assert.AreEqual(MemberDeflectionScheme.CantileverAtEnd, limits.DeflectionScheme, form);
            Assert.AreEqual(Full.DeflectionSchemeSource, limits.DeflectionSchemeSource, form);
            foreach (var (actual, expected) in new[] { (limits.CantileverLengthFactor, Full.CantileverLengthFactor), (limits.MaxDeflectionRatio, Full.MaxDeflectionRatio),
                (limits.Precamber, Full.Precamber), (limits.VariableDeflectionRatio, Full.VariableDeflectionRatio), (limits.DriftRatio, Full.DriftRatio), (limits.MinimumFrequency, Full.MinimumFrequency) })
            { Assert.AreEqual(expected.Value, actual.Value, form); Assert.AreEqual(expected.Source, actual.Source, form); }
            var drift = copy.PhysicalMembers["C"].ServiceabilityLimits;
            Assert.IsNull(drift.DeflectionScheme, form); Assert.IsNull(drift.MaxDeflectionRatio, form); Assert.AreEqual(300, drift.DriftRatio.Value, form);
        }
        foreach (var (form, save) in new (string, Action<GPC.Model.Models.Model, Stream>)[] { ("v2", ModelArchive.Save), ("v4", ModelArchive.SaveDocument) })
        {
            using var stream = new MemoryStream(); save(model, stream);
            var text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
            StringAssert.Contains(text, "<GpcModelArchive version=\"" + form.Substring(1) + "\">", form);
            StringAssert.Contains(text, "ServiceabilityLimits", form);
            stream.Position = 0; Same(ModelArchive.Load(stream), form);
        }
        Same(ModelValues.Copy(model), "copy");
        // Package (version 5): a model with results.
        var mixed = MixedModelFactory.Create();
        mixed.PhysicalMembers.Add("S", new PhysicalMemberDefinition("S", new[] { new BeamMemberPart(250, true) }, "NodeToNode").WithServiceabilityLimits(Full));
        using var package = new MemoryStream(); ModelArchive.Save(mixed, package);
        Assert.AreEqual(0x50, package.ToArray()[0]); Assert.AreEqual(0x4B, package.ToArray()[1]);
        package.Position = 0; var loaded = ModelArchive.Load(package);
        Assert.AreEqual(Fp(mixed.PhysicalMembers["S"]), Fp(loaded.PhysicalMembers["S"]));
        Assert.AreEqual(Full.MinimumFrequency.Source, loaded.PhysicalMembers["S"].ServiceabilityLimits.MinimumFrequency.Source);
    }
}
