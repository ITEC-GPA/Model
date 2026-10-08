using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;
using GPC.Model.ElementProperties;
using GPC.Model.Persistence;
using GPC.Model.Sections.Concrete;

using GPC.Model.PostProcessing;

namespace GPC.Model.PostProcessing
{
    public sealed class AnalysisCompatibilityResult
    {
        public AnalysisCompatibility Status { get; }
        public string Code { get; }
        public string Message { get; }
        internal AnalysisCompatibilityResult(AnalysisCompatibility status, string code, string message)
        { Status = status; Code = code; Message = message; }
    }
    public static class AnalysisCompatibilityValidator
    {
        // Low-level legacy section preparation remains usable without a Model analysis record.
        // ModelChecker always requires the strict Validate gate, including for legacy models.
        internal static bool KnownAnalysisIsCompatible(Models.Model model) => model.Analysis == null
            || Validate(model).Status == AnalysisCompatibility.Compatible && (model.VerificationContext == null || model.VerificationContext.IsCurrent(model));
        internal static ModelDiagnostic KnownAnalysisDiagnostic(Models.Model model, out DataStatus status)
        {
            status = DataStatus.Insufficient;
            if (KnownAnalysisIsCompatible(model)) return null;
            var result = Validate(model);
            if (result.Status == AnalysisCompatibility.RequiresReanalysis || result.Status == AnalysisCompatibility.Compatible) status = DataStatus.Stale;
            return ModelDiagnostic.Error(result.Status == AnalysisCompatibility.Compatible ? "StalePreparedScenario" : result.Code, message: result.Message);
        }
        public static AnalysisCompatibilityResult Validate(Models.Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            try { return ValidateInputs(model); }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            { return Unknown("InvalidAnalysisInputs", ex.Message); }
        }
        private static AnalysisCompatibilityResult ValidateInputs(Models.Model model)
        {
            var analysis = model.Analysis;
            if (analysis == null) return Unknown("UnknownAnalysisProvenance", "Record the original analysis inputs before results; legacy provenance is insufficient for automatic verification.");
            if (model.Guid != analysis.ModelGuid) return Unknown("ForeignAnalysisSnapshot", "The snapshot belongs to another model.");
            if (ModelArchive.Fingerprint(new object[] { model.AnalysisSource }) != analysis.SourceFingerprint)
                return Unknown("AnalysisSourceChanged", "The solver provenance changed since the recorded analysis.");
            if (model.AnalysisFingerprint() != analysis.InputFingerprint)
                return new AnalysisCompatibilityResult(AnalysisCompatibility.RequiresReanalysis, "RequiresReanalysis", "Physical analysis inputs changed. Reanalyse the scenario before verification.");
            foreach (var pair in model.Datasets)
                if (pair.Value == null || pair.Key != pair.Value.Id || pair.Value.InputFingerprint != analysis.InputFingerprint
                    || pair.Value.AnalysisSnapshotId != null && pair.Value.AnalysisSnapshotId != analysis.Id)
                    return Unknown("DatasetAnalysisMismatch", "A dataset is not bound to the recorded analysis.");
            if (AnalysisStorage.Prestress(model) != analysis.PrestressFingerprint)
                return new AnalysisCompatibilityResult(AnalysisCompatibility.RequiresReanalysis, "PrestressRequiresReanalysis", "Prestressing changed and can change the applied actions.");
            if (AnalysisStorage.Reinforcement(model) != analysis.ReinforcementFingerprint)
            {
                if (analysis.ReinforcementRole == ReinforcementAnalysisRole.Unknown)
                    return Unknown("UnknownReinforcementInfluence", "The analysis does not declare whether reinforcement affects stiffness or response.");
                if (analysis.ReinforcementRole == ReinforcementAnalysisRole.IncludedInAnalysis)
                    return new AnalysisCompatibilityResult(AnalysisCompatibility.RequiresReanalysis, "ReinforcementRequiresReanalysis", "The analysis includes the modified reinforcement.");
            }
            return new AnalysisCompatibilityResult(AnalysisCompatibility.Compatible, "CompatibleAnalysis", "The scenario preserves the recorded analysis assumptions.");
        }
        private static AnalysisCompatibilityResult Unknown(string code, string message) => new AnalysisCompatibilityResult(AnalysisCompatibility.Unknown, code, message);
    }
}
