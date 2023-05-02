using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the NTC2018 for steel design.
    /// </summary>
    /// <remarks>Reference: NTC2018. 17 January 2018</remarks>
    [Serializable]
    public class StandardNTC2018Steel : StandardEN1993p11, ISerializable
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public StandardNTC2018Steel(string name = "NTC 2018", string remarks = "Norme tecniche per le costruzioni")
            : base(name, remarks)
        {
            _gammaM0 = 1.05;
            _gammaM1 = 1.05;
            _gammaM2 = 1.25;
            _gammaM3 = 1.25;
            _gammaM3Ser = 1.1;
            _gammaM6Ser = 1.0;
            _gammaM7 = 1.1;

            _alphaImperfectionFactorForCurveA0 = 0.13;
            _alphaImperfectionFactorForCurveA = 0.21;
            _alphaImperfectionFactorForCurveB = 0.34;
            _alphaImperfectionFactorForCurveC = 0.49;
            _alphaImperfectionFactorForCurveD = 0.76;

            _alphaLTImperfectionFactorForCurveA = 0.21;
            _alphaLTImperfectionFactorForCurveB = 0.34;
            _alphaLTImperfectionFactorForCurveC = 0.49;
            _alphaLTImperfectionFactorForCurveD = 0.76;

            _betaForLateralTorsionalBuckling = 1.0; // § 4.2.4.1.3.2 Travi inflesse.
            _lambdaLT0ForLateralTorsionalBuckling = 0.2;
            _betaForLateralTorsionalBucklingMod = 0.75;
            _lambdaLT0ForLateralTorsionalBucklingMod = 0.40;
        }

        protected StandardNTC2018Steel(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
