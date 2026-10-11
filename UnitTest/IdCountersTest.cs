using GPC.Examples;
using GPC.Geometry;
using GPC.Model;
using GPC.Model.Collections;
using GPC.Model.Data.Steel;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Stages;

namespace UnitTest;

/// <summary>
/// The id counters of UniqueIdCollection through the archives: a removed id is not reused after a save (defect reported on 11/10/2026, when
/// the tests left BinaryFormatter). The data contracts write the collections as dictionaries, so their owners (model, elements, reinforced
/// concrete sections) save the counter, only when a removed item had the largest id: the other archives and every fingerprint do not change.
/// </summary>
[TestClass]
public class IdCountersTest
{
    private static ReinforcedConcreteRebar Rebar(double x, int id)
        => new(new RebarSectionCircular("", 10, SteelMaterialEN1992Data.B450C), new Point2d(x, 0), 50, id, "r" + id);

    [TestMethod]
    public void RemovedIdsAreNotReusedAfterTheArchives()
    {
        var m = MixedModelFactory.Create();
        m.Stages.Add(new Stage(40, "Stage 1")); m.Stages.Add(new Stage(41, "Stage 2")); Assert.IsTrue(m.Stages.Remove(41));
        var loads = m.NodesElements[40].Loads; int largestLoad = loads.MaxId; Assert.IsTrue(loads.Remove(largestLoad));
        var section = (ReinforcedConcreteSection)m.BeamElements[250].BeamProperty; int largestRebar = section.Rebars.Max(r => r.Id);
        Assert.IsTrue(section.RemoveRebar(largestRebar));
        string fingerprint = m.AnalysisFingerprint();

        using var stream = new MemoryStream(); ModelArchive.Save(m, stream); stream.Position = 0;
        foreach (var copy in new[] { ModelArchive.Load(stream), ModelArchive.Copy(m) })
        {
            Assert.AreEqual(41, copy.Stages.MaxId);
            Assert.AreEqual(largestLoad, copy.NodesElements[40].Loads.MaxId);
            var copied = (ReinforcedConcreteSection)copy.BeamElements[250].BeamProperty;
            Assert.IsTrue(copied.AddRebar(Rebar(5, ModelObjectId.IDUNASSIGNED), out int id));
            Assert.AreEqual(largestRebar + 1, id);
            Assert.AreEqual(fingerprint, copy.AnalysisFingerprint());
        }
    }

    [TestMethod]
    public void TheCountersAreWrittenOnlyWhenTheIdsDoNotSayThem()
    {
        var m = MixedModelFactory.Create();
        string Document()
        {
            using var stream = new MemoryStream(); ModelArchive.SaveDocument(m, stream);
            return System.Text.Encoding.UTF8.GetString(stream.ToArray());
        }
        StringAssert.DoesNotMatch(Document(), new System.Text.RegularExpressions.Regex("MaxId"));
        var loads = m.NodesElements[40].Loads; Assert.AreEqual(2, loads.Count); Assert.IsTrue(loads.Remove(loads.Keys.Min()));
        StringAssert.DoesNotMatch(Document(), new System.Text.RegularExpressions.Regex("MaxId"), "a removed id below the largest one is said by the ids");
        Assert.IsTrue(loads.Remove(loads.MaxId));
        StringAssert.Contains(Document(), "LoadsMaxId");
    }

    [TestMethod]
    public void ACollectionReadWithoutItsOwnerStillCountsItsRemovals()
    {
        var rebars = new UniqueIdCollection<ReinforcedConcreteRebar>();
        rebars.Add(Rebar(0, 1)); rebars.Add(Rebar(10, 7));
        var copy = ContractFormatter.Copy(rebars);
        Assert.AreEqual(7, copy.MaxId);
        Assert.IsTrue(copy.Equals(rebars));
        Assert.IsTrue(copy.Remove(7));
        copy.Add(Rebar(20, ModelObjectId.IDUNASSIGNED));
        Assert.AreEqual(8, copy.MaxId, "the removed id 7 is not reused");
    }
}
