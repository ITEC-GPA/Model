using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Core;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    /// <summary>Canonical actions/kinematics ready for consumption, independent of reinforcement or a design code.
    /// Ready here is not a structural check outcome. ModelPreparation remains the design-check gate.</summary>
    public sealed class ResultPreparation
    {
        public ElementKey Element { get; private set; }
        public ResultSelection Selection { get; private set; }
        public DataStatus Status { get; private set; } = DataStatus.Insufficient;
        public bool Cancelled { get; private set; }
        public ResultLocation LocalSample { get; private set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; private set; }
        private Models.Model model;
        private Elements.Element owner;
        private ResultLocation source;
        private string inputFingerprint, sourceFingerprint, preparedFingerprint, datasetFingerprint;
        public bool IsCurrent => HasCurrentSamples && model.AnalysisFingerprint() == inputFingerprint;
        private bool HasCurrentSamples => Status == DataStatus.Ready && LocalSample != null
            && ResultAlgebra.HasCurrentDerivation(model, owner, source.State)
            && owner.Results.SelectMany(r => r.Results).Any(r => ReferenceEquals(r, source))
            && ModelValues.Fingerprint(new object[] { source }) == sourceFingerprint
            && ModelValues.Fingerprint(new object[] { Element, Selection, LocalSample }) == preparedFingerprint
            && model.Datasets.TryGetValue(source.State.DatasetId, out var dataset)
            && ModelValues.Fingerprint(new object[] { dataset }) == datasetFingerprint;

        public static ResultPreparation Prepare(Models.Model model, ElementKey key, ResultLocation sample,
            CoordinateSystem targetOrientation = null, CancellationToken cancellationToken = default)
            => PrepareCore(model, key, sample, targetOrientation, cancellationToken, null, null);

        private static ResultPreparation PrepareCore(Models.Model model, ElementKey key, ResultLocation sample,
            CoordinateSystem targetOrientation, CancellationToken cancellationToken, string sharedInputFingerprint, Elements.Element resolvedElement)
        {
            if (model == null || key == null) throw new ArgumentNullException();
            var result = new ResultPreparation { model = model, source = sample, Element = new ElementKey { Family = key.Family, Id = key.Id } };
            var diagnostics = new List<ModelDiagnostic>(); result.Diagnostics = diagnostics.AsReadOnly();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var element = resolvedElement ?? model.AllElements.SingleOrDefault(e => e.Id == key.Id && Models.Model.FamilyOf(e) == key.Family);
                result.owner = element;
                if (element == null || sample == null || !element.Results.SelectMany(r => r.Results).Any(r => ReferenceEquals(r, sample)))
                    throw new ArgumentException("MissingOrForeignResultSample");
                var state = sample.State;
                if (state != null && !ResultAlgebra.HasCurrentDerivation(model, element, state))
                { result.Status = DataStatus.Stale; throw new ArgumentException("StaleDerivedSources"); }
                if (state == null || string.IsNullOrWhiteSpace(state.DatasetId) || !model.Datasets.TryGetValue(state.DatasetId, out var dataset)
                    || dataset.InputFingerprint != state.InputFingerprint || dataset.ModelRevision != state.ModelRevision
                    || dataset.Semantics != state.Semantics || dataset.NormalizedUnits != "N,mm,rad") throw new ArgumentException("DatasetMismatchOrUnknownUnits");
                result.Selection = new ResultSelection { Dataset = state.DatasetId, Case = sample.Case?.Name, Phase = state.Phase, Step = state.Step,
                    ConcomitantState = state.ConcomitantStateId, Mode = state.Mode, MovingLoadPosition = state.MovingLoadPosition };
                result.inputFingerprint = sharedInputFingerprint ?? model.AnalysisFingerprint();
                if (state.InputFingerprint != result.inputFingerprint)
                {
                    result.Status = DataStatus.Stale; throw new ArgumentException("StaleAnalysis");
                }
                if (sample.Case == null || string.IsNullOrWhiteSpace(state.ConcomitantStateId)) throw new ArgumentException("IncompleteResultState");
                if (state.Semantics != AnalysisSemantics.LinearStatic && state.Semantics != AnalysisSemantics.NonlinearStatic && state.Semantics != AnalysisSemantics.ConcomitantEnvelopeState)
                    throw new NotSupportedException("NonConcomitantOrNonPhysicalResultState");
                if (state.IsCumulative == false || state.Phase != null && state.IsCumulative != true) throw new NotSupportedException("IncrementalOrUnknownPhaseState");
                result.sourceFingerprint = ModelValues.Fingerprint(new object[] { sample });
                result.datasetFingerprint = ModelValues.Fingerprint(new object[] { dataset });
                if (element is NodeElement node)
                {
                    var target = ActionTransformations.AtPoint(targetOrientation ?? node.CoordinateSystem, node.Position);
                    if (sample is NodeResultForces f)
                    {
                        NodalActions.Validate(model, node, f);
                        result.LocalSample = ActionTransformations.RotateNode(f, target);
                    }
                    else if (sample is NodeResultDisplacement d) result.LocalSample = ActionTransformations.RotateNode(d, target);
                    else throw new NotSupportedException("UnsupportedNodalResult");
                }
                else if (element is BeamElement beam && sample is StationResultBeamForces b)
                {
                    var reference = new BeamReferenceGeometry(beam);
                    reference.ValidateSample(b);
                    if (b.Body != ActionBody.PositiveSectionFace || string.IsNullOrWhiteSpace(b.StationDomain)) throw new ArgumentException("UnresolvedSectionConvention");
                    if (b.ResultBeamForces == null) throw new ArgumentException("MissingBeamForces");
                    Axes.Validate(b.ResultBeamForces.CoordinateSystem);
                    var normalized = ResultOrientation.Beam(b, ActionTransformations.AtPoint(beam.Assignments.SectionAxes, b.ResultBeamForces.CoordinateSystem.Origin));
                    if (beam.Assignments.SectionCentroidOffset != null) normalized = reference.AtCentroid(normalized);
                    result.LocalSample = ResultOrientation.Beam(normalized, ActionTransformations.AtPoint(targetOrientation ?? beam.Assignments.SectionAxes, normalized.ResultBeamForces.CoordinateSystem.Origin));
                }
                else if (element is AreaElement plate && sample is PointResultPlateForces p)
                {
                    Axes.Validate(plate.CoordinateSystem);
                    if (p.Forces == null || p.Location == null || !Axes.IsFinite(new Point3d(p.Location.X, p.Location.Y, 0))) throw new ArgumentException("MissingShellResultLocation");
                    Axes.Validate(p.Forces.CoordinateSystem);
                    if (p.CoordinateKind != ResultCoordinateKind.LocalPhysical && p.CoordinateKind != ResultCoordinateKind.Natural) throw new NotSupportedException("UnsupportedShellCoordinates");
                    // A source-normal change must be explicit through ResultOrientation.Shell before attachment.
                    if (Axes.Dot(p.Forces.CoordinateSystem.V3, plate.CoordinateSystem.V3) < 1 - 1e-10) throw new ArgumentException("ShellNormalMismatch");
                    result.LocalSample = ResultOrientation.Shell(p, ActionTransformations.AtPoint(targetOrientation ?? plate.CoordinateSystem, p.Forces.CoordinateSystem.Origin));
                }
                else throw new NotSupportedException("UnsupportedElementResult");
                cancellationToken.ThrowIfCancellationRequested();
                result.preparedFingerprint = ModelValues.Fingerprint(new object[] { result.Element, result.Selection, result.LocalSample });
                result.Status = DataStatus.Ready;
                if (!(sharedInputFingerprint == null ? result.IsCurrent : result.HasCurrentSamples))
                { result.LocalSample = null; result.Status = DataStatus.Stale; throw new InvalidOperationException("ResultsChangedDuringPreparation"); }
            }
            catch (OperationCanceledException) { result.Cancelled = true; result.LocalSample = null; }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            {
                result.LocalSample = null;
                if (ex is NotSupportedException) result.Status = DataStatus.NotSupported;
                diagnostics.Add(new ModelDiagnostic { Code = "ResultPreparationRejected", Family = key.Family, ElementId = key.Id, Severity = DiagnosticSeverity.Error, Message = ex.Message });
            }
            return result;
        }

        /// <summary>Selects nodes/beam/plate by ID or groups. Missing data produce entries, never silently disappear.</summary>
        public static IReadOnlyList<ResultPreparation> Prepare(Models.Model model, ElementSelection elements,
            IReadOnlyList<ResultSelection> selections, CancellationToken cancellationToken = default)
        {
            if (elements == null || selections == null || selections.Count == 0) throw new ArgumentException("ExplicitResultSelectionsRequired");
            var states = selections.Select(s => s?.Copy() ?? throw new ArgumentException("MissingResultSelection")).ToArray();
            var selected = elements.Resolve(model); if (selected.Count == 0) throw new ArgumentException("EmptyResultSelection");
            var prepared = new List<ResultPreparation>();
            var inputFingerprint = model.AnalysisFingerprint();
            foreach (var element in selected)
            {
                var seen = new HashSet<ResultLocation>(ReferenceComparer<ResultLocation>.Instance);
                foreach (var selection in states)
                {
                    var samples = ResultQueries.Samples<ResultLocation>(element, selection);
                    if (samples.Count == 0)
                    {
                        var missing = PrepareCore(model, new ElementKey { Family = Models.Model.FamilyOf(element), Id = element.Id }, null, null, cancellationToken, inputFingerprint, element);
                        missing.Selection = selection; prepared.Add(missing);
                    }
                    foreach (var sample in samples)
                        if (seen.Add(sample)) prepared.Add(PrepareCore(model, new ElementKey { Family = Models.Model.FamilyOf(element), Id = element.Id }, sample, null, cancellationToken, inputFingerprint, element));
                }
            }
            bool unchanged = inputFingerprint == model.AnalysisFingerprint();
            foreach (var item in prepared.Where(p => p.Status == DataStatus.Ready))
                if (!unchanged || !item.HasCurrentSamples)
                {
                    item.Status = DataStatus.Stale; item.LocalSample = null;
                    item.Diagnostics = new[] { new ModelDiagnostic { Code = "ResultsChangedDuringPreparation", Severity = DiagnosticSeverity.Error } };
                }
            return prepared.AsReadOnly();
        }
    }
}
