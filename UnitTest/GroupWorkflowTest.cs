using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Examples;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class GroupWorkflowTest
    {
        [TestMethod] public void MixedGroupsSelectFamiliesAndDescendantsWithoutDuplicates()
        {
            var m=PostProcessingTest.Mixed();var parent=m.AddGroup("Spalla");var child=m.AddGroup("Parete",parent);
            Assert.AreSame(parent,child.Parent);Assert.AreSame(child,parent.Childs.Single());
            m.AssignGroup("Spalla",new Element[]{m.BeamElements[10]});
            m.AssignGroup("Parete",new Element[]{m.BeamElements[10],m.AreaElements[10]});
            Assert.AreEqual(1,m.GetGroupElements("Spalla").Count);Assert.AreEqual(2,m.GetGroupElements("Spalla",true).Count);
            Assert.AreSame(m.AreaElements[10],m.GetGroupElements("Spalla",true,EntityFamily.Shell).Single());
            m.SetGroup(m.GetGroupElements("Parete"),"Parete");Assert.AreEqual(2,m.GetGroupElements("Parete").Count);
        }
        [TestMethod] public void GroupRenameUpdatesMembershipAndZoneButNotFemRevision()
        {
            var m=MixedModelFactory.Create();var before=m.AnalysisFingerprint();var g=m.Groups["Wall"];
            m.AreaElements.Values.First().Assignments.ReinforcementZone="Wall";m.RenameGroup("Wall","Parete");
            Assert.AreSame(g,m.Groups["Parete"]);Assert.IsFalse(m.Groups.ContainsKey("Wall"));
            Assert.IsTrue(m.GetGroupElements("Parete").All(e=>!e.Groups.ContainsKey("Wall")));
            Assert.AreEqual("Parete",m.AreaElements.Values.First().Assignments.ReinforcementZone);Assert.AreEqual(before,m.AnalysisFingerprint());
            m.AddGroup("Other");Assert.ThrowsException<InvalidOperationException>(()=>m.RenameGroup("Parete","Other"));
            Assert.AreSame(g,m.Groups["Parete"]);
        }
        [TestMethod] public void ReparentAndBulkChildrenRejectCyclesWithoutPartialChanges()
        {
            var m=new Model();var a=m.AddGroup("A");var b=m.AddGroup("B",a);var c=m.AddGroup("C");
            Assert.ThrowsException<InvalidOperationException>(()=>m.ReparentGroup("A","B"));Assert.IsNull(a.Parent);
            Assert.ThrowsException<InvalidOperationException>(()=>b.AddChilds(new[]{c,a}));Assert.IsNull(c.Parent);Assert.AreEqual(0,b.Childs.Count);
            m.ReparentGroup("B","C");Assert.AreSame(c,b.Parent);Assert.AreEqual(0,a.Childs.Count);
            m.ReparentGroup("B",null);Assert.IsNull(b.Parent);Assert.AreEqual(0,c.Childs.Count);
        }
        [TestMethod] public void GroupAssignmentsValidateEveryMemberAndGroupBeforeMutation()
        {
            var m=PostProcessingTest.Mixed();m.AddGroup("A");
            Assert.ThrowsException<InvalidOperationException>(()=>m.AssignGroup("A",new Element[]{m.BeamElements[10],new NodeElement(GPC.Geometry.Point3d.Origin)}));
            Assert.AreEqual(0,m.BeamElements[10].Groups.Count);
            Assert.ThrowsException<KeyNotFoundException>(()=>m.SetGroupRange(new[]{m.BeamElements[10]},new[]{"A","missing"}));
            Assert.AreEqual(0,m.BeamElements[10].Groups.Count);
        }
        [TestMethod] public void RemovalRequiresExplicitDetachAndNeverDeletesElements()
        {
            var m=PostProcessingTest.Mixed();m.AddGroup("A");m.AssignGroup("A",new[]{m.BeamElements[10]});
            Assert.ThrowsException<InvalidOperationException>(()=>m.RemoveGroup("A"));
            m.RemoveGroup("A",GroupRemovalPolicy.DetachMemberships);Assert.AreEqual(1,m.BeamElements.Count);Assert.AreEqual(0,m.BeamElements[10].Groups.Count);
        }
        [TestMethod] public void HierarchyAndMixedMembershipSurviveArchiveAndLegacyCorruptionIsDiagnosed()
        {
            var m=PostProcessingTest.Mixed();var a=m.AddGroup("A");var b=m.AddGroup("B",a);m.AssignGroup("B",new Element[]{m.BeamElements[10],m.AreaElements[10]});
            using var s=new MemoryStream();ModelArchive.Save(m,s);s.Position=0;var copy=ModelArchive.Load(s);
            Assert.AreSame(copy.Groups["A"],copy.Groups["B"].Parent);Assert.AreEqual(2,copy.GetGroupElements("A",true).Count);
            Assert.AreEqual(0,copy.ValidateGroups().Count);copy.Groups["A"].Childs.Clear();
            Assert.IsTrue(copy.ValidateGroups().Any(d=>d.Code=="GroupHierarchyMismatch"));
            Assert.ThrowsException<InvalidOperationException>(()=>copy.GetGroupElements("A",true));
        }
    }
}
