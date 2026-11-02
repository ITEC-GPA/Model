using System.Collections;
using System.IO.Compression;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using GPC.Examples;
using GPC.Geometry;
using GPC.Model;
using GPC.Model.Collections;
using GPC.Model.Core;
using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Results.Storage;
using GPC.Model.Structure.Members;
using ModelGroup = GPC.Model.Attributes.Group;

namespace UnitTest;

/// <summary>
/// Copies in memory (<see cref="ContractCopy"/>) against the XML round trip they replace (2/11/2026): the same graph, fingerprints
/// and results for the models of the fixtures, and the same contract as DataContractSerializer for every type of the vocabulary.
/// Also the id counters and the name indexes, which no longer scan every key or every index at each addition.
/// </summary>
[TestClass]
public class ModelCopyTest
{
    private static IEnumerable<(string Name, Model Model)> Corpus()
    {
        yield return ("mixed", MixedModelFactory.Create());
        var surfaces = MixedModelFactory.Create();
        surfaces.PhysicalSurfaces.Add("wall", new PhysicalSurfaceDefinition("wall", new[] { 1090, 1130 }));
        yield return ("mixed with surfaces", surfaces);
        foreach (var fixture in new[] { "model-v3-1.6.1.7.xml.gz", "detailing-legacy-5.35.0.0-v4.xml.gz", "member-limits-legacy-5.28.0.0-v4.xml.gz" })
        {
            using var file = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", fixture));
            using var zip = new GZipStream(file, CompressionMode.Decompress);
            yield return (fixture, ModelArchive.Load(zip));
        }
        using (var package = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "model-v5-5.23.0.2.zip")))
            yield return ("model-v5-5.23.0.2.zip", ModelArchive.Load(package));
    }

    /// <summary>The graph as the image writes it, without the GUIDs that copies regenerate (geometry) or keep (entities, checked apart).</summary>
    private static string Graph(Model model)
    {
        using var stream = new MemoryStream();
        ModelImage.WriteGraph(stream, model, typeof(Model), model.AllElements.ToArray());
        return Regex.Replace(Encoding.UTF8.GetString(stream.ToArray()), "(<Guid [^>]*>)[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}(</Guid>)", "$1$2");
    }

    private static string[] Results(Model model)
    {
        var elements = model.AllElements.ToArray();
        return ModelImage.CaptureResults(model, elements).Select(r => Convert.ToBase64String(r.Digest)).ToArray();
    }

    [TestMethod]
    public void InMemoryCopiesEqualTheImageRoundTrip()
    {
        foreach (var (name, model) in Corpus())
        {
            using (ResultScope.Externalize(model.AllElements.ToArray())) ContractCopy.Copy(model, typeof(Model)); // never through the image here
            var copy = ModelValues.Copy(model);
            var image = ModelImage.Capture(model).Open();
            Assert.AreEqual(Graph(image), Graph(copy), name);
            Assert.AreEqual(model.Guid, copy.Guid, name);
            CollectionAssert.AreEqual(model.AllElements.Select(e => e.Guid).ToArray(), copy.AllElements.Select(e => e.Guid).ToArray(), name);
            CollectionAssert.AreEqual(Results(image), Results(copy), name);
            Assert.AreEqual(image.AnalysisFingerprint(), copy.AnalysisFingerprint(), name);
            Assert.AreEqual(image.VerificationFingerprint(null), copy.VerificationFingerprint(null), name);
            Assert.AreEqual(model.AnalysisFingerprint(), copy.AnalysisFingerprint(), name);
            Assert.IsTrue(copy.AllElements.Zip(model.AllElements).All(p => !ReferenceEquals(p.First, p.Second)), name);
        }
    }

    [TestMethod]
    public void InMemoryCopiesKeepTheModelIndependentAndRewired()
    {
        var model = MixedModelFactory.Create();
        var copy = ModelValues.Copy(model);
        Assert.AreSame(copy.NodesElements[10], copy.BeamElements[250].NodeI);
        copy.Groups["Wall"].Name = "Renamed";
        Assert.IsTrue(copy.AreaElements.Values.All(p => p.Groups.ContainsKey("Renamed")));
        Assert.IsTrue(model.AreaElements.Values.All(p => p.Groups.ContainsKey("Wall")));
        var node = new NodeElement(new Point3d(1, 2, 3));
        Assert.AreEqual(model.NodesElements.Keys.Max() + 1, copy.NodesElements.Add(node));
    }

    /// <summary>The kind of contract DataContractSerializer chooses (its internal DataContract.GetDataContract) and the way the copy
    /// treats each type: an ISerializable class, a class of members, an array, a dictionary or another collection, a shared value.</summary>
    [TestMethod]
    public void CopiesFollowTheContractTheSerializerChoosesForEveryTypeOfTheVocabulary()
    {
        var assembly = typeof(DataContractSerializer).Assembly;
        var contractType = assembly.GetType("System.Runtime.Serialization.DataContract") ?? assembly.GetType("System.Runtime.Serialization.DataContracts.DataContract");
        var get = contractType?.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .FirstOrDefault(m => m.Name == "GetDataContract" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(Type));
        if (get == null) Assert.Inconclusive("DataContract.GetDataContract is not available in this runtime.");
        var mismatches = new List<string>(); int compared = 0;
        foreach (var type in Closure(ModelValues.DataContracts))
        {
            if (type.IsAbstract || type.IsInterface || type == typeof(object) || Nullable.GetUnderlyingType(type) != null) continue;
            object contract;
            try { contract = get.Invoke(null, new object[] { type })!; }
            catch (TargetInvocationException) { continue; } // not serializable at all: never a value of a model
            string expected = Expected(type, contract);
            string actual = ContractCopy.KindOf(type);
            compared++;
            // Without a deserialization constructor the serializer writes the type but fails to read it back: the copy leaves it to the XML.
            if (expected == "Custom" && actual.StartsWith("MissingDeserializationConstructor", StringComparison.Ordinal)) continue;
            if (expected != actual) mismatches.Add(type.FullName + ": serializer " + expected + ", copy " + actual);
        }
        Assert.IsTrue(compared > 700, "Types compared: " + compared);
        Assert.AreEqual(0, mismatches.Count, string.Join("\n", mismatches));
    }

    private static string Expected(Type type, object contract)
    {
        var contractClass = contract.GetType();
        for (var t = contractClass; t != null; t = t.BaseType)
            if (t.Name == "PrimitiveDataContract") return type == typeof(byte[]) ? "Array" : "Shared";
        if (contractClass.Name == "EnumDataContract") return "Shared";
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>)) return "Pair";
        var property = contractClass.GetProperty("IsISerializable", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (contractClass.Name == "ClassDataContract") return property != null && (bool)property.GetValue(contract)! ? "Custom" : "Members";
        if (contractClass.Name == "CollectionDataContract")
        {
            string kind = contractClass.GetProperty("Kind", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!.GetValue(contract)!.ToString()!;
            return kind == "Array" ? "Array" : kind.Contains("Dictionary") ? "Dictionary" : "Collection";
        }
        return contractClass.Name;
    }

    /// <summary>The vocabulary and the declared types of its members, as the serializer reaches them.</summary>
    private static HashSet<Type> Closure(IEnumerable<Type> vocabulary)
    {
        var all = new HashSet<Type>(vocabulary);
        var pending = new Queue<Type>(all);
        const BindingFlags declared = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        while (pending.Count > 0)
        {
            var t = pending.Dequeue();
            var next = new List<Type>();
            if (t.IsArray) next.Add(t.GetElementType()!);
            else
            {
                if (t.IsGenericType) next.AddRange(t.GetGenericArguments());
                if (t.IsDefined(typeof(DataContractAttribute), false))
                    next.AddRange(t.GetMembers(declared).Where(m => m.IsDefined(typeof(DataMemberAttribute), false))
                        .Select(m => m is FieldInfo f ? f.FieldType : ((PropertyInfo)m).PropertyType));
                else if (!typeof(IEnumerable).IsAssignableFrom(t) && !typeof(ISerializable).IsAssignableFrom(t) && t.IsSerializable)
                    next.AddRange(t.GetFields(declared).Where(f => !f.IsNotSerialized).Select(f => f.FieldType));
                if (t.BaseType != null && t.BaseType != typeof(object) && t.BaseType != typeof(ValueType)) next.Add(t.BaseType);
            }
            foreach (var n in next)
                if (!n.IsPrimitive && n != typeof(string) && n != typeof(object) && !n.IsGenericParameter && all.Add(n)) pending.Enqueue(n);
        }
        return all;
    }

    /// <summary>Keys added or removed through the dictionary members: the next addition still takes the largest key + 1, as when it
    /// scanned the keys every time.</summary>
    [TestMethod]
    public void IdsFollowTheKeysAddedOrRemovedThroughTheDictionary()
    {
        var nodes = new SortedCollection<NodeElement>();
        NodeElement Node() => new(new Point3d(0, 0, 0));
        Assert.AreEqual(1, nodes.Add(Node()));
        Assert.AreEqual(2, nodes.Add(Node()));
        ((SortedDictionary<int, NodeElement>)nodes).Add(10, Node());
        Assert.AreEqual(11, nodes.Add(Node()));
        Assert.IsTrue(((SortedDictionary<int, NodeElement>)nodes).Remove(11));
        Assert.AreEqual(12, nodes.Add(Node()), "A removed largest id is not reused.");
        Assert.IsTrue(nodes.RemoveById(12));
        Assert.AreEqual(13, nodes.Add(Node()));
        nodes.Clear();
        Assert.AreEqual(1, nodes.Add(Node()));

        var loads = new UniqueIdCollection<GPC.Model.Loads.Load>();
        var cases = new GPC.Model.LoadCases.LoadCaseBase("G");
        GPC.Model.Loads.Load Load() => new GPC.Model.Loads.PointLoad(0, 0, -1, 0, 0, 0, new Point3d(0, 0, 0), cases);
        loads.Add(Load()); loads.Add(Load());
        ((Dictionary<int, GPC.Model.Loads.Load>)loads).Add(20, Load());
        Assert.AreEqual(20, loads.MaxId);
        var next = Load(); loads.Add(next); Assert.AreEqual(21, next.Id);
        Assert.IsTrue(loads.Remove(21)); Assert.AreEqual(21, loads.MaxId, "The removed id stays counted.");
        Assert.AreEqual(21, loads.CounterBeyondIds);
        var last = Load(); loads.Add(last); Assert.AreEqual(22, last.Id);
    }

    /// <summary>A rename reaches every index that holds the object, in the order the indexes were created, whatever their item type;
    /// the indexes of other item types (the groups of each element, for a node) are not involved.</summary>
    [TestMethod]
    public void RenamesReachTheIndexesOfEveryItemTypeInCreationOrder()
    {
        var anyObject = new UniqueNameCollection<ModelObject>();
        var groups = new UniqueNameCollection<ModelGroup>();
        var unrelated = new UniqueNameCollection<GPC.Model.LoadCases.LoadCaseBase>();
        var group = new ModelGroup("A");
        anyObject.Add(group); groups.Add(group);
        var order = new List<string>();
        anyObject.ItemRenamed += (g, from, to) => order.Add("object " + from + ">" + to);
        groups.ItemRenamed += (g, from, to) => order.Add("group " + from + ">" + to);
        unrelated.ItemRenamed += (g, from, to) => order.Add("unrelated");
        group.Name = "B";
        CollectionAssert.AreEqual(new[] { "object A>B", "group A>B" }, order);
        Assert.AreSame(group, anyObject["B"]); Assert.AreSame(group, groups["B"]);
        groups.Add(new ModelGroup("C"));
        Assert.ThrowsException<ArgumentException>(() => group.Name = "C");
        Assert.AreEqual("B", group.Name); Assert.AreSame(group, anyObject["B"]);
    }
}
