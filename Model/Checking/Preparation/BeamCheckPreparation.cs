using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Results;
using GPC.Model.Results.ResultLocations;
using GPC.Model.Sections.Concrete;

using GPC.Model.PostProcessing;

namespace GPC.Model.Checking
{
    /// <summary>Concrete section input validation, independent of engine creation and execution.</summary>
    public static class BeamCheckPreparation
    {
        public static BeamPreparation Prepare(Models.Model model, int beamId, StationResultBeamForces sample, string settings)
        {
            using (ValidationReadScope.Enter(model)) return PrepareCore(model, beamId, sample, settings);
        }
        private static BeamPreparation PrepareCore(Models.Model model, int beamId, StationResultBeamForces sample, string settings)
        {
            var b = model.BeamElements[beamId]; var errors = ValidationReadScope.Errors(model).ToList();
            if (errors.Count > 0) return new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.Insufficient, Diagnostics = errors };
            var compatibility = AnalysisCompatibilityValidator.KnownAnalysisDiagnostic(model, out var compatibilityStatus);
            if (compatibility != null) return new BeamPreparation { BeamId = beamId, Sample = sample, Status = compatibilityStatus, Diagnostics = new[] { compatibility } };
            var status = DataStatus.Insufficient;
            if (sample == null || !b.Results.SelectMany(r => r.Results).Any(r => ReferenceEquals(r, sample))) errors.Add(ModelDiagnostic.Error("MissingSample", b));
            else
            {
                var state = sample.State;
                if (state != null && !ResultAlgebra.HasCurrentDerivation(model, b, state)) { errors.Add(ModelDiagnostic.Error("StaleDerivedSources", b)); status = DataStatus.Stale; }
                if (state == null || string.IsNullOrEmpty(state.DatasetId) || !model.Datasets.TryGetValue(state.DatasetId, out var dataset)
                    || dataset.ModelRevision != state.ModelRevision || dataset.InputFingerprint != state.InputFingerprint || dataset.NormalizedUnits != "N,mm,rad")
                    errors.Add(ModelDiagnostic.Error("DatasetMismatchOrUnknownUnits", b));
                if (state == null || string.IsNullOrWhiteSpace(state.DatasetId) || string.IsNullOrWhiteSpace(state.ConcomitantStateId)
                    || sample.Case == null || state.Components == null || state.Components.Length != 6 || state.Components.Any(c => c != ComponentAvailability.Available))
                    errors.Add(ModelDiagnostic.Error("IncompleteState", b));
                if (state == null || (state.Semantics != AnalysisSemantics.LinearStatic && state.Semantics != AnalysisSemantics.NonlinearStatic && state.Semantics != AnalysisSemantics.ConcomitantEnvelopeState))
                    errors.Add(ModelDiagnostic.Error("NonConcomitantState", b));
                if (state != null && (state.IsCumulative == false || (!string.IsNullOrEmpty(state.Phase) && state.IsCumulative != true)))
                    errors.Add(ModelDiagnostic.Error("IncrementalOrUnknownPhaseState", b));
                if (state != null && state.InputFingerprint != model.AnalysisFingerprint()) { errors.Add(ModelDiagnostic.Error("StaleAnalysis", b)); status = DataStatus.Stale; }
                if (sample.Body != ActionBody.PositiveSectionFace) errors.Add(ModelDiagnostic.Error("UnresolvedSectionActionBody", b));
                try { new BeamReferenceGeometry(b).ValidateSample(sample); }
                catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) { errors.Add(ModelDiagnostic.Error("InvalidBeamReference", b, ex.Message)); }
                if (sample.ResultBeamForces == null || new[] { sample.ResultBeamForces.N, sample.ResultBeamForces.V1, sample.ResultBeamForces.V2, sample.ResultBeamForces.T, sample.ResultBeamForces.M1, sample.ResultBeamForces.M2 }.Any(v => double.IsNaN(v) || double.IsInfinity(v)))
                    errors.Add(ModelDiagnostic.Error("NonFiniteOrMissingForces", b));
            }
            ReinforcedConcreteSection section = null;
            try
            {
                if (sample != null) section = b.Assignments.SectionAt(new BeamReferenceGeometry(b).ConvertStation(sample.ParametricDistance, sample.StationDomain, b.Assignments.StationDomain ?? "NodeToNode"), sample.Side);
                if (section == null) errors.Add(ModelDiagnostic.Error("MissingReinforcementSection", b));
                else if (section.Rebars.Count() == 0) errors.Add(ModelDiagnostic.Error("MissingReinforcement", b));
                Axes.Validate(b.Assignments.SectionAxes);
            }
            catch (ArgumentException ex) { errors.Add(ModelDiagnostic.Error("InvalidSectionAxes", b, ex.Message)); }
            catch (InvalidOperationException ex) { errors.Add(ModelDiagnostic.Error("SectionAssignment", b, ex.Message)); }
            catch (NotSupportedException ex) { errors.Add(ModelDiagnostic.Error("SectionAssignment", b, ex.Message)); status = DataStatus.NotSupported; }
            if (errors.Count > 0) return new BeamPreparation { BeamId = beamId, Sample = sample, Status = status, Diagnostics = errors };
            // SectionAxes describes orientation. Keep the sample's point: this is a rotation, never moment transport.
            var orientation = b.Assignments.SectionGeometryAxes ?? b.Assignments.SectionAxes;
            try { Axes.Validate(sample.ResultBeamForces.CoordinateSystem); }
            catch (ArgumentException ex) { return new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.Insufficient, Diagnostics = new[] { ModelDiagnostic.Error("InvalidResultAxes", b, ex.Message) } }; }
            var target = new CoordinateSystem(sample.ResultBeamForces.CoordinateSystem.Origin, orientation.V1, orientation.V2, orientation.V3);
            if (!b.Assignments.ActionsAtSectionCentroidConfirmed && b.Assignments.SectionCentroidOffset == null)
                return new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.NotSupported, Diagnostics = new[] { ModelDiagnostic.Error("UnresolvedReductionPoint", b) } };
            ResultBeamForces preparedForces;
            try
            {
                var normalized = ResultOrientation.Beam(sample, ActionTransformations.AtPoint(b.Assignments.SectionAxes, sample.ResultBeamForces.CoordinateSystem.Origin));
                if (b.Assignments.SectionCentroidOffset != null) normalized = new BeamReferenceGeometry(b).AtCentroid(normalized);
                preparedForces = ResultOrientation.Beam(normalized, ActionTransformations.AtPoint(orientation, normalized.ResultBeamForces.CoordinateSystem.Origin)).ResultBeamForces;
            }
            catch (ArgumentException ex) { return new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.Insufficient, Diagnostics = new[] { ModelDiagnostic.Error("InvalidResultAxes", b, ex.Message) } }; }
            return new BeamPreparation
            {
                BeamId = beamId,
                Sample = sample,
                Status = DataStatus.Ready,
                Diagnostics = errors,
                Input = new BeamCheckInput
                {
                    Model = model,
                    SampleFingerprint = Persistence.ModelArchive.Fingerprint(new object[] { sample }),
                    PreparedForcesFingerprint = Persistence.ModelArchive.Fingerprint(new object[] { preparedForces }),
                    PreparedSectionFingerprint = Persistence.ModelArchive.Fingerprint(new object[] { section }),
                    BeamId = beamId,
                    Sample = sample,
                    Section = section,
                    Forces = preparedForces,
                    Settings = settings,
                    VerificationRevision = model.VerificationFingerprint(settings)
                }
            };
        }

        internal static bool IsCurrent(BeamCheckInput input)
        {
            using (ValidationReadScope.Enter(input.Model)) return IsCurrentCore(input);
        }
        private static bool IsCurrentCore(BeamCheckInput input) => input.VerificationRevision == input.Model.VerificationFingerprint(input.Settings)
            && AnalysisCompatibilityValidator.KnownAnalysisIsCompatible(input.Model)
            && ResultAlgebra.HasCurrentDerivation(input.Model, input.Model.BeamElements[input.BeamId], input.Sample.State)
            && input.PreparedSectionFingerprint == Persistence.ModelArchive.Fingerprint(new object[] { input.Section })
            && input.SampleFingerprint == Persistence.ModelArchive.Fingerprint(new object[] { input.Sample })
            && input.PreparedForcesFingerprint == Persistence.ModelArchive.Fingerprint(new object[] { input.Forces });
    }
}
