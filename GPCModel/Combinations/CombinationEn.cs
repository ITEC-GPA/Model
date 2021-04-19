using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.LoadCases;

namespace GPC.Model.Combinations
{
    public class CombinationEn : Combination
    {
        #region VARIABLES

        private StandardEN1990 _standardEN1990;

        private StandardEN1990.LimitState _limitState;
        public StandardEN1990.LimitState GetLimitState => _limitState;

        private StandardEN1990.ULSCombinationSets _uLSCombinationSets;
        public StandardEN1990.ULSCombinationSets GetCombinationSets => _uLSCombinationSets;

        private StandardEN1990.ImposedLoadCategory _imposedLoadCategory;
        public StandardEN1990.ImposedLoadCategory GetImposedLoadCategory => _imposedLoadCategory;

        #endregion


        #region PUBLIC CONSTRUCTOR

        public CombinationEn(string name, StandardEN1990 combinationType)
            : base(name)
        {
            this._standardEN1990 = combinationType;
        }

        public CombinationEn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _standardEN1990 = (StandardEN1990)info.GetValue("CombinationType", typeof(StandardEN1990));
        }

        public CombinationEn(string name) 
            : base(name)
        {
        }

        public CombinationEn()
            : base()
        {
        }

        public CombinationEn(string name, StandardEN1990 combinationType, StandardEN1990.LimitState limitState, StandardEN1990.ULSCombinationSets uLSCombinationSets, StandardEN1990.ImposedLoadCategory category)
            : base(name)
        {
            this._standardEN1990 = combinationType;
            this._uLSCombinationSets = uLSCombinationSets;
            this._imposedLoadCategory = category;
        }

        public CombinationEn(string name, StandardEN1990.LimitState limitState)
            :base(name)
        {
            this._limitState = limitState;
        }
        #endregion


        #region PUBLIC OVERRIDE METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CombinationType", _standardEN1990);
        }

        public override bool IsUltimate() => (_standardEN1990.GetLimitState() == StandardEN1990.LimitState.UltimateEquilibrium ||
                                             _standardEN1990.GetLimitState() == StandardEN1990.LimitState.UltimateFatigue ||
                                             _standardEN1990.GetLimitState() == StandardEN1990.LimitState.UltimateGeotechnical ||
                                             _standardEN1990.GetLimitState() == StandardEN1990.LimitState.UltimateStructural) ?
                                             true : false;

        public override string ToString()
        {
            return base.ToString();
        }

        #endregion


        #region PUBLIC METHOD   

        /// <summary>
        /// Generate the combinations of design with the <paramref name="standardEN1990"/> normative
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <returns>A list of combination</returns>
        public static List<CombinationEn> GenerateCombinations(List<LoadCase> loadCases, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSCombinationSets uLS, StandardEN1990.ImposedLoadCategory category)
        {
            List<CombinationEn> combinations = new List<CombinationEn>();

            List<List<LoadCaseCoefficient>> listFavourable = GetFavourableCombinations(loadCases, standardEN1990, limitState, uLS, category);
            for (int i = 0; i< listFavourable.Count(); i++)
            {
                CombinationEn combo = new CombinationEn();

                for (int j = 0; j < listFavourable[i].Count(); j++)
                {
                    combo.AddLoadCaseCoefficient(listFavourable[i][j].LoadCase, listFavourable[i][j].Coefficient);                    
                }
                combinations.Add(combo);
            }

            if (limitState == StandardEN1990.LimitState.UltimateEquilibrium || limitState == StandardEN1990.LimitState.UltimateFatigue || limitState == StandardEN1990.LimitState.UltimateGeotechnical || limitState == StandardEN1990.LimitState.UltimateStructural)
            {
                List<List<LoadCaseCoefficient>> listUnfavourable = GetUnfavourableCombinations(loadCases, standardEN1990, limitState, uLS, category);
                for (int i = 0; i < listUnfavourable.Count(); i++)
                {
                    CombinationEn combo = new CombinationEn();

                    for (int j = 0; j < listUnfavourable[i].Count(); j++)
                    {
                        combo.AddLoadCaseCoefficient(listUnfavourable[i][j].LoadCase, listUnfavourable[i][j].Coefficient);
                    }
                    combinations.Add(combo);
                }
            }

            if (limitState == StandardEN1990.LimitState.UltimateEquilibrium || limitState == StandardEN1990.LimitState.UltimateFatigue || limitState == StandardEN1990.LimitState.UltimateGeotechnical || limitState == StandardEN1990.LimitState.UltimateStructural)
            {
                List<LoadCaseCoefficient> listFavourableBase = GetFavourableBasicCombinations(loadCases, standardEN1990, limitState, uLS, category);
                CombinationEn comboBaseFav = new CombinationEn();
                for (int j = 0; j < listFavourableBase.Count(); j++)
                {
                    comboBaseFav.AddLoadCaseCoefficient(listFavourableBase[j].LoadCase, listFavourableBase[j].Coefficient);
                }
                combinations.Add(comboBaseFav);
            }

            List<LoadCaseCoefficient> listUnfavourableBase = GetUnfavourableBasicCombinations(loadCases, standardEN1990, limitState, uLS, category);
            CombinationEn comboBaseUnfav = new CombinationEn();
            for (int j = 0; j < listUnfavourableBase.Count(); j++)
            {
                comboBaseUnfav.AddLoadCaseCoefficient(listUnfavourableBase[j].LoadCase, listUnfavourableBase[j].Coefficient);
            }
            combinations.Add(comboBaseUnfav);

            return combinations;
        }

        #endregion


        #region PRIVATE METHOD

        /// <summary>
        /// Generate all the combination for permanent loads with favourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <returns>A list of load case coefficient</returns>
        private static List<LoadCaseCoefficient> GetFavourableBasicCombinations(List<LoadCase> loadCases, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSCombinationSets uLS, StandardEN1990.ImposedLoadCategory category)
        {
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<LoadCaseCoefficient>();

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }
            return loadCaseCoefficientsBuffer;
        }

        /// <summary>
        /// Generate all the combination for permanent loads with unfavourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <returns>A list of load case coefficient</returns>
        private static List<LoadCaseCoefficient> GetUnfavourableBasicCombinations(List<LoadCase> loadCases, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSCombinationSets uLS, StandardEN1990.ImposedLoadCategory category)
        {
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<LoadCaseCoefficient>();

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }
            return loadCaseCoefficientsBuffer;
        }

        /// <summary>
        /// Generate all the combination with favourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <returns>A list of load case coefficient</returns>
        private static List<List<LoadCaseCoefficient>> GetFavourableCombinations(List<LoadCase> loadCases, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSCombinationSets uLS, StandardEN1990.ImposedLoadCategory category)
        {
            List<List<LoadCaseCoefficient>> loadCaseCoefficients = new List<List<LoadCaseCoefficient>>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<LoadCaseCoefficient>();

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }

            List<LoadCase> list = new List<LoadCase>();
            foreach (LoadCase loadCase in loadCases)
                if ((loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.Prestress && loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SelfWeight && loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SuperImposedDeadLoad))
                    list.Add(loadCase);

            List<List<LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list, standardEN1990, limitState, uLS, category);
            for (int i = 0; i < randomList.Count(); i++)
            {
                List<LoadCaseCoefficient> tempList = new List<LoadCaseCoefficient>();
                tempList.AddRange(loadCaseCoefficientsBuffer);
                tempList.AddRange(randomList[i]);
                loadCaseCoefficients.Add(tempList);
            }

            return loadCaseCoefficients;
        }

        /// <summary>
        /// Generate all the combination with unfavourable coefficients
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <returns>A list of load case coefficient</returns>
        private static List<List<LoadCaseCoefficient>> GetUnfavourableCombinations(List<LoadCase> loadCases, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSCombinationSets uLS, StandardEN1990.ImposedLoadCategory category)
        {
            List<List<LoadCaseCoefficient>> loadCaseCoefficients = new List<List<LoadCaseCoefficient>>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<LoadCaseCoefficient>();

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }

            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS, category), loadCase);
                loadCaseCoefficientsBuffer.Add(lc);
            }

            List<LoadCase> list = new List<LoadCase>();
            foreach (LoadCase loadCase in loadCases)
                if ((loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.Prestress && loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SelfWeight && loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SuperImposedDeadLoad))
                    list.Add(loadCase);

            List<List<LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list, standardEN1990, limitState, uLS, category);
            for (int i = 0; i < randomList.Count(); i++)
            {
                List<LoadCaseCoefficient> tempList = new List<LoadCaseCoefficient>();
                tempList.AddRange(loadCaseCoefficientsBuffer);
                tempList.AddRange(randomList[i]);
                loadCaseCoefficients.Add(tempList);
            }

            return loadCaseCoefficients;
        }

        /// <summary>
        /// Generate all the combination for the variable loads
        /// </summary>
        /// <param name="list">List of load cases</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <returns>A list of list of load case coefficient</returns>
        private static List<List<LoadCaseCoefficient>> RandomizeVariableLoads(List<LoadCase> list, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSCombinationSets uLS, StandardEN1990.ImposedLoadCategory category)
        {
            List<List<LoadCaseCoefficient>> loadCaseCoefficients = new List<List<LoadCaseCoefficient>>();

            for (int i = 0; i < list.Count(); i++)
            {
                List<LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<LoadCaseCoefficient>();
                LoadCase loadCaseLead = list[i];
                LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(loadCaseLead, standardEN1990, limitState, uLS), loadCaseLead);
                loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);

                foreach (LoadCase loadCaseAccompanying in list)
                {
                    if (!loadCaseAccompanying.Equals(loadCaseLead))
                    {
                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(loadCaseAccompanying, standardEN1990, limitState, uLS, category), loadCaseAccompanying);
                        loadCaseCoefficientsBuffer.Add(loadCaseCoefficientAccompanying);
                    }
                }
                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
            }
            return loadCaseCoefficients;
        }

        /// <summary>
        /// Return the coefficient of unfavourable permanent actions
        /// </summary>
        /// <param name="loadCase">The load cases (only SelfWeight, SuperImposedDeadLoad and Prestress)</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <returns>The coefficient</returns>
        private static double GetCoefficientUnfavourablePermanentActions(LoadCase loadCase, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSCombinationSets uLS, StandardEN1990.ImposedLoadCategory category)
        {
            var loadCaseType = loadCase.GetLoadCaseType();
            double coef;

            if (loadCaseType == LoadCase.LoadCaseType.SelfWeight || loadCaseType == LoadCase.LoadCaseType.SuperImposedDeadLoad)
                coef = standardEN1990.GetGammaGUnfavourable(uLS, limitState);

            else if (loadCaseType == LoadCase.LoadCaseType.Prestress)
                coef = standardEN1990.GetGammaPUnfavourable(uLS, limitState, loadCase);

            else
                throw new Exception("Failed to set coefficient favourable for permanent actions");

            return coef;
        }

        /// <summary>
        /// Return the coefficient of favourable permanent actions
        /// </summary>
        /// <param name="loadCase">The load cases (only SelfWeight, SuperImposedDeadLoad and Prestress)</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <returns>The coefficient</returns>
        private static double GetCoefficientFavourablePermanentActions(LoadCase loadCase, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSCombinationSets uLS, StandardEN1990.ImposedLoadCategory category)
        {
            var loadCaseType = loadCase.GetLoadCaseType();
            double coef;

            if (loadCaseType == LoadCase.LoadCaseType.SelfWeight || loadCaseType == LoadCase.LoadCaseType.SuperImposedDeadLoad)
                coef = standardEN1990.GetGammaGFavourable(uLS, limitState);

            else if (loadCaseType == LoadCase.LoadCaseType.Prestress)
                coef = standardEN1990.GetGammaPFavourable(uLS, limitState, loadCase);

            else
                throw new Exception("Failed to set coefficient favourable for permanent actions");

            return coef;
        }

        /// <summary>
        /// Return the coefficient of leading variable actions
        /// </summary>
        /// <param name="loadCase">The load cases (only variable load are accepted)MO</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <returns>The coefficient</returns>
        private static double GetCoefficientLeadingVariableAction(LoadCase loadCase, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSCombinationSets uLS)
        {
            double gamma = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
            return gamma;
        }

        /// <summary>
        /// Return the coefficient of accompanying variable actions
        /// </summary>
        /// <param name="loadCase">the load cases (only variable load are accepted)</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <returns>The coefficient</returns>
        private static double GetCoefficientAccompanyingVariableAction(LoadCase loadCase, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSCombinationSets uLS, StandardEN1990.ImposedLoadCategory category)
        {
            double gammaQ = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
            double psi0 = standardEN1990.GetPsi0(category, loadCase);
            return gammaQ * psi0;
        }

        #endregion
    }
}
