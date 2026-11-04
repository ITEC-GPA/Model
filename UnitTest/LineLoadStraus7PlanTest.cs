using System.Reflection;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Loads.Assignments;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Restraints;
using GPC.Model.Sections;
using GPC.Model.Structure.Assignments;

namespace UnitTest;

/// <summary>The consumer of <see cref="LineLoad.GetGlobalForces"/> in the libraries: the Straus7 writer, which writes every distributed
/// beam load in global axes. A load given in beam axes with the origin at the start node (as the MIDAS and Straus7 readers give the
/// local loads, <c>beam.CoordinateSystem</c>) is now written as the same load given in global axes (lane L9); before, the coordinates of
/// the node were subtracted from its intensities. Plan only (recording proxy), no native writer.</summary>
[TestClass]
public class LineLoadStraus7PlanTest
{
    public class Recorder : DispatchProxy
    {
        internal readonly List<string> Distributed = new();
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            if (method!.Name == nameof(IStraus7ModelWriteApi.SetBeamDistributedLoad))
                Distributed.Add(args![3] + (((bool)args[4]!) ? " moment " : " force ") + string.Join(" ", ((double[])args[5]!).Select(v => BitConverter.DoubleToInt64Bits(v).ToString("X16"))));
            var type = method.ReturnType;
            return type == typeof(void) || !type.IsValueType ? null : Activator.CreateInstance(type);
        }
    }

    private static List<string> Written(Model model)
    {
        var api = DispatchProxy.Create<ReviewFixesL6Test.IForwardedWriter, Recorder>();
        Straus7ModelWritePlan.Prepare(model).Write(api, model);
        return ((Recorder)(object)api).Distributed;
    }

    private static Model Cantilever(Func<Line3d, LoadCaseBase, CoordinateSystem, (LineLoad Start, LineLoad End)> load)
    {
        var start = new Point3d(5000, 7000, 300); var end = new Point3d(6000, 7000, 300);
        var fixedNode = new NodeElement(start); var freeNode = new NodeElement(end);
        var freedom = new FreedomCase("Supports"); var lc = new LoadCaseBase("P");
        fixedNode.Assignments.Restrains.Add(new RestrainAssignment { FreedomCase = freedom,
            Restrain = new NodeRestrain(fixedNode, Enumerable.Range(0, 6).Select(i => new DofRestrain((GeometryRestrain.DOF)i, true)).ToList()) });
        var property = new ElasticBeamProperty("Elastic rectangle", new Material("E210", 210000, .3, 7.85e-9, 1.2e-5), new SectionRectangular(200, 100));
        var beam = new BeamElement(fixedNode.Position, freeNode.Position, property); beam.ConnectNodes(fixedNode, freeNode);
        beam.Assignments.Formulation = BeamFormulation.StraightTwoNode;
        // beam axes at the start node: V1 = Y, V2 = Z, V3 = X (the axis of the beam)
        var axes = new CoordinateSystem(new Point3d(start.X, start.Y, start.Z), new Vector3d(0, 1, 0), new Vector3d(0, 0, 1));
        beam.Assignments.SectionAxes = axes;
        var (a, b) = load(beam.Line, lc, axes);
        beam.Assignments.Loads.Add(new BeamLoadAssignment { Start = 0, End = 1, LengthConvention = BeamLoadLengthConvention.ActualLength, StartIntensity = a, EndIntensity = b });
        return ModelAssembly.Assemble(new Element[] { beam }, Guid.NewGuid());
    }

    [TestMethod]
    public void LoadsInBeamAxesAtTheStartNodeAreWrittenAsTheSameLoadInGlobalAxes()
    {
        // -2 N/mm and -3 N/mm downwards (local V2 = global Z), 0.5 N mm/mm about the axis of the beam (local V3 = global X)
        var local = Written(Cantilever((line, lc, axes) => (new LineLoad(0, -2, 0, 0, 0, 0.5, line, lc, axes), new LineLoad(0, -3, 0, 0, 0, 0.5, line, lc, axes))));
        var global = Written(Cantilever((line, lc, axes) => (new LineLoad(0, 0, -2, 0.5, 0, 0, line, lc), new LineLoad(0, 0, -3, 0.5, 0, 0, line, lc))));
        Assert.IsTrue(global.Count > 0, "distributed loads written");
        CollectionAssert.AreEqual(global, local);
    }
}
