using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using GPC.Model.PostProcessing;

namespace GPC.Converter
{
    /// <summary>One linear combination of static load cases: a complete concomitant state of a source combination.</summary>
    public sealed class LinearAlternative
    {
        /// <summary>The choices made in the envelopes, e.g. "ENV_Q1=Q1.3+; ENV_G3_K0=SP_SOIL_SLU_2"; empty for a linear combination.</summary>
        public string Label { get; internal set; }
        /// <summary>Factor of each static load case (cases with a zero factor removed).</summary>
        public IReadOnlyDictionary<string, double> Factors { get; internal set; }
    }

    /// <summary>Source load combinations as linear combinations of static cases. Linear terms are summed (nested combinations included);
    /// an envelope gives one alternative for each of its terms, so a linear combination containing envelopes gives their cartesian product.
    /// Each alternative is a concomitant state: the extremes over the alternatives match the solver's component-wise max/min, but
    /// keep the governing state. Absolute and SRSS combinations, and terms of non-static analyses, are not linear and are rejected.</summary>
    public static class CombinationExpansion
    {
        public const string DefinitionsKind = "Load combination definitions";

        public static IReadOnlyList<LinearAlternative> Expand(IReadOnlyCollection<CombinationRecord> definitions, string id, int limit = 100000)
        {
            if (definitions == null || string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException();
            var byId = definitions.ToDictionary(d => d.Id, StringComparer.Ordinal);
            var memo = new Dictionary<string, List<Alternative>>(StringComparer.Ordinal);
            var result = Expand(byId, id, memo, new HashSet<string>(StringComparer.Ordinal), limit);
            return result.Select(a => new LinearAlternative { Label = a.Label, Factors = a.Factors.Where(p => p.Value != 0).ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal) }).ToArray();
        }

        /// <summary>The static cases a combination needs, from its terms and those of its nested combinations, without expanding the envelopes.
        /// Throws like <see cref="Expand"/> for combinations that are not linear superpositions of static cases.</summary>
        public static IReadOnlyList<string> StaticCases(IReadOnlyCollection<CombinationRecord> definitions, string id)
        {
            if (definitions == null || string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException();
            var byId = definitions.ToDictionary(d => d.Id, StringComparer.Ordinal);
            var cases = new List<string>(); var seen = new HashSet<string>(StringComparer.Ordinal); var done = new HashSet<string>(StringComparer.Ordinal);
            void Visit(string combination, HashSet<string> path)
            {
                if (done.Contains(combination)) return;
                if (!byId.TryGetValue(combination, out var definition)) throw new InvalidOperationException("UnknownCombination: " + combination);
                if (!path.Add(combination)) throw new InvalidOperationException("CyclicCombination: " + combination);
                if (definition.Kind == CombinationKind.Absolute || definition.Kind == CombinationKind.Srss)
                    throw new NotSupportedException("Combination " + definition.Name + " (" + definition.Kind + ") is not a linear superposition.");
                foreach (var term in definition.Terms)
                {
                    if (term.IsCombination) Visit(term.Name, path);
                    else if (term.IsStaticCase) { if (seen.Add(term.Name)) cases.Add(term.Name); }
                    else throw new NotSupportedException("Combination " + definition.Name + " has a " + term.Analysis + " term (" + term.Name + "), not a static case.");
                }
                path.Remove(combination); done.Add(combination);
            }
            Visit(id, new HashSet<string>(StringComparer.Ordinal));
            return cases;
        }

        /// <summary>The GPC combination of a definition with exactly one alternative; null otherwise.</summary>
        public static Combination ToCombination(IReadOnlyCollection<CombinationRecord> definitions, CombinationRecord definition, IReadOnlyDictionary<string, LoadCaseBase> cases, string name)
        {
            IReadOnlyList<LinearAlternative> alternatives;
            try { alternatives = Expand(definitions, definition.Id, 2); }
            catch (Exception ex) when (ex is NotSupportedException || ex is InvalidOperationException) { return null; }
            if (alternatives.Count != 1 || alternatives[0].Factors.Count == 0) return null;
            var combination = new Combination(name);
            foreach (var pair in alternatives[0].Factors)
                combination.AddLoadCaseCoefficient(cases.TryGetValue(pair.Key, out var loadCase) ? loadCase : throw new ArgumentException("CombinationCaseMissing: " + pair.Key), pair.Value);
            return combination;
        }

        /// <summary>The definitions preserved in a model imported by the converter.</summary>
        public static IReadOnlyList<CombinationRecord> Definitions(GPC.Model.Models.Model model)
        {
            var preserved = model?.PreservedSourceData.SingleOrDefault(p => p.Kind == DefinitionsKind);
            if (preserved == null) return new CombinationRecord[0];
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(preserved.RawData)))
                return (CombinationRecord[])new DataContractJsonSerializer(typeof(CombinationRecord[])).ReadObject(stream);
        }

        internal static PreservedAssignment Preserve(IEnumerable<CombinationRecord> definitions, string program)
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(CombinationRecord[])).WriteObject(stream, definitions.ToArray());
                return new PreservedAssignment { Kind = DefinitionsKind, SourceRecord = program, RawData = Encoding.UTF8.GetString(stream.ToArray()),
                    UnsupportedReason = "Source combinations; linear ones are also Model combinations, the others are expanded on demand (CombinationExpansion)." };
            }
        }

        private sealed class Alternative
        {
            public string Label;
            public Dictionary<string, double> Factors = new Dictionary<string, double>(StringComparer.Ordinal);
            public Alternative Scaled(double factor) => new Alternative { Label = Label, Factors = Factors.ToDictionary(p => p.Key, p => p.Value * factor, StringComparer.Ordinal) };
        }

        private static List<Alternative> Expand(Dictionary<string, CombinationRecord> byId, string id, Dictionary<string, List<Alternative>> memo, HashSet<string> path, int limit)
        {
            if (memo.TryGetValue(id, out var known)) return known;
            if (!byId.TryGetValue(id, out var definition)) throw new InvalidOperationException("UnknownCombination: " + id);
            if (!path.Add(id)) throw new InvalidOperationException("CyclicCombination: " + id);
            if (definition.Kind == CombinationKind.Absolute || definition.Kind == CombinationKind.Srss)
                throw new NotSupportedException("Combination " + definition.Name + " (" + definition.Kind + ") is not a linear superposition.");
            var parts = new List<List<Alternative>>();
            foreach (var term in definition.Terms)
            {
                if (double.IsNaN(term.Factor) || double.IsInfinity(term.Factor)) throw new InvalidOperationException("NonFiniteCombinationFactor: " + definition.Name);
                if (term.IsCombination) parts.Add(Expand(byId, term.Name, memo, path, limit).Select(a => a.Scaled(term.Factor)).ToList());
                else if (term.IsStaticCase)
                {
                    var single = new Alternative { Label = "" }; single.Factors[term.Name] = term.Factor;
                    parts.Add(new List<Alternative> { single });
                }
                else throw new NotSupportedException("Combination " + definition.Name + " has a " + term.Analysis + " term (" + term.Name + "), not a static case.");
            }
            path.Remove(id);
            List<Alternative> result;
            if (definition.Kind == CombinationKind.Envelope)
            {
                result = new List<Alternative>();
                for (int i = 0; i < definition.Terms.Count; i++)
                    foreach (var a in parts[i])
                    {
                        if (result.Count >= limit) throw new InvalidOperationException("CombinationExpansionLimit: " + definition.Name);
                        result.Add(new Alternative { Factors = a.Factors, Label = Join(definition.Name + "=" + definition.Terms[i].Name.Split('/').Last(), a.Label) });
                    }
            }
            else
            {
                result = new List<Alternative> { new Alternative { Label = "" } };
                foreach (var part in parts)
                {
                    if ((long)result.Count * part.Count > limit) throw new InvalidOperationException("CombinationExpansionLimit: " + definition.Name);
                    result = result.SelectMany(a => part.Select(b =>
                    {
                        var sum = new Alternative { Label = Join(a.Label, b.Label), Factors = new Dictionary<string, double>(a.Factors, StringComparer.Ordinal) };
                        foreach (var p in b.Factors) sum.Factors[p.Key] = (sum.Factors.TryGetValue(p.Key, out var v) ? v : 0) + p.Value;
                        return sum;
                    })).ToList();
                }
            }
            memo[id] = result;
            return result;
        }

        private static string Join(string a, string b) => string.IsNullOrEmpty(a) ? b ?? "" : string.IsNullOrEmpty(b) ? a : a + "; " + b;
    }
}
