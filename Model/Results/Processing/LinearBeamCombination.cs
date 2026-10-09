using GPC.Model.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GPC.Model.Combinations;
using GPC.Model.Results;
using GPC.Model.Results.Locations;
using GPC.Model.Analysis;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Results.Queries;
using GPC.Model.Results.State;

namespace GPC.Model.Results.Processing
{
    public sealed class BeamCombinationResult
    {
        public StationResultBeamForces Sample { get; internal set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; internal set; }
        public bool IsAvailable => Sample != null;
    }

    public static class LinearBeamCombination
    {
        /// <summary>Returns a derived sample without mutating the model. All terms must contain this exact station and side.</summary>
        public static BeamCombinationResult AtStation(Models.Model model, int beamId, Combination combination, string datasetId,
            double station, SectionSide side, string phase = null, string step = null)
        {
            NumericGuard.Station(station); var beam = model.BeamElements[beamId]; var diagnostics = new List<ModelDiagnostic>();
            var terms = new List<StationResultBeamForces>(); var factors = new List<double>();
            var fingerprint = model.AnalysisFingerprint();
            if (!model.Datasets.TryGetValue(datasetId, out var dataset) || dataset.NormalizedUnits != "N,mm,rad" || dataset.InputFingerprint != fingerprint)
                return Reject("DatasetMismatchOrUnknownUnits");
            if (combination == null || combination.LoadCaseCount == 0) return Reject("EmptyCombination");
            foreach (var coefficient in combination.GetLoadCaseCoefficientsPair())
            {
                if (double.IsNaN(coefficient.Value) || double.IsInfinity(coefficient.Value)) return Reject("InvalidCombinationFactor");
                if (coefficient.Value == 0) continue;
                var candidates = ResultQueries.Samples<StationResultBeamForces>(beam, new ResultSelection { Dataset = datasetId, Case = coefficient.Key.Name, Phase = phase, Step = step })
                    .Where(r => r.ParametricDistance == station && r.Side == side).ToArray();
                if (candidates.Length != 1) return Reject("MissingOrAmbiguousCombinationStation");
                var sample = candidates[0]; var state = sample.State;
                if (sample.Case is Combination || state.IsCombined != false) return Reject("AlreadyCombinedOrUnknownProvenance");
                if (state.Semantics != AnalysisSemantics.LinearStatic || state.Mode.HasValue || state.MovingLoadPosition != null || state.IsCumulative == false || (phase != null && state.IsCumulative != true))
                    return Reject("UnsupportedLinearCombinationState");
                if (state.InputFingerprint != fingerprint || state.ModelRevision != dataset.ModelRevision || state.Components == null || state.Components.Length != 6
                    || state.Components.Any(c => c != ComponentAvailability.Available) || string.IsNullOrWhiteSpace(state.ConcomitantStateId)) return Reject("IncompleteCombinationState");
                if (sample.StationDomain != "NodeToNode" || sample.Body != ActionBody.PositiveSectionFace || sample.ResultBeamForces == null) return Reject("UnresolvedCombinationConvention");
                try { Axes.Validate(sample.ResultBeamForces.CoordinateSystem); } catch (ArgumentException) { return Reject("InvalidResultAxes"); }
                if (sample.PhysicalDistance.HasValue && Math.Abs(sample.PhysicalDistance.Value - station * beam.Length) > 1e-7 * Math.Max(1, beam.Length)) return Reject("InconsistentStationDistance");
                terms.Add(sample); factors.Add(coefficient.Value);
            }
            if (terms.Count == 0) return Reject("EmptyCombination");
            var first = terms[0]; var axes = first.ResultBeamForces.CoordinateSystem; var sum = new double[6];
            for (int i = 0; i < terms.Count; i++)
            {
                var term = terms[i]; var old = term.ResultBeamForces.CoordinateSystem;
                if (Axes.Length(old.Origin - axes.Origin) > 1e-8 || Axes.Dot(old.V3, axes.V3) < 1 - 1e-10 || term.State.IsCumulative != first.State.IsCumulative)
                    return Reject("IncompatibleCombinationPointOrFace");
                var f = term.ResultBeamForces.ToCoordinateSystem(axes); var components = new[] { f.N, f.V1, f.V2, f.T, f.M1, f.M2 };
                for (int c = 0; c < 6; c++)
                { sum[c] += factors[i] * components[c]; if (double.IsNaN(sum[c]) || double.IsInfinity(sum[c])) return Reject("NonFiniteCombination"); }
            }
            var trace = string.Join("; ", terms.Select((t, i) => factors[i].ToString("R", CultureInfo.InvariantCulture) + " * " + t.Case.Name + " [" + t.State.ConcomitantStateId + "]"));
            var result = new StationResultBeamForces(combination, new ResultBeamForces(sum[0], sum[1], sum[2], sum[3], sum[4], sum[5], axes), station)
            {
                Side = side,
                PhysicalDistance = station * beam.Length,
                StationDomain = "NodeToNode",
                Body = ActionBody.PositiveSectionFace,
                State = new ResultState
                {
                    DatasetId = datasetId,
                    ModelRevision = dataset.ModelRevision,
                    InputFingerprint = fingerprint,
                    Phase = phase,
                    Step = step,
                    Semantics = AnalysisSemantics.LinearStatic,
                    ConcomitantStateId = "linear:" + combination.Name,
                    IsCombined = true,
                    IsCumulative = first.State.IsCumulative,
                    IsSynthetic = terms.Any(t => t.State.IsSynthetic),
                    Components = Enumerable.Repeat(ComponentAvailability.Available, 6).ToArray(),
                    Transformation = trace,
                    SourceHash = Core.ModelValues.Fingerprint(terms.Cast<object>()),
                    DerivedFrom = terms.Cast<ResultLocation>().ToArray(),
                    DerivedSourceFingerprint = Core.ModelValues.Fingerprint(terms.Cast<object>()),
                    Coverage = "Exact shared station and side only; no interpolation or continuous-maximum claim."
                }
            };
            return new BeamCombinationResult { Sample = result, Diagnostics = diagnostics };

            BeamCombinationResult Reject(string code)
            {
                diagnostics.Add(ModelDiagnostic.Error(code, beam)); return new BeamCombinationResult { Diagnostics = diagnostics };
            }
        }
    }
}
