using System.Reflection;
using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Restraints;
using GPC.Model.Results.Locations;
using GPC.Model.Sections;
using GPC.Model.Structure.Assignments;

namespace UnitTest;

/// <summary>
/// C5 (11/11/2026): the end releases of a beam with a principal angle (an angle, a double angle, any rotated section) whose kind or value
/// differs along or about its two transverse axes, as BridGHe 0.3.0 wrote them (user rule: what 0.3.0 accepted, 0.4.0 accepts and writes as
/// 0.3.0 did). 0.3.0 (StrausR3Helper.cs:894-947 of 95ab7c3) wrote St7SetBeamTRelease3 and St7SetBeamRRelease3 for end 1 and end 2 with the
/// flags along and about the axes 1, 2 and 3 of the beam as given (brFixed 1, brReleased 0, brPartial 2 with its stiffness), with no turn by
/// the principal angle. The Converter refuses them by default (SectionFrameBeamReleaseDiffersInPrincipalAxes) and writes them as 0.3.0 with
/// <see cref="Straus7WriteOptions.SectionAxisDataOnNativeAxes"/> (the convention of the axis data of C4). Native cases need GPC_STRAUS7_WRITE
/// and a licence.
/// </summary>
[TestClass]
public class Straus7LegacyParityC5Test
{
    private static Straus7LegacyParityTest.Recorder Record(Model model, Straus7WriteOptions? options)
    {
        var api = DispatchProxy.Create<Straus7LegacyParityTest.IRecordedLegacyWriter, Straus7LegacyParityTest.Recorder>();
        Straus7ModelWritePlan.Prepare(model, null, null, null, options).Write(api, model); return (Straus7LegacyParityTest.Recorder)(object)api;
    }
    private static string Text((string Name, object?[] Args) call) =>
        call.Name + "(" + string.Join(";", call.Args.Select(a => a is double[] d ? string.Join(",", d.Select(v => v.ToString("R"))) : a is int[] i ? string.Join(",", i) : a?.ToString())) + ")";
    private static string[] Releases(Straus7LegacyParityTest.Recorder log) => log.Named(nameof(IStraus7ModelWriteApi.SetBeamRelease)).Select(Text).ToArray();
    private static string[] Others(Straus7LegacyParityTest.Recorder log) => log.Calls.Where(c => c.Name != nameof(IStraus7ModelWriteApi.SetBeamRelease)).Select(Text).ToArray();
    private static readonly Straus7WriteOptions Legacy = new() { SectionAxisDataOnNativeAxes = true };

    private static BeamDofConnection Free => new(BeamConnectionKind.Released);
    private static BeamDofConnection Fixed => new(BeamConnectionKind.Continuous);
    private static BeamDofConnection Spring(double k) => new(BeamConnectionKind.AbsoluteStiffness, k);

    /// <summary>Releases of every kind at both ends, for translations and rotations, each kind on another axis so that the two transverse axes
    /// always differ: end I translations (released, fixed, partial 1000 N/mm) and rotations (partial 5e7 N·mm/rad, released, fixed); end J
    /// translations (fixed, partial 2000 N/mm, released) and rotations (released, fixed, partial 3e7 N·mm/rad). The beam stays stable (no DOF
    /// released at both ends; a released translation and rotation in one plane at I, the other end fixed in that plane).</summary>
    internal static BeamReleasesAttribute Mixed(CoordinateSystem? frame = null)
    {
        var r = new BeamReleasesAttribute { CoordinateSystem = frame };
        r.I[0] = Free; r.I[1] = Fixed; r.I[2] = Spring(1000); r.I[3] = Spring(5e7); r.I[4] = Free; r.I[5] = Fixed;
        r.J[0] = Fixed; r.J[1] = Spring(2000); r.J[2] = Free; r.J[3] = Free; r.J[4] = Fixed; r.J[5] = Spring(3e7);
        return r;
    }

    /// <summary>The calls 0.3.0 wrote for <see cref="Mixed"/> on beam <paramref name="beam"/> (StrausR3Helper.cs:904-947: TRelease3 and
    /// RRelease3 of end 1, then of end 2, status and partial values on the axes 1, 2, 3 as given).</summary>
    internal static string[] FormerCalls(int beam) => new[]
    {
        $"SetBeamRelease({beam};1;False;0,1,2;0,0,1000)", $"SetBeamRelease({beam};1;True;2,0,1;50000000,0,0)",
        $"SetBeamRelease({beam};2;False;1,2,0;0,2000,0)", $"SetBeamRelease({beam};2;True;0,1,2;0,0,30000000)",
    };

    [TestMethod]
    public void C5_1_RotatedSectionFrameReleases_AreRefusedByDefault_AndWrittenAsTheLegacyExporterWithTheOption()
    {
        // The cantilever of C4 on the unequal angle L 150 × 10 / 100 × 12 (a principal angle), section axes V1 = y, V2 = z, no axis data.
        var rotated = Straus7LegacyParityC4Test.Rotated(true, false); var beam = rotated.BeamElements.Values.Single(); beam.Attributes.Add(Mixed());
        double angle = ((ElasticBeamProperty)beam.BeamProperty).Section.AngleX1;
        Assert.IsTrue(Math.Abs(angle) > .1, "the angle has a principal angle");
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(rotated)).Message, "SectionFrameBeamReleaseDiffersInPrincipalAxes");
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(rotated, null, null, null, new Straus7WriteOptions())).Message, "SectionFrameBeamReleaseDiffersInPrincipalAxes");
        // The same refusal with the section frame given explicitly.
        var explicitFrame = GPC.Model.Core.ModelValues.Copy(rotated); var explicitBeam = explicitFrame.BeamElements.Values.Single();
        explicitBeam.Attributes.Remove(explicitBeam.Attributes.Values.OfType<BeamReleasesAttribute>().Single()); explicitBeam.Attributes.Add(Mixed(explicitBeam.Assignments.SectionAxes));
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(explicitFrame)).Message, "SectionFrameBeamReleaseDiffersInPrincipalAxes");

        // With the option: the calls of 0.3.0, the components on the native axes 1, 2, 3 as given.
        var legacy = Record(rotated, Legacy);
        CollectionAssert.AreEqual(FormerCalls(1), Releases(legacy));
        CollectionAssert.AreEqual(FormerCalls(1), Releases(Record(explicitFrame, Legacy)));
        // The same calls as a beam without a principal angle (a rectangle) with the same section axes and releases: no turn by the principal angle.
        // (Copies of the same model: the same GUIDs, so the same native numbering.)
        var twin = GPC.Model.Core.ModelValues.Copy(rotated); twin.BeamElements.Values.Single().BeamProperty = Straus7LegacyParityC4Test.Rotated(false, false).BeamElements.Values.Single().BeamProperty;
        CollectionAssert.AreEqual(Releases(Record(twin, null)), Releases(legacy));
        // Nothing else changes: every other call is the one of the same angle without the releases (the beam axes are the principal axes, as before).
        var bare = GPC.Model.Core.ModelValues.Copy(rotated); var bareBeam = bare.BeamElements.Values.Single(); bareBeam.Attributes.Remove(bareBeam.Attributes.Values.OfType<BeamReleasesAttribute>().Single());
        var withoutReleases = Record(bare, null); Assert.AreEqual(0, Releases(withoutReleases).Length);
        CollectionAssert.AreEqual(withoutReleases.Calls.Select(Text).ToArray(), Others(legacy));
        var axes = legacy.Named(nameof(IStraus7ModelWriteApi.SetBeamAxes)).Single(); var section = beam.Assignments.SectionAxes; var v = (double[])axes.Args[1]!;
        Assert.IsTrue(Math.Abs(v[0] * section.V1.X + v[1] * section.V1.Y + v[2] * section.V1.Z - 1) > 1e-3, "native axes are the principal axes, not the section axes");

        // Written as before with or without the option: the releases of a beam without a principal angle, a release equal about both transverse
        // axes on the angle, and a release given in the principal axes.
        CollectionAssert.AreEqual(Record(twin, null).Calls.Select(Text).ToArray(), Record(twin, Legacy).Calls.Select(Text).ToArray());
        var equal = GPC.Model.Core.ModelValues.Copy(bare); var symmetric = new BeamReleasesAttribute(); symmetric.I[3] = Free; symmetric.I[4] = Free; symmetric.J[0] = Spring(700); symmetric.J[1] = Spring(700);
        equal.BeamElements.Values.Single().Attributes.Add(symmetric);
        CollectionAssert.AreEqual(Record(equal, null).Calls.Select(Text).ToArray(), Record(equal, Legacy).Calls.Select(Text).ToArray());
        CollectionAssert.AreEqual(new[] { "SetBeamRelease(1;1;False;1,1,1;0,0,0)", "SetBeamRelease(1;1;True;0,0,1;0,0,0)", "SetBeamRelease(1;2;False;2,2,1;700,700,0)", "SetBeamRelease(1;2;True;1,1,1;0,0,0)" }, Releases(Record(equal, null)));
        double c = Math.Cos(angle), s = Math.Sin(angle); var frame = bareBeam.Assignments.SectionAxes;
        var principal = new CoordinateSystem(frame.Origin, frame.V1 * c + frame.V2 * s, frame.V2 * c - frame.V1 * s);
        var inPrincipal = GPC.Model.Core.ModelValues.Copy(bare); inPrincipal.BeamElements.Values.Single().Attributes.Add(Mixed(principal));
        CollectionAssert.AreEqual(Record(inPrincipal, null).Calls.Select(Text).ToArray(), Record(inPrincipal, Legacy).Calls.Select(Text).ToArray());
        CollectionAssert.AreEqual(FormerCalls(1), Releases(Record(inPrincipal, null)));
        // A frame that is neither the section frame nor the principal axes stays refused with the option.
        var other = GPC.Model.Core.ModelValues.Copy(bare); other.BeamElements.Values.Single().Attributes.Add(Mixed(new CoordinateSystem(frame.Origin, frame.V1 * Math.Cos(1.0) + frame.V2 * Math.Sin(1.0), frame.V2 * Math.Cos(1.0) - frame.V1 * Math.Sin(1.0))));
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(other, null, null, null, Legacy)).Message, "UnsupportedStrausBeamAttributeOrReleaseAxes");
    }

    /// <summary>Two beams along x on the line A (0,0,0) – B (1000,0,0) – C (2000,0,0), A and C clamped, a load at B (0, −300, −1000) N: the
    /// first on the angle L 150 × 10 / 100 × 12 (<paramref name="angle"/>) or a rectangle 200 × 100 with the releases <see cref="Mixed"/>,
    /// the second a rectangle 200 × 100 without releases that holds B.</summary>
    internal static Model Propped(bool angle)
    {
        var a = new NodeElement(new Point3d(0, 0, 0)); var b = new NodeElement(new Point3d(1000, 0, 0)); var c = new NodeElement(new Point3d(2000, 0, 0));
        var freedom = new FreedomCase("Supports"); var lc = new LoadCaseBase("P");
        foreach (var n in new[] { a, c })
            n.Assignments.Restrains.Add(new RestrainAssignment { FreedomCase = freedom, Restrain = new NodeRestrain(n, Enumerable.Range(0, 6).Select(i => new DofRestrain((GeometryRestrain.DOF)i, true)).ToList()) });
        var material = new Material("S355", 210000, .3, 7.85e-9, 1.2e-5);
        var released = new BeamElement(a.Position, b.Position, new ElasticBeamProperty(angle ? "L 150x100" : "Rect 200x100", material, angle ? new SectionL(150, 10, 100, 12, "L 150x10/100x12") : new SectionRectangular(200, 100)));
        released.ConnectNodes(a, b);
        var holding = new BeamElement(b.Position, c.Position, new ElasticBeamProperty("Rect 200x100 holding", material, new SectionRectangular(200, 100))); holding.ConnectNodes(b, c);
        foreach (var beam in new[] { released, holding })
        {
            beam.Assignments.Formulation = BeamFormulation.StraightTwoNode;
            beam.Assignments.SectionAxes = new CoordinateSystem(beam.NodeI.Position, new Vector3d(0, 1, 0), new Vector3d(0, 0, 1));
            beam.Assignments.Physical = new BeamPhysicalData { MinimumOutputStations = 5 };
        }
        b.Loads.Add(new PointLoad(0, -300, -1000, 0, 0, 0, b.Position, lc));
        // The assembly takes no attributes (AssemblyRequiresExplicitAssignments): the releases go on the assembled beam.
        var model = ModelAssembly.Assemble(new Element[] { released, holding }, Guid.NewGuid());
        model.BeamElements.Values.Single(e => e.NodeI.Position.X == 0).Attributes.Add(Mixed());
        return model;
    }

    [TestMethod]
    public void C5_1_ProppedModelOfTheNativeCase_IsRefusedByDefault_AndWrittenAsTheLegacyExporterWithTheOption()
    {
        var model = Propped(true); var released = model.BeamElements.Values.Single(b => b.Attributes.Values.OfType<BeamReleasesAttribute>().Any());
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(model)).Message, "SectionFrameBeamReleaseDiffersInPrincipalAxes");
        var legacy = Record(model, Legacy); int id = (int)legacy.Named(nameof(IStraus7ModelWriteApi.SetBeamRelease)).First().Args[0]!;
        CollectionAssert.AreEqual(FormerCalls(id), Releases(legacy));
        CollectionAssert.AreEqual(Releases(Record(Propped(false), null)), Releases(Record(Propped(false), Legacy)));
        Assert.AreEqual(4, Releases(legacy).Length, "the holding beam has no releases: " + released.Id);
    }

    private static double[] Displacement(NodeElement node, string caseName) =>
        node.Results.SelectMany(r => r.Results).OfType<NodeResultDisplacement>().Where(l => l.Case.Name == caseName)
            .Select(l => new[] { l.ResultDisplacement.D1, l.ResultDisplacement.D2, l.ResultDisplacement.D3, l.ResultDisplacement.R1, l.ResultDisplacement.R2, l.ResultDisplacement.R3 }).Single();

    [TestMethod, TestCategory("Straus7Native")]
    public void NativeC5_1_RotatedSectionFrameReleases_ReadBackAsTheLegacyExporterWroteThem_AndTheModelSolves()
    {
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        var model = Propped(true); var plan = Straus7ModelWritePlan.Prepare(model, null, null, null, Legacy);
        var run = plan.RunLinearStatic(model, Path.Combine(folder, "released-angle.st7"), folder);
        Assert.AreEqual(ImportStatus.Completed, run.ImportResults(model, "released").Status);
        var released = model.BeamElements.Values.Single(b => b.Attributes.Values.OfType<BeamReleasesAttribute>().Any());
        // Released 0, fixed 1, partial 2 with its stiffness: every kind at end 1 and end 2, translations and rotations (the 0.3.0 calls of Mixed).
        var expected = new (int End, bool Rotation, int[] Status, double[] Values)[]
        {
            (1, false, new[] { 0, 1, 2 }, new[] { 0, 0, 1000.0 }), (1, true, new[] { 2, 0, 1 }, new[] { 5e7, 0, 0 }),
            (2, false, new[] { 1, 2, 0 }, new[] { 0, 2000.0, 0 }), (2, true, new[] { 0, 1, 2 }, new[] { 0, 0, 3e7 }),
        };
        using (var reader = new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(run.ModelPath, folder);
            // The native beam of the released angle: the one from A (0,0,0) to B (1000,0,0).
            int beam = Enumerable.Range(1, reader.Count(Straus7Entity.Beam)).Single(k =>
            {
                var nodes = reader.ReadElement(Straus7Entity.Beam, k).Nodes.Select(n => reader.ReadNode(n).Coordinates).ToArray();
                return nodes[0][0] == 0 && nodes[1][0] == 1000;
            });
            foreach (var e in expected)
            {
                var read = reader.ReadBeamRelease(beam, e.End, e.Rotation);
                Console.WriteLine($"C5 native: beam {beam} end {e.End} {(e.Rotation ? "rotations" : "translations")}: status {string.Join(",", read.Status)} values {string.Join(",", read.Values.Select(v => v.ToString("R")))}");
                CollectionAssert.AreEqual(e.Status, read.Status, $"end {e.End} {(e.Rotation ? "rotations" : "translations")}");
                CollectionAssert.AreEqual(e.Values, read.Values, $"end {e.End} {(e.Rotation ? "rotations" : "translations")}");
            }
            // The native axes of the beam are the principal axes of the plan (the section axes turned by the principal angle).
            var axes = reader.ReadElement(Straus7Entity.Beam, beam).InitialAxes; double angle = ((ElasticBeamProperty)released.BeamProperty).Section.AngleX1, c = Math.Cos(angle), s = Math.Sin(angle);
            var v1 = released.Assignments.SectionAxes.V1 * c + released.Assignments.SectionAxes.V2 * s;
            Console.WriteLine($"C5 native: principal angle {angle:R} rad, native axis 1 {axes[0]:R},{axes[1]:R},{axes[2]:R}, expected {v1.X:R},{v1.Y:R},{v1.Z:R}");
            Assert.AreEqual(1, axes[0] * v1.X + axes[1] * v1.Y + axes[2] * v1.Z, 1e-6);
        }
        var b = released.NodeJ; var d = Displacement(b, model.LoadCases.Values.Single().Name);
        Console.WriteLine("C5 native: the released angle solves; B " + string.Join(",", d.Select(x => x.ToString("R"))) + ": " + folder);
        Assert.IsTrue(d.All(double.IsFinite) && Math.Abs(d[2]) > 0);
    }
}
