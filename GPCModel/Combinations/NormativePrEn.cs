using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;

namespace GPC.Model.Combinations
{
    public class NormativePrEn
    {
        #region PUBLIC ENUMS

        public enum Annex
        {
            [Description("PrENSetA")] PrEnSetA,
            [Description("PrENSetB")] PrEnSetB,
            [Description("PrENSetC")] PrEnSetC,
        }

        public enum LimitState
        {
            [Description("Ultimate Limit State")] ULS,
            [Description("Serviceability limit states")] SLS,
        }

        public enum Category
        {
            [Description("Category A")] CategoryA,
            [Description("Category B")] CategoryB,
            [Description("Category C")] CategoryC,
            [Description("Category D")] CategoryD,
            [Description("Category E")] CategoryE,
            [Description("Category F")] CategoryF,
            [Description("Category G")] CategoryG,
            [Description("Category H")] CategoryH,
            [Description("Default")] Default,
        }

        #endregion


        #region VARIABLES

        private Annex? _annex;

        private LimitState? _limitState;

        private Category? _category;

        public Annex? GetAnnex() => _annex;

        public LimitState? GetLimitState() => _limitState;

        public Category? GetCategory() => _category;

        #endregion


        #region PUBLIC CONSTRUCTOR

        public NormativePrEn(LimitState limitState, Annex? annex, Category category = Category.Default)
            : this(limitState, annex, Guid.NewGuid(), category)
        {
        }

        public NormativePrEn(LimitState limitState, Annex? annex, Guid guid, Category category = Category.Default)
        {
            this._annex = annex;
            this._limitState = limitState;
            this._category = category;
        }

        #endregion


        #region PUBLIC METHOD

        internal double GetGammaGUnfavourable(LimitState limitState, Annex annex, LoadCase loadCase)
        {
            var loadCaseType = loadCase.GetLoadCaseType();
            double coef = -1;

            if (limitState == LimitState.ULS)
            {
                if (annex == Annex.PrEnSetA)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.SelfWeight:
                            coef = 1.05;
                            break;
                        case LoadCase.LoadCaseType.SuperImposedDeadLoad:
                            coef = 1.05;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetB)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.SelfWeight:
                            coef = 1.35;
                            break;
                        case LoadCase.LoadCaseType.SuperImposedDeadLoad:
                            coef = 1.35;
                            break;
                        case LoadCase.LoadCaseType.LiveLoad:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Wind:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Snow:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Maintenance:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Earthquake:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Temperature:
                            coef = 1.5; ;
                            break;
                        case LoadCase.LoadCaseType.ClimateSummer:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.ClimateWinter:
                            coef = 1.5;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetC)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.SelfWeight:
                            coef = 1.00;
                            break;
                        case LoadCase.LoadCaseType.SuperImposedDeadLoad:
                            coef = 1.00;
                            break;
                        case LoadCase.LoadCaseType.LiveLoad:
                            coef = 1.30;
                            break;
                        case LoadCase.LoadCaseType.Wind:
                            coef = 1.30; 
                            break;
                        case LoadCase.LoadCaseType.Snow:
                            coef = 1.30;
                            break;
                        case LoadCase.LoadCaseType.Maintenance:
                            coef = 1.30;
                            break;
                        case LoadCase.LoadCaseType.Earthquake:
                            coef = 1.30;
                            break;
                        case LoadCase.LoadCaseType.Temperature:
                            coef = 1.30;
                            break;
                        case LoadCase.LoadCaseType.ClimateSummer:
                            coef = 1.30;
                            break;
                        case LoadCase.LoadCaseType.ClimateWinter:
                            coef = 1.30;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new NotImplementedException("Not implemented Annex");
            }
            else if (limitState == LimitState.SLS)
            {
                if (annex == Annex.PrEnSetA)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetB)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetC)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new NotImplementedException("Not implemented Annex");
            }

            if (coef == -1)
                throw new Exception("Failed to set coefficient gamma");

            return coef;
        }

        internal double GetGammaGFavourable(LimitState limitState, Annex annex, LoadCase loadCase)
        {
            var loadCaseType = loadCase.GetLoadCaseType();
            double coef = -1;

            if (limitState == LimitState.ULS)
            {
                if (annex == Annex.PrEnSetA)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.SelfWeight:
                            coef = 0.95;
                            break;
                        case LoadCase.LoadCaseType.SuperImposedDeadLoad:
                            coef = 0.95;
                            break;
                        case LoadCase.LoadCaseType.LiveLoad:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Wind:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Snow:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Maintenance:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Earthquake:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Temperature:
                            coef = 0.0; 
                            break;
                        case LoadCase.LoadCaseType.ClimateSummer:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.ClimateWinter:
                            coef = 0.0;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetB)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.SelfWeight:
                            coef = 1.35;
                            break;
                        case LoadCase.LoadCaseType.SuperImposedDeadLoad:
                            coef = 1.35;
                            break;
                        case LoadCase.LoadCaseType.LiveLoad:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Wind:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Snow:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Maintenance:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Earthquake:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Temperature:
                            coef = 1.5; ;
                            break;
                        case LoadCase.LoadCaseType.ClimateSummer:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.ClimateWinter:
                            coef = 1.5;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetC)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.SelfWeight:
                            coef = 1.00;
                            break;
                        case LoadCase.LoadCaseType.SuperImposedDeadLoad:
                            coef = 1.00;
                            break;
                        case LoadCase.LoadCaseType.LiveLoad:
                            coef = 0.00;
                            break;
                        case LoadCase.LoadCaseType.Wind:
                            coef = 0.00;
                            break;
                        case LoadCase.LoadCaseType.Snow:
                            coef = 0.00;
                            break;
                        case LoadCase.LoadCaseType.Maintenance:
                            coef = 0.00;
                            break;
                        case LoadCase.LoadCaseType.Earthquake:
                            coef = 0.00;
                            break;
                        case LoadCase.LoadCaseType.Temperature:
                            coef = 0.00;
                            break;
                        case LoadCase.LoadCaseType.ClimateSummer:
                            coef = 0.00;
                            break;
                        case LoadCase.LoadCaseType.ClimateWinter:
                            coef = 0.00;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new NotImplementedException("Not implemented Annex");
            }
            else if (limitState == LimitState.SLS)
            {
                if (annex == Annex.PrEnSetA)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetB)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetC)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new NotImplementedException("Not implemented Annex");
            }

            if (coef == -1)
                throw new Exception("Failed to set coefficient gamma");

            return coef;
        }

        internal double GetGammaQUnfavourable(LimitState limitState, Annex annex, LoadCase loadCase)
        {
            var loadCaseType = loadCase.GetLoadCaseType();
            double coef = -1;

            if (limitState == LimitState.ULS)
            {
                if (annex == Annex.PrEnSetA)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.LiveLoad:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Wind:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Snow:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Maintenance:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Earthquake:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Temperature:
                            coef = 1.5; ;
                            break;
                        case LoadCase.LoadCaseType.ClimateSummer:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.ClimateWinter:
                            coef = 1.5;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetB)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.LiveLoad:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Wind:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Snow:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Maintenance:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Earthquake:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Temperature:
                            coef = 1.5; ;
                            break;
                        case LoadCase.LoadCaseType.ClimateSummer:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.ClimateWinter:
                            coef = 1.5;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetC)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.LiveLoad:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Wind:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Snow:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Maintenance:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Earthquake:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.Temperature:
                            coef = 1.5; ;
                            break;
                        case LoadCase.LoadCaseType.ClimateSummer:
                            coef = 1.5;
                            break;
                        case LoadCase.LoadCaseType.ClimateWinter:
                            coef = 1.5;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new NotImplementedException("Not implemented Annex");
            }
            else if (limitState == LimitState.SLS)
            {
                if (annex == Annex.PrEnSetA)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetB)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetC)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new NotImplementedException("Not implemented Annex");
            }

            if (coef == -1)
                throw new Exception("Failed to set coefficient gamma");

            return coef;
        }

        internal double GetGammaQFavourable(LimitState limitState, Annex annex, LoadCase loadCase)
        {
            var loadCaseType = loadCase.GetLoadCaseType();
            double coef = -1;

            if (limitState == LimitState.ULS)
            {
                if (annex == Annex.PrEnSetA)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.LiveLoad:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Wind:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Snow:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Maintenance:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Earthquake:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Temperature:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.ClimateSummer:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.ClimateWinter:
                            coef = 0.0;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetB)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.LiveLoad:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Wind:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Snow:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Maintenance:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Earthquake:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Temperature:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.ClimateSummer:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.ClimateWinter:
                            coef = 0.0;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetC)
                {
                    switch (loadCaseType)
                    {
                        case LoadCase.LoadCaseType.LiveLoad:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Wind:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Snow:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Maintenance:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Earthquake:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.Temperature:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.ClimateSummer:
                            coef = 0.0;
                            break;
                        case LoadCase.LoadCaseType.ClimateWinter:
                            coef = 0.0;
                            break;

                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new NotImplementedException("Not implemented Annex");
            }
            else if (limitState == LimitState.SLS)
            {
                if (annex == Annex.PrEnSetA)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetB)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else if (annex == Annex.PrEnSetC)
                {
                    switch (loadCaseType)
                    {
                        default:
                            throw new Exception("Not implemented coefficient for load case type");
                    }
                }
                else
                    throw new NotImplementedException("Not implemented Annex");
            }

            if (coef == -1)
                throw new Exception("Failed to set coefficient gamma");

            return coef;
        }

        internal double GetGammaPFavourable(LimitState limitState, Annex annex, LoadCase loadCase)
        {
            double coef = -1;

            if (limitState == LimitState.ULS)
            {
                switch (annex)
                {
                    case Annex.PrEnSetA:
                        coef = 1.0;
                        break;
                    case Annex.PrEnSetB:
                        coef = 1.0;
                        break;
                    case Annex.PrEnSetC:
                        coef = 1.0;
                        break;
                }
                
            }
            else if (limitState == LimitState.SLS)
            {
                switch (annex)
                {
                    case Annex.PrEnSetA:
                        coef = 1.0;
                        break;
                    case Annex.PrEnSetB:
                        coef = 1.0;
                        break;
                    case Annex.PrEnSetC:
                        coef = 1.0;
                        break;
                }
            }
            else
                throw new Exception("Failed to set the limit state");

            if (coef == -1)
                throw new Exception("Failed to set coefficient gammaP");

            return coef;
        }

        internal double GetGammaPUnfavourable(LimitState limitState, Annex annex, LoadCase loadCase)
        {
            double coef = -1;

            if (limitState == LimitState.ULS)
            {
                switch (annex)
                {
                    case Annex.PrEnSetA:
                        coef = 1.0;
                        break;
                    case Annex.PrEnSetB:
                        coef = 1.0;
                        break;
                    case Annex.PrEnSetC:
                        coef = 1.0;
                        break;
                }

            }
            else if (limitState == LimitState.SLS)
            {
                switch (annex)
                {
                    case Annex.PrEnSetA:
                        coef = 1.0;
                        break;
                    case Annex.PrEnSetB:
                        coef = 1.0;
                        break;
                    case Annex.PrEnSetC:
                        coef = 1.0;
                        break;
                }
            }
            else
                throw new Exception("Failed to set the limit state");

            if (coef == -1)
                throw new Exception("Failed to set coefficient gammaP");

            return coef;
        }

        internal double GetPsy(LimitState limitState, Annex annex, LoadCase loadCase, Category category = Category.Default)
        {
            var loadCaseType = loadCase.GetLoadCaseType();
            double[] coef = new double[3] { -1, -1, -1 };

            if (annex == Annex.PrEnSetA || annex == Annex.PrEnSetB || annex == Annex.PrEnSetC)
            {
                switch (loadCaseType)
                {
                    case LoadCase.LoadCaseType.SelfWeight:
                        throw new Exception("Don't exist coefficient for this load case type");
                    case LoadCase.LoadCaseType.SuperImposedDeadLoad:
                        throw new Exception("Don't exist coefficient for this load case type");
                    case LoadCase.LoadCaseType.Wind:
                        coef = new double[3] { 0.6, 0.2, 0.0 };
                        break;
                    case LoadCase.LoadCaseType.Snow:
                        coef = new double[3] { 0.7, 0.5, 0.2 };
                        break;
                    case LoadCase.LoadCaseType.Earthquake:
                        throw new Exception("Not implemented coefficient for load case type");
                    case LoadCase.LoadCaseType.Temperature:
                        coef = new double[3] { 0.6, 0.5, 0.0 };
                        break;
                    case LoadCase.LoadCaseType.ClimateSummer:
                        throw new Exception("Not implemented coefficient for load case type");
                    case LoadCase.LoadCaseType.ClimateWinter:
                        throw new Exception("Not implemented coefficient for load case type");

                    default:
                        throw new Exception("Not implemented coefficient for load case type");
                }

                if (loadCaseType == LoadCase.LoadCaseType.LiveLoad || loadCaseType == LoadCase.LoadCaseType.Maintenance)
                {
                    switch (category)
                    {
                        case Category.CategoryA:
                            coef = new double[3] { 0.7, 0.5, 0.3 };
                            break;
                        case Category.CategoryB:
                            coef = new double[3] { 0.7, 0.5, 0.3 };
                            break;
                        case Category.CategoryC:
                            coef = new double[3] { 0.7, 0.7, 0.6 };
                            break;
                        case Category.CategoryD:
                            coef = new double[3] { 0.7, 0.7, 0.6 };
                            break;
                        case Category.CategoryE:
                            coef = new double[3] { 1.0, 0.9, 0.8 };
                            break;
                        case Category.CategoryF:
                            coef = new double[3] { 0.7, 0.7, 0.6 };
                            break;
                        case Category.CategoryG:
                            coef = new double[3] { 0.7, 0.5, 0.3 };
                            break;
                        case Category.CategoryH:
                            coef = new double[3] { 0.0, 0.0, 0.0 };
                            break;
                    }
                }

            }

            else
                throw new NotImplementedException("Not implemented Annex");

            if (coef == new double[3] { -1, -1, -1 })
                throw new Exception("Failed to set coefficient psy");

            if (limitState == LimitState.ULS)
                return coef[0];
            else if (limitState == LimitState.SLS)
                return coef[1];
            else
                throw new Exception("Failed to set coefficient psy");
        }

        #endregion
    }
}
