using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Combinations;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Results.Locations;

namespace GPC.Model.Results.Processing
{
    /// <summary>One combined state of a location of a <see cref="CombinationMatrixReport"/>: what <see cref="CombinationRebuild"/> registers as a
    /// sample, without the sample. The values are those the sample would have, bit for bit.</summary>
    public sealed class CombinationMatrixState
    {
        internal CombinationMatrixState(int alternative, string stateId, double[] values, string[] roles)
        {
            Alternative = alternative; StateId = stateId; Values = values; Roles = roles;
        }
        /// <summary>The index of the alternative in the call (0 for a linear combination).</summary>
        public int Alternative { get; }
        /// <summary>The <c>ConcomitantStateId</c> of the rebuilt sample: "linear:NAME" or "envelope:NAME:LABEL".</summary>
        public string StateId { get; }
        /// <summary>The combined vector in the frame of the location, in the order of <see cref="CombinationMatrixLocation.Components"/>
        /// (plates: Nxx, Nyy, Nxy, Qx, Qy, Mxx, Myy, Mxy; beams: N, V1, V2, T, M1, M2; nodes: Dx…Rz or Fx…Mz).</summary>
        public IReadOnlyList<double> Values { get; }
        /// <summary>The roles the state governs, as in the coverage of the rebuilt sample: "linear", or "min Nxx", "max Mxx"….</summary>
        public IReadOnlyList<string> Roles { get; }
    }

    /// <summary>One location of a <see cref="CombinationMatrixReport"/>: the element, the static sample that gives the location (the template of
    /// <see cref="CombinationRebuild"/>: the sample of the first case of the call), the names of the components and the combined states.</summary>
    public sealed class CombinationMatrixLocation
    {
        internal CombinationMatrixLocation(Element owner, ResultLocation location, string[] components, CombinationMatrixState[] states, PlatePrincipalResult principals)
        {
            Owner = owner; Location = location; Components = components; States = states; Principals = principals;
        }
        public Element Owner { get; }
        public ResultLocation Location { get; }
        public IReadOnlyList<string> Components { get; }
        /// <summary>One state for a linear combination; the governing alternatives of the location, in the order of the alternatives, for an envelope.</summary>
        public IReadOnlyList<CombinationMatrixState> States { get; }
        /// <summary>The principal components of a plate location, when asked; null otherwise.</summary>
        public PlatePrincipalResult Principals { get; }
    }

    /// <summary>The result of <see cref="CombinationMatrix.Evaluate"/>: the same locations, states, values, principal components and diagnostics
    /// as the detached report of <see cref="CombinationRebuild"/>, in the same order.</summary>
    public sealed class CombinationMatrixReport
    {
        public string Combination { get; internal set; }
        public int Alternatives { get; internal set; }
        public int Locations { get; internal set; }
        public IReadOnlyList<CombinationMatrixLocation> Results { get; internal set; } = new CombinationMatrixLocation[0];
        /// <summary>The principal components of the plate locations, in the order of the locations, when asked; empty otherwise.</summary>
        public IReadOnlyList<PlatePrincipalResult> PlatePrincipals { get; internal set; } = new PlatePrincipalResult[0];
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; internal set; } = new ModelDiagnostic[0];
        public bool IsRejected => Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
        /// <summary>Elements combined through <see cref="CombinationRebuild"/> itself, because their static samples are not uniform (see
        /// <see cref="CombinationMatrix"/>).</summary>
        public int RebuiltElements { get; internal set; }
    }

    /// <summary>
    /// The combinations of the linear static results of one dataset, many after the other, without registering samples (GPCModel 5.38, request
    /// of the ANTHEA user of 2/11/2026). <see cref="Prepare"/> collects the static samples of every element; each static case is matched, checked
    /// and rotated once, at the first call that uses it (as <see cref="CombinationRebuildCache"/>). <see cref="Evaluate"/> then does, for each
    /// location, only the arithmetic of <see cref="CombinationRebuild"/>: the same alternatives, the
    /// same sums in the same order, the same governing alternatives (the first on a tie), the same principal components. It returns the values,
    /// roles and state ids of the states the rebuild would register, bit for bit, without the samples, their states, provenance and texts: for a
    /// caller that displays the combinations (ANTHEA). The rebuilt samples, for the checks, remain those of <see cref="CombinationRebuild"/>.
    /// An element is prepared when every static case on it has the same number of samples, the i-th samples of all cases describe the same
    /// location bit for bit (frame, point, station or node, stage accumulation) and each matches only its own location; the other elements are
    /// combined by <see cref="CombinationRebuild"/> itself in every call, with the same report. The static samples must not change while the
    /// matrix is in use (as for <see cref="CombinationRebuildCache"/>). One call at a time; the locations of a call are computed in parallel.
    /// </summary>
    public sealed class CombinationMatrix
    {
        private sealed class Prepared
        {
            internal Element Owner;
            internal Dictionary<string, List<ResultLocation>> ByCase;
            internal HashSet<string> CaseNames;
            internal List<ResultLocation> Canonical;
            internal bool Uniform;
            internal CoordinateSystem[] Axes;
            // Filled case by case at the first call that uses the case (as the cache of the rebuild); one thread per element in a call.
            internal readonly Dictionary<string, double[][]> Values = new Dictionary<string, double[][]>(StringComparer.Ordinal);
            internal readonly Dictionary<string, ExceptionDispatchInfo[]> Errors = new Dictionary<string, ExceptionDispatchInfo[]>(StringComparer.Ordinal);
        }
        private sealed class Outcome
        {
            internal List<CombinationMatrixLocation> Locations = new List<CombinationMatrixLocation>();
            internal int Count; internal ModelDiagnostic Skipped; internal ExceptionDispatchInfo Rejection;
        }

        private readonly Models.Model _model; private readonly string _datasetId; private readonly AnalysisDataset _dataset; private readonly string _input;
        private readonly Prepared[] _elements; private readonly CombinationRebuildCache _cache = new CombinationRebuildCache { SourceFingerprints = false };

        /// <summary>For the tests: combine every element through <see cref="CombinationRebuild"/>, as the elements that are not uniform.</summary>
        internal bool RebuildAll { get; set; }
        /// <summary>The elements prepared for the matrix (the others are combined by the rebuild).</summary>
        public int UniformElements => _elements.Count(e => e.Uniform);
        public int Elements => _elements.Length;

        private CombinationMatrix(Models.Model model, string datasetId, AnalysisDataset dataset, string input, Prepared[] elements)
        {
            _model = model; _datasetId = datasetId; _dataset = dataset; _input = input; _elements = elements;
        }

        /// <summary>Collects the static samples of <paramref name="elements"/> in <paramref name="datasetId"/>, with the checks of the dataset of
        /// <see cref="CombinationRebuild"/>; the samples of a case are checked at the first call that uses it, and a failed check is raised by the
        /// calls that use the case, as by the rebuild.</summary>
        public static CombinationMatrix Prepare(Models.Model model, string datasetId, IEnumerable<Element> elements, CancellationToken cancellationToken = default)
        {
            if (model == null || elements == null || string.IsNullOrWhiteSpace(datasetId)) throw new ArgumentNullException();
            if (!model.Datasets.TryGetValue(datasetId, out var dataset)) throw new ArgumentException("UnknownDataset: " + datasetId);
            string input = model.AnalysisFingerprint();
            if (dataset.InputFingerprint != input || dataset.NormalizedUnits != "N,mm,rad" || dataset.Semantics != AnalysisSemantics.LinearStatic)
                throw new InvalidOperationException("DatasetMismatchOrStaleAnalysis: " + datasetId);
            var registered = new HashSet<Element>(model.AllElements, ReferenceComparer<Element>.Instance);
            var owners = elements.Distinct(ReferenceComparer<Element>.Instance).ToArray();
            if (owners.Any(o => o == null || !registered.Contains(o))) throw new ArgumentException("UnregisteredElement");
            var prepared = new Prepared[owners.Length];
            Parallel.For(0, owners.Length, new ParallelOptions { CancellationToken = cancellationToken }, i => prepared[i] = PrepareElement(model, owners[i], datasetId, dataset));
            return new CombinationMatrix(model, datasetId, dataset, input, prepared);
        }

        private static Prepared PrepareElement(Models.Model model, Element owner, string datasetId, AnalysisDataset dataset)
        {
            var p = new Prepared { Owner = owner, ByCase = new Dictionary<string, List<ResultLocation>>(StringComparer.Ordinal), CaseNames = new HashSet<string>(StringComparer.Ordinal) };
            // The samples a rebuild would collect for its cases, in the same order.
            List<ResultLocation> canonical = null;
            foreach (var sample in owner.Results.SelectMany(r => r.Results))
            {
                if (sample?.State?.DatasetId != datasetId || sample.Case == null) continue;
                p.CaseNames.Add(sample.Case.Name);
                if (sample.Case is Combination || sample.State.DerivedFrom != null) continue;
                if (!p.ByCase.TryGetValue(sample.Case.Name, out var list)) p.ByCase[sample.Case.Name] = list = new List<ResultLocation>();
                list.Add(sample); canonical = canonical ?? list;
            }
            p.Canonical = canonical;
            if (canonical == null) { p.Uniform = true; return p; }
            try
            {
                p.Uniform = p.ByCase.Values.All(l => l.Count == canonical.Count) && p.ByCase.Values.All(l => l.Select((s, i) => SameDescription(s, canonical[i])).All(x => x));
                if (p.Uniform) p.Axes = canonical.Select(t => ActionTransformations.AtPoint(ResultAlgebra.Frame(t), ResultAlgebra.Frame(t).Origin)).ToArray();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) { p.Uniform = false; }
            return p;
        }

        /// <summary>The values of <paramref name="name"/> at every location of a uniform element, prepared at the first call that uses the case: each
        /// sample matches only its own location (as the rebuild requires of the template of any case), then the checks and the rotation of
        /// CombinationRebuildCache.Find, against the template of the location; a failed check is kept for the calls that use the case. False when a
        /// sample matches another location too: the element is no longer uniform and goes through the rebuild.</summary>
        private bool PrepareCase(Prepared p, string name)
        {
            if (p.Values.ContainsKey(name)) return true;
            var list = p.ByCase[name]; var canonical = p.Canonical;
            try
            {
                for (int i = 0; i < canonical.Count; i++)
                    if (list.Count(s => ResultAlgebra.SameLocation(s, canonical[i])) != 1 || !ResultAlgebra.SameLocation(list[i], canonical[i])) { p.Uniform = false; return false; }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) { p.Uniform = false; return false; }
            var values = new double[canonical.Count][]; var errors = new ExceptionDispatchInfo[canonical.Count];
            for (int i = 0; i < canonical.Count; i++)
            {
                try
                {
                    CombinationRebuild.Validate(_model, p.Owner, list[i], canonical[i], _dataset);
                    values[i] = ResultAlgebra.Values(ResultAlgebra.Rotate(list[i], p.Axes[i]));
                }
                catch (Exception ex) { errors[i] = ExceptionDispatchInfo.Capture(ex); }
            }
            p.Values[name] = values; p.Errors[name] = errors;
            return true;
        }

        /// <summary>The same location, bit for bit: type, frame, point, station or node and stage accumulation. Then the match, the rotation and
        /// the checks of the rebuild do not depend on the case that gives the template.</summary>
        private static bool SameDescription(ResultLocation a, ResultLocation b)
        {
            if (a == null || b == null || a.GetType() != b.GetType() || a.ResultTypes == null || b.ResultTypes == null) return false;
            if (a.State.IsCumulative != b.State.IsCumulative || a.State.Phase != b.State.Phase || a.State.Step != b.State.Step) return false;
            if (!SameFrame(ResultAlgebra.Frame(a), ResultAlgebra.Frame(b))) return false;
            if (a is StationResultBeamForces x && b is StationResultBeamForces y)
                return x.ParametricDistance.Equals(y.ParametricDistance) && x.Side == y.Side && x.StationDomain == y.StationDomain && x.Body == y.Body && x.PhysicalDistance.Equals(y.PhysicalDistance);
            if (a is NodeResultForces n && b is NodeResultForces m)
                return n.Kind == m.Kind && n.Body == m.Body && n.OwnerElementFamily == m.OwnerElementFamily && n.OwnerElementId == m.OwnerElementId && n.ElementEnd == m.ElementEnd && n.AggregationSet == m.AggregationSet;
            if (a is PointResultPlateForces p && b is PointResultPlateForces q)
                return p.PointKind == q.PointKind && p.CoordinateKind == q.CoordinateKind && p.SourceNodeId == q.SourceNodeId && p.AveragingRegion == q.AveragingRegion && p.LocationKind == q.LocationKind
                    && p.Location != null && q.Location != null && p.Location.X.Equals(q.Location.X) && p.Location.Y.Equals(q.Location.Y)
                    && (p.GlobalLocation == null) == (q.GlobalLocation == null) && (p.GlobalLocation == null || p.GlobalLocation.X.Equals(q.GlobalLocation.X)
                    && p.GlobalLocation.Y.Equals(q.GlobalLocation.Y) && p.GlobalLocation.Z.Equals(q.GlobalLocation.Z));
            return a is NodeResultDisplacement;
        }
        private static bool SameFrame(CoordinateSystem a, CoordinateSystem b) => a != null && b != null && SamePoint(a.Origin, b.Origin)
            && SameVector(a.V1, b.V1) && SameVector(a.V2, b.V2) && SameVector(a.V3, b.V3);
        private static bool SamePoint(Point3d a, Point3d b) => a != null && b != null && a.X.Equals(b.X) && a.Y.Equals(b.Y) && a.Z.Equals(b.Z);
        private static bool SameVector(Vector3d a, Vector3d b) => a != null && b != null && a.X.Equals(b.X) && a.Y.Equals(b.Y) && a.Z.Equals(b.Z);

        /// <summary>
        /// The combination of <paramref name="alternatives"/> (one: linear; several: envelope) into <paramref name="output"/>, a Model combination,
        /// as <see cref="CombinationRebuild.Rebuild(Models.Model, Combination, IReadOnlyList{CombinationAlternative}, string, IEnumerable{Element}, bool, CancellationToken, CombinationRebuildCache)"/>
        /// with <c>attach: false</c> on the prepared elements: the same checks and diagnostics, the values of its samples. With
        /// <paramref name="platePrincipals"/> the principal components of the plates, as <see cref="CombinationRebuildCache.PlatePrincipals"/>.
        /// </summary>
        public CombinationMatrixReport Evaluate(Combination output, IReadOnlyList<CombinationAlternative> alternatives, bool platePrincipals = false, CancellationToken cancellationToken = default)
        {
            var report = new CombinationMatrixReport { Combination = output?.Name }; var diagnostics = new List<ModelDiagnostic>(); report.Diagnostics = diagnostics;
            try
            {
                if (output == null || alternatives == null) throw new ArgumentNullException();
                if (!_model.Combinations.Values.Any(c => ReferenceEquals(c, output))) throw new InvalidOperationException("DeclareOutputCaseBeforeAnalysis: " + output.Name);
                if (alternatives.Count == 0 || alternatives.Any(a => a == null)) throw new ArgumentException("EmptyCombination: " + output.Name);
                bool envelope = alternatives.Count > 1; report.Alternatives = alternatives.Count;
                var labels = alternatives.Select((a, i) => string.IsNullOrEmpty(a.Label) ? "#" + (i + 1).ToString(System.Globalization.CultureInfo.InvariantCulture) : a.Label).ToArray();
                if (envelope && labels.Distinct(StringComparer.Ordinal).Count() != labels.Length) throw new ArgumentException("DuplicateAlternativeLabel: " + output.Name);
                var cases = alternatives.SelectMany(a => a.Factors.Where(p => p.Value != 0).Select(p => p.Key)).Distinct(StringComparer.Ordinal).ToArray();
                if (cases.Length == 0) throw new ArgumentException("EmptyCombination: " + output.Name);
                foreach (var name in cases) if (!_model.LoadCases.ContainsKey(name)) throw new ArgumentException("UnknownStaticCase: " + name);
                var index = cases.Select((c, i) => (c, i)).ToDictionary(p => p.c, p => p.i, StringComparer.Ordinal);
                var terms = alternatives.Select(a => a.Factors.Where(p => p.Value != 0).Select(p => (Case: index[p.Key], Factor: NumericGuard.Finite(p.Value, "combination factor")))
                    .OrderBy(t => t.Case).ToArray()).ToArray();
                // The prepared static samples stand for the dataset of the preparation; a changed dataset needs a new matrix.
                if (!_model.Datasets.TryGetValue(_datasetId, out var dataset) || !ReferenceEquals(dataset, _dataset) || dataset.InputFingerprint != _input)
                    throw new InvalidOperationException("DatasetMismatchOrStaleAnalysis: " + _datasetId);
                var states = labels.Select(l => envelope ? "envelope:" + output.Name + ":" + l : "linear:" + output.Name).ToArray();

                var outcomes = new Outcome[_elements.Length];
                bool Matrix(Prepared e) => e.Uniform && !RebuildAll && !e.CaseNames.Contains(output.Name);
                Parallel.For(0, _elements.Length, new ParallelOptions { CancellationToken = cancellationToken }, i =>
                {
                    if (Matrix(_elements[i])) outcomes[i] = Combine(_elements[i], output, cases, terms, labels, states, envelope, platePrincipals, cancellationToken);
                });
                // The other elements through the rebuild, one at a time (its cache is not thread safe), in the order of the elements.
                for (int i = 0; i < _elements.Length; i++)
                {
                    if (outcomes[i] != null) continue;
                    outcomes[i] = Rebuilt(_elements[i], output, cases, terms, labels, envelope, platePrincipals, cancellationToken); report.RebuiltElements++;
                }
                var results = new List<CombinationMatrixLocation>(); var principals = platePrincipals ? new List<PlatePrincipalResult>() : null;
                foreach (var outcome in outcomes)
                {
                    outcome.Rejection?.Throw();
                    if (outcome.Skipped != null) { diagnostics.Add(outcome.Skipped); continue; }
                    results.AddRange(outcome.Locations); report.Locations += outcome.Count;
                    if (principals != null) principals.AddRange(outcome.Locations.Where(l => l.Principals != null).Select(l => l.Principals));
                }
                report.Results = results.AsReadOnly();
                if (principals != null) report.PlatePrincipals = principals.AsReadOnly();
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            { diagnostics.Add(ModelDiagnostic.Error("CombinationRejected", message: ex.Message)); report.Results = new CombinationMatrixLocation[0]; report.Locations = 0; report.PlatePrincipals = new PlatePrincipalResult[0]; }
            return report;
        }

        /// <summary>One prepared element: the loop of the rebuild (CombinationRebuild.Element) on the prepared values, without the samples.</summary>
        private Outcome Combine(Prepared e, Combination output, string[] cases, (int Case, double Factor)[][] terms, string[] labels, string[] stateIds,
            bool envelope, bool platePrincipals, CancellationToken token)
        {
            var outcome = new Outcome();
            try
            {
                var lists = cases.Select(c => e.ByCase.TryGetValue(c, out var l) ? l : null).ToArray();
                int[] counts = lists.Select(l => l?.Count ?? 0).ToArray();
                if (counts.All(n => n == 0)) return outcome; // No static results of this dataset on the element, e.g. outside the read plan.
                if (counts.Any(n => n != counts[0])) throw new ArgumentException("IncompleteStaticCases: " + string.Join(", ", cases.Where((c, i) => counts[i] < counts.Max())));
                foreach (var c in cases) if (!PrepareCase(e, c)) return null; // no longer uniform: through the rebuild
                var values = cases.Select(c => e.Values[c]).ToArray(); var errors = cases.Select(c => e.Errors[c]).ToArray();
                var templates = lists[0];
                for (int i = 0; i < templates.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var template = templates[i]; var names = ResultAlgebra.Names(template);
                    var at = new double[cases.Length][];
                    for (int c = 0; c < cases.Length; c++) { errors[c][i]?.Throw(); at[c] = values[c][i]; }
                    int components = at[0].Length;
                    // The sums of the rebuild, term by term in the order of the cases of the call.
                    var vectors = terms.Select(t => { var v = new double[components]; foreach (var (c, f) in t) for (int k = 0; k < components; k++) v[k] += f * at[c][k]; return v; }).ToArray();
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
                    var governing = roles.Select(pair => new CombinationMatrixState(pair.Key, stateIds[pair.Key],
                        vectors[pair.Key].Select(v => NumericGuard.Finite(v, "combined result")).ToArray(), pair.Value.ToArray())).ToArray();
                    var principals = platePrincipals && template is PointResultPlateForces ? CombinationRebuild.Principals(e.Owner, template, vectors, envelope, output.Name, labels) : null;
                    outcome.Locations.Add(new CombinationMatrixLocation(e.Owner, template, names, governing, principals));
                    outcome.Count++;
                }
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) { outcome.Skipped = Skip(e.Owner, output, ex); }
            catch (Exception ex) when (!(ex is OperationCanceledException)) { outcome.Rejection = ExceptionDispatchInfo.Capture(ex); }
            return outcome;
        }

        /// <summary>An element that is not uniform: the rebuild itself, detached, read back into the states of the matrix.</summary>
        private Outcome Rebuilt(Prepared e, Combination output, string[] cases, (int Case, double Factor)[][] terms, string[] labels, bool envelope,
            bool platePrincipals, CancellationToken token)
        {
            var outcome = new Outcome();
            try
            {
                _cache.Bind(_model, _datasetId, _dataset, _input);
                var principals = platePrincipals ? new List<PlatePrincipalResult>() : null;
                var layout = new List<(ResultLocation Template, (int Alternative, string[] Roles)[] States)>();
                var samples = CombinationRebuild.Element(_model, e.Owner, output, cases, terms, labels, envelope, _datasetId, _dataset, _cache, false, principals, out int locations, token, layout);
                int next = 0, plate = 0;
                foreach (var (template, states) in layout)
                {
                    var governing = states.Select(s => { var sample = samples[next++]; return new CombinationMatrixState(s.Alternative, sample.State.ConcomitantStateId, ResultAlgebra.Values(sample), s.Roles); }).ToArray();
                    var principal = principals != null && template is PointResultPlateForces ? principals[plate++] : null;
                    outcome.Locations.Add(new CombinationMatrixLocation(e.Owner, template, ResultAlgebra.Names(template), governing, principal));
                }
                outcome.Count = locations;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) { outcome.Skipped = Skip(e.Owner, output, ex); }
            catch (Exception ex) when (!(ex is OperationCanceledException)) { outcome.Rejection = ExceptionDispatchInfo.Capture(ex); }
            return outcome;
        }

        private static ModelDiagnostic Skip(Element owner, Combination output, Exception ex) => new ModelDiagnostic
        {
            Code = "CombinationElementSkipped", Severity = DiagnosticSeverity.Warning, Family = Models.Model.FamilyOf(owner), ElementId = owner.Id, Message = output.Name + ": " + ex.Message
        };
    }
}
