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

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateEquilibrium;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetB;
            CombinationEn combination = new CombinationEn("test", standardEN1990, limitState, uLSCombinationSets, category);

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

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateEquilibrium;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetB;
            CombinationEn combination = new CombinationEn("test", standardEN1990, limitState, uLSCombinationSets, category);

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

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetB;
            CombinationEn combination = new CombinationEn("test", standardEN1990, limitState, uLSCombinationSets, category);

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

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateGeotechnical;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetB;
            CombinationEn combination = new CombinationEn("test", standardEN1990, limitState, uLSCombinationSets, category);

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
        public void GeneratorUltimateStructuralSetB()
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

            List<LoadCase> loadCaseList = new List<LoadCase>();
            loadCaseList.Add(selfWeightLoadCase);
            loadCaseList.Add(windLoadCase);
            loadCaseList.Add(snowLoadCase);
            loadCaseList.Add(prestressLoadCase);

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetB;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations(loadCaseList, standardEN1990, limitState, uLSCombinationSets, category);

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
        public void GeneratorUltimateEquilibriumSetB()
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

            List<LoadCase> loadCaseList = new List<LoadCase>();
            loadCaseList.Add(selfWeightLoadCase);
            loadCaseList.Add(windLoadCase);
            loadCaseList.Add(snowLoadCase);
            loadCaseList.Add(prestressLoadCase);

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateEquilibrium;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetB;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations(loadCaseList, standardEN1990, limitState, uLSCombinationSets, category);

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
        public void GeneratorUltimateFatigueSetB()
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

            List<LoadCase> loadCaseList = new List<LoadCase>();
            loadCaseList.Add(selfWeightLoadCase);
            loadCaseList.Add(windLoadCase);
            loadCaseList.Add(snowLoadCase);
            loadCaseList.Add(prestressLoadCase);

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateFatigue;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetB;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations(loadCaseList, standardEN1990, limitState, uLSCombinationSets, category);

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
        public void GeneratorUltimateGeotechnicalSetB()
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

            List<LoadCase> loadCaseList = new List<LoadCase>();
            loadCaseList.Add(selfWeightLoadCase);
            loadCaseList.Add(windLoadCase);
            loadCaseList.Add(snowLoadCase);
            loadCaseList.Add(prestressLoadCase);

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateGeotechnical;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetB;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations(loadCaseList, standardEN1990, limitState, uLSCombinationSets, category);

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
        public void GeneratorUltimateStructuralSetA()
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

            List<LoadCase> loadCaseList = new List<LoadCase>();
            loadCaseList.Add(selfWeightLoadCase);
            loadCaseList.Add(windLoadCase);
            loadCaseList.Add(snowLoadCase);
            loadCaseList.Add(prestressLoadCase);

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryC;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateStructural;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetA;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations(loadCaseList, standardEN1990, limitState, uLSCombinationSets, category);

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
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.75) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
        }
        
        [TestMethod]
        public void GeneratorUltimateEquilibriumSetA()
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

            List<LoadCase> loadCaseList = new List<LoadCase>();
            loadCaseList.Add(selfWeightLoadCase);
            loadCaseList.Add(windLoadCase);
            loadCaseList.Add(snowLoadCase);
            loadCaseList.Add(prestressLoadCase);

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryD;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateEquilibrium;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetA;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations(loadCaseList, standardEN1990, limitState, uLSCombinationSets, category);

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
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.75) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateFatigueSetA()
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

            List<LoadCase> loadCaseList = new List<LoadCase>();
            loadCaseList.Add(selfWeightLoadCase);
            loadCaseList.Add(windLoadCase);
            loadCaseList.Add(snowLoadCase);
            loadCaseList.Add(prestressLoadCase);

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryG;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateFatigue;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetA;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations(loadCaseList, standardEN1990, limitState, uLSCombinationSets, category);

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
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.75) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
        }

        [TestMethod]
        public void GeneratorUltimateGeotechnicalSetA()
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

            List<LoadCase> loadCaseList = new List<LoadCase>();
            loadCaseList.Add(selfWeightLoadCase);
            loadCaseList.Add(windLoadCase);
            loadCaseList.Add(snowLoadCase);
            loadCaseList.Add(prestressLoadCase);

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryF;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.UltimateGeotechnical;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetA;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations(loadCaseList, standardEN1990, limitState, uLSCombinationSets, category);

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
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.75) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.90) < 0.001);
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

            List<LoadCase> loadCaseList = new List<LoadCase>();
            loadCaseList.Add(selfWeightLoadCase);
            loadCaseList.Add(windLoadCase);
            loadCaseList.Add(snowLoadCase);
            loadCaseList.Add(prestressLoadCase);

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.ServiceabilityCharacteristic;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetB;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations(loadCaseList, standardEN1990, limitState, uLSCombinationSets, category);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.50) < 0.001);
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

            List<LoadCase> loadCaseList = new List<LoadCase>();
            loadCaseList.Add(selfWeightLoadCase);
            loadCaseList.Add(windLoadCase);
            loadCaseList.Add(snowLoadCase);
            loadCaseList.Add(prestressLoadCase);

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.ServiceabilityQuasiPermanent;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetB;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations(loadCaseList, standardEN1990, limitState, uLSCombinationSets, category);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 0.60) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 0.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.25) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.36) < 0.001);
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

            List<LoadCase> loadCaseList = new List<LoadCase>();
            loadCaseList.Add(selfWeightLoadCase);
            loadCaseList.Add(windLoadCase);
            loadCaseList.Add(snowLoadCase);
            loadCaseList.Add(prestressLoadCase);

            StandardEN1990 standardEN1990 = new StandardEN1990();
            StandardEN1990.ImposedLoadCategory category = StandardEN1990.ImposedLoadCategory.CategoryA;
            StandardEN1990.LimitState limitState = StandardEN1990.LimitState.ServiceabilityFrequent;
            StandardEN1990.ULSCombinationSets uLSCombinationSets = StandardEN1990.ULSCombinationSets.SetB;

            // Act
            List<CombinationEn> outList = CombinationEn.GenerateCombinations(loadCaseList, standardEN1990, limitState, uLSCombinationSets, category);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][windLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][snowLoadCase] - 1.00) < 0.001);
            Assert.IsTrue(Math.Abs(outList[0][snowLoadCase] - 0.50) < 0.001);
            Assert.IsTrue(Math.Abs(outList[1][windLoadCase] - 0.60) < 0.001);
        }
    }
}
