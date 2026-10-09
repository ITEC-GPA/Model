using GPC.Model.Models;
using GPC.Converter;
using GPC.Converter.MidasCivil;
using GPC.Model.Persistence;

namespace GPC.Examples;

internal static class CivilTextImportExample
{
    public static int Run(string[] args)
    {
        if (args.Length != 3)
        {
            Console.Error.WriteLine("Usage: --civil-file <MCT/MGT path> <model revision>"); return 2;
        }
        using var source = File.OpenRead(args[1]);
        var report = new MidasCivilTextReader(args[2]).Import(source);
        Console.WriteLine($"Civil input import: {report.Status}; SHA256={report.SourceHash}");
        foreach (var d in report.Diagnostics) Console.WriteLine($"{d.Severity} {d.Code} [{d.Record}]: {d.Message}");
        if (report.Model == null) return 1;
        var model = report.Model;
        Console.WriteLine($"Nodes={model.NodesElements.Count}; beams={model.BeamElements.Count}; plates={model.AreaElements.Count}; groups={model.Groups.Count}; cases={model.LoadCases.Count}");
        foreach (var name in model.Groups.Keys) Console.WriteLine($"Group {name}: {model.GetGroupElements(name).Count} members");
        foreach (var beam in model.BeamElements.Values)
        {
            var axes = beam.Assignments.SectionAxes;
            Console.WriteLine($"Beam source {beam.Source.OriginalId}: L={beam.Length} mm; section axes available={axes != null}");
        }
        using var archive = new MemoryStream(); ModelArchive.Save(model, archive); archive.Position = 0;
        var restored = ModelArchive.Load(archive);
        Console.WriteLine($"Archive round-trip: same analysis fingerprint={model.AnalysisFingerprint() == restored.AnalysisFingerprint()}");
        Console.WriteLine($"Verification enabled={report.VerificationEnabled}; complete properties and separately matched analysis results first.");
        return 0;
    }
}
