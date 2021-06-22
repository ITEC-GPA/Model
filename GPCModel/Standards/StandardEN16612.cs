using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.Combinations;

namespace GPC.Model.Standards
{
    class StandardEN16612 : StandardEN1990
    {
        #region PUBLIC ENUMS


        #endregion

        #region VARIABLES

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

        public StandardEN16612()
        {          
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

        #region COMBINATIONS OPTIONS

        public class EN16612CombinationsOptions : EN1990CombinationsOptions
        {
            public EN16612CombinationsOptions(LimitStates limitState, ULSStructuralGeotechicalCombinationSets uLS = ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories imposedLoadCategories = ImposedLoadCategories.CategoryA, bool highAltitude = true)
                : base(limitState, uLS, imposedLoadCategories, highAltitude)
            {

            }

            public EN16612CombinationsOptions(LimitStates limitState)
                : base(limitState)
            {
                ULS = ULSStructuralGeotechicalCombinationSets.SetB;
                Category = ImposedLoadCategories.CategoryA;
                HighAltitude = true;
            }
        }

        #endregion

        #region PUBLIC METHOD Gamma e Psi

        /// <summary>
        /// Get the coefficient gamma Q unfavourable 
        /// </summary>
        /// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="climateLoadCase">The load case</param>
        /// <returns>The value of the coefficient</returns>
        public double GetGammaQUnfavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState, ClimateLoadCase climateLoadCase)
        {
            if (limitState == LimitStates.UltimateEquilibrium)
            {
                switch (climateLoadCase.ClimateType)
                {
                    case ClimateLoadCase.ClimateTypes.DeltaP:
                    case ClimateLoadCase.ClimateTypes.DeltaT:
                        return GammaQUnfavourableSetA;
                    default:
                        throw new NotImplementedException("Not implemented coefficient for load case type");
                }
            }
            else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
            {
                if (set == ULSStructuralGeotechicalCombinationSets.SetB)
                {
                    switch (climateLoadCase.ClimateType)
                    {
                        case ClimateLoadCase.ClimateTypes.DeltaP:
                        case ClimateLoadCase.ClimateTypes.DeltaT:
                            return GammaQUnfavourableSetB;
                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
                {
                    switch (climateLoadCase.ClimateType)
                    {
                        case ClimateLoadCase.ClimateTypes.DeltaP:
                        case ClimateLoadCase.ClimateTypes.DeltaT:
                            return GammaQUnfavourableSetC;
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
                switch (climateLoadCase.ClimateType)
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

        /// <summary>
        /// Get the coefficient gamma Q favourable 
        /// </summary>
        /// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="climateLoadCase">The load case</param>
        /// <returns>The value of the coefficient</returns>
        public double GetGammaQFavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState, ClimateLoadCase climateLoadCase)
        {
            if (limitState == LimitStates.UltimateEquilibrium)
            {
                switch (climateLoadCase.ClimateType)
                {
                    case ClimateLoadCase.ClimateTypes.DeltaP:
                    case ClimateLoadCase.ClimateTypes.DeltaT:
                        return GammaQFavourableSetA;
                    default:
                        throw new NotImplementedException("Not implemented coefficient for load case type");
                }
            }
            else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
            {
                if (set == ULSStructuralGeotechicalCombinationSets.SetB)
                {
                    switch (climateLoadCase.ClimateType)
                    {
                        case ClimateLoadCase.ClimateTypes.DeltaP:
                        case ClimateLoadCase.ClimateTypes.DeltaT:
                            return GammaQFavourableSetB;
                        default:
                            throw new NotImplementedException("Not implemented coefficient for load case type");
                    }
                }
                else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
                {
                    switch (climateLoadCase.ClimateType)
                    {
                        case ClimateLoadCase.ClimateTypes.DeltaP:
                        case ClimateLoadCase.ClimateTypes.DeltaT:
                            return GammaQFavourableSetC;
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
                switch (climateLoadCase.ClimateType)
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

        #region PUBLIC GENERATION METHODS

        /// <summary>
        /// Generate all the combinations with the load cases in <paramref name="loadCases"/> and the settings <paramref name="coomboOptions"/>
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="coomboOptions">The normative options (only EN16612 is supported)</param>
        /// <param name="name">The unique name of the combinations (default name is "cmb")</param>
        /// <returns>A collection of combinations</returns>
        public override CombinationsCollection CreateCombinations(LoadCaseBase[] loadCases, CombinationsOptions coomboOptions, string name = "cmb")
        {
            if (coomboOptions is EN16612CombinationsOptions options)
            {
                CombinationsCollection combinations = new CombinationsCollection();
                Combination.CombinationCoefficientEqualityComparer equalityComparer = new Combination.CombinationCoefficientEqualityComparer();
                HashSet<Combination> combinationsHashSet = new HashSet<Combination>(equalityComparer);
                int idProg = 1;

                List<List<Combination.LoadCaseCoefficient>> listFavourable = GetFavourableCombinations(loadCases, options);
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

                List<List<Combination.LoadCaseCoefficient>> listUnfavourable = GetUnfavourableCombinations(loadCases, options);
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

                List<List<Combination.LoadCaseCoefficient>> listCrossClimate = GetcrossedCombinationsMaxClimate(loadCases, options);
                for (int i = 0; i < listCrossClimate.Count(); i++)
                {
                    Combination comboBaseUnfav = new Combination(name + $" {idProg}", options);
                    for (int j = 0; j < listCrossClimate[i].Count(); j++)
                    {
                        comboBaseUnfav.AddLoadCaseCoefficient(listCrossClimate[i][j].LoadCase, listCrossClimate[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(comboBaseUnfav))
                    {
                        combinationsHashSet.Add(comboBaseUnfav);
                        idProg++;
                    }
                }

                List<List<Combination.LoadCaseCoefficient>> listCrossPerm = GetcrossedCombinationsMaxPermanent(loadCases, options);
                for (int i = 0; i < listCrossPerm.Count(); i++)
                {
                    Combination comboBaseUnfav = new Combination(name + $" {idProg}", options);
                    for (int j = 0; j < listCrossPerm[i].Count(); j++)
                    {
                        comboBaseUnfav.AddLoadCaseCoefficient(listCrossPerm[i][j].LoadCase, listCrossPerm[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(comboBaseUnfav))
                    {
                        combinationsHashSet.Add(comboBaseUnfav);
                        idProg++;
                    }
                }

                List<List<Combination.LoadCaseCoefficient>> listFavourableBase = GetBasicCombinationsMinCoeff(loadCases, options);
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

                List<List<Combination.LoadCaseCoefficient>> listUnfavourableBase = GetBasicCombinationsMaxCoeff(loadCases, options);
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
            else
                throw new ArgumentException("CombinationsOptions must be an istance of EN16612CombinationsOptions");
        }

        #endregion

        #region PROTECTED METHOD OVERRIDE

        /// <summary>
        /// Generate all the combination with favourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficient</returns>
        protected override List<List<Combination.LoadCaseCoefficient>> GetFavourableCombinations(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN16612CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficients = new List<List<Combination.LoadCaseCoefficient>>();
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetBasicCombinationsMinCoeff(loadCases, options);

                List<LoadCaseBase> list = new List<LoadCaseBase>();
                foreach (LoadCaseBase loadCase in loadCases)
                {
                    if ((loadCase is LoadCase lc && (lc.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && lc.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
                        lc.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && lc.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake)) ||
                        (loadCase is ClimateLoadCase clc && clc.ClimateType != ClimateLoadCase.ClimateTypes.DeltaH))
                        list.Add(loadCase);
                }
                List<List<Combination.LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list.ToArray(), options);

                for (int i = 0; i < randomList.Count(); i++)
                {
                    bool summerComboVariabili = false;
                    bool winterComboVariabili = false;

                    foreach (Combination.LoadCaseCoefficient loadCaseCoefficient in randomList[i])
                    {
                        if (loadCaseCoefficient.LoadCase is ClimateLoadCase lc)
                        {
                            if (lc.Season == ClimateLoadCase.Seasons.Summer && (lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT || lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                                summerComboVariabili = true;
                            if (lc.Season == ClimateLoadCase.Seasons.Winter && (lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT || lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                                winterComboVariabili = true;
                        }
                    }

                    foreach (List<Combination.LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
                    {
                        bool summerComboBase = false;
                        bool winterComboBase = false;

                        foreach (Combination.LoadCaseCoefficient lcc in l)
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
                            List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
                            tempList.AddRange(l);
                            tempList.AddRange(randomList[i]);
                            loadCaseCoefficients.Add(tempList);
                        }
                        else
                        {
                            List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
                            tempList.AddRange(l);
                            tempList.AddRange(randomList[i]);
                            loadCaseCoefficients.Add(tempList);
                        }
                    }
                }

                return loadCaseCoefficients;
            }
            throw new ArgumentException("CombinationsOptions must be EN16612CombinationsOptions");
        }

        /// <summary>
        /// Generate all the combination with unfavourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficient</returns>
        protected override List<List<Combination.LoadCaseCoefficient>> GetUnfavourableCombinations(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN16612CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficients = new List<List<Combination.LoadCaseCoefficient>>();
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetBasicCombinationsMaxCoeff(loadCases, options);

                List<LoadCaseBase> list = new List<LoadCaseBase>();
                foreach (LoadCaseBase loadCase in loadCases)
                    if ((loadCase is LoadCase lc && (lc.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && lc.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
                        lc.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && lc.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake)) ||
                        (loadCase is ClimateLoadCase clc && clc.ClimateType != ClimateLoadCase.ClimateTypes.DeltaH))
                        list.Add(loadCase);

                List<List<Combination.LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list.ToArray(), options);

                for (int i = 0; i < randomList.Count(); i++)
                {
                    bool summerComboVariabili = false;
                    bool winterComboVariabili = false;

                    foreach (Combination.LoadCaseCoefficient loadCaseCoefficient in randomList[i])
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

                    foreach (List<Combination.LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
                    {
                        bool summerComboBase = false;
                        bool winterComboBase = false;

                        foreach (Combination.LoadCaseCoefficient lcc in l)
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
                            // non si possono mischiare le combinazioni
                        }
                        else if ((summerComboVariabili && summerComboBase) || (winterComboVariabili && winterComboBase) || (!summerComboVariabili && !winterComboVariabili))
                        {
                            List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
                            tempList.AddRange(l);
                            tempList.AddRange(randomList[i]);
                            loadCaseCoefficients.Add(tempList);
                        }
                        else
                        {
                            List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
                            tempList.AddRange(l);
                            tempList.AddRange(randomList[i]);
                            loadCaseCoefficients.Add(tempList);
                        }
                    }
                }

                return loadCaseCoefficients;
            }
            throw new ArgumentException("CombinationsOptions must be EN16612CombinationsOptions");
        }

        /// <summary>
        /// Generate all the combination with favourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficient</returns>
        protected List<List<Combination.LoadCaseCoefficient>> GetcrossedCombinationsMaxClimate(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN16612CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficients = new List<List<Combination.LoadCaseCoefficient>>();
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetBasicCombinationsMinPermMaxClimate(loadCases, options);

                List<LoadCaseBase> list = new List<LoadCaseBase>();
                foreach (LoadCaseBase loadCase in loadCases)
                {
                    if ((loadCase is LoadCase lc && (lc.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && lc.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
                        lc.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && lc.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake)) ||
                        (loadCase is ClimateLoadCase clc && clc.ClimateType != ClimateLoadCase.ClimateTypes.DeltaH))
                        list.Add(loadCase);
                }
                List<List<Combination.LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list.ToArray(), options);

                for (int i = 0; i < randomList.Count(); i++)
                {
                    bool summerComboVariabili = false;
                    bool winterComboVariabili = false;

                    foreach (Combination.LoadCaseCoefficient loadCaseCoefficient in randomList[i])
                    {
                        if (loadCaseCoefficient.LoadCase is ClimateLoadCase lc)
                        {
                            if (lc.Season == ClimateLoadCase.Seasons.Summer && (lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT || lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                                summerComboVariabili = true;
                            if (lc.Season == ClimateLoadCase.Seasons.Winter && (lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT || lc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                                winterComboVariabili = true;
                        }
                    }

                    foreach (List<Combination.LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
                    {
                        bool summerComboBase = false;
                        bool winterComboBase = false;

                        foreach (Combination.LoadCaseCoefficient lcc in l)
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
                            List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
                            tempList.AddRange(l);
                            tempList.AddRange(randomList[i]);
                            loadCaseCoefficients.Add(tempList);
                        }
                        else
                        {
                            List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
                            tempList.AddRange(l);
                            tempList.AddRange(randomList[i]);
                            loadCaseCoefficients.Add(tempList);
                        }
                    }
                }

                return loadCaseCoefficients;
            }
            throw new ArgumentException("CombinationsOptions must be EN16612CombinationsOptions");
        }

        /// <summary>
        /// Generate all the combination with unfavourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficient</returns>
        protected List<List<Combination.LoadCaseCoefficient>> GetcrossedCombinationsMaxPermanent(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN16612CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficients = new List<List<Combination.LoadCaseCoefficient>>();
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetBasicCombinationsMaxPermMinClimate(loadCases, options);

                List<LoadCaseBase> list = new List<LoadCaseBase>();
                foreach (LoadCaseBase loadCase in loadCases)
                    if ((loadCase is LoadCase lc && (lc.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && lc.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
                        lc.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && lc.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake)) ||
                        (loadCase is ClimateLoadCase clc && clc.ClimateType != ClimateLoadCase.ClimateTypes.DeltaH))
                        list.Add(loadCase);

                List<List<Combination.LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list.ToArray(), options);

                for (int i = 0; i < randomList.Count(); i++)
                {
                    bool summerComboVariabili = false;
                    bool winterComboVariabili = false;

                    foreach (Combination.LoadCaseCoefficient loadCaseCoefficient in randomList[i])
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

                    foreach (List<Combination.LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
                    {
                        bool summerComboBase = false;
                        bool winterComboBase = false;

                        foreach (Combination.LoadCaseCoefficient lcc in l)
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
                            // non si possono mischiare le combinazioni
                        }
                        else if ((summerComboVariabili && summerComboBase) || (winterComboVariabili && winterComboBase) || (!summerComboVariabili && !winterComboVariabili))
                        {
                            List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
                            tempList.AddRange(l);
                            tempList.AddRange(randomList[i]);
                            loadCaseCoefficients.Add(tempList);
                        }
                        else
                        {
                            List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
                            tempList.AddRange(l);
                            tempList.AddRange(randomList[i]);
                            loadCaseCoefficients.Add(tempList);
                        }
                    }
                }

                return loadCaseCoefficients;
            }
            throw new ArgumentException("CombinationsOptions must be EN16612CombinationsOptions");
        }

        // NOTA: le due combinazioni crossed sono derivate dalla possibilità di massimizzare gli effetti sulle lastre massimizzando il delta H e minimizzando il peso proprio o viceversa
        // per questo ci sono anche i due metodi non sovrascritti dalla classe base GetBasicCombinationsMinPermMaxClimate e GetBasicCombinationsMaxPermMinClimate

        /// <summary>
        /// Generate all the combination for permanent loads with unfavourable coefficients for climate loads and favourable coefficient for normal loads
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficient</returns>
        protected List<List<Combination.LoadCaseCoefficient>> GetBasicCombinationsMaxPermMinClimate(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN16612CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBase = new List<Combination.LoadCaseCoefficient>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<Combination.LoadCaseCoefficient>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<Combination.LoadCaseCoefficient>();

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

                // aggiunto i climate. summer e winter non possono stare insieme
                if (haveCLimateSummer == true && haveCLimateWinter == false)
                {
                    loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                    foreach (ClimateLoadCase loadCase in loadCases.Where(i => i is ClimateLoadCase clc
                                                       && clc.Season == ClimateLoadCase.Seasons.Summer
                                                       && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))

                    {
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
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
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
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
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                        loadCaseCoefficientsBuffer3.Add(lc);
                    }
                    foreach (ClimateLoadCase loadCase in loadCases.Where(i => i is ClimateLoadCase clc
                                                       && clc.Season == ClimateLoadCase.Seasons.Summer
                                                       && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))

                    {
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
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
            throw new ArgumentException("CombinationsOptions must be EN16612CombinationsOptions");
        }

        /// <summary>
        /// Generate all the combination for permanent loads with unfavourable coefficients for climate loads and favourable coefficient for normal loads
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficient</returns>
        protected List<List<Combination.LoadCaseCoefficient>> GetBasicCombinationsMinPermMaxClimate(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN16612CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBase = new List<Combination.LoadCaseCoefficient>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<Combination.LoadCaseCoefficient>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<Combination.LoadCaseCoefficient>();

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
                    Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
                // aggiungo i SuperImposedDeadLoad
                foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad))
                {
                    Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
                // aggiunto i Prestress
                foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress))
                {
                    Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
                // aggiunto il carico sismico se siamo in condizione sismica (come se fosse un permanente perchè non deve variare)
                if (options.LimitState == StandardEN1990.LimitStates.UltimateSeismic)
                {
                    foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake))
                    {
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
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
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
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
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
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
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                        loadCaseCoefficientsBuffer3.Add(lc);
                    }
                    foreach (ClimateLoadCase loadCase in loadCases.Where(i => i is ClimateLoadCase clc
                                                       && clc.Season == ClimateLoadCase.Seasons.Summer
                                                       && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))

                    {
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
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
            throw new ArgumentException("CombinationsOptions must be EN16612CombinationsOptions");
        }

        /// <summary>
        /// Generate all the combination for permanent loads with favourable coefficients for all permanent actions (climate and normal loads)
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficient</returns>
        protected override List<List<Combination.LoadCaseCoefficient>> GetBasicCombinationsMinCoeff(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN16612CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBase = new List<Combination.LoadCaseCoefficient>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<Combination.LoadCaseCoefficient>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<Combination.LoadCaseCoefficient>();

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

                // aggiunto i climate. summer e winter non possono stare insieme
                if (haveCLimateSummer == true && haveCLimateWinter == false)
                {
                    loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                    foreach (ClimateLoadCase loadCase in loadCases.Where(x => x is ClimateLoadCase clc
                                                                              && clc.Season == ClimateLoadCase.Seasons.Summer
                                                                              && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))
                    {
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
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
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
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
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
                        loadCaseCoefficientsBuffer3.Add(lc);
                    }
                    foreach (ClimateLoadCase loadCase in loadCases.Where(x => x is ClimateLoadCase clc
                                                                              && clc.Season == ClimateLoadCase.Seasons.Summer
                                                                              && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))
                    {
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
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
            throw new ArgumentException("CombinationsOptions must be EN16612CombinationsOptions");
        }

        /// <summary>
        /// Generate all the combination for permanent loads with unfavourable coefficients for all permanent actions (climate and normal loads)
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficient</returns>
        protected override List<List<Combination.LoadCaseCoefficient>> GetBasicCombinationsMaxCoeff(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN16612CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBase = new List<Combination.LoadCaseCoefficient>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<Combination.LoadCaseCoefficient>();
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<Combination.LoadCaseCoefficient>();

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

                // aggiunto i climate. summer e winter non possono stare insieme
                if (haveCLimateSummer == true && haveCLimateWinter == false)
                {
                    loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                    foreach (ClimateLoadCase loadCase in loadCases.Where(i => i is ClimateLoadCase clc
                                                       && clc.Season == ClimateLoadCase.Seasons.Summer
                                                       && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))

                    {
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
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
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
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
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
                        loadCaseCoefficientsBuffer3.Add(lc);
                    }
                    foreach (ClimateLoadCase loadCase in loadCases.Where(i => i is ClimateLoadCase clc
                                                       && clc.Season == ClimateLoadCase.Seasons.Summer
                                                       && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH))

                    {
                        Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
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
            throw new ArgumentException("CombinationsOptions must be EN16612CombinationsOptions");
        }



        /// <summary>
        /// Generate all the combination for the variable loads
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of list of load case coefficient</returns>
        /// <exception cref="ArgumentException"> If there are any permanent load case in the <paramref name="loadCases"/></exception>
        protected override List<List<Combination.LoadCaseCoefficient>> RandomizeVariableLoads(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN16612CombinationsOptions options)
            {
                List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficients = new List<List<Combination.LoadCaseCoefficient >>();

                #region VARIABLE LOAD CHECK

                // controllo che i carichi siano variabili
                foreach (LoadCaseBase loadCase in loadCases)
                {
                    if ((loadCase is ClimateLoadCase clc && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH) ||
                        (loadCase is LoadCase lc && (lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad ||
                        lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress || lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)))
                        throw new ArgumentException("Load must be Variable");
                }

                #endregion

                for (int i = 0; i < loadCases.Count(); i++)
                {
                    #region LIST, HASHSET E BOOL

                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer4 = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer5 = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsSummer = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsWinter = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsWindPressure = new List<Combination.LoadCaseCoefficient>();
                    List<Combination.LoadCaseCoefficient> loadCaseCoefficientsWindSuction = new List<Combination.LoadCaseCoefficient>();

                    HashSet<LoadCase.LoadCaseTypes> hash = new HashSet<LoadCase.LoadCaseTypes>();
                    HashSet<(ClimateLoadCase.Seasons, ClimateLoadCase.ClimateTypes)> chash = new HashSet<(ClimateLoadCase.Seasons, ClimateLoadCase.ClimateTypes)>();

                    bool haveWindPressure = false;
                    bool haveWindSuction = false;
                    bool haveClimateSummer = false;
                    bool haveClimateWinter = false;

                    #endregion

                    #region LEAD LOAD ADD

                    // crea un load lead, cerca tutti i carichi dello stesso tipo e li coefficienta alla stessa maniera.
                    LoadCaseBase loadCaseLead = loadCases[i];

                    if (loadCaseLead is LoadCase lc)
                    {
                        loadCaseCoefficientsBuffer = AddLoadCaseLead(lc.LoadCaseType, loadCases, options);
                        hash.Add(lc.LoadCaseType);
                    }

                    else if (loadCaseLead is ClimateLoadCase clc)
                    {
                        loadCaseCoefficientsBuffer = AddLoadCaseLead(clc.Season, clc.ClimateType, loadCases, options);
                        chash.Add((clc.Season, clc.ClimateType));
                    }

                    #endregion

                    #region CHECK CLIMATE LOAD (se ci sono carichi che devono essere considerati lead insieme a loadCaseLead)

                    // i carichi climatici si massimizzano insieme
                    if (loadCaseLead is ClimateLoadCase loadCaseClimat)
                    {
                        if (loadCaseClimat.Season == ClimateLoadCase.Seasons.Summer && loadCaseClimat.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP)
                        {
                            loadCaseCoefficientsBuffer.AddRange(AddLoadCaseLead(ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT, loadCases, options));
                            chash.Add((ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT));
                        }
                        if (loadCaseClimat.Season == ClimateLoadCase.Seasons.Summer && loadCaseClimat.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT)
                        {
                            loadCaseCoefficientsBuffer.AddRange(AddLoadCaseLead(ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, loadCases, options));
                            chash.Add((ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP));
                        }
                        if (loadCaseClimat.Season == ClimateLoadCase.Seasons.Winter && loadCaseClimat.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT)
                        {
                            loadCaseCoefficientsBuffer.AddRange(AddLoadCaseLead(ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP, loadCases, options));
                            chash.Add((ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP));
                        }
                        if (loadCaseClimat.Season == ClimateLoadCase.Seasons.Winter && loadCaseClimat.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP)
                        {
                            loadCaseCoefficientsBuffer.AddRange(AddLoadCaseLead(ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaT, loadCases, options));
                            chash.Add((ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaT));
                        }
                    }

                    #endregion

                    // aggiunge tutti i carichi secondari che non siano wind pressure o wind suction o climatici. quelli vanno trattati a parte
                    foreach (LoadCaseBase loadCaseAccomp in loadCases)
                    {
                        #region NORMAL LOAD ADD

                        if (loadCaseAccomp is LoadCase loadCaseAccompanying)
                        {
                            if (loadCaseLead is LoadCase loadCaseLead1)
                            {
                                if (!hash.Contains(loadCaseAccompanying.LoadCaseType) && !loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead1.LoadCaseType) &&
                                    loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction && loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
                                {
                                    loadCaseCoefficientsBuffer.AddRange(AddLoadCaseAccompanying(loadCaseAccompanying.LoadCaseType, loadCases, options));
                                    hash.Add(loadCaseAccompanying.LoadCaseType);
                                }
                            }
                            else if (loadCaseLead is ClimateLoadCase _)
                            {
                                if (!hash.Contains(loadCaseAccompanying.LoadCaseType) &&
                                    loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction && loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
                                {
                                    loadCaseCoefficientsBuffer.AddRange(AddLoadCaseAccompanying(loadCaseAccompanying.LoadCaseType, loadCases, options));
                                    hash.Add(loadCaseAccompanying.LoadCaseType);
                                }
                            }
                        }
                        if (loadCaseAccomp is ClimateLoadCase climateLoadCaseAcc1)
                        {
                            if (loadCaseLead is ClimateLoadCase loadCaseLead2)
                            {
                                if (!chash.Contains((climateLoadCaseAcc1.Season, climateLoadCaseAcc1.ClimateType)) && !chash.Contains((climateLoadCaseAcc1.Season, climateLoadCaseAcc1.ClimateType)) &&
                                    (climateLoadCaseAcc1.Season.Equals(loadCaseLead2.Season) && !climateLoadCaseAcc1.ClimateType.Equals(loadCaseLead2.ClimateType)))
                                {
                                    loadCaseCoefficientsBuffer.AddRange(AddLoadCaseAccompanying(climateLoadCaseAcc1.Season, climateLoadCaseAcc1.ClimateType, loadCases, options));
                                    chash.Add((climateLoadCaseAcc1.Season, climateLoadCaseAcc1.ClimateType));
                                }
                            }
                            else if (loadCaseLead is LoadCase loadCaseLead4)
                            {
                                // viene gestito dopo
                            }
                        }

                        #endregion

                        #region BOOL CHECK

                        // controllo se sono presenti carichi WindPressure o WindSuction o Climatici per l'assemblaggio finale delle liste
                        if (loadCaseAccomp is LoadCase lcaaa)
                        {
                            if (lcaaa.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                                haveWindPressure = true;
                            if (lcaaa.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                                haveWindSuction = true;
                        }
                        else if (loadCaseAccomp is ClimateLoadCase clcac)
                        {
                            if (clcac.Season == ClimateLoadCase.Seasons.Summer && (clcac.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP || clcac.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                                haveClimateSummer = true;
                            if (clcac.Season == ClimateLoadCase.Seasons.Winter && (clcac.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP || clcac.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                                haveClimateWinter = true;
                        }

                        #endregion
                    }

                    #region WIND LOAD ADD

                    if ((loadCaseLead is LoadCase lcl &&
                        lcl.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure && lcl.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction) ||
                        (loadCaseLead is ClimateLoadCase _))
                    {
                        foreach (LoadCaseBase loadCaseAccom in loadCases)
                        {
                            // gestione carichi secondari windsuction
                            if (loadCaseAccom is LoadCase loadCaseAccompanying3 && loadCaseAccompanying3.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction &&
                                !hash.Contains(LoadCase.LoadCaseTypes.WindSuction))
                            {
                                loadCaseCoefficientsWindSuction.AddRange(AddLoadCaseAccompanying(LoadCase.LoadCaseTypes.WindSuction, loadCases, options));
                                hash.Add(LoadCase.LoadCaseTypes.WindSuction);
                            }

                            // gestione carichi secondari windpressure
                            if (loadCaseAccom is LoadCase loadCaseAccompanying4 && loadCaseAccompanying4.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure &&
                                !hash.Contains(LoadCase.LoadCaseTypes.WindPressure))
                            {
                                loadCaseCoefficientsWindPressure.AddRange(AddLoadCaseAccompanying(LoadCase.LoadCaseTypes.WindPressure, loadCases, options));
                                hash.Add(LoadCase.LoadCaseTypes.WindPressure);
                            }
                        }
                    }


                    #endregion

                    #region CLIMATE LOAD ADD

                    foreach (LoadCaseBase loadCaseAccompanying in loadCases)
                    {
                        // gestione carichi secondari climateSummer
                        if (loadCaseLead is LoadCase _ ||
                            (loadCaseLead is ClimateLoadCase climLeadLoadCase &&
                            climLeadLoadCase.ClimateType != ClimateLoadCase.ClimateTypes.DeltaP && climLeadLoadCase.ClimateType != ClimateLoadCase.ClimateTypes.DeltaT))
                        {
                            if (loadCaseAccompanying is ClimateLoadCase climAccomp && climAccomp.Season == ClimateLoadCase.Seasons.Winter &&
                                !chash.Contains((ClimateLoadCase.Seasons.Summer, climAccomp.ClimateType)))
                            {
                                loadCaseCoefficientsSummer.AddRange(AddLoadCaseAccompanying(ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, loadCases, options));
                                chash.Add((ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP));

                                loadCaseCoefficientsSummer.AddRange(AddLoadCaseAccompanying(ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT, loadCases, options));
                                chash.Add((ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT));
                            }
                        }

                        // gestione carichi secondari climate winter
                        if (loadCaseLead is LoadCase _ ||
                            (loadCaseLead is ClimateLoadCase climLeadLoadCase1 &&
                            climLeadLoadCase1.ClimateType != ClimateLoadCase.ClimateTypes.DeltaP && climLeadLoadCase1.ClimateType != ClimateLoadCase.ClimateTypes.DeltaT))
                        {
                            if (loadCaseAccompanying is ClimateLoadCase clcAcc && clcAcc.Season == ClimateLoadCase.Seasons.Summer &&
                                !chash.Contains((ClimateLoadCase.Seasons.Winter, clcAcc.ClimateType)))
                            {
                                loadCaseCoefficientsWinter.AddRange(AddLoadCaseAccompanying(ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP, loadCases, options));
                                chash.Add((ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP));
                                loadCaseCoefficientsWinter.AddRange(AddLoadCaseAccompanying(ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaT, loadCases, options));
                                chash.Add((ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaT));
                            }
                        }
                    }

                    #endregion

                    #region ASSEMBLY

                    // assembra le varie liste in base a quali carichi sono presenti

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

                return loadCaseCoefficients;
            }
            throw new ArgumentException("CombinationsOptions must be EN16612CombinationsOptions");
        }

        /// <summary>
        /// Return a list of climate load case coefficients with all the load of type <paramref name="type"/> and season <paramref name="season"/> in the array <paramref name="loadCases"/> 
        /// with the leading variable action coefficient
        /// </summary>
        /// <param name="season">The climate load case season</param>
        /// <param name="type">The climate load case type</param>
        /// <param name="loadCases">The array of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficients</returns>
        protected List<Combination.LoadCaseCoefficient> AddLoadCaseLead(ClimateLoadCase.Seasons season, ClimateLoadCase.ClimateTypes type, LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN16612CombinationsOptions options)
            {
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<Combination.LoadCaseCoefficient>();

                foreach (ClimateLoadCase loadCase in loadCases.Where(j => j is ClimateLoadCase l && l.ClimateType == type && l.Season == season))
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficientLead = new Combination.LoadCaseCoefficient(GetCoefficientLeadingVariableAction(loadCase, options), loadCase);
                    loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                }                
                return loadCaseCoefficientsBuffer;
            }
            else
                throw new ArgumentException("CombinationsOptions must be EN16612");
        }

        /// <summary>
        /// Return a list of climate load case coefficients with all the load of type <paramref name="type"/> and season <paramref name="season"/> in the array <paramref name="loadCases"/> 
        /// with the accompanying variable action coefficient
        /// </summary>
        /// <param name="season">The climate load case season</param>
        /// <param name="type">The climate load case type</param>
        /// <param name="loadCases">The array of load cases</param>
        /// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
        /// <returns>A list of load case coefficients</returns>
        protected List<Combination.LoadCaseCoefficient> AddLoadCaseAccompanying(ClimateLoadCase.Seasons season, ClimateLoadCase.ClimateTypes type, LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
        {
            if (optionsInput is EN16612CombinationsOptions options)
            {
                List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<Combination.LoadCaseCoefficient>();

                foreach (ClimateLoadCase loadCase in loadCases.Where(j => j is ClimateLoadCase l && l.ClimateType == type && l.Season == season))
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficientLead = new Combination.LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(loadCase, options), loadCase);
                    loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                }
                return loadCaseCoefficientsBuffer;
            }
            else
                throw new ArgumentException("CombinationsOptions must be EN16612");
        }

        #endregion

        #region COEFFICIENT FOR CLIMATE LOAD ACTIONS

        /// <summary>
        /// Return the coefficient of unfavourable permanent actions
        /// </summary>
        /// <param name="climateLoadCase">The load case (only climate Delta H is accepted)</param>
        /// <param name="options">The normative options (only EN16612 is supported)</param>
        /// <returns>The coefficient</returns>
        protected double GetCoefficientUnfavourablePermanentActions(ClimateLoadCase climateLoadCase, EN1990CombinationsOptions options)
        {
            if (climateLoadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH)
                return GetGammaGUnfavourable(options.ULS, options.LimitState);

            throw new Exception("Failed to set coefficient favourable for permanent actions");
        }

        /// <summary>
        /// Return the coefficient of favourable permanent actions
        /// </summary>
        /// <param name="climateLoadCase">The load case (only climate Delta H is accepted)</param>
        /// <param name="options">The normative options (only EN16612 is supported)</param>
        /// <returns>The coefficient</returns>
        protected double GetCoefficientFavourablePermanentActions(ClimateLoadCase climateLoadCase, EN1990CombinationsOptions options)
        {
            if (climateLoadCase.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH)
                return GetGammaGFavourable(options.ULS, options.LimitState);

            throw new Exception("Failed to set coefficient favourable for permanent actions");
        }

        /// <summary>
        /// Return the coefficient of leading variable actions
        /// </summary>
        /// <param name="climateLoadCase">The load case (only climate variable load are accepted)</param>
        /// <param name="options">The normative options (only EN16612 is supported)</param>
        /// <returns>The coefficient</returns>
        protected double GetCoefficientLeadingVariableAction(ClimateLoadCase climateLoadCase, EN1990CombinationsOptions options)
        {
            if (options.LimitState == LimitStates.UltimateEquilibrium || options.LimitState == LimitStates.UltimateFatigue
                || options.LimitState == LimitStates.UltimateGeotechnical || options.LimitState == LimitStates.UltimateStructural)
            {
                double gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, climateLoadCase);
                return gamma;
            }
            else if (options.LimitState == LimitStates.UltimateSeismic)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, climateLoadCase);
                double psi2 = GetPsi2(climateLoadCase);
                return gammaQ * psi2;
            }
            else if (options.LimitState == LimitStates.UltimateAccidental)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, climateLoadCase);
                double psi1 = GetPsi1(climateLoadCase);
                return gammaQ * psi1;
            }
            else if (options.LimitState == LimitStates.ServiceabilityCharacteristic)
            {
                double gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, climateLoadCase);
                double psi2 = GetPsi2(climateLoadCase);
                return gamma * psi2;
            }
            else if (options.LimitState == LimitStates.ServiceabilityFrequent)
            {
                double gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, climateLoadCase);
                double psi1 = GetPsi1(climateLoadCase);
                return gamma * psi1;
            }
            else if (options.LimitState == LimitStates.ServiceabilityQuasiPermanent)
            {
                double gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, climateLoadCase);
                double psi2 = GetPsi2(climateLoadCase);
                return gamma * psi2;
            }
            else
                throw new Exception("Failed to set the coefficient for leading variable actions");
        }

        /// <summary>
        /// Return the coefficient of accompanying variable actions
        /// </summary>
        /// <param name="climateLoadCase">the load case (only climate variable load are accepted)</param>
        /// <param name="options">The normative options (only EN16612 is supported)</param>
        /// <returns>The coefficient</returns>
        protected double GetCoefficientAccompanyingVariableAction(ClimateLoadCase climateLoadCase, EN1990CombinationsOptions options)
        {
            if (options.LimitState == LimitStates.UltimateEquilibrium || options.LimitState == LimitStates.UltimateFatigue
                || options.LimitState == LimitStates.UltimateGeotechnical || options.LimitState == LimitStates.UltimateStructural)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, climateLoadCase);
                double psi0 = GetPsi0(climateLoadCase);
                return gammaQ * psi0;
            }
            else if (options.LimitState == LimitStates.ServiceabilityCharacteristic)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, climateLoadCase);
                double psi0 = GetPsi0(climateLoadCase);
                return gammaQ * psi0;
            }
            else if (options.LimitState == LimitStates.UltimateSeismic)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, climateLoadCase);
                double psi2 = GetPsi2(climateLoadCase);
                return gammaQ * psi2;
            }
            else if (options.LimitState == LimitStates.UltimateAccidental)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, climateLoadCase);
                double psi2 = GetPsi2(climateLoadCase);
                return gammaQ * psi2;
            }
            else if (options.LimitState == LimitStates.ServiceabilityFrequent || options.LimitState == LimitStates.ServiceabilityQuasiPermanent)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, climateLoadCase);
                double psi2 = GetPsi2(climateLoadCase);
                return gammaQ * psi2;
            }
            else
                throw new Exception("Failed to set the coefficient for accompanying variable actions");
        }

        #endregion
    }
}
