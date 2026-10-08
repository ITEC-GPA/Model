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
    public sealed class PreparedVerification
    {
        /// <summary>Independent working graph. Mutations invalidate IsCurrent; the analysed graph is never exposed.</summary>
        public Models.Model Model { get; }
        public VerificationProvenance Provenance { get; }
        public AnalysisCompatibilityResult Compatibility { get; }
        private readonly string _results;
        public bool IsCurrent => Provenance.IsCurrent(Model) && _results == AnalysisStorage.Results(Model);
        internal PreparedVerification(Models.Model model, VerificationScenario scenario)
        {
            Model = model; Provenance = VerificationProvenance.Capture(model, scenario); model.VerificationContext = Provenance;
            Compatibility = AnalysisCompatibilityValidator.Validate(model); _results = AnalysisStorage.Results(model);
        }
    }
    public static class VerificationPreparation
    {
        /// <summary>The source supplies the samples, not just AnalysisDataset metadata. No result is rebound to the edited design.</summary>
        public static PreparedVerification Prepare(Models.Model source, VerificationScenario scenario)
        {
            if (source == null || scenario == null) throw new ArgumentNullException();
            if (source.Analysis == null || source.Analysis.Id != scenario.AnalysisId) throw new ArgumentException("ScenarioAnalysisMismatch");
            source.Analysis.OpenModel(); // Validate persisted snapshot before using its declaration.
            var model = AnalysisStorage.Read<Models.Model>(AnalysisStorage.Write(source.AnalysisArchiveView()));
            model.AttachAnalysis(source.Analysis);
            var frozenScenario = AnalysisStorage.Read<VerificationScenario>(AnalysisStorage.Write(scenario));
            model.VerificationScenarios.Add(frozenScenario.Id, frozenScenario);
            frozenScenario.Apply(model);
            return new PreparedVerification(model, frozenScenario);
        }
        public static VerificationProvenance CurrentProvenance(Models.Model model) => model.VerificationContext ?? VerificationProvenance.Capture(model);
    }
}
