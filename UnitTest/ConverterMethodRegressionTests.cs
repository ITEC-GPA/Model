using System.Text.Json;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Examples;

namespace UnitTest;

[TestClass]
public class ConverterMethodRegressionTests
{
    [TestMethod]
    public void TableToJson_DeduplicatesElementIdsAndKeepsBeamPartsAndStage()
    {
        var request = new CivilNxTableRequest { Table = CivilNxResultTable.BeamForce, ElementIds = new[] { 9, 9, 3 }, LoadCases = new[] { "DL(ST)" }, BeamParts = new[] { "I", "J" }, StageSteps = new[] { "Stage 1:001" } };
        using var json = JsonDocument.Parse(request.ToJson()); var a = json.RootElement.GetProperty("Argument");
        CollectionAssert.AreEqual(new[] { 9, 3 }, a.GetProperty("NODE_ELEMS").GetProperty("KEYS").EnumerateArray().Select(x => x.GetInt32()).ToArray());
        CollectionAssert.AreEqual(new[] { "I", "J" }, a.GetProperty("PARTS").EnumerateArray().Select(x => x.GetString()).ToArray());
        Assert.IsTrue(a.GetProperty("OPT_CS").GetBoolean()); Assert.AreEqual("Stage 1:001", a.GetProperty("STAGE_STEP")[0].GetString());
        Assert.AreEqual("N", a.GetProperty("UNIT").GetProperty("FORCE").GetString()); Assert.AreEqual("mm", a.GetProperty("UNIT").GetProperty("DIST").GetString());
    }
    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public void TableToJson_PlateCenterAndNodesAreMutuallyExclusive(bool nodes)
    {
        using var json = JsonDocument.Parse(new CivilNxTableRequest { Table = CivilNxResultTable.PlateForcePerUnitLength, ElementIds = new[] { 1 }, LoadCases = new[] { "DL(ST)" }, PlateNodes = nodes }.ToJson());
        var a = json.RootElement.GetProperty("Argument");
        Assert.AreEqual(nodes, a.GetProperty("NODE_FLAG").GetProperty("NODES").GetBoolean());
        Assert.AreEqual(!nodes, a.GetProperty("NODE_FLAG").GetProperty("CENTER").GetBoolean());
        Assert.IsFalse(a.GetProperty("AVERAGE_NODAL_RESULT").GetBoolean());
    }
    [DataTestMethod]
    [DataRow(CivilNxResultTable.SectionProperties)] [DataRow(CivilNxResultTable.MaterialProperties)]
    public void TableToJson_PropertyTablesNeedNoElementOrLoadCaseSelection(CivilNxResultTable table)
    {
        using var json = JsonDocument.Parse(new CivilNxTableRequest { Table = table }.ToJson());
        var a = json.RootElement.GetProperty("Argument");
        Assert.IsFalse(a.TryGetProperty("NODE_ELEMS", out _)); Assert.IsFalse(a.TryGetProperty("LOAD_CASE_NAMES", out _));
        Assert.AreEqual(table == CivilNxResultTable.SectionProperties ? "SECTIONALL" : "MATERIAL", a.GetProperty("TABLE_TYPE").GetString());
    }
    [TestMethod]
    public void FilterResolve_DeduplicatesCasesWithoutChangingRequestedOrder()
    {
        var plan = new ResultFilter { StaticCases = new[] { "P-", "P+", "P-" } }.Resolve(MixedModelFactory.Create());
        CollectionAssert.AreEqual(new[] { "P-", "P+" }, plan.StaticCases.ToArray());
    }
    [TestMethod]
    public void FilterAll_SelectsBeamsShellsNodesAndOnlyRestrainedSupports()
    {
        var plan = ResultFilter.All(MixedModelFactory.Create());
        CollectionAssert.AreEqual(new[] { 250 }, plan.Beams.Select(x => x.Id).ToArray());
        CollectionAssert.AreEqual(new[] { 1090, 1130 }, plan.Shells.Select(x => x.Id).ToArray());
        CollectionAssert.AreEqual(new[] { 10, 40, 90, 130 }, plan.Nodes.Select(x => x.Id).ToArray());
        CollectionAssert.AreEqual(new[] { 10 }, plan.Supports.Select(x => x.Id).ToArray());
    }
}
