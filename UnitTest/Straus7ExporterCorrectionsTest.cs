using System.Reflection;
using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.FreedomCases;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Restraints;
using GPC.Model.Sections;
using GPC.Model.Structure.Assignments;

namespace UnitTest;

/// <summary>Corrections of the Straus7 exporter review (REVISIONE 3/11, C1-C7). Native cases need GPC_STRAUS7_WRITE and a licence.</summary>
[TestClass]
public class Straus7ExporterCorrectionsTest
{
    public interface IRecordedWriter : IStraus7ModelWriteApi, IStraus7WriteApi, IStraus7DistributionWriteApi, IStraus7MaterialWriteApi,
        IStraus7SectionGeometryWriteApi, IStraus7SectionMeshWriteApi, IStraus7NativeSectionWriteApi, IStraus7InertiaWriteApi,
        IStraus7ConstraintWriteApi, IStraus7SurfacePressureWriteApi { }

    /// <summary>Records every native operation of a write plan without a Straus7 licence.</summary>
    public class Recorder : DispatchProxy
    {
        public readonly List<(string Name, object?[] Args)> Calls = new();
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            Calls.Add((method!.Name, (args ?? Array.Empty<object?>()).Select(a => a is Array array ? array.Clone() : a).ToArray()));
            return method.ReturnType == typeof(void) || !method.ReturnType.IsValueType ? null : Activator.CreateInstance(method.ReturnType);
        }
        public static (IRecordedWriter Api, Recorder Log) Create()
        {
            var api = DispatchProxy.Create<IRecordedWriter, Recorder>(); return (api, (Recorder)(object)api);
        }
        public (string Name, object?[] Args)[] Named(string name) => Calls.Where(c => c.Name == name).ToArray();
    }

    internal static Recorder Record(Model model)
    {
        var (api, log) = Recorder.Create(); Straus7ModelWritePlan.Prepare(model).Write(api, model); return log;
    }
    internal static string NativeFolder()
    {
        var root = Environment.GetEnvironmentVariable("GPC_STRAUS7_WRITE"); if (string.IsNullOrWhiteSpace(root)) Assert.Inconclusive("Set GPC_STRAUS7_WRITE to an artifact parent.");
        string folder = Path.Combine(Path.GetFullPath(root!), Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder); return folder;
    }
    private static List<DofRestrain> Dofs(params int[] dofs) => dofs.Select(i => new DofRestrain((GeometryRestrain.DOF)i, true)).ToList();

    /// <summary>Cantilever whose clamp is split into an unscoped translational support and a rotational one in the explicit freedom case.</summary>
    private static Model SplitClampCantilever(CoordinateSystem? rotationalFrame = null)
    {
        var model = Straus7ModelWriteTest.Cantilever(); var node = model.NodesElements.Values.Single(n => n.Position.X == 0);
        var fc = model.FreedomCases.Values.Single(); node.Assignments.Restrains.Clear();
        node.Assignments.Restrains.Add(new RestrainAssignment { Restrain = new NodeRestrain(node, Dofs(0, 1, 2)) });
        node.Assignments.Restrains.Add(new RestrainAssignment { FreedomCase = fc, Restrain = new NodeRestrain(node, Dofs(3, 4, 5)) { CoordinateSystem = rotationalFrame } });
        return model;
    }

    [TestMethod]
    public void C1_UnscopedAndExplicitRestraintsOfOneNativeCaseBecomeOneCall()
    {
        var model = SplitClampCantilever(); int node = model.NodesElements.Values.Single(n => n.Position.X == 0).Id;
        var calls = Record(model).Named(nameof(IStraus7ModelWriteApi.SetNodeRestraint)).Where(c => (int)c.Args[0]! == node).ToArray();
        Assert.AreEqual(1, calls.Length, "A second St7SetNodeRestraint6 for the same node/case replaces the first one.");
        Assert.AreEqual(1, (int)calls[0].Args[1]!); CollectionAssert.AreEqual(new[] { 1, 1, 1, 1, 1, 1 }, (int[])calls[0].Args[3]!);
    }

    [TestMethod]
    public void C1_ConflictingFramesOfOneNativeCaseAreRejected()
    {
        var rotated = new CoordinateSystem(new Point3d(0, 0, 0), new Vector3d(0, 1, 0), new Vector3d(-1, 0, 0));
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(SplitClampCantilever(rotated))).Message,
            "StrausNodeRequiresOneRestraintFramePerFreedomCase");
        // The same axes with another origin are the same native frame.
        var shifted = new CoordinateSystem(new Point3d(5, 5, 5), new Vector3d(1, 0, 0), new Vector3d(0, 1, 0));
        Assert.AreEqual(1, Record(SplitClampCantilever(shifted)).Named(nameof(IStraus7ModelWriteApi.SetNodeRestraint)).Length);
    }

    [TestMethod]
    public void C1_OverlappingDofOfUnscopedAndExplicitAssignmentsIsRejected()
    {
        var model = SplitClampCantilever(); var node = model.NodesElements.Values.Single(n => n.Position.X == 0);
        node.Assignments.Restrains.Add(new RestrainAssignment { FreedomCase = model.FreedomCases.Values.Single(), Restrain = new NodeRestrain(node, Dofs(0)) });
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => Straus7ModelWritePlan.Prepare(model)).Message, "OverlappingOrInvalidStrausRestraint");
    }

    [TestMethod]
    public void C1_RestraintSpringsAndGroundSpringOfOneCaseShareOneSpringCall()
    {
        var model = SplitClampCantilever(); var node = model.NodesElements.Values.Single(n => n.Position.X == 1000);
        node.Assignments.Restrains.Add(new RestrainAssignment { Restrain = new NodeRestrain(node, new List<DofRestrain> { new DofRestrain((GeometryRestrain.DOF)0, 100d) }) });
        var matrix = new double[36]; matrix[7] = 50; node.Assignments.GroundSprings.Add(new SpringMatrix(matrix, CoordinateSystem.Global));
        var springs = Record(model).Named(nameof(IStraus7ModelWriteApi.SetNodeSpring)).Where(c => (int)c.Args[0]! == node.Id).ToArray();
        Assert.AreEqual(1, springs.Length); CollectionAssert.AreEqual(new[] { 100d, 50d, 0d }, (double[])springs[0].Args[4]!, string.Join(",", (double[])springs[0].Args[4]!));
    }

    [TestMethod]
    public void C2_ShearAreasMatchElementaryShearFlowSolutions()
    {
        var rectangle = new SectionRectangular(200, 100); var (r1, r2) = SectionShearAreas.Compute(rectangle);
        Assert.AreEqual(5d / 6 * rectangle.Area, r1, 1e-9 * rectangle.Area); Assert.AreEqual(5d / 6 * rectangle.Area, r2, 1e-9 * rectangle.Area);
        var rotated = new SectionRectangular(200, 100, 0.3); var (q1, q2) = SectionShearAreas.Compute(rotated);
        Assert.AreEqual(5d / 6 * rotated.Area, q1, 1e-9 * rotated.Area); Assert.AreEqual(5d / 6 * rotated.Area, q2, 1e-9 * rotated.Area);
        var circle = new SectionCircular(100); var (c1, c2) = SectionShearAreas.Compute(circle); // 9/10 A
        Assert.AreEqual(.9 * circle.Area, c1, .01 * circle.Area); Assert.AreEqual(c1, c2, .01 * circle.Area);
        // Thin tube: the chord method gives 2/3 A (walls crossed obliquely); the thin-walled shear-flow value is A/2 (declared difference).
        var pipe = new SectionCHS(500, 5); var (p1, _) = SectionShearAreas.Compute(pipe);
        Assert.AreEqual(2d / 3 * pipe.Area, p1, .02 * pipe.Area);
        var h = new SectionH(300, 10, 200, 15, 200, 15, "I"); var (h1, h2) = SectionShearAreas.Compute(h);
        // Axis 1 is the strong axis: shear along it is carried by the flanges, shear along axis 2 by the web.
        Assert.IsTrue(h1 > 5d / 6 * 2 * 200 * 15 && h1 < 2 * 200 * 15, h1.ToString("R"));
        Assert.IsTrue(h2 > (300 - 30) * 10 && h2 < 1.1 * 300 * 10, h2.ToString("R"));
    }

    private static Model SteelCantilever(ISectionShape shape)
    {
        var model = Straus7ModelWriteTest.Cantilever(); var steel = new GPC.Model.Sections.Steel.SteelSection(shape, GPC.Model.Data.Steel.SteelMaterialEN1993Data.S355);
        model.BeamElements.Values.Single().BeamProperty = steel; return model;
    }

    [TestMethod]
    public void C2_SteelSectionSendsGpcShearAreasAndElasticPropertyKeepsExplicitOnes()
    {
        var shape = new SectionRectangular(200, 100); var section = (double[])Record(SteelCantilever(shape)).Named(nameof(IStraus7ModelWriteApi.CreateBeamProperty)).Single().Args[3]!;
        Assert.AreEqual(5d / 6 * shape.Area, section[6], 1e-9 * shape.Area); Assert.AreEqual(5d / 6 * shape.Area, section[7], 1e-9 * shape.Area);
        var elastic = (double[])Record(Straus7ModelWriteTest.Cantilever()).Named(nameof(IStraus7ModelWriteApi.CreateBeamProperty)).Single().Args[3]!;
        Assert.AreEqual(0, elastic[6]); Assert.AreEqual(0, elastic[7]); // explicit ElasticBeamProperty contract: 0 = no shear deformation
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C2_NativeSteelCantileverMatchesTimoshenkoDeflection()
    {
        var shape = new SectionRectangular(200, 100); var model = SteelCantilever(shape); string folder = NativeFolder();
        var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, Path.Combine(folder, "timoshenko.st7"), folder);
        var material = ((GPC.Model.Sections.Steel.SteelSection)model.BeamElements.Values.Single().BeamProperty).SteelMaterial;
        double e = material.E, g = material.GetShearModule(), l = 1000, p = 1000;
        // Load along the section axis 2 (global Z): bending about axis 1 and shear area 5/6 A.
        double expected = -(p * l * l * l / (3 * e * shape.J11) + p * l / (g * 5d / 6 * shape.Area));
        int freeId = model.NodesElements.Values.Single(n => n.Position.X == 1000).Id;
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(run.ModelPath, folder); reader.OpenResults(run.ResultPath);
        Assert.AreEqual(expected, reader.ReadNodeResult(freeId, 1, false).Values[2], 1e-7 * Math.Abs(expected));
        Assert.AreEqual(5d / 6 * shape.Area, reader.ReadProperty(Straus7Entity.Beam, 1).SectionProperties[7], 1e-9 * shape.Area);
    }

    /// <summary>Mono-symmetric I: wide thick bottom flange, narrow thin top flange, thin web.</summary>
    private static SectionH MonoI() => new SectionH(500, 5, 100, 10, 300, 20, "mono I");

    [TestMethod]
    public void C3_ISectionDimensionsFollowStraus7Order()
    {
        var model = Straus7ModelWriteTest.Cantilever(); var beam = model.BeamElements.Values.Single();
        beam.BeamProperty = new ElasticBeamProperty("I", new Material("E", 210000, .3, 0, 0), MonoI());
        var call = Record(model).Named(nameof(IStraus7SectionGeometryWriteApi.SetBeamSectionGeometry)).Single();
        // bsISection: B1 bottom flange, B2 top flange, D, T1 bottom flange, T2 top flange, T3 web (as the Converter importer).
        Assert.AreEqual(7, (int)call.Args[1]!); CollectionAssert.AreEqual(new[] { 300d, 100, 500, 20, 10, 5 }, (double[])call.Args[2]!);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C3_NativeISectionGeometryReproducesGpcProperties()
    {
        var section = MonoI(); string folder = NativeFolder();
        var resolved = Straus7SectionLibrary.Resolve(Straus7SectionDefinition.Standard(7, new[] { 300d, 100, 500, 20, 10, 5 }), folder).ReferenceProperties;
        Assert.AreEqual(section.Area, resolved[0], 1e-6 * section.Area, "A"); Assert.AreEqual(section.J11, resolved[1], 1e-6 * section.J11, "I11");
        Assert.AreEqual(section.J22, resolved[2], 1e-6 * section.J22, "I22");
        Console.WriteLine("C3 shape 7 native properties: " + string.Join(", ", resolved.Select(v => v.ToString("R"))));
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C3_NativeTaperedISectionMatchesIntegratedAxialCompliance()
    {
        var model = Straus7ModelWriteTest.Cantilever(); var beam = model.BeamElements.Values.Single(); var free = beam.NodeJ;
        var lc = model.LoadCases.Values.Single(); var fc = model.FreedomCases.Values.Single(); var section = MonoI();
        beam.BeamProperty = new ElasticBeamProperty("tapered I", new Material("E1000", 1000, .3, 0, 0), section);
        beam.Assignments.Physical.Taper = new BeamSectionTaper { ScaleI1 = 1, ScaleJ1 = 1, ScaleI2 = 1, ScaleJ2 = 2, Alignment1 = TaperAlignment.Symmetric, Alignment2 = TaperAlignment.Symmetric };
        free.Loads.Clear(); free.Loads.Add(new GPC.Model.Loads.PointLoad(1000, 0, 0, 0, 0, 0, free.Position, lc));
        free.Assignments.Restrains.Add(new RestrainAssignment { FreedomCase = fc, Restrain = new NodeRestrain(free, Dofs(1, 2, 3, 4, 5)) });
        var plan = Straus7ModelWritePlan.Prepare(model, null, new Dictionary<Guid, int> { { beam.Guid, 10 } }); string folder = NativeFolder();
        var run = plan.RunLinearStatic(model, Path.Combine(folder, "tapered-i.st7"), folder);
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(run.ModelPath, folder);
        var property = reader.ReadProperty(Straus7Entity.Beam, 1); Assert.AreEqual(7, property.SectionType);
        CollectionAssert.AreEqual(new[] { 300d, 100, 500, 20, 10, 5 }, property.Geometry);
        reader.OpenResults(run.ResultPath);
        // Depth doubles along the beam with constant plate thicknesses: A(x) = A0 + T3 D0 x/L.
        double a0 = section.Area, a1 = a0 + 5 * 500, expected = 1000 * 1000 / (1000 * (a1 - a0)) * Math.Log(a1 / a0);
        Assert.AreEqual(expected, reader.ReadNodeResult(free.Id, 1, false).Values[0], expected * 1e-4);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C1_NativeSplitClampKeepsAllSixRestraints()
    {
        var model = SplitClampCantilever(); string folder = NativeFolder();
        var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, Path.Combine(folder, "split-clamp.st7"), folder);
        int fixedId = model.NodesElements.Values.Single(n => n.Position.X == 0).Id, freeId = model.NodesElements.Values.Single(n => n.Position.X == 1000).Id;
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(run.ModelPath, folder);
        CollectionAssert.AreEqual(new[] { 1, 1, 1, 1, 1, 1 }, reader.ReadRestraint(fixedId, 1).Status);
        reader.OpenResults(run.ResultPath);
        var property = (ElasticBeamProperty)model.BeamElements.Values.Single().BeamProperty; double ei = property.Material.E * property.Section.J11;
        Assert.AreEqual(-1000 * Math.Pow(1000, 3) / (3 * ei), reader.ReadNodeResult(freeId, 1, false).Values[2], 1e-7 * 1000 * Math.Pow(1000, 3) / (3 * ei));
    }
}

[TestClass]
public class Straus7ExporterSessionCorrectionsTest
{
    [TestMethod, TestCategory("Straus7Native")]
    public void C5_ReaderAfterWriterUsesItsOwnDelegateForTheSameFunction()
    {
        var model = Straus7ModelWriteTest.Cantilever(); string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        using var writer = new Straus7NativeWriter(Path.Combine(folder, "delegates.st7"), folder);
        Straus7ModelWritePlan.Prepare(model).Write(writer, model); // SetBeamAxes binds St7GetBeamAxisSystemInitial with the writer signature
        var element = writer.Reader.ReadElement(Straus7Entity.Beam, 1); // the reader binds it with its own signature
        Assert.AreEqual(1, element.InitialAxes[1], 1e-12);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C5_ChangedExportFilesAreRejectedNotThrown()
    {
        var model = Straus7ModelWriteTest.Cantilever(); string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, Path.Combine(folder, "changed.st7"), folder);
        File.AppendAllText(run.ResultPath, " ");
        var report = run.ImportResults(model, "changed");
        Assert.AreEqual(ImportStatus.Rejected, report.Status); StringAssert.Contains(report.Diagnostics.Single().Message, "StrausExportFilesChanged");
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C5_ConcurrentSessionsCannotShareTheNativeRuntime()
    {
        Straus7ExporterCorrectionsTest.NativeFolder();
        const int threads = 4; using var start = new Barrier(threads); var sessions = new System.Collections.Concurrent.ConcurrentBag<Straus7NativeApi>();
        var refused = new System.Collections.Concurrent.ConcurrentBag<Exception>();
        var workers = Enumerable.Range(0, threads).Select(_ => new Thread(() =>
        {
            start.SignalAndWait();
            try { sessions.Add(new Straus7NativeApi()); } catch (InvalidOperationException ex) { refused.Add(ex); }
        })).ToArray();
        foreach (var w in workers) w.Start(); foreach (var w in workers) w.Join();
        Assert.AreEqual(1, sessions.Count); Assert.AreEqual(threads - 1, refused.Count);
        sessions.Single().Dispose();
        using (var again = new Straus7NativeApi()) Assert.IsNotNull(again.Version);
    }
}
