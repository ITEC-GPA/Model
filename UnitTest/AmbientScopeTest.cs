using GPC.Model.Core;
using GPC.Model.Elements;
using GPC.Model.Results.Storage;
using GPC.Geometry;

namespace UnitTest;

/// <summary>Architecture review R7: the ambient scopes that change a result follow the work started inside them on other threads.</summary>
[TestClass]
public class AmbientScopeTest
{
    [TestMethod]
    public void LegacyFingerprintNamesReachTheWorkOnOtherThreads()
    {
        string current = FingerprintContracts.WireName(typeof(List<NodeElement>));
        using (FingerprintContracts.Legacy("1.2.3.4", "8.0.0.0"))
        {
            string inside = FingerprintContracts.WireName(typeof(List<NodeElement>));
            Assert.IsTrue(inside.Contains("GPCModel, Version=1.2.3.4"), inside);
            Assert.AreEqual(inside, Task.Run(() => FingerprintContracts.WireName(typeof(List<NodeElement>))).Result);
            var parallel = new string[8];
            Parallel.For(0, parallel.Length, i => parallel[i] = FingerprintContracts.WireName(typeof(List<NodeElement>)));
            Assert.IsTrue(parallel.All(n => n == inside));
        }
        Assert.AreEqual(current, FingerprintContracts.WireName(typeof(List<NodeElement>)));
        Assert.AreEqual(current, Task.Run(() => FingerprintContracts.WireName(typeof(List<NodeElement>))).Result);
    }

    [TestMethod]
    public void ExternalizedResultsReachTheWorkOnOtherThreads()
    {
        var node = new NodeElement(new Point3d(0, 0, 0));
        Assert.IsFalse(ResultScope.Skips(node));
        using (ResultScope.Externalize(new Element[] { node }))
        {
            Assert.IsTrue(ResultScope.Skips(node));
            Assert.IsTrue(Task.Run(() => ResultScope.Skips(node)).Result);
            Assert.IsFalse(ResultScope.Skips(new NodeElement(new Point3d(1, 0, 0))));
        }
        Assert.IsFalse(ResultScope.Skips(node));
        Assert.IsFalse(Task.Run(() => ResultScope.Skips(node)).Result);
    }
}
