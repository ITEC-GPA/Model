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
        /// Create a combination with the normative StandardEN1990 set ad default. <paramref name="limitState"/> identify the limit state of the combination and 
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
        /// Create a combination with the normative StandardEN1990.
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
                if (lc.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaH)
                    haveCLimateSummer = true;
                if (lc.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaH)
                    haveCLimateWinter = true;
            }

            // aggiungo i SelfWeight
            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiungo i SuperImposedDeadLoad
            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto i Prestress
            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto il carico sismico se siamo in condizione sismica (come se fosse un permanente perchè non deve variare)
            if (limitState == StandardEN1990.LimitState.UltimateSeismic)
            {
                foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.Earthquake))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
            }

            // aggiunto i climate. summer e winter non possono stare insieme
            if (haveCLimateSummer == true && haveCLimateWinter == false)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == false && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == true && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBase.ToArray().ToList();

                foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer3.Add(lc);
                }
                foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaH))
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
                if (lc.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaH)
                    haveCLimateSummer = true;
                if (lc.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaH)
                    haveCLimateWinter = true;
            }

            // aggiungo i SelfWeight
            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiungo i SuperImposedDeadLoad
            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto i Prestress
            foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.Prestress))
            {
                LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                loadCaseCoefficientsBase.Add(lc);
            }
            // aggiunto il carico sismico se siamo in condizione sismica (come se fosse un permanente perchè non deve variare)
            if (limitState == StandardEN1990.LimitState.UltimateSeismic)
            {
                foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.Earthquake))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBase.Add(lc);
                }
            }

            // aggiunto i climate. summer e winter non possono stare insieme
            if (haveCLimateSummer == true && haveCLimateWinter == false)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == false && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer2.Add(lc);
                }
            }
            if (haveCLimateSummer == true && haveCLimateWinter == true)
            {
                loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBase.ToArray().ToList();
                loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBase.ToArray().ToList();

                foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaH))
                {
                    LoadCaseCoefficient lc = new LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, standardEN1990, limitState, uLS), loadCase);
                    loadCaseCoefficientsBuffer3.Add(lc);
                }
                foreach (LoadCase loadCase in loadCases.Where(i => i.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaH))
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
                if (loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.Prestress && loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SelfWeight &&
                    loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SuperImposedDeadLoad && loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateWinterDeltaH &&
                    loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateSummerDeltaH && loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.Earthquake)
                    list.Add(loadCase);

            List<List<LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list, standardEN1990, limitState, uLS, category, highAltitude);

            for (int i = 0; i < randomList.Count(); i++)
            {
                bool summerComboVariabili = false;
                bool winterComboVariabili = false;
                bool summerComboBase = false;
                bool winterComboBase = false;

                foreach (LoadCaseCoefficient loadCaseCoefficient in randomList[i])
                {
                    var loadCaseType = loadCaseCoefficient.LoadCase.GetLoadCaseType();
                    if ((loadCaseType == LoadCase.LoadCaseType.ClimateSummerDeltaT) || (loadCaseType == LoadCase.LoadCaseType.ClimateSummerDeltaP))
                        summerComboVariabili = true;
                    
                    if ((loadCaseType == LoadCase.LoadCaseType.ClimateWinterDeltaT) || (loadCaseType == LoadCase.LoadCaseType.ClimateWinterDeltaP))
                        winterComboVariabili = true;                    
                }

                foreach (List<LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
                {
                    foreach (LoadCaseCoefficient lcc in l)
                    {
                        var loadCaseType = lcc.LoadCase.GetLoadCaseType();
                        if ((loadCaseType == LoadCase.LoadCaseType.ClimateSummerDeltaT) || (loadCaseType == LoadCase.LoadCaseType.ClimateSummerDeltaP))
                            summerComboBase = true;

                        if ((loadCaseType == LoadCase.LoadCaseType.ClimateWinterDeltaT) || (loadCaseType == LoadCase.LoadCaseType.ClimateWinterDeltaP))
                            winterComboBase = true;
                    }

                    if ((summerComboVariabili && summerComboBase) || (winterComboVariabili && winterComboBase) || (!summerComboVariabili && !winterComboVariabili))
                    {
                        List<LoadCaseCoefficient> tempList = new List<LoadCaseCoefficient>();
                        tempList.AddRange(l);
                        tempList.AddRange(randomList[i]);
                        loadCaseCoefficients.Add(tempList);
                    }
                    else if ((summerComboVariabili && winterComboBase) || (winterComboVariabili && summerComboBase))
                    {
                         // non si possono mischiare le combinazioni
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
            List<List<LoadCaseCoefficient>> loadCaseCoefficientsBase = GetUnfavourableBasicCombinations(loadCases, standardEN1990, limitState, uLS);                       

            List<LoadCase> list = new List<LoadCase>();
            foreach (LoadCase loadCase in loadCases)
                if (loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.Prestress && loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SelfWeight && 
                    loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SuperImposedDeadLoad && loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateWinterDeltaH && 
                    loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateSummerDeltaH && loadCase.GetLoadCaseType() != LoadCase.LoadCaseType.Earthquake)
                    list.Add(loadCase);


            List<List<LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list, standardEN1990, limitState, uLS, category, highAltitude);
            for (int i = 0; i < randomList.Count(); i++)
            {
                foreach (List<LoadCaseCoefficient> l in loadCaseCoefficientsBase)
                {
                    List<LoadCaseCoefficient> tempList = new List<LoadCaseCoefficient>();
                    tempList.AddRange(l);
                    tempList.AddRange(randomList[i]);
                    loadCaseCoefficients.Add(tempList);
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
                if (lc.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaH && lc.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaH &&
                    lc.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight && lc.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad &&
                    lc.GetLoadCaseType() == LoadCase.LoadCaseType.Prestress && lc.GetLoadCaseType() == LoadCase.LoadCaseType.Earthquake)
                    throw new ArgumentException("Load must be Variable");
            }

            List<List<LoadCaseCoefficient>> loadCaseCoefficients = new List<List<LoadCaseCoefficient>>();
            HashSet<LoadCase.LoadCaseType> hash = new HashSet<LoadCase.LoadCaseType>();

            for (int i = 0; i < list.Count(); i++)
            {
                // crea un load lead, cerca tutti i carichi dello stesso tipo e li coefficienta alla stessa maniera.
                List<LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<LoadCaseCoefficient>();
                LoadCase loadCaseLead = list[i];
                var lctype = loadCaseLead.GetLoadCaseType();

                if (!hash.Contains((LoadCase.LoadCaseType)lctype))
                {
                    foreach (LoadCase lc in list.Where(j => j.GetLoadCaseType() == lctype))
                    {
                        LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(lc, standardEN1990, limitState, uLS, category, highAltitude), lc);
                        loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                    }
                    hash.Add((LoadCase.LoadCaseType)lctype);

                    #region CHECK CLIMATE LOAD (se ci sono carichi che devono essere considerati lead insieme a loadCaseLead)

                    if (lctype == LoadCase.LoadCaseType.ClimateSummerDeltaP)
                    {
                        foreach (LoadCase lc in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaT))
                        {
                            LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(lc, standardEN1990, limitState, uLS, category, highAltitude), lc);
                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                        }
                        hash.Add(LoadCase.LoadCaseType.ClimateSummerDeltaT);
                    }
                    if (lctype == LoadCase.LoadCaseType.ClimateSummerDeltaT)
                    {
                        foreach (LoadCase lc in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaP))
                        {
                            LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(lc, standardEN1990, limitState, uLS, category, highAltitude), lc);
                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                        }
                        hash.Add(LoadCase.LoadCaseType.ClimateSummerDeltaP);
                    }
                    if (lctype == LoadCase.LoadCaseType.ClimateWinterDeltaT)
                    {
                        foreach (LoadCase lc in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaP))
                        {
                            LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(lc, standardEN1990, limitState, uLS, category, highAltitude), lc);
                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                        }
                        hash.Add(LoadCase.LoadCaseType.ClimateWinterDeltaP);
                    }
                    if (lctype == LoadCase.LoadCaseType.ClimateWinterDeltaP)
                    {
                        foreach (LoadCase lc in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaT))
                        {
                            LoadCaseCoefficient loadCaseCoefficientLead = new LoadCaseCoefficient(GetCoefficientLeadingVariableAction(lc, standardEN1990, limitState, uLS, category, highAltitude), lc);
                            loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
                        }
                        hash.Add(LoadCase.LoadCaseType.ClimateWinterDeltaT);
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
                    HashSet<LoadCase.LoadCaseType> hashAcc = new HashSet<LoadCase.LoadCaseType>();
                    HashSet<LoadCase.LoadCaseType> hashtemp = new HashSet<LoadCase.LoadCaseType>();
                    bool haveWindPressure = false;
                    bool haveWindSuction = false;
                    bool haveClimateSummer = false;
                    bool haveClimateWinter = false;

                    // aggiunge tutti i carichi secondari che non siano wind pressure o wind suction o climatici. quei due vanno trattati a parte
                    foreach (LoadCase loadCaseAccompanying in list)
                    {
                        #region NORMAL LOAD ADD

                        if (!hashAcc.Contains((LoadCase.LoadCaseType)loadCaseAccompanying.GetLoadCaseType()))
                        {
                            if (!loadCaseAccompanying.GetLoadCaseType().Equals(loadCaseLead.GetLoadCaseType()))
                            {
                                var lcacctype = loadCaseAccompanying.GetLoadCaseType();
                                if (loadCaseAccompanying.GetLoadCaseType() != LoadCase.LoadCaseType.WindSuction && loadCaseAccompanying.GetLoadCaseType() != LoadCase.LoadCaseType.WindPressure &&
                                    loadCaseAccompanying.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateSummerDeltaP && loadCaseAccompanying.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateSummerDeltaT &&
                                    loadCaseAccompanying.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateWinterDeltaP && loadCaseAccompanying.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateWinterDeltaT)
                                {
                                    foreach (LoadCase lca in list.Where(j => j.GetLoadCaseType() == lcacctype))
                                    {

                                        LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                        loadCaseCoefficientsBuffer.Add(loadCaseCoefficientAccompanying);

                                    }
                                    hashAcc.Add((LoadCase.LoadCaseType)lcacctype);
                                }
                            }
                        }

                        #endregion

                        #region BOOL CHECK

                        if (loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.WindPressure)                        
                            haveWindPressure = true;
                        
                        if (loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.WindSuction)                        
                            haveWindSuction = true;

                        if (loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaP || loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaT)
                            haveClimateSummer = true;

                        if (loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaP || loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaT)
                            haveClimateWinter = true;

                        #endregion
                    }

                    #region WIND LOAD

                    // gestione dei carichi secondari quando sono presenti sia windpressure che windsuction
                    if ((loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.WindPressure && loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.WindSuction) && haveWindPressure == true && haveWindSuction == true)
                    {
                        foreach (LoadCase loadCaseAccompanying in list)
                        {
                            if (loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.WindPressure)
                            {
                                if (loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.WindSuction)
                                {
                                    if (!loadCaseAccompanying.GetLoadCaseType().Equals(loadCaseLead.GetLoadCaseType()))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseType)loadCaseAccompanying.GetLoadCaseType()))
                                        {
                                            foreach (LoadCase lca in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.WindSuction))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                                loadCaseCoefficientsWindSuction.Add(loadCaseCoefficientAccompanying);
                                            }
                                            hashAcc.Add((LoadCase.LoadCaseType)loadCaseAccompanying.GetLoadCaseType());
                                        }
                                    }
                                }
                            }

                            if (loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.WindSuction)
                            {
                                if (loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.WindPressure)
                                {
                                    if (!loadCaseAccompanying.GetLoadCaseType().Equals(loadCaseLead.GetLoadCaseType()))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseType)loadCaseAccompanying.GetLoadCaseType()))
                                        {
                                            foreach (LoadCase lca in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.WindPressure))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                                loadCaseCoefficientsWindPressure.Add(loadCaseCoefficientAccompanying);
                                            }
                                            hashAcc.Add((LoadCase.LoadCaseType)loadCaseAccompanying.GetLoadCaseType());
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
                            if (loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.WindPressure)
                            {
                                if (loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.WindSuction)
                                {
                                    if (!loadCaseAccompanying.GetLoadCaseType().Equals(loadCaseLead.GetLoadCaseType()))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseType)loadCaseAccompanying.GetLoadCaseType()))
                                        {
                                            foreach (LoadCase lca in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.WindSuction))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                                loadCaseCoefficientsWindSuction.Add(loadCaseCoefficientAccompanying);
                                            }
                                            hashAcc.Add((LoadCase.LoadCaseType)loadCaseAccompanying.GetLoadCaseType());
                                        }
                                    }
                                }
                            }
                        }

                        // gestione carichi secondari windpressure
                        foreach (LoadCase loadCaseAccompanying in list)
                        {
                            if (loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.WindSuction)
                            {
                                if (loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.WindPressure)
                                {
                                    if (!loadCaseAccompanying.GetLoadCaseType().Equals(loadCaseLead.GetLoadCaseType()))
                                    {
                                        if (!hashAcc.Contains((LoadCase.LoadCaseType)loadCaseAccompanying.GetLoadCaseType()))
                                        {
                                            foreach (LoadCase lca in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.WindPressure))
                                            {
                                                LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                                loadCaseCoefficientsWindPressure.Add(loadCaseCoefficientAccompanying);
                                            }
                                            hashAcc.Add((LoadCase.LoadCaseType)loadCaseAccompanying.GetLoadCaseType());
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
                        if (loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateWinterDeltaP && loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateWinterDeltaT &&
                            loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateSummerDeltaP && loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateSummerDeltaT)
                        {
                            if (loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaP || loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaT)
                            {
                                if (!loadCaseAccompanying.GetLoadCaseType().Equals(loadCaseLead.GetLoadCaseType()))
                                {
                                    if (!hashAcc.Contains((LoadCase.LoadCaseType)loadCaseAccompanying.GetLoadCaseType()))
                                    {
                                        foreach (LoadCase lca in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaP))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                            loadCaseCoefficientsSummer.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add(LoadCase.LoadCaseType.ClimateSummerDeltaP);
                                        foreach (LoadCase lca in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateSummerDeltaT))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                            loadCaseCoefficientsSummer.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add(LoadCase.LoadCaseType.ClimateSummerDeltaT);
                                    }
                                }
                            }
                        }
                    }

                    // gestione carichi secondari windpressure
                    foreach (LoadCase loadCaseAccompanying in list)
                    {
                        if (loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateSummerDeltaP && loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateSummerDeltaT &&
                            loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateWinterDeltaP && loadCaseLead.GetLoadCaseType() != LoadCase.LoadCaseType.ClimateWinterDeltaT)
                        {
                            if (loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaP || loadCaseAccompanying.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaT)
                            {
                                if (!loadCaseAccompanying.GetLoadCaseType().Equals(loadCaseLead.GetLoadCaseType()))
                                {
                                    if (!hashAcc.Contains((LoadCase.LoadCaseType)loadCaseAccompanying.GetLoadCaseType()))
                                    {
                                        foreach (LoadCase lca in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaP))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                            loadCaseCoefficientsWinter.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add(LoadCase.LoadCaseType.ClimateWinterDeltaP);
                                        foreach (LoadCase lca in list.Where(j => j.GetLoadCaseType() == LoadCase.LoadCaseType.ClimateWinterDeltaT))
                                        {
                                            LoadCaseCoefficient loadCaseCoefficientAccompanying = new LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(lca, standardEN1990, limitState, uLS, category, highAltitude), lca);
                                            loadCaseCoefficientsWinter.Add(loadCaseCoefficientAccompanying);
                                        }
                                        hashAcc.Add(LoadCase.LoadCaseType.ClimateWinterDeltaT);
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
            var loadCaseType = loadCase.GetLoadCaseType();
            double coef;

            if (loadCaseType == LoadCase.LoadCaseType.SelfWeight || loadCaseType == LoadCase.LoadCaseType.SuperImposedDeadLoad)
                coef = standardEN1990.GetGammaGUnfavourable(uLS, limitState);

            else if (loadCaseType == LoadCase.LoadCaseType.Earthquake)
                coef = standardEN1990.GetGammaGUnfavourable(uLS, limitState);

            else if (loadCaseType == LoadCase.LoadCaseType.ClimateWinterDeltaH || loadCaseType == LoadCase.LoadCaseType.ClimateSummerDeltaH)
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
        /// <returns>The coefficient</returns>
        private static double GetCoefficientFavourablePermanentActions(LoadCase loadCase, StandardEN1990 standardEN1990, StandardEN1990.LimitState limitState, StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS)
        {
            var loadCaseType = loadCase.GetLoadCaseType();
            double coef;

            if (loadCaseType == LoadCase.LoadCaseType.SelfWeight || loadCaseType == LoadCase.LoadCaseType.SuperImposedDeadLoad)
                coef = standardEN1990.GetGammaGFavourable(uLS, limitState);

            else if(loadCaseType == LoadCase.LoadCaseType.Earthquake)
                coef = standardEN1990.GetGammaGFavourable(uLS, limitState);

            else if (loadCaseType == LoadCase.LoadCaseType.ClimateWinterDeltaH || loadCaseType == LoadCase.LoadCaseType.ClimateSummerDeltaH)
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
