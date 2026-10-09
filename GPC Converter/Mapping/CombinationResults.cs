using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Model.Combinations;
using GPC.Model.Elements;
using GPC.Model.PostProcessing;

namespace GPC.Converter
{
    /// <summary>Combination results rebuilt from the static results read from a solver (<see cref="CombinationRebuild"/>): a source
    /// definition gives its linear alternatives (<see cref="CombinationExpansion"/>), a Model combination its own factors. The results go
    /// to the Model combination of the definition, declared at the import: with its factors for a linear combination, without factors
    /// for an envelope, whose governing alternatives become concomitant states.</summary>
    public static class CombinationResults
    {
        /// <summary>The Model combination that receives the results (null when not declared) and the linear alternatives of a combination
        /// given by source id, source name or Model name.</summary>
        public static (Combination Output, IReadOnlyList<CombinationAlternative> Alternatives) Resolve(GPC.Model.Models.Model model, string name, int limit = 100000)
        {
            if (model == null || string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException();
            var definitions = CombinationExpansion.Definitions(model);
            var definition = Definition(model, definitions, name);
            if (definition != null)
            {
                model.Combinations.TryGetValue(CombinationExpansion.ModelName(definitions, definition, model.LoadCases.ContainsKey), out var output);
                var alternatives = CombinationExpansion.Expand(definitions, definition.Id, limit).Select(a => new CombinationAlternative(a.Label, a.Factors)).ToArray();
                return (output, alternatives);
            }
            if (model.Combinations.TryGetValue(name, out var combination) && combination.LoadCaseCount != 0)
                return (combination, new[] { new CombinationAlternative("", combination.GetLoadCaseCoefficientsPair().ToDictionary(p => p.Key.Name, p => p.Value, StringComparer.Ordinal)) });
            throw new ArgumentException("UnknownCombination: " + name);
        }

        /// <summary>The source definition named by id, by source name (when unique) or by its Model name; null for Model combinations.</summary>
        internal static CombinationRecord Definition(GPC.Model.Models.Model model, IReadOnlyList<CombinationRecord> definitions, string name)
        {
            var byId = definitions.FirstOrDefault(d => d.Id == name); if (byId != null) return byId;
            var byName = definitions.Where(d => d.Name == name).ToArray();
            if (byName.Length > 1) throw new ArgumentException("AmbiguousCombinationName: " + name + "; select it by id.");
            if (byName.Length == 1) return byName[0];
            return definitions.FirstOrDefault(d => CombinationExpansion.ModelName(definitions, d, model.LoadCases.ContainsKey) == name);
        }

        /// <summary>Declares the Model combinations of source definitions missing from the model (models imported before envelopes were declared).
        /// Declaring changes the analysis fingerprint: do it before reading the static results, which are bound to it.</summary>
        public static IReadOnlyList<ModelDiagnostic> Declare(GPC.Model.Models.Model model, IEnumerable<string> names)
        {
            if (model == null || names == null) throw new ArgumentNullException();
            var definitions = CombinationExpansion.Definitions(model); var diagnostics = new List<ModelDiagnostic>(); int declared = 0;
            foreach (var name in names.Distinct(StringComparer.Ordinal))
            {
                var definition = Definition(model, definitions, name);
                if (definition == null) { if (!model.Combinations.ContainsKey(name)) throw new ArgumentException("UnknownCombination: " + name); continue; }
                var modelName = CombinationExpansion.ModelName(definitions, definition, model.LoadCases.ContainsKey);
                if (model.Combinations.ContainsKey(modelName)) continue;
                var cases = model.LoadCases.Values.ToDictionary(c => c.Name, StringComparer.Ordinal);
                var combination = CombinationExpansion.ToCombination(definitions, definition, cases, modelName)
                    ?? (CombinationExpansion.IsExpandable(definitions, definition.Id) ? new Combination(modelName) : throw new NotSupportedException("Combination " + name + " cannot be rebuilt from static cases."));
                model.AddCombination(combination); declared++;
            }
            if (declared != 0 && model.Datasets.Count != 0)
                diagnostics.Add(new ModelDiagnostic { Code = "CombinationsDeclaredAfterResults", Severity = DiagnosticSeverity.Warning,
                    Message = "Combinations declared after results were imported: those datasets no longer match the model; read the static results again." });
            return diagnostics;
        }

        /// <summary>Rebuilds the combinations of a read plan at the beams, plates and nodes of the plan from the static results of a dataset.</summary>
        public static IReadOnlyList<CombinationRebuildReport> Rebuild(GPC.Model.Models.Model model, ResultReadPlan plan, string datasetId, bool attach = true,
            CancellationToken cancellationToken = default)
        {
            if (model == null || plan == null) throw new ArgumentNullException();
            var elements = plan.Beams.Cast<Element>().Concat(plan.Shells).Concat(plan.Nodes).ToArray();
            return plan.Combinations.Select(name => Rebuild(model, name, elements, datasetId, attach, cancellationToken)).ToArray();
        }

        public static CombinationRebuildReport Rebuild(GPC.Model.Models.Model model, string combination, IEnumerable<Element> elements, string datasetId, bool attach = true,
            CancellationToken cancellationToken = default)
        {
            var (output, alternatives) = Resolve(model, combination);
            if (output == null) throw new InvalidOperationException("DeclareOutputCaseBeforeAnalysis: " + combination + " (CombinationResults.Declare, then read the static results).");
            return CombinationRebuild.Rebuild(model, output, alternatives, datasetId, elements, attach, cancellationToken);
        }
    }
}
