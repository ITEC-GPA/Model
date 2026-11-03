using GPC.Converter.Straus7;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Materials;
using GPC.Model.Models;
using GPC.Model.Sections;

namespace UnitTest;

/// <summary>Vocabulary needed by the BridGHe adapter (F6A, lane P2B): identities supplied by the plug-in for groups and typed
/// load cases, and Straus7 group names unique only among siblings.</summary>
[TestClass]
public class BridgheAdapterVocabularyTest
{
    [TestMethod]
    public void GroupsWithASuppliedIdentityAreTheSameGroupInIndependentBranches()
    {
        var id = new Guid("8b1d7c55-6a33-4f62-9d1b-0e4a5c2f9a10");
        var a = new Group(id, "Girder"); var b = new Group(id, "Girder");
        Assert.AreEqual(id, a.Guid); Assert.IsNull(a.Parent); Assert.AreEqual(0, a.Childs.Count);
        var parent = new Group(new Guid("6c2e1f08-3a5d-4e7b-9c10-2b4d6f8a0c13"), "Deck");
        var child = new Group(new Guid("2f6a9d10-0b44-4b0c-8a3e-7d1c5e9b2a64"), "Deck\\Slab"); parent.AddChild(child);
        Assert.AreSame(parent, child.Parent); Assert.AreSame(child, parent.Childs.Single());
        Assert.ThrowsException<ArgumentException>(() => new Group(Guid.Empty, "x"));
        Assert.ThrowsException<ArgumentException>(() => new Group(id, " "));
        var material = new Material("S355", 210000, 0.3, 7.85e-9, 1.2e-5); var property = new ElasticBeamProperty("p", material, new SectionRectangular(200, 100));
        var n1 = new NodeElement(new Guid("00000000-0000-0000-0000-000000000001"), new Point3d(0, 0, 0));
        var n2 = new NodeElement(new Guid("00000000-0000-0000-0000-000000000002"), new Point3d(1000, 0, 0));
        var n3 = new NodeElement(new Guid("00000000-0000-0000-0000-000000000003"), new Point3d(2000, 0, 0));
        var b1 = new BeamElement(new Guid("00000000-0000-0000-0000-000000000011"), n1.Position, n2.Position, property); b1.ConnectNodes(n1, n2); b1.AddGroup(a); b1.Assignments.Formulation = GPC.Model.Structure.Assignments.BeamFormulation.StraightTwoNode;
        var b2 = new BeamElement(new Guid("00000000-0000-0000-0000-000000000012"), n2.Position, n3.Position, property); b2.ConnectNodes(n2, n3); b2.AddGroup(b); b2.Assignments.Formulation = GPC.Model.Structure.Assignments.BeamFormulation.StraightTwoNode;
        var model = ModelAssembly.Assemble(new Element[] { b1, b2 }, new Guid("00000000-0000-0000-0000-0000000000aa"), "branches");
        Assert.AreEqual(1, model.Groups.Count);
        Assert.AreEqual(id, model.Groups["Girder"].Guid);
        Assert.IsTrue(model.BeamElements.Values.All(e => ReferenceEquals(e.Groups["Girder"], model.Groups["Girder"])));
    }

    [TestMethod]
    public void TypedLoadCasesKeepTheSuppliedIdentity()
    {
        var id = new Guid("5a0c3e77-1d2b-4f6e-a3c4-9b8d7e6f5a41");
        var dead = new LoadCase(id, "Dead", LoadCase.LoadCaseTypes.SelfWeight);
        Assert.AreEqual(id, dead.Guid); Assert.AreEqual("Dead", dead.Name); Assert.AreEqual(LoadCase.LoadCaseTypes.SelfWeight, dead.LoadCaseType);
        var gravity = new ModelGravityLoad(new Vector3d(0, 0, 1), -9806.65, dead, CoordinateSystem.Global);
        Assert.AreSame(dead, gravity.LoadCase);
        Assert.ThrowsException<ArgumentException>(() => new LoadCase(Guid.Empty, "Dead", LoadCase.LoadCaseTypes.SelfWeight));
        var copy = GPC.Model.Core.ModelValues.CopyValue(dead);
        Assert.AreEqual(id, copy.Guid); Assert.AreEqual(LoadCase.LoadCaseTypes.SelfWeight, copy.LoadCaseType);
    }

    [TestMethod]
    public void StagesAndFreedomCasesKeepTheSuppliedIdentity()
    {
        var stageId = new Guid("7d3a51c2-9e04-4b8f-a6d1-35c8e2f90b17"); var freedomId = new Guid("1e9b6c4d-02af-4f37-8b55-d6a0c3e8f241");
        var stage = new GPC.Model.Stages.Stage(stageId, 3, "Deck");
        Assert.AreEqual(stageId, stage.Guid); Assert.AreEqual(3, stage.Id); Assert.AreEqual("Deck", stage.Name);
        var freedom = new GPC.Model.FreedomCases.FreedomCase(freedomId, "Default restraints");
        Assert.AreEqual(freedomId, freedom.Guid); Assert.AreEqual("Default restraints", freedom.Name);
        Assert.ThrowsException<ArgumentException>(() => new GPC.Model.Stages.Stage(Guid.Empty, 1, "x"));
        Assert.ThrowsException<ArgumentException>(() => new GPC.Model.FreedomCases.FreedomCase(Guid.Empty, "x"));
        Assert.AreEqual(freedomId, GPC.Model.Core.ModelValues.CopyValue(freedom).Guid);
    }

    [TestMethod]
    public void PathNamedChildGroupsAreWrittenWithTheirLeafName()
    {
        var model = Straus7ModelWriteTest.Cantilever(); var beam = model.BeamElements.Values.First();
        var girder = model.AddGroup("Girder"); var segment = model.AddGroup("Girder\\Group1", girder); var other = model.AddGroup("Bearing/S1/G1");
        beam.AddGroup(segment);
        var calls = Straus7ExporterCorrectionsTest.Record(model).Named(nameof(IStraus7ModelWriteApi.CreateGroup));
        var byName = calls.ToDictionary(c => (string)c.Args[2]!, c => ((int)c.Args[0]!, (int)c.Args[1]!));
        Assert.IsTrue(byName.ContainsKey("Group1"), "The child is written with its leaf name.");
        Assert.AreEqual(byName["Girder"].Item1, byName["Group1"].Item2, "The leaf is written under its native parent.");
        Assert.IsTrue(byName.ContainsKey("Bearing/S1/G1"), "A root group keeps its whole name.");
        Assert.IsFalse(byName.ContainsKey("Girder\\Group1"));
    }
}
