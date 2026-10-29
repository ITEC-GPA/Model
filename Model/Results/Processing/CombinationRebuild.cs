using GPC.Model.Models;
using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Combinations;
using GPC.Model.Elements;
using GPC.Model.Core;
using GPC.Model.Results.Storage;
using GPC.Model.Results.Locations;
using GPC.Model.Analysis;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Results.State;
using GPC.Model.Structure.Topology;

namespace GPC.Model.Results.Processing
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

    /// <summary>
    /// The work of <see cref="CombinationRebuild"/> that does not depend on the factors, kept by a caller that rebuilds many combinations of the
    /// same static results one after the other (ANTHEA: the combinations of a copy of a model): for each location and static case the matched
    /// static sample, validated and rotated into the frame of the location. An entry is used only while the static samples of its case on the
    /// element are the same objects in the same order, and everything is dropped when the model, the dataset or the analysis fingerprint
    /// change: a call with the cache gives the same report, bit for bit, as a call without it (with <see cref="SourceFingerprints"/> true).
    /// Rule for the callers, as for the fingerprint memo of the validation scopes: the static samples are not changed in place while the cache
    /// is in use (replacing them is allowed). Not thread safe: one cache per caller, one call at a time.
    /// </summary>
    public sealed class CombinationRebuildCache
    {
        internal sealed class Match
        {
            internal ResultLocation[] Candidates; internal bool Own; internal ResultLocation Sample; internal double[] Values;
        }
        private sealed class MatchKeyComparer : IEqualityComparer<(ResultLocation Template, string Case)>
        {
            public bool Equals((ResultLocation Template, string Case) x, (ResultLocation Template, string Case) y) =>
                ReferenceEquals(x.Template, y.Template) && string.Equals(x.Case, y.Case, StringComparison.Ordinal);
            public int GetHashCode((ResultLocation Template, string Case) key) =>
                System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(key.Template) * 397 ^ StringComparer.Ordinal.GetHashCode(key.Case ?? "");
        }
        private readonly Dictionary<(ResultLocation Template, string Case), Match> _matches = new Dictionary<(ResultLocation, string), Match>(new MatchKeyComparer());
        private Models.Model _model; private string _datasetId; private AnalysisDataset _dataset; private string _input;

        /// <summary>
        /// False: the states of a report that is not attached (<c>attach: false</c>) carry no fingerprint of their static sources
        /// (<see cref="ResultState.DerivedSourceFingerprint"/> and <see cref="ResultState.SourceHash"/> null); every other value is the same.
        /// For a caller that only reads the values and the governing roles of a detached report, e.g. to display them: the fingerprint walks
        /// every static source of every state, most of the time of a rebuild. Attached results always carry it. Default true.
        /// </summary>
        public bool SourceFingerprints { get; set; } = true;

        /// <summary>Static samples matched to a location and kept for the next calls.</summary>
        public int Entries => _matches.Count;

        /// <summary>Drops everything, e.g. after the static results were changed in place.</summary>
        public void Clear()
        {
            _matches.Clear(); _model = null; _datasetId = null; _dataset = null; _input = null;
        }

        /// <summary>Keeps the entries only for the same model, dataset and analysis fingerprint as the previous call.</summary>
        internal void Bind(Models.Model model, string datasetId, AnalysisDataset dataset, string input)
        {
            if (ReferenceEquals(model, _model) && datasetId == _datasetId && ReferenceEquals(dataset, _dataset) && input == _input) return;
            Clear(); _model = model; _datasetId = datasetId; _dataset = dataset; _input = input;
        }

        /// <summary>The static sample of <paramref name="name"/> at the location of <paramref name="template"/>, validated and rotated into
        /// <paramref name="axes"/>, with the same checks and exceptions, in the same order, as the call without cache. Only a success is kept.</summary>
        internal Match Find(Models.Model model, Element owner, ResultLocation template, string name, bool own, List<ResultLocation> candidates,
            AnalysisDataset dataset, CoordinateSystem axes)
        {
            var key = (template, name);
            if (_matches.TryGetValue(key, out var known) && known.Own == own && Same(known.Candidates, candidates)) return known;
            var matches = own ? new[] { template } : candidates.Where(s => ResultAlgebra.SameLocation(s, template)).ToArray();
            if (matches.Length != 1) throw new ArgumentException((matches.Length == 0 ? "MissingStaticCase " : "AmbiguousStaticCase ") + name);
            CombinationRebuild.Validate(model, owner, matches[0], template, dataset);
            var match = new Match { Candidates = candidates.ToArray(), Own = own, Sample = matches[0], Values = ResultAlgebra.Values(ResultAlgebra.Rotate(matches[0], axes)) };
            _matches[key] = match;
            return match;
        }

        private static bool Same(ResultLocation[] known, List<ResultLocation> current)
        {
            if (known.Length != current.Count) return false;
            for (int i = 0; i < known.Length; i++) if (!ReferenceEquals(known[i], current[i])) return false;
            return true;
        }
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
            IEnumerable<Element> elements, bool attach = true, CancellationToken cancellationToken = default) =>
            Rebuild(model, output, alternatives, datasetId, elements, attach, cancellationToken, null);

        /// <summary>As the other overload; <paramref name="cache"/> keeps the matched, validated and rotated static samples for the next calls on
        /// the same static results (<see cref="CombinationRebuildCache"/>), with the same report. Null: a cache of this call only.</summary>
        public static CombinationRebuildReport Rebuild(Models.Model model, Combination output, IReadOnlyList<CombinationAlternative> alternatives, string datasetId,
            IEnumerable<Element> elements, bool attach, CancellationToken cancellationToken, CombinationRebuildCache cache)
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
                bool fingerprints = attach || cache?.SourceFingerprints != false;
                cache = cache ?? new CombinationRebuildCache(); cache.Bind(model, datasetId, dataset, input);

                var registered = new HashSet<Element>(model.AllElements, ReferenceComparer<Element>.Instance);
                var created = new List<(Element Owner, List<ResultLocation> Samples)>(); var all = new List<ResultLocation>();
                foreach (var owner in elements.Distinct(ReferenceComparer<Element>.Instance))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (owner == null || !registered.Contains(owner)) throw new ArgumentException("UnregisteredElement");
                    try
                    {
                        var samples = Element(model, owner, output, cases, terms, labels, envelope, datasetId, dataset, cache, fingerprints, out int locations, cancellationToken);
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
            string[] labels, bool envelope, string datasetId, AnalysisDataset dataset, CombinationRebuildCache cache, bool fingerprints, out int locations, CancellationToken token)
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
            // In an envelope the governing alternatives of a location share their static samples: the canonical bytes of each are walked
            // once (the memo of the validation scopes, same fingerprints), kept for this element only, so the memory is that of a few samples.
            var memo = fingerprints && envelope ? new FingerprintMemo() : null;
            foreach (var template in byCase[0])
            {
                token.ThrowIfCancellationRequested();
                // The roles name the components of each location: a node can carry displacements and forces (reactions).
                var names = ResultAlgebra.Names(template);
                var axes = ActionTransformations.AtPoint(ResultAlgebra.Frame(template), ResultAlgebra.Frame(template).Origin);
                var samples = new ResultLocation[cases.Length]; var values = new double[cases.Length][];
                for (int c = 0; c < cases.Length; c++)
                {
                    // Matched, validated and rotated once per location and case: the same checks, in the same order, on a miss of the cache.
                    var match = cache.Find(model, owner, template, cases[c], c == 0, byCase[c], dataset, axes);
                    samples[c] = match.Sample; values[c] = match.Values;
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
                    state.DerivedFrom = sources;
                    state.DerivedSourceFingerprint = fingerprints ? InputFingerprint.Compute(sources.Cast<object>(), memo) : null; state.SourceHash = state.DerivedSourceFingerprint;
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
        internal static void Validate(Models.Model model, Element owner, ResultLocation sample, ResultLocation template, AnalysisDataset dataset)
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
