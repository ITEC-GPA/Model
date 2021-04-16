using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;

namespace GPC.Model.Combinations
{
    [Serializable]
    internal abstract class CombinationsGenerator : Combination
    {
        #region VARIABLES

        protected NormativePrEn _normative;

        protected List<LoadCase> _loadCases;

        public string GetName() => _name;

        #endregion


        #region PUBLIC CONSTRUCTOR

        internal CombinationsGenerator(string name, NormativePrEn normative, List<LoadCase> loadcases)
            : this(name, normative, loadcases, Guid.NewGuid())
        {
            if (_loadCases.Count() == 0)
                throw new ArgumentException("Loadcases cannot be empty");
        }

        internal CombinationsGenerator(string name, NormativePrEn normative, List<LoadCase> loadCases, Guid guid)
            : this(name)
        {
            this._normative = normative;
            this._loadCases = loadCases;
        }

        internal CombinationsGenerator(string name) 
            : base(name)
        {
        }

        #endregion


        #region PUBLIC METHOD      

        protected List<LoadCaseCoefficient> GetFavourableCombinations()
        {
            List<LoadCaseCoefficient> loadCaseCoefficients = new List<LoadCaseCoefficient>();

            foreach(LoadCase loadCase in _loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase), loadCase);
                loadCaseCoefficients.Add(lc);
            }

            foreach (LoadCase loadCase in _loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase), loadCase);
                loadCaseCoefficients.Add(lc);
            }

            foreach (LoadCase loadCase in _loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase), loadCase);
                loadCaseCoefficients.Add(lc);
            }

            List<LoadCase> list = new List<LoadCase>();
            foreach (LoadCase loadCase in _loadCases)
            {
                if(loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.Prestress)
                {
                    if(loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SelfWeight)
                    {
                        if (loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SuperImposedDeadLoad)
                        {
                            list.Add(loadCase);
                        }
                    }
                }
            }

            List<LoadCaseCoefficient> randomList = RandomizeVariableLoads(list);
            loadCaseCoefficients.AddRange(randomList);
            
            return loadCaseCoefficients;
        }

        protected List<LoadCaseCoefficient> GetUnfavourableCombinations()
        {
            List<LoadCaseCoefficient> loadCaseCoefficients = new List<LoadCaseCoefficient>();

            foreach (LoadCase loadCase in _loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase), loadCase);
                loadCaseCoefficients.Add(lc);
            }

            foreach (LoadCase loadCase in _loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase), loadCase);
                loadCaseCoefficients.Add(lc);
            }

            foreach (LoadCase loadCase in _loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase), loadCase);
                loadCaseCoefficients.Add(lc);
            }

            List<LoadCase> list = new List<LoadCase>();
            foreach (LoadCase loadCase in _loadCases)
            {
                if (loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.Prestress)
                {
                    if (loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SelfWeight)
                    {
                        if (loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SuperImposedDeadLoad)
                        {
                            list.Add(loadCase);
                        }
                    }
                }
            }

            List<LoadCaseCoefficient> randomList = RandomizeVariableLoads(list);
            loadCaseCoefficients.AddRange(randomList);

            return loadCaseCoefficients;
        }

        #endregion


        #region PRIVATE METHOD

        private List<LoadCaseCoefficient> RandomizeVariableLoads(List<LoadCase> list)
        {
            List<LoadCaseCoefficient> loadCaseCoefficients = new List<LoadCaseCoefficient>();

            foreach(LoadCase loadCaseLead in list)
            {
                LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(loadCaseLead), loadCaseLead);
                loadCaseCoefficients.Add(loadCaseCoefficientLead);
                foreach (LoadCase loadCaseAccompanying in list)
                {
                    if(!loadCaseAccompanying.Equals(loadCaseLead))
                    {
                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientLeadingAccompanyingAction(loadCaseAccompanying), loadCaseAccompanying);
                        loadCaseCoefficients.Add(loadCaseCoefficientAccompanying);
                    }
                }
            }
            return loadCaseCoefficients;
        }

        private double GetCoefficientUnfavourablePermanentActions(LoadCase loadCase)
        {
            var loadCaseType = loadCase.GetLoadCaseType();
            double coef;

            if (loadCaseType == LoadCase.LoadCaseType.SelfWeight || loadCaseType == LoadCase.LoadCaseType.SuperImposedDeadLoad)
            {
                double gamma = _normative.GetGammaGUnfavourable((NormativePrEn.LimitState)_normative.GetLimitState(), (NormativePrEn.Annex)_normative.GetAnnex(), loadCase);
                coef = gamma;
            }

            else if (loadCaseType == LoadCase.LoadCaseType.Prestress)
            {
                double gamma = _normative.GetGammaPUnfavourable((NormativePrEn.LimitState)_normative.GetLimitState(), (NormativePrEn.Annex)_normative.GetAnnex(), loadCase);
                coef = gamma;
            }

            else
            {
                throw new Exception("Failed to set coefficient favourable for permanent actions");
            }

            return coef;
        }

        private double GetCoefficientFavourablePermanentActions(LoadCase loadCase)
        {
            var loadCaseType = loadCase.GetLoadCaseType();
            double coef;

            if (loadCaseType == LoadCase.LoadCaseType.SelfWeight || loadCaseType == LoadCase.LoadCaseType.SuperImposedDeadLoad)
            {
                double gamma = _normative.GetGammaGFavourable((NormativePrEn.LimitState)_normative.GetLimitState(), (NormativePrEn.Annex)_normative.GetAnnex(), loadCase);
                coef = gamma;
            }

            else if (loadCaseType == LoadCase.LoadCaseType.Prestress)
            {
                double gamma = _normative.GetGammaGFavourable((NormativePrEn.LimitState)_normative.GetLimitState(), (NormativePrEn.Annex)_normative.GetAnnex(), loadCase);
                coef = gamma;
            }

            else
            {
                throw new Exception("Failed to set coefficient favourable for permanent actions");
            }

            return coef;
        }

        private double GetCoefficientLeadingVariableAction(LoadCase loadCase)
        {
            double gamma = _normative.GetGammaQUnfavourable((NormativePrEn.LimitState)_normative.GetLimitState(), (NormativePrEn.Annex)_normative.GetAnnex(), loadCase);
            return gamma;
        }

        private double GetCoefficientLeadingAccompanyingAction(LoadCase loadCase)
        {
            double gammaQ = _normative.GetGammaQUnfavourable((NormativePrEn.LimitState)_normative.GetLimitState(), (NormativePrEn.Annex)_normative.GetAnnex(), loadCase);
            double psi0 = _normative.GetPsy((NormativePrEn.LimitState)_normative.GetLimitState(), (NormativePrEn.Annex)_normative.GetAnnex(), loadCase);
            return gammaQ * psi0;
        }

        #endregion

    }
}
