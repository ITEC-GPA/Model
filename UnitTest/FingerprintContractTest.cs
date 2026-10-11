using System.Runtime.Serialization;
using System.Security.Cryptography;
using GPC.Geometry;
using GPC.Model.Constraints;
using GPC.Model.Core;
using GPC.Model.Restraints;
using GPC.Model.Sections;

[assembly: FingerprintVocabulary(typeof(UnitTest.FingerprintContractTest.Vocabulary))]

namespace UnitTest;

/// <summary>Architecture review R8: the fingerprints come only from explicit contracts, with the bytes of the historical reflection.</summary>
[TestClass]
public class FingerprintContractTest
{
    internal sealed class Vocabulary : IFingerprintVocabulary
    {
        public void Declare(FingerprintVocabulary v)
            => v.Record(typeof(DeclaredRecord), "Test.DeclaredRecord", new FingerprintMember("_old", "_renamed"), "Name", new FingerprintMember("Added", omitWhenNull: true));
    }
    internal sealed class DeclaredRecord
    {
        internal double _renamed = 2.5;
        internal string Name = "x";
        internal string? Added;
        [NonSerialized] internal object Cache = new();
    }
    private sealed class UndeclaredRecord { internal double Value = 1; }
    [Serializable]
    private sealed class PhantomVersionEntry : ISerializable
    {
        public void GetObjectData(SerializationInfo info, StreamingContext context) { info.AddValue("Value", 1.0); info.AddValue("PhantomVersion", 3); }
    }
    private static FingerprintVocabulary Empty() => new(typeof(FingerprintContractTest).Assembly);

    [TestMethod]
    public void RecordsThatLeftTheReflectionKeepTheirFingerprints()
    {
        // Fingerprints computed with the reflection of the fields, before the explicit contracts (Model 117ed806).
        Assert.AreEqual("406140D6C064CC7BBB1AD81BB14A352E8FE7B8CF0E65B395F2426FA5BEAA4B83",
            ModelValues.Fingerprint(new object[] { new SectionH(300, 11, 300, 19, 300, 19, "HE 300 B", 27) }));
        Assert.AreEqual("FBE96D3886BBDC0E0C41A4A64D687F946A8AEDC611FA6C4939F3B27AAB00AF74",
            ModelValues.Fingerprint(new object[] { new MultiPointsCostrain.Equation(null, (GeometryRestrain.DOF)2, 0.75) }));
        Assert.AreEqual("B31CA8FD5467817F30021E8C2D672E6F266A87666F99A95ADA1D5CEF6B30C9B2", ModelValues.Fingerprint(new object[] {
            new[] { new KeyValuePair<Guid, byte[]>(new Guid("0f8fad5b-d9cb-469f-a165-70867728950e"), new byte[] { 1, 2, 3 }) } }));
    }

    [TestMethod]
    public void BuiltInContractsDeclareEveryField()
        => Assert.AreEqual("", string.Join("\n", FingerprintContracts.Uncovered()));

    [TestMethod]
    public void DeclaredRecordsUseTheirWireNamesAndSkipTheCaches()
    {
        using var memory = new MemoryStream();
        using (var w = new BinaryWriter(memory, System.Text.Encoding.UTF8, true))
        { w.Write("Test.DeclaredRecord"); w.Write("_old"); w.Write("System.Double"); w.Write(2.5); w.Write("Name"); w.Write("System.String"); w.Write("x"); }
        string expected = Convert.ToHexString(SHA256.HashData(memory.ToArray()));
        var record = new DeclaredRecord();
        Assert.AreEqual(expected, ModelValues.Fingerprint(new object[] { record }));
        record.Cache = new object();
        Assert.AreEqual(expected, ModelValues.Fingerprint(new object[] { record }));
        record.Added = "new";
        Assert.AreNotEqual(expected, ModelValues.Fingerprint(new object[] { record }));
    }

    [TestMethod]
    public void IncompleteOrForeignDeclarationsAreRefused()
    {
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => Empty().Record(typeof(DeclaredRecord), "T", "_renamed", "Name")).Message,
            "declare the fingerprint member of DeclaredRecord.Added");
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => Empty().Record(typeof(DeclaredRecord), "T", "_old", "Name", "Added")).Message,
            "has no instance field _old");
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => Empty().Record(typeof(DeclaredRecord), "T", "_renamed", "Name", "Added", "Cache")).Message,
            "[NonSerialized]");
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => Empty().Record(typeof(CoordinateSystem), "T")).Message, "own assembly");
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => Empty().Record(typeof(PhantomVersionEntry), "T")).Message,
            "does not use field contracts");
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => Empty().Record(typeof(UndeclaredRecord), "GPC.Model.UnitsSystem", "Value")).Message,
            "already used");
    }

    [TestMethod]
    public void UndeclaredRecordsAndVersionEntriesAreRefused()
    {
        // No switch: production refuses them too (decision of 11/10/2026, before the first release of the refactored ANTHEA).
        StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => ModelValues.Fingerprint(new object[] { new UndeclaredRecord() })).Message,
            "UnregisteredFingerprintType");
        StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => ModelValues.Fingerprint(new object[] { new PhantomVersionEntry() })).Message,
            "UndeclaredFingerprintEntry");
    }
}
