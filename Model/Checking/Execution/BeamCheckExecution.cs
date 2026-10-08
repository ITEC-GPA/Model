using GPC.Model.Checking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Model.PostProcessing;

namespace GPC.Model.Checking
{
    public static class BeamCheckExecution
    {
        /// <summary>Sequential reference path. Every supplied station/state is checked before enveloping outcomes.</summary>
        public static CheckReport Beam(Models.Model model, int beamId, string dataset, string caseName, string settings,
            CheckMechanism[] mechanisms, IConcreteSectionVerifier verifier, CancellationToken cancellationToken = default(CancellationToken), IProgress<int> progress = null)
        {
            var samples = ResultQueries.BeamSamples(model.BeamElements[beamId], dataset, caseName);
            var report = new CheckReport { Required = Math.Max(1, samples.Count) * mechanisms.Length };
            if (samples.Count == 0)
            {
                foreach (var mechanism in mechanisms) report.Results.Add(new CheckResult
                {
                    Mechanism = mechanism,
                    ElementId = beamId,
                    Case = caseName,
                    Dataset = dataset,
                    Data = DataStatus.Insufficient,
                    Diagnostics = new List<ModelDiagnostic> { ModelDiagnostic.Error("MissingBeamSamples", model.BeamElements[beamId]) }
                });
                report.SealScope(); return report;
            }
            foreach (var sample in samples)
            {
                var preparation = cancellationToken.IsCancellationRequested ? new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.Insufficient, Diagnostics = new ModelDiagnostic[0] }
                    : BeamCheckPreparation.Prepare(model, beamId, sample, settings);
                foreach (var mechanism in mechanisms) report.Results.Add(SectionCheckExecution.Run(preparation, mechanism, verifier, cancellationToken));
                progress?.Report(report.Results.Count);
            }
            report.SealScope(); return report;
        }
    }
}
