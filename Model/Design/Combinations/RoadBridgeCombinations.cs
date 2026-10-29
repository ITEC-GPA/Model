using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Standards;

namespace GPC.Model.Design.Combinations
{
    /// <summary>
    /// The partial factors of an action set of the road bridges, NTC 2018 Tab. 5.1.V (favourable and unfavourable): permanent structural actions
    /// g1 and g3 (γG1), non structural permanent actions g2 (γG2), variable traffic actions (γQ) and the other variable actions (γQi). Prestress,
    /// shrinkage, creep and settlements of the table are not part of the set.
    /// </summary>
    public sealed class RoadBridgeActionFactors
    {
        public string Name { get; }
        public double StructuralUnfavourable { get; }
        public double StructuralFavourable { get; }
        public double NonStructuralUnfavourable { get; }
        public double NonStructuralFavourable { get; }
        public double TrafficUnfavourable { get; }
        public double TrafficFavourable { get; }
        public double VariableUnfavourable { get; }
        public double VariableFavourable { get; }

        public RoadBridgeActionFactors(string name, double structuralUnfavourable, double structuralFavourable, double nonStructuralUnfavourable,
            double nonStructuralFavourable, double trafficUnfavourable, double trafficFavourable, double variableUnfavourable, double variableFavourable)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
            if (new[] { structuralUnfavourable, structuralFavourable, nonStructuralUnfavourable, nonStructuralFavourable, trafficUnfavourable, trafficFavourable,
                variableUnfavourable, variableFavourable }.Any(v => double.IsNaN(v) || double.IsInfinity(v) || v < 0))
                throw new ArgumentOutOfRangeException(nameof(structuralUnfavourable), "Finite non negative factors required.");
            Name = name; StructuralUnfavourable = structuralUnfavourable; StructuralFavourable = structuralFavourable;
            NonStructuralUnfavourable = nonStructuralUnfavourable; NonStructuralFavourable = nonStructuralFavourable;
            TrafficUnfavourable = trafficUnfavourable; TrafficFavourable = trafficFavourable;
            VariableUnfavourable = variableUnfavourable; VariableFavourable = variableFavourable;
        }

        /// <summary>The set for the generator when the traffic is the only variable action: G1, G2 and Q = traffic.</summary>
        public GeotechnicalActionFactors WithTrafficAsVariable() => new GeotechnicalActionFactors(Name, StructuralUnfavourable, StructuralFavourable,
            NonStructuralUnfavourable, NonStructuralFavourable, TrafficUnfavourable, TrafficFavourable);
    }

    /// <summary>
    /// The groups of the road traffic actions of NTC 2018 Tab. 5.1.VI with ψ0 ≥ ψ1 ≥ ψ2, as the generator requires (Schema 2, ψ0 0 and ψ1 0.75, and
    /// the crowd of group 4, without ψ0, are not here).
    /// </summary>
    public enum RoadTrafficPsiGroup { Schema1Tandem, Schemes156Distributed, Schemes34Concentrated }

    /// <summary>
    /// Combinations of the road bridges of NTC 2018 §5.1.3.12 (D.M. 17/01/2018, S.O. n. 8 alla G.U. n. 42 del 20/02/2018): the action sets EQU, A1 and
    /// A2 of Tab. 5.1.V and the ψ of the traffic groups of Tab. 5.1.VI, checked against the text of the Gazzetta Ufficiale (the Circolare n. 7 of
    /// 21/01/2019, §C5, repeats γQ 1.35 for EQU and STR and 1.15 for GEO). The rows come from <see cref="GeotechnicalCombinations"/> (NTC 2018 §2.5.3).
    /// Decision of the ANTHEA user of 29/10/2026 on the census of the combinations, question D8: «d8 e bridge design devono usare model».
    /// Note (2) of Tab. 5.1.V (the factors of the permanent actions for the non structural ones whose intensity is well defined) is a choice of
    /// the designer, not of the set.
    /// </summary>
    public static class RoadBridgeCombinations
    {
        /// <summary>EQU (equilibrium not involving the ground): γG1 1.10/0.90, γG2 1.50/0, γQ traffic 1.35/0, γQi 1.50/0.</summary>
        public static RoadBridgeActionFactors Equilibrium { get; } = new RoadBridgeActionFactors("EQU ponti stradali", 1.10, 0.90, 1.50, 0, 1.35, 0, 1.50, 0);

        /// <summary>A1 (STR): γG1 1.35/1.00, γG2 1.50/0, γQ traffic 1.35/0, γQi 1.50/0.</summary>
        public static RoadBridgeActionFactors Structural { get; } = new RoadBridgeActionFactors("A1 ponti stradali", 1.35, 1.00, 1.50, 0, 1.35, 0, 1.50, 0);

        /// <summary>A2 (GEO): γG1 1.00/1.00, γG2 1.30/0, γQ traffic 1.15/0, γQi 1.30/0.</summary>
        public static RoadBridgeActionFactors Geotechnical { get; } = new RoadBridgeActionFactors("A2 ponti stradali", 1.00, 1.00, 1.30, 0, 1.15, 0, 1.30, 0);

        /// <summary>ψ0, ψ1, ψ2 of a group of the traffic actions, Tab. 5.1.VI.</summary>
        public static double[] TrafficPsi(RoadTrafficPsiGroup group)
        {
            switch (group)
            {
                case RoadTrafficPsiGroup.Schema1Tandem: return new[] { .75, .75, 0 };
                case RoadTrafficPsiGroup.Schemes156Distributed: return new[] { .40, .40, 0 };
                case RoadTrafficPsiGroup.Schemes34Concentrated: return new[] { .40, .40, 0 };
                default: throw new ArgumentOutOfRangeException(nameof(group));
            }
        }

        /// <summary>
        /// The rows of a deck with the structural permanent actions <paramref name="structural"/> (G1), the non structural ones
        /// <paramref name="nonStructural"/> (G2) and one traffic group <paramref name="traffic"/> (Q with the ψ of <paramref name="psi"/>), with the
        /// action set <paramref name="set"/>: characteristic, frequent and quasi permanent rows and the ultimate rows of every pattern (G1 and G2
        /// favourable or unfavourable, traffic present or absent). Rows with the same factors are merged.
        /// </summary>
        public static IReadOnlyList<GeotechnicalCombinationRow> Deck(RoadBridgeActionFactors set, string structural = "g1", string nonStructural = "g2",
            string traffic = "traffic", RoadTrafficPsiGroup psi = RoadTrafficPsiGroup.Schema1Tandem)
        {
            if (set == null) throw new ArgumentNullException(nameof(set));
            var values = TrafficPsi(psi);
            var actions = new[]
            {
                new GeotechnicalDesignAction(structural, structural, GeotechnicalActionNature.G1),
                new GeotechnicalDesignAction(nonStructural, nonStructural, GeotechnicalActionNature.G2),
                new GeotechnicalDesignAction(traffic, traffic, GeotechnicalActionNature.Q, values[0], values[1], values[2]),
            };
            return GeotechnicalCombinations.Generate(new GeotechnicalCombinationRequest(set.WithTrafficAsVariable(), actions));
        }
    }
}
