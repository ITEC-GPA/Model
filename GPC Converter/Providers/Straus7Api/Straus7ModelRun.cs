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
        internal Straus7ModelRun(Straus7ModelWritePlan plan,string modelPath,string resultPath,string apiVersion,
            IReadOnlyDictionary<int,string> generated=null,Guid? stagedOutput=null,KeyValuePair<string,int>[] stageIncrements=null)
        {
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
                if(generatedCaseParents!=null&&(generatedCaseParents.Count!=generatedCases.Count||generatedCaseParents.Keys.Any(k=>!generatedCases.ContainsKey(k))||generatedCaseParents.Values.Any(g=>!generatedParents.Contains(g))))
                    throw new ArgumentException("InvalidGeneratedMovingCaseParentMap");
                if(generatedCases.Count>0&&generatedCaseParents==null&&generatedParents.Length!=1)throw new ArgumentException("GeneratedMovingCasesRequireExplicitParentMap");
                int expectedCount=stagedOutput.HasValue?stageIncrements.Sum(p=>p.Value):loadCases.Count+generatedCases.Count;
                var patches=new HashSet<Guid>(model.AreaElements.Values.Where(p=>p.PlateProperty is ElasticPlateProperty e&&e.Formulation==ElasticPlateFormulation.LoadPatch).Select(p=>p.Guid));
                int[] Ids(EntityFamily family)=>IdentityMap.Entries.Where(e=>e.Family==family&&!patches.Contains(e.EntityGuid)).Select(e=>e.SolverId).ToArray();
                var native=new Straus7ApiConverter().ReadResults(new Straus7ResultsRequest {ModelPath=ModelPath,ResultPath=ResultPath,
                    CaseNumbers=Enumerable.Range(1,expectedCount).ToArray(),NodeNumbers=Ids(EntityFamily.Node),
                    BeamNumbers=Ids(EntityFamily.Beam),PlateNumbers=Ids(EntityFamily.Shell),BeamMinimumStations=minimumStations,
                    IncludeElementNodeForces=includeElementNodeForces,IncludePlateNodeResults=includePlateNodeResults},cancellation);
                if(native.Status!=ImportStatus.Completed||native.File==null||native.File.ValidationFlags!=0||native.File.Solver!=(stagedOutput.HasValue?3:1)
                    ||native.ModelHash!=ModelHash||native.ResultHash!=ResultHash||native.ApiVersion!=apiVersion
                    ||native.Cases.Count!=expectedCount||native.Cases.Any(c=>stagedOutput.HasValue?c.Stage<1||c.Stage>stageIncrements.Length:c.Stage!=0))throw new InvalidDataException("InvalidStrausExportResultFile");
                if(stagedOutput.HasValue&&Enumerable.Range(1,stageIncrements.Length).Any(s=>native.Cases.Count(c=>c.Stage==s)!=stageIncrements[s-1].Value))throw new InvalidDataException("UnexpectedStageIncrementCount");
                var names=model.LoadCases.Values.ToDictionary(c=>loadCases[c.Guid],c=>c.Name);
                var cases=new Dictionary<int,string>();
                foreach(var c in native.Cases)
                {
                    if(stagedOutput.HasValue){cases.Add(c.Number,model.LoadCases.Values.Single(l=>l.Guid==stagedOutput.Value).Name);continue;}
                    if(generatedCases.TryGetValue(c.Number,out var generatedName))
                    {
                        if(c.Name!=c.Number.ToString(System.Globalization.CultureInfo.InvariantCulture)+": "+generatedName)throw new InvalidDataException("GeneratedMovingCaseNameMismatch");
                        Guid parent=generatedCaseParents==null?generatedParents[0]:generatedCaseParents[c.Number];
                        cases.Add(c.Number,model.LoadCases.Values.Single(l=>l.Guid==parent).Name);continue;
                    }
                    // RunLinearStatic solves all defined load cases, in native order; verify the native number/name pair.
                    if(!names.TryGetValue(c.Number,out var name)||c.Name!=c.Number.ToString(System.Globalization.CultureInfo.InvariantCulture)+": "+name)
                        throw new InvalidDataException("StrausExportResultCaseMismatch");
                    cases.Add(c.Number,name);
                }
                var source=new AnalysisSource {Program="Straus7",SolverVersion=apiVersion,ModelRevision=IdentityMap.AnalysisFingerprint,
                    AnalysisId=ModelHash,GeometryHash=ModelHash};
                var semantics=stagedOutput.HasValue?AnalysisSemantics.NonlinearStatic:AnalysisSemantics.LinearStatic;
                var identityEvidence=Straus7ApiConverter.Evidence("ExportIdentityMap/"+ModelHash,IdentityMap);
                var generatedEvidence=generatedCases.Count==0?null:Straus7ApiConverter.Evidence("GeneratedMovingCases/"+ModelHash,generatedCases.ToDictionary(p=>p.Key,p=>p.Value));
                var parentEvidence=generatedCases.Count==0?null:Straus7ApiConverter.Evidence("GeneratedMovingCaseParents/"+ModelHash,generatedCases.Keys.ToDictionary(n=>n,n=>cases[n]));
                var report=Straus7LinearStaticResults.Normalize(model,native,datasetId,c=>cases[c.Number],(c,n)=>new ResultState {
                    Semantics=semantics,IsCombined=false,IsCumulative=true,ConcomitantStateId="Straus7 primary case "+c.Number,
                    Phase=stagedOutput.HasValue?stageIncrements[c.Stage-1].Key:null,
                    Step=stagedOutput.HasValue?Straus7StageResults.Step(c):generatedCases.ContainsKey(c.Number)?c.Name:null,
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
        {
            if(model==null)throw new ArgumentNullException(nameof(model));
            if(model.Stages.Count!=0||LoadCaseIds.Count==0)
                throw new NotSupportedException("LinearExportRunRequiresExplicitPrimaryCasesWithoutStages");
            IdentityMap.ValidateAgainst(model);
            if(GroupFingerprint(model)!=groupFingerprint)throw new InvalidOperationException("StaleStrausModelPlan");
            using(var writer=new Straus7NativeWriter(outputPath,scratchPath))
            {
                var generated=Write(writer,model);string result=writer.Solve();
                return new Straus7ModelRun(this,writer.OutputPath,result,writer.Reader.Version,generated);
            }
        }
        /// <summary>Staged static solve with small-displacement kinematics. The output case must be declared before plan preparation.
        /// Material nonlinearity is optional; totals are retained per stage/increment and cannot be linearly superposed.</summary>
        public Straus7ModelRun RunStagedNonlinear(global::GPC.Model.Models.Model model,Guid outputCaseGuid,string outputPath,string scratchPath,bool nonlinearMaterial=false,
            double displacementTolerance=1e-8,double residualTolerance=1e-8,int maximumIterations=100)
        {
            if(model==null)throw new ArgumentNullException(nameof(model));IdentityMap.ValidateAgainst(model);
            if(model.Stages.Count==0||!model.LoadCases.Values.Any(c=>c.Guid==outputCaseGuid&&c is GPC.Model.Stages.StageResultCase))throw new ArgumentException("StagedRunRequiresDeclaredOutputCase");
            if(GroupFingerprint(model)!=groupFingerprint)throw new InvalidOperationException("StaleStrausModelPlan");
            var increments=model.Stages.Values.OrderBy(s=>s.Definition.Sequence).Select(s=>new KeyValuePair<string,int>(s.Name,s.AnalysisState.Increments.Length)).ToArray();
            using(var writer=new Straus7NativeWriter(outputPath,scratchPath))
            {
                writer.SetNonlinearConvergence(displacementTolerance,residualTolerance,maximumIterations);
                Write(writer,model);string result=writer.Solve(stagedNonlinear:true,nonlinearMaterial:nonlinearMaterial);
                return new Straus7ModelRun(this,writer.OutputPath,result,writer.Reader.Version,null,outputCaseGuid,increments);
            }
        }
    }
}
