using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// EN 1993: Eurocode 3
    /// </summary>
    [Serializable]
    public class StandardEN1993p11 : Standard, ISerializable
    {
        #region Variables

        /// <inheritdoc cref="GammaM0"/>
        protected double _gammaM0;
        /// <inheritdoc cref="GammaM1"/>
        protected double _gammaM1;
        /// <inheritdoc cref="GammaM2"/>
        protected double _gammaM2;
        /// <inheritdoc cref="GammaM3"/>
        protected double _gammaM3; // EN1993-1-8
        /// <inheritdoc cref="GammaM3Ser"/>
        protected double _gammaM3Ser;
        /// <inheritdoc cref="GammaM4"/>
        protected double _gammaM4;
        /// <inheritdoc cref="GammaM5"/>
        protected double _gammaM5;
        /// <inheritdoc cref="GammaM6Ser"/>
        protected double _gammaM6Ser;
        /// <inheritdoc cref="GammaM7"/>
        protected double _gammaM7;

        /// <inheritdoc cref="NShearBucklingLowGradeOfSteel"/>
        protected double _nShearBucklingLowGradeOfSteel;
        /// <inheritdoc cref="NShearBucklingHighGradeOfSteel"/>
        protected double _nShearBucklingHighGradeOfSteel;

        /// <inheritdoc cref="AlphaImperfectionFactorForCurveA0"/>
        protected double _alphaImperfectionFactorForCurveA0;
        /// <inheritdoc cref="AlphaImperfectionFactorForCurveA"/>
        protected double _alphaImperfectionFactorForCurveA;
        /// <inheritdoc cref="AlphaImperfectionFactorForCurveB"/>
        protected double _alphaImperfectionFactorForCurveB;
        /// <inheritdoc cref="AlphaImperfectionFactorForCurveC"/>
        protected double _alphaImperfectionFactorForCurveC;
        /// <inheritdoc cref="AlphaImperfectionFactorForCurveD"/>
        protected double _alphaImperfectionFactorForCurveD;

        /// <inheritdoc cref="AlphaLTImperfectionFactorForCurveA"/>
        protected double _alphaLTImperfectionFactorForCurveA;
        /// <inheritdoc cref="AlphaLTImperfectionFactorForCurveB"/>
        protected double _alphaLTImperfectionFactorForCurveB;
        /// <inheritdoc cref="AlphaLTImperfectionFactorForCurveC"/>
        protected double _alphaLTImperfectionFactorForCurveC;
        /// <inheritdoc cref="AlphaLTImperfectionFactorForCurveD"/>
        protected double _alphaLTImperfectionFactorForCurveD;

        /// <inheritdoc cref="BetaForLateralTorsionalBuckling"/>
        protected double _betaForLateralTorsionalBuckling;
        /// <inheritdoc cref="LambdaLT0ForLateralTorsionalBuckling"/>
        protected double _lambdaLT0ForLateralTorsionalBuckling;
        /// <inheritdoc cref="BetaForLateralTorsionalBucklingMod"/>
        protected double _betaForLateralTorsionalBucklingMod;
        /// <inheritdoc cref="LambdaLT0ForLateralTorsionalBucklingMod"/>
        protected double _lambdaLT0ForLateralTorsionalBucklingMod;

        #endregion

        #region Properties

        /// <summary>
        /// EN 1993-1-1:2022 definition. --> Partial factor for resistance of cross-sections.
        /// </summary>
        public double GammaM0 { get => _gammaM0; set => _gammaM0 = value; }

        /// <summary>
        /// EN 1993-1-1:2022 definition. --> Partial factor for resistance of members to instability assessed by member checks.
        /// </summary>
        public double GammaM1 { get => _gammaM1; set => _gammaM1 = value; }

        /// <summary>
        /// EN 1993-1-1:2022 definition. --> Partial factor for resistance of cross-sections in tension to fracture.
        /// </summary>
        public double GammaM2 { get => _gammaM2; set => _gammaM2 = value; }

        /// <summary>
        /// EN 1993-1-8:2005 definition. --> Slip resistance at ultimate limit state (Category C).
        /// </summary>
        public double GammaM3 { get => _gammaM3; set => _gammaM3 = value; }

        /// <summary>
        /// EN 1993-1-8:2005 definition. --> Slip resistance at serviceability limit state (Category B).
        /// </summary>
        public double GammaM3Ser { get => _gammaM3Ser; set => _gammaM3Ser = value; }

        /// <summary>
        /// EN 1993-1-8:2005 definition. --> Bearing resistance of an injection bolt.
        /// </summary>
        public double GammaM4 { get => _gammaM4; set => _gammaM4 = value; }

        /// <summary>
        /// EN 1993-1-8:2005 definition. --> Resistance of joints in hollow section lattice girder.
        /// </summary>
        public double GammaM5 { get => _gammaM5; set => _gammaM5 = value; }

        /// <summary>
        /// EN 1993-1-8:2005 definition. --> Resistance of pins at serviceability limit state.
        /// </summary>
        public double GammaM6Ser { get => _gammaM6Ser; set => _gammaM6Ser = value; }

        /// <summary>
        /// EN 1993-1-8:2005 definition. --> Preload of high strength bolts.
        /// </summary>
        public double GammaM7 { get => _gammaM7; set => _gammaM7 = value; }

        /// <summary>
        /// η of the shear buckling (EN 1993-1-5 5.1) for steel grades up to S460 (1.20)
        /// </summary>
        public double NShearBucklingLowGradeOfSteel { get => _nShearBucklingLowGradeOfSteel; set => _nShearBucklingLowGradeOfSteel = value; }
        /// <summary>
        /// η of the shear buckling (EN 1993-1-5 5.1) for steel grades higher than S460 (1.00)
        /// </summary>
        public double NShearBucklingHighGradeOfSteel { get => _nShearBucklingHighGradeOfSteel; set => _nShearBucklingHighGradeOfSteel = value; }

        /// <summary>
        /// Imperfection factor α of the buckling curve a0 ("Table 6.1: Imperfection factors for buckling curves" in EN 1993-1-1:2005).
        /// </summary>
        public double AlphaImperfectionFactorForCurveA0 { get => _alphaImperfectionFactorForCurveA0; set => _alphaImperfectionFactorForCurveA0 = value; }
        /// <summary>
        /// Imperfection factor α of the buckling curve a (EN 1993-1-1:2005 Table 6.1).
        /// </summary>
        public double AlphaImperfectionFactorForCurveA { get => _alphaImperfectionFactorForCurveA; set => _alphaImperfectionFactorForCurveA = value; }
        /// <summary>
        /// Imperfection factor α of the buckling curve b (EN 1993-1-1:2005 Table 6.1).
        /// </summary>
        public double AlphaImperfectionFactorForCurveB { get => _alphaImperfectionFactorForCurveB; set => _alphaImperfectionFactorForCurveB = value; }
        /// <summary>
        /// Imperfection factor α of the buckling curve c (EN 1993-1-1:2005 Table 6.1).
        /// </summary>
        public double AlphaImperfectionFactorForCurveC { get => _alphaImperfectionFactorForCurveC; set => _alphaImperfectionFactorForCurveC = value; }
        /// <summary>
        /// Imperfection factor α of the buckling curve d (EN 1993-1-1:2005 Table 6.1).
        /// </summary>
        public double AlphaImperfectionFactorForCurveD { get => _alphaImperfectionFactorForCurveD; set => _alphaImperfectionFactorForCurveD = value; }

        /// <summary>
        /// Imperfection factor αLT of the lateral torsional buckling curve a ("Table 6.3: Recommended values for imperfection factors for lateral
        /// torsional buckling curves" in EN 1993-1-1:2005).
        /// </summary>
        public double AlphaLTImperfectionFactorForCurveA { get => _alphaLTImperfectionFactorForCurveA; set => _alphaLTImperfectionFactorForCurveA = value; }
        /// <summary>
        /// Imperfection factor αLT of the lateral torsional buckling curve b (EN 1993-1-1:2005 Table 6.3).
        /// </summary>
        public double AlphaLTImperfectionFactorForCurveB { get => _alphaLTImperfectionFactorForCurveB; set => _alphaLTImperfectionFactorForCurveB = value; }
        /// <summary>
        /// Imperfection factor αLT of the lateral torsional buckling curve c (EN 1993-1-1:2005 Table 6.3).
        /// </summary>
        public double AlphaLTImperfectionFactorForCurveC { get => _alphaLTImperfectionFactorForCurveC; set => _alphaLTImperfectionFactorForCurveC = value; }
        /// <summary>
        /// Imperfection factor αLT of the lateral torsional buckling curve d (EN 1993-1-1:2005 Table 6.3).
        /// </summary>
        public double AlphaLTImperfectionFactorForCurveD { get => _alphaLTImperfectionFactorForCurveD; set => _alphaLTImperfectionFactorForCurveD = value; }

        /// <summary>
        /// β in Φ_LT for "6.3.2.2 Lateral torsional buckling curves General case" in EN 1993-1-1:2005 (1.0: the general formula).
        /// </summary>
        public double BetaForLateralTorsionalBuckling { get => _betaForLateralTorsionalBuckling; set => _betaForLateralTorsionalBuckling = value; }
        /// <summary>
        /// λ̄LT,0 in Φ_LT for "6.3.2.2 Lateral torsional buckling curves General case" in EN 1993-1-1:2005 (0.2: the general formula).
        /// </summary>
        public double LambdaLT0ForLateralTorsionalBuckling { get => _lambdaLT0ForLateralTorsionalBuckling; set => _lambdaLT0ForLateralTorsionalBuckling = value; }

        /// <summary>
        /// β in Φ_LT, from "6.3.2.3 Lateral torsional buckling curves for rolled sections or equivalent welded sections" (0.75).
        /// </summary>
        public double BetaForLateralTorsionalBucklingMod { get => _betaForLateralTorsionalBucklingMod; set => _betaForLateralTorsionalBucklingMod = value; }
        /// <summary>
        /// λ̄LT,0 in Φ_LT, from "6.3.2.3 Lateral torsional buckling curves for rolled sections or equivalent welded sections" (0.4).
        /// </summary>
        public double LambdaLT0ForLateralTorsionalBucklingMod { get => _lambdaLT0ForLateralTorsionalBucklingMod; set => _lambdaLT0ForLateralTorsionalBucklingMod = value; }

        /// <summary>
        /// The group of the standard: European
        /// </summary>
        public override StandardGroupType StandardGroup => StandardGroupType.European;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the standard
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="remarks">The remarks</param>
        public StandardEN1993p11(string name = "EN 1993:2005", string remarks = "Eurocode 3")
            : base(name, remarks)
        {
            _gammaM0 = 1.00; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _gammaM1 = 1.00; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _gammaM2 = 1.25; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _gammaM3 = 1.25; // EN 1993-1-8:2005, BS EN 1993-1-8:2005
            _gammaM3Ser = 1.1; // EN 1993-1-8:2005, BS EN 1993-1-8:2005
            _gammaM4 = 1.0; // EN 1993-1-8:2005, BS EN 1993-1-8:2005
            _gammaM5 = 1.0; // EN 1993-1-8:2005, BS EN 1993-1-8:2005
            _gammaM6Ser = 1.0; // EN 1993-1-8:2005, BS EN 1993-1-8:2005
            _gammaM7 = 1.1; // EN 1993-1-8:2005, BS EN 1993-1-8:2005

            _nShearBucklingLowGradeOfSteel = 1.20;
            _nShearBucklingHighGradeOfSteel = 1.00;

            _alphaImperfectionFactorForCurveA0 = 0.13; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _alphaImperfectionFactorForCurveA = 0.21; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _alphaImperfectionFactorForCurveB = 0.34; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _alphaImperfectionFactorForCurveC = 0.49; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _alphaImperfectionFactorForCurveD = 0.76; // EN 1993-1-1:2005, BS EN 1993-1-1:2005

            _alphaLTImperfectionFactorForCurveA = 0.21; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _alphaLTImperfectionFactorForCurveB = 0.34; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _alphaLTImperfectionFactorForCurveC = 0.49; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _alphaLTImperfectionFactorForCurveD = 0.76; // EN 1993-1-1:2005, BS EN 1993-1-1:2005

            _betaForLateralTorsionalBuckling = 1.0; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _lambdaLT0ForLateralTorsionalBuckling = 0.2; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _betaForLateralTorsionalBucklingMod = 0.75; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
            _lambdaLT0ForLateralTorsionalBucklingMod = 0.40; // EN 1993-1-1:2005, BS EN 1993-1-1:2005
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected StandardEN1993p11(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _gammaM0 = info.GetDouble("GammaM0");
            _gammaM1 = info.GetDouble("GammaM1");
            _gammaM2 = info.GetDouble("GammaM2");
            _gammaM3 = info.GetDouble("GammaM3");
            _gammaM3Ser = info.GetDouble("GammaM3Ser");
            _gammaM4 = info.GetDouble("GammaM4");
            _gammaM5 = info.GetDouble("GammaM5");
            _gammaM6Ser = info.GetDouble("GammaM6Ser");
            _gammaM7 = info.GetDouble("GammaM7");
            _nShearBucklingLowGradeOfSteel = info.GetDouble("NShearBucklingLowGradeOfSteel");
            _nShearBucklingHighGradeOfSteel = info.GetDouble("NShearBucklingHighGradeOfSteel");
            _alphaImperfectionFactorForCurveA0 = info.GetDouble("AlphaImperfectionFactorForCurveA0");
            _alphaImperfectionFactorForCurveA = info.GetDouble("AlphaImperfectionFactorForCurveA");
            _alphaImperfectionFactorForCurveB = info.GetDouble("AlphaImperfectionFactorForCurveB");
            _alphaImperfectionFactorForCurveC = info.GetDouble("AlphaImperfectionFactorForCurveC");
            _alphaImperfectionFactorForCurveD = info.GetDouble("AlphaImperfectionFactorForCurveD");
            _alphaLTImperfectionFactorForCurveA = info.GetDouble("AlphaLTImperfectionFactorForCurveA");
            _alphaLTImperfectionFactorForCurveB = info.GetDouble("AlphaLTImperfectionFactorForCurveB");
            _alphaLTImperfectionFactorForCurveC = info.GetDouble("AlphaLTImperfectionFactorForCurveC");
            _alphaLTImperfectionFactorForCurveD = info.GetDouble("AlphaLTImperfectionFactorForCurveD");

            _betaForLateralTorsionalBuckling = info.GetDouble("BetaForLateralTorsionalBuckling");
            _lambdaLT0ForLateralTorsionalBuckling = info.GetDouble("LambdaLT0ForLateralTorsionalBuckling");
            _betaForLateralTorsionalBucklingMod = info.GetDouble("BetaForLateralTorsionalBucklingMod");
            _lambdaLT0ForLateralTorsionalBucklingMod = info.GetDouble("LambdaLT0ForLateralTorsionalBucklingMod");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Serializes the object
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GammaM0", _gammaM0);
            info.AddValue("GammaM1", _gammaM1);
            info.AddValue("GammaM2", _gammaM2);
            info.AddValue("GammaM3", _gammaM3);
            info.AddValue("GammaM3Ser", _gammaM3Ser);
            info.AddValue("GammaM4", _gammaM4);
            info.AddValue("GammaM5", _gammaM5);
            info.AddValue("GammaM6Ser", _gammaM6Ser);
            info.AddValue("GammaM7", _gammaM7);
            info.AddValue("NShearBucklingLowGradeOfSteel", _nShearBucklingLowGradeOfSteel);
            info.AddValue("NShearBucklingHighGradeOfSteel", _nShearBucklingHighGradeOfSteel);
            info.AddValue("AlphaImperfectionFactorForCurveA0", _alphaImperfectionFactorForCurveA0);
            info.AddValue("AlphaImperfectionFactorForCurveA", _alphaImperfectionFactorForCurveA);
            info.AddValue("AlphaImperfectionFactorForCurveB", _alphaImperfectionFactorForCurveB);
            info.AddValue("AlphaImperfectionFactorForCurveC", _alphaImperfectionFactorForCurveC);
            info.AddValue("AlphaImperfectionFactorForCurveD", _alphaImperfectionFactorForCurveD);
            info.AddValue("AlphaLTImperfectionFactorForCurveA", _alphaLTImperfectionFactorForCurveA);
            info.AddValue("AlphaLTImperfectionFactorForCurveB", _alphaLTImperfectionFactorForCurveB);
            info.AddValue("AlphaLTImperfectionFactorForCurveC", _alphaLTImperfectionFactorForCurveC);
            info.AddValue("AlphaLTImperfectionFactorForCurveD", _alphaLTImperfectionFactorForCurveD);
            info.AddValue("BetaForLateralTorsionalBuckling", _betaForLateralTorsionalBuckling);
            info.AddValue("LambdaLT0ForLateralTorsionalBuckling", _lambdaLT0ForLateralTorsionalBuckling);
            info.AddValue("BetaForLateralTorsionalBucklingMod", _betaForLateralTorsionalBucklingMod);
            info.AddValue("LambdaLT0ForLateralTorsionalBucklingMod", _lambdaLT0ForLateralTorsionalBucklingMod);
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

            return obj is StandardEN1993p11 p &&
                   _gammaM0 == p._gammaM0 &&
                   _gammaM1 == p._gammaM1 &&
                   _gammaM2 == p._gammaM2 &&
                   _gammaM3 == p._gammaM3 &&
                   _gammaM3Ser == p._gammaM3Ser &&
                   _gammaM4 == p._gammaM4 &&
                   _gammaM5 == p._gammaM5 &&
                   _gammaM6Ser == p._gammaM6Ser &&
                   _gammaM7 == p._gammaM7 &&
                   _nShearBucklingLowGradeOfSteel == p._nShearBucklingLowGradeOfSteel &&
                   _nShearBucklingHighGradeOfSteel == p._nShearBucklingHighGradeOfSteel &&
                   _alphaImperfectionFactorForCurveA0 == p._alphaImperfectionFactorForCurveA0 &&
                   _alphaImperfectionFactorForCurveA == p._alphaImperfectionFactorForCurveA &&
                   _alphaImperfectionFactorForCurveB == p._alphaImperfectionFactorForCurveB &&
                   _alphaImperfectionFactorForCurveC == p._alphaImperfectionFactorForCurveC &&
                   _alphaImperfectionFactorForCurveD == p._alphaImperfectionFactorForCurveD &&
                   _alphaLTImperfectionFactorForCurveA == p._alphaLTImperfectionFactorForCurveA &&
                   _alphaLTImperfectionFactorForCurveB == p._alphaLTImperfectionFactorForCurveB &&
                   _alphaLTImperfectionFactorForCurveC == p._alphaLTImperfectionFactorForCurveC &&
                   _alphaLTImperfectionFactorForCurveD == p._alphaLTImperfectionFactorForCurveD &&
                   _betaForLateralTorsionalBuckling == p._betaForLateralTorsionalBuckling &&
                   _lambdaLT0ForLateralTorsionalBuckling == p._lambdaLT0ForLateralTorsionalBuckling &&
                   _betaForLateralTorsionalBucklingMod == p._betaForLateralTorsionalBucklingMod &&
                   _lambdaLT0ForLateralTorsionalBucklingMod == p._lambdaLT0ForLateralTorsionalBucklingMod;
        }

        /// <summary>
        /// The hash code of the coefficients and of the base
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _gammaM0.GetHashCode();
                hashCode = hashCode * -17 + _gammaM1.GetHashCode();
                hashCode = hashCode * -17 + _gammaM2.GetHashCode();
                hashCode = hashCode * -17 + _gammaM3.GetHashCode();
                hashCode = hashCode * -17 + _gammaM3Ser.GetHashCode();
                hashCode = hashCode * -17 + _gammaM4.GetHashCode();
                hashCode = hashCode * -17 + _gammaM5.GetHashCode();
                hashCode = hashCode * -17 + _gammaM6Ser.GetHashCode();
                hashCode = hashCode * -17 + _gammaM7.GetHashCode();
                hashCode = hashCode * -17 + _nShearBucklingLowGradeOfSteel.GetHashCode();
                hashCode = hashCode * -17 + _nShearBucklingHighGradeOfSteel.GetHashCode();
                hashCode = hashCode * -17 + _alphaImperfectionFactorForCurveA0.GetHashCode();
                hashCode = hashCode * -17 + _alphaImperfectionFactorForCurveA.GetHashCode();
                hashCode = hashCode * -17 + _alphaImperfectionFactorForCurveB.GetHashCode();
                hashCode = hashCode * -17 + _alphaImperfectionFactorForCurveC.GetHashCode();
                hashCode = hashCode * -17 + _alphaImperfectionFactorForCurveD.GetHashCode();
                hashCode = hashCode * -17 + _alphaLTImperfectionFactorForCurveA.GetHashCode();
                hashCode = hashCode * -17 + _alphaLTImperfectionFactorForCurveB.GetHashCode();
                hashCode = hashCode * -17 + _alphaLTImperfectionFactorForCurveC.GetHashCode();
                hashCode = hashCode * -17 + _alphaLTImperfectionFactorForCurveD.GetHashCode();
                hashCode = hashCode * -17 + _betaForLateralTorsionalBuckling.GetHashCode();
                hashCode = hashCode * -17 + _lambdaLT0ForLateralTorsionalBuckling.GetHashCode();
                hashCode = hashCode * -17 + _betaForLateralTorsionalBucklingMod.GetHashCode();
                hashCode = hashCode * -17 + _lambdaLT0ForLateralTorsionalBucklingMod.GetHashCode();
                return hashCode;
            }
        }

        #endregion
    }
}
