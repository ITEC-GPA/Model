using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Model.Attributes;
using GPC.Model.Structure.Assignments;
using GPC.Model.Persistence;
using GPC.Model.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace UnitTest;

[TestClass]
public class CivilNxBoundaryTest
{
    static ImportReport Import(params (string Table, string Json)[] replacements)
    {
        var tables = CivilNxModelWorkflowTest.Json();
        foreach (var (table, json) in replacements) tables[table] = json;
        return CivilNxGeometryReader.Import(CivilNxModelWorkflowTest.Snapshot(tables), CivilNxModelWorkflowTest.Identity(), new CivilNxModelProfile());
    }
    const string Relative = @"{""FRLS"":{""3"":{""ITEMS"":[{""FLAG_I"":""0000110"",""VALUE_I"":[0,0,0,0,0,0.25,0],""FLAG_J"":""0000000""}]}}}";

    [TestMethod] public void RelativeReleases_PreserveSixDofsAndTheirExplicitLocalAxes()
    {
        var report = Import(("FRLS", Relative)); Assert.IsNotNull(report.Model);
        var release = report.Model.BeamElements.Values.SelectMany(b => b.Attributes.Values.OfType<BeamReleasesAttribute>()).Single();
        Assert.AreEqual(BeamConnectionKind.Continuous, release.I[0].Kind);
        Assert.AreEqual(BeamConnectionKind.Released, release.I[4].Kind);
        Assert.AreEqual(BeamConnectionKind.RelativeFixity, release.I[5].Kind); Assert.AreEqual(.25, release.I[5].Value);
        Assert.IsTrue(release.J.All(d => d.Kind == BeamConnectionKind.Continuous));
        Assert.AreEqual(1, release.CoordinateSystem.V1.X, 1e-12); // MIDAS x follows this horizontal beam.
        Assert.AreEqual(1, release.CoordinateSystem.V2.Y, 1e-12);
        Assert.AreEqual(1, release.CoordinateSystem.V3.Z, 1e-12);
        Assert.IsFalse(report.Model.ValidateAssignments().Any(d => d.Code.Contains("Release")));
    }
    [TestMethod] public void AbsoluteReleases_ConvertTranslationAndRotationStiffnessIndependently()
    {
        var report = Import(("FRLS", @"{""FRLS"":{""3"":{""ITEMS"":[{""bVALUE"":true,""FLAG_I"":""100001"",""VALUE_I"":[12,0,0,0,0,20],""FLAG_J"":""000000""}]}}}"));
        var r = report.Model.BeamElements.Values.SelectMany(b => b.Attributes.Values.OfType<BeamReleasesAttribute>()).Single();
        Assert.AreEqual(12, r.I[0].Value, 1e-12); // 12 kN/m = 12 N/mm.
        Assert.AreEqual(20000000, r.I[5].Value, 1e-8); // 20 kN m/rad = 20,000,000 N mm/rad.
    }
    [TestMethod] public void RigidLinks_KeepTheRequestedEquationsAndSignedLeverArms()
    {
        var report = Import(("RIGD", @"{""RIGD"":{""1"":{""ITEMS"":[{""ID"":4,""DOF"":110001,""S_NODE"":[3]}]}}}"));
        var first = report.Model.NodesElements.Values.Single(n => n.Source.OriginalId == "1");
        var link = first.Assignments.Links.Single();
        Assert.AreEqual(36, link.KinematicCoefficients.Length); Assert.AreEqual(3, link.RightHandSide.Length);
        Assert.AreEqual(3000, link.KinematicCoefficients[4]); Assert.AreEqual(-1, link.KinematicCoefficients[6]);
        Assert.AreEqual(-3000, link.KinematicCoefficients[12 + 3]); Assert.AreEqual(4000, link.KinematicCoefficients[12 + 5]);
        Assert.AreEqual(1, link.KinematicCoefficients[24 + 5]); Assert.AreEqual(-1, link.KinematicCoefficients[24 + 11]);
        Assert.AreEqual(4000, link.LeverArm.X); Assert.AreEqual(3000, link.LeverArm.Z);
    }
    [TestMethod] public void ElasticLinks_KeepTheFullMatrixAndSourceOrientation()
    {
        var report = Import(("ELNK", @"{""ELNK"":{""1"":{""NODE"":[2,3],""LINK"":""GEN"",""ANGLE"":90,""SDR"":[11,12,13,14,15,16],""R_S"":[false,false,false,false,false,false],""bSHEAR"":false}}}"));
        var link = report.Model.NodesElements.Values.SelectMany(n => n.Assignments.Links).Single();
        Assert.AreEqual(11, link.Spring[0,0]); Assert.AreEqual(16000000, link.Spring[5,5]);
        Assert.AreEqual(0, link.Spring[1,2]); Assert.IsNull(link.KinematicCoefficients);
        Assert.AreEqual(1, link.Spring.CoordinateSystem.V2.Z, 1e-12); Assert.AreEqual(-1, link.Spring.CoordinateSystem.V3.Y, 1e-12);
    }
    [TestMethod] public void UnsupportedLawsAndWarping_ArePreservedWithoutAFalseEquivalent()
    {
        var report = Import(("FRLS", Relative.Replace("0000110", "0000111")),
            ("ELNK", @"{""ELNK"":{""1"":{""NODE"":[2,3],""LINK"":""TENS"",""SDR"":[11,0,0,0,0,0]}}}"));
        Assert.IsNotNull(report.Model);
        Assert.IsFalse(report.Model.BeamElements.Values.Any(b => b.Attributes.Values.OfType<BeamReleasesAttribute>().Any()));
        Assert.IsFalse(report.Model.NodesElements.Values.Any(n => n.Assignments.Links.Count > 0));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilNxWarpingReleasePreserved"));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilNxElasticLinkLawPreserved"));
        Assert.IsTrue(report.Preserved.Any(p => p.RawData.Contains("TENS")));
    }
    [TestMethod] public void Stages_DoNotBecomePermanentBoundaryConditions()
    {
        var report = Import(("FRLS", Relative), ("STAG", @"{""STAG"":{""1"":{""NAME"":""stage""}}}"));
        Assert.IsFalse(report.Model.BeamElements.Values.Any(b => b.Attributes.Values.OfType<BeamReleasesAttribute>().Any()));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilNxStagedBoundaryPreserved"));
    }
    [TestMethod] public void InvalidReferencesAndRelativeCoefficients_AreRejectedAtomically()
    {
        Assert.IsNull(Import(("FRLS", Relative.Replace("0.25", "1.2"))).Model);
        Assert.IsNull(Import(("RIGD", @"{""RIGD"":{""1"":{""ITEMS"":[{""DOF"":111111,""S_NODE"":[999]}]}}}")).Model);
        Assert.IsNull(Import(("ELNK", @"{""ELNK"":{""1"":{""NODE"":[2,3],""LINK"":""GEN"",""SDR"":[-1,0,0,0,0,0]}}}")).Model);
    }
    [TestMethod] public void ImportedBoundaries_RoundTripAndParticipateInTheAnalysisFingerprint()
    {
        var model = Import(("FRLS", Relative), ("RIGD", @"{""RIGD"":{""1"":{""ITEMS"":[{""DOF"":111111,""S_NODE"":[3]}]}}}")).Model;
        var before = model.AnalysisFingerprint();
        using var stream = new MemoryStream(); ModelArchive.SaveDocument(model, stream); stream.Position = 0;
        var copy = ModelArchive.Load(stream);
        Assert.AreEqual(before, copy.AnalysisFingerprint());
        var link = copy.NodesElements.Values.SelectMany(n => n.Assignments.Links).Single();
        Assert.IsTrue(copy.NodesElements.ContainsKey(link.OtherNodeId));
        Assert.AreEqual(72, link.KinematicCoefficients.Length);
        var release = copy.BeamElements.Values.SelectMany(b => b.Attributes.Values.OfType<BeamReleasesAttribute>()).Single();
        Assert.AreEqual(.25, release.I[5].Value);
        release.I[5] = new BeamDofConnection(BeamConnectionKind.RelativeFixity, .5);
        Assert.AreNotEqual(before, copy.AnalysisFingerprint());
    }
}
