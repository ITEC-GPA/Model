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
        private readonly IReadOnlyDictionary<Guid,int> loadCases;
        private readonly IReadOnlyDictionary<int,int> minimumStations;
        internal Straus7ModelRun(Straus7ModelWritePlan plan,string modelPath,string resultPath,string apiVersion)
        {
            ModelPath=modelPath;ResultPath=resultPath;this.apiVersion=apiVersion;
            ModelHash=Hash(modelPath);ResultHash=Hash(resultPath);IdentityMap=plan.IdentityMap;
            loadCases=plan.LoadCaseIds;minimumStations=plan.MinimumOutputStations;
        }
        /// <summary>Read the produced primary cases, normalize N/mm/rad values and atomically attach the dataset.
        /// Load patches have no stress-resultant samples. Native source arrays and the solver map are retained as provenance.</summary>
        public ResultImportReport ImportResults(global::GPC.Model.Models.Model model,string datasetId,bool includeElementNodeForces=false,
            bool includePlateNodeResults=false,CancellationToken cancellation=default)
        {
            var rejected=new ResultImportReport {Status=ImportStatus.Rejected};
            try
            {
                cancellation.ThrowIfCancellationRequested();IdentityMap.ValidateAgainst(model);
                if(Hash(ModelPath)!=ModelHash||Hash(ResultPath)!=ResultHash)throw new InvalidDataException("StrausExportFilesChanged");
                var patches=new HashSet<Guid>(model.AreaElements.Values.Where(p=>p.PlateProperty is ElasticPlateProperty e&&e.Formulation==ElasticPlateFormulation.LoadPatch).Select(p=>p.Guid));
                int[] Ids(EntityFamily family)=>IdentityMap.Entries.Where(e=>e.Family==family&&!patches.Contains(e.EntityGuid)).Select(e=>e.SolverId).ToArray();
                var native=new Straus7ApiConverter().ReadResults(new Straus7ResultsRequest {ModelPath=ModelPath,ResultPath=ResultPath,
                    CaseNumbers=Enumerable.Range(1,loadCases.Count).ToArray(),NodeNumbers=Ids(EntityFamily.Node),
                    BeamNumbers=Ids(EntityFamily.Beam),PlateNumbers=Ids(EntityFamily.Shell),BeamMinimumStations=minimumStations,
                    IncludeElementNodeForces=includeElementNodeForces,IncludePlateNodeResults=includePlateNodeResults},cancellation);
                if(native.Status!=ImportStatus.Completed||native.File==null||native.File.ValidationFlags!=0||native.File.Solver!=1
                    ||native.ModelHash!=ModelHash||native.ResultHash!=ResultHash||native.ApiVersion!=apiVersion
                    ||native.Cases.Count!=loadCases.Count||native.Cases.Any(c=>c.Stage!=0))throw new InvalidDataException("InvalidStrausExportResultFile");
                var names=model.LoadCases.Values.ToDictionary(c=>loadCases[c.Guid],c=>c.Name);
                var cases=new Dictionary<int,string>();
                foreach(var c in native.Cases)
                {
                    // RunLinearStatic solves all defined load cases, in native order; verify the native number/name pair.
                    if(!names.TryGetValue(c.Number,out var name)||c.Name!=c.Number.ToString(System.Globalization.CultureInfo.InvariantCulture)+": "+name)
                        throw new InvalidDataException("StrausExportResultCaseMismatch");
                    cases.Add(c.Number,name);
                }
                var source=new AnalysisSource {Program="Straus7",SolverVersion=apiVersion,ModelRevision=IdentityMap.AnalysisFingerprint,
                    AnalysisId=ModelHash,GeometryHash=ModelHash};
                var report=Straus7LinearStaticResults.Normalize(model,native,datasetId,c=>cases[c.Number],(c,n)=>new ResultState {
                    Semantics=AnalysisSemantics.LinearStatic,IsCombined=false,IsCumulative=true,ConcomitantStateId="Straus7 primary case "+c.Number,
                    Components=Enumerable.Repeat(ComponentAvailability.Available,n).ToArray(),Coverage="Explicit exported entities and native stations; no continuous envelope."},
                    AnalysisSemantics.LinearStatic,"GPC.Straus7.R3.ExportedLinearStatic/1",false,cancellation,IdentityMap,source);
                if(report.Status==ImportStatus.Completed||report.Status==ImportStatus.Partial)
                {
                    model.PreservedSourceData.Add(Straus7ApiConverter.Evidence("ExportIdentityMap/"+ModelHash,IdentityMap));
                    model.Datasets[datasetId].SourceHashes.Add("solver-identities",ModelValues.Fingerprint(new object[]{IdentityMap}));
                }
                return report;
            }
            catch(OperationCanceledException){rejected.Status=ImportStatus.Cancelled;}
            catch(Exception ex)when(ex is ArgumentException||ex is InvalidOperationException||ex is IOException||ex is NotSupportedException)
            {rejected.Diagnostics.Add(ModelDiagnostic.Error("StrausExportResultImportRejected",message:ex.Message));}
            return rejected;
        }
        private static string Hash(string path)
        {using(var sha=SHA256.Create())using(var file=File.OpenRead(path))return BitConverter.ToString(sha.ComputeHash(file)).Replace("-","");}
    }

    public sealed partial class Straus7ModelWritePlan
    {
        /// <summary>Creates new native model/result files and returns their verified binding. No overwrite, stages or generated moving cases.</summary>
        public Straus7ModelRun RunLinearStatic(global::GPC.Model.Models.Model model,string outputPath,string scratchPath)
        {
            if(model==null)throw new ArgumentNullException(nameof(model));
            if(model.Stages.Count!=0||LoadCaseIds.Count==0||hasStaticMovingConversion)
                throw new NotSupportedException("LinearExportRunRequiresExplicitPrimaryCasesWithoutStagesOrGeneratedCases");
            IdentityMap.ValidateAgainst(model);
            if(GroupFingerprint(model)!=groupFingerprint)throw new InvalidOperationException("StaleStrausModelPlan");
            using(var writer=new Straus7NativeWriter(outputPath,scratchPath))
            {
                Write(writer,model);string result=writer.Solve();
                return new Straus7ModelRun(this,writer.OutputPath,result,writer.Reader.Version);
            }
        }
    }
}
