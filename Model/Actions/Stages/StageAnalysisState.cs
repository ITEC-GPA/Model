using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Stages
{
    /// <summary>Birth configuration of incoming elements, or a fresh analysis with no inherited history.</summary>
    public enum StageBirthGeometry { OriginalCoordinates, DeformedCoordinates, ResetHistory }

    /// <summary>Dimensionless TOTAL factor at a load step, not a difference from the preceding step.</summary>
    [Serializable]
    public sealed class StageCaseFactor
    {
        public Guid CaseGuid { get; set; }
        public double Factor { get; set; }
    }

    /// <summary>Ordered equilibrium target. Missing load factors mean zero total load; missing freedom factors
    /// mean zero enforced displacement, without removing the stage's active restraints. Time is in seconds.</summary>
    [Serializable]
    public sealed class StageLoadIncrement
    {
        public string Name { get; set; }
        public StageCaseFactor[] LoadFactors { get; set; } = Array.Empty<StageCaseFactor>();
        public StageCaseFactor[] FreedomFactors { get; set; } = Array.Empty<StageCaseFactor>();
        public double? PseudoTimeSeconds { get; set; }
    }

    /// <summary>Physical configuration and ordered load history of one stage. Active group identifiers are an
    /// explicit complete set, not additions to the preceding stage; descendants are not implicitly activated.
    /// The sequence number belongs to Stage.Definition. Fluid level uses mm in the model's global vertical datum.</summary>
    [Serializable]
    public sealed class StageAnalysisState
    {
        public StageBirthGeometry BirthGeometry { get; set; }
        public bool MoveRestrainedNodes { get; set; }
        public bool RotateIncomingClusters { get; set; }
        public double? FluidLevelMm { get; set; }
        public Guid[] ActiveGroupGuids { get; set; } = Array.Empty<Guid>();
        public Guid[] ActiveFreedomCaseGuids { get; set; } = Array.Empty<Guid>();
        public StageLoadIncrement[] Increments { get; set; } = Array.Empty<StageLoadIncrement>();

        public void Validate(Models.Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (!Enum.IsDefined(typeof(StageBirthGeometry), BirthGeometry)) throw new ArgumentException("InvalidStageBirthGeometry");
            if (BirthGeometry == StageBirthGeometry.ResetHistory && (MoveRestrainedNodes || RotateIncomingClusters))
                throw new ArgumentException("ResetStageCannotMorphOrRotate");
            if (MoveRestrainedNodes && BirthGeometry != StageBirthGeometry.DeformedCoordinates)
                throw new ArgumentException("MovingRestrainedNodesRequiresDeformedBirthGeometry");
            if (FluidLevelMm.HasValue) Finite(FluidLevelMm.Value, "StageFluidLevel");
            Ids(ActiveGroupGuids, model.Groups.Values.Select(g => g.Guid), "StageGroups");
            Ids(ActiveFreedomCaseGuids, model.FreedomCases.Values.Select(c => c.Guid), "StageFreedomCases");
            if (Increments == null || Increments.Length == 0 || Increments.Any(i => i == null || string.IsNullOrWhiteSpace(i.Name)))
                throw new ArgumentException("MissingStageIncrements");
            bool timed = Increments[0].PseudoTimeSeconds.HasValue;
            double previous = double.NegativeInfinity;
            foreach (var step in Increments)
            {
                Factors(step.LoadFactors, model.LoadCases.Values.Select(c => c.Guid), "StageLoadFactors");
                Factors(step.FreedomFactors, ActiveFreedomCaseGuids, "StageFreedomFactors");
                if (step.PseudoTimeSeconds.HasValue != timed) throw new ArgumentException("IncompleteStagePseudoTimes");
                if (timed)
                {
                    double time = step.PseudoTimeSeconds.Value; Finite(time, "StagePseudoTime");
                    if (time < 0 || time < previous) throw new ArgumentException("InvalidStagePseudoTimeSequence");
                    previous = time;
                }
            }
        }

        /// <summary>True when a stage carries the physical stage state. A model without a stage collection has none.</summary>
        public static bool HasData(Models.Model model) => model?.Stages != null && model.Stages.Values.Any(s => s?.AnalysisState != null);

        /// <summary>Export preflight: throws on the first invalid stage state. Serialization, copies and fingerprints never call it.</summary>
        public static void ValidateAll(Models.Model model)
        {
            if (!HasData(model)) return;
            if (model.ValidateGroups().Count != 0) throw new ArgumentException("InvalidStageGroupGraph");
            foreach (var stage in model.Stages.Values)
            {
                if (stage?.AnalysisState == null) continue;
                if (stage.Definition == null) throw new ArgumentException("MissingStageSequence: " + stage.Name);
                stage.AnalysisState.Validate(model);
            }
        }

        /// <summary>The stage states that the writers reject, as diagnostics: dangling group, case and freedom case references after
        /// an edit leave the model saveable and are reported here (ModelEditSession.Validate) instead.</summary>
        public static IReadOnlyList<Core.Diagnostics.ModelDiagnostic> Diagnose(Models.Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            var result = new List<Core.Diagnostics.ModelDiagnostic>();
            if (!HasData(model)) return result;
            if (model.ValidateGroups().Count != 0) result.Add(Core.Diagnostics.ModelDiagnostic.Error("InvalidStageGroupGraph"));
            foreach (var stage in model.Stages.Values)
            {
                if (stage?.AnalysisState == null) continue;
                if (stage.Definition == null) { result.Add(Core.Diagnostics.ModelDiagnostic.Error("InvalidStageAnalysisState", message: "MissingStageSequence: " + stage.Name)); continue; }
                try { stage.AnalysisState.Validate(model); }
                catch (ArgumentException ex) { result.Add(Core.Diagnostics.ModelDiagnostic.Error("InvalidStageAnalysisState", message: stage.Name + ": " + ex.Message)); }
            }
            return result;
        }

        private static void Factors(StageCaseFactor[] factors, IEnumerable<Guid> known, string context)
        {
            if (factors == null || factors.Any(f => f == null)) throw new ArgumentException("Invalid" + context);
            Ids(factors.Select(f => f.CaseGuid).ToArray(), known, context);
            foreach (var factor in factors) Finite(factor.Factor, context);
        }

        private static void Ids(Guid[] ids, IEnumerable<Guid> known, string context)
        {
            var registry = new HashSet<Guid>(known);
            if (ids == null || ids.Any(id => id == Guid.Empty || !registry.Contains(id)) || ids.Distinct().Count() != ids.Length)
                throw new ArgumentException("Invalid" + context);
        }

        private static void Finite(double value, string context)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentException("NonFinite" + context);
        }
    }
}
