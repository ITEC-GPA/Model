using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System.Collections.Generic;
using GPC.TestUtilities;
using System.Linq;

namespace ModelObjectTest
{
    [TestClass]
    public class CombinationTest : UnitTestBase
    {

        [TestMethod]
        public void CombinationTest1()
        {
            // Arrange
            List<LoadCase> loadCases = new List<LoadCase>();
            List<double> coefficients = new List<double>();

            loadCases.Add(new LoadCase("Snow", LoadCase.LoadCaseTypes.Snow, Guid.NewGuid()));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseTypes.LiveLoad, Guid.NewGuid()));
            coefficients.Add(1);

            loadCases.Add(new LoadCase("SW", LoadCase.LoadCaseTypes.SelfWeight, Guid.NewGuid()));
            coefficients.Add(3);

            loadCases.Add(new LoadCase("SDL", LoadCase.LoadCaseTypes.SuperImposedDeadLoad, Guid.NewGuid()));
            coefficients.Add(4);

            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateEquilibrium;
            CombinationEn combination = new CombinationEn("test", limitState);

            combination.AddLoadCaseCoefficients(loadCases, coefficients);

            // Act
            string combinationName = combination.ToString();

            // Assert
            var splitted = combinationName.Split(new string[] { "+" }, StringSplitOptions.None);

            Console.WriteLine(combinationName);
            Assert.IsTrue(splitted[0].Contains("SW"), combinationName);
            Assert.IsTrue(splitted[1].Contains("SDL"), combinationName);
        }

        [TestMethod]
        public void CombinationTest2()
        {
            // Arrange
            List<LoadCase> loadCases = new List<LoadCase>();
            List<double> coefficients = new List<double>();

            loadCases.Add(new LoadCase("Snow", Guid.NewGuid()));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseTypes.LiveLoad, Guid.NewGuid()));
            coefficients.Add(1);

            loadCases.Add(new LoadCase("SW", Guid.NewGuid()));
            coefficients.Add(0.5);

            loadCases.Add(new LoadCase("SDL", LoadCase.LoadCaseTypes.SuperImposedDeadLoad, Guid.NewGuid()));
            coefficients.Add(4);

            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateEquilibrium;
            CombinationEn combination = new CombinationEn("test", limitState);

            combination.AddLoadCaseCoefficients(loadCases, coefficients);

            // Act
            string combinationName = combination.ToString();

            // Assert
            var splitted = combinationName.Split(new string[] { "+" }, StringSplitOptions.None);

            Console.WriteLine(combinationName);
            Assert.IsTrue(splitted[0].Contains("SDL"), combinationName);
            Assert.IsTrue(splitted[3].Contains("SW"), combinationName);
        }

        [TestMethod]
        public void CombinationTest3()
        {
            // Arrange
            List<LoadCase> loadCases = new List<LoadCase>();
            List<double> coefficients = new List<double>();

            loadCases.Add(new LoadCase("Snow", LoadCase.LoadCaseTypes.Snow, Guid.NewGuid()));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseTypes.LiveLoad, Guid.NewGuid()));
            coefficients.Add(1);

            loadCases.Add(new LoadCase("SW", LoadCase.LoadCaseTypes.SelfWeight, Guid.NewGuid()));
            coefficients.Add(0.5);

            LoadCase sdl = new LoadCase("SDL", LoadCase.LoadCaseTypes.SuperImposedDeadLoad, Guid.NewGuid());
            loadCases.Add(sdl);
            coefficients.Add(4);
            loadCases.Add(sdl);
            coefficients.Add(4);

            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;
            CombinationEn combination = new CombinationEn("test", limitState);

            combination.AddLoadCaseCoefficients(loadCases, coefficients);

            // Act
            string combinationName = combination.ToString();

            // Assert
            var splitted = combinationName.Split(new string[] { "+" }, StringSplitOptions.None);

            Console.WriteLine(combinationName);
            Assert.IsTrue(combination[sdl] == 8, combinationName);
            Assert.IsTrue(splitted[0].Contains("SW"), combinationName);
        }

        [TestMethod]
        public void CombinationTest4()
        {
            // Arrange
            List<LoadCase> loadCases = new List<LoadCase>();
            List<double> coefficients = new List<double>();

            loadCases.Add(new LoadCase("Snow", LoadCase.LoadCaseTypes.Snow, Guid.NewGuid()));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseTypes.LiveLoad, Guid.NewGuid()));
            coefficients.Add(1);

            loadCases.Add(new LoadCase("SW", LoadCase.LoadCaseTypes.SelfWeight, Guid.NewGuid()));
            coefficients.Add(0.5);

            LoadCase sdl = new LoadCase("SDL", LoadCase.LoadCaseTypes.SuperImposedDeadLoad, Guid.NewGuid());
            loadCases.Add(sdl);
            coefficients.Add(4);
            loadCases.Add(new LoadCase("SDL", LoadCase.LoadCaseTypes.SuperImposedDeadLoad, Guid.NewGuid()));
            coefficients.Add(4);

            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateGeotechnical;
            CombinationEn combination = new CombinationEn("test", limitState);

            combination.AddLoadCaseCoefficients(loadCases, coefficients);

            // Act
            string combinationName = combination.ToString();

            // Assert
            var splitted = combinationName.Split(new string[] { "+" }, StringSplitOptions.None);

            Console.WriteLine(combinationName);
            Assert.IsTrue(combination[sdl] == 8, combinationName);
            Assert.IsTrue(splitted[0].Contains("SW"), combinationName);
        }

        [TestMethod]
        public void CombinationTest5()
        {
            // Arrange
            List<LoadCase> loadCases = new List<LoadCase>();
            List<double> coefficients = new List<double>();

            loadCases.Add(new LoadCase("Snow", LoadCase.LoadCaseTypes.Snow));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseTypes.LiveLoad));
            coefficients.Add(1);

            loadCases.Add(new LoadCase("SW", LoadCase.LoadCaseTypes.SelfWeight));
            coefficients.Add(0.5);

            LoadCase sdl = new LoadCase("SDL", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            loadCases.Add(sdl);
            coefficients.Add(4);
            loadCases.Add(new LoadCase("SDL", LoadCase.LoadCaseTypes.SuperImposedDeadLoad));
            coefficients.Add(4);


            CombinationAsce combination = new CombinationAsce("test", StandardASCE16.LimitState.LFRD);

            combination.AddLoadCaseCoefficients(loadCases, coefficients);

            combination.AddLoadCaseCoefficient(new LoadCase("Zero", LoadCase.LoadCaseTypes.Earthquake), 0);

            // Act
            string combinationName = combination.ToString();

            // Assert
            var splitted = combinationName.Split(new string[] { "+" }, StringSplitOptions.None);

            Console.WriteLine(combinationName);
            Assert.IsTrue(combination[sdl] == 8, combinationName);
            Assert.IsTrue(combination[new LoadCase("test")] == 0, combinationName);
            Assert.IsTrue(combination[new LoadCase("Zero", LoadCase.LoadCaseTypes.Earthquake)] == 0, combinationName);
            Assert.IsTrue(splitted[0].Contains("SW"), combinationName);
            Assert.IsFalse(splitted[0].Contains("Zero"), combinationName);
        }

        [TestMethod]
        public void CombinationContainsLoadCases()
        {
            // Arrange

            CombinationAsce combination = new CombinationAsce("cmb1", StandardASCE16.LimitState.LFRD);
            
            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseTypes.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            var lc3 = new LoadCase("LC3", LoadCase.LoadCaseTypes.ClimateSummerDeltaH);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseTypes.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseTypes.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);

            combination.AddLoadCaseCoefficient(lc1, 1);
            combination.AddLoadCaseCoefficient(lc2, 2);
            combination.AddLoadCaseCoefficient(lc3, 3);
            combination.AddLoadCaseCoefficient(lc4, 4);
            combination.AddLoadCaseCoefficient(lc5, 5);


            // Assert / Act

            Assert.IsTrue(combination.ContainsLoadCases(new List<LoadCase> { lc1 }));
            Assert.IsTrue(combination.ContainsLoadCases(new List<LoadCase> { lc1, lc2 }));
            Assert.IsTrue(combination.ContainsLoadCases(new List<LoadCase> { lc5, lc1 }));
            Assert.IsFalse(combination.ContainsLoadCases(new List<LoadCase> { lc6 }));
            Assert.IsFalse(combination.ContainsLoadCases(new List<LoadCase> { lc6, lc1 }));
            Assert.IsTrue(combination.ContainsLoadCases(new List<LoadCase> ()));
        }

        [TestMethod]
        public void CombinationContainsLoadCaseCoefficients()
        {
            // Arrange

            CombinationAsce combination = new CombinationAsce("cmb1", StandardASCE16.LimitState.LFRD);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseTypes.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            var lc3 = new LoadCase("LC3", LoadCase.LoadCaseTypes.ClimateSummerDeltaH);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseTypes.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseTypes.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);

            combination.AddLoadCaseCoefficient(lc1, 1);
            combination.AddLoadCaseCoefficient(lc2, 2);
            combination.AddLoadCaseCoefficient(lc3, 3);
            combination.AddLoadCaseCoefficient(lc4, 4);
            combination.AddLoadCaseCoefficient(lc5, 5);


            // Assert / Act

            Assert.IsTrue(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCase, double>(lc1, 1) } ));
            Assert.IsTrue(combination.ContainsLoadCaseCoefficients(new List<(LoadCase, double)> { (lc1, 1) }));

            Assert.IsFalse(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCase, double>(lc1, 2) } ));
            Assert.IsFalse(combination.ContainsLoadCaseCoefficients(new List<(LoadCase, double)> { (lc1, 2) }));


            Assert.IsTrue(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCase, double>(lc1, 1), 
                                                                           new KeyValuePair<LoadCase, double>(lc2, 2) } ));

            Assert.IsFalse(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCase, double>(lc1, 1),
                                                                            new KeyValuePair<LoadCase, double>(lc2, 3) }));


            Assert.IsTrue(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCase, double>(lc1, 1),
                                                                           new KeyValuePair<LoadCase, double>(lc4, 4) }));

            Assert.IsFalse(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCase, double>(lc6, 1) }));

            Assert.IsFalse(combination.ContainsLoadCaseCoefficients(new KeyValuePair<LoadCase, double>[1]));

        }

        [TestMethod]
        public void CombinationGetLoadCaseCoefficients()
        {
            // Arrange

            CombinationAsce combination = new CombinationAsce("cmb1", StandardASCE16.LimitState.LFRD);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseTypes.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            var lc3 = new LoadCase("LC3", LoadCase.LoadCaseTypes.ClimateSummerDeltaH);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseTypes.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseTypes.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);

            combination.AddLoadCaseCoefficient(lc1, 1);
            combination.AddLoadCaseCoefficient(lc2, 2);
            combination.AddLoadCaseCoefficient(lc3, 3);
            combination.AddLoadCaseCoefficient(lc4, 4);
            combination.AddLoadCaseCoefficient(lc5, 5);


            // Assert / Act
            var pair = combination.GetLoadCaseCoefficientsPair();
            var tuple = combination.GetLoadCaseCoefficientsTuple();

            var tuple2 = combination.GetLoadCaseCoefficientsTuple(new List<LoadCase> { lc1, lc2 });
            var tuple3 = combination.GetLoadCaseCoefficientsTuple(new List<LoadCase> { lc2, lc6 });


            Assert.IsTrue(pair[0].Key == lc1, pair[0].Key.Name.ToString());
            Assert.IsTrue(pair[1].Key == lc2, pair[1].Key.Name.ToString());
            Assert.IsTrue(tuple[0].loadcase == lc1, tuple[0].loadcase.Name.ToString());
            Assert.IsTrue(tuple[1].loadcase == lc2, tuple[1].loadcase.Name.ToString());

            Assert.IsTrue(tuple2.Length == 2, tuple2.Length.ToString());
            Assert.IsTrue(tuple2[0].coefficient == 1, tuple2[0].coefficient.ToString());

            Assert.IsTrue(tuple3.Length == 1, tuple3.Length.ToString());
            Assert.IsTrue(tuple3[0].coefficient == 2, tuple3[0].coefficient.ToString());
        }

        [TestMethod]
        public void EqualsHashCode()
        {
            // Arrange

            CombinationAsce combination1 = new CombinationAsce("cmb1", StandardASCE16.LimitState.LFRD);
            CombinationAsce combination2 = new CombinationAsce("cmb1", StandardASCE16.LimitState.LFRD);
            CombinationAsce combination3 = new CombinationAsce("cmb1", StandardASCE16.LimitState.ASD);

            CombinationAsce combination4 = new CombinationAsce("cmb1", StandardASCE16.LimitState.LFRD);

            CombinationAsce combination5 = new CombinationAsce("cmb1", StandardASCE16.LimitState.LFRD);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseTypes.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            var lc3 = new LoadCase("LC3", LoadCase.LoadCaseTypes.ClimateSummerDeltaH);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseTypes.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseTypes.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);

            combination1.AddLoadCaseCoefficient(lc1, 1);
            combination1.AddLoadCaseCoefficient(lc2, 2);
            combination1.AddLoadCaseCoefficient(lc3, 3);
            combination1.AddLoadCaseCoefficient(lc4, 4);
            combination1.AddLoadCaseCoefficient(lc5, 5);

            combination2.AddLoadCaseCoefficient(lc1, 1);
            combination2.AddLoadCaseCoefficient(lc2, 2);
            combination2.AddLoadCaseCoefficient(lc3, 3);
            combination2.AddLoadCaseCoefficient(lc4, 4);
            combination2.AddLoadCaseCoefficient(lc5, 5);

            combination3.AddLoadCaseCoefficient(lc1, 1);
            combination3.AddLoadCaseCoefficient(lc2, 2);
            combination3.AddLoadCaseCoefficient(lc3, 3);
            combination3.AddLoadCaseCoefficient(lc4, 4);
            combination3.AddLoadCaseCoefficient(lc5, 5);

            combination4.AddLoadCaseCoefficient(lc1, 1);
            combination4.AddLoadCaseCoefficient(lc2, 2);
            combination4.AddLoadCaseCoefficient(lc3, 3);
            combination4.AddLoadCaseCoefficient(lc4, 4);
            combination4.AddLoadCaseCoefficient(lc5, 5);
            combination4.AddLoadCaseCoefficient(lc6, 5);

            combination5.AddLoadCaseCoefficient(lc1, 1);
            combination5.AddLoadCaseCoefficient(lc2, 2);
            combination5.AddLoadCaseCoefficient(lc3, 2);
            combination5.AddLoadCaseCoefficient(lc4, 4);
            combination5.AddLoadCaseCoefficient(lc5, 5);

            // Assert / Act
            Assert.IsTrue(combination1.Equals(combination2));
            Assert.IsTrue(combination1.GetHashCode().Equals(combination2.GetHashCode()));

            Assert.IsFalse(combination1.Equals(combination3));
            Assert.IsFalse(combination1.GetHashCode().Equals(combination3.GetHashCode()));
            
            Assert.IsFalse(combination1.Equals(combination4));
            Assert.IsFalse(combination1.GetHashCode().Equals(combination4.GetHashCode()));

            Assert.IsFalse(combination1.Equals(combination5));
            Assert.IsFalse(combination1.GetHashCode().Equals(combination5.GetHashCode()));

        }

        [TestMethod]
        public void ENGeneratorUltimateStructural1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryC;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;
            StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetC;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category, uLS, false);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][prestressLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][prestressLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][prestressLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.65) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.78) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateStructural2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;
            StandardEN1990.ULSStructuralGeotechicalCombinationSets uLSCombinationSets = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetB;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category, uLSCombinationSets);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateStructural3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "LiveLoad";
            LoadCase liveLoadLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                liveLoadLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryE;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;
            StandardEN1990.ULSStructuralGeotechicalCombinationSets uLSCombinationSets = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetC;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category, uLSCombinationSets, false);

            // Assert
            Assert.IsTrue(outList.Count() == 4);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][liveLoadLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][liveLoadLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.65) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.30) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][snowLoadCase] - 0.65) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 1.30) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.78) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][WindPressureLoadCase] - 0.78) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateEquilibrium1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryD;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateEquilibrium;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.10) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.10) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.10) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateEquilibrium2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardUNIEN1990 standardUNIEN1990 = new StandardUNIEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateEquilibrium;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardUNIEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.10) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.10) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.10) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateEquilibrium3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "Temperature";
            LoadCase temperatureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.Temperature);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                temperatureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryD;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateEquilibrium;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.10) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.10) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.10) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][temperatureLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][temperatureLoadCase] - 0.9) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateFatigue1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryG;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateFatigue;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateFatigue2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateFatigue;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateFatigue3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryG;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateFatigue;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateGeotechnical1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure1";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "WindPressure2";
            LoadCase WindPressureLoadCase2 = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.WindPressure);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                WindPressureLoadCase2
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryF;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateGeotechnical;
            StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetB;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category, uLS, false);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase2] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.75) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateGeotechnical2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardUNIEN1990 standardUNIEN1990 = new StandardUNIEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateGeotechnical;
            StandardEN1990.ULSStructuralGeotechicalCombinationSets uLSCombinationSets = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetB;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardUNIEN1990, limitState, category, uLSCombinationSets);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateGeotechnical3()
        {
            // Arrange
            string loadCaseName1 = "SuperImposedDeadLoad";
            LoadCase superImposedDeadLoadLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                superImposedDeadLoadLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryF;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateGeotechnical;
            StandardEN1990.ULSStructuralGeotechicalCombinationSets uLSCombinationSets = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetC;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category, uLSCombinationSets, false);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(Math.Abs(outList[0][superImposedDeadLoadLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][superImposedDeadLoadLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][superImposedDeadLoadLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.65) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.78) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorServiceabilityCharacteristic()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                prestressLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.ServiceabilityCharacteristic;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.70) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.60) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 0.20) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorServiceabilityQuasiPermanent()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.ServiceabilityQuasiPermanent;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 2);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.20) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorServiceabilityFrequent()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.ServiceabilityFrequent;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase] - 0.20) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 0.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.20) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase] - 0.00) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorMultyLoadCase()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName5 = "selfWeight2";
            LoadCase selfWeightLoadCase2 = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure1";
            LoadCase WindPressureLoadCase1 = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName4 = "WindPressure2";
            LoadCase WindPressureLoadCase2 = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName8 = "WindPressure3";
            LoadCase WindPressureLoadCase4 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName6 = "WindPressure4";
            LoadCase WindPressureLoadCase3 = new LoadCase(loadCaseName6, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName7 = "Snow2";
            LoadCase snowLoadCase2 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName3 = "Snow1";
            LoadCase snowLoadCase1 = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);


            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                selfWeightLoadCase2,
                WindPressureLoadCase1,
                WindPressureLoadCase2,
                WindPressureLoadCase3,
                WindPressureLoadCase4,
                snowLoadCase1,
                snowLoadCase2
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase3] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase4] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase2] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase3] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase4] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][WindPressureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][WindPressureLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][WindPressureLoadCase3] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][WindPressureLoadCase4] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][snowLoadCase2] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][WindPressureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][WindPressureLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][WindPressureLoadCase3] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][WindPressureLoadCase4] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][snowLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][snowLoadCase2] - 1.50) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorMultyLoadCase2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName5 = "selfWeight2";
            LoadCase selfWeightLoadCase2 = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure1";
            LoadCase WindPressureLoadCase1 = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName4 = "WindPressure2";
            LoadCase WindPressureLoadCase2 = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName8 = "Temperature1";
            LoadCase temperatureLoadCase4 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.Temperature);
            string loadCaseName6 = "Temperature2";
            LoadCase temperatureLoadCase3 = new LoadCase(loadCaseName6, LoadCase.LoadCaseTypes.Temperature);
            string loadCaseName7 = "Snow2";
            LoadCase snowLoadCase2 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName3 = "Snow1";
            LoadCase snowLoadCase1 = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);


            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                selfWeightLoadCase2,
                WindPressureLoadCase1,
                WindPressureLoadCase2,
                temperatureLoadCase3,
                temperatureLoadCase4,
                snowLoadCase1,
                snowLoadCase2
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 8);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][selfWeightLoadCase] - 1.35) < 0.001);

            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][WindPressureLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][temperatureLoadCase3] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][temperatureLoadCase4] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase2] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[1][temperatureLoadCase3] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][temperatureLoadCase4] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][WindPressureLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase2] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[2][snowLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][snowLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][WindPressureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][WindPressureLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][temperatureLoadCase3] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][temperatureLoadCase4] - 0.90) < 0.001);

            Assert.IsTrue(Math.Abs(outList[3][WindPressureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][WindPressureLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][temperatureLoadCase3] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][temperatureLoadCase4] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][snowLoadCase2] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[4][temperatureLoadCase3] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][temperatureLoadCase4] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][WindPressureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][WindPressureLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][snowLoadCase2] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[5][snowLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][snowLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][WindPressureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][WindPressureLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][temperatureLoadCase3] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][temperatureLoadCase4] - 0.90) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorWindPressureSuction1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName5 = "WindPressure1";
            LoadCase windPressureLoadCase1 = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName2 = "WindPressure2";
            LoadCase windPressureLoadCase2 = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName4 = "WindSuction1";
            LoadCase windSuctionLoadCase1 = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.WindSuction);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windPressureLoadCase1,
                windPressureLoadCase2,
                windSuctionLoadCase1
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);

            Assert.IsTrue(Math.Abs(outList[0][windPressureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windPressureLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windSuctionLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][windPressureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][windPressureLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][windSuctionLoadCase1] - 1.50) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorWindPressureSuction2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName5 = "WindPressure1";
            LoadCase windPressureLoadCase1 = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName2 = "WindPressure2";
            LoadCase windPressureLoadCase2 = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName4 = "WindSuction1";
            LoadCase windSuctionLoadCase1 = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.WindSuction);
            string loadCaseName8 = "WindSuction2";
            LoadCase windSuctionLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.WindSuction);
            string loadCaseName6 = "Temperature2";
            LoadCase temperatureLoadCase1 = new LoadCase(loadCaseName6, LoadCase.LoadCaseTypes.Temperature);
            string loadCaseName7 = "Snow2";
            LoadCase snowLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName3 = "Snow1";
            LoadCase snowLoadCase2 = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);


            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windPressureLoadCase1,
                windPressureLoadCase2,
                windSuctionLoadCase1,
                windSuctionLoadCase2,
                temperatureLoadCase1,
                snowLoadCase1,
                snowLoadCase2
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 14);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[8][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[9][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[11][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[12][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[13][selfWeightLoadCase] - 1.35) < 0.001);

            Assert.IsTrue(Math.Abs(outList[0][windPressureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windPressureLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][temperatureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase1] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[1][windSuctionLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windSuctionLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][temperatureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase1] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[2][temperatureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][windPressureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][windPressureLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][snowLoadCase2] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[3][temperatureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][windSuctionLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][windSuctionLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][snowLoadCase2] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[4][snowLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][snowLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][windPressureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][windPressureLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][temperatureLoadCase1] - 0.90) < 0.001);

            Assert.IsTrue(Math.Abs(outList[5][snowLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][snowLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][windSuctionLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][windSuctionLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][temperatureLoadCase1] - 0.90) < 0.001);

            Assert.IsTrue(Math.Abs(outList[6][windPressureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][windPressureLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][temperatureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][snowLoadCase1] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[7][windSuctionLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][windSuctionLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][temperatureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][snowLoadCase1] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[8][temperatureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[8][windPressureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[8][windPressureLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[8][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[8][snowLoadCase2] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[9][temperatureLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[9][windSuctionLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[9][windSuctionLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[9][snowLoadCase1] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[9][snowLoadCase2] - 1.05) < 0.001);

            Assert.IsTrue(Math.Abs(outList[10][snowLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][snowLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][windPressureLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][windPressureLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][temperatureLoadCase1] - 0.90) < 0.001);

            Assert.IsTrue(Math.Abs(outList[11][snowLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[11][snowLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[11][windSuctionLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[11][windSuctionLoadCase2] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[11][temperatureLoadCase1] - 0.90) < 0.001);

        }

        [TestMethod]
        public void ENGeneratorClimate1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName6 = "ClimateSummerDeltaT";
            LoadCase climateSummerDeltaTLoadCase1 = new LoadCase(loadCaseName6, LoadCase.LoadCaseTypes.ClimateSummerDeltaT);
            string loadCaseName7 = "ClimateSummerDeltaH";
            LoadCase climateSummerDeltaHLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.ClimateSummerDeltaH);
            string loadCaseName3 = "ClimateSummerDeltaP";
            LoadCase climateSummerDeltaPLoadCase2 = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.ClimateSummerDeltaP);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                climateSummerDeltaTLoadCase1,
                climateSummerDeltaHLoadCase1,
                climateSummerDeltaPLoadCase2
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 8);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);        
            Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
        }
        [TestMethod]
        public void ENGeneratorClimate2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "ClimateSummerDeltaT";
            LoadCase climateSummerDeltaTLoadCase1 = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.ClimateSummerDeltaT);
            string loadCaseName3 = "ClimateSummerDeltaH";
            LoadCase climateSummerDeltaHLoadCase1 = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.ClimateSummerDeltaH);
            string loadCaseName4 = "ClimateSummerDeltaP";
            LoadCase climateSummerDeltaPLoadCase1 = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.ClimateSummerDeltaP);
            string loadCaseName5 = "ClimateSummerDeltaP2";
            LoadCase climateSummerDeltaPLoadCase2 = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.ClimateSummerDeltaP);
            string loadCaseName6 = "ClimateSummerDeltaP3";
            LoadCase climateSummerDeltaPLoadCase3 = new LoadCase(loadCaseName6, LoadCase.LoadCaseTypes.ClimateSummerDeltaP);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                climateSummerDeltaTLoadCase1,
                climateSummerDeltaHLoadCase1,
                climateSummerDeltaPLoadCase1,
                climateSummerDeltaPLoadCase2,
                climateSummerDeltaPLoadCase3
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 8);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][selfWeightLoadCase] - 1.35) < 0.001);

            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorClimate3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "ClimateSummerDeltaT";
            LoadCase climateSummerDeltaTLoadCase1 = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.ClimateSummerDeltaT);
            string loadCaseName3 = "ClimateSummerDeltaH";
            LoadCase climateSummerDeltaHLoadCase1 = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.ClimateSummerDeltaH);
            string loadCaseName4 = "ClimateSummerDeltaP1";
            LoadCase climateSummerDeltaPLoadCase1 = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.ClimateSummerDeltaP);
            string loadCaseName5 = "ClimateSummerDeltaP2";
            LoadCase climateSummerDeltaPLoadCase2 = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.ClimateSummerDeltaP);
            string loadCaseName6 = "ClimateSummerDeltaP3";
            LoadCase climateSummerDeltaPLoadCase3 = new LoadCase(loadCaseName6, LoadCase.LoadCaseTypes.ClimateSummerDeltaP);
            string loadCaseName7 = "ClimateWinterDeltaP1";
            LoadCase climateWinterDeltaPLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.ClimateWinterDeltaP);
            string loadCaseName8 = "ClimateWinterDeltaP2";
            LoadCase climateWinterDeltaPLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.ClimateWinterDeltaP);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                climateSummerDeltaHLoadCase1,
                climateSummerDeltaTLoadCase1,
                climateSummerDeltaPLoadCase1,
                climateSummerDeltaPLoadCase2,
                climateSummerDeltaPLoadCase3,
                climateWinterDeltaPLoadCase1,
                climateWinterDeltaPLoadCase2
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 10);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[8][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[9][selfWeightLoadCase] - 1.35) < 0.001);

            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[8][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);

            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);

            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);

            Assert.IsTrue(Math.Abs(outList[2][climateWinterDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][climateWinterDeltaPLoadCase2] - 1.50) < 0.001);

            Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);

            Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);

            Assert.IsTrue(Math.Abs(outList[5][climateWinterDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][climateWinterDeltaPLoadCase2] - 1.50) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorClimate4()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "ClimateSummerDeltaT";
            LoadCase climateSummerDeltaTLoadCase1 = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.ClimateSummerDeltaT);
            string loadCaseName3 = "ClimateSummerDeltaH";
            LoadCase climateSummerDeltaHLoadCase1 = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.ClimateSummerDeltaH);
            string loadCaseName4 = "ClimateSummerDeltaP1";
            LoadCase climateSummerDeltaPLoadCase1 = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.ClimateSummerDeltaP);
            string loadCaseName5 = "ClimateSummerDeltaP2";
            LoadCase climateSummerDeltaPLoadCase2 = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.ClimateSummerDeltaP);
            string loadCaseName6 = "ClimateSummerDeltaP3";
            LoadCase climateSummerDeltaPLoadCase3 = new LoadCase(loadCaseName6, LoadCase.LoadCaseTypes.ClimateSummerDeltaP);
            string loadCaseName7 = "ClimateWinterDeltaP1";
            LoadCase climateWinterDeltaPLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.ClimateWinterDeltaP);
            string loadCaseName8 = "ClimateWinterDeltaP2";
            LoadCase climateWinterDeltaPLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.ClimateWinterDeltaP);
            string loadCaseName9 = "Wind1";
            LoadCase windLoadCase1 = new LoadCase(loadCaseName9, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName10 = "Wind2";
            LoadCase windLoadCase2 = new LoadCase(loadCaseName10, LoadCase.LoadCaseTypes.WindPressure);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                climateSummerDeltaTLoadCase1,
                climateSummerDeltaHLoadCase1,
                climateSummerDeltaPLoadCase1,
                climateSummerDeltaPLoadCase2,
                climateSummerDeltaPLoadCase3,
                climateWinterDeltaPLoadCase1,
                climateWinterDeltaPLoadCase2,
                windLoadCase1,
                windLoadCase2
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 16);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[8][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[9][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[11][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[12][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[13][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[14][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[15][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);

            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase2] - 0.90) < 0.001);

            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase2] - 0.90) < 0.001);

            Assert.IsTrue(Math.Abs(outList[2][climateWinterDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][climateWinterDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][windLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][windLoadCase2] - 0.90) < 0.001);

            Assert.IsTrue(Math.Abs(outList[3][windLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][windLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][climateWinterDeltaPLoadCase1] - 0.45) < 0.001);            // valore da modificare quando cambieranno gli psi dei vetri
            Assert.IsTrue(Math.Abs(outList[3][climateWinterDeltaPLoadCase2] - 0.45) < 0.001);            // valore da modificare quando cambieranno gli psi dei vetri

            Assert.IsTrue(Math.Abs(outList[4][windLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][windLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaTLoadCase1] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase1] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase2] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase3] - 0.45) < 0.001);

            Assert.IsTrue(Math.Abs(outList[5][windLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][windLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][climateSummerDeltaTLoadCase1] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][climateSummerDeltaPLoadCase1] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][climateSummerDeltaPLoadCase2] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][climateSummerDeltaPLoadCase3] - 0.45) < 0.001);

            Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][windLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[6][windLoadCase2] - 0.90) < 0.001);

            Assert.IsTrue(Math.Abs(outList[7][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][windLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[7][windLoadCase2] - 0.90) < 0.001);

            Assert.IsTrue(Math.Abs(outList[8][climateWinterDeltaPLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[8][climateWinterDeltaPLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[8][windLoadCase1] - 0.90) < 0.001);
            Assert.IsTrue(Math.Abs(outList[8][windLoadCase2] - 0.90) < 0.001);

            Assert.IsTrue(Math.Abs(outList[9][windLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[9][windLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[9][climateWinterDeltaPLoadCase1] - 0.45) < 0.001);            // valore da modificare quando cambieranno gli psi dei vetri
            Assert.IsTrue(Math.Abs(outList[9][climateWinterDeltaPLoadCase2] - 0.45) < 0.001);            // valore da modificare quando cambieranno gli psi dei vetri

            Assert.IsTrue(Math.Abs(outList[10][windLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][windLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][climateSummerDeltaTLoadCase1] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][climateSummerDeltaPLoadCase1] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][climateSummerDeltaPLoadCase2] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[10][climateSummerDeltaPLoadCase3] - 0.45) < 0.001);

            Assert.IsTrue(Math.Abs(outList[11][windLoadCase1] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[11][windLoadCase2] - 1.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[11][climateSummerDeltaTLoadCase1] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[11][climateSummerDeltaPLoadCase1] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[11][climateSummerDeltaPLoadCase2] - 0.45) < 0.001);
            Assert.IsTrue(Math.Abs(outList[11][climateSummerDeltaPLoadCase3] - 0.45) < 0.001);

        }

        [TestMethod]
        public void ENGeneratorUltimateSeismic1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);
            string loadCaseName5 = "Seismic";
            LoadCase seismicLoadCase = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.Earthquake);
            string loadCaseName6 = "LiveLoad";
            LoadCase liveLoadLoadCase = new LoadCase(loadCaseName6, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase,
                seismicLoadCase,
                liveLoadLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryC;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateSeismic;
            StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetC;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category, uLS, false);

            // Assert
            Assert.IsTrue(outList.Count() == 2);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][prestressLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][prestressLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][seismicLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][seismicLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase] - 0.60) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateSeismic2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.Prestress);
            string loadCaseName5 = "Seismic";
            LoadCase seismicLoadCase = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.Earthquake);
            string loadCaseName6 = "LiveLoad1";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName6, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName7 = "LiveLoad2";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad3";
            LoadCase liveLoadLoadCase3 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                prestressLoadCase,
                seismicLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
                liveLoadLoadCase3,
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryC;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateSeismic;
            StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetC;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category, uLS, false);

            // Assert
            Assert.IsTrue(outList.Count() == 2);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][prestressLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][prestressLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][seismicLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][seismicLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase1] - 0.60) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase2] - 0.60) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase3] - 0.60) < 0.001);
        }

        [TestMethod]
        public void ENGeneratorUltimateSeismic3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName4 = "ClimateSummerDeltaH";
            LoadCase climateSummerDeltaHLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseTypes.ClimateSummerDeltaH);
            string loadCaseName5 = "Seismic";
            LoadCase seismicLoadCase = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.Earthquake);
            string loadCaseName6 = "ClimateSummerDeltaP";
            LoadCase climateSummerDeltaPLoadCase1 = new LoadCase(loadCaseName6, LoadCase.LoadCaseTypes.ClimateSummerDeltaP);
            string loadCaseName7 = "LiveLoad2";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad3";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                climateSummerDeltaHLoadCase,
                seismicLoadCase,
                climateSummerDeltaPLoadCase1,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryC;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateSeismic;
            StandardEN1990.ULSStructuralGeotechicalCombinationSets uLS = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetC;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category, uLS, false);

            // Assert
            Assert.IsTrue(outList.Count() == 4);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaHLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaHLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][seismicLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][seismicLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][seismicLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][seismicLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase1] - 0.60) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase2] - 0.60) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][liveLoadLoadCase1] - 0.60) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][liveLoadLoadCase2] - 0.60) < 0.001);
        }

        [TestMethod]
        public void ASCEGeneratorLFRD1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName7 = "LiveLoad2";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad3";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
            };

            StandardASCE16 standardASCE16 = new StandardASCE16();

            // Act
            List<CombinationAsce> outList = CombinationAsce.GenerateCombinations("combo", loadCaseList, standardASCE16, StandardASCE16.LimitState.LFRD);

            // Assert
            Assert.IsTrue(outList.Count() == 11);
        }

        [TestMethod]
        public void ASCEGeneratorLFRD2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight1";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName7 = "LiveLoad2";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad3";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName9 = "selfWeight12";
            LoadCase selfWeightLoadCase2 = new LoadCase(loadCaseName9, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName10 = "Earthquake";
            LoadCase earthquakeLoadCase = new LoadCase(loadCaseName10, LoadCase.LoadCaseTypes.Earthquake);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                selfWeightLoadCase2,
                WindPressureLoadCase,
                snowLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
                earthquakeLoadCase,
            };

            StandardASCE16 standardASCE16 = new StandardASCE16();

            // Act
            List<CombinationAsce> outList = CombinationAsce.GenerateCombinations("combo", loadCaseList, standardASCE16, StandardASCE16.LimitState.LFRD);

            // Assert
            Assert.IsTrue(outList.Count() == 12);
        }

        [TestMethod]
        public void ASCEGeneratorASD1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName7 = "LiveLoad2";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad3";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
            };

            StandardASCE16 standardASCE16 = new StandardASCE16();

            // Act
            List<CombinationAsce> outList = CombinationAsce.GenerateCombinations("combo", loadCaseList, standardASCE16, StandardASCE16.LimitState.ASD);

            // Assert
            Assert.IsTrue(outList.Count() == 11);
        }

        [TestMethod]
        public void ASCEGeneratorASD2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "SuperImposedDeadLoad";
            LoadCase superImposedLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName7 = "LiveLoad2";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad3";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                superImposedLoadCase,
                snowLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
            };

            StandardASCE16 standardASCE16 = new StandardASCE16();

            // Act
            List<CombinationAsce> outList = CombinationAsce.GenerateCombinations("combo", loadCaseList, standardASCE16, StandardASCE16.LimitState.ASD);

            // Assert
            Assert.IsTrue(outList.Count() == 7);
        }
    }
}

