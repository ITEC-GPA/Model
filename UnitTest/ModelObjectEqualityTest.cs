using GPC.Model;
using GPC.Model.LoadCases;

namespace UnitTest;

/// <summary>Architecture review R4: equality of the root of the model objects.</summary>
[TestClass]
public class ModelObjectEqualityTest
{
    private sealed class Alpha : ModelObject { public Alpha(string name) : base(name) { } }
    private sealed class Beta : ModelObject { public Beta(string name) : base(name) { } }

    [TestMethod]
    public void ObjectsOfDifferentTypesWithTheSameNameAreNotEqual()
    {
        var alpha = new Alpha("same"); var beta = new Beta("same");
        Assert.IsFalse(alpha.Equals(beta));
        Assert.IsFalse(beta.Equals(alpha));
        Assert.IsFalse(alpha == beta);
        Assert.IsTrue(alpha != beta);
    }

    [TestMethod]
    public void ObjectsOfTheSameTypeWithTheSameNameAreEqualWithTheSameHash()
    {
        var first = new Alpha("same"); var second = new Alpha("same");
        Assert.IsTrue(first.Equals(second) && second.Equals(first));
        Assert.AreEqual(first.GetHashCode(), second.GetHashCode());
        Assert.IsFalse(first.Equals(new Alpha("other")));
        Assert.IsFalse(ModelObjectIdentityComparer.Instance.Equals(first, second));
    }

    [TestMethod]
    public void ContentEqualsStatesTheContentComparison()
    {
        var first = new LoadCase("G1", LoadCase.LoadCaseTypes.SelfWeight);
        var second = new LoadCase("G1", LoadCase.LoadCaseTypes.SelfWeight);
        Assert.IsTrue(first.ContentEquals(second));
        Assert.AreEqual(first.Equals(second), first.ContentEquals(second));
        Assert.IsFalse(first.ContentEquals(new LoadCaseBase("G1")));
        Assert.IsFalse(first.ContentEquals(null));
    }
}
