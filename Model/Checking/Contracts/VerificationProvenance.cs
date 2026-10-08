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
    /// <summary>Immutable identities and value digests retained with each verification outcome.</summary>
    [Serializable]
    public sealed class VerificationProvenance
    {
        public string AnalysisSnapshotId { get; private set; }
        public string AnalysisFingerprint { get; private set; }
        public string ScenarioId { get; private set; }
        public string ScenarioFingerprint { get; private set; }
        public string DesignFingerprint { get; private set; }
        private Dictionary<string, string> _datasets;
        public IReadOnlyDictionary<string, string> DatasetFingerprints => new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(_datasets);
        internal static VerificationProvenance Capture(Models.Model model, VerificationScenario scenario = null) => new VerificationProvenance {
            AnalysisSnapshotId = model.Analysis?.Id, AnalysisFingerprint = model.Analysis?.InputFingerprint,
            ScenarioId = scenario?.Id ?? "current-model", ScenarioFingerprint = scenario?.Fingerprint ?? AnalysisStorage.Design(model),
            DesignFingerprint = AnalysisStorage.Design(model), _datasets = model.Datasets.ToDictionary(p => p.Key, p => ModelArchive.Fingerprint(new object[] { p.Value })) };
        public bool IsCurrent(Models.Model model)
        {
            try
            {
                return model != null && AnalysisSnapshotId != null && model.Analysis?.Id == AnalysisSnapshotId
                    && (!model.VerificationScenarios.TryGetValue(ScenarioId, out var scenario) || scenario.Fingerprint == ScenarioFingerprint)
                    && model.AnalysisFingerprint() == AnalysisFingerprint && AnalysisStorage.Design(model) == DesignFingerprint
                    && model.Datasets.Count == _datasets.Count && _datasets.All(p => model.Datasets.TryGetValue(p.Key, out var value)
                        && ModelArchive.Fingerprint(new object[] { value }) == p.Value)
                    && AnalysisCompatibilityValidator.Validate(model).Status == AnalysisCompatibility.Compatible;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            { return false; }
        }
    }
}
