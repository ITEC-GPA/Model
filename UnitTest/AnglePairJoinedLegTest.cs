using GPC.Model.Core;
using GPC.Model.Data.Sections;
using GPC.Model.Persistence;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class AnglePairJoinedLegTest
{
    [DataTestMethod]
    [DataRow(150d, 100d, true, true)]
    [DataRow(150d, 100d, false, false)]
    [DataRow(100d, 150d, true, false)]
    [DataRow(100d, 150d, false, true)]
    [DataRow(100d, 100d, true, true)]
    [DataRow(100d, 100d, false, true)]
    public void JoinedLegIsChosenByLengthAndPreservesComponentAxes(double h, double b, bool longest, bool vertical)
    {
        var single = new SectionL(b, 10, h, 10, "L");
        var pair = SectionBuiltUp.DoubleAngle(single, 12, longest ? SectionBuiltUp.AnglePairJoinedLeg.Long : SectionBuiltUp.AnglePairJoinedLeg.Short, "2L");
        Assert.AreSame(single, pair.Parts[0].Section);
        Assert.AreSame(single, pair.Parts[1].Section);
        Assert.AreEqual(vertical, pair.Parts[1].MirrorX);
        Assert.AreEqual(!vertical, pair.Parts[1].MirrorY);
        Assert.AreEqual(vertical ? 2 * b + 12 : b, pair.Width, 1e-9);
        Assert.AreEqual(vertical ? h : 2 * h + 12, pair.Height, 1e-9);
        Assert.AreEqual(2 * (10 * b + (h - 10) * 10), pair.Area, 1e-9);
        Assert.AreEqual(0, pair.Jxy, 1e-7);
        var restored = ModelArchive.CopyValue(pair);
        Assert.AreSame(restored.Parts[0].Section, restored.Parts[1].Section);
        Assert.AreEqual(ModelValues.Fingerprint(new object[] { pair }), ModelValues.Fingerprint(new object[] { restored }));
    }

    [TestMethod]
    public void CatalogAngleCanBePairedOnEitherLegWithoutChangingTheCatalogProfile()
    {
        var single = (SectionL)SectionMappings.CreateSection(SectionCatalogs.Find("L8X6X1")!);
        var before = ModelValues.Fingerprint(new object[] { single });
        var longPair = SectionBuiltUp.DoubleAngle(single, 10, SectionBuiltUp.AnglePairJoinedLeg.Long, "long backs");
        var shortPair = SectionBuiltUp.DoubleAngle(single, 10, SectionBuiltUp.AnglePairJoinedLeg.Short, "short backs");
        Assert.AreNotEqual(longPair.Parts[1].MirrorX, shortPair.Parts[1].MirrorX);
        Assert.AreEqual(before, ModelValues.Fingerprint(new object[] { single }));
        Assert.AreSame(longPair.Parts[0].Section, longPair.Parts[1].Section);
        Assert.AreSame(shortPair.Parts[0].Section, shortPair.Parts[1].Section);
    }

    [TestMethod]
    public void InvalidLegCannotSelectAnAccidentalDefault()
    {
        var single = new SectionL(100, 10, 150, 10, "L");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => SectionBuiltUp.DoubleAngle(single, 10, (SectionBuiltUp.AnglePairJoinedLeg)42, "bad"));
        Assert.ThrowsException<ArgumentNullException>(() => SectionBuiltUp.DoubleAngle(null!, 10, SectionBuiltUp.AnglePairJoinedLeg.Long, "bad"));
    }
}
