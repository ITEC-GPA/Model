using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using GPC.Geometry;
using GPC.Model.Persistence;
using GPC.Model.Sections;

namespace UnitTest;

[TestClass]
public class SectionInputStabilityTest
{
    private static Section Generic() => new ObservedSection();
    private static string Fingerprint(Section section) => ModelArchive.Fingerprint(new object[] { section });

    private sealed class ObservedSection : Section
    {
        public int TorsionCalculations { get; private set; }
        public ObservedSection() : base(new SectionRectangular(20, 10).Shape, "generic") => SetMechanicalProperties();
        public override SectionTorsionProperties CalculateTorsionProperties(double meshSize = 0)
        {
            TorsionCalculations++;
            return base.CalculateTorsionProperties(meshSize);
        }
        public void ChangeOutline()
        {
            _shape = new SectionRectangular(40, 10).Shape;
            SetMechanicalProperties();
        }
        public bool PlasticModuliAreDeferred => double.IsNaN(_wpl1) && double.IsNaN(_wpl2)
            && double.IsNaN(_wplX) && double.IsNaN(_wplY);
    }

    [TestMethod]
    public void ReadingCalculatedPropertiesDoesNotChangeInputs()
    {
        var section = (ObservedSection)Generic();
        string before = Fingerprint(section);
        double jt = section.Jt, jw = section.Jw;
        var center = section.ShearCenter;
        double w1 = section.Wpl1, w2 = section.Wpl2, wx = section.WplX, wy = section.WplY;
        Assert.IsTrue(jt > 0 && w1 > 0 && w2 > 0 && wx > 0 && wy > 0);
        Assert.AreEqual(before, Fingerprint(section));
        Assert.AreEqual(jt, section.Jt);
        Assert.AreEqual(1, section.TorsionCalculations);
        Assert.IsTrue(section.PlasticModuliAreDeferred, "The calculation cache must not overwrite the section definition.");
    }

    [TestMethod]
    public void FingerprintSerializationAndEqualityDoNotRunMechanicalCalculations()
    {
        var section = (ObservedSection)Generic();
        var other = (ObservedSection)Generic();
        Fingerprint(section);
        var info = new SerializationInfo(section.GetType(), new FormatterConverter());
        section.GetObjectData(info, new StreamingContext());
        Assert.IsTrue(section.Equals(other));
        Assert.AreEqual(section.GetHashCode(), other.GetHashCode());
        Assert.AreEqual(0, section.TorsionCalculations + other.TorsionCalculations);
        Assert.IsTrue(section.PlasticModuliAreDeferred && other.PlasticModuliAreDeferred);
    }

    [TestMethod]
    public void RecomputingDefinitionInvalidatesCalculatedProperties()
    {
        var section = (ObservedSection)Generic();
        double jt = section.Jt, wx = section.WplX;
        string before = Fingerprint(section);
        section.ChangeOutline();
        Assert.AreNotEqual(before, Fingerprint(section));
        Assert.IsTrue(section.Jt > jt);
        Assert.IsTrue(section.WplX > wx);
        Assert.AreEqual(2, section.TorsionCalculations);
    }

    [TestMethod]
    public void GivenMechanicalValuesRemainPartOfTheInput()
    {
        Section Given(double jt) => new Section(200, 1000, 500, jt, 0, new Point2d(0, 0), new Point3d(0, 0, 0), 0, "given");
        var a = Given(123);
        var b = Given(124);
        Assert.AreNotEqual(Fingerprint(a), Fingerprint(b));
        Assert.IsFalse(a.Equals(b));
        Assert.AreEqual(123, a.Jt);
    }

    [TestMethod]
    public void RoundTripBeforeAndAfterCalculationKeepsTheSameDefinition()
    {
        var section = new Section(new SectionRectangular(20, 10).Shape, "generic");
        section.SetMechanicalProperties();
        string before = Fingerprint(section);
        double jt = section.Jt, wx = section.WplX;
#pragma warning disable SYSLIB0011
        using var stream = new MemoryStream();
        var formatter = new BinaryFormatter();
        formatter.Serialize(stream, section);
        stream.Position = 0;
        var copy = (Section)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
        Assert.AreEqual(before, Fingerprint(copy));
        Assert.IsTrue(section.Equals(copy));
        Assert.AreEqual(jt, copy.Jt);
        Assert.AreEqual(wx, copy.WplX);
    }

    [TestMethod]
    public void LegacyGenericArchiveNormalizesPreviouslyCalculatedValues()
    {
        var section = new Section(new SectionRectangular(20, 10).Shape, "generic");
        section.SetMechanicalProperties();
        var current = new SerializationInfo(typeof(Section), new FormatterConverter());
        section.GetObjectData(current, new StreamingContext());
        var legacy = new SerializationInfo(typeof(Section), new FormatterConverter());
        foreach (SerializationEntry entry in current)
        {
            object? value = entry.Name switch
            {
                "SectionVersion" => 4.0,
                "Jt" => section.Jt,
                "Jw" => section.Jw,
                "ShearCenter" => section.ShearCenter,
                "WPL1" => section.Wpl1,
                "WPL2" => section.Wpl2,
                "WPLX" => section.WplX,
                "WPLY" => section.WplY,
                _ => entry.Value
            };
            legacy.AddValue(entry.Name, value, entry.ObjectType);
        }
        var constructor = typeof(Section).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
            new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null)!;
        var copy = (Section)constructor.Invoke(new object[] { legacy, new StreamingContext() });
        Assert.AreEqual(Fingerprint(section), Fingerprint(copy));
        Assert.IsTrue(section.Equals(copy));
        Assert.AreEqual(section.Jt, copy.Jt);
        Assert.AreEqual(section.WplX, copy.WplX);
    }
}
