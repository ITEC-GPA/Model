using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using GPC.Model.Analysis;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Results.State;
using GPC.Model.Stages;

namespace GPC.Converter.Straus7
{
    /// <summary>Results of a staged nonlinear static solution (Straus7 R3 NLA with stages): every primary result case is the state at the end of an
    /// increment of a stage, with all the loads applied up to it. The states are registered on the stage output case <see cref="NonlinearCase"/>
    /// (a <see cref="StageResultCase"/> declared at import when the model has stages) with Phase = the Model stage and Step = the increment, nonlinear
    /// static semantics and the units, axes and signs of <see cref="Straus7LinearStaticResults"/>. They are never superposed: no combination.</summary>
    public static class Straus7StageResults
    {
        public const string NonlinearCase = "Straus7 NLA (stages)", NonlinearSourceCase = "NLA";
        private static readonly Regex Increment = new Regex(@"^(.*?) \[[^\[\]]*\]\[[A-Z]\]$", RegexOptions.CultureInvariant | RegexOptions.Singleline);

        /// <summary>The increment of an NLA result case: Straus7 names it "&lt;case&gt;: &lt;increment&gt; [&lt;stage&gt;][&lt;U|M|R&gt;]"; the stage suffix is removed.</summary>
        public static string Step(Straus7ResultCase resultCase)
        {
            if (resultCase == null) throw new ArgumentNullException(nameof(resultCase));
            var name = (resultCase.Name ?? "").Trim(); var match = Increment.Match(name);
            return match.Success && match.Groups[1].Value.Trim().Length != 0 ? match.Groups[1].Value.Trim()
                : name.Length != 0 ? name : "Case " + resultCase.Number.ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>Normalizes the NLA cases of <paramref name="native"/> (solver 3, every case in a stage of the model) into <paramref name="datasetId"/>.</summary>
        public static ResultImportReport Import(GPC.Model.Models.Model model, Straus7ResultsReport native, string datasetId, bool isSynthetic = false,
            CancellationToken cancellationToken = default)
        {
            var rejected = new ResultImportReport { Status = ImportStatus.Rejected };
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (model == null || native == null) throw new ArgumentNullException();
                if (native.Status != ImportStatus.Completed || native.File == null || native.File.ValidationFlags != 0)
                    throw new InvalidDataException("Only successfully acquired, natively validated results can be normalized.");
                if (native.File.Solver != 3 || native.ModelStageCount < 1 || native.Cases.Count == 0 || native.Cases.Any(c => c.Stage < 1 || c.Stage > native.ModelStageCount))
                    throw new NotSupportedException("This profile supports the primary cases of a staged nonlinear static solution only.");
                if (native.Cases.Select(c => c.Number).Distinct().Count() != native.Cases.Count) throw new InvalidDataException("Duplicate result case metadata.");
                if (!model.LoadCases.TryGetValue(NonlinearCase, out var output) || !(output is StageResultCase))
                    throw new InvalidDataException("The Model has no stage output case: import the model with its stages.");
                var stages = Straus7ModelData.Read(model).Stages.ToDictionary(s => s.Number);
                foreach (var c in native.Cases)
                    if (!stages.TryGetValue(c.Stage, out var stage) || !model.Stages.Values.Any(s => s.Name == stage.ModelName))
                        throw new InvalidDataException("Result case " + c.Number + " refers to stage " + c.Stage + ", absent from the imported Model.");
                var steps = native.Cases.Select(c => (c.Stage, Step(c))).ToArray();
                if (steps.Distinct().Count() != steps.Length) throw new InvalidDataException("Two result cases have the same stage and increment.");
                return Straus7LinearStaticResults.Normalize(model, native, datasetId, c => NonlinearCase, (c, count) => State(c, stages[c.Stage].ModelName, count),
                    AnalysisSemantics.NonlinearStatic, "GPC.Straus7.R3.StagedNonlinear/1", isSynthetic, cancellationToken);
            }
            catch (OperationCanceledException) { rejected.Status = ImportStatus.Cancelled; }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidDataException || ex is InvalidOperationException || ex is NotSupportedException || ex is OverflowException)
            { rejected.Diagnostics.Add(ModelDiagnostic.Error("Straus7NormalizationRejected", message: ex.Message)); }
            return rejected;
        }

        private static ResultState State(Straus7ResultCase resultCase, string phase, int count) => new ResultState { Semantics = AnalysisSemantics.NonlinearStatic,
            Phase = phase, Step = Step(resultCase), HistoryId = "Straus7 NLA stages",
            ConcomitantStateId = "Straus7 NLA case " + resultCase.Number.ToString(CultureInfo.InvariantCulture), IsCombined = false, IsCumulative = true,
            Components = Enumerable.Repeat(ComponentAvailability.Available, count).ToArray(),
            Coverage = "State at the end of the increment, with the loads applied up to it; only the elements active in the stage have results. No superposition." };
    }
}
