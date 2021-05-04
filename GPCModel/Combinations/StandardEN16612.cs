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

        #region PUBLIC METHOD OVERRIDE

        /// <summary>
        /// Generate all the combination for the variable loads
        /// </summary>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="options">The combination generation options</param>
        /// <returns>A list of list of load case coefficient</returns>
        /// <exception cref="ArgumentException"> If there are any permanent load case in the <paramref name="loadCases"/></exception>
        protected override List<List<LoadCaseCoefficient>> RandomizeVariableLoads(LoadCaseBase[] loadCases, En1990CombinationsOptions options)
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


        /// <summary>
        /// Return the coefficient of unfavourable permanent actions
        /// </summary>
        /// <param name="loadCase">The load cases (only SelfWeight, SuperImposedDeadLoad and Prestress)</param>
        /// <param name="options"></param>
        /// <returns>The coefficient</returns>
        protected override double GetCoefficientUnfavourablePermanentActions(LoadCaseBase loadCase, En1990CombinationsOptions options)
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
        /// Return the coefficient of favourable permanent actions
        /// </summary>
        /// <param name="loadCase">The load cases (only SelfWeight, SuperImposedDeadLoad and Prestress)</param>
        /// <param name="options">The generation options</param>
        /// <returns>The coefficient</returns>
        protected override double GetCoefficientFavourablePermanentActions(LoadCaseBase loadCase, En1990CombinationsOptions options)
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
        /// Return the coefficient of leading variable actions
        /// </summary>
        /// <param name="loadCase">The load cases (only variable load are accepted)MO</param>
        /// <param name="options"></param>
        /// <returns>The coefficient</returns>
        protected override double GetCoefficientLeadingVariableAction(LoadCaseBase loadCase, En1990CombinationsOptions options)
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
        protected override double GetCoefficientAccompanyingVariableAction(LoadCaseBase loadCase, En1990CombinationsOptions options)
        {
            if (options.LimitState == LimitStates.UltimateEquilibrium || options.LimitState == LimitStates.UltimateFatigue
                || options.LimitState == LimitStates.UltimateGeotechnical || options.LimitState == LimitStates.UltimateStructural)
            {
                double gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
                double psi0 = loadCase is LoadCase ? GetPsi0(options.Category, (LoadCase)loadCase, options.HighAltitude) : GetPsi0((ClimateLoadCase)loadCase);
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
