using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GPC.Model.Core;
using GPC.Model.Models;
using GPC.Model.Stages;

namespace GPC.Converter.Straus7
{
    public interface IStraus7StageWriteApi
    {
        /// <summary>Read-only preflight: no existing stages; exact native group/load/freedom case inventories.</summary>
        void RequireStageInputs(int[] groupIds, int[] loadCaseIds, int[] freedomCaseIds);
        void SetStagedAnalysis();
        void CreateStage(int number, string name, int[] flags, double? fluidLevelMm);
        void SetStageGroup(int stage, int group, bool active);
        void SetStageCase(int stage, int caseNumber, bool freedom, bool active);
        void SetStagePseudoTime(int stage, bool enabled);
        void AddStageIncrement(int stage, int increment, string name, double? pseudoTimeSeconds);
        void SetStageFactor(int stage, int increment, int caseNumber, bool freedom, double totalFactor);
    }

    /// <summary>Detached stage schedule for an existing topology. Maps contain every group and case in both
    /// models. Complete matrices explicitly write zero factors, so unloading cannot inherit a preceding load.
    /// The generic timed StageDefinition schedule is not silently approximated by native NLA increments.</summary>
    public sealed class Straus7StageWritePlan
    {
        private readonly Guid modelGuid;
        private readonly int[] groups, loads, freedoms;
        private readonly Action<IStraus7StageWriteApi>[] operations;
        public string AnalysisFingerprint { get; }
        public int StageCount { get; }
        public int IncrementCount { get; }
        public IReadOnlyDictionary<Guid, int> StageIds { get; }

        private Straus7StageWritePlan(Guid modelGuid, string fingerprint, int count, int increments,
            int[] groups, int[] loads, int[] freedoms, Dictionary<Guid, int> stageIds, List<Action<IStraus7StageWriteApi>> operations)
        {
            this.modelGuid = modelGuid; AnalysisFingerprint = fingerprint; StageCount = count; IncrementCount = increments;
            this.groups = groups; this.loads = loads; this.freedoms = freedoms; this.operations = operations.ToArray();
            StageIds = new ReadOnlyDictionary<Guid, int>(stageIds);
        }

        public static Straus7StageWritePlan Prepare(global::GPC.Model.Models.Model model,
            IReadOnlyDictionary<Guid, int> groupIds, IReadOnlyDictionary<Guid, int> loadCaseIds, IReadOnlyDictionary<Guid, int> freedomCaseIds,
            int? unassignedNativeGroup = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            model.RequireExportable(); // domain preflight of every exporter (review L4)
            return PrepareCore(model, groupIds, loadCaseIds, freedomCaseIds, unassignedNativeGroup);
        }

        /// <summary>The stage part of a full export, whose preflight already ran (Straus7ModelWritePlan.Prepare).</summary>
        internal static Straus7StageWritePlan PrepareCore(global::GPC.Model.Models.Model model,
            IReadOnlyDictionary<Guid, int> groupIds, IReadOnlyDictionary<Guid, int> loadCaseIds, IReadOnlyDictionary<Guid, int> freedomCaseIds,
            int? unassignedNativeGroup = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            StageAnalysisState.ValidateAll(model);
            int[] groups = Map(groupIds, model.Groups.Values.Select(g => g.Guid), "Group");
            if (unassignedNativeGroup.HasValue)
            {
                if (unassignedNativeGroup.Value < 1 || groups.Contains(unassignedNativeGroup.Value)) throw new ArgumentException("InvalidUnassignedNativeGroup");
                groups = groups.Concat(new[] { unassignedNativeGroup.Value }).OrderBy(id => id).ToArray();
                if (model.BeamElements.Values.Cast<GPC.Model.Elements.Element>().Concat(model.AreaElements.Values).Any(e => e.Groups.Count == 0))
                    throw new ArgumentException("StagedStructuralElementsRequireExplicitGroups");
            }
            int[] loads = Map(loadCaseIds, model.LoadCases.Values.Select(c => c.Guid), "LoadCase");
            int[] freedoms = Map(freedomCaseIds, model.FreedomCases.Values.Select(c => c.Guid), "FreedomCase");
            if (model.Stages.Count == 0 || model.Stages.Values.Any(s => s.AnalysisState == null))
                throw new ArgumentException("MissingStageAnalysisState");
            if (model.StageCombinationsMap.Count != 0) throw new NotSupportedException("LegacyStageCombinationMapRequiresExplicitMigration");
            var stages = model.Stages.Values.OrderBy(s => s.Definition.Sequence).ToArray();
            if (stages.Select(s => s.Definition.Sequence).Distinct().Count() != stages.Length)
                throw new ArgumentException("DuplicateStageSequence");
            if (stages.Select(s => s.Guid).Distinct().Count() != stages.Length) throw new ArgumentException("DuplicateStageIdentity");
            var operations = new List<Action<IStraus7StageWriteApi>>(); int increments = 0;
            for (int index = 0; index < stages.Length; index++)
            {
                var stage = stages[index]; var definition = stage.Definition;
                if (string.IsNullOrWhiteSpace(stage.Name)) throw new ArgumentException("MissingStageName");
                if (definition.DurationDays != 0 || definition.AdditionalOutputDays.Count != 0 || definition.GroupChanges.Count != 0
                    || definition.SaveResults.HasValue || definition.SaveIntermediateSteps.HasValue || definition.SaveLoadSteps.HasValue || definition.LoadSteps.HasValue)
                    throw new NotSupportedException("TimedStageScheduleRequiresSeparateConversion");
                var state = ModelValues.CopyValue(stage.AnalysisState); int number = index + 1; string name = stage.Name;
                int[] flags = { state.BirthGeometry == StageBirthGeometry.DeformedCoordinates ? 1 : 0,
                    state.MoveRestrainedNodes ? 1 : 0, state.RotateIncomingClusters ? 1 : 0, state.FluidLevelMm.HasValue ? 1 : 0,
                    state.BirthGeometry == StageBirthGeometry.ResetHistory ? 1 : 0 };
                operations.Add(a => a.CreateStage(number, name, (int[])flags.Clone(), state.FluidLevelMm));
                var activeGroups = new HashSet<int>(state.ActiveGroupGuids.Select(id => groupIds[id]));
                foreach (int id in groups) { bool active = activeGroups.Contains(id); operations.Add(a => a.SetStageGroup(number, id, active)); }
                var activeLoads = new HashSet<int>(state.Increments.SelectMany(i => i.LoadFactors).Select(f => loadCaseIds[f.CaseGuid]));
                var activeFreedoms = new HashSet<int>(state.ActiveFreedomCaseGuids.Select(id => freedomCaseIds[id]));
                bool timed = state.Increments[0].PseudoTimeSeconds.HasValue;
                operations.Add(a => a.SetStagePseudoTime(number, timed));
                for (int stepIndex = 0; stepIndex < state.Increments.Length; stepIndex++)
                {
                    var step = state.Increments[stepIndex]; int increment = stepIndex + 1; increments++;
                    operations.Add(a => a.AddStageIncrement(number, increment, step.Name, step.PseudoTimeSeconds));
                    var loadFactors = step.LoadFactors.ToDictionary(f => loadCaseIds[f.CaseGuid], f => f.Factor);
                    var freedomFactors = step.FreedomFactors.ToDictionary(f => freedomCaseIds[f.CaseGuid], f => f.Factor);
                    foreach (int id in loads)
                    { double value = loadFactors.TryGetValue(id, out var f) ? f : 0; operations.Add(a => a.SetStageFactor(number, increment, id, false, value)); }
                    foreach (int id in freedoms)
                    { double value = freedomFactors.TryGetValue(id, out var f) ? f : 0; operations.Add(a => a.SetStageFactor(number, increment, id, true, value)); }
                }
                // R31 initializes case selection when the FIRST increment is added, re-enabling cases.
                // Apply the authoritative selections after the grid exists (verified by native readback).
                foreach (int id in loads) { bool active = activeLoads.Contains(id); operations.Add(a => a.SetStageCase(number, id, false, active)); }
                foreach (int id in freedoms) { bool active = activeFreedoms.Contains(id); operations.Add(a => a.SetStageCase(number, id, true, active)); }
            }
            return new Straus7StageWritePlan(model.Guid, model.AnalysisFingerprint(), stages.Length, increments, groups, loads, freedoms,
                stages.Select((s, i) => new { s.Guid, Number = i + 1 }).ToDictionary(s => s.Guid, s => s.Number), operations);
        }

        private static int[] Map(IReadOnlyDictionary<Guid, int> map, IEnumerable<Guid> known, string kind)
        {
            var expected = known.ToArray();
            if (map == null || expected.Any(id => id == Guid.Empty) || expected.Distinct().Count() != expected.Length
                || map.Count != expected.Length || expected.Any(id => !map.ContainsKey(id))
                || map.Values.Any(id => id < 1) || map.Values.Distinct().Count() != map.Count)
                throw new ArgumentException("InvalidStrausStage" + kind + "Map");
            return map.Values.OrderBy(id => id).ToArray();
        }

        public void Write(IStraus7StageWriteApi api, global::GPC.Model.Models.Model currentInput)
        {
            if (api == null || currentInput == null) throw new ArgumentNullException();
            if (currentInput.Guid != modelGuid || currentInput.AnalysisFingerprint() != AnalysisFingerprint
                || currentInput.Stages.Count != StageIds.Count || currentInput.Stages.Values.Any(s => !StageIds.ContainsKey(s.Guid)))
                throw new InvalidOperationException("StaleStrausStagePlan");
            StageAnalysisState.ValidateAll(currentInput);
            api.RequireStageInputs((int[])groups.Clone(), (int[])loads.Clone(), (int[])freedoms.Clone());
            api.SetStagedAnalysis();
            foreach (var operation in operations) operation(api);
        }
    }
}
