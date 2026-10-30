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
using GPC.Model.Checking.Preparation;
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

        internal static VerificationProvenance Capture(Models.Model model, VerificationScenario scenario = null)
        {
            // The design digest once for the scenario and the design (they were the same walk, twice). In a read scope (the section checks
            // of a SectionCheckRun) the design and dataset digests are those of the scope, computed once for all its captures and for the
            // currency checks. Same values, read in the same order.
            string scenarioFingerprint = scenario?.Fingerprint;
            string design = ValidationReadScope.Read(model, "design-fingerprint", () => AnalysisStorage.Design(model));
            return new VerificationProvenance
            {
                AnalysisSnapshotId = model.Analysis?.Id,
                AnalysisFingerprint = model.Analysis?.InputFingerprint,
                ScenarioId = scenario?.Id ?? "current-model",
                ScenarioFingerprint = scenarioFingerprint ?? design,
                DesignFingerprint = design,
                _datasets = model.Datasets.ToDictionary(p => p.Key, p => ValidationReadScope.FingerprintOf(model, p.Value))
            };
        }
        public bool IsCurrent(Models.Model model)
        {
            if (model == null)
                return false;
            // One read scope: the analysis fingerprint is computed once for the comparison and for the compatibility gate. Within the read
            // session of a whole report (CheckReport.CurrentOutcome) the scenario, analysis, design and dataset digests are computed once
            // for all its rows. Same comparisons, in the same order, as without the scope.
            using (ValidationReadScope.Enter(model))
            {
                try
                {
                    return AnalysisSnapshotId != null && model.Analysis?.Id == AnalysisSnapshotId && (!model.VerificationScenarios.TryGetValue(ScenarioId, out var scenario) || ValidationReadScope.Read(model, scenario, "scenario-fingerprint", () => scenario.Fingerprint) == ScenarioFingerprint) && model.AnalysisFingerprint() == AnalysisFingerprint && ValidationReadScope.Read(model, "design-fingerprint", () => AnalysisStorage.Design(model)) == DesignFingerprint && model.Datasets.Count == _datasets.Count && _datasets.All(p => model.Datasets.TryGetValue(p.Key, out var value) && ValidationReadScope.FingerprintOf(model, value) == p.Value) && AnalysisCompatibilityValidator.Validate(model).Status == AnalysisCompatibility.Compatible;
                }
                catch (Exception ex)when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
                {
                    return false;
                }
            }
        }
    }
}
