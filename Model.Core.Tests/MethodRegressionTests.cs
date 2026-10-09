using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Core.Coordinates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Model.Core.Tests;

[TestClass]
public class MethodRegressionTests
{
    [TestMethod]
    public void CopyValue_CoordinateSystemIsIndependentAfterTranslation()
    {
        var original = new CoordinateSystem(new Point3d(10, 20, 30), Vector3d.YAxis, Vector3d.ZAxis);
        var copy = ModelValues.CopyValue(original);
        copy.SetOrigin(new Point3d(1, 2, 3));
        Assert.AreEqual(10.0, original.Origin.X);
        Assert.AreEqual(1.0, copy.Origin.X);
        Assert.AreNotSame(original.V1, copy.V1);
        Assert.AreEqual(1.0, copy.ToGlobal(new Vector3d(0, 0, 1)).X, 1e-12);
    }

    [TestMethod]
    public void Copy_NullModelIsRejected()
    { Assert.ThrowsException<ArgumentNullException>(() => ModelValues.Copy(null!)); }

    [TestMethod]
    public void Fingerprint_SequenceOrderAndSingleValueChangesAreSignificant()
    {
        var original = ModelValues.Fingerprint(new object[] { new double[] { 1, 2, 3 } });
        Assert.AreNotEqual(original, ModelValues.Fingerprint(new object[] { new double[] { 3, 2, 1 } }));
        Assert.AreNotEqual(original, ModelValues.Fingerprint(new object[] { new double[] { 1, 2, 3.001 } }));
        Assert.AreEqual(original, ModelValues.Fingerprint(new object[] { new double[] { 1, 2, 3 } }));
    }
}
