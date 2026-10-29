using GPC.Model.Models;
using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Model.Elements;
using GPC.Model.Core;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Analysis;
using GPC.Model.Checking.Contracts;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Results.Queries;
using GPC.Model.Results.State;
using GPC.Model.Structure.Members;

namespace GPC.Model.Checking.Preparation
{
    /// <summary>Model source of one member-node displacement of a deformation task. Like BeamActionInput it is current while the node,
    /// the registered sample, its dataset and the analysis are the ones read at preparation.</summary>
    internal sealed class MemberDisplacementSource
    {
        internal Models.Model Model;
        internal NodeElement Node;
        internal NodeResultDisplacement Sample;
        internal string DatasetFingerprint;
        internal string SampleFingerprint;
        internal bool IsCurrent
        {
            get
            {
                using (ValidationReadScope.Enter(Model))
                    return Sample?.State != null && Model.NodesElements.TryGetValue(Node.Id, out var node) && ReferenceEquals(node, Node)
                        && AnalysisCompatibilityValidator.KnownAnalysisIsCompatible(Model) && ValidationReadScope.IsRegistered(Model, node, Sample)
                        && Model.Datasets.TryGetValue(Sample.State.DatasetId, out var dataset) && DatasetFingerprint == ValidationReadScope.FingerprintOf(Model, dataset)
                        && SampleFingerprint == ValidationReadScope.FingerprintOf(Model, Sample) && Sample.State.InputFingerprint == Model.AnalysisFingerprint()
                        && ResultAlgebra.HasCurrentDerivation(Model, node, Sample.State);
            }
        }
    }

    /// <summary>Member-node displacements of a deformation task, in the member axes, for the task state and the declared variable
    /// selection. Only exported nodal samples are used: nodes are the points of the member path, nothing is interpolated.</summary>
    internal sealed class MemberDeformationPreparation
    {
        private const double Tolerance = 1e-7;
        internal MemberInputSnapshot Snapshot;
        internal IReadOnlyList<MemberDisplacementSource> Sources;
        internal IReadOnlyList<ModelDiagnostic> Diagnostics;
        internal DataStatus Data;
        internal int Required, Available;

        /// <summary>The distinct nodes of the path in member order, with their location; a node outside the member station domain
        /// (beyond a rigid offset of an OffsetToOffset path) is not a point of the member and is left out.</summary>
        internal static IReadOnlyList<(NodeElement Node, MemberLocation Location)> Nodes(Models.Model model, PhysicalMemberGeometry geometry)
        {
            var nodes = new List<(NodeElement, MemberLocation)>(); var seen = new HashSet<int>();
            foreach (var part in geometry.Definition.Parts)
            {
                var beam = model.BeamElements[part.BeamId];
                foreach (var end in part.AlongIJ ? new[] { (beam.NodeI, 0d), (beam.NodeJ, 1d) } : new[] { (beam.NodeJ, 1d), (beam.NodeI, 0d) })
                {
                    if (!seen.Add(end.Item1.Id)) continue;
                    try { nodes.Add((end.Item1, geometry.FromElement(beam.Id, end.Item2, "NodeToNode", SectionSide.Unspecified))); }
                    catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) { }
                }
            }
            return nodes.AsReadOnly();
        }

        /// <summary>Scope inputs of the deformation tasks: the nodal samples of every member with such a task, for every selection of the
        /// request and every declared variable selection. Empty without deformation tasks, so historical scope digests are unchanged.</summary>
        internal static IEnumerable<object> ScopeInputs(Models.Model model, IReadOnlyDictionary<string, PhysicalMemberGeometry> members, BeamCheckPlanRequest r)
        {
            var specs = r.MemberChecks.Where(s => s.Deformation != null).ToArray();
            if (specs.Length == 0) yield break;
            yield return "MemberDeformations-v1";
            var selections = r.Results.Concat(specs.Select(s => s.Deformation.VariableSelection).Where(s => s != null))
                .Select(s => new { Key = ModelValues.Fingerprint(new object[] { s }), Selection = s })
                .GroupBy(s => s.Key, StringComparer.Ordinal).OrderBy(g => g.Key, StringComparer.Ordinal).Select(g => g.First().Selection).ToArray();
            foreach (var id in specs.Select(s => s.MemberId).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal))
                foreach (var (node, _) in Nodes(model, members[id]))
                {
                    yield return node.Id;
                    foreach (var selection in selections) yield return ResultQueries.Samples<NodeResultDisplacement>(node, selection).ToArray();
                }
        }

        internal static MemberDeformationPreparation Prepare(Models.Model model, PhysicalMemberGeometry geometry, MemberCheckSpecification spec,
            ResultSelection selection, CancellationToken token)
        {
            var result = new MemberDeformationPreparation(); var errors = new List<ModelDiagnostic>(); var sources = new List<MemberDisplacementSource>();
            double end = spec.End ?? geometry.Length; bool stale = false;
            // The model-level gates of BeamActionPreparation: topology/assignment errors and a known analysis that is not current.
            errors.AddRange(ValidationReadScope.Errors(model));
            var compatibility = AnalysisCompatibilityValidator.KnownAnalysisDiagnostic(model, out var compatibilityStatus);
            if (compatibility != null) { errors.Add(compatibility); stale |= compatibilityStatus == DataStatus.Stale; }
            bool blocked = errors.Count != 0;
            var nodes = Nodes(model, geometry).Where(n => n.Location.Distance >= spec.Start - Tolerance && n.Location.Distance <= end + Tolerance).ToArray();
            var deformation = spec.Deformation;
            List<(MemberDisplacementSample Sample, ResultState State)> Collect(ResultSelection state, string role)
            {
                var collected = new List<(MemberDisplacementSample, ResultState)>();
                foreach (var (node, location) in nodes)
                {
                    var samples = ResultQueries.Samples<NodeResultDisplacement>(node, state);
                    result.Required++;
                    if (samples.Count == 0) { errors.Add(ModelDiagnostic.Error("MissingMemberNodeDisplacement", node, role)); continue; }
                    if (samples.Count > 1) { errors.Add(ModelDiagnostic.Error("AmbiguousMemberNodeDisplacement", node, role)); continue; }
                    result.Available++;
                    if (token.IsCancellationRequested || blocked) continue;
                    var prepared = PrepareSample(model, geometry, node, location, samples[0], errors, ref stale);
                    if (prepared.HasValue) { collected.Add((prepared.Value.Sample, samples[0].State)); sources.Add(prepared.Value.Source); }
                }
                if (!token.IsCancellationRequested && !blocked)
                {
                    foreach (var boundary in new[] { spec.Start, end })
                        if (!collected.Any(s => Math.Abs(s.Item1.Location.Distance - boundary) <= Tolerance))
                            errors.Add(ModelDiagnostic.Error("MissingMemberDisplacementBoundary", message: role));
                    if (collected.Select(s => ModelValues.Fingerprint(new object[] { s.Item1.State })).Distinct(StringComparer.Ordinal).Count() > 1)
                        errors.Add(ModelDiagnostic.Error("IncoherentMemberStates", message: role));
                }
                return collected;
            }
            var total = Collect(selection, "Total");
            var variable = deformation.VariableBasis == MemberVariableDeflectionBasis.None ? new List<(MemberDisplacementSample, ResultState)>()
                : Collect(deformation.VariableSelection, deformation.VariableBasis.ToString());
            if (!spec.Context.GlobalStateConfirmed || string.IsNullOrWhiteSpace(selection.ConcomitantState)) errors.Add(ModelDiagnostic.Error("GlobalMemberStateNotConfirmed"));
            if (deformation.VariableBasis != MemberVariableDeflectionBasis.None)
            {
                if (string.IsNullOrWhiteSpace(deformation.VariableSelection.ConcomitantState)) errors.Add(ModelDiagnostic.Error("VariableMemberStateNotConfirmed"));
                // The deducted or variable state must be another state than the task state.
                if (deformation.VariableSelection.Dataset == selection.Dataset && deformation.VariableSelection.Case == selection.Case
                    && deformation.VariableSelection.Phase == selection.Phase && deformation.VariableSelection.Step == selection.Step
                    && deformation.VariableSelection.ConcomitantState == selection.ConcomitantState && deformation.VariableSelection.Mode == selection.Mode
                    && deformation.VariableSelection.MovingLoadPosition == selection.MovingLoadPosition)
                    errors.Add(ModelDiagnostic.Error("VariableSelectionIsTheTaskState"));
                // A difference of two states is the deflection of the variable actions only by linear superposition.
                if (deformation.VariableBasis == MemberVariableDeflectionBasis.Difference && total.Concat(variable).Any(s => s.Item2.Semantics != AnalysisSemantics.LinearStatic))
                    errors.Add(ModelDiagnostic.Error("VariableDifferenceRequiresLinearStates"));
            }
            result.Snapshot = new MemberInputSnapshot(geometry, spec, new MemberDeformationSnapshot(deformation, spec.Category.Value,
                total.Select(s => s.Item1), variable.Select(s => s.Item1)));
            result.Sources = sources.AsReadOnly();
            result.Diagnostics = errors.AsReadOnly();
            result.Data = stale ? DataStatus.Stale : errors.Count == 0 ? DataStatus.Ready : DataStatus.Insufficient;
            return result;
        }

        private static (MemberDisplacementSample Sample, MemberDisplacementSource Source)? PrepareSample(Models.Model model, PhysicalMemberGeometry geometry,
            NodeElement node, MemberLocation location, NodeResultDisplacement sample, List<ModelDiagnostic> errors, ref bool stale)
        {
            int count = errors.Count;
            var state = sample.State;
            AnalysisDataset dataset = null;
            if (state == null || string.IsNullOrWhiteSpace(state.DatasetId) || !model.Datasets.TryGetValue(state.DatasetId, out dataset) || dataset == null
                || dataset.ModelRevision != state.ModelRevision || dataset.InputFingerprint != state.InputFingerprint || dataset.NormalizedUnits != "N,mm,rad")
                errors.Add(ModelDiagnostic.Error("DatasetMismatchOrUnknownUnits", node));
            if (state == null || string.IsNullOrWhiteSpace(state.ConcomitantStateId) || sample.Case == null || state.Components == null || state.Components.Length != 6
                || state.Components.Any(v => v != ComponentAvailability.Available))
                errors.Add(ModelDiagnostic.Error("IncompleteState", node));
            if (state == null || state.Semantics != AnalysisSemantics.LinearStatic && state.Semantics != AnalysisSemantics.NonlinearStatic && state.Semantics != AnalysisSemantics.ConcomitantEnvelopeState)
                errors.Add(ModelDiagnostic.Error("NonConcomitantState", node));
            if (state != null && (state.IsCumulative == false || !string.IsNullOrEmpty(state.Phase) && state.IsCumulative != true))
                errors.Add(ModelDiagnostic.Error("IncrementalOrUnknownPhaseState", node));
            if (state != null && (state.InputFingerprint != model.AnalysisFingerprint() || !ResultAlgebra.HasCurrentDerivation(model, node, state)))
            { errors.Add(ModelDiagnostic.Error("StaleAnalysis", node)); stale = true; }
            if (errors.Count != count) return null;
            try
            {
                var d = sample.ResultDisplacement ?? throw new ArgumentException("MissingDisplacements");
                Axes.Validate(d.CoordinateSystem);
                // Displacements are referred to the node, as the converter writes them (no transport of a rotation lever arm).
                if (Axes.Length(d.CoordinateSystem.Origin - node.Position) > 1e-6) throw new ArgumentException("NodalDisplacementNotAtNode");
                var rotated = ActionTransformations.RotateNode(sample, ActionTransformations.AtPoint(geometry.Axes, d.CoordinateSystem.Origin)).ResultDisplacement;
                string fingerprint = ValidationReadScope.FingerprintOf(model, sample);
                var values = new[] { rotated.D1, rotated.D2, rotated.D3, rotated.R1, rotated.R2, rotated.R3 };
                var selection = new ResultSelection { Dataset = state.DatasetId, Case = sample.Case.Name, Phase = state.Phase, Step = state.Step,
                    ConcomitantState = state.ConcomitantStateId, Mode = state.Mode, MovingLoadPosition = state.MovingLoadPosition };
                return (new MemberDisplacementSample(location, node.Id, selection, values, fingerprint),
                    new MemberDisplacementSource { Model = model, Node = node, Sample = sample, SampleFingerprint = fingerprint,
                        DatasetFingerprint = ValidationReadScope.FingerprintOf(model, dataset) });
            }
            catch (NotSupportedException ex) { errors.Add(ModelDiagnostic.Error("UnsupportedNodalDisplacement", node, ex.Message)); }
            catch (ArgumentException ex) { errors.Add(ModelDiagnostic.Error("InvalidNodalDisplacement", node, ex.Message)); }
            return null;
        }
    }
}
