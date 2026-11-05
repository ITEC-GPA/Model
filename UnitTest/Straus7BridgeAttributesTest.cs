using System.Reflection;
using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Models;
using GPC.Model.Structure.Bridges;
using GPC.Model.Structure.Members;

namespace UnitTest;

/// <summary>The descriptive bridge attributes (lane L11: member, super element and section state of a beam, rigid connection of a node) are
/// skipped by the Straus7 writer as the element labels (<see cref="Straus7ElementLabelTest"/>): the same native calls with and without them.
/// On the wrong kind of element the domain preflight of the writer refuses them; any other unmapped attribute is still rejected. Plan only
/// (recording proxy), no native writer.</summary>
[TestClass]
public class Straus7BridgeAttributesTest
{
    private static List<string> Written(Model model)
    {
        var api = DispatchProxy.Create<ReviewFixesL6Test.IForwardedWriter, Straus7ElementLabelTest.Recorder>();
        Straus7ModelWritePlan.Prepare(model).Write(api, model);
        return ((Straus7ElementLabelTest.Recorder)(object)api).Calls;
    }

    private static void Decorate(Model model)
    {
        foreach (var beam in model.BeamElements.Values)
        {
            beam.Attributes.Add(new BridgeMemberAttribute(new BridgeMemberMetadata("G" + beam.Id, BridgeMemberFamily.Girder, 1, beam.Id, 1, 0, 1000, new Vector3d(0, 0, 1), "G1-C" + beam.Id)));
            beam.Attributes.Add(new SuperElementAttribute("G1"));
            beam.Attributes.Add(new BridgeSectionStateAttribute(BridgeSectionBehavior.ShortTerm, beam.Id % 2 == 0, 6.25, "M03_N0", "F" + beam.Id));
        }
        var nodes = model.NodesElements.Values.OrderBy(n => n.Id).ToArray();
        for (int i = 0; i < nodes.Length; i++)
        {
            // Every node is linked to the next one; the first also to a point that is not a node.
            nodes[i].Attributes.Add(new RigidConnectionAttribute(nodes[(i + 1) % nodes.Length].Position));
            if (i == 0) nodes[i].Attributes.Add(new RigidConnectionAttribute(new Point3d(nodes[i].Position.X, nodes[i].Position.Y, nodes[i].Position.Z - 1500)));
        }
    }

    [TestMethod]
    public void BridgeAttributesOfNodesAndBeamsAreNotWritten()
    {
        foreach (var build in new Func<Model>[] { () => Straus7ModelWriteTest.Cantilever(), () => Straus7MaterialLawCorrectionsTest.Plate(null) })
        {
            // The same model, written before and after the attributes are added.
            var model = build(); var expected = Written(model); string fingerprint = model.AnalysisFingerprint();
            Decorate(model);
            Assert.IsTrue(model.NodesElements.Count > 1 && model.NodesElements.Values.All(n => n.Attributes.Values.OfType<RigidConnectionAttribute>().Any()));
            Assert.IsTrue(model.BeamElements.Values.All(b => b.Attributes.Count >= 3));
            Assert.AreEqual(fingerprint, model.AnalysisFingerprint());
            CollectionAssert.AreEqual(expected, Written(model));
            // Together with the labels.
            foreach (var element in model.AllElements) element.Attributes.Add(new ElementLabelAttribute("E" + element.Id));
            CollectionAssert.AreEqual(expected, Written(model));
        }
        Assert.IsTrue(Straus7ModelWriteTest.Cantilever().BeamElements.Count > 0, "the cantilever has beams");
    }

    [TestMethod]
    public void MisplacedBridgeAttributesAndOtherAttributesAreRefused()
    {
        // A beam attribute on a plate, a rigid connection on a beam or a plate: refused by the domain preflight of the writer.
        var plate = Straus7MaterialLawCorrectionsTest.Plate(null);
        plate.AreaElements.Values.Single().Attributes.Add(new SuperElementAttribute("G1"));
        StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => Written(plate)).Message, "BridgeBeamAttributeOnWrongElement");
        var cantilever = Straus7ModelWriteTest.Cantilever();
        cantilever.BeamElements.Values.First().Attributes.Add(new RigidConnectionAttribute(new Point3d(0, 0, 0)));
        StringAssert.Contains(Assert.ThrowsException<InvalidOperationException>(() => Written(cantilever)).Message, "RigidConnectionOnWrongElement");
        // An attribute the writer does not map is still rejected next to the bridge attributes.
        var released = Straus7MaterialLawCorrectionsTest.Plate(null);
        Decorate(released);
        released.AreaElements.Values.Single().Attributes.Add(new BeamReleasesAttribute());
        StringAssert.Contains(Assert.ThrowsException<NotSupportedException>(() => Written(released)).Message, "UnsupportedStrausPlateAttributes");
    }
}
