using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.Combinations;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the Eurocode Standard
    /// </summary>
    /// <remarks>Reference: EN 1990:2002/A1:2005</remarks>
    public class StandardEN1990 : Standard, Standard.ICombinationsGenerator
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
        private readonly double _gammaGFavourableSetA;
        private readonly double _gammaGUnfavourableSetA;
        private readonly double _gammaGFavourableSetB;
        private readonly double _gammaGUnfavourableSetB;
        private readonly double _gammaGFavourableSetC;
        private readonly double _gammaGUnfavourableSetC;

        // Gamma Q
        private readonly double _gammaQFavourableSetA;
        private readonly double _gammaQUnfavourableSetA;
        private readonly double _gammaQFavourableSetB;
        private readonly double _gammaQUnfavourableSetB;
        private readonly double _gammaQFavourableSetC;
        private readonly double _gammaQUnfavourableSetC;

        // Gamma P
        private readonly double _gammaPFavourableSetA;
        private readonly double _gammaPUnfavourableSetA;
        private readonly double _gammaPFavourableSetB;
        private readonly double _gammaPUnfavourableSetB;
        private readonly double _gammaPFavourableSetC;
        private readonly double _gammaPUnfavourableSetC;

        // Imposed Load Psi
        private readonly double _psi0ImposedLoadCategoryA;
        private readonly double _psi0ImposedLoadCategoryB;
        private readonly double _psi0ImposedLoadCategoryC;
        private readonly double _psi0ImposedLoadCategoryD;
        private readonly double _psi0ImposedLoadCategoryE;
        private readonly double _psi0ImposedLoadCategoryF;
        private readonly double _psi0ImposedLoadCategoryG;
        private readonly double _psi0ImposedLoadCategoryH;

        private readonly double _psi1ImposedLoadCategoryA;
        private readonly double _psi1ImposedLoadCategoryB;
        private readonly double _psi1ImposedLoadCategoryC;
        private readonly double _psi1ImposedLoadCategoryD;
        private readonly double _psi1ImposedLoadCategoryE;
        private readonly double _psi1ImposedLoadCategoryF;
        private readonly double _psi1ImposedLoadCategoryG;
        private readonly double _psi1ImposedLoadCategoryH;

        private readonly double _psi2ImposedLoadCategoryA;
        private readonly double _psi2ImposedLoadCategoryB;
        private readonly double _psi2ImposedLoadCategoryC;
        private readonly double _psi2ImposedLoadCategoryD;
        private readonly double _psi2ImposedLoadCategoryE;
        private readonly double _psi2ImposedLoadCategoryF;
        private readonly double _psi2ImposedLoadCategoryG;
        private readonly double _psi2ImposedLoadCategoryH;

        // Snow Psi
        private readonly double _psi0SnowHighAltitude;
        private readonly double _psi0SnowLowAltitude;
        private readonly double _psi1SnowHighAltitude;
        private readonly double _psi1SnowLowAltitude;
        private readonly double _psi2SnowHighAltitude;
        private readonly double _psi2SnowLowAltitude;

        // Wind Psi
        private readonly double _psi0Wind;
        private readonly double _psi1Wind;
        private readonly double _psi2Wind;

        // Temperature psi
        private readonly double _psi0Temperature;
        private readonly double _psi1Temperature;
        private readonly double _psi2Temperature;


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
        }

        #endregion

        #region COMBINATIONS OPTIONS

        public class EN1990CombinationsOptions : CombinationsOptions
        {
            public LimitStates LimitState { get; set; }

            public ImposedLoadCategories Category { get; set; } = ImposedLoadCategories.CategoryA;

            public ULSStructuralGeotechicalCombinationSets ULS { get; set; } = ULSStructuralGeotechicalCombinationSets.SetB;

            public bool HighAltitude { get; set; } = true;

            public EN1990CombinationsOptions(LimitStates limitState)
            {
                LimitState = limitState;
            }

            public EN1990CombinationsOptions(LimitStates limitState, ULSStructuralGeotechicalCombinationSets uLS = ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories imposedLoadCategories = ImposedLoadCategories.CategoryA, bool highAltitude = true)
            {
                LimitState = limitState;
                Category = imposedLoadCategories;
                ULS = uLS;
                HighAltitude = highAltitude;
            }

            public EN1990CombinationsOptions(LimitStates limitState, ImposedLoadCategories imposedLoadCategories = ImposedLoadCategories.CategoryA, bool highAltitude = true)
            {
                LimitState = limitState;
                Category = imposedLoadCategories;
                HighAltitude = highAltitude;
            }

            public override bool Equals(object obj)
            {
                if (obj is null)
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                EN1990CombinationsOptions objCasted = obj as EN1990CombinationsOptions;

                return !(objCasted is null) && objCasted.Category.Equals(Category) && objCasted.LimitState.Equals(LimitState) && objCasted.ULS.Equals(ULS) && objCasted.HighAltitude.Equals(HighAltitude);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = 23;
                    hashCode = 17 * hashCode + LimitState.GetHashCode();
                    hashCode = 17 * hashCode + Category.GetHashCode();
                    hashCode = 17 * hashCode + ULS.GetHashCode();
                    hashCode = 17 * hashCode + HighAltitude.GetHashCode();

                    return hashCode;
                }
            }
        }

        #endregion

        #region PUBLIC METHOD Gamma e Psi

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
        public double GetGammaQUnfavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState, LoadCase loadCase)
        {
            if (limitState == LimitStates.UltimateEquilibrium)
            {
                switch (loadCase.LoadCaseType)
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
                    switch (loadCase.LoadCaseType)
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
                    switch (loadCase.LoadCaseType)
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
                switch (loadCase.LoadCaseType)
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
            
            throw new ArgumentException("Failed to set coefficient gamma favourable");
        }

        /// <summary>
        /// Get the coefficient gamma Q favourable 
        /// </summary>
        /// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="loadCase">The load case</param>
        /// <returns>The value of the coefficient</returns>
        public double GetGammaQFavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState, LoadCase loadCase)
        {
            if (limitState == LimitStates.UltimateEquilibrium)
            {
                switch (loadCase.LoadCaseType)
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
                    switch (loadCase.LoadCaseType)
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
                    switch (loadCase.LoadCaseType)
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

            throw new ArgumentException("Failed to set coefficient gamma favourable");

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

        #endregion

        #region PUBLIC GENERATION METHODS

        /// <summary>
        /// Generate all the combinations with the load cases in <paramref name="loadCasesInput"/> and the settings <paramref name="options"/>
        /// </summary>
        /// <param name="loadCasesInput">List of load cases</param>
        /// <param name="options">The standard options</param>
        /// <param name="prefix">The common prefix for each combination in the collection (default name is "cmb")</param>
        /// <returns>A collection of combinations</returns>
        /// <exception cref="ArgumentException"> If there are any  climate load in the <paramref name="loadCasesInput"/></exception>
        public virtual CombinationsCollection CreateCombinations(LoadCaseBase[] loadCasesInput, CombinationsOptions options, string prefix = "cmb")
        {
            List<LoadCase> loadCases = new List<LoadCase>();
            foreach (LoadCaseBase loadCase in loadCasesInput)
            {
                if (loadCase is ClimateLoadCase climateLoadCase)
                    throw new ArgumentException("EN not support climate load: Load case must not be a climate load case");

                else if (loadCase is LoadCase LoadCaseNormal)
                    loadCases.Add(LoadCaseNormal);
            }

            CombinationsCollection combinations = new CombinationsCollection();
            Combination.CombinationCoefficientEqualityComparer equalityComparer = new Combination.CombinationCoefficientEqualityComparer();
            HashSet<Combination> combinationsHashSet = new HashSet<Combination>(equalityComparer);
            int idProg = 1;

            List<List<Combination.LoadCaseCoefficient>> listFavourable = GetFavourableCombinations(loadCases.ToArray(), (EN1990CombinationsOptions)options);
            for (int i = 0; i < listFavourable.Count(); i++)
            {
                Combination combo = new Combination(prefix + $" {idProg}", options);

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

            List<List<Combination.LoadCaseCoefficient>> listUnfavourable = GetUnfavourableCombinations(loadCases.ToArray(), (EN1990CombinationsOptions)options);
            for (int i = 0; i < listUnfavourable.Count(); i++)
            {
                Combination combo = new Combination(prefix + $" {idProg}", options);

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

            List<List<Combination.LoadCaseCoefficient>> listFavourableBase = GetBasicCombinationsMinCoeff(loadCases.ToArray(), (EN1990CombinationsOptions)options);
            for (int i = 0; i < listFavourableBase.Count(); i++)
            {
                Combination comboBaseFav = new Combination(prefix + $" {idProg}", options);
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

            List<List<Combination.LoadCaseCoefficient>> listUnfavourableBase = GetBasicCombinationsMaxCoeff(loadCases.ToArray(), (EN1990CombinationsOptions)options);
            for (int i = 0; i < listUnfavourableBase.Count(); i++)
            {
                Combination comboBaseUnfav = new Combination(prefix + $" {idProg}", options);
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

        #endregion

        #region PROTECTED METHODS

        /// <summary>
        /// Generate all the combination with favourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options</param>
        /// <returns>A list of load case coefficient</returns>
        protected virtual List<List<Combination.LoadCaseCoefficient>> GetFavourableCombinations(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN1990CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficients = new List<List<Combination.LoadCaseCoefficient>>();
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetBasicCombinationsMinCoeff(loadCases, options);

                List<LoadCase> list = new List<LoadCase>();
                foreach (LoadCase loadCase in loadCases)
                {
                    if (loadCase is LoadCase lc && (lc.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && lc.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
                        lc.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && lc.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake))
                        list.Add(loadCase);
                }

                List<List<Combination.LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list.ToArray(), options);

                for (int i = 0; i < randomList.Count(); i++)
                {
                    foreach (List<Combination.LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
                    {
                        List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
                        tempList.AddRange(l);
                        tempList.AddRange(randomList[i]);
                        loadCaseCoefficients.Add(tempList);
                    }
                }

                return loadCaseCoefficients;
            }
            throw new ArgumentException("CombinationsOptions must be EN1990CombinationsOptions");
        }

        /// <summary>
        /// Generate all the combination with unfavourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options</param>
        /// <returns>A list of load case coefficient</returns>
        protected virtual List<List<Combination.LoadCaseCoefficient>> GetUnfavourableCombinations(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN1990CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficients = new List<List<Combination.LoadCaseCoefficient>>();
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetBasicCombinationsMaxCoeff(loadCases, options);

                List<LoadCase> list = new List<LoadCase>();
                foreach (LoadCase loadCase in loadCases)
                {
                    if (loadCase is LoadCase lc && (lc.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && lc.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
                        lc.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && lc.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake))
                        list.Add(loadCase);
                }

                List<List<Combination.LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list.ToArray(), options);

                for (int i = 0; i < randomList.Count(); i++)
                {
                    foreach (List<Combination.LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
                    {
                        List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
                        tempList.AddRange(l);
                        tempList.AddRange(randomList[i]);
                        loadCaseCoefficients.Add(tempList);
                    }
                }

                return loadCaseCoefficients;
            }
            throw new ArgumentException("CombinationsOptions must be EN1990CombinationsOptions");
        }

        /// <summary>
        /// Generate all the combination for permanent loads with favourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options</param>
        /// <returns>A list of load case coefficient</returns>
        protected virtual List<List<Combination.LoadCaseCoefficient>> GetBasicCombinationsMinCoeff(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN1990CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBase = new List<Combination.LoadCaseCoefficient>();


                // aggiungo i SelfWeight
                foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight))
                {
                    Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
                // aggiungo i SuperImposedDeadLoad
                foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad))
                {
                    Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
                // aggiunto i Prestress
                foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress))
                {
                    Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
                // aggiunto il carico sismico se siamo in condizione sismica (come se fosse un permanente perchè non deve variare)
                if (options.LimitState == LimitStates.UltimateSeismic)
                {
                    foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake))
                    {
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                        loadCaseCoefficientsBase.Add(lc);
                    }
                }

                outList.Add(loadCaseCoefficientsBase);

                return outList;
            }
            throw new ArgumentException("CombinationsOptions must be EN1990CombinationsOptions");
        }

        /// <summary>
        /// Generate all the combination for permanent loads with unfavourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options</param>
        /// <returns>A list of load case coefficient</returns>
        protected virtual List<List<Combination.LoadCaseCoefficient>> GetBasicCombinationsMaxCoeff(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN1990CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBase = new List<Combination.LoadCaseCoefficient>();

                // aggiungo i SelfWeight
                foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight))
                {
                    Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
                // aggiungo i SuperImposedDeadLoad
                foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad))
                {
                    Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
                // aggiunto i Prestress
                foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress))
                {
                    Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
                // aggiunto il carico sismico se siamo in condizione sismica (come se fosse un permanente perchè non deve variare)
                if (options.LimitState == StandardEN1990.LimitStates.UltimateSeismic)
                {
                    foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake))
                    {
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                        loadCaseCoefficientsBase.Add(lc);
                    }
                }

                outList.Add(loadCaseCoefficientsBase);

                return outList;
            }
            throw new ArgumentException("CombinationsOptions must be EN1990CombinationsOptions");
        }

        /// <summary>
        /// Generate all the combination for the variable loads in <paramref name="loadCasesInput"/> with the options <paramref name="optionsInput"/>
        /// </summary>
        /// <param name="loadCasesInput">List of load cases</param>
        /// <param name="optionsInput">The combination generation options</param>
        /// <returns>A list of list of load case coefficient</returns>
        /// <exception cref="ArgumentException"> If there are any permanent load case or climate load in the <paramref name="loadCasesInput"/></exception>
        protected virtual List<List<Combination.LoadCaseCoefficient>> RandomizeVariableLoads(LoadCaseBase[] loadCasesInput, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN1990CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficients = new List<List<Combination.LoadCaseCoefficient>>();
                List<LoadCase> loadCasesList = new List<LoadCase>();

                // controllo che i carichi siano variabili
                foreach (LoadCaseBase loadCase in loadCasesInput)
                {
                    if (loadCase is LoadCase lc)
                    {
                        if (lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad ||
                        lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress || lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                            throw new ArgumentException("Load case must be Variable");
                        else
                            loadCasesList.Add(lc);
                    }

                    if (loadCase is ClimateLoadCase climateLoadCase)
                        throw new ArgumentException("EN not support climate load: Load case must not be a climate load case");
                }

                LoadCase[] loadCases = loadCasesList.ToArray();

                for (int i = 0; i < loadCases.Count(); i++)
                {
                    #region LIST, HASHSET E BOOL

                    HashSet<LoadCase.LoadCaseTypes> hash = new HashSet<LoadCase.LoadCaseTypes>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsWindPressure = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsWindSuction = new List<Combination.LoadCaseCoefficient>();
                    HashSet<LoadCase.LoadCaseTypes> hashAcc = new HashSet<LoadCase.LoadCaseTypes>();
                    bool haveWindPressure = false;
                    bool haveWindSuction = false;

                    #endregion

                    #region LEAD LOAD ADD

                    // crea un load lead, cerca tutti i carichi dello stesso tipo e li coefficienta alla stessa maniera.
                    LoadCase loadCaseLead = loadCases[i];
                    loadCaseCoefficientsBuffer = AddLoadCaseLead(loadCaseLead.LoadCaseType, loadCases, options);
                    hash.Add(loadCaseLead.LoadCaseType);

                    #endregion

                    // aggiunge tutti i carichi secondari che non siano wind pressure o wind suction. quei due vanno trattati a parte
                    foreach (LoadCase loadCaseAccompanying in loadCases)
                    {
                        #region NORMAL LOAD ADD

                        if (!hashAcc.Contains(loadCaseAccompanying.LoadCaseType) && !loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType) &&
                            loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction && loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
                        {
                            loadCaseCoefficientsBuffer.AddRange(AddLoadCaseAccompanying(loadCaseAccompanying.LoadCaseType, loadCases, options));
                            hashAcc.Add(loadCaseAccompanying.LoadCaseType);
                        }

                        #endregion

                        #region BOOL CHECK

                        // controllo se sono presenti carichi WindPressure o WindSuction per l'assemblaggio finale delle liste
                        if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                            haveWindPressure = true;
                        if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                            haveWindSuction = true;

                        #endregion
                    }

                    #region WIND LOAD ADD

                    // gestione carichi secondari windsuction
                    foreach (LoadCase loadCaseAccompanying in loadCases)
                    {
                        if (loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure && loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction &&
                            !loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType) && !hashAcc.Contains(loadCaseAccompanying.LoadCaseType))
                        {
                            loadCaseCoefficientsWindSuction.AddRange(AddLoadCaseAccompanying(LoadCase.LoadCaseTypes.WindSuction, loadCases, options));
                            hashAcc.Add(loadCaseAccompanying.LoadCaseType);
                        }
                    }

                    // gestione carichi secondari windpressure
                    foreach (LoadCase loadCaseAccompanying in loadCases)
                    {
                        if (loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction && loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure &&
                            !loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType) && !hashAcc.Contains(loadCaseAccompanying.LoadCaseType))
                        {
                            loadCaseCoefficientsWindPressure.AddRange(AddLoadCaseAccompanying(LoadCase.LoadCaseTypes.WindPressure, loadCases, options));
                            hashAcc.Add(loadCaseAccompanying.LoadCaseType);
                        }
                    }

                    #endregion

                    #region ASSEMBLY

                    if (haveWindPressure == true && haveWindSuction == true)
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
                            loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                    }
                    else if (haveWindPressure == false && haveWindSuction == true)
                    {
                        loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();

                        if (loadCaseCoefficientsWindSuction.Count() != 0)
                        {
                            loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindSuction);
                            loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                        }
                        if (loadCaseCoefficientsWindSuction.Count() == 0)
                            loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                    }
                    else if (haveWindPressure == true && haveWindSuction == false)
                    {
                        loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();

                        if (loadCaseCoefficientsWindPressure.Count() != 0)
                        {
                            loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindPressure);
                            loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                        }
                        if (loadCaseCoefficientsWindPressure.Count() == 0)
                            loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
                    }
                    else
                        loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);

                    #endregion

                }

                return loadCaseCoefficients;
            }
            throw new ArgumentException("CombinationsOptions must be EN1990CombinationsOptions");
        }

        /// <summary>
        /// Return a list of load case coefficients with all the load of type <paramref name="types"/> in the array <paramref name="loadCases"/> with the leading variable action coefficient
        /// </summary>
        /// <param name="types">The load case lead type (only EN1990 loads are supported)</param>
        /// <param name="loadCases">The array of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficients</returns>
        protected List<Combination.LoadCaseCoefficient> AddLoadCaseLead(LoadCase.LoadCaseTypes types, LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN1990CombinationsOptions options)
            {
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<Combination.LoadCaseCoefficient>();
                foreach (LoadCase loadCaseL in loadCases.Where(j => j is LoadCase lc && lc.LoadCaseType == types))
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficientLead = new Combination.LoadCaseCoefficient(GetCoefficientLeadingVariableAction(loadCaseL, options), loadCaseL);
                    loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                }
                return loadCaseCoefficientsBuffer;
            }
            else
                throw new ArgumentException("Lead load must not be a climate load or CombinationsOptions must be EN1990");
        }

        /// <summary>
        /// Return a list of load case coefficients with all the load of type <paramref name="types"/> in the array <paramref name="loadCases"/> with the accompanying variable action coefficient
        /// </summary>
        /// <param name="types">The load case lead type(only EN1990 loads are supported)</param>
        /// <param name="loadCases">The array of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficients</returns>
        protected List<Combination.LoadCaseCoefficient> AddLoadCaseAccompanying(LoadCase.LoadCaseTypes types, LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN1990CombinationsOptions options)
            {
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<Combination.LoadCaseCoefficient>();
                foreach (LoadCase loadCase in loadCases.Where(j => j is LoadCase lc && lc.LoadCaseType == types))
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficientLead = new Combination.LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(loadCase, options), loadCase);
                    loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                }
                return loadCaseCoefficientsBuffer;
            }
            else
                throw new ArgumentException("Lead load must not be a climate load or CombinationsOptions must be EN1990");
        }

        #endregion

        #region COEFFICIENT

        /// <summary>
        /// Return the coefficient of unfavourable permanent actions
        /// </summary>
        /// <param name="loadCase">The load cases (only permanent loads are accepted)</param>
        /// <param name="options">The normative options</param>
        /// <returns>The coefficient</returns>
        /// <exception cref="ArgumentException"> If don't exist the coefficient for the <paramref name="loadCase"/> with options <paramref name="options"/></exception>
        protected double GetCoefficientUnfavourablePermanentActions(LoadCase loadCase, EN1990CombinationsOptions options)
        {
            if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || loadCase.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                return GetGammaGUnfavourable(options.ULS, options.LimitState);
            else if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                return GetGammaGUnfavourable(options.ULS, options.LimitState);
            else if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.Prestress)
                return GetGammaPUnfavourable(options.ULS, options.LimitState);

            throw new ArgumentException("Failed to set coefficient favourable for permanent actions");
        }

        /// <summary>
        /// Return the coefficient of favourable permanent actions
        /// </summary>
        /// <param name="loadCase">The load cases (only permanent loads are accepted)</param>
        /// <param name="options">The normative options</param>
        /// <returns>The coefficient</returns>
        /// <exception cref="ArgumentException"> If don't exist the coefficient for the <paramref name="loadCase"/> with options <paramref name="options"/></exception>
        protected double GetCoefficientFavourablePermanentActions(LoadCase loadCase, EN1990CombinationsOptions options)
        {
            if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || loadCase.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                return GetGammaGFavourable(options.ULS, options.LimitState);
            else if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                return GetGammaGFavourable(options.ULS, options.LimitState);
            else if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.Prestress)
                return GetGammaPFavourable(options.ULS, options.LimitState);

            throw new ArgumentException("Failed to set coefficient favourable for permanent actions");
        }

        /// <summary>
        /// Return the coefficient of leading variable actions
        /// </summary>
        /// <param name="loadCase">The load cases (only variable loads are accepted)MO</param>
        /// <param name="options">The normative options</param>
        /// <returns>The coefficient</returns>
        /// <exception cref="ArgumentException"> If don't exist the coefficient for the <paramref name="loadCase"/> with options <paramref name="options"/></exception>
        protected double GetCoefficientLeadingVariableAction(LoadCase loadCase, EN1990CombinationsOptions options)
        {
            double psi1;
            double psi2;
            double gamma;
            double gammaQ;

            if (options.LimitState == LimitStates.UltimateEquilibrium || options.LimitState == LimitStates.UltimateFatigue
            || options.LimitState == LimitStates.UltimateGeotechnical || options.LimitState == LimitStates.UltimateStructural)
            {
                gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                return gamma;
            }
            else if (options.LimitState == LimitStates.UltimateSeismic)
            {
                gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
                return gammaQ * psi2;
            }
            else if (options.LimitState == LimitStates.UltimateAccidental)
            {
                gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                psi1 = GetPsi1(options.Category, loadCase, options.HighAltitude);
                return gammaQ * psi1;
            }
            else if (options.LimitState == LimitStates.ServiceabilityCharacteristic)
            {
                gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
                return gamma * psi2;
            }
            else if (options.LimitState == LimitStates.ServiceabilityFrequent)
            {
                gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                psi1 = GetPsi1(options.Category, loadCase, options.HighAltitude);
                return gamma * psi1;
            }
            else if (options.LimitState == LimitStates.ServiceabilityQuasiPermanent)
            {
                gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
                return gamma * psi2;
            }

            throw new ArgumentException("Failed to set the coefficient for leading variable actions");
        }

        /// <summary>
        /// Return the coefficient of accompanying variable actions
        /// </summary>
        /// <param name="loadCase">The load cases (only variable loads are accepted)</param>
        /// <param name="options">The normative options</param>
        /// <returns>The coefficient</returns>
        /// <exception cref="ArgumentException"> If don't exist the coefficient for the <paramref name="loadCase"/> with options <paramref name="options"/></exception>
        protected double GetCoefficientAccompanyingVariableAction(LoadCase loadCase, EN1990CombinationsOptions options)
        {
            double psi0;
            double psi2;
            double gammaQ;

            if (options.LimitState == LimitStates.UltimateEquilibrium || options.LimitState == LimitStates.UltimateFatigue
            || options.LimitState == LimitStates.UltimateGeotechnical || options.LimitState == LimitStates.UltimateStructural)
            {
                gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                psi0 = GetPsi0(options.Category, loadCase, options.HighAltitude);
                return gammaQ * psi0;
            }
            else if (options.LimitState == LimitStates.ServiceabilityCharacteristic)
            {
                gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                psi0 = GetPsi0(options.Category, loadCase, options.HighAltitude);
                return gammaQ * psi0;
            }
            else if (options.LimitState == LimitStates.UltimateSeismic)
            {
                gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
                return gammaQ * psi2;
            }
            else if (options.LimitState == LimitStates.UltimateAccidental)
            {
                gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
                return gammaQ * psi2;
            }
            else if (options.LimitState == LimitStates.ServiceabilityFrequent || options.LimitState == LimitStates.ServiceabilityQuasiPermanent)
            {
                gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
                return gammaQ * psi2;
            }

            throw new ArgumentException("Failed to set the coefficient for leading variable actions");
        }

        #endregion
                
    }
}
