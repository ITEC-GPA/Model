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

            loadCases.Add(new LoadCase("Snow", LoadCase.LoadCaseType.Snow, Guid.NewGuid()));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseType.LiveLoad, Guid.NewGuid()));
            coefficients.Add(1);

            loadCases.Add(new LoadCase("SW", LoadCase.LoadCaseType.SelfWeight, Guid.NewGuid()));
            coefficients.Add(3);

            loadCases.Add(new LoadCase("SDL", LoadCase.LoadCaseType.SuperImposedDeadLoad, Guid.NewGuid()));
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

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseType.LiveLoad, Guid.NewGuid()));
            coefficients.Add(1);

            loadCases.Add(new LoadCase("SW", Guid.NewGuid()));
            coefficients.Add(0.5);

            loadCases.Add(new LoadCase("SDL", LoadCase.LoadCaseType.SuperImposedDeadLoad, Guid.NewGuid()));
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

            loadCases.Add(new LoadCase("Snow", LoadCase.LoadCaseType.Snow, Guid.NewGuid()));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseType.LiveLoad, Guid.NewGuid()));
            coefficients.Add(1);

            loadCases.Add(new LoadCase("SW", LoadCase.LoadCaseType.SelfWeight, Guid.NewGuid()));
            coefficients.Add(0.5);

            LoadCase sdl = new LoadCase("SDL", LoadCase.LoadCaseType.SuperImposedDeadLoad, Guid.NewGuid());
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

            loadCases.Add(new LoadCase("Snow", LoadCase.LoadCaseType.Snow, Guid.NewGuid()));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseType.LiveLoad, Guid.NewGuid()));
            coefficients.Add(1);

            loadCases.Add(new LoadCase("SW", LoadCase.LoadCaseType.SelfWeight, Guid.NewGuid()));
            coefficients.Add(0.5);

            LoadCase sdl = new LoadCase("SDL", LoadCase.LoadCaseType.SuperImposedDeadLoad, Guid.NewGuid());
            loadCases.Add(sdl);
            coefficients.Add(4);
            loadCases.Add(new LoadCase("SDL", LoadCase.LoadCaseType.SuperImposedDeadLoad, Guid.NewGuid()));
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

            loadCases.Add(new LoadCase("Snow", LoadCase.LoadCaseType.Snow));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseType.LiveLoad));
            coefficients.Add(1);

            loadCases.Add(new LoadCase("SW", LoadCase.LoadCaseType.SelfWeight));
            coefficients.Add(0.5);

            LoadCase sdl = new LoadCase("SDL", LoadCase.LoadCaseType.SuperImposedDeadLoad);
            loadCases.Add(sdl);
            coefficients.Add(4);
            loadCases.Add(new LoadCase("SDL", LoadCase.LoadCaseType.SuperImposedDeadLoad));
            coefficients.Add(4);


            CombinationAsce combination = new CombinationAsce("test", CombinationAsce.CombinationType.LFRD);

            combination.AddLoadCaseCoefficients(loadCases, coefficients);

            combination.AddLoadCaseCoefficient(new LoadCase("Zero", LoadCase.LoadCaseType.Earthquake), 0);

            // Act
            string combinationName = combination.ToString();

            // Assert
            var splitted = combinationName.Split(new string[] { "+" }, StringSplitOptions.None);

            Console.WriteLine(combinationName);
            Assert.IsTrue(combination[sdl] == 8, combinationName);
            Assert.IsTrue(combination[new LoadCase("test")] == 0, combinationName);
            Assert.IsTrue(combination[new LoadCase("Zero", LoadCase.LoadCaseType.Earthquake)] == 0, combinationName);
            Assert.IsTrue(splitted[0].Contains("SW"), combinationName);
            Assert.IsFalse(splitted[0].Contains("Zero"), combinationName);
        }

        [TestMethod]
        public void GeneratorUltimateStructural1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
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
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.65) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.78) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateStructural2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
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
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);                         
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateStructural3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryE;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;
            StandardEN1990.ULSStructuralGeotechicalCombinationSets uLSCombinationSets = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetC;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category, uLSCombinationSets, false);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.65) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.78) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateEquilibrium1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
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
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateEquilibrium2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SuperImposedDeadLoad);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
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
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateEquilibrium3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Temperature";
            LoadCase temperatureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Temperature);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

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
        public void GeneratorUltimateFatigue1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
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
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateFatigue2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
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
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateFatigue3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
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
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateGeotechnical1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
                snowLoadCase,
                prestressLoadCase
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
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.75) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateGeotechnical2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateGeotechnical;
            StandardEN1990.ULSStructuralGeotechicalCombinationSets uLSCombinationSets = StandardEN1990.ULSStructuralGeotechicalCombinationSets.SetB;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState,category, uLSCombinationSets);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.5) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 1.05) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateGeotechnical3()
        {
            // Arrange
            string loadCaseName1 = "SuperImposedDeadLoad";
            LoadCase superImposedDeadLoadLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SuperImposedDeadLoad);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Temperature);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                superImposedDeadLoadLoadCase,
                windLoadCase,
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
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(Math.Abs(outList[0][superImposedDeadLoadLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][superImposedDeadLoadLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][superImposedDeadLoadLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[3][superImposedDeadLoadLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[4][superImposedDeadLoadLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[5][superImposedDeadLoadLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.3) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.65) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.78) < 0.001);
        }

        [TestMethod]
        public void GeneratorServiceabilityCharacteristic()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
                snowLoadCase,
                prestressLoadCase
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
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.70) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.60) < 0.001);
        }

        [TestMethod]
        public void GeneratorServiceabilityQuasiPermanent()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
                snowLoadCase,
                prestressLoadCase
            };

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.ServiceabilityQuasiPermanent;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations("combo", loadCaseList, standardEN1990, limitState, category);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 0.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 0.20) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.20) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.00) < 0.001);
        }

        [TestMethod]
        public void GeneratorServiceabilityFrequent()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseType.SelfWeight);
            string loadCaseName2 = "Wind";
            LoadCase windLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseType.Wind);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseType.Snow);
            string loadCaseName4 = "PreStress";
            LoadCase prestressLoadCase = new LoadCase(loadCaseName4, LoadCase.LoadCaseType.Prestress);

            List<LoadCase> loadCaseList = new List<LoadCase>
            {
                selfWeightLoadCase,
                windLoadCase,
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
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 0.20) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 0.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.20) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.00) < 0.001);
        }
    }
}
