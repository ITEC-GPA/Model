using System.Reflection;
using GPC.Converter.Straus7;
using GPC.Model.Attributes;
using GPC.Model.Models;

namespace UnitTest;

/// <summary>Element labels (<see cref="ElementLabelAttribute"/>: the Element Id of BridGHe and Arachne) are descriptive: the Straus7 writer
/// writes the same native model with and without them (BridGHe F6, C1: the legacy reader keeps the Element Id as a label). Any other
/// attribute the writer does not map is still rejected. Plan only (recording proxy), no native writer.</summary>
[TestClass]
public class Straus7ElementLabelTest
{
    public class Recorder : DispatchProxy
    {
        internal readonly List<string> Calls = new();
        protected override object? Invoke(MethodInfo? method, object?[]? args)
        {
            Calls.Add(method!.Name + "(" + string.Join(", ", (args ?? Array.Empty<object?>()).Select(Text)) + ")");
            var type = method.ReturnType;
            return type == typeof(void) || !type.IsValueType ? null : Activator.CreateInstance(type);
        }
        private static string Text(object? value) => value switch
        {
            null => "null",
            double d => BitConverter.DoubleToInt64Bits(d).ToString("X16"),
            System.Collections.IEnumerable e and not string => "[" + string.Join(" ", e.Cast<object?>().Select(Text)) + "]",
            _ => value.ToString() ?? "",
        };
    }

    private static List<string> Written(Model model)
    {
        var api = DispatchProxy.Create<ReviewFixesL6Test.IForwardedWriter, Recorder>();
        Straus7ModelWritePlan.Prepare(model).Write(api, model);
        return ((Recorder)(object)api).Calls;
    }

    [TestMethod]
    public void ElementLabelsOfNodesBeamsAndPlatesAreNotWritten()
    {
        foreach (var build in new Func<Model>[] { () => Straus7ModelWriteTest.Cantilever(), () => Straus7MaterialLawCorrectionsTest.Plate(null) })
        {
            // The same model (the native numbering follows the element GUIDs), written before and after labelling its elements.
            var model = build(); var expected = Written(model);
            foreach (var element in model.AllElements) element.Attributes.Add(new ElementLabelAttribute("E" + element.Id));
            Assert.IsTrue(model.AllElements.Count() > 1 && model.AllElements.All(e => e.Attributes.Values.OfType<ElementLabelAttribute>().Any()));
            CollectionAssert.AreEqual(expected, Written(model));
        }
        // Another attribute of a plate is still refused.
        var plate = Straus7MaterialLawCorrectionsTest.Plate(null);
        plate.AreaElements.Values.Single().Attributes.Add(new BeamReleasesAttribute());
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Straus7ModelWritePlan.Prepare(plate).Write(DispatchProxy.Create<ReviewFixesL6Test.IForwardedWriter, Recorder>(), plate)).Message,
            "UnsupportedStrausPlateAttributes");
    }
}
