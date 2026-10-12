using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Stages
{
    /// <summary>Source construction schedule, in days. No activation is applied to the permanent model by this value.</summary>
    [Serializable]
    public sealed class StageDefinition : ISerializable
    {
        private readonly double[] _additionalOutputDays;
        private readonly StageGroupChange[] _groupChanges;
        public int Sequence { get; }
        public double DurationDays { get; }
        public bool? SaveResults { get; }
        public bool? SaveIntermediateSteps { get; }
        public bool? SaveLoadSteps { get; }
        public int? LoadSteps { get; }
        public string SourceRecord { get; }
        public IReadOnlyList<double> AdditionalOutputDays => Array.AsReadOnly(_additionalOutputDays);
        public IReadOnlyList<StageGroupChange> GroupChanges => Array.AsReadOnly(_groupChanges);

        public StageDefinition(int sequence, double durationDays, IEnumerable<double> additionalOutputDays = null,
            IEnumerable<StageGroupChange> groupChanges = null, bool? saveResults = null, bool? saveIntermediateSteps = null,
            bool? saveLoadSteps = null, int? loadSteps = null, string sourceRecord = null)
        {
            if (sequence < 1) throw new ArgumentOutOfRangeException(nameof(sequence));
            if (NumericGuard.Finite(durationDays, nameof(durationDays)) < 0) throw new ArgumentOutOfRangeException(nameof(durationDays));
            if (loadSteps.HasValue && loadSteps.Value < 0) throw new ArgumentOutOfRangeException(nameof(loadSteps));
            Sequence = sequence; DurationDays = durationDays; SaveResults = saveResults;
            SaveIntermediateSteps = saveIntermediateSteps; SaveLoadSteps = saveLoadSteps; LoadSteps = loadSteps; SourceRecord = sourceRecord;
            _additionalOutputDays = (additionalOutputDays ?? Enumerable.Empty<double>()).ToArray();
            if (_additionalOutputDays.Any(d => NumericGuard.Finite(d, nameof(additionalOutputDays)) < 0 || d > durationDays)
                || _additionalOutputDays.Distinct().Count() != _additionalOutputDays.Length)
                throw new ArgumentException("Output days must be distinct and inside the stage duration.", nameof(additionalOutputDays));
            _groupChanges = (groupChanges ?? Enumerable.Empty<StageGroupChange>()).ToArray();
            if (_groupChanges.Any(c => c == null || c.Day > durationDays))
                throw new ArgumentException("Group changes must be non-null and inside the stage duration.", nameof(groupChanges));
        }

        private StageDefinition(SerializationInfo info, StreamingContext context)
            : this(Versioned(info).GetInt32("Sequence"), info.GetDouble("DurationDays"),
                (double[])info.GetValue("AdditionalOutputDays", typeof(double[])),
                (StageGroupChange[])info.GetValue("GroupChanges", typeof(StageGroupChange[])),
                SerializationFields.Read<bool?>(info, "SaveResults"), SerializationFields.Read<bool?>(info, "SaveIntermediateSteps"),
                SerializationFields.Read<bool?>(info, "SaveLoadSteps"), SerializationFields.Read<int?>(info, "LoadSteps"), info.GetString("SourceRecord")) { }
        private static SerializationInfo Versioned(SerializationInfo info)
        { SerializationVersions.Read(info, "StageDefinitionVersion", 1); return info; }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("StageDefinitionVersion", 1); info.AddValue("Sequence", Sequence); info.AddValue("DurationDays", DurationDays);
            info.AddValue("AdditionalOutputDays", _additionalOutputDays); info.AddValue("GroupChanges", _groupChanges);
            info.AddValue("SaveResults", SaveResults); info.AddValue("SaveIntermediateSteps", SaveIntermediateSteps);
            info.AddValue("SaveLoadSteps", SaveLoadSteps); info.AddValue("LoadSteps", LoadSteps); info.AddValue("SourceRecord", SourceRecord);
        }
    }

    public enum StageGroupKind { Structure, Boundary, Load }
    public enum StageGroupAction { Activate, Deactivate }
    public enum StageEventTiming { Unspecified, First, Last, Day }
    public enum StageBoundaryPosition { Unspecified, Original, Deformed }

    /// <summary>One explicit source group operation. Element age is not the elapsed day of the stage. Redistribution is a ratio (0..1).</summary>
    [Serializable]
    public sealed class StageGroupChange : ISerializable
    {
        public string GroupName { get; }
        public StageGroupKind Kind { get; }
        public StageGroupAction Action { get; }
        public StageEventTiming Timing { get; }
        public double? Day { get; }
        public double? ElementAgeDays { get; }
        public double? RedistributionRatio { get; }
        public StageBoundaryPosition BoundaryPosition { get; }

        public StageGroupChange(string groupName, StageGroupKind kind, StageGroupAction action,
            StageEventTiming timing = StageEventTiming.Unspecified, double? day = null, double? elementAgeDays = null,
            double? redistributionRatio = null, StageBoundaryPosition boundaryPosition = StageBoundaryPosition.Unspecified)
        {
            if (string.IsNullOrWhiteSpace(groupName)) throw new ArgumentException("Explicit group name required.", nameof(groupName));
            if (!Enum.IsDefined(typeof(StageGroupKind), kind) || !Enum.IsDefined(typeof(StageGroupAction), action)
                || !Enum.IsDefined(typeof(StageEventTiming), timing) || !Enum.IsDefined(typeof(StageBoundaryPosition), boundaryPosition))
                throw new ArgumentException("Unknown construction stage operation.");
            if ((timing == StageEventTiming.Day) != day.HasValue) throw new ArgumentException("A day is required only for a timed operation.", nameof(day));
            if (day.HasValue && NumericGuard.Finite(day.Value, nameof(day)) < 0) throw new ArgumentOutOfRangeException(nameof(day));
            if (elementAgeDays.HasValue && (kind != StageGroupKind.Structure || action != StageGroupAction.Activate
                || NumericGuard.Finite(elementAgeDays.Value, nameof(elementAgeDays)) < 0)) throw new ArgumentOutOfRangeException(nameof(elementAgeDays));
            if (redistributionRatio.HasValue && (kind != StageGroupKind.Structure || action != StageGroupAction.Deactivate
                || NumericGuard.Finite(redistributionRatio.Value, nameof(redistributionRatio)) < 0 || redistributionRatio.Value > 1))
                throw new ArgumentOutOfRangeException(nameof(redistributionRatio));
            if (boundaryPosition != StageBoundaryPosition.Unspecified && (kind != StageGroupKind.Boundary || action != StageGroupAction.Activate))
                throw new ArgumentException("A boundary position applies only to boundary activation.", nameof(boundaryPosition));
            GroupName = groupName; Kind = kind; Action = action; Timing = timing; Day = day;
            ElementAgeDays = elementAgeDays; RedistributionRatio = redistributionRatio; BoundaryPosition = boundaryPosition;
        }

        private StageGroupChange(SerializationInfo info, StreamingContext context)
            : this(Versioned(info).GetString("GroupName"), (StageGroupKind)info.GetInt32("Kind"), (StageGroupAction)info.GetInt32("Action"),
                (StageEventTiming)info.GetInt32("Timing"), SerializationFields.Read<double?>(info, "Day"),
                SerializationFields.Read<double?>(info, "ElementAgeDays"), SerializationFields.Read<double?>(info, "RedistributionRatio"),
                (StageBoundaryPosition)info.GetInt32("BoundaryPosition")) { }
        private static SerializationInfo Versioned(SerializationInfo info)
        { SerializationVersions.Read(info, "StageGroupChangeVersion", 1); return info; }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("StageGroupChangeVersion", 1); info.AddValue("GroupName", GroupName); info.AddValue("Kind", (int)Kind);
            info.AddValue("Action", (int)Action); info.AddValue("Timing", (int)Timing); info.AddValue("Day", Day);
            info.AddValue("ElementAgeDays", ElementAgeDays); info.AddValue("RedistributionRatio", RedistributionRatio);
            info.AddValue("BoundaryPosition", (int)BoundaryPosition);
        }
    }
}
