using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.Results;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    /// <summary>Material-neutral actions. Units N, Nmm, mm; axes belong to the physical section.</summary>
    public sealed class BeamActionInput
    {
        public Models.Model Model { get; internal set; }
        public BeamElement Element { get; internal set; }
        public StationResultBeamForces Sample { get; internal set; }
        public ResultBeamForces Forces { get; internal set; }
        /// <summary>The property resolved at this cut, from an assignment or the element's constant property.</summary>
        public ElementProperties.BeamProperty Property { get; internal set; }
        public string Settings { get; internal set; }
        public CheckInputSnapshot Snapshot { get; internal set; }
        internal string Revision;
        internal string DatasetFingerprint;
        internal string PreparedPropertyFingerprint;
        public bool IsCurrent { get { using (Checking.ValidationReadScope.Enter(Model)) return IsCurrentCore(); } }
        private bool IsCurrentCore() => Sample?.State != null && Model.BeamElements.TryGetValue(Element.Id, out var beam) && ReferenceEquals(beam, Element)
            && AnalysisCompatibilityValidator.KnownAnalysisIsCompatible(Model)
            && beam.Results.SelectMany(r => r.Results).Any(s => ReferenceEquals(s, Sample))
            && Model.Datasets.TryGetValue(Sample.State.DatasetId, out var dataset) && DatasetFingerprint == ModelArchive.Fingerprint(new object[] { dataset })
            && Revision == Model.VerificationFingerprint(Settings)
            && Snapshot.SampleFingerprint == ModelArchive.Fingerprint(new object[] { Sample })
            && Snapshot.ForcesFingerprint == ModelArchive.Fingerprint(new object[] { Forces })
            && PreparedPropertyFingerprint == ModelArchive.Fingerprint(new object[] { Property })
            && Snapshot.SectionFingerprint == ModelArchive.Fingerprint(new object[] { Element.BeamProperty, Element.Assignments.Sections.ToArray() })
            && ResultAlgebra.HasCurrentDerivation(Model, Element, Sample.State);
    }
    public sealed class BeamActionPreparation
    {
        public BeamActionInput Input { get; private set; }
        public DataStatus Status { get; private set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; private set; }
        public static BeamActionPreparation Prepare(Models.Model model, int id, StationResultBeamForces sample, string settings)
        {
            using (Checking.ValidationReadScope.Enter(model)) return PrepareCore(model, id, sample, settings);
        }
        private static BeamActionPreparation PrepareCore(Models.Model model, int id, StationResultBeamForces sample, string settings)
        {
            var beam = model.BeamElements[id]; var diagnostics = new List<ModelDiagnostic>();
            var result = new BeamActionPreparation { Status = DataStatus.Insufficient, Diagnostics = diagnostics.AsReadOnly() };
            diagnostics.AddRange(Checking.ValidationReadScope.Errors(model));
            if (diagnostics.Count > 0) return result;
            var compatibility = AnalysisCompatibilityValidator.KnownAnalysisDiagnostic(model, out var compatibilityStatus);
            if (compatibility != null) { diagnostics.Add(compatibility); result.Status = compatibilityStatus; return result; }
            if (sample == null || !beam.Results.SelectMany(r => r.Results).Any(s => ReferenceEquals(s, sample)))
            { diagnostics.Add(ModelDiagnostic.Error("MissingSample", beam)); return result; }
            var state = sample.State;
            if (state == null || string.IsNullOrWhiteSpace(state.DatasetId) || !model.Datasets.TryGetValue(state.DatasetId, out var dataset)
                || dataset == null || dataset.ModelRevision != state.ModelRevision || dataset.InputFingerprint != state.InputFingerprint || dataset.NormalizedUnits != "N,mm,rad")
                diagnostics.Add(ModelDiagnostic.Error("DatasetMismatchOrUnknownUnits", beam));
            if (state == null || string.IsNullOrWhiteSpace(state.ConcomitantStateId) || sample.Case == null || state.Components == null
                || state.Components.Length != 6 || state.Components.Any(v => v != ComponentAvailability.Available)) diagnostics.Add(ModelDiagnostic.Error("IncompleteState", beam));
            if (state == null || (state.Semantics != AnalysisSemantics.LinearStatic && state.Semantics != AnalysisSemantics.NonlinearStatic && state.Semantics != AnalysisSemantics.ConcomitantEnvelopeState))
                diagnostics.Add(ModelDiagnostic.Error("NonConcomitantState", beam));
            if (state != null && (state.IsCumulative == false || (!string.IsNullOrEmpty(state.Phase) && state.IsCumulative != true))) diagnostics.Add(ModelDiagnostic.Error("IncrementalOrUnknownPhaseState", beam));
            if (state != null && (state.InputFingerprint != model.AnalysisFingerprint() || !ResultAlgebra.HasCurrentDerivation(model, beam, state)))
            { diagnostics.Add(ModelDiagnostic.Error("StaleAnalysis", beam)); result.Status = DataStatus.Stale; }
            if (sample.Body != ActionBody.PositiveSectionFace) diagnostics.Add(ModelDiagnostic.Error("UnresolvedSectionActionBody", beam));
            try
            {
                if (beam.Assignments.Formulation != BeamFormulation.StraightTwoNode) throw new NotSupportedException("StraightTwoNodeRequired");
                new BeamReferenceGeometry(beam).ValidateSample(sample);
                Axes.Validate(beam.Assignments.SectionAxes);
                var frame = beam.Assignments.SectionGeometryAxes ?? beam.Assignments.SectionAxes; Axes.Validate(frame);
                if (sample.ResultBeamForces == null) throw new ArgumentException("MissingForces");
                var raw = sample.ResultBeamForces;
                if (new[] { raw.N, raw.V1, raw.V2, raw.T, raw.M1, raw.M2 }.Any(v => double.IsNaN(v) || double.IsInfinity(v))) throw new ArgumentException("NonFiniteForces");
                Axes.Validate(raw.CoordinateSystem);
                if (!beam.Assignments.ActionsAtSectionCentroidConfirmed && beam.Assignments.SectionCentroidOffset == null) throw new NotSupportedException("UnresolvedReductionPoint");
                if (diagnostics.Count > 0) return result;
                var normalized = ResultOrientation.Beam(sample, ActionTransformations.AtPoint(beam.Assignments.SectionAxes, raw.CoordinateSystem.Origin));
                if (beam.Assignments.SectionCentroidOffset != null) normalized = new BeamReferenceGeometry(beam).AtCentroid(normalized);
                var forces = ResultOrientation.Beam(normalized, ActionTransformations.AtPoint(frame, normalized.ResultBeamForces.CoordinateSystem.Origin)).ResultBeamForces;
                var property = beam.Assignments.Sections.Count == 0 ? beam.BeamProperty : beam.Assignments.PropertyAt(
                    new BeamReferenceGeometry(beam).ConvertStation(sample.ParametricDistance, sample.StationDomain, beam.Assignments.StationDomain ?? "NodeToNode"), sample.Side);
                result.Input = new BeamActionInput { Model = model, Element = beam, Sample = sample, Forces = forces, Property = property, Settings = settings,
                    PreparedPropertyFingerprint = ModelArchive.Fingerprint(new object[] { property }),
                    DatasetFingerprint = ModelArchive.Fingerprint(new object[] { model.Datasets[state.DatasetId] }),
                    Revision = model.VerificationFingerprint(settings), Snapshot = new CheckInputSnapshot(
                        ModelArchive.Fingerprint(new object[] { beam.BeamProperty, beam.Assignments.Sections.ToArray() }),
                        ModelArchive.Fingerprint(new object[] { sample }), ModelArchive.Fingerprint(new object[] { forces }), new BeamForceSnapshot(forces)) };
                result.Status = DataStatus.Ready;
            }
            catch (NotSupportedException ex) { if (result.Status != DataStatus.Stale) result.Status = DataStatus.NotSupported; diagnostics.Add(ModelDiagnostic.Error("UnsupportedBeamActions", beam, ex.Message)); }
            catch (ArgumentException ex) { diagnostics.Add(ModelDiagnostic.Error("InvalidBeamActions", beam, ex.Message)); }
            catch (InvalidOperationException ex) { diagnostics.Add(ModelDiagnostic.Error("SectionAssignment", beam, ex.Message)); }
            return result;
        }
    }

    /// <summary>Persistable evidence from a named native method, with its precise limited coverage.</summary>
    [Serializable]
    public sealed class NativeMethodDetails : CheckDetails
    {
        public string ScopeDescription { get; private set; }
        public string NativeInputFingerprint { get; private set; }
        private readonly string[] _warnings;
        public IReadOnlyList<string> Warnings => Array.AsReadOnly(_warnings);
        public NativeMethodDetails(string method, string scope, string fingerprint, IEnumerable<CheckMetric> metrics,
            IEnumerable<CheckCalculationValue> trace = null, IEnumerable<string> warnings = null,
            IEnumerable<CheckPointValue> points = null, CheckConvergence convergence = null)
            : base(method, method + ":normalized-limit-1", metrics, trace, points, convergence)
        { ScopeDescription = scope; NativeInputFingerprint = fingerprint; _warnings = (warnings ?? Enumerable.Empty<string>()).ToArray(); }
    }
}
