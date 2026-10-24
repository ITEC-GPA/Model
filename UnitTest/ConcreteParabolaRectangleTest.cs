using System.Reflection;
using System.Runtime.Serialization;
using GPC.Model.Core;
using GPC.Model.Materials;
using GPC.Model.Standards;

namespace UnitTest;

[TestClass]
public class ConcreteParabolaRectangleTest
{
    private static ConcreteMaterialEN1992 Concrete(double fck = 25) => new("C", fck, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);

    [TestMethod]
    public void DefaultUsesTheAnalyticCharacteristicAndDesignStressThroughBothApis()
    {
        var material = Concrete(); Material polymorphic = material;
        var standard = new StandardEN1992p11 { AlphaCC = .85, GammaC = 1.5 };
        foreach (double e in new[] { 0, -.000125, -.000833, -.002, -.0035, -.00350001 })
        {
            double r = e / -.002;
            double expected = e < -.0035 ? 0 : e <= -.002 ? -25 : -25 * (2 * r - r * r);
            Assert.AreEqual(expected, polymorphic.GetStress(e), 1e-12);
            Assert.AreEqual(expected * .85 / 1.5, material.CalculateDesignStressConcrete(standard, e), 1e-12);
            Assert.AreEqual(expected * .85 / 1.5, material.CalculateDesignStressConcrete((Standard)standard, e), 1e-12);
        }
        Assert.AreEqual(material.StressStrainTableTension.GetStress(1e-5), material.GetStress(1e-5), 0);
    }

    [TestMethod]
    public void ExplicitHistoricalMethodUsesTheStoredChordsAndIsMarkedObsolete()
    {
        var material = Concrete(); double e = -.000125, analytic = material.GetStress(e);
#pragma warning disable CS0618 // Deliberately test the historical option.
        material.ParabolaRectangleMethod = ConcreteParabolaRectangleMethod.TabulatedLegacy;
        var field = typeof(ConcreteParabolaRectangleMethod).GetField(nameof(ConcreteParabolaRectangleMethod.TabulatedLegacy))!;
#pragma warning restore CS0618
        Assert.IsFalse(ConcreteParabolaRectangleLaw.TryCreate(material, out _));
        Assert.AreEqual(material.StressStrainTableCompression.GetStress(e), material.GetStress(e), 0);
        Assert.IsTrue(Math.Abs(analytic - material.GetStress(e)) > .09);
        StringAssert.Contains(field.GetCustomAttribute<ObsoleteAttribute>()!.Message, "Analytic");
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => material.ParabolaRectangleMethod = (ConcreteParabolaRectangleMethod)99);
    }

    [TestMethod]
    public void CustomAndHighStrengthTablesAreNeverReplacedByTheQuadraticLaw()
    {
        var material = Concrete();
        Assert.IsTrue(ConcreteParabolaRectangleLaw.TryCreate(material, out _));
        material.StressStrainTableCompression.SetStress(-2, 1);
        Assert.IsFalse(ConcreteParabolaRectangleLaw.TryCreate(material, out _));
        Assert.AreEqual(material.StressStrainTableCompression.GetStress(-.000125), material.GetStress(-.000125), 0);
        material = Concrete(70);
        Assert.IsFalse(ConcreteParabolaRectangleLaw.TryCreate(material, out _));
        Assert.AreEqual(material.StressStrainTableCompression.GetStress(-.001), material.GetStress(-.001), 0);
    }

    [TestMethod]
    public void ArchivesCopiesAndFingerprintsRetainTheMethod()
    {
        var material = Concrete();
        string analytic = ModelValues.Fingerprint(new object[] { material });
        foreach (var method in Enum.GetValues<ConcreteParabolaRectangleMethod>())
        {
            material.ParabolaRectangleMethod = method;
            var copy = ContractFormatter.Copy(material);
            Assert.AreEqual(method, copy.ParabolaRectangleMethod);
            Assert.IsTrue(material.Equals(copy));
            Assert.AreEqual(material.GetStress(-.000125), copy.GetStress(-.000125), 0);
            string fingerprint = ModelValues.Fingerprint(new object[] { material });
            Assert.AreEqual(fingerprint, ModelValues.Fingerprint(new object[] { copy }));
            Assert.AreEqual(method == ConcreteParabolaRectangleMethod.Analytic, analytic == fingerprint);
        }
    }

    [TestMethod]
    public void OlderArchivesDefaultToAnalyticAndNewerVersionsAreRefused()
    {
        var material = Concrete();
        var original = new SerializationInfo(material.GetType(), new FormatterConverter());
        material.GetObjectData(original, new StreamingContext());
        var ctor = material.GetType().GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
            new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null)!;
        ConcreteMaterialEN1992 Read(int version)
        {
            var info = new SerializationInfo(material.GetType(), new FormatterConverter());
            foreach (SerializationEntry entry in original)
                if (entry.Name != "ParabolaRectangleMethod")
                    info.AddValue(entry.Name, entry.Name == "ConcreteMaterialEuropeanCommonVersion" ? version : entry.Value, entry.ObjectType);
            return (ConcreteMaterialEN1992)ctor.Invoke(new object[] { info, new StreamingContext() });
        }
        var old = Read(3);
        Assert.AreEqual(ConcreteParabolaRectangleMethod.Analytic, old.ParabolaRectangleMethod);
        Assert.AreEqual(material.GetStress(-.000125), old.GetStress(-.000125), 0);
        Assert.IsInstanceOfType(Assert.ThrowsException<TargetInvocationException>(() => Read(5)).InnerException, typeof(SerializationException));
    }
}
