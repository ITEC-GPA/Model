using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System.Collections.Generic;
using GPC.TestUtilities;
using System.Linq;
using static GPC.Model.Combinations.StandardASCE16;
using static GPC.Model.Combinations.StandardEN1990;

namespace ModelObjectTest
{
    [TestClass]
    public class CombinationTest : UnitTestBase
    {
        #region COMBINATION TEST

        [TestMethod]
        public void CombinationTest1()
        {
            // Arrange
            List<LoadCase> loadCases = new List<LoadCase>();
            List<double> coefficients = new List<double>();

            loadCases.Add(new LoadCase("Snow", LoadCase.LoadCaseTypes.Snow));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseTypes.LiveLoad));
            coefficients.Add(1);

            LoadCase lcSw = new LoadCase("SW", LoadCase.LoadCaseTypes.SelfWeight);
            loadCases.Add(lcSw);
            coefficients.Add(3);

            LoadCase lcSdl = new LoadCase("SDL", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            loadCases.Add(lcSdl);
            coefficients.Add(4);

            Combination combination = new Combination("test");

            combination.AddLoadCaseCoefficients(loadCases, coefficients);

            // Act
            string combinationName = combination.ToString();

            // Assert

            Console.WriteLine(combinationName);

            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == lcSw).Count() == 1);
            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == lcSw).SingleOrDefault().coefficient == 3);
            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == lcSdl).Count() == 1);
            Assert.IsTrue(combination.GetLoadCaseCoefficientsTuple().Where(i => i.loadcase == lcSdl).SingleOrDefault().coefficient == 4);
        }

        [TestMethod]
        public void CombinationTest2()
        {
            // Arrange
            List<LoadCaseBase> loadCases = new List<LoadCaseBase>();
            List<double> coefficients = new List<double>();

            loadCases.Add(new LoadCaseBase("Snow"));
            coefficients.Add(2);

            loadCases.Add(new LoadCase("Live", LoadCase.LoadCaseTypes.LiveLoad, Guid.NewGuid()));
            coefficients.Add(1);

            loadCases.Add(new LoadCaseBase("SW", Guid.NewGuid()));
            coefficients.Add(0.5);

            loadCases.Add(new LoadCase("SDL", LoadCase.LoadCaseTypes.SuperImposedDeadLoad, Guid.NewGuid()));
            coefficients.Add(4);

            Combination combination = new Combination("test");

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

            Combination combination = new Combination("test");

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

            Combination combination = new Combination("test");

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

            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.LFRD);
            Combination combination = new Combination("cmb", options);

            combination.AddLoadCaseCoefficients(loadCases, coefficients);

            combination.AddLoadCaseCoefficient(new LoadCase("Zero", LoadCase.LoadCaseTypes.Earthquake), 0);

            // Act
            string combinationName = combination.ToString();

            // Assert
            var splitted = combinationName.Split(new string[] { "+" }, StringSplitOptions.None);

            Console.WriteLine(combinationName);
            Assert.IsTrue(combination[sdl] == 8, combinationName);
            Assert.IsTrue(combination[new LoadCaseBase("test")] == 0, combinationName);
            Assert.IsTrue(combination[new LoadCase("Zero", LoadCase.LoadCaseTypes.Earthquake)] == 0, combinationName);
            Assert.IsTrue(splitted[0].Contains("SW"), combinationName);
            Assert.IsFalse(splitted[0].Contains("Zero"), combinationName);
        }

        [TestMethod]
        public void CombinationContainsLoadCases()
        {
            // Arrange

            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.LFRD);
            Combination combination = new Combination("cmb", options);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseTypes.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseTypes.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseTypes.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);

            combination.AddLoadCaseCoefficient(lc1, 1);
            combination.AddLoadCaseCoefficient(lc2, 2);
            combination.AddLoadCaseCoefficient(lc4, 4);
            combination.AddLoadCaseCoefficient(lc5, 5);


            // Assert / Act

            Assert.IsTrue(combination.ContainsLoadCases(new List<LoadCase> { lc1 }));
            Assert.IsTrue(combination.ContainsLoadCases(new List<LoadCase> { lc1, lc2 }));
            Assert.IsTrue(combination.ContainsLoadCases(new List<LoadCase> { lc5, lc1 }));
            Assert.IsFalse(combination.ContainsLoadCases(new List<LoadCase> { lc6 }));
            Assert.IsFalse(combination.ContainsLoadCases(new List<LoadCase> { lc6, lc1 }));
            Assert.IsTrue(combination.ContainsLoadCases(new List<LoadCase>()));
        }

        [TestMethod]
        public void CombinationContainsLoadCaseCoefficients()
        {
            // Arrange

            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.LFRD);
            Combination combination = new Combination("cmb", options);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseTypes.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseTypes.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseTypes.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);

            combination.AddLoadCaseCoefficient(lc1, 1);
            combination.AddLoadCaseCoefficient(lc2, 2);
            combination.AddLoadCaseCoefficient(lc4, 4);
            combination.AddLoadCaseCoefficient(lc5, 5);


            // Assert / Act

            Assert.IsTrue(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCaseBase, double>(lc1, 1) }));
            Assert.IsTrue(combination.ContainsLoadCaseCoefficients(new List<(LoadCaseBase, double)> { (lc1, 1) }));

            Assert.IsFalse(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCaseBase, double>(lc1, 2) }));
            Assert.IsFalse(combination.ContainsLoadCaseCoefficients(new List<(LoadCaseBase, double)> { (lc1, 2) }));


            Assert.IsTrue(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCaseBase, double>(lc1, 1),
                                                                           new KeyValuePair<LoadCaseBase, double>(lc2, 2) }));

            Assert.IsFalse(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCaseBase, double>(lc1, 1),
                                                                            new KeyValuePair<LoadCaseBase, double>(lc2, 3) }));


            Assert.IsTrue(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCaseBase, double>(lc1, 1),
                                                                           new KeyValuePair<LoadCaseBase, double>(lc4, 4) }));

            Assert.IsFalse(combination.ContainsLoadCaseCoefficients(new[] { new KeyValuePair<LoadCaseBase, double>(lc6, 1) }));

            Assert.IsFalse(combination.ContainsLoadCaseCoefficients(new KeyValuePair<LoadCaseBase, double>[1]));


        }

        [TestMethod]
        public void CombinationGetLoadCaseCoefficients()
        {
            // Arrange

            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.LFRD);
            Combination combination = new Combination("cmb", options);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseTypes.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseTypes.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseTypes.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);

            combination.AddLoadCaseCoefficient(lc1, 1);
            combination.AddLoadCaseCoefficient(lc2, 2);
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

        #endregion

        #region GENERATE COMBINATIONS EN1990

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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryC, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);           
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.0);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.0);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.3);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.65);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.0);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.0);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.78);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.3);

            // Assert
            Assert.IsTrue(outList.Count() == 3);            
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 3", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.35);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.5);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination2 = new Combination("cmb 4", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.35);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.5);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryE, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(liveLoadLoadCase, 1.30);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 0.78);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.65);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(liveLoadLoadCase, 0.65);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.78);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.30);

            Combination combination3 = new Combination("cmb 3", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(liveLoadLoadCase, 1.30);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.78);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 0.65);

            // Assert
            Assert.IsTrue(outList.Count() == 4);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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
                prestressLoadCase,
                snowLoadCase,
                selfWeightLoadCase,
                WindPressureLoadCase,    
            };

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateEquilibrium, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryD, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 0.90);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.10);
            combination3.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.10);
            combination4.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));
            Assert.IsTrue(outList.Contains(combination4));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateEquilibrium, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryA, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 0.90);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.10);
            combination3.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.10);
            combination4.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));
            Assert.IsTrue(outList.Contains(combination4));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateEquilibrium, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryD, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 0.90);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(temperatureLoadCase, 1.50);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(temperatureLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.10);
            combination3.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(temperatureLoadCase, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.10);
            combination4.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(temperatureLoadCase, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));
            Assert.IsTrue(outList.Contains(combination4));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateFatigue, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryG, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.30);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.91);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.78);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.30);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateFatigue, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            Combination combination3 = new Combination("cmb 1", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.30);
            combination3.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination4 = new Combination("cmb 2", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.30);
            combination4.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));
            Assert.IsTrue(outList.Contains(combination4));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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
                WindPressureLoadCase,
                prestressLoadCase,
                selfWeightLoadCase,
                snowLoadCase,
            };

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateFatigue, ImposedLoadCategories.CategoryG, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination3.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination4.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));
            Assert.IsTrue(outList.Contains(combination4));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateGeotechnical, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryF, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase2, 1.50);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase2, 1.50);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));
            Assert.IsTrue(outList.Contains(combination4));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateGeotechnical, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination3.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase, 1.05);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.35);
            combination4.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));
            Assert.IsTrue(outList.Contains(combination4));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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
            
            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateGeotechnical, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryF, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(superImposedDeadLoadLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 1.30);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.65);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(superImposedDeadLoadLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.78);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 1.3);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.ServiceabilityCharacteristic, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.70);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase, 0.60);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 0.20);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.ServiceabilityQuasiPermanent, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.20);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);

            // Assert
            Assert.IsTrue(outList.Count() == 2);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.ServiceabilityFrequent, ULSStructuralGeotechicalCombinationSets.SetC, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            CombinationContainsLoadCaseCoefficients();
            Combination expComb1 = new Combination("cmb1", options);
            expComb1.AddLoadCaseCoefficient(selfWeightLoadCase, 1);
            expComb1.AddLoadCaseCoefficient(WindPressureLoadCase, 0.2);
            expComb1.AddLoadCaseCoefficient(snowLoadCase, 0.2);
            expComb1.AddLoadCaseCoefficient(prestressLoadCase, 1);

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(snowLoadCase, 0.50);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase, 0.2);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(prestressLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(snowLoadCase, 0.50);

            // Assert
            Assert.IsTrue(outList.Count() == 3);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
        }

        [TestMethod]
        public void ENGeneratorMultyLoadCase()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase1 = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
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
                selfWeightLoadCase1,
                selfWeightLoadCase2,
                WindPressureLoadCase1,
                WindPressureLoadCase2,
                WindPressureLoadCase3,
                WindPressureLoadCase4,
                snowLoadCase1,
                snowLoadCase2
            };

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase2, 1.50);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase3, 1.50);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase4, 1.50);
            combination1.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination1.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase3, 0.90);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase4, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination2.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase2, 1.50);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase3, 1.50);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase4, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination3.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase3, 0.90);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase4, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination4.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));
            Assert.IsTrue(outList.Contains(combination4));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
        }

        [TestMethod]
        public void ENGeneratorMultyLoadCase2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase1 = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
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
                selfWeightLoadCase1,
                selfWeightLoadCase2,
                WindPressureLoadCase1,
                WindPressureLoadCase2,
                temperatureLoadCase3,
                temperatureLoadCase4,
                snowLoadCase1,
                snowLoadCase2
            };

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(WindPressureLoadCase2, 1.50);
            combination1.AddLoadCaseCoefficient(temperatureLoadCase3, 0.90);
            combination1.AddLoadCaseCoefficient(temperatureLoadCase4, 0.90);
            combination1.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination1.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination2.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination2.AddLoadCaseCoefficient(temperatureLoadCase3, 1.50);
            combination2.AddLoadCaseCoefficient(temperatureLoadCase4, 1.50);
            combination2.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination2.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination3.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination3.AddLoadCaseCoefficient(temperatureLoadCase3, 0.90);
            combination3.AddLoadCaseCoefficient(temperatureLoadCase4, 0.90);
            combination3.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase1, 1.50);
            combination4.AddLoadCaseCoefficient(WindPressureLoadCase2, 1.50);
            combination4.AddLoadCaseCoefficient(temperatureLoadCase3, 0.90);
            combination4.AddLoadCaseCoefficient(temperatureLoadCase4, 0.90);
            combination4.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination4.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination5 = new Combination("cmb 5", options);
            combination5.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination5.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.00);
            combination5.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination5.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination5.AddLoadCaseCoefficient(temperatureLoadCase3, 1.50);
            combination5.AddLoadCaseCoefficient(temperatureLoadCase4, 1.50);
            combination5.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination5.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination6 = new Combination("cmb 6", options);
            combination6.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination6.AddLoadCaseCoefficient(selfWeightLoadCase1, 1.35);
            combination6.AddLoadCaseCoefficient(WindPressureLoadCase1, 0.90);
            combination6.AddLoadCaseCoefficient(WindPressureLoadCase2, 0.90);
            combination6.AddLoadCaseCoefficient(temperatureLoadCase3, 0.90);
            combination6.AddLoadCaseCoefficient(temperatureLoadCase4, 0.90);
            combination6.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination6.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 8);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));
            Assert.IsTrue(outList.Contains(combination4));
            Assert.IsTrue(outList.Contains(combination5));
            Assert.IsTrue(outList.Contains(combination6));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(windPressureLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(windPressureLoadCase2, 1.50);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(windSuctionLoadCase1, 1.50);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(windPressureLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(windPressureLoadCase2, 1.50);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(windSuctionLoadCase1, 1.50);          

            // Assert
            Assert.IsTrue(outList.Count() == 6);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));
            Assert.IsTrue(outList.Contains(combination4));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories.CategoryA, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            Combination combination1 = new Combination("cmb 1", options);
            combination1.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination1.AddLoadCaseCoefficient(windPressureLoadCase1, 1.50);
            combination1.AddLoadCaseCoefficient(windPressureLoadCase2, 1.50);
            combination1.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination1.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination1.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination2 = new Combination("cmb 2", options);
            combination2.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination2.AddLoadCaseCoefficient(windSuctionLoadCase1, 1.50);
            combination2.AddLoadCaseCoefficient(windSuctionLoadCase2, 1.50);
            combination2.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination2.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination2.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination3 = new Combination("cmb 3", options);
            combination3.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination3.AddLoadCaseCoefficient(windPressureLoadCase1, 0.90);
            combination3.AddLoadCaseCoefficient(windPressureLoadCase2, 0.90);
            combination3.AddLoadCaseCoefficient(temperatureLoadCase1, 1.50);
            combination3.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination3.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination4 = new Combination("cmb 4", options);
            combination4.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination4.AddLoadCaseCoefficient(windSuctionLoadCase1, 0.90);
            combination4.AddLoadCaseCoefficient(windSuctionLoadCase2, 0.90);
            combination4.AddLoadCaseCoefficient(temperatureLoadCase1, 1.50);
            combination4.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination4.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination5 = new Combination("cmb 5", options);
            combination5.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination5.AddLoadCaseCoefficient(windPressureLoadCase1, 0.90);
            combination5.AddLoadCaseCoefficient(windPressureLoadCase2, 0.90);
            combination5.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination5.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination5.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            Combination combination6 = new Combination("cmb 6", options);
            combination6.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination6.AddLoadCaseCoefficient(windSuctionLoadCase1, 0.90);
            combination6.AddLoadCaseCoefficient(windSuctionLoadCase2, 0.90);
            combination6.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination6.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination6.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            Combination combination7 = new Combination("cmb 7", options);
            combination7.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination7.AddLoadCaseCoefficient(windPressureLoadCase1, 1.50);
            combination7.AddLoadCaseCoefficient(windPressureLoadCase2, 1.50);
            combination7.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination7.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination7.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination8 = new Combination("cmb 8", options);
            combination8.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination8.AddLoadCaseCoefficient(windSuctionLoadCase1, 1.50);
            combination8.AddLoadCaseCoefficient(windSuctionLoadCase2, 1.50);
            combination8.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination8.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination8.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination9 = new Combination("cmb 9", options);
            combination9.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination9.AddLoadCaseCoefficient(windPressureLoadCase1, 0.90);
            combination9.AddLoadCaseCoefficient(windPressureLoadCase2, 0.90);
            combination9.AddLoadCaseCoefficient(temperatureLoadCase1, 1.50);
            combination9.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination9.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination10 = new Combination("cmb 10", options);
            combination10.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination10.AddLoadCaseCoefficient(windSuctionLoadCase1, 0.90);
            combination10.AddLoadCaseCoefficient(windSuctionLoadCase2, 0.90);
            combination10.AddLoadCaseCoefficient(temperatureLoadCase1, 1.50);
            combination10.AddLoadCaseCoefficient(snowLoadCase1, 1.05);
            combination10.AddLoadCaseCoefficient(snowLoadCase2, 1.05);

            Combination combination11 = new Combination("cmb 11", options);
            combination11.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination11.AddLoadCaseCoefficient(windPressureLoadCase1, 0.90);
            combination11.AddLoadCaseCoefficient(windPressureLoadCase2, 0.90);
            combination11.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination11.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination11.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            Combination combination12 = new Combination("cmb 12", options);
            combination12.AddLoadCaseCoefficient(selfWeightLoadCase, 1.00);
            combination12.AddLoadCaseCoefficient(windSuctionLoadCase1, 0.90);
            combination12.AddLoadCaseCoefficient(windSuctionLoadCase2, 0.90);
            combination12.AddLoadCaseCoefficient(temperatureLoadCase1, 0.90);
            combination12.AddLoadCaseCoefficient(snowLoadCase1, 1.50);
            combination12.AddLoadCaseCoefficient(snowLoadCase2, 1.50);

            // Assert
            Assert.IsTrue(outList.Count() == 14);
            Assert.IsTrue(outList.Contains(combination1));
            Assert.IsTrue(outList.Contains(combination2));
            Assert.IsTrue(outList.Contains(combination3));
            Assert.IsTrue(outList.Contains(combination4));
            Assert.IsTrue(outList.Contains(combination5));
            Assert.IsTrue(outList.Contains(combination6));
            Assert.IsTrue(outList.Contains(combination7));
            Assert.IsTrue(outList.Contains(combination8));
            Assert.IsTrue(outList.Contains(combination9));
            Assert.IsTrue(outList.Contains(combination10));
            Assert.IsTrue(outList.Contains(combination11));
            Assert.IsTrue(outList.Contains(combination12));

            foreach (Combination combination in outList)
            {
                string combinationName = combination.ToString();
                Console.WriteLine(combinationName);
            }

        }

        #endregion

        #region COMBINATION GENERATIONS EN 16612

        [TestMethod]
        public void EN16612GeneratorClimate1()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName6 = "ClimateSummerDeltaT";
            ClimateLoadCase climateSummerDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName6, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);
            string loadCaseName7 = "ClimateSummerDeltaH";
            ClimateLoadCase climateSummerDeltaHLoadCase1 = new ClimateLoadCase(loadCaseName7, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName3 = "ClimateSummerDeltaP";
            ClimateLoadCase climateSummerDeltaPLoadCase2 = new ClimateLoadCase(loadCaseName3, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                climateSummerDeltaTLoadCase1,
                climateSummerDeltaHLoadCase1,
                climateSummerDeltaPLoadCase2
            };

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB,
                ImposedLoadCategories.CategoryA, true);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 8);



            //Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[7][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);        
            //Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
        }
        [TestMethod]
        public void EN16612GeneratorClimate2()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "ClimateSummerDeltaT";
            ClimateLoadCase climateSummerDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName2, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);
            string loadCaseName3 = "ClimateSummerDeltaH";
            ClimateLoadCase climateSummerDeltaHLoadCase1 = new ClimateLoadCase(loadCaseName3, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName4 = "ClimateSummerDeltaP";
            ClimateLoadCase climateSummerDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName4, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName5 = "ClimateSummerDeltaP2";
            ClimateLoadCase climateSummerDeltaPLoadCase2 = new ClimateLoadCase(loadCaseName5, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName6 = "ClimateSummerDeltaP3";
            ClimateLoadCase climateSummerDeltaPLoadCase3 = new ClimateLoadCase(loadCaseName6, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                climateSummerDeltaTLoadCase1,
                climateSummerDeltaHLoadCase1,
                climateSummerDeltaPLoadCase1,
                climateSummerDeltaPLoadCase2,
                climateSummerDeltaPLoadCase3
            };

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB,
                ImposedLoadCategories.CategoryA, true);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 8);
            //Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[7][selfWeightLoadCase] - 1.35) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
        }

        [TestMethod]
        public void EN16612GeneratorClimate3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "ClimateSummerDeltaT";
            ClimateLoadCase climateSummerDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName2, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);
            string loadCaseName3 = "ClimateSummerDeltaH";
            ClimateLoadCase climateSummerDeltaHLoadCase1 = new ClimateLoadCase(loadCaseName3, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName4 = "ClimateSummerDeltaP1";
            ClimateLoadCase climateSummerDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName4, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName5 = "ClimateSummerDeltaP2";
            ClimateLoadCase climateSummerDeltaPLoadCase2 = new ClimateLoadCase(loadCaseName5, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName6 = "ClimateSummerDeltaP3";
            ClimateLoadCase climateSummerDeltaPLoadCase3 = new ClimateLoadCase(loadCaseName6, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName7 = "ClimateWinterDeltaP1";
            ClimateLoadCase climateWinterDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName7, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName8 = "ClimateWinterDeltaP2";
            ClimateLoadCase climateWinterDeltaPLoadCase2 = new ClimateLoadCase(loadCaseName8, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB,
                ImposedLoadCategories.CategoryA, true);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 10);
            //Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[7][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[8][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[9][selfWeightLoadCase] - 1.35) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[8][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[2][climateWinterDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][climateWinterDeltaPLoadCase2] - 1.50) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[5][climateWinterDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[5][climateWinterDeltaPLoadCase2] - 1.50) < 0.001);
        }

        [TestMethod]
        public void EN16612GeneratorClimate4()
        {
            // Arrange
            string loadCaseName1 = "selfWeight";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "ClimateSummerDeltaT";
            ClimateLoadCase climateSummerDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName2, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);
            string loadCaseName3 = "ClimateSummerDeltaH";
            ClimateLoadCase climateSummerDeltaHLoadCase1 = new ClimateLoadCase(loadCaseName3, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName4 = "ClimateSummerDeltaP1";
            ClimateLoadCase climateSummerDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName4, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName5 = "ClimateSummerDeltaP2";
            ClimateLoadCase climateSummerDeltaPLoadCase2 = new ClimateLoadCase(loadCaseName5, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName6 = "ClimateSummerDeltaP3";
            ClimateLoadCase climateSummerDeltaPLoadCase3 = new ClimateLoadCase(loadCaseName6, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName7 = "ClimateWinterDeltaP1";
            ClimateLoadCase climateWinterDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName7, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName8 = "ClimateWinterDeltaP2";
            ClimateLoadCase climateWinterDeltaPLoadCase2 = new ClimateLoadCase(loadCaseName8, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName9 = "Wind1";
            LoadCase windLoadCase1 = new LoadCase(loadCaseName9, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName10 = "Wind2";
            LoadCase windLoadCase2 = new LoadCase(loadCaseName10, LoadCase.LoadCaseTypes.WindPressure);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB,
                ImposedLoadCategories.CategoryA, true);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 16);
            //Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[5][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[7][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[8][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[9][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[10][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[11][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[12][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[13][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[14][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[15][selfWeightLoadCase] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaHLoadCase1] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[10][climateSummerDeltaHLoadCase1] - 1.35) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][windLoadCase1] - 0.90) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][windLoadCase2] - 0.90) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][windLoadCase1] - 0.90) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][windLoadCase2] - 0.90) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[2][climateWinterDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][climateWinterDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][windLoadCase1] - 0.90) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][windLoadCase2] - 0.90) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[3][windLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][windLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][climateWinterDeltaPLoadCase1] - 0.45) < 0.001);            // valore da modificare quando cambieranno gli psi dei vetri
            //Assert.IsTrue(Math.Abs(outList[3][climateWinterDeltaPLoadCase2] - 0.45) < 0.001);            // valore da modificare quando cambieranno gli psi dei vetri

            //Assert.IsTrue(Math.Abs(outList[4][windLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][windLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaTLoadCase1] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase1] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase2] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[4][climateSummerDeltaPLoadCase3] - 0.45) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[5][windLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[5][windLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[5][climateSummerDeltaTLoadCase1] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[5][climateSummerDeltaPLoadCase1] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[5][climateSummerDeltaPLoadCase2] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[5][climateSummerDeltaPLoadCase3] - 0.45) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][windLoadCase1] - 0.90) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[6][windLoadCase2] - 0.90) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[7][climateSummerDeltaTLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[7][climateSummerDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[7][climateSummerDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[7][climateSummerDeltaPLoadCase3] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[7][windLoadCase1] - 0.90) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[7][windLoadCase2] - 0.90) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[8][climateWinterDeltaPLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[8][climateWinterDeltaPLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[8][windLoadCase1] - 0.90) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[8][windLoadCase2] - 0.90) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[9][windLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[9][windLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[9][climateWinterDeltaPLoadCase1] - 0.45) < 0.001);            // valore da modificare quando cambieranno gli psi dei vetri
            //Assert.IsTrue(Math.Abs(outList[9][climateWinterDeltaPLoadCase2] - 0.45) < 0.001);            // valore da modificare quando cambieranno gli psi dei vetri

            //Assert.IsTrue(Math.Abs(outList[10][windLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[10][windLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[10][climateSummerDeltaTLoadCase1] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[10][climateSummerDeltaPLoadCase1] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[10][climateSummerDeltaPLoadCase2] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[10][climateSummerDeltaPLoadCase3] - 0.45) < 0.001);

            //Assert.IsTrue(Math.Abs(outList[11][windLoadCase1] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[11][windLoadCase2] - 1.50) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[11][climateSummerDeltaTLoadCase1] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[11][climateSummerDeltaPLoadCase1] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[11][climateSummerDeltaPLoadCase2] - 0.45) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[11][climateSummerDeltaPLoadCase3] - 0.45) < 0.001);

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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateSeismic, ULSStructuralGeotechicalCombinationSets.SetC,
                ImposedLoadCategories.CategoryC, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 2);
            //Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][prestressLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][prestressLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][seismicLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][seismicLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase] - 0.60) < 0.001);
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

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateSeismic, ULSStructuralGeotechicalCombinationSets.SetC,
                ImposedLoadCategories.CategoryC, false);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 2);
            //Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][prestressLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][prestressLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][seismicLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][seismicLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase1] - 0.60) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase2] - 0.60) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase3] - 0.60) < 0.001);
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
            string loadCaseName5 = "Seismic";
            LoadCase seismicLoadCase = new LoadCase(loadCaseName5, LoadCase.LoadCaseTypes.Earthquake);
            string loadCaseName7 = "LiveLoad2";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad3";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                seismicLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
            };

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateSeismic, ULSStructuralGeotechicalCombinationSets.SetC,
                ImposedLoadCategories.CategoryC, true);
            StandardEN1990 standardEN1990 = new StandardEN1990();

            // Act
            CombinationsCollection outList = standardEN1990.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 2);
            //Assert.IsTrue(Math.Abs(outList[0][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][selfWeightLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][climateSummerDeltaHLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][climateSummerDeltaHLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][seismicLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][seismicLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[2][seismicLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[3][seismicLoadCase] - 1.00) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase1] - 0.60) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[0][liveLoadLoadCase2] - 0.60) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][liveLoadLoadCase1] - 0.60) < 0.001);
            //Assert.IsTrue(Math.Abs(outList[1][liveLoadLoadCase2] - 0.60) < 0.001);
        }

        [TestMethod]
        public void EN16612GeneratorAGC_ULS1()
        {
            // Arrange
            string loadCaseName1 = "Dead Load";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);

            string loadCaseName3 = "ClimateSummerDeltaH";
            ClimateLoadCase climateSummerDeltaHLoadCase = new ClimateLoadCase(loadCaseName3, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName4 = "ClimateSummerDeltaP";
            ClimateLoadCase climateSummerDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName4, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName5 = "ClimateSummerDeltaT";
            ClimateLoadCase climateSummerDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName5, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);
            string loadCaseName6 = "ClimateWinterDeltaH";
            ClimateLoadCase climateWinterDeltaHLoadCase = new ClimateLoadCase(loadCaseName6, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName7 = "ClimateWinterDeltaP";
            ClimateLoadCase climateWinterDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName7, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName8 = "ClimateWinterDeltaT";
            ClimateLoadCase climateWinterDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName8, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                climateSummerDeltaHLoadCase,
                climateSummerDeltaTLoadCase1,
                climateSummerDeltaPLoadCase1,
                climateWinterDeltaHLoadCase,
                climateWinterDeltaPLoadCase1,
                climateWinterDeltaTLoadCase1,

            };

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB,
                ImposedLoadCategories.CategoryC, false);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 22);
            foreach (Combination combo in outList)
            {
                Console.WriteLine(combo.ToString());
            }
        }

        [TestMethod]
        public void EN16612GeneratorAGC_ULS2()
        {
            // Arrange
            string loadCaseName1 = "Dead Load";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);

            string loadCaseName3 = "ClimateSummerDeltaH";
            ClimateLoadCase climateSummerDeltaHLoadCase = new ClimateLoadCase(loadCaseName3, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName4 = "ClimateSummerDeltaP";
            ClimateLoadCase climateSummerDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName4, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName5 = "ClimateSummerDeltaT";
            ClimateLoadCase climateSummerDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName5, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);
            string loadCaseName6 = "ClimateWinterDeltaH";
            ClimateLoadCase climateWinterDeltaHLoadCase = new ClimateLoadCase(loadCaseName6, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName7 = "ClimateWinterDeltaP";
            ClimateLoadCase climateWinterDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName7, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName8 = "ClimateWinterDeltaT";
            ClimateLoadCase climateWinterDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName8, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);

            string loadCaseName9 = "LiveLoad";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName9, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                climateSummerDeltaHLoadCase,
                climateSummerDeltaTLoadCase1,
                climateSummerDeltaPLoadCase1,
                climateWinterDeltaHLoadCase,
                climateWinterDeltaPLoadCase1,
                climateWinterDeltaTLoadCase1,
                liveLoadLoadCase1,

            };

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.UltimateStructural, ULSStructuralGeotechicalCombinationSets.SetB,
                ImposedLoadCategories.CategoryC, false);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 22);
            foreach (Combination combo in outList)
            {
                Console.WriteLine(combo.ToString());
            }
        }

        [TestMethod]
        public void EN16612GeneratorAGC_SLS1()
        {
            // Arrange
            string loadCaseName1 = "Dead Load";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);

            string loadCaseName3 = "ClimateSummerDeltaH";
            ClimateLoadCase climateSummerDeltaHLoadCase = new ClimateLoadCase(loadCaseName3, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName4 = "ClimateSummerDeltaP";
            ClimateLoadCase climateSummerDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName4, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName5 = "ClimateSummerDeltaT";
            ClimateLoadCase climateSummerDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName5, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);
            string loadCaseName6 = "ClimateWinterDeltaH";
            ClimateLoadCase climateWinterDeltaHLoadCase = new ClimateLoadCase(loadCaseName6, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName7 = "ClimateWinterDeltaP";
            ClimateLoadCase climateWinterDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName7, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName8 = "ClimateWinterDeltaT";
            ClimateLoadCase climateWinterDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName8, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                climateSummerDeltaHLoadCase,
                climateSummerDeltaTLoadCase1,
                climateSummerDeltaPLoadCase1,
                climateWinterDeltaHLoadCase,
                climateWinterDeltaPLoadCase1,
                climateWinterDeltaTLoadCase1,

            };

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.ServiceabilityCharacteristic, ULSStructuralGeotechicalCombinationSets.SetB,
                ImposedLoadCategories.CategoryC, false);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 22);
            foreach (Combination combo in outList)
            {
                Console.WriteLine(combo.ToString());
            }
        }

        [TestMethod]
        public void EN16612GeneratorAGC_SLS2()
        {
            // Arrange
            string loadCaseName1 = "Dead Load";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);

            string loadCaseName3 = "ClimateSummerDeltaH";
            ClimateLoadCase climateSummerDeltaHLoadCase = new ClimateLoadCase(loadCaseName3, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName4 = "ClimateSummerDeltaP";
            ClimateLoadCase climateSummerDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName4, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName5 = "ClimateSummerDeltaT";
            ClimateLoadCase climateSummerDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName5, ClimateLoadCase.Seasons.Summer, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);
            string loadCaseName6 = "ClimateWinterDeltaH";
            ClimateLoadCase climateWinterDeltaHLoadCase = new ClimateLoadCase(loadCaseName6, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaH, 100, 100);
            string loadCaseName7 = "ClimateWinterDeltaP";
            ClimateLoadCase climateWinterDeltaPLoadCase1 = new ClimateLoadCase(loadCaseName7, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaP, 100, 100);
            string loadCaseName8 = "ClimateWinterDeltaT";
            ClimateLoadCase climateWinterDeltaTLoadCase1 = new ClimateLoadCase(loadCaseName8, ClimateLoadCase.Seasons.Winter, ClimateLoadCase.ClimateTypes.DeltaT, 100, 100);

            string loadCaseName9 = "LiveLoad";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName9, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                climateSummerDeltaHLoadCase,
                climateSummerDeltaTLoadCase1,
                climateSummerDeltaPLoadCase1,
                climateWinterDeltaHLoadCase,
                climateWinterDeltaPLoadCase1,
                climateWinterDeltaTLoadCase1,
                liveLoadLoadCase1,

            };

            En1990CombinationsOptions options = new En1990CombinationsOptions(StandardEN1990.LimitStates.ServiceabilityCharacteristic, ULSStructuralGeotechicalCombinationSets.SetB,
                ImposedLoadCategories.CategoryC, false);
            StandardEN16612 standardEN16612 = new StandardEN16612();

            // Act
            CombinationsCollection outList = standardEN16612.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 22);
            foreach (Combination combo in outList)
            {
                Console.WriteLine(combo.ToString());
            }
        }

        #endregion

        #region ASCE16

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
            string loadCaseName7 = "LiveLoad1";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad2";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
            };

            StandardASCE16 standardASCE16 = new StandardASCE16();
            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.LFRD);

            // Act
            CombinationsCollection outList = standardASCE16.CreateCombinations(loadCaseList.ToArray(), options);

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
            string loadCaseName7 = "LiveLoad1";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad2";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName9 = "selfWeight2";
            LoadCase selfWeightLoadCase2 = new LoadCase(loadCaseName9, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName10 = "Earthquake";
            LoadCase earthquakeLoadCase = new LoadCase(loadCaseName10, LoadCase.LoadCaseTypes.Earthquake);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
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
            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.LFRD);

            // Act
            CombinationsCollection outList = standardASCE16.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 12);
        }

        [TestMethod]
        public void ASCEGeneratorLFRD3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight1";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "selfWeight2";
            LoadCase selfWeightLoadCase2 = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName7 = "LiveLoad1";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad2";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                selfWeightLoadCase2,
                snowLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
            };

            StandardASCE16 standardASCE16 = new StandardASCE16();
            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.LFRD);

            // Act
            CombinationsCollection outList = standardASCE16.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 8);
        }

        [TestMethod]
        public void ASCEGeneratorLFRD4()
        {
            // Arrange
            string loadCaseName1 = "selfWeight1";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure1";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName2_2 = "WindPressure2";
            LoadCase WindPressureLoadCase2 = new LoadCase(loadCaseName2_2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName2_3 = "WindSuction1";
            LoadCase WindSuctionLoadCase = new LoadCase(loadCaseName2_3, LoadCase.LoadCaseTypes.WindSuction);
            string loadCaseName3 = "Snow1";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName7 = "LiveLoad1";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad2";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName9 = "selfWeight2";
            LoadCase selfWeightLoadCase2 = new LoadCase(loadCaseName9, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName10 = "Earthquake";
            LoadCase earthquakeLoadCase = new LoadCase(loadCaseName10, LoadCase.LoadCaseTypes.Earthquake);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                selfWeightLoadCase2,
                WindPressureLoadCase,
                WindPressureLoadCase2,
                WindSuctionLoadCase,
                snowLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
                earthquakeLoadCase,
            };

            StandardASCE16 standardASCE16 = new StandardASCE16();
            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.ASD);

            // Act
            CombinationsCollection outList = standardASCE16.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 13);
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

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                WindPressureLoadCase,
                snowLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
            };

            StandardASCE16 standardASCE16 = new StandardASCE16();
            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.ASD);

            // Act
            CombinationsCollection outList = standardASCE16.CreateCombinations(loadCaseList.ToArray(), options);

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

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                superImposedLoadCase,
                snowLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
            };

            StandardASCE16 standardASCE16 = new StandardASCE16();
            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.ASD);

            // Act
            CombinationsCollection outList = standardASCE16.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 7);
        }

        [TestMethod]
        public void ASCEGeneratorASD3()
        {
            // Arrange
            string loadCaseName1 = "selfWeight1";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "selfWeight2";
            LoadCase selfWeightLoadCase2 = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName3 = "Snow";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName7 = "LiveLoad1";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad2";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                selfWeightLoadCase2,
                snowLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
            };

            StandardASCE16 standardASCE16 = new StandardASCE16();
            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.ASD);

            // Act
            CombinationsCollection outList = standardASCE16.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 7);
        }

        [TestMethod]
        public void ASCEGeneratorASD4()
        {
            // Arrange
            string loadCaseName1 = "selfWeight1";
            LoadCase selfWeightLoadCase = new LoadCase(loadCaseName1, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName2 = "WindPressure1";
            LoadCase WindPressureLoadCase = new LoadCase(loadCaseName2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName2_2 = "WindPressure2";
            LoadCase WindPressureLoadCase2 = new LoadCase(loadCaseName2_2, LoadCase.LoadCaseTypes.WindPressure);
            string loadCaseName2_3 = "WindSuction1";
            LoadCase WindSuctionLoadCase = new LoadCase(loadCaseName2_3, LoadCase.LoadCaseTypes.WindSuction);
            string loadCaseName3 = "Snow1";
            LoadCase snowLoadCase = new LoadCase(loadCaseName3, LoadCase.LoadCaseTypes.Snow);
            string loadCaseName7 = "LiveLoad1";
            LoadCase liveLoadLoadCase1 = new LoadCase(loadCaseName7, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName8 = "LiveLoad2";
            LoadCase liveLoadLoadCase2 = new LoadCase(loadCaseName8, LoadCase.LoadCaseTypes.LiveLoad);
            string loadCaseName9 = "selfWeight2";
            LoadCase selfWeightLoadCase2 = new LoadCase(loadCaseName9, LoadCase.LoadCaseTypes.SelfWeight);
            string loadCaseName10 = "Earthquake";
            LoadCase earthquakeLoadCase = new LoadCase(loadCaseName10, LoadCase.LoadCaseTypes.Earthquake);

            List<LoadCaseBase> loadCaseList = new List<LoadCaseBase>
            {
                selfWeightLoadCase,
                selfWeightLoadCase2,
                WindPressureLoadCase,
                WindPressureLoadCase2,
                WindSuctionLoadCase,
                snowLoadCase,
                liveLoadLoadCase1,
                liveLoadLoadCase2,
                earthquakeLoadCase,
            };

            StandardASCE16 standardASCE16 = new StandardASCE16();
            ASCE16CombinationsOptions options = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.LFRD);

            // Act
            CombinationsCollection outList = standardASCE16.CreateCombinations(loadCaseList.ToArray(), options);

            // Assert
            Assert.IsTrue(outList.Count() == 15);
        }

        #endregion

        #region GENERIC TEST

        [TestMethod]
        public void EqualsHashCode()
        {
            // Arrange

            ASCE16CombinationsOptions options1 = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.LFRD);
            ASCE16CombinationsOptions options2 = new ASCE16CombinationsOptions(StandardASCE16.LimitStates.ASD);

            Combination combination1 = new Combination("cmb1", options1);
            Combination combination2 = new Combination("cmb1", options1);
            Combination combination3 = new Combination("cmb1", options2);
            Combination combination4 = new Combination("cmb1", options1);
            Combination combination5 = new Combination("cmb1", options1);

            var lc1 = new LoadCase("LC1", LoadCase.LoadCaseTypes.SelfWeight);
            var lc2 = new LoadCase("LC2", LoadCase.LoadCaseTypes.SuperImposedDeadLoad);
            var lc4 = new LoadCase("LC4", LoadCase.LoadCaseTypes.Maintenance);
            var lc5 = new LoadCase("LC5", LoadCase.LoadCaseTypes.LiveLoad);
            var lc6 = new LoadCase("LC6", LoadCase.LoadCaseTypes.Snow);

            combination1.AddLoadCaseCoefficient(lc1, 1);
            combination1.AddLoadCaseCoefficient(lc2, 2);
            combination1.AddLoadCaseCoefficient(lc4, 4);
            combination1.AddLoadCaseCoefficient(lc5, 5);

            combination2.AddLoadCaseCoefficient(lc1, 1);
            combination2.AddLoadCaseCoefficient(lc2, 2);
            combination2.AddLoadCaseCoefficient(lc4, 4);
            combination2.AddLoadCaseCoefficient(lc5, 5);

            combination3.AddLoadCaseCoefficient(lc1, 1);
            combination3.AddLoadCaseCoefficient(lc2, 2);
            combination3.AddLoadCaseCoefficient(lc4, 4);
            combination3.AddLoadCaseCoefficient(lc5, 5);

            combination4.AddLoadCaseCoefficient(lc1, 1);
            combination4.AddLoadCaseCoefficient(lc2, 2);
            combination4.AddLoadCaseCoefficient(lc4, 4);
            combination4.AddLoadCaseCoefficient(lc5, 5);
            combination4.AddLoadCaseCoefficient(lc6, 5);

            combination5.AddLoadCaseCoefficient(lc1, 1);
            combination5.AddLoadCaseCoefficient(lc2, 2);
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

        #endregion
    }
}