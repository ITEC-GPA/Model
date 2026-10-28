using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

/// <summary>Plate results at the element nodes (St7GetPlateResultArray, spNodesAverageNever) through the transport double: one sample per corner,
/// at the node, with the solver's own values of that plate (no averaging between plates). Values are invented, not solver output.</summary>
public partial class Straus7ApiWorkflowTest
{
    private sealed class PlateNodeApi : IStraus7ReadApi, IStraus7PlateNodeResultReadApi
    {
        private readonly SyntheticApi inner = new();
        public string NodeFault = "";
        public Straus7ResultTable ReadPlateNodeResult(int number, int resultCase, bool moments)
        {
            if (moments && NodeFault == "no-moments") throw new Straus7ApiException("St7GetPlateResultArray", 118, "Result quantity not stored");
            var nodes = new[] { 1, 2, 3, 4 }; if (NodeFault == "order") Array.Reverse(nodes);
            // Node k: Fxx 10k, Fyy 20k, Fxy 30k, Fyz 40k, Fxz 50k; Mxx k, Myy 2k, Mxy 3k (native order xx, yy, zz, xy, yz, xz).
            var values = Enumerable.Range(1, 4).SelectMany(k => moments ? new[] { k, 2d * k, 0, 3d * k, 0, 0 } : new[] { 10d * k, 20d * k, 0, 30d * k, 40d * k, 50d * k }).ToArray();
            if (NodeFault == "out-of-plane" && !moments) values[2] = 1;
            return new Straus7ResultTable { Entity = Straus7Entity.Plate, Number = number, CaseNumber = resultCase, Quantity = moments ? "PlateMomentLocalNodes" : "PlateForceLocalNodes",
                Rows = 4, Columns = 6, Values = values, ElementState = new[] { 1, 1, 1 }, NodeNumbers = nodes };
        }
        public string Version => inner.Version;
        public void OpenModelReadOnly(string path, string scratchPath) => inner.OpenModelReadOnly(path, scratchPath);
        public void CloseModel() => inner.CloseModel();
        public void Dispose() => inner.Dispose();
        public int[] ReadUnits() => inner.ReadUnits();
        public int Count(Straus7Entity entity) => inner.Count(entity);
        public Straus7Node ReadNode(int number) => inner.ReadNode(number);
        public Straus7Element ReadElement(Straus7Entity entity, int number) => inner.ReadElement(entity, number);
        public Straus7Property ReadProperty(Straus7Entity entity, int number) => inner.ReadProperty(entity, number);
        public int GroupCount => inner.GroupCount;
        public Straus7Group ReadGroup(int index) => inner.ReadGroup(index);
        public int LoadCaseCount => inner.LoadCaseCount;
        public string ReadLoadCase(int number) => inner.ReadLoadCase(number);
        public int StageCount => inner.StageCount;
        public Straus7ResultFile ValidateResults(string path) => inner.ValidateResults(path);
        public Straus7ResultFile OpenResults(string path) => inner.OpenResults(path);
        public void CloseResults() => inner.CloseResults();
        public Straus7ResultCase ReadResultCase(int number) => inner.ReadResultCase(number);
        public Straus7ResultTable ReadBeamForces(int number, int resultCase, int minimumStations) => inner.ReadBeamForces(number, resultCase, minimumStations);
        public Straus7ResultTable ReadPlateResult(int number, int resultCase, bool moments) => inner.ReadPlateResult(number, resultCase, moments);
        public Straus7ResultTable ReadNodeResult(int number, int resultCase, bool reactions) => inner.ReadNodeResult(number, resultCase, reactions);
    }

    [TestMethod]
    public void PlateNodeResults_AreOneSamplePerCornerAtTheNode_AndPersist()
    {
        var converter = new Straus7ApiConverter(() => new PlateNodeApi()); var imported = converter.Import(Request()); Partial(imported); var model = imported.Model;
        var request = ResultsRequest(); request.IncludePlateNodeResults = true;
        var native = converter.ReadResults(request);
        Assert.AreEqual(ImportStatus.Completed, native.Status, string.Join(";", native.Diagnostics.Select(d => d.Message))); Assert.AreEqual(7, native.Tables.Count);
        var result = Straus7LinearStaticResults.Import(model, native, "corners", CaseMap, isSynthetic: true);
        Assert.AreEqual(ImportStatus.Completed, result.Status, string.Join(";", result.Diagnostics.Select(d => d.Message))); Assert.AreEqual(10, result.ImportedSamples);
        var plate = model.AreaElements.Values.Single();
        var corners = plate.Results.SelectMany(r => r.Results).OfType<PointResultPlateForces>().Where(s => s.PointKind == ShellResultPointKind.ElementNodeExtrapolated).ToArray();
        Assert.AreEqual(4, corners.Length);
        foreach (var corner in corners)
        {
            var node = plate.Nodes.Single(n => n.Id == corner.SourceNodeId); int k = int.Parse(node.Source.OriginalId);
            Assert.AreEqual(10 * k, corner.Forces.Fxx); Assert.AreEqual(30 * k, corner.Forces.Fxy); Assert.AreEqual(50 * k, corner.Forces.Fxz); Assert.AreEqual(40 * k, corner.Forces.Fyz);
            Assert.AreEqual(1000 * k, corner.Forces.Mxx); Assert.AreEqual(3000 * k, corner.Forces.Mxy);
            Assert.AreEqual(node.Position.X - 1000, corner.Location.X, 1e-9); Assert.AreEqual(node.Position.Y - 500, corner.Location.Y, 1e-9);
            Assert.AreEqual(ResultCoordinateKind.LocalPhysical, corner.CoordinateKind);
        }
        Assert.AreEqual(1, plate.Results.SelectMany(r => r.Results).OfType<PointResultPlateForces>().Count(s => s.PointKind == ShellResultPointKind.Centroid));
        using var archive = new MemoryStream(); ModelArchive.Save(model, archive); archive.Position = 0;
        var copy = ModelArchive.Load(archive).AreaElements.Values.Single().Results.SelectMany(r => r.Results).OfType<PointResultPlateForces>();
        Assert.AreEqual(4, copy.Count(s => s.PointKind == ShellResultPointKind.ElementNodeExtrapolated && s.SourceNodeId.HasValue));
    }

    [DataTestMethod]
    [DataRow("order")][DataRow("out-of-plane")]
    public void PlateNodeResults_InWrongOrderOrWithOutOfPlaneTermsAreRejectedAtomically(string fault)
    {
        var converter = new Straus7ApiConverter(() => new PlateNodeApi { NodeFault = fault }); var model = converter.Import(Request()).Model;
        var request = ResultsRequest(); request.IncludePlateNodeResults = true;
        var native = converter.ReadResults(request); Assert.AreEqual(ImportStatus.Completed, native.Status);
        var result = Straus7LinearStaticResults.Import(model, native, "corners", CaseMap, isSynthetic: true);
        Assert.AreEqual(ImportStatus.Rejected, result.Status); Assert.AreEqual(0, model.Datasets.Count);
    }

    [TestMethod]
    public void PlateNodeResults_MissingOrUnsupportedAreReportsNotInventedValues()
    {
        var request = ResultsRequest(); request.IncludePlateNodeResults = true;
        var missing = new Straus7ApiConverter(() => new PlateNodeApi { NodeFault = "no-moments" }).ReadResults(request);
        Assert.AreEqual(ImportStatus.Rejected, missing.Status); Assert.AreEqual(0, missing.Tables.Count);
        var unsupported = new Straus7ApiConverter(() => new SyntheticApi()).ReadResults(request);
        Assert.AreEqual(ImportStatus.Rejected, unsupported.Status, "A binding without plate node results rejects the explicit request.");
    }
}
