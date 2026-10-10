using GPC.Model.Results.Queries;
using GPC.Model.Models;
using GPC.Examples;
using GPC.Model.Persistence;
using GPC.Model.Results.Locations;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;
using GPC.Model.Checking.Reports;
using GPC.Model.Compatibility;
using GPC.Model.Core.Identity;

if (args.Length > 0 && args[0] == "--model-only")
{
    Environment.ExitCode = ModelCompletionExample.Run();
    return;
}

if (args.Length > 0 && args[0] == "--straus-api-file")
{
    Environment.ExitCode = Straus7ImportExample.Run(args);
    return;
}
if (args.Length > 0 && args[0] == "--straus-api-results")
{
    Environment.ExitCode = Straus7ImportExample.RunResults(args);
    return;
}
if (args.Length > 0 && args[0] == "--civil-file")
{
    Environment.ExitCode = CivilTextImportExample.Run(args);
    return;
}

var model=MixedModelFactory.Create();
Console.WriteLine(model.Name+" — no FEM analysis is performed.");
Console.WriteLine($"Nodes: {model.NodesElements.Count}, beams: {model.BeamElements.Count}, shells: {model.AreaElements.Count}");
Console.WriteLine($"Elements incident to node 10: {model.GetAdjacency()[10].Count}; topology diagnostics: {model.ValidateTopology().Count}");
Console.WriteLine($"Source B1 resolves to ID {model.FindBySource(new SourceIdentity("Synthetic","mixed-v1",EntityFamily.Beam,"B1")).Id}");
var sample=Verification.BeamSample(model.BeamElements[250],"synthetic-static","P+",.5,SectionSide.Unspecified);
var prepared=Verification.PrepareBeam(model,250,sample,"Synthetic demonstration; NTC2018 defaults; constant eccentricity; tensile concrete excluded");
// The engines and ModelChecker are downstream of Model (Checker repository): without a verifier the check is NotEvaluated.
IConcreteSectionVerifier? verifier=null;
var result=Verification.Run(prepared,CheckMechanism.UlsBiaxialSection,verifier);
model.CheckReports.Add(CheckReport.ForSingleResult(result));
Console.WriteLine($"Midspan: V2={sample.ResultBeamForces.V2} N; M1={sample.ResultBeamForces.M1} Nmm; preparation={prepared.Status}");
Console.WriteLine($"Section check: {result.Execution}/{result.Data}/{result.Outcome}; ratio={result.Utilization}; engine={result.EngineVersion}");
foreach(var d in result.Diagnostics) Console.WriteLine(d.Code+": "+d.Message);
// The ModelChecker demonstration on this model (ConcreteSectionVerifier, ModelCheckRequest) is ModelCheckerWorkflowTest in Checker.
var shell=(PointResultPlateForces)model.AreaElements.Values.First().Results[0].Results[0];
Console.WriteLine($"Shell Fxx={shell.Forces.Fxx} N/mm, Mxx={shell.Forces.Mxx} Nmm/mm; structural design method not selected.");
var shellPreparation=ShellActionPreparation.Prepare(model,model.AreaElements.Values.First().Id,shell,null);
Console.WriteLine($"Shell preparation: {shellPreparation.Status}; {string.Join(", ",shellPreparation.Diagnostics.Select(d=>d.Code))}");
using var stream=new MemoryStream();ModelArchive.Save(model,stream);stream.Position=0;var restored=ModelArchive.Load(stream);
Console.WriteLine($"Round-trip: {stream.Length} bytes; shared node restored={ReferenceEquals(restored.NodesElements[10],restored.BeamElements[250].NodeI)}");
Console.WriteLine($"Analysis revision unchanged={model.AnalysisFingerprint()==restored.AnalysisFingerprint()}");
if(args.Length>0 && args[0]!="--without-checker") { using var file=File.Create(args[0]);ModelArchive.Save(model,file);Console.WriteLine("Saved: "+Path.GetFullPath(args[0])); }
