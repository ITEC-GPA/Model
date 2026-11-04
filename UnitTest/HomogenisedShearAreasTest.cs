using GPC.Geometry;
using GPC.Model.Sections;
using SectionShearAreas = GPC.Model.Sections.SectionShearAreas;

namespace UnitTest;

/// <summary>Shear areas of sections of parts of different materials, homogenised in reference units (decisione utente 4/11: the composite
/// girders of BridGHe take the shear areas of their section homogenised in steel units, De Saint Venant at ν = 0, with the modular ratio
/// of their property).</summary>
[TestClass]
public class HomogenisedShearAreasTest
{
    private static Shape2d Rectangle(double x0, double y0, double x1, double y1) => new Shape2d(new Polygon2d(new[] { (x0, y0), (x1, y0), (x1, y1), (x0, y1) }
        .Select(p => new Point2d(p.Item1, p.Item2)).ToArray()));
    private static void Rel(double expected, double actual, double tolerance, string what) =>
        Assert.AreEqual(expected, actual, tolerance * Math.Abs(expected), what + ": " + expected.ToString("R") + " / " + actual.ToString("R"));

    [TestMethod]
    public void OneMaterialIsTheShearAreaOfTheSectionAndAUniformRatioScalesIt()
    {
        var single = SectionShearAreas.Compute(new SectionRectangular(200, 100));
        var outline = new[] { (Rectangle(-50, -100, 50, 100), 1d) };
        var same = SectionShearAreas.ComputeHomogenised(outline, 0, "R");
        Rel(single.Area1, same.Area1, 1e-9, "SA1"); Rel(single.Area2, same.Area2, 1e-9, "SA2");
        Rel(100 * 200 * 5.0 / 6, same.Area2, 2e-3, "Jourawski 5/6 A");
        var scaled = SectionShearAreas.ComputeHomogenised(new[] { (Rectangle(-50, -100, 50, 100), 4d) }, 0, "R/4");
        Rel(same.Area1 / 4, scaled.Area1, 1e-9, "SA1 / n"); Rel(same.Area2 / 4, scaled.Area2, 1e-9, "SA2 / n");
    }

    [TestMethod]
    public void TwoMaterialRectangleMatchesTheHandHomogenisedSection()
    {
        // A narrow two-layer rectangle (steel 20 × 100 below, a material with n = 2 above, 20 × 100) against the hand-homogenised
        // section (the upper layer 10 wide), for the shear across the layers: the same Jourawski flow, the same energy.
        const double n = 2;
        var parts = new[] { (Rectangle(-10, 0, 10, 100), 1d), (Rectangle(-10, 100, 10, 200), n) };
        var homogenised = SectionShearAreas.ComputeHomogenised(parts, 0, "two materials");
        var hand = SectionShearAreas.ComputeHomogenised(new[] { (Rectangle(-10, 0, 10, 100), 1d), (Rectangle(-5, 100, 5, 200), 1d) }, 0, "hand homogenised");
        Assert.AreEqual(1, homogenised.Bodies);
        // Exact Jourawski reference of the homogenised T (widths 20 and 10): A* = I*² / ∫ (S*/b)² b dy, in steel units.
        double Integrate(Func<double, double> f, double a, double b) { const int m = 20000; double h = (b - a) / m, s = 0; for (int i = 0; i < m; i++) s += f(a + (i + .5) * h); return s * h; }
        double area = 20 * 100 + 10 * 100, centroid = (20 * 100 * 50 + 10 * 100 * 150) / area;
        double Width(double y) => y < 100 ? 20 : 10;
        double inertia = Integrate(y => Width(y) * (y - centroid) * (y - centroid), 0, 200);
        double FirstMoment(double y) => Integrate(z => Width(z) * (z - centroid), y, 200);
        double jourawski = inertia * inertia / Integrate(y => { double s = FirstMoment(y); return s * s / Width(y); }, 0, 200);
        Rel(jourawski, homogenised.Area2, 1e-4, "Jourawski of the homogenised section");
        // The hand-homogenised outline (the upper layer narrowed to b/n) is the same section in Jourawski's theory; as a plane outline it has
        // a re-entrant corner where the width steps, so its plane flexure stores a little more energy: 1.5 % less shear area here.
        Rel(hand.Area2, homogenised.Area2, 2e-2, "SA2 of the hand-homogenised outline");
    }

    [TestMethod]
    public void AStiffPartAloneIsTheLimitOfAVerySoftPart()
    {
        // n -> infinity: the soft part carries nothing; the shear areas tend to those of the steel part alone.
        var steel = SectionShearAreas.ComputeHomogenised(new[] { (Rectangle(-10, 0, 10, 100), 1d) }, 0, "steel");
        var composite = SectionShearAreas.ComputeHomogenised(new[] { (Rectangle(-10, 0, 10, 100), 1d), (Rectangle(-100, 100, 100, 150), 1e7) }, 0, "soft slab");
        // Within the discretisation: the steel part is meshed at the size of the whole section (0.3 % on SA1 here).
        Rel(steel.Area2, composite.Area2, 1e-3, "SA2"); Rel(steel.Area1, composite.Area1, 1e-2, "SA1");
        // With every part of the same material it is the shear area of the one-material section (the steel T).
        var outline = new Shape2d(new Polygon2d(new[] { (-10d, 0d), (10, 0), (10, 100), (100, 100), (100, 150), (-100, 150), (-100, 100), (-10, 100) }
            .Select(p => new Point2d(p.Item1, p.Item2)).ToArray()));
        var section = new Section(outline, "T"); section.SetMechanicalProperties(); var welded = SectionShearAreas.Compute(section);
        // In the principal axes of the T (axis 1 at AngleX1).
        var steelT = SectionShearAreas.ComputeHomogenised(new[] { (Rectangle(-10, 0, 10, 100), 1d), (Rectangle(-100, 100, 100, 150), 1d) }, section.AngleX1, "steel T");
        Rel(welded.Area1, steelT.Area1, 1e-2, "SA1 of the steel T"); Rel(welded.Area2, steelT.Area2, 1e-2, "SA2 of the steel T");
    }

    [TestMethod]
    public void InvalidPartsAreRejected()
    {
        Assert.ThrowsException<ArgumentNullException>(() => SectionShearAreas.ComputeHomogenised(null!, 0));
        Assert.ThrowsException<ArgumentException>(() => SectionShearAreas.ComputeHomogenised(Array.Empty<(Shape2d, double)>(), 0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => SectionShearAreas.ComputeHomogenised(new[] { (Rectangle(0, 0, 1, 1), 0d) }, 0));
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => SectionShearAreas.ComputeHomogenised(new[] { (Rectangle(0, 0, 1, 1), double.PositiveInfinity) }, 0));
        Assert.ThrowsException<ArgumentException>(() => SectionShearAreas.ComputeHomogenised(new[] { (Rectangle(0, 0, 1, 1), 1d) }, double.NaN));
    }
}
