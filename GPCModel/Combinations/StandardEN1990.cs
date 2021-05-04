using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using static GPC.Model.Combinations.Combination;

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
            SetB,
            SetC,
        }

        /// <summary>
        /// The limit states. Reference: EN 1990:2002/A1:2005 
        /// </summary>
        public enum LimitStates
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
        public enum ImposedLoadCategories
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
        public double GetGammaGUnfavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState)
        {
            if (limitState == LimitStates.UltimateEquilibrium)
            {
                return _gammaGUnfavourableSetA;
            }
            else if (limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateStructural)
            {
                if (set == ULSStructuralGeotechicalCombinationSets.SetB)
                    return _gammaGUnfavourableSetB;
                else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
                    return _gammaGUnfavourableSetC;
                else
                    throw new NotImplementedException("Failed to set coefficient gamma unfavourable");
            }
            else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
            {
                return 1.0;
            }
            else if (limitState == LimitStates.ServiceabilityQuasiPermanent || limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent)
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
        public double GetGammaGFavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState)
        {
            if (limitState == LimitStates.UltimateEquilibrium)
            {
                return _gammaGFavourableSetA;
            }
            else if (limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateStructural)
            {
                if (set == ULSStructuralGeotechicalCombinationSets.SetB)
                    return _gammaGFavourableSetB;
                else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
                    return _gammaGFavourableSetC;
                else
                    throw new NotImplementedException("Failed to set coefficient gamma G favourable");
            }
            else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
            {
                return 1.0;
            }
            else if (limitState == LimitStates.ServiceabilityQuasiPermanent || limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent)
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
        /// <returns>The value of the coefficient</returns>
        public double GetGammaPFavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState)
        {
            if (limitState == LimitStates.UltimateEquilibrium)
            {
                return _gammaPFavourableSetA;
            }
            else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
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
            else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
            {
                return 1.0;
            }
            else if (limitState == LimitStates.ServiceabilityQuasiPermanent || limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent)
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
        /// <returns>The value of the coefficient</returns>
        public double GetGammaPUnfavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState)
        {
            if (limitState == LimitStates.UltimateEquilibrium)
            {
                return _gammaPUnfavourableSetA;
            }
            else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
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
            else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
            {
                return 1.0;
            }
            else if (limitState == LimitStates.ServiceabilityQuasiPermanent || limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent)
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
        public double GetGammaQUnfavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState, LoadCaseBase loadCase)
        {
            if (loadCase is LoadCase lc)
            {
                var loadCaseType = lc.LoadCaseType;

                if (limitState == LimitStates.UltimateEquilibrium)
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
                            return _gammaQUnfavourableSetA;
                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
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
                                return _gammaQUnfavourableSetC;
                            default:
                                throw new NotImplementedException("Not implemented coefficient for load case type");
                        }
                    }
                    else
                        throw new NotImplementedException("Not implemented Annex");
                }
                else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
                {
                    return 1.0;
                }
                else if (limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent || limitState == LimitStates.ServiceabilityQuasiPermanent)
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
                            return 1.0;

                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new ArgumentException("Failed to set coefficient gamma favourable");
            }
            else if (loadCase is ClimateLoadCase clc)
            {
                var loadCaseType = clc.ClimateType;

                if (limitState == LimitStates.UltimateEquilibrium)
                {
                    switch (loadCaseType)
                    {
                        case ClimateLoadCase.ClimateTypes.DeltaP:
                        case ClimateLoadCase.ClimateTypes.DeltaT:
                            return _gammaQUnfavourableSetA;
                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
                {
                    if (set == ULSStructuralGeotechicalCombinationSets.SetB)
                    {
                        switch (loadCaseType)
                        {
                            case ClimateLoadCase.ClimateTypes.DeltaP:
                            case ClimateLoadCase.ClimateTypes.DeltaT:
                                return _gammaQUnfavourableSetB;
                            default:
                                throw new NotImplementedException("Not implemented coefficient for load case type");
                        }
                    }
                    else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
                    {
                        switch (loadCaseType)
                        {
                            case ClimateLoadCase.ClimateTypes.DeltaP:
                            case ClimateLoadCase.ClimateTypes.DeltaT:
                                return _gammaQUnfavourableSetC;
                            default:
                                throw new NotImplementedException("Not implemented coefficient for load case type");
                        }
                    }
                    else
                        throw new NotImplementedException("Not implemented Annex");
                }
                else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
                {
                    return 1.0;
                }
                else if (limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent || limitState == LimitStates.ServiceabilityQuasiPermanent)
                {
                    switch (loadCaseType)
                    {
                        case ClimateLoadCase.ClimateTypes.DeltaP:
                        case ClimateLoadCase.ClimateTypes.DeltaT:
                            return 1.0;

                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new ArgumentException("Failed to set coefficient gamma favourable");
            }

            throw new ArgumentException("Unsupported load case type");
        }

        /// <summary>
        /// Get the coefficient gamma Q favourable 
        /// </summary>
        /// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="loadCase">The load case</param>
        /// <returns>The value of the coefficient</returns>
        public double GetGammaQFavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState, LoadCaseBase loadCase)
        {
            if (loadCase is LoadCase lc)
            {
                var loadCaseType = lc.LoadCaseType;

                if (limitState == LimitStates.UltimateEquilibrium)
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
                            return _gammaQFavourableSetA;
                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
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
                                return _gammaQFavourableSetC;
                            default:
                                throw new NotImplementedException("Not implemented coefficient for load case type");
                        }
                    }
                    else
                        throw new NotImplementedException("Not implemented Annex");
                }
                else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
                {
                    return 1.0;
                }
                else if (limitState == LimitStates.ServiceabilityQuasiPermanent || limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent)
                {
                    return 1.00;
                }
            }
            else if (loadCase is ClimateLoadCase clc)
            {
                var loadCaseType = clc.ClimateType;

                if (limitState == LimitStates.UltimateEquilibrium)
                {
                    switch (loadCaseType)
                    {
                        case ClimateLoadCase.ClimateTypes.DeltaP:
                        case ClimateLoadCase.ClimateTypes.DeltaT:
                            return _gammaQFavourableSetA;
                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
                {
                    if (set == ULSStructuralGeotechicalCombinationSets.SetB)
                    {
                        switch (loadCaseType)
                        {
                            case ClimateLoadCase.ClimateTypes.DeltaP:
                            case ClimateLoadCase.ClimateTypes.DeltaT:
                                return _gammaQFavourableSetB;
                            default:
                                throw new NotImplementedException("Not implemented coefficient for load case type");
                        }
                    }
                    else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
                    {
                        switch (loadCaseType)
                        {
                            case ClimateLoadCase.ClimateTypes.DeltaP:
                            case ClimateLoadCase.ClimateTypes.DeltaT:
                                return _gammaQFavourableSetC;
                            default:
                                throw new NotImplementedException("Not implemented coefficient for load case type");
                        }
                    }
                    else
                        throw new NotImplementedException("Not implemented Annex");
                }
                else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
                {
                    return 1.0;
                }
                else if (limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent || limitState == LimitStates.ServiceabilityQuasiPermanent)
                {
                    switch (loadCaseType)
                    {
                        case ClimateLoadCase.ClimateTypes.DeltaP:
                        case ClimateLoadCase.ClimateTypes.DeltaT:
                            return 1.0;

                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new ArgumentException("Failed to set coefficient gamma favourable");
            }        
            
            throw new ArgumentException("Not implemented coefficient for load case type");
        }

        /// <summary>
        /// Get the coefficient psi 0 for buildings
        /// </summary>
        /// <param name="category">The category of the imposed load</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>
        /// <returns>The value of the coefficient</returns>
        public double GetPsi0(ImposedLoadCategories category, LoadCase loadCase, bool highAltitude = true)
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
                    case ImposedLoadCategories.CategoryA:
                        return _psi0ImposedLoadCategoryA;
                    case ImposedLoadCategories.CategoryB:
                        return _psi0ImposedLoadCategoryB;
                    case ImposedLoadCategories.CategoryC:
                        return _psi0ImposedLoadCategoryC;
                    case ImposedLoadCategories.CategoryD:
                        return _psi0ImposedLoadCategoryD;
                    case ImposedLoadCategories.CategoryE:
                        return _psi0ImposedLoadCategoryE;
                    case ImposedLoadCategories.CategoryF:
                        return _psi0ImposedLoadCategoryF;
                    case ImposedLoadCategories.CategoryG:
                        return _psi0ImposedLoadCategoryG;
                    case ImposedLoadCategories.CategoryH:
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
                    case LoadCase.LoadCaseTypes.WindPressure:
                    case LoadCase.LoadCaseTypes.WindSuction:
                        return _psi0Wind;
                    case LoadCase.LoadCaseTypes.Temperature:
                        return _psi0Temperature;
                    default:
                        throw new NotImplementedException("Not implemented coefficient for load case type");
                }
            }

            throw new ArgumentException("Don't exist coefficient for this load case");
        }

        /// <summary>
        /// Get the coefficient psi 0 for buildings
        /// </summary>
        /// <param name="loadCase">The climate load case</param>
        /// <returns>The value of the coefficient</returns>
        public double GetPsi0(ClimateLoadCase loadCase)
        {
            if (loadCase.Season == ClimateLoadCase.Seasons.Summer && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP)
                return _psi0ClimateSummerDeltaP;
            else if (loadCase.Season == ClimateLoadCase.Seasons.Summer && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT)
                return _psi0ClimateSummerDeltaP;
            else if (loadCase.Season == ClimateLoadCase.Seasons.Winter && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP)
                return _psi0ClimateWinterDeltaP;
            else if (loadCase.Season == ClimateLoadCase.Seasons.Winter && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT)
                return _psi0ClimateWinterDeltaT;
            throw new ArgumentException("Don't exist coefficient for this load case");
        }

        /// <summary>
        /// Get the coefficient psi 1 for buildings
        /// </summary>
        /// <param name="category">The category of the imposed load</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>        
        /// <returns>The value of the coefficient</returns>
        public double GetPsi1(ImposedLoadCategories category, LoadCase loadCase, bool highAltitude = true)
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
                    case ImposedLoadCategories.CategoryA:
                        return _psi1ImposedLoadCategoryA;
                    case ImposedLoadCategories.CategoryB:
                        return _psi1ImposedLoadCategoryB;
                    case ImposedLoadCategories.CategoryC:
                        return _psi1ImposedLoadCategoryC;
                    case ImposedLoadCategories.CategoryD:
                        return _psi1ImposedLoadCategoryD;
                    case ImposedLoadCategories.CategoryE:
                        return _psi1ImposedLoadCategoryE;
                    case ImposedLoadCategories.CategoryF:
                        return _psi1ImposedLoadCategoryF;
                    case ImposedLoadCategories.CategoryG:
                        return _psi1ImposedLoadCategoryG;
                    case ImposedLoadCategories.CategoryH:
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
        /// Get the coefficient psi 1 for buildings
        /// </summary>
        /// <param name="loadCase">The climate load case</param>
        /// <returns>The value of the coefficient</returns>
        public double GetPsi1(ClimateLoadCase loadCase)
        {
            if (loadCase.Season == ClimateLoadCase.Seasons.Summer && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP)
                return _psi1ClimateSummerDeltaP;
            else if (loadCase.Season == ClimateLoadCase.Seasons.Summer && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT)
                return _psi1ClimateSummerDeltaP;
            else if (loadCase.Season == ClimateLoadCase.Seasons.Winter && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP)
                return _psi1ClimateWinterDeltaP;
            else if (loadCase.Season == ClimateLoadCase.Seasons.Winter && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT)
                return _psi1ClimateWinterDeltaT;

            throw new ArgumentException("Don't exist coefficient for this load case");
        }

        /// <summary>
        /// Get the coefficient psi 2 for buildings
        /// </summary>
        /// <param name="category">The category of the imposed load</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>
        /// <returns>The value of the coefficient</returns>
        public double GetPsi2(ImposedLoadCategories category, LoadCase loadCase, bool highAltitude = true)
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
                    case ImposedLoadCategories.CategoryA:
                        return _psi2ImposedLoadCategoryA;
                    case ImposedLoadCategories.CategoryB:
                        return _psi2ImposedLoadCategoryB;
                    case ImposedLoadCategories.CategoryC:
                        return _psi2ImposedLoadCategoryC;
                    case ImposedLoadCategories.CategoryD:
                        return _psi2ImposedLoadCategoryD;
                    case ImposedLoadCategories.CategoryE:
                        return _psi2ImposedLoadCategoryE;
                    case ImposedLoadCategories.CategoryF:
                        return _psi2ImposedLoadCategoryF;
                    case ImposedLoadCategories.CategoryG:
                        return _psi2ImposedLoadCategoryG;
                    case ImposedLoadCategories.CategoryH:
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

        /// <summary>
        /// Get the coefficient psi 2 for buildings
        /// </summary>
        /// <param name="loadCase">The climate load case</param>
        /// <returns>The value of the coefficient</returns>
        public double GetPsi2(ClimateLoadCase loadCase)
        {
            if (loadCase.Season == ClimateLoadCase.Seasons.Summer && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP)
                return _psi2ClimateSummerDeltaP;
            else if (loadCase.Season == ClimateLoadCase.Seasons.Summer && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT)
                return _psi2ClimateSummerDeltaP;
            else if (loadCase.Season == ClimateLoadCase.Seasons.Winter && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP)
                return _psi2ClimateWinterDeltaP;
            else if (loadCase.Season == ClimateLoadCase.Seasons.Winter && loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT)
                return _psi2ClimateWinterDeltaT;

            throw new ArgumentException("Don't exist coefficient for this load case");
        }

        #endregion

        #region COMBINATIONS GENERATION

        public class En1990CombinationsOptions : CombinationsOptions
        {
            public LimitStates LimitState { get; set; }

            public ImposedLoadCategories Category { get; set; } = ImposedLoadCategories.CategoryA;

            public ULSStructuralGeotechicalCombinationSets ULS { get; set; } = ULSStructuralGeotechicalCombinationSets.SetB;

            public bool HighAltitude { get; set; } = true;

            public En1990CombinationsOptions(LimitStates limitState)
            {
                LimitState = limitState;
            }

            public En1990CombinationsOptions(LimitStates limitState, ULSStructuralGeotechicalCombinationSets uLS = ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories imposedLoadCategories = ImposedLoadCategories.CategoryA, bool highAltitude = true)
            {
                LimitState = limitState;
                Category = imposedLoadCategories;
                ULS = uLS;
                HighAltitude = highAltitude;
            }

            public En1990CombinationsOptions(LimitStates limitState, ImposedLoadCategories imposedLoadCategories = ImposedLoadCategories.CategoryA, bool highAltitude = true)
            {
                LimitState = limitState;
                Category = imposedLoadCategories;
                HighAltitude = highAltitude;
            }
        }

        public override CombinationsCollection CreateCombinations(LoadCaseBase[] loadCases, CombinationsOptions options, string name = "cmb")
        {
            CombinationsCollection combinations = new CombinationsCollection();
            CombinationCoefficientEqualityComparer equalityComparer = new CombinationCoefficientEqualityComparer();
            HashSet<Combination> combinationsHashSet = new HashSet<Combination>(equalityComparer);
            int idProg = 1;

            List<List<LoadCaseCoefficient>> listFavourable = GetFavourableCombinations(loadCases, (En1990CombinationsOptions)options);
            for (int i = 0; i < listFavourable.Count(); i++)
            {
                Combination combo = new Combination(name + $" {idProg}", options);

                for (int j = 0; j < listFavourable[i].Count(); j++)
                {
                    combo.AddLoadCaseCoefficient(listFavourable[i][j].LoadCase, listFavourable[i][j].Coefficient);
                }
                if (!combinationsHashSet.Contains(combo))
                {
                    combinationsHashSet.Add(combo);
                    idProg++;
                }
            }

            List<List<LoadCaseCoefficient>> listUnfavourable = GetUnfavourableCombinations(loadCases, (En1990CombinationsOptions)options);
            for (int i = 0; i < listUnfavourable.Count(); i++)
            {
                Combination combo = new Combination(name + $" {idProg}", options);

                for (int j = 0; j < listUnfavourable[i].Count(); j++)
                {
                    combo.AddLoadCaseCoefficient(listUnfavourable[i][j].LoadCase, listUnfavourable[i][j].Coefficient);
                }
                if (!combinationsHashSet.Contains(combo))
                {
                    combinationsHashSet.Add(combo);
                    idProg++;
                }
            }

            List<List<LoadCaseCoefficient>> listFavourableBase = GetFavourableBasicCombinations(loadCases, (En1990CombinationsOptions)options);
            for (int i = 0; i < listFavourableBase.Count(); i++)
            {
                Combination comboBaseFav = new Combination(name + $" {idProg}", options);
                for (int j = 0; j < listFavourableBase[i].Count(); j++)
                {
                    comboBaseFav.AddLoadCaseCoefficient(listFavourableBase[i][j].LoadCase, listFavourableBase[i][j].Coefficient);
                }
                if (!combinationsHashSet.Contains(comboBaseFav))
                {
                    combinationsHashSet.Add(comboBaseFav);
                    idProg++;
                }
            }

            List<List<LoadCaseCoefficient>> listUnfavourableBase = GetUnfavourableBasicCombinations(loadCases, (En1990CombinationsOptions)options);
            for (int i = 0; i < listUnfavourableBase.Count(); i++)
            {
                Combination comboBaseUnfav = new Combination(name + $" {idProg}", options);
                for (int j = 0; j < listUnfavourableBase[i].Count(); j++)
                {
                    comboBaseUnfav.AddLoadCaseCoefficient(listUnfavourableBase[i][j].LoadCase, listUnfavourableBase[i][j].Coefficient);
                }
                if (!combinationsHashSet.Contains(comboBaseUnfav))
                {
                    combinationsHashSet.Add(comboBaseUnfav);
                    idProg++;
                }
            }

            foreach (Combination cmb in combinationsHashSet)
                combinations.Add(cmb);

            return combinations;
        }


        /// <summary>
        /// Return the coefficient of favourable permanent actions
        /// </summary>
        /// <param name="loadCase">The load cases (only SelfWeight, SuperImposedDeadLoad and Prestress)</param>
        /// <param name="options">The generation options</param>
        /// <returns>The coefficient</returns>
        private double GetCoefficientFavourablePermanentActions(LoadCaseBase loadCase, En1990CombinationsOptions options)
        {
            if (loadCase is LoadCase lc)
            {
                var loadCaseType = lc.LoadCaseType;

                if (loadCaseType == LoadCase.LoadCaseTypes.SelfWeight || loadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                    return GetGammaGFavourable(options.ULS, options.LimitState);

                else if (loadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                    return GetGammaGFavourable(options.ULS, options.LimitState);
                else if (loadCaseType == LoadCase.LoadCaseTypes.Prestress)
                    return GetGammaPFavourable(options.ULS, options.LimitState);
            }
            else if (loadCase is ClimateLoadCase clc)
            {
                if (clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH)
                    return GetGammaGFavourable(options.ULS, options.LimitState);

            }
            throw new Exception("Failed to set coefficient favourable for permanent actions");
        }

        /// <summary>
        /// Generate all the combination for permanent loads with favourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="options">The generation options</param>
        /// <returns>A list of load case coefficient</returns>
        private List<List<LoadCaseCoefficient>> GetFavourableBasicCombinations(LoadCaseBase[] loadCases, En1990CombinationsOptions options)
        {
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBase = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<LoadCaseCoefficient>();

            // controllo che ci siano i carichi climatici
            bool haveCLimateSummer = false;
            bool haveCLimateWinter = false;
            foreach (ClimateLoadCase clc in loadCases.Where(lc => lc is ClimateLoadCase clc && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))
            {
                if (clc.Season == ClimateLoadCase.Seasons.Summer)
                    haveCLimateSummer = true;
                if (clc.Season == ClimateLoadCase.Seasons.Winter)
                    haveCLimateWinter = true;
            }
            
            // aggiungo i SelfWeight
            foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiungo i SuperImposedDeadLoad
            foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto i Prestress
            foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto il carico sismico se siamo in condizione sismica (come se fosse un permanente perchè non deve variare)
            if (options.LimitState == LimitStates.UltimateSeismic)
            {
                foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
            }

            // aggiunto i climate. summer e winter non possono stare insieme
            if (haveCLimateSummer == true && haveCLimateWinter == false)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (ClimateLoadCase loadCase in loadCases.Where(x => x is ClimateLoadCase clc
                                                                          && clc.Season == ClimateLoadCase.Seasons.Summer
                                                                          && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == false && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (ClimateLoadCase loadCase in loadCases.Where(x => x is ClimateLoadCase clc
                                                                          && clc.Season == ClimateLoadCase.Seasons.Winter
                                                                          && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options ), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == true && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBase.ToArray().ToList();

                foreach (ClimateLoadCase loadCase in loadCases.Where(x => x is ClimateLoadCase clc
                                                                          && clc.Season == ClimateLoadCase.Seasons.Winter
                                                                          && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBuffer3.Add(lc);
                }
                foreach (ClimateLoadCase loadCase in loadCases.Where(x => x is ClimateLoadCase clc
                                                                          && clc.Season == ClimateLoadCase.Seasons.Summer
                                                                          && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }

            if (loadCaseCoefficientsBuffer2.Count() != 0)
                outList.Add(loadCaseCoefficientsBuffer2);
            if (loadCaseCoefficientsBuffer3.Count() != 0)
                outList.Add(loadCaseCoefficientsBuffer3);

            outList.Add(loadCaseCoefficientsBase);
            
            return outList;
        }

        /// <summary>
        /// Generate all the combination with unfavourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="options"></param>
        /// <returns>A list of load case coefficient</returns>
        private List<List<LoadCaseCoefficient>> GetUnfavourableCombinations(LoadCaseBase[] loadCases, En1990CombinationsOptions options)
        {
            List<List<LoadCaseCoefficient>> loadCaseCoefficients = new List<List<LoadCaseCoefficient>>();
            List<List<LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetUnfavourableBasicCombinations(loadCases, options);

            List<LoadCaseBase> list = new List<LoadCaseBase>();
            foreach (LoadCaseBase loadCase in loadCases)
                if ((loadCase is LoadCase lc && (lc.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && lc.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
                    lc.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && lc.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake)) || 
                    (loadCase is ClimateLoadCase clc && clc.ClimateType != ClimateLoadCase.ClimateTypes.DeltaH))
                    list.Add(loadCase);

            List<List<LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list.ToArray(), options);

            for (int i = 0; i < randomList.Count(); i++)
            {
                bool summerComboVariabili = false;
                bool winterComboVariabili = false;

                foreach (LoadCaseCoefficient loadCaseCoefficient in randomList[i])
                {
                    if (loadCaseCoefficient.LoadCase is ClimateLoadCase loadCase)
                    {
                        if (loadCase.Season == ClimateLoadCase.Seasons.Summer
                            && (loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT || loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                            summerComboVariabili = true;

                        if (loadCase.Season == ClimateLoadCase.Seasons.Winter
                            && (loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT || loadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                            winterComboVariabili = true;
                    }
                }

                foreach (List<LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
                {
                    bool summerComboBase = false;
                    bool winterComboBase = false;

                    foreach (LoadCaseCoefficient lcc in l)
                    {
                        if (lcc.LoadCase is ClimateLoadCase loadCase)
                        {
                            if (loadCase.Season == ClimateLoadCase.Seasons.Summer)
                                summerComboBase = true;

                            if (loadCase.Season == ClimateLoadCase.Seasons.Winter)
                                winterComboBase = true;
                        }
                    }

                    if ((summerComboVariabili && winterComboBase) || (winterComboVariabili && summerComboBase))
                    {
                        // Giorgio: Serve ancora ??
                        // non si possono mischiare le combinazioni
                    }
                    else if ((summerComboVariabili && summerComboBase) || (winterComboVariabili && winterComboBase) || (!summerComboVariabili && !winterComboVariabili))
                    {
                        List<LoadCaseCoefficient> tempList = new List<LoadCaseCoefficient>();
                        tempList.AddRange(l);
                        tempList.AddRange(randomList[i]);
                        loadCaseCoefficients.Add(tempList);
                    }
                    else
                    {
                        List<LoadCaseCoefficient> tempList = new List<LoadCaseCoefficient>();
                        tempList.AddRange(l);
                        tempList.AddRange(randomList[i]);
                        loadCaseCoefficients.Add(tempList);
                    }
                }
            }

            return loadCaseCoefficients;
        }

        /// <summary>
        /// Generate all the combination for the variable loads
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="options">The combination generation options</param>
        /// <returns>A list of list of load case coefficient</returns>
        /// <exception cref="ArgumentException"> If there are any permanent load case in the <paramref name="loadCases"/></exception>
        private List<List<LoadCaseCoefficient>> RandomizeVariableLoads(LoadCaseBase[] loadCases, En1990CombinationsOptions options)
        {
            List<List<LoadCaseCoefficient>> loadCaseCoefficients = new List<List<LoadCaseCoefficient>>();

            // controllo che i carichi siano variabili
            foreach (LoadCaseBase loadCase in loadCases)
            {               
                if ((loadCase is ClimateLoadCase clc && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH) || 
                    (loadCase is LoadCase lc && ( lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad || 
                    lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress || lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)))
                    throw new ArgumentException("Load must be Variable");
            }
                      
            HashSet<LoadCase.LoadCaseTypes> hash = new HashSet<LoadCase.LoadCaseTypes>();
            HashSet<(ClimateLoadCase.Seasons, ClimateLoadCase.ClimateTypes)> chash = new HashSet<(ClimateLoadCase.Seasons, ClimateLoadCase.ClimateTypes)>();

            for (int i = 0; i < loadCases.Count(); i++)
            {
                List<LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<LoadCaseCoefficient>();

                // crea un load lead, cerca tutti i carichi dello stesso tipo e li coefficienta alla stessa maniera.
                LoadCaseBase loadCaseLead = loadCases[i];

                //if (!hash.Contains(lctype))
                if ((loadCases[i] is LoadCase && !hash.Contains(((LoadCase)loadCases[i]).LoadCaseType)) || 
                    (loadCases[i] is ClimateLoadCase && !chash.Contains((((ClimateLoadCase)loadCases[i]).Season, ((ClimateLoadCase)loadCases[i]).ClimateType))))
                {
                    if (loadCases[i] is LoadCase lc)
                    {
                        foreach (LoadCase loadCase in loadCases.Where(j => j is LoadCase l && l.LoadCaseType == lc.LoadCaseType))
                        {
                            LoadCaseCoefficient loadCaseCoefficientLead = 
                                new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(loadCase, options), loadCase);
                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                        }
                        hash.Add(lc.LoadCaseType);
                    }

                    #region CHECK CLIMATE LOAD (se ci sono carichi che devono essere considerati lead insieme a loadCaseLead)

                    if (loadCases[i] is ClimateLoadCase loadCaseClimat)
                    {
                        if (loadCaseClimat.Season == ClimateLoadCase.Seasons.Summer && loadCaseClimat.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP)
                        {
                            foreach (ClimateLoadCase climateLoadCase in loadCases.Where(j => j is ClimateLoadCase clc
                                                                                             && clc.Season == ClimateLoadCase.Seasons.Summer
                                                                                             && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                            {
                                LoadCaseCoefficient loadCaseCoefficientLead = 
                                    new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(climateLoadCase, options), climateLoadCase);
                                loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                            }
                            chash.Add((ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT));
                        }
                        if (loadCaseClimat.Season == ClimateLoadCase.Seasons.Summer && loadCaseClimat.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT)
                        {
                            foreach (ClimateLoadCase climateLoadCase in loadCases.Where(j => j is ClimateLoadCase clc
                                                                                             && clc.Season == ClimateLoadCase.Seasons.Summer
                                                                                             && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                            {
                                LoadCaseCoefficient loadCaseCoefficientLead = 
                                    new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(climateLoadCase, options), climateLoadCase);
                                loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                            }
                            chash.Add((ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP));
                        }
                        if (loadCaseClimat.Season == ClimateLoadCase.Seasons.Winter && loadCaseClimat.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT)
                        {
                            foreach (ClimateLoadCase climateLoadCase in loadCases.Where(j => j is ClimateLoadCase clc
                                                                                             && clc.Season == ClimateLoadCase.Seasons.Winter
                                                                                             && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                            {
                                LoadCaseCoefficient loadCaseCoefficientLead = 
                                    new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(climateLoadCase, options), climateLoadCase);
                                loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                            }
                            chash.Add((ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP));
                        }
                        if (loadCaseClimat.Season == ClimateLoadCase.Seasons.Winter && loadCaseClimat.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP)
                        {
                            foreach (ClimateLoadCase climateLoadCase in loadCases.Where(j => j is ClimateLoadCase clc
                                                                                             && clc.Season == ClimateLoadCase.Seasons.Winter
                                                                                             && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                            {
                                LoadCaseCoefficient loadCaseCoefficientLead = 
                                    new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(climateLoadCase, options), climateLoadCase);
                                loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                            }
                            chash.Add((ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaT));
                        }

                    }

                    #endregion
                    
                    List<LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<LoadCaseCoefficient>();
                    List<LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<LoadCaseCoefficient>();
                    List<LoadCaseCoefficient> loadCaseCoefficientsBuffer4 = new List<LoadCaseCoefficient>();
                    List<LoadCaseCoefficient> loadCaseCoefficientsBuffer5 = new List<LoadCaseCoefficient>();
                    List<LoadCaseCoefficient> loadCaseCoefficientsSummer = new List<LoadCaseCoefficient>();
                    List<LoadCaseCoefficient> loadCaseCoefficientsWinter = new List<LoadCaseCoefficient>();
                    List<LoadCaseCoefficient> loadCaseCoefficientsWindPressure = new List<LoadCaseCoefficient>();
                    List<LoadCaseCoefficient> loadCaseCoefficientsWindSuction = new List<LoadCaseCoefficient>();
                    HashSet<LoadCase.LoadCaseTypes> hashAcc = new HashSet<LoadCase.LoadCaseTypes>();
                    HashSet<(ClimateLoadCase.Seasons, ClimateLoadCase.ClimateTypes)> chashAcc = new HashSet<(ClimateLoadCase.Seasons, ClimateLoadCase.ClimateTypes)>();
                    HashSet<LoadCase.LoadCaseTypes> hashtemp = new HashSet<LoadCase.LoadCaseTypes>();
                    bool haveWindPressure = false;
                    bool haveWindSuction = false;
                    bool haveClimateSummer = false;
                    bool haveClimateWinter = false;
                    
                    // aggiunge tutti i carichi secondari che non siano wind pressure o wind suction o climatici. quei due vanno trattati a parte
                    foreach (LoadCaseBase loadCaseAccompanying in loadCases)
                    {
                        #region NORMAL LOAD ADD

                        if (loadCases[i] is LoadCase && loadCaseAccompanying is LoadCase)
                        {
                            if (!hashAcc.Contains(((LoadCase)loadCaseAccompanying).LoadCaseType))
                            {
                                if (!((LoadCase)loadCaseAccompanying).LoadCaseType.Equals(((LoadCase)loadCases[i]).LoadCaseType))
                                {
                                    var lcacctype = ((LoadCase)loadCaseAccompanying).LoadCaseType;
                                    if (((LoadCase)loadCaseAccompanying).LoadCaseType != LoadCase.LoadCaseTypes.WindSuction &&
                                        ((LoadCase)loadCaseAccompanying).LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
                                    {
                                        foreach (LoadCase lca in loadCases.Where(j => j is LoadCase tlc && tlc.LoadCaseType == lcacctype))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new
                                                LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add(lcacctype);
                                    }
                                }
                            }
                        }
                        else if (loadCases[i] is ClimateLoadCase && loadCaseAccompanying is ClimateLoadCase)
                        {
                            if (!chashAcc.Contains((((ClimateLoadCase)loadCaseAccompanying).Season, ((ClimateLoadCase)loadCaseAccompanying).ClimateType)))
                            {
                                var lcaccseason = ((ClimateLoadCase)loadCaseAccompanying).Season;
                                var lcacctype = ((ClimateLoadCase)loadCaseAccompanying).ClimateType;
                                if (!((ClimateLoadCase)loadCaseAccompanying).Season.Equals(((ClimateLoadCase)loadCases[i]).Season) && 
                                    !((ClimateLoadCase)loadCaseAccompanying).ClimateType.Equals(((ClimateLoadCase)loadCases[i]).ClimateType))
                                {
                                    if (!(((ClimateLoadCase)loadCaseAccompanying).Season == ClimateLoadCase.Seasons.Summer && 
                                        ((ClimateLoadCase)loadCaseAccompanying).ClimateType == ClimateLoadCase.ClimateTypes.DeltaP) &&
                                        !(((ClimateLoadCase)loadCaseAccompanying).Season == ClimateLoadCase.Seasons.Summer &&
                                        ((ClimateLoadCase)loadCaseAccompanying).ClimateType == ClimateLoadCase.ClimateTypes.DeltaT) &&
                                        !(((ClimateLoadCase)loadCaseAccompanying).Season == ClimateLoadCase.Seasons.Winter &&
                                        ((ClimateLoadCase)loadCaseAccompanying).ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                                    {
                                        foreach (ClimateLoadCase lca in loadCases.Where(j => j is ClimateLoadCase clc 
                                                                                             && clc.Season == lcaccseason 
                                                                                             && clc.ClimateType == lcacctype))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = 
                                                new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientAccompanying);
                                        }
                                        chashAcc.Add((lcaccseason, lcacctype));
                                    }
                                }
                            }
                        }

                        #endregion

                        #region BOOL CHECK

                        if (loadCaseAccompanying is LoadCase)
                        {
                            if (((LoadCase)loadCaseAccompanying).LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                                haveWindPressure = true;

                            if (((LoadCase)loadCaseAccompanying).LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                                haveWindSuction = true;
                        }
                        else if (loadCaseAccompanying is ClimateLoadCase clc)
                        {
                            if (clc.Season == ClimateLoadCase.Seasons.Summer && (clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP || clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                                haveClimateSummer = true;

                            if (clc.Season == ClimateLoadCase.Seasons.Winter && (clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP || clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                                haveClimateWinter = true;
                        }

                        #endregion
                    }
                    
                    #region WIND LOAD

                    // gestione dei carichi secondari quando sono presenti sia windpressure che windsuction
                    if ((loadCaseLead is LoadCase lcl && lcl.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure && lcl.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction) && haveWindPressure == true && haveWindSuction == true)
                    {
                        foreach (LoadCaseBase loadCaseAccom in loadCases)
                        {
                            if (lcl.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
                            {
                                if (loadCaseAccom is LoadCase loadCaseAccompanying && loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                                {
                                    if (!loadCaseAccompanying.LoadCaseType.Equals(lcl.LoadCaseType))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType))
                                        {
                                            foreach (LoadCase lca in loadCases.Where(j => j is LoadCase lcw && lcw.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                                loadCaseCoefficientsWindSuction.Add(loadCaseCoefficientAccompanying);
                                            }
                                            hashAcc.Add((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType);
                                        }
                                    }
                                }
                            }

                            if (lcl.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction)
                            {
                                if (loadCaseAccom is LoadCase loadCaseAccompanying && loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                                {
                                    if (!loadCaseAccompanying.LoadCaseType.Equals(lcl.LoadCaseType))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType))
                                        {
                                            foreach (LoadCase lca in loadCases.Where(j => j is LoadCase lcw && lcw.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                                loadCaseCoefficientsWindPressure.Add(loadCaseCoefficientAccompanying);
                                            }
                                            hashAcc.Add((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType);
                                        }
                                    }
                                }
                            }
                        }
                    }


                    if (haveWindPressure == false || haveWindSuction == false)
                    {
                        // gestione carichi secondari windsuction
                        foreach (LoadCaseBase loadCaseAccom in loadCases)
                        {
                            if (loadCaseLead is LoadCase lcl2 && lcl2.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
                            {
                                if (loadCaseAccom is LoadCase loadCaseAccompanying && loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                                {
                                    if (!loadCaseAccompanying.LoadCaseType.Equals(lcl2.LoadCaseType))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType))
                                        {
                                            foreach (LoadCase lca in loadCases.Where(j => j is LoadCase lcw && lcw.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                                loadCaseCoefficientsWindSuction.Add(loadCaseCoefficientAccompanying);
                                            }
                                            hashAcc.Add((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType);
                                        }
                                    }
                                }
                            }
                        }

                        // gestione carichi secondari windpressure
                        foreach (LoadCaseBase loadCaseAccom in loadCases)
                        {
                            if (loadCaseLead is LoadCase lcl2 && lcl2.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction)
                            {
                                if (loadCaseAccom is LoadCase loadCaseAccompanying && loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                                {
                                    if (!loadCaseAccompanying.LoadCaseType.Equals(lcl2.LoadCaseType))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType))
                                        {
                                            foreach (LoadCase lca in loadCases.Where(j => j is LoadCase lcw && lcw.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                                loadCaseCoefficientsWindPressure.Add(loadCaseCoefficientAccompanying);
                                            }
                                            hashAcc.Add((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    #endregion

                    #region CLIMATE LOAD

                    // gestione dei carichi secondari quando sono presenti sia climateSummer che climateWinter

                    // gestione carichi secondari climateSummer
                    foreach (LoadCaseBase loadCaseAccompanying in loadCases)
                    {
                        if (loadCaseLead is ClimateLoadCase climLeadLoadCase && climLeadLoadCase.ClimateType != ClimateLoadCase.ClimateTypes.DeltaP && climLeadLoadCase.ClimateType != ClimateLoadCase.ClimateTypes.DeltaT)
                        {
                            if (loadCaseAccompanying is ClimateLoadCase climAccomp && climAccomp.Season == ClimateLoadCase.Seasons.Winter && 
                                (climAccomp.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP || climAccomp.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                            {
                                if (!climAccomp.ClimateType.Equals(climLeadLoadCase.ClimateType))
                                {
                                    if (!hashAcc.Contains((LoadCase.LoadCaseTypes)climAccomp.ClimateType))
                                    {
                                        foreach (LoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Summer && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                            loadCaseCoefficientsSummer.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add((LoadCase.LoadCaseTypes)ClimateLoadCase.ClimateTypes.DeltaP);
                                        foreach (LoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Summer && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                            loadCaseCoefficientsSummer.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add((LoadCase.LoadCaseTypes)ClimateLoadCase.ClimateTypes.DeltaT);
                                    }
                                }
                            }
                        }
                    }

                    // gestione carichi secondari climate winter
                    foreach (LoadCaseBase loadCaseAccompanying in loadCases)
                    {
                        if (loadCaseLead is ClimateLoadCase clcLead && clcLead.ClimateType != ClimateLoadCase.ClimateTypes.DeltaP && clcLead.ClimateType != ClimateLoadCase.ClimateTypes.DeltaT)
                        {
                            if (loadCaseAccompanying is ClimateLoadCase clcAcc && clcAcc.Season == ClimateLoadCase.Seasons.Summer && 
                                (clcAcc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP || clcAcc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                            {
                                if (!clcAcc.ClimateType.Equals(clcLead.ClimateType))
                                {
                                    if (!hashAcc.Contains((LoadCase.LoadCaseTypes)clcAcc.ClimateType))
                                    {
                                        foreach (LoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Winter && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                            loadCaseCoefficientsWinter.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add((LoadCase.LoadCaseTypes)ClimateLoadCase.ClimateTypes.DeltaP);
                                        foreach (LoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Winter && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                            loadCaseCoefficientsWinter.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add((LoadCase.LoadCaseTypes)ClimateLoadCase.ClimateTypes.DeltaT);
                                    }
                                }
                            }
                        }
                    }



                    #endregion

                    #region ASSEMBLY

                    if (haveWindPressure == true && haveWindSuction == true)
                    {
                        if (haveClimateSummer == true && haveClimateWinter == true)
                        {
                            bool modWP = false;
                            bool modW = false;
                            bool modWS = false;
                            bool modS = false;

                            loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();
                            loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBuffer.ToArray().ToList();
                            loadCaseCoefficientsBuffer4 = loadCaseCoefficientsBuffer.ToArray().ToList();
                            loadCaseCoefficientsBuffer5 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindPressure.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindPressure);
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWindPressure);
                                modWP = true;
                            }

                            if (loadCaseCoefficientsWindSuction.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer4.AddRange(loadCaseCoefficientsWindSuction);
                                loadCaseCoefficientsBuffer5.AddRange(loadCaseCoefficientsWindSuction);
                                modWS = true;
                            }

                            if (loadCaseCoefficientsSummer.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsSummer);
                                loadCaseCoefficientsBuffer5.AddRange(loadCaseCoefficientsSummer);
                                modS = true;
                            }

                            if (loadCaseCoefficientsWinter.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWinter);
                                loadCaseCoefficientsBuffer4.AddRange(loadCaseCoefficientsWinter);
                                modW = true;
                            }

                            if (modWP && modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            if (modWP && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            if (modWS && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                            if (modWS && modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);

                            if ((modWS && modS == false) || (modWS == false && modS))
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);
                            else if ((modWS && modW == false) || (modWS == false && modW))
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                            else if ((modWP && modS == false) || (modWP == false && modS))
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            else if ((modWP && modW == false) || (modWP == false && modW))
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            else
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                        }
                        else if (haveClimateSummer == true && haveClimateWinter == false)
                        {
                            bool modWP = false;
                            bool modWS = false;
                            bool modS = false;

                            loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();
                            loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindPressure.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindPressure);
                                modWP = true;
                            }
                            if (loadCaseCoefficientsSummer.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsSummer);
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsSummer);
                                modS = true;
                            }

                            if (loadCaseCoefficientsWindSuction.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWindSuction);
                                modWS = true;
                            }

                            if (modWP && modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            if (modWS && modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            else if (modWP == false && modS || modWP && modS == false)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            else if ((modWS == false && modS) || (modWS && modS == false))
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            else
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);

                        }
                        else if (haveClimateSummer == false && haveClimateWinter == true)
                        {
                            bool modWP = false;
                            bool modWS = false;
                            bool modW = false;

                            loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();
                            loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindPressure.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindPressure);
                                modWP = true;
                            }
                            if (loadCaseCoefficientsWindSuction.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWindSuction);
                                modWS = true;
                            }
                            if (loadCaseCoefficientsWinter.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWinter);
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWinter);
                                modW = true;
                            }
                            if (modWP && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            if (modWS && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            if (modWP == false && modW || modWP && modW == false)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            else if ((modWS == false && modW) || (modWS && modW == false))
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            else
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                        }
                        else
                        {
                            loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();
                            loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindPressure.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindPressure);
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            }
                            if (loadCaseCoefficientsWindSuction.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWindSuction);
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            }
                            if (loadCaseCoefficientsWindSuction.Count() == 0 && loadCaseCoefficientsWindPressure.Count() == 0)
                            {
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                            }
                        }
                    }
                    else if (haveWindPressure == false && haveWindSuction == true)
                    {
                        if (haveClimateSummer == true && haveClimateWinter == true)
                        {
                            bool modS = false;
                            bool modWS = false;
                            bool modW = false;

                            loadCaseCoefficientsBuffer4 = loadCaseCoefficientsBuffer.ToArray().ToList();
                            loadCaseCoefficientsBuffer5 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindSuction.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer4.AddRange(loadCaseCoefficientsWindSuction);
                                loadCaseCoefficientsBuffer5.AddRange(loadCaseCoefficientsWindSuction);
                                modWS = true;
                            }
                            if (loadCaseCoefficientsWinter.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer4.AddRange(loadCaseCoefficientsWinter);
                                modW = true;
                            }
                            if (loadCaseCoefficientsSummer.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer5.AddRange(loadCaseCoefficientsSummer);
                                modS = true;
                            }

                            if (modWS && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                            if (modWS && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);
                            if (modW && modW)
                            {
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);
                            }
                            else if ((modWS == false && modW) || (modWS && modW == false))
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                            else if ((modWS == false && modS) || (modWS && modS == false))
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);
                            else
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                        }
                        else if (haveClimateSummer == true && haveClimateWinter == false)
                        {
                            bool modWS = false;
                            bool modS = false;

                            loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindSuction.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindSuction);
                                modWS = true;
                            }
                            if (loadCaseCoefficientsSummer.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsSummer);
                                modS = true;
                            }

                            if (modWS && modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            else if (!modWS || !modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            else
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);

                        }
                        else if (haveClimateSummer == false && haveClimateWinter == true)
                        {
                            bool modWS = false;
                            bool modW = false;

                            loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindSuction.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWindSuction);
                                modWS = true;
                            }
                            if (loadCaseCoefficientsWinter.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWinter);
                                modW = true;
                            }

                            if (modWS && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            else if (!modWS || !modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            else
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                        }
                        else
                        {
                            loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindSuction.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindSuction);
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            }
                            if (loadCaseCoefficientsWindSuction.Count() == 0)
                            {
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                            }
                        }
                    }
                    else if (haveWindPressure == true && haveWindSuction == false)
                    {
                        if (haveClimateSummer == true && haveClimateWinter == true)
                        {
                            bool modWP = false;
                            bool modW = false;
                            bool modS = false;

                            loadCaseCoefficientsBuffer4 = loadCaseCoefficientsBuffer.ToArray().ToList();
                            loadCaseCoefficientsBuffer5 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindPressure.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer4.AddRange(loadCaseCoefficientsWindPressure);
                                loadCaseCoefficientsBuffer5.AddRange(loadCaseCoefficientsWindPressure);
                                modWP = true;
                            }
                            if (loadCaseCoefficientsWinter.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer4.AddRange(loadCaseCoefficientsWinter);
                                modW = true;
                            }
                            if (loadCaseCoefficientsSummer.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer5.AddRange(loadCaseCoefficientsSummer);
                                modS = true;
                            }

                            if (modWP && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                            if (modWP && modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);
                            if (modW && modS)
                            {
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);
                            }
                            else if ((modWP == false && modS) || (modWP && modS == false))
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);
                            else if ((modWP == false && modW) || (modWP && modW == false))
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                            else
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);

                        }
                        else if (haveClimateSummer == true && haveClimateWinter == false)
                        {
                            bool modWP = false;
                            bool modS = false;
                            loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindPressure.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindPressure);
                                modWP = true;
                            }
                            if (loadCaseCoefficientsSummer.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsSummer);
                                modS = true;
                            }

                            if (modWP && modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            else if (!modWP || !modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            else
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                        }
                        else if (haveClimateSummer == false && haveClimateWinter == true)
                        {
                            bool modWP = false;
                            bool modW = false;
                            loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindPressure.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWindPressure);
                                modWP = true;
                            }
                            if (loadCaseCoefficientsWinter.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWinter);
                                modW = true;
                            }
                            if (modWP && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            else if (!modWP || !modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            else
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                        }
                        else
                        {
                            loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWindPressure.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindPressure);
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            }
                            if (loadCaseCoefficientsWindPressure.Count() == 0)
                            {
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                            }
                        }
                    }
                    else
                    {
                        if (haveClimateSummer == true && haveClimateWinter == true)
                        {
                            loadCaseCoefficientsBuffer4 = loadCaseCoefficientsBuffer.ToArray().ToList();
                            loadCaseCoefficientsBuffer5 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsWinter.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer4.AddRange(loadCaseCoefficientsWinter);
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                            }
                            if (loadCaseCoefficientsSummer.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer5.AddRange(loadCaseCoefficientsSummer);
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);
                            }
                            if (loadCaseCoefficientsSummer.Count() == 0 && loadCaseCoefficientsWinter.Count() == 0)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                        }
                        else if (haveClimateSummer == true && haveClimateWinter == false)
                        {
                            loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();

                            if (loadCaseCoefficientsSummer.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsSummer);
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            }
                            if (loadCaseCoefficientsSummer.Count() == 0)
                            {
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                            }
                        }
                        else if (haveClimateSummer == false && haveClimateWinter == true)
                        {
                            loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBuffer.ToArray().ToList();
                            if (loadCaseCoefficientsSummer.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWinter);
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            }
                            if (loadCaseCoefficientsSummer.Count() != 0)
                            {
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                            }
                        }
                        else
                        {
                            loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                        }
                    }

                    #endregion
                
                }
            }
            

            return loadCaseCoefficients;
        }

        /// <summary>
        /// Return the coefficient of unfavourable permanent actions
        /// </summary>
        /// <param name="loadCase">The load cases (only SelfWeight, SuperImposedDeadLoad and Prestress)</param>
        /// <param name="options"></param>
        /// <returns>The coefficient</returns>
        private double GetCoefficientUnfavourablePermanentActions(LoadCaseBase loadCase, En1990CombinationsOptions options)
        {
            if (loadCase is LoadCase lc)
            {
                var loadCaseType = lc.LoadCaseType;

                if (loadCaseType == LoadCase.LoadCaseTypes.SelfWeight || loadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                    return GetGammaGUnfavourable(options.ULS, options.LimitState);
                else if (loadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                    return GetGammaGUnfavourable(options.ULS, options.LimitState);
                else if (loadCaseType == LoadCase.LoadCaseTypes.Prestress)
                    return GetGammaPUnfavourable(options.ULS, options.LimitState);
            }
            else if (loadCase is ClimateLoadCase clc)
            {
                if (clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH)
                    return GetGammaGUnfavourable(options.ULS, options.LimitState);
            }

            throw new Exception("Failed to set coefficient favourable for permanent actions");
        }

        /// <summary>
        /// Generate all the combination for permanent loads with unfavourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="options"></param>
        /// <returns>A list of load case coefficient</returns>
        private List<List<LoadCaseCoefficient>> GetUnfavourableBasicCombinations(LoadCaseBase[] loadCases, En1990CombinationsOptions options)
        {
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBase = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<LoadCaseCoefficient>();

            // controllo che ci siano i carichi climatici
            bool haveCLimateSummer = false;
            bool haveCLimateWinter = false;
            foreach (ClimateLoadCase loadCase in loadCases.Where(lc => lc is ClimateLoadCase clc && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))
            {
                if (loadCase.Season == ClimateLoadCase.Seasons.Summer)
                    haveCLimateSummer = true;
                if (loadCase.Season == ClimateLoadCase.Seasons.Winter)
                    haveCLimateWinter = true;
            }

            // aggiungo i SelfWeight
            foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiungo i SuperImposedDeadLoad
            foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto i Prestress
            foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto il carico sismico se siamo in condizione sismica (come se fosse un permanente perchè non deve variare)
            if (options.LimitState == StandardEN1990.LimitStates.UltimateSeismic)
            {
                foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
            }

            // aggiunto i climate. summer e winter non possono stare insieme
            if (haveCLimateSummer == true && haveCLimateWinter == false)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (ClimateLoadCase loadCase in loadCases.Where(i => i is ClimateLoadCase clc
                                                   && clc.Season == ClimateLoadCase.Seasons.Summer
                                                   && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))

                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == false && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (ClimateLoadCase loadCase in loadCases.Where(i => i is ClimateLoadCase clc
                                                   && clc.Season == ClimateLoadCase.Seasons.Winter
                                                   && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))

                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == true && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBase.ToArray().ToList();

                foreach (ClimateLoadCase loadCase in loadCases.Where(i => i is ClimateLoadCase clc
                                                                   && clc.Season == ClimateLoadCase.Seasons.Winter
                                                                   && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBuffer3.Add(lc);
                }
                foreach (ClimateLoadCase loadCase in loadCases.Where(i => i is ClimateLoadCase clc
                                                   && clc.Season == ClimateLoadCase.Seasons.Summer
                                                   && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))

                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }

            if (loadCaseCoefficientsBuffer2.Count() != 0)
                outList.Add(loadCaseCoefficientsBuffer2);
            if (loadCaseCoefficientsBuffer3.Count() != 0)
                outList.Add(loadCaseCoefficientsBuffer3);

            outList.Add(loadCaseCoefficientsBase);
            
            return outList;
        }

        /// <summary>
        /// Generate all the combination with favourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="options">The genetation options</param>
        /// <returns>A list of load case coefficient</returns>
        private List<List<LoadCaseCoefficient>> GetFavourableCombinations(LoadCaseBase[] loadCases, En1990CombinationsOptions options)
        {
            List<List<LoadCaseCoefficient>> loadCaseCoefficients = new List<List<LoadCaseCoefficient>>();
            List<List<LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetFavourableBasicCombinations(loadCases, options);

            List<LoadCaseBase> list = new List<LoadCaseBase>();
            foreach (LoadCaseBase loadCase in loadCases)
            {
                if ((loadCase is LoadCase lc && (lc.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && lc.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
                    lc.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && lc.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake)) ||
                    (loadCase is ClimateLoadCase clc && clc.ClimateType != ClimateLoadCase.ClimateTypes.DeltaH))
                    list.Add(loadCase);
            }
            List<List<LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list.ToArray(), options);

            for (int i = 0; i < randomList.Count(); i++)
            {
                bool summerComboVariabili = false;
                bool winterComboVariabili = false;

                foreach (LoadCaseCoefficient loadCaseCoefficient in randomList[i])
                {
                    if (loadCaseCoefficient.LoadCase is ClimateLoadCase lc)
                    {
                        if (lc.Season == ClimateLoadCase.Seasons.Summer && (lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT || lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                            summerComboVariabili = true;
                        if (lc.Season == ClimateLoadCase.Seasons.Winter && (lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT || lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                            winterComboVariabili = true;
                    }
                }

                foreach (List<LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
                {
                    bool summerComboBase = false;
                    bool winterComboBase = false;

                    foreach (LoadCaseCoefficient lcc in l)
                    {
                        if (lcc.LoadCase is ClimateLoadCase lc)
                        {
                            if (lc.Season == ClimateLoadCase.Seasons.Summer)
                                summerComboBase = true;

                            if (lc.Season == ClimateLoadCase.Seasons.Winter)
                                winterComboBase = true;
                        }
                    }

                    if ((summerComboVariabili && winterComboBase) || (winterComboVariabili && summerComboBase))
                    {
                        // non si possono mischiare le combinazioni
                    }
                    else if ((summerComboVariabili && summerComboBase) || (winterComboVariabili && winterComboBase) || (!summerComboVariabili && !winterComboVariabili))
                    {
                        List<LoadCaseCoefficient> tempList = new List<LoadCaseCoefficient>();
                        tempList.AddRange(l);
                        tempList.AddRange(randomList[i]);
                        loadCaseCoefficients.Add(tempList);
                    }
                    else
                    {
                        List<LoadCaseCoefficient> tempList = new List<LoadCaseCoefficient>();
                        tempList.AddRange(l);
                        tempList.AddRange(randomList[i]);
                        loadCaseCoefficients.Add(tempList);
                    }
                }
            }

            return loadCaseCoefficients;
        }

        /// <summary>
        /// Return the coefficient of leading variable actions
        /// </summary>
        /// <param name="loadCase">The load cases (only variable load are accepted)MO</param>
        /// <param name="options"></param>
        /// <returns>The coefficient</returns>
        private double GetCoefficientLeadingVariableAction(LoadCaseBase loadCase, En1990CombinationsOptions options)
        {
            if (options.LimitState == LimitStates.UltimateEquilibrium || options.LimitState == LimitStates.UltimateFatigue
                || options.LimitState == LimitStates.UltimateGeotechnical || options.LimitState == LimitStates.UltimateStructural)
            {
                double gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                return gamma;
            }
            else if (options.LimitState == LimitStates.UltimateSeismic)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                double psi2 = loadCase is LoadCase ? GetPsi2(options.Category, (LoadCase)loadCase, options.HighAltitude) : GetPsi2((ClimateLoadCase)loadCase);
                return gammaQ * psi2;
            }
            else if (options.LimitState == LimitStates.UltimateAccidental)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                double psi1 = loadCase is LoadCase ? GetPsi1(options.Category, (LoadCase)loadCase, options.HighAltitude) : GetPsi1((ClimateLoadCase)loadCase);
                return gammaQ * psi1;
            }
            else if (options.LimitState == LimitStates.ServiceabilityCharacteristic)
            {
                double gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                double psi2 = loadCase is LoadCase ? GetPsi2(options.Category, (LoadCase)loadCase, options.HighAltitude) : GetPsi2((ClimateLoadCase)loadCase);
                return gamma * psi2;
            }
            else if (options.LimitState == LimitStates.ServiceabilityFrequent)
            {
                double gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                double psi1 = loadCase is LoadCase ? GetPsi1(options.Category, (LoadCase)loadCase, options.HighAltitude) : GetPsi1((ClimateLoadCase)loadCase);
                return gamma * psi1;
            }
            else if (options.LimitState == LimitStates.ServiceabilityQuasiPermanent)
            {
                double gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                double psi2 = loadCase is LoadCase ? GetPsi2(options.Category, (LoadCase)loadCase, options.HighAltitude) : GetPsi2((ClimateLoadCase)loadCase);
                return gamma * psi2;
            }
            else
                throw new Exception("Failed to set the coefficient for leading variable actions");
        }

        /// <summary>
        /// Return the coefficient of accompanying variable actions
        /// </summary>
        /// <param name="loadCase">the load cases (only variable load are accepted)</param>
        /// <param name="options"></param>
        /// <returns>The coefficient</returns>
        private double GetCoefficientAccompanyingVariableAction(LoadCaseBase loadCase, En1990CombinationsOptions options)
        {
            if (options.LimitState == LimitStates.UltimateEquilibrium || options.LimitState == LimitStates.UltimateFatigue 
                || options.LimitState == LimitStates.UltimateGeotechnical || options.LimitState == LimitStates.UltimateStructural)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                double psi0 = loadCase is LoadCase ?  GetPsi0(options.Category, (LoadCase)loadCase, options.HighAltitude) : GetPsi0((ClimateLoadCase)loadCase);
                return gammaQ * psi0;
            }
            else if (options.LimitState == LimitStates.ServiceabilityCharacteristic)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                double psi0 = loadCase is LoadCase ? GetPsi0(options.Category, (LoadCase)loadCase, options.HighAltitude) : GetPsi0((ClimateLoadCase)loadCase);
                return gammaQ * psi0;
            }
            else if (options.LimitState == LimitStates.UltimateSeismic)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                double psi2 = loadCase is LoadCase ? GetPsi2(options.Category, (LoadCase)loadCase, options.HighAltitude) : GetPsi2((ClimateLoadCase)loadCase);
                return gammaQ * psi2;
            }
            else if (options.LimitState == LimitStates.UltimateAccidental)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                double psi2 = loadCase is LoadCase ? GetPsi2(options.Category, (LoadCase)loadCase, options.HighAltitude) : GetPsi2((ClimateLoadCase)loadCase);
                return gammaQ * psi2;
            }
            else if (options.LimitState == LimitStates.ServiceabilityFrequent || options.LimitState == LimitStates.ServiceabilityQuasiPermanent)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                double psi2 = loadCase is LoadCase ? GetPsi2(options.Category, (LoadCase)loadCase, options.HighAltitude) : GetPsi2((ClimateLoadCase)loadCase);
                return gammaQ * psi2;
            }
            else
                throw new Exception("Failed to set the coefficient for accompanying variable actions");
        }


        #endregion
    }
}
