using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.Stages;

namespace GPC.Converter.CivilNx
{
    public sealed partial class CivilNxModelProfile
    {
        private sealed partial class Reader
        {
            private void Stages()
            {
                var rows = Rows<StageData>("STAG");
                if (rows.Count == 0)
                {
                    if (stageResultCases.Length != 0) throw new InvalidDataException("StageCasesWithoutConstructionStages");
                    return;
                }
                int sequence = 0;
                foreach (var pair in rows.OrderBy(p => StageId(p.Key)))
                {
                    token.ThrowIfCancellationRequested();
                    var data = pair.Value; var record = "STAG/" + pair.Key;
                    if (string.IsNullOrWhiteSpace(data?.Name)) throw new InvalidDataException("MissingConstructionStageName: " + record);
                    var stage = new Stage(StageId(pair.Key), data.Name); batch.Stages.Add(stage); sequence++;
                    if (!data.Duration.HasValue)
                    {
                        Warn("CivilNxStageDefinitionPreserved", record, "Stage name retained; missing duration prevents interpreting its schedule.");
                        continue;
                    }
                    var changes = new System.Collections.Generic.List<StageGroupChange>();
                    foreach (var item in data.ActivateElements ?? new ElementActivation[0])
                        changes.Add(new StageGroupChange(item?.Group, StageGroupKind.Structure, StageGroupAction.Activate, elementAgeDays: item?.Age));
                    foreach (var item in data.DeactivateElements ?? new ElementDeactivation[0])
                        changes.Add(new StageGroupChange(item?.Group, StageGroupKind.Structure, StageGroupAction.Deactivate, redistributionRatio: item?.Redistribution / 100.0));
                    foreach (var item in data.ActivateBoundaries ?? new BoundaryActivation[0])
                    {
                        var position = item?.Position == "DEFORMED" ? StageBoundaryPosition.Deformed : item?.Position == "ORIGINAL" ? StageBoundaryPosition.Original
                            : item?.Position == null ? StageBoundaryPosition.Unspecified : throw new InvalidDataException("UnknownBoundaryActivationPosition: " + record);
                        changes.Add(new StageGroupChange(item?.Group, StageGroupKind.Boundary, StageGroupAction.Activate, boundaryPosition: position));
                    }
                    foreach (var group in data.DeactivateBoundaries ?? new string[0])
                        changes.Add(new StageGroupChange(group, StageGroupKind.Boundary, StageGroupAction.Deactivate));
                    void Loads(LoadActivation[] loads, StageGroupAction action)
                    {
                        foreach (var item in loads ?? new LoadActivation[0])
                        {
                            var timing = item?.Day == "FIRST" ? StageEventTiming.First : item?.Day == "LAST" ? StageEventTiming.Last : StageEventTiming.Day;
                            double? day = null;
                            if (timing == StageEventTiming.Day)
                            {
                                if (!double.TryParse(item?.Day, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                                    throw new InvalidDataException("InvalidStageLoadDay: " + record);
                                day = value;
                            }
                            changes.Add(new StageGroupChange(item?.Group, StageGroupKind.Load, action, timing, day));
                        }
                    }
                    Loads(data.ActivateLoads, StageGroupAction.Activate); Loads(data.DeactivateLoads, StageGroupAction.Deactivate);
                    stage.Definition = new StageDefinition(sequence, data.Duration.Value, data.AdditionalDays, changes,
                        data.SaveResults, data.SaveSteps, data.SaveLoadSteps, data.LoadSteps, record);
                }
                var controls = Rows<StageControlData>("STCT");
                var names = new[] { "Summation" }.Concat(stageResultCases).Concat(controls.Values
                    .SelectMany(c => c?.ErectionCases ?? new StageErectionCase[0]).Select(c => c?.Name)).Distinct(StringComparer.Ordinal);
                foreach (var name in names)
                {
                    if (string.IsNullOrWhiteSpace(name)) throw new InvalidDataException("MissingConstructionStageCaseName");
                    var modelName = name + "(CS)";
                    if (batch.LoadCases.Any(c => c.Name == modelName)) throw new InvalidDataException("ConstructionStageCaseNameCollision: " + modelName);
                    batch.LoadCases.Add(new LoadCaseRecord { Name = modelName, StageSourceCaseName = name, Record = "STAG/STCT result scope" });
                }
                Warn("CivilNxConstructionScheduleImported", "STAG", "Construction schedule imported as metadata; group operations are not permanent assignments. Result selection requires explicit phase and step.");
            }

            private static int StageId(string text) => int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0
                ? id : throw new InvalidDataException("InvalidConstructionStageId: " + text);
        }

        [DataContract] private sealed class StageData
        {
            [DataMember(Name = "NAME")] public string Name;
            [DataMember(Name = "DURATION")] public double? Duration;
            [DataMember(Name = "bSV_RSLT")] public bool? SaveResults;
            [DataMember(Name = "bSV_STEP")] public bool? SaveSteps;
            [DataMember(Name = "bLOAD_STEP")] public bool? SaveLoadSteps;
            [DataMember(Name = "INCRE_STEP")] public int? LoadSteps;
            [DataMember(Name = "ADD_STEP")] public double[] AdditionalDays;
            [DataMember(Name = "ACT_ELEM")] public ElementActivation[] ActivateElements;
            [DataMember(Name = "DACT_ELEM")] public ElementDeactivation[] DeactivateElements;
            [DataMember(Name = "ACT_BNGR")] public BoundaryActivation[] ActivateBoundaries;
            [DataMember(Name = "DACT_BNGR")] public string[] DeactivateBoundaries;
            [DataMember(Name = "ACT_LOAD")] public LoadActivation[] ActivateLoads;
            [DataMember(Name = "DACT_LOAD")] public LoadActivation[] DeactivateLoads;
        }
        [DataContract] private sealed class ElementActivation
        {
            [DataMember(Name = "GRUP_NAME")] public string Group;
            [DataMember(Name = "AGE")] public double? Age;
        }
        [DataContract] private sealed class ElementDeactivation
        {
            [DataMember(Name = "GRUP_NAME")] public string Group;
            [DataMember(Name = "REDIST")] public double? Redistribution;
        }
        [DataContract] private sealed class BoundaryActivation
        {
            [DataMember(Name = "BNGR_NAME")] public string Group;
            [DataMember(Name = "POS")] public string Position;
        }
        [DataContract] private sealed class LoadActivation
        {
            [DataMember(Name = "LOAD_NAME")] public string Group;
            [DataMember(Name = "DAY")] public string Day;
        }
        [DataContract] private sealed class StageControlData
        {
            [DataMember(Name = "vEREC")] public StageErectionCase[] ErectionCases;
        }
        [DataContract] private sealed class StageErectionCase
        {
            [DataMember(Name = "LTYPECC")] public string Name;
        }
    }
}
