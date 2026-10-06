using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Combinations;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.Results.ElementResults;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    /// <summary>One linear combination of static load cases: the only alternative of a linear combination, or one alternative of an envelope.</summary>
    public sealed class CombinationAlternative
    {
        public CombinationAlternative(string label, IReadOnlyDictionary<string, double> factors)
        {
            Label = label ?? ""; Factors = factors ?? throw new ArgumentNullException(nameof(factors));
        }
        /// <summary>The choices made in the envelopes (e.g. "ENV_Q=Q1"); empty for a linear combination.</summary>
        public string Label { get; }
        /// <summary>Factor of each static load case, by Model load case name.</summary>
        public IReadOnlyDictionary<string, double> Factors { get; }
    }

    public sealed class CombinationRebuildReport
    {
        public string Combination { get; internal set; }
        public int Alternatives { get; internal set; }
        /// <summary>Result locations combined (beam stations, plate points, nodal quantities).</summary>
        public int Locations { get; internal set; }
        /// <summary>The combined states: one per location for a linear combination, the governing alternatives of each location for an envelope.</summary>
        public IReadOnlyList<ResultLocation> Samples { get; internal set; } = new ResultLocation[0];
        public bool Attached { get; internal set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; internal set; } = new ModelDiagnostic[0];
        public bool IsRejected => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
    }

    /// <summary>Rebuilds a combination from the linear static results of one dataset, for many elements at once. At each result location the
    /// static samples are rotated to one frame and every alternative is the sum of the factored static vectors, as in
    /// <see cref="ResultAlgebra.LinearCombination"/>. A linear combination gives one state per location. An envelope gives, for each component,
    /// the alternatives with the minimum and the maximum: each governing alternative is registered once, as a complete concomitant state
    /// (<see cref="AnalysisSemantics.ConcomitantEnvelopeState"/>) derived from its static samples, never as a vector of independent extremes.
    /// The output combination must be a Model combination declared before the static results were imported. Elements whose static results
    /// are incomplete are skipped with a warning; nothing is registered when the call is rejected.</summary>
    public static class CombinationRebuild
    {
        public static CombinationRebuildReport Rebuild(Models.Model model, Combination output, IReadOnlyList<CombinationAlternative> alternatives, string datasetId,
            IEnumerable<Element> elements, bool attach = true, CancellationToken cancellationToken = default)
        {
            var report = new CombinationRebuildReport { Combination = output?.Name }; var diagnostics = new List<ModelDiagnostic>(); report.Diagnostics = diagnostics;
            try
            {
                if (model == null || output == null || alternatives == null || elements == null || string.IsNullOrWhiteSpace(datasetId)) throw new ArgumentNullException();
                if (!model.Combinations.Values.Any(c => ReferenceEquals(c, output))) throw new InvalidOperationException("DeclareOutputCaseBeforeAnalysis: " + output.Name);
                if (alternatives.Count == 0 || alternatives.Any(a => a == null)) throw new ArgumentException("EmptyCombination: " + output.Name);
                bool envelope = alternatives.Count > 1; report.Alternatives = alternatives.Count;
                var labels = alternatives.Select((a, i) => string.IsNullOrEmpty(a.Label) ? "#" + (i + 1).ToString(CultureInfo.InvariantCulture) : a.Label).ToArray();
                if (envelope && labels.Distinct(StringComparer.Ordinal).Count() != labels.Length) throw new ArgumentException("DuplicateAlternativeLabel: " + output.Name);
                var cases = alternatives.SelectMany(a => a.Factors.Where(p => p.Value != 0).Select(p => p.Key)).Distinct(StringComparer.Ordinal).ToArray();
                if (cases.Length == 0) throw new ArgumentException("EmptyCombination: " + output.Name);
                foreach (var name in cases) if (!model.LoadCases.ContainsKey(name)) throw new ArgumentException("UnknownStaticCase: " + name);
                var index = cases.Select((c, i) => (c, i)).ToDictionary(p => p.c, p => p.i, StringComparer.Ordinal);
                var terms = alternatives.Select(a => a.Factors.Where(p => p.Value != 0).Select(p => (Case: index[p.Key], Factor: NumericGuard.Finite(p.Value, "combination factor")))
                    .OrderBy(t => t.Case).ToArray()).ToArray();
                if (!model.Datasets.TryGetValue(datasetId, out var dataset)) throw new ArgumentException("UnknownDataset: " + datasetId);
                string input = model.AnalysisFingerprint();
                if (dataset.InputFingerprint != input || dataset.NormalizedUnits != "N,mm,rad" || dataset.Semantics != AnalysisSemantics.LinearStatic)
                    throw new InvalidOperationException("DatasetMismatchOrStaleAnalysis: " + datasetId);

                var registered = new HashSet<Element>(model.AllElements, ReferenceComparer<Element>.Instance);
                var created = new List<(Element Owner, List<ResultLocation> Samples)>(); var all = new List<ResultLocation>();
                foreach (var owner in elements.Distinct(ReferenceComparer<Element>.Instance))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (owner == null || !registered.Contains(owner)) throw new ArgumentException("UnregisteredElement");
                    try
                    {
                        var samples = Element(model, owner, output, cases, terms, labels, envelope, datasetId, dataset, out int locations, cancellationToken);
                        if (samples.Count == 0) continue;
                        created.Add((owner, samples)); all.AddRange(samples); report.Locations += locations;
                    }
                    catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException)
                    {
                        diagnostics.Add(new ModelDiagnostic { Code = "CombinationElementSkipped", Severity = DiagnosticSeverity.Warning, Family = Models.Model.FamilyOf(owner),
                            ElementId = owner.Id, Message = output.Name + ": " + ex.Message });
                    }
                }
                if (attach)
                {
                    if (model.AnalysisFingerprint() != input) throw new InvalidOperationException("ModelChangedDuringRebuild");
                    foreach (var (owner, samples) in created)
                    {
                        if (owner is BeamElement) owner.AddResult(new BeamResult(samples.Cast<IBeamResultLocation>().ToArray()));
                        else if (owner is NodeElement) owner.AddResult(new NodeResult(samples.Cast<INodeResultLocation>().ToList()));
                        else owner.AddResult(new PlateElementResult(samples.Cast<IPlateResultLocation>().ToList()));
                    }
                    report.Attached = true;
                }
                report.Samples = all.AsReadOnly();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            { diagnostics.Add(ModelDiagnostic.Error("CombinationRejected", message: ex.Message)); report.Samples = new ResultLocation[0]; report.Locations = 0; }
            return report;
        }

        private static List<ResultLocation> Element(Models.Model model, Element owner, Combination output, string[] cases, (int Case, double Factor)[][] terms,
            string[] labels, bool envelope, string datasetId, AnalysisDataset dataset, out int locations, CancellationToken token)
        {
            locations = 0;
            var byCase = cases.Select(_ => new List<ResultLocation>()).ToArray(); var index = cases.Select((c, i) => (c, i)).ToDictionary(p => p.c, p => p.i, StringComparer.Ordinal);
            var existing = new List<ResultLocation>();
            foreach (var sample in owner.Results.SelectMany(r => r.Results))
            {
                if (sample?.State?.DatasetId != datasetId || sample.Case == null) continue;
                if (sample.Case.Name == output.Name) { existing.Add(sample); continue; }
                if (sample.Case is Combination || sample.State.DerivedFrom != null || !index.TryGetValue(sample.Case.Name, out var c)) continue;
                byCase[c].Add(sample);
            }
            var result = new List<ResultLocation>();
            if (byCase.All(l => l.Count == 0)) return result; // No static results of this dataset on the element, e.g. outside the read plan.
            if (byCase.Any(l => l.Count != byCase[0].Count)) throw new ArgumentException("IncompleteStaticCases: " + string.Join(", ", cases.Where((c, i) => byCase[i].Count < byCase.Max(l => l.Count))));
            var names = ResultAlgebra.Names(byCase[0][0]);
            foreach (var template in byCase[0])
            {
                token.ThrowIfCancellationRequested();
                var axes = ResultTransformations.AtPoint(ResultAlgebra.Frame(template), ResultAlgebra.Frame(template).Origin);
                var samples = new ResultLocation[cases.Length]; var values = new double[cases.Length][];
                for (int c = 0; c < cases.Length; c++)
                {
                    var matches = c == 0 ? new[] { template } : byCase[c].Where(s => ResultAlgebra.SameLocation(s, template)).ToArray();
                    if (matches.Length != 1) throw new ArgumentException((matches.Length == 0 ? "MissingStaticCase " : "AmbiguousStaticCase ") + cases[c]);
                    Validate(model, owner, matches[0], template, dataset);
                    samples[c] = matches[0]; values[c] = ResultAlgebra.Values(ResultAlgebra.Rotate(matches[0], axes));
                }
                int components = values[0].Length;
                var vectors = terms.Select(t => { var v = new double[components]; foreach (var (c, f) in t) for (int k = 0; k < components; k++) v[k] += f * values[c][k]; return v; }).ToArray();
                var roles = new SortedDictionary<int, List<string>>();
                void Govern(int alternative, string role) { if (!roles.TryGetValue(alternative, out var list)) roles[alternative] = list = new List<string>(); list.Add(role); }
                if (!envelope) Govern(0, "linear");
                else
                    for (int k = 0; k < components; k++)
                    {
                        int low = 0, high = 0;
                        for (int a = 1; a < vectors.Length; a++) { if (vectors[a][k] < vectors[low][k]) low = a; if (vectors[a][k] > vectors[high][k]) high = a; }
                        Govern(low, "min " + names[k]); Govern(high, "max " + names[k]);
                    }
                foreach (var pair in roles)
                {
                    var t = terms[pair.Key];
                    // An alternative without load (all factors zero) is still a state of the envelope: derived from the location, factor 0.
                    var sources = t.Length != 0 ? t.Select(x => samples[x.Case]).ToArray() : new[] { template };
                    var sample = ResultAlgebra.Create(template, vectors[pair.Key].Select(v => NumericGuard.Finite(v, "combined result")).ToArray(), axes, output);
                    var state = template.State.Copy(); state.Original = null; state.SourceRecord = null;
                    state.DerivedFrom = sources; state.DerivedSourceFingerprint = ModelArchive.Fingerprint(sources.Cast<object>()); state.SourceHash = state.DerivedSourceFingerprint;
                    state.IsSynthetic = sources.Any(s => s.State.IsSynthetic); state.IsCombined = true;
                    state.Transformation = t.Length == 0 ? "0 * " + cases[0] : string.Join("; ", t.Select(x => x.Factor.ToString("R", CultureInfo.InvariantCulture) + " * " + cases[x.Case]
                        + " [" + samples[x.Case].State.ConcomitantStateId + "]"));
                    if (envelope)
                    {
                        state.Semantics = AnalysisSemantics.ConcomitantEnvelopeState; state.ConcomitantStateId = "envelope:" + output.Name + ":" + labels[pair.Key];
                        state.Coverage = "Governs " + string.Join(", ", pair.Value) + " over the " + terms.Length.ToString(CultureInfo.InvariantCulture)
                            + " linear alternatives of " + output.Name + " at this location; complete concomitant state, exact sum of the static samples.";
                    }
                    else
                    {
                        state.ConcomitantStateId = "linear:" + output.Name;
                        state.Coverage = "Exact shared location and action identity; linear static superposition only.";
                    }
                    sample.State = state;
                    if (existing.Any(e => e.State?.ConcomitantStateId == state.ConcomitantStateId && e.State.Phase == state.Phase && e.State.Step == state.Step && ResultAlgebra.SameLocation(e, sample)))
                        throw new ArgumentException("DuplicateDerivedResult: " + state.ConcomitantStateId);
                    result.Add(sample);
                }
                locations++;
            }
            return result;
        }

        /// <summary>The checks of <see cref="ResultAlgebra.LinearCombination"/> on one static sample, without searching the registry: the samples come from the owner.</summary>
        private static void Validate(Models.Model model, Element owner, ResultLocation sample, ResultLocation template, AnalysisDataset dataset)
        {
            var state = sample.State;
            if (owner is NodeElement && !(sample is NodeResultForces) && !(sample is NodeResultDisplacement) || owner is BeamElement && !(sample is StationResultBeamForces)
                || owner is AreaElement && !(sample is PointResultPlateForces)) throw new ArgumentException("ResultOwnerFamilyMismatch");
            if (state.InputFingerprint != dataset.InputFingerprint || state.ModelRevision != dataset.ModelRevision || state.Semantics != AnalysisSemantics.LinearStatic
                || state.IsCombined != false || state.Mode.HasValue || state.MovingLoadPosition != null) throw new NotSupportedException("UnsupportedLinearCombinationState");
            if (state.IsCumulative == false || state.Phase != null && state.IsCumulative != true) throw new NotSupportedException("IncrementalOrUnknownPhaseState");
            if (state.IsCumulative != template.State.IsCumulative || state.Phase != template.State.Phase || state.Step != template.State.Step) throw new ArgumentException("IncompatibleStageAccumulation");
            if (string.IsNullOrWhiteSpace(state.ConcomitantStateId) || state.Components == null || state.Components.Length != ResultAlgebra.Values(sample).Length
                || state.Components.Any(c => c != ComponentAvailability.Available)) throw new ArgumentException("IncompleteResultState");
            foreach (double value in ResultAlgebra.Values(sample)) NumericGuard.Finite(value, "result");
            Axes.Validate(ResultAlgebra.Frame(sample));
            if (owner is NodeElement point && Axes.Length(ResultAlgebra.Frame(sample).Origin - point.Position) > 1e-8) throw new ArgumentException("NodalResultPointMismatch");
            if (sample is NodeResultForces nf && owner is NodeElement node) NodalActions.Validate(model, node, nf);
            if (sample is StationResultBeamForces bf && owner is BeamElement beam)
            {
                new BeamReferenceGeometry(beam).ValidateSample(bf);
                if (bf.Body != ActionBody.PositiveSectionFace) throw new ArgumentException("UnresolvedSectionActionBody");
            }
        }
    }
}
