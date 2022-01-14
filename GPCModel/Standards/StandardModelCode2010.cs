using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the Fib Model Code 2010
    /// </summary>
    /// <remarks>Reference: Fib Model Code 2010. March 2010</remarks>
    [Serializable]
    public class StandardModelCode2010 : Standard, ISerializable
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

        #region Public Constructor

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

        public StandardModelCode2010(SerializationInfo info, StreamingContext context)
		{
            _gammaC = info.GetDouble("GammaC");
            _gammaCAccidental = info.GetDouble("GammaCAccidental");
            _gammaCE = info.GetDouble("GammaCE");
            _gammaS = info.GetDouble("GammaS");
            _gammaSAccidental = info.GetDouble("GammaSAccidental");
            _gammaSPrestress = info.GetDouble("GammaSPrestress");
            _gammaSPrestressAccidental = info.GetDouble("GammaSPrestressAccidental");
            _alphaCC = info.GetDouble("AlphaCC");
            _alphaCT = info.GetDouble("AlphaCT");
            _gammaF = info.GetDouble("GammaF");
            _steelCoefficientStrainTension = info.GetDouble("SteelCoefficientStrainTension");
        }

        #endregion

        #region Equals - hashcode - operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("GammaC", _gammaC);
            info.AddValue("GammaCAccidental", _gammaCAccidental);
            info.AddValue("GammaCE", _gammaCE);
            info.AddValue("GammaS", _gammaS);
            info.AddValue("GammaSAccidental", _gammaSAccidental);
            info.AddValue("GammaSPrestress", _gammaSPrestress);
            info.AddValue("GammaSPrestressAccidental", _gammaSPrestressAccidental);
            info.AddValue("AlphaCC", _alphaCC);
            info.AddValue("AlphaCT", _alphaCT);
            info.AddValue("GammaF", _gammaF);
            info.AddValue("SteelCoefficientStrainTension", _steelCoefficientStrainTension);
        }

        public override bool Equals(object obj)
		{
            if (ReferenceEquals(this, obj))
                return true;

            return obj is StandardModelCode2010 code &&
                   _gammaC == code._gammaC &&
                   _gammaCAccidental == code._gammaCAccidental &&
                   _gammaCE == code._gammaCE &&
                   _gammaS == code._gammaS &&
                   _gammaSAccidental == code._gammaSAccidental &&
                   _gammaSPrestress == code._gammaSPrestress &&
                   _gammaSPrestressAccidental == code._gammaSPrestressAccidental &&
                   _alphaCC == code._alphaCC &&
                   _alphaCT == code._alphaCT &&
                   _steelCoefficientStrainTension == code._steelCoefficientStrainTension &&
                   _gammaF == code._gammaF;
		}

		public override int GetHashCode()
		{
			int hashCode = 23;
			hashCode = hashCode * -17 + base.GetHashCode();
			hashCode = hashCode * -17 + _gammaC.GetHashCode();
			hashCode = hashCode * -17 + _gammaCAccidental.GetHashCode();
			hashCode = hashCode * -17 + _gammaCE.GetHashCode();
			hashCode = hashCode * -17 + _gammaS.GetHashCode();
			hashCode = hashCode * -17 + _gammaSAccidental.GetHashCode();
			hashCode = hashCode * -17 + _gammaSPrestress.GetHashCode();
			hashCode = hashCode * -17 + _gammaSPrestressAccidental.GetHashCode();
			hashCode = hashCode * -17 + _alphaCC.GetHashCode();
			hashCode = hashCode * -17 + _alphaCT.GetHashCode();
			hashCode = hashCode * -17 + _steelCoefficientStrainTension.GetHashCode();
			hashCode = hashCode * -17 + _gammaF.GetHashCode();
			return hashCode;
		}

		#endregion
	}
}
