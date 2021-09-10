using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
    public class StandardEn1992p11 : Standard
    {
        #region VARIABLES

        private readonly double _gammaC;
        private readonly double _gammaCAccidental;
        private readonly double _gammaCE;
        private readonly double _gammaS;
        private readonly double _gammaSAccidental;
        private readonly double _gammaSPrestress;
        private readonly double _gammaSPrestressAccidental;

        private readonly double _alphaCC;
        private readonly double _alphaCT;

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




        public StandardEn1992p11()
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

		}

	}
}
