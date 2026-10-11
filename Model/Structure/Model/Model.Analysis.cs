using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Analysis;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Reports;
using GPC.Model.Checking.Scenarios;

namespace GPC.Model.Models
{
    public partial class Model
    {
        public AnalysisSnapshot Analysis { get; private set; }
        public VerificationProvenance VerificationContext { get; internal set; }
        public Dictionary<string, VerificationScenario> VerificationScenarios { get; private set; } = new Dictionary<string, VerificationScenario>();
        public AnalysisSnapshot CaptureAnalysis(ReinforcementAnalysisRole reinforcementRole = ReinforcementAnalysisRole.Unknown, string assumptionReference = null)
        {
            var snapshot = AnalysisSnapshot.Capture(this, reinforcementRole, assumptionReference);
            AttachAnalysis(snapshot); return snapshot;
        }
        internal void AttachAnalysis(AnalysisSnapshot snapshot)
        {
            if (snapshot == null || snapshot.ModelGuid != Guid) throw new ArgumentException("Snapshot belongs to another model.");
            Analysis = snapshot; VerificationContext = null;
        }
        internal Model AnalysisArchiveView()
        {
            var copy = (Model)MemberwiseClone();
            copy.Analysis = null; copy.VerificationContext = null; copy.CheckReports = new List<CheckReport>();
            copy.VerificationScenarios = new Dictionary<string, VerificationScenario>();
            return copy;
        }

        /// <summary>
        /// Encodes the results of every element in compact form and releases the decoded objects (for large imports: call it after
        /// each dataset). The results are decoded again, as new objects, on the next access to <see cref="Elements.Element.Results"/>:
        /// samples obtained before the call no longer belong to the model.
        /// </summary>
        /// <returns>The bytes of the compact results</returns>
        /// <exception cref="NotSupportedException">If a result references a sample of another element</exception>
        public long CompactResults()
        {
            var cases = new global::GPC.Model.Results.Storage.ResultCaseTable(this);
            long bytes = 0;
            foreach (var element in AllElements) bytes += element.CompactResults(cases, this).Bytes;
            return bytes;
        }

        /// <summary>True if a result of an element belongs to the dataset. Compact results are not decoded.</summary>
        public bool ContainsResultsOf(string datasetId) => datasetId != null && AllElements.Any(e => e.ContainsResultsOf(datasetId));
    }
}
