using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the ASCE7-16 Standard
    /// </summary>
    /// <remarks>Reference: ASCE7-16</remarks>
    [Serializable]
    public class StandardASCE16 : Standard, Standard.ICombinationsGenerator
    {
        #region PUBLIC ENUMS        

        /// <summary>
        /// The limit states. Reference: ASCE7-16
        /// </summary>
        public enum LimitStates
        {
            LFRD,
            ASD
        }

        #endregion

        #region VARIABLES

        // LFRD
        private double _lfrd1PermCoef1;

        private double _lfrd2PermCoef1;
        private double _lfrd2VarCoef1;
        private double _lfrd2VarCoef2;

        private double _lfrd3PermCoef1;
        private double _lfrd3VarCoef1;
        private double _lfrd3VarCoef2L;
        private double _lfrd3VarCoef2W;

        private double _lfrd4PermCoef1;
        private double _lfrd4VarCoef1;
        private double _lfrd4VarCoef2;
        private double _lfrd4VarCoef3;

        private double _lfrd5PermCoef1;
        private double _lfrd5VarCoef1;

        private double _lfrd6PermCoef1;
        private double _lfrd6VarCoef1;
        private double _lfrd6VarCoef2;
        private double _lfrd6VarCoef3;

        private double _lfrd7PermCoef1;
        private double _lfrd7VarCoef1;
        private double _lfrd7VarCoef2;


        // ASD
        private double _asd1PermCoef1;

        private double _asd2PermCoef1;
        private double _asd2VarCoef1;

        private double _asd3PermCoef1;
        private double _asd3VarCoef1;

        private double _asd4PermCoef1;
        private double _asd4VarCoef1;
        private double _asd4VarCoef2;

        private double _asd5PermCoef1;
        private double _asd5VarCoef1;

        private double _asd6PermCoef1;
        private double _asd6VarCoef1;
        private double _asd6VarCoef2;
        private double _asd6VarCoef3;

        private double _asd7PermCoef1;
        private double _asd7VarCoef1;

        private double _asd8PermCoef1;
        private double _asd8VarCoef1;
        private double _asd8VarCoef2;

        private double _asd9PermCoef1;
        private double _asd9VarCoef1;
        private double _asd9VarCoef2;
        private double _asd9VarCoef3;
        private double _asd9VarCoef4;

        private double _asd10PermCoef1;
        private double _asd10VarCoef1;
        private double _asd10VarCoef2;

        #endregion

        #region PROPERTIES

        // LFRD
        // combo1
        public double Lfrd1PermCoef1 { get => _lfrd1PermCoef1; set => _lfrd1PermCoef1 = value; }
        // combo2
        public double Lfrd2PermCoef1 { get => _lfrd2PermCoef1; set => _lfrd2PermCoef1 = value; }
        public double Lfrd2VarCoef1 { get => _lfrd2VarCoef1; set => _lfrd2VarCoef1 = value; }
        public double Lfrd2VarCoef2 { get => _lfrd2VarCoef2; set => _lfrd2VarCoef2 = value; }
        // combo3                
        public double Lfrd3PermCoef1 { get => _lfrd3PermCoef1; set => _lfrd3PermCoef1 = value; }
        public double Lfrd3VarCoef1 { get => _lfrd3VarCoef1; set => _lfrd3VarCoef1 = value; }
        public double Lfrd3VarCoef2L { get => _lfrd3VarCoef2L; set => _lfrd3VarCoef2L = value; }
        public double Lfrd3VarCoef2W { get => _lfrd3VarCoef2W; set => _lfrd3VarCoef2W = value; }
        // combo4      
        public double Lfrd4PermCoef1 { get => _lfrd4PermCoef1; set => _lfrd4PermCoef1 = value; }
        public double Lfrd4VarCoef1 { get => _lfrd4VarCoef1; set => _lfrd4VarCoef1 = value; }
        public double Lfrd4VarCoef2 { get => _lfrd4VarCoef2; set => _lfrd4VarCoef2 = value; }
        public double Lfrd4VarCoef3 { get => _lfrd4VarCoef3; set => _lfrd4VarCoef3 = value; }
        // combo5                          
        public double Lfrd5PermCoef1 { get => _lfrd5PermCoef1; set => _lfrd5PermCoef1 = value; }
        public double Lfrd5VarCoef1 { get => _lfrd5VarCoef1; set => _lfrd5VarCoef1 = value; }
        // combo6         
        public double Lfrd6PermCoef1 { get => _lfrd6PermCoef1; set => _lfrd6PermCoef1 = value; }
        public double Lfrd6VarCoef1 { get => _lfrd6VarCoef1; set => _lfrd6VarCoef1 = value; }
        public double Lfrd6VarCoef2 { get => _lfrd6VarCoef2; set => _lfrd6VarCoef2 = value; }
        public double Lfrd6VarCoef3 { get => _lfrd6VarCoef3; set => _lfrd6VarCoef3 = value; }
        // combo7            
        public double Lfrd7PermCoef1 { get => _lfrd7PermCoef1; set => _lfrd7PermCoef1 = value; }
        public double Lfrd7VarCoef1 { get => _lfrd7VarCoef1; set => _lfrd7VarCoef1 = value; }
        public double Lfrd7VarCoef2 { get => _lfrd7VarCoef2; set => _lfrd7VarCoef2 = value; }

        // ASD
        // combo1
        public double Asd1PermCoef1 { get => _asd1PermCoef1; set => _asd1PermCoef1 = value; }
        // combo2              
        public double Asd2PermCoef1 { get => _asd2PermCoef1; set => _asd2PermCoef1 = value; }
        public double Asd2VarCoef1 { get => _asd2VarCoef1; set => _asd2VarCoef1 = value; }
        // combo3     
        public double Asd3PermCoef1 { get => _asd3PermCoef1; set => _asd3PermCoef1 = value; }
        public double Asd3VarCoef1 { get => _asd3VarCoef1; set => _asd3VarCoef1 = value; }
        // combo4     
        public double Asd4PermCoef1 { get => _asd4PermCoef1; set => _asd4PermCoef1 = value; }
        public double Asd4VarCoef1 { get => _asd4VarCoef1; set => _asd4VarCoef1 = value; }
        public double Asd4VarCoef2 { get => _asd4VarCoef2; set => _asd4VarCoef2 = value; }
        // combo5     
        public double Asd5PermCoef1 { get => _asd5PermCoef1; set => _asd5PermCoef1 = value; }
        public double Asd5VarCoef1 { get => _asd5VarCoef1; set => _asd5VarCoef1 = value; }
        // combo6     
        public double Asd6PermCoef1 { get => _asd6PermCoef1; set => _asd6PermCoef1 = value; }
        public double Asd6VarCoef1 { get => _asd6VarCoef1; set => _asd6VarCoef1 = value; }
        public double Asd6VarCoef2 { get => _asd6VarCoef2; set => _asd6VarCoef2 = value; }
        public double Asd6VarCoef3 { get => _asd6VarCoef3; set => _asd6VarCoef3 = value; }
        // combo7     
        public double Asd7PermCoef1 { get => _asd7PermCoef1; set => _asd7PermCoef1 = value; }
        public double Asd7VarCoef1 { get => _asd7VarCoef1; set => _asd7VarCoef1 = value; }
        // combo8     
        public double Asd8PermCoef1 { get => _asd8PermCoef1; set => _asd8PermCoef1 = value; }
        public double Asd8VarCoef1 { get => _asd8VarCoef1; set => _asd8VarCoef1 = value; }
        public double Asd8VarCoef2 { get => _asd8VarCoef2; set => _asd8VarCoef2 = value; }
        // combo9     
        public double Asd9PermCoef1 { get => _asd9PermCoef1; set => _asd9PermCoef1 = value; }
        public double Asd9VarCoef1 { get => _asd9VarCoef1; set => _asd9VarCoef1 = value; }
        public double Asd9VarCoef2 { get => _asd9VarCoef2; set => _asd9VarCoef2 = value; }
        public double Asd9VarCoef3 { get => _asd9VarCoef3; set => _asd9VarCoef3 = value; }
        public double Asd9VarCoef4 { get => _asd9VarCoef4; set => _asd9VarCoef4 = value; }
        // combo10     
        public double Asd10PermCoef1 { get => _asd10PermCoef1; set => _asd10PermCoef1 = value; }
        public double Asd10VarCoef1 { get => _asd10VarCoef1; set => _asd10VarCoef1 = value; }
        public double Asd10VarCoef2 { get => _asd10VarCoef2; set => _asd10VarCoef2 = value; }

        #endregion

        #region PUBLIC CONSTRUCTOR

        public StandardASCE16(string name = "ASCE7-16", string remarks = "Minimum Design Loads for Buildings and Other Structures: ASCE Standard ASCE/SEI 7-16")
            : base(name, remarks)
        {
            // LFRD
            _lfrd1PermCoef1 = 1.4;

            _lfrd2PermCoef1 = 1.2;
            _lfrd2VarCoef1 = 1.6;
            _lfrd2VarCoef2 = 0.5;

            _lfrd3PermCoef1 = 1.2;
            _lfrd3VarCoef1 = 1.6;
            _lfrd3VarCoef2L = 1;
            _lfrd3VarCoef2W = 0.5;

            _lfrd4PermCoef1 = 1.2;
            _lfrd4VarCoef1 = 1.0;
            _lfrd4VarCoef2 = 1.0;
            _lfrd4VarCoef3 = 0.5;

            _lfrd5PermCoef1 = 0.9;
            _lfrd5VarCoef1 = 1.0;

            _lfrd6PermCoef1 = 1.2;
            _lfrd6VarCoef1 = 1.0;
            _lfrd6VarCoef2 = 1.0;
            _lfrd6VarCoef3 = 0.2;

            _lfrd7PermCoef1 = 0.9;
            _lfrd7VarCoef1 = -1;
            _lfrd7VarCoef2 = +1;

            // ASD
            _asd1PermCoef1 = 1.0;

            _asd2PermCoef1 = 1.0;
            _asd2VarCoef1 = 1.0;

            _asd3PermCoef1 = 1.0;
            _asd3VarCoef1 = 1.0;

            _asd4PermCoef1 = 1.0;
            _asd4VarCoef1 = 0.75;
            _asd4VarCoef2 = 0.75;

            _asd5PermCoef1 = 1;
            _asd5VarCoef1 = 0.6;

            _asd6PermCoef1 = 1.0;
            _asd6VarCoef1 = 0.75;
            _asd6VarCoef2 = 0.75 * 0.6;
            _asd6VarCoef3 = 0.75;

            _asd7PermCoef1 = 0.6;
            _asd7VarCoef1 = 0.6;

            _asd8PermCoef1 = 1;
            _asd8VarCoef1 = 0.7;
            _asd8VarCoef2 = 0.7;

            _asd9PermCoef1 = 1.0;
            _asd9VarCoef1 = 0.525;
            _asd9VarCoef2 = 0.525;
            _asd9VarCoef3 = 0.75;
            _asd9VarCoef4 = 0.75;

            _asd10PermCoef1 = 0.6;
            _asd10VarCoef1 = -0.7;
            _asd10VarCoef2 = 0.7;
        }

        #endregion

        #region COMBINATIONS OPTIONS

        public class ASCE16CombinationsOptions : CombinationsOptions
        {
            public LimitStates LimitState { get; set; }

            public ASCE16CombinationsOptions(LimitStates limitState)
            {
                LimitState = limitState;
            }

            public override bool Equals(object obj)
            {
                if (obj is null)
                    return false;

                if (ReferenceEquals(this, obj))
                    return true;

                ASCE16CombinationsOptions objCasted = obj as ASCE16CombinationsOptions;

                return !(objCasted is null) && objCasted.LimitState.Equals(LimitState);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = 23;
                    hashCode = 17 * hashCode + LimitState.GetHashCode();

                    return hashCode;
                }
            }
        }

        #endregion

        #region COMBINATIONS GENERATION

        #region PUBLIC METHOD

        public UniqueNameCollection<Combination> CreateCombinations(LoadCaseBase[] loadCases, CombinationsOptions options, string name = "cmb")
        {
            if (loadCases.Any(i => i is ClimateLoadCase))
            {
                throw new ArgumentException("Load cases must not be a climate load");
            }

            int idProg = 1;
            Combination.CombinationCoefficientEqualityComparer equalityComparer = new Combination.CombinationCoefficientEqualityComparer();
            HashSet<Combination> combinationsHashSet = new HashSet<Combination>(equalityComparer);
            UniqueNameCollection<Combination> combinations = new UniqueNameCollection<Combination>();

            if (((ASCE16CombinationsOptions)options).LimitState == StandardASCE16.LimitStates.LFRD)
            {
                List<Combination.LoadCaseCoefficient> LFRDCombo1List = LFRDCombo1(loadCases.Cast<LoadCase>().ToList());
                Combination combo1 = new Combination(name + $" {idProg}");
                for (int j = 0; j < LFRDCombo1List.Count; j++)
                {
                    combo1.AddLoadCaseCoefficient(LFRDCombo1List[j].LoadCase, LFRDCombo1List[j].Coefficient);
                }
                if (!combinationsHashSet.Contains(combo1))
                {
                    combinationsHashSet.Add(combo1);
                    idProg++;
                }


                List<List<Combination.LoadCaseCoefficient>> LFRDCombo2List = LFRDCombo2(loadCases.Cast<LoadCase>().ToList());
                for (int i = 0; i < LFRDCombo2List.Count; i++)
                {
                    Combination combo2 = new Combination(name + $" {idProg}");
                    for (int j = 0; j < LFRDCombo2List[i].Count; j++)
                    {
                        combo2.AddLoadCaseCoefficient(LFRDCombo2List[i][j].LoadCase, LFRDCombo2List[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(combo2))
                    {
                        combinationsHashSet.Add(combo2);
                        idProg++;
                    }
                }

                List<List<Combination.LoadCaseCoefficient>> LFRDCombo3List = LFRDCombo3(loadCases.Cast<LoadCase>().ToList());
                for (int i = 0; i < LFRDCombo3List.Count; i++)
                {
                    Combination combo3 = new Combination(name + $" {idProg}");
                    for (int j = 0; j < LFRDCombo3List[i].Count; j++)
                    {
                        combo3.AddLoadCaseCoefficient(LFRDCombo3List[i][j].LoadCase, LFRDCombo3List[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(combo3))
                    {
                        combinationsHashSet.Add(combo3);
                        idProg++;
                    }
                }

                List<List<Combination.LoadCaseCoefficient>> LFRDCombo4List = LFRDCombo4(loadCases.Cast<LoadCase>().ToList());
                for (int i = 0; i < LFRDCombo4List.Count; i++)
                {
                    Combination combo4 = new Combination(name + $" {idProg}");
                    for (int j = 0; j < LFRDCombo4List[i].Count; j++)
                    {
                        combo4.AddLoadCaseCoefficient(LFRDCombo4List[i][j].LoadCase, LFRDCombo4List[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(combo4))
                    {
                        combinationsHashSet.Add(combo4);
                        idProg++;
                    }
                }

                List<List<Combination.LoadCaseCoefficient>> LFRDCombo5List = LFRDCombo5(loadCases.Cast<LoadCase>().ToList());
                for (int i = 0; i < LFRDCombo5List.Count; i++)
                {
                    Combination combo5 = new Combination(name + $" {idProg}");
                    for (int j = 0; j < LFRDCombo5List[i].Count; j++)
                    {
                        combo5.AddLoadCaseCoefficient(LFRDCombo5List[i][j].LoadCase, LFRDCombo5List[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(combo5))
                    {
                        combinationsHashSet.Add(combo5);
                        idProg++;
                    }
                }

                List<Combination.LoadCaseCoefficient> LFRDCombo6List = LFRDCombo6(loadCases.Cast<LoadCase>().ToList());
                Combination combo6 = new Combination(name + $" {idProg}");
                for (int j = 0; j < LFRDCombo6List.Count; j++)
                {
                    combo6.AddLoadCaseCoefficient(LFRDCombo6List[j].LoadCase, LFRDCombo6List[j].Coefficient);
                }
                if (!combinationsHashSet.Contains(combo6))
                {
                    combinationsHashSet.Add(combo6);
                    idProg++;
                }

                List<Combination.LoadCaseCoefficient> LFRDCombo7List = LFRDCombo7(loadCases.Cast<LoadCase>().ToList());
                Combination combo7 = new Combination(name + $" {idProg}");
                for (int j = 0; j < LFRDCombo7List.Count; j++)
                {
                    combo7.AddLoadCaseCoefficient(LFRDCombo7List[j].LoadCase, LFRDCombo7List[j].Coefficient);
                }
                if (!combinationsHashSet.Contains(combo7))
                {
                    combinationsHashSet.Add(combo7);
                    idProg++;
                }
            }

            else if (((ASCE16CombinationsOptions)options).LimitState == StandardASCE16.LimitStates.ASD)
            {
                List<Combination.LoadCaseCoefficient> ASDCombo1List = ASDCombo1(loadCases.Cast<LoadCase>().ToList());
                Combination combo1 = new Combination(name + $" {idProg}");
                for (int j = 0; j < ASDCombo1List.Count; j++)
                {
                    combo1.AddLoadCaseCoefficient(ASDCombo1List[j].LoadCase, ASDCombo1List[j].Coefficient);
                }
                if (!combinationsHashSet.Contains(combo1))
                {
                    combinationsHashSet.Add(combo1);
                    idProg++;
                }

                List<List<Combination.LoadCaseCoefficient>> ASDCombo2List = ASDCombo2(loadCases.Cast<LoadCase>().ToList());
                for (int i = 0; i < ASDCombo2List.Count; i++)
                {
                    Combination combo2 = new Combination(name + $" {idProg}");
                    for (int j = 0; j < ASDCombo2List[i].Count; j++)
                    {
                        combo2.AddLoadCaseCoefficient(ASDCombo2List[i][j].LoadCase, ASDCombo2List[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(combo2))
                    {
                        combinationsHashSet.Add(combo2);
                        idProg++;
                    }
                }

                List<List<Combination.LoadCaseCoefficient>> ASDCombo3List = ASDCombo3(loadCases.Cast<LoadCase>().ToList());
                for (int i = 0; i < ASDCombo3List.Count; i++)
                {
                    Combination combo3 = new Combination(name + $" {idProg}");
                    for (int j = 0; j < ASDCombo3List[i].Count; j++)
                    {
                        combo3.AddLoadCaseCoefficient(ASDCombo3List[i][j].LoadCase, ASDCombo3List[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(combo3))
                    {
                        combinationsHashSet.Add(combo3);
                        idProg++;
                    }
                }

                List<List<Combination.LoadCaseCoefficient>> ASDCombo4List = ASDCombo4(loadCases.Cast<LoadCase>().ToList());
                for (int i = 0; i < ASDCombo4List.Count; i++)
                {
                    Combination combo4 = new Combination(name + $" {idProg}");
                    for (int j = 0; j < ASDCombo4List[i].Count; j++)
                    {
                        combo4.AddLoadCaseCoefficient(ASDCombo4List[i][j].LoadCase, ASDCombo4List[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(combo4))
                    {
                        combinationsHashSet.Add(combo4);
                        idProg++;
                    }
                }

                List<List<Combination.LoadCaseCoefficient>> ASDCombo5List = ASDCombo5(loadCases.Cast<LoadCase>().ToList());
                for (int i = 0; i < ASDCombo5List.Count; i++)
                {
                    Combination combo5 = new Combination(name + $" {idProg}");
                    for (int j = 0; j < ASDCombo5List[i].Count; j++)
                    {
                        combo5.AddLoadCaseCoefficient(ASDCombo5List[i][j].LoadCase, ASDCombo5List[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(combo5))
                    {
                        combinationsHashSet.Add(combo5);
                        idProg++;
                    }
                }

                List<List<Combination.LoadCaseCoefficient>> ASDCombo6List = ASDCombo6(loadCases.Cast<LoadCase>().ToList());
                for (int i = 0; i < ASDCombo6List.Count; i++)
                {
                    Combination combo6 = new Combination(name + $" {idProg}");
                    for (int j = 0; j < ASDCombo6List[i].Count; j++)
                    {
                        combo6.AddLoadCaseCoefficient(ASDCombo6List[i][j].LoadCase, ASDCombo6List[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(combo6))
                    {
                        combinationsHashSet.Add(combo6);
                        idProg++;
                    }
                }

                List<List<Combination.LoadCaseCoefficient>> ASDCombo7List = ASDCombo7(loadCases.Cast<LoadCase>().ToList());
                for (int i = 0; i < ASDCombo7List.Count; i++)
                {
                    Combination combo7 = new Combination(name + $" {idProg}");
                    for (int j = 0; j < ASDCombo7List[i].Count; j++)
                    {
                        combo7.AddLoadCaseCoefficient(ASDCombo7List[i][j].LoadCase, ASDCombo7List[i][j].Coefficient);
                    }
                    if (!combinationsHashSet.Contains(combo7))
                    {
                        combinationsHashSet.Add(combo7);
                        idProg++;
                    }
                }

                List<Combination.LoadCaseCoefficient> ASDCombo8List = ASDCombo8(loadCases.Cast<LoadCase>().ToList());
                Combination combo8 = new Combination(name + $" {idProg}");
                for (int j = 0; j < ASDCombo8List.Count; j++)
                {
                    combo8.AddLoadCaseCoefficient(ASDCombo8List[j].LoadCase, ASDCombo8List[j].Coefficient);
                }
                if (!combinationsHashSet.Contains(combo8))
                {
                    combinationsHashSet.Add(combo8);
                    idProg++;
                }

                List<Combination.LoadCaseCoefficient> ASDCombo9List = ASDCombo9(loadCases.Cast<LoadCase>().ToList());
                Combination combo9 = new Combination(name + $" {idProg}");
                for (int j = 0; j < ASDCombo9List.Count; j++)
                {
                    combo9.AddLoadCaseCoefficient(ASDCombo9List[j].LoadCase, ASDCombo9List[j].Coefficient);
                }
                if (!combinationsHashSet.Contains(combo9))
                {
                    combinationsHashSet.Add(combo9);
                    idProg++;
                }

                List<Combination.LoadCaseCoefficient> ASDCombo10List = ASDCombo10(loadCases.Cast<LoadCase>().ToList());
                Combination combo10 = new Combination(name + $" {idProg}");
                for (int j = 0; j < ASDCombo10List.Count; j++)
                {
                    combo10.AddLoadCaseCoefficient(ASDCombo10List[j].LoadCase, ASDCombo10List[j].Coefficient);
                }
                if (!combinationsHashSet.Contains(combo10))
                {
                    combinationsHashSet.Add(combo10);
                    idProg++;
                }
            }

            else
                throw new ArgumentException("Fail to generate");

            foreach (Combination comb in combinationsHashSet)
                combinations.Add(comb.Name, comb);
            return combinations;
        }

        #endregion

        #region PRIVATE METHOD

        #region LFRD

        private List<Combination.LoadCaseCoefficient> LFRDCombo1(List<LoadCase> loadCases)
        {
            // LFRD combo 1 : 1.4 D

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSList = new List<Combination.LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd1PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            return loadCaseCoefficientSList;
        }

        private List<List<Combination.LoadCaseCoefficient>> LFRDCombo2(List<LoadCase> loadCases)
        {
            // LFRD combo 2 : 1.2 D + 1.6 L + 0.5 (Lr or S or R)

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListS = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListM = new List<Combination.LoadCaseCoefficient>();
            List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd2PermCoef1, l);
                    loadCaseCoefficientSListM.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd2VarCoef1, l);
                    loadCaseCoefficientSListM.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd2VarCoef2, l);
                    loadCaseCoefficientSListM.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListM);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd2PermCoef1, l);
                    loadCaseCoefficientSListS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd2VarCoef1, l);
                    loadCaseCoefficientSListS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd2VarCoef2, l);
                    loadCaseCoefficientSListS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListS);

            return outList;
        }

        private List<List<Combination.LoadCaseCoefficient>> LFRDCombo3(List<LoadCase> loadCases)
        {
            // LFRD combo 3 : 1.2 D + 1.6 L + 0.5 (Lr or S)

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListLrL = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListLrW = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListSL = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListSW = new List<Combination.LoadCaseCoefficient>();
            List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3PermCoef1, l);
                    loadCaseCoefficientSListLrL.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3VarCoef1, l);
                    loadCaseCoefficientSListLrL.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3VarCoef2L, l);
                    loadCaseCoefficientSListLrL.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListLrL);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3PermCoef1, l);
                    loadCaseCoefficientSListLrW.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3VarCoef1, l);
                    loadCaseCoefficientSListLrW.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3VarCoef2L, l);
                    loadCaseCoefficientSListLrW.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListLrW);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3PermCoef1, l);
                    loadCaseCoefficientSListSL.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3VarCoef1, l);
                    loadCaseCoefficientSListSL.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3VarCoef2L, l);
                    loadCaseCoefficientSListSL.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListSL);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3PermCoef1, l);
                    loadCaseCoefficientSListSW.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3VarCoef1, l);
                    loadCaseCoefficientSListSW.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd3VarCoef2L, l);
                    loadCaseCoefficientSListSW.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListSW);

            return outList;
        }

        private List<List<Combination.LoadCaseCoefficient>> LFRDCombo4(List<LoadCase> loadCases)
        {
            // LFRD combo 4 : 1.2 D + 1.0 W + L + 0.5 (Lr or S)

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWpLr = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWsLr = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWpS = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWsS = new List<Combination.LoadCaseCoefficient>();
            List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4PermCoef1, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef1, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWpLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4PermCoef1, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef1, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWsLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4PermCoef1, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef1, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWpS);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4PermCoef1, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef1, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd4VarCoef2, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWsS);

            return outList;
        }

        private List<List<Combination.LoadCaseCoefficient>> LFRDCombo5(List<LoadCase> loadCases)
        {
            // LFRD combo 4 : 1.2 D + 1.0 W + L + 0.5 (Lr or S)

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWp = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWs = new List<Combination.LoadCaseCoefficient>();
            List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd5PermCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd5VarCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWp);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd5PermCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd5VarCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWs);

            return outList;
        }

        private List<Combination.LoadCaseCoefficient> LFRDCombo6(List<LoadCase> loadCases)
        {
            // LFRD combo 1 : 1.4 D

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSList = new List<Combination.LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd6PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd6VarCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd6VarCoef2, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd6VarCoef3, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            return loadCaseCoefficientSList;
        }

        private List<Combination.LoadCaseCoefficient> LFRDCombo7(List<LoadCase> loadCases)
        {
            // LFRD combo 1 : 1.4 D

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSList = new List<Combination.LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd7PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd7VarCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            return loadCaseCoefficientSList;
        }

        #endregion


        #region ASD

        private List<Combination.LoadCaseCoefficient> ASDCombo1(List<LoadCase> loadCases)
        {
            // ASD combo 1 : D

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSList = new List<Combination.LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd1PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            return loadCaseCoefficientSList;
        }

        private List<List<Combination.LoadCaseCoefficient>> ASDCombo2(List<LoadCase> loadCases)
        {
            // ASD combo 2 : D + L 

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListM = new List<Combination.LoadCaseCoefficient>();
            List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd2PermCoef1, l);
                    loadCaseCoefficientSListM.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd2VarCoef1, l);
                    loadCaseCoefficientSListM.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListM);

            return outList;
        }

        private List<List<Combination.LoadCaseCoefficient>> ASDCombo3(List<LoadCase> loadCases)
        {
            // ASD combo 3 : D + 0.75 L + 0.75 (Lr or S)

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListLr = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListS = new List<Combination.LoadCaseCoefficient>();
            List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd3PermCoef1, l);
                    loadCaseCoefficientSListLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd3VarCoef1, l);
                    loadCaseCoefficientSListLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd3PermCoef1, l);
                    loadCaseCoefficientSListS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd3VarCoef1, l);
                    loadCaseCoefficientSListS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListS);

            return outList;
        }

        private List<List<Combination.LoadCaseCoefficient>> ASDCombo4(List<LoadCase> loadCases)
        {
            // ASD combo 4 : D + 0.75 L + 0.75 (Lr or S)

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListLLr = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListLS = new List<Combination.LoadCaseCoefficient>();
            List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd4PermCoef1, l);
                    loadCaseCoefficientSListLLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd4VarCoef1, l);
                    loadCaseCoefficientSListLLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd4VarCoef2, l);
                    loadCaseCoefficientSListLLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListLLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd4PermCoef1, l);
                    loadCaseCoefficientSListLS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd4VarCoef1, l);
                    loadCaseCoefficientSListLS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd4VarCoef2, l);
                    loadCaseCoefficientSListLS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListLS);

            return outList;
        }

        private List<List<Combination.LoadCaseCoefficient>> ASDCombo5(List<LoadCase> loadCases)
        {
            // ASD combo 5 : 1 D + 0.6 W

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWp = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWs = new List<Combination.LoadCaseCoefficient>();
            List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd5PermCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd5VarCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWp);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Lfrd5PermCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd5VarCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWs);

            return outList;
        }

        private List<List<Combination.LoadCaseCoefficient>> ASDCombo6(List<LoadCase> loadCases)
        {
            // ASD combo 6 : D + 0.75L + 0.75*0.6W + 0.75(Lr or S or R)

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWpLr = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWpS = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWsLr = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWsS = new List<Combination.LoadCaseCoefficient>();
            List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6PermCoef1, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef1, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef2, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef3, l);
                    loadCaseCoefficientSListWpLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWpLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6PermCoef1, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef1, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef2, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Maintenance)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef3, l);
                    loadCaseCoefficientSListWsLr.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWsLr);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6PermCoef1, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef1, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef2, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef3, l);
                    loadCaseCoefficientSListWpS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWpS);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6PermCoef1, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef1, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef2, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }

                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd6VarCoef3, l);
                    loadCaseCoefficientSListWsS.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWsS);

            return outList;
        }

        private List<List<Combination.LoadCaseCoefficient>> ASDCombo7(List<LoadCase> loadCases)
        {
            // ASD combo 7 : 0.6 D + 0.6 W

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWp = new List<Combination.LoadCaseCoefficient>();
            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSListWs = new List<Combination.LoadCaseCoefficient>();

            List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd7PermCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd7VarCoef1, l);
                    loadCaseCoefficientSListWp.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWp);

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd7PermCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd7VarCoef1, l);
                    loadCaseCoefficientSListWs.Add(loadCaseCoefficient);
                }
            }
            outList.Add(loadCaseCoefficientSListWs);

            return outList;
        }

        private List<Combination.LoadCaseCoefficient> ASDCombo8(List<LoadCase> loadCases)
        {
            // ASD combo 8 : D + 0.7 Ev + 0.7 Eh

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSList = new List<Combination.LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd8PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd8VarCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }
            return loadCaseCoefficientSList;
        }

        private List<Combination.LoadCaseCoefficient> ASDCombo9(List<LoadCase> loadCases)
        {
            // ASD combo 9 : D + 0.525 Ev + 0.525 Eh + 0.75 L + 0.75 S

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSList = new List<Combination.LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd9PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd9VarCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.LiveLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd9VarCoef3, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Snow)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd9VarCoef4, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            return loadCaseCoefficientSList;
        }

        private List<Combination.LoadCaseCoefficient> ASDCombo10(List<LoadCase> loadCases)
        {
            // ASD combo 10 : 0.6 D - 0.7 Ev + 0.7 Eh

            List<Combination.LoadCaseCoefficient> loadCaseCoefficientSList = new List<Combination.LoadCaseCoefficient>();

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || l.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd10PermCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            foreach (LoadCase l in loadCases)
            {
                if (l.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
                {
                    Combination.LoadCaseCoefficient loadCaseCoefficient = new Combination.LoadCaseCoefficient(Asd10VarCoef1, l);
                    loadCaseCoefficientSList.Add(loadCaseCoefficient);
                }
            }

            return loadCaseCoefficientSList;
        }



        #endregion

        #endregion


        #endregion

        #region PUBLIC OVERRIDE METHODS


        public object Clone(string name)
        {
            return new Combination(name);
        }

        /// <summary>
        /// Create a new empty <see cref="Combination"/> object. I.e. with the same properties except the <see cref="Combination.LoadCaseCoefficient"/> List that will be empty
        /// </summary>
        public object CloneEmpty()
        {
            var cloned = new StandardASCE16();
            return cloned;
        }

        public Combination Duplicate(string nameOverride)
        {
            var duplicated = (Combination)Clone(nameOverride);
            return duplicated;
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as StandardASCE16);
        }

        public bool Equals(StandardASCE16 other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return other != null && base.Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(StandardASCE16 obj1, StandardASCE16 obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(StandardASCE16 obj1, StandardASCE16 obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
