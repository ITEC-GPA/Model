using System;
using System.Runtime.Serialization;

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

        /// <inheritdoc cref="GammaC"/>
        protected double _gammaC;
        /// <inheritdoc cref="GammaCAccidental"/>
        protected double _gammaCAccidental;
        /// <inheritdoc cref="GammaCE"/>
        protected double _gammaCE;

        /// <inheritdoc cref="GammaS"/>
        protected double _gammaS;
        /// <inheritdoc cref="GammaSAccidental"/>
        protected double _gammaSAccidental;
        /// <inheritdoc cref="GammaSPrestress"/>
        protected double _gammaSPrestress;
        /// <inheritdoc cref="GammaSPrestressAccidental"/>
        protected double _gammaSPrestressAccidental;

        /// <inheritdoc cref="AlphaCC"/>
        protected double _alphaCC;
        /// <inheritdoc cref="AlphaCT"/>
        protected double _alphaCT;

        /// <inheritdoc cref="SteelCoefficientStrainTension"/>
        protected double _steelCoefficientStrainTension;

        /// <inheritdoc cref="GammaF"/>
        protected double _gammaF;

        /// <inheritdoc cref="ServiceabilityStressConcreteCoefficientForCharacteristicCombination"/>
        protected double _serviceabilityStressConcreteCoefficientForCharacteristicCombination;
        /// <inheritdoc cref="ServiceabilityStressConcreteCoefficientForQuasiPermanentCombination"/>
        protected double _serviceabilityStressConcreteCoefficientForQuasiPermanentCombination;
        /// <inheritdoc cref="ServiceabilityStressSteelCoefficientForCharacteristicCombination"/>
        protected double _serviceabilityStressSteelCoefficientForCharacteristicCombination;
        /// <inheritdoc cref="ServiceabilityStressPrestressSteelCoefficientForCharacteristicCombination"/>
        protected double _serviceabilityStressPrestressSteelCoefficientForCharacteristicCombination;

        #endregion

        #region Properties

        /// <summary>
        /// Partial safety factor for concrete for persistent design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaC { get => _gammaC; set => _gammaC = value; }

        /// <summary>
        /// Partial safety factor for concrete for accidental design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaCAccidental { get => _gammaCAccidental; set => _gammaCAccidental = value; }

        /// <summary>
        /// Partial safety factor fr ultimate limit state for elastic modulus
        /// </summary>
        public double GammaCE { get => _gammaCE; set => _gammaCE = value; }

        /// <summary>
        /// Partial safety factor for reinforcing steel for persistent design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaS { get => _gammaS; set => _gammaS = value; }

        /// <summary>
        /// Partial safety factor for reinforcing steel for accidental design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaSAccidental { get => _gammaSAccidental; set => _gammaSAccidental = value; }

        /// <summary>
        /// Partial safety factor for prestressing steel for persistent design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaSPrestress { get => _gammaSPrestress; set => _gammaSPrestress = value; }

        /// <summary>
        /// Partial safety factor for prestressing steel for accidental design situations. See EN1992-1-1 §3.1.6
        /// </summary>
        public double GammaSPrestressAccidental { get => _gammaSPrestressAccidental; set => _gammaSPrestressAccidental = value; }

        /// <summary>
        /// Coefficient taking account of long term effects on the compressive strength and
        /// of unfavourable effects resulting from the way the load is applied. See EN1992-1-1 §3.1.6
        /// </summary>
        public double AlphaCC { get => _alphaCC; set => _alphaCC = value; }

        /// <summary>
        /// coefficient taking account of long term effects on the tensile strength and of
        /// unfavourable effects, resulting from the way the load is applied. See EN1992-1-1 §3.1.6
        /// </summary>
        public double AlphaCT { get => _alphaCT; set => _alphaCT = value; }

        /// <summary>
        /// Partial safety factor for FRC in tension (residual strength)
        /// </summary>
        public double GammaF { get => _gammaF; set => _gammaF = value; }

        /// <summary>
        /// Reduction coefficient for ultimate steel strain. 7.2.3.2
        /// </summary>
        public double SteelCoefficientStrainTension { get => _steelCoefficientStrainTension; set => _steelCoefficientStrainTension = value; }

        /// <summary>
        /// Coefficient for serviceability limit state 7.2
        /// </summary>
        public double ServiceabilityStressConcreteCoefficientForCharacteristicCombination { get => _serviceabilityStressConcreteCoefficientForCharacteristicCombination; set => _serviceabilityStressConcreteCoefficientForCharacteristicCombination = value; }

        /// <summary>
        /// Coefficient for serviceability limit state 7.2
        /// </summary>
        public double ServiceabilityStressConcreteCoefficientForQuasiPermanentCombination { get => _serviceabilityStressConcreteCoefficientForQuasiPermanentCombination; set => _serviceabilityStressConcreteCoefficientForQuasiPermanentCombination = value; }

        /// <summary>
        /// Coefficient for serviceability limit state 7.2
        /// </summary>
        public double ServiceabilityStressSteelCoefficientForCharacteristicCombination { get => _serviceabilityStressSteelCoefficientForCharacteristicCombination; set => _serviceabilityStressSteelCoefficientForCharacteristicCombination = value; }
        
        /// <summary>
        /// Coefficient for serviceability limit state 7.2
        /// </summary>
        public double ServiceabilityStressPrestressSteelCoefficientForCharacteristicCombination { get => _serviceabilityStressPrestressSteelCoefficientForCharacteristicCombination; set => _serviceabilityStressPrestressSteelCoefficientForCharacteristicCombination = value; }

        /// <summary>
        /// The group of the standard: European
        /// </summary>
        public override StandardGroupType StandardGroup => StandardGroupType.European;

        #endregion

        #region Public Constructor

        /// <summary>
        /// Default Constructor
        /// </summary>
        public StandardModelCode2010(string name = "Fib Model Code 2010", string remarks = "Fib Model Code 2010. March 2010")
            : base(name, remarks)
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
            _serviceabilityStressConcreteCoefficientForCharacteristicCombination = 0.6;
            _serviceabilityStressConcreteCoefficientForQuasiPermanentCombination = 0.45;
            _serviceabilityStressSteelCoefficientForCharacteristicCombination = 0.8;
            _serviceabilityStressPrestressSteelCoefficientForCharacteristicCombination = 0.75;
        }

        /// <summary>
        /// Creates the standard with the default remarks
        /// </summary>
        /// <param name="name">The name</param>
        public StandardModelCode2010(string name = "Fib Model Code 2010")
            : this(name, "Fib Model Code 2010. March 2010")
        {
        }

        /// <summary>
        /// Creates the standard with the default name and remarks
        /// </summary>
        public StandardModelCode2010()
            : this("Fib Model Code 2010", "Fib Model Code 2010. March 2010")
        {
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected StandardModelCode2010(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("StandardModelCode2010Version");
            }
            catch (Exception)
            {
                version = 1;
            }

            if (version == 1)
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
                _serviceabilityStressConcreteCoefficientForCharacteristicCombination = info.GetDouble("ServiceabilityStressConcreteCoefficientForCharacteristicCombination");
                _serviceabilityStressConcreteCoefficientForQuasiPermanentCombination = info.GetDouble("ServiceabilityStressConcreteCoefficientForQuasiPermanentCombination");
                _serviceabilityStressSteelCoefficientForCharacteristicCombination = info.GetDouble("ServiceabilityStressStessCoefficientForCharacteristicCombination");
                _serviceabilityStressPrestressSteelCoefficientForCharacteristicCombination = info.GetDouble("ServiceabilityStressPrestressSteelCoefficientForCharacteristicCombination");
            }
        }

        #endregion

        #region Equals - hashcode - operators

        /// <summary>
        /// Serializes the object
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 1;

            info.AddValue("StandardModelCode2010Version", version);

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
            info.AddValue("ServiceabilityStressConcreteCoefficientForCharacteristicCombination", _serviceabilityStressConcreteCoefficientForCharacteristicCombination);
            info.AddValue("ServiceabilityStressConcreteCoefficientForQuasiPermanentCombination", _serviceabilityStressConcreteCoefficientForQuasiPermanentCombination);
            info.AddValue("ServiceabilityStressStessCoefficientForCharacteristicCombination", _serviceabilityStressSteelCoefficientForCharacteristicCombination);
            info.AddValue("ServiceabilityStressPrestressSteelCoefficientForCharacteristicCombination", _serviceabilityStressPrestressSteelCoefficientForCharacteristicCombination);
        }

        /// <summary>
        /// Equality with an object of the same type
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is equal</returns>
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
                   _gammaF == code._gammaF &&
                   _serviceabilityStressConcreteCoefficientForCharacteristicCombination == code.ServiceabilityStressConcreteCoefficientForCharacteristicCombination &&
                   _serviceabilityStressConcreteCoefficientForQuasiPermanentCombination == code.ServiceabilityStressConcreteCoefficientForQuasiPermanentCombination &&
                   _serviceabilityStressSteelCoefficientForCharacteristicCombination == code.ServiceabilityStressSteelCoefficientForCharacteristicCombination &&
                   _serviceabilityStressPrestressSteelCoefficientForCharacteristicCombination == code.ServiceabilityStressPrestressSteelCoefficientForCharacteristicCombination &&
                   base.Equals(code);
        }

        /// <summary>
        /// The hash code of the coefficients and of the base
        /// </summary>
        /// <returns>The hash code</returns>
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
            hashCode = hashCode * -17 + _serviceabilityStressConcreteCoefficientForCharacteristicCombination.GetHashCode();
            hashCode = hashCode * -17 + _serviceabilityStressConcreteCoefficientForQuasiPermanentCombination.GetHashCode();
            hashCode = hashCode * -17 + _serviceabilityStressSteelCoefficientForCharacteristicCombination.GetHashCode();
            hashCode = hashCode * -17 + _serviceabilityStressPrestressSteelCoefficientForCharacteristicCombination.GetHashCode();
            return hashCode;
        }

        #endregion
    }
}
