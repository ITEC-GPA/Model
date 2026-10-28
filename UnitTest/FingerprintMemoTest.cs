using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Checking.Preparation;
using GPC.Model.Compatibility;
using GPC.Model.Core;
using GPC.Model.LoadCases;
using GPC.Model.Materials;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;
using GPC.Model.Sections.Steel;
using GPC.Model.Structure.Assignments;

namespace UnitTest;

/// <summary>The memo of a validation read scope reuses the canonical bytes of samples, properties and datasets, and the XML of a
/// property copy. The fingerprints and copies are those computed without it, also across cycles and under historical names.</summary>
[TestClass]
public class FingerprintMemoTest
{
    private static StationResultBeamForces Sample(ILoadCase loadCase, double n) => new StationResultBeamForces(loadCase, new ResultBeamForces(n, 0, 0, 0, 0, 0,
        ResultTransformations.AtPoint(CoordinateSystem.Global, new Point3d(0, 0, 0))), .5) { State = new ResultState { DatasetId = "d", ConcomitantStateId = "C" } };
    private static SteelSection Section() => new SteelSection(new GPC.Model.Sections.SectionH(200, 8, 100, 10, 100, 10, "H"), new SteelMaterial("S275", 210000, 275, 430));

    [TestMethod]
    public void MemoizedBytesAreTheBytesOfThePlainWalkAlsoAcrossCycles()
    {
        var model = new GPC.Model.Models.Model("Memo cycles");
        var loadCase = new LoadCaseBase("C");
        var a = Sample(loadCase, 1); var b = Sample(loadCase, 2); var c = Sample(loadCase, 3);
        // a and b derive from each other; c derives from itself.
        a.State.DerivedFrom = new ResultLocation[] { b }; b.State.DerivedFrom = new ResultLocation[] { a }; c.State.DerivedFrom = new ResultLocation[] { c };
        var section = Section();
        var inputs = new object[][]
        {
            new object[] { a }, new object[] { b }, new object[] { c }, new object[] { a, b, c },
            // From inside the walk of the array held by a's state, the memoized walk of b (which reaches that array) does not apply.
            new object[] { a.State.DerivedFrom }, new object[] { b.State.DerivedFrom, a.State }, new object[] { new object[] { a, new object[] { b } } },
            new object[] { section, new object[] { section, section }, new BeamSectionAssignment { Start = 0, End = 1, Property = section } },
            new object[] { null, "text", 1.5, new ResultLocation[] { c, a } },
        };
        var plain = inputs.Select(i => ModelValues.Fingerprint(i)).ToArray();
        using (ValidationReadScope.Enter(model))
        {
            // Fill the memo in both orders, then compute everything twice.
            foreach (var value in new object[] { b, a, c, section }) Assert.AreEqual(ModelValues.Fingerprint(new[] { value }), ValidationReadScope.FingerprintOf(model, value));
            for (int round = 0; round < 2; round++)
                for (int i = 0; i < inputs.Length; i++) Assert.AreEqual(plain[i], ValidationReadScope.Fingerprint(model, inputs[i]), "input " + i);
        }
        // A value fingerprinted in a scope that has ended is walked again: its change is seen.
        a.ResultBeamForces.N = 10;
        using (ValidationReadScope.Enter(model))
            Assert.AreNotEqual(plain[0], ValidationReadScope.Fingerprint(model, inputs[0]));
    }

    [TestMethod]
    public void HistoricalNamesBypassTheMemo()
    {
        var model = new GPC.Model.Models.Model("Memo legacy");
        var sample = Sample(new LoadCaseBase("C"), 1); var section = Section();
        var inputs = new object[] { sample, new object[] { section }, section };
        string legacy;
        using (FingerprintContracts.Legacy("5.0.0.0", "4.0.0.0")) legacy = ModelValues.Fingerprint(inputs);
        using (ValidationReadScope.Enter(model))
        {
            Assert.AreEqual(ModelValues.Fingerprint(inputs), ValidationReadScope.Fingerprint(model, inputs));
            using (FingerprintContracts.Legacy("5.0.0.0", "4.0.0.0"))
            {
                Assert.AreEqual(legacy, ValidationReadScope.Fingerprint(model, inputs));
                Assert.AreEqual(ModelValues.Fingerprint(new object[] { sample }), ValidationReadScope.FingerprintOf(model, sample));
            }
            Assert.AreEqual(ModelValues.Fingerprint(inputs), ValidationReadScope.Fingerprint(model, inputs));
        }
    }

    [TestMethod]
    public void ScopedCopiesAreNewDetachedCopiesOfTheCurrentValue()
    {
        var model = new GPC.Model.Models.Model("Memo copies");
        var section = Section(); string source = ModelValues.Fingerprint(new object[] { section });
        using (ValidationReadScope.Enter(model))
        {
            var first = ValidationReadScope.CopyValue(model, section); var second = ValidationReadScope.CopyValue(model, section);
            Assert.AreNotSame(section, first); Assert.AreNotSame(first, second); Assert.AreNotSame(first.SteelMaterial, second.SteelMaterial);
            Assert.AreEqual(source, ValidationReadScope.CopyFingerprint(model, section, first));
            Assert.AreEqual(source, ValidationReadScope.CopyFingerprint(model, section, second));
            Assert.AreEqual(source, ModelValues.Fingerprint(new object[] { second }));
            first.SteelMaterial.AlfaThermalExpansion *= 2;
            Assert.AreEqual(section.SteelMaterial.AlfaThermalExpansion, second.SteelMaterial.AlfaThermalExpansion);
            Assert.AreEqual(source, ModelValues.Fingerprint(new object[] { ValidationReadScope.CopyValue(model, section) }));
            Assert.IsNull(ValidationReadScope.CopyValue<SteelSection>(model, null));
            // An unregistered value is refused as by ModelValues.CopyValue, every time.
            for (int i = 0; i < 2; i++) Assert.ThrowsException<SerializationException>(() => ValidationReadScope.CopyValue(model, new List<int>()));
        }
        // The next scope copies the value as it is then.
        section.SteelMaterial.AlfaThermalExpansion *= 3;
        using (ValidationReadScope.Enter(model))
            Assert.AreEqual(section.SteelMaterial.AlfaThermalExpansion, ValidationReadScope.CopyValue(model, section).SteelMaterial.AlfaThermalExpansion);
    }
}
