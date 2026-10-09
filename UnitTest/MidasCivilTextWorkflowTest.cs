using System.Globalization;
using System.Text;
using GPC.Converter;
using GPC.Converter.MidasCivil;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Restraints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Analysis;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;

namespace UnitTest;

[TestClass]
public class MidasCivilTextWorkflowTest
{
    // Synthetic documented command records, NOT an export from an installed MIDAS solver.
    private const string Mixed = @"; synthetic Civil input
*VERSION
fixture-release-A
*UNIT
KN,M,KCAL,C
*NODE
10,0,0,0
20,2,0,0
30,2,1,0
40,0,1,0
*ELEMENT
101,BEAM,1,2,10,20,0,0
102,PLATE,1,3,10,20,30,40,1,0
103,PLATE,1,3,10,30,40,0,2,0
*GROUP
""Deck, west; side"",10 to 40 by 10,101 to 103,0
Empty,,,0
*MATERIAL
1,CONC,""synthetic material"",0,0,,C,NO,0.05,2,30000000,0.2,0.00001,25,0
*SECTION
2,DBUSER,synthetic section,CC,0,0,0,0,0,0,YES,NO,SB,2,0.3,0.5
*THICKNESS
3,VALUE,YES,0.2,0
*STLDCASE
G,D,synthetic dead load
Q,L,synthetic live load
*USE-STLD,G
*CONLOAD
10 20,1,2,-3,4,-5,6,Loads G
10,1,2,-3,4,-5,6,Loads G
*USE-STLD,Q
*CONLOAD
20,0,0,-7,0,0,0,
*CONSTRAINT
10,111000,Base
10,000111,Base
40,001000,
*ENDDATA
";
    private static ImportReport Import(string text) => Import(Encoding.UTF8.GetBytes(text));
    private static ImportReport Import(byte[] data)
    {
        using var stream = new MemoryStream(data);
        var report = new MidasCivilTextReader("synthetic-revision").Import(stream);
        Assert.IsTrue(stream.CanRead, "Reader must leave the caller's stream open."); return report;
    }
    private static void Partial(ImportReport report)
    {
        Assert.AreEqual(ImportStatus.Partial, report.Status, string.Join("; ", report.Diagnostics.Select(d => d.Message)));
        Assert.IsNotNull(report.Model); Assert.IsFalse(report.VerificationEnabled);
        Assert.IsFalse(report.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
    }
    private static NodeElement Node(GPC.Model.Models.Model m, string id) =>
        (NodeElement)m.FindBySource(new SourceIdentity("MIDAS Civil", "synthetic-revision", EntityFamily.Node, id));
    private static string BeamText(string j, string angle = "0") =>
        "*UNIT\nN,MM\n*NODE\n1,0,0,0\n2," + j + "\n*ELEMENT\n1,BEAM,1,1,1,2," + angle + ",0\n*ENDDATA\n";

    [TestMethod]
    public void MixedModel_MapsSharedNodesGroupsLoadsConstraints_AndKeepsUnmappedProperties()
    {
        var report = Import(Mixed); Partial(report); var m = report.Model;
        Assert.AreEqual(4, m.NodesElements.Count); Assert.AreEqual(1, m.BeamElements.Count); Assert.AreEqual(2, m.AreaElements.Count);
        var beam = m.BeamElements.Values.Single();
        Assert.AreEqual(2000, beam.Length, 1e-9); Assert.AreSame(Node(m, "10"), beam.NodeI);
        Assert.AreEqual(7, m.GetGroupElements("Deck, west; side").Count); Assert.AreEqual(0, m.GetGroupElements("Empty").Count);
        Assert.AreSame(m.Groups["Deck, west; side"], beam.Groups["Deck, west; side"]);
        Assert.AreEqual(2, Node(m, "10").Loads.Count, "Coincident source loads must accumulate, not deduplicate.");
        var load = Node(m, "10").Loads.Values.Cast<PointLoad>().First();
        Assert.AreEqual(1000, load.F1); Assert.AreEqual(2000, load.F2); Assert.AreEqual(-3000, load.F3);
        Assert.AreEqual(4000000, load.M1); Assert.AreEqual(-5000000, load.M2); Assert.AreEqual(6000000, load.M3);
        Assert.AreSame(m.LoadCases["G"], load.LoadCase); Assert.AreSame(Node(m, "10").Position, load.Point);
        var restraint = Node(m, "10").Assignments.Restrains.Single().Restrain;
        Assert.AreSame(Node(m, "10"), restraint.Point); Assert.AreEqual(6, restraint.Restrains.Count);
        Assert.AreEqual(GeometryRestrain.DOF.DZ, Node(m, "40").Assignments.Restrains.Single().Restrain.Restrains.Single().Dof);
        Assert.IsNull(beam.BeamProperty); Assert.IsFalse(beam.Assignments.ActionsAtSectionCentroidConfirmed);
        Assert.IsTrue(beam.Assignments.OtherAssignments.Single().RawData.Contains("101,BEAM,1,2"));
        Assert.IsTrue(m.AreaElements.Values.All(s => s.Assignments.PhysicalThickness == null && s.Assignments.LayerAxes == null));
        Assert.IsTrue(report.Preserved.Any(p => p.Kind == "MIDAS Civil *THICKNESS" && p.RawData.Contains("0.2")));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilInputWithoutResults"));
        using var original = new MemoryStream(Encoding.UTF8.GetBytes(Mixed));
        Assert.AreEqual(SourceEvidence.Sha256(original), m.AnalysisSource.GeometryHash);
        Assert.AreEqual("fixture-release-A", m.AnalysisSource.SolverVersion); Assert.IsNull(m.AnalysisSource.AnalysisId);
    }

    [DataTestMethod]
    [DataRow("2000,0,0", "0", 0d, 1d, 0d, 0d, 0d, 1d, 1d, 0d, 0d)]
    [DataRow("2000,0,0", "90", 0d, 0d, 1d, 0d, -1d, 0d, 1d, 0d, 0d)]
    [DataRow("0,0,2000", "0", 0d, -1d, 0d, 1d, 0d, 0d, 0d, 0d, 1d)]
    [DataRow("0,0,2000", "90", 1d, 0d, 0d, 0d, 1d, 0d, 0d, 0d, 1d)]
    [DataRow("0,0,-2000", "0", 0d, 1d, 0d, 1d, 0d, 0d, 0d, 0d, -1d)]
    public void BeamAxes_MidasXYZ_MapToGpcV3V1V2(string j, string beta, double x1, double y1, double z1,
        double x2, double y2, double z2, double x3, double y3, double z3)
    {
        var report = Import(BeamText(j, beta)); Partial(report);
        var b = report.Model.BeamElements.Values.Single(); var axes = b.Assignments.SectionAxes;
        Assert.AreSame(b.CoordinateSystem, axes); Axes.Validate(axes);
        Vector(axes.V1, x1, y1, z1); Vector(axes.V2, x2, y2, z2); Vector(axes.V3, x3, y3, z3);
    }
    private static void Vector(Vector3d actual, double x, double y, double z)
    { Assert.AreEqual(x, actual.X, 1e-10); Assert.AreEqual(y, actual.Y, 1e-10); Assert.AreEqual(z, actual.Z, 1e-10); }

    [TestMethod]
    public void InclinedBeam_HasProjectedUpAndRightHandedAxes()
    {
        var report = Import(BeamText("1000,0,1000")); Partial(report); var a = report.Model.BeamElements.Values.Single().CoordinateSystem;
        double c = Math.Sqrt(.5); Vector(a.V1, 0, 1, 0); Vector(a.V2, -c, 0, c); Vector(a.V3, c, 0, c);
    }

    [TestMethod]
    public void UnitChangesApplyAtEachRecord_AndCultureDoesNotChangeNumbers()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("it-IT");
            var report = Import("*UNIT\nKN,M\n*NODE\n1,1.25,0,0\n*UNIT\nN,MM\n*NODE\n2,2.5e3,0,0\n*ELEMENT\n1,BEAM,1,1,1,2,0,0\n*STLDCASE\nG,D,\n*USE-STLD,G\n*CONLOAD\n1,1,0,0,2,0,0\n*UNIT\nKN,M\n*CONLOAD\n1,1,0,0,2,0,0\n*ENDDATA");
            Partial(report); Assert.AreEqual(1250, report.Model.BeamElements.Values.Single().Length);
            var loads = Node(report.Model, "1").Loads.Values.Cast<PointLoad>().ToArray();
            CollectionAssert.AreEquivalent(new[] { 1d, 1000d }, loads.Select(l => l.F1).ToArray());
            CollectionAssert.AreEquivalent(new[] { 2d, 2000000d }, loads.Select(l => l.M1).ToArray());
        }
        finally { CultureInfo.CurrentCulture = previous; }
    }

    [TestMethod]
    public void ContinuationsRangesQuotedNamesAndSourceLines_ArePreserved()
    {
        var text = Mixed.Replace("10 to 40 by 10,101 to 103", "10to40by10,101 \\\n 102 103");
        var report = Import(text); Partial(report);
        Assert.AreEqual(7, report.Model.GetGroupElements("Deck, west; side").Count);
        Assert.IsTrue(report.Preserved.Single(p => p.Kind == "MIDAS Civil *GROUP").RawData.Contains("\\\n"));
        var duplicate = Import(BeamText("1000,0,0").Replace("*ELEMENT", "001,0,0,0\n*ELEMENT")); Partial(duplicate);
        Assert.AreEqual(2, duplicate.Model.NodesElements.Count); Assert.IsTrue(duplicate.Diagnostics.Any(d => d.Code == "IdenticalDuplicate"));
    }

    [DataTestMethod]
    [DataRow("*LOCALAXIS\n10,1,0,0,30\n")]
    [DataRow("*STAGE\nNAME=Stage 1\n")]
    public void LocalAxesOrStages_PreserveConstraintsWithoutInventingGlobalSupports(string block)
    {
        var report = Import(Mixed.Replace("*ENDDATA", block + "*ENDDATA")); Partial(report);
        Assert.IsTrue(report.Model.NodesElements.Values.All(n => n.Assignments.Restrains.Count == 0));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "CivilCommandPreserved" && d.Record.StartsWith("*CONSTRAINT")));
        Assert.AreEqual(-3000, Node(report.Model, "10").Loads.Values.Cast<PointLoad>().First().F3);
    }

    [TestMethod]
    public void ReferencePointBeam_IsRetainedWithoutInferredSectionAxes()
    {
        var report = Import(BeamText("1000,0,0").Replace("1,2,0,0", "1,2,REF,0,0,1,0,0")); Partial(report);
        Assert.IsNull(report.Model.BeamElements.Values.Single().Assignments.SectionAxes);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == "BeamReferenceAxesPreserved"));
    }

    [TestMethod]
    public void ArchiveRoundTrip_PreservesIdentityAxesGroupsLoadsAndEvidence()
    {
        var report = Import(Mixed); Partial(report); using var archive = new MemoryStream();
        ModelArchive.Save(report.Model, archive); archive.Position = 0; var restored = ModelArchive.Load(archive);
        Assert.AreEqual(report.Model.AnalysisFingerprint(), restored.AnalysisFingerprint());
        Assert.AreSame(Node(restored, "10"), restored.BeamElements.Values.Single().NodeI);
        Assert.AreEqual(7, restored.GetGroupElements("Deck, west; side").Count);
        Assert.AreEqual(2, Node(restored, "10").Loads.Count);
        Assert.AreEqual(report.Preserved.Count, restored.PreservedSourceData.Count);
    }

    [DataTestMethod]
    [DataRow("*UNIT\nKN,M,KCAL,C", "*UNIT\nBANANA,M")]
    [DataRow("*UNIT\nKN,M,KCAL,C\n", "")]
    [DataRow("20,2,0,0", "20,NaN,0,0")]
    [DataRow("20,2,0,0", "20,1e308,0,0")]
    [DataRow("20,2,0,0", "20,2,0,0\n20,3,0,0")]
    [DataRow("101,BEAM", "101,TRUSS")]
    [DataRow("103,PLATE", "101,PLATE")]
    [DataRow("101,BEAM,1,2,10,20", "101,BEAM,1,2,10,99")]
    [DataRow("103,PLATE,1,3,10,30,40,0", "103,PLATE,1,3,10,30,99,0")]
    [DataRow("10 to 40 by 10", "10to40by0")]
    [DataRow("10 to 40 by 10", "40to10")]
    [DataRow("10 to 40 by 10", "10xyz")]
    [DataRow("10 to 40 by 10", "1to2147483647")]
    [DataRow("10 to 40 by 10", "999")]
    [DataRow("101 to 103", "101to104")]
    [DataRow("10,111000", "10,11000")]
    [DataRow("40,001000,", "99,001000,")]
    [DataRow("10 20,1,2,-3", "99,1,2,-3")]
    [DataRow("Empty,,,0", "Empty,,,0\nEmpty,,,0")]
    [DataRow("Q,L,synthetic live load", "G,L,synthetic live load")]
    [DataRow("*USE-STLD,G", "*USE-STLD,Missing")]
    [DataRow("*USE-STLD,G\n", "")]
    [DataRow("10 20,1,2,-3,4,-5,6", "10 20,1,2,-3,4,-5,")]
    [DataRow("*ENDDATA", "")]
    [DataRow("*ENDDATA", "*ENDDATA\n*NODE\n99,0,0,0")]
    public void InvalidInput_RejectsAtomicallyAndRetainsExactOriginalBytes(string oldValue, string newValue)
    {
        var data = Encoding.UTF8.GetBytes(Mixed.Replace(oldValue, newValue)); var report = Import(data);
        Assert.AreEqual(ImportStatus.Rejected, report.Status); Assert.IsNull(report.Model);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error));
        CollectionAssert.AreEqual(data, Convert.FromBase64String(report.Preserved.Single(p => p.Kind == "MIDAS Civil file").RawData));
        Assert.IsNotNull(report.SourceHash);
    }

    [TestMethod]
    public void UnknownCommand_IsRetainedWithDiagnosticInsteadOfSilentlyIgnored()
    {
        var report = Import(Mixed.Replace("*ENDDATA", "*FUTURE-COMMAND,abc\nopaque,1,2,3\n*ENDDATA")); Partial(report);
        Assert.IsTrue(report.Preserved.Any(p => p.Kind == "MIDAS Civil *FUTURE-COMMAND" && p.RawData.Contains("opaque")));
        Assert.IsTrue(report.Diagnostics.Any(d => d.Record.StartsWith("*FUTURE-COMMAND") && d.Severity == DiagnosticSeverity.Warning));
    }

    [DataTestMethod]
    [DataRow("1,0,0,0\n", "1,0,0,0,extra\n")]
    [DataRow("1,0,0,0\n", "1,0,0,\\\n")]
    [DataRow("*ENDDATA\n", "*GROUP\n\"unclosed,,,\n*ENDDATA\n")]
    [DataRow("*ENDDATA\n", "*GROUP\n\"closed\"suffix,,,\n*ENDDATA\n")]
    public void LexicalErrors_IncludeStructuredSourceRecord(string oldValue, string newValue)
    {
        var report = Import(BeamText("1000,0,0").Replace(oldValue, newValue));
        Assert.AreEqual(ImportStatus.Rejected, report.Status);
        Assert.IsFalse(string.IsNullOrWhiteSpace(report.Diagnostics.Single(d => d.Code == "SourceReadFailed").Record));
    }

    [DataTestMethod]
    [DataRow("KGF", "CM", 9.80665, 10d)]
    [DataRow("TONF", "M", 9806.65, 1000d)]
    [DataRow("LBF", "IN", 4.4482216152605, 25.4)]
    [DataRow("KIPS", "FT", 4448.2216152605, 304.8)]
    public void GravitationalAndImperialUnits_MapForceLengthAndMoment(string force, string length, double f, double l)
    {
        var text = BeamText("1000,0,0").Replace("N,MM", force + "," + length).Replace("*ENDDATA",
            "*STLDCASE\nG,D,\n*USE-STLD,G\n*CONLOAD\n1,1,0,0,1,0,0\n*ENDDATA");
        var report = Import(text); Partial(report);
        Assert.AreEqual(1000 * l, report.Model.BeamElements.Values.Single().Length, 1e-6);
        var load = (PointLoad)Node(report.Model, "1").Loads.Values.Single();
        Assert.AreEqual(f, load.F1, 1e-10); Assert.AreEqual(f * l, load.M1, 1e-7);
    }

    [TestMethod]
    public void Encoding_IsStrictAndLegacyEncodingMustBeExplicit()
    {
        var latin1 = Encoding.Latin1.GetBytes(Mixed.Replace("Empty", "Caffè"));
        var rejected = Import(latin1); Assert.AreEqual(ImportStatus.Rejected, rejected.Status);
        CollectionAssert.AreEqual(latin1, Convert.FromBase64String(rejected.Preserved.Single().RawData));
        using var stream = new MemoryStream(latin1);
        var accepted = new MidasCivilTextReader("synthetic-revision", textEncoding: Encoding.Latin1).Import(stream); Partial(accepted);
        Assert.IsTrue(accepted.Model.Groups.ContainsKey("Caffè"));
        var bom = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(Mixed)).ToArray(); Partial(Import(bom));
    }

    [TestMethod]
    public void CancellationProducesNoCandidate_AndVersionIsNotAWhitelist()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Mixed)); using var cancellation = new CancellationTokenSource(); cancellation.Cancel();
        var cancelled = new MidasCivilTextReader("r").Import(stream, cancellation.Token);
        Assert.AreEqual(ImportStatus.Cancelled, cancelled.Status); Assert.IsNull(cancelled.Model);
        var report = Import(Mixed.Replace("fixture-release-A", "fixture-future-release")); Partial(report);
        Assert.AreEqual("fixture-future-release", report.Model.AnalysisSource.SolverVersion);
    }
}
