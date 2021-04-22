using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;

namespace GPC.Model.Combinations
{
    /// <summary>
    /// This class collects all the coefficient of the Eurocode Standard
    /// </summary>
    /// <remarks>Reference: EN 1990:2002/A1:2005</remarks>
    public class StandardEN1990 : Standard
    {
        #region PUBLIC ENUMS

        /// <summary>
        /// The sets for structural and geotechical ultimate limit states. Reference: EN 1990:2002/A1:2005 Annex A1
        /// </summary>
        public enum ULSStructuralGeotechicalCombinationSets
        {
            [Description("ENSetB")] SetB,
            [Description("ENSetC")] SetC,
        }

        /// <summary>
        /// The limit states. Reference: EN 1990:2002/A1:2005 
        /// </summary>
        public enum LimitState
        {
            UltimateEquilibrium,
            UltimateStructural,
            UltimateGeotechnical,
            UltimateFatigue,
            UltimateSeismic,
            UltimateAccidental,
            ServiceabilityCharacteristic,
            ServiceabilityFrequent,
            ServiceabilityQuasiPermanent
        }

        /// <summary>
        /// The category of buildings for imposed loads. Reference: EN 1990:2002/A1:2005 Annex A1. EN 1991-1-1:2002
        /// </summary>
        public enum ImposedLoadCategory
        {
            [Description("Category A")] CategoryA,
            [Description("Category B")] CategoryB,
            [Description("Category C")] CategoryC,
            [Description("Category D")] CategoryD,
            [Description("Category E")] CategoryE,
            [Description("Category F")] CategoryF,
            [Description("Category G")] CategoryG,
            [Description("Category H")] CategoryH,
        }

        #endregion


        #region VARIABLES

        // Gamma G
        private double _gammaGFavourableSetA;                       
        private double _gammaGUnfavourableSetA;
        private double _gammaGFavourableSetB;
        private double _gammaGUnfavourableSetB;
        private double _gammaGFavourableSetC;
        private double _gammaGUnfavourableSetC;

        // Gamma Q
        private double _gammaQFavourableSetA;                       
        private double _gammaQUnfavourableSetA;
        private double _gammaQFavourableSetB;
        private double _gammaQUnfavourableSetB;
        private double _gammaQFavourableSetC;
        private double _gammaQUnfavourableSetC;

        // Gamma P
        private double _gammaPFavourableSetA;                       
        private double _gammaPUnfavourableSetA;
        private double _gammaPFavourableSetB;
        private double _gammaPUnfavourableSetB;
        private double _gammaPFavourableSetC;
        private double _gammaPUnfavourableSetC;

        // Imposed Load Psi
        private double _psi0ImposedLoadCategoryA;
        private double _psi0ImposedLoadCategoryB;
        private double _psi0ImposedLoadCategoryC;
        private double _psi0ImposedLoadCategoryD;
        private double _psi0ImposedLoadCategoryE;
        private double _psi0ImposedLoadCategoryF;
        private double _psi0ImposedLoadCategoryG;
        private double _psi0ImposedLoadCategoryH;

        private double _psi1ImposedLoadCategoryA;
        private double _psi1ImposedLoadCategoryB;
        private double _psi1ImposedLoadCategoryC;
        private double _psi1ImposedLoadCategoryD;
        private double _psi1ImposedLoadCategoryE;
        private double _psi1ImposedLoadCategoryF;
        private double _psi1ImposedLoadCategoryG;
        private double _psi1ImposedLoadCategoryH;
                        
        private double _psi2ImposedLoadCategoryA;
        private double _psi2ImposedLoadCategoryB;
        private double _psi2ImposedLoadCategoryC;
        private double _psi2ImposedLoadCategoryD;
        private double _psi2ImposedLoadCategoryE;
        private double _psi2ImposedLoadCategoryF;
        private double _psi2ImposedLoadCategoryG;
        private double _psi2ImposedLoadCategoryH;

        // Snow Psi
        private double _psi0SnowHighAltitude;
        private double _psi0SnowLowAltitude;
        private double _psi1SnowHighAltitude;
        private double _psi1SnowLowAltitude;
        private double _psi2SnowHighAltitude;
        private double _psi2SnowLowAltitude;

        // Wind Psi
        private double _psi0Wind;
        private double _psi1Wind;
        private double _psi2Wind;

        // Temperature psi
        private double _psi0Temperature;
        private double _psi1Temperature;
        private double _psi2Temperature;

        // Climate psi
        private double _psi0ClimateSummerDeltaP;
        private double _psi0ClimateSummerDeltaT;
        private double _psi0ClimateWinterDeltaP;
        private double _psi0ClimateWinterDeltaT;
        private double _psi1ClimateSummerDeltaP;
        private double _psi1ClimateSummerDeltaT;
        private double _psi1ClimateWinterDeltaP;
        private double _psi1ClimateWinterDeltaT;
        private double _psi2ClimateSummerDeltaP;
        private double _psi2ClimateSummerDeltaT;
        private double _psi2ClimateWinterDeltaP;
        private double _psi2ClimateWinterDeltaT;

        // Gamma G
        public double GammaGFavourableSetA => _gammaGFavourableSetA;
        public double GammaGUnfavourableSetA => _gammaGUnfavourableSetA;
        public double GammaGFavourableSetB => _gammaGFavourableSetB;
        public double GammaGUnfavourableSetB => _gammaGUnfavourableSetB;
        public double GammaGFavourableSetC => _gammaGFavourableSetC;
        public double GammaGUnfavourableSetC => _gammaGUnfavourableSetC;

        // Gamma Q
        public double GammaQFavourableSetA => _gammaQFavourableSetA;
        public double GammaQUnfavourableSetA => _gammaQUnfavourableSetA;
        public double GammaQFavourableSetB => _gammaQFavourableSetB;
        public double GammaQUnfavourableSetB => _gammaQUnfavourableSetB;
        public double GammaQFavourableSetC => _gammaQFavourableSetC;
        public double GammaQUnfavourableSetC => _gammaQUnfavourableSetC;

        // Gamma P
        public double GammaPFavourableSetA => _gammaPFavourableSetA;
        public double GammaPUnfavourableSetA => _gammaPUnfavourableSetA;
        public double GammaPFavourableSetB => _gammaPFavourableSetB;
        public double GammaPUnfavourableSetB => _gammaPUnfavourableSetB;
        public double GammaPFavourableSetC => _gammaPFavourableSetC;
        public double GammaPUnfavourableSetC => _gammaPUnfavourableSetC;

        // Imposed Load Psi
        public double ImposedLoadPsi0CategoryA => _psi0ImposedLoadCategoryA;
        public double ImposedLoadPsi0CategoryB => _psi0ImposedLoadCategoryB;
        public double ImposedLoadPsi0CategoryC => _psi0ImposedLoadCategoryC;
        public double ImposedLoadPsi0CategoryD => _psi0ImposedLoadCategoryD;
        public double ImposedLoadPsi0CategoryE => _psi0ImposedLoadCategoryE;
        public double ImposedLoadPsi0CategoryF => _psi0ImposedLoadCategoryF;
        public double ImposedLoadPsi0CategoryG => _psi0ImposedLoadCategoryG;
        public double ImposedLoadPsi0CategoryH => _psi0ImposedLoadCategoryH;

        public double ImposedLoadPsi1CategoryA => _psi1ImposedLoadCategoryA;
        public double ImposedLoadPsi1CategoryB => _psi1ImposedLoadCategoryB;
        public double ImposedLoadPsi1CategoryC => _psi1ImposedLoadCategoryC;
        public double ImposedLoadPsi1CategoryD => _psi1ImposedLoadCategoryD;
        public double ImposedLoadPsi1CategoryE => _psi1ImposedLoadCategoryE;
        public double ImposedLoadPsi1CategoryF => _psi1ImposedLoadCategoryF;
        public double ImposedLoadPsi1CategoryG => _psi1ImposedLoadCategoryG;
        public double ImposedLoadPsi1CategoryH => _psi1ImposedLoadCategoryH;

        public double ImposedLoadPsi2CategoryA => _psi2ImposedLoadCategoryA;
        public double ImposedLoadPsi2CategoryB => _psi2ImposedLoadCategoryB;
        public double ImposedLoadPsi2CategoryC => _psi2ImposedLoadCategoryC;
        public double ImposedLoadPsi2CategoryD => _psi2ImposedLoadCategoryD;
        public double ImposedLoadPsi2CategoryE => _psi2ImposedLoadCategoryE;
        public double ImposedLoadPsi2CategoryF => _psi2ImposedLoadCategoryF;
        public double ImposedLoadPsi2CategoryG => _psi2ImposedLoadCategoryG;
        public double ImposedLoadPsi2CategoryH => _psi2ImposedLoadCategoryH;
                      
        // Snow Psi
        public double Psi0SnowHighAltitude => _psi0SnowHighAltitude;
        public double Psi0SnowLowAltitude => _psi0SnowLowAltitude;
        public double Psi1SnowHighAltitude => _psi1SnowHighAltitude;
        public double Psi1SnowLowAltitude => _psi1SnowLowAltitude;
        public double Psi2SnowHighAltitude => _psi2SnowHighAltitude;
        public double Psi2SnowLowAltitude => _psi2SnowLowAltitude;
        // public bool IsHighAltidute => _isHighAltitude;

        // Wind Psi
        public double Psi0Wind => _psi0Wind;
        public double Psi1Wind => _psi1Wind;
        public double Psi2Wind => _psi2Wind;

        // Temperature Psi
        public double Psi0Temperature => _psi0Temperature;
        public double Psi1Temperature => _psi1Temperature;
        public double Psi2Temperature => _psi2Temperature;

        // Climate Psi
        public double Psi0ClimateSummerDeltaP => _psi0ClimateSummerDeltaP;
        public double Psi0ClimateSummerDeltaT => _psi0ClimateSummerDeltaT;
        public double Psi0ClimateWinterDeltaP => _psi0ClimateWinterDeltaP;
        public double Psi0ClimateWinterDeltaT => _psi0ClimateWinterDeltaT;
        public double Psi1ClimateSummerDeltaP => _psi1ClimateSummerDeltaP;
        public double Psi1ClimateSummerDeltaT => _psi1ClimateSummerDeltaT;
        public double Psi1ClimateWinterDeltaP => _psi1ClimateWinterDeltaP;
        public double Psi1ClimateWinterDeltaT => _psi1ClimateWinterDeltaT;
        public double Psi2ClimateSummerDeltaP => _psi2ClimateSummerDeltaP;
        public double Psi2ClimateSummerDeltaT => _psi2ClimateSummerDeltaT;
        public double Psi2ClimateWinterDeltaP => _psi2ClimateWinterDeltaP;
        public double Psi2ClimateWinterDeltaT => _psi2ClimateWinterDeltaT;

        #endregion


        #region PUBLIC CONSTRUCTOR

        public StandardEN1990()
        {
            _gammaGUnfavourableSetA = 1.10;
            _gammaGFavourableSetA = 0.90;
            _gammaGUnfavourableSetB = 1.35;
            _gammaGFavourableSetB = 1.00;
            _gammaGUnfavourableSetC = 1.00;
            _gammaGFavourableSetC = 1.00;

            _gammaQUnfavourableSetA = 1.50;
            _gammaQFavourableSetA = 0.00;
            _gammaQUnfavourableSetB = 1.50;
            _gammaQFavourableSetB = 0.00;
            _gammaQUnfavourableSetC = 1.30;
            _gammaQFavourableSetC = 0.00;

            _gammaPFavourableSetA = 1.00;
            _gammaPUnfavourableSetA = 1.00;
            _gammaPFavourableSetB = 1.00;
            _gammaPUnfavourableSetB = 1.00;
            _gammaPFavourableSetC = 1.00;
            _gammaPUnfavourableSetC = 1.00;

            _psi0ImposedLoadCategoryA = 0.70;
            _psi0ImposedLoadCategoryB = 0.70;
            _psi0ImposedLoadCategoryC = 0.70;
            _psi0ImposedLoadCategoryD = 0.70;
            _psi0ImposedLoadCategoryE = 1.00;
            _psi0ImposedLoadCategoryF = 0.70;
            _psi0ImposedLoadCategoryG = 0.70;
            _psi0ImposedLoadCategoryH = 0.00;

            _psi1ImposedLoadCategoryA = 0.50;
            _psi1ImposedLoadCategoryB = 0.50;
            _psi1ImposedLoadCategoryC = 0.70;
            _psi1ImposedLoadCategoryD = 0.70;
            _psi1ImposedLoadCategoryE = 0.90;
            _psi1ImposedLoadCategoryF = 0.70;
            _psi1ImposedLoadCategoryG = 0.50;
            _psi1ImposedLoadCategoryH = 0.00;

            _psi2ImposedLoadCategoryA = 0.30;
            _psi2ImposedLoadCategoryB = 0.30;
            _psi2ImposedLoadCategoryC = 0.60;
            _psi2ImposedLoadCategoryD = 0.60;
            _psi2ImposedLoadCategoryE = 0.80;
            _psi2ImposedLoadCategoryF = 0.60;
            _psi2ImposedLoadCategoryG = 0.30;
            _psi2ImposedLoadCategoryH = 0.00;

            _psi0SnowHighAltitude = 0.70;
            _psi0SnowLowAltitude = 0.50;
            _psi1SnowHighAltitude = 0.50;
            _psi1SnowLowAltitude = 0.20;
            _psi2SnowHighAltitude = 0.20;
            _psi2SnowLowAltitude = 0.00;

            _psi0Wind = 0.60;
            _psi1Wind = 0.20;
            _psi2Wind = 0.00;

            _psi0Temperature = 0.60;
            _psi1Temperature = 0.50;
            _psi2Temperature = 0.0;

            // da controllare. non sono corretti
            _psi0ClimateSummerDeltaP = 0.30;
            _psi0ClimateSummerDeltaT = 0.30;
            _psi0ClimateWinterDeltaP = 0.30;
            _psi0ClimateWinterDeltaT = 0.30;
            _psi1ClimateSummerDeltaP = 0.30;
            _psi1ClimateSummerDeltaT = 0.30;
            _psi1ClimateWinterDeltaP = 0.30;
            _psi1ClimateWinterDeltaT = 0.30;
            _psi2ClimateSummerDeltaP = 0.00;
            _psi2ClimateSummerDeltaT = 0.00;
            _psi2ClimateWinterDeltaP = 0.00;
            _psi2ClimateWinterDeltaT = 0.00;
        }

        #endregion


        #region PUBLIC METHOD

        /// <summary>
        /// Get the coefficient gamma G unfavourable 
        /// </summary>
        /// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <returns>The value of the coefficient</returns>
        public double GetGammaGUnfavourable(ULSStructuralGeotechicalCombinationSets set, LimitState limitState)
        {
            if (limitState == LimitState.UltimateEquilibrium)
            {
                return _gammaGUnfavourableSetA;
            }
            else if (limitState == LimitState.UltimateFatigue || limitState == LimitState.UltimateGeotechnical || limitState == LimitState.UltimateStructural)
            {
                if (set == ULSStructuralGeotechicalCombinationSets.SetB)
                    return _gammaGUnfavourableSetB;
                else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
                    return _gammaGUnfavourableSetC;
                else
                    throw new NotImplementedException("Failed to set coefficient gamma unfavourable");
            }
            else if (limitState == LimitState.UltimateSeismic || limitState == LimitState.UltimateAccidental)
            {
                return 1.0;
            }
            else if (limitState == LimitState.ServiceabilityQuasiPermanent || limitState == LimitState.ServiceabilityCharacteristic || limitState == LimitState.ServiceabilityFrequent)
            {
                return 1.0;
            }
            else 
                throw new ArgumentException("Failed to set coefficient gammaG unfavourable");

        }

        /// <summary>
        /// Get the coefficient gamma G favourable 
        /// </summary>
        /// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <returns>The value of the coefficient</returns>
        public double GetGammaGFavourable(ULSStructuralGeotechicalCombinationSets set, LimitState limitState)
        {
            if (limitState == LimitState.UltimateEquilibrium)
            {
                return _gammaGFavourableSetA;
            }
            else if (limitState == LimitState.UltimateFatigue || limitState == LimitState.UltimateGeotechnical || limitState == LimitState.UltimateStructural)
            {
                if (set == ULSStructuralGeotechicalCombinationSets.SetB)
                    return _gammaGFavourableSetB;
                else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
                    return _gammaGFavourableSetC;
                else
                    throw new NotImplementedException("Failed to set coefficient gamma G favourable");
            }
            else if(limitState == LimitState.UltimateSeismic || limitState == LimitState.UltimateAccidental)
            {
                return 1.0;
            }
            else if (limitState == LimitState.ServiceabilityQuasiPermanent || limitState == LimitState.ServiceabilityCharacteristic || limitState == LimitState.ServiceabilityFrequent)
            {
                return 1.0;
            }
            else
                throw new ArgumentException("Failed to set coefficient gamma G favourable");
        }

        /// <summary>
        /// Get the coefficient gamma P favourable 
        /// </summary>
        /// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="loadCase">The load case</param>
        /// <returns>The value of the coefficient</returns>
        public double GetGammaPFavourable(ULSStructuralGeotechicalCombinationSets set, LimitState limitState, LoadCase loadCase)
        {
            if (limitState == LimitState.UltimateEquilibrium)
            {
                return _gammaPFavourableSetA;            
            }
            else if (limitState == LimitState.UltimateGeotechnical || limitState == LimitState.UltimateFatigue || limitState == LimitState.UltimateStructural)
            {
                switch (set)
                {
                    case ULSStructuralGeotechicalCombinationSets.SetB:
                        return _gammaPFavourableSetB;
                    case ULSStructuralGeotechicalCombinationSets.SetC:
                        return _gammaPFavourableSetC;
                    default:
                        throw new NotImplementedException("Failed to set coefficient gamma P favourable");
                }
            }
            else if (limitState == LimitState.UltimateSeismic || limitState == LimitState.UltimateAccidental)
            {
                return 1.0;
            }
            else if (limitState == LimitState.ServiceabilityQuasiPermanent || limitState == LimitState.ServiceabilityCharacteristic || limitState == LimitState.ServiceabilityFrequent)
            {
                return 1.00;                
            }
            else
                throw new ArgumentException("Failed to set coefficient gamma P favourable");
        }

        /// <summary>
        /// Get the coefficient gamma P unfavourable 
        /// </summary>
        /// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="loadCase">The load case</param>
        /// <returns>The value of the coefficient</returns>
        public double GetGammaPUnfavourable(ULSStructuralGeotechicalCombinationSets set, LimitState limitState, LoadCase loadCase)
        {
            if (limitState == LimitState.UltimateEquilibrium)
            {
                return _gammaPUnfavourableSetA;               
            }
            else if (limitState == LimitState.UltimateGeotechnical || limitState == LimitState.UltimateFatigue || limitState == LimitState.UltimateStructural)
            {
                switch (set)
                {
                    case ULSStructuralGeotechicalCombinationSets.SetB:
                        return _gammaPUnfavourableSetB;
                    case ULSStructuralGeotechicalCombinationSets.SetC:
                        return _gammaPUnfavourableSetC;
                    default:
                        throw new NotImplementedException("Failed to set coefficient gamma P unfavourable");
                }
            }
            else if (limitState == LimitState.UltimateSeismic || limitState == LimitState.UltimateAccidental)
            {
                return 1.0;
            }
            else if (limitState == LimitState.ServiceabilityQuasiPermanent || limitState == LimitState.ServiceabilityCharacteristic || limitState == LimitState.ServiceabilityFrequent)
            {
                return 1.00;
            }
            else
                throw new ArgumentException("Failed to set coefficient gamma P favourable");
        }

        /// <summary>
        /// Get the coefficient gamma Q unfavourable 
        /// </summary>
        /// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="loadCase">The load case</param>
        /// <returns>The value of the coefficient</returns>
        public double GetGammaQUnfavourable(ULSStructuralGeotechicalCombinationSets set, LimitState limitState, LoadCase loadCase)
        {
            var loadCaseType = loadCase.LoadCaseType;

            if (limitState == LimitState.UltimateEquilibrium)
            {
                switch (loadCaseType)
                {
                    case LoadCase.LoadCaseTypes.LiveLoad:
                    case LoadCase.LoadCaseTypes.WindPressure:
                    case LoadCase.LoadCaseTypes.WindSuction:
                    case LoadCase.LoadCaseTypes.Snow:
                    case LoadCase.LoadCaseTypes.Maintenance:
                    case LoadCase.LoadCaseTypes.Earthquake:
                    case LoadCase.LoadCaseTypes.Temperature:
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaP:
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaT:
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaP:
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaT:
                        return _gammaQUnfavourableSetA;
                    default:
                        throw new NotImplementedException("Not implemented coefficient for load case type");
                }
            }
            else if (limitState == LimitState.UltimateGeotechnical || limitState == LimitState.UltimateFatigue || limitState == LimitState.UltimateStructural)
            {
                if (set == ULSStructuralGeotechicalCombinationSets.SetB)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseTypes.LiveLoad:
                        case LoadCase.LoadCaseTypes.WindPressure:
                        case LoadCase.LoadCaseTypes.WindSuction:
                        case LoadCase.LoadCaseTypes.Snow:
                        case LoadCase.LoadCaseTypes.Maintenance:
                        case LoadCase.LoadCaseTypes.Earthquake:
                        case LoadCase.LoadCaseTypes.Temperature:
                        case LoadCase.LoadCaseTypes.ClimateSummerDeltaP:
                        case LoadCase.LoadCaseTypes.ClimateSummerDeltaT:
                        case LoadCase.LoadCaseTypes.ClimateWinterDeltaP:
                        case LoadCase.LoadCaseTypes.ClimateWinterDeltaT:
                            return _gammaQUnfavourableSetB;
                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseTypes.LiveLoad:
                        case LoadCase.LoadCaseTypes.WindPressure:
                        case LoadCase.LoadCaseTypes.WindSuction:
                        case LoadCase.LoadCaseTypes.Snow:
                        case LoadCase.LoadCaseTypes.Maintenance:
                        case LoadCase.LoadCaseTypes.Earthquake:
                        case LoadCase.LoadCaseTypes.Temperature:
                        case LoadCase.LoadCaseTypes.ClimateSummerDeltaP:
                        case LoadCase.LoadCaseTypes.ClimateSummerDeltaT:
                        case LoadCase.LoadCaseTypes.ClimateWinterDeltaP:
                        case LoadCase.LoadCaseTypes.ClimateWinterDeltaT:
                            return _gammaQUnfavourableSetC;
                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new NotImplementedException("Not implemented Annex");
            }
            else if (limitState == LimitState.UltimateSeismic || limitState == LimitState.UltimateAccidental)
            {
                return 1.0;
            }
            else if (limitState == LimitState.ServiceabilityCharacteristic || limitState == LimitState.ServiceabilityFrequent || limitState == LimitState.ServiceabilityQuasiPermanent)
            {
                switch (loadCaseType)
                {
                    case LoadCase.LoadCaseTypes.LiveLoad:
                    case LoadCase.LoadCaseTypes.WindPressure:
                    case LoadCase.LoadCaseTypes.WindSuction:
                    case LoadCase.LoadCaseTypes.Snow:
                    case LoadCase.LoadCaseTypes.Maintenance:
                    case LoadCase.LoadCaseTypes.Earthquake:
                    case LoadCase.LoadCaseTypes.Temperature:
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaP:
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaT:
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaP:
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaT:
                        return 1.0;

                    default:
                        throw new NotImplementedException("Not implemented coefficient for load case type");
                }
            }
            else
                throw new ArgumentException("Failed to set coefficient gamma favourable");
        }

        /// <summary>
        /// Get the coefficient gamma Q favourable 
        /// </summary>
        /// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="loadCase">The load case</param>
        /// <returns>The value of the coefficient</returns>
        public double GetGammaQFavourable(ULSStructuralGeotechicalCombinationSets set, LimitState limitState, LoadCase loadCase)
        {
            var loadCaseType = loadCase.LoadCaseType;

            if (limitState == LimitState.UltimateEquilibrium)
            {
                switch (loadCaseType)
                {
                    case LoadCase.LoadCaseTypes.LiveLoad:
                    case LoadCase.LoadCaseTypes.WindPressure:
                    case LoadCase.LoadCaseTypes.WindSuction:
                    case LoadCase.LoadCaseTypes.Snow:
                    case LoadCase.LoadCaseTypes.Maintenance:
                    case LoadCase.LoadCaseTypes.Earthquake:
                    case LoadCase.LoadCaseTypes.Temperature:
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaP:
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaT:
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaP:
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaT:
                        return _gammaQFavourableSetA;
                    default:
                        throw new NotImplementedException("Not implemented coefficient for load case type");
                }
            }
            else if (limitState == LimitState.UltimateGeotechnical || limitState == LimitState.UltimateFatigue || limitState == LimitState.UltimateStructural)
            {
                if (set == ULSStructuralGeotechicalCombinationSets.SetB)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseTypes.LiveLoad:
                        case LoadCase.LoadCaseTypes.WindPressure:
                        case LoadCase.LoadCaseTypes.WindSuction:
                        case LoadCase.LoadCaseTypes.Snow:
                        case LoadCase.LoadCaseTypes.Maintenance:
                        case LoadCase.LoadCaseTypes.Earthquake:
                        case LoadCase.LoadCaseTypes.Temperature:
                        case LoadCase.LoadCaseTypes.ClimateSummerDeltaP:
                        case LoadCase.LoadCaseTypes.ClimateSummerDeltaT:
                        case LoadCase.LoadCaseTypes.ClimateWinterDeltaP:
                        case LoadCase.LoadCaseTypes.ClimateWinterDeltaT:
                            return _gammaQFavourableSetB;
                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseTypes.LiveLoad:
                        case LoadCase.LoadCaseTypes.WindPressure:
                        case LoadCase.LoadCaseTypes.WindSuction:
                        case LoadCase.LoadCaseTypes.Snow:
                        case LoadCase.LoadCaseTypes.Maintenance:
                        case LoadCase.LoadCaseTypes.Earthquake:
                        case LoadCase.LoadCaseTypes.Temperature:
                        case LoadCase.LoadCaseTypes.ClimateSummerDeltaP:
                        case LoadCase.LoadCaseTypes.ClimateSummerDeltaT:
                        case LoadCase.LoadCaseTypes.ClimateWinterDeltaP:
                        case LoadCase.LoadCaseTypes.ClimateWinterDeltaT:
                            return _gammaQFavourableSetC;
                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new NotImplementedException("Not implemented Annex");
            }
            else if (limitState == LimitState.UltimateSeismic || limitState == LimitState.UltimateAccidental)
            {
                return 1.0;
            }
            else if (limitState == LimitState.ServiceabilityQuasiPermanent || limitState == LimitState.ServiceabilityCharacteristic || limitState == LimitState.ServiceabilityFrequent)
            {
                return 1.00;
            }
            else
                throw new ArgumentException("Not implemented coefficient for load case type");
        }

        /// <summary>
        /// Get the coefficient psi 0 for buildings
        /// </summary>
        /// <param name="category">The category of the imposed load</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>
        /// <returns>The value of the coefficient</returns>
        public double GetPsi0(ImposedLoadCategory category, LoadCase loadCase, bool highAltitude = true)
        {
            var loadCaseType = loadCase.LoadCaseType;

            if (loadCaseType == LoadCase.LoadCaseTypes.Snow)
            {
                if (highAltitude)
                    return Psi0SnowHighAltitude;
                else if (!highAltitude)
                    return Psi0SnowLowAltitude;
                else
                    throw new NotImplementedException("Failed to set coefficient psi0 for snow load");
            }
            else if (loadCaseType == LoadCase.LoadCaseTypes.LiveLoad || loadCaseType == LoadCase.LoadCaseTypes.Maintenance)
            {
                switch (category)
                {
                    case ImposedLoadCategory.CategoryA:
                        return _psi0ImposedLoadCategoryA;
                    case ImposedLoadCategory.CategoryB:
                        return _psi0ImposedLoadCategoryB;
                    case ImposedLoadCategory.CategoryC:
                        return _psi0ImposedLoadCategoryC;
                    case ImposedLoadCategory.CategoryD:
                        return _psi0ImposedLoadCategoryD;
                    case ImposedLoadCategory.CategoryE:
                        return _psi0ImposedLoadCategoryE;
                    case ImposedLoadCategory.CategoryF:
                        return _psi0ImposedLoadCategoryF;
                    case ImposedLoadCategory.CategoryG:
                        return _psi0ImposedLoadCategoryG;
                    case ImposedLoadCategory.CategoryH:
                        return _psi0ImposedLoadCategoryH;
                    default:
                        throw new NotImplementedException("Failed to set coefficient psi0 for live load load or maintenance load");
                }
            }
            else 
            {
                switch (loadCaseType)
                {
                    case LoadCase.LoadCaseTypes.SelfWeight:
                    case LoadCase.LoadCaseTypes.SuperImposedDeadLoad:
                    case LoadCase.LoadCaseTypes.Earthquake: 
                        throw new ArgumentException("Don't exist coefficient for this load case type");
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaP:
                        return _psi0ClimateSummerDeltaP;
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaT:
                        return _psi0ClimateSummerDeltaP;
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaP:
                        return _psi0ClimateWinterDeltaP;
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaT:
                        return _psi0ClimateWinterDeltaT;
                    case LoadCase.LoadCaseTypes.WindPressure:
                    case LoadCase.LoadCaseTypes.WindSuction:
                        return _psi0Wind;
                    case LoadCase.LoadCaseTypes.Temperature:
                        return _psi0Temperature;
                    default:
                        throw new NotImplementedException("Not implemented coefficient for load case type");
                }
            }            
        }

        /// <summary>
        /// Get the coefficient psi 1 for buildings
        /// </summary>
        /// <param name="category">The category of the imposed load</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>        
        /// <returns>The value of the coefficient</returns>
        public double GetPsi1(ImposedLoadCategory category, LoadCase loadCase, bool highAltitude = true)
        {
            var loadCaseType = loadCase.LoadCaseType;

            if (loadCaseType == LoadCase.LoadCaseTypes.Snow)
            {
                if (highAltitude)
                    return Psi1SnowHighAltitude;
                else if (!highAltitude)
                    return Psi1SnowLowAltitude;
                else
                    throw new NotImplementedException("Failed to set coefficient psi0 for snow load");
            }
            else if (loadCaseType == LoadCase.LoadCaseTypes.LiveLoad || loadCaseType == LoadCase.LoadCaseTypes.Maintenance)
            {
                switch (category)
                {
                    case ImposedLoadCategory.CategoryA:
                        return _psi1ImposedLoadCategoryA;
                    case ImposedLoadCategory.CategoryB:
                        return _psi1ImposedLoadCategoryB;
                    case ImposedLoadCategory.CategoryC:
                        return _psi1ImposedLoadCategoryC;
                    case ImposedLoadCategory.CategoryD:
                        return _psi1ImposedLoadCategoryD;
                    case ImposedLoadCategory.CategoryE:
                        return _psi1ImposedLoadCategoryE;
                    case ImposedLoadCategory.CategoryF:
                        return _psi1ImposedLoadCategoryF;
                    case ImposedLoadCategory.CategoryG:
                        return _psi1ImposedLoadCategoryG;
                    case ImposedLoadCategory.CategoryH:
                        return _psi1ImposedLoadCategoryH;
                    default:
                        throw new NotImplementedException("Failed to set coefficient psi0 for live load load or maintenance load");
                }
            }
            else
            {
                switch (loadCaseType)
                {
                    case LoadCase.LoadCaseTypes.SelfWeight:
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaP:
                        return _psi1ClimateSummerDeltaP;
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaT:
                        return _psi1ClimateSummerDeltaP;
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaP:
                        return _psi1ClimateWinterDeltaP;
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaT:
                        return _psi1ClimateWinterDeltaT;
                    case LoadCase.LoadCaseTypes.Earthquake:
                    case LoadCase.LoadCaseTypes.SuperImposedDeadLoad:
                        throw new ArgumentException("Don't exist coefficient for this load case type");
                    case LoadCase.LoadCaseTypes.WindPressure:
                    case LoadCase.LoadCaseTypes.WindSuction:
                        return _psi1Wind;
                    case LoadCase.LoadCaseTypes.Temperature:
                        return _psi1Temperature;
                    default:
                        throw new NotImplementedException("Not implemented coefficient for load case type");
                }
            }
        }

        /// <summary>
        /// Get the coefficient psi 2 for buildings
        /// </summary>
        /// <param name="category">The category of the imposed load</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>
        /// <returns>The value of the coefficient</returns>
        public double GetPsi2(ImposedLoadCategory category, LoadCase loadCase, bool highAltitude = true)
        {
            var loadCaseType = loadCase.LoadCaseType;

            if (loadCaseType == LoadCase.LoadCaseTypes.Snow)
            {
                if (highAltitude)
                    return Psi2SnowHighAltitude;
                else if (!highAltitude)
                    return Psi2SnowLowAltitude;
                else
                    throw new NotImplementedException("Failed to set coefficient psi0 for snow load");
            }
            else if (loadCaseType == LoadCase.LoadCaseTypes.LiveLoad || loadCaseType == LoadCase.LoadCaseTypes.Maintenance)
            {
                switch (category)
                {
                    case ImposedLoadCategory.CategoryA:
                        return _psi2ImposedLoadCategoryA;
                    case ImposedLoadCategory.CategoryB:
                        return _psi2ImposedLoadCategoryB;
                    case ImposedLoadCategory.CategoryC:
                        return _psi2ImposedLoadCategoryC;
                    case ImposedLoadCategory.CategoryD:
                        return _psi2ImposedLoadCategoryD;
                    case ImposedLoadCategory.CategoryE:
                        return _psi2ImposedLoadCategoryE;
                    case ImposedLoadCategory.CategoryF:
                        return _psi2ImposedLoadCategoryF;
                    case ImposedLoadCategory.CategoryG:
                        return _psi2ImposedLoadCategoryG;
                    case ImposedLoadCategory.CategoryH:
                        return _psi2ImposedLoadCategoryH;
                    default:
                        throw new NotImplementedException("Failed to set coefficient psi0 for live load load or maintenance load");
                }
            }
            else
            {
                switch (loadCaseType)
                {
                    case LoadCase.LoadCaseTypes.SelfWeight:
                    case LoadCase.LoadCaseTypes.SuperImposedDeadLoad:
                    case LoadCase.LoadCaseTypes.Earthquake:
                        throw new ArgumentException("Don't exist coefficient for this load case type");
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaP:
                        return _psi2ClimateSummerDeltaP;
                    case LoadCase.LoadCaseTypes.ClimateSummerDeltaT:
                        return _psi2ClimateSummerDeltaP;
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaP:
                        return _psi2ClimateWinterDeltaP;
                    case LoadCase.LoadCaseTypes.ClimateWinterDeltaT:
                        return _psi2ClimateWinterDeltaT;
                    case LoadCase.LoadCaseTypes.WindPressure:
                    case LoadCase.LoadCaseTypes.WindSuction:
                        return _psi2Wind;
                    case LoadCase.LoadCaseTypes.Temperature:
                        return _psi2Temperature;
                    default:
                        throw new NotImplementedException("Not implemented coefficient for load case type");
                }
            }
        }

        #endregion
    }
}
