using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;
using GPC.Model.Elements;
using GPC.Model.Results.Locations;

namespace GPC.Model.Checking.Reports
{
    /// <summary>The station force samples of a beam, indexed by the state and location a stored section row names: the currency of a report
    /// (<see cref="CheckReport.CurrentOutcome(Models.Model, IConcreteSectionVerifier, IPhysicalMemberVerifier, BeamCheckPlanRequest)"/>) finds
    /// the samples of each row with one lookup instead of scanning all the results of its beam for every row.</summary>
    internal static class StationSamples
    {
        private static readonly StationResultBeamForces[] None = new StationResultBeamForces[0];

        /// <summary>The samples of <paramref name="beam"/> with the dataset, case, station, side, phase, step, concomitant state, mode and
        /// moving load position of the row, in the order of the results of the beam: the same samples as the scan with <c>==</c> on each
        /// field (ordinal strings, exact numbers). In a read scope the index of each beam is built once.</summary>
        internal static IReadOnlyList<StationResultBeamForces> Matching(Models.Model model, BeamElement beam, CheckResult result)
        {
            // Without a dataset the row also names the samples without a state: keep the scan, and whatever it meets, for that row.
            if (result.Dataset == null)
                return Scan(beam, result);
            // No station, or an undefined one, is equal to no distance.
            if (!result.Station.HasValue || double.IsNaN(result.Station.Value))
                return None;
            var index = ValidationReadScope.Read(model, beam, "station-samples", () => Index(beam));
            return index.TryGetValue(new Key(result.Dataset, result.Case, result.Station.Value, result.Side, result.Phase, result.Step, result.ConcomitantStateId, result.Mode, result.MovingLoadPosition), out var samples)
                ? samples : (IReadOnlyList<StationResultBeamForces>)None;
        }

        private static StationResultBeamForces[] Scan(BeamElement beam, CheckResult result) => beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().Where(r => r.State?.DatasetId == result.Dataset && r.Case?.Name == result.Case && r.ParametricDistance == result.Station && r.Side == result.Side && r.State.Phase == result.Phase && r.State.Step == result.Step && r.State.ConcomitantStateId == result.ConcomitantStateId && r.State.Mode == result.Mode && r.State.MovingLoadPosition == result.MovingLoadPosition).ToArray();

        // A sample without a state can only match a row without a dataset (scanned); an undefined distance matches no row.
        private static Dictionary<Key, List<StationResultBeamForces>> Index(BeamElement beam)
        {
            var index = new Dictionary<Key, List<StationResultBeamForces>>();
            foreach (var sample in beam.Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>())
            {
                var state = sample.State;
                if (state == null || double.IsNaN(sample.ParametricDistance))
                    continue;
                var key = new Key(state.DatasetId, sample.Case?.Name, sample.ParametricDistance, sample.Side, state.Phase, state.Step, state.ConcomitantStateId, state.Mode, state.MovingLoadPosition);
                if (!index.TryGetValue(key, out var samples))
                    index.Add(key, samples = new List<StationResultBeamForces>());
                samples.Add(sample);
            }

            return index;
        }

        private struct Key : IEquatable<Key>
        {
            private readonly string _dataset, _case, _phase, _step, _concomitant, _movingLoad;
            private readonly double _station;
            private readonly SectionSide _side;
            private readonly int? _mode;
            internal Key(string dataset, string caseName, double station, SectionSide side, string phase, string step, string concomitant, int? mode, string movingLoad)
            {
                // -0 and +0 are equal for ==: one key, one hash.
                _dataset = dataset; _case = caseName; _station = station == 0 ? 0.0 : station; _side = side;
                _phase = phase; _step = step; _concomitant = concomitant; _mode = mode; _movingLoad = movingLoad;
            }

            public bool Equals(Key other) => _station == other._station && _side == other._side && _mode == other._mode
                && string.Equals(_dataset, other._dataset) && string.Equals(_case, other._case) && string.Equals(_phase, other._phase)
                && string.Equals(_step, other._step) && string.Equals(_concomitant, other._concomitant) && string.Equals(_movingLoad, other._movingLoad);
            public override bool Equals(object obj) => obj is Key other && Equals(other);
            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = _station.GetHashCode();
                    hash = hash * 31 + (int)_side;
                    hash = hash * 31 + (_mode ?? -1);
                    hash = hash * 31 + Hash(_dataset); hash = hash * 31 + Hash(_case); hash = hash * 31 + Hash(_phase);
                    hash = hash * 31 + Hash(_step); hash = hash * 31 + Hash(_concomitant); hash = hash * 31 + Hash(_movingLoad);
                    return hash;
                }
            }
            private static int Hash(string text) => text == null ? 0 : StringComparer.Ordinal.GetHashCode(text);
        }
    }
}
