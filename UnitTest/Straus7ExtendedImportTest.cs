using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Elements;
using GPC.Model.Sections.Steel;
using GPC.Model.Structure.Assignments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>End releases, node springs, links, LSA combinations and element families through a transport double of the R3 API (m, kN, kPa),
/// with the conventions verified on the native solver by <see cref="Straus7NativeElementsTest"/>; values are invented, not solver output.</summary>
public partial class Straus7AssignmentImportTest
{
    private sealed class ExtendedApi : IStraus7ReadApi, IStraus7AssignmentReadApi, IStraus7ExtendedReadApi, IStraus7StageReadApi, IStraus7MassTemperatureReadApi
    {
        public readonly List<Straus7Stage> Stages = new();
        public readonly Dictionary<(Straus7Entity, int, int), double[]> MassValues = new();
        public readonly Dictionary<(Straus7Entity, int, int), double[]> TemperatureValues = new();
        public readonly Dictionary<int, bool[]> MassOptions = new();
        public Straus7Stage ReadStage(int number, IReadOnlyList<int> groups) => Stages[number - 1];
        public int[] ReadElementResultState(Straus7Entity entity, int number, int resultCase) => new[] { 1, 1, 1 };
        public double[] ReadNodeTranslationalMass(int node) => MassValues[(Straus7Entity.Node, node, -1)];
        public double[] ReadNodeRotationalMass(int node, out int ucs) { var v = MassValues[(Straus7Entity.Node, node, -2)]; ucs = (int)v[3]; return v.Take(3).ToArray(); }
        public double[] ReadNodeNonStructuralMass(int node, int loadCase, int id) => MassValues[(Straus7Entity.Node, node, loadCase)];
        public double[] ReadBeamNonStructuralMass(int beam, int loadCase, int id, out int distribution) { var v = MassValues[(Straus7Entity.Beam, beam, loadCase)]; distribution = (int)v[10]; return v.Take(10).ToArray(); }
        public double[] ReadPlateNonStructuralMass(int plate, int loadCase, int id) => MassValues[(Straus7Entity.Plate, plate, loadCase)];
        public double ReadNodeTemperature(int node, int loadCase, out int type) { var v = TemperatureValues[(Straus7Entity.Node, node, loadCase)]; type = (int)v[1]; return v[0]; }
        public double[] ReadBeamTemperatureGradient(int beam, int loadCase) => TemperatureValues[(Straus7Entity.Beam, beam, loadCase)];
        public double ReadPlateTemperatureGradient(int plate, int loadCase) => TemperatureValues[(Straus7Entity.Plate, plate, loadCase)][0];
        public bool[] ReadLoadCaseMassOption(int loadCase) => MassOptions.TryGetValue(loadCase, out var o) ? o : new[] { true, true };
        public readonly AssignmentApi Inner = Scenario();
        public readonly Dictionary<int, double[]> ExtraNodes = new() { [5] = new[] { 0d, 2.5, 0 }, [6] = new[] { 4d, 2, 1 } };
        public readonly Dictionary<int, int> Formulations = new();
        public readonly Dictionary<(Straus7Entity, int, int), List<Straus7Attribute>> Extra = new();
        public readonly Dictionary<(int, int, bool), Straus7BeamRelease> Releases = new();
        public readonly Dictionary<(int, bool), Straus7NodeStiffness> Springs = new();
        public readonly List<Straus7Link> Links = new();
        public readonly List<Straus7Combination> Combinations = new();
        public int Bricks;
        public void Add(Straus7Entity entity, int number, int attribute, int local = 0, int lc = 0, int id = 0)
        {
            if (!Extra.TryGetValue((entity, number, attribute), out var list)) Extra[(entity, number, attribute)] = list = new();
            list.Add(new Straus7Attribute { Local = local, Case = lc, Id = id });
        }
        public Straus7BeamRelease ReadBeamRelease(int beam, int end, bool rotational) => Releases[(beam, end, rotational)];
        public Straus7NodeStiffness ReadNodeStiffness(int node, int freedomCase, bool rotational) => Springs[(node, rotational)];
        public Straus7Link ReadLink(int number) => Links[number - 1];
        public int CombinationCount => Combinations.Count;
        public Straus7Combination ReadCombination(int number, int loadCases, int freedomCases) => Combinations[number - 1];
        public int CombinedCombinationCount { get; set; }
        public Straus7Attribute[] ReadAttributes(Straus7Entity entity, int number, int attribute) =>
            Inner.ReadAttributes(entity, number, attribute).Concat(Extra.TryGetValue((entity, number, attribute), out var list) ? list : new()).ToArray();
        public int FreedomCaseCount => Inner.FreedomCaseCount;
        public Straus7Restraint ReadRestraint(int node, int freedomCase) => Inner.ReadRestraint(node, freedomCase);
        public Straus7Ucs ReadUcs(int id) => Inner.ReadUcs(id);
        public double[] ReadNodeLoad(int node, int loadCase, bool moment) => Inner.ReadNodeLoad(node, loadCase, moment);
        public double[] ReadBeamOffset(int beam) => Inner.ReadBeamOffset(beam);
        public double[] ReadPlateOffset(int plate) => Inner.ReadPlateOffset(plate);
        public double[] ReadPlateThickness(int plate) => Inner.ReadPlateThickness(plate);
        public Straus7DistributedLoad ReadBeamDistributedLoad(int beam, Straus7LoadFrame frame, int direction, int loadCase, int id) => Inner.ReadBeamDistributedLoad(beam, frame, direction, loadCase, id);
        public double[] ReadBeamPointLoad(int beam, int loadCase, int id, bool moment, bool global) => Inner.ReadBeamPointLoad(beam, loadCase, id, moment, global);
        public double[] ReadPlateNormalPressure(int plate, int loadCase) => Inner.ReadPlateNormalPressure(plate, loadCase);
        public double[] ReadPlateGlobalPressure(int plate, int surface, int loadCase, out int project) => Inner.ReadPlateGlobalPressure(plate, surface, loadCase, out project);
        public Straus7LoadCaseData ReadLoadCaseData(int number) => Inner.ReadLoadCaseData(number);
        public string Version => Inner.Version;
        public void OpenModelReadOnly(string path, string scratchPath) { }
        public void CloseModel() { }
        public void Dispose() { }
        public int[] ReadUnits() => Inner.ReadUnits();
        public int Count(Straus7Entity entity) => entity switch { Straus7Entity.Node => 4 + ExtraNodes.Count, Straus7Entity.Brick => Bricks, Straus7Entity.Link => Links.Count, _ => Inner.Count(entity) };
        public Straus7Node ReadNode(int number) => ExtraNodes.TryGetValue(number, out var xyz) ? new() { Number = number, Coordinates = xyz } : Inner.ReadNode(number);
        public Straus7Element ReadElement(Straus7Entity entity, int number)
        {
            var e = Inner.ReadElement(entity, number);
            if (entity == Straus7Entity.Beam && Formulations.TryGetValue(number, out var f)) { e.Formulation = f; if (f != 6) e.InitialAxes = null; }
            return e;
        }
        public Straus7Property ReadProperty(Straus7Entity entity, int number)
        {
            var p = Inner.ReadProperty(entity, number);
            if (entity == Straus7Entity.Beam && Formulations.TryGetValue(number, out var f))
                p = new Straus7Property { Number = p.Number, Name = p.Name, Formulation = f, SectionType = p.SectionType, Geometry = p.Geometry, Material = p.Material,
                    SectionProperties = p.SectionProperties, MaterialName = p.MaterialName, SpringData = f == 1 ? new[] { 2000d, 100, 50, 0, 0, 0, 0 } : null };
            return p;
        }
        public int GroupCount => Inner.GroupCount;
        public Straus7Group ReadGroup(int index) => Inner.ReadGroup(index);
        public int LoadCaseCount => Inner.LoadCaseCount;
        public string ReadLoadCase(int number) => Inner.ReadLoadCase(number);
        public int StageCount => Stages.Count;
        public Straus7ResultFile ValidateResults(string path) => throw new NotSupportedException();
        public Straus7ResultFile OpenResults(string path) => throw new NotSupportedException();
        public void CloseResults() { }
        public Straus7ResultCase ReadResultCase(int number) => throw new NotSupportedException();
        public Straus7ResultTable ReadBeamForces(int number, int resultCase, int minimumStations) => throw new NotSupportedException();
        public Straus7ResultTable ReadPlateResult(int number, int resultCase, bool moments) => throw new NotSupportedException();
        public Straus7ResultTable ReadNodeResult(int number, int resultCase, bool reactions) => throw new NotSupportedException();
    }

    private ImportReport Import(ExtendedApi api) => new Straus7ApiConverter(() => api).Import(new Straus7ImportRequest { ModelPath = modelPath, ModelRevision = "r1", AnalysisId = "a1" });
    private static Straus7BeamRelease Release(int[] status, double[]? values = null) => new() { Status = status, Values = values ?? new double[3] };

    [TestMethod]
    public void EndReleasesAndNodeSprings_ConvertStatusesStiffnessesAndAxes()
    {
        var api = new ExtendedApi();
        api.Add(Straus7Entity.Beam, 1, Straus7Attributes.BeamREndRelease, local: 2);
        api.Releases[(1, 2, true)] = Release(new[] { 0, 2, 1 }, new[] { 0, 3000d, 0 });
        api.Add(Straus7Entity.Beam, 2, Straus7Attributes.BeamTEndRelease, local: 1);
        api.Releases[(2, 1, false)] = Release(new[] { 1, 1, 2 }, new[] { 0, 0, 40d });
        api.Add(Straus7Entity.Node, 3, Straus7Attributes.KTranslation, lc: 1); api.Springs[(3, false)] = new() { Ucs = 1, Values = new[] { 0, 0, 500d } };
        api.Add(Straus7Entity.Node, 3, Straus7Attributes.KRotation, lc: 1); api.Springs[(3, true)] = new() { Ucs = 1, Values = new[] { 0, 0, 2d } };
        api.Add(Straus7Entity.Node, 4, Straus7Attributes.KTranslation, lc: 1); api.Springs[(4, false)] = new() { Ucs = 5, Values = new[] { 100d, 0, 0 } };
        var report = Import(api); Partial(report); var m = report.Model;
        var column = m.BeamElements.Values.Single(b => b.Source.OriginalId == "1").Attributes.Values.OfType<BeamReleasesAttribute>().Single();
        Assert.IsTrue(column.I.All(d => d.Kind == BeamConnectionKind.Continuous));
        Assert.AreEqual(BeamConnectionKind.Released, column.J[3].Kind, "Rotation about principal axis 1 = V1.");
        Assert.AreEqual(BeamConnectionKind.AbsoluteStiffness, column.J[4].Kind); Assert.AreEqual(3e9, column.J[4].Value, 1e-3, "3000 kNm/rad.");
        Assert.AreEqual(BeamConnectionKind.Continuous, column.J[5].Kind); Assert.AreEqual(1, column.CoordinateSystem.V1.Y, 1e-12);
        var girder = m.BeamElements.Values.Single(b => b.Source.OriginalId == "2").Attributes.Values.OfType<BeamReleasesAttribute>().Single();
        Assert.AreEqual(BeamConnectionKind.AbsoluteStiffness, girder.I[2].Kind); Assert.AreEqual(40, girder.I[2].Value, 1e-12, "Axial 40 kN/m = 40 N/mm.");
        Assert.IsTrue(girder.J.All(d => d.Kind == BeamConnectionKind.Continuous));
        var spring = m.NodesElements.Values.Single(n => n.Source.OriginalId == "3").Assignments.GroundSprings.Single();
        Assert.AreEqual(500, spring[2, 2]); Assert.AreEqual(2e6, spring[5, 5]); Assert.AreEqual(4000, spring.CoordinateSystem.Origin.X);
        var skew = m.NodesElements.Values.Single(n => n.Source.OriginalId == "4").Assignments.GroundSprings.Single();
        Assert.AreEqual(100, skew[0, 0]); Assert.AreEqual(1, skew.CoordinateSystem.V1.Y, 1e-12, "UCS 5: x along global Y.");
        Assert.IsFalse(m.ValidateAssignments().Any(d => d.Severity == DiagnosticSeverity.Error));
    }

    [TestMethod]
    public void Links_RigidMasterSlaveAndPinnedBecomeRigidNodeLinks_OthersArePreserved()
    {
        var api = new ExtendedApi();
        api.ExtraNodes[7] = new[] { 0d, 2.5, 0 };
        api.Links.Add(new Straus7Link { Number = 1, Type = 5, Nodes = new[] { 3, 4 }, Ucs = 1, Plane = 0 });
        api.Links.Add(new Straus7Link { Number = 2, Type = 5, Nodes = new[] { 1, 2 }, Ucs = 1, Plane = 1 });
        api.Links.Add(new Straus7Link { Number = 3, Type = 1, Nodes = new[] { 5, 7 }, Ucs = 1, Dofs = new[] { 1, 1, 1, 0, 0, 0 } });
        api.Links.Add(new Straus7Link { Number = 4, Type = 4, Nodes = new[] { 4, 5 } });
        api.Links.Add(new Straus7Link { Number = 5, Type = 6, Nodes = new[] { 1, 6 } });
        api.Links.Add(new Straus7Link { Number = 6, Type = 5, Nodes = new[] { 1, 6 }, Ucs = 1, Plane = 1 });
        var report = Import(api); Partial(report); var m = report.Model;
        NodalLink Link(string node) => m.NodesElements.Values.Single(n => n.Source.OriginalId == node).Assignments.Links.Single();
        Assert.AreEqual(72, Link("3").KinematicCoefficients.Length, "Rigid XYZ: six equations of 12 coefficients.");
        Assert.AreEqual(36, Link("1").KinematicCoefficients.Length, "Rigid in the XY plane: DX, DY, RZ.");
        Assert.AreEqual(36, Link("5").KinematicCoefficients.Length, "Master-slave of coincident nodes: the three linked translations.");
        Assert.AreEqual(0, m.NodesElements.Values.Single(n => n.Source.OriginalId == "4").Assignments.Links.Count, "Rigid 3-4 is on node 3; pinned 4-5 is not mapped (distinct nodes).");
        Assert.AreEqual(3, report.Diagnostics.Count(d => d.Code == "Straus7LinkPreserved"), "Pinned link of distinct nodes, shrink link, planar link out of its plane.");
        Assert.IsTrue(report.Preserved.Any(p => p.SourceRecord == "Link/5"));
    }

    [TestMethod]
    public void LsaCombinations_AreLinearCombinationsOfTheLoadCases()
    {
        var api = new ExtendedApi { CombinedCombinationCount = 1 };
        api.Combinations.Add(new Straus7Combination { Number = 1, Name = "ULS", Enabled = true, Factors = new[] { 1.35, 1.0, 0 } });
        api.Combinations.Add(new Straus7Combination { Number = 2, Name = "Empty", Enabled = false, Factors = new double[3] });
        var report = Import(api); Partial(report); var m = report.Model;
        var definition = CombinationExpansion.Definitions(m).Single();
        Assert.AreEqual("ULS", definition.Name); Assert.AreEqual("LSA", definition.Source); Assert.AreEqual("ACTIVE", definition.Status);
        CollectionAssert.AreEqual(new[] { Straus7ApiConverter.CaseName(1, "Loads"), Straus7ApiConverter.CaseName(2, "Gravity") }, definition.Terms.Select(t => t.Name).ToArray());
        Assert.AreEqual(1.35, definition.Terms[0].Factor); Assert.IsTrue(m.Combinations.ContainsKey("ULS"));
        foreach (var code in new[] { "Straus7EmptyCombinationPreserved", "Straus7CombinedCombinationsPreserved" }) Assert.IsTrue(report.Diagnostics.Any(d => d.Code == code), code);
    }

    [TestMethod]
    public void ElementFamilies_TrussSpringBricksAndUnsupportedBeamsDoNotRejectTheModel()
    {
        var api = new ExtendedApi { Bricks = 3 };
        api.Formulations[2] = 3; // Girder as a truss.
        var truss = Import(api); Partial(truss);
        var member = truss.Model.BeamElements.Values.Single(b => b.Source.OriginalId == "2");
        Assert.AreEqual(BeamFormulation.Truss, member.Assignments.Formulation); Assert.IsInstanceOfType(member.BeamProperty, typeof(SteelSection));
        Assert.AreEqual(1, member.Assignments.SectionAxes.V3.X, 1e-12, "Computed axes: V3 from I to J.");
        Assert.IsTrue(truss.Diagnostics.Any(d => d.Code == "Straus7BricksPreserved" && d.Message.StartsWith("3 ")));
        Assert.IsTrue(truss.Diagnostics.Any(d => d.Code == "AxialOnlyBeamFormulation" && d.Severity == DiagnosticSeverity.Warning));
        api = new ExtendedApi(); api.Formulations[2] = 1; // Girder as a spring-damper: a spring between nodes 4 and 3.
        var spring = Import(api); Partial(spring);
        Assert.AreEqual(1, spring.Model.BeamElements.Count);
        var link = spring.Model.NodesElements.Values.Single(n => n.Source.OriginalId == "4").Assignments.Links.Single().Spring;
        Assert.AreEqual(2000, link[2, 2], "Axial along V3."); Assert.AreEqual(100, link[0, 0]); Assert.AreEqual(100, link[1, 1]); Assert.AreEqual(5e7, link[5, 5], "50 kNm/rad.");
        Assert.AreEqual(1, link.CoordinateSystem.V3.X, 1e-12);
        api = new ExtendedApi(); api.Formulations[2] = 5; // Point contact.
        var contact = Import(api); Partial(contact);
        Assert.AreEqual(1, contact.Model.BeamElements.Count); Assert.IsTrue(contact.Diagnostics.Any(d => d.Code == "Straus7BeamFamilyPreserved" && d.Record.Contains("point contact")));
    }
}
