using System.Reflection;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Models;

namespace UnitTest;

/// <summary>The descriptive rigid connection of a node (lane L11; the bridge data of the beams left GPCModel with lane L12) is skipped by
/// the Straus7 writer as the element labels (<see cref="Straus7ElementLabelTest"/>): the same native calls with and without it. On the
/// wrong kind of element the domain preflight of the writer refuses it; any other unmapped attribute is still rejected. Plan only
/// (recording proxy), no native writer.</summary>
[TestClass]
public class Straus7RigidConnectionTest
{
    private static List<string> Written(Model model)
    {
        var api = DispatchProxy.Create<ReviewFixesL6Test.IForwardedWriter, Straus7ElementLabelTest.Recorder>();
        Straus7ModelWritePlan.Prepare(model).Write(api, model);
        return ((Straus7ElementLabelTest.Recorder)(object)api).Calls;
    }

    private static void Decorate(Model model)
    {
        var nodes = model.NodesElements.Values.OrderBy(n => n.Id).ToArray();
        for (int i = 0; i < nodes.Length; i++)
        {
            // Every node is linked to the next one; the first also to a point that is not a node.
            nodes[i].Attributes.Add(new RigidConnectionAttribute(nodes[(i + 1) % nodes.Length].Position));
            if (i == 0) nodes[i].Attributes.Add(new RigidConnectionAttribute(new Point3d(nodes[i].Position.X, nodes[i].Position.Y, nodes[i].Position.Z - 1500)));
        }
    }

    [TestMethod]
    public void RigidConnectionsOfTheNodesAreNotWritten()
    {
        foreach (var build in new Func<Model>[] { () => Straus7ModelWriteTest.Cantilever(), () => Straus7MaterialLawCorrectionsTest.Plate(null) })
        {
            // The same model, written before and after the attributes are added.
            var model = build(); var expected = Written(model); string fingerprint = model.AnalysisFingerprint();
            Decorate(model);
            Assert.IsTrue(model.NodesElements.Count > 1 && model.NodesElements.Values.All(n => n.Attributes.Values.OfType<RigidConnectionAttribute>().Any()));
            Assert.AreEqual(fingerprint, model.AnalysisFingerprint());
            CollectionAssert.AreEqual(expected, Written(model));
            // Together with the labels.
            foreach (var element in model.AllElements) element.Attributes.Add(new ElementLabelAttribute("E" + element.Id));
            CollectionAssert.AreEqual(expected, Written(model));
        }
        Assert.IsTrue(Straus7ModelWriteTest.Cantilever().BeamElements.Count > 0, "the cantilever has beams");
    }

    [TestMethod]
    public void MisplacedRigidConnectionsAndOtherAttributesAreRefused()
    {
        // A rigid connection on a beam or a plate: refused by the domain preflight of the writer.
        var cantilever = Straus7ModelWriteTest.Cantilever();
        cantilever.BeamElements.Values.First().Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, 0)));
        StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => Written(cantilever)).Message, "RigidConnectionOnWrongElement");
        var plate = Straus7MaterialLawCorrectionsTest.Plate(null);
        plate.AreaElements.Values.Single().Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, 0)));
        StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => Written(plate)).Message, "RigidConnectionOnWrongElement");
        // An attribute the writer does not map is still rejected next to the rigid connections.
        var released = Straus7MaterialLawCorrectionsTest.Plate(null);
        Decorate(released);
        released.AreaElements.Values.Single().Attributes.Add(new BeamReleasesAttribute());
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Written(released)).Message, "UnsupportedStrausPlateAttributes");
    }
}
