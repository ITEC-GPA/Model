using GPC.Model.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;
using GPC.Model.ElementProperties;
using GPC.Model.Core;
using GPC.Model.Sections.Concrete;
using GPC.Model.Analysis;
using GPC.Model.Checking.Scenarios;

namespace GPC.Model.Checking.Contracts
{
    /// <summary>Immutable identities and value digests retained with each verification outcome.</summary>
    [Serializable]
    [System.Runtime.Serialization.DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class VerificationProvenance
    {
        [field: System.Runtime.Serialization.DataMember(Name = "<AnalysisSnapshotId>k__BackingField", IsRequired = true)]
        public string AnalysisSnapshotId { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<AnalysisFingerprint>k__BackingField", IsRequired = true)]
        public string AnalysisFingerprint { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ScenarioId>k__BackingField", IsRequired = true)]
        public string ScenarioId { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<ScenarioFingerprint>k__BackingField", IsRequired = true)]
        public string ScenarioFingerprint { get; private set; }

        [field: System.Runtime.Serialization.DataMember(Name = "<DesignFingerprint>k__BackingField", IsRequired = true)]
        public string DesignFingerprint { get; private set; }

        [System.Runtime.Serialization.DataMember(IsRequired = true)]
        private Dictionary<string, string> _datasets;
        public IReadOnlyDictionary<string, string> DatasetFingerprints => new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(_datasets);

        internal static VerificationProvenance Capture(Models.Model model, VerificationScenario scenario = null) => new VerificationProvenance
        {
            AnalysisSnapshotId = model.Analysis?.Id,
            AnalysisFingerprint = model.Analysis?.InputFingerprint,
            ScenarioId = scenario?.Id ?? "current-model",
            ScenarioFingerprint = scenario?.Fingerprint ?? AnalysisStorage.Design(model),
            DesignFingerprint = AnalysisStorage.Design(model),
            _datasets = model.Datasets.ToDictionary(p => p.Key, p => ModelValues.Fingerprint(new object[] { p.Value }))
        };
        public bool IsCurrent(Models.Model model)
        {
            try
            {
                return model != null && AnalysisSnapshotId != null && model.Analysis?.Id == AnalysisSnapshotId && (!model.VerificationScenarios.TryGetValue(ScenarioId, out var scenario) || scenario.Fingerprint == ScenarioFingerprint) && model.AnalysisFingerprint() == AnalysisFingerprint && AnalysisStorage.Design(model) == DesignFingerprint && model.Datasets.Count == _datasets.Count && _datasets.All(p => model.Datasets.TryGetValue(p.Key, out var value) && ModelValues.Fingerprint(new object[] { value }) == p.Value) && AnalysisCompatibilityValidator.Validate(model).Status == AnalysisCompatibility.Compatible;
            }
            catch (Exception ex)when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            {
                return false;
            }
        }
    }
}
