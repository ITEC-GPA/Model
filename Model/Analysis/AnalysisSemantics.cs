using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.Analysis
{
    /// <summary>Meaning of an analysis dataset and of its result states. <see cref="FormFinding"/> (review A4): the node
    /// displacements are the move from the initial to the found equilibrium geometry and the forces are prestress, not a
    /// structural response to loads; structural consumers reject it (<see cref="AnalysisSemanticsRules.IsStructuralState"/>).</summary>
    public enum AnalysisSemantics { Unknown, LinearStatic, NonlinearStatic, ConcomitantEnvelopeState, IndependentExtrema, Modal, ResponseSpectrum, FormFinding }

    public static class AnalysisSemanticsRules
    {
        /// <summary>True for the structural equilibrium states the checks read: linear, nonlinear or a concomitant envelope state.
        /// A form-finding shape is never one.</summary>
        public static bool IsStructuralState(this AnalysisSemantics semantics) => semantics == AnalysisSemantics.LinearStatic
            || semantics == AnalysisSemantics.NonlinearStatic || semantics == AnalysisSemantics.ConcomitantEnvelopeState;

        /// <summary>The diagnostic for a state that is not structural: a form-finding shape has its own code.</summary>
        public static string NonStructuralCode(AnalysisSemantics? semantics, string fallback = "NonConcomitantState")
            => semantics == AnalysisSemantics.FormFinding ? "FormFindingResultIsNotStructural" : fallback;
    }
}
