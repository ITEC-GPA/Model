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

    /// <summary>Energy shear coefficient of the solid circle from the exact Saint-Venant flexure stresses (Timoshenko–Goodier):
    /// τ_y = α(R² − y² − βx²), τ_x = −γxy; k = 1/(16 T) with T = α'²(5/8 − 5β/12 + β²/8) + γ'²/24 (R = 1). 6/7 for ν = 0.</summary>
    internal static double CircleEnergyCoefficient(double nu)
    {
        double alpha = (3 + 2 * nu) / (8 * (1 + nu)), beta = (1 - 2 * nu) / (3 + 2 * nu), gamma = (1 + 2 * nu) / (4 * (1 + nu));
        return 1 / (16 * (alpha * alpha * (5d / 8 - 5 * beta / 12 + beta * beta / 8) + gamma * gamma / 24));
    }

    [TestMethod]
    public void C2_FiniteElementShearAreasMatchSaintVenantFlexureSolutions()
    {
        // ν = 0: every rectangle has k = 5/6 (energy and Cowper coincide); rotation of the outline does not matter.
        foreach (var rectangle in new[] { new SectionRectangular(200, 100), new SectionRectangular(100, 100), new SectionRectangular(200, 100, 0.3) })
        {
            var (r1, r2) = SectionShearAreas.Compute(rectangle, 0);
            Assert.AreEqual(5d / 6 * rectangle.Area, r1, 2e-4 * rectangle.Area); Assert.AreEqual(5d / 6 * rectangle.Area, r2, 2e-4 * rectangle.Area);
        }
        // Solid circle against the exact flexure solution, with and without the Poisson term.
        var circle = new SectionCircular(100);
        foreach (double nu in new[] { 0, .3 })
        {
            var (c1, c2) = SectionShearAreas.Compute(circle, nu);
            Assert.AreEqual(CircleEnergyCoefficient(nu) * circle.Area, c1, 1e-3 * circle.Area, "ν=" + nu); Assert.AreEqual(c1, c2, 1e-3 * circle.Area);
        }
        // Thin tube: A/2 (the chord method of the first correction gave 2/3 A).
        var pipe = new SectionCHS(500, 5); var (p1, p2) = SectionShearAreas.Compute(pipe, .3);
        Assert.AreEqual(.5 * pipe.Area, p1, .005 * pipe.Area); Assert.AreEqual(p1, p2, 1e-6 * pipe.Area);
        // Axis 1 is the strong axis: shear along it is carried by the flanges, shear along axis 2 by the web.
        var h = new SectionH(300, 10, 200, 15, 200, 15, "I"); var (h1, h2) = SectionShearAreas.Compute(h, .3);
        Assert.IsTrue(h1 > 5d / 6 * 2 * 200 * 15 && h1 < 2 * 200 * 15, h1.ToString("R"));
        Assert.IsTrue(h2 > (300 - 30) * 10 && h2 < 1.1 * 300 * 10, h2.ToString("R"));
        // Cowper's rectangle k = 10(1+ν)/(12+11ν) is not an energy coefficient: for ν ≠ 0 the two definitions differ (reported, not asserted).
        var square = new SectionRectangular(100, 100); var (s1, _) = SectionShearAreas.Compute(square, .3);
        Console.WriteLine("C2 square ν=0.3: energy k=" + (s1 / square.Area).ToString("F4") + ", Cowper k=" + (13 / 15.3).ToString("F4"));
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C2_NativeShearAreasAgreeWithStraus7ForStandardSections()
    {
        string folder = NativeFolder();
        var cases = new (string Name, ISectionShape Section, int Shape, double[] Dimensions)[] {
            ("rectangle 100x200", new SectionRectangular(200, 100), 3, new[] { 100d, 200, 0, 0, 0, 0 }),
            ("circle 100", new SectionCircular(100), 1, new[] { 100d, 0, 0, 0, 0, 0 }),
            ("CHS 500x5", new SectionCHS(500, 5), 2, new[] { 500d, 0, 0, 5, 0, 0 }),
            ("I 300/100x500", new SectionH(500, 5, 100, 10, 300, 20, "I mono"), 7, new[] { 300d, 100, 500, 20, 10, 5 }),
            ("I 200x300", new SectionH(300, 10, 200, 15, 200, 15, "I sym"), 7, new[] { 200d, 200, 300, 15, 15, 10 }),
            ("RHS 300x200", new SectionRHS(300, 200, 12, 12, 8, 8, "RHS"), 4, new[] { 200d, 300, 0, 12, 8, 0 }) };
        foreach (var c in cases)
        {
            var native = Straus7SectionLibrary.Resolve(Straus7SectionDefinition.Standard(c.Shape, c.Dimensions), folder).ReferenceProperties;
            var free = SectionShearAreas.Compute(c.Section, 0); var steel = SectionShearAreas.Compute(c.Section, .3);
            Console.WriteLine($"C2 {c.Name}: Straus7 {native[6]:F1} / {native[7]:F1}; GPC ν=0 {free.Area1:F1} / {free.Area2:F1}; GPC ν=0.3 {steel.Area1:F1} / {steel.Area2:F1}");
            // R31 computes its standard-section shear areas without the Poisson term: compare at ν = 0.
            Assert.AreEqual(native[6], free.Area1, .005 * native[6], c.Name + " SA1"); Assert.AreEqual(native[7], free.Area2, .005 * native[7], c.Name + " SA2");
        }
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
        // Computed with the ν of the steel material (0.3).
        var expected = SectionShearAreas.Compute(shape, .3); Assert.AreEqual(expected.Area1, section[6]); Assert.AreEqual(expected.Area2, section[7]);
        Assert.AreEqual(5d / 6 * shape.Area, section[7], 1e-3 * shape.Area); Assert.IsTrue(section[6] < .8 * shape.Area, section[6].ToString("R"));
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
        // Load along the section axis 2 (global Z): bending about axis 1 and the GPC shear area along axis 2 (≈ 5/6 A for ν = 0.3).
        double shearArea = SectionShearAreas.Compute(shape, material.Ni).Area2;
        double expected = -(p * l * l * l / (3 * e * shape.J11) + p * l / (g * shearArea));
        int freeId = model.NodesElements.Values.Single(n => n.Position.X == 1000).Id;
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(run.ModelPath, folder); reader.OpenResults(run.ResultPath);
        Assert.AreEqual(expected, reader.ReadNodeResult(freeId, 1, false).Values[2], 1e-7 * Math.Abs(expected));
        Assert.AreEqual(shearArea, reader.ReadProperty(Straus7Entity.Beam, 1).SectionProperties[7], 1e-9 * shape.Area);
        Assert.AreEqual(5d / 6 * shape.Area, shearArea, 1e-3 * shape.Area);
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

[TestClass]
public class Straus7MaterialLawCorrectionsTest
{
    private static MaterialAnalysisData Law(MaterialNonlinearResponse response, MaterialHardening hardening) => new MaterialAnalysisData {
        StressStrainPairs = new[] { 0d, 0, .001, 210, .1, 300 }, NonlinearResponse = response, Hardening = hardening };

    internal static Model Plate(MaterialAnalysisData? law)
    {
        var nodes = new[] { new NodeElement(new Point3d(0, 0, 0)), new NodeElement(new Point3d(100, 0, 0)), new NodeElement(new Point3d(100, 100, 0)), new NodeElement(new Point3d(0, 100, 0)) };
        var plate = new AreaElement(null, new ElasticPlateProperty("law plate", new Material("S", 210000, .3, 0, 0) { AnalysisData = law }, 10, 10)); plate.ConnectNodes(nodes);
        var model = ModelAssembly.Assemble(new Element[] { plate }, Guid.NewGuid()); model.AddLoadCase(new GPC.Model.LoadCases.LoadCaseBase("empty")); return model;
    }
    private static Model Beam(MaterialAnalysisData? law)
    {
        var model = Straus7ModelWriteTest.Cantilever(); var beam = model.BeamElements.Values.Single();
        beam.BeamProperty = new ElasticBeamProperty("law beam", new Material("S", 210000, .3, 0, 0) { AnalysisData = law }, new SectionRectangular(10, 10)); return model;
    }
    private static (int Response, int Criterion) Native(Model model, int family)
    {
        string folder = Straus7ExporterCorrectionsTest.NativeFolder(), path = Path.Combine(folder, "law.st7");
        using (var writer = new Straus7NativeWriter(path, folder)) { Straus7ModelWritePlan.Prepare(model).Write(writer, model); writer.Save(); }
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(path, folder);
        int response = reader.ReadNonlinearType(family, 1, out int criterion); return (response, criterion);
    }

    [TestMethod]
    public void C5_PlateHardeningOtherThanIsotropicIsRejectedBeforeWriting()
    {
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(Plate(Law(MaterialNonlinearResponse.Elastoplastic, MaterialHardening.Kinematic)))).Message,
            "PlateHardeningNotSupportedByStraus7Api");
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C5_NativeMaterialLawsKeepTheNativeYieldCriterion()
    {
        var plainBeam = Native(Beam(null), 1); var plainPlate = Native(Plate(null), 2); Console.WriteLine("C5 native defaults: beam " + plainBeam + ", plate " + plainPlate);
        var beam = Native(Beam(Law(MaterialNonlinearResponse.Elastoplastic, MaterialHardening.Isotropic)), 1);
        Assert.AreEqual(1, beam.Response); Assert.AreEqual(plainBeam.Criterion, beam.Criterion, "beam yield criterion");
        var plate = Native(Plate(Law(MaterialNonlinearResponse.NonlinearElastic, MaterialHardening.Isotropic)), 2);
        Assert.AreEqual(0, plate.Response, "the GPC response is written"); Assert.AreEqual(plainPlate.Criterion, plate.Criterion, "plate yield criterion");
        var elastoplastic = Native(Plate(Law(MaterialNonlinearResponse.Elastoplastic, MaterialHardening.Isotropic)), 2);
        Assert.AreEqual(1, elastoplastic.Response); Assert.AreEqual(plainPlate.Criterion, elastoplastic.Criterion, "plate yield criterion");
        Console.WriteLine("C5 native defaults: beam " + plainBeam + ", plate " + plainPlate);
    }
}

[TestClass]
public class Straus7OrientationCorrectionsTest
{
    /// <summary>Rotated rectangle (principal axis 1 at <paramref name="angle"/> from the section x axis = global Y), load along section y = global Z.</summary>
    [DataTestMethod, DataRow(.3), DataRow(-.3), TestCategory("Straus7Native")]
    public void C5_NativePrincipalAngleCouplesBendingAsTheSectionGeometry(double angle)
    {
        var model = Straus7ModelWriteTest.Cantilever(); var beam = model.BeamElements.Values.Single(); var section = new SectionRectangular(200, 100, angle);
        Assert.AreEqual(angle, section.AngleX1, 1e-12);
        beam.BeamProperty = new ElasticBeamProperty("rotated", new Material("E", 210000, .3, 0, 0), section);
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, Path.Combine(folder, "angle.st7"), folder);
        int free = model.NodesElements.Values.Single(n => n.Position.X == 1000).Id;
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(run.ModelPath, folder); reader.OpenResults(run.ResultPath);
        var u = reader.ReadNodeResult(free, 1, false).Values;
        double p = -1000, k = Math.Pow(1000, 3) / (3 * 210000), c = Math.Cos(angle), s = Math.Sin(angle);
        // d = (F.e1/J22) e1 + (F.e2/J11) e2 with e1 = (c, s), e2 = (-s, c) in (section x, section y) and F = (0, p).
        double dx = k * (p * s / section.J22 * c - p * c / section.J11 * s), dy = k * (p * s / section.J22 * s + p * c / section.J11 * c);
        Assert.AreEqual(dx, u[1], 1e-7 * Math.Abs(dy), "global Y (section x)"); Assert.AreEqual(dy, u[2], 1e-7 * Math.Abs(dy), "global Z (section y)");
    }
}

[TestClass]
public class Straus7WriterFieldsCorrectionsTest
{
    private static Model WithCombination()
    {
        var model = Straus7ModelWriteTest.Cantilever(); var lc = model.LoadCases.Values.Single();
        var combination = new GPC.Model.Combinations.Combination("ULS");
        combination.AddLoadCaseCoefficients(new List<GPC.Model.LoadCases.LoadCaseBase> { lc }, new List<double> { 1.35 }); model.AddCombination(combination); return model;
    }

    [TestMethod]
    public void C6_UserIdsAndUnstagedNonlinearIncrementsAreWrittenAsLegacy()
    {
        var model = WithCombination(); var log = Straus7ExporterCorrectionsTest.Record(model);
        var ids = log.Named(nameof(IStraus7ModelWriteApi.SetEntityUserId)).Select(c => ((int)c.Args[0]!, (int)c.Args[1]!, (int)c.Args[2]!)).ToArray();
        foreach (var node in model.NodesElements.Values) Assert.IsTrue(ids.Any(i => i.Item1 == 0 && i.Item3 == node.Id), "node " + node.Id);
        Assert.IsTrue(ids.Any(i => i.Item1 == 1 && i.Item3 == model.BeamElements.Values.Single().Id));
        var increment = log.Named(nameof(IStraus7ModelWriteApi.AddNonlinearIncrement)).Single();
        Assert.AreEqual("ULS", increment.Args[0]); CollectionAssert.AreEqual(new[] { 1.35 }, (double[])increment.Args[2]!);
    }

    [TestMethod]
    public void C6_LegacyStageCombinationMatrixWithoutStagesIsRejected()
    {
        var model = WithCombination(); model.StageCombinationsMap[1] = new HashSet<string> { "ULS" };
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(model)).Message, "LegacyStageCombinationsMapWithoutStagesIsNotExported");
    }

    [TestMethod]
    public void C6_LegacyScalarRotationDoesNotOverrideTheAuthoritativeSectionAxes()
    {
        var model = Straus7ModelWriteTest.Cantilever(); var reference = (double[])Straus7ExporterCorrectionsTest.Record(model).Named(nameof(IStraus7ModelWriteApi.SetBeamAxes)).Single().Args[1]!;
        model.BeamElements.Values.Single().RotationAroundFirstAxis = .5;
        CollectionAssert.AreEqual(reference, (double[])Straus7ExporterCorrectionsTest.Record(model).Named(nameof(IStraus7ModelWriteApi.SetBeamAxes)).Single().Args[1]!);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C6_NativeUserIdsAndNonlinearIncrementsReadBack()
    {
        var model = WithCombination(); string folder = Straus7ExporterCorrectionsTest.NativeFolder(), path = Path.Combine(folder, "ids.st7");
        var plan = Straus7ModelWritePlan.Prepare(model);
        using (var writer = new Straus7NativeWriter(path, folder)) { plan.Write(writer, model); writer.Save(); }
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(path, folder);
        foreach (var entry in plan.IdentityMap.Entries.Where(e => e.Family == GPC.Model.Core.Identity.EntityFamily.Node))
            Assert.AreEqual(model.NodesElements.Values.Single(n => n.Guid == entry.EntityGuid).Id, reader.ReadNode(entry.SolverId).UserId);
        int increments = reader.ReadStageIncrementCount(0); Assert.AreEqual("ULS", reader.ReadStageIncrementName(0, increments));
        Assert.AreEqual(1.35, reader.ReadStageFactor(0, increments, 1, false), 1e-12);
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C6_ScratchSectionModelsAreRemoved()
    {
        string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        Straus7SectionLibrary.Resolve(Straus7SectionDefinition.Standard(7, new[] { 300d, 100, 500, 20, 10, 5 }), folder);
        Assert.AreEqual(0, Directory.GetFiles(folder, "section-resolve-*").Length, string.Join(", ", Directory.GetFiles(folder)));
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C6_FailedRunLeavesNoPartialOutputAndCanBeRetried()
    {
        var model = Straus7ModelWriteTest.Cantilever(); string folder = Straus7ExporterCorrectionsTest.NativeFolder(), path = Path.Combine(folder, "retry.st7");
        File.WriteAllText(Path.Combine(folder, "retry.keep"), "user file with the same stem");
        // A native failure after the model file was created: an unrestrained cantilever cannot be solved.
        var free = Straus7ModelWriteTest.Cantilever(); foreach (var node in free.NodesElements.Values) node.Assignments.Restrains.Clear();
        Exception? failure = null; try { Straus7ModelWritePlan.Prepare(free).RunLinearStatic(free, path, folder); } catch (Exception ex) { failure = ex; }
        Assert.IsNotNull(failure, "the unrestrained solve must fail"); Console.WriteLine("C6 native failure: " + failure!.Message);
        CollectionAssert.AreEquivalent(new[] { "retry.keep" }, Directory.GetFiles(folder, "retry.*").Select(Path.GetFileName).ToArray());
        var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, path, folder); Assert.IsTrue(File.Exists(run.ResultPath));
        Assert.IsTrue(File.Exists(Path.Combine(folder, "retry.keep")));
    }
}

[TestClass]
public class Straus7NativeCoverageTest
{
    private static List<DofRestrain> Dofs(params int[] dofs) => dofs.Select(i => new DofRestrain((GeometryRestrain.DOF)i, true)).ToList();
    private const double E = 210000, L = 1000;
    private static double J11 => new SectionRectangular(200, 100).J11;
    private static double J22 => new SectionRectangular(200, 100).J22;

    [TestMethod, TestCategory("Straus7Native")]
    public void C7_RestraintInRotatedFramePropsTheTipAlongItsAxis()
    {
        var model = Straus7ModelWriteTest.Cantilever(); var tip = model.NodesElements.Values.Single(n => n.Position.X == L); double a = .5;
        var frame = new CoordinateSystem(tip.Position, new Vector3d(0, Math.Cos(a), Math.Sin(a)), new Vector3d(0, -Math.Sin(a), Math.Cos(a)));
        tip.Assignments.Restrains.Add(new RestrainAssignment { FreedomCase = model.FreedomCases.Values.Single(), Restrain = new NodeRestrain(tip, Dofs(0)) { CoordinateSystem = frame } });
        string folder = Straus7ExporterCorrectionsTest.NativeFolder(); var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, Path.Combine(folder, "ucs.st7"), folder);
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(run.ModelPath, folder);
        var restraint = reader.ReadRestraint(tip.Id, 1); Assert.AreNotEqual(1, restraint.Ucs);
        var ucs = reader.ReadUcs(restraint.Ucs).Data; Assert.AreEqual(Math.Cos(a), ucs[4], 1e-12); Assert.AreEqual(Math.Sin(a), ucs[5], 1e-12);
        reader.OpenResults(run.ResultPath); var u = reader.ReadNodeResult(tip.Id, 1, false).Values;
        // Tip flexibilities: Y (section x) L^3/3EJ22, Z (section y) L^3/3EJ11; the prop removes the motion along d = (0, cos a, sin a).
        double fy = L * L * L / (3 * E * J22), fz = L * L * L / (3 * E * J11), dy = Math.Cos(a), dz = Math.Sin(a), p = -1000;
        double r = -p * dz * fz / (dy * dy * fy + dz * dz * fz), uy = fy * r * dy, uz = fz * (p + r * dz);
        Assert.AreEqual(0, dy * u[1] + dz * u[2], 1e-9); Assert.AreEqual(uy, u[1], 1e-7 * Math.Abs(uz)); Assert.AreEqual(uz, u[2], 1e-7 * Math.Abs(uz));
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C7_RotationalSpringAddsRigidRotationAtTheBase()
    {
        var model = Straus7ModelWriteTest.Cantilever(); var root = model.NodesElements.Values.Single(n => n.Position.X == 0); double k = 2e9;
        var dofs = Dofs(0, 1, 2, 3, 5); dofs.Add(new DofRestrain((GeometryRestrain.DOF)4, k));
        root.Assignments.Restrains.Clear(); root.Assignments.Restrains.Add(new RestrainAssignment { FreedomCase = model.FreedomCases.Values.Single(), Restrain = new NodeRestrain(root, dofs) });
        string folder = Straus7ExporterCorrectionsTest.NativeFolder(); var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, Path.Combine(folder, "spring.st7"), folder);
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(run.ModelPath, folder);
        Assert.AreEqual(k, reader.ReadNodeStiffness(root.Id, 1, true).Values[1], 1e-6 * k);
        reader.OpenResults(run.ResultPath); int tip = model.NodesElements.Values.Single(n => n.Position.X == L).Id;
        double expected = -1000 * (L * L * L / (3 * E * J11) + L * L / k); // N mm/rad
        Assert.AreEqual(expected, reader.ReadNodeResult(tip, 1, false).Values[2], 1e-7 * Math.Abs(expected));
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C7_StandardRhsGeometryReproducesGpcProperties()
    {
        var model = Straus7ModelWriteTest.Cantilever(); var beam = model.BeamElements.Values.Single(); var box = new SectionRHS(300, 200, 12, 12, 8, 8, "RHS");
        beam.BeamProperty = new ElasticBeamProperty("RHS", new Material("E", E, .3, 0, 0), box);
        var call = Straus7ExporterCorrectionsTest.Record(model).Named(nameof(IStraus7SectionGeometryWriteApi.SetBeamSectionGeometry)).Single();
        Assert.AreEqual(4, (int)call.Args[1]!); var dims = (double[])call.Args[2]!;
        string folder = Straus7ExporterCorrectionsTest.NativeFolder(); var native = Straus7SectionLibrary.Resolve(Straus7SectionDefinition.Standard(4, dims), folder).ReferenceProperties;
        Assert.AreEqual(box.Area, native[0], 1e-6 * box.Area, "A " + string.Join(",", dims)); Assert.AreEqual(box.J11, native[1], 1e-6 * box.J11, "I11"); Assert.AreEqual(box.J22, native[2], 1e-6 * box.J22, "I22");
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C7_ImportedBeamForcesKeepTheirSign()
    {
        var model = Straus7ModelWriteTest.Cantilever(); string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, Path.Combine(folder, "sign.st7"), folder);
        Assert.AreEqual(ImportStatus.Completed, run.ImportResults(model, "sign").Status);
        var samples = model.BeamElements.Values.Single().Results.SelectMany(r => r.Results).OfType<GPC.Model.Results.Locations.StationResultBeamForces>().ToArray();
        var root = samples.Single(s => s.ParametricDistance == 0).ResultBeamForces;
        // Positive section face at the clamp of a cantilever loaded by -1000 N along section axis 2 (global Z) at the tip:
        // the outer part acts with F = -1000 N on V2 and M = L ex x F = +1e6 N mm about V1 (global Y).
        Assert.AreEqual(1e6, root.M1, 1e-3); Assert.AreEqual(-1000, root.V2, 1e-6); Assert.AreEqual(0, root.M2, 1e-6);
    }
}

[TestClass]
public class Straus7PropertySharingTest
{
    private static Model TwoBeams(BeamProperty first, BeamProperty second)
    {
        var nodes = new[] { new NodeElement(new Point3d(0, 0, 0)), new NodeElement(new Point3d(500, 0, 0)), new NodeElement(new Point3d(1000, 0, 0)) };
        nodes[0].Assignments.Restrains.Add(new RestrainAssignment { Restrain = new NodeRestrain(nodes[0], Enumerable.Range(0, 6).Select(i => new DofRestrain((GeometryRestrain.DOF)i, true)).ToList()) });
        var beams = new[] { (nodes[0], nodes[1], first), (nodes[1], nodes[2], second) }.Select(p =>
        {
            var beam = new BeamElement(p.Item1.Position, p.Item2.Position, p.Item3); beam.ConnectNodes(p.Item1, p.Item2);
            beam.Assignments.Formulation = BeamFormulation.StraightTwoNode;
            beam.Assignments.SectionAxes = new CoordinateSystem(p.Item1.Position, new Vector3d(0, 1, 0), new Vector3d(0, 0, 1)); return (Element)beam;
        }).ToArray();
        nodes[2].Loads.Add(new GPC.Model.Loads.PointLoad(0, 0, -1000, 0, 0, 0, nodes[2].Position, new GPC.Model.LoadCases.LoadCaseBase("P")));
        return ModelAssembly.Assemble(beams, Guid.NewGuid());
    }
    private static ElasticBeamProperty Rotated(string name) => new ElasticBeamProperty(name, new Material("S355 steel", 210000, .3, 7.85e-9, 0), new SectionRectangular(200, 100, .2));

    [TestMethod]
    public void C5_EquivalentBeamsShareOnePropertyAndItsBxs()
    {
        var property = Rotated("girder"); var log = Straus7ExporterCorrectionsTest.Record(TwoBeams(property, property));
        var created = log.Named(nameof(IStraus7ModelWriteApi.CreateBeamProperty)); Assert.AreEqual(1, created.Length); Assert.AreEqual("girder", created[0].Args[1]);
        Assert.AreEqual(1, log.Named(nameof(IStraus7SectionMeshWriteApi.SetBeamSectionMesh)).Length);
        CollectionAssert.AreEqual(new[] { 1, 1 }, log.Named(nameof(IStraus7ModelWriteApi.SetElement)).Select(c => (int)c.Args[2]!).ToArray());
        Assert.AreEqual("S355 steel", log.Named(nameof(IStraus7ModelWriteApi.SetBeamMaterialName)).Single().Args[1]);
        // Equal values in distinct instances are shared too; a different property is not.
        Assert.AreEqual(1, Straus7ExporterCorrectionsTest.Record(TwoBeams(Rotated("girder"), Rotated("girder"))).Named(nameof(IStraus7ModelWriteApi.CreateBeamProperty)).Length);
        var different = Straus7ExporterCorrectionsTest.Record(TwoBeams(Rotated("girder"), Rotated("cross girder")));
        CollectionAssert.AreEqual(new[] { 1, 2 }, different.Named(nameof(IStraus7ModelWriteApi.SetElement)).Select(c => (int)c.Args[2]!).ToArray());
    }

    [TestMethod, TestCategory("Straus7Native")]
    public void C5_NativeSharedPropertyKeepsNamesAndResults()
    {
        var property = Rotated("girder"); var model = TwoBeams(property, property); string folder = Straus7ExporterCorrectionsTest.NativeFolder();
        var run = Straus7ModelWritePlan.Prepare(model).RunLinearStatic(model, Path.Combine(folder, "shared.st7"), folder);
        using var reader = new Straus7NativeApi(); reader.OpenModelReadOnly(run.ModelPath, folder);
        var first = reader.ReadElement(Straus7Entity.Beam, 1); var second = reader.ReadElement(Straus7Entity.Beam, 2);
        Assert.AreEqual(1, first.Property); Assert.AreEqual(1, second.Property);
        var native = reader.ReadProperty(Straus7Entity.Beam, 1); Assert.AreEqual("girder", native.Name); Assert.AreEqual("S355 steel", native.MaterialName);
        reader.OpenResults(run.ResultPath); int tip = model.NodesElements.Values.Single(n => n.Position.X == 1000).Id;
        var section = (SectionRectangular)property.Section; double k = -1000 * Math.Pow(1000, 3) / (3 * 210000), c = Math.Cos(.2), s = Math.Sin(.2);
        Assert.AreEqual(k * (s * s / section.J22 + c * c / section.J11), reader.ReadNodeResult(tip, 1, false).Values[2], 1e-6 * Math.Abs(k / section.J11));
    }
}

[TestClass]
public class MovingPathOptionsContractTest
{
    public interface IRecordedMovingWriter : IStraus7MovingWriteApi, IStraus7AdvancedMovingWriteApi { }
    private static Straus7MovingPathOptions Legacy() => new Straus7MovingPathOptions {
        Colour = 0x00ff00, TransitionTrailingLoad = true, BeamLateralTolerance = true, RelativeTolerance = .05, ConvertToStaticCases = true };

    [TestMethod]
    public void B4_LoadApplicationControlsAndDisplayDataSurviveTheArchive()
    {
        var model = MovingLoadContractsTest.Sample(); var path = model.MovingLoads.Paths.Single(); string before = model.AnalysisFingerprint();
        Legacy().Store(model, path.Id);
        Assert.AreNotEqual(before, model.AnalysisFingerprint(), "the controls change the applied loads");
        Model reopened; using (var stream = new MemoryStream()) { GPC.Model.Persistence.ModelArchive.SaveDocument(model, stream); stream.Position = 0; reopened = GPC.Model.Persistence.ModelArchive.Load(stream); }
        Assert.AreEqual(model.AnalysisFingerprint(), reopened.AnalysisFingerprint());
        var options = reopened.MovingLoads.Paths.Single().Options;
        Assert.IsTrue(options.TransitionTrailingLoad); Assert.IsTrue(options.BeamLateralTolerance); Assert.AreEqual(.05, options.RelativeTolerance); Assert.IsTrue(options.ConvertToStaticCases);
        Assert.AreEqual("65280", reopened.PreservedSourceData.Single(p => p.SourceRecord == Straus7MovingPathOptions.ColourRecord + path.Id.ToString("D")).RawData);
        var copy = GPC.Model.Core.ModelValues.Copy(reopened); Assert.AreEqual(.05, copy.MovingLoads.Paths.Single().Options.RelativeTolerance);
        copy.MovingLoads.Paths.Single().Options.RelativeTolerance = .02; Assert.AreEqual(.05, reopened.MovingLoads.Paths.Single().Options.RelativeTolerance);
        Assert.AreNotEqual(reopened.AnalysisFingerprint(), copy.AnalysisFingerprint());

        // The writer reads the persisted controls and colour without explicit options.
        var api = System.Reflection.DispatchProxy.Create<IRecordedMovingWriter, Straus7ExporterCorrectionsTest.Recorder>(); var log = (Straus7ExporterCorrectionsTest.Recorder)(object)api;
        var cases = reopened.LoadCases.Values.Select((c, i) => (c.Guid, i + 1)).ToDictionary(p => p.Guid, p => p.Item2);
        Straus7MovingWritePlan.Prepare(reopened, cases).Write(api, reopened);
        var template = log.Named(nameof(IStraus7MovingWriteApi.CreateMovingTemplate)).Single();
        var flags = (int[])template.Args[2]!; var values = (double[])template.Args[3]!;
        Assert.AreEqual(0x00ff00, flags[0]); Assert.AreEqual(1, flags[3]); Assert.AreEqual(1, flags[4]); Assert.AreEqual(.05, values[0]);
        Assert.AreEqual(true, log.Named(nameof(IStraus7MovingWriteApi.SetMovingPath)).Single().Args[3]);
    }

    [TestMethod]
    public void B4_ExplicitOptionsMustAgreeWithThePersistedControlsAndLegacySetsNeedASelection()
    {
        var model = MovingLoadContractsTest.Sample(); var path = model.MovingLoads.Paths.Single(); Legacy().Store(model, path.Id);
        var cases = model.LoadCases.Values.Select((c, i) => (c.Guid, i + 1)).ToDictionary(p => p.Guid, p => p.Item2);
        var other = Legacy(); other.RelativeTolerance = .02;
        StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => Straus7MovingWritePlan.Prepare(model, cases, new Dictionary<Guid, Straus7MovingPathOptions> { { path.Id, other } })).Message,
            "MovingPathOptionsConflictWithModel");
        var colour = Legacy(); colour.Colour = 1; Straus7MovingWritePlan.Prepare(model, cases, new Dictionary<Guid, Straus7MovingPathOptions> { { path.Id, colour } });
        var legacySet = Legacy(); legacySet.EntitySetId = 3; legacySet.Store(model, path.Id);
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7MovingWritePlan.Prepare(model, cases)).Message, "LegacyMovingEntitySetRequiresTargetSelection");
    }
}

[TestClass]
public class MovingCentrifugalContractTest
{
    /// <summary>The text contract of GPCModel 5.43-5.48 (same data-contract name, namespace and member names).</summary>
    [System.Runtime.Serialization.DataContract(Name = "MovingCentrifugalLaw", Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.Actions.Moving")]
    public sealed class LegacyLaw
    {
        [System.Runtime.Serialization.DataMember(Name = "<ExpressionLanguage>k__BackingField", Order = 0)] public string? Language;
        [System.Runtime.Serialization.DataMember(Name = "<OffsetExpression>k__BackingField", Order = 1)] public string? Offset;
        [System.Runtime.Serialization.DataMember(Name = "<FactorExpression>k__BackingField", Order = 2)] public string? Factor;
        [System.Runtime.Serialization.DataMember(Name = "<LengthUnit>k__BackingField", Order = 3)] public string? Length;
        [System.Runtime.Serialization.DataMember(Name = "<ForceUnit>k__BackingField", Order = 4)] public string? Force;
        [System.Runtime.Serialization.DataMember(Name = "<MinimumFactor>k__BackingField", Order = 5)] public double? Minimum;
        [System.Runtime.Serialization.DataMember(Name = "<MaximumFactor>k__BackingField", Order = 6)] public double? Maximum;
        [System.Runtime.Serialization.DataMember(Name = "<LimitFactor>k__BackingField", Order = 7)] public bool Limit;
    }
    private static GPC.Model.Actions.Moving.MovingCentrifugalLaw ReadLegacy(LegacyLaw legacy)
    {
        using var stream = new MemoryStream(); new System.Runtime.Serialization.DataContractSerializer(typeof(LegacyLaw)).WriteObject(stream, legacy); stream.Position = 0;
        return (GPC.Model.Actions.Moving.MovingCentrifugalLaw)new System.Runtime.Serialization.DataContractSerializer(typeof(GPC.Model.Actions.Moving.MovingCentrifugalLaw)).ReadObject(stream)!;
    }

    [TestMethod]
    public void B8_LegacyTextLawsAreConvertedToNewtonMillimetreData()
    {
        var law = ReadLegacy(new LegacyLaw { Language = "Straus7-R3", Offset = "2", Factor = "V^2/(g*R)", Length = "m", Force = "kN", Limit = true, Minimum = 0, Maximum = .2 });
        Assert.AreEqual(2000, law.OffsetForce); Assert.AreEqual(GPC.Model.Actions.Moving.MovingCentrifugalFactorKind.SpeedSquaredOverGravityRadius, law.FactorKind);
        Assert.AreEqual(.2, law.MaximumFactor); Assert.IsTrue(law.LimitFactor); law.Validate();
        var constant = ReadLegacy(new LegacyLaw { Language = "Straus7-R3", Offset = "", Factor = "0.1", Length = "mm", Force = "N" });
        Assert.AreEqual(GPC.Model.Actions.Moving.MovingCentrifugalFactorKind.Constant, constant.FactorKind); Assert.AreEqual(.1, constant.Factor); Assert.AreEqual(0, constant.OffsetForce);
        // Written back without any text: the copy has only the N-mm members.
        using (var stream = new MemoryStream())
        {
            new System.Runtime.Serialization.DataContractSerializer(typeof(GPC.Model.Actions.Moving.MovingCentrifugalLaw)).WriteObject(stream, law);
            string xml = System.Text.Encoding.UTF8.GetString(stream.ToArray()); Assert.IsFalse(xml.Contains("Straus7-R3")); Assert.IsFalse(xml.Contains("kN"));
        }
        foreach (var bad in new[] { new LegacyLaw { Language = "Straus7-R3", Offset = "0", Factor = "V^2/(9.81*R)", Length = "m", Force = "kN" },
            new LegacyLaw { Language = "Straus7-R3", Offset = "0", Factor = "0.1", Length = "furlong", Force = "N" },
            new LegacyLaw { Language = "Other", Offset = "0", Factor = "0.1", Length = "mm", Force = "N" } })
            Assert.ThrowsException<System.Runtime.Serialization.SerializationException>(() => ReadLegacy(bad));
    }

    [TestMethod]
    public void B8_Straus7DialectStaysInTheConverter()
    {
        var expression = new Straus7CentrifugalExpression { OffsetExpression = "1.5", FactorExpression = "V^2/(g*R)", LengthUnit = "m", ForceUnit = "kN", LimitFactor = true, MinimumFactor = 0, MaximumFactor = .3 };
        var law = expression.ToDomain(out var none); Assert.IsNull(none); Assert.AreEqual(1500, law.OffsetForce, 1e-9);
        var written = Straus7CentrifugalExpression.FromDomain(law, null);
        Assert.AreEqual("1500", written.OffsetExpression); Assert.AreEqual("V^2/(g*R)", written.FactorExpression); Assert.AreEqual("mm", written.LengthUnit); Assert.AreEqual("N", written.ForceUnit);
        // A radius-dependent library law (EN 1991-2 style) stays in the R3 dialect, outside Model, bound by record and SHA-256.
        var library = new Straus7CentrifugalExpression { OffsetExpression = "0", FactorExpression = "((IFPOSB(R-0)*0.2)-(IFPOSB(R-200)*0.2))+((IFPOSB(R-200)*(40/R))-(IFPOSB(R-1500)*(40/R)))", LengthUnit = "m", ForceUnit = "kN" };
        var adapter = library.ToDomain(out var record); Assert.IsNotNull(record);
        Assert.AreEqual(GPC.Model.Actions.Moving.MovingCentrifugalFactorKind.AdapterDefined, adapter.FactorKind); Assert.AreEqual(record!.SourceRecord, adapter.AdapterRecord);
        var model = MovingLoadContractsTest.Sample(); StringAssert.Contains(Assert.ThrowsException<ArgumentException>(() => Straus7CentrifugalExpression.FromDomain(adapter, model)).Message, "MissingOrChangedStrausCentrifugalExpression");
        model.PreservedSourceData.Add(record); var back = Straus7CentrifugalExpression.FromDomain(adapter, model);
        Assert.AreEqual(library.FactorExpression, back.FactorExpression); Assert.AreEqual("m", back.LengthUnit); Assert.AreEqual("kN", back.ForceUnit);
        record.RawData = record.RawData.Replace("0.2", "0.3"); Assert.ThrowsException<ArgumentException>(() => Straus7CentrifugalExpression.FromDomain(adapter, model));
        Assert.IsFalse(typeof(GPC.Model.Actions.Moving.MovingCentrifugalLaw).GetProperties().Any(p => p.Name.Contains("Expression") || p.Name.Contains("Unit") || p.Name.Contains("Language")),
            "no dialect or unit text in Model");
    }
}
