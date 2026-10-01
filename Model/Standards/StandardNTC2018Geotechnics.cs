using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// NTC 2018 (D.M. 17/01/2018, chapters 6 and 7.11) geotechnical partial factors.
    /// Tab. 6.2.I (A1, A2), 6.2.II (M1, M2), 6.4.I shallow foundations, 6.4.II piles, 6.4.IV correlation factors, 6.4.VI transverse
    /// piles, 6.5.I retaining walls, §6.8.2 slopes and global stability (R2 = 1.1), Tab. 7.11.II and 7.11.III and §7.11.4 seismic.
    /// Approaches: foundations, walls and piles Approccio 2 (A1+M1+R3); global stability and slopes A2+M2+R2; seismic: actions
    /// with partial factors 1, M1, seismic γR. Values as used by the legacy ANTHEA calculations; to be checked against the text.
    /// </summary>
    [Serializable]
    public class StandardNTC2018Geotechnics : StandardGeotechnical
    {
        public override string Edition => "2018";
        private const string A1 = "A1", A2 = "A2", Seismic = "A-seismic", M1 = "M1", M2 = "M2";

        public StandardNTC2018Geotechnics() : this("NTC 2018 - Geotecnica") { }
        public StandardNTC2018Geotechnics(string name, string remarks = "D.M. 17/01/2018, capitoli 6 e 7.11; Circolare 21/01/2019 n. 7")
            : base(name, remarks)
        {
            // Tab. 6.2.I: G1 (structural and soil), G2 (non-structural), Q.
            SetActionSet(new GeotechnicalActionFactors(A1, 1.3, 1.0, 1.5, .8, 1.5, 0));
            SetActionSet(new GeotechnicalActionFactors(A2, 1.0, 1.0, 1.3, .8, 1.3, 0));
            SetActionSet(new GeotechnicalActionFactors(Seismic, 1.0, 1.0, 1.0, 1.0, 1.0, 0));
            // Tab. 6.2.II: tan φ', c', cu, qu, γ.
            SetMaterialSet(new GeotechnicalMaterialFactors(M1, 1.0, 1.0, 1.0, 1.0, 1.0));
            SetMaterialSet(new GeotechnicalMaterialFactors(M2, 1.25, 1.25, 1.4, 1.6, 1.0));
            var st = GeotechnicalSituation.PersistentTransient; var eq = GeotechnicalSituation.Seismic;
            // Tab. 6.4.I and 7.11.II: shallow foundations.
            Define(GeotechnicalCheck.ShallowFoundationBearing, st, "R3", 2.3); Define(GeotechnicalCheck.ShallowFoundationSliding, st, "R3", 1.1);
            Define(GeotechnicalCheck.ShallowFoundationBearing, eq, "R3", 2.3); Define(GeotechnicalCheck.ShallowFoundationSliding, eq, "R3", 1.1);
            // Tab. 6.5.I and 7.11.III: retaining walls.
            Define(GeotechnicalCheck.RetainingWallBearing, st, "R3", 1.4); Define(GeotechnicalCheck.RetainingWallSliding, st, "R3", 1.1);
            Define(GeotechnicalCheck.RetainingWallOverturning, st, "R3", 1.15); Define(GeotechnicalCheck.RetainingWallPassiveResistance, st, "R3", 1.4);
            Define(GeotechnicalCheck.RetainingWallBearing, eq, "R3", 1.2); Define(GeotechnicalCheck.RetainingWallSliding, eq, "R3", 1.0);
            Define(GeotechnicalCheck.RetainingWallOverturning, eq, "R3", 1.0); Define(GeotechnicalCheck.RetainingWallPassiveResistance, eq, "R3", 1.2);
            // §6.8.2, §6.5.3.1.1 and §7.11.4: global stability and slopes.
            Define(GeotechnicalCheck.GlobalStability, st, "R2", 1.1); Define(GeotechnicalCheck.SlopeStability, st, "R2", 1.1);
            Define(GeotechnicalCheck.GlobalStability, eq, "R2", 1.2); Define(GeotechnicalCheck.SlopeStability, eq, "R2", 1.2);
            // Tab. 6.4.II (bored piles, R3) and 6.4.VI (transverse load).
            Define(GeotechnicalCheck.PileBase, st, "R3", 1.35); Define(GeotechnicalCheck.PileShaftCompression, st, "R3", 1.15);
            Define(GeotechnicalCheck.PileTotalCompression, st, "R3", 1.30); Define(GeotechnicalCheck.PileShaftTension, st, "R3", 1.25);
            Define(GeotechnicalCheck.PileTransverse, st, "R3", 1.3);
        }
        protected StandardNTC2018Geotechnics(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public override IReadOnlyList<GeotechnicalCombination> Combinations(GeotechnicalCheck check, GeotechnicalSituation situation)
        {
            bool stability = check == GeotechnicalCheck.GlobalStability || check == GeotechnicalCheck.SlopeStability;
            string set = stability ? "R2" : "R3";
            if (!ResistanceFactor(check, situation, set).HasValue) return new GeotechnicalCombination[0];
            if (situation == GeotechnicalSituation.Seismic)
                return new[] { Combination(check, situation, Seismic, M1, set, "NTC 2018 §7.11 (A = 1, M1, γR seismic)") };
            return new[] { stability ? Combination(check, situation, A2, M2, set, "NTC 2018 §6.8.2 (A2+M2+R2)")
                : Combination(check, situation, A1, M1, set, "NTC 2018 §6.4-6.5 Approccio 2 (A1+M1+R3)") };
        }

        // Tab. 6.4.IV: n = 1, 2, 3, 4, 5, 7, ≥ 10.
        public override Tuple<double, double> PileCorrelationFactors(int investigatedProfiles)
            => Tabulated(investigatedProfiles, new[] { 1, 2, 3, 4, 5, 7, 10 }, new[] { 1.70, 1.65, 1.60, 1.55, 1.50, 1.45, 1.40 }, new[] { 1.70, 1.55, 1.48, 1.42, 1.34, 1.28, 1.21 });
    }
}
