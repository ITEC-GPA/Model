using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GPC.Model.Standards;

namespace GPC.Model.Design.Combinations
{
    /// <summary>Nature of a characteristic action of a geotechnical combination (NTC 2018 §2.5.3 and Tab. 6.2.I): G1 structural and soil permanent,
    /// G2 non-structural permanent, Q variable, A accidental.</summary>
    public enum GeotechnicalActionNature { G1, G2, Q, A }

    /// <summary>Limit state of a generated geotechnical combination.</summary>
    public enum GeotechnicalLimitState { Ultimate, Characteristic, Frequent, QuasiPermanent, Seismic, Exceptional }

    /// <summary>
    /// A characteristic action of the combinations: id, group of the correlated actions (one presence and one lead in the enumeration, so the
    /// actions of a group share the nature), nature and 0 ≤ ψ2 ≤ ψ1 ≤ ψ0 ≤ 1 (used only for Q).
    /// </summary>
    public sealed class GeotechnicalDesignAction
    {
        public string Id { get; }
        public string Group { get; }
        public GeotechnicalActionNature Nature { get; }
        public double Psi0 { get; }
        public double Psi1 { get; }
        public double Psi2 { get; }
        public GeotechnicalDesignAction(string id, string group, GeotechnicalActionNature nature, double psi0 = 0, double psi1 = 0, double psi2 = 0)
        {
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(group)) throw new ArgumentException("Action id and group required.");
            if (!Enum.IsDefined(typeof(GeotechnicalActionNature), nature)) throw new ArgumentOutOfRangeException(nameof(nature));
            if (!(psi2 >= 0 && psi2 <= psi1 && psi1 <= psi0 && psi0 <= 1)) throw new ArgumentOutOfRangeException(nameof(psi0), "0 ≤ ψ2 ≤ ψ1 ≤ ψ0 ≤ 1 required.");
            Id = id; Group = group; Nature = nature; Psi0 = psi0; Psi1 = psi1; Psi2 = psi2;
        }
    }

    /// <summary>
    /// What to generate. <see cref="Ultimate"/> is the action set of the ultimate rows, for example A1 of the combination of the check from
    /// <see cref="StandardGeotechnical.Combinations"/> or the structural set of a <see cref="CombinationProfile"/>; a caller that departs from the
    /// standard passes its own set with its own name. Bodies are permanent weights with their own factor (structure, fill, water, ...), favourable
    /// or unfavourable with the G1 values of the set: <see cref="Bodies"/> enter the pattern of the ultimate rows, <see cref="RowBodies"/> double
    /// every ultimate row (favourable first). <see cref="SeismicVariants"/> is the number of seismic variants (the caller keeps kh, kv, the seismic
    /// actions and the use of each one).
    /// <para>
    /// <see cref="VariablePresence"/>: the seismic and exceptional rows enumerate the presence of every variable group, as the ultimate rows do
    /// (ψ2 Qk only where unfavourable, NTC 2018 §2.5.3; default), or have one row with every variable action at ψ2 (the fixed rules of an older
    /// generator, for example the version 1 documents of the ANTHEA walls). <see cref="PatternOrder"/>: the order of the bits of the ultimate
    /// pattern, fastest first, as names of the bodies and keys of the G1, G2 and Q groups; empty for the default order (bodies, then permanent
    /// groups, then variable groups, each in the order of the request).
    /// </para>
    /// </summary>
    public sealed class GeotechnicalCombinationRequest
    {
        public GeotechnicalActionFactors Ultimate { get; }
        public IReadOnlyList<GeotechnicalDesignAction> Actions { get; }
        public IReadOnlyList<string> Bodies { get; }
        public IReadOnlyList<string> RowBodies { get; }
        public int SeismicVariants { get; }
        public bool VariablePresence { get; }
        public IReadOnlyList<string> PatternOrder { get; }
        public GeotechnicalCombinationRequest(GeotechnicalActionFactors ultimate, IEnumerable<GeotechnicalDesignAction> actions, IEnumerable<string> bodies = null,
            IEnumerable<string> rowBodies = null, int seismicVariants = 0)
            : this(ultimate, actions, bodies, rowBodies, seismicVariants, true, null)
        {
        }
        public GeotechnicalCombinationRequest(GeotechnicalActionFactors ultimate, IEnumerable<GeotechnicalDesignAction> actions, IEnumerable<string> bodies,
            IEnumerable<string> rowBodies, int seismicVariants, bool variablePresence, IEnumerable<string> patternOrder)
        {
            Ultimate = ultimate ?? throw new ArgumentNullException(nameof(ultimate));
            Actions = (actions ?? throw new ArgumentNullException(nameof(actions))).ToArray();
            if (Actions.Any(a => a == null)) throw new ArgumentNullException(nameof(actions));
            if (Actions.Select(a => a.Id).Distinct(StringComparer.Ordinal).Count() != Actions.Count) throw new ArgumentException("Action ids must be unique.", nameof(actions));
            foreach (var g in Actions.GroupBy(a => a.Group, StringComparer.Ordinal))
                if (g.Select(a => a.Nature).Distinct().Count() > 1) throw new ArgumentException("The actions of a group must share the nature: " + g.Key, nameof(actions));
            Bodies = (bodies ?? Array.Empty<string>()).ToArray(); RowBodies = (rowBodies ?? Array.Empty<string>()).ToArray();
            if (Bodies.Concat(RowBodies).Any(string.IsNullOrWhiteSpace)) throw new ArgumentException("Body names required.");
            if (seismicVariants < 0) throw new ArgumentOutOfRangeException(nameof(seismicVariants));
            SeismicVariants = seismicVariants; VariablePresence = variablePresence;
            PatternOrder = (patternOrder ?? Array.Empty<string>()).ToArray();
            if (PatternOrder.Count > 0)
            {
                var entries = Bodies.Concat(Actions.Where(a => a.Nature != GeotechnicalActionNature.A).Select(a => a.Group).Distinct(StringComparer.Ordinal)).ToArray();
                if (entries.Distinct(StringComparer.Ordinal).Count() != entries.Length)
                    throw new ArgumentException("A body and a group share the name: the pattern order would be ambiguous.", nameof(patternOrder));
                if (PatternOrder.Count != entries.Length || PatternOrder.Distinct(StringComparer.Ordinal).Count() != PatternOrder.Count
                    || PatternOrder.Any(p => !entries.Contains(p, StringComparer.Ordinal)))
                    throw new ArgumentException("The pattern order must name every body and every G1, G2 and Q group once.", nameof(patternOrder));
            }
        }
    }

    /// <summary>
    /// A generated row: number (one counter over all the rows), limit state, leading variable group (characteristic, frequent and ultimate rows),
    /// accidental group (exceptional rows), seismic variant (index in the request, −1 otherwise), factors of the bodies (<c>Bodies</c> then
    /// <c>RowBodies</c>, 1 outside the ultimate rows) and the factor of every action by id.
    /// </summary>
    public sealed class GeotechnicalCombinationRow
    {
        public int Number { get; }
        public GeotechnicalLimitState State { get; }
        public string Lead { get; }
        public string AccidentalGroup { get; }
        public int SeismicVariant { get; }
        public IReadOnlyList<double> BodyFactors { get; }
        public IReadOnlyDictionary<string, double> Factors { get; }
        internal GeotechnicalCombinationRow(int number, GeotechnicalLimitState state, string lead, string accidentalGroup, int seismicVariant,
            double[] bodyFactors, Dictionary<string, double> factors)
        {
            Number = number; State = state; Lead = lead; AccidentalGroup = accidentalGroup; SeismicVariant = seismicVariant; BodyFactors = bodyFactors; Factors = factors;
        }
    }

    /// <summary>
    /// Combinations of NTC 2018 §2.5.3 with the action set of the request (geotechnical checks, and the structural load cases of a model with
    /// the sets of a <see cref="CombinationProfile"/>), in this order:
    /// <list type="bullet">
    /// <item>characteristic and frequent rows with every leading variable group (one row without lead when there is no Q): G 1, A 0, lead 1 or ψ1,
    /// the others ψ0 or ψ2; one quasi permanent row (Q ψ2);</item>
    /// <item>ultimate rows for every pattern: bit 0 the first body (fastest), then the other bodies, every G1 or G2 group (favourable or
    /// unfavourable) and the presence of every Q group, in the order of the first action of the group, or in the declared
    /// <see cref="GeotechnicalCombinationRequest.PatternOrder"/>; for every pattern one row for every present leading Q group (lead γQ, the others
    /// γQ ψ0), one row when no Q is present; every row doubled by every row body;</item>
    /// <item>seismic rows for every variant, with the quasi permanent factors: E + Gk + Σ ψ2i Qki;</item>
    /// <item>exceptional rows for every A group: the quasi permanent factors and the group at 1, Ad + Gk + Σ ψ2i Qki.</item>
    /// </list>
    /// In the seismic and exceptional rows a variable action enters only where unfavourable (ψ2 Qk only if unfavourable, NTC 2018 §2.5.3): with
    /// <see cref="GeotechnicalCombinationRequest.VariablePresence"/> every variant and every A group has one row for every presence pattern of the
    /// Q groups, the absent groups at 0: first every group present, then the bits of absence in the order of the first action of the group (the
    /// first group absent, the second absent, both absent, ...). Decision of the ANTHEA user of 28/10/2026 on the census of the combinations,
    /// question D6 («Come raccomandato»: presence or absence of every Q group also in these rows, as in the ultimate ones); until GPCModel 5.23
    /// one row with every Q at ψ2, still available for fixed older rules.
    /// Rows with the same state, variant, body factors and action factors are merged without using a number (for example the absence of a group
    /// with ψ2 = 0). Disabled actions are not passed.
    /// Requested on 26/10/2026 («sposta le combo e la generazione dentro model»): the generator of the retaining walls of GPCChecker.Geotechnics
    /// becomes an adapter of this one (ANTHEA docs/refactoring/combinazioni-model-censimento.md, step 1).
    /// </summary>
    public static class GeotechnicalCombinations
    {
        public static IReadOnlyList<GeotechnicalCombinationRow> Generate(GeotechnicalCombinationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            var actions = request.Actions; var set = request.Ultimate;
            var groups = actions.GroupBy(a => a.Group, StringComparer.Ordinal).ToArray();
            var variables = groups.Where(g => g.First().Nature == GeotechnicalActionNature.Q).ToArray();
            var permanent = groups.Where(g => g.First().Nature == GeotechnicalActionNature.G1 || g.First().Nature == GeotechnicalActionNature.G2).ToArray();
            int bodies = request.Bodies.Count, rowBodies = request.RowBodies.Count;
            int bits = bodies + permanent.Length + variables.Length;
            if (bits > 30 || rowBodies > 30) throw new ArgumentException("Too many bodies and action groups for the pattern of the ultimate rows.", nameof(request));
            // Bit of every entry of the pattern: the default order (bodies, permanent groups, variable groups) or the declared one.
            int Bit(string entry, int fallback) => request.PatternOrder.Count == 0 ? fallback : IndexOf(request.PatternOrder, entry);
            var bodyBits = Enumerable.Range(0, bodies).Select(i => Bit(request.Bodies[i], i)).ToArray();
            var permanentBits = Enumerable.Range(0, permanent.Length).Select(i => Bit(permanent[i].Key, bodies + i)).ToArray();
            var variableBits = Enumerable.Range(0, variables.Length).Select(i => Bit(variables[i].Key, bodies + permanent.Length + i)).ToArray();
            var rows = new List<GeotechnicalCombinationRow>(); var seen = new HashSet<string>(StringComparer.Ordinal); int number = 0;
            void Add(GeotechnicalLimitState state, string lead, string accidental, int variant, double[] patternBodies, Dictionary<string, double> factors)
            {
                int doubled = state == GeotechnicalLimitState.Ultimate ? rowBodies : 0;
                for (int pattern = 0; pattern < 1 << doubled; pattern++)
                {
                    var bodyFactors = new double[bodies + rowBodies];
                    for (int i = 0; i < bodies; i++) bodyFactors[i] = patternBodies == null ? 1 : patternBodies[i];
                    for (int i = 0; i < rowBodies; i++) bodyFactors[bodies + i] = doubled == 0 ? 1 : (pattern & (1 << i)) != 0 ? set.PermanentUnfavourable : set.PermanentFavourable;
                    string fingerprint = string.Join("/", state, variant, string.Join(",", bodyFactors.Select(R))) + "/" + string.Join(",", actions.Select(a => R(factors[a.Id])));
                    if (!seen.Add(fingerprint)) continue;
                    rows.Add(new GeotechnicalCombinationRow(++number, state, lead, accidental, variant, bodyFactors, new Dictionary<string, double>(factors, StringComparer.Ordinal)));
                }
            }
            Dictionary<string, double> Service(GeotechnicalLimitState state, string lead = null) => actions.ToDictionary(a => a.Id, a =>
                a.Nature == GeotechnicalActionNature.G1 || a.Nature == GeotechnicalActionNature.G2 ? 1d : a.Nature == GeotechnicalActionNature.A ? 0d
                : state == GeotechnicalLimitState.QuasiPermanent ? a.Psi2 : a.Group == lead ? (state == GeotechnicalLimitState.Characteristic ? 1 : a.Psi1)
                : (state == GeotechnicalLimitState.Characteristic ? a.Psi0 : a.Psi2), StringComparer.Ordinal);
            foreach (var state in new[] { GeotechnicalLimitState.Characteristic, GeotechnicalLimitState.Frequent })
                foreach (string lead in variables.Length == 0 ? new string[] { null } : variables.Select(v => v.Key))
                    Add(state, lead, null, -1, null, Service(state, lead));
            Add(GeotechnicalLimitState.QuasiPermanent, null, null, -1, null, Service(GeotechnicalLimitState.QuasiPermanent));
            for (int mask = 0; mask < 1 << bits; mask++)
            {
                var patternBodies = new double[bodies];
                for (int i = 0; i < bodies; i++) patternBodies[i] = (mask & (1 << bodyBits[i])) == 0 ? set.PermanentFavourable : set.PermanentUnfavourable;
                var factors = actions.ToDictionary(a => a.Id, _ => 0d, StringComparer.Ordinal);
                for (int p = 0; p < permanent.Length; p++)
                {
                    bool high = (mask & (1 << permanentBits[p])) != 0;
                    foreach (var a in permanent[p]) factors[a.Id] = a.Nature == GeotechnicalActionNature.G1 ? (high ? set.PermanentUnfavourable : set.PermanentFavourable)
                        : (high ? set.NonStructuralUnfavourable : set.NonStructuralFavourable);
                }
                var present = variables.Where((_, i) => (mask & (1 << variableBits[i])) != 0).ToArray();
                if (present.Length == 0) Add(GeotechnicalLimitState.Ultimate, null, null, -1, patternBodies, factors);
                foreach (var lead in present)
                {
                    foreach (var v in present) foreach (var a in v) factors[a.Id] = set.VariableUnfavourable * (v.Key == lead.Key ? 1 : a.Psi0);
                    Add(GeotechnicalLimitState.Ultimate, lead.Key, null, -1, patternBodies, factors);
                }
            }
            // Seismic and exceptional rows: the quasi permanent factors with every presence pattern of the Q groups (census D6), or one row.
            IEnumerable<Dictionary<string, double>> Presence(Dictionary<string, double> quasi)
            {
                int patterns = request.VariablePresence ? 1 << variables.Length : 1;
                for (int absent = 0; absent < patterns; absent++)
                {
                    var factors = new Dictionary<string, double>(quasi, StringComparer.Ordinal);
                    for (int i = 0; i < variables.Length; i++)
                        if ((absent & (1 << i)) != 0) foreach (var a in variables[i]) factors[a.Id] = 0;
                    yield return factors;
                }
            }
            for (int i = 0; i < request.SeismicVariants; i++)
                foreach (var factors in Presence(Service(GeotechnicalLimitState.QuasiPermanent))) Add(GeotechnicalLimitState.Seismic, null, null, i, null, factors);
            foreach (var accident in groups.Where(g => g.First().Nature == GeotechnicalActionNature.A))
            {
                var quasi = Service(GeotechnicalLimitState.QuasiPermanent); foreach (var a in accident) quasi[a.Id] = 1;
                foreach (var factors in Presence(quasi)) Add(GeotechnicalLimitState.Exceptional, null, accident.Key, -1, null, factors);
            }
            return rows;
        }

        private static int IndexOf(IReadOnlyList<string> list, string entry)
        {
            for (int i = 0; i < list.Count; i++) if (string.Equals(list[i], entry, StringComparison.Ordinal)) return i;
            throw new ArgumentException("Pattern entry not declared: " + entry);
        }

        private static string R(double v) => v.ToString("R", CultureInfo.InvariantCulture);
    }
}
