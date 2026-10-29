using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Standards;

namespace GPC.Model.Design.Combinations
{
    /// <summary>
    /// Category of a variable action for its combination factors ψ0, ψ1 and ψ2 (NTC 2018 Tab. 2.5.I, EN 1990 Tab. A1.1): imposed loads of the
    /// categories A-H, wind, snow up to and above 1000 m a.s.l., temperature. The categories I and K of NTC 2018 (roofs, «da valutarsi caso per
    /// caso») have no values: the caller gives its own ψ (<see cref="GeotechnicalDesignAction"/>).
    /// </summary>
    public enum VariableActionCategory { A, B, C, D, E, F, G, H, Wind, SnowUpTo1000m, SnowAbove1000m, Temperature }

    /// <summary>
    /// The values of a standard for the combinations of <see cref="GeotechnicalCombinations"/> (ANTHEA census of the combinations, M3): the action
    /// sets of the ultimate rows, equilibrium (EQU), structural (STR, A1), geotechnical (GEO, A2) and seismic (every factor 1, Q favourable 0), and
    /// ψ0, ψ1, ψ2 by category. Decision of the ANTHEA user of 28/10/2026 on the census, question D10 («Come raccomandato»): the profile of NTC 2018
    /// now (<see cref="Ntc2018"/>), with the values of EN 1990 kept in a profile (<see cref="En1990"/>) for the future choice «Eurocodici con una
    /// norma di progetto»; no choice of the standard in ANTHEA for now. The formulas of the rows are those of NTC 2018 §2.5.3 implemented by
    /// <see cref="GeotechnicalCombinations"/>; the formulas proper to EN 1990 (6.10a and 6.10b, ψ1 of the leading action in the accidental
    /// combination (6.11b)) are not implemented, and EN 1990 has one γG for G1 and G2.
    /// </summary>
    public sealed class CombinationProfile
    {
        private readonly Dictionary<VariableActionCategory, double[]> _psi;

        public string Name { get; }
        public string Reference { get; }
        public GeotechnicalActionFactors Equilibrium { get; }
        public GeotechnicalActionFactors Structural { get; }
        public GeotechnicalActionFactors Geotechnical { get; }
        public GeotechnicalActionFactors Seismic { get; }

        /// <param name="psi">ψ0, ψ1, ψ2 of every category (0 ≤ ψ2 ≤ ψ1 ≤ ψ0 ≤ 1).</param>
        public CombinationProfile(string name, string reference, GeotechnicalActionFactors equilibrium, GeotechnicalActionFactors structural,
            GeotechnicalActionFactors geotechnical, GeotechnicalActionFactors seismic, IDictionary<VariableActionCategory, double[]> psi)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
            Name = name; Reference = reference ?? "";
            Equilibrium = equilibrium ?? throw new ArgumentNullException(nameof(equilibrium));
            Structural = structural ?? throw new ArgumentNullException(nameof(structural));
            Geotechnical = geotechnical ?? throw new ArgumentNullException(nameof(geotechnical));
            Seismic = seismic ?? throw new ArgumentNullException(nameof(seismic));
            if (psi == null) throw new ArgumentNullException(nameof(psi));
            _psi = new Dictionary<VariableActionCategory, double[]>();
            foreach (VariableActionCategory category in Enum.GetValues(typeof(VariableActionCategory)))
            {
                if (!psi.TryGetValue(category, out var values) || values == null || values.Length != 3) throw new ArgumentException("ψ0, ψ1, ψ2 required for " + category, nameof(psi));
                if (!(values[2] >= 0 && values[2] <= values[1] && values[1] <= values[0] && values[0] <= 1)) throw new ArgumentOutOfRangeException(nameof(psi), "0 ≤ ψ2 ≤ ψ1 ≤ ψ0 ≤ 1 required: " + category);
                _psi[category] = values.ToArray();
            }
        }

        public double Psi0(VariableActionCategory category) => Values(category)[0];
        public double Psi1(VariableActionCategory category) => Values(category)[1];
        public double Psi2(VariableActionCategory category) => Values(category)[2];

        /// <summary>A variable action of the generator with the ψ of its category.</summary>
        public GeotechnicalDesignAction Variable(string id, string group, VariableActionCategory category)
        {
            var values = Values(category);
            return new GeotechnicalDesignAction(id, group, GeotechnicalActionNature.Q, values[0], values[1], values[2]);
        }

        private double[] Values(VariableActionCategory category) =>
            _psi.TryGetValue(category, out var values) ? values : throw new ArgumentOutOfRangeException(nameof(category));

        /// <summary>
        /// NTC 2018 (D.M. 17/01/2018): Tab. 2.6.I, EQU G1 0.9/1.1, G2 0.8/1.5, Q 0/1.5; A1 and A2 as Tab. 6.2.I (<see cref="StandardNTC2018Geotechnics"/>,
        /// checked against the text in the phase G of the ANTHEA migration); seismic set of §2.5.3 and §7.11; ψ of Tab. 2.5.I. The EQU row of Tab. 2.6.I
        /// and Tab. 2.5.I are written from the technical literature and coincide with the recommended values of EN 1990 for ψ: to be checked against the
        /// text («da riscontrare sul testo»).
        /// </summary>
        public static CombinationProfile Ntc2018 { get; } = CreateNtc2018();

        /// <summary>EN 1990:2002/A1:2005 with the recommended values of <see cref="StandardEN1990"/>.</summary>
        public static CombinationProfile En1990 => FromEN1990(new StandardEN1990());

        /// <summary>
        /// The values of an EN 1990 standard (recommended, or those of a national annex set on the instance): Tab. A1.2(A) EQU, A1.2(B) STR, A1.2(C) GEO,
        /// with the same γG for G1 and G2; seismic set with every factor 1 ((6.12b): Gk + AEd + Σ ψ2,i Qk,i); ψ of Tab. A1.1.
        /// </summary>
        public static CombinationProfile FromEN1990(StandardEN1990 standard)
        {
            if (standard == null) throw new ArgumentNullException(nameof(standard));
            GeotechnicalActionFactors Set(string name, double gUnfavourable, double gFavourable, double qUnfavourable, double qFavourable) =>
                new GeotechnicalActionFactors(name, gUnfavourable, gFavourable, gUnfavourable, gFavourable, qUnfavourable, qFavourable);
            var psi = new Dictionary<VariableActionCategory, double[]>
            {
                [VariableActionCategory.A] = new[] { standard.ImposedLoadPsi0CategoryA, standard.ImposedLoadPsi1CategoryA, standard.ImposedLoadPsi2CategoryA },
                [VariableActionCategory.B] = new[] { standard.ImposedLoadPsi0CategoryB, standard.ImposedLoadPsi1CategoryB, standard.ImposedLoadPsi2CategoryB },
                [VariableActionCategory.C] = new[] { standard.ImposedLoadPsi0CategoryC, standard.ImposedLoadPsi1CategoryC, standard.ImposedLoadPsi2CategoryC },
                [VariableActionCategory.D] = new[] { standard.ImposedLoadPsi0CategoryD, standard.ImposedLoadPsi1CategoryD, standard.ImposedLoadPsi2CategoryD },
                [VariableActionCategory.E] = new[] { standard.ImposedLoadPsi0CategoryE, standard.ImposedLoadPsi1CategoryE, standard.ImposedLoadPsi2CategoryE },
                [VariableActionCategory.F] = new[] { standard.ImposedLoadPsi0CategoryF, standard.ImposedLoadPsi1CategoryF, standard.ImposedLoadPsi2CategoryF },
                [VariableActionCategory.G] = new[] { standard.ImposedLoadPsi0CategoryG, standard.ImposedLoadPsi1CategoryG, standard.ImposedLoadPsi2CategoryG },
                [VariableActionCategory.H] = new[] { standard.ImposedLoadPsi0CategoryH, standard.ImposedLoadPsi1CategoryH, standard.ImposedLoadPsi2CategoryH },
                [VariableActionCategory.Wind] = new[] { standard.Psi0Wind, standard.Psi1Wind, standard.Psi2Wind },
                [VariableActionCategory.SnowUpTo1000m] = new[] { standard.Psi0SnowLowAltitude, standard.Psi1SnowLowAltitude, standard.Psi2SnowLowAltitude },
                [VariableActionCategory.SnowAbove1000m] = new[] { standard.Psi0SnowHighAltitude, standard.Psi1SnowHighAltitude, standard.Psi2SnowHighAltitude },
                [VariableActionCategory.Temperature] = new[] { standard.Psi0Temperature, standard.Psi1Temperature, standard.Psi2Temperature },
            };
            return new CombinationProfile("EN 1990", standard.Name + ", Tab. A1.1 and A1.2(A)-(C)",
                Set("EQU", standard.GammaGUnfavourableSetA, standard.GammaGFavourableSetA, standard.GammaQUnfavourableSetA, standard.GammaQFavourableSetA),
                Set("STR", standard.GammaGUnfavourableSetB, standard.GammaGFavourableSetB, standard.GammaQUnfavourableSetB, standard.GammaQFavourableSetB),
                Set("GEO", standard.GammaGUnfavourableSetC, standard.GammaGFavourableSetC, standard.GammaQUnfavourableSetC, standard.GammaQFavourableSetC),
                Set("Seismic", 1, 1, 1, 0), psi);
        }

        private static CombinationProfile CreateNtc2018()
        {
            var geotechnics = new StandardNTC2018Geotechnics();
            var psi = new Dictionary<VariableActionCategory, double[]>
            {
                // Tab. 2.5.I: A residential, B offices, C crowding, D commercial, E storage and industrial, F vehicles ≤ 30 kN, G vehicles 30-160 kN,
                // H roofs for maintenance only; wind; snow up to and above 1000 m a.s.l.; thermal variations.
                [VariableActionCategory.A] = new[] { .7, .5, .3 },
                [VariableActionCategory.B] = new[] { .7, .5, .3 },
                [VariableActionCategory.C] = new[] { .7, .7, .6 },
                [VariableActionCategory.D] = new[] { .7, .7, .6 },
                [VariableActionCategory.E] = new[] { 1, .9, .8 },
                [VariableActionCategory.F] = new[] { .7, .7, .6 },
                [VariableActionCategory.G] = new[] { .7, .5, .3 },
                [VariableActionCategory.H] = new[] { 0d, 0, 0 },
                [VariableActionCategory.Wind] = new[] { .6, .2, 0 },
                [VariableActionCategory.SnowUpTo1000m] = new[] { .5, .2, 0 },
                [VariableActionCategory.SnowAbove1000m] = new[] { .7, .5, .2 },
                [VariableActionCategory.Temperature] = new[] { .6, .5, 0 },
            };
            return new CombinationProfile("NTC 2018", "D.M. 17/01/2018, §2.5.3, Tab. 2.5.I and 2.6.I",
                new GeotechnicalActionFactors("EQU", 1.1, .9, 1.5, .8, 1.5, 0), geotechnics.ActionSet("A1"), geotechnics.ActionSet("A2"),
                geotechnics.ActionSet("A-seismic"), psi);
        }
    }
}
