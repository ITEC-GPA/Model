using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

#if riprogettare
namespace GPC.Model.Combinations
{
    public sealed class CombinationAsce : Combination, IEquatable<CombinationAsce>, ICloneable
    {
#region VARIABLES

        private StandardASCE16 _standardASCE16;

        private StandardASCE16.LimitStates? _combinationType;

#endregion

        public StandardASCE16.LimitStates? LimitState => _combinationType;


#region PUBLIC CONSTRUCTOR


        /// <param name="name">The identifying name of combination</param>
        public CombinationAsce(string name)
            : base(name)
        {

        }

        /// <summary>
        /// Create a combination. <paramref name="combinationType"/> identify the limit state of the combination 
        /// </summary>
        /// <param name="name">The identifying name of combination</param>
        /// <param name="combinationType">The limit state of the combination</param>
        public CombinationAsce(string name, StandardASCE16.LimitStates combinationType)
            : base(name)
        {
            this._combinationType = combinationType;
        }


        /// <summary>
        /// Create a combination with the normative <paramref name="standard"/>.
        /// </summary>
        /// <param name="name">The identifying name of combination</param>
        /// <param name="standard">The annex of ASCE7</param>
        /// <param name="combination">The limit state of the combination</param>
        public CombinationAsce(string name, StandardASCE16 standard, StandardASCE16.LimitStates combination)
            : base(name)
        {
            this._standardASCE16 = standard;
            this._combinationType = combination;
        }


        /// <param name="combination"></param>
        public CombinationAsce(CombinationAsce combination)
            : base(combination)
        {
            this._combinationType = combination._combinationType;
            this._standardASCE16 = combination._standardASCE16;
        }

        public CombinationAsce(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _combinationType = (StandardASCE16.LimitStates)info.GetValue("CombinationType", typeof(StandardASCE16.LimitStates));
        }

#endregion


#region PUBLIC OVERRIDE METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CombinationType", _combinationType);
        }

        public override bool IsUltimate() => _combinationType == StandardASCE16.LimitStates.LFRD ? true : false;

        public override string ToString()
        {
            return base.ToString();
        }


        public override object Clone()
        {
            return new CombinationAsce(this);
        }

        /// <summary>
        /// Create a new empty <see cref="CombinationAsce"/> object. I.e. with the same properties except the <see cref="Combination.LoadCaseCoefficient"/> List that will be empty
        /// </summary>
        public override object CloneEmpty()
        {
            var cloned = new CombinationAsce(this);
            cloned._coefficients.Clear();

            return cloned;
        }

        public override Combination Duplicate(string nameOverride)
        {
            var duplicated = (CombinationAsce)Clone();
            duplicated._name = nameOverride;

            return duplicated;
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as CombinationAsce);
        }

        public bool Equals(CombinationAsce other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;
            
            return other != null && base.Equals(other) && _combinationType == other._combinationType;
        }

        public override int GetHashCode()
        {
            var hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _combinationType.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(CombinationAsce obj1, CombinationAsce obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(CombinationAsce obj1, CombinationAsce obj2)
        {
            return !(obj1 == obj2);
        }

#endregion


#region PUBLIC METHOD  

        /// <summary>
        /// Generate the combinations of design with the <paramref name="standardASCE16"/> normative
        /// </summary>
        /// <param name="name">The name of the combinations set</param>
        /// <param name="loadCases">List of load cases</param>
        /// <param name="standardASCE16">The used normative</param>
        /// <param name="limitState">The limit state of combinations</param>
        /// <returns></returns>
        public static List<CombinationAsce> GenerateCombinations(string name, List<LoadCase> loadCases, StandardASCE16 standardASCE16, StandardASCE16.LimitStates limitState)
        {
            if (limitState == StandardASCE16.LimitStates.LFRD)
            {
                HashSet<CombinationAsce> combinations = new HashSet<CombinationAsce>(); 
                
                List<LoadCaseCoefficient> LFRDCombo1List = LFRDCombo1(loadCases, standardASCE16);
                CombinationAsce combo1 = new CombinationAsce(name, standardASCE16, limitState);
                for (int j = 0; j < LFRDCombo1List.Count; j++)
                {
                    combo1.AddLoadCaseCoefficient(LFRDCombo1List[j].LoadCase, LFRDCombo1List[j].Coefficient);
                }
                if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo1))
                {
                    combinations.Add(combo1);
                }

                List<List<LoadCaseCoefficient>> LFRDCombo2List = LFRDCombo2(loadCases, standardASCE16);
                for (int i = 0; i < LFRDCombo2List.Count; i++)
                {
                    CombinationAsce combo2 = new CombinationAsce(name, standardASCE16, limitState);
                    for (int j = 0; j < LFRDCombo2List[i].Count; j++)
                    {
                        combo2.AddLoadCaseCoefficient(LFRDCombo2List[i][j].LoadCase, LFRDCombo2List[i][j].Coefficient);
                    }
                    if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo2))
                    {
                        combinations.Add(combo2);
                    }
                }

                List<List<LoadCaseCoefficient>> LFRDCombo3List = LFRDCombo3(loadCases, standardASCE16);
                for (int i = 0; i < LFRDCombo3List.Count; i++)
                {
                    CombinationAsce combo3 = new CombinationAsce(name, standardASCE16, limitState);
                    for (int j = 0; j < LFRDCombo3List[i].Count; j++)
                    {
                        combo3.AddLoadCaseCoefficient(LFRDCombo3List[i][j].LoadCase, LFRDCombo3List[i][j].Coefficient);
                    }
                    if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo3))
                    {
                        combinations.Add(combo3);
                    }
                }

                List<List<LoadCaseCoefficient>> LFRDCombo4List = LFRDCombo4(loadCases, standardASCE16);
                for (int i = 0; i < LFRDCombo4List.Count; i++)
                {
                    CombinationAsce combo4 = new CombinationAsce(name, standardASCE16, limitState);
                    for (int j = 0; j < LFRDCombo4List[i].Count; j++)
                    {
                        combo4.AddLoadCaseCoefficient(LFRDCombo4List[i][j].LoadCase, LFRDCombo4List[i][j].Coefficient);
                    }
                    if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo4))
                    {
                        combinations.Add(combo4);
                    }
                }

                List<List<LoadCaseCoefficient>> LFRDCombo5List = LFRDCombo5(loadCases, standardASCE16);
                for (int i = 0; i < LFRDCombo5List.Count; i++)
                {
                    CombinationAsce combo5 = new CombinationAsce(name, standardASCE16, limitState);
                    for (int j = 0; j < LFRDCombo5List[i].Count; j++)
                    {
                        combo5.AddLoadCaseCoefficient(LFRDCombo5List[i][j].LoadCase, LFRDCombo5List[i][j].Coefficient);
                    }
                    if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo5))
                    {
                        combinations.Add(combo5);
                    }
                }

                List<LoadCaseCoefficient> LFRDCombo6List = LFRDCombo6(loadCases, standardASCE16);
                CombinationAsce combo6 = new CombinationAsce(name, standardASCE16, limitState);
                for (int j = 0; j < LFRDCombo6List.Count; j++)
                {
                    combo6.AddLoadCaseCoefficient(LFRDCombo6List[j].LoadCase, LFRDCombo6List[j].Coefficient);
                }
                if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo6))
                {
                    combinations.Add(combo6);
                }

                List<LoadCaseCoefficient> LFRDCombo7List = LFRDCombo7(loadCases, standardASCE16);
                CombinationAsce combo7 = new CombinationAsce(name, standardASCE16, limitState);
                for (int j = 0; j < LFRDCombo7List.Count; j++)
                {
                    combo7.AddLoadCaseCoefficient(LFRDCombo7List[j].LoadCase, LFRDCombo7List[j].Coefficient);
                }
                if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo7))
                {
                    combinations.Add(combo7);
                }

                return combinations.ToList();
            }
            else if (limitState == StandardASCE16.LimitStates.ASD)
            {
                HashSet<CombinationAsce> combinations = new HashSet<CombinationAsce>();

                List<LoadCaseCoefficient> ASDCombo1List = ASDCombo1(loadCases, standardASCE16);
                CombinationAsce combo1 = new CombinationAsce(name, standardASCE16, limitState);
                for (int j = 0; j < ASDCombo1List.Count; j++)
                {
                    combo1.AddLoadCaseCoefficient(ASDCombo1List[j].LoadCase, ASDCombo1List[j].Coefficient);
                }
                if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo1))
                {
                    combinations.Add(combo1);
                }

                List<List<LoadCaseCoefficient>> ASDCombo2List = ASDCombo2(loadCases, standardASCE16);
                for (int i = 0; i < ASDCombo2List.Count; i++)
                {
                    CombinationAsce combo2 = new CombinationAsce(name, standardASCE16, limitState);
                    for (int j = 0; j < ASDCombo2List[i].Count; j++)
                    {
                        combo2.AddLoadCaseCoefficient(ASDCombo2List[i][j].LoadCase, ASDCombo2List[i][j].Coefficient);
                    }
                    if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo2))
                    {
                        combinations.Add(combo2);
                    }
                }

                List<List<LoadCaseCoefficient>> ASDCombo3List = ASDCombo3(loadCases, standardASCE16);
                for (int i = 0; i < ASDCombo3List.Count; i++)
                {
                    CombinationAsce combo3 = new CombinationAsce(name, standardASCE16, limitState);
                    for (int j = 0; j < ASDCombo3List[i].Count; j++)
                    {
                        combo3.AddLoadCaseCoefficient(ASDCombo3List[i][j].LoadCase, ASDCombo3List[i][j].Coefficient);
                    }
                    if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo3))
                    {
                        combinations.Add(combo3);
                    }
                }

                List<List<LoadCaseCoefficient>> ASDCombo4List = ASDCombo4(loadCases, standardASCE16);
                for (int i = 0; i < ASDCombo4List.Count; i++)
                {
                    CombinationAsce combo4 = new CombinationAsce(name, standardASCE16, limitState);
                    for (int j = 0; j < ASDCombo4List[i].Count; j++)
                    {
                        combo4.AddLoadCaseCoefficient(ASDCombo4List[i][j].LoadCase, ASDCombo4List[i][j].Coefficient);
                    }
                    if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo4))
                    {
                        combinations.Add(combo4);
                    }
                }

                List<List<LoadCaseCoefficient>> ASDCombo5List = ASDCombo5(loadCases, standardASCE16);
                for (int i = 0; i < ASDCombo5List.Count; i++)
                {
                    CombinationAsce combo5 = new CombinationAsce(name, standardASCE16, limitState);
                    for (int j = 0; j < ASDCombo5List[i].Count; j++)
                    {
                        combo5.AddLoadCaseCoefficient(ASDCombo5List[i][j].LoadCase, ASDCombo5List[i][j].Coefficient);
                    }
                    if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo5))
                    {
                        combinations.Add(combo5);
                    }
                }

                List<List<LoadCaseCoefficient>> ASDCombo6List = ASDCombo6(loadCases, standardASCE16);
                for (int i = 0; i < ASDCombo6List.Count; i++)
                {
                    CombinationAsce combo6 = new CombinationAsce(name, standardASCE16, limitState);
                    for (int j = 0; j < ASDCombo6List[i].Count; j++)
                    {
                        combo6.AddLoadCaseCoefficient(ASDCombo6List[i][j].LoadCase, ASDCombo6List[i][j].Coefficient);
                    }
                    if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo6))
                    {
                        combinations.Add(combo6);
                    }
                }

                List<List<LoadCaseCoefficient>> ASDCombo7List = ASDCombo7(loadCases, standardASCE16);
                for (int i = 0; i < ASDCombo7List.Count; i++)
                {
                    CombinationAsce combo7 = new CombinationAsce(name, standardASCE16, limitState);
                    for (int j = 0; j < ASDCombo7List[i].Count; j++)
                    {
                        combo7.AddLoadCaseCoefficient(ASDCombo7List[i][j].LoadCase, ASDCombo7List[i][j].Coefficient);
                    }
                    if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo7))
                    {
                        combinations.Add(combo7);
                    }
                }

                List<LoadCaseCoefficient> ASDCombo8List = ASDCombo8(loadCases, standardASCE16);
                CombinationAsce combo8 = new CombinationAsce(name, standardASCE16, limitState);
                for (int j = 0; j < ASDCombo8List.Count; j++)
                {
                    combo8.AddLoadCaseCoefficient(ASDCombo8List[j].LoadCase, ASDCombo8List[j].Coefficient);
                }
                if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo8))
                {
                    combinations.Add(combo8);
                }

                List<LoadCaseCoefficient> ASDCombo9List = ASDCombo9(loadCases, standardASCE16);
                CombinationAsce combo9 = new CombinationAsce(name, standardASCE16, limitState);
                for (int j = 0; j < ASDCombo9List.Count; j++)
                {
                    combo9.AddLoadCaseCoefficient(ASDCombo9List[j].LoadCase, ASDCombo9List[j].Coefficient);
                }
                if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo9))
                {
                    combinations.Add(combo9);
                }

                List<LoadCaseCoefficient> ASDCombo10List = ASDCombo10(loadCases, standardASCE16);
                CombinationAsce combo10 = new CombinationAsce(name, standardASCE16, limitState);
                for (int j = 0; j < ASDCombo10List.Count; j++)
                {
                    combo10.AddLoadCaseCoefficient(ASDCombo10List[j].LoadCase, ASDCombo10List[j].Coefficient);
                }
                if (!CombinationCoefficientEqualityComparer.Equals(combinations, combo10))
                {
                    combinations.Add(combo10);
                }

                return combinations.ToList();
            }
            else
                throw new ArgumentException("Fail to generate");
        }

#endregion


#region PRIVATE METHOD

#region LFRD

        private static List<LoadCaseCoefficient> LFRDCombo1(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // LFRD combo 1 : 1.4 D

            List<LoadCaseCoefficient> loadCaseCoefficientSList = new List<LoadCaseCoefficient>();

            foreach(LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd1PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            return loadCaseCoefficientSList;
        }

        private static List<List<LoadCaseCoefficient>> LFRDCombo2(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // LFRD combo 2 : 1.2 D + 1.6 L + 0.5 (Lr or S or R)

            List<LoadCaseCoefficient> loadCaseCoefficientSListS = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListM = new List<LoadCaseCoefficient>();
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd2PermCoef1, l);
                    loadCaseCoefficientSListM.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd2VarCoef1, l);
                    loadCaseCoefficientSListM.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd2VarCoef2, l);
                    loadCaseCoefficientSListM.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListM);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd2PermCoef1, l);
                    loadCaseCoefficientSListS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd2VarCoef1, l);
                    loadCaseCoefficientSListS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd2VarCoef2, l);
                    loadCaseCoefficientSListS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListS);

            return outList;
        }

        private static List<List<LoadCaseCoefficient>> LFRDCombo3(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // LFRD combo 3 : 1.2 D + 1.6 L + 0.5 (Lr or S)

            List<LoadCaseCoefficient> loadCaseCoefficientSListLrL = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListLrW = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListSL = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListSW = new List<LoadCaseCoefficient>();
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3PermCoef1, l);
                    loadCaseCoefficientSListLrL.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3VarCoef1, l);
                    loadCaseCoefficientSListLrL.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3VarCoef2L, l);
                    loadCaseCoefficientSListLrL.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListLrL);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3PermCoef1, l);
                    loadCaseCoefficientSListLrW.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3VarCoef1, l);
                    loadCaseCoefficientSListLrW.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3VarCoef2L, l);
                    loadCaseCoefficientSListLrW.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListLrW);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3PermCoef1, l);
                    loadCaseCoefficientSListSL.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3VarCoef1, l);
                    loadCaseCoefficientSListSL.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3VarCoef2L, l);
                    loadCaseCoefficientSListSL.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListSL);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3PermCoef1, l);
                    loadCaseCoefficientSListSW.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3VarCoef1, l);
                    loadCaseCoefficientSListSW.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd3VarCoef2L, l);
                    loadCaseCoefficientSListSW.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListSW);

            return outList;
        }

        private static List<List<LoadCaseCoefficient>> LFRDCombo4(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // LFRD combo 4 : 1.2 D + 1.0 W + L + 0.5 (Lr or S)

            List<LoadCaseCoefficient> loadCaseCoefficientSListWpLr = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListWsLr = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListWpS = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListWsS = new List<LoadCaseCoefficient>();
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4PermCoef1, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef1, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWpLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4PermCoef1, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef1, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWsLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4PermCoef1, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef1, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWpS);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4PermCoef1, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef1, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWsS);

            return outList;
        }

        private static List<List<LoadCaseCoefficient>> LFRDCombo5(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // LFRD combo 4 : 1.2 D + 1.0 W + L + 0.5 (Lr or S)

            List<LoadCaseCoefficient> loadCaseCoefficientSListWp = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListWs = new List<LoadCaseCoefficient>();
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd5PermCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd5VarCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWp);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd5PermCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd5VarCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }                
            }
            outList.Add(loadCaseCoefficientSListWs);

            return outList;
        }

        private static List<LoadCaseCoefficient> LFRDCombo6(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // LFRD combo 1 : 1.4 D

            List<LoadCaseCoefficient> loadCaseCoefficientSList = new List<LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd6PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd6VarCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd6VarCoef2, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd6VarCoef3, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            return loadCaseCoefficientSList;
        }

        private static List<LoadCaseCoefficient> LFRDCombo7(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // LFRD combo 1 : 1.4 D

            List<LoadCaseCoefficient> loadCaseCoefficientSList = new List<LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd7PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd7VarCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }            

            return loadCaseCoefficientSList;
        }

#endregion


#region ASD

        private static List<LoadCaseCoefficient> ASDCombo1(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // ASD combo 1 : D

            List<LoadCaseCoefficient> loadCaseCoefficientSList = new List<LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd1PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            return loadCaseCoefficientSList;
        }

        private static List<List<LoadCaseCoefficient>> ASDCombo2(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // ASD combo 2 : D + L 

            List<LoadCaseCoefficient> loadCaseCoefficientSListS = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListM = new List<LoadCaseCoefficient>();
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd2PermCoef1, l);
                    loadCaseCoefficientSListM.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd2VarCoef1, l);
                    loadCaseCoefficientSListM.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListM);

            return outList;
        }

        private static List<List<LoadCaseCoefficient>> ASDCombo3(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // ASD combo 3 : D + 0.75 L + 0.75 (Lr or S)

            List<LoadCaseCoefficient> loadCaseCoefficientSListLr = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListS = new List<LoadCaseCoefficient>();
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd3PermCoef1, l);
                    loadCaseCoefficientSListLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd3VarCoef1, l);
                    loadCaseCoefficientSListLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd3PermCoef1, l);
                    loadCaseCoefficientSListS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd3VarCoef1, l);
                    loadCaseCoefficientSListS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListS);            

            return outList;
        }

        private static List<List<LoadCaseCoefficient>> ASDCombo4(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // ASD combo 4 : D + 0.75 L + 0.75 (Lr or S)

            List<LoadCaseCoefficient> loadCaseCoefficientSListLLr = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListLS = new List<LoadCaseCoefficient>();
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd4PermCoef1, l);
                    loadCaseCoefficientSListLLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd4VarCoef1, l);
                    loadCaseCoefficientSListLLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd4VarCoef2, l);
                    loadCaseCoefficientSListLLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListLLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd4PermCoef1, l);
                    loadCaseCoefficientSListLS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd4VarCoef1, l);
                    loadCaseCoefficientSListLS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd4VarCoef2, l);
                    loadCaseCoefficientSListLS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListLS);
            
            return outList;
        }

        private static List<List<LoadCaseCoefficient>> ASDCombo5(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // ASD combo 5 : 1 D + 0.6 W

            List<LoadCaseCoefficient> loadCaseCoefficientSListWp = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListWs = new List<LoadCaseCoefficient>();
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd5PermCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd5VarCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWp);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Lfrd5PermCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd5VarCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWs);

            return outList;
        }

        private static List<List<LoadCaseCoefficient>> ASDCombo6(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // ASD combo 6 : D + 0.75L + 0.75*0.6W + 0.75(Lr or S or R)

            List<LoadCaseCoefficient> loadCaseCoefficientSListWpLr = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListWpS = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListWsLr = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListWsS = new List<LoadCaseCoefficient>();
            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6PermCoef1, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef1, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef2, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef3, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWpLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6PermCoef1, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef1, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef2, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef3, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWsLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6PermCoef1, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef1, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef2, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef3, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWpS);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6PermCoef1, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef1, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef2, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd6VarCoef3, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWsS);

            return outList;
        }

        private static List<List<LoadCaseCoefficient>> ASDCombo7(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // ASD combo 7 : 0.6 D + 0.6 W

            List<LoadCaseCoefficient> loadCaseCoefficientSListWp = new List<LoadCaseCoefficient>();
            List<LoadCaseCoefficient> loadCaseCoefficientSListWs = new List<LoadCaseCoefficient>();

            List<List<LoadCaseCoefficient>> outList = new List<List<LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd7PermCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd7VarCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWp);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd7PermCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd7VarCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWs);

            return outList;
        }

        private static List<LoadCaseCoefficient> ASDCombo8(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // ASD combo 8 : D + 0.7 Ev + 0.7 Eh

            List<LoadCaseCoefficient> loadCaseCoefficientSList = new List<LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd8PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd8VarCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }
            return loadCaseCoefficientSList;
        }

        private static List<LoadCaseCoefficient> ASDCombo9(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // ASD combo 9 : D + 0.525 Ev + 0.525 Eh + 0.75 L + 0.75 S

            List<LoadCaseCoefficient> loadCaseCoefficientSList = new List<LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd9PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd9VarCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd9VarCoef3, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd9VarCoef4, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            return loadCaseCoefficientSList;
        }

        private static List<LoadCaseCoefficient> ASDCombo10(List<LoadCase> loadCases, StandardASCE16 standardASCE16)
        {
            // ASD combo 10 : 0.6 D - 0.7 Ev + 0.7 Eh

            List<LoadCaseCoefficient> loadCaseCoefficientSList = new List<LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd10PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                {
                    LoadCaseCoefficient loadCaseCoefficient = new LoadCaseCoefficient(standardASCE16.Asd10VarCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            return loadCaseCoefficientSList;
        }



#endregion

#endregion


    }
}
#endif