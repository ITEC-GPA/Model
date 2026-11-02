using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO.Compression;
using GPC.Model.Analysis;
using GPC.Model.Core;
using GPC.Model.Models;
using GPC.Model.Persistence;

namespace UnitTest;

/// <summary>Fingerprints are data, the same on every runtime: UnitTest runs these tests on .NET 6, Model.NetFramework.Tests on .NET
/// Framework 4.7.2. Type.FullName names the assemblies of the generic arguments, and those of the framework types differ between the two
/// (mscorlib for System.Private.CoreLib, System.Core for HashSet&lt;T&gt;, System for Stack&lt;T&gt;): up to GPCModel 5.37.0.0 no archive
/// with an analysis snapshot written on .NET loaded on .NET Framework, since StageCombinationsMap is a Dictionary of HashSet.</summary>
[TestClass]
public class RuntimeIndependentFingerprintTest
{
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
}
