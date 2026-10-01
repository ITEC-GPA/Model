using GPC.Examples;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;

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
IConcreteSectionVerifier? verifier=null;
#if GPC_CHECKER
if(!args.Contains("--without-checker")) verifier=new GPC.Model.Checker.ConcreteSectionVerifier(new GPC.Model.Standards.StandardNTC2018Concrete(),
    GPC.Checkers.Concrete.SectionSolvers.SectionSolver.FailureAnalysisTypes.ConstantEccentricity,false,64,0,0);
#endif
var result=Verification.Run(prepared,CheckMechanism.UlsBiaxialSection,verifier);
model.CheckReports.Add(CheckReport.ForSingleResult(result));
Console.WriteLine($"Midspan: V2={sample.ResultBeamForces.V2} N; M1={sample.ResultBeamForces.M1} Nmm; preparation={prepared.Status}");
Console.WriteLine($"Section check: {result.Execution}/{result.Data}/{result.Outcome}; ratio={result.Utilization}; engine={result.EngineVersion}");
foreach(var d in result.Diagnostics) Console.WriteLine(d.Code+": "+d.Message);
#if GPC_CHECKER
if (verifier != null)
{
    var request = new GPC.Model.Checker.ModelCheckRequest();
    request.Jobs.Add(new GPC.Model.Checker.ModelCheckJob
    {
        Name = "All beam and plate samples",
        Options = new GPC.Model.Checker.ConcreteVerificationOptions { Standard = new GPC.Model.Standards.StandardNTC2018Concrete(),
            Criterion = GPC.Checkers.Concrete.SectionSolvers.SectionSolver.FailureAnalysisTypes.ConstantEccentricity },
        Preparation = new PreparationRequest { Results = new[] { new ResultSelection { Dataset = "synthetic-static", Case = "P+" } },
            Settings = "Synthetic mixed model demonstration" }
    });
    var batch = new GPC.Model.Checker.ModelChecker().Verify(model, request);
    model.CheckReports.AddRange(batch.Jobs);
    Console.WriteLine($"ModelChecker: required={batch.Required}; executed={batch.Executed}; native checkers={batch.CreatedCheckers}; outcome={batch.Outcome}");
    foreach (var item in batch.Jobs.SelectMany(j => j.Results).Where(r => r.Outcome == EngineeringOutcome.NotEvaluated))
        Console.WriteLine($"  {item.Family} {item.ElementId}: {item.Data}; {string.Join(", ", item.Diagnostics.Select(d => d.Code))}");
}
#endif
var shell=(PointResultPlateForces)model.AreaElements.Values.First().Results[0].Results[0];
Console.WriteLine($"Shell Fxx={shell.Forces.Fxx} N/mm, Mxx={shell.Forces.Mxx} Nmm/mm; structural design method not selected.");
var shellPreparation=ShellActionPreparation.Prepare(model,model.AreaElements.Values.First().Id,shell,null);
Console.WriteLine($"Shell preparation: {shellPreparation.Status}; {string.Join(", ",shellPreparation.Diagnostics.Select(d=>d.Code))}");
using var stream=new MemoryStream();ModelArchive.Save(model,stream);stream.Position=0;var restored=ModelArchive.Load(stream);
Console.WriteLine($"Round-trip: {stream.Length} bytes; shared node restored={ReferenceEquals(restored.NodesElements[10],restored.BeamElements[250].NodeI)}");
Console.WriteLine($"Analysis revision unchanged={model.AnalysisFingerprint()==restored.AnalysisFingerprint()}");
if(args.Length>0 && args[0]!="--without-checker") { using var file=File.Create(args[0]);ModelArchive.Save(model,file);Console.WriteLine("Saved: "+Path.GetFullPath(args[0])); }
