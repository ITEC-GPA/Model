using GPC.Model.Core;
using GPC.Model.Sections.Concrete;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Model.Core.Tests;

/// <summary>
/// Arrangement of the laps in the detailing data (5.36.0.0) as a value of the core, without the I/O assembly: the fingerprint of data without the
/// arrangement is the one of GPCModel 5.35.0.0, the arrangement changes it and survives the value copy.
/// </summary>
[TestClass]
public class DetailingLapArrangementValueTest
{
    // ModelValues.Fingerprint of the same data computed by GPCModel 5.35.0.0 (origin/master 07fb7618), before the fields existed.
    private const string Before = "89ACCDEA67480107F6441B396A3161A3C6B057875567CFB1FEE28810FD9184BC";

    [TestMethod]
    public void LapArrangementIsPartOfTheValueAndAbsentFieldsKeepTheFingerprint()
    {
        var without = new ConcreteDetailingData(34, 25, 5, 300, 300, 300, 20, "drawings S-3", lapZone: true, compressionBarsRestrained: true, endZonesConfirmed: true);
        var empty = new ConcreteDetailingData(34, 25, 5, 300, 300, 300, 20, "drawings S-3", true, true, true, null, null);
        var given = new ConcreteDetailingData(34, 25, 5, 300, 300, 300, 20, "drawings S-3", true, true, true, 100, 1);
        Assert.AreEqual(Before, ModelValues.Fingerprint(new object[] { without }));
        Assert.AreEqual(Before, ModelValues.Fingerprint(new object[] { empty }));
        string fingerprint = ModelValues.Fingerprint(new object[] { given });
        Assert.AreNotEqual(Before, fingerprint);
        Assert.AreNotEqual(fingerprint, ModelValues.Fingerprint(new object[] { new ConcreteDetailingData(34, 25, 5, 300, 300, 300, 20, "drawings S-3", true, true, true, 100, 2) }));
        var copy = ModelValues.CopyValue(given);
        Assert.AreNotSame(given, copy); Assert.AreEqual(given, copy); Assert.AreEqual(100, copy.LappedPercent); Assert.AreEqual(1, copy.LapLayers);
        Assert.AreEqual(fingerprint, ModelValues.Fingerprint(new object[] { copy }));
        Assert.IsNull(ModelValues.CopyValue(without).LapLayers);
    }
}
