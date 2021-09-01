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

        protected double _gammaM0;
        protected double _gammaM1;
        protected double _gammaM2;

        protected double _nShearBucklingLowGradeOfSteel;
        protected double _nShearBucklingHighGradeOfSteel;

        protected double _alphaImperfectionFactorForCurveA0;
        protected double _alphaImperfectionFactorForCurveA;
        protected double _alphaImperfectionFactorForCurveB;
        protected double _alphaImperfectionFactorForCurveC;
        protected double _alphaImperfectionFactorForCurveD;

        protected double _alphaLTImperfectionFactorForCurveA;
        protected double _alphaLTImperfectionFactorForCurveB;
        protected double _alphaLTImperfectionFactorForCurveC;
        protected double _alphaLTImperfectionFactorForCurveD;

        protected double _betaForLateralTorsionalBuckling;
        protected double _lambdaLT0ForLateralTorsionalBuckling;
        protected double _betaForLateralTorsionalBucklingMod;
        protected double _lambdaLT0ForLateralTorsionalBucklingMod;


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

        public double BetaForLateralTorsionalBucklingMod => _betaForLateralTorsionalBucklingMod;

        public double LambdaLT0ForLateralTorsionalBucklingMod => _lambdaLT0ForLateralTorsionalBucklingMod;

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
            _betaForLateralTorsionalBucklingMod = 0.75;
            _lambdaLT0ForLateralTorsionalBucklingMod = 0.40;
        }

    }
}
