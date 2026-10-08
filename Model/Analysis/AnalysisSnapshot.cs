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
    public enum AnalysisCompatibility { Unknown, Compatible, RequiresReanalysis }
    public enum ReinforcementAnalysisRole { Unknown, ExcludedFromAnalysis, IncludedInAnalysis }

    /// <summary>An independent, immutable copy of the inputs recorded BEFORE importing/calculating results.
    /// OpenModel always returns a new graph. Capturing a current design is not evidence of a historical analysis.</summary>
    [Serializable]
    public sealed class AnalysisSnapshot
    {
        private byte[] _model;
        private string _contentHash;
        public string Id { get; private set; }
        public Guid ModelGuid { get; private set; }
        public string InputFingerprint { get; private set; }
        public string SourceFingerprint { get; private set; }
        public string ReinforcementFingerprint { get; private set; }
        public string PrestressFingerprint { get; private set; }
        public ReinforcementAnalysisRole ReinforcementRole { get; private set; }
        public string AssumptionReference { get; private set; }
        private AnalysisSnapshot() { }
        public static AnalysisSnapshot Capture(Models.Model model, ReinforcementAnalysisRole reinforcementRole = ReinforcementAnalysisRole.Unknown,
            string assumptionReference = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (!Enum.IsDefined(typeof(ReinforcementAnalysisRole), reinforcementRole)) throw new ArgumentOutOfRangeException(nameof(reinforcementRole));
            if (reinforcementRole != ReinforcementAnalysisRole.Unknown && string.IsNullOrWhiteSpace(assumptionReference))
                throw new ArgumentException("An explicit analysis assumption and its source are required.", nameof(assumptionReference));
            if (model.Datasets.Count != 0 || model.AllElements.Any(e => e.Results.Any()))
                throw new InvalidOperationException("Capture analysis inputs before results. Historical provenance cannot be inferred from the current design.");
            var bytes = AnalysisStorage.Write(model.AnalysisArchiveView());
            var copy = AnalysisStorage.Read<Models.Model>(bytes);
            return new AnalysisSnapshot { Id = Guid.NewGuid().ToString("D"), ModelGuid = copy.Guid, _model = bytes,
                _contentHash = AnalysisStorage.Digest(bytes), InputFingerprint = copy.AnalysisFingerprint(),
                SourceFingerprint = ModelArchive.Fingerprint(new object[] { copy.AnalysisSource }),
                ReinforcementFingerprint = AnalysisStorage.Reinforcement(copy), PrestressFingerprint = AnalysisStorage.Prestress(copy),
                ReinforcementRole = reinforcementRole, AssumptionReference = assumptionReference };
        }
        public Models.Model OpenModel()
        {
            if (_model == null || _contentHash != AnalysisStorage.Digest(_model) || string.IsNullOrWhiteSpace(Id)
                || !Enum.IsDefined(typeof(ReinforcementAnalysisRole), ReinforcementRole)
                || ReinforcementRole != ReinforcementAnalysisRole.Unknown && string.IsNullOrWhiteSpace(AssumptionReference))
                throw new SerializationException("Invalid analysis snapshot.");
            var model = AnalysisStorage.Read<Models.Model>(_model);
            if (model.Analysis != null || model.VerificationContext != null || model.Datasets.Count != 0 || model.AllElements.Any(e => e.Results.Any())
                || model.Guid != ModelGuid || model.AnalysisFingerprint() != InputFingerprint
                || ModelArchive.Fingerprint(new object[] { model.AnalysisSource }) != SourceFingerprint
                || AnalysisStorage.Reinforcement(model) != ReinforcementFingerprint || AnalysisStorage.Prestress(model) != PrestressFingerprint)
                throw new SerializationException("Analysis snapshot inputs disagree with their provenance.");
            return model;
        }
    }
}
