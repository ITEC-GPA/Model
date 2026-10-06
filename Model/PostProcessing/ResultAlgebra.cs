using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Combinations;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Persistence;
using GPC.Model.Results;
using GPC.Model.Results.ResultLocations;
using GPC.Model.Results.ElementResults;

namespace GPC.Model.PostProcessing
{
    public sealed class DerivedResult
    {
        public ResultLocation Sample { get; internal set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; internal set; } = new ModelDiagnostic[0];
        public bool IsAvailable => Sample != null;
        internal Func<bool> Current { get; set; }
        internal Models.Model Model { get; set; }
        internal ElementKey OwnerKey { get; set; }
        public bool IsCurrent => Sample != null && Current != null && Current();
    }
    /// <summary>Each extreme retains the entire concomitant state that produced it; extrema are never assembled into a force vector.</summary>
    public sealed class ResultEnvelope
    {
        public IReadOnlyList<ResultLocation> Minima { get; internal set; }
        public IReadOnlyList<ResultLocation> Maxima { get; internal set; }
        public IReadOnlyList<ResultLocation> MinimumSources { get; internal set; }
        public IReadOnlyList<ResultLocation> MaximumSources { get; internal set; }
        public IReadOnlyList<string> Components { get; internal set; }
        public string Coverage { get; internal set; }
        internal Func<bool> Current { get; set; }
        public bool IsCurrent => Current != null && Current();
    }

    public static class ResultAlgebra
    {
        /// <summary>The template is an existing sample identifying dataset, phase, step, physical location and action kind.
        /// Exact matching only; no result interpolation or implicit nesting of combinations.</summary>
        public static DerivedResult LinearCombination(Models.Model model, ElementKey key, Combination combination, ResultLocation template)
        {
            try
            {
                ValidateRegistered(model, key, template);
                if (combination == null || combination.LoadCaseCount == 0) throw new ArgumentException("EmptyCombination");
                var owner = Owner(model, key); var terms = new List<ResultLocation>(); var factors = new List<double>();
                foreach (var pair in combination.GetLoadCaseCoefficientsPair())
                {
                    NumericGuard.Finite(pair.Value, "combination factor"); if (pair.Value == 0) continue;
                    var candidates = ResultQueries.Samples<ResultLocation>(owner, new ResultSelection { Dataset = template.State.DatasetId, Case = pair.Key.Name,
                        Phase = template.State.Phase, Step = template.State.Step }).Where(s => SameLocation(s, template)).ToArray();
                    if (candidates.Length != 1) throw new ArgumentException("MissingOrAmbiguousCombinationLocation");
                    var sample = candidates[0]; ValidateRegistered(model, key, sample);
                    if (sample.Case is Combination || sample.State.IsCombined != false || sample.State.Semantics != AnalysisSemantics.LinearStatic
                        || sample.State.Mode.HasValue || sample.State.MovingLoadPosition != null)
                        throw new NotSupportedException("UnsupportedLinearCombinationState");
                    if (sample.State.IsCumulative != template.State.IsCumulative) throw new ArgumentException("IncompatibleStageAccumulation");
                    terms.Add(sample); factors.Add(pair.Value);
                }
                if (terms.Count == 0) throw new ArgumentException("EmptyCombination");
                var result = Sum(terms, factors, combination);
                result.State.IsCombined = true; result.State.ConcomitantStateId = "linear:" + combination.Name;
                result.State.Coverage = "Exact shared location and action identity; linear static superposition only.";
                return Derived(model, key, result);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            { return new DerivedResult { Diagnostics = new[] { new ModelDiagnostic { Code = "CombinationRejected", Message = ex.Message, Severity = DiagnosticSeverity.Error, Family = key?.Family, ElementId = key?.Id } } }; }
        }

        /// <summary>Envelope over explicit registered physical states (including nonlinear/staged states).
        /// Modal shapes, response-spectrum independent extrema and incomplete states are not physical concomitant vectors.</summary>
        public static ResultEnvelope Envelope(Models.Model model, ElementKey key, IReadOnlyList<ResultLocation> samples)
        {
            if (samples == null || samples.Count == 0) throw new ArgumentException("EmptyEnvelope");
            var normalized = new List<ResultLocation>(); var first = samples[0];
            string input = model.AnalysisFingerprint(); var owner = Owner(model, key);
            foreach (var sample in samples)
            {
                ValidateRegistered(model, key, sample, false, input, owner);
                if (!SameLocation(sample, first)) throw new ArgumentException("IncompatibleEnvelopeLocation");
                normalized.Add(Rotate(sample, Frame(first)));
            }
            var vectors = normalized.Select(Values).ToArray(); var minima = new List<ResultLocation>(); var maxima = new List<ResultLocation>();
            var minimumSources = new List<ResultLocation>(); var maximumSources = new List<ResultLocation>();
            for (int c = 0; c < vectors[0].Length; c++)
            {
                int low = 0, high = 0;
                for (int i = 1; i < vectors.Length; i++) { if (vectors[i][c] < vectors[low][c]) low = i; if (vectors[i][c] > vectors[high][c]) high = i; }
                // Separate copies: mutating a displayed extreme cannot corrupt another component's governing state.
                minima.Add(Rotate(normalized[low], Frame(first))); maxima.Add(Rotate(normalized[high], Frame(first)));
                minimumSources.Add(samples[low]); maximumSources.Add(samples[high]);
            }
            var sources = samples.ToArray(); var digest = ModelArchive.Fingerprint(sources.Cast<object>().Concat(minima).Concat(maxima));
            var datasets = ModelArchive.Fingerprint(sources.Select(s => (object)model.Datasets[s.State.DatasetId]));
            return new ResultEnvelope { Minima = minima.AsReadOnly(), Maxima = maxima.AsReadOnly(), Components = Names(first),
                MinimumSources = minimumSources.AsReadOnly(), MaximumSources = maximumSources.AsReadOnly(),
                Current = () => input == model.AnalysisFingerprint() && sources.All(s => owner.Results.SelectMany(r => r.Results).Any(r => ReferenceEquals(r, s))
                    && s.State?.DatasetId != null && model.Datasets.ContainsKey(s.State.DatasetId) && HasCurrentDerivation(model, owner, s.State))
                    && digest == ModelArchive.Fingerprint(sources.Cast<object>().Concat(minima).Concat(maxima))
                    && datasets == ModelArchive.Fingerprint(sources.Select(s => (object)model.Datasets[s.State.DatasetId])),
                Coverage = "Only the supplied discrete states at this location. Each extreme points to a complete original state; no continuous-maximum claim." };
        }

        /// <summary>Explicit accumulation of ordered increments from one declared linear history. Sequence numbers and history identity
        /// must come from the source, not from lexicographic stage names. Nonlinear construction stages must supply cumulative solver results.</summary>
        public static DerivedResult AccumulateLinearHistory(Models.Model model, ElementKey key, IReadOnlyList<ResultLocation> increments, ILoadCase outputCase)
        {
            try
            {
                if (increments == null || increments.Count == 0 || outputCase == null) throw new ArgumentException("EmptyHistory");
                var first = increments[0];
                for (int i = 0; i < increments.Count; i++)
                {
                    var sample = increments[i]; ValidateRegistered(model, key, sample, true);
                    if (!SameLocation(sample, first) || sample.State.DatasetId != first.State.DatasetId || sample.Case?.Name != first.Case?.Name
                        || sample.State.Semantics != AnalysisSemantics.LinearStatic || sample.State.IsCumulative != false || sample.State.IsCombined != false
                        || sample.State.Mode.HasValue || sample.State.MovingLoadPosition != null || sample.State.HistoryId == null
                        || sample.State.HistoryId != first.State.HistoryId || sample.State.IncrementIndex != i || sample.State.IncrementsStartAtZero != true)
                        throw new ArgumentException("IncompleteOrIncompatibleLinearHistory");
                }
                var result = Sum(increments, Enumerable.Repeat(1.0, increments.Count).ToArray(), outputCase);
                result.State.Phase = increments[increments.Count - 1].State.Phase; result.State.Step = increments[increments.Count - 1].State.Step;
                result.State.IsCumulative = true; result.State.IsCombined = false; result.State.ConcomitantStateId = "history:" + first.State.HistoryId + ":" + (increments.Count - 1);
                result.State.IncrementIndex = null; result.State.Coverage = "Explicit consecutive linear increments from a declared zero initial state.";
                return Derived(model, key, result);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            { return new DerivedResult { Diagnostics = new[] { ModelDiagnostic.Error("HistoryRejected", message: ex.Message) } }; }
        }

        internal static Element Owner(Models.Model model, ElementKey key)
            => model.AllElements.Single(e => e.Id == key.Id && Models.Model.FamilyOf(e) == key.Family);
        private static DerivedResult Derived(Models.Model model, ElementKey key, ResultLocation sample)
        {
            string input = model.AnalysisFingerprint(); string output = ModelArchive.Fingerprint(new object[] { sample });
            var owner = Owner(model, key); string datasets = ModelArchive.Fingerprint(sample.State.DerivedFrom.Select(s => (object)model.Datasets[s.State.DatasetId]));
            return new DerivedResult { Sample = sample, Model = model, OwnerKey = new ElementKey { Family = key.Family, Id = key.Id },
                Current = () => input == model.AnalysisFingerprint() && output == ModelArchive.Fingerprint(new object[] { sample })
                    && HasCurrentDerivation(model, owner, sample.State)
                    && sample.State.DerivedFrom.All(s => model.Datasets.ContainsKey(s.State.DatasetId))
                    && datasets == ModelArchive.Fingerprint(sample.State.DerivedFrom.Select(s => (object)model.Datasets[s.State.DatasetId])) };
        }
        public static bool HasCurrentDerivation(Models.Model model, Element owner, ResultState state)
        {
            if (state == null) return false;
            if (state.DerivedFrom == null) return state.DerivedSourceFingerprint == null;
            if (state.DerivedFrom.Length == 0 || string.IsNullOrWhiteSpace(state.DerivedSourceFingerprint)) return false;
            var registered = owner.Results.SelectMany(r => r.Results).ToArray();
            return state.DerivedFrom.All(s => s?.State != null && registered.Any(r => ReferenceEquals(r, s)) && s.State.DerivedFrom == null
                && s.State.DatasetId != null && model.Datasets.TryGetValue(s.State.DatasetId, out var ds) && ds.InputFingerprint == state.InputFingerprint && ds.ModelRevision == s.State.ModelRevision
                && ds.NormalizedUnits == "N,mm,rad" && ds.Semantics == s.State.Semantics)
                && state.DerivedSourceFingerprint == ModelArchive.Fingerprint(state.DerivedFrom.Cast<object>());
        }
        /// <summary>Attaches a checked derivation atomically to the same owner. Declare output cases/combinations before importing analysis results.</summary>
        public static void Attach(DerivedResult derived)
        {
            if (derived == null || !derived.IsCurrent) throw new InvalidOperationException("StaleOrMissingDerivation");
            var model = derived.Model; var sample = derived.Sample; var owner = Owner(model, derived.OwnerKey);
            bool known = sample.Case is Combination ? model.Combinations.Values.Any(c => ReferenceEquals(c, sample.Case))
                : model.LoadCases.Values.Any(c => ReferenceEquals(c, sample.Case));
            if (!known) throw new InvalidOperationException("DeclareOutputCaseBeforeAnalysis");
            if (owner.Results.SelectMany(r => r.Results).Any(r => r.Case?.Name == sample.Case.Name && r.State?.DatasetId == sample.State.DatasetId
                && r.State.Phase == sample.State.Phase && r.State.Step == sample.State.Step && r.State.ConcomitantStateId == sample.State.ConcomitantStateId && SameLocation(r, sample)))
                throw new InvalidOperationException("DuplicateDerivedResult");
            if (sample is IBeamResultLocation beam) owner.AddResult(new BeamResult(new[] { beam }));
            else if (sample is INodeResultLocation node) owner.AddResult(new NodeResult(new List<INodeResultLocation> { node }));
            else if (sample is IPlateResultLocation shell) owner.AddResult(new PlateElementResult(new List<IPlateResultLocation> { shell }));
            else throw new NotSupportedException("UnsupportedDerivedResult");
        }
        internal static void ValidateRegistered(Models.Model model, ElementKey key, ResultLocation sample, bool allowIncrement = false, string inputFingerprint = null, Element resolvedOwner = null)
        {
            if (model == null || key == null || sample?.State == null) throw new ArgumentException("MissingResultState");
            var owner = resolvedOwner ?? Owner(model, key); var state = sample.State;
            if (owner is NodeElement && !(sample is NodeResultForces) && !(sample is NodeResultDisplacement)
                || owner is BeamElement && !(sample is StationResultBeamForces) || owner is AreaElement && !(sample is PointResultPlateForces)
                || !(owner is NodeElement) && !(owner is BeamElement) && !(owner is AreaElement)) throw new ArgumentException("ResultOwnerFamilyMismatch");
            if (!owner.Results.SelectMany(r => r.Results).Any(r => ReferenceEquals(r, sample))) throw new ArgumentException("MissingOrForeignResultSample");
            if (!HasCurrentDerivation(model, owner, state)) throw new ArgumentException("StaleDerivedSources");
            if (state.DatasetId == null || !model.Datasets.TryGetValue(state.DatasetId, out var ds) || ds.InputFingerprint != (inputFingerprint ?? model.AnalysisFingerprint())
                || ds.InputFingerprint != state.InputFingerprint || ds.ModelRevision != state.ModelRevision || ds.NormalizedUnits != "N,mm,rad"
                || ds.Semantics != state.Semantics) throw new ArgumentException("DatasetMismatchOrStaleAnalysis");
            if (sample.Case == null || string.IsNullOrWhiteSpace(state.ConcomitantStateId) || state.Components == null || state.Components.Length != Values(sample).Length
                || state.Components.Any(c => c != ComponentAvailability.Available)) throw new ArgumentException("IncompleteResultState");
            if (state.Semantics != AnalysisSemantics.LinearStatic && state.Semantics != AnalysisSemantics.NonlinearStatic && state.Semantics != AnalysisSemantics.ConcomitantEnvelopeState
                || state.Mode.HasValue) throw new NotSupportedException("NonPhysicalOrNonConcomitantState");
            if (!allowIncrement && (state.IsCumulative == false || state.Phase != null && state.IsCumulative != true)) throw new NotSupportedException("IncrementalOrUnknownPhaseState");
            foreach (double value in Values(sample)) NumericGuard.Finite(value, "result");
            Axes.Validate(Frame(sample));
            if (owner is NodeElement point && Axes.Length(Frame(sample).Origin - point.Position) > 1e-8) throw new ArgumentException("NodalResultPointMismatch");
            if (sample is NodeResultForces nf && owner is NodeElement node) NodalActions.Validate(model, node, nf);
            if (sample is StationResultBeamForces bf && owner is BeamElement beam)
            {
                new BeamReferenceGeometry(beam).ValidateSample(bf);
                if (bf.Body != ActionBody.PositiveSectionFace) throw new ArgumentException("UnresolvedSectionActionBody");
            }
        }
        internal static CoordinateSystem Frame(ResultLocation s)
        {
            if (s?.ResultTypes == null) throw new ArgumentException("MissingResultValues");
            if (s is StationResultBeamForces b) return b.ResultBeamForces.CoordinateSystem;
            if (s is NodeResultForces n) return n.ResultBeamForces.CoordinateSystem;
            if (s is NodeResultDisplacement d) return d.ResultDisplacement.CoordinateSystem;
            if (s is PointResultPlateForces p) return p.Forces.CoordinateSystem;
            throw new NotSupportedException("UnsupportedResultAlgebra");
        }
        internal static ResultLocation Rotate(ResultLocation s, CoordinateSystem axes)
        {
            if (s is StationResultBeamForces b) return ResultOrientation.Beam(b, axes);
            if (s is NodeResultForces n) return ResultTransformations.RotateNode(n, axes);
            if (s is NodeResultDisplacement d) return ResultTransformations.RotateNode(d, axes);
            if (s is PointResultPlateForces p) return ResultOrientation.Shell(p, axes);
            throw new NotSupportedException("UnsupportedResultAlgebra");
        }
        internal static bool SameLocation(ResultLocation a, ResultLocation b)
        {
            if (a == null || b == null || a.GetType() != b.GetType()) return false;
            Axes.Validate(Frame(a)); Axes.Validate(Frame(b));
            if (Axes.Length(Frame(a).Origin - Frame(b).Origin) > 1e-8) return false;
            if (a is StationResultBeamForces x && b is StationResultBeamForces y)
                return x.ParametricDistance == y.ParametricDistance && x.Side == y.Side && x.StationDomain == y.StationDomain && x.Body == y.Body;
            if (a is NodeResultForces n && b is NodeResultForces m)
                return n.Kind == m.Kind && n.Body == m.Body && n.OwnerElementFamily == m.OwnerElementFamily && n.OwnerElementId == m.OwnerElementId && n.ElementEnd == m.ElementEnd && n.AggregationSet == m.AggregationSet;
            if (a is PointResultPlateForces p && b is PointResultPlateForces q)
            {
                if (p.PointKind != q.PointKind || p.CoordinateKind != q.CoordinateKind || p.SourceNodeId != q.SourceNodeId || p.AveragingRegion != q.AveragingRegion
                    || p.Location == null || q.Location == null || (p.GlobalLocation == null) != (q.GlobalLocation == null)) return false;
                if (p.GlobalLocation != null && Axes.Length(p.GlobalLocation - q.GlobalLocation) > 1e-8) return false;
                var rotated = ResultOrientation.Shell(p, q.Forces.CoordinateSystem);
                return Math.Abs(rotated.Location.X - q.Location.X) < 1e-8 && Math.Abs(rotated.Location.Y - q.Location.Y) < 1e-8;
            }
            return a is NodeResultDisplacement;
        }
        internal static double[] Values(ResultLocation s)
        {
            if (s?.ResultTypes == null) throw new ArgumentException("MissingResultValues");
            if (s is StationResultBeamForces b) { var f = b.ResultBeamForces; return new[] { f.N, f.V1, f.V2, f.T, f.M1, f.M2 }; }
            if (s is NodeResultForces n) return new[] { n.Fx, n.Fy, n.Fz, n.Mx, n.My, n.Mz };
            if (s is NodeResultDisplacement d) { var f = d.ResultDisplacement; return new[] { f.D1, f.D2, f.D3, f.R1, f.R2, f.R3 }; }
            if (s is PointResultPlateForces p) { var f = p.Forces; return new[] { f.Fxx, f.Fyy, f.Fxy, f.Fxz, f.Fyz, f.Mxx, f.Myy, f.Mxy }; }
            throw new NotSupportedException("UnsupportedResultAlgebra");
        }
        internal static string[] Names(ResultLocation s) => s is PointResultPlateForces ? new[] { "Nxx", "Nyy", "Nxy", "Qx", "Qy", "Mxx", "Myy", "Mxy" }
            : s is StationResultBeamForces ? new[] { "N", "V1", "V2", "T", "M1", "M2" }
            : s is NodeResultDisplacement ? new[] { "Dx", "Dy", "Dz", "Rx", "Ry", "Rz" } : new[] { "Fx", "Fy", "Fz", "Mx", "My", "Mz" };
        private static ResultLocation Sum(IReadOnlyList<ResultLocation> terms, IReadOnlyList<double> factors, ILoadCase outputCase)
        {
            var first = terms[0]; var axes = ResultTransformations.AtPoint(Frame(first), Frame(first).Origin); var sum = new double[Values(first).Length];
            for (int i = 0; i < terms.Count; i++)
            {
                var values = Values(Rotate(terms[i], axes));
                for (int c = 0; c < sum.Length; c++) sum[c] = NumericGuard.Finite(sum[c] + factors[i] * values[c], "combined result");
            }
            var state = first.State.Copy(); state.Original = null; state.SourceRecord = null;
            if (terms.Any(t => t.State.DerivedFrom != null)) throw new NotSupportedException("NestedDerivationRequiresOriginalSources");
            state.DerivedFrom = terms.ToArray(); state.DerivedSourceFingerprint = ModelArchive.Fingerprint(terms.Cast<object>());
            state.SourceHash = ModelArchive.Fingerprint(terms.Cast<object>()); state.IsSynthetic = terms.Any(t => t.State.IsSynthetic);
            state.Transformation = string.Join("; ", terms.Select((t, i) => factors[i].ToString("R", CultureInfo.InvariantCulture) + " * " + t.Case.Name + " [" + t.State.ConcomitantStateId + "]"));
            var result = Create(first, sum, axes, outputCase);
            result.State = state; return result;
        }
        /// <summary>A sample at the location of <paramref name="first"/> with the given values in <paramref name="axes"/>; the state is left to the caller.</summary>
        internal static ResultLocation Create(ResultLocation first, double[] sum, CoordinateSystem axes, ILoadCase outputCase)
        {
            ResultLocation result;
            if (first is StationResultBeamForces b)
                result = new StationResultBeamForces(outputCase, new ResultBeamForces(sum[0], sum[1], sum[2], sum[3], sum[4], sum[5], axes), b.ParametricDistance)
                { Side = b.Side, Body = b.Body, PhysicalDistance = b.PhysicalDistance, StationDomain = b.StationDomain };
            else if (first is NodeResultForces n)
                result = new NodeResultForces(outputCase, new ResultBeamForces(sum[2], sum[0], sum[1], sum[5], sum[3], sum[4], axes))
                { Kind = n.Kind, Body = n.Body, OwnerElementFamily = n.OwnerElementFamily, OwnerElementId = n.OwnerElementId, ElementEnd = n.ElementEnd, AggregationSet = n.AggregationSet };
            else if (first is NodeResultDisplacement) result = new NodeResultDisplacement(outputCase, new ResultDisplacement(axes, sum[0], sum[1], sum[2], sum[3], sum[4], sum[5]));
            else
            {
                var p = (PointResultPlateForces)first;
                result = new PointResultPlateForces(outputCase, new ResultPlateForces(axes, sum[0], sum[1], sum[2], sum[3], sum[4], sum[5], sum[6], sum[7]), new Point2d(p.Location.X, p.Location.Y), p.LocationKind)
                { PointKind = p.PointKind, CoordinateKind = p.CoordinateKind, SourceNodeId = p.SourceNodeId, AveragingRegion = p.AveragingRegion,
                    GlobalLocation = p.GlobalLocation == null ? null : new Point3d(p.GlobalLocation.X, p.GlobalLocation.Y, p.GlobalLocation.Z) };
            }
            return result;
        }
    }
}
