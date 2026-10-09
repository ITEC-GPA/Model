using System;
using System.Collections.Generic;
using GPC.Model.PostProcessing;

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
    }
}
