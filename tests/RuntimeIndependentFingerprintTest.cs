using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO.Compression;
using GPC.Model.Analysis;
using GPC.Model.Core;
using GPC.Model.Models;
using GPC.Model.Persistence;

namespace UnitTest;

/// <summary>Fingerprints and the doubles of archives are data, the same on every runtime: UnitTest runs these tests on .NET 6,
/// Model.NetFramework.Tests on .NET Framework 4.7.2. Type.FullName names the assemblies of the generic arguments, and those of the
/// framework types differ between the two (mscorlib for System.Private.CoreLib, System.Core for HashSet&lt;T&gt;, System for Stack&lt;T&gt;):
/// up to GPCModel 5.37.0.0 no archive with an analysis snapshot written on .NET loaded on .NET Framework, since StageCombinationsMap is a
/// Dictionary of HashSet. Up to 5.37.0.1 .NET Framework read some doubles written by .NET 1 ulp off, and wrote some that did not read back.</summary>
[TestClass]
public class RuntimeIndependentFingerprintTest
{
    // Doubles that .NET Framework read 1 ulp off from the text of .NET (7.763636363636365 and the eleven after it) or whose "R" text it
    // did not read back (10.826600000000001 and the five after it).
    private static readonly long[] Doubles = {
        4620427095885966095, 4529173747557971808, 4619948300867567433, 4613510249799206103, -4603485840251223608, -4617796519939313521,
        4524670147930601312, -8188863338123193314, 4178070368378776789, 3384910843008692860, -8315950619057615594, 6439139742289837737,
        4622284452020469577, 4623867748764467017, 4619751177230073673, 4621791870811225929, 4608984759370891081, 1637726631744473543 };

    // Computed with the released GPCModel 5.36.0.0 on .NET 8 and on .NET 6, alike: the names of the recorded fingerprints. On .NET
    // Framework the same build gave 189906E9..., 8A2C2318... and 6AFC62F6....
    [TestMethod]
    public void FrameworkTypeArgumentsHaveTheNamesOfNet()
    {
        Assert.AreEqual("6C5AFC6587B66AF0B041E190761D60BBB50C0F191775B24BA9973E7AC76924AA", ModelValues.Fingerprint(new object[] {
            new Dictionary<int, HashSet<string>> { [1] = new HashSet<string> { "a", "b" }, [2] = new HashSet<string>() } }));
        // Each framework type that HistoricalTypeNames names explicitly, as a generic argument.
        Assert.AreEqual("D8064DCBE48D3859ABA4B3CDC8ADB87ACE3EFC07669D0F85A43DBE7F4DAF5E20", ModelValues.Fingerprint(new object[] {
            new List<HashSet<int>>(), new List<Queue<int>>(), new List<Stack<int>>(), new List<LinkedList<int>>(), new List<LinkedListNode<int>>(),
            new List<SortedSet<int>>(), new List<SortedList<int, int>>(), new List<SortedDictionary<int, int>>(),
            new List<ConcurrentDictionary<int, int>>(), new List<ObservableCollection<int>>() }));
        // Array, nullable and tuple arguments, nested.
        Assert.AreEqual("9B68B8D6FFF2D0B1A0CB86B3098B3119972B31FAE6E8F0432545677F58F967F9", ModelValues.Fingerprint(new object[] {
            new List<KeyValuePair<string, double>[]>(), new Dictionary<string, List<int?>> { ["k"] = new List<int?> { 1, null } }, new List<Tuple<int, string>>() }));
    }

    [TestMethod]
    public void AnalysisSnapshotsWrittenOnNetValidate()
    {
        // Canonical fingerprints (version 5) and historical ones, of Model 1.6.1.7 on .NET 6 (version 3).
        foreach (var name in new[] { "model-v5-5.23.0.2.zip", "model-v3-1.6.1.7.xml.gz" })
        {
            using var file = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", name));
            using var source = name.EndsWith(".gz", StringComparison.Ordinal) ? new GZipStream(file, CompressionMode.Decompress) : (Stream)file;
            var model = ModelArchive.Load(source);
            Assert.AreEqual(model.Analysis!.InputFingerprint, model.AnalysisFingerprint(), name);
            Assert.AreEqual(AnalysisCompatibility.Compatible, AnalysisCompatibilityValidator.Validate(model).Status, name);
        }
    }

    [TestMethod]
    public void DoublesWrittenOnNetReadExactly()
    {
        // MixedModelFactory with six more nodes at these doubles (ids 100000-100005) and its analysis snapshot, saved by GPCModel
        // 5.37.0.1 on .NET 8. Its snapshot was refused on .NET Framework: the positions read there gave another fingerprint.
        using var file = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "doubles-5.37.0.1-v5.zip"));
        var model = ModelArchive.Load(file);
        var read = Enumerable.Range(0, Doubles.Length / 3).Select(i => model.NodesElements[100000 + i].Position)
            .SelectMany(p => new[] { p.X, p.Y, p.Z }).Select(BitConverter.DoubleToInt64Bits).ToArray();
        CollectionAssert.AreEqual(Doubles, read);
        Assert.AreEqual(model.Analysis!.InputFingerprint, model.AnalysisFingerprint());
    }

    [TestMethod]
    public void DoublesRoundTripExactly()
    {
        // Also the smallest subnormal, the largest subnormal, the smallest normal, the largest double and negative zero.
        var values = Doubles.Concat(new[] { 1L, 0x000FFFFFFFFFFFFFL, 0x0010000000000000L, 0x7FEFFFFFFFFFFFFFL, long.MinValue }).Select(BitConverter.Int64BitsToDouble)
            .Concat(new[] { 0.1, 1.0 / 3, double.NaN, double.PositiveInfinity, double.NegativeInfinity }).ToArray();
        var copy = ModelValues.CopyValue(values);
        CollectionAssert.AreEqual(values.Select(BitConverter.DoubleToInt64Bits).ToArray(), copy.Select(BitConverter.DoubleToInt64Bits).ToArray());
    }
}
