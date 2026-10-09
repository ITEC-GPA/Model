using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Sections.Concrete;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;
using GPC.Model.Checking.Reports;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;

namespace GPC.Model.Checking.Execution
{
    /// <summary>Executes an explicitly supplied section verifier on validated inputs; never selects a code or creates an engine.</summary>
    public static class SectionCheckExecution
    {
        public static CheckResult Run(BeamPreparation preparation, CheckMechanism mechanism, IConcreteSectionVerifier verifier, CancellationToken cancellationToken = default(CancellationToken))
            => Run(preparation, mechanism, null, verifier, cancellationToken);

        /// <summary>Task with explicit discriminators. Only an <see cref="ISectionCheckVerifier"/> that supports it is invoked.</summary>
        public static CheckResult Run(BeamPreparation preparation, SectionCheckSpecification check, IConcreteSectionVerifier verifier, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (check == null) throw new ArgumentNullException(nameof(check));
            check.Validate();
            return Run(preparation, check.Mechanism, check.Copy(), verifier, cancellationToken);
        }

        private static CheckResult Run(BeamPreparation preparation, CheckMechanism mechanism, SectionCheckSpecification check, IConcreteSectionVerifier verifier, CancellationToken cancellationToken)
        {
            var result = new CheckResult { Mechanism = mechanism, Check = check, Execution = ExecutionStatus.NotExecuted, Data = preparation.Status, Outcome = EngineeringOutcome.NotEvaluated };
            result.ElementId = preparation.BeamId; result.Case = preparation.Sample?.Case?.Name;
            result.Dataset = preparation.Sample?.State?.DatasetId; result.Station = preparation.Sample?.ParametricDistance;
            result.Side = preparation.Sample?.Side ?? SectionSide.Unspecified;
            result.Phase = preparation.Sample?.State?.Phase; result.Step = preparation.Sample?.State?.Step;
            result.Coverage = preparation.Sample?.State?.Coverage; result.ConcomitantStateId = preparation.Sample?.State?.ConcomitantStateId;
            result.MovingLoadPosition = preparation.Sample?.State?.MovingLoadPosition; result.Mode = preparation.Sample?.State?.Mode;
            if (cancellationToken.IsCancellationRequested) { result.Execution = ExecutionStatus.Cancelled; return result; }
            if (preparation.Input == null)
            {
                result.Diagnostics.AddRange(preparation.Diagnostics);
                foreach (var diagnostic in result.Diagnostics)
                { diagnostic.Station = preparation.Sample?.ParametricDistance; diagnostic.Dataset = result.Dataset; diagnostic.Case = result.Case; }
                return result;
            }
            var input = preparation.Input;
            if (input.Model.Analysis != null) result.Provenance = VerificationPreparation.CurrentProvenance(input.Model);
            if (!BeamCheckPreparation.IsCurrent(input))
            { result.Data = DataStatus.Stale; result.Diagnostics.Add(ModelDiagnostic.Error("StalePreparation")); return result; }
            result.ElementId = input.BeamId; result.Station = input.Sample.ParametricDistance; result.Side = input.Sample.Side;
            result.Case = input.Sample.Case.Name; result.Dataset = input.Sample.State.DatasetId; result.VerificationRevision = input.VerificationRevision;
            result.Settings = input.Settings; result.SampleRevision = input.SampleFingerprint;
            result.Phase = input.Sample.State.Phase; result.Step = input.Sample.State.Step; result.Coverage = input.Sample.State.Coverage;
            result.ConcomitantStateId = input.Sample.State.ConcomitantStateId;
            result.Input = new CheckInputSnapshot(input.PreparedSectionFingerprint, input.SampleFingerprint,
                input.PreparedForcesFingerprint, new BeamForceSnapshot(input.Forces));
            if (verifier == null) { result.Data = DataStatus.MissingDependency; result.Diagnostics.Add(ModelDiagnostic.Error("MissingDependency")); return result; }
            try
            {
                var configuration = (verifier as IConfiguredSectionVerifier)?.Configuration;
                result.EngineVersion = verifier.Version; result.EngineConfiguration = configuration;
                var specific = verifier as ISectionCheckVerifier;
                if (check == null ? !verifier.Capabilities.Contains(mechanism) : specific == null || !specific.Supports(check))
                {
                    result.Data = DataStatus.NotSupported;
                    result.Diagnostics.Add(ModelDiagnostic.Error(check == null ? "UnsupportedMechanism" : "UnsupportedSectionCheck", message: check?.Key)); return result;
                }
                var evaluated = check == null ? verifier.Verify(input, mechanism, cancellationToken) : specific.Verify(input, check.Copy(), cancellationToken);
                if (cancellationToken.IsCancellationRequested) throw new OperationCanceledException(cancellationToken);
                if (evaluated == null) throw new InvalidOperationException("Verifier returned no result.");
                if (!BeamCheckPreparation.IsCurrent(input) || configuration != (verifier as IConfiguredSectionVerifier)?.Configuration)
                { result.Data = DataStatus.Stale; result.Diagnostics.Add(ModelDiagnostic.Error("InputsChangedDuringVerification")); return result; }
                if (!CheckResultRules.ValidDecision(evaluated))
                    throw new InvalidOperationException("Invalid evaluated outcome or utilization returned by verifier.");
                evaluated.ElementId = result.ElementId; evaluated.Station = result.Station; evaluated.Side = result.Side; evaluated.Case = result.Case;
                evaluated.Dataset = result.Dataset; evaluated.VerificationRevision = result.VerificationRevision; evaluated.EngineVersion = verifier.Version; evaluated.Mechanism = mechanism;
                evaluated.Check = check;
                evaluated.EngineConfiguration = configuration; evaluated.Settings = input.Settings; evaluated.SampleRevision = result.SampleRevision;
                evaluated.Phase = result.Phase; evaluated.Step = result.Step; evaluated.Coverage = result.Coverage; evaluated.ConcomitantStateId = result.ConcomitantStateId;
                evaluated.MovingLoadPosition = result.MovingLoadPosition; evaluated.Mode = result.Mode; evaluated.Family = EntityFamily.Beam;
                evaluated.Input = result.Input;
                evaluated.Provenance = result.Provenance;
                evaluated.SealEvidence();
                return evaluated;
            }
            catch (OperationCanceledException) { result.Execution = ExecutionStatus.Cancelled; }
            catch (Exception ex) { result.Execution = ExecutionStatus.Error; result.Diagnostics.Add(ModelDiagnostic.Error("VerifierError", message: ex.Message)); }
            return result;
        }

    }
}
