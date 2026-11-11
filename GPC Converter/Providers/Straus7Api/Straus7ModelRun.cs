using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using GPC.Model.Analysis;
using GPC.Model.Core;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.ElementProperties;
using GPC.Model.Results.State;

namespace GPC.Converter.Straus7
{
    /// <summary>One completed linear-static export, bound to its input revision and exact native files.
    /// The run retains identities rather than an editable model and never changes existing source identities.</summary>
    public sealed class Straus7ModelRun
    {
        public string ModelPath { get; }
        public string ResultPath { get; }
        public string ModelHash { get; }
        public string ResultHash { get; }
        public SolverIdentityMap IdentityMap { get; }
        private readonly string apiVersion;
        private readonly Dictionary<Guid,string> nativeSections;
        private readonly Dictionary<string,string> sectionConfiguration;
        public string ConfigurationFingerprint { get; }
        private readonly IReadOnlyDictionary<Guid,int> loadCases;
        private readonly IReadOnlyDictionary<int,int> minimumStations;
        private readonly IReadOnlyDictionary<int,string> generatedCases;
        private readonly Guid[] generatedParents;
        private readonly Guid? stagedOutput;
        private readonly KeyValuePair<string,int>[] stageIncrements;
        public IReadOnlyDictionary<int,string> GeneratedCases => new System.Collections.ObjectModel.ReadOnlyDictionary<int,string>(generatedCases.ToDictionary(p=>p.Key,p=>p.Value));
        /// <summary>C2 (P26-04): the native load cases solved, in native order (the result cases 1, 2, … in this order); null: every load case.</summary>
        private readonly int[] solved;
        /// <summary>C2 (P26-07): the GPC combinations read from the Straus7 LSA combination cases, by native combination number.</summary>
        private readonly SortedDictionary<int,Guid> combinations;
        /// <summary>C4 (point 3): a nonlinear run without stages: every result case is in stage 0, its phase is the one of <see cref="stageIncrements"/>.</summary>
        private readonly bool unstaged;
        /// <summary>C2 (P26-04): the native load case numbers solved by this run, in native order (every load case when no selection was given).</summary>
        public IReadOnlyList<int> SolvedLoadCases => solved ?? Enumerable.Range(1,loadCases.Count+generatedCases.Count).ToArray();
        internal Straus7ModelRun(Straus7ModelWritePlan plan,string modelPath,string resultPath,string apiVersion,
            IReadOnlyDictionary<int,string> generated=null,Guid? stagedOutput=null,KeyValuePair<string,int>[] stageIncrements=null,
            int[] solved=null,IReadOnlyDictionary<int,Guid> combinations=null,bool unstaged=false)
        {
            this.unstaged=unstaged;
            this.solved=solved;this.combinations=new SortedDictionary<int,Guid>((combinations??new Dictionary<int,Guid>()).ToDictionary(p=>p.Key,p=>p.Value));
            ModelPath=modelPath;ResultPath=resultPath;this.apiVersion=apiVersion;
            ConfigurationFingerprint=plan.ConfigurationFingerprint;
            nativeSections=plan.NativeSections.ToDictionary(p=>p.Key,p=>Convert.ToBase64String(p.Value.Save()));
            sectionConfiguration=nativeSections.ToDictionary(p=>"Section/"+p.Key.ToString("D"),p=>p.Value);
            foreach(var pair in plan.BeamIntegrationSlices)sectionConfiguration.Add("IntegrationSlices/"+pair.Key.ToString("D"),pair.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            ModelHash=Hash(modelPath);ResultHash=Hash(resultPath);IdentityMap=plan.IdentityMap;
            loadCases=plan.LoadCaseIds;minimumStations=plan.MinimumOutputStations;
            generatedCases=(generated??new Dictionary<int,string>()).ToDictionary(p=>p.Key,p=>p.Value);generatedParents=(Guid[])plan.StaticMovingParentCases.Clone();
            this.stagedOutput=stagedOutput;this.stageIncrements=stageIncrements??Array.Empty<KeyValuePair<string,int>>();
        }
        /// <summary>Read the produced primary cases, normalize N/mm/rad values and atomically attach the dataset.
        /// Load patches have no stress-resultant samples. Native source arrays and the solver map are retained as provenance.</summary>
        public ResultImportReport ImportResults(global::GPC.Model.Models.Model model,string datasetId,bool includeElementNodeForces=false,
            bool includePlateNodeResults=false,CancellationToken cancellation=default)
            => ImportResults(model,datasetId,null,includeElementNodeForces,includePlateNodeResults,cancellation);

        /// <summary>Generated native cases can be attached as distinct Steps of an existing moving-load case.
        /// For multiple parent cases supply the complete native-case to parent-GUID map; no load definitions are added on import.</summary>
        public ResultImportReport ImportResults(global::GPC.Model.Models.Model model,string datasetId,IReadOnlyDictionary<int,Guid> generatedCaseParents,
            bool includeElementNodeForces=false,bool includePlateNodeResults=false,CancellationToken cancellation=default)
        {
            var rejected=new ResultImportReport {Status=ImportStatus.Rejected};
            try
            {
                cancellation.ThrowIfCancellationRequested();IdentityMap.ValidateAgainst(model);
                if(Hash(ModelPath)!=ModelHash||Hash(ResultPath)!=ResultHash)throw new InvalidDataException("StrausExportFilesChanged");
                // C2 (P26-04): the result cases are the solved load cases in native order (Straus7 numbers them consecutively); all of them
                // when the run had no selection, as before.
                var solvedCases=solved??Enumerable.Range(1,loadCases.Count+generatedCases.Count).ToArray();
                var solvedGenerated=solvedCases.Where(generatedCases.ContainsKey).ToArray();
                if(generatedCaseParents!=null&&(generatedCaseParents.Keys.Any(k=>!generatedCases.ContainsKey(k))||solvedGenerated.Any(k=>!generatedCaseParents.ContainsKey(k))||generatedCaseParents.Values.Any(g=>!generatedParents.Contains(g))))
                    throw new ArgumentException("InvalidGeneratedMovingCaseParentMap");
                if(solvedGenerated.Length>0&&generatedCaseParents==null&&generatedParents.Length!=1)throw new ArgumentException("GeneratedMovingCasesRequireExplicitParentMap");
                int primaryCount=stagedOutput.HasValue?stageIncrements.Sum(p=>p.Value):solvedCases.Length;
                bool combined=!stagedOutput.HasValue&&combinations.Count!=0;
                // C2 (P26-07): the combination cases follow the primary cases, in native combination order.
                var caseNumbers=Enumerable.Range(1,primaryCount).Concat(combined?combinations.Keys.Select(j=>primaryCount+j):Enumerable.Empty<int>()).ToArray();
                var patches=new HashSet<Guid>(model.AreaElements.Values.Where(p=>p.PlateProperty is ElasticPlateProperty e&&e.Formulation==ElasticPlateFormulation.LoadPatch).Select(p=>p.Guid));
                int[] Ids(EntityFamily family)=>IdentityMap.Entries.Where(e=>e.Family==family&&!patches.Contains(e.EntityGuid)).Select(e=>e.SolverId).ToArray();
                var native=new Straus7ApiConverter().ReadResults(new Straus7ResultsRequest {ModelPath=ModelPath,ResultPath=ResultPath,
                    CaseNumbers=caseNumbers,NodeNumbers=Ids(EntityFamily.Node),
                    BeamNumbers=Ids(EntityFamily.Beam),PlateNumbers=Ids(EntityFamily.Shell),BeamMinimumStations=minimumStations,
                    IncludeElementNodeForces=includeElementNodeForces,IncludePlateNodeResults=includePlateNodeResults,IncludeCombinations=combined},cancellation);
                if(native.Status!=ImportStatus.Completed||native.File==null||native.File.ValidationFlags!=0||native.File.Solver!=(stagedOutput.HasValue?3:1)
                    ||native.ModelHash!=ModelHash||native.ResultHash!=ResultHash||native.ApiVersion!=apiVersion||!stagedOutput.HasValue&&native.File.PrimaryCases!=primaryCount
                    ||native.Cases.Count!=caseNumbers.Length||native.Cases.Any(c=>stagedOutput.HasValue&&!unstaged?c.Stage<1||c.Stage>stageIncrements.Length:c.Stage!=0))throw new InvalidDataException("InvalidStrausExportResultFile");
                if(stagedOutput.HasValue&&!unstaged&&Enumerable.Range(1,stageIncrements.Length).Any(s=>native.Cases.Count(c=>c.Stage==s)!=stageIncrements[s-1].Value))throw new InvalidDataException("UnexpectedStageIncrementCount");
                // C4 (point 3): the increments of stage 0, one result case each.
                if(unstaged&&native.Cases.Count!=stageIncrements.Single().Value)throw new InvalidDataException("UnexpectedStageIncrementCount");
                var names=model.LoadCases.Values.ToDictionary(c=>loadCases[c.Guid],c=>c.Name);
                var cases=new Dictionary<int,string>();var parentNames=new Dictionary<int,string>();var steps=new Dictionary<int,string>();
                string Text(int n)=>n.ToString(System.Globalization.CultureInfo.InvariantCulture);
                // The name of a primary result case: "<load case>: <name>"; with the combinations opened, "<result case>: [<load case>: <name>]".
                string Primary(int resultCase,int loadCase,string name)=>combined?Text(resultCase)+": ["+Text(loadCase)+": "+name+"]":Text(loadCase)+": "+name;
                foreach(var c in native.Cases)
                {
                    if(stagedOutput.HasValue){cases.Add(c.Number,model.LoadCases.Values.Single(l=>l.Guid==stagedOutput.Value).Name);continue;}
                    if(c.Number>primaryCount)
                    {
                        int j=c.Number-primaryCount;var combination=model.Combinations.Values.SingleOrDefault(k=>k.Guid==combinations[j]);
                        if(combination==null||c.Name!=Text(c.Number)+": ["+Text(j)+": "+combination.Name+"] [Combination]")throw new InvalidDataException("StrausCombinationResultCaseMismatch");
                        cases.Add(c.Number,combination.Name);continue;
                    }
                    int lc=solvedCases[c.Number-1];
                    if(generatedCases.TryGetValue(lc,out var generatedName))
                    {
                        if(c.Name!=Primary(c.Number,lc,generatedName))throw new InvalidDataException("GeneratedMovingCaseNameMismatch");
                        Guid parent=generatedCaseParents==null?generatedParents[0]:generatedCaseParents[lc];
                        string parentName=model.LoadCases.Values.Single(l=>l.Guid==parent).Name;
                        cases.Add(c.Number,parentName);parentNames.Add(lc,parentName);steps.Add(c.Number,Text(lc)+": "+generatedName);continue;
                    }
                    // Verify the native number/name pair of every solved case (in native order).
                    if(!names.TryGetValue(lc,out var name)||c.Name!=Primary(c.Number,lc,name))
                        throw new InvalidDataException("StrausExportResultCaseMismatch");
                    cases.Add(c.Number,name);
                }
                var source=new AnalysisSource {Program="Straus7",SolverVersion=apiVersion,ModelRevision=IdentityMap.AnalysisFingerprint,
                    AnalysisId=ModelHash,GeometryHash=ModelHash};
                var semantics=stagedOutput.HasValue?AnalysisSemantics.NonlinearStatic:AnalysisSemantics.LinearStatic;
                var identityEvidence=Straus7ApiConverter.Evidence("ExportIdentityMap/"+ModelHash,IdentityMap);
                var generatedEvidence=generatedCases.Count==0?null:Straus7ApiConverter.Evidence("GeneratedMovingCases/"+ModelHash,generatedCases.ToDictionary(p=>p.Key,p=>p.Value));
                var parentEvidence=generatedCases.Count==0?null:Straus7ApiConverter.Evidence("GeneratedMovingCaseParents/"+ModelHash,
                    generatedCases.Keys.Where(parentNames.ContainsKey).ToDictionary(n=>n,n=>parentNames[n]));
                var report=Straus7LinearStaticResults.Normalize(model,native,datasetId,c=>cases[c.Number],(c,n)=>new ResultState {
                    Semantics=semantics,IsCombined=!stagedOutput.HasValue&&c.Number>primaryCount,IsCumulative=true,
                    ConcomitantStateId=!stagedOutput.HasValue&&c.Number>primaryCount?"Straus7 combination case "+c.Number:"Straus7 primary case "+c.Number,
                    Phase=stagedOutput.HasValue?stageIncrements[unstaged?0:c.Stage-1].Key:null,
                    Step=stagedOutput.HasValue?Straus7StageResults.Step(c):steps.TryGetValue(c.Number,out var step)?step:null,
                    HistoryId=stagedOutput.HasValue?"Straus7 NLA stages":null,
                    Components=Enumerable.Repeat(ComponentAvailability.Available,n).ToArray(),Coverage="Explicit exported entities and native stations; no continuous envelope."},
                    semantics,stagedOutput.HasValue?"GPC.Straus7.R3.ExportedStagedNonlinear/1":"GPC.Straus7.R3.ExportedLinearStatic/2",false,cancellation,IdentityMap,source);
                if(report.Status==ImportStatus.Completed||report.Status==ImportStatus.Partial)
                {
                    model.PreservedSourceData.Add(identityEvidence);
                    model.Datasets[datasetId].SourceHashes.Add("solver-identities",ModelValues.Fingerprint(new object[]{IdentityMap}));
                    model.Datasets[datasetId].SourceHashes.Add("section-export-configuration",ConfigurationFingerprint);
                    if(nativeSections.Count!=0)model.PreservedSourceData.Add(Straus7ApiConverter.Evidence("NativeSectionDefinitions/"+ModelHash,nativeSections));
                    if(sectionConfiguration.Count!=0)model.PreservedSourceData.Add(Straus7ApiConverter.Evidence("SectionExportConfiguration/"+ModelHash,sectionConfiguration));
                    if(generatedEvidence!=null){model.PreservedSourceData.Add(generatedEvidence);model.PreservedSourceData.Add(parentEvidence);}
                }
                return report;
            }
            catch(OperationCanceledException){rejected.Status=ImportStatus.Cancelled;}
            catch(Exception ex)when(ex is ArgumentException||ex is InvalidOperationException||ex is IOException||ex is InvalidDataException||ex is NotSupportedException)
            {rejected.Diagnostics.Add(ModelDiagnostic.Error("StrausExportResultImportRejected",message:ex.Message));}
            return rejected;
        }
        private static string Hash(string path)
        {using(var sha=SHA256.Create())using(var file=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(file)).Replace("-","");}
    }

    public sealed partial class Straus7ModelWritePlan
    {
        /// <summary>Creates new native model/result files and returns their verified binding, including generated moving cases. No overwrite or stages.</summary>
        public Straus7ModelRun RunLinearStatic(global::GPC.Model.Models.Model model,string outputPath,string scratchPath)
            =>RunLinearStatic(model,outputPath,scratchPath,null);
        /// <summary>C2 (P26-04, P26-06, P26-07): as <see cref="RunLinearStatic(GPC.Model.Models.Model,string,string)"/>, with the load cases to
        /// solve, the combinations to read from Straus7 and the solver settings of the file (<see cref="Straus7LinearRunOptions"/>); null: as before.</summary>
        public Straus7ModelRun RunLinearStatic(global::GPC.Model.Models.Model model,string outputPath,string scratchPath,Straus7LinearRunOptions options)
        {
            if(model==null)throw new ArgumentNullException(nameof(model));
            if(model.Stages.Count!=0||LoadCaseIds.Count==0)
                throw new NotSupportedException("LinearExportRunRequiresExplicitPrimaryCasesWithoutStages");
            IdentityMap.ValidateAgainst(model);
            if(GroupFingerprint(model)!=groupFingerprint)throw new InvalidOperationException("StaleStrausModelPlan");
            options=options??new Straus7LinearRunOptions();
            int[] ordinary=options.LoadCases?.Select(g=>LoadCaseIds.TryGetValue(g,out int id)?id:throw new ArgumentException("UnknownSolvedLoadCase")).Distinct().OrderBy(i=>i).ToArray();
            var combinations=new Dictionary<int,Guid>();
            foreach(var guid in (options.Combinations??new Guid[0]).Distinct())
            {
                var combination=model.Combinations.Values.SingleOrDefault(c=>c.Guid==guid);
                if(combination==null||!CombinationIds.TryGetValue(guid,out int number))throw new ArgumentException("UnknownReadCombination");
                // Every case with a non-zero factor must be solved: a disabled case would be missing from the combination Straus7 computes.
                if(ordinary!=null&&combination.GetLoadCaseCoefficientsPair().Any(p=>p.Value!=0&&!ordinary.Contains(LoadCaseIds[p.Key.Guid])))
                    throw new ArgumentException("ReadCombinationRequiresItsSolvedCases: "+combination.Name);
                combinations.Add(number,guid);
            }
            bool selection=options.LoadCases!=null||!options.SolveGeneratedMovingCases;
            return Fresh(outputPath,()=>
            {
                using(var writer=new Straus7NativeWriter(outputPath,scratchPath))
                {
                    var generated=Write(writer,model);
                    int[] enabled=null;
                    if(selection)
                    {
                        enabled=(ordinary??Enumerable.Range(1,LoadCaseIds.Count).ToArray()).Concat(options.SolveGeneratedMovingCases?generated.Keys:Enumerable.Empty<int>()).Distinct().OrderBy(i=>i).ToArray();
                        if(enabled.Length==0)throw new ArgumentException("LinearRunWithoutSolvedLoadCase");
                    }
                    string result=writer.Solve(options.FileSolverDefaults?new Straus7SolveOptions {EnabledLoadCases=enabled,NonlinearGeometry=null,NonlinearMaterial=null}
                        :new Straus7SolveOptions {EnabledLoadCases=enabled});
                    return new Straus7ModelRun(this,writer.OutputPath,result,writer.Reader.Version,generated,null,null,enabled,combinations);
                }
            });
        }
        /// <summary>Staged static solve with small-displacement kinematics. The output case must be declared before plan preparation.
        /// Material nonlinearity is optional; totals are retained per stage/increment and cannot be linearly superposed.</summary>
        public Straus7ModelRun RunStagedNonlinear(global::GPC.Model.Models.Model model,Guid outputCaseGuid,string outputPath,string scratchPath,bool nonlinearMaterial=false,
            double displacementTolerance=1e-8,double residualTolerance=1e-8,int maximumIterations=100)
            =>RunStagedNonlinear(model,outputCaseGuid,outputPath,scratchPath,Straus7NonlinearRunOptions.Explicit(nonlinearMaterial,displacementTolerance,residualTolerance,maximumIterations));
        /// <summary>C2 (P26-06): as the other overload, with the solver settings given (<see cref="Straus7NonlinearRunOptions"/>): those of today, or
        /// the settings of the new file (<see cref="Straus7NonlinearRunOptions.FileDefaults"/>: geometric and material nonlinearity, the convergence
        /// controls of Straus7), as BridGHe 0.3.0 solved.</summary>
        public Straus7ModelRun RunStagedNonlinear(global::GPC.Model.Models.Model model,Guid outputCaseGuid,string outputPath,string scratchPath,Straus7NonlinearRunOptions options)
        {
            if(model==null)throw new ArgumentNullException(nameof(model));IdentityMap.ValidateAgainst(model);
            if(options==null)throw new ArgumentNullException(nameof(options));
            if(model.Stages.Count==0||!model.LoadCases.Values.Any(c=>c.Guid==outputCaseGuid&&c is GPC.Model.Stages.StageResultCase))throw new ArgumentException("StagedRunRequiresDeclaredOutputCase");
            if(GroupFingerprint(model)!=groupFingerprint)throw new InvalidOperationException("StaleStrausModelPlan");
            var increments=model.Stages.Values.OrderBy(s=>s.Definition.Sequence).Select(s=>new KeyValuePair<string,int>(s.Name,s.AnalysisState.Increments.Length)).ToArray();
            return Fresh(outputPath,()=>
            {
                using(var writer=new Straus7NativeWriter(outputPath,scratchPath))
                {
                    if(!options.FileDefaults)writer.SetNonlinearConvergence(options.DisplacementTolerance,options.ResidualTolerance,options.MaximumIterations);
                    Write(writer,model);
                    string result=writer.Solve(options.FileDefaults?new Straus7SolveOptions {StagedNonlinear=true,NonlinearGeometry=null,NonlinearMaterial=null}
                        :new Straus7SolveOptions {StagedNonlinear=true,NonlinearGeometry=false,NonlinearMaterial=options.NonlinearMaterial});
                    return new Straus7ModelRun(this,writer.OutputPath,result,writer.Reader.Version,null,outputCaseGuid,increments);
                }
            });
        }
        /// <summary>C4 (point 3): the nonlinear static solve without stages of BridGHe 0.3.0 (its solver 1, StrausAnalysis.cs of 0.3.0): the
        /// increments of stage 0 that every plan of a model without stages writes (one per combination, NonlinearCombinationIncrements), solved by
        /// the NLA solver with the staged flag off. The results of every increment are attached to the declared output case, as those of a staged
        /// run, with Phase = <paramref name="phase"/> and Step = the native name of the increment ("k: name"); the provenance labels are those of
        /// the staged run. <paramref name="options"/>: the solver settings, as <see cref="RunStagedNonlinear(GPC.Model.Models.Model,Guid,string,string,Straus7NonlinearRunOptions)"/>.</summary>
        public Straus7ModelRun RunUnstagedNonlinear(global::GPC.Model.Models.Model model,Guid outputCaseGuid,string outputPath,string scratchPath,Straus7NonlinearRunOptions options,string phase)
        {
            if(model==null)throw new ArgumentNullException(nameof(model));IdentityMap.ValidateAgainst(model);
            if(options==null)throw new ArgumentNullException(nameof(options));
            if(string.IsNullOrWhiteSpace(phase))throw new ArgumentException("UnstagedRunRequiresPhaseName");
            if(model.Stages.Count!=0)throw new NotSupportedException("UnstagedRunRequiresModelWithoutStages");
            if(!model.LoadCases.Values.Any(c=>c.Guid==outputCaseGuid&&c is GPC.Model.Stages.StageResultCase))throw new ArgumentException("UnstagedRunRequiresDeclaredOutputCase");
            if(model.Combinations.Count==0)throw new ArgumentException("UnstagedRunRequiresCombinations");
            if(GroupFingerprint(model)!=groupFingerprint)throw new InvalidOperationException("StaleStrausModelPlan");
            var increments=new[]{new KeyValuePair<string,int>(phase,model.Combinations.Count)};
            return Fresh(outputPath,()=>
            {
                using(var writer=new Straus7NativeWriter(outputPath,scratchPath))
                {
                    if(!options.FileDefaults)writer.SetNonlinearConvergence(options.DisplacementTolerance,options.ResidualTolerance,options.MaximumIterations);
                    Write(writer,model);
                    string result=writer.Solve(options.FileDefaults?new Straus7SolveOptions {UnstagedNonlinear=true,NonlinearGeometry=null,NonlinearMaterial=null}
                        :new Straus7SolveOptions {UnstagedNonlinear=true,NonlinearGeometry=false,NonlinearMaterial=options.NonlinearMaterial});
                    return new Straus7ModelRun(this,writer.OutputPath,result,writer.Reader.Version,null,outputCaseGuid,increments,unstaged:true);
                }
            });
        }
        /// <summary>A failed write or solve removes the files it created (model, results, solver logs with the same stem),
        /// so the same output path can be retried; files that existed before are never removed.</summary>
        private static Straus7ModelRun Fresh(string outputPath,Func<Straus7ModelRun> run)
        {
            if(string.IsNullOrWhiteSpace(outputPath))throw new ArgumentException("OutputPathRequired");
            var files=new Straus7ScratchFiles(outputPath);
            try {return run();}
            catch {files.DeleteCreated();throw;}
        }
    }
}
