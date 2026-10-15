using System;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Tension stiffening of cracked concrete: the descending post-cracking branch of the concrete in tension for the nonlinear response analyses
    /// (<see cref="ConcreteMaterial.TensionResponseDiagrams.TensionStiffening"/>). With the strain ε positive in tension:
    /// <list type="bullet">
    /// <item>0 &lt; ε ≤ εcr = fct / Ec: σ = Ec ε, tangent Ec (uncracked);</item>
    /// <item>ε &gt; εcr: σ = fct / (1 + √(500 ε)), tangent dσ/dε = −250 fct / (√(500 ε) (1 + √(500 ε))²), the average tensile stress of cracked
    /// reinforced concrete between the cracks;</item>
    /// <item>ε ≤ 0: σ = 0 and tangent 0, the law describes the tension only (the compression is the diagram of the material).</item>
    /// </list>
    /// At εcr the stress drops from fct to fct / (1 + √(500 εcr)), about 0.82 fct for ordinary concretes, as in the original formulation; beyond it the
    /// stress decreases smoothly and never vanishes, so that an equilibrium iteration does not meet the abrupt drop from fct to zero of the tensile
    /// branch of the material. A function of the strain only: unloading follows the same curve, without damage or crack memory. Mean values, no partial
    /// factors. It is not a resistance law: the ultimate limit state calculations neglect the tensile strength (EN 1992-1-1 6.1(2)P) and never use it.
    /// <para>Source: M. P. Collins, D. Mitchell, Prestressed Concrete Structures, Prentice Hall, Englewood Cliffs 1991, average stress-strain
    /// relationship of cracked concrete in tension f1 = fcr / (1 + √(500 ε1)); the modified compression field theory of F. J. Vecchio, M. P. Collins,
    /// ACI Journal 83(2), 1986, with the coefficient 500 of Collins and Mitchell (200 in the 1986 paper).</para>
    /// </summary>
    public sealed class ConcreteTensionStiffening
    {
        /// <summary>The coefficient of the descending branch, 500 (Collins and Mitchell 1991)</summary>
        public const double Coefficient = 500;

        /// <summary>
        /// The law of an uncracked modulus and a cracking stress
        /// </summary>
        /// <param name="elasticModulus">Ec, the modulus of the uncracked branch (Ecm)</param>
        /// <param name="tensileStrength">fct, the stress at cracking (fctm)</param>
        /// <exception cref="ArgumentOutOfRangeException">For a value not positive and finite</exception>
        public ConcreteTensionStiffening(double elasticModulus, double tensileStrength)
        {
            if (!(elasticModulus > 0) || double.IsInfinity(elasticModulus))
                throw new ArgumentOutOfRangeException(nameof(elasticModulus), "Ec must be positive and finite.");
            if (!(tensileStrength > 0) || double.IsInfinity(tensileStrength))
                throw new ArgumentOutOfRangeException(nameof(tensileStrength), "fct must be positive and finite.");
            ElasticModulus = elasticModulus;
            TensileStrength = tensileStrength;
        }

        /// <summary>Ec, the modulus of the uncracked branch</summary>
        public double ElasticModulus { get; }

        /// <summary>fct, the stress at cracking</summary>
        public double TensileStrength { get; }

        /// <summary>εcr = fct / Ec</summary>
        public double CrackingStrain => TensileStrength / ElasticModulus;

        /// <summary>
        /// The stress and the tangent at a strain (see the class)
        /// </summary>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <param name="stress">σ (positive in tension; 0 for ε ≤ 0)</param>
        /// <param name="tangent">dσ/dε (0 for ε ≤ 0)</param>
        public void Evaluate(double strain, out double stress, out double tangent)
        {
            if (strain <= 0)
            {
                stress = 0;
                tangent = 0;
                return;
            }
            if (strain <= CrackingStrain)
            {
                stress = ElasticModulus * strain;
                tangent = ElasticModulus;
                return;
            }
            double root = Math.Sqrt(Coefficient * strain);
            stress = TensileStrength / (1 + root);
            tangent = -250 * TensileStrength / (root * (1 + root) * (1 + root));
        }

        /// <summary>
        /// The stress at a strain (see the class)
        /// </summary>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <returns>σ (positive in tension; 0 for ε ≤ 0)</returns>
        public double GetStress(double strain)
        {
            Evaluate(strain, out double stress, out _);
            return stress;
        }

        /// <summary>
        /// The tangent at a strain (see the class)
        /// </summary>
        /// <param name="strain">The strain (positive in tension)</param>
        /// <returns>dσ/dε (0 for ε ≤ 0)</returns>
        public double GetTangent(double strain)
        {
            Evaluate(strain, out _, out double tangent);
            return tangent;
        }
    }
}
