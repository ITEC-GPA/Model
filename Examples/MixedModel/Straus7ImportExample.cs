using GPC.Model.Models;
using GPC.Converter.Straus7;
using GPC.Model.Persistence;
using GPC.Model.Analysis;
using GPC.Model.Checking.Preparation;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Results.Queries;

namespace GPC.Examples;

internal static class Straus7ImportExample
{
    public static int Run(string[] args)
    {
        if (args.Length != 3)
        { Console.Error.WriteLine("Usage: --straus-api-file <ST7 path> <model revision>"); return 2; }
        var report = new Straus7ApiConverter().Import(new Straus7ImportRequest { ModelPath = args[1], ModelRevision = args[2] });
        Console.WriteLine($"Straus7 API import: {report.Status}; SHA256={report.SourceHash}");
        foreach (var d in report.Diagnostics) Console.WriteLine($"{d.Severity} {d.Code} [{d.Record}]: {d.Message}");
        if (report.Model == null) return 1;
        var model = report.Model;
        Console.WriteLine($"API={model.AnalysisSource.SolverVersion}; nodes={model.NodesElements.Count}; beams={model.BeamElements.Count}; plates={model.AreaElements.Count}; groups={model.Groups.Count}; cases={model.LoadCases.Count}");
        foreach (var group in model.Groups.Values) Console.WriteLine($"Group {group.Name}; parent={group.Parent?.Name}; members={model.GetGroupElements(group.Name).Count}");
        foreach (var loadCase in model.LoadCases.Values) Console.WriteLine($"Load case: {loadCase.Name}");
        using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0; var copy = ModelArchive.Load(stream);
        Console.WriteLine($"Archive round-trip: same analysis fingerprint={model.AnalysisFingerprint() == copy.AnalysisFingerprint()}");
        Console.WriteLine($"Verification enabled={report.VerificationEnabled}; complete assignments, reinforcement and matching analysis results before checking.");
        return 0;
    }

    public static int RunResults(string[] args)
    {
        if ((args.Length != 7 && args.Length != 8) || !int.TryParse(args[5], out int caseNumber) || caseNumber < 1
            || args.Length == 8 && args[7] != "--element-node-forces")
        {
            Console.Error.WriteLine("Usage: --straus-api-results <ST7 path> <result path> <model revision> <analysis ID> <native result case> <Model case name> [--element-node-forces]");
            return 2;
        }
        var converter = new Straus7ApiConverter();
        var geometry = converter.Import(new Straus7ImportRequest { ModelPath = args[1], ModelRevision = args[3], AnalysisId = args[4] });
        foreach (var d in geometry.Diagnostics) Console.WriteLine($"{d.Severity} {d.Code}: {d.Message}");
        var model = geometry.Model; if (model == null) return 1;
        var native = converter.ReadResults(new Straus7ResultsRequest
        {
            ModelPath = args[1], ResultPath = args[2], CaseNumbers = new[] { caseNumber },
            NodeNumbers = model.NodesElements.Values.Select(e => int.Parse(e.Source.OriginalId)).ToArray(),
            BeamNumbers = model.BeamElements.Values.Select(e => int.Parse(e.Source.OriginalId)).ToArray(),
            PlateNumbers = model.AreaElements.Values.Select(e => int.Parse(e.Source.OriginalId)).ToArray(),
            IncludeElementNodeForces = args.Length == 8
        });
        foreach (var d in native.Diagnostics) Console.WriteLine($"{d.Severity} {d.Code}: {d.Message}");
        var imported = Straus7LinearStaticResults.Import(model, native, "straus-cli", new Dictionary<int, string> { [caseNumber] = args[6] });
        foreach (var d in imported.Diagnostics) Console.WriteLine($"{d.Severity} {d.Code}: {d.Message}");
        if (imported.Status != GPC.Converter.ImportStatus.Completed) return 1;
        var prepared = ResultPreparation.Prepare(model, new ElementSelection { Families = new[] { EntityFamily.Node, EntityFamily.Beam, EntityFamily.Shell } },
            new[] { new ResultSelection { Dataset = "straus-cli", Case = args[6] } });
        Console.WriteLine($"Imported samples={imported.ImportedSamples}; prepared in local axes={prepared.Count(p => p.Status == DataStatus.Ready)}; missing/unsupported={prepared.Count(p => p.Status != DataStatus.Ready)}");
        foreach (var d in prepared.SelectMany(p => p.Diagnostics).Take(20)) Console.WriteLine($"{d.Code}: {d.Message}");
        using var stream = new MemoryStream(); ModelArchive.Save(model, stream); stream.Position = 0; var copy = ModelArchive.Load(stream);
        Console.WriteLine($"Archive samples={copy.AllElements.Sum(e => e.Results.Sum(r => r.Results.Count))}; same input fingerprint={model.AnalysisFingerprint() == copy.AnalysisFingerprint()}");
        Console.WriteLine("Actions prepared. Structural checking additionally requires verified properties, reinforcement, offsets and supported design mechanisms.");
        return prepared.All(p => p.Status == DataStatus.Ready) ? 0 : 1;
    }
}
