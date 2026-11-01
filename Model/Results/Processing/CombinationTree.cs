using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using GPC.Model.Analysis;
using GPC.Model.Models;
using GPC.Model.Combinations;
using GPC.Model.Core;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Elements;
using GPC.Model.Results.Locations;
using GPC.Model.Results.State;
using GPC.Model.Results.Storage;

namespace GPC.Model.Results.Processing
{
    /// <summary>
    /// A combination as the tree of its definition, for <see cref="CombinationRebuild.Rebuild(Models.Model, Combination, CombinationTreeNode, string, IEnumerable{Element}, bool, CancellationToken, CombinationRebuildCache)"/>:
    /// a linear node sums its terms, an envelope node chooses one of them; a term is a static case or a nested node, with its factor. The
    /// alternatives of the tree are those of the expansion of the definition (one per term of an envelope, the cartesian product of the terms of
    /// a linear node), but they are never listed. Immutable, so a node cannot contain itself.
    /// </summary>
    public sealed class CombinationTreeNode
    {
        public CombinationTreeNode(string name, bool envelope, IEnumerable<CombinationTreeTerm> terms)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Combination name required.", nameof(name));
            Name = name; Envelope = envelope; Terms = (terms ?? throw new ArgumentNullException(nameof(terms))).ToArray();
            if (Terms.Count == 0 || Terms.Any(t => t == null)) throw new ArgumentException("EmptyCombination: " + name);
            if (envelope && Terms.Select(t => t.Label).Distinct(StringComparer.Ordinal).Count() != Terms.Count) throw new ArgumentException("DuplicateAlternativeLabel: " + name);
        }
        /// <summary>The name in the labels of the alternatives («name=term»).</summary>
        public string Name { get; }
        public bool Envelope { get; }
        public IReadOnlyList<CombinationTreeTerm> Terms { get; }

        /// <summary>The number of alternatives of the expansion of the node.</summary>
        public BigInteger AlternativeCount()
        {
            var counts = new Dictionary<CombinationTreeNode, BigInteger>(ReferenceComparer<CombinationTreeNode>.Instance);
            BigInteger Count(CombinationTreeNode node)
            {
                if (counts.TryGetValue(node, out var known)) return known;
                BigInteger total = node.Envelope ? BigInteger.Zero : BigInteger.One;
                foreach (var term in node.Terms)
                {
                    var part = term.Combination == null ? BigInteger.One : Count(term.Combination);
                    total = node.Envelope ? total + part : total * part;
                }
                return counts[node] = total;
            }
            return Count(this);
        }
    }

    /// <summary>A term of a <see cref="CombinationTreeNode"/>: a static case or a nested node, with its factor and its label in the alternatives of
    /// an envelope (unique in the envelope).</summary>
    public sealed class CombinationTreeTerm
    {
        public CombinationTreeTerm(string label, double factor, string staticCase)
            : this(label, factor, staticCase ?? throw new ArgumentNullException(nameof(staticCase)), null) { }
        public CombinationTreeTerm(string label, double factor, CombinationTreeNode combination)
            : this(label, factor, null, combination ?? throw new ArgumentNullException(nameof(combination))) { }
        private CombinationTreeTerm(string label, double factor, string staticCase, CombinationTreeNode combination)
        {
            if (double.IsNaN(factor) || double.IsInfinity(factor)) throw new ArgumentException("NonFiniteCombinationFactor: " + label);
            if (staticCase != null && string.IsNullOrWhiteSpace(staticCase)) throw new ArgumentException("Static case name required.");
            Label = label ?? ""; Factor = factor; StaticCase = staticCase; Combination = combination;
        }
        public string Label { get; }
        public double Factor { get; }
        public string StaticCase { get; }
        public CombinationTreeNode Combination { get; }
    }

    public static partial class CombinationRebuild
    {
        /// <summary>
        /// Rebuilds a combination given as a tree (a linear combination of envelopes, at any depth) without listing its alternatives: at each
        /// location and for each component the extreme of a linear node is the sum of the extremes of its terms (the maximum of a term with a
        /// negative factor is its minimum), the extreme of an envelope node the extreme over its terms, and the governing alternative is the
        /// choice of each envelope that gives it. Each governing alternative is then built exactly as the expansion builds it (labels, factors
        /// summed over the terms) and registered as a complete concomitant state, the sum of its factored static samples. The extremes are
        /// those of the whole expansion; among equal values the first alternative of the expansion governs (the first term of each envelope).
        /// The cost grows with the size of the tree, not with the number of alternatives, so it serves the combinations whose expansion is too
        /// large to list. The values summed term by term may differ from the full sums in the last digits, and the choice between alternatives
        /// equal within that rounding may differ from the expansion: below the limit of the expansion use the other overload. The principal
        /// components of the plates (<see cref="CombinationRebuildCache.PlatePrincipals"/>) need every state: they are not given (empty).
        /// </summary>
        public static CombinationRebuildReport Rebuild(Models.Model model, Combination output, CombinationTreeNode combination, string datasetId,
            IEnumerable<Element> elements, bool attach, CancellationToken cancellationToken, CombinationRebuildCache cache)
        {
            var report = new CombinationRebuildReport { Combination = output?.Name }; var diagnostics = new List<ModelDiagnostic>(); report.Diagnostics = diagnostics;
            try
            {
                if (model == null || output == null || combination == null || elements == null || string.IsNullOrWhiteSpace(datasetId)) throw new ArgumentNullException();
                if (!model.Combinations.Values.Any(c => ReferenceEquals(c, output))) throw new InvalidOperationException("DeclareOutputCaseBeforeAnalysis: " + output.Name);
                var count = combination.AlternativeCount(); bool envelope = count > 1;
                report.Alternatives = count > int.MaxValue ? int.MaxValue : (int)count;
                var cases = new List<string>(); var seen = new HashSet<string>(StringComparer.Ordinal); var visited = new HashSet<CombinationTreeNode>(ReferenceComparer<CombinationTreeNode>.Instance);
                void Collect(CombinationTreeNode node)
                {
                    if (!visited.Add(node)) return;
                    foreach (var term in node.Terms)
                        if (term.Combination != null) Collect(term.Combination);
                        else if (term.Factor != 0 && seen.Add(term.StaticCase)) cases.Add(term.StaticCase);
                }
                Collect(combination);
                if (cases.Count == 0) throw new ArgumentException("EmptyCombination: " + output.Name);
                foreach (var name in cases) if (!model.LoadCases.ContainsKey(name)) throw new ArgumentException("UnknownStaticCase: " + name);
                if (!model.Datasets.TryGetValue(datasetId, out var dataset)) throw new ArgumentException("UnknownDataset: " + datasetId);
                string input = model.AnalysisFingerprint();
                if (dataset.InputFingerprint != input || dataset.NormalizedUnits != "N,mm,rad" || dataset.Semantics != AnalysisSemantics.LinearStatic)
                    throw new InvalidOperationException("DatasetMismatchOrStaleAnalysis: " + datasetId);
                bool fingerprints = attach || cache?.SourceFingerprints != false;
                cache = cache ?? new CombinationRebuildCache(); cache.Bind(model, datasetId, dataset, input);
                var tree = new TreeEvaluation(combination, cases.ToArray(), output.Name, count.ToString(CultureInfo.InvariantCulture), envelope);
                var registered = new HashSet<Element>(model.AllElements, ReferenceComparer<Element>.Instance);
                var created = new List<(Element Owner, List<ResultLocation> Samples)>(); var all = new List<ResultLocation>();
                foreach (var owner in elements.Distinct(ReferenceComparer<Element>.Instance))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (owner == null || !registered.Contains(owner)) throw new ArgumentException("UnregisteredElement");
                    try
                    {
                        var samples = TreeElement(model, owner, output, tree, datasetId, dataset, cache, fingerprints, out int locations, cancellationToken);
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

        /// <summary>The choice made in each envelope of a governing alternative: the term of an envelope node, the choices of the terms of a
        /// linear node (null for a static case).</summary>
        private sealed class Pick
        {
            public int Term; public Pick[] Parts;
        }

        /// <summary>The evaluation of a tree at one location: the static vectors of the cases, the extremes with their choices, the alternatives
        /// built as the expansion builds them.</summary>
        private sealed class TreeEvaluation
        {
            private readonly CombinationTreeNode root; private readonly Dictionary<string, int> index;
            public readonly string[] Cases; public readonly string Output; public readonly string Count; public readonly bool Envelope;
            public double[][] Values;
            public TreeEvaluation(CombinationTreeNode root, string[] cases, string output, string count, bool envelope)
            {
                this.root = root; Cases = cases; Output = output; Count = count; Envelope = envelope;
                index = cases.Select((c, i) => (c, i)).ToDictionary(p => p.c, p => p.i, StringComparer.Ordinal);
            }

            private double Static(CombinationTreeTerm term, int k) => term.Factor == 0 ? 0 : term.Factor * Values[index[term.StaticCase]][k];

            public (double Value, Pick Pick) Extreme(int k, bool maximum) => Extreme(root, k, maximum);

            private (double Value, Pick Pick) Extreme(CombinationTreeNode node, int k, bool maximum)
            {
                if (!node.Envelope)
                {
                    var parts = new Pick[node.Terms.Count]; double sum = 0;
                    for (int i = 0; i < parts.Length; i++)
                    {
                        var term = node.Terms[i];
                        if (term.Combination == null) { sum += Static(term, k); continue; }
                        if (term.Factor == 0) { parts[i] = First(term.Combination); continue; }
                        var (value, pick) = Extreme(term.Combination, k, term.Factor > 0 ? maximum : !maximum);
                        sum += term.Factor * value; parts[i] = pick;
                    }
                    return (sum, new Pick { Parts = parts });
                }
                double best = 0; int chosen = -1; Pick chosenPick = null;
                for (int i = 0; i < node.Terms.Count; i++)
                {
                    var term = node.Terms[i]; double value; Pick pick = null;
                    if (term.Combination == null) value = Static(term, k);
                    else if (term.Factor == 0) { value = 0; pick = First(term.Combination); }
                    else { var e = Extreme(term.Combination, k, term.Factor > 0 ? maximum : !maximum); value = term.Factor * e.Value; pick = e.Pick; }
                    if (chosen < 0 || (maximum ? value > best : value < best)) { best = value; chosen = i; chosenPick = pick; }
                }
                return (best, new Pick { Term = chosen, Parts = new[] { chosenPick } });
            }

            // The first alternative of the expansion: the first term of every envelope.
            private static Pick First(CombinationTreeNode node) => node.Envelope
                ? new Pick { Term = 0, Parts = new[] { node.Terms[0].Combination == null ? null : First(node.Terms[0].Combination) } }
                : new Pick { Parts = node.Terms.Select(t => t.Combination == null ? null : First(t.Combination)).ToArray() };

            /// <summary>The alternative of a choice, built with the operations of the expansion (CombinationExpansion.Expand): the label of each
            /// envelope «name=term» joined with «; », the factors scaled by the terms of the nested combinations and summed over the terms of a
            /// linear node in their order.</summary>
            public (string Label, Dictionary<string, double> Factors) Alternative(Pick pick) => Build(root, pick);

            private static (string Label, Dictionary<string, double> Factors) Build(CombinationTreeNode node, Pick pick)
            {
                (string Label, Dictionary<string, double> Factors) Part(CombinationTreeTerm term, Pick part)
                {
                    if (term.Combination == null) return ("", new Dictionary<string, double>(StringComparer.Ordinal) { [term.StaticCase] = term.Factor });
                    var child = Build(term.Combination, part);
                    return (child.Label, child.Factors.ToDictionary(p => p.Key, p => p.Value * term.Factor, StringComparer.Ordinal));
                }
                if (node.Envelope)
                {
                    var term = node.Terms[pick.Term]; var child = Part(term, pick.Parts[0]);
                    return (Join(node.Name + "=" + term.Label, child.Label), child.Factors);
                }
                var result = ("", new Dictionary<string, double>(StringComparer.Ordinal));
                for (int i = 0; i < node.Terms.Count; i++)
                {
                    var b = Part(node.Terms[i], pick.Parts[i]);
                    var sum = (Join(result.Item1, b.Label), new Dictionary<string, double>(result.Item2, StringComparer.Ordinal));
                    foreach (var p in b.Factors) sum.Item2[p.Key] = (sum.Item2.TryGetValue(p.Key, out var v) ? v : 0) + p.Value;
                    result = sum;
                }
                return result;
            }

            private static string Join(string a, string b) => string.IsNullOrEmpty(a) ? b ?? "" : string.IsNullOrEmpty(b) ? a : a + "; " + b;

            /// <summary>The terms of an alternative as the flat rebuild sums them: non-zero factors in the order of the cases.</summary>
            public (int Case, double Factor)[] Terms(Dictionary<string, double> factors) =>
                factors.Where(p => p.Value != 0).Select(p => (Case: index[p.Key], Factor: NumericGuard.Finite(p.Value, "combination factor"))).OrderBy(t => t.Case).ToArray();
        }

        private static List<ResultLocation> TreeElement(Models.Model model, Element owner, Combination output, TreeEvaluation tree, string datasetId,
            AnalysisDataset dataset, CombinationRebuildCache cache, bool fingerprints, out int locations, CancellationToken token)
        {
            locations = 0;
            var cases = tree.Cases;
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
            if (byCase.All(l => l.Count == 0)) return result;
            if (byCase.Any(l => l.Count != byCase[0].Count)) throw new ArgumentException("IncompleteStaticCases: " + string.Join(", ", cases.Where((c, i) => byCase[i].Count < byCase.Max(l => l.Count))));
            var memo = fingerprints && tree.Envelope ? new FingerprintMemo() : null;
            foreach (var template in byCase[0])
            {
                token.ThrowIfCancellationRequested();
                var names = ResultAlgebra.Names(template);
                var axes = ActionTransformations.AtPoint(ResultAlgebra.Frame(template), ResultAlgebra.Frame(template).Origin);
                var samples = new ResultLocation[cases.Length]; var values = new double[cases.Length][];
                for (int c = 0; c < cases.Length; c++)
                {
                    var match = cache.Find(model, owner, template, cases[c], c == 0, byCase[c], dataset, axes);
                    samples[c] = match.Sample; values[c] = match.Values;
                }
                tree.Values = values;
                int components = values[0].Length;
                // The governing alternatives of the location, in the order of their first role.
                var governing = new List<(string Label, (int Case, double Factor)[] Terms, List<string> Roles)>();
                void Govern(Pick pick, string role)
                {
                    var (label, factors) = tree.Alternative(pick);
                    int at = governing.FindIndex(g => g.Label == label);
                    if (at < 0) governing.Add((label, tree.Terms(factors), new List<string> { role })); else governing[at].Roles.Add(role);
                }
                if (!tree.Envelope) Govern(tree.Extreme(0, true).Pick, "linear");
                else
                    for (int k = 0; k < components; k++)
                    {
                        Govern(tree.Extreme(k, false).Pick, "min " + names[k]); Govern(tree.Extreme(k, true).Pick, "max " + names[k]);
                    }
                foreach (var (label, t, roles) in governing)
                {
                    var vector = new double[components];
                    foreach (var (c, f) in t) for (int k = 0; k < components; k++) vector[k] += f * values[c][k];
                    var sources = t.Length != 0 ? t.Select(x => samples[x.Case]).ToArray() : new[] { template };
                    var sample = ResultAlgebra.Create(template, vector.Select(v => NumericGuard.Finite(v, "combined result")).ToArray(), axes, output);
                    var state = template.State.Copy(); state.Original = null; state.SourceRecord = null;
                    state.DerivedFrom = sources;
                    state.DerivedSourceFingerprint = fingerprints ? InputFingerprint.Compute(sources.Cast<object>(), memo) : null; state.SourceHash = state.DerivedSourceFingerprint;
                    state.IsSynthetic = sources.Any(s => s.State.IsSynthetic); state.IsCombined = true;
                    state.Transformation = t.Length == 0 ? "0 * " + cases[0] : string.Join("; ", t.Select(x => x.Factor.ToString("R", CultureInfo.InvariantCulture) + " * " + cases[x.Case]
                        + " [" + samples[x.Case].State.ConcomitantStateId + "]"));
                    if (tree.Envelope)
                    {
                        state.Semantics = AnalysisSemantics.ConcomitantEnvelopeState; state.ConcomitantStateId = "envelope:" + output.Name + ":" + label;
                        state.Coverage = "Governs " + string.Join(", ", roles) + " over the " + tree.Count
                            + " linear alternatives of " + output.Name + " at this location; complete concomitant state, exact sum of the static samples; extremes found term by term, without listing the alternatives.";
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
    }
}
