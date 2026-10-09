using System.Globalization;
using GPC.Model.Data.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class CatalogMethodRegressionTests
{
    [TestMethod]
    public void Normalize_IsCultureIndependentAndKeepsDecimalDimensions()
    {
        var previous = CultureInfo.CurrentCulture;
        try { CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("tr-TR");
            Assert.AreEqual("IPE300", SectionCatalogs.Normalize("ipe_300"));
            Assert.AreEqual("CHS60.3X3.6", SectionCatalogs.Normalize("chs 60,3 * 3,6"));
        } finally { CultureInfo.CurrentCulture = previous; }
    }
    [TestMethod]
    public void CatalogFind_UnknownDesignationDoesNotFallBackToAnotherProfile()
    { Assert.IsNull(SectionCatalogs.EN10365ParallelFlangeIH.Find("IPE_DOES_NOT_EXIST")); }
    [TestMethod]
    public void FindAll_UnknownDesignationReturnsEmptyList()
    { Assert.AreEqual(0, SectionCatalogs.FindAll("NO_SUCH_PROFILE").Count); }
    [TestMethod]
    public void SectionMappingCreate_RejectsProfileOfDifferentFamily()
    { Assert.ThrowsException<ArgumentException>(() => SectionMappings.For(SectionFamily.CircularHollow).Create(SectionCatalogs.Find("IPE300"))); }
    [TestMethod]
    public void CreateSection_UnknownDesignationRaisesExplicitError()
    { Assert.ThrowsException<KeyNotFoundException>(() => SectionMappings.CreateSection("NO_SUCH_PROFILE")); }
    [TestMethod]
    public void CreateSection_EachCallReturnsAnIndependentSection()
    {
        var first = SectionMappings.CreateSection("IPE300"); var second = SectionMappings.CreateSection("IPE300");
        first.Name = "edited";
        Assert.AreNotSame(first, second); Assert.AreNotEqual(first.Name, second.Name);
        Assert.AreEqual(first.Area, second.Area, 1e-9);
        Assert.AreEqual(SectionCatalogs.Find("IPE300").Designation, second.Name);
    }
    [TestMethod]
    public void ProfileHas_MissingPropertyIsNotConfusedWithZero()
    {
        var profile = SectionCatalogs.Find("IPE300");
        Assert.IsTrue(profile.Has("h")); Assert.AreEqual(300.0, profile["h"]);
        Assert.IsFalse(profile.Has("not-published")); Assert.IsTrue(double.IsNaN(profile["not-published"]));
    }
}
