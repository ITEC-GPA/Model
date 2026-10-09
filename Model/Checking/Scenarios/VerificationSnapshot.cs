using System;
using GPC.Model.Analysis;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;

namespace GPC.Model.Checking.Scenarios
{
    /// <summary>Immutable execution input, including analysis provenance, design and result values.
    /// Each execution receives its own graph; editing the source or a previously opened graph cannot alter it.</summary>
    public sealed class VerificationSnapshot
    {
        private readonly byte[] _content;
        private readonly string _results;
        private readonly VerificationProvenance _provenance;
        public string AnalysisId => _provenance.AnalysisSnapshotId;
        public string ScenarioId => _provenance.ScenarioId;
        private VerificationSnapshot(PreparedVerification prepared)
        {
            if (!prepared.IsCurrent || prepared.Compatibility.Status != AnalysisCompatibility.Compatible)
                throw new InvalidOperationException("CompatibleCurrentPreparationRequired");
            _content = AnalysisStorage.Write(prepared.Model);
            var copy = OpenModel();
            _provenance = copy.VerificationContext;
            _results = AnalysisStorage.Results(copy);
            if (!prepared.IsCurrent || !IsCurrent(prepared.Model)) throw new InvalidOperationException("InputsChangedDuringSnapshotCapture");
        }
        public static VerificationSnapshot Capture(PreparedVerification prepared)
            => new VerificationSnapshot(prepared ?? throw new ArgumentNullException(nameof(prepared)));
        public Models.Model OpenModel() => AnalysisStorage.Read<Models.Model>(_content);
        public bool IsCurrent(Models.Model model)
        {
            if (model == null) return false;
            try { return _provenance.IsCurrent(model) && _results == AnalysisStorage.Results(model); }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException) { return false; }
        }
    }
}
