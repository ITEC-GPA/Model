using System.Reflection;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Actions.Moving;

namespace UnitTest;

/// <summary>Review L10 (4/11/2026), user decision 6 on the Straus7 writer: the centrifugal offset of GPC is N per vehicle; the writer converts
/// it to the offset R31 applies to every flagged force (N per point force, N per mm of distributed force) and the reader converts it back.</summary>
[TestClass]
public class ReviewFixesL10CentrifugalTest
{

    internal static MovingPointForce Point(double x, double fz, bool? centrifugal = true) => new() { X = x, Force = new Vector3d(0, 0, fz), Axes = MovingLoadAxes.Path, ApplyCentrifugal = centrifugal };
    internal static MovingDistributedForce Line(double x1, double x2, double q, bool? centrifugal = true, MovingDistributionMobility mobility = MovingDistributionMobility.Grouped)
        => new() { Mobility = mobility, X1 = x1, X2 = x2, Y1 = -10, Y2 = 10, ForcePerLength = new Vector3d(0, 0, q), Axes = MovingLoadAxes.Path, ApplyCentrifugal = centrifugal };
    internal static MovingVehicle Vehicle(string name, MovingPointForce[] points, params MovingDistributedForce[] lines) => new() { Name = name, Speed = 20000, PointForces = points, DistributedForces = lines };
    private static MovingLoadTemplate Template(MovingCentrifugalLaw? law, params MovingVehicle[] vehicles) => new() { Name = "vehicles", Centrifugal = law, Vehicles = vehicles };
    private static MovingCentrifugalLaw Law(double offset = 50, double factor = .1) => new() { FactorKind = MovingCentrifugalFactorKind.Constant, Factor = factor, OffsetForce = offset };
    private static readonly MovingPointForce[] None = Array.Empty<MovingPointForce>();

    /// <summary>Vehicles of 1000 N vertical in total, with several forces; R31 measure (flagged point forces + flagged distributed mm).</summary>
    internal static (string Name, MovingVehicle Vehicle, double Measure)[] MixedVehicles() => new[] {
        ("one point 1000 N", Vehicle("one point", new[] { Point(0, -1000) }), 1d),
        ("two points 2 x 500 N", Vehicle("two points", new[] { Point(-50, -500), Point(50, -500) }), 2d),
        ("three points 3 x 1000/3 N", Vehicle("three points", new[] { Point(-60, -1000d / 3), Point(0, -1000d / 3), Point(60, -1000d / 3) }), 3d),
        ("distributed 10 N/mm x 100 mm", Vehicle("distributed", None, Line(-50, 50, -10)), 100d),
        ("point 500 N + distributed 5 N/mm x 100 mm", Vehicle("mixed", new[] { Point(0, -500) }, Line(-50, 50, -5)), 101d),
        ("two points 2 x 400 N + a third 200 N without the flag", Vehicle("partly flagged", new[] { Point(-50, -400), Point(50, -400), Point(0, -200, null) }), 2d),
    };

    [TestMethod]
    public void L10_TheCentrifugalOffsetOfAVehicleIsWrittenPerForceAndReadBack()
    {
        // Before: the writer wrote OffsetForce as R31's offset, which R31 applies to every flagged force (N per point force, N/mm of
        // distributed force; native test below): a vehicle of two points got twice the offset of GPC, a distributed one 100 times. Now the
        // writer divides by the measure of the vehicles and the reader multiplies back, exactly.
        foreach (var (name, vehicle, measure) in MixedVehicles())
        {
            var template = Template(Law(), vehicle);
            Assert.AreEqual(measure, Straus7CentrifugalExpression.OffsetMeasure(template, out _)!.Value, name);
            var written = Straus7CentrifugalExpression.FromDomain(template.Centrifugal!, null, template);
            Assert.AreEqual(measure == 1 ? "50" : "50/" + measure.ToString("R", System.Globalization.CultureInfo.InvariantCulture), written.OffsetExpression, name);
            Assert.AreEqual("0.1", written.FactorExpression, name);
            var back = written.ToDomain(template, out var record); Assert.IsNull(record, name);
            Assert.AreEqual(50, back.OffsetForce, name + ": exact round trip"); Assert.AreEqual(.1, back.Factor, name); Assert.AreEqual(MovingCentrifugalFactorKind.Constant, back.FactorKind, name);
            // The same law with a nonzero offset read without its template: the offset per force has no N per vehicle, so it stays in the dialect.
            var verbatim = written.ToDomain(out var kept);
            Assert.AreEqual(MovingCentrifugalFactorKind.AdapterDefined, verbatim.FactorKind, name); Assert.IsNotNull(kept, name);
        }
        // Several vehicles: one offset per template in R31, so their measures must agree; vehicles without flagged forces do not count.
        var same = Template(Law(), MixedVehicles()[1].Vehicle, Vehicle("other pair", new[] { Point(-80, -300), Point(80, -300) }), Vehicle("no flags", new[] { Point(0, -500, null) }));
        Assert.AreEqual("50/2", Straus7CentrifugalExpression.FromDomain(same.Centrifugal!, null, same).OffsetExpression);
        var different = Template(Law(), MixedVehicles()[0].Vehicle, MixedVehicles()[1].Vehicle);
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7CentrifugalExpression.FromDomain(different.Centrifugal!, null, different)).Message, "StrausCentrifugalOffsetNotPerVehicle");
        var leading = Template(Law(), Vehicle("lane", None, Line(-50, 50, -10, true, MovingDistributionMobility.Leading)));
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7CentrifugalExpression.FromDomain(leading.Centrifugal!, null, leading)).Message, "Leading");
        // Without an offset nothing is converted: any vehicles, any loaded length.
        foreach (var template in new[] { Template(Law(0), different.Vehicles), Template(Law(0), leading.Vehicles) })
            Assert.AreEqual("0", Straus7CentrifugalExpression.FromDomain(template.Centrifugal!, null, template).OffsetExpression);
        // The reader of a native template: an R31 offset per force (a constant) is the offset of the vehicle times its measure; with vehicles of
        // different measures it stays in the dialect (exact: written back verbatim).
        var native = new Straus7CentrifugalExpression { OffsetExpression = "50", FactorExpression = "0.1", LengthUnit = "mm", ForceUnit = "N" };
        Assert.AreEqual(100, native.ToDomain(Template(null, MixedVehicles()[1].Vehicle), out _).OffsetForce);
        Assert.AreEqual(5000, native.ToDomain(Template(null, MixedVehicles()[3].Vehicle), out _).OffsetForce);
        Assert.AreEqual(MovingCentrifugalFactorKind.AdapterDefined, native.ToDomain(Template(null, different.Vehicles), out var differentRecord).FactorKind);
        Assert.IsNotNull(differentRecord);
        // Declared units: kN per point force and kN per mm of distributed force (R31 takes the length of the model, mm, whatever the length
        // unit of the expression; checked natively below).
        var metric = new Straus7CentrifugalExpression { OffsetExpression = "0.05", FactorExpression = "0", LengthUnit = "m", ForceUnit = "kN" };
        Assert.AreEqual(5000, metric.ToDomain(Template(null, MixedVehicles()[3].Vehicle), out _).OffsetForce, 1e-9);
        Assert.AreEqual(100, metric.ToDomain(Template(null, MixedVehicles()[1].Vehicle), out _).OffsetForce, 1e-12);
        // The full export converts at Prepare (and rejects vehicles of different measures before any native call).
        var (model, path) = ReviewFixesL6Test.PathOnTheCantilever(different, MovingPathShape.Circular, new Point3d(500, -1.25, 0));
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(model)).Message, "StrausCentrifugalOffsetNotPerVehicle");
    }

    /// <summary>Forwards every call to the native writer; when <see cref="Offset"/> is set, writes that centrifugal offset text and those units
    /// instead of the writer's (what R31 receives from a template written otherwise).</summary>
    public class CentrifugalRewrite : DispatchProxy
    {
        internal Straus7NativeWriter Target = null!; internal string? Offset; internal int LengthUnit = -1, ForceUnit = -1; internal int Rewritten;
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == nameof(IStraus7AdvancedMovingWriteApi.SetMovingCentrifugal) && Offset != null)
            {
                args![1] = Offset; var flags = (int[])args[3]!; if (LengthUnit >= 0) flags[1] = LengthUnit; if (ForceUnit >= 0) flags[2] = ForceUnit; Rewritten++;
            }
            try { return method.Invoke(Target, args); }
            catch (TargetInvocationException error) { System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error.InnerException!).Throw(); throw; }
        }
    }

    /// <summary>Writes and solves the cantilever with the path converted to static cases; the horizontal reaction of the clamp (the radial force)
    /// in each fully loaded case, and the template read back from the model.</summary>
    private static (List<double> Radial, Straus7ResolvedMovingTemplate Read) Run(MovingLoadTemplate template, string name, string? offset = null, int lengthUnit = -1, int forceUnit = -1)
    {
        var (model, path) = ReviewFixesL6Test.PathOnTheCantilever(template, MovingPathShape.Circular, new Point3d(500, -1.25, 0));
        var plan = Straus7ModelWritePlan.Prepare(model, new Dictionary<Guid, Straus7MovingPathOptions> { { path.Id, new Straus7MovingPathOptions { ConvertToStaticCases = true } } });
        string folder = Straus7ExporterCorrectionsTest.NativeFolder(), output = Path.Combine(folder, name + ".st7"), result; IReadOnlyDictionary<int, string> generated;
        using (var writer = new Straus7NativeWriter(output, folder))
        {
            var api = DispatchProxy.Create<ReviewFixesL6Test.IForwardedWriter, CentrifugalRewrite>(); var proxy = (CentrifugalRewrite)(object)api;
            proxy.Target = writer; proxy.Offset = offset; proxy.LengthUnit = lengthUnit; proxy.ForceUnit = forceUnit;
            generated = plan.Write(api, model); if (offset != null) Assert.AreEqual(1, proxy.Rewritten, name);
            result = writer.Solve();
        }
        int clamp = model.NodesElements.Values.Single(n => n.Position.X == 0).Id; var radial = new List<double>(); Straus7ResolvedMovingTemplate read;
        using (var reader = new Straus7NativeApi())
        {
            reader.OpenModelReadOnly(output, folder); reader.OpenResults(result);
            foreach (int c in generated.Keys)
            {
                var r = reader.ReadNodeResult(clamp, c, true).Values;
                // Fully loaded positions only (every vehicle of the tests has 1000 N vertical in total).
                if (Math.Abs(Math.Abs(r[2]) - 1000) < 1e-6) radial.Add(Math.Sqrt(r[0] * r[0] + r[1] * r[1]));
            }
            read = reader.ReadExpandedMovingTemplate(1);
        }
        return (radial, read);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void L10_NativeCentrifugalOffsetIsTheRadialForceOfTheVehicle()
    {
        // The cantilever and circular path of the L4/L6 tests (R = 100000.625 mm), static cases. GPC law 50 N per vehicle + 0.1 × vertical:
        // every vehicle gets 150 N whatever its forces, and the template read back from the .st7 gives the same law exactly. Tolerance 1e-5: R31
        // spreads a distributed force on the curved path within 2e-6 of its resultant (the same with the offset written as before).
        var failures = new List<string>(); void Check(bool condition, string message) { if (!condition) failures.Add(message); }
        foreach (var (name, vehicle, measure) in MixedVehicles())
        {
            var template = Template(Law(), vehicle); double vertical = vehicle.PointForces.Where(f => f.ApplyCentrifugal == true).Sum(f => -f.Force.Z) + vehicle.DistributedForces.Sum(f => -f.ForcePerLength.Z * (f.X2 - f.X1));
            var (radial, read) = Run(template, "vehicle-" + measure.ToString(System.Globalization.CultureInfo.InvariantCulture) + "-" + vehicle.PointForces.Length);
            // Before (the writer's text was the offset itself): R31 applied it to every flagged force.
            var (before, _) = Run(template, "vehicle-before-" + measure.ToString(System.Globalization.CultureInfo.InvariantCulture) + "-" + vehicle.PointForces.Length, "50");
            double expected = 50 + .1 * vertical;
            Console.WriteLine($"L10 native centrifugal [{name}]: R31 {string.Join(", ", radial.Select(r => r.ToString("G9")))} N (expected {expected:G9}); " +
                $"offset written as before: {string.Join(", ", before.Select(r => r.ToString("G9")))} N; read back {read.Template.Centrifugal!.OffsetForce:R} N + {read.Template.Centrifugal.Factor:R} × V");
            Check(radial.Count >= 3, name + ": loaded cases " + radial.Count);
            foreach (double r in radial) Check(Math.Abs(r - expected) <= 1e-5 * expected, name + ": " + r);
            foreach (double r in before) Check(Math.Abs(r - (50 * measure + .1 * vertical)) <= 1e-5 * r, name + ": R31 applies its offset per flagged force, " + r);
            Check(read.Template.Centrifugal.OffsetForce == 50 && read.Template.Centrifugal.Factor == .1, name + ": read back exactly " + read.Template.Centrifugal.OffsetForce.ToString("R"));
            Check(read.PreservedData.Count == 0, name + ": no adapter record");
        }
        // Declared units of an R31 expression (a template written otherwise): kN per point force and kN per mm of distributed force. The reader
        // gives the radial force R31 applies.
        foreach (var (name, vehicle, _) in new[] { MixedVehicles()[3], MixedVehicles()[1] })
        {
            var template = Template(Law(50, 0), vehicle);
            var (radial, read) = Run(template, "metric-" + vehicle.Name.Replace(' ', '-'), "0.05", 0, 1);
            Console.WriteLine($"L10 native centrifugal [{name}] offset 0.05 in m and kN: R31 {string.Join(", ", radial.Select(r => r.ToString("G9")))} N; reader {read.Template.Centrifugal!.OffsetForce:R} N per vehicle");
            Check(radial.Count >= 3, name + ": loaded cases " + radial.Count);
            foreach (double r in radial) Check(Math.Abs(r - read.Template.Centrifugal.OffsetForce) <= 1e-5 * r, name + ": reader " + read.Template.Centrifugal.OffsetForce + " and R31 " + r);
        }
        Assert.AreEqual(0, failures.Count, string.Join("; ", failures));
    }
}
