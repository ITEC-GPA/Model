using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using static GPC.Model.Combinations.Combination;

namespace GPC.Model.Combinations
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

        #region PUBLIC METHOD 

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

        #region COMBINATIONS OPTIONS

        public class EN16612CombinationsOptions : EN1990CombinationsOptions
        {
            public EN16612CombinationsOptions(LimitStates limitState, ULSStructuralGeotechicalCombinationSets uLS = ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories imposedLoadCategories = ImposedLoadCategories.CategoryA, bool highAltitude = true)
                :base(limitState, uLS, imposedLoadCategories, highAltitude)
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

        #region PUBLIC GENERATION METHODS

        public override CombinationsCollection CreateCombinations(LoadCaseBase[] loadCases, CombinationsOptions coomboOptions, string name = "cmb")
        {
            if (coomboOptions is EN16612CombinationsOptions options)
            {
                CombinationsCollection combinations = new CombinationsCollection();
                CombinationCoefficientEqualityComparer equalityComparer = new CombinationCoefficientEqualityComparer();
                HashSet<Combination> combinationsHashSet = new HashSet<Combination>(equalityComparer);
                int idProg = 1;

                List<List<LoadCaseCoefficient>> listFavourable = GetFavourableCombinations(loadCases, options);
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

                List<List<LoadCaseCoefficient>> listUnfavourable = GetUnfavourableCombinations(loadCases, options);
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

                List<List<LoadCaseCoefficient>> listFavourableBase = GetFavourableBasicCombinations(loadCases, options);
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

                List<List<LoadCaseCoefficient>> listUnfavourableBase = GetUnfavourableBasicCombinations(loadCases, options);
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
        /// <param name="options">The genetation options</param>
        /// <returns>A list of load case coefficient</returns>
        protected List<List<LoadCaseCoefficient>> GetFavourableCombinations(LoadCaseBase[] loadCases, EN16612CombinationsOptions options)
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
        /// Generate all the combination with unfavourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="options"></param>
        /// <returns>A list of load case coefficient</returns>
        protected List<List<LoadCaseCoefficient>> GetUnfavourableCombinations(LoadCaseBase[] loadCases, EN16612CombinationsOptions options)
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
        /// Generate all the combination for permanent loads with favourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="options">The generation options</param>
        /// <returns>A list of load case coefficient</returns>
        protected List<List<LoadCaseCoefficient>> GetFavourableBasicCombinations(LoadCaseBase[] loadCases, EN16612CombinationsOptions options)
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
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
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
        /// Generate all the combination for permanent loads with unfavourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="options"></param>
        /// <returns>A list of load case coefficient</returns>
        protected List<List<LoadCaseCoefficient>> GetUnfavourableBasicCombinations(LoadCaseBase[] loadCases, EN16612CombinationsOptions options)
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
        /// Generate all the combination for the variable loads
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="options">The combination generation options</param>
        /// <returns>A list of list of load case coefficient</returns>
        /// <exception cref="ArgumentException"> If there are any permanent load case in the <paramref name="loadCases"/></exception>
        protected List<List<LoadCaseCoefficient>> RandomizeVariableLoads(LoadCaseBase[] loadCases, EN16612CombinationsOptions options)
        {
            List<List<LoadCaseCoefficient>> loadCaseCoefficients = new List<List<LoadCaseCoefficient>>();

            // controllo che i carichi siano variabili
            foreach (LoadCaseBase loadCase in loadCases)
            {
                if ((loadCase is ClimateLoadCase clc && clc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaH) ||
                    (loadCase is LoadCase lc && (lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad ||
                    lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress || lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)))
                    throw new ArgumentException("Load must be Variable");
            }

            for (int i = 0; i < loadCases.Count(); i++)
            {
                List<LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<LoadCaseCoefficient>();
                HashSet<LoadCase.LoadCaseTypes> hash = new HashSet<LoadCase.LoadCaseTypes>();
                HashSet<(ClimateLoadCase.Seasons, ClimateLoadCase.ClimateTypes)> chash = new HashSet<(ClimateLoadCase.Seasons, ClimateLoadCase.ClimateTypes)>();

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

                    else if (loadCases[i] is ClimateLoadCase clc)
                    {
                        foreach (ClimateLoadCase loadCase in loadCases.Where(j => j is ClimateLoadCase l && l.ClimateType == clc.ClimateType && l.Season == clc.Season))
                        {
                            LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(loadCase, options), loadCase);
                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                        }
                        chash.Add((clc.Season, clc.ClimateType));
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

                        if (loadCases[i] is LoadCase loadCaseLead1 && loadCaseAccompanying is LoadCase loadCaseAc)
                        {
                            if (!hashAcc.Contains(loadCaseAc.LoadCaseType))
                            {
                                if (!loadCaseAc.LoadCaseType.Equals(loadCaseLead1.LoadCaseType))
                                {
                                    if (loadCaseAc.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction && loadCaseAc.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
                                    {
                                        foreach (LoadCase lca in loadCases.Where(j => j is LoadCase tlc && tlc.LoadCaseType == loadCaseAc.LoadCaseType))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add(loadCaseAc.LoadCaseType);
                                    }
                                }
                            }
                        }
                        else if (loadCases[i] is ClimateLoadCase loadCaseLead2 && loadCaseAccompanying is LoadCase loadCaseAccomp)
                        {
                            if (!hashAcc.Contains(((LoadCase)loadCaseAccompanying).LoadCaseType))
                            {
                                if (loadCaseAccomp.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction && loadCaseAccomp.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
                                {
                                    foreach (LoadCase lca in loadCases.Where(j => j is LoadCase tlc && tlc.LoadCaseType == loadCaseAccomp.LoadCaseType))
                                    {
                                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                        loadCaseCoefficientsBuffer.Add(loadCaseCoefficientAccompanying);
                                    }
                                    hashAcc.Add(loadCaseAccomp.LoadCaseType);
                                }                                
                            }
                        }
                        else if (loadCases[i] is ClimateLoadCase loadCaseLead3 && loadCaseAccompanying is ClimateLoadCase climateLoadCaseAcc1)
                        {
                            if (!chashAcc.Contains((climateLoadCaseAcc1.Season, climateLoadCaseAcc1.ClimateType)) && !chash.Contains((climateLoadCaseAcc1.Season, climateLoadCaseAcc1.ClimateType)))
                            {
                                if ((climateLoadCaseAcc1.Season.Equals(loadCaseLead3.Season) && !climateLoadCaseAcc1.ClimateType.Equals(loadCaseLead3.ClimateType)))
                                {
                                    foreach (ClimateLoadCase lca in loadCases.Where(j => j is ClimateLoadCase clc && clc.Season == climateLoadCaseAcc1.Season && clc.ClimateType == climateLoadCaseAcc1.ClimateType))
                                    {
                                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                        loadCaseCoefficientsBuffer.Add(loadCaseCoefficientAccompanying);
                                    }
                                    chashAcc.Add((climateLoadCaseAcc1.Season, climateLoadCaseAcc1.ClimateType));
                                }
                            }
                        }
                        else if (loadCases[i] is LoadCase loadCaseLead4 && loadCaseAccompanying is ClimateLoadCase climateLoadCaseAcc2)
                        {
                            // viene gestito dopo
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
                            if (loadCaseLead is LoadCase lcl2)
                            {
                                if (lcl2.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
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
                            if (loadCaseLead is ClimateLoadCase clld3)
                            {
                                if (loadCaseAccom is LoadCase loadCaseAccompanying && loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
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

                        // gestione carichi secondari windpressure
                        foreach (LoadCaseBase loadCaseAccom in loadCases)
                        {
                            if (loadCaseLead is LoadCase lcl2 )
                            {
                                if (lcl2.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction)
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
                            if (loadCaseLead is ClimateLoadCase clld3)
                            {
                                if (loadCaseAccom is LoadCase loadCaseAccompanying && loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
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
                                        foreach (ClimateLoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Summer && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                            loadCaseCoefficientsSummer.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add((LoadCase.LoadCaseTypes)ClimateLoadCase.ClimateTypes.DeltaP);
                                        foreach (ClimateLoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Summer && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                            loadCaseCoefficientsSummer.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add((LoadCase.LoadCaseTypes)ClimateLoadCase.ClimateTypes.DeltaT);
                                    }
                                }
                            }
                        }

                        if (loadCaseLead is LoadCase leadLoadCase)
                        {
                            if (loadCaseAccompanying is ClimateLoadCase climAccomp && climAccomp.Season == ClimateLoadCase.Seasons.Winter &&
                                (climAccomp.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP || climAccomp.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                            {
                                if (!chashAcc.Contains((ClimateLoadCase.Seasons.Summer, climAccomp.ClimateType)))
                                {
                                    foreach (ClimateLoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Summer && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                                    {
                                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                        loadCaseCoefficientsSummer.Add(loadCaseCoefficientAccompanying);
                                    }
                                    chashAcc.Add((ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP));
                                    foreach (ClimateLoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Summer && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                                    {
                                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                        loadCaseCoefficientsSummer.Add(loadCaseCoefficientAccompanying);
                                    }
                                    chashAcc.Add((ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT));
                                }
                            }
                        }
                    }

                    // gestione carichi secondari climate winter
                    foreach (LoadCaseBase loadCaseAccompanying in loadCases)
                    {
                        if (loadCaseLead is LoadCase leadLoadCase1)
                        {
                            if (loadCaseAccompanying is ClimateLoadCase clcAcc && clcAcc.Season == ClimateLoadCase.Seasons.Summer &&
                                (clcAcc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP || clcAcc.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                            {
                                if (!chashAcc.Contains((ClimateLoadCase.Seasons.Winter,clcAcc.ClimateType)))
                                {
                                    foreach (ClimateLoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Winter && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                                    {
                                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                        loadCaseCoefficientsWinter.Add(loadCaseCoefficientAccompanying);
                                    }
                                    chashAcc.Add((ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP));
                                    foreach (ClimateLoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Winter && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                                    {
                                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                        loadCaseCoefficientsWinter.Add(loadCaseCoefficientAccompanying);
                                    }
                                    chashAcc.Add((ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaT));
                                }
                            }


                            if (loadCaseAccompanying is ClimateLoadCase clcAcc2 && clcAcc2.Season == ClimateLoadCase.Seasons.Summer &&
                                (clcAcc2.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP || clcAcc2.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                            {
                                if (!chashAcc.Contains((ClimateLoadCase.Seasons.Winter, clcAcc2.ClimateType)))
                                {
                                    foreach (LoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Winter && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaP))
                                    {
                                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                        loadCaseCoefficientsWinter.Add(loadCaseCoefficientAccompanying);
                                    }
                                    chashAcc.Add((ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP));
                                    foreach (LoadCase lca in loadCases.Where(j => j is ClimateLoadCase clct && clct.Season == ClimateLoadCase.Seasons.Winter && clct.ClimateType == ClimateLoadCase.ClimateTypes.DeltaT))
                                    {
                                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, options), lca);
                                        loadCaseCoefficientsWinter.Add(loadCaseCoefficientAccompanying);
                                    }
                                    chashAcc.Add((ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaT));
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

        #endregion

        #region COEFFICIENT

        /// <summary>
        /// Return the coefficient of unfavourable permanent actions
        /// </summary>
        /// <param name="climateLoadCase">The load cases (only climate Delta H is accepted)</param>
        /// <param name="options">The normative options</param>
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
        /// <param name="climateLoadCase">The load cases (only climate Delta H is accepted)</param>
        /// <param name="options">The normative options</param>
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
        /// <param name="climateLoadCase">The load cases (only climate variable load are accepted)</param>
        /// <param name="options">The normative options</param>
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
        /// <param name="climateLoadCase">the load cases (only climate variable load are accepted)</param>
        /// <param name="options">The normative options</param>
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
