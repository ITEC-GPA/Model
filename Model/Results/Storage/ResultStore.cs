using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Elements;
using GPC.Model.Core;
using GPC.Model.PostProcessing;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.Results
{
    /// <summary>Immutable identity of one exported result location. Distinct faces, ends, averaging regions and states never coalesce.</summary>
    public sealed class ResultAddress : IEquatable<ResultAddress>
    {
        public Guid ElementGuid { get; }
        public EntityFamily Family { get; }
        public int ElementId { get; }
        public string Dataset { get; }
        public string Case { get; }
        public string Phase { get; }
        public string Step { get; }
        public string ConcomitantState { get; }
        public string MovingLoadPosition { get; }
        public int? Mode { get; }
        public string ResultKind { get; }
        public string PositionKey { get; }
        private readonly string _key;
        internal ResultAddress(Guid element, EntityFamily family, int id, ResultLocation value)
        {
            ElementGuid = element; Family = family; ElementId = id;
            Dataset = value.State?.DatasetId; Case = value.Case?.Name; Phase = value.State?.Phase; Step = value.State?.Step;
            ConcomitantState = value.State?.ConcomitantStateId; MovingLoadPosition = value.State?.MovingLoadPosition; Mode = value.State?.Mode;
            ResultKind = value.GetType().FullName;
            PositionKey = ModelValues.Fingerprint(Position(value));
            _key = ModelValues.Fingerprint(new object[] { "ResultAddress-v1", element, family, id, Dataset, Case, Phase, Step,
                ConcomitantState, MovingLoadPosition, Mode, ResultKind, PositionKey });
        }
        internal bool Matches(ResultSelection selection) => Dataset == selection.Dataset && Case == selection.Case
            && Phase == selection.Phase && Step == selection.Step && MovingLoadPosition == selection.MovingLoadPosition && Mode == selection.Mode
            && (selection.ConcomitantState == null || ConcomitantState == selection.ConcomitantState);
        private static IEnumerable<object> Position(ResultLocation value)
        {
            if (value is IBeamResultLocation beam) yield return beam.ParametricDistance;
            if (value is StationResultBeamForces forces) { yield return forces.StationDomain; yield return forces.Side; yield return forces.Body; yield return forces.PhysicalDistance; }
            if (value is StationResultDisplacement displacement) { yield return displacement.StationDomain; yield return displacement.Side; yield return displacement.PhysicalDistance; }
            if (value is IPlateResultLocation plate) yield return plate.Location;
            if (value is IBrickResultLocation solid) yield return solid.Location;
            if (value is PointResultPlateForces shell)
            {
                yield return shell.LocationKind; yield return shell.PointKind; yield return shell.CoordinateKind;
                yield return shell.GlobalLocation; yield return shell.SourceNodeId; yield return shell.AveragingRegion;
            }
            if (value is NodeResultForces node)
            {
                yield return node.Kind; yield return node.Body; yield return node.OwnerElementFamily;
                yield return node.OwnerElementId; yield return node.ElementEnd; yield return node.AggregationSet;
            }
            // Other legacy locations retain all their explicit serialized location fields.
            // The result GUID distinguishes otherwise unlocated legacy exports, never numeric values.
            if (!(value is IBeamResultLocation) && !(value is IPlateResultLocation) && !(value is INodeResultLocation) && !(value is IBrickResultLocation)) yield return value.Guid;
        }
        public bool Equals(ResultAddress other) => other != null && _key == other._key;
        public override bool Equals(object obj) => Equals(obj as ResultAddress);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(_key);
    }

    /// <summary>A detached immutable record. Read returns a copy, so clients cannot invalidate an index by editing a sample.</summary>
    public sealed class ResultRecord
    {
        private readonly byte[] _payload;
        private readonly Type _type;
        public ResultAddress Address { get; }
        public string ContentFingerprint { get; }
        internal ResultRecord(Guid element, EntityFamily family, int id, ResultLocation sample)
        {
            _payload = AnalysisStorage.Write(sample); _type = sample.GetType();
            var copy = Read(); Address = new ResultAddress(element, family, id, copy);
            ContentFingerprint = ModelValues.Fingerprint(new object[] { copy });
        }
        public ResultLocation Read() => AnalysisStorage.Read<ResultLocation>(_payload);
        public T Read<T>() where T : ResultLocation => Read() as T ?? throw new InvalidOperationException("DifferentResultType");
        internal bool Is<T>() where T : ResultLocation => typeof(T).IsAssignableFrom(_type);
    }

    public interface IResultStore
    {
        IReadOnlyList<ResultRecord> Records { get; }
        IReadOnlyList<ResultRecord> Query(Guid element, ResultSelection selection);
        IReadOnlyList<ResultRecord> At(ResultAddress address);
    }

    /// <summary>Snapshot adapter over legacy element lists. Duplicate exports remain visible and never overwrite one another.</summary>
    public sealed class InMemoryResultStore : IResultStore
    {
        private readonly Dictionary<Tuple<Guid, string, string>, ResultRecord[]> _states;
        private readonly Dictionary<ResultAddress, ResultRecord[]> _positions;
        public IReadOnlyList<ResultRecord> Records { get; }
        private InMemoryResultStore(ResultRecord[] records)
        {
            Records = Array.AsReadOnly(records);
            _states = records.GroupBy(r => Tuple.Create(r.Address.ElementGuid, r.Address.Dataset, r.Address.Case)).ToDictionary(g => g.Key, g => g.ToArray());
            _positions = records.GroupBy(r => r.Address).ToDictionary(g => g.Key, g => g.ToArray());
        }
        public static InMemoryResultStore Capture(Models.Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            return new InMemoryResultStore(model.AllElements.SelectMany(e => e.Results.SelectMany(r => r.Results)
                .Select(r => new ResultRecord(e.Guid, Models.Model.FamilyOf(e), e.Id, r))).ToArray());
        }
        public IReadOnlyList<ResultRecord> Query(Guid element, ResultSelection selection)
        {
            if (selection == null || string.IsNullOrWhiteSpace(selection.Dataset) || string.IsNullOrWhiteSpace(selection.Case))
                throw new ArgumentException("Dataset and case required.");
            return _states.TryGetValue(Tuple.Create(element, selection.Dataset, selection.Case), out var records)
                ? Array.AsReadOnly(records.Where(r => r.Address.Matches(selection)).ToArray()) : (IReadOnlyList<ResultRecord>)Array.Empty<ResultRecord>();
        }
        public IReadOnlyList<ResultRecord> At(ResultAddress address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            return _positions.TryGetValue(address, out var records) ? Array.AsReadOnly(records) : (IReadOnlyList<ResultRecord>)Array.Empty<ResultRecord>();
        }
    }
}
