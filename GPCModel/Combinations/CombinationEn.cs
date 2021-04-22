using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.LoadCases;

namespace GPC.Model.Combinations
{
    public sealed class CombinationEn : Combination, IEquatable<CombinationEn>, ICloneable
    {
        #region VARIABLES

        private StandardEN1990 _standardEN1990;

        private StandardEN1990.LimitState _limitState;

        private StandardEN1990.ULSStructuralGeotechicalCombinationSets _uLSCombinationSets;

        private StandardEN1990.ImposedLoadCategory _imposedLoadCategory;



        public StandardEN1990.LimitState GetLimitState => _limitState;
        public StandardEN1990.ULSStructuralGeotechicalCombinationSets GetCombinationSets => _uLSCombinationSets;
        public StandardEN1990.ImposedLoadCategory GetImposedLoadCategory => _imposedLoadCategory;

        #endregion


        #region PUBLIC CONSTRUCTOR

        /// <summary>
        /// Create a combination with the normative StandardEN1990 set ad default. <paramref name="limitState"/> identify the limit state of the combination
        /// </summary>
        /// <param name="name">The identifying name of combination</param>
        /// <param name="limitState">The limit state of the combination</param>
        ///<inheritdoc cref="Combination"/>
        public CombinationEn(string name, StandardEN1990.LimitState limitState)
            : this(name, limitState, StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetB, StandardEN1990.ImposedLoadCategory.CategoryA, null)
        {
            this._standardEN1990 = new StandardEN1990();
        }

        /// <summary>
        /// Create a combination with the normative StandardEN1990 set ad default. <paramref name="limitState"/> identify the limit state of the combination 
        /// <paramref name="standard"/> identify the annex of the normative.
        /// </summary>
        /// <param name="name">The identifying name of combination</param>
        /// <param name="limitState">The limit state of the combination</param>
        /// <param name="standard">The annex of EN1990</param>
        ///<inheritdoc cref="Combination"/>
        public CombinationEn(string name, StandardEN1990.LimitState limitState, StandardEN1990 standard)
            : this(name, limitState, StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetB, StandardEN1990.ImposedLoadCategory.CategoryA, standard)
        {
            this._limitState = limitState;
            this._standardEN1990 = standard;
        }

        /// <summary>
        /// Create a combination with the normative StandardEN1990.
        /// </summary>
        /// <param name="name">The identifying name of combination</param>
        /// <param name="limitState">The limit state of the combination</param>
        /// <param name="uLSCombinationSets">The sets for structural and geotechnical ultimate limit states</param>
        /// <param name="category">The category of buildings for imposed loads</param>
        ///<inheritdoc cref="Combination"/>
        ///<inheritdoc cref="StandardEN1990"/>
        public CombinationEn(string name, StandardEN1990.LimitState limitState, StandardEN1990.ULSStructuralGeotechicalCombinationSets uLSCombinationSets, StandardEN1990.ImposedLoadCategory category)
            : this(name, limitState, uLSCombinationSets, category, null)
        {
            this._standardEN1990 = new StandardEN1990();
        }

        /// <summary>
        /// Create a combination with the normative <paramref name="standard"/>.
        /// </summary>
        /// <param name="name">The identifying name of combination</param>
        /// <param name="limitState">The limit state of the combination</param>
        /// <param name="uLSCombinationSets">The sets for structural and geotechical ultimate limit states</param>
        /// <param name="category">The category of buildings for imposed loads</param>
        /// <param name="standard">The annex of EN1990</param>
        ///<inheritdoc cref="Combination"/>
        ///<inheritdoc cref="StandardEN1990"/>
        public CombinationEn(string name, StandardEN1990.LimitState limitState, StandardEN1990.ULSStructuralGeotechicalCombinationSets uLSCombinationSets, StandardEN1990.ImposedLoadCategory category, StandardEN1990 standard)
            : base(name)
        {
            this._standardEN1990 = standard;
            this._limitState = limitState;
            this._uLSCombinationSets = uLSCombinationSets;
            this._imposedLoadCategory = category;
        }

        public CombinationEn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _standardEN1990 = (StandardEN1990)info.GetValue("CombinationType", typeof(StandardEN1990));
        }

        public CombinationEn(CombinationEn combinationEn)
            : base(combinationEn)
        {
            this._limitState = combinationEn._limitState;
        }

        #endregion


        #region PUBLIC OVERRIDE METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CombinationType", _standardEN1990);
        }

        public override bool IsUltimate() => (_limitState == StandardEN1990.LimitState.UltimateEquilibrium ||
                                             _limitState == StandardEN1990.LimitState.UltimateFatigue ||
                                             _limitState == StandardEN1990.LimitState.UltimateGeotechnical ||
                                             _limitState == StandardEN1990.LimitState.UltimateStructural) ?
                                             true : false;

        public override string ToString()
        {
            return base.ToString();
        }



        public override object Clone()
        {
            return new CombinationEn(this);
        }

        /// <summary>
        /// Create a new empty <see cref="CombinationEn"/> object. I.e. with the same properties except the <see cref="Combination.LoadCaseCoefficient"/> List that will be empty
        /// </summary>
        public override object CloneEmpty()
        {
            var cloned = new CombinationEn(this);
            cloned._coefficients.Clear();

            return cloned;
        }



        public override bool Equals(object obj)
        {
            return Equals(obj as CombinationEn);
        }

        public bool Equals(CombinationEn other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return other != null && base.Equals(other) && _limitState == other._limitState;
        }

        public override int GetHashCode()
        {
            var hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _limitState.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(CombinationEn obj1, CombinationEn obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(CombinationEn obj1, CombinationEn obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion


        #region PUBLIC METHOD   

        /// <summary>
        /// Generate the combinations of design with the <paramref name="standardEN1990"/> normative
        /// </summary>
        /// <param name="name">The name of the combinations set</param>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="category">The category of the imposed load</param>        /// 
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit structural or geotechnical</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>
        /// <returns>A list of combination</returns>
        public static List<CombinationEn> GenerateCombinations(string name, List<LoadCase> loadCases, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState,
                                                                StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA, 
                                                                StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetB, bool highAltitude = true)
        {
            HashSet<CombinationEn> combinations = new HashSet<CombinationEn>();

            List<List<LoadCaseCoefficient>> listFavourable = GetFavourableCombinations(loadCases, standardEN1990, limitState, uLS, category, highAltitude);
            for (int i = 0; i< listFavourable.Count(); i++)
            {
                CombinationEn combo = new CombinationEn(name, limitState, uLS, category, standardEN1990);

                for (int j = 0; j < listFavourable[i].Count(); j++)
                {
                    combo.AddLoadCaseCoefficient(listFavourable[i][j].LoadCase, listFavourable[i][j].Coefficient);                    
                }
                if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo))
                {
                    combinations.Add(combo);
                }
            }

            List<List<LoadCaseCoefficient>> listUnfavourable = GetUnfavourableCombinations(loadCases, standardEN1990, limitState, uLS, category, highAltitude);
            for (int i = 0; i < listUnfavourable.Count(); i++)
            {
                CombinationEn combo = new CombinationEn(name, limitState, uLS, category, standardEN1990);

                for (int j = 0; j < listUnfavourable[i].Count(); j++)
                {
                    combo.AddLoadCaseCoefficient(listUnfavourable[i][j].LoadCase, listUnfavourable[i][j].Coefficient);
                }
                if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo))
                {
                    combinations.Add(combo);
                }
            }          

            List<List<LoadCaseCoefficient>> listFavourableBase = GetFavourableBasicCombinations(loadCases, standardEN1990, limitState, uLS);
            for (int i = 0; i < listFavourableBase.Count(); i++)
            {
                CombinationEn comboBaseFav = new CombinationEn(name, limitState, uLS, category, standardEN1990);
                for (int j = 0; j < listFavourableBase[i].Count(); j++)
                {
                    comboBaseFav.AddLoadCaseCoefficient(listFavourableBase[i][j].LoadCase, listFavourableBase[i][j].Coefficient);
                }
                if (!CombinationCoefficientEqualityComparer.Equals(combinations, comboBaseFav))
                {
                    combinations.Add(comboBaseFav);
                }
            }            

            List<List<LoadCaseCoefficient>> listUnfavourableBase = GetUnfavourableBasicCombinations(loadCases, standardEN1990, limitState, uLS);
            for (int i = 0; i < listUnfavourableBase.Count(); i++)
            {
                CombinationEn comboBaseUnfav = new CombinationEn(name, limitState, uLS, category, standardEN1990);
                for (int j = 0; j < listUnfavourableBase[i].Count(); j++)
                {
                    comboBaseUnfav.AddLoadCaseCoefficient(listUnfavourableBase[i][j].LoadCase, listUnfavourableBase[i][j].Coefficient);
                }
                if (!CombinationCoefficientEqualityComparer.Equals(combinations, comboBaseUnfav))
                {
                    combinations.Add(comboBaseUnfav);
                }
            }

            return combinations.ToList();
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
        /// <returns>A list of load case coefficient</returns>
        private static List<List<LoadCaseCoefficient>> GetFavourableBasicCombinations(List<LoadCase> loadCases, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState,
                                                        StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS)
        {
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBase = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<LoadCaseCoefficient>();

            // controllo che ci siano i carichi climatici
            bool haveCLimateSummer = false;
            bool haveCLimateWinter = false;
            foreach (LoadCase lc in loadCases)
            {
                if (lc.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaH)
                    haveCLimateSummer = true;
                if (lc.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaH)
                    haveCLimateWinter = true;
            }

            // aggiungo i SelfWeight
            foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiungo i SuperImposedDeadLoad
            foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto i Prestress
            foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto il carico sismico se siamo in condizione sismica (come se fosse un permanente perchè non deve variare)
            if (limitState == StandardEN1990.LimitState.UltimateSeismic)
            {
                foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
            }

            // aggiunto i climate. summer e winter non possono stare insieme
            if (haveCLimateSummer == true && haveCLimateWinter == false)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == false && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == true && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBase.ToArray().ToList();

                foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer3.Add(lc);
                }
                foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
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
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <returns>A list of load case coefficient</returns>
        private static List<List<LoadCaseCoefficient>> GetUnfavourableBasicCombinations(List<LoadCase> loadCases, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, 
                                                        StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS)
        {
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBase = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<LoadCaseCoefficient>();

            // controllo che ci siano i carichi climatici
            bool haveCLimateSummer = false;
            bool haveCLimateWinter = false;
            foreach (LoadCase lc in loadCases)
            {
                if (lc.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaH)
                    haveCLimateSummer = true;
                if (lc.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaH)
                    haveCLimateWinter = true;
            }

            // aggiungo i SelfWeight
            foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiungo i SuperImposedDeadLoad
            foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto i Prestress
            foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto il carico sismico se siamo in condizione sismica (come se fosse un permanente perchè non deve variare)
            if (limitState == StandardEN1990.LimitState.UltimateSeismic)
            {
                foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
            }

            // aggiunto i climate. summer e winter non possono stare insieme
            if (haveCLimateSummer == true && haveCLimateWinter == false)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == false && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == true && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBase.ToArray().ToList();

                foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer3.Add(lc);
                }
                foreach (LoadCase loadCase in loadCases.Where(i => i.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
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
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>
        /// <returns>A list of load case coefficient</returns>
        private static List<List<LoadCaseCoefficient>> GetFavourableCombinations(List<LoadCase> loadCases, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, 
                                                        StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS, StandardEN1990.ImposedLoadCategory category, bool highAltitude = true)
        {
            List<List<LoadCaseCoefficient>> loadCaseCoefficients = new List<List<LoadCaseCoefficient>>();
            List<List<LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetFavourableBasicCombinations(loadCases, standardEN1990, limitState, uLS);

            List<LoadCase> list = new List<LoadCase>();
            foreach (LoadCase loadCase in loadCases)
                if (loadCase.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && loadCase.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
                    loadCase.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && loadCase.LoadCaseType != LoadCase.LoadCaseTypes.ClimateWinterDeltaH &&
                    loadCase.LoadCaseType != LoadCase.LoadCaseTypes.ClimateSummerDeltaH && loadCase.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake)
                    list.Add(loadCase);

            List<List<LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list, standardEN1990, limitState, uLS, category, highAltitude);

            for (int i = 0; i < randomList.Count(); i++)
            {
                bool summerComboVariabili = false;
                bool winterComboVariabili = false;

                foreach (LoadCaseCoefficient loadCaseCoefficient in randomList[i])
                {
                    var loadCaseType = loadCaseCoefficient.LoadCase.LoadCaseType;
                    if ((loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaT) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaP))
                        summerComboVariabili = true;
                    
                    if ((loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaT) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaP))
                        winterComboVariabili = true;                    
                }

                foreach (List<LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
                {
                    bool summerComboBase = false;
                    bool winterComboBase = false;

                    foreach (LoadCaseCoefficient lcc in l)
                    {
                        var loadCaseType = lcc.LoadCase.LoadCaseType;
                        if ((loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaT) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaP) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaH))
                            summerComboBase = true;

                        if ((loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaT) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaP) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaH))
                            winterComboBase = true;
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
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>
        /// <returns>A list of load case coefficient</returns>
        private static List<List<LoadCaseCoefficient>> GetUnfavourableCombinations(List<LoadCase> loadCases, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, 
                                                        StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS, StandardEN1990.ImposedLoadCategory category, bool highAltitude = true)
        {
            List<List<LoadCaseCoefficient>> loadCaseCoefficients = new List<List<LoadCaseCoefficient>>();
            List<List<LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetUnfavourableBasicCombinations(loadCases, standardEN1990, limitState, uLS);

            List<LoadCase> list = new List<LoadCase>();
            foreach (LoadCase loadCase in loadCases)
                if (loadCase.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && loadCase.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
                    loadCase.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && loadCase.LoadCaseType != LoadCase.LoadCaseTypes.ClimateWinterDeltaH &&
                    loadCase.LoadCaseType != LoadCase.LoadCaseTypes.ClimateSummerDeltaH && loadCase.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake)
                    list.Add(loadCase);

            List<List<LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list, standardEN1990, limitState, uLS, category, highAltitude);

            for (int i = 0; i < randomList.Count(); i++)
            {
                bool summerComboVariabili = false;
                bool winterComboVariabili = false;


                foreach (LoadCaseCoefficient loadCaseCoefficient in randomList[i])
                {
                    var loadCaseType = loadCaseCoefficient.LoadCase.LoadCaseType;
                    if ((loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaT) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaP))
                        summerComboVariabili = true;

                    if ((loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaT) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaP))
                        winterComboVariabili = true;
                }

                foreach (List<LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
                {
                    bool summerComboBase = false;
                    bool winterComboBase = false;

                    foreach (LoadCaseCoefficient lcc in l)
                    {
                        var loadCaseType = lcc.LoadCase.LoadCaseType;
                        if ((loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaT) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaP) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaH))
                            summerComboBase = true;

                        if ((loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaT) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaP) || (loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaH))
                            winterComboBase = true;
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
        /// Generate all the combination for the variable loads
        /// </summary>
        /// <param name="list">List of load cases</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>
        /// <returns>A list of list of load case coefficient</returns>
        private static List<List<LoadCaseCoefficient>> RandomizeVariableLoads(List<LoadCase> list, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, 
                                                        StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS, StandardEN1990.ImposedLoadCategory category, bool highAltitude = true)
        {
            // controllo che i carichi siano variabili
            foreach (LoadCase lc in list)
            {
                if (lc.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaH && lc.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaH &&
                    lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight && lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad &&
                    lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress && lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                    throw new ArgumentException("Load must be Variable");
            }

            List<List<LoadCaseCoefficient>> loadCaseCoefficients = new List<List<LoadCaseCoefficient>>();
            HashSet<LoadCase.LoadCaseTypes> hash = new HashSet<LoadCase.LoadCaseTypes>();

            for (int i = 0; i < list.Count(); i++)
            {
                // crea un load lead, cerca tutti i carichi dello stesso tipo e li coefficienta alla stessa maniera.
                List<LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<LoadCaseCoefficient>();
                LoadCase loadCaseLead = list[i];
                var lctype = loadCaseLead.LoadCaseType;

                if (!hash.Contains((LoadCase.LoadCaseTypes)lctype))
                {
                    foreach (LoadCase lc in list.Where(j => j.LoadCaseType == lctype))
                    {
                        LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(lc, standardEN1990, limitState, uLS, category, highAltitude), lc);
                        loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                    }
                    hash.Add((LoadCase.LoadCaseTypes)lctype);

                    #region CHECK CLIMATE LOAD (se ci sono carichi che devono essere considerati lead insieme a loadCaseLead)

                    if (lctype == LoadCase.LoadCaseTypes.ClimateSummerDeltaP)
                    {
                        foreach (LoadCase lc in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaT))
                        {
                            LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(lc, standardEN1990, limitState, uLS, category, highAltitude), lc);
                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                        }
                        hash.Add(LoadCase.LoadCaseTypes.ClimateSummerDeltaT);
                    }
                    if (lctype == LoadCase.LoadCaseTypes.ClimateSummerDeltaT)
                    {
                        foreach (LoadCase lc in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaP))
                        {
                            LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(lc, standardEN1990, limitState, uLS, category, highAltitude), lc);
                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                        }
                        hash.Add(LoadCase.LoadCaseTypes.ClimateSummerDeltaP);
                    }
                    if (lctype == LoadCase.LoadCaseTypes.ClimateWinterDeltaT)
                    {
                        foreach (LoadCase lc in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaP))
                        {
                            LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(lc, standardEN1990, limitState, uLS, category, highAltitude), lc);
                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                        }
                        hash.Add(LoadCase.LoadCaseTypes.ClimateWinterDeltaP);
                    }
                    if (lctype == LoadCase.LoadCaseTypes.ClimateWinterDeltaP)
                    {
                        foreach (LoadCase lc in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaT))
                        {
                            LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(lc, standardEN1990, limitState, uLS, category, highAltitude), lc);
                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                        }
                        hash.Add(LoadCase.LoadCaseTypes.ClimateWinterDeltaT);
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
                    HashSet<LoadCase.LoadCaseTypes> hashtemp = new HashSet<LoadCase.LoadCaseTypes>();
                    bool haveWindPressure = false;
                    bool haveWindSuction = false;
                    bool haveClimateSummer = false;
                    bool haveClimateWinter = false;

                    // aggiunge tutti i carichi secondari che non siano wind pressure o wind suction o climatici. quei due vanno trattati a parte
                    foreach (LoadCase loadCaseAccompanying in list)
                    {
                        #region NORMAL LOAD ADD

                        if (!hashAcc.Contains((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType))
                        {
                            if (!loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType))
                            {
                                var lcacctype = loadCaseAccompanying.LoadCaseType;
                                if (loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction && loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure &&
                                    loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.ClimateSummerDeltaP && loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.ClimateSummerDeltaT &&
                                    loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.ClimateWinterDeltaP && loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.ClimateWinterDeltaT)
                                {
                                    foreach (LoadCase lca in list.Where(j => j.LoadCaseType == lcacctype))
                                    {

                                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                        loadCaseCoefficientsBuffer.Add(loadCaseCoefficientAccompanying);

                                    }
                                    hashAcc.Add((LoadCase.LoadCaseTypes)lcacctype);
                                }
                            }
                        }

                        #endregion

                        #region BOOL CHECK

                        if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)                        
                            haveWindPressure = true;
                        
                        if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)                        
                            haveWindSuction = true;

                        if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaP || loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaT)
                            haveClimateSummer = true;

                        if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaP || loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaT)
                            haveClimateWinter = true;

                        #endregion
                    }

                    #region WIND LOAD

                    // gestione dei carichi secondari quando sono presenti sia windpressure che windsuction
                    if ((loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure && loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction) && haveWindPressure == true && haveWindSuction == true)
                    {
                        foreach (LoadCase loadCaseAccompanying in list)
                        {
                            if (loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
                            {
                                if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                                {
                                    if (!loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType))
                                        {
                                            foreach (LoadCase lca in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                                loadCaseCoefficientsWindSuction.Add(loadCaseCoefficientAccompanying);
                                            }
                                            hashAcc.Add((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType);
                                        }
                                    }
                                }
                            }

                            if (loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction)
                            {
                                if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                                {
                                    if (!loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType))
                                        {
                                            foreach (LoadCase lca in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
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
                        foreach (LoadCase loadCaseAccompanying in list)
                        {
                            if (loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
                            {
                                if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                                {
                                    if (!loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType))
                                        {
                                            foreach (LoadCase lca in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                                loadCaseCoefficientsWindSuction.Add(loadCaseCoefficientAccompanying);
                                            }
                                            hashAcc.Add((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType);
                                        }
                                    }
                                }
                            }
                        }

                        // gestione carichi secondari windpressure
                        foreach (LoadCase loadCaseAccompanying in list)
                        {
                            if (loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction)
                            {
                                if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                                {
                                    if (!loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType))
                                        {
                                            foreach (LoadCase lca in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
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
                    foreach (LoadCase loadCaseAccompanying in list)
                    {
                        if (loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.ClimateWinterDeltaP && loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.ClimateWinterDeltaT &&
                            loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.ClimateSummerDeltaP && loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.ClimateSummerDeltaT)
                        {
                            if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaP || loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaT)
                            {
                                if (!loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType))
                                {
                                    if (!hashAcc.Contains((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType))
                                    {
                                        foreach (LoadCase lca in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaP))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                            loadCaseCoefficientsSummer.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add(LoadCase.LoadCaseTypes.ClimateSummerDeltaP);
                                        foreach (LoadCase lca in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaT))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                            loadCaseCoefficientsSummer.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add(LoadCase.LoadCaseTypes.ClimateSummerDeltaT);
                                    }
                                }
                            }
                        }
                    }

                    // gestione carichi secondari windpressure
                    foreach (LoadCase loadCaseAccompanying in list)
                    {
                        if (loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.ClimateSummerDeltaP && loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.ClimateSummerDeltaT &&
                            loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.ClimateWinterDeltaP && loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.ClimateWinterDeltaT)
                        {
                            if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaP || loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaT)
                            {
                                if (!loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType))
                                {
                                    if (!hashAcc.Contains((LoadCase.LoadCaseTypes)loadCaseAccompanying.LoadCaseType))
                                    {
                                        foreach (LoadCase lca in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaP))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                            loadCaseCoefficientsWinter.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add(LoadCase.LoadCaseTypes.ClimateWinterDeltaP);
                                        foreach (LoadCase lca in list.Where(j => j.LoadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaT))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                            loadCaseCoefficientsWinter.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add(LoadCase.LoadCaseTypes.ClimateWinterDeltaT);
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

                            if(modWP && modS)                                
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            if(modWP && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
                            if (modWS && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                            if (modWS && modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);

                            if((modWS && modS == false) || (modWS == false && modS))
                                 loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);
                            else if((modWS && modW == false) || (modWS == false && modW))
                                 loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                            else if((modWP && modS == false) || (modWP == false && modS))
                                 loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            else if((modWP && modW == false) || (modWP == false && modW))
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
                                modS = true;                            }

                            if (loadCaseCoefficientsWindSuction.Count() != 0)
                            {
                                loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWindSuction);
                                modWS = true;
                            }

                            if(modWP && modS)
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
                            if(modWP && modW)
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
                            if(loadCaseCoefficientsWindSuction.Count() == 0 && loadCaseCoefficientsWindPressure.Count() == 0)
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

                            if(modWS && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                            if (modWS && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer5);
                            if(modW && modW)
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

                            if(modWS && modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            else if(!modWS || !modS)
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

                            if(modWP && modW)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer4);
                            if(modWP && modS)
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

                            if(modWP && modS)
                                loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
                            else if(!modWP || !modS)
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
                            if(modWP && modW)
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
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <returns>The coefficient</returns>
        private static double GetCoefficientUnfavourablePermanentActions(LoadCase loadCase, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS)
        {
            var loadCaseType = loadCase.LoadCaseType;
            double coef;

            if (loadCaseType == LoadCase.LoadCaseTypes.SelfWeight || loadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                coef = standardEN1990.GetGammaGUnfavourable(uLS, limitState);

            else if (loadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                coef = standardEN1990.GetGammaGUnfavourable(uLS, limitState);

            else if (loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaH || loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaH)
                coef = standardEN1990.GetGammaGUnfavourable(uLS, limitState);

            else if (loadCaseType == LoadCase.LoadCaseTypes.Prestress)
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
        /// <returns>The coefficient</returns>
        private static double GetCoefficientFavourablePermanentActions(LoadCase loadCase, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS)
        {
            var loadCaseType = loadCase.LoadCaseType;
            double coef;

            if (loadCaseType == LoadCase.LoadCaseTypes.SelfWeight || loadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                coef = standardEN1990.GetGammaGFavourable(uLS, limitState);

            else if(loadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                coef = standardEN1990.GetGammaGFavourable(uLS, limitState);

            else if (loadCaseType == LoadCase.LoadCaseTypes.ClimateWinterDeltaH || loadCaseType == LoadCase.LoadCaseTypes.ClimateSummerDeltaH)
                coef = standardEN1990.GetGammaGFavourable(uLS, limitState);

            else if (loadCaseType == LoadCase.LoadCaseTypes.Prestress)
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
        /// <param name="category">The category of the imposed load</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>
        /// <returns>The coefficient</returns>
        private static double GetCoefficientLeadingVariableAction(LoadCase loadCase, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, 
                                                        StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS, StandardEN1990.ImposedLoadCategory category, bool highAltitude = true)
        {
            if(limitState == StandardEN1990.LimitState.UltimateEquilibrium || limitState == StandardEN1990.LimitState.UltimateFatigue || limitState == StandardEN1990.LimitState.UltimateGeotechnical || limitState == StandardEN1990.LimitState.UltimateStructural)
            {
                double gamma = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
                return gamma;
            }
            else if (limitState == StandardEN1990.LimitState.UltimateSeismic)
            {
                double gammaQ = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
                double psi2 = standardEN1990.GetPsi2(category, loadCase, highAltitude);
                return gammaQ * psi2;
            }
            else if (limitState == StandardEN1990.LimitState.UltimateAccidental)
            {
                double gammaQ = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
                double psi1 = standardEN1990.GetPsi1(category, loadCase, highAltitude);
                return gammaQ * psi1;
            }
            else if (limitState == StandardEN1990.LimitState.ServiceabilityCharacteristic)
            {
                double gamma = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
                double psi2 = standardEN1990.GetPsi2(category, loadCase, highAltitude);
                return gamma * psi2;
            }
            else if(limitState == StandardEN1990.LimitState.ServiceabilityFrequent)
            {
                double gamma = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
                double psi1 = standardEN1990.GetPsi1(category, loadCase, highAltitude);
                return gamma * psi1; 
            }
            else if (limitState == StandardEN1990.LimitState.ServiceabilityQuasiPermanent)
            {
                double gamma = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
                double psi2 = standardEN1990.GetPsi2(category, loadCase, highAltitude);
                return gamma * psi2;
            }
            else
                throw new Exception("Failed to set the coefficient for leading variable actions");
        }

        /// <summary>
        /// Return the coefficient of accompanying variable actions
        /// </summary>
        /// <param name="loadCase">the load cases (only variable load are accepted)</param>
        /// <param name="standardEN1990">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <param name="uLS">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
        /// <param name="category">The category of the imposed load</param>
        /// <param name="highAltitude">If true, set the snow load with high altitude</param>
        /// <returns>The coefficient</returns>
        private static double GetCoefficientAccompanyingVariableAction(LoadCase loadCase, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, 
                                                        StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS, StandardEN1990.ImposedLoadCategory category, bool highAltitude = true)
        {
            if (limitState == StandardEN1990.LimitState.UltimateEquilibrium || limitState == StandardEN1990.LimitState.UltimateFatigue || limitState == StandardEN1990.LimitState.UltimateGeotechnical || limitState == StandardEN1990.LimitState.UltimateStructural)
            {
                double gammaQ = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
                double psi0 = standardEN1990.GetPsi0(category, loadCase, highAltitude);
                return gammaQ * psi0;
            }
            else if (limitState == StandardEN1990.LimitState.ServiceabilityCharacteristic)
            {
                double gammaQ = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
                double psi0 = standardEN1990.GetPsi0(category, loadCase, highAltitude);
                return gammaQ * psi0;
            }
            else if (limitState == StandardEN1990.LimitState.UltimateSeismic)
            {
                double gammaQ = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
                double psi2 = standardEN1990.GetPsi2(category, loadCase, highAltitude);
                return gammaQ * psi2;
            }
            else if (limitState == StandardEN1990.LimitState.UltimateAccidental)
            {
                double gammaQ = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
                double psi2 = standardEN1990.GetPsi2(category, loadCase, highAltitude);
                return gammaQ * psi2;
            }
            else if (limitState == StandardEN1990.LimitState.ServiceabilityFrequent || limitState == StandardEN1990.LimitState.ServiceabilityQuasiPermanent)
            {
                double gammaQ = standardEN1990.GetGammaQUnfavourable(uLS, limitState, loadCase);
                double psi2 = standardEN1990.GetPsi2(category, loadCase, highAltitude);
                return gammaQ * psi2;
            }
            else
                throw new Exception("Failed to set the coefficient for accompanying variable actions");
        }

        #endregion
    }
}
