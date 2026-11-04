using GPC.Geometry;
using GPC.Model.Sections;
using SectionShearAreas = GPC.Model.Sections.SectionShearAreas;

namespace UnitTest;

/// <summary>Review L6 (4/11/2026), the follow-ups of L4: the shear-area solver in GPCModel with a public API, the conforming BXS of the
/// welded sections, the centrifugal force of moving loads in global axes and the path axes.</summary>
[TestClass]
public class ReviewFixesL6Test
{
    /// <summary>An I of 200 × 15 flanges and a 270 × 10 web as one outline (a generic section, as an adapter builds it from its plates).</summary>
    private static Shape2d IOutline() => new Shape2d(new Polygon2d(new[] { (0d, 0d), (200, 0), (200, 15), (105, 15), (105, 285), (200, 285), (200, 300),
        (0, 300), (0, 285), (95, 285), (95, 15), (0, 15) }.Select(p => new Point2d(p.Item1, p.Item2)).ToArray()));

    [TestMethod]
    public void L6_ShearAreasOfGpcModelReturnTheAreasTheBodiesAndTheMethod()
    {
        // The solver lives in GPCModel (GPC.Model.Sections), no longer in the Converter behind InternalsVisibleTo.
        Assert.AreEqual("GPCModel", typeof(SectionShearAreas).Assembly.GetName().Name);
        Assert.IsFalse(typeof(SectionShearAreas).Assembly.GetCustomAttributes(typeof(System.Runtime.CompilerServices.InternalsVisibleToAttribute), false)
            .Cast<System.Runtime.CompilerServices.InternalsVisibleToAttribute>().Any(a => a.AssemblyName == "GPC.Converter"), "no InternalsVisibleTo for the Converter");
        var h = new SectionH(300, 10, 200, 15, 200, 15, "I");
        var single = SectionShearAreas.Compute(h, .3);
        Assert.AreEqual(1, single.Bodies); Assert.IsFalse(single.HasSeparateParts); Assert.AreEqual(SectionShearAreaMethod.SaintVenantFlexure, single.Method);
        Assert.AreEqual(.3, single.PoissonRatio); Assert.IsTrue(single.Triangles >= SectionShearAreas.MinimumTriangles && single.Nodes > single.Triangles / 3, single.ToString());
        var (a1, a2) = single; Assert.AreEqual(single.Area1, a1); Assert.AreEqual(single.Area2, a2);
        var pair = SectionShearAreas.Compute(SectionBuiltUp.DoubleAngle(new SectionL(100, 10, 100, 10, "L"), 10, "2L"), .3);
        Assert.AreEqual(2, pair.Bodies); Assert.IsTrue(pair.HasSeparateParts); Assert.AreEqual(SectionShearAreaMethod.SeparatePartsBendingTogether, pair.Method);
        // The forwarder kept for the F6 branches gives the same values, bit for bit.
#pragma warning disable CS0618
        var forwarded = GPC.Converter.SectionShearAreas.Compute(h, .3, out int bodies);
#pragma warning restore CS0618
        Assert.AreEqual(single.Area1, forwarded.Area1); Assert.AreEqual(single.Area2, forwarded.Area2); Assert.AreEqual(1, bodies);
    }

    [TestMethod]
    public void L6_AGenericSectionFromAnOutlineNeedsNoPrecomputedProperties()
    {
        // Before: a generic Section built from an outline (its properties not computed yet, Area = 0) got SA1 = SA2 = 0 without any error,
        // because the factors were multiplied by the area of the section. The BridGHe adapter builds such sections for its girders.
        var bare = new Section(IOutline(), "I outline");
        Assert.AreEqual(0, bare.Area);
        var computed = new Section(IOutline(), "I outline"); computed.SetMechanicalProperties();
        var expected = SectionShearAreas.Compute(computed, .3);
        var areas = SectionShearAreas.Compute(bare, .3);
        Assert.AreEqual(expected.Area1, areas.Area1); Assert.AreEqual(expected.Area2, areas.Area2);
        Assert.IsTrue(areas.Area1 > 2 * 200 * 15 * .7 && areas.Area2 > 270 * 10, areas.ToString());
        Assert.AreEqual(0, bare.Area, "the caller's section is not changed");
        // The same outline as a SectionH: same shear areas (same geometry, same principal axes).
        var h = SectionShearAreas.Compute(new SectionH(300, 10, 200, 15, 200, 15, "I"), .3);
        Assert.AreEqual(h.Area1, areas.Area1, 1e-3 * h.Area1); Assert.AreEqual(h.Area2, areas.Area2, 1e-3 * h.Area2);
        // A section without an outline cannot be solved: an explicit error, never zero areas.
        var declared = new Section(1000, 1e6, 1e5, 1e4, 0, new Point2d(0, 0), new Point3d(0, 0, 0), 0, "declared");
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => SectionShearAreas.Compute(declared, .3)).Message, "ShearAreasRequireSectionGeometry");
    }
}
