using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Elements;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    /// <summary>An exact analysis-state selector. Null phase/step means the state has no phase/step, not a wildcard.</summary>
    [Serializable]
    public sealed class ResultSelection
    {
        public string Dataset { get; set; }
        public string Case { get; set; }
        public string Phase { get; set; }
        public string Step { get; set; }
        public string ConcomitantState { get; set; }
        public string MovingLoadPosition { get; set; }
        public int? Mode { get; set; }
        /// <summary>Combination category declared by the designer for this state. Null means undeclared; it is never deduced
        /// from the case name and does not take part in matching the samples.</summary>
        [field: System.Runtime.Serialization.OptionalField, Persistence.FingerprintWhenSet] public CombinationCategory? Category { get; set; }
        public ResultSelection Copy() => (ResultSelection)MemberwiseClone();
        internal bool Matches(ResultLocation value) => value.State != null && value.State.DatasetId == Dataset && value.Case?.Name == Case
            && value.State.Phase == Phase && value.State.Step == Step && value.State.MovingLoadPosition == MovingLoadPosition && value.State.Mode == Mode
            && (ConcomitantState == null || value.State.ConcomitantStateId == ConcomitantState);
    }

    public static class ResultQueries
    {
        /// <summary>Typed node/beam/shell samples referencing the existing dataset; no numeric data is copied or interpolated.</summary>
        public static IReadOnlyList<T> Samples<T>(Element element, ResultSelection selection) where T : ResultLocation
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            if (selection == null || string.IsNullOrWhiteSpace(selection.Dataset) || string.IsNullOrWhiteSpace(selection.Case)) throw new ArgumentException("Dataset and case required.");
            return element.Results.SelectMany(r => r.Results).OfType<T>().Where(selection.Matches).ToArray();
        }
        public static StationResultBeamForces BeamStation(BeamElement beam, ResultSelection selection, double station, SectionSide side)
        {
            NumericGuard.Station(station);
            var matches = Samples<StationResultBeamForces>(beam, selection).Where(r => r.ParametricDistance == station && r.Side == side).ToArray();
            if (matches.Length > 1) throw new InvalidOperationException("AmbiguousResultState: select a concomitant state explicitly.");
            return matches.SingleOrDefault();
        }
        public static NodeResultForces NodeForce(NodeElement node, ResultSelection selection, NodalForceKind kind, ActionBody body,
            ElementKey owner = null, string elementEnd = null, string aggregationSet = null)
        {
            var matches = Samples<NodeResultForces>(node, selection).Where(r => r.Kind == kind && r.Body == body
                && r.OwnerElementFamily == owner?.Family && r.OwnerElementId == owner?.Id && r.ElementEnd == elementEnd && r.AggregationSet == aggregationSet).ToArray();
            if (matches.Length > 1) throw new InvalidOperationException("AmbiguousNodalResultState");
            return matches.SingleOrDefault();
        }
    }
}
