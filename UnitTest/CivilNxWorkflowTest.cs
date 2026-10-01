using System.Net;
using System.Text.Json;
using GPC.Converter;
using GPC.Converter.CivilNx;
using GPC.Model.PostProcessing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

[TestClass]
public class CivilNxWorkflowTest
{
    private static Dictionary<string, CivilNxResponse> Responses() => new()
    {
        ["UNIT"] = new("db/UNIT", "{\"UNIT\":{\"1\":{\"DIST\":\"M\",\"FORCE\":\"KN\"}}}"),
        ["NODE"] = new("db/NODE", "{\"NODE\":{\"10\":{\"X\":0,\"Y\":0,\"Z\":0},\"20\":{\"X\":0,\"Y\":0,\"Z\":2},\"30\":{\"X\":1,\"Y\":0,\"Z\":2}}}"),
        ["ELEM"] = new("db/ELEM", "{\"ELEM\":{\"101\":{\"TYPE\":\"BEAM\",\"NODE\":[10,20],\"MATL\":1,\"SECT\":2,\"ANGLE\":30,\"FUTURE_FIELD\":\"retained\"},\"102\":{\"TYPE\":\"PLATE\",\"NODE\":[10,20,30],\"MATL\":1,\"SECT\":3,\"ANGLE\":0,\"STYPE\":1}}}")
    };
    private static AnalysisSource Identity(string? version) => new() { Program = "MIDAS Civil NX", SolverVersion = version, ModelRevision = "fixture", AnalysisId = "fixture-analysis" };

    [TestMethod]
    public void CommonSchema_UsesFieldsNotVersionWhitelist_AndPreservesUnknownData()
    {
        foreach (var version in new[] { "fixture-release-A", "fixture-release-B", null })
        {
            var report = CivilNxGeometryReader.Import(new CivilNxSnapshot(Responses()), Identity(version));
            Assert.AreEqual(ImportStatus.Partial, report.Status, string.Join(";", report.Diagnostics.Select(d => d.Message)));
            Assert.AreEqual(3, report.Model.NodesElements.Count); Assert.AreEqual(2000, report.Model.BeamElements.Values.Single().Length);
            Assert.AreSame(report.Model.BeamElements.Values.Single().NodeI, report.Model.AreaElements.Values.Single().Nodes[0]);
            Assert.IsNull(report.Model.BeamElements.Values.Single().Assignments.SectionAxes);
            Assert.IsTrue(report.Model.PreservedSourceData.Any(p => p.RawData.Contains("FUTURE_FIELD")));
            Assert.IsFalse(report.VerificationEnabled);
        }
    }

    [TestMethod]
    public void UnknownGeometrySchemaOrFormulation_IsRejectedWithOriginalResponses()
    {
        var responses = Responses(); responses["NODE"] = new CivilNxResponse("db/NODE", "{\"NODE\":{\"10\":{\"X\":0,\"Y\":0}}}");
        var missing = CivilNxGeometryReader.Import(new CivilNxSnapshot(responses), Identity(null));
        Assert.AreEqual(ImportStatus.Rejected, missing.Status); Assert.IsNull(missing.Model); Assert.AreEqual(3, missing.Preserved.Count);
        responses = Responses(); responses["ELEM"] = new CivilNxResponse("db/ELEM", "{\"ELEM\":{\"1\":{\"TYPE\":\"TENSTR\",\"NODE\":[10,20]}}}");
        Assert.AreEqual(ImportStatus.Rejected, CivilNxGeometryReader.Import(new CivilNxSnapshot(responses), Identity(null)).Status);
    }

    [TestMethod]
    public void TableRequest_IsReadOnlyExplicitAndProperlyEscaped()
    {
        var request = new CivilNxTableRequest { Table = CivilNxResultTable.PlateForcePerUnitLength, Name = "Result \"A\"", ElementIds = new[] { 1, 2 }, LoadCases = new[] { "DL(ST)" } };
        using var json = JsonDocument.Parse(request.ToJson()); var argument = json.RootElement.GetProperty("Argument");
        Assert.AreEqual("PLATEFORCEUL", argument.GetProperty("TABLE_TYPE").GetString());
        Assert.AreEqual("Result \"A\"", argument.GetProperty("TABLE_NAME").GetString()); Assert.IsFalse(argument.GetProperty("AVERAGE_NODAL_RESULT").GetBoolean());
        Assert.IsFalse(argument.TryGetProperty("EXPORT_PATH", out _)); Assert.AreEqual(2, argument.GetProperty("NODE_ELEMS").GetProperty("KEYS").GetArrayLength());
    }

    [TestMethod]
    public void ResultTable_ReadsByColumnName_NotPosition_AndRetainsAdditionalColumns()
    {
        var response = new CivilNxResponse("post/table", "{\"table\":{\"FORCE\":\"N\",\"DIST\":\"mm\",\"HEAD\":[\"NewColumn\",\"Axial\",\"Elem\",\"Angle\",\"Angle\"],\"DATA\":[[\"extra\",\"-1e3\",\"101\",\"0\",\"90\"]]}}");
        var table = CivilNxResultTableData.Read(response, "table");
        Assert.AreEqual("-1e3", table.Get(0, "Axial")); Assert.AreEqual("extra", table.Get(0, "NewColumn"));
        Assert.ThrowsException<InvalidDataException>(() => table.Get(0, "Moment-y")); Assert.ThrowsException<InvalidDataException>(() => table.Get(0, "Angle"));
    }

    private sealed class Handler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, Task<HttpResponseMessage>> Respond { get; set; } = null!;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken token)
        { token.ThrowIfCancellationRequested(); return Respond(request); }
    }
    [TestMethod]
    public async Task ApiTransport_PreservesBasePath_UsesRuntimeKeyAndDetectsModelChanges()
    {
        var responses = Responses(); int calls = 0;
        var handler = new Handler { Respond = request =>
        {
            calls++; Assert.AreEqual(HttpMethod.Get, request.Method); Assert.AreEqual("test-only-key", request.Headers.GetValues("MAPI-Key").Single());
            StringAssert.StartsWith(request.RequestUri!.AbsolutePath, "/civil/db/");
            var name = request.RequestUri.Segments.Last();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(responses[name].Json + (calls == 6 ? " " : "")) });
        } };
        using var client = new CivilNxApiClient(new Uri("https://example.invalid/civil"), () => "test-only-key", handler);
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => client.ReadGeometrySnapshotAsync());
        Assert.AreEqual(6, calls);
        Assert.ThrowsException<ArgumentException>(() => client.ReadDatabaseAsync("../doc/ANAL"));
    }

    [TestMethod]
    public async Task ApiTransport_FailsOnHttpErrorsAndCancellation_WithoutEchoingBody()
    {
        var handler = new Handler { Respond = _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized) { Content = new StringContent("secret error body") }) };
        using var client = new CivilNxApiClient(new Uri("https://example.invalid/civil"), () => "test-only-key", handler);
        var error = await Assert.ThrowsExceptionAsync<HttpRequestException>(() => client.ReadDatabaseAsync("NODE"));
        Assert.AreEqual("CivilNxHttpStatus:401", error.Message);
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(() => client.ReadDatabaseAsync("NODE", new CancellationToken(true)));
    }
}
