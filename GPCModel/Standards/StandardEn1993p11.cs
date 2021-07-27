using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
    public class StandardEN1993p11 : Standard
    {
        /// <summary>
        /// The limit states. Reference: EN 1990:2002/A1:2005 
        /// </summary>
        public enum LimitStates
        {
            UltimateLimitState,
            ServiceabilityLimitState,
        }

        #region VARIABLES

        private readonly double _gammaM0;
        private readonly double _gammaM1;
        private readonly double _gammaM2;

        private readonly double _nShearBucklingLowGradeOfSteel;
        private readonly double _nShearBucklingHighGradeOfSteel;

        private readonly double _alphaImperfectionFactorForCurveA0;
        private readonly double _alphaImperfectionFactorForCurveA;
        private readonly double _alphaImperfectionFactorForCurveB;
        private readonly double _alphaImperfectionFactorForCurveC;
        private readonly double _alphaImperfectionFactorForCurveD;

        private readonly double _alphaLTImperfectionFactorForCurveA;
        private readonly double _alphaLTImperfectionFactorForCurveB;
        private readonly double _alphaLTImperfectionFactorForCurveC;
        private readonly double _alphaLTImperfectionFactorForCurveD;

        private readonly double _betaForLateralTorsionalBuckling;
        private readonly double _lambdaLT0ForLateralTorsionalBuckling;




        public double GammaM0 => _gammaM0;

        public double GammaM1 => _gammaM1;

        public double GammaM2 => _gammaM2;

        public double NShearBucklingLowGradeOfSteel => _nShearBucklingLowGradeOfSteel;

        public double NShearBucklingHighGradeOfSteel => _nShearBucklingHighGradeOfSteel;

        public double AlphaImperfectionFactorForCurveA0 => _alphaImperfectionFactorForCurveA0;
            
        public double AlphaImperfectionFactorForCurveA => _alphaImperfectionFactorForCurveA;

        public double AlphaImperfectionFactorForCurveB => _alphaImperfectionFactorForCurveB;

        public double AlphaImperfectionFactorForCurveC => _alphaImperfectionFactorForCurveC;

        public double AlphaImperfectionFactorForCurveD => _alphaImperfectionFactorForCurveD;

        public double AlphaLTImperfectionFactorForCurveA => _alphaLTImperfectionFactorForCurveA;

        public double AlphaLTImperfectionFactorForCurveB => _alphaLTImperfectionFactorForCurveB;

        public double AlphaLTImperfectionFactorForCurveC => _alphaLTImperfectionFactorForCurveC;

        public double AlphaLTImperfectionFactorForCurveD => _alphaLTImperfectionFactorForCurveD;

        public double BetaForLateralTorsionalBuckling => _betaForLateralTorsionalBuckling;

        public double LambdaLT0ForLateralTorsionalBuckling => _lambdaLT0ForLateralTorsionalBuckling;

        #endregion

        public StandardEN1993p11()
        {
            _gammaM0 = 1.00;
            _gammaM1 = 1.00;
            _gammaM2 = 1.25;

            _nShearBucklingLowGradeOfSteel = 1.20;
            _nShearBucklingHighGradeOfSteel = 1.00;

            _alphaImperfectionFactorForCurveA0 = 0.13;
            _alphaImperfectionFactorForCurveA = 0.21;
            _alphaImperfectionFactorForCurveB = 0.34;
            _alphaImperfectionFactorForCurveC = 0.49;
            _alphaImperfectionFactorForCurveD = 0.76;

            _alphaLTImperfectionFactorForCurveA = 0.21;
            _alphaLTImperfectionFactorForCurveB = 0.34;
            _alphaLTImperfectionFactorForCurveC = 0.49;
            _alphaLTImperfectionFactorForCurveD = 0.76;

            _betaForLateralTorsionalBuckling = 1.0;
            _lambdaLT0ForLateralTorsionalBuckling = 0.2;
        }




    }
}
