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
        /// <summary>Reads construction metadata without requiring geometry or result tables; uses the same parser as the full model import.</summary>
        public ImportBatch ReadConstructionStages(CivilNxSnapshot snapshot, GPC.Model.Analysis.AnalysisSource identity,
            System.Threading.CancellationToken cancellationToken = default)
        {
            if (snapshot == null || identity == null || identity.Program != Program || string.IsNullOrWhiteSpace(identity.ModelRevision))
                throw new ArgumentException("ExplicitCivilNxModelIdentityRequired");
            return new Reader(snapshot, identity, cancellationToken, stageResultCases).ReadConstructionStages();
        }

        private sealed partial class Reader
        {
            public ImportBatch ReadConstructionStages()
            {
                Stages();
                foreach (var w in warnings) batch.Diagnostics.Add(new GPC.Model.Core.Diagnostics.ModelDiagnostic {
                    Code = w.Key.Split('\u001f')[0], Severity = GPC.Model.Core.Diagnostics.DiagnosticSeverity.Warning,
                    Record = w.Key.Split('\u001f')[1], Message = w.Key.Split('\u001f')[2] });
                return batch;
            }

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
                    var data = pair.Value; var record = Rec("STAG", pair.Key);
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
                Warn("CivilNxConstructionScheduleImported", Rec("STAG"), "Construction schedule imported as metadata; group operations are not permanent assignments. Result selection requires explicit phase and step.");
            }

            private static int StageId(string text) => int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0
                ? id : throw new InvalidDataException("InvalidConstructionStageId: " + text);
        }

        [DataContract] internal sealed class StageData
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
        [DataContract] internal sealed class ElementActivation
        {
            [DataMember(Name = "GRUP_NAME")] public string Group;
            [DataMember(Name = "AGE")] public double? Age;
        }
        [DataContract] internal sealed class ElementDeactivation
        {
            [DataMember(Name = "GRUP_NAME")] public string Group;
            [DataMember(Name = "REDIST")] public double? Redistribution;
        }
        [DataContract] internal sealed class BoundaryActivation
        {
            [DataMember(Name = "BNGR_NAME")] public string Group;
            [DataMember(Name = "POS")] public string Position;
        }
        [DataContract] internal sealed class LoadActivation
        {
            [DataMember(Name = "LOAD_NAME")] public string Group;
            [DataMember(Name = "DAY")] public string Day;
        }
        [DataContract] internal sealed class StageControlData
        {
            [DataMember(Name = "vEREC")] public StageErectionCase[] ErectionCases;
        }
        [DataContract] internal sealed class StageErectionCase
        {
            [DataMember(Name = "LTYPECC")] public string Name;
        }
    }
}
