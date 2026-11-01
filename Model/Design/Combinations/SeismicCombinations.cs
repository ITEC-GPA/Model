using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Design.Combinations
{
    /// <summary>Direction of the seismic action of a load case: none (the case is combined alone, with both signs) or one of the global
    /// directions X, Y (horizontal) and Z (vertical) of NTC 2018 §7.3.5.</summary>
    public enum SeismicDirection { None, X, Y, Z }

    /// <summary>A seismic load case of a model and the direction of its action.</summary>
    public sealed class SeismicCase
    {
        public string Id { get; }
        public SeismicDirection Direction { get; }
        public SeismicCase(string id, SeismicDirection direction = SeismicDirection.None)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Seismic case id required.", nameof(id));
            if (!Enum.IsDefined(typeof(SeismicDirection), direction)) throw new ArgumentOutOfRangeException(nameof(direction));
            Id = id; Direction = direction;
        }
    }

    /// <summary>A seismic variant: the factors of the seismic cases that act together in one seismic row, in order.</summary>
    public sealed class SeismicVariant
    {
        public IReadOnlyList<KeyValuePair<string, double>> Factors { get; }
        internal SeismicVariant(IEnumerable<KeyValuePair<string, double>> factors) { Factors = factors.ToArray(); }
    }

    /// <summary>
    /// The seismic variants of the rows of NTC 2018 §2.5.3 (E + G1 + G2 + Σ ψ2j Qkj), one row for each variant (the count of
    /// <see cref="GeotechnicalCombinationRequest.SeismicVariants"/>).
    /// <list type="bullet">
    /// <item>A case without direction enters alone with +1 and −1, at its place in the order of the cases.</item>
    /// <item>The directional cases combine with the rule of NTC 2018 §7.3.5, (7.3.10): 1,00·E of one direction and 0,30·E of the others, the
    /// coefficients permuted so that each direction leads once, with every combination of signs. The cases of one direction are alternatives
    /// (one of them at a time, for example the same spectrum with two accidental eccentricities). The variants come at the place of the first
    /// directional case: for each leading direction (X, Y, Z order), for each choice of one case per direction (cases in their order), the
    /// signs with the leading direction slowest, then the other directions in the X, Y, Z order, + before −. A single direction gives ±1 of its
    /// cases, as a case without direction.</item>
    /// </list>
    /// Whether the vertical component is needed (NTC 2018 §7.2.2) is the choice of the caller: a Z case enters only when given.
    /// </summary>
    public static class SeismicCombinations
    {
        /// <summary>Factor of the companion directions, NTC 2018 §7.3.5 (7.3.10).</summary>
        public const double Companion = 0.30;

        public static IReadOnlyList<SeismicVariant> Variants(IEnumerable<SeismicCase> cases)
        {
            var list = (cases ?? throw new ArgumentNullException(nameof(cases))).ToArray();
            if (list.Any(c => c == null)) throw new ArgumentNullException(nameof(cases));
            if (list.Select(c => c.Id).Distinct(StringComparer.Ordinal).Count() != list.Length) throw new ArgumentException("Seismic case ids must be unique.", nameof(cases));
            var variants = new List<SeismicVariant>(); bool directionalDone = false;
            foreach (var c in list)
            {
                if (c.Direction == SeismicDirection.None)
                {
                    variants.Add(Single(c.Id, 1)); variants.Add(Single(c.Id, -1));
                }
                else if (!directionalDone) { directionalDone = true; variants.AddRange(Directional(list)); }
            }
            return variants;
        }

        static SeismicVariant Single(string id, double factor) => new SeismicVariant(new[] { new KeyValuePair<string, double>(id, factor) });

        static IEnumerable<SeismicVariant> Directional(SeismicCase[] cases)
        {
            var directions = new[] { SeismicDirection.X, SeismicDirection.Y, SeismicDirection.Z }
                .Select(d => cases.Where(c => c.Direction == d).Select(c => c.Id).ToArray()).Where(ids => ids.Length > 0).ToArray();
            int n = directions.Length;
            for (int lead = 0; lead < n; lead++)
            {
                var order = new[] { lead }.Concat(Enumerable.Range(0, n).Where(i => i != lead)).ToArray();
                foreach (var choice in Choices(directions))
                    for (int signs = 0; signs < 1 << n; signs++)
                    {
                        var factors = new KeyValuePair<string, double>[n];
                        for (int k = 0; k < n; k++)
                        {
                            int d = order[k];
                            double sign = ((signs >> (n - 1 - k)) & 1) == 0 ? 1 : -1;
                            factors[k] = new KeyValuePair<string, double>(choice[d], sign * (d == lead ? 1 : Companion));
                        }
                        yield return new SeismicVariant(factors);
                    }
            }
        }

        // Every choice of one case per direction, the last direction fastest.
        static IEnumerable<string[]> Choices(string[][] directions)
        {
            var index = new int[directions.Length];
            while (true)
            {
                yield return directions.Select((ids, d) => ids[index[d]]).ToArray();
                int k = directions.Length - 1;
                while (k >= 0 && ++index[k] == directions[k].Length) { index[k] = 0; k--; }
                if (k < 0) yield break;
            }
        }
    }
}
