using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the Fib Model Code 2010
    /// </summary>
    /// <remarks>Reference: Fib Model Code 2010. March 2010</remarks>
    public class StandardModelCode2010 : Standard
    {
        #region Variables

        protected double _gammaC;
        protected double _gammaCAccidental;
        protected double _gammaCE;

        protected double _gammaS;
        protected double _gammaSAccidental;
        protected double _gammaSPrestress;
        protected double _gammaSPrestressAccidental;

        protected double _alphaCC;
        protected double _alphaCT;

        protected double _concreteLimitStrainPureCompression;
        protected double _steelCoefficientStrainTension;

        protected double _gammaF;

        #endregion

        /// <summary>
        /// Partial safety factor for concrete for persistent design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaC => _gammaC;

        /// <summary>
        /// Partial safety factor for concrete for accidental design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaCAccidental => _gammaCAccidental;

        /// <summary>
        /// Partial safety factor fr ultimate limit state for elastic modulus
        /// </summary>
        public double GammaCE => _gammaCE;

        /// <summary>
        /// Partial safety factor for reinforcing steel for persistent design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaS => _gammaS;

        /// <summary>
        /// Partial safety factor for reinforcing steel for accidental design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaSAccidental => _gammaSAccidental;

        /// <summary>
        /// Partial safety factor for prestressing steel for persistent design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaSPrestress => _gammaSPrestress;

        /// <summary>
        /// Partial safety factor for prestressing steel for accidental design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaSPrestressAccidental => _gammaSPrestressAccidental;

        /// <summary>
        /// Coefficient taking account of long term effects on the compressive strength and
        /// of unfavourable effects resulting from the way the load is applied. See EN1992-1-1 §3.1.6
        /// </summary>
        public double AlphaCC => _alphaCC;

        /// <summary>
        /// coefficient taking account of long term effects on the tensile strength and of
        /// unfavourable effects, resulting from the way the load is applied. See EN1992-1-1 §3.1.6
        /// </summary>
        public double AlphaCT => _alphaCT;

        /// <summary>
        /// Partial safety factor for FRC in tension (residual strength)
        /// </summary>
        public double GammaF => _gammaF;

        /// <summary>
        /// Reduction coefficient for ultimate steel strain. 7.2.3.2
        /// </summary>
        public double SteelCoefficientStrainTension => _steelCoefficientStrainTension;


        /// <summary>
        /// Default Constructor
        /// </summary>
        public StandardModelCode2010()
		{
			_gammaC = 1.5;
            _gammaCAccidental = 1.2;
            _gammaCE = 1.2;
            _gammaS = 1.15;
            _gammaSAccidental = 1.0;
            _gammaSPrestress = 1.15;
            _gammaSPrestressAccidental = 1.0;
            _alphaCC = 1.0;
            _alphaCT = 1.0;
            _gammaF = 1.5;
            _steelCoefficientStrainTension = 0.9;
        }

	}
}
