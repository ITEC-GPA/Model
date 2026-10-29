using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Model.Stages;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>Stages, masses and temperatures through the transport double of the R3 API (m, kN, kPa, kg, Celsius): normalized to GPC units
/// (tonne, mm, °C) as <see cref="Straus7ModelData"/>; values are invented, not solver output. The native counterpart is Straus7NativeStagesTest.</summary>
public partial class Straus7AssignmentImportTest
{
    [TestMethod]
    public void MassesAndTemperatures_AreNormalizedDataOfTheModel()
    {
        var api = new ExtendedApi();
        api.Add(Straus7Entity.Node, 3, Straus7Attributes.NodeTranslationalMass); api.MassValues[(Straus7Entity.Node, 3, -1)] = new[] { 500d, 500, 250 };
        api.Add(Straus7Entity.Node, 3, Straus7Attributes.NodeRotationalMass); api.MassValues[(Straus7Entity.Node, 3, -2)] = new[] { 10d, 20, 30, 1 };
        api.Add(Straus7Entity.Node, 2, Straus7Attributes.NodeNonStructuralMass, lc: 2, id: 1); api.MassValues[(Straus7Entity.Node, 2, 2)] = new[] { 200d, 1, 0, 0, .1 };
        api.Add(Straus7Entity.Beam, 1, Straus7Attributes.BeamNonStructuralMass, lc: 2, id: 1);
        api.MassValues[(Straus7Entity.Beam, 1, 2)] = new[] { 100d, 300, 0, 0, 0, 0, 1, 0, 0, 0, 1 }; // dlLinear 100 -> 300 kg/m.
        api.Add(Straus7Entity.Plate, 1, Straus7Attributes.PlateNonStructuralMass, lc: 2, id: 1); api.MassValues[(Straus7Entity.Plate, 1, 2)] = new[] { 50d, .8, 0, 0, 0 };
        api.Add(Straus7Entity.Node, 1, Straus7Attributes.NodeTemperature, lc: 1); api.TemperatureValues[(Straus7Entity.Node, 1, 1)] = new[] { 35d, 0 };
        api.Add(Straus7Entity.Beam, 1, Straus7Attributes.BeamTemperatureGradient, lc: 1); api.TemperatureValues[(Straus7Entity.Beam, 1, 1)] = new[] { 10d, -5 };
        api.Add(Straus7Entity.Plate, 1, Straus7Attributes.PlateTemperatureGradient, lc: 1); api.TemperatureValues[(Straus7Entity.Plate, 1, 1)] = new[] { 20d };
        api.Inner.Cases[1] = new Straus7LoadCaseData { Number = 1, Type = 0, Defaults = new[] { 15d, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } };
        api.MassOptions[2] = new[] { true, false };
        var report = Import(api); Partial(report);
        var data = Straus7ModelData.Read(report.Model);
        string Lc(int n, string name) => Straus7ApiConverter.CaseName(n, name);
        var t = data.Masses.Single(m => m.Kind == Straus7MassKind.NodeTranslational);
        Assert.AreEqual(3, t.Number); CollectionAssert.AreEqual(new[] { .5, .5, .25 }, t.Values, "kg to tonne."); Assert.IsNull(t.Case);
        var r = data.Masses.Single(m => m.Kind == Straus7MassKind.NodeRotational);
        Assert.AreEqual(20e3, r.Values[1], 1e-9, "20 kg m2 = 0.02 t x 1e6 mm2."); Assert.AreEqual(1, r.Axes[0], 1e-12, "Global UCS.");
        var n = data.Masses.Single(m => m.Kind == Straus7MassKind.NodeNonStructural);
        Assert.AreEqual(Lc(2, "Gravity"), n.Case); Assert.AreEqual(.2, n.Values[0], 1e-12); Assert.AreEqual(100, n.Offset[2], 1e-9, "0.1 m."); Assert.AreEqual(1, n.DynamicFactor);
        var b = data.Masses.Single(m => m.Kind == Straus7MassKind.BeamNonStructural);
        CollectionAssert.AreEqual(new[] { 0d, 1 }, b.Stations); Assert.AreEqual(1e-4, b.Values[0], 1e-15, "100 kg/m = 1e-4 t/mm."); Assert.AreEqual(3e-4, b.Values[1], 1e-15);
        var p = data.Masses.Single(m => m.Kind == Straus7MassKind.PlateNonStructural);
        Assert.AreEqual(5e-8, p.Values[0], 1e-20, "50 kg/m2 = 5e-8 t/mm2."); Assert.AreEqual(.8, p.DynamicFactor);
        var reference = data.Temperatures.Single(x => x.Kind == Straus7TemperatureKind.ReferenceTemperature);
        Assert.AreEqual(Lc(1, "Loads"), reference.Case); Assert.AreEqual(15, reference.Values[0]);
        var node = data.Temperatures.Single(x => x.Kind == Straus7TemperatureKind.NodeTemperature);
        Assert.AreEqual(35, node.Values[0]); Assert.AreEqual("Reference", node.Type); Assert.AreEqual(1, node.Number);
        var beam = data.Temperatures.Single(x => x.Kind == Straus7TemperatureKind.BeamGradient);
        CollectionAssert.AreEqual(new[] { .01, -.005 }, beam.Values, "10 °C/m = 0.01 °C/mm.");
        Assert.AreEqual(.02, data.Temperatures.Single(x => x.Kind == Straus7TemperatureKind.PlateGradient).Values[0], 1e-15);
        var inertia = data.Inertia.Single();
        Assert.AreEqual(Lc(2, "Gravity"), inertia.Case); Assert.IsTrue(inertia.StructuralMass); Assert.IsFalse(inertia.NonStructuralMass);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "Straus7MassesAndTemperaturesAsData"));
        Assert.AreEqual(0, report.Model.Stages.Count);
    }

    [TestMethod]
    public void Stages_AreModelStagesWithGroupChangesAndActiveElements()
    {
        var api = new ExtendedApi();
        api.Stages.Add(new Straus7Stage { Number = 1, Name = "Empty", Data = new[] { 0, 0, 0, 0, 0 }, EnabledGroups = new int[0] });
        api.Stages.Add(new Straus7Stage { Number = 2, Name = "All", Data = new[] { 1, 1, 0, 0, 0 }, EnabledGroups = new[] { 1 } });
        api.Stages.Add(new Straus7Stage { Number = 3, Name = "Removed", Data = new[] { 0, 0, 0, 0, 1 }, EnabledGroups = new int[0] });
        var report = Import(api); Partial(report); var m = report.Model;
        var stages = m.Stages.Values.OrderBy(s => s.Definition.Sequence).ToArray();
        CollectionAssert.AreEqual(new[] { "Stage 1: Empty", "Stage 2: All", "Stage 3: Removed" }, stages.Select(s => s.Name).ToArray());
        Assert.AreEqual(0, stages[0].Definition.GroupChanges.Count);
        Assert.AreEqual(StageGroupAction.Activate, stages[1].Definition.GroupChanges.Single().Action); Assert.AreEqual("Group 1: Model", stages[1].Definition.GroupChanges.Single().GroupName);
        Assert.AreEqual(StageGroupAction.Deactivate, stages[2].Definition.GroupChanges.Single().Action);
        var data = Straus7ModelData.Read(m).Stages;
        Assert.AreEqual(Straus7StageType.Unmorphed, data[0].Type); Assert.AreEqual(Straus7StageType.Morphed, data[1].Type); Assert.AreEqual(Straus7StageType.Reset, data[2].Type);
        Assert.IsTrue(data[1].MoveFixedNodes);
        CollectionAssert.AreEqual(new[] { 1, 2 }, data[1].ActiveBeams); CollectionAssert.AreEqual(new[] { 1 }, data[1].ActivePlates);
        Assert.AreEqual(0, data[0].ActiveBeams.Length + data[2].ActiveBeams.Length);
        Assert.IsInstanceOfType(m.LoadCases[Straus7StageResults.NonlinearCase], typeof(StageResultCase), "Output case of staged nonlinear results.");
        Assert.IsTrue(report.Preserved.Any(x => x.SourceRecord == "Stage/2"));
        Assert.AreEqual("2: Live half", Straus7StageResults.Step(new Straus7ResultCase { Number = 2, Name = "2: Live half [Complete][U]", Stage = 2 }));
        Assert.AreEqual("4: x", Straus7StageResults.Step(new Straus7ResultCase { Number = 4, Name = "4: x", Stage = 1 }), "Without the stage suffix the name is kept.");
    }
}
