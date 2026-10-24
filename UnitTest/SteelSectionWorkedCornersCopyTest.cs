using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class SteelSectionWorkedCornersCopyTest
{
    private static SteelSection Steel(Section shape, bool welded = false)
        => new(shape, new SteelMaterial("S275 actual thickness", 210000, 275, 410),
            welded ? Section.SectionTypes.Welded : Section.SectionTypes.Rolled);
    private static SteelSection Copy(SteelSection source, bool domain)
        => domain ? ModelValues.CopyValue(source) : ContractFormatter.Copy(source);
    private static IEnumerable<Curve3d> Segments(Curve3d curve)
    {
        if (curve is PolyCurve3d poly)
            for (int i = 0; i < poly.SegmentCount; i++)
                foreach (var segment in Segments(poly.GetSegment(i))) yield return segment;
        else yield return curve;
    }
    private static Curve3d[] Boundary(ISectionShape section)
        => section.GetCurveOutlines().SelectMany(o => Segments(o.Boundary)).ToArray();
    private static void Close(double expected, double actual, double relative = 2e-11)
        => Assert.AreEqual(expected, actual, Math.Max(1e-9, relative * Math.Abs(expected)));

    [DataTestMethod]
    [DataRow(true, false)] [DataRow(true, true)] [DataRow(false, false)] [DataRow(false, true)]
    public void RolledHCopyKeepsCurvedRootsBeforeAndAfterLazyPlasticEvaluation(bool domain, bool warm)
    {
        var original = Steel(new SectionH(533.1, 10.1, 209.3, 15.6, 209.3, 15.6, "SCI C1", 12.7));
        if (warm) { _ = original.Wpl1; _ = original.Wpl2; _ = Boundary(original); }
        var copy = Copy(original, domain);
        Assert.AreEqual(4, Boundary(copy).OfType<ArcCurve3d>().Count());
        Assert.AreEqual(4, Boundary(original).OfType<ArcCurve3d>().Count());
        Close(original.Wpl1, copy.Wpl1); Close(original.Wpl2, copy.Wpl2);
        // Independent exact circular-root integrals. Wpl's existing64-chord/quadrant
        // outline has a declared3e-5 geometric approximation; A/J are analytic.
        Close(11737.802520902502, copy.Area); Close(552273065.0878704, copy.J11);
        Close(2360087.417021688, copy.Wpl1, 3e-5);
        Assert.IsTrue(copy.Wpl1 > 2350000, "Sharp value2325735.515 must not survive the copy.");
        Assert.AreEqual(Section.SectionTypes.Rolled, copy.SectionType);
        var again = Copy(copy, domain); Close(copy.Wpl1, again.Wpl1);
        Assert.AreEqual(4, Boundary(again).OfType<ArcCurve3d>().Count());
    }

    [DataTestMethod]
    [DataRow(true, false)] [DataRow(true, true)] [DataRow(false, false)] [DataRow(false, true)]
    public void WeldedHCopyKeepsStraightChamfersAndNeverChangesThemIntoFillets(bool domain, bool warm)
    {
        const double h = 300, b = 150, tw = 10, tf = 15, throat = 6;
        var original = Steel(new SectionH(h, tw, b, tf, b, tf, "welded H", throat), true);
        if (warm) { _ = original.Wpl1; _ = original.Wpl2; }
        var copy = Copy(original, domain);
        // Four physical triangular weld-root corners, leg=1.41R per existing Model contract.
        double leg = 1.41 * throat, ac = leg * leg / 2;
        double expected = 2 * b * tf * (h / 2 - tf / 2) + tw * Math.Pow(h - 2 * tf, 2) / 4
            + 4 * ac * ((h - 2 * tf) / 2 - leg / 3);
        Close(expected, copy.Wpl1); Close(original.Wpl1, copy.Wpl1); Close(original.Area, copy.Area);
        Assert.AreEqual(0, Boundary(copy).OfType<ArcCurve3d>().Count());
        Assert.AreEqual(16, Boundary(copy).Length);
        Assert.AreEqual(Section.SectionTypes.Welded, copy.SectionType);
    }

    [DataTestMethod]
    [DataRow(true, true)] [DataRow(true, false)] [DataRow(false, true)] [DataRow(false, false)]
    public void PackedIdenticalAnglePairPreservesBothWorkedOutlinesAndComponentAlias(bool domain, bool longLeg)
    {
        var angle = new SectionL(100, 10, 150, 10, "L150x100x10", 15, 7.5);
        var pair = SectionBuiltUp.DoubleAngle(angle, 12, longLeg ? SectionBuiltUp.AnglePairJoinedLeg.Long
            : SectionBuiltUp.AnglePairJoinedLeg.Short, "2L");
        var original = Steel(pair); var copy = Copy(original, domain); var restored = (SectionBuiltUp)copy.SectionShape;
        Assert.AreSame(restored.Parts[0].Section, restored.Parts[1].Section);
        Assert.AreNotSame(angle, restored.Parts[0].Section);
        Assert.AreEqual(6, Boundary(copy).OfType<ArcCurve3d>().Count());
        Close(original.Area, copy.Area); Close(original.Wpl1, copy.Wpl1); Close(original.Wpl2, copy.Wpl2);
        var chord = restored.Parts[0].Section;
        // The vertical inner flat begins at t+r=25, not the sharp-corner t=10.
        var vertical = Boundary(chord).OfType<LineCurve3d>().Where(l => Math.Abs(l.StartPoint.X - 10) < 1e-9
            && Math.Abs(l.EndPoint.X - 10) < 1e-9).ToArray();
        Assert.AreEqual(1, vertical.Length);
        Close(25, Math.Min(vertical[0].StartPoint.Y, vertical[0].EndPoint.Y));
        Close(142.5, Math.Max(vertical[0].StartPoint.Y, vertical[0].EndPoint.Y));
        Assert.AreEqual(pair.Parts[1].MirrorX, restored.Parts[1].MirrorX);
        Assert.AreEqual(pair.Parts[1].MirrorY, restored.Parts[1].MirrorY);
    }

    [DataTestMethod] [DataRow(true)] [DataRow(false)]
    public void PackedChannelPairRestoresItsRealRootsOnBothComponents(bool domain)
    {
        var channel = new SectionC(200, 8, 80, 12, 80, 12, "C", 12, 0);
        var original = Steel(SectionBuiltUp.DoubleChannel(channel, 12, true, "2C"));
        var copy = Copy(original, domain); var restored = (SectionBuiltUp)copy.SectionShape;
        Assert.AreSame(restored.Parts[0].Section, restored.Parts[1].Section);
        Assert.AreEqual(4, Boundary(copy).OfType<ArcCurve3d>().Count());
        Close(original.Area, copy.Area); Close(original.Wpl1, copy.Wpl1); Close(original.Wpl2, copy.Wpl2);
    }

    [DataTestMethod] [DataRow(true)] [DataRow(false)]
    public void GeometricRadiiAndSectionsWithoutProductionDependentCornersAreUnchanged(bool domain)
    {
        foreach (var shape in new Section[] { new SectionRHSRoundedCorners(200, 100, 10, 15, 10, "RHS"),
            new SectionCircular(100), new SectionH(300, 10, 150, 15, 150, 15, "sharp H", 0) })
        {
            var original = Steel(shape); var copy = Copy(original, domain);
            Close(original.Area, copy.Area); Close(original.J11, copy.J11);
            Close(original.Wpl1, copy.Wpl1); Close(original.Wpl2, copy.Wpl2);
            Assert.AreEqual(Boundary(original).OfType<ArcCurve3d>().Count(), Boundary(copy).OfType<ArcCurve3d>().Count());
        }
    }

    [TestMethod]
    public void RestoringTheTransientFlagDoesNotRequireANewArchiveFieldOrFingerprintBaseline()
    {
        var original = Steel(new SectionH(533.1, 10.1, 209.3, 15.6, 209.3, 15.6, "SCI C1", 12.7));
        var copy = ModelValues.CopyValue(original);
        Assert.AreEqual(ModelValues.Fingerprint(new object[] { original }), ModelValues.Fingerprint(new object[] { copy }));
        Close(original.Wpl1, copy.Wpl1);
        Assert.AreEqual(ModelValues.Fingerprint(new object[] { original }), ModelValues.Fingerprint(new object[] { copy }));
    }
}
