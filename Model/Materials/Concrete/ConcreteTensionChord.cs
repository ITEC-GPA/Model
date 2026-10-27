using System;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Tension stiffening on the bars, the tension chord model of the nonlinear response analyses (<see cref="ConcreteMaterial.TensionResponseDiagrams.TensionChord"/>,
    /// fib Model Code 2010 §7.6.4.4, crack formation and stabilised cracking of the tension chord; EN 1992-1-1 7.3.4(2) and 7.4.3(3)). With the strain positive in
    /// tension and the mean strain εm of the plane at the bar:
    /// <list type="bullet">
    /// <item>concrete: σ = Ec ε up to εcr = fct / Ec, zero beyond (cracked concrete carries no tension, the bars carry it at the cracks);</item>
    /// <item>bars: the steel law is read at the equivalent strain ε*, the strain of the bar at the crack: ε* = εm up to εcr (uncracked);
    /// ε* = εm / (1 − β) while the cracks form (εm ≤ (1 − β) εsr); ε* = εm + β εsr when the cracking is stabilised, with
    /// εsr = σsr / Es and σsr = fct (1 + αe ρeff) / ρeff the stress of the bar at the crack when the concrete of its effective area cracks,
    /// αe = Es / Ec and ρeff = As / Ac,eff of the effective area of the bar. The two branches meet at εm = (1 − β) εsr; in compression ε* = εm.</item>
    /// </list>
    /// β = 0.4 for long-term or repeated loading (kt of EN 1992-1-1 7.3.4(2) and NTC 2018, β of fib Model Code 2010), 0.6 for short-term
    /// loading. Mean values, no partial factors; never used by the resistance calculations (EN 1992-1-1 6.1(2)P). A function of the strain only,
    /// without crack memory. Decision of the ANTHEA user of 26-27/10/2026 (BUG-TS-2, «TS-2 scelgo 1», «β = 0,4»).
    /// </summary>
    public sealed class ConcreteTensionChord
    {
        /// <summary>β of long-term or repeated loading</summary>
        public const double LongTermBeta = .4;

        /// <summary>
        /// The law of a concrete modulus, a cracking stress, a modulus of the bars and β
        /// </summary>
        /// <param name="elasticModulus">Ec, the modulus of the uncracked concrete (Ecm)</param>
        /// <param name="tensileStrength">fct, the stress at cracking (fctm)</param>
        /// <param name="steelModulus">Es, the modulus of the bars</param>
        /// <param name="beta">β, 0 ≤ β &lt; 1 (0.4 long-term, 0.6 short-term)</param>
        /// <exception cref="ArgumentOutOfRangeException">For a modulus or a strength not positive and finite, or β outside [0; 1)</exception>
        public ConcreteTensionChord(double elasticModulus, double tensileStrength, double steelModulus, double beta = LongTermBeta)
        {
            if (!(elasticModulus > 0) || double.IsInfinity(elasticModulus))
                throw new ArgumentOutOfRangeException(nameof(elasticModulus), "Ec must be positive and finite.");
            if (!(tensileStrength > 0) || double.IsInfinity(tensileStrength))
                throw new ArgumentOutOfRangeException(nameof(tensileStrength), "fct must be positive and finite.");
            if (!(steelModulus > 0) || double.IsInfinity(steelModulus))
                throw new ArgumentOutOfRangeException(nameof(steelModulus), "Es must be positive and finite.");
            if (!(beta >= 0 && beta < 1))
                throw new ArgumentOutOfRangeException(nameof(beta), "β must be in [0; 1).");
            ElasticModulus = elasticModulus;
            TensileStrength = tensileStrength;
            SteelModulus = steelModulus;
            Beta = beta;
        }

        /// <summary>Ec, the modulus of the uncracked concrete</summary>
        public double ElasticModulus { get; }

        /// <summary>fct, the stress at cracking</summary>
        public double TensileStrength { get; }

        /// <summary>Es, the modulus of the bars</summary>
        public double SteelModulus { get; }

        /// <summary>β</summary>
        public double Beta { get; }

        /// <summary>εcr = fct / Ec</summary>
        public double CrackingStrain => TensileStrength / ElasticModulus;

        /// <summary>
        /// The concrete in tension: σ = Ec ε up to εcr, zero beyond and for ε ≤ 0 (the compression is the diagram of the material)
        /// </summary>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="stress">σ</param>
        /// <param name="tangent">dσ/dε</param>
        public void EvaluateConcrete(double strain, out double stress, out double tangent)
        {
            if (strain > 0 && strain <= CrackingStrain)
            {
                stress = ElasticModulus * strain;
                tangent = ElasticModulus;
                return;
            }
            stress = 0;
            tangent = 0;
        }

        /// <summary>
        /// σsr = fct (1 + αe ρeff) / ρeff, the stress of the bar at the crack when the concrete of its effective area cracks
        /// </summary>
        /// <param name="effectiveRatio">ρeff = As / Ac,eff (positive)</param>
        /// <returns>σsr</returns>
        /// <exception cref="ArgumentOutOfRangeException">For a ratio not positive and finite</exception>
        public double CrackingBarStress(double effectiveRatio)
        {
            if (!(effectiveRatio > 0) || double.IsInfinity(effectiveRatio))
                throw new ArgumentOutOfRangeException(nameof(effectiveRatio), "ρeff must be positive and finite.");
            return TensileStrength * (1 + SteelModulus / ElasticModulus * effectiveRatio) / effectiveRatio;
        }

        /// <summary>
        /// The equivalent strain of a bar, at which its steel law gives the stress at the crack (see the class)
        /// </summary>
        /// <param name="meanStrain">εm, the strain of the plane at the bar (positive in tension)</param>
        /// <param name="effectiveRatio">ρeff of the bar (positive)</param>
        /// <param name="derivative">dε*/dεm</param>
        /// <returns>ε*</returns>
        public double EquivalentStrain(double meanStrain, double effectiveRatio, out double derivative)
        {
            if (meanStrain <= CrackingStrain)
            {
                derivative = 1;
                return meanStrain;
            }
            double srStrain = CrackingBarStress(effectiveRatio) / SteelModulus;
            if (meanStrain <= (1 - Beta) * srStrain)
            {
                derivative = 1 / (1 - Beta);
                return meanStrain / (1 - Beta);
            }
            derivative = 1;
            return meanStrain + Beta * srStrain;
        }
    }
}
