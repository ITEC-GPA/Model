using System.Runtime.Serialization;
using GPC.Model;
using GPC.Model.Collections;
using GPC.Model.LoadCases;
using GPC.Model.Persistence;

namespace UnitTest;

[TestClass]
public class ModelRegistryIdentityTest
{
    [TestMethod]
    public void EntityHashSetRemainsStableAfterRenamingAndDistinguishesEqualNames()
    {
        var first = new LoadCaseBase("same");
        var second = new LoadCaseBase("same");
        var items = new HashSet<LoadCaseBase>(ModelObjectIdentityComparer.Instance) { first, second };
        Assert.AreEqual(2, items.Count);
        int hash = ModelObjectIdentityComparer.Instance.GetHashCode(first);
        first.Name = "renamed";
        Assert.IsTrue(items.Contains(first));
        Assert.AreEqual(hash, ModelObjectIdentityComparer.Instance.GetHashCode(first));
        Assert.IsTrue(items.Remove(first));
        Assert.IsTrue(items.Contains(second));
    }

    [TestMethod]
    public void BulkAddRejectsDuplicatesBeforeAddingAnyItem()
    {
        var registry = new UniqueNameCollection<LoadCaseBase>();
        registry.Add(new LoadCaseBase("existing"));
        Assert.ThrowsException<ArgumentException>(() => registry.AddRange(new[]
            { new LoadCaseBase("new"), new LoadCaseBase("existing") }));
        Assert.AreEqual(1, registry.Count);
        Assert.IsFalse(registry.ContainsKey("new"));
    }

    [TestMethod]
    public void DirectRenameUpdatesEveryRegistryIncludingLegacyDictionaryAccess()
    {
        var item = new LoadCaseBase("old");
        var first = new UniqueNameCollection<LoadCaseBase>();
        var second = new UniqueNameCollection<LoadCaseBase>();
        first.Add(item);
        ((Dictionary<string, LoadCaseBase>)second).Add(item.Name, item);
        item.Name = "new";
        Assert.IsFalse(first.ContainsKey("old") || second.ContainsKey("old"));
        Assert.AreSame(item, first["new"]);
        Assert.AreSame(item, second["new"]);
        ((Dictionary<string, LoadCaseBase>)second).Clear();
        first.Rename("new", "third");
        Assert.AreSame(item, first["third"]);
        Assert.AreEqual(0, second.Count);
    }

    [TestMethod]
    public void RenameConflictDoesNotPartiallyChangeAnyRegistry()
    {
        var item = new LoadCaseBase("old");
        var first = new UniqueNameCollection<LoadCaseBase>();
        var second = new UniqueNameCollection<LoadCaseBase>();
        first.Add(item);
        second.Add(item);
        var occupied = new LoadCaseBase("new");
        second.Add(occupied);
        Assert.ThrowsException<ArgumentException>(() => item.Name = "new");
        Assert.AreEqual("old", item.Name);
        Assert.AreSame(item, first["old"]);
        Assert.AreSame(item, second["old"]);
        Assert.AreSame(occupied, second["new"]);
        Assert.IsFalse(first.ContainsKey("new"));
    }

    [TestMethod]
    public void RemovedItemsAreNotKeptInAnIndex()
    {
        var item = new LoadCaseBase("old");
        var registry = new UniqueNameCollection<LoadCaseBase>();
        registry.Add(item);
        ((IDictionary<string, LoadCaseBase>)registry).Remove("old");
        registry.Add(new LoadCaseBase("occupied"));
        item.Name = "occupied";
        Assert.AreEqual(1, registry.Count);
        Assert.AreNotSame(item, registry["occupied"]);
    }

    [TestMethod]
    public void EqualNamesDoNotRemoveOtherEntities()
    {
        var first = new Entity(1, "same");
        var second = new Entity(2, "same");
        var registry = new SortedCollection<Entity>();
        registry.Add(first);
        registry.Add(second);
        Assert.IsFalse(registry.Contains(new Entity(1, "same")));
        Assert.IsTrue(registry.Remove(first));
        Assert.AreEqual(1, registry.Count);
        Assert.AreSame(second, registry[2]);
    }

    [TestMethod]
    public void AddingADifferentEntityCannotSilentlyReplaceAnExistingId()
    {
        var registry = new SortedCollection<Entity>();
        var first = new Entity(1, "same");
        registry.Add(first);
        Assert.ThrowsException<InvalidOperationException>(() => registry.Add(new Entity(1, "different")));
        Assert.AreSame(first, registry[1]);
        Assert.AreEqual(1, registry.Add(first));
    }

    [TestMethod]
    public void UniqueIdRegistryUsesIdentityAndObservesIdsAddedThroughDictionary()
    {
        var registry = new UniqueIdCollection<Entity>();
        var first = new Entity(1, "same");
        var second = new Entity(2, "same");
        registry.Add(first);
        registry.Add(second);
        Assert.IsFalse(registry.Remove(new Entity(1, "same")));
        Assert.IsTrue(registry.Remove(first));
        Assert.AreSame(second, registry[2]);
        Assert.ThrowsException<InvalidOperationException>(() => registry.Add(new Entity(2, "different")));
        ((Dictionary<int, Entity>)registry).Add(100, new Entity(100, "legacy"));
        var generated = new Entity(ModelObjectId.IDUNASSIGNED, "new");
        registry.Add(generated);
        Assert.AreEqual(101, generated.Id);
    }

    [TestMethod]
    public void RenamingAnArchivedGroupUpdatesMembershipAndReinforcementZone()
    {
        var model = PostProcessingTest.Mixed();
        var group = model.AddGroup("old");
        model.AssignGroup("old", new[] { model.AreaElements[10] });
        model.AreaElements[10].Assignments.ReinforcementZone = "old";
        using var stream = new MemoryStream();
        ModelArchive.Save(model, stream);
        stream.Position = 0;
        var copy = ModelArchive.Load(stream);
        copy.Groups["old"].Name = "new";
        Assert.AreEqual(0, copy.ValidateGroups().Count);
        Assert.AreEqual("new", copy.AreaElements[10].Assignments.ReinforcementZone);
        Assert.AreSame(copy.Groups["new"], copy.AreaElements[10].Groups["new"]);
        Assert.AreEqual(1, copy.GetGroupElements("new").Count);
        Assert.ThrowsException<ArgumentException>(() => copy.Groups["new"].Name = " ");
        Assert.AreSame(copy.Groups["new"], copy.AreaElements[10].Groups["new"]);
    }

    private sealed class Entity : ModelObjectId
    {
        public Entity(int id, string name) : base(id, name) { }
    }
}
