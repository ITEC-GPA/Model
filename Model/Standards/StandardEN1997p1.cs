using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>Design approaches of EN 1997-1 §2.4.7.3.4 (nationally determined).</summary>
    public enum GeotechnicalDesignApproach { DA1, DA2, DA3 }

    /// <summary>Execution of the piles, which selects the resistance factors of EN 1997-1 Tables A.6-A.8.</summary>
    public enum PileExecution { Driven, Bored, ContinuousFlightAuger }

    /// <summary>
    /// EN 1997-1:2004 recommended values of Annex A: A1, A2, EQU (A.1-A.3), M1, M2 (A.4), R1-R4 for spread foundations (A.5),
    /// piles (A.6-A.8), retaining structures (A.13) and slopes and overall stability (A.14), correlation factors ξ3, ξ4 (A.10).
    /// The design approach and the pile execution are explicit. Overturning of walls is an EQU check (EQU actions, M2 materials, γR = 1).
    /// Seismic situations (EN 1998-5) are not defined here (no combination). National annexes override values explicitly.
    /// </summary>
    [Serializable]
    public class StandardEN1997p1 : StandardGeotechnical
    {
        public override string Edition => "2004/AC:2009";
        private GeotechnicalDesignApproach _approach;
        private PileExecution _piles;
        public GeotechnicalDesignApproach DesignApproach { get => _approach; set => _approach = value; }
        public PileExecution PileExecution { get => _piles; set => _piles = value; }

        public StandardEN1997p1() : this("EN 1997-1", GeotechnicalDesignApproach.DA1) { }
        public StandardEN1997p1(string name, GeotechnicalDesignApproach approach, PileExecution piles = PileExecution.Bored, string remarks = "EN 1997-1:2004 + AC:2009, Annex A recommended values")
            : base(name, remarks)
        {
            _approach = approach; _piles = piles;
            SetActionSet(new GeotechnicalActionFactors("A1", 1.35, 1.0, 1.35, 1.0, 1.5, 0));
            SetActionSet(new GeotechnicalActionFactors("A2", 1.0, 1.0, 1.0, 1.0, 1.3, 0));
            SetActionSet(new GeotechnicalActionFactors("EQU", 1.1, .9, 1.1, .9, 1.5, 0));
            SetMaterialSet(new GeotechnicalMaterialFactors("M1", 1.0, 1.0, 1.0, 1.0, 1.0));
            SetMaterialSet(new GeotechnicalMaterialFactors("M2", 1.25, 1.25, 1.4, 1.4, 1.0));
            var st = GeotechnicalSituation.PersistentTransient;
            foreach (var r in new[] { "R1", "R2", "R3" })
            {
                bool r2 = r == "R2";
                Define(GeotechnicalCheck.ShallowFoundationBearing, st, r, r2 ? 1.4 : 1.0); Define(GeotechnicalCheck.ShallowFoundationSliding, st, r, r2 ? 1.1 : 1.0);
                Define(GeotechnicalCheck.RetainingWallBearing, st, r, r2 ? 1.4 : 1.0); Define(GeotechnicalCheck.RetainingWallSliding, st, r, r2 ? 1.1 : 1.0);
                Define(GeotechnicalCheck.RetainingWallPassiveResistance, st, r, r2 ? 1.4 : 1.0);
                Define(GeotechnicalCheck.GlobalStability, st, r, r2 ? 1.1 : 1.0); Define(GeotechnicalCheck.SlopeStability, st, r, r2 ? 1.1 : 1.0);
            }
            Define(GeotechnicalCheck.RetainingWallOverturning, st, "EQU", 1.0);
            // Tables A.6 (driven), A.7 (bored), A.8 (CFA): base, shaft, total, shaft in tension for R1, R2, R3, R4.
            var table = new Dictionary<PileExecution, double[][]>
            {
                { PileExecution.Driven, new[] { new[] { 1.0, 1.0, 1.0, 1.25 }, new[] { 1.1, 1.1, 1.1, 1.15 }, new[] { 1.0, 1.0, 1.0, 1.1 }, new[] { 1.3, 1.3, 1.3, 1.6 } } },
                { PileExecution.Bored, new[] { new[] { 1.25, 1.0, 1.15, 1.25 }, new[] { 1.1, 1.1, 1.1, 1.15 }, new[] { 1.0, 1.0, 1.0, 1.1 }, new[] { 1.6, 1.3, 1.5, 1.6 } } },
                { PileExecution.ContinuousFlightAuger, new[] { new[] { 1.1, 1.0, 1.1, 1.25 }, new[] { 1.1, 1.1, 1.1, 1.15 }, new[] { 1.0, 1.0, 1.0, 1.1 }, new[] { 1.45, 1.3, 1.4, 1.6 } } }
            };
            foreach (var pair in table)
                for (int r = 0; r < 4; r++)
                {
                    string set = PileSet(pair.Key, r + 1);
                    Define(GeotechnicalCheck.PileBase, st, set, pair.Value[r][0]); Define(GeotechnicalCheck.PileShaftCompression, st, set, pair.Value[r][1]);
                    Define(GeotechnicalCheck.PileTotalCompression, st, set, pair.Value[r][2]); Define(GeotechnicalCheck.PileShaftTension, st, set, pair.Value[r][3]);
                }
        }
        protected StandardEN1997p1(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _approach = (GeotechnicalDesignApproach)info.GetInt32("DesignApproach"); _piles = (PileExecution)info.GetInt32("PileExecution");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("DesignApproach", (int)_approach); info.AddValue("PileExecution", (int)_piles);
        }

        private static string PileSet(PileExecution execution, int r) => "R" + r + "-" + execution;
        private static bool IsPile(GeotechnicalCheck c) => c == GeotechnicalCheck.PileBase || c == GeotechnicalCheck.PileShaftCompression
            || c == GeotechnicalCheck.PileTotalCompression || c == GeotechnicalCheck.PileShaftTension;

        public override IReadOnlyList<GeotechnicalCombination> Combinations(GeotechnicalCheck check, GeotechnicalSituation situation)
        {
            if (situation != GeotechnicalSituation.PersistentTransient || check == GeotechnicalCheck.PileTransverse) return new GeotechnicalCombination[0];
            const string reference = "EN 1997-1 §2.4.7.3.4 and Annex A";
            if (check == GeotechnicalCheck.RetainingWallOverturning)
                return new[] { Combination(check, situation, "EQU", "M2", "EQU", "EN 1997-1 §2.4.7.2 (EQU)") };
            if (IsPile(check))
            {
                switch (_approach)
                {
                    case GeotechnicalDesignApproach.DA1: return new[] { Combination(check, situation, "A1", "M1", PileSet(_piles, 1), reference), Combination(check, situation, "A2", "M1", PileSet(_piles, 4), reference) };
                    case GeotechnicalDesignApproach.DA2: return new[] { Combination(check, situation, "A1", "M1", PileSet(_piles, 2), reference) };
                    default: return new[] { Combination(check, situation, "A2", "M2", PileSet(_piles, 3), reference) };
                }
            }
            switch (_approach)
            {
                case GeotechnicalDesignApproach.DA1: return new[] { Combination(check, situation, "A1", "M1", "R1", reference), Combination(check, situation, "A2", "M2", "R1", reference) };
                case GeotechnicalDesignApproach.DA2: return new[] { Combination(check, situation, "A1", "M1", "R2", reference) };
                // DA3: A1 on structural actions and A2 on geotechnical actions; the geotechnical set is returned.
                default: return new[] { Combination(check, situation, "A2", "M2", "R3", reference) };
            }
        }

        // Table A.10 (from ground test profiles): n = 1, 2, 3, 4, 5, 7, 10.
        public override Tuple<double, double> PileCorrelationFactors(int investigatedProfiles)
            => Tabulated(investigatedProfiles, new[] { 1, 2, 3, 4, 5, 7, 10 }, new[] { 1.40, 1.35, 1.33, 1.31, 1.29, 1.27, 1.25 }, new[] { 1.40, 1.27, 1.23, 1.20, 1.15, 1.12, 1.08 });

        public override bool Equals(object obj) => obj is StandardEN1997p1 s && s._approach == _approach && s._piles == _piles && base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();
    }
}
