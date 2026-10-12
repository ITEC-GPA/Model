using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GPC.Model.Core;
using GPC.Model.Stages;
using GPC.Model.Results.State;
using GPC.Model.Analysis;

namespace GPC.Converter.CivilNx
{
    /// <summary>Exact source labels. Step numbering is supplied by the caller, never guessed from the duration.</summary>
    public sealed class CivilNxStageStep
    {
        public string Phase { get; }
        public string Step { get; }
        public string RequestName => Phase + ":" + Step;
        public CivilNxStageStep(string phase, string step)
        {
            if (string.IsNullOrWhiteSpace(phase) || string.IsNullOrWhiteSpace(step) || phase.Contains(":") || step.Contains(":"))
                throw new ArgumentException("Unambiguous source phase and step labels required.");
            Phase = phase; Step = step;
        }
    }

    /// <summary>Explicit CS scope for both the request and its imported response. Stage outputs are not declared linear static combinations.</summary>
    public sealed class CivilNxStageSelection
    {
        private readonly CivilNxStageStep[] _steps;
        private readonly string[] _sourceCases;
        public IReadOnlyList<CivilNxStageStep> Steps => Array.AsReadOnly(_steps);
        public IReadOnlyList<string> SourceCases => Array.AsReadOnly(_sourceCases);
        public CivilNxDisplacementReference DisplacementReference { get; }
        /// <summary>Null unless established from the source analysis. Current/Real displacements are not labelled summable increments.</summary>
        public bool? ForceResultsAreCumulative { get; }
        public CivilNxStageSelection(IEnumerable<CivilNxStageStep> steps, IEnumerable<string> sourceCases,
            CivilNxDisplacementReference displacementReference, bool? forceResultsAreCumulative = null)
        {
            _steps = (steps ?? throw new ArgumentNullException(nameof(steps))).ToArray();
            _sourceCases = (sourceCases ?? throw new ArgumentNullException(nameof(sourceCases))).ToArray();
            if (_steps.Length == 0 || _steps.Any(s => s == null) || _steps.Select(s => s.RequestName).Distinct(StringComparer.Ordinal).Count() != _steps.Length)
                throw new ArgumentException("Explicit distinct stage steps required.", nameof(steps));
            if (_sourceCases.Length == 0 || _sourceCases.Any(string.IsNullOrWhiteSpace) || _sourceCases.Distinct(StringComparer.Ordinal).Count() != _sourceCases.Length)
                throw new ArgumentException("Explicit distinct stage output cases required.", nameof(sourceCases));
            if (!Enum.IsDefined(typeof(CivilNxDisplacementReference), displacementReference)) throw new ArgumentOutOfRangeException(nameof(displacementReference));
            DisplacementReference = displacementReference; ForceResultsAreCumulative = forceResultsAreCumulative;
        }

        internal Dictionary<string, StageResultCase> Validate(GPC.Model.Models.Model model)
        {
            foreach (var phase in _steps.Select(s => s.Phase).Distinct(StringComparer.Ordinal))
                if (model.Stages.Values.Count(s => s.Name == phase) != 1) throw new InvalidDataException("UnknownOrAmbiguousConstructionStage: " + phase);
            var cases = new Dictionary<string, StageResultCase>(StringComparer.Ordinal);
            foreach (var name in _sourceCases)
            {
                var found = model.LoadCases.Values.OfType<StageResultCase>().Where(c => c.SourceCaseName == name).ToArray();
                if (found.Length != 1) throw new InvalidDataException("UnknownOrAmbiguousStageOutputCase: " + name);
                cases.Add(name, found[0]);
            }
            return cases;
        }

        internal ResultState State(string sourceCase, string phase, string step, int count, bool displacement, string revision)
        {
            if (!_sourceCases.Contains(sourceCase, StringComparer.Ordinal) || !_steps.Any(s => s.Phase == phase && s.Step == step))
                throw new InvalidDataException("UnrequestedConstructionStageResult: " + sourceCase + " / " + phase + " / " + step);
            return new ResultState {
                // Even a geometrically linear staged analysis may include aging, creep and stiffness changes. No superposition assumption.
                Semantics = AnalysisSemantics.Unknown, Phase = phase, Step = step,
                HistoryId = "Civil NX construction " + revision,
                ConcomitantStateId = "Civil NX CS " + ModelValues.Fingerprint(new object[] { sourceCase, phase, step,
                    displacement ? DisplacementReference.ToString() : null }),
                IsCombined = false, IsCumulative = displacement ? (DisplacementReference == CivilNxDisplacementReference.Accumulative ? true : (bool?)null) : ForceResultsAreCumulative,
                SourceDisplacementReference = displacement ? DisplacementReference.ToString() : null,
                Components = Enumerable.Repeat(ComponentAvailability.Available, count).ToArray(),
                Coverage = "Explicit construction phase/step selection. Only returned active elements/nodes; absence is not a zero. No linear-static superposition assumption."
            };
        }
    }
}
